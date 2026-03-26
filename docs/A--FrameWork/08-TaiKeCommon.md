# TaiKeCommon — 第三方图表组件封装

> 路径：`src/Modules/TaiKeCommon/`  
> 类型：WPF 类库  
> 输出：`TaiKeCommon.dll` → exe 根目录

## 项目简介

`TaiKeCommon` 封装 **LiveCharts** 图表组件，为运动设备和报表模块提供数据可视化支持。

## 核心职责

- 封装 `LiveChartsCore.SkiaSharpView.WPF` 图表控件
- 提供通用的图表数据绑定和样式模板

## 依赖关系

### 项目引用

无

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `LiveChartsCore.SkiaSharpView.WPF` | WPF 图表控件 |

## 输出到 exe 目录

`TaiKeCommon.dll` → Shell 输出目录根下
