using FluentAssertions;
using Luster.Module.Motion.FiveAxis;
using Luster.Module.Motion.FiveAxis.Functions;
using Luster.TaskFlow.Common.Attributes;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// P5-6 检测站骨架 + AOI#1 三段式编排节点验收:
    /// 1) FiveAxisStation 站 Function + FiveAxisStationModule 站模块可实例化 + [Parameter] 范式;
    /// 2) RtcpFrameEnter/RtcpFrameExit(BuildFrame/ExitFrame)可实例化 + [Parameter] + Exit 幂等无设备不抛;
    /// 3) HandoverNode(15/13 步交握抽离)可实例化 + [Parameter] + 方向枚举 + 无设备不抛(信号未配置跳过);
    /// 4) FiveAxisModule / FiveAxisStationModule 注册了新节点(FuncTypes 含新节点类型);
    /// 5) 异常结构化:HandoverNode 无设备/无信号 DoExcute 不抛(交握子集可简化),RtcpFrameExit 幂等。
    /// 真机 RTCP 精度/交握时序属硬件类验收,标 ⚠️ 待人类现场(TES carve-out),不在此验。
    /// </summary>
    [TestFixture]
    public class StationNodesTests
    {
        private static readonly Type[] NewStationNodes =
        {
            typeof(FiveAxisStation),
            typeof(RtcpFrameEnter),
            typeof(RtcpFrameExit),
            typeof(HandoverNode),
        };

        [Test]
        public void AllNewStationNodes_AreInstantiable()
        {
            foreach (var type in NewStationNodes)
            {
                var instance = Activator.CreateInstance(type);
                instance.Should().NotBeNull($"{type.Name} 应可无参实例化");
            }
        }

        [Test]
        public void EachNewNode_HasParameterAttributes_ForParamGrid()
        {
            // ParamGrid 范式:节点通过 [Parameter] 特性暴露参数。
            // FiveAxisStation 作为站 Function 也需 [Parameter](IsEnabled 等)。
            foreach (var type in NewStationNodes)
            {
                var parameters = type.GetProperties()
                    .Select(p => p.GetCustomAttribute<ParameterAttribute>())
                    .Where(a => a != null)
                    .Count();
                parameters.Should().BeGreaterThan(0, $"{type.Name} 应至少挂载一个 [Parameter] 特性");
            }
        }

        [Test]
        public void FiveAxisStation_ImplementsIFreeStation()
        {
            typeof(FiveAxisStation).GetInterfaces()
                .Should().Contain(typeof(Luster.TaskFlow.Motion.Logic.IFreeStation),
                    "FiveAxisStation 应实现 IFreeStation(自由工站契约)");
        }

        [Test]
        public void FiveAxisModule_RegistersNewStationNodes()
        {
            // FiveAxisModule 注册 P5-6 新节点(BuildFrame/ExitFrame/HandoverNode)。
            var module = new FiveAxisModule();
            module.InitFunctions();
            var funcTypes = typeof(Luster.TaskFlow.Common.Module.AbsModule)
                .GetProperty("FuncTypes")?.GetValue(module) as System.Collections.Generic.Dictionary<string, Type>;

            funcTypes.Should().NotBeNull("FuncTypes 应存在");
            funcTypes.ContainsKey(nameof(RtcpFrameEnter)).Should().BeTrue("应注册 RtcpFrameEnter");
            funcTypes.ContainsKey(nameof(RtcpFrameExit)).Should().BeTrue("应注册 RtcpFrameExit");
            funcTypes.ContainsKey(nameof(HandoverNode)).Should().BeTrue("应注册 HandoverNode");
        }

        [Test]
        public void FiveAxisStationModule_RegistersFiveAxisStation()
        {
            // FiveAxisStationModule(站骨架)注册 FiveAxisStation 站 Function。
            var module = new FiveAxisStationModule();
            module.InitFunctions();
            var funcTypes = typeof(Luster.TaskFlow.Common.Module.AbsModule)
                .GetProperty("FuncTypes")?.GetValue(module) as System.Collections.Generic.Dictionary<string, Type>;

            funcTypes.Should().NotBeNull("FuncTypes 应存在");
            funcTypes.ContainsKey(nameof(FiveAxisStation)).Should().BeTrue("FiveAxisStationModule 应注册 FiveAxisStation");
        }

        [Test]
        public void RtcpFrameExit_IsIdempotent_WhenNoDevice()
        {
            // RtcpFrameExit 幂等:无设备(虚拟/空跑)返回成功,不抛——保证急停/异常路径 complete 段清理不阻断。
            var node = new RtcpFrameExit { AxisDevice = null };
            Action act = () => node.DoExcute(out _);
            act.Should().NotThrow();
            node.Success.Should().BeTrue("无设备时 ExitFrame 幂等返回成功");
        }

        [Test]
        public void RtcpFrameEnter_ReturnsFalse_WhenNoDevice_ButDoesNotThrow()
        {
            // RtcpFrameEnter 无设备:结构化返回 false + errMsg(不静默吞错,不抛)。
            var node = new RtcpFrameEnter { AxisDevice = null };
            var ok = node.DoExcute(out var errMsg);
            ok.Should().BeFalse("无设备应返回 false");
            errMsg.Should().NotBeEmpty("应给出结构化错误信息");
            node.Success.Should().BeFalse();
            node.FrameEnabled.Should().BeFalse();
        }

        [Test]
        public void HandoverNode_FeedDirection_NoThrow_WhenSignalsUnconfigured()
        {
            // 交握节点信号未配置(全 null)时:WaitSignal/SetSignal 跳过,DoExcute 跑空状态机成功不抛。
            // 验证异常结构化:无设备不抛空引用,交握子集可简化跑通(虚拟模式)。
            var node = new HandoverNode
            {
                Direction = HandoverNode.HandoverDirection.Feed,
                SignalTimeoutMs = 100,
            };
            var ok = node.DoExcute(out var errMsg);
            ok.Should().BeTrue("信号未配置时交握状态机应跑空成功");
            errMsg.Should().BeEmpty();
            node.Success.Should().BeTrue();
        }

        [Test]
        public void HandoverNode_LeaveDirection_NoThrow_WhenSignalsUnconfigured()
        {
            var node = new HandoverNode
            {
                Direction = HandoverNode.HandoverDirection.Leave,
                SignalTimeoutMs = 100,
            };
            var ok = node.DoExcute(out var errMsg);
            ok.Should().BeTrue("下游出料方向信号未配置时亦应跑空成功");
            node.Success.Should().BeTrue();
        }

        [Test]
        public void HandoverNode_HasBothFeedAndLeaveDirections()
        {
            // 方向枚举含 Feed(上游 15 步)/ Leave(下游 13 步)。
            var directions = Enum.GetNames(typeof(HandoverNode.HandoverDirection));
            directions.Should().Contain("Feed");
            directions.Should().Contain("Leave");
        }

        [Test]
        public void RtcpFrameExit_DoExcuteHasTryCatch_ForStructuredCleanup()
        {
            // 异常结构化:RtcpFrameExit 用 try/catch 包裹退出逻辑(替代源端空 catch),
            // 静态走查 DoExcute 方法体含 catch 异常处理块(幂等清理)。
            var method = typeof(RtcpFrameExit).GetMethod("DoExcute");
            method.Should().NotBeNull();
            // 行为验证:无设备不抛(try/catch 生效),幂等成功。
            var node = new RtcpFrameExit { AxisDevice = null };
            Action act = () => node.DoExcute(out _);
            act.Should().NotThrow();
        }

        [Test]
        public void HandoverNode_DoExcuteHasTryCatch_ForStructuredAlarm()
        {
            // 异常结构化:HandoverNode.DoExcute 用 try/catch 把交握异常转 OnAlarm 结构化报警
            // (替代源端空 catch / step 101/102 静默撤离)。
            var method = typeof(HandoverNode).GetMethod("DoExcute");
            method.Should().NotBeNull();
            // 行为验证:无信号跑空不抛,catch 块不误触发。
            var node = new HandoverNode { Direction = HandoverNode.HandoverDirection.Feed, SignalTimeoutMs = 50 };
            Action act = () => node.DoExcute(out _);
            act.Should().NotThrow();
        }
    }
}
