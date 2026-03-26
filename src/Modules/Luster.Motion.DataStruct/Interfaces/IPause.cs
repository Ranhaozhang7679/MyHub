#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IPauseRecovery
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Interfaces
* 文 件 名:       IPauseRecovery.cs
* 创建时间:       2022/7/28 20:06:42
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ae7290a8-2894-4181-98f9-2084c9890393
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/28 20:06:42
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 设备暂停和恢复接口
    /// </summary>
    public interface IPause
    {
        /// <summary>
        /// 线程中断
        /// </summary>
        string Name { get; set;}
        
        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();
    }
}