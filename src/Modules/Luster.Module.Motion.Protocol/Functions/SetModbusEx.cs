using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.DataStruct.Network;
using System.Threading;
using System.Data;

namespace Luster.Module.Motion.Protocol.Functions
{
    /// <summary>
    /// Modbus写入功能码
    /// </summary>
    public enum ModbusWriteFuncCode
    {
        [Description("05 写单个线圈")]
        WriteSingleCoil = 5,
        [Description("06 写单个寄存器")]
        WriteSingleRegister = 6,
        [Description("15 写多个线圈")]
        WriteMultipleCoils = 15,
        [Description("16 写多个寄存器")]
        WriteMultipleRegisters = 16
    }

    /// <summary>
    /// 通用设置值(Modbus)
    /// </summary>
    public class SetModbusEx : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("协议类型", 1, CN = "协议类型", DefaultV = ProtocolType.ModbusTCP)]
        public ProtocolType ProtocolType { get; set; }

        [Parameter("设备地址", 2, CN = "设备地址", DefaultV = 1)]
        public int Address { get; set; }

        [Parameter("功能码", 3, CN = "功能码", DefaultV = ModbusWriteFuncCode.WriteSingleRegister)]
        public ModbusWriteFuncCode FuncCode { get; set; }

        [DependOn("FuncCode", ModbusWriteFuncCode.WriteSingleRegister, ModbusWriteFuncCode.WriteMultipleRegisters)]
        [Parameter("数据类型", 4, CN = "数据类型")]
        public DataType DataType { get; set; }

        [DependOn("FuncCode", ModbusWriteFuncCode.WriteSingleRegister, ModbusWriteFuncCode.WriteMultipleRegisters)]
        [DependOn("DataType", DataType.Int, DataType.Float, DataType.Double)]
        [Parameter("字节顺序", 5, CN = "字节顺序")]
        public EndianType EndianType { get; set; }

        [Parameter("首地址,十进制值", 6, CN = "首地址")]
        public int StartAddress { get; set; }

        [DependOn("FuncCode", ModbusWriteFuncCode.WriteSingleCoil, ModbusWriteFuncCode.WriteMultipleCoils)]
        [Parameter("线圈值(1=ON/0=OFF,支持0x十六进制)", 7, CN = "线圈值", DefaultV = "1")]
        public string CoilValue { get; set; }

        [DependOn("DataType", DataType.Bool)]
        [Parameter("写入布尔值", 7, CN = "布尔值")]
        public bool BoolVal { get; set; }

        [DependOn("DataType", DataType.Short, DataType.Int)]
        [Parameter("写入整型值", 8, CN = "整型值", DefaultV = 0, CanRef = ParamRef.Ref)]
        public int IntVal { get; set; }

        [DependOn("DataType", DataType.Double)]
        [Parameter("写入双精度值", 9, CN = "双精度值", DefaultV = 0, CanRef = ParamRef.Ref)]
        public double DoubleVal { get; set; }

        [DependOn("DataType", DataType.String)]
        [Parameter("写入字符串", 10, CN = "字符串", DefaultV = "", CanRef = ParamRef.Ref)]
        public string StringVal { get; set; }

        public SetModbusEx()
        {
            this.Tips = "通用设置线圈或者寄存器上的值";
            this.Icon = "\xe692";
        }

        internal static object lockPlc = new object();

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCommuncation>(CommDevice, out var communcation);

            communcation.Open();
            // 配置协议
            communcation.SetProtocol(ProtocolType);

            // 配置协议
            if (communcation.Protocol is IModbus m)
            {
                m.EndianType = EndianType;
            }

            // 根据数据类型计算需要写入的寄存器长度
            int length = 1;
            if (DataType == DataType.Int) length = 2;
            else if (DataType == DataType.Double) length = 4;

            // 格式化功能码为两位的10进制字符串如 "05", "06", "15", "16" 
            string funcCodeStr = ((int)FuncCode).ToString("D2");
            // 如果长度大于1但使用的是单寄存器写入码(06,05)，自动修正为多寄存器/多线圈写入码(16,15)
            if (length > 1)
            {
                if (funcCodeStr == "06") funcCodeStr = "16";
                else if (funcCodeStr == "05") funcCodeStr = "15";
            }

            lock (lockPlc)
            {
                // FC05/FC15 线圈写入：绕过 DataType，直接解析线圈值
                if (FuncCode == ModbusWriteFuncCode.WriteSingleCoil || FuncCode == ModbusWriteFuncCode.WriteMultipleCoils)
                {
                    bool coilBool = ParseCoilValue(CoilValue);
                    communcation.Write<bool>(coilBool, $"{Address} {funcCodeStr} {StartAddress} {length}");
                    Retry<bool>(communcation, coilBool, funcCodeStr);
                }
                else if (DataType == DataType.Bool)
                {
                    communcation.Write<bool>(BoolVal, $"{Address} {funcCodeStr} {StartAddress} {length}");
                    Retry<bool>(communcation, BoolVal, funcCodeStr);
                }
                else if (DataType == DataType.Int)
                {
                    communcation.Write<int>(IntVal, $"{Address} {funcCodeStr} {StartAddress} {length}");
                    Retry<int>(communcation, IntVal, funcCodeStr);
                }
                else if (DataType == DataType.Short)
                {
                    communcation.Write<short>((short)IntVal, $"{Address} {funcCodeStr} {StartAddress} {length}");
                    Retry<short>(communcation, (short)IntVal, funcCodeStr);
                }
                else if (DataType == DataType.Double)
                {
                    communcation.Write<double>(DoubleVal, $"{Address} {funcCodeStr} {StartAddress} {length}");
                    Retry<double>(communcation, DoubleVal, funcCodeStr);
                }
                else if (DataType == DataType.String)
                {
                    if (!string.IsNullOrEmpty(StringVal))
                    {
                        List<char> charArray = StringVal.ToCharArray().ToList();
                        ///奇数长度补齐一位
                        bool IsOdd = charArray.Count % 2 != 0;
                        List<int> Datas = new List<int>();
                        charArray.ForEach(x => { Datas.Add((int)x); });
                        ///确保长度都是偶数
                        if (IsOdd)
                        {
                            Datas.Add(0);
                        }
                        for (int i = 0; i < Datas.Count; i += 2)
                        {
                            short data = (short)(Datas[i] * 256 + Datas[i + 1]);
                            int realAddress = StartAddress + i / 2;
                            communcation.Write<short>(data, $"{Address} {funcCodeStr} {realAddress} 1");
                        }
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 重试，确认写入Modbus成功
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communcation"></param>
        /// <param name="val"></param>
        /// <param name="writeFuncStr">写入功能码，用于推断读取功能码</param>
        private void Retry<T>(VCommuncation communcation, T val, string writeFuncStr) where T : IComparable
        {
            // 根据是否是线圈推断读取功能码(01读取线圈状态, 03读取保持寄存器状态)
            string readFuncCode = "03";
            if (typeof(T) == typeof(bool) || writeFuncStr == "05" || writeFuncStr == "15")
            {
                readFuncCode = "01";
            }

            // 根据数据类型计算需要读取的寄存器长度以做校验
            int length = 1;
            if (typeof(T) == typeof(int)) length = 2;
            else if (typeof(T) == typeof(double)) length = 4;

            string readMsg = $"{Address} {readFuncCode} {StartAddress} {length}";
            bool writeSuccess = false;
            for (int i = 0; i < 1; i++)
            {
                // 延时20ms后,再次读取确认
                SpinWait.SpinUntil(() => false, 20);
                var resultVals = communcation.Read<T>(readMsg);
                if (resultVals != null && resultVals.Count > 0)
                {
                    if (CompareVal<T>.Compare(OpRule.Equal, val, resultVals[0]))
                    {
                        writeSuccess = true;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }

            // 如果查询一次后，结果没有就记录下
            if (!writeSuccess)
            {
                MyOwner.OnLog(LogType.Warning, $"模块:{MyOwner.Alias} 写入Modbus 失败!");
            }
        }

        /// <summary>
        /// 解析线圈值，支持十进制(1/0)和十六进制(0xFF00/0x0000)格式
        /// </summary>
        private static bool ParseCoilValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
            {
                int hexVal = Convert.ToInt32(value, 16);
                return hexVal != 0;
            }

            // 十进制解析
            if (int.TryParse(value, out int decVal))
                return decVal != 0;

            // 兜底：非空非零字符串视为 ON
            return true;
        }
    }
}
