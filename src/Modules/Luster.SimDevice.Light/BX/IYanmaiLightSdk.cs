namespace Luster.SimDevice.Light.BX
{
    /// <summary>
    /// 燕脉光源 SDK 抽象（TES-65 P2-A）。
    /// 源端 BX 调 <c>dll_yanMall</c>（connectTCPIP/RegistEvent/SendData/SoftTrigger 等），仓库未提供该 SDK，
    /// 真实调用经此接口；现场接入 dll_yanMall 后实现。
    /// 软件层 <see cref="LightControllerBX"/> 在 <see cref="LightControllerBX.SimulationMode"/>=true 时不依赖此接口。
    /// </summary>
    public interface IYanmaiLightSdk
    {
        /// <summary>TCP 连接（对齐源端 dll_yanMall.connectTCPIP）</summary>
        bool ConnectTcpIp(string ip, int port);

        /// <summary>断开连接</summary>
        bool DisConnect();

        /// <summary>设置通道亮度（对齐源端通道亮度写入）</summary>
        void SetChannelValue(int channelIndex, int intensity);

        /// <summary>读取通道亮度</summary>
        int GetChannelValue(int channelIndex);

        /// <summary>设置触发模式（1=软触发，0=硬触发）</summary>
        bool SetTriggerMode(ushort mode);

        /// <summary>设置分组参数（燕脉分组光源）</summary>
        bool SetGroupParam(int nGroup);

        /// <summary>获取分组参数</summary>
        bool GetGroupParam(int nGroup);

        /// <summary>软触发（按当前步）</summary>
        bool SoftTrigger(ushort currentStep);
    }
}
