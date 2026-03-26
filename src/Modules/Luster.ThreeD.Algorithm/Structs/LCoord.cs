#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LCOORD
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.DataModels
* 文 件 名:       LCOORD.cs
* 创建时间:       2021/11/14 20:06:55
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      67d533f2-a9cf-4981-ba81-1ebfab4d7991
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/14 20:06:55
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
    public struct LCoord : IXMLParser, ITransform
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public double[] matrix;

        public double[] Matrix
        {
            get => matrix; set
            {
                matrix[0] = value[0];
                matrix[1] = value[1];
                matrix[2] = value[2];
                matrix[3] = value[3];
                matrix[4] = value[4];
                matrix[5] = value[5];
                matrix[6] = value[6];
                matrix[7] = value[7];
                matrix[8] = value[8];
                matrix[9] = value[9];
                matrix[10] = value[10];
                matrix[11] = value[11];
            }
        }

        public LCoord(LMatrix trans)
        {
            matrix = new double[16];
            Init();
            matrix[0] = trans.RotMatrix[0];
            matrix[1] = trans.RotMatrix[1];
            matrix[2] = trans.RotMatrix[2];
            matrix[3] = trans.Offset[0];
            matrix[4] = trans.RotMatrix[3];
            matrix[5] = trans.RotMatrix[4];
            matrix[6] = trans.RotMatrix[5];
            matrix[7] = trans.Offset[1];
            matrix[8] = trans.RotMatrix[6];
            matrix[9] = trans.RotMatrix[7];
            matrix[10] = trans.RotMatrix[8];
            matrix[11] = trans.Offset[2];
            matrix[12] = 0.0;
            matrix[13] = 0.0;
            matrix[14] = 0.0;
            matrix[15] = 1.0;
        }

        public LCoord(double[] matrix)
        {
            this.matrix = matrix;
        }

        public static LCoord GetMCS()
        {
            LCoord result = default(LCoord);
            result.Init();
            return result;
        }

        public void Init()
        {
            // 初始化矩阵
            matrix = new double[16]
            {
                1,0,0,0,
                0,1,1,0,
                0,0,1,0,
                0,0,0,1
            };
        }

        public LMatrix GetTransform()
        {
            LMatrix result = default(LMatrix);
            result.Init();
            double[] array = matrix;
            result.RotMatrix[0] = array[0];
            result.RotMatrix[1] = array[1];
            result.RotMatrix[2] = array[2];
            result.RotMatrix[3] = array[4];
            result.RotMatrix[4] = array[5];
            result.RotMatrix[5] = array[6];
            result.RotMatrix[6] = array[8];
            result.RotMatrix[7] = array[9];
            result.RotMatrix[8] = array[10];
            result.Offset[0] = array[3];
            result.Offset[1] = array[7];
            result.Offset[2] = array[11];
            return result;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool IsValid()
        {
            double[] array = matrix;
            if (array == null)
            {
                return false;
            }
            double k = array[8];
            double j = array[4];
            LVector tZVector = new LVector();
            tZVector.I = array[0];
            tZVector.J = j;
            tZVector.K = k;
            double k2 = array[9];
            double j2 = array[5];
            LVector tZVector2 = new LVector();
            tZVector2.I = array[1];
            tZVector2.J = j2;
            tZVector2.K = k2;
            double k3 = array[10];
            double j3 = array[6];
            LVector tZVector3 = new LVector();
            tZVector3.I = array[2];
            tZVector3.J = j3;
            tZVector3.K = k3;
            if (!(tZVector.GetLength3D() > 1.0001) && !(tZVector.GetLength3D() < 0.9999))
            {
                if (!(tZVector2.GetLength3D() > 1.0001) && !(tZVector2.GetLength3D() < 0.9999))
                {
                    if (!(tZVector3.GetLength3D() > 1.0001) && !(tZVector3.GetLength3D() < 0.9999))
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("LCoord");

            if (matrix != null)
                xRoot.SetAttributeValue("Matrix", string.Join(",", matrix));

            return xRoot;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">对象配置文件</param>
        public void ParserXml(XElement xElement)
        {
            // 初始化结构
            Init();

            XAttribute xMatrix = xElement.Attribute("Matrix");
            if (xMatrix != null)
            {
                string[] v = xMatrix.Value.Split(',');
                for (int i = 0; i < v.Length; i++)
                {
                    matrix[i] = Convert.ToDouble(v[i]);
                }
            }
        }

        /// <summary>
        /// 矩阵
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsValid())
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 12; i++)
                {
                    sb.AppendFormat($"{Math.Round(matrix[i], 2)},");
                }

                sb.Remove(sb.Length - 1, 1);

                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取逆矩阵
        /// </summary>
        /// <returns></returns>
        public double[] GetInvert()
        {
            double[] array = matrix;
            double num = array[5];
            double num2 = array[0];
            double num3 = array[4];
            double num4 = array[1];
            double num5 = num2 * num - num4 * num3;
            double num6 = array[6];
            double num7 = array[2];
            double num8 = num6 * num2 - num7 * num3;
            double num9 = array[7];
            double num10 = array[3];
            double num11 = num9 * num2 - num10 * num3;
            double num12 = num6 * num4 - num7 * num;
            double num13 = num9 * num4 - num10 * num;
            double num14 = num9 * num7 - num10 * num6;
            double num15 = array[13];
            double num16 = array[8];
            double num17 = array[12];
            double num18 = array[9];
            double num19 = num16 * num15 - num18 * num17;
            double num20 = array[14];
            double num21 = array[10];
            double num22 = num20 * num16 - num21 * num17;
            double num23 = array[15];
            double num24 = array[11];
            double num25 = num23 * num16 - num24 * num17;
            double num26 = num20 * num18 - num21 * num15;
            double num27 = num23 * num18 - num24 * num15;
            double num28 = num23 * num21 - num24 * num20;
            double num29 = 1.0 / (num28 * num5 - num27 * num8 + num26 * num11 + num25 * num12 - num22 * num13 + num19 * num14);
            double[] invMatrix = new double[16];
            invMatrix[0] = (num * num28 - num6 * num27 + num9 * num26) * num29;
            invMatrix[1] = (array[2] * num27 - array[1] * num28 - array[3] * num26) * num29;
            invMatrix[2] = (array[13] * num14 - array[14] * num13 + array[15] * num12) * num29;
            invMatrix[3] = (array[10] * num13 - array[9] * num14 - array[11] * num12) * num29;
            invMatrix[4] = (array[6] * num25 - array[4] * num28 - array[7] * num22) * num29;
            invMatrix[5] = (array[0] * num28 - array[2] * num25 + array[3] * num22) * num29;
            invMatrix[6] = (array[14] * num11 - array[12] * num14 - array[15] * num8) * num29;
            invMatrix[7] = (array[8] * num14 - array[10] * num11 + array[11] * num8) * num29;
            invMatrix[8] = (array[4] * num27 - array[5] * num25 + array[7] * num19) * num29;
            invMatrix[9] = (array[1] * num25 - array[0] * num27 - array[3] * num19) * num29;
            invMatrix[10] = (array[12] * num13 - array[13] * num11 + array[15] * num5) * num29;
            invMatrix[11] = (array[9] * num11 - array[8] * num13 - array[11] * num5) * num29;
            invMatrix[12] = (array[5] * num22 - array[4] * num26 - array[6] * num19) * num29;
            invMatrix[13] = (array[0] * num26 - array[1] * num22 + array[2] * num19) * num29;
            invMatrix[14] = (array[13] * num8 - array[12] * num12 - array[14] * num5) * num29;
            invMatrix[15] = (array[8] * num12 - array[9] * num8 + array[10] * num5) * num29;
            return invMatrix;
        }

        public bool IsValidate()
        {
            return true;
        }
    }
}
