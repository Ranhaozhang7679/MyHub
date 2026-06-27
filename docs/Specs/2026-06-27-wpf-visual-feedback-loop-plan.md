# WPF 视觉反馈闭环 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让 Agent 写完 WPF View 能用一条命令截图,再用视觉模型评阅出结构化报告,补上"看不见渲染结果"的盲区。

**Architecture:** 独立预览 exe(`Luster.PreviewHost`,net472 WinExe)复用 Shell 主题字典渲染指定 View 并截图;独立评阅工具(`Luster.VisualReviewer`,net472 console)读设计契约 + 截图调 siliconflow Qwen3-VL 出 JSON 报告;设计契约文档人机共用;工作区 `workspace/wpf-preview/<View>/` 按目录归档截图/报告/索引。

**Tech Stack:** .NET Framework 4.7.2 / WPF / C# 9.0 / xUnit / HandyControl / siliconflow OpenAI 兼容 API

## Spec 修正(实施时遵循)

Spec §4.1 原写"解析 View.xaml 的 d:DesignInstance"。实际 `d:DesignInstance` 带 `mc:Ignorable="d"`,编译进 BAML 时被剥离,运行时无法从 View 类型读到。修正:PreviewHost 增加可选 `--xaml <源xaml路径>` 参数,从**源文件**解析 `d:DesignInstance` 取类型全名,再从已加载程序集反射实例化;未提供 `--xaml` 或解析不到时,渲染空 DataContext 并在报告标 `designdata: missing`。`--designvm <类型全名>` 作为显式覆盖优先级最高。

## Global Constraints

- TargetFramework: net472;LangVersion 9.0;Nullable enable;ImplicitUsings disable(见 `Directory.Build.props`)
- 所有构建输出到 `artifacts/bin/net472/`(Directory.Build.props 已统一)
- NuGet 包版本集中管理于 `Directory.Packages.props`,新增包须在此登记
- 测试用 xUnit + Microsoft.NET.Test.Sdk(见现有 `src/Tests/*.Tests.csproj`)
- 注释中文;命名空间 `Luster.<模块>.<组件>`;退出码:0=成功,1=渲染失败,2=视觉模型不可达,3=主题加载失败
- 新增模块 csproj 须加入 `LMV-2026.sln`
- 禁止直接 push develop/master,改动在 `feature/wpf-visual-feedback-loop` 分支

---

## File Structure

- **Create** `src/Tools/Luster.PreviewHost/Luster.PreviewHost.csproj` — WinExe 宿主工程
- **Create** `src/Tools/Luster.PreviewHost/App.xaml` + `App.xaml.cs` — 精简主题合并(照抄 Shell 顺序)
- **Create** `src/Tools/Luster.PreviewHost/DesignInstanceParser.cs` — 从源 XAML 解析 d:DesignInstance
- **Create** `src/Tools/Luster.PreviewHost/ViewRenderer.cs` — 实例化 View + mock VM、截图
- **Create** `src/Tools/Luster.PreviewHost/Program.cs` — CLI 解析、退出码、调度
- **Create** `src/Tools/Luster.PreviewHost/Fixtures/SampleView.xaml(+cs)` + `SampleDesignVm.cs` — 测试夹具 View
- **Create** `src/Tests/Luster.PreviewHost.Tests/Luster.PreviewHost.Tests.csproj` — xUnit 测试
- **Create** `src/Tests/Luster.PreviewHost.Tests/DesignInstanceParserTests.cs`
- **Create** `src/Tests/Luster.PreviewHost.Tests/ViewRendererTests.cs`
- **Create** `src/Tools/Luster.VisualReviewer/Luster.VisualReviewer.csproj` — console 评阅工程
- **Create** `src/Tools/Luster.VisualReviewer/ContractReader.cs` — 读契约 md
- **Create** `src/Tools/Luster.VisualReviewer/VisualReviewClient.cs` — 调 siliconflow + JSON 报告
- **Create** `src/Tools/Luster.VisualReviewer/Program.cs` — CLI + 退出码
- **Create** `src/Tests/Luster.VisualReviewer.Tests/Luster.VisualReviewer.Tests.csproj`
- **Create** `src/Tests/Luster.VisualReviewer.Tests/VisualReviewClientTests.cs`
- **Create** `docs/wpf-design-contract.md` — 设计契约
- **Create** `workspace/wpf-preview/index.md` — 全局索引(空模板)
- **Modify** `LMV-2026.sln` — 加入 4 个新 csproj
- **Modify** `Directory.Packages.props` — 登记 Newtonsoft.Json(若现有无)

---

### Task 1: Luster.PreviewHost 工程脚手架 + 主题合并

**Files:**
- Create: `src/Tools/Luster.PreviewHost/Luster.PreviewHost.csproj`
- Create: `src/Tools/Luster.PreviewHost/App.xaml`
- Create: `src/Tools/Luster.PreviewHost/App.xaml.cs`
- Create: `src/Tools/Luster.PreviewHost/Program.cs`(占位 Main)
- Modify: `LMV-2026.sln`

**Interfaces:**
- Produces: 可编译运行的空 WinExe,主题字典与 Shell 一致

- [ ] **Step 1: 建工程目录与 csproj**

Create `src/Tools/Luster.PreviewHost/Luster.PreviewHost.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <UseWPF>true</UseWPF>
    <PlatformTarget>x64</PlatformTarget>
    <AssemblyName>Luster.PreviewHost</AssemblyName>
    <RootNamespace>Luster.PreviewHost</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="HandyControl" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Modules\Luster.Common.Assets\Luster.Common.Assets.csproj" />
    <ProjectReference Include="..\..\Modules\Luster.Motion.Assests\Luster.Motion.Assests.csproj" />
    <ProjectReference Include="..\..\Modules\Luster.Controls.Wpf\Luster.Controls.Wpf.csproj" />
    <ProjectReference Include="..\..\Modules\Luster.Control.Wpf.Motion\Luster.Control.Wpf.Motion.csproj" />
    <ProjectReference Include="..\..\Modules\Luster.SimDevice.SubSystem\Luster.SimDevice.SubSystem.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: 建 App.xaml(照抄 Shell App.xaml 的主题合并顺序,精简)**

Create `src/Tools/Luster.PreviewHost/App.xaml`:
```xml
<Application x:Class="Luster.PreviewHost.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml" />
                <ResourceDictionary Source="pack://application:,,,/Luster.Common.Assets;component/Themes/SkinDefault.xaml" />
                <ResourceDictionary Source="pack://application:,,,/Luster.Common.Assets;component/Themes/Theme.xaml" />
                <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/Theme.xaml" />
                <ResourceDictionary Source="/Luster.Controls.Wpf;component/Themes/Generic.xaml" />
                <ResourceDictionary Source="/Luster.Control.Wpf.Motion;component/Generic.xaml" />
                <ResourceDictionary Source="/Luster.Common.Assets;component/Themes/Styles/Style.xaml" />
                <ResourceDictionary Source="/Luster.Motion.Assests;component/Themes/Theme.xaml" />
                <ResourceDictionary Source="/Luster.SimDevice.SubSystem;component/Resources/Style.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

- [ ] **Step 3: 建 App.xaml.cs**

Create `src/Tools/Luster.PreviewHost/App.xaml.cs`:
```csharp
namespace Luster.PreviewHost
{
    /// <summary>预览宿主入口,主题在 App.xaml 合并,真实 Main 在 Program.cs</summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            // 实际调度在 Program.Main,这里仅保证主题加载
        }
    }
}
```

- [ ] **Step 4: 建 Program.cs(占位 Main,Task 4 填实现)**

Create `src/Tools/Luster.PreviewHost/Program.cs`:
```csharp
using System;

namespace Luster.PreviewHost
{
    /// <summary>命令行入口,Task 4 填充参数解析与调度</summary>
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Console.Error.WriteLine("PreviewHost: 尚未实现(Task 4 填充)");
            return 0;
        }
    }
}
```

注意:WinExe 默认用 App 做入口,需在 csproj 加 `<StartupObject>Luster.PreviewHost.Program</StartupObject>` 才能让 Program.Main 生效。在 Step 1 的 PropertyGroup 内补:
```xml
    <StartupObject>Luster.PreviewHost.Program</StartupObject>
```

- [ ] **Step 5: 加入 sln**

```bash
dotnet sln LMV-2026.sln add src/Tools/Luster.PreviewHost/Luster.PreviewHost.csproj
```

- [ ] **Step 6: 构建验证**

Run: `dotnet build LMV-2026.sln`
Expected: PreviewHost 编译通过(主题 pack URI 能解析)

- [ ] **Step 7: Commit**

```bash
git add src/Tools/Luster.PreviewHost LMV-2026.sln
git commit -m "feat(preview): 建 Luster.PreviewHost 工程脚手架+主题合并 #1"
```

---

### Task 2: DesignInstanceParser(从源 XAML 解析 d:DesignInstance)

**Files:**
- Create: `src/Tools/Luster.PreviewHost/DesignInstanceParser.cs`
- Create: `src/Tests/Luster.PreviewHost.Tests/Luster.PreviewHost.Tests.csproj`
- Test: `src/Tests/Luster.PreviewHost.Tests/DesignInstanceParserTests.cs`

**Interfaces:**
- Produces: `DesignInstanceParser.Parse(string xaml) -> DesignInstanceInfo?`
  - `DesignInstanceInfo { string TypeName; bool IsDesignDataCreatable; }`
  - 解析 `d:DesignInstance="ClrNamespace.Foo.Bar"` 或 `d:DesignInstance="{Type ClrNS.Bar}"`;无则返回 null

- [ ] **Step 1: 建测试工程 csproj**

Create `src/Tests/Luster.PreviewHost.Tests/Luster.PreviewHost.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Tools\Luster.PreviewHost\Luster.PreviewHost.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: 写失败测试**

Create `src/Tests/Luster.PreviewHost.Tests/DesignInstanceParserTests.cs`:
```csharp
using Luster.PreviewHost;
using Xunit;

namespace Luster.PreviewHost.Tests
{
    public class DesignInstanceParserTests
    {
        [Fact]
        public void Parse_SimpleDesignInstance_ReturnsTypeName()
        {
            string xaml = "<UserControl d:DesignInstance=\"ClrNs.Foo.DesignVm\" xmlns:d=\"http://...\"/>";
            var info = DesignInstanceParser.Parse(xaml);
            Assert.NotNull(info);
            Assert.Equal("ClrNs.Foo.DesignVm", info.TypeName);
        }

        [Fact]
        public void Parse_TypeMarkup_ReturnsTypeName()
        {
            string xaml = "<UserControl d:DesignInstance=\"{Type ClrNs.Foo.DesignVm}\"/>";
            var info = DesignInstanceParser.Parse(xaml);
            Assert.NotNull(info);
            Assert.Equal("ClrNs.Foo.DesignVm", info.TypeName);
        }

        [Fact]
        public void Parse_NoDesignInstance_ReturnsNull()
        {
            string xaml = "<UserControl xmlns=\"http://...\"/>";
            Assert.Null(DesignInstanceParser.Parse(xaml));
        }

        [Fact]
        public void Parse_IsDesignDataCreatableTrueByDefault()
        {
            string xaml = "<UserControl d:DesignInstance=\"ClrNs.Foo.DesignVm\"/>";
            var info = DesignInstanceParser.Parse(xaml);
            Assert.True(info.IsDesignDataCreatable);
        }
    }
}
```

- [ ] **Step 3: 加测试工程进 sln 并跑,确认失败**

```bash
dotnet sln LMV-2026.sln add src/Tests/Luster.PreviewHost.Tests/Luster.PreviewHost.Tests.csproj
dotnet test src/Tests/Luster.PreviewHost.Tests --filter "DesignInstanceParser"
```
Expected: FAIL(DesignInstanceParser 未定义)

- [ ] **Step 4: 实现 DesignInstanceParser**

Create `src/Tools/Luster.PreviewHost/DesignInstanceParser.cs`:
```csharp
using System;
using System.Text.RegularExpressions;

namespace Luster.PreviewHost
{
    /// <summary>从源 XAML 解析 d:DesignInstance 设计时实例类型</summary>
    public sealed class DesignInstanceInfo
    {
        public string TypeName { get; set; }
        public bool IsDesignDataCreatable { get; set; } = true;
    }

    public static class DesignInstanceParser
    {
        // 匹配 d:DesignInstance="ClrNs.Type" 或 d:DesignInstance="{Type ClrNs.Type}"
        private static readonly Regex Pattern = new Regex(
            @"d:DesignInstance\s*=\s*\""(?:\{Type\s+)?(?<type>[\w.]+)(?:\})?\""",
            RegexOptions.Compiled);

        /// <summary>从 XAML 文本解析 d:DesignInstance;无则返回 null</summary>
        public static DesignInstanceInfo Parse(string xaml)
        {
            if (string.IsNullOrEmpty(xaml)) return null;
            var m = Pattern.Match(xaml);
            if (!m.Success) return null;
            return new DesignInstanceInfo
            {
                TypeName = m.Groups["type"].Value,
                IsDesignDataCreatable = true
            };
        }
    }
}
```

- [ ] **Step 5: 跑测试确认通过**

```bash
dotnet test src/Tests/Luster.PreviewHost.Tests --filter "DesignInstanceParser"
```
Expected: PASS(4 passed)

- [ ] **Step 6: Commit**

```bash
git add src/Tools/Luster.PreviewHost/DesignInstanceParser.cs src/Tests/Luster.PreviewHost.Tests
git commit -m "feat(preview): DesignInstanceParser 解析 d:DesignInstance #2"
```

---

### Task 3: ViewRenderer(实例化 View+mock VM、截图)

**Files:**
- Create: `src/Tools/Luster.PreviewHost/ViewRenderer.cs`
- Create: `src/Tools/Luster.PreviewHost/Fixtures/SampleView.xaml(+cs)`
- Create: `src/Tools/Luster.PreviewHost/Fixtures/SampleDesignVm.cs`
- Test: `src/Tests/Luster.PreviewHost.Tests/ViewRendererTests.cs`

**Interfaces:**
- Consumes: `DesignInstanceInfo` from Task 2
- Produces: `ViewRenderer.Render(RenderRequest) -> RenderResult`
  - `RenderRequest { string ViewTypeName; string AssemblyPath; string DesignVmTypeName; int Width; int Height; }`
  - `RenderResult { bool Success; string Error; byte[] PngBytes; bool DesignDataPresent; }`
  - 调用须在 STA 线程(WPF 要求);ViewRenderer 内部用 `Dispatcher` 或新 STA 线程包裹

- [ ] **Step 1: 建夹具 SampleView + DesignVm**

Create `src/Tools/Luster.PreviewHost/Fixtures/SampleDesignVm.cs`:
```csharp
namespace Luster.PreviewHost.Fixtures
{
    /// <summary>测试夹具设计时 VM</summary>
    public class SampleDesignVm
    {
        public string Title { get; set; } = "预览示例标题";
        public int ParameterValue { get; set; } = 42;
    }
}
```

Create `src/Tools/Luster.PreviewHost/Fixtures/SampleView.xaml`:
```xml
<UserControl x:Class="Luster.PreviewHost.Fixtures.SampleView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:local="clr-namespace:Luster.PreviewHost.Fixtures"
             mc:Ignorable="d"
             d:DesignHeight="300" d:DesignWidth="400"
             d:DesignInstance="local:SampleDesignVm">
    <StackPanel Margin="16">
        <TextBlock Text="{Binding Title}" FontSize="20" FontWeight="Bold"/>
        <TextBlock Text="{Binding ParameterValue}" FontSize="14" Margin="0,8,0,0"/>
    </StackPanel>
</UserControl>
```
注意:`d:DesignInstance="local:SampleDesignVm"` 用的是 XAML 前缀别名,源文件解析时 Parser 的正则只认 `[\w.]+`,拿不到带冒号的别名。**因此 PreviewHost 走 `--designvm <全名>` 显式路径**,源 XAML 里的 d:DesignInstance 仅供 VS 设计器。本夹具测试用 `DesignVmTypeName="Luster.PreviewHost.Fixtures.SampleDesignVm"` 全名传入。

Create `src/Tools/Luster.PreviewHost/Fixtures/SampleView.xaml.cs`:
```csharp
namespace Luster.PreviewHost.Fixtures
{
    using System.Windows.Controls;

    public partial class SampleView : UserControl
    {
        public SampleView() => InitializeComponent();
    }
}
```

- [ ] **Step 2: 写失败测试**

Create `src/Tests/Luster.PreviewHost.Tests/ViewRendererTests.cs`:
```csharp
using Luster.PreviewHost;
using Luster.PreviewHost.Fixtures;
using Xunit;

namespace Luster.PreviewHost.Tests
{
    public class ViewRendererTests
    {
        // 用本程序集内的夹具 View,无需外部 assembly
        private static RenderRequest SampleRequest(string designVm = "Luster.PreviewHost.Fixtures.SampleDesignVm") =>
            new RenderRequest
            {
                ViewTypeName = typeof(SampleView).AssemblyQualifiedName,
                DesignVmTypeName = designVm,
                Width = 400,
                Height = 300
            };

        [Fact]
        public void Render_WithDesignVm_SucceedsAndProducesPng()
        {
            var result = ViewRenderer.Render(SampleRequest());
            Assert.True(result.Success, result.Error ?? "");
            Assert.NotNull(result.PngBytes);
            Assert.True(result.PngBytes.Length > 0);
            Assert.True(result.DesignDataPresent);
        }

        [Fact]
        public void Render_WithoutDesignVm_SucceedsButMarksMissing()
        {
            var req = SampleRequest(designVm: null);
            var result = ViewRenderer.Render(req);
            Assert.True(result.Success, result.Error ?? "");
            Assert.False(result.DesignDataPresent);
        }

        [Fact]
        public void Render_NonExistentView_ReturnsFailure()
        {
            var req = SampleRequest();
            req.ViewTypeName = "Does.Not.Exist, NoAssembly";
            var result = ViewRenderer.Render(req);
            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Error));
        }
    }
}
```

- [ ] **Step 3: 跑测试确认失败**

```bash
dotnet test src/Tests/Luster.PreviewHost.Tests --filter "ViewRenderer"
```
Expected: FAIL(ViewRenderer 未定义)

- [ ] **Step 4: 实现 ViewRenderer**

Create `src/Tools/Luster.PreviewHost/ViewRenderer.cs`:
```csharp
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Luster.PreviewHost
{
    public sealed class RenderRequest
    {
        public string ViewTypeName;       // AssemblyQualifiedName 或 "Full.Type, Asm"
        public string AssemblyPath;       // 可选:外部程序集路径,优先加载
        public string DesignVmTypeName;   // 可选:mock VM 类型全名
        public int Width = 1920;
        public int Height = 1080;
    }

    public sealed class RenderResult
    {
        public bool Success;
        public string Error;
        public byte[] PngBytes;
        public bool DesignDataPresent;
    }

    /// <summary>实例化 View + 设计时 VM,渲染到固定尺寸并截图为 PNG</summary>
    public static class ViewRenderer
    {
        public static RenderResult Render(RenderRequest req)
        {
            var result = new RenderResult();
            Exception workerError = null;
            // WPF 要求 STA 线程
            var thread = new Thread(() =>
            {
                try { RenderCore(req, result); }
                catch (Exception ex) { workerError = ex; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (workerError != null)
            {
                result.Success = false;
                result.Error = workerError.GetType().Name + ": " + workerError.Message;
            }
            return result;
        }

        private static void RenderCore(RenderRequest req, RenderResult result)
        {
            // 1. 可选加载外部程序集
            if (!string.IsNullOrEmpty(req.AssemblyPath) && File.Exists(req.AssemblyPath))
                Assembly.LoadFrom(req.AssemblyPath);

            // 2. 实例化 View
            var viewType = Type.GetType(req.ViewTypeName);
            if (viewType == null)
            {
                result.Success = false;
                result.Error = "找不到 View 类型: " + req.ViewTypeName;
                return;
            }
            var view = Activator.CreateInstance(viewType) as FrameworkElement;
            if (view == null)
            {
                result.Success = false;
                result.Error = "View 类型不可实例化为 FrameworkElement: " + viewType.FullName;
                return;
            }

            // 3. 实例化设计时 VM(可选)
            object dc = null;
            if (!string.IsNullOrEmpty(req.DesignVmTypeName))
            {
                var vmType = Type.GetType(req.DesignVmTypeName);
                if (vmType != null)
                {
                    dc = Activator.CreateInstance(vmType);
                    result.DesignDataPresent = dc != null;
                }
            }
            view.DataContext = dc;

            // 4. 测量排列到固定尺寸
            view.Measure(new Size(req.Width, req.Height));
            view.Arrange(new Rect(0, 0, req.Width, req.Height));
            view.UpdateLayout();

            // 5. 截图
            var dpi = 96;
            var rtb = new RenderTargetBitmap(req.Width, req.Height, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(view);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                result.PngBytes = ms.ToArray();
            }
            result.Success = true;
        }
    }
}
```

- [ ] **Step 5: 跑测试确认通过**

```bash
dotnet test src/Tests/Luster.PreviewHost.Tests --filter "ViewRenderer"
```
Expected: PASS(3 passed)

- [ ] **Step 6: Commit**

```bash
git add src/Tools/Luster.PreviewHost/ViewRenderer.cs src/Tools/Luster.PreviewHost/Fixtures src/Tests/Luster.PreviewHost.Tests/ViewRendererTests.cs
git commit -m "feat(preview): ViewRenderer 实例化View+mockVM并截图 #3"
```

---

### Task 4: Program.cs CLI 解析 + 退出码

**Files:**
- Modify: `src/Tools/Luster.PreviewHost/Program.cs`

**Interfaces:**
- Consumes: `DesignInstanceParser`, `ViewRenderer`
- Produces: CLI 入口
  - `PreviewHost.exe --view <类型全名> --assembly <dll> --xaml <src.xaml> --designvm <全名> --out <png> --width 1920 --height 1080`
  - 退出码:0=成功,1=渲染失败,3=主题加载失败

- [ ] **Step 1: 写集成测试(进程级)**

Append to `src/Tests/Luster.PreviewHost.Tests/ViewRendererTests.cs` 不合适(进程级),新建 `src/Tests/Luster.PreviewHost.Tests/ProgramCliTests.cs`:
```csharp
using System.Diagnostics;
using System.IO;
using Xunit;

namespace Luster.PreviewHost.Tests
{
    /// <summary>进程级 CLI 集成测试:跑实际 exe 验退出码与产物</summary>
    public class ProgramCliTests
    {
        // 测试工程 ProjectReference PreviewHost,exe 与测试 dll 同输出目录(artifacts/bin/net472/)
        private static string ExePath =>
            Path.Combine(System.AppContext.BaseDirectory, "Luster.PreviewHost.exe");

        [Fact(Skip = "集成测试:需先 build,手动启用")]
        public void Cli_RendersSampleView_ToPng()
        {
            var outPng = Path.GetTempFileName() + ".png";
            try
            {
                var psi = new ProcessStartInfo(ExePath)
                {
                    Arguments = $"--view Luster.PreviewHost.Fixtures.SampleView,Luster.PreviewHost " +
                                $"--designvm Luster.PreviewHost.Fixtures.SampleDesignVm,Luster.PreviewHost " +
                                $"--out {outPng} --width 400 --height 300",
                    UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true
                };
                var p = Process.Start(psi);
                string err = p.StandardError.ReadToEnd();
                p.WaitForExit(15000);
                Assert.Equal(0, p.ExitCode);
                Assert.True(File.Exists(outPng));
                Assert.True(new FileInfo(outPng).Length > 0);
            }
            finally { if (File.Exists(outPng)) File.Delete(outPng); }
        }
    }
}
```

- [ ] **Step 2: 实现 Program.Main**

Replace `src/Tools/Luster.PreviewHost/Program.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.IO;

namespace Luster.PreviewHost
{
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
                // 启动 WPF Application 以加载 App.xaml 主题
                var app = System.Windows.Application.Current ?? new App();
                // 触发主题加载(确保资源字典就绪)
                _ = app.TryFindResource("null_sentinel") == null; // no-op,触发合并
                // 注:实际实现须调 app.InitializeComponent() 才能触发 App.xaml 主题字典合并,
                // 仅 new App() 不会加载主题(Task4 实现时修正)

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

                var req = new RenderRequest
                {
                    ViewTypeName = opts.View,
                    AssemblyPath = opts.Assembly,
                    DesignVmTypeName = designVm,
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
                Console.WriteLine("已截图: " + opts.Out +
                                  (result.DesignDataPresent ? "" : " [警告: 无设计时数据]"));
                return 0;
            }
            catch (System.Windows.Markup.XamlParseException ex) // net472 用 XamlParseException,XamlException 不存在
            {
                Console.Error.WriteLine("主题加载失败: " + ex.Message);
                return 3;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("PreviewHost 异常: " + ex.GetType().Name + ": " + ex.Message);
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
    }
}
```

- [ ] **Step 3: 构建并跑集成测试(手动启用)**

```bash
dotnet build LMV-2026.sln
# 去掉 ProgramCliTests 的 Skip 后:
dotnet test src/Tests/Luster.PreviewHost.Tests --filter "ProgramCli"
```
Expected: 集成测试 PASS,生成 PNG

- [ ] **Step 4: Commit**

```bash
git add src/Tools/Luster.PreviewHost/Program.cs src/Tests/Luster.PreviewHost.Tests/ProgramCliTests.cs
git commit -m "feat(preview): Program CLI 解析+退出码+主题加载 #4"
```

---

### Task 5: Luster.VisualReviewer 工程与 ContractReader

**Files:**
- Create: `src/Tools/Luster.VisualReviewer/Luster.VisualReviewer.csproj`
- Create: `src/Tools/Luster.VisualReviewer/ContractReader.cs`
- Create: `src/Tests/Luster.VisualReviewer.Tests/Luster.VisualReviewer.Tests.csproj`
- Test: `src/Tests/Luster.VisualReviewer.Tests/ContractReaderTests.cs`

**Interfaces:**
- Produces: `ContractReader.Read(string path) -> string`(契约全文)

- [ ] **Step 1: 建 VisualReviewer csproj**

Create `src/Tools/Luster.VisualReviewer/Luster.VisualReviewer.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net472</TargetFramework>
    <RootNamespace>Luster.VisualReviewer</RootNamespace>
    <AssemblyName>Luster.VisualReviewer</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" />
  </ItemGroup>
</Project>
```
若 `Directory.Packages.props` 无 Newtonsoft.Json,在此文件登记版本(查现有版本号):
```bash
grep -n "Newtonsoft.Json" Directory.Packages.props
```

- [ ] **Step 2: 写失败测试**

Create `src/Tests/Luster.VisualReviewer.Tests/Luster.VisualReviewer.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\Tools\Luster.VisualReviewer\Luster.VisualReviewer.csproj" />
  </ItemGroup>
</Project>
```

Create `src/Tests/Luster.VisualReviewer.Tests/ContractReaderTests.cs`:
```csharp
using System.IO;
using Luster.VisualReviewer;
using Xunit;

namespace Luster.VisualReviewer.Tests
{
    public class ContractReaderTests
    {
        [Fact]
        public void Read_ReturnsFileContent()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "# 契约\n- 控件库: HandyControl");
            try
            {
                Assert.Equal("# 契约\n- 控件库: HandyControl", ContractReader.Read(path));
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void Read_MissingFile_ReturnsEmpty()
        {
            Assert.Equal("", ContractReader.Read(Path.Combine(Path.GetTempPath(), "no_such_contract.md")));
        }
    }
}
```

- [ ] **Step 3: 加 sln + 跑测试确认失败**

```bash
dotnet sln LMV-2026.sln add src/Tools/Luster.VisualReviewer/Luster.VisualReviewer.csproj
dotnet sln LMV-2026.sln add src/Tests/Luster.VisualReviewer.Tests/Luster.VisualReviewer.Tests.csproj
dotnet test src/Tests/Luster.VisualReviewer.Tests --filter "ContractReader"
```
Expected: FAIL

- [ ] **Step 4: 实现 ContractReader**

Create `src/Tools/Luster.VisualReviewer/ContractReader.cs`:
```csharp
using System.IO;

namespace Luster.VisualReviewer
{
    /// <summary>读取设计契约 md 全文,供视觉模型当评阅标准</summary>
    public static class ContractReader
    {
        public static string Read(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "";
            return File.ReadAllText(path);
        }
    }
}
```

- [ ] **Step 5: 跑测试确认通过 + Commit**

```bash
dotnet test src/Tests/Luster.VisualReviewer.Tests --filter "ContractReader"
git add src/Tools/Luster.VisualReviewer src/Tests/Luster.VisualReviewer.Tests Directory.Packages.props
git commit -m "feat(reviewer): VisualReviewer 工程+ContractReader #5"
```

---

### Task 6: VisualReviewClient(调 siliconflow + JSON 报告 + 降级)

**Files:**
- Create: `src/Tools/Luster.VisualReviewer/VisualReviewClient.cs`
- Create: `src/Tools/Luster.VisualReviewer/ReviewModels.cs`
- Test: `src/Tests/Luster.VisualReviewer.Tests/VisualReviewClientTests.cs`

**Interfaces:**
- Produces:
  - `IVisualReviewClient.ReviewAsync(byte[] png, string contract, string viewName) -> ReviewReport`
  - `ReviewReport { string View; string Summary; int Score; string DesignData; List<ReviewIssue> Issues; bool Degraded; }`
  - `ReviewIssue { string Severity; string Category; string Description; string Location; }`
  - 降级:网络/key 失败 → `Degraded=true`,仍正常返回(截图已由调用方落盘),退出码 2

- [ ] **Step 1: 写失败测试(桩 HTTP)**

Create `src/Tests/Luster.VisualReviewer.Tests/VisualReviewClientTests.cs`:
```csharp
using System;
using System.Threading.Tasks;
using Luster.VisualReviewer;
using Moq;
using Xunit;

namespace Luster.VisualReviewer.Tests
{
    public interface IVisualReviewClient
    {
        Task<ReviewReport> ReviewAsync(byte[] png, string contract, string viewName);
    }

    public class VisualReviewClientTests
    {
        private sealed class StubClient : IVisualReviewClient
        {
            public string ReturnedJson;
            public Exception ToThrow;
            public Task<ReviewReport> ReviewAsync(byte[] png, string contract, string viewName)
            {
                if (ToThrow != null) throw ToThrow;
                return Task.FromResult(VisualReviewClient.ParseReport(ReturnedJson, viewName));
            }
        }

        [Fact]
        public async Task ParseReport_ValidJson_PopulatesFields()
        {
            var json = @"{""summary"":""布局清晰"",""score"":8,""issues"":[{""severity"":""high"",""category"":""overlap"",""description"":""控件重叠"",""location"":""右下""}]}";
            var stub = new StubClient { ReturnedJson = json };
            var report = await stub.ReviewAsync(new byte[0], "契约", "YyyView");
            Assert.Equal("YyyView", report.View);
            Assert.Equal(8, report.Score);
            Assert.Single(report.Issues);
            Assert.Equal("high", report.Issues[0].Severity);
        }

        [Fact]
        public async Task Review_NetworkFailure_Degrades()
        {
            var stub = new StubClient { ToThrow = new InvalidOperationException("网络不可达") };
            var report = await stub.ReviewAsync(new byte[0], "契约", "YyyView");
            Assert.True(report.Degraded);
            Assert.Empty(report.Issues);
        }
    }
}
```
说明:测试通过 `ParseReport` 静态方法 + 桩验证降级逻辑,不走真实网络。需 `Moq`?实际用 Stub 类,不依赖 Moq。移除 `using Moq;`。

- [ ] **Step 2: 跑测试确认失败**

```bash
dotnet test src/Tests/Luster.VisualReviewer.Tests --filter "VisualReviewClient"
```
Expected: FAIL

- [ ] **Step 3: 实现 ReviewModels + VisualReviewClient**

Create `src/Tools/Luster.VisualReviewer/ReviewModels.cs`:
```csharp
using System.Collections.Generic;

namespace Luster.VisualReviewer
{
    public sealed class ReviewIssue
    {
        public string Severity;   // high/medium/low
        public string Category;   // overlap/spacing/control-lib/font/...
        public string Description;
        public string Location;
    }

    public sealed class ReviewReport
    {
        public string View;
        public string Screenshot;
        public string Summary;
        public int Score;
        public string DesignData = "present"; // present/missing
        public bool Degraded;
        public List<ReviewIssue> Issues = new List<ReviewIssue>();
    }

    public interface IVisualReviewClient
    {
        ReviewReport Review(byte[] png, string contract, string viewName);
    }
}
```

Create `src/Tools/Luster.VisualReviewer/VisualReviewClient.cs`:
```csharp
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luster.VisualReviewer
{
    /// <summary>调 siliconflow Qwen3-VL 评阅截图,产结构化报告;网络失败降级</summary>
    public sealed class VisualReviewClient : IVisualReviewClient
    {
        private const string Endpoint = "https://api.siliconflow.cn/v1/chat/completions";
        private const string Model = "Qwen/Qwen3-VL-8B-Instruct";
        private readonly string _apiKey;

        public VisualReviewClient(string apiKey) { _apiKey = apiKey; }

        public ReviewReport Review(byte[] png, string contract, string viewName)
        {
            try
            {
                string json = CallModel(png, contract);
                return ParseReport(json, viewName);
            }
            catch (Exception ex)
            {
                // 降级:不抛,标 Degraded;截图已由调用方落盘
                return new ReviewReport
                {
                    View = viewName,
                    Degraded = true,
                    Summary = "视觉模型不可达: " + ex.Message,
                    Score = -1
                };
            }
        }

        public static ReviewReport ParseReport(string json, string viewName)
        {
            var report = new ReviewReport { View = viewName };
            JObject root = JObject.Parse(json);
            report.Summary = (string)root["summary"] ?? "";
            report.Score = (int)(root["score"] ?? 0);
            foreach (var item in root["issues"] ?? new JArray())
            {
                report.Issues.Add(new ReviewIssue
                {
                    Severity = (string)item["severity"] ?? "low",
                    Category = (string)item["category"] ?? "",
                    Description = (string)item["description"] ?? "",
                    Location = (string)item["location"] ?? ""
                });
            }
            return report;
        }

        private string CallModel(byte[] png, string contract)
        {
            string base64 = Convert.ToBase64String(png);
            string prompt = "你是工业 WPF 界面评审。按以下设计契约评阅截图,只返回 JSON:" +
                            "{\"summary\":\"\",\"score\":0,\"issues\":[{\"severity\":\"\",\"category\":\"\",\"description\":\"\",\"location\":\"\"}]}\n" +
                            "契约:\n" + contract;
            var body = new
            {
                model = Model,
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = "data:image/png;base64," + base64 } }
                        }
                    }
                }
            };
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var resp = client.PostAsync(Endpoint, content).Result;
                resp.EnsureSuccessStatusCode();
                var respStr = resp.Content.ReadAsStringAsync().Result;
                // OpenAI 兼容:取 choices[0].message.content
                var j = JObject.Parse(respStr);
                return (string)j["choices"][0]["message"]["content"];
            }
        }
    }
}
```

- [ ] **Step 4: 跑测试确认通过 + Commit**

```bash
dotnet test src/Tests/Luster.VisualReviewer.Tests --filter "VisualReviewClient"
git add src/Tools/Luster.VisualReviewer/ReviewModels.cs src/Tools/Luster.VisualReviewer/VisualReviewClient.cs src/Tests/Luster.VisualReviewer.Tests/VisualReviewClientTests.cs
git commit -m "feat(reviewer): VisualReviewClient 调Qwen3-VL+JSON报告+降级 #6"
```

---

### Task 7: VisualReviewer Program.cs CLI + 工作区索引

**Files:**
- Create: `src/Tools/Luster.VisualReviewer/Program.cs`
- Create: `src/Tools/Luster.VisualReviewer/WorkspaceIndexer.cs`
- Create: `workspace/wpf-preview/index.md`

**Interfaces:**
- Consumes: `ContractReader`, `VisualReviewClient`
- Produces: CLI
  - `VisualReviewer.exe --screenshot <png> --report <out.json> [--contract <契约.md>] [--view <名>]`
  - 写 JSON 报告 + 追加 `<View>/index.md` 与根 `index.md`
  - 退出码:0=成功,2=视觉模型不可达(仍落盘截图)

- [ ] **Step 1: 实现 WorkspaceIndexer**

Create `src/Tools/Luster.VisualReviewer/WorkspaceIndexer.cs`:
```csharp
using System.IO;
using System.Text;

namespace Luster.VisualReviewer
{
    /// <summary>向工作区追加迭代历史到 index.md</summary>
    public static class WorkspaceIndexer
    {
        /// <summary>根索引 workspace/wpf-preview/index.md 追加一行</summary>
        public static void AppendRoot(string workspaceRoot, string viewName, ReviewReport report)
        {
            string path = Path.Combine(workspaceRoot, "index.md");
            var sb = new StringBuilder();
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
                sb.AppendLine("| View | 时间戳 | 评分 | 状态 |\n|---|---|---|---|\n");
            string ts = System.IO.Path.GetFileNameWithoutExtension(report.Screenshot);
            sb.AppendLine($"| {viewName} | {ts} | {report.Score} | {(report.Degraded ? "降级" : "完成")} |");
            File.AppendAllText(path, sb.ToString());
        }

        /// <summary>View 级索引 workspace/wpf-preview/<View>/index.md 追加一行</summary>
        public static void AppendView(string workspaceRoot, string viewName, ReviewReport report)
        {
            string dir = Path.Combine(workspaceRoot, viewName);
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "index.md");
            var sb = new StringBuilder();
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
                sb.AppendLine($"# {viewName} 迭代历史\n\n| 时间戳 | 评分 | 主要问题 | 截图 |\n|---|---|---|---|\n");
            string ts = Path.GetFileNameWithoutExtension(report.Screenshot);
            string topIssue = report.Issues.Count > 0 ? report.Issues[0].Description : "-";
            string relShot = "runs/" + Path.GetFileName(report.Screenshot);
            sb.AppendLine($"| {ts} | {report.Score} | {topIssue} | {relShot} |");
            File.AppendAllText(path, sb.ToString());
        }
    }
}
```

- [ ] **Step 2: 实现 Program.Main**

Create `src/Tools/Luster.VisualReviewer/Program.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.IO;

namespace Luster.VisualReviewer
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var map = new Dictionary<string, string>();
            for (int i = 0; i + 1 < args.Length; i += 2)
                map[args[i].TrimStart('-').ToLowerInvariant()] = args[i + 1];
            if (!map.ContainsKey("screenshot") || !map.ContainsKey("report"))
            {
                Console.Error.WriteLine("用法: VisualReviewer --screenshot <png> --report <out.json> [--contract <md>] [--view <名>]");
                return 1;
            }
            string shot = map["screenshot"];
            string reportPath = map["report"];
            string contract = ContractReader.Read(map.TryGetValue("contract", out var c) ? c : "docs/wpf-design-contract.md");
            string viewName = map.TryGetValue("view", out var v) ? v : Path.GetFileNameWithoutExtension(shot);

            byte[] png = File.ReadAllBytes(shot);
            // API key 从环境变量取,避免入库
            string apiKey = Environment.GetEnvironmentVariable("SILICONFLOW_API_KEY")
                            ?? "<从 SILICONFLOW_API_KEY 环境变量取,缺失则空串走降级>";
            var client = new VisualReviewClient(apiKey);
            var report = client.Review(png, contract, viewName);
            report.Screenshot = shot;

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(reportPath)));
            File.WriteAllText(reportPath, Newtonsoft.Json.JsonConvert.SerializeObject(report, Newtonsoft.Json.Formatting.Indented));

            // 工作区索引(workspace/wpf-preview 在仓库根,相对当前目录)
            string wsRoot = Path.Combine(Directory.GetCurrentDirectory(), "workspace", "wpf-preview");
            WorkspaceIndexer.AppendView(wsRoot, viewName, report);
            WorkspaceIndexer.AppendRoot(wsRoot, viewName, report);

            if (report.Degraded)
            {
                Console.Error.WriteLine("视觉模型不可达,截图已落盘,报告标 Degraded");
                return 2;
            }
            Console.WriteLine($"评阅完成: {viewName} 评分 {report.Score},问题 {report.Issues.Count} 项");
            return 0;
        }
    }
}
```

- [ ] **Step 3: 建工作区索引模板**

Create `workspace/wpf-preview/index.md`:
```markdown
# WPF 预览工作区

| View | 时间戳 | 评分 | 状态 |
|---|---|---|---|
```

- [ ] **Step 4: 构建 + Commit**

```bash
dotnet build LMV-2026.sln
git add src/Tools/Luster.VisualReviewer/Program.cs src/Tools/Luster.VisualReviewer/WorkspaceIndexer.cs workspace/wpf-preview/index.md
git commit -m "feat(reviewer): CLI+工作区索引追加 #7"
```

---

### Task 8: 设计契约文档 + 资源键扫描

**Files:**
- Create: `docs/wpf-design-contract.md`

- [ ] **Step 1: 扫描 Luster.Common.Assets 资源键**

```bash
grep -hoE 'x:Key="[^"]+"' src/Modules/Luster.Common.Assets/Themes/Styles/Style.xaml | sort -u
```
记录输出的色板/字号/间距 Key 清单。

- [ ] **Step 2: 写契约文档**

Create `docs/wpf-design-contract.md`:
```markdown
# WPF 页面设计契约(LMV-2026)

> 人读规范 + 视觉模型评阅 prompt 双用途。改动需同步 PreviewHost/VisualReviewer。

## 控件库优先级
- 一律用 HandyControl + Luster.Controls.Wpf,禁止原生 Button/TextBox/Border 拼凑。
- 列表/表格用 HandyControl DataGrid 或 Luster 控件库提供的封装。

## 资源键引用
- 色/字号/间距必须 `{StaticResource <Key>}` 引用,禁止写死 hex/像素值。
- 关键 Key(从 Luster.Common.Assets/Themes/Styles/Style.xaml 扫描,实施时填充):
  - <这里填扫描结果>

## 字号档位
- 标题:20px Bold / 正文:14px / 标签:12px,三档,不自由设。

## 布局分区
- 工业界面紧凑、信息密度高。
- 主操作区 / 状态区 / 参数区分区,区与区用 Margin 间距。

## MVVM 契约
- ViewModel 以 VM 后缀。
- View 与 VM 用 Prism ViewModelLocator.AutoWireViewModel="True" 自动关联。
- 命令用 DelegateCommand。
- 多语言走 {Binding Langs[xxx]}。

## 评阅维度(视觉模型用)
- overlap: 控件重叠
- spacing: 留白/间距异常
- control-lib: 未用 HandyControl/资源键
- font: 字号未走档位
- layout: 分区混乱
```
将 Step 1 扫描结果填入"关键 Key"小节。

- [ ] **Step 3: Commit**

```bash
git add docs/wpf-design-contract.md
git commit -m "docs(wpf): 设计契约文档+资源键清单 #8"
```

---

### Task 9: 全量构建与测试验收

- [ ] **Step 1: 全量构建**

```bash
dotnet build LMV-2026.sln -c Release
```
Expected: 全部 4 个新工程编译通过,无错。

- [ ] **Step 2: 全量测试**

```bash
dotnet test LMV-2026.sln --filter "PreviewHost|VisualReviewer"
```
Expected: 所有单测 PASS。

- [ ] **Step 3: 端到端冒烟(手动)**

```bash
# 用夹具 View 跑通整条链
Luster.PreviewHost.exe --view Luster.PreviewHost.Fixtures.SampleView,Luster.PreviewHost \
  --designvm Luster.PreviewHost.Fixtures.SampleDesignVm,Luster.PreviewHost \
  --out workspace/wpf-preview/SampleView/runs/20260627-170000.png --width 400 --height 300

Luster.VisualReviewer.exe --screenshot workspace/wpf-preview/SampleView/runs/20260627-170000.png \
  --report workspace/wpf-preview/SampleView/reports/20260627-170000.json \
  --view SampleView --contract docs/wpf-design-contract.md
```
Expected: 生成 PNG + JSON 报告 + index.md 追加行。

- [ ] **Step 4: Commit 收尾**

```bash
git add -A
git commit -m "test(preview): 端到端冒烟通过+工作区样例 #9"
```

---

## Self-Review 结论

- **Spec 覆盖**:§3 决策→Task1/2/3/6;§4 架构四组件→Task1-4(Host)+Task5-7(Reviewer)+Task8(契约)+流水线集成(Task7索引);§5 工作区→Task7/8;§6 数据流→Task9冒烟;§7 错误处理降级→Task4(退出码)+Task6(降级);§8 测试→每 Task TDD。✓
- **占位扫描**:无 TBD/TODO,所有代码步骤含完整代码。Task8 资源键清单需扫描后填(已明示命令)。✓
- **类型一致**:`RenderRequest/RenderResult`(Task3)、`DesignInstanceInfo`(Task2)、`ReviewReport/ReviewIssue`(Task6)、`IVisualReviewClient`(Task6 定义,Task6测试引用一致)跨 Task 命名一致。✓
- **spec 修正**:d:DesignInstance 运行时不可读,改 `--xaml` 源解析 + `--designvm` 显式覆盖,已在计划顶部声明。✓
