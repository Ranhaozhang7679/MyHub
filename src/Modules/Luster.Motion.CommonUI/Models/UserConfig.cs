#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UserConfig
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       UserConfig.cs
* 创建时间:       2022/8/4 17:07:31
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      3cb4bdfa-e3a2-4f59-8649-533dcf18e881
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/4 17:07:31
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    public class UserConfig : IXMLParser
    {
        /// <summary>
        /// 配置保存路径
        /// </summary>
        string _userConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "UserConfig.xml");

        public List<UserModel> Users { get; set; }

        public XElement ExportXml()
        {
            var xml = new XElement("Users");
            foreach (var item in Users)
            {
                var xElemet = item.ExportXml();
                xml.Add(xElemet);
            }
            return xml;
        }

        public void ParserXml(XElement xElement)
        {
            Users = new List<UserModel>();
            foreach (var xItems in xElement.Elements("UserModel"))
            {
                var user = new UserModel();
                user.ParserXml(xItems);
                Users.Add(user);
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void SaveUserConfig()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_userConfigPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_userConfigPath));//创建路径
            }
            XElement xe = ExportXml();
            xe.Save(_userConfigPath);
        }

        /// <summary>
        /// 加载
        /// </summary>
        public void LoadUserConfig()
        {
            if (File.Exists(_userConfigPath))
            {
                XElement userConfig = XElement.Load(_userConfigPath);
                ParserXml(userConfig);
            }
            else
            {
                Users = UserModel.InitUsers();
                SaveUserConfig();
            }
        }
    }
}
