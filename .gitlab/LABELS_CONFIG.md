# GitLab 标签配置指南

## 🏷️ 标签体系设计

本项目采用**分类标签**和**编号标签**相结合的管理方式。

---

## 📋 标签分类

### 1️⃣ 类型标签 (Type Labels)

| 标签名 | 颜色代码 | 说明 |
|--------|----------|------|
| `~feature` | #428BCA (蓝色) | 新功能开发 |
| `~bug` | #D9534F (红色) | Bug缺陷 |
| `~hotfix` | #FF0000 (深红色) | 紧急修复 |
| `~release` | #8E44AD (紫色) | 版本发布 |
| `~discussion` | #F0AD4E (橙色) | 技术讨论 |
| `~enhancement` | #5CB85C (绿色) | 功能增强 |
| `~refactor` | #5BC0DE (青色) | 代码重构 |
| `~documentation` | #95A5A6 (灰色) | 文档 |
| `~test` | #34495E (深灰) | 测试相关 |

---

### 2️⃣ 优先级标签 (Priority Labels)

| 标签名 | 颜色代码 | 说明 | SLA |
|--------|----------|------|-----|
| `~P0` | #C0392B (深红) | 紧急 - Critical | 24小时内响应 |
| `~P1` | #E67E22 (橙色) | 高 - High | 3天内开始 |
| `~P2` | #F39C12 (黄色) | 中 - Medium | 1周内开始 |
| `~P3` | #95A5A6 (灰色) | 低 - Low | 有空时处理 |

---

### 3️⃣ 状态标签 (Status Labels)

| 标签名 | 颜色代码 | 说明 |
|--------|----------|------|
| `~todo` | #ECEFF1 (浅灰) | 待处理 |
| `~doing` | #2196F3 (蓝色) | 进行中 |
| `~review` | #FF9800 (橙色) | 待评审 |
| `~testing` | #9C27B0 (紫色) | 测试中 |
| `~blocked` | #F44336 (红色) | 被阻塞 |
| `~done` | #4CAF50 (绿色) | 已完成 |

---

### 4️⃣ 编号标签 (Numbered Labels)

#### 功能号 (Feature Number)
用于追踪特定功能的开发进度

**命名规范：** `~F-XXX` 或 `~功能-XXX`

**示例：**
- `~F-001` - 用户登录功能
- `~F-002` - 数据导出功能
- `~F-015` - 设备控制面板
- `~F-023` - 运动轨迹规划

**颜色：** #1E88E5 (亮蓝色)

#### 版本号 (Version Number)
用于标识Issue属于哪个版本

**命名规范：** `~v-X.X.X` 或 `~版本-X.X.X`

**示例：**
- `~v-1.0.0` - 首个正式版本
- `~v-1.1.0` - 功能更新版本
- `~v-1.0.1` - 补丁版本
- `~v-2.0.0` - 重大版本更新

**颜色：** #7E57C2 (紫色)

> **注意：** 版本标签与Milestone配合使用，Milestone用于管理发布计划，标签用于快速筛选

#### 错误修复号 (Bugfix Number)
用于追踪特定Bug的修复

**命名规范：** `~BF-XXX` 或 `~修复-XXX`

**示例：**
- `~BF-001` - 登录失败Bug
- `~BF-007` - 内存泄漏问题
- `~BF-015` - UI渲染错误
- `~BF-032` - 数据同步异常

**颜色：** #E53935 (红色)

---

### 5️⃣ 模块/组件标签 (Module Labels)

根据项目实际架构创建，例如：

| 标签名 | 颜色代码 | 说明 |
|--------|----------|------|
| `~模块:UI` | #3F51B5 | 用户界面相关 |
| `~模块:Motion` | #009688 | 运动控制模块 |
| `~模块:Device` | #FF5722 | 设备管理模块 |
| `~模块:Database` | #795548 | 数据库相关 |
| `~模块:API` | #607D8B | API接口 |
| `~模块:Core` | #212121 | 核心框架 |

---

## 🎯 标签使用规范

### 基本原则
1. **每个Issue必须至少包含一个类型标签**（feature/bug/hotfix等）
2. **重要Issue应设置优先级标签**（P0-P3）
3. **使用编号标签进行追踪**（F-XXX/BF-XXX/v-X.X.X）
4. **及时更新状态标签**（todo → doing → review → testing → done）

### 标签组合示例

#### 示例1：新功能开发
```
~feature ~F-025 ~v-1.2.0 ~P1 ~模块:Motion ~doing
```
表示：功能编号025，计划在v1.2.0发布，高优先级，属于Motion模块，正在开发中

#### 示例2：Bug修复
```
~bug ~BF-012 ~v-1.1.1 ~P0 ~模块:UI ~testing
```
表示：Bug修复编号012，计划在v1.1.1修复，紧急，属于UI模块，测试中

#### 示例3：紧急热修复
```
~hotfix ~BF-033 ~v-1.0.3 ~P0 ~critical ~模块:Core ~review
```
表示：紧急修复编号033，发布在v1.0.3，最高优先级，核心模块，待评审

#### 示例4：版本发布
```
~release ~v-2.0.0 ~P0
```
表示：2.0.0版本发布Issue，最高优先级

---

## 📊 编号管理规范

### 功能号 (F-XXX) 分配规则
- **001-099：** 核心基础功能
- **100-199：** 用户界面功能
- **200-299：** 设备管理功能
- **300-399：** 运动控制功能
- **400-499：** 数据处理功能
- **500-599：** 系统配置功能
- **600-699：** 报表和导出功能
- **700-999：** 其他扩展功能

### 错误修复号 (BF-XXX) 分配规则
- **001-099：** Critical级别Bug
- **100-199：** High级别Bug
- **200-299：** Medium级别Bug
- **300-999：** Low级别Bug或优化

### 版本号 (v-X.X.X) 规范
遵循语义化版本：
- **主版本号 (X.0.0)：** 重大架构变更或不兼容更新
- **次版本号 (X.X.0)：** 新功能添加，向后兼容
- **补丁版本号 (X.X.X)：** Bug修复和小改进

---

## 🔧 GitLab 标签创建脚本

可以使用GitLab API批量创建标签。以下是示例脚本（需要配置Access Token）：

### PowerShell 脚本示例

```powershell
# 配置
$GitLabUrl = "https://gitlab.com"  # 你的GitLab地址
$ProjectId = "YOUR_PROJECT_ID"
$AccessToken = "YOUR_ACCESS_TOKEN"

$headers = @{
    "PRIVATE-TOKEN" = $AccessToken
}

# 创建类型标签
$typeLabels = @(
    @{name="feature"; color="#428BCA"; description="新功能开发"},
    @{name="bug"; color="#D9534F"; description="Bug缺陷"},
    @{name="hotfix"; color="#FF0000"; description="紧急修复"},
    @{name="release"; color="#8E44AD"; description="版本发布"}
)

foreach ($label in $typeLabels) {
    $body = @{
        name = $label.name
        color = $label.color
        description = $label.description
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "$GitLabUrl/api/v4/projects/$ProjectId/labels" `
        -Method POST -Headers $headers -Body $body -ContentType "application/json"
}

Write-Host "标签创建完成！"
```

---

## 📝 Issue创建时的标签建议

在各个Issue模板底部，已经预设了合适的标签命令：

### Feature Issue
```markdown
/label ~feature ~F-XXX ~v-X.X.X ~P1
```

### Bug Issue
```markdown
/label ~bug ~BF-XXX ~P2
```

### Hotfix Issue
```markdown
/label ~hotfix ~BF-XXX ~v-X.X.X ~P0 ~critical
```

### Release Issue
```markdown
/label ~release ~v-X.X.X ~P0
```

---

## 🔍 常用标签筛选查询

### 查找所有未完成的高优先级功能
```
label:~feature label:~P1 -label:~done
```

### 查找特定版本的所有Issue
```
label:~v-1.2.0
```

### 查找被阻塞的Issue
```
label:~blocked
```

### 查找某个功能相关的所有Issue
```
label:~F-025
```

### 查找待修复的Bug
```
label:~bug -label:~done
```

---

**最后更新：** 2026-02-12  
**维护者：** 项目管理团队
