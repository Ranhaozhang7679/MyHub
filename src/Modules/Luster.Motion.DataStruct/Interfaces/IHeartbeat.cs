using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 心跳检查
    /// </summary>
    public interface IHeartbeat
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 心跳是否正常
        /// </summary>
        /// <returns></returns>
        bool IsHealth(out string connectStr);

        /// <summary>
        /// 通信连接
        /// </summary>
        string ConnectStr { get; }
    }
}
