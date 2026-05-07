#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VSocket
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.VDevice
* 文 件 名:       VSocket.cs
* 创建时间:       2022/7/5 18:38:26
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b25f3a7e-a04c-47cc-9fe6-3072b935ccb7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/5 18:38:26
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.VDevice
{
    /// <summary>
    /// 通讯
    /// </summary>
    public class VCommuncation : VirtualDeviceBase, IHome, IStop, IHeartbeat, IDisposable, ICloneable, IDeviceShield, IDeviceError, IAutoCheck, IParam,IMaintain
    {
        public override int Sort => 7;

        /// <summary>
        /// Socket 通信
        /// </summary>
        public ICommunication Communication { get; set; }

        /// <summary>
        /// 协议种类
        /// </summary>
        public ProtocolType ProtocolType { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public BrandType Brand { get; set; }

        /// <summary>
        /// 协议
        /// </summary>
        [Ignore]
        public IProtocol Protocol { get; set; }

        public override string Unit => "Times";

        /// <summary>
        /// 命令集合
        /// </summary>
        public LArray<SocketAction> Actions { get; set; }

        /// <summary>
        /// 用户参数 方便传参使用
        /// </summary>
        public List<string> UserParam { get; set; }

        /// <summary>
        /// 停止对象
        /// </summary>
        private ConcurrentDictionary<Guid, CancellationTokenSource> stopResetDict;

        public override DeviceError[] ErrorCodes => new DeviceError[]
        {
            DeviceError.ConnectTimeFail,
        };


        public int MinValue { get; set; } = 5;
        public int MaxValue { get; set; } = 300;
        public int TargetValue { get; set; } = 50;



        List<byte> finsHandShakingSend = new List<byte>();
        List<byte> finsHandShakingReceive = new List<byte>();
        /// <summary>
        /// Fins连接头部报文
        /// </summary>
        string finsHandShakingSendStr = "46494E530000000C0000000000000000000000C9";
        string finsHandShakingReceiveStr = "46494E530000001000000000000000000000000100000001";



        /// <summary>
        /// 防止同时连接
        /// </summary>
        private /*static*/ object lockObj = new object();

        public event Func<string, bool, bool> OnCheckEvent;

        public /*static*/ bool IsSendFinsHandShak;

        /// <summary>
        /// 停止会中断通讯网络
        /// </summary>
        //public override bool IsBreak
        //{
        //    get => base.IsBreak; set
        //    {
        //        base.IsBreak = value;
        //        if (value)
        //            Stop();
        //    }
        //}

        /// <summary>
        /// 构造函数
        /// </summary>
        public VCommuncation() : base()
        {
            NeedHome = true;
            Actions = new LArray<SocketAction>();
            stopResetDict = new ConcurrentDictionary<Guid, CancellationTokenSource>();
            CurrentErrorCode = DeviceError.ConnectTimeFail;
            finsHandShakingSend.AddRange(ConvertHexStringToBytes(finsHandShakingSendStr));
        }

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="timeout"></param>
        public void Open(int timeout = 5000, byte[] bytes = null)
        {
            if (GetIsShield()) return;

            // 防止多线程同时连接
            lock (lockObj)
            {
                try
                {
                    CommResult res = Communication.Open(timeout);

                    //如果是Fins，打开端口后，需要发送握手指令
                    if (ProtocolType == ProtocolType.FinsTCP && !IsSendFinsHandShak)
                    {
                        Communication.Send(finsHandShakingSend, 24);
                        IsSendFinsHandShak = true;
                    }

                    if (!res.IsSuccess)
                    {
                        // 超时后关闭通讯
                        Communication.Close();
                        throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, res.ErrorMsg);
                    }
                }
                catch (TimeoutException timeEx)
                {
                    throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, timeEx.Message);
                }

                Communication.LogEvent -= OnLog;
                Communication.LogEvent += OnLog;
            }
        }


        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            Communication.Close();
            IsSendFinsHandShak = false;
        }

        public void ClearCache()
        {
            Communication.ClearCache();
        }

        /// <summary>
        /// 通讯释放
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (Communication != null && Communication.IsConnected)
            {
                Communication.Close();
            }
        }
        /// <summary>
        /// 读取值
        /// </summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="DeviceException"></exception>
        public List<T> Read<T>(string readMsg = "", int timeout = 2000)
        {

            List<T> retDatas = new List<T>();
            if (GetIsShield()) return retDatas;
            ///增加读取值的虚拟方法
            ProcessAction(() =>
            {
                var result = Protocol.Read<T>(Communication, readMsg, timeout);
                if (result.IsSuccess)
                {
                    retDatas = result.Datas;
                }
                else
                {
                    Communication.Close();
                    throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID);
                }

                VStatus = VStatus.Idle;
                OnLog(Common.DataStruct.Enums.LogType.Debug, $"{readMsg} 读取到值 {string.Join(",", retDatas)}");
            }, () => { Thread.Sleep(TargetValue); });
            return retDatas;
        }

        /// <summary>
        /// 读取值
        /// </summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="DeviceException"></exception>
        public T ReadSingle<T>(string readMsg = "", int timeout = 2000, bool isLog = true, bool isCloseConnect = true)
        {
            T retVal = default(T);
            if (GetIsShield()) return retVal;
            var result = Protocol.Read<T>(Communication, readMsg, timeout);

            ///增加读取值的虚拟方法
            ProcessAction(() =>
            {
                if (result.IsSuccess)
                {
                    retVal = result.Datas[0];
                }
                else
                {
                    if (isCloseConnect)
                    {
                        Communication.Close();
                        OnLog(Common.DataStruct.Enums.LogType.Debug, $"通信发生异常已主动关闭！！！");
                    }

                    if (typeof(T) == typeof(string))
                    {
                        retVal = (T)(object)string.Empty;
                        OnLog(Common.DataStruct.Enums.LogType.Debug, $"通信发生异常返回默认空值！！！");
                    }
                }

                if (isLog)
                {
                    OnLog(Common.DataStruct.Enums.LogType.Debug, $"{readMsg} 读取到值 {retVal}");
                }
            },
            () => { Thread.Sleep(TargetValue); });

            return retVal;
        }


        /// <summary>
        /// 写入值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="writeMsg"></param>
        /// <exception cref="DeviceException"></exception>
        public void Write<T>(List<T> values, string writeMsg = "")
        {
            if (GetIsShield()) return;
            ///增加写入的虚拟方法
            ProcessAction(() =>
            {
                var commResult = Protocol.Write<T>(Communication, values, writeMsg);
                if (!commResult.IsSuccess)
                {
                    Communication.Close();
                    throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, commResult.ErrorMsg);
                }
            }, () => { Thread.Sleep(TargetValue); });
        }

        /// <summary>
        /// 写入值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="writeMsg"></param>
        /// <exception cref="DeviceException"></exception>
        public void Write<T>(T value, string writeMsg = "", bool isWriteLog = true)
        {
            if (isWriteLog)
            {
                OnLog(LogType.Debug, $"设备:{Name} {writeMsg} 写入值 {value}");
            }

            if (GetIsShield()) return;
            //增加写入的虚拟方法
            CommResult commResult = null;
            ProcessAction(() =>
            {
                if (!Communication.IsConnected) return;

                commResult = Protocol.Write<T>(Communication, new List<T>() { value }, writeMsg);
                if (!commResult.IsSuccess)
                {
                    Communication.Close();
                    throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, commResult.ErrorMsg);
                }
            }, () => { Thread.Sleep(TargetValue); });

            if (commResult != null && !string.IsNullOrEmpty(commResult.SentHex))
            {
                OnLog(LogType.Debug, $"发送指令: {commResult.SentHex}");
            }
        }
        /// <summary>
        /// 写入值,判断是否成功，不阻塞
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="writeMsg"></param>
        /// <exception cref="DeviceException"></exception>
        public bool WriteWithoutBlocking<T>(T value, string writeMsg = "")
        {

            OnLog(LogType.Debug, $"设备:{Name} {writeMsg} 写入值 {value}");
            if (GetIsShield()) return true;

            var commResult = Protocol.Write<T>(Communication, new List<T>() { value }, writeMsg);
            if (!commResult.IsSuccess)
            {
                Communication.Close();
            }
            return commResult.IsSuccess;
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
                case Enums.ProtocolType.StringUtf8:
                    retProtocol = new StringUtf8Protocol();
                    break;
                case Enums.ProtocolType.StringDefault:
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
                case Enums.ProtocolType.MCBinary:
                    retProtocol = new MCBinary();
                    break;

                case Enums.ProtocolType.FinsTCP:
                    retProtocol = new FinsTCP();
                    break;

                    //case Enums.ProtocolType.HslModbusTcp:
                    //    retProtocol = new HslMdobus();
                    //    break;   [VCommunication.cs]
            }

            ProtocolType = protocol;

            retProtocol.LogEvent -= RetProtocol_LogEvent;
            retProtocol.LogEvent += RetProtocol_LogEvent;

            // 真实协议
            Protocol = retProtocol;
        }

        /// <summary>
        /// 更改品牌
        /// </summary>
        /// <param name="brandType"></param>
        public void SetBrand(BrandType brandType)
        {
            Brand = brandType;
        }


        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void RetProtocol_LogEvent(LogType logType, string logMsg)
        {
            OnLog(logType, logMsg);
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        public void WriteAction(string actionName)
        {
            var action = Actions.FirstOrDefault(u => u.Name == actionName);
            if (action == null)
            {
                throw new FriendlyException($"设备：{Name}不存在动作:{actionName}!");
            }

            if (Protocol == null)
            {
                throw new FriendlyException($"设备：{Name},未配置协议!");
            }

            switch (action.DataType)
            {
                case DataType.Bool:
                    Write<bool>(bool.Parse(action.Value), action.Command);
                    break;
                case DataType.Int:
                    Write<int>(int.Parse(action.Value), action.Command);
                    break;
                case DataType.String:
                    Write<string>(action.Value, action.Command);
                    break;
                default:
                    throw new FriendlyException($"动作类型:{action.DataType}不支持");
            }
        }

        /// <summary>
        /// 回零
        /// </summary>
        public override void Home()
        {
            foreach (var item in Actions)
            {
                if (NeedHome && item.IsHome)
                {
                    WriteAction(item.Name);
                }
            }

            if (NeedHome)
            {
                if (Communication != null && Communication.IsConnected)
                {
                    Communication.Close();
                }

                // 回零时默认开启连接
                var result = Communication.Open(1000);
                if (!result.IsSuccess)
                {
                    throw new FriendlyException($"通讯设备:{Name} 连接失败!");
                }
            }
        }

        /// <summary>
        /// 读取动作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionName"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        public T ReadAction<T>(string actionName)
        {
            var action = Actions.FirstOrDefault(u => u.Name == actionName);
            if (action == null)
            {
                throw new FriendlyException($"设备：{Name}不存在动作:{actionName}!");
            }

            if (Protocol == null)
            {
                throw new FriendlyException($"设备：{Name},未配置协议!");
            }

            return ReadSingle<T>(actionName);
        }


        #region 导入和导出
        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);

            // 配置协议
            SetProtocol(ProtocolType);

        }

        #endregion

        #region 暂停停止

        /// <summary>
        /// 等待值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="readMsg"></param>
        /// <param name="timeout"></param>
        public void Wait<T>(Guid moduleID, T val, OpRule opRule, string readMsg = "", int timeout = 2000,
            CancellationTokenSource stopSource = null, int sleepTime = 50) where T : IComparable
        {
            if (GetIsShield()) return;

            // 构建停止
            if (stopSource == null)
            {
                stopSource = new CancellationTokenSource();
            }

            // 同一个模块第二次运行，如果上一次还在运行，就要将上一个的Wait结束掉
            VStatus = VStatus.Running;

            try
            {
                OnLog(Common.DataStruct.Enums.LogType.Debug, $"{readMsg} Wait等待值 {opRule.GetDescription()} {val}");
                Protocol.Wait<T>(Communication, readMsg, val, opRule, timeout, sleepTime, stopSource);
                OnLog(Common.DataStruct.Enums.LogType.Debug, $"{readMsg} Wait满足值 {opRule.GetDescription()} {val}");
                VStatus = VStatus.Idle;
            }
            catch (TimeoutException ex)
            {
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, ex.Message);
            }
        }

        /// <summary>
        /// 停止循环
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            // 将在等待的模块停止掉
            if (stopResetDict != null)
            {
                foreach (var item in stopResetDict)
                {
                    item.Value?.Cancel();
                }
            }

            // 关掉通信
            if (Communication != null)
            {
                Communication.Close();
            }
        }

        public bool IsHealth(out string connStr)
        {
            connStr = string.Empty;

            if (Communication != null)
            {
                //connStr = Communication.ToString();
                return Communication.TryConnect();
            }

            return false;
        }

        /// <summary>
        /// 通信连接
        /// </summary>
        public string ConnectStr => Communication.ToString();

        /// <summary>
        /// 对象Clone
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            VCommuncation newClone = new VCommuncation();
            ICommunication newComu = CommunicationBase.CreateByType(this.Communication?.GetType());
            newComu.ParserXml(this.Communication.ExportXml());
            newClone.LogEvent -= OnLog;
            newClone.LogEvent += OnLog;
            newClone.Communication = newComu;
            newClone.Engine = this.Engine;
            newClone.SetProtocol(ProtocolType);

            return newClone;
        }

        /// <summary>
        /// 点检通讯
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            bool isOK = false;
            isOK = OnCheckEvent($"即将点检通讯:{this.Name}", true);
            if (!isOK)
            {
                return isOK;
            }
            bool result = ConnectOK();
            isOK = OnCheckEvent($"尝试连接...连接成功", result);
            return isOK;
        }

        public bool ConnectOK()
        {
            return Communication.Open(5000).IsSuccess;
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

        #endregion
    }

    /// <summary>
    /// 行为
    /// </summary>
    public class SocketAction : IXMLParser
    {
        /// <summary>
        /// 命令名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 命令
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 是否是回零动作
        /// </summary>
        public bool IsHome { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        public ActionType ActionType { get; set; }

        /// <summary>
        /// 读取类型
        /// </summary>
        public Luster.Common.DataStruct.Enums.DataType DataType { get; set; }

        /// <summary>
        /// 写入的值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 序列化
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

    public class SlaveItem : IXMLParser
    {
        /// <summary>
        /// 对应的PosKey
        /// </summary>
        public Guid PosKey { get; set; }

        /// <summary>
        /// 命令名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 序列化
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
}