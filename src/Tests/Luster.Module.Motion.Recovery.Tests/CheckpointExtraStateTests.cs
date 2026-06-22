using Luster.Motion.DataStruct.Checkpoint;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Luster.Module.Motion.Recovery.Tests
{
    /// <summary>
    /// TES-28 batch2:HandoverStateSnapshot.ExtraState(ICW 快照字段) 序列化往返测试。
    /// </summary>
    public class CheckpointExtraStateTests : IDisposable
    {
        private readonly string _tmpDir;
        public CheckpointExtraStateTests()
        {
            _tmpDir = Path.Combine(Path.GetTempPath(), "ckpt_extra_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tmpDir);
        }
        public void Dispose() { try { Directory.Delete(_tmpDir, true); } catch { } }

        [Fact]
        public void ExtraState_ICW快照字段_序列化往返()
        {
            // ICW 快照:ExtraState 装 "Recipe=1;Mode=2;Result=Success"
            var signals = new HandoverSignalState(
                false, false, false, true, true, true, false,
                Bool4(), Bool4(), Bool4(), Bool4());
            var handover = new HandoverStateSnapshot(
                "ICW", 5, true, signals, "ICW站",
                "Recipe=1;Mode=2;Result=Success", DateTime.UtcNow);

            var cp = new RunCheckpoint(
                new List<string> { "SN-ICW" }.AsReadOnly(),
                "ICW1", "ICW检测", 3, handover,
                new AxisSafePosition(new Dictionary<string, double>(), DateTime.UtcNow),
                true, DateTime.UtcNow);

            var store = new CheckpointStore(_tmpDir);
            store.Save(cp);

            var loaded = store.Load("ICW1");
            Assert.NotNull(loaded.Handover);
            Assert.Equal("ICW", loaded.Handover.Role);
            Assert.Equal("Recipe=1;Mode=2;Result=Success", loaded.Handover.ExtraState);
            Assert.True(loaded.Handover.Signals.InterLock);
        }

        [Fact]
        public void ExtraState_FeedLeave默认空_向后兼容()
        {
            // Feed/Leave 用旧构造(无 ExtraState)→ 默认空字符串,向后兼容 batch1
            var signals = new HandoverSignalState(
                true, false, false, false, true, true, false,
                Bool4(), Bool4(), Bool4(), Bool4());
            var handover = new HandoverStateSnapshot("Feed", 3, true, signals, "上游A", DateTime.UtcNow);

            var cp = new RunCheckpoint(
                new List<string>().AsReadOnly(), "AOI1", "检测", 0,
                handover, null, false, DateTime.UtcNow);

            var store = new CheckpointStore(_tmpDir);
            store.Save(cp);
            var loaded = store.Load("AOI1");

            Assert.Equal("", loaded.Handover.ExtraState); // Feed/Leave 默认空
        }

        private static IReadOnlyList<bool> Bool4() => new List<bool> { false, false, false, false }.AsReadOnly();
    }
}
