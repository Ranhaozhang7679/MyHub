#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LFolder
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LFolder.cs
* 创建时间:       2022/2/15 20:41:02
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      74eebeee-6943-473a-8c01-64fb5e13c25f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/15 20:41:02
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 文件夹
    /// </summary>
    public class LFolder : IXMLParser
    {
        public string Path { get; set; }

        public LFolder()
        {
        }

        public LFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Path = string.Empty;
            }
            else
            {
                Path = path;
            }
        }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("Folder");
            xRoot.SetAttributeValue("Path", Path);

            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Path", path => this.Path = path);
        }
    }
}