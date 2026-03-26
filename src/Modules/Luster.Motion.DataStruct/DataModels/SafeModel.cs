#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SafeRegion
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Engine
* 文 件 名:       SafeRegion.cs
* 创建时间:       2022/9/13 13:51:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5a0d0d11-f8a3-4672-9bd1-9a9c5272e0f7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/13 13:51:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

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
    public class SafeModel : IXMLParser
    {
        /// <summary>
        /// 依赖轴ID
        /// </summary>
        public VAxis Axis { get; set; }

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

        public AxisPosition Position { get; set; }  


        public SafeModel(IPosition pos, VAxis vAxis)
        {
            Pos = pos;
            Axis = vAxis;
        }

        public SafeModel(VAxis axis)
        {
            Axis = axis;
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRegion = new XElement("SafeItem");
            xRegion.SetAttributeValue("AxisID", Axis.ID);

            if (Axis is IPosition pos)
            {
                foreach (var item in pos.GetSafeRegions())
                {
                    XElement xItem = new XElement("Item");

                    xItem.SetAttributeValue("ID", item.Pos.ID);
                    xItem.SetAttributeValue("Min", item.Min);
                    xItem.SetAttributeValue("Max", item.Max);
                    xRegion.Add(xItem);
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
                    pos.AddPosition(this);
                }
            });
        }
    }
}