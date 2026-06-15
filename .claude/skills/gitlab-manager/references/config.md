# GitLab 配置

> **⚠️ 此文件包含敏感信息，请勿提交到版本控制。**

在此填写你的 GitLab 仓库配置信息：

```
GITLAB_URL=http://10.9.1.153:8687
PROJECT_ID=33
PRIVATE_TOKEN=Cn5SUfKfFBnnTYx6yA9i
```

## 获取方式

| 配置项 | 获取方法 |
|--------|----------|
| `GITLAB_URL` | 你的 GitLab 实例首页地址，不带尾部斜杠 |
| `PROJECT_ID` | 项目 Settings → General 中的数字 ID |
| `PRIVATE_TOKEN` | 用户 Settings → Access Tokens → 创建 token，勾选 `api` 权限 |

## 令牌权限要求

| 操作 | 最低权限 |
|------|----------|
| 查看 Issue/MR | `read_api` |
| 创建/修改 Issue | `api` |
| 创建 MR | `api` |
| 添加评论 | `api` |
