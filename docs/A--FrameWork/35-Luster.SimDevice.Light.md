# Luster.SimDevice.Light — 仿真光源设备

> 路径：`src/Modules/Luster.SimDevice.Light/`  
> 类型：类库（设备插件）  
> 输出：`Luster.SimDevice.Light.dll` → `Devices/` 子目录

## 项目简介

`Luster.SimDevice.Light` 是 **仿真光源设备**，模拟工业照明光源的控制功能。

## 核心职责

- **光源设备仿真** - 模拟光源设备行为
- **光源控制** - 模拟光源开关、亮度调节
- **参数配置** - 光源参数模拟
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

`Luster.SimDevice.Light.dll` → `Devices/` 子目录

**特殊属性：**
- `CopyToDevicesFolder=true` - 自动拷贝到 Devices/ 目录
