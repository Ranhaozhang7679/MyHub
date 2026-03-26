#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AppConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Controls
* 文 件 名:       AppConfig.cs
* 创建时间:       2022/4/22 10:11:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      df445b0b-7883-4563-8d22-1b551abb334f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/22 10:11:43
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem
{
    public class AppConfig
    {
        /// <summary>
        /// 默认语言
        /// </summary>
        public static string Lang = "zh-cn";

        /// <summary>
        /// 皮肤
        /// </summary>
        public static SkinType Skin { get; set; }
    }
}