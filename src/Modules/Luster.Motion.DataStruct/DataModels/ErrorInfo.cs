using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Attributes;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// ErrorInfo 类型
    /// </summary>
    public class ErrorInfo : IXMLParser
    {
        /// <summary>
        /// 标准错误码
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// 错误详情
        /// </summary>
        public List<ErrorDetail> Errors { get; set; }

        /// <summary>
        /// 用户处理方式
        /// </summary>
        public List<string> UserActions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ErrorInfo()
        {
            ErrorType = "None";
            Errors = new List<ErrorDetail>();
            UserActions = new List<string>()
            {
                "1.XXXXX"
            };
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eType"></param>
        public ErrorInfo(string eType, string desc = "")
        {
            ErrorType = eType;
            Errors = new List<ErrorDetail>()
            {
                // 创建默认的配置
                new ErrorDetail(eType)
                {
                    Message = desc
                }
            };

            // 用户动作
            UserActions = new List<string>()
            {
                "1.XXXXX"
            };
        }

        /// <summary>
        /// 导出Xml
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xErrInfo = new XElement("Error");
            xErrInfo.SetAttributeValue("ErrorType", ErrorType);
            if (Errors != null && Errors.Count > 0)
            {
                foreach (var item in Errors)
                {
                    xErrInfo.Add(item.ExportXml());
                }
            }

            // 用户动作
            XElement xAction = new XElement("Actions");
            if (UserActions != null && UserActions.Count > 0)
            {
                foreach (var item in UserActions)
                {
                    var xItem = new XElement("Item");
                    xItem.Value = item;

                    xAction.Add(xItem);
                }
            }

            xErrInfo.Add(xAction);

            return xErrInfo;
        }

        /// <summary>
        /// 解析配置
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            Errors?.Clear();
            UserActions?.Clear();
            xElement.GetAttribute("ErrorType", (eType) =>
            {
                ErrorType = eType;
            });

            // 解析对应的 ErrorItem
            foreach (var xItem in xElement.Elements("ErrorItem"))
            {
                ErrorDetail eDetail = new ErrorDetail();
                eDetail.ParserXml(xItem);

                Errors.Add(eDetail);
            }

            // 添加Actions
            var xAction = xElement.Element("Actions");
            if (xAction != null)
            {
                foreach (var item in xAction.Elements())
                {
                    UserActions.Add(item.Value?.ToString());
                }
            }
        }
    }

    /// <summary>
    /// 错误详细
    /// </summary>
    public class ErrorDetail : IXMLParser
    {
        /// <summary>
        /// 系统错误
        /// </summary>
        public string System { get; set; } = "Default";

        /// <summary>
        /// 对应的错误码
        /// </summary>
        public string Code { get; set; } = "None";

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 系统附加信息
        /// </summary>
        public string Addition { get; set; }


        /// <summary>
        /// 英文描述
        /// </summary>
        public string ForeignMessage { get; set; } = "Axis Error";

        /// <summary>
        /// 对应的错误图标
        /// </summary>
        /// 
        public string Icon { get; set; }

        /// <summary>
        /// 设备异常才显示名称
        /// </summary>
        public string Name { get; set; }


        public string AlarmType { get; set; } = "InfoTip";

        /// <summary>
        /// 报警开始时间
        /// </summary>
        public string AlarmTime { get; set; }

        /// <summary>
        /// 数组
        /// </summary>
        public List<DeivceError> Devices { get; set; } = new List<DeivceError>();

        /// <summary>
        /// 用户处理措施
        /// </summary>
        [Ignore]
        public List<string> UserActions { get; set; } = new List<string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ErrorDetail()
        {
        }

        /// <summary>
        /// 有参构造函数
        /// </summary>
        /// <param name="eCode"></param>
        public ErrorDetail(string eCode)
        {
            Code = eCode;
        }

        public XElement ExportXml()
        {
            XElement xDevice = new XElement("ErrorItem");
            xDevice.SetAttributeValue("System", System);

            if (!string.IsNullOrEmpty(Code))
            {
                xDevice.SetAttributeValue("Code", Code);
            }

            if (!string.IsNullOrEmpty(Message))
            {
                xDevice.SetAttributeValue("Message", Message);
            }

            if (!string.IsNullOrEmpty(ForeignMessage))
            {
                xDevice.SetAttributeValue("ForeignMessage", ForeignMessage);
            }

            if (Devices != null && Devices.Count > 0)
            {
                foreach (var item in Devices)
                {
                    xDevice.Add(item.ExportXml());
                }
            }

            return xDevice;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Code", sys =>
            {
                Code = sys;
            });

            xElement.GetAttribute("System", sys =>
            {
                System = sys;
            });

            xElement.GetAttribute("Message", sys =>
            {
                Message = sys;
            });


            xElement.GetAttribute("ForeignMessage", sys =>
            {
                ForeignMessage = sys;
            });

            Devices.Clear();
            foreach (var item in xElement.Elements())
            {
                DeivceError deivceError = new DeivceError();
                deivceError.ParserXml(item);
                Devices.Add(deivceError);
            }
        }
    }

    public class DeivceError : IXMLParser
    {
        /// <summary>
        /// 对应的错误码
        /// </summary>
        public string Code { get; set; } = "None";

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// 对应的设备ID
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xDevice = new XElement("DeviceError");
            xDevice.SetAttributeValue("Name", Name);
            xDevice.SetAttributeValue("ID", ID);
            xDevice.SetAttributeValue("Code", Code);
            xDevice.SetAttributeValue("Message", Message);

            return xDevice;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Name", id =>
            {
                Name = id;
            });

            xElement.GetAttribute("ID", id =>
            {
                ID = id;
            });

            xElement.GetAttribute("Code", id =>
            {
                Code = id;
            });

            xElement.GetAttribute("Message", id =>
            {
                Message = id;
            });
        }
    }
}
