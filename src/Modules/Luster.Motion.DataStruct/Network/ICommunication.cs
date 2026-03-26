using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Network
{
    /// <summary>
    /// 通信接口
    /// </summary>
    public interface ICommunication : IXMLParser
    {
        //用于在停止时取消通信，以及在VCommunication-IHome中使用
        CancellationTokenSource tokenSource { get; set; }

        /// <summary>
        /// 是否连接成功
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// log事件
        /// </summary>
        public event Action<LogType, string> LogEvent;

        /// <summary>
        /// 通信打开
        /// </summary>
        CommResult Open(int timeout = 2000);

        /// <summary>
        /// 通信关闭
        /// </summary>
        CommResult Close();

        void ClearCache();

        /// <summary>
        /// 发送并接收
        /// </summary>
        /// <param name="command">字节</param>
        /// <param name="receiveLen">接收的长度 如果为0只是发送</param>
        /// <param name="errLen">errlen 用于判断错误长度</param>
        /// <returns>接收的字节长度</returns>
        List<byte> Send(List<byte> command, int receiveLen = 0, int errLen = -1, int timeoutMs = 2000);

        /// <summary>
        /// 读取所有
        /// </summary>
        /// <returns></returns>
        List<byte> ReceiveToEnd(CancellationTokenSource source, ManualResetEventSlim pauseRest, int timeout = 10000);

        /// <summary>
        /// 连接尝试
        /// </summary>
        /// <returns></returns>
        bool TryConnect();
    }

    /// <summary>
    /// 协议解析
    /// </summary>
    public interface IProtocol
    {
        /// <summary>
        /// 暂停
        /// </summary>
        ManualResetEventSlim PauseReset { get; set; }

        /// <summary>
        /// 日志事件
        /// </summary>
        event Action<LogType, string> LogEvent;

        /// <summary>
        /// 读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout);

        /// <summary>
        /// 写入信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="data"></param>
        /// <param name="writeMsg"></param>
        /// <returns></returns>
        CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg);

        /// <summary>
        /// 等待值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="readMsg"></param>
        /// <param name="val"></param>
        /// <param name="timeout"></param>
        /// <param name="sleep"></param>
        /// <param name="tokenS"></param>
        void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable;
    }
}
