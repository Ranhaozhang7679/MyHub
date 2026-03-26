using Luster.Motion.DataStruct;
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
    public class VRobotDialogVM : DialogVM
    {
        protected VRobotDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            RobotLists= _engine.Engine .GetRealDevices(typeof(IRobot)).Select (u=>u as IRobot).ToList();
        }

        #region 属性

        /// <summary>
        /// 机器人列表
        /// </summary>
        public List<IRobot> RobotLists { get; set; }

        /// <summary>
        /// 机器人
        /// </summary>
        private IRobot _robot;
        public IRobot Robot
        {
            get => _robot;
            set => SetProperty(ref _robot, value);
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
            set=> SetProperty(ref _module, value);
        }

        #endregion

        #region 方法
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            //构建虚拟设备对象,转成对应模式(这里需要添加)
            string name = (Robot as IDevice).Name;
            var vRobot = new VRobot()
            {
                ID = Guid.NewGuid(),
                Name = Name,
                Module = Module,
                DeviceID = (Robot as IDevice).ID,
            };
            result.Parameters.Add("VRobot", vRobot);
        }
        #endregion

    }


}
