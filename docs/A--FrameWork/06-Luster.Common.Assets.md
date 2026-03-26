# Luster.Common.Assets — 通用 UI 资源库

> 路径：`src/Modules/Luster.Common.Assets/`  
> 类型：WPF 类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Common.Assets.dll` → exe 根目录

## 项目简介

`Luster.Common.Assets` 提供整个应用的**通用 UI 资源层**，包括主题系统（明/暗主题切换）、颜色方案、字体、样式模板、多语言支持（中/英文）、以及通用对话框组件（消息、文本输入、计算器、表达式编辑器等）。

## 核心职责

- **主题系统**：`SkinDefault.xaml` / `SkinDark.xaml` 双主题切换
- **颜色与画笔**：`Colors.xaml` / `ColorsDark.xaml` / `Brushes.xaml`
- **字体 & 图标**：自定义 `iconfont.ttf` 字体图标
- **样式模板**：统一的控件 Style 定义
- **多语言**：中文（`Lang.resx`）/ 英文（`Lang.en.resx`），T4 模板生成 `LangProvider`
- **通用对话框**：`MessageDialog`, `TextDialog`, `CalcDialog`, `ExpDialog`, `StringMatchDialog`
- **参数网格**：`ParamGrid` 参数编辑控件
- **窗口**：`DialogNonClientArea`（自定义窗口标题栏）

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Controls.Wpf` | 自定义 WPF 控件 |
| `Luster.TaskFlow.Common` | 任务流通用定义 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `HandyControl` | WPF 增强控件库 |
| `Luster.Prism` | IoC 容器 + 模块化 |
| `Costura.Fody` | 嵌入 native DLL |

### 本地 DLL 引用

| DLL | 用途 |
|-----|------|
| `ICSharpCode.AvalonEdit` | 代码编辑器控件 |

## 输出到 exe 目录

`Luster.Common.Assets.dll` → Shell 输出目录根下  
多语言资源文件 → `zh-Hans/` 等语言子目录
