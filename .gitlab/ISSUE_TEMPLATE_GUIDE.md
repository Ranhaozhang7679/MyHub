# GitLab Issue 模板使用指南

## 📚 概述

本项目采用标准的 **GitFlow 工作流**进行版本管理和Issue追踪。我们提供了5种Issue模板，对应不同的工作场景。

## 🎯 Issue 模板类型

### 1. 🌟 Feature (新功能)
**使用场景：** 开发新功能或增强现有功能

**工作流程：**
```
develop → feature/功能名 → develop → release/vX.X.X → main
```

**分支命名规范：**
- `feature/user-authentication`
- `feature/dashboard-widgets`
- `feature/export-to-excel`

**标签建议：** `~feature`, `~enhancement`

---

### 2. 🐛 Bug (缺陷修复)
**使用场景：** 修复非紧急的Bug

**工作流程：**
```
develop → bugfix/问题描述 → develop → release/vX.X.X → main
```

**分支命名规范：**
- `bugfix/login-error`
- `bugfix/memory-leak`
- `bugfix/ui-rendering-issue`

**标签建议：** `~bug`, `~bugfix`, 优先级标签（`~P0`-`~P3`）

---

### 3. 🚨 Hotfix (紧急修复)
**使用场景：** 修复生产环境的紧急Bug

**工作流程：**
```
main → hotfix/vX.X.X → main + develop
```

**分支命名规范：**
- `hotfix/v1.2.1-critical-crash`
- `hotfix/v2.0.1-security-fix`

**标签建议：** `~hotfix`, `~bug`, `~critical`

**⚠️ 注意：** Hotfix 完成后必须同时合并到 `main` 和 `develop` 分支！

---

### 4. 📦 Release (版本发布)
**使用场景：** 准备新版本发布

**工作流程：**
```
develop → release/vX.X.X → main + develop
```

**分支命名规范：**
- `release/v1.2.0`
- `release/v2.0.0`

**标签建议：** `~release`, Milestone（`%v1.2.0`）

**发布流程：**
1. 从 `develop` 创建 `release/vX.X.X` 分支
2. 在 release 分支上进行版本号更新、文档完善、bug修复
3. 测试通过后合并到 `main` 并打 Tag
4. 同时合并回 `develop`
5. 删除 release 分支

---

### 5. 💬 Discussion (技术讨论)
**使用场景：** 技术方案讨论、架构设计评审、最佳实践分享

**标签建议：** `~discussion`, `~proposal`, `~rfc`

**适用情况：**
- 技术选型需要团队评审
- 架构重构方案讨论
- 复杂问题需要集思广益
- 编码规范和最佳实践制定

---

## 🔄 GitFlow 工作流图示

```
main (生产环境)
  ↓
  ├─── hotfix/v1.0.1 (紧急修复) → main + develop
  │
  └─── release/v1.1.0 (发布分支) → main + develop
         ↑
       develop (开发主分支)
         ↑
         ├─── feature/new-feature (新功能)
         ├─── feature/another-feature (新功能)
         └─── bugfix/fix-bug (Bug修复)
```

## 🏷️ 标签体系建议

### 类型标签
- `~feature` - 新功能
- `~bug` - Bug
- `~hotfix` - 紧急修复
- `~release` - 版本发布
- `~discussion` - 讨论
- `~enhancement` - 功能增强
- `~refactor` - 代码重构
- `~documentation` - 文档

### 优先级标签
- `~P0` - 紧急（必须立即处理）
- `~P1` - 高优先级（本周内完成）
- `~P2` - 中优先级（本月内完成）
- `~P3` - 低优先级（有空时处理）

### 状态标签
- `~todo` - 待处理
- `~doing` - 进行中
- `~review` - 待评审
- `~testing` - 测试中
- `~blocked` - 被阻塞
- `~done` - 已完成

### 模块标签
根据项目实际情况创建，例如：
- `~ui` - 用户界面
- `~api` - 后端API
- `~database` - 数据库
- `~performance` - 性能
- `~security` - 安全

## 📅 Milestone 管理

**命名规范：**
- `%v1.0.0` - 主版本号
- `%v1.1.0` - 次版本号
- `%v1.0.1` - 补丁版本号

**使用建议：**
- 每个 Feature 和 Bug 应关联到具体的 Milestone
- Milestone 包含该版本计划完成的所有 Issue
- Release Issue 必须与对应的 Milestone 关联

## 🔧 快速命令参考

创建 Issue 时，可以在模板底部使用这些快速命令：

```markdown
/label ~feature ~P1           # 添加标签
/milestone %v1.2.0            # 关联Milestone
/assign @username             # 分配给某人
/estimate 2d                  # 预估工作量
/spend 1h 30m                 # 记录已花费时间
/close                        # 关闭Issue
```

## 📝 Issue 编写最佳实践

### 1. 标题要清晰
❌ 不好：`修复Bug`  
✅ 好：`修复登录页面在Chrome下无法提交表单的Bug`

### 2. 描述要详细
- 提供足够的上下文信息
- 使用清单（Checklist）追踪进度
- 添加截图或代码示例
- 引用相关的Issue或文档

### 3. 合理使用标签和Milestone
- 每个Issue至少有一个类型标签
- 重要Issue应设置优先级
- 关联到具体的Milestone

### 4. 及时更新状态
- 开始工作时更新状态为 `~doing`
- 遇到阻塞及时标记 `~blocked` 并说明原因
- 完成后关闭Issue并做简要总结

## 🎯 Issue 生命周期

```
创建 → 分配 → 开发 → 代码审查 → 测试 → 关闭
 ↓      ↓      ↓       ↓         ↓      ↓
todo  doing  review  testing   done  closed
```

## 🤝 协作规范

1. **创建Issue时：** 填写完整的模板信息，便于他人理解
2. **分配Issue时：** 确保被分配人知晓并接受任务
3. **开发过程中：** 在相关的Commit和MR中引用Issue编号（如 `#123`）
4. **代码审查：** 在MR中关联对应的Issue
5. **完成后：** 写清楚完成情况和遗留问题（如有）

## 📞 需要帮助？

如果对Issue模板使用有任何疑问，请：
1. 查看本指南
2. 在团队内部讨论
3. 创建一个 Discussion Issue 征求意见

---

**最后更新：** 2026-02-12  
**维护者：** 项目管理团队
