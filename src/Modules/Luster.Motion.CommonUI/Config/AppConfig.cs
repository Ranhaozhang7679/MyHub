#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AppConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Config
* 文 件 名:       AppConfig
* 创建时间:       2021/11/3 14:11:31
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      8bf064d3-f5cc-461c-a0ff-1685abda44a2
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/3 14:11:31
* 修 改 人:		  luster
************************************************************************************/

#endregion

using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI
{
    public class AppConfig
    {
        public static string Lang = "zh-cn";

        public static SkinType Skin { get; set; }

        /// <summary>
        /// 运控
        /// </summary>
        public static string System = "HoloMotion";
    }
}