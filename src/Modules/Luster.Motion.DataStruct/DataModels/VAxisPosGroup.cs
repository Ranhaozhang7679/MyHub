using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 点位分组
    /// </summary>
    public class VAxisPosGroup : List<AxisPosition>, IXMLParser
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// 点位名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// X点位
        /// </summary>
        public double X
        {
            get
            {
                double x = double.NaN;
                var position = this.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == Enums.AxisType.X);
                if (position != null)
                {
                    x = position.Position;
                }
                return x;
            }
        }

        /// <summary>
        /// Y点位
        /// </summary>
        public double Y
        {
            get
            {
                double x = double.NaN;
                var position = this.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == Enums.AxisType.Y);
                if (position != null)
                {
                    x = position.Position;
                }
                return x;
            }
        }

        /// <summary>
        /// Z点位
        /// </summary>
        public double Z
        {
            get
            {
                double x = double.NaN;
                var position = this.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == Enums.AxisType.Z);
                if (position != null)
                {
                    x = position.Position;
                }
                return x;
            }
        }

        /// <summary>
        /// U点位
        /// </summary>
        public double U
        {
            get
            {
                double x = double.NaN;
                var position = this.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == Enums.AxisType.U);
                if (position != null)
                {
                    x = position.Position;
                }
                return x;
            }
        }

        /// <summary>
        /// V点位
        /// </summary>
        public double V
        {
            get
            {
                double x = double.NaN;
                var position = this.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == Enums.AxisType.V);
                if (position != null)
                {
                    x = position.Position;
                }
                return x;
            }
        }

        /// <summary>
        /// W点位
        /// </summary>
        public double W
        {
            get
            {
                double x = double.NaN;
                var position = this.FirstOrDefault(u => u.Axis != null && u.Axis.AxisType == Enums.AxisType.W);
                if (position != null)
                {
                    x = position.Position;
                }
                return x;
            }
        }

        /// <summary>
        /// 设备引擎
        /// </summary>
        public IDeviceEngine DeviceEngine { get; set; }

        /// <summary>
        /// 点位组
        /// </summary>
        /// <param name="posName"></param>
        /// <param name="axisPos"></param>
        public VAxisPosGroup()
        {
        }

        #region 导入和导出功能
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xPos = new XElement(Name);
            xPos.SetAttributeValue("Key", Key);
            foreach (var item in this)
            {
                xPos.Add(item.ExportXml());
            }

            return xPos;
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            // 点位名称
            Name = xElement.Name.LocalName;

            // 唯一标识
            xElement.GetAttribute("Key", key =>
            {
                Key = Guid.Parse(key);
            });

            this.Clear();

            List<VAxis> vAxes = new List<VAxis>();
            if (DeviceEngine != null)
            {
                vAxes = DeviceEngine.GetVDevices<VAxis>();
            }

            // 添加组
            foreach (var item in xElement.Elements())
            {
                AxisPosition p = new AxisPosition();
                p.ParserXml(item);

                // 更新轴对象
                if (vAxes.Count > 0)
                {
                    var exist = vAxes.FirstOrDefault(u => u.AxisNo == p.AxisNo);
                    if (exist != null)
                    {
                        p.Axis = exist;
                    }
                }

                this.Add(p);
            }
        }
        #endregion
    }
}
