# Luster.Motion.CommonUI — Motion 通用 UI 组件

> 路径：`src/Modules/Luster.Motion.CommonUI/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.CommonUI.dll` → exe 根目录

## 项目简介

`Luster.Motion.CommonUI` 是 **Motion 系统的通用 UI 组件库**，提供可复用的视图、对话框、用户控件和 ViewModel。该项目是 Motion UI 层的核心基础，被其他 UI 模块（AlarmUI、EditorUI、ReportUI 等）广泛引用。

## 核心职责

- **通用对话框** - 提供各种业务对话框（Views/Dialogs/）
  - 设备配置对话框（DeviceDialog）
  - 项目创建对话框（CreateProjectDialog）
  - 暂停对话框（HivePauseDialog）
  - 光幕对话框（LightCurtainDialog）
  - PLC 报警对话框（PlcAlarmDialog）
  - 输出图表对话框（SetOutPutChartDialog）
  - C# 代码编辑器（CSharpEditor）
- **日志查看器** - 日志内容展示控件（LogContent.xaml）
- **图表组件** - 基于 LiveCharts 的数据可视化
  - LiveCharts.Wpf（传统版本）
  - LiveChartsCore.SkiaSharpView.WPF（新版本）
- **代码编辑** - 基于 AvalonEdit 和 Roslyn 的 C# 脚本编辑器
  - 语法高亮
  - 代码补全（Microsoft.CodeAnalysis.CSharp.Scripting）
  - 实时编译验证
- **QR 码生成** - 使用 QRCoder 生成二维码
- **文件对话框** - 使用 WindowsAPICodePack 提供现代文件选择器
- **数据库访问** - 集成 FreeSql ORM

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.Integration` | 外部系统集成 |
| `Luster.Motion.TaskFlow.Engine` | Motion 任务流引擎 |
| `Luster.Common.Assets` | 通用资源 |
| `Luster.Control.Wpf.Motion` | Motion WPF 控件 |
| `Luster.Motion.Assests` | Motion 资源包 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架（IoC、MVVM、导航） |
| `HandyControl` | WPF UI 控件库 |
| `LiveCharts.Wpf` + `LiveChartsCore.SkiaSharpView.WPF` | 图表组件 |
| `AvalonEdit` | 代码编辑器控件 |
| `Microsoft.CodeAnalysis.CSharp.Scripting` | C# 脚本编译 |
| `QRCoder` | 二维码生成 |
| `FreeSql` | ORM 数据库访问 |
| `Luster.WindowsAPICodePack` | Windows API 封装 |
| `MinVer` | 自动版本号生成 |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `Microsoft.VisualBasic` | VB 运行时支持 |
| `System.Windows.Forms` | WinForms 互操作 |

## 输出到 exe 目录

`Luster.Motion.CommonUI.dll` → Shell 输出目录根下

**特殊说明：**
- 该项目移除了 `Resources/` 目录（已在 csproj 中排除）
- 包含丰富的 XAML 视图和对话框资源
- 提供 C# 脚本运行时环境（Roslyn）
