# Luster.Motion.Integration — Motion 集成层

> 路径：`src/Modules/Luster.Motion.Integration/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.Integration.dll` → exe 根目录

## 项目简介

`Luster.Motion.Integration` 是 **Motion 系统的外部集成层**，负责与第三方系统（如 MES、SFC、AOI、工卡验证等）进行通信和数据交换。该项目封装了各种外部接口的调用逻辑，为 Motion 业务模块提供统一的集成服务。

## 核心职责

- **Web 服务集成** - 与外部 Web API 进行 HTTP/HTTPS 通信（Web/）
  - RESTful API 调用
  - SOAP 服务调用
  - JSON/XML 数据序列化与反序列化
- **SFC 系统集成** - 与车间控制系统（Shop Floor Control）对接（SFC/）
  - 工单管理
  - 生产流程控制
  - 数据上报
- **AOI 系统集成** - 与自动光学检测系统对接（AOI/）
  - 检测结果获取
  - 图片数据采集（Foxconn.IMES.PictureCollection）
- **工卡验证** - 工卡扫描与验证逻辑（WorkCardVerify/）
  - 工卡号校验
  - 权限验证
- **TCP 通信** - 基于 SimpleTCP 的设备通信
- **日志记录** - 使用 Serilog 记录集成过程的详细日志

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.TaskFlow.Engine` | Motion 任务流引擎（间接引用 TaskFlow.Common 和 TaskFlow.Motion） |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Serilog` + `Serilog.Sinks.Async` + `Serilog.Sinks.File` | 结构化日志记录 |
| `MinVer` | 自动版本号生成 |

### 外部 DLL 引用

| DLL | 路径 | 用途 |
|-----|------|------|
| `Foxconn.IMES.PictureCollection.dll` | `Dll/` | 富士康 IMES 图片采集 |
| `Newtonsoft.Json.dll` | `lib/` | JSON 序列化 |
| `SimpleTCP.dll` | `lib/` | TCP 通信 |
| `System.Configuration` | .NET Framework | 配置管理 |

## 模块目录结构

```
Luster.Motion.Integration/
├── Web/                      ← Web 服务集成（10 个文件）
│   ├── HttpClient 封装
│   ├── API 调用逻辑
│   └── 数据模型
├── SFC/                      ← SFC 系统集成
│   └── 车间控制接口
├── AOI/                      ← AOI 系统集成
│   └── 检测结果处理
├── WorkCardVerify/           ← 工卡验证（3 个文件）
│   ├── 验证逻辑
│   └── 数据模型
└── Dll/                      ← 外部 DLL
    └── Foxconn.IMES.PictureCollection.dll
```

## 输出到 exe 目录

`Luster.Motion.Integration.dll` → Shell 输出目录根下

**特殊说明：**
- 该项目移除了 `Models/` 目录（已在 csproj 中排除）
- 依赖的外部 DLL（SimpleTCP、Newtonsoft.Json）会一同拷贝到输出目录
