using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{

    /// <summary>
    /// 机器人点位对象
    /// </summary>
    public class RobotPosition : IXMLParser
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// 对应的机器人
        /// </summary>
        [Ignore]
        public VRobot Robot { get; set; }

        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 点位位置
        /// </summary>
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double PositionC { get; set; }

        public double PositionH { get; set; }

        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xPos = new XElement(Name);
            xPos.SetAttributeValue("Key", Key);

            if (Robot != null)
            {
                xPos.SetAttributeValue("PositionX", PositionX);
                xPos.SetAttributeValue("PositionY", PositionY);
                xPos.SetAttributeValue("PositionZ", PositionZ);
                xPos.SetAttributeValue("PositionC", PositionC);
                xPos.SetAttributeValue("PositionH", PositionH);
            }

            return xPos;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            Name = xElement.Name.ToString();

            xElement.GetAttribute(nameof(Key), (key) =>
            {
                Key = Guid.Parse(key);
            });

            xElement.GetAttribute(nameof(PositionX), (positionX) =>
            {
                if (double.TryParse(positionX, out var n))
                {
                    PositionX = n;
                }
            });

            xElement.GetAttribute(nameof(PositionY), (positionY) =>
            {
                if (double.TryParse(positionY, out var n))
                {
                    PositionY = n;
                }
            });

            xElement.GetAttribute(nameof(PositionZ), (positionZ) =>
            {
                if (double.TryParse(positionZ, out var n))
                {
                    PositionZ = n;
                }
            });

            xElement.GetAttribute(nameof(PositionC), (positionC) =>
            {
                if (double.TryParse(positionC, out var n))
                {
                    PositionC = n;
                }
            });

            xElement.GetAttribute(nameof(PositionH), (positionH) =>
            {
                if (double.TryParse(positionH, out var n))
                {
                    PositionH = n;
                }
            });

            // 隶属轴编号
            //xElement.GetAttribute(nameof(AxisNo), (axisNo) =>
            //{
            //    if (int.TryParse(axisNo, out var n))
            //    {
            //        AxisNo = n;
            //    }
            //});
        }
        #endregion
    }


    /// <summary>
    /// 6轴机器人点位对象
    /// </summary>
    public class SixRobotPosition : IXMLParser
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// 对应的机器人
        /// </summary>
        [Ignore]
        public VRobot Robot { get; set; }

        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 点位位置
        /// </summary>
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double PositionU { get; set; }
        public double PositionV { get; set; }
        public double PositionW { get; set; }


        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xPos = new XElement(Name);
            xPos.SetAttributeValue("Key", Key);

            if (Robot != null)
            {
                xPos.SetAttributeValue("PositionX", PositionX);
                xPos.SetAttributeValue("PositionY", PositionY);
                xPos.SetAttributeValue("PositionZ", PositionZ);
                xPos.SetAttributeValue("PositionU", PositionU);
                xPos.SetAttributeValue("PositionV", PositionV);
                xPos.SetAttributeValue("PositionW", PositionW);
            }

            return xPos;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            Name = xElement.Name.ToString();

            xElement.GetAttribute(nameof(Key), (key) =>
            {
                Key = Guid.Parse(key);
            });

            xElement.GetAttribute(nameof(PositionX), (positionX) =>
            {
                if (double.TryParse(positionX, out var n))
                {
                    PositionX = n;
                }
            });

            xElement.GetAttribute(nameof(PositionY), (positionY) =>
            {
                if (double.TryParse(positionY, out var n))
                {
                    PositionY = n;
                }
            });

            xElement.GetAttribute(nameof(PositionZ), (positionZ) =>
            {
                if (double.TryParse(positionZ, out var n))
                {
                    PositionZ = n;
                }
            });

            xElement.GetAttribute(nameof(PositionU), (positionU) =>
            {
                if (double.TryParse(positionU, out var n))
                {
                    PositionU = n;
                }
            });

            xElement.GetAttribute(nameof(PositionV), (positionV) =>
            {
                if (double.TryParse(positionV, out var n))
                {
                    PositionV = n;
                }
            });

            xElement.GetAttribute(nameof(PositionW), (positionW) =>
            {
                if (double.TryParse(positionW, out var n))
                {
                    PositionW = n;
                }
            });

            // 隶属轴编号
            //xElement.GetAttribute(nameof(AxisNo), (axisNo) =>
            //{
            //    if (int.TryParse(axisNo, out var n))
            //    {
            //        AxisNo = n;
            //    }
            //});
        }
        #endregion
    }



    public  class VRobotPointPos
    {

    }
}
