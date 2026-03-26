# Luster.Module.Motion.Camera — Motion 相机插件

> 路径：`src/Modules/Luster.Module.Motion.Camera/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.Camera.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.Camera` 是 **Motion 相机控制插件**，提供相机采集、图像处理、视觉算法集成等功能。该插件集成了 3D 算法库，支持立体视觉和图像分析。

## 核心职责

- **相机控制** - 相机设备管理和控制
  - 相机初始化和配置
  - 图像采集和触发
  - 相机参数调整
- **图像处理** - 基本图像处理功能
  - 图像增强
  - 图像滤波
  - 图像变换
- **3D 视觉算法** - 集成 ThreeD.Algorithm 库
  - 立体视觉计算
  - 点云处理
  - 3D 重建
- **任务流集成** - 相机采集任务节点

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.ThreeD.Algorithm` | 3D 算法库 |
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

`Luster.Module.Motion.Camera.dll` → `Motions/` 子目录

**特殊属性：**
- `CopyToMotionsFolder=true` - 自动拷贝到 Motions/ 目录
