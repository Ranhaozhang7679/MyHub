#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ISegment
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.Interfaces
* 文 件 名:       ISegment.cs
* 创建时间:       2021/12/21 8:52:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      5162f020-33be-4c6c-8ea2-6166aa2384f6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/21 8:52:00
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Interfaces
{
    /// <summary>
    /// 感兴趣区域
    /// </summary>
    public interface IGeometry : IDisposable, IEmptyObj
    {
        /// <summary>
        /// ROI 指针
        /// </summary>
        IntPtr LPtr { get; }
    }

    /// <summary>
    /// 几何对象赋值
    /// </summary>
    public interface ISetGeometry
    {
        void SetGeometry(IGeometry geometry);
    }
}