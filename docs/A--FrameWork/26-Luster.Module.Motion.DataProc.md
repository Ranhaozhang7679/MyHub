# Luster.Module.Motion.DataProc — Motion 数据处理插件

> 路径：`src/Modules/Luster.Module.Motion.DataProc/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.DataProc.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.DataProc` 是 **Motion 数据处理插件**，提供数据采集、处理、分析和存储功能。该插件负责生产数据的收集、转换和持久化。

## 核心职责

- **数据采集** - 从设备和传感器采集数据
- **数据处理** - 数据清洗、转换和计算
- **数据分析** - 统计分析和趋势分析
- **数据存储** - 数据持久化到数据库
- **任务流集成** - 数据处理任务节点

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.Integration` | Motion 集成层 |
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.Module.Motion.DataProc.dll` → `Motions/` 子目录
