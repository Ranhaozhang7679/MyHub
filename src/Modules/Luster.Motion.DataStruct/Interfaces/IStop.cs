using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 停止接口
    /// </summary>
    public interface IStop
    {
        string Name { get; set; }

        void Stop();
    }
}
