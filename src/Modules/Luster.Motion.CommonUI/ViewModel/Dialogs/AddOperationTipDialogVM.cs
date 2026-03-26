#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AddOperationTipDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.ViewModel.Dialogs
* 文 件 名:       AddOperationTipDialogVM.cs
* 创建时间:       2022/12/1 14:53:16
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      7bbf787c-a419-4734-bcbd-f7afe54ce22b
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/1 14:53:16
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class AddOperationTipDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 操作提示信息
        /// </summary>
        private string _tip;
        [Required]
        public string Tip
        {
            get => _tip;
            set => SetProperty(ref _tip, value);
        }

        /// <summary>
        /// 操作类型
        /// </summary>
        private SystemOperation _operationType;
        [Required]
        public SystemOperation OperationType
        {
            get => _operationType;
            set => SetProperty(ref _operationType, value);
        }

        private OperationTip _operationTip;

        public List<KeyValue> KeyValues { get; }

        public AddOperationTipDialogVM()
        {
            KeyValues = typeof(SystemOperation).EnumToDataSource();
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<OperationTip>("Model", out _operationTip))
            {
                if (_operationTip != null)
                {
                    Tip = _operationTip.Tip;
                    OperationType = _operationTip.Operation;
                }
            }
        }

        protected override void Ok(IDialogResult result)
        {
            if (_operationTip != null)
            {
                _operationTip.Tip = Tip;
                _operationTip.Operation = OperationType;
            }
            else
            {
                _operationTip = new OperationTip()
                {
                    GUID = Guid.NewGuid().ToString(),
                    Operation = OperationType,
                    Tip = Tip
                };
            }
            result.Parameters.Add("Model", _operationTip);
        }
    }
}
