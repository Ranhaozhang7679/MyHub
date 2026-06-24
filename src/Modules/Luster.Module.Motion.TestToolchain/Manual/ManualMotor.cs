using System;

namespace Luster.Module.Motion.TestToolchain.Manual
{
    /// <summary>
    /// 单轴手动操作记录（TES-34 P9-B，迁移自源端 ManualMotor）。
    /// 记录轴点动前的位置 <see cref="LastPosi"/>，回退时移动回原位。
    /// </summary>
    /// <remarks>
    /// <b>对齐源端</b>：源端 ManualMotor.Backup 调
    /// <c>((MotorComponent)Component).MoveToPosition(this.LastPosi, false, true)</c>——
    /// 第三参 manual=true 标记回退运动为手动模式。lmv 侧回退委托由调用方绑定
    /// VAxis.MoveAbs（复用 SingleAxis 节点的 VAxis 能力），manual 标志由调用方按需传入。
    /// </remarks>
    public class ManualMotor : IManualOperation
    {
        private readonly string _key;
        private readonly double _lastPosi;
        private readonly Action<double> _restore;

        /// <param name="key">设备标识（用于去重）</param>
        /// <param name="lastPosi">操作前的轴位置（mm）</param>
        /// <param name="restore">回退委托（传入 LastPosi 移动回原位，绑定 VAxis.MoveAbs）</param>
        public ManualMotor(string key, double lastPosi, Action<double> restore)
        {
            _key = key;
            _lastPosi = lastPosi;
            _restore = restore;
        }

        /// <summary>操作前位置</summary>
        public double LastPosi => _lastPosi;

        /// <inheritdoc/>
        public string ComponentKey => _key;

        /// <inheritdoc/>
        public bool Backup(out string msg)
        {
            msg = $"Posi:{_lastPosi:F3}";
            _restore?.Invoke(_lastPosi);
            return true;
        }

        /// <inheritdoc/>
        public string ToDetailString() => $"Posi:{_lastPosi:F3}";
    }
}
