#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCameraContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VCameraContentVM.cs
* 创建时间:       2022/4/28 15:43:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      488ef7ec-e241-41fb-a4e1-23403f2ee15b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/28 15:43:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.DataModels;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Services.Dialogs;
using Luster.Motion.DataStruct.Real;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.DataModels;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Luster.Common.Assets;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VCameraContentVM : PageVM
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private BitmapFrame _frameData;

        /// <summary>
        /// 视频流数据
        /// </summary>
        public BitmapFrame FrameData
        {
            get => _frameData;
            set { SetProperty(ref _frameData, value); }
        }

        /// <summary>
        /// 当前选择的model
        /// </summary>
        private VCameraModel _selectedVCameraModel;
        public VCameraModel SelectedVCameraModel
        {
            get => _selectedVCameraModel;
            set => SetProperty(ref _selectedVCameraModel, value);
        }

        private VCamera _vCamera;

        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 获取视频流
        /// </summary>
        bool isGet;


        /// <summary>
        /// 面阵相机列表
        /// </summary>
        private ObservableCollection<LightModel> _lightModels;
        public ObservableCollection<LightModel> LightModels
        {
            get { return _lightModels; }
            set { SetProperty(ref _lightModels, value); }
        }


        /// <summary>
        /// 面阵相机列表
        /// </summary>
        private ObservableCollection<VCameraModel> _vCameraModels;
        public ObservableCollection<VCameraModel> VCameraModels
        {
            get { return _vCameraModels; }
            set { SetProperty(ref _vCameraModels, value); }
        }

        /// <summary>
        /// 打开相机
        /// </summary>
        public DelegateCommand OpenCameraCommand { get; private set; }

        /// <summary>
        /// 开启相机取流
        /// </summary>
        public DelegateCommand StartGrabCommand { get; private set; }

        /// <summary>
        /// 截取一帧图片
        /// </summary>
        public DelegateCommand ShotScreenCommand { get; private set; }

        /// <summary>
        /// 关闭相机
        /// </summary>
        public DelegateCommand CloseCameraCommand { get; private set; }

        /// <summary>
        /// 参数应用
        /// </summary>
        public DelegateCommand ApplyCameraCommand { get; set; }

        /// <summary>
        /// 离线文件选择
        /// </summary>
        public DelegateCommand SelectFileCommand { get; set; }


        /// <summary>
        /// 设置光源亮度
        /// </summary>
        public DelegateCommand<LightModel> SetIntensityCommand { get; private set; }

        /// <summary>
        /// 删除选中设备
        /// </summary>
        public DelegateCommand<VCameraModel> RemoveCommand { get; private set; }

        /// <summary>
        /// 选中行事件
        /// </summary>
        public DelegateCommand<VCameraModel> SelectedCommand { get; private set; }

        protected VCameraContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            LoadDevices();
            ApplyCameraCommand = new DelegateCommand(ApplyCamera);
            OpenCameraCommand = new DelegateCommand(OpenCamera);
            ShotScreenCommand = new DelegateCommand(ShotScreen);
            CloseCameraCommand = new DelegateCommand(CloseCamera);
            StartGrabCommand = new DelegateCommand(StartGrab);
            SelectFileCommand = new DelegateCommand(SelectFile);
            RemoveCommand = new DelegateCommand<VCameraModel>(Remove);
            SetIntensityCommand = new DelegateCommand<LightModel>(SetIntensity);
            SelectedCommand = new DelegateCommand<VCameraModel>(Selected);
        }

        /// <summary>
        /// 选择离线图片文件
        /// </summary>
        private void SelectFile()
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = "(*.jpg,*.png,*.jpeg,*.bmp,*.gif)|*.jgp;*.png;*.jpeg;*.bmp;*.gif|All files(*.*)|*.*";
            if (openFile.ShowDialog() == true)
            {
                var filename = openFile.FileName;
                SelectedVCameraModel.ImagePath = filename;
                FrameData = BitmapFrame.Create(openFile.OpenFile());
            }
            
        }

        /// <summary>
        /// 截取相机一帧图片
        /// </summary>
        private void ShotScreen()
        {
            if (_vCamera != null)
            {
                _vCamera.OnCamera();
            }
        }

        /// <summary>
        /// 打开相机
        /// </summary>
        private void OpenCamera()
        {
            if (_vCamera != null)
            {
                // 先取消，在挂事件反正多次挂事件
                _vCamera.FrameImageEvent -= ShowFrameData;
                _vCamera.OpenCamera();
                _vCamera.FrameImageEvent += ShowFrameData;
                isGet = false;
            }
        }

        /// <summary>
        /// 开启相机连续取图
        /// </summary>
        private void StartGrab()
        {
            if (_vCamera != null)
            {
                _vCamera.CameraStarGrab();
            }
        }


        #region 通过回调函数生成UI图片
        private void ShowFrameData(LImage frameData)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                if(!isGet)
                {
                    FrameData = ConvertByteArrayToBitmap(frameData.Pointer, frameData.Channel, frameData.Width, frameData.Height);
                }
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

        BitmapFrame bitmapFrame = null;
        private BitmapFrame ConvertByteArrayToBitmap(IntPtr data, int channel, int imgW, int imgH)
        {
            System.Drawing.Imaging.PixelFormat imageFormat = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            if (channel == 3)
            {
                imageFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            }
            try
            {
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

                    bitmapFrame = BitmapFrame.Create(bitmapImage);
                    return bitmapFrame;
                    //return bitmapImage;
                }

            }
            catch (Exception ex)
            {
                deviceEngine.OnLog(LogType.Error, $"视频流回调错误，生成图像错误,详细信息{ex.Message}");
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 关闭相机
        /// </summary>
        private void CloseCamera()
        {
            if (_vCamera != null)
            {
                isGet = true;
                _vCamera.FrameImageEvent -= ShowFrameData;
                _vCamera.CloseCamera();
                FrameData = null;
            }
        }


        /// <summary>
        /// 应用相机参数
        /// </summary>
        private void ApplyCamera()
        {
            if (_vCamera != null)
            {
                _vCamera.SetCameraParas();
            }
        }

        /// <summary>
        /// 设置通道亮度
        /// </summary>
        /// <param name="lightModel"></param>
        private void SetIntensity(LightModel lightModel)
        {
            if (lightModel != null)
            {
                if (_vCamera != null)
                {

                    _vCamera.SetLightIntensity(lightModel.ChannelId, lightModel.Intensity, lightModel.OriginalId);
                }
            }
        }

        /// <summary>
        /// 选中行事件
        /// </summary>
        /// <param name="item">行绑定的ViewModel</param>
        private void Selected(VCameraModel item)
        {
            SelectedVCameraModel = item;
            if (_vCamera != null)
            {
                // 取消原相机取流事件
                _vCamera.FrameImageEvent -= ShowFrameData;
            }
            if (item != null)
            {
                var device = deviceEngine.GetVirtualByID(item.ID);
                _vCamera = device as VCamera;
                if (_vCamera != null)
                {
                    UpdateLightModel(_vCamera);
                    // 更新配置
                    PropertyObj = _vCamera.GetRealCamera();
                    // 加载相机取流事件
                    _vCamera.FrameImageEvent += ShowFrameData;
                }
            }
            else
            {
                PropertyObj = new object();
                LightModels = new ObservableCollection<LightModel>();
            }
        }

        /// <summary>
        /// 生成光源通道数据
        /// </summary>
        /// <param name="vCamera"></param>
        private void UpdateLightModel(VCamera vCamera)
        {
            LightModels = new ObservableCollection<LightModel>();
            // 已有配置通道，根据实际配置生成
            if (vCamera.ChannelInfos.Count > 0)
            {
                foreach (var item in vCamera.ChannelInfos)
                {
                    var model = new LightModel()
                    {
                        ChannelId = item.Key,
                        OriginalId = item.Key,
                        Intensity = item.Value
                    };
                    LightModels.Add(model);
                }
            }
            // 未配置通道，根据所需数量生产
            else
            {
                for (int i = 0; i < vCamera.ChannelCount; i++)
                {
                    var model = new LightModel()
                    {
                        ChannelId = i + 1,
                        OriginalId = i + 1,
                        Intensity = 100
                    };
                    LightModels.Add(model);
                }
            }
        }

        /// <summary>
        /// 移除选中设备
        /// </summary>
        /// <param name="item"></param>
        private void Remove(VCameraModel item)
        {
            dialogService.ShowConfirm($"确认删除相机:{item.Name}", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    deviceEngine.ReomoveVirtual(item.ID);
                    var model = VCameraModels.FirstOrDefault(u => u.ID == item.ID);
                    if (model != null)
                    {
                        if (model.ID == SelectedVCameraModel.ID)
                        {
                            SelectedVCameraModel = null;
                        }
                        VCameraModels.Remove(model);
                    }
                }
            });
        }

        /// <summary>
        /// 添加设备
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowVCameraDialog(r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<VCamera>("VCamera", out var vM))
                    {
                        deviceEngine.AddVirtual(vM);
                        LoadDevices();
                    }
                }
            });
        }


        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices()
        {
            var devices = deviceEngine.GetDevices(typeof(VCamera));
            VCameraModels = new ObservableCollection<VCameraModel>();
            foreach (var device in devices)
            {
                var vCamera = device as VCamera;
                var vCameraModel = new VCameraModel(vCamera);
                VCameraModels.Add(vCameraModel);
            }
        }
    }
}