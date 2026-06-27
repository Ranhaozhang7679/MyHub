using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Luster.PreviewHost
{
    /// <summary>命令行入口:解析参数 → 加载主题 → 解析类型 → 渲染 → 写 PNG</summary>
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            var opts = ParseArgs(args);
            if (opts == null)
            {
                PrintUsage();
                return 1;
            }
            try
            {
                // 启动 WPF Application 以加载 App.xaml 主题。
                // 注意:StartupObject=Program 时,生成的 App.Main 不被调用,
                // 故必须显式调用 InitializeComponent() 触发 LoadComponent 加载合并字典;
                // 仅 new App() 不会加载 App.xaml 资源(由 App.g.cs 验证)。
                // app 必须声明为 App 类型,否则按 Application 静态类型解析不到生成的 InitializeComponent。
                App app = System.Windows.Application.Current as App ?? new App();
                app.InitializeComponent();

                // 可选:预加载外部程序集,使后续类型解析能命中外部 View/VM
                if (!string.IsNullOrEmpty(opts.Assembly) && File.Exists(opts.Assembly))
                    Assembly.LoadFrom(opts.Assembly);

                // 解析 --xaml 拿 d:DesignInstance(若未显式给 --designvm)
                string designVm = opts.DesignVm;
                bool designDataMissing = false;
                if (string.IsNullOrEmpty(designVm) && !string.IsNullOrEmpty(opts.XamlPath) && File.Exists(opts.XamlPath))
                {
                    var info = DesignInstanceParser.Parse(File.ReadAllText(opts.XamlPath));
                    // 源 XAML 里 d:DesignInstance 多为 local: 别名,运行时无法解析全名 → 视为 missing 提示
                    if (info == null || info.TypeName.Contains(":"))
                    {
                        designDataMissing = true;
                    }
                    else
                    {
                        designVm = info.TypeName;
                    }
                }

                // 解析 View 类型(兜底遍历已加载程序集,规避外部程序集 Type.GetType 解析不到)
                var viewType = ResolveType(opts.View);
                if (viewType == null)
                {
                    Console.Error.WriteLine("PreviewHost 找不到 View 类型: " + opts.View);
                    return 1;
                }

                // 解析 DesignVm 类型(可选;解析失败不阻塞渲染,仅标记 missing)
                string designVmAqn = null;
                if (!string.IsNullOrEmpty(designVm))
                {
                    var vmType = ResolveType(designVm);
                    if (vmType != null)
                        designVmAqn = vmType.AssemblyQualifiedName;
                    else
                        designDataMissing = true;
                }

                var req = new RenderRequest
                {
                    ViewTypeName = viewType.AssemblyQualifiedName,
                    AssemblyPath = opts.Assembly,
                    DesignVmTypeName = designVmAqn,
                    Width = opts.Width,
                    Height = opts.Height
                };
                var result = ViewRenderer.Render(req);
                if (!result.Success)
                {
                    Console.Error.WriteLine("PreviewHost 渲染失败: " + result.Error);
                    return 1;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(opts.Out)));
                File.WriteAllBytes(opts.Out, result.PngBytes);
                string warning = !result.DesignDataPresent
                    ? (designDataMissing
                        ? " [警告: 无设计时数据(d:DesignInstance 别名不可解析或 VM 类型未找到)]"
                        : " [警告: 无设计时数据]")
                    : "";
                Console.WriteLine("已截图: " + opts.Out + warning);
                return 0;
            }
            catch (System.Windows.Markup.XamlParseException ex)
            {
                // 主题(资源字典)加载失败:WPF 抛 XamlParseException(注意:非 System.Xaml.XamlException,
                // 二者无继承关系)。退出码 3 与渲染失败(1)区分。
                Console.Error.WriteLine("主题加载失败: " + ex.Message +
                    (ex.InnerException != null ? " -> " + ex.InnerException.Message : ""));
                return 3;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("PreviewHost 异常: " + ex.GetType().Name + ": " + ex.Message +
                    (ex.InnerException != null ? " -> " + ex.InnerException.Message : ""));
                return 1;
            }
        }

        private sealed class Options
        {
            public string View; public string Assembly; public string XamlPath;
            public string DesignVm; public string Out;
            public int Width = 1920; public int Height = 1080;
        }

        private static Options ParseArgs(string[] args)
        {
            var map = new Dictionary<string, string>();
            for (int i = 0; i + 1 < args.Length; i += 2)
                map[args[i].TrimStart('-').ToLowerInvariant()] = args[i + 1];
            if (!map.ContainsKey("view") || !map.ContainsKey("out")) return null;
            var o = new Options { View = map["view"], Out = map["out"] };
            if (map.TryGetValue("assembly", out var a)) o.Assembly = a;
            if (map.TryGetValue("xaml", out var x)) o.XamlPath = x;
            if (map.TryGetValue("designvm", out var d)) o.DesignVm = d;
            if (map.TryGetValue("width", out var w) && int.TryParse(w, out var wi)) o.Width = wi;
            if (map.TryGetValue("height", out var h) && int.TryParse(h, out var hi)) o.Height = hi;
            return o;
        }

        private static void PrintUsage()
        {
            Console.Error.WriteLine(
                "用法: PreviewHost --view <类型全名[,程序集]> --out <png> " +
                "[--assembly <dll>] [--xaml <src.xaml>] [--designvm <全名[,程序集]>] " +
                "[--width 1920] [--height 1080]");
        }

        /// <summary>
        /// 解析类型:先 Type.GetType(支持 AQN / "Full,Asm" 短形式 / 调用方程序集内的全名),
        /// 失败则取不含程序集部分的全名,遍历已加载程序集兜底查找。
        /// 用于规避外部程序集(Assembly.LoadFrom 加载)类型 Type.GetType 解析不到的问题:
        /// Type.GetType(bareFullName) 只搜调用方程序集 + mscorlib,不搜 LoadFrom 加载的程序集。
        /// </summary>
        private static Type ResolveType(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var t = Type.GetType(name);
            if (t != null) return t;
            // 取不含程序集部分的全名,遍历已加载程序集查找
            string bare = name;
            int comma = name.IndexOf(',');
            if (comma >= 0) bare = name.Substring(0, comma).Trim();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(bare);
                if (t != null) return t;
            }
            return null;
        }
    }
}
