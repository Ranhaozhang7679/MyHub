using Luster.Motion.DataStruct.Real;
using Luster.SimDevice;
using System.Collections.Generic;

namespace Luster.SimDevice.Camera.Basler
{
    /// <summary>
    /// Basler Pylon SDK 抽象（TES-33 P8-D）。
    /// 仓库未提供 Pylon.dll，真实 SDK 调用经此接口；现场接入 Pylon 后实现该接口。
    /// 软件层 <see cref="BaslerCamera"/> 在 <see cref="BaslerCamera.SimulationMode"/>=true 时不依赖此接口。
    /// </summary>
    public interface IBaslerSdk
    {
        void EnumDevices(out List<IDevice> devices);
        bool Open(string serialNumber, out string message);
        void StartGrabbing();
        void StopGrabbing();
        void SavePicture();
        void SaveVideo();
        void SetName(string name);
        void SetIp(uint ip, uint subnet, uint gateway);
        void ReadParams(out float frameRate, out float exposureTime, out float gain, out float gamma);
        void SetEnumParam(string key, uint value);
        void SetCommand(string key);
        void Close();
    }
}
