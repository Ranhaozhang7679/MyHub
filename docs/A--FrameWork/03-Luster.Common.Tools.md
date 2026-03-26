# Luster.Common.Tools — 通用工具库

> 路径：`src/Modules/Luster.Common.Tools/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Common.Tools.dll` → exe 根目录

## 项目简介

`Luster.Common.Tools` 是全局通用工具库，提供图像处理、文件压缩、Excel 操作、加密狗验证、网络通信、数学表达式求值、图形布局等功能。通过 **Costura.Fody** 将多个 native DLL 嵌入程序集。

## 核心职责

- **图像处理**：Magick.NET、SkiaSharp 图像操作
- **文件处理**：Excel（Aspose.Cells）、ZIP（SharpZipLib）
- **网络通信**：SSH（Renci.SshNet）、HTTP
- **加密狗验证**：Sentinel HASP 许可证验证（sntl_adminapi）
- **表达式求值**：Luster.DynamicExpresso 动态表达式引擎
- **图布局**：Microsoft.Msagl 图形可视化
- **日志**：NLog 日志记录

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.DataStruct` | 基础数据结构 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Costura.Fody` | 嵌入 native DLL 到程序集 |
| `Magick.NET-Q16-AnyCPU` | 图像处理 |
| `NLog` | 日志框架 |

### 本地 DLL 引用（lib / libCommon）

`Aspose.Cells`, `ICSharpCode.SharpZipLib`, `Luster.DynamicExpresso`, `Microsoft.Msagl.*`, `Newtonsoft.Json`, `NLog`, `Renci.SshNet`, `SkiaSharp`, `sntl_adminapi_net_windows`

### 嵌入的 Native DLL（Costura64/）

`slm_runtime.dll`, `slm_runtime_dev.dll`, `sntl_adminapi_net_windows.dll`, `sntl_adminapi_windows.dll`, `sntl_adminapi_windows_x64.dll`

## 输出到 exe 目录

`Luster.Common.Tools.dll`（包含嵌入的 native DLL）→ Shell 输出目录根下
