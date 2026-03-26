using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.MotionCards;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.MotionCard.LC
{
    public class LC_E4O4 : MotionCardBase, IFlyingPhoto,IMotionCard
    {
        public override string Brand => "凌臣飞拍E4O4";
        public int SlaveNum { get; set; }

        public int SlaveNo { get; set; }

        #region 飞拍模块

        /// <summary>
        /// 编码器当前值清零
        /// </summary>
        /// <param name="encoderNo"></param>
        public void ClearEncoderData(int encoderNo)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置编码器当前值
                int ret = MiniEcatLib.Mb_E4O4Encoder_SetCurrentData(SlaveNo, encoderNo, 0);
                if (ret != 0)
                {
                    err = $"设置E4O4模块的编码器通道号{encoderNo}值失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 连接飞拍模块
        /// </summary>
        /// <param name="slaveNo"></param>
        public void Connect(int slaveNo)
        {
            SlaveNo = slaveNo;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                int slaveNum = 0;
                //连接MiniBus总线
                MiniEcatLib.Mb_InitEcat(ref slaveNum, 0); //option:0：关闭别名，1：启用别名
                if (slaveNum > 0)
                {
                    return true;
                }
                else
                {
                    err = "未检测到从站连接";
                    return false;
                }
            });
        }

        /// <summary>
        /// 初始化编码器
        /// </summary>
        /// <param name="encoderNo"></param>
        public void EncoderInit(int encoderNo = 0)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //初始化编码器-模块内部默认可忽略
                //ret = MiniEcatLib.Mb_E4O4Encoder_Initial(SlaveNo, encoderNo, 4, 0, 1);
                //if (ret != 0)
                //{
                //    err = $"设置E4O4模块的初始化编码器通道号{encoderNo}失败，错误码：{ret}";
                //    return false;
                //}

                //设置编码器当前值
                ret = MiniEcatLib.Mb_E4O4Encoder_SetCurrentData(SlaveNo, encoderNo, 0);
                if (ret != 0)
                {
                    err = $"设置E4O4模块的编码器通道号{encoderNo}值失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 获取编码器当前数值
        /// </summary>
        /// <param name="encoderNo">编码器通道号，从0开始计数<</param>
        /// <param name="data"></param>
        public void GetEncoderCurData(int encoderNo, out long data)
        {
            int ret = 0;
            int curData = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //获取编码器当前数值
                ret = MiniEcatLib.Mb_E4O4Encoder_GetEncoderData(SlaveNo, encoderNo, ref curData);
                if (ret != 0)
                {
                    err = $"获取E1O16模块的编码器计数值失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
            data = curData;
        }

        /// <summary>
        /// 获取触发输出口计数值
        /// </summary>
        /// <param name="trigNo">触发输出口，从0开始计数</param>
        /// <param name="count">触发输出计数值</param>
        public void GetTriggerCount(int trigNo, ref int count)
        {
            int curData = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                int ret = MiniEcatLib.Mb_E4O4TrigOut_GetCounter(SlaveNo, trigNo, ref curData);
                if (ret != 0)
                {
                    err = $"获取触发输出口计数值失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
            count = curData;
        }

        /// <summary>
        /// 获取预设定比较器触发数据个数
        /// </summary>
        /// <param name="trigNo">预设定比较器号</param>
        /// <param name="Type">不需要</param>
        /// <param name="count">触发点位个数</param>
        public void GetTriggerFifoCount(int trigNo, int Type, ref int count)
        {
            int curData = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                //获取预设定比较器触发数据个数
                int ret = MiniEcatLib.Mb_E4O4PreCmp_GetTrigDataCnt(SlaveNo, trigNo, ref curData);
                if (ret != 0)
                {
                    err = $"获取预设定比较器触发数据个数失败，错误码：{ret}";
                    return false;
                }
                return true;
            });

            count = curData;
        }

        /// <summary>
        /// 设置触发输出通道绑定比较器
        /// </summary>
        /// <param name="encoderNo">编码器通道</param>
        /// <param name="preCmpNo">预设定比较器号</param>
        /// <param name="trigNo">触发输出口</param>
        public void SetPreCmpParam(int encoderNo, int preCmpNo, int trigNo)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                uint preCompareMask = (uint)Math.Pow(2, preCmpNo);
                //设置触发输出通道绑定比较器
                int ret = MiniEcatLib.Mb_E4O4TrigOut_BandingCompare(SlaveNo, trigNo, 0, preCompareMask, 0);
                if (ret != 0)
                {
                    err = $"设置触发输出通道绑定比较器失败，错误码：{ret}";
                    return false;
                }

                ret = MiniEcatLib.Mb_E4O4PreCmp_ResetTrigData(SlaveNo, preCmpNo);
                if (ret != 0)
                {
                    err = $"重置预设定比较器比较坐标失败，错误码：{ret}";
                    return false;
                }

                ret = MiniEcatLib.Mb_E4O4PreCmp_BindingEncoder(SlaveNo, encoderNo, preCmpNo);
                if (ret != 0)
                {
                    err = $"设置预设定比较器绑定编码器失败，错误码：{ret}";
                    return false;
                }

                ret = MiniEcatLib.Mb_E4O4PreCmp_SetTrigDir(SlaveNo, preCmpNo, 2);
                if (ret != 0)
                {
                    err = $"设置预设定比较器触发时编码器运行方向失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 设置预设定比较器使能状态
        /// </summary>
        /// <param name="preCompareNo">预设定比较器号</param>
        /// <param name="onoff">true-打开，false-关闭</param>
        public void SetPreCompareEnable(int preCompareNo, bool onoff)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                //trigNo-预设定比较器号
                int ret = MiniEcatLib.Mb_E4O4PreCmp_SetEnable(SlaveNo, preCompareNo, onoff ? 1 : 0);
                if (ret != 0)
                {
                    err = $"设置预设定比较器状态失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 重置触发输出口计数值
        /// </summary>
        /// <param name="trigNo">触发输出口</param>
        public void ResetTriggerOutCount(int trigNo)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                //重置触发输出口计数值
                int ret = MiniEcatLib.Mb_E4O4TrigOut_ResetCounter(SlaveNo, trigNo);
                if (ret != 0)
                {
                    err = $"重置触发输出口计数值失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 设置触发输出口输出脉宽
        /// </summary>
        /// <param name="trigNo">脉冲输出口 0~3</param>
        /// <param name="width">脉冲宽度，单位10ns ，设定范围100~9999999</param>
        public void SetPulseWidth(int trigNo, int width)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                int ret = MiniEcatLib.Mb_E4O4TrigOut_SetPulseWidth(SlaveNo, trigNo, width);
                if (ret != 0)
                {
                    err = $"设置触发输出口输出脉宽失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 设置预设定比较器触发点位
        /// </summary>
        /// <param name="preCompareNo">预设定比较器号</param>
        /// <param name="posArray">触发点位数组</param>
        public void SetTriggerData(int preCompareNo, int[] posArray)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                //设置预设定比较器触发点位
                int ret = MiniEcatLib.Mb_E4O4PreCmp_SetTrigData(SlaveNo, preCompareNo, ref posArray[0], posArray.Count());
                if (ret != 0)
                {
                    err = $"获取预设定比较器触发数据个数失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void SetTriggerCameraOffset(int trigNo, long offset)
        {

        }

        public void SetTriggerParm(int trigNo, int ltcNo, int type)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16触发通道绑定的锁存输入与触发类型
                ret = MiniEcatLib.Mb_E1O16Trigger_BingLtc(SlaveNo, trigNo, ltcNo, type);
                if (ret != 0)
                {
                    err = $"设置E1O16触发通道绑定的锁存输入与触发类型失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        public void SetTriggerPos(int trigNo, long position)
        {

        }

        #endregion

        #region 轴卡        

        public bool GetDigitalIn(int index)
        {
            throw new NotImplementedException();
        }

        public bool GetDigitalOut(int index)
        {
            throw new NotImplementedException();
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            throw new NotImplementedException();
        }

        public double GetAnalogIn(int index)
        {
            throw new NotImplementedException();
        }

        public double GetAnalogOut(int index)
        {
            throw new NotImplementedException();
        }

        public void SetAnalogOut(int index, double analogVal)
        {
            throw new NotImplementedException();
        }

        public void ScanAxis(out uint axisNum)
        {
            throw new NotImplementedException();
        }

        public void ScanDigitalIO(out uint digitalIn, out ushort digitalOut)
        {
            throw new NotImplementedException();
        }

        public void ScanAnglog(out ushort anglogIn, out ushort anglogOut)
        {
            throw new NotImplementedException();
        }

        public bool CheckMotionDone(int precision, int axisNo = 0, double targetPulse = 0)
        {
            throw new NotImplementedException();
        }

        public double GetCurrentPos(int axisNo, double perPulse)
        {
            throw new NotImplementedException();
        }

        public void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus, double homeAcc, double Offset, AxisPML axisPML)
        {
            throw new NotImplementedException();
        }

        public void HomeCancel(int axisNo)
        {
            throw new NotImplementedException();
        }

        public void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML)
        {
            throw new NotImplementedException();
        }

        public void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            throw new NotImplementedException();
        }

        public void Stop(int axisNo, bool isAll = false)
        {
            throw new NotImplementedException();
        }

        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            throw new NotImplementedException();
        }

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            throw new NotImplementedException();
        }

        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            throw new NotImplementedException();
        }

        public void ServOn(int axisNo, bool isOn)
        {
            throw new NotImplementedException();
        }

        public void ResetState(int axisNo)
        {
            throw new NotImplementedException();
        }

        public void SDORead(short slave, short index, short subindex, short data_size, out int value, short count)
        {
            throw new NotImplementedException();
        }

        public void SDOWrite(short slave, short index, short subindex, int data, short data_size)
        {
            throw new NotImplementedException();
        }

        public void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count)
        {
            throw new NotImplementedException();
        }

        public void PDOWrite(short axis, short index, short subindex, int data, short data_size)
        {
            throw new NotImplementedException();
        }

        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
