#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbSort
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       TbSort.cs
* 创建时间:       2022/7/14 14:19:35
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e43c52f3-0b48-4aad-8b17-37942a394977
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/14 14:19:35
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 用于数据查询排序接口
    /// </summary>
    public class TbSort
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 正序排列
        /// </summary>
        public bool Ascending { get; set; }

        public TbSort()
        {
            Ascending = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="ascendinging">从小到大</param>
        public TbSort(string propertyName, bool ascendinging = false)
        {
            PropertyName = propertyName;
        }
    }
}