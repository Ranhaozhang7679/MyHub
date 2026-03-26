#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Gui2DNativeAPI
* 机器名称:       L05960-02
* 命名空间:       Luster.ThreeD.Algorithm.NativeMethods
* 文 件 名:       Gui2DNativeAPI.cs
* 创建时间:       2022/12/19 11:03:00
* 作    者:       L05960
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yonglv@lusterinc.com 
* 唯一标识：      34a2571f-a5bb-4068-8421-5b3c769241cd
* 登录用户:       yonglv
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:       2022/12/19 11:03:00
* 修 改 人:       L05960
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.NativeMethods
{
    public class Gui2DNativeAPI
    {
        public const string DllPath = "Algorithm\\Luster2DAlgorithm.dll";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CreateImageWin(IntPtr hWnd, int x, int y, int w, int h);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ResizeImageWin(int w, int h);

        /// <summary>
        /// 
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void DisposeImageWin();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ShowGrayImage(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="isContinuedMem"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ShowGrayStream(IntPtr buffer, int width, int height, bool isContinuedMem);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ShowRgbImage(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="isContinuedMem"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ShowRgbStream(IntPtr buffer, int width, int height, bool isContinuedMem);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ShowRangeImage(string path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="xResolution"></param>
        /// <param name="yResolution"></param>
        /// <param name="zResolution"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="zOffset"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ShowRangeStream(IntPtr buffer, int width, int height, double xResolution, double yResolution, double zResolution, double xOffset, double yOffset, double zOffset);

        /// <summary>
        /// 
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void UpdateDisplay();
    }
}