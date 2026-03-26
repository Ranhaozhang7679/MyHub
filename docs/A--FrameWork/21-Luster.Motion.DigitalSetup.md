# Luster.Motion.DigitalSetup — Motion 数字化设置

> 路径：`src/Modules/Luster.Motion.DigitalSetup/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.DigitalSetup.dll` → exe 根目录

## 项目简介

`Luster.Motion.DigitalSetup` 是 **Motion 系统的数字化设置模块**，提供仿真设备配置、数字孟生设置等功能。该模块集成了仿真设备 UI 和 Motion 编辑器 UI，作为 Prism 模块加载到主程序。

## 核心职责

- **数字化设置界面** - 提供数字化配置工具
- **仿真设备集成** - 集成 SimDevice.EngineUI 和 SimDevice.SubSystem
  - 仿真设备引擎 UI
  - 仿真设备子系统
- **数据可视化** - 使用 LiveChartsCore 展示数据
  - LiveChartsCore.SkiaSharpView.WPF（新一代图表库）
- **数据访问** - 通过 DataAccess 层访问数据库
- **编辑器集成** - 集成 Motion.EditorUI 的编辑功能

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.DataAccess` | 数据访问层 |
| `Luster.SimDevice.EngineUI` | 仿真设备引擎 UI |
| `Luster.SimDevice.SubSystem` | 仿真设备子系统 |
| `Luster.Motion.Assests` | Motion 资源包 |
| `Luster.Motion.CommonUI` | Motion 通用 UI 组件 |
| `Luster.Motion.EditorUI` | Motion 编辑器 UI |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架 |
| `HandyControl` | WPF UI 控件库 |
| `LiveChartsCore` + `LiveChartsCore.SkiaSharpView.WPF` | 新一代图表组件 |
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.Motion.DigitalSetup.dll` → Shell 输出目录根下

**特殊说明：**
- 该模块是 Motion 系统与仿真设备系统的桥梁
- 集成了多个 UI 模块，提供统一的数字化设置界面
