#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VPoint
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VPoint.cs
* 创建时间:       2022/1/7 13:26:34
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      4653b269-5e30-49ad-9c01-de7b1576f0f1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/7 13:26:34
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Interfaces;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm
{
    public class VVector : LDisposable, IXMLParser, IActor, IVariable, IPoint3, IRelativeCoord, IPoint
    {
        /// <summary>
        /// X
        /// </summary>
        [OutProperty]
        public double X { get; set; }

        /// <summary>
        /// Y
        /// </summary>
        [OutProperty]
        public double Y { get; set; }

        /// <summary>
        /// Z
        /// </summary>
        [OutProperty]
        public double Z { get; set; }

        [OutProperty]
        public double I { get; set; }

        [OutProperty]
        public double J { get; set; }

        [OutProperty]
        public double K { get; set; }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public VVector()
        {
            X = 0;
            Y = 0;
            Z = 0;

            I = 0;
            J = 0;
            K = 0;
        }

        public VVector(double x = 0, double y = 0, double z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public VVector(double x = 0, double y = 0, double z = 0, double i = 0, double j = 0, double k = 0)
        {
            X = x;
            Y = y;
            Z = z;
            I = i;
            J = j;
            K = k;
        }

        public VVector(LPoint3 p3)
        {
            X = p3.X;
            Y = p3.Y;
            Z = p3.Z;
        }

        /// <summary>
        /// 点对象
        /// </summary>
        public VVector Point => this;


        #region IXMLParser

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Point");
            xRoot.SetAttributeValue("X", X.ToString());
            xRoot.SetAttributeValue("Y", Y.ToString());
            xRoot.SetAttributeValue("Z", Z.ToString());

            return xRoot;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">对象配置文件</param>
        public override void ParserXml(XElement xElement)
        {
            XAttribute xX = xElement.Attribute("X");
            if (xX != null)
            {
                X = Convert.ToDouble(xX.Value);
            }

            XAttribute xY = xElement.Attribute("Y");
            if (xY != null)
            {
                Y = Convert.ToDouble(xY.Value);
            }

            XAttribute xZ = xElement.Attribute("Z");
            if (xZ != null)
            {
                Z = Convert.ToDouble(xZ.Value);
            }
        }

        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <param name="interactor"></param>
        public void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddPointActor(ID, ToPoint());
            // 显示点的方向信息
            if (I != -999 && J != -999 && K != -999)
            {
                i3D.AddVectorActor(ID, new VVector(ToPoint()), new VDirection(ToVector()));
            }
        }

        public LPoint3 ToPoint()
        {
            LPoint3 p = new LPoint3(X, Y, Z);
            if (Coord == null) return p;

            LCoord coord = new LCoord(Coord.Matrix);
            NativeAPI.PointToGlobal(p, coord, out var outPoint);
            return outPoint;
        }

        /// <summary>
        /// 带有法向信息的点
        /// </summary>
        /// <returns></returns>
        public LVector ToVector()
        {
            LVector v = new LVector(I, J, K);
            if (Coord == null) return v;

            LCoord coord = new LCoord(Coord.Matrix);
            NativeAPI.VectorToGlobal(v, coord, out var oVector);
            return oVector;
        }

        public override string ToString()
        {
            return $"{Math.Round(X, 4)} {Math.Round(Y, 4)} {Math.Round(Z, 4)}";

            if (Coord == null)
            {
                return $"{Math.Round(X, 4)} {Math.Round(Y, 4)} {Math.Round(Z, 4)}";
            }
            else
            {
                return WorldToRel(Coord).ToString();
            }

        }

        /// <summary>
        /// 是否为空对象判断
        /// </summary>
        /// <returns></returns>
        public override bool IsEmpty()
        {
            return this == null;
        }

        public override object Clone()
        {
            return new VVector(X, Y, Z);
        }

        #region 相对坐标系
        /// <summary>
        /// 坐标系
        /// </summary>
        public ICoord3 Coord { get; set; }

        /// <summary>
        /// 将当前对象转换到真实的世界坐标系下
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object RelToWorld(ICoord3 coord = null)
        {
            LPoint3 p = new LPoint3(X, Y, Z);
            if (coord == null) return new VVector(p);

            LCoord lCoord = new LCoord(coord.Matrix);
            NativeAPI.PointToGlobal(p, lCoord, out var outPoint);

            var retP = new VVector(outPoint);
            if (I != 0 || J != 0 || K != 0)
            {
                LVector v = new LVector(I, J, K);
                NativeAPI.VectorToGlobal(v, lCoord, out var outV);
                retP.I = outV.I;
                retP.J = outV.J;
                retP.K = outV.K;
            }

            return retP;
        }

        /// <summary>
        /// 从世界坐标系转换到相对坐标系中
        /// </summary>
        /// <returns></returns>
        public object WorldToRel(ICoord3 coord = null)
        {
            LPoint3 p = new LPoint3(X, Y, Z);
            if (coord == null) return new VVector(p);

            LCoord lCoord = new LCoord(coord.Matrix);
            NativeAPI.PointToRelative(p, lCoord, out var outPoint);
            return new VVector(outPoint);
        }
        #endregion
    }

    /// <summary>
    /// 方向
    /// </summary>
    public class VDirection : LDisposable, IXMLParser, IActor, IDirection, IRelativeCoord
    {
        /// <summary>
        /// I
        /// </summary>
        public double I { get; set; }

        /// <summary>
        /// J
        /// </summary>
        public double J { get; set; }

        /// <summary>
        /// K
        /// </summary>
        public double K { get; set; }

        /// <summary>
        /// 反向
        /// </summary>
        [OutProperty("反向")]
        public VDirection InvertDirection
        {
            get
            {
                return new VDirection(-I, -J, -K);
            }
        }

        public VDirection Direction => this;

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public VDirection()
        {
            I = 0;
            J = 0;
            K = 0;
        }

        public VDirection(double x = 0, double y = 0, double z = 0)
        {
            I = x;
            J = y;
            K = z;

            Normalize();
        }

        public VDirection(VVector v)
        {
            I = v.X;
            J = v.Y;
            K = v.Z;

            Normalize();
        }

        public VDirection(LVector v)
        {
            I = v.I;
            J = v.J;
            K = v.K;

            Normalize();
        }

        #region IXMLParser

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Vector");
            xRoot.SetAttributeValue("I", I.ToString());
            xRoot.SetAttributeValue("J", J.ToString());
            xRoot.SetAttributeValue("K", K.ToString());

            return xRoot;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">对象配置文件</param>
        public override void ParserXml(XElement xElement)
        {
            XAttribute xI = xElement.Attribute("I");
            if (xI != null)
            {
                I = Convert.ToDouble(xI.Value);
            }

            XAttribute xJ = xElement.Attribute("J");
            if (xJ != null)
            {
                J = Convert.ToDouble(xJ.Value);
            }

            XAttribute xK = xElement.Attribute("K");
            if (xK != null)
            {
                K = Convert.ToDouble(xK.Value);
            }
        }

        #endregion

        public LVector ToVector()
        {
            LVector v = new LVector(I, J, K);
            if (Coord == null) return v;

            LCoord coord = new LCoord(Coord.Matrix);
            NativeAPI.VectorToGlobal(v, coord, out var oVector);
            return oVector;
        }

        public LPoint3 ToPoint()
        {
            LPoint3 point = new LPoint3(I, J, K);
            return point;
        }

        public override string ToString()
        {
            return $"{Math.Round(I, 4)},{Math.Round(J, 4)},{Math.Round(K, 4)}";
        }

        /// <summary>
        /// 是否为空对象判断
        /// </summary>
        /// <returns></returns>
        public override bool IsEmpty()
        {
            return this == null;
        }

        public void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            //i3D.AddVectorActor(ID, ToVector());
        }

        public override object Clone()
        {
            return new VDirection(I, J, K);
        }

        private void Normalize()
        {
            double len = Math.Sqrt(Math.Pow(I, 2) + Math.Pow(J, 2) + Math.Pow(K, 2));
            if (Math.Abs(len - 1) > 1e-3 && len != 0)
            {
                I /= len;
                J /= len;
                K /= len;
            }
        }

        #region 相对坐标系
        /// <summary>
        /// 坐标系
        /// </summary>
        public ICoord3 Coord { get; set; }

        /// <summary>
        /// 将当前对象转换到真实的世界坐标系下
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object RelToWorld(ICoord3 coord = null)
        {
            LVector v = new LVector(I, J, K);
            if (coord == null) return v;

            LCoord lCoord = new LCoord(Coord.Matrix);
            NativeAPI.VectorToGlobal(v, lCoord, out var oVector);
            return new VDirection(oVector);
        }

        /// <summary>
        /// 从世界坐标系转换到相对坐标系中
        /// </summary>
        /// <returns></returns>
        public object WorldToRel(ICoord3 coord = null)
        {
            LVector v = new LVector(I, J, K);
            if (coord == null) return v;

            LCoord lCoord = new LCoord(Coord.Matrix);
            NativeAPI.VectorToRelative(v, lCoord, out var oVector);
            return new VDirection(oVector);
        }
        #endregion
    }

    public class VSize : LDisposable, IXMLParser
    {
        /// <summary>
        /// X
        /// </summary>
        public double XLen { get; set; }

        /// <summary>
        /// Y
        /// </summary>
        public double YLen { get; set; }

        /// <summary>
        /// Z
        /// </summary>
        public double ZLen { get; set; }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public VSize()
        {
            XLen = 0;
            YLen = 0;
            ZLen = 0;
        }

        public VSize(double x = 0, double y = 0, double z = 0)
        {
            XLen = x;
            YLen = y;
            ZLen = z;
        }

        #region IXMLParser

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public override XElement ExportXml()
        {
            XElement xRoot = new XElement("Size3");
            xRoot.SetAttributeValue("XLen", XLen.ToString());
            xRoot.SetAttributeValue("YLen", YLen.ToString());
            xRoot.SetAttributeValue("ZLen", ZLen.ToString());

            return xRoot;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">对象配置文件</param>
        public override void ParserXml(XElement xElement)
        {
            XAttribute xX = xElement.Attribute("XLen");
            if (xX != null)
            {
                XLen = Convert.ToDouble(xX.Value);
            }

            XAttribute xY = xElement.Attribute("YLen");
            if (xY != null)
            {
                YLen = Convert.ToDouble(xY.Value);
            }

            XAttribute xZ = xElement.Attribute("ZLen");
            if (xZ != null)
            {
                ZLen = Convert.ToDouble(xZ.Value);
            }
        }

        #endregion

        public LSize3 ToSize()
        {
            LSize3 size = new LSize3(XLen, YLen, ZLen);
            return size;
        }

        public override string ToString()
        {
            return $"{Math.Round(XLen, 2)},{Math.Round(YLen, 2)},{Math.Round(ZLen, 2)}";
        }

        /// <summary>
        /// 是否为空对象判断
        /// </summary>
        /// <returns></returns>
        public override bool IsEmpty()
        {
            return this == null;
        }

        public override object Clone()
        {
            return new VSize(XLen, YLen, ZLen);
        }
    }
}