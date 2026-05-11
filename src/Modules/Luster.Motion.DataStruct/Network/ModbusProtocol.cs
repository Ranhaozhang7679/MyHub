#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModbusProtocol
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Network
* 文 件 名:       ModbusProtocol.cs
* 创建时间:       2022/7/22 19:40:13
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6c0faa05-a2cf-41ac-b6e8-c92c2935102a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/22 19:40:13
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Network
{
    /// <summary>
    /// 类型
    /// </summary>
    public enum EndianType
    {
        ABCD,
        BADC,
        CDAB,
        DCBA,
    }

    /// <summary>
    /// IModBus接口对象
    /// </summary>
    public interface IModbus
    {

        /// <summary>
        /// 字节顺序
        /// </summary>
        EndianType EndianType { get; set; }

        /// <summary>
        /// 字符串反转
        /// </summary>
        bool ReverseString { get; set; }
    }

    public abstract class ModbusProtocol : IProtocol, IModbus
    {
        /// <summary>
        /// 暂停
        /// </summary>
        public ManualResetEventSlim PauseReset { get; set; }

        /// <summary>
        /// 日志事件
        /// </summary>
        public event Action<LogType, string> LogEvent;

        /// <summary>
        /// 字节顺序
        /// </summary>
        public EndianType EndianType { get; set; } = EndianType.ABCD;

        /// <summary>
        /// 字符串反转
        /// </summary>
        public bool ReverseString { get; set; } = false;

        /// <summary>
        /// log 触发
        /// </summary>
        /// <param name="log"></param>
        protected void OnLog(LogType logType, string logMsg)
        {
            LogEvent?.Invoke(logType, logMsg);
        }

        #region 校验方式
        protected List<byte> CRC16(List<byte> value, ushort poly = 0xA001, ushort crcInit = 0xFFFF)
        {
            if (value == null || !value.Any())
                throw new ArgumentException("");

            //运算
            ushort crc = crcInit;
            for (int i = 0; i < value.Count; i++)
            {
                crc = (ushort)(crc ^ (value[i]));
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ poly) : (ushort)(crc >> 1);
                }
            }
            byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
            byte lo = (byte)(crc & 0x00FF);         //低位置

            List<byte> buffer = new List<byte>();
            //buffer.AddRange(value);
            buffer.Add(lo);
            buffer.Add(hi);
            return buffer;
        }

        /// <summary>
        /// 错误码
        /// </summary>
        protected static Dictionary<int, string> Errors = new Dictionary<int, string>
        {
            { 0x01, "非法功能码"},
            { 0x02, "非法数据地址"},
            { 0x03, "非法数据值"},
            { 0x04, "从站设备故障"},
            { 0x05, "确认，从站需要一个耗时操作"},
            { 0x06, "从站忙"},
            { 0x08, "存储奇偶性差错"},
            { 0x0A, "不可用网关路径"},
            { 0x0B, "网关目标设备响应失败"},
        };

        /// <summary>
        /// 检测CRC校验
        /// </summary>
        /// <param name="respBytes"></param>
        /// <returns></returns>
        protected bool DoCheckCRC(List<byte> respBytes)
        {
            List<byte> checkCRC = respBytes.GetRange(respBytes.Count - 2, 2);
            respBytes.RemoveRange(respBytes.Count - 2, 2);
            var calcCRC = CRC16(respBytes);
            return checkCRC.SequenceEqual(calcCRC);
        }
        #endregion


        protected void ParseMsg(string msg, out byte address, out byte func, out ushort startAddr, out ushort len)
        {
            var msgArray = msg.Split(' ');
            if (msgArray.Length != 4)
            {
                throw new FormatException("Modbus msg format is 01 03 100 1");
            }

            address = byte.Parse(msgArray[0]);
            func = byte.Parse(msgArray[1]);
            startAddr = ushort.Parse(msgArray[2]);
            len = ushort.Parse(msgArray[3]);
        }

        /// <summary>
        /// 报文封装
        /// </summary>
        /// <param name="deviceAddr"></param>
        /// <param name="funcCode"></param>
        /// <param name="startAddr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected List<byte> GetReadCommand(byte deviceAddr, byte funcCode, ushort startAddr, ushort length)
        {
            List<byte> buffer = new List<byte>();
            buffer.Add(deviceAddr);
            buffer.Add(funcCode);
            buffer.Add(BitConverter.GetBytes(startAddr)[1]);
            buffer.Add(BitConverter.GetBytes(startAddr)[0]);
            buffer.Add(BitConverter.GetBytes(length)[1]);
            buffer.Add(BitConverter.GetBytes(length)[0]);
            return buffer;
        }

        /// <summary>
        /// 基本多写报文
        /// </summary>
        /// <param name="deviceAddr">客户端地址</param>
        /// <param name="funcCode">功能码</param>
        /// <param name="startAddr">起始地址</param>
        /// <param name="count">需要写入的寄存器数</param>
        /// <param name="len">需要写入的字节数</param>
        /// <returns></returns>
        protected List<byte> GetWriteCommand(byte deviceAddr, byte funcCode, ushort startAddr, ushort length, byte len)
        {
            List<byte> command = new List<byte>();
            command.Add(deviceAddr);
            command.Add(funcCode);
            command.Add(BitConverter.GetBytes(startAddr)[1]);
            command.Add(BitConverter.GetBytes(startAddr)[0]);

            // 写寄存器数量
            command.Add(BitConverter.GetBytes(length)[1]);
            command.Add(BitConverter.GetBytes(length)[0]);

            // 要写入寄存器的字节数
            command.Add(len);

            return command;
        }


        // 把所有情况下中的数据部分报文进行解析，根据类型来的
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="respByte"></param>
        /// <param name="len">主要是针对读线圈的时候取线圈数量</param>
        /// <returns></returns>
        protected CommResult<T> Analysis<T>(List<byte> respByte, int len)
        {
            CommResult<T> result = new CommResult<T>();
            try
            {
                // 解析线圈  Bool  
                if (typeof(T).Equals(typeof(bool)))
                {
                    respByte.Reverse();
                    var valueTemp = string.Join("", respByte.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).ToList()).ToList();
                    valueTemp.Reverse();

                    Type tConvert = typeof(Convert);
                    // 查找Convet这个类里面的ToBoolean方法
                    MethodInfo method = tConvert.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(mi => mi.Name == "ToBoolean") as MethodInfo;

                    if (method != null)
                    {
                        for (int i = 0; i < Math.Min(valueTemp.Count, len); i++)
                        {
                            result.Datas.Add((T)method.Invoke(tConvert, new object[] { int.Parse(valueTemp[i].ToString()) }));
                        }
                    }
                    else
                    {
                        throw new Exception("未找到匹配的数据类型转换方法");
                    }
                }
                else
                {
                    if (typeof(T) != typeof(string))
                    {
                        int typeLen = System.Runtime.InteropServices.Marshal.SizeOf<T>();
                        for (int i = 0; i < respByte.Count;)
                        {
                            List<byte> valueByte = new List<byte>();

                            for (int sit = 0; sit < typeLen; sit++)
                            {
                                valueByte.Add(respByte[i++]);
                            }
                            // 字节序的调整
                            valueByte = new List<byte>(this.SwitchEndian(valueByte.ToArray(), EndianType));

                            //T value = default(T);
                            Type tBitConverter = typeof(BitConverter);
                            MethodInfo method = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static)
                                .FirstOrDefault(mi => mi.ReturnType == typeof(T)) as MethodInfo;
                            if (method == null)
                                throw new Exception("未找到匹配的数据类型转换方法");

                            result.Datas.Add((T)method?.Invoke(tBitConverter, new object[] { valueByte.ToArray(), 0 }));
                        }

                    }
                    else
                    {
                        // 启用字符串反转
                        if (ReverseString)
                        {
                            List<byte> bytes = new List<byte>();
                            for (int i = 0; i < respByte.Count; i += 2)
                            {
                                bytes.Add(respByte[i + 1]);
                                bytes.Add(respByte[i]);
                            }

                            respByte = bytes;
                        }

                        if (len % 2 != 0)
                        {
                            respByte.RemoveAt(respByte.Count - 1);
                        }

                        var strResult = Encoding.Default.GetString(respByte.ToArray()) as object;
                        result.Datas.Add((T)strResult);
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

        /// <summary>
        /// 需要支持2个字节的调整
        /// 需要支持4个字节的调整
        /// 需要支持8个字节的调整
        /// </summary>
        /// <param name="value"></param>
        /// <param name="endianType"></param>
        /// <returns></returns>
        protected byte[] SwitchEndian(byte[] value, EndianType endianType)
        {
            List<byte> result = new List<byte>(value);
            switch (endianType)
            {
                case EndianType.ABCD:
                    result.Reverse();
                    return result.ToArray();

                case EndianType.CDAB: // 4字节处理
                    if (value.Length == 4)
                    {
                        result[3] = value[2];
                        result[2] = value[3];
                        result[1] = value[0];
                        result[0] = value[1];
                    }
                    return result.ToArray();
                case EndianType.BADC: // 4字节处理
                    if (value.Length == 4)
                    {
                        result[3] = value[1];
                        result[2] = value[0];
                        result[1] = value[3];
                        result[0] = value[2];
                    }
                    return result.ToArray();
                case EndianType.DCBA:
                    return value;
                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// 需要支持2个字节的调整
        /// 需要支持4个字节的调整
        /// 需要支持8个字节的调整
        /// </summary>
        /// <param name="value"></param>
        /// <param name="endianType"></param>
        /// <returns></returns>
        protected byte[] SwitchEndianString(byte[] value, EndianType endianType)
        {
            List<byte> result = new List<byte>(value);
            switch (endianType)
            {
                case EndianType.ABCD:
                    result.Reverse();
                    return result.ToArray();

                case EndianType.CDAB: // 4字节处理
                    if (value.Length == 4)
                    {
                        result[3] = value[2];
                        result[2] = value[3];
                        result[1] = value[0];
                        result[0] = value[1];
                    }
                    return result.ToArray();
                case EndianType.BADC: // 4字节处理
                    if (value.Length == 4)
                    {
                        result[3] = value[1];
                        result[2] = value[0];
                        result[1] = value[3];
                        result[0] = value[2];
                    }
                    return result.ToArray();
                case EndianType.DCBA:
                    return value;
                default:
                    break;
            }

            return null;
        }


        /// <summary>
        /// 字节检测
        /// </summary>
        /// <param name="respBytes"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="validation"></param>
        /// <returns></returns>
        protected bool CheckException(List<byte> respBytes, out string msg, Func<List<byte>, bool> validation)
        {
            msg = "OK";

            if (respBytes.Count == 0)
            {
                msg = "响应超时";
                return false;
            }

            // 响应报文的校验
            // LRC  -》 1个字节
            // TCP没有校验
            if (validation != null && !validation.Invoke(respBytes))
            {
                msg = "数据传输异常，校验不通过";
                return false;
            }


            // 功能码
            int func = respBytes[1];
            if ((func & 0x80) == 0x80)
            {
                // 如果异常，再检查异常Code
                var code = respBytes[2];
                msg = Errors.ContainsKey(code) ? Errors[code] : "自定义异常";
                return false;
            }

            return true;
        }


        /// <summary>
        /// 构建线圈的命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="addr"></param>
        /// <param name="startAddr"></param>
        /// <returns></returns>
        protected List<byte> GetWriteCoilCommand<T>(List<T> datas, byte addr, byte func, ushort startAddr)
        {
            // FC05: 写单个线圈，PDU格式 = [addr][05][startAddr Hi][startAddr Lo][FF00/0000]
            if (func == 5)
            {
                return GetWriteSingleCoilCommand<T>(datas, addr, startAddr);
            }
            // FC15: 写多个线圈
            return GetWriteMultipleCoilCommand<T>(datas, addr, func, startAddr);
        }

        /// <summary>
        /// 构建FC05写单个线圈命令
        /// </summary>
        private List<byte> GetWriteSingleCoilCommand<T>(List<T> datas, byte addr, ushort startAddr)
        {
            bool value = Convert.ToBoolean(datas[0]);
            List<byte> command = new List<byte>();
            command.Add(addr);
            command.Add(0x05);
            command.Add(BitConverter.GetBytes(startAddr)[1]);
            command.Add(BitConverter.GetBytes(startAddr)[0]);
            command.Add(value ? (byte)0xFF : (byte)0x00);
            command.Add(0x00);
            return command;
        }

        /// <summary>
        /// 构建FC15写多个线圈命令
        /// </summary>
        private List<byte> GetWriteMultipleCoilCommand<T>(List<T> datas, byte addr, byte func, ushort startAddr)
        {
            ushort dataCount = (ushort)datas.Count;
            byte byteCount = (byte)Math.Ceiling(dataCount * 1.0 / 8);

            List<byte> command = this.GetWriteCommand(addr, func, startAddr, dataCount, byteCount);

            List<int> valueTemp = new List<int>();
            List<int> intValues = new List<int>();
            for (int i = 0; i < dataCount; i++)
            {
                valueTemp.Insert(0, Convert.ToBoolean(datas[i]) ? 1 : 0);
                if (valueTemp.Count == 8)
                {
                    string strValue = string.Join("", valueTemp.Select(vt => vt.ToString()));
                    intValues.Insert(0, Convert.ToInt32(strValue, 2));
                    valueTemp.Clear();
                }
            }

            if (valueTemp.Count > 0)
            {
                string strValue = string.Join("", valueTemp.Select(iv => iv.ToString()));
                intValues.Add(Convert.ToInt32(strValue, 2));
            }

            command.AddRange(intValues.Select(iv => Convert.ToByte(iv)).ToList());
            return command;
        }

        /// <summary>
        /// 构建寄存器的命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="addr"></param>
        /// <param name="startAddr"></param>
        /// <returns></returns>
        protected List<byte> GetWriteRegisterCommand<T>(List<T> datas, byte addr, byte func, ushort startAddr)
        {
            // FC06: 写单个寄存器，PDU格式 = [addr][06][startAddr Hi][startAddr Lo][value Hi][value Lo]
            if (func == 6)
            {
                return GetWriteSingleRegisterCommand<T>(datas, addr, startAddr);
            }
            // FC16: 写多个寄存器
            return GetWriteMultipleRegisterCommand<T>(datas, addr, func, startAddr);
        }

        /// <summary>
        /// 构建FC06写单个寄存器命令
        /// </summary>
        private List<byte> GetWriteSingleRegisterCommand<T>(List<T> datas, byte addr, ushort startAddr)
        {
            List<byte> command = new List<byte>();
            command.Add(addr);
            command.Add(0x06);
            command.Add(BitConverter.GetBytes(startAddr)[1]);
            command.Add(BitConverter.GetBytes(startAddr)[0]);

            dynamic v = datas[0];
            byte[] valueByte = BitConverter.GetBytes(v);
            valueByte = this.SwitchEndian(valueByte, EndianType);
            command.AddRange(valueByte);

            return command;
        }

        /// <summary>
        /// 构建FC16写多个寄存器命令
        /// </summary>
        private List<byte> GetWriteMultipleRegisterCommand<T>(List<T> datas, byte addr, byte func, ushort startAddr)
        {
            bool isStr = typeof(T) == typeof(string);
            int len = 0;
            List<string> strDatas = new List<string>();
            // 字节数
            if (!isStr)
            {
                len = System.Runtime.InteropServices.Marshal.SizeOf<T>() * datas.Count;
            }
            else
            {
                ///将string转成Char[],计算长度
                datas.ForEach(x =>
                {
                    int strLen = x.ToString().Length;
                    int everyStrLen = (strLen % 2 == 0 ? 0 : 1) + strLen;
                    len += everyStrLen;
                });
            }

            // 寄存器数  len/2
            var command = this.GetWriteCommand(addr, func, startAddr, (byte)(len / 2), (byte)len);

            if (!isStr)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    dynamic v = datas[i];
                    byte[] valueByte = BitConverter.GetBytes(v);

                    valueByte = this.SwitchEndian(valueByte, EndianType);
                    command.AddRange(valueByte);
                }
            }
            else
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    int strLen = datas[i].ToString().Length;

                    command.AddRange(Encoding.Default.GetBytes(datas[i].ToString()));

                    // 如果字符长度不是偶数，modbus 协议需要补零
                    if (strLen % 2 != 0)
                    {
                        command.Add((byte)0);
                    }
                }
            }

            return command;
        }

        /// <summary>
        /// 读取寄存器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public abstract CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout);

        /// <summary>
        /// 将字节数组转换为hex字符串
        /// </summary>
        protected static string ToHexString(List<byte> bytes)
        {
            return BitConverter.ToString(bytes.ToArray()).Replace("-", " ");
        }

        /// <summary>
        /// 写入寄存器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="data"></param>
        /// <param name="writeMsg"></param>
        /// <returns></returns>
        public abstract CommResult Write<T>(ICommunication communication, List<T> data, string writeMsg);

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
        public abstract void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource) where T : IComparable;

    }

    public class CompareVal<T> where T : IComparable
    {
        public static bool GreaterThan(T threshVal, T curVal)
        {
            return (curVal.CompareTo(threshVal) >= 0);
        }

        public static bool Equal(T threshVal, T curVal)
        {
            return (curVal.CompareTo(threshVal) == 0);
        }

        public static bool LessEqual(T threshVal, T curVal)
        {
            return (curVal.CompareTo(threshVal) <= 0);
        }

        public static bool Compare(OpRule opRule, T threshVal, T curVal)
        {
            bool isOk = false;
            switch (opRule)
            {
                case OpRule.Equal:
                    isOk = Equal(threshVal, curVal);
                    break;
                case OpRule.GatherEqual:
                    isOk = GreaterThan(threshVal, curVal);
                    break;
                case OpRule.LessEqual:
                    isOk = LessEqual(threshVal, curVal);
                    break;
            }

            return isOk;
        }
    }

    /// <summary>
    /// Modbus TCP
    /// </summary>
    public class ModbusTcp : ModbusProtocol
    {
        /// <summary>
        /// 读寄存器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
            var byteList = BuildReadCommand<T>(communication, readMsg, out var receiveLen, out var len);
            CommResult state = new CommResult(true);
            if (!communication.IsConnected)
            {
                state = communication.Open(timeout);
            }

            if (state.IsSuccess)
            {
                var errMsg = "";
                List<byte> respBytes = new List<byte>();
                for (int i = 0; i < 5; i++)
                {
                    respBytes = communication.Send(byteList, receiveLen, 9, timeout);

                    if (respBytes.Count > 6)
                    {
                        respBytes.RemoveRange(0, 6);
                    }

                    if (respBytes.Count == 0)
                    {
                        //LogDataSing.GetInstance().GetLogData.Enqueue($"respBytes,count=0,通讯失败!");
                        Thread.Sleep(5);
                        continue;
                    }

                    if (!CheckException(respBytes, out errMsg, null))
                    {
                        string strSend = "";
                        foreach (var item in respBytes)
                        {
                            strSend += item.ToString();
                            strSend += ",";
                        }
                        //LogDataSing.GetInstance().GetLogData.Enqueue($"非法数据值为:{strSend}");
                        Thread.Sleep(5);
                        continue;

                    }
                    // 解析数据部分
                    respBytes.RemoveRange(0, 3);
                    return this.Analysis<T>(respBytes, len);
                }
                if (respBytes.Count == 0)
                {
                    return new CommResult<T> { IsSuccess = false, ErrorMsg = "通讯失败!" };
                }
                return new CommResult<T>(false, errMsg);
            }
            else
            {
                throw new FriendlyException(state.ErrorMsg);
            }
        }

        public override CommResult Write<T>(ICommunication communication, List<T> datas, string writeMsg)
        {
            if (string.IsNullOrEmpty(writeMsg))
            {
                throw new ArgumentNullException(writeMsg);
            }

            CommResult result = new CommResult(true);

            // 解析报文
            ParseMsg(writeMsg, out var addr, out var func, out var startAddr, out var count);

            try
            {
                // 是否写线圈
                bool isCoil = typeof(T) == typeof(bool);

                List<byte> commands = new List<byte>();
                if (isCoil)
                {
                    commands = GetWriteCoilCommand(datas, addr, func, startAddr);
                }
                else
                {
                    commands = GetWriteRegisterCommand<T>(datas, addr, func, startAddr);
                }

                List<byte> byteList = new List<byte>();
                // 添加MBAP
                byteList.Add(0x00);
                byteList.Add(0x00);
                byteList.Add(0x00);
                byteList.Add(0x00);
                byteList.Add((byte)(commands.Count / 256));
                byteList.Add((byte)(commands.Count % 256));
                byteList.AddRange(commands);

                result.SentHex = ToHexString(byteList);

                List<byte> respBytes = new List<byte>();
                var msg = "";
                for (int i = 0; i < 5; i++)
                {

                    result.IsSuccess = true;
                    result.ErrorMsg = "";
                    // 进行通信
                    if (communication.IsConnected)
                    {
                        respBytes = communication.Send(byteList, 12, 9);///

                        if (respBytes.Count == 0)
                        {
                            Thread.Sleep(5);
                            //LogDataSing.GetInstance().GetLogData.Enqueue($"通讯连接失败:{communication}!");
                            continue;
                        }

                        respBytes.RemoveRange(0, 6);

                        if (!CheckException(respBytes, out msg, null))
                        {
                            Thread.Sleep(5);
                            result.IsSuccess = false;
                            result.ErrorMsg = msg;
                            continue;
                        }
                    }
                    else
                    {
                        //LogDataSing.GetInstance().GetLogData.Enqueue($"通讯连接失败:{communication},请重新回零!");
                        throw new Exception($"通讯连接失败:{communication},请重新回零!");
                    }
                    break;
                }
                if (respBytes.Count == 0)
                {
                    throw new Exception($"通讯连接失败:{communication}!");
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
            }

            return result;
        }


        private List<byte> BuildReadCommand<T>(ICommunication communication, string readMsg, out int receiveLen, out ushort len)
        {
            if (string.IsNullOrEmpty(readMsg))
            {
                throw new ArgumentNullException(readMsg);
            }

            ParseMsg(readMsg, out var addr, out var func, out var startAddr, out len);

            int typeLen = 1;
            receiveLen = 0;
            Type retType = typeof(T);
            ///字符串类型需要判断奇偶
            bool isOdd = len % 2 == 1;
            bool isStr = typeof(T) == typeof(string);
            if (retType.Equals(typeof(bool)))
            {
                if (new byte[] { 1, 2 }.Contains(func))
                    func = 1;
                receiveLen = (int)Math.Ceiling(typeLen * len * 1.0 / 8) + 9;
            }
            else if (!isStr)
            {
                typeLen = System.Runtime.InteropServices.Marshal.SizeOf(retType) / 2;
                receiveLen = (len * typeLen) * 2 + 9;
            }
            else
            {
                receiveLen = ((isOdd) ? ((len + 1) / 2) : (len / 2)) * 2 + 9;
            }

            ///获取读取指令
            List<byte> reqBaseCommand = null;
            if (!isStr)
            {
                reqBaseCommand = this.GetReadCommand(addr, func, startAddr, (ushort)(len * typeLen));
            }
            else
            {
                if (!isOdd)
                {
                    reqBaseCommand = this.GetReadCommand(addr, func, startAddr, (ushort)(len / 2));
                }
                else
                {
                    reqBaseCommand = this.GetReadCommand(addr, func, startAddr, (ushort)((len + 1) / 2));
                }
            }

            // 添加MBAP
            List<byte> byteList = new List<byte>();
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.Add(0x00);
            byteList.Add(0x06);
            byteList.AddRange(reqBaseCommand);


            return byteList;
        }

        /// <summary>
        /// 读寄存器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource stopCancel)
        {
            var byteList = BuildReadCommand<T>(communication, readMsg, out var receiveLen, out var len);

            CommResult state = new CommResult(true);
            if (!communication.IsConnected)
            {
                state = communication.Open(timeout);
            }

            var isNumberic = typeof(T) == typeof(double) || typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(float);

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

                    List<byte> respBytes = new List<byte>();
                    var errMsg = "";
                    for (int i = 0; i < 5; i++)
                    {

                        respBytes = communication.Send(byteList, receiveLen, 9);
                        if (respBytes.Count == 0)
                        {
                            Thread.Sleep(5);
                            //LogDataSing.GetInstance().GetLogData.Enqueue($"ModbusProtocol通讯连接失败:{communication}!");
                            continue;
                        }
                        if (!CheckException(respBytes, out errMsg, null))
                        {
                            Thread.Sleep(5);
                            //LogDataSing.GetInstance().GetLogData.Enqueue($"校验失败,{errMsg}");
                            continue;
                        }
                        break;
                    }
                    if (respBytes.Count == 0)
                    {
                        //LogDataSing.GetInstance().GetLogData.Enqueue($"ModbusProtocol通讯连接失败:{communication}!");
                        throw new TimeoutException($"通讯连接失败:{communication}!");
                    }
                    if (string.IsNullOrEmpty(errMsg))
                    {
                        throw new FriendlyException($"校验失败,{errMsg}");
                    }

                    respBytes.RemoveRange(0, 6);



                    // 解析数据部分
                    respBytes.RemoveRange(0, 3);

                    var result = this.Analysis<T>(respBytes, len);
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
                            throw new TimeoutException($"ModBus报文 {readMsg} 超时 >= {timeout}");
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



    /// <summary>
    /// RTU 版本
    /// </summary>
    public class ModbusRTU : ModbusProtocol
    {

        private List<byte> BuildReadCommand<T>(ICommunication communication, string readMsg, out int receiveLen, out ushort len)
        {
            if (string.IsNullOrEmpty(readMsg))
            {
                throw new ArgumentNullException(readMsg);
            }

            ParseMsg(readMsg, out var addr, out var func, out var startAddr, out len);

            int typeLen = 1;
            receiveLen = 0;
            Type retType = typeof(T);
            if (retType.Equals(typeof(bool)))
            {
                if (new byte[] { 1, 2 }.Contains(func))
                    func = 1;

                // 8个寄存器一个字节
                // 向中取整：1.5 -》 2   1.4 -》 1    Ceiling（1.1-》2）
                receiveLen = (int)Math.Ceiling(typeLen * len * 1.0 / 8) + 5;
            }
            else
            {
                // 每一个数据需要多少个寄存器
                typeLen = System.Runtime.InteropServices.Marshal.SizeOf<T>() / 2;
                receiveLen = (typeLen * len) * 2 + 5;
            }

            List<byte> byteList = this.GetReadCommand(addr, func, startAddr, (ushort)(len * typeLen));

            // 添加CRC校验码
            byteList.AddRange(CRC16(byteList));

            return byteList;
        }

        /// <summary>
        /// 读寄存器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
            var byteList = BuildReadCommand<T>(communication, readMsg, out var receiveLen, out var len);
            // 连接
            var state = communication.Open(timeout);
            if (state.IsSuccess)
            {
                List<byte> respBytes = communication.Send(byteList, receiveLen, 5);

                if (!CheckException(respBytes, out var errMsg, DoCheckCRC))
                {
                    return new CommResult<T>(false, errMsg);
                }
                // 解析数据部分
                respBytes.RemoveRange(0, 3);

                return this.Analysis<T>(respBytes, len);
            }
            else
            {
                throw new Exception(state.ErrorMsg);
            }
        }

        public override void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource)
        {
            var byteList = BuildReadCommand<T>(communication, readMsg, out var receiveLen, out var len);
            // 连接
            var state = communication.Open(timeout);
            if (state.IsSuccess)
            {
                int flag = 0;
                // 持续等待
                while (true)
                {
                    // 支持外部中断
                    if (tokenSource.IsCancellationRequested)
                    {
                        OnLog(LogType.Debug, $"Wait {readMsg} {opRule} {val} 被外部停止!");
                        break;
                    }

                    // 暂停
                    if (PauseReset != null && !PauseReset.IsSet)
                    {
                        PauseReset.Wait();

                        // 如果先暂停，后停止
                        if (tokenSource.IsCancellationRequested)
                        {
                            OnLog(LogType.Debug, $"Wait {readMsg} {opRule} {val} 被外部停止!");
                            break;
                        }
                    }

                    List<byte> respBytes = communication.Send(byteList, receiveLen, 5);

                    if (!CheckException(respBytes, out var errMsg, DoCheckCRC))
                    {
                        throw new FriendlyException($"校验失败,{errMsg}");
                    }

                    // 解析数据部分
                    respBytes.RemoveRange(0, 3);

                    var result = this.Analysis<T>(respBytes, len);
                    if (result.IsSuccess && result.Datas.Count > 0)
                    {
                        // 第一个值和
                        if (CompareVal<T>.Compare(opRule, val, result.Datas[0]))
                        {
                            break;
                        }
                    }

                    Thread.Sleep(sleep);

                    if (timeout != -1)
                    {
                        flag += sleep;
                        if (flag > timeout)
                        {
                            throw new TimeoutException($"Modbus等待超时 >= {timeout}");
                        }
                    }
                }
            }
            else
            {
                throw new Exception(state.ErrorMsg);
            }
        }

        public override CommResult Write<T>(ICommunication communication, List<T> datas, string writeMsg)
        {
            if (string.IsNullOrEmpty(writeMsg))
            {
                throw new ArgumentNullException(writeMsg);
            }

            CommResult result = new CommResult(true);

            // 解析报文
            ParseMsg(writeMsg, out var addr, out var func, out var startAddr, out var count);
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // 是否写线圈
                    bool isCoil = typeof(T) == typeof(bool);

                    List<byte> commands = new List<byte>();
                    if (isCoil)
                    {
                        commands = GetWriteCoilCommand(datas, addr, func, startAddr);
                    }
                    else
                    {
                        //FFU 写入功能码必须是06
                        if (writeMsg == "211 06 261 1" || writeMsg == "211 06 260 1")
                        {
                            commands = GetWriteRegisterCommand<T>(datas, addr, 06, startAddr);
                        }
                        else if (writeMsg.Contains("17924"))
                        {
                            //清零
                            commands = new List<byte> { addr, 16, 70, 4, 0, 2, 4 };
                            byte[] bytes = new byte[] { 0, 0, 0, 0};

                            if (typeof(T) == typeof(float) && datas.Count() == 1)
                            {
                                dynamic v = datas[0];
                                bytes = BitConverter.GetBytes(v);

                                if (BitConverter.IsLittleEndian)
                                {
                                    Array.Reverse(bytes);
                                }
                            }

                            commands.AddRange(bytes);
                        }
                        else
                        {
                            commands = GetWriteRegisterCommand<T>(datas, addr, 16, startAddr);
                        }
                    }

                    // CRC校验
                    commands.AddRange(CRC16(commands));

                    result.SentHex = ToHexString(commands);

                    // 进行通信
                    if (communication.IsConnected)
                    {
                        List<byte> respBytes = new List<byte>();
                        respBytes = communication.Send(commands, 8, 5);

                        if (!CheckException(respBytes, out var msg, DoCheckCRC))
                        {
                            result.IsSuccess = false;
                            result.ErrorMsg = msg;
                        }
                        else
                        {
                            result.IsSuccess = true;
                            return result;
                        }
                    }
                    else
                    {
                        throw new Exception($"通讯连接失败:{communication},请重新回零!");
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.ErrorMsg = ex.Message;
                }
                Thread.Sleep(50);
            }

            return result;
        }
    }

    /// <summary>
    /// Ascii
    /// </summary>
    public class ModbusAscii : ModbusProtocol
    {
        #region LRC 校验
        private bool DoCheckLRC(List<byte> respBytes)
        {
            byte checkLRC = respBytes[respBytes.Count - 1];
            respBytes.RemoveRange(respBytes.Count - 1, 1);
            var calcLRC = LRC(respBytes);
            return checkLRC.Equals(calcLRC);
        }

        protected List<byte> LRC(List<byte> value)
        {
            if (value == null) return null;

            int sum = 0;
            for (int i = 0; i < value.Count; i++)
            {
                sum += value[i];
            }

            sum = sum % 256;
            sum = 256 - sum;

            value.Add((byte)sum);
            return value;
        }

        #endregion
        private List<byte> AsciiArrayToByteArray(List<byte> value)
        {
            List<string> asciiStrList = new List<string>();
            foreach (var item in value)
            {
                asciiStrList.Add(((char)item).ToString());
            }

            // 将每两个Ascii字符组成一个16进制 转换成字节  ---  对应：3
            List<byte> resultBytes = new List<byte>();
            for (int i = 0; i < asciiStrList.Count; i++)
            {
                var stringHex = asciiStrList[i].ToString() + asciiStrList[++i].ToString();
                resultBytes.Add(Convert.ToByte(stringHex, 16));
            }
            return resultBytes;
        }

        private List<byte> BuildReadCommand<T>(ICommunication communication, string readMsg, out int receiveLen, out ushort len)
        {
            if (string.IsNullOrEmpty(readMsg))
            {
                throw new ArgumentNullException(readMsg);
            }

            ParseMsg(readMsg, out var addr, out var func, out var startAddr, out len);

            int typeLen = 1;
            receiveLen = 0;
            Type retType = typeof(T);
            if (retType.Equals(typeof(bool)))
            {
                if (new byte[] { 1, 2 }.Contains(func))
                    func = 1;

                // 8个寄存器一个字节
                // 向中取整：1.5 -》 2   1.4 -》 1    Ceiling（1.1-》2）
                receiveLen = (int)(Math.Ceiling(typeLen * len * 1.0 / 8) + 4) * 2 + 3;
            }
            else
            {
                // 每一个数据需要多少个寄存器
                typeLen = System.Runtime.InteropServices.Marshal.SizeOf<T>() / 2;
                // （寄存器数量*请求数*2  + 从站地址 + 功能码 + 返回字节数 + LRC） * 2 + 冒号 + 换行 + 回车
                receiveLen = (len * typeLen * 2 + 4) * 2 + 3;
            }

            List<byte> byteList = this.GetReadCommand(addr, func, startAddr, (ushort)(len * typeLen));

            // 添加LRC校验码
            byteList.AddRange(LRC(byteList));

            // 需要进行Ascii码的转换
            // 转换成Ascii码字符16进制
            var hex = byteList.Select(b => b.ToString("X2"));

            // 转换成一个完整的字符串
            string hexStr = string.Join("", hex.ToArray());
            // 转换成AsciiList
            List<byte> asciiList = new List<byte>(Encoding.ASCII.GetBytes(hexStr));

            // 添加报头和报尾
            asciiList.Insert(0, 0x3A);
            asciiList.Add(13);
            asciiList.Add(10);

            return asciiList;
        }

        /// <summary>
        /// 读寄存器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communication"></param>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override CommResult<T> Read<T>(ICommunication communication, string readMsg, int timeout)
        {
            var byteList = BuildReadCommand<T>(communication, readMsg, out var receiveLen, out var len);

            // 连接
            var state = communication.Open();
            if (state.IsSuccess)
            {
                // 为什么是加11
                // 01 03 00 00 00 01 XX
                // 01 03 02 00 01 XX
                List<byte> respBytes = communication.Send(byteList, receiveLen, 11); ///

                // 解析 Ascii报文 -》 byte集合
                // 去头掐尾
                respBytes.RemoveAt(0);
                respBytes.RemoveRange(respBytes.Count - 2, 2);

                // 将Ascii码转换成ByteList
                respBytes = AsciiArrayToByteArray(respBytes);

                if (!CheckException(respBytes, out var errMsg, DoCheckLRC))
                {
                    return new CommResult<T>(false, errMsg);
                }

                // 解析数据部分
                respBytes.RemoveRange(0, 3);

                return this.Analysis<T>(respBytes, len);
            }
            else
            {
                throw new Exception(state.ErrorMsg);
            }
        }

        public override CommResult Write<T>(ICommunication communication, List<T> datas, string writeMsg)
        {
            if (string.IsNullOrEmpty(writeMsg))
            {
                throw new ArgumentNullException(writeMsg);
            }

            CommResult result = new CommResult(true);

            // 解析报文
            ParseMsg(writeMsg, out var addr, out var func, out var startAddr, out var count);

            try
            {
                // 是否写线圈
                bool isCoil = typeof(T) == typeof(bool);

                List<byte> commands = new List<byte>();
                if (isCoil)
                {
                    commands = GetWriteCoilCommand(datas, addr, func, startAddr);
                }
                else
                {
                    commands = GetWriteRegisterCommand<T>(datas, addr, func, startAddr);
                }

                // CRC校验
                commands.AddRange(LRC(commands));

                result.SentHex = ToHexString(commands);

                // 需要进行Ascii码的转换
                // 转换成Ascii码字符16进制
                var hex = commands.Select(b => b.ToString("X2"));
                // 转换成一个完整的字符串
                string hexStr = string.Join("", hex.ToArray());
                // 转换成AsciiList
                List<byte> asciiList = new List<byte>(Encoding.ASCII.GetBytes(hexStr));

                // 添加报头和报尾
                asciiList.Insert(0, 0x3A);
                asciiList.Add(13);
                asciiList.Add(10);

                // 进行通信
                var state = communication.Open();
                if (state.IsSuccess)
                {
                    // 此处len参数究竟怎么传？？？？？
                    List<byte> respBytes = communication.Send(commands, 17, 11);///

                    // 解析 Ascii报文 -》 byte集合
                    // 去头掐尾
                    respBytes.RemoveAt(0);
                    respBytes.RemoveRange(respBytes.Count - 2, 2);

                    // 将Ascii码转换成ByteList
                    respBytes = AsciiArrayToByteArray(respBytes);

                    if (!CheckException(respBytes, out var msg, DoCheckLRC))
                    {
                        result.IsSuccess = false;
                        result.ErrorMsg = msg;
                    }
                }
                else
                {
                    throw new Exception(state.ErrorMsg);
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMsg = ex.Message;
            }

            return result;
        }

        public override void Wait<T>(ICommunication communication, string readMsg, T val, OpRule opRule, int timeout, int sleep, CancellationTokenSource tokenSource)
        {
            var byteList = BuildReadCommand<T>(communication, readMsg, out var receiveLen, out var len);

            // 连接
            var state = communication.Open();
            if (state.IsSuccess)
            {
                int flag = 0;

                // 持续等待
                while (true)
                {
                    // 支持外部中断
                    if (tokenSource.IsCancellationRequested) break;

                    // 暂停
                    if (PauseReset != null && !PauseReset.IsSet)
                    {
                        PauseReset.Wait();
                    }

                    // 为什么是加11
                    // 01 03 00 00 00 01 XX
                    // 01 03 02 00 01 XX
                    List<byte> respBytes = communication.Send(byteList, receiveLen, 11); ///

                    // 解析 Ascii报文 -》 byte集合
                    // 去头掐尾
                    respBytes.RemoveAt(0);
                    respBytes.RemoveRange(respBytes.Count - 2, 2);

                    // 将Ascii码转换成ByteList
                    respBytes = AsciiArrayToByteArray(respBytes);

                    if (!CheckException(respBytes, out var errMsg, DoCheckLRC))
                    {
                        throw new FriendlyException($"校验失败,{errMsg}");
                    }

                    // 解析数据部分
                    respBytes.RemoveRange(0, 3);

                    var result = this.Analysis<T>(respBytes, len);
                    if (result.IsSuccess && result.Datas.Count > 0)
                    {
                        // 第一个值和
                        if (CompareVal<T>.Compare(opRule, val, result.Datas[0]))
                        {
                            break;
                        }
                    }

                    Thread.Sleep(sleep);

                    if (timeout != -1)
                    {
                        flag += sleep;
                        if (flag > timeout)
                        {
                            throw new TimeoutException($"Modbus等待超时 >= {timeout}");
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