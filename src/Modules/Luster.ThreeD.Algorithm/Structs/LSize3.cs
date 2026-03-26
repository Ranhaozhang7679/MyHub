#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LSize
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Models
* 文 件 名:       LSize.cs
* 创建时间:       2021/12/16 10:28:18
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      275d6ff7-53d5-4a94-97b7-556f2af9d281
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/16 10:28:18
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.ThreeD.Algorithm.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LSize3
    {
        public double XLen;

        public double YLen;

        public double ZLen;

        public LSize3(double xLen, double yLen, double zLen)
        {
            XLen = xLen;
            YLen = yLen;
            ZLen = zLen;
        }


        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns>对象XML配置</returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("Size3");
            xRoot.SetAttributeValue("XLen", XLen.ToString());
            xRoot.SetAttributeValue("YLen", YLen.ToString());
            xRoot.SetAttributeValue("ZLen", ZLen.ToString());

            return xRoot;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">对象配置文件</param>
        public void ParserXml(XElement xElement)
        {
            XAttribute xX = xElement.Attribute("XLen");
            if (xX != null)
            {
                XLen = Convert.ToDouble(xX.Value);
            }

            XAttribute xY = xElement.Attribute("YLen");
            if (xY != null)
            {
                YLen = Convert.ToDouble(xY.Value);
            }

            XAttribute xZ = xElement.Attribute("ZLen");
            if (xZ != null)
            {
                ZLen = Convert.ToDouble(xZ.Value);
            }
        }
    }
}
