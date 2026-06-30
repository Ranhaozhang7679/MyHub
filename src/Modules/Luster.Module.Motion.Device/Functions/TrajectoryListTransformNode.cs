using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 轨迹列表坐标转换节点(P2-E,TES-190)。
    /// 提炼自旧程序 Check5AxisStationBase.FreshActionList 的后端可执行语义:
    /// 批量轨迹点(六维位姿 X/Y/Z/RX/RY/RZ)在工件坐标↔工具坐标间转换。
    /// 用 [Parameter(MultiValues=true)] 承载轨迹点列表(对齐 AxisPosMove 列表范式),
    /// DoExcute 遍历逐点调 Coord5Axis 姿态变换(PoseO2D/PoseD2O/PoseTool2Work/PoseWork2Tool),收集到 OUT 列表。
    /// 纯算法节点,不侵入 IMotionCard 主契约。
    /// 挂在 Device 模块(与 AxisPosMove 同源,避开 FiveAxisModule 与 P5-3 的注册点竞写)。
    /// </summary>
    /// <remarks>
    /// 旧程序轨迹编辑是 WinForm UI(FormEdit分点 等),语义不能直接搬;此处提炼的是
    /// "轨迹点列表 + 坐标系转换"的后端可执行语义,UI 编辑能力不在本节点范围。
    /// </remarks>
    public class TrajectoryListTransformNode : MotionFunction
    {
        /// <summary>坐标转换模式</summary>
        public enum TransformMode
        {
            [Description("正解(工件→工具)")]
            Org2Dest,

            [Description("逆解(工具→工件)")]
            Dest2Org,

            [Description("工具姿态→工件姿态")]
            Tool2Work,

            [Description("工件姿态→工具姿态")]
            Work2Tool,
        }

        public TrajectoryListTransformNode()
        {
            this.Tips = "轨迹列表坐标转换";
        }

        #region 输入:轨迹点列表 + 转换模式

        /// <summary>输入轨迹点列表(六维位姿),MultiValues 驱动 ParamGrid 多行编辑</summary>
        [Parameter("输入轨迹点列表", 1, CN = "输入轨迹", MultiValues = true)]
        public virtual TrajectoryPointList InputPoints { get; set; }

        /// <summary>坐标转换模式</summary>
        [Parameter("坐标转换模式", 2, CN = "转换模式", DefaultV = TransformMode.Org2Dest)]
        public virtual TransformMode Mode { get; set; }

        #endregion

        #region 输入:Coord5Axis 参数(A 轴)

        [Parameter("A旋转中心X", 10, Group = "A轴", CN = "ACenterX", DefaultV = 1.0)]
        public virtual double ACenterX { get; set; }

        [Parameter("A旋转中心Y", 11, Group = "A轴", CN = "ACenterY", DefaultV = 0.0)]
        public virtual double ACenterY { get; set; }

        [Parameter("A旋转中心Z", 12, Group = "A轴", CN = "ACenterZ", DefaultV = 0.0)]
        public virtual double ACenterZ { get; set; }

        [Parameter("A轴方向矢量X", 13, Group = "A轴", CN = "ADirX", DefaultV = 0.0)]
        public virtual double ADirX { get; set; }

        [Parameter("A轴方向矢量Y", 14, Group = "A轴", CN = "ADirY", DefaultV = 0.0)]
        public virtual double ADirY { get; set; }

        [Parameter("A轴方向矢量Z", 15, Group = "A轴", CN = "ADirZ", DefaultV = 0.0)]
        public virtual double ADirZ { get; set; }

        [Parameter("A一圈脉冲数", 16, Group = "A轴", CN = "ACirPulses", DefaultV = 0.0)]
        public virtual double ACirPulses { get; set; }

        #endregion

        #region 输入:Coord5Axis 参数(C 轴)

        [Parameter("C旋转中心X", 20, Group = "C轴", CN = "CCenterX", DefaultV = 0.0)]
        public virtual double CCenterX { get; set; }

        [Parameter("C旋转中心Y", 21, Group = "C轴", CN = "CCenterY", DefaultV = 0.0)]
        public virtual double CCenterY { get; set; }

        [Parameter("C旋转中心Z", 22, Group = "C轴", CN = "CCenterZ", DefaultV = 0.0)]
        public virtual double CCenterZ { get; set; }

        [Parameter("C轴方向矢量X", 23, Group = "C轴", CN = "CDirX", DefaultV = 0.0)]
        public virtual double CDirX { get; set; }

        [Parameter("C轴方向矢量Y", 24, Group = "C轴", CN = "CDirY", DefaultV = 0.0)]
        public virtual double CDirY { get; set; }

        [Parameter("C轴方向矢量Z", 25, Group = "C轴", CN = "CDirZ", DefaultV = 1.0)]
        public virtual double CDirZ { get; set; }

        [Parameter("C一圈脉冲数", 26, Group = "C轴", CN = "CCirPulses", DefaultV = 0.0)]
        public virtual double CCirPulses { get; set; }

        #endregion

        #region 输出:转换后轨迹点列表

        /// <summary>转换后轨迹点列表(OUT)</summary>
        [Parameter("转换后轨迹点列表", 50, CN = "输出轨迹", ParamType = ParamType.OUT)]
        public virtual TrajectoryPointList OutputPoints { get; set; }

        #endregion

        /// <summary>
        /// 由标量参数构造 Coord5Axis 运动学核心(参数契约与 FiveAxisKinematicsNode 对齐)。
        /// </summary>
        private Coord5Axis BuildCoord5Axis()
        {
            return new Coord5Axis
            {
                ACenter = new PositionXYZ(ACenterX, ACenterY, ACenterZ),
                ADir = new PositionXYZ(ADirX, ADirY, ADirZ),
                ACirPulses = ACirPulses,
                CCenter = new PositionXYZ(CCenterX, CCenterY, CCenterZ),
                CDir = new PositionXYZ(CDirX, CDirY, CDirZ),
                CCirPulses = CCirPulses,
            };
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            OutputPoints = new TrajectoryPointList();

            // 空列表直接返回(不视为错误),与 AxisPosMove 空点位的容忍语义一致
            if (InputPoints == null || InputPoints.Count == 0)
            {
                MyOwner?.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug,
                    $"TrajectoryListTransform({Mode}): 输入轨迹为空,跳过转换");
                return true;
            }

            var coord = BuildCoord5Axis();

            // 遍历轨迹点,逐点做坐标转换(提炼自旧程序 FreshActionList 的关节↔工件位姿转换语义)
            foreach (var pt in InputPoints)
            {
                PositionXYZRxRyRz outPt;
                switch (Mode)
                {
                    case TransformMode.Dest2Org:
                        outPt = coord.PoseD2O(pt);
                        break;
                    case TransformMode.Tool2Work:
                        outPt = coord.PoseTool2Work(pt);
                        break;
                    case TransformMode.Work2Tool:
                        outPt = coord.PoseWork2Tool(pt);
                        break;
                    default:
                        outPt = coord.PoseO2D(pt);
                        break;
                }
                OutputPoints.Add(outPt);
            }

            MyOwner?.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug,
                $"TrajectoryListTransform({Mode}): {InputPoints.Count} 点 -> {OutputPoints.Count} 点 转换完成");

            return true;
        }
    }

    /// <summary>
    /// 轨迹点列表容器(List<PositionXYZRxRyRz> 派生),承载六维位姿轨迹点。
    /// 对齐 AxisPosMove.VAxisPos 的 MultiValues 列表范式,但剥离运动设备耦合,纯位姿数据,
    /// 便于 ParamGrid 多行编辑与 recipe XML 序列化。
    /// </summary>
    [Serializable]
    public class TrajectoryPointList : List<PositionXYZRxRyRz>
    {
        public TrajectoryPointList() : base() { }
    }
}
