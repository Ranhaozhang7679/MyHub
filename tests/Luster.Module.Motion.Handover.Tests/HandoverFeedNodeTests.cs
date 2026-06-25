#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverFeedNodeTests
* 文 件 名:       HandoverFeedNodeTests.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 上料状态机单测(主路径 + 异常分支)
************************************************************************************/
#endregion

using Luster.Module.Motion.Handover;
using Luster.Module.Motion.Handover.Signals;
using System.Collections.Generic;
using Xunit;

namespace Luster.Module.Motion.Handover.Tests
{
    /// <summary>
    /// 上料交握状态机单测:验证 15 步主路径 + 异常分支 101/102 可还原。
    /// <para>测试通过 <see cref="FeedTestNode"/> 注入"信号快照字典"模拟 Modbus 寄存器:
    /// ReadSignal 读字典、WriteSignal 写字典,真实还原源端"写后回读确认"的握手语义,
    /// 让状态机在编排的下游信号下自然推进。</para>
    /// </summary>
    public class HandoverFeedNodeTests
    {
        /// <summary>构造一个配置好地址的上料测试节点</summary>
        private static FeedTestNode CreateNode()
        {
            var node = new FeedTestNode
            {
                Address = new HandoverSignalAddress
                {
                    SendReadyAddress = "1 05 0 1",
                    SendingAddress = "1 05 1 1",
                    SendTranSferAddress = "1 05 2 1",
                    SendInterLockAddress = "1 05 3 1",
                    RecReadyAddress = "1 02 0 1",
                    RecingAddress = "1 02 1 1",
                    RecTranSferAddress = "1 02 2 1",
                    RecInterLockAddress = "1 02 3 1",
                },
            };
            return node;
        }

        /// <summary>驱动状态机直到完成/中止/超步</summary>
        private static int DriveToDone(FeedTestNode node, int max = 5000)
        {
            int step = 0, guard = 0;
            while (step != HandoverNode.StepDone && step != HandoverNode.StepAbort && guard++ < max)
            {
                step = node.RunStep(step);
            }
            return step;
        }

        /// <summary>
        /// 上料主路径:0→14→98→99→100,验证 15 步主路径完整走通并完成。
        /// <para>下游信号按步编排(还原源端时变握手):
        /// case 9 推进条件为 RecTransfer ON 或 RecInterLock OFF —— 主路径下下游在 case 9 后撤销 InterLock。</para>
        /// </summary>
        [Fact]
        public void Feed_MainPath_15Steps_Completes()
        {
            var node = CreateNode();
            var a = node.Address;

            // 按步编排下游信号(Rec* 侧),Send 侧由 WriteSignal 回读自动满足
            node.OnRead = (step, addr) =>
            {
                // RecReady 恒 ON(上游就绪),异常回退分支除外
                if (addr == a.RecReadyAddress) return true;
                if (addr == a.RecingAddress) return true;       // case 6 上游发送中
                // RecInterLock:case 3/4 期间 ON;case 9 后撤销 OFF(下游撤离)使 case 9/13 推进
                if (addr == a.RecInterLockAddress) return step < 9;
                // RecTransfer:主路径下 case 13 要求 OFF
                if (addr == a.RecTranSferAddress) return false;
                return false;
            };

            int step = DriveToDone(node);

            Assert.Equal(HandoverNode.StepDone, step);
            // 验证关键发送信号被写入(InterLock/Sending/Transfer 均 ON 过)
            Assert.Contains((a.SendInterLockAddress, true), node.Writes);
            Assert.Contains((a.SendingAddress, true), node.Writes);
            Assert.Contains((a.SendTranSferAddress, true), node.Writes);
            // 完成时发送侧全部 OFF(LoadSingleClaer)
            Assert.Contains((a.SendInterLockAddress, false), node.Writes);
            Assert.Contains((a.SendingAddress, false), node.Writes);
            Assert.Contains((a.SendTranSferAddress, false), node.Writes);
        }

        /// <summary>
        /// 上料异常分支:case 13 时 RecInterLock 或 RecTransfer 仍 ON → 进 101 → 102 → 99 → 100。
        /// 验证异常分支 101/102 可还原。
        /// </summary>
        [Fact]
        public void Feed_AbnormalBranch_101_102_Restores()
        {
            var node = CreateNode();
            var a = node.Address;

            node.OnRead = (step, addr) =>
            {
                if (addr == a.RecReadyAddress) return true;
                if (addr == a.RecingAddress) return true;
                // RecInterLock 恒 ON(下游未撤离互锁)
                if (addr == a.RecInterLockAddress) return true;
                // case 13:RecTransfer 仍 ON → 进异常 101
                if (addr == a.RecTranSferAddress) return true;
                // case 101:SendReady 回读 ON
                if (addr == a.SendReadyAddress) return true;
                return false;
            };

            int step = DriveToDone(node);

            // 异常分支最终走 101→102→99→100 完成(对齐源端异常收尾)
            Assert.Equal(HandoverNode.StepDone, step);
            // 异常分支 case 101 会写 SendReady ON(对齐源端)
            Assert.Contains((a.SendReadyAddress, true), node.Writes);
            // 异常分支会调用 MoveToSafePosition(慢速撤离)
            Assert.True(node.SafeMoves > 0, "异常分支应触发慢速撤离");
        }

        /// <summary>
        /// 上料等待上游 Ready:未就绪时停在 case 1 不推进(对齐源端 SyncWait 循环)。
        /// </summary>
        [Fact]
        public void Feed_WaitUpstreamReady_StaysAt1()
        {
            var node = CreateNode();
            // 上游全部未就绪
            node.OnRead = (step, addr) => false;

            // case 0:MoveToWaitPosition(桩返回 true)+ ClearSignals → 进 1
            int step = node.RunStep(0);
            Assert.Equal(1, step);
            // case 1 等 RecReady,未就绪应停 1
            Assert.Equal(1, node.RunStep(1));
        }

        /// <summary>
        /// 测试子类:暴露 StepMachine,用按步信号脚本 + 写后回读模拟 Modbus 读写。
        /// </summary>
        private class FeedTestNode : HandoverFeedNode
        {
            /// <summary>按步信号读脚本:(当前步, 地址) → 信号值</summary>
            public System.Func<int, string, bool> OnRead = (s, a) => false;

            /// <summary>记录所有写操作(address, value)</summary>
            public List<(string, bool)> Writes = new List<(string, bool)>();

            /// <summary>慢速撤离调用计数</summary>
            public int SafeMoves;

            /// <summary>Send 侧回读快照(WriteSignal 写入后回读可见)</summary>
            private readonly Dictionary<string, bool> _sendSnap = new Dictionary<string, bool>();

            /// <summary>当前步(供 OnRead 脚本按步返回)</summary>
            private int _curStep;

            /// <summary>暴露 protected StepMachine 供测试驱动</summary>
            public int RunStep(int step)
            {
                _curStep = step;
                return StepMachine(step);
            }

            protected override bool ReadSignal(string address)
            {
                // Send 侧优先回读快照(还原源端写后回读确认)
                if (_sendSnap.TryGetValue(address, out var v)) return v;
                // Rec 侧用按步脚本
                return OnRead(_curStep, address);
            }

            protected override bool WriteSignal(string address, bool value)
            {
                Writes.Add((address, value));
                _sendSnap[address] = value; // 写后回读可见
                return true;
            }

            // 运动钩子桩:默认成功,避免状态机因运动失败中止
            protected override bool MoveToWaitPosition() => true;
            protected override bool MoveToWorkPosition() => true;
            protected override bool MoveToSafePosition() { SafeMoves++; return true; }
            protected override bool SorbControl(bool on) => true;
            protected override bool TransferProductInfo() => true;
            protected override void SyncWait(int milliseconds) { /* 测试不等待 */ }
            protected override void StepInfo(int step, string message) { /* 测试不记日志,避免依赖 MyOwner */ }
        }
    }
}
