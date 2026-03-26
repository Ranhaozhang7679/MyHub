using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    /// <summary>
    /// 数据转移
    /// </summary>
    public enum DataTransType
    {
        [Description("无")]
        None,

        [Description("获取")]
        Get,

        [Description("转移")]
        Transfer
    }
}
