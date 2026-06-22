using Luster.Motion.DataStruct.Checkpoint;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Luster.Module.Motion.Recovery
{
    /// <summary>
    /// checkpoint JSON 文件持久化（ADR-B，TES-28 P0-B）。
    /// 产品维度单文件 + 原子写（写临时文件 → File.Replace），与 lmv recipe 并存不侵入。
    /// </summary>
    /// <remarks>
    /// 文件路径 = <c>&lt;baseDir&gt;/checkpoints/&lt;stationId&gt;.json</c>。
    /// 原子写用 <see cref="File.Replace(string,string,string)"/>：先写临时文件，再原子替换目标，
    /// 断电瞬间最多丢最后一次未刷新 checkpoint（R-B4 缓解：关键 phase 转换同步刷盘）。
    /// </remarks>
    public class CheckpointStore : ICheckpointStore
    {
        private readonly string _baseDir;

        public CheckpointStore(string baseDir = null)
        {
            _baseDir = string.IsNullOrEmpty(baseDir)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "checkpoints")
                : baseDir;
            if (!Directory.Exists(_baseDir))
            {
                Directory.CreateDirectory(_baseDir);
            }
        }

        /// <inheritdoc/>
        public RunCheckpoint Load(string stationId)
        {
            string path = PathFor(stationId);
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                var dto = JsonConvert.DeserializeObject<CheckpointDto>(json);
                return dto?.ToModel();
            }
            catch
            {
                // checkpoint 损坏视为无 checkpoint，触发清机恢复（不抛异常阻断启动）
                return null;
            }
        }

        /// <inheritdoc/>
        public void Save(RunCheckpoint checkpoint)
        {
            if (checkpoint == null) return;
            string path = PathFor(checkpoint.StationId);
            var dto = CheckpointDto.FromModel(checkpoint);
            string json = JsonConvert.SerializeObject(dto, Formatting.Indented);

            // 原子写：临时文件 → 替换目标
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(path))
            {
                string bak = path + ".bak";
                File.Replace(tmp, path, bak);
            }
            else
            {
                File.Move(tmp, path);
            }
        }

        /// <inheritdoc/>
        public void Clear(string stationId)
        {
            string path = PathFor(stationId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private string PathFor(string stationId)
        {
            // stationId 作文件名需净化，防止路径穿越
            string safe = string.IsNullOrEmpty(stationId) ? "default" : string.Concat(stationId.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_baseDir, safe + ".json");
        }

        /// <summary>JSON 序列化 DTO（与 RunCheckpoint 值对象互转）</summary>
        private sealed class CheckpointDto
        {
            public List<string> InStationProductSNs { get; set; }
            public string StationId { get; set; }
            public string Phase { get; set; }
            public int TrajectoryActionIndex { get; set; }
            public HandoverStateSnapshotDto Handover { get; set; }
            public AxisSafePositionDto LastSafePosition { get; set; }
            public bool TraceWritten { get; set; }
            public DateTime CapturedAtUtc { get; set; }

            public static CheckpointDto FromModel(RunCheckpoint m)
            {
                return new CheckpointDto
                {
                    InStationProductSNs = new List<string>(m.InStationProductSNs),
                    StationId = m.StationId,
                    Phase = m.Phase,
                    TrajectoryActionIndex = m.TrajectoryActionIndex,
                    Handover = HandoverStateSnapshotDto.FromModel(m.Handover),
                    LastSafePosition = AxisSafePositionDto.FromModel(m.LastSafePosition),
                    TraceWritten = m.TraceWritten,
                    CapturedAtUtc = m.CapturedAtUtc
                };
            }

            public RunCheckpoint ToModel()
            {
                return new RunCheckpoint(
                    InStationProductSNs ?? new List<string>(),
                    StationId, Phase, TrajectoryActionIndex,
                    Handover?.ToModel(),
                    LastSafePosition?.ToModel(),
                    TraceWritten, CapturedAtUtc);
            }
        }

        private sealed class HandoverStateSnapshotDto
        {
            public string Role { get; set; }
            public int CurrentStep { get; set; }
            public bool IsOnline { get; set; }
            public HandoverSignalStateDto Signals { get; set; }
            public string PeerStationId { get; set; }
            public string ExtraState { get; set; }
            public DateTime CapturedAtUtc { get; set; }

            public static HandoverStateSnapshotDto FromModel(HandoverStateSnapshot m)
            {
                if (m == null) return null;
                return new HandoverStateSnapshotDto
                {
                    Role = m.Role, CurrentStep = m.CurrentStep, IsOnline = m.IsOnline,
                    Signals = HandoverSignalStateDto.FromModel(m.Signals),
                    PeerStationId = m.PeerStationId, ExtraState = m.ExtraState,
                    CapturedAtUtc = m.CapturedAtUtc
                };
            }

            public HandoverStateSnapshot ToModel()
            {
                return new HandoverStateSnapshot(
                    Role, CurrentStep, IsOnline, Signals?.ToModel(),
                    PeerStationId, ExtraState, CapturedAtUtc);
            }
        }

        private sealed class HandoverSignalStateDto
        {
            public bool Ready, Sending, Transfer, InterLock, Heartbeat, DoorLock, Request;
            public List<bool> ProductExist, ProductOK, ProductNG1, ProductNG2;

            public static HandoverSignalStateDto FromModel(HandoverSignalState s)
            {
                if (s == null) return null;
                return new HandoverSignalStateDto
                {
                    Ready = s.Ready, Sending = s.Sending, Transfer = s.Transfer,
                    InterLock = s.InterLock, Heartbeat = s.Heartbeat, DoorLock = s.DoorLock, Request = s.Request,
                    ProductExist = new List<bool>(s.ProductExist), ProductOK = new List<bool>(s.ProductOK),
                    ProductNG1 = new List<bool>(s.ProductNG1), ProductNG2 = new List<bool>(s.ProductNG2)
                };
            }

            public HandoverSignalState ToModel()
            {
                return new HandoverSignalState(
                    Ready, Sending, Transfer, InterLock, Heartbeat, DoorLock, Request,
                    ProductExist, ProductOK, ProductNG1, ProductNG2);
            }
        }

        private sealed class AxisSafePositionDto
        {
            public Dictionary<string, double> AxisPositions { get; set; }
            public DateTime CapturedAtUtc { get; set; }

            public static AxisSafePositionDto FromModel(AxisSafePosition m)
            {
                if (m == null) return null;
                var dict = new Dictionary<string, double>();
                foreach (var kv in m.AxisPositions) dict[kv.Key] = kv.Value;
                return new AxisSafePositionDto { AxisPositions = dict, CapturedAtUtc = m.CapturedAtUtc };
            }

            public AxisSafePosition ToModel()
            {
                return new AxisSafePosition(AxisPositions ?? new Dictionary<string, double>(), CapturedAtUtc);
            }
        }
    }
}
