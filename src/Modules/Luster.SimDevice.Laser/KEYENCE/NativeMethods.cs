#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NativeMethods
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.Laser.KEYENCE
* 文 件 名:       NativeMethods.cs
* 创建时间:       2022/11/11 15:40:12
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      eb37c6eb-a8d1-4f3e-83b6-b16eeddc0d30
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/11 15:40:12
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Laser.KEYENCE
{
    #region Enum

    /// <summary>
    ///返回值定义
    /// </summary>
    public enum Rc
    {
        [Description("成功")]
        Ok = 0x0000,
        [Description("设备打开失败")]
        ErrOpenDevice = 0x1000,
        [Description("无设备")]
        ErrNoDevice,
        [Description("命令发送失败")]
        ErrSend,
        [Description("响应接受失败")]
        ErrReceive,
        [Description("超时")]
        ErrTimeout,
        [Description("无空余空间")]
        ErrNomemory,
        [Description("参数传输错误")]
        ErrParameter,
        [Description("非法的反馈数据")]
        ErrRecvFmt,
        [Description("高速通信初始化失败")]
        ErrHispeedNoDevice = 0x1009,
        [Description("高速通信初始化完毕")]
        ErrHispeedOpenYet,
        [Description("高速通信发生错误")]
        ErrHispeedRecvYet,
        [Description("引数传输的缓存大小不足")]
        ErrBufferShort,
    }

    /// <summary>
    /// 设置参数类型
    /// </summary>
    public enum SettingType : byte
    {
        [Description("环境参数")]
        Environment = 0x01,
        [Description("常规参数")]
        Common = 0x02,
        [Description("配置文件序号0")]
        Program00 = 0x10,
        Program01,
        Program02,
        Program03,
        Program04,
        Program05,
        Program06,
        Program07,
        Program08,
        Program09,
        Program10,
        Program11,
        Program12,
        Program13,
        Program14,
        Program15,
    };


    /// <summary>
    /// 获取批次配置文件位置规范方法指定
    /// </summary>
    public enum LJX8IF_BATCH_POSITION : byte
    {
        [Description("从当前开始")]
        LJX8IF_BATCH_POSITION_CURRENT = 0x00,
        [Description("指定位置")]
        LJX8IF_BATCH_POSITION_SPEC = 0x02,
        [Description("从确定批次后的最新开始")]
        LJX8IF_BATCH_POSITION_COMMITED = 0x03,
        [Description("单个最新")]
        LJX8IF_BATCH_POSITION_CURRENT_ONLY = 0x04,
    };

    /// <summary>
    /// 指定将发送的设定值反映到哪一级别
    /// </summary>
    public enum LJX8IF_SETTING_DEPTH : byte
    {
        [Description("设定写入区域")]
        Write = 0x00,
        [Description("运行设定区域")]
        Running = 0x01,
        [Description("保存区域")]
        Save = 0x02,
    };


    /// <summary>
    /// 内存分配
    /// </summary>
    public enum LJX8IF_PROFILE_BANK : byte
    {
        [Description("活动区域")]
        LJX8IF_PROFILE_BANK_ACTIVE = 0x00,
        [Description("非活动区域")]
        LJX8IF_PROFILE_BANK_INACTIVE = 0x01,
    };

    /// <summary>
    /// 指定轮廓获取位置的指定方法
    /// </summary>
    public enum LJX8IF_PROFILE_POSITION : byte
    {
        [Description("从当前开始")]
        LJX8IF_PROFILE_POSITION_CURRENT = 0x00,
        [Description("从最早的开始")]
        LJX8IF_PROFILE_POSITION_OLDEST = 0x01,
        [Description("指定位置")]
        LJX8IF_PROFILE_POSITION_SPEC = 0x02,
    };

    #endregion

    #region Structure
    /// <summary>
    /// 版本信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_VERSION_INFO
    {
        public int nMajorNumber;
        public int nMinorNumber;
        public int nRevisionNumber;
        public int nBuildNumber;
    };

    /// <summary>
    /// 网口
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_ETHERNET_CONFIG
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] abyIpAddress;
        public ushort wPortNo;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserve;

    };

    /// <summary>
    /// 发送 / 接收设定项目详情
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_TARGET_SETTING
    {
        public byte byType;
        public byte byCategory;
        public byte byItem;
        public byte reserve;
        public byte byTarget1;
        public byte byTarget2;
        public byte byTarget3;
        public byte byTarget4;
    };

    //参数设置与获取
    enum LJX8IF_SETTING_ITEM
    {

        TRIG_MODE = 0x0001,//触发模式
        SAMPLED_CYCLE = 0x0002,//采样周期
        BATCH_ON_OFF = 0x0003,//批处理开关
        ENCODER_TYPE = 0x0007,//编码器类型

        REFINING_POINTS = 0x0009,//细化点数
        BATCH_POINT = 0x000A,//批处理点数

        CYCLICAL_PATTERN = 0x0010,//循环模式 0 关闭 1 打开
        Z_MEASURING_RANGE = 0x0103,//Z方向测量范围

        SENSITIVITY = 0x0105,//感光灵敏度
        EXP_TIME = 0x0106,//曝光时间
        LIGHT_CONTROL = 0x010B,//光亮控制 
        LIGHT_MAX = 0x010C,//激光亮度上限
        LIGHT_MIN = 0x010D,//激光亮度下限
        PEAK_SENSITIVITY = 0x010F,//峰值灵敏度
        PEAK_SELECT = 0x0111,  //峰值选择

        X_SAMPLING = 0x0202,   //X轴压缩设定

        FILTER_X_MEDIAN = 0x020A,  //X轴中位数滤波
        FILTER_X_SMOOTH = 0x020B,  //X轴平滑滤波
        FILTER_Y_MEDIAN = 0x020C,  //Y轴中位数滤波
        FILTER_Y_SMOOTH = 0x020D,  //Y轴平滑滤波

        CHANGE_3D_25D = 0x3000,    //3D/2.5D切换 2.5模式下X轴压缩设定为 自动变更默认值.

        X_PIXEL = 0x3001,          //X数据宽度(单位像素)
        X_PITCH = 0x3002      //X Resolution

    }

    //曝光时间
    enum LJX8IF_EXP_TIME
    {
        T10US = 0,
        T15US = 1,
        T30US = 2,
        T60US = 3,
        T120US = 4,
        T240US = 5,
        T480US = 6,
        T960US = 7,
        T1920US = 8,
        T2400US = 9,
        T4900US = 10,
        T9800US = 11,

    }

    //帧率参数
    enum LJX8IF_Frame
    {
        F10Hz = 0,
        F20Hz = 1,
        F50Hz = 2,
        F100Hz = 3,
        F200Hz = 4,
        F500Hz = 5,
        F1000Hz = 6,
        F2000Hz = 7,
        F4000Hz = 8,
        F8000Hz = 9,
        F16000Hz = 10,
        F1500Hz = 11,
        F2500Hz = 12,
        F3000Hz = 13,
        F3500Hz = 14,
        F4500Hz = 15,
        F5000Hz = 16,
        F6000Hz = 17,
        F7000Hz = 18,
        F12000Hz = 19,
    }


    public enum LJX8IF_StartTriggerMode
    {
        [Description("软触发")]
        Soft = 0,
        [Description("硬件")]
        IO = 1
    }

    /*******接口函数返回值**********/
    enum LJX8IF_ERROR
    {
        SR7IF_ERROR_NOT_FOUND = (-999),                  // Item is not found.
        SR7IF_ERROR_COMMAND = (-998),                  // Command not recognized.
        SR7IF_ERROR_PARAMETER = (-997),                  // Parameter is invalid.
        SR7IF_ERROR_UNIMPLEMENTED = (-996),                  // Feature not implemented.
        SR7IF_ERROR_HANDLE = (-995),                  // Handle is invalid.
        SR7IF_ERROR_MEMORY = (-994),                  // Out of memory.
        SR7IF_ERROR_TIMEOUT = (-993),                  // Action timed out.
        SR7IF_ERROR_DATABUFFER = (-992),                  // Buffer not large enough for data.
        SR7IF_ERROR_STREAM = (-991),                  // Error in stream.
        SR7IF_ERROR_CLOSED = (-990),                  // Resource is no longer avaiable.
        SR7IF_ERROR_VERSION = (-989),                  // Invalid version number.
        SR7IF_ERROR_ABORT = (-988),                  // Operation aborted.
        SR7IF_ERROR_ALREADY_EXISTS = (-987),                  // Conflicts with existing item.
        SR7IF_ERROR_FRAME_LOSS = (-986),                  // Loss of frame.
        SR7IF_ERROR_ROLL_DATA_OVERFLOW = (-985),                  // Continue mode Data overflow.
        SR7IF_ERROR_ROLL_BUSY = (-984),                  // Read Busy.
        SR7IF_ERROR_MODE = (-983),                  // Err mode.
        SR7IF_ERROR_CAMERA_NOT_ONLINE = (-982),                  // Camera not online.
        SR7IF_ERROR = (-1),                    // General error.
        SR7IF_OK = (0),                     // Operation successful.
        SR7IF_NORMAL_STOP = (-100)                //A normal stop caused by external IO or other causes

    }


    //触发模式
    enum LJX8IF_TRIG_MODE
    {
        CONTINUE = 0,
        EXT_TRIGGER = 1,
        ENCODER = 2,
    }

    //采集周期
    //100 200 400 600 1000 1500 2000 2500

    //批处理开关
    enum LJX8IF_BATCH_ON_OFF
    {
        OFF = 0,
        ON = 1
    }

    //编码器类型
    enum LJX8IF_ENCODER_TYPE
    {
        E_1_1 = 0,//0：1 相 1 递增；
        E_2_1 = 1,//1：2 相 1 递增；
        E_2_2 = 2,//2：2 相 2 递增；
        E_2_4 = 3 //3：2 相 4 递增；
    }

    //细化点数   1 -- 
    //批处理点数 1 -- 

    //循环模式 0 关闭 1 打开
    enum LJX8IF_CYCLICAL_PATTERN
    {
        CLOSE = 0,
        OPEN = 1
    }

    //Z方向测量范围 注：只支持SR8020/SR8060
    enum LJX8IF_Z_MEASURING_RANGE
    {
        Z840 = 0,
        Z768 = 1,
        Z512 = 2,
        Z384 = 3,
        Z256 = 4,
        Z192 = 5,
        Z128 = 6,
        Z96 = 7,
        Z64 = 8,
        Z48 = 9,
        Z32 = 10
    };

    //感光灵敏度
    enum LJX8IF_SENSITIVITY
    {
        HIGH = 0,
        HIGH_RANGE_1 = 1,
        HIGH_RANGE_2 = 2,
        HIGH_RANGE_3 = 3,
        HIGH_RANGE_4 = 4,
        CUSTOMIZATION = 5
    }

    //光亮控制 
    enum LJX8IF_LIGHT_CONTROL
    {
        AUTO = 0,
        MAN = 1
    }

    //激光亮度上限 0-99
    //激光亮度下限 0-99

    //峰值灵敏度
    enum LJX8IF_PEAK_SENSITIVITY
    {
        N_1 = 1,
        N_2 = 2,
        N_3 = 3,
        N_4 = 4,
        N_5 = 5
    }

    //峰值选择
    enum LJX8IF_PEAK_SELECT
    {
        STANDARD = 0,
        NEAR = 1,
        FAR = 2,
        BE_NULL = 3,
        CONTINUE = 4,
        GLUE = 5

    }

    //X轴压缩设定 注：2.5D 模式下不能设置
    enum LJX8IF_X_SAMPLING
    {
        OFF = 1,
        X2 = 2,
        X4 = 4,
        X8 = 8,
        X16 = 16
    }

    //X轴中位数滤波
    enum LJX8IF_FILTER_X_MEDIAN
    {
        OFF = 1,
        N3 = 3,
        N5 = 5,
        N7 = 7,
        N9 = 9
    }

    //Y轴中位数滤波
    enum LJX8IF_FILTER_Y_MEDIAN
    {
        OFF = 1,
        N3 = 3,
        N5 = 5,
        N7 = 7,
        N9 = 9
    }

    //X轴平滑滤波
    enum LJX8IF_FILTER_X_SMOOTH
    {

        N1 = 1,
        N2 = 2,
        N4 = 4,
        N8 = 8,
        N16 = 16,
        N32 = 32,
        N64 = 64

    }

    //Y轴平滑滤波
    enum LJX8IF_FILTER_Y_SMOOTH
    {

        N1 = 1,
        N2 = 2,
        N4 = 4,
        N8 = 8,
        N16 = 16,
        N32 = 32,
        N64 = 64,
        N128 = 128,
        N256 = 256

    }

    /// <summary>
    /// 轮廓信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_PROFILE_INFO
    {
        public byte byProfileCount;
        public byte reserve1;
        public byte byLuminanceOutput;
        public byte reserve2;
        public short nProfileDataCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserve3;
        public int lXStart;
        public int lXPitch;
    };

    /// <summary>
    /// 轮廓信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_PROFILE_HEADER
    {
        public uint reserve;
        public uint dwTriggerCount;//表示轮廓是由测量开始后的第几次触发获得的
        public int lEncoderCount;//触发发送时的编码器计数。（编码器计数）从 “0” 开始计数
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public uint[] reserve2;
    };

    /// <summary>
    /// 轮廓页脚信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_PROFILE_FOOTER
    {
        public uint reserve;
    };

    /// <summary>
    /// 获取轮廓时数据信息（批量处理：关）
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_GET_PROFILE_REQUEST
    {
        public byte byTargetBank;
        public byte byPositionMode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserve;
        public uint dwGetProfileNo;
        public byte byGetProfileCount;
        public byte byErase;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserve2;
    };

    /// <summary>
    /// 获取轮廓时数据信息（批量处理：开）
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_GET_BATCH_PROFILE_REQUEST
    {
        public byte byTargetBank;
        public byte byPositionMode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserve;
        public uint dwGetBatchNo;
        public uint dwGetProfileNo;
        public byte byGetProfileCount;
        public byte byErase;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserve2;
    };

    /// <summary>
    ///获取轮廓返回信息（批量处理：关）
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_GET_PROFILE_RESPONSE
    {
        public uint dwCurrentProfileNo;
        public uint dwOldestProfileNo;
        public uint dwGetTopProfileNo;
        public byte byGetProfileCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] reserve;
    };

    /// <summary>
    ///获取轮廓返回信息（批量处理：开）
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_GET_BATCH_PROFILE_RESPONSE
    {
        public uint dwCurrentBatchNo;
        public uint dwCurrentBatchProfileCount;
        public uint dwOldestBatchNo;
        public uint dwOldestBatchProfileCount;
        public uint dwGetBatchNo;
        public uint dwGetBatchProfileCount;
        public uint dwGetBatchTopProfileNo;
        public byte byGetProfileCount;
        public byte byCurrentBatchCommited;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] reserve;
    };

    /// <summary>
    /// 高速通信启动准备请求结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LJX8IF_HIGH_SPEED_PRE_START_REQUEST
    {
        public byte bySendPosition;     // 发送起始位置
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] reserve;     
    };

    #endregion

    #region Method
    /// <summary>
    /// 用于高速通信的回调功能
    /// </summary>
    /// <param name="pBuffer">返回轮廓数据</param>
    /// <param name="dwSize">一条轮廓的 byte 尺寸 </param>
    /// <param name="dwCount">轮廓线数</param>
    /// <param name="dwNotify">定案条件？？？</param>
    /// <param name="dwUser">线程 ID</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HighSpeedDataCallBack(IntPtr pBuffer, uint dwSize, uint dwCount, uint dwNotify, uint dwUser);

    /// <summary>
    /// 用于高速通信的回调功能（返回数组指针数据）
    /// </summary>
    /// <param name="pProfileHeaderArray">头文件</param>
    /// <param name="pHeightProfileArray">轮廓高度值</param>
    /// <param name="pLuminanceProfileArray">轮廓亮度值</param>
    /// <param name="dwLuminanceEnable">亮度数据是否有效</param>
    /// <param name="dwProfileDataCount">一条轮廓数据量</param>
    /// <param name="dwCount">轮廓数量</param>
    /// <param name="dwNotify">定案条件？？？</param>
    /// <param name="dwUser">线程 ID</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void HighSpeedDataCallBackForSimpleArray(IntPtr pProfileHeaderArray, IntPtr pHeightProfileArray, IntPtr pLuminanceProfileArray, uint dwLuminanceEnable, uint dwProfileDataCount, uint dwCount, uint dwNotify, uint dwUser);

    /// <summary>
    /// 通用方法
    /// </summary>
    internal class NativeMethod
    {
        /// <summary>
        /// 设备数量
        /// </summary>
        internal static int DeviceCount
        {
            get { return 6; }
        }

        /// <summary>
        /// 环境设置数据字节的固定值
        /// </summary>
        internal static UInt32 EnvironmentSettingSize
        {
            get { return 60; }
        }

        /// <summary>
        /// 常用测量设置数据字节的固定值
        /// </summary>
        internal static UInt32 CommonSettingSize
        {
            get { return 20; }
        }

        /// <summary>
        /// 程序大小设置（byte）
        /// </summary>
        internal static UInt32 ProgramSettingSize
        {
            get { return 10980; }
        }

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_Initialize();

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_Finalize();

        [DllImport("LJX8_IF.dll")]
        internal static extern LJX8IF_VERSION_INFO LJX8IF_GetVersion();

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_EthernetOpen(int lDeviceId, ref LJX8IF_ETHERNET_CONFIG pEthernetConfig);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_CommunicationClose(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_RebootController(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_ReturnToFactorySetting(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_ControlLaser(int lDeviceId, byte byState);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetError(int lDeviceId, byte byReceivedMax, ref byte pbyErrCount, IntPtr pwErrCode);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_ClearError(int lDeviceId, short wErrCode);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_TrgErrorReset(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetTriggerAndPulseCount(int lDeviceId, ref uint pdwTriggerCount, ref int plEncoderCount);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_SetTimerCount(int lDeviceId, uint dwTimerCount);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetTimerCount(int lDeviceId, ref uint pdwTimerCount);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetHeadTemperature(int lDeviceId, ref short pnSensorTemperature, ref short pnProcessorTemperature, ref short pnCaseTemperature);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetHeadModel(int lDeviceId, IntPtr pHeadModel);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetSerialNumber(int lDeviceId, IntPtr pControllerSerialNo, IntPtr pHeadSerialNo);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetAttentionStatus(int lDeviceId, ref ushort pwAttentionStatus);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_Trigger(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_StartMeasure(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_StopMeasure(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_ClearMemory(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_SetSetting(int lDeviceId, byte byDepth, LJX8IF_TARGET_SETTING targetSetting, IntPtr pData, uint dwDataSize, ref uint pdwError);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetSetting(int lDeviceId, byte byDepth, LJX8IF_TARGET_SETTING targetSetting, IntPtr pData, uint dwDataSize);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_InitializeSetting(int lDeviceId, byte byDepth, byte byTarget);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_ReflectSetting(int lDeviceId, byte byDepth, ref uint pdwError);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_RewriteTemporarySetting(int lDeviceId, byte byDepth);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_CheckMemoryAccess(int lDeviceId, ref byte pbyBusy);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_ChangeActiveProgram(int lDeviceId, byte byProgramNo);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetActiveProgram(int lDeviceId, ref byte pbyProgramNo);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_SetXpitch(int lDeviceId, uint dwXpitch);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetXpitch(int lDeviceId, ref uint pdwXpitch);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetProfile(int lDeviceId, ref LJX8IF_GET_PROFILE_REQUEST pReq,
        ref LJX8IF_GET_PROFILE_RESPONSE pRsp, ref LJX8IF_PROFILE_INFO pProfileInfo, IntPtr pdwProfileData, uint dwDataSize);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetBatchProfile(int lDeviceId, ref LJX8IF_GET_BATCH_PROFILE_REQUEST pReq,
        ref LJX8IF_GET_BATCH_PROFILE_RESPONSE pRsp, ref LJX8IF_PROFILE_INFO pProfileInfo,
        IntPtr pdwBatchData, uint dwDataSize);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_GetBatchSimpleArray(int lDeviceId, ref LJX8IF_GET_BATCH_PROFILE_REQUEST pReq,
        ref LJX8IF_GET_BATCH_PROFILE_RESPONSE pRsp, ref LJX8IF_PROFILE_INFO pProfileInfo,
        IntPtr pProfileHeaderArray, IntPtr pHeightProfileArray, IntPtr pLuminanceProfileArray);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_InitializeHighSpeedDataCommunication(
        int lDeviceId, ref LJX8IF_ETHERNET_CONFIG pEthernetConfig, ushort wHighSpeedPortNo,
        HighSpeedDataCallBack pCallBack, uint dwProfileCount, uint dwThreadId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_InitializeHighSpeedDataCommunicationSimpleArray(
        int lDeviceId, ref LJX8IF_ETHERNET_CONFIG pEthernetConfig, ushort wHighSpeedPortNo,
        HighSpeedDataCallBackForSimpleArray pCallBackSimpleArray, uint dwProfileCount, uint dwThreadId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_PreStartHighSpeedDataCommunication(
        int lDeviceId, ref LJX8IF_HIGH_SPEED_PRE_START_REQUEST pReq,
        ref LJX8IF_PROFILE_INFO pProfileInfo);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_StartHighSpeedDataCommunication(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_StopHighSpeedDataCommunication(int lDeviceId);

        [DllImport("LJX8_IF.dll")]
        internal static extern int LJX8IF_FinalizeHighSpeedDataCommunication(int lDeviceId);
    }
    #endregion

    #region

    /// <summary>
    /// Object pinning class
    /// </summary>
    public sealed class PinnedObject : IDisposable
    {
        #region Field

        private GCHandle _handle;      // Garbage collector handle

        #endregion

        #region Property

        /// <summary>
        /// Get the address.
        /// </summary>
        public IntPtr Pointer
        {
            // Get the leading address of the current object that is pinned.
            get { return _handle.AddrOfPinnedObject(); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="target">Target to protect from the garbage collector</param>
        public PinnedObject(object target)
        {
            // Pin the target to protect it from the garbage collector.
            _handle = GCHandle.Alloc(target, GCHandleType.Pinned);
        }

        #endregion

        #region Interface
        /// <summary>
        /// Interface
        /// </summary>
        public void Dispose()
        {
            _handle.Free();
            _handle = new GCHandle();
        }

        #endregion
    }

    #endregion
}