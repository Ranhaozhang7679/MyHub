using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{

    public class VRobotStatusModel : BindableBase
    {
        /// <summary>
        /// 状态名称
        /// </summary>
        private RobotStatus _name;
        public RobotStatus Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 描述
        /// </summary>
        private string _alias;
        public string Alias { get => _alias; set => SetProperty(ref _alias, value); }


        /// <summary>
        /// 状态值
        /// </summary>
        private bool _status;
        public bool Status { get => _status; set => SetProperty(ref _status, value); }


        public VRobotStatusModel(RobotStatus status, bool isPass)
        {
            Name = status;
            Status = isPass;
            switch (status)
            {
                case RobotStatus.Alarm:
                    Alias = "ALM";
                    break;

                case RobotStatus.Connect:
                    Alias = "CONN";
                    break;

                case RobotStatus.SvOn:
                    Alias = "SvOn";
                    break;

                case RobotStatus.Drag:
                    Alias = "DRAG";
                    break;

                case RobotStatus.Stop:
                    Alias = "Stop";
                    break;

                case RobotStatus.Pause:
                    Alias = "Pause";
                    break;

                case RobotStatus.Moving:
                    Alias = "MOV";
                    break;

                case RobotStatus.ManualMoving:
                    Alias = "MMOV";
                    break;

                case RobotStatus.Emg:
                    Alias = "EMG";
                    break;


                case RobotStatus.OnPos:
                    Alias = "OPS";
                    break;

            }
        }

    }

    public class VRobotModel : BindableBase
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 隶属模组
        /// </summary>
        private string _module;
        public string Module
        {
            get => _module;
            set { SetProperty(ref _module, value); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 机器人名称
        /// </summary>
        private string _robotName;
        public string RobotName
        {
            get => _robotName;
            set => SetProperty(ref _robotName, value);
        }

        /// <summary>
        /// 机器人状态
        /// </summary>
        public List<VRobotStatusModel> StatusList { get; set; }


        private List<KeyValue> _currentPoint;
        public List<KeyValue> CurrentPoint { get => _currentPoint; set => SetProperty(ref _currentPoint, value); }


        public VRobot Tag;
        public VRobotModel(VRobot vRobot)
        {
            Tag = vRobot;
            Module = vRobot.Module;
            ID = vRobot.ID;
            Name = vRobot.Name;
            RobotName = vRobot.RobotName;

            StatusList = new List<VRobotStatusModel>()
            {
                new VRobotStatusModel(RobotStatus.Alarm,false),
                new VRobotStatusModel(RobotStatus.Connect,false),
                new VRobotStatusModel(RobotStatus.SvOn,false),
                new VRobotStatusModel(RobotStatus.Drag,false),
                new VRobotStatusModel(RobotStatus.Stop,false),
                new VRobotStatusModel(RobotStatus.Pause,false),
                new VRobotStatusModel(RobotStatus.Moving,false),
                new VRobotStatusModel(RobotStatus.ManualMoving,false),
                new VRobotStatusModel(RobotStatus.Emg,false),
                new VRobotStatusModel(RobotStatus.OnPos,false)
            };

            CurrentPoint = Tag.GetCurrentPosion(CoordType.Cartesian, false);
        }
    }
}
