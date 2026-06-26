using System;

namespace Luster.Module.Motion.Safety.Network
{
    /// <summary>
    /// 32 位握手信号位定义（TES-37/TES-38 共用基础件）。
    /// 忠实迁移自源端 SP-2025140 <c>Plugin.CommonPlugin\Model\Args\ModbusSignal.cs</c>，
    /// 一个 32 位 Modbus 字承载上下游交接的全部状态位：
    /// bit0-15 = 4 产品 × {Exist, OK, NG1, NG2}；bit16 Ready；bit17 Sending/Receiving；
    /// bit18 Transfer；bit19 Interlock；bit20 Heartbeat；bit21 Request；bit22-30 预留；bit31 DoorLock。
    /// </summary>
    /// <remarks>
    /// 源端在线缆上会做高/低 16 位交换（<c>ModbusSignal.GetValue</c>），本结构提供
    /// <see cref="ToWire"/> / <see cref="FromWire"/> 透明处理，业务层直接操作位语义即可。
    /// 另修正源端 <c>SendProduct3OK</c>/<c>SendProduct4OK</c> setter 互换 bug（源端 :371-372）。
    /// </remarks>
    [Flags]
    public enum HandshakeBit : uint
    {
        None = 0,

        // ===== bit0-15：4 产品 × {Exist, OK, NG1, NG2} =====
        Product1Exist = 1u << 0,
        Product1_OK = 1u << 1,
        Product1_NG1 = 1u << 2,
        Product1_NG2 = 1u << 3,

        Product2Exist = 1u << 4,
        Product2_OK = 1u << 5,
        Product2_NG1 = 1u << 6,
        Product2_NG2 = 1u << 7,

        Product3Exist = 1u << 8,
        Product3_OK = 1u << 9,
        Product3_NG1 = 1u << 10,
        Product3_NG2 = 1u << 11,

        Product4Exist = 1u << 12,
        Product4_OK = 1u << 13,
        Product4_NG1 = 1u << 14,
        Product4_NG2 = 1u << 15,

        // ===== bit16-21：握手状态位 =====
        Ready = 1u << 16,
        Sending = 1u << 17,
        Transfer = 1u << 18,
        Interlock = 1u << 19,
        Heartbeat = 1u << 20,
        Request = 1u << 21,

        // ===== bit31：门锁 =====
        DoorLock = 1u << 31
    }

    /// <summary>
    /// 32 位握手信号字。封装位读写与线缆高低 16 位交换，
    /// 供 <c>HandoverNode</c>（TES-37）与 <c>InterlockMatrix</c>（TES-38）共用。
    /// </summary>
    public class ModbusSignal32
    {
        private uint _value;

        public ModbusSignal32(uint value = 0) { _value = value; }

        /// <summary>当前业务侧值（已还原位语义，未做线缆交换）</summary>
        public uint Value => _value;

        /// <summary>读指定位</summary>
        public bool GetBit(HandshakeBit bit) => (_value & (uint)bit) != 0;

        /// <summary>置/清指定位</summary>
        public void SetBit(HandshakeBit bit, bool on)
        {
            if (on) _value |= (uint)bit;
            else _value &= ~(uint)bit;
        }

        /// <summary>全部清零</summary>
        public void ResetAll() => _value = 0;

        /// <summary>
        /// 输出线缆值：高/低 16 位交换（对齐源端 <c>ModbusSignal.GetValue</c>）。
        /// </summary>
        public uint ToWire() => Swap16(_value);

        /// <summary>
        /// 从线缆值解析：高/低 16 位交换还原业务侧位语义。
        /// </summary>
        public static ModbusSignal32 FromWire(uint wire) => new ModbusSignal32(Swap16(wire));

        /// <summary>高/低 16 位交换</summary>
        private static uint Swap16(uint data) => ((data & 0xFFFF0000u) >> 16) | ((data & 0x0000FFFFu) << 16);

        /// <summary>批量更新产品在籍/结果位（4 产品）</summary>
        public void RefreshProductSignals(bool[] exist, bool[] ok, bool[] ng1, bool[] ng2)
        {
            // 先清掉 bit0-15 产品区
            _value &= 0xFFFF0000u;
            for (int i = 0; i < 4; i++)
            {
                if (i < exist.Length && exist[i]) _value |= ProductExistBit(i);
                if (i < ok.Length && ok[i]) _value |= ProductOkBit(i);
                if (i < ng1.Length && ng1[i]) _value |= ProductNg1Bit(i);
                if (i < ng2.Length && ng2[i]) _value |= ProductNg2Bit(i);
            }
        }

        private static uint ProductExistBit(int i) => 1u << (i * 4 + 0);
        private static uint ProductOkBit(int i) => 1u << (i * 4 + 1);
        private static uint ProductNg1Bit(int i) => 1u << (i * 4 + 2);
        private static uint ProductNg2Bit(int i) => 1u << (i * 4 + 3);
    }
}
