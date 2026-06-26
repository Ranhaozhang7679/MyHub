using System;
using System.Collections.Generic;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 五轴连续插补旁路接口(路 B,ADR v2 定稿)。
    /// 不扩展 <see cref="IMotionCard"/> 主契约,仅由具备卡侧连续插补能力的板卡(本项目为正运动 ZMotion 五轴适配器)实现。
    /// 与 <see cref="IFiveAxisRTCP"/> 同层、同范式:卡侧能力 → 旁路接口 → 仅五轴适配器实现 → 平台主干零改动(R1 非侵入)。
    /// </summary>
    /// <remarks>
    /// 源端对应:旧框架 <c>IoperateCrd</c>(OpenCrdConti/StartCrdConti/AddContiLine/CrdSetSmoothProfile/GetContiRemainSpace)
    /// + "连续插补-同步 IO" 8 方法(AddContiOutput/AddContiDelay/AddContiOutFlag/ReadContiOutFlag 等)。
    /// 实现契约:连续插补是飞拍轨迹执行主路径,<see cref="CrdContiStop"/>/<see cref="CrdContiClose"/> 必须由
    /// 节点级 try/finally 保证执行(P5-3 M-13 finally 关闭契约,验收工程师查 try/finally 结构)。
    /// </remarks>
    public interface IFiveAxisContiInterp
    {
        /// <summary>
        /// 打开连续插补模式(对应源端 OpenCrdConti,底层 ZAux_Direct_SetMerge=1)。
        /// </summary>
        /// <param name="crd">坐标系/插补器号(源端以轴号充当 crd)</param>
        /// <param name="axisList">参与插补的轴号列表</param>
        /// <param name="mode">插补模式</param>
        bool CrdContiOpen(int crd, int[] axisList, CrdMode mode);

        /// <summary>
        /// 启动连续插补(对应源端 StartCrdConti)。
        /// </summary>
        bool CrdContiStart(int crd);

        /// <summary>
        /// 追加一段直线插补(对应源端 AddContiLine,底层 ZAux_Direct_MoveAbsSp/MoveSp + SetMovemark)。
        /// </summary>
        /// <param name="crd">坐标系号</param>
        /// <param name="endPos">各轴终点位置(顺序与 Open 时 axisList 对齐)</param>
        /// <param name="mode">点位模式(绝对/相对)</param>
        bool CrdContiAddLine(int crd, double[] endPos, ContiMoveMode mode);

        /// <summary>
        /// 追加一段延时(对应源端 AddContiDelay,底层 MoveDelay + SetMovemark)。
        /// </summary>
        /// <param name="delayMs">延时毫秒</param>
        /// <param name="markIndex">点位标记号(供 ReadContiOutFlag 回读对齐)</param>
        bool CrdContiAddDelay(int crd, int delayMs, int markIndex);

        /// <summary>
        /// 追加同步输出(对应源端 AddContiOutput/AddContiOutFlag,底层 MoveOp/MoveTable)。
        /// 在插补轨迹的指定标记点输出 IO 电平(飞拍触发)。
        /// </summary>
        /// <param name="ioIndex">输出位号</param>
        /// <param name="level">输出电平</param>
        /// <param name="markIndex">轨迹标记号</param>
        bool CrdContiAddOutput(int crd, int ioIndex, bool level, int markIndex);

        /// <summary>
        /// 回读比较输出触发标志(对应源端 ReadContiOutFlag,底层 GetTable)。
        /// 用于飞拍触发点回读,供锁存偏移 LatchedOffset 计算。
        /// </summary>
        /// <param name="crd">坐标系号</param>
        /// <param name="index">[in] 期望标记号 / [out] 实际回读到的标记号</param>
        bool ReadContiOutFlag(int crd, ref int index);

        /// <summary>
        /// 查询插补器剩余缓冲空间(对应源端 GetContiRemainSpace,底层 GetRemain_Buffer)。
        /// 背压检查:剩余不足时节点应 Wait/Polling,不可丢弃点位(节点实现硬性契约)。
        /// </summary>
        bool GetContiRemainSpace(int crd, out int space);

        /// <summary>
        /// 等待连续插补完成(对应源端插补完成轮询,底层 GetIfIdle)。
        /// </summary>
        bool WaitCrdDone(int crd, int timeoutMs);

        /// <summary>
        /// 停止连续插补(对应源端 StopCrdConti,底层 Single_Cancel)。
        /// ⚠️ 必须在节点级 try/finally 中调用,保证异常/急停时执行(M-13 finally 契约)。
        /// </summary>
        bool CrdContiStop(int crd);

        /// <summary>
        /// 关闭连续插补模式(对应源端 CloseCrdConti,底层 SetMerge=0)。
        /// ⚠️ 必须在节点级 try/finally 中调用,保证执行(M-13 finally 契约)。
        /// </summary>
        bool CrdContiClose(int crd);

        /// <summary>
        /// 配置速度前瞻/平滑参数(对应源端 CrdSetSmoothProfile,底层 SetCornerMode/SetZsmooth/SetDecelAngle/SetStopAngle)。
        /// </summary>
        bool SetSmoothProfile(int crd, SmoothProfile profile);
    }

    /// <summary>
    /// 五轴高速锁存旁路接口(路 B,ADR v2 定稿)。
    /// 不扩展 <see cref="IMotionCard"/> 主契约,仅由具备卡侧高速位置锁存能力的板卡实现。
    /// 源端对应旧框架 <c>IoperateHighLatcher</c>(位置触发锁存,飞拍拍照位置捕获)。
    /// </summary>
    /// <remarks>
    /// 实现契约:<see cref="ClearLatch"/> 必须由节点级 try/finally 保证执行(P5-3 M-13 finally 关闭契约)。
    /// </remarks>
    public interface IFiveAxisLatch
    {
        /// <summary>
        /// 启动高速锁存(对应源端 ResetHighLatcher,底层 REGIST 指令配置触发源/边沿/缓存)。
        /// </summary>
        /// <param name="axis">被锁存轴号</param>
        /// <param name="trigger">触发配置</param>
        bool StartLatch(int axis, LatchTrigger trigger);

        /// <summary>
        /// 批量等待锁存到位(v2 主路径)。源端 <c>WaitLatched(axis, count, out value)</c>
        /// 对 X/Y/Z/A/C 五轴各调、每次等 N 个锁存点,是飞拍锁存主路径。
        /// </summary>
        /// <param name="axis">被锁存轴号</param>
        /// <param name="count">本次飞拍轨迹该轴的锁存点数(源端 actLis.Length)</param>
        /// <param name="timeoutMs">整批超时(与源端 RunAction 循环对齐,非单点超时)</param>
        /// <param name="latchedPos">[out] 锁存到的位置数组,长度 = count</param>
        bool WaitLatched(int axis, int count, int timeoutMs, out double[] latchedPos);

        /// <summary>
        /// 单值便利重载(内部转调批量 count=1)。
        /// </summary>
        bool WaitLatched(int axis, int timeoutMs, out double latchedPos);

        /// <summary>
        /// 读取单点锁存位置(对应源端 GetHighLatchedValue count=1)。
        /// </summary>
        bool ReadLatch(int axis, out double latchedPos);

        /// <summary>
        /// 清除锁存缓存(对应源端 ResetHighLatcher 重置)。
        /// ⚠️ 必须在节点级 try/finally 中调用,保证执行(M-13 finally 契约)。
        /// </summary>
        bool ClearLatch(int axis);
    }

    /// <summary>
    /// 连续插补模式(对齐源端 absolute/relative 切换)。
    /// </summary>
    public enum CrdMode
    {
        /// <summary>绝对插补(底层 MoveAbsSp)</summary>
        Absolute,

        /// <summary>相对插补(底层 MoveSp)</summary>
        Relative,
    }

    /// <summary>
    /// 连续插补点位运动模式。
    /// </summary>
    public enum ContiMoveMode
    {
        /// <summary>绝对点位(底层 MoveAbsSp)</summary>
        Absolute,

        /// <summary>相对点位(底层 MoveSp)</summary>
        Relative,
    }

    /// <summary>
    /// 高速锁存触发配置。
    /// </summary>
    [Serializable]
    public class LatchTrigger
    {
        /// <summary>锁存通道号(对应源端 latchIndex,卡端锁存资源编号)</summary>
        public int LatchIndex { get; set; }

        /// <summary>触发源轴/编码器索引(对应源端 SourceIndex)</summary>
        public int SourceIndex { get; set; }

        /// <summary>触发边沿</summary>
        public LatchTriggerEdge TriggerEdge { get; set; }

        /// <summary>是否连续模式(对应源端 ContiMode:连续锁存 vs 单次锁存)</summary>
        public bool ContinuousMode { get; set; }

        /// <summary>锁存缓存最大长度(对应源端 MaxLength)</summary>
        public int MaxLength { get; set; } = 4096;
    }

    /// <summary>
    /// 锁存触发边沿(对齐源端 TriggerEdgeCode)。
    /// </summary>
    public enum LatchTriggerEdge
    {
        /// <summary>上升沿触发(源端 mode+3)</summary>
        RisingEdge,

        /// <summary>下降沿触发(源端 mode+4)</summary>
        FallingEdge,
    }

    /// <summary>
    /// 速度前瞻/平滑参数(对应源端 CrdSmoothProfile / AxisProperty.SmoothSetting)。
    /// </summary>
    [Serializable]
    public class SmoothProfile
    {
        /// <summary>拐角模式(底层 SetCornerMode)</summary>
        public int CornerMode { get; set; }

        /// <summary>拐角平滑半径(底层 SetZsmooth)</summary>
        public double CornerRadius { get; set; }

        /// <summary>减速角度阈值(度,底层 SetDecelAngle,内部转弧度)</summary>
        public double DecelAngle { get; set; }

        /// <summary>停止角度阈值(度,底层 SetStopAngle,内部转弧度)</summary>
        public double StopAngle { get; set; }
    }
}
