#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LAPI
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm
* 文 件 名:       LAPI.cs
* 创建时间:       2021/12/16 10:30:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      4c9c1ec9-7d82-4c5a-8bdd-4af2fdcbf354
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/16 10:30:03
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Core;
using Luster.ThreeD.Algorithm.Enums;
using Luster.ThreeD.Algorithm.Interfaces;
using Luster.ThreeD.Algorithm.ROIs;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Luster.ThreeD.Algorithm
{
    public partial class NetAPI
    {
        /// <summary>
        /// 将数据转换为放射矩阵
        /// </summary>
        /// <param name="rX">X转动</param>
        /// <param name="rY">Y转动</param>
        /// <param name="rZ">Z转动</param>
        /// <param name="mX">X平移</param>
        /// <param name="mY">Y平移</param>
        /// <param name="mZ">Z平移</param>
        /// <returns></returns>
        public static LMatrix DataToMatrix(double rX, double rY, double rZ, double mX, double mY, double mZ)
        {
            LMatrix matrix = new LMatrix();
            matrix.Init();
            NativeAPI.DataToTranform(rX, rY, rZ, mX, mY, mZ, ref matrix);

            return matrix;
        }

        /// <summary>
        /// 点云变换
        /// </summary>
        /// <param name="src">原始点云</param>
        /// <param name="matrix">变换矩阵</param>
        /// <returns>变换后的点云</returns>
        public static VCloud Transform(VCloud src, ITransform transform)
        {
            var matrix = transform.Matrix;
            NativeAPI.CloudTransform(src.LPtr, src.Indexes, ref matrix[0], out var dstPtr);
            return new VCloud(dstPtr);
        }

        /// <summary>
        /// 点云平移
        /// </summary>
        /// <param name="src"></param>
        /// <param name="in_offsetDir"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static VCloud CloudOffset(VCloud src, LVector in_offsetDir, double offset)
        {
            NativeAPI.CloudOffset(src.LPtr, src.Indexes, in_offsetDir, offset, out var dstPtr);
            return new VCloud(dstPtr);
        }

        /// <summary>
        /// 点云绕轴旋转
        /// </summary>
        /// <param name="src"></param>
        /// <param name="xRotateAngle"></param>
        /// <param name="yRotateAngle"></param>
        /// <param name="zRotateAngle"></param>
        /// <returns></returns>
        public static VCloud CloudRotate(VCloud src, double xRotateAngle, double yRotateAngle, double zRotateAngle)
        {
            NativeAPI.CloudRotate(src.LPtr, src.Indexes, xRotateAngle, yRotateAngle, zRotateAngle, out var dstPtr);
            return new VCloud(dstPtr);
        }

        /// <summary>
        /// 点云镜像
        /// </summary>
        /// <param name="src"></param>
        /// <param name="aroundXY"></param>
        /// <param name="aroundXZ"></param>
        /// <param name="aroundYZ"></param>
        /// <returns></returns>
        public static VCloud CloudFilp(VCloud src, bool aroundXY, bool aroundXZ, bool aroundYZ)
        {
            NativeAPI.CloudFilp(src.LPtr, src.Indexes, aroundXY, aroundXZ, aroundYZ, out var dstPtr);
            return new VCloud(dstPtr);
        }



        public static void TransformSource(VCloud src, ITransform transform, bool isInvert = false)
        {
            if (src == null) return;

            NativeAPI.TransformCloudBySource(src.LPtr, src.Indexes,transform.Matrix, isInvert);

            // 将矩阵置为空
            var matrix = new LMatrix();
            matrix.Init();
            src.Matrix = matrix;
        }

        /// <summary>
        /// 处理异常信息
        /// </summary>
        /// <param name="resultCode">返回码</param>
        /// <param name="msgPtr">错误消息</param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private static T ProcessErr<T>(int resultCode, IntPtr msgPtr, out string errMsg, Func<T> func)
        {
            errMsg = string.Empty;
            if (resultCode == -1)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
                return default(T);
            }
            else if (resultCode == 10001)
            {
                errMsg = "指针对象为空";
                return default(T);
            }
            else if (resultCode > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
                return default(T);
            }

            return func.Invoke();
        }

        /// <summary>
        /// 通过多点构建点云
        /// </summary>
        /// <param name="vectors">点集合</param>
        /// <returns></returns>
        public static VCloud GenCloudByPoints(VVector[] vectors)
        {
            LPoint3[] points = new LPoint3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                points[i] = vectors[i].ToPoint();
            }
            NativeAPI.GenCloudByPoints(points, points.Length, out var cloudPtr);
            return new VCloud(cloudPtr);
        }

        /// <summary>
        /// 多点云配准
        /// </summary>
        /// <param name="matrixs">标定文件</param>
        /// <param name="clouds">点云集合</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>新的点云</returns>
        public static VCloud Registration(VTransform[] transforms, VCloud[] clouds, out string errMsg)
        {
            errMsg = string.Empty;
            int len = clouds.Length;
            IntPtr[] pClouds = new IntPtr[len];
            for (int i = 0; i < len; i++)
            {
                pClouds[i] = clouds[i].LPtr;
            }

            LMatrix[] matrixs = new LMatrix[len];
            for (int i = 0; i < len; i++)
            {
                matrixs[i] = new LMatrix(transforms[i].Matrix);
            }

            int result = NativeAPI.CloudRegistration(pClouds, matrixs, len, out var outPTR, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
             {
                 return new VCloud(outPTR);
             });
        }

        //点云去重
        public static VCloud ReRepeat(VCloud cloud, VROIs vROIs, SmoothMode smoothMode, MergeMode mergeMode, int iterNum, NeighsSearchMode neighsSearchMode, int nearestKNum, VSize voxelSize, out string errMsg)
        {
            errMsg = string.Empty;
            int count = vROIs == null ? 0 : vROIs.Count;
            IntPtr[] ptrs = new IntPtr[count];
            if (vROIs != null)
            {
                //获取对应的坐标系,做随模板运动动作
                var rois = vROIs.GetRelativeROIs();
                for (int i = 0; i < count; i++)
                {
                    ptrs[i] = rois[i].LPtr;
                }
            }
            
            ROIType rType = (ROIType)Enum.Parse(typeof(ROIType), vROIs[0].GetType().Name);
            // 锁住指针数组
            GCHandle handle = GCHandle.Alloc(ptrs, GCHandleType.Pinned);
            int result = NativeAPI.CloudReRepeat(cloud.LPtr, count, (int)rType, ptrs, (int)smoothMode, iterNum, (int)mergeMode, (int)neighsSearchMode, nearestKNum, voxelSize.ToSize(), out var cloudPtr, out var msgPTR);
            handle.Free();
            return ProcessErr(result, msgPTR, out errMsg, () =>
            {
                return new VCloud(cloudPtr);
            });
        }

        /// <summary>
        /// 点云去噪
        /// </summary>
        /// <param name="pCloud"></param>
        /// <param name="filterMode"></param>
        /// <param name="pointNeighsNum"></param>
        /// <param name="stddevCoeffThreshold"></param>
        /// <param name="voxelGridLeafSize"></param>
        /// <param name="neighsNumThreshold"></param>
        /// <param name="checkNeighsNumThreshSetAuto"></param>
        /// <param name="filterNegative"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VCloud Denoising(VCloud pCloud, FilterMode filterMode, int pointNeighsNum, double stddevCoeffThreshold, VSize voxelGridLeafSize, int neighsNumThreshold, bool checkNeighsNumThreshSetAuto, bool filterNegative, out string errMsg)
        {
            errMsg = string.Empty;

            int result = NativeAPI.Denoising(pCloud.LPtr, pCloud.Indexes, filterMode, pointNeighsNum, stddevCoeffThreshold, voxelGridLeafSize.ToSize(), neighsNumThreshold, checkNeighsNumThreshSetAuto, filterNegative, out IntPtr outPtr, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCloud(outPtr);
            });
        }

        /// <summary>
        /// 点云边缘提取
        /// </summary>
        /// <param name="pCloud"></param>
        /// <param name="subROIs"></param>
        /// <param name="KnnSearchNum"></param>
        /// <param name="DistThresh"></param>
        /// <param name="Sigma"></param>
        /// <param name="projPlane"></param>
        /// <param name="innerDis"></param>
        /// <param name="pointToPlaneDisThresh"></param>
        /// <param name="bAmplifyMode"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VCloud ExtractEdge(VCloud pCloud, VROIs vROIs, int edgeType, int KnnSearchNum, double DistThresh, double Sigma, VPlane projPlane, VSize innerDis, double pointToPlaneDisThresh, bool bAmplifyMode, out string errMsg)
        {
            errMsg = string.Empty;
            IntPtr planePtr = projPlane == null ? IntPtr.Zero : projPlane.LPtr;
            //边缘提取
            int count = 0;
            IntPtr[] ptrs = new IntPtr[count];
            ROIType rType = ROIType.VCuboid;
            if (edgeType == 0)
            {
                count = vROIs.Count;
                ptrs = new IntPtr[count];
                //获取对应的坐标系,做随模板运动动作
                var subROIs = vROIs.GetRelativeROIs();
                for (int i = 0; i < count; i++)
                {
                    ptrs[i] = subROIs[i].LPtr;
                }
                rType = (ROIType)Enum.Parse(typeof(ROIType), vROIs[0].GetType().Name);
            }
            // 锁住指针数组
            GCHandle handle = GCHandle.Alloc(ptrs, GCHandleType.Pinned);
            // 子ROI为空的场景
            int result = NativeAPI.CloudExtractEdge(pCloud.LPtr, pCloud.Indexes, count, (int)rType, ptrs, planePtr, KnnSearchNum, DistThresh, Sigma, innerDis.XLen, innerDis.YLen, innerDis.ZLen, pointToPlaneDisThresh, bAmplifyMode, out IntPtr outPtr, out IntPtr msgPtr);
            handle.Free();
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCloud(outPtr);
            });

        }

        /// <summary>
        /// 点云法向计算
        /// </summary>
        /// <param name="inCloud">输入点云</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns></returns>
        public static VCloud CloudNormalCalculation(VCloud  inCloud,  out string errMsg)
        {
            errMsg = "";
            int result = NativeAPI.CloudNormalCalculation(inCloud.LPtr, out IntPtr outPtr, out IntPtr msgPtr);
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCloud(outPtr);
            });
        }


        public static VCloud CloudProjectedToPlane(VCloud pCloud, VPlane projPlane, out string errMsg)
        {
            errMsg = "";

            int result = NativeAPI.CloudProjectedToPlane(pCloud.LPtr, pCloud.Indexes, projPlane.LPtr, out IntPtr outPtr, out IntPtr msgPtr);
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCloud(outPtr);
            });
        }

        /// <summary>
        /// 点云将采样
        /// </summary>
        /// <param name="pCloud">标定文件</param>
        /// <returns>新的点云</returns>
        public static VCloud Downsamping(VCloud pCloud, SampleMethod sampleMethod, double sampleRatio, int indicesSampleStep, double spaceDistBetweenPoints, out string errMsg)
        {
            errMsg = string.Empty;

            int result = NativeAPI.Downsamping(pCloud.LPtr, pCloud.Indexes, sampleMethod, sampleRatio, indicesSampleStep, spaceDistBetweenPoints, out IntPtr outPtr, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCloud(outPtr);
            });
        }

        /// <summary>
        /// 点云平滑
        /// </summary>
        /// <param name="pCloud"></param>
        /// <param name="searchMethod"></param>
        /// <param name="knnSearchNum"></param>
        /// <param name="searchRadius"></param>
        /// <param name="projectionType"></param>
        /// <param name="upSample"></param>
        /// <param name="size"></param>
        /// <param name="diationNum"></param>
        /// <param name="autoCloudResolution"></param>
        /// <param name="cloudResolution"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VCloud Smooth(VCloud pCloud, SearchNeighMethod searchMethod, int knnSearchNum, double searchRadius, ProjectionReviseMethod projectionType, UpSampleMethod upSample, VSize size, int diationNum, bool autoCloudResolution, double cloudResolution, out string errMsg)
        {
            errMsg = string.Empty;

            int result = NativeAPI.Smooth(pCloud.LPtr, pCloud.Indexes, searchMethod, knnSearchNum, searchRadius, (int)projectionType, (int)upSample, size.ToSize(), diationNum, autoCloudResolution, cloudResolution, out IntPtr outPtr, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCloud(outPtr);
            });
        }

        /// <summary>
        /// 分割
        /// </summary>
        /// <param name="cloud">点云</param>
        /// <param name="roiType">ROI类型</param>
        /// <param name="isInside">是否包含</param>
        /// <param name="surfaceDis">表面距离</param>
        /// <param name="isRetIndex">是否返回索引</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>分割后的点云</returns>
        //public static VCloud Segment(VCloud cloud, IGeometry roi, bool isInside, double surfaceDis, out string errMsg)
        //{
        //    errMsg = string.Empty;

        //    // 获取ROI类型
        //    ROIType rType = (ROIType)Enum.Parse(typeof(ROIType), roi.GetType().Name);

        //    // 1.通过ROI来获取包含点云的索引信息
        //    int result = NativeAPI.CloudSegment(cloud.LPtr, cloud.Indexes, rType, roi.LPtr, !isInside, surfaceDis, true, out var indexes, out var cloudPtr, out var msgPtr);

        //    if (result > 0)
        //    {
        //        errMsg = Marshal.PtrToStringUni(msgPtr);
        //        return cloud;
        //    }

        //    // 直接返回点云索引
        //    return new VCloud(cloud, indexes);
        //}

        /// <summary>
        /// 分割
        /// </summary>
        /// <param name="cloud">点云</param>
        /// <param name="rois">ROI对下集合</param>
        /// <param name="isInside">是否包含</param>
        /// <param name="surfaceDis">表面距离</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>分割后的索引点云</returns>
        public static VCloud Segment(VCloud cloud, List<LBaseROI> rois, bool isInside, double surfaceDis, out string errMsg)
        {
            errMsg = string.Empty;

            // 获取ROI类型
            ROIType rType = (ROIType)Enum.Parse(typeof(ROIType), rois[0].GetType().Name);

            // 传递ROI指针数组
            var roiPtrs = rois.Select(u => u.LPtr).ToArray();

            // 1.通过ROI来获取包含点云的索引信息
            int result = NativeAPI.CloudSegment(cloud.LPtr, cloud.Indexes, rType, rois.Count, roiPtrs, !isInside, surfaceDis, out var indexes, out var msgPtr);

            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
                return cloud;
            }
            // 如果使用点云索引
            return new VCloud(cloud, indexes);
        }


        public static void GetSearchPoints(VCloud cloud, double radius, int searchType, VVector[] points, out List<VVector> maxPoints, out List<double> tolerances)
        {
            // 获取ROI类型
            ROIType rType = ROIType.VSphere;
            tolerances = new List<double>();
            maxPoints = new List<VVector>();

            foreach (var p in points)
            {
                // 1.通过点来构建中心
                VSphere sphere = new VSphere(p, radius);
                IntPtr[] ptrs = new IntPtr[1] { sphere.LPtr };

                // 2.求出形状内的点对应的索引
                int result = NativeAPI.CloudSegment(cloud.LPtr, cloud.Indexes, rType, 1, ptrs, false, 0, out var indexes, out var msgPtr);

                // 计算出最大点
                NativeAPI.CalcMaxDistance(cloud.LPtr, cloud.Indexes, indexes, searchType, cloud.Distances, out var maxPoint, out var maxTolerance, out msgPtr);

                sphere.Dispose();
                maxPoints.Add(new VVector(maxPoint));
                tolerances.Add(maxTolerance);
            }
        }

        #region 坐标系和坐标系

        /// <summary>
        /// 通过数据构建坐标系
        /// </summary>
        /// <param name="coordAxis"></param>
        /// <param name="origin"></param>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        /// <returns></returns>
        public static VCoord GenCoordByData(CoordAxis coordAxis, VVector origin, VDirection xAxis, VDirection yAxis, VDirection zAxis, bool isFlipX, bool isFlipY, bool isFlipZ, out string errMsg)
        {
            int result = NativeAPI.GenCoordByData(origin.ToPoint(), (int)coordAxis, xAxis.ToVector(), yAxis.ToVector(), zAxis.ToVector(), isFlipX, isFlipY, isFlipZ, out var coord, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCoord(coord);
            });
        }

        /// <summary>
        /// 通过3面构建坐标系
        /// </summary>
        /// <param name="plane1"></param>
        /// <param name="plane2"></param>
        /// <param name="plane3"></param>
        /// <param name="isFlipX"></param>
        /// <param name="isFlipY"></param>
        /// <param name="isFlipZ"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VCoord GenCoordByPlane3(VPlane plane1, VPlane plane2, VPlane plane3, bool isFlipX, bool isFlipY, bool isFlipZ, out string errMsg)
        {

            int result = NativeAPI.GetCoordByThreePlane(plane1.LPtr, plane2.LPtr, plane3.LPtr, isFlipX, isFlipY, isFlipZ, out var coord, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCoord(coord);
            });
        }

        /// <summary>
        /// 坐标系对齐
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="isTrans"></param>
        /// <returns></returns>
        public static VTransform AlignByCoord(VCloud cloud, VCoord src, VCoord dst, bool isTrans = false)
        {
            IntPtr cloudPtr = cloud == null ? IntPtr.Zero : cloud.LPtr;
            IArray array = cloud == null ? new IArray() : cloud.Indexes;
            int result = NativeAPI.CoordinateAlign(cloudPtr, array, src.ToCoord(), dst.ToCoord(), isTrans, out var matrix, out var msgPtr);

            string errMsg = string.Empty;
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                cloud.Matrix = matrix;
                return new VTransform(matrix);
            });
        }

        /*
            rps 对齐
         */

        public static VTransform AlignByRPS(VCloud pCloud, EffectiveDirectionType effectiveDirectionType, AlignmentLock lockType, int maxIterNums, double maxDistanceThreshold, double mixPercentage, List<VVector> cloudPoints, List<VVector> modelPoints, out string errMsg)
        {
            errMsg = String.Empty;
            IntPtr cloudPtr = pCloud == null ? IntPtr.Zero : pCloud.LPtr;
            IArray array = pCloud == null ? new IArray() : pCloud.Indexes;
            LPointArray cPoints = new LPointArray(cloudPoints.ToArray());
            LPointArray mPoints = new LPointArray(modelPoints.ToArray());

            int result = NativeAPI.RPSAlignExcute(cloudPtr, (int)effectiveDirectionType, (int)lockType, maxIterNums, maxDistanceThreshold, mixPercentage, mPoints, cPoints, out var matrix, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                pCloud.Matrix = matrix;
                return new VTransform(matrix);
            });
        }


        public static VTransform AlignByRPSBest(VMesh mesh, VCloud pCloud, double sampleRatio, int iterations, List<VVector> modelPoints, List<VVector> cloudPoints, bool isBest, LockType lockType, CoordDirection rotateC, CoordDirection translateC, BestFitAlignType bestFitAlignType, out double sumSquaredDistance, out double pointsDistanceRMSEm, out string errMsg)
        {
            errMsg = String.Empty;
            IntPtr cloudPtr = pCloud == null ? IntPtr.Zero : pCloud.LPtr;
            IArray array = pCloud == null ? new IArray() : pCloud.Indexes;
            LPointArray cPoints = new LPointArray(cloudPoints.ToArray());
            LPointArray mPoints = new LPointArray(modelPoints.ToArray());

            int result = NativeAPI.RPSBestFitAlign(cloudPtr, array, mesh.LPtr, sampleRatio, iterations, mPoints, cPoints, isBest, (int)lockType, (int)rotateC, (int)translateC, (int)bestFitAlignType, out var matrix, out sumSquaredDistance, out pointsDistanceRMSEm, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                pCloud.Matrix = matrix;
                return new VTransform(matrix);
            });
        }

        /*
         最佳拟合对其
         */

        public static VTransform BestFitAlignment(VMesh mesh, VCloud pCloud, double sampleRatio, int maxIterations, LockType lockType, CoordDirection rotateC, CoordDirection translateC, BestFitAlignType bestFitAlignType, out double sumSquaredDistance, out double pointsDistanceRMSEm, out string errMsg)
        {
            errMsg = string.Empty;

            int result = NativeAPI.BestFitAlignExcute(pCloud.LPtr, pCloud.Indexes, mesh.LPtr, sampleRatio, maxIterations, (int)lockType, (int)rotateC, (int)translateC, (int)bestFitAlignType, out var matrix, out sumSquaredDistance, out pointsDistanceRMSEm, out IntPtr msgPtr);

            return ProcessErr<VTransform>(result, msgPtr, out errMsg, () =>
            {
                // 更新点云矩阵信息
                var transform = new VTransform(matrix);
                transform.Header = new VTransform(pCloud.Matrix);
                return transform;
            });
        }

        /// <summary>
        /// 将几何对象进行变换
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static object GeometryTrans(IGeometry geometry, VTransform transform)
        {
            IGeometry retGeom = null;
            if (geometry.GetType() == typeof(VCuboid))
            {
                NativeAPI.GeometryAlign(6, geometry.LPtr, transform.GetMatrix(), out var cuboPtr);
                retGeom = new VCuboid(cuboPtr);
            }
            else
            {
                throw new NotSupportedException(geometry.GetType().Name);
            }

            return retGeom;
        }

        /// <summary>
        /// 坐标系偏移旋转
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="axis"></param>
        /// <param name="rotateAngle"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="zOffset"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VCoord CoordOffsetRoate(LCoord coord, CartCoordAxis axis, double rotateAngle, double xOffset, double yOffset, double zOffset, out string errMsg)
        {
            int result = NativeAPI.CoordOffsetRoate(coord, (int)axis, rotateAngle, xOffset, yOffset, zOffset, out LCoord outCoord, out IntPtr msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VCoord(outCoord);
            });
        }

        /// <summary>
        /// ROI转换到世界坐标系
        /// </summary>
        /// <param name="rois"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static VROIs ROIsToWorld(VROIs rois, LCoord coord)
        {
            VROIs outROIs = new VROIs();
            IntPtr msgPtr = IntPtr.Zero;
            ROIType roiType = rois.ROIType;
            foreach (var item in rois)
            {
                NativeAPI.ROIToGlobal((int)roiType, item.LPtr, coord, out var oShape, out msgPtr);

                switch (roiType)
                {
                    case ROIType.VCuboid:
                        outROIs.Add(new VCuboid(oShape));
                        break;
                    case ROIType.VSphere:
                        outROIs.Add(new VSphere(oShape));

                        break;
                    case ROIType.VCylinder:
                        outROIs.Add(new VCylinder(oShape));
                        break;
                    default:
                        throw new ArgumentException($"类型:{roiType}不存在!");
                }
            }

            return outROIs;
        }

        /// <summary>
        /// 转换到相对坐标系
        /// </summary>
        /// <param name="rois"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static VROIs ROIsToRelative(VROIs rois, LCoord coord)
        {
            VROIs outROIs = new VROIs();
            IntPtr msgPtr = IntPtr.Zero;
            ROIType roiType = rois.ROIType;
            foreach (var item in rois)
            {
                NativeAPI.ROIToRelative((int)roiType, item.LPtr, coord, out var oShape, out msgPtr);

                switch (roiType)
                {
                    case ROIType.VCuboid:
                        outROIs.Add(new VCuboid(oShape));
                        break;
                    case ROIType.VSphere:
                        outROIs.Add(new VSphere(oShape));

                        break;
                    case ROIType.VCylinder:
                        outROIs.Add(new VCylinder(oShape));
                        break;
                    default:
                        throw new ArgumentException($"类型:{roiType}不存在!");
                }
            }

            return outROIs;
        }

        /// <summary>
        /// ROI变换
        /// </summary>
        /// <param name="rois"></param>
        /// <param name="srcCoord"></param>
        /// <param name="dstCoord"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static VROIs ROIsTransform(VROIs rois, VCoord srcCoord, VCoord dstCoord)
        {
            VROIs outROIs = new VROIs();
            IntPtr msgPtr = IntPtr.Zero;
            ROIType roiType = rois.ROIType;
            LCoord src = srcCoord.ToCoord();
            LCoord dst = dstCoord.ToCoord();
            foreach (var item in rois)
            {
                //修改源坐标系和目标坐标系 
                NativeAPI.ROITransform((int)roiType, item.LPtr, src, dst, out var oShape);

                switch (roiType)
                {
                    case ROIType.VCuboid:
                        outROIs.Add(new VCuboid(oShape));
                        break;
                    case ROIType.VSphere:
                        outROIs.Add(new VSphere(oShape));

                        break;
                    case ROIType.VCylinder:
                        outROIs.Add(new VCylinder(oShape));
                        break;
                    default:
                        throw new ArgumentException($"类型:{roiType}不存在!");
                }
            }

            return outROIs;
        }
        #endregion

        #region 数模比对

        public static void CloudMesh(VCloud pCloud, VMesh mesh, double sampleRatio, ComapreDistanceType distanceType, out string errMsg)
        {
            errMsg = string.Empty;
            int pNum = pCloud.Indexes.Length == 0 ? pCloud.PointNum : pCloud.Indexes.Length;
            int result = NativeAPI.CloudMeshCompare(pCloud.LPtr, pCloud.Indexes, mesh.LPtr, pCloud.Matrix, sampleRatio, distanceType, out var disResults, out var min, out var max, out var std, out var mean, out var msgPtr);

            pCloud.Distances = disResults;

            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
            }
        }

        #endregion

        #region 卡尺工具

        /// <summary>
        /// 卡尺取点
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="geometry"></param>
        /// <param name="caliperType"></param>
        /// <param name="vDir"></param>
        /// <param name="searchDepth"></param>
        /// <param name="clearence"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VVector CaliperTool(VCloud cloud, IGeometry geometry, CaliperType caliperType, VDirection vDir, double searchDepth, double clearence, out string errMsg)
        {
            errMsg = string.Empty;
            var direction = vDir.ToVector();

            int result = NativeAPI.CaliperTool(cloud.LPtr, cloud.Indexes, geometry.LPtr, (int)caliperType, direction, searchDepth, clearence, out var point, out var msgPtr);
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(point);
            });
        }

        /// <summary>
        /// 卡尺极值工具
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="geometry"></param>
        /// <param name="caliperType"></param>
        /// <param name="vDir"></param>
        /// <param name="searchDepth"></param>
        /// <param name="clearence"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VVector CaliperExtremeTool(VCloud cloud, CaliperType caliperType, int pointType, VVector refPoint, double searchRadius, VDirection vDir, VDirection cylDir, double searchDepth, double clearence, VDirection extremeDir, double startPercent, double endPercent, VSize size, int knnSearchNum, VSize voxelSize, out string errMsg)
        {
            errMsg = string.Empty;
            var direction = vDir.ToVector();
            if (cylDir == null)
            {
                cylDir = vDir;
            }
            int result = NativeAPI.CaliperToolEx(cloud.LPtr, cloud.Indexes, (int)caliperType, pointType, refPoint.ToPoint(), searchRadius, direction, cylDir.ToVector(), searchDepth, clearence, extremeDir.ToVector(), startPercent, endPercent, size.ToSize(), knnSearchNum, voxelSize.ToSize(), out var point, out var msgPtr);
            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(point);
            });
        }

        /// <summary>
        /// 卡尺取点(CMM)
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="fitType"></param>
        /// <param name="vDir"></param>
        /// <param name="searchDepth"></param>
        /// <param name="clearence"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static VVector CaliperCMM(VCloud cloud, CaliperFitType fitType, VVector refPoint, double searchDepth, double searchRadius, VDirection vDir, out string errMsg)
        {
            errMsg = String.Empty;

            int result = NativeAPI.SimulatedCMMCaliper(cloud.LPtr, cloud.Indexes, refPoint.ToPoint(), (int)fitType, vDir.ToVector(), searchDepth, searchRadius, out var point, out var geometryPtr, out DArray outRMS, out var msgPtr);

            return ProcessErr(result, msgPtr, out errMsg, () =>
            {
                return new VVector(point);
            });
        }
        #endregion

        #region 对齐


        #endregion

        #region 截面工具
        /// <summary>
        /// 获取截面参数
        /// </summary>
        /// <param name="planeNum"></param>
        /// <param name="normal"></param>
        /// <param name="rectWidth"></param>
        /// <param name="rectHeight"></param>
        /// <param name="radius"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="arcSpan"></param>
        public static void GetToolParams(out int planeNum, out LVector arrDirection, out double rectWidth, out double rectHeight, out double radius, out LPoint3 startPoint, out LPoint3 endPoint, out double arcSpan)
        {
            GuiNativeAPI.GetToolParams(out planeNum, out arrDirection, out rectWidth, out rectHeight, out radius, out startPoint, out endPoint, out arcSpan);
        }

        /// <summary>
        /// 多截面分割点云
        /// </summary>
        /// <param name="isProjectType">是否投影</param>
        /// <param name="cloud">点云</param>
        /// <param name="secPath">路径类型</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">终止点</param>
        /// <param name="refCir">关联圆弧</param>
        /// <param name="arcSpan">圆弧跨度</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="arrayMode">阵列模式</param>
        /// <param name="genCirArcWay">生成圆弧模式</param>
        /// <param name="arrNum">阵列个数</param>
        /// <param name="arrInterval">阵列间距</param>
        /// <param name="sectionMode">截面模式</param>
        /// <param name="arrDir">阵列元素方向</param>
        /// <param name="rectWidth">矩形宽</param>
        /// <param name="rectHeight">矩形高</param>
        /// <param name="radius">圆半径</param>
        /// <param name="disThresh">距离阈值</param>
        /// <param name="enable">是否可以交互调整参数</param>
        /// <param name="enable">错误消息</param>
        /// <returns></returns>
        public static VCloud GetCloudSectionSegment(IsProjectType isProjectType, VCloud cloud, SecPathType secPath, VVector startPoint, VVector endPoint, VCircle refCir, double arcSpan, double offset, ArrayMode arrayMode, GenCirArcWay genCirArcWay, int arrNum, double arrInterval, SectionMode sectionMode, VDirection arrDir, double rectWidth, double rectHeight, double radius, double disThresh, bool enable, out string errMsg)
        {
            //阵列类型
            bool arrMode = arrayMode == ArrayMode.Number ? true : false;
            //圆弧类型
            bool arcMode = genCirArcWay == GenCirArcWay.StartEndPoint ? true : false;
            VCloud vCloud = null;
            var mes = IntPtr.Zero;
            if (isProjectType == IsProjectType.NoProject)
            {
                if (secPath == SecPathType.LineSegment)
                {
                    GuiNativeAPI.GetLineSectionROI(cloud.LPtr, cloud.Indexes, startPoint.ToPoint(), endPoint.ToPoint(), true, offset, arrMode, arrNum, arrInterval, (int)sectionMode, arrDir.ToVector(), rectWidth, rectHeight, radius, disThresh, enable, out var array, out mes);
                    vCloud = new VCloud(cloud, array);
                }
                else if (secPath == SecPathType.CircularArc)
                {
                    GuiNativeAPI.GetArcSectionROI(cloud.LPtr, cloud.Indexes, startPoint.ToPoint(), endPoint.ToPoint(), refCir.LPtr, arcSpan, arcMode, true, offset, arrMode, arrNum, arrInterval, (int)sectionMode, arrDir.ToVector(), rectWidth, rectHeight, radius, disThresh, enable, out var array, out mes);
                    vCloud = new VCloud(cloud, array);
                }
                else
                {
                    GuiNativeAPI.GetCircleSectionROI(cloud.LPtr, cloud.Indexes, startPoint.ToPoint(), refCir.LPtr, true, offset, arrMode, arrNum, arrInterval, (int)sectionMode, arrDir.ToVector(), rectWidth, rectHeight, radius, disThresh, enable, out var array, out mes);
                    vCloud = new VCloud(cloud, array);
                }
            }
            else
            {
                if (secPath == SecPathType.LineSegment)
                {
                    GuiNativeAPI.StartLineSectionTool(cloud.LPtr, cloud.Indexes, startPoint.ToPoint(), endPoint.ToPoint(), true, offset, arrMode, arrNum, arrInterval, (int)sectionMode, arrDir.ToVector(), rectWidth, rectHeight, radius, disThresh, enable, out var cloudP, out mes);
                    vCloud = new VCloud(cloudP);
                }
                else if (secPath == SecPathType.CircularArc)
                {
                    GuiNativeAPI.StartArcSectionTool(cloud.LPtr, cloud.Indexes, startPoint.ToPoint(), endPoint.ToPoint(), refCir.LPtr, arcSpan, arcMode, true, offset, arrMode, arrNum, arrInterval, (int)sectionMode, arrDir.ToVector(), rectWidth, rectHeight, radius, disThresh, enable, out var cloudP, out mes);
                    vCloud = new VCloud(cloudP);
                }
                else
                {
                    GuiNativeAPI.StartCircleSectionTool(cloud.LPtr, cloud.Indexes, startPoint.ToPoint(), refCir.LPtr, true, offset, arrMode, arrNum, arrInterval, (int)sectionMode, arrDir.ToVector(), rectWidth, rectHeight, radius, disThresh, enable, out var cloudP, out mes);
                    vCloud = new VCloud(cloudP);
                }
            }
            int ret = 0;
            if (mes != IntPtr.Zero)
            {
                ret = -1;
            }
            return ProcessErr(ret, mes, out errMsg, () =>
            {
                return vCloud;
            });

        }

        /// <summary>
        /// 结束截面工具
        /// </summary>
        public static void EndSectionTool()
        {
            GuiNativeAPI.EndSectionTool();
        }
        #endregion
    }
}