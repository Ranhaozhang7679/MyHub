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
    }
}
