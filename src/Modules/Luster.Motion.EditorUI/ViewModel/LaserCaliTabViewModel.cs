using Luster.Common.DataStruct.DataModels; // LineLaser(点云结构)
using Luster.Common.DataStruct.Enums; // LogType
using Luster.Motion.CommonUI; // ICommonBus
using Luster.Motion.DataStruct; // VLineLaser, IDeviceEngine
using Luster.Motion.DataStruct.DataModels; // VAxis
using Luster.Motion.DataStruct.Real; // ILineLaser
using Luster.Motion.FiveAxis.Data.Calibration; // LaserCaliResult, LinearConverter
using Luster.Motion.FiveAxis.Position; // PositionXYZ
using Luster.Motion.FiveAxis.Service; // IFiveAxisCalibrationService
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Threading;

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 激光标定 Tab 视图模型(TES-163)。
    /// 匹配 TES-140 留下的 LaserCaliTabView.xaml 绑定契约:
    /// 激光↔Z 轴两点定标(LaserStandard/LaserMap.Map1|Map2.DirectValue|UnitValue)+ 激光/相机示教位置
    /// (LaserPosi/CameraPosi)+ 实时读取(RealtimeLaserValue)+ 各点位更新按钮(6 个 DelegateCommand)。
    /// VM 只做设备取数 + 写回模型,不调 LaserCalibrate(标定执行属其它 issue)。
    /// 实时读时序 / 轴名映射 / 单值提取 ⚠️ 待人类现场验证。
    /// </summary>
    public class LaserCaliTabViewModel : BindableBase, IDisposable
    {
        private readonly ICommonBus _commonBus;
        // 标定 Service 注入保留,供后续标定执行 issue 调 LaserCalibrate 使用(本 issue 不调)。
        private readonly IFiveAxisCalibrationService _caliService;
        private readonly IDeviceEngine _deviceEngine;

        /// <summary>标定结果模型(VM 暴露其子属性供 XAML 绑定,落盘由上层负责)</summary>
        private readonly LaserCaliResult _model = new LaserCaliResult();

        // ===== 实时读取状态 =====
        private bool _isRealtimeReading;
        private Timer _softTriggerTimer;
        private ILineLaser _lineLaser;

        public LaserCaliTabViewModel(ICommonBus commonBus, IFiveAxisCalibrationService caliService, IDeviceEngine deviceEngine)
        {
            _commonBus = commonBus;
            _caliService = caliService;
            _deviceEngine = deviceEngine;
            // 与 _model 共享同一实例,保证 UI 编辑/命令写回同步到模型
            _laserPosi = _model.LaserPosi;
            _cameraPosi = _model.CameraPosi;
        }

        /// <summary>激光标准值(TextBox 双向绑定)</summary>
        private double _laserStandard;
        public double LaserStandard
        {
            get => _laserStandard;
            set
            {
                if (SetProperty(ref _laserStandard, value))
                    _model.LaserStandard = value;
            }
        }

        /// <summary>激光↔Z 轴线性映射(两点定标)。子属性 Map1/Map2.DirectValue/UnitValue 由 XAML 直接绑定写回。</summary>
        public LinearConverter LaserMap => _model.LaserMap;

        /// <summary>实时激光值(OneWay 只读,由 ScanFinishEvent 回调刷新)</summary>
        private string _realtimeLaserValue = "0";
        public string RealtimeLaserValue
        {
            get => _realtimeLaserValue;
            set => SetProperty(ref _realtimeLaserValue, value);
        }

        /// <summary>激光示教位置(显示 X,Y,Z)</summary>
        private PositionXYZ _laserPosi;
        public PositionXYZ LaserPosi
        {
            get => _laserPosi;
            set
            {
                if (SetProperty(ref _laserPosi, value))
                    _model.LaserPosi = value;
            }
        }

        /// <summary>相机示教位置(显示 X,Y,Z)</summary>
        private PositionXYZ _cameraPosi;
        public PositionXYZ CameraPosi
        {
            get => _cameraPosi;
            set
            {
                if (SetProperty(ref _cameraPosi, value))
                    _model.CameraPosi = value;
            }
        }

        // ===== 命令(6 个 DelegateCommand)=====

        /// <summary>打开激光</summary>
        private DelegateCommand _openLaserCommand;
        public DelegateCommand OpenLaserCommand => _openLaserCommand ?? (_openLaserCommand = new DelegateCommand(OpenLaser));

        /// <summary>切换实时读取(开/关)</summary>
        private DelegateCommand _toggleRealtimeReadCommand;
        public DelegateCommand ToggleRealtimeReadCommand => _toggleRealtimeReadCommand ?? (_toggleRealtimeReadCommand = new DelegateCommand(ToggleRealtimeRead));

        /// <summary>更新点位1:当前实时激光值→Map1.DirectValue,当前 Z 轴位置→Map1.UnitValue</summary>
        private DelegateCommand _refreshPoint1Command;
        public DelegateCommand RefreshPoint1Command => _refreshPoint1Command ?? (_refreshPoint1Command = new DelegateCommand(RefreshPoint1));

        /// <summary>更新点位2:当前实时激光值→Map2.DirectValue,当前 Z 轴位置→Map2.UnitValue</summary>
        private DelegateCommand _refreshPoint2Command;
        public DelegateCommand RefreshPoint2Command => _refreshPoint2Command ?? (_refreshPoint2Command = new DelegateCommand(RefreshPoint2));

        /// <summary>更新激光位置:取当前 X/Y/Z 轴位置→LaserPosi</summary>
        private DelegateCommand _refreshLaserPosiCommand;
        public DelegateCommand RefreshLaserPosiCommand => _refreshLaserPosiCommand ?? (_refreshLaserPosiCommand = new DelegateCommand(RefreshLaserPosi));

        /// <summary>更新相机位置:取当前 X/Y/Z 轴位置→CameraPosi</summary>
        private DelegateCommand _refreshCameraPosiCommand;
        public DelegateCommand RefreshCameraPosiCommand => _refreshCameraPosiCommand ?? (_refreshCameraPosiCommand = new DelegateCommand(RefreshCameraPosi));

        // ===== 命令实现 =====

        private void OpenLaser()
        {
            var lineLaser = ResolveLineLaser();
            if (lineLaser == null) return;
            lineLaser.LaserStart();
            _commonBus?.OnLog(LogType.Debug, "激光已打开");
        }

        private void ToggleRealtimeRead()
        {
            if (_isRealtimeReading)
            {
                StopRealtimeRead();
                _commonBus?.OnLog(LogType.Debug, "实时读取已停止");
            }
            else
            {
                StartRealtimeRead();
            }
        }

        private void StartRealtimeRead()
        {
            var lineLaser = ResolveLineLaser();
            if (lineLaser == null) return;

            _lineLaser = lineLaser;
            // 订阅扫描完成事件(参照 VLineLaserContentVM 范式:订阅底层 ILineLaser.ScanFinishEvent)
            _lineLaser.ScanFinishEvent -= OnScanFinish;
            _lineLaser.ScanFinishEvent += OnScanFinish;

            // 周期软触发刷新(参照 VLineLaserContentVM 软触发模式,实时读需周期触发)
            _softTriggerTimer?.Dispose();
            _softTriggerTimer = new Timer(_ =>
            {
                try { _lineLaser?.SoftTrigger(); }
                catch { /* 设备异常忽略,保实时读不中断 */ }
            }, null, 0, 200);

            _isRealtimeReading = true;
            _commonBus?.OnLog(LogType.Debug, "实时读取已开启");
        }

        private void StopRealtimeRead()
        {
            if (_lineLaser != null)
            {
                _lineLaser.ScanFinishEvent -= OnScanFinish;
                _lineLaser = null;
            }
            _softTriggerTimer?.Dispose();
            _softTriggerTimer = null;
            _isRealtimeReading = false;
        }

        /// <summary>
        /// 扫描完成回调:把测量值刷新到 RealtimeLaserValue。
        /// ⚠️ 待人类现场验证:线扫 LineLaser 为点云结构(Row/Column/ZPointer),实时单值激光测量值的
        /// 提取字段/pamaName 待硬件确认。当前以 LaserGetPamas() 默认返回值占位,保证 UI 实时刷新与点位更新可用。
        /// </summary>
        private void OnScanFinish(LineLaser lineLaser)
        {
            try
            {
                var pama = _lineLaser?.LaserGetPamas();
                RealtimeLaserValue = pama?.ToString() ?? "0";
            }
            catch
            {
                RealtimeLaserValue = "0";
            }
        }

        private void RefreshPoint1()
        {
            // 当前实时激光值 → Map1.DirectValue
            if (double.TryParse(RealtimeLaserValue, out var laserVal))
            {
                LaserMap.Map1.DirectValue = laserVal;
            }
            // 当前 Z 轴位置 → Map1.UnitValue
            LaserMap.Map1.UnitValue = GetAxisPos('Z');
            // LinearPointMap 为 POCO 无 INPC,手动触发 LaserMap 刷新让 TextBox 重读
            RaisePropertyChanged(nameof(LaserMap));
        }

        private void RefreshPoint2()
        {
            if (double.TryParse(RealtimeLaserValue, out var laserVal))
            {
                LaserMap.Map2.DirectValue = laserVal;
            }
            LaserMap.Map2.UnitValue = GetAxisPos('Z');
            RaisePropertyChanged(nameof(LaserMap));
        }

        private void RefreshLaserPosi()
        {
            LaserPosi = new PositionXYZ(GetAxisPos('X'), GetAxisPos('Y'), GetAxisPos('Z'));
        }

        private void RefreshCameraPosi()
        {
            CameraPosi = new PositionXYZ(GetAxisPos('X'), GetAxisPos('Y'), GetAxisPos('Z'));
        }

        // ===== 设备辅助 =====

        /// <summary>取线激光底层接口,取不到给友好提示不崩</summary>
        private ILineLaser ResolveLineLaser()
        {
            var vLaser = _deviceEngine?.GetVDevices<VLineLaser>()?.FirstOrDefault();
            if (vLaser == null)
            {
                _commonBus?.OnLog(LogType.Error, "未找到线激光设备(VLineLaser),请检查设备配置");
                return null;
            }
            var lineLaser = vLaser.GetDevice() as ILineLaser;
            if (lineLaser == null)
            {
                _commonBus?.OnLog(LogType.Error, "线激光设备未关联底层 ILineLaser");
            }
            return lineLaser;
        }

        /// <summary>
        /// 取指定轴(X/Y/Z)当前位置。
        /// TODO(待人类现场验证): 轴名映射待硬件确认。当前按轴名含 X/Y/Z 选取;匹配不到则按索引约定 X=0/Y=1/Z=2。
        /// </summary>
        private double GetAxisPos(char axisChar)
        {
            var axises = _deviceEngine?.GetVDevices<VAxis>();
            if (axises == null || axises.Count == 0) return 0;

            var key = axisChar.ToString();
            VAxis axis = axises.FirstOrDefault(a => !string.IsNullOrEmpty(a.Name)
                && a.Name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0);
            if (axis == null)
            {
                // 兜底:按索引约定(X=0/Y=1/Z=2)
                int idx = axisChar == 'X' ? 0 : axisChar == 'Y' ? 1 : 2;
                if (idx < 0 || idx >= axises.Count) return 0;
                axis = axises[idx];
            }

            try
            {
                return axis.GetCurrentPos();
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            StopRealtimeRead();
        }
    }
}
