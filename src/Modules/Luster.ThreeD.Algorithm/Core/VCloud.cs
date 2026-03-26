#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PointCloud
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm
* 文 件 名:       PointCloud.cs
* 创建时间:       2021/11/15 16:59:05
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      a5ec243b-0921-4c4d-a1e2-9109051fa125
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/15 16:59:05
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.Common.DataStruct;
using Luster.ThreeD.Algorithm.Interfaces;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Luster.ThreeD.Algorithm.ROIs;
using Luster.ThreeD.Algorithm.Enums;

namespace Luster.ThreeD.Algorithm
{
    /// <summary>
    /// 点云
    /// </summary>
    public class VCloud : LDisposable, IActor, ISave, ICloneable
    {
        /// <summary>
        /// 点云索引
        /// </summary>
        public IArray Indexes { get; set; }

        /// <summary>
        /// 距离值
        /// </summary>
        public DArray Distances { get; set; }

        /// <summary>
        /// 矩阵信息
        /// </summary>
        public LMatrix Matrix { get; set; }

        /// <summary>
        /// 点云数量
        /// </summary>
        public int PointNum { get; set; } = 0;

        /// <summary>
        /// 点云中心
        /// </summary>
        [OutProperty("点云中心")]
        public VVector CenterPoint { get { return GetCenter(); } set { } }

        public bool IsShowBoundBox { get; set; } = false;

        /// <summary>
        /// 是否index来源于clone，因为Indexes是结构体，复制实际上是对原来对象的copy,所以需要一个对象来判断该对象是否需要释放
        /// </summary>
        private bool isIndexRef = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public VCloud() : base(IntPtr.Zero)
        {
            InitCloudInfo();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vCloud"></param>
        /// <param name="indexes"></param>
        public VCloud(VCloud vCloud, IArray indexes)
        {
            this.IsReference = true;
            this.lptr = vCloud.LPtr;
            this.Indexes = indexes;
            InitCloudInfo();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vCloud"></param>
        /// <param name="indexes"></param>
        public VCloud(VCloud vCloud)
        {
            this.IsReference = true;
            this.lptr = vCloud.LPtr;
            this.Indexes = vCloud.Indexes;
            isIndexRef = true;
            InitCloudInfo();
        }

        /// <summary>
        /// 通过指针来构造对象
        /// </summary>
        /// <param name="ptr"></param>
        public VCloud(IntPtr ptr) : base(ptr)
        {
            InitCloudInfo();
        }

        /// <summary>
        /// 初始化点云信息
        /// </summary>
        private void InitCloudInfo()
        {
            LMatrix matrix = new LMatrix();
            matrix.Init();
            Matrix = matrix;

            if (LPtr != IntPtr.Zero)
            {
                PointNum = NativeAPI.GetCloudInfo(LPtr, Indexes, out var num, out _);
                PointNum = num;
            }
        }

        /// <summary>
        /// 读取点云数据
        /// </summary>
        /// <param name="file">ply文件</param>
        /// <returns></returns>
        public static VCloud ReadPly(string plyfile, bool isOnlyPoints, out string errMsg)
        {
            errMsg = string.Empty;
            int result = NativeAPI.ReadPly(plyfile, isOnlyPoints, out var lptr, out var msgPtr);

            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
            }

            return new VCloud(lptr);
        }

        public static VCloud ReadPCD(string pcdfile)
        {
            return new VCloud(IntPtr.Zero);
        }

        public static VCloud ReadFile(string file, bool isOnlyPoints, out string errMsg)
        {
            int result = 0;
            string ext = Path.GetExtension(file);
            IntPtr cloudPtr = IntPtr.Zero;
            IntPtr msgPtr = IntPtr.Zero;
            errMsg = String.Empty;
            switch (ext)
            {
                case ".txt":
                    result = NativeAPI.ReadTxt(file, isOnlyPoints, out cloudPtr, out msgPtr);
                    break;
                case ".ply":
                    result = NativeAPI.ReadPly(file, isOnlyPoints, out cloudPtr, out msgPtr);
                    break;
                default:
                    errMsg = $"Not support format {ext}";
                    break;
            }

            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
                NativeAPI.DeleteArr(msgPtr, NativeAPI.ArrayType.CharArr);
            }

            return new VCloud(cloudPtr);
        }

        /// <summary>
        /// 读取点云数据
        /// </summary>
        /// <param name="txtfile">文本文件</param>
        /// <returns></returns>
        public static VCloud ReadTxt(string txtfile, bool isOnlyPoints, out string errMsg)
        {
            errMsg = string.Empty;
            int result = NativeAPI.ReadTxt(txtfile, isOnlyPoints, out var lptr, out var msgPtr);

            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
                NativeAPI.DeleteArr(msgPtr, NativeAPI.ArrayType.CharArr);
            }

            return new VCloud(lptr);
        }

        /// <summary>
        /// 将激光数据转换为点云数据
        /// </summary>
        /// <param name="laser">激光数据</param>
        /// <returns>点云数据</returns>
        public static VCloud ParseFromLaser(LineLaser laser)
        {
            LSize3 size = new LSize3(laser.OffsetX, laser.OffsetY, laser.OffsetZ);
            NativeAPI.ParseLineLaser(laser.Row, laser.RowDis, laser.Column, laser.ColDis, laser.ZPointer, laser.Resolution, laser.ValidMin, laser.ValidMax, size, laser.Invert, (int)laser.ZDataType, out var cloudPtr);
            return new VCloud(cloudPtr);
        }

        #region Actor对象

        /// <summary>
        /// 添加到渲染界面中
        /// </summary>
        /// <param name="interactor"></param>
        public void AddActor(IInteractor interactor)
        {
            I3DInteractor i3D = interactor as I3DInteractor;
            i3D.AddPtsActor(ID, LPtr, Indexes);
        }


        /// <summary>
        /// 获取点云中心点
        /// </summary>
        /// <returns></returns>
        public VVector GetCenter()
        {
            return NetAPI.GetCenterByCloud(this);
        }

        public VSize GetBoundSize(Guid ownerId, bool vis, BoundBoxType boxType = BoundBoxType.AxisAlign)
        {
            IsShowBoundBox = vis;
            return NetAPI.GetCloudBoundBox(ownerId, this, boxType, out string err, vis);
        }

        #endregion

        /// <summary>
        /// 重新对象是否方法
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && lptr != IntPtr.Zero && !IsReference)
            {
                NativeAPI.DeleteObj(lptr, NativeAPI.ClassType.PCloud);
                lptr = IntPtr.Zero;
                IsDisposed = true;
            }

            if (disposing && Indexes.Length > 0 && !isIndexRef)
            {
                NativeAPI.DeleteArr(Indexes.Ptr, NativeAPI.ArrayType.IntArr);
                Indexes = new IArray();
            }

            if (disposing && Distances.Length > 0)
            {
                NativeAPI.DeleteArr(Distances.Ptr, NativeAPI.ArrayType.DoubleArr);
                Distances.Dispose();
            }
        }

        /// <summary>
        /// 点云数量
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"PointCloud:{PointNum}";
        }

        /// <summary>
        /// ROI cloud
        /// </summary>
        /// <returns></returns>
        public VCloud GetROICloud(IGeometry roi, bool isInside, double surfaceDis, out string errMsg)
        {
            errMsg = string.Empty;

            // 通过ROI来获取
            if (roi == null)
            {
                return new VCloud(this);
            }

            IntPtr[] ptrs = new IntPtr[1] { roi.LPtr };
            GCHandle handle = GCHandle.Alloc(ptrs, GCHandleType.Pinned);
            // 构建
            // 1.通过ROI来获取包含点云的索引信息
            int result = NativeAPI.CloudSegment(LPtr, Indexes, 0, 1, ptrs, !isInside, surfaceDis, out var indexes, out var msgPtr);

            handle.Free();
            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
                return this;
            }

            return new VCloud(this, indexes);
        }

        /// <summary>
        /// 通过ROIs数组构建获取点云索引
        /// </summary>
        /// <param name="rois">ROI集合</param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public VCloud GetCloudByROIs(VROIs vROIs, out string errMsg, ICoord3 coord3s = null)
        {
            errMsg = string.Empty;

            // 如果ROIs为空，则返回原始点云对象
            if (vROIs == null || vROIs.Count == 0)
            {
                return new VCloud(this);
            }
            int count = vROIs.Count;
            IntPtr[] ptrs = new IntPtr[count];
            //获取对应的坐标系,做随模板运动动作
            var rois = vROIs.GetRelativeROIs(coord3s);
            for (int i = 0; i < count; i++)
            {
                ptrs[i] = rois[i].LPtr;
            }

            // 锁住指针数组
            GCHandle handle = GCHandle.Alloc(ptrs, GCHandleType.Pinned);

            var typeName = rois[0].GetType().Name;
            if (!Enum.TryParse<ROIType>(typeName, out var roiType))
            {
                errMsg = $"ROI类型：{typeName} 不支持!";
            }

            // 构建
            // 1.通过ROI来获取包含点云的索引信息
            int result = NativeAPI.CloudSegment(LPtr, Indexes, roiType, count, ptrs, !rois.IsInside, rois.SurfDistance, out var indexes, out var msgPtr);

            // 释放指针数组
            handle.Free();
            if (result > 0)
            {
                errMsg = Marshal.PtrToStringUni(msgPtr);
                return this;
            }

            return new VCloud(this, indexes);
        }

        /// <summary>
        /// 仿射变换
        /// </summary>
        /// <param name="transform"></param>
        public void Transform(ITransform transform, bool invert = false)
        {
            NativeAPI.TransformCloudBySource(LPtr, Indexes, transform.Matrix, invert);
        }

        #region ISave 接口

        public override void Save(string path)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".txt")
            {
                NativeAPI.WritePCloud(LPtr, Indexes, path, ext, false);
            }
            else if (ext == ".ply")
            {
                NativeAPI.WritePCloud(LPtr, Indexes, path, ext, true);
            }
            else
            {
                throw new FriendlyException($"Not Support Extension:{ext}");
            }
        }

        /// <summary>
        /// 对象Clone
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {

            return new VCloud(this);
        }

        public override string[] SaveFormat => new string[] { ".txt", ".ply" };

        #endregion
    }
}