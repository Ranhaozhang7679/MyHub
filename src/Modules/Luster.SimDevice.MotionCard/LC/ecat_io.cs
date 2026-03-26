using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Luster.SimDevice.MotionCard.LC
{
    class Ecat_io
    {

        private const string DllPath = @"ecat_io.dll";
        #region ecat_io接口中用到的结构体

        /// <summary>
        /// 总线上的资源
        /// </summary>
        public struct EC_RES
        {
            public int SlaveNum;    //从站个数
            public int DiNum;       //数字量输入通道数
            public int DoNum;       //数字量输出通道数
            public int AiNum;       //模拟量输入通道数
            public int AoNum;       //模拟量输出通道数
        };

        /// <summary>
        /// 从站的资源
        /// </summary>
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct EC_SLAVE_INFO
        {
            public int VendorID;        //厂家ID
            public int ProductCode;     //产品编号
            public int RevisionNo;      //版本号
            public int ModuleNum;       //该从站的模块数量
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 32, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I4)]
            public int[] ModuleId;      //模块ID int[32]
            public int DiNum;           //该从站的数字量输入通道数
            public int DoNum;           //该从站的数字量输出通道数
            public int AiNum;           //该从站的模拟量输入通道数
            public int AoNum;           //该从站的模拟量输出通道数
        };

        #endregion

        /// <summary>
        /// 打开IO主站卡，使用其它指令前必须先打开设备
        /// </summary>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <param name="param">参数，保留</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Open(short card, short param);

        /// <summary>
        /// 关闭IO主站卡，退出应用程序之前须关闭设备 。
        /// </summary>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Close(short card);

        [DllImport(DllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_GetVersion(out char pVersion, short size, short card);

        /// <summary>
        /// 获取网络中的从站资源
        /// </summary>
        /// <param name="pResScan">扫描到的从站资源</param>
        /// <param name="pResOnline">连接之后在线的从站</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_GetSlaveResource(out EC_RES pResScan, out EC_RES pResOnline, short card);

        /// <summary>
        /// 获取从站信息。
        /// </summary>
        /// <param name="pInfo">从站信息</param>
        /// <param name="slaveNo">从站号，从1开始</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_GetSlaveInfo(out EC_SLAVE_INFO pInfo, short slaveNo, short card);

        /// <summary>
        /// 打开IO主站卡，使用其它指令之前必须先打开设备
        /// </summary>
        /// <param name="eniPath">指定ENI文件加载路径</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_LoadEni(string eniPath, short card);

        /// <summary>
        /// 与从站建立通讯，进入OP状态。
        /// </summary>
        /// <param name="option">断线重连模式，1输出保持断线前状态，0不保持</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_ConnectECAT(short option, short card);

        /// <summary>
        /// 断开与从站的通讯 退出OP状态 。
        /// </summary>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_DisconnectECAT(short card);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_EcatSDODownload(short slaveNo, short index, short subindex, short data_size, uint data, short card);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_EcatSDOUpload(short slaveNo, short index, short subindex, short data_size, out uint pBuf, short count, short card);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_SetPdoData(short slaveNo, short moduleNo, short index, short subindex, short data_size, uint data, short card);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_GetPdoData(short slaveNo, short moduleNo, short index, short subindex, short data_size, out uint pData, short card);

        /// <summary>
        /// 设置数字量输出通道状态
        /// </summary>
        /// <param name="slaveNo">从站号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="value">输出状态，取值0或者1</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Set_Digital_Chn_Out(short slaveNo, short channel, short value, short card);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Set_Digital_Port_Out(short slaveNo, short chnBegin, uint lValue, uint lMask, short card);

        /// <summary>
        /// 获取数字量输出通道状态
        /// </summary>
        /// <param name="slaveNo">从站号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="value">输出状态，取值0或者1</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Get_Digital_Chn_Out(short slaveNo, short channel, ref short value, short card);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Get_Digital_Port_Out(short slaveNo, short chnBegin, ref uint lValue, short card);

        /// <summary>
        /// 读取数字量输入通道状态
        /// </summary>
        /// <param name="slaveNo">从站号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="value">输入状态，取值0或者1</param>
        /// <param name="value">输入状态，取值0或者1</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Get_Digital_Chn_In(short slaveNo, short channel, ref short value, short card);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Get_Digital_Port_In(short slaveNo, short chnBegin, ref uint lValue, short card);

        /// <summary>
        /// 设置模拟量输出值
        /// </summary>
        /// <param name="slaveNo">从站号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">输出值缓冲区，可以同时多个通道输出</param>
        /// <param name="count">输出通道的个数</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Set_Analog_Chn_Out(short slaveNo, short channel, out short pValue, short count, short card);

        /// <summary>
        /// 获取模拟量输出值
        /// </summary>
        /// <param name="slaveNo">从站号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">数据缓冲区，可以同时获取多个通道输出值</param>
        /// <param name="count">需要获取的输出通道的个数</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Get_Analog_Chn_Out(short slaveNo, short channel, out short pValue, short count, short card);

        /// <summary>
        /// 读取模拟量输入值
        /// </summary>
        /// <param name="slaveNo">从站号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">数据缓冲区，可以同时获取多个通道输入值</param>
        /// <param name="count">需要获取的输入通道的个数</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern short EC_Get_Analog_Chn_In(short slaveNo, short channel, out short pValue, short count, short card);


    }
}
