using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
//using Luster.SimDevice.VDevice;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class CylinderModel : BindableBase
    {
        /// <summary>
        /// 对应的ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                bool src = _isSelected;
                SetProperty(ref _isSelected, value);
                if (src != value)
                    OnSelected(value);
            }
        }

        /// <summary>
        /// 选择发生了变更
        /// </summary>
        public event Action<bool> SelectedChanged;

        public void OnSelected(bool isSelected)
        {
            SelectedChanged?.Invoke(isSelected);
        }

        /// <summary>
        /// 模组名称
        /// </summary>
        private string _module;
        public string Module { get => _module; set => SetProperty(ref _module, value); }

        #region 常规参数
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 气缸类型
        /// </summary>
        private CylinderCategory _vCategory;
        public CylinderCategory VCategory { get => _vCategory; set => SetProperty(ref _vCategory, value); }


        private string _category;
        public string Category { get => _category; set => SetProperty(ref _category, value); }

        private string _extendDOName;
        public string ExtendDOName { get => _extendDOName; set => SetProperty(ref _extendDOName, value); }

        private string _retractDOName;
        public string RetractDOName { get => _retractDOName; set => SetProperty(ref _retractDOName, value); }

        private string _extendDIName;
        public string ExtendDIName { get => _extendDIName; set => SetProperty(ref _extendDIName, value); }

        private string _retractDIName;
        public string RetractDIName { get => _retractDIName; set => SetProperty(ref _retractDIName, value); }

        /// <summary>
        /// 伸出气缸状态
        /// </summary>
        private bool _statusExtend;
        public bool StatusExtend { get => _statusExtend; set => SetProperty(ref _statusExtend, value); }

        /// <summary>
        /// 缩回气缸状态
        /// </summary>
        private bool _statusRetract;
        public bool StatusRetract { get => _statusRetract; set => SetProperty(ref _statusRetract, value); }

        public VCylinder Tag { get; set; }

        /// <summary>
        /// 气缸伸出名称
        /// </summary>
        private string _extendName;
        public string ExtendName { get => _extendName; set => SetProperty(ref _extendName, value); }

        /// <summary>
        /// 气缸缩回名称
        /// </summary>
        private string _retractName;
        public string RetractName { get => _retractName; set => SetProperty(ref _retractName, value); }

        /// <summary>
        /// 气缸伸出传感器名称
        /// </summary>
        private string _inExtendName;
        public string InExtendName { get => _inExtendName; set => SetProperty(ref _inExtendName, value); }

        /// <summary>
        /// 气缸缩回传感器名称
        /// </summary>
        private string _inRetractName;
        public string InRetractName { get => _inRetractName; set => SetProperty(ref _inRetractName, value); }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vVacuum"></param>
        public CylinderModel(VCylinder vCylinder)
        {
            ID = vCylinder.ID;
            IsSelected = false;
            Module = vCylinder.Module;
            VCategory = vCylinder.Category;
            Category = vCylinder.Category.GetDescription();
            Name = vCylinder.Name;
            ExtendDOName = vCylinder.OutExtend.Name;
            RetractDOName = vCylinder.OutRetract?.Name;
            ExtendDIName = vCylinder.InExtendSensor.Name;
            RetractDIName = vCylinder.InRetractSensor.Name;
            Tag = vCylinder;
            StatusExtend = false;
            StatusRetract = false;

            ExtendName = vCylinder.ExtendName;
            RetractName = vCylinder.RetractName;
            InExtendName = vCylinder.InExtendName;
            InRetractName = vCylinder.InRetractName;
        }
    }
}
