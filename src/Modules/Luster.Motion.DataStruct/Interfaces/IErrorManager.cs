using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 系统错误配置
    /// 1、支持错误设置
    /// 2、支持
    /// </summary>
    public interface IErrorManager : IXMLParser
    {
        /// <summary>
        /// 返回错误详细
        /// </summary>
        /// <param name="alarmInfo"></param>
        /// <param name="system"></param>
        /// <returns></returns>
        ErrorDetail GetErrorDetail(AlarmInfo alarmInfo, string system = "Default");

        /// <summary>
        /// 添加错误类型
        /// </summary>
        /// <param name="errType">错误类型</param>
        void AddErrorType(string errType, string desc);

        /// <summary>
        /// 获取所有错误类型
        /// </summary>
        /// <returns></returns>
        List<ErrorInfo> GetErrorInfos();

        /// <summary>
        /// 加载错误配置
        /// </summary>
        /// <param name="errConfig"></param>
        void LoadErrorConfig(string errConfig, IDeviceEngine deviceEngine);
    }
}
