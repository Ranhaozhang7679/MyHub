using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices;

namespace Luster.SimDevice.MotionCard.LC
{
    /// <summary>
    /// 大寰 给的最新LC的api
    /// </summary>
    public class ecat_motion2
    {

        public enum Scop_Sample_Mode
        {
            SCOPE_SAMPLE_MODE_CONTINUE = 0,         //连续采集
            SCOPE_SAMPLE_MODE_SINGLE = 1         //单次采集
        }

        public enum Scop_Status
        {

            SCOPE_STATUS_IDLE = 0,    //未触发采样
            SCOPE_STATUS_SAMPLING = 1,     //采样中
            SCOPE_STATUS_DONE = 2,      //采样结束
            SCOPE_STATUS_ERROR = 3,       //采样出错
        }
        public enum Scop_Sample_Depth
        {
            SCOPE_SAMPLE_DEPTH_1K = 1000,
            SCOPE_SAMPLE_DEPTH_2K = 2000,
            SCOPE_SAMPLE_DEPTH_4K = 4000,
            SCOPE_SAMPLE_DEPTH_8K = 8000

        }

        public enum Scop_Trig_Mode
        {
            SCOPE_TRIG_MODE_NONE = 0,   //无触发,立即采集
            SCOPE_TRIG_MODE_RISING_EDGE = 1,//上升沿触发
            SCOPE_TRIG_MODE_FALLING_EDGE = 2, //下降沿触发
            SCOPE_TRIG_MODE_BOTH_EDGE = 3         //上升/下降沿触发

        }

        public enum Scop_Sample_Status
        {
            SCOPE_SAMPLE_STS_IDLE = 0,         //空闲
            SCOPE_SAMPLE_STS_WAITING = 1,     //等待触发        
            SCOPE_SAMPLE_STS_RUNNING = 2,       //采样中
            SCOPE_SAMPLE_STS_FINISH = 3,       //采样结束#define 
            SCOPE_SAMPLE_STS_ERROR = 4           //发生错误#define 
        }

        public enum Scop_Watch_Type
        {
            SCOPE_WATCH_NULL = 0,
            SCOPE_WATCH_AXIS_CMD_POS = 1,
            SCOPE_WATCH_AXIS_CMD_VEL = 2,
            SCOPE_WATCH_AXIS_CMD_ACC = 3,
            SCOPE_WATCH_AXIS_ENC_POS = 4,
            SCOPE_WATCH_AXIS_ENC_VEL = 5,
            SCOPE_WATCH_AXIS_STATUS = 6,
            SCOPE_WATCH_CRD_STATUS = 7,
            SCOPE_WATCH_DIGITAL_CHN_IN = 20,
            SCOPE_WATCH_DIGITAL_PORT_IN8 = 21,
            SCOPE_WATCH_DIGITAL_PORT_IN16 = 22,
            SCOPE_WATCH_DIGITAL_CHN_OUT = 23,
            SCOPE_WATCH_DIGITAL_PORT_OUT8 = 24,
            SCOPE_WATCH_DIGITAL_PORT_OUT16 = 25,
            SCOPE_WATCH_ANALOG_IN16 = 26,
            SCOPE_WATCH_ANALOG_IN32 = 27,
            SCOPE_WATCH_ANALOG_OUT16 = 28,
            SCOPE_WATCH_ANALOG_OUT32 = 29,
            SCOPE_WATCH_VAR_IN8 = 30,
            SCOPE_WATCH_VAR_IN16 = 31,
            SCOPE_WATCH_VAR_IN32 = 32,
            SCOPE_WATCH_VAR_OUT8 = 33,
            SCOPE_WATCH_VAR_OUT16 = 34,
            SCOPE_WATCH_VAR_OUT32 = 35
        }

        public struct TScopeCfg
        {
            public ushort sampleMode;  //采样模式（单次或者连续），SCOPE_SAMPLE_MODE_xxx
            public ushort sampleDepth; //单次采样时的采样深度，SCOPE_SAMPLE_DEPTH_xxx
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] // 固定大小为 3 的
            public ushort[] reserved; //保留参数
            public ushort trigMode;    //采样触发模式，SCOPE_TRIG_MODE_xxx
            public ushort trigChn;     //触发通道，从0开始
            public short trigTime;    //触发时间，0表示立即采样，正数表示延迟几个周期
            public int trigLevel;   //触发比较值
        }



        #region ecat_motion接口中用到的结构体

        /*网络中的从站资源*/
        public struct SL_RES
        {
            public int SlaveNum;    //从站个数
            public int AxisNum; //伺服轴数
            public int IoSlaveNum; //IO从站数
            public int DiNum;       //数字量输入通道数
            public int DoNum;       //数字量输出通道数
            public int AiNum;       //模拟量输入通道数
            public int AoNum;       //模拟量输出通道数
            public int inVarNum;  //输入变量数
            public int outVarNum; //输出变量数
        }
        /*从站信息*/
        public struct SL_INFO
        {
            public uint VendorID;    //厂家ID
            public uint ProductCode; //产品编号
            public uint RevisionNo;  //版本号
            public int SlaveType;   //从站类型，0-伺服，3-IO，16-耦合器
            public int ModuleNum;   //从站的模块数量（从站为IO时有效）
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 32, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I4)]
            public int[] ModuleId;  //模块ID（从站为IO时有效）
        }

        public struct SL_IO_RES
        {
            public int DiNum;     //数字量输入通道数
            public int DoNum;     //数字量输出通道数
            public int AiNum;     //模拟量输入通道数
            public int AoNum;     //模拟量输出通道数
            public int inVarNum;  //输入变量数
            public int outVarNum; //输出变量数
        };

        /*点位模式运动参数*/
        public struct CmdPrm
        {
            public short sTime;
            public double acc;
            public double dec;

        };

        public struct ECAT_SM_INFO
        {
            public uint ulSm0StartAddress;  //地址
            public uint ulSm0DataSize;      //数据大小(bit)
            public uint ulSm0ControlByte;   //控制字
            public uint ulSm0Enable;        //使能

            public uint ulSm1StartAddress;  //地址
            public uint ulSm1DataSize;      //数据大小(bit)
            public uint ulSm1ControlByte;   //控制字
            public uint ulSm1Enable;        //使能
        };



        /*插补运动坐标系参数*/
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct CrdCfg
        {
            /// short
            public short dimension;
            /// short[8]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I2)]
            public short[] axis;
            /// short
            public short setOriginFlag;
            /// int[8]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I4)]
            public int[] orignPos;
            /// short
            public short evenTime;
            /// double
            public double synVelMax;
            /// double
            public double synAccMax;
            /// double
            public double synDecSmooth;
            /// double
            public double synDecAbrupt;
        }

        public struct CrdCfg_Unit
        {
            /// short
            public short dimension;
            /// short[8]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I2)]
            public short[] axis;
            /// short
            public short setOriginFlag;

            /// short
            public short evenTime;
            /// int[8]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
            public double[] orignPos;

            /// double
            public double synVelMax;
            /// double
            public double synAccMax;
            /// double
            public double synDecSmooth;
            /// double
            public double synDecAbrupt;
        }


        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct CrdBufOperation
        {
            public ushort delay;                         // 延时时间
            public short doType;                        // 缓存区IO的类型,0:不输出IO
            public ushort doAddress;                     // IO模块地址
            public ushort doMask;                        // 缓存区IO的输出控制掩码
            public ushort doValue;                       // 缓存区IO的输出值
            public short dacChannel;                     // DAC输出通道
            public short dacValue;                       // DAC输出值
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = System.Runtime.InteropServices.UnmanagedType.U2)]
            public ushort[] dataExt;               // 辅助操作扩展数据
        }

        //前瞻缓冲区；与前瞻相关的数据结构
        public struct CrdBlockData
        {
            public short iMotionType;                             // 运动类型,0为直线插补,1为2D圆弧插补,2为3D圆弧插补,6为IO,7为延时，8位DAC
            public short iCirclePlane;                            // 圆弧插补的平面;XY—1，YZ-2，ZX-3
            public short arcPrmType;                               // 1-圆心表示法；2-半径表示法
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I4)]
            public int[] lPos;            // 当前段各轴终点位置

            public double dRadius;                                // 圆弧插补的半径
            public short iCircleDir;                             // 圆弧旋转方向,0:顺时针;1:逆时针
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
            public double[] dCenter;                             // 2维圆弧插补的圆心相对坐标值，即圆心相对于起点位置的偏移量
                                                                 // 3维圆弧插补的圆心在用户坐标系下的坐标值
            public int height;                                 // 螺旋线的高度
            public double pitch;    // 螺旋线的螺距
                                    //double[3]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
            public double[] beginPos;
            //double[3]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
            public double[] midPos;
            //double[3]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
            public double[] endPos;
            //double[3][3]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 9, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
            public double[] R_inv;
            //double[3][3]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 9, ArraySubType = System.Runtime.InteropServices.UnmanagedType.R8)]
            public double[] R;

            public double dVel;                                   // 当前段合成目标速度
            public double dAcc;                                   // 当前段合成加速度
            public short loop;
            public short iVelEndZero;                             // 标志当前段的终点速度是否强制为0,值0——不强制为0;值1——强制为0
            public CrdBufOperation operation;
            public double dVelEnd;                                // 当前段合成终点速度
            public double dVelStart;                              // 当前段合成的起始速度
            public double dResPos;                                // 当前段合成位移量
        }

        public struct TPComparePrm
        {

            public short encx;             //X方向比较轴的轴号
            public short ency;             //Y方向比较轴的轴号
            public short resv1;            //保留参数
            public short resv2;            //保留参数
            public short source;           //比较源： 0-规划；1-反馈
            public short outputType;       //输出方式：0-脉冲输出；1-电平输出
            public short startLevel;       //起始电平：0-默认；1-取反
            public short time;             //输出脉冲上升沿宽度，单位100us
            public short maxerr;           //比较范围最大误差
            public short threshold;        //最优阈值
            public short pluseCount;       //输出脉冲个数
            public short spacetime;        //输出脉冲下降沿宽度，单位100us
            public short delaytime;        //输出延时时间，单位100us
        }


        public struct TPCompareData
        {

            public int px;                //X方向比较位置
            public int py;                //Y方向比较位置
            public int resv1;             //保留参数
            public int resv2;
        }


        public struct TPCompareDataEx
        {

            public int px;                //X方向比较位置
            public int py;                //Y方向比较位置
            public int resv1;             //保留参数
            public int resv2;
            public short time;             //输出脉冲上升沿宽度，单位100us
            public short spacetime;        //输出脉冲下降沿宽度，单位100us
            public short delaytime;        //输出延时时间，单位100us
            public short pluseCount;       //输出脉冲个数
        }


        #endregion

        #region 板卡基础函数

        /// <summary>
        /// 设置卡号识别方式
        /// </summary>
        /// <param name="sType">卡号方式  0 - 物理插槽  1 - 拨码开关</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCardOrderType(short sType = 0);

        /// <summary>
        /// 获取板卡数量
        /// </summary>
        /// <param name="sNum">板卡数量</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CardNumber(ref short sNum);

        /// <summary>
        /// 获取板卡类型
        /// </summary>
        /// <param name="sType">板卡类型
        ///                   1  - CARD_TYPE_8001，表示8001
        ///                   3  - CARD_TYPE_8003，表示8003
        ///                   15 - CARD_TYPE_M50，表示M50
        ///                   16 - CARD_TYPE_M60，表示M60</param>
        /// <param name="card">IO卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CardType(ref short sType, short card = 0);

        /// <summary>
        /// 获取板卡的序号
        /// </summary>
        /// <param name="sOrder">板卡序号</param>
        /// <param name="card">板卡实际所在位置</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCardOrder(ref short sOrder, short card = 0);

        /// <summary>
        /// 初始化板卡
        /// </summary>
        /// <param name="card">卡号，从0开始计数</param>
        /// <param name="param">参数，保留</param>
        /// <returns>返回值参见说明书</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Open(short card, short param);

        /// <summary>
        /// 关闭板卡
        /// </summary>
        /// <param name="card">卡号，从0开始计数</param>
        /// <returns>返回值参见说明书</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Close(short card);

        /// <summary>
        /// 复位板卡参数
        /// </summary>
        /// <param name="card">卡号，从0开始计数</param>
        /// <returns>返回值参见说明书</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Reset(short card);

        /// <summary>
        /// 获取板卡版本信息
        /// </summary>
        /// <param name="pVersion">字符数组的首地址指针</param>
        /// <param name="size">数组长度</param>
        /// <param name="card">卡号，从0开始计数</param>
        /// <returns>返回值参见说明书</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetVersion(out byte pVersion, int size, short card);

        /// <summary>
        /// 获取急停状态，保持信号，代表急停触发过，使用M_ClrEmg清除
        /// </summary>
        /// <param name="emg">0-急停未触发过，1-急停已经触发过</param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEmg(ref short emg, short card);

        /// <summary>
        ///清除急停状态
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ClrEmg(short card);


        /// <summary>
        /// 设置急停执行动作
        /// </summary>
        /// <param name="EAction">
        ///  参数说明：
        ///  EAction - 急停触发时的动作，
        ///       0x00 - 不减速，直接掉使能（默认值）
        ///       0x01 - 以缓停减速度停机，然后掉使能
        ///       0x02 - 以急停减速度停机，然后掉使能
        ///       0x11 - 以缓停减速度停机，不掉使能
        ///       0x12 - 以急停减速度停机，不掉使能
        /// </param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetEmgAction(short EAction = 0, short card = 0);

        /// <summary>
        /// 读取急停执行动作
        /// </summary>
        /// <param name="EAction">
        ///  参数说明：
        ///  EAction - 急停触发时的动作，
        ///       0x00 - 不减速，直接掉使能（默认值）
        ///       0x01 - 以缓停减速度停机，然后掉使能
        ///       0x02 - 以急停减速度停机，然后掉使能
        ///       0x11 - 以缓停减速度停机，不掉使能
        ///       0x12 - 以急停减速度停机，不掉使能
        /// </param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEmgAction(ref short EAction, short card = 0);


        /// <summary>
        /// 设置急停极性
        /// </summary>
        /// <param name="senseLevel">0-常开，1-常闭</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetEmgInv(short senseLevel, short card = 0);


        /// <summary>
        /// 获取急停极性
        /// </summary>
        /// <param name="senseLevel">0-常开，1-常闭</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEmgInv(ref short senseLevel, short card = 0);

        /// <summary>
        /// 加载板卡系统配置文件，可由MotionAssistant生成，包含急停设置等板卡相关参数设置
        /// </summary>
        /// <param name="filename">文件绝对路径</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_LoadSystemParamFromFile(string filename, short card = 0);

        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetFpgaSecurityCode(ref ulong pCode, short card);

        /// <summary>
        /// 获取系统运行时间
        /// </summary>
        /// <param name="nCounts">PDO总帧数</param>
        /// <param name="nTT">两个通讯帧之间的时间计数，单位2.5ns</param>
        /// <param name="nTl">报文处理时间计数，单位2.5ns</param>
        /// <param name="nT2">预留</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCycleCounts(ref uint nCounts, ref uint nTT, ref uint nTl, ref uint nT2, short card = 0);

        /// <summary>
        /// 设置急停信号来源
        /// </summary>
        /// <param name="emgSrc">急停信号来源，[1-从站DI通道数]表示从站DI为信号源，其他值表示从急停专用口输入</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetEmgSrc(short emgSrc, short card = 0);

        /// <summary>
        /// 设置急停信号来源
        /// </summary>
        /// <param name="emgSrc">急停信号来源，[1-从站DI通道数]表示从站DI为信号源，其他值表示从急停专用口输入</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEmgSrc(ref short emgSrc, short card = 0);

        /// <summary>
        /// 指定LOG存放路径
        /// </summary>
        /// <param name="pFilePath">log存放绝对路径，文件夹路径</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetLogFileDirectory(string pFilePath);

        /// <summary>
        /// 设置LOG记录选项
        /// </summary>
        /// <param name="saveToFile">1-开始记录LOG 0-信息放到共享内存，可用log工具捕获</param>
        /// <param name="updateInput">0-所有Get类函数不打印，1-所有Get类函数打印</param>
        /// <param name="printLevel">打印等级 0-所有函数信息 1-warning信息 2-alarm信息</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetLogFilePrintOption(short saveToFile = 0, short updateInput = 0, short printLevel = 0);


        #endregion

        #region 总线操作函数
        /// <summary>
        /// 复位总线通讯，直接中断总线通讯
        /// </summary>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ResetFpga(short card);

        /// <summary>
        /// 获取从站CRC错误统计数据
        /// </summary>
        /// <param name="err">存放从站错误统计值的指针，存放顺序位：从站0错误计数 - 从站1错误计数 - ... - 从站n错误计数</param>
        /// <param name="dataNum">错误统计值个数</param>
        /// <param name="card">主站卡地址，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCrcErrorInfo(ref IntPtr err, ref short dataNum, short card = 0);

        /// <summary>
        /// 获取 Ethercat 网络中的从站资源
        /// </summary>
        /// <param name="pRes">从站资源结构体 SL_RES 的指针地址</param>
        /// <param name="card">卡号，从0开始计数</param>
        /// <returns>返回值参见说明书</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetSlaveResource(out SL_RES pRes, short card);

        /// <summary>
        /// 获取 Ethercat 网络中指定从站的信息。
        /// </summary>
        /// <param name="pInfo">从站信息结构体 SL_INFO 的指针地址</param>
        /// <param name="slaveNo"> 从站号，从 1 开始计数</param>
        /// <param name="card">卡号，从0开始计数</param>
        /// <returns>返回值参见说明书</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetSlaveInfo(out SL_INFO pInfo, short slaveNo, short card);

        /// <summary>
        /// 获取从站中的IO资源
        /// </summary>
        /// <param name="pIoRes">IO资源结构体</param>
        /// <param name="IoM">IO模块号，从1开始</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetSlaveIoResource(out SL_IO_RES pIoRes, short IoM, short card = 0);

        /// <summary>
        /// 加载运动控制板卡 ENI 文件
        /// </summary>
        /// <param name="eniPath">Eni 文件的路径地址</param>
        /// <param name="card">卡号，从0开始计数</param>
        /// <returns>返回值参见说明书</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_LoadEni(string eniPath, short card);

        /// <summary>
        /// 加载上一次的总线配置信息
        /// </summary>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_LoadEcatConfigDefault(short card = 0);

        /// <summary>
        /// 链接总线，如果上次没有断开总线则直接快速链接
        /// </summary>
        /// <param name="option">断线后 0-DO输出不保持 1-DO输出保持</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ConnectECAT(short option, short card);

        /// <summary>
        /// 断开总线链接
        /// </summary>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_DisconnectECAT(short card);

        /// <summary>
        /// 下发SDO数据给从站
        /// </summary>
        /// <param name="slave">从站号</param>
        /// <param name="index">从站SDO 索引</param>
        /// <param name="subindex">从站SDO 子索引 </param>
        /// <param name="data">下发数据</param>
        /// <param name="data_size">数据大小，单位byte</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EcatSDOWrite(short slave, short index, short subindex, uint data, short data_size, short card);

        /// <summary>
        /// 快速下发SDO数据给从站，不进行3次校验
        /// </summary>
        /// <param name="slave">从站号</param>
        /// <param name="index">从站SDO 索引</param>
        /// <param name="subindex">从站SDO 子索引 </param>
        /// <param name="data">下发数据</param>
        /// <param name="data_size">数据大小，单位byte</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EcatSDOWriteFast(short slave, short index, short subindex, uint data, short data_size, short card = 0);

        /// <summary>
        ///  获取 Ethercat 网络中所有从站的信息。
        /// </summary>
        /// <param name="pInfo">从站信息结构体数组指针</param>
        /// <param name="slaveNo">获取到的从站个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetSlaveInfoAll(IntPtr pInfo, ref short slaveNo, short card = 0);

        /// <summary>
        /// 读取SDO数据
        /// </summary>
        /// <param name="slave">从站号</param>
        /// <param name="index">从站SDO 索引</param>
        /// <param name="subindex">从站SDO 子索引 </param>
        /// <param name="data_size">数据大小，单位byte</param>
        /// <param name="pBuf">读取到的数据</param>
        /// <param name="count">默认1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EcatSDORead(short slave, short index, short subindex, short data_size, out uint pBuf, short count, short card);

        /// <summary>
        /// 将指定 PDO 对象控制权限交给用户
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="index">PDO 映射地址，只有 eni 中包含的 TXPDO 才可以设置。</param>
        /// <param name="subindex">PDO 映射地址的子索引。</param>
        /// <param name="getcontrol">设置用户权限，1：用户可以使用 M_AxisPDOWrite 进行写入，0：对应 PDO 由板卡控制</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EcatSetPdoControlByUser(short axis, short index, short subindex, short getcontrol, short card = 0);

        /// <summary>
        /// 设置指定轴的 PDO 对象的数据。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="index">PDO索引</param>
        /// <param name="subindex">PDO子索引</param>
        /// <param name="data">数据</param>
        /// <param name="data_size">数据长度，单位byte</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPDOWrite(short axis, short index, short subindex, uint data, short data_size, short card = 0);

        /// <summary>
        /// 读取指定轴的 PDO 对象的数据。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="index">PDO索引</param>
        /// <param name="subindex">PDO子索引</param>
        /// <param name="pBuf">数据</param>
        /// <param name="data_size">数据长度，单位byte</param>
        /// <param name="count">默认1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPDORead(short axis, short index, short subindex, short data_size, ref uint pBuf, short count = 0, short card = 0);

        /// <summary>
        /// 获取连接总线时异常信息，目前支持在 PREOP 和 SAFEOP 状态下出现问题时获取第一个问题从站。
        /// </summary>
        /// <param name="err">连接报错信息，格式为 0x 0014_aabb: 0014 是报错码，aa 是状
        ///态（04 是 safe，08 是 op）, bb 是出现问题的从站逻辑顺序编号 从 0 开始</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetConnectError(ref uint err, short card = 0);

        /// <summary>
        ///  设置每个轴的连接超时时间，单位10ms
        /// </summary>
        /// <param name="timeout">每个轴连接超时时间</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetConnectTimeoutValue(int timeout = 3000, short card = 0);

        /// <summary>
        /// 获取指定轴的从站ID号
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="slaveIndex">对应轴的从站ID</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAxisSlaveIndex(short axis, ref short slaveIndex, short card = 0);

        /// <summary>
        /// 获取连接状态
        /// </summary>
        /// <param name="isOp">主站是否退出OP</param>
        /// <param name="isSlaveChange">从站是否发生变化</param>
        /// <param name="isOffline">是否存在掉线</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetConnectStatus(ref short isOp, ref short isSlaveChange, ref short isOffline, short card = 0);

        /// <summary>
        /// 获取网口状态（停用）
        /// </summary>
        /// <param name="isINV"></param>
        /// <param name="isINVNo"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetRJ45Status(ref short isINV, ref short isINVNo, short card = 0);

        /// <summary>
        /// 开启关闭总线别名
        /// </summary>
        /// <param name="enable">1-开启，0-关闭</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EnableAlias(short enable = 0, short card = 0);

        /// <summary>
        /// 扫描从站模块ID信息
        /// </summary>
        /// <param name="totalSlaveNum">总从站数量</param>
        /// <param name="slaveIndex">从站索引</param>
        /// <param name="slaveType">返回从站类型</param>
        /// <param name="dcSync">返回DC同步状态</param>
        /// <param name="smInfo">返回从站SM信息结构体</param>
        /// <param name="moduleNum">返回模块数量</param>
        /// <param name="moduleIdBuff">返回模块ID缓冲区</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ScanModuleID(short totalSlaveNum, short slaveIndex, ref short slaveType, ref short dcSync, ref ECAT_SM_INFO smInfo,
                             ref short moduleNum, ref uint moduleIdBuff, short card = 0);

        /// <summary>
        /// 读取伺服从站别名数组
        /// </summary>
        /// <param name="pAlias">返回别名数组首地址</param>
        /// <param name="pSlaveNum">返回从站数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ReadServoAlias(ref short pAlias, ref short pSlaveNum, short card = 0);

        /// <summary>
        /// 读取IO从站别名数组
        /// </summary>
        /// <param name="pAlias">返回别名数组首地址</param>
        /// <param name="pSlaveNum">返回从站数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ReadIOAlias(ref short pAlias, ref short pSlaveNum, short card = 0);

        /// <summary>
        /// 读取所有从站别名数组
        /// </summary>
        /// <param name="pAlias">返回别名数组首地址</param>
        /// <param name="pSlaveNum">返回从站数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ReadSlaveAlias(ref short pAlias, ref short pSlaveNum, short card = 0);

        /// <summary>
        /// 写入伺服从站别名数组
        /// </summary>
        /// <param name="pAlias">别名数组首地址</param>
        /// <param name="slaveNum">从站数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_WriteServoAlias(ref short pAlias, short slaveNum, short card = 0);

        /// <summary>
        /// 写入单个从站别名
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="alias">别名值</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_WriteSingleSlaveAlias(short axis, ushort alias, short card = 0);

        /// <summary>
        /// 获取伺服从站别名掩码
        /// </summary>
        /// <param name="pAlias">别名数组首地址</param>
        /// <param name="slaveNum">从站数量</param>
        /// <param name="pMask">返回掩码值</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetServoAliasMask(ref short pAlias, short slaveNum, ref uint pMask, short card = 0);


        #endregion

        #region 总线IO函数
        /// <summary>
        /// 设置数字量通道输出
        /// </summary>
        /// <param name="channel">通道 1开始</param>
        /// <param name="Value">0-关闭，1-开启</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Digital_Chn_Output(short channel, short Value, short card);

        /// <summary>
        /// 获取数字量通道输出状态
        /// </summary>
        /// <param name="channel">通道 1开始</param>
        /// <param name="pValue">返回输出状态：0-关闭，1-开启</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Chn_Output(short channel, out short pValue, short card);

        /// <summary>
        /// 设置指定从站数字量通道输出
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="channel">通道 1开始</param>
        /// <param name="value">0-关闭，1-开启</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Slave_Digital_Chn_Output(short IoM, short channel, short value, short card = 0);

        /// <summary>
        /// 获取指定从站数字量通道输出状态
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="channel">通道 1开始</param>
        /// <param name="value">返回输出状态：0-关闭，1-开启</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Digital_Chn_Output(short IoM, short channel, ref short value, short card = 0);

        /// <summary>
        /// 设置数字量端口输出（批量操作）
        /// </summary>
        /// <param name="chnBegin">起始通道号,读取连续的32个通道值</param>
        /// <param name="lValue">输出值，每个bit对应一个通道</param>
        /// <param name="lMask">掩码，bit为1时对应通道可改写</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Digital_Port_Output(short chnBegin, uint lValue, uint lMask, short card);

        /// <summary>
        /// 获取数字量端口输出状态（批量操作）
        /// </summary>
        /// <param name="chnBegin">起始通道号</param>
        /// <param name="lValue">返回输出状态，每个bit对应一个通道</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Port_Output(short chnBegin, ref uint lValue, short card);

        /// <summary>
        /// 设置指定从站数字量端口输出（批量操作）
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="chnBegin">起始通道号</param>
        /// <param name="lValue">输出值，每个bit对应一个通道</param>
        /// <param name="lMask">掩码，bit为1时对应通道可改写</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Slave_Digital_Port_Output(short IoM, short chnBegin, uint lValue, uint lMask, short card = 0);

        /// <summary>
        /// 获取指定从站数字量端口输出状态（批量操作）
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="chnBegin">起始通道号</param>
        /// <param name="lValue">返回输出状态，每个bit对应一个通道</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Digital_Port_Output(short IoM, short chnBegin, ref uint lValue, short card = 0);

        /// <summary>
        /// 获取所有数字量输出状态
        /// </summary>
        /// <param name="chnNum">返回输出通道数量</param>
        /// <param name="pValue">返回输出状态数组</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Output(ref short chnNum, ref ushort pValue, short card = 0);

        /// <summary>
        /// 获取数字量通道输入状态
        /// </summary>
        /// <param name="channel">通道 1开始</param>
        /// <param name="pValue">返回输入状态：0-关闭，1-开启</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Chn_Input(short channel, out short pValue, short card);

        /// <summary>
        /// 获取数字量端口输入状态（批量操作）
        /// </summary>
        /// <param name="chnBegin">起始通道号</param>
        /// <param name="lValue">返回输入状态，每个bit对应一个通道</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Port_Input(short chnBegin, ref uint lValue, short card);

        /// <summary>
        /// 获取指定从站数字量通道输入状态
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="channel">通道 1开始</param>
        /// <param name="value">返回输入状态：0-关闭，1-开启</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Digital_Chn_Input(short IoM, short channel, ref short value, short card = 0);

        /// <summary>
        /// 获取指定从站数字量端口输入状态（批量操作）
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="chnBegin">起始通道号</param>
        /// <param name="lValue">返回输入状态，每个bit对应一个通道</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Digital_Port_Input(short IoM, short chnBegin, ref uint lValue, short card = 0);

        /// <summary>
        /// 获取所有数字量输入状态
        /// </summary>
        /// <param name="chnNum">返回输入通道数量</param>
        /// <param name="pValue">返回输入状态数组</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Input(ref short chnNum, ref ushort pValue, short card = 0);

        /// <summary>
        /// 获取所有数字量输出状态（扩展版本）
        /// </summary>
        /// <param name="chnNum">返回输出通道数量</param>
        /// <param name="dataNum">返回数据组数量</param>
        /// <param name="pValue">返回输出状态指针</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Output_Ex(ref short chnNum, ref short dataNum, ref IntPtr pValue, short card = 0);

        /// <summary>
        /// 获取所有数字量输入状态（扩展版本）
        /// </summary>
        /// <param name="chnNum">返回输入通道数量</param>
        /// <param name="dataNum">返回数据组数量</param>
        /// <param name="pValue">返回输入状态指针</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Digital_Input_Ex(ref short chnNum, ref short dataNum, ref IntPtr pValue, short card = 0);


        /// <summary>
        /// 数字量输出翻转
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="reverseTime">烦转时间，1ms为单位</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Digital_Chn_Reverse(short channel, double reverseTime, short card = 0);


        /// <summary>
        /// 数字量输出PWM
        /// </summary>
        /// <param name="doType">输出类型（保留）</param>
        /// <param name="doChn">输出通道号，从1开始</param>
        /// <param name="hPeriod">脉冲高电平时间，以总线周期为单位</param>
        /// <param name="lPeriod"> 脉冲低电平时间，以总线周期为单位</param>
        /// <param name="pulseNum">脉冲个数</param>
        /// <param name="firstLevel">初始电平 </param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Digital_Chn_PWM(short doType, short doChn, short hPeriod, short lPeriod, int pulseNum, short firstLevel, short card = 0);


        #endregion

        #region 总线模拟量函数
        /// <summary>
        /// 设置模拟量输出
        /// </summary>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">设置模拟量通道的数值，short 类型，-32768--32767</param>
        /// <param name="count">需要设置的通道数量</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Analog_Output(short channel, ref short pValue, short count, short card);

        /// <summary>
        /// 获取模拟量输出值
        /// </summary>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">返回输出值</param>
        /// <param name="count">读取通道数量</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Analog_Output(short channel, out short pValue, short count, short card);


        /// <summary>
        /// 设置指定从站模拟量输出
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始计数，别名开启则为从站号</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">输出值数组指针</param>
        /// <param name="count">需要设置输出的通道数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Slave_Analog_Output(short IoM, short channel, ref short pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取指定从站模拟量输出值
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始计数，别名开启则为从站号</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">返回输出值数组地址</param>
        /// <param name="count">需要读取的通道数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Analog_Output(short IoM, short channel, ref short pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取模拟量输入值
        /// </summary>
        /// <param name="channel">模拟量通道号，从 1 开始计数，输入输出独立</param>
        /// <param name="pValue">获取模拟量通道的数值，short 类型，-32768--32767</param>
        /// <param name="count">读取的通道数量</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Analog_Input(short channel, out short pValue, short count, short card);

        /// <summary>
        /// 获取指定从站模拟量输入值
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">返回输入值</param>
        /// <param name="count">读取值数量，默认1</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Analog_Input(short IoM, short channel, ref short pValue, short count = 1, short card = 0);


        /// <summary>
        /// 设置32位模拟量输出
        /// </summary>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">32位输出值指针</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Analog_Output_32(short channel, ref uint pValue, short card);

        /// <summary>
        /// 设置指定从站32位模拟量输出
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">32位输出值指针</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Slave_Analog_Output_32(short IoM, short channel, ref uint pValue, short card = 0);

        /// <summary>
        /// 获取32位模拟量输入值
        /// </summary>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">返回32位输入值</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Analog_Input_32(short channel, out uint pValue, short card);

        /// <summary>
        /// 获取指定从站32位模拟量输入值
        /// </summary>
        /// <param name="IoM">IO模块序号，从1开始</param>
        /// <param name="channel">通道号，从1开始</param>
        /// <param name="pValue">返回32位输入值</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Analog_Input_32(short IoM, short channel, ref uint pValue, short card = 0);

        #endregion

        #region 总线变量函数
        /// <summary>
        /// 设置NDC输出变量值
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="pValue">输出值</param>
        /// <param name="count">写入值数量，默认1</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Ndc_OutVar(short channel, ref uint pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取NDC输出变量值
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="pValue">返回输出值</param>
        /// <param name="count">读取值数量，默认1</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Ndc_OutVar(short channel, ref uint pValue, short count = 1, short card = 0);

        /// <summary>
        /// 设置指定从站NDC输出变量值
        /// </summary>
        /// <param name="address">从站地址</param>
        /// <param name="channel">通道号</param>
        /// <param name="pValue">输出值</param>
        /// <param name="count">写入值数量，默认1</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Set_Slave_Ndc_OutVar(short address, short channel, ref uint pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取指定从站NDC输出变量值
        /// </summary>
        /// <param name="address">从站地址</param>
        /// <param name="channel">通道号</param>
        /// <param name="pValue">返回输出值</param>
        /// <param name="count">读取值数量，默认1</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Ndc_OutVar(short address, short channel, ref uint pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取所有NDC输出变量值
        /// </summary>
        /// <param name="pValue">返回输出值数组</param>
        /// <param name="chnNum">返回通道数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Ndc_OutVar_All(ref uint pValue, ref short chnNum, short card = 0);

        /// <summary>
        /// 获取所有NDC输出变量值（扩展版本）
        /// </summary>
        /// <param name="pValue">返回输出值指针</param>
        /// <param name="chnNum">返回通道数量</param>
        /// <param name="dataNum">返回数据组数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Ndc_OutVar_All_Ex(IntPtr pValue, ref short chnNum, ref short dataNum, short card = 0);

        /// <summary>
        /// 获取NDC输入变量值
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="pValue">返回输入值</param>
        /// <param name="count">读取值数量，默认1</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Ndc_InVar(short channel, ref uint pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取指定从站NDC输入变量值
        /// </summary>
        /// <param name="address">从站地址</param>
        /// <param name="channel">通道号</param>
        /// <param name="pValue">返回输入值</param>
        /// <param name="count">读取值数量，默认1</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Slave_Ndc_InVar(short address, short channel, ref uint pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取所有NDC输入变量值
        /// </summary>
        /// <param name="pValue">返回输入值数组</param>
        /// <param name="chnNum">返回通道数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Ndc_InVar_All(ref uint pValue, ref short chnNum, short card = 0);

        /// <summary>
        /// 获取所有NDC输入变量值（扩展版本）
        /// </summary>
        /// <param name="pValue">返回输入值指针</param>
        /// <param name="chnNum">返回通道数量</param>
        /// <param name="dataNum">返回数据组数量</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Get_Ndc_InVar_All_Ex(IntPtr pValue, ref short chnNum, ref short dataNum, short card = 0);

        #endregion

        #region 驱动器回零函数


        /// <summary>
        /// 设置驱动器模式，模式6为回零模式，模式8为运动模式
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="mode">模式</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetHomingMode(short axis, short mode, short card);

        /// <summary>
        /// 设置回零参数
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="method">回零模式，0x6098，详见驱动器手册</param>
        /// <param name="offset">回零偏移，0x607c 详见驱动器手册</param>
        /// <param name="speed1">回零速度1，0x6099：01，详见驱动器手册</param>
        /// <param name="speed2">回零速度2，0x6099：02，详见驱动器手册</param>
        /// <param name="acc">回零加速度，0x609a，详见驱动器手册</param>
        /// <param name="probeFunction">预留，默认给0</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetHomingPrm(short axis, short method, int offset, uint speed1, uint speed2, uint acc, ushort probeFunction, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetHomingPrm_Unit(short axis, short method, double offset = 0.0, double speed1 = 0.0, double speed2 = 0.0,
                                 double acc = 0.0, ushort probeFunction = 0, short card = 0);

        /// <summary>
        /// 获取回零参数
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="method">回零模式，0x6098，详见驱动器手册</param>
        /// <param name="offset">回零偏移，0x607c 详见驱动器手册</param>
        /// <param name="speed1">回零速度1，0x6099：01，详见驱动器手册</param>
        /// <param name="speed2">回零速度2，0x6099：02，详见驱动器手册</param>
        /// <param name="acc">回零加速度，0x609a，详见驱动器手册</param>
        /// <param name="probeFunction">预留，默认给0</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetHomingPrm(short axis, ref short method, ref int offset, ref uint speed1, ref uint speed2, ref uint acc, ref ushort probeFunction, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetHomingPrm_Unit(short axis, ref short pMethod, ref double pOffset, ref double pSpeed1, ref double pSpeed2,
                                 ref double pAcc, ref ushort pProbeFunction, short card = 0);

        /// <summary>
        /// 开始驱动器回零动作
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HomingStart(short axis, short card = 0);

        /// <summary>
        /// 等待驱动器回零完成，阻塞型函数
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="timeout">回零超时时间</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_WaitHomingFinished(short axis, int timeout, short card = 0);

        /// <summary>
        /// 获取回零状态，预留
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="phomingStatus"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEcatHomingStatus(short axis, out short phomingStatus, short card);

        /// <summary>
        /// 取消回零
        /// </summary>
        /// <param name="mask">bit0代表轴1，bit1代表轴2，bitn代表轴n+1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HomeCancel(uint mask, short card);

        /// <summary>
        /// 取消单轴回零
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HomeCancelSingleAxis(short axis, short card = 0);


        /// <summary>
        /// 多轴开始回零
        /// </summary>
        /// <param name="mask">bit n代表axis n+1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HomingStartMulti(uint mask, short card = 0);


        #endregion

        #region 单轴基础状态函数

        /// <summary>
        /// 使能
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Servo_On(short axis, short card);

        /// <summary>
        /// 快速使能，期间不判断0x6041
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="interval">6040 6-7-15状态切换延时</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Servo_On_Quickly(short axis, short interval, short card = 0);

        /// <summary>
        /// 多轴快速使能
        /// </summary>
        /// <param name="axisNum">使能轴数量</param>
        /// <param name="axis">使能轴号数组首地址</param>
        /// <param name="interval">模式切换间隔</param>
        /// <param name="servoState">使能状态，bit n为1则axis n+1 使能失败</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Mul_Servo_On_Quickly(short axisNum, ref short axis, short interval, ref uint servoState, short card = 0);

        /// <summary>
        /// 去使能
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Servo_Off(short axis, short card);

        /// <summary>
        /// 设置软限位
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="positive">正向软限位，给0不启用</param>
        /// <param name="negative">负向软限位，给0不启用</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetSoftLimit(short axis, int positive, int negative, short card);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetSoftLimit_Unit(short axis, double positive, double negative, short card = 0);

        /// <summary>
        /// 获取软限位
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pPositive">正向软限位</param>
        /// <param name="pNegative">负向软限位</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetSoftLimit(short axis, out int pPositive, out int pNegative, short card);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetSoftLimit_Unit(short axis, out double pPositive, out double pNegative, short card = 0);

        /// <summary>
        /// 设置轴到位判断阈值，设置后INP信号生效
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="band">到位范围，单位脉冲</param>
        /// <param name="time">到位稳定时间，单位 DC周期</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetAxisBand(short axis, uint band, uint time, short card);

        /// <summary>
        /// 获取轴到位判断阈值
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pBand">到位范围，单位脉冲</param>
        /// <param name="time">到位稳定时间，单位 DC周期</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAxisBand(short axis, out uint pBand, out uint time, short card);

        /// <summary>
        /// 设置轴停止加速度，停止函数生效
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="dSmoothDec">缓停加速度，单位脉冲/s/s</param>
        /// <param name="dEmergencyDec">急停加速度，单位脉冲/s/s</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetStopDec(short axis, double dSmoothDec, double dEmergencyDec, short card);

        /// <summary>
        /// 读取轴停止加速度，停止函数生效
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="dSmoothDec">缓停加速度，单位脉冲/s/s</param>
        /// <param name="dEmergencyDec">急停加速度，单位脉冲/s/s</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetStopDec(short axis, out double dSmoothDec, out double dEmergencyDec, short card);

        /// <summary>
        /// 读取轴状态
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pSts">轴状态，轴状态数组首地址，bit1-伺服报警 bit4-位置超差 bit5-正向极限 bit6-负向极限 bit7-缓停触发 bit8-急停触发 bit9-使能 bit10-运动中
        ///                      bit11-到位inp bit16回零错误 bit17-回零完成 bit18-驱动器返回目标到达 bit20-原点开关 bit24-掉线
        /// </param>
        /// <param name="count">读取的轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetSts(short axis, out int pSts, short count, short card);

        /// <summary>
        /// 批量读取轴状态，针对开启别名后轴号不连续的情况
        /// </summary>
        /// <param name="axisArray">轴数组</param>
        /// <param name="pSts">轴状态数组</param>
        /// <param name="count">获取的轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetMultiSts(ref short axisArray, ref int pSts, short count = 1, short card = 0);

        /// <summary>
        /// 清除报警
        /// </summary>
        /// <param name="axis">轴号，起始轴号</param>
        /// <param name="count">操作的轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ClrSts(short axis, short count, short card);

        /// <summary>
        /// 获取板卡规划值
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pValue">轴规划值或数组首地址</param>
        /// <param name="count">读取的轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCmd(short axis, out double pValue, short count, short card);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCmd_Unit(short axis, out double pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取板卡规划速度
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pValue">轴规划速度或数组首地址</param>
        /// <param name="count">读取的轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCmdVel(short axis, out double pValue, short count, short card);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCmdVel_Unit(short axis, out double pValue, short count = 1, short card = 0);

        /// <summary>
        /// 获取板卡反馈位置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pValue">轴反馈位置或数组首地址</param>
        /// <param name="count">读取的轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEncPos(short axis, out double pValue, short count, short card);

        /// <summary>
        /// 获取板卡反速度，需要添加0x606c PDO
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pValue">轴反馈位置或数组首地址</param>
        /// <param name="count">读取的轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEncVel(short axis, out double pValue, short count, short card);

        /// <summary>
        /// 获取轴跟随误差
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="error">误差值</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetFollowError(short axis, ref int error, short card = 0);

        /// <summary>
        /// 加载轴参数文件，可用MotionAssistant生成
        /// </summary>
        /// <param name="filename">文件绝对路径，需要完整路径</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_LoadParamFromFile(string filename, short card);

        /// <summary>
        /// 加载轴参数文件，可用MotionAssistant生成（暂停使用）
        /// </summary>
        /// <param name="filename">文件绝对路径，需要完整路径</param>
        ///  <param name="AxisNum">轴数量</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_LoadParamFromFile(string filename, short AxisNum, short card);

        /// <summary>
        /// 设置轴当前位置 当前位置=驱动器反馈位置+板卡OFFSET，仅改变板卡OFFSET，断电后或者回原后OFFSET清零
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">位置</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCurrentPos(short axis, int pos, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCurrentPos_Unit(short axis, double pos, short card = 0);


        /// <summary>
        /// 写轴参数文件（停用）
        /// </summary>
        /// <param name="AxisNum"></param>
        /// <param name="ParamName"></param>
        /// <param name="Param"></param>
        /// <param name="filePath"></param>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void M_WriteAxisParam(string AxisNum, string ParamName, string Param, string filePath);

        /// <summary>
        /// 设置驱动器0x6060地址数据，兼容PDO，SDO两种类型
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="mode">模式，详见驱动器Ethercat手册</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EcatSetOperationMode(short axis, short mode, short card);

        /// <summary>
        /// 读取驱动器0x6060地址数据，兼容PDO，SDO两种类型
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="mode">模式，详见驱动器Ethercat手册</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EcatGetOperationMode(short axis, ref short mode, short card);

        /// <summary>
        /// 设置驱动器自带DO输出，给0x60FE输出数据，需将60FE:01  60FE:02配成PDO
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="digitalOut">数字量输出，bit0代表驱动器输出0</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetEcatDigitalOut(short axis, uint digitalOut, short card);

        /// <summary>
        /// 读取驱动器的数字量输入，0X60FD，bit0-负限位触发 bit1-正限位触发 bit2-原点触发 
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="digitalInput"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetEcatDigitalInput(short axis, out uint digitalInput, short card);

        /// <summary>
        /// 获取驱动器0x6041值
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="statusword">状态字</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EcatStatusWord(short axis, ref ushort statusword, short card);

        /// <summary>
        /// 读取轴实际位置，0x6064
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pPosition">实际位置</param>
        /// <param name="count">个数，默认为1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ReadActualPosition(short axis, out int pPosition, short count = 1, short card = 0);

        /// <summary>
        /// 读取伺服错误码，需将0x603f添加到PDO，错误信息需根据驱动器查询
        /// </summary>
        /// <param name="axis">轴号，从1开始</param>
        /// <param name="pCode">错误码</param>
        /// <param name="count">默认给1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ReadErrorCode(short axis, ref short pCode, short count = 1, short card = 0);

        /// <summary>
        /// 设置轴状态同步，限位锁定设置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="on">开启同步，关闭限位锁定</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetAxisStsSync(short axis, short on, short card = 0);

        /// <summary>
        /// 获取轴状态同步，限位锁定设置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="on">开启同步，关闭限位锁定</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAxisStsSync(short axis, ref short on, short card = 0);

        /// <summary>
        /// 设置当前轴的位置误差，规划和反馈差值大于设定是，M_GetSts bit4会置1
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="error">位置误差</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetAxisPosError(short axis, uint error, short card = 0);

        /// <summary>
        /// 读取当前轴的位置误差，规划和反馈差值大于设定是，M_GetSts bit4会置1
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="error">位置误差</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAxisPosError(short axis, ref uint error, short card = 0);

        /// <summary>
        /// 设置当前碰到急停后位置同步方式
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="enable">1-遇到限位后规划值主动对齐反馈值 0-遇到限位后规划值不变化</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EnableHardLimitPosSync(short axis, short enable = 1, short card = 0);

        /// <summary>
        /// 设置单轴脉冲当量，设置后所有后缀带unit函数可以使用
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="equiv">脉冲当量，设置后1个unit=equiv设置脉冲值</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetAxisEquiv(short axis, double equiv, short card = 0);

        /// <summary>
        /// 获取单轴脉冲当量，设置后所有后缀带unit函数可以使用
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="equiv">脉冲当量，设置后1个unit=equiv设置脉冲值</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAxisEquiv(short axis, ref double equiv, short card = 0);

        /// <summary>
        /// 读取当前扭矩值，需要往PDO中添加0x6077
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pTorque">扭矩千分比</param>
        /// <param name="count">读取数量</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ReadActualTorque(short axis, ref short pTorque, short count = 1, short card = 0);

        /// <summary>
        /// 设置目标扭矩值
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="torque">扭矩目标值</param>
        /// <param name="vel">扭矩变化率</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_WriteTargetTorque(short axis, short torque, double vel, short card = 0);


        #endregion

        #region 单轴点位运动函数
        /// <summary>
        /// 设置单轴运动的加速度，减速度，加加速时间
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pPrm">运行参数结构体，通过结构体传参</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetMove(short axis, ref CmdPrm pPrm, short card);

        /// <summary>
        /// 读取单轴运动的加速度，减速度，加加速时间
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pPrm">运行参数结构体，通过结构体传参</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetMove(short axis, out CmdPrm pPrm, short card);

        /// <summary>
        /// 绝对运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">绝对点位，单位脉冲</param>
        /// <param name="vel">绝对运动速度，单位脉冲</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AbsMove(short axis, int pos, double vel, short card);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AbsMove_Unit(short axis, double pos, double vel, short card = 0);

        /// <summary>
        /// 相对运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="dis">相对距离，单位脉冲</param>
        /// <param name="vel">绝对运动速度，单位脉冲</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_RelMove(short axis, int dis, double vel, short card);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_RelMove_Unit(short axis, double pos, double vel, short card = 0);

        /// <summary>
        /// 开始恒速运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="vel">恒速速度</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Jog(short axis, double vel, short card);

        /// <summary>
        /// 停止运动
        /// </summary>
        /// <param name="mask">bit0代表轴1，bit1代表轴2，bitn代表轴n+1</param>
        /// <param name="option">0-缓停减速度停机，1-急停减速度停机</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Stop(uint mask, uint option, short card);

        /// <summary>
        /// 暂停当前点位运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisMovePause(short axis, short card = 0);

        /// <summary>
        /// 继续当前点位运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisMoveResume(short axis, short card = 0);

        /// <summary>
        /// 单轴停止运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="option">0-缓停，1-急停</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_StopSingleAxis(short axis, int option, short card = 0);

        /// <summary>
        /// 获取目标位置
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pValue">单轴目标位置</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetTargetPos(short axis, ref int pValue, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetTargetPos_Unit(short axis, out double pValue, short card = 0);

        /// <summary>
        /// 位置对齐，将607a对齐到6064
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="count">对齐轴个数</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPosAlign(short axis, short count = 1, short card = 0);

        /// <summary>
        /// 设置IO控制轴停机
        /// </summary>
        /// <param name="axis"> 轴号，从1开始</param>
        /// <param name="enable">使能，0-关闭，1-激活</param>
        /// <param name="diType"> DI类型，0-伺服数字输入，1-通用输入</param>
        /// <param name="index"> 索引，从0开始</param>
        /// <param name="trigSrc"> 有效电平，0或1  2-上升沿；3-下降沿</param>
        /// <param name="stopType">停止方式，1-缓停/2-急停</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetAxisStopDi(short axis, short enable, short diType, short index, short trigSrc, short stopType, short card = 0);

        /// <summary>
        /// 读取IO控制轴停机
        /// </summary>
        /// <param name="axis"> 轴号，从1开始</param>
        /// <param name="enable">使能，0-关闭，1-激活</param>
        /// <param name="diType"> DI类型，0-伺服数字输入，1-通用输入</param>
        /// <param name="index"> 索引，从0开始</param>
        /// <param name="trigSrc"> 有效电平，0或1  2-上升沿；3-下降沿</param>
        /// <param name="stopType">停止方式，1-缓停/2-急停</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAxisStopDi(short axis, ref short enable, ref short diType, ref short index, ref short trigSrc, ref short stopType, short card = 0);

        /// <summary>
        /// 设置IO变速运动参数
        /// </summary>
        /// <param name="axis">轴号，从1开始</param>
        /// <param name="moveType">触发后运动类型，0 - 相对运动；1 - 绝对运动</param>
        /// <param name="movePos">触发后移动的距离或位置</param>
        /// <param name="moveVel">触发后的最高速度</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_IoChangePosSpeed_SetPrm(short axis, short moveType, double movePos, double moveVel, short card = 0);

        /// <summary>
        /// 获取IO变速运动参数
        /// </summary>
        /// <param name="axis">轴号，从1开始</param>
        /// <param name="moveType">触发后运动类型，0 - 相对运动；1 - 绝对运动</param>
        /// <param name="movePos">触发后移动的距离或位置</param>
        /// <param name="moveVel">触发后的最高速度</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_IoChangePosSpeed_GetPrm(short axis, ref short moveType, ref double movePos, ref double moveVel, short card = 0);

        /// <summary>
        /// 设置IO变速条件
        /// </summary>
        /// <param name="axis">轴号，从1开始</param>
        /// <param name="enable">使能IO触发功能，0 - 关闭；1 - 启动</param>
        /// <param name="diType">IO类型（保留）</param>
        /// <param name="diChannel">IO触发的输入通道号，从1开始</param>
        /// <param name="diEdge">触发信号边沿，0 - 下降沿；1 - 上升沿</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_IoChangePosSpeed_SetCfg(short axis, short enable, short diType, short diChannel, short diEdge, short card = 0);

        /// <summary>
        /// 获取IO变速条件
        /// </summary>
        /// <param name="axis">轴号，从1开始</param>
        /// <param name="enable">使能IO触发功能，0 - 关闭；1 - 启动</param>
        /// <param name="diType">IO类型（保留）</param>
        /// <param name="diChannel">IO触发的输入通道号，从1开始</param>
        /// <param name="diEdge">触发信号边沿，0 - 下降沿；1 - 上升沿</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_IoChangePosSpeed_GetCfg(short axis, ref short enable, ref short diType, ref short diChannel, ref short diEdge, short card = 0);


        /// <summary>
        /// 获取当前IO触发状态
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="trigCount">触发次数</param>
        /// <param name="trigPos">触发位置</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_IoChangePosSpeed_GetStatus(short axis, ref int trigCount, ref double trigPos, short card = 0);


        /// <summary>
        /// 设置点位运动参数
        /// </summary>
        /// <param name="axis">轴号，从1开始</param>
        /// <param name="moveType">点位运动方式，0 - 走绝对位置；1 - 走相对位置</param>
        /// <param name="pos"> 位置值（count），当moveType为0表示终点位置，当moveType为1表示移动距离</param>
        /// <param name="vel">运行速度（count/s）</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetMoveParam(short axis, short moveType, int pos, double vel, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetMoveParam_Unit(short axis, short moveType, double pos, double vel, short card = 0);


        /// <summary>
        /// 启动点位运动
        /// </summary>
        /// <param name="count">要启动的轴数量</param>
        /// <param name="axisArray">轴号数组</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_StartMove(short count, ref short axisArray, short card = 0);

        /// <summary>
        /// 指定单轴进行软起动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="midPos">第一段位置，单位unit</param>
        /// <param name="tarPos">第二段位置，单位unit</param>
        /// <param name="startVel">启动速度，单位unit/s</param>
        /// <param name="maxVel1">第一段最高速度，单位unit/s</param>
        /// <param name="endVel1">第一段结束速度，单位unit/s</param>
        /// <param name="delayMs">第一段结束后延迟时间</param>
        /// <param name="maxVel2">第二段最高速度，单位unit/s</param>
        /// <param name="endVel2">第二段结束速度，单位unit/s</param>
        /// <param name="acc">加速度，单位unit/s/s</param>
        /// <param name="dec">减速度，单位unit/s/s</param>
        /// <param name="sTime">平滑系数 0-100</param>
        /// <param name="moveMode">运动模式 0-相对运动，1-绝对运动</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SoftStart_Move_Unit(short axis, double midPos, double tarPos, double startVel, double maxVel1, double endVel1, uint delayMs,
double maxVel2, double endVel2, double acc, double dec, short sTime, short moveMode, short card = 0);

        /// <summary>
        /// 单轴软着陆
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="midPos">第一段位置，单位unit</param>
        /// <param name="tarPos">>第二段位置，单位unit</param>
        /// <param name="startVel">启动速度，单位unit/s</param>
        /// <param name="maxVel">最高速度，单位unit/s</param>
        /// <param name="stopVel">着陆速度，单位unit/s</param>
        /// <param name="acc">加速度，单位unit/s/s</param>
        /// <param name="dec">减速度，单位unit/s/s</param>
        /// <param name="sTime">平滑系数 0-100</param>
        /// <param name="moveMode">运动模式 0-相对运动，1-绝对运动</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SoftLanding_Move_Unit(short axis, double midPos, double tarPos, double startVel, double maxVel, double stopVel,
double acc, double dec, short sTime, short moveMode, short card = 0);



        #endregion

        #region 单轴连续运动

        /// <summary>
        /// 设置指定轴进行单轴的连续运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="acc">加速度</param>
        /// <param name="dec">减速度</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPVMode(short axis, double acc, double dec, short card = 0);

        /// <summary>
        /// :设置指定轴单轴连续运动的点位。
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">位置</param>
        /// <param name="vel">速度</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPVDataAbs(short axis, long pos, double vel, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPVDataAbs_Unit(short axis, double pos, double vel, short card = 0);

        /// <summary>
        /// 查询单轴连续运动的缓存区剩余空间
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="lSpace">剩余空间数量</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPVSpace(short axis, ref int lSpace, short card = 0);

        /// <summary>
        /// 清除单轴连续运动缓存区中的数据
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPVClear(short axis, short card = 0);

        /// <summary>
        /// 指定轴开始连续运动
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisPVStart(short axis, short card = 0);

        #endregion

        #region 单轴补偿函数

        /// <summary>
        /// 设置螺距补偿参数
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="n">补偿位置个数</param>
        /// <param name="startPos">补偿开始位置</param>
        /// <param name="lenPos">补偿距离</param>
        /// <param name="pCompPos">正向补偿表数组地址</param>
        /// <param name="pCompNeg">负向补偿表数组地址</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetLeadScrewComp(short axis, short n, int startPos, int lenPos, ref int pCompPos, ref int pCompNeg, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetLeadScrewComp_Unit(short axis, short n, double startPos, double lenPos, ref double pCompPos, ref double pCompNeg, short card = 0);

        /// <summary>
        /// 启动/关闭螺距补偿功能
        /// </summary>
        /// <param name="axis"> 轴号，从1开始</param>
        /// <param name="enable"> 启动标志，0-取消补偿；1-启动补偿</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EnableLeadScrewComp(short axis, short enable, short card = 0);


        /// <summary>
        /// 读取螺距补偿信息
        /// </summary>
        /// <param name="axis">轴号，从1开始</param>
        /// <param name="enable">启动标志，0-取消补偿；1-启动补偿</param>
        /// <param name="prfPos">补偿后的真实规划位置</param>
        /// <param name="encPos">补偿后的真实反馈位置</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetLeadScrewCompInfo(short axis, ref short enable, ref int prfPos, ref int encPos, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetLeadScrewCompInfo_Unit(short axis, ref short enable, ref double prfPos, ref double encPos, short card = 0);


        /// <summary>
        /// 设置反向间隙补偿参数
        /// </summary>
        /// <param name="axis"> 轴号，从1开始</param>
        /// <param name="compValue">反向补偿值，等于0表示不补偿，正数有效</param>
        /// <param name="incValue">间隙补偿的变化量，等于0或者大于补偿值时补偿量将直接补偿</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetBacklash(short axis, uint compValue, uint incValue, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetBacklash_Unit(short axis, double compValue, double incValue, short card = 0);

        /// <summary>
        /// 读取反向间隙补偿参数
        /// </summary>
        /// <param name="axis"> 轴号，从1开始</param>
        /// <param name="pCompValue">反向补偿值，等于0表示不补偿，正数有效</param>
        /// <param name="pIncValue">间隙补偿的变化量，等于0或者大于补偿值时补偿量将直接补偿</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetBacklash(short axis, ref uint pCompValue, ref uint pIncValue, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetBacklash_Unit(short axis, ref double pCompValue, ref double pIncValue, short card = 0);


        #endregion

        #region 低速位置比较触发
        ////////////////////////////////////////////////////////////////////////////////
        //
        // 以下为低速位置比较功能相关指令
        //
        ////////////////////////////////////////////////////////////////////////////////
        /*********************************************************
        函数说明：设置低速位置比较器
        参数说明：axis      - 轴号，从1开始
                  enable    - 启动比较器，0 - 禁用；1 - 启动
                  cmp_src   - 比较源，0 - 位置规划值；1 - 位置反馈值
                  card      - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare_Set_Config(short axis, short enable, short cmp_src, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare_Get_Config(short axis, ref short enable, ref short cmp_src, short card = 0);

        /*********************************************************
        函数说明：清除所有位置比较数据
        参数说明：axis      - 轴号，从1开始
                  card      - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare_Clear_Data(short axis, short card = 0);

        /*********************************************************
        函数说明：添加位置比较数据
        参数说明：axis    - 轴号，从1开始
                  pos     - 位置比较位置
                  dir     - 比较模式，0 - 小于等于；1 = 大于等于
                  doChn   - DO输出通道号，从1开始
                  cycle   - DO输出周期数，等于0表示level状态一直输出，非0表示输出level状态持续cycle周期后翻转
                  level   - DO输出状态，0或1
                  card    - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare_Add_Data(short axis, double pos, short dir, short doChn, uint cycle, short level, short card = 0);

        /*********************************************************
        函数说明：查询低速位置比较状态
        参数说明：axis      - 轴号，从1开始
                  spaceNum  - 可添加的比较位置点数量
                  usedNum   - 已比较的位置点数量
                  currPos   - 当前正在进行比较的位置，当未添加任何比较数据时此参数返回值意义
                  card      - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare_Get_Status(short axis, ref short spaceNum, ref short usedNum, ref double currPos, short card = 0);

        /*********************************************************
        函数说明：设置2维低速位置比较器
        参数说明：enable    - 启动比较器，0 - 禁用；1 - 启动
                  cmp_src   - 比较源，0 - 位置规划值；1 - 位置反馈值
                  card      - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare2D_Set_Config(short enable, short cmp_src, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare2D_Get_Config(ref short enable, ref short cmp_src, short card = 0);

        /*********************************************************
        函数说明：清除2维低速所有位置比较数据
        参数说明：card      - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare2D_Clear_Data(short card = 0);

        /*********************************************************
        函数说明：添加2维位置比较数据
        参数说明：axis    - 轴号数组（2维），轴号从1开始
                  pos     - 比较位置（2维）
                  dir     - 比较模式（2维），0 - 小于等于；1 = 大于等于
                  doChn   - DO输出通道号，从1开始
                  cycle   - DO输出周期数，等于0表示level状态一直输出，非0表示输出level状态持续cycle周期后翻转
                  level   - DO输出状态，0或1
                  card    - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare2D_Add_Data(ref short axis, ref double pos, ref short dir, short doChn, uint cycle, short level, short card = 0);

        /*********************************************************
        函数说明：查询2维低速位置比较状态
        参数说明：spaceNum  - 可添加的比较位置点数量
                  usedNum   - 已比较的位置点数量
                  currPos   - 当前正在进行比较的位置（2维），当未添加任何比较数据时此参数返回值意义
                  card      - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_lCompare2D_Get_Status(ref short spaceNum, ref short usedNum, ref double currPos, short card = 0);


        #endregion

        #region 多轴同动运动

        /// <summary>
        /// 多轴同动运动
        /// </summary>
        /// <param name="demension">多轴维度</param>
        /// <param name="axis">参与同动轴的数组首地址</param>
        /// <param name="position">运动坐标的位置首地址</param>
        /// <param name="acc">运动合成加速度</param>
        /// <param name="vel">运动合成速度</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Line_All(short demension, ref short axis, ref int position, double acc, double vel, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Line_All_Unit(short dim, ref short axisarray, ref double positionarray, double acc, double vel, short card = 0);

        /// <summary>
        /// 设置多轴连续同动
        /// </summary>
        /// <param name="dim">多轴维度</param>
        /// <param name="axis">轴号数组</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Buf_Line_Configure(short dim, ref short axis, short card = 0);

        /// <summary>
        /// 设置多轴连续同动点位
        /// </summary>
        /// <param name="dim">多轴维度</param>
        /// <param name="axis">轴号数组</param>
        /// <param name="position">位置数组</param>
        /// <param name="acc">点位合成加速度</param>
        /// <param name="vel">点位合成速度</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Buf_Line_Data(short dim, ref short axis, ref int position, double acc, double vel, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Buf_Line_Data_Unit(short dim, ref short axis, ref double position, double acc, double vel, short card = 0);

        /// <summary>
        /// 开启多轴连续同动
        /// </summary>
        /// <param name="dim">多轴维度</param>
        /// <param name="axis">轴号数组</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Buf_Line_Start(short dim, ref short axis, short card = 0);

        #endregion

        #region PT运动
        /// <summary>
        /// 设置指定轴为PT运动模式
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="mode">FIFO模式,0-静态模式，1-动态模式</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PrfPt(short axis, short mode, short card);

        /// <summary>
        /// 添加PT运动中的DO输出命令
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="doChannel">DO通道号</param>
        /// <param name="val">输出值：0-关闭，1-开启</param>
        /// <param name="advanceTime">提前时间(ms)</param>
        /// <param name="fifo">FIFO缓冲区号，默认0</param>
        /// <param name="card">卡号，默认0，0或1</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtDataDO(short axis, short doChannel, short val, short advanceTime, short fifo = 0, short card = 0);

        /// <summary>
        /// 添加PT运动中的脉冲输出命令
        /// </summary>
        /// 预留接口，暂未实现
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtDataPulseEx(short axis, short pulseChn, short pulsecnt, short fifo, short card = 0);

        /// <summary>
        /// 获取PT运动缓冲区剩余空间
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pSpace">返回剩余空间点数</param>
        /// <param name="fifo">FIFO缓冲区号</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtSpace(short axis, out short pSpace, short fifo, short card);

        /// <summary>
        /// 批量添加PT运动数据
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="dataCount">数据数量</param>
        /// <param name="posArray">位置数组</param>
        /// <param name="timeArray">时间数组</param>
        /// <param name="typeArray">运动类型数组，数据段类型 0:普通段，1:匀速段（常用），2:减速段</param>
        /// <param name="fifo">FIFO缓冲区号，默认0</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtMultiData(short axis, short dataCount, ref int posArray, ref int timeArray, ref short typeArray, short fifo = 0, short card = 0);

        /// <summary>
        /// 添加多轴PT运动数据
        /// </summary>
        /// <param name="axisArray">轴号数组</param>
        /// <param name="posArray">位置数组</param>
        /// <param name="timeArray">时间数组</param>
        /// <param name="count">轴数量，默认1</param>
        /// <param name="type">运动类型数组，数据段类型 0:普通段，1:匀速段（常用），2:减速段</param>
        /// <param name="fifo">FIFO缓冲区号，默认0</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtDataMulAxis(ref short axisArray, ref int posArray, ref int timeArray, short count, short type, short fifo = 0, short card = 0);

        /// <summary>
        /// 添加单轴PT运动数据
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="pos">目标位置</param>
        /// <param name="time">运动时间(ms)</param>
        /// <param name="type">运动类型数组，数据段类型 0:普通段，1:匀速段（常用），2:减速段</param>
        /// <param name="fifo">FIFO缓冲区号</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtData(short axis, int pos, int time, short type, short fifo, short card);

        /// <summary>
        /// 清空PT运动缓冲区
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="fifo">FIFO缓冲区号</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtClear(short axis, short fifo, short card);

        /// <summary>
        /// 启动PT运动
        /// </summary>
        /// <param name="mask">轴掩码，按位指示需要对应开始 PT 运动的轴号，Bit0 为轴 1，Bit1 为轴 2，以此类推</param>
        /// <param name="option">按位指示需要控制的轴的 FIFO ，Bit0 对应轴 1，Bit1对应轴 2，以此类推。BIT 为 0 时使用 FIFO0，BIT 为 1 时使用 FIFO1</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_PtStart(uint mask, uint option, short card);

        /// <summary>
        /// 设置PT运动循环次数
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="loop">循环次数</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetPtLoop(short axis, int loop, short card);

        /// <summary>
        /// 获取PT运动循环次数
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="loop">返回循环次数</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetPtLoop(short axis, ref int loop, short card);

        /// <summary>
        /// 设置PT运动内存模式
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="memory">内存模式</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetPtMemory(short axis, short memory, short card);

        /// <summary>
        /// 获取PT运动内存模式
        /// </summary>
        /// <param name="axis">轴号</param>
        /// <param name="memory">返回内存模式</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetPtMemory(short axis, out short memory, short card);

        #endregion

        #region 电子齿轮
        /// <summary>
        /// 设置电子齿轮模式
        /// </summary>
        /// <param name="axis">从轴号，从1开始</param>
        /// <param name="dir">跟随方向，0：双向跟随，1：正向跟随，-1：负向跟随</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Gear(short axis, short dir, short card);

        /// <summary>
        /// 设置电子齿轮主轴参数
        /// </summary>
        /// <param name="axis">从轴号</param>
        /// <param name="masterindex">主轴号</param>
        /// <param name="masterType">主轴对象，1:主轴编码器反馈、2:主轴规划位置、3:主轴 、4:主轴规划输出值 、5:主轴编码器输出值</param>
        /// <param name="masterItem">轴类型，masterType 为 3 时生效、0 表示主轴规划输出值、1 表示主轴的编码器输出</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetGearMaster(short axis, short masterindex, short masterType, short masterItem, short card);

        /// <summary>
        /// 获取电子齿轮主轴参数
        /// </summary>
        /// <param name="axis">从轴号</param>
        /// <param name="masterindex">主轴号</param>
        /// <param name="masterType">返回主轴对象，1:主轴编码器反馈、2:主轴规划位置、3:主轴 、4:主轴规划输出值 、5:主轴编码器输出值</param>
        /// <param name="masterItem">返回轴类型，masterType 为 3 时生效、0 表示主轴规划输出值、1 表示主轴的编码器输出</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetGearMaster(short axis, out short masterindex, out short masterType, out short masterItem, short card);

        /// <summary>
        /// 设置电子齿轮比率
        /// </summary>
        /// <param name="axis">从轴号</param>
        /// <param name="masterEven">主轴的传动比系数</param>
        /// <param name="slaveEven">从轴的传动比系数</param>
        /// <param name="masterSlope">主轴离合区位移，范围>=0 并且!=1</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetGearRatio(short axis, int masterEven, int slaveEven, int masterSlope, short card);

        /// <summary>
        /// 获取电子齿轮比率
        /// </summary>
        /// <param name="axis">从轴号</param>
        /// <param name="masterEven">主轴的传动比系数</param>
        /// <param name="slaveEven">从轴的传动比系数</param>
        /// <param name="masterSlope">主轴离合区位移，范围>=0 并且!=1</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetGearRatio(short axis, out int masterEven, out int slaveEven, out int masterSlope, short card);

        /// <summary>
        /// 启动电子齿轮运动（多轴）
        /// </summary>
        /// <param name="mask">轴掩码，每个bit对应一个轴</param>
        /// <param name="card">卡号</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GearStart(uint mask, short card);

        /// <summary>
        /// 启动电子齿轮运动（单轴）
        /// </summary>
        /// <param name="axis">从轴号</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GearStartSingleAxis(short axis, short card = 0);



        #endregion

        #region 连续插补

        /// <summary>
        /// 设置连续插补的坐标系参数
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="pCrdPrm">坐标系参数结构体CrdPrm的内存指针</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCrd(short crd, ref CrdCfg pCrdPrm, short card);

        /// <summary>
        /// 设置连续插补的坐标系参数,以Unit为单位
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="pCrdPrm">坐标系参数结构体CrdPrm_Unit的内存指针</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCrd_Unit(short crd, ref CrdCfg_Unit pCrdPrm, short card = 0);

        /// <summary>
        /// 获取连续插补的坐标系参数
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="pCrdPrm">坐标系参数结构体CrdPrm的内存指针</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCrd(short crd, out CrdCfg pCrdPrm, short card);

        /// <summary>
        /// 获取连续插补的坐标系参数,以Unit为单位
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="pCrdPrm">坐标系参数结构体CrdPrm_Unit的内存指针</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCrd_Unit(short crd, ref CrdCfg_Unit pCrdPrm, short card = 0);

        /// <summary>
        /// 读取连续插补队列中剩余容量
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="pSpace">插补队列中剩余容量</param>
        /// <param name="count">获取的数量</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdSpace(short crd, out int pSpace, short count, short fifo, short card);

        /// <summary>
        /// 清除坐标系与FIFO中的位置缓存
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="count">计数，默认为1</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdClear(short crd, short count, short fifo, short card);

        /// <summary>
        /// 获取缓冲区中最后一个点的坐标
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="position">传出的数组首地址</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetLastCrdPos(short crd, out int position, short fifo, short card);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetLastCrdPos_Unit(short crd, out double position, short fifo, short card = 0);

        /// <summary>
        /// 向缓冲区压入DO命令
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="channel">DO的通道号</param>
        /// <param name="doValue">设置DO输出的状态 0：无输出，1：有输出</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_BufIO(short crd, ushort channel, ushort doValue, short fifo, short card);

        /// <summary>
        /// 向缓冲区压入DOPORT命令，一次性输出多个DO点位。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="slaveAddr">需要输出的DOPORT所在的IOM号</param>
        /// <param name="chnBegin">DOPORT输出的起始通道号</param>
        /// <param name="sValue">DOPORT的输出数值，从chnBegin开始每个bit对应一个通道。</param>
        /// <param name="sMask">输出掩码，从chnBegin开始每个bit对应一个通道的掩码，对应bit为1时，该通道的状态才能够被改写。</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_BufMultiDO(short crd, short slaveAddr, short chnBegin, short sValue, short sMask, short fifo = 0, short card = 0);

        /// <summary>
        /// 向缓冲区压入DO脉冲命令。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="channel">DO的通道号</param>
        /// <param name="pulseWidth">输出脉宽值，单位ms</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_BufPulse(short crd, short channel, ushort pulseWidth, short fifo = 0, short card = 0);

        /// <summary>
        /// 向缓冲区压入等待DI命令
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="channel">Di的通道号</param>
        /// <param name="sValue">等待的DI状态</param>
        /// <param name="timeout">等待的超时时间，如果超时则退出CRD，STATUS为3</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_BufWaitDI(short crd, short channel, short sValue, ushort timeout, short fifo = 0, short card = 0);

        /// <summary>
        /// 向缓冲区压入等待Do命令
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="channel">Do的通道号</param>
        /// <param name="sValue">等待的Do状态</param>
        /// <param name="timeout">等待的超时时间，如果超时则退出CRD，STATUS为3</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_BufWaitDO(short crd, short channel, short sValue, ushort timeout, short fifo = 0, short card = 0);

        /// <summary>
        /// 向缓冲区压入延时命令
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="delayTime">延迟的时间，单位是总线周期。范围是1~65535</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_BufDelay(short crd, uint delayTime, short fifo, short card);

        /// <summary>
        /// 使对应坐标系开始运行连续插补运动
        /// </summary>
        /// <param name="mask">mask的bit0为1：启动坐标系1，bit1：坐标系2</param>
        /// <param name="option">option的bit0为坐标系1，bit1为坐标系2。对应位为0启用FIFO0，为1启用FIFO1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdStart(short mask, short option, short card);

        /// <summary>
        /// 使对应坐标系开始停止连续插补运动
        /// </summary>
        /// <param name="mask">mask的bit0为1：启动坐标系1，bit1：坐标系2</param>
        /// <param name="option">Option 是对应的停止方法，0：平滑停止，1：急停</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdStop(short mask, short option, short card);

        /// <summary>
        /// 获取缓冲区的运行状态
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="pSts">插补运动状态 0：不在运动，1：正在运动  2：暂停 3：错误</param>
        /// <param name="pCmdNum">FIFO已存的命令空间</param>
        /// <param name="pSpace">FIFO剩余空间</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdStatus(short crd, out short pSts, out short pCmdNum, out int pSpace, short fifo, short card);

        /// <summary>
        /// 预留接口
        /// </summary>
        /// <param name="crd"></param>
        /// <param name="pPos"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCrdPos(short crd, out double pPos, short card);

        /// <summary>
        /// 预留接口
        /// </summary>
        /// <param name="crd"></param>
        /// <param name="pSynVel"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCrdVel(short crd, out double pSynVel, short card);

        /// <summary>
        /// 预留接口
        /// </summary>
        /// <param name="crd"></param>
        /// <param name="decSmoothStop"></param>
        /// <param name="decAbruptStop"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCrdStopDec(short crd, double decSmoothStop, double decAbruptStop, short card);

        /// <summary>
        /// 预留接口
        /// </summary>
        /// <param name="crd"></param>
        /// <param name="decSmoothStop"></param>
        /// <param name="decAbruptStop"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCrdStopDec(short crd, out double decSmoothStop, out double decAbruptStop, short card);

        /// <summary>
        /// 向缓存区压入2维圆弧半径加终点方式的螺旋线插补，Z向螺旋线运动。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="x">螺旋线圆弧的终点坐标 X</param>
        /// <param name="y">螺旋线圆弧的终点坐标 Y</param>
        /// <param name="z">螺旋线圆弧的终点坐标 Z</param>
        /// <param name="radius">螺旋线圆弧的半径</param>
        /// <param name="circleDir">螺旋线圆弧的旋转方向 0-顺时针 1-逆时针</param>
        /// <param name="pitch">螺距值，xy运动一整个圆时Z移动的距离</param>
        /// <param name="synVel">插补的最大线速度</param>
        /// <param name="synAcc">插补的最大加速度</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HelicalLineXYR(short crd, int x, int y, int z, double radius, short circleDir, double pitch, double synVel, double synAcc, short fifo, short card);

        /// <summary>
        /// 向缓存区压入2维圆弧半径加终点方式的螺旋线插补，X向螺旋线运动。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="y">螺旋线圆弧的终点坐标 Y</param>
        /// <param name="z">螺旋线圆弧的终点坐标 Z</param>
        /// <param name="x">螺旋线圆弧的终点坐标 X</param>
        /// <param name="radius">螺旋线圆弧的半径</param>
        /// <param name="circleDir">螺旋线圆弧的旋转方向 0-顺时针 1-逆时针</param>
        /// <param name="pitch">螺距值，YZ运动一整个圆时X移动的距离</param>
        /// <param name="synVel">插补的最大线速度</param>
        /// <param name="synAcc">插补的最大加速度</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HelicalLineYZR(short crd, int y, int z, int x, double radius, short circleDir, double pitch, double synVel, double synAcc, short fifo, short card);

        /// <summary>
        /// 向缓存区压入2维圆弧半径加终点方式的螺旋线插补，Y向螺旋线运动。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="z">螺旋线圆弧的终点坐标 Z</param>
        /// <param name="x">螺旋线圆弧的终点坐标 X</param>
        /// <param name="y">螺旋线圆弧的终点坐标 Y</param>
        /// <param name="radius">螺旋线圆弧的半径</param>
        /// <param name="circleDir">螺旋线圆弧的旋转方向 0-顺时针 1-逆时针</param>
        /// <param name="pitch">螺距值，ZX运动一整个圆时Y移动的距离</param>
        /// <param name="synVel">插补的最大线速度</param>
        /// <param name="synAcc">插补的最大加速度</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HelicalLineZXR(short crd, int z, int x, int y, double radius, short circleDir, double pitch, double synVel, double synAcc, short fifo, short card);

        /// <summary>
        /// 向缓存区压入2维圆弧圆心加终点方式的螺旋线插补，Z向螺旋线运动。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="x">螺旋线圆弧的终点坐标 X</param>
        /// <param name="y">螺旋线圆弧的终点坐标 Y</param>
        /// <param name="z">螺旋线圆弧的终点坐标 Z</param>
        /// <param name="xCenter">螺旋线圆弧的圆心相对坐标 X</param>
        /// <param name="yCenter">螺旋线圆弧的圆心相对坐标Y</param>
        /// <param name="circleDir">螺旋线圆弧的旋转方向 0-顺时针 1-逆时针</param>
        /// <param name="pitch">螺距值，xy运动一整个圆时Z移动的距离</param>
        /// <param name="synVel">插补的最大线速度</param>
        /// <param name="synAcc">插补的最大加速度</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HelicalLineXYC(short crd, int x, int y, int z, double xCenter, double yCenter, short circleDir, double pitch, double synVel, double synAcc, short fifo, short card);

        /// <summary>
        /// 向缓存区压入2维圆弧圆心加终点方式的螺旋线插补，X向螺旋线运动。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="y">螺旋线圆弧的终点坐标 Y</param>
        /// <param name="z">螺旋线圆弧的终点坐标 Z</param>
        /// <param name="x">螺旋线圆弧的终点坐标 X</param>
        /// <param name="yCenter">螺旋线圆弧的圆心相对坐标 Y</param>
        /// <param name="zCenter">螺旋线圆弧的圆心相对坐标Z</param>
        /// <param name="circleDir">螺旋线圆弧的旋转方向 0-顺时针 1-逆时针</param>
        /// <param name="pitch">螺距值，YZ运动一整个圆时X移动的距离</param>
        /// <param name="synVel">插补的最大线速度</param>
        /// <param name="synAcc">插补的最大加速度</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HelicalLineYZC(short crd, int y, int z, int x, double yCenter, double zCenter, short circleDir, double pitch, double synVel, double synAcc, short fifo, short card);

        /// <summary>
        /// 向缓存区压入2维圆弧圆心加终点方式的螺旋线插补，Y向螺旋线运动。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="z">螺旋线圆弧的终点坐标 Z</param>
        /// <param name="x">螺旋线圆弧的终点坐标 X</param>
        /// <param name="y">螺旋线圆弧的终点坐标 Y</param>
        /// <param name="zCenter">螺旋线圆弧的圆心相对坐标 Z</param>
        /// <param name="xCenter">螺旋线圆弧的圆心相对坐标X</param>
        /// <param name="circleDir">螺旋线圆弧的旋转方向 0-顺时针 1-逆时针</param>
        /// <param name="pitch">螺距值，ZX运动一整个圆时Y移动的距离</param>
        /// <param name="synVel">插补的最大线速度</param>
        /// <param name="synAcc">插补的最大加速度</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_HelicalLineZXC(short crd, int z, int x, int y, double zCenter, double xCenter, short circleDir, double pitch, double synVel, double synAcc, short fifo, short card);

        /// <summary>
        /// 初始化速度前瞻功能
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="T">拐角时间，单位为cycletime，经验值是1-100。</param>
        /// <param name="accMax">最大加速度，单位： pulse/S/S。</param>
        /// <param name="n">缓冲区大小，不小于压入点的大小，且需要小于FIFO容量</param>
        /// <param name="pCrdData">结构体数组指针首地址，存放规划计算的参数结果，不需要填入数值，只需要分配内存，将该结构体数组指针传入即可</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetVelPlanning(short crd, short fifo, double T, double accMax, short n, ref CrdBlockData pCrdData, short card);

        /// <summary>
        /// 将速度前瞻计算数据推送到FIFO中
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="pCrdData">结构体数组指针首地址，将之前的计算结果的首地址给出即可</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdData(short crd, ref CrdBlockData pCrdData, short fifo, short card); //CrdBlockData

        /// <summary>
        /// 向缓存区压入直线插补命令，最多2048个点
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="dimension">插补维度</param>
        /// <param name="axisArray">插补的参与轴数组首地址</param>
        /// <param name="posArray">插补位置数组首地址，位置元素和轴数组中的元素一一对应</param>
        /// <param name="mVel">插补的最大线速度</param>
        /// <param name="acc">插补的最大加速度</param>
        /// <param name="velEnd">插补的结束速度</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Line(short crd, short dimension, ref short axisArray, ref int posArray, double mVel, double acc, double velEnd = 0, short fifo = 0, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Line_Unit(short crd, short dimension, ref short axisArray, ref double posArray, double mVel, double acc, double velEnd = 0, short fifo = 0, short card = 0);

        /// <summary>
        /// 向缓存区压入2维圆弧插补命令，最多2048个点
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="axisArray">参与插补的轴数组首地址</param>
        /// <param name="posArray">圆弧的终点位置数组首地址</param>
        /// <param name="radius">圆弧半径</param>
        /// <param name="circleDir">旋转方向 0：顺时针  1：逆时针</param>
        /// <param name="mVel">该段运动的最大速度，若大于坐标系速度以坐标系速度运行</param>
        /// <param name="synAcc">该段运动的最大加速度，若大于坐标系加速度以坐标系加速度运行</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Arc2R(short crd, ref short axisArray, ref int posArray, double radius, short circleDir, double mVel, double synAcc, short fifo = 0, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Arc2R_Unit(short crd, ref short axisArray, ref double posArray, double radius, short circleDir, double mVel, double synAcc, short fifo = 0, short card = 0);

        /// <summary>
        /// 向缓存区压入2维圆弧插补命令，最多2048个点
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="axisArray">参与插补的轴数组首地址</param>
        /// <param name="posArray">圆弧的终点位置数组首地址</param>
        /// <param name="centerArray">圆弧的圆心数组首地址，圆心坐标为相对坐标</param>
        /// <param name="circleDir">旋转方向 0：顺时针  1：逆时针</param>
        /// <param name="mVel">该段运动的最大速度，若大于坐标系速度以坐标系速度运行</param>
        /// <param name="synAcc">该段运动的最大加速度，若大于坐标系加速度以坐标系加速度运行</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Arc2C(short crd, ref short axisArray, ref int posArray, ref double centerArray, short circleDir, double mVel, double synAcc, short fifo = 0, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Arc2C_Unit(short crd, ref short axisArray, ref double posArray, ref double centerArray, short circleDir, double mVel, double synAcc, short fifo = 0, short card = 0);

        /// <summary>
        /// 向插补坐标系中插入3D圆弧
        /// </summary>
        /// <param name="crd">坐标系号</param>
        /// <param name="endPosArray">终点位置坐标数组首地址</param>
        /// <param name="midPosArray">圆弧过程点位置坐标数组首地址</param>
        /// <param name="synVel">合成速度</param>
        /// <param name="synAcc">合成加速度</param>
        /// <param name="velEnd">停止速度</param>
        /// <param name="fifo">FIFO号</param>
        /// <param name="card">卡号</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_Arc3D(short crd, ref int endPosArray, ref int midPosArray, double synVel, double synAcc, double velEnd = 0, short fifo = 0, short card = 0);

        /// <summary>
        /// 使对应坐标系暂停连续插补运动。使用暂停和继续函数时，只针对每个CRD的FIFO-0生效。FIFO-1无效。
        /// </summary>
        /// <param name="crd">坐标系号，1或者2</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdPause(short crd, short card = 0);

        /// <summary>
        /// 使对应坐标系继续连续插补运动。使用暂停和继续函数时，只针对每个CRD的FIFO-0生效。FIFO-1无效。
        /// </summary>
        /// <param name="crd">坐标系号，1或者2</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdContinue(short crd, short card = 0);

        /// <summary>
        /// 使对应坐标系开始运行连续插补运动
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CrdStartSingle(short crd, short fifo = 0, short card = 0);

        /// <summary>
        /// 使对应坐标系开始运行连续插补运动
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="axis">需要跟随的轴号，不能使用CRD占用的轴</param>
        /// <param name="pos">跟随轴的移动位置</param>
        /// <param name="fifo">先入先出缓存器，从0开始计数，共两个，0和1</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_BufAxisGear(short crd, short axis, int pos, short fifo = 0, short card = 0);

        /// <summary>
        /// 设置连续插补的加速度保护，当加速度达到单轴的急停减速度时会触发保护。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="sActive">是否开启保护</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCrdAccProtectActive(short crd, short sActive, short card = 0);

        /// <summary>
        /// 获取连续插补的加速度保护，当加速度达到单轴的急停减速度时会触发保护。
        /// </summary>
        /// <param name="crd">坐标系编号，可输入1,2</param>
        /// <param name="sActive">是否开启保护</param>
        /// <param name="card">卡片卡号，从0开始，默认按主板插槽号排序</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetCrdAccProtectActive(short crd, ref short sActive, short card = 0);

        #endregion

        #region 看门狗函数
        /// <summary>
        /// 设置看门狗使能状态和周期
        /// </summary>
        /// <param name="enable">使能状态：0-禁用，1-启用</param>
        /// <param name="period">看门狗周期(ms)</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetWatchdogEnable(short enable, int period, short card = 0);

        /// <summary>
        /// 获取看门狗使能状态和周期
        /// </summary>
        /// <param name="enable">返回使能状态：0-禁用，1-启用</param>
        /// <param name="period">返回看门狗周期(ms)</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetWatchdogEnable(ref short enable, ref int period, short card = 0);

        /// <summary>
        /// 设置看门狗触发动作
        /// </summary>
        /// <param name="actionMask">动作掩码，每个bit对应一个动作</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetWatchdogAction(int actionMask, short card = 0);

        /// <summary>
        /// 获取看门狗触发动作设置
        /// </summary>
        /// <param name="actionMask">返回动作掩码，每个bit对应一个动作</param>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetWatchdogAction(ref int actionMask, short card = 0);

        /// <summary>
        /// 重置看门狗计数器（喂狗）
        /// </summary>
        /// <param name="card">卡号，默认0</param>
        /// <returns>执行结果：0-成功，非0-错误码</returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_WatchdogReset(short card = 0);


        #endregion

        #region 高速位置比较函数（针对M60-ED系列）


        /// <summary>
        /// 设置位置比较模式
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="dim">比较的维数，取值[1-2]</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetCompareMode(short channel, short dim, short card = 0);

        /// <summary>
        /// 触发端口强制信号输出
        /// </summary>
        /// <param name="channel">channel    - 通道号，取值[1-2]</param>
        /// <param name="startLevel">输出起始电平，0 - 默认电平；1 - 电平取反</param>
        /// <param name="outputType">输出类型，0 - 脉冲输出，1 - 电平输出</param>
        /// <param name="time">脉冲模式下表示脉冲宽度（单位 100us），电平模式无效</param>
        /// <param name="pluseCount">脉冲输出个数，范围[0-65535]，其中为0时表示持续输出</param>
        /// <param name="spaceTime">脉冲输出的间隔时间（单位 100us)</param>
        /// <param name="delayTime">延时触发时间（单位 100us)</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareForceOutput(short channel, short startLevel, short outputType, short time, ushort pluseCount,
                                           ushort spaceTime, ushort delayTime = 0, short card = 0);

        /// <summary>
        /// 设置位置比较参数
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="pPrm">位置比较参数对应的结构体，参考相关定义</param>
        /// <param name="mode">触发脉冲输出的模式，0 - 每次触发输出一系列相同的脉冲，用M_CompareSetData指令设置触发位置；
        ///                                         1 - 每次触发输出不同的脉冲系列，用M_CompareSetDataEx指令设置触发位置和脉冲参数
        ///                                         </param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareSetParam(short channel, ref TPComparePrm pPrm, short mode = 0, short card = 0);

        /// <summary>
        /// 往位置比较模块添加比较的位置信息
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="count">添加位置数据的个数，[1-1056]</param>
        /// <param name="pBuf">存放位置数据的指针</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareSetData(short channel, short count, ref TPCompareData pBuf, short card = 0);

        /// <summary>
        /// 往位置比较模块添加比较的位置信息(输出不同的脉冲）
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="count">添加位置数据的个数，[1-1056]</param>
        /// <param name="pBuf">存放位置数据的指针</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareSetDataEx(short channel, short count, TPCompareDataEx pBuf, short card = 0);

        /// <summary>
        /// 等间距位置比较（只适用于一维）
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="startPos">触发起始位置</param>
        /// <param name="repeatTimes">比较输出重复次数，需大于0</param>
        /// <param name="interval">触发位置间隔，正数表示正向间距，负数表示负向间距，不能为0</param>
        /// <param name="time">输出脉冲高电平宽度，以100us为单位</param>
        /// <param name="spacetime">输出脉冲低电平宽度，以100us为单位</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareLinear(short channel, int startPos, uint repeatTimes, int interval,
                                     ushort time, ushort spacetime, short card = 0);

        /// <summary>
        /// 清除位置比较缓冲区数据
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareClearData(short channel, short card = 0);

        /// <summary>
        /// 查询位置比较输出状态
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="pStatus">工作状态，0-空闲状态；1-正在强制输出；2-正在进行位置比较</param>
        /// <param name="pCount">位置比较已输出的个数</param>
        /// <param name="pFifoCount">当前缓冲区剩余空间，最大1024;</param>
        /// <param name="pBufCount">当前FPGA剩余空间，最大32;</param>
        /// <param name="pTriggerPos">触发时的位置，共4个位置</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareStatus(short channel, ref short pStatus, ref int pCount, ref short pFifoCount,
                                     ref short pBufCount, ref int pTriggerPos, short card = 0);

        /// <summary>
        /// 开始位置比较
        /// </summary>
        /// <param name="channel"> 通道号，取值[1-2]</param>
        /// <param name="card"> 卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareStart(short channel, short card = 0);

        /// <summary>
        /// 停止位置比较
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_CompareStop(short channel, short card = 0);

        #endregion

        #region 额外编码器函数（针对M60-ED系列）

        /// <summary>
        /// 读取编码器输入通道计数值
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="count">需要读取的通道数量</param>
        /// <param name="pPosBuf">保存数据的缓冲区</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAuxEncPosition(short channel, short count, ref int pPosBuf, short card = 0);

        /// <summary>
        /// 清除编码器输入通道计数值
        /// </summary>
        /// <param name="channel">通道号，取值[1-2]</param>
        /// <param name="count">需要读取的通道数量</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ClearAuxEncPosition(short channel, short count, short card = 0);
        #endregion

        #region 锁存函数（针对M60-ED系列）

        /// <summary>
        /// 设置位置锁存模式，并启动位置捕获
        /// </summary>
        /// <param name="encChn">编码器通道号，取值[1-2]</param>
        /// <param name="trigSrc">触发源，默认0</param>
        /// <param name="trigSense">触发沿，0-下降沿；1-上升沿</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetLatchMode(short encChn, short trigSrc, short trigSense, short card = 0);

        /// <summary>
        /// 获取位置锁存模式，并启动位置捕获
        /// </summary>
        /// <param name="encChn">编码器通道号，取值[1-2]</param>
        /// <param name="trigSrc">触发源，默认0</param>
        /// <param name="trigSense">触发沿，0-下降沿；1-上升沿</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetLatchMode(short encChn, ref short trigSrc, ref short trigSense, short card = 0);

        /// <summary>
        /// 读取位置锁存状态
        /// </summary>
        /// <param name="encChn">编码器通道号，取值[1-2]</param>
        /// <param name="status">捕获状态，0-未触发；1-已触发</param>
        /// <param name="pos">触发锁存的位置</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetLatchStatus(short encChn, ref short status, ref int pos, short card = 0);

        /*********************************************************
        函数说明：清除锁存状态
        参数说明：encChn    - 编码器通道号，取值[1-2]
                  card      - 卡号，从0开始
        返回说明: 参考返回值定义
        *********************************************************/
        /// <summary>
        /// 清除锁存状态
        /// </summary>
        /// <param name="encChn">编码器通道号，取值[1-2]</param>
        /// <param name="card">卡号，从0开始</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ClearLatchStatus(short encChn, short card = 0);

        #endregion

        #region 手轮函数（针对M60-ED系列）
        /// <summary>
        /// 设置手摇轮功能参数
        /// </summary>
        /// <param name="axis">控制轴号，从1开始[1-32]</param>
        /// <param name="encChannel">手摇轮脉冲信号输入的通道号，从0开始[0-1]</param>
        /// <param name="encMode">手摇轮脉冲信号模式，0-正交脉冲，1-脉冲+方向</param>
        /// <param name="multi">手摇轮倍率，正数表示相同方向，负数表示相反方向</param>
        /// <param name="smooth">平滑系数，范围：0-99</param>
        /// <param name="card">卡号，从0开始（若不对其赋值，默认为卡0）</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetAxisMPG(short axis, short encChannel, short encMode, int multi, short smooth, short card = 0);

        /// <summary>
        /// 获取手摇轮功能参数。
        /// </summary>
        /// <param name="axis">控制轴号，从1开始[1-32]</param>
        /// <param name="encChannel">手摇轮脉冲信号输入的通道号，从0开始[0-1]</param>
        /// <param name="encMode">手摇轮脉冲信号模式，0-正交脉冲，1-脉冲+方向</param>
        /// <param name="multi">手摇轮倍率，正数表示相同方向，负数表示相反方向</param>
        /// <param name="smooth">平滑系数，范围：0-99</param>
        /// <param name="card">卡号，从0开始（若不对其赋值，默认为卡0）</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetAxisMPG(short axis, ref short encChannel, ref short encMode, ref int multi, ref short smooth, short card = 0);

        /// <summary>
        /// 设置多轴手摇轮控制参数
        /// </summary>
        /// <param name="axisNum">控制轴的数量</param>
        /// <param name="axisList">控制轴数组首地址</param>
        /// <param name="encChannel">手摇轮脉冲信号输入的通道号，从0开始[0-1]</param>
        /// <param name="encMode">手摇轮脉冲信号模式，0-正交脉冲，1-脉冲+方向</param>
        /// <param name="smooth">平滑系数，范围：0-99</param>
        /// <param name="multiArray">手摇轮倍率数组首地址，正数表示同向，负数表示反向</param>
        /// <param name="card">卡号，从0开始（若不对其赋值，默认为卡0）</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetMultiAxisMPG(short axisNum, ref short axisList, short encChannel, short encMode, short smooth, ref int multiArray, short card = 0);

        /// <summary>
        /// 获取多轴手摇轮控制参数
        /// </summary>
        /// <param name="encChannel">手摇轮脉冲信号输入的通道号，从0开始[0-1]</param>
        /// <param name="smooth">平滑系数，范围：0-99</param>
        /// <param name="axisNum">控制轴的数量</param>
        /// <param name="axisList">控制轴数组首地址</param>
        /// <param name="multiArray">手摇轮倍率数组首地址，正数表示同向，负数表示反向</param>
        /// <param name="card">卡号，从0开始（若不对其赋值，默认为卡0）</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetMultiAxisMPG(short encChannel, ref short smooth, ref short axisNum, ref short axisList, ref int multiArray, short card = 0);

        /// <summary>
        /// 开始手摇轮功能
        /// </summary>
        /// <param name="axis">控制轴号，从1开始[1-32]</param>
        /// <param name="card">卡号，从0开始（若不对其赋值，默认为卡0）</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_AxisMPG_Start(short axis, short card = 0);

        /// <summary>
        /// 开始多轴手摇轮功能
        /// </summary>
        /// <param name="axisNum">控制轴的数量</param>
        /// <param name="axisList">控制轴数组首地址</param>
        /// <param name="card">卡号，从0开始（若不对其赋值，默认为卡0）</param>
        /// <returns></returns>
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_MultiAxisMPG_Start(short axisNum, ref short axisList, short card = 0);

        #endregion

        #region 示波器函数(预留功能)
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ScopeSetCfg(ref TScopeCfg scopeCfg, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ScopeGetCfg(ref TScopeCfg scopeCfg, short card = 0);

        /*********************************************************
        函数说明：设置采样参数
        参数说明：channelNum        - 采样通道数量
                  pDataType         - 采样数据类型，SCOPE_WATCH_xxxxxx
                  pDataIndex        - 采样数据索引号，比如电机轴从1开始
                  card              - 卡号，从0开始
        返回说明: 参考返回值定义
        ********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ScopeSetSamplePrm(short channelNum, ref short pDataType, ref short pDataIndex, short card = 0);
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ScopeGetSamplePrm(ref short pChannelNum, ref short pDataType, ref short pDataIndex, short card = 0);

        /*********************************************************
        函数说明: 读取采样状态
        参数说明：status            - 采样状态，0-未触发采样；1-采样中；2-采样结束；3-采样出错
                  dataReady         - 数据是否就绪，0-无数据；1-数据可以读取
                  totalPackageCnt   - 采样数据总包数
                  missedPackageCnt  - 丢包数
                  card              - 卡号，从0开始
        返回说明: 参考返回值定义
        ********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetScopeSampleStatus(ref short status, ref short dataReady, ref int totalPackageCnt, ref int missedPackageCnt, short card = 0);

        /*********************************************************
        函数说明: 读取数据采样
        参数说明：pData      - 采样数据缓冲区
                  chnNum     - 采样通道数量
                  dataNum    - 数据包所包含的数据个数
                  packageIndex - 数据包索引
                  card       - 卡号，从0开始
        返回说明: 参考返回值定义
        ********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetScopeSampleData(ref uint pData, ref short chnNum, ref short dataNum, ref int packageIndex, short card = 0);

        /*********************************************************
        函数说明: 启动或停止数据采样
        参数说明：enable - 0：停止采样；1：启动采样
                  card     - 卡号，从0开始
        返回说明: 参考返回值定义
        ********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_ScopeEnableSample(short enable, short card = 0);

        #endregion

        #region 预留功能

        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetPulseSyncParam(short pulseChn, short bindNo, short delay, ushort xpulse, ushort ypulse, ushort zpulse, ushort pulseOut, short card = 0);

        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_GetPulseSyncParam(short pulseChn, ref short bindNo, ref short delay, ref ushort xpulse, ref ushort ypulse, ref ushort pulseOut, short card = 0);
        /*********************************************************
        * 函数功能: 启动/关闭同步输出
        * 参数说明：pulseChn - 输出通道，设为1
        *			enable   - 0 - 关闭，1 - 启动
        *			card     - 卡号，从0开始
        ********************************************************/
        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_EnablePulseSync(short pulseChn, short enable, short card = 0);

        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern short M_SetObjectCenterBias(short crd, double biasX, double biasY, double biasU, short card = 0);

        #endregion
    }
}


