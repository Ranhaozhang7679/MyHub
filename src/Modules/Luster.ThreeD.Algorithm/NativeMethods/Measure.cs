#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Measure
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.NativeMethods
* 文 件 名:       Measure.cs
* 创建时间:       2022/1/27 16:05:44
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      f423fbb0-0a71-4db9-b6d7-030a6b8f24e0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/27 16:05:44
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm
{
    public partial class NativeAPI
    {
        #region 1---距离测量

        /// <summary>
        /// 计算三维点到三维点之间的欧式距离
        /// </summary>
        /// <param name="in_position1">点1</param>
        /// <param name="in_position2">点2</param>
        /// <param name="out_distance">输出距离</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalDistanceFromPoint2Point(LPoint3 in_position1, LPoint3 in_position2, out double out_distance, out IntPtr msgPtr);

        /// <summary>
        /// 基于相对坐标系计算三维点到三维点之间的距离
        /// </summary>
        /// <param name="inPos1">点1</param>
        /// <param name="inPos2">点2</param>
        /// <param name="distanceType">计算距离类型 0-欧式距离 1-X轴向距离 2-Y轴向距离 3-Z轴向距离</param>
        /// <param name="isRelativeCoord">输入是否是相对坐标系下的点坐标值</param>
        /// <param name="coord">相对坐标系</param>
        /// <param name="out_distance">输出距离</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalDistance2PointByCoord(LPoint3 inPos1, LPoint3 inPos2, int distanceType, bool isRelativeCoord, LCoord coord, out double out_distance, out IntPtr msgPtr);

        /// <summary>
        /// 计算三维点到三维线之间的距离
        /// </summary>
        /// <param name="in_position">点</param>
        /// <param name="in_line">线</param>
        /// <param name="out_distance">输出距离</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalDistanceFromPoint2Line(LPoint3 in_position, IntPtr in_line, out double out_distance, out IntPtr msgPtr);

        /// <summary>
        /// 计算三维点到三维面之间的距离
        /// </summary>
        /// <param name="in_position">点</param>
        /// <param name="in_plane">面</param>
        /// <param name="out_distance">输出距离</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalDistanceFromPoint2Plane(LPoint3 in_position, IntPtr in_plane, out double out_distance, out IntPtr msgPtr);

        /// <summary>
        /// 计算三维直线到三维直线之间的距离
        /// </summary>
        /// <param name="in_line1">线1</param>
        /// <param name="in_line2">线2</param>
        /// <param name="out_distance">输出距离</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalDistanceFromLine2Line(IntPtr in_line1, IntPtr in_line2, out double out_distance, out IntPtr msgPtr);

        /// <summary>
        /// 计算三维直线到三维面之间的距离
        /// </summary>
        /// <param name="in_line">线</param>
        /// <param name="in_plane">面</param>
        /// <param name="out_distance">输出距离</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalDistanceFromLine2Plane(IntPtr in_line, IntPtr in_plane, out double out_distance, out IntPtr msgPtr);

        #endregion

        #region 2---角度测量

        /// <summary>
        /// 计算三维直线到三维直线之间的夹角
        /// </summary>
        /// <param name="in_line1">线1</param>
        /// <param name="in_line2">线2</param>
        /// <param name="out_angle">输出角度</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalAngleFromLineAndLine(IntPtr in_line1, IntPtr in_line2, out double out_angle, out IntPtr msgPtr);

        /// <summary>
        /// 计算三维直线和三维平面之间的夹角
        /// </summary>
        /// <param name="in_line">线</param>
        /// <param name="in_plane">面</param>
        /// <param name="out_angle">输出角度</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalAngleFromLineAndPlane(IntPtr in_line, IntPtr in_plane, out double out_angle, out IntPtr msgPtr);

        /// <summary>
        /// 计算三维平面和三维平面之间的夹角
        /// </summary>
        /// <param name="in_plane1">面1</param>
        /// <param name="in_plane2">面2</param>
        /// <param name="out_angle">输出角度</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalAngleFromPlaneAndPlane(IntPtr in_plane1, IntPtr in_plane2, out double out_angle, out IntPtr msgPtr);

        #endregion
    }
}