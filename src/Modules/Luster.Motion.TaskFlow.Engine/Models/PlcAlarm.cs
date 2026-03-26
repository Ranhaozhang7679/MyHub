#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PlcConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       PlcConfig.cs
* 创建时间:       2022/9/1 14:37:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      22b038f0-c587-4bc6-a16b-ec454a8e21d1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/1 14:37:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// Plc 报警对象
    /// </summary>
    public class PlcAlarm : Common.DataStruct.Interfaces.IXMLParser
    {
        /// <summary>
        /// 报警值
        /// </summary>
        [PropSort(0), DisplayName("报警地址")]
        public int Address { get; set; }

        /// <summary>
        /// 报警描述
        /// </summary>
        [PropSort(1), DisplayName("报警描述")]
        public string Desc { get; set; }

        /// <summary>
        /// 报警代码
        /// </summary>
        [PropSort(2), DisplayName("报警代码")]
        public string AlarmCode { get; set; }

        /// <summary>
        /// 当前是否报警
        /// </summary>
        [Ignore]
        public bool IsAlarm { get; set; }

        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        public XElement ExportXml()
        {
            return this.ToXml();
        }
    }
}