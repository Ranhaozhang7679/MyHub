#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IStop
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Interfaces
* 文 件 名:       IStop.cs
* 创建时间:       2022/5/18 18:30:08
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      adc91f48-f488-4949-bb37-3ac9031e221c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 18:30:08
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    /// <summary>
    /// 暂停模块
    /// </summary>
    public interface IPauseFunction
    {
        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();

        /// <summary>
        /// 是否需要暂停
        /// </summary>
        bool IsNeedPause { get; }

        /// <summary>
        /// 开始时间
        /// </summary>
        DateTime StartTime { get; }
    }
}