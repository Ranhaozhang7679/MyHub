#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StringMergeVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.ViewModel
* 文 件 名:       StringMergeVM.cs
* 创建时间:       2022/9/8 11:34:23
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      09a41018-e035-400b-9993-4c9681eda499
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 11:34:23
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Luster.Common.Assets.Tools;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Common.Assets.ViewModel
{
    public class StringExDialogVM : TextEditorVM
    {
        public StringExDialogVM() : base()
        {

        }

        /// <summary>
        /// 预览命令
        /// </summary>
        private DelegateCommand _previewCommand;
        public DelegateCommand PreviewCommand => _previewCommand ?? (_previewCommand = new DelegateCommand(() =>
        {
            string text = DocumentText.Text.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var lString = BuildStringEx();
                if (lString != null)
                {
                    CheckVarValidate(lString.Variables);
                    PreviewText = lString.GetString(Parameter.Owner);
                }
            }
        }));


        private LStringEx BuildStringEx()
        {
            LStringEx lString = new LStringEx()
            {
                StringEx = DocumentText.Text.Trim(),
            };

            lString.Variables = GetVariables(lString.StringEx);

            return lString;
        }

        /// <summary>
        /// 点击确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add("StringEx", BuildStringEx());
        }

        /// <summary>
        /// 当前对象
        /// </summary>
        private LStringEx lString;

        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<ParameterAttribute>("Parameter", out var parameter))
            {
                Parameter = parameter;
                lString = Parameter.Value as LStringEx;
                if (lString != null)
                {
                    DocumentText.Text = lString.StringEx;
                }
            }
            else
            {
                throw new KeyNotFoundException("Parameter");
            }
        }
    }
}