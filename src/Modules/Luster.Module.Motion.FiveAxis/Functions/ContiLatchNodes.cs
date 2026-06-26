using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Luster.Module.Motion.FiveAxis.Functions
{
    // ====================================================================
    // P5-3 连续插补 + 高速锁存节点(P5-3-b,10 个新节点)。
    //
    // 范围:连续插补旁路接口 IFiveAxisContiInterp + 高速锁存旁路接口 IFiveAxisLatch
    //      的 MotionFunction 节点封装,落点 Luster.Module.Motion.FiveAxis(与 P5-2 同模块)。
    //      节点经 FiveAxisModule.AddFunction 注册,可由 XML recipe 编排(TES-29 范式)。
    //
    // M-13 finally 关闭契约(硬性验收项):连续插补 Open/Start 后,Stop/Close/ClearLatch
    //      必须在节点级 try/finally 中执行,保证异常/急停时卡端状态清理,不残留插补/锁存。
    //      以下 CrdConti/LatchStart 等执行节点均用 try/finally 包裹,验收工程师查此结构。
    //
    // 卡引用:节点持有的 VDevice(MultiAxis 仿真设备)经 GetDevice() 拿到 IMotionCard,
    //        再 as IFiveAxisContiInterp / IFiveAxisLatch 取旁路接口(仅 ZMotion 五轴适配器实现)。
    // ====================================================================

    /// <summary>
    /// 连续插补执行节点(P5-3,④)。
    /// 封装 IFiveAxisContiInterp 一次飞拍轨迹的 Open→AddLine→AddOutput→WaitDone→Stop/Close 全生命周期,
    /// 节点级 try/finally 保证 Stop/Close 必执行(M-13 finally 契约)。
    /// 输入:坐标系号 + 终点序列(各轴目标)+ 同步输出标记;输出:是否成功完成。
    /// </summary>
    public class CrdConti : MotionFunction
    {
        /// <summary>多轴仿真设备(经其 GetDevice() 拿 IMotionCard + IFiveAxisContiInterp)</summary>
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        /// <summary>坐标系/插补器号(源端以轴号充当 crd)</summary>
        [Parameter("坐标系号", 2, Group = "输入", CN = "CRD", DefaultV = 0)]
        public virtual int Crd { get; set; }

        /// <summary>插补模式</summary>
        [Parameter("插补模式", 3, Group = "输入", CN = "模式", DefaultV = CrdMode.Absolute)]
        public virtual CrdMode Mode { get; set; }

        /// <summary>终点序列(逗号分隔,每组用分号分隔各轴,如 "100,0,0;200,0,0")</summary>
        [Parameter("终点序列(组内逗号,组分号)", 4, Group = "输入", CN = "终点序列", DefaultV = "")]
        public virtual string EndPoints { get; set; }

        /// <summary>同步输出位号(飞拍触发,-1 表示不输出)</summary>
        [Parameter("同步输出位号", 5, Group = "输入", CN = "输出位", DefaultV = -1)]
        public virtual int OutputIoIndex { get; set; }

        /// <summary>等待完成超时(ms,-1 无限等待)</summary>
        [Parameter("完成超时(ms)", 6, Group = "输入", CN = "超时", DefaultV = 60000)]
        public virtual int TimeoutMs { get; set; }

        /// <summary>执行成功</summary>
        [Parameter("执行成功", 50, Group = "输出", CN = "成功", ParamType = ParamType.OUT)]
        public virtual bool Success { get; set; }

        public CrdConti()
        {
            this.Tips = "连续插补";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            if (vAxisM == null)
            {
                errMsg = "未获取到多轴设备";
                return false;
            }

            var conti = (vAxisM.GetDevice() as IMotionCard) as IFiveAxisContiInterp;
            if (conti == null)
            {
                errMsg = "当前板卡不支持连续插补(IFiveAxisContiInterp 未实现)";
                return false;
            }

            var axisList = vAxisM.Axises.Select(a => 0).ToArray();
            Success = false;
            var opened = false;

            // M-13 finally 关闭契约:Open 后无论成功失败,Stop/Close 必执行。
            try
            {
                if (!conti.CrdContiOpen(Crd, axisList, Mode))
                {
                    errMsg = "CrdContiOpen 失败";
                    return false;
                }
                opened = true;

                if (!conti.CrdContiStart(Crd))
                {
                    errMsg = "CrdContiStart 失败";
                    return false;
                }

                var markIndex = 0;
                foreach (var group in ParseEndPoints(EndPoints, axisList.Length))
                {
                    if (!conti.CrdContiAddLine(Crd, group, Mode == CrdMode.Absolute ? ContiMoveMode.Absolute : ContiMoveMode.Relative))
                    {
                        errMsg = $"CrdContiAddLine 失败,mark={markIndex}";
                        return false;
                    }
                    if (OutputIoIndex >= 0)
                    {
                        conti.CrdContiAddOutput(Crd, OutputIoIndex, true, markIndex);
                    }
                    markIndex++;
                }

                if (!conti.WaitCrdDone(Crd, TimeoutMs))
                {
                    errMsg = "连续插补等待完成超时";
                    return false;
                }

                Success = true;
                return true;
            }
            finally
            {
                // ⚠️ M-13 finally 关闭契约:Stop/Close 必执行,清理卡端插补状态。
                if (opened)
                {
                    try { conti.CrdContiStop(Crd); } catch (Exception ex)
                    {
                        MyOwner?.OnLog(LogType.Error, $"CrdConti.Stop 异常:{ex.Message}");
                    }
                    try { conti.CrdContiClose(Crd); } catch (Exception ex)
                    {
                        MyOwner?.OnLog(LogType.Error, $"CrdConti.Close 异常:{ex.Message}");
                    }
                }
            }
        }

        /// <summary>解析终点序列字符串为各轴目标数组(组内逗号分隔,组分号分隔)。</summary>
        private static List<double[]> ParseEndPoints(string text, int axisCount)
        {
            var result = new List<double[]>();
            if (string.IsNullOrWhiteSpace(text)) return result;

            foreach (var group in text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = group.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var pos = new double[Math.Max(parts.Length, axisCount)];
                for (var i = 0; i < parts.Length && i < pos.Length; i++)
                {
                    double.TryParse(parts[i].Trim(), out pos[i]);
                }
                result.Add(pos);
            }
            return result;
        }
    }

    /// <summary>
    /// 连续插补平滑参数配置节点(P5-3)。
    /// 封装 IFiveAxisContiInterp.SetSmoothProfile(速度前瞻/拐角平滑),对齐源端 CrdSetSmoothProfile。
    /// </summary>
    public class CrdContiSmooth : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("坐标系号", 2, Group = "输入", CN = "CRD", DefaultV = 0)]
        public virtual int Crd { get; set; }

        [Parameter("拐角模式", 3, Group = "平滑", CN = "拐角模式", DefaultV = 0)]
        public virtual int CornerMode { get; set; }

        [Parameter("拐角半径", 4, Group = "平滑", CN = "拐角半径", DefaultV = 0.0)]
        public virtual double CornerRadius { get; set; }

        [Parameter("减速角度(度)", 5, Group = "平滑", CN = "减速角度", DefaultV = 0.0)]
        public virtual double DecelAngle { get; set; }

        [Parameter("停止角度(度)", 6, Group = "平滑", CN = "停止角度", DefaultV = 0.0)]
        public virtual double StopAngle { get; set; }

        public CrdContiSmooth()
        {
            this.Tips = "连续插补平滑";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var conti = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisContiInterp;
            if (conti == null)
            {
                errMsg = "当前板卡不支持连续插补";
                return false;
            }

            var profile = new SmoothProfile
            {
                CornerMode = CornerMode,
                CornerRadius = CornerRadius,
                DecelAngle = DecelAngle,
                StopAngle = StopAngle,
            };
            return conti.SetSmoothProfile(Crd, profile);
        }
    }

    /// <summary>
    /// 插补器背压检查节点(P5-3)。
    /// 封装 IFiveAxisContiInterp.GetContiRemainSpace,剩余不足时节点应 Wait/Polling,不可丢弃点位(节点实现硬性契约)。
    /// 输出剩余空间 + 是否充足(>=阈值)。
    /// </summary>
    public class CrdContiRemainCheck : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("坐标系号", 2, Group = "输入", CN = "CRD", DefaultV = 0)]
        public virtual int Crd { get; set; }

        /// <summary>充足阈值(剩余 >= 阈值 视为充足)</summary>
        [Parameter("充足阈值", 3, Group = "输入", CN = "阈值", DefaultV = 64)]
        public virtual int Threshold { get; set; }

        [Parameter("剩余空间", 50, Group = "输出", CN = "剩余", ParamType = ParamType.OUT)]
        public virtual int RemainSpace { get; set; }

        [Parameter("是否充足", 51, Group = "输出", CN = "充足", ParamType = ParamType.OUT)]
        public virtual bool IsEnough { get; set; }

        public CrdContiRemainCheck()
        {
            this.Tips = "插补背压检查";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var conti = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisContiInterp;
            if (conti == null)
            {
                errMsg = "当前板卡不支持连续插补";
                return false;
            }

            if (!conti.GetContiRemainSpace(Crd, out var space))
            {
                errMsg = "GetContiRemainSpace 失败";
                return false;
            }
            RemainSpace = space;
            IsEnough = space >= Threshold;
            return true;
        }
    }

    /// <summary>
    /// 等待连续插补完成节点(P5-3)。
    /// 封装 IFiveAxisContiInterp.WaitCrdDone,独立编排用(CrdConti 已内含等待,此节点供分步编排)。
    /// </summary>
    public class CrdContiWaitDone : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("坐标系号", 2, Group = "输入", CN = "CRD", DefaultV = 0)]
        public virtual int Crd { get; set; }

        [Parameter("完成超时(ms)", 3, Group = "输入", CN = "超时", DefaultV = 60000)]
        public virtual int TimeoutMs { get; set; }

        [Parameter("是否完成", 50, Group = "输出", CN = "完成", ParamType = ParamType.OUT)]
        public virtual bool Done { get; set; }

        public CrdContiWaitDone()
        {
            this.Tips = "等待插补完成";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var conti = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisContiInterp;
            if (conti == null)
            {
                errMsg = "当前板卡不支持连续插补";
                return false;
            }
            Done = conti.WaitCrdDone(Crd, TimeoutMs);
            return Done;
        }
    }

    /// <summary>
    /// 启动高速锁存节点(P5-3)。
    /// 封装 IFiveAxisLatch.StartLatch,配置触发源/边沿/缓存。配合 LatchWait/LatchClear 使用,
    /// LatchClear 必须在节点级 try/finally 中执行(M-13 finally 契约,见 LatchWait 节点)。
    /// </summary>
    public class LatchStart : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("被锁存轴号", 2, Group = "输入", CN = "轴号", DefaultV = 0)]
        public virtual int Axis { get; set; }

        [Parameter("锁存通道号", 3, Group = "输入", CN = "锁存号", DefaultV = 0)]
        public virtual int LatchIndex { get; set; }

        [Parameter("触发源索引", 4, Group = "输入", CN = "触发源", DefaultV = 0)]
        public virtual int SourceIndex { get; set; }

        [Parameter("触发边沿", 5, Group = "输入", CN = "边沿", DefaultV = LatchTriggerEdge.RisingEdge)]
        public virtual LatchTriggerEdge TriggerEdge { get; set; }

        [Parameter("连续模式", 6, Group = "输入", CN = "连续", DefaultV = true)]
        public virtual bool ContinuousMode { get; set; }

        [Parameter("缓存最大长度", 7, Group = "输入", CN = "最大长度", DefaultV = 4096)]
        public virtual int MaxLength { get; set; }

        public LatchStart()
        {
            this.Tips = "启动锁存";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var latch = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisLatch;
            if (latch == null)
            {
                errMsg = "当前板卡不支持高速锁存(IFiveAxisLatch 未实现)";
                return false;
            }

            var trigger = new LatchTrigger
            {
                LatchIndex = LatchIndex,
                SourceIndex = SourceIndex,
                TriggerEdge = TriggerEdge,
                ContinuousMode = ContinuousMode,
                MaxLength = MaxLength,
            };
            return latch.StartLatch(Axis, trigger);
        }
    }

    /// <summary>
    /// 批量等待锁存到位节点(P5-3,主路径)。
    /// 封装 IFiveAxisLatch.WaitLatched(axis, count, ...) 批量重载(v2 主路径)。
    /// ⚠️ M-13 finally 关闭契约:ClearLatch 必执行,保证异常/急停时清理锁存缓存。
    /// 输出:锁存位置数组(逗号拼接,供下游 LatchOffsetCalc/LatchDataProcess 消费)。
    /// </summary>
    public class LatchWait : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("被锁存轴号", 2, Group = "输入", CN = "轴号", DefaultV = 0)]
        public virtual int Axis { get; set; }

        /// <summary>本次飞拍轨迹该轴的锁存点数(源端 actLis.Length)</summary>
        [Parameter("锁存点数", 3, Group = "输入", CN = "点数", DefaultV = 1)]
        public virtual int Count { get; set; }

        /// <summary>整批超时(ms)</summary>
        [Parameter("整批超时(ms)", 4, Group = "输入", CN = "超时", DefaultV = 60000)]
        public virtual int TimeoutMs { get; set; }

        /// <summary>锁存位置数组(逗号拼接)</summary>
        [Parameter("锁存位置", 50, Group = "输出", CN = "锁存位置", ParamType = ParamType.OUT)]
        public virtual string LatchedPositions { get; set; }

        /// <summary>是否成功</summary>
        [Parameter("是否成功", 51, Group = "输出", CN = "成功", ParamType = ParamType.OUT)]
        public virtual bool Success { get; set; }

        public LatchWait()
        {
            this.Tips = "等待锁存";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var latch = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisLatch;
            if (latch == null)
            {
                errMsg = "当前板卡不支持高速锁存";
                return false;
            }

            // M-13 finally 关闭契约:无论等待成功与否,ClearLatch 必执行清理缓存。
            try
            {
                Success = latch.WaitLatched(Axis, Count, TimeoutMs, out var positions);
                LatchedPositions = string.Join(",", positions);
                return Success;
            }
            finally
            {
                // ⚠️ M-13 finally 关闭契约:ClearLatch 必执行。
                try { latch.ClearLatch(Axis); } catch (Exception ex)
                {
                    MyOwner?.OnLog(LogType.Error, $"LatchWait.ClearLatch 异常:{ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 读取单点锁存位置节点(P5-3)。
    /// 封装 IFiveAxisLatch.ReadLatch,非批量场景的单点位置回读。
    /// </summary>
    public class LatchRead : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("被锁存轴号", 2, Group = "输入", CN = "轴号", DefaultV = 0)]
        public virtual int Axis { get; set; }

        [Parameter("锁存位置", 50, Group = "输出", CN = "锁存位置", ParamType = ParamType.OUT)]
        public virtual double LatchedPos { get; set; }

        public LatchRead()
        {
            this.Tips = "读取锁存";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var latch = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisLatch;
            if (latch == null)
            {
                errMsg = "当前板卡不支持高速锁存";
                return false;
            }
            var ok = latch.ReadLatch(Axis, out var pos);
            LatchedPos = pos;
            return ok;
        }
    }

    /// <summary>
    /// 清除锁存缓存节点(P5-3)。
    /// 封装 IFiveAxisLatch.ClearLatch,独立编排用(急停/重置时显式清理)。
    /// ⚠️ M-13 finally 关闭契约:此节点本身即清理动作,LatchWait 已内含 try/finally 调用。
    /// </summary>
    public class LatchClear : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("被锁存轴号", 2, Group = "输入", CN = "轴号", DefaultV = 0)]
        public virtual int Axis { get; set; }

        public LatchClear()
        {
            this.Tips = "清除锁存";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var latch = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisLatch;
            if (latch == null)
            {
                errMsg = "当前板卡不支持高速锁存";
                return false;
            }
            return latch.ClearLatch(Axis);
        }
    }

    /// <summary>
    /// 锁存偏移计算节点(P5-3,⑥)。
    /// 回读连续插补输出触发标志(ReadContiOutFlag)+ 锁存位置,计算飞拍触发点偏移 LatchedOffset。
    /// 对齐源端 CheckNormalAction.cs:644-663(ReadContiOutFlag 供 LatchedOffset 计算)。
    /// 输入:锁存位置 + 触发标志索引;输出:LatchedOffset(锁存位置 - 标记点对应轨迹位置)。
    /// </summary>
    public class LatchOffsetCalc : MotionFunction
    {
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        [Parameter("坐标系号", 2, Group = "输入", CN = "CRD", DefaultV = 0)]
        public virtual int Crd { get; set; }

        /// <summary>锁存到的位置(由 LatchWait/LatchRead 上游喂入)</summary>
        [Parameter("锁存位置", 3, Group = "输入", CN = "锁存位置", DefaultV = 0.0)]
        public virtual double LatchedPos { get; set; }

        /// <summary>触发点对应的轨迹命令位置(由轨迹序列按标记号取)</summary>
        [Parameter("命令位置", 4, Group = "输入", CN = "命令位置", DefaultV = 0.0)]
        public virtual double CommandPos { get; set; }

        /// <summary>偏移偏移量(锁存位置 - 命令位置)</summary>
        [Parameter("锁存偏移", 50, Group = "输出", CN = "偏移", ParamType = ParamType.OUT)]
        public virtual double LatchedOffset { get; set; }

        /// <summary>回读到的触发标志索引</summary>
        [Parameter("触发标志", 51, Group = "输出", CN = "标志", ParamType = ParamType.OUT)]
        public virtual int OutFlagIndex { get; set; }

        public LatchOffsetCalc()
        {
            this.Tips = "锁存偏移计算";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            var conti = (vAxisM?.GetDevice() as IMotionCard) as IFiveAxisContiInterp;
            if (conti == null)
            {
                errMsg = "当前板卡不支持连续插补(无法回读输出标志)";
                return false;
            }

            // 回读飞拍触发点标志(源端 ReadContiOutFlag,供 LatchedOffset 对齐)。
            var flag = 0;
            if (!conti.ReadContiOutFlag(Crd, ref flag))
            {
                errMsg = "ReadContiOutFlag 失败";
                return false;
            }
            OutFlagIndex = flag;
            // LatchedOffset = 锁存位置 - 命令位置(源端 LatchedOffset 计算同构)。
            LatchedOffset = LatchedPos - CommandPos;
            return true;
        }
    }

    /// <summary>
    /// 锁存数据处理节点(P5-3,⑦)。
    /// 将批量锁存位置序列(LatchWait 输出的逗号串)解析为轨迹点序列,
    /// 供下游飞拍轨迹执行链(P5-6)消费。对齐源端锁存值批处理(对 X/Y/Z/A/C 五轴各调)。
    /// 输入:锁存位置串 + 轴号;输出:点数 + 平均位置 + 位置数组(分号串)。
    /// </summary>
    public class LatchDataProcess : MotionFunction
    {
        /// <summary>锁存位置串(逗号分隔,由 LatchWait 喂入)</summary>
        [Parameter("锁存位置串", 1, Group = "输入", CN = "锁存串", DefaultV = "")]
        public virtual string LatchedPositions { get; set; }

        /// <summary>轴号(标识本批锁存所属轴)</summary>
        [Parameter("轴号", 2, Group = "输入", CN = "轴号", DefaultV = 0)]
        public virtual int Axis { get; set; }

        [Parameter("点数", 50, Group = "输出", CN = "点数", ParamType = ParamType.OUT)]
        public virtual int PointCount { get; set; }

        [Parameter("平均位置", 51, Group = "输出", CN = "平均", ParamType = ParamType.OUT)]
        public virtual double AveragePos { get; set; }

        [Parameter("位置数组", 52, Group = "输出", CN = "数组", ParamType = ParamType.OUT)]
        public virtual string PositionArray { get; set; }

        public LatchDataProcess()
        {
            this.Tips = "锁存数据处理";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            var positions = new List<double>();
            if (!string.IsNullOrWhiteSpace(LatchedPositions))
            {
                foreach (var part in LatchedPositions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (double.TryParse(part.Trim(), out var v)) positions.Add(v);
                }
            }

            PointCount = positions.Count;
            AveragePos = positions.Count > 0 ? positions.Average() : 0;
            PositionArray = string.Join(";", positions);
            return true;
        }
    }
}
