#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30308.42000
* 类 名 称:       IHandoverAutoSignalService
* 命名空间:       Luster.Module.Motion.Handover.Services
* 文 件 名:       IHandoverAutoSignalService.cs
* 创建时间:       2026/06/23
* 作    者:       全栈工程师
* 所属部门：      系统集成部
* 版  权:    	  <copyright company="凌云光工业">
* 签  名:         Luster Technology Co.,Ltd.
************************************************************************************/
#endregion

using System;

namespace Luster.Module.Motion.Handover.Services
{
    /// <summary>
    /// 自动运行命令类型（ADR-TES-37 D4 / §3.4）。
    /// <para>由 16 位 <c>AutoReadSignal</c> 字的命令位边沿映射而来，对应 <c>MotionController</c> 命令入口。</para>
    /// <para>映射关系：<see cref="Start"/>→<c>MotionController.Start()</c>、<see cref="Stop"/>→<c>Stop()</c>、
    /// <see cref="Pause"/>→<c>Pause()</c>、<see cref="Reset"/>→<c>Recovery()</c>、<see cref="Init"/>→<c>Home()</c>。</para>
    /// <para><b>Auto/AllowInit 位</b>是模式标志（非命令），不纳入本枚举，不触发命令派发。</para>
    /// </summary>
    public enum AutoCommandType
    {
        /// <summary>启动（源端 <c>AutoReadSignal.START</c>=0 → MotionController.Start()）</summary>
        Start = 0,

        /// <summary>停止（源端 <c>AutoReadSignal.STOP</c>=1 → MotionController.Stop()）</summary>
        Stop = 1,

        /// <summary>暂停（源端 <c>AutoReadSignal.PAUSE</c>=2 → MotionController.Pause()）</summary>
        Pause = 2,

        /// <summary>复位（源端 <c>AutoReadSignal.RESET</c>=3 → MotionController.Recovery()）</summary>
        Reset = 3,

        /// <summary>初始化（源端 <c>AutoReadSignal.INIT</c>=4 → MotionController.Home()）</summary>
        Init = 4,
    }

    /// <summary>
    /// 自动信号命令边沿事件参数（ADR-TES-37 §3.4）。
    /// </summary>
    public class AutoSignalEventArgs : EventArgs
    {
        /// <summary>触发的命令类型</summary>
        public AutoCommandType Command { get; }

        /// <summary>true=上升沿触发（PLC 写入置位），false=下降沿（PLC 清零）</summary>
        public bool IsRisingEdge { get; }

        /// <summary>边沿发生时间戳</summary>
        public DateTime Timestamp { get; }

        /// <summary>构造自动信号命令边沿事件参数</summary>
        /// <param name="command">命令类型</param>
        /// <param name="isRisingEdge">是否上升沿</param>
        /// <param name="timestamp">时间戳</param>
        public AutoSignalEventArgs(AutoCommandType command, bool isRisingEdge, DateTime timestamp)
        {
            Command = command;
            IsRisingEdge = isRisingEdge;
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Modbus 自动信号字（16 位 <c>AutoReadSignal</c>）后台边沿监听服务（ADR-TES-37 D4 / §3.4）。
    /// <para>风格对齐 P7-A <c>IIOTriggerService</c>（后台轮询 + 边沿检测 + 事件广播），但数据源=Modbus 寄存器字（非 VIO），
    /// 消费方=<c>MotionController</c> 命令派发。两者数据源/消费方不同，故不合并（ADR D4）。</para>
    /// <para><b>生命周期 owner = DeviceEngine bootstrap</b>（对齐 P7-A ADR §10 补遗），是平台基础设施，
    /// 非 <c>HandoverNode</c>、非 <c>MotionController</c>。注册点挂 <c>DeviceEngine.Initialize</c> 完成、
    /// <c>VModbusServer</c> 就绪之后；禁止由某个 <c>HandoverNode</c> 持有生命周期。</para>
    /// <para><b>信号源注入</b>：构造时注入 <c>readAutoSignal</c> 寄存器字读取委托（对齐 <c>IOTriggerService</c> 的
    /// <c>statusReader</c> 注入风格），生产环境由 DeviceEngine bootstrap 解析 <c>VModbusServer</c> 后注入读取委托，
    /// 单测可注入受控源绕开 Engine 依赖。</para>
    /// </summary>
    public interface IHandoverAutoSignalService
    {
        /// <summary>配置自动信号字所在的 <c>VModbusServer</c> 名称 + 寄存器地址。
        /// 生产环境由 DeviceEngine bootstrap 据此解析 <c>VModbusServer</c> 并构造读取委托注入。</summary>
        /// <param name="vModbusServerName">VModbusServer 虚拟设备名称</param>
        /// <param name="autoSignalAddress">自动信号字寄存器地址</param>
        void Configure(string vModbusServerName, int autoSignalAddress);

        /// <summary>启动后台轮询线程（典型 20ms 周期，可配置）</summary>
        void Start();

        /// <summary>停止后台轮询线程</summary>
        void Stop();

        /// <summary>命令边沿事件：仅 Start/Stop/Pause/Reset/Init 5 个命令位的电平翻转会通过此事件广播。
        /// 由 <c>AutoCommandDispatcher</c> 订阅后派发到 <c>MotionController</c>。</summary>
        event EventHandler<AutoSignalEventArgs> OnAutoCommand;
    }
}
