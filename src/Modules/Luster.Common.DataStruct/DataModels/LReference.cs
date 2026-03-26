#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LReference
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LReference.cs
* 创建时间:       2022/6/17 20:23:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dea9839f-5282-4856-a531-ccbd6d3759fc
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/17 20:23:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 引用
    /// </summary>
    public class LReference
    {
        /// <summary>
        /// 引用模块ID
        /// </summary>
        public Guid RefID { get; set; }

        /// <summary>
        /// 引用参数名称
        /// </summary>
        public string RefName { get; set; }

        /// <summary>
        /// 被引用名称
        /// </summary>
        public string ByRefName { get; set; }
    }
}