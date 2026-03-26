using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HandyControl.Tools;

namespace Luster.SimDevice.SubSystem.Langs
{
    public class LangProvider : INotifyPropertyChanged
    {
        public static LangProvider Instance { get; } = ResourceHelper.GetResource<LangProvider>("DeviceLangs");

        private static string CultureInfoStr;

        public static CultureInfo Culture
        {
            get => Lang.Culture;
            set
            {
                if (value == null) return;
                if (Equals(CultureInfoStr, value.EnglishName)) return;
                Lang.Culture = value;
                CultureInfoStr = value.EnglishName;

                Instance.UpdateLangs();
            }
        }

        public static string GetLang(string key) 
        {
            string val = Lang.ResourceManager.GetString(key, Culture);
            if (string.IsNullOrEmpty(val)) return key;

            return val;
        }

        public static void SetLang(DependencyObject dependencyObject, DependencyProperty dependencyProperty, string key) =>
            BindingOperations.SetBinding(dependencyObject, dependencyProperty, new Binding(key)
            {
                Source = Instance,
                Mode = BindingMode.OneWay
            });

		private void UpdateLangs()
        {
			OnPropertyChanged(nameof(_2_5DDepthMap));
			OnPropertyChanged(nameof(_2DSkeletonMap));
			OnPropertyChanged(nameof(AbsoluteMove));
			OnPropertyChanged(nameof(Acc));
			OnPropertyChanged(nameof(AccSpeed));
			OnPropertyChanged(nameof(ActionArea));
			OnPropertyChanged(nameof(ActionName));
			OnPropertyChanged(nameof(ActionType));
			OnPropertyChanged(nameof(Add));
			OnPropertyChanged(nameof(AddAddress));
			OnPropertyChanged(nameof(Address));
			OnPropertyChanged(nameof(AddressCount));
			OnPropertyChanged(nameof(AddressDescription));
			OnPropertyChanged(nameof(AddressList));
			OnPropertyChanged(nameof(AlarmCode));
			OnPropertyChanged(nameof(AlarmConfig));
			OnPropertyChanged(nameof(AlarmConfigCustom));
			OnPropertyChanged(nameof(AnalogIn));
			OnPropertyChanged(nameof(AnalogIO));
			OnPropertyChanged(nameof(AnalogOut));
			OnPropertyChanged(nameof(AutoScan));
			OnPropertyChanged(nameof(Axis));
			OnPropertyChanged(nameof(Axis1));
			OnPropertyChanged(nameof(AxisCount));
			OnPropertyChanged(nameof(AxisForward));
			OnPropertyChanged(nameof(AxisIODebug));
			OnPropertyChanged(nameof(AxisMotionPremise));
			OnPropertyChanged(nameof(AxisNo));
			OnPropertyChanged(nameof(AxisPML));
			OnPropertyChanged(nameof(AxisPosConfig));
			OnPropertyChanged(nameof(AxisPosition));
			OnPropertyChanged(nameof(AxisType));
			OnPropertyChanged(nameof(BatchImport));
			OnPropertyChanged(nameof(BaudRate));
			OnPropertyChanged(nameof(Behavior));
			OnPropertyChanged(nameof(Blow));
			OnPropertyChanged(nameof(BlowClose));
			OnPropertyChanged(nameof(BlowDO));
			OnPropertyChanged(nameof(Brand));
			OnPropertyChanged(nameof(Brightness));
			OnPropertyChanged(nameof(Browse));
			OnPropertyChanged(nameof(Button));
			OnPropertyChanged(nameof(ButtonLight));
			OnPropertyChanged(nameof(Calculate));
			OnPropertyChanged(nameof(Camera));
			OnPropertyChanged(nameof(CameraConfigutation));
			OnPropertyChanged(nameof(CameraWindow));
			OnPropertyChanged(nameof(Cancel));
			OnPropertyChanged(nameof(Channel));
			OnPropertyChanged(nameof(ChannelCount));
			OnPropertyChanged(nameof(CheckRecord));
			OnPropertyChanged(nameof(CheckResult));
			OnPropertyChanged(nameof(CheckTime));
			OnPropertyChanged(nameof(CircleDisplacement));
			OnPropertyChanged(nameof(Clear));
			OnPropertyChanged(nameof(ClearError));
			OnPropertyChanged(nameof(Close));
			OnPropertyChanged(nameof(CollectRows));
			OnPropertyChanged(nameof(Comm));
			OnPropertyChanged(nameof(CommandPulse));
			OnPropertyChanged(nameof(CommType));
			OnPropertyChanged(nameof(Communcation));
			OnPropertyChanged(nameof(CommuncationAction));
			OnPropertyChanged(nameof(CommuncationAddress));
			OnPropertyChanged(nameof(CommuncationTest));
			OnPropertyChanged(nameof(CommuncationType));
			OnPropertyChanged(nameof(Connect));
			OnPropertyChanged(nameof(ContinuousSend));
			OnPropertyChanged(nameof(CoordinateSystem));
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(nameof(CreateROI));
			OnPropertyChanged(nameof(CurrentPosition));
			OnPropertyChanged(nameof(CurrentScale));
			OnPropertyChanged(nameof(CurrentValue));
			OnPropertyChanged(nameof(CylinderType));
			OnPropertyChanged(nameof(DataBits));
			OnPropertyChanged(nameof(DataTrigger));
			OnPropertyChanged(nameof(DataType));
			OnPropertyChanged(nameof(DataValue));
			OnPropertyChanged(nameof(Dcc));
			OnPropertyChanged(nameof(DccSpeed));
			OnPropertyChanged(nameof(Default));
			OnPropertyChanged(nameof(Delay));
			OnPropertyChanged(nameof(Delete));
			OnPropertyChanged(nameof(Detector));
			OnPropertyChanged(nameof(Device));
			OnPropertyChanged(nameof(DeviceList));
			OnPropertyChanged(nameof(DeviceManagement));
			OnPropertyChanged(nameof(DeviceName));
			OnPropertyChanged(nameof(Diatance));
			OnPropertyChanged(nameof(DigitalIn));
			OnPropertyChanged(nameof(DigitalIO));
			OnPropertyChanged(nameof(DigitalOut));
			OnPropertyChanged(nameof(Disable));
			OnPropertyChanged(nameof(DisplayType));
			OnPropertyChanged(nameof(DoubleControl));
			OnPropertyChanged(nameof(Down));
			OnPropertyChanged(nameof(DownEnable));
			OnPropertyChanged(nameof(Drag));
			OnPropertyChanged(nameof(Edit));
			OnPropertyChanged(nameof(EighthIO));
			OnPropertyChanged(nameof(EmergencyStop));
			OnPropertyChanged(nameof(Empty));
			OnPropertyChanged(nameof(Enabled));
			OnPropertyChanged(nameof(EncoderNo));
			OnPropertyChanged(nameof(EncoderType));
			OnPropertyChanged(nameof(EndianType));
			OnPropertyChanged(nameof(ErrorMessage));
			OnPropertyChanged(nameof(ExcelFile));
			OnPropertyChanged(nameof(Export));
			OnPropertyChanged(nameof(ExportToTxt));
			OnPropertyChanged(nameof(ExposureTime));
			OnPropertyChanged(nameof(Extend));
			OnPropertyChanged(nameof(ExtendDetetor));
			OnPropertyChanged(nameof(ExtendDI));
			OnPropertyChanged(nameof(ExtendDO));
			OnPropertyChanged(nameof(ExtendName));
			OnPropertyChanged(nameof(ExtendStatus));
			OnPropertyChanged(nameof(FifthIO));
			OnPropertyChanged(nameof(Finish));
			OnPropertyChanged(nameof(FirstAxis));
			OnPropertyChanged(nameof(FirstIO));
			OnPropertyChanged(nameof(FlyingPhoto));
			OnPropertyChanged(nameof(FlyingPhotoName));
			OnPropertyChanged(nameof(ForwardTrip));
			OnPropertyChanged(nameof(FourthAxis));
			OnPropertyChanged(nameof(FourthIO));
			OnPropertyChanged(nameof(Frame));
			OnPropertyChanged(nameof(Frequency));
			OnPropertyChanged(nameof(GearRatioDenominator));
			OnPropertyChanged(nameof(GearRatioNumerator));
			OnPropertyChanged(nameof(Grab));
			OnPropertyChanged(nameof(HaveTransmission));
			OnPropertyChanged(nameof(HighSpeed));
			OnPropertyChanged(nameof(Home));
			OnPropertyChanged(nameof(HomeDevice));
			OnPropertyChanged(nameof(HomeParas));
			OnPropertyChanged(nameof(HomeType));
			OnPropertyChanged(nameof(HomeZero));
			OnPropertyChanged(nameof(ImportPlcAddress));
			OnPropertyChanged(nameof(Index));
			OnPropertyChanged(nameof(InExtendName));
			OnPropertyChanged(nameof(InIOName));
			OnPropertyChanged(nameof(Inport));
			OnPropertyChanged(nameof(InRetractName));
			OnPropertyChanged(nameof(IntensityImage));
			OnPropertyChanged(nameof(IOCount));
			OnPropertyChanged(nameof(IOMontor));
			OnPropertyChanged(nameof(IOName));
			OnPropertyChanged(nameof(IOType));
			OnPropertyChanged(nameof(IPAddress));
			OnPropertyChanged(nameof(Jog));
			OnPropertyChanged(nameof(JogSpped));
			OnPropertyChanged(nameof(Jump));
            OnPropertyChanged(nameof(KeyParameterConfig));
			OnPropertyChanged(nameof(LangComment));
			OnPropertyChanged(nameof(LaserConfigutation));
			OnPropertyChanged(nameof(LaserConfigutationPath));
			OnPropertyChanged(nameof(LaserDataTriggerTip));
			OnPropertyChanged(nameof(LaserHardTrrigerConfigutation));
			OnPropertyChanged(nameof(LaserJob));
			OnPropertyChanged(nameof(LaserName));
			OnPropertyChanged(nameof(LaserStartIO));
			OnPropertyChanged(nameof(LaserStartTip));
			OnPropertyChanged(nameof(LastMonthMaintenance));
			OnPropertyChanged(nameof(LastWeekMaintenance));
			OnPropertyChanged(nameof(LCTNo));
			OnPropertyChanged(nameof(LightConfigutation));
			OnPropertyChanged(nameof(LightController));
			OnPropertyChanged(nameof(LightDelayToOff));
			OnPropertyChanged(nameof(LinearComplement));
			OnPropertyChanged(nameof(LineLaser));
			OnPropertyChanged(nameof(Load));
			OnPropertyChanged(nameof(LocationRange));
			OnPropertyChanged(nameof(LowSpeed));
			OnPropertyChanged(nameof(Maintain));
			OnPropertyChanged(nameof(MaintainScale));
			OnPropertyChanged(nameof(Maintenance));
			OnPropertyChanged(nameof(Management));
			OnPropertyChanged(nameof(MapType));
			OnPropertyChanged(nameof(Match));
			OnPropertyChanged(nameof(MaxPosition));
			OnPropertyChanged(nameof(MaxPositionWithUnit));
			OnPropertyChanged(nameof(MaxScale));
			OnPropertyChanged(nameof(MinPosition));
			OnPropertyChanged(nameof(MinPositionWithUnit));
			OnPropertyChanged(nameof(ModbusRTU));
			OnPropertyChanged(nameof(Modify));
			OnPropertyChanged(nameof(ModuleConfig));
			OnPropertyChanged(nameof(ModuleName));
			OnPropertyChanged(nameof(ModuleOrName));
			OnPropertyChanged(nameof(MonitorDevice));
			OnPropertyChanged(nameof(MonitorValue));
			OnPropertyChanged(nameof(MotionCard));
			OnPropertyChanged(nameof(MotionPremise));
			OnPropertyChanged(nameof(MotionTest));
			OnPropertyChanged(nameof(Move));
			OnPropertyChanged(nameof(MoveDistance));
			OnPropertyChanged(nameof(MovePrecondition));
			OnPropertyChanged(nameof(MoveType));
			OnPropertyChanged(nameof(Name));
			OnPropertyChanged(nameof(NeedDetetor));
			OnPropertyChanged(nameof(NeedHome));
			OnPropertyChanged(nameof(NeedShield));
			OnPropertyChanged(nameof(NegativeTrip));
			OnPropertyChanged(nameof(NextCommand));
			OnPropertyChanged(nameof(NormalParas));
			OnPropertyChanged(nameof(Null));
			OnPropertyChanged(nameof(OfflineConfigutation));
			OnPropertyChanged(nameof(OfflineImage));
			OnPropertyChanged(nameof(OfflinePointCloud));
			OnPropertyChanged(nameof(Open));
			OnPropertyChanged(nameof(Operation));
			OnPropertyChanged(nameof(OtherConfigutation));
			OnPropertyChanged(nameof(OutIOName));
            OnPropertyChanged(nameof(PositionParameterConfig));
            OnPropertyChanged(nameof(PacketData));
			OnPropertyChanged(nameof(Parity));
			OnPropertyChanged(nameof(PerPageCount));
			OnPropertyChanged(nameof(PlaceTime));
			OnPropertyChanged(nameof(PlcAddress));
			OnPropertyChanged(nameof(PleaseInputAxisName));
			OnPropertyChanged(nameof(PleaseSelectCommType));
			OnPropertyChanged(nameof(Point));
			OnPropertyChanged(nameof(Port));
			OnPropertyChanged(nameof(PosionMotionPremise));
			OnPropertyChanged(nameof(Position));
			OnPropertyChanged(nameof(PositionPulse));
			OnPropertyChanged(nameof(PressDistance));
			OnPropertyChanged(nameof(PressDriver));
			OnPropertyChanged(nameof(Pressure));
			OnPropertyChanged(nameof(Print));
			OnPropertyChanged(nameof(Printer));
			OnPropertyChanged(nameof(Priority));
			OnPropertyChanged(nameof(Process));
			OnPropertyChanged(nameof(Protocol));
			OnPropertyChanged(nameof(PulseCal));
			OnPropertyChanged(nameof(PulseCount));
			OnPropertyChanged(nameof(PulseRatio));
			OnPropertyChanged(nameof(PulseWidth));
			OnPropertyChanged(nameof(PutValue));
			OnPropertyChanged(nameof(Read));
			OnPropertyChanged(nameof(ReadAll));
			OnPropertyChanged(nameof(ReadData));
			OnPropertyChanged(nameof(ReadOnly));
			OnPropertyChanged(nameof(ReadOrWrite));
			OnPropertyChanged(nameof(RelativeMove));
			OnPropertyChanged(nameof(Retract));
			OnPropertyChanged(nameof(RetractDetetor));
			OnPropertyChanged(nameof(RetractDI));
			OnPropertyChanged(nameof(RetractDO));
			OnPropertyChanged(nameof(RetractName));
			OnPropertyChanged(nameof(RetractStatus));
			OnPropertyChanged(nameof(Robot));
			OnPropertyChanged(nameof(RobotName));
			OnPropertyChanged(nameof(RobotRunModel));
			OnPropertyChanged(nameof(RobotRunModelTip));
			OnPropertyChanged(nameof(ROIHeight));
			OnPropertyChanged(nameof(ROIStartX));
			OnPropertyChanged(nameof(ROIStartY));
			OnPropertyChanged(nameof(ROIWidth));
			OnPropertyChanged(nameof(RunSpeed));
			OnPropertyChanged(nameof(SafeDevice));
			OnPropertyChanged(nameof(SafeDoor));
			OnPropertyChanged(nameof(Save));
			OnPropertyChanged(nameof(SecondAxis));
			OnPropertyChanged(nameof(SecondIO));
			OnPropertyChanged(nameof(Select));
			OnPropertyChanged(nameof(SelectAxis));
			OnPropertyChanged(nameof(SelectBrand));
			OnPropertyChanged(nameof(SelectEncoderTip));
			OnPropertyChanged(nameof(SelectExcelFile));
			OnPropertyChanged(nameof(SelectLaserConfigutation));
			OnPropertyChanged(nameof(SelectLaserConfigutationPath));
			OnPropertyChanged(nameof(SelectMotinCard));
			OnPropertyChanged(nameof(SelectOfflineImage));
			OnPropertyChanged(nameof(SelectOfflinePointCloud));
			OnPropertyChanged(nameof(SelectSafeDevice));
			OnPropertyChanged(nameof(SendPacket));
			OnPropertyChanged(nameof(SerialNumber));
			OnPropertyChanged(nameof(Set));
			OnPropertyChanged(nameof(SeventhIO));
			OnPropertyChanged(nameof(ShieldDevice));
			OnPropertyChanged(nameof(Shot));
			OnPropertyChanged(nameof(ShowPointCloud));
			OnPropertyChanged(nameof(SimulationValue));
			OnPropertyChanged(nameof(SingleControl));
			OnPropertyChanged(nameof(SixthIO));
			OnPropertyChanged(nameof(SlaveName));
			OnPropertyChanged(nameof(SlaveNo));
			OnPropertyChanged(nameof(Slot));
			OnPropertyChanged(nameof(SocketType));
			OnPropertyChanged(nameof(Speed));
			OnPropertyChanged(nameof(SpeedCoefficient));
			OnPropertyChanged(nameof(SpeedTip));
			OnPropertyChanged(nameof(StartAddress));
			OnPropertyChanged(nameof(StartCheck));
			OnPropertyChanged(nameof(StartIndex));
			OnPropertyChanged(nameof(StartIO));
			OnPropertyChanged(nameof(StartTrigger));
			OnPropertyChanged(nameof(StationNO));
			OnPropertyChanged(nameof(Status));
			OnPropertyChanged(nameof(Stop));
			OnPropertyChanged(nameof(StopBits));
			OnPropertyChanged(nameof(SubDefinite));
			OnPropertyChanged(nameof(Suck));
			OnPropertyChanged(nameof(SuckDO));
			OnPropertyChanged(nameof(Sure));
			OnPropertyChanged(nameof(TagHeight));
			OnPropertyChanged(nameof(TagWidth));
			OnPropertyChanged(nameof(TaikeContent));
			OnPropertyChanged(nameof(TaikeCurve));
			OnPropertyChanged(nameof(Teach));
			OnPropertyChanged(nameof(ThirdAxis));
			OnPropertyChanged(nameof(ThirdIO));
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(TriggerCount));
			OnPropertyChanged(nameof(TriggerFifoCount));
			OnPropertyChanged(nameof(TriggerNo));
			OnPropertyChanged(nameof(TriggerNoCount));
			OnPropertyChanged(nameof(TriggerNoType));
			OnPropertyChanged(nameof(TriggerPluseRatio));
			OnPropertyChanged(nameof(TxtReader));
			OnPropertyChanged(nameof(UnMonitorDevice));
			OnPropertyChanged(nameof(Up));
			OnPropertyChanged(nameof(Update));
			OnPropertyChanged(nameof(UppperEnable));
			OnPropertyChanged(nameof(USB));
			OnPropertyChanged(nameof(Used));
			OnPropertyChanged(nameof(VacuumSensor));
			OnPropertyChanged(nameof(VacuumSensorDI));
			OnPropertyChanged(nameof(Value));
			OnPropertyChanged(nameof(VAxis));
			OnPropertyChanged(nameof(VAxis3));
			OnPropertyChanged(nameof(VAxisIOAxisListTip));
			OnPropertyChanged(nameof(VAxisListTip));
			OnPropertyChanged(nameof(VAxisM));
			OnPropertyChanged(nameof(VBelt));
			OnPropertyChanged(nameof(VCamera));
			OnPropertyChanged(nameof(VCommuncation));
			OnPropertyChanged(nameof(VCylinder));
			OnPropertyChanged(nameof(VDevice));
			OnPropertyChanged(nameof(VESD));
			OnPropertyChanged(nameof(VFeeder));
			OnPropertyChanged(nameof(VFlyingPhoto));
			OnPropertyChanged(nameof(VInputIO));
			OnPropertyChanged(nameof(VIO));
			OnPropertyChanged(nameof(VIOSimulation));
			OnPropertyChanged(nameof(VisionProcessData));
			OnPropertyChanged(nameof(VLineLaser));
			OnPropertyChanged(nameof(VMCylinder));
			OnPropertyChanged(nameof(VOutputIO));
			OnPropertyChanged(nameof(VPCylinder));
			OnPropertyChanged(nameof(VPlc));
			OnPropertyChanged(nameof(VPrinter));
			OnPropertyChanged(nameof(VRobot));
			OnPropertyChanged(nameof(VSerialPort));
			OnPropertyChanged(nameof(VSocket));
			OnPropertyChanged(nameof(VTricolorlamp));
			OnPropertyChanged(nameof(VVacuum));
			OnPropertyChanged(nameof(Write));
			OnPropertyChanged(nameof(WriteData));
			OnPropertyChanged(nameof(WritePacket));
			OnPropertyChanged(nameof(WriteValue));
			OnPropertyChanged(nameof(XJCPressureSensor));
			OnPropertyChanged(nameof(ZeroOffset));
        }

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string _2_5DDepthMap => Lang._2_5DDepthMap;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string _2DSkeletonMap => Lang._2DSkeletonMap;

        /// <summary>
        ///   查找类似 绝对运动 的本地化字符串。
        /// </summary>
		public string AbsoluteMove => Lang.AbsoluteMove;

        /// <summary>
        ///   查找类似 加速度(mm²) 的本地化字符串。
        /// </summary>
		public string Acc => Lang.Acc;

        /// <summary>
        ///   查找类似 加速(mm/s²) 的本地化字符串。
        /// </summary>
		public string AccSpeed => Lang.AccSpeed;

        /// <summary>
        ///   查找类似 动作配置 的本地化字符串。
        /// </summary>
		public string ActionArea => Lang.ActionArea;

        /// <summary>
        ///   查找类似 动作名称 的本地化字符串。
        /// </summary>
		public string ActionName => Lang.ActionName;

        /// <summary>
        ///   查找类似 动作类型 的本地化字符串。
        /// </summary>
		public string ActionType => Lang.ActionType;

        /// <summary>
        ///   查找类似 添加 的本地化字符串。
        /// </summary>
		public string Add => Lang.Add;

        /// <summary>
        ///   查找类似 添加地址 的本地化字符串。
        /// </summary>
		public string AddAddress => Lang.AddAddress;

        /// <summary>
        ///   查找类似 地址 的本地化字符串。
        /// </summary>
		public string Address => Lang.Address;

        /// <summary>
        ///   查找类似 地址数量 的本地化字符串。
        /// </summary>
		public string AddressCount => Lang.AddressCount;

        /// <summary>
        ///   查找类似 地址描述 的本地化字符串。
        /// </summary>
		public string AddressDescription => Lang.AddressDescription;

        /// <summary>
        ///   查找类似 地址列表 的本地化字符串。
        /// </summary>
		public string AddressList => Lang.AddressList;

        /// <summary>
        ///   查找类似 报警代码 的本地化字符串。
        /// </summary>
		public string AlarmCode => Lang.AlarmCode;

        /// <summary>
        ///   查找类似 报警配置 的本地化字符串。
        /// </summary>
		public string AlarmConfig => Lang.AlarmConfig;

        /// <summary>
        ///   查找类似 自定义报警配置 的本地化字符串。
        /// </summary>
		public string AlarmConfigCustom => Lang.AlarmConfigCustom;

        /// <summary>
        ///   查找类似 模拟输入 的本地化字符串。
        /// </summary>
		public string AnalogIn => Lang.AnalogIn;

        /// <summary>
        ///   查找类似 模拟量输入输出 的本地化字符串。
        /// </summary>
		public string AnalogIO => Lang.AnalogIO;

        /// <summary>
        ///   查找类似 模拟输出 的本地化字符串。
        /// </summary>
		public string AnalogOut => Lang.AnalogOut;

        /// <summary>
        ///   查找类似 自动扫描 的本地化字符串。
        /// </summary>
		public string AutoScan => Lang.AutoScan;

        /// <summary>
        ///   查找类似 轴 的本地化字符串。
        /// </summary>
		public string Axis => Lang.Axis;

        /// <summary>
        ///   查找类似 轴1 的本地化字符串。
        /// </summary>
		public string Axis1 => Lang.Axis1;

        /// <summary>
        ///   查找类似 轴数量 的本地化字符串。
        /// </summary>
		public string AxisCount => Lang.AxisCount;

        /// <summary>
        ///   查找类似 正方向 的本地化字符串。
        /// </summary>
		public string AxisForward => Lang.AxisForward;

        /// <summary>
        ///   查找类似 轴点位调试 的本地化字符串。
        /// </summary>
		public string AxisIODebug => Lang.AxisIODebug;

        /// <summary>
        ///   查找类似 轴运动前提 的本地化字符串。
        /// </summary>
		public string AxisMotionPremise => Lang.AxisMotionPremise;

        /// <summary>
        ///   查找类似 轴号 的本地化字符串。
        /// </summary>
		public string AxisNo => Lang.AxisNo;

        /// <summary>
        ///   查找类似 型号 的本地化字符串。
        /// </summary>
		public string AxisPML => Lang.AxisPML;

        /// <summary>
        ///   查找类似 轴点位配置 的本地化字符串。
        /// </summary>
		public string AxisPosConfig => Lang.AxisPosConfig;

        /// <summary>
        ///   查找类似 位置 的本地化字符串。
        /// </summary>
		public string AxisPosition => Lang.AxisPosition;

        /// <summary>
        ///   查找类似 轴类型 的本地化字符串。
        /// </summary>
		public string AxisType => Lang.AxisType;

        /// <summary>
        ///   查找类似 批量导入 的本地化字符串。
        /// </summary>
		public string BatchImport => Lang.BatchImport;

        /// <summary>
        ///   查找类似 波特率 的本地化字符串。
        /// </summary>
		public string BaudRate => Lang.BaudRate;

        /// <summary>
        ///   查找类似 行为 的本地化字符串。
        /// </summary>
		public string Behavior => Lang.Behavior;

        /// <summary>
        ///   查找类似 吹 的本地化字符串。
        /// </summary>
		public string Blow => Lang.Blow;

        /// <summary>
        ///   查找类似 吹/关 的本地化字符串。
        /// </summary>
		public string BlowClose => Lang.BlowClose;

        /// <summary>
        ///   查找类似 吹DO 的本地化字符串。
        /// </summary>
		public string BlowDO => Lang.BlowDO;

        /// <summary>
        ///   查找类似 厂商 的本地化字符串。
        /// </summary>
		public string Brand => Lang.Brand;

        /// <summary>
        ///   查找类似 亮度 的本地化字符串。
        /// </summary>
		public string Brightness => Lang.Brightness;

        /// <summary>
        ///   查找类似 浏览 的本地化字符串。
        /// </summary>
		public string Browse => Lang.Browse;

        /// <summary>
        ///   查找类似 按钮 的本地化字符串。
        /// </summary>
		public string Button => Lang.Button;

        /// <summary>
        ///   查找类似 按钮灯 的本地化字符串。
        /// </summary>
		public string ButtonLight => Lang.ButtonLight;

        /// <summary>
        ///   查找类似 计算 的本地化字符串。
        /// </summary>
		public string Calculate => Lang.Calculate;

        /// <summary>
        ///   查找类似 相机 的本地化字符串。
        /// </summary>
		public string Camera => Lang.Camera;

        /// <summary>
        ///   查找类似 相机配置 的本地化字符串。
        /// </summary>
		public string CameraConfigutation => Lang.CameraConfigutation;

        /// <summary>
        ///   查找类似 相机窗口 的本地化字符串。
        /// </summary>
		public string CameraWindow => Lang.CameraWindow;

        /// <summary>
        ///   查找类似 取消 的本地化字符串。
        /// </summary>
		public string Cancel => Lang.Cancel;

        /// <summary>
        ///   查找类似 通道 的本地化字符串。
        /// </summary>
		public string Channel => Lang.Channel;

        /// <summary>
        ///   查找类似 通道数 的本地化字符串。
        /// </summary>
		public string ChannelCount => Lang.ChannelCount;

        /// <summary>
        ///   查找类似 点检记录 的本地化字符串。
        /// </summary>
		public string CheckRecord => Lang.CheckRecord;

        /// <summary>
        ///   查找类似 点检结果 的本地化字符串。
        /// </summary>
		public string CheckResult => Lang.CheckResult;

        /// <summary>
        ///   查找类似 点检时间 的本地化字符串。
        /// </summary>
		public string CheckTime => Lang.CheckTime;

        /// <summary>
        ///   查找类似 一圈移动量 的本地化字符串。
        /// </summary>
		public string CircleDisplacement => Lang.CircleDisplacement;

        /// <summary>
        ///   查找类似 清零 的本地化字符串。
        /// </summary>
		public string Clear => Lang.Clear;

        /// <summary>
        ///   查找类似 清错 的本地化字符串。
        /// </summary>
		public string ClearError => Lang.ClearError;

        /// <summary>
        ///   查找类似 关闭 的本地化字符串。
        /// </summary>
		public string Close => Lang.Close;

        /// <summary>
        ///   查找类似 采集行数 的本地化字符串。
        /// </summary>
		public string CollectRows => Lang.CollectRows;

        /// <summary>
        ///   查找类似 串口名称 的本地化字符串。
        /// </summary>
		public string Comm => Lang.Comm;

        /// <summary>
        ///   查找类似 指令脉冲 的本地化字符串。
        /// </summary>
		public string CommandPulse => Lang.CommandPulse;

        /// <summary>
        ///   查找类似 通信方式 的本地化字符串。
        /// </summary>
		public string CommType => Lang.CommType;

        /// <summary>
        ///   查找类似 通讯地址 的本地化字符串。
        /// </summary>
		public string Communcation => Lang.Communcation;

        /// <summary>
        ///   查找类似 通讯行为 的本地化字符串。
        /// </summary>
		public string CommuncationAction => Lang.CommuncationAction;

        /// <summary>
        ///   查找类似 通讯地址 的本地化字符串。
        /// </summary>
		public string CommuncationAddress => Lang.CommuncationAddress;

        /// <summary>
        ///   查找类似 通信调试 的本地化字符串。
        /// </summary>
		public string CommuncationTest => Lang.CommuncationTest;

        /// <summary>
        ///   查找类似 通讯方式 的本地化字符串。
        /// </summary>
		public string CommuncationType => Lang.CommuncationType;

        /// <summary>
        ///   查找类似 连接 的本地化字符串。
        /// </summary>
		public string Connect => Lang.Connect;

        /// <summary>
        ///   查找类似 持续发送 的本地化字符串。
        /// </summary>
		public string ContinuousSend => Lang.ContinuousSend;

        /// <summary>
        ///   查找类似 坐标系 的本地化字符串。
        /// </summary>
		public string CoordinateSystem => Lang.CoordinateSystem;

        /// <summary>
        ///   查找类似 数量 的本地化字符串。
        /// </summary>
		public string Count => Lang.Count;

        /// <summary>
        ///   查找类似 创建ROI 的本地化字符串。
        /// </summary>
		public string CreateROI => Lang.CreateROI;

        /// <summary>
        ///   查找类似 当前位置 的本地化字符串。
        /// </summary>
		public string CurrentPosition => Lang.CurrentPosition;

        /// <summary>
        ///   查找类似 当前里程 的本地化字符串。
        /// </summary>
		public string CurrentScale => Lang.CurrentScale;

        /// <summary>
        ///   查找类似 当前值 的本地化字符串。
        /// </summary>
		public string CurrentValue => Lang.CurrentValue;

        /// <summary>
        ///   查找类似 气缸类型 的本地化字符串。
        /// </summary>
		public string CylinderType => Lang.CylinderType;

        /// <summary>
        ///   查找类似 数据位 的本地化字符串。
        /// </summary>
		public string DataBits => Lang.DataBits;

        /// <summary>
        ///   查找类似 数据触发 的本地化字符串。
        /// </summary>
		public string DataTrigger => Lang.DataTrigger;

        /// <summary>
        ///   查找类似 数据类型 的本地化字符串。
        /// </summary>
		public string DataType => Lang.DataType;

        /// <summary>
        ///   查找类似 数据值 的本地化字符串。
        /// </summary>
		public string DataValue => Lang.DataValue;

        /// <summary>
        ///   查找类似 减速度(mm²) 的本地化字符串。
        /// </summary>
		public string Dcc => Lang.Dcc;

        /// <summary>
        ///   查找类似 减速(mm/s²) 的本地化字符串。
        /// </summary>
		public string DccSpeed => Lang.DccSpeed;

        /// <summary>
        ///   查找类似 默认值 的本地化字符串。
        /// </summary>
		public string Default => Lang.Default;

        /// <summary>
        ///   查找类似 延迟(ms) 的本地化字符串。
        /// </summary>
		public string Delay => Lang.Delay;

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public string Delete => Lang.Delete;

        /// <summary>
        ///   查找类似 检知 的本地化字符串。
        /// </summary>
		public string Detector => Lang.Detector;

        /// <summary>
        ///   查找类似 硬件设备 的本地化字符串。
        /// </summary>
		public string Device => Lang.Device;

        /// <summary>
        ///   查找类似 设备列表 的本地化字符串。
        /// </summary>
		public string DeviceList => Lang.DeviceList;

        /// <summary>
        ///   查找类似 设备管理 的本地化字符串。
        /// </summary>
		public string DeviceManagement => Lang.DeviceManagement;

        /// <summary>
        ///   查找类似 设备名称 的本地化字符串。
        /// </summary>
		public string DeviceName => Lang.DeviceName;

        /// <summary>
        ///   查找类似 距离(mm) 的本地化字符串。
        /// </summary>
		public string Diatance => Lang.Diatance;

        /// <summary>
        ///   查找类似 数字输入 的本地化字符串。
        /// </summary>
		public string DigitalIn => Lang.DigitalIn;

        /// <summary>
        ///   查找类似 数字量输入输出 的本地化字符串。
        /// </summary>
		public string DigitalIO => Lang.DigitalIO;

        /// <summary>
        ///   查找类似 数字输出 的本地化字符串。
        /// </summary>
		public string DigitalOut => Lang.DigitalOut;

        /// <summary>
        ///   查找类似 去使能 的本地化字符串。
        /// </summary>
		public string Disable => Lang.Disable;

        /// <summary>
        ///   查找类似 显示类型 的本地化字符串。
        /// </summary>
		public string DisplayType => Lang.DisplayType;

        /// <summary>
        ///   查找类似 双控 的本地化字符串。
        /// </summary>
		public string DoubleControl => Lang.DoubleControl;

        /// <summary>
        ///   查找类似 下移 的本地化字符串。
        /// </summary>
		public string Down => Lang.Down;

        /// <summary>
        ///   查找类似 去使能 的本地化字符串。
        /// </summary>
		public string DownEnable => Lang.DownEnable;

        /// <summary>
        ///   查找类似 拖拽 的本地化字符串。
        /// </summary>
		public string Drag => Lang.Drag;

        /// <summary>
        ///   查找类似 编辑 的本地化字符串。
        /// </summary>
		public string Edit => Lang.Edit;

        /// <summary>
        ///   查找类似 第八个 IO 的本地化字符串。
        /// </summary>
		public string EighthIO => Lang.EighthIO;

        /// <summary>
        ///   查找类似 急停 的本地化字符串。
        /// </summary>
		public string EmergencyStop => Lang.EmergencyStop;

        /// <summary>
        ///   查找类似 清空 的本地化字符串。
        /// </summary>
		public string Empty => Lang.Empty;

        /// <summary>
        ///   查找类似 使能 的本地化字符串。
        /// </summary>
		public string Enabled => Lang.Enabled;

        /// <summary>
        ///   查找类似 编码器通道号 的本地化字符串。
        /// </summary>
		public string EncoderNo => Lang.EncoderNo;

        /// <summary>
        ///   查找类似 编码器类型 的本地化字符串。
        /// </summary>
		public string EncoderType => Lang.EncoderType;

        /// <summary>
        ///   查找类似 字节顺序 的本地化字符串。
        /// </summary>
		public string EndianType => Lang.EndianType;

        /// <summary>
        ///   查找类似 错误信息 的本地化字符串。
        /// </summary>
		public string ErrorMessage => Lang.ErrorMessage;

        /// <summary>
        ///   查找类似 Excel文件 的本地化字符串。
        /// </summary>
		public string ExcelFile => Lang.ExcelFile;

        /// <summary>
        ///   查找类似 导出 的本地化字符串。
        /// </summary>
		public string Export => Lang.Export;

        /// <summary>
        ///   查找类似 导出到文本 的本地化字符串。
        /// </summary>
		public string ExportToTxt => Lang.ExportToTxt;

        /// <summary>
        ///   查找类似 曝光时间 的本地化字符串。
        /// </summary>
		public string ExposureTime => Lang.ExposureTime;

        /// <summary>
        ///   查找类似 伸出 的本地化字符串。
        /// </summary>
		public string Extend => Lang.Extend;

        /// <summary>
        ///   查找类似 伸出检知 的本地化字符串。
        /// </summary>
		public string ExtendDetetor => Lang.ExtendDetetor;

        /// <summary>
        ///   查找类似 伸出DI 的本地化字符串。
        /// </summary>
		public string ExtendDI => Lang.ExtendDI;

        /// <summary>
        ///   查找类似 伸出DO 的本地化字符串。
        /// </summary>
		public string ExtendDO => Lang.ExtendDO;

        /// <summary>
        ///   查找类似 伸出名称 的本地化字符串。
        /// </summary>
		public string ExtendName => Lang.ExtendName;

        /// <summary>
        ///   查找类似 伸出状态 的本地化字符串。
        /// </summary>
		public string ExtendStatus => Lang.ExtendStatus;

        /// <summary>
        ///   查找类似 第五个 IO 的本地化字符串。
        /// </summary>
		public string FifthIO => Lang.FifthIO;

        /// <summary>
        ///   查找类似 完成 的本地化字符串。
        /// </summary>
		public string Finish => Lang.Finish;

        /// <summary>
        ///   查找类似 轴1 的本地化字符串。
        /// </summary>
		public string FirstAxis => Lang.FirstAxis;

        /// <summary>
        ///   查找类似 第一个 IO 的本地化字符串。
        /// </summary>
		public string FirstIO => Lang.FirstIO;

        /// <summary>
        ///   查找类似 飞拍模块 的本地化字符串。
        /// </summary>
		public string FlyingPhoto => Lang.FlyingPhoto;

        /// <summary>
        ///   查找类似 飞拍模块名称 的本地化字符串。
        /// </summary>
		public string FlyingPhotoName => Lang.FlyingPhotoName;

        /// <summary>
        ///   查找类似 正行程 的本地化字符串。
        /// </summary>
		public string ForwardTrip => Lang.ForwardTrip;

        /// <summary>
        ///   查找类似 轴4 的本地化字符串。
        /// </summary>
		public string FourthAxis => Lang.FourthAxis;

        /// <summary>
        ///   查找类似 第四个 IO 的本地化字符串。
        /// </summary>
		public string FourthIO => Lang.FourthIO;

        /// <summary>
        ///   查找类似 帧率 的本地化字符串。
        /// </summary>
		public string Frame => Lang.Frame;

        /// <summary>
        ///   查找类似 频率(ms) 的本地化字符串。
        /// </summary>
		public string Frequency => Lang.Frequency;

        /// <summary>
        ///   查找类似 齿轮比分母 的本地化字符串。
        /// </summary>
		public string GearRatioDenominator => Lang.GearRatioDenominator;

        /// <summary>
        ///   查找类似 齿轮比分子 的本地化字符串。
        /// </summary>
		public string GearRatioNumerator => Lang.GearRatioNumerator;

        /// <summary>
        ///   查找类似 取流 的本地化字符串。
        /// </summary>
		public string Grab => Lang.Grab;

        /// <summary>
        ///   查找类似 使用变速箱 的本地化字符串。
        /// </summary>
		public string HaveTransmission => Lang.HaveTransmission;

        /// <summary>
        ///   查找类似 回零高速 的本地化字符串。
        /// </summary>
		public string HighSpeed => Lang.HighSpeed;

        /// <summary>
        ///   查找类似 主页 的本地化字符串。
        /// </summary>
		public string Home => Lang.Home;

        /// <summary>
        ///   查找类似 回零设备 的本地化字符串。
        /// </summary>
		public string HomeDevice => Lang.HomeDevice;

        /// <summary>
        ///   查找类似 回零参数 的本地化字符串。
        /// </summary>
		public string HomeParas => Lang.HomeParas;

        /// <summary>
        ///   查找类似 回零类型 的本地化字符串。
        /// </summary>
		public string HomeType => Lang.HomeType;

        /// <summary>
        ///   查找类似 回零 的本地化字符串。
        /// </summary>
		public string HomeZero => Lang.HomeZero;

        /// <summary>
        ///   查找类似 导入PLC地址 的本地化字符串。
        /// </summary>
		public string ImportPlcAddress => Lang.ImportPlcAddress;

        /// <summary>
        ///   查找类似 序号 的本地化字符串。
        /// </summary>
		public string Index => Lang.Index;

        /// <summary>
        ///   查找类似 伸出状态 的本地化字符串。
        /// </summary>
		public string InExtendName => Lang.InExtendName;

        /// <summary>
        ///   查找类似 输入IO 名称 的本地化字符串。
        /// </summary>
		public string InIOName => Lang.InIOName;

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public string Inport => Lang.Inport;

        /// <summary>
        ///   查找类似 缩回状态 的本地化字符串。
        /// </summary>
		public string InRetractName => Lang.InRetractName;

        /// <summary>
        ///   查找类似 灰度图 的本地化字符串。
        /// </summary>
		public string IntensityImage => Lang.IntensityImage;

        /// <summary>
        ///   查找类似 IO 数量 的本地化字符串。
        /// </summary>
		public string IOCount => Lang.IOCount;

        /// <summary>
        ///   查找类似 IO监控 的本地化字符串。
        /// </summary>
		public string IOMontor => Lang.IOMontor;

        /// <summary>
        ///   查找类似 IO名称 的本地化字符串。
        /// </summary>
		public string IOName => Lang.IOName;

        /// <summary>
        ///   查找类似 IO类型 的本地化字符串。
        /// </summary>
		public string IOType => Lang.IOType;

        /// <summary>
        ///   查找类似 IP 地址 的本地化字符串。
        /// </summary>
		public string IPAddress => Lang.IPAddress;

        /// <summary>
        ///   查找类似 JOG 的本地化字符串。
        /// </summary>
		public string Jog => Lang.Jog;

        /// <summary>
        ///   查找类似 Jog速度 的本地化字符串。
        /// </summary>
		public string JogSpped => Lang.JogSpped;

        /// <summary>
        ///   查找类似 门型轨迹 的本地化字符串。
        /// </summary>
		public string Jump => Lang.Jump;

        /// <summary>
        ///   查找类似 关键参数配置 的本地化字符串。
        /// </summary>
		public string KeyParameterConfig => Lang.KeyParameterConfig;

        /// <summary>
        ///   查找类似 查找类似 {0} 的本地化字符串。 的本地化字符串。
        /// </summary>
		public string LangComment => Lang.LangComment;

        /// <summary>
        ///   查找类似 激光配置 的本地化字符串。
        /// </summary>
		public string LaserConfigutation => Lang.LaserConfigutation;

        /// <summary>
        ///   查找类似 线扫配置文件地址 的本地化字符串。
        /// </summary>
		public string LaserConfigutationPath => Lang.LaserConfigutationPath;

        /// <summary>
        ///   查找类似 激光数据触发模式 的本地化字符串。
        /// </summary>
		public string LaserDataTriggerTip => Lang.LaserDataTriggerTip;

        /// <summary>
        ///   查找类似 激光硬触发配置 的本地化字符串。
        /// </summary>
		public string LaserHardTrrigerConfigutation => Lang.LaserHardTrrigerConfigutation;

        /// <summary>
        ///   查找类似 激光 Job 的本地化字符串。
        /// </summary>
		public string LaserJob => Lang.LaserJob;

        /// <summary>
        ///   查找类似 激光名称 的本地化字符串。
        /// </summary>
		public string LaserName => Lang.LaserName;

        /// <summary>
        ///   查找类似 激光开始触发IO  的本地化字符串。
        /// </summary>
		public string LaserStartIO => Lang.LaserStartIO;

        /// <summary>
        ///   查找类似 激光开始触发模式 的本地化字符串。
        /// </summary>
		public string LaserStartTip => Lang.LaserStartTip;

        /// <summary>
        ///   查找类似 上次月保养时间 的本地化字符串。
        /// </summary>
		public string LastMonthMaintenance => Lang.LastMonthMaintenance;

        /// <summary>
        ///   查找类似 上次周保养时间 的本地化字符串。
        /// </summary>
		public string LastWeekMaintenance => Lang.LastWeekMaintenance;

        /// <summary>
        ///   查找类似 锁存通道 的本地化字符串。
        /// </summary>
		public string LCTNo => Lang.LCTNo;

        /// <summary>
        ///   查找类似 光源配置 的本地化字符串。
        /// </summary>
		public string LightConfigutation => Lang.LightConfigutation;

        /// <summary>
        ///   查找类似 光源控制器 的本地化字符串。
        /// </summary>
		public string LightController => Lang.LightController;

        /// <summary>
        ///   查找类似 光源熄灭延迟时间(ms) 的本地化字符串。
        /// </summary>
		public string LightDelayToOff => Lang.LightDelayToOff;

        /// <summary>
        ///   查找类似 线性插补 的本地化字符串。
        /// </summary>
		public string LinearComplement => Lang.LinearComplement;

        /// <summary>
        ///   查找类似 线激光 的本地化字符串。
        /// </summary>
		public string LineLaser => Lang.LineLaser;

        /// <summary>
        ///   查找类似 加载配置 的本地化字符串。
        /// </summary>
		public string Load => Lang.Load;

        /// <summary>
        ///   查找类似 定位区间(mm) 的本地化字符串。
        /// </summary>
		public string LocationRange => Lang.LocationRange;

        /// <summary>
        ///   查找类似 回零低速 的本地化字符串。
        /// </summary>
		public string LowSpeed => Lang.LowSpeed;

        /// <summary>
        ///   查找类似 设备保养 的本地化字符串。
        /// </summary>
		public string Maintain => Lang.Maintain;

        /// <summary>
        ///   查找类似 维护里程 的本地化字符串。
        /// </summary>
		public string MaintainScale => Lang.MaintainScale;

        /// <summary>
        ///   查找类似 保养 的本地化字符串。
        /// </summary>
		public string Maintenance => Lang.Maintenance;

        /// <summary>
        ///   查找类似 设备管理 的本地化字符串。
        /// </summary>
		public string Management => Lang.Management;

        /// <summary>
        ///   查找类似 轮廓图或者深度图 的本地化字符串。
        /// </summary>
		public string MapType => Lang.MapType;

        /// <summary>
        ///   查找类似 匹配 的本地化字符串。
        /// </summary>
		public string Match => Lang.Match;

        /// <summary>
        ///   查找类似 最大位置 的本地化字符串。
        /// </summary>
		public string MaxPosition => Lang.MaxPosition;

        /// <summary>
        ///   查找类似 最大行程(单位 mm) 的本地化字符串。
        /// </summary>
		public string MaxPositionWithUnit => Lang.MaxPositionWithUnit;

        /// <summary>
        ///   查找类似 最大里程 的本地化字符串。
        /// </summary>
		public string MaxScale => Lang.MaxScale;

        /// <summary>
        ///   查找类似 最小位置 的本地化字符串。
        /// </summary>
		public string MinPosition => Lang.MinPosition;

        /// <summary>
        ///   查找类似 最小行程(单位 mm) 的本地化字符串。
        /// </summary>
		public string MinPositionWithUnit => Lang.MinPositionWithUnit;

        /// <summary>
        ///   查找类似 ModbusRTU 的本地化字符串。
        /// </summary>
		public string ModbusRTU => Lang.ModbusRTU;

        /// <summary>
        ///   查找类似 修改 的本地化字符串。
        /// </summary>
		public string Modify => Lang.Modify;

        /// <summary>
        ///   查找类似 模块配置 的本地化字符串。
        /// </summary>
		public string ModuleConfig => Lang.ModuleConfig;

        /// <summary>
        ///   查找类似 模组名称 的本地化字符串。
        /// </summary>
		public string ModuleName => Lang.ModuleName;

        /// <summary>
        ///   查找类似 模组或者名称 的本地化字符串。
        /// </summary>
		public string ModuleOrName => Lang.ModuleOrName;

        /// <summary>
        ///   查找类似 监控设备 的本地化字符串。
        /// </summary>
		public string MonitorDevice => Lang.MonitorDevice;

        /// <summary>
        ///   查找类似 监控值 的本地化字符串。
        /// </summary>
		public string MonitorValue => Lang.MonitorValue;

        /// <summary>
        ///   查找类似 控制卡 的本地化字符串。
        /// </summary>
		public string MotionCard => Lang.MotionCard;

        /// <summary>
        ///   查找类似 运动前提 的本地化字符串。
        /// </summary>
		public string MotionPremise => Lang.MotionPremise;

        /// <summary>
        ///   查找类似 运动调试 的本地化字符串。
        /// </summary>
		public string MotionTest => Lang.MotionTest;

        /// <summary>
        ///   查找类似 运动 的本地化字符串。
        /// </summary>
		public string Move => Lang.Move;

        /// <summary>
        ///   查找类似 移动距离 的本地化字符串。
        /// </summary>
		public string MoveDistance => Lang.MoveDistance;

        /// <summary>
        ///   查找类似 运动前提 的本地化字符串。
        /// </summary>
		public string MovePrecondition => Lang.MovePrecondition;

        /// <summary>
        ///   查找类似 运动类型 的本地化字符串。
        /// </summary>
		public string MoveType => Lang.MoveType;

        /// <summary>
        ///   查找类似 名称 的本地化字符串。
        /// </summary>
		public string Name => Lang.Name;

        /// <summary>
        ///   查找类似 是否检知 的本地化字符串。
        /// </summary>
		public string NeedDetetor => Lang.NeedDetetor;

        /// <summary>
        ///   查找类似 是否回零 的本地化字符串。
        /// </summary>
		public string NeedHome => Lang.NeedHome;

        /// <summary>
        ///   查找类似 是否屏蔽 的本地化字符串。
        /// </summary>
		public string NeedShield => Lang.NeedShield;

        /// <summary>
        ///   查找类似 负行程 的本地化字符串。
        /// </summary>
		public string NegativeTrip => Lang.NegativeTrip;

        /// <summary>
        ///   查找类似 下一指令 的本地化字符串。
        /// </summary>
		public string NextCommand => Lang.NextCommand;

        /// <summary>
        ///   查找类似 常规参数 的本地化字符串。
        /// </summary>
		public string NormalParas => Lang.NormalParas;

        /// <summary>
        ///   查找类似 空字符 的本地化字符串。
        /// </summary>
		public string Null => Lang.Null;

        /// <summary>
        ///   查找类似 离线配置 的本地化字符串。
        /// </summary>
		public string OfflineConfigutation => Lang.OfflineConfigutation;

        /// <summary>
        ///   查找类似 离线图片 的本地化字符串。
        /// </summary>
		public string OfflineImage => Lang.OfflineImage;

        /// <summary>
        ///   查找类似 离线点云 的本地化字符串。
        /// </summary>
		public string OfflinePointCloud => Lang.OfflinePointCloud;

        /// <summary>
        ///   查找类似 打开 的本地化字符串。
        /// </summary>
		public string Open => Lang.Open;

        /// <summary>
        ///   查找类似 操作 的本地化字符串。
        /// </summary>
		public string Operation => Lang.Operation;

        /// <summary>
        ///   查找类似 其他配置 的本地化字符串。
        /// </summary>
		public string OtherConfigutation => Lang.OtherConfigutation;

        /// <summary>
        ///   查找类似 输出 IO 名称 的本地化字符串。
        /// </summary>
		public string OutIOName => Lang.OutIOName;

        /// <summary>
        ///   查找类似 指令报文 的本地化字符串。
        /// </summary>
		public string PacketData => Lang.PacketData;

        /// <summary>
        ///   查找类似 点位参数配置 的本地化字符串。
        /// </summary>
		public string PositionParameterConfig => Lang.PositionParameterConfig;

        /// <summary>
        ///   查找类似 奇偶校验位 的本地化字符串。
        /// </summary>
		public string Parity => Lang.Parity;

        /// <summary>
        ///   查找类似 单页数量 的本地化字符串。
        /// </summary>
		public string PerPageCount => Lang.PerPageCount;

        /// <summary>
        ///   查找类似 到位时间 的本地化字符串。
        /// </summary>
		public string PlaceTime => Lang.PlaceTime;

        /// <summary>
        ///   查找类似 PLC地址 的本地化字符串。
        /// </summary>
		public string PlcAddress => Lang.PlcAddress;

        /// <summary>
        ///   查找类似 请输入轴名称 的本地化字符串。
        /// </summary>
		public string PleaseInputAxisName => Lang.PleaseInputAxisName;

        /// <summary>
        ///   查找类似 请选择通信方式 的本地化字符串。
        /// </summary>
		public string PleaseSelectCommType => Lang.PleaseSelectCommType;

        /// <summary>
        ///   查找类似 点位 的本地化字符串。
        /// </summary>
		public string Point => Lang.Point;

        /// <summary>
        ///   查找类似 端口号 的本地化字符串。
        /// </summary>
		public string Port => Lang.Port;

        /// <summary>
        ///   查找类似 点位运动前提 的本地化字符串。
        /// </summary>
		public string PosionMotionPremise => Lang.PosionMotionPremise;

        /// <summary>
        ///   查找类似 位置(mm) 的本地化字符串。
        /// </summary>
		public string Position => Lang.Position;

        /// <summary>
        ///   查找类似 位置(pulse) 的本地化字符串。
        /// </summary>
		public string PositionPulse => Lang.PositionPulse;

        /// <summary>
        ///   查找类似 推压距离 的本地化字符串。
        /// </summary>
		public string PressDistance => Lang.PressDistance;

        /// <summary>
        ///   查找类似 压力曲线 的本地化字符串。
        /// </summary>
		public string PressDriver => Lang.PressDriver;

        /// <summary>
        ///   查找类似 推压值(N) 的本地化字符串。
        /// </summary>
		public string Pressure => Lang.Pressure;

        /// <summary>
        ///   查找类似 打印 的本地化字符串。
        /// </summary>
		public string Print => Lang.Print;

        /// <summary>
        ///   查找类似 打印机 的本地化字符串。
        /// </summary>
		public string Printer => Lang.Printer;

        /// <summary>
        ///   查找类似 优先级 的本地化字符串。
        /// </summary>
		public string Priority => Lang.Priority;

        /// <summary>
        ///   查找类似 使用进度 的本地化字符串。
        /// </summary>
		public string Process => Lang.Process;

        /// <summary>
        ///   查找类似 协议 的本地化字符串。
        /// </summary>
		public string Protocol => Lang.Protocol;

        /// <summary>
        ///   查找类似 脉冲比计算 的本地化字符串。
        /// </summary>
		public string PulseCal => Lang.PulseCal;

        /// <summary>
        ///   查找类似 到位脉冲 的本地化字符串。
        /// </summary>
		public string PulseCount => Lang.PulseCount;

        /// <summary>
        ///   查找类似 脉冲比 的本地化字符串。
        /// </summary>
		public string PulseRatio => Lang.PulseRatio;

        /// <summary>
        ///   查找类似 输出脉宽 的本地化字符串。
        /// </summary>
		public string PulseWidth => Lang.PulseWidth;

        /// <summary>
        ///   查找类似 输出赋值 的本地化字符串。
        /// </summary>
		public string PutValue => Lang.PutValue;

        /// <summary>
        ///   查找类似 读取 的本地化字符串。
        /// </summary>
		public string Read => Lang.Read;

        /// <summary>
        ///   查找类似 批量读取 的本地化字符串。
        /// </summary>
		public string ReadAll => Lang.ReadAll;

        /// <summary>
        ///   查找类似 读取数据 的本地化字符串。
        /// </summary>
		public string ReadData => Lang.ReadData;

        /// <summary>
        ///   查找类似 是否只读 的本地化字符串。
        /// </summary>
		public string ReadOnly => Lang.ReadOnly;

        /// <summary>
        ///   查找类似 读写类型 的本地化字符串。
        /// </summary>
		public string ReadOrWrite => Lang.ReadOrWrite;

        /// <summary>
        ///   查找类似 相对运动 的本地化字符串。
        /// </summary>
		public string RelativeMove => Lang.RelativeMove;

        /// <summary>
        ///   查找类似 缩回 的本地化字符串。
        /// </summary>
		public string Retract => Lang.Retract;

        /// <summary>
        ///   查找类似 缩回检知 的本地化字符串。
        /// </summary>
		public string RetractDetetor => Lang.RetractDetetor;

        /// <summary>
        ///   查找类似 缩回DI 的本地化字符串。
        /// </summary>
		public string RetractDI => Lang.RetractDI;

        /// <summary>
        ///   查找类似 缩回DO 的本地化字符串。
        /// </summary>
		public string RetractDO => Lang.RetractDO;

        /// <summary>
        ///   查找类似 缩回名称 的本地化字符串。
        /// </summary>
		public string RetractName => Lang.RetractName;

        /// <summary>
        ///   查找类似 缩回状态 的本地化字符串。
        /// </summary>
		public string RetractStatus => Lang.RetractStatus;

        /// <summary>
        ///   查找类似 机器人 的本地化字符串。
        /// </summary>
		public string Robot => Lang.Robot;

        /// <summary>
        ///   查找类似 机器人名称 的本地化字符串。
        /// </summary>
		public string RobotName => Lang.RobotName;

        /// <summary>
        ///   查找类似 机器人运动模式 的本地化字符串。
        /// </summary>
		public string RobotRunModel => Lang.RobotRunModel;

        /// <summary>
        ///   查找类似 机器人运动模式选择 的本地化字符串。
        /// </summary>
		public string RobotRunModelTip => Lang.RobotRunModelTip;

        /// <summary>
        ///   查找类似 ROI高度 的本地化字符串。
        /// </summary>
		public string ROIHeight => Lang.ROIHeight;

        /// <summary>
        ///   查找类似 ROI开始X 的本地化字符串。
        /// </summary>
		public string ROIStartX => Lang.ROIStartX;

        /// <summary>
        ///   查找类似 ROI开始Y 的本地化字符串。
        /// </summary>
		public string ROIStartY => Lang.ROIStartY;

        /// <summary>
        ///   查找类似 ROI宽度 的本地化字符串。
        /// </summary>
		public string ROIWidth => Lang.ROIWidth;

        /// <summary>
        ///   查找类似 运行速度 的本地化字符串。
        /// </summary>
		public string RunSpeed => Lang.RunSpeed;

        /// <summary>
        ///   查找类似 安全设备 的本地化字符串。
        /// </summary>
		public string SafeDevice => Lang.SafeDevice;

        /// <summary>
        ///   查找类似 安全门 的本地化字符串。
        /// </summary>
		public string SafeDoor => Lang.SafeDoor;

        /// <summary>
        ///   查找类似 保存 的本地化字符串。
        /// </summary>
		public string Save => Lang.Save;

        /// <summary>
        ///   查找类似 轴2 的本地化字符串。
        /// </summary>
		public string SecondAxis => Lang.SecondAxis;

        /// <summary>
        ///   查找类似 第二个 IO 的本地化字符串。
        /// </summary>
		public string SecondIO => Lang.SecondIO;

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public string Select => Lang.Select;

        /// <summary>
        ///   查找类似 请选择运动轴 的本地化字符串。
        /// </summary>
		public string SelectAxis => Lang.SelectAxis;

        /// <summary>
        ///   查找类似 请选择厂商 的本地化字符串。
        /// </summary>
		public string SelectBrand => Lang.SelectBrand;

        /// <summary>
        ///   查找类似 请选择编码器 的本地化字符串。
        /// </summary>
		public string SelectEncoderTip => Lang.SelectEncoderTip;

        /// <summary>
        ///   查找类似 请选择Excel文件 的本地化字符串。
        /// </summary>
		public string SelectExcelFile => Lang.SelectExcelFile;

        /// <summary>
        ///   查找类似 请选择当前激光配置 的本地化字符串。
        /// </summary>
		public string SelectLaserConfigutation => Lang.SelectLaserConfigutation;

        /// <summary>
        ///   查找类似 选择线扫配置文件地址 的本地化字符串。
        /// </summary>
		public string SelectLaserConfigutationPath => Lang.SelectLaserConfigutationPath;

        /// <summary>
        ///   查找类似 请选择运动控制卡 的本地化字符串。
        /// </summary>
		public string SelectMotinCard => Lang.SelectMotinCard;

        /// <summary>
        ///   查找类似 请选择离线图片 的本地化字符串。
        /// </summary>
		public string SelectOfflineImage => Lang.SelectOfflineImage;

        /// <summary>
        ///   查找类似 请选择离线点云 的本地化字符串。
        /// </summary>
		public string SelectOfflinePointCloud => Lang.SelectOfflinePointCloud;

        /// <summary>
        ///   查找类似 请选择对应的安全设备 的本地化字符串。
        /// </summary>
		public string SelectSafeDevice => Lang.SelectSafeDevice;

        /// <summary>
        ///   查找类似 发送报文 的本地化字符串。
        /// </summary>
		public string SendPacket => Lang.SendPacket;

        /// <summary>
        ///   查找类似 序列号 的本地化字符串。
        /// </summary>
		public string SerialNumber => Lang.SerialNumber;

        /// <summary>
        ///   查找类似 设置 的本地化字符串。
        /// </summary>
		public string Set => Lang.Set;

        /// <summary>
        ///   查找类似 第七个 IO 的本地化字符串。
        /// </summary>
		public string SeventhIO => Lang.SeventhIO;

        /// <summary>
        ///   查找类似 屏蔽设备 的本地化字符串。
        /// </summary>
		public string ShieldDevice => Lang.ShieldDevice;

        /// <summary>
        ///   查找类似 存图 的本地化字符串。
        /// </summary>
		public string Shot => Lang.Shot;

        /// <summary>
        ///   查找类似 显示点云 的本地化字符串。
        /// </summary>
		public string ShowPointCloud => Lang.ShowPointCloud;

        /// <summary>
        ///   查找类似 模拟赋值 的本地化字符串。
        /// </summary>
		public string SimulationValue => Lang.SimulationValue;

        /// <summary>
        ///   查找类似 单控 的本地化字符串。
        /// </summary>
		public string SingleControl => Lang.SingleControl;

        /// <summary>
        ///   查找类似 第六个 IO 的本地化字符串。
        /// </summary>
		public string SixthIO => Lang.SixthIO;

        /// <summary>
        ///   查找类似 从站名称 的本地化字符串。
        /// </summary>
		public string SlaveName => Lang.SlaveName;

        /// <summary>
        ///   查找类似 从站号 的本地化字符串。
        /// </summary>
		public string SlaveNo => Lang.SlaveNo;

        /// <summary>
        ///   查找类似 PC插槽 的本地化字符串。
        /// </summary>
		public string Slot => Lang.Slot;

        /// <summary>
        ///   查找类似 Socket类型 的本地化字符串。
        /// </summary>
		public string SocketType => Lang.SocketType;

        /// <summary>
        ///   查找类似 速度(mm) 的本地化字符串。
        /// </summary>
		public string Speed => Lang.Speed;

        /// <summary>
        ///   查找类似 速度系数 的本地化字符串。
        /// </summary>
		public string SpeedCoefficient => Lang.SpeedCoefficient;

        /// <summary>
        ///   查找类似 真实速度=速度 * 系数 的本地化字符串。
        /// </summary>
		public string SpeedTip => Lang.SpeedTip;

        /// <summary>
        ///   查找类似 开始地址 的本地化字符串。
        /// </summary>
		public string StartAddress => Lang.StartAddress;

        /// <summary>
        ///   查找类似 开始点检 的本地化字符串。
        /// </summary>
		public string StartCheck => Lang.StartCheck;

        /// <summary>
        ///   查找类似 起始编号 的本地化字符串。
        /// </summary>
		public string StartIndex => Lang.StartIndex;

        /// <summary>
        ///   查找类似 开始IO 的本地化字符串。
        /// </summary>
		public string StartIO => Lang.StartIO;

        /// <summary>
        ///   查找类似 开始触发 的本地化字符串。
        /// </summary>
		public string StartTrigger => Lang.StartTrigger;

        /// <summary>
        ///   查找类似 站点 的本地化字符串。
        /// </summary>
		public string StationNO => Lang.StationNO;

        /// <summary>
        ///   查找类似 状态 的本地化字符串。
        /// </summary>
		public string Status => Lang.Status;

        /// <summary>
        ///   查找类似 停止 的本地化字符串。
        /// </summary>
		public string Stop => Lang.Stop;

        /// <summary>
        ///   查找类似 停止位 的本地化字符串。
        /// </summary>
		public string StopBits => Lang.StopBits;

        /// <summary>
        ///   查找类似 子定义 的本地化字符串。
        /// </summary>
		public string SubDefinite => Lang.SubDefinite;

        /// <summary>
        ///   查找类似 吸 的本地化字符串。
        /// </summary>
		public string Suck => Lang.Suck;

        /// <summary>
        ///   查找类似 吸DO 的本地化字符串。
        /// </summary>
		public string SuckDO => Lang.SuckDO;

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public string Sure => Lang.Sure;

        /// <summary>
        ///   查找类似 标签高度 的本地化字符串。
        /// </summary>
		public string TagHeight => Lang.TagHeight;

        /// <summary>
        ///   查找类似 标签宽度 的本地化字符串。
        /// </summary>
		public string TagWidth => Lang.TagWidth;

        /// <summary>
        ///   查找类似 泰科统计 的本地化字符串。
        /// </summary>
		public string TaikeContent => Lang.TaikeContent;

        /// <summary>
        ///   查找类似 泰科曲线 的本地化字符串。
        /// </summary>
		public string TaikeCurve => Lang.TaikeCurve;

        /// <summary>
        ///   查找类似 示教 的本地化字符串。
        /// </summary>
		public string Teach => Lang.Teach;

        /// <summary>
        ///   查找类似 轴3 的本地化字符串。
        /// </summary>
		public string ThirdAxis => Lang.ThirdAxis;

        /// <summary>
        ///   查找类似 第三个 IO 的本地化字符串。
        /// </summary>
		public string ThirdIO => Lang.ThirdIO;

        /// <summary>
        ///   查找类似 标题 的本地化字符串。
        /// </summary>
		public string Title => Lang.Title;

        /// <summary>
        ///   查找类似 触发次数 的本地化字符串。
        /// </summary>
		public string TriggerCount => Lang.TriggerCount;

        /// <summary>
        ///   查找类似 触发Fifo数量 的本地化字符串。
        /// </summary>
		public string TriggerFifoCount => Lang.TriggerFifoCount;

        /// <summary>
        ///   查找类似 触发输出通道 的本地化字符串。
        /// </summary>
		public string TriggerNo => Lang.TriggerNo;

        /// <summary>
        ///   查找类似 触发输出通道计数 的本地化字符串。
        /// </summary>
		public string TriggerNoCount => Lang.TriggerNoCount;

        /// <summary>
        ///   查找类似 触发通道类型 的本地化字符串。
        /// </summary>
		public string TriggerNoType => Lang.TriggerNoType;

        /// <summary>
        ///   查找类似 触发脉冲百分比 的本地化字符串。
        /// </summary>
		public string TriggerPluseRatio => Lang.TriggerPluseRatio;

        /// <summary>
        ///   查找类似 读取Txt 的本地化字符串。
        /// </summary>
		public string TxtReader => Lang.TxtReader;

        /// <summary>
        ///   查找类似 未监控设备(双击添加监控) 的本地化字符串。
        /// </summary>
		public string UnMonitorDevice => Lang.UnMonitorDevice;

        /// <summary>
        ///   查找类似 上移 的本地化字符串。
        /// </summary>
		public string Up => Lang.Up;

        /// <summary>
        ///   查找类似 更新 的本地化字符串。
        /// </summary>
		public string Update => Lang.Update;

        /// <summary>
        ///   查找类似 上使能 的本地化字符串。
        /// </summary>
		public string UppperEnable => Lang.UppperEnable;

        /// <summary>
        ///   查找类似 USB口 的本地化字符串。
        /// </summary>
		public string USB => Lang.USB;

        /// <summary>
        ///   查找类似 已使用 的本地化字符串。
        /// </summary>
		public string Used => Lang.Used;

        /// <summary>
        ///   查找类似 真空检知 的本地化字符串。
        /// </summary>
		public string VacuumSensor => Lang.VacuumSensor;

        /// <summary>
        ///   查找类似 真空检知DI 的本地化字符串。
        /// </summary>
		public string VacuumSensorDI => Lang.VacuumSensorDI;

        /// <summary>
        ///   查找类似 配置值 的本地化字符串。
        /// </summary>
		public string Value => Lang.Value;

        /// <summary>
        ///   查找类似 仿真轴 的本地化字符串。
        /// </summary>
		public string VAxis => Lang.VAxis;

        /// <summary>
        ///   查找类似 仿真轴3 的本地化字符串。
        /// </summary>
		public string VAxis3 => Lang.VAxis3;

        /// <summary>
        ///   查找类似 轴列表(双击显示) 的本地化字符串。
        /// </summary>
		public string VAxisIOAxisListTip => Lang.VAxisIOAxisListTip;

        /// <summary>
        ///   查找类似 轴列表(双击添加) 的本地化字符串。
        /// </summary>
		public string VAxisListTip => Lang.VAxisListTip;

        /// <summary>
        ///   查找类似 仿真多轴 的本地化字符串。
        /// </summary>
		public string VAxisM => Lang.VAxisM;

        /// <summary>
        ///   查找类似 仿真皮带 的本地化字符串。
        /// </summary>
		public string VBelt => Lang.VBelt;

        /// <summary>
        ///   查找类似 仿真相机 的本地化字符串。
        /// </summary>
		public string VCamera => Lang.VCamera;

        /// <summary>
        ///   查找类似 仿真通信 的本地化字符串。
        /// </summary>
		public string VCommuncation => Lang.VCommuncation;

        /// <summary>
        ///   查找类似 仿真气缸 的本地化字符串。
        /// </summary>
		public string VCylinder => Lang.VCylinder;

        /// <summary>
        ///   查找类似 虚拟设备 的本地化字符串。
        /// </summary>
		public string VDevice => Lang.VDevice;

        /// <summary>
        ///   查找类似 仿真电批 的本地化字符串。
        /// </summary>
		public string VESD => Lang.VESD;

        /// <summary>
        ///   查找类似 仿真飞达 的本地化字符串。
        /// </summary>
		public string VFeeder => Lang.VFeeder;

        /// <summary>
        ///   查找类似 仿真飞拍 的本地化字符串。
        /// </summary>
		public string VFlyingPhoto => Lang.VFlyingPhoto;

        /// <summary>
        ///   查找类似 仿真输入 的本地化字符串。
        /// </summary>
		public string VInputIO => Lang.VInputIO;

        /// <summary>
        ///   查找类似 仿真IO 的本地化字符串。
        /// </summary>
		public string VIO => Lang.VIO;

        /// <summary>
        ///   查找类似 IO仿真设备 的本地化字符串。
        /// </summary>
		public string VIOSimulation => Lang.VIOSimulation;

        /// <summary>
        ///   查找类似 Vision过程参数 的本地化字符串。
        /// </summary>
		public string VisionProcessData => Lang.VisionProcessData;

        /// <summary>
        ///   查找类似 仿真线激光 的本地化字符串。
        /// </summary>
		public string VLineLaser => Lang.VLineLaser;

        /// <summary>
        ///   查找类似 仿真电缸 的本地化字符串。
        /// </summary>
		public string VMCylinder => Lang.VMCylinder;

        /// <summary>
        ///   查找类似 仿真输出 的本地化字符串。
        /// </summary>
		public string VOutputIO => Lang.VOutputIO;

        /// <summary>
        ///   查找类似 力矩电缸 的本地化字符串。
        /// </summary>
		public string VPCylinder => Lang.VPCylinder;

        /// <summary>
        ///   查找类似 仿真PLC 的本地化字符串。
        /// </summary>
		public string VPlc => Lang.VPlc;

        /// <summary>
        ///   查找类似 仿真打印机 的本地化字符串。
        /// </summary>
		public string VPrinter => Lang.VPrinter;

        /// <summary>
        ///   查找类似 仿真机器人 的本地化字符串。
        /// </summary>
		public string VRobot => Lang.VRobot;

        /// <summary>
        ///   查找类似 仿真SerialPort 的本地化字符串。
        /// </summary>
		public string VSerialPort => Lang.VSerialPort;

        /// <summary>
        ///   查找类似 仿真Socket 的本地化字符串。
        /// </summary>
		public string VSocket => Lang.VSocket;

        /// <summary>
        ///   查找类似 仿真三色灯 的本地化字符串。
        /// </summary>
		public string VTricolorlamp => Lang.VTricolorlamp;

        /// <summary>
        ///   查找类似 仿真真空 的本地化字符串。
        /// </summary>
		public string VVacuum => Lang.VVacuum;

        /// <summary>
        ///   查找类似 写入 的本地化字符串。
        /// </summary>
		public string Write => Lang.Write;

        /// <summary>
        ///   查找类似 写入数据 的本地化字符串。
        /// </summary>
		public string WriteData => Lang.WriteData;

        /// <summary>
        ///   查找类似 写入报文 的本地化字符串。
        /// </summary>
		public string WritePacket => Lang.WritePacket;

        /// <summary>
        ///   查找类似 写入值 的本地化字符串。
        /// </summary>
		public string WriteValue => Lang.WriteValue;

        /// <summary>
        ///   查找类似 鑫精诚 的本地化字符串。
        /// </summary>
		public string XJCPressureSensor => Lang.XJCPressureSensor;

        /// <summary>
        ///   查找类似 零点偏移 的本地化字符串。
        /// </summary>
		public string ZeroOffset => Lang.ZeroOffset;


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class LangKeys
    {
        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string _2_5DDepthMap = nameof(_2_5DDepthMap);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string _2DSkeletonMap = nameof(_2DSkeletonMap);

        /// <summary>
        ///   查找类似 绝对运动 的本地化字符串。
        /// </summary>
		public static string AbsoluteMove = nameof(AbsoluteMove);

        /// <summary>
        ///   查找类似 加速度(mm²) 的本地化字符串。
        /// </summary>
		public static string Acc = nameof(Acc);

        /// <summary>
        ///   查找类似 加速(mm/s²) 的本地化字符串。
        /// </summary>
		public static string AccSpeed = nameof(AccSpeed);

        /// <summary>
        ///   查找类似 动作配置 的本地化字符串。
        /// </summary>
		public static string ActionArea = nameof(ActionArea);

        /// <summary>
        ///   查找类似 动作名称 的本地化字符串。
        /// </summary>
		public static string ActionName = nameof(ActionName);

        /// <summary>
        ///   查找类似 动作类型 的本地化字符串。
        /// </summary>
		public static string ActionType = nameof(ActionType);

        /// <summary>
        ///   查找类似 添加 的本地化字符串。
        /// </summary>
		public static string Add = nameof(Add);

        /// <summary>
        ///   查找类似 添加地址 的本地化字符串。
        /// </summary>
		public static string AddAddress = nameof(AddAddress);

        /// <summary>
        ///   查找类似 地址 的本地化字符串。
        /// </summary>
		public static string Address = nameof(Address);

        /// <summary>
        ///   查找类似 地址数量 的本地化字符串。
        /// </summary>
		public static string AddressCount = nameof(AddressCount);

        /// <summary>
        ///   查找类似 地址描述 的本地化字符串。
        /// </summary>
		public static string AddressDescription = nameof(AddressDescription);

        /// <summary>
        ///   查找类似 地址列表 的本地化字符串。
        /// </summary>
		public static string AddressList = nameof(AddressList);

        /// <summary>
        ///   查找类似 报警代码 的本地化字符串。
        /// </summary>
		public static string AlarmCode = nameof(AlarmCode);

        /// <summary>
        ///   查找类似 报警配置 的本地化字符串。
        /// </summary>
		public static string AlarmConfig = nameof(AlarmConfig);

        /// <summary>
        ///   查找类似 自定义报警配置 的本地化字符串。
        /// </summary>
		public static string AlarmConfigCustom = nameof(AlarmConfigCustom);

        /// <summary>
        ///   查找类似 模拟输入 的本地化字符串。
        /// </summary>
		public static string AnalogIn = nameof(AnalogIn);

        /// <summary>
        ///   查找类似 模拟量输入输出 的本地化字符串。
        /// </summary>
		public static string AnalogIO = nameof(AnalogIO);

        /// <summary>
        ///   查找类似 模拟输出 的本地化字符串。
        /// </summary>
		public static string AnalogOut = nameof(AnalogOut);

        /// <summary>
        ///   查找类似 自动扫描 的本地化字符串。
        /// </summary>
		public static string AutoScan = nameof(AutoScan);

        /// <summary>
        ///   查找类似 轴 的本地化字符串。
        /// </summary>
		public static string Axis = nameof(Axis);

        /// <summary>
        ///   查找类似 轴1 的本地化字符串。
        /// </summary>
		public static string Axis1 = nameof(Axis1);

        /// <summary>
        ///   查找类似 轴数量 的本地化字符串。
        /// </summary>
		public static string AxisCount = nameof(AxisCount);

        /// <summary>
        ///   查找类似 正方向 的本地化字符串。
        /// </summary>
		public static string AxisForward = nameof(AxisForward);

        /// <summary>
        ///   查找类似 轴点位调试 的本地化字符串。
        /// </summary>
		public static string AxisIODebug = nameof(AxisIODebug);

        /// <summary>
        ///   查找类似 轴运动前提 的本地化字符串。
        /// </summary>
		public static string AxisMotionPremise = nameof(AxisMotionPremise);

        /// <summary>
        ///   查找类似 轴号 的本地化字符串。
        /// </summary>
		public static string AxisNo = nameof(AxisNo);

        /// <summary>
        ///   查找类似 型号 的本地化字符串。
        /// </summary>
		public static string AxisPML = nameof(AxisPML);

        /// <summary>
        ///   查找类似 轴点位配置 的本地化字符串。
        /// </summary>
		public static string AxisPosConfig = nameof(AxisPosConfig);

        /// <summary>
        ///   查找类似 位置 的本地化字符串。
        /// </summary>
		public static string AxisPosition = nameof(AxisPosition);

        /// <summary>
        ///   查找类似 轴类型 的本地化字符串。
        /// </summary>
		public static string AxisType = nameof(AxisType);

        /// <summary>
        ///   查找类似 批量导入 的本地化字符串。
        /// </summary>
		public static string BatchImport = nameof(BatchImport);

        /// <summary>
        ///   查找类似 波特率 的本地化字符串。
        /// </summary>
		public static string BaudRate = nameof(BaudRate);

        /// <summary>
        ///   查找类似 行为 的本地化字符串。
        /// </summary>
		public static string Behavior = nameof(Behavior);

        /// <summary>
        ///   查找类似 吹 的本地化字符串。
        /// </summary>
		public static string Blow = nameof(Blow);

        /// <summary>
        ///   查找类似 吹/关 的本地化字符串。
        /// </summary>
		public static string BlowClose = nameof(BlowClose);

        /// <summary>
        ///   查找类似 吹DO 的本地化字符串。
        /// </summary>
		public static string BlowDO = nameof(BlowDO);

        /// <summary>
        ///   查找类似 厂商 的本地化字符串。
        /// </summary>
		public static string Brand = nameof(Brand);

        /// <summary>
        ///   查找类似 亮度 的本地化字符串。
        /// </summary>
		public static string Brightness = nameof(Brightness);

        /// <summary>
        ///   查找类似 浏览 的本地化字符串。
        /// </summary>
		public static string Browse = nameof(Browse);

        /// <summary>
        ///   查找类似 按钮 的本地化字符串。
        /// </summary>
		public static string Button = nameof(Button);

        /// <summary>
        ///   查找类似 按钮灯 的本地化字符串。
        /// </summary>
		public static string ButtonLight = nameof(ButtonLight);

        /// <summary>
        ///   查找类似 计算 的本地化字符串。
        /// </summary>
		public static string Calculate = nameof(Calculate);

        /// <summary>
        ///   查找类似 相机 的本地化字符串。
        /// </summary>
		public static string Camera = nameof(Camera);

        /// <summary>
        ///   查找类似 相机配置 的本地化字符串。
        /// </summary>
		public static string CameraConfigutation = nameof(CameraConfigutation);

        /// <summary>
        ///   查找类似 相机窗口 的本地化字符串。
        /// </summary>
		public static string CameraWindow = nameof(CameraWindow);

        /// <summary>
        ///   查找类似 取消 的本地化字符串。
        /// </summary>
		public static string Cancel = nameof(Cancel);

        /// <summary>
        ///   查找类似 通道 的本地化字符串。
        /// </summary>
		public static string Channel = nameof(Channel);

        /// <summary>
        ///   查找类似 通道数 的本地化字符串。
        /// </summary>
		public static string ChannelCount = nameof(ChannelCount);

        /// <summary>
        ///   查找类似 点检记录 的本地化字符串。
        /// </summary>
		public static string CheckRecord = nameof(CheckRecord);

        /// <summary>
        ///   查找类似 点检结果 的本地化字符串。
        /// </summary>
		public static string CheckResult = nameof(CheckResult);

        /// <summary>
        ///   查找类似 点检时间 的本地化字符串。
        /// </summary>
		public static string CheckTime = nameof(CheckTime);

        /// <summary>
        ///   查找类似 一圈移动量 的本地化字符串。
        /// </summary>
		public static string CircleDisplacement = nameof(CircleDisplacement);

        /// <summary>
        ///   查找类似 清零 的本地化字符串。
        /// </summary>
		public static string Clear = nameof(Clear);

        /// <summary>
        ///   查找类似 清错 的本地化字符串。
        /// </summary>
		public static string ClearError = nameof(ClearError);

        /// <summary>
        ///   查找类似 关闭 的本地化字符串。
        /// </summary>
		public static string Close = nameof(Close);

        /// <summary>
        ///   查找类似 采集行数 的本地化字符串。
        /// </summary>
		public static string CollectRows = nameof(CollectRows);

        /// <summary>
        ///   查找类似 串口名称 的本地化字符串。
        /// </summary>
		public static string Comm = nameof(Comm);

        /// <summary>
        ///   查找类似 指令脉冲 的本地化字符串。
        /// </summary>
		public static string CommandPulse = nameof(CommandPulse);

        /// <summary>
        ///   查找类似 通信方式 的本地化字符串。
        /// </summary>
		public static string CommType = nameof(CommType);

        /// <summary>
        ///   查找类似 通讯地址 的本地化字符串。
        /// </summary>
		public static string Communcation = nameof(Communcation);

        /// <summary>
        ///   查找类似 通讯行为 的本地化字符串。
        /// </summary>
		public static string CommuncationAction = nameof(CommuncationAction);

        /// <summary>
        ///   查找类似 通讯地址 的本地化字符串。
        /// </summary>
		public static string CommuncationAddress = nameof(CommuncationAddress);

        /// <summary>
        ///   查找类似 通信调试 的本地化字符串。
        /// </summary>
		public static string CommuncationTest = nameof(CommuncationTest);

        /// <summary>
        ///   查找类似 通讯方式 的本地化字符串。
        /// </summary>
		public static string CommuncationType = nameof(CommuncationType);

        /// <summary>
        ///   查找类似 连接 的本地化字符串。
        /// </summary>
		public static string Connect = nameof(Connect);

        /// <summary>
        ///   查找类似 持续发送 的本地化字符串。
        /// </summary>
		public static string ContinuousSend = nameof(ContinuousSend);

        /// <summary>
        ///   查找类似 坐标系 的本地化字符串。
        /// </summary>
		public static string CoordinateSystem = nameof(CoordinateSystem);

        /// <summary>
        ///   查找类似 数量 的本地化字符串。
        /// </summary>
		public static string Count = nameof(Count);

        /// <summary>
        ///   查找类似 创建ROI 的本地化字符串。
        /// </summary>
		public static string CreateROI = nameof(CreateROI);

        /// <summary>
        ///   查找类似 当前位置 的本地化字符串。
        /// </summary>
		public static string CurrentPosition = nameof(CurrentPosition);

        /// <summary>
        ///   查找类似 当前里程 的本地化字符串。
        /// </summary>
		public static string CurrentScale = nameof(CurrentScale);

        /// <summary>
        ///   查找类似 当前值 的本地化字符串。
        /// </summary>
		public static string CurrentValue = nameof(CurrentValue);

        /// <summary>
        ///   查找类似 气缸类型 的本地化字符串。
        /// </summary>
		public static string CylinderType = nameof(CylinderType);

        /// <summary>
        ///   查找类似 数据位 的本地化字符串。
        /// </summary>
		public static string DataBits = nameof(DataBits);

        /// <summary>
        ///   查找类似 数据触发 的本地化字符串。
        /// </summary>
		public static string DataTrigger = nameof(DataTrigger);

        /// <summary>
        ///   查找类似 数据类型 的本地化字符串。
        /// </summary>
		public static string DataType = nameof(DataType);

        /// <summary>
        ///   查找类似 数据值 的本地化字符串。
        /// </summary>
		public static string DataValue = nameof(DataValue);

        /// <summary>
        ///   查找类似 减速度(mm²) 的本地化字符串。
        /// </summary>
		public static string Dcc = nameof(Dcc);

        /// <summary>
        ///   查找类似 减速(mm/s²) 的本地化字符串。
        /// </summary>
		public static string DccSpeed = nameof(DccSpeed);

        /// <summary>
        ///   查找类似 默认值 的本地化字符串。
        /// </summary>
		public static string Default = nameof(Default);

        /// <summary>
        ///   查找类似 延迟(ms) 的本地化字符串。
        /// </summary>
		public static string Delay = nameof(Delay);

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public static string Delete = nameof(Delete);

        /// <summary>
        ///   查找类似 检知 的本地化字符串。
        /// </summary>
		public static string Detector = nameof(Detector);

        /// <summary>
        ///   查找类似 硬件设备 的本地化字符串。
        /// </summary>
		public static string Device = nameof(Device);

        /// <summary>
        ///   查找类似 设备列表 的本地化字符串。
        /// </summary>
		public static string DeviceList = nameof(DeviceList);

        /// <summary>
        ///   查找类似 设备管理 的本地化字符串。
        /// </summary>
		public static string DeviceManagement = nameof(DeviceManagement);

        /// <summary>
        ///   查找类似 设备名称 的本地化字符串。
        /// </summary>
		public static string DeviceName = nameof(DeviceName);

        /// <summary>
        ///   查找类似 距离(mm) 的本地化字符串。
        /// </summary>
		public static string Diatance = nameof(Diatance);

        /// <summary>
        ///   查找类似 数字输入 的本地化字符串。
        /// </summary>
		public static string DigitalIn = nameof(DigitalIn);

        /// <summary>
        ///   查找类似 数字量输入输出 的本地化字符串。
        /// </summary>
		public static string DigitalIO = nameof(DigitalIO);

        /// <summary>
        ///   查找类似 数字输出 的本地化字符串。
        /// </summary>
		public static string DigitalOut = nameof(DigitalOut);

        /// <summary>
        ///   查找类似 去使能 的本地化字符串。
        /// </summary>
		public static string Disable = nameof(Disable);

        /// <summary>
        ///   查找类似 显示类型 的本地化字符串。
        /// </summary>
		public static string DisplayType = nameof(DisplayType);

        /// <summary>
        ///   查找类似 双控 的本地化字符串。
        /// </summary>
		public static string DoubleControl = nameof(DoubleControl);

        /// <summary>
        ///   查找类似 下移 的本地化字符串。
        /// </summary>
		public static string Down = nameof(Down);

        /// <summary>
        ///   查找类似 去使能 的本地化字符串。
        /// </summary>
		public static string DownEnable = nameof(DownEnable);

        /// <summary>
        ///   查找类似 拖拽 的本地化字符串。
        /// </summary>
		public static string Drag = nameof(Drag);

        /// <summary>
        ///   查找类似 编辑 的本地化字符串。
        /// </summary>
		public static string Edit = nameof(Edit);

        /// <summary>
        ///   查找类似 第八个 IO 的本地化字符串。
        /// </summary>
		public static string EighthIO = nameof(EighthIO);

        /// <summary>
        ///   查找类似 急停 的本地化字符串。
        /// </summary>
		public static string EmergencyStop = nameof(EmergencyStop);

        /// <summary>
        ///   查找类似 清空 的本地化字符串。
        /// </summary>
		public static string Empty = nameof(Empty);

        /// <summary>
        ///   查找类似 使能 的本地化字符串。
        /// </summary>
		public static string Enabled = nameof(Enabled);

        /// <summary>
        ///   查找类似 编码器通道号 的本地化字符串。
        /// </summary>
		public static string EncoderNo = nameof(EncoderNo);

        /// <summary>
        ///   查找类似 编码器类型 的本地化字符串。
        /// </summary>
		public static string EncoderType = nameof(EncoderType);

        /// <summary>
        ///   查找类似 字节顺序 的本地化字符串。
        /// </summary>
		public static string EndianType = nameof(EndianType);

        /// <summary>
        ///   查找类似 错误信息 的本地化字符串。
        /// </summary>
		public static string ErrorMessage = nameof(ErrorMessage);

        /// <summary>
        ///   查找类似 Excel文件 的本地化字符串。
        /// </summary>
		public static string ExcelFile = nameof(ExcelFile);

        /// <summary>
        ///   查找类似 导出 的本地化字符串。
        /// </summary>
		public static string Export = nameof(Export);

        /// <summary>
        ///   查找类似 导出到文本 的本地化字符串。
        /// </summary>
		public static string ExportToTxt = nameof(ExportToTxt);

        /// <summary>
        ///   查找类似 曝光时间 的本地化字符串。
        /// </summary>
		public static string ExposureTime = nameof(ExposureTime);

        /// <summary>
        ///   查找类似 伸出 的本地化字符串。
        /// </summary>
		public static string Extend = nameof(Extend);

        /// <summary>
        ///   查找类似 伸出检知 的本地化字符串。
        /// </summary>
		public static string ExtendDetetor = nameof(ExtendDetetor);

        /// <summary>
        ///   查找类似 伸出DI 的本地化字符串。
        /// </summary>
		public static string ExtendDI = nameof(ExtendDI);

        /// <summary>
        ///   查找类似 伸出DO 的本地化字符串。
        /// </summary>
		public static string ExtendDO = nameof(ExtendDO);

        /// <summary>
        ///   查找类似 伸出名称 的本地化字符串。
        /// </summary>
		public static string ExtendName = nameof(ExtendName);

        /// <summary>
        ///   查找类似 伸出状态 的本地化字符串。
        /// </summary>
		public static string ExtendStatus = nameof(ExtendStatus);

        /// <summary>
        ///   查找类似 第五个 IO 的本地化字符串。
        /// </summary>
		public static string FifthIO = nameof(FifthIO);

        /// <summary>
        ///   查找类似 完成 的本地化字符串。
        /// </summary>
		public static string Finish = nameof(Finish);

        /// <summary>
        ///   查找类似 轴1 的本地化字符串。
        /// </summary>
		public static string FirstAxis = nameof(FirstAxis);

        /// <summary>
        ///   查找类似 第一个 IO 的本地化字符串。
        /// </summary>
		public static string FirstIO = nameof(FirstIO);

        /// <summary>
        ///   查找类似 飞拍模块 的本地化字符串。
        /// </summary>
		public static string FlyingPhoto = nameof(FlyingPhoto);

        /// <summary>
        ///   查找类似 飞拍模块名称 的本地化字符串。
        /// </summary>
		public static string FlyingPhotoName = nameof(FlyingPhotoName);

        /// <summary>
        ///   查找类似 正行程 的本地化字符串。
        /// </summary>
		public static string ForwardTrip = nameof(ForwardTrip);

        /// <summary>
        ///   查找类似 轴4 的本地化字符串。
        /// </summary>
		public static string FourthAxis = nameof(FourthAxis);

        /// <summary>
        ///   查找类似 第四个 IO 的本地化字符串。
        /// </summary>
		public static string FourthIO = nameof(FourthIO);

        /// <summary>
        ///   查找类似 帧率 的本地化字符串。
        /// </summary>
		public static string Frame = nameof(Frame);

        /// <summary>
        ///   查找类似 频率(ms) 的本地化字符串。
        /// </summary>
		public static string Frequency = nameof(Frequency);

        /// <summary>
        ///   查找类似 齿轮比分母 的本地化字符串。
        /// </summary>
		public static string GearRatioDenominator = nameof(GearRatioDenominator);

        /// <summary>
        ///   查找类似 齿轮比分子 的本地化字符串。
        /// </summary>
		public static string GearRatioNumerator = nameof(GearRatioNumerator);

        /// <summary>
        ///   查找类似 取流 的本地化字符串。
        /// </summary>
		public static string Grab = nameof(Grab);

        /// <summary>
        ///   查找类似 使用变速箱 的本地化字符串。
        /// </summary>
		public static string HaveTransmission = nameof(HaveTransmission);

        /// <summary>
        ///   查找类似 回零高速 的本地化字符串。
        /// </summary>
		public static string HighSpeed = nameof(HighSpeed);

        /// <summary>
        ///   查找类似 主页 的本地化字符串。
        /// </summary>
		public static string Home = nameof(Home);

        /// <summary>
        ///   查找类似 回零设备 的本地化字符串。
        /// </summary>
		public static string HomeDevice = nameof(HomeDevice);

        /// <summary>
        ///   查找类似 回零参数 的本地化字符串。
        /// </summary>
		public static string HomeParas = nameof(HomeParas);

        /// <summary>
        ///   查找类似 回零类型 的本地化字符串。
        /// </summary>
		public static string HomeType = nameof(HomeType);

        /// <summary>
        ///   查找类似 回零 的本地化字符串。
        /// </summary>
		public static string HomeZero = nameof(HomeZero);

        /// <summary>
        ///   查找类似 导入PLC地址 的本地化字符串。
        /// </summary>
		public static string ImportPlcAddress = nameof(ImportPlcAddress);

        /// <summary>
        ///   查找类似 序号 的本地化字符串。
        /// </summary>
		public static string Index = nameof(Index);

        /// <summary>
        ///   查找类似 伸出状态 的本地化字符串。
        /// </summary>
		public static string InExtendName = nameof(InExtendName);

        /// <summary>
        ///   查找类似 输入IO 名称 的本地化字符串。
        /// </summary>
		public static string InIOName = nameof(InIOName);

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public static string Inport = nameof(Inport);

        /// <summary>
        ///   查找类似 缩回状态 的本地化字符串。
        /// </summary>
		public static string InRetractName = nameof(InRetractName);

        /// <summary>
        ///   查找类似 灰度图 的本地化字符串。
        /// </summary>
		public static string IntensityImage = nameof(IntensityImage);

        /// <summary>
        ///   查找类似 IO 数量 的本地化字符串。
        /// </summary>
		public static string IOCount = nameof(IOCount);

        /// <summary>
        ///   查找类似 IO监控 的本地化字符串。
        /// </summary>
		public static string IOMontor = nameof(IOMontor);

        /// <summary>
        ///   查找类似 IO名称 的本地化字符串。
        /// </summary>
		public static string IOName = nameof(IOName);

        /// <summary>
        ///   查找类似 IO类型 的本地化字符串。
        /// </summary>
		public static string IOType = nameof(IOType);

        /// <summary>
        ///   查找类似 IP 地址 的本地化字符串。
        /// </summary>
		public static string IPAddress = nameof(IPAddress);

        /// <summary>
        ///   查找类似 JOG 的本地化字符串。
        /// </summary>
		public static string Jog = nameof(Jog);

        /// <summary>
        ///   查找类似 Jog速度 的本地化字符串。
        /// </summary>
		public static string JogSpped = nameof(JogSpped);

        /// <summary>
        ///   查找类似 门型轨迹 的本地化字符串。
        /// </summary>
		public static string Jump = nameof(Jump);

        /// <summary>
        ///   查找类似 关键参数配置 的本地化字符串。
        /// </summary>
		public static string KeyParameterConfig = nameof(KeyParameterConfig);

        /// <summary>
        ///   查找类似 查找类似 {0} 的本地化字符串。 的本地化字符串。
        /// </summary>
		public static string LangComment = nameof(LangComment);

        /// <summary>
        ///   查找类似 激光配置 的本地化字符串。
        /// </summary>
		public static string LaserConfigutation = nameof(LaserConfigutation);

        /// <summary>
        ///   查找类似 线扫配置文件地址 的本地化字符串。
        /// </summary>
		public static string LaserConfigutationPath = nameof(LaserConfigutationPath);

        /// <summary>
        ///   查找类似 激光数据触发模式 的本地化字符串。
        /// </summary>
		public static string LaserDataTriggerTip = nameof(LaserDataTriggerTip);

        /// <summary>
        ///   查找类似 激光硬触发配置 的本地化字符串。
        /// </summary>
		public static string LaserHardTrrigerConfigutation = nameof(LaserHardTrrigerConfigutation);

        /// <summary>
        ///   查找类似 激光 Job 的本地化字符串。
        /// </summary>
		public static string LaserJob = nameof(LaserJob);

        /// <summary>
        ///   查找类似 激光名称 的本地化字符串。
        /// </summary>
		public static string LaserName = nameof(LaserName);

        /// <summary>
        ///   查找类似 激光开始触发IO  的本地化字符串。
        /// </summary>
		public static string LaserStartIO = nameof(LaserStartIO);

        /// <summary>
        ///   查找类似 激光开始触发模式 的本地化字符串。
        /// </summary>
		public static string LaserStartTip = nameof(LaserStartTip);

        /// <summary>
        ///   查找类似 上次月保养时间 的本地化字符串。
        /// </summary>
		public static string LastMonthMaintenance = nameof(LastMonthMaintenance);

        /// <summary>
        ///   查找类似 上次周保养时间 的本地化字符串。
        /// </summary>
		public static string LastWeekMaintenance = nameof(LastWeekMaintenance);

        /// <summary>
        ///   查找类似 锁存通道 的本地化字符串。
        /// </summary>
		public static string LCTNo = nameof(LCTNo);

        /// <summary>
        ///   查找类似 光源配置 的本地化字符串。
        /// </summary>
		public static string LightConfigutation = nameof(LightConfigutation);

        /// <summary>
        ///   查找类似 光源控制器 的本地化字符串。
        /// </summary>
		public static string LightController = nameof(LightController);

        /// <summary>
        ///   查找类似 光源熄灭延迟时间(ms) 的本地化字符串。
        /// </summary>
		public static string LightDelayToOff = nameof(LightDelayToOff);

        /// <summary>
        ///   查找类似 线性插补 的本地化字符串。
        /// </summary>
		public static string LinearComplement = nameof(LinearComplement);

        /// <summary>
        ///   查找类似 线激光 的本地化字符串。
        /// </summary>
		public static string LineLaser = nameof(LineLaser);

        /// <summary>
        ///   查找类似 加载配置 的本地化字符串。
        /// </summary>
		public static string Load = nameof(Load);

        /// <summary>
        ///   查找类似 定位区间(mm) 的本地化字符串。
        /// </summary>
		public static string LocationRange = nameof(LocationRange);

        /// <summary>
        ///   查找类似 回零低速 的本地化字符串。
        /// </summary>
		public static string LowSpeed = nameof(LowSpeed);

        /// <summary>
        ///   查找类似 设备保养 的本地化字符串。
        /// </summary>
		public static string Maintain = nameof(Maintain);

        /// <summary>
        ///   查找类似 维护里程 的本地化字符串。
        /// </summary>
		public static string MaintainScale = nameof(MaintainScale);

        /// <summary>
        ///   查找类似 保养 的本地化字符串。
        /// </summary>
		public static string Maintenance = nameof(Maintenance);

        /// <summary>
        ///   查找类似 设备管理 的本地化字符串。
        /// </summary>
		public static string Management = nameof(Management);

        /// <summary>
        ///   查找类似 轮廓图或者深度图 的本地化字符串。
        /// </summary>
		public static string MapType = nameof(MapType);

        /// <summary>
        ///   查找类似 匹配 的本地化字符串。
        /// </summary>
		public static string Match = nameof(Match);

        /// <summary>
        ///   查找类似 最大位置 的本地化字符串。
        /// </summary>
		public static string MaxPosition = nameof(MaxPosition);

        /// <summary>
        ///   查找类似 最大行程(单位 mm) 的本地化字符串。
        /// </summary>
		public static string MaxPositionWithUnit = nameof(MaxPositionWithUnit);

        /// <summary>
        ///   查找类似 最大里程 的本地化字符串。
        /// </summary>
		public static string MaxScale = nameof(MaxScale);

        /// <summary>
        ///   查找类似 最小位置 的本地化字符串。
        /// </summary>
		public static string MinPosition = nameof(MinPosition);

        /// <summary>
        ///   查找类似 最小行程(单位 mm) 的本地化字符串。
        /// </summary>
		public static string MinPositionWithUnit = nameof(MinPositionWithUnit);

        /// <summary>
        ///   查找类似 ModbusRTU 的本地化字符串。
        /// </summary>
		public static string ModbusRTU = nameof(ModbusRTU);

        /// <summary>
        ///   查找类似 修改 的本地化字符串。
        /// </summary>
		public static string Modify = nameof(Modify);

        /// <summary>
        ///   查找类似 模块配置 的本地化字符串。
        /// </summary>
		public static string ModuleConfig = nameof(ModuleConfig);

        /// <summary>
        ///   查找类似 模组名称 的本地化字符串。
        /// </summary>
		public static string ModuleName = nameof(ModuleName);

        /// <summary>
        ///   查找类似 模组或者名称 的本地化字符串。
        /// </summary>
		public static string ModuleOrName = nameof(ModuleOrName);

        /// <summary>
        ///   查找类似 监控设备 的本地化字符串。
        /// </summary>
		public static string MonitorDevice = nameof(MonitorDevice);

        /// <summary>
        ///   查找类似 监控值 的本地化字符串。
        /// </summary>
		public static string MonitorValue = nameof(MonitorValue);

        /// <summary>
        ///   查找类似 控制卡 的本地化字符串。
        /// </summary>
		public static string MotionCard = nameof(MotionCard);

        /// <summary>
        ///   查找类似 运动前提 的本地化字符串。
        /// </summary>
		public static string MotionPremise = nameof(MotionPremise);

        /// <summary>
        ///   查找类似 运动调试 的本地化字符串。
        /// </summary>
		public static string MotionTest = nameof(MotionTest);

        /// <summary>
        ///   查找类似 运动 的本地化字符串。
        /// </summary>
		public static string Move = nameof(Move);

        /// <summary>
        ///   查找类似 移动距离 的本地化字符串。
        /// </summary>
		public static string MoveDistance = nameof(MoveDistance);

        /// <summary>
        ///   查找类似 运动前提 的本地化字符串。
        /// </summary>
		public static string MovePrecondition = nameof(MovePrecondition);

        /// <summary>
        ///   查找类似 运动类型 的本地化字符串。
        /// </summary>
		public static string MoveType = nameof(MoveType);

        /// <summary>
        ///   查找类似 名称 的本地化字符串。
        /// </summary>
		public static string Name = nameof(Name);

        /// <summary>
        ///   查找类似 是否检知 的本地化字符串。
        /// </summary>
		public static string NeedDetetor = nameof(NeedDetetor);

        /// <summary>
        ///   查找类似 是否回零 的本地化字符串。
        /// </summary>
		public static string NeedHome = nameof(NeedHome);

        /// <summary>
        ///   查找类似 是否屏蔽 的本地化字符串。
        /// </summary>
		public static string NeedShield = nameof(NeedShield);

        /// <summary>
        ///   查找类似 负行程 的本地化字符串。
        /// </summary>
		public static string NegativeTrip = nameof(NegativeTrip);

        /// <summary>
        ///   查找类似 下一指令 的本地化字符串。
        /// </summary>
		public static string NextCommand = nameof(NextCommand);

        /// <summary>
        ///   查找类似 常规参数 的本地化字符串。
        /// </summary>
		public static string NormalParas = nameof(NormalParas);

        /// <summary>
        ///   查找类似 空字符 的本地化字符串。
        /// </summary>
		public static string Null = nameof(Null);

        /// <summary>
        ///   查找类似 离线配置 的本地化字符串。
        /// </summary>
		public static string OfflineConfigutation = nameof(OfflineConfigutation);

        /// <summary>
        ///   查找类似 离线图片 的本地化字符串。
        /// </summary>
		public static string OfflineImage = nameof(OfflineImage);

        /// <summary>
        ///   查找类似 离线点云 的本地化字符串。
        /// </summary>
		public static string OfflinePointCloud = nameof(OfflinePointCloud);

        /// <summary>
        ///   查找类似 打开 的本地化字符串。
        /// </summary>
		public static string Open = nameof(Open);

        /// <summary>
        ///   查找类似 操作 的本地化字符串。
        /// </summary>
		public static string Operation = nameof(Operation);

        /// <summary>
        ///   查找类似 其他配置 的本地化字符串。
        /// </summary>
		public static string OtherConfigutation = nameof(OtherConfigutation);

        /// <summary>
        ///   查找类似 输出 IO 名称 的本地化字符串。
        /// </summary>
		public static string OutIOName = nameof(OutIOName);

        /// <summary>
        ///   查找类似 点位参数配置 的本地化字符串。
        /// </summary>
		public static string PositionParameterConfig = nameof(PositionParameterConfig);

        /// <summary>
        ///   查找类似 指令报文 的本地化字符串。
        /// </summary>
		public static string PacketData = nameof(PacketData);

        /// <summary>
        ///   查找类似 奇偶校验位 的本地化字符串。
        /// </summary>
		public static string Parity = nameof(Parity);

        /// <summary>
        ///   查找类似 单页数量 的本地化字符串。
        /// </summary>
		public static string PerPageCount = nameof(PerPageCount);

        /// <summary>
        ///   查找类似 到位时间 的本地化字符串。
        /// </summary>
		public static string PlaceTime = nameof(PlaceTime);

        /// <summary>
        ///   查找类似 PLC地址 的本地化字符串。
        /// </summary>
		public static string PlcAddress = nameof(PlcAddress);

        /// <summary>
        ///   查找类似 请输入轴名称 的本地化字符串。
        /// </summary>
		public static string PleaseInputAxisName = nameof(PleaseInputAxisName);

        /// <summary>
        ///   查找类似 请选择通信方式 的本地化字符串。
        /// </summary>
		public static string PleaseSelectCommType = nameof(PleaseSelectCommType);

        /// <summary>
        ///   查找类似 点位 的本地化字符串。
        /// </summary>
		public static string Point = nameof(Point);

        /// <summary>
        ///   查找类似 端口号 的本地化字符串。
        /// </summary>
		public static string Port = nameof(Port);

        /// <summary>
        ///   查找类似 点位运动前提 的本地化字符串。
        /// </summary>
		public static string PosionMotionPremise = nameof(PosionMotionPremise);

        /// <summary>
        ///   查找类似 位置(mm) 的本地化字符串。
        /// </summary>
		public static string Position = nameof(Position);

        /// <summary>
        ///   查找类似 位置(pulse) 的本地化字符串。
        /// </summary>
		public static string PositionPulse = nameof(PositionPulse);

        /// <summary>
        ///   查找类似 推压距离 的本地化字符串。
        /// </summary>
		public static string PressDistance = nameof(PressDistance);

        /// <summary>
        ///   查找类似 压力曲线 的本地化字符串。
        /// </summary>
		public static string PressDriver = nameof(PressDriver);

        /// <summary>
        ///   查找类似 推压值(N) 的本地化字符串。
        /// </summary>
		public static string Pressure = nameof(Pressure);

        /// <summary>
        ///   查找类似 打印 的本地化字符串。
        /// </summary>
		public static string Print = nameof(Print);

        /// <summary>
        ///   查找类似 打印机 的本地化字符串。
        /// </summary>
		public static string Printer = nameof(Printer);

        /// <summary>
        ///   查找类似 优先级 的本地化字符串。
        /// </summary>
		public static string Priority = nameof(Priority);

        /// <summary>
        ///   查找类似 使用进度 的本地化字符串。
        /// </summary>
		public static string Process = nameof(Process);

        /// <summary>
        ///   查找类似 协议 的本地化字符串。
        /// </summary>
		public static string Protocol = nameof(Protocol);

        /// <summary>
        ///   查找类似 脉冲比计算 的本地化字符串。
        /// </summary>
		public static string PulseCal = nameof(PulseCal);

        /// <summary>
        ///   查找类似 到位脉冲 的本地化字符串。
        /// </summary>
		public static string PulseCount = nameof(PulseCount);

        /// <summary>
        ///   查找类似 脉冲比 的本地化字符串。
        /// </summary>
		public static string PulseRatio = nameof(PulseRatio);

        /// <summary>
        ///   查找类似 输出脉宽 的本地化字符串。
        /// </summary>
		public static string PulseWidth = nameof(PulseWidth);

        /// <summary>
        ///   查找类似 输出赋值 的本地化字符串。
        /// </summary>
		public static string PutValue = nameof(PutValue);

        /// <summary>
        ///   查找类似 读取 的本地化字符串。
        /// </summary>
		public static string Read = nameof(Read);

        /// <summary>
        ///   查找类似 批量读取 的本地化字符串。
        /// </summary>
		public static string ReadAll = nameof(ReadAll);

        /// <summary>
        ///   查找类似 读取数据 的本地化字符串。
        /// </summary>
		public static string ReadData = nameof(ReadData);

        /// <summary>
        ///   查找类似 是否只读 的本地化字符串。
        /// </summary>
		public static string ReadOnly = nameof(ReadOnly);

        /// <summary>
        ///   查找类似 读写类型 的本地化字符串。
        /// </summary>
		public static string ReadOrWrite = nameof(ReadOrWrite);

        /// <summary>
        ///   查找类似 相对运动 的本地化字符串。
        /// </summary>
		public static string RelativeMove = nameof(RelativeMove);

        /// <summary>
        ///   查找类似 缩回 的本地化字符串。
        /// </summary>
		public static string Retract = nameof(Retract);

        /// <summary>
        ///   查找类似 缩回检知 的本地化字符串。
        /// </summary>
		public static string RetractDetetor = nameof(RetractDetetor);

        /// <summary>
        ///   查找类似 缩回DI 的本地化字符串。
        /// </summary>
		public static string RetractDI = nameof(RetractDI);

        /// <summary>
        ///   查找类似 缩回DO 的本地化字符串。
        /// </summary>
		public static string RetractDO = nameof(RetractDO);

        /// <summary>
        ///   查找类似 缩回名称 的本地化字符串。
        /// </summary>
		public static string RetractName = nameof(RetractName);

        /// <summary>
        ///   查找类似 缩回状态 的本地化字符串。
        /// </summary>
		public static string RetractStatus = nameof(RetractStatus);

        /// <summary>
        ///   查找类似 机器人 的本地化字符串。
        /// </summary>
		public static string Robot = nameof(Robot);

        /// <summary>
        ///   查找类似 机器人名称 的本地化字符串。
        /// </summary>
		public static string RobotName = nameof(RobotName);

        /// <summary>
        ///   查找类似 机器人运动模式 的本地化字符串。
        /// </summary>
		public static string RobotRunModel = nameof(RobotRunModel);

        /// <summary>
        ///   查找类似 机器人运动模式选择 的本地化字符串。
        /// </summary>
		public static string RobotRunModelTip = nameof(RobotRunModelTip);

        /// <summary>
        ///   查找类似 ROI高度 的本地化字符串。
        /// </summary>
		public static string ROIHeight = nameof(ROIHeight);

        /// <summary>
        ///   查找类似 ROI开始X 的本地化字符串。
        /// </summary>
		public static string ROIStartX = nameof(ROIStartX);

        /// <summary>
        ///   查找类似 ROI开始Y 的本地化字符串。
        /// </summary>
		public static string ROIStartY = nameof(ROIStartY);

        /// <summary>
        ///   查找类似 ROI宽度 的本地化字符串。
        /// </summary>
		public static string ROIWidth = nameof(ROIWidth);

        /// <summary>
        ///   查找类似 运行速度 的本地化字符串。
        /// </summary>
		public static string RunSpeed = nameof(RunSpeed);

        /// <summary>
        ///   查找类似 安全设备 的本地化字符串。
        /// </summary>
		public static string SafeDevice = nameof(SafeDevice);

        /// <summary>
        ///   查找类似 安全门 的本地化字符串。
        /// </summary>
		public static string SafeDoor = nameof(SafeDoor);

        /// <summary>
        ///   查找类似 保存 的本地化字符串。
        /// </summary>
		public static string Save = nameof(Save);

        /// <summary>
        ///   查找类似 轴2 的本地化字符串。
        /// </summary>
		public static string SecondAxis = nameof(SecondAxis);

        /// <summary>
        ///   查找类似 第二个 IO 的本地化字符串。
        /// </summary>
		public static string SecondIO = nameof(SecondIO);

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public static string Select = nameof(Select);

        /// <summary>
        ///   查找类似 请选择运动轴 的本地化字符串。
        /// </summary>
		public static string SelectAxis = nameof(SelectAxis);

        /// <summary>
        ///   查找类似 请选择厂商 的本地化字符串。
        /// </summary>
		public static string SelectBrand = nameof(SelectBrand);

        /// <summary>
        ///   查找类似 请选择编码器 的本地化字符串。
        /// </summary>
		public static string SelectEncoderTip = nameof(SelectEncoderTip);

        /// <summary>
        ///   查找类似 请选择Excel文件 的本地化字符串。
        /// </summary>
		public static string SelectExcelFile = nameof(SelectExcelFile);

        /// <summary>
        ///   查找类似 请选择当前激光配置 的本地化字符串。
        /// </summary>
		public static string SelectLaserConfigutation = nameof(SelectLaserConfigutation);

        /// <summary>
        ///   查找类似 选择线扫配置文件地址 的本地化字符串。
        /// </summary>
		public static string SelectLaserConfigutationPath = nameof(SelectLaserConfigutationPath);

        /// <summary>
        ///   查找类似 请选择运动控制卡 的本地化字符串。
        /// </summary>
		public static string SelectMotinCard = nameof(SelectMotinCard);

        /// <summary>
        ///   查找类似 请选择离线图片 的本地化字符串。
        /// </summary>
		public static string SelectOfflineImage = nameof(SelectOfflineImage);

        /// <summary>
        ///   查找类似 请选择离线点云 的本地化字符串。
        /// </summary>
		public static string SelectOfflinePointCloud = nameof(SelectOfflinePointCloud);

        /// <summary>
        ///   查找类似 请选择对应的安全设备 的本地化字符串。
        /// </summary>
		public static string SelectSafeDevice = nameof(SelectSafeDevice);

        /// <summary>
        ///   查找类似 发送报文 的本地化字符串。
        /// </summary>
		public static string SendPacket = nameof(SendPacket);

        /// <summary>
        ///   查找类似 序列号 的本地化字符串。
        /// </summary>
		public static string SerialNumber = nameof(SerialNumber);

        /// <summary>
        ///   查找类似 设置 的本地化字符串。
        /// </summary>
		public static string Set = nameof(Set);

        /// <summary>
        ///   查找类似 第七个 IO 的本地化字符串。
        /// </summary>
		public static string SeventhIO = nameof(SeventhIO);

        /// <summary>
        ///   查找类似 屏蔽设备 的本地化字符串。
        /// </summary>
		public static string ShieldDevice = nameof(ShieldDevice);

        /// <summary>
        ///   查找类似 存图 的本地化字符串。
        /// </summary>
		public static string Shot = nameof(Shot);

        /// <summary>
        ///   查找类似 显示点云 的本地化字符串。
        /// </summary>
		public static string ShowPointCloud = nameof(ShowPointCloud);

        /// <summary>
        ///   查找类似 模拟赋值 的本地化字符串。
        /// </summary>
		public static string SimulationValue = nameof(SimulationValue);

        /// <summary>
        ///   查找类似 单控 的本地化字符串。
        /// </summary>
		public static string SingleControl = nameof(SingleControl);

        /// <summary>
        ///   查找类似 第六个 IO 的本地化字符串。
        /// </summary>
		public static string SixthIO = nameof(SixthIO);

        /// <summary>
        ///   查找类似 从站名称 的本地化字符串。
        /// </summary>
		public static string SlaveName = nameof(SlaveName);

        /// <summary>
        ///   查找类似 从站号 的本地化字符串。
        /// </summary>
		public static string SlaveNo = nameof(SlaveNo);

        /// <summary>
        ///   查找类似 PC插槽 的本地化字符串。
        /// </summary>
		public static string Slot = nameof(Slot);

        /// <summary>
        ///   查找类似 Socket类型 的本地化字符串。
        /// </summary>
		public static string SocketType = nameof(SocketType);

        /// <summary>
        ///   查找类似 速度(mm) 的本地化字符串。
        /// </summary>
		public static string Speed = nameof(Speed);

        /// <summary>
        ///   查找类似 速度系数 的本地化字符串。
        /// </summary>
		public static string SpeedCoefficient = nameof(SpeedCoefficient);

        /// <summary>
        ///   查找类似 真实速度=速度 * 系数 的本地化字符串。
        /// </summary>
		public static string SpeedTip = nameof(SpeedTip);

        /// <summary>
        ///   查找类似 开始地址 的本地化字符串。
        /// </summary>
		public static string StartAddress = nameof(StartAddress);

        /// <summary>
        ///   查找类似 开始点检 的本地化字符串。
        /// </summary>
		public static string StartCheck = nameof(StartCheck);

        /// <summary>
        ///   查找类似 起始编号 的本地化字符串。
        /// </summary>
		public static string StartIndex = nameof(StartIndex);

        /// <summary>
        ///   查找类似 开始IO 的本地化字符串。
        /// </summary>
		public static string StartIO = nameof(StartIO);

        /// <summary>
        ///   查找类似 开始触发 的本地化字符串。
        /// </summary>
		public static string StartTrigger = nameof(StartTrigger);

        /// <summary>
        ///   查找类似 站点 的本地化字符串。
        /// </summary>
		public static string StationNO = nameof(StationNO);

        /// <summary>
        ///   查找类似 状态 的本地化字符串。
        /// </summary>
		public static string Status = nameof(Status);

        /// <summary>
        ///   查找类似 停止 的本地化字符串。
        /// </summary>
		public static string Stop = nameof(Stop);

        /// <summary>
        ///   查找类似 停止位 的本地化字符串。
        /// </summary>
		public static string StopBits = nameof(StopBits);

        /// <summary>
        ///   查找类似 子定义 的本地化字符串。
        /// </summary>
		public static string SubDefinite = nameof(SubDefinite);

        /// <summary>
        ///   查找类似 吸 的本地化字符串。
        /// </summary>
		public static string Suck = nameof(Suck);

        /// <summary>
        ///   查找类似 吸DO 的本地化字符串。
        /// </summary>
		public static string SuckDO = nameof(SuckDO);

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public static string Sure = nameof(Sure);

        /// <summary>
        ///   查找类似 标签高度 的本地化字符串。
        /// </summary>
		public static string TagHeight = nameof(TagHeight);

        /// <summary>
        ///   查找类似 标签宽度 的本地化字符串。
        /// </summary>
		public static string TagWidth = nameof(TagWidth);

        /// <summary>
        ///   查找类似 泰科统计 的本地化字符串。
        /// </summary>
		public static string TaikeContent = nameof(TaikeContent);

        /// <summary>
        ///   查找类似 泰科曲线 的本地化字符串。
        /// </summary>
		public static string TaikeCurve = nameof(TaikeCurve);

        /// <summary>
        ///   查找类似 示教 的本地化字符串。
        /// </summary>
		public static string Teach = nameof(Teach);

        /// <summary>
        ///   查找类似 轴3 的本地化字符串。
        /// </summary>
		public static string ThirdAxis = nameof(ThirdAxis);

        /// <summary>
        ///   查找类似 第三个 IO 的本地化字符串。
        /// </summary>
		public static string ThirdIO = nameof(ThirdIO);

        /// <summary>
        ///   查找类似 标题 的本地化字符串。
        /// </summary>
		public static string Title = nameof(Title);

        /// <summary>
        ///   查找类似 触发次数 的本地化字符串。
        /// </summary>
		public static string TriggerCount = nameof(TriggerCount);

        /// <summary>
        ///   查找类似 触发Fifo数量 的本地化字符串。
        /// </summary>
		public static string TriggerFifoCount = nameof(TriggerFifoCount);

        /// <summary>
        ///   查找类似 触发输出通道 的本地化字符串。
        /// </summary>
		public static string TriggerNo = nameof(TriggerNo);

        /// <summary>
        ///   查找类似 触发输出通道计数 的本地化字符串。
        /// </summary>
		public static string TriggerNoCount = nameof(TriggerNoCount);

        /// <summary>
        ///   查找类似 触发通道类型 的本地化字符串。
        /// </summary>
		public static string TriggerNoType = nameof(TriggerNoType);

        /// <summary>
        ///   查找类似 触发脉冲百分比 的本地化字符串。
        /// </summary>
		public static string TriggerPluseRatio = nameof(TriggerPluseRatio);

        /// <summary>
        ///   查找类似 读取Txt 的本地化字符串。
        /// </summary>
		public static string TxtReader = nameof(TxtReader);

        /// <summary>
        ///   查找类似 未监控设备(双击添加监控) 的本地化字符串。
        /// </summary>
		public static string UnMonitorDevice = nameof(UnMonitorDevice);

        /// <summary>
        ///   查找类似 上移 的本地化字符串。
        /// </summary>
		public static string Up = nameof(Up);

        /// <summary>
        ///   查找类似 更新 的本地化字符串。
        /// </summary>
		public static string Update = nameof(Update);

        /// <summary>
        ///   查找类似 上使能 的本地化字符串。
        /// </summary>
		public static string UppperEnable = nameof(UppperEnable);

        /// <summary>
        ///   查找类似 USB口 的本地化字符串。
        /// </summary>
		public static string USB = nameof(USB);

        /// <summary>
        ///   查找类似 已使用 的本地化字符串。
        /// </summary>
		public static string Used = nameof(Used);

        /// <summary>
        ///   查找类似 真空检知 的本地化字符串。
        /// </summary>
		public static string VacuumSensor = nameof(VacuumSensor);

        /// <summary>
        ///   查找类似 真空检知DI 的本地化字符串。
        /// </summary>
		public static string VacuumSensorDI = nameof(VacuumSensorDI);

        /// <summary>
        ///   查找类似 配置值 的本地化字符串。
        /// </summary>
		public static string Value = nameof(Value);

        /// <summary>
        ///   查找类似 仿真轴 的本地化字符串。
        /// </summary>
		public static string VAxis = nameof(VAxis);

        /// <summary>
        ///   查找类似 仿真轴3 的本地化字符串。
        /// </summary>
		public static string VAxis3 = nameof(VAxis3);

        /// <summary>
        ///   查找类似 轴列表(双击显示) 的本地化字符串。
        /// </summary>
		public static string VAxisIOAxisListTip = nameof(VAxisIOAxisListTip);

        /// <summary>
        ///   查找类似 轴列表(双击添加) 的本地化字符串。
        /// </summary>
		public static string VAxisListTip = nameof(VAxisListTip);

        /// <summary>
        ///   查找类似 仿真多轴 的本地化字符串。
        /// </summary>
		public static string VAxisM = nameof(VAxisM);

        /// <summary>
        ///   查找类似 仿真皮带 的本地化字符串。
        /// </summary>
		public static string VBelt = nameof(VBelt);

        /// <summary>
        ///   查找类似 仿真相机 的本地化字符串。
        /// </summary>
		public static string VCamera = nameof(VCamera);

        /// <summary>
        ///   查找类似 仿真通信 的本地化字符串。
        /// </summary>
		public static string VCommuncation = nameof(VCommuncation);

        /// <summary>
        ///   查找类似 仿真气缸 的本地化字符串。
        /// </summary>
		public static string VCylinder = nameof(VCylinder);

        /// <summary>
        ///   查找类似 虚拟设备 的本地化字符串。
        /// </summary>
		public static string VDevice = nameof(VDevice);

        /// <summary>
        ///   查找类似 仿真电批 的本地化字符串。
        /// </summary>
		public static string VESD = nameof(VESD);

        /// <summary>
        ///   查找类似 仿真飞达 的本地化字符串。
        /// </summary>
		public static string VFeeder = nameof(VFeeder);

        /// <summary>
        ///   查找类似 仿真飞拍 的本地化字符串。
        /// </summary>
		public static string VFlyingPhoto = nameof(VFlyingPhoto);

        /// <summary>
        ///   查找类似 仿真输入 的本地化字符串。
        /// </summary>
		public static string VInputIO = nameof(VInputIO);

        /// <summary>
        ///   查找类似 仿真IO 的本地化字符串。
        /// </summary>
		public static string VIO = nameof(VIO);

        /// <summary>
        ///   查找类似 IO仿真设备 的本地化字符串。
        /// </summary>
		public static string VIOSimulation = nameof(VIOSimulation);

        /// <summary>
        ///   查找类似 Vision过程参数 的本地化字符串。
        /// </summary>
		public static string VisionProcessData = nameof(VisionProcessData);

        /// <summary>
        ///   查找类似 仿真线激光 的本地化字符串。
        /// </summary>
		public static string VLineLaser = nameof(VLineLaser);

        /// <summary>
        ///   查找类似 仿真电缸 的本地化字符串。
        /// </summary>
		public static string VMCylinder = nameof(VMCylinder);

        /// <summary>
        ///   查找类似 仿真输出 的本地化字符串。
        /// </summary>
		public static string VOutputIO = nameof(VOutputIO);

        /// <summary>
        ///   查找类似 力矩电缸 的本地化字符串。
        /// </summary>
		public static string VPCylinder = nameof(VPCylinder);

        /// <summary>
        ///   查找类似 仿真PLC 的本地化字符串。
        /// </summary>
		public static string VPlc = nameof(VPlc);

        /// <summary>
        ///   查找类似 仿真打印机 的本地化字符串。
        /// </summary>
		public static string VPrinter = nameof(VPrinter);

        /// <summary>
        ///   查找类似 仿真机器人 的本地化字符串。
        /// </summary>
		public static string VRobot = nameof(VRobot);

        /// <summary>
        ///   查找类似 仿真SerialPort 的本地化字符串。
        /// </summary>
		public static string VSerialPort = nameof(VSerialPort);

        /// <summary>
        ///   查找类似 仿真Socket 的本地化字符串。
        /// </summary>
		public static string VSocket = nameof(VSocket);

        /// <summary>
        ///   查找类似 仿真三色灯 的本地化字符串。
        /// </summary>
		public static string VTricolorlamp = nameof(VTricolorlamp);

        /// <summary>
        ///   查找类似 仿真真空 的本地化字符串。
        /// </summary>
		public static string VVacuum = nameof(VVacuum);

        /// <summary>
        ///   查找类似 写入 的本地化字符串。
        /// </summary>
		public static string Write = nameof(Write);

        /// <summary>
        ///   查找类似 写入数据 的本地化字符串。
        /// </summary>
		public static string WriteData = nameof(WriteData);

        /// <summary>
        ///   查找类似 写入报文 的本地化字符串。
        /// </summary>
		public static string WritePacket = nameof(WritePacket);

        /// <summary>
        ///   查找类似 写入值 的本地化字符串。
        /// </summary>
		public static string WriteValue = nameof(WriteValue);

        /// <summary>
        ///   查找类似 鑫精诚 的本地化字符串。
        /// </summary>
		public static string XJCPressureSensor = nameof(XJCPressureSensor);

        /// <summary>
        ///   查找类似 零点偏移 的本地化字符串。
        /// </summary>
		public static string ZeroOffset = nameof(ZeroOffset);

    }
}
