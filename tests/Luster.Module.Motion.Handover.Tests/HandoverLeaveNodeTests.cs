#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverLeaveNodeTests
* 文 件 名:       HandoverLeaveNodeTests.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 下料状态机单测(主路径 + 异常分支)
************************************************************************************/
#endregion

using Luster.Module.Motion.Handover;
using Luster.Module.Motion.Handover.Signals;
using System.Collections.Generic;
using Xunit;

namespace Luster.Module.Motion.Handover.Tests
{
    /// <summary>
    /// 下料交握状态机单测:验证 13 步主路径 + 异常分支 101/102 可还原。
    /// <para>测试方式同 <see cref="HandoverFeedNodeTests"/>:按步信号脚本 + 写后回读模拟 Modbus 读写。</para>
    /// </summary>
    public class HandoverLeaveNodeTests
    {
        /// <summary>构造一个配置好地址的下料测试节点</summary>
        private static LeaveTestNode CreateNode()
        {
            var node = new LeaveTestNode
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
        private static int DriveToDone(LeaveTestNode node, int max = 5000)
        {
            int step = 0, guard = 0;
            while (step != HandoverNode.StepDone && step != HandoverNode.StepAbort && guard++ < max)
            {
                step = node.RunStep(step);
            }
            return step;
        }

        /// <summary>
        /// 下料主路径:0→12→99→100,验证 13 步主路径完整走通并完成。
        /// <para>下游信号按步编排:case 10 要求 RecTransfer ON(下游确认接收),
        /// case 12 要求 RecTransfer/RecInterLock OFF(下游撤离完成)。</para>
        /// </summary>
        [Fact]
        public void Leave_MainPath_13Steps_Completes()
        {
            var node = CreateNode();
            var a = node.Address;

            node.OnRead = (step, addr) =>
            {
                if (addr == a.RecReadyAddress) return true;       // 下游就绪恒 ON
                if (addr == a.RecInterLockAddress) return step <= 10; // case 3 ON;case 12 撤销 OFF
                if (addr == a.RecingAddress) return true;          // case 7 下游接收中
                // RecTransfer:case 10 要求 ON(确认),case 12 要求 OFF(撤离完成)
                if (addr == a.RecTranSferAddress) return step == 10;
                return false;
            };

            int step = DriveToDone(node);

            Assert.Equal(HandoverNode.StepDone, step);
            // 验证关键发送信号被写入(SendInterLock/Sending/SendTransfer 均 ON 过)
            Assert.Contains((a.SendInterLockAddress, true), node.Writes);
            Assert.Contains((a.SendingAddress, true), node.Writes);
            Assert.Contains((a.SendTranSferAddress, true), node.Writes);
            // 完成时发送侧全部 OFF(UnloadSingleClaer)
            Assert.Contains((a.SendInterLockAddress, false), node.Writes);
            Assert.Contains((a.SendingAddress, false), node.Writes);
            Assert.Contains((a.SendTranSferAddress, false), node.Writes);
            // 下料主路径 case 99 会慢速返回下料等待位
            Assert.True(node.SafeMoves > 0, "主路径完成应慢速返回等待位");
        }

        /// <summary>
        /// 下料异常分支:case 10 时 RecTransfer OFF → 进 101 → 102 → 100。
        /// 验证异常分支 101/102 可还原。
        /// </summary>
        [Fact]
        public void Leave_AbnormalBranch_101_102_Restores()
        {
            var node = CreateNode();
            var a = node.Address;

            node.OnRead = (step, addr) =>
            {
                if (addr == a.RecReadyAddress) return true;
                if (addr == a.RecInterLockAddress) return true;
                if (addr == a.RecingAddress) return true;
                // case 10:RecTransfer OFF → 进异常 101(对齐源端 :1268)
                if (addr == a.RecTranSferAddress) return false;
                return false;
            };

            int step = DriveToDone(node);

            // 异常分支最终走 101→102→100 完成(对齐源端异常收尾)
            Assert.Equal(HandoverNode.StepDone, step);
            // 异常分支会调用 MoveToSafePosition(异常撤离)
            Assert.True(node.SafeMoves > 0, "异常分支应触发异常撤离");
            // 异常分支 case 102 会清信号(UnloadSingleClaer)
            Assert.Contains((a.SendingAddress, false), node.Writes);
        }

        /// <summary>
        /// 下料等待下游 Ready:未就绪时停在 case 1 不推进。
        /// </summary>
        [Fact]
        public void Leave_WaitDownstreamReady_StaysAt1()
        {
            var node = CreateNode();
            node.OnRead = (step, addr) => false;

            // case 0:MoveToWaitPosition(桩返回 true)→ 进 1
            int step = node.RunStep(0);
            Assert.Equal(1, step);
            // case 1 等 RecReady,未就绪应停 1
            Assert.Equal(1, node.RunStep(1));
        }

        /// <summary>
        /// 测试子类:暴露 StepMachine,用按步信号脚本 + 写后回读模拟 Modbus 读写。
        /// </summary>
        private class LeaveTestNode : HandoverLeaveNode
        {
            /// <summary>按步信号读脚本:(当前步, 地址) → 信号值</summary>
            public System.Func<int, string, bool> OnRead = (s, a) => false;

            /// <summary>记录所有写操作(address, value)</summary>
            public List<(string, bool)> Writes = new List<(string, bool)>();

            /// <summary>慢速撤离调用计数</summary>
            public int SafeMoves;

            /// <summary>Send 侧回读快照(WriteSignal 写入后回读可见)</summary>
            private readonly Dictionary<string, bool> _sendSnap = new Dictionary<string, bool>();

            /// <summary>当前步</summary>
            private int _curStep;

            /// <summary>暴露 protected StepMachine 供测试驱动</summary>
            public int RunStep(int step)
            {
                _curStep = step;
                return StepMachine(step);
            }

            protected override bool ReadSignal(string address)
            {
                if (_sendSnap.TryGetValue(address, out var v)) return v;
                return OnRead(_curStep, address);
            }

            protected override bool WriteSignal(string address, bool value)
            {
                Writes.Add((address, value));
                _sendSnap[address] = value;
                return true;
            }

            // 运动钩子桩
            protected override bool MoveToWaitPosition() => true;
            protected override bool MoveToWorkPosition() => true;
            protected override bool MoveToSafePosition() { SafeMoves++; return true; }
            protected override bool SorbControl(bool on) => true;
            protected override bool TransferProductInfo() => true;
            protected override void SyncWait(int milliseconds) { }
            protected override void StepInfo(int step, string message) { }
        }
    }
}
