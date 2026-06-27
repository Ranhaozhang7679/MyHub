# WPF 页面设计契约(LMV-2026)

> 人读规范 + 视觉模型评阅维度来源。VisualReviewer 的 prompt 自包含像素可见维度(见 §7),**不加载本文档全文**;开发人员新增/修改 View 时,本文档为硬性约束。维度变更需同步 §8 清单所列三处。

> 资源键来源:`src/Modules/Luster.Common.Assets/Themes/` 与 `src/Modules/Luster.Motion.Assests/Themes/`,由 Shell `App.xaml` 全局合并(含 HandyControl `SkinDefault.xaml` + `Theme.xaml`)。本文档所述 Key 均为实际扫描结果,非占位。

---

## 1. 控件库优先级

- **一律使用 HandyControl + Luster.Controls.Wpf(+ Luster.Control.Wpf.Motion)**,禁止用原生 `Button` / `TextBox` / `Border` / `CheckBox` 等拼凑自定义控件。
  - 按钮:`hc:Button` 或 `Luster.Controls.Wpf` 封装按钮;**不要**裸 `<Button>` 自绘背景。
  - 输入:`hc:TextBox` / `hc:NumericUpDown` / `hc:ComboBox`;表单校验直接套用 `TextBoxValidationStyle` / `NumericUpDownValidationStyle` / `ComboBoxValidationStyle`(见 §2)。
  - 列表/表格:`hc:DataGrid` + `DataColumnHeaderStyleCenter` / `DataColumnTextElementStyleCenter` 等列样式;**不要**自绘 `ListBox` 模板。
  - 对话框:`hc:MessageBox`,并套用 `MessageBoxCustom` 样式(已含品牌色 NonClientAreaBackground=#262e2f 与 logo)。
- **样式/模板进资源字典**,不要散落在各 View 的内联 `<Style>`。新增样式应进 `Luster.Common.Assets/Themes/Styles/Style.xaml`(通用)或 `Luster.Motion.Assests/Themes/Styles/Style.xaml`(运控专属),View 内只 `{StaticResource XxxStyle}` 引用。
- HandyControl 提供的标准画刷(`PrimaryBrush` / `DangerBrush` / `SuccessBrush` / `WarningBrush` / `InfoBrush` / `AccentBrush` / `RegionBrush` / `BorderBrush` / `TextBrush` 等)经 `SkinDefault.xaml` 全局可用,**优先使用**,不要写死 hex。

---

## 2. 资源键引用

色 / 字号 / 间距 / 圆角 **必须** `{StaticResource <Key>}` 或 `{DynamicResource <Key>}` 引用,**禁止**在 View 里写死 hex 字符串或裸像素值。主题色键用 `DynamicResource`(支持亮/暗皮肤切换);固定画刷(如 `lusterColor`)用 `StaticResource` 即可。

### 2.1 颜色(Color)— `Basic/Colors/Colors.xaml` 亮色 + `ColorsDark.xaml` 暗色

| Key | 亮色值 | 暗色值 | 用途 |
|-----|--------|--------|------|
| `MainBackgroundColor` | `#262e2f` | `#262e2f` | 主背景色(Shell 外壳) |
| `MainContentForegroundColor` | `#FFF0F0F0` | `#FF1A1A1A` | 内容区前景(文字) |
| `MainContentBackgroundColor` | `#FFF5F5F5` | `#FF1C1C1C` | 内容区背景 |
| `ControlAccentColorKey` | `#1ba1e2` | `#1ba1e2` | 控件强调色(蓝) |
| `EditorBackgroundColor` / `EditorForegroundColor` / `EditorLineNumbersForegroundColor` | White / Black / Black | `#FF181818` / `#FFFFFFFF` / `#ff929292` | AvalonEdit 编辑器 |
| `EditorNonPrintableCharacterColor` / `EditorLinkTextForegroundColor` / `EditorLinkTextBackgroundColor` | `#3F8080FF` / `#FF4040FF` / `#00000000` | `#2FFFFFFF` / `#FFAAAAFF` / `Transparent` | 编辑器辅助色 |

> 颜色键一般不直接引用,通过下文画刷键间接消费。

### 2.2 画刷(Brush)— `Basic/Brushes.xaml`

**主题画刷(随皮肤切换,用 `DynamicResource`):**

| Key | 绑定 Color | 用途 |
|-----|-----------|------|
| `MainBackgroundBrush` | `MainBackgroundColor` | 主背景 |
| `MainContentForegroundBrush` | `MainContentForegroundColor` | 内容区文字 |
| `MainContentBackgroundBrush` | `MainContentBackgroundColor` | 内容区背景 |
| `ControlAccentBrushKey` | `ControlAccentColorKey` | 强调蓝 |
| `RegionBrush` | — (`#F9F9F9`) | 区块/面板底色 |
| `EditorBackground` / `EditorForeground` / `EditorLineNumbersForeground` | 对应 Editor*Color | 编辑器 |
| `EditorSelectionBrush` / `EditorSelectionBorder` | `ControlAccentColorKey`(Opacity 0.75) | 编辑器选区 |

**固定画刷(不随皮肤变,可用 `StaticResource`):**

| Key | 值 | 用途 |
|-----|----|------|
| `lusterColor` | `#D8504D` | **品牌红**,菜单/按钮 hover 强调(Style.xaml 全局 Button/MenuItem hover 即用此色) |
| `MenuButtonBorderBrush` | 白→`#F2F2F2` 竖向渐变 | 菜单按钮边框 |
| `ToolBarBackground` | `#F5F4F5`→`#D1CFD1` 竖向渐变 | 工具栏背景 |
| `CloudDrawingBrush` / `MainContentForegroundDrawingBrush` | DrawingBrush | 装饰性平铺纹理 |

> `Luster.Motion.Assests` 额外补充:`GridBackgroundColor` = `#FFF0F2F5`(表格底色)、`TitleBackgroundColor` = `#414549`(标题栏底色)。

### 2.3 字号档位(Font)— `Basic/Fonts.xaml`

资源字典里**未定义**正文字号 token,只有图标字号样式(见下)。正文/标题/标签三档走 §3 默认值。

**图标字号样式(TextBlock,iconfont 字体):**

| Key | FontSize | 用途 |
|-----|----------|------|
| `IconSmall` | 16 | 列表/工具栏小图标 |
| `IconMid` | 28 | 卡片/中型图标 |
| `IconLarge` | 32 | 大图标 |
| `TextBlockFabricIcons` | 16 | 通用 Fabric 图标(Style.xaml) |

> `Luster.Motion.Assests` 镜像定义了 `MotionIconSmall` / `MotionIconMid` / `MotionIconLarge`(同尺寸,字体路径指向 Motion.Assests 自带 iconfont)。运控模块内图标二选一,不要混用。

### 2.4 尺寸/间距(Size)— `Basic/Sizes.xaml`

| Key | 类型 | 值 | 用途 |
|-----|------|----|------|
| `DefaultControlHeight` | Double | `28` | 标准控件高度(按钮/输入框/下拉) |
| `DefaultControlPadding` | Thickness | `10,4` | 标准控件内边距 |
| `DefaultInputPadding` | Thickness | `8,4` | 输入控件内边距 |
| `DefaultCornerRadius` | CornerRadius | `4` | 标准圆角 |

> 控件高度、内边距、圆角一律引用上述 Key,**禁止** `Height="30"` `Padding="5,2"` `CornerRadius="6"` 等写死值。如需新档位,在 `Sizes.xaml` 新增 Key 并在本文档登记,不要在 View 内就地写死。

### 2.5 通用样式(Style)— `Themes/Styles/Style.xaml`

| Key | TargetType | 用途 |
|-----|-----------|------|
| `MessageBoxCustom` | `hc:MessageBox` | 品牌化对话框(外壳 `#262e2f` + logo) |
| `CirclePanelButton` | Button | 圆形面板按钮 |
| `TabItemTransparent` | TabItem | 透明背景 Tab |
| `ListBoxTransparent` / `ListBoxItemTransparent` / `ListBoxItemNew` | ListBox / ListBoxItem | 透明列表(带 NEW 角标) |
| `TextBlockFabricIcons` | TextBlock | Fabric 图标文字 |
| `DataColumnHeaderStyleCenter` / `DataColumnHeaderSelectAllStyle` | DataGridColumnHeader | 表头居中 / 全选表头 |
| `DataColumnTextElementStyleCenter` / `DataColumnComboboxElementStyleCenter` / `DataColumnCheckBoxElementStyleCenter` | TextBlock / ComboBox / CheckBox | 单元格内容居中 |
| `TextBoxValidationStyle` / `NumericUpDownValidationStyle` / `ComboBoxValidationStyle` | TextBox / NumericUpDown / ComboBox | 带校验提示的表单控件(TitleWidth=80,左标题,红框+ToolTip 报错) |
| `Path4GeometryItem` / `GroupItemStyle` | Path / GroupItem | 几何路径项 / DataGrid 分组头 |
| `Custom1Transition` / `Custom2Transition` / `Custom3Transition` | Storyboard | 进场动画(位移/旋转/缩放) |
| `FluidMoveBehaviorWrapPanelItemsPanelTemplate` | ItemsPanelTemplate | 流动重排 WrapPanel |

### 2.6 几何图标(Geometry)— `Basic/Geometries.xaml`

大量 `XxxGeometry` Key(`HomeGeometry` / `SaveGeometry` / `OpenFolderGeometry` / `NewFolderGeometry` / `RunAllGeometry` / `RunOneGeometry` / `LoopGeometry` / `StopGeometry` / `SettingGeometry` / `ConfirmGeometry` / `ConnectGeometry` / `DisConnectGeometry` / `CoordinateGeometry` / `PositionGeometry` / `EditGeometry` / `ExportGeometry` / `LogGeometry` / `ReportGeometry` / `RedoGeometry` / `RevokeGeometry` / `AddCloudGeometry` / `AddSTLGeometry` / `LengthGeometry` / `AngularityGeometry` / `RoundnessGeometry` / `CylindricityGeometry` / `StraightnessGeometry` / `FlatnessGeometry` / `VerticalityGeometry` / `ParallelismGeometry` / `LineProfileGeometry` / `SurfaceProfileGeometry` / `SymmetryGeometry` / `CoaxialityGeometry` / `ColorSelectorGeometry` / `PinPointGeometry` / `RunNextGeometry` / `BullsEyeGeometry` / `LoadGeometry` / `CorrectGeometry` / `ForwardGeometry` / `ImportantGeometry` / `MathGeometry` / `AnimationGeometry` / `NewGeometry` / `CirclePanelDemoGeometry` / `CirclePanelRightGeometry` / `ImageGeometry` / `LoveGeometry` / `BlogGeometry` / `VisualStudioGeometry` / `SphereGeometry` / `CuboidGeometry` / `HiddenGeometry`)。

> 图标一律 `{StaticResource XxxGeometry}` 配 `Path`/`IconElement`,**禁止**用 emoji/字符/图片代替。优先复用已有几何,新增几何先进 `Geometries.xaml` 登记再引用。

---

## 3. 字号档位

资源字典未定义正文字号 token,本契约固定三档,**不得**自由设值:

| 档位 | 字号 | 字重 | 用途 |
|------|------|------|------|
| 标题 | `20` px | Bold | 页面/分区标题、对话框主标题 |
| 正文 | `14` px | Normal | 列表、表格、表单、说明文字(默认) |
| 标签 | `12` px | Normal | 输入框 Title、次要标注、表头副文字、状态标签 |

- 图标字号走 §2.3 的 `IconSmall/Mid/Large`(16/28/32),**不算**正文档位。
- 后续如需引入字号 token,在 `Fonts.xaml` 新增 `FontSizeTitle`/`FontSizeBody`/`FontSizeCaption` 并回填本节;在登记前仍用裸数值 20/14/12。

---

## 4. 布局分区

工业界面**紧凑、信息密度高**,符合操作员站姿触屏/鼠标双操作习惯。

- **三分区结构**:主操作区(左/中,占主导)、状态区(顶/底状态条,实时反馈)、参数区(右/侧栏,配方/参数编辑)。区与区之间用 `Margin`(建议 8~16)或 `Grid` 列分隔,**不要**用空白 Border 凑间距。
- **网格对齐**:同一行控件基线对齐,高度统一引用 `DefaultControlHeight`(28);同列控件左对齐,标题列宽统一(表单用 `hc:InfoElement.TitleWidth=80`,与校验样式一致)。
- **紧凑优先**:行间距 4~8,组间距 12~16;非交互装饰元素最小化,留白服务于操作焦点而非美观。
- **状态可见性**:运行/停止/报警等状态用 `lusterColor`(品牌红)或 HandyControl `DangerBrush`/`SuccessBrush`/`WarningBrush` 高亮,**禁止**自创颜色。
- **响应式**:对话框/面板用 `Grid` 比例列(`Width="*"`),避免固定像素宽度导致 DPI/分辨率不适配;`hc:SimplePanel`/`SimpleStackPanel` 优于裸 `Canvas`。

---

## 5. MVVM 契约

- **ViewModel 命名**:一律 `VM` 后缀(`MainVM`、`MotionConfigVM`),与 View 通过名称约定自动关联。
- **View-VM 关联**:View 根节点声明 `prism:ViewModelLocator.AutoWireViewModel="True"`,**不要**在 code-behind 手动 `new VM()` 设 `DataContext`(设计时数据除外,见 §6)。
- **命令**:用 `Prism` 的 `DelegateCommand` / `DelegateCommand<T>`;异步命令暴露 `IsBusy` 给 View 绑定 loading。
- **属性通知**:VM 实现 `INotifyPropertyChanged`(继承 `BindableBase` 或 `NotificationObject`),**禁止**裸字段绑定。
- **多语言**:文字一律 `{Binding Langs[xxx]}`(`Langs` 为 `LangProvider`,App.xaml 全局注册);运控专属文字用 `{Binding MotionLangs[xxx]}`,设备仿真用 `DeviceLangs`。**禁止**在 XAML 写死中/英文字面量。
- **权限**:按钮/菜单可见性走 `AuthVisibilityConverter`(`{Binding ..., Converter={StaticResource AuthVisibilityConverter}}`),不要在 VM 里手撸权限判断。
- **Region 导航**:页面切换用 Prism `RegionManager.RequestNavigate`,不要自建 `ContentControl.Content` 切换逻辑。

---

## 6. Blend 鼓励条款(设计时数据与可视化设计)

> 本节为控制器明确要求,VisualReviewer 评阅时把"是否用 `d:DesignInstance` 提供设计时数据"作为加分项。

- **鼓励使用 Blend for Visual Studio** 做可视化设计:拖拽布局、调样式/控件模板、微调动画时间线。Blend 产出的样式与控件模板**必须**进资源字典(`Luster.Common.Assets/Themes/Styles/Style.xaml` 通用,或对应模块 `Themes/Styles/`),**不得**散落各 View 内联。
- **设计时数据一律用 `d:DesignInstance`**,人机共用(PreviewHost 截图与 Blend 设计器走同一份数据)。形如:
  ```xml
  <UserControl ...
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      mc:Ignorable="d"
      d:DataContext="{d:DesignInstance Type=module:SomeFeatureVM, IsDesignTimeCreatable=True}">
  ```
  - **全名形式**:`Type` 用完全限定类型(`module:SomeFeatureVM`,即 `xmlns:module="clr-namespace:Luster.xxx;assembly=..."`),便于 PreviewHost 的 `DesignInstanceParser` 反射解析;**不要**用 `d:DesignData` 或省略 namespace 的短名。
  - `IsDesignTimeCreatable="True"`:VM 需有无参构造(或默认构造注入假数据),保证 Blend 与 PreviewHost 都能实例化。
  - 集合属性在 VM 默认构造里填充 3~5 条样例数据,覆盖正常/空/异常态,确保截图与 Blend 预览有内容、可评阅。
- **Blend 资源字典落地**:新增样式/模板键须同步登记到本文档 §2.5;移除/改名键须同步更新文档,避免 View 引用悬空。
- **禁止**用 `d:DesignInstance` 之外的 `d:DesignData` `.xaml` 文件(冗余且易与 VM 漂移);也**禁止**在 code-behind 写 `if (DesignerProperties.GetIsInDesignMode)` 分支塞假数据。

---

## 7. 评阅维度(视觉模型用)

VisualReviewer 调用视觉模型时,以下维度作为评阅 checklist,每项给出 pass / warn / fail + 证据(坐标或截图描述):

| 维度 | 检查点 | 判 fail 的典型情况 |
|------|--------|--------------------|
| **overlap** | 控件无重叠、无遮挡 | 文字被图标盖住、按钮互相压、表格列内容被截断 |
| **spacing** | 留白/间距一致、无突兀 | 同行控件间距不一、Margin 负值导致贴边、分区无间距挤成一团 |
| **font** | 字号走三档(20/14/12)或图标档(16/28/32) | 出现 13/15/18 等非档位字号、标题不够大、正文过小 |
| **layout** | 三分区清晰、对齐、紧凑 | 主操作区与状态区混淆、控件基线不齐、固定像素宽度在缩放下溢出 |

> 源码级维度(控件库前缀 `hc:`、资源键 `{StaticResource}`、写死值、字号档位字面值)不在视觉模型评阅范围——视觉模型从像素看不到这些。源码级检查由 `Luster.XamlLinter` 静态解析覆盖,见 SKILL.md Step 4。

> 评阅输出 JSON,字段:`overlap` / `spacing` / `font` / `layout`,每项 `{verdict, evidence}`;任一 fail 则整体不通过,回写 Issue 到工作区索引供下游修复。

---

## 8. 维护约定

- **新增资源键**:先在对应 `Basic/*.xaml` 或 `Styles/Style.xaml` 定义,再回填本文档 §2 对应小节,最后在 View 引用。三步缺一即视为违反契约。
- **删除/改名资源键**:全局 `Grep` 确认无引用,同步更新本文档,避免 View 引用悬空导致 `StaticResource` 解析异常(运行时白屏)。
- **亮/暗皮肤**:颜色键必须同时提供 `Colors.xaml`(亮)与 `ColorsDark.xaml`(暗)两份;画刷用 `DynamicResource` 绑定 Color,保证皮肤切换生效。
- **契约与 PreviewHost/VisualReviewer/XamlLinter 同步**(防漂移):本文档维度变更(如新增/移除评阅维度、改禁裸清单、改字号档位)须同步检查以下三处,缺一即视为违反契约:
  1. `src/Tools/Luster.VisualReviewer/VisualReviewClient.cs` 的 `CallModel` prompt 模板(像素可见维度);
  2. `src/Tools/Luster.XamlLinter/RuleConfig.cs` 规则清单(源码级维度:禁裸控件/颜色属性/尺寸属性/字号档位);
  3. 本节(§8)及 §7。
  > 历史曾因改 prompt 未同步契约导致 `control-lib` 维度漂移(视觉模型套模板瞎报源码级假问题),故立此清单。`ContractReader` 已于 P1 移除(prompt 不再加载契约全文),勿再引用。
