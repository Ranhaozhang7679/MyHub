using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 点位分组
    /// </summary>
    public class ModuleNameModel : IXMLParser
    {
        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name { get; set; }


        public ModuleNameModel()
        {
        }

        #region 导入和导出功能
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml("ModuleName");
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        #endregion
    }
}
