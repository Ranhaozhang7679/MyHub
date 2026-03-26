using FreeSql.DataAnnotations;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 配方变更记录
    /// </summary>
    [Table(Name = "ChangeRecord")]
    public class TbChangeRecord : BaseTable
    {
        /// <summary>
        /// 变更动作 新增、编辑、删除、修改
        /// </summary>
        [PropSort(1), DisplayName("变更动作")]
        public string ChangeType { get; set; }

        /// <summary>
        /// 变更模块
        /// </summary>
        [PropSort(2), DisplayName("变更模块")]
        public string Module { get; set; }

        /// <summary>
        /// 变更属性
        /// </summary>
        [PropSort(3), DisplayName("变更属性")]
        public string Property { get; set; }

        /// <summary>
        /// 变更内容
        /// </summary>
        [PropSort(4), DisplayName("变更内容")]
        public string Content { get; set; }

        [PropSort(5),DisplayName("变更人")]
        public string Role {  get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is TbChangeRecord record)
            {
                return record.ChangeType == this.ChangeType && record.Module == this.Module && record.Property == this.Property;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
