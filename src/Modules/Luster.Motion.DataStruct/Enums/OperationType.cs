using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum OperationType
    {
        [Description("新增")]
        Add,

        [Description("修改")]
        Update,

        [Description("删除")]
        Delete,

        [Description("查询")]
        Query,

        [Description("忽略")]
        Skip
    }
}
