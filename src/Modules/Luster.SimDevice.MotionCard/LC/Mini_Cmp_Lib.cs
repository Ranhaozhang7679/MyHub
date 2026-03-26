using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using APS168_W64;



namespace Luster.SimDevice.MotionCard.LC
{


    enum Cmd
    {
        Read_PL_REG = 0,
        Write_PL_REG = 1,
        Read_eeprom = 2,
        Write_eeprom = 3,
        Read_SRAM = 4,
        Write_SRAM = 5
    }

    enum Register_Address
    {
        Encoder_Ctl = 0xc,//编码器设置

        Encoder_ch0 = 0x100,//编码器通道数值
        Encoder_ch1 = 0x104,
        Encoder_ch2 = 0x108,
        Encoder_ch3 = 0x10c,

        //编码器初始0
        Encoder_Set0 = 0x10,
        Encoder_Set1 = 0x14,
        Encoder_Set2 = 0x18,
        Encoder_Set3 = 0x1c,

        MODE_OUT = 0x138,//输出口模式

        //线性比较器输入设置
        Mask_line0 = 0x20,
        Mask_line1 = 0x24,

        //线性比较器0~9初始位置
        Start_line0 = 0x28,
        Start_line1 = 0x2C,
        Start_line2 = 0x30,
        Start_line3 = 0x34,
        Start_line4 = 0x38,
        Start_line5 = 0x3C,
        Start_line6 = 0x40,
        Start_line7 = 0x44,
        Start_line8 = 0x48,
        Start_line9 = 0x4C,

        //线性比较器0~9结束位置
        End_line0 = 0x50,
        End_line1 = 0x54,
        End_line2 = 0x58,
        End_line3 = 0x5C,
        End_line4 = 0x60,
        End_line5 = 0x64,
        End_line6 = 0x68,
        End_line7 = 0x6C,
        End_line8 = 0x70,
        End_line9 = 0x74,

        //线性比较器0~9间隔位置
        Gap_line0 = 0x78,
        Gap_line1 = 0x7c,
        Gap_line2 = 0x80,
        Gap_line3 = 0x84,
        Gap_line4 = 0x88,
        Gap_line5 = 0x8C,
        Gap_line6 = 0x90,
        Gap_line7 = 0x94,
        Gap_line8 = 0x98,
        Gap_line9 = 0x9C,

        //触发输出口0~3设置
        Mask_map0 = 0xcc,
        Mask_map1 = 0xd0,

        //0~3脉冲输出宽度  单位:10ns
        Pulse_width0 = 0xBC,
        Pulse_width1 = 0xC0,
        Pulse_width2 = 0xC4,
        Pulse_width3 = 0xC8,

        //0~3脉冲输出重置
        Pulse_counter_reset = 0x200,

        //0~3脉冲输出计数
        Pulse_counter_ch0 = 0x204,
        Pulse_counter_ch1 = 0x208,
        Pulse_counter_ch2 = 0x20c,
        Pulse_counter_ch3 = 0x210,

        //预设比较器FIFO清零
        Pre_Reset = 0xA0,

        //预设比较器信号源
        Mask_Pre = 0xA8,

        //0~9比较器使能设置
        Compare_en = 0xA4,

        //预设比较器数据区域
        PreCompare0_fifo = 0x1000,
        PreCompare1_fifo = 0x2000,
        PreCompare2_fifo = 0x3000,
        PreCompare3_fifo = 0x4000,

        //预设定比较器比较方向
        PreCompare_dir = 0xAC,
        //2D比较触发X坐标
        REGION_POINT_X = 0xE0,
        //2D比较触发Y坐标
        REGION_POINT_Y = 0xE4,
        //正方形触发区域的宽度
        REGION_LENGTH = 0xE8,
        //输入编码器的选择，4选2-----0~3bit 选X轴  4~7选Y轴
        REGION_ENCODER_INPUT_MAP = 0xEC,
        //FIFO状态，低16位为占有量----X轴
        REGION_FIFO_CNT_X = 0xF0,
        //FIFO状态，低16位为占有量----Y轴
        REGION_FIFO_CNT_Y = 0xF4,
        //FIFO复位----BIT0为1时，FIFO复位
        REGION_FIFO_RESET_P = 0xF8,
        // 2D触发脉冲锁存到的坐标点---x轴
        REGION_LTC_X = 0x220,
        // 2D触发脉冲锁存到的坐标点---y轴
        REGION_LTC_Y = 0x224,

    }
    public class Mini_Cmp_Lib
    {
        static int[] Encoder_Ctl_Data = new int[16];
        static uint[] Mask_Line0_Data = new uint[16];
        static uint[] Mask_Line1_Data = new uint[16];
        static int[] Mask_Pre_Data = new int[16];
        static uint[] Mask_map0_Data = new uint[16];
        static uint[] Mask_map1_Data = new uint[16];
        static uint[] Compare_enable_status = new uint[16];
        static int[] PreCompare_dir = new int[16];
        static Object locker = new Object();
        static Object locker_endis = new Object();
        static uint[] LineCompareMask = new uint[16];
        static uint[] PreCompareMask = new uint[16];
        /// <summary>
        /// 获取编码器坐标
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="EncoderNo">第几个CMP模块</param>
        /// <param name="data_out">数据</param>
        /// <returns></returns>
        public static int Encoder_GetEncoderData(short CardNo, short IoM, int EncoderNo, ref int data_out)
        {
            int ret = 0; uint data = 0;
            ret = ecat_motion.M_Get_Slave_Analog_Input_32(IoM,(short)(2 + EncoderNo), ref data, CardNo);
            data_out = (int)data;
            return ret;

        }


        /// <summary>
        /// 设置指定通道的编码器计数值
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="EncoderNo">编码器通道号：0~3</param>
        /// <param name="Position">设置当前值</param>
        /// <returns></returns>
        public static int Encoder_SetCurrentData(short CardNo, short IoM, int EncoderNo, int Position)
        {
            int ret = 0;
            uint data = (uint)Position;
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Encoder_Set0 + EncoderNo * 4, data);
            return ret;

        }


        /// <summary>
        /// 初始化编码器通道
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="EncoderNo">编码器通道号</param>
        /// <param name="EncoderMode">编码器计数方式:1-1倍频，2-2倍频，4-4倍频</param>
        /// <param name="dir">编码器计数方向：0-正向，1-反向</param>
        /// <param name="enable">是否启用编码器计数通道：1-启用，0-不启用</param>
        /// <returns></returns>
        public static int Encoder_Initial(short CardNo, short IoM, int EncoderNo, int EncoderMode, int dir, int enable)
        {
            int ret = 0;
            //修改EncoderMode
            Encoder_Ctl_Data[IoM] = Encoder_Ctl_Data[IoM] & (int.MaxValue ^ (int)(15 * Math.Pow(256, EncoderNo))) | EncoderMode * (int)(Math.Pow(256, EncoderNo));
            //修改DIR
            Encoder_Ctl_Data[IoM] = Encoder_Ctl_Data[IoM] & (int.MaxValue ^ (int)(16 * Math.Pow(256, EncoderNo))) | dir * 16 * (int)(Math.Pow(256, EncoderNo));
            //修改Enable
            Encoder_Ctl_Data[IoM] = Encoder_Ctl_Data[IoM] & (int.MaxValue ^ (int)(128 * Math.Pow(256, EncoderNo))) | enable * 128 * (int)(Math.Pow(256, EncoderNo));
            uint data = (uint)Encoder_Ctl_Data[IoM];
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Encoder_Ctl, data);
            return ret;

        }

        /// <summary>
        /// 重置预设定比较器触发数据
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="PreCompareNo">预设定比较器号0-3</param>
        /// <returns></returns>
        public static int PreCmp_ResetTrigData(short CardNo, short IoM, int PreCompareNo)
        {
            int ret = 0;
            uint data = (uint)Math.Pow(2, PreCompareNo);
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Pre_Reset, data);
            return ret;
        }

        /// <summary>
        /// 将预设定比较器和编码器绑定
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="PreCompareNo">预设定比较器号</param>
        /// <returns></returns>
        public static int PreCmp_BindingEncoder(short CardNo, short IoM, int EncoderNo, int PreCompareNo)
        {
            int ret = 0;
            //设置编码器输入映射到比较器
            Mask_Pre_Data[IoM] = Mask_Pre_Data[IoM] & (int.MaxValue ^ (int)(15 * Math.Pow(16, PreCompareNo))) | (int)Math.Pow(2, EncoderNo) * (int)(Math.Pow(16, PreCompareNo));
            uint data = (uint)Mask_Pre_Data[IoM];
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_Pre, data);
            return ret;
        }

        /// <summary>
        /// 设定预设定比较器触发运行方向
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="PreCompareNo">预设定比较器号0-3</param>
        /// <param name="dir">预设定比较器号比较方向：0-正向，1-反向，2-双向</param>
        /// <returns></returns>
        public static int PreCmp_SetTrigDir(short CardNo, short IoM, int PreCompareNo, int dir)
        {
            int ret = 0;
            PreCompare_dir[IoM] = PreCompare_dir[IoM] & (int.MaxValue ^ (int)(3 * Math.Pow(4, PreCompareNo))) | dir * (int)(Math.Pow(4, PreCompareNo));
            uint data = (uint)PreCompare_dir[IoM];
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.PreCompare_dir, data);
            return ret;

        }

        /// <summary>
        /// 压入预设定比较器触发点位
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="PreCompareNo">预设定比较器号0-3</param>
        /// <param name="Pos_Array">位置数组，长度1-1023</param>
        /// <returns></returns>
        public static int PreCmp_SetTrigData(short CardNo, short IoM, int PreCompareNo, int[] Pos_Array)
        {
            int ret = 0;
            ret = PreCmp_ResetTrigData(CardNo, IoM, PreCompareNo);
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.PreCompare0_fifo + 0x1000 * PreCompareNo, (uint)Pos_Array.Length);
            int address = 0;
            uint pos = 0;
            for (int i = 1; i <= Pos_Array.Length; i++)
            {
                address = (int)Register_Address.PreCompare0_fifo + 0x1000 * PreCompareNo + 0x4 * i;
                pos = (uint)Pos_Array[i - 1];
                SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)address, pos);
            }
            return ret;

        }

        /// <summary>
        /// 使能预设定比较器
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="PreCompareNo">绑定的预设定比较器号0-9</param>
        /// <param name="Enable">比较器使能状态 0-去使能 1-使能</param>>
        /// <returns></returns>
        public static int PreCmp_SetEnable(short CardNo, short IoM, uint PreCompareNo, int Enable)
        {
            lock (locker_endis)
            {
                int ret = 0;
                short slaveid = IoM;
                if (Enable == 0)
                {
                    PreCompareMask[slaveid] = PreCompareMask[slaveid] & ~((uint)Math.Pow(2, PreCompareNo));//按位置0
                    Compare_enable_status[slaveid] = (Compare_enable_status[slaveid] & (uint.MaxValue ^ 0xf)) | PreCompareMask[slaveid];
                    ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);
                    System.Threading.Thread.Sleep(5);
                    ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);
                }
                else if (Enable == 1)
                {
                    PreCompareMask[slaveid] = PreCompareMask[slaveid] | (uint)Math.Pow(2, PreCompareNo);//按位置1
                    Compare_enable_status[slaveid] = Compare_enable_status[slaveid] & (uint.MaxValue ^ 0xf) | PreCompareMask[slaveid];
                    ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);
                }
                return ret;
            }
        }

        /// <summary>
        /// 线性比较器和编码器绑定
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="EncoderNo">第几个CMP模块：0-3</param>
        /// <param name="LineCompareNo">线性比较器号：0-9</param>
        /// <returns></returns>
        public static int LineCmp_BingdingEncoder(short CardNo, short IoM, int EncoderNo, int LineCompareNo)
        {
            int ret = 0;
            //设置编码器输入映射到比较器0
            if (0 <= EncoderNo && EncoderNo <= 3)
            {
                if (LineCompareNo >= 0 && LineCompareNo <= 7)
                {
                    Mask_Line0_Data[IoM] = Mask_Line0_Data[IoM] & (uint.MaxValue ^ (uint)(15 * Math.Pow(16, LineCompareNo))) | (uint)Math.Pow(2, EncoderNo) * (uint)(Math.Pow(16, LineCompareNo));
                    ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_line0, Mask_Line0_Data[IoM]);
                }
                else if (LineCompareNo >= 8 && LineCompareNo <= 9)
                {
                    Mask_Line1_Data[IoM] = Mask_Line1_Data[IoM] & (uint.MaxValue ^ (uint)(15 * Math.Pow(16, LineCompareNo - 8))) | (uint)Math.Pow(2, EncoderNo) * (uint)(Math.Pow(16, LineCompareNo - 8));
                    ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_line1, Mask_Line1_Data[IoM]);
                }
            }
            return ret;

        }

        /// <summary>
        /// 设置线性比较器比较触发参数
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="LineCompareNo">线性比较器：0~9</param>
        /// <param name="StartPos">线性触发起始点</param>
        /// <param name="EndPos">线性触发终止点</param>
        /// <param name="Interval">线性触发间隔</param>
        /// <returns></returns>
        public static int LineCmp_SetTriggerData(short CardNo, short IoM, int LineCompareNo, int StartPos, int EndPos, int Interval)
        {
            int ret = 0;
            uint sp = (uint)StartPos;
            uint ep = (uint)EndPos;
            uint iv = (uint)Interval;
            //设置线性触发起始坐标
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)(Register_Address.Start_line0 + 0x4 * LineCompareNo), sp);
            if (ret < 0)
                return ret;

            //设置线性触发结束坐标
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)(Register_Address.End_line0 + 0x4 * LineCompareNo), ep);
            if (ret < 0)
                return ret;

            //设置线性触发触发间隔
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)(Register_Address.Gap_line0 + 0x4 * LineCompareNo), iv);


            return ret;


        }

        /// <summary>
        /// 使能线性比较器
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="LineCompareNo">绑定的线性比较器号0-9</param>
        /// <param name="Enable">比较器使能状态 0-去使能 1-使能</param>>
        /// <returns></returns>
        public static int LineCmp_SetEnable(short CardNo, short IoM, uint LineCompareNo, int Enable)
        {
            lock (locker_endis)
            {
                int ret = 0;
                short slaveid = IoM;
                if (Enable == 0)
                {

                    {
                        LineCompareMask[slaveid] = LineCompareMask[slaveid] & ~((uint)Math.Pow(2, LineCompareNo));//按位置0
                        Compare_enable_status[slaveid] = Compare_enable_status[slaveid] & (uint.MaxValue ^ 0x3FF0000) | (LineCompareMask[slaveid] * 0x10000);//|PreCompareMask;
                        ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);
                        System.Threading.Thread.Sleep(5);
                        ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);

                    }
                }
                else if (Enable == 1)
                {
                    LineCompareMask[slaveid] = LineCompareMask[slaveid] | (uint)Math.Pow(2, LineCompareNo); //按位置1
                    Compare_enable_status[slaveid] = Compare_enable_status[slaveid] & (uint.MaxValue ^ 0x3FF0000) | (LineCompareMask[slaveid] * 0x10000);//|PreCompareMask;
                    SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);

                }
                return ret;
            }

        }

        /// <summary>
        /// 设置2D位置比较触发触发源
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">比较器号</param>
        /// <param name="X_Source">X轴触发源编码器号</param>
        /// <param name="Y_Source">Y轴触发源编码器号</param>
        /// <returns></returns>
        public static int _2DCmp_SetCompareSource(short CardNo, short IoM, uint X_Source, uint Y_Source)
        {
            int ret = 0;
            uint X = (uint)Math.Pow(2, X_Source); uint Y = (uint)Math.Pow(2, Y_Source);
            uint Source = X | (Y * 16);
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.REGION_ENCODER_INPUT_MAP, Source);
            return ret;
        }

        /// <summary>
        /// 设置2D位置比较触发触发点位
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">比较器号</param>
        /// <param name="X_Array">触发点位X坐标数组</param>
        /// <param name="Y_Array">触发点位Y坐标数组<</param>
        /// <param name="PointCount">点位个数</param>
        /// <returns></returns>
        public static int _2DCmp_SetTrigPoint(short CardNo, short IoM, int[] X_Array, int[] Y_Array, int PointCount)
        {
            lock (locker_endis)
            {
                int ret = 0;
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.REGION_FIFO_RESET_P, 1);
                System.Threading.Thread.Sleep(2);
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.REGION_FIFO_RESET_P, 0);
                for (int i = 0; i < PointCount; i++)
                {
                    ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.REGION_POINT_X, (uint)X_Array[i]);
                    ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.REGION_POINT_Y, (uint)Y_Array[i]);
                }
                return ret;
            }
        }

        /// <summary>
        /// 设置2D位置比较触发触发区域边长
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">比较器号</param>
        /// <param name="RegionWindow">触发区域边长</param>
        /// <returns></returns>
        public static int _2DCmp_SetTrigRegionWindow(short CardNo, short IoM, uint RegionWindow)
        {
            int ret = 0;
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.REGION_LENGTH, RegionWindow);
            return ret;
        }

        /// <summary>
        /// 获取2D位置比较触发FIFO计数
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">比较器号</param>
        /// <param name="X_FIFO">X轴FIFO数量</param>
        /// <param name="Y_FIFO">Y轴FIFO数量</param>
        /// <returns></returns>
        public static int _2DCmp_GetTrigFifoCnt(short CardNo, short IoM, ref uint X_FIFO, ref uint Y_FIFO)
        {
            lock (locker_endis)
            {
                int ret = 0;
                uint cntx = 0; uint cnty = 0;
                ret = GetData(CardNo, IoM, (int)Cmd.Read_PL_REG, (int)Register_Address.REGION_FIFO_CNT_X, ref cntx);
                X_FIFO = cntx & 0xff;
                ret = GetData(CardNo, IoM, (int)Cmd.Read_PL_REG, (int)Register_Address.REGION_FIFO_CNT_Y, ref cnty);
                Y_FIFO = cnty & 0xff;
                return ret;
            }
        }

        /// <summary>
        /// 获取2D位置比较触发当前触发锁存点位
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">比较器号</param>
        /// <param name="X_LTCPoint">触发锁存点位X轴坐标</param>
        /// <param name="Y_LTCPoint">触发锁存点位Y轴坐标</param>
        /// <returns></returns>
        public static int _2DCmp_GetTrigLTCPoint(short CardNo, short IoM, ref int X_LTCPoint, ref int Y_LTCPoint)
        {
            lock (locker_endis)
            {
                int ret = 0;
                uint cntx = 0; uint cnty = 0;
                ret = GetData(CardNo, IoM, (int)Cmd.Read_PL_REG, (int)Register_Address.REGION_LTC_X, ref cntx);
                X_LTCPoint = (int)cntx;
                ret = GetData(CardNo, IoM, (int)Cmd.Read_PL_REG, (int)Register_Address.REGION_LTC_Y, ref cnty);
                Y_LTCPoint = (int)cnty;
                return ret;
            }
        }

        public static int _2DCmp_SetEnable(short CardNo, short IoM, int Enable)
        {

            lock (locker_endis)
            {
                int ret = 0;
                short slaveid = IoM;
                if (Enable == 0)
                {

                    {
                        Compare_enable_status[slaveid] = Compare_enable_status[slaveid] & (~((uint)1 << 28));
                        ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);
                        System.Threading.Thread.Sleep(5);
                        Compare_enable_status[slaveid] = Compare_enable_status[slaveid] & (~((uint)1 << 28));
                        ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);

                    }
                }
                else if (Enable == 1)
                {
                    Compare_enable_status[slaveid] = Compare_enable_status[slaveid] | (((uint)1 << 28));
                    SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Compare_en, Compare_enable_status[slaveid]);

                }
                return ret;
            }



        }

        /// <summary>
        /// 设置触发输出口的输出模式
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="Mode">模式：0-通用DO，1-位置比较输出，2-PWM输出</param>
        /// <returns></returns>
        public static int TriggerOut_SetOutMode(short CardNo, short IoM, uint Mode)
        {
            int ret = 0;
            //设置输出口模式为编码器比较输出
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.MODE_OUT, Mode);
            return ret;
        }

        /// <summary>
        /// 设置脉冲输出口的脉冲宽度
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="TriggerOutNum">脉冲输出口</param>
        /// <param name="PulseWidth">脉冲宽度</param>
        /// <returns></returns>
        public static int TriggerOut_SetPulseWidth(short CardNo, short IoM, int TriggerOutNum, uint PulseWidth)
        {
            int ret = 0;
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)(Register_Address.Pulse_width0 + 0x4 * TriggerOutNum), PulseWidth);
            return ret;

        }

        /// <summary>
        /// 设置触发输出口的功能
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="TriggerOutNum">触发输出通道号</param>
        /// <param name="LineCompareMask">绑定的线性比较器号0-9，每一位对应一个线性比较器，bit0对应比较器0</param>
        /// <param name="PreCompareMask">绑定的预设定比较器号0-3，每一位对应一个预设定比较器，bit0对应预设比较器0</param>
        /// <param name="PulseWidth">输出脉宽单位10ns  ：100-1000000</param>
        /// <param name="PulsePolarity">输出极性 0：不取反 1：取反</param>
        /// <returns></returns>
        public static int TriggerOut_SetParam(short CardNo, short IoM, int TriggerOutNum, uint LineCompareMask, uint PreCompareMask, uint PulsePolarity = 0)
        {
            int ret = 0;

            if (TriggerOutNum == 0)
            {
                Mask_map0_Data[IoM] = Mask_map0_Data[IoM] & (UInt32.MaxValue ^ 65535) | (LineCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum))) | (0x400 * PreCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum))) | (PulsePolarity * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[IoM]);
            }
            else if (TriggerOutNum == 1)
            {
                Mask_map0_Data[IoM] = Mask_map0_Data[IoM] & (UInt32.MaxValue ^ 4294901760) | (LineCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum))) | (0x400 * PreCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum))) | (PulsePolarity * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[IoM]);
            }


            else if (TriggerOutNum == 2)
            {
                Mask_map1_Data[IoM] = Mask_map1_Data[IoM] & (UInt32.MaxValue ^ 65535) | (LineCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum - 2))) | (0x400 * PreCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum - 2))) | (PulsePolarity * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[IoM]);

            }

            else if (TriggerOutNum == 3)
            {
                Mask_map1_Data[IoM] = Mask_map1_Data[IoM] & (UInt32.MaxValue ^ 4294901760) | (LineCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum - 2))) | (0x400 * PreCompareMask * ((uint)Math.Pow(0x10000, (uint)TriggerOutNum - 2))) | (PulsePolarity * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[IoM]);
            }

            return ret;

        }


        /// <summary>
        /// 开启触发输出口2D位置比较功能
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">比较器号</param>
        /// <param name="TriggerOutNum">触发输出口号码</param>
        /// <param name="IsOpen2DCompare">是否开启2D比较触发，开启为1，关闭为0</param>
        /// <returns></returns>
        public static int TriggerOut_Set2DCompare(short CardNo, short IoM, int TriggerOutNum, uint IsOpen2DCompare)
        {
            int ret = 0;

            if (TriggerOutNum == 0)
            {
                if (IsOpen2DCompare == 1)
                {
                    Mask_map0_Data[IoM] = Mask_map0_Data[IoM] | (1 << 14);
                }
                else if (IsOpen2DCompare == 0)
                {
                    Mask_map0_Data[IoM] = Mask_map0_Data[IoM] & (~(uint)(1 << 14));
                }
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[IoM]);
            }
            else if (TriggerOutNum == 1)
            {
                if (IsOpen2DCompare == 1)
                {
                    Mask_map0_Data[IoM] = Mask_map0_Data[IoM] | (1 << 30);
                }
                else if (IsOpen2DCompare == 0)
                {
                    Mask_map0_Data[IoM] = Mask_map0_Data[IoM] & (~(uint)(1 << 30));
                }
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[IoM]);
            }

            else if (TriggerOutNum == 2)
            {
                if (IsOpen2DCompare == 1)
                {
                    Mask_map1_Data[IoM] = Mask_map1_Data[IoM] | (1 << 14);
                }
                else if (IsOpen2DCompare == 0)
                {
                    Mask_map1_Data[IoM] = Mask_map1_Data[IoM] & (~(uint)(1 << 14));
                }
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[IoM]);

            }

            else if (TriggerOutNum == 3)
            {
                if (IsOpen2DCompare == 1)
                {
                    Mask_map1_Data[IoM] = Mask_map1_Data[IoM] | (1 << 30);
                }
                else if (IsOpen2DCompare == 0)
                {
                    Mask_map1_Data[IoM] = Mask_map1_Data[IoM] & (~(uint)(1 << 30));
                }
                ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[IoM]);
            }


            return ret;
        }


        /// <summary>
        /// 重置触发输出计数
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="TriggerOutNum">触发输出通道</param>
        /// <returns></returns>
        public static int TriggerOut_ResetCounter(short CardNo, short IoM, uint TriggerOutNum)
        {
            int ret = 0;
            uint data = 0;
            data = (uint)Math.Pow(2, TriggerOutNum);
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Pulse_counter_reset, data);
            ret = SetData(CardNo, IoM, (int)Cmd.Write_PL_REG, (int)Register_Address.Pulse_counter_reset, 0);
            return ret;
        }


        /// <summary>
        /// 获取触发输出计数
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">第几个CMP模块</param>
        /// <param name="TriggerOutNum">触发通道号</param>
        /// <param name="TrigCount">触发计数值</param>
        /// <returns></returns>
        public static int TriggerOut_GetCounter(short CardNo, short IoM, int TriggerOutNum, ref uint TrigCount)
        {
            int ret = 0;
            ret = ecat_motion.M_Get_Slave_Analog_Input_32(IoM, (short)(6 + TriggerOutNum ), ref TrigCount, CardNo);
            return ret;
        }

        /// <summary>
        /// 设置手动触发
        /// </summary>
        /// <param name="CardNo">卡号</param>
        /// <param name="IoM">比较器号</param>
        /// <param name="TriggerOutNum">触发通道号</param>
        /// <returns></returns>
        public static int TriggerOut_SetManualOutput(short CardNo, short IoM, int TriggerOutNum)
        {
            short slaveid = IoM;
            int ret = 0;
            uint bit_polarity = 0;
            if (TriggerOutNum == 0)
            {
                bit_polarity = (~(Mask_map0_Data[slaveid] >> 15) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[slaveid]);
                System.Threading.Thread.Sleep(5);

                bit_polarity = (~(Mask_map0_Data[slaveid] >> 15) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[slaveid]);
            }
            else if (TriggerOutNum == 1)
            {
                bit_polarity = (~(Mask_map0_Data[slaveid] >> 31) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[slaveid]);
                System.Threading.Thread.Sleep(5);
                bit_polarity = (~(Mask_map0_Data[slaveid] >> 31) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map0_Data[slaveid] = Mask_map0_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum)));
                }
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map0, Mask_map0_Data[slaveid]);
            }


            else if (TriggerOutNum == 2)
            {
                bit_polarity = (~(Mask_map1_Data[slaveid] >> 15) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[slaveid]);
                System.Threading.Thread.Sleep(5);
                bit_polarity = (~(Mask_map1_Data[slaveid] >> 15) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[slaveid]);
            }

            else if (TriggerOutNum == 3)
            {
                bit_polarity = (~(Mask_map1_Data[slaveid] >> 31) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[slaveid]);
                System.Threading.Thread.Sleep(5);
                bit_polarity = (~(Mask_map1_Data[slaveid] >> 31) & 1) & 1;
                if (bit_polarity == 1)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] | (1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                if (bit_polarity == 0)
                {
                    Mask_map1_Data[slaveid] = Mask_map1_Data[slaveid] & ~(1 * (0x8000 * (uint)Math.Pow(0x10000, TriggerOutNum - 2)));
                }
                ret = SetData(CardNo, slaveid, (int)Cmd.Write_PL_REG, (int)Register_Address.Mask_map1, Mask_map1_Data[slaveid]);
            }

            return ret;

        }


        public static int SetData(short CardID, short IoM, int cmd, int address, uint data)
        {
            lock (locker)
            {
                int ret = 0;
                uint data1 = 0;
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data1, CardID);
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data1, CardID);
                System.Threading.Thread.Sleep(2);
                uint data2 = (uint)Math.Pow(2, 31) | (uint)(cmd * Math.Pow(2, 24)) | (uint)address;
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 3, ref data, CardID);
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 3, ref data, CardID);
                System.Threading.Thread.Sleep(2);
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data2, CardID);
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data2, CardID);
                System.Threading.Thread.Sleep(2);
                return ret;
            }
        }
        public static int GetData(short CardID, short IoM, int cmd, int address, ref uint dataout)
        {
            lock (locker)
            {
                int ret = 0;
                uint data1 = 0;
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data1, CardID);
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data1, CardID);
                System.Threading.Thread.Sleep(10);
                uint data2 = (uint)Math.Pow(2, 31) | (uint)(cmd * Math.Pow(2, 24)) | (uint)address;
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data2, CardID);
                ret = ecat_motion.M_Set_Slave_Analog_Output_32(IoM, 2, ref data2, CardID);
                System.Threading.Thread.Sleep(10);
                ret = ecat_motion.M_Get_Slave_Analog_Input_32(IoM, 10, ref dataout, CardID);
                ret = ecat_motion.M_Get_Slave_Analog_Input_32(IoM, 10, ref dataout, CardID);
                return ret;
            }
        }

    }


}
