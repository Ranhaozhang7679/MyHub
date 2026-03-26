#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Measure
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.NetMethods
* 文 件 名:       Measure.cs
* 创建时间:       2022/1/27 16:14:47
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      75a2c697-1ebc-43f5-95c7-ee831212a446
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/27 16:14:47
* 修 改 人:		  L05123
************************************************************************************/
#endregion

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
        /// <summary>
        /// 说明：计算三维点到三维点之间的距离
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>

        public static double CalDistanceFromPoint2Point(VVector in_position1, VVector in_position2, out string errMsg)
        {
            int result = NativeAPI.CalDistanceFromPoint2Point(in_position1.ToPoint(), in_position2.ToPoint(), out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
              {
                  return distance;
              });
        }


        /// <summary>
        /// 说明：计算三维点到三维点之间的距离
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>

        public static double CalDistance2PointByCoord(VVector inPos1, VVector inPos2, int distanceType, VCoord coord, out string errMsg)
        {
            bool isRef = coord != null;
            LCoord refCoord = new LCoord();
            refCoord.Init();
            if (coord != null)
            {
                refCoord = coord.ToCoord();
            }

            int result = NativeAPI.CalDistance2PointByCoord(inPos1.ToPoint(), inPos2.ToPoint(), distanceType, isRef, refCoord, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }
        /// <summary>
        /// 说明: 计算三维点到三维线之间的距离
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>

        public static double CalDistanceFromPoint2Line(VVector in_position, VLine in_line, out string errMsg)
        {
            int result = NativeAPI.CalDistanceFromPoint2Line(in_position.ToPoint(), in_line.LPtr, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }


        /// <summary>
        /// 说明: 计算三维点到三维面之间的距离
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>

        public static double CalDistanceFromPoint2Plane(VVector in_position, VPlane in_plane, out string errMsg)
        {
            int result = NativeAPI.CalDistanceFromPoint2Plane(in_position.ToPoint(), in_plane.LPtr, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }


        /// <summary>
        /// 说明: 计算三维直线到三维直线之间的距离
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>

        public static double CalDistanceFromLine2Line(VLine in_line1, VLine in_line2, out string errMsg)
        {
            int result = NativeAPI.CalDistanceFromLine2Line(in_line1.LPtr, in_line2.LPtr, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }


        /// <summary>
        /// 说明: 计算三维直线到三维面之间的距离
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>

        public static double CalDistanceFromLine2Plane(VLine in_line, VPlane in_plane, out string errMsg)
        {
            int result = NativeAPI.CalDistanceFromLine2Plane(in_line.LPtr, in_plane.LPtr, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }


        /// <summary>
        /// 说明: 计算三维直线到三维直线之间的夹角
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static double CalAngleFromLineAndLine(VLine in_line1, VLine in_line2, out string errMsg)
        {
            int result = NativeAPI.CalAngleFromLineAndLine(in_line1.LPtr, in_line2.LPtr, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }


        /// <summary>
        /// 说明: 计算三维直线到三维平面之间的夹角
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static double CalAngleFromLineAndPlane(VLine in_line, VPlane in_plane, out string errMsg)
        {
            int result = NativeAPI.CalAngleFromLineAndPlane(in_line.LPtr, in_plane.LPtr, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }

        /// <summary>
        /// 说明: 计算三维平面和三维平面之间的夹角
        /// </summary>
        /// <param name="file"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static double CalAngleFromPlaneAndPlane(VPlane in_plane1, VPlane in_plane2, out string errMsg)
        {
            int result = NativeAPI.CalAngleFromPlaneAndPlane(in_plane1.LPtr, in_plane2.LPtr, out double distance, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return distance;
            });
        }
    }
}
