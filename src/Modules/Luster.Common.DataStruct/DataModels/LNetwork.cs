#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LNetwork
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LNetwork.cs
* 创建时间:       2022/7/4 21:19:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      32b08ca0-2b38-4f8a-9062-92c9106fca8b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 21:19:59
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Luster.Common.DataStruct.DataModels
{
    public class LNetwork : IXMLParser, IEmptyObj
    {
        /// <summary>
        /// ip 地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 子网掩码
        /// </summary>
        public string Submask { get; set; }

        /// <summary>
        /// 网关
        /// </summary>
        public string Network { get; set; }

        /// <summary>
        /// 网络类型
        /// </summary>
        public string SocketRole { get; set; } = "Client";

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("Network");
            xRoot.SetAttributeValue("Ip", Ip);

            if (Port > 100)
            {
                xRoot.SetAttributeValue("Port", Port);
            }

            if (!string.IsNullOrEmpty(Submask))
            {
                xRoot.SetAttributeValue("Submask", Submask);
            }

            if (!string.IsNullOrEmpty(Network))
            {
                xRoot.SetAttributeValue("Network", Network);
            }

            if(!string.IsNullOrEmpty(SocketRole))
            {
                xRoot.SetAttributeValue("SocketRole", SocketRole);
            }

            return xRoot;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Ip);
        }

        public void ParserXml(XElement xElement)
        {
            XElement xRoot = new XElement("Network");
            xRoot.SetAttributeValue("Ip", Ip);

            xElement.GetAttribute("Ip", (ip) =>
            {
                Ip = ip;
            });

            xElement.GetAttribute("Port", (portStr) =>
            {
                if (int.TryParse(portStr, out var port))
                {
                    Port = port;
                }
            });

            xElement.GetAttribute("Submask", (subMask) =>
            {
                Submask = subMask;
            });

            xElement.GetAttribute("Network", (network) =>
            {
                Network = network;
            });

            xElement.GetAttribute("SocketRole", (socketrole) =>
            {
                SocketRole = socketrole;
            });

        }

        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }
    }
}