# 界面适配权限管理 SOP

## 概述

本文档描述如何将现有 WPF 界面接入权限管理系统，实现**操作权限控制**（按钮/命令）和**可见性控制**（界面元素显示隐藏）两种管控方式。

---

## 权限体系架构

```
AuthDictionary（权限项定义）
    │
    ├── [AuthRight]      → 标注方法 → AuthViewModelBase 自动注册 → AuthCommand 运行时拦截
    │                                                                  → 无权限弹窗提示
    │
    └── [AuthVisibility]  → 标注属性/方法/字段 → AuthViewModelBase 自动注册
                                                      ↓
                                              XAML 中使用 AuthBehavior 附加属性
                                                      → 自动 Hide 或 Disable 控件
                                                      → 用户切换时自动刷新
```

### 核心类一览

| 类 | 所在项目 | 职责 |
|---|---|---|
| `AuthDictionary` | Luster.Common.Authorization | 定义全局权限项常量（`AuthItem`） |
| `AuthItem` | Luster.Common.Authorization | 权限点结构体：Module / View / Operation / Description |
| `AuthRightAttribute` | Luster.Common.Authorization | 标注操作权限方法，反射读取 AuthDictionary |
| `AuthVisibilityAttribute` | Luster.Common.Authorization | 标注可见性权限，反射读取 AuthDictionary |
| `AuthViewModelBase` | Luster.Authorization.Client | VM 基类，构造时自动扫描注册 `[AuthRight]` / `[AuthVisibility]` |
| `AuthCommand` | Luster.Authorization.Client | 带 `CheckAuth` 校验的命令，无权限弹窗拦截 |
| `AuthBehavior` | Luster.Authorization.Client | XAML 附加属性，根据权限 Hide/Disable 控件，用户切换自动刷新 |

### 继承关系

```
BindableBase
  └── AuthViewModelBase          ← 注入 IAuthorizationFacade，自动注册权限
        └── MotionVM             ← 添加 SysRole / IsAdmin / IsEngineer
              └── MotionPageVM   ← 添加 INavigationAware 页面导航支持
                    └── 各业务 ViewModel（如 LoginContentFXVM）
```

---

## SOP 步骤

### 第一步：定义权限项（AuthDictionary）

在 `AuthDictionary` 中新增权限常量：

```csharp
// 文件：Luster.Common.Authorization / AuthKeys.cs

public static class AuthDictionary
{
    // 操作权限
    public static readonly AuthItem SaveRecipe = new AuthItem(
        "产线模块", "配方管理", "保存配方", "允许保存和修改配方数据");

    // 可见性权限
    public static readonly AuthItem VizDebugPanel = new AuthItem(
        "产线模块", "调试面板", "显示调试面板", "控制调试面板分页的可见性");
}
```

**命名规范**：
- 操作权限：动词 + 名词，如 `SaveRecipe`、`ModifyPassword`
- 可见性权限：`Viz` 前缀 + 名词，如 `VizDebugPanel`、`VizAdvancedConfigTab`
- Module / View 分两级分类，保持一致

---

### 第二步：ViewModel 接入权限基类

确保 ViewModel 继承链中有 `AuthViewModelBase`（项目中的 VM 通常已继承 `MotionPageVM`，无需额外操作）。

**关键**：构造函数必须向基类传递 `IAuthorizationFacade`：

```csharp
public class MyViewModel : MotionPageVM
{
    public MyViewModel(ICommonBus commonBus, IAuthorizationFacade facade)
        : base(commonBus, facade)   // ← 传递给 MotionPageVM → MotionVM → AuthViewModelBase
    {
        // 基类构造时已自动完成权限注册
    }
}
```

---

### 第三步A：操作权限 — 保护命令按钮

适用场景：按钮点击、菜单操作等需要权限校验的交互。

#### 3A-1 声明 AuthCommand

```csharp
private AuthCommand _saveCommand;
public AuthCommand SaveCommand => _saveCommand
    ?? (_saveCommand = new AuthCommand(Auth, AuthDictionary.SaveRecipe, OnSave));
```

#### 3A-2 标注 [AuthRight] 特性

```csharp
[AuthRight(nameof(AuthDictionary.SaveRecipe))]
private void OnSave()
{
    // 双重校验：AuthCommand.Execute 已做 CheckAuth，方法内再次确认
    if (!Auth.CheckAuth(default)) return;
    // 业务逻辑...
}
```

#### 3A-3 XAML 绑定

```xml
<Button Content="保存" Command="{Binding SaveCommand}" />
```

> **原理**：`AuthViewModelBase` 构造时通过反射扫描 `[AuthRight]`，将权限项注册到 `right_info` 表。`AuthCommand.Execute` 内部调用 `CheckAuth`，无权限时弹窗提示并阻止执行。

---

### 第三步B：可见性权限 — 控制界面元素

适用场景：根据用户角色自动隐藏/禁用 Tab、面板、按钮等。

#### 3B-1 在 ViewModel 中声明可见性标记

**方式一：标注空方法**（推荐，最简洁）

```csharp
[AuthVisibility(nameof(AuthDictionary.VizDebugPanel))]
private void ShowDebugPanel() { }
```

**方式二：标注属性**

```csharp
[AuthVisibility(nameof(AuthDictionary.VizDebugPanel))]
public bool ShowDebugPanel { get; set; }
```

> 目的仅是让 `AuthViewModelBase` 自动扫描并注册该权限项到数据库，方法/属性本身不需要实现逻辑。

#### 3B-2 在 XAML 中使用 AuthBehavior

首先确保 XAML 引入命名空间：

```xml
xmlns:auth="clr-namespace:DC.Authorization.WPF.Helper;assembly=Luster.Authorization.Client"
```

然后为目标控件添加附加属性：

```xml
<!-- 隐藏模式：权限不足时 Collapsed -->
<TabItem Header="调试面板"
         auth:AuthBehavior.RightItem="{x:Static auth:AuthDictionary.VizDebugPanel}"
         auth:AuthBehavior.Action="Hide">
</TabItem>

<!-- 禁用模式：权限不足时 IsEnabled=False -->
<Button Content="危险操作"
        auth:AuthBehavior.RightItem="{x:Static auth:AuthDictionary.ChangeMotorSpeed}"
        auth:AuthBehavior.Action="Disable" />
```

> **原理**：`AuthBehavior` 是附加属性，控件加载时自动检查 `IAuthorizationFacade.HasAuth()`，根据结果设置 `Visibility` 或 `IsEnabled`。订阅了 `LoginService` 的登录/登出事件，用户切换时自动刷新所有绑定控件的可见性状态。

---

### 第四步：权限分配

代码完成后的运行时操作：

1. 启动应用，用管理员账号登录
2. 进入权限管理界面（登录页双击标题）
3. 新定义的权限项会自动出现在权限列表中（由 `AutoRegisterRights` 注册）
4. 将权限项分配给对应角色
5. 重新登录验证效果

---

## 完整示例

### ViewModel 端

```csharp
public class RecipeEditorVM : MotionPageVM
{
    // ── 操作权限命令 ──
    private AuthCommand _saveCommand;
    public AuthCommand SaveCommand => _saveCommand
        ?? (_saveCommand = new AuthCommand(Auth, AuthDictionary.SaveRecipe, OnSave));

    [AuthRight(nameof(AuthDictionary.SaveRecipe))]
    private void OnSave()
    {
        if (!Auth.CheckAuth(default)) return;
        // 保存配方逻辑...
    }

    // ── 可见性权限标记 ──
    [AuthVisibility(nameof(AuthDictionary.VizDebugPanel))]
    private void ShowDebugPanel() { }
}
```

### XAML 端

```xml
<UserControl
    xmlns:auth="clr-namespace:DC.Authorization.WPF.Helper;assembly=Luster.Authorization.Client"
    ...>

    <!-- 操作权限：按钮使用 AuthCommand -->
    <Button Content="保存配方" Command="{Binding SaveCommand}" />

    <!-- 可见性权限：Tab 页面自动隐藏 -->
    <TabItem Header="调试面板"
             auth:AuthBehavior.RightItem="{x:Static auth:AuthDictionary.VizDebugPanel}"
             auth:AuthBehavior.Action="Hide">
        <!-- 调试面板内容 -->
    </TabItem>
</UserControl>
```

---

## 快速参考对照表

| 需求 | 做法 | VM 端 | XAML 端 |
|---|---|---|---|
| 保护按钮操作 | `AuthCommand` + `[AuthRight]` | 声明命令 + 标注方法 | `Command="{Binding XxxCommand}"` |
| 隐藏界面元素 | `AuthBehavior` + `[AuthVisibility]` | 标注空方法/属性 | `auth:AuthBehavior.RightItem="..." Action="Hide"` |
| 禁用界面元素 | `AuthBehavior` + `[AuthVisibility]` | 标注空方法/属性 | `auth:AuthBehavior.RightItem="..." Action="Disable"` |

---

## 注意事项

1. **必须定义在 AuthDictionary**：`[AuthRight]` 和 `[AuthVisibility]` 通过 `nameof()` 引用 `AuthDictionary` 中的静态字段，反射获取 `AuthItem`。如果名称不匹配，会注册为"无效权限定义"。
2. **基类构造顺序**：`AuthViewModelBase` 在构造时自动调用 `AutoRegisterRights()`，因此 `[AuthRight]` / `[AuthVisibility]` 标注的方法必须在子类中定义，且基类构造函数必须先执行。
3. **XAML 命名空间**：`AuthBehavior` 需要引入 `xmlns:auth="clr-namespace:DC.Authorization.WPF.Helper;assembly=Luster.Authorization.Client"`。
4. **双重校验**：`AuthCommand.Execute` 内部已做 `CheckAuth`（弹窗提示），方法内再写 `if (!Auth.CheckAuth(default)) return;` 是防御性编程，两者不冲突。
5. **用户切换自动生效**：`AuthBehavior` 订阅了 `LoginService` 的 `OnCardLogin` / `OnPasswordLogin` / `OnLogout` 事件，用户切换时自动刷新所有绑定控件的可见性/启用状态，无需手动刷新。
6. **AuthBehavior 只控制 UIElement**：附加属性只能用在 `UIElement` 及其子类上（`Button`、`TabItem`、`StackPanel` 等）。
