# Luster.Motion.Assests — Motion 资源包

> 路径：`src/Modules/Luster.Motion.Assests/`  
> 类型：类库（NuGet 内部包，`GeneratePackageOnBuild=True`）  
> 输出：`Luster.Motion.Assests.dll` → exe 根目录

## 项目简介

`Luster.Motion.Assests` 是 **Motion 系统的 WPF 资源包**，提供 Motion 专用的 UI 主题、图标字体、图片资源和多语言支持。该项目作为 NuGet 内部包被 Motion UI 模块引用，确保整个 Motion 系统的视觉风格统一。

## 核心职责

- **WPF 主题系统** - 提供 Motion 专用的 XAML 主题（Themes/）
  - 颜色定义（Colors.xaml）
  - 控件样式（Style.xaml）
  - 资源字典（ResourceDictionary）
- **图标字体** - 内嵌 iconfont.ttf 字体文件，提供矢量图标
- **图片资源** - 包含 Motion 系统所需的所有 PNG/ICO 图片
  - 导航图标（Home、Alarm、Config、Data、Vision 等）
  - 状态图标（Run、Stop、Paused、Recovery、Zero 等）
  - 语言切换图标（chinese.png、english.png）
  - 提示图标（信息、警告、错误、成功）
- **多语言支持** - 通过 .resx 资源文件提供中英文双语
  - Lang.resx（中文）
  - Lang.en.resx（英文）
  - LangProvider.tt（T4 模板自动生成语言提供者）
- **扩展方法** - 提供 WPF 相关的扩展方法（Extention/）

## 依赖关系

### 项目引用

| 项目 | 说明 |
|------|------|
| `Luster.Common.Assets` | 通用资源基础库 |

### NuGet 包依赖

| 包名 | 用途 |
|------|------|
| `Dirkster.AvalonDock` | WPF 停靠窗口布局系统 |
| `HandyControl` | WPF UI 控件库 |
| `MinVer` | 自动版本号生成 |

## 资源目录结构

```
Luster.Motion.Assests/
├── Fonts/
│   └── iconfont.ttf          ← 图标字体
├── Images/                   ← 图片资源（50+ 张）
│   ├── HomeNormal.png / HomeSelect.png
│   ├── AlarmNormal.png / AlarmSelect.png
│   ├── RunNormal.png / RunSelect.png / RunDisable.png
│   ├── chinese.png / english.png
│   └── 信息.png / 警告.png / 错误.png / 成功.png
├── Themes/                   ← WPF 主题
│   ├── Basic/Colors/
│   └── Styles/
├── Langs/                    ← 多语言资源
│   ├── Lang.resx             ← 中文（默认）
│   ├── Lang.en.resx          ← 英文
│   ├── Lang.Designer.cs      ← 自动生成
│   └── LangProvider.tt       ← T4 模板
└── Extention/                ← WPF 扩展方法
```

## 输出到 exe 目录

`Luster.Motion.Assests.dll` → Shell 输出目录根下

**资源嵌入方式：**
- 所有图片和字体作为 `Resource` 嵌入到 DLL 中
- 多语言 .resx 文件作为 `EmbeddedResource` 嵌入
- 运行时通过 `pack://application:,,,/Luster.Motion.Assests;component/...` URI 访问
