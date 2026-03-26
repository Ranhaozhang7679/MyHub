#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommunicationBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Network
* 文 件 名:       CommunicationBase.cs
* 创建时间:       2022/7/20 17:22:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5d2b6007-4f22-49db-a34a-830d87c86c51
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 17:22:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.Network
{
    /// <summary>
    /// 基础通信
    /// </summary>
    public abstract class CommunicationBase : ICommunication
    {
        #region 常用变量
        // <summary>
        /// 读串口缓存超时时间，毫秒值
        /// </summary>
        protected int readTimeout { get; set; } = 1500;

        /// <summary>
        /// 轮巡时间
        /// </summary>
        protected const int PollSecond = 1000 * 1000;

        /// <summary>
        /// 缓存尺寸
        /// </summary>
        protected int bufferSize = 512;

        /// <summary>
        /// log事件
        /// </summary>
        public event Action<LogType, string> LogEvent;

        /// <summary>
        /// log 触发
        /// </summary>
        /// <param name="log"></param>
        protected void OnLog(LogType logType, string logMsg)
        {
            LogEvent?.Invoke(logType, logMsg);
        }

        /// <summary>
        /// 计时器
        /// </summary>
        protected Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// 线程阻断
        /// </summary>
        protected ManualResetEvent timeOutReset = new ManualResetEvent(false);
        #endregion

        /// <summary>
        /// 是否连接成功
        /// </summary>
        [Ignore]
        public virtual bool IsConnected => false;

        public CancellationTokenSource tokenSource { get; set; }

        /// <summary>
        /// 通信打开
        /// </summary>
        public abstract CommResult Open(int timeout = 2000);

        /// <summary>
        /// 通信关闭
        /// </summary>
        public abstract CommResult Close();

        /// <summary>
        /// 缓存清理
        /// </summary>
        public virtual void ClearCache()
        {
        }
        /// <summary>
        /// 协议发送
        /// </summary>
        /// <param name="command"></param>
        /// <param name="receiveLen"></param>
        /// <param name="errLen"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual List<byte> Send(List<byte> command, int receiveLen = 0, int errLen = -1, int timeoutMs = 2000)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 读取所有
        /// </summary>
        /// <returns></returns>
        public virtual List<byte> ReceiveToEnd(CancellationTokenSource source, ManualResetEventSlim pauseRest, int timeout = 2000)
        {
            throw new NotImplementedException();
        }

        public virtual XElement ExportXml()
        {
            var xComm = new XElement("Communication");
            xComm.SetAttributeValue("RealType", this.GetType().Name);

            return xComm;
        }

        public virtual void ParserXml(XElement xElement)
        {
        }

        /// <summary>
        /// 通过类型来构建真实对象
        /// </summary>
        /// <param name="realType"></param>
        /// <returns></returns>
        public static ICommunication CreateByType(Type realType)
        {
            return Activator.CreateInstance(realType) as ICommunication;
        }

        /// <summary>
        /// 连接尝试
        /// </summary>
        /// <returns></returns>
        public virtual bool TryConnect()
        {
            return IsConnected;
        }
    }
}