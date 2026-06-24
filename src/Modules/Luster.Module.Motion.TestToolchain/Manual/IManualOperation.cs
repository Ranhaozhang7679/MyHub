namespace Luster.Module.Motion.TestToolchain.Manual
{
    /// <summary>
    /// 手动操作记录项（TES-34 P9-B，迁移自源端 CommonMachineModelLibrary ManualBase）。
    /// 记录一次手动操作的目标设备与操作前状态，供 <see cref="ManualStack"/> 回退（Undo）。
    /// </summary>
    /// <remarks>
    /// <b>设计</b>：与具体设备类型解耦——Backup 通过调用方注入的回退委托执行
    /// （IO 回退→VIO.SetDigital、单轴回退→VAxis.MoveAbs、多轴→MultiAxis），由调用方绑定
    /// IOSimulation/SingleAxis/MultiAxis 节点能力。本接口本身纯逻辑，便于单测。
    /// <b>对齐源端</b>：ComponentKey 用于 <see cref="ManualStack"/> 同设备去重（源端 AddManual 折叠连续同设备操作）；
    /// Backup 返回 false 表示回退失败需中止出栈（源端 RemoveLast 契约）。
    /// </remarks>
    public interface IManualOperation
    {
        /// <summary>目标设备标识（用于同设备去重）</summary>
        string ComponentKey { get; }

        /// <summary>
        /// 回退到操作前状态。
        /// </summary>
        /// <param name="msg">回退明细（如 Status:True / Posi:12.3）</param>
        /// <returns>true=回退成功；false=回退失败，应中止出栈</returns>
        bool Backup(out string msg);

        /// <summary>操作状态明细（供 UI 列表显示）</summary>
        string ToDetailString();
    }
}
