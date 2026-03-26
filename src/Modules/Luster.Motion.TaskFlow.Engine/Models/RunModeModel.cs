#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RunModeModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       RunModeModel.cs
* 创建时间:       2023/2/10 14:32:53
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      ba774257-eff2-4629-87b8-7722dd38c862
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/10 14:32:53
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class RunModeModel : IXMLParser
    {
        /// <summary>
        /// 模式
        /// </summary>
        public string Mode { get; set; }


        [Ignore]
        public List<GlobalModel> SelectGlobalVar { get; set; }

        /// <summary>
        /// 当前运行模式
        /// </summary>
        public bool IsRunMode { get; set; }

        public RunModeModel()
        {
            SelectGlobalVar = new List<GlobalModel>();
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            SelectGlobalVar = new List<GlobalModel>();
            var element = xElement.Elements("SelectGlobalVar");
            if (element != null)
            {
                foreach (var xItems in element.Elements("GlobalModel"))
                {
                    var globalvar = new GlobalModel();
                    globalvar.ParserXml(xItems);
                    SelectGlobalVar.Add(globalvar);
                }
            }
            element.Remove();
            this.FromXml(xElement);
        }



        public XElement ExportXml()
        {
            var root = this.ToXml();
            if (SelectGlobalVar != null)
            {
                var globalVarElement = new XElement("SelectGlobalVar");
                foreach (var model in SelectGlobalVar)
                {
                    var element = model.ExportXml();
                    globalVarElement.Add(element);
                }
                root.Add(globalVarElement);
            }
            return root;
        }

    }
}