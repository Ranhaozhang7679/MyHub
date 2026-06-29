using System;
using System.Reflection;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Xunit;

namespace Luster.Module.Motion.Regression.Baseline.Tests
{
    /// <summary>
    /// ⑤ 异常超时回归 (TES-165 P9-D / Suite=Timeout)。
    /// 锁定 DeviceTimeoutException 继承体系 + AlarmType.Timeout 存在 +
    /// OverTimeFunction.OverTime 默认值契约（特性 DefaultV=5000ms）。
    /// 纯契约级，无硬件依赖，必 GREEN。
    /// 注：方法名用 ASCII，规避 xunit.runner.visualstudio 3.1.5 对中文方法名的发现不稳定问题。
    /// </summary>
    [Trait("Category", "Regression")]
    [Trait("Suite", "Timeout")]
    public class TimeoutRegressionTests
    {
        // 设备超时异常可被 catch(TimeoutException) 兜底（异常处理链路契约）
        [Fact]
        public void DeviceTimeoutException_InheritsTimeoutException()
        {
            Assert.True(typeof(DeviceTimeoutException).IsSubclassOf(typeof(TimeoutException)));
        }

        [Fact]
        public void DeviceTimeoutException_ConstructsWithAlarmInfo()
        {
            var ex = new DeviceTimeoutException("T1", "轴超时", "ScanAxis", "Axis1");
            Assert.Equal("T1", ex.AlarmCode);
            Assert.Equal("ScanAxis", ex.Module);
            Assert.Equal("Axis1", ex.DeviceName);
            Assert.Equal("轴超时", ex.Message);
            // 可被 TimeoutException 兜底捕获
            TimeoutException baseEx = ex;
            Assert.NotNull(baseEx);
        }

        [Fact]
        public void AlarmType_TimeoutMemberExists()
        {
            Assert.True(Enum.IsDefined(typeof(AlarmType), "Timeout"));
            Assert.Equal(AlarmType.Timeout, (AlarmType)Enum.Parse(typeof(AlarmType), "Timeout"));
        }

        // 反射读取 [Parameter(DefaultV=5000)] 特性，不实例化（避免 MotionFunction 装配依赖）
        [Fact]
        public void OverTimeFunction_OverTimeDefaultContract5000ms()
        {
            var prop = typeof(OverTimeFunction).GetProperty("OverTime");
            Assert.NotNull(prop);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();
            Assert.NotNull(attr);
            Assert.Equal(5000, (int)attr.DefaultV);
        }

        // 状态机超时终态契约（与 ① 呼应，锁 Timeout 分支）
        [Fact]
        public void RunStatus_TimeoutStateExists()
        {
            Assert.True(Enum.IsDefined(typeof(RunStatus), "TimeOut"));
            Assert.Equal(RunStatus.TimeOut, (RunStatus)5);
        }
    }
}
