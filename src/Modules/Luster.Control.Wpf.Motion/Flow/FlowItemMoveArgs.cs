#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlowItemMoveArgs
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       FlowItemMoveArgs.cs
* 创建时间:       2022/5/26 8:43:17
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      642b3e5a-b68e-4efb-a7cb-f564ec05be81
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/26 8:43:17
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Luster.Control.Wpf.Motion.Flow
{
    public class FlowItemHoverArgs : RoutedEventArgs
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="routedEvent"></param>
        public FlowItemHoverArgs(RoutedEvent routedEvent) : base(routedEvent)
        {
        }

        /// <summary>
        /// 要移动的索引
        /// </summary>
        public IFlowRender HoverItem { get; set; }

        /// <summary>
        /// 移动后索引
        /// </summary>
        public int MoveIndex { get; set; }

        /// <summary>
        /// 如果为true，代表从前移动到后面
        /// </summary>
        public bool BeforeToAfter { get; set; }
    }
}