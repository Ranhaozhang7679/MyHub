# Luster.SimDevice.Printer — 仿真打印机设备

> 路径：`src/Modules/Luster.SimDevice.Printer/`  
> 类型：类库（设备插件）  
> 输出：`Luster.SimDevice.Printer.dll` → `Devices/` 子目录

## 项目简介

`Luster.SimDevice.Printer` 是 **仿真打印机设备**，模拟标签打印机、条码打印机等打印设备的功能。

## 核心职责

- **打印机仿真** - 模拟打印机设备行为
- **打印控制** - 模拟打印任务执行
- **参数配置** - 打印机参数模拟
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

`Luster.SimDevice.Printer.dll` → `Devices/` 子目录

**特殊属性：**
- `CopyToDevicesFolder=true` - 自动拷贝到 Devices/ 目录
