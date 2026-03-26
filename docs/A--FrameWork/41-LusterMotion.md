# LusterMotion — 主程序（Shell）

> 路径：`src/Shell/LusterMotion/`  
> 类型：WinExe（可执行程序）  
> 输出：`LusterMotion.exe` → exe 根目录

## 项目简介

`LusterMotion` 是 **Luster Motion 系统的主程序（Shell）**，作为整个应用程序的入口点，负责初始化 Prism 框架、加载所有模块、配置 IoC 容器，并启动主窗口。该项目使用 Prism.DryIoc 实现模块化架构。

## 核心职责

- **应用程序入口** - Main 方法和应用程序启动
- **Prism 框架初始化** - 配置 Prism.DryIoc 容器
- **模块加载** - 加载所有 Prism 模块
  - Motion UI 模块（AlarmUI, EditorUI, ReportUI 等）
  - Motion SubSystem
  - SimDevice SubSystem
- **依赖注入配置** - 配置 DryIoc 容器和服务注册
- **主窗口启动** - 启动 WPF 主窗口
- **全局配置** - 加载应用程序配置
- **日志初始化** - 初始化 Serilog 日志系统

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.DataAccess` | 数据访问层 |
| `Luster.SimDevice` | 仿真设备基础库 |
| `Luster.Motion.DigitalSetup` | Motion 数字化设置 |
| `Luster.Common.Assets` | 通用资源 |
| `Luster.Control.Wpf.Motion` | Motion WPF 控件 |
| `Luster.Motion.AlarmUI` | Motion 报警 UI 模块 |
| `Luster.Motion.Assests` | Motion 资源包 |
| `Luster.Motion.CommonUI` | Motion 通用 UI |
| `Luster.Motion.EditorUI` | Motion 编辑器 UI 模块 |
| `Luster.Motion.ReportUI` | Motion 报表 UI 模块 |
| `Luster.Motion.SubSystem` | Motion 子系统模块 |
| `Luster.SimDevice.SubSystem` | 仿真设备子系统模块 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架（IoC、模块化、MVVM） |
| `MinVer` | 自动版本号生成 |

## 应用程序架构

```
LusterMotion.exe
├── Prism.DryIoc 容器
├── 模块加载器
│   ├── Motion.AlarmUI (报警模块)
│   ├── Motion.EditorUI (编辑器模块)
│   ├── Motion.ReportUI (报表模块)
│   ├── Motion.DigitalSetup (数字化设置)
│   ├── Motion.SubSystem (Motion 子系统)
│   └── SimDevice.SubSystem (仿真设备子系统)
├── 插件目录
│   ├── Motions/ (Motion 插件，动态加载)
│   └── Devices/ (仿真设备插件，动态加载)
└── 配置文件
    ├── Config/Error.json
    ├── Config/Version.json
    └── appsettings.json
```

## 输出到 exe 目录

`LusterMotion.exe` → `artifacts/bin/LusterMotion/Debug/net472/`

**输出目录结构：**
```
artifacts/bin/LusterMotion/Debug/net472/
├── LusterMotion.exe              ← 主程序
├── *.dll                         ← 所有模块 DLL（扁平放置）
├── Config/                       ← 配置文件
├── Devices/                      ← 设备驱动 DLL
├── Motions/                      ← 运动插件 DLL
├── CardErrorCode/                ← 运动卡错误码
├── Langs/                        ← 多语言资源
└── LOGO.ico
```

## 特殊属性

| 属性 | 值 | 说明 |
|------|-----|------|
| `OutputType` | `WinExe` | Windows 可执行程序 |
| `IsShellProject` | `true` | 标记为 Shell 项目，触发模块收集 |
| `UseWPF` | `true` | WPF 应用程序 |
| `TargetFramework` | `net472` | .NET Framework 4.7.2 |

## 启动流程

1. **Main 方法** - 应用程序入口
2. **Prism Bootstrapper** - 初始化 Prism 框架
3. **配置 DryIoc 容器** - 注册服务和依赖
4. **加载模块** - 扫描并加载所有 Prism 模块
5. **初始化日志** - 配置 Serilog 日志系统
6. **创建主窗口** - 启动 WPF 主窗口
7. **显示 UI** - 显示应用程序界面

**特殊说明：**
- 该项目是整个解决方案的顶层项目
- 通过 `Directory.Build.targets` 自动收集所有依赖模块
- 使用 Prism 模块化架构，支持插件式扩展
- 所有 UI 模块通过 Prism 的 `IModule` 接口动态加载
