using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice;
using System;
using System.Collections.Generic;

namespace Luster.SimDevice.Camera.Dahua
{
    /// <summary>
    /// Dahua 相机适配器（TES-33 P8-D）。
    /// 按 <see cref="ICamera"/> 契约新建，落 Devices/ 由 <c>DeviceEngine</c> 反射发现。
    /// 真实 SDK（Dahua MV SDK）仓库未提供，经 <see cref="IDahuaSdk"/> 抽象注入，
    /// <see cref="SimulationMode"/>=true 时走软件层 mock。现场用啥补啥。⚠️ 真实采图待现场。
    /// </summary>
    public class DahuaCamera : CameraBase, ICamera
    {
        [Ignore]
        public IDahuaSdk Sdk { get; set; }

        [PropItem(10, DisplayName = "模拟模式")]
        public bool SimulationMode { get; set; } = true;

        public override string Brand => "Dahua";

        public bool IsOpen { get; set; }

        /// <inheritdoc/>
        public event Action<LImage> FrameImageEvent;

        public DahuaCamera() { }

        /// <summary>触发帧图像事件</summary>
        protected void RaiseFrameImage(LImage image) { FrameImageEvent?.Invoke(image); }

        public override void Open() { string msg; CameraOpen(out msg); }
        public override void Close() { CloseCamera(); }

        public void CameraListRead(out List<IDevice> m_stDeviceList)
        {
            m_stDeviceList = new List<IDevice>();
            if (SimulationMode) { m_stDeviceList.Add(this); return; }
            Sdk?.EnumDevices(out m_stDeviceList);
        }

        public bool CameraOpen(out string message)
        {
            message = string.Empty;
            if (IsOpen) return true;
            if (SimulationMode)
            {
                if (string.IsNullOrEmpty(SerialNumber)) { message = "序列号为空!"; return false; }
                IsOpen = true; return true;
            }
            if (Sdk == null) { message = "Dahua SDK 未接入，请现场接入 IDahuaSdk 实现"; return false; }
            bool ok = Sdk.Open(SerialNumber, out message);
            IsOpen = ok; return ok;
        }

        public void CameraStartGrab() { if (!SimulationMode) Sdk?.StartGrabbing(); }
        public void CameraStopGrab() { if (!SimulationMode) Sdk?.StopGrabbing(); }
        public void CameraPicSave() { if (!SimulationMode) Sdk?.SavePicture(); }
        public void CameraVedioSave() { if (!SimulationMode) Sdk?.SaveVideo(); }
        public void CameraNameSet(string cameraName) { if (!SimulationMode) Sdk?.SetName(cameraName); }
        public void CameraIpSet(uint cameraIp, uint cameraSubnet, uint cameraGateway)
        { if (!SimulationMode) Sdk?.SetIp(cameraIp, cameraSubnet, cameraGateway); }

        public void CameraParaRead(out float frameRate, out float exposureTime, out float gain, out float gamma)
        {
            frameRate = FrameRate; exposureTime = ExposureTime; gain = Gain; gamma = 0;
            if (SimulationMode) return;
            Sdk?.ReadParams(out frameRate, out exposureTime, out gain, out gamma);
        }

        public void CameraEnumParaSet(string strKey, uint nValue) { if (!SimulationMode) Sdk?.SetEnumParam(strKey, nValue); }
        public void CameraCommandValSet(string strKey) { if (!SimulationMode) Sdk?.SetCommand(strKey); }
        public void CloseCamera() { if (!SimulationMode) Sdk?.Close(); IsOpen = false; }
        public void CameraFromFile(string filename) { /* Dahua 配置文件加载，现场接入时补 */ }

        /// <summary>模拟采图（供 P8-B ImageArchiveService 归档测试）</summary>
        public byte[] SimulateCapture(int width = 1024, int height = 768)
        {
            if (!SimulationMode) return null;
            var data = new byte[width * height];
            for (int i = 0; i < data.Length; i++) data[i] = (byte)(i % 256);
            return data;
        }
    }

    /// <summary>Dahua MV SDK 抽象（现场接入后实现）</summary>
    public interface IDahuaSdk
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
