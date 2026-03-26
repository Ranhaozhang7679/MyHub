# Luster.TaskFlow.Engine — 通用任务流引擎

> 路径：`src/Modules/Luster.TaskFlow.Engine/`  
> 类型：类库  
> 输出：`Luster.TaskFlow.Engine.dll` → exe 根目录

## 项目简介

`Luster.TaskFlow.Engine` 是**通用任务流执行引擎**，提供任务编排、调度和执行功能。整合了 3D 算法能力，支持复杂的多步骤自动化流程。

## 核心职责

- 任务流的解析与执行引擎
- 节点编排和调度管理
- 与 3D 算法模块集成，支持视觉和测量相关的任务节点

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.ThreeD.Algorithm` | 3D 几何算法 |
| `Luster.TaskFlow.Common` | 任务流通用定义 |

## 输出到 exe 目录

`Luster.TaskFlow.Engine.dll` → Shell 输出目录根下
