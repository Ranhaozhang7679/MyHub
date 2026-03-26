# Luster.Module.Motion.Logic — Motion 逻辑插件

> 路径：`src/Modules/Luster.Module.Motion.Logic/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.Logic.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.Logic` 是 **Motion 逻辑控制插件**，提供核心逻辑控制功能，包括状态机、流程控制、条件判断等。该插件使用 Prism IoC 容器进行依赖注入。

## 核心职责

- **状态机控制** - 设备和流程状态管理
- **流程控制** - 业务流程逻辑控制
- **条件判断** - 逻辑条件评估
- **事件处理** - 事件驱动逻辑
- **Prism IoC** - 依赖注入和服务定位

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架（IoC 容器） |
| `MinVer` | 自动版本号生成 |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `System.ComponentModel.DataAnnotations` | 数据验证注解 |

## 输出到 exe 目录

`Luster.Module.Motion.Logic.dll` → `Motions/` 子目录
