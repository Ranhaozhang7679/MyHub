using Luster.Motion.DataStruct.Checkpoint;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Module.Motion.Handover.Signals;
using System.Collections.Generic;
using Xunit;

namespace Luster.Module.Motion.Handover.Tests
{
    /// <summary>
    /// TES-28 batch2:HandoverNode.GetSnapshot() seam + InterlockMatrix.AttachTo + 复合快照 单测。
    /// </summary>
    public class HandoverSnapshotTests
    {
        /// <summary>可注入 ReadSignal 的 testable HandoverNode(不依赖 MyOwner/设备)</summary>
        private class SnapshotTestNode : HandoverNode
        {
            public System.Func<string, bool> OnRead = a => false;
            protected override bool ReadSignal(string address) => OnRead(address);
            protected override int StepMachine(int currentStep) => StepDone;
            protected override void ClearSignals() { }
            protected override bool CheckAccessError() => false; // 不驱动状态机
            protected override void StepInfo(int step, string message) { }
        }

        private static HandoverSignalAddress MakeAddress()
        {
            return new HandoverSignalAddress
            {
                RecReadyAddress = "R Ready", SendingAddress = "S Send", RecTranSferAddress = "R Trans",
                RecInterLockAddress = "R ILock", RecHeartBeatAddress = "R HB", SelfDoorLockAddress = "SelfDoor"
            };
        }

        [Fact]
        public void GetSnapshot_采集语义态不装raw()
        {
            var node = new SnapshotTestNode { Role = HandoverRole.Feed, Address = MakeAddress() };
            node.OnRead = a => a == "R Ready" || a == "R HB"; // Ready+Heartbeat ON

            var snap = node.GetSnapshot();

            Assert.NotNull(snap);
            Assert.Equal("Feed", snap.Role);
            Assert.True(snap.Signals.Ready);
            Assert.True(snap.Signals.Heartbeat);
            Assert.False(snap.Signals.InterLock);
            Assert.True(snap.IsOnline); // Heartbeat ON → 在线
        }

        [Fact]
        public void GetSnapshot_只读不改状态机步号()
        {
            var node = new SnapshotTestNode { Role = HandoverRole.Leave, Address = MakeAddress() };
            // CurrentStep 默认 0,GetSnapshot 不应改它
            var snap = node.GetSnapshot();
            Assert.Equal(0, snap.CurrentStep);
            Assert.Equal("Leave", snap.Role);
        }

        [Fact]
        public void GetSnapshot_地址未配置_返回占位快照不抛()
        {
            var node = new SnapshotTestNode { Role = HandoverRole.Feed, Address = new HandoverSignalAddress() };
            var snap = node.GetSnapshot();
            Assert.NotNull(snap);
            Assert.False(snap.Signals.Ready); // 空地址 → false
            Assert.False(snap.IsOnline);
        }

        [Fact]
        public void GetSnapshot_实现IHandoverSnapshotProvider接口()
        {
            IHandoverSnapshotProvider provider = new SnapshotTestNode { Role = HandoverRole.Feed, Address = MakeAddress() };
            var snap = provider.GetSnapshot();
            Assert.NotNull(snap);
        }

        [Fact]
        public void AttachTo_挂载Handover_上下游互锁维度投影()
        {
            // Feed 节点 InterLock ON → UpstreamInterlock 触发
            var feedNode = new SnapshotTestNode { Role = HandoverRole.Feed, Address = MakeAddress() };
            feedNode.OnRead = a => a == "R ILock"; // InterLock ON

            var matrix = new InterlockMatrix();
            matrix.AttachTo(feedNode);

            // 物理快照:无任何触发
            var physical = new FuncSnapshot((k, t) => false);
            var composite = matrix.CreateSnapshot(physical);

            // UpstreamInterlock 应从 Feed 节点投影为触发
            Assert.True(composite.IsTriggered(SafetyInputKind.UpstreamInterlock, null));
            // DownstreamInterlock 无 Leave 节点 → false
            Assert.False(composite.IsTriggered(SafetyInputKind.DownstreamInterlock, null));
            // 其他维度委托物理快照 → false
            Assert.False(composite.IsTriggered(SafetyInputKind.EStop, "e1"));
        }

        [Fact]
        public void AttachTo_Leave节点投影DownstreamInterlock()
        {
            var leaveNode = new SnapshotTestNode { Role = HandoverRole.Leave, Address = MakeAddress() };
            leaveNode.OnRead = a => a == "R ILock"; // InterLock ON

            var matrix = new InterlockMatrix();
            matrix.AttachTo(leaveNode);

            var composite = matrix.CreateSnapshot(new FuncSnapshot((k, t) => false));
            Assert.True(composite.IsTriggered(SafetyInputKind.DownstreamInterlock, null));
            Assert.False(composite.IsTriggered(SafetyInputKind.UpstreamInterlock, null));
        }

        [Fact]
        public void AttachTo_未挂载_CreateSnapshot返回原物理快照()
        {
            var matrix = new InterlockMatrix();
            var physical = new FuncSnapshot((k, t) => k == SafetyInputKind.EStop);
            var composite = matrix.CreateSnapshot(physical);
            Assert.Same(physical, composite); // 无 Handover 时直接返回原快照
        }

        [Fact]
        public void AttachTo_门锁未到位_互锁触发()
        {
            // DoorLock OFF(对端门锁未到位) → 互锁触发
            var feedNode = new SnapshotTestNode { Role = HandoverRole.Feed, Address = MakeAddress() };
            feedNode.OnRead = a => false; // 所有信号 OFF,含 SelfDoorLock

            var matrix = new InterlockMatrix();
            matrix.AttachTo(feedNode);
            var composite = matrix.CreateSnapshot(new FuncSnapshot((k, t) => false));

            Assert.True(composite.IsTriggered(SafetyInputKind.UpstreamInterlock, null)); // DoorLock 未到位
        }

        [Fact]
        public void Evaluate_复合快照_上下游互锁规则触发()
        {
            // 一条 UpstreamInterlock → Abort 规则
            var rule = new InterlockRule
            {
                RuleId = "UPSTREAM_ILK",
                AlarmCode = "UP_ILK",
                Recovery = RecoveryPolicy.Abort,
                Inputs = new[] { new InterlockInput { Kind = SafetyInputKind.UpstreamInterlock } }
            };
            var matrix = new InterlockMatrix(new[] { rule });

            var feedNode = new SnapshotTestNode { Role = HandoverRole.Feed, Address = MakeAddress() };
            feedNode.OnRead = a => a == "R ILock"; // 上游 InterLock ON
            matrix.AttachTo(feedNode);

            var composite = matrix.CreateSnapshot(new FuncSnapshot((k, t) => false));
            var triggered = matrix.Evaluate(composite);

            Assert.Single(triggered);
            Assert.Equal("UPSTREAM_ILK", triggered[0].RuleId);
            Assert.True(matrix.HasFatal(composite)); // Abort → Fatal
        }

        private sealed class FuncSnapshot : IInputSnapshot
        {
            private readonly System.Func<SafetyInputKind, string, bool> _f;
            public FuncSnapshot(System.Func<SafetyInputKind, string, bool> f) { _f = f; }
            public bool IsTriggered(SafetyInputKind kind, string target = null) => _f(kind, target);
        }
    }
}
