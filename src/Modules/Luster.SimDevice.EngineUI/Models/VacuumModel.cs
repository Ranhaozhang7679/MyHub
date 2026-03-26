using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class VacuumModel: BindableBase
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
        /// 模组名称
        /// </summary>
        private string _module;
        public string Module { get => _module; set => SetProperty(ref _module, value); }

        #region 常规参数
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _vacuumSuckName;
        public string VacuumSuckName { get => _vacuumSuckName; set => SetProperty(ref _vacuumSuckName, value); }

        private string _vacuumBlowName;
        public string VacuumBlowName { get => _vacuumBlowName; set => SetProperty(ref _vacuumBlowName, value); }

        private string _vacuumCheckName;
        public string VacuumCheckName { get => _vacuumCheckName; set => SetProperty(ref _vacuumCheckName, value); }

        private int _timeOuts;
        public int TimeOuts { get => _timeOuts; set => SetProperty(ref _timeOuts, value); }

        private string _vacuumShield;
        public string VacuumShield { get => _vacuumShield; set => SetProperty(ref _vacuumShield, value); }

        /// <summary>
        /// 模组名称
        /// </summary>
        private bool _status;
        public bool Status { get => _status; set => SetProperty(ref _status, value); }

        #endregion

        /// <summary>
        /// 选择发生了变更
        /// </summary>
        public event Action<bool> SelectedChanged;

        public void OnSelected(bool isSelected)
        {
            SelectedChanged?.Invoke(isSelected);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vVacuum"></param>
        public VacuumModel(VVacuum vVacuum)
        {
            ID = vVacuum.ID;
            IsSelected = false;
            Module = vVacuum.Module;
            Name = vVacuum.Name;
            VacuumSuckName = vVacuum.OutSuck.Name;
            VacuumBlowName = vVacuum.OutBlow.Name;
            VacuumCheckName = vVacuum.InVacuumSensor.Name;
            Tag = vVacuum;
            Status = false;
        }

        public VVacuum Tag { get; set; }

    }
}
