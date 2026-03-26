# Luster.Module.Motion.TwoD — Motion 2D 视觉插件

> 路径：`src/Modules/Luster.Module.Motion.TwoD/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.TwoD.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.TwoD` 是 **Motion 2D 视觉插件**，提供 2D 图像处理、视觉算法、特征提取等功能。该插件集成了任务流引擎和 Motion 集成层。

## 核心职责

- **2D 图像处理** - 图像预处理和增强
  - 图像滤波
  - 边缘检测
  - 形态学操作
- **特征提取** - 图像特征识别
  - 轮廓检测
  - 圆形检测
  - 直线检测
- **模板匹配** - 图像模板匹配算法
- **位置定位** - 目标位置计算
- **任务流集成** - 2D 视觉任务节点

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.Integration` | Motion 集成层 |
| `Luster.TaskFlow.Engine` | 通用任务流引擎 |
| `Luster.TaskFlow.Motion` | 运动任务流数据模型 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `MinVer` | 自动版本号生成 |

## 输出到 exe 目录

`Luster.Module.Motion.TwoD.dll` → `Motions/` 子目录
