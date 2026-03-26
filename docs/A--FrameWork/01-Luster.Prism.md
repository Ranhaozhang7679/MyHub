# Luster.Prism — 基础框架层

> 路径：`src/Infrastructure/Luster.Prism/`  
> 类型：类库（NuGet 内部包）  
> 输出：`Luster.Prism.dll` → 作为 NuGet 包被各模块引用

## 项目简介

`Luster.Prism` 是整个解决方案的**基础设施层**，封装了 **Prism（DryIoc 容器）** 框架，并整合了日志（Serilog）与配置（Microsoft.Extensions.Configuration）系统。所有需要 IoC 容器、模块化加载、日志或配置的项目均通过 NuGet 方式引用此包。

## 核心职责

- 封装 **Prism.DryIoc** 和 **Prism.Wpf** 的模块化加载机制
- 提供统一的 **Serilog 日志** 管道（Console、File、Debug 多 Sink）
- 整合 **Microsoft.Extensions.Logging** 泛型 `ILogger<T>` 接口
- 提供 **Microsoft.Extensions.Configuration** 统一配置系统（JSON、环境变量、UserSecrets）

## 依赖关系

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Prism.DryIoc` | IoC 容器 + 模块化框架 |
| `Prism.Wpf` | WPF 导航、Region、Dialog 支持 |
| `Serilog` + 多个 Sinks | 结构化日志 |
| `Microsoft.Extensions.Logging` | 泛型日志接口 |
| `Microsoft.Extensions.Configuration.*` | 配置系统（JSON / 环境变量 / UserSecrets） |

### 项目引用

无（最底层基础包）

## 输出到 exe 目录

`Luster.Prism.dll` 通过 NuGet 包引用传递到所有依赖项目，最终与其 NuGet 依赖一同出现在 Shell 输出目录根下。
