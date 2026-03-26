#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LPoint
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.DataModels
* 文 件 名:       LPoint.cs
* 创建时间:       2021/11/11 14:10:39
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d0574051-78c7-4ebf-a264-3f8b8a296fb9
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/11 14:10:39
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
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
    public struct LPoint3 : IXMLParser
    {
        /// <summary>
        /// 点 X
        /// </summary>
        public double X;

        /// <summary>
        /// 点 Y
        /// </summary>
        public double Y;

        /// <summary>
        /// 点 Z
        /// </summary>
        public double Z;

        public LPoint3(double xIn = 0, double yIn = 0, double zIn = 0)
        {
            X = xIn;
            Y = yIn;
            Z = zIn;
        }

        public unsafe void Transform(LMatrix matrix)
        {
            double num = X;
            double num2 = Y;
            double num3 = Z;
            matrix.VectorRotate(&num, &num2, &num3, false);
            matrix.VectorOffset(ref num, ref num2, ref num3, false);
            X = num;
            Y = num2;
            Z = num3;
        }

        public unsafe void InvTransform(LMatrix matrix)
        {
            double num = X;
            double num2 = Y;
            double num3 = Z;
            matrix.VectorOffset(ref num, ref num2, ref num3, true);
            matrix.VectorRotate(&num, &num2, &num3, true);
            X = num;
            Y = num2;
            Z = num3;
        }

        public unsafe void Mirror(double ii, double jj, double kk)
        {
            //LTransform LTransform = global::< Module >.InterfaceStruct.GetMirrorMatrtix(ii, jj, kk);
            //double num = X;
            //double num2 = Y;
            //double num3 = 1.0;
            //LTransform.VectorRotate(&num, &num2, &num3, false);
            //X = num;
            //Y = num2;
        }

        public static LPoint3 operator +(LPoint3 pt, double offset)
        {
            LPoint3 result = new LPoint3();
            result.X = pt.X + offset;
            result.Y = pt.Y + offset;
            result.Z = pt.Z + offset;
            return result;
        }

        public static LPoint3 operator +(LPoint3 pt1, LPoint3 pt2)
        {
            LPoint3 result = new LPoint3();
            result.X = pt1.X + pt2.X;
            result.Y = pt1.Y + pt2.Y;
            result.Z = pt1.Z + pt2.Z;
            return result;
        }

        public static LPoint3 operator -(LPoint3 pt, double offset)
        {
            LPoint3 result = new LPoint3();
            result.X = pt.X - offset;
            result.Y = pt.Y - offset;
            result.Z = pt.Z - offset;
            return result;
        }

        public static LPoint3 operator -(LPoint3 pt1, LPoint3 pt2)
        {
            LPoint3 result = new LPoint3();
            result.X = pt1.X - pt2.X;
            result.Y = pt1.Y - pt2.Y;
            result.Z = pt1.Z - pt2.Z;
            return result;
        }

        public static LPoint3 operator /(LPoint3 pt, int n)
        {
            double num = 1.0 / n;

            LPoint3 result = new LPoint3();
            result.X = pt.X * num;
            result.Y = pt.Y * num;
            result.Z = pt.Z * num;
            return result;
        }

        public static LPoint3 operator *(LPoint3 pt, double a)
        {
            LPoint3 result = new LPoint3();
            result.X = pt.X * a;
            result.Y = pt.Y * a;
            result.Z = pt.Z * a;
            return result;
        }

        /// <summary>
        /// 重新字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Math.Round(X, 1)},{Math.Round(Y, 1)},{Math.Round(Z, 1)}";
        }

        public unsafe static LPoint3 ToPoint(string strPoint)
        {
            string text = "(){}";
            string text2 = ", ";
            string[] array = strPoint.Trim(text.ToCharArray()).Split(text2.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            LPoint3 result = default(LPoint3);
            if ((long)(IntPtr)(void*)array.LongLength >= 2L)
            {
                result.X = Convert.ToDouble(array[0]);
                result.Y = Convert.ToDouble(array[1]);
                if ((long)(IntPtr)(void*)array.LongLength >= 3L)
                {
                    result.Z = Convert.ToDouble(array[2]);
                }
            }
            return result;
        }

        /// <summary>
        /// 归一化
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static double Norm(LPoint3 pt)
        {
            double num = pt.Y;
            double num2 = pt.X;
            double num3 = pt.Z;

            double num4 = num2;
            double num5 = num4 * num4;
            double num6 = num;
            double num7 = num5 + num6 * num6;
            double num8 = num3;
            return Math.Sqrt(num7 + num8 * num8);
        }

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("LPoint");
            xRoot.SetAttributeValue("X", X.ToString());
            xRoot.SetAttributeValue("Y", Y.ToString());
            xRoot.SetAttributeValue("Z", Z.ToString());

            return xRoot;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">对象配置文件</param>
        public void ParserXml(XElement xElement)
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

        public void AddActor(IInteractor interactor)
        {
            throw new NotImplementedException();
        }
    }
}
