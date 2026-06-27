# XAML 静态检查器(Luster.XamlLinter)设计

> 视觉反馈闭环的第三步:补 Reviewer 看不到的源码级维度。

## 背景

LMV-2026 的 WPF 视觉反馈闭环已有两步:

- **Step 2 Luster.PreviewHost** — 截图(像素可见)
- **Step 3 Luster.VisualReviewer** — 视觉模型评像素可见维度(overlap/spacing/layout/font)

实验(`workspace/model-compare/compare2.py`)证实:视觉模型从像素**看不到**源码级维度——控件库前缀 `hc:`、资源键 `{StaticResource}`、校验样式、字号档位。把契约全文喂模型逼它评这些,只会套模板瞎猜(实测把 `hc:ComboBox` 说成"原生控件"、Score 0)。P1 已用优化 prompt 修掉视觉评阅的瞎猜,但源码级维度仍无人覆盖。

**正确分工**:像素可见维度 → 视觉模型;源码级维度 → 静态解析(本 spec)。两者职责分离、报告并列,各自只评自己看得见的。

## 目标

独立 exe `Luster.XamlLinter`,用 `System.Xaml.XamlXmlReader` 解析指定 View 的 XAML 文件,按设计契约(`docs/wpf-design-contract.md`)的源码级规则检查,输出结构化 JSON 报告。不依赖视觉模型、不依赖主题字典、可离线运行、可独立测试。

## 非目标

- 不验 `{StaticResource Key}` 的 Key 是否真存在(键合法性留 v2,需反射加载主题字典 BAML,复杂度高且契约清单易过期)。
- 不做语义/逻辑分析(绑定是否正确、命令是否接线等)。
- 不修改 XAML,只出报告(建议式,呼应 Reviewer 的建议式语义)。

## 关键决策(已与用户确认)

1. **形态:独立 exe**(与 PreviewHost/Reviewer 并列的第三步),不合并进 Reviewer。视觉维度与源码维度报告分开,职责最清晰。
2. **实现:`XamlXmlReader` 解析节点流**,纯正则会误报注释里的 `<Button>`、字符串里的 `#fff`,静态检查器一旦误报可信度崩塌(同原 Reviewer 套模板问题)。net472 原生带 `System.Xaml`,无额外依赖。
3. **v1 深度:仅写死值 + 裸控件 + 内联 Style + 字号档位**,不验键合法性。

## 检查规则(v1,5 条)

每条规则对应契约的一个源码级维度。Location 报行号(`IXamlLineInfo`)。

| 规则 | Severity | 契约出处 | 判断逻辑 |
|---|---|---|---|
| `bare-control` | high | §1 控件库优先级 | 元素默认 ns(`http://schemas.microsoft.com/winfx/2006/xaml/presentation`)且 LocalName ∈ 禁裸清单 → 应改 `hc:` 或 Luster.Controls.Wpf 封装 |
| `hardcoded-color` | medium | §2.1/§2.2 资源键 | 颜色类属性(Background/Foreground/BorderBrush/Fill/Color/Stroke/OpacityMask)值匹配 `#hex` → 应 `{StaticResource}` |
| `hardcoded-size` | medium | §2.4 尺寸/间距 | 尺寸类属性(Height/Width/Padding/Margin/CornerRadius/MinWidth/MinHeight/MaxWidth/MaxHeight)是裸数值 → 应引用 Sizes.xaml Key |
| `inline-style` | medium | §1(样式进资源字典) | View 内出现 `<Style>` 元素 → 应进资源字典,View 内只 `{StaticResource}` 引用 |
| `font-size-tier` | low | §3 字号档位 | `FontSize` 裸数值 ∉ {12,14,20} → 应走三档(标题20/正文14/标签12) |

### 禁裸控件清单(v1)

契约 §1 明确"不要裸"的 presentation-ns 控件:

`Button`、`TextBox`、`PasswordBox`、`ComboBox`、`ListBox`、`Border`、`Label`、`CheckBox`、`RadioButton`、`Slider`、`ProgressBar`、`Expander`、`GroupBox`、`TabControl`、`TreeView`、`DataGrid`、`DatePicker`、`Calendar`、`Menu`、`ContextMenu`、`ToolBar`、`StatusBar`、`TextBlock` 例外放行(纯文本展示无 hc 等价,且契约未禁)。

> 清单可后续按实际补;放 `RuleConfig.cs` 集中管理。

### 跳过项

- **`d:` 设计时属性**:d 命名空间(`http://schemas.microsoft.com/expression/blend/2008`)的成员不参与检查(如 `d:DesignHeight`)。
- **`x:` 编译指令**:x:Class/x:Name 等不检查。
- **`mc:Ignorable`** 标记的命名空间属性。

## 实现要点

### XamlXmlReader 节点遍历

`XamlXmlReader` 把 XAML 读成节点流(XAML 节点流),比正则可靠:

- `StartObject` 带 `XamlType` → 取 `PreferredXamlNamespace`(判裸控件:== presentation ns 且 LocalName ∈ 清单)
- `StartMember` 带 `Member` → 取命名空间(d: 跳过)+ 属性名(颜色/尺寸清单)+ 后续 `Value` 节点取值(正则判 hex/裸数值)
- `IXamlLineInfo` → 取行号写进 Location
- 注释/字符串里的 `<Button>`/`#fff` 不会进节点流,天然不误报

### 属性值类型判断

- **hex 颜色**:正则 `^#([0-9a-fA-F]{3}|{6}|{8})$` 匹配颜色属性值。
- **裸数值尺寸**:尺寸属性值为纯数字(含小数)或逗号分隔多值(`5,2`)→ 写死;`{StaticResource}`/`{Binding}`/`{DynamicResource}` 等标记扩展不报。
- **字号档位**:FontSize 属性值为纯数值且 ∉ {12,14,20} → 报 low。

### 命名空间常量

```csharp
const string PresentationNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
const string DesignNs       = "http://schemas.microsoft.com/expression/blend/2008";
const string XNs            = "http://schemas.microsoft.com/winfx/2006/xaml";
// hc: 的真实 URI(用于将来扩展判断, v1 不依赖)
const string HandyControlNs = "https://handyorg.github.io/handycontrol";
```

## 工程结构(仿 PreviewHost/Reviewer)

```
src/Tools/Luster.XamlLinter/
  Luster.XamlLinter.csproj   # console exe, net472, 引 System.Xaml
  Program.cs                  # CLI 解析: --xaml/--report/--view, 退出码 0/1
  XamlLinter.cs               # 核心: XamlXmlReader 遍历 + 规则匹配
  LintModels.cs               # LintReport / LintIssue
  RuleConfig.cs               # 禁裸清单/颜色属性集/尺寸属性集/合法字号集
src/Tests/Luster.XamlLinter.Tests/
  XamlLinterTests.cs          # TDD: 每规则一个测试 + 边界(空/注释/d:跳过)
  Fixtures/*.xaml             # 故意含各类违规的样本
```

### CLI

```bash
artifacts/bin/net472/Luster.XamlLinter.exe \
  --xaml src/Modules/.../ParamView.xaml \
  --report workspace/wpf-preview/ParamView/lint-<ts>.json \
  --view ParamView
```

- `--xaml <path>` 必需,View 源文件路径
- `--report <out.json>` 必需,JSON 报告输出
- `--view <名>` 可选,View 名(报告字段,默认取 xaml 文件名)

### 退出码

- `0` = 检查完成(可能有 issue;issue 是建议不阻塞,呼应 Reviewer 建议式语义)
- `1` = 参数错误 / XAML 解析失败(`XamlParseException` 等,看 stderr)

> 不因有 issue 而退出码非零——避免在流水线里把"有改进建议"误当"失败"。

## JSON 报告结构

```json
{
  "View": "ParamView",
  "Xaml": "src/Modules/.../ParamView.xaml",
  "Summary": "发现 3 个源码级问题:1 个裸控件、2 个写死值",
  "IssueCount": 3,
  "Issues": [
    {"Severity":"high","Rule":"bare-control","Description":"裸 <Button> 应改 hc:Button 或 Luster.Controls.Wpf 封装","Location":"L12"},
    {"Severity":"medium","Rule":"hardcoded-color","Description":"Background 写死 #1ba1e2,应 {StaticResource} 引用主题色键","Location":"L15"},
    {"Severity":"low","Rule":"font-size-tier","Description":"FontSize=16 不在三档(12/14/20)","Location":"L18"}
  ]
}
```

## 测试策略(TDD)

每条规则一个测试,加边界用例,全部走 `XamlLinter.Lint(string xamlContent)` 静态方法(不读文件、不依赖磁盘),`Program.cs` 只做 CLI 包装:

- `BareControl_BareButton_ReportsHigh` / `BareControl_HcButton_NoIssue`
- `HardcodedColor_Hex_ReportsMedium` / `HardcodedColor_StaticResource_NoIssue`
- `HardcodedSize_PixelValue_ReportsMedium` / `HardcodedSize_Binding_NoIssue`
- `InlineStyle_Reported` / `FontSize_OutOfTier_ReportsLow` / `FontSize_InTier_NoIssue`
- 边界:`EmptyXaml_NoIssue`、`CommentContainsButton_NotReported`(注释里 `<Button>` 不误报)、`DesignAttribute_Skipped`(d:DesignHeight 不报)

夹具 XAML 放 `Fixtures/`,测试直接内联字符串也可(`Program.cs` 与 `XamlLinter` 分离使核心可纯内存测)。

## SKILL.md 集成

- 加 Step 4:XamlLinter 静态检查
- Step 3 Reviewer 预警段补一句:"源码级合规(控件库前缀 `hc:`/资源键/字号档位)由 **Step 4 XamlLinter** 覆盖,不在视觉评阅范围"
- Step 4 建议式不阻塞(同 Reviewer)

## 风险与限制

- **XamlXmlReader 对 `mc:Ignorable` 内容的处理**:需确认被忽略命名空间(如 `d:`)的成员是否仍进节点流——实现时验证,若进则显式跳过。
- **行号准确性**:`IXamlLineInfo.HasLineInfo` 取决于 reader 配置,需开启。
- **禁裸清单可能不全**:v1 先放契约明确的几个,实际使用中发现漏报再补;清单集中 `RuleConfig.cs` 易维护。
- **不验键合法性**:View 引用了不存在的 `{StaticResource FooBar}` 不报——这是 v1 的有意取舍,留 v2。

## 后续(v2+)

- 资源键合法性验证(反射加载主题字典 BAML 提取合法键集)
- 主题样式引用验证(`TextBoxValidationStyle` 等)
- 自定义规则配置(契约 md 解析自动生成规则)
