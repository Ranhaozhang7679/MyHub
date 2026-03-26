#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TreeListBoxAutomationPeer
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.TreeListBox
* 文 件 名:       TreeListBoxAutomationPeer
* 创建时间:       2021/11/4 9:57:49
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      0d180679-d547-42cd-a019-fa3674169999
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/4 9:57:49
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace Luster.Controls.Wpf.TreeEx
{
    /// <summary>
    /// Exposes <see cref="T:TreeListBox"/> types to UI Automation.
    /// </summary>
    public class TreeListBoxAutomationPeer : ListBoxAutomationPeer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeListBoxAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public TreeListBoxAutomationPeer(TreeListBox owner)
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
            return "TreeListBox";
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
