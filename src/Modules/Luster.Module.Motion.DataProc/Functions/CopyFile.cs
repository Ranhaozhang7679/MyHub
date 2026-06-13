using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
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
    public class CopyFile : MotionFunction
    {

        [NotEmpty]
        [Parameter("要拷贝的文件夹", 1, CN = "源图片路径", CanRef = ParamRef.Ref)]
        public string SourcePath { get; set; }

        [NotEmpty]
        [Parameter("目标文件夹", 2, CN = "源图片路径", CanRef = ParamRef.Ref)]
        public string DstPath { get; set; }


        [Parameter("拷贝结果", 10, CN = "拷贝文件结果", ParamType = ParamType.OUT)]
        public bool Result { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public CopyFile()
        {
            this.Icon = "\xe629";
            this.Tips = "复制文件";
        }

        /// <summary>
        /// 文件夹拷贝
        /// </summary>
        /// <param name="strSourceFolder">源文件夹</param>
        /// <param name="strDestFolder">目标文件夹</param>
        public static void CopyFolder(string strSourceFolder, string strDestFolder)
        {
            if (!Directory.Exists(strDestFolder))
            {
                Directory.CreateDirectory(strDestFolder);
            }

            string[] files = Directory.GetFileSystemEntries(strSourceFolder);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                string filePath = Path.Combine(strDestFolder, fi.Name);

                if ((fi.Attributes & FileAttributes.Directory) != 0)
                {
                    CopyFolder(fi.FullName, filePath);
                }
                else
                {
                    File.Copy(fi.FullName, filePath, true);
                }
            }
        }
        /// <summary>
        /// 运动
        /// </summary>
        /// <param name="errMsg">errMsg</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            try
            {
                DirectoryInfo a = new DirectoryInfo(SourcePath);
                if (!a.Exists)
                {
                    Result = false;
                    MyOwner.OnLog(LogType.Info, $"源文件夹不存在");
                    errMsg = "源文件夹不存在";
                    return false;
                }

                DirectoryInfo b = new DirectoryInfo(DstPath);
                if (!b.Exists)
                {
                    Result = false;
                    MyOwner.OnLog(LogType.Info, $"目标文件夹不存在");
                    errMsg = "源文件夹不存在";
                    return false;
                }
                CopyFolder(SourcePath, DstPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误：{ex.Message}");
            }
            return base.DoExcute(out errMsg);
        }

    }
}
