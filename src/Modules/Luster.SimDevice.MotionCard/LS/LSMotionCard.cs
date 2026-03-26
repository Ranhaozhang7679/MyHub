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
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.MotionCard.Common;
using Luster.SimDevice.MotionCard.GG;
using Luster.SimDevice.MotionCard.LC;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Luster.SimDevice.MotionCard.LS
{
    public class LSMotionCard : MotionCardBase, IMotionCard
    {
        public override string Brand => "雷赛";

        /// <summary>
        /// 轴卡的插槽
        /// </summary>
        private ushort cardNo = 0;

        /// <summary>
        /// 睡眠时间
        /// </summary>
        private readonly int SleepTime = 5;

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
            cardNo = (ushort)pcie.Slot;

            ushort cardnum = 0;
            ushort[] cardids = new ushort[8];
            uint[] cardtypes = new uint[8];
            SafeNativeMethod(() =>
            {
                // 初始化
                short num = LTDMC.dmc_board_init();
                if (num <= 0 || num > 8)
                {
                    OnLog(LogType.Error, "初始化卡失败");
                    return false;
                }

                // 获取控制卡数量
                int res = LTDMC.dmc_get_CardInfList(ref cardnum, cardtypes, cardids);
                if (res != 0)
                {
                    OnLog(LogType.Error, "获取控制卡信息失败");
                    return false;
                }

                bool EtherCardFlag = false;
                for (ushort i = 0; i < cardnum; i++)
                {
                    int _cardtype = (int)cardtypes[i];
                    _cardtype = _cardtype & 0xfffff;
                    if (_cardtype == 0x15032 || _cardtype == 0x13032 || _cardtype == 0x13064)  //查找第一张E35032或E3032卡或E3064卡
                    {
                        cardNo = cardids[i];
                        EtherCardFlag = true;
                        break;
                    }
                }

                if (!EtherCardFlag)
                {
                    OnLog(LogType.Error, "不存在EtherCAT总线卡");
                    return false;
                }

                // 获取总线状态
                ushort err = 0;
                res = LTDMC.nmc_get_errcode(cardNo, 2, ref err);

                if (res == 0)
                {
                    if (err != 0) //总线报错
                    {
                        DateTime dt_start = DateTime.Now;
                        res = LTDMC.dmc_soft_reset(cardNo);//复位总线错误
                        if (res != 0)
                        {
                            OnLog(LogType.Error, $"控制卡总线复位错误 {res}");
                            return false;
                        }

                        this.ErrMsg = "复位中...";
                        while (true)
                        {
                            if ((DateTime.Now - dt_start).TotalMilliseconds >= 10000.0)//超时退出循环
                            {
                                OnLog(LogType.Error, "控制卡总线复位超时!");
                                return false;
                            }

                            if (this.Status == DeviceStatus.Stop) break;
                            res = LTDMC.nmc_get_errcode(cardNo, 2, ref err);

                            // 总线复位正常完成
                            if (res == 0)
                            {
                                if (err == 0)
                                {
                                    this.ErrMsg = "";
                                    break;
                                }
                            }
                            else
                            {
                                OnLog(LogType.Error, $"nmc_get_errcode2 {res}");
                                return false;
                            }

                            Thread.Sleep(50);
                        }
                    }
                }
                else
                {
                    OnLog(LogType.Error, $"nmc_get_errcode1 {res}");
                    return false;
                }
                return true;
            }, "控制卡初始化失败");

            OnStatusChanged(DeviceStatus.Online);
        }
        #endregion

        /// <summary>
        /// 扫描轴
        /// </summary>
        /// <param name="axisNum"></param>
        /// <returns></returns>
        public void ScanAxis(out uint axisNum)
        {
            CheckInit();

            axisNum = 0;

            uint totalAxis = 0;

            // 轴卡数量扫描
            SafeNativeMethod(() =>
            {
                return LTDMC.nmc_get_total_axes(cardNo, ref totalAxis) == 0;
            }, "轴卡扫描失败");

            axisNum = totalAxis;
        }

        /// <summary>
        /// 扫描IO
        /// </summary>
        /// <param name="digitalIn"></param>
        /// <param name="digitalOut"></param>
        /// <returns></returns>
        public void ScanDigitalIO(out uint digitalIn, out ushort digitalOut)
        {
            CheckInit();

            digitalIn = 0;
            digitalOut = 0;

            ushort diNum = 0;
            ushort doNum = 0;

            // 数字IO扫描
            SafeNativeMethod(() =>
            {
                return LTDMC.nmc_get_total_ionum(cardNo, ref diNum, ref doNum) == 0;
            }, "数字信号扫描失败");


            digitalIn = diNum;
            digitalOut = doNum;
        }

        /// <summary>
        /// 扫描模拟信号
        /// </summary>
        /// <param name="anglogIn"></param>
        /// <param name="anglogOut"></param>
        /// <returns></returns>
        public void ScanAnglog(out ushort anglogIn, out ushort anglogOut)
        {
            CheckInit();

            anglogIn = 0;
            anglogOut = 0;

            ushort aiNum = 0;
            ushort aoNum = 0;

            // 数字IO扫描
            SafeNativeMethod(() =>
            {
                return LTDMC.nmc_get_total_ionum(cardNo, ref aiNum, ref aoNum) == 0;
            }, "数字信号扫描失败");

            anglogIn = aiNum;
            anglogOut = aoNum;
        }


        #region 轴卡通用方法
        /// <summary>
        /// 检查是否完成
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public bool CheckMotionDone(int precision, int axisNo = 0, double targetPos = 0)
        {
            CheckInit();
            //return LTDMC.dmc_check_done(cardNo, (ushort)axisNo) != 0 && LTDMC.dmc_read_current_speed(cardNo, (ushort)axisNo) < 3;

            if (axisNo == -1)
            {
                //return LTDMC.dmc_check_done(cardNo, (ushort)axisNo) != 0 && LTDMC.dmc_read_current_speed(cardNo, (ushort)axisNo) < 5;
                return LTDMC.dmc_check_done_multicoor(cardNo, 0) == 1;
            }
            else
            {
                // 判断是否运动完成
                double currentPos = 0.0;
                //double targetPos = 0.0;

                // 1.首先检查轴的运动状态
                if (LTDMC.dmc_check_done(cardNo, (ushort)axisNo) != 0)
                {
                    Thread.Sleep(10);
                    //LTDMC.dmc_get_target_position_unit(cardNo, (ushort)axisNo, ref targetPos);
                    LTDMC.dmc_get_encoder_unit(cardNo, (ushort)axisNo, ref currentPos);

                    if (precision == 45) precision = 10;
                    //OnLog(LogType.Debug, $"4.2:轴{axisNo}目标位:{targetPos};当前位{currentPos}");

                    // 用目标位置减去当前的编码器位置
                    if ((Math.Abs(targetPos - currentPos) <= precision))
                    {
                        OnLog(LogType.Debug, $"轴:{axisNo} CheckMotionDone 到位停止,目标位置:{targetPos},当前位置:{currentPos}!");
                        return true;
                    }
                }
                return false;

            }

        }

        #region 模拟信号IO
        public double GetAnalogIn(int axisNo)
        {
            CheckInit();

            int res = 0;
            int val = 0;
            ushort uIndex = (ushort)axisNo;
            SafeNativeMethod(() =>
            {
                //nmc_get_node_od读负数异常
                //从站地址, 主索引,长度等参数做到界面上?怎么与凌臣兼容?
                //res = LTDMC.nmc_get_node_od(cardNo, 2, 1010, 0x6000, uIndex, 32, ref val);
                res = LTDMC.nmc_read_txpdo_extra(cardNo, 2, (ushort)(uIndex - 1), 1, ref val);
                return true;
            }, $"Index={axisNo},读取失败!");

            // 高电平
            return val;
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
            ushort uIndex = (ushort)(index + 7);
            SafeNativeMethod(() =>
            {
                res = LTDMC.dmc_read_inbit(cardNo, uIndex);
                return true;
            }, $"Index={index},读取失败!");

            // 高电平
            return res == 0;
        }

        public bool GetDigitalOut(int index)
        {
            CheckInit();

            int res = 0;
            ushort uIndex = (ushort)(index + 7);
            SafeNativeMethod(() =>
            {
                res = LTDMC.dmc_read_outbit(cardNo, uIndex);

                return true;
            }, $"Index={index},读取失败!");

            // 雷赛 高电平
            return res == 0;
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            CheckInit();
            ushort uIndex = (ushort)(index + 7);
            ushort outVal = (ushort)(digitalOut ? 0 : 1);
            if (uIndex == 42)
            {

            }//调试断点用
            SafeNativeMethod(() =>
            {
                int res = LTDMC.dmc_write_outbit(cardNo, uIndex, outVal);
                return res == 0;
            }, "输出设置失败");
        }
        #endregion

        /// <summary>
        /// 检查停止状态
        /// </summary>
        /// <param name="uAxis">轴号</param>
        private void CheckStopState(ushort uAxis)
        {
            ushort state = 0;

            // 检查状态
            while (true)
            {
                LTDMC.nmc_get_axis_state_machine(cardNo, uAxis, ref state);
                if (state == 4) break;

                Thread.Sleep(SleepTime);
                if (this.Status == DeviceStatus.Stop) break;
            }
        }

        /// <summary>
        /// 检查是否停止
        /// </summary>
        /// <param name="uAxis"></param>
        /// <returns></returns>
        private bool CheckIsStop(ushort uAxis)
        {
            ushort state = 0;
            LTDMC.nmc_get_axis_state_machine(cardNo, uAxis, ref state);
            return state == 4;
        }

        public void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus, double homeAcc, double Offset, AxisPML axisPML)
        {
            // 清错
            ClearError(axisNo);

            CheckAxisOn(axisNo);

            //int timeout = 30 * 1000;
            //int runtime = 0;
            //string error;

            //原点命令发出后,延时判断原点OK信号 100ms
            double highSpeed = high * perPlus;
            double lowSpeed = low * perPlus;
            double tacc = high / homeAcc;
            ushort uAxis = (ushort)axisNo;
            ushort uHomeMode = (ushort)homeMode;

            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = LTDMC.nmc_set_home_profile(cardNo, uAxis, uHomeMode, lowSpeed, highSpeed, tacc, tacc, 0);
                ret = LTDMC.nmc_home_move(cardNo, uAxis);
                Thread.Sleep(500);

                bool isResult = ret == 0;
                if (!isResult)
                {
                    var cardmsg = GetCodeCommon.GetCardErrorMsg(ret, 2);
                    // 读取伺服错误代码
                    var subErr = getAxisSDOError((short)uAxis, axisPML);
                    err = $"轴:{uAxis} 回零失败,错误码:{ret}:{cardmsg},{subErr}";
                    return false;
                }

                return isResult;
            });
            //SafeNativeMethod(() =>
            //{
            //    // 1.使用 nmc_set_home_profile 函数设置回原点模式及相关回原点运动参数
            //    LTDMC.nmc_set_home_profile(cardNo, uAxis, uHomeMode, lowSpeed, highSpeed, tacc, tacc, 0);
            //    //// 2.确认轴先运动停止
            //    //double targetPulse = 0;
            //    //LTDMC.dmc_get_target_position_unit(cardNo, (ushort)axisNo, ref targetPulse);
            //    //while (!CheckMotionDone(50, axisNo, targetPulse))
            //    //{
            //    //    Thread.Sleep(SleepTime);
            //    //}

            //    //ushort statemachine=0;

            //    //// 2.确保轴在准备好状态
            //    //while(statemachine!=4)
            //    //{
            //    //    LTDMC.nmc_get_axis_state_machine(cardNo, uAxis, ref statemachine);
            //    //    Thread.Sleep(50);
            //    //}


            //    // 3.使用 nmc_home_move 函数执行回原点运动
            //    short res = LTDMC.nmc_home_move(cardNo, uAxis);
            //    Thread.Sleep(500);
            //    return true;

            //}, $"轴:{axisNo} 回零失败");
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
            ushort SlaveAddress = 0, Sub_SlaveAddress = 0;
            int pBuf_Cad = 0, pBuf_Axis = 0;
            string AxiserrMsg = "";
            short ret = LTDMC.nmc_get_axis_node_address(cardNo, (ushort)uAxisNo, ref SlaveAddress, ref Sub_SlaveAddress);
            if (ret != 0)
            {
                return $"获取从站号错误码:0x{ret}";
            }

            ret = LTDMC.nmc_get_node_od(cardNo, 2, SlaveAddress, 0X603F, 0x00, 16, ref pBuf_Cad);
            if (ret != 0)
            {
                return $"SDORead_603f错误码:0x{pBuf_Cad}";
            }

            short index = 0;
            short subindex = 0;

            switch (axisPML)
            {
                case AxisPML.Unknown:
                    break;
                case AxisPML.ASDAB3:
                    index = 0x2001; subindex = 0x00; // 当前故障代码
                    var re_axis = LTDMC.nmc_get_node_od(cardNo, 2, SlaveAddress, (ushort)index, (ushort)subindex, 16, ref pBuf_Axis);
                    if (re_axis != 0)
                    {
                        return $"Card错误码:0x{pBuf_Cad},SDORead_Axis错误码:0x{pBuf_Axis}";
                    }
                    AxiserrMsg = GetCodeCommon.GetASDAB3ErrorNameOrByCardCode((uint)pBuf_Axis, (uint)pBuf_Cad);
                    break;
                case AxisPML.SV630N:
                    index = 0x203F; subindex = 0x00;//203Fh高16位为厂商内部故障码，低16位为厂商外部故障码
                    re_axis = LTDMC.nmc_get_node_od(cardNo, 2, SlaveAddress, (ushort)index, (ushort)subindex, 32, ref pBuf_Axis);
                    if (re_axis != 0)
                    {
                        return $"Card错误码:0x{pBuf_Cad},SDORead_Axis错误码:0x{pBuf_Axis}";
                    }
                    AxiserrMsg = GetCodeCommon.GetSV630NErrorNameOrByCardCode((uint)pBuf_Axis, (uint)pBuf_Cad);
                    break;
                case AxisPML.LDHD:
                    index = 0X603F; subindex = 0x00;
                    AxiserrMsg = GetCodeCommon.GetLDHDErrorNameOrByCardCode((uint)pBuf_Axis, (uint)pBuf_Cad);                   
                    break;
                case AxisPML.Leadshine:
                    break;
                default:
                    break;
            }
            subErr = $"Card错误码:0x{pBuf_Cad:X},伺服错误码:{AxiserrMsg}";
            return subErr;
        }

        /// <summary>
        /// 回零完成状态
        /// </summary>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public override bool CheckHomeDone(int axisNo = 0)
        {
            ushort uAxis = (ushort)axisNo;
            bool isDone = false;      // 回零完成标志

            string msgHead = $"{Brand} 轴{axisNo}检查回零完成失败。";
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                ushort homeState = 0;
                ret = LTDMC.dmc_get_home_result(cardNo, uAxis, ref homeState);
                if (ret != 0)
                {
                    err = $"{msgHead}。原因：获取回零状态失败。错误码：{ret}";
                    return false;
                }

                // 回零完成检测回零阶段
                if (homeState == 1 && CheckMotionDone(500, axisNo))
                {
                    LTDMC.dmc_set_position(cardNo, uAxis, 0);//指令脉冲清零
                    isDone = true;
                }

                // 判定设备状态如果被调用者停止了，直接退出 回零完成检测
                //if (Status == DeviceStatus.Stop)
                //{
                //    err = $"{msgHead} 原因：被调用者终止检查。";
                //}

                return true;
            });

            return isDone;
        }

        /// <summary>
        /// 回零取消
        /// </summary>
        /// <param name="axisNo">轴号</param>
        public void HomeCancel(int axisNo)
        {
            CheckInit();
            CheckAxisOn(axisNo);
            ushort uAxis = (ushort)axisNo;
            SafeNativeMethod(() =>
            {
                short ret = LTDMC.dmc_stop(cardNo, uAxis, 0);
                CheckStopState(uAxis);
                return ret == 0;

            }, $"轴:{axisNo} 回零取消执行失败");
        }


        public void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML)
        {
            CheckInit();

            CheckAxisOn(axisNo);

            // 检查皮带是否处于停止状态,如果停止才再次进行JOG
            if (!CheckAxisStop(axisNo))
            {
                return;
            }

            OnLog(LogType.Debug, $"轴:{axisNo}开始JOG");

            short ret = 0;
            ushort axis = (ushort)axisNo;
            double dTacc = Math.Abs(vel / acc);
            double dTdec = Math.Abs(vel / dec);

            // 起始速度 uint/s
            double minVel = 0;
            double maxVel = vel * perPlus;
            double dStopVel = 0;

            SafeNativeMethod(() =>
            {
                if (vel > 0)
                {
                    ret = LTDMC.dmc_set_profile_unit(cardNo, axis, minVel, maxVel, dTacc, dTdec, dStopVel);
                }
                else
                {
                    ret = LTDMC.dmc_set_profile_unit(cardNo, axis, minVel, -maxVel, dTacc, dTdec, dStopVel);
                }

                ret = LTDMC.dmc_set_s_profile(cardNo, axis, 0, slineTime / 1000);//设置S段速度参数
                ret = LTDMC.dmc_set_dec_stop_time(cardNo, axis, dTdec); //设置减速停止时间
                ret = LTDMC.dmc_vmove(cardNo, axis, (ushort)(vel > 0 ? 1 : 0));//连续运动

                // 104 是正极限位置 105 是负极限位
                return (ret == 0 || ret == 104 || ret == 105);

            }, $"轴:{axisNo} Jog运动失败,错误码:{ret}:{GetCodeCommon.GetCardErrorMsg(ret, 2)},{getAxisSDOError((short)axisNo, axisPML)}");
        }

        public void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            CheckInit();

            CheckAxisOn(axisNo);

            OnLog(LogType.Debug, $"轴:{axisNo}开始Move");
            short ret = 0;
            int stopreason = 0;
            ushort uAxis = (ushort)axisNo;

            // 加速时间
            double dTacc = Math.Abs(vel / acc);

            // 减速时间
            double dTdec = Math.Abs(vel / dec);

            // 起始速度 uint/s
            double minVel = 0;
            double maxVel = vel * perPlus;

            // 减速时间
            double stopVel = 0;

            SafeNativeMethod(() =>
            {
                ret = LTDMC.dmc_set_profile_unit(cardNo, uAxis, minVel, vel * perPlus, dTacc, dTdec, stopVel);//设置速度参数
                ret = LTDMC.dmc_set_s_profile(cardNo, uAxis, 0, slineTime / 1000);//设置S段速度参数
                ret = LTDMC.dmc_set_dec_stop_time(cardNo, uAxis, dTdec); //设置减速停止时间

                ushort mode = (ushort)(isAbsMove ? 1 : 0);
                ret = LTDMC.dmc_pmove_unit(cardNo, uAxis, (pos * perPlus), mode);//定长运动
                if (ret != 0)
                {
                    LTDMC.dmc_get_stop_reason(cardNo, uAxis, ref stopreason);
                    //输出停止原因
                    switch (stopreason)
                    {
                        case 0:
                            OnLog(LogType.Debug, $"轴:停止原因正常停止");
                            break;
                        case 1:
                            OnLog(LogType.Debug, $"轴:停止原因ALM 立即停止，IMD_STOP_AT_ALM");
                            break;
                        case 2:
                            OnLog(LogType.Debug, $"轴:停止原因ALM 减速停止，DEC_STOP_AT_ALM");
                            break;
                        case 3:
                            OnLog(LogType.Debug, $"轴:停止原因LTC 外部触发立即停止，IMD_STOP_AT_LTC");
                            break;
                        case 4:
                            OnLog(LogType.Debug, $"轴:停止原因EMG 立即停止，IMD_STOP_AT_EMG");
                            break;
                        case 5:
                            OnLog(LogType.Debug, $"轴:停止原因正硬限位立即停止，IMD_STOP_AT_ELP");
                            break;
                        case 6:
                            OnLog(LogType.Debug, $"轴:停止原因负硬限位立即停止，IMD_STOP_AT_ELN");
                            break;
                        case 7:
                            OnLog(LogType.Debug, $"轴:停止原因正硬限位减速停止，DEC_STOP_AT_ELP");
                            break;
                        case 8:
                            OnLog(LogType.Debug, $"轴:停止原因负硬限位减速停止，DEC_STOP_AT_ELN");
                            break;
                        case 9:
                            OnLog(LogType.Debug, $"轴:停止原因正软限位立即停止，IMD_STOP_AT_SOFT_ELP");
                            break;
                        case 10:
                            OnLog(LogType.Debug, $"轴:停止原因负软限位立即停止，IMD_STOP_AT_SOFT_ELN");
                            break;
                        case 11:
                            OnLog(LogType.Debug, $"轴:停止原因正软限位减速停止，DEC_STOP_AT_SOFT_ELP");
                            break;
                        case 12:
                            OnLog(LogType.Debug, $"轴:停止原因负软限位减速停止，DEC_STOP_AT_SOFT_ELN");
                            break;
                        case 13:
                            OnLog(LogType.Debug, $"轴:停止原因命令立即停止，IMD_STOP_AT_CMD");
                            break;
                        case 14:
                            OnLog(LogType.Debug, $"轴:停止原因命令减速停止，DEC_STOP_AT_CMD");
                            break;
                        case 15:
                            OnLog(LogType.Debug, $"轴:停止原因其它原因立即停止，IMD_STOP_AT_OTHER");
                            break;
                        case 16:
                            OnLog(LogType.Debug, $"轴:停止原因其它原因减速停止，DEC_STOP_AT_OTHER");
                            break;
                        case 17:
                            OnLog(LogType.Debug, $"轴:停止原因未知原因立即停止，IMD_STOP_AT_UNKOWN");
                            break;
                        case 18:
                            OnLog(LogType.Debug, $"轴:停止原因未知原因减速停止，DEC_STOP_AT_UNKOWN");
                            break;
                        case 19:
                            OnLog(LogType.Debug, $"轴:停止原因外部 IO 减速停止，DEC_STOP_AT_DE");
                            break;
                        default:
                            OnLog(LogType.Debug, $"轴:停止原因{stopreason}");
                            break;
                    }
                }
                // 104 是正极限位置 105 是负极限位
                // 雷赛轴卡，如果碰到限位，LTDMC.dmc_get_target_position_unit(cardNo, (ushort)axisNo, ref targetPos)中targetPos不会被更新
                return (ret == 0 /*|| ret == 104 || ret == 105*/);
            }, $"轴:{axisNo} 运动失败,错误码:{ret}:{GetCodeCommon.GetCardErrorMsg(ret, 2)},{getAxisSDOError((short)axisNo, axisPML)}");
        }


        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            CheckInit();

            CheckAxisOn(axisNo,IsThrowException);

            uint motionIoStatus = 0;
            bool Alarm = false;
            bool Pel = false;
            bool Mel = false;
            bool Emg = false;
            bool Org = false;
            bool Moving = false;
            bool isHome = false;
            bool SvOn = false;


            SafeNativeMethod(() =>
            {
                motionIoStatus = LTDMC.dmc_axis_io_status(cardNo, (ushort)axisNo);
                short moving = LTDMC.dmc_check_done(cardNo, (ushort)axisNo);
                Alarm = (motionIoStatus & 0x01) == 0x01;
                Pel = (motionIoStatus & 0x02) == 0x02;
                Mel = (motionIoStatus & 0x04) == 0x04;
                Emg = (motionIoStatus & 0x08) == 0x08;
                Org = (motionIoStatus & 0x10) == 0x10;
                Moving = moving == 0;

                // 使能状态
                ushort Axis_State_machine = 0;
                var re = LTDMC.nmc_get_axis_state_machine(cardNo, (ushort)axisNo, ref Axis_State_machine);
                if (re == 0)
                {
                    SvOn = Axis_State_machine == 4;
                }
                else
                {
                    throw new DeviceException($"轴:{axisNo} 状态获取失败,错误码:{re}:{GetCodeCommon.GetCardErrorMsg(re, 2)}");
                }

                // 是否回零状态
                ushort homeState = 0;
                re = LTDMC.dmc_get_home_result(cardNo, (ushort)axisNo, ref homeState);
                if (re == 0)
                {
                    isHome = (homeState == 1);
                }
                else
                {
                    throw new DeviceException($"轴:{axisNo} 回零状态获取失败,错误码:{re}:{GetCodeCommon.GetCardErrorMsg(re, 2)}");
                }
                return true;
            }, $"轴:{axisNo} 状态检查失败!");

            return new Dictionary<AxisStatus, bool>()
            {
                {AxisStatus.Alarm,Alarm},
                {AxisStatus.Emg, Emg},
                {AxisStatus.Mel, Mel},
                {AxisStatus.Pel, Pel},
                {AxisStatus.Org, Org},
                {AxisStatus.Moving, Moving},
                {AxisStatus.OnPos, !Moving},
                {AxisStatus.IsHome, isHome},
                {AxisStatus.SvOn, SvOn} // 使能状态
            };
        }

        /// <summary>
        /// 是否运动到极限
        /// </summary>
        /// <returns></returns>
        private bool IsEng(ushort axisNo)
        {
            var mStatus = LTDMC.dmc_axis_io_status(cardNo, axisNo);

            return (mStatus & 0x04) == 0x04;
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

            short ret;
            if (isOn)
            {
                ret = LTDMC.nmc_set_axis_enable(cardNo, (ushort)axisNo);
            }
            else
            {
                ret = LTDMC.nmc_set_axis_disable(cardNo, (ushort)axisNo);
            }

            if (ret != 0)
            {
                OnLog(LogType.Error, $"轴：{axisNo} 使能失败!");
            }
        }

        /// <summary>
        /// 获取轴当前位置
        /// </summary>
        /// <param name="axisNo"></param>
        /// <returns></returns>
        public double GetCurrentPos(int axisNo, double perPulse)
        {
            CheckInit();

            CheckAxisOn(axisNo);

            ushort uAxis = (ushort)axisNo;
            double pos = -999, currentPos = 0;

            SafeNativeMethod(() =>
            {
                short ret = LTDMC.dmc_get_encoder_unit(cardNo, uAxis, ref currentPos);

                if (ret == 0)
                {
                    pos = currentPos / perPulse;
                }

                return ret == 0;
            }, $"轴:{axisNo} 位置获取失败");

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
            // base.SetCurrentPos(axisNo, perPulse, position);

            CheckInit();

            CheckAxisOn(axisNo);

            ushort uAxis = (ushort)axisNo;
            double setPos = position * perPulse;  //当前位置

            SafeNativeMethod(() =>
            {
                short ret = LTDMC.dmc_set_encoder_unit(cardNo, uAxis, setPos);

                return ret == 0;
            }, $"轴：{uAxis} 设定位置为{position}失败");
        }

        public void Stop(int axisNo, bool isAll = false)
        {
            CheckInit();

            //CheckAxisOn(axisNo);//按照LC写法，不检测，防止掉使能导致调用停止API前抛异常，停不下来，死循环

            OnLog(LogType.Debug, $"轴:{axisNo}开始Stop");

            ushort uAxisNo = (ushort)axisNo;
            SafeNativeMethod(() =>
            {
                short ret = 0;
                if (axisNo == -1)
                {
                    ret = LTDMC.dmc_stop_multicoor(cardNo, 0, 1);
                }
                else
                {
                    if (isAll)
                    {
                        ret = LTDMC.dmc_emg_stop(cardNo);
                    }
                    else
                    {
                        ret = LTDMC.dmc_stop(cardNo, uAxisNo, 1);
                    }
                }

                // 确认轴的状态是否有停止
                //CheckMotionDone(40, axisNo);
                //20250901，已经根据停止API的结果判断，不需要进一步判断轴是否停止，无法传入指定的位置（缺少相应轴的脉冲比输入）

                return ret == 0;

            }, $"轴:{axisNo} 停止失败");
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
        private void CheckAxisOn(int axisNo,bool IsThrowException=true)
        {
            ushort Axis_State_machine = 0;
            var re = LTDMC.nmc_get_axis_state_machine(cardNo, (ushort)axisNo, ref Axis_State_machine);
            if (re == 0) 
            {
                if (Axis_State_machine != 4 && IsThrowException)
                {
                    throw new DeviceException($"设备:{axisNo} 未使能!");
                }
            }
            else
            {
                throw new DeviceException($"设备:{axisNo} 获取轴状态失败,错误码:{re}:{GetCodeCommon.GetCardErrorMsg(re, 2)}");
            }

        }

        /// <summary>
        /// 检测轴是否在运动中
        /// </summary>
        /// <param name="axisNo"></param>
        private bool CheckAxisStop(int axisNo)
        {
            short state = LTDMC.dmc_check_done((ushort)cardNo, (ushort)axisNo);

            if (state == 0)
            {
                OnLog(LogType.Error, $"轴:{axisNo} 未停止!");
            }

            return state == 1;
        }

        private void ClearError(int axisNo)
        {
            CheckInit();
            ushort uAxisNo = (ushort)axisNo;
            SafeNativeMethod(() =>
            {
                short ret;
                ret = LTDMC.nmc_clear_axis_errcode(cardNo, uAxisNo);
                return ret == 0;
            });
        }
        #endregion

        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);

            // 控制卡关闭
            LTDMC.dmc_board_close();

            Status = DeviceStatus.Offline;
        }


        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            //轴状态
            ushort stateMachine = 0;
            //坐标系号
            ushort crdNo = 0;
            //轴数量
            ushort axisNum = (ushort)axisId.Count;
            //轴号数组
            ushort[] uaxisId = new ushort[axisNum];
            //轴位置数组
            double[] movPos = new double[axisNum];
            //起始速度
            double minVel = 0;
            //运动速度
            double maxVel = vel[0] * perPlusArr[0];
            //加减速时间
            double acct = vel[0] / acc[0];


            for (int i = 0; i < axisId.Count; i++)
            {
                uaxisId[i] = (ushort)axisId[i];
                movPos[i] = pos[i] * perPlusArr[i];
            }


            for (ushort axisid = 0; axisid < axisId.Count; axisid++)
            {
                LTDMC.nmc_get_axis_state_machine(cardNo, axisid, ref stateMachine);
                if (stateMachine != 4)
                {
                    OnLog(LogType.Error, $"轴：{axisid}进行插补报错!");
                    return;
                }
            }
            //设置直线插补运动速度曲线
            LTDMC.dmc_set_vector_profile_unit(cardNo, crdNo, minVel, maxVel, acct, acct, minVel);
            //执行直线插补运动曲线,绝对运动
            LTDMC.dmc_line_unit(cardNo, crdNo, axisNum, uaxisId, movPos, 1);

            while (true)
            {
                Thread.Sleep(SleepTime);
                if ((LTDMC.dmc_check_done_multicoor(cardNo, crdNo) == 0))
                    break;
            }


        }

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            //圆弧插补半径
            double rds = radius;
            //轴数量
            ushort axisCount = (ushort)axisId.Count;
            //插补圈数
            ushort circle = 0;
            //坐标系号
            ushort crdNo = 0;
            //圆弧方向
            // 0为顺时针
            // 1为逆时针
            ushort dr = (ushort)dir;

            //轴圆弧终点位置
            double[] tarPos = new double[axisCount];

            //轴号
            ushort[] axisList = new ushort[axisCount];

            //运动模式
            //0：相对运动模式
            //1: 绝对运动模式
            ushort posMode = 0;


            //轴状态
            ushort stateMachine = 0;

            //起始速度
            double minVel = 0;
            //运动速度
            double maxVel = vel[0] * perPlusArr[0];
            //加减速时间
            double acct = vel[0] / acc[0];


            //圆弧目标位置获取
            //轴号数组获取
            for (int i = 0; i < axisCount; i++)
            {
                tarPos[i] = pos[i] * perPlusArr[i];
                axisList[i] = (ushort)axisId[i];
            }

            //检查轴状态，判断是否可以运动
            for (ushort axisid = 0; axisid < axisCount; axisid++)
            {
                LTDMC.nmc_get_axis_state_machine(cardNo, (ushort)crdNo, ref stateMachine);
                if (stateMachine != 4)
                {
                    OnLog(LogType.Error, $"轴：{axisid}进行插补报错!");
                    return;
                }
            }
            //设置插补速度曲线参数
            LTDMC.dmc_set_vector_profile_unit(cardNo, crdNo, minVel, maxVel, acct, acct, minVel);
            //圆弧插补运动开始
            LTDMC.dmc_arc_move_radius_unit(cardNo, crdNo, axisCount, axisList, tarPos, radius, dr, circle, posMode);


            //等待运动完成
            while (true)
            {
                Thread.Sleep(SleepTime);
                if ((LTDMC.dmc_check_done_multicoor(cardNo, crdNo) == 0))
                    break;
            }
        }

        #region SDO和PDO读写
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

        #endregion

        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {

        }

    }
}