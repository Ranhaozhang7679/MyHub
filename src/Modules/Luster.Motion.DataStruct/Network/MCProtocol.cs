using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Network
{
    public abstract class MCProtocol : IProtocol
    {


        /// <summary>
        /// 解析报文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="respByte"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        protected CommResult<T> Analysis<T>(List<byte> respByte)
        {
            CommResult<T> result = new CommResult<T>();
            try
            {
                //区分是否是bool类型
                if(typeof(T).Equals(typeof(bool)))
                {
                    Type tConvert = typeof(Convert);
                    // 查找Convet这个类里面的ToBoolean方法
                    MethodInfo method = tConvert.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(mi => mi.Name == "ToBoolean") as MethodInfo;
                    if (method != null)
                    {
                        for (int i = 0; i < respByte.Count/2; i++)
                        {                       
                            result.Datas.Add((T)method.Invoke(tConvert, new object[] { respByte[2 * i] % 8>0 ? true:false}));
                        }
                    }
                    else
                    {
                        throw new Exception("未找到匹配的数据类型转换方法");
                    }
                }
                // 数值型
                else
                {
                    int typeLen = System.Runtime.InteropServices.Marshal.SizeOf<T>();
                    for (int i = 0; i < respByte.Count / 2; i++)
                    {
                        List<byte> valueByte = new List<byte>();

                        for (int sit = 0; sit < typeLen; sit++)
                        {
                            valueByte.Add(respByte[i++]);
                        }

                        Type tBitConverter = typeof(BitConverter);
                        MethodInfo method = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(mi => mi.ReturnType == typeof(T)) as MethodInfo;
                        if (method == null)
                            throw new Exception("未找到匹配的数据类型转换方法");

                        result.Datas.Add((T)method?.Invoke(tBitConverter, new object[] { valueByte.ToArray(), 0 }));
                    }
                }
               
            }
            catch(Exception ex) 
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
            }

            return result;


        }

        /// <summary>
        /// 解析指令
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="count"></param>
        /// <param name="startAddr"></param>
        /// <param name="type"></param>
        public void ParseMsg(string msg,out ushort count, out ulong startAddr,out byte type) 
        { 
            //类型
            // X 9C
            // Y 9D
            // M 90
            // L 92
            // F 93
            // V 94
            // B A0
            // D A8
            // W B4
            var msgArray=msg.Split(' ');

            switch (msgArray[0].ToString())
            {
                case "X":
                    type = 0x9C;
                    break;
                case "Y":
                    type = 0x9D;
                    break;
                case "M":
                    type = 0x90;
                    break;
                case "L":
                    type = 0x92;
                    break;
                case "F":
                    type = 0x93;
                    break;
                case "V":
                    type = 0x94;
                    break;
                case "B":
                    type = 0xA0;
                    break;
                case "D":
                    type = 0xA8;
                    break;
                case "W":
                    type = 0xB4;
                    break;
                default:
                    type = 0x90;
                    break;
            }
            startAddr = ulong.Parse(msgArray[1]);
            count = ushort.Parse(msgArray[2]);
        }

        //生成读指令
        public List<byte> BuildReadCommand<T>(ICommunication communication,string readMsg,out ushort addrCount)
        {
            //发送
            //50 00 00 FF FF 03 00 0E 00 10 00 01 14 00 00 58 1B 00 A8 01 00 0C 00
            //50 00 指令报文副头部
            //      00 网络编号 
            //         FF 可编程控制器网络编号
            //            FF 03 请求目标模块I/O编号
            //                  00 请求目标模块站号
            //                     12 00 请求数据长度
            //                            10 00 CPU监视定时器
            //                                 01 04 指令批量读
            //                                       00 00 子指令
            //                                             58 1B 00 起始软元件
            //                                                      A8 软元件代码
            //                                                         02 00 软元件点数      
            ParseMsg(readMsg, out ushort count,out ulong startAddr, out byte type);
            ConvertStartAddr(startAddr, type,out byte[] startBytes);
            List<byte> byteList = new List<byte>();
            byteList.Add(0x50);
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.Add(0xFF);
            byteList.Add(0xFF);
            byteList.Add(0x03);
            byteList.Add(0x00);
            byteList.Add(0x0C);
            byteList.Add(0x00);
            byteList.Add(0x10);
            byteList.Add(0x00);
            byteList.Add(0x01);
            byteList.Add(0x04);
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.AddRange(startBytes);
            byteList.Add(type);
            byteList.AddRange(BitConverter.GetBytes(count));
            addrCount = count;
            return byteList;
        }

        //生成写指令
        public List<byte> BuildWriteCommand<T>(ICommunication communication, List<T> data, string writeMsg, out ushort addrCount)
        {
            //发送
            //50 00 00 FF FF 03 00 0E 00 10 00 01 14 00 00 58 1B 00 A8 01 00 0C 00
            //50 00 指令报文副头部
            //      00 网络编号 
            //         FF 可编程控制器网络编号
            //            FF 03 请求目标模块I/O编号
            //                  00 请求目标模块站号
            //                     12 00 请求数据长度
            //                            10 00 CPU监视定时器
            //                                 01 04 指令批量读
            //                                       00 00 子指令
            //                                             58 1B 00 起始软元件
            //                                                      A8 软元件代码
            //                                                         02 00 软元件点数
            //                                                               0C 00 软元件点数的数据
            List<byte> byteList = new List<byte>();
            ParseMsg(writeMsg, out ushort count, out ulong startAddr, out byte type);
            ConvertStartAddr(startAddr, type,out byte[] startBytes);
            byteList.Add(0x50);
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.Add(0xFF);
            byteList.Add(0xFF);
            byteList.Add(0x03);
            byteList.Add(0x00);
            byteList.AddRange(BitConverter.GetBytes((ushort)(count*2+12)));
            byteList.Add(0x10);
            byteList.Add(0x00);
            byteList.Add(0x01);
            byteList.Add(0x14);
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.AddRange(startBytes);
            byteList.Add(type);
            byteList.AddRange(BitConverter.GetBytes(count));
            if (type == 0x9C || type == 0x9D || type == 0x90 || type == 0x92 || type == 0x93 || type == 0x94 || type == 0xA0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    byteList.AddRange(BitConverter.GetBytes((ushort)(Convert.ToBoolean(data[i]) ? 1 : 0)));
                }
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    byteList.AddRange(BitConverter.GetBytes(ushort.Parse((data[i].ToString()))));
                }
            }

            addrCount = count;     
            return byteList;
        }
        public ManualResetEventSlim PauseReset { get ; set ; }
        public event Action<LogType, string> LogEvent;
        public abstract CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout);
        public abstract CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg);
        public abstract void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable;

        /// <summary>
        /// log 触发
        /// </summary>
        /// <param name="log"></param>
        protected void OnLog(LogType logType, string logMsg)
        {
            LogEvent?.Invoke(logType, logMsg);
        }


        //16进制string转成byte数组
        public  byte[] ConvertHexStringToBytes(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("参数长度不正确,必须是偶数位。");
            }
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return returnBytes;
        }

        //转换首地址，输出byte
        public void ConvertStartAddr(ulong startAddr,byte type,out byte[] bytes)
        {
            
            if (type == 0x9C || type == 0x9D || type == 0x90 || type == 0x92|| type == 0x93|| type == 0x94|| type == 0xA0)
            {
                string hexStr = startAddr.ToString().PadLeft(6, '0');
                bytes = ConvertHexStringToBytes(hexStr);
                Array.Reverse(bytes);
            }
            else
            {
                bytes = new byte[3];
                Array.Copy((BitConverter.GetBytes(startAddr)), bytes, 3);
            }
        }

    }


    public class MCBinary : MCProtocol
    {      
        //读寄存器
        public override CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
 
            var byteList = BuildReadCommand<T>(communication, readMsg,out ushort count) ;


            CommResult state = new CommResult(true);
            if(!communication.IsConnected)
            {
                state = communication.Open(timeout);
            }
            if(state.IsSuccess)
            {
              List<Byte> respBytes= communication.Send(byteList,11+count*2);
             //去除固定格式
              respBytes.RemoveRange(0,11);
              return this.Analysis<T>(respBytes);
            }
            state.IsSuccess = true;
            state.ErrorMsg = "";
            List<byte> bytes = new List<byte>();
            CommResult<T> stat = new CommResult<T>();

            var strResult = Encoding.Default.GetString(bytes.ToArray()) as object;
            stat.Datas.Add((T)strResult);

            return stat;

        }

        //写寄存器
        public override CommResult Write<T>(ICommunication communication, List<T> datas, string writeMsg)
        {
            if (string.IsNullOrEmpty(writeMsg))
            {
                throw new ArgumentNullException(writeMsg);
            }
            CommResult result = new CommResult(true);

            try
            {
                if (communication.IsConnected)
                {
                    List<byte> byteList = BuildWriteCommand(communication, datas, writeMsg, out ushort count);
                    List<byte> respBytes = communication.Send(byteList, 11);
                    if(respBytes.Count==0)
                    {
                        throw new Exception($"通讯连接失败:{communication}!");
                    }

                    if(respBytes.Count!=11)
                    {
                        result.IsSuccess = false;
                        result.ErrorMsg = "error";
                    }
                }
                else
                {
                    throw new Exception($"通讯连接失败:{communication},请重新回零!");
                }

            }
            catch (Exception e) 
            {
             result.IsSuccess= false;
             result.ErrorMsg = e.Message;
            }
            return result;
        }

        //等寄存器
        public override void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource stopCancel)
        {
            var byteList=BuildReadCommand<T>(communication,readMsg,out ushort count);
            CommResult state=new CommResult(true);
            if(!communication.IsConnected)
            {
                state = communication.Open(timeout);
            }

            if (state.IsSuccess)
            {
                int flag = 0;
                // 持续等待
                while (true)
                {
                    if (!communication.IsConnected)
                    {
                        OnLog(LogType.Debug, $"Wait {readMsg} {opRule} {val} 网络停止!");
                        break;
                    }

                    // 支持外部中断
                    if (stopCancel != null && stopCancel.IsCancellationRequested)
                    {
                        OnLog(LogType.Debug, $"Wait {readMsg} {opRule} {val} 被外部停止!");
                        break;
                    }

                    // 暂停
                    if (PauseReset != null && !PauseReset.IsSet)
                    {
                        OnLog(LogType.Debug, $"Wait {readMsg} {opRule} {val} 被暂停!");
                        PauseReset.Wait();

                        // 如果先暂停，后停止
                        if (stopCancel.IsCancellationRequested)
                        {
                            OnLog(LogType.Debug, $"Wait {readMsg} {opRule} {val} 被外部停止!");
                            break;
                        }
                    }

                    List<byte> respBytes = communication.Send(byteList, 11 + count * 2);

                    if (respBytes.Count ==0)
                    {
                        throw new TimeoutException($"通讯连接失败:{communication}!");
                    }


                    if (respBytes.Count<11)
                    {
                        throw new FriendlyException($"校验失败");
                    }

                    // 解析数据部分
                    respBytes.RemoveRange(0, 11);

                    var result = this.Analysis<T>(respBytes);
                    if (result.IsSuccess && result.Datas.Count > 0)
                    {
                        // 第一个值和
                        if (CompareVal<T>.Compare(opRule, val, result.Datas[0]))
                        {
                            break;
                        }
                    }

                    Thread.Sleep(sleep);

                    if (timeout > 0)
                    {
                        flag += sleep;
                        if (flag > timeout)
                        {
                            throw new TimeoutException($"MC报文 {readMsg} 超时 >= {timeout}");
                        }
                    }
                }
            }
            else
            {
                throw new Exception(state.ErrorMsg);
            }

        }

    }


}
