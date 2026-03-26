#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProjectInfo
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       ProjectInfo.cs
* 创建时间:       2022/4/26 9:37:27
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b9a2f130-1791-4c3c-9723-511483de064f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/26 9:37:27
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    /// <summary>
    /// 工程信息
    /// </summary>
    public class ProjectInfo
    {
        /// <summary>
        /// 工程路径
        /// </summary>
        public string SolutionPath { get; set; }

        /// <summary>
        /// 工程名称
        /// </summary>
        public string ProjectName { get; set; }
    }
}