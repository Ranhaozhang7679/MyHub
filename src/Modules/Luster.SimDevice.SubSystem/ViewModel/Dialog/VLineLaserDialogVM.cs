#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VLineLaserDialogVM
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       VLineLaserDialogVM.cs
* 创建时间:       2022/9/29 19:36:28
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      28497ae5-ddf3-4db5-a56b-f9cfa6f23695
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/29 19:36:28
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
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
    public class VLineLaserDialogVM : DialogVM
    {

        protected VLineLaserDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            //获取激光列表
            LaserLists = _engine.Engine.GetRealDevices(typeof(ILineLaser)).Select(u => u as ILineLaser).ToList();
        }

        #region 属性

        /// <summary>
        /// 真实激光列表
        /// </summary>
        public List<ILineLaser> LaserLists { get; set; }

        /// <summary>
        /// 真实激光
        /// </summary>
        private ILineLaser _laser;
        [Required]
        public ILineLaser Laser
        {
            get => _laser;
            set => SetProperty(ref _laser, value);
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
            set
            {
                SetProperty(ref _module, value);
            }
        }
        #endregion


        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            //构建虚拟设备对象,转成对应模式(这里需要添加)
            string name = (Laser as IDevice).Name;
            var vLaser = new VLineLaser()
            {
                ID = Guid.NewGuid(),
                Name = Name,
                Module = Module,
                LaserSN = Laser.SN,
                DeviceID = (Laser as IDevice).ID,
                LineLaserPath = new Common.DataStruct.DataModels.LPath(),
            };
            result.Parameters.Add("VLineLaser", vLaser);
        }
    }
}