using Luster.Module.Motion.FiveAxis.Functions;
using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.FiveAxis
{
    /// <summary>
    /// 五轴运动学模块。
    /// 提供五轴能力接入 lmv 的工程骨架 + MathNet.Numerics 依赖底座 + Coord5Axis 运动学核心(Luster.Motion.FiveAxis.*),
    /// 供 P5-2(Coord5Axis 正逆解)、P5-3(连续插补接口)在此挂载 MotionFunction 算子节点。
    /// 模块 DLL 经 CopyToMotionsFolder 落 Motions\,由 ModuleFactory 反射自动加载(Shell/引擎零改动)。
    /// </summary>
    public class FiveAxisModule : MotionModule
    {
        public override void InitFunctions()
        {
            // P5-2:Coord5Axis 五轴正逆解算子节点(经 XML recipe 可编排)
            AddFunction<FiveAxisKinematicsNode>();
            // P5-3:连续插补节点(待 P5-3 挂载)

            // P5-4:单点激光测距 + 激光 Z 单点标定(TES-99)
            // LaserMeasure 产出 (激光读数, 当前Z),LaserZCalibrate 两点定标调 FiveAxisCalibrationService.LaserCalibrate
            AddFunction<LaserMeasureNode>();
            AddFunction<LaserZCalibrateNode>();
        }
    }

    /// <summary>
    /// 五轴模块创建器:独立 System="FiveAxis",与既有 HoloMotion 模块分区隔离。
    /// </summary>
    public class FiveAxisModuleCreator : MotionModuleCreator<FiveAxisModule>
    {
        public override string System => "FiveAxis";
    }
}
