#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IHomeToZero
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Interfaces
* 文 件 名:       IHomeToZero.cs
* 创建时间:       2022/6/20 15:05:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c7e9f52f-7bb0-4b40-afd0-0b2e948c32b9
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/20 15:05:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 回零接口
    /// 重试、取消
    /// </summary>
    public interface IMonitor
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 报警信息
        /// </summary>
        string AlarmCode { get; set; }

        /// <summary>
        /// 是否启用监控
        /// </summary>
        bool IsMonitor { get; set; }
        /// <summary>
        /// 开启监控
        /// </summary>
        void StartMonitor();

        /// <summary>
        /// 休眠时间
        /// </summary>
        int Frequency { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        string ErrorMessage { get; set; }
    }
}