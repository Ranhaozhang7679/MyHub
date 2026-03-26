# Luster.SimDevice.Laser — 仿真激光设备

> 路径：`src/Modules/Luster.SimDevice.Laser/`  
> 类型：类库（设备插件）  
> 输出：`Luster.SimDevice.Laser.dll` → `Devices/` 子目录

## 项目简介

`Luster.SimDevice.Laser` 是 **仿真激光设备**，模拟激光打标、激光测距等激光设备的功能。

## 核心职责

- **激光设备仿真** - 模拟激光设备行为
- **激光控制** - 模拟激光开关、功率控制
- **参数配置** - 激光参数模拟
- **数据访问** - 配置数据持久化

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.DataAccess` | 数据访问层 |
| `Luster.SimDevice` | 仿真设备基础库 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Costura.Fody` | 嵌入 native DLL |
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.SimDevice.Laser.dll` → `Devices/` 子目录

**特殊属性：**
- `CopyToDevicesFolder=true` - 自动拷贝到 Devices/ 目录
