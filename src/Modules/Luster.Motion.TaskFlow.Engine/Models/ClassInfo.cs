#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ClassModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       ClassModel.cs
* 创建时间:       2022/8/15 13:29:54
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      52b73ca4-5ecd-4e8f-bc63-6e01fe214587
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/15 13:29:54
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class ClassInfo : IXMLParser
    {
        /// <summary>
        /// 班别名称
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 首班标识
        /// </summary>
        public bool FirstClass { get; set; }

        /// <summary>
        /// 对象转换XML
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml();
        }

        /// <summary>
        /// XML转换对象
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {

            this.FromXml(xElement);
        }

        public DateTime GetStartTime()
        {
            var now = DateTime.Now;
            return DateTime.Parse($"{now.ToString("yyyy-MM-dd")} {StartTime.ToString("HH:mm")}");
        }

        public DateTime GetEndTime()
        {
            var sTime = GetStartTime();
            var eTime = DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} {EndTime.ToString("HH:mm")}");
            if (sTime > eTime)
            {
                return eTime.AddDays(1);
            }

            return eTime;
        }
    }
}
