#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Geometry
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.NetMethods
* 文 件 名:       Geometry.cs
* 创建时间:       2022/1/24 14:23:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      f8c901af-38c6-415f-9aec-615d937cf48b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/24 14:23:21
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.ThreeD.Algorithm.Enums;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm
{
    public partial class NetAPI
    {
        #region 1.对象点
        /// <summary>
        /// 圆心点
        /// </summary>
        public static VVector GetCenterByCloud(VCloud cloud)
        {
            int result = NativeAPI.GetPointCloudCenterPoint(cloud.LPtr, cloud.Indexes, out var outPoint);

            return new VVector(outPoint);
        }

        /// <summary>
        /// 获取点云包围盒尺寸
        /// </summary>
        /// <param name="ownerId">点云所属模块ID</param>
        /// <param name="cloud">需计算点云</param>
        /// <param name="boundType">包围盒种类</param>
        /// <param name="errMsg">错误消息</param>
        /// <param name="visible">显示/隐藏</param>
        /// <returns></returns>
        public static VSize GetCloudBoundBox(Guid ownerId, VCloud cloud, BoundBoxType boundType, out string errMsg, bool visible = false)
        {
            errMsg = string.Empty;
            int result = NativeAPI.GetBoundingBox(ownerId.ToString(), cloud.LPtr, cloud.Indexes, (int)boundType, visible, out double boxLength, out double boxWidth, out double boxHeight, out IntPtr msgPtr);
            //包围盒计算时，避免框的点云在一个面上时，包围盒大小计算失败的问题
            if (result == 104)
            {
                return new VSize(5, 5, 0.1);
            }
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VSize(boxLength, boxWidth, boxHeight);
            });
        }

        /// <summary>
        /// 圆心点
        /// </summary>
        public static VVector GetCircleCenterPoint(VCircle circle, out string errMsg)
        {
            int result = NativeAPI.GetCircleCenterPoint(circle.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 圆锥顶点
        /// </summary>
        public static VVector GetConeVertex(VCone cone, out string errMsg)
        {
            int result = NativeAPI.GetConeVertex(cone.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 圆柱端点
        /// </summary>
        public static VVector GetCylinderEndPoint(VCylinder cylinder, out string errMsg)
        {
            int result = NativeAPI.GetCylinderEndPoint(cylinder.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 矩形中心点
        /// </summary>
        public static VVector GetRectangularCenterPoint(VRectangular rect, out string errMsg)
        {
            int result = NativeAPI.GetRectangularCenterPoint(rect.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 球心
        /// </summary>
        public static VVector GetBallCenterPoint(VSphere sphere, out string errMsg)
        {
            int result = NativeAPI.GetBallCenterPoint(sphere.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 通过方向和偏移构建点
        /// </summary>
        /// <param name="srcPoint"></param>
        /// <param name="dir"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static VVector GetPointByDirAndOffset(VVector srcPoint, VDirection dir, double offset, out string errMsg)
        {
            int result = NativeAPI.GetOffsetPointsByPointAndDir(srcPoint.ToPoint(), dir.ToVector(), offset, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        public static VVector GetExtremePointByCloud(VCloud pCloud, int calcMethod, VVector refPoint, VDirection dir, double radius, double startPercent, double endPercent, VSize offset, out string errMsg)
        {
            LPoint3 point = new LPoint3();
            if (refPoint != null)
            {
                point = refPoint.ToPoint();
            }

            int result = NativeAPI.GetExtremePointByCloud(pCloud.LPtr, pCloud.Indexes, calcMethod, point, radius, dir.ToVector(), startPercent, endPercent, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint.X + offset.XLen, outPoint.Y + offset.YLen, outPoint.Z + offset.ZLen);
            });
        }
        #endregion

        #region 2.相交点

        /// <summary>
        /// 两线相交
        /// </summary>
        public static VVector GetPointByTwoIntersectLine(VLine line1, VLine line2, out string errMsg)
        {
            int result = NativeAPI.GetPointByTwoIntersectLineSegment(line1.LPtr, line2.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 线面相交
        /// </summary>
        public static VVector GetPointByLinePlaneIntersect(VLine line1, VPlane plane, out string errMsg)
        {
            int result = NativeAPI.GetPointByLineSegmentPlaneIntersect(line1.LPtr, plane.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 三个三维平面的交点
        /// </summary>
        public static VVector GetPointByThreePlanesIntersect(VPlane plane1, VPlane plane2, VPlane plane3, out string errMsg)
        {
            int result = NativeAPI.GetPointByThreePlanesIntersect(plane1.LPtr, plane2.LPtr, plane3.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 两圆锥轴相交点
        /// </summary>
        public static VVector GetPointByTwoConicalShaft(VCone cone1, VCone cone2, out string errMsg)
        {
            int result = NativeAPI.GetPointByTwoConicalShaft(cone1.LPtr, cone2.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 两圆柱轴相交点
        /// </summary>
        public static VVector GetPointByTwoCylindricalShaft(VCylinder cylinder1, VCylinder cylinder2, out string errMsg)
        {
            int result = NativeAPI.GetPointByTwoCylindricalShaft(cylinder1.LPtr, cylinder2.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 平面和圆锥轴线交点
        /// </summary>
        public static VVector GetPointByPlaneAndConicalShaft(VPlane plane1, VCone cone, out string errMsg)
        {
            int result = NativeAPI.GetPointByPlaneAndConicalShaft(plane1.LPtr, cone.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 平面和圆柱轴线交点
        /// </summary>
        public static VVector GetPointByPlaneAndCylindricalShaft(VPlane plane1, VCylinder cylinder, out string errMsg)
        {
            errMsg = string.Empty;
            int result = NativeAPI.GetPointByPlaneAndCylindricalShaft(plane1.LPtr, cylinder.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        #endregion

        #region 3.投影点

        /// <summary>
        /// 三维点到三维直线上投影
        /// </summary>
        public static VVector GetPointByPoint2Line(VVector point1, VLine line, out string errMsg)
        {
            int result = NativeAPI.GetPointByPoint2LineSegment(point1.ToPoint(), line.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 三维点到三维平面上投影
        /// </summary>
        public static VVector GetPointByPoint2Plane(VVector point1, VPlane plane, out string errMsg)
        {
            int result = NativeAPI.GetPointByPoint2Plane(point1.ToPoint(), plane.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 三维点投影到圆锥轴
        /// </summary>
        public static VVector GetPointByPoint2ConicalShaft(VVector point1, VCone cone, out string errMsg)
        {
            int result = NativeAPI.GetPointByPoint2ConicalShaft(point1.ToPoint(), cone.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        /// <summary>
        /// 三维点投影到圆柱轴
        /// </summary>
        public static VVector GetPointByPoint2CylindricalShaft(VVector point1, VCylinder cylinder, out string errMsg)
        {
            int result = NativeAPI.GetPointByPoint2ConicalShaft(point1.ToPoint(), cylinder.LPtr, out var outPoint, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(outPoint);
            });
        }

        #endregion

        #region 4.线段特征构建
        /// <summary>
        /// 根据直线所经过点的位置坐标position和直线方向向量direction构造直线
        /// 若方向向量direction为零向量,则修正为默认方向向量(0, 0, 1)
        /// </summary>
        public static VLine GetLineByPosDir(VVector point1, VDirection direct, double len, out string errMsg)
        {
            int result = NativeAPI.GetLineSegmentByPosDir(point1.ToPoint(), direct.ToVector(), len, out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 用起始点point1和终止点point2来构造直线。若两点为同一点则返回false，否则返回true
        /// </summary>
        public static VLine GetLineByStartEndPoints(VVector point1, VVector point2, out string errMsg)
        {
            int result = NativeAPI.GetLineSegmentByStartEndPoints(point1.ToPoint(), point2.ToPoint(), out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 	说明：根据直线所经过点的位置坐标position、直线倾斜角度radTilt与直线旋转角度radRotatio构造直线
        /// 	note: 倾斜角radTilt范围为：[0, π] ;旋转角radRotation范围为：[0, 2π)
        /// </summary>
        public static VLine GetLineByPositionTiltRotation(VVector point1, double radTilt, double radTotation, double len, out string errMsg)
        {
            int result = NativeAPI.GetLineSegmentByPositionTiltRotation(point1.ToPoint(), radTilt, radTotation, len, out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 根据给定直线、方向和偏移构造平行线  注：方向应不为零向量
        /// </summary>
        public static VLine GetParallelLineByLineDirPos(VLine line, VDirection dir, double offset, out string errMsg)
        {
            int result = NativeAPI.GetParallelLineSegmentByLineSegmentDirPos(line.LPtr, dir.ToVector(), offset, out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 根据给定直线和一点构造垂直线  注：给定点应为给定直线外一点
        /// </summary>
        public static VLine GetVerticalLineByLinePos(VLine line, VVector pos, out string errMsg)
        {
            int result = NativeAPI.GetVerticalLineSegmentByLineSegmentAndPos(line.LPtr, pos.ToPoint(), out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 根据给定直线和一点构造垂直线  注：给定点应为给定直线外一点
        /// </summary>
        public static VLine GetVerticalLineByPlaneAndPos(VLine line, VVector pos, out string errMsg)
        {
            int result = NativeAPI.GetVerticalLineSegmentByPlaneAndPos(line.LPtr, pos.ToPoint(), out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 三维直线到三维平面上的投影
        /// </summary>
        public static VLine GetLineByLine2Plane(VLine line, VPlane plane, out string errMsg)
        {
            int result = NativeAPI.GetLineSegmentByLineSegment2Plane(line.LPtr, plane.LPtr, out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 构建垂线
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VLine GetVerticalLineByPlaneAndPos(VPlane plane, VVector point, out string errMsg)
        {
            int result = NativeAPI.GetVerticalLineSegmentByPlaneAndPos(plane.LPtr, point.ToPoint(), out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 说明：三维平面与三维平面是否相交，相交时给出交线line；其中输出变量bIntersect为true代表相交
        /// </summary>
        public static VLine GenLineByPlanePlaneIntersect(VPlane plane1, VPlane plane2, out string errMsg)
        {
            errMsg = string.Empty;
            int result = NativeAPI.GetLineSegmentByPlanePlaneIntersect(plane1.LPtr, plane2.LPtr, out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 圆锥轴线
        /// </summary>
        public static VLine GetLineByConicalShaft(VCone cone, out string errMsg)
        {
            int result = NativeAPI.GetLineSegmentByConicalShaft(cone.LPtr, out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 圆柱轴线
        /// </summary>
        public static VLine GetLineByCylindricalShaft(VCylinder cylinder, out string errMsg)
        {
            int result = NativeAPI.GetLineSegmentByCylindricalShaft(cylinder.LPtr, out var outLine, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(outLine);
            });
        }

        /// <summary>
        /// 点云拟合直线
        /// </summary>
        /// <param name="roi"></param>
        /// <returns></returns>
        public static VLine LineFit(VCloud cloud, LineFitMethod fitMethod, PlaneProjectionType projType, VPlane pPlane, LineDirection lineDir, double disThreshold, double confidence, double outlierRatio, double sampleHalfWidth, DirectionLock lockDir, out string errMsg)
        {
            // 2.通过索引信息来拟合直线
            int result = NativeAPI.LineSegmentFit(cloud.LPtr, cloud.Indexes, (int)fitMethod, (int)projType, pPlane == null ? IntPtr.Zero : pPlane.LPtr, (int)lineDir, disThreshold, confidence, outlierRatio, sampleHalfWidth, (int)lockDir, out var linePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(linePtr);
            });
        }

        /// <summary>
        /// 线段平移
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="fitMethod"></param>
        /// <param name="projType"></param>
        /// <param name="pPlane"></param>
        /// <param name="lineDir"></param>
        /// <param name="disThreshold"></param>
        /// <param name="confidence"></param>
        /// <param name="outlierRatio"></param>
        /// <param name="sampleHalfWidth"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VLine GetLineByTranslation(VLine line, double offsetX, double offsetY, double offsetZ, out string errMsg)
        {
            // 2.通过索引信息来拟合直线
            int result = NativeAPI.GetLineSegmentByTranslation(line.LPtr, offsetX, offsetY, offsetZ, out var linePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(linePtr);
            });
        }

        /// <summary>
        /// 线段绕点旋转
        /// </summary>
        /// <param name="line"></param>
        /// <param name="rotatePos"></param>
        /// <param name="plane"></param>
        /// <param name="angle"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VLine GetLineByRotate(VLine line, VVector rotatePos, VPlane plane, double angle, out string errMsg)
        {
            // 角度转换为弧度 1弧度=180/PI
            double radian = (Math.PI / 180) * angle;

            // 2.通过索引信息来拟合直线
            int result = NativeAPI.GetLineSegmentByRotate(line.LPtr, rotatePos.ToPoint(), plane.LPtr, radian, out var linePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VLine(linePtr);
            });
        }

        #endregion

        #region 6.面特征构建


        /// <summary>
        /// 说明: 根据平面所经过点的位置坐标position和法向量normal构造平面
        /// note:若法向量normal为零向量,则修正为默认向量(0, 0, 1)
        /// </summary>
        public static VPlane GetPlaneByPosNormal(VVector vPoint, VDirection vDirection, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByPosNormal(vPoint.ToPoint(), vDirection.ToVector(), out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                var p = new VPlane(outPlane);
                p.Center = vPoint;
                return p;
            });
        }

        /// <summary>
        /// 	说明: 将平面按照法向量normal和偏移量dOffset构造平面
        /// 	note:偏移量dOffset可正可负
        /// </summary>
        public static VPlane GetPlaneByNormal(VDirection vDirection, double offset, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByNormal(vDirection.ToVector(), offset, out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
           {
               return new VPlane(outPlane);
           });
        }

        /// <summary>
        /// 说明:根据一点和两个方向构造平面
        /// </summary>
        public static VPlane GetPlaneByPointAnd2Direction(VVector vVector, VDirection vDirection1, VDirection vDirection2, double offset, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByPointAnd2Direction(vVector.ToPoint(), vDirection1.ToVector(), vDirection1.ToVector(), out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
           {
               return new VPlane(outPlane);
           });
        }

        /// <summary>
        /// 说明:用一般方程 aX + bY + cZ +d = 0 的系数构造平面
        /// note:若系数a、b、c同时为0，则返回false，否则返回true
        /// </summary>
        public static VPlane GetPlaneByCoefficients(double a, double b, double c, double d, double offset, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByCoefficients(a, b, c, d, out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(outPlane);
            });
        }

        /// <summary>
        /// 三点构建面
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static VPlane GetPlaneByTreePoints(VVector v1, VVector v2, VVector v3, out string errMsg)
        {
            errMsg = string.Empty;

            // 2.通过索引信息来拟合直线
            int result = NativeAPI.GetPlaneByTreePoints(v1.ToPoint(), v2.ToPoint(), v3.ToPoint(), out var planePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(planePtr);
            });
        }

        /// <summary>
        /// 根据点和直线构建面
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="vLine"></param>
        /// <returns></returns>
        public static VPlane GetPlaneByPointLine(VVector point1, VLine vLine, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByPointAndLineSegment(point1.ToPoint(), vLine.LPtr, out var planePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(planePtr);
            });
        }

        /// <summary>
        /// 说明: 根据平面所经过点的位置坐标position和直线line构造平面
        /// note: 若点position在直线上，则返回false，否则返回true。
        /// 构造的平面经过点，并且垂直于直线。
        /// </summary>
        /// <param name="in_line"></param>
        /// <param name="in_plane"></param>
        /// <param name="out_line"></param>
        /// <returns></returns>
        public static VPlane GetVerticalPlaneByPointLine(VVector point1, VLine vLine, out string errMsg)
        {
            int result = NativeAPI.GetVerticalPlaneByPointAndLineSegment(point1.ToPoint(), vLine.LPtr, out var planePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(planePtr);
            });
        }

        /// <summary>
        /// 说明:根据给定平面和偏移构造平行面
        /// </summary>
        public static VPlane GetParallePlaneByPlaneOffset(VPlane inPlane, double offset, out string errMsg)
        {
            int result = NativeAPI.GetParallePlaneByPlaneOffset(inPlane.LPtr, offset, out var outPlane, out var msgPtr);

            var center = inPlane.Center;
            result = NativeAPI.GetOffsetPointsByPointAndDir(center.ToPoint(), inPlane.Direction.ToVector(), offset, out var outCenter, out msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                var plane = new VPlane(outPlane);
                plane.Width = inPlane.Width;
                plane.Height = inPlane.Height;
                plane.Center = new VVector(outCenter);
                return plane;
            });
        }

        /// <summary>
        ///  说明:根据给定平面和两点构造垂直面
        /// </summary>
        public static VPlane GetPerpendicularPlaneByPlaneAndTwoPoint(VPlane inPlane, VVector vPoint1, VVector vPoint2, out string errMsg)
        {
            int result = NativeAPI.GetPerpendicularPlaneByPlaneAndTwoPoint(inPlane.LPtr, vPoint1.ToPoint(), vPoint2.ToPoint(), out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(outPlane);
            });
        }

        /// <summary>
        ///  说明:根据给定点和圆锥构造平面
        ///  此构造面经过点并垂直于圆锥轴
        /// </summary>
        public static VPlane GetPlaneByPointAndCone(VCone cone, VVector vPoint1, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByPointAndCone(cone.LPtr, vPoint1.ToPoint(), out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(outPlane);
            });
        }

        /// <summary>
        ///  说明:根据给定点和圆柱构造平面
        ///  note:此构造面经过点并垂直于圆柱轴
        /// </summary>
        public static VPlane GetPlaneByPointAndCylinder(VCylinder cylinder, VVector vPoint1, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByPointAndCylinder(cylinder.LPtr, vPoint1.ToPoint(), out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(outPlane);
            });
        }

        /// <summary>
        ///  说明:根据直线和平面构造平面
        ///  note:此构造面经过直线并且垂直于平面
        /// </summary>
        public static VPlane GetPlaneByLineAndPlane(VPlane inPlane, VLine line, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByLineSegmentAndPlane(inPlane.LPtr, line.LPtr, out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(outPlane);
            });
        }

        /// <summary>
        ///  说明:根据给定平面和两点构造垂直面
        /// </summary>
        public static VPlane GetPlaneByCircle(VCircle inCircle, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByCircle(inCircle.LPtr, out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(outPlane);
            });
        }

        /// <summary>
        ///  说明:根据给定平面和两点构造垂直面
        /// </summary>
        public static VPlane GetPlaneByRectangular(VRectangular inRect, out string errMsg)
        {
            int result = NativeAPI.GetPlaneByRectangular(inRect.LPtr, out var outPlane, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VPlane(outPlane);
            });
        }

        /// <summary>
        /// 点云拟合平面
        /// </summary>
        /// <param name="roi"></param>
        /// <returns></returns>
        public static VPlane PlaneFit(VCloud cloud, PlaneFitMethod fitMethod, PlaneDierection dir, RegionMode regionMode, ZLookupMethod lookMethod, long sizeX, long sizeY, out string errMsg)
        {
            errMsg = string.Empty;

            // 2.通过索引信息来拟合直线
            int result = NativeAPI.PlaneFit(cloud.LPtr, cloud.Indexes, (int)fitMethod, (int)dir, (int)regionMode, (int)lookMethod, sizeX, sizeY, out var planePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
             {
                 return new VPlane(planePtr);
             });
        }

        /// <summary>
        /// 点云拟合直线
        /// </summary>
        /// <param name="roi"></param>
        /// <returns></returns>
        public static VPlane MovePlaneToSpecifyPoint(VPlane vPlane, VVector destPoint, out string errMsg)
        {
            errMsg = string.Empty;

            // 2.通过索引信息来拟合直线
            int result = NativeAPI.MovePlaneToSpecifyPoint(vPlane.LPtr, destPoint.ToPoint(), out var planePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                var plane = new VPlane(planePtr);
                plane.Center = destPoint;
                plane.Width = vPlane.Width;
                plane.Height = vPlane.Height;
                return plane;
            });
        }

        #endregion

        #region 8.方向信息

        /// <summary>
        /// 获取线的方向
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static VDirection GetLineDirection(VLine line)
        {
            NativeAPI.GetLineSegmentDirVector(line.LPtr, out var dir);

            return new VDirection(dir);
        }

        /// <summary>
        /// 获取面的方向
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static VDirection GetPlaneDirection(VPlane plane, out string errMsg)
        {
            int result = NativeAPI.GetPlaneNormal(plane.LPtr, out var dir, out var zVal, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VDirection(dir);
            });
        }

        #endregion

        #region 9.圆柱

        /// <summary>
        /// 通过ROI对点云进行分割得到圆
        /// </summary>
        /// <param name="roi"></param>
        /// <returns></returns>
        public static VCylinder CylinderFit(VCloud cloud, out string errMsg)
        {
            errMsg = string.Empty;
            int result = 0;

            // 2.通过索引信息来拟合圆柱
            result = NativeAPI.CylinderFit(cloud.LPtr, cloud.Indexes, out var cylinderPtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCylinder(cylinderPtr);
            });
        }

        #endregion

        #region 10.圆柱特征

        /// <summary>
        /// 获取面的方向
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static VCylinder GenCylinderByData(VVector pos, VDirection dir, double radius, double height = 1)
        {
            NativeAPI.GenCylinderByData(pos.ToPoint(), dir.ToVector(), height, radius, out var cylinderPtr);

            return new VCylinder(cylinderPtr);
        }

        #endregion

        #region 11.圆特征
        /// <summary>
        /// 通过ROI对点云进行分割得到圆
        /// </summary>
        /// <param name="roi"></param>
        /// <returns></returns>
        public static VCircle CircleFit(VCloud cloud, CircleFitMode fitMode, BasePlaneType basePlaneType, VPlane basePlane, double dDisThre, double dConfidence, double dOutlierRatio, double iSampleHalfWidth, CircleFitCloudShape circleFitCloud, out string errMsg)
        {
            errMsg = string.Empty;
            int result = 0;
            IntPtr msgPtr = IntPtr.Zero;
            IntPtr planePtr = basePlane == null ? IntPtr.Zero : basePlane.LPtr;
            // 2.通过索引信息来拟合圆
            result = NativeAPI.CircleFit(cloud.LPtr, cloud.Indexes, (int)fitMode, (int)basePlaneType, planePtr, dDisThre, dConfidence, dOutlierRatio, iSampleHalfWidth, (int)circleFitCloud, out var circlePtr, out msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCircle(circlePtr);
            });
        }
        #endregion

        #region 12.球特征

        /// <summary>
        /// 通过ROI对点云进行分割得到圆
        /// </summary>
        /// <param name="roi"></param>
        /// <returns></returns>
        public static VSphere SphereFit(VCloud cloud, out string errMsg)
        {
            errMsg = string.Empty;
            int result = 0;

            // 2.通过索引信息来拟合圆柱
            result = NativeAPI.SphereFit(cloud.LPtr, cloud.Indexes, out var spherePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VSphere(spherePtr);
            });
        }

        #endregion

        #region 13.圆锥特征

        /// <summary>
        /// 通过ROI对点云进行分割得到圆
        /// </summary>
        /// <param name="roi"></param>
        /// <returns></returns>
        public static VCone ConeFit(VCloud cloud, out string errMsg)
        {
            errMsg = string.Empty;
            int result = 0;

            // 2.通过索引信息来拟合圆柱
            result = NativeAPI.ConeFit(cloud.LPtr, cloud.Indexes, out var conePtr, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCone(conePtr);
            });
        }

        #endregion
    }
}