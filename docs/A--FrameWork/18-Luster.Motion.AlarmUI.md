# Luster.Motion.AlarmUI — Motion 报警 UI

> 路径：`src/Modules/Luster.Motion.AlarmUI/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.AlarmUI.dll` → exe 根目录

## 项目简介

`Luster.Motion.AlarmUI` 是 **Motion 系统的报警管理 UI 模块**，提供报警信息的展示、查询、统计和历史记录功能。该模块作为 Prism 模块加载到主程序中，提供独立的报警管理界面。

## 核心职责

- **报警列表展示** - 实时显示当前报警信息
  - 报警级别（错误、警告、信息）
  - 报警时间
  - 报警来源（设备、工位）
  - 报警描述
- **报警历史查询** - 查询和筛选历史报警记录
  - 时间范围筛选
  - 报警级别筛选
  - 关键字搜索
- **报警统计图表** - 使用 LiveCharts 展示报警统计数据
  - 报警趋势图
  - 报警分布图
  - 报警频率分析
- **报警确认与处理** - 报警确认、复位等操作
- **Prism 模块化** - 作为独立模块集成到主程序

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.CommonUI` | Motion 通用 UI 组件（继承基础功能） |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Luster.Prism` | Prism 框架（模块化、MVVM） |
| `HandyControl` | WPF UI 控件库 |
| `LiveCharts.Wpf` | 图表组件 |
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.Motion.AlarmUI.dll` → Shell 输出目录根下

**特殊说明：**
- 该模块通过 Prism 的 `IModule` 接口注册到主程序
- 依赖 CommonUI 提供的基础对话框和控件
- 报警数据通过 CommonUI 的数据访问层（FreeSql）读取
