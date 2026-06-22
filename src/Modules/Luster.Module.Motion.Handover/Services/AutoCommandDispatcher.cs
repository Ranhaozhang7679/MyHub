#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30308.42000
* 类 名 称:       AutoCommandDispatcher
* 命名空间:       Luster.Module.Motion.Handover.Services
* 文 件 名:       AutoCommandDispatcher.cs
* 创建时间:       2026/06/23
* 作    者:       全栈工程师
* 所属部门：      系统集成部
* 版  权:    	  <copyright company="凌云光工业">
* 签  名:         Luster Technology Co.,Ltd.
************************************************************************************/
#endregion

using Luster.Motion.TaskFlow.Engine;
using System;

namespace Luster.Module.Motion.Handover.Services
{
    /// <summary>
    /// 自动命令派发器（ADR-TES-37 D4 / §3.5）：订阅 <see cref="IHandoverAutoSignalService.OnAutoCommand"/>
    /// → 调 <see cref="IMotionController"/> 命令。
    /// <para>与 <see cref="IHandoverAutoSignalService"/> 同模块，但职责分离（边沿监听 vs 命令派发）。</para>
    /// <para>映射：<see cref="AutoCommandType.Start"/>→<c>Start()</c>、<see cref="AutoCommandType.Stop"/>→<c>Stop()</c>、
    /// <see cref="AutoCommandType.Pause"/>→<c>Pause()</c>、<see cref="AutoCommandType.Reset"/>→<c>Recovery()</c>、
    /// <see cref="AutoCommandType.Init"/>→<c>Home()</c>。</para>
    /// </summary>
    /// <remarks>
    /// <b>边沿触发</b>：仅上升沿（PLC 写入置位）触发命令派发；下降沿（PLC 清零）忽略——对齐源端 PLC 写入触发命令的语义。
    /// <b>互斥/去抖</b>：源端 <c>ModbusAutoSignal</c> 仅做位解码、无命令消费者（全栈已核实 3 站 0 引用），
    /// 故无可对齐的互斥行为；命令幂等性由 <c>MotionController</c> 既有状态机保证（如 <c>Start()</c> 返回 bool，
    /// 运行中再 Start 由控制器自身判定），本派发器不重复实现互斥、不侵入 <c>MotionController</c> 既有契约（ADR D4 只新增）。
    /// <para><b>依赖注入</b>：构造注入 <see cref="IMotionController"/>（接口，对齐模块内 <c>Ioc.Resolve&lt;IMotionController&gt;()</c>
    /// 用法，且便于单测 Moq），不直接依赖 <c>MotionController</c> 具体类。</para>
    /// </remarks>
    public sealed class AutoCommandDispatcher : IDisposable
    {
        private readonly IHandoverAutoSignalService _service;
        private readonly IMotionController _controller;
        private bool _disposed;

        /// <summary>
        /// 构造自动命令派发器
        /// </summary>
        /// <param name="service">自动信号边沿监听服务（命令事件源）</param>
        /// <param name="controller">运动控制器命令入口（派发目标）</param>
        public AutoCommandDispatcher(IHandoverAutoSignalService service, IMotionController controller)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _service.OnAutoCommand += OnAutoCommand;
        }

        /// <summary>自动命令事件处理：仅上升沿派发到对应 <c>MotionController</c> 命令</summary>
        private void OnAutoCommand(object? sender, AutoSignalEventArgs e)
        {
            // 仅上升沿触发命令（PLC 写入置位语义）；下降沿忽略
            if (!e.IsRisingEdge)
            {
                return;
            }

            switch (e.Command)
            {
                case AutoCommandType.Start:
                    _controller.Start();
                    break;
                case AutoCommandType.Stop:
                    _controller.Stop();
                    break;
                case AutoCommandType.Pause:
                    _controller.Pause();
                    break;
                case AutoCommandType.Reset:
                    _controller.Recovery();
                    break;
                case AutoCommandType.Init:
                    _controller.Home();
                    break;
                default:
                    // 未知命令类型不派发（防御性，枚举扩展时显式处理）
                    break;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _service.OnAutoCommand -= OnAutoCommand;
            _disposed = true;
        }
    }
}
