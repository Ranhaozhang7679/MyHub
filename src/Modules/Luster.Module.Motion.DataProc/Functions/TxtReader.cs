using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    /// <summary>
    /// 字符串解析
    /// </summary>
    public class TxtReader : MotionFunction
    {

        [NotEmpty]
        [Parameter("目标文件名称", 1, CN = "目标文件名称")]
        public LStringEx DstPath { get; set; }



        [Parameter("文件是否存在", 10, CN = "文件存在", ParamType = ParamType.OUT)]
        public bool IsExist { get; set; }

        [Parameter("文件内容", 11, CN = "文本内容", ParamType = ParamType.OUT)]
        public string Content { get; set; }=string.Empty;



        /// <summary>
        /// 构造函数
        /// </summary>
        public TxtReader()
        {
            this.Icon = "\xe629";
            this.Tips = "从txt读取字符串";
        }


        private System.IO.StreamReader reader;

        /// <summary>
        /// 运动
        /// </summary>
        /// <param name="errMsg">errMsg</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            try
            {
                string txtPath = DstPath.GetString(Owner) + ".txt";
                if(!File.Exists(txtPath)) 
                {
                   IsExist = false;
                }
                else
                {
                    reader=new StreamReader(txtPath);
                    Content=reader.ReadToEnd();
                    IsExist=true;
                    Thread.Sleep(1);
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
               Content=string.Empty;
               IsExist = false;
            }
            return base.DoExcute(out errMsg);
        }

    }
}
