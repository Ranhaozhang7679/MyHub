using Luster.Motion.DataStruct.Enums;
using Luster.Module.Motion.Safety.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Luster.Module.Motion.Safety
{
    /// <summary>
    /// 报警矩阵 CSV 导入契约（ADR-A seam）。
    /// TES-38 产出，供 TES-28 / ErrorManager 配置化加载报警矩阵。
    /// </summary>
    public interface IAlarmMatrixImporter
    {
        /// <summary>从 CSV 导入报警矩阵为 <see cref="AlarmSchema"/> 列表</summary>
        IReadOnlyList<AlarmSchema> Import(string csvPath);
    }

    /// <summary>
    /// 报警矩阵 CSV 配置化导入（TES-38，实现 <see cref="IAlarmMatrixImporter"/>）。
    /// 对齐源端 <c>CheckStationTask.GetAlarmList</c> 读取 <c>data\ErrCode.csv</c> 的 11 列 schema：
    /// 器件类型, 位置类型名, Index, &lt;unused&gt;, ResultType, Name, EName, Code, Description, EDescription, Suggestion
    /// 解析为 <see cref="AlarmSchema"/> 列表。
    /// </summary>
    /// <remarks>
    /// 源端用 <c>Encoding.Default</c>（GBK）读取，本实现同样默认 GBK 以兼容既有 CSV；
    /// 缺列/空行/空 Code 跳过并计入 <see cref="LastSkipped"/> 便于排查。
    /// 共享枚举（<see cref="AlarmSeverity"/>/<see cref="RecoveryPolicy"/> 等）来自框架层。
    /// </remarks>
    public class AlarmMatrixLoader : IAlarmMatrixImporter
    {
        /// <summary>CSV 列数（源端 schema）</summary>
        public const int ExpectedColumnCount = 11;

        /// <summary>上次解析跳过的行数（含空行/缺列/空 Code，不含表头）</summary>
        public int LastSkipped { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<AlarmSchema> Import(string csvPath) => Load(csvPath);

        /// <summary>
        /// 从文件加载报警矩阵。
        /// </summary>
        /// <param name="path">CSV 路径，绝对路径</param>
        /// <param name="encoding">编码，默认 GBK（Encoding.Get("GB18030")）</param>
        public List<AlarmSchema> Load(string path, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException($"报警矩阵 CSV 不存在：{path}", path);

            encoding = encoding ?? Encoding.GetEncoding("GB18030");
            var lines = File.ReadAllLines(path, encoding);
            return Parse(lines);
        }

        /// <summary>解析已读取的行（便于单测注入）</summary>
        public List<AlarmSchema> Parse(IEnumerable<string> lines)
        {
            LastSkipped = 0;
            var result = new List<AlarmSchema>();
            if (lines == null) return result;

            bool headerSeen = false;
            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) { LastSkipped++; continue; }
                var line = raw.Trim();
                var cols = line.Split(new[] { ',' }, StringSplitOptions.None);

                // 表头：器件类型
                if (!headerSeen && cols.Length > 0 && cols[0].Trim() == "器件类型")
                {
                    headerSeen = true;
                    continue;
                }

                if (cols.Length < ExpectedColumnCount)
                {
                    LastSkipped++;
                    continue;
                }

                // cols[0]=器件类型, cols[1]=位置类型名, cols[2]=Index, cols[3]=unused,
                // cols[4]=ResultType, cols[5]=Name, cols[6]=EName, cols[7]=Code,
                // cols[8]=Description, cols[9]=EDescription, cols[10]=Suggestion
                var schema = new AlarmSchema
                {
                    Source = $"{cols[0].Trim()}/{cols[1].Trim()}#{cols[2].Trim()}",
                    Code = cols[7].Trim(),
                    Message = cols[5].Trim(),
                    EnglishName = cols[6].Trim(),
                    Description = cols[8].Trim(),
                    Suggestion = cols[10].Trim(),
                    Category = MapCategory(cols[0].Trim()),
                    Severity = MapSeverity(cols[4].Trim()),
                    RecoveryPolicy = MapRecovery(cols[4].Trim()),
                    PlatformAlarmType = MapAlarmType(cols[4].Trim()),
                    PlatformAlarmProc = MapAlarmProc(cols[4].Trim()),
                    LatchPolicy = MapLatch(cols[4].Trim()),
                    SuppressPolicy = SuppressPolicy.None
                };

                if (string.IsNullOrEmpty(schema.Code))
                {
                    LastSkipped++;
                    continue;
                }
                result.Add(schema);
            }
            return result;
        }

        /// <summary>源端器件类型 → AlarmCategory</summary>
        private static AlarmCategory MapCategory(string deviceType)
        {
            switch (deviceType)
            {
                case "马达":
                case "马达组合":
                case "IO步进":
                case "编码器":
                    return AlarmCategory.Device;
                case "通讯":
                    return AlarmCategory.Communication;
                case "视觉":
                    return AlarmCategory.Camera;
                case "数字量输入":
                case "数字量输出":
                case "输入输出组合":
                case "气缸":
                case "真空":
                case "模拟量输入":
                case "模拟量输出":
                case "光源":
                case "低速锁存":
                case "高速锁存":
                case "高速比较":
                case "扫码枪":
                case "机械手":
                default:
                    return AlarmCategory.Device;
            }
        }

        /// <summary>源端 SyncResultType → AlarmSeverity</summary>
        private static AlarmSeverity MapSeverity(string resultType)
        {
            switch (resultType)
            {
                case "Success":
                case "NoAlarmFail":
                    return AlarmSeverity.Info;
                case "Warning":
                case "BeyongExpectationApproach":
                case "OutOfServiceTimeApproach":
                    return AlarmSeverity.Warning;
                case "EL_Fail":
                case "NEL_Fail":
                case "PEL_Fail":
                case "Alarm_Fail":
                case "Origin_Lost":
                    return AlarmSeverity.Fatal;
                default:
                    return AlarmSeverity.Error;
            }
        }

        /// <summary>源端 SyncResultType → RecoveryPolicy</summary>
        private static RecoveryPolicy MapRecovery(string resultType)
        {
            switch (resultType)
            {
                case "Success":
                case "NoAlarmFail":
                    return RecoveryPolicy.None;
                case "ActionFail":
                case "CheckFail":
                case "CheckAction_Fail":
                    return RecoveryPolicy.Retry;
                case "TimeOut":
                    return RecoveryPolicy.Retry;
                case "EL_Fail":
                case "NEL_Fail":
                case "PEL_Fail":
                case "Alarm_Fail":
                case "Origin_Lost":
                    return RecoveryPolicy.Abort;
                default:
                    return RecoveryPolicy.Manual;
            }
        }

        /// <summary>源端 SyncResultType → 平台 AlarmType</summary>
        private static AlarmType MapAlarmType(string resultType)
        {
            switch (resultType)
            {
                case "Success":
                case "NoAlarmFail":
                    return AlarmType.InfoTip;
                case "Warning":
                    return AlarmType.WarningTip;
                case "TimeOut":
                    return AlarmType.Timeout;
                case "EL_Fail":
                case "NEL_Fail":
                case "PEL_Fail":
                case "Alarm_Fail":
                    return AlarmType.DeviceError;
                case "Origin_Lost":
                    return AlarmType.HomeError;
                default:
                    return AlarmType.FailError;
            }
        }

        /// <summary>源端 SyncResultType → 平台 AlarmProc</summary>
        private static AlarmProc MapAlarmProc(string resultType)
        {
            switch (resultType)
            {
                case "Success":
                case "NoAlarmFail":
                    return AlarmProc.Contine;
                case "Warning":
                    return AlarmProc.Check;
                case "ActionFail":
                case "CheckFail":
                case "CheckAction_Fail":
                    return AlarmProc.Retry;
                case "EL_Fail":
                case "NEL_Fail":
                case "PEL_Fail":
                case "Alarm_Fail":
                case "Origin_Lost":
                    return AlarmProc.Stop;
                default:
                    return AlarmProc.Stop;
            }
        }

        /// <summary>急停/限位/伺服报警类必锁存，其余默认 AutoClear</summary>
        private static LatchPolicy MapLatch(string resultType)
        {
            switch (resultType)
            {
                case "EL_Fail":
                case "NEL_Fail":
                case "PEL_Fail":
                case "Alarm_Fail":
                case "Origin_Lost":
                    return LatchPolicy.Latch;
                default:
                    return LatchPolicy.AutoClear;
            }
        }
    }
}
