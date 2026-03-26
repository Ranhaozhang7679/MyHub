#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LTransform
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.DataStruct.DataModels
* 文 件 名:       LTransform.cs
* 创建时间:       2021/11/14 19:45:10
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      f0399171-c661-4cf2-a160-6ad2465e3015
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/14 19:45:10
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
    public struct LMatrix : IXMLParser
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public double[] RotMatrix;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] Offset;

        public void Init()
        {
            Offset = new double[3] { 0, 0, 0 };
            RotMatrix = new double[9] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        }

        public bool IsNoTransform()
        {
            return Offset[0] == 0 && Offset[1] == 0 && Offset[2] == 0 &&
                RotMatrix[0] == 1 && RotMatrix[1] == 0 && RotMatrix[2] == 0 &&
                RotMatrix[3] == 1 && RotMatrix[4] == 1 && RotMatrix[5] == 0 &&
                RotMatrix[6] == 1 && RotMatrix[7] == 0 && RotMatrix[8] == 1;
        }


        public LMatrix(double[] matrix)
        {
            Offset = new double[3] { matrix[3], matrix[7], matrix[11], };

            RotMatrix = new double[9] {
                matrix[0], matrix[1], matrix[2],
                matrix[4], matrix[5], matrix[6],
                matrix[8], matrix[9], matrix[10] };
        }

        /// <summary>
        /// 转换为数组
        /// </summary>
        /// <returns></returns>
        public double[] ToArray()
        {
            return new double[16]
            {
                RotMatrix[0],RotMatrix[1],RotMatrix[2],Offset[0],
                RotMatrix[3],RotMatrix[4],RotMatrix[5],Offset[1],
                RotMatrix[6],RotMatrix[7],RotMatrix[8],Offset[2],
                0,0,0,1
            };
        }

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("LMatrix");

            if (RotMatrix != null)
                xRoot.SetAttributeValue("RotMatrix", string.Join(",", RotMatrix));

            if (Offset != null)
                xRoot.SetAttributeValue("Offset", string.Join(",", Offset));

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
            XAttribute xMatrix = xElement.Attribute("RotMatrix");
            if (xMatrix != null)
            {
                string[] v = xMatrix.Value.Split(',');
                for (int i = 0; i < v.Length; i++)
                {
                    RotMatrix[i] = Convert.ToDouble(v[i]);
                }
            }

            XAttribute xOffset = xElement.Attribute("Offset");
            if (xOffset != null)
            {
                string[] v = xOffset.Value.Split(',');
                for (int i = 0; i < v.Length; i++)
                {
                    Offset[i] = Convert.ToDouble(v[i]);
                }
            }
        }

        public void ReadCalibfile(string xCalib, string yCalib, string zCalib)
        {
            // X轴解析
            string[] xs = xCalib.Split(' ');
            for (int i = 0; i < 3; i++)
            {
                RotMatrix[i] = double.Parse(xs[i]);
            }
            Offset[0] = double.Parse(xs[3]);

            // Y轴解析
            string[] ys = yCalib.Split(' ');
            for (int i = 0; i < 3; i++)
            {
                RotMatrix[i + 3] = double.Parse(ys[i]);
            }
            Offset[1] = double.Parse(ys[3]);

            // X轴解析
            string[] zs = zCalib.Split(' ');
            for (int i = 0; i < 3; i++)
            {
                RotMatrix[i + 6] = double.Parse(zs[i]);
            }
            Offset[2] = double.Parse(zs[3]);
        }

        public LMatrix SingleRotate(LMatrix matrix, bool bInverseTransform)
        {
            LMatrix result = default(LMatrix);
            result.Init();
            if (!bInverseTransform)
            {
                double[] rotMatrix = RotMatrix;
                int num = 0;
                do
                {
                    int num2 = 0;
                    do
                    {
                        int num3 = num + num2;
                        int num4 = num3;
                        result.RotMatrix[num3] = 0.0;
                        int num5 = 0;
                        int num6 = num2;
                        do
                        {
                            double[] rotMatrix2 = result.RotMatrix;
                            rotMatrix2[num4] = matrix.RotMatrix[num + num5] * rotMatrix[num6] + rotMatrix2[num4];
                            num5++;
                            num6 += 3;
                        }
                        while (num5 < 3);
                        num2++;
                    }
                    while (num2 < 3);
                    num += 3;
                }
                while (num < 9);
            }
            else
            {
                int num7 = 0;
                double[] rotMatrix = RotMatrix;
                int num8 = 0;
                int num9 = 0;
                int num10;
                do
                {
                    num10 = num9;
                    uint num11 = 3u;
                    do
                    {
                        result.RotMatrix[num10] = 0.0;
                        int num12 = num7;
                        uint num13 = 3u;
                        do
                        {
                            double[] rotMatrix2 = result.RotMatrix;
                            rotMatrix2[num10] = matrix.RotMatrix[num12] * rotMatrix[num8 + num10 + num12] + rotMatrix2[num10];
                            num12 += 3;
                            num13 = (uint)((int)num13 + -1);
                        }
                        while (num13 != 0);
                        num10++;
                        num11 = (uint)((int)num11 + -1);
                    }
                    while (num11 != 0);
                    num7++;
                    num9 = num10;
                    num8 += -4;
                }
                while (num10 < 9);
            }
            return result;
        }

        #region Offset 方法
        /// <summary>
        /// 对象平移
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void AddOffset(double x, double y, double z)
        {
            double[] offset = Offset;
            offset[0] += x;
            offset[1] += y;
            offset[2] += z;
        }

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="dim">X、Y、Z</param>
        /// <param name="value">对应的值</param>
        public void SetOffset(int dim, double value)
        {
            if ((uint)dim <= 2u)
            {
                Offset[dim] = value;
            }
        }

        /// <summary>
        /// 更新移动量
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetOffset(double x, double y, double z)
        {
            double[] offset = Offset;
            offset[0] = x;
            offset[1] = y;
            offset[2] = z;
        }

        /// <summary>
        /// 获取某个维度的Offset
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="value"></param>
        public void GetOffset(int dim, out double value)
        {
            value = -1;
            if (dim <= 2)
            {
                value = Offset[dim];
            }
        }

        /// <summary>
        /// 获取平移
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void GetOffset(out double x, out double y, double z)
        {
            x = Offset[0];
            y = Offset[1];
            z = Offset[2];
        }
        #endregion

        public static LMatrix GetRotateMatrix(double angle, int dim)
        {
            LMatrix result = default(LMatrix);
            result.Init();

            double num = angle / 180.0 * 3.1415926535897931;
            double num2 = Math.Cos(num);
            double num3 = Math.Sin(num);
            int num4 = (dim + 1) % 3;
            int num5 = (dim + 2) % 3;
            int num6 = 0;
            do
            {
                result.RotMatrix[num6] = 0.0;
                num6++;
            }
            while (num6 < 9);
            result.RotMatrix[dim * 4] = 1.0;
            result.RotMatrix[num4 * 4] = num2;
            result.RotMatrix[num4 * 3 + num5] = 0.0 - num3;
            result.RotMatrix[num5 * 3 + num4] = num3;
            result.RotMatrix[num5 * 4] = num2;
            return result;
        }

        public static LMatrix getRotateTransformByNormalVec(double x, double y, double z)
        {
            LMatrix result = default(LMatrix);
            double num = x * x + y * y;
            double num2 = Math.Sqrt(z * z + num);
            if (!(num2 < 1E-16) && !(Math.Sqrt(num) < 1E-16))
            {
                result.Init();
                double num3 = x / num2;
                double num4 = y / num2;
                double num5 = z / num2;
                double num6 = num4;
                double num7 = num6 * num6;
                double num8 = num3;
                double num9 = num7 + num8 * num8;
                double num10 = Math.Sqrt(num9);
                double num11 = num3 / num10;
                double num12 = num4 / num10;
                double num13 = num5;
                double num14 = Math.Sqrt(num13 * num13 + num9);
                double num15 = num5 / num14;
                double num16 = num10 / num14;
                result.RotMatrix[0] = num15 * num11;
                result.RotMatrix[1] = 0.0 - num12;
                result.RotMatrix[2] = num16 * num11;
                result.RotMatrix[3] = num15 * num12;
                result.RotMatrix[4] = num11;
                result.RotMatrix[5] = num16 * num12;
                result.RotMatrix[6] = 0.0 - num16;
                result.RotMatrix[7] = 0.0;
                result.RotMatrix[8] = num15;
                return result;
            }
            return GetRotateMatrix(0.0, 2);
        }

        public static double GetRotateTheta(LMatrix T, int dim)
        {
            int num = (dim + 1) % 3;
            double[] rotMatrix = T.RotMatrix;
            int num2 = num;
            return Math.Acos(rotMatrix[num2 * num2 * 3]) * 180.0 / 3.1415926535897931;
        }

        /// <summary>
        /// 针对位移进行旋转变换
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="bInverseTransform"></param>
        public unsafe void VectorRotate(double* x, double* y, double* z, bool bInverseTransform)
        {
   //         double temp = *x;
   //         *(ref temp + 8) = *y;
   //         *(ref temp + 16) = *z;
			//temp temp2;
   //         if (!bInverseTransform)
   //         {
   //             double[] rotMatrix = this.RotMatrix;
   //             int num = 0;
			//	temp* ptr = &temp2;
   //             int num4;
   //             do
   //             {
   //                 double num2 = 0.0;
   //                 *(double*)ptr = 0.0;
   //                 long num3 = 0L;
   //                 num4 = num;
   //                 double num6;
   //                 do
   //                 {
   //                     double num5 = rotMatrix[num4] * *(num3 * 8L + ref temp) +num2;
   //                     num2 = num5;
   //                     num6 = num5;
   //                     num4++;
   //                     num3 += 1L;
   //                 }
   //                 while (num3 < 3L);
   //                 *(double*)ptr = num6;
   //                 num = num4;
   //                 ptr += 8L / (long)sizeof(temp);
   //             }
   //             while (num4 < 9);
   //         }
   //         else
   //         {
   //             int num7 = 0;
   //             double[] rotMatrix = this.RotMatrix;
			//	temp* ptr2 = &temp2;
   //             do
   //             {
   //                 double num8 = 0.0;
   //                 *(double*)ptr2 = 0.0;
			//		temp* ptr3 = &temp;
   //                 int num9 = num7;
   //                 uint num10 = 3U;
   //                 double num12;
   //                 do
   //                 {
   //                     double num11 = rotMatrix[num9] * *(double*)ptr3 + num8;
   //                     num8 = num11;
   //                     num12 = num11;
   //                     num9 += 3;
   //                     ptr3 += 8L / (long)sizeof(temp);
   //                     num10 += uint.MaxValue;
   //                 }
   //                 while (num10 > 0U);
   //                 *(double*)ptr2 = num12;
   //                 num7++;
   //                 ptr2 += 8L / (long)sizeof(temp);
   //             }
   //             while (num7 < 3);
   //         }
   //         *x = temp2;
   //         *y = *(ref temp2 + 8);
   //         *z = *(ref temp2 + 16);
        }

        /// <summary>
        /// 对距离进行偏移
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="bInverseTransform"></param>
        public void VectorOffset(ref double x, ref double y, ref double z, bool bInverseTransform)
        {
            double[] offset = Offset;
            if (offset != null)
            {
                if (!bInverseTransform)
                {
                    x = offset[0] + x;
                    y = Offset[1] + y;
                    z = Offset[2] + z;
                }
                else
                {
                    x -= offset[0];
                    y -= Offset[1];
                    z -= Offset[2];
                }
            }
        }

        public LMatrix Rotate(LMatrix matrix, bool bInverseTransform)
        {
            double[] offset = Offset;
            double num = offset[0];
            double num2 = offset[1];
            double num3 = offset[2];

            // 先旋转
            LMatrix lTransform = SingleRotate(matrix, bInverseTransform);

            // 再平移
            matrix.VectorOffset(ref num, ref num2, ref num3, bInverseTransform);

            lTransform.Offset[0] = num;
            lTransform.Offset[1] = num2;
            lTransform.Offset[2] = num3;
            return lTransform;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public unsafe bool AxisAlign(int PrimaryAxis, LVector PrimaryVector, int SecondaryAxis, LVector SecondaryVector)
        {
            //IL_0124: Expected I, but got I8
            //IL_0137: Expected I, but got I8
            //         if ((uint)PrimaryAxis <= 2u && (uint)SecondaryAxis <= 2u && PrimaryAxis != SecondaryAxis)
            //         {
            //$ArrayType$$$BY02H $ArrayType$$$BY02H;
            //             *(int*)(&$ArrayType$$$BY02H) = PrimaryAxis;
            //             *(int*)(ref $ArrayType$$$BY02H + 4) = SecondaryAxis;
            //             *(int*)(ref $ArrayType$$$BY02H + 8) = 3 - PrimaryAxis - SecondaryAxis;
            //             PrimaryVector = PrimaryVector.Unit();
            //             LVector LVector = SecondaryVector.Unit();
            //             SecondaryVector = LVector;
            //             LVector LVector2 = LVector.Vect_crossMult(PrimaryVector, LVector);
            //             if ((byte)((LVector2.GetLength3D() < 1E-08) ? 1 : 0) != 0)
            //             {
            //                 return false;
            //             }
            //             LVector LVector3 = LVector2.Unit();
            //             LVector2 = LVector3;
            //             LVector LVector4 = LVector.Vect_crossMult(PrimaryVector, LVector3);
            //             LVector4 = LVector4.Unit();
            //$ArrayType$$$BY08N $ArrayType$$$BY08N;
            //             *(double*)(&$ArrayType$$$BY08N) = PrimaryVector.i;
            //             *(double*)(ref $ArrayType$$$BY08N + 24) = PrimaryVector.j;
            //             *(double*)(ref $ArrayType$$$BY08N + 48) = PrimaryVector.k;
            //             *(double*)(ref $ArrayType$$$BY08N + 16) = LVector2.i;
            //             *(double*)(ref $ArrayType$$$BY08N + 40) = LVector2.j;
            //             *(double*)(ref $ArrayType$$$BY08N + 64) = LVector2.k;
            //             *(double*)(ref $ArrayType$$$BY08N + 8) = 0.0 - LVector4.i;
            //             *(double*)(ref $ArrayType$$$BY08N + 32) = 0.0 - LVector4.j;
            //             *(double*)(ref $ArrayType$$$BY08N + 56) = 0.0 - LVector4.k;
            //             long num = 0L;
            //             double[] rotMatrix = RotMatrix;
            //$ArrayType$$$BY08N* ptr = &$ArrayType$$$BY08N;
            //             do
            //             {
            //                 int num2 = *(int*)(num * 4 + ref $ArrayType$$$BY02H);
            //	$ArrayType$$$BY08N* ptr2 = ptr;
            //                 uint num3 = 3u;
            //                 do
            //                 {
            //                     rotMatrix[num2] = *(double*)ptr2;
            //                     num2 += 3;
            //                     ptr2 = ($ArrayType$$$BY08N *)((long)(IntPtr)ptr2 + 24);
            //                     num3 = (uint)((int)num3 + -1);
            //                 }
            //                 while (num3 != 0);
            //                 num++;
            //                 ptr = ($ArrayType$$$BY08N *)((long)(IntPtr)ptr + 8);
            //             }
            //             while (num < 3);
            //             return true;
            //         }
            return false;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public unsafe bool AxisAlign(int PrimaryAxis, LVector PrimaryVector)
        {
            //IL_01e3: Expected I, but got I8
            //IL_01f3: Expected I, but got I8
            //          if ((uint)PrimaryAxis <= 2u && (byte)((PrimaryVector.GetLength3D() < 1E-08) ? 1 : 0) == 0 && !(Math.Abs(PrimaryVector.GetLength3D() - 1.0) > 1E-08))
            //          {
            //	$ArrayType$$$BY02H $ArrayType$$$BY02H;
            //              *(int*)(&$ArrayType$$$BY02H) = PrimaryAxis % 3;
            //              *(int*)(ref $ArrayType$$$BY02H + 4) = (PrimaryAxis + 1) % 3;
            //              *(int*)(ref $ArrayType$$$BY02H + 8) = (PrimaryAxis + 2) % 3;
            //              PrimaryVector = PrimaryVector.Unit();
            //	temp temp;
            //              *(double*)(&temp) = PrimaryVector.i;
            //              *(double*)(ref temp + 8) = PrimaryVector.j;
            //              *(double*)(ref temp + 16) = PrimaryVector.k;
            //	$ArrayType$$$BY08N $ArrayType$$$BY08N;
            //              *(double*)(&$ArrayType$$$BY08N) = *(double*)((long)(*(int*)(&$ArrayType$$$BY02H)) *8L + ref temp);
            //              *(double*)(ref $ArrayType$$$BY08N + 24) = *(double*)((long)(*(int*)(ref $ArrayType$$$BY02H + 4)) *8L + ref temp);
            //              *(double*)(ref $ArrayType$$$BY08N + 48) = *(double*)((long)(*(int*)(ref $ArrayType$$$BY02H + 8)) *8L + ref temp);
            //              double num = *(double*)(&$ArrayType$$$BY08N) +1.0;
            //              if (Math.Abs(num) <= 4.4408920985006262E-16)
            //              {
            //                  *(double*)(ref $ArrayType$$$BY08N + 8) = 0.0;
            //		*(double*)(ref $ArrayType$$$BY08N + 32) = -1.0;
            //		*(double*)(ref $ArrayType$$$BY08N + 56) = 0.0;
            //		*(double*)(ref $ArrayType$$$BY08N + 16) = 0.0;
            //		*(double*)(ref $ArrayType$$$BY08N + 40) = 0.0;
            //		*(double*)(ref $ArrayType$$$BY08N + 64) = 1.0;
            //	}
            //	else
            //	{
            //		*(double*)(ref $ArrayType$$$BY08N + 8) = 0.0 - *(double*)(ref $ArrayType$$$BY08N + 24);
            //		*(double*)(ref $ArrayType$$$BY08N + 32) = *(double*)(ref $ArrayType$$$BY08N + 48) * *(double*)(ref $ArrayType$$$BY08N + 48) / num + *(double*)(&$ArrayType$$$BY08N);
            //		double num2 = *(double*)(ref $ArrayType$$$BY08N + 56) = 0.0 - *(double*)(ref $ArrayType$$$BY08N + 48) * *(double*)(ref $ArrayType$$$BY08N + 24) / num;
            //		*(double*)(ref $ArrayType$$$BY08N + 16) = 0.0 - *(double*)(ref $ArrayType$$$BY08N + 48);
            //		*(double*)(ref $ArrayType$$$BY08N + 40) = num2;
            //		*(double*)(ref $ArrayType$$$BY08N + 64) = *(double*)(ref $ArrayType$$$BY08N + 24) * *(double*)(ref $ArrayType$$$BY08N + 24) / num + *(double*)(&$ArrayType$$$BY08N);
            //	}
            //	long num3 = 0L;
            //	double[] rotMatrix = RotMatrix;
            //	$ArrayType$$$BY08N* ptr = &$ArrayType$$$BY08N;
            //	do
            //	{
            //		long num4 = 0L;
            //		int num5 = *(int*)(num3 * 4 + ref $ArrayType$$$BY02H);
            //		$ArrayType$$$BY08N* ptr2 = ptr;
            //		do
            //		{
            //			rotMatrix[*(int*)(num4 * 4 + ref $ArrayType$$$BY02H) * 3 + num5] = *(double*)ptr2;
            //			num4++;
            //			ptr2 = ($ArrayType$$$BY08N*)((long)(IntPtr)ptr2 + 24);
            //		}
            //		while (num4 < 3);
            //		num3++;
            //		ptr = ($ArrayType$$$BY08N*)((long)(IntPtr)ptr + 8);
            //	}
            //	while (num3 < 3);
            //	return true;
            //}
            return false;
        }

        public override string ToString()
        {
            if (RotMatrix == null || Offset == null) return "";

            return $"Matrix \\r\\n " +
                  $"Translate:{Offset[0]},{Offset[1]},{Offset[2]} \\r\\n " +
                  $"RotateX:{RotMatrix[0]},{RotMatrix[1]},{RotMatrix[2]} \\r\\n" +
                  $"RotateY:{RotMatrix[3]},{RotMatrix[4]},{RotMatrix[5]} \\r\\n" +
                  $"RotateZ:{RotMatrix[6]},{RotMatrix[7]},{RotMatrix[8]}";
        }

        /// <summary>
        /// 矩阵乘法
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static LMatrix operator *(LMatrix s1, LMatrix s2)
        {
            LMatrix matrix = new LMatrix();

            matrix = s1.Rotate(s2, false);
            return matrix;
        }
    }
}