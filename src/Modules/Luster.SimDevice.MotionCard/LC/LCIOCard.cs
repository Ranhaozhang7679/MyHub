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
using static Luster.SimDevice.MotionCard.LC.Ecat_io;

namespace Luster.SimDevice.MotionCard.LC
{
    public class LCIOCard : MotionCardBase, IMotionCard
    {
        public override string Brand => "凌臣IO卡";


        /// <summary>
        /// IO卡的插槽
        /// </summary>
        private short cardNo = 0;

        /// <summary>
        /// 扫描到的从站资源
        /// </summary>
        Ecat_io.EC_RES Ec_res = new Ecat_io.EC_RES();
        /// <summary>
        /// 连接之后在线的从站
        /// </summary>
        Ecat_io.EC_RES Ec_res_online = new Ecat_io.EC_RES();

        /// <summary>
        /// 所有从站的信息
        /// </summary>
        List<Ecat_io.EC_SLAVE_INFO> Ec_slave_infos = new List<EC_SLAVE_INFO>();

        /// <summary>
        /// 根据输入的资源索引，找到对应的从站号和从站索引
        /// </summary>
        /// <param name="index">输入的资源号</param>
        /// <param name="func">委托：获取的输出资源数量</param>
        /// <param name="slaveNo">从站号</param>
        /// <param name="channel">从站内部资源号</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void GetResNum(int index, Func<EC_SLAVE_INFO, int> func, out short slaveNo, out short channel)
        {
            slaveNo = 0;
            channel = 0;
            int tempCount = 0;
            bool find = false;
            for (int i = 0; i < Ec_slave_infos.Count; i++)
            {
                tempCount += func.Invoke(Ec_slave_infos[i]);
                if (index <= tempCount)
                {
                    slaveNo = (short)(i + 1);
                    channel = (short)index;
                    find = true;
                    break;
                }
            }
            if (!find)
            {
                throw new ArgumentOutOfRangeException($"失败。原因：{index}超出最大范围{tempCount}");
            }
        }

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

            // 设置编号
            cardNo = (short)pcie.Slot;
            short[] cardids = new short[8];
            int[] cardtypes = new int[8];
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                Ecat_io.EC_Close(cardNo);
                Thread.Sleep(100);

                // 打开凌臣IO卡。操作凌臣IO卡的第一步必须是打开卡，且只能打开一次。
                ret = Ecat_io.EC_Open(cardNo, 0);
                if (ret != 0)
                {
                    err = $"确认板卡是否安装完成以及重复打开。错误码:{ret}";
                    return false;
                }

                //初始化运控卡
                string ENIPath = "C:\\Program Files (x86)\\LCT\\Pcie-EC8001\\ENI\\eni.xml";//具体路径测试时再确认
                ret = Ecat_io.EC_LoadEni(ENIPath, cardNo);
                if (ret != 0)
                {
                    err = $"检查加载的 ENI 路径，和供应商联系索要最新 ENI 并且替换。错误码:{ret}";
                    return false;
                }

                // 与从站建立通信 
                ret = Ecat_io.EC_ConnectECAT(1, cardNo);
                if (ret != 0)
                {
                    err = $"连接总线失败。错误码:{ret}";
                    return false;
                }

                //获取 Ethercat 网络中的从站资源
                ret = Ecat_io.EC_GetSlaveResource(out Ec_res, out Ec_res_online, cardNo);
                if (ret != 0)
                {
                    err = $"获取总线上的从站资源失败。错误码:{ret}";
                    return false;
                }
                for (short i = 0; i < Ec_res_online.SlaveNum; i++)
                {
                    Ecat_io.EC_SLAVE_INFO ec_slave_info;
                    ret = Ecat_io.EC_GetSlaveInfo(out ec_slave_info, (short)(i + 1), cardNo);
                    if (ret != 0)
                    {
                        err = $"获取从站{i}的资源失败。错误码:{ret}";
                        return false;
                    }
                    Ec_slave_infos.Add(ec_slave_info);
                }

                OnStatusChanged(DeviceStatus.Online);

                return true;
            });
        }

        #region 获取资源
        public void ScanAnglog(out ushort anglogIn, out ushort anglogOut)
        {
            CheckInit();

            anglogIn = 0;
            anglogOut = 0;
            for (int i = 0; i < Ec_slave_infos.Count; i++)
            {
                anglogIn += (ushort)Ec_slave_infos[i].AiNum;
                anglogOut += (ushort)Ec_slave_infos[i].AoNum;
            }
        }

        public void ScanDigitalIO(out uint digitalIn, out ushort digitalOut)
        {
            CheckInit();

            digitalIn = 0;
            digitalOut = 0;
            for (int i = 0; i < Ec_slave_infos.Count; i++)
            {
                digitalIn += (uint)Ec_slave_infos[i].DiNum;
                digitalOut += (ushort)Ec_slave_infos[i].DoNum;
            }
        }
        #endregion

        #region 模拟量API
        public double GetAnalogIn(int index)
        {
            short value;
            CheckInit();

            short slaveNo = 0;
            short channel = 0;
            GetResNum(index, info => { return info.AiNum; }, out slaveNo, out channel);

            int ret = Ecat_io.EC_Get_Analog_Chn_In(slaveNo, channel, out value, 1, (short)cardNo);
            return value;
        }

        public double GetAnalogOut(int index)
        {
            short value;
            CheckInit();

            short slaveNo = 0;
            short channel = 0;
            GetResNum(index, info => { return info.DoNum; }, out slaveNo, out channel);

            int ret = Ecat_io.EC_Get_Analog_Chn_Out(slaveNo, channel, out value, 1, (short)cardNo);
            return value;
        }

        public void SetAnalogOut(int index, double analogVal)
        {
            short value = (short)analogVal;
            CheckInit();

            short slaveNo = 0;
            short channel = 0;
            GetResNum(index, info => { return info.DoNum; }, out slaveNo, out channel);

            int ret = Ecat_io.EC_Set_Analog_Chn_Out(slaveNo, channel, out value, 1, (short)cardNo);
        }
        #endregion


        #region 数字量API
        public bool GetDigitalIn(int index)
        {
            short value = 0;
            CheckInit();

            short slaveNo = 0;
            short channel = 0;
            GetResNum(index, info => { return info.DiNum; }, out slaveNo, out channel);

            int ret = Ecat_io.EC_Get_Digital_Chn_In(slaveNo, channel, ref value, (short)cardNo);
            return value == 1;
        }

        public bool GetDigitalOut(int index)
        {
            short value = 0;
            CheckInit();

            short slaveNo = 0;
            short channel = 0;
            GetResNum(index, info => { return info.DoNum; }, out slaveNo, out channel);

            int ret = Ecat_io.EC_Get_Digital_Chn_Out(slaveNo, channel, ref value, (short)cardNo);
            return value == 1;
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            short value = digitalOut ? (short)1 : (short)0;
            CheckInit();

            short slaveNo = 0;
            short channel = 0;
            GetResNum(index, info => { return info.DoNum; }, out slaveNo, out channel);

            int ret = Ecat_io.EC_Set_Digital_Chn_Out(slaveNo, channel, value, (short)cardNo);
        }
        #endregion

        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            if (!HasInit()) return;

            // 断开连接卡
            try
            {
                Ecat_io.EC_DisconnectECAT(cardNo);
            }
            catch (Exception)
            {
            }

            // 控制卡关闭
            Ecat_io.EC_Close(cardNo);

            Status = DeviceStatus.Stop;
            OnLog(LogType.Debug, "凌臣IO对象释放");
            Status = DeviceStatus.Offline;
        }

        #region IO卡没有这些函数

        public void Stop(int axisNo, bool isAll = false)
        {
            throw new NotImplementedException();
        }

        public void ServOn(int axisNo, bool isOn)
        {
            throw new NotImplementedException();
        }

        public void ScanAxis(out uint axisNum)
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

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            throw new NotImplementedException();
        }

        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            throw new NotImplementedException();
        }

        public void ResetState(int axisNo)
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

        public bool CheckMotionDone(int precision, int axisNo = 0, double targetPulse = 0)
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
