using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public enum AssFileCategory
    {
        [Description("自动定位与水平")]
        AutomaticPosAndLeveling,

        [Description("压力线性")]
        CalibrationTable,

        [Description("压力重复性")]
        PressureRepetition,

        [Description("吸嘴压力标定")]
        SuctionNozzle,

        [Description("自动压印")]
        AutomaticEmbossing,

        [Description("自动对焦")]
        AutoFocusing,

        [Description("自动视野")]
        AutoFieldOfView,

        [Description("自动灰度")]
        AutoGrayScale,

        [Description("手眼标定")]
        AutoVisualCalibration,
    }
    /// <summary>
    /// 写入表格数据
    /// </summary>
    public class TableInsert : MotionFunction, IHomeFunction
    {
        [NotEmpty]
        [DependOn("IsTemporaryWrite", true)]
        [Parameter("数据文件,支持csv/txt/xls", 1, CN = "文件路径", CanRef = ParamRef.NoRef, DefaultV = "D:\\Motion\\AssData.csv")]
        public string DataFile { get; set; }

        // 新增临时写入选择参数
        [Parameter("是否写入\"自定义文件\"", 0, CN = "文件自定义写入", DefaultV = true)]
        public bool IsTemporaryWrite { get; set; }

        [DependOn("IsTemporaryWrite", false)]
        [Parameter("数字架线文件类别", 2, CN = "文件类别", DefaultV = AssFileCategory.CalibrationTable)]
        public AssFileCategory FileCategory { get; set; }


        [DependOn("IsTemporaryWrite", false)]
        [Parameter("清空上一批最新数据", 2, CN = "是否清空上一批最新数据", DefaultV = true, CanRef = ParamRef.Ref)]
        public bool ClearLatestData { get; set; } = true;

        /// <summary>
        /// 根据表头自动创建参数 BuildExternParameters,类型LStringEx,数据项名称根据表头生成
        /// </summary>
        [NotEmpty]
        [Parameter("表头,用','分隔", 3, CN = "表头", DefaultV = "项序,项次,标准,实测,状态,完成时间")]
        public string Header { get; set; }

        [Parameter("项序", 7, CN = "项序")]
        public LStringEx 项序 { get; set; }

        [Parameter("项次", 7, CN = "项次")]
        public LStringEx 项次 { get; set; }

        [Parameter("标准", 7, CN = "标准")]
        public LStringEx 标准 { get; set; }

        [Parameter("实测", 7, CN = "实测")]
        public LStringEx 实测 { get; set; }

        [Parameter("状态", 7, CN = "状态")]
        public LStringEx 状态 { get; set; }

        [Parameter("完成时间", 7, CN = "完成时间")]
        public LStringEx 完成时间 { get; set; }

        //[NotEmpty]
        //[Parameter("主键列名", 1, CN = "主键列名", CanRef = ParamRef.NoRef)]
        //public string Primarykey { get; set; }

        /// <summary>
        /// 读取数据
        /// </summary>
        public TableInsert()
        {
            this.Tips = "表格写入";
            this.Icon = "\xe629";
        }

        private Luster.Common.Tools.ExcelTool excelTool = null;
        private DataTable excelTable = null;
        private int index = 0;

        FileStream fs = null;
        StreamReader sr = null;

        /// <summary>
        /// 判断文件是否可写（未被占用）
        /// </summary>
        private bool IsFileWriteable(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    // 能打开并写入说明可用
                }
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// 数据列读取
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = null;

            var headerFields = ValidateAndParseHeader(Header, out string headerErr);
            if (headerFields == null)
            {
                errMsg = $"表头格式错误: {headerErr}";
                return false;
            }

            // 准备数据行
            var lseDatas = GetParametersByType<LStringEx>();
            Dictionary<string, object> rowDict = new Dictionary<string, object>();
            for (int i = 0; i < headerFields.Length; i++)
            {
                string key = headerFields[i];
                if (key == "完成时间")
                {
                    rowDict[key] = DateTime.Now;
                }
                else
                {
                    rowDict[key] = i < lseDatas.Count && lseDatas[i] != null ? lseDatas[i].GetString(MyOwner) : "";
                }
            }

            if (IsTemporaryWrite == false)
            {
                // 根据文件类别设置路径
                var baseDir = Path.Combine(Path.GetDirectoryName(MyOwner.DeviceEngine.RecipeDataPath), "db", "Ass_Data");

                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }
                // 最新数据文件路径（界面显示用）
                string latestFile = Path.Combine(baseDir, $"AssTb{FileCategory}_Latest.csv");

                // 历史数据文件路径（永久存储）
                string historyFile = Path.Combine(baseDir, $"AssTb{FileCategory}_History.csv");

                // 如果 ClearLatestData=true，清空最新数据文件（流程开始时）
                if (ClearLatestData)
                {
                    File.WriteAllText(latestFile, string.Join(",", headerFields) + Environment.NewLine, Encoding.UTF8);
                }

                // 追加到最新数据文件
                AppendToCsvFile(latestFile, headerFields, rowDict);

                // 追加到历史数据文件
                AppendToCsvFile(historyFile, headerFields, rowDict);
            }
            else
            {
                // 临时写入模式：写入自定义文件
                string ext = Path.GetExtension(DataFile)?.ToLower();

                // 判断文件是否存在，不存在则新建
                if (!File.Exists(DataFile))
                {
                    File.WriteAllText(DataFile, string.Join(",", headerFields) + Environment.NewLine, Encoding.UTF8);
                }

                // 校验文件表头是否一致
                string[] fileHeaderFields = null;
                if (ext == ".csv" || ext == ".txt")
                {
                    var allLines = File.ReadAllLines(DataFile, Encoding.UTF8).ToList();
                    if (allLines.Count > 0)
                    {
                        fileHeaderFields = allLines[0].Replace('，', ',').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim()).ToArray();
                        if (fileHeaderFields.Length != headerFields.Length || !fileHeaderFields.SequenceEqual(headerFields))
                        {
                            errMsg = "表头与文件表头不一致";
                            return false;
                        }
                    }
                }
                else if (ext == ".xls" || ext == ".xlsx")
                {
                    if (excelTool == null)
                        excelTool = new Luster.Common.Tools.ExcelTool(DataFile, true);

                    var sheetTable = excelTool.GetTableBySheet("Sheet1", 0, 0);
                    if (sheetTable != null && sheetTable.Columns.Count > 0)
                    {
                        fileHeaderFields = sheetTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName.Trim()).ToArray();
                        if (fileHeaderFields.Length != headerFields.Length || !fileHeaderFields.SequenceEqual(headerFields))
                        {
                            errMsg = "表头与Excel文件表头不一致";
                            return false;
                        }
                    }
                }

                // 写入前判断文件是否可用
                if ((ext == ".csv" || ext == ".txt") && !IsFileWriteable(DataFile))
                {
                    errMsg = "文件被占用或不可写入，请稍后重试。";
                    return false;
                }

                if (ext == ".csv" || ext == ".txt")
                {
                    // 读取所有行
                    var allLines = File.ReadAllLines(DataFile, Encoding.UTF8).ToList();

                    // 如果文件为空，写入表头
                    if (allLines.Count == 0)
                    {
                        allLines.Add(string.Join(",", headerFields));
                    }

                    // 添加新行
                    var rowValues = new List<string>();
                    foreach (var key in headerFields)
                    {
                        rowValues.Add(rowDict[key]?.ToString() ?? "");
                    }
                    allLines.Add(string.Join(",", rowValues));
                    File.WriteAllLines(DataFile, allLines, Encoding.UTF8);
                }
                else if (ext == ".xls" || ext == ".xlsx")
                {
                    // Excel文件写入前判断
                    if (!IsFileWriteable(DataFile))
                    {
                        errMsg = "Excel文件被占用或不可写入，请稍后重试。";
                        return false;
                    }

                    if (excelTool == null)
                        excelTool = new Luster.Common.Tools.ExcelTool(DataFile, true);

                    if (excelTable == null)
                    {
                        excelTable = new DataTable();
                        foreach (var key in headerFields)
                            excelTable.Columns.Add(key);
                    }

                    // 添加新行
                    excelTool.AddRow(rowDict);
                }
                else
                {
                    errMsg = "不支持的文件格式";
                    return false;
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 追加数据到CSV文件
        /// </summary>
        private void AppendToCsvFile(string filePath, string[] headerFields, Dictionary<string, object> rowDict)
        {
            // 文件不存在则创建并写入表头
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, string.Join(",", headerFields) + Environment.NewLine, Encoding.UTF8);
            }

            // 追加数据行
            var rowValues = headerFields.Select(key => rowDict[key]?.ToString() ?? "");
            File.AppendAllText(filePath, string.Join(",", rowValues) + Environment.NewLine, Encoding.UTF8);
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);

            // 监听 Header 变更
            if (parameter.Name == nameof(Header))
            {
                string headerStr = newV?.ToString() ?? string.Empty;
                var headerFields = ValidateAndParseHeader(headerStr, out string headerErr);

                if (headerFields == null)
                {
                    throw new FriendlyException($"表头格式错误: {headerErr}");
                }

                // 清除除了 DataFile 和 Header 外的所有参数
                var keysToRemove = Owner.Parameters.Keys
                    .Where(k => k != nameof(DataFile) && k != nameof(Header))
                    .ToList();
                foreach (var key in keysToRemove)
                {
                    Owner.Parameters.Remove(key);
                }

                // 根据表头自动创建参数
                int sortBase = 10;
                for (int i = 0; i < headerFields.Length; i++)
                {
                    string key = headerFields[i];
                    string cn = key;
                    int sort = sortBase + i;

                    if (Owner.Parameters.ContainsKey(key))
                        continue;

                    BuildExternParameter(key, cn, sort, typeof(LStringEx), (p) =>
                    {
                        p.EditorType = typeof(LStringEx);
                    });
                }
            }

            if (parameter.Name == nameof(LPath))
            {
                Home();
            }
        }

        /// <summary>
        /// 回零
        /// </summary>
        public override void Home()
        {
            excelTool = null;
            fs?.Close();
            fs = null;
            sr?.Close();
            sr = null;

            index = 0;
        }

        /// <summary>
        /// 校验并解析表头
        /// </summary>
        /// <param name="headerStr"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private string[] ValidateAndParseHeader(string headerStr, out string errMsg)
        {
            errMsg = null;
            if (string.IsNullOrWhiteSpace(headerStr))
            {
                errMsg = "表头不能为空";
                return null;
            }

            // 替换中文逗号为英文逗号
            headerStr = headerStr.Replace('，', ',');

            var fields = headerStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(f => f.Trim())
                                  .ToArray();

            if (fields.Length == 0)
            {
                errMsg = "表头字段不能为空";
                return null;
            }

            // 校验每个字段
            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field))
                {
                    errMsg = "表头字段不能有空项";
                    return null;
                }
                // 只允许字母、数字、下划线
                //if (!System.Text.RegularExpressions.Regex.IsMatch(field, @"^[A-Za-z0-9_]+$"))
                //{
                //    errMsg = $"表头字段 '{field}' 非法，只允许字母、数字、下划线";
                //    return null;
                //}
            }

            return fields;
        }
    }
}
