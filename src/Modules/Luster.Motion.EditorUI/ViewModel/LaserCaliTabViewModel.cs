using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Service;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.ComponentModel;

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 激光标定 Tab 的 ViewModel（TES-141 VM + 接线阶段）。
    /// 对应 XAML：Views/LaserCaliTabView.xaml（TES-140 已 done，本 issue 不改 XAML）。
    /// 职责：块A 激光↔Z 轴两点定标参数 + 块B 实时激光值 + 块C 6 操作按钮 + 块D 激光/相机示教位置。
    /// Service 接线：P2-B IFiveAxisCalibrationService.LaserCalibrate（纯计算，RefreshPoint1/2 已真实绑定）。
    /// 设备接线：4 个设备相关命令（OpenLaser/ToggleRealtimeRead/RefreshLaserPosi/RefreshCameraPosi）为 stub，
    ///   待 @架构师 定义 ILineLaser 激光设备 + 运动位置读取(IRobot/IMotionEngine) 接口契约后绑定真实实现。
    /// 数据 owner：私有 _result（LaserCaliResult）；TODO 待父级 FiveAxisCaliProfile 持有后接入共享实例。
    /// 注：基类 MotionVM(→AuthViewModelBase) 的 OnPropertyChanged 接收 PropertyChangedEventArgs（非 Prism 标准 string），
    ///   故代理到 _result 的属性用手动 NotifyProperty 触发变更通知。
    /// </summary>
    public class LaserCaliTabViewModel : MotionPageVM
    {
        private readonly IFiveAxisCalibrationService _caliService;

        // 激光标定结果数据 owner（当前 issue 用 new；TODO 待父级 FiveAxisCaliProfile 持有后接入共享实例）
        private readonly LaserCaliResult _result = new LaserCaliResult();

        // 块B：实时激光值（本地字段，设备读取接口就位后由轮询填充）
        private double _realtimeLaserValue;

        // 命令状态字段
        private bool _isLaserOpen;
        private bool _isRealtimeReading;

        public LaserCaliTabViewModel(ICommonBus commonBus, IFiveAxisCalibrationService caliService)
            : base(commonBus)
        {
            _caliService = caliService;
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
        }

        /// <summary>
        /// 代理到 _result 的属性触发变更通知（基类 OnPropertyChanged 接收 PropertyChangedEventArgs）。
        /// </summary>
        private void NotifyProperty(string propertyName)
            => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        #region 块A：激光↔Z 轴映射标定参数（代理到 _result，可编辑）

        /// <summary>激光标准值（代理到 _result.LaserStandard）</summary>
        public double LaserStandard
        {
            get => _result.LaserStandard;
            set
            {
                if (_result.LaserStandard != value)
                {
                    _result.LaserStandard = value;
                    NotifyProperty(nameof(LaserStandard));
                }
            }
        }

        /// <summary>
        /// 激光↔Z 轴线性映射（代理到 _result.LaserMap，只读暴露对象）。
        /// XAML 路径绑定 LaserMap.Map1.DirectValue/Map1.UnitValue/Map2.DirectValue/Map2.UnitValue（TwoWay TextBox 直接改对象字段）。
        /// VM 程序化刷新值后调 NotifyProperty(nameof(LaserMap)) 通知。
        /// </summary>
        public LinearConverter LaserMap => _result.LaserMap;

        #endregion

        #region 块B：实时激光值（只读）

        /// <summary>实时激光值（本地字段，设备读取接口就位后由轮询填充）</summary>
        public double RealtimeLaserValue
        {
            get => _realtimeLaserValue;
            set => SetProperty(ref _realtimeLaserValue, value);
        }

        #endregion

        #region 块D：激光/相机示教位置（代理到 _result）

        /// <summary>激光示教位置（代理到 _result.LaserPosi）</summary>
        public PositionXYZ LaserPosi
        {
            get => _result.LaserPosi;
            set
            {
                if (!ReferenceEquals(_result.LaserPosi, value))
                {
                    _result.LaserPosi = value;
                    NotifyProperty(nameof(LaserPosi));
                }
            }
        }

        /// <summary>相机示教位置（代理到 _result.CameraPosi）</summary>
        public PositionXYZ CameraPosi
        {
            get => _result.CameraPosi;
            set
            {
                if (!ReferenceEquals(_result.CameraPosi, value))
                {
                    _result.CameraPosi = value;
                    NotifyProperty(nameof(CameraPosi));
                }
            }
        }

        #endregion

        #region 命令状态属性（即使 XAML 未绑也保留，命令逻辑用）

        /// <summary>激光是否已打开（OpenLaserCommand 翻转，设备 Open stub）</summary>
        public bool IsLaserOpen
        {
            get => _isLaserOpen;
            set => SetProperty(ref _isLaserOpen, value);
        }

        /// <summary>是否正在实时读取（ToggleRealtimeReadCommand 翻转，设备轮询 stub）</summary>
        public bool IsRealtimeReading
        {
            get => _isRealtimeReading;
            set => SetProperty(ref _isRealtimeReading, value);
        }

        #endregion

        #region 块C：操作命令

        private DelegateCommand _refreshPoint1Command;
        /// <summary>更新点位1：触发 LaserCalibrate 重新算 LaserMap</summary>
        public DelegateCommand RefreshPoint1Command => _refreshPoint1Command
            ?? (_refreshPoint1Command = new DelegateCommand(RefreshPoint1));

        private DelegateCommand _refreshPoint2Command;
        /// <summary>更新点位2：触发 LaserCalibrate 重新算 LaserMap</summary>
        public DelegateCommand RefreshPoint2Command => _refreshPoint2Command
            ?? (_refreshPoint2Command = new DelegateCommand(RefreshPoint2));

        private DelegateCommand _openLaserCommand;
        /// <summary>打开/关闭激光设备（设备 Open stub）</summary>
        public DelegateCommand OpenLaserCommand => _openLaserCommand
            ?? (_openLaserCommand = new DelegateCommand(OpenLaser));

        private DelegateCommand _toggleRealtimeReadCommand;
        /// <summary>切换实时读取（设备轮询 stub）</summary>
        public DelegateCommand ToggleRealtimeReadCommand => _toggleRealtimeReadCommand
            ?? (_toggleRealtimeReadCommand = new DelegateCommand(ToggleRealtimeRead));

        private DelegateCommand _refreshLaserPosiCommand;
        /// <summary>更新激光示教位置（设备位置读取 stub）</summary>
        public DelegateCommand RefreshLaserPosiCommand => _refreshLaserPosiCommand
            ?? (_refreshLaserPosiCommand = new DelegateCommand(RefreshLaserPosi));

        private DelegateCommand _refreshCameraPosiCommand;
        /// <summary>更新相机示教位置（设备位置读取 stub）</summary>
        public DelegateCommand RefreshCameraPosiCommand => _refreshCameraPosiCommand
            ?? (_refreshCameraPosiCommand = new DelegateCommand(RefreshCameraPosi));

        /// <summary>
        /// 更新点位1：理想应先从激光设备读当前 DirectValue、从运动接口读当前 Z（UnitValue）写入 Map1，再触发标定。
        /// 当前激光设备 + 运动位置读取接口契约待 @架构师 定义，暂用 UI 已输入的 Map1/Map2 字段值直接调 LaserCalibrate。
        /// </summary>
        private void RefreshPoint1()
        {
            // TODO(TES-144): 待 @架构师 定义 ILineLaser 激光设备 + 运动位置读取(IRobot/IMotionEngine)接口契约后，
            // 先从设备读当前激光值写入 _result.LaserMap.Map1.DirectValue，读当前 Z 写入 _result.LaserMap.Map1.UnitValue。
            // P2-B IFiveAxisCalibrationService 仅含 LaserCalibrate 纯计算，不含设备读取。
            _caliService.LaserCalibrate(
                _result,
                _result.LaserMap.Map1.DirectValue,  // laser1: 点位1激光测量值
                _result.LaserMap.Map1.UnitValue,    // z1: 点位1Z轴位置
                _result.LaserMap.Map2.DirectValue,  // laser2: 点位2激光测量值
                _result.LaserMap.Map2.UnitValue,     // z2: 点位2Z轴位置
                _result.LaserStandard,
                _result.LaserPosi,
                _result.CameraPosi);
            // 标定后 LaserMap 内部已更新，通知 UI 刷新
            NotifyProperty(nameof(LaserMap));
        }

        /// <summary>
        /// 更新点位2：同 RefreshPoint1，理想应先刷新 Map2 的设备读数再触发标定。
        /// 设备接口契约待定义，暂用当前字段值直接调 LaserCalibrate。
        /// </summary>
        private void RefreshPoint2()
        {
            // TODO(TES-144): 待 @架构师 定义 ILineLaser 激光设备 + 运动位置读取(IRobot/IMotionEngine)接口契约后，
            // 先从设备读当前激光值写入 _result.LaserMap.Map2.DirectValue，读当前 Z 写入 _result.LaserMap.Map2.UnitValue。
            // P2-B IFiveAxisCalibrationService 仅含 LaserCalibrate 纯计算，不含设备读取。
            _caliService.LaserCalibrate(
                _result,
                _result.LaserMap.Map1.DirectValue,
                _result.LaserMap.Map1.UnitValue,
                _result.LaserMap.Map2.DirectValue,
                _result.LaserMap.Map2.UnitValue,
                _result.LaserStandard,
                _result.LaserPosi,
                _result.CameraPosi);
            NotifyProperty(nameof(LaserMap));
        }

        /// <summary>
        /// 打开/关闭激光设备。设备 Open 真实实现待 ILineLaser 接口契约定义。
        /// </summary>
        private void OpenLaser()
        {
            // TODO(TES-144): 待 @架构师 定义 ILineLaser 激光设备接口契约后绑定真实 Open 实现；
            // P2-B IFiveAxisCalibrationService 仅含 LaserCalibrate 纯计算，不含设备控制。
            IsLaserOpen = !IsLaserOpen;
        }

        /// <summary>
        /// 切换实时激光值读取。设备轮询真实实现待 ILineLaser 接口契约定义。
        /// </summary>
        private void ToggleRealtimeRead()
        {
            // TODO(TES-144): 待 @架构师 定义 ILineLaser 激光设备接口契约后绑定真实轮询实现；
            // P2-B IFiveAxisCalibrationService 仅含 LaserCalibrate 纯计算，不含设备轮询。
            IsRealtimeReading = !IsRealtimeReading;
            // TODO: IsRealtimeReading=true 时启动轮询定时器读激光值更新 RealtimeLaserValue；=false 时停止轮询。
        }

        /// <summary>
        /// 更新激光示教位置：从运动接口读当前激光位置写入 _result.LaserPosi。
        /// 运动位置读取接口契约待 @架构师 定义，暂抛 NotImplementedException 诚实表达未实现。
        /// </summary>
        private void RefreshLaserPosi()
        {
            // TODO(TES-144): 待 @架构师 定义运动位置读取(IRobot/IMotionEngine)接口契约后，
            // 读当前激光位置写入 _result.LaserPosi 并 NotifyProperty(nameof(LaserPosi))。
            // P2-B IFiveAxisCalibrationService 仅含 LaserCalibrate 纯计算，不含运动位置读取。
            throw new NotImplementedException("待设备位置读取接口契约由架构师定义");
        }

        /// <summary>
        /// 更新相机示教位置：从运动接口读当前相机位置写入 _result.CameraPosi。
        /// 运动位置读取接口契约待 @架构师 定义，暂抛 NotImplementedException 诚实表达未实现。
        /// </summary>
        private void RefreshCameraPosi()
        {
            // TODO(TES-144): 待 @架构师 定义运动位置读取(IRobot/IMotionEngine)接口契约后，
            // 读当前相机位置写入 _result.CameraPosi 并 NotifyProperty(nameof(CameraPosi))。
            // P2-B IFiveAxisCalibrationService 仅含 LaserCalibrate 纯计算，不含运动位置读取。
            throw new NotImplementedException("待设备位置读取接口契约由架构师定义");
        }

        #endregion
    }
}
