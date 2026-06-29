using System;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Enums;
using Xunit;

namespace Luster.Module.Motion.Regression.Baseline.Tests
{
    /// <summary>
    /// ① 状态机契约回归 (TES-165 P9-D / Suite=StateMachine)。
    /// 锁定 RunStatus / DeviceMode 枚举成员与值，防止迁移/重构过程中枚举被改名/删值导致回归。
    /// 纯契约级，无硬件依赖，必 GREEN。
    /// 注：方法名用 ASCII，规避 xunit.runner.visualstudio 3.1.5 对中文方法名的发现不稳定问题。
    /// </summary>
    [Trait("Category", "Regression")]
    [Trait("Suite", "StateMachine")]
    public class StateMachineContractTests
    {
        // RunStatus 枚举 9 个状态，值固定（防改名/删值）
        [Fact]
        public void RunStatus_MembersCompleteAndValuesMatch()
        {
            Assert.Equal(9, Enum.GetNames(typeof(RunStatus)).Length);
            Assert.Equal(RunStatus.Default, (RunStatus)0);
            Assert.Equal(RunStatus.Running, (RunStatus)1);
            Assert.Equal(RunStatus.Success, (RunStatus)2);
            Assert.Equal(RunStatus.Error, (RunStatus)3);
            Assert.Equal(RunStatus.Alarmed, (RunStatus)4);
            Assert.Equal(RunStatus.TimeOut, (RunStatus)5);
            Assert.Equal(RunStatus.Skip, (RunStatus)6);
            Assert.Equal(RunStatus.Pause, (RunStatus)7);
            Assert.Equal(RunStatus.Stop, (RunStatus)8);
        }

        // 名称契约：防误改名（如 TimeOut 被改成 Timeout 导致序列化/反射回归）
        [Fact]
        public void RunStatus_KeyNamesExistAgainstRename()
        {
            Assert.Contains("Default", Enum.GetNames(typeof(RunStatus)));
            Assert.Contains("TimeOut", Enum.GetNames(typeof(RunStatus)));
            Assert.Contains("Alarmed", Enum.GetNames(typeof(RunStatus)));
            Assert.Contains("Pause", Enum.GetNames(typeof(RunStatus)));
            Assert.Contains("Stop", Enum.GetNames(typeof(RunStatus)));
        }

        // 设备模式 5 个，Virtual 默认值 0（离线仿真基线）
        [Fact]
        public void DeviceMode_MembersCompleteAndValuesMatch()
        {
            Assert.Equal(5, Enum.GetNames(typeof(DeviceMode)).Length);
            Assert.Equal(DeviceMode.Virtual, (DeviceMode)0);
            Assert.Equal(DeviceMode.Real, (DeviceMode)1);
            Assert.Equal(DeviceMode.Empty, (DeviceMode)2);
            Assert.Equal(DeviceMode.Project, (DeviceMode)3);
            Assert.Equal(DeviceMode.Debug, (DeviceMode)4);
        }

        // TODO(TES-165): MotionRunEngine.Run() / Timeout() 状态机驱动路径
        // 依赖设备引擎装配（IMotionController/站级装配），属系统集成测试范畴，本契约集不实例化。
        // PhotoHandshake 状态机已由 Production.Tests 覆盖（① 相邻），MotionRunEngine 的步进/超时
        // 分支回归待软硬件联调阶段补。
    }
}
