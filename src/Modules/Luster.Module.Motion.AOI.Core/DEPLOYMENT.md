# AOI 业务模块包：部署清单

> 范围：本清单只覆盖 P0-J site-profile 子任务所交付的 `Luster.Module.Motion.AOI.Core` 模块包；后续 P1-P3 五轴核心、P6 UI 等子任务交付后，需在本清单追加对应 DLL 与 profile 字段。

## 部署目标目录结构

LusterMotion 安装根（运行 `LusterMotion.exe` 所在目录）：

```
<InstallRoot>/
├── LusterMotion.exe
├── Motions/
│   └── Luster.Module.Motion.AOI.Core.dll          # 本次新增
├── Devices/                                       # 由设备适配子任务投递
├── Config/
│   ├── Recipes/{AOI1|AOI2|Wipe}/                  # 站点专属配方目录（运维准备）
│   ├── Traces/{AOI1|AOI2|Wipe}/                   # 追溯目录
│   ├── Card/{AOI1|AOI2|Wipe}/                     # 板卡配置目录
│   └── Handshake/{AOI1|AOI2|Wipe}/                # 通讯通道 CSV
├── SiteProfiles/                                  # 本次新增
│   ├── AOI1/site-profile.xml
│   ├── AOI2/site-profile.xml
│   └── Wipe/site-profile.xml
└── Logs/{AOI1|AOI2|Wipe}/
```

## 安装步骤

1. **DLL 投递**
   - `Luster.Module.Motion.AOI.Core.dll` → `Motions/`（构建后由 `CopyToMotionsFolder=true` 自动落点；手动部署时直接复制此 DLL 即可）。

2. **站点 profile 投递**
   - 构建后由 `CopySiteProfiles` 目标自动复制 `SiteProfiles/` 到 `<OutputPath>SiteProfiles/`。
   - 现场部署时，将 `SiteProfiles/<目标站>/site-profile.xml` 复制到安装根 `SiteProfiles/<目标站>/`。

3. **目录准备（现场运维）**
   - 按 `site-profile.xml` 中的 `RecipeRoot` / `TraceRoot` / `LogRoot` / `CardConfigPath` 准备目录。
   - 把 SP-2025140 现场抽出来的三套通讯 CSV 放入 `Config/Handshake/<站>/`。
   - ⚠ 现场配置差异（端口号、卡轴号、旋转中心、刀尖参考点）由人类工程师填入 profile，禁止用占位值上线。

4. **站点切换**
   - 运维通过修改启动参数或 `Config/SolutionConfig.xml` 指定当前 `AoiSiteType`。
   - 启动时 `AoiCoreModule.LoadSiteProfile` 自动读取对应 profile，`AoiCoreModule.ValidateProfile` 在自动流程开始前执行；校验失败抛 `AoiSiteProfileException`，应转为弹窗或安全模式提示。

## 启动拦截条件（任意一条不通过即拦截）

- `SiteType` 未指定或与 `AoiDeploymentManifest.SiteType` 不一致
- profile 版本与 manifest 版本不一致
- `AxisMap` 关键轴（X/Y/Z；AOI 站再加 U/V）为空
- 必备设备别名缺失（至少 `MotionCard`）
- AOI 站 `Rtcp` 配置为 null
- `Handshakes` 通道清单为空
- profile 引用的 `RequiredMotionModules` / `RequiredDeviceModules` 未在部署清单中

## 现场差异资料缺口（待人类补充）

以下字段在本次交付的样例 profile 中以保守默认值占位，**严禁上线使用**，必须由长盈 FQC 现场或客户工艺工程师确认：

| Profile 字段 | 默认占位 | 说明 |
|---|---|---|
| `AxisMap/Channel CardAxis` | 0..4 | 现场 ZMotion 卡实际轴号 |
| `Rtcp.CoordinateSystem` | 0 / 1 | 卡侧 RTCP 坐标系编号 |
| `Rtcp/RotationCenter` | (0,0,0) | 五轴旋转中心机械量测值 |
| `Rtcp/ToolCenterPoint` | (0,0,0) | 刀尖参考点机械量测值 |
| `Handshakes/Channel Config` | CSV 路径 | 现场通讯参数与端口号 |
| `DeviceModules` | 已列出主要模块 | 现场实际 SDK 版本与厂商型号 |

## 验收依据

- `Luster.Module.Motion.AOI.Core.dll` 出现在 `<OutputPath>Motions/` 下
- `SiteProfiles/{AOI1,AOI2,Wipe}/site-profile.xml` 出现在 `<OutputPath>SiteProfiles/` 下
- `Luster.Module.Motion.AOI.Core.Tests` 全部通过
- 启动现场时，错误 profile（缺字段或站点不匹配）能在自动流程进入前被拦截并给出明确错误文本
