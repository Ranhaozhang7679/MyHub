using Luster.SimDevice.Light.BX;
using Luster.SimDevice.Light.CST;
using Luster.Motion.DataStruct.Real;
using Luster.TaskFlow.Motion.Interfaces;
using System.Linq;
using Xunit;

namespace Luster.SimDevice.Light.Tests
{
    /// <summary>
    /// TES-65 P2-A:BX 燕脉光源控制器 + 三色灯 ILightManager 验证。
    /// </summary>
    public class LightControllerBXTests
    {
        #region BX 燕脉光源 SimulationMode

        [Fact]
        public void BX_SimulationMode_Connect成功()
        {
            var light = new LightControllerBX { SimulationMode = true };
            bool ok = light.Connect();
            Assert.True(ok);
            Assert.True(light.IsConnected);
        }

        [Fact]
        public void BX_SimulationMode_DisConnect后IsConnected为false()
        {
            var light = new LightControllerBX { SimulationMode = true };
            light.Connect();
            Assert.True(light.IsConnected);
            light.DisConnect();
            Assert.False(light.IsConnected);
        }

        [Fact]
        public void BX_SimulationMode_SetChannelAndVal不抛()
        {
            var light = new LightControllerBX { SimulationMode = true };
            light.Connect();
            light.SetChannelAndVal(3, 200);
            Assert.Equal(3, light.Channel);
        }

        [Fact]
        public void BX_SimulationMode_厂家方法返回true()
        {
            var light = new LightControllerBX { SimulationMode = true };
            light.Connect();
            Assert.True(light.SetTrigMode());
            Assert.True(light.SetGroupParm(1));
            Assert.True(light.GetGroupParm(1));
            Assert.True(light.SoftTrigger(0));
        }

        [Fact]
        public void BX_真实模式无Sdk_Connect失败()
        {
            var light = new LightControllerBX { SimulationMode = false };
            bool ok = light.Connect();
            Assert.False(ok);
            Assert.False(light.IsConnected);
        }

        [Fact]
        public void BX_真实模式无Sdk_厂家方法返回false()
        {
            var light = new LightControllerBX { SimulationMode = false };
            // 未连接 + 无 Sdk
            Assert.False(light.SetTrigMode());
            Assert.False(light.SoftTrigger());
        }

        [Fact]
        public void BX_Brand为BX()
        {
            var light = new LightControllerBX();
            Assert.Equal("BX", light.Brand);
        }

        [Fact]
        public void BX_实现ILightController接口()
        {
            ILightController light = new LightControllerBX { SimulationMode = true };
            Assert.NotNull(light);
        }

        [Fact]
        public void BX_默认参数对齐源端()
        {
            var light = new LightControllerBX();
            Assert.Equal(10000, light.ConnectPort);     // 源端 ConnectPort=10000
            Assert.Equal(0, light.TriggerMode);          // 源端 mTriggerMode=0 硬触发
        }

        #endregion

        #region BX 与 CST 并存（非侵入）

        [Fact]
        public void BX与CST并存_不同Brand()
        {
            var bx = new LightControllerBX();
            var cst = new CSTController();
            Assert.Equal("BX", bx.Brand);
            Assert.Equal("CST", cst.Brand);
            Assert.NotEqual(bx.Brand, cst.Brand);
        }

        [Fact]
        public void BX与CST均实现ILightController()
        {
            ILightController bx = new LightControllerBX();
            ILightController cst = new CSTController();
            Assert.NotNull(bx);
            Assert.NotNull(cst);
        }

        #endregion

        #region 三色灯 ILightManager 验证（平台既有能力）

        [Fact]
        public void ILightManager接口存在_三色灯走平台()
        {
            // 验收点:三色灯走平台 ILightManager（平台既有,非新建）
            var type = typeof(ILightManager);
            Assert.NotNull(type);
            var methods = type.GetMethods().Select(m => m.Name).ToList();
            Assert.Contains("SetBuzzer", methods);
            Assert.Contains("StartLight", methods);
            Assert.Contains("RunningLight", methods);
            Assert.Contains("StopLight", methods);
        }

        #endregion

        #region IYanmaiLightSdk stub（真实 SDK 接入路径验证）

        [Fact]
        public void BX_注入Sdk_真实模式走Sdk()
        {
            var sdk = new StubYanmaiSdk { ConnectOk = true };
            var light = new LightControllerBX { SimulationMode = false, Sdk = sdk, ConnectIp = "192.168.1.100", ConnectPort = 10000 };
            bool ok = light.Connect();
            Assert.True(ok);
            Assert.True(light.IsConnected);
            Assert.Equal("192.168.1.100", sdk.LastIp);
            Assert.Equal(10000, sdk.LastPort);

            light.SetChannelAndVal(2, 150);
            Assert.Equal(2, sdk.LastChannel);
            Assert.Equal(150, sdk.LastIntensity);

            Assert.True(light.SetTrigMode());
            Assert.True(light.SoftTrigger(5));
        }

        [Fact]
        public void BX_注入Sdk_Connect失败_厂家方法失败()
        {
            var sdk = new StubYanmaiSdk { ConnectOk = false };
            var light = new LightControllerBX { SimulationMode = false, Sdk = sdk };
            Assert.False(light.Connect());
            Assert.False(light.SetTrigMode());
        }

        private class StubYanmaiSdk : IYanmaiLightSdk
        {
            public bool ConnectOk { get; set; } = true;
            public string LastIp { get; private set; }
            public int LastPort { get; private set; }
            public int LastChannel { get; private set; }
            public int LastIntensity { get; private set; }

            public bool ConnectTcpIp(string ip, int port) { LastIp = ip; LastPort = port; return ConnectOk; }
            public bool DisConnect() => true;
            public void SetChannelValue(int channelIndex, int intensity) { LastChannel = channelIndex; LastIntensity = intensity; }
            public int GetChannelValue(int channelIndex) => LastChannel == channelIndex ? LastIntensity : 0;
            public bool SetTriggerMode(ushort mode) => ConnectOk;
            public bool SetGroupParam(int nGroup) => ConnectOk;
            public bool GetGroupParam(int nGroup) => ConnectOk;
            public bool SoftTrigger(ushort currentStep) => ConnectOk;
        }

        #endregion
    }
}
