# EngineStatus 状态机分析报告

> 生成日期：2026-04-07
> 分析范围：`Luster.Motion.TaskFlow.Engine` 状态机逻辑
> 涉及文件：`EngineStatus.cs`、`MotionController.cs`、`MotionEngine_part1.cs`、`MotionEngine_part2.cs`

---

## 一、状态定义

`EngineStatus` 定义于 `Luster.Motion.DataStruct` 命名空间，标记了 `[Flags]` 属性：

| 值 | 枚举成员 | 中文描述 | 说明 |
|---|---|---|---|
| 0 | `Idle` | 空闲中 | 初始状态，软件启动后或完成加载后的默认状态 |
| 1 | `Homing` | 回零中 | 设备回零过程中 |
| 2 | `Ready` | 待启动 | 回零成功，等待用户启动 |
| 4 | `Running` | 运行中 | 流程正在执行 |
| 8 | `Alarm` | 报警中 | 发生报警，需复位后才能恢复 |
| 16 | `Pause` | 暂停中 | 用户主动暂停或可恢复的暂停 |
| 32 | `Stop` | 停止中 | 流程停止，几乎所有状态都可进入 Stop |
| 64 | `Resetting` | 复位中 | 报警后的复位操作进行中 |
| 128 | `MaterialPending` | 待料中 | **仅 UI 显示，不参与引擎内部状态转换** |

---

## 二、状态转换全景

### 2.1 标准流程

```
Idle ──Home()──> Homing ──HomeTask成功──> Ready ──RunStations()──> Running
```

### 2.2 暂停与恢复

```
Running ──Pause(isAlarm=false)──> Pause ──Recovery()──> Running
Running ──Pause(isAlarm=true)──-> Alarm   (报警触发)
```

### 2.3 报警与复位

```
Running ──报警触发──> Alarm ──Recovery()──> Resetting ──复位成功──> Ready / Pause / Idle
                                                 └──复位失败──> 保持原状态
```

### 2.4 停止（任意状态可达）

```
Any ──Stop()──> Stop ──Home()──> Homing ──> Ready
```

### 2.5 完整转换矩阵

| 当前状态 \ 允许的操作 | Home | Start/RunStations | Pause | Recovery | Reset | Stop |
|---|---|---|---|---|---|---|
| **Idle (0)** | -> Homing | - | - | - | - | -> Stop |
| **Homing (1)** | - | - | -> Alarm | - | - | -> Stop |
| **Ready (2)** | - | -> Running | - | - | - | -> Stop |
| **Running (4)** | - | - | -> Pause / Alarm | - | - | -> Stop |
| **Alarm (8)** | - | - | - | - | -> Resetting | -> Stop |
| **Pause (16)** | - | -> Resetting + Running | - | -> Running | - | -> Stop |
| **Stop (32)** | -> Homing | - | - | - | - | - |
| **Resetting (64)** | - | - | - | - | - | -> Stop（可中断） |

---

## 三、各方法的入口条件与转换逻辑

### 3.1 Home（回零）

- **文件**：`MotionEngine_part2.cs:185-266`，`MotionController.cs:1393-1497`
- **入口条件**（MotionController 层）：
  - `MachineStatus != Running`（否则抛异常）
  - 非虚拟模式
  - 急停未按下、烟雾报警未触发
- **状态转换**：`Any(非Running) -> Homing -> Ready`
- **失败路径**：HomeTask 失败后状态保持 `Homing`，用户只能 `Stop` 后重新回零
- **注意**：内部调用 `tokenSource?.Cancel()` 终止之前的运行线程

### 3.2 Start / RunStations（启动）

- **文件**：`MotionEngine_part2.cs:450-537`，`MotionController.cs:821-988`
- **入口条件**：
  - **Pause 状态**：先调用 `Reset(AlarmInfo)`，成功后调用 `Recovery()`
  - **Ready 状态**：直接调用 `RunStations()`
- **状态转换**：`Ready -> Running`
- **失败路径**：工站运行失败时调用 `Stop()` + 红灯蜂鸣

### 3.3 Pause（暂停）

- **文件**：`MotionEngine_part2.cs:546-609`，`MotionController.cs:1289-1357`
- **入口条件**（MotionController 层）：
  - `(!isAlarm && Pause) || Ready || Idle || Stop` 时直接返回（不暂停）
  - Running、Homing 等状态才执行暂停
  - `lockCommand > 0` 防连续点击
- **状态转换**：
  - `isAlarm=true`：`Running/Homing -> Alarm`
  - `isAlarm=false`：`Running/Homing -> Pause`
- **机制**：使用 `ManualResetEventSlim`（`pauseResetEvent.Reset()`）阻塞所有工站线程

### 3.4 Recovery（恢复）

- **文件**：`MotionEngine_part2.cs:614-625`
- **入口条件**：`EngineStatus == Pause`
- **状态转换**：`Pause -> Running`
- **机制**：`pauseResetEvent.Set()` 释放所有阻塞线程

### 3.5 Reset（复位）

- **文件**：`MotionEngine_part2.cs:631-742`，`MotionController.cs:1159-1258`
- **入口条件**（MotionController 层）：
  - `MachineStatus == Alarm`（非 Alarm 直接返回）
  - 安全门已关闭
  - 硬件 Recovery 成功
  - 无急停、无光栅遮挡、无 PLC 报警
- **状态转换**：`Alarm -> Resetting -> Pause/Ready/Idle`
- **复位后目标状态取决于 `alarmInfo.EStatus`**：
  - `alarmInfo.EStatus == Ready` -> Ready
  - `alarmInfo.EStatus == Idle` -> Idle
  - 其他 -> Pause
- **失败路径**：部分模块复位失败 -> 状态保持原状态（Alarm/Stop）
- **中断**：复位过程中如果用户点击 Stop，立即中断复位

### 3.6 Stop（停止）

- **文件**：`MotionEngine_part2.cs:747-805`，`MotionController.cs:1107-1153`
- **入口条件**：`EngineStatus != Stop`（已停止则直接返回）
- **状态转换**：`Any(non-Stop) -> Stop`
- **机制**：`tokenSource?.Cancel()` 取消所有线程；清除待复位模块；所有模块 `IsBreak = true`

---

## 四、报警触发的状态转换

报警通过 `MotionController.OnAlarm()` 方法分发，不同 `AlarmType` 触发不同行为：

| AlarmType | 行为 | 状态转换 |
|---|---|---|
| `WarningTip` / `FailError` / `Timeout` / `PopInfoTip` | `Pause(isAlarm=true)` | -> Alarm |
| `DeviceError`（急停/烟雾） | 通过 OnAlarm 设置报警 | -> Alarm |
| `PlcAlarm` | **直接赋值** `EngineStatus = Alarm`（绕过 Pause 流程） | -> Alarm |
| `InfoTip` | 仅记录日志，不改变状态 | 无转换 |

报警信息 `AlarmInfo.EStatus` 记录的是报警发生时引擎的状态，用于后续 Reset 复位成功后决定恢复到哪个状态。

---

## 五、分层架构

```
┌─────────────────────────────────────────────────────────┐
│  UI 层 (Prism MVVM)                                      │
│  CommonBus / FlowBus / ToolBarContentVM / Hive界面        │
└──────────────┬──────────────────────────────────────────┘
               │ 按钮点击 / EngineStatusEvent 通知
┌──────────────▼──────────────────────────────────────────┐
│  控制器层 (MotionController)                              │
│  Start / Stop / Pause / Home / Recovery / OnAlarm        │
│  MachineMonitor (IO/按钮监控) / LightManager / RobotDO   │
│  lockCommand (防连击)                                     │
└──────────────┬──────────────────────────────────────────┘
               │ 调用引擎方法 / 赋值 EngineStatus
┌──────────────▼──────────────────────────────────────────┐
│  引擎层 (MotionEngine)                                    │
│  EngineStatus 属性 (setter 有事件守卫，但当前不拦截)       │
│  RunStations / Pause / Recovery / Reset / Stop / Home    │
│  pauseResetEvent / tokenSource                            │
└──────────────┬──────────────────────────────────────────┘
               │ 硬件操作回调
┌──────────────▼──────────────────────────────────────────┐
│  设备层 (IDeviceEngine)                                   │
│  Recovery / Stop / IsHome / AlarmEvent                    │
└─────────────────────────────────────────────────────────┘
```

---

## 六、状态机健全性评估

### 总体结论：当前不是一个合格的状态机

当前代码能工作，是因为**调用方小心地按正确顺序调用**，而非状态机本身保证了正确性。本质上是一个"靠纪律维持的伪状态机"。

### 6.1 致命缺陷：缺少集中式守卫

`EngineStatus` 的 setter 不做任何合法性校验：

```csharp
set
{
    if (status != value)
    {
        bool? canChange = EngineStatusEvent?.Invoke(status, value);
        if (canChange.Value)    // ← 没有对 canChange 做 null 检查
        {
            _eStatus = value;
        }
    }
}
```

**问题**：
- 状态是否合法转换，**完全取决于调用方自己判断**
- `EngineStatusEvent` 是多播委托，`?.Invoke()` 只返回**最后一个订阅者**的返回值
- `canChange` 可能为 `null`（无订阅者时），直接 `.Value` 会抛 `NullReferenceException`
- **任何代码都可以把状态从 Idle 直接设为 Running**，没有任何机制阻止

### 6.2 状态转换依赖调用顺序，存在竞态

```csharp
if (MachineStatus == EngineStatus.Ready)   // 检查
{
    // ... 中间几十行代码 ...
    MotionEngine.RunStations();            // 使用，此时状态可能已被别的线程改了
}
```

`MachineStatus` 属性每次都从 `MotionEngine.EngineStatus` 实时读取，但**检查和使用之间没有原子性保证**。定时器回调、IO 信号、UI 操作都可能并发修改状态。

### 6.3 `[Flags]` 标记与互斥使用矛盾

`[Flags]` 意味着允许组合（如 `Alarm | Pause = 24`），但实际所有逻辑都假设状态是**互斥单值**。没有任何代码处理组合值的情况。如果某处代码意外设置了组合值，所有 `if (MachineStatus == EngineStatus.xxx)` 判断都会失效。

### 6.4 异常路径中状态可能"卡死"

**场景 1：Pause 失败后 lockCommand 泄漏**

```csharp
Interlocked.Increment(ref lockCommand);
MotionEngine.Pause(isAlarm, (isOk, ex) =>
{
    Interlocked.Decrement(ref lockCommand);  // 回调中递减
});
```

如果 `MotionEngine.Pause` 内部在回调触发前就抛了异常，`lockCommand` 永远不会递减，后续所有操作都被拦截。

**场景 2：Reset 中途被 Stop 打断**

```csharp
EngineStatus = EngineStatus.Resetting;
// ... 逐个模块复位 ...
// 如果此时外部调用 Stop()，状态被强制设为 Stop
// 但部分模块已经复位成功，部分没有 → 不一致状态
```

**场景 3：PLC 报警直接赋值绕过所有守卫**

```csharp
MotionEngine.EngineStatus = EngineStatus.Alarm;  // 直接赋值
```

跳过了 `Pause()` 方法中的 IO 处理、灯控制、机器人信号等清理逻辑。

### 6.5 缺少的状态/转换

| 缺失 | 说明 |
|---|---|
| **HomingFailed 状态** | HomeTask 失败后状态保持 Homing，用户只能 Stop 再重来 |
| **Stop 中 Stop 的幂等性** | `lockCommand > 0` 会导致 Stop 被跳过 |
| **Ready 状态下报警** | Pause 方法排除了 Ready，Home 过程中报警只能先 Stop |
| **MaterialPending** | 引擎完全不识别此状态，`[Flags]` 下若意外设上，所有 `==` 判断失效 |

### 6.6 评分总结

| 维度 | 评分 | 说明 |
|---|---|---|
| **确定性** | 不合格 | 多处竞态，无原子状态检查 |
| **完整性** | 勉强 | 主流程覆盖了，异常路径有盲区 |
| **防护性** | 不合格 | setter 无守卫，`[Flags]` 引入组合风险 |
| **可维护性** | 差 | 转换逻辑分散在 MotionController 2000+ 行中，无集中定义 |
| **健壮性** | 差 | lockCommand 泄漏、回调前异常、PLC 绕过守卫 |

---

## 七、优化建议

### 优化 #1（高优先级）：引入集中式状态转换表

**现状**：状态转换的合法性校验分散在各方法的 `if` 判断中，setter 不做校验。

**建议**：在 setter 中增加合法转换字典：

```csharp
private static readonly Dictionary<EngineStatus, HashSet<EngineStatus>> _validTransitions = new()
{
    [EngineStatus.Idle]       = new() { EngineStatus.Homing, EngineStatus.Stop },
    [EngineStatus.Homing]     = new() { EngineStatus.Ready, EngineStatus.Alarm, EngineStatus.Stop },
    [EngineStatus.Ready]      = new() { EngineStatus.Running, EngineStatus.Stop, EngineStatus.Homing },
    [EngineStatus.Running]    = new() { EngineStatus.Pause, EngineStatus.Alarm, EngineStatus.Stop },
    [EngineStatus.Alarm]      = new() { EngineStatus.Resetting, EngineStatus.Stop },
    [EngineStatus.Pause]      = new() { EngineStatus.Running, EngineStatus.Resetting, EngineStatus.Stop },
    [EngineStatus.Resetting]  = new() { EngineStatus.Ready, EngineStatus.Pause, EngineStatus.Idle, EngineStatus.Alarm, EngineStatus.Stop },
    [EngineStatus.Stop]       = new() { EngineStatus.Homing },
};
```

**收益**：堵住 PLC 报警绕过守卫等所有非法转换路径，改动最小、收益最大。

---

### 优化 #2（中优先级）：移除 `[Flags]` 属性

**现状**：`EngineStatus` 标记了 `[Flags]`，但实际使用中各状态是互斥的，从未出现组合值。

**建议**：移除 `[Flags]` 属性，改为普通枚举。如果未来需要组合状态，应改用独立的 bool 属性而非位运算。

**风险**：若意外产生组合值（如 `Alarm | Pause = 24`），所有 `==` 判断失效。

---

### 优化 #3（中优先级）：`lockCommand` 替换为 `SemaphoreSlim`

**现状**：使用 `int lockCommand` + `Interlocked.Increment/Decrement` 实现防连击。

**问题**：
- 递增后如果方法中途异常返回，可能忘记递减导致永久锁定
- 异常路径中多处 `Decrement` 分散，维护成本高

**建议**：使用 `SemaphoreSlim(1, 1)` 或在方法入口统一用 `try/finally` 保证释放。

---

### 优化 #4（中优先级）：`EngineStatusEvent` 职责分离

**现状**：`EngineStatusEvent` 同时承担"通知 UI"和"否决状态转换"两个职责。

**问题**：
- 所有注册者返回 `true`，否决功能形同虚设
- 多播委托只取最后一个返回值，前面的否决被静默忽略
- `canChange` 可能为 `null`，存在 NRE 风险

**建议**：
- 状态校验放在 setter 内部的转换表中
- UI 通知使用独立的 `StatusChanged` 事件

---

### 优化 #5（低优先级）：`Thread.Sleep` 替换为时间戳去抖

**现状**：`MachineMonitor()` 中使用 `Thread.Sleep(10)` 做去抖（急停、烟雾报警），在 100ms 定时器回调线程中执行。`Home()` 方法中也有 `Thread.Sleep(100)`。

**建议**：使用独立的时间戳比对（`Stopwatch`），而非 `Sleep` 阻塞监控线程。

---

### 优化 #6（低优先级）：Recovery 接受显式 targetStatus 参数

**现状**：`Reset()` 成功后恢复到哪个状态取决于 `alarmInfo.EStatus`（报警发生时的状态），而非显式参数。

**问题**：如果报警信息被覆盖或为 `null`，可能导致恢复到意外状态。

**建议**：`Recovery()` 接受一个显式的 `targetStatus` 参数，而非依赖 `alarmInfo.EStatus` 的暗示耦合。

---

## 八、关联文件

| 文件 | 用途 |
|---|---|
| `doc/EngineStatus_StateMachine.dot` | 状态转换关系图（含优化项标注） |
| `doc/EngineStatus_Architecture.dot` | 分层架构图（含优化项标注） |
| `src/Device/Luster.Motion.DataStruct/Enums/EngineStatus.cs` | 状态枚举定义 |
| `src/Engine/Luster.Motion.TaskFlow.Engine/MotionController.cs` | 控制器（状态转换调用入口） |
| `src/Engine/Luster.Motion.TaskFlow.Engine/Engine/MotionEngine_part2.cs` | 引擎核心状态转换逻辑 |
