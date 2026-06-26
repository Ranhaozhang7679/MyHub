using Luster.Module.Motion.Safety.Models;
using Luster.Motion.DataStruct.Enums;
using System;
using Xunit;

namespace Luster.Module.Motion.Recovery.Tests
{
    public class AlarmRaiserTests
    {
        [Fact]
        public void IsCritical_关键异常码识别()
        {
            Assert.True(AlarmRaiser.IsCritical("CAM_FRAME_LOST"));
            Assert.True(AlarmRaiser.IsCritical("EMG_PRESSED"));
            Assert.True(AlarmRaiser.IsCritical("SAFETY_DOOR_OPEN"));
            Assert.True(AlarmRaiser.IsCritical("SERVO_ALARM"));
            Assert.False(AlarmRaiser.IsCritical("NON_CRITICAL"));
        }

        [Fact]
        public void Get_关键异常_策略正确()
        {
            var emg = AlarmRaiser.Get("EMG_PRESSED");
            Assert.NotNull(emg);
            Assert.Equal(AlarmSeverity.Fatal, emg.Severity);
            Assert.Equal(AlarmCategory.Safety, emg.Category);
            Assert.Equal(RecoveryPolicy.Abort, emg.RecoveryPolicy);
            Assert.Equal(LatchPolicy.Latch, emg.LatchPolicy);
            Assert.Equal(SuppressPolicy.None, emg.SuppressPolicy); // 不可静默
        }

        [Fact]
        public void Get_通信断链_锁存待人工()
        {
            var comm = AlarmRaiser.Get("COMM_HANDOVER_BROKEN");
            Assert.Equal(AlarmCategory.Communication, comm.Category);
            Assert.Equal(LatchPolicy.Latch, comm.LatchPolicy);
            Assert.Equal(RecoveryPolicy.Manual, comm.RecoveryPolicy);
        }

        [Fact]
        public void Get_相机丢帧_自动清除可重试()
        {
            var cam = AlarmRaiser.Get("CAM_FRAME_LOST");
            Assert.Equal(AlarmCategory.Camera, cam.Category);
            Assert.Equal(LatchPolicy.AutoClear, cam.LatchPolicy);
            Assert.Equal(RecoveryPolicy.Retry, cam.RecoveryPolicy);
        }

        [Fact]
        public void Wrap_关键异常_强制不可静默()
        {
            var ex = new InvalidOperationException("相机采集超时");
            var schema = AlarmRaiser.Wrap(ex, "CAM_FRAME_LOST", "AOI1相机", "TRACE_001");

            Assert.Equal("CAM_FRAME_LOST", schema.Code);
            Assert.Equal("AOI1相机", schema.Source);
            Assert.Equal("TRACE_001", schema.TraceId);
            // 关键异常强制不可静默
            Assert.Equal(SuppressPolicy.None, schema.SuppressPolicy);
            Assert.Equal(AlarmSeverity.Error, schema.Severity);
        }

        [Fact]
        public void Wrap_未知码_降级为Warning不可静默()
        {
            var ex = new Exception("某异常");
            var schema = AlarmRaiser.Wrap(ex, "UNKNOWN_CODE", "模块X");

            Assert.Equal("UNKNOWN_CODE", schema.Code);
            Assert.Equal(AlarmSeverity.Warning, schema.Severity);
            // 默认也不抑制
            Assert.Equal(SuppressPolicy.None, schema.SuppressPolicy);
        }

        [Fact]
        public void Wrap_P0H必治理项全覆盖()
        {
            // 架构师 P0-H 本期必治理：相机丢帧/通信断链/急停安全门/IO异常/轨迹越界/伺服限位
            string[] mustCover = {
                "CAM_FRAME_LOST", "COMM_HANDOVER_BROKEN", "COMM_MODBUS_TIMEOUT", "COMM_MCNET_BROKEN",
                "EMG_PRESSED", "SAFETY_DOOR_OPEN", "IO_READ_WRITE_FAIL",
                "TRAJ_OUT_OF_BOUNDS", "SERVO_ALARM", "AXIS_LIMIT"
            };
            foreach (var code in mustCover)
            {
                Assert.True(AlarmRaiser.IsCritical(code), $"P0-H 必治理项缺失: {code}");
                var s = AlarmRaiser.Get(code);
                Assert.Equal(SuppressPolicy.None, s.SuppressPolicy); // 全部不可静默
            }
        }

        [Theory]
        [InlineData("EMG_PRESSED", AlarmSeverity.Fatal)]
        [InlineData("SAFETY_DOOR_OPEN", AlarmSeverity.Fatal)]
        [InlineData("SERVO_ALARM", AlarmSeverity.Fatal)]
        [InlineData("AXIS_LIMIT", AlarmSeverity.Fatal)]
        [InlineData("CAM_FRAME_LOST", AlarmSeverity.Error)]
        [InlineData("TRAJ_OUT_OF_BOUNDS", AlarmSeverity.Error)]
        public void 急停安全门伺服限位_Fatal级(string code, AlarmSeverity expectedSev)
        {
            Assert.Equal(expectedSev, AlarmRaiser.Get(code).Severity);
        }
    }
}
