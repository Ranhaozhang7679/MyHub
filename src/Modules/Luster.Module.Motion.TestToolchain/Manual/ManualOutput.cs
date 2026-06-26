using System;

namespace Luster.Module.Motion.TestToolchain.Manual
{
    /// <summary>
    /// 数字输出手动操作记录（TES-34 P9-B，迁移自源端 ManualOutput）。
    /// 记录 IO 强制前的状态 <see cref="LastStatus"/>，回退时恢复。
    /// </summary>
    /// <remarks>
    /// <b>对齐源端</b>：源端 ManualOutput.Backup 调 <c>((IOutputDigital)Component).SetValue(this.LastStatus)</c>。
    /// lmv 侧回退委托由调用方绑定 VIO.SetDigital（复用 IOSimulation/SetIO 节点的 VIO 能力）。
    /// 源端 IOutputDigital 实现者仅 OutputComponent + LightControlComponent，本类不限定设备类型。
    /// </remarks>
    public class ManualOutput : IManualOperation
    {
        private readonly string _key;
        private readonly bool _lastStatus;
        private readonly Action<bool> _restore;

        /// <param name="key">设备标识（用于去重）</param>
        /// <param name="lastStatus">操作前的数字输出状态</param>
        /// <param name="restore">回退委托（传入 LastStatus 恢复，绑定 VIO.SetDigital）</param>
        public ManualOutput(string key, bool lastStatus, Action<bool> restore)
        {
            _key = key;
            _lastStatus = lastStatus;
            _restore = restore;
        }

        /// <summary>操作前状态</summary>
        public bool LastStatus => _lastStatus;

        /// <inheritdoc/>
        public string ComponentKey => _key;

        /// <inheritdoc/>
        public bool Backup(out string msg)
        {
            msg = $"Status:{_lastStatus}";
            _restore?.Invoke(_lastStatus);
            return true;
        }

        /// <inheritdoc/>
        public string ToDetailString() => $"Status:{_lastStatus}";
    }
}
