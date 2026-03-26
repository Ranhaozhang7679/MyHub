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
using System.Runtime.Remoting.Messaging;
using Luster.Motion.Integration.Web;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class VisionProcessData : MotionFunction, IHomeFunction
    {
        [Parameter("Viosion过程参数名称,用','分隔", 1, CN = "标题栏", DefaultV = 0)]
        public string Header { get; set; }

        [Parameter("包含多少个参数项", 2, CN = "数据项", DefaultV = 0)]
        public int DataCount { get; set; }

        [NotEmpty]
        [Parameter("数据来源", 3, CN = "数据来源")]
        public LStringEx DataSource { get; set; }

        /// <summary>
        /// 读取数据
        /// </summary>
        public VisionProcessData()
        {
            this.Tips = "Vision过程参数";
            this.Icon = "\xe6aa";
        }

        private Luster.Common.Tools.ExcelTool excelTool = null;
        private DataTable excelTable = null;
        private int index = 0;

        FileStream fs = null;
        StreamReader sr = null;
        // private VisionAPI visionAPI;
        private Dictionary<string, string> array = new Dictionary<string, string>();
        /// <summary> 
        /// 数据列读取
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            array.Clear();
            var lseDatas = GetParametersByType<LStringEx>();

            string[] name = Header.Split(',');

            try
            {
                // 如果有数据缺失，写日志
                if (name.Length != lseDatas.Count())
                {
                    MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info, $"列名数量({name.Length})与数据数量({lseDatas.Count()})不匹配！");
                }

                //动态参数数量变化
                //DataCount 修改时会通过 BuildExternParameters 动态创建参数，但 Header 属性可能未同步更新：
                //int count = lseDatas.Count();
                //for (int i = 0; i < count; i++)
                //{
                //    array.Add(name[i], lseDatas[i].GetString(MyOwner));
                //}
                // 不取最小值进行遍历
                int min = Math.Min(name.Length, lseDatas.Count());
                for (int i = 0; i < min; i++)
                {
                    array.Add(name[i], lseDatas[i].GetString(MyOwner));
                }
            }
            catch (Exception ex)
            {
                string strErr =
                     "Header: " + (Header?.ToString() ?? "未获取到Header") + "\r\n " +
                     "StringEx:" + string.Join(",", lseDatas.Select(x => x.StringEx ?? "未获取到StringEx")) + "\r\n" +
                     "Value: " + string.Join(",", lseDatas.Select(x => x.GetString(MyOwner) ?? "GetString返回为null"));

                // 捕获其他意外异常，保证流程继续
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info, $"遍历数组取元素时异常：{strErr}\r\n{ex.Message}");
            }

            VisionAPI.VisionProcessData = array;
            return base.DoExcute(out errMsg);
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
