#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ExportVIOModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       ExportVIOModel.cs
* 创建时间:       2022/10/27 10:44:06
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      1c0b509a-7ac4-4dae-97ac-196e5dcf0ced
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/27 10:44:06
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class ExportVIOModel
    {
        [DisplayName("序号")]
        public int Index { get; set; }

        [DisplayName("地址")]
        public string PortNo { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("子定义")]
        public string SubDefinite { get; set; }

        [DisplayName("模块")]
        public string ModuleName { get; set; }
    }
}
