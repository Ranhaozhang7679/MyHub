#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NativeMethods
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm
* 文 件 名:       NativeMethods.cs
* 创建时间:       2021/11/15 16:50:38
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      d2c76a5b-2a36-43cb-8937-8f7bd18b0325
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/15 16:50:38
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Luster.ThreeD.Algorithm.Enums;
using Luster.Common.DataStruct.Enums;
using Luster.ThreeD.Algorithm.Structs;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// API 对象
    /// </summary>
    public partial class NativeAPI
    {
        /// <summary>
        /// 算法Dll文件
        /// </summary>
        private const string DllPath = "Algorithm\\Luster3DAlgorithm.dll";

        #region 1.0 对象管理

        public enum ClassType
        {
            PCloud,
            MeshData,
            InitAlign,
            Line,
            Plane,
            Circle,
            Sphere,
            Cuboio,
            Cylinder,
            Cone,
            ModelMatch,
        };

        public enum ArrayType
        {
            IntArr,
            DoubleArr,
            CharArr
        };

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="ptr">对象指针</param>
        /// <param name="cType">对象类型</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void DeleteObj(IntPtr ptr, ClassType cType);

        /// <summary>
        /// 删除数组
        /// </summary>
        /// <param name="ptr">数组指针</param>
        /// <param name="AType">数组类型</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
         SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void DeleteArr(IntPtr ptr, ArrayType AType);

        #endregion

        #region 2.0 IO读取

        /// <summary>
        /// 从Ply文件读取点云
        /// </summary>
        /// <param name="file">文件地址</param>
        /// <param name="onlyPoints">是否仅读取点（是：不读取法向信息；否：读取法向信息）</param>
        /// <param name="ptr">读取的点云指针</param>
        /// <param name="msg">提示信息指针</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ReadPly(string file, bool onlyPoints, out IntPtr ptr, out IntPtr msg);

        /// <summary>
        /// 从TXT文件读取点云
        /// </summary>
        /// <param name="file">文件地址</param>
        /// <param name="onlyPoints">是否仅读取点（是：不读取法向信息；否：读取法向信息）</param>
        /// <param name="ptr">读取的点云指针</param>
        /// <param name="msg">提示信息指针</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ReadTxt(string file, bool onlyPoints, out IntPtr ptr, out IntPtr msg);

        /// <summary>
        /// 将点云写入文件
        /// </summary>
        /// <param name="pCloud">点云指针</param>
        /// <param name="indexes">点云Index</param>
        /// <param name="saveFile">存储地址</param>
        /// <param name="fileType">存储类型</param>
        /// <param name="isBinary">是否存储为二进制</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int WritePCloud(IntPtr pCloud, IArray indexes, string saveFile, string fileType, bool isBinary);


        /// <summary>
        /// 读取STL网格数据
        /// </summary>
        /// <param name="file">网格文件地址</param>
        /// <param name="ptr">网格对象指针</param>
        /// <param name="msg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ReadStl(string file, out IntPtr ptr, out IntPtr msg);

        /// <summary>
        /// 获取网格数据详情
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="meshPtr"></param>
        /// <param name="pLen"></param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetTriMesh(IntPtr voxelMesh, out IntPtr triMesh, out int pLen);

        /// <summary>
        /// 非托管数据转换到托管数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="ptr">非托管指针</param>
        /// <param name="plen">对象个数</param>
        /// <returns>托管对象</returns>
        public static T[] PtrToStructure<T>(IntPtr ptr, int plen)
        {
            var size = Marshal.SizeOf(typeof(T));
            Type type = typeof(T);
            T[] ps = new T[plen];
            for (int i = 0; i < plen; i++)
            {
                ps[i] = (T)Marshal.PtrToStructure((IntPtr)(ptr + i * size), type);
            }

            return ps;
        }

        /// <summary>
        /// 通过线激光来解析点云数据（单位默认：mm）
        /// </summary>
        /// <param name="row">行数</param>
        /// <param name="rowDis">行距</param>
        /// <param name="col">列数</param>
        /// <param name="colDis">列间距</param>
        /// <param name="zPointer">Z向高度数据指针</param>
        /// <param name="resolution">Z向分辨率</param>
        /// <param name="min">最小有效值</param>
        /// <param name="max">最大有效值</param>
        /// <param name="offset">偏移量</param>
        /// <param name="invert">是否反向</param>
        /// <param name="dataType">Z向数据类型 1-short 2-int 4-float 5-double</param>
        /// <param name="cloud">返回点云</param>
        /// <returns> 0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ParseLineLaser(int row, double rowDis, int col, double colDis, IntPtr zPointer, double resolution, double min, double max, LSize3 offset, bool invert, int dataType, out IntPtr cloud);

        #endregion

        #region 3.0 坐标系

        /// <summary>
        /// 基于原点和方向来构建右手坐标系
        /// </summary>
        /// <param name="origin">坐标原点</param>
        /// <param name="axis">构建坐标轴系类型 0-xy 1-xz 2 yz</param>
        /// <param name="xAxis">X轴方向</param>
        /// <param name="yAxis">Y轴方向</param>
        /// <param name="zAxis">Z轴方向</param>
        /// <param name="isFlipX">是否将X轴进行反向</param>
        /// <param name="isFlipY">是否将Y轴进行反向</param>
        /// <param name="isFlipZ">是否将Z轴进行反向</param>
        /// <param name="outCoord">输出新的坐标系</param>
        /// <param name="msgPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GenCoordByData(LPoint3 origin, int axis, LVector xAxis, LVector yAxis, LVector zAxis, bool isFlipX, bool isFlipY, bool isFlipZ, out LCoord outCoord, out IntPtr msgPtr);

        /// <summary>
        /// 三面构建右手坐标系
        /// </summary>
        /// <param name="plane1">X轴垂直平面</param>
        /// <param name="plane2">Y轴垂直平面</param>
        /// <param name="plane3">Z轴垂直平面</param>
        /// <param name="isFlipX">是否将X轴进行反向</param>
        /// <param name="isFlipY">是否将Y轴进行反向</param>
        /// <param name="isFlipZ">是否将Z轴进行反向</param>
        /// <param name="outCoord">构建的坐标系</param>
        /// <param name="msgPtr">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetCoordByThreePlane(IntPtr plane1, IntPtr plane2, IntPtr plane3, bool isFlipX, bool isFlipY, bool isFlipZ, out LCoord outCoord, out IntPtr msgPtr);

        /// <summary>
        ///  坐标系偏移旋转
        /// </summary>
        /// <param name="coord">输入坐标系</param>
        /// <param name="axis">旋转轴。 0-X 1-Y 2-Z</param>
        /// <param name="rotateAngle">旋转角度</param>
        /// <param name="xOffset">X方向移动量</param>
        /// <param name="yOffset">Y方向移动量</param>
        /// <param name="zOffset">Z方向移动量</param>
        /// <param name="outCoord">计算所得坐标系</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns> 0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
     SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CoordOffsetRoate(LCoord coord, int axis, double rotateAngle, double xOffset, double yOffset, double zOffset, out LCoord outCoord, out IntPtr errMsg);

        /// <summary>
        /// 逆矩阵求解
        /// </summary>
        /// <param name="matrix">原矩阵</param>
        /// <param name="invMatrix">所求逆矩阵</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int InvertTransform(ref double matrix, out LMatrix invMatrix);

        /// <summary>
        /// 矩阵乘法
        /// </summary>
        /// <param name="m1">矩阵1</param>
        /// <param name="m2">矩阵2</param>
        /// <param name="matrix">所求结果矩阵</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int MatrixCross(ref double m1, ref double m2, out LMatrix matrix);

        /// <summary>
        /// 通过相对坐标系，将点转换到世界坐标系下表示
        /// </summary>
        /// <param name="point3">源点坐标</param>
        /// <param name="relativeCoord">相对坐标系</param>
        /// <param name="outPoint">变换后的点坐标</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int PointToGlobal(LPoint3 point3, LCoord relativeCoord, out LPoint3 outPoint);

        /// <summary>
        /// 通过相对坐标系，将向量转换到世界坐标系
        /// </summary>
        /// <param name="vector">源向量</param>
        /// <param name="relativeCoord">相对坐标系</param>
        /// <param name="outVector">变换后的向量</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int VectorToGlobal(LVector vector, LCoord relativeCoord, out LVector outVector);

        /// <summary>
        /// 通过相对坐标系，将世界坐标系下的点转换到相对坐标系下表示
        /// </summary>
        /// <param name="point3">源点坐标</param>
        /// <param name="relativeCoord">相对坐标系</param>
        /// <param name="outPoint">变换后的点坐标</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int PointToRelative(LPoint3 point3, LCoord relativeCoord, out LPoint3 outPoint);

        /// <summary>
        /// 通过相对坐标系，将向量转换到相对坐标系下表示
        /// </summary>
        /// <param name="vector">源向量</param>
        /// <param name="relativeCoord">相对坐标系</param>
        /// <param name="outVector">变换后的向量</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int VectorToRelative(LVector vector, LCoord relativeCoord, out LVector outVector);

        /// <summary>
        /// 通过相对坐标系，将ROI转换到世界坐标系中
        /// </summary>
        /// <param name="roiType">ROI类型，0-长方体 1-球 2-圆柱</param>
        /// <param name="shpae">形状数据指针</param>
        /// <param name="relativeCoord">相对坐标系</param>
        /// <param name="outShape">变换后的ROI对象指针</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ROIToGlobal(int roiType, IntPtr shpae, LCoord relativeCoord, out IntPtr outShape, out IntPtr errMsg);

        /// <summary>
        /// 通过相对坐标系，将ROI转换到相对坐标系中
        /// </summary>
        /// <param name="roiType">ROI类型，0-长方体 1-球 2-圆柱</param>
        /// <param name="shpae">形状数据指针</param>
        /// <param name="relativeCoord">相对坐标系</param>
        /// <param name="outShape">变换后的ROI对象指针</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ROIToRelative(int roiType, IntPtr shpae, LCoord relativeCoord, out IntPtr outShape, out IntPtr errMsg);

        /// <summary>
        /// 将ROI从源坐标系转换到目标坐标系中
        /// </summary>
        /// <param name="roiType">ROI类型，0-长方体 1-球 2-圆柱</param>
        /// <param name="shpae">形状数据指针</param>
        /// <param name="src">源坐标系</param>
        /// <param name="dst">目标坐标系</param>
        /// <param name="outShape">变换后的ROI对象指针</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ROITransform(int roiType, IntPtr shpae, LCoord src, LCoord dst, out IntPtr outShape);
        #endregion

        #region 4.0 对其


        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
           SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ModelMatchTrain(IntPtr  pModCloud,  int iMaxResNum, double dMinScore, double dOverlapRate, double dRelDiameter, int iFeaturePointMode, double dEdgeSigMulScale, int iKnnSeaNum, double dModRefDisSca, double dSenceRefDisSca, double dSenceSampleScale, out IntPtr modelMatch, out IntPtr errMsg);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
           SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ModelMatchExcute(IntPtr modelMatch , IntPtr pSenCloud,  out LMatrix outMatrix, out IntPtr errMsg);

        /// <summary>
        /// 对原始点云进行变换
        /// </summary>
        /// <param name="pCloud">输入原始点云数据</param>
        /// <param name="iArr">变换数据索引。为空时，表示所有点云数据进行变换</param>
        /// <param name="matrix">变换矩阵</param>
        /// <param name="invert">是否进行逆变换</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
           SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int TransformCloudBySource(IntPtr pCloud, IArray iArr, double[] matrix, bool invert);

        /// <summary>
        ///  预对齐工具-训练网格模型
        /// </summary>
        /// <param name="voxelMesh">网格数据指针</param>
        /// <param name="shapeComplexity">形状复杂度。用于指定模型训练的精细程度，分为：超简单（默认值）、简单、一般、复杂、超复杂</param>
        /// <param name="searchTime">搜索时间。目标匹配过程中，指定对齐大致计算可持续的时间，分为：超短（默认值）、短、普通、长、超长</param>
        /// <param name="invertNormals">指定是否修正目标点云法线的方向，若启用时，目标匹配过程中其法线方向会反向，默认不启用该参数。</param>
        /// <param name="setBestFit">是否执行最佳拟合对齐。</param>
        /// <param name="alignPtr">预对齐工具指针</param>
        /// <param name="errPtr">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int InitAlignTrain(IntPtr voxelMesh, int shapeComplexity, int searchTime, bool invertNormals, bool setBestFit, out IntPtr alignPtr, out IntPtr errPtr);

        /// <summary>
        ///  预对齐-快速匹配
        /// </summary>
        /// <param name="alignFunc">预对齐工具指针</param>
        /// <param name="pointCloud">点云数据</param>
        /// <param name="rms">误差</param>
        /// <param name="martix">点云数据到网格模型的刚性变换矩阵</param>
        /// <param name="errPtr">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int InitAlignExcute(IntPtr alignFunc, IntPtr pointCloud, out double rms, out LMatrix martix, out IntPtr errPtr);

        /// <summary>
        /// 最佳拟合对其
        /// </summary>
        /// <param name="pointCloud">输入点云数据</param>
        /// <param name="indexes">变换数据索引</param>
        /// <param name="voxelMesh">载入stl理论模型</param>
        /// <param name="sampleRatio">采样比率</param>
        /// <param name="maxIterations">最大重复次数</param>
        /// <param name="lockType"> 约束类型</param>
        /// <param name="rotateLock">旋转约束</param>
        /// <param name="translateLock">平移约束</param>
        /// <param name="alignmentType">对齐方式：0-快速对齐  1-鲁棒性对齐</param>
        /// <param name="martix">点云数据到网格模型的刚性变换矩阵</param>
        /// <param name="sumSquaredDistance">输入点云数据和模板点云数据之间的距离平方之和</param>
        /// <param name="pointsDistanceRMSEm">输入点云数据到模板数据之间的距离均方根误差RMSE</param>
        /// <param name="errPtr">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int BestFitAlignExcute(IntPtr pointCloud, IArray indexes, IntPtr voxelMesh, double sampleRatio, int maxIterations, int lockType, int rotateLock, int translateLock, int alignmentType, out LMatrix martix, out double sumSquaredDistance, out double pointsDistanceRMSEm, out IntPtr errPtr);

        /// <summary>
        /// 对点云数据执行RPS对齐
        /// </summary>
        /// <param name="pCloud">点云</param>
        /// <param name="effectiveDirectType">有效方向：指定点对中实际进行对齐中使用的方向</param>
        /// <param name="lockType">对齐约束</param>
        /// <param name="iterations">最大迭代次数。</param>
        /// <param name="maxDistanceThreshold">最大偏差距离阈值。</param>
        /// <param name="minPercentage">最小定额</param>
        /// <param name="modelArr">模型点集合</param>
        /// <param name="cloudArr">点云点集合 </param>
        /// <param name="outMatrix">点云数据到网格模型的刚性变换矩阵</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int RPSAlignExcute(IntPtr pCloud, int effectiveDirectType, int lockType, int iterations, double maxDistanceThreshold, double minPercentage, LPointArray modelArr, LPointArray cloudArr, out LMatrix outMatrix, out IntPtr errMsg);

        /// <summary>
        /// 对点云数据执行RPS和最佳拟合组合对齐
        /// </summary>
        /// <param name="pointCloud">输入点云数据</param>
        /// <param name="indexes">变换数据索引。indexes为空时，表示所有点云数据进行变换</param>
        /// <param name="voxelMesh">网格模型</param>
        /// <param name="sampleRatio">采样比率</param>
        /// <param name="iterations">最大重复次数</param>
        /// <param name="modelArr">模型点集合</param>
        /// <param name="cloudArr">点云点集合</param>
        /// <param name="isBest">是否执行最佳拟合</param>
        /// <param name="lockType">约束类型</param>
        /// <param name="rotateLock">旋转约束</param>
        /// <param name="translateLock">平移约束</param>
        /// <param name="alignmentType">对齐方式 0-快速对齐  1-鲁棒性对齐</param>
        /// <param name="outMatrix">点云数据到网格模型的刚性变换矩阵</param>
        /// <param name="sumSquaredDistance">输入点云数据和模板点云数据之间的距离平方之和</param>
        /// <param name="pointsDistanceRMSEm">输入点云数据到模板数据之间的距离均方根误差RMSE</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
       SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int RPSBestFitAlign(IntPtr pointCloud, IArray indexes, IntPtr voxelMesh, double sampleRatio, int iterations, LPointArray modelArr, LPointArray cloudArr, bool isBest, int lockType, int rotateLock, int translateLock, int alignmentType, out LMatrix outMatrix, out double sumSquaredDistance, out double pointsDistanceRMSEm, out IntPtr errMsg);

        /// <summary>
        /// 坐标系对齐
        /// </summary>
        /// <param name="pointCloud">输入点云数据</param>
        /// <param name="indexes">变换数据索引。indexes为空时，表示所有点云数据进行对齐</param>
        /// <param name="src">源坐标系</param>
        /// <param name="dst">目标坐标系</param>
        /// <param name="isTrans">是否对输入点云数据进行变换</param>
        /// <param name="outMatrix">点云数据到目标坐标系的刚性变换矩阵</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
         SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CoordinateAlign(IntPtr pointCloud, IArray indexes, LCoord src, LCoord dst, bool isTrans, out LMatrix outMatrix, out IntPtr errMsg);

        /// <summary>
        /// 几何形状对齐变换
        /// </summary>
        /// <param name="geom">几何形状类型：0 点；  1 线； 2 面；3 圆 ；4 圆柱体；5 圆锥 ；6 长方体 ；7 球体</param>
        /// <param name="geoPtr">几何形状指针</param>
        /// <param name="matrx">变换矩阵</param>
        /// <param name="outGeom">变换后的几何形状指针对象</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
         SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GeometryAlign(int geom, IntPtr geoPtr, LMatrix matrx, out IntPtr outGeom);
        #endregion

        #region 5.0 网格比较

        /// <summary>
        /// 点云网格比对
        /// </summary>
        /// <param name="pointCloud"> 输入点云数据</param>
        /// <param name="indexes">对比数据索引。注：使用指定下标时，不会进行采样处理</param>
        /// <param name="voxelMesh">网格数据</param>
        /// <param name="matrix">暂时不使用该矩阵</param>
        /// <param name="sampleRatio">采样比例，默认为1</param>
        /// <param name="distanceType">距离类型</param>
        /// <param name="disResults">误差结果集合</param>
        /// <param name="min">误差最小值</param>
        /// <param name="max">误差最大值</param>
        /// <param name="std">距离偏差结果的标准差</param>
        /// <param name="mean">距离偏差结果的均值</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudMeshCompare(IntPtr pointCloud, IArray indexes, IntPtr voxelMesh, LMatrix matrix, double sampleRatio, ComapreDistanceType distanceType, out DArray disResults, out double min, out double max, out double std, out double mean, out IntPtr errMsg);

        #endregion

        #region 6.0 点云刚体变换

        /// <summary>
        /// 点云变换
        /// </summary>
        /// <param name="pointCloud">输入点云数据</param>
        /// <param name="indexes">对比数据索引</param>
        /// <param name="matrix">变换矩阵</param>
        /// <param name="outCloud">变换后的点云</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudTransform(IntPtr pointCloud, IArray indexes, ref double matrix, out IntPtr outCloud);

        /// <summary>
        /// 点云平移
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">平移数据索引</param>
        /// <param name="in_offsetDir">平移方向</param>
        /// <param name="offset">沿着平移方向的平移量</param>
        /// <param name="outCloud">平移后的点云</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudOffset(IntPtr pCloud, IArray indexes, LVector in_offsetDir, double offset, out IntPtr outCloud);

        /// <summary>
        /// 点云绕轴旋转
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">平移数据索引</param>
        /// <param name="xRotateAngle">绕X轴旋转角度°</param>
        /// <param name="yRotateAngle">绕Y轴旋转角度°</param>
        /// <param name="zRotateAngle">绕Z轴旋转角度°</param>
        /// <param name="outCloud">旋转后点云</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudRotate(IntPtr pCloud, IArray indexes, double xRotateAngle, double yRotateAngle, double zRotateAngle, out IntPtr outCloud);

        /// <summary>
        /// 点云镜像
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">镜像数据索引</param>
        /// <param name="aroundXY">是否绕Z轴做翻转</param>
        /// <param name="aroundXZ">是否绕Y轴做翻转</param>
        /// <param name="aroundYZ">是否绕X轴做翻转</param>
        /// <param name="outCloud">输出点云</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudFilp(IntPtr pCloud, IArray indexes, bool aroundXY, bool aroundXZ, bool aroundYZ, out IntPtr outCloud);

        /// <summary>
        /// 通过旋转角度和平移量，构建变换矩阵
        /// </summary>
        /// <param name="rX">绕X轴旋转角度°</param>
        /// <param name="rY">绕Y轴旋转角度°</param>
        /// <param name="rZ">绕Z轴旋转角度°</param>
        /// <param name="mX">沿X方向平移量 mm</param>
        /// <param name="mY">沿Y方向平移量 mm</param>
        /// <param name="mZ">沿Z方向平移量 mm</param>
        /// <param name="outMatrix">变换矩阵</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int DataToTranform(double rX, double rY, double rZ, double mX, double mY, double mZ, ref LMatrix outMatrix);

        #endregion

        #region 7.0 点云配准

        /// <summary>
        /// 点云拼接
        /// </summary>
        /// <param name="pCloud">输入点云集合</param>
        /// <param name="matrixs">输入变换矩阵集合</param>
        /// <param name="calibNum">拼接数量</param>
        /// <param name="calibCloud">拼接后的点云</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudRegistration(IntPtr[] pCloud, LMatrix[] matrixs, int calibNum, out IntPtr calibCloud, out IntPtr errMsg);

        /// <summary>
        /// 点云去重
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="roiNum">ROI个数</param>
        /// <param name="subShape">ROI对象指针集合</param>
        /// <param name="smoothMode">平滑模式 ：快速模式、高精模式和超高精模式</param>
        /// <param name="iterNum">平滑程度。指定平滑的迭代次数</param>
        /// <param name="mergeMode">合并模式。包含仅平滑、体素采样、邻域均值和均值漂移</param>
        /// <param name="searchMode"> 邻域搜索方法。包含体素近邻搜索和KNN近邻搜索。</param>
        /// <param name="kPointNum">合并搜索近邻K。</param>
        /// <param name="voxelSize">体素尺寸大小。指定体素网格大小，仅在体素近邻搜索时使用。</param>
        /// <param name="calibCloud">去重后的的点云</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudReRepeat(IntPtr pCloud, int roiNum, int roiType, IntPtr[] subShape, int smoothMode, int iterNum, int mergeMode, int searchMode, int kPointNum, LSize3 voxelSize, out IntPtr calibCloud, out IntPtr errMsg);

        #endregion

        #region 8.0 点云去噪、降采样等预处理

        /// <summary>
        /// 获取点云信息
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">点云数据索引</param>
        /// <param name="outNum">点个数</param>
        /// <param name="center">暂时舍弃</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetCloudInfo(IntPtr pCloud, IArray indexes, out int outNum, out LPoint3 center);

        /// <summary>
        /// 通过点构建点云
        /// </summary>
        /// <param name="vectors">点集合</param>
        /// <param name="num">点个数</param>
        /// <param name="cloudPtr">构建所得点云指针对象</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GenCloudByPoints(LPoint3[] vectors, int num, out IntPtr cloudPtr);

        /// <summary>
        /// 点云滤波
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据进行滤波</param>
        /// <param name="filterMode">滤波模式。包含统计学和体素滤波</param>
        /// <param name="pointNeighsNum">各点邻域的搜索点数</param>
        /// <param name="stddevCoeffThreshold">标准偏差系数阈值</param>
        /// <param name="voxelGridLeafSize">体素尺寸</param>
        /// <param name="neighsNumThreshold"> 邻域点数阈值</param>
        /// <param name="checkNeighsNumThreshSetAuto">是否自动计算邻域点数阈值</param>
        /// <param name="filterNegative">是否反向滤波</param>
        /// <param name="outPointCloud">输出点云数据</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int Denoising(IntPtr pCloud, IArray indexes, FilterMode filterMode, int pointNeighsNum, double stddevCoeffThreshold, LSize3 voxelGridLeafSize, int neighsNumThreshold, bool checkNeighsNumThreshSetAuto, bool filterNegative, out IntPtr outPointCloud, out IntPtr errMsg);

        /// <summary>
        /// 点云降采样
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据进行降采样</param>
        /// <param name="sampleMethod">采样方法。包含随机采样、固定索引步长采样、空间点距采样和曲率采样</param>
        /// <param name="sampleRatio">采样比例。仅随机采样和曲率采样有效。</param>
        /// <param name="indicesSampleStep">索引步长。指定降采样的索引步长，仅固定索引步长采样有效。</param>
        /// <param name="spaceDistBetweenPoints">空间点距。指定降采样后各点的间隔，仅空间点距采样有效。</param>
        /// <param name="outPointCloud">输出点云数据</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int Downsamping(IntPtr pCloud, IArray indexes, SampleMethod sampleMethod, double sampleRatio, int indicesSampleStep, double spaceDistBetweenPoints, out IntPtr outPointCloud, out IntPtr errMsg);

        /// <summary>
        /// 基于移动最小二乘平滑的点云平滑
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据进行平滑</param>
        /// <param name="searchMethod">搜索方法。指定近邻搜索方法为KNN近邻搜索(暂不支持 相对半径搜索或绝对半径搜索)</param>
        /// <param name="knnSearchNum">邻域搜索点数</param>
        /// <param name="searchRadius">搜索半径。</param>
        /// <param name="projectionType">投影方法。包含不投影、简单投影和垂直投影。</param>
        /// <param name="upSample">上采样方法。包含不采样、局部平面、均匀分布和体素扩增。</param>
        /// <param name="size">体素尺寸。针对体素扩增上采样方法设置，指定点云体素划分的体素尺寸大小。</param>
        /// <param name="diationNum">体素扩增次数。针对体素扩增上采样方法设置，指定体素扩增上采样的次数</param>
        /// <param name="autoCloudResolution"> 是否自动计算点云分辨率</param>
        /// <param name="cloudResolution">点云分辨率</param>
        /// <param name="outPointCloud">输出点云数据</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int Smooth(IntPtr pCloud, IArray indexes, SearchNeighMethod searchMethod, int knnSearchNum, double searchRadius, int projectionType, int upSample, LSize3 size, int diationNum, bool autoCloudResolution, double cloudResolution, out IntPtr outPointCloud, out IntPtr errMsg);

        /// <summary>
        /// 点云边缘提取
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indices"> 数据索引。regionIndexes为空时，表示所有点云数据进行边缘提取</param>
        /// <param name="roicount">ROI数目</param>
        /// <param name="roiType">ROI类型</param>
        /// <param name="subIndices">ROI指针对象集合</param>
        /// <param name="planePtr">投影面</param>
        /// <param name="knnSearchNum">KNN领域搜索点[1-200]</param>
        /// <param name="distThresh">局外点距离阀值[0.0001,1]</param>
        /// <param name="sigma">边缘因子[-10,10]</param>
        /// <param name="xDis">X方向内缩距离</param>
        /// <param name="yDis">Y方向内缩距离</param>
        /// <param name="zDis">Z方向内缩距离</param>
        /// <param name="pointToPlaneDisThresh">roi区域点到基准面距离阀值</param>
        /// <param name="bAmplifyMode">是否开启内部扩增模式</param>
        /// <param name="cloud">输出点云数据</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudExtractEdge(IntPtr pCloud, IArray indices, int roicount, int roiType, IntPtr[] subIndices, IntPtr planePtr, int knnSearchNum, double distThresh, double sigma, double xDis, double yDis, double zDis, double pointToPlaneDisThresh, bool bAmplifyMode, out IntPtr cloud, out IntPtr errMsg);


        /// <summary>
        /// 计算点云法向信息
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="cloud">输出点云</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudNormalCalculation(IntPtr pCloud,  out IntPtr cloud, out IntPtr errMsg);

        /// <summary>
        /// 点云投影到平面
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据进行投影</param>
        /// <param name="in_plane">投影平面</param>
        /// <param name="outCloud">输出点云数据</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudProjectedToPlane(IntPtr pCloud, IArray indexes, IntPtr in_plane, out IntPtr outCloud, out IntPtr errMsg);
        #endregion

        #region 9.0 点云分割


        /// <summary>
        /// 多ROI的点云分割
        /// </summary>
        /// <param name="pCloud">输入点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据进行分割。受isIndex影响。</param>
        /// <param name="roiType">几何体类型。 0-长方体 1-球体 2-圆柱体</param>
        /// <param name="roiNum">几何体ROI的数量</param>
        /// <param name="shape">几何体ROI数据</param>
        /// <param name="invertCloud">反向选择。原本应被输出的点云将不会输出，原本不被输出的点云将被输出。</param>
        /// <param name="distance">在几何体表面指定距离以内取点，内部取绝对值。 设定值等于0，表示仅内部取点；非0表示取指定距离<abs(distance)>的几何表面的点</param>
        /// <param name="outIndexes">输出点云对应的数据索引</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CloudSegment(IntPtr pCloud, IArray indexes, ROIType roiType, int roiNum, IntPtr[] shape, bool invertCloud, double distance, out IArray outIndexes, out IntPtr errMsg);

        #endregion

        #region 10.0 卡尺工具

        /// <summary>
        /// 卡尺工具（球体和圆柱体）
        /// </summary>
        /// <param name="in_pointCloud">输入点云</param>
        /// <param name="index">数据索引。indexes为空时，表示所有点云数据进行计算</param>
        /// <param name="in_caliper">卡尺指针对象</param>
        /// <param name="caliperType">卡尺类型 0 球 ； 1 圆柱；2 CMM </param>
        /// <param name="vecDirect">探触方向</param>
        /// <param name="searchDepth">探触深度</param>
        /// <param name="dClearence">余隙</param>
        /// <param name="out_point">所求点</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CaliperTool(IntPtr in_pointCloud, IArray index, IntPtr in_caliper, int caliperType, LVector vecDirect, double searchDepth, double dClearence, out LPoint3 out_point, out IntPtr errMsg);

        /// <summary>
        /// 卡尺工具（模拟CMM点）
        /// </summary>
        /// <param name="in_pointCloud">输入点云</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据进行计算</param>
        /// <param name="in_referPoint">参考点位置</param>
        /// <param name="fitType">模拟CMM点拟合类型</param>
        /// <param name="vecDirect">探触方向。即卡尺测量方向，卡尺沿该方向与点云数据接触取点。</param>
        /// <param name="searchDepth">探触深度。卡尺参考点沿探触方向定义的搜索区域深度。</param>
        /// <param name="searchRadius">搜索半径。卡尺参考点沿探触方向定义的搜索区域半径。</param>
        /// <param name="out_point">输出点结果。由于是拟合面求线面交点，因此取点结果为虚拟点。</param>
        /// <param name="out_Geometry"> 模拟CMM点拟合结果</param>
        /// <param name="out_PointDev">模拟CMM参考点与结果点偏差。 数组大小为4，分别为X偏差/Y偏差/Z偏差/沿探触方向偏差</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int SimulatedCMMCaliper(IntPtr in_pointCloud, IArray indexes, LPoint3 in_referPoint, int fitType, LVector vecDirect, double searchDepth, double searchRadius, out LPoint3 out_point, out IntPtr out_Geometry, out DArray out_PointDev, out IntPtr errMsg);


        /// <summary>
        /// 卡尺取点+极值
        /// </summary>
        /// <param name="pCloud">输入点云</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据进行计算</param>
        /// <param name="caliperType">参考点位置</param>
        /// <param name="pointType">卡尺点计算方法（仅对球形/圆柱卡尺生效）</param>
        /// <param name="refPoint"> 参考点。用于构建卡尺第一次取点的ROI</param>
        /// <param name="radius">半径。用于构建卡尺第一次取点的ROI</param>
        /// <param name="vecDirect">探触方向。即卡尺测量方向，卡尺沿该方向与点云数据接触取点。</param>
        /// <param name="cylDirect">圆柱方向</param>
        /// <param name="searchDepth">探触深度</param>
        /// <param name="dClearence">余隙。球或圆柱初始位置沿探触方向移动的距离</param>
        /// <param name="extremeDir">极值点取点方向</param>
        /// <param name="startPercent">起始比例</param>
        /// <param name="endPercent">终止比例</param>
        /// <param name="geoSize">点云极值对应的长方体尺寸</param>
        /// <param name="knnSearchNum">平滑K近邻搜索领域点数,现在设置为0 将不进行平滑直接进行极值点计算</param>
        /// <param name="voxelSize"> 平滑体素尺寸</param>
        /// <param name="out_point">输出点结果</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CaliperToolEx(IntPtr pCloud, IArray indexes, int caliperType, int pointType, LPoint3 refPoint, double radius, LVector vecDirect, LVector cylDirect, double searchDepth, double dClearence, LVector extremeDir, double startPercent, double endPercent, LSize3 geoSize, int knnSearchNum, LSize3 voxelSize, out LPoint3 out_point, out IntPtr errMsg);


        /// <summary>
        /// 点云极值点获取
        /// </summary>
        /// <param name="in_pointCloud">点云数据</param>
        /// <param name="indexes">数据索引</param>
        /// <param name="calcMethod">极值点类型 : ROI和对象构建两种模式下均有 均值；中值；最大值；最小值；投影均值 </param>
        /// <param name="refPoint">参考点</param>
        /// <param name="radius">ROI半径</param>
        /// <param name="vecDirect">极值点方向</param>
        /// <param name="startPercent">卡尺点计算时使用的数据起始比例</param>
        /// <param name="endPercent">卡尺点计算时使用的数据终止比例</param>
        /// <param name="out_point">输出极值点</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetExtremePointByCloud(IntPtr in_pointCloud, IArray indexes, int calcMethod, LPoint3 refPoint, double radius, LVector vecDirect, double startPercent, double endPercent, out LPoint3 out_point, out IntPtr errMsg);

        #endregion


    }
}