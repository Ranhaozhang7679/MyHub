using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice;
using Luster.SimDevice.Real;
using System;

namespace Luster.SimDevice.Light.BX
{
    /// <summary>
    /// BX 燕脉光源控制器（TES-65 P2-A）。
    /// 迁自源端 <c>Plugin.CommonPlugin\LightControllers\BX\LightControllerBX.cs</c>（燕脉 dll_yanMall 协议），
    /// 实现 <see cref="ILightController"/>（通道亮度）+ 源端厂家方法（Connect/SetTrigMode/SetGroupParm/SoftTrigger）。
    /// </summary>
    /// <remarks>
    /// <b>与 CST 区分</b>：lmv 既有 <c>CSTController</c> 是博兴（CSTControllerDll），本类是燕脉（dll_yanMall），
    /// 源端 BX = 燕脉（<c>stuYanMaiGroup</c>），两者并存（非侵入，不改 CSTController）。
    /// <b>SDK 接入</b>：燕脉 SDK（dll_yanMall）仓库未提供，真实调用经 <see cref="IYanmaiLightSdk"/> 抽象注入；
    /// <see cref="SimulationMode"/>=true 时走软件层 mock，真实模式现场接入 SDK 实现。
    /// 落 Devices/ 由 <c>DeviceEngine.LoadDrivers</c> 反射发现。⚠️ 真实光源控制待现场。
    /// </remarks>
    public class LightControllerBX : LightControllertBase, ILightController
    {
        /// <summary>燕脉 SDK 抽象（现场注入 dll_yanMall 实现；null=未接入）</summary>
        [Ignore]
        public IYanmaiLightSdk Sdk { get; set; }

        /// <summary>模拟模式（true=软件层 mock，false=真实 SDK）</summary>
        [PropItem(10, DisplayName = "模拟模式")]
        public bool SimulationMode { get; set; } = true;

        /// <summary>连接 IP（对齐源端 conncectIp）</summary>
        [PropItem(11, DisplayName = "连接IP")]
        public string ConnectIp { get; set; } = "127.0.0.1";

        /// <summary>连接端口（对齐源端 ConnectPort=10000）</summary>
        [PropItem(12, DisplayName = "连接端口")]
        public int ConnectPort { get; set; } = 10000;

        /// <summary>触发模式：1=软触发，0=硬触发（对齐源端 mTriggerMode）</summary>
        [PropItem(13, DisplayName = "触发模式(1软0硬)")]
        public ushort TriggerMode { get; set; } = 0;

        /// <inheritdoc/>
        public override string Brand => "BX";

        /// <inheritdoc/>
        public int Channel { get; set; }

        private bool _isConnected;

        public LightControllerBX() { _isConnected = false; }

        /// <summary>是否已连接</summary>
        public bool IsConnected => _isConnected;

        /// <inheritdoc/>
        public override void InitApi()
        {
            if (SimulationMode) { _isConnected = true; return; }
            // 真实 SDK 初始化由 Open/Connect 处理
        }

        /// <inheritdoc/>
        public override void Open()
        {
            Connect();
        }

        /// <inheritdoc/>
        public override void Close()
        {
            DisConnect();
        }

        /// <summary>连接光源（对齐源端 Connect：connectTCPIP + RegistEvent）</summary>
        public bool Connect()
        {
            if (SimulationMode) { _isConnected = true; return true; }
            if (Sdk == null) return false;
            _isConnected = Sdk.ConnectTcpIp(ConnectIp, ConnectPort);
            return _isConnected;
        }

        /// <summary>断开连接（对齐源端 DisConnect）</summary>
        public bool DisConnect()
        {
            if (SimulationMode) { _isConnected = false; return true; }
            if (Sdk == null) return false;
            bool ok = Sdk.DisConnect();
            _isConnected = !ok;
            return ok;
        }

        /// <inheritdoc/>
        public void SetChannelAndVal(int channelIndex, int intensity)
        {
            if (SimulationMode) { Channel = channelIndex; return; }
            if (!_isConnected || Sdk == null) return;
            Sdk.SetChannelValue(channelIndex, intensity);
        }

        /// <inheritdoc/>
        public void GetChannelIntensity(int channelIndex, ref int intensity)
        {
            if (SimulationMode) { intensity = Channel == channelIndex ? intensity : 0; return; }
            if (!_isConnected || Sdk == null) return;
            intensity = Sdk.GetChannelValue(channelIndex);
        }

        /// <summary>设置触发模式（对齐源端 SetTrigMode：1软触发/0硬触发）</summary>
        public bool SetTrigMode()
        {
            if (SimulationMode) return true;
            if (!_isConnected || Sdk == null) return false;
            return Sdk.SetTriggerMode(TriggerMode);
        }

        /// <summary>设置分组参数（对齐源端 SetGroupParm，燕脉分组光源）</summary>
        public bool SetGroupParm(int nGroup)
        {
            if (SimulationMode) return true;
            if (!_isConnected || Sdk == null) return false;
            return Sdk.SetGroupParam(nGroup);
        }

        /// <summary>获取分组参数（对齐源端 GetGroupParm）</summary>
        public bool GetGroupParm(int nGroup)
        {
            if (SimulationMode) return true;
            if (!_isConnected || Sdk == null) return false;
            return Sdk.GetGroupParam(nGroup);
        }

        /// <summary>软触发（对齐源端 SoftTrigger，按当前步触发）</summary>
        public bool SoftTrigger(ushort currentStep = 0)
        {
            if (SimulationMode) return true;
            if (!_isConnected || Sdk == null) return false;
            return Sdk.SoftTrigger(currentStep);
        }
    }
}
