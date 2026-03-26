#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Tolerance
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.NetMethods
* 文 件 名:       Tolerance.cs
* 创建时间:       2022/2/28 10:22:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      73fac842-a8a1-4051-9d6b-0cfb11a02d59
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/28 10:22:43
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
        #region 公差
        /*
         平行度
         */
        public static double GetParallelism(VCloud cloud, VLine baseLine, VPlane basePlane, ParallelismMode mode, out double maxOffset, out double minOffset, out string errMsg)
        {
            errMsg = String.Empty;

            // 平行度
            double parallelismVal = 0;
            int result = NativeAPI.GetParallelism(cloud.LPtr, cloud.Indexes, mode, (baseLine == null ? IntPtr.Zero : baseLine.LPtr), (basePlane == null ? IntPtr.Zero : basePlane.LPtr), out parallelismVal, out maxOffset, out minOffset, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return parallelismVal;
            });
        }

        /*
         圆度
         */
        public static double GetRoundness(VCloud cloud, GenToleranceType roundnessMode, VPlane basePlane, VCircle circle, out double maxOffset, out double minOffset, out VCircle outCircle, out string errMsg)
        {
            errMsg = String.Empty;

            // 圆度
            double roundnessVal = 0;
            IntPtr planePtr = basePlane == null ? IntPtr.Zero : basePlane.LPtr;
            IntPtr circlePtr = circle == null ? IntPtr.Zero : circle.LPtr;
            
            int result = NativeAPI.GetRoundness(cloud.LPtr, cloud.Indexes, (int)roundnessMode, planePtr, circlePtr, out roundnessVal, out maxOffset, out minOffset, out var outCirclePtr, out IntPtr msgPtr);

            outCircle = new VCircle(outCirclePtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return roundnessVal;
            });
        }

        /*
            获取平面度，基于点云
         */
        public static double GetFlatness(VCloud cloud, FitType fitType, VPlane basePlane, out double maxOffset, out double minOffset, out VPlane outPlane, out string errMsg)
        {
            errMsg = String.Empty;

            IntPtr planePtr = basePlane == null ? IntPtr.Zero : basePlane.LPtr;

            // 平面度
            
            int result = NativeAPI.GetFlatness(cloud.LPtr, cloud.Indexes, (int)fitType, planePtr, out var flatness, out maxOffset, out minOffset, out var outPlanePtr,out DArray arr, out var msgPtr);
            NativeAPI.DeleteArr(arr.Ptr, NativeAPI.ArrayType.DoubleArr);
            outPlane = new VPlane(outPlanePtr);
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return flatness;
            });
        }

        /*
           获取平面度,基于点集合
        */
        public static double GetFlatnessByPoints(LArray<VVector> vectors, FitType fitType, VPlane basePlane, out double maxOffset, out double minOffset, out VPlane outPlane, out string errMsg)
        {
            errMsg = String.Empty;

            IntPtr planePtr = basePlane == null ? IntPtr.Zero : basePlane.LPtr;

            LPointArray pointArray = new LPointArray(vectors.ToArray());

            int result = NativeAPI.GetFlatnessByPointGroup(pointArray, (int)fitType, planePtr, out var flatness, out maxOffset, out minOffset,out var outPlanePtr, out DArray arr, out var msgPtr);
            NativeAPI.DeleteArr(arr.Ptr, NativeAPI.ArrayType.DoubleArr);
            outPlane = new VPlane(outPlanePtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return flatness;
            });
        }

        /*
            获取直线度
         */
        public static double GetStraightness(VCloud cloud, FitType fitType, VLine baseLine, ConstraintType cType, VPlane cPlane, VDirection vDir, out double maxOffset, out double minOffset, out VLine outLine, out string errMsg)
        {
            errMsg = String.Empty;

            IntPtr linePtr = baseLine == null ? IntPtr.Zero : baseLine.LPtr;
            IntPtr planePtr = cPlane == null ? IntPtr.Zero : cPlane.LPtr;

            // 圆度
            double straightness = 0;
            int result = NativeAPI.GetStraightness(cloud.LPtr, cloud.Indexes, (int)fitType, linePtr, (int)cType, planePtr, vDir.ToVector(), out straightness, out maxOffset, out minOffset, out var outLinePtr, out IntPtr msgPtr);
            outLine = new VLine(outLinePtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return straightness;
            });
        }
        #endregion


        /*
            获取垂直度
         */
        public static double GetVerticality(VCloud cloud, VerticalityDefMode defMode, VerticalityRef refMode, VPlane basePlane, VLine baseLine, out double maxOffset, out double minOffset, out VPlane outPlane, out string errMsg)
        {
            errMsg = String.Empty;

            IntPtr linePtr = baseLine == null ? IntPtr.Zero : baseLine.LPtr;
            IntPtr planePtr = basePlane == null ? IntPtr.Zero : basePlane.LPtr;

            // 垂直度
            int result = NativeAPI.GetVerticality(cloud.LPtr, cloud.Indexes, (int)defMode,(int)refMode, planePtr, linePtr, out var verticality, out maxOffset, out minOffset, out var outLinePtr, out IntPtr msgPtr);
            outPlane = new VPlane(outLinePtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return verticality;
            });
        }


        /*
            获取倾斜度
         */
        public static double GetInclination(VCloud cloud, double inclinationAngle, InclinationFitType fitType, VerticalityMode inclinationMode, VPlane projectionPlane, VPlane basePlane, VLine baseLine, out double maxOffset, out double minOffset, out VPlane outLine, out string errMsg)
        {
            errMsg = String.Empty;

            IntPtr linePtr = baseLine == null ? IntPtr.Zero : baseLine.LPtr;
            IntPtr planePtr = basePlane == null ? IntPtr.Zero : basePlane.LPtr;
            IntPtr projPlane = projectionPlane == null ? IntPtr.Zero : projectionPlane.LPtr;
            
            int result = NativeAPI.GetInclination(cloud.LPtr, cloud.Indexes, inclinationAngle,(int)fitType, (int)inclinationMode, projPlane, planePtr, linePtr, out var inclination, out maxOffset, out minOffset, out var outLinePtr, out IntPtr msgPtr);
            outLine = new VPlane(outLinePtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return inclination;
            });
        }


        /*
            获取圆柱度
         */
        public static double GetCylindricity(VCloud cloud, FitType fitType, VCylinder standardCylinder, out double maxOffset, out double minOffset, out VCylinder outCylinder, out string errMsg)
        {
            errMsg = String.Empty;

            IntPtr cylinderPtr = standardCylinder == null ? IntPtr.Zero : standardCylinder.LPtr;

            // 圆柱度
            int result = NativeAPI.GetCylindricity(cloud.LPtr, cloud.Indexes, (int)fitType, cylinderPtr, out var cylindricity, out maxOffset, out minOffset, out var outCyPtr, out IntPtr msgPtr);
            outCylinder = new VCylinder(outCyPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return cylindricity;
            });
        }


        /*
            获取线轮廓度
         */
        public static double GetLineProfile(AlignmentType type, LArray<VVector> basePoints, LArray<VVector> mPoints, out double maxOffset, out double minOffset, out LArray<double> offsets, out string errMsg)
        {
            errMsg = String.Empty;

            var bPoints = basePoints.Select(p => p.ToPoint()).ToArray();
            var nPoints = basePoints.Select(p => p.ToVector()).ToArray();
            var measurePoints = mPoints.Select(p => p.ToPoint()).ToArray();
            // 
            int result = NativeAPI.GetLineProfileTolerance((int)type, bPoints, nPoints, measurePoints, mPoints.Count, out var tolerance, out DArray arr, out IntPtr msgPtr);

            offsets = new LArray<double>(arr.ToArray());
            minOffset = offsets.Min();
            maxOffset = offsets.Max();
            NativeAPI.DeleteArr(arr.Ptr, NativeAPI.ArrayType.DoubleArr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return tolerance;
            });
        }
    }

}
