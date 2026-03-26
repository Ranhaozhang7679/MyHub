using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Luster.SimDevice.MotionCard.LC
{
    class MiniEcatLib
    {
        public struct E1O16ClassifyBatchData
        {
            public int PosCount;//数据总数

            public int chn0_cnt;
            public int chn1_cnt;
            public int chn2_cnt;
            public int chn3_cnt;
            public int chn4_cnt;
            public int chn5_cnt;
            public int chn6_cnt;
            public int chn7_cnt;
            public int chn8_cnt;
            public int chn9_cnt;
            public int chn10_cnt;
            public int chn11_cnt;
            public int chn12_cnt;
            public int chn13_cnt;
            public int chn14_cnt;
            public int chn15_cnt;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn0_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn1_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn2_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn3_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn4_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn5_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn6_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn7_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn8_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn9_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn10_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn11_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn12_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn13_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn14_pos;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public ulong[] chn15_pos;
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void E1O16LTCCallbackDelegate(long ltcpos, int ltccnt);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mb_E1O16LTC_SetWaitLTCCallBack(int slaveno, int ltcno, E1O16LTCCallbackDelegate callback);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetRegister(int SlaveID, int Register);
        /// <summary>
        /// 获取模块版本信息
        /// </summary>
        /// <param name="version">版本号</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_GetVersion(int SlaveNo, ref byte HWVersion, ref byte ModuleVersion);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_GetSlaveResource(int SlaveNo, ref int DiNum, ref int DoNum, ref int AiNum, ref int AoNum, ref int LTCNum, ref int EncNum, ref int TrigNum);
        /// <summary>
        /// 连接模块总线
        /// </summary>
        /// <param name="slavenum">从站数量反馈值</param>
        /// <param name="option">是否开启别名，0-不开启 ，1-开启</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_InitEcat(ref int slavenum, int option);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_InitMointorMode(ref int slavenum);

        /// <summary>
        /// 获取从站名称
        /// </summary>
        /// <param name="slaveno">从站号，自动从1开始</param>
        /// <returns>从站名称指针</returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Mb_GetSlaveName(int slaveno);
        /// <returns>从站名称指针</returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mb_CloseEcat();
        /// <summary>
        /// 获取别名开启下从站的别名
        /// </summary>
        /// <param name="slavealiasarray">别名数组首地址</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_GetSlaveAlias(ref int slavealiasarray);

        /*=====================================================
        添加日期：20210223
        获取总线连接状态
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_GetConnectStatus(int SlaveNo, ref int ConnectStatus);

        /// <summary>
        /// 从文件加载配置参数，配置文件由调试工具生成
        /// </summary>
        /// <param name="filename">文件地址</param>
        /// <returns>设置报错信息，详见说明书</returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_LoadParamFromFile(string filename);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16_LoadParamFromFile(string filename);

        /// <summary>
        /// 获取编码器当前数值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="EncoderNo">编码器通道号，从0开始计数</param>
        /// <param name="data_out">编码器数值</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Encoder_GetEncoderData(int SlaveNo, int EncoderNo, ref int data_out);

        /// <summary>
        /// 设置编码器当前值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="EncoderNo">编码器通道号，从0开始计数</param>
        /// <param name="EncoderData">编码器设定值</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Encoder_SetCurrentData(int SlaveNo, int EncoderNo, int EncoderData);

        /// <summary>
        /// 设置编码器的计数方式和方向以及使能状态
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="EncoderNo">编码器通道号，从0开始计数</param>
        /// <param name="EncoderMode">编码器计数方式:1-1倍频，2-2倍频，4-4倍频</param>
        /// <param name="dir">编码器计数方向：0-正向，1-反向</param>
        /// <param name="enable">是否启用编码器计数通道：1-启用，0-不启用</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Encoder_Initial(int SlaveNo, int EncoderNo, int EncoderMode, int dir, int enable);

        /// <summary>
        /// 获取编码器的设置参数
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="EncoderNo">编码器通道号，从0开始计数</param>
        /// <param name="EncoderMode">编码器计数方式:1-1倍频，2-2倍频，4-4倍频</param>
        /// <param name="dir">编码器计数方向：0-正向，1-反向</param>
        /// <param name="enable">是否启用编码器计数通道：1-启用，0-不启用</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Encoder_GetParam(int SlaveNo, int EncoderNo, ref int EncoderMode, ref int dir, ref int enable);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Encoder_SetFilterCount(int SlaveNo, int FilterCount);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Encoder_GetFilterCount(int SlaveNo, ref int FilterCount);

        /// <summary>
        /// 设置触发输出口输出模式
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="Mode">输出口模式选择，0-GPIO,1-脉冲输出，2-PWM输出</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_SetOutMode(int SlaveNo, int Mode);

        /// <summary>
        /// 获取触发输出口输出模式
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="Mode">输出口模式选择，0-GPIO,1-脉冲输出，2-PWM输出</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetOutMode(int SlaveNo, ref int Mode);

        /*=====================================================
        添加日期：20210510
        设置触发输出模式为电平翻转模式还是脉冲模式 0-脉冲 1-电平翻转
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_SetTrigMode(int SlaveNo, int TriggerOutNum, int TrigMode);

        /*=====================================================
       添加日期：20210510
       获取当前触发通道的触发电平 0-负电平 1-正电平
       返回值：无
       =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetCurrentTrigLevel(int SlaveNo, int TriggerOutNum, ref int TrigLevel);

        /*=====================================================
        添加日期：20210510
        获取触发输出模式为电平翻转模式还是脉冲模式 0-脉冲 1-电平翻转
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetTrigMode(int SlaveNo, int TriggerOutNum, ref int TrigMode);

        /// <summary>
        /// 设置触发输出口输出脉宽
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        /// <param name="PulseWidth">脉冲宽度，单位10ns ，设定范围100~9999999</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_SetPulseWidth(int SlaveNo, int TriggerOutNum, int PulseWidth);

        /// <summary>
        /// 获取触发输出口设定脉宽
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        /// <param name="PulseWidth">脉冲宽度，单位10ns ，设定范围100~9999999</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetPulseWidth(int SlaveNo, int TriggerOutNum, ref int PulseWidth);


        /// <summary>
        /// 设置触发输出通道绑定比较器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        /// <param name="LineCompareMask">绑定的线性比较器号0-9，每一位对应一个线性比较器，bit0对应比较器0</param>
        /// <param name="PreCompareMask">绑定的预设定比较器号0-3，每一位对应一个预设定比较器,bit0对应预设比较器0</param>
        /// <param name="PulsePolarity">输出极性 0：不取反 1：取反</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_BandingCompare(int SlaveNo, int TriggerOutNum, uint LineCompareMask, uint PreCompareMask, uint PulsePolarity = 0);

        /// <summary>
        /// 设置触发通道与动态比较器之间的映射关系
        /// </summary>
        /// <param name="SlaveNo">从站号，从1开始计数</param>
        /// <param name="TriggerOutNum">触发通道号，从0开始计数</param>
        /// <param name="DynamicCmpMask">动态比较器序列号，一个bit代表一个通道</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_BandingDynamic(int SlaveNo, int TriggerOutNum, uint DynamicCmpMask);

        /// <summary>
        /// 获取触发输出通道绑定比较器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        /// <param name="LineCompareMask">绑定的线性比较器号0-9，每一位对应一个线性比较器，bit0对应比较器0</param>
        /// <param name="PreCompareMask">绑定的预设定比较器号0-3，每一位对应一个预设定比较器,bit0对应预设比较器0</param>
        /// <param name="PulsePolarity">输出极性 0：不取反 1：取反</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetBanding(int SlaveNo, int TriggerOutNum, ref uint LineCompareMask, ref uint PreCompareMask, ref int PulsePolarity);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetBandingDynamic(int SlaveNo, int TriggerOutNum, ref uint DynamicCmpMask);

        /// <summary>
        /// 重置触发输出口计数值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_ResetCounter(int SlaveNo, int TriggerOutNum);

        /// <summary>
        /// 获取触发输出口计数值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        /// <param name="TrigCount">触发输出计数值</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetCounter(int SlaveNo, int TriggerOutNum, ref int TrigCount);

        /// <summary>
        /// 输出指定输出口一个长脉宽脉冲
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_SetManualOutput(int SlaveNo, int TriggerOutNum);

        /// <summary>
        /// 输出指定输出口一个设定脉宽脉冲
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_SetManualPulseOutput(int SlaveNo, int TriggerOutNum);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_SetManualPulseOutputWithProtect(int SlaveNo, int TriggerOutNum);

        /// <summary>
        /// 指定输出口绑定2D比较器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        /// <param name="IsOpen2DCompare">是否开启2D比较功能，0-不开启 1-开启</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_Set2DCompare(int SlaveNo, int TriggerOutNum, uint IsOpen2DCompare);

        /// <summary>
        /// 获取指定输出口绑定2D比较器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="TriggerOutNum">触发输出口，从0开始计数</param>
        /// <param name="IsOpen2DCompare">是否开启2D比较功能，0-不开启 1-开启</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_Get2DCompareEnable(int SlaveNo, int TriggerOutNum, ref int IsOpen2DCompare);

        /// <summary>
        /// 重置预设定比较器比较坐标
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="PreCompareNo">预设定比较器号，从0开始计数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_ResetTrigData(int SlaveNo, int PreCompareNo);

        /// <summary>
        /// 设置预设定比较器绑定编码器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="EncoderNo">编码器通道号</param>
        /// <param name="PreCompareNo">预设定比较器号，从0开始计数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_BindingEncoder(int SlaveNo, int EncoderNo, int PreCompareNo);

        /// <summary>
        /// 获取预设定比较器绑定编码器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="PreCompareNo">预设定比较器号</param>
        /// <param name="EncoderNo">编码器通道号</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_GetBindingEncoder(int SlaveNo, int PreCompareNo, ref int EncoderNo);

        /// <summary>
        /// 设置预设定比较器触发时编码器运行方向
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="PreCompareNo">预设定比较器号</param>
        /// <param name="dir">方向 0-正向，1-反向，2-双向</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_SetTrigDir(int SlaveNo, int PreCompareNo, int dir);

        /// <summary>
        /// 设置预设定比较器触发点位
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="PreCompareNo">预设定比较器号</param>
        /// <param name="Pos_Array">触发点位数组</param>
        /// <param name="ArrayCount">触发点位个数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_SetTrigData(int SlaveNo, int PreCompareNo, ref int Pos_Array, int ArrayCount);

        /// <summary>
        /// 获取预设定比较器触发数据个数
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="PreCompareNo">预设定比较器号</param>
        /// <param name="ArrayCount">触发点位个数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_GetTrigDataCnt(int SlaveNo, int PreCompareNo, ref int ArrayCount);

        /// <summary>
        /// 获取预设定比较器触发数据
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="PreCompareNo">预设定比较器号</param>
        /// <param name="Pos_Array">触发点位数组首地址</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_GetTrigData(int SlaveNo, int PreCompareNo, ref int Pos_Array);

        /// <summary>
        /// 设置预设定比较器使能状态
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="PreCompareNo">预设定比较器号</param>
        /// <param name="Enable">使能状态，0-关闭，1-开启</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_SetEnable(int SlaveNo, int PreCompareNo, int Enable);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PreCmp_SetEnableAllChn(int SlaveNo, int EnableMask);


        /// <summary>
        /// 设置线性比较器绑定编码器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="EncoderNo">编码器号从0开始</param>
        /// <param name="LineCompareNo">线性比较器号，从0开始</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LineCmp_BingdingEncoder(int SlaveNo, int EncoderNo, int LineCompareNo);

        /// <summary>
        /// 获取线性比较器绑定的编码器
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LineCompareNo">线性比较器号，从0开始</param>
        /// <param name="EncoderNo">编码器号从0开始</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LineCmp_GetBingdingEncoder(int SlaveNo, int LineCompareNo, ref int EncoderNo);

        /// <summary>
        /// 设置线性比较器触发的数据
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LineCompareNo">线性比较器号，从0开始</param>
        /// <param name="StartPos">线性触发开始坐标</param>
        /// <param name="EndPos">线性触发终点坐标</param>
        /// <param name="Interval">线性触发间隔距离</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LineCmp_SetTriggerData(int SlaveNo, int LineCompareNo, int StartPos, int EndPos, int Interval);

        /// <summary>
        /// 获取线性比较器触发的数据
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LineCompareNo">线性比较器号，从0开始</param>
        /// <param name="StartPos">线性触发开始坐标</param>
        /// <param name="EndPos">线性触发终点坐标</param>
        /// <param name="Interval">线性触发间隔距离</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LineCmp_GetTriggerData(int SlaveNo, int LineCompareNo, ref int StartPos, ref int EndPos, ref int Interval);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LineCmp_SetEnable(int SlaveNo, int LineCompareNo, int Enable);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LineCmp_SetEnableAllChn(int SlaveNo, uint EnableMask);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LineCmpAndPreCmp_ResetAllChn(int SlaveNo);

        //=====分段式线性比较器绑定触发输出通道=====
        //=== SlaveNo--从站号
        //=== TriggerOutNum--触发输出通道号  0~3
        //=== SegmLineMask--分段式线性比较器序列，一个bit对应一个比较器  0~3
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_BindingSegmLine(int SlaveNo, int TriggerOutNum, uint SegmLineMask);

        //=====获取分段式线性比较器绑定的触发输出通道序列=====
        //=== SlaveNo--从站号
        //=== TriggerOutNum--触发输出通道号  0~3
        //=== SegmLineMask--获取到的分段式线性比较器序列，一个bit对应一个比较器  0~3
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetBindingSegmLine(int SlaveNo, int TriggerOutNum, ref uint SegmLineMask);

        //=====分段式线性比较器绑定编码器输入源=====
        //=== SlaveNo--从站号
        //=== EncoderNo--编码器通道号  0~3
        //=== SegmLineNo--分段式线性比较器号  0~1
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4SegmLine_BindingEncoder(int SlaveNo, int EncoderNo, int SegmLineNo);

        //=====获取分段式线性比较器绑定编码器输入源===== 
        //=== SlaveNo--从站号
        //=== SegmLineNo--分段式线性比较器号  0~1
        //=== EncoderNo--获取到的编码器通道号 0~3
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4SegmLine_GetBindingEncoder(int SlaveNo, int SegmLineNo, ref int EncoderNo);

        //=====设置分段式线性比较器的触发数据===== 
        //=== SlaveNo--从站号
        //=== SegmLineNo--分段式线性比较器号  0~1
        //=== StartPosFstAdd--线段起始坐标数组首地址
        //=== EndPosFstAdd--线段终点坐标数组首地址
        //=== IntervalFstAdd--线段间隔数组首地址
        //=== ArrayCount--数组总数
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4SegmLine_SetTrigData(int SlaveNo, int SegmLineNo, ref int StartPosFstAdd, ref int EndPosFstAdd, ref int IntervalFstAdd, int ArrayCount);

        //=====重置分段式线性比较器的触发数据===== 
        //=== SlaveNo--从站号
        //=== SegmLineNo--分段式线性比较器号  0~1
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4SegmLine_ResetTrigData(int SlaveNo, int SegmLineNo);

        //=====获取分段式线性比较器的FIFO中的剩余空间===== 
        //=== SlaveNo--从站号
        //=== SegmLineNo--分段式线性比较器号  0~1
        //=== FifoNum--FIFO中的剩余空间，一个线段消耗一个空间
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4SegmLine_GetFifoNum(int SlaveNo, int SegmLineNo, ref int FifoNum);




        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O42DCmp_SetCompareSource(int SlaveNo, uint X_Source, uint Y_Source);



        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O42DCmp_SetTrigPoint(int SlaveNo, ref int X_Array, ref int Y_Array, int PointCount);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O42DCmp_SetTrigRegionWindow(int SlaveNo, uint RegionWindow);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O42DCmp_SetEnable(int SlaveNo, int Enable);



        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O42DCmp_GetTrigFifoCnt(int SlaveNo, ref uint X_FIFO, ref uint Y_FIFO);



        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O42DCmp_GetTrigLTCPoint(int SlaveNo, ref int X_LTCPoint, ref int Y_LTCPoint);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O42DCmp_ResetTrigFifo(int SlaveNo);




        /// <summary>
        /// 设置位置锁存信号的输入滤波时间
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="FilterTime">滤波时间，当输入脉宽大于该值时，触发锁存；设定范围100-999999，单位10ns</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_SetFilterTime(int SlaveNo, int FilterTime);


        /// <summary>
        /// 重置位置锁存FIFO中已锁存的坐标
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LtcFifoNo">锁存FIFO通道号</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_ResetFifo(int SlaveNo, int LtcFifoNo);

        /// <summary>
        /// 获取位置锁存FIFO中已锁存的数据个数
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LtcFifoNo">锁存FIFO通道号</param>
        /// <param name="FifoNum">FIFO中已锁存数据个数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_GetFifoNum(int SlaveNo, int LtcFifoNo, ref int FifoNum);


        /// <summary>
        /// 获取位置锁存FIFO中锁存的数据
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LtcFifoNo">锁存FIFO通道号</param>
        /// <param name="FifoData">锁存位置，当前获取的数据会从FIFO中剔除</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_GetFifoData(int SlaveNo, int LtcFifoNo, ref int FifoData);


        /// <summary>
        /// 启动锁存SRAM功能
        /// </summary>
        /// <param name="SlaveNo">从站号，从1开始计数</param>
        /// <param name="LtcFifoNo">锁存通道号，从0开始计数</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_SramEnable(int SlaveNo);

        /// <summary>
        /// 关闭锁存SRAM功能
        /// </summary>
        /// <param name="SlaveNo">从站号，从1开始计数</param>
        /// <param name="LtcFifoNo">锁存通道号，从0开始计数</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_SramDisable(int SlaveNo);

        /// <summary>
        /// 获取锁存SRAM中的计数值
        /// </summary>
        /// <param name="SlaveNo">从站号，从1开始计数</param>
        /// <param name="LtcFifoNo">锁存通道号，从0开始计数</param>
        /// <param name="SramCnt">SRAM中的计数值</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_GetSramCnt(int SlaveNo, int LtcFifoNo, ref int SramCnt);

        /// <summary>
        /// 获取锁存SRAM中的锁存数据
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="LtcFifoNo">锁存通道号，从0开始计数</param>
        /// <param name="DataCnt">数据数量</param>
        /// <param name="SramData">SRAM中的数据值</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4LTC_GetSramData(int SlaveNo, int LtcFifoNo, int DataCnt, int[] SramData);

        /// <summary>
        /// 获取高速脉冲计数器计数值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LtcInNo">高速脉冲计数通道号，从0开始计数</param>
        /// <param name="Count">计数值</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PulseCounter_GetCounter(int SlaveNo, int LtcInNo, ref long Count);

        /// <summary>
        /// 重置高速脉冲计数器数值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LtcInNo">高速脉冲计数通道号，从0开始计数</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PulseCounter_ResetCounter(int SlaveNo, int LtcInNo);

        /// <summary>
        /// 设置高速脉冲计数器输入脉宽
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LtcInNo">高速脉冲计数通道号，从0开始计数</param>
        /// <param name="PulseWidth">输入脉宽，当输入信号脉宽大于该设定值，计数+1</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PulseCounter_SetPulseWidth(int SlaveNo, int LtcInNo, int PulseWidth);

        /// <summary>
        /// 获取高速脉冲计数器输入脉宽设定值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="LtcInNo">高速脉冲计数通道号，从0开始计数</param>
        /// <param name="PulseWidth">输入脉宽，当输入信号脉宽大于该设定值，计数+1</param>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4PulseCounter_GetPulseWidth(int SlaveNo, int LtcInNo, ref int PulseWidth);

        /// <summary>
        /// 设置动态比较器的编码器来源
        /// </summary>
        /// <param name="SlaveNo">模块从站号，自动从1开始</param>
        /// <param name="DynamicCmpNo">动态比较器比较器号，从0开始计数，0~3</param>
        /// <param name="EncoderNo">编码器号，从0开始计数</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4DynamicCmp_BindingEncoder(int SlaveNo, int DynamicCmpNo, int EncoderNo);

        /// <summary>
        /// 获取动态比较器的编码器来源
        /// </summary>
        /// <param name="SlaveNo">模块从站号，自动从1开始</param>
        /// <param name="DynamicCmpNo">动态比较器比较器号，从0开始计数，0~3</param>
        /// <param name="EncoderNo">获取的编码器号，从0开始计数</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4DynamicCmp_GetBindingEncoder(int SlaveNo, int DynamicCmpNo, ref int EncoderNo);

      

        /// <summary>
        /// 获取动态触发表内部FIFO计数值
        /// </summary>
        /// <param name="SlaveNo">从站号，自动从1开始</param>
        /// <param name="DynamicCmpNo">动态比较器号，从0开始计数</param>
        /// <param name="Cnt">内部FIFO计数</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4DynamicCmp_GetFifoCnt(int SlaveNo, int DynamicCmpNo, ref int Cnt);
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4DynamicCmp_SetFifoData(int SlaveNo, int DynamicCmpNo, int Count, ref int PositionArray);
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4DynamicCmp_ClrFifoData(int SlaveNo, int DynamicCmpNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4DynamicCmp_ClrFifoDataAllChn(int SlaveNo, int DynamicCmpNoMask);


     

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4DynamicLine_SetLineData(int SlaveNo, int DynamicCmpNo, int ArrayCount, ref int PositionArray);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Formula_SetConfigInfo(int SlaveNo, int FormulaIndex, int FifoNo, ref int FifoData, int FifoCnt);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Formula_LoadConfigration(ref int SlaveNo, int SlaveCnt, int FormulaIndex);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Formula_InitialConfigration(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4Formula_GetTableSts(int SlaveNo, ref int TableSts, ref int TableNum);

        /*
         * E1O16lib
         * 
         * encoder
         */
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Encoder_GetCurrentData(int SlaveNo, ref long EncData);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Encoder_SetCurrentData(int SlaveNo, long EncoderData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Encoder_SetCountMode(int SlaveNo, int CountMode);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Encoder_GetCountMode(int SlaveNo, ref int CountMode);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16OutPut_SetOpModeGPIO(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Output_GetOpMode(int SlaveNo, ref int OpMode);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16OutPut_SetOpModePulse(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16OutPut_SetGPIOOutput(int SlaveNo, int GPIOOutput);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16OutPut_GetGPIOOutput(int SlaveNo, ref int GPIOOutput);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16InPut_GetGPIOInput(int SlaveNo, ref int GPIOInput);

        /*=====================================================
        添加日期：20201229
        初始化E1O16编码器
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Encoder_Initial(int SlaveNo);

        /*=====================================================
        添加日期：20201229
        设置E1O16触发输出绑定锁存通道，并设置触发输出类型
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_BingLtc(int SlaveNo, int TrigNo, int LTCNo, int Type);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetBindSts(int SlaveNo, int TrigNo, ref int LTCNo, ref int Type);

        /*=====================================================
        添加日期：20201229
        设置E1O16触发输出脉冲宽度，单位10ns，范围100~10000000
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_SetPulseWidth(int SlaveNo, int TrigNo, int Width);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetPulseWidth(int SlaveNo, int TrigNo, ref int Width);

        /*=====================================================
        添加日期：20201229
        获取E1O16触发输出计数
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetCount(int SlaveNo, int TrigNo, ref int Count);

        /// <summary>
        /// 获取所有通道的触发计数值
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="pCountArray">触发计数数组首地址</param>
        /// <returns>参见手册</returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetAllChnTrigCount(int SlaveNo, ref int pCountArray);

        /// <summary>
        /// 重置指定通道的触发输出计数
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <returns>参见手册</returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_ResetCount(int SlaveNo, int TrigNo);

        /// <summary>
        /// 获取触发输出瞬间的回锁坐标
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="LTCValue">回锁位置值</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetLtcValue(int SlaveNo, int TrigNo, ref long LTCValue);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetAllChnLtcValue(int SlaveNo, ref long pLTCValue);
        /*=====================================================
        添加日期：20201229
        设置E1O16踢料触发坐标，坐标值为绝对坐标
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_SetClassifyPosition(int SlaveNo, int TrigNo, long ClassifyPosition);

        /// <summary>
        /// 获取触发的踢料FIFO中缓存的数据个数
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="TrigNo">触发通道号</param>
        /// <param name="FifoCount">FIFO计数值</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetClassifyFifoCount(int SlaveNo, int TrigNo, ref int FifoCount);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_ReadClassifyPosition(int SlaveNo, int TrigNo, ref ulong ClassifyPosition, ref int LegalDataCnt);

        /// <summary>
        /// 获取所有通道的踢料FIFO中的数据个数
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="pFifoCountArray">踢料FIFO数据个数数组首地址</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetAllChnClassifyFifoCount(int SlaveNo, ref int pFifoCountArray);

        /*=====================================================
        添加日期：20201229
        获取E1O16相机触发FIFO计数
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetCameraFifoCount(int SlaveNo, int TrigNo, ref int FifoCount);

        /// <summary>
        /// 获取所有通道的相机缓存FIFO中的数据个数
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="pFifoCountArray">相机FIFO中的数据个数数组首地址</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetAllChnCameraFifoCount(int SlaveNo, ref int pFifoCountArray);



        /*=====================================================
        添加日期：20201229
        设置E1O16相机触发相对于光电的距离，坐标值为相对距离
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_SetCameraOffset(int SlaveNo, int TrigNo, long CameraOffset);

        /*=====================================================
        添加日期：20210302
        获取E1O16相机触发相对于光电的距离，坐标值为相对距离
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetCameraOffset(int SlaveNo, int TrigNo, ref ulong CameraOffset);

        /*=====================================================
        添加日期：20210226
        设置脉冲输出的安全间距，输出后间隔内无法再输出
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_SetSafeDistance(int SlaveNo, uint SafeDistance);

        /*=====================================================
       添加日期：20210302
       获取脉冲输出的安全间距，输出后间隔内无法再输出
       返回值：无
       =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetSafeDistance(int SlaveNo, ref uint SafeDistance);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_ResetClassifyFifo(int SlaveNo, int TrigNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_SetLTCPolarity(int SlaveNo, int Polarity);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetLTCPolarity(int SlaveNo, ref int Polarity);
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_ResetAllChannelCameraFIFO(int SlaveNo);

        /*=====================================================
        添加日期：20201229
        设置E1O16产品间安全距离
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_SetSafetyClearDistance(int SlaveNo, int SafeDistance);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetSafetyClearDistance(int SlaveNo, ref int SafeDistance);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetFilterWidth(int SlaveNo, ref int FilterWidth);

        /*=====================================================
        添加日期：20201229
        设置E1O16锁存的滤波时间
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_SetFilterWidth(int SlaveNo, int FilterWidth);


        /*=====================================================
        添加日期：20210221
        重置E1O16锁存FIFO数据
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_ResetLTCFifoData(int SlaveNo, int LTCNo);

        /*=====================================================
        添加日期：20210221
        重置E1O16锁存计数器计数
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_ClearLTCCounter(int SlaveNo, int LTCNo);

        /*=====================================================
        添加日期：20210221
        设置锁存输入间隔时间
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_SetLTCGapTime(int SlaveNo, int GapTime);

        /*=====================================================
        添加日期：20210221
        获取锁存计数器计数值
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetLTCCounterData(int SlaveNo, int LTCNo, ref int Count);

        /// <summary>
        /// 获取所有通道的锁存计数器计数值
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="pCountArray">计数值数组首地址</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetAllChnLTCCounterData(int SlaveNo, ref int pCountArray);

        /*=====================================================
        添加日期：20210221
        获取锁存FIFO数据数量
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetLTCFifoCount(int SlaveNo, int LTCNo, ref int Count);

        /// <summary>
        /// 获取所有通道的锁存FIFO计数值
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="pLTCFifoCountArray">锁存FIFO计数值首地址</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetAllChnLTCFifoCount(int SlaveNo, ref int pLTCFifoCountArray);

        /*=====================================================
        添加日期：20210221
        获取锁存FIFO中的数据
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetLTCFifoData(int SlaveNo, int LTCNo, ref long FifoData);

        /*=====================================================
        添加日期：20210221
        获取锁存FIFO中的数据
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16LTC_GetLTCFifoDataSt(int SlaveNo, int LTCNo, ref long LTCData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_SetCycleTime(int SlaveNo, int CalCycleTime);
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_GetCycleTime(int SlaveNo, ref int CalCycleTime);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_SetEnable(int SlaveNo, int Enable);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_GetEnable(int SlaveNo, ref int Enable);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_GetEncVel(int SlaveNo, ref int ActualVel);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_GetCompensDis(int SlaveNo, int TirgOutNo, ref int CompensDis);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_CalibrateCompens(int SlaveNo, int TrigOutNo, float Dis_vmax, float Vmax, float Dis_vmin, float Vmin);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Compens_ResetCompens(int SlaveNo, int TrigOutNo);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_ClassifyPosBatchSet(int SlaveNo, E1O16ClassifyBatchData BatchData);


        /// <summary>
        /// 控制模块端口输出一个设定脉宽的脉冲信号，可以多个通道同步输出
        /// </summary>
        /// <param name="SlaveNo">从站号</param>
        /// <param name="TrigChnMask">触发通道掩码，一个BIT对应一个通道，对应BIT给1则通道输出一个脉冲</param>
        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_ManualTrigger(int SlaveNo, int TrigChn, int PulseWidth);


        /// <returns></returns>
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_SetOutLevel(int SlaveNo, int TrigChn, int Level);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DigitalIO_SetPortOutput(int SlaveNo, uint PortOutput);


        /*=====================================================
        添加日期：20210105
        按32bit读取数字量模块的输入信号
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DigitalIO_GetPortInput(int SlaveNo, ref uint PortInput);



        /*=====================================================
        添加日期：20210105
        初始化IO模块
        返回值：无
        =======================================================*/
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DigitalIO_Initial(int SlaveNo);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DigitalIO_GetPortOutput(int SlaveNo, ref uint PortOutput);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DigitalIO_SetChannelOutput(int SlaveNo, int ChannelNum, int ChannelOutput);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DAQAIO8_GetAnalogInput(int SlaveNo, ref short AnalogInput);
        //[DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int Mb_E3A4_GetEncData(int SlaveNo, int EncCnt, ref int EncData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DAQAIO8_SetChnAnalogOutput(int SlaveNo, int Chn, short AnalogOutput);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DAQAIO8_GetAnalogOutput(int SlaveNo, ref short AnalogOutput);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_GetEncoderData(int SlaveNo, int EncoderNo, ref int EncoderData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_GetActAnalogValue(int SlaveNo, int AnalogChn, ref double AnalogValue);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_InitAnalogCal(int SlaveNo, int Rate);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_SetColMode(int SlaveNo, int AnalogChn, int Mode);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_SetTrigColPoint(int SlaveNo, int AnalogChn, ref int Position, int Count);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_GetAnalogFifoCount(int SlaveNo, int AnalogChn, ref int Count);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_GetAnalogFifoData(int SlaveNo, int AnalogChn, int GetCnt, ref double Data);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_SetEncoderData(int SlaveNo, int EncoderNo, int EncoderData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_SetCompareSource(int SlaveNo, int CompareNo, int EncoderNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A2_SetAnalogTrigSource(int SlaveNo, int AnalogChn, int CompareMask);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_SetColMode(int SlaveNo, int Mode);


       


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetActEAValue(int SlaveNo, ref double AnalogValue, ref int EncValue);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_InitAnalogCal(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_SetSyncColParam(int SlaveNo, int Frequency, int ColNum);



        //20221117新加
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetEncoderData(int SlaveNo,int Channel,ref int EncoderData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetAnalogInput(int SlaveNo, int AnalogChn, ref short AnalogInput);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetTimeStamp(int SlaveNo, ref uint TimeStamp);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetFifoCount(int SlaveNo, ref int Count);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_ResetFifo(int SlaveNo);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_SetTrigMode(int SlaveNo, int FilterTime);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetFifoData(int SlaveNo, ref short Ain0, ref short Ain1, ref short Ain2, ref short Ain3, ref int Encoder, ref uint TimeStamp);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetSyncColFifoData(int SlaveNo, int readcnt, ref int PosData, ref double AnalogData0, ref double AnalogData1, ref double AnalogData2, ref double AnalogData3);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetSyncColEnc(int SlaveNo, int StartIndex, int EndIndex, ref int PosData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_GetSyncColData(int SlaveNo, int DataCnt, ref int AnalogCh0, ref int AnalogCh1, ref int AnalogCh2, ref int AnalogCh3, ref int Encoder, ref int TimeStamp);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_StartCol(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_SetEncoderData(int SlaveNo, int Channel,int EncoderData);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_SetTimerMode(int SlaveNo, int Frequency);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_SetThresholdMode(int SlaveNo, int AiChn, int ThresholdValue);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_C2A4_ReleaseTrg(int SlaveNo,int AiChn);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_RingCounterMode(int SlaveNo, int TrigNo, int Enable, int CntPerC, ref int RingTrigPos);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_StartRingTrigMode(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_StopRingTrigMode(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_SetRingTrigConfig(int SlaveNo, int TrigNo, int LtcChn, int ArrayCnt, ref int OffsetDistanceArray);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E1O16Trigger_GetRingTrigConfig(int SlaveNo, int TrigNo, ref int LtcChn, ref int ArrayCnt, ref int OffsetDistanceArray);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReadRegister(int slave_no, int addr);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVOutput_GetChnEnableLtcPos(int SlaveNo, int Chn, ref long EnableMask);

        //=====2D比较器绑定触发输出通道=====
        //=== SlaveNo--从站号
        //=== TriggerOutNum--触发输出通道号  0~3
        //=== IsOpen2DCompare--该通道是否绑定2D比较器  0-不绑定 1-绑定
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_SetMDCompare(int SlaveNo, int TriggerOutNum,  uint IsOpenMDCompare);


        //=====获取2D比较器绑定触发输出通道=====
        //=== SlaveNo--从站号
        //=== TriggerOutNum--触发输出通道号  0~3
        //=== IsOpen2DCompare--该通道是否绑定2D比较器  0-不绑定 1-绑定
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4TrigOut_GetMDCompareEnable(int SlaveNo, int TriggerOutNum, ref int IsOpenMDCompare);


        //=====2D比较器绑定编码器信号源=====
        //=== SlaveNo--从站号
        //=== X_Source--2D中的X轴选择的编码器信号源 0~3
        //=== Y_Source--2D中的Y轴选择的编码器信号源 0~3
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4MDCmp_SetCompareSource(int SlaveNo, int X_Source, int Y_Source, int Z_Source);

        //=====2D比较器绑定编码器信号源=====
        //=== SlaveNo--从站号
        //=== X_Array--2D中的X轴触发坐标中心点数组首地址
        //=== Y_Array--2D中的X轴触发坐标中心点数组首地址
        //=== PointCount--数组长度，范围 1-100。如需大于100点可再次执行。
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4MDCmp_SetTrigPoint(int SlaveNo, ref int X_Array, ref int Y_Array, ref int Z_Array, int PointCount);

        //=====2D比较器绑定编码器信号源=====
        //=== SlaveNo--从站号
        //=== RegionWindow--2D中心点的允许误差范围
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int  Mb_E4O4MDCmp_SetTrigRegionWindow(int SlaveNo,  uint RegionWindow);

        //=====2D比较器设置使能状态=====
        //=== SlaveNo--从站号
        //=== Enable--使能状态 0--不开启 1--开启
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4MDCmp_SetEnable(int SlaveNo, int Enable);

        //=====2D比较器获取FIFO剩余数量=====
        //=== SlaveNo--从站号
        //=== X_FIFO--FIFO剩余数量 0~512
        //=== Y_FIFO--FIFO剩余数量 0~512
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4MDCmp_GetTrigFifoCnt(int SlaveNo, ref uint FIFOCNT);

        //=====2D比较器获取触发后回锁到的坐标值=====
        //=== SlaveNo--从站号
        //=== X_LTCPoint--最近一个触发时X轴的坐标
        //=== Y_LTCPoint--最近一个触发时Y轴的坐标
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4MDCmp_GetTrigLTCPoint(int SlaveNo, ref int X_LTCPoint, ref int Y_LTCPoint, ref int Z_LTCPoint);

        //=====2D比较器重置FIFO内数据=====
        //=== SlaveNo--从站号
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_E4O4MDCmp_ResetTrigFifo(int SlaveNo);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSram(short SlaveNo, int cmd, int address, ref int data);

        #region DIv Library
        //=====读取当前编码器计数值=====
        //=== SlaveNo--从站号
        //=== Encoderdata--编码器输入计数值
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVEncoder_GetCurrentPos(int SlaveNo, ref long Encoderdata);

        //=====设置编码器计数值=====
        //===	SlaveNo : 从站号
        //===	Encoderdata : 编码器计数值
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVEncoder_SetCurrentPos(int Slaveno, long Encoderdata);

        //=====设置编码器模式和使能状态====
        //=== SlaveNo--从站号
        //=== Mode-- 0-关闭 1-开启外部编码器 2-开启虚拟编码器
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVEncoder_SetMode(int SlaveNo, int Mode, int EncVel = 0);

        //=====设置编码器滤波参数，先通过滤波次数获取一次波形的数据，然后根据阈值设置判断是否满足波形要求=====
        //===   例如 FilterCnt = 100，Threshold = 80，则先采集100次波形数据，如果合格数据大于等于80，则记一次脉冲。
        //===	SlaveNo : 从站号
        //===	FilterCnt : 滤波次数，一次10ns
        //===	FilterThreshold : 滤波阈值，小于等于滤波次数
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVEncoder_SetFilterParam(int Slaveno, int FilterCnt, int FilterThreshold);

        //=====设置编码器计数模式=====
        //===	SlaveNo : 从站号
        //===	EncoderMode : 0：双向 1：正向
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVEncoder_SetCountMode(int SlaveNo, int EncoderMode);


        //=====读取编码器锁存数据=====
        //===	SlaveNo : 从站号
        //===	LatchData : 锁存的编码器数据
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVEncoder_GetLTCData(int Slaveno, ref long LatchData);





        //=====设置指定通道为AB相输出模式=====
        //===	SlaveNo : 从站号
        //===	OPChn : 触发输出通道，通道8~15可以设置
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVOutput_SetABMode(int SlaveNo, int OPChn);

        //=====获取指定通道为分频模式=====
        //===	SlaveNo : 从站号
        //===	OPChn : 触发输出通道，通道8~15可以设置
        //===   PulseWidth : 触发脉冲宽度
        //===   Interval : 分频间隔
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVOutput_SetDivMode(int SlaveNo, int OPChn, int PulseWidth, int Interval);


        //=====获取指定通道输出计数值=====
        //===	SlaveNo : 从站号
        //===	OPChn : 触发输出通道
        //===   PulseCnt : 脉冲输出计数值
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVOutput_GetPulseCnt(int SlaveNo, int OPChn, ref int PulseCnt);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVEncoder_GetLTCCount(int SlaveNo, ref int FifoCnt);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVOutput_SetEnable(int SlaveNo, uint EnableMask);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVOutput_GetEnable(int SlaveNo, ref int EnableMask);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_DIVOutput_SetChnEnable(int SlaveNo, int Channel, int Enable);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetData(short SlaveNo, int cmd, int address, ref int ret);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Encoder_GetEncoderData(int SlaveNo, ref int EncoderData);


        //=====获取触发计数=====
        //=== SlaveNo--从站号
        //=== TrigChn--触发通道号
        //=== TrigCnt--触发计数
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Trig_GetTrigCnt(int SlaveNo, int TrigChn, ref int TrigCnt);

        //=====设置触发点位=====
        //=== SlaveNo--从站号
        //=== TrigChn--触发通道号
        //=== TrigData--最后的触发点位，自动排入队列的最后一个数据
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Trig_SetTrigData(int SlaveNo, int TrigChn, int TrigData);

        //=====获取触发FIFO计数=====
        //=== SlaveNo--从站号
        //=== TrigChn--触发通道号
        //=== TrigData--最后的触发点位，自动排入队列的最后一个数据
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Trig_GetFifoCnt(int SlaveNo, int TrigChn, ref int FifoCnt);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Trig_ResetFifo(int SlaveNo, int TrigNo);
        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Trig_GetPulseWidth(int SlaveNo, int TrigChn, ref int PulseWidth);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Trig_SetPulseWidth(int SlaveNo, int TrigNo, int PulseWidth);

        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Encoder_SetEncoderData(int SlaveNo, int EncoderData);


        [DllImport("MiniEcatLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mb_L8T16Trig_RstTrigCnt(int SlaveNo, int TrigChn);
        #endregion
    }
}

