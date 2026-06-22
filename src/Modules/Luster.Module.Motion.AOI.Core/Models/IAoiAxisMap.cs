using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// 三站差异中的轴名 → 卡轴号映射。
    /// X/Y/Z/U/V 必填，缺失任意一项均视为站点配置不完整，校验阶段直接拦截。
    /// </summary>
    public interface IAoiAxisMap
    {
        /// <summary>X 轴名称。</summary>
        string XAxisName { get; }

        /// <summary>Y 轴名称。</summary>
        string YAxisName { get; }

        /// <summary>Z 轴名称。</summary>
        string ZAxisName { get; }

        /// <summary>U 轴名称（俯仰）。</summary>
        string UAxisName { get; }

        /// <summary>V 轴名称（旋转）。</summary>
        string VAxisName { get; }

        /// <summary>所有命名轴 → 卡轴号。</summary>
        IReadOnlyDictionary<string, int> AxisChannelMap { get; }
    }
}
