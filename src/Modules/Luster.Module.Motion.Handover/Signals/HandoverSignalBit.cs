using System;

namespace Luster.Module.Motion.Handover.Signals
{
    /// <summary>
    /// 32 位交握信号位偏移定义（字典标准，非状态机数据载体）。
    /// 忠实迁移自源端 SP-2025140 <c>Plugin.CommonPlugin\Model\Args\ModbusSignal.cs</c> 的
    /// <c>LocalSend</c> / <c>LocalReceive</c> 枚举偏移，发送/接收两侧位布局完全对称：
    /// bit0-3 = 4 产品在籍 Exist；bit4-15 = 4 产品 × {OK, NG1, NG2}（每产品 3 位）；
    /// bit16 Ready；bit17 Sending/Receiving；bit18 Transfer；bit19 Interlock；
    /// bit20 Heartbeat；bit21 Request；bit22-30 预留；bit31 DoorLock。
    /// </summary>
    /// <remarks>
    /// <b>用途边界（ADR-TES-37 D3）</b>：本类仅作位含义查阅与诊断的字典标准，
    /// <b>不是活的状态机数据载体</b>。源端 <c>SendSignal32</c>/<c>ReceiveSignal32</c> 在产线
    /// 实为死代码，实际读写仍走字符串地址（<c>ReadLoadSignal</c>/<c>WriteLoadSignal</c>，
    /// 见源端 <c>CheckStationTask.cs:716-981</c>）。本 Issue 不改此运行路径，仅提供位定义。
    /// <para>
    /// <b>3-4 互换 bug 修正登记</b>：源端 <c>ModbusSignal.cs:371-372</c> 存在复制粘贴 bug——
    /// <c>SendProduct4OK</c> 属性的 getter/setter 操作的是 <c>_product3OK</c> 字段并置位
    /// <c>S_Product_3_OK</c> 位，<c>SendProduct3OK</c> 反之操作 <c>_product4OK</c>/<c>S_Product_4_OK</c>，
    /// 即"属性名 ↔ 字段 ↔ 位偏移"三者错位。本类按语义对齐：属性名编号与位偏移一一对应
    /// （<see cref="Product3Ok"/> = bit10 = <c>S_Product_3_OK</c>，
    /// <see cref="Product4Ok"/> = bit13 = <c>S_Product_4_OK</c>），不再互换。
    /// 3 个工站副本（AOI#1/AOI#2/擦拭）该 bug 一致，迁移时统一修正。
    /// </para>
    /// <para>
    /// <b>与 TES-38 Safety 模块 <c>HandshakeBit</c> 的关系（待架构师裁决）</b>：
    /// <c>Luster.Module.Motion.Safety.Network.ModbusSignal32</c> 也定义了 32 位握手位枚举，
    /// 但其位布局为"每产品 4 位 {Exist,OK,NG1,NG2}"（bit0-3=产品1 四态、bit4-7=产品2…），
    /// 与源端 <c>LocalSend</c>/<c>LocalReceive</c> 的"Exist 独立 4 位 + 每产品 3 位 OK/NG"
    /// 布局不同——同一语义位（如 Product2_OK）在两套枚举中偏移不同。两套并存会导致
    /// Safety 的 <c>InterlockConditionType.HandshakeBit</c> 互锁条件在引用握手位时取到错位值。
    /// 本 Issue 范围仅落 Handover 位定义（按源端），不擅改 Safety 件；该布局冲突已升级架构师
    /// （见本 Issue 完成报告 + TES-37 父 Issue 评论），统一方案落地前两套各自独立、勿混用。
    /// </para>
    /// </remarks>
    public static class HandoverSignalBit
    {
        // ===== bit0-3：4 产品在籍信号 Exist =====
        /// <summary>产品1 在籍（源端 <c>S_ProductExist_1</c> = 0）</summary>
        public const int Product1Exist = 0;
        /// <summary>产品2 在籍（源端 <c>S_ProductExist_2</c> = 1）</summary>
        public const int Product2Exist = 1;
        /// <summary>产品3 在籍（源端 <c>S_ProductExist_3</c> = 2）</summary>
        public const int Product3Exist = 2;
        /// <summary>产品4 在籍（源端 <c>S_ProductExist_4</c> = 3）</summary>
        public const int Product4Exist = 3;

        // ===== bit4-6：产品1 结果 =====
        /// <summary>产品1 OK（源端 <c>S_Product_1_OK</c> = 4）</summary>
        public const int Product1Ok = 4;
        /// <summary>产品1 NG1（源端 <c>S_Product_1_NG1</c> = 5）</summary>
        public const int Product1Ng1 = 5;
        /// <summary>产品1 NG2（源端 <c>S_Product_1_NG2</c> = 6）</summary>
        public const int Product1Ng2 = 6;

        // ===== bit7-9：产品2 结果 =====
        /// <summary>产品2 OK（源端 <c>S_Product_2_OK</c> = 7）</summary>
        public const int Product2Ok = 7;
        /// <summary>产品2 NG1（源端 <c>S_Product_2_NG1</c> = 8）</summary>
        public const int Product2Ng1 = 8;
        /// <summary>产品2 NG2（源端 <c>S_Product_2_NG2</c> = 9）</summary>
        public const int Product2Ng2 = 9;

        // ===== bit10-12：产品3 结果（源端 3-4 互换 bug 修正点 :371-372） =====
        /// <summary>产品3 OK（源端 <c>S_Product_3_OK</c> = 10）。
        /// 修正：源端 <c>SendProduct3OK</c> 属性错置 <c>S_Product_4_OK</c> 位，本定义对齐枚举原意。</summary>
        public const int Product3Ok = 10;
        /// <summary>产品3 NG1（源端 <c>S_Product_3_NG1</c> = 11）</summary>
        public const int Product3Ng1 = 11;
        /// <summary>产品3 NG2（源端 <c>S_Product_3_NG2</c> = 12）</summary>
        public const int Product3Ng2 = 12;

        // ===== bit13-15：产品4 结果（源端 3-4 互换 bug 修正点 :371-372） =====
        /// <summary>产品4 OK（源端 <c>S_Product_4_OK</c> = 13）。
        /// 修正：源端 <c>SendProduct4OK</c> 属性错置 <c>S_Product_3_OK</c> 位，本定义对齐枚举原意。</summary>
        public const int Product4Ok = 13;
        /// <summary>产品4 NG1（源端 <c>S_Product_4_NG1</c> = 14）</summary>
        public const int Product4Ng1 = 14;
        /// <summary>产品4 NG2（源端 <c>S_Product_4_NG2</c> = 15）</summary>
        public const int Product4Ng2 = 15;

        // ===== bit16-21：握手状态位 =====
        /// <summary>就绪（源端 <c>S_ReadyOK</c> = 16 / <c>R_ReadyOK</c> = 16）</summary>
        public const int Ready = 16;
        /// <summary>发送中/接收中（源端 <c>S_Sending</c> = 17 / <c>R_Receiving</c> = 17）</summary>
        public const int Sending = 17;
        /// <summary>传输中（源端 <c>S_Transfer</c> = 18 / <c>R_Transfer</c> = 18）</summary>
        public const int Transfer = 18;
        /// <summary>互锁（源端 <c>S_Interlock</c> = 19 / <c>R_Interlock</c> = 19）</summary>
        public const int InterLock = 19;
        /// <summary>心跳（2 倍读周期翻转；源端 <c>S_HreartBeat</c> = 20 / <c>R_HreartBeat</c> = 20）</summary>
        public const int Heartbeat = 20;
        /// <summary>请求（源端 <c>S_Request</c> = 21 / <c>R_Reseve1</c> = 21）</summary>
        public const int Request = 21;

        // ===== bit22-30：预留（源端 <c>S_Reseve2</c>..<c>S_Reseve10</c> = 22-30） =====
        /// <summary>预留位 2（源端 <c>S_Reseve2</c> = 22）</summary>
        public const int Reserve2 = 22;
        /// <summary>预留位 3（源端 <c>S_Reseve3</c> = 23）</summary>
        public const int Reserve3 = 23;
        /// <summary>预留位 4（源端 <c>S_Reseve4</c> = 24）</summary>
        public const int Reserve4 = 24;
        /// <summary>预留位 5（源端 <c>S_Reseve5</c> = 25）</summary>
        public const int Reserve5 = 25;
        /// <summary>预留位 6（源端 <c>S_Reseve6</c> = 26）</summary>
        public const int Reserve6 = 26;
        /// <summary>预留位 7（源端 <c>S_Reseve7</c> = 27）</summary>
        public const int Reserve7 = 27;
        /// <summary>预留位 8（源端 <c>S_Reseve8</c> = 28）</summary>
        public const int Reserve8 = 28;
        /// <summary>预留位 9（源端 <c>S_Reseve9</c> = 29）</summary>
        public const int Reserve9 = 29;
        /// <summary>预留位 10（源端 <c>S_Reseve10</c> = 30）</summary>
        public const int Reserve10 = 30;

        // ===== bit31：安全门 =====
        /// <summary>安全门锁（源端 <c>S_DoorLock</c> = 31 / <c>R_DoorLock</c> = 31）</summary>
        public const int DoorLock = 31;

        /// <summary>信号位总数（32 位字）</summary>
        public const int BitCount = 32;

        /// <summary>
        /// 构造指定位的掩码（1 &lt;&lt; bit）。
        /// 供诊断/单测按位偏移构造预期值用，运行时读写仍走字符串地址。
        /// </summary>
        public static uint Mask(int bit) => 1u << bit;

        /// <summary>
        /// 线缆值高低 16 位交换（对齐源端 <c>ModbusSignal.GetValue</c> 字节序）。
        /// 源端在线缆上做高/低 16 位交换，业务侧位语义需经此还原。
        /// </summary>
        public static uint SwapHalves(uint data)
            => ((data & 0xFFFF0000u) >> 16) | ((data & 0x0000FFFFu) << 16);
    }
}
