using Luster.Motion.DigitalSetup.Datas;
using Prism.Mvvm;
using System;

namespace Luster.Motion.DigitalSetup.ViewModel.Validations
{
    /// <summary>
    /// 验证ViewModel基类 - 提供通用功能
    /// </summary>
    public abstract class BaseValidationVM : BindableBase, IValidationControl
    {
        #region 属性

        private string _validationName;
        public string ValidationName
        {
            get => _validationName;
            set => SetProperty(ref _validationName, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value);
                OnConfigChanged();
            }
        }

        private DateTime _lastRunTime;
        public DateTime LastRunTime
        {
            get => _lastRunTime;
            set => SetProperty(ref _lastRunTime, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private string _validationResult;
        public string ValidationResult
        {
            get => _validationResult;
            set => SetProperty(ref _validationResult, value);
        }

        private ValidationStatus _currentStatus = ValidationStatus.Pending;
        public ValidationStatus CurrentStatus
        {
            get => _currentStatus;
            set
            {
                SetProperty(ref _currentStatus, value);
                OnValidationStatusChanged(value);
            }
        }

        #endregion

        #region 事件

        public event EventHandler ConfigChanged;
        public event EventHandler<ValidationStatusChangedEventArgs> ValidationStatusChanged;

        #endregion

        #region 方法

        public virtual void Initialize(string name)
        {
            ValidationName = name;
            LastRunTime = DateTime.MinValue;
            ValidationResult = string.Empty;
            CurrentStatus = ValidationStatus.Pending;
        }

        public abstract void LoadFromConfigData(ValidationItemData data);
        public abstract ValidationItemData ToConfigData();

        protected virtual void OnConfigChanged()
        {
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnValidationStatusChanged(ValidationStatus status)
        {
            ValidationStatusChanged?.Invoke(this, new ValidationStatusChangedEventArgs(status));
        }

        #endregion
    }
}
