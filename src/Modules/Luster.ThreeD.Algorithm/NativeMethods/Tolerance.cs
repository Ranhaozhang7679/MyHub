#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Tolerance
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.NativeMethods
* 文 件 名:       Tolerance.cs
* 创建时间:       2022/1/27 16:53:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      d9d0f32b-c603-4c9c-bbed-1a00ecdef828
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/27 16:53:00
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.ThreeD.Algorithm.Enums;
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

        #region 1---形位公差

        /// <summary>
        /// 获取基于基准线+面的 直线平行度
        /// </summary>
        /// <param name="clondPtr">输入点集</param>
        /// <param name="cloudIndexes">数据索引,当为空时，表示所有点参与计算</param>
        /// <param name="mode">构建模式</param>
        /// <param name="baseLine">基准线</param>
        /// <param name="basePlane">基准面</param>
        /// <param name="val">输出平行度</param>
        /// <param name="maxOffset">最大偏移值</param>
        /// <param name="minOffset">最小偏移值</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
          SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetParallelism(IntPtr clondPtr, IArray cloudIndexes, ParallelismMode mode, IntPtr baseLine, IntPtr basePlane, out double val, out double maxOffset, out double minOffset, out IntPtr errMsg);

        /// <summary>
        /// 圆度
        /// </summary>
        /// <param name="cloudPtr">输入点集</param>
        /// <param name="cloudIndexes">点云索引</param>
        /// <param name="roundnessMode">获取理想圆的方法   0-最小二乘 1-用户自定义</param>
        /// <param name="planePtr">基准面 当获取理想圆不是用户自定义时，需要设定该参数</param>
        /// <param name="circle">标准圆 当获取理想圆是用户自定义时，需要设定该参数</param>
        /// <param name="roundnessVal">圆度</param>
        /// <param name="maxOffset">最大偏移值</param>
        /// <param name="minOffset">最小偏移值</param>
        /// <param name="outCircle">拟合圆</param>
        /// <param name="msgPtr">失败信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetRoundness(IntPtr cloudPtr, IArray cloudIndexes, int roundnessMode, IntPtr planePtr, IntPtr circle, out double roundnessVal, out double maxOffset, out double minOffset, out IntPtr outCircle, out IntPtr msgPtr);

        /// <summary>
        /// 获取平面度
        /// </summary>
        /// <param name="in_pointCloud">输入点云</param>
        /// <param name="cloudIndexes">点云索引</param>
        /// <param name="AcquisitionMethod">获取理想平面的方法   0-最小二乘; 1-最小包容 2-用户自定义</param>
        /// <param name="in_planeParam">基准面  当通过用户自定义方式获取理想平面时，需要设定该参数</param>
        /// <param name="out_flatness">输出平面度值</param>
        /// <param name="out_maxoffset">最大偏移值</param>
        /// <param name="out_minoffset">最小偏移值</param>
        /// <param name="outPlane">拟合平面</param>
        /// <param name="outPointsOffset">点云到拟合平面的偏差</param>
        /// <param name="errMsg">失败信息</param>
        /// <param name="bHighPrecisionMode">是否开启高精度计算模式，AcquisitionMethod设定最小包容时有效，开启后耗时会增加</param>
        /// <param name="iMaxIteration">最大迭代次数</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetFlatness(IntPtr in_pointCloud, IArray cloudIndexes, int AcquisitionMethod, IntPtr in_planeParam, out double out_flatness, out double out_maxoffset, out double out_minoffset, out IntPtr outPlane, out DArray outPointsOffset, out IntPtr errMsg, bool bHighPrecisionMode = false, int iMaxIteration = 50);

        /// <summary>
        /// 基于点集合获取平面度
        /// </summary>
        /// <param name="pointArray">点集合</param>
        /// <param name="AcquisitionMethod">0-最小二乘; 1-最小包容 2-用户自定义</param>
        /// <param name="in_planeParam">基准面  当通过用户自定义方式获取理想平面时，需要设定该参数</param>
        /// <param name="out_flatness">平面度值</param>
        /// <param name="out_maxoffset">最大偏移值</param>
        /// <param name="out_minoffset">最小偏移值</param>
        /// <param name="outPlane">拟合平面</param>
        /// <param name="arr">点云到拟合平面的偏差</param>
        /// <param name="errMsg">错误消息</param>
        /// <param name="bHighPrecisionMode">是否开启高精度计算模式</param>
        /// <param name="iMaxIteration">最大迭代次数</param>
        /// <returns></returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetFlatnessByPointGroup(LPointArray pointArray, int AcquisitionMethod, IntPtr in_planeParam, out double out_flatness, out double out_maxoffset, out double out_minoffset, out IntPtr outPlane,out DArray arr, out IntPtr errMsg, bool bHighPrecisionMode = false, int iMaxIteration = 50);

        /// <summary>
        /// 直线度
        /// </summary>
        /// <param name="in_pointCloud">输入点云</param>
        /// <param name="cloudIndexes">点云索引</param>
        /// <param name="AcquisitionMethod">获取理想直线的方法</param>
        /// <param name="in_lineParam">标准直线</param>
        /// <param name="ConstraintMethod">约束方法 0-无约束 1-平面约束 2-方向约束</param>
        /// <param name="in_planeConstraintParam">约束平面</param>
        /// <param name="in_dirConstraintParam">约束方向</param>
        /// <param name="out_straightness">直线度</param>
        /// <param name="out_maxoffset">最大偏移值</param>
        /// <param name="out_minoffset">最小偏移值</param>
        /// <param name="linePtr">拟合直线</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
      SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetStraightness(IntPtr in_pointCloud, IArray cloudIndexes, int AcquisitionMethod, IntPtr in_lineParam, int ConstraintMethod, IntPtr in_planeConstraintParam, LVector in_dirConstraintParam, out double out_straightness, out double out_maxoffset, out double out_minoffset, out IntPtr linePtr, out IntPtr errMsg);



        /// <summary>
        /// 垂直度
        /// </summary>
        /// <param name="in_pointCloud">输入点云</param>
        /// <param name="cloudIndexes">点云索引</param>
        /// <param name="definitionMode">垂直度定义方式0-平面型（最小包容距离）1-圆柱型（最小外接圆直径）仅参考元素是平面时有效</param>
        /// <param name="referenceMode">参考类型  0-参考元素是标准面 1-参考元素是标准线</param>
        /// <param name="in_basePlane">约束方法 0-无约束 1-平面约束 2-方向约束</param>
        /// <param name="in_baseLine">基准线  注:垂直度模式为线线时设置</param>
        /// <param name="out_verticality">输出垂直度</param>
        /// <param name="out_maxoffset">上偏移值</param>
        /// <param name="out_minoffset">下偏移值</param>
        /// <param name="out_plane">中心面</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
    SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetVerticality(IntPtr in_pointCloud, IArray cloudIndexes, int definitionMode, int referenceMode, IntPtr in_basePlane, IntPtr in_baseLine, out double out_verticality, out double out_maxoffset, out double out_minoffset, out IntPtr out_plane, out IntPtr errMsg);


        /// <summary>
        /// 倾斜度
        /// </summary>
        /// <param name="in_pointCloud">输入点云</param>
        /// <param name="cloudIndexes">点云索引</param>
        /// <param name="inclinationAngle">倾斜角度 角度范围:(-180, 180]</param>
        /// <param name="fitMode">0:不拟合  1:最小二乘方式</param>
        /// <param name="inclinationMode">倾斜度模式   0:线对线 1:线对面 2:面对线 3:面对面</param>
        /// <param name="in_projectionPlane">投影面  注：当拟合方式为"不拟合"时设置</param>
        /// <param name="in_basePlane">基准面   注:倾斜度模式为"线对面"、"面对面"时设置</param>
        /// <param name="in_baseLine">基准线   注:倾斜度模式为"线对线"、"面对线"时设置</param>
        /// <param name="out_inclination">倾斜度</param>
        /// <param name="out_maxoffset">上偏移值</param>
        /// <param name="out_minoffset">下偏移值</param>
        /// <param name="out_plane">中心面</param>
        /// <param name="errMsg">失败消息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
    SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetInclination(IntPtr in_pointCloud, IArray cloudIndexes, double inclinationAngle, int fitMode, int inclinationMode, IntPtr in_projectionPlane, IntPtr in_basePlane, IntPtr in_baseLine, out double out_inclination, out double out_maxoffset, out double out_minoffset, out IntPtr out_plane, out IntPtr errMsg);

        /// <summary>
        /// 圆柱度
        /// </summary>
        /// <param name="in_pointCloud">输入点云</param>
        /// <param name="cloudIndexes">点云索引</param>
        /// <param name="fitMode">获取理想圆柱的方式   0:最小二乘 1:最小包容  2:用户自定义</param>
        /// <param name="in_standardCylinder">标准圆柱  注：当获取方式为"用户自定义"时设置</param>
        /// <param name="out_cylindricity">圆柱度</param>
        /// <param name="out_maxoffset">上偏移值</param>
        /// <param name="out_minoffset">下偏移值</param>
        /// <param name="out_cylinder">拟合圆柱</param>
        /// <param name="errMsg">失败信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
    SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetCylindricity(IntPtr in_pointCloud, IArray cloudIndexes, int fitMode, IntPtr in_standardCylinder, out double out_cylindricity, out double out_maxoffset, out double out_minoffset, out IntPtr out_cylinder, out IntPtr errMsg);

        /// <summary>
        /// 获取线轮廓度
        /// </summary>
        /// <param name="alignmentMode">对齐方式  0:不对齐模式 1:最小二乘模式  2:矢量最小二乘 3:最小最大 4:矢量最小最大</param>
        /// <param name="inNominalPoints">标称点</param>
        /// <param name="inNominalNormals">标称点法向</param>
        /// <param name="inMeasurePts">测量点</param>
        /// <param name="pointsNums">测量点个数</param>
        /// <param name="out_tolerance">轮廓度</param>
        /// <param name="out_deviation">各点之间的偏差</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
   SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GetLineProfileTolerance(int alignmentMode, LPoint3[] inNominalPoints, LVector[] inNominalNormals, LPoint3[] inMeasurePts, int pointsNums, out double out_tolerance, out DArray out_deviation, out IntPtr errMsg);

        /// <summary>
        /// 面轮廓度
        /// </summary>
        /// <param name="pointCloud">点云数据</param>
        /// <param name="indexes">数据索引</param>
        /// <param name="searchIndexes">经过元素得到的索引</param>
        /// <param name="sType">0-最小值，1-最大值，2-均值</param>
        /// <param name="dArray">对应的距离值</param>
        /// <param name="outMaxPoint">上偏移值</param>
        /// <param name="tolerance">轮廓度</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>0: 成功  反之，失败</returns>
        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl,
        SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int CalcMaxDistance(IntPtr pointCloud, IArray indexes, IArray searchIndexes, int sType, DArray dArray, out LPoint3 outMaxPoint, out double tolerance, out IntPtr errMsg);

        #endregion
    }
}