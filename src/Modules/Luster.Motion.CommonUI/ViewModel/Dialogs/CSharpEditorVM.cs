using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Control.Wpf.Motion.Editors;
using Luster.Motion.CommonUI.ViewModel;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class CSharpEditorVM : MotionDialogVM
    {
        ICommonBus commonBus = null;
        private List<MetadataReference> _references = new List<MetadataReference>();


        #region 字段和属性

        private string _CodeEditorText = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using Prism.Modularity;
using Luster.Motion.EditorUI;
using System.ComponentModel;
using Luster.Motion.DataStruct.Network;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.SimDevice.Engine;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.DataModels;
using System.Threading;

namespace LusterMotionScript
{
    public class UserScript
    {

        private FlowBus _eventBus;

        private IDeviceEngine _deviceEngine;

        // 无参构造函数
        public UserScript()
        {
            // 通过 Prism 容器获取单例服务
            _eventBus = Prism.Ioc.ContainerLocator.Container.Resolve<FlowBus>();
            _deviceEngine = Prism.Ioc.ContainerLocator.Container.Resolve<IDeviceEngine>();
        }

        public UserScript(FlowBus flowBus,IDeviceEngine deviceEngine)
        {
            _eventBus = flowBus;
            _deviceEngine = deviceEngine;
        }

        public void Initial()
        {
            System.Diagnostics.Debug.WriteLine(""Script Initial"");
        }

        public void Run()
        {
            System.Diagnostics.Debug.WriteLine(""Script Run"");

            //拿到所有全局变量并遍历
            var id = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
            var Globalmd = _eventBus.GetModule(id);
            foreach (var item in Globalmd.Parameters)
            {
                if (!item.Value.Visible) continue;
            }
            
            //TCP通讯
            ICommunication comm = new CommTCP()
            {
                Network = new LNetwork() { Ip = ""127.0.0.1"", Port = 8888 }
            };
            CommResult ret =comm.Open();
            if(!ret.IsSuccess)
            {
                //do something
            }
            List<byte> command = Encoding.UTF8.GetBytes(""Hello, server!"").ToList();
            comm.Send(command);

            VPlc device = (VPlc)_deviceEngine.GetVirtualByName(""plc1"");
            var plcid = device.ID.ToString();
            Console.WriteLine(plcid);

            #region  示例代码
            //获取指定节点
            //Guid nodeID = Guid.Parse(""7282315a-cb75-4e76-bff3-a698538d7717"");
            //var module = _eventBus.GetModule(nodeID);
            //var mid = module.ID.ToString();
            //Console.WriteLine(mid);   
            //获取全局变量
            //var gID = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
            //var gModule = _eventBus.GetModule(gID);
            //var GlobalParams = gModule.Parameters;
            //plc 读写
            //VPlc device= (VPlc)_deviceEngine.GetVirtualByName(""plc_ts1"");
            ////写入PLC
            //string EmptyRunAddr = ""100"";
            //device.Write<bool>(EmptyRunAddr, true, $""01 05 {EmptyRunAddr} 1"");
            ////批量读plc
            //List<bool> readVal=device.ReadBatchAlarm(100,1);
            //读取轴
            //VAxis axis1 = (VAxis)_deviceEngine.GetVirtualByName(""仿真轴1_1"");
            //axis1.Jog(true, 1.1);
            //读取IO
            //var list = _deviceEngine.GetDevices(typeof(VIO));
            //for (int i = 0; i < list.Count; i++) {  VIO device = (VIO)list[i];  device.SetDigital(false);}
            ////气缸
            //VCylinder cyl=(VCylinder)_deviceEngine.GetVirtualByName(""气缸1"");
            //if (cyl == null) return;
            ////伸出
            //cyl.Extend();
            ////缩回
            //cyl.Retract();
            ////真空
            //VVacuum  vac=(VVacuum)_deviceEngine.GetVirtualByName(""真空1"");
            ////吸
            //vac.Suck();
            ////吹
            //vac.Blow();
            #endregion
        }

        public void Destroy()
        {
            System.Diagnostics.Debug.WriteLine(""Script Destroy"");
        }
    }
}";
        private string _outputDirectory;
        private string _latestGeneratedDll;
        private string _statusText;
        private string _outputDirText;
        private string _errorText;
        private string _fileListBoxText;
        private string _outputText; 
        private string _latestFileText;

        IMotionModule _MotionModule = null;
        private string _ID;
        

        private List<string> _FileList;

        /// <summary>
        /// CS文件编辑器内容
        /// </summary>
        public string CodeEditorText 
        {
            get => _CodeEditorText;
            set => SetProperty(ref _CodeEditorText, value);
        }

        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutputDirectory
        {             
            get => _outputDirectory;
            set => SetProperty(ref _outputDirectory, value);
        }

        /// <summary>
        /// 最新dll
        /// </summary>
        public string LatestGeneratedDll
        {
            get => _latestGeneratedDll;
            set => SetProperty(ref _latestGeneratedDll, value);
        }

        /// <summary>
        /// 状态文本
        /// </summary>
        public string StatusText { get => _statusText; set=> SetProperty(ref _statusText, value); }

        /// <summary>
        /// 输出目录文本
        /// </summary>
        public string OutputDirText { get => _outputDirText; set=> SetProperty(ref _outputDirText, value); }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorText { get => _errorText; set=> SetProperty(ref _errorText, value); }

        /// <summary>
        /// 输出信息
        /// </summary>
        public string OutputText { get => _outputText; set=> SetProperty(ref _outputText, value); }

        /// <summary>
        /// 最近文件
        /// </summary>
        public string LatestFileText { get => _latestFileText; set=> SetProperty(ref _latestFileText, value); }

        /// <summary>
        /// 输出文件列表
        /// </summary>
        public List<string> FileList
        {
            get => _FileList;
            set => SetProperty(ref _FileList, value);
        }

        #endregion

        public CSharpEditorVM(ICommonBus commonBus)
        {
            this.commonBus = commonBus;
            Title = "C# 代码编辑器";
            FileList = new List<string>();                       
            StatusText = "就绪";            
            
        }

        #region commands
        /// <summary>
        /// 编译
        /// </summary>
        private DelegateCommand _GenerateDll_Click;
        public DelegateCommand GenerateDll_Click => _GenerateDll_Click ?? (_GenerateDll_Click = new DelegateCommand(() =>
        {
            CompileAndExecute(CodeEditorText, true);
        }));

        /// <summary>
        /// 运行文件
        /// </summary>
        private DelegateCommand _RunInMemory_Click;
        public DelegateCommand RunInMemory_Click => _RunInMemory_Click ?? (_RunInMemory_Click = new DelegateCommand(() =>
        {
            CompileAndExecute(CodeEditorText, false);
        }));

        /// <summary>
        /// 运行dll
        /// </summary>
        private DelegateCommand _RunDll_Click;
        public DelegateCommand RunDll_Click => _RunDll_Click ?? (_RunDll_Click = new DelegateCommand(() =>
        {
            if (string.IsNullOrEmpty(_latestGeneratedDll) || !File.Exists(_latestGeneratedDll))
            {
                MessageBox.Show("没有可用的DLL文件，请先生成DLL", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ExecuteAssemblyFromFile(_latestGeneratedDll);
        }));

        /// <summary>
        /// 清除输出
        /// </summary>
        private DelegateCommand _ClearOutput_Click;
        public DelegateCommand ClearOutput_Click => _ClearOutput_Click ?? (_ClearOutput_Click = new DelegateCommand(() =>
        {
            OutputText = "";
            ErrorText = "";
            StatusText = "就绪";
        }));

        /// <summary>
        /// 打开输出目录
        /// </summary>
        private DelegateCommand _OpenOutputDir_Click;
        public DelegateCommand OpenOutputDir_Click => _OpenOutputDir_Click ?? (_OpenOutputDir_Click = new DelegateCommand(() =>
        {
            if (Directory.Exists(_outputDirectory))
            {
                System.Diagnostics.Process.Start("explorer.exe", _outputDirectory);
            }
        }));

        /// <summary>
        /// 清理输出文件
        /// </summary>
        private DelegateCommand _CleanOutput_Click;
        public DelegateCommand CleanOutput_Click => _CleanOutput_Click ?? (_CleanOutput_Click = new DelegateCommand(() =>
        {
            try
            {
                if (Directory.Exists(_outputDirectory))
                {
                    foreach (var file in Directory.GetFiles(_outputDirectory))
                    {
                        File.Delete(file);
                    }
                    UpdateFileList();
                    StatusText = "输出文件已清理";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清理失败: {ex.Message}");
            }
        }));

        #endregion

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            string id;
            base.OnDialogOpened(parameters);
            parameters.TryGetValue<string>("ID", out id);
            _ID = id;
            object motionModule = new object();
            parameters.TryGetValue<object>("Module", out motionModule);
            _MotionModule = motionModule as IMotionModule;
            InitializeReferences();
            InitializeOutputDirectory();
            initializeCodeEditor();
        }

        /// <summary>
        /// 加载依赖项
        /// </summary>
        private void InitializeReferences()
        {
            // 添加基本的 .NET 程序集引用
            AddReference(typeof(object).Assembly);
            AddReference(typeof(Console).Assembly);
            AddReference(typeof(Enumerable).Assembly);
            AddReference(typeof(System.Dynamic.DynamicObject).Assembly);
            AddReference(typeof(Uri).Assembly);
            AddReference(typeof(List<>).Assembly);

            // 添加 .NET 8 运行时常用基础程序集
            string runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            string[] basicAssemblies = new[]
            {
                "System.Runtime.dll",
                "netstandard.dll",
                "System.Console.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.Private.CoreLib.dll"
            };
            foreach (var asm in basicAssemblies)
            {
                var path = Path.Combine(runtimeDir, asm);
                if (File.Exists(path) && !_references.Any(r => r.Display == path))
                {
                    _references.Add(MetadataReference.CreateFromFile(path));
                }
            }
            //添加sharpscript程序集引用（自动识别SharpScript.csproj依赖）
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string csprojPath = Path.Combine(baseDir, "SharpScript", "SharpScript.csproj");
                if (File.Exists(csprojPath))
                {
                    var doc = System.Xml.Linq.XDocument.Load(csprojPath);

                    // 1. 处理 <Reference> 节点下的 <HintPath>
                    var hintPaths = doc.Descendants()
                        .Where(x => x.Name.LocalName == "HintPath")
                        .Select(x => x.Value)
                        .Distinct();

                    foreach (var hintPath in hintPaths)
                    {
                        string dllPath = Path.GetFullPath(Path.Combine(baseDir, "SharpScript\\", hintPath));
                        if (File.Exists(dllPath) && !_references.Any(r => r.Display == dllPath))
                        {
                            _references.Add(MetadataReference.CreateFromFile(dllPath));
                        }
                    }

                    // 2. 处理 <Reference Include="xxx.dll" /> 直接引用
                    var refIncludes = doc.Descendants()
                        .Where(x => x.Name.LocalName == "Reference" && x.Attribute("Include") != null)
                        .Select(x => x.Attribute("Include").Value)
                        .Where(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        .Distinct();

                    foreach (var refDll in refIncludes)
                    {
                        string dllPath = Path.Combine(baseDir, "SharpScript", refDll);
                        if (File.Exists(dllPath) && !_references.Any(r => r.Display == dllPath))
                        {
                            _references.Add(MetadataReference.CreateFromFile(dllPath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorText = $"SharpScript依赖解析失败: {ex.Message}";
            }
            StatusText = $"已加载 {_references.Count} 个引用";
        }

        /// <summary>
        /// 设置输出目录
        /// </summary>
        private void InitializeOutputDirectory()
        {
            // 设置输出目录
            string currentRecipePath = commonBus.CurrentRecipe.GetRecipePath();
            _outputDirectory = Path.Combine(currentRecipePath, "script", _ID);
            Directory.CreateDirectory(_outputDirectory);
            OutputDirText = _outputDirectory;

            UpdateFileList();
        }


        /// <summary>
        /// 初始化代码编辑器
        /// </summary>
        private void initializeCodeEditor()
        {
            // 根据ID从输出目录_outputDirectory加载最近的文件
            if (Directory.Exists(_outputDirectory))
            {
                var files = Directory.GetFiles(_outputDirectory, "*.json")
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .ToList();

                if (files.Count > 0)
                {
                    // 加载最近的cs文件内容
                    try
                    {
                        CodeEditorText = File.ReadAllText(files[0], Encoding.UTF8);
                        LatestFileText = Path.GetFileName(files[0]);
                        StatusText = $"已加载最近文件: {LatestFileText}";
                        return;
                    }
                    catch (Exception ex)
                    {
                        ErrorText = $"加载文件失败: {ex.Message}";
                    }
                }
            }

            // 如果没有则加载默认模板：即当前目录下\SharpScript中的UserScript文件内容
            //try
            //{
            //    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            //    string templatePath = Path.Combine(baseDir, "SharpScript", "UserScript.cs");
            //    if (File.Exists(templatePath))
            //    {
            //        CodeEditorText = File.ReadAllText(templatePath, Encoding.UTF8);
            //        StatusText = "已加载默认模板";
            //    }
            //    else
            //    {
            //        CodeEditorText = "// 请在此处编写C#代码";
            //        StatusText = "未找到默认模板，已加载空白模板";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CodeEditorText = "// 模板加载失败";
            //    ErrorText = $"模板加载失败: {ex.Message}";
            //    StatusText = "模板加载失败";
            //}
        }
        private void AddReference(Assembly assembly)
        {
            try
            {
                if (!string.IsNullOrEmpty(assembly.Location))
                {
                    var reference = MetadataReference.CreateFromFile(assembly.Location);
                    _references.Add(reference);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法添加引用 {assembly.FullName}: {ex.Message}");
            }
        }

        private void CompileAndExecute(string code, bool generatePhysicalFile)
        {
            try
            {
                OutputText = "";
                ErrorText = "";
                StatusText = generatePhysicalFile ? "正在生成DLL文件..." : "正在编译...";

                // 解析代码
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code,
                    new CSharpParseOptions(LanguageVersion.Latest));

                // 创建编译选项
                var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Debug,
                    allowUnsafe: false);

                // 生成唯一程序集名称
                string assemblyName = $"DynamicAssembly_{DateTime.Now:yyyyMMdd_HHmmss}";

                // 创建编译
                var compilation = CSharpCompilation.Create(
                    assemblyName,
                    new[] { syntaxTree },
                    _references,
                    compilationOptions);

                if (generatePhysicalFile)
                {
                    CompileToPhysicalFile(compilation);
                }
                else
                {
                    CompileToMemory(compilation);
                }
            }
            catch (Exception ex)
            {
                ErrorText = $"编译错误: {ex}";
                StatusText = "编译失败";
            }
        }

        private void CompileToMemory(Compilation compilation)
        {
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ExecuteAssemblyFromMemory(ms);
                    StatusText = "内存执行完成";
                }
                else
                {
                    ShowCompilationErrors(result);
                }
            }
        }

        private void CompileToPhysicalFile(Compilation compilation)
        {
            try
            {
                

                string dllPath = Path.Combine(_outputDirectory, $"{compilation.AssemblyName}.dll");
                string pdbPath = Path.Combine(_outputDirectory, $"{compilation.AssemblyName}.pdb");
                string csPath = Path.Combine(_outputDirectory, $"{compilation.AssemblyName}.json");

                // 保存源代码文件
                File.WriteAllText(csPath, CodeEditorText);
                bool fileSaved = false;
                using (var dllStream = File.Create(dllPath))
                using (var pdbStream = File.Create(pdbPath))
                {
                    var emitOptions = new EmitOptions(
                        debugInformationFormat: DebugInformationFormat.PortablePdb);

                    EmitResult result = compilation.Emit(
                        dllStream,
                        pdbStream,
                        options: emitOptions);

                    if (result.Success)
                    {
                        dllStream.Flush();
                        pdbStream.Flush();

                        _latestGeneratedDll = dllPath;
                        LatestFileText = Path.GetFileName(dllPath);

                        StatusText = $"DLL生成成功: {Path.GetFileName(dllPath)}";
                        OutputText = $"✅ 文件生成成功!\n\n" +
                                           $"DLL: {dllPath}\n" +
                                           $"PDB: {pdbPath}\n" +
                                           $"CS: {csPath}\n\n" +
                                           $"文件大小: {new FileInfo(dllPath).Length} 字节";
                        fileSaved = true;
                        UpdateFileList();
                    }
                    else
                    {
                        ShowCompilationErrors(result);
                    }
                }
                if (fileSaved)
                {
                    _MotionModule.Parameters["DllPath"].Value = new LPath() { Path = dllPath, PathType = System.Diagnostics.Eventing.Reader.PathType.FilePath };

                }
            }
            catch (Exception ex)
            {
                ErrorText = $"文件生成错误: {ex.Message}";
                StatusText = "文件生成失败";
            }
        }

        private void ExecuteAssemblyFromMemory(MemoryStream assemblyStream)
        {
            try
            {
                var originalOut = Console.Out;
                var outputWriter = new StringWriter();
                Console.SetOut(outputWriter);

                var assembly = Assembly.Load(assemblyStream.ToArray());
                 Type type = assembly.GetType("LusterMotionScript.UserScript");

                if (type == null)
                {
                    System.Diagnostics.Debug.WriteLine("未找到类型 LusterMotionScript.UserScript");
                    return;
                }

                var _instance = Activator.CreateInstance(type);
                var _method_run = type.GetMethod("Run");
                var _method_detroy = type.GetMethod("Destroy");
                MethodInfo method_intial = type.GetMethod("Initial");
                if (_method_run == null)
                {
                    System.Diagnostics.Debug.WriteLine("未找到方法 Run");
                    throw new FriendlyException($"未找到方法 Run！");
                }

                if (method_intial != null)
                {
                    method_intial.Invoke(_instance, null);
                }
                   
                if (_method_run != null)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            _method_run.Invoke(_instance, null);
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ErrorText += $"\n执行错误: {ex}";
                            });
                        }
                    }).ContinueWith(t =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OutputText = outputWriter.ToString();
                            StatusText = "执行完成";
                            Console.SetOut(originalOut);
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorText = $"加载错误: {ex}";
                StatusText = "执行失败";
            }
        }

        [Obsolete]
        private void ExecuteAssemblyFromFile1(string assemblyPath)
        {
            try
            {
                StatusText = "正在执行DLL...";

                var originalOut = Console.Out;
                var outputWriter = new StringWriter();
                Console.SetOut(outputWriter);

                // 使用LoadFrom而不是Load来避免程序集锁定问题
                //var assembly = Assembly.LoadFrom(assemblyPath);

                // 从文件读取字节数组，然后加载，这样可以避免文件锁定
                byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
                var assembly = Assembly.Load(assemblyBytes);
                
                Type type = assembly.GetType("LusterMotionScript.UserScript");

                if (type == null)
                {
                    System.Diagnostics.Debug.WriteLine("未找到类型 LusterMotionScript.UserScript");
                    return;
                }

                var _instance = Activator.CreateInstance(type);
                var _method_run = type.GetMethod("Run");
                var _method_detroy = type.GetMethod("Destroy");
                MethodInfo method_intial = type.GetMethod("Initial");
                if (_method_run == null)
                {
                    System.Diagnostics.Debug.WriteLine("未找到方法 Run");
                    throw new FriendlyException($"未找到方法 Run！");
                }

                if (method_intial != null)
                {
                    method_intial.Invoke(_instance, null);
                }
                   
                if (_method_run != null)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            _method_run.Invoke(_instance, null);
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ErrorText += $"\n执行错误: {ex.Message}";
                            });
                        }
                    }).ContinueWith(t =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OutputText = outputWriter.ToString();
                            StatusText = "DLL执行完成";
                            Console.SetOut(originalOut);
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorText = $"DLL执行错误: {ex.Message}";
                StatusText = "执行失败";
            }
        }
        private void ExecuteAssemblyFromFile(string assemblyPath)
        {
            try
            {
                StatusText = "正在执行DLL...";

                var originalOut = Console.Out;
                var outputWriter = new StringWriter();
                Console.SetOut(outputWriter);

                // 从文件读取字节数组，然后加载，这样可以避免文件锁定
                byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
                var assembly = Assembly.Load(assemblyBytes);

                Type type = assembly.GetType("LusterMotionScript.UserScript");

                if (type == null)
                {
                    System.Diagnostics.Debug.WriteLine("未找到类型 LusterMotionScript.UserScript");
                    return;
                }

                var instance = Activator.CreateInstance(type);
                var runMethod = type.GetMethod("Run");
                var destroyMethod = type.GetMethod("Destroy");
                var initialMethod = type.GetMethod("Initial");

                if (runMethod == null)
                {
                    throw new FriendlyException("未找到方法 Run！");
                }

                // 同步执行，确保完成后才继续
                try
                {
                    initialMethod?.Invoke(instance, null);
                    runMethod.Invoke(instance, null);
                    destroyMethod?.Invoke(instance, null);
                }
                finally
                {
                    // 强制垃圾回收帮助释放资源
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    OutputText = outputWriter.ToString();
                    StatusText = "DLL执行完成";
                    Console.SetOut(originalOut);
                });
            }
            catch (Exception ex)
            {
                ErrorText = $"DLL执行错误: {ex}";
                StatusText = "执行失败";
            }
        }

        private void ShowCompilationErrors(EmitResult result)
        {
            var errors = new StringBuilder();
            foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
            {
                var lineSpan = diagnostic.Location.GetLineSpan();
                errors.AppendLine($"❌ {diagnostic.Severity}: {diagnostic.GetMessage()}");
                errors.AppendLine($"   位置: 行 {lineSpan.StartLinePosition.Line + 1}, 列 {lineSpan.StartLinePosition.Character + 1}");
                errors.AppendLine();
            }

            ErrorText = errors.ToString();
            StatusText = "编译错误";
        }

        private void UpdateFileList()
        {
            FileList.Clear();
            if (Directory.Exists(_outputDirectory))
            {
                var files = Directory.GetFiles(_outputDirectory)
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .Take(10);

                foreach (var file in files)
                {
                    var info = new FileInfo(file);
                    FileList.Add(
                        $"{info.LastWriteTime:HH:mm:ss} | {info.Length,8} bytes | {info.Name}");
                }
            }
        }

    }
}
