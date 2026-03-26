#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCylinder
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VCylinder.cs
* 创建时间:       2021/12/17 11:43:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      9f72b510-a848-4734-9ac6-5f6782e17c00
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 11:43:15
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Enums;
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
    /// 圆柱
    /// </summary>
    public class VCylinder : LBaseROI
    {
        /// <summary>
        /// 起点
        /// </summary>
        [OutProperty("起点")]
        public VVector StartPoint { get; set; }

        /// <summary>
        /// 端点
        /// </summary>
        [OutProperty("端点")]
        public VVector EndPoint
        {
            get; set;
        }

        /// <summary>
        /// 中心点
        /// </summary>
        [OutProperty("中心点")]
        public override VVector Center
        {
            get
            {
                return new VVector((StartPoint.X + EndPoint.X) / 2, (StartPoint.Y + EndPoint.Y) / 2, (StartPoint.Z + EndPoint.Z) / 2);
            }
            set { }
        }

        [OutProperty("轴线")]
        public VLine AxisLine
        {
            get; set;
        }

        [OutProperty("半径")]
        public double Radius { get; set; }

        [OutProperty("高度")]
        public double Height { get; set; }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public VCylinder() : base()
        {
            StartPoint = new VVector(0, 0, 0);
            EndPoint = new VVector(0, 0, 0);
            Radius = 1;
            Height = 1;
        }

        public VCylinder(VVector startPoint,VVector endPoint, VLine axisLine, double height, double radius)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            AxisLine = axisLine;
            Radius = radius;
            Height = height;
            NativeAPI.GenCylinderByData(Center.ToPoint(), AxisLine.Direction.ToVector(), height, radius, out lptr);
        }

        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="ptr"></param>
        public VCylinder(IntPtr ptr) : base(ptr)
        {
            InitCylinder();
        }

        /// <summary>
        /// 通过指针来构建圆柱
        /// </summary>
        private void InitCylinder()
        {
            NativeAPI.GetCylinderData(LPtr, out var vStart, out var vEnd, out var linePtr, out var height, out var vRadius);
            StartPoint = new VVector(vStart);
            EndPoint = new VVector(vEnd);
            AxisLine = new VLine(linePtr);
            Radius = vRadius;
            Height = height;
        }

        public override void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddCylinderActor(ID, LPtr);
        }

        #region IXmlParser 接口

        public override XElement ExportXml()
        {
           
            XElement xRoot = new XElement("VCylinder");
            xRoot.SetAttributeValue("Radius", Radius);
            xRoot.SetAttributeValue("Height", Height);
            xRoot.Add(StartPoint.ExportXml());
            xRoot.Add(EndPoint.ExportXml());
            xRoot.Add(AxisLine.ExportXml());
            xRoot.Add(Center.ExportXml());
            return xRoot;
        }

        public override void ParserXml(XElement xElement)
        {
            int count = xElement.Elements().Count();
            for (int i = 0; i < count; i++)
            {
                XElement xItem = xElement.Elements().ElementAt(i);

                switch (i)
                {
                    case 0:
                        StartPoint.ParserXml(xItem);
                        break;

                    case 1:
                        EndPoint.ParserXml(xItem);
                        break;

                    case 2:
                        {
                            if (AxisLine==null)
                            {
                                AxisLine = new VLine();
                            }
                            AxisLine.ParserXml(xItem);
                        }
                        break;
                    case 3:
                        Center.ParserXml(xItem);
                        break;
                }
            }
            xElement.GetAttribute("Radius", (r) => Radius = double.Parse(r));
            xElement.GetAttribute("Height", (r) => Height = double.Parse(r));
            NativeAPI.GenCylinderByData(Center.ToPoint(), AxisLine.Direction.ToVector(), Height, Radius, out lptr);
        }

        #endregion

        public override string ToString()
        {
            return "VCylinder";
        }
    }
}