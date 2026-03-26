# Luster.TaskFlow.Common — 任务流通用定义

> 路径：`src/Modules/Luster.TaskFlow.Common/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.TaskFlow.Common.dll` → exe 根目录

## 项目简介

`Luster.TaskFlow.Common` 定义了**任务流引擎的通用接口和数据结构**，是所有 TaskFlow 相关项目的公共基础层。

## 核心职责

- 任务流节点、连接、执行上下文等接口定义
- 任务流配置和序列化的数据模型
- 通用的任务流枚举和常量

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.Tools` | 通用工具库 |

## 输出到 exe 目录

`Luster.TaskFlow.Common.dll` → Shell 输出目录根下
