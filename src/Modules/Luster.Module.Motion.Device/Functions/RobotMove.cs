using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class RobotMove : MotionFunction
    {

        [NotEmpty]
        [Parameter("机器人选择", 0, CN = "机器人", EditorType = typeof(VRobot))]
        public VDevice VRobot { get; set; }

        [NotEmpty]
        [Parameter("机器人点位", 1, CN = "点位")]
        public string Point { get; set; }

        [Parameter("机器人运动模式", 2, CN = "运动模式", DefaultV = RobotMoveMode.Move)]
        public RobotMoveMode RobotMoveMode { get; set; }

        [Range(1,100)]
        [Parameter("机器人速度百分比", 3, CN = "速度百分比", DefaultV = 10)]
        public int SpeedCoefficient { get; set; }


        [Parameter("机器人运动耗时", 4, CN = "耗时(ms)", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double CT { get; set; }


        public RobotMove()
        {
            this.Tips = "Robot点位运动";
            this.Icon = "\xe605";
            this.DynParam = true;            
        }

        public override bool DoExcute(out string errMsg)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            GetVDevice<VRobot>(VRobot, out var robot);

            //获取共享点位序号
            int pointIndex = GetRobotPoints(robot).ToList().IndexOf(Point) + 1;

            //设置机器人速度百分比
            robot.SetSpeedCoefficient((short)SpeedCoefficient);

            //运动模式
            switch (RobotMoveMode)
            {
                case RobotMoveMode.Move:
                    robot.MovP(pointIndex);
                    break;

                case RobotMoveMode.Jump:
                    robot.Jump(pointIndex);
                    break;
            }

            //耗时输出
            stopwatch.Stop();
            CT = stopwatch.ElapsedMilliseconds;

            return base.DoExcute(out errMsg);
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(VRobot))
            {
                GetVDevice<VRobot>(newV as VDevice, out var robotDevice);
                if (robotDevice != null)
                {
                    var points = GetRobotPoints(robotDevice);
                    OnDataSource(nameof(Point), points.ToArray());
                }
            }
        }

        protected List<string> GetRobotPoints(VRobot robot)
        {
            List<RobotPosition> Points = new List<RobotPosition>();
            Points = robot.Positions;
            List<string> PointNames = new List<string>();
            foreach (var point in Points)
            {
                PointNames.Add(point.Name);
            }
            return PointNames;
        }

    }
}
