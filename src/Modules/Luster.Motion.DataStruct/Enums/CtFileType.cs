#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CtFileType
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       CtFileType.cs
* 创建时间:       2022/10/26 11:01:19
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      63350cab-c07b-4afb-9ef4-5d1182ae59b5
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/26 11:01:19
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum CtFileType
    {
        [Description("产品SN")]
        Product,

        [Description("工站")]
        Module
    }
}
