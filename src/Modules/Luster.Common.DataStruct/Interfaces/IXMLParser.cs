#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IXMLParser
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Interfaces
* 文 件 名:       IXMLParser
* 创建时间:       2021/10/29 17:56:15
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      52e18499-377f-4eb1-860f-374e8d936703
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/29 17:56:15
* 修 改 人:		  luster
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.Interfaces
{
    /// <summary>
    /// 对象序列化
    /// </summary>
    public interface IXMLParser
    {
        /// <summary>
        /// 导出到Xml
        /// </summary>
        /// <returns>XElement</returns>
        XElement ExportXml();

        /// <summary>
        /// 通过Xml进行解析
        /// </summary>
        /// <param name="xElement">xElement</param>
        void ParserXml(XElement xElement);
    }
}