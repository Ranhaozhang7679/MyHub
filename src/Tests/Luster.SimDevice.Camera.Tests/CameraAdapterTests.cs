using Luster.SimDevice.Camera.Basler;
using Luster.SimDevice.Camera.Dahua;
using Luster.Motion.DataStruct.Real;
using Xunit;

namespace Luster.SimDevice.Camera.Tests
{
    /// <summary>
    /// TES-33 P8-D:Basler/Dahua 相机适配器 SimulationMode 单测。
    /// 真实 SDK（Pylon/Dahua MV）未接入，软件层 SimulationMode mock 覆盖开相机/采图/参数。
    /// </summary>
    public class CameraAdapterTests
    {
        [Fact]
        public void Basler_SimulationMode_开相机成功()
        {
            var cam = new BaslerCamera { SimulationMode = true, SerialNumber = "BSL001" };
            bool ok = cam.CameraOpen(out string msg);
            Assert.True(ok);
            Assert.True(cam.IsOpen);
        }

        [Fact]
        public void Basler_SimulationMode_序列号空失败()
        {
            var cam = new BaslerCamera { SimulationMode = true, SerialNumber = "" };
            bool ok = cam.CameraOpen(out string msg);
            Assert.False(ok);
            Assert.Contains("序列号", msg);
        }

        [Fact]
        public void Basler_SimulationMode_采图返回字节()
        {
            var cam = new BaslerCamera { SimulationMode = true, SerialNumber = "BSL001" };
            byte[] data = cam.SimulateCapture(100, 50);
            Assert.NotNull(data);
            Assert.Equal(100 * 50, data.Length);
        }

        [Fact]
        public void Basler_SimulationMode_设备列表含自身()
        {
            var cam = new BaslerCamera { SimulationMode = true, SerialNumber = "BSL001" };
            cam.CameraListRead(out var list);
            Assert.Single(list);
        }

        [Fact]
        public void Basler_真实模式无Sdk_开相机失败带提示()
        {
            var cam = new BaslerCamera { SimulationMode = false, SerialNumber = "BSL001" };
            bool ok = cam.CameraOpen(out string msg);
            Assert.False(ok);
            Assert.Contains("SDK", msg);
        }

        [Fact]
        public void Basler_Brand为Basler()
        {
            var cam = new BaslerCamera();
            Assert.Equal("Basler", cam.Brand);
        }

        [Fact]
        public void Basler_实现ICamera接口()
        {
            ICamera cam = new BaslerCamera { SimulationMode = true, SerialNumber = "BSL001" };
            Assert.NotNull(cam);
        }

        [Fact]
        public void Dahua_SimulationMode_开相机成功()
        {
            var cam = new DahuaCamera { SimulationMode = true, SerialNumber = "DH001" };
            bool ok = cam.CameraOpen(out string msg);
            Assert.True(ok);
            Assert.True(cam.IsOpen);
        }

        [Fact]
        public void Dahua_SimulationMode_采图返回字节()
        {
            var cam = new DahuaCamera { SimulationMode = true, SerialNumber = "DH001" };
            byte[] data = cam.SimulateCapture(80, 60);
            Assert.NotNull(data);
            Assert.Equal(80 * 60, data.Length);
        }

        [Fact]
        public void Dahua_SimulationMode_参数读取返回默认()
        {
            var cam = new DahuaCamera { SimulationMode = true, FrameRate = 15, ExposureTime = 20000, Gain = 5 };
            cam.CameraParaRead(out float fr, out float exp, out float gain, out float gamma);
            Assert.Equal(15, fr);
            Assert.Equal(20000, exp);
            Assert.Equal(5, gain);
        }

        [Fact]
        public void Dahua_真实模式无Sdk_开相机失败()
        {
            var cam = new DahuaCamera { SimulationMode = false, SerialNumber = "DH001" };
            bool ok = cam.CameraOpen(out string msg);
            Assert.False(ok);
            Assert.Contains("SDK", msg);
        }

        [Fact]
        public void Dahua_Brand为Dahua()
        {
            var cam = new DahuaCamera();
            Assert.Equal("Dahua", cam.Brand);
        }

        [Fact]
        public void Dahua_实现ICamera接口()
        {
            ICamera cam = new DahuaCamera { SimulationMode = true, SerialNumber = "DH001" };
            Assert.NotNull(cam);
        }

        [Fact]
        public void 两适配器_关相机后IsOpen为false()
        {
            var basler = new BaslerCamera { SimulationMode = true, SerialNumber = "BSL001" };
            basler.CameraOpen(out _);
            Assert.True(basler.IsOpen);
            basler.CloseCamera();
            Assert.False(basler.IsOpen);

            var dahua = new DahuaCamera { SimulationMode = true, SerialNumber = "DH001" };
            dahua.CameraOpen(out _);
            dahua.CloseCamera();
            Assert.False(dahua.IsOpen);
        }
    }
}
