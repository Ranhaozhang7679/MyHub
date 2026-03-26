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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 门类型
    /// </summary>
    public enum DoorType
    {
        [Description("门锁")]
        DoorLock,

        [Description("门磁")]
        DoorContact
    }

    public class DoorLockModel : IXMLParser
    {
        /// <summary>
        /// 门锁名称
        /// </summary>
        public DoorType DoorType { get; set; }

        /// <summary>
        /// IOList
        /// </summary>
        [Ignore]
        public List<VDevice> ListLockIO { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Ignore]
        public string Desc { get; set; }


        /// <summary>
        /// 描述
        /// </summary>
        [Ignore]
        public string LockIODesc { get; set; }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            ListLockIO = new List<VDevice>();
            var element = xElement.Elements("ListIO");
            if (element != null)
            {
                foreach (var xItems in element.Elements("VDevice"))
                {
                    var vdevicevar = new VDevice();
                    vdevicevar.ParserXml(xItems);
                    ListLockIO.Add(vdevicevar);
                }
            }
            element.Remove();
            this.FromXml(xElement);
        }



        public XElement ExportXml()
        {
            var root = this.ToXml();
            if (ListLockIO != null)
            {
                var listIOElement = new XElement("ListIO");
                foreach (var model in ListLockIO)
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
            Desc = DoorType.GetDescription();

            if (ListLockIO == null) return;
            foreach (var item in ListLockIO)
            {
                if (string.IsNullOrEmpty(LockIODesc))
                {
                    LockIODesc = item.Name;
                }
                else
                {
                    LockIODesc = $"{LockIODesc},{item.Name}";
                }
            }
        }




    }
}
