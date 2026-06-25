#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverSignalBit
* 文 件 名:       HandoverSignalBit.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 交握信号位偏移定义标准
************************************************************************************/
#endregion

namespace Luster.Module.Motion.Handover.Signals
{
    /// <summary>
    /// 32 位交握信号位偏移定义(字典,非状态机数据载体)。
    /// <para>迁自源端 SP-2025140 <c>Plugin.CommonPlugin.Model.Args.LocalSend</c> /
    /// <c>LocalReceive</c> 枚举(<c>Model\Args\ModbusSignal.cs:587-660</c>),
    /// 位偏移按源端真实枚举值保留(可还原)。ADR-TES-37 决策 D3 已登记修正点:
    /// 源端 <c>SendSignal32.SendProduct3OK</c>/<c>SendProduct4OK</c> 3-4 互换复制粘贴 bug
    /// (<c>ModbusSignal.cs:371-372</c>),本字典已修正。</para>
    /// <para>ADR-TES-37 决策 D3:本类仅供位含义查阅与诊断,<b>不是活的状态机数据载体</b>;
    /// 实际读写走 <see cref="HandoverSignalAddress"/> 的字符串地址(对齐源端运行路径)。</para>
    /// </summary>
    public static class HandoverSignalBit
    {
        // —— 产品在籍位 0-3(对应源端 S_ProductExist_1..4 / R_ProductExist_1..4)
        /// <summary>产品在籍 1(位偏移 0)</summary>
        public const int ProductExist1 = 0;
        /// <summary>产品在籍 2(位偏移 1)</summary>
        public const int ProductExist2 = 1;
        /// <summary>产品在籍 3(位偏移 2)</summary>
        public const int ProductExist3 = 2;
        /// <summary>产品在籍 4(位偏移 3)</summary>
        public const int ProductExist4 = 3;

        // —— 产品结果位 4-15(对应源端 S_Product_x_OK / NG1 / NG2)
        /// <summary>产品 1 OK(位偏移 4)</summary>
        public const int Product1OK = 4;
        /// <summary>产品 2 OK(位偏移 7)</summary>
        public const int Product2OK = 7;
        /// <summary>产品 3 OK(位偏移 10;修正源端 :371-372 与 Product4OK 互换的 bug)</summary>
        public const int Product3OK = 10;
        /// <summary>产品 4 OK(位偏移 13;修正源端 :371-372 与 Product3OK 互换的 bug)</summary>
        public const int Product4OK = 13;

        // —— 握手核心位 16-20(对应源端 S_ReadyOK / S_Sending / S_Transfer / S_Interlock / S_HreartBeat)
        /// <summary>就绪(位偏移 16,对应源端 S_ReadyOK / R_ReadyOK)</summary>
        public const int Ready = 16;
        /// <summary>发送/接收中(位偏移 17,对应源端 S_Sending / R_Receiving)</summary>
        public const int Sending = 17;
        /// <summary>传输完成(位偏移 18,对应源端 S_Transfer / R_Transfer)</summary>
        public const int Transfer = 18;
        /// <summary>互锁(位偏移 19,对应源端 S_Interlock / R_Interlock)</summary>
        public const int InterLock = 19;
        /// <summary>心跳(位偏移 20,2 倍读周期翻转;对应源端 S_HreartBeat / R_HreartBeat)</summary>
        public const int Heartbeat = 20;

        // —— 门锁位 31(对应源端 S_DoorLock / R_DoorLock)
        /// <summary>安全门锁(位偏移 31,对应源端 S_DoorLock / R_DoorLock)</summary>
        public const int DoorLock = 31;
    }

    /// <summary>
    /// 16 位自动运行信号位(独立字,PLC 写入触发命令)。
    /// <para>迁自源端 <c>ModbusAutoSignal.AutoReadSignal</c> 的位定义
    /// (Start/Stop/Pause/Reset/Init/Auto/AllowInit)。</para>
    /// <para>ADR-TES-37 决策 D4:本位定义供 <c>IHandoverAutoSignalService</c>
    /// (TES-37-6)边沿监听使用,命令映射 → MotionController.Start/Stop/Pause/Recovery/Home。
    /// 本 Issue(TES-37-3)仅落位定义标准,不实现服务。</para>
    /// </summary>
    public static class AutoSignalBit
    {
        /// <summary>启动(位偏移 0)→ MotionController.Start()</summary>
        public const int Start = 0;
        /// <summary>停止(位偏移 1)→ MotionController.Stop()</summary>
        public const int Stop = 1;
        /// <summary>暂停(位偏移 2)→ MotionController.Pause()</summary>
        public const int Pause = 2;
        /// <summary>复位(位偏移 3)→ MotionController.Recovery()</summary>
        public const int Reset = 3;
        /// <summary>初始化/回零(位偏移 4)→ MotionController.Home()</summary>
        public const int Init = 4;
        /// <summary>自动模式(位偏移 5)</summary>
        public const int Auto = 5;
        /// <summary>允许初始化(位偏移 6)</summary>
        public const int AllowInit = 6;

        /// <summary>
        /// 位偏移 → 16 位掩码(TES-55 补:供 HandoverAutoSignalService 边沿监听按位与判断)。
        /// <para>AutoReadSignal 是 16 位 ushort,故返回 ushort;与 0350a14e 那条线
        /// <c>HandoverSignalBit.Mask(int) => 1u &lt;&lt; bit</c>(返回 uint)同款,仅返回类型对齐 16 位信号字。</para>
        /// </summary>
        /// <param name="bit">位偏移(0-6)</param>
        /// <returns>该位的 16 位掩码</returns>
        public static ushort Mask(int bit) => (ushort)(1 << bit);
    }
}
