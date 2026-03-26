#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RecipeModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       RecipeModel.cs
* 创建时间:       2023/2/27 15:55:24
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      a5688747-6e4e-4b9b-b5ea-6326bc4201ed
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/27 15:55:24
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
    public class RecipeModel : IXMLParser
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        ///配方名称
        /// </summary>
        public string RecipeName { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string RecipePath { get; set; }

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