using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel.Dialogs
{
    /// <summary>
    /// 文本输入对话框 ViewModel
    /// </summary>
    public class TextInputDialogVM : BindableBase, IDialogAware
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _inputText;
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public event Action<IDialogResult> RequestClose;

        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public TextInputDialogVM()
        {
            ConfirmCommand = new DelegateCommand(OnConfirm);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("Title") ?? "输入";
            InputText = parameters.GetValue<string>("DefaultValue") ?? "";
        }

        private void OnConfirm()
        {
            var text = InputText?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            var parameters = new DialogParameters { { "InputText", text } };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
        }

        private void OnCancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }
    }
}