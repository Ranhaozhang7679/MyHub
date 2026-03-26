# Luster.Module.Motion.Business — Motion 业务逻辑插件

> 路径：`src/Modules/Luster.Module.Motion.Business/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.Business.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.Business` 是 **Motion 业务逻辑插件**，封装了核心业务流程和业务规则，包括生产流程控制、数据处理、外部系统交互等。该插件作为业务层核心，连接了任务流、UI 和外部集成系统。

## 核心职责

- **生产流程控制** - 生产业务流程管理
  - 工单管理
  - 流程调度
  - 状态机控制
- **业务规则引擎** - 业务逻辑处理
  - 数据验证
  - 业务规则执行
  - 异常处理
- **协议集成** - 与 Protocol 插件协同工作
  - 设备通信协议
  - 数据格式转换
- **外部系统交互** - 通过 Integration 层对接外部系统
  - MES 数据上报
  - SFC 流程控制
  - Web 服务调用
- **UI 交互** - 提供业务相关的 UI 组件
  - 业务对话框
  - 数据展示控件

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.Integration` | 外部系统集成（设置为 `Private=False`） |
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |
| `Luster.Motion.CommonUI` | Motion 通用 UI 组件 |
| `Luster.Module.Motion.Protocol` | Motion 协议插件 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `MinVer` | 自动版本号生成 |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `System.Web` | Web 相关功能（HTTP 编码等） |

## 输出到 exe 目录

`Luster.Module.Motion.Business.dll` → `Motions/` 子目录

**特殊属性：**
- `CopyToMotionsFolder=true` - 自动拷贝到 Motions/ 目录
- `Motion.Integration` 引用设置为 `Private=False`，避免重复拷贝依赖
