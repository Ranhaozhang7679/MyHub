#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LParameter
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       LParameter.cs
* 创建时间:       2023/2/6 16:12:49
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      83c2e6ba-ac2a-4290-a7d7-da5ea6942e28
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/6 16:12:49
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 参数配置
    /// </summary>
    public class LParams
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public List<P> Params { get; set; }

        public LParams()
        {
            Params = new List<P>();
        }
    }

    /// <summary>
    /// 参数
    /// </summary>
    public class P
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}