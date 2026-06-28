using FluentAssertions;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.EditorUI.ViewModel;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Device;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Service;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using NUnit.Framework;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// LaserCaliTabViewModel 单测(TES-163 嫁接版)。
    /// 嫁接后 VM 基类为 MotionPageVM(基类构造需要 ICommonBus.EventBus 非 null)→ 手写 FakeCommonBus
    /// (EventBus 返回真实 Prism EventAggregator,CurrentUser 返回 null)。IDeviceEngine 仍用空列表 fake。
    /// 设备交互路径(实时读时序/轴名映射/单值提取)⚠️ 待人类现场验证,此处只测 VM 纯逻辑分支与状态机。
    /// </summary>
    [TestFixture]
    public class LaserCaliTabViewModelTests
    {
        private static LaserCaliTabViewModel CreateVm(FakeDeviceEngine engine = null, FakeCalibrationService service = null)
        {
            engine = engine ?? new FakeDeviceEngine();
            service = service ?? new FakeCalibrationService();
            return new LaserCaliTabViewModel(new FakeCommonBus(), service, engine);
        }

        [Test]
        public void Constructor_WithFakes_ShouldNotThrow()
        {
            var vm = CreateVm();
            vm.Should().NotBeNull();
        }

        [Test]
        public void Commands_ShouldAllBeNonNull()
        {
            var vm = CreateVm();
            vm.OpenLaserCommand.Should().NotBeNull();
            vm.ToggleRealtimeReadCommand.Should().NotBeNull();
            vm.RefreshPoint1Command.Should().NotBeNull();
            vm.RefreshPoint2Command.Should().NotBeNull();
            vm.RefreshLaserPosiCommand.Should().NotBeNull();
            vm.RefreshCameraPosiCommand.Should().NotBeNull();
            vm.ApplyCalibrateCommand.Should().NotBeNull();
            vm.SaveCommand.Should().NotBeNull();
            vm.LoadCommand.Should().NotBeNull();
        }

        [Test]
        public void OpenLaser_WithEmptyDevices_ShouldNotThrow()
        {
            var vm = CreateVm();
            Action act = () => vm.OpenLaserCommand.Execute();
            act.Should().NotThrow();
        }

        [Test]
        public void ToggleRealtimeRead_WithEmptyDevices_ShouldNotThrow()
        {
            var vm = CreateVm();
            // 空设备列表下重复开关切换不应抛
            Action act = () =>
            {
                vm.ToggleRealtimeReadCommand.Execute(); // 开(设备空,早退)
                vm.ToggleRealtimeReadCommand.Execute(); // 关
                vm.ToggleRealtimeReadCommand.Execute(); // 再开
                vm.ToggleRealtimeReadCommand.Execute(); // 再关
            };
            act.Should().NotThrow();
        }

        [Test]
        public void RefreshCommands_WithEmptyDevices_ShouldNotThrow()
        {
            var vm = CreateVm();
            Action act = () =>
            {
                vm.RefreshPoint1Command.Execute();
                vm.RefreshPoint2Command.Execute();
                vm.RefreshLaserPosiCommand.Execute();
                vm.RefreshCameraPosiCommand.Execute();
            };
            act.Should().NotThrow();
        }

        [Test]
        public void RefreshPoint1_ShouldWriteRealtimeValueToMap1DirectValue()
        {
            var vm = CreateVm();
            vm.RealtimeLaserValue = "1.23";

            vm.RefreshPoint1Command.Execute();

            vm.LaserMap.Map1.DirectValue.Should().Be(1.23);
            // 空设备列表下 Z 轴位置 = 0
            vm.LaserMap.Map1.UnitValue.Should().Be(0);
        }

        [Test]
        public void RefreshPoint2_ShouldWriteRealtimeValueToMap2DirectValue()
        {
            var vm = CreateVm();
            vm.RealtimeLaserValue = "4.56";

            vm.RefreshPoint2Command.Execute();

            vm.LaserMap.Map2.DirectValue.Should().Be(4.56);
            vm.LaserMap.Map2.UnitValue.Should().Be(0);
        }

        [Test]
        public void RefreshPoint1_WithNonParsableRealtimeValue_ShouldNotThrowAndKeepDirectValue()
        {
            var vm = CreateVm();
            vm.RealtimeLaserValue = "abc";
            var before = vm.LaserMap.Map1.DirectValue;

            Action act = () => vm.RefreshPoint1Command.Execute();
            act.Should().NotThrow();
            // 解析失败时不写 DirectValue(保持原值)
            vm.LaserMap.Map1.DirectValue.Should().Be(before);
        }

        [Test]
        public void RefreshLaserPosi_WithEmptyDevices_ShouldSetZeroPosition()
        {
            var vm = CreateVm();

            vm.RefreshLaserPosiCommand.Execute();

            vm.LaserPosi.Should().NotBeNull();
            vm.LaserPosi.X.Should().Be(0);
            vm.LaserPosi.Y.Should().Be(0);
            vm.LaserPosi.Z.Should().Be(0);
        }

        [Test]
        public void RefreshCameraPosi_WithEmptyDevices_ShouldSetZeroPosition()
        {
            var vm = CreateVm();

            vm.RefreshCameraPosiCommand.Execute();

            vm.CameraPosi.Should().NotBeNull();
            vm.CameraPosi.X.Should().Be(0);
            vm.CameraPosi.Y.Should().Be(0);
            vm.CameraPosi.Z.Should().Be(0);
        }

        [Test]
        public void RealtimeLaserValue_Default_ShouldBeParsableZero()
        {
            var vm = CreateVm();
            vm.RealtimeLaserValue.Should().Be("0");

            vm.RefreshPoint1Command.Execute();
            vm.LaserMap.Map1.DirectValue.Should().Be(0);
        }

        [Test]
        public void LaserStandard_SetShouldRaiseAndPersist()
        {
            var vm = CreateVm();
            var changed = new List<string>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.LaserStandard = 9.9;

            vm.LaserStandard.Should().Be(9.9);
            changed.Should().Contain(nameof(LaserCaliTabViewModel.LaserStandard));
        }

        [Test]
        public void Dispose_AfterToggle_ShouldNotThrow()
        {
            var vm = CreateVm();
            vm.ToggleRealtimeReadCommand.Execute();
            Action act = () => vm.Dispose();
            act.Should().NotThrow();
        }

        /// <summary>
        /// 离页退订(嫁接新增价值):OnNavigatedFrom 调 StopRealtimeRead,防 ScanFinishEvent 重复订阅泄漏。
        /// 空设备下未实际订阅,验证离页路径不崩即可。
        /// </summary>
        [Test]
        public void OnNavigatedFrom_AfterToggle_ShouldNotThrow()
        {
            var vm = CreateVm();
            vm.ToggleRealtimeReadCommand.Execute();

            Action act = () => vm.OnNavigatedFrom(null);

            act.Should().NotThrow();
        }

        // ===== 验收标准 #2:VM 调 Service =====

        /// <summary>
        /// ApplyCalibrateCommand 应调 Service.LaserCalibrate,传入当前两点激光读数+Z 高度+标准值,
        /// 并由 Service 写回 _result(走 Service 路径,非 VM 内联)。
        /// </summary>
        [Test]
        public void ApplyCalibrateCommand_ShouldInvokeServiceWithCurrentValuesAndWriteBack()
        {
            var service = new FakeCalibrationService();
            var vm = CreateVm(service: service);
            vm.LaserStandard = 5.0;
            vm.RealtimeLaserValue = "1.1";
            vm.RefreshPoint1Command.Execute();
            vm.RealtimeLaserValue = "2.2";
            vm.RefreshPoint2Command.Execute();

            vm.ApplyCalibrateCommand.Execute();

            service.LaserCalibrateCalled.Should().BeTrue();
            service.CapturedLaser1.Should().Be(1.1);
            service.CapturedZ1.Should().Be(0);
            service.CapturedLaser2.Should().Be(2.2);
            service.CapturedZ2.Should().Be(0);
            service.CapturedLaserStandard.Should().Be(5.0);
            // Service 写回路径:_result 字段被填回
            vm.LaserMap.Map1.DirectValue.Should().Be(1.1);
            vm.LaserMap.Map2.DirectValue.Should().Be(2.2);
            vm.LaserStandard.Should().Be(5.0);
            vm.Message.Should().Contain("已完成");
        }

        /// <summary>Service 返回 false 时,ApplyCalibrateCommand 应写失败 Message</summary>
        [Test]
        public void ApplyCalibrateCommand_WhenServiceReturnsFalse_ShouldWriteFailureMessage()
        {
            var service = new FakeCalibrationService { LaserCalibrateReturn = false };
            var vm = CreateVm(service: service);

            vm.ApplyCalibrateCommand.Execute();

            service.LaserCalibrateCalled.Should().BeTrue();
            vm.Message.Should().Contain("失败");
        }

        // ===== 验收标准 #3:XML 往返 =====

        /// <summary>Save→Load 内存往返后,LaserStandard/Map1/Map2 字段值应一致(ExportXml→ParserXml)</summary>
        [Test]
        public void SaveLoadCommand_XmlRoundTrip_ShouldPreserveLaserFields()
        {
            var vm = CreateVm();
            vm.LaserStandard = 7.5;
            vm.RealtimeLaserValue = "3.3";
            vm.RefreshPoint1Command.Execute();
            vm.RealtimeLaserValue = "4.4";
            vm.RefreshPoint2Command.Execute();

            vm.SaveCommand.Execute();

            // 改乱当前值
            vm.LaserStandard = 0;
            vm.LaserMap.Map1.DirectValue = 0;
            vm.LaserMap.Map1.UnitValue = 99;
            vm.LaserMap.Map2.DirectValue = 0;
            vm.LaserMap.Map2.UnitValue = 99;

            vm.LoadCommand.Execute();

            // XML 往返后字段恢复(ExportXml→ParserXml)
            vm.LaserStandard.Should().Be(7.5);
            vm.LaserMap.Map1.DirectValue.Should().Be(3.3);
            vm.LaserMap.Map1.UnitValue.Should().Be(0);
            vm.LaserMap.Map2.DirectValue.Should().Be(4.4);
            vm.LaserMap.Map2.UnitValue.Should().Be(0);
        }

        /// <summary>未先 Save 直接 Load,应写"无可加载"提示</summary>
        [Test]
        public void LoadCommand_WithoutSave_ShouldWriteNoDataMessage()
        {
            var vm = CreateVm();
            vm.LoadCommand.Execute();
            vm.Message.Should().Contain("无可加载");
        }

        // ===== 手写 fake =====

        /// <summary>
        /// 手写 fake IFiveAxisCalibrationService:LaserCalibrate 记录调用 + 可配置返回值 + 写回 laser
        /// (对齐 FiveAxisCalibrationService.LaserCalibrate 赋值语义,验证 VM↔Service 写回路径通);其余方法空实现。
        /// </summary>
        private class FakeCalibrationService : IFiveAxisCalibrationService
        {
            public bool LaserCalibrateCalled;
            public bool LaserCalibrateReturn = true;
            public double CapturedLaser1, CapturedZ1, CapturedLaser2, CapturedZ2, CapturedLaserStandard;
            public PositionXYZ CapturedLaserPosi, CapturedCameraPosi;

            public bool RoughCalibrate(RoughCaliResult rough, double mrxPulses, double mrzPulses) => false;
            public bool AccurateCalibrate(FiveAxisFrameProfile frameProfile, AccurateCaliResult accurate, Coord5Axis rough5Para,
                double ballRadius, double mrxPulses, double mrzPulses) => false;
            public bool LaserCalibrate(LaserCaliResult laser, double laser1, double z1, double laser2, double z2,
                double laserStandard, PositionXYZ laserPosi, PositionXYZ cameraPosi)
            {
                LaserCalibrateCalled = true;
                CapturedLaser1 = laser1; CapturedZ1 = z1;
                CapturedLaser2 = laser2; CapturedZ2 = z2;
                CapturedLaserStandard = laserStandard;
                CapturedLaserPosi = laserPosi; CapturedCameraPosi = cameraPosi;
                if (LaserCalibrateReturn && laser != null)
                {
                    // 写回(对齐 FiveAxisCalibrationService.LaserCalibrate 赋值语义)
                    laser.LaserStandard = laserStandard;
                    laser.LaserMap.Map1.DirectValue = laser1;
                    laser.LaserMap.Map1.UnitValue = z1;
                    laser.LaserMap.Map2.DirectValue = laser2;
                    laser.LaserMap.Map2.UnitValue = z2;
                    laser.LaserPosi = laserPosi;
                    laser.CameraPosi = cameraPosi;
                }
                return LaserCalibrateReturn;
            }
            public bool CalibrateWorkOrigin(TeachWorkOriginResult origin) => false;
        }

        /// <summary>
        /// 最小 fake IDeviceEngine(手写,无 Moq):仅 GetVDevices&lt;T&gt; 返回空列表,
        /// 其余成员返回默认值/空实现。VM 仅依赖 GetVDevices&lt;T&gt;,故其余不触发。
        /// </summary>
        private class FakeDeviceEngine : IDeviceEngine
        {
            public bool AutoSetMotionModuleCT { get; set; }
            public bool IsNeedSave { get; set; }
            public DeviceMode DeviceMode { get; set; }
            public bool IsBreakAction { get; set; }
            public List<VAxisPosGroup> PosGroup { get; set; }
            public string RecipeDataPath { get; set; }
            public string RecipeSlnPath { get; set; }
            public string RecipeConfigPath { get; set; }
            public string ModuleConfigPath { get; set; }
            public List<ModuleNameModel> ModuleNameGroup { get; set; }

            public event Action<string, object, object> PropertyChangedEvent;
            public event Action<Guid, string, string> ControlValueChangedEvent;
            public event Action<LogType, string> LogEvent;
            public event Action<int, int, string> LoadingEvent;
            public event Action<AlarmInfo> AlarmEvent_device;
            public event Func<IVirtualDevice, string> PrevDeleteEvent;
            public event Action<string, string> AlarmCodeChangedEvent;
            public event Func<AxisPosition, string> AxisPosDeleteEvent;
            public event Action<Guid, string, string> DeviceNameChangedEvent;
            public event Action VDeviceChangedEvent;
            public event Action SaveEvent;
            public event Func<string, string, string, string, bool> UpdateAlarmModuleParamsEvent;
            public event Action<DeviceMode> ModeChangedEvent;
            public event Action<bool> PrevModeChangeEvent;
            public event Action<IDeviceEngine, string> InitializedEvent;
            public event Action<EngineStatus, EngineStatus> StatusChangedEvent;
            public event Func<EngineStatus> GetMachineStatusEvent;
            public event Action<IMaintain> MaintainZeroEvent;
            public event Action<SystemRole> RoleChangedEvent;
            public event Func<string, string, List<object>> GetModuleListEvent;
            public event Func<IEnumerable<object>> GetPDCAModulesEvent;
            public event Func<IEnumerable<object>> GetSFCModulesEvent;

            // VM 唯一依赖:返回空设备列表
            public List<T> GetVDevices<T>(string module = null) => new List<T>();
            public List<IVirtualDevice> GetDevices(Type virtualType = null, string module = "") => new List<IVirtualDevice>();
            public List<IDevice> GetRealDevices(Type virtualType = null) => new List<IDevice>();

            public IVirtualDevice GetVirtualByID(Guid id) => null;
            public IVirtualDevice GetVirtualByName(string name) => null;
            public IDevice GetDeviceByID(Guid deviceID) => null;
            public IDevice GetDeviceByName(string name) => null;
            public AxisPosition GetAxisPosition(Guid key) => null;
            public EngineStatus GetMachineStatus() => default;
            public Dictionary<Type, object> GetDevice_Type() => new Dictionary<Type, object>();
            public List<Type> GetAdapters() => new List<Type>();
            public List<KeyValue> GetBrands(Type type) => new List<KeyValue>();
            public List<Type> GetVirtualTypes() => new List<Type>();
            public List<Type> GetRealTypes() => new List<Type>();
            public List<string> GetModules() => new List<string>();
            public List<string> GetModulesUsed() => new List<string>();
            public List<XElement> LoadControlPara() => new List<XElement>();
            public List<object> GetModulesFromMotionEngine(string guid = null, string type = null) => new List<object>();
            public IEnumerable<object> GetPDCAModulesFromMotionEngine() => new List<object>();
            public IEnumerable<object> GetSFCModulesFromMotionEngine() => new List<object>();
            public bool RaiseUpdateAlarmModuleParams(string moduleId, string code, string message, string detail) => false;
            public bool Recovery() => false;

            public bool SetEngineMode(DeviceMode mode, out string errMsg) { errMsg = null; return false; }
            public bool CheckHardware(out string msg) { msg = null; return false; }
            public bool Home(out string errMsg) { errMsg = null; return false; }
            public bool IsHome(out string errMsg) { errMsg = null; return false; }

            public void OnLog(LogType type, string message) { }
            public void OnLoading(int count, int cur, string message) { }
            public void Initialize(string deviceTask) { }
            public void LoadDrivers(string loadPath = "") { }
            public void Save(string saveTask = "") { }
            public void OnAlarm(AlarmInfo alarm) { }
            public void AddDevice(IDevice device) { }
            public void ReomoveDevice(Guid deviceID) { }
            public void AddVirtual(IVirtualDevice virDevice, bool setReal = true) { }
            public void ReomoveVirtual(params Guid[] deviceID) { }
            public void ReomoveVirtual(Type vType) { }
            public void Stop() { }
            public void Pause() { }
            public void StartDeviceMonitor() { }
            public void StopDeviceMonitor() { }
            public string CheckAxisPosCanDelete(AxisPosition axisPos) => null;
            public void RemoveAxisPos(AxisPosition axisPos) { }
            public void UpdatePostion(AxisPosition pos, double newV) { }
            public void TeachPosGroup(string name, string module, params VAxis[] vAxis) { }
            public void RemovePosGroup(string name) { }
            public void UpdatePosGroup(VAxisPosGroup pGroup, AxisType aType, double position) { }
            public void UpdatePosGroup(AxisPosition pPos, double position) { }
            public void SavePosGroup(XElement xParent) { }
            public void LoadPosGroup(XElement xPosGroup) { }
            public void MaintainZero(IMaintain maintain) { }
            public void OnUserLogin(SystemRole role) { }
            public void OnStatusChanged(EngineStatus src, EngineStatus dst) { }
            public void UpdateControlPara(Guid guid, string Name, string value) { }
            public void AddModuleNameGroup(string name) { }
            public void RemoveModuleNameGroup(string name) { }
            public void SaveModuleNameGroup(XElement xParent) { }
            public void LoadModuleNameGroup(XElement xModuleGroup) { }
            public void RaiseAlarmCodeChangedEvent(string oldCode, string newCode) { }
            public void RaiseVDeviceChangedEvent() { }
            public void Dispose() { }
        }

        /// <summary>
        /// ICommonBus 手写最小 fake(适配 MotionPageVM 基类构造链):EventBus 返回真实 Prism EventAggregator
        /// (基类 RegisterEvent 需要),CurrentUser 返回 null(RegisterEvent 跳过 SysRole 赋值),
        /// 其余成员空实现/默认值(构造期不调用)。照抄 ICommonBus.cs 签名实现为空体,避免 Moq 依赖。
        /// </summary>
        private sealed class FakeCommonBus : ICommonBus
        {
            public IEventAggregator EventBus { get; set; } = new EventAggregator();
            public UserModel CurrentUser { get; set; } = null;

            public event Action<XElement> LoadRecipeEvent { add { } remove { } }
            public bool IsNeedSave { get; set; }
            public int EditCount => 0;
            public EngineStatus GetStatus() => default(EngineStatus);
            public PageModel CurrentPage { get; set; }
            public void OnNavigate(PageModel pageModel) { }
            public void OnLog(LogInfo logInfo) { }
            public void OnLog(LogType logType, string logInfo, string logThreadNo = "") { }
            public void OnSaveSystem(string sysConfig = "") { }
            public void OnSaveError(string sysConfig = "") { }
            public void OnLoadSystem(string sysConfig = "") { }
            public void OnActiveRecipe(Recipe recipe) { }
            public void OnSaveRecipe(string saveRecipe = "") { }
            public void OnBackUpRecipe(bool IsMaual = false) { }
            public void OnSaveDevice() { }
            public void PublishEvent<T, K>(K eventData) where T : PubSubEvent<K>, new() { }
            public Recipe CurrentRecipe { get; set; }
            public UserConfig UserConfig { get; set; }
            public ProjectInfo ProjInfo { get; set; }
            public BarcodeConfig BarConfig { get; set; }
            public void OnUserLogin(UserModel model) { }
            public void OnUserRoleChange(UserInfo userInfo) { }
            public void OnRemainTimeChange(int remainTime) { }
            public void OnAvalonLayoutSave() { }
            public List<ProjectInfo> ProjectList { get; set; } = new List<ProjectInfo>();
            public string L(string key) => key;
            public void ChangeDeviceMode(DeviceMode deviceMode) { }
            public List<LNode> GetOutDataTree() => new List<LNode>();
            public List<MapData> GetMapDatas() => new List<MapData>();
            public void UpdayeMapDataSource(List<MapData> mapData, List<MapData> newMapDatas, List<MapData> removeMapData) { }
            public void OnChangeRecord(OperationType changeType, string module, string prop, string content) { }
            public void InitSolution(string solution = "") { }
            public void AddProject(string projName, string slnPath, string recipe) { }
            public void RemoveProject(string projName) { }
            public void OpenExistProj(string projName, string slnPath) { }
            public void SaveSolution(string solution = "") { }
            public void CheckBackUpFile() { }
            public void ChangeLanguage() { }
            public void SaveChartConfig(string chartconfig, List<ChartDataModel> chartList) { }
            public List<ChartDataModel> LoadChartList() => new List<ChartDataModel>();
            public void NewOrOpenHolo3D(IMotionModule holoModule, string taskName, bool isNew = true) { }
            public void RegisterSystemDll() { }
            public Type GetUiModuleType() => null;
            public Type GetMainContentType() => null;
            public Type GetToolbarContentType() => null;
            public void StartHistoryFileDelete() { }
            public string PickAvailableDrive() => null;
        }
    }
}
