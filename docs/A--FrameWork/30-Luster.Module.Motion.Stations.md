# Luster.Module.Motion.Stations — Motion 工位插件

> 路径：`src/Modules/Luster.Module.Motion.Stations/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.Stations.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.Stations` 是 **Motion 工位管理插件**，提供工位配置、工位控制和工位状态管理功能。该插件用于管理生产线上的各个工位。

## 核心职责

- **工位配置** - 工位参数和布局配置
- **工位控制** - 工位启停和运行控制
- **工位状态** - 工位状态监控和管理
- **工位调度** - 工位任务调度
- **任务流集成** - 工位任务节点

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.Module.Motion.Stations.dll` → `Motions/` 子目录
