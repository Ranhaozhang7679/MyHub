#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommTCP
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Network
* 文 件 名:       CommTCP.cs
* 创建时间:       2022/9/18 11:29:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      bc8682b1-3fa9-46a3-a623-3e8f6bead750
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/18 11:29:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.Network
{
    /// <summary>
    /// TCP通信
    /// </summary>
    public class CommTCP : CommunicationBase
    {
        /// <summary>
        /// socket通信
        /// </summary>
        private Socket socket;

        private ManualResetEvent _connectDone = new ManualResetEvent(false);
        private Exception _exception;

        /// <summary>
        /// 网络地址
        /// </summary>
        public LNetwork Network { get; set; }

        public override bool IsConnected => socket != null && socket.Connected;

        /// <summary>
        /// 每个实例自己的连接锁：避免所有设备Open被全局串行
        /// </summary>
        private readonly object _openLock = new object();

        /// <summary>
        /// 计算连接超时
        /// </summary>
        private Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// 本地IP
        /// </summary>
        public string clientIp = "";
        /// <summary>
        /// 远端IP
        /// </summary>
        public string serverIp = "";


        /// <summary>
        /// 端口打开
        /// </summary>
        /// <param name="timeout">连接超时时间</param>
        /// <returns></returns>
        public override CommResult Open(int timeout = 500)
        {
            // 快速路径：避免使用GetActiveTcpConnections这样的重操作
            if (socket != null && socket.Connected)
            {
                // 轻量断线判断：可读且无数据通常表示对端关闭
                if (!(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0))
                {
                    return new CommResult(true, "");
                }
            }

            // 如果同一个CommTCP实例有其他线程正在连接，则立即返回（不阻塞等待）
            if (!Monitor.TryEnter(_openLock))
            {
                return new CommResult(false, "Connect in progress");
            }

            try
            {
                stopwatch.Restart();

                // 二次检查（防止刚好在锁外建立成功）
                if (socket != null && socket.Connected)
                {
                    if (!(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0))
                    {
                        return new CommResult(true, "");
                    }
                }

                _connectDone.Reset();
                _exception = null;

                try
                {
                    socket?.Close();
                    socket?.Dispose();
                    socket = null;

                    var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        // 低延迟场景（小包交互）通常需要禁用Nagle
                        NoDelay = true
                    };

                    socket = newSocket;

                    newSocket.BeginConnect(Network.Ip, Network.Port, callback =>
                    {
                        try
                        {
                            var cbSocket = (Socket)callback.AsyncState;

                            // 关键：用EndConnect得到真实结果（失败会抛异常）
                            cbSocket.EndConnect(callback);

                            // 打开socket的KeepAlive机制
                            cbSocket.IOControl(IOControlCode.KeepAliveValues, KeepAlive(), null);
                        }
                        catch (Exception ex)
                        {
                            _exception = ex;
                            OnLog(LogType.Error, "ComTCPError:" + ex.Message);
                        }
                        finally
                        {
                            _connectDone.Set();
                        }
                    }, newSocket);

                    // 等待连接完成或超时
                    if (!_connectDone.WaitOne(timeout, false))
                    {
                        try
                        {
                            socket?.Close();
                        }
                        catch { }

                        throw new TimeoutException("连接超时!");
                    }

                    // 回调里产生过异常，在此抛出
                    if (_exception != null)
                        throw _exception;

                    if (socket != null && socket.Connected)
                    {
                        clientIp = ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
                        serverIp = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                        return new CommResult(true, "");
                    }

                    return new CommResult(false, $"Network:{Network} Connect failed!");
                }
                catch (Exception ex)
                {
                    // 不要吞异常，避免上层出现盲目重试导致启动变慢
                    return new CommResult(false, $"Network:{Network} Connect failed! {ex.Message}");
                }
                finally
                {
                    tokenSource?.Cancel();// 如果之前有tokenSource，则取消它
                    tokenSource = new CancellationTokenSource();
                }
            }
            finally
            {
                Monitor.Exit(_openLock);
            }
        }

        public override CommResult Close()
        {
            try
            {
                // 正常关闭连接
                if (socket?.Connected ?? false)
                    socket?.Shutdown(SocketShutdown.Both);
                Thread.Sleep(100);
            }
            catch
            {
            }

            //彻底关闭连接
            try
            {
                socket?.Close();
                socket?.Dispose();
                socket = null;
                return new CommResult(true);
            }
            catch (Exception ex)
            {
                return new CommResult(false, ex.Message);
            }
            finally
            {
                tokenSource?.Cancel();
            }
        }

        /// <summary>
        /// 对TCP进行锁住
        /// </summary>
        private static object lockTCP = new object();

        public override List<byte> Send(List<byte> command, int receiveLen = 0, int errLen = -1, int timeoutMs = 2000)
        {
            if (socket == null) return new List<byte>(); ;

            bool isBlock = socket.Blocking;
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            try
            {
                lock (lockTCP)
                {
                    if (command != null && command.Count > 0)
                    {
                        socket.Send(command.ToArray(), 0, command.Count, SocketFlags.None);
                    }

                    int bufferSize = 512;// 字节长度的处理
                    byte[] receiveBytes = new byte[receiveLen];// 定义一个正常报文长度的数组
                    int receiveByteCount = 0;// 当前已经接收到的字节数


                    if (socket != null)
                    {
                        socket.Blocking = true;
                    }

                    int flag = 0;
                    while (receiveByteCount < receiveLen && receiveByteCount != errLen)
                    {
                        // 分批读取
                        // 需要读取的字节数（正常报文的完整长度-已经接收到的字节数）大于缓冲长度的话，只需要读取缓冲长度的字节
                        int receiveLength = Math.Min(bufferSize, (receiveLen - receiveByteCount));
                        try
                        {
                            //超时之后释放Socekt,使PDCA退出后重新连接
                            if (sw.ElapsedMilliseconds >= timeoutMs)
                            {
                                socket?.Close();
                                socket?.Dispose();
                                socket = null;
                                return new List<byte>();
                                //throw new Exception($"Recevice TCP Data Timeout:{socket?.RemoteEndPoint}");
                            }
                            // 参数：存放接收字节的数组，从第几个字节开始存入
                            if (socket.Poll(PollSecond, SelectMode.SelectRead))
                            {
                                var readLeng = socket.Receive(receiveBytes, receiveByteCount, receiveLength, SocketFlags.None);
                                if (readLeng == 0)
                                {
                                    throw new Exception("No Recevice Data");
                                }
                                receiveByteCount += readLeng;
                            }
                            else
                            {
                                throw new Exception($"Recevice Data Fail:{socket.RemoteEndPoint}");
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Recevice Data Fail:" + ex.Message);
                        }
                        flag++;
                    }

                    return new List<byte>(receiveBytes);
                }
            }
            catch
            {
                return new List<byte>();
            }
            finally
            {
                if (socket != null && socket.Connected)
                {
                    // 如果连接正常，则设置为非阻塞
                    socket.Blocking = isBlock;
                }
                sw?.Stop();
            }
        }

        public override List<byte> ReceiveToEnd(CancellationTokenSource source, ManualResetEventSlim pauseRest, int timeout = 10000)
        {
            List<byte> respBytes = new List<byte>();

            int bufferSize = 512;// 字节长度的处理
            int receiveByteCount = 0;// 当前已经接收到的字节数

            // 2.定义接收信号超过2s中超时一次
            int receiveTimeout = 2000;

            if (socket == null)
            {
                return respBytes;
            }

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, receiveTimeout);

            int time = 0;

            bool isBlocking = socket.Blocking;
            // 在进行Receive之前要进行显示的配置blocking为True
            socket.Blocking = true;
            while (!source.Token.IsCancellationRequested)
            {
                if (time > timeout || socket == null)
                {
                    throw new TimeoutException($"Communication:{socket?.RemoteEndPoint} Respose Data timeout >={timeout}ms");
                }

                // 暂停
                if (pauseRest != null && !pauseRest.IsSet)
                {
                    pauseRest.Wait();

                    // 如果先暂停，后停止
                    if (socket == null)
                    {
                        OnLog(LogType.Debug, $"Communication:{socket?.RemoteEndPoint} stopped externally!");
                        break;
                    }
                }

                // 分批读取
                // 需要读取的字节数（正常报文的完整长度-已经接收到的字节数）大于缓冲长度的话，只需要读取缓冲长度的字节
                try
                {
                    byte[] receiveBytes = new byte[bufferSize];// 定义一个正常报文长度的数组

                    // 参数：存放接收字节的数组，从第几个字节开始存入
                    var readLeng = socket.Receive(receiveBytes, 0, bufferSize, SocketFlags.None);
                    if (readLeng == 0)
                    {
                        time += 100;
                        Thread.Sleep(100);
                        continue;
                    }

                    if (readLeng < bufferSize)
                    {
                        respBytes.AddRange(receiveBytes.Take(readLeng));
                        break;
                    }
                    else
                    {
                        respBytes.AddRange(receiveBytes);
                    }

                    receiveByteCount += readLeng;
                }
                catch (InvalidOperationException i)
                {
                    // 服务器中断了
                    time += 100;
                    Thread.Sleep(100);
                    continue;
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10060)
                    {
                        // 超时的时候错误号码是10060
                        time += receiveTimeout;
                    }
                    else
                    {
                        // 服务器中断了
                        time += 100;
                        Thread.Sleep(100);
                    }

                    continue;
                }
                catch (Exception ex)
                {
                    throw new Exception("Abnormal response data received!" + ex.Message);
                }
            }

            socket.Blocking = isBlocking;

            return respBytes;
        }

        /// <summary>
        /// 清理通信缓存
        /// </summary>
        public override void ClearCache()
        {
            if (socket == null || !socket.Connected)
            {
                return;
            }

            bool isHasCache = socket.Poll(1000, SelectMode.SelectRead);
            if (isHasCache)
            {
                int receiveByteCount = 0;
                while (true)
                {
                    byte[] receiveBytes = new byte[bufferSize];// 定义一个正常报文长度的数组

                    // 参数：存放接收字节的数组，从第几个字节开始存入
                    try
                    {
                        var readLeng = socket.Receive(receiveBytes, receiveByteCount, bufferSize, SocketFlags.None);
                        if (readLeng == 0 || readLeng < bufferSize)
                        {
                            break;
                        }

                        receiveByteCount += readLeng;
                    }
                    catch (Exception e)
                    {
                        OnLog(LogType.Error, $"Communication Exception:{e.Message}");
                        break;
                    }
                    finally
                    {
                        Thread.Sleep(1);
                    }
                }
            }
        }

        /// <summary>
        /// 打开socket网络监控，用于判断连接掉线：拔掉网线等异常。
        /// 默认打开监控，如果连接在3秒没有数据传递，则开始发送监听包，500ms发送一次，一共发送2次，如果2次都失败则代表掉线。
        /// </summary>
        /// <param name="onOff">是否开启KeepAlive</param>
        /// <param name="keepAliveTime">开始首次KeepAlive探测前的TCP空闭时间</param>
        /// <param name="keepAliveInterval">两次KeepAlive探测间的时间间隔</param>
        /// <returns></returns>
        private byte[] KeepAlive(bool onOff = true, int keepAliveTime = 3000, int keepAliveInterval = 500)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff ? 1 : 0).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        /// <summary>
        /// 判断连接是否在线
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private bool isSocketConnected(Socket socket)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation c in tcpConnections)
            {
                if (c == null || c.LocalEndPoint == null) continue;
                if (socket == null) continue;

                TcpState stateOfConnection = c.State;

                if (c.LocalEndPoint.Equals(socket.LocalEndPoint) && c.RemoteEndPoint.Equals(socket.RemoteEndPoint))
                {
                    return stateOfConnection == TcpState.Established;
                }
            }
            return false;
        }

        /// <summary>
        /// 通信
        /// </summary>
        private Socket hSocket = null;

        /// <summary>
        /// 线程阻断
        /// </summary>
        protected ManualResetEvent hReset = new ManualResetEvent(false);

        /// <summary>
        /// 测试连接 不建立对象
        /// </summary>
        /// <returns></returns>
        public override bool TryConnect()
        {
            bool blockingState = false;
            try
            {
                if (hSocket != null && hSocket.Connected)
                {
                    blockingState = hSocket.Blocking;

                    byte[] tmp = new byte[1];
                    hSocket.Blocking = false;
                    hSocket.Send(tmp, 0, 0);
                    return true;
                }

                // 使用Ping做快速探测
                PingReply pingResult;
                using (var ping = new Ping())
                {
                    pingResult = ping.Send(Network.Ip);
                }

                if (pingResult == null || pingResult.Status != IPStatus.Success)
                {
                    return false;
                }

                // Ping通了再尝试TCP连接验证端口
                hSocket?.Close();
                hSocket?.Dispose();
                hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };

                hReset.Reset();

                hSocket.BeginConnect(Network.Ip, Network.Port, callback =>
                {
                    try
                    {
                        var cbSocket = (Socket)callback.AsyncState;
                        cbSocket.EndConnect(callback);
                        cbSocket.IOControl(IOControlCode.KeepAliveValues, KeepAlive(), null);
                    }
                    catch
                    {
                        // ignore: 由Connected判断
                    }
                    finally
                    {
                        hReset.Set();
                    }
                }, hSocket);

                hReset.WaitOne(500);

                return hSocket.Connected;
            }
            catch (SocketException e)
            {
                // 产生 10035 == WSAEWOULDBLOCK 错误，说明被阻止了，但是还是连接的
                if (e.NativeErrorCode.Equals(10035))
                {
                    return true;
                }

                hSocket?.Close();
                hSocket = null;
                return false;
            }
            catch (PingException)
            {
                return false;
            }
            finally
            {
                // 恢复hSocket的blocking状态（原代码误恢复到了socket）
                if (hSocket != null)
                    hSocket.Blocking = blockingState;
            }
        }

        /// <summary>
        /// Xml导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            var xRoot = base.ExportXml();

            // 添加网络
            xRoot.Add(Network.ExportXml());
            return xRoot;
        }

        /// <summary>
        /// Xml解析
        /// </summary>
        /// <param name="xElement"></param>
        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);
            Network = new LNetwork();
            var xNet = xElement.Element("Network");
            if (xNet != null)
            {
                Network.ParserXml(xNet);
            }
        }

        public override string ToString()
        {
            return Network?.ToString();
        }
    }
}