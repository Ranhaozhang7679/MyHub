using FluentAssertions;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// LaserCaliTabViewModel 单测(TES-163)。
    /// 无 Moq → 手写 fake:IFiveAxisCalibrationService(空实现)+ IDeviceEngine(仅 GetVDevices&lt;T&gt; 返回空列表,
    /// 其余成员返回默认值/空实现)。ICommonBus 传 null(VM 内部 null 安全)。
    /// 设备交互路径(实时读时序/轴名映射/单值提取)⚠️ 待人类现场验证,此处只测 VM 纯逻辑分支与状态机。
    /// </summary>
    [TestFixture]
    public class LaserCaliTabViewModelTests
    {
        private static LaserCaliTabViewModel CreateVm(FakeDeviceEngine engine = null)
        {
            engine = engine ?? new FakeDeviceEngine();
            return new LaserCaliTabViewModel(null, new FakeCalibrationService(), engine);
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
            vm.LaserStandard = 9.9;
            vm.LaserStandard.Should().Be(9.9);
        }

        [Test]
        public void Dispose_AfterToggle_ShouldNotThrow()
        {
            var vm = CreateVm();
            vm.ToggleRealtimeReadCommand.Execute();
            Action act = () => vm.Dispose();
            act.Should().NotThrow();
        }

        // ===== 手写 fake =====

        /// <summary>最小 fake IFiveAxisCalibrationService(空实现,本 issue VM 不调标定执行)</summary>
        private class FakeCalibrationService : IFiveAxisCalibrationService
        {
            public bool RoughCalibrate(RoughCaliResult rough, double mrxPulses, double mrzPulses) => false;
            public bool AccurateCalibrate(FiveAxisFrameProfile frameProfile, AccurateCaliResult accurate, Coord5Axis rough5Para,
                double ballRadius, double mrxPulses, double mrzPulses) => false;
            public bool LaserCalibrate(LaserCaliResult laser, double laser1, double z1, double laser2, double z2,
                double laserStandard, PositionXYZ laserPosi, PositionXYZ cameraPosi) => false;
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
    }
}
