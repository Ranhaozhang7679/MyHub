using Luster.Module.Motion.Handover.Services;
using Luster.Module.Motion.Handover.Signals;
using Luster.Motion.TaskFlow.Engine;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Handover.Tests
{
    /// <summary>
    /// HandoverAutoSignalService + AutoCommandDispatcher 单测（TES-37-6 验收）。
    /// <para>用受控信号源（直接拨 16 位字值）+ 受控时钟确定性驱动 <see cref="HandoverAutoSignalService.ScanOnce"/>，
    /// 验证各命令位边沿 → <see cref="AutoCommandType"/> 事件 → <see cref="IMotionController"/> 命令派发。</para>
    /// </summary>
    [TestFixture]
    public class HandoverAutoSignalServiceTests
    {
        /// <summary>受控 16 位信号源：单测直接拨动 AutoReadSignal 字值，绕开 VModbusServer/Engine 依赖</summary>
        private sealed class MockSignalSource
        {
            public ushort Value { get; set; }
            public ushort Read() => Value;
        }

        /// <summary>受控时钟：单测确定性推进时间</summary>
        private sealed class MockClock
        {
            public DateTime Now { get; set; } = new DateTime(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc);
            public DateTime Read() => Now;
            public void AdvanceMs(int ms) => Now = Now.AddMilliseconds(ms);
        }

        private static ushort Bits(params int[] bits)
        {
            ushort v = 0;
            foreach (var b in bits)
            {
                v |= AutoSignalBit.Mask(b);
            }
            return v;
        }

        private static HandoverAutoSignalService NewService(
            MockSignalSource src, MockClock? clock = null, int scanMs = 20)
        {
            return new HandoverAutoSignalService(src.Read, scanMs, (clock ?? new MockClock()).Read);
        }

        // ===== Service: 边沿检测 =====

        [Test]
        public void FirstFrame_DoesNotRaiseEdge_AvoidsSpuriousTrigger()
        {
            var src = new MockSignalSource { Value = Bits(AutoSignalBit.Start) };
            var svc = NewService(src);
            var raised = new List<AutoSignalEventArgs>();
            svc.OnAutoCommand += (s, e) => raised.Add(e);

            // 首帧：PLC 已置位 Start，但不应判为上升沿（避免启动瞬间误触发）
            svc.ScanOnce();

            Assert.That(raised, Is.Empty, "首帧不应产生边沿事件");
        }

        [Test]
        public void RisingEdge_OnEachCommandBit_RaisesCorrectCommand()
        {
            var src = new MockSignalSource { Value = 0 };
            var svc = NewService(src);
            var raised = new List<AutoSignalEventArgs>();
            svc.OnAutoCommand += (s, e) => raised.Add(e);

            svc.ScanOnce(); // 首帧记录初值 0

            // 依次累加置位每个命令位（前序位保持，避免误产生下降沿），每次只新增 1 个上升沿
            var cases = new[]
            {
                (AutoSignalBit.Start, AutoCommandType.Start),
                (AutoSignalBit.Stop, AutoCommandType.Stop),
                (AutoSignalBit.Pause, AutoCommandType.Pause),
                (AutoSignalBit.Reset, AutoCommandType.Reset),
                (AutoSignalBit.Init, AutoCommandType.Init),
            };

            ushort acc = 0;
            foreach (var (bit, cmd) in cases)
            {
                acc |= AutoSignalBit.Mask(bit);
                src.Value = acc;
                svc.ScanOnce();
            }

            Assert.That(raised.Count, Is.EqualTo(5), "5 个命令位各 1 次上升沿");
            Assert.That(
                raised.ConvertAll(r => r.Command),
                Is.EqualTo(new[] { AutoCommandType.Start, AutoCommandType.Stop, AutoCommandType.Pause, AutoCommandType.Reset, AutoCommandType.Init }));
            Assert.That(raised.TrueForAll(r => r.IsRisingEdge), Is.True, "均为上升沿");
        }

        [Test]
        public void FallingEdge_RaisesWithIsRisingEdgeFalse()
        {
            var src = new MockSignalSource { Value = Bits(AutoSignalBit.Pause) };
            var svc = NewService(src);
            var raised = new List<AutoSignalEventArgs>();
            svc.OnAutoCommand += (s, e) => raised.Add(e);

            svc.ScanOnce(); // 首帧记录初值（Pause 已置位）

            src.Value = 0; // 清零 → 下降沿
            svc.ScanOnce();

            Assert.That(raised.Count, Is.EqualTo(1));
            Assert.That(raised[0].Command, Is.EqualTo(AutoCommandType.Pause));
            Assert.That(raised[0].IsRisingEdge, Is.False, "应为下降沿");
        }

        [Test]
        public void NoChange_DoesNotRaise()
        {
            var src = new MockSignalSource { Value = Bits(AutoSignalBit.Start, AutoSignalBit.Reset) };
            var svc = NewService(src);
            var raised = new List<AutoSignalEventArgs>();
            svc.OnAutoCommand += (s, e) => raised.Add(e);

            svc.ScanOnce(); // 首帧
            svc.ScanOnce(); // 同值，无变化
            svc.ScanOnce(); // 同值，无变化

            Assert.That(raised, Is.Empty);
        }

        [Test]
        public void MultipleBitsRisingInOneScan_AllRaised()
        {
            var src = new MockSignalSource { Value = 0 };
            var svc = NewService(src);
            var raised = new List<AutoSignalEventArgs>();
            svc.OnAutoCommand += (s, e) => raised.Add(e);

            svc.ScanOnce(); // 首帧

            src.Value = Bits(AutoSignalBit.Start, AutoSignalBit.Stop, AutoSignalBit.Init);
            svc.ScanOnce();

            Assert.That(raised.Count, Is.EqualTo(3), "一帧内 3 位同时上升沿应各广播一次");
            Assert.That(
                raised.ConvertAll(r => r.Command),
                Is.EquivalentTo(new[] { AutoCommandType.Start, AutoCommandType.Stop, AutoCommandType.Init }));
        }

        [Test]
        public void ModeBits_AutoAndAllowInit_DoNotRaiseCommand()
        {
            var src = new MockSignalSource { Value = 0 };
            var svc = NewService(src);
            var raised = new List<AutoSignalEventArgs>();
            svc.OnAutoCommand += (s, e) => raised.Add(e);

            svc.ScanOnce(); // 首帧

            // Auto(5) / AllowInit(6) 是模式标志，翻转不应产生命令事件
            src.Value = Bits(AutoSignalBit.Auto, AutoSignalBit.AllowInit);
            svc.ScanOnce();

            Assert.That(raised, Is.Empty, "Auto/AllowInit 模式位不应触发命令事件");
        }

        [Test]
        public void Configure_StoresNameAndAddress()
        {
            var src = new MockSignalSource();
            var svc = NewService(src);

            svc.Configure("VModbusServer_AOI1", 0x0100);

            Assert.That(svc.VModbusServerName, Is.EqualTo("VModbusServer_AOI1"));
            Assert.That(svc.AutoSignalAddress, Is.EqualTo(0x0100));
        }

        [Test]
        public void ReadException_DoesNotCrash_AndDoesNotCorruptState()
        {
            // 受控读取器：首帧抛异常（模拟 VModbusServer 未就绪），后续返回受控值
            ushort value = 0;
            int calls = 0;
            Func<ushort> reader = () =>
            {
                calls++;
                if (calls == 1)
                {
                    throw new InvalidOperationException("VModbusServer 未就绪");
                }
                return value;
            };
            var svc = new HandoverAutoSignalService(reader, 20, () => DateTime.Now);
            var raised = new List<AutoSignalEventArgs>();
            svc.OnAutoCommand += (s, e) => raised.Add(e);

            // 第 1 帧：读取异常 → 不崩溃，不初始化，不产生事件
            svc.ScanOnce();
            Assert.That(raised, Is.Empty, "异常帧不应产生事件");

            // 第 2 帧：读取成功，值=0 → 记录初值，不产生事件
            svc.ScanOnce();
            Assert.That(raised, Is.Empty, "首帧成功记录初值不应产生事件");

            // 第 3 帧：Start 置位 → 上升沿，应产生事件（证明异常帧未污染 _previous）
            value = Bits(AutoSignalBit.Start);
            svc.ScanOnce();
            Assert.That(raised.Count, Is.EqualTo(1));
            Assert.That(raised[0].Command, Is.EqualTo(AutoCommandType.Start));
            Assert.That(raised[0].IsRisingEdge, Is.True);
        }

        // ===== Dispatcher: 命令派发 =====

        private static (HandoverAutoSignalService svc, Mock<IMotionController> controller, AutoCommandDispatcher dispatcher)
            NewDispatcher(MockSignalSource src)
        {
            var svc = NewService(src);
            var mock = new Mock<IMotionController>();
            var dispatcher = new AutoCommandDispatcher(svc, mock.Object);
            return (svc, mock, dispatcher);
        }

        [Test]
        public void Dispatcher_RisingStart_CallsMotionControllerStart()
        {
            var src = new MockSignalSource { Value = 0 };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce(); // 首帧

            src.Value = Bits(AutoSignalBit.Start);
            svc.ScanOnce();

            controller.Verify(c => c.Start(), Times.Once);
            controller.Verify(c => c.Stop(It.IsAny<Action>()), Times.Never);
            using (dispatcher) { }
        }

        [Test]
        public void Dispatcher_RisingStop_CallsMotionControllerStop()
        {
            var src = new MockSignalSource { Value = 0 };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce();

            src.Value = Bits(AutoSignalBit.Stop);
            svc.ScanOnce();

            controller.Verify(c => c.Stop(It.IsAny<Action>()), Times.Once);
            using (dispatcher) { }
        }

        [Test]
        public void Dispatcher_RisingPause_CallsMotionControllerPause()
        {
            var src = new MockSignalSource { Value = 0 };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce();

            src.Value = Bits(AutoSignalBit.Pause);
            svc.ScanOnce();

            controller.Verify(c => c.Pause(It.IsAny<bool>(), It.IsAny<Action>()), Times.Once);
            using (dispatcher) { }
        }

        [Test]
        public void Dispatcher_RisingReset_CallsMotionControllerRecovery()
        {
            var src = new MockSignalSource { Value = 0 };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce();

            src.Value = Bits(AutoSignalBit.Reset);
            svc.ScanOnce();

            controller.Verify(c => c.Recovery(), Times.Once);
            using (dispatcher) { }
        }

        [Test]
        public void Dispatcher_RisingInit_CallsMotionControllerHome()
        {
            var src = new MockSignalSource { Value = 0 };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce();

            src.Value = Bits(AutoSignalBit.Init);
            svc.ScanOnce();

            controller.Verify(c => c.Home(), Times.Once);
            using (dispatcher) { }
        }

        [Test]
        public void Dispatcher_FallingEdge_DoesNotDispatch()
        {
            var src = new MockSignalSource { Value = Bits(AutoSignalBit.Start) };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce(); // 首帧（Start 已置位）

            src.Value = 0; // 下降沿
            svc.ScanOnce();

            controller.Verify(c => c.Start(), Times.Never, "下降沿不应派发命令");
            controller.Verify(c => c.Stop(It.IsAny<Action>()), Times.Never);
            using (dispatcher) { }
        }

        [Test]
        public void Dispatcher_AllFiveRisingInSequence_AllDispatched()
        {
            var src = new MockSignalSource { Value = 0 };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce(); // 首帧

            // 逐位置位再清零，5 个命令各上升沿一次
            var sequence = new[]
            {
                AutoSignalBit.Start, AutoSignalBit.Stop, AutoSignalBit.Pause,
                AutoSignalBit.Reset, AutoSignalBit.Init,
            };
            foreach (var bit in sequence)
            {
                src.Value = Bits(bit);
                svc.ScanOnce();
                src.Value = 0;
                svc.ScanOnce();
            }

            controller.Verify(c => c.Start(), Times.Once);
            controller.Verify(c => c.Stop(It.IsAny<Action>()), Times.Once);
            controller.Verify(c => c.Pause(It.IsAny<bool>(), It.IsAny<Action>()), Times.Once);
            controller.Verify(c => c.Recovery(), Times.Once);
            controller.Verify(c => c.Home(), Times.Once);
            using (dispatcher) { }
        }

        [Test]
        public void Dispatcher_Dispose_Unsubscribes_NoFurtherDispatch()
        {
            var src = new MockSignalSource { Value = 0 };
            var (svc, controller, dispatcher) = NewDispatcher(src);
            svc.ScanOnce(); // 首帧

            dispatcher.Dispose();

            src.Value = Bits(AutoSignalBit.Start);
            svc.ScanOnce();

            controller.Verify(c => c.Start(), Times.Never, "Dispose 后不应再派发");
        }

        [Test]
        public void Dispatcher_NullArgs_Throws()
        {
            var src = new MockSignalSource();
            var svc = NewService(src);
            var controller = new Mock<IMotionController>();

            Assert.Throws<ArgumentNullException>(() => new AutoCommandDispatcher(null!, controller.Object));
            Assert.Throws<ArgumentNullException>(() => new AutoCommandDispatcher(svc, null!));
        }
    }
}
