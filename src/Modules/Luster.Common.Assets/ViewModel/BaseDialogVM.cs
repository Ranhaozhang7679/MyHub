#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseDialog
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.ViewModel.Dialog
* 文 件 名:       BaseDialog.cs
* 创建时间:       2021/12/20 15:18:42
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      c7d4db5f-ccfe-4f75-bc39-1cbe41b24729
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/20 15:18:42
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Assets.ViewModel
{
    /// <summary>
    /// 弹窗
    /// </summary>
    public class BaseDialogVM : BindableBase, IDialogAware, INotifyDataErrorInfo
    {
        /// <summary>
        /// 标题信息 
        /// </summary>
        private string _title = "Window";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        /// <summary>
        /// 确定文本
        /// </summary>
        private string _okText;
        public string OKText { get => _okText; set => SetProperty(ref _okText, value); }

        /// <summary>
        /// 否文本
        /// </summary>
        private string _noText;
        public string NoText { get => _noText; set => SetProperty(ref _noText, value); }

        /// <summary>
        /// 取消文本
        /// </summary>
        private string _cancelText;
        public string CancelText { get => _cancelText; set => SetProperty(ref _cancelText, value); }

        /// <summary>
        /// 取消文本
        /// </summary>
        private string _addText;
        public string AddText { get => _addText; set => SetProperty(ref _addText, value); }


        /// <summary>
        /// 关闭对话框
        /// </summary>
        private DelegateCommand<string> _closeCommand;

        public DelegateCommand<string> CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<string>((parameter) =>
          {
              CloseDialog(parameter);
          }));


        /// <summary>
        /// 关闭请求
        /// </summary>
        public event Action<IDialogResult> RequestClose;

        /// <summary>
        /// 对创建进行关闭
        /// </summary>
        /// <param name="parameter"></param>
        protected virtual void CloseDialog(string parameter)
        {
            IDialogResult dialogResult;
            if (parameter?.ToLower() == "ok")
            {
                ValidateAll();

                // 如果存在异常，则不允许关闭
                if (HasErrors)
                {
                    return;
                }

                dialogResult = new DialogResult(ButtonResult.OK);
                Ok(dialogResult);
                RaiseRequestClose(dialogResult);
            }
            else if (parameter?.ToLower() == "cancel")
            {
                dialogResult = new DialogResult(ButtonResult.Cancel);
                Cancel(dialogResult);
                RaiseRequestClose(dialogResult);

            }
            else if (parameter?.ToLower() == "no")
            {
                dialogResult = new DialogResult(ButtonResult.No);
                No(dialogResult);
                RaiseRequestClose(dialogResult);
            }
            else if (parameter?.ToLower() == "add")
            {
                dialogResult = new DialogResult(ButtonResult.OK);
                Add(dialogResult);
            }
            else
            {

                dialogResult = new DialogResult(ButtonResult.None);
                RaiseRequestClose(dialogResult);

            }

        }

        /// <summary>
        /// 点击OK
        /// </summary>
        /// <param name="result"></param>
        protected virtual void Ok(IDialogResult result)
        {

        }

        /// <summary>
        /// 点击No
        /// </summary>
        /// <param name="result"></param>
        protected virtual void No(IDialogResult result)
        {

        }

        /// <summary>
        /// 点击No
        /// </summary>
        /// <param name="result"></param>
        protected virtual void Cancel(IDialogResult result)
        {

        }

     
        protected virtual void Add(IDialogResult result)
        {

        }

        /// <summary>
        /// 对话框该笔
        /// </summary>
        /// <param name="dialogResult"></param>
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public virtual bool CanCloseDialog() => true;

        /// <summary>
        /// 对话框关闭后处理
        /// </summary>
        public virtual void OnDialogClosed()
        {
        }

        /// <summary>
        /// 窗口打开
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            OKText = L("Ok");
            NoText = L("No");
            CancelText = L("Cancel");
            AddText = L("Add");

            if (parameters.TryGetValue<string>("Title", out var pTitle))
            {
                Title = pTitle;
            }
            else
            {
                Title = "窗口";
            }
        }

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual string L(string key)
        {
            var str = Luster.Common.Assets.Langs.LangProvider.GetLang(key);
            if (string.IsNullOrEmpty(str))
            {
                return key;
            }

            return str;
        }

        #region 表达错误验证功能

        /// <summary>
        /// 对所有属性进行验证
        /// </summary>
        protected void ValidateAll()
        {
            var properties = GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var item in properties)
            {
                var attrs = item.GetCustomAttributes();
                if (!attrs.Any()) continue;

                if (item.GetSetMethod() == null) continue;

                ValidateProp(item);
            }
        }

        /// <summary>
        /// 向设置属性添加表单验证
        /// </summary>
        /// <param name="args"></param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            // 动态检测
            ValidateProp(GetType().GetProperty(args.PropertyName));
        }

        /// <summary>
        /// 对属性进行检测
        /// </summary>
        /// <param name="prop"></param>
        private void ValidateProp(PropertyInfo prop)
        {
            if (prop != null)
            {
                string propName = prop.Name;
                List<string> errors = new List<string>();
                ErrorContainer.ClearErrors(propName);
                foreach (var attr in prop.GetCustomAttributes())
                {
                    if (attr is not ValidationAttribute validAttr) continue;
                    var val = prop.GetValue(this, null);

                    string errMsg = "";
                    // 1.必填
                    if (attr is RequiredAttribute vAttr)
                    {
                        if (!vAttr.IsValid(val))
                        {
                            errMsg = vAttr.ErrorMessage;
                            if (string.IsNullOrEmpty(errMsg))
                            {
                                errMsg = $"参数不能为空!";
                            }

                            errors.Add(errMsg);
                        }
                    }
                    else if (attr is RangeAttribute rangeAttr)
                    {
                        if (!rangeAttr.IsValid(val))
                        {
                            errMsg = rangeAttr.ErrorMessage;
                            if (string.IsNullOrEmpty(errMsg))
                            {
                                errMsg = $"参数取值必须在 {rangeAttr.Minimum}~{rangeAttr.Maximum}";
                            }

                            errors.Add(errMsg);
                        }
                    }
                    else if (attr is StringLengthAttribute strLenAttr)
                    {
                        if (!strLenAttr.IsValid(val))
                        {
                            errMsg = strLenAttr.ErrorMessage;
                            if (string.IsNullOrEmpty(errMsg))
                            {
                                errMsg = $"参数长度必须在 {strLenAttr.MinimumLength}~{strLenAttr.MaximumLength}";
                            }

                            errors.Add(errMsg);
                        }
                    }
                    else
                    {
                        if (!validAttr.IsValid(val))
                        {
                            errMsg = validAttr.ErrorMessage;
                            if (string.IsNullOrEmpty(errMsg))
                            {
                                errMsg = $"参数异常，未填写说明";
                            }

                            errors.Add(errMsg);
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    ErrorContainer.SetErrors(propName, errors);
                }
            }
        }

        /// <summary>
        /// 错误
        /// </summary>
        private ErrorsContainer<string> _errorsContainer;
        protected ErrorsContainer<string> ErrorContainer
        {
            get
            {
                if (_errorsContainer == null)
                {
                    _errorsContainer = new ErrorsContainer<string>((prop) =>
                      {
                          ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
                      });
                }

                return _errorsContainer;
            }
        }

        private void ValidateCommon(string propName, Action<object> pAction)
        {
            var prop = this.GetType().GetProperty(propName);
            if (prop != null)
            {
                ErrorContainer.ClearErrors(propName);
                object val = prop.GetValue(this, null);
                pAction(val);
            }
        }

        /// <summary>
        /// 获取所有错误
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable GetErrors(string propertyName)
        {
            return ErrorContainer.GetErrors(propertyName);
        }


        /// <summary>
        /// 错误消失
        /// </summary>
        public bool HasErrors => ErrorContainer.HasErrors;

        /// <summary>
        /// 事件通知
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        #endregion
    }
}