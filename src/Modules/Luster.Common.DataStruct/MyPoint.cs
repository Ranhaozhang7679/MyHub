using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.DataStruct
{
    [Serializable]
    public class MyPoint
    {
        [Description("点位名称")]
        public string Name { get; set; }

        [Description("起点X")]
        public int X { get; set; }

        [Description("起点Y")]
        public int Y { get; set; }

        [Description("高度")]
        public int Height { get; set; }

        [Description("宽度")]
        public int Width { get; set; }

        [Description("线宽")]
        public int Thickness { get; set; }

        public MyPoint ParseXml(XElement element)
        {
            this.Name = element.Name.LocalName;

            element.GetAttribute("X", (x) => { this.X = int.Parse(x); });

            element.GetAttribute("Y", (y) => { this.Y = int.Parse(y); });

            element.GetAttribute("Height", (height) => { this.Height = int.Parse(height); });

            element.GetAttribute("Width", (width) => { this.Width = int.Parse(width); });

            element.GetAttribute("Thickness", (thickness) => { this.Thickness = int.Parse(thickness); });

            return this;
        }

        public XElement ExportXml()
        {
            XElement xParam = new XElement(Name);
            xParam.SetAttributeValue("X", this.X);
            xParam.SetAttributeValue("Y", this.Y);
            xParam.SetAttributeValue("Height", this.Height);
            xParam.SetAttributeValue("Width", this.Width);
            xParam.SetAttributeValue("Thickness", this.Thickness);
            return xParam;
        }

    }
}
