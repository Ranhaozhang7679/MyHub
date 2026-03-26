# Luster.SimDevice.Camera — 仿真相机设备

> 路径：`src/Modules/Luster.SimDevice.Camera/`  
> 类型：类库（设备插件）  
> 输出：`Luster.SimDevice.Camera.dll` → `Devices/` 子目录

## 项目简介

`Luster.SimDevice.Camera` 是 **仿真相机设备**，模拟工业相机的采集和控制功能。该项目集成了海康威视 MVS SDK，使用 Costura.Fody 将 native DLL 嵌入到程序集中。

## 核心职责

- **相机仿真** - 模拟工业相机行为
- **图像采集** - 模拟图像采集过程
- **MVS SDK 集成** - 集成海康威视相机 SDK
- **参数配置** - 相机参数模拟
- **数据访问** - 通过 DataAccess 层存储配置

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.DataAccess` | 数据访问层 |
| `Luster.SimDevice` | 仿真设备基础库 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Costura.Fody` | 嵌入 native DLL 到程序集 |
| `MinVer` | 自动版本号生成 |

### 外部 DLL 引用

| DLL | 路径 | 用途 |
|-----|------|------|
| `MvCameraControl.Net.dll` | `MVS/` | 海康威视相机 SDK |

## 输出到 exe 目录

`Luster.SimDevice.Camera.dll` → `Devices/` 子目录

**特殊属性：**
- `CopyToDevicesFolder=true` - 自动拷贝到 Devices/ 目录
- `PlatformTarget=x64` - 64位平台
- `AllowUnsafeBlocks=True` - 允许不安全代码（用于 native 互操作）
- 使用 Costura.Fody 嵌入 MVS SDK 的 native DLL
