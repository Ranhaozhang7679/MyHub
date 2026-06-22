using System;

namespace Luster.Module.Motion.Handover.Signals
{
    /// <summary>
    /// 16 位自动运行信号位定义（独立 Modbus 字，与 32 位交握字不同）。
    /// 忠实迁移自源端 SP-2025140 <c>ModbusAutoSignal.cs</c> 的 <c>AutoReadSignal</c> 枚举偏移：
    /// PLC 写入本字触发站级命令。位含义见各常量注释。
    /// </summary>
    /// <remarks>
    /// <b>用途边界（ADR-TES-37 D4）</b>：本类作位含义字典，供 TES-37-6
    /// <c>IHandoverAutoSignalService</c> 边沿监听 + <c>AutoCommandDispatcher</c> 命令派发查阅。
    /// 命令映射：Start→<c>MotionController.Start()</c>、Stop→<c>Stop()</c>、
    /// Pause→<c>Pause()</c>、Reset→<c>Recovery()</c>、Init→<c>Home()</c>。
    /// 实际读写仍走字符串地址，本类非状态机数据载体。
    /// </remarks>
    public static class AutoSignalBit
    {
        /// <summary>启动（源端 <c>AutoReadSignal.START</c> = 0 → MotionController.Start()）</summary>
        public const int Start = 0;
        /// <summary>停止（源端 <c>AutoReadSignal.STOP</c> = 1 → MotionController.Stop()）</summary>
        public const int Stop = 1;
        /// <summary>暂停（源端 <c>AutoReadSignal.PAUSE</c> = 2 → MotionController.Pause()）</summary>
        public const int Pause = 2;
        /// <summary>复位（源端 <c>AutoReadSignal.RESET</c> = 3 → MotionController.Recovery()）</summary>
        public const int Reset = 3;
        /// <summary>初始化（源端 <c>AutoReadSignal.INIT</c> = 4 → MotionController.Home()）</summary>
        public const int Init = 4;
        /// <summary>自动模式（源端 <c>AutoReadSignal.AUTO</c> = 5）</summary>
        public const int Auto = 5;
        /// <summary>允许初始化（源端 <c>AutoReadSignal.AllowInit</c> = 6）</summary>
        public const int AllowInit = 6;

        // ===== bit7-15：预留（源端 <c>Reseve1</c>..<c>Reseve9</c> = 7-15） =====
        /// <summary>预留位 1（源端 <c>Reseve1</c> = 7）</summary>
        public const int Reserve1 = 7;
        /// <summary>预留位 2（源端 <c>Reseve2</c> = 8）</summary>
        public const int Reserve2 = 8;
        /// <summary>预留位 3（源端 <c>Reseve3</c> = 9）</summary>
        public const int Reserve3 = 9;
        /// <summary>预留位 4（源端 <c>Reseve4</c> = 10）</summary>
        public const int Reserve4 = 10;
        /// <summary>预留位 5（源端 <c>Reseve5</c> = 11）</summary>
        public const int Reserve5 = 11;
        /// <summary>预留位 6（源端 <c>Reseve6</c> = 12）</summary>
        public const int Reserve6 = 12;
        /// <summary>预留位 7（源端 <c>Reseve7</c> = 13）</summary>
        public const int Reserve7 = 13;
        /// <summary>预留位 8（源端 <c>Reseve8</c> = 14）</summary>
        public const int Reserve8 = 14;
        /// <summary>预留位 9（源端 <c>Reseve9</c> = 15）</summary>
        public const int Reserve9 = 15;

        /// <summary>信号位总数（16 位字）</summary>
        public const int BitCount = 16;

        /// <summary>构造指定位的掩码（1 &lt;&lt; bit）。供诊断/单测用。</summary>
        public static ushort Mask(int bit) => (ushort)(1 << bit);
    }

    /// <summary>
    /// 16 位自动运行回送信号位定义（本站发给 PLC 的状态字，与 <see cref="AutoSignalBit"/> 方向相反）。
    /// 忠实迁移自源端 <c>ModbusAutoSignal.cs</c> 的 <c>AutoSendSignal</c> 枚举偏移：
    /// bit0 Light；bit1-8 预留；bit9 NotReady；bit10 Initing；bit11 Worning；
    /// bit12 Error；bit13 Ready；bit14 Running；bit15 Stop。
    /// </summary>
    /// <remarks>
    /// 本类作位含义字典，供状态回送查阅。实际读写仍走字符串地址。
    /// </remarks>
    public static class AutoSendSignalBit
    {
        /// <summary>光源（源端 <c>AutoSendSignal.LIGHT</c> = 0）</summary>
        public const int Light = 0;
        /// <summary>预留位 1（源端 <c>Reseve1</c> = 1）</summary>
        public const int Reserve1 = 1;
        /// <summary>预留位 2（源端 <c>Reseve2</c> = 2）</summary>
        public const int Reserve2 = 2;
        /// <summary>预留位 3（源端 <c>Reseve3</c> = 3）</summary>
        public const int Reserve3 = 3;
        /// <summary>预留位 4（源端 <c>Reseve4</c> = 4）</summary>
        public const int Reserve4 = 4;
        /// <summary>预留位 5（源端 <c>Reseve5</c> = 5）</summary>
        public const int Reserve5 = 5;
        /// <summary>预留位 6（源端 <c>Reseve6</c> = 6）</summary>
        public const int Reserve6 = 6;
        /// <summary>预留位 7（源端 <c>Reseve7</c> = 7）</summary>
        public const int Reserve7 = 7;
        /// <summary>预留位 8（源端 <c>Reseve8</c> = 8）</summary>
        public const int Reserve8 = 8;
        /// <summary>未就绪（源端 <c>AutoSendSignal.NotReady</c> = 9）</summary>
        public const int NotReady = 9;
        /// <summary>初始化中（源端 <c>AutoSendSignal.Initing</c> = 10）</summary>
        public const int Initing = 10;
        /// <summary>告警（源端 <c>AutoSendSignal.Worning</c> = 11）</summary>
        public const int Worning = 11;
        /// <summary>错误（源端 <c>AutoSendSignal.Error</c> = 12）</summary>
        public const int Error = 12;
        /// <summary>就绪（源端 <c>AutoSendSignal.Ready</c> = 13）</summary>
        public const int Ready = 13;
        /// <summary>运行中（源端 <c>AutoSendSignal.Running</c> = 14）</summary>
        public const int Running = 14;
        /// <summary>停止（源端 <c>AutoSendSignal.Stop</c> = 15）</summary>
        public const int Stop = 15;

        /// <summary>信号位总数（16 位字）</summary>
        public const int BitCount = 16;

        /// <summary>构造指定位的掩码（1 &lt;&lt; bit）。供诊断/单测用。</summary>
        public static ushort Mask(int bit) => (ushort)(1 << bit);
    }
}
