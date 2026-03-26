#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseROI
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.ROIs
* 文 件 名:       BaseROI.cs
* 创建时间:       2022/2/16 13:25:55
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      37a857dc-7fe8-4b5d-881c-a20bc4ed98c5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/16 13:25:55
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Enums;
using Luster.ThreeD.Algorithm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// ROI 父类
    /// </summary>
    public class LBaseROI : BaseGeometry
    {
        /// <summary>
        /// 是否包含或排除
        /// </summary>
        public bool IsInside { get; set; }

        /// <summary>
        /// 表面距离
        /// </summary>
        public double SurfDistance { get; set; }

        /// <summary>
        /// 坐标系
        /// </summary>
        public ICoord Coord { get; set; }

        /// <summary>
        /// 获取集合中心点
        /// </summary>
        public virtual VVector Center { get; set; }

        public LBaseROI() : base(IntPtr.Zero)
        {
        }

        public LBaseROI(IntPtr ptr) : base(ptr)
        {
        }
    }
}