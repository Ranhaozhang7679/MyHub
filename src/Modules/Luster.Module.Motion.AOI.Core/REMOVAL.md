# AOI 业务模块包：移除/还原清单

目标：移除 AOI 业务模块后，LusterMotion 仍可启动并运行**标准运控流程**，平台主干无残留 AOI 专属逻辑。该清单覆盖本次 P0-J 子任务交付的 `Luster.Module.Motion.AOI.Core`；后续模块（FiveAxis、Devices、UI 等）交付后，需在本清单追加移除步骤。

## 移除步骤

1. **删除运控模块 DLL**
   - 删除 `Motions/Luster.Module.Motion.AOI.Core.dll`。

2. **删除站点 profile 与配置**
   - 删除 `SiteProfiles/AOI1/`、`SiteProfiles/AOI2/`、`SiteProfiles/Wipe/`。
   - 删除 `Config/Recipes/{AOI1,AOI2,Wipe}/`、`Config/Traces/{AOI1,AOI2,Wipe}/`、`Config/Card/{AOI1,AOI2,Wipe}/`、`Config/Handshake/{AOI1,AOI2,Wipe}/`。
   - 删除 `Logs/{AOI1,AOI2,Wipe}/`。

3. **保留的平台资源**
   - 不删除 `Motions/Luster.Module.Motion.Stations.dll`、`Luster.TaskFlow.*`、`Luster.Motion.*` 等平台主干模块。
   - 不删除 `Config/SolutionConfig.xml` 中的非 AOI 站点。

## 移除前验证

- 当前是否处于运行中的自动生产流程？若是，先正常停机退出。
- `SiteProfiles/<站>/site-profile.xml` 是否仅被 AOI 模块引用？平台主干**不应**直接引用该 XML（这是 ADR 的不侵入约束）。

## 移除后验证

| 检查项 | 期望 |
|---|---|
| 启动 LusterMotion | 正常进入主界面，不报 AOI 模块缺失致命错误 |
| `ModuleFactory.LoadModules("Motions")` | 不再扫描出 `AoiCoreModule`，但既有 `Stations`、`Algorithm` 等模块继续被发现 |
| Shell 主区域 / 编辑器 / TaskFlow | 标准运控流程节点（FreeStation、HomeStation、TestStation、NGStation、StartStation 等）可正常加载与编辑 |
| Devices / DeviceEngine | 标准设备驱动继续可用；缺失 AOI 设备模块时报警等级降级为提示，不阻塞启动 |
| `Config/SolutionConfig.xml` | 若该文件之前指向某 AOI 站，建议清空 `AoiSiteType` 项；保留其他工程项可继续编辑 |
| 不侵入断言 | `IMotionCard` 未被扩展过 RTCP 方法；`Luster.Module.Motion.Device` 等主干工程内无 AOI 命名空间引用 |

## 平台无 AOI 残留断言

按 ADR 的不侵入约束，以下文件**不允许**因 AOI 迁移被修改过：

- `src/Modules/Luster.TaskFlow.Common/**`
- `src/Modules/Luster.TaskFlow.Motion/**`
- `src/Modules/Luster.Module.Motion.Device/**`（除非新增 AOI 专属接口被有意安置于此，但当前 ADR 明确不允许）
- `src/Shell/LusterMotion/**` 主干（仅允许在 `SolutionConfig` 这类配置数据层引用站点 profile）

如有任何上述路径下出现 AOI 专属代码或硬编码，本次迁移视为不通过可还原性验收。

## 回滚

若移除后发现需要回切到 AOI 模式：
1. 重新部署 `Luster.Module.Motion.AOI.Core.dll` 与对应 `SiteProfiles/<站>/`、`Config/Handshake/<站>/`。
2. 更新 `Config/SolutionConfig.xml` 指向目标站点。
3. 重启 LusterMotion，自动流程启动前由 `AoiCoreModule.ValidateProfile` 校验配置完整性。
