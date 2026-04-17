# JunRudderVCM 使用 SOP

> 钧舵(JunRudder) GSFDmini 伺服驱动器 + 音圈电机执行器
> 通信: EtherCAT(CIA402) via 固高(GoogolTech)板卡
> 参考文档: GSFDmini伺服驱动器用户手册-20250723.pdf

---

## 1. 硬件与接线

### 1.1 系统组成

| 组件 | 型号 | 说明 |
|------|------|------|
| 控制卡 | 固高 EtherCAT 板卡 | EtherCAT 主站 |
| 驱动器 | GSFDmini | 伺服驱动器，内置力位控制 |
| 执行器 | 钧舵音圈电机 | 含编码器 |

### 1.2 关键特性

| 特性 | 说明 |
|------|------|
| 力控方式 | 驱动器内置开环力位控制(P96)，SDO 配参数后驱动器自主完成 |
| 压力反馈 | 0x201Bh 模拟量(-10V~10V → -32768~32767)，线性标定转压力 |
| 运动流程 | 快进 → 一段速度(逼近) → 二段速度(探测) → 保压 → 回退 |
| 位置单位 | mm × PerPluse(脉冲/mm) → 脉冲数写入 SDO |

### 1.3 接线要点

- EtherCAT 网线连接: 控制卡 Out → GSFDmini In → 下一从站(如有)
- 编码器线: 驱动器自带，无需额外接线
- 模拟量输入: 压力传感器信号接入驱动器模拟量端口(0x201B 对应通道)

### 1.4 EtherCAT 配置

1. 使用固高配置工具生成 ENI 文件
2. 确保 PDO 映射包含:
   - TxPDO: 0x6041(状态字)、0x6064(位置反馈)、0x201B(压力反馈)等
   - RxPDO: 0x6040(控制字)、0x6060(运行模式)等
3. 软着陆力控参数通过 SDO 写入，不需要额外 PDO 映射

---

## 2. 动作类型总览

| 动作类型 | 枚举值 | 说明 | 典型用途 |
|----------|--------|------|----------|
| 使能 | ServoOn | 伺服使能 | 开机初始化 |
| 复位 | Reset | 清除报警 | 异常恢复 |
| 失能 | ServoOff | 伺服下使能 | 停机/维护 |
| 回零 | Home | 标准 CIA402 回零 | 开机回零 |
| 硬着陆 | HardLanding | 绝对定位运动 | 点位移动 |
| 软着陆 | SoftLanding | 驱动器内置力控(开环力位控制 P96) | 力控压装 |

---

## 3. 各动作详细说明

### 3.1 使能 (ServoOn)

**调用:** `_axis.ServOn(true)`

**内部流程:**
1. 通过固高板卡调用 CIA402 标准使能序列
2. Switch on disabled → Ready to switch on → Switch on → Operation enabled

**使用时机:** 开机后、运动前必须先使能

---

### 3.2 复位 (Reset)

**调用:** `_axis.ResetStatus()`

**内部流程:** 清除报警 + 重新使能

**使用时机:** 轴报警后恢复

---

### 3.3 失能 (ServoOff)

**调用:** `_axis.ServOn(false)`

**使用时机:** 维护、停机时

---

### 3.4 回零 (Home)

**调用:** `_axis.Home()` + `_axis.CheckHomeDone(HomeTimeout)`

**内部流程:**
1. 通过固高板卡标准回零方法启动回零
2. 等待回零完成，超时由 `HomeTimeout` 参数控制

**参数说明:**

| 参数 | 单位 | 默认值 | 说明 |
|------|------|--------|------|
| 回零模式代码 | — | 0 | CIA402 回零模式 |
| 回零高速 | mm/s | 50 | 快速搜索速度 |
| 回零低速 | mm/s | 10 | 慢速搜索速度 |
| 回零加速度 | mm/s² | 1000 | |
| 回零超时 | 秒 | 60 | |

---

### 3.5 硬着陆 (HardLanding)

**流程:**
```
MoveAbs(目标位置, 速度, 加速度, 减速度) → CheckMotionDone → 判断位置是否在范围内
```

**参数:**

| 参数 | 说明 |
|------|------|
| 目标位置 | 绝对位置(mm) |
| 运动速度 | mm/s |
| 加速度/减速度 | mm/s² |
| 位置上限/下限 | 到位判断范围(mm) |

**输出:**
- OutResult: 位置是否在[下限, 上限]范围内
- OutPosition: 实际位置
- OutFailReason: 失败原因(位置超限时输出)

---

### 3.6 软着陆 (SoftLanding)

**文档依据:** GSFDmini 用户手册 P96 (3.6.6 力控开环模式)

**原理:**
> 驱动器内置力位控制系统，通过 SDO 配置位置/速度/扭矩参数后，
> 向 0x2016h 写入上升沿触发，驱动器自主完成:
> 快进 → 一段速度(逼近) → 二段速度(探测) → 保压 → 回退。
> 上位机通过轮询 0x201Ah 状态机监控执行进度。

**核心 SDO 地址:**

| 地址 | 名称 | 读/写 | 用途 |
|------|------|-------|------|
| 0x2016 | 力控触发 | W | bit0 上升沿触发, bit2=1 立即回退, bit8~11 保持 CSP |
| 0x201A | 力控状态 | R | bit0~3: 1=准备/完成, 2=快进, 3=一段, 4=二段, 6=回退 |
| 0x201B | 压力反馈 | R | 模拟量 -32768~32767, 对应 -10V~10V |

**执行流程:**
```
1. 写入力控参数-位置 (mm × PerPluse → 脉冲数):
   - 0x2009 → 回退位置
   - 0x200A → 快进位置
   - 0x200B → 速度切换位置
   - 0x200C → 最大行程限制

2. 写入力控参数-速度 (mm/s × PerPluse → 脉冲/s):
   - 0x200E → 一段速度(逼近)
   - 0x200F → 二段速度(探测)
   - 0x2010 → 快进/回退速度
   - 0x2012 → 加速度
   - 0x2013 → 减速度

3. 写入力控参数-判定:
   - 0x2011 → 停止速度阈值 (mm/s × PerPluse)
   - 0x2014 → 力矩保持时间(ms)
   - 0x2015 → 判断停止时间(ms)

4. 写入扭矩限制:
   - 0x2017 → 扭矩正向限制(峰值电流1/10000)
   - 0x2018 → 扭矩负向限制(同上)

5. 确保 CSP 模式: 0x6060 = 8, 延时 50ms

6. 触发力控 (0x2016 上升沿):
   - 写 0x0F00 (bit8~11=1, bit0=0)
   - 延时 10ms
   - 写 0x0F01 (bit0=1, 上升沿触发)

7. 轮询等待完成 (每 10ms):
   ├─ 读取 0x201A, 取 phase = state & 0x0F
   ├─ phase > 1 → 标记已启动
   ├─ 已启动 且 phase == 1 → 力控完成
   │   → OutPosition = 当前位置, OutPressure = 读取压力
   │   → OutResult = true, 返回
   └─ 超时 → OutResult = false
```

**力控状态机 (0x201A bit0~3):**

| phase | 阶段 | 说明 |
|-------|------|------|
| 1 | 准备/完成 | 初始状态，或力控执行完毕(保压+回退完成) |
| 2 | 快进 | 以快进速度快速接近 |
| 3 | 一段速度 | 到达快进位置后，以一段速度逼近 |
| 4 | 二段速度(探测) | 到达速度切换位置后，以二段速度低速探测接触 |
| 6 | 回退 | 保压完成后，回退到回退位置 |

**参数说明:**

| 参数 | SDO地址 | 单位 | 默认值 | 说明 |
|------|---------|------|--------|------|
| 扭矩正向限制 | 0x2017/0x2018 | 峰值电流1/10000 | 1000 | 控制最大出力 |
| 快进位置 | 0x200A | mm | — | 快速接近的目标位置 |
| 速度切换位置 | 0x200B | mm | — | 一段→二段切换位置 |
| 最大行程限制 | 0x200C | mm | 20 | 超过此位置立即停止 |
| 回退位置 | 0x2009 | mm | 0 | 保压完成后回退到的位置 |
| 一段速度(逼近) | 0x200E | mm/s | 20 | 接近产品的速度 |
| 二段速度(探测) | 0x200F | mm/s | 5 | 探测接触的低速 |
| 快进/回退速度 | 0x2010 | mm/s | 50 | 快进和回退阶段的速度 |
| 加速度 | 0x2012 | mm/s² | 1000 | |
| 减速度 | 0x2013 | mm/s² | 1000 | |
| 停止速度阈值 | 0x2011 | mm/s | 0.5 | 速度低于此值判定为停止 |
| 力矩保持时间 | 0x2014 | ms | 500 | 探测到接触后保压时长 |
| 判断停止时间 | 0x2015 | ms | 100 | 判断停止的采样时间窗口 |
| 软着陆超时 | — | 秒 | 10 | 上位机层面的超时保护 |
| 标定系数K | — | — | 1.0 | 压力 = K × 0x201B原始值 + B |
| 标定偏移B | — | — | 0.0 | |

**输出:**
- OutResult: 是否成功
- OutPosition: 完成时的实际位置(mm)
- OutPressure: 完成时的实际压力(经标定换算)
- OutFailReason: 超时时输出详细原因

**紧急停止:**
调用 `Stop()` 方法会:
1. 设置中断标志
2. 写 0x2016 = 0x0F04 (bit2=1) 立即结束力控
3. 调用 `_axis.Stop()` 停止轴运动

---

## 4. 调用链

### 4.1 整体调用架构

```
用户任务流 (TaskFlow)
  └─ JunRudderVCM.DoExcute()
       ├─ GetVDevice<VAxis>(DeviceParam, out _axis)   // 解析轴设备
       └─ switch(ActionType)
            ├─ ExecuteServoOn()      → _axis.ServOn(true)
            ├─ ExecuteReset()        → _axis.ResetStatus()
            ├─ ExecuteServoOff()     → _axis.ServOn(false)
            ├─ ExecuteHome()         → _axis.Home() + _axis.CheckHomeDone()
            ├─ ExecuteHardLanding()  → _axis.MoveAbs() + _axis.CheckMotionDone()
            └─ ExecuteSoftLanding()  → _axis.SDOWrite() + _axis.SDORead() 轮询
```

### 4.2 软着陆 SDO 调用链

```
JunRudderVCM.ExecuteSoftLanding()
  │
  ├─ _axis.SDOWrite(index, subindex, data, data_size)
  │     │
  │     └─ VAxis.SDOWrite()                          // Luster.Motion.DataStruct
  │           │  ProcessAction(() => {
  │           │    motionCard.SDOWrite(axisNo, index, subindex, data, data_size)
  │           │  })
  │           │
  │           └─ GGMotionCard.SDOWrite()              // Luster.SimDevice.MotionCard.GG
  │                 │  CheckInit()
  │                 │  GetAxisNum(slave → coreNum, axisNum)
  │                 │  SafeNativeMethod((out err) => {
  │                 │    mc.GTN_EcatSDODownload(coreNum, axisNum, index, subIndex, ...)
  │                 │  })
  │                 │
  │                 └─ mc.GTN_EcatSDODownload()        // P/Invoke → gts.dll
  │                       [DllImport("gts.dll")]
  │                       short GTN_EcatSDODownload(core, slave_position, index, subindex, ...)
  │
  └─ _axis.SDORead(index, subindex, data_size, out value, count)
        │
        └─ VAxis.SDORead()                             // Luster.Motion.DataStruct
              │  ProcessAction(() => {
              │    motionCard.SDORead(axisNo, index, subindex, data_size, out pBuf, count)
              │  })
              │
              └─ GGMotionCard.SDORead()                 // Luster.SimDevice.MotionCard.GG
                    │  CheckInit()
                    │  GetAxisNum(slave → coreNum, axisNum)
                    │  SafeNativeMethod((out err) => {
                    │    mc.GTN_EcatSDOUpload(coreNum, axisNum, index, subIndex, ...)
                    │  })
                    │
                    └─ mc.GTN_EcatSDOUpload()            // P/Invoke → gts.dll
                          [DllImport("gts.dll")]
                          short GTN_EcatSDOUpload(core, slave_position, index, subindex, ...)
```

### 4.3 运动控制调用链

```
JunRudderVCM
  │
  ├─ _axis.ServOn(true/false)
  │     └─ GGMotionCard.ServOn()  → mc.GTN_ServoOn(core, axis)
  │
  ├─ _axis.Home()
  │     └─ GGMotionCard.Home()    → mc.GTN_PrfHome(core, axis) + mc.GTN_Update(core, mask)
  │
  ├─ _axis.MoveAbs(pos, vel, acc, dec)
  │     └─ GGMotionCard.MoveAbs() → mc.GTN_PrfTrap/GTN_SetPos/GTN_SetVel/GTN_Update
  │
  ├─ _axis.Stop()
  │     └─ GGMotionCard.Stop()    → mc.GTN_Stop(core, axis)
  │
  └─ _axis.GetCurrentPos()
        └─ GGMotionCard.GetCurrentPos() → mc.GTN_GetEncPos(core, axis, ...)
```

### 4.4 轴号映射

```
外部轴号 (AxisNo, 从1开始)
  │
  └─ GGMotionCard.GetAxisNum()
       │
       ├─ axisNo ∈ [1, axisCountCore1]       → core=coreNum1(1), axisNum=axisNo
       └─ axisNo ∈ (axisCountCore1, total]   → core=coreNum2(2), axisNum=axisNo-axisCountCore1

固高 SDK 参数映射:
  core     → short (内核号, 1 或 2)
  axisNum  → ushort (EtherCAT 从站编号, 即轴在内核内的序号)
```

### 4.5 SDO 数据类型映射

```
Function 层          VAxis 层           GGMotionCard 层         固高 SDK
─────────────────────────────────────────────────────────────────────────
_axis.SDOWrite(      VAxis.SDOWrite(    GGMotionCard.SDOWrite(  GTN_EcatSDODownload(
  0x2017, 0,           axisNo,            slave,                  core,
  TorquePositiveLimit,   0x2017, 0,        0x2017, 0,             axisNum,
  2)                     data, 2)          data, 2)                byte[], data_size,
                                                                     out errCode)
数据类型转换:
  int data → byte[] = BitConverter.GetBytes(data) → ref byte[0] 传入 SDK
  data_size=1: 1字节, data_size=2: 2字节(Int16), data_size=4: 4字节(Int32)
```

---

## 5. 压力标定流程

### 5.1 标定原理

钧舵音圈电机通过模拟量反馈(0x201Bh)间接测量压力:
```
压力 = K × 0x201B原始值 + B
```
0x201B 为 -32768~32767 的 int16 值，对应 -10V~10V 模拟量输入。

### 5.2 标定步骤

**准备工作:**
- 外部力传感器(精度 0.1g 或更高)
- 标定块

**标定流程:**
1. 电机回零并使能
2. 执行软着陆(使用较小扭矩限制)
3. 记录 0x201B 原始值和力传感器实际读数
4. 修改扭矩限制，重复多组数据

**标定数据示例:**

| 次数 | 0x201B原始值 | 力传感器(g) |
|------|-------------|------------|
| 1 | 500 | 120 |
| 2 | 1000 | 245 |
| 3 | 1500 | 368 |
| 4 | 2000 | 492 |
| 5 | 2500 | 615 |

### 5.3 计算标定系数

使用最小二乘法拟合线性关系:
```
力传感器(g) = K × 0x201B原始值 + B
```

以上数据为例: K ≈ 0.247, B ≈ -3.2

**在 Function 中填入:**
- 标定系数K = 0.247
- 标定偏移B = -3.2

---

## 6. 典型工艺流程

### 6.1 开机初始化

```
Step 1: 使能 (ServoOn)
Step 2: 回零 (Home)
```

### 6.2 压装工艺

```
Step 1: 硬着陆 (HardLanding) → 移动到接近产品的安全位置(可选)
Step 2: 软着陆 (SoftLanding) → 驱动器自主完成力控压装
         - 快进位置: 产品表面上方约5mm
         - 速度切换位置: 产品表面上方约1mm
         - 一段速度: 10-20mm/s
         - 二段速度: 2-5mm/s (探测速度，越小精度越高)
         - 扭矩限制: 根据目标压力设定
         - 保压时间: 300-1000ms
         - 回退位置: 0mm
Step 3: 硬着陆 (HardLanding) → 移动到安全位置
```

### 6.3 异常恢复

```
Step 1: 复位 (Reset) → 清除报警并重新使能
Step 2: 根据需要重新回零或继续操作
```

### 6.4 关机

```
Step 1: 移动到安全位置 (HardLanding)
Step 2: 失能 (ServoOff)
```

---

## 7. 故障排查

### 7.1 常见错误

| 现象 | 可能原因 | 处理 |
|------|----------|------|
| 使能失败 | 驱动器硬件异常 | 检查接线，尝试复位 |
| SDO 读写失败 | 板卡未初始化/从站号错误 | 检查板卡初始化和轴号配置 |
| 回零超时 | 回零参数不合理 | 检查回零模式和速度设置 |
| 软着陆不触发 | CSP 模式未就绪 | 确认 0x6060 = 8 已写入 |
| 软着陆一直不完成 | 扭矩限制太大/太小 | 调整扭矩限制参数 |
| 软着陆超时 | 扭矩不够导致探测不到接触 | 增大扭矩限制或减小二段速度 |
| 压力值异常 | 标定系数K/B未配置 | 先完成压力标定 |

### 7.2 SDO 地址速查

| 地址 | 名称 | 读/写 | 用途 |
|------|------|-------|------|
| 0x6040 | 控制字 | W | CIA402 控制指令 |
| 0x6041 | 状态字 | R | CIA402 状态反馈 |
| 0x6060 | 运行模式 | R/W | 6=回零, 8=CSP |
| 0x2009 | 回退位置 | W | pls |
| 0x200A | 快进位置 | W | pls |
| 0x200B | 速度切换位置 | W | pls |
| 0x200C | 最大行程限制 | W | pls |
| 0x200E | 一段速度 | W | pls/s |
| 0x200F | 二段速度 | W | pls/s |
| 0x2010 | 快进/回退速度 | W | pls/s |
| 0x2011 | 停止速度阈值 | W | pls/s |
| 0x2012 | 加速度 | W | pls/s² |
| 0x2013 | 减速度 | W | pls/s² |
| 0x2014 | 力矩保持时间 | W | ms |
| 0x2015 | 判断停止时间 | W | ms |
| 0x2016 | 力控触发/控制 | R/W | bit0=触发, bit2=回退, bit8~11=CSP保持 |
| 0x2017 | 扭矩正向限制 | W | 峰值电流1/10000 |
| 0x2018 | 扭矩负向限制 | W | 峰值电流1/10000 |
| 0x201A | 力控状态 | R | bit0~3: 阶段码 |
| 0x201B | 压力反馈(模拟量) | R | -32768~32767 |
| 0x603F | 报警代码 | R | 故障诊断 |

---

## 8. 参数速查表

### 8.1 公共参数(所有动作可见)

| 序号 | 名称 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| 0 | 轴设备选择 | VAxis | — | 必选，选择对应的轴 |
| 1 | 动作类型 | Enum | ServoOn | 选择要执行的动作 |
| 2 | 目标位置(mm) | double | — | 硬着陆目标位置 |
| 3 | 位置上限(mm) | double | — | 硬着陆到位判断 |
| 4 | 位置下限(mm) | double | — | 硬着陆到位判断 |
| 5 | 运动速度(mm/s) | double | 50 | 硬着陆速度 |
| 6 | 加速度(mm/s²) | double | 1000 | |
| 7 | 减速度(mm/s²) | double | 1000 | |

### 8.2 软着陆参数(仅 SoftLanding 可见)

| 序号 | 名称 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| 8 | 扭矩正向限制 | int | 1000 | 峰值电流1/10000，控制最大出力 |
| 9 | 二段速度-探测速度(mm/s) | double | 5 | 探测接触的低速 |
| 10 | 快进位置(mm) | double | — | 快速接近的目标位置 |
| 11 | 速度切换位置(mm) | double | — | 一段→二段切换位置 |
| 12 | 软着陆超时(秒) | int | 10 | 上位机超时保护 |
| 13 | 回退位置(mm) | double | 0 | 保压后回退到的位置 |
| 14 | 最大行程限制(mm) | double | 20 | 超程保护 |
| 15 | 快进/回退速度(mm/s) | double | 50 | 快进和回退的速度 |
| 16 | 一段速度-逼近速度(mm/s) | double | 20 | 接近产品的速度 |
| 24 | 力矩保持时间(ms) | int | 500 | 接触后保压时长 |
| 25 | 判断停止时间(ms) | int | 100 | 判定停止的采样窗口 |
| 26 | 停止速度阈值(mm/s) | double | 0.5 | 低于此速度判定停止 |
| 27 | 压力标定系数K | double | 1.0 | 压力 = K × 0x201B + B |
| 28 | 压力标定偏移B | double | 0.0 | |

### 8.3 回零参数(仅 Home 可见)

| 序号 | 名称 | 类型 | 默认值 |
|------|------|------|--------|
| 17 | 回零模式代码 | short | 0 |
| 18 | 回零高速(mm/s) | double | 50 |
| 19 | 回零低速(mm/s) | double | 10 |
| 20 | 回零加速度(mm/s²) | double | 1000 |
| 21 | 回零超时(秒) | int | 60 |

### 8.4 输出参数(只读)

| 序号 | 名称 | 类型 | 说明 |
|------|------|------|------|
| 30 | 执行结果 | bool | true=成功 |
| 31 | 实际位置(mm) | double | 硬着陆/软着陆完成时的位置 |
| 32 | 实际压力 | double | 软着陆完成时的压力(标定后) |
| 33 | 失败原因 | string | 失败时的详细描述 |
