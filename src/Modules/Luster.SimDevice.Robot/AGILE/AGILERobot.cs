using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Robot.ATD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Luster.SimDevice.Robot.AGILE.DianaApi;

namespace Luster.SimDevice.Robot.AGILE
{
    public class AGILERobot : RobotBase, IRobot
    {
        public int AxisCount { get; set; } = 6;

        public SrvNetSt srvNetSt = new SrvNetSt();

        public static RobotStateInfo RobotStateInfo = new RobotStateInfo();

        private static int errorCode = 0;
        private static ErrorCallBack ErrFun = new ErrorCallBack(ErrorControl);
        private static RobotstateCallBack RobotStateFun = new RobotstateCallBack(LogRobotState);

        public override string Brand => "思灵机器人";

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
                //if (GetServOnStatus() != 1)
                //{
                //    ServOn(1);
                //}
                return true;
            });
            OnStatusChanged(DeviceStatus.Online);
        }

        public void ClearAlarm()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                int ret = DianaApi.cleanErrorInfo();
                if (ret != 0)
                {
                    err = $"{Brand}清除指定IP地址机器人的错误信息失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void ConnectController()
        {
            if (Adapter is Network network)
            {
                SafeNativeMethod((out string err) =>
                {
                    err = "";

                    srvNetSt = new SrvNetSt();
                    DianaApi.initSrvNetInfo(ref srvNetSt);

                    var code = DianaApi.getLinkState();
                    if (code == 0)
                    {
                        DianaApi.destroySrv();
                    }

                    srvNetSt.srvIp = network.Ip;
                    srvNetSt.locSrvPort = (short)network.Port;// 0:auto port

                    int ret = DianaApi.initSrv(ErrFun, RobotStateFun, srvNetSt);

                    if (ret < 0)
                    {
                        err = $"{Brand}机器人连接失败。错误码：{ret}";
                        return false;
                    }

                    return true;
                });
            }
        }

        public static void ErrorControl(int err)
        {
            errorCode = err;
        }

        public static void LogRobotState(RobotStateInfo pInfo)
        {
            RobotStateInfo = pInfo;
        }

        public void DisConnectController()
        {
            if (Adapter is Network network)
            {
                SafeNativeMethod((out string err) =>
                {
                    err = "";
                    int ret = DianaApi.holdBrake();
                    if (ret < 0)
                    {
                        err = $"{Brand}停止机械臂失败。错误码：{ret}";
                        return false;
                    }

                    ret = DianaApi.destroySrv(network.Ip);
                    if (ret < 0)
                    {
                        err = $"{Brand}机器人断开连接失败。错误码：{ret}";
                        return false;
                    }

                    return true;
                });
            }

        }

        public void GetCurrentPosion(out List<double> posions, bool IsJoint)
        {
            List<double> pos = new List<double>();

            if (IsJoint)
            {
                pos = RobotStateInfo.jointPos.ToList().Select((item) => item / Math.PI * 180).ToList();
            }
            else
            {
                pos = RobotStateInfo.tcpPos.ToList().Select((item) => item * 1000).ToList();
            }

            posions = pos;
        }

        public Dictionary<RobotStatus, bool> GetRobotStatus()
        {
            Dictionary<RobotStatus, bool> tuples = new Dictionary<RobotStatus, bool>();

            bool Alarm = false;
            bool Connect = false;
            bool ServOn = false;
            bool Drag = false;
            bool Stop = false;
            bool Pause = false;
            bool Moving = false;
            bool ManualMoving = false;
            bool Emg = false;


            if (Adapter is Network network)
            {
                SafeNativeMethod((out string err) =>
                {
                    err = "";
                    char state = DianaApi.getRobotState(network.Ip);
                    ServOn = state == 5 ? false : true;
                    Pause = state == 1 ? true : false;
                    Alarm = state == 6 ? true : false;
                    Moving = state == 0 ? true : false;
                    ManualMoving = state == 0 ? true : false;
                    Drag = state == 3 ? true : false;

                    Connect = DianaApi.getLinkState(network.Ip) == 0;

                    return true;
                });
            }


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

        public short GetVelScale()
        {
            return 100;
        }

        public void Jump(int index, bool isArch = true)
        {

        }

        public void ManualMove(short Axis, bool IsJoint)
        {
            if (Adapter is Network network)
            {

                if (Axis == 0)
                {
                    Stop();
                    return;
                }

                int ret = 0;

                if (!IsJoint)
                {
                    TcpDirectionE tcpDirectionE = new TcpDirectionE();
                    switch (Axis)
                    {
                        case 7:
                        case 1:
                            tcpDirectionE = TcpDirectionE.T_MOVE_X_UP;
                            break;

                        case 8:
                        case 2:
                            tcpDirectionE = TcpDirectionE.T_MOVE_X_DOWN;
                            break;

                        case 9:
                        case 3:
                            tcpDirectionE = TcpDirectionE.T_MOVE_Y_UP;
                            break;

                        case 10:
                        case 4:
                            tcpDirectionE = TcpDirectionE.T_MOVE_Y_DOWN;
                            break;

                        case 11:
                        case 5:
                            tcpDirectionE = TcpDirectionE.T_MOVE_Z_UP;
                            break;

                        case 12:
                        case 6:
                            tcpDirectionE = TcpDirectionE.T_MOVE_Z_DOWN;
                            break;
                    }

                    SafeNativeMethod((out string err) =>
                    {
                        err = "";

                        if (Axis < 7)
                        {
                            ret = DianaApi.moveTCP(tcpDirectionE, 0.1, 0.2, null, network.Ip);
                        }
                        else
                        {
                            ret = DianaApi.rotationTCP(tcpDirectionE, 0.1, 0.2, null, network.Ip);
                        }
                        if (ret != 0)
                        {
                            err = $"{Brand}机器人基于基坐标系手动笛卡尔运动失败。错误码：{ret}";
                            return false;
                        }

                        return true;
                    });
                }
                else
                {
                    JointDirectionE jointDirectionE = new JointDirectionE();
                    jointDirectionE = Axis % 2 == 0 ? JointDirectionE.T_MOVE_DOWN : JointDirectionE.T_MOVE_UP;

                    //关节索引号
                    int index = 1;
                    switch (Axis)
                    {
                        case 1:
                        case 2:
                            index = 0;
                            break;
                        case 3:
                        case 4:
                            index = 1;
                            break;
                        case 5:
                        case 6:
                            index = 2;
                            break;
                        case 7:
                        case 8:
                            index = 3;
                            break;
                        case 9:
                        case 10:
                            index = 4;
                            break;
                        case 11:
                        case 12:
                            index = 5;
                            break;
                    }

                    SafeNativeMethod((out string err) =>
                    {
                        err = "";

                        ret = DianaApi.moveJoint(jointDirectionE, index, 0.8, 0.8, network.Ip);
                        if (ret != 0)
                        {
                            err = $"{Brand}机器人基于基坐标系手动关节运动失败。错误码：{ret}";
                            return false;
                        }

                        return true;
                    });
                }
            }
        }

        public void MovP(int index, bool isAbsolute = true, double distance = 0)
        {

        }

        public List<double> ReadPoint(int index)
        {
            List<double> Point = new List<double>();
            return Point;
        }

        public void ServOn(short mode)
        {
            ClearAlarm();

            if (Adapter is Network network)
            {
                SafeNativeMethod((out string err) =>
                {
                    err = "";
                    if (mode == 1)
                    {
                        int ret = DianaApi.releaseBrake();
                        if (ret != 0)
                        {
                            err = $"{Brand}机器人上使能失败。错误码：{ret}";
                            return false;
                        }
                        Thread.Sleep(2000);

                        ret = DianaApi.freeDriving(0, network.Ip);
                        if (ret != 0)
                        {
                            err = $"{Brand}机器人退出拖曳模式失败。错误码：{ret}";
                            return false;
                        }
                    }
                    else if (mode == 0)
                    {
                        int ret = DianaApi.holdBrake();
                        if (ret != 0)
                        {
                            err = $"{Brand}机器人去使能失败。错误码：{ret}";
                            return false;
                        }
                    }
                    else if (mode == 2)
                    {
                        //目前有问题暂时不用
                        //int ret = DianaApi.releaseBrake();
                        //if (ret != 0)
                        //{
                        //    err = $"{Brand}机器人上使能失败。错误码：{ret}";
                        //    return false;
                        //}

                        //Thread.Sleep(2000);

                        //拖曳模式
                        int ret = DianaApi.freeDriving(1, network.Ip);
                        if (ret != 0)
                        {
                            err = $"{Brand}机器人进入拖曳模式失败。错误码：{ret}";
                            return false;
                        }
                    }


                    return true;
                });
            }

        }

        public void SetManualMode(short mode)
        {

        }

        public void SetMoveAccept(bool onoff)
        {

        }

        public void SetVelScale(short inputDat)
        {
            return;
            if (Adapter is Network network)
            {
                SafeNativeMethod((out string err) =>
                {
                    err = "";
                    int ret = DianaApi.setVelocityPercentValue(inputDat, network.Ip);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人设备速度百分比失败。错误码：{ret}";
                        return false;
                    }

                    return true;
                });
            }

        }

        public void Stop()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                int ret = DianaApi.stop();
                if (ret != 0)
                {
                    err = $"{Brand}机器人停止失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        public void WriteCurPoint(int index)
        {

        }

        public void WritePoint(int index, List<double> point)
        {

        }

        public void WriteCurSixPoint(string pointName)
        {
            if (Adapter is Network network)
            {
                double[] pos = new double[6];
                SafeNativeMethod((out string err) =>
                {
                    err = "";

                    double[] tcpPos = RobotStateInfo.tcpPos;
                    double[] joints = RobotStateInfo.jointPos;

                    int ret = DianaApi.addWayPoint(pointName, tcpPos, joints, "GTOOL", "GBase", network.Ip);
                    if (ret != 0)
                    {
                        ret = DianaApi.setWayPoint(pointName, tcpPos, joints, "GTOOL", "GBase", network.Ip);
                        if (ret != 0)
                        {
                            err = $"{Brand}机器人写入当前点位失败。错误码：{ret}";
                            return false;
                        }
                    }

                    return true;
                });
            }

        }

        public void SixMoveL(string pointName, double[] pose)
        {
            if (Adapter is Network network)
            {
                double[] pos = new double[6];
                SafeNativeMethod((out string err) =>
                {
                    err = "";
                    int ret = DianaApi.moveLToPose(pose, 0.2, 0.4, null, 0, 0, 0, network.Ip);
                    if (ret != 0)
                    {
                        err = $"{Brand}机器人MoveL指令运动到点位{pointName}失败。错误码：{ret}";
                        return false;
                    }

                    return true;
                });
            }
        }
    }
}
