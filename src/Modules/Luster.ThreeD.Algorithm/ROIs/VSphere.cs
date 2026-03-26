#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VSphere
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VSphere.cs
* 创建时间:       2021/12/17 11:41:34
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      3634529a-2653-40db-9a65-76a4f43564f1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 11:41:34
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
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
    /// 球体
    /// </summary>
    public class VSphere : LBaseROI
    {
        /// <summary>
        /// 球心
        /// </summary>
        [OutProperty("球心")]
        public override VVector Center
        {
            get; set;
        }

        /// <summary>
        /// 半径
        /// </summary>
        [OutProperty("半径")]
        public double Radius { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VSphere() : base()
        {
        }

        public VSphere(IntPtr ptr) : base(ptr)
        {
            if (ptr == IntPtr.Zero) return;
            NativeAPI.GetSphereData(ptr, out var center, out var r);
            Radius = r;
            Center = new VVector(center);
        }

        public VSphere(VVector center, double radius)
        {
            Center = center;
            Radius = radius;
            NativeAPI.GenSphereByData(center.ToPoint(), radius, out lptr);
        }

        protected override void DisposeObj()
        {
            NativeAPI.DeleteObj(lptr, NativeAPI.ClassType.Sphere);
        }

        /// <summary>
        /// 字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"VSphere";
        }

        public override void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddSphereActor(ID, LPtr);
        }

        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Sphere");
            xRoot.SetAttributeValue("Radius", Radius);
            xRoot.Add(Center.ExportXml());
            return xRoot;
        }

        public override void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Radius", (r) => Radius = double.Parse(r));
            var xCenter = xElement.Elements().FirstOrDefault();
            Center = new VVector();
            Center.ParserXml(xCenter);
            NativeAPI.GenSphereByData(Center.ToPoint(), Radius, out lptr);
        }
    }
}