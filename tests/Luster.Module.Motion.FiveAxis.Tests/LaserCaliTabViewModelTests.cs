using FluentAssertions;
using NUnit.Framework;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
// ICommonBus 成员类型所用命名空间（照抄 ICommonBus.cs 的 using，保证全部类型可见）
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
// 被测 VM + Service + 数据契约
using Luster.Motion.EditorUI.ViewModel;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Service;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// LaserCaliTabViewModel 单测（TES-141 VM 验收）。
    /// 覆盖：构造不抛 / 关键属性 INPC / RefreshPoint1·2 透传 LaserCalibrate /
    /// OpenLaser 翻转 IsLaserOpen / ToggleRealtimeRead 翻转 IsRealtimeReading /
    /// RefreshLaserPosi·RefreshCameraPosi 抛 NotImplementedException。
    /// 手写 fake（无 Moq），沿用 FiveAxisCalibrationServiceTests 的 FakeFrame 风格。
    /// </summary>
    [TestFixture]
    public class LaserCaliTabViewModelTests
    {
        private LaserCaliTabViewModel CreateVm(FakeFiveAxisCalibrationService svc = null)
            => new LaserCaliTabViewModel(new FakeCommonBus(), svc ?? new FakeFiveAxisCalibrationService());

        #region 构造

        /// <summary>构造注入 fake 依赖不抛（基类 MotionVM 访问 EventBus/HandyControl.ConfigHelper 应正常）。</summary>
        [Test]
        public void Ctor_WithValidDeps_DoesNotThrow()
        {
            Action act = () => new LaserCaliTabViewModel(new FakeCommonBus(), new FakeFiveAxisCalibrationService());

            act.Should().NotThrow();
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>LaserStandard 代理到 _result，setter 手动 NotifyProperty 触发变更通知。</summary>
        [Test]
        public void LaserStandard_Setter_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            var changed = new List<string>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.LaserStandard = 12.34;

            changed.Should().Contain(nameof(LaserCaliTabViewModel.LaserStandard));
        }

        /// <summary>RealtimeLaserValue 用 SetProperty，应触发 INPC。</summary>
        [Test]
        public void RealtimeLaserValue_Setter_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            var changed = new List<string>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.RealtimeLaserValue = 7.5;

            changed.Should().Contain(nameof(LaserCaliTabViewModel.RealtimeLaserValue));
        }

        /// <summary>LaserPosi 代理到 _result，setter 用 ReferenceEquals 比较，传新对象触发 INPC。</summary>
        [Test]
        public void LaserPosi_Setter_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            var changed = new List<string>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.LaserPosi = new PositionXYZ(1, 2, 3);

            changed.Should().Contain(nameof(LaserCaliTabViewModel.LaserPosi));
        }

        /// <summary>CameraPosi 代理到 _result，setter 用 ReferenceEquals 比较，传新对象触发 INPC。</summary>
        [Test]
        public void CameraPosi_Setter_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            var changed = new List<string>();
            ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.CameraPosi = new PositionXYZ(4, 5, 6);

            changed.Should().Contain(nameof(LaserCaliTabViewModel.CameraPosi));
        }

        #endregion

        #region RefreshPoint1/2 透传 LaserCalibrate

        /// <summary>RefreshPoint1Command.Execute() → LaserCalibrate 被调用且 VM 字段透传（laser1/z1/laser2/z2/laserStandard/laserPosi/cameraPosi）。</summary>
        [Test]
        public void RefreshPoint1Command_Execute_CallsLaserCalibrateWithCurrentFields()
        {
            var fake = new FakeFiveAxisCalibrationService();
            var vm = CreateVm(fake);
            vm.LaserStandard = 12.34;
            vm.LaserMap.Map1.DirectValue = 1.0;
            vm.LaserMap.Map1.UnitValue = 100.0;
            vm.LaserMap.Map2.DirectValue = 5.0;
            vm.LaserMap.Map2.UnitValue = 500.0;
            vm.LaserPosi = new PositionXYZ(10, 20, 30);
            vm.CameraPosi = new PositionXYZ(12, 22, 31);

            vm.RefreshPoint1Command.Execute();

            fake.WasCalled.Should().BeTrue();
            fake.Laser1.Should().Be(1.0);
            fake.Z1.Should().Be(100.0);
            fake.Laser2.Should().Be(5.0);
            fake.Z2.Should().Be(500.0);
            fake.LaserStandard.Should().Be(12.34);
            // LaserPosi/CameraPosi 透传同一引用（_result.LaserPosi/_result.CameraPosi）
            fake.LaserPosi.Should().BeSameAs(vm.LaserPosi);
            fake.CameraPosi.Should().BeSameAs(vm.CameraPosi);
            // VM 透传 _result(LaserCaliResult) 实例，验证携带的 LaserStandard 字段值
            fake.Laser.Should().NotBeNull();
            fake.Laser.LaserStandard.Should().Be(12.34);
        }

        /// <summary>RefreshPoint2Command.Execute() → 同样透传 LaserCalibrate。</summary>
        [Test]
        public void RefreshPoint2Command_Execute_CallsLaserCalibrateWithCurrentFields()
        {
            var fake = new FakeFiveAxisCalibrationService();
            var vm = CreateVm(fake);
            vm.LaserStandard = 9.0;
            vm.LaserMap.Map1.DirectValue = 2.0;
            vm.LaserMap.Map1.UnitValue = 200.0;
            vm.LaserMap.Map2.DirectValue = 6.0;
            vm.LaserMap.Map2.UnitValue = 600.0;
            vm.LaserPosi = new PositionXYZ(1, 1, 1);
            vm.CameraPosi = new PositionXYZ(2, 2, 2);

            vm.RefreshPoint2Command.Execute();

            fake.WasCalled.Should().BeTrue();
            fake.Laser1.Should().Be(2.0);
            fake.Z1.Should().Be(200.0);
            fake.Laser2.Should().Be(6.0);
            fake.Z2.Should().Be(600.0);
            fake.LaserStandard.Should().Be(9.0);
        }

        #endregion

        #region OpenLaser / ToggleRealtimeRead 翻转

        /// <summary>OpenLaserCommand.Execute() 翻转 IsLaserOpen（false→true→false）。</summary>
        [Test]
        public void OpenLaserCommand_Execute_TogglesIsLaserOpen()
        {
            var vm = CreateVm();
            bool initial = vm.IsLaserOpen;

            vm.OpenLaserCommand.Execute();
            vm.IsLaserOpen.Should().Be(!initial);

            vm.OpenLaserCommand.Execute();
            vm.IsLaserOpen.Should().Be(initial);
        }

        /// <summary>ToggleRealtimeReadCommand.Execute() 翻转 IsRealtimeReading（false→true→false）。</summary>
        [Test]
        public void ToggleRealtimeReadCommand_Execute_TogglesIsRealtimeReading()
        {
            var vm = CreateVm();
            bool initial = vm.IsRealtimeReading;

            vm.ToggleRealtimeReadCommand.Execute();
            vm.IsRealtimeReading.Should().Be(!initial);

            vm.ToggleRealtimeReadCommand.Execute();
            vm.IsRealtimeReading.Should().Be(initial);
        }

        #endregion

        #region NotImplementedException

        /// <summary>RefreshLaserPosiCommand.Execute() 抛 NotImplementedException（运动位置读取接口未定义）。</summary>
        [Test]
        public void RefreshLaserPosiCommand_Execute_ThrowsNotImplemented()
        {
            var vm = CreateVm();

            Action act = () => vm.RefreshLaserPosiCommand.Execute();

            act.Should().Throw<NotImplementedException>();
        }

        /// <summary>RefreshCameraPosiCommand.Execute() 抛 NotImplementedException。</summary>
        [Test]
        public void RefreshCameraPosiCommand_Execute_ThrowsNotImplemented()
        {
            var vm = CreateVm();

            Action act = () => vm.RefreshCameraPosiCommand.Execute();

            act.Should().Throw<NotImplementedException>();
        }

        #endregion

        #region Fakes

        /// <summary>
        /// IFiveAxisCalibrationService 手写 fake（spy 模式）：仅 LaserCalibrate 记录调用参数，
        /// 其余三方法构造期/测试中不会被触发，throw NotImplementedException 暴露误调用。
        /// </summary>
        private sealed class FakeFiveAxisCalibrationService : IFiveAxisCalibrationService
        {
            public bool WasCalled { get; private set; }
            public LaserCaliResult Laser { get; private set; }
            public double Laser1 { get; private set; }
            public double Z1 { get; private set; }
            public double Laser2 { get; private set; }
            public double Z2 { get; private set; }
            public double LaserStandard { get; private set; }
            public PositionXYZ LaserPosi { get; private set; }
            public PositionXYZ CameraPosi { get; private set; }

            public bool LaserCalibrate(LaserCaliResult laser, double laser1, double z1, double laser2, double z2,
                double laserStandard, PositionXYZ laserPosi, PositionXYZ cameraPosi)
            {
                WasCalled = true;
                Laser = laser;
                Laser1 = laser1; Z1 = z1;
                Laser2 = laser2; Z2 = z2;
                LaserStandard = laserStandard;
                LaserPosi = laserPosi;
                CameraPosi = cameraPosi;
                return true;
            }

            public bool RoughCalibrate(RoughCaliResult rough, double mrxPulses, double mrzPulses)
                => throw new NotImplementedException();

            public bool AccurateCalibrate(FiveAxisFrameProfile frameProfile, AccurateCaliResult accurate, Coord5Axis rough5Para,
                double ballRadius, double mrxPulses, double mrzPulses)
                => throw new NotImplementedException();

            public bool CalibrateWorkOrigin(TeachWorkOriginResult origin)
                => throw new NotImplementedException();
        }

        /// <summary>
        /// ICommonBus 手写最小 fake：EventBus 返回真实 Prism EventAggregator（基类 RegisterEvent 需要），
        /// CurrentUser 返回 null（RegisterEvent 跳过 SysRole 赋值），其余成员空实现（构造期不调用）。
        /// ICommonBus 成员众多，照抄接口签名实现为空体/默认值，避免 Moq 依赖。
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

        #endregion
    }
}
