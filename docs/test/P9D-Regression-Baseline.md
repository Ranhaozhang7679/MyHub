# P9-D 五轴 AOI 迁移自动化回归测试基线

> 关联 issue：TES-165（TES-34 P9-D）。覆盖工站状态机 / 模式切换 / 关键 IO 轴动作 / 握手信号 / 异常超时五类核心行为。
> 全程虚拟模式（`DeviceMode.Virtual` + `ZMotionMotionCard.SimulationMode=true`），无硬件依赖，可一键运行、可重复回归。

## 一键运行

```powershell
pwsh scripts/run-regression.ps1
# 或
powershell -File scripts/run-regression.ps1
```

脚本依次执行三个步骤并聚合 Pass/Fail，退出码 `0`=全绿、`1`=有失败：

1. `dotnet test tests/Luster.Module.Motion.FiveAxis.Tests/...csproj --filter "Category=Regression"`
2. `dotnet test tests/Luster.SimDevice.MotionCard.Tests/...csproj --filter "Category=Regression"`
3. `dotnet run --project src/Tools/Luster.Tools.DiffRegression/...csproj -c Release -- --self-test`

> **前置环境**：仓库根需有可还原的 `.\packages` 本地 NuGet 包源（`NuGet.config` 将 `Luster.*` 内部包映射到该本地源，GitLab 源已注释）。若 `.\packages` 缺失，测试工程报 `NU1301` 无法编译——恢复该包源后即可一键运行。DiffRegression 工具仅依赖 nuget.org 上的 Newtonsoft.Json，不受此影响。

## 分类法（Category）

所有回归用例带 `[Category("Regression")]` 伞标签，另按行为域叠加子类标签：`StationStateMachine` / `ModeSwitch` / `IOAxis` / `Handshake` / `Timeout` / `SourceAlignment` / `DiffFixture`。
`--filter "Category=Regression"` 即一键选取全部回归用例；按子类过滤可聚焦单域（如 `Category=Handshake`）。

## 五类核心行为 → 用例映射

### 1. 工站状态机（StationStateMachine）

被测代码：`RunStatus` 枚举（`src/Modules/Luster.TaskFlow.Common/Enums/RunStatus.cs:36`）、`MotionRunEngine.Run`（`src/Modules/Luster.TaskFlow.Motion/Modules/MotionRunEngine.cs:209`，IsBreak 早返 :212、BrokenOff :228-236）、`FiveAxisStation`（`src/Modules/Luster.Module.Motion.FiveAxis/Functions/StationNodes.cs:50`，DoExcute :82）。

| 用例 | 文件:行 | 覆盖点 |
|---|---|---|
| `MotionRunEngine_VirtualRun_SingleNodeChain_SetsSuccessStatus` | `tests/Luster.Module.Motion.FiveAxis.Tests/RegressionBaselineTests.cs:41` | 虚拟模式单节点链 Run → ok=true、Status=Success、无错误（驱动路径） |
| `MotionRunEngine_IsBreakTrue_EarlyReturn_DoesNotRun` | `RegressionBaselineTests.cs:66` | IsBreak=true 时 Run 早返不运行（早返路径） |
| `DeviceModeVirtual_EndToEnd_AOI1CapabilityChain_RunsGreenViaMotionRunEngine` | `tests/Luster.Module.Motion.FiveAxis.Tests/AOI1VirtualEndToEndTests.cs:195` | 16 节点端到端链跑绿（全链 RunStatus.Success） |
| `AllNewStationNodes_AreInstantiable` / `FiveAxisStation_ImplementsIFreeStation` 等 | `tests/Luster.Module.Motion.FiveAxis.Tests/StationNodesTests.cs:32,57,65,80` | 工站节点实例化 + IFreeStation 契约 |

### 2. 模式切换（ModeSwitch）

被测代码：`DeviceMode` 枚举（`src/Modules/Luster.Motion.DataStruct/Enums/DeviceMode.cs:36`，Virtual/Real 可选，Empty/Project/Debug `[Ignore]`）、`DeviceEngine.DeviceMode`（`src/Modules/Luster.SimDevice/Engine/DeviceEngine.cs:63`，公开可读写）。

| 用例 | 文件:行 | 覆盖点 |
|---|---|---|
| `DeviceEngine_SwitchRealToVirtual_ReflectedInProperty` | `RegressionBaselineTests.cs:95` | Real→Virtual 切换即时反映到属性 |
| `DeviceEngine_VirtualMode_ModuleRunGreen` | `RegressionBaselineTests.cs:109` | Virtual 模式下模块跑绿（证明虚拟模式可无硬件运行） |

> `[Ignore]` 模式（Empty/Project/Debug）不在公共可选范围，按约定不测。

### 3. 关键 IO 轴动作（IOAxis）

被测代码：`IMotionCard`（`src/Modules/Luster.Motion.DataStruct/Real/IMotionCard.cs`，GetDigitalIn:44/SetDigitalOut:58/SetAnalogOut:81/Home:135/Jog:153/Move:159/MoveLine:177/ServOn:206）、仿真后端 `ZMotionMotionCard{SimulationMode=true}`（`src/Modules/Luster.SimDevice.MotionCard/ZMotion/ZMotionMotionCard.cs`）。

| 用例 | 文件:行 | 覆盖点 |
|---|---|---|
| `ZMotion_SimCard_SetDigitalOut_NoThrow_GetDigitalIn_DefaultFalse_IndependentDicts` | `RegressionBaselineTests.cs:137` | SetDigitalOut 不抛；输入/输出独立字典，输出写入不反映到输入读取（诚实契约） |
| `ZMotion_SimCard_ServOn_Move_Home_NoThrow` | `RegressionBaselineTests.cs:158` | ServOn/Move/Home 轴动作不抛 |
| `ZMotion_SimCard_SetAnalogOut_NoThrow` | `RegressionBaselineTests.cs:176` | 模拟量输出不抛 |
| `SimulationMode_SupportsHomeMoveLineContinuousAndIoWithoutHardware` 等 | `tests/Luster.SimDevice.MotionCard.Tests/ZMotionMotionCardTests.cs:25,97,117,126,154` | 仿真卡 Home/MoveLine/Conti/Latch/IO 全套契约 |
| `AllTenContiLatchNodes_AreInstantiable` / `LatchOffsetCalc_...` 等 | `tests/Luster.Module.Motion.FiveAxis.Tests/ContiLatchNodesTests.cs:37,47,89,103` | Conti/Latch 节点结构与偏移计算 |

### 4. 握手信号（Handshake）

被测代码：`HandoverNode`（`src/Modules/Luster.Module.Motion.FiveAxis/Functions/StationNodes.cs:365`，`HandoverDirection{Feed,Leave}`:368，8 个 VIO 信号 :384-413，`SignalTimeoutMs` 默认 30000:417，Feed 15 步 `RunFeedHandover`:474 / Leave 13 步 `RunLeaveHandover`:505，`WaitSignal`:533 / `SetSignal`:540，DoExcute try/catch :444——`DeviceTimeoutException`→`OnAlarm(FailError)`+return false、 generic→`OnAlarm(DeviceError)`+return false）。

| 用例 | 文件:行 | 覆盖点 |
|---|---|---|
| `HandoverNode_Feed_NullSignals_RunsEmptySubset_Success` | `RegressionBaselineTests.cs:193` | Feed 信号全 null 跑空子集成功（WaitSignal/SetSignal 跳过） |
| `HandoverNode_Leave_NullSignals_RunsEmptySubset_Success` | `RegressionBaselineTests.cs:211` | Leave 方向跑空子集成功 |
| `HandoverNode_Feed_ConfiguredInputNeverArrives_TimesOut_ReturnsFalse` | `RegressionBaselineTests.cs:239` | **关键缺口**：配置输入信号 VIO 永不到达 → SignalTimeoutMs(200ms) 内超时返回 false + FailError 告警，不挂起（5s wall-clock 守卫） |
| `HandoverNode_FeedDirection_NoThrow_WhenSignalsUnconfigured` 等 | `StationNodesTests.cs:115,131,144,166` | 空子集不抛 + DoExcute try/catch 结构契约 |

> 单个 `HandoverNode` 无法自闭环（输出 VIO 与输入 VIO 是不同实例），故配置信号超时用例仅绑定一个永不到达的输入 VIO（`RecReady`），验证超时路径真实触发而非挂起。完整的 Feed/Leave 双向时序需对端工站配合，属联调范畴。

### 5. 异常超时（Timeout）

被测代码：`RtcpFrameExit.DoExcute`（`StationNodes.cs:315`，始终返回 true 幂等）、`RtcpFrameEnter`（:226，无设备结构化 false+errMsg）、`CrdConti.DoExcute`（`ContiLatchNodes.cs:74`，try/finally :97-150）、`LatchWait.DoExcute`（:413，try/finally :425-439）、`MotionRunEngine.Run` IsBreak/BrokenOff/ProcessAlarm。

| 用例 | 文件:行 | 覆盖点 |
|---|---|---|
| `RtcpFrameExit_Idempotent_NoDevice_ReturnsTrueTwice` | `RegressionBaselineTests.cs:285` | 无设备连续两次 DoExcute 均返回 true、不抛（急停/complete 段清理不阻断） |
| `RtcpFrameEnter_NoDevice_ReturnsFalseWithErrMsg_NoThrow` | `RegressionBaselineTests.cs:303` | 无设备结构化返回 false+errMsg，不静默吞错 |
| `CrdConti_NoDevice_ReturnsFalse_NoThrow` | `RegressionBaselineTests.cs:318` | 无设备返回 false、不抛 |
| `RtcpFrameExit_IsIdempotent_WhenNoDevice` / `RtcpFrameEnter_ReturnsFalse_WhenNoDevice_...` | `StationNodesTests.cs:93,103,153` | RTCP 帧退出/进入结构契约 |
| `CrdConti_DoExcuteHasTryFinally_...` / `LatchWait_DoExcuteHasTryFinally_...` | `ContiLatchNodesTests.cs:61,80` | Conti/Latch try/finally 清理契约（M13 关闭/清锁） |
| `FiveAxisFrame_SimulationMode_LifecycleRunsAndShortCircuits` 等 | `ZMotionMotionCardTests.cs:179,190,214` | 五轴帧生命周期 + 短路 |

## 源端 vs 迁移后基准对齐

- **既有对齐证据（独立基准）**：`CaliDiffFixtureTests.cs:46,84,125` 以独立推导的地面真值（ground truth）生成标定 baseline/actual CSV 对，经 `Luster.Tools.DiffRegression` 工具做 cali diff。据既有基线记录：26 字段 PASS、MaxError ≤1.78e-15（浮点噪声级）。`DiffRegression --self-test` 作为工具自检纳入一键脚本。
- **数学层独立对齐**：`Coord5AxisSourceAlignmentTests.cs` E1-E5（:44,62,81,98,119）以独立矩阵推导对齐源端运动学，容差 ≤1e-6；`Coord5AxisTests.cs` / `CalibrationDataModelTests.cs` / `FiveAxisKinematicsNodeTests.cs` / `LaserZCalibrateNodeTests.cs` 覆盖坐标/标定数据模型往返一致性。
- **完整源端轨迹捕获**：需人类提供源端运行基准数据（recipe 全链节点输出 trace），属数据依赖项——**不阻塞用例集本体建设**。源端基准到位后，可扩展 DiffRegression 的 matrix/detect/ct 模式做全链 diff。已按 issue 数据依赖条款升级人类提供源端基准。

## 核心路径覆盖率说明（≥80%）

五类核心行为 + 端到端 16 节点链（`AOI1VirtualEndToEndTests.cs:195`）覆盖了工站状态机驱动/早返、Virtual/Real 模式切换、IO 数字量/模拟量/轴动作（Home/Move/ServOn/Conti/Latch）、握手 Feed/Leave 空子集与超时告警、RTCP 帧进入/退出幂等与 Conti/Latch try/finally 清理。被测行为均落在已交付的 P5 软件层（TES-102 done）与 P9-C 虚拟模式（TES-68 done）内，核心路径覆盖满足 ≥80% 验收线。未覆盖项见下表，均属硬件/精度/外部依赖 carve-out，不影响软件层回归基线成立。

## 未覆盖项（carve-out）

| 项 | 原因 | 归属 |
|---|---|---|
| 精标 FrameCal（RTCP 五轴精标） | 需真机 RTCP 精度，ADR-TES-110 | TES-120 等硬件 carve-out |
| 检测 R3 vision | 需真实视觉硬件与算法联调 | 硬件 carve-out |
| CT 真机节拍 | 需真机实测 cycle time | 硬件 carve-out |
| 握手双向完整时序 | 需对端工站联调（上下料站配合） | P9-E FAT/SAT 联调 |
| 源端全链 trace diff | 待人类提供源端基准数据 | 数据依赖升级 |

## 治理标记

- Task level：T1（可独立推进的工程任务）
- Data level：D1（含源端基准比对数据，内部敏感）
