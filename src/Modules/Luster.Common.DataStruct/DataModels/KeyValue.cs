#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       KeyValue
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Models
* 文 件 名:       KeyValue.cs
* 创建时间:       2022/1/6 20:29:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      b2040941-fe97-4af1-bd2f-8953544110f5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/6 20:29:51
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
    public class KeyValue
    {
        /// <summary>
        /// 关键字
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 支持记忆
        /// </summary>
        public  bool IsMemoric { get; set; }

        /// <summary>
        /// 回零复位值
        /// </summary>
        public bool IsHomeDefault {  get; set; }
    }
}