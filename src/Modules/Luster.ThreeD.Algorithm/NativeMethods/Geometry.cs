#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NativeAPI
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.NativeMethods
* 文 件 名:       NativeAPI.cs
* 创建时间:       2022/1/20 16:12:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      6c3e1925-9a41-4ab8-ab71-b5648d40553a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/20 16:12:22
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
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
        #region 1---获取对象点
        /// <summary>
        /// 获取点云中心
        /// </summary>
        /// <param name="cloudPtr">输入点云</param>
        /// <param name="indexes">数据索引，为空时表示所有点云参与计算</param>
        /// <param name="out_point">中心坐标</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointCloudCenterPoint(IntPtr cloudPtr, IArray indexes, out LPoint3 out_point);

        /// <summary>
        /// 获点云的包围盒
        /// </summary>
        /// <param name="actorID">基元GUID</param>
        /// <param name="cloud">点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与计算</param>
        /// <param name="boxType"> 包围盒类型  0-近似最小体积包围盒（速度最慢）  1-主方向下的最小包围盒  2-轴向平行包围盒，效率最高</param>
        /// <param name="vis">是否显示</param>
        /// <param name="boxLength">包围盒长</param>
        /// <param name="boxWidth">包围盒宽</param>
        /// <param name="boxHeight">包围盒高</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>执行结果 0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetBoundingBox(string actorID, IntPtr cloud, IArray indexes, int boxType,bool vis, out double boxLength, out double boxWidth, out double boxHeight, out IntPtr errMsg);

        /// <summary>
        /// 圆心点
        /// </summary>
        /// <param name="in_circle">圆指针对象</param>
        /// <param name="out_point">圆心坐标</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetCircleCenterPoint(IntPtr in_circle, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 圆锥顶点
        /// </summary>
        /// <param name="in_cone">圆锥对象</param>
        /// <param name="out_point">圆锥顶点坐标</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetConeVertex(IntPtr in_cone, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 圆柱端点获取
        /// </summary>
        /// <param name="in_cylinder">圆柱指针对象</param>
        /// <param name="out_point">输出端点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetCylinderEndPoint(IntPtr in_cylinder, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 矩形中心点
        /// </summary>
        /// <param name="in_rectangular">长方形指针对象</param>
        /// <param name="out_point">输出中心点信息</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetRectangularCenterPoint(IntPtr in_rectangular, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 球心
        /// </summary>
        /// <param name="in_ball">球指针对象</param>
        /// <param name="out_point">输出中心点信息</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetBallCenterPoint(IntPtr in_ball, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 构建偏移点
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="direction">方向</param>
        /// <param name="offset">偏移量</param>
        /// <param name="outOffsetPoint">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetOffsetPointsByPointAndDir(LPoint3 startPoint, LVector direction, double offset, out LPoint3 outOffsetPoint, out IntPtr errMsg);

        #endregion

        #region 1---两几何相交点

        /// <summary>
        /// 空间两线相交获取交点
        /// </summary>
        /// <param name="in_line1">直线1</param>
        /// <param name="in_line2">直线2</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByTwoIntersectLineSegment(IntPtr in_line1, IntPtr in_line2, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 三维线和三维平面的交点
        /// </summary>
        /// <param name="in_line">直线1</param>
        /// <param name="in_plane">直线2</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByLineSegmentPlaneIntersect(IntPtr in_line, IntPtr in_plane, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 三个三维平面的交点
        /// </summary>
        /// <param name="in_plane1">面1</param>
        /// <param name="in_plane2">面2</param>
        /// <param name="in_plane3">面3</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByThreePlanesIntersect(IntPtr in_plane1, IntPtr in_plane2, IntPtr in_plane3, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 两圆锥轴相交点
        /// </summary>
        /// <param name="in_cone1">圆锥1</param>
        /// <param name="in_cone2">圆锥2</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByTwoConicalShaft(IntPtr in_cone1, IntPtr in_cone2, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 两圆柱轴相交点
        /// </summary>
        /// <param name="in_cylinder1">圆柱1</param>
        /// <param name="in_cylinder2">圆柱2</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByTwoCylindricalShaft(IntPtr in_cylinder1, IntPtr in_cylinder2, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        ///  平面和圆锥轴线交点
        /// </summary>
        /// <param name="in_plane">平面</param>
        /// <param name="in_cone">圆锥</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByPlaneAndConicalShaft(IntPtr in_plane, IntPtr in_cone, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 平面和圆柱轴线交点
        /// </summary>
        /// <param name="in_plane">平面</param>
        /// <param name="in_cylinder">圆柱</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByPlaneAndCylindricalShaft(IntPtr in_plane, IntPtr in_cylinder, out LPoint3 out_point, out IntPtr errMsg);

        #endregion

        #region 1---几何到几何投影点

        /// <summary>
        /// 三维点到三维直线上投影
        /// </summary>
        /// <param name="in_point">点</param>
        /// <param name="in_line">线</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                 SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByPoint2LineSegment(LPoint3 in_point, IntPtr in_line, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 三维点到三维平面上投影
        /// </summary>
        /// <param name="in_point">点</param>
        /// <param name="in_plane">面</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByPoint2Plane(LPoint3 in_point, IntPtr in_plane, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 三维点投影到圆锥轴
        /// </summary>
        /// <param name="in_point">点</param>
        /// <param name="in_cone">圆锥</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByPoint2ConicalShaft(LPoint3 in_point, IntPtr in_cone, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 三维点投影到圆柱轴
        /// </summary>
        /// <param name="in_point">点</param>
        /// <param name="in_cylinder">圆柱</param>
        /// <param name="out_point">输出点坐标</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointByPoint2CylindricalShaft(LPoint3 in_point, IntPtr in_cylinder, out LPoint3 out_point, out IntPtr errMsg);

        #endregion

        #region 2---线段特征

        /// <summary>
        /// 获取线段两端点信息
        /// </summary>
        /// <param name="line">线</param>
        /// <param name="start">输出-起点</param>
        /// <param name="end">输出-终点</param>
        /// <param name="direction">输出-方向</param>
        /// <param name="len">输出长度</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineStartAndEnd(IntPtr line, out LPoint3 start, out LPoint3 end, out LVector direction, out double len);

        /// <summary>
        /// 根据点和方向构建线
        /// </summary>
        /// <param name="in_position">起点</param>
        /// <param name="in_direction">方向</param>
        /// <param name="len">长度</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByPosDir(LPoint3 in_position, LVector in_direction, double len, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 用起始点point1和终止点point2来构造直线。若两点为同一点则返回false，否则返回true
        /// </summary>
        /// <param name="in_point1">起点</param>
        /// <param name="in_point2">终点</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByStartEndPoints(LPoint3 in_point1, LPoint3 in_point2, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 根据直线所经过点的位置坐标position、直线倾斜角度radTilt与直线旋转角度radRotatio构造直线
        /// note: 倾斜角radTilt范围为：[0, π] ;旋转角radRotation范围为：[0, 2π)
        /// </summary>
        /// <param name="in_position">起点</param>
        /// <param name="in_radTilt"> 线段倾斜角度。 范围为：[0, π]</param>
        /// <param name="in_radRotation">线段旋转角度。 范围为：[0, 2π)</param>
        /// <param name="len">线段长度</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByPositionTiltRotation(LPoint3 in_position, double in_radTilt, double in_radRotation, double len, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 根据给定线段、方向和偏移构造平行线段
        /// </summary>
        /// <param name="in_line">线</param>
        /// <param name="in_direction">偏移方向</param>
        /// <param name="dOffset">偏移量</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetParallelLineSegmentByLineSegmentDirPos(IntPtr in_line, LVector in_direction, double dOffset, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 根据给定线段、起始点和是否同向标志构造平行线段
        /// </summary>
        /// <param name="in_line">线</param>
        /// <param name="in_position">起始点</param>
        /// <param name="bSyntropy">true时线段与当前线段同向，反之异向。默认同向</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetParallelLineSegmentBySign(IntPtr in_line, LPoint3 in_position, bool bSyntropy, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 根据给定线段和起始点构造垂线段
        /// </summary>
        /// <param name="in_line">线</param>
        /// <param name="in_point">起始点</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetVerticalLineSegmentByLineSegmentAndPos(IntPtr in_line, LPoint3 in_point, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 根据给定平面和一点构造垂直线段
        /// </summary>
        /// <param name="in_plane">面</param>
        /// <param name="in_point">点</param>
        /// <param name="out_lineSegment">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetVerticalLineSegmentByPlaneAndPos(IntPtr in_plane, LPoint3 in_point, out IntPtr out_lineSegment, out IntPtr errMsg);

        /// <summary>
        /// 圆锥轴线
        /// </summary>
        /// <param name="in_cone">圆锥指针对象</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByConicalShaft(IntPtr in_cone, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 圆柱轴线
        /// </summary>
        /// <param name="in_cylinder">圆柱</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByCylindricalShaft(IntPtr in_cylinder, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 三维平面与三维平面的交线
        /// </summary>
        /// <param name="inPlane1">平面1</param>
        /// <param name="inPlane2">平面2</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByPlanePlaneIntersect(IntPtr inPlane1, IntPtr inPlane2, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 三维线段到三维平面上的投影
        /// </summary>
        /// <param name="in_line">线</param>
        /// <param name="in_plane">面</param>
        /// <param name="out_line">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByLineSegment2Plane(IntPtr in_line, IntPtr in_plane, out IntPtr out_line, out IntPtr errMsg);

        /// <summary>
        /// 直线拟合
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与拟合</param>
        /// <param name="fitMethod">直线拟合方法。 0-最小二乘  1-带局外点  2-最小包容</param>
        /// <param name="planeProjectionType">投影类型。 0-不指定投影平面   1-指定投影平面</param>
        /// <param name="plane">投影平面</param>
        /// <param name="lineDirection">直线方向。 0-Z轴正方向  1-轴负方向</param>
        /// <param name="dDisThre">距离阈值  拟合方式设定为1-带局外点时有效</param>
        /// <param name="confidence">置信度  拟合方式设定为1-带局外点时有效</param>
        /// <param name="outlierRatio">局外点比例  拟合方式设定为1-带局外点时有效</param>
        /// <param name="sampleHalfWidth">采样半宽  拟合方式设定为1-带局外点时有效</param>
        /// <param name="lockDir">方向约束。0:不约束  1:X正方向 2:X负方向 3:Y正方向 4:Y负方向 5: Z正方向 6: Z负方向</param>
        /// <param name="outLine">拟合直线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int LineSegmentFit(IntPtr pCloud, IArray indexes, int fitMethod, int planeProjectionType, IntPtr plane, int lineDirection, double dDisThre, double confidence, double outlierRatio, double sampleHalfWidth, int lockDir, out IntPtr outLine, out IntPtr errMsg);

        /// <summary>
        /// 获取直线方向向量
        /// </summary>
        /// <param name="in_line">线</param>
        /// <param name="out_dirVector">方向向量</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentDirVector(IntPtr in_line, out LVector out_dirVector);

        /// <summary>
        /// 线段平移
        /// </summary>
        /// <param name="in_lineSegment">线</param>
        /// <param name="OffsetX">X轴平移量</param>
        /// <param name="OffsetY">Y轴平移量</param>
        /// <param name="OffsetZ">Z轴平移量</param>
        /// <param name="out_lineSegment">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByTranslation(IntPtr in_lineSegment, double OffsetX, double OffsetY, double OffsetZ, out IntPtr out_lineSegment, out IntPtr errMsg);

        /// <summary>
        /// 线段旋转
        /// </summary>
        /// <param name="in_lineSegment">线</param>
        /// <param name="in_point">旋转轴线上的一点</param>
        /// <param name="in_plane">与旋转轴线垂直的平面</param>
        /// <param name="radRotation">旋转角度（弧度制）</param>
        /// <param name="out_lineSegment">输出线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineSegmentByRotate(IntPtr in_lineSegment, LPoint3 in_point, IntPtr in_plane, double radRotation, out IntPtr out_lineSegment, out IntPtr errMsg);

        #endregion

        #region 3---面特征

        /// <summary>
        /// 根据平面所经过点的位置坐标和法向量构造平面
        /// </summary>
        /// <param name="in_position">点位置</param>
        /// <param name="in_normal">法线方向</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByPosNormal(LPoint3 in_position, LVector in_normal, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据平面法向量和偏移量构造平面
        /// </summary>
        /// <param name="in_normal">法向量</param>
        /// <param name="dOffset">偏移量</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByNormal(LVector in_normal, double dOffset, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据一点和两个方向构造平面
        /// </summary>
        /// <param name="in_point">平面上的一点</param>
        /// <param name="in_dir1">方向1</param>
        /// <param name="in_dir2">方向2</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByPointAnd2Direction(LPoint3 in_point, LVector in_dir1, LVector in_dir2, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 用一般方程 aX + bY + cZ +d = 0 的系数构造平面
        /// </summary>
        /// <param name="a">平面一般方程X系数</param>
        /// <param name="b">平面一般方程Y系数</param>
        /// <param name="c">平面一般方程Z系数</param>
        /// <param name="d">平面一般方程常量</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByCoefficients(double a, double b, double c, double d, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 3点构建平面
        /// </summary>
        /// <param name="in_position1">点1</param>
        /// <param name="in_position2">点2</param>
        /// <param name="in_position3">点3</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByTreePoints(LPoint3 in_position1, LPoint3 in_position2, LPoint3 in_position3, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据平面所经过点的位置和经过的直线构造平面
        /// </summary>
        /// <param name="in_position">点</param>
        /// <param name="in_line">线</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByPointAndLineSegment(LPoint3 in_position, IntPtr in_line, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据平面所经过点的位置坐标和与之垂直的直线构造平面
        /// </summary>
        /// <param name="in_position">平面上的点</param>
        /// <param name="in_line">与平面垂直的线</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetVerticalPlaneByPointAndLineSegment(LPoint3 in_position, IntPtr in_line, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据给定平面和偏移构造平行面
        /// </summary>
        /// <param name="in_plane">参考平面</param>
        /// <param name="dOffset">偏移量</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetParallePlaneByPlaneOffset(IntPtr in_plane, double dOffset, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据给定平面和两点构造垂直面
        /// </summary>
        /// <param name="in_plane">参考平面</param>
        /// <param name="in_point1">点1</param>
        /// <param name="in_point2">点2</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPerpendicularPlaneByPlaneAndTwoPoint(IntPtr in_plane, LPoint3 in_point1, LPoint3 in_point2, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据给定点和圆锥构造平面
        /// </summary>
        /// <param name="in_cone">垂直于构造平面的圆锥</param>
        /// <param name="in_point">点</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByPointAndCone(IntPtr in_cone, LPoint3 in_point, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据给定点和圆柱构造平面
        /// </summary>
        /// <param name="in_cylinder">垂直于构造平面的圆柱</param>
        /// <param name="in_point">点</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByPointAndCylinder(IntPtr in_cylinder, LPoint3 in_point, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据直线和平面构造平面
        /// </summary>
        /// <param name="in_plane">垂直于构造平面的平面</param>
        /// <param name="in_line">构造平面经过的线段</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByLineSegmentAndPlane(IntPtr in_plane, IntPtr in_line, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据圆构造平面
        /// </summary>
        /// <param name="in_circle">平面上的圆</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByCircle(IntPtr in_circle, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 根据矩形构造平面
        /// </summary>
        /// <param name="in_rectangular">平面上的矩形</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneByRectangular(IntPtr in_rectangular, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 面拟合
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="indexes">数据索引。 为空时表示所有点云数据参与拟合</param>
        /// <param name="fitMethod">平面拟合方法。  0-面积 1-点</param>
        /// <param name="planeDir">平面方向。  0-正向像素空间Z  1-负向像素空间Z</param>
        /// <param name="regionMode">区域形状。 仅在拟合方法为面积模式下有效。 </param>
        /// <param name="lookupMethod">Z查询方法。 仅在拟合方法为点模式下有效。 0-单个像素  1-邻域中值  2-邻域(均值)</param>
        /// <param name="sizeX">邻域尺寸</param>
        /// <param name="sizeY">邻域尺寸</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int PlaneFit(IntPtr pCloud, IArray indexes, int fitMethod, int planeDir, int regionMode, int lookupMethod, long sizeX, long sizeY, out IntPtr outPlane, out IntPtr errMsg);

        /// <summary>
        /// 获取平面法向量
        /// </summary>
        /// <param name="in_plane">参考平面</param>
        /// <param name="out_normalVector">平面法向量</param>
        /// <param name="zVal">平面上点的Z值</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlaneNormal(IntPtr in_plane, out LVector out_normalVector, out double zVal, out IntPtr errMsg);

        /// <summary>
        /// 获取平面上3个点
        /// </summary>
        /// <param name="in_plane">参考面</param>
        /// <param name="out_point1">点1</param>
        /// <param name="out_point2">点2</param>
        /// <param name="out_point3">点3</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPlane3Points(IntPtr in_plane, out LPoint3 out_point1, out LPoint3 out_point2, out LPoint3 out_point3, out IntPtr errMsg);

        //获取拟合面属性 暂时不用
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void GetFitPlaneProperty(IntPtr cloud, IArray indexes, IntPtr in_plane, IntPtr roi, int roiType, out double out_width, out double out_length, out LPoint3 out_centerPoint);

        /// <summary>
        /// 平面平移到指定点得到新的平面
        /// </summary>
        /// <param name="in_plane">参考平面</param>
        /// <param name="destP">指定点</param>
        /// <param name="out_plane">输出面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int MovePlaneToSpecifyPoint(IntPtr in_plane, LPoint3 destP, out IntPtr out_plane, out IntPtr errMsg);


        #endregion

        #region 4---圆特征

        /// <summary>
        /// 数据构建圆
        /// </summary>
        /// <param name="point">圆中心</param>
        /// <param name="dir">圆方向</param>
        /// <param name="radius">圆半径</param>
        /// <param name="circlePtr">输出圆</param>
        /// <returns>0-成功，否则，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GenCircleByData(LPoint3 point, LVector dir, double radius, out IntPtr circlePtr);

        /// <summary>
        /// 拟合圆
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="indexes">数据索引。注：不指定下标时，则全部点执行</param>
        /// <param name="fitMode">拟合方式 0:最小二乘  1:局外点  2-最小包容  3-最小外接  4-最大内接</param>
        /// <param name="ibasePlaneMode">基准面设置方式  0:内部拟合 1:用户自定义</param>
        /// <param name="in_basePlane">基准面。  基准面设置方式为"用户自定义"时设置</param>
        /// <param name="dDisThre">距离阈值  取值（0-1000.0]  (仅限于带局外点时使用)</param>
        /// <param name="dConfidence">置信度  取值 (0-1.0)  (仅限于带局外点时使用)</param>
        /// <param name="dOutlierRatio">局外点比例   取值 (0-1.0) (仅限于带局外点时使用) </param>
        /// <param name="iSampleHalfWidth">采样半宽   取值 [1.0-1000.0] (仅限于带局外点时使用)</param>
        /// <param name="iCloudShape">点云形状   0-圆  1-圆弧或曲线  2-椭圆  3-散乱（仅限于拟合方法为最大内切圆）</param>
        /// <param name="outCircle">输出圆</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
         SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CircleFit(IntPtr pCloud, IArray indexes, int fitMode, int ibasePlaneMode, IntPtr in_basePlane, double dDisThre, double dConfidence, double dOutlierRatio, double iSampleHalfWidth, int iCloudShape, out IntPtr outCircle, out IntPtr errMsg);

        /// <summary>
        /// 获取圆属性
        /// </summary>
        /// <param name="circle">圆</param>
        /// <param name="outCenter">输出圆心</param>
        /// <param name="outDir">输出方向</param>
        /// <param name="raidus">输出半径</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPropertyByCircle(IntPtr circle, out LPoint3 outCenter, out LVector outDir, out double raidus);

        #endregion

        #region 5---圆柱特征

        /// <summary>
        /// 基于点、方向、半径、高度构建圆柱
        /// </summary>
        /// <param name="pos">轴线上一点</param>
        /// <param name="dir">轴线方向</param>
        /// <param name="height">圆柱高度</param>
        /// <param name="radius">圆柱半径</param>
        /// <param name="outCylinder">输出圆柱</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GenCylinderByData(LPoint3 pos, LVector dir, double height, double radius, out IntPtr outCylinder);

        /// <summary>
        /// 圆柱拟合
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与拟合</param>
        /// <param name="outCylinder">输出圆柱</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CylinderFit(IntPtr pCloud, IArray indexes, out IntPtr outCylinder, out IntPtr errMsg);

        /// <summary>
        /// 获取圆柱的起点、终点、高度、和方向信息
        /// </summary>
        /// <param name="cylinder">输入圆柱</param>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="linePtr">轴线</param>
        /// <param name="height">高</param>
        /// <param name="radius">半径</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                  SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetCylinderData(IntPtr cylinder, out LPoint3 start, out LPoint3 end, out IntPtr linePtr, out double height, out double radius);

        /// <summary>
        /// 基于线段构建圆柱
        /// </summary>
        /// <param name="lineSegment">线段</param>
        /// <param name="radius">圆柱半径</param>
        /// <param name="outCylinder">输出圆柱</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                   SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CreateCylinderByLineSegment(IntPtr lineSegment, double radius, out IntPtr outCylinder, out IntPtr msgPtr);

        /// <summary>
        /// 基于点+平面方向构建圆柱
        /// </summary>
        /// <param name="in_plane">圆柱轴线垂直的平面</param>
        /// <param name="point">圆柱轴线上的一点</param>
        /// <param name="radius">圆柱半径</param>
        /// <param name="height">圆柱高度</param>
        /// <param name="outCylinder">输出圆柱</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                    SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CreateCylinderByPlaneAndPoint(IntPtr in_plane, LPoint3 point, double radius, double height, out IntPtr outCylinder, out IntPtr msgPtr);

        #endregion

        #region 6---圆锥特征

        //暂时未实现，弃用
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GenConeByData(LPoint3 vertex, LVector dir, double angle, double height, out IntPtr scVoxelCone);

        /// <summary>
        /// 圆锥拟合
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与拟合</param>
        /// <param name="outCone">输出圆锥</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ConeFit(IntPtr pCloud, IArray indexes, out IntPtr outCone, out IntPtr errMsg);

        #endregion

        #region 7---球体特征

        /// <summary>
        /// 利用球心和半径构造球体
        /// </summary>
        /// <param name="center">球心</param>
        /// <param name="radius">球半径</param>
        /// <param name="outSphere">输出球</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GenSphereByData(LPoint3 center, double radius, out IntPtr outSphere);

        /// <summary>
        /// 球体拟合
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与拟合</param>
        /// <param name="outSphere">拟合球体</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int SphereFit(IntPtr pCloud, IArray indexes, out IntPtr outSphere, out IntPtr errMsg);

        /// <summary>
        /// 获取球的数据
        /// </summary>
        /// <param name="sphere">球</param>
        /// <param name="center">输出球心</param>
        /// <param name="radius">输出半径</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetSphereData(IntPtr sphere, out LPoint3 center, out double radius);

        #endregion

        #region 8---长方体特征

        /// <summary>
        /// 根据长方体的中心点、长宽高正方向向量、长宽高，构建长方体
        /// </summary>
        /// <param name="cuboCenter">长方体中心点</param>
        /// <param name="lenVector">长方体的长度正方向向量</param>
        /// <param name="widthVector">长方体的宽度正方向向量</param>
        /// <param name="heightVector">长方体的高度正方向向量</param>
        /// <param name="size">长方体的尺寸</param>
        /// <param name="cubo">构建的长方体</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GenCuboidByData(LPoint3 cuboCenter, LPoint3 lenVector, LPoint3 widthVector, LPoint3 heightVector, LSize3 size, out IntPtr cubo);

        /// <summary>
        /// 获取长方体属性
        /// </summary>
        /// <param name="cuboidData">输入长方体</param>
        /// <param name="cuboCenter">长方体中心点</param>
        /// <param name="lenVector">长方体的长度正方向向量</param>
        /// <param name="widthVector">长方体的宽度正方向向量</param>
        /// <param name="heightVector">长方体的高度正方向向量</param>
        /// <param name="len">长方体的长度</param>
        /// <param name="width">长方体的宽度</param>
        /// <param name="height">长方体的高度</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetCuboidData(IntPtr cuboidData, out LPoint3 cuboCenter, out LPoint3 lenVector, out LPoint3 widthVector, out LPoint3 heightVector, out double len, out double width, out double height);

        #endregion
    }
}