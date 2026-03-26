#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IArray
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Models
* 文 件 名:       IArray.cs
* 创建时间:       2021/12/6 20:16:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f037a8df-5343-4de0-98a0-69890c8d3364
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/6 20:16:57
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
    [StructLayout(LayoutKind.Sequential)]
    public struct IArray
    {
        public IntPtr Ptr;

        public int Length;

        public IArray(int[] arr)
        {
            Length = arr.Length;
            Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * Length);
            Marshal.Copy(arr, 0, Ptr, arr.Length);
        }

        public int[] ToArray()
        {
            int size = Marshal.SizeOf(typeof(int)) * Length;
            int[] d = new int[Length];

            if (Length > 0)
            {
                Marshal.Copy(Ptr, d, 0, Length);
            }

            return d;
        }

        public void Clear()
        {
            Length = 0;
            Ptr = IntPtr.Zero;
        }
    }
}
