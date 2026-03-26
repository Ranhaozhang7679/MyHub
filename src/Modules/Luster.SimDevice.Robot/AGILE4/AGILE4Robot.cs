using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Robot.AGILE;
using Luster.SimDevice.Robot.ATD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Robot.AGILE4
{
    /// <summary>
    /// 思灵四轴机器人
    /// </summary>
    public class AGILE4Robot : RobotBase, IRobot
    {
        public int AxisCount { get; set; } = 4;

        public override string Brand => "四轴思灵机器人";

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

                int ret = RobotSdkNet.ClearAlarm();
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人清除报警失败。错误码：{ret}";
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

                    int ret = RobotSdkNet.ConnectRobot(network.Ip);
                    if (ret != (int)ErrorCode.ERROR_OK)
                    {
                        err = $"{Brand}机器人连接失败。错误码：{ret}";
                        return false;
                    }

                    ret = RobotSdkNet.EnableApiControl(true);
                    if (ret != (int)ErrorCode.ERROR_OK)
                    {
                        err = $"{Brand}机器人启用外部API控制失败。错误码：{ret}";
                        return false;
                    }

                    return true;
                });
            }
        }

        public void DisConnectController()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";

                //int ret = RobotSdkNet.EnableApiControl(true);
                //if (ret != (int)ErrorCode.ERROR_OK)
                //{
                //    err = $"{Brand}机器人禁止外部API控制失败。错误码：{ret}";
                //    return false;
                //}

                RobotSdkNet.DisconnectRobot();

                return true;
            });
        }

        public void GetCurrentPosion(out List<double> posions, bool IsJoint)
        {
            throw new NotImplementedException();
        }

        public Dictionary<RobotStatus, bool> GetRobotStatus()
        {
            throw new NotImplementedException();
        }

        public short GetVelScale()
        {
            uint ratio = 0;
            SafeNativeMethod((out string err) =>
            {
                err = "";

                int ret = RobotSdkNet.GetSpeedRatio(ref ratio);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人获取机器人当前速率失败。错误码：{ret}";
                    return false;
                }
                return true;
            });

            return (short)ratio;
        }

        public void Jump(int index, bool isArch = true)
        {

        }

        public void ManualMove(short Axis, bool IsJoint)
        {
            SafeNativeMethod((out string err) =>
            {
                int ret = 0;
                double speed_percent = GetVelScale();
                err = "";
                RobotJoint robotJoint = new RobotJoint();
                RobotPos robotPos = new RobotPos();
                if (IsJoint)
                {                  
                    ret = RobotSdkNet.GetJoint(ref robotJoint);
                    if (ret != (int)ErrorCode.ERROR_OK)
                    {
                        err = $"{Brand}机器人获取当前机器人各轴角度失败。错误码：{ret}";
                        return false;
                    }

                    robotJoint.j[Axis] += 1;
                }
                else
                {                 
                    ret = RobotSdkNet.GetPos(ref robotPos);
                    if (ret != (int)ErrorCode.ERROR_OK)
                    {
                        err = $"{Brand}机器人获取当前机器人位置失败。错误码：{ret}";
                        return false;
                    }
                    switch (Axis)
                    {
                        case 1:
                            robotPos.x += 1;
                            break;
                        case 2:
                            robotPos.x -= 1;
                            break;
                        case 3:
                            robotPos.y += 1;
                            break;
                        case 4:
                            robotPos.y -= 1;
                            break;
                        case 5:
                            robotPos.z += 1;
                            break;
                        case 6:
                            robotPos.z -= 1;
                            break;
                        case 7:
                            robotPos.a += 1;
                            break;
                        case 8:
                            robotPos.a -= 1;
                            break;
                    }
                }

                ret = IsJoint ? RobotSdkNet.Move2Joint(robotJoint, speed_percent) : RobotSdkNet.Move2Pos(robotPos,0,0, speed_percent);
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

        public void MovP(int index, bool isAbsolute = true, double distance = 0)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                RobotPos robotPos = new RobotPos();
                uint tool_index = 0;
                uint wobj_index = 0;
                int ret = RobotSdkNet.GetPoseVariable2((uint)index, ref robotPos, ref tool_index, ref wobj_index);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人获取当前机器人位置失败。错误码：{ret}";
                    return false;
                }

                ret = RobotSdkNet.Move2Pos(robotPos, 0, 0, GetVelScale());
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人移动到位置{index}失败。错误码：{ret}";
                    return false;
                }

                return true;
            });
        }

        public List<double> ReadPoint(int index)
        {
            List<double> points = new List<double>();
            SafeNativeMethod((out string err) =>
            {
                err = "";
                RobotPos robotPos = new RobotPos();
                uint tool_index = 0;
                uint wobj_index = 0;
                int ret = RobotSdkNet.GetPoseVariable2((uint)index, ref robotPos, ref tool_index, ref wobj_index);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人获取当前机器人位置失败。错误码：{ret}";
                    return false;
                }
                points.Add(robotPos.x);
                points.Add(robotPos.y);
                points.Add(robotPos.z);
                points.Add(robotPos.a);
                points.Add(robotPos.cfg);

                return true;
            });


            return points;
        }

        public void ServOn(short mode)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                if (mode == 1)
                {
                    int ret = RobotSdkNet.PowerOn();
                    if (ret != (int)ErrorCode.ERROR_OK)
                    {
                        err = $"{Brand}机器人上电失败。错误码：{ret}";
                        return false;
                    }
                }
                else
                {
                    int ret = RobotSdkNet.PowerOff();
                    if (ret != (int)ErrorCode.ERROR_OK)
                    {
                        err = $"{Brand}机器人下电失败。错误码：{ret}";
                        return false;
                    }
                }

                return true;
            });
        }

        public void SetManualMode(short mode)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";

                int ret = RobotSdkNet.SetControlMode(RobotMode.ROBOT_MODE_MANUAL);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人设置手动低速模式失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void SetMoveAccept(bool onoff)
        {

        }

        public void SetVelScale(short inputDat)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";

                int ret = RobotSdkNet.SetSpeedRatio((uint)inputDat);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人设置机器人当前速率失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void SixMoveL(string pointName, double[] pose)
        {

        }

        public void Stop()
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";

                int ret = RobotSdkNet.StopMove();
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人停止运动失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void WriteCurPoint(int index)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";
                RobotPos robotPos = new RobotPos();
                int ret = RobotSdkNet.GetPos(ref robotPos);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人获取当前机器人位置失败。错误码：{ret}";
                    return false;
                }

                ret = RobotSdkNet.SetPoseVariable2((uint)index, robotPos);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人设置位姿形变量失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }

        public void WriteCurSixPoint(string pointName)
        {

        }

        public void WritePoint(int index, List<double> point)
        {
            SafeNativeMethod((out string err) =>
            {
                err = "";

                RobotPos robotPos = new RobotPos();
                robotPos.x= point[0];
                robotPos.y= point[1];
                robotPos.z= point[2];
                robotPos.a= point[3];
                robotPos.cfg = (int)point[4];

                int ret = RobotSdkNet.SetPoseVariable2((uint)index, robotPos);
                if (ret != (int)ErrorCode.ERROR_OK)
                {
                    err = $"{Brand}机器人设置位姿形变量失败。错误码：{ret}";
                    return false;
                }
                return true;
            });
        }
    }
}
