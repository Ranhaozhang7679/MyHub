using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Luster.Module.Motion.TestToolchain.Manual
{
    /// <summary>
    /// 轴组手动操作记录（TES-34 P9-B，迁移自源端 ManualMotorGroup）。
    /// 记录多轴点动前的位置 <see cref="LastPosi"/>（double[]），回退时移动回原位。
    /// </summary>
    /// <remarks>
    /// <b>对齐源端</b>：源端 ManualMotorGroup.Backup 调
    /// <c>((MotorGroupComponent)Component).MoveToPosition(new List&lt;double&gt;(LastPosi), false, out _)</c>。
    /// lmv 侧回退委托由调用方绑定 MultiAxis 多轴移动能力。
    /// </remarks>
    public class ManualMotorGroup : IManualOperation
    {
        private readonly string _key;
        private readonly double[] _lastPosi;
        private readonly Action<double[]> _restore;

        /// <param name="key">设备标识（用于去重）</param>
        /// <param name="lastPosi">操作前的各轴位置（mm）</param>
        /// <param name="restore">回退委托（传入 LastPosi 移动回原位，绑定 MultiAxis）</param>
        public ManualMotorGroup(string key, double[] lastPosi, Action<double[]> restore)
        {
            _key = key;
            _lastPosi = lastPosi;
            _restore = restore;
        }

        /// <summary>操作前各轴位置</summary>
        public double[] LastPosi => _lastPosi;

        /// <inheritdoc/>
        public string ComponentKey => _key;

        /// <inheritdoc/>
        public bool Backup(out string msg)
        {
            msg = ToDetailString();
            _restore?.Invoke(_lastPosi);
            return true;
        }

        /// <inheritdoc/>
        public string ToDetailString()
        {
            if (_lastPosi == null || _lastPosi.Length == 0) return "Posi:[]";
            StringBuilder sb = new StringBuilder("Posi:[");
            sb.Append(string.Join(",", _lastPosi.Select(p => p.ToString("F3", CultureInfo.InvariantCulture))));
            sb.Append("]");
            return sb.ToString();
        }
    }
}
