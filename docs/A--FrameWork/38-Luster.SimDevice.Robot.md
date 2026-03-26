# Luster.SimDevice.Robot — 仿真机器人设备

> 路径：`src/Modules/Luster.SimDevice.Robot/`  
> 类型：类库（设备插件）  
> 输出：`Luster.SimDevice.Robot.dll` → `Devices/` 子目录

## 项目简介

`Luster.SimDevice.Robot` 是 **仿真机器人设备**，模拟工业机器人的运动控制和操作功能。

## 核心职责

- **机器人仿真** - 模拟机器人设备行为
- **运动控制** - 模拟机器人运动轨迹
- **姿态控制** - 模拟机器人姿态调整
- **参数配置** - 机器人参数模拟

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.SimDevice` | 仿真设备基础库 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|\n| `Costura.Fody` | 嵌入 native DLL |
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.SimDevice.Robot.dll` → `Devices/` 子目录

**特殊属性：**
- `CopyToDevicesFolder=true` - 自动拷贝到 Devices/ 目录
