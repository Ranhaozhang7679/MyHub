using Luster.Module.Motion.Safety.Network;
using Xunit;

namespace Luster.Module.Motion.Safety.Tests
{
    public class ModbusSignal32Tests
    {
        [Fact]
        public void SetGetBit_基础位读写()
        {
            var sig = new ModbusSignal32();
            Assert.False(sig.GetBit(HandshakeBit.Ready));

            sig.SetBit(HandshakeBit.Ready, true);
            Assert.True(sig.GetBit(HandshakeBit.Ready));

            sig.SetBit(HandshakeBit.Ready, false);
            Assert.False(sig.GetBit(HandshakeBit.Ready));
        }

        [Fact]
        public void ToWire_FromWire_高低16位交换()
        {
            // bit16(Ready) 置位 → 业务值 0x00010000；线缆交换后应为 0x00000001
            var sig = new ModbusSignal32();
            sig.SetBit(HandshakeBit.Ready, true);
            Assert.Equal(0x00010000u, sig.Value);

            uint wire = sig.ToWire();
            Assert.Equal(0x00000001u, wire);

            // 反向解析还原
            var parsed = ModbusSignal32.FromWire(wire);
            Assert.True(parsed.GetBit(HandshakeBit.Ready));
            Assert.Equal(sig.Value, parsed.Value);
        }

        [Fact]
        public void ToWire_双字节交换往返一致()
        {
            var sig = new ModbusSignal32(0xABCD1234u);
            uint wire = sig.ToWire();
            // 高低 16 位交换：0x1234ABCD
            Assert.Equal(0x1234ABCDu, wire);
            Assert.Equal(0xABCD1234u, ModbusSignal32.FromWire(wire).Value);
        }

        [Fact]
        public void ResetAll_清零全部位()
        {
            var sig = new ModbusSignal32();
            sig.SetBit(HandshakeBit.Interlock, true);
            sig.SetBit(HandshakeBit.DoorLock, true);
            Assert.NotEqual(0u, sig.Value);

            sig.ResetAll();
            Assert.Equal(0u, sig.Value);
        }

        [Fact]
        public void RefreshProductSignals_4产品位正确分布()
        {
            var sig = new ModbusSignal32();
            sig.RefreshProductSignals(
                exist: new[] { true, false, true, false },
                ok:   new[] { false, true, false, false },
                ng1:  new[] { false, false, true, false },
                ng2:  new[] { false, false, false, true });

            // 源端布局：Exist=i / OK=4+i*3 / NG1=4+i*3+1 / NG2=4+i*3+2
            // 产品1 Exist = bit0
            Assert.True(sig.GetBit(HandshakeBit.Product1Exist));
            // 产品3 Exist = bit2
            Assert.True(sig.GetBit(HandshakeBit.Product3Exist));
            // 产品2 OK = bit7
            Assert.True(sig.GetBit(HandshakeBit.Product2_OK));
            // 产品3 NG1 = bit11
            Assert.True(sig.GetBit(HandshakeBit.Product3_NG1));
            // 产品4 NG2 = bit15
            Assert.True(sig.GetBit(HandshakeBit.Product4_NG2));
            // 未置位项
            Assert.False(sig.GetBit(HandshakeBit.Product1_OK));
            Assert.False(sig.GetBit(HandshakeBit.Product4Exist));
        }

        [Fact]
        public void DoorLock_最高位可独立置位()
        {
            var sig = new ModbusSignal32();
            sig.SetBit(HandshakeBit.DoorLock, true);
            Assert.Equal(0x80000000u, sig.Value);
            Assert.True(sig.GetBit(HandshakeBit.DoorLock));
        }
    }
}
