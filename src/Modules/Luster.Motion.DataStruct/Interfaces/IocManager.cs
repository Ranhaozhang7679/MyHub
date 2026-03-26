using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 对象解析器
    /// </summary>
    public interface IIocManager
    {
        /// <summary>
        /// 对象解析
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>();
    }
}
