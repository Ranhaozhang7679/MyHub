# Luster.Common.DataAccess — 数据访问层

> 路径：`src/Modules/Luster.Common.DataAccess/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Common.DataAccess.dll` → exe 根目录

## 项目简介

`Luster.Common.DataAccess` 封装了基于 **FreeSql + SQLite** 的数据访问层，提供 ORM 映射、Repository 模式和 IoC 集成能力，用于系统配置、运行参数、测量数据等的持久化存储。

## 核心职责

- 基于 **FreeSql** 的 ORM 数据访问封装
- **SQLite** 数据库的连接和管理
- Repository 模式（`FreeSql.Repository`）
- 与 Prism IoC 容器集成（通过 `Luster.Prism` NuGet 包）

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Motion.DataStruct` | Motion 专用数据结构（实体模型） |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `FreeSql` | ORM 核心框架 |
| `FreeSql.Provider.Sqlite` | SQLite 数据库驱动 |
| `FreeSql.Repository` | 仓储模式实现 |
| `Luster.Prism` | IoC 容器集成 |

## 输出到 exe 目录

`Luster.Common.DataAccess.dll` → Shell 输出目录根下
