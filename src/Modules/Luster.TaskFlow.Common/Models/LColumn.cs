#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LColumn
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Interfaces
* 文 件 名:       LColumn
* 创建时间:       2021/10/31 15:01:14
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2155e45a-79bc-4743-bf1d-d91ccff7b08c
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 15:01:14
* 修 改 人:		  luster
************************************************************************************/
#endregion


using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common
{
    public class LColumn
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 对应的模块
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 对应的ParameterAttribute 的Name 属性
        /// </summary>
        public string ParamKey { get; set; }

        /// <summary>
        /// 当前值对象
        /// </summary>
        public object Value { get; set; }

        public LColumn()
        {
        }

        public LColumn(ParameterAttribute p)
        {
            Alias = p.Alias;
            ParamKey = p.Name;
            Value = p.Value;
            ID = p.Owner.ID;
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null) return false;

        //    if (obj is LColumn dataColumn)
        //    {
        //        return dataColumn.ID == ID && dataColumn.ParamKey == ParamKey;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public override int GetHashCode()
        //{
        //    return ID.GetHashCode() ^ ParamKey.GetHashCode();
        //}

        public static bool operator ==(LColumn left, LColumn right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is not null && right is null)
            {
                return false;
            }

            if (left is null && right is not null)
            {
                return false;
            }

            return left.ID == right.ID && left.ParamKey == right.ParamKey;
        }

        public static bool operator !=(LColumn left, LColumn right)
        {
            return !(left == right);
        }
    }
}
