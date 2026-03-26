using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace Luster.Module.Motion.DataProc.Functions
{
    public enum FileType
    {
        [Description("Xls")]
        Xls,

        [Description("Txt")]
        Txt,

        [Description("xml")]
        Xml
    }

    public class Alarm
    {
        [Description("故障代码")]
        public string ErrorCode { get; set; }

        [Description("故障信息")]
        public string ErrorMessage { get; set; }

        [Description("故障描述")]
        public string ErrorDetail { get; set; }
    }

    public class ReadDataFile : MotionFunction, IHomeFunction
    {
        [Parameter("文件类型", 0, CN = "文件类型", DefaultV = FileType.Xls)]
        public FileType FileType { get; set; }

        [Parameter("数据文件,支持csv/txt/xls", 1, CN = "数据文件", CanRef = ParamRef.NoRef)]
        public LPath DataFile { get; set; }

        [Parameter("数据读取起始行,索引从0开始", 2, CN = "起始行", DefaultV = 0)]
        public int StartRow { get; set; }

        [Parameter("读取行", 3, CN = "读取行", DefaultV = false)]
        public bool IsRowRead { get; set; }

        [DependOn("IsRowRead", false)]
        [Parameter("数据列", 4, CN = "读取列", DefaultV = 0)]
        public int Column { get; set; }

        [DependOn("FileType", FileType.Txt)]
        [Parameter("数据分隔符", 5, CN = "分隔符", DefaultV = ",")]
        public string Separator { get; set; }

        [Parameter("数据结果", 6, CN = "读取结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutResult { get; set; }

        [Parameter("读取到的第几行", 7, CN = "读取行", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int OutRow { get; set; }

        /// <summary>
        /// 读取数据
        /// </summary>
        public ReadDataFile()
        {
            this.Tips = "通过数据文件读取数据列";
            this.Icon = "\xe629";
        }

        private Luster.Common.Tools.ExcelTool excelTool = null;
        private DataTable excelTable = null;
        private int index = 0;

        FileStream fs = null;
        StreamReader sr = null;

        /// <summary>
        /// 数据列读取
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            OutRow = StartRow + index;
            OutResult = "";

            switch (FileType)
            {
                case FileType.Xls:
                    if (excelTool == null)
                    {
                        excelTool = new Common.Tools.ExcelTool(DataFile.Path, false);
                        excelTable = excelTool.GetTableBySheet(0, 0, 0);
                    }

                    // 验证表格
                    if (excelTable.Rows.Count < OutRow)
                    {
                        errMsg = $"表格行数:{excelTable.Rows.Count} < {OutRow}";
                        return false;
                    }

                    var itemArray = excelTable.Rows[OutRow].ItemArray;
                    if (IsRowRead)
                    {
                        foreach(var str in itemArray)
                        {
                            if (string.IsNullOrEmpty(OutResult))
                                OutResult = $"{str}";
                            else
                               OutResult = OutResult+$",{str}";
                        }
                    }
                    else
                    {
                        ValidColumn(itemArray.Length);
                        OutResult = itemArray[Column]?.ToString();
                    }

                    break;
                case FileType.Txt:

                    if (fs == null)
                    {
                        fs = new FileStream(DataFile.Path, FileMode.Open, FileAccess.Read);
                        sr = new StreamReader(fs);
                    }

                    // 读取前几行
                    int i = 0;
                    while (i < StartRow)
                    {
                        sr.ReadLine();
                    }

                    // 读取当前行
                    string line = sr.ReadLine();
                    if (line == null)
                    {
                        errMsg = "文件读取结束";
                        return true;
                    }

                    if (!string.IsNullOrEmpty(line))
                    {
                        if (IsRowRead) 
                        {
                            OutResult = line;
                        }
                        else
                        {
                            string[] res = line.Split(Separator.ToCharArray());
                            ValidColumn(res.Length);
                            OutResult = res[Column].ToString();
                        }
                    }

                    break;

            }

            // 每运行一次，索引累加1
            index++;

            // 输出保证是从1开始
            OutRow++;



            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 有效性
        /// </summary>
        /// <param name="arrLen"></param>
        /// <exception cref="Exception"></exception>
        private void ValidColumn(int arrLen)
        {
            if (arrLen < Column)
            {
                throw new Exception($"数组长度<Column {arrLen}<{Column}");
            }
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
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
    }
}
