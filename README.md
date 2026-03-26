# LM2026
# LM2026 - Luster Motion Control System

<div align="center">

![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue)
![WPF](https://img.shields.io/badge/WPF-Application-blueviolet)
![GitFlow](https://img.shields.io/badge/Workflow-GitFlow-orange)
![License](https://img.shields.io/badge/License-Proprietary-red)

**企业级运动控制系统 - 2026版本**

</div>

---

## 📋 目录

- [项目简介](#-项目简介)
- [🚀 新手快速开始](#-新手快速开始)
- [📐 开发规范](#-开发规范)
  - [GitFlow 工作流](#gitflow-工作流)
  - [Issue 管理](#issue-管理)
  - [分支命名规范](#分支命名规范)
- [🤖 CI/CD 自动化](#-cicd-自动化)
- [📁 项目结构](#-项目结构)
- [💻 开发指南](#-开发指南)
- [📚 文档索引](#-文档索引)
- [🤝 团队协作](#-团队协作)
- [❓ 常见问题](#-常见问题)

---

## 🎯 项目简介

**LM2026 (Luster Motion 2026)** 是一款企业级运动控制系统，基于 .NET Framework 4.7.2 和 WPF 技术栈开发。系统采用模块化架构设计，支持多种运动控制设备、视觉系统、激光控制等工业自动化场景。

### 核心特性

- **🎨 模块化架构** - 基于 Prism 框架的插件式模块设计
- **🔧 设备支持** - 支持运动卡、相机、激光器、打印机、机器人等多种设备
- **📊 任务流引擎** - 可视化任务流编排和执行引擎
- **🎯 运动控制** - 2D/3D 运动轨迹规划与算法
- **📈 数据可视化** - 实时数据监控和报表系统
- **🛠️ 仿真系统** - 完整的设备仿真环境

### 技术栈

- **.NET Framework 4.7.2** - 应用程序框架
- **WPF (Windows Presentation Foundation)** - UI 框架
- **Prism** - MVVM 框架和模块化支持
- **HandyControl** - 现代化 UI 控件库
- **CentralPackageManagement** - NuGet 包集中管理

---

## 🚀 新手快速开始

### 🎓 我是新手，如何开始？

欢迎加入 LM2026 项目！如果你是第一次接触本项目，请按照以下步骤快速上手：

#### 第一步：了解开发规范 ⚡

**必读文档（5分钟）：**

1. **[GitFlow 工作流](#gitflow-工作流)** - 了解我们的分支管理策略
2. **[Issue 管理规范](.gitlab/ISSUE_TEMPLATE_GUIDE.md)** - 学习如何创建和管理 Issue
3. **[标签体系](.gitlab/LABELS_CONFIG.md)** - 理解项目的标签分类

#### 第二步：配置开发环境 🛠️

**系统要求：**
- Windows 10/11 (x64)
- Visual Studio 2022 或更高版本
- .NET Framework 4.7.2 SDK
- Git 2.30+

**推荐配置：**
```bash
# 1. 安装 Visual Studio 2022 并确保包含以下工作负载：
#    - .NET 桌面开发
#    - WPF 开发工具

# 2. 安装 Git（推荐使用 Git for Windows）
winget install Git.Git

# 3. 配置 Git 用户信息
git config --global user.name "你的姓名"
git config --global user.email "你的邮箱@example.com"
```

#### 第三步：克隆仓库 📦

```bash
# 克隆仓库到本地
git clone <仓库地址> lm2026
cd lm2026

# 查看当前分支（应该在 develop 分支）
git branch

# 如果不在 develop，切换到 develop 分支
git checkout develop
```

#### 第四步：构建项目 🔨

```bash
# 使用 Visual Studio 打开解决方案
# 方式1: 双击打开
LM2026.slnx

# 方式2: 从命令行打开
start LM2026.slnx

# 在 Visual Studio 中：
# 1. 右键点击解决方案 -> "还原 NuGet 包"
# 2. 菜单栏 -> 生成 -> 重新生成解决方案
# 3. 设置 LusterMotion 为启动项目
# 4. 按 F5 运行项目
```

#### 第五步：开始你的第一个任务 🎯

1. **在 GitLab 上查看分配给你的 Issue**
2. **根据 Issue 类型创建相应的分支：**
   ```bash
   # 新功能
   git checkout -b feature/功能描述 develop
   
   # Bug 修复
   git checkout -b bugfix/问题描述 develop
   
   # 示例
   git checkout -b feature/add-user-login develop
   ```

3. **开始编码，并定期提交：**
   ```bash
   git add .
   git commit -m "feat: 添加用户登录功能 #123"
   # #123 是对应的 Issue 编号
   ```

4. **推送分支并创建 Merge Request：**
   ```bash
   git push origin feature/add-user-login
   # 然后在 GitLab 上创建 MR
   ```

### ⚡ 快速参考卡片

| 我想做... | 应该... |
|---------|--------|
| 开发新功能 | 从 `develop` 创建 `feature/xxx` 分支 |
| 修复 Bug | 从 `develop` 创建 `bugfix/xxx` 分支 |
| 紧急修复生产 Bug | 从 `main` 创建 `hotfix/vX.X.X-xxx` 分支 |
| 准备发布版本 | 从 `develop` 创建 `release/vX.X.X` 分支 |
| 提交代码 | 编写符合规范的 commit message，引用 Issue |
| 合并代码 | 创建 Merge Request，等待 Code Review |

---

## 📐 开发规范

本项目采用 **GitFlow + Issue 工作流 + CI 自动化** 的标准化开发流程。所有代码变更必须遵循此规范。

### GitFlow 工作流

我们使用经典的 GitFlow 分支模型进行版本管理：

```
main (生产环境，受保护)
  ↓
  ├─── hotfix/vX.X.X (紧急修复) → main + develop
  │
  └─── release/vX.X.X (发布分支) → main + develop
         ↑
       develop (开发主分支，受保护)
         ↑
         ├─── feature/feature-name (新功能)
         ├─── feature/another-feature (新功能)
         └─── bugfix/fix-something (Bug修复)
```

#### 分支说明

| 分支类型 | 命名规范 | 基于分支 | 合并目标 | 说明 |
|---------|---------|---------|---------|------|
| `main` | `main` | - | - | 生产环境分支，每个提交对应一个发布版本 |
| `develop` | `develop` | `main` | - | 开发主分支，包含下一版本的所有功能 |
| `feature/*` | `feature/功能描述` | `develop` | `develop` | 新功能开发分支 |
| `bugfix/*` | `bugfix/问题描述` | `develop` | `develop` | Bug 修复分支（非紧急）|
| `hotfix/*` | `hotfix/vX.X.X-描述` | `main` | `main` + `develop` | 生产环境紧急修复 |
| `release/*` | `release/vX.X.X` | `develop` | `main` + `develop` | 版本发布准备分支 |

#### 分支命名规范

```bash
# ✅ 正确示例
feature/user-authentication
feature/dashboard-widgets
bugfix/login-error
bugfix/memory-leak-in-processor
hotfix/v1.2.1-critical-crash
hotfix/v2.0.1-security-fix
release/v1.3.0
release/v2.0.0

# ❌ 错误示例
feature/Feature1           # 不够描述性
fix-bug                    # 缺少类型前缀
feature/添加用户登录        # 应使用英文
Hotfix/v1.0.1             # 首字母不应大写
```

### Issue 管理

本项目所有开发工作都必须基于 Issue 进行。我们提供了 **5 种 Issue 模板**：

#### 1. 🌟 Feature (新功能)

**何时使用：** 开发新功能或增强现有功能

**工作流程：**
```bash
# 1. 在 GitLab 创建 Feature Issue（使用模板）
# 2. 创建功能分支
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name

# 3. 开发并提交（Commit message 中引用 Issue）
git commit -m "feat: 实现用户认证功能 #123"

# 4. 推送并创建 MR
git push origin feature/your-feature-name
# 在 GitLab 创建 Merge Request 到 develop
```

**Issue 模板：** [.gitlab/issue_templates/feature.md](.gitlab/issue_templates/feature.md)

#### 2. 🐛 Bug (缺陷修复)

**何时使用：** 修复非紧急的 Bug

**工作流程：**
```bash
# 类似 Feature，但分支名使用 bugfix/
git checkout -b bugfix/fix-login-error develop
git commit -m "fix: 修复登录页面提交错误 #124"
```

**Issue 模板：** [.gitlab/issue_templates/bug.md](.gitlab/issue_templates/bug.md)

#### 3. 🚨 Hotfix (紧急修复)

**何时使用：** 修复生产环境的紧急 Bug

**工作流程：**
```bash
# ⚠️ 注意：Hotfix 从 main 分支创建！
git checkout main
git pull origin main
git checkout -b hotfix/v1.2.1-critical-crash

# 修复后合并到 main 和 develop
# 1. 合并到 main 并打 Tag
git checkout main
git merge --no-ff hotfix/v1.2.1-critical-crash
git tag -a v1.2.1 -m "Hotfix: 修复关键崩溃问题"
git push origin main --tags

# 2. 合并回 develop
git checkout develop
git merge --no-ff hotfix/v1.2.1-critical-crash
git push origin develop

# 3. 删除 hotfix 分支
git branch -d hotfix/v1.2.1-critical-crash
```

**Issue 模板：** [.gitlab/issue_templates/hotfix.md](.gitlab/issue_templates/hotfix.md)

#### 4. 📦 Release (版本发布)

**何时使用：** 准备新版本发布

**Issue 模板：** [.gitlab/issue_templates/release.md](.gitlab/issue_templates/release.md)

#### 5. 💬 Discussion (技术讨论)

**何时使用：** 技术方案讨论、架构设计评审

**Issue 模板：** [.gitlab/issue_templates/discussion.md](.gitlab/issue_templates/discussion.md)

### Commit Message 规范

我们遵循 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

**Type 类型：**
- `feat`: 新功能
- `fix`: Bug 修复
- `docs`: 文档更新
- `style`: 代码格式调整（不影响功能）
- `refactor`: 代码重构
- `perf`: 性能优化
- `test`: 测试相关
- `chore`: 构建/工具链更新

**示例：**
```bash
# ✅ 好的 Commit Message
feat(motion): 添加多轴联动控制功能 #123
fix(ui): 修复设备列表刷新异常 #124
docs: 更新 API 文档
refactor(core): 重构模块加载逻辑 #125

# ❌ 不好的 Commit Message
update code
fix bug
修改了一些文件
```

**⚠️ 重要：** Commit message 中必须包含相关的 Issue 编号（如 `#123`）

### Code Review 规范

所有代码合并到 `develop` 或 `main` 分支前必须经过 Code Review：

1. **创建 Merge Request (MR)**
   - 标题清晰描述变更内容
   - 关联相关 Issue
   - 填写 MR 描述模板

2. **指定 Reviewer**
   - 至少需要 1 位团队成员审核
   - 核心模块需要 2 位审核

3. **解决 Review 意见**
   - 及时回复和修改
   - 解决所有讨论后才能合并

4. **合并策略**
   - 使用 `--no-ff` 保留分支历史
   - 合并后删除源分支

---


## 🤖 CI/CD 自动化

本项目已配置 GitLab CI/CD 自动化流程，无需手动干预即可自动执行以下任务：

### 自动化流程

```
代码推送 → 自动检测 → 钉钉通知 → (未来) 自动构建 → 自动测试
```

#### 1. 📢 代码推送通知

**触发条件：** 任何分支的代码推送

**功能：**
- 自动发送钉钉通知到团队群
- 包含推送者、分支、提交信息、修改文件列表
- 标记是否涉及核心结构文件（.sln, .csproj, .config 等）
- 提供 GitLab 链接快速查看详情

**配置文件：** `scripts/push-notify.yml`

#### 2. 📬 Merge Request 通知

**触发条件：** 创建或更新 Merge Request

**功能：**
- MR 创建时发送通知
- MR 更新时发送通知
- 显示源分支、目标分支、变更文件数量
- @相关人员进行 Code Review

**配置文件：** `scripts/mr-notify.yml`

#### 3. 🔄 MR 自动同步

**触发条件：** Merge Request 创建或更新

**功能：**
- 自动同步 MR 状态到钉钉
- 显示 MR 进度和审批状态

**配置文件：** `scripts/sync-mr.yml`

#### 4. 🏗️ 自动构建（待启用）

**未来规划：**
- 自动编译 .NET 项目
- 自动生成 Release 包
- 自动运行单元测试

**配置文件：** `scripts/build-dotnet.yml`（已准备，未启用）

### 配置说明

#### 必需的 GitLab CI/CD 变量

在项目的 **Settings → CI/CD → Variables** 中配置：

| 变量名 | 说明 | 是否加密 |
|-------|------|---------|
| `DINGTALK_TOKEN` | 钉钉机器人 Webhook Token | ✅ |
| `GITLAB_API_TOKEN` | GitLab API 访问令牌 | ✅ |

#### GitLab Runner 要求

- **Tag：** `windows-shell`
- **Executor：** PowerShell
- **操作系统：** Windows 10/11

### 查看 CI/CD 运行状态

1. 访问项目的 **CI/CD → Pipelines**
2. 查看最近的 Pipeline 运行记录
3. 点击具体 Job 查看详细日志

---
