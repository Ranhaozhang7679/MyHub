#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LPointArray
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Structs
* 文 件 名:       LPointArray.cs
* 创建时间:       2022/3/7 17:11:54
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      bec7cf8e-6559-469d-8e56-ea0c9f139989
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/7 17:11:54
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LPointArray
    {
        /// <summary>
        /// 点长度
        /// </summary>
        public int Length;

        /// <summary>
        /// 点对象数组
        /// </summary>
        public IntPtr PArray;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vVectors"></param>
        public LPointArray(params VVector[] vVectors)
        {
            Length = vVectors.Length;
            PArray = IntPtr.Zero;

            int size = Marshal.SizeOf(typeof(LPoint3)) * Length;

            // 将对象赋值到指针对象中
            LPoint3[] ps = new LPoint3[size];
            for (int i = 0; i < Length; i++)
            {
                ps[i] = new LPoint3(vVectors[i].X, vVectors[i].Y, vVectors[i].Z);
            }

            // 构建指针对象
            PArray = Marshal.UnsafeAddrOfPinnedArrayElement(ps, 0);
        }
    }
}
