using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.Common.Tools.Tools;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using System.IO;

namespace Luster.Module.Motion.Protocol.Functions
{
    /// <summary>
    /// SFTP上传
    /// </summary>
    public class SFTPUpload : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("SFTP地址", 0, CN = "SFTP地址", CanRef = ParamRef.Ref)]
        public string SftpAddress { get; set; }

        [NotEmpty]
        [Parameter("SFTP端口号", 1, CN = "SFTP端口", DefaultV = 22, CanRef = ParamRef.Ref)]
        public int SftpPort { get; set; }

        [NotEmpty]
        [Parameter("SFTP账户名称", 2, CN = "SFTP账户", CanRef = ParamRef.Ref)]
        public string UserName { get; set; }

        [NotEmpty]
        [Parameter("SFTP账户密码", 3, CN = "SFTP密码", CanRef = ParamRef.Ref)]
        public string Password { get; set; }

        [NotEmpty]
        [Parameter("本地文件地址", 4, CN = "本地地址", CanRef = ParamRef.Ref)]
        public string LocalAddress { get; set; }

        [NotEmpty]
        [Parameter("SFTP文件地址", 5, CN = "SFTP目录", CanRef = ParamRef.Ref)]
        public string RelAddress { get; set; }

        public SFTPUpload()
        {
            this.Icon = "\xe6e0"; 
            this.Tips = "SFTP客户机,最终上传到服务器的文件是压缩文件";
        }


        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            string dataTimeNow = DateTime.Now.ToString("yyyy-MM-dd");

            LocalAddress = LocalAddress.Replace("$DT$", dataTimeNow);
            RelAddress = RelAddress.Replace("$DT$", dataTimeNow);

            string localPath = LocalAddress + ".zip";
            //创建本地压缩文件
            if (!ZipTool.CompressFolder(localPath, LocalAddress))
            {
                errMsg = $"本地文件压缩失败，本地压缩文件路径:{LocalAddress}";
            }

            if (string.IsNullOrEmpty(errMsg))
            {
                string fileNameNoExt = Path.GetFileNameWithoutExtension(localPath);

                SftpTool sftpTool = new SftpTool(SftpAddress, UserName, Password, SftpPort);

                if (!sftpTool.IsExist(RelAddress))
                {
                    sftpTool.CreateDirectory(RelAddress);
                    MyOwner.OnLog(LogType.Debug, $"创建远程数据文件夹:{RelAddress}");
                }
                RelAddress += "/"+fileNameNoExt + ".zip";

                //上传压缩文件
                if (string.IsNullOrEmpty(errMsg))
                {
                    if (!sftpTool.Put(localPath, RelAddress))
                    {
                        MyOwner.OnLog(LogType.Debug, $"SFTP首次上传文件失败:{RelAddress}");
                        if (!sftpTool.Put(localPath, RelAddress))
                        {
                            errMsg = $"SFTP二次上传文件失败，本地路径:{localPath}，远程路径:{RelAddress}";
                            MyOwner.OnLog(LogType.Debug, errMsg);
                        }
                    }
                }
            }
            
            return base.DoExcute(out errMsg);
        }

    }
}
