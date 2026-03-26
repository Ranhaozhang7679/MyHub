#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PathType
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Enums
* 文 件 名:       PathType
* 创建时间:       2021/11/5 14:07:02
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      764821ca-1a44-4734-9910-c49a1b96faff
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/5 14:07:02
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Enums
{
    public enum PathType
    {
        /// <summary>
        /// 绝对路径
        /// </summary>
        Absolute,

        /// <summary>
        /// 相对路径
        /// </summary>
        Relative
    }
}
