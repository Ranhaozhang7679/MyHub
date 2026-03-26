using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Luster.Module.Motion.DataProc.Functions
{

    public enum WriteWay
    {
        [Description("文末追加")]
        Append,

        [Description("文本覆盖")]
        OverWrite,
    }


    /// <summary>
    /// 字符串解析
    /// </summary>
    public class TxtWriter : MotionFunction
    {
        /// <summary>
        /// 
        /// </summary>
        [Parameter("原始字符串", 0, CN = "原始字符")]
        public LStringEx Source { get; set; }



        [NotEmpty]
        [Parameter("目标文件名称", 1, CN = "目标文件名称")]
        public LStringEx DstPath { get; set; }


        [Parameter("文本写入方式", 1, CN = "文本写入方式")]
        public WriteWay writeWay { get; set; }


        [Parameter("文本写入结果", 10, CN = "文本写入结果", ParamType = ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TxtWriter()
        {
            this.Icon = "\xe629";
            this.Tips = "将字符串写入txt";
        }


        private System.IO.StreamWriter writer;

        /// <summary>
        /// 运动
        /// </summary>
        /// <param name="errMsg">errMsg</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            try
            {
                bool isAppend = writeWay == WriteWay.Append;

                string txtPath =Path.Combine(DstPath.GetString(Owner)+".txt");
                string txtContent = Source.GetString(Owner);

                writer = new StreamWriter(txtPath, isAppend);
                writer.Write(txtContent);
                Thread.Sleep(1);
                writer.Close();
                Result = true;
            }

            catch (Exception ex)
            {
                Result = false;
            }
            return base.DoExcute(out errMsg);
        }

    }
}
