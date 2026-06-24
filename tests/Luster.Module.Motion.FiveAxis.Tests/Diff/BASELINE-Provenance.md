# 标定 diff 回归基线 — 来源与场景说明（TES-118）

本目录的 `cali_*_baseline.csv` 是 **源端 SP-2025140 标定算法语义** 的独立手算基线，
供 `Luster.Tools.DiffRegression --mode cali` 与迁移端 `FiveAxisCalibrationService` 实际输出做 diff 回归。

## 来源（非编造）

知识库 `F:\Knowledge` 与源端工作副本 `F:/SVN-git-mirror/SP-2025140` 内 **无** 已落盘的标定结果基线文件
（源端标定算法耦合在 WinForm `Form5Cali.cs` UI 事件里，无可离线运行产出的结果文件）。
故基线按 Issue「范围冻结」的「知识库无的从源端离线采集」要求，采用 **从源端算法语义独立手算** 的方式采集：

- 源端算法语义锚点（核实于知识库 `需求规格.md` 与迁移端 `FiveAxisCalibrationService` 源端对照注释）：
  - 粗标 `btnRoughCalculate`（Form5Cali.cs:831）→ `AngleHelper.CalculateRoateCenter` 三点求旋转中心
  - 激光标定 `laserCaliApply`（Form5Cali.cs:281）→ LinearConverter 两点定标 y=kx+b
  - 工件原点 `btnWorkCalculateFromTeach`（Form5Cali.cs:1757）→ `CalculateOriginOffset`
- 基线值由 **初等旋转几何 / 线性拟合 / atan2** 独立手算得出，不调用迁移端任何代码（与 actual 独立）。

## 场景：粗标 cali_rough_baseline.csv

输入示教点（与 actual 用同一组输入，见 `CalibrationDiffActualTests.cs`）：
- FirstPosi=(0,0,0,RX=0,RZ=0)，Rx=90°，Rz=90°
- ResultFirstPosi=(50,30,5)，ResultRxPosi=(0,10,25)，ResultRzPosi=(10,30,0)
- mrxPulses=360000，mrzPulses=720000

手算（A 轴 YZ 平面绕中心 (10,5) 转 90°：(30,5)→(10,25)；C 轴 XY 平面绕中心 (30,10) 转 90°：(50,30)→(10,30)）：
- ACenter=(0,10,5)，ADir=(1,0,0)，ACirPulses=360000
- CCenter=(30,10,0)，CDir=(0,0,1)，CCirPulses=720000

## 场景：激光 cali_laser_baseline.csv

输入：laser1=1,z1=100，laser2=5,z2=500，laserStandard=12.34，LaserPosi=(10,20,30)，CameraPosi=(12,22,31)。
手算：k=(500-100)/(5-1)=100，b=100-100×1=0；CameraOffset=CameraPosi-LaserPosi=(2,2,1)。

## 场景：工件原点 cali_origin_baseline.csv

输入：OriginPosi=(1,2,3)，LongSidePosi=(4,6,9)，OrgPosiType=OriginPosi。
手算：Trans=(1,2,3, atan2(6-2,4-1))=(1,2,3, atan2(4,3))=(1,2,3,0.9272952180016122)。

## 精标（AccurateCalibrate）—— carve-out

精标算法在卡端 ZMotion 固件（FrameCal），PC 侧无可剥离 C#；同卡同固件输入相同采样点输出一致，
diff≤1e-6 自然满足，**真机精标 diff ⚠️ 待人类现场验证（R-F4）**，不在此虚拟侧 diff。
