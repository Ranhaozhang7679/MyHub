using FluentAssertions;
using Luster.Motion.DataStruct.DataModels;     // VIO / VDevice
using Luster.Motion.DataStruct.Enums;           // IOBehavior / DeviceMode / HomeMode / AxisPML
using Luster.Module.Motion.FiveAxis;            // FiveAxisModule
using Luster.Module.Motion.FiveAxis.Functions;  // HandoverNode / RtcpFrameEnter / RtcpFrameExit / CrdConti
using Luster.SimDevice.Engine;                  // DeviceEngine
using Luster.SimDevice.MotionCard.ZMotion;      // ZMotionMotionCard
using Luster.TaskFlow.Common.Enums;             // RunStatus
using Luster.TaskFlow.Motion;                    // IMotionModule
using Luster.TaskFlow.Motion.Modules;           // MotionRunEngine
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// P9-D 回归基线,覆盖五类核心行为缺口(虚拟模式、确定性、无硬件):
    /// 1) 工站状态机(MotionRunEngine 驱动单节点链 → Success;IsBreak=true 早返不运行);
    /// 2) 模式切换(DeviceMode Real→Virtual 属性即时反映;Virtual 模式模块跑绿);
    /// 3) 关键 IO/轴动作(ZMotion 仿真卡 SetDigitalOut/GetDigitalIn 独立字典、ServOn/Move/Home/SetAnalogOut 不抛);
    /// 4) 握手信号(HandoverNode Feed/Leave 空信号子集跑通;配置输入信号永不到达 → 超时返回 false,不挂起);
    /// 5) 异常超时(RtcpFrameExit 幂等、RtcpFrameEnter 无设备返 false、CrdConti 无设备返 false,均不抛)。
    ///
    /// 虚拟后端 = DeviceEngine{DeviceMode=Virtual} + ZMotionMotionCard{SimulationMode=true},不触达硬件 SDK。
    /// 真机 RTCP 精度/交握时序属硬件类验收,⚠️ 待人类现场(TES carve-out),不在此验。
    /// 辅助方法 BuildVirtualBackend/MakeModule/ParameterSetter 镜像自 AOI1VirtualEndToEndTests,精简为回归基线所需。
    /// </summary>
    [TestFixture]
    public class RegressionBaselineTests
    {
        #region StationStateMachine —— 工站状态机(MotionRunEngine 驱动 + IsBreak 早返)

        /// <summary>缺口:虚拟模式下 MotionRunEngine 驱动单节点 HandoverNode 链应跑绿(ok=true、Status=Success、无错误信息)。</summary>
        [Test]
        [Category("Regression")]
        [Category("StationStateMachine")]
        public void MotionRunEngine_VirtualRun_SingleNodeChain_SetsSuccessStatus()
        {
            BuildVirtualBackend(out var engine);
            var parent = new FiveAxisModule();
            parent.SetFunction(nameof(RtcpFrameExit));

            var module = MakeModule(nameof(HandoverNode), engine, parent, s =>
            {
                s.Set("Direction", HandoverNode.HandoverDirection.Feed);
                s.Set("SignalTimeoutMs", 200);
            });

            var runEngine = new MotionRunEngine();
            var ok = false;
            runEngine.Run(module, ref ok);

            ok.Should().BeTrue($"虚拟模式单节点链应跑绿,失败信息:{runEngine.ErrorMessage}");
            module.Status.Should().Be(RunStatus.Success, "单节点 HandoverNode 跑通后 Status 应为 Success");
            runEngine.ErrorMessage.Should().BeEmpty("跑绿不应有错误信息");
        }

        /// <summary>缺口:IsBreak=true 时 MotionRunEngine.Run 早返不运行(ok 保持 false、Status 非 Success)。</summary>
        [Test]
        [Category("Regression")]
        [Category("StationStateMachine")]
        public void MotionRunEngine_IsBreakTrue_EarlyReturn_DoesNotRun()
        {
            BuildVirtualBackend(out var engine);
            var parent = new FiveAxisModule();
            parent.SetFunction(nameof(RtcpFrameExit));

            var module = MakeModule(nameof(HandoverNode), engine, parent, s =>
            {
                s.Set("Direction", HandoverNode.HandoverDirection.Feed);
                s.Set("SignalTimeoutMs", 200);
            });
            module.IsBreak = true;   // 早返开关:Run 检测到 IsBreak 立即返回,不执行 DoExcute

            var runEngine = new MotionRunEngine();
            var ok = false;
            runEngine.Run(module, ref ok);

            ok.Should().BeFalse("IsBreak=true 时 Run 应早返,不应把 ok 置真");
            module.Status.Should().NotBe(RunStatus.Success, "早返路径不应将 Status 置为 Success");
        }

        #endregion

        #region ModeSwitch —— 模式切换(DeviceMode 属性反映 + Virtual 跑绿)

        /// <summary>缺口:DeviceMode Real→Virtual 切换应即时反映到 DeviceEngine.DeviceMode 属性。</summary>
        [Test]
        [Category("Regression")]
        [Category("ModeSwitch")]
        public void DeviceEngine_SwitchRealToVirtual_ReflectedInProperty()
        {
            var engine = new DeviceEngine { DeviceMode = DeviceMode.Real };
            engine.DeviceMode.Should().Be(DeviceMode.Real, "初始应为 Real");

            engine.DeviceMode = DeviceMode.Virtual;

            engine.DeviceMode.Should().Be(DeviceMode.Virtual, "切换后 DeviceMode 属性应反映为 Virtual");
        }

        /// <summary>缺口:DeviceMode.Virtual 下经 MotionRunEngine 驱动 HandoverNode 应跑绿(证明虚拟模式可无硬件跑通)。</summary>
        [Test]
        [Category("Regression")]
        [Category("ModeSwitch")]
        public void DeviceEngine_VirtualMode_ModuleRunGreen()
        {
            BuildVirtualBackend(out var engine);
            engine.DeviceMode.Should().Be(DeviceMode.Virtual, "虚拟后端应为 Virtual 模式");

            var parent = new FiveAxisModule();
            parent.SetFunction(nameof(RtcpFrameExit));
            var module = MakeModule(nameof(HandoverNode), engine, parent, s =>
            {
                s.Set("Direction", HandoverNode.HandoverDirection.Feed);
                s.Set("SignalTimeoutMs", 200);
            });

            var runEngine = new MotionRunEngine();
            var ok = false;
            runEngine.Run(module, ref ok);

            ok.Should().BeTrue($"Virtual 模式应使模块跑绿,失败信息:{runEngine.ErrorMessage}");
        }

        #endregion

        #region IOAxis —— 关键 IO/轴动作(ZMotion 仿真卡,无硬件)

        /// <summary>缺口:SetDigitalOut 写独立输出字典不抛;GetDigitalIn 读独立输入字典(默认 false),输出写入不应反映到输入读取。</summary>
        [Test]
        [Category("Regression")]
        [Category("IOAxis")]
        public void ZMotion_SimCard_SetDigitalOut_NoThrow_GetDigitalIn_DefaultFalse_IndependentDicts()
        {
            var card = new ZMotionMotionCard
            {
                SimulationMode = true,
                AxisCount = 5,
                DigitalInCount = 2,
                DigitalOutCount = 2,
            };
            card.InitApi();

            Action setOut = () => card.SetDigitalOut(0, true);
            setOut.Should().NotThrow("SetDigitalOut 在仿真模式应可写输出字典");

            card.GetDigitalIn(0).Should().BeFalse("输入字典与输出字典独立,输出写入不应反映到输入读取");
        }

        /// <summary>缺口:仿真卡 ServOn/Move/Home 轴动作不抛(虚拟分支短路,不触达 SDK)。</summary>
        [Test]
        [Category("Regression")]
        [Category("IOAxis")]
        public void ZMotion_SimCard_ServOn_Move_Home_NoThrow()
        {
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 5 };
            card.InitApi();

            Action act = () =>
            {
                card.ServOn(1, true);
                card.Move(0, 10, 10, 10, 1000, 0, true, 1, AxisPML.Unknown);
                card.Home(1, HomeMode.CurrentHome, 0, 0, 1000, 10, 0, AxisPML.Unknown);
            };
            act.Should().NotThrow("仿真模式 ServOn/Move/Home 应不触达硬件 SDK,不抛异常");
        }

        /// <summary>缺口:仿真卡 SetAnalogOut 模拟量输出不抛。</summary>
        [Test]
        [Category("Regression")]
        [Category("IOAxis")]
        public void ZMotion_SimCard_SetAnalogOut_NoThrow()
        {
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 5 };
            card.InitApi();

            Action act = () => card.SetAnalogOut(0, 1.5);
            act.Should().NotThrow("仿真模式 SetAnalogOut 应不抛异常");
        }

        #endregion

        #region Handshake —— 握手信号(HandoverNode 空子集 + 配置信号超时)

        /// <summary>缺口:Feed 方向信号全 null 时交握跑空子集成功(WaitSignal/SetSignal 跳过)。</summary>
        [Test]
        [Category("Regression")]
        [Category("Handshake")]
        public void HandoverNode_Feed_NullSignals_RunsEmptySubset_Success()
        {
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

        /// <summary>缺口:Leave 方向信号全 null 时交握跑空子集成功。</summary>
        [Test]
        [Category("Regression")]
        [Category("Handshake")]
        public void HandoverNode_Leave_NullSignals_RunsEmptySubset_Success()
        {
            var node = new HandoverNode
            {
                Direction = HandoverNode.HandoverDirection.Leave,
                SignalTimeoutMs = 100,
            };
            var ok = node.DoExcute(out var errMsg);

            ok.Should().BeTrue("下游出料方向信号未配置时亦应跑空成功");
            errMsg.Should().BeEmpty();
            node.Success.Should().BeTrue();
        }

        /// <summary>
        /// 缺口(关键):配置输入信号 VIO 永不到达时,HandoverNode 应在 SignalTimeoutMs 内超时返回 false(而非挂起)。
        /// 验证超时路径真实触发:Wall-clock 上限 5s 防挂起 + 下限断言确认实际等待(非瞬时异常)。
        ///
        /// 接线:DeviceEngine(Virtual)+ VIO{Behavior=Input, Value=0} 经 AddVirtual 注册(设 vio.Engine)→
        /// HandoverNode.RecReady 绑定 VDevice{DeviceID=vio.ID} → GetVDevice 按 ID 命中 vio →
        /// WaitSignal 等 vio.GetDigital()==true 永不满足(Value=0,虚拟分支 ProcessAction 读 Value 不触卡)→ WaitFunc 超时抛 DeviceTimeoutException →
        /// DoExcute 捕获转 OnAlarm(FailError) 返回 false。
        ///
        /// 注:直接调 DoExcute(非 Run),故 RecReady 直接设属性——ValidateAllIn 仅在 Run 内触发,不会同步 Parameters.Value 到属性。
        /// </summary>
        [Test]
        [Category("Regression")]
        [Category("Handshake")]
        public void HandoverNode_Feed_ConfiguredInputNeverArrives_TimesOut_ReturnsFalse()
        {
            // 虚拟后端 + 一个永不到达的输入 VIO(Value 默认 0 → GetDigital 恒 false)
            var engine = new DeviceEngine { DeviceMode = DeviceMode.Virtual };
            var vio = new VIO
            {
                ID = Guid.NewGuid(),
                Behavior = IOBehavior.Input,   // 输入行为:GetDigital 读 Value(默认 0 → false)
            };
            engine.AddVirtual(vio);            // 注册 VIO + 设 vio.Engine(虚拟分支 ProcessAction 依赖 Engine.DeviceMode)

            // 经 MakeModule 接线:SetFunction 自动 wire MyOwner=module;DeviceEngine/BrokenOff 为 WaitFunc 必备
            var parent = new FiveAxisModule();
            parent.SetFunction(nameof(RtcpFrameExit));
            var module = MakeModule(nameof(HandoverNode), engine, parent);
            var node = (HandoverNode)module.TaskFunction;
            node.Direction = HandoverNode.HandoverDirection.Feed;
            node.SignalTimeoutMs = 200;        // 短超时,快速验证超时路径
            node.RecReady = new VDevice { DeviceID = vio.ID, Name = "RecReady" };  // 绑定永不到达的输入信号

            // Wall-clock 守卫:超时路径应在 ~200ms 返回;若未触发(挂起)则 5s 后 FAIL 告警,不让测试挂死 runner
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string errMsg = null;
            bool ok = false;
            var task = Task.Run(() => ok = node.DoExcute(out errMsg));
            if (!task.Wait(5000))
            {
                Assert.Fail("超时路径未触发,DoExcute 挂起超过 5s(SignalTimeoutMs=200 应在 ~200ms 内返回 false)");
            }
            sw.Stop();

            ok.Should().BeFalse("配置的输入信号永不到达,DoExcute 应超时返回 false");
            node.Success.Should().BeFalse("超时路径 Success 应保持 false");
            errMsg.Should().NotBeEmpty("应给出结构化超时错误信息");
            sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(100,
                "耗时应接近 SignalTimeoutMs(200ms),以确认真实等待超时而非瞬时异常");
        }

        #endregion

        #region Timeout —— 异常超时(RtcpFrameExit 幂等 / RtcpFrameEnter / CrdConti 无设备)

        /// <summary>缺口:RtcpFrameExit 无设备幂等——连续两次 DoExcute 均返回 true、不抛(急停/异常路径 complete 段清理不阻断)。</summary>
        [Test]
        [Category("Regression")]
        [Category("Timeout")]
        public void RtcpFrameExit_Idempotent_NoDevice_ReturnsTrueTwice()
        {
            var node = new RtcpFrameExit { AxisDevice = null };
            bool ok1 = false, ok2 = false;
            Action act = () =>
            {
                ok1 = node.DoExcute(out _);
                ok2 = node.DoExcute(out _);
            };
            act.Should().NotThrow("RtcpFrameExit 无设备幂等退出不应抛异常");
            ok1.Should().BeTrue("首次调用应返回 true");
            ok2.Should().BeTrue("二次调用应幂等返回 true");
        }

        /// <summary>缺口:RtcpFrameEnter 无设备结构化返回 false + errMsg,不抛(不静默吞错)。</summary>
        [Test]
        [Category("Regression")]
        [Category("Timeout")]
        public void RtcpFrameEnter_NoDevice_ReturnsFalseWithErrMsg_NoThrow()
        {
            var node = new RtcpFrameEnter { AxisDevice = null };
            bool ok = false;
            string errMsg = null;
            Action act = () => ok = node.DoExcute(out errMsg);
            act.Should().NotThrow("RtcpFrameEnter 无设备不应抛异常");
            ok.Should().BeFalse("无设备应返回 false");
            errMsg.Should().NotBeEmpty("应给出结构化错误信息");
        }

        /// <summary>缺口:CrdConti 无设备(AxisDevice=null)返回 false,不抛。</summary>
        [Test]
        [Category("Regression")]
        [Category("Timeout")]
        public void CrdConti_NoDevice_ReturnsFalse_NoThrow()
        {
            var node = new CrdConti { AxisDevice = null, Crd = 0 };
            bool ok = false;
            Action act = () => ok = node.DoExcute(out _);
            act.Should().NotThrow("CrdConti 无设备不应抛异常");
            ok.Should().BeFalse("无设备应返回 false");
        }

        #endregion

        #region 虚拟后端辅助(镜像 AOI1VirtualEndToEndTests,精简为回归基线所需)

        /// <summary>
        /// 构建虚拟后端:DeviceEngine(DeviceMode.Virtual)+ ZMotionMotionCard(SimulationMode)。
        /// 镜像 AOI1VirtualEndToEndTests.BuildVirtualBackend,精简掉 VAxisM/五轴绑定(回归基线 HandoverNode 不需要)。
        /// </summary>
        private static void BuildVirtualBackend(out DeviceEngine engine)
        {
            engine = new DeviceEngine { DeviceMode = DeviceMode.Virtual };
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 8 };
            card.ID = Guid.NewGuid();
            card.InitApi();
            engine.AddDevice(card);
        }

        /// <summary>构造一个绑定 FiveAxisModule 的节点模块,并完成虚拟后端接线(镜像 AOI1VirtualEndToEndTests.MakeModule)。</summary>
        private static IMotionModule MakeModule(string funcName, DeviceEngine engine, IMotionModule parent,
            Action<ParameterSetter> setParams = null)
        {
            var module = new FiveAxisModule();
            module.SetFunction(funcName);
            module.DeviceEngine = engine;
            module.BrokenOff = new ManualResetEventSlim(true);   // Set:跳过 Run 的暂停等待 + IsStartStation
            module.Parent = parent;                               // 非 null 且非 IFreeStation(Run 内 Parent.TaskFunction 访问安全)
            var setter = new ParameterSetter(module);
            setParams?.Invoke(setter);
            return module;
        }

        /// <summary>ParameterAttribute.Value 便捷设置器(ValidateAllIn 据此同步 Function 属性)。</summary>
        private sealed class ParameterSetter
        {
            private readonly FiveAxisModule _module;
            public ParameterSetter(FiveAxisModule m) => _module = m;
            public void Set(string name, object value)
            {
                _module.Parameters[name].Should().NotBeNull($"节点应含参数 {name}");
                _module.Parameters[name].Value = value;
            }
        }

        #endregion
    }
}
