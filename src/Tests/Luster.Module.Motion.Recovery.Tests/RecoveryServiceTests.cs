using Luster.Motion.DataStruct.Checkpoint;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using Xunit;

namespace Luster.Module.Motion.Recovery.Tests
{
    public class RecoveryServiceTests
    {
        /// <summary>可编程 IInputSnapshot</summary>
        private sealed class FuncSnapshot : IInputSnapshot
        {
            private readonly Func<SafetyInputKind, string, bool> _f;
            public FuncSnapshot(Func<SafetyInputKind, string, bool> f) { _f = f; }
            public bool IsTriggered(SafetyInputKind kind, string target = null) => _f(kind, target);
        }

        private static InterlockMatrix MatrixWithFatal(bool fatal)
        {
            // fatal=true 时加一条 Abort 级规则（EStop 触发）
            var rules = new List<InterlockRule>();
            if (fatal)
            {
                rules.Add(new InterlockRule
                {
                    RuleId = "EMG",
                    AlarmCode = "EMG_PRESSED",
                    Recovery = RecoveryPolicy.Abort,
                    Inputs = new[] { new InterlockInput { Kind = SafetyInputKind.EStop, Target = "e1" } }
                });
            }
            return new InterlockMatrix(rules);
        }

        private static RunCheckpoint MakeCp(int productCount = 2, int actionIndex = 5, bool traceWritten = true)
        {
            var sns = new List<string>();
            for (int i = 0; i < productCount; i++) sns.Add("SN" + i);
            return new RunCheckpoint(sns.AsReadOnly(), "AOI1", "检测", actionIndex,
                null, null, traceWritten, DateTime.UtcNow);
        }

        [Fact]
        public void Recover_安全联锁致命触发_拒绝恢复()
        {
            var svc = new RecoveryService();
            var matrix = MatrixWithFatal(fatal: true);
            // EStop 触发
            var snap = new FuncSnapshot((k, t) => k == SafetyInputKind.EStop);

            var result = svc.Recover(MakeCp(), RecoveryStrategy.Resume, matrix, snap);

            Assert.False(result.Success);
            Assert.Contains("EMG_PRESSED", result.AlarmCodes);
            Assert.Contains("安全联锁仍触发", result.Message);
        }

        [Fact]
        public void Recover_无checkpoint_只能清机()
        {
            var svc = new RecoveryService();
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);

            var r1 = svc.Recover(null, RecoveryStrategy.ClearMachine, matrix, snap);
            Assert.True(r1.Success);

            var r2 = svc.Recover(null, RecoveryStrategy.Resume, matrix, snap);
            Assert.False(r2.Success);
            Assert.Contains("无 checkpoint", r2.Message);
        }

        [Fact]
        public void Recover_续跑_返回断点actionIndex()
        {
            var svc = new RecoveryService();
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 2, actionIndex: 7);

            var result = svc.Recover(cp, RecoveryStrategy.Resume, matrix, snap);

            Assert.True(result.Success);
            Assert.Equal(7, result.ResumedActionIndex);
            Assert.Contains("action#7", result.Message);
        }

        [Fact]
        public void Recover_续跑_无在籍产品_失败()
        {
            var svc = new RecoveryService();
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 0, actionIndex: 3);

            var result = svc.Recover(cp, RecoveryStrategy.Resume, matrix, snap);

            Assert.False(result.Success);
            Assert.Contains("无在籍产品", result.Message);
        }

        [Fact]
        public void Recover_续跑_追溯未写_补写提示()
        {
            var svc = new RecoveryService();
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 1, actionIndex: 4, traceWritten: false);

            var result = svc.Recover(cp, RecoveryStrategy.Resume, matrix, snap);

            Assert.True(result.Success);
            Assert.Contains("补写追溯", result.Message);
        }

        [Fact]
        public void Recover_清机_成功且actionIndex归零()
        {
            var svc = new RecoveryService();
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 3, actionIndex: 9);

            var result = svc.Recover(cp, RecoveryStrategy.ClearMachine, matrix, snap);

            Assert.True(result.Success);
            Assert.Equal(0, result.ResumedActionIndex);
            Assert.Contains("3 件", result.Message);
        }

        [Fact]
        public void Recover_报废_成功()
        {
            var svc = new RecoveryService();
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 2, actionIndex: 5);

            var result = svc.Recover(cp, RecoveryStrategy.ScrapCurrent, matrix, snap);

            Assert.True(result.Success);
            Assert.Contains("报废", result.Message);
        }

        [Fact]
        public void Recover_verifier注入_产品不在位_续跑失败()
        {
            var verifier = new FailVerifier(productsInPlace: false);
            var svc = new RecoveryService(verifier);
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 1, actionIndex: 3);

            var result = svc.Recover(cp, RecoveryStrategy.Resume, matrix, snap);

            Assert.False(result.Success);
            Assert.Contains("PRODUCT_NOT_IN_PLACE", result.AlarmCodes);
        }

        [Fact]
        public void Recover_verifier注入_轴位不符_续跑失败()
        {
            var verifier = new FailVerifier(axisAtSafe: false);
            var svc = new RecoveryService(verifier);
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 1, actionIndex: 3);

            var result = svc.Recover(cp, RecoveryStrategy.Resume, matrix, snap);

            Assert.False(result.Success);
            Assert.Contains("AXIS_NOT_AT_SAFE", result.AlarmCodes);
        }

        [Fact]
        public void Recover_verifier注入_轴位不符_清机也失败()
        {
            // 清机也要求轴回安全位
            var verifier = new FailVerifier(axisAtSafe: false);
            var svc = new RecoveryService(verifier);
            var matrix = MatrixWithFatal(fatal: false);
            var snap = new FuncSnapshot((k, t) => false);
            var cp = MakeCp(productCount: 1, actionIndex: 3);

            var result = svc.Recover(cp, RecoveryStrategy.ClearMachine, matrix, snap);

            Assert.False(result.Success);
            Assert.Contains("AXIS_NOT_AT_SAFE", result.AlarmCodes);
        }

        private sealed class FailVerifier : IRecoveryVerifier
        {
            private readonly bool _productsInPlace;
            private readonly bool _axisAtSafe;
            public FailVerifier(bool productsInPlace = true, bool axisAtSafe = true)
            { _productsInPlace = productsInPlace; _axisAtSafe = axisAtSafe; }
            public bool VerifyProductsInPlace(IReadOnlyList<string> productSNs) => _productsInPlace;
            public bool VerifyAxisAtSafePosition(AxisSafePosition expected) => _axisAtSafe;
            public bool VerifyHandoverState(HandoverStateSnapshot expected) => true;
        }
    }
}
