using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum SocketRole
    {
        [Description("客户端")]
        Client = 0,

        [Description("服务器")]
        Server = 1
    }
}
