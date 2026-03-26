#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VTransform
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VTransform.cs
* 创建时间:       2022/2/15 15:34:34
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      1b99d9af-943d-480d-bae1-a4ab8ee71d13
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/15 15:34:34
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm.Core
{
    /// <summary>
    /// 仿射变换
    /// </summary>
    public class VTransform : LDisposable, ITransform, ISave
    {
        /// <summary>
        /// 矩阵信息 4 x 4 16个数字
        /// </summary>
        public double[] Matrix { get; set; }

        /// <summary>
        /// 之前的矩阵对象
        /// </summary>
        public VTransform Header { get; set; }

        /// <summary>
        /// 获取矩阵
        /// </summary>
        /// <returns></returns>
        public LMatrix GetMatrix()
        {
            LMatrix matrix = new LMatrix(Matrix);
            if (Header != null)
            {
                var header = Header.GetMatrix().ToArray();
                NativeAPI.MatrixCross(ref Matrix[0], ref header[0], out matrix);
            }

            return matrix;
        }

        /// <summary>
        /// 获取逆矩阵
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public double[] GetInvert()
        {
            NativeAPI.InvertTransform(ref Matrix[0], out var invMatrix);

            return new double[16]
            {
                invMatrix.RotMatrix[0],invMatrix.RotMatrix[1],invMatrix.RotMatrix[2],invMatrix.Offset[0],
                invMatrix.RotMatrix[3],invMatrix.RotMatrix[4],invMatrix.RotMatrix[5],invMatrix.Offset[1],
                invMatrix.RotMatrix[6],invMatrix.RotMatrix[7],invMatrix.RotMatrix[8],invMatrix.Offset[2],
                0,0,0,1
            };
        }

        public VTransform() : base(IntPtr.Zero)
        {
            Matrix = new double[16]
            {
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                0,0,0,1
            };
        }

        public VTransform(double[] m) : this()
        {
            Matrix = new double[16]
            {
                m[0],m[1],m[2],m[3],
                m[4],m[5],m[6],m[7],
                m[8],m[9],m[10],m[11],
                m[12],m[13],m[14],m[15],
            };
        }

        public VTransform(LMatrix lMatrix) : this()
        {
            Matrix[0] = lMatrix.RotMatrix[0];
            Matrix[1] = lMatrix.RotMatrix[1];
            Matrix[2] = lMatrix.RotMatrix[2];

            Matrix[4] = lMatrix.RotMatrix[3];
            Matrix[5] = lMatrix.RotMatrix[4];
            Matrix[6] = lMatrix.RotMatrix[5];

            Matrix[8] = lMatrix.RotMatrix[6];
            Matrix[9] = lMatrix.RotMatrix[7];
            Matrix[10] = lMatrix.RotMatrix[8];

            Matrix[3] = lMatrix.Offset[0];
            Matrix[7] = lMatrix.Offset[1];
            Matrix[11] = lMatrix.Offset[2];
        }

        /// <summary>
        /// 对象Clone
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            VTransform vTransform = new VTransform(Matrix);
            return vTransform;
        }

        /// <summary>
        /// 对矩阵进行转置
        /// </summary>
        /// <returns></returns>
        public VTransform Transcope()
        {
            // 将行变成列
            double[] transcope = new double[16];
            transcope[0] = Matrix[0];
            transcope[1] = Matrix[4];
            transcope[2] = Matrix[8];
            transcope[3] = Matrix[12];

            transcope[4] = Matrix[1];
            transcope[5] = Matrix[5];
            transcope[6] = Matrix[9];
            transcope[7] = Matrix[13];

            transcope[8] = Matrix[2];
            transcope[9] = Matrix[6];
            transcope[10] = Matrix[10];
            transcope[11] = Matrix[14];

            transcope[12] = Matrix[3];
            transcope[13] = Matrix[7];
            transcope[14] = Matrix[11];
            transcope[15] = Matrix[15];

            return new VTransform(transcope);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override bool IsEmpty()
        {
            return Matrix == null;
        }

        /// <summary>
        /// 矩阵对象
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Transform";
        }

        #region 对象保存

        /// <summary>
        /// 旋转矩阵
        /// </summary>
        public override string[] SaveFormat => new string[] { ".calib" };

        /// <summary>
        /// 文件保存
        /// </summary>
        /// <param name="path"></param>
        public override void Save(string path)
        {
            var matrix = GetMatrix();
            var matrixArr = matrix.ToArray();

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < 4; i++)
                {
                    int start = i * 4;
                    sw.WriteLine($"{matrixArr[start]},{matrixArr[start + 1]},{matrixArr[start + 2]},{matrixArr[start + 3]}");
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="path"></param>
        public static VTransform ReadTxt(string path)
        {
            double[] matrix = new double[16];
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                int num = 0;
                while (true)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }
                    var chars = new char[] { ',', ' ', ';' };
                    if (line.Split(chars).Length < 4) continue;

                    double[] vals = line.Split<double>(chars);
                    for (int i = 0; i < vals.Length; i++)
                    {
                        matrix[num++] = vals[i];
                    }
                }
            }

            return new VTransform(matrix);
        }

        #endregion
    }
}