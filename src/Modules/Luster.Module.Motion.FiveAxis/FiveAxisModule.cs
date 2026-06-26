using Luster.Module.Motion.FiveAxis.Functions;
using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.FiveAxis
{
    /// <summary>
    /// 五轴运动学模块(P5-1 骨架 + P5-2 正逆解 + P5-3 连续插补/锁存节点)。
    /// 提供五轴能力接入 lmv 的工程骨架 + MathNet.Numerics / MathNetExtend 依赖底座,
    /// 供 P5-2(Coord5Axis 正逆解)、P5-3(连续插补接口)在此挂载 MotionFunction 算子节点。
    /// 模块 DLL 经 CopyToMotionsFolder 落 Motions\,由 ModuleFactory 反射自动加载(Shell/引擎零改动)。
    /// </summary>
    public class FiveAxisModule : MotionModule
    {
        public override void InitFunctions()
        {
            // P5-2:Coord5Axis 五轴正逆解算子节点(经 XML recipe 可编排)
            AddFunction<FiveAxisKinematicsNode>();

            // P5-3:连续插补 + 高速锁存旁路接口节点(10 个,经 XML recipe 可编排)
            //   连续插补(IFiveAxisContiInterp):CrdConti / CrdContiSmooth / CrdContiRemainCheck / CrdContiWaitDone
            //   高速锁存(IFiveAxisLatch):LatchStart / LatchWait / LatchRead / LatchClear
            //   锁存偏移/数据处理:LatchOffsetCalc / LatchDataProcess
            //   M-13 finally 关闭契约:CrdConti(Stop/Close)、LatchWait(ClearLatch) 用 try/finally 保证清理。
            AddFunction<CrdConti>();
            AddFunction<CrdContiSmooth>();
            AddFunction<CrdContiRemainCheck>();
            AddFunction<CrdContiWaitDone>();
            AddFunction<LatchStart>();
            AddFunction<LatchWait>();
            AddFunction<LatchRead>();
            AddFunction<LatchClear>();
            AddFunction<LatchOffsetCalc>();
            AddFunction<LatchDataProcess>();
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

