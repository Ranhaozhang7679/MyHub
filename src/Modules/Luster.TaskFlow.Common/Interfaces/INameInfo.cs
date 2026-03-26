#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       INameInfo
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Interfaces
* 文 件 名:       INameInfo
* 创建时间:       2021/10/30 10:24:41
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7b965b72-5419-46c1-864f-f561930a1604
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 10:24:41
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Interfaces
{
    public interface INameInfo : IName
    {
        /// <summary>
        /// 别名
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// 提示
        /// </summary>
        string Tips { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        string Icon { get; set; }
    }
}
