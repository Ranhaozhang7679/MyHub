using Luster.Module.Motion.Safety.Models;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
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
        public void Parse_视觉器件映射为Camera类别()
        {
            var loader = new AlarmMatrixLoader();
            var lines = new[] {
                Header,
                "视觉,A面相机,4,0,CheckFail,相机丢帧,CamLost,CAM_LOST,采集丢帧,Frame lost,检查相机"
            };
            var result = loader.Parse(lines);
            var s = Assert.Single(result);
            Assert.Equal(AlarmCategory.Camera, s.Category);
            Assert.Equal(RecoveryPolicy.Retry, s.RecoveryPolicy);
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

        [Fact]
        public void Import_实现IAlarmMatrixImporter接口()
        {
            IAlarmMatrixImporter importer = new AlarmMatrixLoader();
            var lines = new[] { Header, "马达,X轴,1,0,EL_Fail,X轴正限位,XAxisPEL,AXIS_PEL,正限位,PEL,退回" };
            var loader = (AlarmMatrixLoader)importer;
            var expected = loader.Parse(lines);
            Assert.Single(expected);
            Assert.Equal("AXIS_PEL", expected[0].Code);
        }

        private static AlarmSchema Schema(string code, string message = "", string en = "")
            => new AlarmSchema { Code = code, Message = message, EnglishName = en };

        [Fact]
        public void BuildVAlarms_按Code去重并映射字段()
        {
            var loader = new AlarmMatrixLoader();
            var schemas = new[]
            {
                Schema("EMG", "急停", "Emergency"),
                Schema("AXIS_PEL", "正限位", "PEL"),
                Schema("EMG", "急停重复", "Dup") // 内部重复，应跳过
            };

            var result = loader.BuildVAlarms(schemas, existing: null);

            Assert.Equal(2, result.Count);
            Assert.Equal("EMG", result[0].AlarmKey);
            Assert.Equal("急停", result[0].AlarmCN);
            Assert.Equal("Emergency", result[0].AlarmEn);
            Assert.Equal("急停", result[0].Name);
            Assert.Equal("AXIS_PEL", result[1].AlarmKey);
        }

        [Fact]
        public void BuildVAlarms_跳过引擎已有AlarmKey()
        {
            var loader = new AlarmMatrixLoader();
            var schemas = new[]
            {
                Schema("EMG", "急停", "Emergency"),
                Schema("NEW", "新报警", "New")
            };
            // 引擎已存在 EMG
            var existing = new[] { new VAlarm { AlarmKey = "EMG" } };

            var result = loader.BuildVAlarms(schemas, existing);

            var only = Assert.Single(result);
            Assert.Equal("NEW", only.AlarmKey);
        }

        [Fact]
        public void BuildVAlarms_空Code与null跳过()
        {
            var loader = new AlarmMatrixLoader();
            var schemas = new[]
            {
                Schema("", "空Code"),
                null,
                Schema("OK", "正常", "OK")
            };

            var result = loader.BuildVAlarms(schemas, existing: null);

            var only = Assert.Single(result);
            Assert.Equal("OK", only.AlarmKey);
        }
    }
}
