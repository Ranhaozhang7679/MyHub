#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       InfoInputDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.ViewModel
* 文 件 名:       InfoInputDialogVM.cs
* 创建时间:       2026/03/21
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      da1abca6-8e9b-4d56-b634-45e1b00e0750
* 创建年份:       2026
************************************************************************************/

#endregion

using Prism.Services.Dialogs;
using System.ComponentModel.DataAnnotations;

namespace Luster.Common.Assets.ViewModel
{
    /// <summary>
    /// 信息输入对话框ViewModel，无字符长度限制
    /// </summary>
    public class InfoInputDialogVM : BaseDialogVM
    {
        /// <summary>
        /// Dialog的文本信息
        /// </summary>
        private string _text;
        [Required]
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        /// <summary>
        /// 确定按钮文本
        /// </summary>
        private string _btnOkText;
        public string BtnOKText
        {
            get { return _btnOkText; }
            set { SetProperty(ref _btnOkText, value); }
        }

        /// <summary>
        /// 取消按钮文本
        /// </summary>
        private string _btnCancelText;
        public string BtnCancelText
        {
            get { return _btnCancelText; }
            set { SetProperty(ref _btnCancelText, value); }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("Text", Text);
        }

        /// <summary>
        /// 打开时获取参数
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            BtnCancelText = L("Cancel");
            BtnOKText = L("OK");

            if (parameters.TryGetValue<string>("Text", out var text))
            {
                Text = text;
            }
            else
            {
                Text = string.Empty;
            }
        }
    }
}
