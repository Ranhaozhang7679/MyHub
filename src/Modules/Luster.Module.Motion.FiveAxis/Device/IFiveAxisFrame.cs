using Luster.Motion.FiveAxis.Kinematics;
using System.Collections.Generic;

namespace Luster.Motion.FiveAxis.Device
{
    /// <summary>
    /// 五轴卡端正逆解模式 + 精标解算旁路接口(ADR-TES-110)。
    /// 不扩展 <c>IMotionCard</c> 主契约,仅由具备卡端正逆解能力的板卡(正运动 ZMotion)实现,保 R1 非侵入性。
    ///
    /// 对齐源端 <c>ZMCMotion.Z5Axes_Frame</c>/<c>Z5Axes_Reframe</c>/<c>Z5Axes_ExitFrame</c>/<c>ZFrameCali</c>
    /// 与 <c>Check5AxisStationBase.Frame/ExitFrame/FrameCal</c>。
    /// 范式参照 <c>IFiveAxisRTCP</c>(RTCP Path A 卡端 + 旁路接口)。
    /// </summary>
    /// <remarks>
    /// <b>Frame 逆解模式生命周期(关键,R-F2)</b>:
    /// Frame 模式是卡端 <c>Connframe</c> 全局状态,进入后影响后续所有运动指令的坐标解算,
    /// 必须由上层 Service 显式 <see cref="ExitFrame"/> 退出(建议 try/finally 保证清理),接口本身不自动清理。
    /// 严格顺序:<see cref="ExitFrame"/>(清残留) → <see cref="Frame"/>(粗标) → <see cref="FrameCal"/> → <see cref="ExitFrame"/>(必退)。
    /// <see cref="FrameCal"/> 前置:必须在 <see cref="Frame"/>(粗标 Rough5Para) 进逆解模式之后调用。
    /// </remarks>
    public interface IFiveAxisFrame
    {
        /// <summary>
        /// 进入五轴逆解模式(对齐源端 <c>Z5Axes_Frame</c> / <c>Check5AxisStationBase.Frame:606</c>)。
        /// 卡端 <c>Connframe</c> 加载 <see cref="Coord5Axis"/> 结构参数,后续 LineMove 走工件坐标自动解算。
        /// 前置:实轴/虚轴处于 idle(实现内 cancel + 等待 Loaded,对齐源端 <c>ZMCMotion.cs:2826-2881</c>)。
        /// </summary>
        /// <param name="crdIndex">坐标系编号(对应卡端 CrdProfile)</param>
        /// <param name="realAxisList">实轴(关节轴)编号列表</param>
        /// <param name="virAxisList">虚轴(工件坐标轴)编号列表</param>
        /// <param name="para">五轴结构参数(粗标结果,进逆解模式用)</param>
        bool Frame(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, Coord5Axis para);

        /// <summary>
        /// 进入五轴正解模式(对齐源端 <c>Z5Axes_Reframe</c> / <c>reFrame:577</c>)。
        /// <b>本期只留签名</b>,后续正解 Issue 实现。
        /// </summary>
        bool Reframe(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, Coord5Axis para);

        /// <summary>
        /// 退出五轴正逆解模式(对齐源端 <c>Z5Axes_ExitFrame</c> / <c>ExitFrame:625</c>)。
        /// 含多轴运动停止 + 单轴 cancel 逻辑(对齐源端 <c>ZMCMotion.cs:3227-3240</c>)。
        /// </summary>
        bool ExitFrame(IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList);

        /// <summary>
        /// 卡端精标解算(对齐源端 <c>ZFrameCali:3252</c> / <c>Check5AxisStationBase.FrameCal:660</c>)。
        /// 输入工件位姿采样点(已转 5 轴脉冲数组),卡端 <c>FRAME_CAL</c> 固件算法解算,
        /// 输出 A 轴机械零点 <paramref name="aZero"/> + 精标 <see cref="Coord5Axis"/> 结构参数。
        /// 前置:已 <see cref="Frame"/>(粗标参数) 进入逆解模式。
        /// </summary>
        /// <param name="crdIndex">坐标系编号</param>
        /// <param name="realAxisList">实轴编号列表(源端 <c>FrameCal:670</c> <c>Take(3)</c> 取前 3 轴 X/Y/Z)</param>
        /// <param name="axisPosi">采样点列表,每点为 5 轴脉冲数组(源端 <c>MotorPosiHelper.To5AxisLis</c> 转换结果)</param>
        /// <param name="aZero">输出:A 轴机械零点(源端读 <c>OutZeroTb[3]</c>)</param>
        /// <param name="para">输出:精标五轴结构参数(源端读 <c>OutRobotTb</c> 16 float)</param>
        bool FrameCal(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<double[]> axisPosi,
                      out double aZero, out Coord5Axis para);
    }

    /// <summary>
    /// 卡端精标(FrameCal)表地址(ADR-TES-110 R-F4)。
    /// 对齐源端 <c>CrdProfile.CrdAddr.FrameCalAddr</c>(<c>ZMCMotion.cs:3269/3272/3280/3285</c> 引用):
    /// 卡端 <c>FRAME_CAL</c> 固件算法在 Table 区读写——写入采样点,读回 aZero 与 16 float 结构参数。
    /// </summary>
    /// <remarks>
    /// <b>⚠️ R-F4 真机表地址配置待人类现场验证</b>:本数据结构只定义地址字段语义,具体数值由
    /// 运控平台 CrdProfile 配置(源端在站 BaseProfile/卡端 CrdAddr 里预置),本期接口不绑定具体值。
    /// 迁移后表地址需与源端逐项核对,属 R-F4 现场验证范畴。
    /// </remarks>
    public class FiveAxisFrameAddr
    {
        /// <summary>输入:采样点脉冲表起始地址(源端 <c>InAxisPosiTb</c> <c>SetTable</c> 写入)</summary>
        public int InAxisPosiTb { get; set; }

        /// <summary>输入:扩展参数表起始地址(源端 <c>InExtendTb</c>,<c>FRAME_CAL</c> 命令第 4 参数)</summary>
        public int InExtendTb { get; set; }

        /// <summary>输出:A 轴零点表起始地址(源端 <c>OutZeroTb</c> <c>GetTable</c> 读 5 float,取 [3])</summary>
        public int OutZeroTb { get; set; }

        /// <summary>输出:精标结构参数表起始地址(源端 <c>OutRobotTb</c> <c>GetTable</c> 读 16 float)</summary>
        public int OutRobotTb { get; set; }
    }
}
