#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UserModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       UserModel.cs
* 创建时间:       2022/8/4 16:28:33
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      2bd31326-8b87-4f08-bd19-9907eac45723
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/4 16:28:33
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.CommonUI.Models
{
    public class UserModel : IXMLParser
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSystem { get; set; }

        //public string BadgeID { get; set; }

        /// <summary>
        /// 当前的设备模式
        /// </summary>
        public DeviceMode CurrentMode { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public SystemRole UserRole { get; set; } = SystemRole.Operator;

        /// <summary>
        /// 人员数据导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml("");
        }

        /// <summary>
        /// 人员数据导入
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        /// <summary>
        /// 初始化用户表
        /// </summary>
        /// <returns></returns>
        public static List<UserModel> InitUsers()
        {
            var users = new List<UserModel>()
            {
                new UserModel()
                {
                    Id=1,
                    UserName=SystemRole.Admin.ToString(),
                    UserRole=SystemRole.Admin,
                    Password="Luster@1996",
                },
                new UserModel()
                {
                    Id=2,
                    UserName=SystemRole.Sustaining.ToString(),
                    UserRole=SystemRole.Sustaining,
                    Password="Engineer",
                },
                new UserModel()
                {
                    Id=3,
                    UserName=SystemRole.Operator.ToString(),
                    UserRole=SystemRole.Operator,
                    Password="123456",
                },
            };

            return users;
        }
    }
}
