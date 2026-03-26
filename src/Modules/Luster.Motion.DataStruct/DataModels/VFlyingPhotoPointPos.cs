using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
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
    /// 飞拍点位对象
    /// </summary>
    public class FlyingPhotoPosition : IXMLParser
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// 对应的飞拍
        /// </summary>
        [Ignore]
        public VFlyingPhoto FlyingPhoto { get; set; }

        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 点位位置
        /// </summary>
        public long Position { get; set; }


        /// <summary>
        /// 编码器通道号
        /// </summary>
        public int EncoderNo { get; set; }

        /// <summary>
        /// 触发输出通道
        /// </summary>
        public int TriggerNo { get; set; }

        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xPos = new XElement(Name);
            xPos.SetAttributeValue("Key", Key);

            if (FlyingPhoto != null)
            {
                xPos.SetAttributeValue("Position", Position);
                xPos.SetAttributeValue("EncoderNo", EncoderNo);
                xPos.SetAttributeValue("TriggerNo", TriggerNo);
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

            xElement.GetAttribute(nameof(Position), (position) =>
            {
                if (long.TryParse(position, out var n))
                {
                    Position = n;
                }
            });

            xElement.GetAttribute(nameof(EncoderNo), (encoderNo) =>
            {
                if (int.TryParse(encoderNo, out var n))
                {
                    EncoderNo = n;
                }
            });

            xElement.GetAttribute(nameof(TriggerNo), (triggerNo) =>
            {
                if (int.TryParse(triggerNo, out var n))
                {
                    TriggerNo = n;
                }
            });

        }
        #endregion
    }

    public class VFlyingPhotoPointPos
    {

    }
}
