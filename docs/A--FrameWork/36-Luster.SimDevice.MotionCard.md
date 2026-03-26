# Luster.SimDevice.MotionCard — 仿真运动卡设备

> 路径：`src/Modules/Luster.SimDevice.MotionCard/`  
> 类型：类库（设备插件）  
> 输出：`Luster.SimDevice.MotionCard.dll` → `Devices/` 子目录

## 项目简介

`Luster.SimDevice.MotionCard` 是 **仿真运动卡设备**，模拟运动控制卡的功能，包括轴控制、IO 控制等。该项目会拷贝 `CardErrorCode/` 目录到输出目录。

## 核心职责

- **运动卡仿真** - 模拟运动控制卡行为
- **轴控制** - 模拟多轴运动控制
- **IO 控制** - 模拟数字 IO 和模拟 IO
- **错误码管理** - 运动卡错误码定义
- **参数配置** - 运动卡参数模拟

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.SimDevice` | 仿真设备基础库 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Costura.Fody` | 嵌入 native DLL |
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.SimDevice.MotionCard.dll` → `Devices/` 子目录

**特殊属性：**
- `CopyToDevicesFolder=true` - 自动拷贝到 Devices/ 目录
- `CopyErrorCodeFolder=true` - 拷贝 `CardErrorCode/` 目录到输出根目录
