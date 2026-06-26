using System;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// AOI 站点类型。
    /// 对应 SP-2025140 长盈 FQC 三套独立工程：AOI#1、AOI#2、Wipe。
    /// </summary>
    public enum AoiSiteType
    {
        /// <summary>未指定（用于校验“未设置”错误）。</summary>
        Unspecified = 0,

        /// <summary>AOI#1 站，金属外观检测主站。</summary>
        Aoi1 = 1,

        /// <summary>AOI#2 站，金属外观检测副站。</summary>
        Aoi2 = 2,

        /// <summary>Wipe 擦拭站。</summary>
        Wipe = 3,
    }
}
