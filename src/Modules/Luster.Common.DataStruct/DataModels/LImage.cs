#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LImage
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LImage.cs
* 创建时间:       2022/3/16 18:33:18
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      24ef2241-c5e6-44ff-8751-f0e8dc19fe53
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/16 18:33:18
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.DataModels
{
    public class LImage : IDisposable
    {
        /// <summary>
        /// 指针对象
        /// </summary>
        public IntPtr Pointer { get; set; } = IntPtr.Zero;

        /// <summary>
        /// 宽
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 通道
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// 图像位数
        /// </summary>
        public int ByteOfPixel { get; set; }

        /// <summary>
        /// 对象释放
        /// </summary>
        public void Dispose()
        {
            if (Pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
            }
        }
    }
}