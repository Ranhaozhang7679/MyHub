using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Luster.Common.Tools.SharedMemory.CircularBuffer;

namespace Luster.Motion.DataStruct.VDevice
{
    public class VRobot : VirtualDeviceBase
    {
        public override int Sort => 14;

        /// <summary>
        /// 机器人
        /// </summary>
        public IRobot robot = null;


        /// <summary>
        /// 坐标系
        /// </summary>
        private CoordType _coordType;
        public CoordType CoordType
        {
            get => _coordType;
            set => _coordType = value;
        }


        /// <summary>
        /// 真实机器人名称
        /// </summary>
        public string RobotName { get; set; }

        /// <summary>
        /// 机器人配置
        /// </summary>
        /// <param name="hardDevice"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            base.SetDevice(hardDevice, out errMsg);

            robot = hardDevice as IRobot;
            if (robot == null)
            {
                errMsg = "设备不是机器人对象";
                return false;
            }

            // 硬件中Add时候进行的初始化
            ProcessAction(() =>
            {
                // 机器人初始化
                if (!hardDevice.HasInit())
                {
                    hardDevice.InitApi();

                    // 打开设备
                    hardDevice.Open();

                    // 启用
                    hardDevice.SetEnabled(true);
                }
            });

            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 连接机器人
        /// </summary>
        public void Connect()
        {
            ProcessAction(() =>
            {
                robot.ConnectController();
            }, null, true);
        }

        /// <summary>
        /// 断开连接机器人
        /// </summary>
        public void DisConnect()
        {
            ProcessAction(() =>
            {
                robot.DisConnectController();
            }, null, true);
        }

        /// <summary>
        /// 设置使能模式
        /// </summary>
        /// <param name="mode"></param>
        public void ServOn(short mode)
        {
            ProcessAction(() =>
            {
                robot.ServOn(mode);
            }, null, true);
        }


        /// <summary>
        /// 设置运动权限
        /// </summary>
        /// <param name="onoff"></param>
        public void SetMoveAccept(bool onoff)
        {
            ProcessAction(() =>
            {
                robot.SetMoveAccept(onoff);
            }, null, true);
        }

        /// <summary>
        /// 设置机器人手动单步/连续运动
        /// </summary>
        /// <param name="mode"></param>
        public void SetManualMode(short mode)
        {
            ProcessAction(() =>
            {
                robot.SetManualMode(mode);
            }, null, true);
        }

        /// <summary>
        /// 手动运动
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="IsJoint"></param>
        public void ManualMove(short Axis, bool IsJoint)
        {
            ProcessAction(() =>
            {
                robot.ManualMove(Axis, IsJoint);
            }, null, true);
        }


        /// <summary>
        /// 点到点方式运行到笛卡尔坐标系绝对位置的指令
        /// </summary>
        public void MovP(int index, bool IsAbsolute = true, double distance = 0)
        {
            ProcessAction(() =>
            {
                robot.MovP(index, IsAbsolute, distance);

            }, null, true);
        }

        public void SixMoveL(string pointName, double[] pose)
        {
            ProcessAction(() =>
            {
                robot.SixMoveL(pointName, pose);

            }, null, true);
        }


        /// <summary>
        /// 点到点方式控制机器人进行拱形移动的指令
        /// </summary>
        public void Jump(int index, bool isArch = true)
        {
            ProcessAction(() =>
            {
                robot.Jump(index, isArch);
            }, null, true);
        }

        /// <summary>
        /// 清除报警
        /// </summary>
        public void ClearAlarm()
        {
            ProcessAction(() =>
            {
                robot.ClearAlarm();
            }, null, true);
        }

        /// <summary>
        /// 获取机器人状态信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<RobotStatus, bool> GetRobotStatus()
        {
            Dictionary<RobotStatus, bool> tuples = new Dictionary<RobotStatus, bool>();

            ProcessAction(() =>
            {
                tuples = robot.GetRobotStatus();
            }, null, true);

            return tuples;
        }

        /// <summary>
        /// 获取机器人当前速率
        /// </summary>
        /// <returns></returns>
        public int GetSpeedCoefficient()
        {
            int speedCoefficient = 10;
            ProcessAction(() =>
            {
                speedCoefficient = robot.GetVelScale();
            }, null, true);
            return speedCoefficient;
        }

        /// <summary>
        /// 设置机器人当前速率
        /// </summary>
        /// <param name="value"></param>
        public void SetSpeedCoefficient(short value)
        {
            ProcessAction(() =>
            {
                robot.SetVelScale(value);
            }, null, true);
        }

        private string[] SixCartesianAxisName = { "X", "Y", "Z", "U", "V", "W" };

        private string[] CartesianAxisName = { "X", "Y", "Z", "U", "Hand", "V", "W" };
        private string[] JointAxisName = { "J1", "J2", "J3", "J4", "J5", "J6" };
        /// <summary>
        /// 获取机器人当前坐标
        /// </summary>
        /// <returns></returns>
        public List<KeyValue> GetCurrentPosion(CoordType coordType, bool isConnect)
        {
            List<double> points = new List<double>();
            List<KeyValue> values = new List<KeyValue>();
            if (coordType == CoordType.Cartesian)
            {
                ProcessAction(() =>
                {
                    robot.GetCurrentPosion(out points, false);
                }, null, isConnect);

                if (points.Count == 0)
                {
                    if (robot.AxisCount == 6)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            values.Add(new KeyValue() { Key = SixCartesianAxisName[i], Value = 0 });
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            values.Add(new KeyValue() { Key = CartesianAxisName[i], Value = 0 });
                        }
                    }

                }
                else
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        values.Add(new KeyValue() { Key = robot.AxisCount == 6 ? SixCartesianAxisName[i] : CartesianAxisName[i], Value = Math.Round(points[i], 3) });
                    }
                }
            }
            else
            {
                robot.GetCurrentPosion(out points, true);
                for (int i = 0; i < points.Count - 1; i++)
                {
                    values.Add(new KeyValue() { Key = JointAxisName[i], Value = Math.Round(points[i], 3) });
                }
            }
            return values;
        }

        public void GetGetPointFormTable()
        {
            robot.ReadPoint(0);
        }


        /// <summary>
        /// 获取共享点位列表中单个点位的信息
        /// </summary>
        /// <param name="index">共享点位序号</param>
        public List<double> ReadPoint(int index)
        {
            List<double> Point = new List<double>();

            ProcessAction(() =>
            {
                Point = robot.ReadPoint(index);
            }, null, true);

            return Point;
        }

        /// <summary>
        /// 往共享点位列表中写单个点位的信息
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Point"></param>
        public void WritePoint(int index, List<double> Point)
        {
            ProcessAction(() =>
            {
                robot.WritePoint(index, Point);
            }, null, true);
        }

        public void WriteCurSixPoint(string pointName)
        {
            ProcessAction(() =>
            {
                robot.WriteCurSixPoint(pointName);
            }, null, true);
        }

        [Ignore]
        public List<RobotPosition> Positions { get; set; } = new List<RobotPosition>();

        [Ignore]
        public List<SixRobotPosition> SixPositions { get; set; } = new List<SixRobotPosition>();

        /// <summary>
        /// 新增点位
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <exception cref="FriendlyException"></exception>
        public void AddPostion(string name, List<KeyValue> points)
        {
            if (robot.AxisCount == 4)
            {
                if (Positions.Any(u => u.Name == name))
                {
                    throw new FriendlyException($"点位名称:{name}已存在!");
                }

                Positions.Add(new RobotPosition
                {
                    Name = name,
                    PositionX = Convert.ToDouble(points[0].Value),
                    PositionY = Convert.ToDouble(points[1].Value),
                    PositionZ = Convert.ToDouble(points[2].Value),
                    PositionC = Convert.ToDouble(points[3].Value),
                    PositionH = Convert.ToDouble(points[4].Value),
                    Key = Guid.NewGuid(),
                    Robot = this
                });
            }
            else
            {

                if (SixPositions.Any(u => u.Name == name))
                {
                    throw new FriendlyException($"点位名称:{name}已存在!");
                }

                SixPositions.Add(new SixRobotPosition
                {
                    Name = name,
                    PositionX = Convert.ToDouble(points[0].Value),
                    PositionY = Convert.ToDouble(points[1].Value),
                    PositionZ = Convert.ToDouble(points[2].Value),
                    PositionU = Convert.ToDouble(points[3].Value),
                    PositionV = Convert.ToDouble(points[4].Value),
                    PositionW = Convert.ToDouble(points[5].Value),
                    Key = Guid.NewGuid(),
                    Robot = this
                });
            }

            Engine.IsNeedSave = true;
        }

        /// <summary>
        /// 移除点位
        /// </summary>
        /// <param name="name"></param>
        public void RemovePostion(string name)
        {
            if (robot.AxisCount == 4)
            {
                int num = Positions.RemoveAll(u => u.Name == name);
                if (num > 0)
                {
                    Engine.IsNeedSave = true;
                }
            }
            else
            {
                int num = SixPositions.RemoveAll(u => u.Name == name);
                if (num > 0)
                {
                    Engine.IsNeedSave = true;
                }
            }

        }

        #region 导入和导出

        public override XElement ExportXml()
        {
            var xml = base.ExportXml();

            if (Positions.Count > 0)
            {
                XElement xPos = new XElement(nameof(Positions));
                foreach (var item in Positions)
                {
                    xPos.Add(item.ExportXml());
                }

                xml.Add(xPos);
            }

            if (SixPositions.Count > 0)
            {
                XElement xPos = new XElement(nameof(SixPositions));
                foreach (var item in SixPositions)
                {
                    xPos.Add(item.ExportXml());
                }

                xml.Add(xPos);
            }

            return xml;

        }

        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);
            Positions.Clear();
            SixPositions.Clear();
            // 解析点位
            var xPos = xElement.Element(nameof(Positions));
            if (xPos != null)
            {
                foreach (var xItem in xPos.Elements())
                {
                    RobotPosition pos = new RobotPosition();
                    pos.ParserXml(xItem);
                    pos.Robot = this;
                    Positions.Add(pos);
                }
            }

            var xSixPos = xElement.Element(nameof(SixPositions));
            if (xSixPos != null)
            {
                foreach (var xItem in xSixPos.Elements())
                {
                    SixRobotPosition pos = new SixRobotPosition();
                    pos.ParserXml(xItem);
                    pos.Robot = this;
                    SixPositions.Add(pos);
                }
            }

            return;

        }

        #endregion

    }
}
