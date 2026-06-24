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
        public void ZMotionMotionCard_ImplementsFiveAxisFrameContract()
        {
            var card = new ZMotionMotionCard { SimulationMode = true };
            card.Should().BeAssignableTo<IFiveAxisFrame>();
        }

        /// <summary>
        /// ADR-TES-110:SimulationMode 下 Frame 生命周期编排可跑通(进/退/解算短路返回 true,对齐源端 VIRTUAL_MODE)。
        /// 不触达卡端 SDK,无需硬件。FrameCal 输出零值默认 para。
        /// </summary>
        [Test]
        public void FiveAxisFrame_SimulationMode_LifecycleRunsAndShortCircuits()
        {
            var card = new ZMotionMotionCard { SimulationMode = true, AxisCount = 8 };
            card.InitApi();
            var frame = (IFiveAxisFrame)card;
            var realLis = new List<int> { 1, 2, 3, 4, 5 };
            var virLis = new List<int> { 101, 102, 103, 104, 105 };
            var para = new FiveAxisFramePara { ACenterX = 0, ACenterY = 10, ACenterZ = 5, ADirX = 1, ACirPulses = 360000, CCirPulses = 720000 };

            // 表地址配置(SimulationMode 下不实际使用,但契约可调)
            frame.ConfigureFrameTableAddr(1, new FiveAxisFrameTableAddr { Axis5ParaAddr = 1000, InAxisPosiTb = 1100, InExtendTb = 1200, OutZeroTb = 1300, OutRobotTb = 1400 }).Should().BeTrue();
            // 严格生命周期:ExitFrame → Frame → FrameCal → ExitFrame
            frame.ExitFrame(realLis, virLis).Should().BeTrue();
            frame.Frame(1, realLis, virLis, para).Should().BeTrue();
            var axisPosi = new List<double[]> { new double[] { 1, 2, 3, 0, 0 }, new double[] { 4, 5, 6, 1, 0 } };
            frame.FrameCal(1, realLis.Take(3).ToList(), axisPosi, out var aZero, out var outPara).Should().BeTrue();
            frame.ExitFrame(realLis, virLis).Should().BeTrue();
            // SimulationMode 输出零值默认
            aZero.Should().Be(0);
            outPara.ACenterX.Should().Be(0);
        }

        /// <summary>Reframe 本期留签名抛 NotSupportedException(ADR-TES-110,待后续正解 Issue)。</summary>
        [Test]
        public void FiveAxisFrame_Reframe_ThrowsNotSupported_StubOnly()
        {
            var card = new ZMotionMotionCard { SimulationMode = true };
            card.InitApi();
            var frame = (IFiveAxisFrame)card;

            Action act = () => frame.Reframe(1, new List<int> { 1, 2, 3 }, new List<int> { 101, 102, 103 }, new FiveAxisFramePara());

            act.Should().Throw<NotSupportedException>();
        }
    }
}
