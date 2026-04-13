# BUSOP 数字架线模块

## 概述

BUSOP 模块用于管理和查看 Excel（xlsx）文件中的 Sheet 页内容，支持子界面增删改、Sheet 预览、缩放拖拽、COM 互操作打开 Excel 等功能。

## 文件结构

```
Views/
  BusopContent.xaml / .cs        -- 主界面（左侧列表 + 右侧预览）
  Dialogs/
    BusopSettingsDialog.xaml     -- 设置对话框（配置 Excel 路径和 Sheet 页）
    TextInputDialog.xaml         -- 文本输入对话框（添加/重命名子界面）

ViewModel/
  BusopContentVM.cs              -- 主界面 ViewModel
  Dialogs/
    BusopSettingsDialogVM.cs     -- 设置对话框 ViewModel
    TextInputDialogVM.cs         -- 文本输入对话框 ViewModel

Datas/
  BusopSubItemConfig.cs          -- 配置数据模型（BusopConfig / BusopSubItemConfig）

Services/
  BusopConfigService.cs          -- 配置持久化服务
```

## 配置文件

配置保存在 `{配方路径}/db/Ass_Data/BusopConfig.json`。

```json
{
  "excelFilePath": "相对路径或绝对路径",
  "subItems": [
    { "name": "BUSOP01", "sheetName": "Sheet1" },
    { "name": "BUSOP02", "sheetName": "" }
  ]
}
```

- `excelFilePath`：支持绝对路径和配方相对路径，相对路径会自动拼接配方目录
- `sheetName`：对应的 xlsx Sheet 页名称，为空时不显示预览图

## 界面布局

```
+------------+---+----------------------------------+
|  BUSOP     |   |  [缩放控制] [打开BUSOP] [设置]   |
| [+][-][✎] | |                                  |
+------------+   |                                  |
| BUSOP01  ● |   |      Sheet 页渲染预览图          |
| BUSOP02  ● |   |      （支持缩放/拖拽/滚轮）      |
| BUSOP03  ● |   |                                  |
| ...        |   |                                  |
+------------+---+----------------------------------+
```

## 功能说明

### 1. 子界面管理（左侧面板）

- **添加**（`+`）：弹出输入对话框，输入名称后添加到列表和配置，自动选中
- **删除**（`-`）：确认后删除选中项，自动选中相邻项
- **重命名**（`✎`）：弹出输入对话框修改名称，同步更新配置
- 所有操作自动持久化到 `BusopConfig.json`

### 2. Sheet 预览（右侧面板）

- 切换子界面时，如果配置了 `SheetName`，使用 Aspose.Cells `SheetRender` 将 Sheet 渲染为 PNG 图片
- 预览图异步加载，不阻塞 UI
- 无配置时显示状态提示（如"请先在设置中配置 Sheet 页"）

### 3. 缩放与拖拽

| 操作       | 行为                          |
|------------|-------------------------------|
| 鼠标滚轮   | 步进 20% 缩放，范围 20%~500%  |
| `+` 按钮   | 放大 20%                      |
| `-` 按钮   | 缩小 20%                      |
| `适应` 按钮| 重置为 100%，偏移归零         |
| 鼠标拖拽   | 按住左键拖动图片平移          |

实现方式：
- `ScaleTransform` + `TranslateTransform` 绑定 VM 中的 `ZoomScale`、`OffsetX`、`OffsetY`
- code-behind 处理鼠标事件，更新 VM 偏移值

### 4. 打开 BUSOP（`OpenBusopCommand`）

通过 COM 互操作打开 xlsx 文件并跳转到指定 Sheet 页：
1. 按优先级检测 Excel / WPS（`Excel.Application` → `et.Application` → `Kwps.Application`）
2. 使用 `Type.InvokeMember` 反射调用 COM 接口（兼容不同 COM 实现）
3. COM 不可用或失败时，回退为 `Process.Start` 系统默认程序打开

### 5. 设置对话框（`BusopSettingsDialog`）

- 配置 Excel 文件路径：支持浏览选择，自动将配方目录下的路径转为相对路径
- 配置当前子界面对应的 Sheet 页名称：下拉选择或手动输入
- Sheet 列表通过 Aspose.Cells 从 xlsx 文件读取

## 关键类

### BusopContentVM

继承 `BaseAss`，但 **不使用** 基类的 CSV 数据加载机制（`ViewType = null`，不调用 `InitModels`）。

| 属性/命令              | 说明                           |
|------------------------|-------------------------------|
| `CurrentSubItemConfig` | 当前选中的子界面配置           |
| `ExcelFilePath`        | Excel 文件完整路径（运行时）   |
| `SheetImage`           | Sheet 渲染预览图               |
| `ZoomScale`            | 缩放比例（1.0 = 100%）         |
| `OffsetX` / `OffsetY`  | 拖拽偏移                       |
| `OpenBusopCommand`     | 打开 Excel/WPS                 |
| `AddSubItemCommand`    | 添加子界面                     |
| `DeleteSubItemCommand` | 删除子界面                     |
| `RenameSubItemCommand` | 重命名子界面                   |

### BusopConfigService

| 方法                | 说明                                         |
|---------------------|----------------------------------------------|
| `LoadConfig()`      | 加载配置，文件不存在时返回默认 18 项配置     |
| `SaveConfig()`      | 保存配置到 JSON                              |
| `GetExcelFullPath()`| 将相对路径转为绝对路径（拼接配方目录）       |
| `GetSheetNames()`   | 从 xlsx 文件读取所有 Sheet 页名称            |

## 注意事项

- 配置中的 `excelFilePath` 建议使用相对路径（相对于配方目录），方便配方迁移
- Sheet 预览使用 Aspose.Cells 渲染，大文件可能需要几秒钟，已做异步处理
- COM 互操作使用反射而非 `dynamic`，确保兼容 Excel 和 WPS 不同的接口
- `BusopContentVM` 不参与 `BaseAss` 的 CSV 数据加载流程，`ViewType` 设为 `null`
