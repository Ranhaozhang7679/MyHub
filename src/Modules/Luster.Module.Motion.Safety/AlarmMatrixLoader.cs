using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Module.Motion.Safety.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// 把 <see cref="AlarmSchema"/> 列表转换为 <see cref="VAlarm"/> 列表，
        /// 按 <see cref="AlarmSchema.Code"/>(=AlarmKey) 去重：跳过 <paramref name="existing"/> 中已存在的 Code，
        /// 以及 schemas 内部重复的 Code。纯逻辑，不触碰引擎，便于单测。
        /// </summary>
        /// <param name="schemas">从 ErrCode.csv 解析出的报警 schema</param>
        /// <param name="existing">引擎中已存在的 VAlarm（其 AlarmKey 视为已占用）</param>
        /// <returns>待新增的 VAlarm 列表（AlarmKey=Code, AlarmCN=Message, AlarmEn=EnglishName）</returns>
        public List<VAlarm> BuildVAlarms(IEnumerable<AlarmSchema> schemas, IEnumerable<VAlarm> existing)
        {
            var result = new List<VAlarm>();
            if (schemas == null) return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (existing != null)
            {
                foreach (var a in existing)
                {
                    if (a != null && !string.IsNullOrEmpty(a.AlarmKey)) seen.Add(a.AlarmKey);
                }
            }

            foreach (var s in schemas)
            {
                if (s == null || string.IsNullOrEmpty(s.Code)) continue;
                if (!seen.Add(s.Code)) continue; // 引擎已有或本次重复，跳过

                result.Add(new VAlarm
                {
                    ID = Guid.NewGuid(),
                    Name = string.IsNullOrEmpty(s.Message) ? s.Code : s.Message,
                    AlarmKey = s.Code,
                    AlarmCN = s.Message,
                    AlarmEn = s.EnglishName
                });
            }
            return result;
        }

        /// <summary>
        /// 把已加载的 <see cref="AlarmSchema"/> 列表导入目标端 <see cref="IDeviceEngine"/>：
        /// 转换为 <see cref="VAlarm"/> 并经 <see cref="IDeviceEngine.AddVirtual"/> 注入，
        /// 按 <see cref="AlarmSchema.Code"/>(=AlarmKey) 去重。仅显式调用时生效。
        /// </summary>
        /// <param name="engine">目标端设备引擎</param>
        /// <param name="schemas">从 ErrCode.csv 加载的报警 schema</param>
        /// <returns>实际新增并注入的 VAlarm 数量</returns>
        public int ImportToEngine(IDeviceEngine engine, IEnumerable<AlarmSchema> schemas)
        {
            if (engine == null || schemas == null) return 0;

            var existing = engine.GetVDevices<VAlarm>();
            var toAdd = BuildVAlarms(schemas, existing);

            foreach (var alarm in toAdd)
            {
                // VAlarm 是配置型虚拟设备，setReal=false 避免查找真实硬件设备
                engine.AddVirtual(alarm, false);
            }
            return toAdd.Count;
        }

        /// <summary>
        /// 一站式：从 ErrCode.csv 加载并导入到 <see cref="IDeviceEngine"/>。
        /// </summary>
        /// <param name="engine">目标端设备引擎</param>
        /// <param name="csvPath">ErrCode.csv 完整路径</param>
        /// <param name="encoding">编码，默认 GBK</param>
        /// <returns>实际新增并注入的 VAlarm 数量</returns>
        public int ImportToEngine(IDeviceEngine engine, string csvPath, Encoding encoding = null)
        {
            var schemas = Load(csvPath, encoding);
            return ImportToEngine(engine, schemas);
        }
    }
}
