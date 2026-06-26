using Luster.Motion.DataStruct.Checkpoint;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 交握快照提供者（ADR-C seam，框架层抽象，避免 <c>InterlockMatrix</c> 反向依赖业务层 <c>HandoverNode</c>）。
    /// <para><c>HandoverNode</c>（业务层）实现本接口，<c>InterlockMatrix.AttachTo</c> 接收本接口，
    /// 使上下游互锁随交握状态动态生效。</para>
    /// </summary>
    public interface IHandoverSnapshotProvider
    {
        /// <summary>采集当前交握状态快照（只读，不改状态机）</summary>
        HandoverStateSnapshot GetSnapshot();
    }
}
