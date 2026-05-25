using Luster.Common.DataStruct.Interfaces;
using System;
using System.IO;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    /// <summary>
    /// 登录界面配置（记忆登录模式、登录等级等选择）
    /// </summary>
    public class LoginConfig : IXMLParser
    {
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "LoginConfig.xml");

        /// <summary>
        /// 登录模式（0=FXCard, 1=Offline）
        /// </summary>
        public int LoginMode { get; set; } = 0;

        /// <summary>
        /// 登录等级（0=Admin, 1=Integrator, 2=Maintenance, 3=Operator）
        /// </summary>
        public int LoginLevel { get; set; } = 1;

        public XElement ExportXml()
        {
            return new XElement("LoginConfig",
                new XElement("LoginMode", LoginMode),
                new XElement("LoginLevel", LoginLevel));
        }

        public void ParserXml(XElement xElement)
        {
            if (int.TryParse(xElement.Element("LoginMode")?.Value, out int mode))
                LoginMode = mode;
            if (int.TryParse(xElement.Element("LoginLevel")?.Value, out int level))
                LoginLevel = level;
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void Save()
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            ExportXml().Save(_configPath);
        }

        /// <summary>
        /// 从文件加载配置，不存在则返回默认值
        /// </summary>
        public void Load()
        {
            if (File.Exists(_configPath))
            {
                ParserXml(XElement.Load(_configPath));
            }
        }
    }
}
