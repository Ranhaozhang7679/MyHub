#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BreadItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       BreadItem.cs
* 创建时间:       2022/7/1 16:45:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      46c98231-476f-4e02-95f9-325db0c16a91
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/1 16:45:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Models
{
    public class BreadItem : BindableBase
    {
        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 是否第一个
        /// </summary>
        public bool IsFirst { get; set; }

        /// <summary>
        /// 是否最后一个节点
        /// </summary>
        public bool IsLast { get; set; }
        /// <summary>
        /// 当前值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<BreadItem> Children { get; set; }
    }
}