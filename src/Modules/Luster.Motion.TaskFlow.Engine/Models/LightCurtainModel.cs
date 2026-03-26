#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LightCurtainModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       LightCurtainModel.cs
* 创建时间:       2022/10/26 16:43:28
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      81ab3e07-3949-479b-b588-73617735a7f1
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/26 16:43:28
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class LightCurtainModel : IXMLParser
    {
        /// <summary>
        /// 设备
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        public VDevice Device { get; set; }

        /// <summary>
        /// 全局变量
        /// </summary>
        public string Globalvar { get; set; }

        /// <summary>
        /// 全局变量Key
        /// </summary>
        public string GlobalKey { get; set; }

        /// <summary>
        /// 全局变量
        /// </summary>

        [Ignore]
        public ParameterAttribute GlobalParameter { get; set; }

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