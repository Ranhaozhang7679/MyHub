using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.Real;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class VideoDialogVM : DialogVM
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);


        private ImageSource _frameData;

        /// <summary>
        /// 视频流数据
        /// </summary>
        public ImageSource FrameData
        {
            get => _frameData;
            set { SetProperty(ref _frameData, value); }
        }

        public DelegateCommand CloseCameraCommand { get; private set; }

        private ICamera _camera;
        private ISimDeviceEngineUI _simDeviceEngine;

        protected VideoDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            _simDeviceEngine= _engine;
            CloseCameraCommand = new DelegateCommand(CloseCamera);
        }


        private void CloseCamera()
        {
            if (_camera != null)
            {
                _camera.FrameImageEvent -= ShowFrameData;
                _camera.CameraStopGrab();
                _camera.CloseCamera();
                FrameData = null;
            }
            CloseDialog("ok");
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<IDevice>("Device", out var device) && device != null)
            {
                var errMessage = string.Empty;
                _camera = device as ICamera;
               if( _camera.CameraOpen(out errMessage))
                {
                    _camera.FrameImageEvent += ShowFrameData;
                    _camera.CameraStartGrab();
                    _camera.CameraVedioSave();
                }
                if (!string.IsNullOrEmpty(errMessage)) 
                {
                    _simDeviceEngine.Engine.OnLog(Common.DataStruct.Enums.LogType.Error, errMessage);
                }
            }
        }


        private void ShowFrameData(LImage frameData)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                FrameData = ConvertByteArrayToBitmap(frameData.Pointer, frameData.Channel, frameData.Width, frameData.Height);
            }));
        }

        private void SetGrayscalePallete(Bitmap image)
        {
            if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                ColorPalette cp = image.Palette;
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                }
                image.Palette = cp;
            }
        }

        private ImageSource ConvertByteArrayToBitmap(IntPtr data, int channel, int imgW, int imgH)
        {
            try
            {
                System.Drawing.Imaging.PixelFormat imageFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                using (var img = new Bitmap(imgW, imgH, imageFormat))
                {
                    var length = channel * imgW * imgH;
                    //  视频流数据转为Bitmap
                    var rect = new Rectangle(0, 0, imgW, imgH);
                    var imgData = img.LockBits(rect, ImageLockMode.ReadWrite, img.PixelFormat);
                    CopyMemory(imgData.Scan0, data, (uint)length);
                    img.UnlockBits(imgData);

                    SetGrayscalePallete(img);
                    // Bitmap转为ImageSource
                    var stream = new MemoryStream();
                    img.Save(stream, ImageFormat.Bmp);
                    stream.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
            catch(Exception ex)
            {
                _simDeviceEngine.OnLog(Common.DataStruct.Enums.LogType.Error, $"相机视频流回调出错，详细信息:{ex.Message}");
                return null;
            }
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();          
        }
    }
}
