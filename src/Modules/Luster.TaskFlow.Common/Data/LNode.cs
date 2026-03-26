#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LNode
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LNode
* 创建时间:       2021/10/31 17:38:17
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4ef1a6a3-6d9f-4179-a9fa-180cfb82c685
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 17:38:17
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Data
{
    /// <summary>
    /// 树节点类
    /// </summary>
    public class LNode
    {
        /// <summary>
        /// 节点
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Tips { get; set; }

        /// <summary>
        /// 隶属层级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 存放对象结构
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 临时存放对象结果
        /// </summary>
        public object Tag1 { get; set; }

        /// <summary>
        /// 对应的父节点
        /// </summary>
        public LNode Parent { get; set; }

        /// <summary>
        /// 图标信息
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<LNode> Children { get; set; }

        public LNode()
        {
            Children = new List<LNode>();
            Icon = string.Empty;
        }
    }
}
