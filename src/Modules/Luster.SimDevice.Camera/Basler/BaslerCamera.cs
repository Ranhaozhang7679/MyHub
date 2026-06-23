using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice;
using System;
using System.Collections.Generic;

namespace Luster.SimDevice.Camera.Basler
{
    /// <summary>
    /// Basler 相机适配器（TES-33 P8-D）。
    /// 按 <see cref="ICamera"/> 契约新建，落 Devices/ 由 <c>DeviceEngine</c> 反射发现。
    /// </summary>
    /// <remarks>
    /// <b>SDK 接入策略</b>：Basler Pylon SDK（Pylon.dll）仓库未提供，真实 SDK 调用经
    /// <see cref="IBaslerSdk"/> 抽象注入；<see cref="SimulationMode"/>=true 时走软件层 mock
    /// （开相机/采图/存图返回模拟结果），真实模式需现场接入 Pylon SDK 实现 <see cref="IBaslerSdk"/>。
    /// 现场用啥补啥——非现场型号可后补。真实采图 ⚠️ 待 P8-D 现场接入。
    /// </remarks>
    public class BaslerCamera : CameraBase, ICamera
    {
        /// <summary>Basler SDK 抽象（现场注入 Pylon 实现；null=未接入）</summary>
        [Ignore]
        public IBaslerSdk Sdk { get; set; }

        /// <summary>模拟模式（true=软件层 mock，false=真实 SDK）</summary>
        [PropItem(10, DisplayName = "模拟模式")]
        public bool SimulationMode { get; set; } = true;

        /// <inheritdoc/>
        public override string Brand => "Basler";

        public bool IsOpen { get; set; }

        /// <inheritdoc/>
        public event Action<LImage> FrameImageEvent;

        public BaslerCamera()
        {
        }

        /// <summary>触发帧图像事件（SimulationMode 模拟 / 真实 SDK 抓拍后调用）</summary>
        protected void RaiseFrameImage(LImage image)
        {
            FrameImageEvent?.Invoke(image);
        }

        /// <inheritdoc/>
        public override void Open()
        {
            string msg;
            CameraOpen(out msg);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            CloseCamera();
        }

        /// <inheritdoc/>
        public void CameraListRead(out List<IDevice> m_stDeviceList)
        {
            m_stDeviceList = new List<IDevice>();
            if (SimulationMode)
            {
                // 模拟：返回自身作为唯一设备
                m_stDeviceList.Add(this);
                return;
            }
            Sdk?.EnumDevices(out m_stDeviceList);
        }

        /// <inheritdoc/>
        public bool CameraOpen(out string message)
        {
            message = string.Empty;
            if (IsOpen) return true;

            if (SimulationMode)
            {
                if (string.IsNullOrEmpty(SerialNumber))
                {
                    message = "序列号为空!";
                    return false;
                }
                IsOpen = true;
                return true;
            }

            if (Sdk == null)
            {
                message = "Basler SDK 未接入（Pylon），请现场接入 IBaslerSdk 实现";
                return false;
            }
            bool ok = Sdk.Open(SerialNumber, out message);
            IsOpen = ok;
            return ok;
        }

        /// <inheritdoc/>
        public void CameraStartGrab()
        {
            if (SimulationMode) { return; }
            Sdk?.StartGrabbing();
        }

        /// <inheritdoc/>
        public void CameraStopGrab()
        {
            if (SimulationMode) { return; }
            Sdk?.StopGrabbing();
        }

        /// <inheritdoc/>
        public void CameraPicSave()
        {
            // 图片保存走 P8-B ImageArchiveService（按 SN 归档），此处仅触发抓拍保存
            if (SimulationMode) { return; }
            Sdk?.SavePicture();
        }

        public void CameraVedioSave() { if (!SimulationMode) Sdk?.SaveVideo(); }

        public void CameraNameSet(string cameraName) { if (!SimulationMode) Sdk?.SetName(cameraName); }

        public void CameraIpSet(uint cameraIp, uint cameraSubnet, uint cameraGateway)
        { if (!SimulationMode) Sdk?.SetIp(cameraIp, cameraSubnet, cameraGateway); }

        /// <inheritdoc/>
        public void CameraParaRead(out float frameRate, out float exposureTime, out float gain, out float gamma)
        {
            frameRate = FrameRate; exposureTime = ExposureTime; gain = Gain; gamma = 0;
            if (SimulationMode) { return; }
            Sdk?.ReadParams(out frameRate, out exposureTime, out gain, out gamma);
        }

        public void CameraEnumParaSet(string strKey, uint nValue) { if (!SimulationMode) Sdk?.SetEnumParam(strKey, nValue); }
        public void CameraCommandValSet(string strKey) { if (!SimulationMode) Sdk?.SetCommand(strKey); }

        /// <inheritdoc/>
        public void CloseCamera()
        {
            if (!SimulationMode) Sdk?.Close();
            IsOpen = false;
        }

        public void CameraFromFile(string filename) { /* Basler 配置文件加载，现场接入时补 */ }

        /// <summary>模拟采图（返回模拟图片字节，供 P8-B ImageArchiveService 归档测试）</summary>
        public byte[] SimulateCapture(int width = 1024, int height = 768)
        {
            if (!SimulationMode) return null;
            // 模拟灰度图字节
            var data = new byte[width * height];
            for (int i = 0; i < data.Length; i++) data[i] = (byte)(i % 256);
            return data;
        }
    }
}
