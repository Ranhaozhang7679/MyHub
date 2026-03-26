# Luster.Controls.Wpf — WPF 自定义控件库

> 路径：`src/Modules/Luster.Controls.Wpf/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Controls.Wpf.dll` → exe 根目录

## 项目简介

`Luster.Controls.Wpf` 提供一组**通用 WPF 自定义控件**，包括增强列表控件、树形控件、画布拖拽控件等，供上层 UI 项目复用。

## 核心职责

- **ListEx**：增强列表控件（带自定义样式和行为）
- **TreeEx**：增强树形控件（支持拖拽、多选等）
- **CanvasDrag**：画布拖拽控件（用于可视化编辑器）
- **Themes**：通用主题样式文件

## 依赖关系

### 项目引用

无

### 系统引用

`PresentationCore`, `PresentationFramework`, `System.Xaml`, `WindowsBase`（WPF 基础框架）

## 输出到 exe 目录

`Luster.Controls.Wpf.dll` → Shell 输出目录根下
