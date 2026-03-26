#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ClassModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       ClassModel.cs
* 创建时间:       2022/8/15 13:29:54
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      52b73ca4-5ecd-4e8f-bc63-6e01fe214587
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/15 13:29:54
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{

    public class SysOperateIOModel : IXMLParser
    {
        /// <summary>
        /// 稼动IO名称
        /// </summary>
        public SystemOperation SysOperateType { get; set; }

        /// <summary>
        /// IOList
        /// </summary>
        [Ignore]
        public List<SysSetIOModel> ListIO { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Ignore]
        public string Desc { get; set; }


        /// <summary>
        /// 描述
        /// </summary>
        [Ignore]
        public string IODesc { get; set; }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            ListIO = new List<SysSetIOModel>();
            var element = xElement.Elements("ListIO");
            if (element != null)
            {
                foreach (var xItems in element.Elements("SysSetIOModel"))
                {
                    var vdevicevar = new SysSetIOModel();
                    vdevicevar.ParserXml(xItems);
                    ListIO.Add(vdevicevar);
                }
            }
            element.Remove();
            this.FromXml(xElement);
        }



        public XElement ExportXml()
        {
            var root = this.ToXml();
            if (ListIO != null)
            {
                var listIOElement = new XElement("ListIO");
                foreach (var model in ListIO)
                {
                    var element = model.ExportXml();
                    listIOElement.Add(element);
                }
                root.Add(listIOElement);
            }
            return root;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetDescName()
        {
            Desc = SysOperateType.GetDescription();

            if (ListIO == null) return;
            foreach (var item in ListIO)
            {
                if (string.IsNullOrEmpty(IODesc))
                {
                    IODesc = item.SetIO.Name;
                }
                else
                {
                    IODesc = $"{IODesc},{item.SetIO.Name}";
                }
            }
        }




    }

    public class SysSetIOModel: IXMLParser
    {
        /// <summary>
        /// IO
        /// </summary>
        public VDevice SetIO { get; set; }

        /// <summary>
        /// IO状态
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        ///是否设置
        /// </summary>
        public bool IsSet { get; set; }

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
