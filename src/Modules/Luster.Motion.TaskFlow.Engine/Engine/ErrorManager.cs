using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Engine
{
    public class ErrorManager : IErrorManager
    {
        /// <summary>
        /// ErrorInfos集合
        /// </summary>
        private List<ErrorInfo> errorInfos;

        public ErrorManager()
        {
            errorInfos = new List<ErrorInfo>();
        }


        public void AddErrorType(string errType, string desc)
        {
            if (string.IsNullOrEmpty(errType))
            {
                throw new FriendlyException($"错误类型不能为空:{errType}");
            }

            string errTypeTrim = errType.Trim();
            if (!errorInfos.Any(u => u.ErrorType == errTypeTrim))
            {
                errorInfos.Add(new ErrorInfo(errTypeTrim, desc));
            }
        }

        /// <summary>
        /// 获取对应系统的ErrorCode
        /// </summary>
        /// <param name="eType">错误类型</param>
        /// <param name="system">隶属系统</param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetErrorCode(string eType, string system, out string errMsg)
        {
            errMsg = "";
            string retCode = DeviceError.None.ToString();
            var errorInfo = errorInfos.FirstOrDefault(u => u.ErrorType == eType);
            if (errorInfo != null)
            {
                // 默认使用默认系统
                if (string.IsNullOrEmpty(system))
                {
                    system = "Default";
                }

                var sysError = errorInfo.Errors.FirstOrDefault(u => u.System == system);
                if (sysError != null)
                {
                    errMsg = sysError.Message;
                    retCode = sysError.Code;
                }
            }

            return retCode;
        }

        public ErrorDetail GetErrorDetail(AlarmInfo alarmInfo, string system = "Default")
        {
            ErrorDetail errorInfo = null;
            ErrorInfo err=null;
            if(alarmInfo.AlarmCode!=null)
            {
                err = errorInfos.FirstOrDefault(u => alarmInfo.AlarmCode.EndsWith(u.ErrorType));
                if (err != null)
                {
                    var sysError = err.Errors.FirstOrDefault(u => u.System == system);
                    if (sysError != null)
                    {
                        // 设备报警
                        if (!string.IsNullOrEmpty(alarmInfo.DeviceID))
                        {
                            var deviceErr = sysError.Devices.FirstOrDefault(u => u.ID == alarmInfo.DeviceID);
                            if (deviceErr != null)
                            {
                                errorInfo = new ErrorDetail()
                                {
                                    Code = deviceErr.Code,
                                    System = system,
                                    //Message = $"ErrorCode:{deviceErr.Code} 描述:{deviceErr.Name} {deviceErr.Message}",
                                    Message = deviceErr.Message,
                                    Addition = alarmInfo.Message,
                                    Icon = alarmInfo.AlarmIcon,
                                    Name = deviceErr.Name,
                                    UserActions = err.UserActions,
                                    AlarmType = alarmInfo.AlarmType.ToString(),
                                    AlarmTime = alarmInfo.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                };
                            }
                        }
                        else
                        {
                            errorInfo = new ErrorDetail()
                            {
                                //临时措施
                                Code = sysError.Code,
                                System = system,
                                Message = sysError.Message,
                                Addition = alarmInfo.Message,

                                Name = "",
                                UserActions = err.UserActions,
                                AlarmType = alarmInfo.AlarmType.ToString(),
                                AlarmTime = alarmInfo.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                        }
                    }
                }
            }
            

            if (errorInfo == null)
            {
                errorInfo = new ErrorDetail()
                {
                    Code = alarmInfo.AlarmCode,
                    System = system,
                    Message= alarmInfo.Message,
                    Addition = alarmInfo.Message,
                    UserActions = new List<string>()
                    {
                        "1、待 XXXX"
                    },
                    AlarmType = alarmInfo.AlarmType.ToString(),
                    AlarmTime = alarmInfo.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
                };

                if (err != null)
                {
                    errorInfo.UserActions = err.UserActions;
                }

                if (alarmInfo.Sender is IName name)
                {
                    errorInfo.Name = name.Name;
                }
            }

            return errorInfo;
        }

        /// <summary>
        /// 获取ErrorInfo
        /// </summary>
        /// <returns></returns>
        public List<ErrorInfo> GetErrorInfos()
        {
            return errorInfos;
        }

        /// <summary>
        /// 加载错误配置
        /// </summary>
        /// <param name="errConfig"></param>
        public void LoadErrorConfig(string errConfig, IDeviceEngine deviceEngine)
        {
            try
            {
                // 如果文档存在就解析文件
                if (File.Exists(errConfig))
                {
                    XElement xRoot = XElement.Load(errConfig);
                    this.ParserXml(xRoot);
                }

                // 基于系统的SystemError 自动构建错误类型
                foreach (var item in typeof(SystemError).GetEnumValues())
                {
                    if (!errorInfos.Any(u => u.ErrorType == item.ToString()))
                    {
                        errorInfos.Add(new ErrorInfo(item.ToString(), item.GetDescription()));
                    }
                };

                // 硬件错误
                foreach (var item in typeof(DeviceError).GetEnumValues())
                {
                    if (item.ToString() == DeviceError.None.ToString()) continue;

                    if (!errorInfos.Any(u => u.ErrorType == item.ToString()))
                    {
                        var err = new ErrorInfo(item.ToString(), item.GetDescription());
                        errorInfos.Add(err);
                    }
                };

                // 硬件对应的每个设备错误
                var devices = deviceEngine.GetVDevices<IDeviceError>();
                foreach (var device in devices)
                {
                    foreach (var item in device.ErrorCodes)
                    {
                        var err = errorInfos.FirstOrDefault(u => u.ErrorType == item.ToString());
                        if (err.Errors.Count() > 0)
                        {
                            var dErrors = err.Errors[0].Devices;
                            if (!dErrors.Any(u => u.ID == device.ID.ToString()))
                            {
                                dErrors.Add(new DeivceError()
                                {
                                    ID = device.ID.ToString(),
                                    Code = item.ToString(),
                                    Message = item.GetDescription(),
                                    Name = device.Name,
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 配置解析错误
            }
        }

        /// <summary>
        /// 配置解析
        /// </summary>
        /// <param name="xElement"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ParserXml(XElement xElement)
        {
            errorInfos = new List<ErrorInfo>();

            foreach (var item in xElement.Elements())
            {
                ErrorInfo eInfo = new ErrorInfo();
                eInfo.ParserXml(item);
                errorInfos.Add(eInfo);
            }

        }

        /// <summary>
        /// 导出配置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public XElement ExportXml()
        {
            XElement xErr = new XElement("Errors");
            if (errorInfos != null && errorInfos.Count > 0)
            {
                foreach (var item in errorInfos)
                {
                    xErr.Add(item.ExportXml());
                }
            }

            return xErr;
        }
    }
}
