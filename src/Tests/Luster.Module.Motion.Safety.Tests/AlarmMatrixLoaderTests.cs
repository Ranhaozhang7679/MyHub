using Luster.Module.Motion.Safety.Models;
using Luster.Motion.DataStruct.Enums;
using System.Linq;
using Xunit;

namespace Luster.Module.Motion.Safety.Tests
{
    public class AlarmMatrixLoaderTests
    {
        // 源端 ErrCode.csv 11 列 schema：
        // 器件类型, 位置类型名, Index, unused, ResultType, Name, EName, Code, Description, EDescription, Suggestion
        private const string Header = "器件类型,位置类型名,Index,unused,ResultType,Name,EName,Code,Description,EDescription,Suggestion";

        [Fact]
        public void Parse_跳过表头与缺列行()
        {
            var loader = new AlarmMatrixLoader();
            var lines = new[] {
                Header,
                "马达,X轴,1,0,Alarm_Fail,X轴伺服报警,XAxisAlarm,AXIS_ALARM,伺服报警,Servo alarm,检查驱动器",
                "短行,缺列"   // 缺列，应跳过
            };
            var result = loader.Parse(lines);
            Assert.Single(result);
            Assert.Equal(1, loader.LastSkipped); // 仅缺列行计入跳过
        }

        [Fact]
        public void Parse_字段映射正确()
        {
            var loader = new AlarmMatrixLoader();
            var lines = new[] {
                Header,
                "马达,X轴,1,0,EL_Fail,X轴正限位,XAxisPEL,AXIS_PEL,正限位触发,PEL trigger,手动退回"
            };
            var result = loader.Parse(lines);
            var s = Assert.Single(result);

            Assert.Equal("AXIS_PEL", s.Code);
            Assert.Equal("X轴正限位", s.Message);
            Assert.Equal("XAxisPEL", s.EnglishName);
            Assert.Equal("正限位触发", s.Description);
            Assert.Equal("手动退回", s.Suggestion);
            Assert.Equal("马达/X轴#1", s.Source);
            // EL_Fail → Fatal + Abort + Latch
            Assert.Equal(AlarmSeverity.Fatal, s.Severity);
            Assert.Equal(RecoveryPolicy.Abort, s.RecoveryPolicy);
            Assert.Equal(LatchPolicy.Latch, s.LatchPolicy);
            Assert.Equal(AlarmType.DeviceError, s.PlatformAlarmType);
        }

        [Fact]
        public void Parse_空Code行跳过()
        {
            var loader = new AlarmMatrixLoader();
            var lines = new[] {
                Header,
                "气缸,A面,2,0,ActionFail,气缸不到位,CylNotIn,,动作超时,Timeout,检查气缸"  // Code 空
            };
            var result = loader.Parse(lines);
            Assert.Empty(result);
            Assert.Equal(1, loader.LastSkipped);
        }

        [Fact]
        public void Parse_TimeOut映射为重试()
        {
            var loader = new AlarmMatrixLoader();
            var lines = new[] {
                Header,
                "真空,A面,3,0,TimeOut,真空超时,VacTimeout,VAC_TO,抽真空超时,Vacuum timeout,检查真空"
            };
            var result = loader.Parse(lines);
            var s = Assert.Single(result);
            Assert.Equal(AlarmSeverity.Error, s.Severity);
            Assert.Equal(RecoveryPolicy.Retry, s.RecoveryPolicy);
            Assert.Equal(AlarmType.Timeout, s.PlatformAlarmType);
            Assert.Equal(LatchPolicy.AutoClear, s.LatchPolicy);
        }

        [Fact]
        public void Parse_多行按序加载()
        {
            var loader = new AlarmMatrixLoader();
            var lines = new[] {
                Header,
                "马达,X轴,1,0,EL_Fail,X轴正限位,XAxisPEL,AXIS_PEL,正限位,PEL,退回",
                "数字量输入,急停,2,0,Alarm_Fail,急停按下,Emergency,EMG,急停,EMG pressed,复位",
                "通讯,PLC,3,0,TimeOut,PLC断线,PlcLost,PLC_LOST,通信断链,Comm lost,检查网线"
            };
            var result = loader.Parse(lines);
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] { "AXIS_PEL", "EMG", "PLC_LOST" }, result.Select(s => s.Code).ToArray());
        }
    }
}
