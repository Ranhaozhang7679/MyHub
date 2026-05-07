#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LSMotionCard
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.MotionCard.LS
* 文 件 名:       LSMotionCard.cs
* 创建时间:       2022/4/28 21:06:55
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a543f16b-1b2e-4895-949b-0e6976383bc1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/28 21:06:55
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.MotionCard.Common;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.SimDevice.MotionCard.LC
{
    public class LCMotionCard : MotionCardBase, IMotionCard
    {
        private short GetAxisNum(int axisNo)
        {
            return (short)(axisNo);
        }
        public override string Brand => "凌臣轴卡";


        /// <summary>
        /// 轴卡的插槽
        /// </summary>
        private short cardNo = 0;

        /// <summary>
        /// 睡眠时间
        /// </summary>
        private readonly int SleepTime = 5;

        private bool zeroFlag = false;

        Dictionary<int, bool> zeroDone = new Dictionary<int, bool>()
        {
            {1, false},{2, false},{3, false},{4, false},{5, false},{6, false},{7, false},{8, false},
            {9, false},{10, false},{11, false},{12, false},{13, false},{14, false},{15, false},{16, false},
            {17, false},{18, false},{19, false},{20, false},{21, false},{22, false},{23, false},{24, false},
            {25, false},{26, false},{27, false},{28, false},{29, false},{30, false},{31, false},{32, false},
        };

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

            // 设置编号
            cardNo = (short)pcie.Slot;
            short[] cardids = new short[8];
            int[] cardtypes = new int[8];
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                ecat_motion.M_Close(cardNo);
                Thread.Sleep(100);

                // 打开凌臣运控卡。操作凌臣运控卡的第一步必须是打开卡，且只能打开一次。
                ret = ecat_motion.M_Open(cardNo, 0);
                if (ret != 0)
                {
                    err = "确认板卡是否安装完成以及重复打开";
                    return false;
                }

                //设置急停参数
                short senseLevel = 1;                        //急停极性 → 1:常闭  0：常开
                ecat_motion.M_SetEmgAction(0x00, cardNo);    // 默认值直接掉使能
                ecat_motion.M_SetEmgInv(senseLevel, cardNo); //设置轴卡的急停极性
                ecat_motion.M_ClrEmg(cardNo);

                //初始化运控卡
                string ENIPath = "C:\\Program Files (x86)\\LCT\\PCIe-M60\\Eni\\eni.xml";//具体路径测试时再确认
                short numini = ecat_motion.M_LoadEni(ENIPath, cardNo);
                if (numini != 0)
                {
                    err = "检查加载的 ENI 路径，和供应商联系索要最新 ENI 并且替换";
                    return false;
                }

                // 重置fpga
                ecat_motion.M_ResetFpga(cardNo);
                Thread.Sleep(500);

                int time = 0;
                while (time < 3)
                {
                    time++;
                    // 保持输出状态
                    short numconnect = ecat_motion.M_ConnectECAT(1, cardNo);
                    if (numconnect != 0)
                    {
                        ecat_motion.M_ResetFpga(cardNo);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        time = 0;
                        break;
                    }
                }

                if (time > 0)
                {
                    err = "连接总线失败";
                    return false;
                }

                //获取 Ethercat 网络中的从站资源
                ret = ecat_motion.M_GetSlaveResource(out Sl_res, cardNo);
                if (ret == 10)
                {
                    err = $"轴重复打开,错误码:{ret}";
                }

                return true;
            });
            OnStatusChanged(DeviceStatus.Online);
        }
        #endregion

        #region 扫描从站信息
        ecat_motion.SL_RES Sl_res = new ecat_motion.SL_RES();

        /// <summary>
        /// 扫描轴
        /// </summary>
        /// <param name="axisNum"></param>
        /// <returns></returns>
        public void ScanAxis(out uint AxisNum)
        {
            CheckInit();

            AxisNum = (uint)Sl_res.AxisNum;
        }

        /// <summary>
        /// 扫描IO
        /// </summary>
        /// <param name="digitalIn"></param>
        /// <param name="digitalOut"></param>
        /// <returns></returns>
        public void ScanDigitalIO(out uint DiNum, out ushort DoNum)
        {
            CheckInit();

            DiNum = (uint)Sl_res.DiNum;
            DoNum = (ushort)Sl_res.DoNum;
        }

        /// <summary>
        /// 扫描模拟信号
        /// </summary>
        /// <param name="anglogIn"></param>
        /// <param name="anglogOut"></param>
        /// <returns></returns>
        public void ScanAnglog(out ushort AiNum, out ushort AoNum)
        {
            CheckInit();

            AiNum = (ushort)Sl_res.AiNum;
            AoNum = (ushort)Sl_res.AoNum;
        }
        #endregion

        #region 轴卡通用方法
        /// <summary>
        /// 检查是否完成
        /// </summary>
        /// <param name="ServoSts">轴状态</param>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public bool CheckMotionDone(int ServoSts, int axisNo = 0,double targetPulse=0)
        {
            CheckInit();

            if (axisNo == -1)
            {
                // 插补运动检查
                short ret = ecat_motion.M_CrdStatus(1, out var sts_1, out var cmdNum_1, out var space_1, 0, 0);
                if (ret != 0)
                    throw new Exception("调用获取卡状态函数出错,错误码:" + ret);

                var isRet = (!(sts_1 == 1 || cmdNum_1 != 0));

                if (isRet)
                {
                    OnLog(LogType.Debug, $"插补运动 CheckMotionDone 到位停止!");
                }

                return isRet;
            }
            else
            {
                // 判断是否运动完成
                //到位信号 bit11 当设置到位判断阈值,此信号可相信
                double currentPos;
                int motionIoStatus;
                var uAxisNo = GetAxisNum(axisNo);
                int targetPos = 0;

                ecat_motion.M_GetTargetPos(uAxisNo, ref targetPos, cardNo);

                // 1.首先检查轴的运动状态
                ecat_motion.M_GetSts(uAxisNo, out motionIoStatus, 1, cardNo);
                if ((motionIoStatus & 0x400) != 0x400)
                {
                    ecat_motion.M_GetEncPos(uAxisNo, out currentPos, 1, cardNo);
                    // 用目标位置减去当前的编码器位置
                    if ((Math.Abs(targetPos - currentPos) <= ServoSts))
                    {
                        OnLog(LogType.Debug, $"轴:{uAxisNo} CheckMotionDone 到位停止,目标位置:{targetPos},当前位置:{currentPos}!");
                        return true;
                    }
                }

                return false;
            }
        }

        #region 模拟信号IO
        public double GetAnalogIn(int index)
        {
            short value;
            CheckInit();
            int ret = ecat_motion.M_Get_Analog_Input((short)index, out value, 1, (short)cardNo);
            return value;
        }

        public double GetAnalogOut(int axisNo)
        {
            throw new NotImplementedException();
        }

        public void SetAnalogOut(int index, double analogVal)
        {
            CheckInit();
            throw new NotImplementedException();
        }
        #endregion

        #region 数字信号
        public bool GetDigitalIn(int index)
        {
            CheckInit();

            int res = 0;
            short value = 0;
            SafeNativeMethod(() =>
            {
                res = ecat_motion.M_Get_Digital_Chn_Input((short)index, out value, (short)cardNo);
                return true;
            }, $"Index={index},读取失败!");

            // 高电平
            return value == 1;
        }

        public bool GetDigitalOut(int index)
        {
            CheckInit();

            int res = 0;
            short value = 0;
            SafeNativeMethod(() =>
            {
                res = ecat_motion.M_Get_Digital_Chn_Output((short)index, out value, (short)cardNo);
                return true;
            }, $"Index={index},读取失败!");

            // 凌臣 高电平
            return value == 1;
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            CheckInit();
            short valueOut = (short)(digitalOut ? 1 : 0);

            SafeNativeMethod(() =>
            {
                int res = ecat_motion.M_Set_Digital_Chn_Output((short)index, valueOut, (short)cardNo);
                if(res != 0) OnLog(LogType.Debug, $"RES={res}");
                return res == 0;
            }, $"Index={index},输出设置失败");
        }
        #endregion

        /// <summary>
        /// 检查停止状态
        /// </summary>
        /// <param name="Axis">轴号</param>
        private void CheckStopState(short Axis)
        {
            int motionIoStatus;
            // 检查状态
            while (true)
            {
                ecat_motion.M_GetSts(Axis, out motionIoStatus, 1, cardNo);
                if ((motionIoStatus & 0x400) != 0x400) break;

                Thread.Sleep(SleepTime);
                if (this.Status == DeviceStatus.Stop) break;
            }
        }

        /// <summary>
        /// 检查是否停止
        /// </summary>
        /// <param name="uAxis"></param>
        /// <returns></returns>
        private bool CheckIsStop(short uAxisNo)
        {
            int motionIoStatus;
            ecat_motion.M_GetSts(uAxisNo, out motionIoStatus, 1, cardNo);
            OnLog(LogType.Debug, $"轴停止状态{motionIoStatus.ToString()}");
            return ((motionIoStatus & 0x400) != 0x400);
        }

        public void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus,
            double homeAcc, double Offset, AxisPML axisPML)
        {
            // 清错
            ClearError(axisNo);

            CheckAxisOn(axisNo);

            uint HIGH = (uint)(high * perPlus);
            uint LOW = (uint)(low * perPlus);
            uint ACC = (uint)(homeAcc * perPlus);

            short homeMethod = (short)homeMode;

            var uAxisNo = GetAxisNum(axisNo);
            SetAxisHome(uAxisNo, false);

            OnLog(LogType.Debug, $"轴:{uAxisNo} 开始Home HomeMode:{homeMode}");
            SafeNativeMethod((out string err) =>
            {
                err = "";
                // 1.使用 M_SetHomingMode & M_SetHomingPrm 函数设置回原点模式及相关回原点运动参数
                ecat_motion.M_SetHomingMode(uAxisNo, 6, cardNo);//使用驱动器的回零模式，一般为6，CSP为8

                //2.设置回零参数
                if (homeMode < 0) homeMethod = 35;
                ecat_motion.M_SetHomingPrm(uAxisNo, homeMethod, (int)Offset, HIGH, LOW, ACC, 0, cardNo);

                homeMethod = (short)homeMode;
                //如果回零方式为负数，需要单独写0x6098，M_SetHomingPrm暂不支持负数回零
                if (homeMethod < 0)
                {
                    ecat_motion.M_EcatSDOWrite(GetAxisSlave(uAxisNo), 0x6098, 0, (uint)((byte)homeMethod), 1, cardNo);
                }

                // 3.使用 M_HomingStart 函数执行回原点运动
                short ret = ecat_motion.M_HomingStart(uAxisNo, cardNo);

                Thread.Sleep(500);

                bool isResult = ret == 0;
                if (!isResult)
                {
                    var cardmsg = GetCodeCommon.GetCardErrorMsg(ret,1);
                    // 读取伺服错误代码
                    var subErr = getAxisSDOError(uAxisNo, axisPML);
                    err = $"轴:{uAxisNo} 回零失败:{ret}:{cardmsg},{subErr}";

                    return false;
                }

                return isResult;
            });
        }

        /// <summary>
        /// 回零完成状态
        /// </summary>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public override bool CheckHomeDone(int axisNo = 0)
        {
            bool isDone = false;
            var uAxisNo = GetAxisNum(axisNo);
            ecat_motion.M_GetSts(uAxisNo, out var sts, 1, cardNo);

            // 检查是否停止
            isDone = (sts & 0x400) != 0x400;

            if (isDone)
            {
                short res = 0;

                // 5.切换到驱动器CSP模式，一般为6，CSP为8
                res += ecat_motion.M_SetHomingMode(uAxisNo, 8, cardNo);

                // 6.使用 M_HomeCancel 函数取消回零模式
                res += ecat_motion.M_HomeCancel((ushort)(1 << (uAxisNo - 1)), cardNo);

                // 7.只要回零成功，就记录（因为回零存在非0位的清空）
                bool isHome = false;
                if (Status != DeviceStatus.Stop)
                {
                    isHome = res == 0;
                }

                SetAxisHome(uAxisNo, isHome);
            }

            return isDone;
        }
        /// <summary>
        /// 配置轴回零
        /// </summary>
        /// <param name="uAxisNo"></param>
        /// <param name="isHome"></param>
        private void SetAxisHome(int uAxisNo, bool isHome)
        {
            if (zeroDone.ContainsKey(uAxisNo))
            {
                zeroDone[uAxisNo] = isHome;
            }
            else
            {
                zeroDone.Add(uAxisNo, isHome);
            }
        }

        /// <summary>
        /// 回零取消
        /// </summary>
        /// <param name="axisNo">轴号</param>
        public void HomeCancel(int axisNo)
        {
            CheckInit();

            CheckAxisOn(axisNo);

            var uAxisNo = GetAxisNum(axisNo);
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = ecat_motion.M_HomeCancelSingleAxis(uAxisNo, cardNo);

                bool isResult = ret == 0;
                if (!isResult)
                {
                    err = $"轴:{uAxisNo},回零取消执行失败,错误代码:{ret}";
                    return false;
                }

                CheckStopState(uAxisNo);

                return isResult;
            });
        }

        public void Jog(double vel, double acc, double dec, double perPlus, 
            double slineTime, int axisNo, AxisPML axisPML)
        {
            CheckInit();

            CheckAxisOn(axisNo);

            var uAxisNo = GetAxisNum(axisNo);


            // 如果move，不进行再次JOG
            if (!CheckIsStop(uAxisNo))
            {
                OnLog(LogType.Error, $"轴:{uAxisNo} 准备JOG,但是未停止,重复发送指令!");
                return;
            }

            OnLog(LogType.Debug, $"轴:{uAxisNo} 开始JOG ,speed={vel} acc={acc} dec={dec} perplus={perPlus}");

            ecat_motion.M_ClrSts(uAxisNo, 1, cardNo);

            SafeNativeMethod((out string err) =>
            {
                err = "";
                ecat_motion.CmdPrm CmdPrm = new ecat_motion.CmdPrm();
                CmdPrm.acc = acc * perPlus;
                CmdPrm.dec = dec * perPlus;
                CmdPrm.sTime = (short)slineTime;
                short ret = ecat_motion.M_SetMove(uAxisNo, ref CmdPrm, cardNo);
                ret = ecat_motion.M_Jog(uAxisNo, vel * perPlus, (short)cardNo);

                bool isResult = ret == 0;
                if (!isResult)
                {
                    var cardmsg = GetCodeCommon.GetCardErrorMsg(ret, 1);
                    // 读取伺服错误代码
                    var subErr = getAxisSDOError(uAxisNo, axisPML);
                    err = $"轴:{uAxisNo} Jog失败,错误码:{ret}:{cardmsg},{subErr}";
                    return false;
                }

                return isResult;
            });
        }

        public void Move(double pos, double vel, double acc, double dec, double perPlus,
            double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            CheckInit();
            CheckAxisOn(axisNo);
            var uAxisNo = GetAxisNum(axisNo);
            // 目标位置
            int movePos = (int)(pos * perPlus);
            int times = 0;
            // 如果move，先停止确认，接着在进行位置判断
            if (!CheckIsStop(uAxisNo))
            {
                while(times<30)
                {
                    Thread.Sleep(50);
                    if (!CheckIsStop(uAxisNo))
                    {
                        OnLog(LogType.Warning, $"确认轴状态是否停止状态次数：{times}");
                        times++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (times==30)
                {
                    ecat_motion.M_GetEncPos(uAxisNo, out var currentPos, 1, cardNo);
                    if (Math.Abs(currentPos - movePos) > 50)
                    {
                        OnLog(LogType.Warning, $"轴:{uAxisNo} 准备Move,但是未停止,重复发送指令,当前位置:{currentPos / perPlus} 目标位置:{pos}!");
                        throw new DeviceException($"轴:{uAxisNo} 正在运动中!");
                    }
                }
                times = 0;
            }
            // 如果信号判断失败，再次判断目标位置和当前位置差距，如果差距很小，则
            
            short ret;
            ecat_motion.M_ClrSts(uAxisNo, 1, cardNo);
            ecat_motion.CmdPrm cmdPrm = new ecat_motion.CmdPrm();
            cmdPrm.acc = acc * perPlus;
            cmdPrm.dec = dec * perPlus;
            cmdPrm.sTime = (short)slineTime;

            OnLog(LogType.Debug, $"轴:{uAxisNo} 开始Move,speed={vel} pos={pos} acc={acc} dec={dec} perplus={perPlus}");

            SafeNativeMethod((out string err) =>
            {
                err = "";
                ret = ecat_motion.M_SetAxisBand(uAxisNo, 1, 500, cardNo);
                ret = ecat_motion.M_SetMove(uAxisNo, ref cmdPrm, cardNo);
                if (isAbsMove)
                {
                    ret = ecat_motion.M_AbsMove(uAxisNo, movePos, vel * perPlus, (short)cardNo);
                }
                else
                {
                    ret = ecat_motion.M_RelMove(uAxisNo, movePos, vel * perPlus, (short)cardNo);
                }

                bool isResult = ret == 0;
                if (!isResult)
                {
                    var cardmsg = GetCodeCommon.GetCardErrorMsg(ret, 1);
                    // 读取伺服错误代码
                    var subErr = getAxisSDOError(uAxisNo, axisPML);                   
                    err = $"轴:{uAxisNo} Move失败,错误码:{ret}:{cardmsg},{subErr}";                  
                    return false;
                }

                // if (isResult)
                // 回过零后就不要在更新了
                // SetAxisHome(uAxisNo, false);
                return isResult;
            });
        }

        /// <summary>
        /// 读取错误码
        /// </summary>
        /// <param name="uAxisNo"></param>
        /// <param name="axisPML"></param>
        /// <returns></returns>
        private string getAxisSDOError(short uAxisNo, AxisPML axisPML)
        {
            string subErr = "";
            short slaveNo = 0;
            short index = 0;
            short subindex = 0;
            uint pBuf_Axis = 0;
            uint pBuf_Cad = 0;
            string AxiserrMsg = "";
            // 读取伺服错误代码
            short ret = ecat_motion.M_GetAxisSlaveIndex(uAxisNo, ref slaveNo, cardNo);
            if (ret != 0)
            {
                return $"获取从站号错误码:0x{ret}";
            }
            var re_603f = ecat_motion.M_EcatSDORead(slaveNo, 0X603F, 0x00, 2, out pBuf_Cad, 1, cardNo);
            if (re_603f != 0)
            {
                return $"SDORead_603f错误码:0x{pBuf_Cad}";
            }
          
            switch (axisPML)
            {
                case AxisPML.Unknown:
                    break;
                case AxisPML.ASDAB3:
                    index = 0x2001; subindex = 0x00; // 当前故障代码
                    var re_axis = ecat_motion.M_EcatSDORead(slaveNo, index, subindex, 2, out pBuf_Axis, 1, cardNo);
                    if (re_axis != 0)
                    {
                        return $"Card错误码:0x{pBuf_Cad},SDORead_Axis错误码:0x{pBuf_Axis}";
                    }
                    AxiserrMsg = GetCodeCommon.GetASDAB3ErrorNameOrByCardCode(pBuf_Axis, pBuf_Cad);
                    break;
                case AxisPML.SV630N:
                    index = 0x203F; subindex = 0x00;//203Fh高16位为厂商内部故障码，低16位为厂商外部故障码
                    re_axis = ecat_motion.M_EcatSDORead(slaveNo, index, subindex, 4, out pBuf_Axis, 1, cardNo);
                    if (re_axis != 0)
                    {
                        return $"Card错误码:0x{pBuf_Cad},SDORead_Axis错误码:0x{pBuf_Axis}";
                    }                   
                    AxiserrMsg = GetCodeCommon.GetSV630NErrorNameOrByCardCode(pBuf_Axis, pBuf_Cad);
                    break;
                case AxisPML.LDHD:
                    index = 0x603F; subindex = 0x00;                    
                    AxiserrMsg = GetCodeCommon.GetLDHDErrorNameOrByCardCode((uint)pBuf_Axis, (uint)pBuf_Cad);//高创读取伺服错误代码
                    break;
                case AxisPML.Leadshine:
                    break;
                default:
                    break;
            }
            if (index == 0)
            {
                return $"Card错误码:0x{pBuf_Cad:X}";
            }
            subErr = $"Card错误码:0x{pBuf_Cad:X},伺服错误码:{AxiserrMsg}";
            return subErr;
        }

        
        
        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            CheckInit();

            int motionIoStatus;
            bool Alarm = false;
            bool Pel = false;
            bool Mel = false;
            bool Emg = false;
            bool Moving = false;
            bool OnPos = false;
            bool Org = false;
            bool isHome = false;
            bool SvOn = false;

            short uAxisNo = GetAxisNum(axisNo);
            SafeNativeMethod(() =>
            {
                var re = ecat_motion.M_GetSts(uAxisNo, out motionIoStatus, 1, cardNo);
                if (re!=0)
                {
                    // 读取伺服错误代码
                    short ret = ecat_motion.M_EcatSDORead(uAxisNo, 0X603F, 0, 2, out var pBuf, 1, cardNo);
                    if (ret == 0)
                    {
                        if (pBuf != 17)
                        {
                            OnLog(LogType.Info, $"伺服报警代码:{pBuf}");
                        }
                    }
                    throw new DeviceException($"轴:{uAxisNo} 状态获取失败,错误代码:{re}!");
                }

                Alarm = (motionIoStatus & 0x02) == 0x02;
                Pel = (motionIoStatus & 0x20) == 0x20;
                Mel = (motionIoStatus & 0x40) == 0x40;
                Moving = (motionIoStatus & 0x400) == 0x400;
                OnPos = (motionIoStatus & 0x800) == 0x800;
                Org = (motionIoStatus & 0x100000) == 0x100000;
                SvOn = (motionIoStatus & 0x200) == 0x200;
                Emg = false;

                if (zeroDone.ContainsKey(uAxisNo))
                {
                    isHome = zeroDone[uAxisNo];
                }
   
                return true;
            }, $"轴:{uAxisNo} 状态检查失败!");



            return new Dictionary<AxisStatus, bool>()
            {
                {AxisStatus.Alarm,Alarm},
                {AxisStatus.Emg, Emg},
                {AxisStatus.Mel,Mel},
                {AxisStatus.Pel,Pel},
                {AxisStatus.Org,Org},
                {AxisStatus.Moving,Moving},
                {AxisStatus.OnPos, OnPos},
                {AxisStatus.IsHome, isHome},
                {AxisStatus.SvOn,SvOn }

            };
        }

        /// <summary>
        /// 是否运动到极限
        /// </summary>
        /// <returns></returns>
        private bool IsEng(ushort axisNo)
        {
            int motionIoStatus;
            ecat_motion.M_GetSts(GetAxisNum(axisNo), out motionIoStatus, 1, cardNo);

            return ((motionIoStatus & 0x20) == 0x20 || (motionIoStatus & 0x40) == 0x40);
        }

        /// <summary>
        /// 对轴进行使能操作
        /// </summary>
        /// <param name="axisNo">轴编号</param>
        /// <param name="isOn"></param>
        /// <returns></returns>
        public void ServOn(int axisNo, bool isOn)
        {
            // 对轴进行使能操作
            var uAxis = GetAxisNum(axisNo);
            short ret;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                // 1.先检测轴是否处于报警状态
                ret = ecat_motion.M_GetSts(uAxis, out var ptrs, 1, cardNo);
                if ((ptrs & 0x02) == 0x02)
                {
                    ret = ecat_motion.M_ClrSts(uAxis, 1, cardNo);
                    Thread.Sleep(50);

                    // 2.第二次检测轴是否处于报警状态,再次失败，需要重启状态
                    ret = ecat_motion.M_GetSts(uAxis, out ptrs, 1, cardNo);
                    if ((ptrs & 0x02) == 0x02)
                    {
                        return false;
                    }
                }

                if (isOn)
                {
                    ret = ecat_motion.M_Servo_On(uAxis, cardNo);
                }
                else
                {
                    ret = ecat_motion.M_Servo_Off(uAxis, cardNo);
                }

                bool isResult = ret == 0;
                if (!isResult)
                {
                    err = $"轴:{uAxis} 使能失败,请检查驱动器硬件是否异常,错误代码:{ret}!";
                    return false;
                }

                return isResult;
            });
        }

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public double GetCurrentPos(int axisNo, double perPulse)
        {
            CheckInit();

            // 这里上使能，会导致轴没法手动下使能
            //CheckAxisOn(axisNo);

            double pos = -999, currentPos = 0;
            var uAxis = GetAxisNum(axisNo);
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = ecat_motion.M_GetEncPos(uAxis, out currentPos, 1, cardNo);

                bool isResult = ret == 0;
                if (isResult)
                {
                    pos = currentPos / perPulse;
                }

                if (!isResult)
                {
                    err = $"轴：{uAxis} 位置获取失败,错误代码:{ret}";
                    return false;
                }

                return isResult;
            });

            // 保留小数
            return Math.Round(pos, 3);
        }

        /// <summary>
        /// 修改轴的当前位置为设定值。应用场景：转盘旋转越界重置位置使用
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="perPulse">脉冲毫米比或脉冲角度比</param>
        /// <param name="position">位置。单位：毫米或角度</param>
        public override void SetCurrentPos(int axisNo, double perPulse, double position)
        {
            CheckInit();

            CheckAxisOn(axisNo);

            int setPos = (int)(position * perPulse);  //当前位置
            var uAxis = GetAxisNum(axisNo);
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = ecat_motion.M_SetCurrentPos(uAxis, setPos, cardNo);
                bool isResult = ret == 0;
                if (!isResult)
                {
                    err = $"轴：{uAxis} 设定位置为{position}失败,错误代码:{ret}";
                    return false;
                }

                return isResult;
            });
        }

        public void Stop(int axisNo, bool isAll = false)
        {
            CheckInit();

            if (axisNo == -1)
            {
                // 停止插补运动
                short mask_crd_1 = 1;//mask的bit0对应1#坐标系，bit1对应2#坐标系
                short option = 0;    //option 是对应的停止方法，0：平滑停止，1：急停
                ecat_motion.M_CrdStop(mask_crd_1, option, 0);//1#坐标系(使用0# FIFO )停止连续插补运动
            }
            else
            {
                var uAxis = GetAxisNum(axisNo);
                OnLog(LogType.Debug, $"轴:{uAxis} Stop");
                SafeNativeMethod((out string err) =>
                {
                    err = "";
                    short ret = 0;
                    if (isAll)
                    {
                        ret = ecat_motion.M_SetEmgAction(0x00, cardNo);
                    }
                    else
                    {
                        ret = ecat_motion.M_Stop((uint)(1 << (uAxis - 1)), 0, (short)cardNo);
                    }

                    bool isResult = ret == 0;
                    if (!isResult)
                    {
                        err = $"轴:{uAxis} 停止失败,错误代码:{ret}";
                        return false;
                    }

                    // 确认轴的状态是否有停止
                    CheckStopState(uAxis);

                    return isResult;
                });
            }
        }

        /// <summary>
        /// 状态重置
        /// </summary>
        /// <param name="axisNo">轴卡编号</param>
        public void ResetState(int axisNo)
        {
            ClearError(axisNo);

            // 使能
            ServOn(axisNo, true);
        }

        /// <summary>
        /// 使能检查
        /// </summary>
        /// <param name="axisNo"></param>
        /// <exception cref="DeviceException"></exception>
        private void CheckAxisOn(int axisNo)
        {
            var uAxisNo = GetAxisNum(axisNo);
            int motionIoStatus;
            var re = ecat_motion.M_GetSts(uAxisNo, out motionIoStatus, 1, cardNo);
            if (re != 0)
            {
                throw new DeviceException($"轴:{uAxisNo} 状态获取失败,检查总线连接，错误代码:{re}");
            }
            bool isServOn = (motionIoStatus & 0x200) == 0x200;
            if (!isServOn)
            {
                OnLog(LogType.Warning, $"轴:{uAxisNo}使能失败");
                //throw new DeviceException("轴使能丢失");
                // ServOn(axisNo, true);
                Thread.Sleep(200); // 给轴上使能
            }

            if ((motionIoStatus & 0x100) == 0x800000)
            {
                throw new DeviceException($"轴:{uAxisNo} 断线!");
            }
        }

        private void ClearError(int axisNo)
        {
            CheckInit();

            var uAxisNo = GetAxisNum(axisNo);
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret;
                ret = ecat_motion.M_ClrSts(uAxisNo, 1, cardNo);

                // 清错后，必须添加50ms延时
                Thread.Sleep(50);
                bool isResult = ret == 0;
                if (!isResult)
                {
                    err = $"轴:{uAxisNo} 清错失败,错误代码:{ret}";
                    return false;
                }

                // if (isResult)
                // 回过零后就不要在更新了
                // SetAxisHome(uAxisNo, false);
                return isResult;
            });

        }
        #endregion

        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            if (!HasInit()) return;

            // 轴使能关闭
            ScanAxis(out var axisNum);
            for (int i = 1; i <= axisNum; i++)
            {
                ServOn(i, false);
                Thread.Sleep(100);
            }

            // 断开连接卡
            try
            {
                ecat_motion.M_DisconnectECAT(cardNo);
            }
            catch (Exception)
            {
            }

            // 控制卡关闭
            ecat_motion.M_Close(cardNo);

            Status = DeviceStatus.Stop;
            OnLog(LogType.Debug, "凌臣轴卡对象释放");
            Status = DeviceStatus.Offline;
        }

        /// <summary>
        /// 清理报警信息
        /// </summary>
        public override void ClearEmg()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret;
                ret = ecat_motion.M_ClrEmg(cardNo);
                if (ret != 0)
                {
                    err = $"凌臣轴卡清除急停触发状态失败,错误代码:{ret}";
                    return false;
                }
                return true;
            });         
        }
        #region 插补运动

        /// <summary>
        /// 多轴直线插补运动
        /// </summary>
        /// <param name="AxisCount"></param>
        /// <param name="axisId"></param>
        /// <param name="pos"></param>
        /// <param name="perPlusArr"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            short CmdNum_1 = 0;                                    //1#坐标系的0#FIFO里存在的点位数（即未到达的点位数量）
            short Sts_1 = 0;                                       //1#坐标系的0#FIFO的运行状态（1表示运动中，0表示停止状态）
            short crd_1 = 1;                                       //1#坐标系
            int Space_1 = 0;                                       //1#坐标系的0#FIFO剩余空间（Space_1 + CmdNum_1 = 512）

            ///1.创建坐标系
            ecat_motion.CrdCfg crdCfg_1 = new ecat_motion.CrdCfg();

            int axisNum = axisId.Count;
            crdCfg_1.dimension = (short)axisNum;


            crdCfg_1.orignPos = new int[8];
            //同理，为了数据对齐，该数组的长度必须为8，
            //用户在使用时只需将参与连续插补的轴号放在crdCfg.orignPos[]的前4个元素里即可，后面4个元素是无效的。 
            for (int i = 0; i < crdCfg_1.dimension; i++)
            {
                crdCfg_1.orignPos[i] = 0;//起点坐标清零
            }
            crdCfg_1.axis = new short[8];
            //为了数据对齐，该数组的长度必须为8，
            //用户在使用时只需将参与连续插补的轴号放在crdCfg.axis[]的前4个元素里即可，后4个元素是无效的。 
            for (int i = 0; i < axisNum; i++)
            {
                crdCfg_1.axis[i] = (short)(axisId[i]);
            }

            double m_vel = vel[0] * perPlusArr[0];
            double m_acc = acc[0] * perPlusArr[0];

            crdCfg_1.evenTime = 10;           //最小匀速时间：使每段插补能够强制匀速运动的最小时间，若速度曲线只有加速度段和减速段，可以在尖峰处形成匀速的水平直线，减小速度突变带来的冲击，单位cycletime（默认 1ms）
            crdCfg_1.synVelMax = m_vel;      //坐标系的最大限制插补速度：分段速度大于该速度会默认按该速度运动，单位 pulse/S
            crdCfg_1.synAccMax = m_acc;     //坐标系的最大限制插补加速度：分段加速度大于该加速度会默认按该加速度运动，单位 pulse/S/S                                                                                                      
            crdCfg_1.synDecSmooth = m_acc;  //插补平滑停止减速度：单位 pulse/S/S
            crdCfg_1.synDecAbrupt = m_acc; //插补紧急停止减速度：单位 pulse/S/S
            //参数写入卡
            short ret = ecat_motion.M_SetCrd(crd_1, ref crdCfg_1, 0);
            if (ret != 0)
            {
                Thread.Sleep(100);
                ret = ecat_motion.M_SetCrd(crd_1, ref crdCfg_1, 0);
                if (ret != 0)
                    throw new Exception("调用坐标系写入函数出错,错误码:" + ret);
            }

            //清除坐标系缓存
            ret = ecat_motion.M_CrdClear(crd_1, 1, 0, 0);
            if (ret != 0)
                throw new Exception("调用坐标系清除函数出错,错误码:" + ret);

            ///2.写入坐标

            // 每个轴位置列
            int[] m_pos = new int[axisNum];
            for (int i = 0; i < axisNum; i++)
            {
                m_pos[i] = (int)(pos[i] * perPlusArr[i]);
            }

            ret = ecat_motion.M_Line(crd_1, crdCfg_1.dimension, ref crdCfg_1.axis[0], ref m_pos[0], m_vel, m_acc);
            if (ret != 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("伺服代码 ");
                for (int i = 0; i < axisNum; i++)
                {
                    var axisNo = (short)(axisId[i]);

                    // 读取伺服错误代码
                    ecat_motion.M_EcatSDORead(axisNo, 0X603F, 0, 2, out var pBuf, 1, cardNo);
                    sb.Append($"轴:{axisNo} 伺服代码:{pBuf} ");
                }

                throw new Exception($"调用点位写入函数出错,错误码:{ret},伺服代码:{sb}");
            }

            ///3.开始插补
            //ret = ecat_motion.M_CrdStart(1, 0, cardNo);
            ret = ecat_motion.M_CrdStartSingle(crd_1, 0, 0);
            if (ret != 0)
                throw new Exception("调用插补函数出错,错误码:" + ret);

            ///4.检查插补完成
            //while (true)
            //{
            //    // 休眠5ms
            //    Thread.Sleep(SleepTime);

            //    ret = ecat_motion.M_CrdStatus(crd_1, out Sts_1, out CmdNum_1, out Space_1, 0, cardNo);
            //    if (ret != 0)
            //        throw new Exception("调用获取卡状态函数出错,错误码:" + ret);

            //    if (!(Sts_1 == 1 || CmdNum_1 != 0))
            //    {
            //        ///插补完成
            //        break;
            //    }

            //    // 软件主动停止循环
            //    if (this.Status == DeviceStatus.Stop) break;
            //}

            /////5.插补动作完成,关闭插补功能
            //Thread.Sleep(1);
            //short mask_crd_1 = 1;//mask的bit0对应1#坐标系，bit1对应2#坐标系
            //short option = 0;    //option 是对应的停止方法，0：平滑停止，1：急停
            //ret = ecat_motion.M_CrdStop(mask_crd_1, option, 0);//1#坐标系(使用0# FIFO )停止连续插补运动
        }

        /// <summary>
        /// 多轴圆弧插补运动
        /// </summary>
        /// <param name="axisId"></param>
        /// <param name="pos"></param>
        /// <param name="perPlusArr"></param>
        /// <param name="vel"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            short CmdNum_1 = 0;                                    //1#坐标系的0#FIFO里存在的点位数（即未到达的点位数量）
            short Sts_1 = 0;                                       //1#坐标系的0#FIFO的运行状态（1表示运动中，0表示停止状态）
            short crd_1 = 1;                                       //1#坐标系
            int Space_1 = 0;                                       //1#坐标系的0#FIFO剩余空间（Space_1 + CmdNum_1 = 512）

            ///1.创建坐标系
            ecat_motion.CrdCfg crdCfg_1 = new ecat_motion.CrdCfg();
            int axisNum = axisId.Count;
            crdCfg_1.dimension = (short)axisNum;
            crdCfg_1.orignPos = new int[8];
            //同理，为了数据对齐，该数组的长度必须为8，
            //用户在使用时只需将参与连续插补的轴号放在crdCfg.orignPos[]的前4个元素里即可，后面4个元素是无效的。 
            for (int i = 0; i < crdCfg_1.dimension; i++)
            {
                crdCfg_1.orignPos[i] = 0;//起点坐标清零
            }
            crdCfg_1.axis = new short[8];
            //为了数据对齐，该数组的长度必须为8，
            //用户在使用时只需将参与连续插补的轴号放在crdCfg.axis[]的前4个元素里即可，后4个元素是无效的。 
            for (int i = 0; i < axisNum; i++)
            {
                crdCfg_1.axis[i] = (short)axisId[i];
            }

            double m_vel = vel[0] * perPlusArr[0];
            double m_acc = acc[0] * perPlusArr[0];

            crdCfg_1.evenTime = 10;           //最小匀速时间：使每段插补能够强制匀速运动的最小时间，若速度曲线只有加速度段和减速段，可以在尖峰处形成匀速的水平直线，减小速度突变带来的冲击，单位cycletime（默认 1ms）
            crdCfg_1.synVelMax = m_vel;      //坐标系的最大限制插补速度：分段速度大于该速度会默认按该速度运动，单位 pulse/S
            crdCfg_1.synAccMax = m_acc;     //坐标系的最大限制插补加速度：分段加速度大于该加速度会默认按该加速度运动，单位 pulse/S/S                                                                                                      
            crdCfg_1.synDecSmooth = m_acc;  //插补平滑停止减速度：单位 pulse/S/S
            crdCfg_1.synDecAbrupt = m_acc; //插补紧急停止减速度：单位 pulse/S/S
            //参数写入卡
            short ret = ecat_motion.M_SetCrd(crd_1, ref crdCfg_1, 0);
            if (ret != 0)
                throw new DeviceException("调用坐标系写入函数出错,错误码:" + ret);

            //清除坐标系缓存
            ret = ecat_motion.M_CrdClear(crd_1, 1, 0, 0);
            if (ret != 0)
                throw new DeviceException("调用坐标系清除函数出错,错误码:" + ret);

            ///2.写入坐标
            int[] m_pos = new int[axisNum];
            for (int i = 0; i < axisNum; i++)
            {
                m_pos[i] = (int)(pos[i] * perPlusArr[i]);
            }
            ret = ecat_motion.M_Arc2R(crd_1, ref crdCfg_1.axis[0], ref m_pos[0], radius, dir, m_vel, m_acc);
            if (ret != 0)
                throw new DeviceException("调用点位写入函数出错,错误码:" + ret);

            ///3.开始插补
            ret = ecat_motion.M_CrdStartSingle(crd_1, 0, 0);
            if (ret != 0)
                throw new DeviceException("调用插补函数出错,错误码:" + ret);
        }

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 比较器相关函数

        /// <summary>
        /// 将输入编码器，预设比较器，和输出口对应绑定，并设定比较数据。
        /// </summary>
        /// <param name="encoderNum">输入编码器号：0~3</param>
        /// <param name="cmpIndex">预设定比较器号：0~3</param>
        /// <param name="outNum">输出口序号：0~3</param>
        /// <param name="pulseWidth">脉冲宽度，单位10ns ，设定范围100~9999999</param>
        /// <param name="cmpPoses">比较点位值组</param>
        private void CMPInit(short encoderNum, short cmpIndex, short outNum, uint pulseWidth, int[] cmpPoses)
        {
            CheckInit();

            short IoM = 2; //比较器索引号，需要人工配置

            string msgHead = $"{Brand} 位置比较 {cmpIndex} 比较参数设定失败。";
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 设置触发输出模式：0-通用DO，1-位置比较输出，2-PWM输出
                ret = Mini_Cmp_Lib.TriggerOut_SetOutMode(cardNo, IoM, 1);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置触发输出模式失败。错误码：{ret}";
                    return false;
                }

                // 设置触发输出脉宽
                ret = Mini_Cmp_Lib.TriggerOut_SetPulseWidth(cardNo, IoM, outNum, pulseWidth);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置触发输出脉宽失败。错误码：{ret}";
                    return false;
                }

                // 编码器清零 编码器清零需要轴在零点位置执行。
                // 编码器和比较器的位置关系，通过驱动器参数设定和硬件接线保证一致。
                ret = Mini_Cmp_Lib.Encoder_SetCurrentData(cardNo, IoM, encoderNum, 0);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：编码器清零失败。错误码：{ret}";
                    return false;
                }

                // 将触发输出绑定到预设比较器。输出极性：0不去反；1取反
                uint lineMask = 0;
                uint preMask = (uint)(1 << cmpIndex);
                ret = Mini_Cmp_Lib.TriggerOut_SetParam(cardNo, IoM, outNum, lineMask, preMask, 0);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：将触发输出绑定到预设比较器失败。错误码：{ret}";
                    return false;
                }

                // 将预设定比较器绑定到指定编码器
                ret = Mini_Cmp_Lib.PreCmp_BindingEncoder(cardNo, IoM, encoderNum, cmpIndex);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：将预设定比较器绑定到指定编码器失败。错误码：{ret}";
                    return false;
                }

                // 清除预设定比较器比较队列
                ret = Mini_Cmp_Lib.PreCmp_ResetTrigData(cardNo, IoM, cmpIndex);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：清除预设定比较器比较队列失败。错误码：{ret}";
                    return false;
                }

                // 设定预设定比较器的触发方向： 0-正向， 1-反向， 2-双向
                ret = Mini_Cmp_Lib.PreCmp_SetTrigDir(cardNo, IoM, cmpIndex, 0);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设定预设定比较器的触发方向失败。错误码：{ret}";
                    return false;
                }

                // 设置比较点数据
                ret = Mini_Cmp_Lib.PreCmp_SetTrigData(cardNo, IoM, cmpIndex, cmpPoses);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：设置比较点数据失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

        }

        /// <summary>
        /// 指定通道输出脉宽信号
        /// </summary>
        /// <param name="cmpIndex">预设定比较器号：0~3</param>
        /// <param name="outNum">输出口序号：0~3</param>
        /// <param name="pulseWidth">脉冲宽度，单位10ns ，设定范围100~9999999</param>
        private void CMPOutput(short cmpIndex, short outNum/*, ushort pulseWidth*/)
        {
            CheckInit();

            short IoM = 2; //比较器索引号，需要人工配置

            string msgHead = $"{Brand} 位置比较 {cmpIndex} 输出口 {outNum} 输出脉宽信号失败。";
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 指定输出通道输出比较信号
                ret = Mini_Cmp_Lib.TriggerOut_SetManualOutput(cardNo, IoM, outNum);
                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }

                return true;
            });

        }

        /// <summary>
        /// 重新开始比较输出
        /// </summary>
        /// <param name="cmpIndex">预设定比较器号：0~3</param>
        private void CMPRestart(short cmpIndex)
        {
            CheckInit();

            short IoM = 2; //比较器索引号，需要人工配置

            string msgHead = $"{Brand} 位置比较 {cmpIndex} 重新开始比较失败。";
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 关闭预设比较器比较
                ret = Mini_Cmp_Lib.PreCmp_SetEnable(cardNo, IoM, (uint)cmpIndex, 0);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：关闭预设比较器比较失败。错误码：{ret}";
                    return false;
                }

                // 打开预设比较器比较
                ret = Mini_Cmp_Lib.PreCmp_SetEnable(cardNo, IoM, (uint)cmpIndex, 1);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：打开预设比较器比较失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

        }

        /// <summary>
        /// 获取已触发的比较个数
        /// </summary>
        /// <param name="cmpIndex">比较器号</param>
        /// <param name="outNum">输出口序号</param>
        /// <param name="cmpOutCount">已经触发的比较个数</param>
        private void CMPGetCmpCount(short cmpIndex, short outNum, out int cmpOutCount)
        {
            CheckInit();

            short IoM = 2; //比较器索引号，需要人工配置

            cmpOutCount = 0;
            uint tempCmpOutCount = 0;

            string msgHead = $"{Brand} 位置比较 {cmpIndex} 获取输出口 {outNum} 比较输出个数失败。";
            int ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                // 获取输出口比较输出个数
                ret = Mini_Cmp_Lib.TriggerOut_GetCounter(cardNo, IoM, outNum, ref tempCmpOutCount);
                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }

                return true;
            });

            cmpOutCount = (int)tempCmpOutCount;

        }

        #endregion

        #region SDO和PDO读写
        /// <summary>
        /// SDO读取
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="index">寄存器地址</param>
        /// <param name="subindex">子索引</param>
        /// <param name="data_size">寄存器大小，1byte=8,2byte=16,4byte=32</param>
        /// <param name="value">读取的值</param>
        /// <param name="count">读取的个数，默认为1</param>
        public void SDORead(short axisNo, short index, short subindex, short data_size, out int value, short count)
        {
            uint pBuf = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                string msgHead = $"{Brand} SDO读取失败。";
                short ret = ecat_motion.M_EcatSDORead(GetAxisSlave(axisNo), index, subindex, data_size, out pBuf, count, cardNo);
                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }
                return true;
            });
            value = (int)pBuf;
        }

        /// <summary>
        /// SDO写入
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="index">寄存器地址</param>
        /// <param name="subindex">子索引</param>
        /// <param name="data">写入的值</param>
        /// <param name="data_size">寄存器大小，1byte=8,2byte=16,4byte=32</param>
        public void SDOWrite(short axisNo, short index, short subindex, int data, short data_size)
        {
            uint udata = (uint)data;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                string msgHead = $"{Brand} SDO写入失败。";

                short ret = ecat_motion.M_EcatSDOWrite(GetAxisSlave(axisNo), index, subindex, udata, data_size, cardNo);
                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count)
        {
            short Value = 0;

            SafeNativeMethod((out string err) =>
            {
                err = "";
                string msgHead = $"{Brand} PDO读取{index}失败。";
                //short ret = ecat_motion.M_AxisPDORead(axis, index, subindex, data_size, ref Value, count, cardNo);

                short ret = ecat_motion.M_ReadActualTorque(axis, ref Value, count, cardNo);

                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }
                return true;
            });

            value = Value;
        }

        public void PDOWrite(short axis, short index, short subindex, int data, short data_size)
        {
            uint udata= (uint)data;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                string msgHead = $"{Brand} PDO写入{index}失败。";
                short ret = ecat_motion.M_AxisPDOWrite(axis, index, subindex, udata, data_size, cardNo);
                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        #endregion

        #region 获取轴的从站
        public short GetAxisSlave(short axisNo)
        {
            short slaveNum = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                string msgHead = $"{Brand} 轴{axisNo}从站获取失败。";
                short ret = ecat_motion.M_GetAxisSlaveIndex(axisNo, ref slaveNum, cardNo);
                if (ret != 0)
                {
                    err = $"{msgHead}。错误码：{ret}";
                    return false;
                }
                return true;
            });
            return slaveNum;
        }



        #endregion


        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {
            CheckInit();

            CheckAxisOn(axisNo);

            short curAxisNo = GetAxisNum(axisNo);

            List<int> curPos = pos.Select(x => (int)(x * perPulse)).ToList();

            vel = vel.Select(x => x * perPulse).ToList();

            // 如果move，先停止确认，接着在进行位置判断
            if (!CheckIsStop(curAxisNo))
            {
                // 如果信号判断失败，再次判断目标位置和当前位置差距，如果差距很小，则
                //ecat_motion.M_GetEncPos(uAxisNo, out var currentPos, 1, cardNo);
                //if (Math.Abs(currentPos - movePos) > 50)
                //{
                //    OnLog(LogType.Warning, $"轴:{uAxisNo} 准备Move,但是未停止,重复发送指令,当前位置:{currentPos / perPlus} 目标位置:{pos}!");
                //    return;
                //}
            }

            short ret;
            acc = acc * perPulse;
            dec = dec * perPulse;

            OnLog(LogType.Debug, $"轴:{curAxisNo} 开始连续运动, acc={acc} dec={dec} perplus={perPulse}");

            SafeNativeMethod((out string err) =>
            {
                err = "";
                //清除单轴缓存区中的数据
                ret = ecat_motion.M_AxisPVClear(curAxisNo, cardNo);
                //设置指定轴进行单轴的连续运动
                ret += ecat_motion.M_AxisPVMode(curAxisNo, acc, dec, cardNo);

                for (int i = 0; i < curPos.Count; i++)
                {
                    //设置指定轴单轴连续运动的点位
                    ret += ecat_motion.M_AxisPVDataAbs(curAxisNo, curPos[i], vel[i], cardNo);
                }
                int lspace = 0;
                //读取单轴连续运动的缓存区空间，最大2048
                ret += ecat_motion.M_AxisPVSpace(curAxisNo, ref lspace, cardNo);

                if (ret != 0)
                {
                    // 读取伺服错误代码
                    ecat_motion.M_EcatSDORead(curAxisNo, 0X603F, 0, 2, out var pBuf, 1, cardNo);
                    err = $"轴:{curAxisNo} 运动失败,错误代码:{ret},伺服错误代码:{pBuf}";

                    return false;
                }

                if (lspace != curPos.Count)
                {
                    err = $"轴:{curAxisNo} 连续运动缓存区空间{lspace}和设置的点位{curPos.Count}数量不等";
                    return false;
                }

                //开始指定轴的连续运动
                ret += ecat_motion.M_AxisPVStart(curAxisNo, cardNo);
                if (ret != 0)
                {
                    err = $"轴:{curAxisNo} 运动失败,错误代码:{ret}";
                    return false;
                }

                return true;
            });
        }

    }
}
