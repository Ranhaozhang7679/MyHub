using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;

namespace Luster.Motion.FiveAxis.Calibration
{
    /// <summary>
    /// 五轴标定服务契约（TES-190 P2-B Service 化骨架）。
    /// 对齐 <c>FiveAxisCaliParam</c> 的三个 OUT（<c>Rough5Para</c>/<c>Accurate5Para</c>/<c>WorkOriginResult</c>），
    /// 由 <c>FiveAxisCaliParam.DoExcute</c> 经宿主模块 Ioc 服务定位调用，把结果回写 OUT。
    /// </summary>
    /// <remarks>
    /// ⚠️ 范围说明：本接口是“薄 Service 壳”。真正的标定数值求解（粗标/精标/激光/原点）仍在旧程序
    /// Form5Cali/FrameCal/ZFrameCali，不在本仓库、不在本 issue 范围。实现内部仅用 <see cref="Coord5Axis"/>
    /// 正逆解产出“可计算部分”的中间结果回写 OUT，标定求解本体留 TODO（待后续 issue 迁移）。
    /// </remarks>
    public interface IFiveAxisCalibrationService
    {
        /// <summary>
        /// 聚合标定：一次性产出粗标/精标/原点示教三个 OUT 结果。
        /// 标定求解本体待迁移，此处用 Coord5Axis 正逆解产出可验证的运动学中间结果。
        /// </summary>
        CalibrationResult Calibrate(CalibrationInput input);
    }

    /// <summary>
    /// 标定输入（由 FiveAxisCaliParam 的 [Parameter] 属性适配而来）。
    /// </summary>
    public sealed class CalibrationInput
    {
        /// <summary>五轴结构参数（源端 FiveAxisPara:Coord5Axis，节点层已解析为 Coord5Axis）</summary>
        public Coord5Axis FiveAxisPara { get; set; } = new Coord5Axis();

        /// <summary>粗标 Rx 旋转角度（度）</summary>
        public double RoughRx { get; set; } = 45.0;

        /// <summary>粗标 Rz 旋转角度（度）</summary>
        public double RoughRz { get; set; } = 45.0;

        /// <summary>工具姿态（源端 Tool2Work:CoordTransForm，用于原点示教 PoseTool2Work）</summary>
        public PositionXYZRxRyRz ToolPose { get; set; } = new PositionXYZRxRyRz();

        /// <summary>原点位置类型（源端 OrgPosiType）</summary>
        public int OrgPosiType { get; set; } = 0;
    }

    /// <summary>
    /// 标定结果（对齐 FiveAxisCaliParam 的三个 OUT 字段，均为序列化字符串供 ParamGrid 显示/recipe 持久化）。
    /// </summary>
    public sealed class CalibrationResult
    {
        /// <summary>五轴粗略参数结果（OUT: Rough5Para）</summary>
        public string Rough5Para { get; set; } = "";

        /// <summary>精确五轴参数结果（OUT: Accurate5Para）</summary>
        public string Accurate5Para { get; set; } = "";

        /// <summary>工件坐标系结果（OUT: WorkOriginResult）</summary>
        public string WorkOriginResult { get; set; } = "";
    }
}
