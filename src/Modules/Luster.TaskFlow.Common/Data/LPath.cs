#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LPath
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LPath
* 创建时间:       2021/11/5 14:06:26
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      cc5dbacf-10db-444c-8ba2-e8c33c306044
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/5 14:06:26
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.Common.Extensions;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Common.Data
{
    public class LPath : IXMLParser
    {
        public PathType PathType { get; set; }

        public string Path { get; set; }

        public LPath()
        {

        }

        public LPath(Uri uri)
        {
            if (uri == null)
            {
                Path = string.Empty;
            }
            else
            {
                Path = uri.LocalPath;
            }
        }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("Path");
            xRoot.SetAttributeValue("PathType", PathType.ToString());
            xRoot.SetAttributeValue("Path", Path);

            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Path", path => this.Path = path);
            xElement.GetAttribute("PathType", pType => this.PathType = (PathType)Enum.Parse(typeof(PathType), pType));
        }
    }
}
