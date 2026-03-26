#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GlobalCommand
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Config
* 文 件 名:       GlobalCommand.cs
* 创建时间:       2022/4/26 9:51:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      185af303-f9ad-4223-8de8-c66937bd984d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/26 9:51:00
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.Config
{
    public static class GlobalCommands
    {
        /// <summary>
        /// 工程保存
        /// </summary>
        public static CompositeCommand SaveProjCommand = new CompositeCommand();

        /// <summary>
        /// 打开工程
        /// </summary>
        public static CompositeCommand LoadProjCommand = new CompositeCommand();
    }
}