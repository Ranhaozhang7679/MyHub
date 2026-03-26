using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.MotionCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Luster.SimDevice.MotionCard.LC.ecat_motion;

namespace Luster.SimDevice.MotionCard.LC
{
    /// <summary>
    /// 凌臣飞拍飞拍E1O16
    /// </summary>
    public class LCMiniBus : MotionCardBase, IFlyingPhoto, IMotionCard
    {
        public override string Brand => "凌臣飞拍E1O16";

        public int SlaveNum { get; set; }

        public int SlaveNo { get; set; }

        public bool CheckMotionDone(int precision, int axisNo = 0, double targetPulse = 0)
        {
            return true;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 设置E1O16模块的当前编码器计数值
        /// </summary>
        /// <param name="slaveNo"></param>
        public void ClearEncoderData(int encoderNo)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16模块的当前编码器计数值
                int ret = MiniEcatLib.Mb_E1O16Encoder_SetCurrentData(SlaveNo, 0);//设置当前编码器值为0
                if (ret != 0)
                {
                    err = $"设置E1O16模块的当前编码器计数值失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 连接MiniBus总线
        /// </summary>
        public void Connect(int slaveNo)
        {
            SlaveNo = slaveNo;
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                int slaveNum = 0;
                //连接MiniBus总线
                MiniEcatLib.Mb_InitEcat(ref slaveNum, 0); //option:0：关闭别名，1：启用别名
                if (slaveNum > 0)
                {
                    //重置E1O16模块参数
                    ret = MiniEcatLib.Mb_E1O16Encoder_Initial(SlaveNo);
                    if (ret != 0)
                    {
                        err = $"重置E1O16模块参数失败，错误码：{ret}";
                        return false;
                    }

                    //设置E1O16模块的当前编码器计数值
                    //ret = MiniEcatLib.Mb_E1O16Encoder_SetCurrentData(slaveNo, 0);//设置当前编码器值为0
                    //if (ret != 0)
                    //{
                    //    err = $"设置E1O16模块的当前编码器计数值失败，错误码：{ret}";
                    //    return false;
                    //}
                }
                else
                {
                    err = "未检测到从站连接";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 获取从站连接状态 0-断线 1-在线。注意：模块上电时网线插拔可以自动重连，模块断电需要重新执行连接函数
        /// </summary>
        public bool GetConnectStatus()
        {
            int ConnectStatus = 0;

            MiniEcatLib.Mb_GetConnectStatus(SlaveNo, ref ConnectStatus);
            if (ConnectStatus == 1) return true;

            return false;
        }

        /// <summary>
        /// 从文件加载E1O16参数
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadParamFromFile(string filePath)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16模块的当前编码器计数值
                ret = MiniEcatLib.Mb_E1O16_LoadParamFromFile(filePath);
                if (ret != 0)
                {
                    err = $"从文件加载E1O16参数失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 重置E1O16模块
        /// </summary>
        public void EncoderInit(int encoderNo = 0)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //重置E1O16模块
                ret = MiniEcatLib.Mb_E1O16Encoder_Initial(SlaveNo);
                if (ret != 0)
                {
                    err = $"重置E1O16模块失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 获取E1O16模块的编码器计数值
        /// </summary>
        /// <param name="data"></param>
        public void GetEncoderCurData(int encoderNo, out long data)
        {
            int ret = 0;
            long curData = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //获取E1O16模块的编码器计数值
                ret = MiniEcatLib.Mb_E1O16Encoder_GetCurrentData(SlaveNo, ref curData);
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
        /// 设置E1O16模块的当前编码器计数值
        /// </summary>
        /// <param name="data"></param>
        public void SetEncoderCurData(long data)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16模块的当前编码器计数值
                ret = MiniEcatLib.Mb_E1O16Encoder_SetCurrentData(SlaveNo, data);
                if (ret != 0)
                {
                    err = $"设置E1O16模块的当前编码器计数值失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 设置E1O16模块的当前编码器计数方式
        /// </summary>
        /// <param name="data">0-双向；1-正向(反向运动不计数)</param>
        public void SetEncoderCurData(int dir)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16模块的当前编码器计数方式
                ret = MiniEcatLib.Mb_E1O16Encoder_SetCountMode(SlaveNo, dir);
                if (ret != 0)
                {
                    err = $"设置E1O16模块的当前编码器计数方式失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        #region 触发相关函数

        /// <summary>
        /// 设置E1O16触发通道绑定的锁存输入与触发类型（0：相机 1：踢料）
        /// </summary>
        /// <param name="trigNo">触发输出通道，从0开始计数</param>
        /// <param name="ltcNo">锁存通道，从0开始计数,-1则不绑定任何锁存</param>
        /// <param name="type">触发通道类型（0：相机 1：踢料）</param>
        ///  <param name="width">触发输出的脉冲宽度（单位 10ns，100-999999）1s=10^9ns纳秒</param>
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

        /// <summary>
        /// 设置E1O16触发通道输出脉宽（单位 10ns，100-999999）
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="windth"></param>
        public void SetPulseWidth(int trigNo, int windth = 100000)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16触发通道输出脉宽
                ret = MiniEcatLib.Mb_E1O16Trigger_SetPulseWidth(SlaveNo, trigNo, windth);
                if (ret != 0)
                {
                    err = $"设置E1O16触发通道输出脉宽失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 获取E1O16触发输出计数
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="count"></param>
        public void GetTriggerCount(int trigNo, ref int count)
        {
            int ret = 0;
            int curCount = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //获取E1O16触发输出计数
                ret = MiniEcatLib.Mb_E1O16Trigger_GetCount(SlaveNo, trigNo, ref curCount);
                if (ret != 0)
                {
                    err = $"设置E1O16触发通道输出脉宽失败，错误码：{ret}";
                    return false;
                }

                return true;
            });

            count = curCount;
        }

        /// <summary>
        /// 获取E1O16触发输出锁存坐标
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="value"></param>E1O16ClassifyBatchData
        public void GetTriggerEncoderValue(int trigNo, ref long value)
        {
            int ret = 0;
            long curValue = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //获取E1O16触发输出锁存坐标
                ret = MiniEcatLib.Mb_E1O16Trigger_GetLtcValue(SlaveNo, trigNo, ref curValue);
                if (ret != 0)
                {
                    err = $"获取E1O16触发输出锁存坐标失败，错误码：{ret}";
                    return false;
                }

                return true;
            });

            value = curValue;
        }

        /// <summary>
        /// 设置E1O16踢料触发坐标，坐标值为绝对坐标
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="position"></param>
        public void SetTriggerPos(int trigNo, long position)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16踢料触发坐标，坐标值为绝对坐标
                ret = MiniEcatLib.Mb_E1O16Trigger_SetClassifyPosition(SlaveNo, trigNo, position);
                if (ret != 0)
                {
                    err = $"设置E1O16踢料触发坐标失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 重置E1O16踢料触发队列数据
        /// </summary>
        /// <param name="trigNo"></param>
        public void ResetTriggerClassifyPos(int trigNo)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //重置E1O16踢料触发队列数据
                ret = MiniEcatLib.Mb_E1O16Trigger_ResetClassifyFifo(SlaveNo, trigNo);
                if (ret != 0)
                {
                    err = $"重置E1O16踢料触发队列数据失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 获取E1O16踢料队列中的数据个数
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="fifoCount"></param>
        public void GetTriggerClassfiyFifoValue(int trigNo, ref int fifoCount)
        {
            int ret = 0;
            int curFifoCount = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //获取E1O16踢料队列中的数据个数
                ret = MiniEcatLib.Mb_E1O16Trigger_GetClassifyFifoCount(SlaveNo, trigNo, ref curFifoCount);
                if (ret != 0)
                {
                    err = $"获取E1O16踢料队列中的数据个数失败，错误码：{ret}";
                    return false;
                }

                return true;
            });

            fifoCount = curFifoCount;
        }

        /// <summary>
        /// 设置E1O16相机触发相对于光电的距离
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="offset"></param>
        public void SetTriggerCameraOffset(int trigNo, long offset)
        {
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //设置E1O16相机触发相对于光电的距离，坐标值为相对距离
                ret = MiniEcatLib.Mb_E1O16Trigger_SetCameraOffset(SlaveNo, trigNo, offset);
                if (ret != 0)
                {
                    err = $"设置E1O16相机触发相对于光电的距离失败，错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 获取E1O16相机触发FIFO队列中的数据个数
        /// </summary>
        /// <param name="trigNo"></param>
        /// <param name="fifoCount"></param>
        public void GetTriggerCameraFifoValue(int trigNo, ref int fifoCount)
        {
            int ret = 0;
            int curFifoCount = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //获取E1O16相机触发FIFO队列中的数据个数
                ret = MiniEcatLib.Mb_E1O16Trigger_GetCameraFifoCount(SlaveNo, trigNo, ref curFifoCount);
                if (ret != 0)
                {
                    err = $"获取E1O16相机触发FIFO队列中的数据个数失败，错误码：{ret}";
                    return false;
                }

                return true;
            });

            fifoCount = curFifoCount;
        }


        public void GetTriggerFifoCount(int trigNo, int Type, ref int count)
        {
            int curCount = 0;

            if (Type == 0)
            {
                GetTriggerCameraFifoValue(trigNo, ref curCount);
            }
            else
            {
                GetTriggerClassfiyFifoValue(trigNo, ref curCount);
            }

            count = curCount;
        }


        #endregion




        public double GetAnalogIn(int index)
        {
            throw new NotImplementedException();
        }

        public double GetAnalogOut(int index)
        {
            throw new NotImplementedException();
        }

        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            throw new NotImplementedException();
        }

        public double GetCurrentPos(int axisNo, double perPulse)
        {
            throw new NotImplementedException();
        }

        public bool GetDigitalIn(int index)
        {
            throw new NotImplementedException();
        }

        public bool GetDigitalOut(int index)
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

        #region 设备通用方法
        public override void InitApi()
        {
            if (HasInit())
            {
                OnLog(LogType.Info, $"{Brand}_{ID} has initialize!");
                return;
            }

            var pcie = Adapter as PCIe;
            if (pcie == null)
            {
                ErrMsg = "未配置插槽";
                OnLog(LogType.Error, $"{Brand}_{ID} has connect fail!");
                return;
            }

            //从站数量
            int slaveNum = 0;

            //从站号
            int slaveNo = 1;

            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //连接MiniBus总线
                MiniEcatLib.Mb_InitEcat(ref slaveNum, 0); //option:0：关闭别名，1：启用别名
                if (slaveNum > 0)
                {
                    //重置E1O16模块参数
                    ret = MiniEcatLib.Mb_E1O16Encoder_Initial(slaveNo);
                    if (ret != 0)
                    {
                        err = $"重置E1O16模块参数失败，错误码：{ret}";
                        return false;
                    }

                    //设置E1O16模块的当前编码器计数值
                    ret = MiniEcatLib.Mb_E1O16Encoder_SetCurrentData(slaveNo, 0);//设置当前编码器值为0
                    if (ret != 0)
                    {
                        err = $"设置E1O16模块的当前编码器计数值失败，错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    err = "未检测到从站连接";
                    return false;
                }

                return true;
            });
            OnStatusChanged(DeviceStatus.Online);
        }

        public void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML)
        {
            throw new NotImplementedException();
        }

        public void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            throw new NotImplementedException();
        }

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            throw new NotImplementedException();
        }

        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
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

        public void ResetState(int axisNo)
        {
            throw new NotImplementedException();
        }

        public void ScanAnglog(out ushort anglogIn, out ushort anglogOut)
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

        public void SDORead(short slave, short index, short subindex, short data_size, out int value, short count)
        {
            throw new NotImplementedException();
        }

        public void SDOWrite(short slave, short index, short subindex, int data, short data_size)
        {
            throw new NotImplementedException();
        }

        public void ServOn(int axisNo, bool isOn)
        {
            throw new NotImplementedException();
        }

        public void SetAnalogOut(int index, double analogVal)
        {
            throw new NotImplementedException();
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            throw new NotImplementedException();
        }
        #endregion


        public void Stop(int axisNo, bool isAll = false)
        {
            throw new NotImplementedException();
        }

        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {
            throw new NotImplementedException();
        }

        public void SetTriggerData(int preCompareNo, int[] posArray)
        {
            throw new NotImplementedException();
        }

        public void SetPreCompareEnable(int preCompareNo, bool onoff)
        {

        }

        public void ResetTriggerOutCount(int trigNo)
        {

        }

        public void SetPreCmpParam(int encoderNo, int preCmpNo, int trigNo)
        {
            
        }
    }
}
