using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct
{
    public class PosionSafeModel : IXMLParser
    {
        /// <summary>
        /// 依赖轴ID
        /// </summary>
        public VAxis Axis { get; set; }

        public AxisPosition Position { get; set; }

        /// <summary>
        /// 对象
        /// </summary>
        public IPosition Pos { get; set; }

        /// <summary>
        /// 最小区域
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// 最大区域
        /// </summary>
        public double Max { get; set; }

        public PosionSafeModel(IPosition pos, VAxis vAxis)
        {
            Pos = pos;
            Axis = vAxis;
            
        }

        public PosionSafeModel(IPosition pos, VAxis axis, AxisPosition axisPosition)
        {
            Pos = pos;
            Axis = axis;
            Position= axisPosition;
        }

        public PosionSafeModel(VAxis axis, AxisPosition axisPosition)
        {
            Axis = axis;
            Position = axisPosition;
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRegion = new XElement("SafeItem");
            xRegion.SetAttributeValue("PosionID", Position.Key);

            if (Axis is IPosition pos)
            {
                foreach (var item in Axis.posionSafes)
                {
                    if(item .Position== Position)
                    {
                        foreach (var safePosion in item.PosionSafePostions)
                        {
                            XElement xItem = new XElement("Item");

                            xItem.SetAttributeValue("ID", safePosion.Pos.ID );
                            xItem.SetAttributeValue("Min", safePosion.Min);
                            xItem.SetAttributeValue("Max", safePosion.Max);
                            xRegion.Add(xItem);
                        }
                    }

                    //XElement xItem = new XElement("Item");

                    //xItem.SetAttributeValue("ID", item..ID);
                    //xItem.SetAttributeValue("Min", item.Min);
                    //xItem.SetAttributeValue("Max", item.Max);
                    //xRegion.Add(xItem);
                }
            }

            return xRegion;
        }


        public void ParserXml(XElement xElement)
        {           
            xElement.GetAttribute("ID", id =>
            {
                if (Axis != null && Axis is IPosition pos)
                {
                    var vDeviceID = Guid.Parse(id);
                    Pos = pos.GetPosition(vDeviceID);

                    xElement.GetAttribute("Min", min =>
                    {
                        if (double.TryParse(min, out var pMin))
                        {
                            Min = pMin;
                        }
                    });

                    xElement.GetAttribute("Max", max =>
                    {
                        if (double.TryParse(max, out var pMax))
                        {
                            Max = pMax;
                        }
                    });


                    // 添加依赖点位
                    pos.AddPosionPosition(this);
                }
            });
        }
    }
}
