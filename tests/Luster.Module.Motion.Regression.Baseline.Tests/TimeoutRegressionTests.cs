using System;
using System.Reflection;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.SimDevice.Engine;
using Luster.Motion.DataStruct.DataModels;
using System.Diagnostics;
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

        /// <summary>
        /// 信号级真实超时：DeviceEngine(Virtual)+VIO{Behavior=Input,Value=0} 注册后，
        /// WaitIO(true,200ms) 永不满足 → CalcTime 超时抛 DeviceTimeoutException。
        /// Stopwatch 锁 5s 上限(防挂起)/100ms 下限(确认真实等待非瞬时)。
        /// 注：真实链路为 VIO.WaitIO→CalcTime→DeviceTimeoutException；
        /// "DoExcute→OnAlarm(FailError)→return false" 在当前源码不存在
        /// (运行器层 MotionModule 用 AlarmType.Timeout+RunStatus.Alarmed/Error)。
        /// </summary>
        [Fact]
        public void VIO_WaitIO_RealTimeout_ThrowsDeviceTimeoutException_WithinBounds()
        {
            var engine = new DeviceEngine();                       // DeviceMode 默认 Virtual
            var vio = new VIO
            {
                ID = Guid.NewGuid(),
                Name = "vioTimeout",
                Behavior = IOBehavior.Input,
                Value = 0
            };
            engine.AddVirtual(vio);                                // 内部设 vio.Engine=engine、Mode=Virtual；DeviceID 留 Empty 故 motionCard 不绑定

            Assert.False(vio.GetDigital());                        // Value=0 → 虚拟分支 Value>0 = false，永不满足

            var sw = Stopwatch.StartNew();
            var ex = Assert.Throws<DeviceTimeoutException>(() => vio.WaitIO(true, 200));  // timeout=200ms，timeAction=null → CalcTime 超时抛
            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds >= 100, $"下限未达: {sw.ElapsedMilliseconds}ms");
            Assert.True(sw.ElapsedMilliseconds <= 5000, $"上限超出(疑似挂起): {sw.ElapsedMilliseconds}ms");
            Assert.Equal("N03OOOO-01", ex.AlarmCode);
        }
    }
}
