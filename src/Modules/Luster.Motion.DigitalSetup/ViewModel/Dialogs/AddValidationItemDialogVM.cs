using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel.Dialogs
{
    /// <summary>
    /// 添加验证项对话框 ViewModel
    /// </summary>
    public class AddValidationItemDialogVM : BindableBase, IDialogAware
    {
        #region 属性

        private string _itemName;
        /// <summary>
        /// 验证项名称
        /// </summary>
        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        private ValidationType _selectedValidationType;
        /// <summary>
        /// 选中的验证类型
        /// </summary>
        public ValidationType SelectedValidationType
        {
            get => _selectedValidationType;
            set => SetProperty(ref _selectedValidationType, value);
        }

        /// <summary>
        /// 验证类型列表
        /// </summary>
        public List<ValidationType> ValidationTypes { get; }

        public string Title => "添加验证项";

        public event Action<IDialogResult> RequestClose;

        #endregion

        #region 命令

        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #endregion

        public AddValidationItemDialogVM()
        {
            // 初始化验证类型列表
            ValidationTypes = new List<ValidationType>
            {
                ValidationType.Common,
                ValidationType.LoadCellCalibration,
                ValidationType.CCDCalibration,
                ValidationType.VisionStaticData,
                ValidationType.GantryDynamicRepeatibilityData,
                ValidationType.PressPaperResults,
                ValidationType.VisionFlowImages,
                ValidationType.FoolProofingImages,
                ValidationType.VacuumCalibration,
                ValidationType.CPK,
                ValidationType.KeyParameters,
                ValidationType.ScannerCheck
            };

            // 默认选择第一个
            SelectedValidationType = ValidationType.Common;

            ConfirmCommand = new DelegateCommand(OnConfirm);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private void OnConfirm()
        {
            if (!string.IsNullOrWhiteSpace(ItemName))
            {
                var parameters = new DialogParameters
                {
                    { "ItemName", ItemName },
                    { "ValidationType", SelectedValidationType }
                };
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
            }
        }

        private void OnCancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Title"))
            {
                // 可以从参数获取标题等
            }
        }
    }
}
