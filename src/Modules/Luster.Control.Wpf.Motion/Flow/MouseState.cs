#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MouseState
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       MouseState.cs
* 创建时间:       2022/6/6 8:40:27
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      286b0187-b4d9-4b00-b067-a2d466070033
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/6 8:40:27
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Control.Wpf.Motion.Flow
{
    /// <summary>
    /// 鼠标交互枚举
    /// </summary>
    public enum MouseState
    {
        /// <summary>
        /// 默认
        /// </summary>
        None,

        /// <summary>
        /// 选择
        /// </summary>
        Selected,       // 选择,支持多选
        
        /// <summary>
        /// 绘制矩形框
        /// </summary>
        DragRect,       // 画矩形框
        
        /// <summary>
        /// 移动
        /// </summary>
        Move,

        /// <summary>
        /// 绘制连接线
        /// </summary>
        CreateLink
    }
}