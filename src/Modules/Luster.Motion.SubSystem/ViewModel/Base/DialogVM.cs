using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel.Base
{
    public class DialogVM : BaseVM, IDialogAware, INotifyDataErrorInfo
    {
        /// <summary>
        /// 错误容器
        /// </summary>
        protected ErrorsContainer<string> _errorsContainer;

        //protected DialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        //{
        //    _errorsContainer = new ErrorsContainer<string>(OnErrorsChanged);
        //}
        protected DialogVM()
        {
            _errorsContainer = new ErrorsContainer<string>(OnErrorsChanged);
        }

        /// <summary>
        /// Dialog 标题
        /// </summary>
        private string _title = "Window";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        private DelegateCommand<string> _closeCommand;

        public DelegateCommand<string> CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<string>((parameter) =>
        {
            CloseDialog(parameter);
        }));


        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "ok")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "cancel")
                result = ButtonResult.Cancel;

            RequestClose?.Invoke(new DialogResult(result));
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var pTitle))
            {
                Title = pTitle;
            }
            else
            {
                Title = "窗口";
            }
        }

        #region 数据校验
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable GetErrors(string propertyName)
        {
            return _errorsContainer.GetErrors(propertyName);
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors => _errorsContainer.HasErrors;

        protected void OnErrorsChanged(string propertyName)
        {

        }
        #endregion
    }
}
