# Luster.Common.DataStruct — 通用数据结构

> 路径：`src/Modules/Luster.Common.DataStruct/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Common.DataStruct.dll` → exe 根目录

## 项目简介

`Luster.Common.DataStruct` 是整个解决方案的**最底层数据结构定义库**，定义了坐标、几何体、ROI、测量数据等通用数据类型的接口和实现。其他几乎所有项目直接或间接依赖此项目。

## 核心职责

- 定义通用接口（`ICoord`, `IMeasureData`, `IROI`, `IStl`, `IPicker` 等）
- 提供基础数据结构和枚举类型
- 支持 `AllowUnsafeBlocks`（高性能内存操作场景）
- 提供 SourceLink 支持（`Microsoft.SourceLink.GitLab`），便于调试时回溯源码

## 依赖关系

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `System.Diagnostics.EventLog` | 系统事件日志支持 |
| `Microsoft.SourceLink.GitLab` | 源码链接（调试用） |

### 项目引用

无（最底层基础库）

## 输出到 exe 目录

`Luster.Common.DataStruct.dll` → Shell 输出目录根下
