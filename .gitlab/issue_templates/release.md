## 📦 版本发布 (Release Information)

### 版本号
<!-- 例如: v1.2.0 -->
**版本编号：** v-X.X.X  
**里程碑：** %vX.X.X  
**计划发布日期：** YYYY-MM-DD

### 发布类型
- [ ] 🚀 Major Release (主版本 - 重大功能变更/不兼容更新)
- [ ] ✨ Minor Release (次版本 - 新功能添加/向后兼容)
- [ ] 🔧 Patch Release (补丁版本 - Bug修复/性能优化)

### 发布日期
**计划发布日期:** 

## 📝 版本内容 (Release Content)

### ✨ 新功能 (New Features)
<!-- 列出本版本包含的新功能 -->
- 

### 🐛 Bug修复 (Bug Fixes)
<!-- 列出本版本修复的Bug -->
- 

### 🔧 优化改进 (Improvements)
<!-- 列出性能优化、代码重构等改进 -->
- 

### ⚠️ 破坏性变更 (Breaking Changes)
<!-- 如果有不兼容的变更，务必在此列出 -->
- 

### 🗑️ 废弃功能 (Deprecated)
<!-- 列出计划废弃的功能 -->
- 

## 🔗 相关Issue (Related Issues)

<!-- 列出本次发布包含的所有Issue -->
### Features
- Closes #
- Closes #

### Bugs
- Fixes #
- Fixes #

## ✅ 发布检查清单 (Release Checklist)

### 代码准备
- [ ] 所有计划功能已合并到 `develop` 分支
- [ ] 创建 `release/vX.X.X` 分支
- [ ] 更新版本号（AssemblyInfo、package.json等）
- [ ] 更新 CHANGELOG.md
- [ ] 代码审查完成

### 测试验证
- [ ] 单元测试通过
- [ ] 集成测试通过
- [ ] 回归测试通过
- [ ] 性能测试通过（如需要）
- [ ] 安全扫描通过（如需要）

### 文档更新
- [ ] 更新用户文档
- [ ] 更新API文档
- [ ] 更新迁移指南（如有破坏性变更）
- [ ] 更新README.md

### 发布流程
- [ ] 合并到 `main` 分支
- [ ] 创建 Git Tag (vX.X.X)
- [ ] 构建发布包
- [ ] 上传到发布平台/服务器
- [ ] 发布 Release Notes
- [ ] 合并回 `develop` 分支

### 发布后
- [ ] 通知团队和用户
- [ ] 监控系统运行状态
- [ ] 处理用户反馈
- [ ] 删除 release 分支

## 📊 发布统计 (Release Statistics)

- 新增功能数: 
- 修复Bug数: 
- 代码变更量: 
- 贡献者: 

## 🔗 发布资源 (Release Resources)

- Release Notes: 
- 下载链接: 
- 文档链接: 
- 演示视频: 

## 📢 发布说明模板 (Release Notes Template)

```markdown
# Release vX.X.X

发布日期: YYYY-MM-DD

## 新功能
- 

## Bug修复
- 

## 改进优化
- 

## 升级说明
- 

感谢所有贡献者！
```

---

<!-- 请根据实际版本修改 -->
/label ~release ~v-X.X.X ~P0
/milestone %vX.X.X
/assign @
