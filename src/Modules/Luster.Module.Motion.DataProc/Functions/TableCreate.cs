using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.TaskFlow.Common.Models;
using System.Drawing.Imaging;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class TableCreate : MotionFunction, IHomeFunction
    {
        [Parameter("数据文件,支持csv/txt/xls", 1, CN = "数据文件名称", CanRef = ParamRef.NoRef)]
        public string DataFile { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        [Parameter("SN", 2, CN = "SN", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        [Parameter("表头,用','分隔", 3, CN = "标题栏", DefaultV = 0)]
        public string Header { get; set; }


        [Parameter("工位", 4, CN = "工位")]
        public string WorkStation { get; set; } 


        [Parameter("保压头", 5, CN = "保压头")]
        public string PressNo { get; set; } 


        [Parameter("包含多少个数据项", 6, CN = "数据项", DefaultV = 0)]
        public int DataCount { get; set; }

        [NotEmpty]
        [Parameter("数据来源", 7, CN = "数据来源")]
        public LStringEx DataSource{ get; set; }

        /// <summary>
        /// 读取数据
        /// </summary>
        public TableCreate()
        {
            this.Tips = "构建表格";
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
            
            string time = DateTime.Now.ToString("yyyyMMdd");
            string directoryName = Path.GetDirectoryName($"D:\\PressData\\{time}\\{WorkStation}\\{PressNo}\\Data\\");
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            DataFile = $"D:\\PressData\\{time}\\{WorkStation}\\{PressNo}\\Data\\{SN}.csv";
            // 更新可以写入的文件名
            string dir = $"D:\\PressData\\{time}\\{WorkStation}\\{PressNo}\\Data";
            DataFile = GetWriteableFileName(dir, SN);
            // FileShare.ReadWrite以共享方式打开
            fs = new FileStream(DataFile,FileMode.OpenOrCreate,FileAccess.ReadWrite, FileShare.ReadWrite);
            sr = new StreamReader(fs);

            var lseDatas = GetParametersByType<LStringEx>();
            var RowDatas = string.Empty;

            string text=sr.ReadToEnd();
   
            string NewHeader = "Time," + Header + "\r\n".Replace('，',',');
            ///表头为空
            if (!text.Contains(NewHeader))
            {
                fs.Write(Encoding.UTF8.GetBytes(NewHeader),0, Encoding.UTF8.GetBytes(NewHeader).Length);
            }

            foreach(var lseData in lseDatas)
            {
                if (lseData == lseDatas[lseDatas.Count - 1])
                {
                    RowDatas += lseData.GetString(MyOwner);
                }
                else
                {
                    RowDatas += lseData.GetString(MyOwner) + ",";
                }
            }
            
            string DateTimeNow = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            RowDatas = DateTimeNow + "," + RowDatas + "\r\n";
            fs.Write(Encoding.UTF8.GetBytes(RowDatas), 0, Encoding.UTF8.GetBytes(RowDatas).Length);
            //fs.Flush();
            //fs.Write(sw);
            fs.Close();
            return base.DoExcute(out errMsg);
        }

        // 尝试得到真正可以写入的文件名
        string GetWriteableFileName(string folder, string name)
        {
            int idx = 0;
            while (true)
            {
                string file = Path.Combine(folder,
                idx == 0 ? $"{name}{".csv"}" : $"{name}_{idx}{".csv"}");

                try
                {
                    // 仅尝试打开，不真的写内容
                    using (var fs = new FileStream(file,
                                                   FileMode.Append,
                                                   FileAccess.Write,
                                                   FileShare.Read))
                    {
                        return file;                // 成功表示可用
                    }
                }
                catch (IOException ex) when (IsFileLocked(ex))
                {
                    idx++;                          // 被占用，换下一个后缀
                }
            }
        }
        // 辅助：判断 IOException 是否为“文件被占用”
        bool IsFileLocked(IOException ex)
        {
            const int SHARING_VIOLATION = 32;
            const int LOCK_VIOLATION = 33;
            int err = Marshal.GetHRForException(ex) & 0xFFFF;
            return err == SHARING_VIOLATION || err == LOCK_VIOLATION;
        }
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(LPath))
            {
                Home();
            }
            if (parameter.Name == "DataCount" && int.TryParse(newV?.ToString(), out var num) && num >= 1)
            {
                BuildExternParameters(1, num, "DataSource", "数据源", typeof(LStringEx), (p) =>
                {
                    p.EditorType = typeof(LStringEx);
                });
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
