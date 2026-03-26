#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IMeasure
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       IMeasure.cs
* 创建时间:       2022/3/2 9:17:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      87da4949-6874-4c93-bb20-8cdcb9a53ca0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/2 9:17:59
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Interfaces
{
    /// <summary>
    /// 测量对象
    /// </summary>
    public interface ISetMeasure
    {
        /// <summary>
        /// 测量对象
        /// </summary>
        LTolerance Measure { get; set; }

        /// <summary>
        /// 测量接口
        /// </summary>
        /// <param name="standard">标准值</param>
        /// <param name="range">公差范围</param>
        /// <param name="compensate">补偿值</param>
        void SetMeasure(double standard, LRange range, double compensate);
    }
}