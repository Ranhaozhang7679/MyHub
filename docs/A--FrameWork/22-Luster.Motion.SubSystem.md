# Luster.Motion.SubSystem — Motion 子系统

> 路径：`src/Modules/Luster.Motion.SubSystem/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.SubSystem.dll` → exe 根目录

## 项目简介

`Luster.Motion.SubSystem` 是 **Motion 系统的主子系统模块**，集成了工作流管理、产品信息、视觉配置、软件配置、图表展示等核心业务功能。该模块作为 Prism 模块加载到主程序，是 Motion 系统的核心业务层，连接了仿真设备、任务流编辑器和外部集成系统。

## 核心职责

- **工作流管理** - 任务流程管理界面（WorkFlowContent.xaml）
  - 流程编排
  - 流程监控
  - 流程调试
- **产品信息管理** - 产品数据管理界面（ProductInfoContent.xaml）
  - 产品参数配置
  - 产品数据查询
  - 产品信息展示
- **视觉配置** - 视觉系统配置界面（VisionConfig.xaml）
  - 相机参数设置
  - 视觉算法配置
  - 图像处理参数
- **软件配置** - 软件参数配置界面（SoftConfigureContent.xaml）
  - 系统参数设置
  - 模块配置（ModuleConfigureDialog）
  - 运行环境配置
- **数据可视化** - 多种实时图表展示
  - 扭矩图表（ChartTorque2Content）
  - 力值图表（ChartToeInForceContent）
  - 心跳监控（HeartMonitorContent）
  - 数据复制图表（ChartCopyContent）
- **二维码扫描** - 使用 ZXing.Net 进行条码/二维码识别
  - 工卡扫描
  - 产品条码识别
  - 数据自动填充
- **仿真设备集成** - 集成仿真设备引擎和子系统
  - SimDevice.EngineUI（仿真设备引擎 UI）
  - SimDevice.SubSystem（仿真设备子系统）
- **外部系统集成** - 通过 Motion.Integration 对接外部系统
  - MES 系统集成
  - SFC 系统集成
  - TaiKe 设备集成
- **用户管理** - 用户界面和权限管理（UserContent.xaml）
- **确认按钮组件** - 通用确认按钮控件（ConfirmBtnContent.xaml）

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.Integration` | 外部系统集成层 |
| `TaiKeCommon` | 泰科通用库 |
| `Luster.SimDevice.EngineUI` | 仿真设备引擎 UI |
| `Luster.SimDevice.SubSystem` | 仿真设备子系统 |
| `Luster.Common.Assets` | 通用资源 |
| `Luster.Motion.Assests` | Motion 资源包 |
| `Luster.Motion.EditorUI` | Motion 编辑器 UI |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架（模块化、MVVM） |
| `HandyControl` | WPF UI 控件库 |
| `Dirkster.AvalonDock` | 停靠窗口布局系统 |
| `LiveChartsCore` + `LiveChartsCore.SkiaSharpView.WPF` | 新一代图表组件 |
| `ZXing.Net` | 二维码/条码识别库 |
| `Luster.WindowsAPICodePack` | Windows API 封装 |
| `MinVer` | 自动版本号生成 |

### 外部 DLL 引用

| DLL | 路径 | 用途 |
|-----|------|------|
| `HarfBuzzSharp.dll` | `lib/` | 文本渲染引擎 |
| `SkiaSharp.dll` | `lib/` | 2D 图形库（用于 LiveCharts） |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `Microsoft.VisualBasic` | VB 运行时支持 |
| `System.Management` | 系统管理功能 |
| `System.Windows.Forms` | WinForms 互操作 |

## 模块目录结构

```
Luster.Motion.SubSystem/
├── Views/
│   ├── WorkFlowContent.xaml          ← 工作流管理
│   ├── ProductInfoContent.xaml       ← 产品信息
│   ├── VisionConfig.xaml             ← 视觉配置
│   ├── SoftConfigureContent.xaml     ← 软件配置
│   ├── ChartTorque2Content.xaml      ← 扭矩图表
│   ├── ChartToeInForceContent.xaml   ← 力值图表
│   ├── HeartMonitorContent.xaml      ← 心跳监控
│   ├── UserContent.xaml              ← 用户管理
│   ├── ConfirmBtnContent.xaml        ← 确认按钮
│   └── Dialog/
│       ├── ModuleConfigureDialog.xaml ← 模块配置对话框
│       └── HiveDialog.xaml            ← Hive 对话框
└── Converter/                         ← 已移除（在 csproj 中排除）
```

## 输出到 exe 目录

`Luster.Motion.SubSystem.dll` → Shell 输出目录根下

**特殊说明：**
- 该模块是 Motion 系统的核心业务层，集成了大量子模块
- 移除了 `Converter/` 目录和部分视图文件（已在 csproj 中排除）
- 依赖的 SkiaSharp.dll 设置为 `Private=False`，由其他模块提供
- 包含丰富的业务视图和对话框，是用户交互的主要入口
