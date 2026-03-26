#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCone
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VCone.cs
* 创建时间:       2021/12/17 11:43:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3b21926d-711f-4932-a81d-331a6ba9f705
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 11:43:29
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 圆锥
    /// </summary>
    public class VCone : BaseGeometry
    {
        /// <summary>
        /// 顶点
        /// </summary>
        [OutProperty("顶点")]
        public VVector Vertex { get; set; }

        /// <summary>
        /// 轴线
        /// </summary>
        [OutProperty("轴线")]
        public VLine AxisLine
        {
            get
            {
                NativeAPI.GetLineSegmentByConicalShaft(LPtr, out var linePtr, out _);

                return new VLine(linePtr);
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VCone() : base(IntPtr.Zero) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ptr"></param>
        public VCone(IntPtr ptr) : base(ptr)
        {
            NativeAPI.GetConeVertex(ptr, out var vertex, out var errMsg);
            Vertex = new VVector(vertex);
        }

        /// <summary>
        /// 渲染对象
        /// </summary>
        /// <param name="interactor"></param>
        public override void AddActor(IInteractor interactor)
        {
            I3DInteractor i3DInteractor = interactor as I3DInteractor;
            if (i3DInteractor != null)
            {
                i3DInteractor.AddConeActor(ID, LPtr);
            }
        }
    }
}
