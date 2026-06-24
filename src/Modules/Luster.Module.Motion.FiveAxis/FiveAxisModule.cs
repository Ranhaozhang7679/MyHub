using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.FiveAxis
{
    /// <summary>
    /// 五轴运动学模块(P5-1 骨架)。
    /// 提供五轴能力接入 lmv 的工程骨架 + MathNet.Numerics / MathNetExtend 依赖底座,
    /// 供 P5-2(Coord5Axis 正逆解)、P5-3(连续插补接口)在此挂载 MotionFunction 算子节点。
    /// 模块 DLL 经 CopyToMotionsFolder 落 Motions\,由 ModuleFactory 反射自动加载(Shell/引擎零改动)。
    /// </summary>
    public class FiveAxisModule : MotionModule
    {
        public override void InitFunctions()
        {
            // 五轴算子节点由后续 Issue 挂载:
            //   P5-2:AddFunction<FiveAxisKinematics>()(Coord5Axis 正逆解 + RTCP)
            //   P5-3:AddFunction<连续插补节点>()
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
