using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    /// <summary>
    /// 错误代码获取
    /// </summary>
    public interface IErrorFunction
    {
        /// <summary>
        /// 获取错误代码
        /// </summary>
        /// <returns></returns>
        string GetErrorCode(string errCode, Exception ex);

        /// <summary>
        /// 获取报警错误
        /// </summary>
        /// <param name="errMessage"></param>
        /// <returns></returns>
        string GetErrorMessage(string errMessage, Exception ex);
    }
}
