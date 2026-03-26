using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 设备屏蔽接口
    /// </summary>
    public interface IDeviceShield
    {
        /// <summary>
        /// 是否屏蔽
        /// </summary>
        bool IsShield { get; set; }
    }
}
