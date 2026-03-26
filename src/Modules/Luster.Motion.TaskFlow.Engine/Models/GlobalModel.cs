#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BlobalModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       BlobalModel.cs
* 创建时间:       2022/10/28 9:57:36
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      8078984b-665a-476f-825b-e726a57e89a9
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/28 9:57:36
* 修 改 人:		  L05590
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

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class GlobalModel : IXMLParser
    {
        /// <summary>
        /// 全局变量名称
        /// </summary>
        public string GlobalName { get; set; }

        /// <summary>
        /// 全局变量key
        /// </summary>
        public string GlobalKey { get; set; }

        /// <summary>
        /// 是否选择
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }



        public XElement ExportXml()
        {
            return this.ToXml();
        }
    }
}