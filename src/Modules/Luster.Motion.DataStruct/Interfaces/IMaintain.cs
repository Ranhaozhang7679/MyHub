#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IPeriod
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Interfaces
* 文 件 名:       IPeriod.cs
* 创建时间:       2022/6/9 17:04:19
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a569ac62-1169-4594-b0a9-7a54a9ced2cd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/9 17:04:19
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
    /// 设备保养
    /// </summary>
    public interface IMaintain
    {
        Guid ID { get; set; }
        /// <summary>
        /// 设备的名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 最大生命值
        /// </summary>
        double MaxHP { get; set; }

        /// <summary>
        /// 已经使用的次数
        /// </summary>
        double UsedHP { get; set; }

        /// <summary>
        /// 保养阈值
        /// </summary>
        double MaintainHP { get; set; }

        /// <summary>
        /// 当前阈值
        /// </summary>
        double CurrentHP { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        string Unit { get; }

        /// <summary>
        /// 获取使用比例 10%
        /// </summary>
        /// <param name="perStr"></param>
        /// <returns></returns>
        double GetPercent();

        /// <summary>
        /// 显示真实值 10/1000
        /// </summary>
        /// <returns></returns>
        string GetActual();

        /// <summary>
        /// 获取保养提示
        /// </summary>
        /// <returns></returns>
        string GetMaintainTips();
    }
}