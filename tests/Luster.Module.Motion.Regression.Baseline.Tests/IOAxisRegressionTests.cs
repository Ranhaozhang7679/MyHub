using Luster.Module.Motion.TestToolchain.Manual;
using Xunit;

namespace Luster.Module.Motion.Regression.Baseline.Tests
{
    /// <summary>
    /// ③ 关键 IO 轴动作回归 (TES-165 P9-D / Suite=IOAxis)。
    /// 锁定 ManualStack 回退栈语义：RecordIf(成功才入栈)/同设备去重/RemoveLast/RemoveAll/
    /// CanStartAuto 互锁/Clear。复刻 ManualStackTests 核心断言（原工程该文件被 &lt;Compile Remove&gt;
    /// 禁用，本工程独立编译，引用 TestToolchain 拿 ManualStack 本体纯逻辑）。
    /// 无硬件依赖，必 GREEN。
    /// 注：方法名用 ASCII，规避 xunit.runner.visualstudio 3.1.5 对中文方法名的发现不稳定问题。
    /// </summary>
    [Trait("Category", "Regression")]
    [Trait("Suite", "IOAxis")]
    public class IOAxisRegressionTests
    {
        /// <summary>测试用 IManualOperation 桩，记录 Backup 调用</summary>
        private sealed class FakeOp : IManualOperation
        {
            private readonly string _key;
            public bool BackupResult = true;
            public int BackupCalls;
            public FakeOp(string key) { _key = key; }
            public string ComponentKey => _key;
            public bool Backup(out string msg) { BackupCalls++; msg = "ok"; return BackupResult; }
            public string ToDetailString() => $"key={_key}";
        }

        [Fact]
        public void ManualStack_EmptyStackCanStartAuto()
        {
            var stack = new ManualStack();
            Assert.Equal(0, stack.Count);
            Assert.True(stack.CanStartAuto);
        }

        [Fact]
        public void ManualStack_RecordsThenBlocksAutoStart()
        {
            var stack = new ManualStack();
            Assert.True(stack.RecordIf(true, new FakeOp("A")));
            Assert.Equal(1, stack.Count);
            Assert.False(stack.CanStartAuto);
        }

        // 源端 MotorGroupComponent.ManualOperate 漏 result&& 致运动失败也入栈，本栈修正
        [Fact]
        public void ManualStack_FailedOpNotPushed_FixesSourceBug()
        {
            var stack = new ManualStack();
            Assert.False(stack.RecordIf(false, new FakeOp("A")));
            Assert.Equal(0, stack.Count);
        }

        // 对齐源端 AddManual：连续同 ComponentKey 折叠，保留最早态
        [Fact]
        public void ManualStack_SameDeviceDedupFold()
        {
            var stack = new ManualStack();
            stack.RecordIf(true, new FakeOp("A"));
            stack.RecordIf(true, new FakeOp("A"));
            Assert.Equal(1, stack.Count);
        }

        [Fact]
        public void ManualStack_RemoveLastPopsTop()
        {
            var stack = new ManualStack();
            var op = new FakeOp("A");
            stack.RecordIf(true, op);
            bool complete;
            bool ok = stack.RemoveLast(out complete);
            Assert.True(ok);
            Assert.True(complete);
            Assert.Equal(0, stack.Count);
            Assert.Equal(1, op.BackupCalls);
        }

        [Fact]
        public void ManualStack_RemoveLastEmptyStackCompleteTrue()
        {
            var stack = new ManualStack();
            bool complete;
            bool ok = stack.RemoveLast(out complete);
            Assert.True(ok);
            Assert.True(complete);
        }

        [Fact]
        public void ManualStack_BackupFailureAbortsPop()
        {
            var stack = new ManualStack();
            var op = new FakeOp("A") { BackupResult = false };
            stack.RecordIf(true, op);
            bool complete;
            bool ok = stack.RemoveLast(out complete);
            Assert.False(ok);             // 回退失败
            Assert.False(complete);
            Assert.Equal(1, stack.Count); // 栈顶保留（对齐源端 RemoveLast）
        }

        [Fact]
        public void ManualStack_RemoveAllPopsEach()
        {
            var stack = new ManualStack();
            var op1 = new FakeOp("A");
            var op2 = new FakeOp("B");
            stack.RecordIf(true, op1);
            stack.RecordIf(true, op2);
            int remain = stack.RemoveAll();
            Assert.Equal(0, remain);
            Assert.Equal(1, op1.BackupCalls);
            Assert.Equal(1, op2.BackupCalls);
        }

        // Clear 不调 Backup（对齐源端 ClearManualStack）
        [Fact]
        public void ManualStack_ClearDoesNotBackup()
        {
            var stack = new ManualStack();
            var op = new FakeOp("A");
            stack.RecordIf(true, op);
            stack.Clear();
            Assert.Equal(0, stack.Count);
            Assert.Equal(0, op.BackupCalls);
        }
    }
}
