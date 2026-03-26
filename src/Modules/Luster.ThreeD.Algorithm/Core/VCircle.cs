#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCircle
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VCircle.cs
* 创建时间:       2021/12/17 11:43:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7642fe08-0f22-4846-9cfa-10bce1947a7b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 11:43:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
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
    /// 圆
    /// </summary>
    public class VCircle : BaseGeometry
    {
        /// <summary>
        /// 圆心[OutProperty]
        /// </summary>
        [OutProperty("圆心")]
        public VVector Center
        {
            get; set;
        }

        /// <summary>
        /// 方向
        /// </summary>
        [OutProperty("方向")]
        public VDirection Direction
        {
            get; set;
        }

        /// <summary>
        /// 半径
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VCircle() : base(IntPtr.Zero)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ptr">指针</param>
        public VCircle(IntPtr ptr) : base(ptr)
        {
            NativeAPI.GetPropertyByCircle(ptr, out var center, out var dir, out var r);
            Center = new VVector(center);
            Direction = new VDirection(dir);
        }

        public VCircle(VVector vCenter, VDirection dir, double radius)
        {
            NativeAPI.GenCircleByData(vCenter.ToPoint(), dir.ToVector(), radius, out lptr);
            Center = vCenter;
            Direction = dir;
        }

        public override string ToString()
        {
            return "Circle";
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        protected override void DisposeObj()
        {
            base.DisposeObj();
            NativeAPI.DeleteObj(LPtr, NativeAPI.ClassType.Circle);
        }

        #region 圆绘制
        public override void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddCircleActor(ID, LPtr);
        }
        #endregion

        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Circle");
            if (!BuildByCpp)
            {
                xRoot.SetAttributeValue("Radius", Radius);
                xRoot.Add(Center.ExportXml());
                xRoot.Add(Direction.ExportXml());
            }

            return xRoot;
        }

        public override void ParserXml(XElement xElement)
        {
            int count = xElement.Elements().Count();
            xElement.GetAttribute("Radius", (r) =>
            {
                Radius = double.Parse(r);
            });

            for (int i = 0; i < 2; i++)
            {
                XElement xItem = xElement.Elements().ElementAt(i);

                switch (i)
                {
                    case 0:
                        Center.ParserXml(xItem);
                        break;
                    case 1:
                        Direction.ParserXml(xItem);
                        break;
                }
            }
        }
    }
}
