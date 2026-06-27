# WPF 页面视觉反馈闭环 设计文档

- 日期:2026-06-27
- 作者:RanHaoZhang + Claude
- 状态:待评审
- 关联分支:待建 `feature/wpf-visual-feedback-loop`

## 1. 背景与动机

LMV-2026 是基于 .NET Framework 4.7.2 + WPF 的企业级运动控制系统,采用 Prism (DryIoc) 模块化架构。Agent 辅助开发 WPF 页面时存在核心痛点:**Agent 看不见渲染结果**。Web 前端 Agent 能用 Playwright 截图自检,WPF 没有这个闭环——Agent 写完 XAML 只能靠脑补布局,改一遍盲改一遍,页面质量难以保证。

进一步摸排发现项目当前**没有任何设计时预览机制**(全仓搜索 `d:DesignInstance` / `IsInDesignMode` / `DesignTimeAssets` 均无命中),主题字典统一在 `src/Shell/LusterMotion/App.xaml` 合并(HandyControl SkinDefault/Theme + `Luster.Common.Assets` + `Luster.Motion.Assests` + `Luster.Controls.Wpf`/`Luster.Control.Wpf.Motion`/`Luster.SimDevice.SubSystem` 的 Generic/Style)。

一切办法都围绕**把"看不见"变成"看得见"**,并为 Agent 提供"评好坏的尺子"。

## 2. 目标与非目标

### 目标

1. Agent 写完 WPF View 后,能用一条命令拿到该 View 的渲染截图(含真实主题、mock 数据)。
2. 截图可被视觉模型评阅,产出结构化报告(问题清单 + 严重度),供 Agent/Review 关卡参考。
3. 评阅标准来自项目内设计契约文件,人机共用,版本化、可演进。
4. 新增模块独立文件与工作区,截图/报告/测试数据在工作区内按目录组织,带索引可追溯迭代历史。

### 非目标

- 不做像素级 UI 回归(脆且 ROI 低)。
- 不阻塞流水线(评阅为建议式,不强制 pass/fail 关卡)。
- 不替代人工 Review,只补 Agent 自检盲区。
- 不改造现有 Shell 运行时或主题加载机制。

## 3. 关键决策(已与用户确认)

| 决策点 | 选择 | 理由 |
|--------|------|------|
| 预览宿主形态 | 独立预览 exe(`Luster.PreviewHost`) | 不依赖完整运行时(运控/设备引擎),Agent 最易调用,进程隔离 |
| mock 数据来源 | View 自带 `d:DesignInstance` mock | Agent 写页面顺手写设计时数据,宿主解析 XAML 提取类型反射实例化;质量高、语义准 |
| 评判标准来源 | 项目内设计契约文件 `docs/wpf-design-contract.md` | 一物两用:人读规范 + 视觉模型 prompt |
| 评阅结果用法 | 建议式出报告(不阻塞) | 灵活,不把流程绑死;阈值难调,先不硬关卡 |
| 主题复用路线 | 宿主自带精简 App.xaml,引用相同主题程序集的 pack URI | 合并顺序与 Shell 完全一致,保证"所见即运行";不耦合 Shell 运行时 |
| 工作区位置 | 项目内 `workspace/wpf-preview/` | 入 git 可追溯,迭代历史保留 |

## 4. 架构

四个组件,职责单一、可独立测试:

```
┌─────────────────────────────────────────────────────────────┐
│  Agent / Review 关卡                                         │
│  (调用两条命令,读报告决定是否回改;不耦合宿主内部)            │
└──────────┬──────────────────────────────┬───────────────────┘
           │ 1. 截图                       │ 2. 评阅
           ▼                               ▼
┌─────────────────────────┐   ┌──────────────────────────────┐
│  Luster.PreviewHost     │   │  VisualReviewer              │
│  (独立 WPF exe)          │   │  (控制台工具 / 子命令)        │
│                         │   │                              │
│  CLI:                   │   │  CLI:                        │
│   --view <类型全名>      │   │   --screenshot <png>         │
│   --assembly <dll>      │   │   --report <out.json>        │
│   --out <截图.png>       │   │   --contract <契约.md>       │
│   --width/--height      │   │                              │
│                         │   │  读契约 + 截图                │
│  解析 d:DesignInstance   │   │  调 Qwen3-VL(硅基流动)        │
│  反射实例化 mock VM       │   │  产结构化 JSON 报告           │
│  ContentControl 承载     │   │                              │
│  RenderTargetBitmap 截图 │   │  降级:网络失败仍落盘截图       │
└───────────┬─────────────┘   └───────────────┬──────────────┘
            │                                  │
            ▼                                  ▼
   workspace/wpf-preview/<View>/runs/*.png   .../reports/*.json
            └──────────────┬───────────────────┘
                           ▼
              workspace/wpf-preview/index.md (全局索引)
              workspace/wpf-preview/<View>/index.md (迭代历史)
                           ▲
                           │ 供人读规范 / 供模型当 prompt
              docs/wpf-design-contract.md (设计契约)
```

### 4.1 `Luster.PreviewHost`(新 WPF exe 模块)

- 新建独立 csproj,加入 `LMV-2026.sln`,输出到 `artifacts/bin/net472/`。
- 纯命令行工具,无窗口交互。
- 引用主题程序集(`Luster.Common.Assets`、`Luster.Motion.Assests`、HandyControl 等),宿主 App.xaml **照抄 Shell App.xaml 的 MergedDictionaries 顺序**(方案 A),保证主题与真实运行一致。
- CLI 接口:
  ```
  PreviewHost.exe --view <FullTypeName> --assembly <dll名或路径> \
                  --out <截图.png> --width 1920 --height 1080
  ```
- 内部流程:
  1. 加载目标程序集 → 定位 View 类型。
  2. 解析 View.xaml 的 `d:DesignInstance="..."` 声明 → 反射创建 mock VM 实例。
  3. 实例化 View,设 `DataContext = mock VM`。
  4. 放进固定尺寸 `ContentControl`,强制 `Measure/Arrange`。
  5. `RenderTargetBitmap` 截图存 PNG。
  6. 退出码:0=成功,1=渲染失败,3=主题加载失败。
- 失败时 stderr 输出明确原因,**不产空截图**。

### 4.2 `VisualReviewer`(控制台工具 / PreviewHost `--report` 子命令)

- 输入:截图 PNG + `docs/wpf-design-contract.md`。
- 调用 siliconflow `Qwen/Qwen3-VL-8B-Instruct`(已在 CLAUDE.md 配置 key/`--noproxy '*'`)。
  - 端点:`https://api.siliconflow.cn/v1/chat/completions`
  - 图像走 `messages` 里 `image_url`(base64),直连不走代理。
- 评阅维度:布局错位 / 控件重叠 / 留白异常 / 字号档位 / 是否走 HandyControl+资源键 / 工业界面紧凑度。
- 输出 JSON:
  ```json
  {
    "view": "YyyView",
    "screenshot": "workspace/wpf-preview/YyyView/runs/20260627-153012.png",
    "summary": "整体布局清晰,但参数区与状态区间距过小",
    "score": 7,
    "designdata": "present",
    "issues": [
      {"severity": "high",   "category": "overlap",     "description": "参数区与状态区控件重叠", "location": "右下角"},
      {"severity": "medium", "category": "spacing",     "description": "标题与正文未用约定字号档位"},
      {"severity": "low",    "category": "control-lib", "description": "存在原生 Button,应改用 HandyControl"}
    ]
  }
  ```
- 降级:视觉模型不可达时退码 2,**仍把截图落盘**(截图本身有价值,人可看)。

### 4.3 `docs/wpf-design-contract.md`(设计契约)

人读规范 + 视觉模型 prompt 双用途,纳入 git。内容:

- **控件库优先级**:一律用 HandyControl + `Luster.Controls.Wpf`,禁止原生 `Button`/`TextBox`/`Border` 拼凑。
- **资源键清单**:列出 `Luster.Common.Assets/Themes/Styles/Style.xaml` 等暴露的色板/字号/间距 Key,要求 `{StaticResource XxxKey}` 引用而非写死值。
- **字号档位**:标题/正文/标签三档,明确具体值。
- **间距/留白规范**。
- **布局分区规范**:工业界面紧凑、信息密度高、主操作区/状态区/参数区分区。
- **MVVM 契约**:VM 后缀、`ViewModelLocator.AutoWireViewModel="True"`、命令用 `DelegateCommand`、多语言走 `{Binding Langs[xxx]}`。

### 4.4 流水线集成(无新代码,约定)

Dev 产出 View(带 `d:DesignInstance` mock)→ 跑 `PreviewHost` 截图 → 跑 `VisualReviewer` 出报告 → Review 关卡读报告决定是否回喂 Dev。建议式,不阻塞。

## 5. 工作区结构

每 View 一个子目录,多次迭代追加,保留历史:

```
workspace/wpf-preview/
├── index.md                          # 全局索引:列出所有 View + 最近一次评分 + 状态
├── contract.md -> ../../docs/wpf-design-contract.md   # 指向契约(复制或软链)
└── <ViewName>/                       # 每个被预览的 View 一个子目录
    ├── runs/                         # 截图按时间戳归档,保留迭代历史
    │   ├── 20260627-153012.png
    │   └── 20260627-160115.png
    ├── reports/                      # 视觉评 JSON,与截图同名
    │   ├── 20260627-153012.json
    │   └── 20260627-160115.json
    ├── designdata/                   # 该 View 的 d:DesignInstance mock 源(View 内联则空)
    └── index.md                      # 该 View 迭代历史:时间/评分/主要问题/是否通过
```

截图与报告同名配对,方便对照。`index.md` 用表格记录每轮迭代,Agent/Review 关卡一眼看历史走向。

`.gitignore` 策略:`workspace/wpf-preview/**/*.png` 与 `*.json` 可按需 gitignore(产物),但 `**/index.md` 与 `contract.md` 入 git(追溯)。最终策略实施时确认。

## 6. 数据流(一次完整迭代)

1. Agent 写 View.xaml(带 `d:DesignInstance` mock)→ 编译对应模块。
2. Agent 跑:
   ```
   PreviewHost.exe --view Luster.xxx.YyyView --assembly Luster.xxx.dll \
                   --out workspace/wpf-preview/YyyView/runs/<ts>.png \
                   --width 1920 --height 1080
   ```
3. PreviewHost:加载程序集 → 定位 View → 解析 `d:DesignInstance` → 反射建 mock VM → 实例化 View 设 DataContext → 固定尺寸 `ContentControl` 强制 `Measure/Arrange` → `RenderTargetBitmap` 截图存 PNG → 退出码。
4. Agent 跑:
   ```
   VisualReviewer --screenshot <png> --report workspace/wpf-preview/YyyView/reports/<ts>.json
   ```
5. Reviewer 读契约 + 截图 → 调 Qwen3-VL → 产结构化 JSON。
6. Agent 读报告 → 改 XAML → 回到步骤 2(截图时间戳递增,保留历史)。
7. 完成后追加 `YyyView/index.md` 与根 `index.md`。

时间戳格式:`YYYYMMDD-HHMMSS`。

## 7. 错误处理与降级

| 失败场景 | 处理 |
|----------|------|
| PreviewHost 渲染失败(View 缺依赖 / DesignInstance 解析不到) | stderr 明确原因 + 退出码 1,**不产空截图**;Agent 据此修 View,不进视觉评 |
| 视觉模型不可达(网络 / key 失效) | Reviewer 退码 2 + 仍把截图落盘;Agent 可先收截图,网络恢复后补评 |
| `d:DesignInstance` 缺失 | PreviewHost 警告但仍渲染(空 DataContext,布局可能塌)→ 报告里标 `designdata: missing`,提示补 mock |
| 主题加载失败 | 退码 3,提示哪个字典 pack URI 解析不到(常见:对应模块 DLL 没拷到宿主输出目录,需在宿主 csproj 显式引用) |

退出码汇总:0=成功,1=渲染失败,2=视觉模型不可达,3=主题加载失败。

## 8. 测试策略

- **PreviewHost 单测**:用固定 fixture View(项目自带 `TestViews\SampleView.xaml`,带 mock VM)验证——能实例化、能截图、截图非空、尺寸符合参数。用快照对比像素尺寸/非全白,而非像素级比对(脆)。
- **DesignInstance 解析单测**:给含/不含 `d:DesignInstance` 的 XAML,验证解析逻辑正确提取类型名与 mock 实例化。
- **Reviewer 单测**:用固定 fixture 截图 + mock 视觉模型响应(桩接口,不走真实网络),验证 JSON 报告结构、严重度归类、降级路径(网络失败仍落盘截图)。
- **契约文件**:人工审阅,纳入 git;改动需同步宿主 prompt。

测试项目新建 `Luster.PreviewHost.Tests`,加入 sln。

## 9. 待后续确认 / 实施时细化

- VisualReviewer 是独立 exe 还是 PreviewHost 的 `--report` 子命令(影响打包/调用)。
- `workspace/wpf-preview/` 的 `.gitignore` 策略(产物是否入 git)。
- 设计契约 `docs/wpf-design-contract.md` 的具体内容(需扫 `Luster.Common.Assets` 资源键清单后填充)。
- 宿主 App.xaml 照抄清单的同步机制(Shell 改了主题合并顺序时如何感知)。

## 10. 后续步骤

本 spec 评审通过后,进入 writing-plans skill 产出实施计划,再按 PM/Dev/Review 流水线实现。
