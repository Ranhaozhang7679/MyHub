using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Dongle;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Network
{
    public abstract class FinsProtocol : IProtocol
    {
        /// <summary>
        /// Fins头标识
        /// </summary>
        private byte[] _headCode = new byte[] { 0x46, 0x49, 0x4E, 0x53 };

        /// <summary>
        /// Fins命令码
        /// </summary>
        private byte[] _cmdCode = new byte[] { 0x00, 0x00, 0x00, 0x02 };

        /// <summary>
        /// Fins错误码
        /// </summary>
        private byte[] _errCode = new byte[] { 0x00, 0x00, 0x00, 0x00 };

        public ManualResetEventSlim PauseReset { get; set; }

        public event Action<LogType, string> LogEvent;

        public abstract CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout);

        public abstract void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable;

        public abstract CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg);

        protected void OnLog(LogType logType, string logMsg)
        {
            LogEvent?.Invoke(logType, logMsg);
        }

        //解析指令
        public void ParseMsg(string msg, out ushort count, out string startAddr, out byte type)
        {
            //类型
            // CW B0
            // WW B1
            // HW B2
            // AW B3
            // DW 82

            // CB 30
            // WB 31
            // HB 32
            // AB 33
            // DB 02
            var msgArray = msg.Split(' ');

            switch (msgArray[0].ToString())
            {
                case "CW":
                    type = 0xB0;
                    break;
                case "WW":
                    type = 0xB1;
                    break;
                case "HW":
                    type = 0xB2;
                    break;
                case "AW":
                    type = 0xB3;
                    break;
                case "DW":
                    type = 0x82;
                    break;

                case "CB":
                    type = 0x30;
                    break;
                case "WB":
                    type = 0x31;
                    break;
                case "HB":
                    type = 0x32;
                    break;
                case "AB":
                    type = 0x33;
                    break;
                case "DB":
                    type = 0x02;
                    break;
                default:
                throw new ArgumentException();

            }


            //判断是否是bool
            if(msgArray[1].Contains("."))
            {
                var regArray = msgArray[1].Split('.');
                string sReg = regArray[0].PadLeft(2, '0') + regArray[1].PadLeft(2, '0');
                startAddr= sReg;
            }
            else
            {
                startAddr = msgArray[1].PadLeft(4, '0');
            }
            count = ushort.Parse(msgArray[2]);
        }
        //转换首地址，输出首地址byte
        public void ConvertStartAddr(string startAddr, byte type, out byte[] bytes)
        {
             bytes = ConvertHexStringToBytes(startAddr);
        }
        //生成读取指令
        public List<byte> BuildReadCommand<T>(ICommunication communication, string readMsg,out ushort addrCount)
        {
            string clientIp;
            string serverIp;
            string lastIndexClientIp;
            string lastIndexServerIp;

            List<byte> lbytes = new List<byte>();

            clientIp = ((CommTCP)communication).clientIp;
            serverIp = ((CommTCP)communication).serverIp;
            lastIndexClientIp = clientIp.Substring(clientIp.Length - 1, 1);
            lastIndexServerIp = serverIp.Substring(serverIp.Length - 1, 1);
            ParseMsg(readMsg, out ushort count, out string startAddr, out byte type);
            ConvertStartAddr(startAddr, type, out byte[] startByte);

            //头部 0~3
            lbytes.AddRange(_headCode);
            //后续字节长度 4~7
            lbytes.Add(0x00);
            lbytes.Add(0x00);
            lbytes.Add(0x00);
            lbytes.Add(0x1A);
            //命令码8~11
            lbytes.AddRange(_cmdCode);
            //错误码12~15
            lbytes.AddRange(_errCode);

            //UDP / IP帧
            //IP帧 16~19
            lbytes.Add(0x80);//ICF: 发送接收标志字节，发送报文：ICF = 80HEX；响应报文：ICF = C0HEX
            lbytes.Add(0x00);//RSV: 固定为00HEX
            lbytes.Add(0x02);//RSV: 固定为00HEX
            lbytes.Add(0x00);//DNA: 目标网络号；本网络：00；远程网络：01 - 7F

            //20~23
            lbytes.AddRange((Encoding.ASCII.GetBytes(lastIndexServerIp)));//PLC的IP地址最后一位
            lbytes.Add(0x00);//DA2: 目标单元号；对于CPU来说，固定为00;
            lbytes.Add(0x00);//SNA: 源网络号；本网络：00
            lbytes.AddRange((Encoding.ASCII.GetBytes(lastIndexClientIp)));//PC IP地址的最后一位
            //24~25
            lbytes.Add(0x00);//SA2: 源单元号：可设置为与目标单元号相同
            lbytes.Add(0x00);//SID: 服务ID，响应端将接收过来的SID复制后添加到响应帧中


            //FINS帧
            //26~27
            //读命令
            lbytes.Add(0x01);
            lbytes.Add(0x01);
            //读区域
            //28
            lbytes.Add(type);

            //起始地址
            //29~31
            if (typeof(T).Equals(typeof(bool)))
            {
                if(startByte.Length<6)
                   lbytes.Add(0x00);
                lbytes.AddRange(startByte);
            }
            else
            {

                lbytes.AddRange(startByte);
                lbytes.Add(0x00);
            }
            //读取的数量
            //32~33
            lbytes.AddRange(BitConverter.GetBytes(count).Reverse());
            addrCount = count;
            return lbytes;
        }

        //生成写入指令
        public List<byte> BuildWriteCommand<T>(ICommunication communication, List<T> datas,string writeMsg,out ushort addrCount)
        {
            //0.头标识
            //0x46 0x49 0x4E 0x53
            //1.后续字节长度
            //0x00 0x00 0x00 0x1E
            //2.命令码
            //0x00 0x00 0x00 0x02
            //3.错误代码
            //0x00 0x00 0x00 0x00
            //4.消息头
            //0x80 0x00 0x02 0x00
            //5.目标地址
            //Ox56
            //0x00 Ox00
            //6.源地址
            //0xEF
            //0x00 0x00
            //7.命令码
            //0x01 Ox02
            //8.写入区域
            //0x82
            //9.起始地址=字地址+位地址
            //0x00 0x0A 0x00
            //10.写入数量
            // 0x00 0x02
            //11.具体数值
            //0xAB 0xCD 0x12 0x34
            List<byte> lbytes = new List<byte>();

            string clientIp;
            string serverIp;
            string lastIndexClientIp;
            string lastIndexServerIp;
            int sendLength = 16;
            clientIp = ((CommTCP)communication).clientIp;
            serverIp = ((CommTCP)communication).serverIp;
            lastIndexClientIp = clientIp.Substring(clientIp.Length - 1, 1);
            lastIndexServerIp = serverIp.Substring(serverIp.Length - 1, 1);
            ParseMsg(writeMsg, out ushort count, out string startAddr, out byte type);
            ConvertStartAddr(startAddr, type, out byte[] startBytes);

            if (typeof(T).Equals(typeof(bool)))
            {
                sendLength = 26 +  1 * count;
            }
            else
            {
                sendLength = 26 + 2*count;
            }
            //头部 0~3
            lbytes.AddRange(_headCode);
            //后续字节长度 4~7
            //lbytes.Add(0x00);
            //lbytes.Add(0x00);
            //lbytes.Add(0x00);
            lbytes.AddRange(BitConverter.GetBytes(sendLength).Reverse());
            //命令码8~11
            lbytes.AddRange(_cmdCode);
            //错误码12~15
            lbytes.AddRange(_errCode);
            //UDP / IP帧
            //IP帧 16~19
            lbytes.Add(0x80);//ICF: 发送接收标志字节，发送报文：ICF = 80HEX；响应报文：ICF = C0HEX
            lbytes.Add(0x00);//RSV: 固定为00HEX
            lbytes.Add(0x02);//RSV: 固定为00HEX
            lbytes.Add(0x00);//DNA: 目标网络号；本网络：00；远程网络：01 - 7F
            //20~23
            lbytes.AddRange((Encoding.ASCII.GetBytes(lastIndexServerIp)));//PLC的IP地址最后一位
            lbytes.Add(0x00);//DA2: 目标单元号；对于CPU来说，固定为00;
            lbytes.Add(0x00);//SNA: 源网络号；本网络：00
            lbytes.AddRange((Encoding.ASCII.GetBytes(lastIndexClientIp)));//PC IP地址的最后一位
            //24~25
            lbytes.Add(0x00);//SA2: 源单元号：可设置为与目标单元号相同
            lbytes.Add(0x00);//SID: 服务ID，响应端将接收过来的SID复制后添加到响应帧中

            //FINS帧
            //26~27
            //写命令
            lbytes.Add(0x01);
            lbytes.Add(0x02);

            //写区域
            //28
            lbytes.Add(type);

            //起始地址 
            // 
            //29~31

            if (typeof(T).Equals(typeof(bool)))
            {
                //字地址+位地址
                if(startBytes.Length<6)
                   lbytes.Add(0x00);
                lbytes.AddRange(startBytes);
            }
            else
            {
                lbytes.AddRange(startBytes);
                lbytes.Add(0x00);
            }

            //写入的数量
            //30~33
            lbytes.AddRange(BitConverter.GetBytes(count).Reverse());


            //写入的值
            //34++

            if (typeof(T).Equals(typeof(bool)))
            {
                for (int i = 0; i < count; i++)
                {

                   if(bool.Parse(datas[i].ToString()))
                    {
                        lbytes.Add(0x01);
                    }
                    else
                    {   lbytes.Add(0x00); 
                    }

                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    lbytes.AddRange(BitConverter.GetBytes(ushort.Parse(datas[i].ToString())).Reverse());
                }
            }
                


            addrCount = count;  
            return lbytes;

        }

        //16进制string转成byte数组
        public byte[] ConvertHexStringToBytes(string hexString)
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
                if (typeof(T).Equals(typeof(bool)))
                {
                    Type tConvert = typeof(Convert);
                    // 查找Convet这个类里面的ToBoolean方法
                    MethodInfo method = tConvert.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(mi => mi.Name == "ToBoolean") as MethodInfo;
                    if (method != null)
                    {
                        for (int i = 0; i < respByte.Count ; i++)
                        {
                            result.Datas.Add((T)method.Invoke(tConvert, new object[] {respByte[i]}));
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
                        valueByte.Reverse();
                        
                        Type tBitConverter = typeof(BitConverter);
                        MethodInfo method = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(mi => mi.ReturnType == typeof(T)) as MethodInfo;
                        if (method == null)
                            throw new Exception("未找到匹配的数据类型转换方法");

                        result.Datas.Add((T)method?.Invoke(tBitConverter, new object[] { valueByte.ToArray(), 0 }));
                    }
                }

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
            }

            return result;


        }


    }

    public class FinsTCP : FinsProtocol
    {
        //0.读取
        public override CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
            var readByteList= BuildReadCommand<T>(communication, readMsg, out ushort count);

            CommResult state = new CommResult(true);
            if (!communication.IsConnected)
            {
                state = communication.Open(timeout);
            }
            if (state.IsSuccess)
            {
                List<Byte> respBytes = communication.Send(readByteList, 30 + count * (typeof(T).Equals(typeof(bool))?1:2));
                //去除固定格式
                respBytes.RemoveRange(0, 30);
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


        //1.写入
        public override CommResult Write<T>(ICommunication communication, List<T> datas, string writeMsg)
        {
            CommResult result = new CommResult(true);
            try
            {
                if(communication.IsConnected)
                {

                    List<byte> byteList = BuildWriteCommand(communication, datas, writeMsg, out ushort count);
                    List<byte> respBytes = communication.Send(byteList, 30);
                    if (respBytes.Count == 0)
                    {
                        throw new Exception($"通讯连接失败:{communication}!");
                    }
                    //if (respBytes[16] == 0xC0)
                    //{
                    //    result.IsSuccess = true;
                    //    result.ErrorMsg ="";
                    //}
                    //else
                    //{
                    //    result.IsSuccess = false;
                    //    result.ErrorMsg = "error";
                    //}            
                }
                else
                {
                    throw new Exception($"通讯连接失败:{communication},请重新回零!");
                }
            }
            catch (Exception e) 
            {
                result.IsSuccess = false;
                result.ErrorMsg = e.Message;
            
            }
            return result;
        }

        //2.等待
        public override void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource stopCancel)
        {
            var byteList = BuildReadCommand<T>(communication, readMsg, out ushort count);
            CommResult state = new CommResult(true);
            if (!communication.IsConnected)
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

                    List<byte> respBytes = communication.Send(byteList, 30 + count * (typeof(T).Equals(typeof(bool)) ? 1 : 2));

                    if (respBytes.Count == 0)
                    {
                        throw new TimeoutException($"通讯连接失败:{communication}!");
                    }


                    if (respBytes.Count < 30)
                    {
                        throw new FriendlyException($"校验失败");
                    }

                    // 解析数据部分
                    respBytes.RemoveRange(0, 30);

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
                            throw new TimeoutException($"Fins报文 {readMsg} 超时 >= {timeout}");
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
