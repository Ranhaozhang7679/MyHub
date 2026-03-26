#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogInfo
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LogInfo.cs
* 创建时间:       2022/5/18 20:30:56
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f866f45e-3570-41f1-9b18-b6f701b987cc
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 20:30:56
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.DataModels
{
    public class LogInfo
    {
        /// <summary>
        /// log对应的线程ID
        /// </summary>
        public string LogThreadID { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public LogType LogType { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string LogMessage { get; set; }
    }
}