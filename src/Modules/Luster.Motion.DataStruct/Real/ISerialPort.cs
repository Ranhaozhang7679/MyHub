using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Luster.Motion.DataStruct.Real
{
    public interface ISerialPort : IDevice
    {
        /// <summary>
        /// 打开串口
        /// </summary>
        void Open();

        /// <summary>
        /// 读取
        /// </summary>
        void Read();
        /// <summary>
        /// 清零
        /// </summary>
        void Zero();
        /// <summary>
        /// 关闭串口
        /// </summary>
        void Close();
        /// <summary>
        /// 设置参数
        /// </summary>
        void SetParam(string port, int baudRate, int dataBits, StopBits stopbits, Parity parity);
    }
}
