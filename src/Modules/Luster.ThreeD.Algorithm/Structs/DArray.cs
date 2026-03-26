#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DArray
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Models
* 文 件 名:       DArray.cs
* 创建时间:       2021/11/23 19:10:57
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5dc624c9-2b37-46d7-8c7c-a062f89aaf94
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/23 19:10:57
* 修 改 人:		  luster
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
    /// <summary>
    /// Double数组
    /// </summary>
    public struct DArray : IDisposable
    {
        public IntPtr Ptr { get; set; }

        public int Length { get; set; }

        public void Dispose()
        {
            Length = 0;
        }

        public double[] ToArray()
        {
            int size = Marshal.SizeOf(typeof(double)) * Length;
            double[] d = new double[Length];

            if (Length > 0)
                Marshal.Copy(Ptr, d, 0, Length);

            return d;
        }
    }
}
