# ToolBarContent 权限管理适配计划

## Context

ToolBarContent.xaml 中的 btnMode（模式切换按钮）、Pages（导航页面列表）和 Commands（启停操作按钮）目前通过硬编码 `page_IsEnabled` / `page_IsVisible` 和角色判断控制可见性和可用性。需要将这些统一接入权限管理体系（AuthDictionary + AuthBehavior），使管理员可以在权限管理界面灵活配置每个页面/按钮对不同角色的可见性。

同时，ToolBarContentVM 中 UserInfoEvent 里硬编码的 Operator 角色判断需要删除，改由权限体系统一管理。

## 修改范围

### 1. AuthDictionary 新增权限项定义

**文件**: `src/Modules/Luster.Common.Authorization/AuthKeys.cs`

### 2. PageModel / CommandModel 增加 AuthItemName 属性

**文件**: `src/Modules/Luster.Motion.CommonUI/Models/PageModel.cs` / `CommandModel.cs`

为 Model 添加 `AuthItemName` 字符串属性，关联 AuthDictionary 中的权限项名称。

### 3. 创建 AuthVisibilityConverter

**新文件**: `src/Modules/Luster.Authorization.Client/Helper/AuthVisibilityConverter.cs`

DataTemplate 内无法直接使用 AuthBehavior（需要静态引用），通过 Converter 桥接：接收 AuthItemName → 反射获取 AuthItem → HasAuth → Visibility。

### 4. ToolBarContentVM 修改

**文件**: `src/Modules/Luster.Motion.SubSystem/ViewModel/ToolBarContentVM.cs`

- 构造函数注入 IAuthorizationFacade
- 添加 [AuthVisibility] 标注方法注册权限项
- 删除硬编码 Operator 判断

### 5. ToolBarContent.xaml + App.xaml 修改

**文件**: `src/Modules/Luster.Motion.SubSystem/Views/ToolBarContent.xaml` / `src/Shell/LusterMotion/App.xaml`

- btnMode 使用 AuthBehavior 附加属性
- Pages/Commands DataTemplate 使用 AuthVisibilityConverter
- App.xaml 注册全局 Converter

## 关键文件清单

| 文件 | 操作 |
|---|---|
| `src/Modules/Luster.Common.Authorization/AuthKeys.cs` | 新增权限项 |
| `src/Modules/Luster.Motion.CommonUI/Models/PageModel.cs` | 新增 AuthItemName 属性 |
| `src/Modules/Luster.Motion.CommonUI/Models/CommandModel.cs` | 新增 AuthItemName 属性 |
| `src/Modules/Luster.Authorization.Client/Helper/AuthVisibilityConverter.cs` | 新建 |
| `src/Modules/Luster.Motion.SubSystem/ViewModel/ToolBarContentVM.cs` | 修改构造函数、添加标注、删除硬编码 |
| `src/Modules/Luster.Motion.SubSystem/Views/ToolBarContent.xaml` | 添加 AuthBehavior 和 Converter 绑定 |
| `src/Shell/LusterMotion/App.xaml` | 注册全局 Converter |

## 验证方式

1. `dotnet build LMV-2026.sln` 编译通过
2. 启动应用，管理员登录后打开权限管理界面，确认所有新增的权限项已出现在列表中
3. 为 Operator 角色取消 Flow/Configure/Project 的可见性权限，登出后用 Operator 登录，确认这三个页面不在工具栏显示
4. 切换用户，确认页面和按钮可见性实时刷新
5. 确认 btnMode 模式切换按钮在无权限时隐藏
