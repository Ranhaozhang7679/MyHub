# Luster.Control.Wpf.Motion — Motion WPF 控件库

> 路径：`src/Modules/Luster.Control.Wpf.Motion/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Control.Wpf.Motion.dll` → exe 根目录

## 项目简介

`Luster.Control.Wpf.Motion` 是 **Motion 系统专用的 WPF 自定义控件库**，提供与运动控制、任务流相关的可视化控件。这些控件被 Motion UI 模块广泛使用，用于构建任务流编辑器、设备监控界面等。

## 核心职责

- **任务流可视化控件** - 提供任务流图形化编辑所需的自定义控件
  - 节点控件（TaskNode）
  - 连接线控件（Connection）
  - 画布控件（FlowCanvas）
- **运动控制控件** - 运动参数设置、轴状态显示等专用控件
- **数据绑定支持** - 与 TaskFlow.Motion 数据模型深度集成
- **MVVM 架构** - 所有控件遵循 MVVM 模式，支持数据绑定和命令

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |
| `Luster.Motion.Assests` | Motion 资源包（主题、图标） |

### NuGet 包依赖

无（仅依赖 WPF 框架）

## 输出到 exe 目录

`Luster.Control.Wpf.Motion.dll` → Shell 输出目录根下

**特殊说明：**
- 该项目移除了 `Resources/` 目录（已在 csproj 中排除）
- 所有 XAML 控件模板嵌入到 DLL 中
