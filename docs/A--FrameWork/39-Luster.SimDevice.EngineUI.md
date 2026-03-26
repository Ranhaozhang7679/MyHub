# Luster.SimDevice.EngineUI — 仿真设备引擎 UI

> 路径：`src/Modules/Luster.SimDevice.EngineUI/`  
> 类型：类库（设备插件）  
> 输出：`Luster.SimDevice.EngineUI.dll` → `Devices/` 子目录

## 项目简介

`Luster.SimDevice.EngineUI` 是 **仿真设备引擎 UI 模块**，提供仿真设备的管理界面，包括设备列表、设备状态监控、设备配置等功能。

## 核心职责

- **设备管理界面** - 仿真设备列表和管理
- **设备状态监控** - 实时显示设备状态
- **设备配置界面** - 设备参数配置UI
- **设备控制面板** - 设备操作控制界面
- **WPF UI 组件** - 提供可复用的设备 UI 控件

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.SimDevice` | 仿真设备基础库 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.SimDevice.EngineUI.dll` → `Devices/` 子目录

**特殊属性：**
- `CopyToDevicesFolder=true` - 自动拷贝到 Devices/ 目录
- `UseWPF=true` - WPF 项目
