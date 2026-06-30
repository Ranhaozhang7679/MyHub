# NuGet 包管理说明

## 概述

本项目已完成从本地 `lib/`、`libCommon/`、`packages/` 目录依赖向 GitLab NuGet 包注册中心的迁移。所有第三方和内部 DLL 现在统一通过 NuGet 还原，不再依赖本地文件目录。

## NuGet 源配置（NuGet.config）

项目使用两个 NuGet 包源：

| 源 | 地址 | 用途 |
|---|---|---|
| **gitlab** | `http://10.9.1.153:8687/api/v4/projects/33/packages/nuget/index.json` | 内部包和自定义第三方包 |
| **nuget.org** | `https://api.nuget.org/v3/index.json` | 公共开源 NuGet 包 |

### 包源映射规则

通过 `packageSourceMapping` 精确控制每个包从哪个源下载：

**从 GitLab 获取的包：**

| 包名模式 | 说明 |
|---|---|
| `Luster.*` | 所有 Luster 内部包（Prism、Assets、DataStruct 等） |
| `HandyControl` | WPF UI 控件库（版本 3.3.0.10，固定在 GitLab） |
| `Luster.HslCommunication` | HSL 通讯库 |
| `Luster.SimpleTCP` | TCP 通讯库 |
| `Luster.Aspose.Cells` / `Luster.Aspose.Words` | Aspose 文档处理库 |
| `Luster.DynamicExpresso` | 动态表达式解析库 |
| `Luster.FinSensor` | 传感器通讯库 |
| `Luster.Microsoft.Msagl.*` | 图形布局库（Msagl / Drawing / GraphViewerGdi） |
| `Luster.GvVision.*` | 机器视觉控件（AxInterop / Interop） |
| `Luster.VisionWPFControl` | 视觉 WPF 控件 |
| `Luster.MoonPdfLib` | PDF 查看库 |
| `Luster.MouseKeyboardActivityMonitor` | 键鼠钩子库 |

**从 nuget.org 获取的包：** 其余所有公共包（如 `Newtonsoft.Json`、`Serilog`、`Prism.Wpf`、`SkiaSharp` 等）。

### 其他配置项

- `allowInsecureConnections="true"` — 允许 HTTP 连接（GitLab 使用 HTTP）
- `globalPackagesFolder=".\packages"` — 包下载到项目根目录的 `packages/` 文件夹，不使用全局缓存

## 中央包版本管理（Directory.Packages.props）

项目启用了中央包版本管理（`ManagePackageVersionsCentrally`），所有 NuGet 包版本在 `Directory.Packages.props` 中统一声明。各 `.csproj` 文件只需写 `<PackageReference Include="包名" />`，不指定版本号。

包版本分为以下几类：

| 分类 | 示例 |
|---|---|
| 公共 UI/控件库 | HandyControl 3.3.0.10、LiveCharts、AvalonEdit |
| Luster 内部包 | Luster.Prism 1.0.0.6、Luster.Common.Tools 1.0.0.112 |
| Luster 第三方包（原 lib/libCommon） | Luster.Aspose.Cells 1.0.0、Luster.HslCommunication 1.0.0 等 |
| 数据库/数据访问 | FreeSql、DapperExtensions、Sqlite |
| 日志/测试/工具 | Serilog、NUnit、BenchmarkDotNet |

## 迁移记录

### 迁移内容

已将以下本地 DLL 目录的依赖迁移到 GitLab NuGet：

| 原目录 | 迁移方式 |
|---|---|
| `lib/*.dll` | 打包为 `Luster.*` NuGet 包上传到 GitLab |
| `libCommon/*.dll` | 打包为 `Luster.*` NuGet 包上传到 GitLab |

### GitLab 上的 NuGet 包清单

所有已上传到 GitLab 的包（项目 ID: 33）：

| 包名 | 版本 | 来源 |
|---|---|---|
| HandyControl | 3.3.0.10 | nuget.org 公共版，固定版本 |
| Luster.Prism | 1.0.0.6 | 内部项目构建 |
| Luster.WindowsAPICodePack | 1.0.0 | 原 libCommon/ |
| Luster.Aspose.Cells | 1.0.0 | 原 libCommon/ |
| Luster.Aspose.Words | 1.0.0 | 原 libCommon/ |
| Luster.DynamicExpresso | 1.0.0 | 原 libCommon/ |
| Luster.HslCommunication | 1.0.0 | 原 lib/ |
| Luster.SimpleTCP | 1.0.0 | 原 lib/ |
| Luster.SharpZipLib | 1.0.0 | 原 libCommon/ |
| Luster.FinSensor | 1.0.0 | 原 lib/ |
| Luster.Microsoft.Msagl | 1.0.0 | 原 lib/ |
| Luster.Microsoft.Msagl.Drawing | 1.0.0 | 原 lib/ |
| Luster.Microsoft.Msagl.GraphViewerGdi | 1.0.0 | 原 lib/ |
| Luster.GvVision.AxInterop | 1.0.0 | 原 lib/ |
| Luster.GvVision.Interop | 1.0.0 | 原 lib/ |
| Luster.VisionWPFControl | 1.0.0 | 原 lib/ |
| Luster.ICSharpCode.AvalonEdit | 1.0.0 | 原 lib/ |
| Luster.MoonPdfLib | 1.0.0 | 原 libCommon/ |
| Luster.MouseKeyboardActivityMonitor | 1.0.0 | 原 libCommon/ |
| Luster.Sentinel.Runtime | 1.0.0 | 加密锁运行时 |

### 项目文件变更

以下 `.csproj` 文件已移除 `lib/libCommon` 的 `Reference+HintPath` 引用，替换为 `PackageReference`：

| 项目文件 | 移除的引用 | 替换为 |
|---|---|---|
| Luster.Common.Tools | Aspose.Cells, Aspose.Words, SharpZipLib, DynamicExpresso, Msagl(x3), Newtonsoft.Json, NLog, SkiaSharp, SSH.NET | 对应的 PackageReference |
| Luster.Common.Assets | ICSharpCode.AvalonEdit, Newtonsoft.Json | AvalonEdit, Newtonsoft.Json |
| Luster.Motion.SubSystem | HarfBuzzSharp, SkiaSharp | 由 LiveChartsCore.SkiaSharpView 传递依赖 |
| Luster.Motion.Integration | Newtonsoft.Json, SimpleTCP | Newtonsoft.Json, Luster.SimpleTCP |
| Luster.Motion.DataStruct | HslCommunication | Luster.HslCommunication |

### 仍保留的本地引用

以下引用指向项目内本地文件（非 lib/libCommon），暂未迁移：

| 项目 | 引用 | 说明 |
|---|---|---|
| Luster.Common.Tools | `Costura64\sntl_adminapi_net_windows.dll` | 加密锁本地嵌入资源 |
| Luster.Motion.Integration | `Dll\Foxconn.IMES.PictureCollection.dll` | IMES 图片采集 DLL |
| Luster.SimDevice.Laser | `Costura64\*.dll`, `FocalSpce5000\*.dll` | 激光设备 SDK |
| Luster.SimDevice.Camera | `MVS\MvCameraControl.Net.dll` | 工业相机 SDK |

## 常用操作

### 还原 NuGet 包

```bash
dotnet restore LMV-2026.sln
```

### 清除缓存并强制重新还原

```bash
dotnet nuget locals http-cache --clear
rm -rf packages/
dotnet restore LMV-2026.sln --force-evaluate
```

### 上传新的 NuGet 包到 GitLab

1. 准备 nuspec 文件（DLL 放在同目录）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd">
  <metadata>
    <id>Luster.包名</id>
    <version>1.0.0</version>
    <authors>Luster</authors>
    <description>描述</description>
  </metadata>
  <files>
    <file src="DLL文件名.dll" target="lib\net472" />
  </files>
</package>
```

2. 打包：

```bash
scripts/migrate-nuget/nuget.exe pack 包名.nuspec -OutputDirectory output/
```

3. 上传：

```bash
scripts/migrate-nuget/nuget.exe push output/包名.1.0.0.nupkg -Source "http://10.9.1.153:8687/api/v4/projects/33/packages/nuget/index.json" -ApiKey "%NUGET_GITLAB_TOKEN%" -ConfigFile NuGet.config
```

> **注意**：必须使用 `-ConfigFile NuGet.config` 参数，否则 nuget.exe 会因 HTTP 源拒绝推送。
> `-ApiKey` 通过环境变量 `%NUGET_GITLAB_TOKEN%` 注入，请勿在文档或命令历史中硬编码明文 Token；环境变量设置方式见 `docs/NuGet-gitlab-credentials.md`（TES-186 新增）。

### 更新包版本

1. 修改 `Directory.Packages.props` 中的版本号
2. 各项目无需改动（版本集中管理）
3. 执行 `dotnet restore` 还原新版本
