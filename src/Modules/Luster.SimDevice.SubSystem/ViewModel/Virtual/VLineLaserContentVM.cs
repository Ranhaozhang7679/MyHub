#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VLineLaserContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VLineLaserContentVM.cs
* 创建时间:       2022/4/28 15:42:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      cf342628-cfbe-4e3a-bba9-5832166071f1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/28 15:42:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Luster.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VLineLaserContentVM : PageVM
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        protected VLineLaserContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            LoadDevices();
            //获取轴列表
            Axises = _engine.Engine.GetDevices(typeof(VAxis)).Select(u => u as VAxis).ToList();
            TriggerDOs = _engine.Engine.GetDevices(typeof(VIO)).Select(u => u as VIO).ToList().Where(u => u.Behavior == IOBehavior.Output).ToList();
            //线扫赋值开关触发模式 软触发 和 硬件IO触发 
            StartTriggerModes = new List<string>() { "SoftWare", "Hardware" };
            //线扫赋值数据触发模式 时间 编码器 和 硬件IO 
            DataTriggerModes = new List<string>() { "Time", "Encode", "IO" };
            //编码器输出参数类型
            EncoderOut = new List<string>() { "差分", "单端" };
        }

        #region 字段

        private LaserDataMode laserDataMode;

        /// <summary>
        /// 当前
        /// </summary>
        private VLineLaser _vLineLaser;

        //轮廓高度值
        private int[] _IHeightProfile;

        private short[] _SHeightProfile;

        private float[] _FHeightProfile;

        #endregion

        #region 属性

        private string _jobPath;
        /// <summary>
        /// 配置文件地址
        /// </summary>
        public string JobPath
        {
            get { return _jobPath; }
            set { SetProperty(ref _jobPath, value); }
        }

        private bool _jobPathVis = false;
        /// <summary>
        /// 配置文件地址是否显示
        /// </summary>
        public bool JobPathVis
        {
            get { return _jobPathVis; }
            set { SetProperty(ref _jobPathVis, value); }
        }

        private Canvas _profileShowCanvas;

        /// <summary>
        /// 报告单页预览查看器
        /// </summary>
        public Canvas ProfileShowCanvas
        {
            get => _profileShowCanvas;
            set => SetProperty(ref _profileShowCanvas, value);
        }

        private ImageSource _heightImage;

        /// <summary>
        /// 高度图
        /// </summary>
        public ImageSource HeightImage
        {
            get => _heightImage;
            set { SetProperty(ref _heightImage, value); }
        }

        private ImageSource _profileImage;

        /// <summary>
        /// 轮廓图
        /// </summary>
        public ImageSource ProfileImage
        {
            get => _profileImage;
            set { SetProperty(ref _profileImage, value); }
        }

        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 当前选择的线扫相机
        /// </summary>
        private VLineLaserModel _selectedLaserModel;
        public VLineLaserModel SelectedLaserModel
        {
            get => _selectedLaserModel;
            set
            {
                if (value?.FBrand == "SmartRay")
                {
                    JobPathVis = true;
                }
                else
                {
                    JobPathVis = false;
                }
                SetProperty(ref _selectedLaserModel, value);
            }
        }

        /// <summary>
        ///线扫相机列表
        /// </summary>
        private ObservableCollection<VLineLaserModel> _vLaserModels;
        public ObservableCollection<VLineLaserModel> VLaserModels
        {
            get { return _vLaserModels; }
            set { SetProperty(ref _vLaserModels, value); }
        }

        /// <summary>
        /// 真实轴列表
        /// </summary>
        public List<VAxis> Axises { get; set; }

        /// <summary>
        /// 当前真实轴
        /// </summary>
        private VAxis _currentaxis;

        public VAxis CurrentAxis
        {
            get { return _currentaxis; }
            set { SetProperty(ref _currentaxis, value); }
        }

        /// <summary>
        /// 真实轴列表
        /// </summary>
        public List<VIO> TriggerDOs { get; set; }

        /// <summary>
        /// 当前真实轴
        /// </summary>
        private VIO _currentaxtriggerdos;

        public VIO CurrentTriggerDO
        {
            get { return _currentaxtriggerdos; }
            set { SetProperty(ref _currentaxtriggerdos, value); }
        }

        /// <summary>
        /// 当前轴位置
        /// </summary>
        private double _axisPos;
        public double AxisPos
        {
            get { return _axisPos; }
            set { SetProperty(ref _axisPos, value); }
        }

        /// <summary>
        /// 当前轴位置
        /// </summary>
        private double _movePos;
        public double MovePos
        {
            get { return _movePos; }
            set { SetProperty(ref _movePos, value); }
        }

        /// <summary>
        ///线扫相机开关触发模式
        /// </summary>
        private List<string> startTriggerModes;
        public List<string> StartTriggerModes
        {
            get { return startTriggerModes; }
            set { SetProperty(ref startTriggerModes, value); }
        }

        /// <summary>
        ///数据显示模式
        /// </summary>
        private List<string> dataShowModes;
        public List<string> DataShowModes
        {
            get { return dataShowModes; }
            set { SetProperty(ref dataShowModes, value); }
        }

        /// <summary>
        /// 线扫当前开始触发模式
        /// </summary>
        private string curStartTriMode;
        public string CurStartTriMode
        {
            get { return curStartTriMode; }
            set
            {
                SetProperty(ref curStartTriMode, value);
            }
        }

        /// <summary>
        ///编码器输出类型（差分和单端）
        /// </summary>
        private List<string> encoderOut;
        public List<string> EncoderOut
        {
            get { return encoderOut; }
            set { SetProperty(ref encoderOut, value); }
        }

        /// <summary>
        /// 编码器输出类型（差分或单端）
        /// </summary>
        private string curEncoderOut = "差分";
        public string CurEncoderOut
        {
            get { return curEncoderOut; }
            set { SetProperty(ref curEncoderOut, value); }
        }


        private bool encoderOutVis = false;
        /// <summary>
        /// 编码器类型接入类型是否显示
        /// </summary>
        public bool EncoderOutVis
        {
            get { return encoderOutVis; }
            set { SetProperty(ref encoderOutVis, value); }
        }

        /// <summary>
        ///线扫相机数据触发模式
        /// </summary>
        private List<string> dataTriggerModes;
        public List<string> DataTriggerModes
        {
            get { return dataTriggerModes; }
            set { SetProperty(ref dataTriggerModes, value); }
        }

        /// <summary>
        ///线扫相机开始触发DO信号
        /// </summary>
        private List<string> startTriggerDO;
        public List<string> StartTriggerDO
        {
            get { return startTriggerDO; }
            set { SetProperty(ref startTriggerDO, value); }
        }

        /// <summary>
        /// 线扫当前数据触发模式
        /// </summary>
        private string curDataTriMode;
        public string CurDataTriMode
        {
            get { return curDataTriMode; }
            set
            {
                if (value == "Encode" && SelectedLaserModel?.FBrand == "SSZN")
                {
                    EncoderOutVis = true;
                }
                else
                {
                    EncoderOutVis = false;
                }
                SetProperty(ref curDataTriMode, value);
            }
        }

        /// <summary>
        ///线扫相机所有JOB名称
        /// </summary>
        private List<string> laserJobs;
        public List<string> LaserJobs
        {
            get { return laserJobs; }
            set { SetProperty(ref laserJobs, value); }
        }

        /// <summary>
        /// 线扫当前触发模式
        /// </summary>
        private string curJob;
        public string CurJob
        {
            get { return curJob; }
            set { SetProperty(ref curJob, value); }
        }

        /// <summary>
        /// 曝光值
        /// </summary>
        private double exposure = 100;
        public double Exposure
        {
            get { return exposure; }
            set { SetProperty(ref exposure, value); }
        }

        /// <summary>
        /// 帧率
        /// </summary>
        private int frameRate = 1000;
        public int FrameRate
        {
            get { return frameRate; }
            set { SetProperty(ref frameRate, value); }
        }

        /// <summary>
        /// 扫描行数
        /// </summary>
        private int rowNum = 1000;
        public int RowNum
        {
            get { return rowNum; }
            set { SetProperty(ref rowNum, value); }
        }

        /// <summary>
        /// 轮廓点集合
        /// </summary>
        private PointCollection laserProfile;
        public PointCollection LaserProfile
        {
            get { return laserProfile; }
            set { SetProperty(ref laserProfile, value); }
        }


        #endregion

        #region 命令

        /// <summary>
        ///选择线扫对象
        /// </summary>
        private DelegateCommand<object> showSelectedCommand;
        public DelegateCommand<object> ShowSelectedCommand => showSelectedCommand ?? (showSelectedCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj?.ToString() == "2.5D深度图")
            {
                laserDataMode = LaserDataMode.PointCloud;
            }
            else if (obj?.ToString() == "2D轮廓图")
            {
                laserDataMode = LaserDataMode.Profile;
            }
        }));

        /// <summary>
        /// 移除线扫对象
        /// </summary>
        private DelegateCommand<VLineLaserModel> removeCommand;
        public DelegateCommand<VLineLaserModel> RemoveCommand => removeCommand ?? (removeCommand = new DelegateCommand<VLineLaserModel>((item) =>
        {
            dialogService.ShowConfirm($"确认删除相机:{item.Name}", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    deviceEngine.ReomoveVirtual(item.ID);
                    var model = VLaserModels.FirstOrDefault(u => u.ID == item.ID);
                    if (model != null)
                    {
                        if (model.ID == SelectedLaserModel.ID)
                        {
                            SelectedLaserModel = null;
                        }
                        VLaserModels.Remove(model);
                    }
                }
            });
        }));

        /// <summary>
        /// 移除线扫对象
        /// </summary>
        private DelegateCommand<VLineLaserModel> selectedCommand;
        public DelegateCommand<VLineLaserModel> SelectedCommand => selectedCommand ?? (selectedCommand = new DelegateCommand<VLineLaserModel>((item) =>
        {
            SelectedLaserModel = item;
            if (_vLineLaser != null)
            {

            }
            if (item != null)
            {
                var device = deviceEngine.GetVirtualByID(item.ID);
                _vLineLaser = device as VLineLaser;
                if (_vLineLaser != null)
                {
                    //获得当前的Job
                    LaserJobs = _vLineLaser.GetJobs();
                    JobPath =  (_vLineLaser.GetDevice() as ILineLaser).CurJobID;
                }
            }
            else
            {
                PropertyObj = new object();
            }
        }));

        /// <summary>
        /// 设置激光
        /// </summary>
        private DelegateCommand connectLaserCommand;
        public DelegateCommand ConnectLaserCommand => connectLaserCommand ?? (connectLaserCommand = new DelegateCommand(() =>
        {
            //开启数据流
            if (_vLineLaser != null)
            {
                (_vLineLaser.GetDevice() as ILineLaser).LaserStart();
            }
        }));

        /// <summary>
        /// 设置激光
        /// </summary>
        private DelegateCommand setLaserCommand;
        public DelegateCommand SetLaserCommand => setLaserCommand ?? (setLaserCommand = new DelegateCommand(() =>
        {
            if (_vLineLaser != null && SelectedLaserModel != null)
            {
                //设置JOB参数
                _vLineLaser.SetJob(CurJob);
                //开始触发模式
                (_vLineLaser.GetDevice() as ILineLaser).LaserSetPamas(LaserPamas.StartTriggerMode, (LaserStartTrigger)Enum.Parse(typeof(LaserStartTrigger), CurStartTriMode));
                //数据触发模式
                (_vLineLaser.GetDevice() as ILineLaser).LaserSetPamas(LaserPamas.DataTriggerMode, (LaserDataTrigger)Enum.Parse(typeof(LaserDataTrigger), CurDataTriMode));
                //采集行数
                (_vLineLaser.GetDevice() as ILineLaser).LaserSetPamas(LaserPamas.RowNum, RowNum);
                //曝光值
                (_vLineLaser.GetDevice() as ILineLaser).LaserSetPamas(LaserPamas.Exposure, Exposure);

            }
        }));

        /// <summary>
        /// 开始激光取流
        /// </summary>
        private DelegateCommand openLaserCommand;
        public DelegateCommand OpenLaserCommand => openLaserCommand ?? (openLaserCommand = new DelegateCommand(() =>
        {
            if (_vLineLaser != null)
            {
                (_vLineLaser.GetDevice() as ILineLaser).ScanFinishEvent -= LineLaserDataShow;
                (_vLineLaser.GetDevice() as ILineLaser).ScanFinishEvent += LineLaserDataShow;
                //开启一次软触发
                if (CurStartTriMode == "SoftWare")
                {
                    (_vLineLaser.GetDevice() as ILineLaser).SoftTrigger();
                }
            }
        }));

        /// <summary>
        /// 停止当前线扫
        /// </summary>
        private DelegateCommand stopLaserCommand;
        public DelegateCommand StopLaserCommand => stopLaserCommand ?? (stopLaserCommand = new DelegateCommand(() =>
        {
            if (_vLineLaser != null)
            {

                (_vLineLaser.GetDevice() as ILineLaser).ScanFinishEvent -= LineLaserDataShow;
                //关闭线扫
                (_vLineLaser.GetDevice() as ILineLaser).LaserStop();
                _vLineLaser.GetDevice().Close();
            }
        }));

        /// <summary>
        /// 停止当前线扫
        /// </summary>
        private DelegateCommand setTriggerCommand;
        public DelegateCommand SetTriggerCommand => setTriggerCommand ?? (setTriggerCommand = new DelegateCommand(() =>
        {
            if (_vLineLaser != null)
            {
                //设置线扫触发DO
                (_vLineLaser.GetDevice() as ILineLaser).LaserSetSpPama("StartTriggerIO", CurrentTriggerDO);
            }
        }));

        /// <summary>
        /// 加载离线数据
        /// </summary>
        private DelegateCommand selectFileCommand;
        public DelegateCommand SelectFileCommand => selectFileCommand ?? (selectFileCommand = new DelegateCommand(() =>
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = "(*.ply,*.txt)|*.ply;*.txt|All files(*.*)|*.*";
            if (SelectedLaserModel != null)
            {
                SelectedLaserModel.PointCloudPath = String.Empty;
                if (openFile.ShowDialog() == true)
                {
                    //获取点云及文件名
                    SelectedLaserModel.PointCloudPath = openFile.FileName;
                }
            }
            else
            {
                dialogService.ShowInfoTip("请先加载并选中对应虚拟线扫设备！");
            }
        }));


        /// <summary>
        /// 加载离线数据
        /// </summary>
        private DelegateCommand _selectJobPathCommand;
        public DelegateCommand SelectJobPathCommand => _selectJobPathCommand ?? (_selectJobPathCommand = new DelegateCommand(() =>
        {
            if (SelectedLaserModel != null)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                JobPath = string.Empty;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    JobPath = dialog.FileName;
                }
                (_vLineLaser.GetDevice() as ILineLaser).LoadConfig(JobPath);
                LaserJobs = (_vLineLaser.GetDevice() as ILineLaser).GetJobs();
            }
            else
            {
                dialogService.ShowInfoTip("请先加载并选中对应虚拟线扫设备！");
            }

        }));

        /// <summary>
        /// 轴改变
        /// </summary>
        private DelegateCommand axisChangedCommand;
        public DelegateCommand AxisChangedCommand => axisChangedCommand ?? (axisChangedCommand = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                UpdatePos();
            }
        }));

        /// <summary>
        /// 绝对运动
        /// </summary>
        private DelegateCommand _moveAbsCommand;
        public DelegateCommand MoveAbsCommand => _moveAbsCommand ?? (_moveAbsCommand = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                CurrentAxis.MoveAbs(MovePos);
                UpdatePos();
            }
        }));

        /// <summary>
        /// 运动
        /// </summary>
        private DelegateCommand _moveRelCommand;
        public DelegateCommand MoveRelCommand => _moveRelCommand ?? (_moveRelCommand = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                CurrentAxis.MoveRel(MovePos);
                UpdatePos();
            }
        }));

        /// <summary>
        /// 回零
        /// </summary>
        private DelegateCommand _homeCommand;
        public DelegateCommand HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                CurrentAxis.Home();
                UpdatePos();
            }
        }));



        #region JOG运动模式
        /// <summary>
        /// JOG点动
        /// </summary>
        private DelegateCommand _jogCommandAdd;
        public DelegateCommand JogCommandAdd => _jogCommandAdd ?? (_jogCommandAdd = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                //速度暂时写死
                CurrentAxis.Jog(true, 100);
                UpdatePos();
            }
        }));

        /// <summary>
        /// JOG点动
        /// </summary>
        private DelegateCommand _jogCommandSub;
        public DelegateCommand JogCommandSub => _jogCommandSub ?? (_jogCommandSub = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                CurrentAxis.Jog(false, 100);
                UpdatePos();
            }
        }));

        /// <summary>
        /// 停止运动
        /// </summary>
        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                CurrentAxis.Stop();
                UpdatePos();
                // 防止定位会造成卡住的问题
                tokenS?.Cancel();
            }
        }));

        #endregion

        /// <summary>
        /// 清除报警
        /// </summary>
        private DelegateCommand _clearErrorCommand;
        public DelegateCommand ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand(() =>
        {
            if (CurrentAxis != null)
            {
                // 显示对象
                CurrentAxis.ResetStatus();
                UpdatePos();
            }
        }));


        #endregion

        #region 方法

        //数据回调展示
        private void LineLaserDataShow(LineLaser lineLaser)
        {
            //集成3D和2D显示
            if (lineLaser.ZDataType == Common.DataStruct.Enums.DataType.Int)
            {
                _IHeightProfile = new int[lineLaser.Column * lineLaser.Row];
                Marshal.Copy(lineLaser.ZPointer, _IHeightProfile, 0, lineLaser.Column * lineLaser.Row);
            }
            else if (lineLaser.ZDataType == Common.DataStruct.Enums.DataType.Short)
            {
                _SHeightProfile = new short[lineLaser.Column * lineLaser.Row];
                Marshal.Copy(lineLaser.ZPointer, _SHeightProfile, 0, lineLaser.Column * lineLaser.Row);
            }

            //绘制高度图
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (laserDataMode == LaserDataMode.PointCloud)
                {
                    //点云模式
                    if (lineLaser.ZDataType == Common.DataStruct.Enums.DataType.Int)
                    {
                        HeightImage = CreateHeightImage(_IHeightProfile, 20, -20, lineLaser.Resolution, 255, lineLaser.Column, lineLaser.Row, 1, 1);
                    }
                    else if (lineLaser.ZDataType == Common.DataStruct.Enums.DataType.Short)
                    {
                        HeightImage = CreateHeightImage(_SHeightProfile, 20, -20, lineLaser.Resolution, 255, lineLaser.Column, lineLaser.Row, 1, 1);
                    }
                    
                }
                else
                {
                    //轮廓模式
                    //ProfileImage = CreateProfileImage(_IHeightProfile, lineLaser.Resolution, lineLaser.Column, lineLaser.Row,);
                }
            }));
        }

        /// <summary>
        /// 添加设备
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowVLineLaserDialog(r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    //应该先判断当前设备是否存在
                    if (r.Parameters.TryGetValue<VLineLaser>("VLineLaser", out var vL))
                    {
                        deviceEngine.AddVirtual(vL);
                        LoadDevices();
                    }
                }
            });
        }

        private CancellationTokenSource tokenS;

        /// <summary>
        /// 轴位置更新
        /// </summary>
        /// <param name="isNeedCheckMotionDone"></param>
        private void UpdatePos(bool isNeedCheckMotionDone = true)
        {
            if (tokenS != null)
            {
                tokenS.Cancel();
            }
            tokenS = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (true)
                {
                    if (tokenS.IsCancellationRequested)
                    {
                        AxisPos = CurrentAxis.GetCurrentPos();
                        break;
                    }
                    AxisPos = CurrentAxis.GetCurrentPos();
                    Thread.Sleep(50);
                }
            }, tokenS.Token);
            // JOG 模式不需要等待CheckMotionDone
            if (isNeedCheckMotionDone)
            {
                Task.Run(() =>
                {
                    CurrentAxis.CheckMotionDone();
                    tokenS?.Cancel();
                });
            }
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices()
        {
            //线扫设备
            var devices = deviceEngine.GetDevices(typeof(VLineLaser));
            VLaserModels = new ObservableCollection<VLineLaserModel>();
            foreach (var device in devices)
            {
                var vLineLaser = device as VLineLaser;
                var vLineLaserModel = new VLineLaserModel(vLineLaser);
                vLineLaserModel.LaserName = vLineLaser.GetDevice().Name;
                VLaserModels.Add(vLineLaserModel);
            }
        }

        /// <summary>
        /// 创建高度图
        /// </summary>
        /// <param name="_BatchData">高度数据</param>
        /// <param name="max_height">最大高度值（激光有效高度范围上限）</param>
        /// <param name="min_height">最小高度值（激光有效高度范围下限）</param>
        /// <param name="_ColorMax"></param>
        /// <param name="img_w"></param>
        /// <param name="img_h"></param>
        /// <param name="_xscale"></param>
        /// <param name="_yscale"></param>
        private ImageSource CreateHeightImage<T>(T[] _BatchData, double max_height, double min_height, double zResolution, int _ColorMax, int imgWidth, int imgHeight, double _xscale, double _yscale)
        {
            if (imgWidth < 800)
                _xscale = 1;
            //数据转换
            double fscale = Convert.ToDouble(_ColorMax) / (max_height - min_height);   //颜色区间与高度区间比例
            int imgH = Convert.ToInt32(imgHeight * _yscale);
            int imgW = Convert.ToInt32(imgWidth * _xscale);
            int TmpX = 0;
            int Tmppx = 0;
            int Tmpys = Convert.ToInt32(1.0 / _yscale);
            int Tmpxs = Convert.ToInt32(1.0 / _xscale);

            int TT = (imgW * 8 + 31) / 32;   //图像四字节对齐
            TT = TT * 4;

            int dwDataSize = TT * imgH;
            byte[] BatchImage = new byte[dwDataSize];

            for (int i = 0; i < imgH; i++)
            {
                TmpX = i * Tmpys * imgWidth;
                Tmppx = i * TT;
                for (int j = 0; j < imgW; j++)
                {
                    double Tmp = Convert.ToDouble(_BatchData[TmpX + j * Tmpxs]) * zResolution;
                    if (Tmp < min_height)
                        BatchImage[Tmppx + j] = 0;
                    else if (Tmp > max_height)
                        BatchImage[Tmppx + j] = 0xff;
                    else
                    {
                        byte tmpt = Convert.ToByte((Tmp - min_height) * fscale);
                        BatchImage[Tmppx + j] = tmpt;
                    }
                }
            }

            Bitmap tmpBitmap = new Bitmap(imgW, imgH, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            // 256 调色板
            ColorPalette monoPalette = tmpBitmap.Palette;
            System.Drawing.Color[] entries = monoPalette.Entries;
            for (int i = 0; i < 256; i++)
                entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            tmpBitmap.Palette = monoPalette;
            Rectangle rect = new Rectangle(0, 0, tmpBitmap.Width, tmpBitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = tmpBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            int bytes = TT * tmpBitmap.Height;
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(BatchImage, 0, ptr, bytes);
            tmpBitmap.UnlockBits(bmpData);
            return BitmapToImageSource(tmpBitmap);
        }

        /// <summary>
        /// 生成轮廓图
        /// </summary>
        /// <param name="profData"></param>
        /// <param name="dataWidth"></param>
        /// <param name="zResolution"></param>
        /// <param name="heightRange"></param>
        /// <param name="imgWidth"></param>
        /// <param name="imgHeight"></param>
        /// <returns></returns>
        private ImageSource CreateProfileImage(int[] profData, int dataWidth, double zResolution, double heightRange, int imgWidth, int imgHeight)
        {
            Bitmap tmpBmp = new Bitmap(imgWidth, imgHeight);
            Graphics g = Graphics.FromImage(tmpBmp);

            g.Clear(System.Drawing.Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            double x_scale = Convert.ToDouble(imgWidth) / dataWidth;
            //y方向比例
            double y_scale = Convert.ToDouble(imgHeight * 0.5) / heightRange;

            PointF currentPoint = new PointF(-100, -100);
            PointF currentPointNext = new PointF(-100, -100);
            for (int i = 1; i < dataWidth; i++)
            {
                double Tmpy = profData[i] * zResolution;
                double Tmpyy = profData[i - 1] * zResolution;
                if (Tmpy > (-100) && Tmpyy > (-100))
                {
                    currentPoint.X = Convert.ToSingle(i * x_scale - 10);
                    currentPoint.Y = Convert.ToSingle(imgHeight - (Tmpyy + heightRange) * y_scale);

                    currentPointNext.X = Convert.ToSingle((i - 1) * x_scale - 10);
                    currentPointNext.Y = Convert.ToSingle(imgHeight - (Tmpyy + heightRange) * y_scale);
                    g.DrawLine(new System.Drawing.Pen(System.Drawing.Color.Red, 2), currentPointNext, currentPoint);
                }
            }

            // Bitmap转为ImageSource
            var stream = new MemoryStream();
            tmpBmp.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        /// <summary>
        /// 格式转换
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private ImageSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            ImageSource result =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
            return result;
        }

        #endregion
    }
}