using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct
{
    /// <summary>
    /// 角色
    /// </summary>
    public enum SystemRole
    {
        /// <summary>
        /// 管理员
        /// </summary>
        [Description("管理员")]
        Admin,

        /// <summary>
        /// 工程师
        /// </summary>
        [Description("工程师")]
        Sustaining,

        /// <summary>
        /// 操作员
        /// </summary>
        [Description("操作员")]
        Operator
    }
}
