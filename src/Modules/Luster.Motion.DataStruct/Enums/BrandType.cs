using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    /// <summary>
    /// 厂家
    /// </summary>
    public enum BrandType
    {
        [Description("默认厂家")]
        Default,
        [Description("三菱")]
        Mitsubishi,
        [Description("基恩士")]
        Keyence,
        [Description("欧姆龙")]
        Omron,
        [Description("汇川")]
        Inovance,
        [Description("西门子")]
        Siemens,
        [Description("施耐德")]
        Schneider,
        [Description("倍福")]
        Beckhoff
    }
}
