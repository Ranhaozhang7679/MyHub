using Luster.Common.DataStruct.DataModels; // LineLaser(点云结构,OnScanFinish 参数)
using Luster.Common.DataStruct.Enums; // LogType
using Luster.Motion.CommonUI; // ICommonBus(基类 commonBus 字段类型)
using Luster.Motion.CommonUI.ViewModel; // MotionPageVM(基类)
using Luster.Motion.DataStruct; // VLineLaser, IDeviceEngine
using Luster.Motion.DataStruct.DataModels; // VAxis
using Luster.Motion.DataStruct.Real; // ILineLaser
using Luster.Motion.FiveAxis.Data.Calibration; // LaserCaliResult, LinearConverter
using Luster.Motion.FiveAxis.Position; // PositionXYZ
using Luster.Motion.FiveAxis.Service; // IFiveAxisCalibrationService
using Prism.Commands;
using Prism.Events; // IEventAggregator(RegisterEvent override)
using Prism.Regions; // NavigationContext(override OnNavigatedFrom)
using System;
using System.ComponentModel; // PropertyChangedEventArgs(NotifyProperty)
using System.Linq;
using System.Threading; // Timer
using System.Xml.Linq; // XElement(XML 往返)

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 激光标定 Tab 视图模型(TES-163 嫁接版,单一权威版本)。
    /// 嫁接:前端 MotionPageVM 脚手架(基类/构造/绑定属性代理范式)+ 全栈 6 命令真实 VAxis/ILineLaser 绑定 +
    /// Service 容器注册 + 单测,消除与 github/tes-144-laser-cali-vm stub 版并存。
    /// 匹配 TES-140 留下的 LaserCaliTabView.xaml 绑定契约:
    /// 激光↔Z 轴两点定标(LaserStandard/LaserMap.Map1|Map2.DirectValue|UnitValue)+ 激光/相机示教位置
    /// (LaserPosi/CameraPosi)+ 实时读取(RealtimeLaserValue)+ 各点位更新按钮(6 个 DelegateCommand)。
    /// VM 做设备取数 + 写回模型,并通过 ApplyCalibrateCommand 调 Service.LaserCalibrate 求解(UI/算法解耦)。
    /// 实时读时序 / 轴名映射 / 单值提取 ⚠️ 待人类现场验证。
    /// </summary>
    public class LaserCaliTabViewModel : MotionPageVM, IDisposable
    {
        private readonly IFiveAxisCalibrationService _caliService;
        private readonly IDeviceEngine _deviceEngine;

        /// <summary>五轴标定配置根模型(激光标定 XML 往返容器,_result 挂在其 LaserCali 上)</summary>
        private readonly FiveAxisCaliProfile _profile;

        /// <summary>标定结果数据 owner(代理目标,挂到 _profile.LaserCali;SaveCommand 走 profile.ExportXml 落盘)</summary>
        private readonly LaserCaliResult _result;

        /// <summary>XML 往返内存载体(TODO: 落盘路径待上层配置,当前内存往返证明序列化通,不引入文件 IO 强依赖)</summary>
        private XElement _lastSavedXml;

        /// <summary>标定执行/保存/加载的结果消息(OneWay,UI 提示用)</summary>
        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        // ===== 实时读取状态 =====
        private bool _isRealtimeReading;
        private Timer _softTriggerTimer;
        private ILineLaser _lineLaser;

        /// <summary>
        /// 构造:ICommonBus 走基类 MotionVM.commonBus 字段(基类构造需要 EventBus 非 null,
        /// 生产环境注入真实 ICommonBus;单测注入 FakeCommonBus)。IDeviceEngine 注入用于真实设备取数。
        /// _result 挂到 FiveAxisCaliProfile.LaserCali,让 XML 往返有容器(验收标准 #3)。
        /// </summary>
        public LaserCaliTabViewModel(ICommonBus commonBus, IFiveAxisCalibrationService caliService, IDeviceEngine deviceEngine)
            : base(commonBus)
        {
            _caliService = caliService;
            _deviceEngine = deviceEngine;
            _profile = new FiveAxisCaliProfile();
            _result = _profile.LaserCali;
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
        }

        /// <summary>
        /// 代理到 _result 的属性触发变更通知(基类 MotionVM→AuthViewModelBase 的 OnPropertyChanged
        /// 接收 PropertyChangedEventArgs,非 Prism 标准 string,故统一走此辅助方法)。
        /// </summary>
        private void NotifyProperty(string propertyName)
            => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        // ===== 块A:激光↔Z 轴映射标定参数(代理到 _result,可编辑)=====

        /// <summary>激光标准值(TextBox 双向绑定,代理到 _result.LaserStandard)</summary>
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
        /// 激光↔Z 轴线性映射(两点定标,代理到 _result.LaserMap,只读暴露对象)。
        /// XAML 路径绑定 LaserMap.Map1.DirectValue/Map1.UnitValue/Map2.DirectValue/Map2.UnitValue
        /// (TwoWay TextBox 直接改对象字段)。VM 程序化刷新值后调 NotifyProperty(nameof(LaserMap)) 通知。
        /// </summary>
        public LinearConverter LaserMap => _result.LaserMap;

        // ===== 块B:实时激光值(只读 OneWay,由 ScanFinishEvent 回调刷新)=====

        /// <summary>实时激光值(string,由 LaserGetPamas() 返回值占位;OneWay 只读)</summary>
        private string _realtimeLaserValue = "0";
        public string RealtimeLaserValue
        {
            get => _realtimeLaserValue;
            set => SetProperty(ref _realtimeLaserValue, value);
        }

        // ===== 块D:激光/相机示教位置(代理到 _result)=====

        /// <summary>激光示教位置(代理到 _result.LaserPosi)</summary>
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

        /// <summary>相机示教位置(代理到 _result.CameraPosi)</summary>
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

        // ===== 块C:命令(6 个 DelegateCommand)=====

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

        /// <summary>执行激光标定:调 IFiveAxisCalibrationService.LaserCalibrate 求解,失败写 Message(验收标准 #2)</summary>
        private DelegateCommand _applyCalibrateCommand;
        public DelegateCommand ApplyCalibrateCommand => _applyCalibrateCommand ?? (_applyCalibrateCommand = new DelegateCommand(ApplyCalibrate));

        /// <summary>保存标定结果:走 FiveAxisCaliProfile.ExportXml 落盘往返(验收标准 #3 XML 部分)</summary>
        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(Save));

        /// <summary>加载标定结果:走 FiveAxisCaliProfile.ParserXml 回填(验收标准 #3 XML 部分)</summary>
        private DelegateCommand _loadCommand;
        public DelegateCommand LoadCommand => _loadCommand ?? (_loadCommand = new DelegateCommand(Load));

        // ===== 命令实现(真实 VAxis/ILineLaser 绑定)=====

        private void OpenLaser()
        {
            var lineLaser = ResolveLineLaser();
            if (lineLaser == null) return;
            lineLaser.LaserStart();
            commonBus?.OnLog(LogType.Debug, "激光已打开");
        }

        private void ToggleRealtimeRead()
        {
            if (_isRealtimeReading)
            {
                StopRealtimeRead();
                commonBus?.OnLog(LogType.Debug, "实时读取已停止");
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
            commonBus?.OnLog(LogType.Debug, "实时读取已开启");
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
                _result.LaserMap.Map1.DirectValue = laserVal;
            }
            // 当前 Z 轴位置 → Map1.UnitValue
            _result.LaserMap.Map1.UnitValue = GetAxisPos('Z');
            // LinearPointMap 为 POCO 无 INPC,手动触发 LaserMap 刷新让 TextBox 重读
            NotifyProperty(nameof(LaserMap));
        }

        private void RefreshPoint2()
        {
            if (double.TryParse(RealtimeLaserValue, out var laserVal))
            {
                _result.LaserMap.Map2.DirectValue = laserVal;
            }
            _result.LaserMap.Map2.UnitValue = GetAxisPos('Z');
            NotifyProperty(nameof(LaserMap));
        }

        private void RefreshLaserPosi()
        {
            LaserPosi = new PositionXYZ(GetAxisPos('X'), GetAxisPos('Y'), GetAxisPos('Z'));
        }

        private void RefreshCameraPosi()
        {
            CameraPosi = new PositionXYZ(GetAxisPos('X'), GetAxisPos('Y'), GetAxisPos('Z'));
        }

        /// <summary>
        /// 执行激光标定:把当前两点激光读数+Z 高度+标准值+示教位置喂给 Service.LaserCalibrate,
        /// 由 Service 写回 _result(UI/算法解耦:RefreshPoint1/2 只负责采点,求解走 Service)。
        /// 成功刷新绑定属性,失败/异常写 Message。
        /// </summary>
        private void ApplyCalibrate()
        {
            try
            {
                var ok = _caliService?.LaserCalibrate(
                    _result,
                    _result.LaserMap.Map1.DirectValue,
                    _result.LaserMap.Map1.UnitValue,
                    _result.LaserMap.Map2.DirectValue,
                    _result.LaserMap.Map2.UnitValue,
                    _result.LaserStandard,
                    _result.LaserPosi,
                    _result.CameraPosi);
                if (ok ?? false)
                {
                    // Service 写回 _result 后刷新绑定,让 UI 重读 LaserStandard/LaserMap/LaserPosi/CameraPosi
                    NotifyProperty(nameof(LaserStandard));
                    NotifyProperty(nameof(LaserMap));
                    NotifyProperty(nameof(LaserPosi));
                    NotifyProperty(nameof(CameraPosi));
                    Message = "激光标定已完成";
                }
                else
                {
                    Message = "激光标定失败:Service 返回 false";
                }
            }
            catch (Exception ex)
            {
                Message = "激光标定异常:" + ex.Message;
            }
        }

        /// <summary>
        /// 保存标定结果:走 FiveAxisCaliProfile.ExportXml() 产出 XElement。
        /// TODO(落盘路径待配置): 当前用内存往返(_lastSavedXml)证明 XML 序列化通,不引入文件 IO 强依赖(单测可跑);
        /// 落盘路径确定后改写文件(上层配方路径 / Profile 管理器)。
        /// </summary>
        private void Save()
        {
            try
            {
                _lastSavedXml = _profile.ExportXml();
                Message = "标定结果已保存(内存往返,落盘路径待配置)";
            }
            catch (Exception ex)
            {
                Message = "保存异常:" + ex.Message;
            }
        }

        /// <summary>
        /// 加载标定结果:走 FiveAxisCaliProfile.ParserXml(XElement) 就地回填 _profile.LaserCali(= _result),
        /// 回填后刷新绑定属性让 UI 重读。
        /// </summary>
        private void Load()
        {
            try
            {
                if (_lastSavedXml == null)
                {
                    Message = "无可加载的标定数据(请先保存)";
                    return;
                }
                _profile.ParserXml(_lastSavedXml);
                // _result 是 _profile.LaserCali 同一引用,ParserXml 就地填充字段,刷新绑定让 UI 重读
                NotifyProperty(nameof(LaserStandard));
                NotifyProperty(nameof(LaserMap));
                NotifyProperty(nameof(LaserPosi));
                NotifyProperty(nameof(CameraPosi));
                Message = "标定结果已加载";
            }
            catch (Exception ex)
            {
                Message = "加载异常:" + ex.Message;
            }
        }

        // ===== 设备辅助 =====

        /// <summary>取线激光底层接口,取不到给友好提示不崩</summary>
        private ILineLaser ResolveLineLaser()
        {
            var vLaser = _deviceEngine?.GetVDevices<VLineLaser>()?.FirstOrDefault();
            if (vLaser == null)
            {
                commonBus?.OnLog(LogType.Error, "未找到线激光设备(VLineLaser),请检查设备配置");
                return null;
            }
            var lineLaser = vLaser.GetDevice() as ILineLaser;
            if (lineLaser == null)
            {
                commonBus?.OnLog(LogType.Error, "线激光设备未关联底层 ILineLaser");
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

        // ===== 生命周期:离页退订(嫁接新增,防 ScanFinishEvent 重复订阅泄漏)=====

        /// <summary>
        /// 离开页面时停止实时读取并退订 ScanFinishEvent,防止单例 VM 重复导航累积订阅泄漏。
        /// (MotionPageVM.IsNavigationTarget 默认 true → VM 单例复用,必须靠离页退订清理订阅。)
        /// </summary>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            StopRealtimeRead();
        }

        public void Dispose()
        {
            StopRealtimeRead();
        }
    }
}
