#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FTPUpload
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       FTPUpload.cs
* 创建时间:       2022/9/8 15:26:07
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      33bf0090-9c14-4a68-8a09-805374d1eaf6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 15:26:07
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    /// <summary>
    /// FTP 上传
    /// </summary>
    public class FTPUpload : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("FTP地址", 1, CN = "FTP地址")]
        public string FtpAddress { get; set; }

        [NotEmpty]
        [Parameter("FTP账户名称", 2, CN = "FTP账户")]
        public string UserName { get; set; }

        [NotEmpty]
        [Parameter("FTP账户密码", 3, CN = "FTP密码")]
        public string Password { get; set; }

        [NotEmpty]
        [Parameter("本地文件地址", 4, CN = "本地地址")]
        public LStringEx LocalAddress { get; set; }

        [NotEmpty]
        [Parameter("FTP文件地址", 5, CN = "FTP目录")]
        public string RelAddress { get; set; }

        public FTPUpload()
        {
            this.Icon = "\xe6bb";
            this.Tips = "FTP客户机,最终上传到服务器的文件是 ftp://FTP地址/RelAddress/FileName.png";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            try
            {
                var ftpClient = Luster.Common.Tools.FtpTool.GetClient(FtpAddress, UserName, Password);
                string strLocal = LocalAddress.GetString(Owner);
                ftpClient.UploadFile(RelAddress, strLocal);
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, ex.Message);
            }

            return base.DoExcute(out errMsg);
        }
    }
}
