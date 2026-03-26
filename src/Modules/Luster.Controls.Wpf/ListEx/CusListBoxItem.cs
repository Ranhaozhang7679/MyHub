#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DisplayElementModel
* 机器名称:       L05014-NB
* 命名空间:       Luster.SubSystem.ThreeD.Model
* 文 件 名:       DisplayElementModel.cs
* 创建时间:       2022/6/7 10:15:01
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      b1bab3e4-8edf-4092-93f8-281e2670bf54
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/7 10:15:01
* 修 改 人:		  L05014
************************************************************************************/

#endregion
using Luster.Controls.Wpf.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Controls.Wpf.ListEx
{
    public class CusListBoxItem:ListBoxItem
    {

        /// <summary>
        /// Identifies the <see cref="IsDropTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.Register(
            nameof(IsDropTarget),
            typeof(bool),
            typeof(CusListBoxItem),
            new UIPropertyMetadata(false));


        /// <summary>
        /// Gets or sets a value indicating whether this instance should preview as a drop target.
        /// </summary>
        public bool IsDropTarget
        {
            get
            {
                return (bool)this.GetValue(IsDropTargetProperty);
            }

            set
            {
                this.SetValue(IsDropTargetProperty, value);
            }
        }
        /// <summary>
        /// Gets the parent <see cref="ReportListbox" />.
        /// </summary>
        public CusListBox ParentReportListBox
        {
            get
            {
                return (CusListBox)ItemsControl.ItemsControlFromItemContainer(this);
            }
        }
    }
}
