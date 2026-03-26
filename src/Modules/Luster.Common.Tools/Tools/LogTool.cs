#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogTool
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Tools.Tools
* 文 件 名:       LogTool.cs
* 创建时间:       2022/10/10 11:00:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      978406c7-bc86-457c-8233-7a70e0722637
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/10 11:00:00
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    /// <summary>
    /// 日志组件
    /// </summary>
    public class LogTool
    {
        private static object lockObj = new object();
        /// <summary>
        /// 配置Log保存路径
        /// </summary>
        /// <param name="dirPath">log对应的路径</param>
        public static void SetLogPath(string dirPath)
        {
            // 配置log目录
            FileInfo fileInfo = new FileInfo(dirPath);
            string slogPath = fileInfo.DirectoryName + "\\STATUS";
            // Parameter文件夹内需保存csv文件
            //string paralogPath = fileInfo.DirectoryName + "\\Parameter";

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (!Directory.Exists(slogPath))
            {
                Directory.CreateDirectory(slogPath);
            }

            //if (!Directory.Exists(paralogPath))
            //{
            //    Directory.CreateDirectory(paralogPath);
            //}

            lock (lockObj)
            {
                NLog.LogManager.Configuration.Variables["LogDir"] = dirPath;
                NLog.LogManager.Configuration.Variables["LogDir1"] = slogPath;
                //NLog.LogManager.Configuration.Variables["LogDir2"] = paralogPath;
            }
        }

        /// <summary>
        /// 通用Log方法
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="logMsg"></param>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        /// <exception cref="FriendlyException"></exception>
        public static void Log(LogType logType, string logMsg, string logger = "*", Exception ex = null)
        {
            switch (logType)
            {
                case LogType.Debug:
                    Debug(logMsg, logger);
                    break;
                case LogType.Info:
                    Info(logMsg, logger);
                    break;
                case LogType.Warning:
                    Warn(logMsg, logger);
                    break;
                case LogType.Error:
                    Error(logMsg, ex, logger);
                    break;
                default:
                    throw new FriendlyException($"未知的日志类型:{logType}");
            }
        }

        public static void Debug(string logMessage, string logger = "*")
        {
            NLog.LogManager.GetLogger(logger).Log(NLog.LogLevel.Debug, logMessage);
        }

        public static void Info(string logMessage, string logger = "*")
        {
            NLog.LogManager.GetLogger(logger).Log(NLog.LogLevel.Info, logMessage);
        }

        public static void Warn(string logMessage, string logger = "*")
        {
            NLog.LogManager.GetLogger(logger).Log(NLog.LogLevel.Warn, logMessage);
        }

        public static void Error(string logMessage, Exception ex = null, string logger = "*")
        {
            NLog.LogManager.GetLogger(logger).Log(NLog.LogLevel.Error, ex, logMessage);
        }
    }
}