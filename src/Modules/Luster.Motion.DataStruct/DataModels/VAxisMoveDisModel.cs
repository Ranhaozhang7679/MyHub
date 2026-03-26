#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VAxisMoveDisModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       VAxisMoveDisModel.cs
* 创建时间:       2023/7/19 13:59:45
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      f997dbd1-124d-44a2-a159-4cc2a996b1a6
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/7/19 13:59:45
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
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
    public class VAxisMoveDisModel : IXMLParser
    {
        /// <summary>
        /// 动作
        /// </summary>
        public string ActionName { get; set; }

        [Ignore]
        public List<MoveDisModel> ListDisLimit { get; set; }



        /// <summary>
        /// 导出到XML
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            var xml = this.ToXml();
            if (ListDisLimit != null)
            {
                if (ListDisLimit.Count > 0)
                {
                    XElement xPos = new XElement(nameof(ListDisLimit));
                    foreach (var item in ListDisLimit)
                    {
                        xPos.Add(item.ExportXml());
                    }

                    xml.Add(xPos);
                }
            }
            return xml;
        }

        /// <summary>
        /// 解析Xml
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
            if (ListDisLimit != null)
            {
                ListDisLimit.Clear();
            }
            else
            {
                ListDisLimit=new List<MoveDisModel>();
            }
            
            // 解析点位
            var xPos = xElement.Element(nameof(ListDisLimit));
            if (xPos != null)
            {
                foreach (var xItem in xPos.Elements())
                {
                    MoveDisModel pos = new MoveDisModel();
                    pos.ParserXml(xItem);
                    ListDisLimit.Add(pos);
                }
            }
        }
    }

    public class MoveDisModel : IXMLParser
    {

        /// <summary>
        /// 轴
        /// </summary>
        public string Axis { get; set; }

        /// <summary>
        /// 最小距离
        /// </summary>
        public double MinPos { get; set; }

        /// <summary>
        /// 最大距离
        /// </summary>
        public double MaxPos { get; set; }


        /// <summary>
        /// 导出到XML
        /// </summary>
        /// <returns></returns>
        public virtual XElement ExportXml()
        {
            return this.ToXml();
        }

        /// <summary>
        /// 解析Xml
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }
    }
}