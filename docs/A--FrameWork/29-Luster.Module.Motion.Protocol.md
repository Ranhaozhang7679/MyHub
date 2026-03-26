# Luster.Module.Motion.Protocol — Motion 协议插件

> 路径：`src/Modules/Luster.Module.Motion.Protocol/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.Protocol.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.Protocol` 是 **Motion 通信协议插件**，提供设备通信协议的实现，包括串口通信、TCP/IP 通信、Modbus 等协议支持。

## 核心职责

- **串口通信** - RS232/RS485 串口协议
- **TCP/IP 通信** - 网络通信协议
- **Modbus 协议** - Modbus RTU/TCP 协议
- **自定义协议** - 设备专用通信协议
- **协议解析** - 数据包解析和封装
- **任务流集成** - 通信任务节点

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `MinVer` | 自动版本号生成 |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `System.ComponentModel.DataAnnotations` | 数据验证注解 |

## 输出到 exe 目录

`Luster.Module.Motion.Protocol.dll` → `Motions/` 子目录
