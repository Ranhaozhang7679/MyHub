# Luster.Module.Motion.Algorithm — Motion 算法插件

> 路径：`src/Modules/Luster.Module.Motion.Algorithm/`  
> 类型：类库（Motion 插件）  
> 输出：`Luster.Module.Motion.Algorithm.dll` → `Motions/` 子目录

## 项目简介

`Luster.Module.Motion.Algorithm` 是 **Motion 算法插件**，提供运动控制相关的算法实现，包括路径规划、插补算法、运动学计算等。该插件通过 `CopyToMotionsFolder=true` 属性自动拷贝到 `Motions/` 目录，由主程序动态加载。

## 核心职责

- **路径规划算法** - 运动路径计算和优化
  - 直线路径规划
  - 圆弧路径规划
  - 样条曲线路径规划
- **插补算法** - 运动轨迹插补
  - 直线插补
  - 圆弧插补
  - 螺旋插补
- **运动学计算** - 机器人运动学求解
  - 正运动学（FK）
  - 逆运动学（IK）
- **任务流集成** - 与任务流引擎深度集成
  - 算法节点定义
  - 参数配置接口
  - 执行结果反馈

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

### .NET Framework 引用

| 引用 | 用途 |
|------|------|
| `System.ComponentModel.DataAnnotations` | 数据验证注解 |

## 输出到 exe 目录

`Luster.Module.Motion.Algorithm.dll` → `Motions/` 子目录

**特殊属性：**
- `CopyToMotionsFolder=true` - 通过 `Directory.Build.targets` 自动拷贝到 Motions/ 目录
- 作为插件动态加载，支持热插拔
