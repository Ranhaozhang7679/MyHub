# Luster.Motion.EditorUI — Motion 编辑器 UI

> 路径：`src/Modules/Luster.Motion.EditorUI/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.EditorUI.dll` → exe 根目录

## 项目简介

`Luster.Motion.EditorUI` 是 **Motion 系统的任务流编辑器 UI 模块**，提供任务流的图形化编辑、轴参数配置、工位管理等功能。该模块是 Motion 系统的核心编辑工具，作为 Prism 模块集成到主程序。

## 核心职责

- **任务流编辑器** - 图形化任务流编辑界面
  - 拖拽式节点编辑
  - 连接线绘制
  - 节点属性配置
- **轴参数配置** - 运动轴参数设置（Views/AxisSetContent.xaml）
  - 轴位置配置对话框（AxisPosDialog）
  - 轴配置对话框（AxisConfigDialog）
- **工位管理** - 工位配置对话框（StationDialog）
- **全局变量设置** - 全局变量和模式变量配置
  - SetGlobalVarDialog
  - SetModeGlobalVarDialog
- **运行模式设置** - SetRunModeDialog
- **Prism 模块化** - 作为独立模块集成

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.TaskFlow.Engine` | Motion 任务流引擎 |
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |
| `Luster.Common.Assets` | 通用资源 |
| `Luster.Control.Wpf.Motion` | Motion WPF 控件 |
| `Luster.Motion.Assests` | Motion 资源包 |
| `Luster.Motion.CommonUI` | Motion 通用 UI 组件 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架 |
| `HandyControl` | WPF UI 控件库 |
| `MinVer` | 自动版本号生成 |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `Microsoft.VisualBasic` | VB 运行时支持 |
| `System.Windows.Forms` | WinForms 互操作 |

## 输出到 exe 目录

`Luster.Motion.EditorUI.dll` → Shell 输出目录根下
