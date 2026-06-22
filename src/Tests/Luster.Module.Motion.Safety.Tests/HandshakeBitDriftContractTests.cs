using Luster.Module.Motion.Handover.Signals;
using Luster.Module.Motion.Safety.Network;
using Xunit;

namespace Luster.Module.Motion.Safety.Tests
{
    /// <summary>
    /// 跨模块防漂移契约单测（TES-51）。
    /// 把同一语义位在三套定义上钉死，任一方漂移即失败：
    ///   1) Safety 件 <see cref="HandshakeBit"/>（[Flags] enum，位掩码）
    ///   2) Handover 件 <see cref="HandoverSignalBit"/>（const int 字典，位偏移）
    ///   3) 源端 SP-2025140 <c>ModbusSignal.cs:587-660</c> 的 <c>LocalSend</c>/<c>LocalReceive</c> 枚举真值
    /// 三者对同一语义位的偏移必须完全一致。源端真值为唯一权威。
    /// </summary>
    /// <remarks>
    /// 本测试为<b>仅测试引用</b> Handover 件（见 Safety.Tests.csproj 的 ProjectReference 注释），
    /// 不构成 Safety 生产件对 Handover 的依赖；两模块生产件仍各自独立（ADR-TES-37 D3-补遗）。
    /// </remarks>
    public class HandshakeBitDriftContractTests
    {
        // ===== 源端 LocalSend/LocalReceive 枚举真值（ModbusSignal.cs:587-660，发送/接收对称） =====
        // 按源端枚举成员原样登记偏移，作为钉死基准；任何一方漂移都会让下面的断言失败。
        private static class SourceOffset
        {
            // 在籍 Exist（S_ProductExist_1..4 = 0..3）
            public const int Product1Exist = 0;
            public const int Product2Exist = 1;
            public const int Product3Exist = 2;
            public const int Product4Exist = 3;

            // 产品1 结果（S_Product_1_OK/NG1/NG2 = 4/5/6）
            public const int Product1Ok = 4;
            public const int Product1Ng1 = 5;
            public const int Product1Ng2 = 6;

            // 产品2 结果（S_Product_2_OK/NG1/NG2 = 7/8/9）
            public const int Product2Ok = 7;
            public const int Product2Ng1 = 8;
            public const int Product2Ng2 = 9;

            // 产品3 结果（S_Product_3_OK/NG1/NG2 = 10/11/12，源端 3-4 互换 bug 修正点 :371-372）
            public const int Product3Ok = 10;
            public const int Product3Ng1 = 11;
            public const int Product3Ng2 = 12;

            // 产品4 结果（S_Product_4_OK/NG1/NG2 = 13/14/15，源端 3-4 互换 bug 修正点 :371-372）
            public const int Product4Ok = 13;
            public const int Product4Ng1 = 14;
            public const int Product4Ng2 = 15;

            // 握手状态位（S_ReadyOK=16 / S_Sending=17 / S_Transfer=18 / S_Interlock=19 / S_HreartBeat=20 / S_Request=21）
            public const int Ready = 16;
            public const int Sending = 17;
            public const int Transfer = 18;
            public const int Interlock = 19;
            public const int Heartbeat = 20;
            public const int Request = 21;

            // 门锁（S_DoorLock=31）
            public const int DoorLock = 31;
        }

        /// <summary>
        /// 三套定义对同一语义位的偏移必须一致：
        ///   (uint)HandshakeBit.X == 1u &lt;&lt; HandoverSignalBit.Y == 1u &lt;&lt; sourceOffset
        /// </summary>
        private static void AssertSameBit(HandshakeBit handshake, int handoverOffset, int sourceOffset)
        {
            uint expected = 1u << sourceOffset;
            Assert.Equal(expected, (uint)handshake);
            Assert.Equal(expected, 1u << handoverOffset);
            Assert.Equal(sourceOffset, handoverOffset);
        }

        // ===== 四产品在籍 Exist =====

        [Fact]
        public void 产品在籍位_三套定义一致()
        {
            AssertSameBit(HandshakeBit.Product1Exist, HandoverSignalBit.Product1Exist, SourceOffset.Product1Exist);
            AssertSameBit(HandshakeBit.Product2Exist, HandoverSignalBit.Product2Exist, SourceOffset.Product2Exist);
            AssertSameBit(HandshakeBit.Product3Exist, HandoverSignalBit.Product3Exist, SourceOffset.Product3Exist);
            AssertSameBit(HandshakeBit.Product4Exist, HandoverSignalBit.Product4Exist, SourceOffset.Product4Exist);
        }

        // ===== 四产品 OK/NG1/NG2 =====

        [Fact]
        public void 产品1结果位_三套定义一致()
        {
            AssertSameBit(HandshakeBit.Product1_OK, HandoverSignalBit.Product1Ok, SourceOffset.Product1Ok);
            AssertSameBit(HandshakeBit.Product1_NG1, HandoverSignalBit.Product1Ng1, SourceOffset.Product1Ng1);
            AssertSameBit(HandshakeBit.Product1_NG2, HandoverSignalBit.Product1Ng2, SourceOffset.Product1Ng2);
        }

        [Fact]
        public void 产品2结果位_三套定义一致()
        {
            AssertSameBit(HandshakeBit.Product2_OK, HandoverSignalBit.Product2Ok, SourceOffset.Product2Ok);
            AssertSameBit(HandshakeBit.Product2_NG1, HandoverSignalBit.Product2Ng1, SourceOffset.Product2Ng1);
            AssertSameBit(HandshakeBit.Product2_NG2, HandoverSignalBit.Product2Ng2, SourceOffset.Product2Ng2);
        }

        [Fact]
        public void 产品3结果位_三套定义一致_且3与4互换bug已修正()
        {
            // 源端 ModbusSignal.cs:371-372 的 SendProduct3OK/SendProduct4OK setter 互换 bug：
            // 修正后属性名编号 ↔ 位偏移一一对应，Product3_OK 落 bit10、Product4_OK 落 bit13。
            AssertSameBit(HandshakeBit.Product3_OK, HandoverSignalBit.Product3Ok, SourceOffset.Product3Ok);
            AssertSameBit(HandshakeBit.Product3_NG1, HandoverSignalBit.Product3Ng1, SourceOffset.Product3Ng1);
            AssertSameBit(HandshakeBit.Product3_NG2, HandoverSignalBit.Product3Ng2, SourceOffset.Product3Ng2);
        }

        [Fact]
        public void 产品4结果位_三套定义一致_且3与4互换bug已修正()
        {
            AssertSameBit(HandshakeBit.Product4_OK, HandoverSignalBit.Product4Ok, SourceOffset.Product4Ok);
            AssertSameBit(HandshakeBit.Product4_NG1, HandoverSignalBit.Product4Ng1, SourceOffset.Product4Ng1);
            AssertSameBit(HandshakeBit.Product4_NG2, HandoverSignalBit.Product4Ng2, SourceOffset.Product4Ng2);
        }

        // ===== bit16-21 握手状态位 =====

        [Fact]
        public void 握手状态位_三套定义一致()
        {
            AssertSameBit(HandshakeBit.Ready, HandoverSignalBit.Ready, SourceOffset.Ready);
            AssertSameBit(HandshakeBit.Sending, HandoverSignalBit.Sending, SourceOffset.Sending);
            AssertSameBit(HandshakeBit.Transfer, HandoverSignalBit.Transfer, SourceOffset.Transfer);
            AssertSameBit(HandshakeBit.Interlock, HandoverSignalBit.InterLock, SourceOffset.Interlock);
            AssertSameBit(HandshakeBit.Heartbeat, HandoverSignalBit.Heartbeat, SourceOffset.Heartbeat);
            AssertSameBit(HandshakeBit.Request, HandoverSignalBit.Request, SourceOffset.Request);
        }

        // ===== bit31 门锁 =====

        [Fact]
        public void 门锁位_三套定义一致()
        {
            AssertSameBit(HandshakeBit.DoorLock, HandoverSignalBit.DoorLock, SourceOffset.DoorLock);
        }

        // ===== 全量互斥：bit0-15 产品区 + 状态位无偏移重叠 =====

        [Fact]
        public void 全部语义位偏移互斥_无重叠()
        {
            // 把三套定义里全部关键语义位的掩码 OR 到一起，聚合值应等于 bits0-21 | bit31
            // （若任意两语义位偏移相同，OR 会丢位，聚合值会小于预期 → 失败）。
            uint[] masks =
            {
                (uint)HandshakeBit.Product1Exist, (uint)HandshakeBit.Product2Exist,
                (uint)HandshakeBit.Product3Exist, (uint)HandshakeBit.Product4Exist,
                (uint)HandshakeBit.Product1_OK, (uint)HandshakeBit.Product1_NG1, (uint)HandshakeBit.Product1_NG2,
                (uint)HandshakeBit.Product2_OK, (uint)HandshakeBit.Product2_NG1, (uint)HandshakeBit.Product2_NG2,
                (uint)HandshakeBit.Product3_OK, (uint)HandshakeBit.Product3_NG1, (uint)HandshakeBit.Product3_NG2,
                (uint)HandshakeBit.Product4_OK, (uint)HandshakeBit.Product4_NG1, (uint)HandshakeBit.Product4_NG2,
                (uint)HandshakeBit.Ready, (uint)HandshakeBit.Sending, (uint)HandshakeBit.Transfer,
                (uint)HandshakeBit.Interlock, (uint)HandshakeBit.Heartbeat, (uint)HandshakeBit.Request,
                (uint)HandshakeBit.DoorLock,
            };

            uint all = 0;
            foreach (uint m in masks)
            {
                Assert.Equal(0u, all & m); // 新位不得与已聚合位重叠
                all |= m;
            }

            // 23 个语义位（4 Exist + 12 OK/NG + 6 状态 + 1 门锁）恰好覆盖 bits0-21 与 bit31
            Assert.Equal(23, masks.Length);
            Assert.Equal(0x803FFFFFu, all); // bits 0-21 (0x003FFFFF) | bit31 (0x80000000)
        }
    }
}
