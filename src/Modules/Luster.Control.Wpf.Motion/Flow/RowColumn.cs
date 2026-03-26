#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RowColumn
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       RowColumn.cs
* 创建时间:       2022/6/30 14:37:07
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5ff291c0-25cc-4d72-a678-385a89377e79
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/30 14:37:07
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
    public struct RowColumn
    {
        /// <summary>
        /// 所处列
        /// </summary>
        public int Column { get; set; } 
        
        /// <summary>
        /// 所处行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 位置信息
        /// </summary>
        public Rect Rect { get; set; }

        /// <summary>
        /// 是否包含
        /// </summary>
        /// <param name="subRect"></param>
        /// <returns></returns>
        public bool Contains(Rect subRect)
        {
            return Rect.Contains(subRect);
        }
    }
}