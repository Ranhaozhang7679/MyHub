# Luster.SimDevice — 仿真设备基础库

> 路径：`src/Modules/Luster.SimDevice/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.SimDevice.dll` → exe 根目录

## 项目简介

`Luster.SimDevice` 是 **仿真设备基础库**，提供仿真设备的抽象基类和通用接口。所有具体的仿真设备（相机、激光、光源、运动卡等）都继承自该基础库，确保仿真设备的统一接口和行为。

## 核心职责

- **设备抽象基类** - 定义仿真设备的基础接口
  - 设备初始化接口
  - 设备控制接口
  - 设备状态查询接口
- **通用设备行为** - 仿真设备的通用功能
  - 设备生命周期管理
  - 设备参数管理
  - 设备事件通知
- **设备工厂模式** - 设备实例创建和管理
- **设备注册机制** - 设备类型注册和发现

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.DataStruct` | Motion 数据结构（设备数据模型） |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Microsoft.SourceLink.GitLab` | 源代码链接（调试支持） |
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.SimDevice.dll` → Shell 输出目录根下

**特殊说明：**
- 该项目作为 NuGet 内部包（`GeneratePackageOnBuild=True`）
- 被所有具体的仿真设备项目引用
- 定义了仿真设备的统一接口规范
