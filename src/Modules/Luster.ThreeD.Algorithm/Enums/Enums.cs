#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlignEnums
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Enums
* 文 件 名:       AlignEnums.cs
* 创建时间:       2021/11/24 8:54:34
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      0eb28688-63a5-4293-8ed6-32074c08b06d
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/24 8:54:34
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Enums
{

    /// <summary>
    /// 计算点的类型
    /// </summary>
    public enum PointType
    {
        [Description("均值点")]
        Mean = 6,

        [Description("中值点")]
        Median,

        [Description("最大点")]
        Max,

        [Description("最小点")]
        Min,

        [Description("投影均值点")]
        ProjectionMean,
    }

    public enum PointROI
    {
        [Description("均值点")]
        Mean = 1,

        [Description("中值点")]
        Median,

        [Description("最大点")]
        Max,

        [Description("最小点")]
        Min,
        [Description("投影均值点")]
        ProjectionMean,
    }

    /// <summary>
    /// 形状复杂度类型
    /// </summary>
    public enum ShapeComplexityType
    {
        [Description("最简单")]
        eSimplest = 0,      // 最简单（默认值）

        [Description("简单")]
        eSimple = 1,        // 简单

        [Description("一般")]
        eCommon = 2,        // 一般

        [Description("复杂")]
        eComplex = 3,       // 复杂

        [Description("最复杂")]
        eMostComplex = 4,   // 最复杂
    };

    /// <summary>
    /// 搜索时间类型
    /// </summary>
    public enum SearchTimeType
    {
        [Description("超短")]
        eShortest = 0,  // 超短（默认值）
        [Description("短")]
        eShort = 1,     // 短
        [Description("普通")]
        eOrdinary = 2,  // 普通
        [Description("长")]
        eLong = 3,      // 长
        [Description("超长")]
        eLongest = 4,   // 超长
    };

    /// <summary>
    /// RPS 
    /// </summary>
    public enum EffectiveDirectionType
    {
        [Description("XYZ")]
        eXYZ = 0,   // XYZ（默认值）
        [Description("XY")]
        eXY,        // XY
        [Description("XZ")]
        eXZ,        // XZ
        [Description("YZ")]
        eYZ,        // YZ
        [Description("X")]
        eX,         // X
        [Description("Y")]
        eY,         // Y
        [Description("Z")]
        eZ,         // Z
    };

    /// <summary>
    /// 对齐约束类型
    /// </summary>
    public enum LockType
    {
        [Description("无约束")]
        eLockTypeNOT = 0,        // 无约束

        [Description("坐标系约束")]
        eLockTypeCoordinate = 1, // 坐标系约束

        // 以下约束类型尚未实现
        //eLockType_Line = 2,       // 线约束
        //eLockType_Point = 3,      // 点约束
        //eLockType_Plane = 4,      // 面约束
    };

    public enum DirectionLock
    {
        [Description("无约束")]
        None,

        [Description("X+")]
        X1,

        [Description("X-")]
        X2,

        [Description("Y+")]
        Y1,

        [Description("Y-")]
        Y2,

        [Description("Z+")]
        Z1,

        [Description("Z-")]
        Z2,
    }

    /// <summary>
    /// 坐标轴方向，支持位运算
    /// </summary>
    [Flags]
    public enum CoordDirection
    {
        [Description("X轴")]
        X = 1,

        [Description("Y轴")]
        Y = 2,

        [Description("Z轴")]
        Z = 4,
    }

    /// <summary>
    /// 对齐约束，包含了坐标系约束、点约束、线约束以及面约束
    /// </summary>
    public enum AlignmentLock
    {
        [Description("坐标系约束")]
        LockCoordinate,

        [Description("点约束")]
        LockPoint,

        [Description("线约束")]
        LockLine,

        [Description("面约束")]
        LockPlane
    };

    /// <summary>
    /// 最佳拟合对齐种类
    /// </summary>
    public enum BestFitAlignType
    {
        /// <summary>
        /// 快速对齐:该模式下鲁棒性一般、速度稍快;
        /// </summary>
        [Description("快速对齐")]
        Fast = 0,
        /// <summary>
        /// 鲁棒性对齐:鲁棒性强，适用于干扰较多场景、速度较慢
        /// </summary>
        [Description("鲁棒性对齐")]
        Robust = 1,
    };

    /// <summary>
    /// 比较的距离类型
    /// </summary>
    public enum ComapreDistanceType
    {
        /// <summary>
        /// 最短距离（默认值，即计算点到面的垂直距离）
        /// </summary>
        [Description("最短距离")]
        eShortest = 0,

        /// <summary>
        /// 几何距离，即计算点到面片中心距离
        /// </summary>
        [Description("几何距离")]
        eGeometric = 1,

        /// <summary>
        /// 沿法线距离，即计算点沿法线到面片的距离
        /// </summary>
        [Description("沿法线距离")]
        eAlongNormal = 2,
    }

    public enum FilterMode
    {
        [Description("统计学")]
        eSOR,       // 统计学

        [Description("邻域点数滤波")]
        eNeighsNum, // 邻域点数滤波
    };

    public enum SampleMethod
    {
        [Description("随机采样")]
        eRandom = 0, // 随机采样

        [Description("固定索引步长")]
        eFixedStep,  // 固定索引步长，此方法不需要设置采样比例  

        [Description("空间点距采样")]
        eSpace,      // 空间点距采样，此方法不需要设置采样比例，需设置 KDTree
        //eCurvity,    // 曲率采样，暂不支持
    };

    /// <summary>
    /// 近邻点搜索方式
    /// </summary>
    public enum SearchNeighMethod
    {
        [Description("K近邻")]
        eKnnSearch = 0,

        //[Description("绝对搜索半径")]
        //eAbsRadiusSearch,

        //[Description("相对搜索半径")]
        //eRelativeRadiusSearch,
    };

    /// <summary>
    /// 近邻点搜索方式
    /// </summary>
    public enum UpSampleMethod
    {
        [Description("不上采样")]
        eNoUpSample = 0,       // 不进行上采样

        [Description("局部平面")]
        eSampleLocalPlane,     // 局部平面上采样，根据‘采样半径’和‘采样步长’参数，在输入点的局部平面进行上采样

        [Description("均匀局部")]
        eRandomUniformDensity, // 均匀局部平面上采样，按照均匀随机分布规则在局部平面上进行采样，点云密度由‘半径内点数’参数给定，仅在搜索方式为半径搜索时才可生效

        [Description("体素扩增")]
        eVoxelGridDilation,    // 体素网格扩增上采样，体素网格将按照扩增迭代次数进行扩增，然后将体素点垂直投影至MLS曲面
    };


    public enum ProjectionReviseMethod
    {
        [Description("非修正投影")]
        eNoRevise = 0, // 非修正投影，输出以SVD分解结果为法线的局部平面上的投影点

        [Description("简单投影")]
        eSimple,       // 简单投影，目标点沿法线投影至曲面，从而获取投影点

        [Description("垂直投影")]
        eOrthogonal,   // 垂直投影，在曲面上找到距离目标点最近的投影点
    };
    #region 三维直线拟合

    /// <summary>
    /// 拟合方法
    /// </summary>
    public enum LineFitMethod
    {
        [Description("最小二乘")]
        eLeastSquare = 0, // 

        [Description("带局外点")]
        eAdaption = 1,    // 
    };


    /// <summary>
    /// 投影类型
    /// </summary>
    public enum PlaneProjectionType
    {
        [Description("拟合空间直线")]
        eNotProject = 0,    // 不指定投影平面，拟合空间直线     

        [Description("拟合平面直线")]
        eProject = 1,       // 指定投影平面，拟合平面直线
    };

    /// <summary>
    /// 直线方向
    /// </summary>
    public enum LineDirection
    {
        [Description("Z轴正方向")]
        ePositiveAxisZ = 0, // 

        [Description("Z轴负方向")]
        eNegetiveAxisZ = 1, // 
    };

    // 带局外点拟合的自适应参数
    struct stVW_AdaptiveFitParameter
    {
        double m_dDisThre;      // 距离阈值
        double m_dConfidence;   // 置信度
        double m_dOutlierRatio; // 局外点比例
        int m_nSampleHalfWidth; // 采样半宽
    };


    // 拟合得到的结果
    struct stVW_3DLineFitResult
    {
        double m_dRms;       // RMS误差
        double m_dMaxOffset; // 最大偏移
        double m_dMinOffset; // 最小偏移 
    };
    #endregion

    #region 坐标系
    public enum CoordAxis
    {
        [Description("XY轴")]
        XY,

        [Description("XZ轴")]
        XZ,

        [Description("YZ轴")]
        YZ,
    }

    public enum CartCoordAxis
    {
        [Description("X轴")]
        X,

        [Description("Y轴")]
        Y,

        [Description("Z轴")]
        Z,
    }


    #endregion

    #region 公差
    public enum ParallelismMode
    {
        [Description("基于线面")]
        LinePlane,

        [Description("基于线")]
        Line,

        [Description("基于面")]
        Plane,
    }


    public enum RoundnessMode
    {
        [Description("最小二乘法")]
        LeastSquareFit,

        [Description("用户定义")]
        UserDefine,
    }

    /// <summary>
    /// 获取理想平面
    /// </summary>
    public enum FitType
    {
        [Description("最小二乘法")]
        eLeastSquareFit,

        [Description("最小包容法")]
        eMinimumZoneFit,

        [Description("自定义")]
        eUserDefine,
    }

    /// <summary>
    ///生成公差类型
    /// </summary>
    public enum GenToleranceType
    {

        [Description("自计算参考")]
        SelfCalcul,//使用最小二成等方式

        [Description("指定参考")]
        UserDefine,
    }

    /// <summary>
    /// 获取理想平面
    /// </summary>
    public enum CircleFitMode
    {
        [Description("最小二乘")]
        eLeastSquareFit,

        [Description("带局外点")]
        eAdaption,
    }

    /// <summary>
    /// 圆拟合
    /// </summary>
    public enum CircleFitCloudShape
    {
        [Description("圆")]
        Circle = 0,  // 
        [Description("圆弧或曲线")]
        Curve = 1,  // 
        [Description("椭圆")]
        Ellipse = 2,  // 
    };

    /// <summary>
    /// 圆拟合基准面定义
    /// </summary>
    public enum BasePlaneType
    {
        [Description("内部拟合")]
        eInternalFit = 0,  // 

        [Description("用户定义")]
        eUserDefined = 1,  // 
    };



    /// <summary>
    /// 约束类型
    /// </summary>
    public enum ConstraintType
    {
        [Description("无约束")]
        eNoConstraint = 0,           // 无约束(对应公差带为圆柱区域)

        [Description("指定平面约束")]
        ePlaneConstraint = 1,        // 指定平面约束(对应公差带为指定平面内的两平行直线之间的区域)

        [Description("指定方向约束")]
        eDirectionConstraint = 2,    // 指定方向约束(对应公差带为给定方向上的两平行平面之间的区域）
    };
    #endregion

    #region 去重


    public enum MatrixType
    {
        [Description("自动")]
        Auto = 0,

        [Description("从本地文件")]
        FromFile,
    }

    // 平滑模式
    public enum SmoothMode
    {
        [Description("双边滤波平滑")]
        eBilateralFilter = 0, // 双边滤波平滑           

        [Description("线性平滑")]
        eLinearFilter,        // 线性平滑

        [Description("mls平滑")]
        eMlsFilter,           // mls平滑
    };


    // 合并模式
    public enum MergeMode
    {
        [Description("仅平滑")]
        eOnlySmooth = 0, // 仅平滑（只对重叠点云平滑至单层，不删除重合点）

        //[Description("体素采样")]
        //eVoxelSample,    // 体素采样 

        //[Description("局部邻域均值")]
        //eNeighsMean,     // 局部邻域均值

        //[Description("均值漂移")]
        //eMeanShift,      // 均值漂移            
    };


    // 邻域搜索模式
    public enum NeighsSearchMode
    {
        [Description("体素近邻搜索")]
        eVoxelGridSearch = 0, // 体素近邻搜索

        [Description("树搜索")]
        eKDTreeSearch,        // 树搜索
    };

    #endregion

    #region 三位平面拟合
    // 3D平面拟合方法
    public enum PlaneFitMethod
    {
        [Description("面积")]
        eArea = 0,   // 面积

        [Description("点")]
        ePoints = 1, // 点
    };


    // 平面方向
    public enum PlaneDierection
    {
        [Description("正向像素空间Z")]
        ePositiveImageZ = 0, // 正向像素空间Z

        [Description("负向像素空间Z")]
        eNegativeImageZ = 1, // 负向像素空间Z
    };


    // 区域形状，仅在拟合方法为eArea模式下有效
    public enum RegionMode
    {
        [Description("圆")]
        eCircle = 0,             // 圆

        [Description("椭圆")]
        eEllipse,                // 椭圆

        [Description("多边形")]
        ePolygon,                // 多边形

        [Description("矩形")]
        eRectangle,              // 矩形

        [Description("仿射矩形")]
        eAffineRectangle,        // 仿射矩形

        [Description("圆环段")]
        eCircularAnnulusSection, // 圆环段

        [Description("多仿射矩形")]
        eManyAffineRectangles,   // 多仿射矩形
        //eEllipticalAnnulusSection,	// 椭圆环段
    };


    // Z查询方法，仅在拟合方法为ePoints模式下有效
    public enum ZLookupMethod
    {
        [Description("单个像素")]
        eSinglePixel = 0,        // 单个像素

        [Description("邻域中值")]
        eNeighborhoodMedian = 1, // 邻域中值

        [Description("邻域均值")]
        eNeighborhood = 2,       // 邻域(均值)
    };
    #endregion

    #region 卡尺类型
    public enum CaliperType
    {
        [Description("球形卡尺")]
        eSphere = 0,  // 

        [Description("圆柱卡尺")]
        eCylinder,    // 

        [Description("CMM")]
        CMM
    };

    // 拟合类型（当前支持平面/圆柱/圆锥/球，卡尺类型为模拟CMM时用于拟合面）
    public enum CaliperFitType
    {
        [Description("平面")]
        eFitPlane = 0,   // 平面

        [Description("圆柱")]
        eFitCylinder,    // 圆柱

        [Description("圆锥")]
        eFitCone,        // 圆锥

        [Description("球")]
        eFitSphere,      // 球
    };
    #endregion

    #region 距离

    /// <summary>
    /// 两点类型
    /// </summary>
    public enum TwoPointDistanceType
    {
        [Description("默认距离")]
        Default,

        [Description("X轴距离")]
        X,

        [Description("Y轴距离")]
        Y,

        [Description("Z轴距离")]
        Z
    }
    #endregion


    /// <summary>
    /// 创建面的方式
    /// </summary>
    public enum GenPlaneType
    {
        [Description("多点建面")]
        Point3,

        [Description("点和方向")]
        PointDirection,

        [Description("方向和偏移")]
        DirectionOffset
    }

    /// <summary>
    /// 垂直度模式
    /// </summary>
    public enum VerticalityMode
    {
        [Description("线和线")]
        LineToLine = 0,

        [Description("线和面")]
        LineToPlane = 1,

        [Description("面和线")]
        PlaneToLine = 2,

        [Description("面和面")]
        PlaneToPlane = 3
    }

    /// <summary>
    ///垂直度定义类型
    /// </summary>
    public enum VerticalityDefMode
    {
        [Description("平面型")]
        Plane = 0,//（最小包容距离）

        [Description("圆柱型")]
        Cylinder = 1,// （最小外接圆直径）
    }

    /// <summary>
    ///垂直度参考类型
    /// </summary>
    public enum VerticalityRef
    {
        [Description("标准面")]
        StaPlane = 0,

        [Description("标准线")]
        StaLine = 1,
    }


    /// <summary>
    /// 获取理想平面
    /// </summary>
    public enum InclinationFitType
    {
        [Description("不拟合")]
        None,

        [Description("最小二乘法")]
        eLeastSquareFit,
    }

    /// <summary>
    /// 线轮廓度对齐方式
    /// </summary>
    public enum AlignmentType
    {
        [Description("不对齐")]
        WithoutAlignment = 0,    // 不对齐模式，即直接计算标称点和实际点的轮廓度

        [Description("最小二乘")]
        LeastSquares,            // 最小二乘模式

        [Description("矢量最小二乘")]
        VectorLeastSquares,      // 矢量最小二乘模式
    }

    #region 截面工具

    /// <summary>
    /// 点云是否投影到面
    /// </summary>
    public enum IsProjectType
    {
        [Description("点云不投影")]
        NoProject,
        [Description("点云投影到面")]
        ProjectPlane,//（投影即将截面ROI分割的点云数据再次投影到面上）
    }

    /// <summary>
    /// 截面几何路径类型： 线段、圆、圆弧
    /// </summary>
    public enum SecPathType
    {
        [Description("线段")]
        LineSegment,
        [Description("圆")]
        Circle,
        [Description("圆弧")]
        CircularArc,
    }

    /// <summary>
    /// 圆弧生成方式
    /// </summary>
    public enum GenCirArcWay
    {
        [Description("起点和终点")]
        StartEndPoint,
        [Description("起点和弧度")]
        PointArc,
    }

    /// <summary>
    /// 截面模式:无界平面、三维矩形、三维圆
    /// </summary>
    public enum SectionMode
    {
        //[Description("无界平面")]
        //Plane,
        [Description("三维矩形")]
        Rectangle3D = 1,
        [Description("三维圆")]
        Circle3D,
    }

    /// <summary>
    /// 阵列模式
    /// </summary>
    public enum ArrayMode
    {
        [Description("指定个数")]
        Number,
        [Description("指定间隔")]
        Interval,

    }



    #endregion

    #region 几何与点云显示模式

    public enum ShowMode
    {
        [Description("顶点模式")]
        Point,
        [Description("线框模式")]
        Line,
        [Description("面模式")]
        Plane,
    }

    /// <summary>
    /// 比较类型
    /// </summary>
    public enum CompareType
    {
        [Description("网格比较")]
        CloudMesh = 0,
        [Description("高度色谱")]
        HeightCorSpect = 1,
    }

    /// <summary>
    /// 包围盒类型
    /// </summary>
    public enum BoundBoxType
    {
        [Description("近似最小体积")]
        SmallestVolume = 0, // 近似最小体积包围盒，效率最慢
        [Description("主方向下")]
        SmallestAxis,       // 主方向下的最小包围盒（调用对应接口可主动设置主方向）
        [Description("轴向平行")]
        AxisAlign,          // 轴向平行包围盒，效率最高
    };


    #endregion

    /// <summary>
    /// 特征点模式
    /// </summary>
    public enum FeaturePointMode
    {
        [Description("所有点")]
        AllPoint = 0,
        [Description("边缘点")]
        EdgePoint,
    }
}
