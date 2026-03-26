using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Interfaces
{
    /// <summary>
    /// 获取引用模块
    /// </summary>
    public interface IRefFunction
    {
        /// <summary>
        /// 引用的模块
        /// </summary>
        /// <returns></returns>
        List<LReference> GetReferences();
    }
}
