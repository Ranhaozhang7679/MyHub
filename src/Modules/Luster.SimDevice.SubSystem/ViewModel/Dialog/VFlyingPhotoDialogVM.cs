using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class VFlyingPhotoDialogVM : DialogVM
    {
        protected VFlyingPhotoDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            FlyingPhotoLists = _engine.Engine.GetRealDevices(typeof(IFlyingPhoto)).Select(u => u as IFlyingPhoto).ToList();
        }

        #region 属性

        /// <summary>
        /// 飞怕模块列表
        /// </summary>
        public List<IFlyingPhoto> FlyingPhotoLists { get; set; }

        /// <summary>
        /// 飞拍模块
        /// </summary>
        private IFlyingPhoto _flyingPhoto;
        public IFlyingPhoto FlyingPhoto
        {
            get => _flyingPhoto;
            set => SetProperty(ref _flyingPhoto, value);
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        [Required]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 隶属模组
        /// </summary>
        private string _module;
        [Required]
        public string Module
        {
            get => _module;
            set => SetProperty(ref _module, value);
        }

        /// <summary>
        /// 从站号
        /// </summary>
        private int _slaveNo=1;
        [Required]
        public int SlaveNo
        {
            get => _slaveNo;
            set => SetProperty(ref _slaveNo, value);
        }

        #endregion

        #region 方法
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            //构建虚拟设备对象,转成对应模式(这里需要添加)
            string name = (FlyingPhoto as IDevice).Name;
            var vRobot = new VFlyingPhoto()
            {
                ID = Guid.NewGuid(),
                Name = Name,
                Module = Module,
                DeviceID = (FlyingPhoto as IDevice).ID,
                SlaveNo = SlaveNo
            };
            result.Parameters.Add("VFlyingPhoto", vRobot);
        }
        #endregion

    }
}
