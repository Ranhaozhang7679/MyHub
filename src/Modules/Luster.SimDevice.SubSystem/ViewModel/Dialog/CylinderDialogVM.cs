#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CylinderDialogVM
* 机器名称:       F04846
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       CylinderDialogVM.cs
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

using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class CylinderDialogVM : ModuleDialogVM
    {
        protected CylinderDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {


        }

        #region 通用参数

        /// <summary>
        /// 气缸名称
        /// </summary>
        private string _cylinderName;
        [Required]
        public string CylinderName { get => _cylinderName; set => SetProperty(ref _cylinderName, value); }

        /// <summary>
        /// 是否是单控气缸
        /// </summary>
        private bool _isSingle;
        public bool IsSingle
        {
            get { return _isSingle; }
            set { SetProperty(ref _isSingle, value); }
        }

        /// <summary>
        /// 气缸伸出名称
        /// </summary>
        private string _extendName;
        [Required]
        public string ExtendName { get => _extendName; set => SetProperty(ref _extendName, value); }

        /// <summary>
        /// 气缸缩回名称
        /// </summary>
        private string _retractName;
        [Required]
        public string RetractName { get => _retractName; set => SetProperty(ref _retractName, value); }

        /// <summary>
        /// 气缸伸出传感器名称
        /// </summary>
        private string _inExtendName;
        [Required]
        public string InExtendName { get => _inExtendName; set => SetProperty(ref _inExtendName, value); }

        /// <summary>
        /// 气缸缩回传感器名称
        /// </summary>
        private string _inRetractName;
        [Required]
        public string InRetractName { get => _inRetractName; set => SetProperty(ref _inRetractName, value); }

        #endregion

        #region DO参数
        /// <summary>
        /// 气缸缩回DO
        /// </summary>
        private VIO _cylinderRetractDO;
        public VIO CylinderRetractDO { get => _cylinderRetractDO; set => SetProperty(ref _cylinderRetractDO, value); }

        /// <summary>
        /// 气缸伸出DO
        /// </summary>
        private VIO _cylinderExtendDO;
        [Required]
        public VIO CylinderExtendDO { get => _cylinderExtendDO; set => SetProperty(ref _cylinderExtendDO, value); }
        #endregion

        #region DI参数
        /// <summary>
        /// 气缸伸出位DI
        /// </summary>
        private VIO _cylinderExtendDI;
        [Required]
        public VIO CylinderExtendDI { get => _cylinderExtendDI; set => SetProperty(ref _cylinderExtendDI, value); }

        /// <summary>
        /// 气缸缩回位DI
        /// </summary>
        private VIO _cylinderRetractDI;
        [Required]
        public VIO CylinderRetractDI { get => _cylinderRetractDI; set => SetProperty(ref _cylinderRetractDI, value); }

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




        #endregion

        /// <summary>
        /// 清除选型
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ComboBox comboBox)
            {
                comboBox.SelectedItem = null;
            }
        }));

        public CylinderModel cylinderModel;
        private bool _editFlag = false;

        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<CylinderModel>("CylinderModel", out var cModel) && cModel != null)
            {
                _editFlag = true;
                cylinderModel = cModel;
                Module = cModel.Module;
                IsSingle = cModel.VCategory == CylinderCategory.Single;
                CylinderName = cModel.Name;
                CylinderExtendDO = cModel.Tag.OutExtend;
                CylinderRetractDO = cModel.Tag.OutRetract;
                CylinderExtendDI = cModel.Tag.InExtendSensor;
                CylinderRetractDI = cModel.Tag.InRetractSensor;
                MinValue = cModel.Tag.MinValue;
                MaxValue = cModel.Tag.MaxValue;
                TargetValue = cModel.Tag.TargetValue;

                ExtendName = !string.IsNullOrEmpty(cModel.ExtendName) ? cModel.ExtendName : "伸出";
                RetractName = !string.IsNullOrEmpty(cModel.RetractName) ? cModel.RetractName : "缩回";
                InExtendName = !string.IsNullOrEmpty(cModel.InExtendName) ? cModel.InExtendName : "伸出状态";
                InRetractName = !string.IsNullOrEmpty(cModel.InRetractName) ? cModel.InRetractName : "缩回状态";

            }
            else
            {
                MinValue = 50;
                MaxValue = 1000;
                TargetValue = 300;
            }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);

            // 1.检查当前是否有重复
            if (CylinderExtendDO == CylinderRetractDO || CylinderExtendDI == CylinderRetractDI)
            {
                throw new DeviceException("气缸IO配置索引冲突");
            }

            var useIOs = new List<IVirtualDevice>()
            {
                CylinderExtendDO, CylinderExtendDI, CylinderRetractDI
            };

            if (!IsSingle)
            {
                useIOs.Add(CylinderRetractDO);
            }

            // 2.引用设备检查，排除掉已有设备
            //SimEngineUI.CheckIsUsed(useIOs.ToArray(), cylinderModel == null ? Guid.Empty : cylinderModel.ID);

            if (_editFlag)
            {
                _editFlag = false;

                //更新Device
                var device = deviceEngine.GetVirtualByID(cylinderModel.ID) as VCylinder;
                device.Name = CylinderName;
                device.OutExtend = CylinderExtendDO;
                device.Category = IsSingle ? CylinderCategory.Single : CylinderCategory.Double;
                device.OutRetract = IsSingle ? null : CylinderRetractDO;
                device.InExtendSensor = CylinderExtendDI;
                device.InRetractSensor = CylinderRetractDI;
                device.MinValue = MinValue;
                device.MaxValue = MaxValue;
                device.TargetValue = TargetValue;
                device.ExtendName = ExtendName;
                device.RetractName = RetractName;
                device.InExtendName = InExtendName;
                device.InRetractName = InRetractName;

                //20220803-Jack
                cylinderModel = new CylinderModel(device);
                result.Parameters.Add("CylinderModel", cylinderModel);
            }
            else
            {
                //构建虚拟设备对象
                VCylinder vCylinder = new VCylinder()
                {
                    ID = Guid.NewGuid(),
                    Name = CylinderName,
                    Module = Module,
                    Category = IsSingle ? CylinderCategory.Single : CylinderCategory.Double,
                    OutExtend = CylinderExtendDO,
                    OutRetract = IsSingle ? null : CylinderRetractDO,
                    InExtendSensor = CylinderExtendDI,
                    InRetractSensor = CylinderRetractDI,
                    MinValue = MinValue,
                    MaxValue = MaxValue,
                    TargetValue = TargetValue,
                    ExtendName = ExtendName,
                    RetractName = RetractName,
                    InExtendName = InExtendName,
                    InRetractName = InRetractName,
                };

                // 添加对象
                deviceEngine.AddVirtual(vCylinder);
            }
        }
    }
}
