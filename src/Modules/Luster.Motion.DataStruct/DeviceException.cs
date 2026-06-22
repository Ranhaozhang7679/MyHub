#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceException
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct
* 文 件 名:       DeviceException.cs
* 创建时间:       2022/5/6 9:49:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9430d318-63d9-4b53-bcb8-3f802da0d54c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/6 9:49:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 设备异常
    /// </summary>
    public class DeviceException : Exception
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        public string AlarmCode { get; set; }

        /// <summary>
        /// 对应的设备ID
        /// </summary>
        public string DeviceID { get; set; }


        public string ModuleName {  get; set; }


        public string Name {  get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public DeviceException(string message) : base(message)
        {
        }

        public DeviceException(string message, Exception exception) : base(message, exception)
        {

        }

        /// <summary>
        /// 支持报警代码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public DeviceException(string code, string message, string moduleName="", string name = "") : base(message)
        {
            AlarmCode = code;
            ModuleName = moduleName;
            Name = name;
        }


        public DeviceException(DeviceError errorCode, Guid id) : this(errorCode.ToString())
        {
            AlarmCode = errorCode.ToString();
            DeviceID = id.ToString();
        }
        public DeviceException(string errorCode, Guid id) : this(errorCode.ToString())
        {
            AlarmCode = errorCode.ToString();
            DeviceID = id.ToString();
        }

        /// <summary>
        /// 支持报警代码、设备ID和报警描述
        /// </summary>
        /// <param name="code"></param>
        /// <param name="deviceID"></param>
        /// <param name="message"></param>
        /// <param name="moduleName"></param>
        /// <param name="name"></param>
        public DeviceException(string code, Guid deviceID, string message = "", string moduleName = "", string name = "") : base(message)
        {
            AlarmCode = code;
            DeviceID = deviceID.ToString();
            ModuleName = moduleName;
            Name = name;
        }
    }

    /// <summary>
    /// 设备运行异常，支持超时
    /// </summary>
    public class DeviceTimeoutException : TimeoutException
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        public string AlarmCode { get; set; }

        /// <summary>
        /// 对应的设备ID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备所属模块
        /// </summary>
        public string Module {  get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code">报警代码</param>
        /// <param name="message">报警消息</param>
        public DeviceTimeoutException(string code, string message, string moduleName = "",string devicename="") : base(message)
        {
            AlarmCode = code;
            Module = moduleName;
            DeviceName = devicename;
        }

        public DeviceTimeoutException(DeviceError code, Guid deviceID, string message = "",string moduleName = "", string devicename = "") : base(message)
        {
            AlarmCode = code.ToString();
            DeviceID = deviceID.ToString();
            Module = moduleName;
            DeviceName= devicename;
        }
        public DeviceTimeoutException(string code, Guid deviceID, string message = "", string moduleName = "", string devicename = "") : base(message)
        {
            AlarmCode = code;
            DeviceID = deviceID.ToString();
            Module = moduleName;
            DeviceName = devicename;
        }

        public DeviceTimeoutException(SystemError code, string message = "",string moduleName="") : base(message)
        {
            AlarmCode = code.ToString();
        }

    }

    public class DeviceTipException : Exception
    {

        /// <summary>
        /// 报警代码
        /// </summary>
        public string AlarmCode { get; set; }

        /// <summary>
        /// 对应的设备ID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="code">报警代码</param>
        /// <param name="message">报警消息</param>
        public DeviceTipException(string code, string message) : base(message)
        {
            AlarmCode = code;
        }

        public DeviceTipException(DeviceError code, Guid deviceID, string message = "") : base(message)
        {
            AlarmCode = code.ToString();
            DeviceID = deviceID.ToString();
        }

        public DeviceTipException(SystemError code, string message = "") : base(message)
        {
            AlarmCode = code.ToString();
        }
    }
}