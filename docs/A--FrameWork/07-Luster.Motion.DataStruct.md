# Luster.Motion.DataStruct — Motion 数据结构

> 路径：`src/Modules/Luster.Motion.DataStruct/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.DataStruct.dll` → exe 根目录

## 项目简介

`Luster.Motion.DataStruct` 定义了 **Motion 系统专用**的数据结构，包括设备状态枚举、运动参数模型、HslCommunication 通信协议结构等。它构建在 `Common.Tools` 之上，是 Motion 业务逻辑的数据基础。

## 核心职责

- 定义设备引擎状态枚举（`DeviceEngineStatus` 等）
- Motion 运行参数的实体模型
- PLC / HslCommunication 通信数据结构
- 作为 `SimDevice` 和 `DataAccess` 的数据定义层

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.Tools` | 通用工具库 |

### 本地 DLL 引用

| DLL | 用途 |
|-----|------|
| `HslCommunication` | PLC/工业通信协议库 |

## 输出到 exe 目录

`Luster.Motion.DataStruct.dll` → Shell 输出目录根下
