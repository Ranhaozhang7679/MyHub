#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IName
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       IName.cs
* 创建时间:       2022/6/17 10:28:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      95cfba82-8b12-4f89-a97e-2c067326dcf7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/17 10:28:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Interfaces
{
    public interface IName
    {
        /// <summary>
        /// 具有名称
        /// </summary>
        string Name { get; set; }
    }
}