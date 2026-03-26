#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TextDialog
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.ViewModel.Dialog
* 文 件 名:       TextDialog.cs
* 创建时间:       2021/12/13 17:50:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      da1abca6-8e9b-4d56-b634-45e1b00e074f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/13 17:50:21
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Assets.ViewModel
{
    public class TextDialogVM : BaseDialogVM
    {
        /// <summary>
        /// Dialog的文本信息
        /// </summary>
        private string _text;
        [Required]
        [StringLength(15, MinimumLength = 2)]
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        /// <summary>
        /// Dialog的文本信息
        /// </summary>
        private string _btnOkText;
        public string BtnOKText
        {
            get { return _btnOkText; }
            set { SetProperty(ref _btnOkText, value); }
        }

        /// <summary>
        /// Dialog的文本信息
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
        /// 打开是获取对象
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
                Text = String.Empty;
            }
        }
    }
}