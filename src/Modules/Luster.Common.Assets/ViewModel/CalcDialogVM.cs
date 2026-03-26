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
using Luster.Common.Tools;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
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
    public class CalcDialogVM : BaseDialogVM
    {
        /// <summary>
        /// 文档流
        /// </summary>
        private TextDocument _document;
        public TextDocument DocumentText
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }

        /// <summary>
        /// 标签列表
        /// </summary>
        private string _previewText;
        public string PreviewText
        {
            get => _previewText;
            set { SetProperty(ref _previewText, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public CalcDialogVM() : base()
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
                var exp = new ExpressTool(text);
                try
                {
                    var result = exp.Calulate(new Dictionary<string, object>()
                    {
                        {"Value",GetFunc.Invoke() }
                    });

                    PreviewText = result.ToString();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }));

        /// <summary>
        /// 获取函数功能
        /// </summary>
        private Func<double> GetFunc = null;

        /// <summary>
        /// 点击确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add("Express", DocumentText.Text.Trim());
        }

        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            // 对象初始化
            DocumentText = new TextDocument();

            if (parameters.TryGetValue<string>("Express", out var expStr))
            {
                DocumentText.Text = expStr;
            }

            if (parameters.TryGetValue<Func<double>>("Callback", out var func))
            {
                GetFunc = func;
            }
        }
    }
}