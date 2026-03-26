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
    public class VFlyingPhotoModel : BindableBase
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
        /// 飞拍模块名称
        /// </summary>
        private string _flyingPhotoName;
        public string FlyingPhotoName
        {
            get => _flyingPhotoName;
            set => SetProperty(ref _flyingPhotoName, value);
        }

        /// <summary>
        /// 从站号
        /// </summary>
        private int _slaveNo;
        public int SlaveNo
        {
            get => _slaveNo;
            set => SetProperty(ref _slaveNo, value);
        }


        public VFlyingPhoto Tag;

        public VFlyingPhotoModel(VFlyingPhoto vFlyingPhoto)
        {
            Tag = vFlyingPhoto;
            Module = vFlyingPhoto.Module;
            ID = vFlyingPhoto.ID;
            Name = vFlyingPhoto.Name;
            FlyingPhotoName = vFlyingPhoto.FlyingPhotoName;
            SlaveNo= vFlyingPhoto.SlaveNo;

            //StatusList = new List<VRobotStatusModel>()
            //{
            //    new VRobotStatusModel(RobotStatus.Alarm,false),
            //    new VRobotStatusModel(RobotStatus.Connect,false),
            //    new VRobotStatusModel(RobotStatus.SvOn,false),
            //    new VRobotStatusModel(RobotStatus.Drag,false),
            //    new VRobotStatusModel(RobotStatus.Stop,false),
            //    new VRobotStatusModel(RobotStatus.Pause,false),
            //    new VRobotStatusModel(RobotStatus.Moving,false),
            //    new VRobotStatusModel(RobotStatus.ManualMoving,false),
            //    new VRobotStatusModel(RobotStatus.Emg,false),
            //    new VRobotStatusModel(RobotStatus.OnPos,false)
            //};

            //CurrentPoint = Tag.GetCurrentPosion(CoordType.Cartesian, false);
        }

    }
}
