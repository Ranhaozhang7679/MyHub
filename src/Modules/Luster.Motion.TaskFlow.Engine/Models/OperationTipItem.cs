#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OperationTipItem
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       OperationTipItem.cs
* 创建时间:       2022/12/1 15:37:09
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      16c87677-8c54-4654-bfa0-25f9966296c0
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/1 15:37:09
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class OperationTip : IXMLParser
    {
        public string GUID { get; set; }

        /// <summary>
        /// 操作提示
        /// </summary>
        private string _tip;

        public string Tip
        {
            get => _tip;
            set => _tip = value;
        }

        /// <summary>
        /// 操纵类型
        /// </summary>
        private SystemOperation _operation;
        public SystemOperation Operation
        {
            get => _operation;
            set => _operation = value;
        }

        public XElement ExportXml()
        {
            var xRoot = this.ToXml("");
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }
    }
}
