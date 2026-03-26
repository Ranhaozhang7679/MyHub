using Luster.ThreeD.Algorithm.Interfaces;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Runtime.InteropServices;

namespace Luster.ThreeD.Algorithm
{
    public class GuiNativeAPI
    {
        public const string DllPath = "Algorithm\\Luster3DAlgorithm.dll";

        /// <summary>
        /// 创建window
        /// </summary>
        /// <param name="winPTR">窗口句柄</param>
        /// <param name="width">实际宽</param>
        /// <param name="height">实际高</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void CreateWin(IntPtr winPTR, int width, int height);

        /// <summary>
        /// 调整windows 尺寸
        /// </summary>
        /// <param name="width">窗口宽</param>
        /// <param name="height">窗口高</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ResizeWin(int width, int height);

        /// <summary>
        /// 设置显示标签
        /// </summary>
        /// <param name="v">所属模块GUID</param>
        /// <param name="text">显示对象内容</param>
        /// <param name="visible">是否显示</param>
        /// <param name="measure">测量对象值</param>
        /// <param name="lang">多语言种类</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
            SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetTextVisible([MarshalAs(UnmanagedType.LPStr)] string v, [MarshalAs(UnmanagedType.LPWStr)] string text, bool visible, IntPtr measure, [MarshalAs(UnmanagedType.LPStr)] string lang);

        /// <summary>
        /// 读取点云
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCoord(string actorID, LCoord coord);

        /// <summary>
        /// 读取点云
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCloud(string actorID, IntPtr cloudPtr, IArray indexes);

        /// <summary>
        /// 读取点云
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddMesh(string actorID, IntPtr meshPtr);

        /// <summary>
        /// 添加线集合
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddPoint(string actorID, LPoint3 point3);

        /// <summary>
        /// 添加法向量
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="pt"></param>
        /// <param name="v"></param>
        /// <param name="visible"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddNormalVector(string actorID, LPoint3 pt, LVector v, bool visible);

        /// <summary>
        /// 添加线集合
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddLine(string actorID, IntPtr linePtr);

        /// <summary>
        /// 添加面集合
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddPlane(string actorID, LPoint3 center, LVector vector, double width = 1, double height = 1);

        /// <summary>
        /// 添加圆集合
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCircle(string actorID, IntPtr circlePtr);

        /// <summary>
        /// 添加圆集合
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCylinder(string actorID, IntPtr cylinderPtr);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetGeometryProp(int geometryType, double cloudSize, double pSize, double lineWidth, double lineScale, double opacity);

        /// <summary>
        /// 模型显示模式  type  0: 顶点模式  1：线框模式  2：面模式
        /// </summary>
        /// <param name="meshID"></param>
        /// <param name="type"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ModelShowMode(string meshID, int type);

        /// <summary>
        /// 添加立方体
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCuboid(string actorID, IntPtr cuboidPtr);

        /// <summary>
        /// 添加文字说明
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="text"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddText(string actorID, string text);

        /// <summary>
        /// CAD文件
        /// </summary>
        /// <param name="v"></param>
        /// <param name="cadPath"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
               SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCad(string v, string cadPath);

        /// <summary>
        /// 读取点云
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="filename"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ReadCloudByFile(string actorID, string filename);

        /// <summary>
        /// 显示隐藏
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="isVisible"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetVisible(string actorID, bool isVisible);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void CenterCamera(string actorID);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetBoundBox(bool visible);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void PointerCoord(bool isShow, LCoord coord);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
             SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void PointerNormal(bool isShow, LCoord coord);

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="isVisible"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetColor(string actorID, byte red, byte green, byte blue, byte alpha);

        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="isVisible"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void HighLight(string actorID);

        /// <summary>
        /// 背景色
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetBackground(byte red, byte green, byte blue, byte alpha);

        /*
           添加世界坐标系
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ShowWorldAxes(bool isVisible);

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="matrix"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetActorMatrix(string actorID, ref double matrix);

        /*
           添加球体
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddSphere(string actorID, IntPtr cuboidPtr);

        /*
           添加圆锥
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCone(string actorID, IntPtr conePtr);

        /// <summary>
        /// 显示隐藏
        /// </summary>
        /// <param name="actorID"></param>
        /// <param name="isVisible"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void Render(bool resetCamera = true);

        /*
           对象渲染
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void RemoveAll();

        /*
            移除Actor
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
              SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void RemoveActor(string actorID);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetPointCloudZDirValue(IntPtr in_pointCloud, IArray indexes, out DArray zDirValues, out double minZDirValue, out double maxZDirValue);

        /*
            鼠标点击事件
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void MouseDownEvent(int x, int y, int ctrl, int shift, char keycode, int repeatcount, string mouseButton);

        /*
            设置相机视角
         */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetCameraView(CameraView cameraView);

        /// <summary>
        /// 相机视角坐标系变换
        /// </summary>
        /// <param name="cameraView"></param>

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void CameraAngleTransform(LCoord in_coord);

        /// <summary>
        /// 相机旋转
        /// </summary>
        /// <param name="angle"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void RotateCamera(double angle);

        /// <summary>
        /// 相机翻转
        /// </summary>
        /// <param name="angle"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void FlipCamera(double angle);

        /*
            设置相机视角
         */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void DisposeWin();

        /*
          设置Caption
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddCaptionEvent(string actorID, bool attach);

        #region 添加其他元素

        /*
            添加ScalrBar
        */
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddScalarBar(string actorID, string cloudID, DArray dResults, IArray colorArr, int numOfLabels, double minTolerance, double maxTolerance);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void CloseScalarBar(string actorID);

        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="viewPort"></param>
        /// <param name="pickCallBack"></param>
        /// <param name="finishCallBack"></param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void PickLabelWidgets(string cloudID, int type, DArray datas, int viewPort, IntPtr pickCallBack, IntPtr finishCallBack);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void DeleteLabelWidgets(IntPtr finishCallBack);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void EndLabelWidgets();


        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void HideLabelWidgets(IntPtr finishCallBack);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void DeleteAllLabelWidgets(string cloudID, int type);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetLabelWidgetsVisible(string cloudID, int type, bool visible);

        #endregion

        #region ROI 交互功能

        /*
            开始交互
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartInteraction(int geomType, IntPtr cloud, IntPtr callBack, bool multiROI = false);

        /*
            编辑交互
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void EditInteraction(int geomType, string geomPtr, IntPtr geoPtr, IntPtr callBack, bool multiROI = false);


        /*
            多ROI编辑交互
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void MultiROIEditInteraction(IntPtr cloud, int geomType, int elementID, int roiNum, IntPtr[] shapes, IntPtr callBack, IntPtr multiCallBack);

        /*
            结束交互
        */

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void EndInteraction();

        #endregion

        #region 截面交互工具

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void GetToolParams(out int planeNum, out LVector normal, out double rectWidth, out double rectHeight, out double radius, out LPoint3 startPoint, out LPoint3 endPoint, out double arcSpan);

        /// <summary>
        /// 获取线段型截面参数
        /// </summary>
        /// <param name="pointCloud">点云</param>
        /// <param name="indexes">点云下标</param>
        /// <param name="startPoint">线段起点</param>
        /// <param name="endPoint">线段终点</param>
        /// <param name="posDir">方向（正逆时针）</param>
        /// <param name="offset">从起点的偏移量</param>
        /// <param name="numOrSpace">个数阵列或者间距阵列</param>
        /// <param name="sectionNum">阵列个数</param>
        /// <param name="space">阵列间隔</param>
        /// <param name="sectionMode">截面模式（无界平面、长方体、三维球）</param>
        /// <param name="normal">阵列元素法向</param>
        /// <param name="rectWidth">矩形宽</param>
        /// <param name="rectHeight">矩形高</param>
        /// <param name="radius">圆半径</param>
        /// <param name="disThresh">距离阈值</param>
        /// <param name="enableTool">是否使用截面交互工具</param>
        /// <param name="outIndexes">结果</param>
        /// <param name="errMsg">错误消息</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void GetLineSectionROI(IntPtr pointCloud, IArray indexes, LPoint3 startPoint, LPoint3 endPoint, bool posDir, double offset, bool numOrSpace, int sectionNum, double space, int sectionMode, LVector normal, double rectWidth, double rectHeight, double radius, double disThresh,bool enableTool, out IArray outIndexes, out IntPtr errMsg);

        /// <summary>
        /// 获取圆弧型截面参数
        /// </summary>
        /// <param name="pointCloud">点云</param>
        /// <param name="indexes">点云下标</param>
        /// <param name="startPoint">圆弧起点</param>
        /// <param name="endPoint">圆弧终点</param>
        /// <param name="refCircle">相关圆弧</param>
        /// <param name="arcSpan">圆弧跨度</param>
        /// <param name="arcType">圆弧构造类型（起点和终点、起点和跨度值）</param>
        /// <param name="posDir">起始方向（true 从起点开始阵列、从终点开始阵列）</param>
        /// <param name="offset">偏移量</param>
        /// <param name="numOrSpace">个数阵列或者间距阵列</param>
        /// <param name="sectionNum">阵列个数</param>
        /// <param name="space">阵列间隔</param>
        /// <param name="sectionMode">截面模式（无界平面、长方体、三维球）</param>
        /// <param name="normal">阵列元素法向</param>
        /// <param name="rectWidth">矩形宽</param>
        /// <param name="rectHeight">矩形高</param>
        /// <param name="radius">圆半径</param>
        /// <param name="disThresh">距离阈值</param>
        /// <param name="enableTool">是否使用截面交互工具</param>
        /// <param name="outIndexes">结果</param>
        /// <param name="errMsg">错误消息</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void GetArcSectionROI(IntPtr pointCloud, IArray indexes, LPoint3 startPoint, LPoint3 endPoint, IntPtr refCircle, double arcSpan, bool arcType, bool posDir, double offset, bool numOrSpace, int sectionNum, double space, int sectionMode, LVector normal, double rectWidth, double rectHeight, double radius, double disThresh, bool enableTool, out IArray outIndexes, out IntPtr errMsg);

        /// <summary>
        /// 获取圆型截面参数
        /// </summary>
        /// <param name="pointCloud">点云</param>
        /// <param name="indexes">点云下标</param>
        /// <param name="startPoint">起点</param>
        /// <param name="refCircle">相关圆弧</param>
        /// <param name="posDir">起始方向（true 从起点开始阵列、从终点开始阵列）</param>
        /// <param name="offset">偏移量</param>
        /// <param name="numOrSpace">个数阵列或者间距阵列</param>
        /// <param name="sectionNum">阵列个数</param>
        /// <param name="space">阵列间隔</param>
        /// <param name="sectionMode">截面模式（无界平面、长方体、三维球）</param>
        /// <param name="normal">阵列元素法向</param>
        /// <param name="rectWidth">矩形宽</param>
        /// <param name="rectHeight">矩形高</param>
        /// <param name="radius">圆半径</param>
        /// <param name="disThresh">距离阈值</param>
        /// <param name="enableTool">是否使用截面交互工具</param>
        /// <param name="outIndexes">结果</param>
        /// <param name="errMsg">错误消息</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void GetCircleSectionROI(IntPtr pointCloud, IArray indexes, LPoint3 startPoint, IntPtr refCircle, bool posDir, double offset, bool numOrSpace, int sectionNum, double space, int sectionMode, LVector normal, double rectWidth, double rectHeight, double radius, double disThresh, bool enableTool, out IArray outIndexes, out IntPtr errMsg);

        /// <summary>
        /// 结束截面ROI交互
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void EndSectionTool();

        /// <summary>
        /// 开启线段多截面工具
        /// </summary>
        /// <param name="pointCloud">点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与截面操作</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">结束点</param>
        /// <param name="posDir">阵列方向/ture：从起始点——终点构造多平面，false从终点——起始点构造多平面</param>
        /// <param name="offset">起始偏移距离(参考线段)，范围为[0，10000]</param>
        /// <param name="numOrSpace"> 阵列模式/true:使用个数构造多平面，按指定数量均分</param>
        /// <param name="sectionNum">平面个数，范围[2，100000]</param>
        /// <param name="space">间隔距离，范围为（0，10000]，两平面之间距离；间隔角度,范围为（0,360)，两平面之间间隔角度</param>
        /// <param name="sectionMode">阵列元素类型/截面模式  0-无界平面  1-三维矩形  2-三维圆形</param>
        /// <param name="normal">阵列元素法向，当截面路径模式为线段模式时有效；非线段模式时平面的法向量是平面所在点与圆/圆弧的切线方向</param>
        /// <param name="rectWidth">三维矩形截面的宽度</param>
        /// <param name="rectHeight">三维矩形截面的高度</param>
        /// <param name="radius">三维圆形截面的半径</param>
        /// <param name="disThresh">点到平面的距离阈值，范围为[0，10000]</param>
        /// <param name="enableTool">是否使用截面交互工具</param>
        /// <param name="pSectionCloud">所求截面点云</param>
        /// <param name="errMsg">错误消息</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartLineSectionTool(IntPtr pointCloud, IArray indexes, LPoint3 startPoint, LPoint3 endPoint, bool posDir, double offset, bool numOrSpace, int sectionNum, double space, int sectionMode, LVector normal, double rectWidth, double rectHeight, double radius, double disThresh, bool enableTool, out IntPtr pSectionCloud, out IntPtr errMsg);


        /// <summary>
        /// 开启圆多截面工具
        /// </summary>
        /// <param name="pointCloud">点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与截面操作</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="refCircle">路径圆</param>
        /// <param name="posDir">阵列方向/ture：从起始点——终点构造多平面，false从终点——起始点构造多平面</param>
        /// <param name="offset">起始偏移距离(参考线段)，范围为[0，10000]</param>
        /// <param name="numOrSpace"> 阵列模式/true:使用个数构造多平面，按指定数量均分</param>
        /// <param name="sectionNum">平面个数，范围[2，100000]</param>
        /// <param name="space">间隔距离，范围为（0，10000]，两平面之间距离；间隔角度,范围为（0,360)，两平面之间间隔角度</param>
        /// <param name="sectionMode">阵列元素类型/截面模式  0-无界平面  1-三维矩形  2-三维圆形</param>
        /// <param name="normal">阵列元素法向，当截面路径模式为线段模式时有效；非线段模式时平面的法向量是平面所在点与圆/圆弧的切线方向</param>
        /// <param name="rectWidth">三维矩形截面的宽度</param>
        /// <param name="rectHeight">三维矩形截面的高度</param>
        /// <param name="radius">三维圆形截面的半径</param>
        /// <param name="disThresh">点到平面的距离阈值，范围为[0，10000]</param>
        /// <param name="enableTool">是否使用截面交互工具</param>
        /// <param name="pSectionCloud">所求截面点云</param>
        /// <param name="errMsg">错误消息</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
       SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartCircleSectionTool(IntPtr pointCloud, IArray indexes, LPoint3 startPoint, IntPtr refCircle, bool posDir, double offset, bool numOrSpace, int sectionNum, double space, int sectionMode, LVector normal, double rectWidth, double rectHeight, double radius, double disThresh, bool enableTool, out IntPtr pSectionCloud, out IntPtr errMsg);

        /// <summary>
        /// 开启圆弧多截面工具
        /// </summary>
        /// <param name="pointCloud">点云数据</param>
        /// <param name="indexes">数据索引。indexes为空时，表示所有点云数据参与截面操作</param>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">结束点</param>
        /// <param name="refCircle">路径圆</param>
        /// <param name="posDir">阵列方向/ture：从起始点——终点构造多平面，false从终点——起始点构造多平面</param>
        /// <param name="offset">起始偏移距离(参考线段)，范围为[0，10000]</param>
        /// <param name="numOrSpace"> 阵列模式/true:使用个数构造多平面，按指定数量均分</param>
        /// <param name="sectionNum">平面个数，范围[2，100000]</param>
        /// <param name="space">间隔距离，范围为（0，10000]，两平面之间距离；间隔角度,范围为（0,360)，两平面之间间隔角度</param>
        /// <param name="sectionMode">阵列元素类型/截面模式  0-无界平面  1-三维矩形  2-三维圆形</param>
        /// <param name="normal">阵列元素法向，当截面路径模式为线段模式时有效；非线段模式时平面的法向量是平面所在点与圆/圆弧的切线方向</param>
        /// <param name="rectWidth">三维矩形截面的宽度</param>
        /// <param name="rectHeight">三维矩形截面的高度</param>
        /// <param name="radius">三维圆形截面的半径</param>
        /// <param name="disThresh">点到平面的距离阈值，范围为[0，10000]</param>
        /// <param name="enableTool">是否使用截面交互工具</param>
        /// <param name="pSectionCloud">所求截面点云</param>
        /// <param name="errMsg">错误消息</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartArcSectionTool(IntPtr pointCloud, IArray indexes, LPoint3 startPoint, LPoint3 endPoint, IntPtr refCircle, double arcSpan, bool arcType, bool posDir, double offset, bool numOrSpace, int sectionNum, double space, int sectionMode, LVector normal, double rectWidth, double rectHeight, double radius, double disThresh, bool enableTool, out IntPtr pSectionCloud, out IntPtr errMsg);

        #endregion

        #region 标注

        /// <summary>
        /// 两点距离
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void PointToPointDistanceDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, LPoint3 point1, LPoint3 point2, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        /// <summary>
        /// 两点距离
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void PointToLineDistanceDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, LPoint3 point1, IntPtr line, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        /// <summary>
        /// 点到面距离
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void PointToPlaneDistanceDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, LPoint3 point1, IntPtr plane, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        /// <summary>
        /// 线到线距离
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void LineToLineDistanceDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, IntPtr line1, IntPtr line2, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        /// <summary>
        /// 线到面距离
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void LineToPlaneDistanceDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, IntPtr line, IntPtr plane, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        /// <summary>
        /// 线到线角度
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void LineToLineAngleDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, IntPtr line1, IntPtr line2, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        /// <summary>
        /// 线到面角度
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void LineToPlaneAngleDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, IntPtr line, IntPtr plane, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        /// <summary>
        /// 面到面角度
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void PlaneToPlaneAngleDimension([MarshalAs(UnmanagedType.LPStr)] string actorID, IntPtr plane1, IntPtr plane2, [MarshalAs(UnmanagedType.LPWStr)] string text, LMeasure measureData);

        #endregion

        #region 锚定

        /// <summary>
        /// GeometryType Obj_Point
        /// </summary>
        /// <param name="pCloud">点云</param>
        /// <param name="pMesh">网格</param>
        /// <param name="callBack">回调</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartAnchor(string pCloud, string pMesh, IntPtr callBack);

        /// <summary>
        /// 结束锚定
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void EndAnchor();

        #endregion

        #region 标定
        /// <summary>
        /// GeometryType Obj_Point | Obj_Line |Obj_Plane | Obj_Circle Obj_PointSelect
        /// </summary>
        /// <param name="geomType">几何类型</param>
        /// <param name="pCloud">点云</param>
        /// <param name="pMesh">网格</param>
        /// <param name="callBack">回调</param>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartCalib(string meshID);

        /// <summary>
        /// 结束锚定
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void SetCalibCloud(string pCloudID);



        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void CompleteCalib(string cloudID);

        /// <summary>
        /// 结束锚定
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void EndCalib();


        /// <summary>
        /// 删除过程对象
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void RemoveElementID(int id);

        #endregion

        /// <summary>
        /// 拍照
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void ExecuteScreenShot([MarshalAs(UnmanagedType.LPWStr)] string savePath);

        /// <summary>
        /// 将某个对象设置为选择中心
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void AddRotateCenter(string actorID, int viewPort = 0);

        #region CMM和卡尺取点动画
        /// <summary>
        /// 卡尺取点动画
        /// callback(LPoint3,status) status 0是运行中 1是完成
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartCaliperAnimation(IntPtr in_pointCloud, IArray indexes, IntPtr in_caliper, int caliperType, LVector vecDirect, double searchDepth, double dClearence, long intervalTime, LSize3 size, IntPtr callback);

        /// <summary>
        /// 将某个对象设置为选择中心
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void StartCMMAnimation(IntPtr in_pointCloud, IArray indexes, LPoint3 in_referPoint, int fitType, LVector vecDirect, double searchDepth, double searchRadius, long intervalTime, LSize3 size, IntPtr callback);

        /// <summary>
        /// 将某个对象设置为选择中心
        /// </summary>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern void EndAnimation();
        #endregion
    }
}
