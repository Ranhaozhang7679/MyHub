#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VModbusServer
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.VDevice
* 文 件 名:       VModbusServer.cs
* 创建时间:       2026/06/22
* 作    者:       Multica 全栈工程师
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 创建年份:       2026
* 备    注:       TES-45(TES-37-2) Modbus TCP Server 侧虚拟设备,依 ADR-TES-37 D1/D5
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.VDevice
{
    /// <summary>
    /// Modbus TCP Server 侧虚拟设备:持 Hsl <see cref="HslCommunication.ModBus.ModbusTcpServer"/>,
    /// DataPool 即数据视图,被 HandoverICWNode(Server 侧)经 <c>GetVDevice&lt;VModbusServer&gt;</c> 引用。
    /// <para>实现依据 ADR-TES-37 决策 D1(用 Hsl ModbusTcpServer 移植)+ D5(新建 VModbusServer 虚拟设备)。</para>
    /// <para>不侵入既有 <c>CommTCP</c>/<c>ModbusProtocol</c>/<c>VPlc</c> 契约,纯新增。PLC 可作 Master 读写站状态/产品信息。</para>
    /// <para>迁移自源端 <c>HandoverModbusTcpServer</c>/<c>HandoverICWModbusTcpServer</c> 的 Server 侧能力
    /// (DataPool / 字节序 / 心跳 / <c>SaveDataPool</c> / <c>LoadDataPool(CacheFileName)</c> 持久化)。</para>
    /// </summary>
    public class VModbusServer : VirtualDeviceBase, IHome, IStop, IHeartbeat, IDeviceError, IDisposable
    {
        #region 字段

        /// <summary>
        /// Hsl Server 实例(非序列化,运行时构建)
        /// </summary>
        [Ignore]
        public HslCommunication.ModBus.ModbusTcpServer Server { get; private set; }

        /// <summary>
        /// 心跳定时器(非序列化)
        /// </summary>
        [Ignore]
        private System.Timers.Timer _heartbeatTimer;

        /// <summary>
        /// 最近一次心跳活跃时间
        /// </summary>
        [Ignore]
        private DateTime _lastBeatTime;

        /// <summary>
        /// 同步锁
        /// </summary>
        [Ignore]
        private readonly object _lockObj = new object();

        #endregion

        #region 配置属性

        /// <summary>
        /// 页面排序
        /// </summary>
        public override int Sort => 9;

        /// <summary>
        /// 监听端口(对齐源端 socketProfile.Port,默认 502)
        /// </summary>
        public int Port { get; set; } = 502;

        /// <summary>
        /// 站号(对齐源端 Hsl Station,默认 1)
        /// </summary>
        public int Station { get; set; } = 1;

        /// <summary>
        /// 监听 IP 地址(配置项;Hsl <see cref="HslCommunication.Core.Net.NetworkServerBase.ServerStart(int)"/>
        /// 仅按端口监听,绑定所有网卡,此处仅作工程配置记录与展示)
        /// </summary>
        public string IpAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// 寄存器缓存持久化文件名(对齐源端 CacheFileName,默认 ModbusServer.dat)
        /// </summary>
        public string CacheFileName { get; set; } = "ModbusServer.dat";

        /// <summary>
        /// 字节顺序(映射到 Hsl DataFormat,默认 DCBA,对齐源端 CommReadString 行为)
        /// </summary>
        public EndianType EndianType { get; set; } = EndianType.DCBA;

        /// <summary>
        /// 字符串字节序反转(对齐源端 IsStringReverse = true)
        /// </summary>
        public bool IsStringReverse { get; set; } = true;

        /// <summary>
        /// Hsl 激活码(对齐源端 HandoverCollection 构造内 SetAuthorizationCode)。
        /// <para>⚠️ 授权合规待人类确认(见父 Issue 评审结论),实现按源端原样搬,合规结论出来前不交付生产。</para>
        /// </summary>
        public string AuthorizationCode { get; set; } = "f562cc4c-4772-4b32-bdcd-f3e122c534e3";

        /// <summary>
        /// 心跳寄存器地址(字符串地址,对齐源端 socketProfile.Heart);留空则关闭心跳监控
        /// </summary>
        public string HeartbeatAddress { get; set; } = string.Empty;

        /// <summary>
        /// 心跳周期(秒,对齐源端 socketProfile.BeatTime)
        /// </summary>
        public int BeatTime { get; set; } = 3;

        /// <summary>
        /// 心跳离线判定超时(秒,对齐源端 socketProfile.ReadTimeOut)
        /// </summary>
        public int ReadTimeOut { get; set; } = 10;

        /// <summary>
        /// 是否在线(心跳监控结果,非序列化)
        /// </summary>
        [Ignore]
        public bool IsOnline { get; private set; }

        /// <summary>
        /// Server 是否已启动
        /// </summary>
        [Ignore]
        public bool IsStarted => Server?.IsStarted ?? false;

        /// <summary>
        /// 错误代码
        /// </summary>
        public override DeviceError[] ErrorCodes => new DeviceError[]
        {
            DeviceError.ConnectTimeFail,
        };

        #endregion

        #region 构造

        /// <summary>
        /// 无参构造(反射构造)
        /// </summary>
        public VModbusServer() : base()
        {
            NeedHome = true;
            CurrentErrorCode = DeviceError.ConnectTimeFail;
        }

        #endregion

        #region 生命周期(IHome / IStop / IHeartbeat)

        /// <summary>
        /// 初始化并启动 Server(对齐源端 TryConnect)。
        /// 由 DeviceEngine 在回零阶段统一调用(IHome.Home)。
        /// </summary>
        public override void Home()
        {
            lock (_lockObj)
            {
                if (IsStarted) return;

                // Hsl 激活码接线(源端原样搬)
                if (!string.IsNullOrEmpty(AuthorizationCode))
                {
                    if (!HslCommunication.Authorization.SetAuthorizationCode(AuthorizationCode))
                    {
                        OnLog(LogType.Warning, $"{Name}:Hsl 授权失败!");
                    }
                }

                Server = new HslCommunication.ModBus.ModbusTcpServer
                {
                    Station = Station,
                    DataFormat = ToHslDataFormat(EndianType),
                    IsStringReverse = IsStringReverse,
                };

                // 恢复持久化数据(DataPool round-trip 的加载端)
                var cachePath = GetCacheFilePath();
                if (!string.IsNullOrEmpty(cachePath) && File.Exists(cachePath))
                {
                    try
                    {
                        Server.LoadDataPool(cachePath);
                        OnLog(LogType.Debug, $"{Name}:加载 DataPool 缓存 {cachePath}");
                    }
                    catch (Exception ex)
                    {
                        OnLog(LogType.Warning, $"{Name}:加载 DataPool 缓存失败:{ex.Message}");
                    }
                }

                try
                {
                    Server.ServerStart(Port);
                    VStatus = VStatus.Idle;
                    OnLog(LogType.Info, $"{Name}:Modbus Server 启动 端口={Port} 站号={Station}");
                }
                catch (Exception ex)
                {
                    VStatus = VStatus.Stop;
                    OnLog(LogType.Error, $"{Name}:Modbus Server 启动失败:{ex.Message}");
                    throw new FriendlyException($"Modbus Server:{Name} 启动失败(端口 {Port}):{ex.Message}");
                }

                // 心跳监控(可选)
                StartHeartbeat();
            }
        }

        /// <summary>
        /// 停止 Server 并持久化 DataPool(对齐源端 Dispose)
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            lock (_lockObj)
            {
                StopHeartbeat();

                if (Server != null && Server.IsStarted)
                {
                    try
                    {
                        SaveDataPool();
                        Server.ServerClose();
                        OnLog(LogType.Info, $"{Name}:Modbus Server 已停止");
                    }
                    catch (Exception ex)
                    {
                        OnLog(LogType.Warning, $"{Name}:Modbus Server 停止异常:{ex.Message}");
                    }
                }

                VStatus = VStatus.Idle;
            }
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            Stop();
        }

        /// <summary>
        /// 健康检查(IHeartbeat):Server 是否在监听
        /// </summary>
        public bool IsHealth(out string connStr)
        {
            connStr = $"{Name} tcp://*:{Port} station={Station}";
            return IsStarted;
        }

        /// <summary>
        /// 通信连接描述
        /// </summary>
        public string ConnectStr => $"tcp://*:{Port} station={Station}";

        #endregion

        #region 寄存器读写(对齐源端 CommReadUshort/CommWriteUshort/CommWriteString/CommReadString)

        /// <summary>
        /// 读单个保持寄存器(ushort)
        /// </summary>
        public ushort ReadRegister(int address)
        {
            EnsureStarted();
            var resp = Server.ReadUInt16(address.ToString(), 1);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:读寄存器 {address} 失败:{resp.Message}");
            }
            return resp.Content[0];
        }

        /// <summary>
        /// 写单个保持寄存器(ushort)
        /// </summary>
        public void WriteRegister(int address, ushort value)
        {
            EnsureStarted();
            var resp = Server.Write(address.ToString(), value);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:写寄存器 {address} 失败:{resp.Message}");
            }
        }

        /// <summary>
        /// 读 32 位无符号(uint,占 2 寄存器)
        /// </summary>
        public uint ReadRegisterUint(int address)
        {
            EnsureStarted();
            var resp = Server.ReadUInt32(address.ToString(), 1);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:读 uint 寄存器 {address} 失败:{resp.Message}");
            }
            return resp.Content[0];
        }

        /// <summary>
        /// 写 32 位无符号(uint,占 2 寄存器)
        /// </summary>
        public void WriteRegisterUint(int address, uint value)
        {
            EnsureStarted();
            var resp = Server.Write(address.ToString(), value);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:写 uint 寄存器 {address} 失败:{resp.Message}");
            }
        }

        /// <summary>
        /// 写字符串(多寄存器编码,length 为寄存器个数)
        /// </summary>
        public void WriteString(int address, string text, int length)
        {
            EnsureStarted();
            var resp = Server.Write(address.ToString(), text, length);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:写字符串 {address} 失败:{resp.Message}");
            }
        }

        /// <summary>
        /// 读字符串(length 为寄存器个数)
        /// </summary>
        public string ReadString(int address, int length)
        {
            EnsureStarted();
            var resp = Server.ReadString(address.ToString(), (ushort)length);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:读字符串 {address} 失败:{resp.Message}");
            }
            return resp.Content;
        }

        /// <summary>
        /// 按字符串地址读单个保持寄存器(对齐源端 CommReadUshort(string,...))
        /// </summary>
        public ushort ReadRegister(string address)
        {
            EnsureStarted();
            var resp = Server.ReadUInt16(address, 1);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:读寄存器 {address} 失败:{resp.Message}");
            }
            return resp.Content[0];
        }

        /// <summary>
        /// 按字符串地址写单个保持寄存器(对齐源端 CommWriteUshort(string,...))
        /// </summary>
        public void WriteRegister(string address, ushort value)
        {
            EnsureStarted();
            var resp = Server.Write(address, value);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:写寄存器 {address} 失败:{resp.Message}");
            }
        }

        /// <summary>
        /// 按字符串地址写字符串(对齐源端 CommWriteString(string,...))
        /// </summary>
        public void WriteString(string address, string value)
        {
            EnsureStarted();
            var resp = Server.Write(address, value);
            if (!resp.IsSuccess)
            {
                throw new FriendlyException($"{Name}:写字符串 {address} 失败:{resp.Message}");
            }
        }

        #endregion

        #region DataPool 持久化

        /// <summary>
        /// 保存 DataPool 到缓存文件(对齐源端 SaveDataPool)
        /// </summary>
        public void SaveDataPool()
        {
            if (Server == null) return;

            var cachePath = GetCacheFilePath();
            if (string.IsNullOrEmpty(cachePath)) return;

            try
            {
                var dir = Path.GetDirectoryName(cachePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                Server.SaveDataPool(cachePath);
                OnLog(LogType.Debug, $"{Name}:保存 DataPool 缓存 {cachePath}");
            }
            catch (Exception ex)
            {
                OnLog(LogType.Warning, $"{Name}:保存 DataPool 缓存失败:{ex.Message}");
            }
        }

        /// <summary>
        /// 获取缓存文件完整路径:CacheFileName 为绝对路径时直接用,否则相对 AppContext.BaseDirectory
        /// </summary>
        private string GetCacheFilePath()
        {
            if (string.IsNullOrEmpty(CacheFileName)) return string.Empty;
            return Path.IsPathRooted(CacheFileName)
                ? CacheFileName
                : Path.Combine(AppContext.BaseDirectory, CacheFileName);
        }

        #endregion

        #region 心跳监控(对齐源端 CheckOnline / ReadHeartBeat / WriteHeartBeat)

        private void StartHeartbeat()
        {
            if (Server == null || !Server.IsStarted) return;
            if (string.IsNullOrEmpty(HeartbeatAddress)) return;

            _lastBeatTime = DateTime.Now;
            _heartbeatTimer = new System.Timers.Timer
            {
                Interval = BeatTime * 1000,
                AutoReset = true,
            };
            _heartbeatTimer.Elapsed += (s, e) => CheckOnline();
            _heartbeatTimer.Start();
        }

        private void StopHeartbeat()
        {
            if (_heartbeatTimer != null)
            {
                _heartbeatTimer.Stop();
                _heartbeatTimer.Dispose();
                _heartbeatTimer = null;
            }
        }

        /// <summary>
        /// 心跳在线检查(对齐源端 CheckOnline:读心跳字判定 PLC 是否活跃)
        /// </summary>
        private void CheckOnline()
        {
            try
            {
                if (Server == null || !Server.IsStarted) return;

                var resp = Server.Read(HeartbeatAddress, 1);
                if (!resp.IsSuccess || resp.Content == null || resp.Content.Length < 2)
                {
                    SetOnlineStatus(false);
                    return;
                }

                // 心跳字非 0 视为活跃
                bool active = BitConverter.ToUInt16(resp.Content, 0) != 0;
                if (active)
                {
                    _lastBeatTime = DateTime.Now;
                    SetOnlineStatus(true);
                }
                else if ((DateTime.Now - _lastBeatTime).TotalSeconds > ReadTimeOut)
                {
                    SetOnlineStatus(false);
                }
            }
            catch (Exception ex)
            {
                OnLog(LogType.Warning, $"{Name}:心跳检查异常:{ex.Message}");
            }
        }

        private void SetOnlineStatus(bool status)
        {
            if (IsOnline != status)
            {
                IsOnline = status;
                OnLog(LogType.Debug, $"{Name}:在线状态变更 -> {status}");
            }
        }

        #endregion

        #region 辅助

        /// <summary>
        /// 启动校验
        /// </summary>
        private void EnsureStarted()
        {
            if (!IsStarted)
            {
                throw new FriendlyException($"Modbus Server:{Name} 未启动,请先 Home()");
            }
        }

        /// <summary>
        /// lmv EndianType → Hsl DataFormat 映射
        /// </summary>
        private static HslCommunication.Core.DataFormat ToHslDataFormat(EndianType endian)
        {
            switch (endian)
            {
                case EndianType.ABCD: return HslCommunication.Core.DataFormat.ABCD;
                case EndianType.BADC: return HslCommunication.Core.DataFormat.BADC;
                case EndianType.CDAB: return HslCommunication.Core.DataFormat.CDAB;
                case EndianType.DCBA: return HslCommunication.Core.DataFormat.DCBA;
                default: return HslCommunication.Core.DataFormat.DCBA;
            }
        }

        #endregion
    }
}
