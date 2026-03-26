#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlowButtonArgs
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       FlowButtonArgs.cs
* 创建时间:       2022/5/26 8:40:42
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4c294754-78de-4756-b886-9ac8a5fa754e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/26 8:40:42
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Luster.Control.Wpf.Motion.Flow
{
    /// <summary>
    /// 鼠标点击参数
    /// </summary>
    public class FlowItemDownArgs : RoutedEventArgs
    {
        public FlowItemDownArgs(RoutedEvent routedEvent, IFlowRender item, MouseButtonEventArgs e) : base(routedEvent)
        {
            SelectItems = new List<IFlowRender>();
            FlowItem = item;
            MouseDownArgs = e;
        }

        public MouseButtonEventArgs MouseDownArgs { get; set; }

        /// <summary>
        /// 当前选中项
        /// </summary>
        public IFlowRender FlowItem { get; set; }

        /// <summary>
        /// 当前选中的项目
        /// </summary>
        public List<IFlowRender> SelectItems { get; set; }
    }


    /// <summary>
    /// 鼠标点击参数
    /// </summary>
    public class FlowLineDownArgs : RoutedEventArgs
    {
        public FlowLineDownArgs(RoutedEvent routedEvent, FlowLine line, MouseButtonEventArgs e) : base(routedEvent)
        {
            Fline = line;
            MouseDownArgs = e;
        }

        public MouseButtonEventArgs MouseDownArgs { get; set; }

        /// <summary>
        /// 当前选中项
        /// </summary>
        public FlowLine Fline { get; set; }
    }

    /// <summary>
    /// Item 拖拽
    /// </summary>
    public class ItemDropArgs : RoutedEventArgs
    {
        /// <summary>
        /// 所处行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 所处列
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 所处索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 数据对象
        /// </summary>
        public DragEventArgs Args { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="routedEvent"></param>
        /// <param name=""></param>
        /// <param name="e"></param>
        public ItemDropArgs(RoutedEvent routedEvent, int row, int col, int index, DragEventArgs args) : base(routedEvent)
        {
            Row = row;
            Column = col;
            Args = args;
            Index = index;
        }
    }
}