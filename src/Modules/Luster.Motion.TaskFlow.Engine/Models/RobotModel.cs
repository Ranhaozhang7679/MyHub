using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class RobotModel : IXMLParser
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        public VDevice Device { get; set; }

        /// <summary>
        /// 全局变量
        /// </summary>
        public string GlobalVar { get; set; }

        /// <summary>
        /// 全局变量Key
        /// </summary>
        public string GlobalKey { get; set; }

        /// <summary>
        /// 全局变量参数
        /// </summary>
        [Ignore]
        public ParameterAttribute GlobalParameter { get; set; }

        /// <summary>
        /// 对象转XML
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml();
        }
        /// <summary>
        /// XML转对象
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
             this.FromXml(xElement);
        }
    }
}
