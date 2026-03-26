#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlowDoubleClickArgs
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       FlowDoubleClickArgs.cs
* 创建时间:       2022/6/5 15:37:52
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d9a2a456-4741-4ddb-89b1-98584ecbdf88
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/5 15:37:52
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
    public class FlowDoubleClickArgs : RoutedEventArgs
    {
        public IFlowRender FlowItem { get; set; }

        public FlowDoubleClickArgs(RoutedEvent args, IFlowRender flowItem) : base(args)
        {
            FlowItem = flowItem;
        }
    }
}