using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Robot.ATD;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Luster.Common.Tools.SharedMemory.CircularBuffer;
using static Luster.SimDevice.Robot.ATD.PMCLib;

namespace Luster.SimDevice.Robot.STEP
{
    public class ADTRobot : RobotBase, IRobot
    {
        public override string Brand => "ADT";

        public int AxisCount { get; set; } = 4;

        public PMCLib.ADT_point aDT_Point = new PMCLib.ADT_point();

        public PMCLib.ADT_pointAngle aDT_PointAngle = new PMCLib.ADT_pointAngle();

        public override void InitApi()
        {
            if (HasInit())
            {
                OnLog(LogType.Info, $"{Brand}_{ID} has initialize!");
                return;
            }
           
            SafeNativeMethod((out string err) =>
            {
                err = "";
                //1、获取网络连接状态
                //if (!GetConnectStatus())
                //{
                //    //2、连接控制器
                //    ConnectController();
                //}

                //2、连接控制器
                ConnectController();

                //3、使能
                if (GetServOnStatus() != 1)
                {
                    ServOn(1);
                }
                return true;
            });
            OnStatusChanged(DeviceStatus.Online);
        }


        #region 在不获取运动权限下使用
        /// <summary>
        /// 设置使能状态
        /// </summary>
        /// <param name="mode">0：断使能模式;1：上使能模式;2：拖拽模式；</param>
        public void ServOn(short mode)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                if (GetControlState())
                {
                    SetMoveAccept(false);
                }
                short ret = PMCLib.ADT_SetEnableStatus(mode);
                if (ret != 0)
                {
                    err = $"{Brand}机器人设置使能状态函数调用失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 手动运动
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="IsJoint"></param>
        public void ManualMove(short Axis, bool IsJoint)
        {
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                if (GetControlState())
                {
                    SetMoveAccept(false);
                }

                ret = IsJoint ? PMCLib.ADT_JointMoveManual(Axis) : PMCLib.ADT_EulerMoveManual(Axis);
                if (ret != 0)
                {
                    if (IsJoint)
                        err = $"{Brand}机器人手动关节运动失败。错误码：{ret}";
                    else
                        err = $"{Brand}机器人手动笛卡尔运动失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        #endregion

        /// <summary>
        /// 获取使能状态
        /// </summary>
        /// <returns>0：断使能模式;1：上使能模式;2：拖拽模式；3：模式切换中</returns>
        public short GetServOnStatus()
        {
            short datBuf = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_GetEnableStatus(ref datBuf, false);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取当前的使能状态函数调用失败，错误码：{ret}";
                    return false;
                }
                return true;
            });
            return datBuf;
        }

        /// <summary>
        /// 获取网络连接状态
        /// </summary>
        /// <returns></returns>
        public bool GetConnectStatus()
        {
            return PMCLib.ADT_GetConnectStatus();
        }


        public override void Close()
        {
            base.Close();
        }

        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            if (!HasInit()) return;

            string msgHead = $"{Brand}";
            string mgsHeadCore1 = $"{msgHead} 释放机器人失败。";

            // 断使能
            ServOn(0);

            // 断开连接
            DisConnectController();

            // 修改设备的状态
            OnStatusChanged(DeviceStatus.Offline);
        }

        /// <summary>
        /// 连接控制器
        /// </summary>
        /// <param name="ipAddress"></param>
        public void ConnectController()
        {
            if (Adapter is Network network)
            {
                SafeNativeMethod((out string err) =>
                {
                    err = "";
                    short ret = PMCLib.ADT_Connect2Controller_CustomAddress(network.Ip);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人连接失败。错误码：{ret}";
                        return false;
                    }

                    return true;
                });              
            }
        }


        /// <summary>
        /// 断开连接控制器
        /// </summary>
        /// <param name="ipAddress"></param>
        public void DisConnectController()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_Disconnect2Controller();
                if (ret != 0)
                {
                    err = $"{Brand}机器人断开连接失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        #region 机器人处于空闲停止状态下使用
        /// <summary>
        /// 获取运动权限
        /// </summary>
        public bool AutoMAccept()
        {
            short datBuf = -1;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_AutoMAccept(ref datBuf);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取运动权限函数调用失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

            return datBuf != 0 ? false : true;
        }

        /// <summary>
        /// 释放运动权限
        /// </summary>
        public bool AutoMRelease()
        {
            short datBuf = -1;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_AutoMRelease(ref datBuf);
                if (ret != 0)
                {
                    err = $"{Brand}机器人释放运动权限函数调用失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

            return datBuf == 0 ? true : false;
        }

        /// <summary>
        /// 设置运动权限
        /// </summary>
        /// <param name="onoff"></param>
        public void SetMoveAccept(bool onoff)
        {
            short datBuf = -1;
            int state = -1;
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                ret = PMCLib.ADT_GetRobotRunStatus(ref datBuf);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取机器人当前的状态函数调用失败。错误码：{ret}";
                    return false;
                }
                if (datBuf != 0)
                {
                    err = $"{Brand}机器人获取机器人当前的状态不是空闲或停止状态。错误码：{datBuf}";
                    return false;
                }

                if (onoff)
                {
                    ret = PMCLib.ADT_AutoMAccept(ref datBuf);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人获取运动权限函数调用失败。错误码：{ret}";
                        return false;
                    }

                    if (datBuf != 0)
                    {
                        err = $"{Brand}机器人获取运动权限失败。错误码：{datBuf}";
                        return false;
                    }

                    Thread.Sleep(300); //询问厂商要加延时

                    ret = PMCLib.ADT_GetControlState(ref state); //state：0未获取；1获取
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人获取当前是否获取控制器运动权限函数调用失败。错误码：{ret}";
                        return false;
                    }
                    if (state != 1)
                    {
                        err = $"{Brand}机器人当前未获取控制器运动权限。错误码：{state}";
                        return false;
                    }

                    return true;
                }
                else
                {
                    ret = PMCLib.ADT_AutoMRelease(ref datBuf);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人释放运动权限函数调用失败。错误码：{ret}";
                        return false;
                    }
                    if (datBuf != 0)
                    {
                        err = $"{Brand}机器人释放运动权限失败。错误码：{datBuf}";
                        return false;
                    }

                    Thread.Sleep(300);

                    ret = PMCLib.ADT_GetControlState(ref state);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人获取当前是否获取控制器运动权限函数调用失败。错误码：{ret}";
                        return false;
                    }
                    if (state != 0)
                    {
                        err = $"{Brand}机器人释放控制器运动权限失败。错误码：{state}";
                        return false;
                    }

                    return true;
                }

            });
        }
        #endregion

        /// <summary>
        /// 设置当前机器人的坐标系序号
        /// </summary>
        public void SetCurrentCoord()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_SetCurrentCoord(0, 0);
                if (ret != 0)
                {
                    err = $"{Brand}机器人设置当前机器人的坐标系序号失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 获取设置当前机器人的坐标系序号是否成功
        /// </summary>
        public void GetSetCoordError()
        {
            short datBuf = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_GetSetCoordError(ref datBuf);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取设置当前机器人的坐标系序号是否成功失败。错误码：{ret}";
                    return false;
                }

                if (datBuf != 0)
                {
                    err = $"{Brand}机器人获取设置当前机器人的坐标系序号失败。返回值错误码：{datBuf}";
                    return false;
                }

                return true;
            });

        }

        #region 在获取运动权限下使用
        /// <summary>
        /// MovP运动
        /// </summary>
        public void ADTMovP(int index, bool isAbsolute, double distance)
        {
            PMCLib.ADT_MovPPara aDT_MovPPara = new PMCLib.ADT_MovPPara();
            aDT_MovPPara.A = index;
            aDT_MovPPara.B = isAbsolute ? 0 : (float)distance;
            aDT_MovPPara.CP = 50; //平滑过渡百分比
            aDT_MovPPara.Acc = 30;//加速度百分比
            aDT_MovPPara.Dec = 30;//减速度百分比
            aDT_MovPPara.Spd = 50;//速度百分比
            aDT_MovPPara.Jerk = 60;//加加速度百分比
            aDT_MovPPara.PType = isAbsolute ? 0 : 1;

            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_AutoMoveP(aDT_MovPPara);
                if (ret != 0)
                {
                    err = $"{Brand}机器人ADT_AutoMoveP函数调用失败。错误码：{ret}";
                    return false;
                }

                short datBuf = 0;
                ret = PMCLib.ADT_AutoMovePError(ref datBuf);
                if (ret != 0)
                {
                    err = $"{Brand}机器人ADT_AutoMovePError函数调用失败。错误码：{ret}";
                    return false;
                }
                if (datBuf != 0)
                {
                    err = $"{Brand}机器人MovP指令执行失败。错误码：{datBuf}";
                    return false;
                }

                return true;
            });

        }

        /// <summary>
        /// MovArch运动
        /// </summary>
        public void ADTMovArch(int index, bool isArch)
        {
            PMCLib.ADT_MArchPara aDT_MArchPara = new PMCLib.ADT_MArchPara();
            aDT_MArchPara.A = index;
            aDT_MArchPara.B = 0; //Z轴上抬限位高度（绝对位置）
            aDT_MArchPara.C = 50; //Z轴上升高度（相对起始点）
            aDT_MArchPara.D = 50; //Z轴下降高度（相对终点）
            aDT_MArchPara.CP = 50; //平滑系数百分比
            aDT_MArchPara.Acc = 30;//加速度百分比
            aDT_MArchPara.Dec = 30;//减速度百分比
            aDT_MArchPara.Spd = 50;//速度百分比
            aDT_MArchPara.Jerk = 60;//加加速度百分比

            aDT_MArchPara.ZsAcc = 60;//Z 轴上升加速度百分比
            aDT_MArchPara.ZsDec = 60;//Z 轴上升减速度百分比
            aDT_MArchPara.ZsSpd = 60;//Z 轴上升速度百分比
            aDT_MArchPara.ZeAcc = 60;//Z 轴下降加速度百分比
            aDT_MArchPara.ZeDec = 60;//Z 轴下降减速度百分比
            aDT_MArchPara.ZeSpd = 50;//Z 轴下降速度百分比
            aDT_MArchPara.ArchType = isArch ? 0 : 1; //运动类型（0：拱形；1：门型）


            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_AutoMoveArch(aDT_MArchPara);
                if (ret != 0)
                {
                    err = $"{Brand}机器人ADT_AutoMoveArch函数调用失败。错误码：{ret}";
                    return false;
                }

                short datBuf = 0;
                ret = PMCLib.ADT_AutoMovePError(ref datBuf);
                if (ret != 0)
                {
                    err = $"{Brand}机器人ADT_AutoMovePError函数调用失败。错误码：{ret}";
                    return false;
                }
                if (datBuf != 0)
                {
                    err = $"{Brand}机器人MovP指令执行失败。错误码：{datBuf}";
                    return false;
                }

                return true;
            });

        }

        public void MovP(int index, bool isAbsolute = true, double distance = 0)
        {
            ////1、设置使能状态 ADT_SetEnableStatus(short inputDat)；
            //short state = GetServOnStatus();
            //if (state != 1) ServOn(1);


            ////2、获取运动权限：ADT_AutoMAccept(short *datBuf);
            //if (!GetControlState())
            //{
            //    SetMoveAccept(true);
            //}

            //3、设置指定用户和工具号：ADT_GetCurrentCoord(int *user,int *tool,bool realTime = true)；
            SetCurrentCoord();

            //4、判断设置的用户和工具号是否成功：ADT_GetSetCoordError(short *datBuf)
            GetSetCoordError();

            //5、MovP运动 6、MovP运动错误号
            ADTMovP(index, isAbsolute, distance);

            //7、MovP到位检查（咨询厂商：ADT_AutoWaitPos用的比较多）
            CheckMotionDone();

        }

        public void Jump(int index, bool isArch = true)
        {
            ////1、设置使能状态 ADT_SetEnableStatus(short inputDat)；
            //short state = GetServOnStatus();
            //if (state != 1) ServOn(1);


            ////2、获取运动权限：ADT_AutoMAccept(short *datBuf);
            //if (!GetControlState())
            //{
            //    SetMoveAccept(true);
            //}
            //3、设置指定用户和工具号：ADT_GetCurrentCoord(int *user,int *tool,bool realTime = true)；
            SetCurrentCoord();

            //4、判断设置的用户和工具号是否成功：ADT_GetSetCoordError(short *datBuf)
            GetSetCoordError();

            //5、MovP运动 6、MovP运动错误号
            ADTMovArch(index, true);

            //7、MovP到位检查（咨询厂商：ADT_AutoWaitPos用的比较多）
            CheckMotionDone();

            //8、若所有相关的运动使用结束，可考虑释放运动权限
            //AutoMRelease();
        }

        #endregion


        public bool CheckMotionDone()
        {
            short datBuf = -1;
            short ret = 0;

            int state = 0;

            SafeNativeMethod((out string err) =>
            {

                err = "";
                while (true)
                {
                    ret = PMCLib.ADT_GetRobotRunState(ref state);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人ADT_GetRobotRunState函数调用失败。错误码：{ret}";
                        return false;
                    }
                    if (state == 1)
                        break;
                    Thread.Sleep(10);
                }
                while (true)
                {

                    ret = PMCLib.ADT_AutoWaitPos(ref datBuf, true, false);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人ADT_AutoWaitPos函数调用失败。错误码：{ret}";
                        return false;
                    }
                    if (datBuf == 0)
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }

                return true;
            });

            return datBuf == 0 ? true : false;

        }

        /// <summary>
        /// 设置（修改）机器人当前速率
        /// </summary>
        public void SetVelScale(short value)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_SetCurrentVelScale(value);
                if (ret != 0)
                {
                    err = $"{Brand}机器人设置机器人当前速率函数调用失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// 获取机器人当前速率
        /// </summary>
        /// <returns></returns>
        public short GetVelScale()
        {
            short datBuf = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_GetCurrentVelScale(ref datBuf);
                if (ret != 0)
                {
                    err = $"{Brand}机器人设置机器人当前速率函数调用失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

            return datBuf;
        }

        /// <summary>
        /// 设置机器人手动单步/连续运动
        /// </summary>
        /// <param name="mode"></param>
        public void SetManualMode(short mode)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_ManualMode(mode);
                if (ret != 0)
                {
                    err = $"{Brand}机器人设置运动模式函数调用失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        public void GetCurrentPosion(out List<double> posions, bool IsJoint)
        {
            short ret = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                if (IsJoint)
                {
                    ret = PMCLib.ADT_GetCurrentPositionAngle(ref aDT_PointAngle, false);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人获取机器人当前关节坐标失败。错误码：{ret}";
                        return false;
                    }
                }

                else
                {
                    ret = PMCLib.ADT_GetCurrentPosition(ref aDT_Point, false);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人获取机器人当前笛卡尔坐标失败。错误码：{ret}";
                        return false;
                    }
                }
                return true;
            });

            posions = new List<double>();
            if (IsJoint)
            {
                posions.Add(aDT_PointAngle.J1);
                posions.Add(aDT_PointAngle.J2);
                posions.Add(aDT_PointAngle.J3);
                posions.Add(aDT_PointAngle.J4);
                posions.Add(aDT_PointAngle.handSide);
            }
            else
            {
                posions.Add(aDT_Point.x);
                posions.Add(aDT_Point.y);
                posions.Add(aDT_Point.z);
                posions.Add(aDT_Point.c);
                posions.Add(aDT_Point.handSide);
            }

        }

        public Dictionary<RobotStatus, bool> GetRobotStatus()
        {
            Dictionary<RobotStatus, bool> tuples = new Dictionary<RobotStatus, bool>();
            short ret = 0;

            bool Alarm = false;
            bool Connect = false;
            bool ServOn = false;
            bool Drag = false;
            bool Stop = false;
            bool Pause = false;
            bool Moving = false;
            bool ManualMoving = false;
            bool Emg = false;

            short datBuf = 0;


            SafeNativeMethod((out string err) =>
            {
                int scram = 0;
                int alarmNo = 0;
                err = "";
                ret = PMCLib.ADT_GetScramState(ref scram, ref alarmNo);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取当前机器人报警状态函数调用失败。错误码：{ret}";
                    return false;
                }
                Alarm = scram == 0 ? false : true;
                if (Alarm)
                {
                    Emg = (scram & 0x10) == 0x10;
                    OnLog(LogType.Info, $"机器人报警代码:{alarmNo}");
                }
                return true;
            });

            Connect = GetConnectStatus();

            short EnableStatu = GetServOnStatus();
            ServOn = EnableStatu == 1 ? true : false;
            Drag = EnableStatu == 2 ? true : false;

            SafeNativeMethod((out string err) =>
            {
                err = "";
                ret = PMCLib.ADT_GetRobotRunStatus(ref datBuf, false);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取当前机器人状态函数调用失败。错误码：{ret}";
                    return false;
                }
                Stop = datBuf == 0 ? true : false;
                Pause = datBuf == 1 ? true : false;
                Moving = datBuf == 2 ? true : false;
                ManualMoving = datBuf == 3 ? true : false;

                return true;
            });

            return new Dictionary<RobotStatus, bool>()
            {
                {RobotStatus.Alarm ,Alarm  },
                {RobotStatus.Connect ,Connect  },
                {RobotStatus.SvOn ,ServOn  },
                {RobotStatus.Drag ,Drag  },
                {RobotStatus.Stop ,Stop  },
                {RobotStatus.Pause  ,Pause  },
                {RobotStatus.Moving  ,Moving  },
                {RobotStatus.ManualMoving  ,ManualMoving  },
                {RobotStatus.Emg  ,Emg  },
                {RobotStatus.OnPos  ,false  }
            };
        }

        /// <summary>
        /// 共享列表中读点位
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<double> ReadPoint(int index)
        {
            List<double> Point = new List<double>();
            PMCLib.ADT_point aDT_Point = new PMCLib.ADT_point();
            SafeNativeMethod((out string err) =>
            {
                err = "";

                short ret = PMCLib.ADT_GetPointFormTable(index, ref aDT_Point);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取共享点位列表中单个点位的坐标信息函数调用失败。错误码：{ret}";
                    return false;
                }
                Point.Add(aDT_Point.x);
                Point.Add(aDT_Point.y);
                Point.Add(aDT_Point.z);
                Point.Add(aDT_Point.c);
                Point.Add(aDT_Point.handSide);

                //float[] pos= { 0,0,0,0,0,0};
                //ret = PMCLib.ADT_GetCurrentUserCoord(pos);


                return true;
            });

            return Point;
        }

        /// <summary>
        /// 共享列表中写点位
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WritePoint(int index, List<double> point)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                PMCLib.ADT_point aDT_Point = new PMCLib.ADT_point();
                aDT_Point.x = (float)point[0];
                aDT_Point.y = (float)point[1];
                aDT_Point.z = (float)point[2];
                aDT_Point.c = (float)point[3];
                aDT_Point.handSide = (float)point[4];
                short ret = PMCLib.ADT_SetPoint2Table(index, aDT_Point);
                if (ret != 0)
                {
                    err = $"{Brand}机器人把点位写入到点位列表中函数调用失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 共享列表中写当前点位
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteCurPoint(int index)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_RecordPoint2Table((short)index);
                if (ret != 0)
                {
                    err = $"{Brand}机器人把当前笛卡尔坐标点记录到共享点位表中函数调用失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 获取当前运动权限
        /// </summary>
        /// <returns>True:获取 False:未获取</returns>
        public bool GetControlState()
        {
            int state = -1;
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_GetControlState(ref state);
                if (ret != 0)
                {
                    err = $"{Brand}机器人获取当前是否获取控制器运动权限函数调用失败。错误码：{ret}";
                    return false;
                }

                return true;
            });

            if (state == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清除报警
        /// </summary>
        public void ClearAlarm()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_ProgramControl(4);
                if (ret != 0)
                {
                    err = $"{Brand}机器人设置机器人的运行状态函数调用失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        /// <summary>
        /// 停止当前指令运动
        /// </summary>
        public void Stop()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                short ret = PMCLib.ADT_AutoMoveStop(0);
                if (ret != 0)
                {
                    err = $"{Brand}机器人停止当前指令运动函数调用失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void WriteCurSixPoint(string pointName)
        {

        }

        public void SixMoveL(string pointName,double[] pose)
        {
            
        }
    }
}
