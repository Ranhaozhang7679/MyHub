#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LRange
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LRange.cs
* 创建时间:       2022/2/8 13:30:32
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      e193bde5-cd8d-4b33-954c-c83d0f4ce79c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/8 13:30:32
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.DataModels
{
    public class LRange : IXMLParser
    {
        public double Min { get; set; }

        public double Max { get; set; }

        public LRange() : this(-1, 1)
        {
        }

        public LRange(double min, double max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// XML导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("Range");
            xRoot.SetAttributeValue("Min", Min);
            xRoot.SetAttributeValue("Max", Max);

            return xRoot;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Min", min => Min = double.Parse(min));
            xElement.GetAttribute("Max", max => Max = double.Parse(max));
        }

        public bool IsSatisfy(double val)
        {
            return val >= Min && val <= Max;
        }

        public override string ToString()
        {
            if (Min == Max)
            {
                return Max.ToString();
            }
            else if (Math.Abs(Min) == Max)
            {
                return $"±{Max}";
            }
            else
            {
                return $"{Min}/{Max}";
            }
        }
    }
}