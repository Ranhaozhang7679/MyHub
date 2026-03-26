#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleSetModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       ModuleSetModel.cs
* 创建时间:       2022/11/4 16:22:37
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      b47d55bc-513b-4a91-b292-c881fe48f6af
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/4 16:22:37
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
    public class ModuleSetModel: IXMLParser
    {

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DockKeyName
        /// </summary>
        public string DockName { get; set; }


        /// <summary>
        /// 控件名称
        /// </summary>
        public string ControlName { get; set; }


        /// <summary>
        /// 是否选择
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 控件名称
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// 对象转换XML
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml();
        }

        /// <summary>
        /// XML转换对象
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {

            this.FromXml(xElement);
        }
    }
}