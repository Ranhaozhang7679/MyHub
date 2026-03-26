#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VacuumDialogVM
* 机器名称:       F04846
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VacuumDialogVM.cs
* 创建时间:       2022/6/16 11:17:27
* 作    者:       房晶鹏
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       jingpengfang@lusterinc.com 
* 唯一标识：      ac299086-e4e4-460a-b201-902a279af15b
* 登录用户:       fangjingpeng
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/16 11:17:27
* 修 改 人:		  F04846
************************************************************************************/
#endregion
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class VacuumDialogVM : ModuleDialogVM
    {
        protected VacuumDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
        }


        #region 通用参数
        /// <summary>
        /// 真空名称
        /// </summary>
        private string _vacuumName;
        [Required]
        public string VacuumName { get => _vacuumName; set => SetProperty(ref _vacuumName, value); }
        #endregion

        #region DO参数
        /// <summary>
        /// 真空吸
        /// </summary>
        private VIO _outSuck;
        [Required]
        public VIO OutSuck { get => _outSuck; set => SetProperty(ref _outSuck, value); }

        /// <summary>
        /// 真空吹
        /// </summary>
        private VIO _outBlow;
        [Required]
        public VIO OutBlow { get => _outBlow; set => SetProperty(ref _outBlow, value); }

        #endregion

        #region DI参数
        /// <summary>
        /// 真空检知DI
        /// </summary>
        private VIO _inVacuumSensor;
        [Required]
        public VIO InVacuumSensor { get => _inVacuumSensor; set => SetProperty(ref _inVacuumSensor, value); }

        #endregion

        private int _minValue;
        /// <summary>
        /// 最小
        /// </summary>
        [Required]
        public int MinValue { get => _minValue; set => SetProperty(ref _minValue, value); }

        private int _maxValue;
        /// <summary>
        /// 最大值
        /// </summary>
        [Required]
        public int MaxValue { get => _maxValue; set => SetProperty(ref _maxValue, value); }

        private int _targetValue;

        /// <summary>
        /// 目标值
        /// </summary>
        [Required]
        public int TargetValue { get => _targetValue; set => SetProperty(ref _targetValue, value); }



        public VacuumModel vacuumModel;
        private bool _editFlag = false;

        /// <summary>
        /// 覆写对话框打开方法
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<VacuumModel>("VacuumModel", out var vModel) && vModel != null)
            {
                _editFlag = true;
                vacuumModel = vModel;
                Module = vModel.Module;
                VacuumName = vModel.Name;
                OutSuck = vModel.Tag.OutSuck;
                OutBlow = vModel.Tag.OutBlow;
                InVacuumSensor = vModel.Tag.InVacuumSensor;
            }
            MinValue = 10;
            MaxValue = 500;
            TargetValue = 200;
        }


        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);

            if (OutSuck == OutBlow)
            {
                throw new DeviceException("真空吸和吹索引冲突");
            }

            SimEngineUI.CheckIsUsed(new Motion.DataStruct.Virtual.IVirtualDevice[] { OutSuck, OutBlow, InVacuumSensor }, vacuumModel == null ? Guid.Empty : vacuumModel.ID);


            if (_editFlag)
            {
                _editFlag = false;
                //更新Device
                var device = deviceEngine.GetVirtualByID(vacuumModel.ID) as VVacuum;
                device.Name = VacuumName;
                device.OutSuck = OutSuck;
                device.OutBlow = OutBlow;
                device.InVacuumSensor = InVacuumSensor;
                device.MinValue = MinValue; device.MaxValue = MaxValue;
                device.TargetValue = TargetValue;
            }
            else
            {
                //构建虚拟设备对象
                VVacuum vacuum = new VVacuum()
                {
                    ID = Guid.NewGuid(),
                    Name = VacuumName,
                    Module = Module,
                    InVacuumSensor = InVacuumSensor,
                    OutSuck = OutSuck,
                    OutBlow = OutBlow,
                    MinValue = MinValue, MaxValue = MaxValue,
                    TargetValue = TargetValue,
                };

                // 添加对象
                deviceEngine.AddVirtual(vacuum);
            }
        }
    }
}
