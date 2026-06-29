using System;
using Luster.Module.Motion.Handover;
using Xunit;

namespace Luster.Module.Motion.Regression.Baseline.Tests
{
    /// <summary>
    /// ④ 握手信号回归 (TES-165 P9-D / Suite=Handshake)。
    /// 锁定 HandoverNode 状态机契约常量（StepDone/StepAbort）+ HandoverRole 角色枚举。
    /// 信号读写边沿检测依赖 VCommuncation 通信设备装配，属集成测试范畴，本集只锁契约。
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

        // TODO(TES-165): HandoverAutoSignalService 边沿检测（信号上升沿/下降沿判定）
        // 依赖 VCommuncation 实例 + 信号地址表装配，属软硬件联调测试；
        // Feed/Leave/ICW 状态机步进已由 Handover.Tests 覆盖（最完整），本集只锁契约常量。
    }
}
