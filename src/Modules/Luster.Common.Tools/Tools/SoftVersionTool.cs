#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SoftVersionTool
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Tools.Tools
* 文 件 名:       SoftVersionTool.cs
* 创建时间:       2022/9/6 14:46:25
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6e14b13a-67db-4550-b440-c8fbc2288fd0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/6 14:46:25
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    /// <summary>
    /// 获取软件版本信息
    /// </summary>
    public class SoftVersionTool
    {

        /// <summary>
        /// 获取软件版本信息
        /// </summary>
        /// <param name="softName">软件名称 xxx.exe</param>
        /// <param name="baseDir">程序所在目录</param>
        /// <returns>版本号</returns>
        public static string GetVersion(string softName, string baseDir = "")
        {
            string versionName = "0.0";
            if (string.IsNullOrEmpty(baseDir))
            {
                baseDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            // 获取当前软件版本信息
            string path = Path.Combine(baseDir, softName);

            var fileInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
            if (fileInfo != null)
            {
                versionName = fileInfo.ProductVersion;
            }

            return versionName;
        }
    }
}