using FluentAssertions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.MotionCard.ZMotion;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luster.SimDevice.MotionCard.Tests
{
    [TestFixture]
    public class ZMotionMotionCardTests
    {
        [Test]
        public void ZMotionMotionCard_ImplementsMotionCardAndRtcpContracts()
        {
            var card = new ZMotionMotionCard { SimulationMode = true };

            card.Should().BeAssignableTo<IMotionCard>();
            card.Should().BeAssignableTo<IFiveAxisRTCP>();
            card.Brand.Should().Be("正运动");
        }

        [Test]
        public void SimulationMode_SupportsHomeMoveLineContinuousAndIoWithoutHardware()
        {
            var card = new ZMotionMotionCard
            {
                SimulationMode = true,
                AxisCount = 5,
                DigitalInCount = 2,
                DigitalOutCount = 2,
            };
            card.InitApi();

            card.Home(1, HomeMode.CurrentHome, 10, 2, 1000, 0.1, 0, AxisPML.Unknown);
            card.CheckHomeDone(1).Should().BeTrue();

            card.Move(12.5, 100, 100, 100, 1000, 0, true, 1, AxisPML.Unknown);
            card.GetCurrentPos(1, 1000).Should().BeApproximately(12.5, 0.000001);
            card.CheckMotionDone(1, 1, 12500).Should().BeTrue();

            card.MoveLine(
                new List<int> { 1, 2, 3 },
                new List<double> { 20, 30, 40 },
                new List<double> { 1000, 1000, 1000 },
                new List<double> { 100, 100, 100 },
                new List<double> { 100, 100, 100 });
            card.GetCurrentPos(2, 1000).Should().BeApproximately(30, 0.000001);
            card.CheckMotionDone(1, -1).Should().BeTrue();

            card.AxisContinuousMove(3, 100, 100, 1000, new List<double> { 41, 42, 43 }, new List<double> { 10, 10, 10 });
            card.GetCurrentPos(3, 1000).Should().BeApproximately(43, 0.000001);

            card.SetDigitalOut(1, true);
            card.GetDigitalOut(1).Should().BeTrue();

            card.ScanAxis(out var axisNum);
            axisNum.Should().Be(5);
            card.ScanDigitalIO(out var diNum, out var doNum);
            diNum.Should().Be(2);
            doNum.Should().Be(2);
        }

        [Test]
        public void FiveAxisRtcp_CanConfigureEnableDisableAndRejectIncompleteAxisMap()
        {
            var card = new ZMotionMotionCard { SimulationMode = true };
            card.InitApi();

            var config = new FiveAxisRtcpConfig
            {
                CoordinateSystem = 1,
                VirtualAxisIds = new List<int> { 101, 102, 103, 104, 105 },
                RealAxisIds = new List<int> { 1, 2, 3, 4, 5 },
                RotationCenterX = 1,
                RotationCenterY = 2,
                RotationCenterZ = 3,
            };

            card.ConfigureRtcp(config).Should().BeTrue();
            card.RtcpEnabled.Should().BeFalse();
            card.SetRtcpEnabled(true).Should().BeTrue();
            card.RtcpEnabled.Should().BeTrue();
            card.SetRtcpEnabled(false).Should().BeTrue();
            card.RtcpEnabled.Should().BeFalse();

            Action invalid = () => card.ConfigureRtcp(new FiveAxisRtcpConfig
            {
                VirtualAxisIds = new List<int> { 101, 102 },
                RealAxisIds = new List<int> { 1, 2, 3, 4, 5 },
            });
            invalid.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PublicMotionCardMethods_AreImplementedWithoutThrowingNotImplementedException()
        {
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 5, DigitalInCount = 1, DigitalOutCount = 1 };
            card.InitApi();
            card.ServOn(1, true);

            Action callServo = () => card.ServOn(1, false);
            callServo.Should().NotThrow<NotImplementedException>();

            Action callSetAnalog = () => card.SetAnalogOut(0, 1.0);
            callSetAnalog.Should().NotThrow<NotImplementedException>();

            Action callSdo = () => { card.SDOWrite(1, 0x6041, 0x00, 1, 16); card.SDORead(1, 0x6041, 0x00, 16, out _, 1); };
            callSdo.Should().NotThrow<NotImplementedException>();

            Action callPdo = () => { card.PDOWrite(1, 0x6040, 0x00, 1, 16); var v = 0; card.PDORead(1, 0x6040, 0x00, 16, ref v, 1); };
            callPdo.Should().NotThrow<NotImplementedException>();
        }

        [Test]
        public void ZMotionMotionCard_ImplementsContiInterpAndLatchBypassContracts()
        {
            // R1 非侵入验证:旁路接口与 IFiveAxisRTCP 同层,仅 ZMotion 五轴适配器实现。
            var card = new ZMotionMotionCard { SimulationMode = true };
            card.Should().BeAssignableTo<IFiveAxisContiInterp>();
            card.Should().BeAssignableTo<IFiveAxisLatch>();
        }

        [Test]
        public void SimulationMode_ContiInterpRunsFullLifecycleWithDeterministicStubs()
        {
            // ADR v2 虚拟分支确定性桩值:ReadContiOutFlag 按注入点位递增、GetContiRemainSpace 返回充足,
            // 让虚拟端到端链跑通;M-13 finally 关闭契约由节点层保证,此处验证卡端 Open/Add/Stop/Close 不抛异常。
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 5 };
            card.InitApi();
            var conti = (IFiveAxisContiInterp)card;

            conti.CrdContiOpen(0, new[] { 1, 2, 3, 4, 5 }, CrdMode.Absolute).Should().BeTrue();
            conti.CrdContiStart(0).Should().BeTrue();
            conti.CrdContiAddLine(0, new[] { 100.0, 0, 0, 0, 0 }, ContiMoveMode.Absolute).Should().BeTrue();
            conti.CrdContiAddOutput(0, 1, true, 0).Should().BeTrue();

            conti.GetContiRemainSpace(0, out var space).Should().BeTrue();
            space.Should().BeGreaterThan(0, "虚拟分支应返回充足背压");

            var flag = 0;
            conti.ReadContiOutFlag(0, ref flag).Should().BeTrue();
            flag.Should().Be(0, "虚拟分支 ReadContiOutFlag 回读注入的标记号");

            conti.WaitCrdDone(0, 100).Should().BeTrue();
            conti.CrdContiStop(0).Should().BeTrue();
            conti.CrdContiClose(0).Should().BeTrue();

            conti.SetSmoothProfile(0, new SmoothProfile { CornerMode = 1, CornerRadius = 0.5, DecelAngle = 30, StopAngle = 60 }).Should().BeTrue();
        }

        [Test]
        public void SimulationMode_LatchBatchWaitAndClearWithReplayedPoints()
        {
            // ADR v2 虚拟分支:锁存值按注入点位回放;WaitLatched count 批量重载为主路径。
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 5 };
            card.InitApi();
            var latch = (IFiveAxisLatch)card;

            latch.StartLatch(1, new LatchTrigger { LatchIndex = 0, SourceIndex = 0, TriggerEdge = LatchTriggerEdge.RisingEdge, ContinuousMode = true }).Should().BeTrue();
            // StartLatch 后注入虚拟飞拍触发点(模拟轨迹推进中被锁存的点位)。
            card.InjectVirtualLatchPoints(0, new[] { 10.0, 20.0, 30.0 });

            latch.WaitLatched(1, 3, 1000, out var positions).Should().BeTrue();
            positions.Should().HaveCount(3);
            positions[0].Should().BeApproximately(10.0, 1e-9);
            positions[2].Should().BeApproximately(30.0, 1e-9);

            // 单值便利重载转调 count=1
            card.InjectVirtualLatchPoints(0, new[] { 42.0 });
            latch.WaitLatched(1, 100, out var single).Should().BeTrue();
            single.Should().BeApproximately(42.0, 1e-9);

            latch.ClearLatch(1).Should().BeTrue();
        }
    }
}
