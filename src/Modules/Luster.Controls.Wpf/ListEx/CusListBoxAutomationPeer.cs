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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace Luster.Controls.Wpf.ListEx
{
    public class CusListBoxAutomationPeer : ListBoxAutomationPeer
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CusListBoxAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public CusListBoxAutomationPeer(CusListBox owner)
            : base(owner)
        {
        }

        /// <summary>
        /// Gets the name of the <see cref="T:TreeListBox" /> that is associated with this <see cref="T:TreeListBoxAutomationPeer" />. 
        /// This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName" />.
        /// </summary>
        /// <returns>A string that contains "ListBox".</returns>
        protected override string GetClassNameCore()
        {
            return "CusListBox";
        }

        /// <summary>
        /// Gets the collection of child elements of the <see cref="T:System.Windows.Controls.ItemsControl" /> 
        /// that is associated with this <see cref="T:System.Windows.Automation.Peers.ItemsControlAutomationPeer" />. 
        /// This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetChildren" />.
        /// </summary>
        /// <returns>The collection of child elements.</returns>
        protected override List<AutomationPeer> GetChildrenCore()
        {
            return base.GetChildrenCore();
        }
    }
}
