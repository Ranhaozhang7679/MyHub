#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30308.42000
* 类 名 称:       HandoverAutoSignalService
* 命名空间:       Luster.Module.Motion.Handover.Services
* 文 件 名:       HandoverAutoSignalService.cs
* 创建时间:       2026/06/23
* 作    者:       全栈工程师
* 所属部门：      系统集成部
* 版  权:    	  <copyright company="凌云光工业">
* 签  名:         Luster Technology Co.,Ltd.
************************************************************************************/
#endregion

using Luster.Module.Motion.Handover.Signals;
using System;
using System.Threading;

namespace Luster.Module.Motion.Handover.Services
{
    /// <summary>
    /// Modbus 自动信号字后台边沿监听服务实现（ADR-TES-37 D4 / §3.4）。
    /// <para>后台线程周期读取 16 位 <c>AutoReadSignal</c> 字，对 Start/Stop/Pause/Reset/Init 5 个命令位做边沿检测，
    /// 电平翻转时广播 <see cref="OnAutoCommand"/>（含上升沿/下降沿标志）。</para>
    /// <para>扫描逻辑与后台线程解耦：<see cref="ScanOnce"/> 同步执行一次扫描，便于单测确定性驱动，
    /// 无需依赖线程时序；<see cref="Start"/>/<see cref="Stop"/> 仅负责后台生命周期（对齐 P7-A <c>IOTriggerService</c> 风格）。</para>
    /// </summary>
    /// <remarks>
    /// <b>边沿语义</b>：首帧仅记录初值不判边沿（避免启动瞬间把 PLC 已置位的命令位误判为上升沿）；
    /// 之后每位电平翻转即广播一次事件。命令派发的"仅上升沿触发"由 <c>AutoCommandDispatcher</c> 决定，
    /// 本服务如实广播上升/下降沿供下游消费。
    /// <para><b>信号源</b>：构造注入 <c>readAutoSignal</c> 读取委托。生产环境由 DeviceEngine bootstrap 解析
    /// <c>VModbusServer</c>（按 <see cref="Configure"/> 配置的名称 + 地址）后注入；单测注入受控源。</para>
    /// <para><b>生命周期 owner</b>：本服务为平台基础设施，由 DeviceEngine bootstrap 持有，不挂在 <c>HandoverNode</c>。</para>
    /// </remarks>
    public class HandoverAutoSignalService : IHandoverAutoSignalService
    {
        /// <summary>命令位 → 命令类型映射表（仅 5 个命令位，Auto/AllowInit 为模式标志不入表）</summary>
        private static readonly int[] _commandBits =
        {
            AutoSignalBit.Start,
            AutoSignalBit.Stop,
            AutoSignalBit.Pause,
            AutoSignalBit.Reset,
            AutoSignalBit.Init,
        };

        private readonly Func<ushort> _readAutoSignal;
        private readonly int _scanIntervalMs;
        private readonly Func<DateTime> _clock;

        private string? _vModbusServerName;
        private int _autoSignalAddress;

        private ushort _previous;
        private bool _initialized;

        private Thread? _thread;
        private volatile bool _running;

        /// <summary>
        /// 构造自动信号边沿监听服务
        /// </summary>
        /// <param name="readAutoSignal">16 位 <c>AutoReadSignal</c> 字读取委托。
        /// 生产环境：由 DeviceEngine bootstrap 解析 <c>VModbusServer</c> 后注入（按 <see cref="Configure"/> 配置读寄存器）；
        /// 单测：注入受控源绕开 Engine 依赖。</param>
        /// <param name="scanIntervalMs">后台扫描周期（ms），默认 20ms（源端经验值，对齐 <c>IOTriggerService</c>）</param>
        /// <param name="clock">时间源委托，默认 <c>DateTime.Now</c>；单测可注入受控时钟</param>
        public HandoverAutoSignalService(
            Func<ushort> readAutoSignal,
            int scanIntervalMs = 20,
            Func<DateTime>? clock = null)
        {
            _readAutoSignal = readAutoSignal ?? throw new ArgumentNullException(nameof(readAutoSignal));
            _scanIntervalMs = scanIntervalMs > 0 ? scanIntervalMs : 20;
            _clock = clock ?? (() => DateTime.Now);
        }

        /// <inheritdoc/>
        public event EventHandler<AutoSignalEventArgs>? OnAutoCommand;

        /// <summary>当前配置的 VModbusServer 名称（生产 wiring 读取寄存器时用，诊断/单测可查阅）</summary>
        public string? VModbusServerName => _vModbusServerName;

        /// <summary>当前配置的自动信号字寄存器地址</summary>
        public int AutoSignalAddress => _autoSignalAddress;

        /// <inheritdoc/>
        public void Configure(string vModbusServerName, int autoSignalAddress)
        {
            _vModbusServerName = vModbusServerName;
            _autoSignalAddress = autoSignalAddress;
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            _thread = new Thread(ScanLoop)
            {
                IsBackground = true,
                Name = "HandoverAutoSignalService",
            };
            _thread.Start();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            _running = false;
            _thread?.Join(2000);
            _thread = null;
        }

        /// <summary>后台扫描循环</summary>
        private void ScanLoop()
        {
            while (_running)
            {
                ScanOnce();
                Thread.Sleep(_scanIntervalMs);
            }
        }

        /// <summary>
        /// 执行一次扫描：读取 16 位自动信号字，对 5 个命令位做边沿检测并广播 <see cref="OnAutoCommand"/>。
        /// 暴露为 internal 供单测同步驱动，避免依赖后台线程时序。
        /// </summary>
        internal void ScanOnce()
        {
            ushort current;
            try
            {
                current = _readAutoSignal();
            }
            catch
            {
                // 读取异常（如 VModbusServer 未就绪）不更新上一次状态，避免误判边沿；下一帧重试。
                return;
            }

            // 首帧仅记录初值，不判边沿（避免启动瞬间把 PLC 已置位的命令位误判为上升沿）
            if (!_initialized)
            {
                _previous = current;
                _initialized = true;
                return;
            }

            // 仅处理命令位（Auto/AllowInit 为模式标志，不触发命令派发）
            foreach (var bit in _commandBits)
            {
                ushort mask = AutoSignalBit.Mask(bit);
                bool prev = (_previous & mask) != 0;
                bool now = (current & mask) != 0;
                if (now != prev)
                {
                    bool isRisingEdge = now && !prev;
                    OnAutoCommand?.Invoke(this, new AutoSignalEventArgs(
                        BitToCommand(bit), isRisingEdge, _clock()));
                }
            }

            _previous = current;
        }

        /// <summary>命令位偏移 → 命令类型（与 <c>AutoSignalBit</c> / <c>AutoCommandType</c> 偏移一致）</summary>
        private static AutoCommandType BitToCommand(int bit)
        {
            // AutoCommandType 的整数值与 AutoSignalBit.Start/Stop/Pause/Reset/Init 偏移对齐
            return (AutoCommandType)bit;
        }
    }
}
