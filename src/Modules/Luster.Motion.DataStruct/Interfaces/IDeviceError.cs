using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 设备异常分类
    /// </summary>
    public interface IDeviceError
    {
        /// <summary>
        /// 隶属设备对应的ID
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 设备的异常分类
        /// </summary>
        DeviceError[] ErrorCodes { get; }

        /// <summary>
        /// 错误代码
        /// </summary>
        Dictionary<DeviceError, string> Errors { get; }

        string ErrorMessage { get; set; }
    }
}
