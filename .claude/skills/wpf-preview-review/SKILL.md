---
name: wpf-preview-review
description: LMV-2026 项目的 WPF 页面视觉反馈闭环。当你在 LMV-2026 仓库里写/改 WPF View(XAML),或想知道 Agent 写的 WPF 页面长什么样、符不符合设计规范时,用这个 skill。它通过独立预览宿主(Luster.PreviewHost)把指定 View 连同真实主题渲染成 PNG 截图,再用视觉模型(Qwen3-VL)对照设计契约评阅,产出结构化问题报告。让"看不见渲染结果"的盲改变成有视觉反馈的迭代。涉及 WPF 页面、XAML、View 渲染、界面预览、UI 评审、截图自检、d:DesignInstance 设计时数据等场景都应触发。
---

# WPF 页面视觉反馈闭环

## 这个 skill 解决什么

Agent 写 WPF 页面最大的痛点是**看不见渲染结果**——写完 XAML 只能靠脑补布局,改一遍盲改一遍。Web 前端能 Playwright 截图自检,WPF 没有。本闭环补上这个缺口:一条命令把 View 连同项目真实主题渲染成截图,再用视觉模型按设计契约评阅,产出问题清单。

两个工具(已在仓库 `feature/wpf-visual-feedback-loop` 分支实现):

- **`Luster.PreviewHost`** — 独立预览 exe,复用 Shell 主题字典,实例化指定 View + 设计时 mock VM,渲染成 PNG。
- **`Luster.VisualReviewer`** — 评阅 console,读截图,调 siliconflow Qwen3-VL 评像素可见维度,出结构化 JSON 报告(prompt 自包含维度,不加载契约全文)。

## 命令运行环境

本 skill 的命令按 **Git Bash**(本机默认 shell)写,用 `\` 行续行 + `$(date ...)`。若你用 PowerShell,需把 `\` 续行改反引号 `` ` ``、`$(date +%Y%m%d-%H%M%S)` 改 `$(Get-Date -Format yyyyMMdd-HHmmss)`、`export VAR=...` 改 `$env:VAR="..."`。下文不再重复提示。

## 前置条件

1. **先 build**。两个 exe 是构建产物,不在 git 里。
   - 首次或拉取新代码后,整 sln build 一次:
     ```bash
     dotnet build LMV-2026.sln
     # 产物在 artifacts/bin/net472/Luster.PreviewHost.exe 和 Luster.VisualReviewer.exe
     ```
   - **若只是新增/改了 PreviewHost 的 mock VM**,不必整 sln,轻量 build 单工程即可(~6 秒):
     ```bash
     dotnet build src/Tools/Luster.PreviewHost/Luster.PreviewHost.csproj
     ```
   注意:build 前确保 `LusterMotion.exe` 没在运行,否则会锁 DLL 导致 build 复制失败。

2. **视觉评阅需要 API key**(siliconflow)。设环境变量(不写进源码,安全):
   ```bash
   export SILICONFLOW_API_KEY="你的key"
   ```
   - siliconflow 是国内服务,**需直连**。若机器全局代理把 `api.siliconflow.cn` 绕到代理会失败(且只报降级退出码 2,难排查)——确保系统代理对该域名直连,或在 `NO_PROXY` 排除它。
   - 未设 key 时 Reviewer 会降级(退出码 2,仍落盘报告标 Degraded,不评阅)——截图由 PreviewHost 此前已落盘,可人工看图。

3. **目标 View 须可被反射实例化**。View 类需 public、有无参构造。设计时数据通过 `--designvm` 传 mock VM 类型全名。

## 核心流程:截图 → 评阅 → 迭代

### Step 0: 挑选目标 View

用 `Glob` 找 `src/Modules/**/*.xaml`,挑一个要预览的 View。挑选建议:

- 优先 `UserControl`(不要 `Window`——Window 有 chrome,离屏渲染易出异常)。
- 内容明确(有几个控件,不是空 Grid),依赖尽量少。
- 简单 View 先试,验证流程通了再上复杂 View。
- 看清楚它所在的**程序集**(csproj 名/AssemblyName),`--view` 的"程序集"部分要填对。PreviewHost 直接引用了 `Luster.Common.Assets`/`Luster.Motion.Assests`/`Luster.Controls.Wpf`/`Luster.Control.Wpf.Motion`/`Luster.SimDevice.SubSystem`,这些模块的 View 可直接 `--view`;**其他模块的 View 必须加 `--assembly <dll路径>`**(见坑3)。

### Step 1: 给 View 准备设计时数据(若还没有)

PreviewHost 靠 mock VM 填充 View 的 DataContext(它**不走 Prism 的 ViewModelLocator**,DataContext 完全由 `--designvm` 决定,真实 VM 的逻辑/数据不会跑起来)。给 View 配一个设计时 VM。

**mock VM 放哪里**:推荐放 `src/Tools/Luster.PreviewHost/Fixtures/`,namespace 用 `Luster.PreviewHost.Fixtures`——和夹具并列,零额外引用、不污染业务模块,`--designvm` 填 `Luster.PreviewHost.Fixtures.XxxDesignVm,Luster.PreviewHost`。这样不用 rebuild 业务模块。
(也可放 View 同程序集,但那要 rebuild 业务模块、且 mock VM 会进业务 DLL,不推荐。)

mock VM 要求:public、无参构造、属性填示例值。例如 View 是 `Luster.Motion.DigitalSetup.Views.ParamView`,在 `src/Tools/Luster.PreviewHost/Fixtures/ParamDesignVm.cs` 写:

```csharp
public class ParamDesignVm
{
    public string Title { get; set; } = "运动参数";
    public double Speed { get; set; } = 25.5;
    public ObservableCollection<AxisInfo> Axes { get; set; } = new()
    {
        new AxisInfo { Name = "X轴", Position = 100.0 },
        new AxisInfo { Name = "Y轴", Position = 50.0 }
    };
}
```

> 为何要 mock VM:View 的布局依赖绑定数据,空 DataContext 会让列表/文本塌缩,截图失真。mock 让预览接近真实运行样貌。这也和 Blend 设计器共用同一套机制——你用 Blend 调 UI 时用的 `d:DesignInstance`,这里也能复用(但运行时解析需用 `--designvm` 传全名,见下文坑)。

### Step 2: 截图

```bash
artifacts/bin/net472/Luster.PreviewHost.exe \
  --view Luster.Motion.DigitalSetup.Views.ParamView,Luster.Motion.DigitalSetup \
  --designvm Luster.Motion.DigitalSetup.Views.ParamDesignVm,Luster.Motion.DigitalSetup \
  --out workspace/wpf-preview/ParamView/runs/$(date +%Y%m%d-%H%M%S).png \
  --width 1920 --height 1080
```

**参数说明:**
- `--view <类型全名,程序集>` — 要截图的 View(AssemblyQualifiedName 简短形式)。
- `--designvm <类型全名,程序集>` — mock VM 类型(可选;不给则空 DataContext)。
- `--assembly <dll路径>` — 若 View 在 PreviewHost 未直接引用的外部模块,补上 DLL 路径让宿主加载。
- `--xaml <源xaml路径>` — 尝试从源文件解析 `d:DesignInstance`(备用;多为 `local:` 别名运行时不可解析,优先用 `--designvm`)。
- `--out <png>` — 截图输出路径。
- `--width/--height` — 渲染尺寸(默认 1920×1080)。

**退出码:**
- `0` = 成功,生成了 PNG。
- `1` = 渲染失败(View 找不到/实例化失败/依赖缺失)——看 stderr。
- `3` = XAML 加载失败——可能是主题字典 pack URI 解析不到(目标模块 DLL 没拷到宿主输出目录),也可能是 View 里某个 `{StaticResource}` 资源键在已加载主题里解析不到。看 stderr 的 `XamlParseException` 详情定位是哪个资源。

成功后到工作区目录归档截图:`workspace/wpf-preview/<View名>/runs/<时间戳>.png`。

### Step 3: 视觉评阅

```bash
export SILICONFLOW_API_KEY="你的key"
artifacts/bin/net472/Luster.VisualReviewer.exe \
  --screenshot workspace/wpf-preview/ParamView/runs/<时间戳>.png \
  --report workspace/wpf-preview/ParamView/reports/<时间戳>.json \
  --view ParamView
```

**参数说明:**
- `--screenshot <png>` — 必需,PreviewHost 落盘的截图。
- `--report <out.json>` — 必需,JSON 报告输出路径。
- `--view <名>` — 可选,View 名(用于报告与 index;默认取截图文件名)。

Reviewer 会:读截图 → 调 Qwen3-VL → 写 JSON 报告 → 追加工作区 `index.md`(View 级 + 根级迭代历史)。

**⚠️ 评阅报告可信度预警(重要):**
- **视觉模型只评像素可见维度**(overlap 重叠 / spacing 留白 / layout 对齐分区 / font 字号视觉大小)。这是设计如此——模型从像素只能看到这些。
- **源码级合规不在评阅范围**:控件库前缀 `hc:`、资源键 `{StaticResource}`、校验样式、字号档位等,模型从像素看不到,**不会报这些(不是漏评)**。源码级检查走 **Step 4 XamlLinter**(静态解析),不要期待 Reviewer 覆盖。
- **报告仅供参考,勿盲信 Score**;每条 issue 应带 evidence(截图里实际看到的客观描述),无 evidence 的描述可忽略。
- **看不出问题就 0 issues + 高分是正常的**(视觉无瑕疵),不是模型偷懒。原 prompt 把整份契约喂模型、逼它评源码级维度,导致套模板瞎猜源码级假问题(如把 `hc:ComboBox` 说成"原生控件"、Score 0)——此根因已修复(prompt 改为只评像素可见维度 + 要求 evidence)。

**退出码:**
- `0` = 评阅完成。
- `1` = 参数错误(缺 `--screenshot`/`--report`)。
- `2` = 视觉模型不可达(网络/key 失效)——仍落盘报告(标 Degraded);截图由 PreviewHost 此前已落盘,可人工看图。

**JSON 报告结构:**
```json
{
  "View": "ParamView",
  "Summary": "整体布局清晰,但参数区与状态区间距过小",
  "Score": 7,
  "DesignData": "present",
  "Degraded": false,
  "Issues": [
    {"Severity": "high", "Category": "overlap", "Description": "参数区与状态区控件重叠", "Location": "右下角"},
    {"Severity": "medium", "Category": "font", "Description": "标题未用约定字号档位"}
  ]
}
```
> 上例是"理想形态"示意。优化 prompt 后模型只评像素可见维度、要求 evidence,不再套模板瞎猜源码级假问题;视觉无瑕疵时会得高分 + 0 issues(正常)。**仍建议先看截图再读报告**,报告当线索(见上面的可信度预警)。

评阅维度(像素可见,定义在 prompt 与 `docs/wpf-design-contract.md`):`overlap`(重叠)/ `spacing`(留白)/ `layout`(对齐分区)/ `font`(字号视觉大小)。源码级维度如 `control-lib`(控件库前缀 `hc:`)不在视觉评阅范围,走 **Step 4 XamlLinter**。

### Step 4: XAML 静态检查(源码级维度)

视觉模型只评像素可见维度,源码级合规(裸控件/写死颜色尺寸/内联 Style/字号档位)由独立工具 `Luster.XamlLinter` 覆盖。它用 `XamlXmlReader` 解析 View XAML 节点流,**不依赖视觉模型、可离线运行、报告带行号**。与 Reviewer 报告并列:Step 3 看像素,Step 4 看源码。

```bash
artifacts/bin/net472/Luster.XamlLinter.exe \
  --xaml src/Modules/.../ParamView.xaml \
  --report workspace/wpf-preview/ParamView/lint-$(date +%Y%m%d-%H%M%S).json \
  --view ParamView
```

**参数说明:**
- `--xaml <path>` — 必需,View 源文件路径。
- `--report <out.json>` — 必需,JSON 报告输出路径。
- `--view <名>` — 可选,View 名(默认取 xaml 文件名)。

**退出码:**
- `0` = 检查完成(可能有 issue;**建议式不阻塞**,issue 是改进建议而非失败)。
- `1` = 参数错误 / XAML 解析失败(看 stderr)。

**检查规则(v1,5 条,对应契约源码级维度):**
| 规则 | Severity | 说明 |
|---|---|---|
| `bare-control` | high | 裸 `<Button>`/`<TextBox>`/`<Border>` 等应改 `hc:` 或 Luster.Controls.Wpf 封装 |
| `hardcoded-color` | medium | 颜色属性写死 `#hex` 应 `{StaticResource}` 引主题色键 |
| `hardcoded-size` | medium | 尺寸属性写死像素值(含 `Margin="5,2"` 多值)应引 Sizes.xaml Key |
| `inline-style` | medium | View 内联 `<Style>` 应进资源字典 |
| `font-size-tier` | low | `FontSize` 不在三档(12/14/20) |

- `d:` 设计时属性(如 `d:DesignHeight`)跳过;标记扩展(`{StaticResource}`/`{Binding}`)不报;注释/字符串里的 `<Button>` 不会误报(节点流天然过滤)。
- **v1 不验 `{StaticResource Key}` 的 Key 是否存在**(键合法性留 v2,需反射加载主题字典)。

> 上表 `bare-control` 正是补 Reviewer 的盲区:视觉模型从像素分不清裸 `Button` 与 `hc:Button`(都渲染成按钮),静态解析靠命名空间精确区分。实测 AddUserDialog 报出 6 个真裸控件(TextBox/PasswordBox/Button)而**不误报**其中的 `hc:ComboBox`。

### Step 5: 按报告改 XAML,回到 Step 2

截图时间戳递增,保留迭代历史。多轮后 `workspace/wpf-preview/<View>/index.md` 会累积每轮评分与主要问题,一眼看走向。

> `index.md` 是**追加累积**语义:每次评阅都追加一行,不会自动清旧记录。若混入了非本次产生的行(如历史测试),那是正常的累积,可手动删。

## 设计契约(`docs/wpf-design-contract.md`)

人读规范 + 视觉模型评阅标准,双用途。改动契约会直接影响评阅结果。关键条款:

- **控件库**:一律用 HandyControl + `Luster.Controls.Wpf`,禁原生 `Button`/`TextBox`/`Border` 拼凑。
- **资源键**:色/字号/间距用 `{StaticResource <Key>}` 引用(契约里列了 90+ 真实 Key),不写死值。
- **字号档位**:标题/正文/标签三档。
- **布局**:工业界面紧凑,主操作区/状态区/参数区分区。
- **Blend**:鼓励用 Blend for Visual Studio 做可视化设计与样式微调;设计时数据用 `d:DesignInstance`,产物进资源字典不散落。

改 UI 前先读这份契约,它就是"好坏的尺子"。

## 常见坑

1. **`d:DesignInstance="local:Vm"` 运行时解析不了**。`d:` 前缀带 `mc:Ignorable`,编译进 BAML 时被剥离;且 `local:` 是 XAML 别名,源文件解析也拿不到全名。所以**总用 `--designvm <全名,程序集>` 显式传**,不要指望 `--xaml` 自动解析。

2. **跨线程渲染**。PreviewHost 在主 STA 线程加载主题并实例化 View(同线程,共享 Dispatcher),引用 HandyControl 未冻结画刷不会崩。**不要**把 ViewRenderer 改回新建工作线程渲染——会因线程亲和性抛 `InvalidOperationException` 且渲染空白。

3. **外部模块 View 找不到**。PreviewHost 直接引用了 `Luster.Common.Assets`/`Luster.Motion.Assests`/`Luster.Controls.Wpf` 等,这些模块的 View 可直接 `--view`。其他模块的 View 需 `--assembly <dll路径>` 让宿主加载(或给类型全名含程序集名让兜底解析)。

4. **截图空白/近空白**。检查:① View 是否真有内容(不是空 Grid);② mock VM 是否填了数据(空集合会让列表塌缩);③ 主题是否加载成功(退出码 3?);④ 本地化文本空(见坑6)。PNG 字节数 > 5000 不能证明非空白(浅色近空白图压缩后也可能超)——**人工或视觉模型确认截图有内容再评阅**,否则 Reviewer 会瞎评(见 Step 3 预警)。

5. **`prism:ViewModelLocator.AutoWireViewModel` 在 PreviewHost 不生效**。PreviewHost 没有 Prism 容器,AutoWire 静默失败(被 `--designvm` 覆盖,不崩),DataContext 完全由 `--designvm` 决定。不要期待真实 VM 的逻辑/数据跑起来——那是 mock VM 的职责。

6. **本地化 `{lang:Lang Key=...}` 未初始化致截图文本空**。PreviewHost 未初始化 `LangProvider`,含本地化标记的 View(按钮文字、部分标签)会渲染成空。**这是预期,评阅时别当成缺陷。** 若需要文字,可在 mock VM 里把关键文本当属性直接绑(绕过本地化),或接受空文本只看布局。

7. **build 失败 "文件被另一个进程锁定"**。`LusterMotion.exe` 在运行,关掉再 build。

## 与开发流水线集成

这套闭环天然嵌入 CLAUDE.md 的 PM/Dev/Review 流水线:

- **Dev 子 Agent** 写完 View(带 mock VM)→ 跑 Step 2 截图 + Step 3 评阅 → 读报告改 XAML → 迭代到无 high 级问题。
- **Review 关卡** 读工作区 `index.md` 与最新报告,判断 UI 是否达标,不达标回喂 Dev。

建议式不阻塞:报告是参考,不强制 pass/fail 关卡。

## 快速自检(验证闭环可用)

用仓库自带的夹具 View 跑通整条链:

```bash
dotnet build LMV-2026.sln
artifacts/bin/net472/Luster.PreviewHost.exe \
  --view Luster.PreviewHost.Fixtures.ThemedSampleView,Luster.PreviewHost \
  --designvm Luster.PreviewHost.Fixtures.ThemedSampleDesignVm,Luster.PreviewHost \
  --out workspace/wpf-preview/selftest.png --width 400 --height 300
echo "退出码: $?"  # 应为 0
ls -la workspace/wpf-preview/selftest.png  # 约 7000+ bytes
```

退出码 0 且 PNG 有内容(可用视觉模型或人工看一眼确认非空白),说明闭环工具链可用,可以开始预览你的真实 View。
