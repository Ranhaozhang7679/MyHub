# Luster.SimDevice.SubSystem — 仿真设备子系统

> 路径：`src/Modules/Luster.SimDevice.SubSystem/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.SimDevice.SubSystem.dll` → exe 根目录

## 项目简介

`Luster.SimDevice.SubSystem` 是 **仿真设备子系统**，提供仿真设备的业务逻辑层，连接仿真设备引擎、Motion 业务模块和任务流系统。该模块作为 Prism 模块加载到主程序。

## 核心职责

- **仿真设备业务层** - 仿真设备业务逻辑封装
- **设备集成** - 集成多个仿真设备模块
  - SimDevice.EngineUI（设备引擎 UI）
  - SimDevice.Laser（激光设备）
- **Motion 业务集成** - 与 Motion.Business 协同工作
- **任务流集成** - 与 TaskFlow.Motion 集成
- **编辑器集成** - 与 Motion.EditorUI 集成
- **资源管理** - 使用 Common.Assets 和 Motion.Assests

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Module.Motion.Business` | Motion 业务逻辑 |
| `Luster.Motion.EditorUI` | Motion 编辑器 UI |
| `Luster.Motion.TaskFlow.Engine` | Motion 任务流引擎 |
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |
| `Luster.Common.Assets` | 通用资源 |
| `Luster.SimDevice.EngineUI` | 仿真设备引擎 UI |
| `Luster.SimDevice.Laser` | 仿真激光设备 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架（模块化） |
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.SimDevice.SubSystem.dll` → Shell 输出目录根下

**特殊说明：**
- 该项目作为 NuGet 内部包（`GeneratePackageOnBuild=True`）
- 作为 Prism 模块加载，提供仿真设备的完整业务功能
- 连接了仿真设备层和 Motion 业务层
