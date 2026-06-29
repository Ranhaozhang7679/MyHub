using System;
using System.Collections.Generic;
using Luster.Motion.DataStruct.DataModels;
using Luster.Module.Motion.Handover;
using Xunit;

namespace Luster.Module.Motion.Regression.Baseline.Tests
{
    /// <summary>
    /// ④ 握手信号回归 (TES-165 P9-D / Suite=Handshake)。
    /// 锁定 HandoverNode 状态机契约常量（StepDone/StepAbort）+ HandoverRole 角色枚举，
    /// 并补 ICW 握手超时到期重试分支回归（DateTimeNow seam 已就位，可控时钟驱动）。
    /// 信号读写边沿检测依赖 VCommuncation 通信设备装配，属集成测试范畴，本集只锁契约 + 超时 seam。
    /// 无硬件依赖，必 GREEN。
    /// 注：方法名用 ASCII，规避 xunit.runner.visualstudio 3.1.5 对中文方法名的发现不稳定问题。
    /// </summary>
    [Trait("Category", "Regression")]
    [Trait("Suite", "Handshake")]
    public class HandshakeRegressionTests
    {
        // 对齐源端 step==100 退出
        [Fact]
        public void HandoverNode_StepDoneContract()
        {
            Assert.Equal(100, HandoverNode.StepDone);
        }

        // 对齐源端运动失败 return false：整个握手返回失败
        [Fact]
        public void HandoverNode_StepAbortContract()
        {
            Assert.Equal(-1000, HandoverNode.StepAbort);
        }

        // 交握角色 3 个：上料(Feed)/下料(Leave)/ICW
        [Fact]
        public void HandoverRole_MembersCompleteAndValuesMatch()
        {
            Assert.Equal(3, Enum.GetNames(typeof(HandoverRole)).Length);
            Assert.Equal(HandoverRole.Feed, (HandoverRole)0);
            Assert.Equal(HandoverRole.Leave, (HandoverRole)1);
            Assert.Equal(HandoverRole.ICW, (HandoverRole)2);
        }

        // ICW 握手响应超时到期重试回归（HandoverICWNode.cs:160-164）：
        // 驱动可控时钟从 _responseStart 持续返回 None（无响应），越过 ResponseTimeout 后命中
        // "读取响应超时，重试"分支 → step 回 StepClearAndWriteInfo 重置 _responseStart 整轮重试。
        // 断言：WarningPause 记录超时消息 + Request 被写 ≥2 次（首轮 + 超时后重试轮）。
        [Fact]
        public void IcwHandshake_ResponseTimeoutExpiresRetriesAndCompletes()
        {
            var node = CreateIcwTestNode();
            var addr = node.IcwAddress;

            // 响应脚本：足够多次 None 让 SyncWait(PollInterval) 累积越过 ResponseTimeout 触发超时重试，
            // 最后一次 Success 让整轮重试后正常收尾（避免无限轮询）。
            // PollInterval=100、ResponseTimeout=5000 → 约 50 次 None 越过阈值，给 60 次余量后 Success。
            var script = new Queue<ushort>();
            for (int i = 0; i < 60; i++) script.Enqueue((ushort)ICWSyncResultCodeType.None);
            script.Enqueue((ushort)ICWSyncResultCodeType.Success);
            node.ResponseScript = script;

            bool ok = node.DoExcute(out string errMsg);

            Assert.True(ok, errMsg);
            // 命中超时重试分支：WarningPause 记录了"读取响应超时,重试"
            Assert.Contains(node.Warnings, w => w.Contains("读取响应超时,重试"));
            // 超时后整轮重试：Request 位被写 1 至少两次（首轮 + 超时重置后的重试轮）
            int starts = node.Writes.FindAll(w => w.Item1 == addr.RequestAddress && w.Item2 == 1).Count;
            Assert.True(starts >= 2, $"超时到期应触发整轮重试,Request 应写 ≥2 次,实际 {starts} 次");
        }

        // ICW 握手未越超时阈值时不触发重试（对照用例）：
        // 响应脚本在 ResponseTimeout 内返回 Success，不应记录超时重试 WarningPause。
        [Fact]
        public void IcwHandshake_WithinTimeoutNoRetryWarning()
        {
            var node = CreateIcwTestNode();

            // 2 次 None（累积 200ms << ResponseTimeout 5000ms）后 Success，不越过阈值
            node.ResponseScript = new Queue<ushort>(new ushort[]
            {
                (ushort)ICWSyncResultCodeType.None,
                (ushort)ICWSyncResultCodeType.None,
                (ushort)ICWSyncResultCodeType.Success,
            });

            bool ok = node.DoExcute(out string errMsg);

            Assert.True(ok, errMsg);
            // 未越超时阈值 → 不应记录超时重试
            Assert.DoesNotContain(node.Warnings, w => w.Contains("读取响应超时,重试"));
        }

        /// <summary>
        /// 构造可控时钟的 ICW 测试节点（照搬 HandoverICWNodeTests.IcwTestNode 构造模式）：
        /// override DateTimeNow/SyncWait 注入固定时钟 + 寄存器字典 + 响应脚本，
        /// 不依赖 VCommuncation 通信设备装配。
        /// </summary>
        private static IcwRegressionNode CreateIcwTestNode()
        {
            var node = new IcwRegressionNode
            {
                IcwAddress = new IcwHandoverAddress
                {
                    RequestAddress = "1 03 4000 1",
                    ResponseAddress = "1 03 4100 1",
                    HeartbeatAddress = "1 03 3833 1",
                    RecipeAddress = "1 03 4564 1",
                    ModeAddress = "1 03 4565 1",
                },
                RecipeValue = 7,
                ModeValue = 4,
                ResponseTimeout = 5000,
                PollInterval = 100,
                // 心跳周期置 0：避免心跳写入干扰超时判定（WriteHeartbeat 仅 HeartbeatInterval>0 才写）
                HeartbeatInterval = 0,
            };
            // IcwServerDevice 仅需非空以通过 DoExcute 启动自检（读写已被测试子类接管）
            node.IcwServerDevice = new VDevice { Name = "TestIcwServer" };
            return node;
        }

        /// <summary>
        /// 最小 ICW 测试节点子类：override DateTimeNow/SyncWait 注入可控时钟，
        /// override ReadIcwUshort/WriteIcwUshort 用寄存器字典 + 响应脚本，
        /// override StepInfo/WarningPause 接管日志钩子。不依赖 MyOwner。
        /// </summary>
        private class IcwRegressionNode : HandoverICWNode
        {
            private readonly Dictionary<string, ushort> _regs = new Dictionary<string, ushort>();
            public Queue<ushort> ResponseScript = new Queue<ushort>();
            public List<(string, ushort)> Writes = new List<(string, ushort)>();
            public List<string> Warnings = new List<string>();
            private DateTime _clock = new DateTime(2026, 6, 29, 0, 0, 0);

            protected override bool WriteIcwUshort(string address, ushort value)
            {
                if (string.IsNullOrWhiteSpace(address)) return false;
                _regs[address] = value;
                Writes.Add((address, value));
                return true;
            }

            protected override bool ReadIcwUshort(string address, out ushort value)
            {
                value = 0;
                if (string.IsNullOrWhiteSpace(address)) return false;
                // ResponseAddress 走脚本；其余地址回读寄存器字典
                if (address == IcwAddress.ResponseAddress)
                {
                    value = ResponseScript.Count > 0
                        ? ResponseScript.Dequeue()
                        : (ushort)ICWSyncResultCodeType.Success;
                    _regs[address] = value;
                    return true;
                }
                value = _regs.TryGetValue(address, out var v) ? v : (ushort)0;
                return true;
            }

            // 轮询间隙推进时钟（模拟真实等待），驱动超时判定
            protected override void SyncWait(int milliseconds)
            {
                _clock = _clock.AddMilliseconds(milliseconds);
            }

            protected override DateTime DateTimeNow() => _clock;

            // 测试不依赖 MyOwner，接管日志/告警钩子
            protected override void StepInfo(int step, string message) { }
            protected override void WarningPause(string message) { Warnings.Add(message); }
        }
    }
}
