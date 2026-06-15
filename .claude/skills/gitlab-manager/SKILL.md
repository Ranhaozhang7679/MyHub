---
name: gitlab-manager
description: GitLab 仓库 Issue 和 Merge Request 管理。当用户需要创建 Issue、修改 Issue、创建 Merge Request、查看 Issue 列表、添加评论到 Issue 或 MR 时使用。也适用于 GitLab、merge request、MR、工单、议题、合并请求等关键词。
---

# GitLab 仓库管理

通过 GitLab REST API v4 管理指定仓库的 Issue 和 Merge Request。

## 配置

使用前必须填写以下配置项。修改 `references/config.md` 中的值：

| 配置项 | 说明 | 示例 |
|--------|------|------|
| `GITLAB_URL` | GitLab 实例地址 | `https://gitlab.example.com` |
| `PROJECT_ID` | 项目数字 ID | `12345` |
| `PRIVATE_TOKEN` | 访问令牌（需要 api 权限） | `glpat-xxxxxxxxxxxx` |

> **安全提醒**：访问令牌属于敏感凭证。`config.md` 已在 `.gitignore` 中排除。切勿将令牌提交到代码仓库。

## 任务路由

根据用户意图选择对应分支：

| 用户意图 | 跳转到 |
|----------|--------|
| 创建 Issue | [创建 Issue](#创建-issue) |
| 修改/更新 Issue | [修改 Issue](#修改-issue) |
| 查看 Issue 列表 | [查看 Issues](#查看-issues) |
| 创建 Merge Request | [创建 MR](#创建-merge-request) |
| 获取 MR 列表 | [查看 MRs](#查看-mrs) |
| 添加评论 | [添加评论](#添加评论) |

开始前，先读取 `references/config.md` 获取配置值。若配置缺失或令牌无效，立即停止并提示用户补充。

## 通用约定

- 所有 API 调用使用 `curl` 通过 Bash 工具执行
- 请求头：`PRIVATE-TOKEN: <token>`，`Content-Type: application/json; charset=utf-8`
- API 基础路径：`${GITLAB_URL}/api/v4/projects/${PROJECT_ID}`
- 响应为 JSON 格式，提取关键字段展示给用户
- 错误处理：HTTP 非 2xx 时，解析 `message` 字段并报告给用户
- **中文 JSON 处理**：Windows 下 curl 直接传中文会 400，必须先写入临时文件再用 `--data @/tmp/gitlab_req.json` 发送。对所有 POST/PUT 请求均使用此方式

## 创建 Issue

**收集输入**（缺少则询问用户）：
- `title`（必填）：Issue 标题
- `description`（可选）：详细描述，支持 Markdown
- `labels`（可选）：逗号分隔的标签名
- `assignee_ids`（可选）：指派人 ID 列表
- `milestone_id`（可选）：里程碑 ID

**执行**：

```bash
printf '{"title":"%s","description":"%s","labels":"%s"}' "<title>" "<desc>" "<labels>" > /tmp/gitlab_req.json
curl --silent -w "\n%{http_code}" \
     --header "PRIVATE-TOKEN: ${TOKEN}" \
     --header "Content-Type: application/json; charset=utf-8" \
     --request POST \
     --data @/tmp/gitlab_req.json \
     "${GITLAB_URL}/api/v4/projects/${PROJECT_ID}/issues"
```

**验证**：确认响应中 `iid`（Issue 内部编号）和 `web_url` 存在。

**输出**：向用户报告创建结果，包含 Issue 编号、标题、链接。

## 修改 Issue

**收集输入**：
- `iid`（必填）：Issue 内部编号
- 需要修改的字段：`title`、`description`、`labels`、`state_event`（`close`/`reopen`）、`assignee_ids`、`milestone_id` 等

**执行**：

```bash
printf '{"title":"%s","state_event":"%s"}' "<new_title>" "close" > /tmp/gitlab_req.json
curl --silent -w "\n%{http_code}" \
     --header "PRIVATE-TOKEN: ${TOKEN}" \
     --header "Content-Type: application/json; charset=utf-8" \
     --request PUT \
     --data @/tmp/gitlab_req.json \
     "${GITLAB_URL}/api/v4/projects/${PROJECT_ID}/issues/${IID}"
```

**验证**：确认响应中修改后的字段值已更新。

**输出**：向用户报告修改结果，包含变更前后对比。

## 查看 Issues

**收集输入**：
- `state`（可选）：`opened`（默认）、`closed`、`all`
- `labels`（可选）：按标签过滤
- `search`（可选）：搜索关键词

**执行**：

```bash
curl --header "PRIVATE-TOKEN: ${TOKEN}" \
     "${GITLAB_URL}/api/v4/projects/${PROJECT_ID}/issues?state=opened&per_page=20"
```

**输出**：以表格形式展示 Issue 列表：`IID | 标题 | 状态 | 标签 | 指派人 | 创建时间`。

## 创建 Merge Request

**收集输入**（缺少则询问用户）：
- `source_branch`（必填）：源分支
- `target_branch`（必填）：目标分支
- `title`（必填）：MR 标题
- `description`（可选）：详细描述，支持 Markdown
- `assignee_ids`（可选）：审核人 ID 列表
- `labels`（可选）：标签
- `remove_source_branch`（可选）：合并后删除源分支，默认 `true`

**执行**：

```bash
printf '{"source_branch":"%s","target_branch":"%s","title":"%s","remove_source_branch":true}' "<src>" "<target>" "<title>" > /tmp/gitlab_req.json
curl --silent -w "\n%{http_code}" \
     --header "PRIVATE-TOKEN: ${TOKEN}" \
     --header "Content-Type: application/json; charset=utf-8" \
     --request POST \
     --data @/tmp/gitlab_req.json \
     "${GITLAB_URL}/api/v4/projects/${PROJECT_ID}/merge_requests"
```

**验证**：确认响应中 `iid`（MR 内部编号）和 `web_url` 存在。

**输出**：向用户报告创建结果，包含 MR 编号、标题、源→目标分支、链接。

## 查看 MRs

**收集输入**：
- `state`（可选）：`opened`（默认）、`closed`、`merged`、`all`

**执行**：

```bash
curl --header "PRIVATE-TOKEN: ${TOKEN}" \
     "${GITLAB_URL}/api/v4/projects/${PROJECT_ID}/merge_requests?state=opened&per_page=20"
```

**输出**：以表格形式展示 MR 列表：`IID | 标题 | 源分支→目标分支 | 状态 | 作者 | 创建时间`。

## 添加评论

**收集输入**：
- `type`（必填）：`issue` 或 `merge_request`
- `iid`（必填）：Issue 或 MR 的内部编号
- `body`（必填）：评论内容，支持 Markdown

**执行**：

```bash
printf '{"body":"%s"}' "<comment>" > /tmp/gitlab_req.json
curl --silent -w "\n%{http_code}" \
     --header "PRIVATE-TOKEN: ${TOKEN}" \
     --header "Content-Type: application/json; charset=utf-8" \
     --request POST \
     --data @/tmp/gitlab_req.json \
     "${GITLAB_URL}/api/v4/projects/${PROJECT_ID}/${TYPE}s/${IID}/notes"
```

**输出**：确认评论已添加，展示评论摘要。

## 常见失败处理

| 错误 | 原因 | 处理 |
|------|------|------|
| `401 Unauthorized` | 令牌无效或过期 | 提示用户更新令牌 |
| `403 Forbidden` | 权限不足 | 提示用户检查令牌权限范围 |
| `404 Not Found` | 项目或 Issue 不存在 | 确认 PROJECT_ID 和 IID 是否正确 |
| `409 Conflict` | MR 已存在 | 提示已有相同源→目标的 MR |
| `400 Bad Request` | 参数错误 | 检查必填字段是否完整 |

## When to Use

当用户需要通过 GitLab API 管理仓库的 Issue 和 Merge Request 时使用本 skill。包括但不限于：创建/修改/关闭 Issue、创建 MR、查看列表、添加评论。

## Limitations

- 仅支持 REST API v4，不涵盖 GraphQL API
- 不支持文件操作（创建/删除/编辑文件请使用 Git 命令）
- 不支持 CI/CD 管线管理
- 分页默认取前 20 条，大量数据需用户明确要求翻页
- 不处理 Webhook、Project Access Token 等管理操作
