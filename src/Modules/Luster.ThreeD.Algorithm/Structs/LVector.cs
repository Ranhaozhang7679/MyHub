#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LVector
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.DataModels
* 文 件 名:       LVector.cs
* 创建时间:       2021/11/14 19:47:28
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      315c95fd-f4c6-4596-a911-f2c61faf3a4e
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/14 19:47:28
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LVector : IXMLParser
    {
        public double I { get; set; }

        public double J { get; set; }

        public double K { get; set; }

        public LVector(LPoint3 start, LPoint3 end)
        {
            I = end.X - start.X;
            J = end.Y - start.Y;
            K = end.Z - start.Z;
        }

        public LVector(double iIn, double jIn, double kIn)
        {
            I = iIn;
            J = jIn;
            K = kIn;
        }

        public unsafe void Transform(LMatrix matrix)
        {
            double num = I;
            double num2 = J;
            double num3 = K;
            matrix.VectorRotate(&num, &num2, &num3, false);
            I = num;
            J = num2;
            K = num3;
        }

        public unsafe void InvTransform(LMatrix matrix)
        {
            double num = I;
            double num2 = J;
            double num3 = K;
            matrix.VectorRotate(&num, &num2, &num3, true);
            I = num;
            J = num2;
            K = num3;
        }

        public unsafe void Mirror(double ii, double jj, double kk)
        {
            //LTransform LTransform = global::< Module >.InterfaceStruct.GetMirrorMatrtix(ii, jj, kk);
            //double num = I;
            //double num2 = J;
            //double num3 = 1.0;
            //LTransform.VectorRotate(&num, &num2, &num3, false);
            //I = num;
            //J = num2;
        }

        public LVector Rotate(double angle, int dim)
        {
            LVector result = this;
            LMatrix rotateMatrix = LMatrix.GetRotateMatrix(angle, dim);
            result.Transform(rotateMatrix);
            return result;
        }

        public static LVector Convert(LPoint3 pt)
        {
            LVector result = new LVector(pt, new LPoint3(0, 0, 0));
            return result;
        }

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("LVector");
            xRoot.SetAttributeValue("I", I.ToString());
            xRoot.SetAttributeValue("J", J.ToString());
            xRoot.SetAttributeValue("K", K.ToString());

            return xRoot;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">对象配置文件</param>
        public void ParserXml(XElement xElement)
        {
            XAttribute xI = xElement.Attribute("I");
            if (xI != null)
            {
                I = System.Convert.ToDouble(xI.Value);
            }

            XAttribute xJ = xElement.Attribute("J");
            if (xJ != null)
            {
                J = System.Convert.ToDouble(xJ.Value);
            }

            XAttribute xK = xElement.Attribute("K");
            if (xK != null)
            {
                K = System.Convert.ToDouble(xK.Value);
            }
        }

        public double GetLength2D()
        {
            double num = J;
            double num2 = I;
            double num3 = num2;
            double num4 = num3 * num3;
            double num5 = num;
            return Math.Sqrt(num4 + num5 * num5);
        }

        public double GetLength3D()
        {
            double num = J;
            double num2 = I;
            double num3 = K;
            double num4 = num2;
            double num5 = num4 * num4;
            double num6 = num;
            double num7 = num5 + num6 * num6;
            double num8 = num3;
            return Math.Sqrt(num7 + num8 * num8);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool IsZero()
        {
            return (byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0;
        }

        public LVector Unit()
        {
            LVector result = default(LVector);
            double num = J;
            double num2 = I;
            double num3 = K;
            double num4 = num2;
            double num5 = num4 * num4;
            double num6 = num;
            double num7 = num5 + num6 * num6;
            double num8 = num3;
            double num9 = Math.Sqrt(num7 + num8 * num8);
            if (num9 < 1E-08)
            {
                result.I = 0.0;
                result.J = 0.0;
                result.K = 0.0;
            }
            else
            {
                result.I = I / num9;
                result.J = J / num9;
                result.K = K / num9;
            }
            return result;
        }

        public double XAngleInPlaneXY()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            double num = Math.Atan2(J, I) * 180.0 / 3.1415926535897931;
            if (num < 0.0)
            {
                num += 360.0;
            }
            if (num == 360.0)
            {
                num = 0.0;
            }
            return num;
        }

        public double XAngleInPlaneYZ()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            double num = Math.Atan2(K, J) * 180.0 / 3.1415926535897931;
            if (num < 0.0)
            {
                num += 360.0;
            }
            if (num == 360.0)
            {
                num = 0.0;
            }
            return num;
        }

        public double XAngleInPlaneXZ()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            double num = Math.Atan2(K, I) * 180.0 / 3.1415926535897931;
            if (num < 0.0)
            {
                num += 360.0;
            }
            if (num == 360.0)
            {
                num = 0.0;
            }
            return num;
        }

        public double XYAngle()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            return Math.Asin(K / GetLength3D()) * 180.0 / 3.1415926535897931;
        }

        public double XZAngle()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            return Math.Asin(J / GetLength3D()) * 180.0 / 3.1415926535897931;
        }

        public double YZAngle()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            return Math.Asin(I / GetLength3D()) * 180.0 / 3.1415926535897931;
        }

        public double XAngle()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            return Math.Acos(I / GetLength3D()) * 180.0 / 3.1415926535897931;
        }

        public double YAngle()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            return Math.Acos(J / GetLength3D()) * 180.0 / 3.1415926535897931;
        }

        public double ZAngle()
        {
            if ((byte)((GetLength3D() < 1E-08) ? 1 : 0) != 0)
            {
                return 0.0;
            }
            return Math.Acos(K / GetLength3D()) * 180.0 / 3.1415926535897931;
        }

        public static LVector operator +(LVector l, LPoint3 piont)
        {
            LVector result = new LVector();
            result.I = l.I + piont.X;
            result.J = l.J + piont.Y;
            result.K = l.K + piont.Z;
            return result;
        }

        public static LVector operator *(LVector v1, double scale)
        {
            LVector result = new LVector();
            result.I = v1.I * scale;
            result.J = v1.J * scale;
            result.K = v1.K * scale;
            return result;
        }

        public static LVector operator *(LVector v1, LVector vect2)
        {
            LVector result = default(LVector);
            double num = v1.J;
            double num2 = vect2.K;
            double num3 = v1.K;
            double num4 = vect2.J;
            result.I = num2 * num - num4 * num3;
            double num5 = vect2.I;
            double num6 = v1.I;
            result.J = num5 * num3 - num6 * num2;
            result.K = num6 * num4 - num5 * num;
            return result;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static bool Equals(LVector vect1, LVector vect2)
        {
            int num = (Math.Abs(vect1.I - vect2.I) < 1E-08 && Math.Abs(vect1.J - vect2.J) < 1E-08 && Math.Abs(vect1.K - vect2.K) < 1E-08) ? 1 : 0;
            return (byte)num != 0;
        }

        public static LVector operator /(LVector v1, double scale)
        {
            LVector result = new LVector();
            result.I = v1.I / scale;
            result.J = v1.J / scale;
            result.K = v1.K / scale;
            return result;
        }

        public static LVector Vect_crossMult(LVector vect1, LVector vect2)
        {
            LVector result = default(LVector);
            double num = vect2.K;
            double num2 = vect1.J;
            double num3 = vect1.K;
            double num4 = vect2.J;
            result.I = num2 * num - num4 * num3;
            double num5 = vect2.I;
            double num6 = vect1.I;
            result.J = num5 * num3 - num6 * num;
            result.K = num6 * num4 - num5 * num2;
            return result;
        }

        public static double Vect_pointMult(LVector vect1, LVector vect2)
        {
            return vect1.J * vect2.J + vect1.I * vect2.I + vect1.K * vect2.K;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static bool Vect_isParallel(LVector vect1, LVector vect2)
        {
            return (byte)((Vect_crossMult(vect1, vect2).GetLength3D() < 1E-08) ? 1 : 0) != 0;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static bool Vect_isPerpend(LVector vect1, LVector vect2)
        {
            return (byte)((Math.Abs(Vect_pointMult(vect1, vect2)) < 1E-08) ? 1 : 0) != 0;
        }

        public static double Vect_calcAngle(LVector vect1, LVector vect2)
        {
            double length3D = vect1.GetLength3D();
            double length3D2 = vect2.GetLength3D();
            double d = Vect_pointMult(vect1, vect2) / length3D / length3D2;
            return Math.Acos(d) * 180.0 / 3.1415926535897931;
        }

        public override string ToString()
        {
            double num = K;
            double num2 = J;
            double num3 = I;
            return num3.ToString() + ", " + num2.ToString() + ", " + num.ToString();
        }
    }
}
