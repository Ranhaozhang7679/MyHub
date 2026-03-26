#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OperationTipItem
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       OperationTipItem.cs
* 创建时间:       2022/12/1 15:56:28
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      b384f447-35be-41ac-9f8f-60a70458b458
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/1 15:56:28
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Models
{
    public class OperationTipItem : BindableBase
    {
        public string GUID { get; set; }

        /// <summary>
        /// 操作提示
        /// </summary>
        private string _tip;

        public string Tip
        {
            get { return _tip; }
            set
            {
                if (_tip != value)
                {
                    SetProperty(ref _tip, value);
                    Tag.Tip = value;
                }
            }
        }

        /// <summary>
        /// 操纵类型
        /// </summary>
        private string _operation;
        public string Operation
        {
            get { return _operation; }
            set
            {
                if (_operation != value)
                {
                    SetProperty(ref _operation, value);
                }
            }
        }

        public OperationTip Tag { get; set; }

        public OperationTipItem()
        {
        }

        public OperationTipItem(OperationTip model)
        {
            Tag = model;
            GUID = model.GUID;
            Tip = model.Tip;
            Operation = model.Operation.ToDescriptionOrString();
        }

    }
}
