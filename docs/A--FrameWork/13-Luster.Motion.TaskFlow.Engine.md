# Luster.Motion.TaskFlow.Engine — Motion 任务流引擎

> 路径：`src/Modules/Luster.Motion.TaskFlow.Engine/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.TaskFlow.Engine.dll` → exe 根目录

## 项目简介

`Luster.Motion.TaskFlow.Engine` 扩展了通用任务流引擎，为 Motion 系统提供**专用的任务流引擎能力**，包括 Prism IoC 集成和 Web 服务接口（`IWebService`）。

## 核心职责

- Motion 专用的任务流编排与执行引擎
- Prism IoC 容器集成（`Luster.Prism` NuGet 包）
- Web 服务接口定义（HyperTrain `IWebService`）
- 桥接通用 TaskFlow 组件与 Motion 业务

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.TaskFlow.Common` | 任务流通用定义 |
| `Luster.TaskFlow.Motion` | 运动任务流数据 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | IoC 容器集成 |

## 输出到 exe 目录

`Luster.Motion.TaskFlow.Engine.dll` → Shell 输出目录根下
