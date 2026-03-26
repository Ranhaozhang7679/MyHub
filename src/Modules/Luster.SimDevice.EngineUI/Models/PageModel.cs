#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NavigationModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       NavigationModel.cs
* 创建时间:       2022/4/18 9:20:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      38745388-60e9-4a03-b6e9-0b8c2a6d9a04
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 9:20:39
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    /// <summary>
    /// 页面切换
    /// </summary>
    public class PageModel 
    {
        /// <summary>
        /// 对应的Icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设备分组
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 对应的视图名称
        /// </summary>
        public string PageView { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// 当前VM
        /// </summary>
        public object CurrentVM { get; set; }

        /// <summary>
        /// 当前项对有的数量
        /// </summary>
        public int ItemNum { get; set; }
    }
}