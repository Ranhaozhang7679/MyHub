# Luster.Module.Motion.Device — Motion 设备插件

> 路径：`src/Modules/Luster.Module.Motion.Device/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.Device.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.Device` 是 **Motion 设备管理插件**，提供设备驱动管理、设备控制和设备状态监控功能。该插件依赖 TaiKeCommon 库和 Logic 插件。

## 核心职责

- **设备驱动管理** - 设备驱动加载和管理
- **设备控制** - 设备操作和控制接口
- **设备状态监控** - 设备状态实时监控
- **设备配置** - 设备参数配置和管理
- **逻辑控制集成** - 与 Logic 插件协同工作

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `TaiKeCommon` | 泰科通用库 |
| `Luster.Module.Motion.Logic` | Motion 逻辑控制插件 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `MinVer` | 自动版本号生成 |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `System.ComponentModel.DataAnnotations` | 数据验证注解 |

## 输出到 exe 目录

`Luster.Module.Motion.Device.dll` → `Motions/` 子目录
