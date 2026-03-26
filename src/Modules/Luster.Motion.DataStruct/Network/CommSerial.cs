#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommSerial
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Network
* 文 件 名:       CommSerial.cs
* 创建时间:       2022/9/18 11:26:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      286807f3-d884-4382-9060-50b619e89cfd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/18 11:26:57
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.Network
{
    public class CommSerial : CommunicationBase
    {
        /// <summary>
        /// 串口通信
        /// </summary>
        private SerialPort serialPort = null;

        protected new int readTimeout { get; set; } = 1500;
        public override bool IsConnected => serialPort != null && serialPort.IsOpen;

        /// <summary>
        /// 端口
        /// </summary>
        public string PortName { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 数据位
        /// </summary>
        public int Databits { get; set; }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// 奇偶校验
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// 连接
        /// </summary>
        private /*static*/ object lockSerial = new object();

        /// <summary>
        /// 通信端口打开
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override CommResult Open(int timeout = 2000)
        {
            CommResult result = new CommResult(true);

            // 如果依据连接了，直接出去
            if (serialPort != null && serialPort.IsOpen) return result;

            try
            {
                // 防止多线程造成多次连接
                lock (lockSerial)
                {
                    if (serialPort == null)
                    {
                        serialPort = new SerialPort();
                        serialPort.PortName = PortName;
                        serialPort.BaudRate = BaudRate;
                        serialPort.DataBits = Databits;
                        serialPort.StopBits = StopBits;
                        serialPort.Parity = Parity;
                        serialPort.ReceivedBytesThreshold = 1;
                        serialPort.ReadTimeout = 20;
                    }

                    int count = 0;
                    while (true)
                    {
                        if (serialPort.IsOpen)
                            break;

                        if (count > timeout)
                        {
                            throw new TimeoutException($"Com Port:{PortName} Opened Timeout");
                        }

                        try
                        {
                            serialPort.Open();
                            break;
                        }
                        catch (System.IO.IOException)
                        {
                            Thread.Sleep(10);

                            // 异常抛出会比较耗时，所以要加上100
                            count += 200;
                            continue;
                        }
                    }
                }

                if (serialPort == null || !serialPort.IsOpen)
                    throw new Exception("Com Open Fail");
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 端口关闭
        /// </summary>
        /// <returns></returns>
        public override CommResult Close()
        {
            CommResult result = new CommResult(true);
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                    serialPort.Close();
                serialPort = null;
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
            }

            return result;
        }

        // 发送读写，保证在一个线程里面
        private /*static*/ object lockCom = new object();

        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="receiveLen"></param>
        /// <param name="errLen"></param>
        /// <returns></returns>
        public override List<byte> Send(List<byte> command, int receiveLen = -1, int errLen = -1, int timeoutMs = 2000)
        {
            lock (lockCom)
            {
                if (command != null && command.Count > 0)
                {

                    serialPort.DiscardInBuffer();
                    serialPort.Write(command.ToArray(), 0, command.Count);

                    // Modbus发出命令，防止太快，等2ms，用于读取值
                    Thread.Sleep(20);
                }

                // 支持正常情况下的字节的接收、需要判断异常情况下的字节
                int flag = 0;
                List<byte> receiveBytes = new List<byte>();

                int eachWaitMs = 10;//循环的延迟
                int totalMs = eachWaitMs + serialPort.ReadTimeout;//总操作的延时
                // 不一定说需要使用时间来做超时退出：尝试多少次
                while (flag < readTimeout / totalMs && receiveBytes.Count < receiveLen)
                {
                    try
                    {
                        // 程序点击停止，对串口进行空检查
                        if (serialPort == null)
                        {
                            break;
                        }

                        int rByte = serialPort.ReadByte();
                        if (rByte == -1)
                        {
                            break;
                        }
                        else
                        {
                            byte data = (byte)rByte;
                            receiveBytes.Add(data);
                        }
                        //Thread.Sleep(eachWaitMs);
                    }
                    catch (Exception ex)
                    {
                        if (receiveBytes.Count == errLen) break;
                    }
                    Thread.Sleep(eachWaitMs);
                    flag++;
                }

                return receiveBytes;
            }
        }

        public override List<byte> ReceiveToEnd(CancellationTokenSource source, ManualResetEventSlim pauseRest, int timeout = 2000)
        {
            int nActualReadCount = 0;  //读到的字符串
            List<byte> result = new List<byte>();
            result.Clear();
            //从开始计算
            DateTime startTime = DateTime.Now;
            //先查看缓存是否为空，为空就要等一会
            int nLen = serialPort.BytesToRead;   //当前要读取的
            int Toatl = nLen;  //一共要读取的
                               //防止串口接触不良，可多次读取
            while (nActualReadCount < Toatl || Toatl == 0)
            {
                TimeSpan diffT = DateTime.Now - startTime;
                //超时就退出来
                if (diffT.TotalMilliseconds > timeout)
                {
                    break;
                }
                byte[] buffer = new byte[nLen];
                int rcvCnt = serialPort.Read(buffer, 0, nLen);
                result.AddRange(buffer);//载入缓存
                nActualReadCount += rcvCnt;  //查看读取了多少字符
                                             //读完再更新一下，看有没有内容
                nLen = serialPort.BytesToRead;  //只要在时间内缓存有的都读出来
                Toatl += nLen;
            }
            return result;
            //var receiveLen = serialPort.BytesToRead;
            //byte[] buffer = new byte[receiveLen];
            //serialPort.Read(buffer, 0, receiveLen);
            //return new List<byte>(buffer);
        }

        public override XElement ExportXml()
        {
            var xRoot = base.ExportXml();
            xRoot.SetAttributeValue("PortName", PortName);
            xRoot.SetAttributeValue("BaudRate", BaudRate);
            xRoot.SetAttributeValue("Databits", Databits);
            xRoot.SetAttributeValue("StopBits", StopBits);
            xRoot.SetAttributeValue("Parity", Parity);

            return xRoot;
        }

        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);
            xElement.GetAttribute("PortName", p => PortName = p);
            xElement.GetAttribute("BaudRate", p => BaudRate = int.Parse(p));
            xElement.GetAttribute("Databits", p => Databits = int.Parse(p));
            xElement.GetAttribute("StopBits", p => StopBits = (StopBits)Enum.Parse(typeof(StopBits), p));
            xElement.GetAttribute("Parity", p => Parity = (Parity)Enum.Parse(typeof(Parity), p));
        }

        public override string ToString()
        {
            //return $"Port:{PortName} BaudRate:{BaudRate} Databits:{Databits} StopBits:{StopBits} Parity{Parity}";
            return $"{PortName}/{BaudRate}/{Databits}/{StopBits}/{Parity}";
        }

        /// <summary>
        /// 心跳检测
        /// </summary>
        /// <returns></returns>
        public override bool TryConnect()
        {
            return serialPort != null && serialPort.IsOpen;
        }
    }
}