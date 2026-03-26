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
    public class DIY_Alarm
    {
        [PropSort(0), DisplayName("报警地址")]
        public int AlarmID {  get; set; }

        [PropSort(1),DisplayName("报警描述")]
        public string AlarmContent {  get; set; }

        [PropSort(2),DisplayName("报警处理方式")]
        public string AlarmSolution { get; set; }

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
