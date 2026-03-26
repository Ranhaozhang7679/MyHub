# Luster.TaskFlow.Motion — 运动任务流

> 路径：`src/Modules/Luster.TaskFlow.Motion/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.TaskFlow.Motion.dll` → exe 根目录

## 项目简介

`Luster.TaskFlow.Motion` 是 Motion 系统的**任务流数据和配置层**，将通用任务流定义与 Motion 业务的数据访问层桥接，提供运动流程编排所需的数据模型和持久化支持。

## 核心职责

- 运动任务流的数据模型和配置定义
- 与数据访问层（FreeSql/SQLite）集成，持久化运动流程配置
- 运动参数的序列化和反序列化

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.DataAccess` | 数据库访问（FreeSql） |
| `Luster.TaskFlow.Common` | 任务流通用定义 |

## 输出到 exe 目录

`Luster.TaskFlow.Motion.dll` → Shell 输出目录根下
