using System;
using System.Collections.Generic;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 五轴卡端正逆解模式 + 精标解算旁路接口（ADR-TES-110）。
    /// 不扩展 <c>IMotionCard</c> 主契约，仅由具备卡端正逆解能力的板卡（ZMotion）实现。
    /// 对齐源端 <c>ZMCMotion.Z5Axes_Frame/Z5Axes_ExitFrame/Z5Axes_Reframe/ZFrameCali</c>
    /// + <c>Check5AxisStationBase.Frame/ExitFrame/FrameCal</c>。范式参照 <see cref="IFiveAxisRTCP"/>。
    /// </summary>
    /// <remarks>
    /// <b>生命周期（关键，R-F2）</b>：Frame 模式是卡端 <c>Connframe</c> 全局状态，进入后影响后续所有运动指令的坐标解算，
    /// 必须由上层 Service 显式 <see cref="ExitFrame"/> 退出（建议 try/finally 保证清理），接口本身不自动清理。
    /// 严格顺序：<c>ExitFrame</c>（清残留）→ <c>Frame(粗标)</c> → <c>FrameCal</c> → <c>ExitFrame</c>（必退）。
    ///
    /// <b>参数类型说明（实现决策）</b>：ADR 骨架签名用 <c>Coord5Axis</c>，但 <c>Coord5Axis</c> 落在
    /// <c>Luster.Module.Motion.FiveAxis</c>（引用本 DataStruct 模块），本接口落点 DataStruct 不能反向引用 FiveAxis（循环依赖）。
    /// 故参照 <see cref="IFiveAxisRTCP"/> + <see cref="FiveAxisRtcpConfig"/> 范式，定义 DataStruct 本地
    /// <see cref="FiveAxisFramePara"/> 承载 6 结构字段（对齐 ADR R-F5「占位类型」缓解）。上层精标 Service 负责
    /// <c>Coord5Axis</c>↔<see cref="FiveAxisFramePara"/> 互转；卡端实现只认本接口类型，不依赖 FiveAxis。
    /// </remarks>
    public interface IFiveAxisFrame
    {
        /// <summary>
        /// 配置坐标系对应的卡端表地址（对齐源端 <c>GetCrdProfile(crdIndex).CrdAddr</c>）。
        /// 精标 Frame/FrameCal 需写/读卡端 Table（Axis5ParaAddr、FrameCalAddr 子表），地址由现场配置决定。
        /// ⚠️ 表地址具体值待人类现场验证（ADR R-F4），软件层只定契约。
        /// </summary>
        /// <param name="crdIndex">坐标系编号（对应卡端 CrdProfile）</param>
        /// <param name="addr">该坐标系的 Frame/FrameCal 卡端表地址</param>
        bool ConfigureFrameTableAddr(int crdIndex, FiveAxisFrameTableAddr addr);

        /// <summary>
        /// 进入五轴逆解模式（对齐源端 <c>Z5Axes_Frame</c> / <c>Check5AxisStationBase.Frame:597</c>）。
        /// 卡端 Connframe 加载 Coord5Axis 结构参数，后续 LineMove 走工件坐标自动解算。
        /// 前置：实轴/虚轴处于 idle（实现内 cancel + 等待 Loaded，对齐源端 :2577-2630）。
        /// </summary>
        /// <param name="crdIndex">坐标系编号（对应卡端 CrdProfile）</param>
        /// <param name="realAxisList">实轴（关节轴）编号列表</param>
        /// <param name="virAxisList">虚轴（工件坐标轴）编号列表</param>
        /// <param name="para">五轴结构参数（粗标结果，进逆解模式用）</param>
        bool Frame(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, FiveAxisFramePara para);

        /// <summary>
        /// 进入五轴正解模式（对齐源端 <c>Z5Axes_Reframe</c> / <c>reFrame:577</c>）。本期留签名，后续正解 Issue 实现。
        /// </summary>
        bool Reframe(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, FiveAxisFramePara para);

        /// <summary>
        /// 退出五轴正逆解模式（对齐源端 <c>Z5Axes_ExitFrame</c> / <c>ExitFrame:616</c>）。
        /// 含停止实轴/虚轴 + 等待 CrdDone 逻辑（对齐源端 :616-637，超时 FrameTimeOut）。
        /// </summary>
        /// <param name="realAxisList">实轴编号列表</param>
        /// <param name="virAxisList">虚轴编号列表</param>
        bool ExitFrame(IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList);

        /// <summary>
        /// 卡端精标解算（对齐源端 <c>ZFrameCali:2794</c> / <c>Check5AxisStationBase.FrameCal:651</c>）。
        /// 输入工件位姿采样点（已转 5 轴脉冲数组），卡端 <c>FRAME_CAL</c> 固件算法解算，
        /// 输出 A 轴机械零点 <paramref name="aZero"/> + 精标结构参数 <paramref name="para"/>。
        /// 前置：已 <see cref="Frame"/> 进逆解模式（源端 frameCal():1328-1334 严格顺序）。
        /// </summary>
        /// <param name="crdIndex">坐标系编号</param>
        /// <param name="realAxisList">实轴编号列表（源端 <c>Take(3)</c> 取前 3 轴，用于 BASE 指令）</param>
        /// <param name="axisPosi">采样点列表，每点为 5 轴脉冲数组（源端 <c>MotorPosiHelper.To5AxisLis</c> 转换结果）</param>
        /// <param name="aZero">输出：A 轴机械零点（源端读 OutZeroTb[3]）</param>
        /// <param name="para">输出：精标五轴结构参数（源端读 OutRobotTb 16 float）</param>
        bool FrameCal(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<double[]> axisPosi,
                      out double aZero, out FiveAxisFramePara para);
    }

    /// <summary>
    /// 五轴结构参数（DataStruct 本地承载，对应 <c>Coord5Axis</c> 6 结构字段）。
    /// 卡端 <c>ZAux_Direct_SetTable</c>/<c>GetTable</c> 用 float[26]/float[16] 往返，本类提供命名字段映射（对齐源端 robotPara 布局）。
    /// 不依赖 FiveAxis 的 <c>Coord5Axis</c>/<c>PositionXYZ</c>，规避 DataStruct↔FiveAxis 循环依赖。
    /// </summary>
    [Serializable]
    public class FiveAxisFramePara
    {
        /// <summary>A 轴旋转中心到原点的偏移</summary>
        public double ACenterX { get; set; }
        /// <summary>A 轴旋转中心到原点的偏移</summary>
        public double ACenterY { get; set; }
        /// <summary>A 轴旋转中心到原点的偏移</summary>
        public double ACenterZ { get; set; }
        /// <summary>A 轴旋转中线的方向矢量</summary>
        public double ADirX { get; set; }
        /// <summary>A 轴旋转中线的方向矢量</summary>
        public double ADirY { get; set; }
        /// <summary>A 轴旋转中线的方向矢量</summary>
        public double ADirZ { get; set; }
        /// <summary>A 一圈脉冲数</summary>
        public double ACirPulses { get; set; }

        /// <summary>C 轴旋转中心到原点的偏移</summary>
        public double CCenterX { get; set; }
        /// <summary>C 轴旋转中心到原点的偏移</summary>
        public double CCenterY { get; set; }
        /// <summary>C 轴旋转中心到原点的偏移</summary>
        public double CCenterZ { get; set; }
        /// <summary>C 轴旋转中线的方向矢量</summary>
        public double CDirX { get; set; }
        /// <summary>C 轴旋转中线的方向矢量</summary>
        public double CDirY { get; set; }
        /// <summary>C 轴旋转中线的方向矢量</summary>
        public double CDirZ { get; set; }
        /// <summary>C 一圈脉冲数</summary>
        public double CCirPulses { get; set; }
    }

    /// <summary>
    /// 五轴 Frame/FrameCal 卡端表地址配置（对应源端 <c>CrdProfile.CrdAddr</c>）。
    /// ⚠️ 具体地址值待人类现场验证（ADR R-F4），软件层只定契约。ZMotion 实现按 crdIndex 查表使用。
    /// </summary>
    [Serializable]
    public class FiveAxisFrameTableAddr
    {
        /// <summary>五轴结构参数表起始地址（Frame 写 26-float robotPara，源端 Axis5ParaAddr）</summary>
        public int Axis5ParaAddr { get; set; }
        /// <summary>FrameCal 输入采样点表起始地址（源端 FrameCalAddr.InAxisPosiTb）</summary>
        public int InAxisPosiTb { get; set; }
        /// <summary>FrameCal 输入扩展表地址（源端 FrameCalAddr.InExtendTb）</summary>
        public int InExtendTb { get; set; }
        /// <summary>FrameCal 输出 aZero 表地址（源端 FrameCalAddr.OutZeroTb，读 vs[3]）</summary>
        public int OutZeroTb { get; set; }
        /// <summary>FrameCal 输出结构参数表地址（源端 FrameCalAddr.OutRobotTb，读 16 float）</summary>
        public int OutRobotTb { get; set; }
    }
}
