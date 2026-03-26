#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VPlcAddr
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.VDevice
* 文 件 名:       VPlcAddr.cs
* 创建时间:       2022/9/1 10:52:04
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b08a12f2-1b2a-4b1c-8619-73b784b3c0b6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/1 10:52:04
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.VDevice
{
    /// <summary>
    /// Plc 地址
    /// </summary>
    public class VPlc : VirtualDeviceBase, IHome, IStop, IMonitor, IDeviceError
    {
        public override int Sort => 8;

        /// <summary>
        /// Socket 通信
        /// </summary>
        public ICommunication PlcComm { get; set; }

        /// <summary>
        /// 地址信息
        /// </summary>
        [Ignore]
        public List<PlcAddr> Address { get; set; }

        /// <summary>
        /// 协议
        /// </summary>
        public ProtocolType ProtocolType { get; set; }

        /// <summary>
        /// 字节顺序
        /// </summary>
        public EndianType EndianType { get; set; }

        /// <summary>
        /// 协议
        /// </summary>
        [Ignore]
        public IProtocol Protocol { get; set; }

        /// <summary>
        /// 刷新频率
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 默认启用监控
        /// </summary>
        public bool IsMonitor { get; set; } = true;

        /// <summary>
        /// 错误代码
        /// </summary>
        public override DeviceError[] ErrorCodes => new DeviceError[] { DeviceError.ConnectTimeFail };

        /// <summary>
        /// Plc控制
        /// </summary>
        public VPlc()
        {
            Frequency = 50;
            Address = new List<PlcAddr>();
            ProtocolType = ProtocolType.ModbusTCP;
        }

        /// <summary>
        /// 打开端口
        /// </summary>
        public void Open()
        {
            if (PlcComm == null)
            {
                throw new FriendlyException("Plc通讯地址配置错误!");
            }

            // 服务器连接
            if (!PlcComm.IsConnected)
            {
                PlcComm.Open();
            }
            CurrentErrorCode = DeviceError.ConnectTimeFail;
            // 配置协议
            SetProtocol(ProtocolType);
        }

        public void Close()
        {
            PlcComm?.Close();
        }

        /// <summary>
        /// 对不同类型数据进行分组
        /// </summary>
        private List<DataTypeAddress> groupDict = new List<DataTypeAddress>();

        /// <summary>
        /// 数据类型分组
        /// </summary>
        private void DataTypeGroup()
        {
            groupDict?.Clear();

            List<PlcAddr> plcAddr = new List<PlcAddr>();

            var dGroupType = Address.GroupBy(u => u.DataType);

            int regionCount = 50;
            foreach (var item in dGroupType)
            {
                var list = Address.Where(u => u.DataType == item.Key && u.AddrType == PlcAddrType.Read)
                                    .OrderBy(u => u.Addr)
                                    .ToList();
                if (list.Count == 0) continue;

                int count = list.Count;
                // 如果地址超出了100个区间，则需要重新开一个类型
                var max = list.Max(u => u.Addr);
                var min = list.Min(u => u.Addr);
                if ((max - min) > regionCount)
                {
                    // 分割地址区间
                    int startAddr = list[0].Addr;
                    for (int i = 0; i < count; i++)
                    {
                        if (list[i].Addr - startAddr > regionCount)
                        {
                            var tempAddrs = list.Where(u => u.Addr >= startAddr && u.Addr < list[i].Addr).ToList();
                            BuildAddrRegion(tempAddrs, item.Key);
                            startAddr = list[i].Addr;
                        }

                        // 如果是最后区间
                        if (i == list.Count - 1)
                        {
                            var tempAddrs = list.Where(u => u.Addr >= startAddr && u.Addr <= list[i].Addr).ToList();
                            BuildAddrRegion(tempAddrs, item.Key);
                        }
                    }
                }
                else
                {
                    BuildAddrRegion(list, item.Key);
                }
            }
        }

        private void BuildAddrRegion(List<PlcAddr> list, DataType dType)
        {
            int len = int.Parse(list[list.Count - 1].Address) - int.Parse(list[0].Address) + 1;
            if (dType == DataType.Int || dType == DataType.Float)
            {
                len = len / 2 + (len % 2 == 0 ? 0 : 1);
            }
            if (dType == DataType.Double)
            {
                len = len / 4 + (len % 4 == 0 ? 0 : 1);
            }

            groupDict.Add(new DataTypeAddress()
            {
                StartAddr = int.Parse(list[0].Address),
                Count = len,
                DataType = dType,
                Addrs = list
            });
        }

        private void DataTypeGroup1()
        {
            groupDict?.Clear();

            List<PlcAddr> plcAddr = new List<PlcAddr>();

            var dGroupType = Address.GroupBy(u => u.DataType);

            foreach (var item in dGroupType)
            {
                var list = Address.Where(u => u.DataType == item.Key && u.AddrType == PlcAddrType.Read)
                                    .OrderBy(u => u.Address)
                                    .ToList();

                int startIndex = 0;
                list.ForEach(u => u.Index = startIndex++);

                var count = list.Count;
                int start = 0;
                for (int i = 0; i < count; i++)
                {
                    // 最后一条
                    if (i == count - 1)
                    {
                        Build(item.Key, start, i, list, null, true);
                    }
                    else
                    {
                        Build(item.Key, start, i, list, result =>
                        {
                            if (result)
                            {
                                start = i;
                            }
                        });
                    }
                }
            }
        }

        private void Build(DataType dType, int start, int curIndex, List<PlcAddr> addrs, Action<bool> func = null, bool isEnd = false)
        {
            int diff = 0;
            int len = 1;
            if (curIndex > 0)
            {
                diff = addrs[curIndex].Addr - addrs[curIndex - 1].Addr;
                len = addrs[curIndex].Addr - addrs[start].Addr + 1;
            }

            var list = addrs.Where(u => u.Index >= start && u.Index <= curIndex).ToList();
            bool isBreak = false;

            var addr = new DataTypeAddress()
            {
                StartAddr = addrs[start].Addr,
                Count = len,
                DataType = dType,
                Addrs = list
            };

            switch (dType)
            {
                case DataType.Bool:
                case DataType.Short:

                    if (diff > 1)
                    {
                        list.RemoveAt(list.Count - 1);
                        addr.Count = list.Count;
                        groupDict.Add(addr);
                        isBreak = true;
                    }

                    break;

                case DataType.Int:
                case DataType.Float:

                    addr.Count = len / 2 + (len % 2 == 0 ? 0 : 1);
                    if (diff > 2)
                    {
                        list.RemoveAt(list.Count - 1);
                        addr.Count = list.Count / 2 + (list.Count % 2 == 0 ? 0 : 1);
                        groupDict.Add(addr);
                        isBreak = true;
                    }

                    break;
                case DataType.Double:

                    // 重新配置
                    addr.Count = len / 4 + (len % 4 == 0 ? 0 : 1);
                    if (diff > 4)
                    {
                        list.RemoveAt(list.Count - 1);
                        addr.Count = list.Count / 4 + (list.Count % 4 == 0 ? 0 : 1);
                        groupDict.Add(addr);
                        isBreak = true;
                    }

                    break;
            }

            if (!isBreak && isEnd)
            {
                groupDict.Add(addr);
                IsBreak = true;
            }

            func?.Invoke(isBreak);
        }

        /// <summary>
        /// 启用监控
        /// </summary>
        public void StartMonitor()
        {
            // 只对只读地址进行监控
            // 批量读取前，需要连打开PLC（如果已经连上，不再打开）
            Open();
            ReadBatch(1, PlcAddrType.Read);
        }

        /// <summary>
        /// 监控间隔
        /// </summary>
        public int MonitorSleep => 50;


        /// <summary>
        /// 对所有地址读取一次地址
        /// </summary>
        /// <summary>
        /// 对所有地址读取一次地址
        /// </summary>
        public void ReadBatch(int client = 1, PlcAddrType pType = PlcAddrType.None, int timeout = 1000)
        {
            if (groupDict == null)
            {
                DataTypeGroup();
            }

            // 创建副本，防止线程冲突
            var snapshot = groupDict.ToList();

            foreach (var item in snapshot)
            {
                try
                {
                    switch (item.DataType)
                    {
                        case DataType.Bool:
                            {
                                string readMsg = $"{client.ToString().PadLeft(2, '0')} 01 {item.StartAddr} {item.Count}";
                                CommResult<bool> commResult = Protocol.Read<bool>(PlcComm, readMsg, timeout);
                                if (commResult.Datas.Count != item.Count)
                                {
                                    OnLog(LogType.Debug, $"PLC {readMsg} 读取数量:{commResult.Datas.Count}!=获取数量:{item.Count}");
                                    continue;
                                }

                                int addr = 0;
                                foreach (var pItem in item.Addrs)
                                {
                                    if (int.TryParse(pItem.Address, out addr))
                                    {
                                        int index = addr - item.StartAddr;
                                        pItem.Value = commResult.Datas[index] ? 1 : 0;
                                    }
                                }
                            }
                            break;
                        case DataType.Short:
                            {
                                string readMsg = $"{client} 03 {item.StartAddr} {item.Count}";
                                CommResult<short> commResult = Protocol.Read<short>(PlcComm, readMsg, timeout);
                                if (commResult.Datas.Count != item.Count)
                                {
                                    OnLog(LogType.Debug, $"PLC {readMsg} 读取数量:{commResult.Datas.Count}!=获取数量:{item.Count}");
                                    continue;
                                }

                                int addr = 0;
                                foreach (var pItem in item.Addrs)
                                {
                                    if (int.TryParse(pItem.Address, out addr))
                                    {
                                        int index = addr - item.StartAddr;
                                        pItem.Value = commResult.Datas[index];
                                    }
                                }
                            }
                            break;
                        case DataType.Int:
                            {
                                string readMsg = $"{client} 03 {item.StartAddr} {item.Count}";
                                CommResult<int> commResult = Protocol.Read<int>(PlcComm, readMsg, timeout);
                                if (commResult.Datas.Count != item.Count)
                                {
                                    OnLog(LogType.Debug, $"PLC {readMsg} 读取数量:{commResult.Datas.Count}!=获取数量:{item.Count}");
                                    continue;
                                }
                                int addr = 0;
                                foreach (var pItem in item.Addrs)
                                {
                                    if (int.TryParse(pItem.Address, out addr))
                                    {
                                        // 因为int类型是双地址位置，需要
                                        int index = (addr - item.StartAddr) / 2;
                                        pItem.Value = commResult.Datas[index];
                                    }
                                }
                            }
                            break;
                        case DataType.Float:
                            {
                                string readMsg = $"{client} 03 {item.StartAddr} {item.Count}";
                                CommResult<float> commResult = Protocol.Read<float>(PlcComm, readMsg, timeout);
                                if (commResult.Datas.Count != item.Count)
                                {
                                    OnLog(LogType.Debug, $"PLC {readMsg} 读取数量:{commResult.Datas.Count}!=获取数量:{item.Count}");
                                    continue;
                                }
                                int addr = 0;
                                foreach (var pItem in item.Addrs)
                                {
                                    if (int.TryParse(pItem.Address, out addr))
                                    {
                                        int index = (addr - item.StartAddr) / 2;
                                        pItem.Value = commResult.Datas[index];
                                    }
                                }
                            }
                            break;
                        case DataType.Double:
                            {
                                string readMsg = $"{client} 03 {item.StartAddr} {item.Count}";
                                CommResult<double> commResult = Protocol.Read<double>(PlcComm, readMsg, timeout);
                                if (commResult.Datas.Count != item.Count)
                                {
                                    OnLog(LogType.Debug, $"PLC {readMsg} 读取数量:{commResult.Datas.Count}!=获取数量:{item.Count}");
                                    continue;
                                }
                                int addr = 0;
                                foreach (var pItem in item.Addrs)
                                {
                                    if (int.TryParse(pItem.Address, out addr))
                                    {
                                        int index = (addr - item.StartAddr) / 4;
                                        pItem.Value = commResult.Datas[index];
                                    }
                                }
                            }

                            break;
                        default:
                            throw new FriendlyException($"类型:{item.DataType} 不支持!");
                    }
                }
                catch (Exception)
                {
                    OnLog(LogType.Debug, $"PLC {item.Addrs.ToList().ToString()} 读取异常");
                }
                //Thread.Sleep(MonitorSleep);
            }
        }

        /// <summary>
        /// 等待
        /// </summary>
        /// <param name="plcAddr"></param>
        public void Wait(string addr, OpRule opRule, double val, int timeout = -1)
        {
            var plcAddr = Address.FirstOrDefault(p => p.Address == addr);
            if (plcAddr == null)
            {
                throw new FriendlyException($"Plc:{Name} 不存在地址:{addr}");
            }

            VStatus = VStatus.Running;

            CalcTime(() =>
            {
                bool isTrue = CompareVal<double>.Compare(opRule, val, plcAddr.Value);
                return isTrue;
            }, timeout, 20, () =>
            {
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID);
            });

            VStatus = VStatus.Idle;
        }


        /// <summary>
        /// 写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        public void Write<T>(string addr, T val, string writeMsg)
        {
            var plcAddr = Address.FirstOrDefault(p => p.Address == addr);
            if (plcAddr != null)
            {
                if (double.TryParse(val?.ToString(), out var v))
                {
                    plcAddr.Value = v;
                }
            }

            // 更新Plc对应的地址值
            var commResult = Protocol.Write<T>(PlcComm, new List<T>() { val }, writeMsg);
            if (!commResult.IsSuccess)
            {
                PlcComm.Close();
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, commResult.ErrorMsg);
            }
        }

        /// <summary>
        /// 地址检测
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="action"></param>
        /// <exception cref="FriendlyException"></exception>
        private void CheckAddr(string addr, Action<PlcAddr> action)
        {
            var plcAddr = Address.FirstOrDefault(p => p.Address == addr);
            if (plcAddr == null)
            {
                throw new FriendlyException($"Plc:{Name} 不存在地址:{addr}");
            }

            action(plcAddr);
        }

        public bool ReadBool(string addr)
        {
            bool isTrue = false;
            CheckAddr(addr, addr =>
            {
                isTrue = addr.Value == 1;
            });

            return isTrue;
        }

        public List<bool> ReadBatchAlarm(int startAddr, int count)
        {
            string readMsg = $"01 01 {startAddr} {count}";
            // 更新Plc对应的地址值
            var commResult = Protocol.Read<bool>(PlcComm, readMsg, 1000);
            if (!commResult.IsSuccess)
            {
                PlcComm.Close();
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, commResult.ErrorMsg);
            }

            return commResult.Datas;
        }

        public double ReadNumber(string addr)
        {
            double value = 0;
            CheckAddr(addr, addr =>
            {
                value = addr.Value;
            });

            return value;
        }

        /// <summary>
        /// 配置协议
        /// </summary>
        /// <param name="protocol"></param>
        public void SetProtocol(ProtocolType protocol)
        {
            // 如果相等
            if (ProtocolType == protocol && Protocol != null)
            {
                return;
            }

            IProtocol retProtocol = null;
            switch (protocol)
            {
                case Enums.ProtocolType.StringDefault:
                case Enums.ProtocolType.StringUtf8:
                case Enums.ProtocolType.StringHex:
                    retProtocol = new StringProtocol();
                    break;
                case Enums.ProtocolType.ModbusRTU:
                    retProtocol = new ModbusRTU();
                    break;
                case Enums.ProtocolType.ModbusASCII:
                    retProtocol = new ModbusAscii();
                    break;
                case Enums.ProtocolType.ModbusTCP:
                    retProtocol = new ModbusTcp();
                    break;
            }

            ProtocolType = protocol;

            retProtocol.LogEvent -= OnLog;
            retProtocol.LogEvent += OnLog;

            // 配置字节顺序
            if (retProtocol != null && retProtocol is IModbus m)
            {
                m.EndianType = EndianType;
            }

            // 真实协议
            Protocol = retProtocol;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public override void Stop()
        {
            base.Stop();
        }

        /// <summary>
        /// Xml导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            var xml = base.ExportXml();

            var addrs = Address.OrderBy(u => u.DataType).ThenBy(u => u.Address).ToList();

            // 生成地址信息
            foreach (var item in addrs)
            {
                xml.Add(item.ExportXml());
            }

            return xml;
        }

        /// <summary>
        /// Xml解析
        /// </summary>
        /// <param name="xElement"></param>
        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);
            Address = new List<PlcAddr>();
            foreach (var element in xElement.Elements("PlcAddr"))
            {
                PlcAddr address = new PlcAddr();
                address.ParserXml(element);
                Address.Add(address);
            }

            SetProtocol(ProtocolType);

            UpdateAddr();
        }

        /// <summary>
        /// 如果地址发生了变更，需要
        /// </summary>
        public void UpdateAddr()
        {
            groupDict?.Clear();
            DataTypeGroup();
        }

        public List<string> GetAddress(DataType dataType, PlcAddrType plcAddType = PlcAddrType.None)
        {
            var list = Address.Where(u => u.DataType == dataType);
            if (plcAddType != PlcAddrType.None)
            {
                list = list.Where(u => u.AddrType == plcAddType);
            }

            return list.Select(u => u.Address).ToList();
        }

        public override string ToString()
        {
            return $"类型:{nameof(VPlc)} {Name}";
        }
    }

    public class PlcAddr : IXMLParser
    {
        /// <summary>
        /// Plc地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// 是只读还是只写
        /// </summary>
        public PlcAddrType AddrType { get; set; } = PlcAddrType.Read;

        [Ignore]
        public int Index { get; set; } = 0;

        /// <summary>
        /// 转换成地址
        /// </summary>
        [Ignore]
        public int Addr => int.Parse(Address);

        /// <summary>
        /// 值
        /// </summary>
        [Ignore]
        public double Value { get; set; }

        /// <summary>
        /// 地址描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 对象导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml();
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }
    }

    /// <summary>
    /// 基于数据类型管理地址
    /// </summary>
    class DataTypeAddress
    {
        public DataType DataType { get; set; }

        /// <summary>
        /// 起始地址
        /// </summary>
        public int StartAddr { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 类型空间
        /// </summary>
        public int DataSize { get; set; }

        public List<PlcAddr> Addrs { get; set; }
    }
}