#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VROIs
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.ROIs
* 文 件 名:       VROIs.cs
* 创建时间:       2022/7/26 13:49:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6e53d29f-69c7-48ee-9259-a16aa6bcecbd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/26 13:49:21
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Core;
using Luster.ThreeD.Algorithm.Enums;
using Luster.ThreeD.Algorithm.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm.ROIs
{
    /// <summary>
    /// ROI集合
    /// </summary>
    public class VROIs : LArray<LBaseROI>, IName
    {
        /// <summary>
        /// ROI类型
        /// </summary>
        public ROIType ROIType { get; set; }

        /// <summary>
        /// 是否包含或排除
        /// </summary>
        public bool IsInside { get; set; } = true;

        /// <summary>
        /// 表面距离
        /// </summary>
        public double SurfDistance { get; set; }

        /// <summary>
        /// 模板引用的ID
        /// </summary>
        public string TemplateCoordID { get; set; }

        /// <summary>
        /// 模板坐标
        /// </summary>
        public VCoord TemplateCoord { get; set; }

        /// <summary>
        /// 随动ROI
        /// </summary>
        public VROIs FollowUpROIs { get; set; }

        public string Name
        {
            get
            {
                if (Count > 0)
                {
                    return $"ROI:{Count}";
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
            }
        }
        /// <summary>
        /// 获取相对ROI
        /// </summary>
        /// <param name="refCoord"></param>
        /// <returns></returns>
        public VROIs GetRelativeROIs(ICoord3 refCoord = null)
        {
            if (TemplateCoord == null || refCoord == null)
            {
                return this;
            }
            else
            {
                return NetAPI.ROIsTransform(this,TemplateCoord, new VCoord(new LCoord(refCoord.Matrix)));
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            var xRoot = new XElement("ROIs");

            if (this.Count > 0)
            {
                string realType = this[0].GetType().Name;

                // 记录真实类型
                xRoot.SetAttributeValue("RealType", realType);

                foreach (var item in this)
                {
                    if (item is IXMLParser parser)
                    {
                        xRoot.Add(parser.ExportXml());
                    }
                    else
                    {
                        xRoot.Add(new XElement("item", item.ToString()));
                    }
                }
            }

            if (TemplateCoord != null)
            {
                xRoot.SetAttributeValue("Coord", string.Join(",", TemplateCoord.Matrix));
            }
            xRoot.SetAttributeValue("CoordID", TemplateCoordID);
            xRoot.SetAttributeValue("ROIType", ROIType.ToString());
            xRoot.SetAttributeValue("IsInside", IsInside);
            xRoot.SetAttributeValue("SurfDistance", SurfDistance);
            return xRoot;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void ParserXml(XElement xElement)
        {
            string realType = "";
            xElement.GetAttribute("RealType", (rType) => realType = $"{rType}");
            var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(u => u.Name == realType);
            foreach (var xItem in xElement.Elements())
            {
                if (typeof(IXMLParser).IsAssignableFrom(type))
                {
                    var obj = Activator.CreateInstance(type) as IXMLParser;
                    obj.ParserXml(xItem);
                    this.Add(obj as LBaseROI);
                }
            }

            xElement.GetAttribute("Coord", c =>
            {
                double[] m = c.Split<double>(',');
                TemplateCoord = new VCoord();
                TemplateCoord.Matrix = m;
            });
            xElement.GetAttribute("CoordID", (v) => TemplateCoordID = v);
            xElement.GetAttribute("ROIType", (v) => ROIType = (ROIType)Enum.Parse(typeof(ROIType), v));
            xElement.GetAttribute("IsInside", (v) => IsInside = bool.Parse(v));
            xElement.GetAttribute("SurfDistance", (v) => SurfDistance = double.Parse(v));
        }
    }
}