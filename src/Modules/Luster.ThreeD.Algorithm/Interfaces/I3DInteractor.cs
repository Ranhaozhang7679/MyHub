#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IWindow
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Interfaces
* 文 件 名:       IWindow
* 创建时间:       2021/10/30 10:40:19
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      4e824c17-1847-4d66-9f2c-c3b864477ab4
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 10:40:19
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Enums;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Interfaces
{
    public enum CameraView
    {
        Top,
        Bottom,
        Front,
        Back,
        Left,
        Right
    }

    /// <summary>
    /// 窗口渲染
    /// </summary>
    public interface I3DInteractor : IInteractor
    {
        void ScreenHot(string imgPath);

        /// <summary>
        /// 相机居中
        /// </summary>
        void CenterCamrea(string actorID);

        /// <summary>
        /// 设置背景颜色
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        void SetBackground(byte r, byte g, byte b, byte a);

        /// <summary>
        /// 设置相机位置
        /// </summary>
        /// <param name="actorID">actor的ID</param>
        /// <param name="cameraPos">相机位置</param>
        void SetCameraView(CameraView cameraView);

        /// <summary>
        /// 相机视角坐标系变换
        /// </summary>
        /// <param name="in_coord">坐标系</param>
        void CameraAngleTransform(LCoord in_coord);

        /// <summary>
        /// 设置相机位置
        /// </summary>
        /// <param name="actorID">actor的ID</param>
        /// <param name="cameraPos">相机位置</param>
        void RotateCamera(double angle);

        /// <summary>
        /// 设置相机位置
        /// </summary>
        /// <param name="actorID">actor的ID</param>
        /// <param name="cameraPos">相机位置</param>
        void FlipCamera(double angle);

        /// <summary>
        /// 包围盒显示
        /// </summary>
        /// <param name="visible">显示包围盒</param>
        void ShowBoundBox(bool visible);

        /// <summary>
        /// 指针坐标显示
        /// </summary>
        /// <param name="isShow"></param>
        /// <param name="coord"></param>
        void PointerCoord(bool isShow,LCoord coord);

        /// <summary>
        /// 指针坐标方向显示
        /// </summary>
        /// <param name="isShow"></param>
        /// <param name="coord"></param>
        void PointerNormal(bool isShow, LCoord coord);


        /// <summary>
        /// 坐标系
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="coord"></param>
        void AddAxesActor(Guid actorID, LCoord coord);

        /// <summary>
        /// 显示点云数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="points"></param>
        void AddPtsActor(Guid id, IntPtr cloudPtr, IArray indexes);

        /// <summary>
        /// 添加cad对象
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="cadPtr">文件路径</param>
        void AddCadActor(Guid id, string cadPath);

        /// <summary>
        /// 显示点云数据
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="triangles">文件路径</param>
        void AddStlActor(Guid id, IntPtr meshPtr);

        /// <summary>
        /// 添加Point3
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="point3">点</param>
        void AddPointActor(Guid id, LPoint3 point3);

        /// <summary>
        /// 添加Line
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="linePtr">线指针</param>
        void AddLineActor(Guid id, IntPtr linePtr);

        /// <summary>
        /// 添加Plane
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="linePtr">线指针</param>
        void AddPlaneActor(Guid id, VPlane vPlane);

        /// <summary>
        /// 添加Circel
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="linePtr">线指针</param>
        void AddCircleActor(Guid id, IntPtr circle);

        /// <summary>
        /// 添加向量显示
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="point">起始点</param>
        /// <param name="dir">方向</param>
        /// <param name="visible"></param>
        void AddVectorActor(Guid id, VVector point, VDirection dir, bool visible = true);

        /// <summary>
        /// 添加圆柱
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="cylinderPtr">圆柱指针</param>s
        void AddCylinderActor(Guid id, IntPtr cylinderPtr);

        /// <summary>
        /// 添加立方体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cuboidPtr"></param>
        void AddCuboidActor(Guid id, IntPtr cuboidPtr);

        /// <summary>
        /// 添加球体
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="lPtr"></param>
        void AddSphereActor(Guid iD, IntPtr lPtr);

        /// <summary>
        /// 添加圆锥
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="conePtr">圆锥指针</param>
        void AddConeActor(Guid id, IntPtr conePtr);

        /// <summary>
        /// 显示点云数据
        /// </summary>
        /// <param name="id">唯一标识</param>
        /// <param name="triangles">文件路径</param>
        void AddScalarBar(Guid id, Guid cloudID, DArray dArray, System.Drawing.Color[] colors, int numOfLabels, double minTolerance, double maxTolerance);

        /// <summary>
        /// 求取点云Z向最大值和最小值
        /// </summary>
        /// <param name="in_pointCloud"></param>
        /// <param name="indexes"></param>
        /// <param name="zDirValues"></param>
        /// <param name="minZDirValue"></param>
        /// <param name="maxZDirValue"></param>
        /// <returns></returns>
        void GetPointCloudZDirValue(VCloud cloud, out double minZDirValue, out double maxZDirValue);

        /// <summary>
        /// 清除色谱显示
        /// </summary>
        /// <param name="actorID"></param>
        void CloseScalarBar(string actorID);

        /// <summary>
        /// 添加文字说明
        /// </summary>
        /// <param name="id"></param>
        /// <param name="text"></param>
        void AddTextActor(Guid id, string text);

        /// <summary>
        /// 添加Caption
        /// </summary>
        /// <param name="actorId">活动</param>
        /// <param name="attach"></param>
        void OnAddCaption(Guid actorId, bool attach = true);

        /// <summary>
        /// 添加属性信息
        /// </summary>
        /// <param name="geometryType">几何类型 点云|点|线段|透明度</param>
        /// <param name="cloudSize">点云点尺寸</param>
        /// <param name="pSize">点的尺寸</param>
        /// <param name="lineWidth">线宽度</param>
        /// <param name="lineScale">线延长</param>
        /// <param name="opacity">透明度</param>
        void SetGeometryProp(int geometryType = 0, double cloudSize = 0.5, double pSize = 1.0, double lineWidth = 5.0, double lineScale = 1.0, double opacity = 0.5);

        /// <summary>
        /// 几何模型显示模式（顶点、线框、面）
        /// </summary>
        /// <param name="meshID"></param>
        /// <param name="type"></param>
        void ModelShowMode(string meshID, ShowMode type);

        #region ROI 交互

        /// <summary>
        /// ROI新建
        /// </summary>
        /// <param name="geomType"></param>
        /// <param name="cloudID">只有存在点云的情况下，才能进行创建，因为没有点云可能会产生无效点</param>
        /// <param name="callBack"></param>
        void StartInteraction(ROIType geomType, VCloud cloud, IntPtr callBack,bool multiROI=false);

        /// <summary>
        /// ROI 编辑
        /// </summary>
        /// <param name="geomType"></param>
        /// <param name="geometryID"></param>
        /// <param name="callBack"></param>
        void EditInteraction(ROIType geomType, string geometryID, IntPtr geoPtr, IntPtr callBack, bool multiROI = false);

        /// <summary>
        /// 多 ROI 编辑
        /// </summary>
        /// <param name="geomType"></param>
        /// <param name="elementID"></param>
        /// <param name="roiNum"></param>
        /// <param name="shapes"></param>
        /// <param name="callBack"></param>
        /// <param name="multiCallBack"></param>
        void MultiROIEditInteraction(VCloud cloud, ROIType geomType, int elementID, int roiNum, IntPtr[] shapes, IntPtr callBack, IntPtr multiCallBack);

        /// <summary>
        /// 结束交互
        /// </summary>
        void EndInteraction();

        #endregion

        #region 截面ROI交互

        void EndSectionTool();

        void GetToolParams(out int planeNum, out LVector normal, out double rectWidth, out double rectHeight, out double radius, out LPoint3 startPoint, out LPoint3 endPoint, out double arcSpan);

        /// <summary>
        /// 截面ROI交互
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="secPath"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="refCir"></param>
        /// <param name="arcSpan"></param>
        /// <param name="offset"></param>
        /// <param name="arrayMode"></param>
        /// <param name="genCirArcWay"></param>
        /// <param name="arrNum"></param>
        /// <param name="arrInterval"></param>
        /// <param name="sectionMode"></param>
        /// <param name="arrDir"></param>
        /// <param name="rectWidth"></param>
        /// <param name="rectHeight"></param>
        /// <param name="radius"></param>
        /// <param name="disThresh"></param>
        /// <param name="enable"></param>
        void SetCloudSectionROI(IsProjectType isProjectType,VCloud cloud, SecPathType secPath, VVector startPoint, VVector endPoint, VCircle refCir, double arcSpan, double offset, ArrayMode arrayMode, GenCirArcWay genCirArcWay, int arrNum, double arrInterval, SectionMode sectionMode, VDirection arrDir, double rectWidth, double rectHeight, double radius, double disThresh, bool enable);

        #endregion

        #region 标注

        /// <summary>
        /// 两点距离
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void PointToPointDistanceDimension(Guid id, VVector point1, VVector point2, string text, LTolerance tolerance);

        /// <summary>
        /// 两点距离
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void PointToLineDistanceDimension(Guid id, VVector point1, VLine line, string text, LTolerance tolerance);

        /// <summary>
        /// 点到面距离
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void PointToPlaneDistanceDimension(Guid id, VVector point1, VPlane plane, string text, LTolerance tolerance);

        /// <summary>
        /// 线到线距离
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void LineToLineDistanceDimension(Guid id, VLine line1, VLine line2, string text, LTolerance tolerance);

        /// <summary>
        /// 线到面距离
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void LineToPlaneDistanceDimension(Guid id, VLine line, VPlane plane, string text, LTolerance tolerance);

        /// <summary>
        /// 线到线角度
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void LineToLineAngleDimension(Guid id, VLine line1, VLine line2, string text, LTolerance tolerance);

        /// <summary>
        /// 线到面角度
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void LineToPlaneAngleDimension(Guid id, VLine line, VPlane plane, string text, LTolerance tolerance);

        /// <summary>
        /// 面到面角度
        /// </summary>
        /// <param name="id"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="text"></param>
        void PlaneToPlaneAngleDimension(Guid id, VPlane plane1, VPlane plane2, string text, LTolerance tolerance);

        #endregion

        #region 添加标签

        void PickLabelWidgets(string cloudID, CompareType type, DArray datas, int viewPort, IntPtr pickCallBack, IntPtr finishCallBack);

        void DeleteLabelWidgets( IntPtr finishCallBack);

        void EndLabelWidgets();

        void HideLabelWidgets(IntPtr finishCallBack);

        void DeleteAllLabelWidgets(string cloudID, CompareType type);

        void SetLabelWidgetsVisible(string cloudID, CompareType type, bool visible);

        #endregion

        #region 锚定选择

        /// <summary>
        /// 开始锚定
        /// </summary>
        /// <param name="geomType"></param>
        /// <param name="cloudID">只有存在点云的情况下，才能进行创建，因为没有点云可能会产生无效点</param>
        /// <param name="callBack"></param>
        void StartAnchor(Guid cloudID, Guid meshID, IntPtr callBack);

        /// <summary>
        /// 结束锚定
        /// </summary>
        void EndAnchor();

        #endregion

        #region 开始标定
        /// <summary>
        /// 开始标定
        /// </summary>
        /// <param name="meshID">网格ID</param>
        void StartCalib(string meshID);

        /// <summary>
        /// 设置要进行标定的点云对象
        /// </summary>
        /// <param name="cloudID"></param>
        void SetCalibCloud(string cloudID);

        /// <summary>
        /// 完成对某个点云的标定
        /// </summary>
        /// <param name="cloudID">点云ID</param>
        void CompleteCalib(string cloudID);

        /// <summary>
        /// 结束锚定状态
        /// </summary>
        void EndCalib();
        #endregion

        /// <summary>
        /// 移除ElementID，原生的ID
        /// </summary>
        /// <param name="elementID"></param>
        void RemoveElementID(int elementID);

        #region 动画效果
        /// <summary>
        /// 卡尺取点动画
        /// callback(LPoint3,status) status 0是运行中 1是完成
        /// </summary>
        void StartCaliperAnimation(VCloud in_pointCloud, IGeometry in_caliper, VDirection dir, double searchDepth, double dClearence, long intervalTime,VSize cuboSize, IntPtr callback);

        /// <summary>
        /// 将某个对象设置为选择中心
        /// </summary>
        void StartCMMAnimation(VCloud in_pointCloud, VVector refPoint, int fitType, VDirection dir, double searchDepth, double searchRadius, long intervalTime, VSize cuboSize, IntPtr callback);

        /// <summary>
        /// 将某个对象设置为选择中心
        /// </summary>
        void EndAnimation();
        #endregion
    }
}