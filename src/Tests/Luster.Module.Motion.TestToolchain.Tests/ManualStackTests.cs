using DC.Authorization.Models;
using Luster.Module.Motion.TestToolchain;
using Luster.Module.Motion.TestToolchain.Manual;
using Luster.Motion.TaskFlow.Engine.Models;
using System.Xml.Linq;
using Xunit;

namespace Luster.Module.Motion.TestToolchainTests
{
    /// <summary>
    /// TES-34 P9-A/B 单测：DebugProfile 默认值/复制/XML 往返 + ManualStack 回退栈语义 +
    /// ManualOutput/Motor/MotorGroup Backup 委托 + TestAuthItems 权限项。
    /// 软件层纯逻辑测试（不注入权限时放行，对齐 RecipeManagerLogicTests）。
    /// </summary>
    public class ManualStackTests
    {
        #region DebugProfile

        [Fact]
        public void DebugProfile_默认值对齐源端()
        {
            var dp = new DebugProfile();
            Assert.True(dp.SingleMode);            // 源端 ctor 默认 true
            Assert.False(dp.RunWithProduct);
            Assert.False(dp.RunWithICW);
            Assert.False(dp.IsCalibritionMode);
            Assert.False(dp.IsCalibritionSave);
            Assert.False(dp.EnableHandShakeSafe); // legacy 死字段
            Assert.False(dp.LoadMCEnable);        // legacy 死字段
        }

        [Fact]
        public void DebugProfile_CopyFrom_逐字段复制()
        {
            var src = new DebugProfile
            {
                SingleMode = false, RunWithProduct = true, RunWithICW = true,
                IsCalibritionMode = true, IsCalibritionSave = true,
                EnableHandShakeSafe = true, LoadMCEnable = true
            };
            var dst = new DebugProfile();
            dst.CopyFrom(src);
            Assert.False(dst.SingleMode);
            Assert.True(dst.RunWithProduct);
            Assert.True(dst.IsCalibritionSave);
            Assert.True(dst.LoadMCEnable);
        }

        [Fact]
        public void DebugProfile_Xml往返_保留字段()
        {
            var dp = new DebugProfile
            {
                SingleMode = false, RunWithProduct = true, RunWithICW = true, IsCalibritionMode = true
            };
            XElement xml = dp.ExportXml();
            var dp2 = new DebugProfile();
            dp2.ParserXml(xml);
            Assert.False(dp2.SingleMode);
            Assert.True(dp2.RunWithProduct);
            Assert.True(dp2.RunWithICW);
            Assert.True(dp2.IsCalibritionMode);
        }

        [Fact]
        public void SystemConfig_DebugSetting挂载Xml往返()
        {
            // 验证 P9-A：DebugProfile 挂载 SystemConfig 软件配置区后持久化往返
            var sc = new SystemConfig
            {
                DebugSetting = new DebugProfile { SingleMode = false, RunWithProduct = true }
            };
            var xml = sc.ExportXml();
            Assert.NotNull(xml.Element("DebugProfile"));
            var sc2 = new SystemConfig();
            sc2.ParserXml(xml);
            Assert.False(sc2.DebugSetting.SingleMode);
            Assert.True(sc2.DebugSetting.RunWithProduct);
        }

        #endregion

        #region ManualStack

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
        public void ManualStack_空栈可启动自动()
        {
            var stack = new ManualStack();
            Assert.Equal(0, stack.Count);
            Assert.True(stack.CanStartAuto);
        }

        [Fact]
        public void ManualStack_记录后栈非空阻止自动启动()
        {
            var stack = new ManualStack();
            stack.RecordIf(true, new FakeOp("A"));
            Assert.Equal(1, stack.Count);
            Assert.False(stack.CanStartAuto);
        }

        [Fact]
        public void ManualStack_同设备去重折叠()
        {
            // 对齐源端 AddManual：连续同 ComponentKey 折叠，保留最早态
            var stack = new ManualStack();
            stack.RecordIf(true, new FakeOp("A"));
            stack.RecordIf(true, new FakeOp("A"));
            Assert.Equal(1, stack.Count);
        }

        [Fact]
        public void ManualStack_操作失败不入栈_修复源端bug()
        {
            // 源端 MotorGroupComponent.ManualOperate 漏 result&& 致运动失败也入栈，本栈修正
            var stack = new ManualStack();
            stack.RecordIf(false, new FakeOp("A"));
            Assert.Equal(0, stack.Count);
        }

        [Fact]
        public void ManualStack_RemoveLast回退栈顶()
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
        public void ManualStack_RemoveLast空栈complete为true()
        {
            var stack = new ManualStack();
            bool complete;
            bool ok = stack.RemoveLast(out complete);
            Assert.True(ok);
            Assert.True(complete);
        }

        [Fact]
        public void ManualStack_Backup失败中止出栈()
        {
            var stack = new ManualStack();
            var op = new FakeOp("A") { BackupResult = false };
            stack.RecordIf(true, op);
            bool complete;
            bool ok = stack.RemoveLast(out complete);
            Assert.False(ok);           // 回退失败
            Assert.False(complete);
            Assert.Equal(1, stack.Count); // 栈顶保留（对齐源端 RemoveLast）
        }

        [Fact]
        public void ManualStack_RemoveAll逐条回退()
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

        [Fact]
        public void ManualStack_Clear不回退()
        {
            var stack = new ManualStack();
            var op = new FakeOp("A");
            stack.RecordIf(true, op);
            stack.Clear();
            Assert.Equal(0, stack.Count);
            Assert.Equal(0, op.BackupCalls); // Clear 不调 Backup（对齐源端 ClearManualStack）
        }

        [Fact]
        public void ManualStack_GetSnapshot返回拷贝()
        {
            var stack = new ManualStack();
            stack.RecordIf(true, new FakeOp("A"));
            var snap = stack.GetSnapshot();
            Assert.Single(snap);
            stack.Clear();
            Assert.Single(snap); // 快照不受原栈变化影响（对齐源端 GetManualStack 拷贝）
        }

        [Fact]
        public void ManualStack_未注入权限放行()
        {
            // 纯逻辑测试：auth=null 时放行（对齐 RecipeManager.EvaluatePermission）
            var stack = new ManualStack(null);
            Assert.True(stack.RecordIf(true, new FakeOp("A")));
        }

        #endregion

        #region ManualOutput / ManualMotor / ManualMotorGroup

        [Fact]
        public void ManualOutput_Backup调用恢复委托()
        {
            bool restoredValue = false;
            bool called = false;
            var op = new ManualOutput("IO1", true, v => { called = true; restoredValue = v; });
            Assert.True(op.Backup(out _));
            Assert.True(called);
            Assert.True(restoredValue);
            Assert.Equal("IO1", op.ComponentKey);
            Assert.True(op.LastStatus);
        }

        [Fact]
        public void ManualMotor_Backup调用恢复委托()
        {
            double restoredPosi = 0;
            var op = new ManualMotor("Axis1", 12.5, p => restoredPosi = p);
            Assert.True(op.Backup(out _));
            Assert.Equal(12.5, restoredPosi);
            Assert.Equal(12.5, op.LastPosi);
        }

        [Fact]
        public void ManualMotorGroup_Backup调用恢复委托()
        {
            double[] restored = null;
            var op = new ManualMotorGroup("Group1", new double[] { 1, 2, 3 }, p => restored = p);
            Assert.True(op.Backup(out _));
            Assert.Equal(new double[] { 1, 2, 3 }, restored);
        }

        #endregion

        #region TestAuthItems

        [Fact]
        public void TestAuthItems_ToRights覆盖4项()
        {
            Right[] rights = TestAuthItems.ToRights();
            Assert.Equal(4, rights.Length);
            Assert.All(rights, r => Assert.Equal(RightType.Operation, r.Type));
            Assert.Equal("测试工具链", rights[0].ModuleName);
        }

        [Fact]
        public void TestAuthItems_All包含手动操作与回退()
        {
            Assert.Contains(TestAuthItems.All, a => a.Operation == "手动操作");
            Assert.Contains(TestAuthItems.All, a => a.Operation == "手动回退");
        }

        #endregion
    }
}
