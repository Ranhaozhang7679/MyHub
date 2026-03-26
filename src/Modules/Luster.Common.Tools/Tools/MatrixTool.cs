#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MatrixTool
* 机器名称:       L05014-NB
* 命名空间:       Luster.Common.Tools.Tools
* 文 件 名:       MatrixTool.cs
* 创建时间:       2022/8/16 14:03:46
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      7a12ff4b-8472-4065-b5d5-18439fb850a3
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/16 14:03:46
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools.Tools
{
    public class MatrixTool
    {
        /// <summary>
        /// 一维数组转二维数组
        /// </summary>
        /// <param name="oneArray">一维数组</param>
        /// <param name="row">目标行数</param>
        /// <returns></returns>
        public static double[,] ArrayOneToDouble(double[] oneArray, int row)
        {
            if (oneArray.Length % row != 0)
                return null;
            int col = oneArray.Length / row;
            double[,] twoArray = new double[row, col];
            for (int i = 0; i < oneArray.Length; i++)
            {
                twoArray[i / col, i % col] = oneArray[i];
            }
            return twoArray;
        }

        /// <summary>
        /// 二维数组转一维数组
        /// </summary>
        /// <param name="twoArray"></param>
        /// <returns></returns>
        public static double[] ArrayDoubleToOne(double[,] twoArray)
        {
            if (twoArray.Length==0)
            { 
                 return null;
            }
            int row = twoArray.GetLength(0);
            int col = twoArray.GetLength(1);
            double[] oneArray = new double[row* col];
            int k = 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    oneArray[k++] = twoArray[i, j];
                }            
            }
            return oneArray;
        }

        /// <summary>
        /// 求行列式的代数余子式矩阵
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double[,] GetCofactorOfDeterminantAlgebra(double[,] x, int a)
        {
            double[,] temp;
            double[,] result = new double[a, a];
            //i,m两个for循环对x矩阵遍历，求代数余子式
            for (int i = 0; i < a; i++)
            {
                for (int m = 0; m < a; m++)
                {
                    //生成余子式数组 
                    temp = new double[a - 1, a - 1];
                    //j为余子式列，i为行
                    for (int j = 0; j < a - 1; j++)
                        for (int k = 0; k < a - 1; k++)
                        {
                            //判断构造的元素在去掉的列前面还是后面  行的上面还是下面 
                            if (j < i && k < m)
                                temp[k, j] = x[k, j];
                            if (j < i && k >= m)
                                temp[k, j] = x[k + 1, j];
                            if (j >= i && k < m)
                                temp[k, j] = x[k, j + 1];
                            if (j >= i && k >= m)
                                temp[k, j] = x[k + 1, j + 1];
                        }
                    //计算余子式的符号 
                    double s = Math.Pow(-1, i + m);
                    //得代数余子式的一项
                    result[i, m] = s * GetMatrixDeterminant(temp, a - 1);
                }
            }
            return result;
        }

        /// <summary>
        /// 行列式的值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double GetMatrixDeterminant(double[,] x, int a)
        {
            //声明临时矩阵数组 
            double[,] temp;
            double s = 1.0;
            //用他来控制余子式的符号,声明临时存储矩阵行列式变量和符号变量  
            double result = 0.0;
            if (a == 1)
            {
                return x[0, 0] * s;
            }
            for (int i = 0; i < a; i++)
            {
                //给余子式数组分配空间 
                temp = new double[a - 1, a - 1];
                //j为余子式列，i为行
                for (int j = 0; j < a - 1; j++)
                    for (int k = 0; k < a - 1; k++)
                    {
                        //判断构造的元素在去掉的列前面还是后面 
                        if (j < i)
                            temp[k, j] = x[k + 1, j];
                        else
                            temp[k, j] = x[k + 1, j + 1];
                    }
                //计算余子式的符号
                s = Math.Pow(-1, i);
                //用递归算法计算行列式的值  
                result += x[0, i] * GetMatrixDeterminant(temp, a - 1) * s;
            }
            return result;
        }

        /// <summary>
        /// 矩阵求逆   
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double[,] MatrixInversion(double[,] x)
        {
            int row = x.GetLength(0);
            int col = x.GetLength(1);
            //用来存放代数余子式矩阵 
            double[,] result1 = new double[row, col];
            //用来存放求逆后的矩阵 
            double[,] result2 = new double[row, col];                
            //行列大小不相等，不能求逆矩阵
            if (row != col)
                return x;
            else
            {
                double m;
                //求出行列式的模长，进行下一步的判断 
                m = GetMatrixDeterminant(x, row);
                //判断是否有逆矩阵 ，输入矩阵的模长值为0，所以它没有逆矩阵
                if (m == 0)                                
                    return x;
                else
                {
                    //代数余子式 
                    result1 = GetCofactorOfDeterminantAlgebra(x, row);                                
                    double n = 1 / m;
                    //一一求出逆矩阵的每一项
                    for (int i = 0; i < row; i++)              
                        for (int j = 0; j < row; j++)
                            result2[i, j] = n * result1[i, j];
                }
            }
            return result2;
        }

        /// <summary>
        /// 矩阵·乘法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double[,] AmultiplyB(double[,] a, double[,] b)
        {
            int rowa = a.GetLength(0);
            int cola = a.GetLength(1);
            int rowb = b.GetLength(0);
            int colb = b.GetLength(1);
            double[,] result = new double[rowa, colb];

            for (int i = 0; i < rowa; i++)
            {
                for (int k = 0; k < colb; k++)
                {
                    double c = 0;
                    for (int j = 0; j < cola; j++)
                    {
                        c = a[i, j] * b[j, k] + c;
                    }
                    result[i, k] = c;
                }
            }
            return result;
        }
        /// <summary>
        /// 矩阵转置 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] MatrixT(double[,] matrix)
        {
            int row = matrix.GetLength(0);
            int col = matrix.GetLength(1);
            //用于存放转置后的结果    
            double[,] result = new double[col, row];
            for (int i = 0; i < row; i++)
                for (int j = 0; j < col; j++)
                    //第a行b列与第b行a列交换，实现转置 
                    result[j, i] = matrix[i, j];
            return result;
        }      
    }
}