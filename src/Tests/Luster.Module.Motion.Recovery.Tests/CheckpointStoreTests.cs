using Luster.Motion.DataStruct.Checkpoint;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Luster.Module.Motion.Recovery.Tests
{
    public class CheckpointStoreTests : IDisposable
    {
        private readonly string _tmpDir;

        public CheckpointStoreTests()
        {
            _tmpDir = Path.Combine(Path.GetTempPath(), "ckpt_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tmpDir);
        }

        public void Dispose()
        {
            try { Directory.Delete(_tmpDir, true); } catch { }
        }

        private static RunCheckpoint MakeCheckpoint(string stationId = "AOI1", int actionIndex = 7)
        {
            var signals = new HandoverSignalState(
                ready: true, sending: false, transfer: false, interLock: false,
                heartbeat: true, doorLock: true, request: false,
                new List<bool> { true, false, false, false },
                new List<bool> { false, false, false, false },
                new List<bool> { false, false, false, false },
                new List<bool> { false, false, false, false });
            var handover = new HandoverStateSnapshot("Feed", 6, true, signals, "上游A", DateTime.UtcNow);
            var safePos = new AxisSafePosition(
                new Dictionary<string, double> { ["X"] = 1.23, ["Y"] = 4.56, ["Z"] = 0.0 },
                DateTime.UtcNow);
            return new RunCheckpoint(
                new List<string> { "SN001", "SN002" }.AsReadOnly(),
                stationId, "检测", actionIndex, handover, safePos, true, DateTime.UtcNow);
        }

        [Fact]
        public void SaveLoad_往返一致()
        {
            var store = new CheckpointStore(_tmpDir);
            var cp = MakeCheckpoint();
            store.Save(cp);

            var loaded = store.Load("AOI1");
            Assert.NotNull(loaded);
            Assert.Equal("AOI1", loaded.StationId);
            Assert.Equal("检测", loaded.Phase);
            Assert.Equal(7, loaded.TrajectoryActionIndex);
            Assert.Equal(2, loaded.InStationProductSNs.Count);
            Assert.Equal("SN001", loaded.InStationProductSNs[0]);
            Assert.True(loaded.TraceWritten);
        }

        [Fact]
        public void SaveLoad_交握快照与轴安全位往返()
        {
            var store = new CheckpointStore(_tmpDir);
            store.Save(MakeCheckpoint());
            var loaded = store.Load("AOI1");

            Assert.NotNull(loaded.Handover);
            Assert.Equal("Feed", loaded.Handover.Role);
            Assert.Equal(6, loaded.Handover.CurrentStep);
            Assert.True(loaded.Handover.IsOnline);
            Assert.True(loaded.Handover.Signals.Ready);
            Assert.True(loaded.Handover.Signals.ProductExist[0]);
            Assert.False(loaded.Handover.Signals.ProductExist[1]);

            Assert.NotNull(loaded.LastSafePosition);
            Assert.Equal(1.23, loaded.LastSafePosition.AxisPositions["X"]);
            Assert.Equal(4.56, loaded.LastSafePosition.AxisPositions["Y"]);
        }

        [Fact]
        public void Save_原子写_覆盖旧文件()
        {
            var store = new CheckpointStore(_tmpDir);
            store.Save(MakeCheckpoint(actionIndex: 1));
            store.Save(MakeCheckpoint(actionIndex: 99));

            var loaded = store.Load("AOI1");
            Assert.Equal(99, loaded.TrajectoryActionIndex);
        }

        [Fact]
        public void Load_不存在返回null()
        {
            var store = new CheckpointStore(_tmpDir);
            Assert.Null(store.Load("不存在"));
        }

        [Fact]
        public void Load_损坏文件返回null_不抛异常()
        {
            string path = Path.Combine(_tmpDir, "损坏.json");
            File.WriteAllText(path, "{ 不是合法 json");
            var store = new CheckpointStore(_tmpDir);
            Assert.Null(store.Load("损坏"));
        }

        [Fact]
        public void Clear_删除checkpoint()
        {
            var store = new CheckpointStore(_tmpDir);
            store.Save(MakeCheckpoint());
            Assert.NotNull(store.Load("AOI1"));

            store.Clear("AOI1");
            Assert.Null(store.Load("AOI1"));
        }

        [Fact]
        public void Save_Handover为null_可序列化()
        {
            // HandoverNode 就位前，checkpoint.Handover 可为 null（Feed/Leave 先行）
            var store = new CheckpointStore(_tmpDir);
            var cp = new RunCheckpoint(
                new List<string>().AsReadOnly(), "AOI2", "空跑", 0,
                null, null, false, DateTime.UtcNow);
            store.Save(cp);

            var loaded = store.Load("AOI2");
            Assert.NotNull(loaded);
            Assert.Null(loaded.Handover);
            Assert.Null(loaded.LastSafePosition);
        }
    }
}
