using Luster.Common.DataStruct.Attributes;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Models;
using System.IO;

namespace Luster.Module.Motion.DataProc.Functions
{
    public enum RenameType
    {
        [Description("文件")]
        File,

        [Description("文件夹")]
        Folder
    }

    public class Rename : MotionFunction, IHomeFunction
    {
        /// <summary>
        /// 条件表达式
        /// </summary>
        [NotEmpty]
        [Parameter("字符串生成的方式", 0, CN = "生成方式", DefaultV = RenameType.File)]
        public RenameType RenameType { get; set; }

        [NotEmpty]
        [Parameter("原始文件的名称，以反斜杠进行分隔(/)", 2, CN = "旧名称")]
        public LStringEx OldFileName { get; set; }

        [NotEmpty]
        [Parameter("新文件的名称，以反斜杠进行分隔(/)", 3, CN = "新名称")]
        public LStringEx NewFileName { get; set; }

        public Rename()
        {
            this.Tips = "重命名";
            this.Icon = "\xe6ae";
        }

        /// <summary>
        /// 函数执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>运行结果</returns>
        public override bool DoExcute(out string errMsg)
        {
            string oldFileName = OldFileName.GetString(MyOwner);
            string newFileName = NewFileName.GetString(MyOwner);
            if (RenameType == RenameType.File)
            {
                if (!File.Exists(oldFileName))
                {
                    errMsg = $"旧文件名称:{oldFileName}不存在!";
                    return false;
                }

                string rootDir = Path.GetDirectoryName(newFileName);
                if (!Directory.Exists(rootDir))
                {
                    Directory.CreateDirectory(rootDir);
                }

                // 文件名称
                File.Copy(oldFileName, newFileName, true);
            }
            else
            {
                // 文件夹
                if (!Directory.Exists(oldFileName))
                {
                    errMsg = $"旧目录名称:{oldFileName}不存在!";
                    return false;
                }

                if (!Directory.Exists(newFileName))
                {
                    Directory.CreateDirectory(newFileName);
                }

                // 文件夹递归拷贝
                Luster.Common.Tools.FolderTool.CopyFiles(oldFileName, newFileName, true);
            }

            return base.DoExcute(out errMsg);
        }
    }
}
