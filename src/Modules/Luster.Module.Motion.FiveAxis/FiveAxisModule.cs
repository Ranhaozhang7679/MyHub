using Luster.Module.Motion.FiveAxis.Functions;
using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.FiveAxis
{
    /// <summary>
    /// 五轴运动学模块(P5-1 骨架 + P5-2 Coord5Axis 正逆解 + P5-3 连续插补/锁存节点 + P5-6 检测站编排节点)。
    /// 提供五轴能力接入 lmv 的工程骨架 + MathNet.Numerics / MathNetExtend 依赖底座,
    /// 供 P5-2(Coord5Axis 正逆解)、P5-3(连续插补接口)、P5-6(检测站编排)在此挂载 MotionFunction 算子节点。
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

            // P5-6:AOI#1 三段式编排节点(经 XML recipe 可编排,见 Recipes/AOI1_ThreeSegment.recipe.xml)
            //   RtcpFrameEnter/Exit:卡侧 RTCP 帧建立/退出(源端 BuildFrame/ExitFrame),Exit 幂等保证急停路径清理。
            //   HandoverNode:站间交握状态机(源端上游 15 步/下游 13 步信号交握抽离),异常转结构化报警。
            AddFunction<RtcpFrameEnter>();
            AddFunction<RtcpFrameExit>();
            AddFunction<HandoverNode>();
        }
    }

    /// <summary>
    /// 五轴检测站模块(P2-D 站骨架,P5-6):<see cref="MotionModule"/> 派生,注册
    /// <see cref="FiveAxisStation"/> 站 Function(IFreeStation)。recipe 中本模块实例作为
    /// AOI#1 检测站节点,其 Children 挂三段式 Group 链(prepare/work/complete)。
    /// 与 <see cref="FiveAxisModule"/> 同分区(System="FiveAxis"),Creator 反射暴露到工具箱。
    /// </summary>
    public class FiveAxisStationModule : MotionModule
    {
        public override void InitFunctions()
        {
            // 五轴检测站 Function(IFreeStation):DoExcute 跑 Children[0] 三段式 Group 链。
            AddFunction<FiveAxisStation>();
        }
    }

    /// <summary>
    /// 五轴模块创建器:独立 System="FiveAxis",与既有 HoloMotion 模块分区隔离。
    /// FiveAxisModule(算子节点)与 FiveAxisStationModule(检测站)共用此分区。
    /// </summary>
    public class FiveAxisModuleCreator : MotionModuleCreator<FiveAxisModule>
    {
        public override string System => "FiveAxis";
    }

    /// <summary>
    /// 五轴检测站模块创建器:与 <see cref="FiveAxisModuleCreator"/> 同分区(System="FiveAxis")。
    /// </summary>
    public class FiveAxisStationModuleCreator : MotionModuleCreator<FiveAxisStationModule>
    {
        public override string System => "FiveAxis";
    }
}

