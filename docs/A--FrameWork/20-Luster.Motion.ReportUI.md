# Luster.Motion.ReportUI — Motion 报表 UI

> 路径：`src/Modules/Luster.Motion.ReportUI/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.ReportUI.dll` → exe 根目录

## 项目简介

`Luster.Motion.ReportUI` 是 **Motion 系统的报表管理 UI 模块**，提供生产数据报表、统计图表、数据导出等功能。该模块作为 Prism 模块集成到主程序，支持多种报表格式和数据可视化。

## 核心职责

- **生产报表** - 生产数据统计和展示
  - 产量报表
  - 良率报表
  - 工时报表
- **数据可视化** - 使用 LiveCharts 展示统计图表
  - 趋势图
  - 柱状图
  - 饼图
- **数据导出** - 支持导出为 Excel、CSV 等格式
  - 使用 WindowsAPICodePack 提供文件保存对话框
- **ToeIn 内容视图** - ToeInContent.xaml（特定业务视图）
- **数据库查询** - 通过 DataAccess 层查询生产数据
- **TaiKe 集成** - 与 TaiKeCommon 库集成（可能用于特定硬件或协议）

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `TaiKeCommon` | 泰科通用库 |
| `Luster.Common.DataAccess` | 数据访问层 |
| `Luster.Motion.TaskFlow.Engine` | Motion 任务流引擎 |
| `Luster.Motion.CommonUI` | Motion 通用 UI 组件 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架 |
| `HandyControl` | WPF UI 控件库 |
| `LiveCharts.Wpf` | 图表组件 |
| `Luster.WindowsAPICodePack` | Windows API 封装 |
| `MinVer` | 自动版本号生成 |

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `System.Windows.Forms` | WinForms 互操作 |

## 输出到 exe 目录

`Luster.Motion.ReportUI.dll` → Shell 输出目录根下
