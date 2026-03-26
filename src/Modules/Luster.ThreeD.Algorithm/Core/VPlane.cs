#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VPlane
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Models
* 文 件 名:       VPlane.cs
* 创建时间:       2021/12/17 11:38:32
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      dfe72fec-53df-4351-ae0c-3ed5cd5c8a68
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 11:38:32
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
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 面
    /// </summary>
    public class VPlane : BaseGeometry, IDirection
    {
        public VVector Point1 { get; set; }

        public VVector Point2 { get; set; }

        public VVector Point3 { get; set; }

        /// <summary>
        /// 中心点
        /// </summary>
        public VVector Center { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        [OutProperty("方向")]
        public VDirection Direction { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; } = 1;

        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; } = 1;

        /// <summary>
        /// z方向Z
        /// </summary>
        public double ZVal { get; set; }

        public VPlane() : base()
        {
        }

        public VPlane(IntPtr ptr) : base(ptr)
        {
            NativeAPI.GetPlaneNormal(ptr, out var planeNormal, out var zVal, out _);
            ZVal = zVal;
            Direction = new VDirection(planeNormal);

            NativeAPI.GetPlane3Points(ptr, out var p1, out var p2, out var p3, out _);
            Center = new VVector((p1.X + p2.X + p3.X) / 3, (p1.Y + p2.Y + p3.Y) / 3, (p1.Z + p2.Z + p3.Z) / 3);
        }


        public void SetPoint3(VVector p1, VVector p2, VVector p3)
        {
            Center = new VVector((p1.X + p2.X + p3.X) / 3, (p1.Y + p2.Y + p3.Y) / 3, (p1.Z + p2.Z + p3.Z) / 3);

            // 欧式距离
            
            var firstXY = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2));
            var firstXZ = Math.Sqrt(Math.Pow(p3.X - p1.X, 2) + Math.Pow(p3.Y - p1.Y, 2) + Math.Pow(p3.Z - p1.Z, 2));
            var firstYZ = Math.Sqrt(Math.Pow(p3.X - p2.X, 2) + Math.Pow(p3.Y - p2.Y, 2) + Math.Pow(p3.Z - p2.Z, 2));

            List<double> dis = new List<double>() { firstXY, firstXZ, firstYZ };
            dis.Sort();

            Width = dis[1] * 1.2;
            Height = dis[2] * 1.2;
        }

        /// <summary>
        /// 通过点和方向构建面
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        public VPlane(VVector pos, VDirection dir, double width = 1, double height = 1)
        {
            Center = pos;
            Direction = dir;
            Width = width;
            Height = height;
            NativeAPI.GetPlaneByPosNormal(pos.ToPoint(), dir.ToVector(), out lptr, out _);
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        protected override void DisposeObj()
        {
            base.DisposeObj();
            NativeAPI.DeleteObj(LPtr, NativeAPI.ClassType.Plane);
        }

        #region 面绘制

        public override void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;

            if (Center == null)
            {
                Center = new VVector(0, 0, 0);
            }

            i3D.AddPlaneActor(ID, this);
            i3D.AddVectorActor(ID, Center, Direction);
        }

        #endregion
    }
}