#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCuboid
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VCuboid.cs
* 创建时间:       2021/12/21 10:20:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      129b5309-738c-4650-8a34-013a41d550bf
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/21 10:20:30
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Enums;
using Luster.ThreeD.Algorithm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 长方体
    /// </summary>
    public class VCuboid : LBaseROI
    {
        /// <summary>
        /// 长方体中心
        /// </summary>
        public override VVector Center { get; set; } = new VVector();

        /// <summary>
        /// 长度方向
        /// </summary>
        public VDirection LVector { get; set; } = new VDirection();

        /// <summary>
        /// 宽度方向向量
        /// </summary>
        public VDirection WVector { get; set; } = new VDirection();

        /// <summary>
        /// 高度方向向量
        /// </summary>
        public VDirection HVector { get; set; } = new VDirection();

        /// <summary>
        /// 长、宽、高尺寸
        /// </summary>
        public VSize LWHSize { get; set; } = new VSize();

        /// <summary>
        /// 构造函数
        /// </summary>
        public VCuboid() : base()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ptr"></param>
        public VCuboid(IntPtr ptr) : base(ptr)
        {
            Init();
        }

        /// <summary>
        /// 对象初始化
        /// </summary>
        private void Init()
        {
            if (LPtr == IntPtr.Zero) return;
            NativeAPI.GetCuboidData(LPtr, out var center, out var lVector, out var wVector, out var hVector, out var len, out var w, out var h);

            Center = new VVector(center);
            LVector = new VDirection(lVector.X, lVector.Y, lVector.Z);
            WVector = new VDirection(wVector.X, wVector.Y, wVector.Z);
            HVector = new VDirection(hVector.X, hVector.Y, hVector.Z);
            LWHSize = new VSize(len, w, h);
        }

        /// <summary>
        /// 长高宽
        /// </summary>
        /// <param name="vCenter"></param>
        /// <param name="lVec"></param>
        /// <param name="hVec"></param>
        /// <param name="wVec"></param>
        /// <param name="size"></param>
        public VCuboid(VVector vCenter, VDirection lVec, VDirection wVec, VDirection hVec, VSize size)
        {
            Center = vCenter;
            LVector = new VDirection(lVec.I, lVec.J, lVec.K);
            WVector = new VDirection(wVec.I, wVec.J, wVec.K);
            HVector = new VDirection(hVec.I, hVec.J, hVec.K);
            LWHSize = size;
            BuildCpp();
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Cuboid");
            xRoot.SetAttributeValue("IsInside", IsInside);
            xRoot.SetAttributeValue("SurfDistance", SurfDistance);

            // 此处及时是通过BuildByCPP也要保存，因为新增Module会copy对象，否则对象为空
            xRoot.Add(Center.ExportXml());
            xRoot.Add(LVector.ExportXml());
            xRoot.Add(WVector.ExportXml());
            xRoot.Add(HVector.ExportXml());
            xRoot.Add(LWHSize.ExportXml());

            return xRoot;
        }

        /// <summary>
        /// 解析对象
        /// </summary>
        /// <param name="xElement"></param>
        public override void ParserXml(XElement xElement)
        {
            int count = xElement.Elements().Count();
            for (int i = 0; i < count; i++)
            {
                XElement xItem = xElement.Elements().ElementAt(i);

                switch (i)
                {
                    case 0:
                        Center.ParserXml(xItem);
                        break;

                    case 1:
                        LVector.ParserXml(xItem);
                        break;

                    case 2:
                        WVector.ParserXml(xItem);
                        break;

                    case 3:
                        HVector.ParserXml(xItem);
                        break;

                    case 4:
                        LWHSize.ParserXml(xItem);
                        break;
                }
            }

            xElement.GetAttribute("IsInside", (v) => IsInside = bool.Parse(v));
            xElement.GetAttribute("SurfDistance", (v) => SurfDistance = double.Parse(v));

            if (count > 0)
                BuildCpp();
        }

        /// <summary>
        /// 基于已有数据构建C++的对象
        /// </summary>
        public void BuildCpp()
        {
            NativeAPI.GenCuboidByData(Center.ToPoint(), LVector.ToPoint(),  WVector.ToPoint(), HVector.ToPoint(), LWHSize.ToSize(), out lptr);
            IsDisposed = false;
        }

        protected override void DisposeObj()
        {
            NativeAPI.DeleteObj(lptr, NativeAPI.ClassType.Cuboio);
        }

        public override string ToString()
        {
            return "VCuboid";
        }

        /// <summary>
        /// 立方体
        /// </summary>
        /// <param name="interactor"></param>
        public override void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddCuboidActor(ID, LPtr);
        }
    }
}