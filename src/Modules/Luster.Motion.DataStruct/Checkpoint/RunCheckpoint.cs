using System;
using System.Collections.Generic;

namespace Luster.Motion.DataStruct.Checkpoint
{
    /// <summary>
    /// 运行 checkpoint（ADR-B，TES-28 P0-B 持久化模型）。
    /// 产品维度事务边界落盘，掉电后供 <see cref="IRecoveryService"/> 三策略恢复。
    /// </summary>
    /// <remarks>
    /// 实现环境约束：net472 / C# 7.3，不可用 record/init/nullable，统一 sealed class + 构造函数赋值。
    /// <see cref="Handover"/> 为值对象快照，不依赖 <c>HandoverNode</c> 具体类型——
    /// 由 <c>HandoverNode.GetSnapshot()</c>（ADR-TES-37 补遗）在采集时填充，
    /// HandoverNode 就位前该字段可为 null（Feed/Leave 恢复先行，ICW 字段待 TES-47 收口补）。
    /// </remarks>
    public sealed class RunCheckpoint
    {
        /// <summary>产品在籍 SN 列表</summary>
        public IReadOnlyList<string> InStationProductSNs { get; }

        /// <summary>工位标识</summary>
        public string StationId { get; }

        /// <summary>当前阶段（phase）</summary>
        public string Phase { get; }

        /// <summary>轨迹 action index（断点续跑的节点位置）</summary>
        public int TrajectoryActionIndex { get; }

        /// <summary>ICW/上下游交握状态快照（来自 HandoverNode.GetSnapshot()，可空）</summary>
        public HandoverStateSnapshot Handover { get; }

        /// <summary>最后安全位置（各轴断电前安全位）</summary>
        public AxisSafePosition LastSafePosition { get; }

        /// <summary>追溯写入状态（true=已落追溯，恢复时据此决定是否补写）</summary>
        public bool TraceWritten { get; }

        /// <summary>采集时间（UTC，用于判 checkpoint 新鲜度）</summary>
        public DateTime CapturedAtUtc { get; }

        public RunCheckpoint(
            IReadOnlyList<string> inStationProductSNs,
            string stationId,
            string phase,
            int trajectoryActionIndex,
            HandoverStateSnapshot handover,
            AxisSafePosition lastSafePosition,
            bool traceWritten,
            DateTime capturedAtUtc)
        {
            InStationProductSNs = inStationProductSNs ?? new List<string>().AsReadOnly();
            StationId = stationId ?? string.Empty;
            Phase = phase ?? string.Empty;
            TrajectoryActionIndex = trajectoryActionIndex;
            Handover = handover;
            LastSafePosition = lastSafePosition;
            TraceWritten = traceWritten;
            CapturedAtUtc = capturedAtUtc;
        }
    }

    /// <summary>
    /// 交握状态快照（ADR-B 值对象，装语义态不装 raw uint，与线缆位布局解耦）。
    /// </summary>
    public sealed class HandoverStateSnapshot
    {
        /// <summary>交握方向（Feed/Leave/ICW，对齐 ADR-TES-37 §3.2）</summary>
        public string Role { get; }

        /// <summary>15/13 步状态机当前步</summary>
        public int CurrentStep { get; }

        /// <summary>心跳在线</summary>
        public bool IsOnline { get; }

        /// <summary>语义态信号</summary>
        public HandoverSignalState Signals { get; }

        /// <summary>对端站点标识</summary>
        public string PeerStationId { get; }

        /// <summary>
        /// 角色专用扩展状态（ICW 用：装 "Recipe=X;Mode=Y;Result=Z" 等ICW特有字段；
        /// Feed/Leave 留空）。向后兼容，默认空字符串。
        /// </summary>
        public string ExtraState { get; }

        /// <summary>采集时间（UTC）</summary>
        public DateTime CapturedAtUtc { get; }

        public HandoverStateSnapshot(string role, int currentStep, bool isOnline,
            HandoverSignalState signals, string peerStationId, DateTime capturedAtUtc)
            : this(role, currentStep, isOnline, signals, peerStationId, string.Empty, capturedAtUtc)
        {
        }

        public HandoverStateSnapshot(string role, int currentStep, bool isOnline,
            HandoverSignalState signals, string peerStationId, string extraState, DateTime capturedAtUtc)
        {
            Role = role ?? string.Empty;
            CurrentStep = currentStep;
            IsOnline = isOnline;
            Signals = signals;
            PeerStationId = peerStationId ?? string.Empty;
            ExtraState = extraState ?? string.Empty;
            CapturedAtUtc = capturedAtUtc;
        }
    }

    /// <summary>
    /// 交握语义态（非 [Flags]，与 ADR-TES-37 §3.3 HandoverSignalBit 位字典解耦）。
    /// </summary>
    public sealed class HandoverSignalState
    {
        public bool Ready { get; }
        public bool Sending { get; }
        public bool Transfer { get; }
        public bool InterLock { get; }
        public bool Heartbeat { get; }
        public bool DoorLock { get; }
        public bool Request { get; }

        /// <summary>4 产品在籍（len 4）</summary>
        public IReadOnlyList<bool> ProductExist { get; }
        /// <summary>4 产品 OK（len 4）</summary>
        public IReadOnlyList<bool> ProductOK { get; }
        /// <summary>4 产品 NG1（len 4）</summary>
        public IReadOnlyList<bool> ProductNG1 { get; }
        /// <summary>4 产品 NG2（len 4）</summary>
        public IReadOnlyList<bool> ProductNG2 { get; }

        public HandoverSignalState(
            bool ready, bool sending, bool transfer, bool interLock,
            bool heartbeat, bool doorLock, bool request,
            IReadOnlyList<bool> productExist, IReadOnlyList<bool> productOK,
            IReadOnlyList<bool> productNG1, IReadOnlyList<bool> productNG2)
        {
            Ready = ready; Sending = sending; Transfer = transfer; InterLock = interLock;
            Heartbeat = heartbeat; DoorLock = doorLock; Request = request;
            ProductExist = productExist ?? Bool4();
            ProductOK = productOK ?? Bool4();
            ProductNG1 = productNG1 ?? Bool4();
            ProductNG2 = productNG2 ?? Bool4();
        }

        private static IReadOnlyList<bool> Bool4() => new List<bool> { false, false, false, false }.AsReadOnly();
    }

    /// <summary>
    /// 轴安全位置（ADR-B，断电前各轴安全位，恢复时复核）。
    /// </summary>
    public sealed class AxisSafePosition
    {
        /// <summary>轴名 → 安全位置（脉冲/工程单位）</summary>
        public IReadOnlyDictionary<string, double> AxisPositions { get; }

        /// <summary>采集时间（UTC）</summary>
        public DateTime CapturedAtUtc { get; }

        public AxisSafePosition(IReadOnlyDictionary<string, double> axisPositions, DateTime capturedAtUtc)
        {
            AxisPositions = axisPositions ?? new Dictionary<string, double>();
            CapturedAtUtc = capturedAtUtc;
        }
    }

    /// <summary>
    /// 启动恢复三策略（ADR-B，TES-28 验收"清机·续跑·报废"）。
    /// </summary>
    public enum RecoveryStrategy
    {
        /// <summary>清机：清除在籍产品，回到安全位</summary>
        ClearMachine = 0,
        /// <summary>续跑：保留在籍产品，从断点继续</summary>
        Resume = 1,
        /// <summary>报废当前在籍产品</summary>
        ScrapCurrent = 2
    }

    /// <summary>
    /// 恢复结果（ADR-B，IRecoveryService.Recover 返回值）。
    /// </summary>
    public sealed class RecoveryResult
    {
        public bool Success { get; }
        public RecoveryStrategy Strategy { get; }
        /// <summary>恢复到的事件点（续跑用：phase/action index），失败时为 -1</summary>
        public int ResumedActionIndex { get; }
        /// <summary>恢复过程中触发的报警码列表（含实物校验失败项）</summary>
        public IReadOnlyList<string> AlarmCodes { get; }
        /// <summary>恢复说明（成功/失败原因）</summary>
        public string Message { get; }

        public RecoveryResult(bool success, RecoveryStrategy strategy, int resumedActionIndex,
            IReadOnlyList<string> alarmCodes, string message)
        {
            Success = success;
            Strategy = strategy;
            ResumedActionIndex = resumedActionIndex;
            AlarmCodes = alarmCodes ?? new List<string>().AsReadOnly();
            Message = message ?? string.Empty;
        }

        public static RecoveryResult Ok(RecoveryStrategy strategy, int resumedActionIndex, string message)
            => new RecoveryResult(true, strategy, resumedActionIndex, null, message);

        public static RecoveryResult Fail(RecoveryStrategy strategy, IReadOnlyList<string> alarmCodes, string message)
            => new RecoveryResult(false, strategy, -1, alarmCodes, message);
    }
}
