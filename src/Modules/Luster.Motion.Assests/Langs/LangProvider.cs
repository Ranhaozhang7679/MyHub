using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HandyControl.Tools;

namespace Luster.Motion.Assests.Langs
{
    public class LangProvider : INotifyPropertyChanged
    {
        public static LangProvider Instance { get; } = ResourceHelper.GetResource<LangProvider>("MotionLangs");

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
			OnPropertyChanged(nameof(Absolutely));
			OnPropertyChanged(nameof(AbsoluteMotion));
			OnPropertyChanged(nameof(Acc));
			OnPropertyChanged(nameof(AccessoriesAmount));
			OnPropertyChanged(nameof(AccessoriesBatch));
			OnPropertyChanged(nameof(AccessoriesName));
			OnPropertyChanged(nameof(Action));
			OnPropertyChanged(nameof(Active));
			OnPropertyChanged(nameof(ActiveRecipeNotFound));
			OnPropertyChanged(nameof(Actual_CT));
			OnPropertyChanged(nameof(ActualSpeed));
			OnPropertyChanged(nameof(Add));
			OnPropertyChanged(nameof(AddClass));
			OnPropertyChanged(nameof(AddPlcAlarm));
			OnPropertyChanged(nameof(AddProduct));
			OnPropertyChanged(nameof(AddUser));
			OnPropertyChanged(nameof(Alarm));
			OnPropertyChanged(nameof(AlarmAddress));
			OnPropertyChanged(nameof(AlarmCode));
			OnPropertyChanged(nameof(AlarmConfigure));
			OnPropertyChanged(nameof(AlarmContent));
			OnPropertyChanged(nameof(AlarmDetailInfo));
			OnPropertyChanged(nameof(AlarmEnglish));
			OnPropertyChanged(nameof(AlarmID));
			OnPropertyChanged(nameof(AlarmInfo));
			OnPropertyChanged(nameof(Alarming));
			OnPropertyChanged(nameof(AlarmMonitoring));
			OnPropertyChanged(nameof(AlarmSolution));
			OnPropertyChanged(nameof(AlarmTime));
			OnPropertyChanged(nameof(AlarmType));
			OnPropertyChanged(nameof(Algorithm));
			OnPropertyChanged(nameof(AlignByBestFit));
			OnPropertyChanged(nameof(AlignByCoord));
			OnPropertyChanged(nameof(AlignByInit));
			OnPropertyChanged(nameof(AlignByRPS));
			OnPropertyChanged(nameof(Alignment));
			OnPropertyChanged(nameof(All));
			OnPropertyChanged(nameof(AlreadyExists));
			OnPropertyChanged(nameof(Am));
			OnPropertyChanged(nameof(AnalogConvert));
			OnPropertyChanged(nameof(AngleMeasure));
			OnPropertyChanged(nameof(AnnotationGeneration));
			OnPropertyChanged(nameof(AOI));
			OnPropertyChanged(nameof(ApiVersion));
			OnPropertyChanged(nameof(Apply));
			OnPropertyChanged(nameof(ArcLine2Line));
			OnPropertyChanged(nameof(ArcLine2Plane));
			OnPropertyChanged(nameof(ArcPlane2Plane));
			OnPropertyChanged(nameof(AreaCamera));
			OnPropertyChanged(nameof(ArrangeDocument));
			OnPropertyChanged(nameof(AssemblyNotFound));
			OnPropertyChanged(nameof(AssemblyNotFound1));
			OnPropertyChanged(nameof(AsyncGroup));
			OnPropertyChanged(nameof(AutoCommunicationConfig));
			OnPropertyChanged(nameof(AutoFieldOfView));
			OnPropertyChanged(nameof(AutoFocusing));
			OnPropertyChanged(nameof(AutoGrayScale));
			OnPropertyChanged(nameof(AutomaticEmbossing));
			OnPropertyChanged(nameof(AutomaticLoadCell));
			OnPropertyChanged(nameof(AutomaticPosAndLeveling));
			OnPropertyChanged(nameof(AutoRun));
			OnPropertyChanged(nameof(AutoVerication));
			OnPropertyChanged(nameof(AutoVisualCalibration));
			OnPropertyChanged(nameof(AverageTime));
			OnPropertyChanged(nameof(AvgNoOfCodeSewwp));
			OnPropertyChanged(nameof(Axis));
			OnPropertyChanged(nameof(AxisArm));
			OnPropertyChanged(nameof(AxisDebug));
			OnPropertyChanged(nameof(AxisPos));
			OnPropertyChanged(nameof(AxisPosMove));
            OnPropertyChanged(nameof(AxisPosArray));
            OnPropertyChanged(nameof(DHRoboticsVCM));
            OnPropertyChanged(nameof(JunRudderVCM));
            OnPropertyChanged(nameof(AxisPriority));
			OnPropertyChanged(nameof(BackCarrierNum));
			OnPropertyChanged(nameof(BackgroundStation));
			OnPropertyChanged(nameof(BackUp));
			OnPropertyChanged(nameof(BackUpSet));
			OnPropertyChanged(nameof(BaliVersion));
			OnPropertyChanged(nameof(BatchImportPoints));
			OnPropertyChanged(nameof(BeltCarry));
			OnPropertyChanged(nameof(BestFit));
			OnPropertyChanged(nameof(Binarization));
			OnPropertyChanged(nameof(Block));
			OnPropertyChanged(nameof(BoundBox));
			OnPropertyChanged(nameof(Branch));
			OnPropertyChanged(nameof(Branch_Does_Not_Exist));
			OnPropertyChanged(nameof(BranchGroup));
			OnPropertyChanged(nameof(Broswer));
			OnPropertyChanged(nameof(Business));
			OnPropertyChanged(nameof(BUSOP));
			OnPropertyChanged(nameof(Button));
			OnPropertyChanged(nameof(ButtonControl));
			OnPropertyChanged(nameof(Buzzer));
			OnPropertyChanged(nameof(CacheData));
			OnPropertyChanged(nameof(CADModel));
			OnPropertyChanged(nameof(CalcTime));
			OnPropertyChanged(nameof(Calculator));
			OnPropertyChanged(nameof(Calib));
			OnPropertyChanged(nameof(CalibByPosMove));
			OnPropertyChanged(nameof(CalibrationTable));
			OnPropertyChanged(nameof(Camera));
			OnPropertyChanged(nameof(CameraIO));
			OnPropertyChanged(nameof(Cancel));
			OnPropertyChanged(nameof(CancelCoordTemplate));
			OnPropertyChanged(nameof(CancelSkip));
			OnPropertyChanged(nameof(CannotDeleteProjWhithRecipeActive));
			OnPropertyChanged(nameof(CannotPermissionToModifyFunction));
			OnPropertyChanged(nameof(CapacityReset));
			OnPropertyChanged(nameof(CapacityStatistics));
			OnPropertyChanged(nameof(CarrierBlackList));
			OnPropertyChanged(nameof(CarrierCount));
			OnPropertyChanged(nameof(category_key));
			OnPropertyChanged(nameof(CCDImage));
			OnPropertyChanged(nameof(CgAoi));
			OnPropertyChanged(nameof(ChangeRecord));
			OnPropertyChanged(nameof(ChangeType));
			OnPropertyChanged(nameof(ChartList));
			OnPropertyChanged(nameof(CheckForUpdates));
			OnPropertyChanged(nameof(CheckIO));
			OnPropertyChanged(nameof(CheckVariable));
			OnPropertyChanged(nameof(Choose));
			OnPropertyChanged(nameof(ChooseTips));
			OnPropertyChanged(nameof(Circle));
			OnPropertyChanged(nameof(Class));
			OnPropertyChanged(nameof(Clear));
			OnPropertyChanged(nameof(ClearMistake));
			OnPropertyChanged(nameof(Close));
			OnPropertyChanged(nameof(CloseAll));
			OnPropertyChanged(nameof(CloseOther));
			OnPropertyChanged(nameof(CloudCalib));
			OnPropertyChanged(nameof(CloudDenoising));
			OnPropertyChanged(nameof(CloudDownSampling));
			OnPropertyChanged(nameof(CloudMesh));
			OnPropertyChanged(nameof(CloudProcess));
			OnPropertyChanged(nameof(CloudRegistration));
			OnPropertyChanged(nameof(CloudReRepeat));
			OnPropertyChanged(nameof(CloudSegment));
			OnPropertyChanged(nameof(CloudSmooth));
			OnPropertyChanged(nameof(CloudTransform));
			OnPropertyChanged(nameof(Cockpit));
			OnPropertyChanged(nameof(CockpitIP));
			OnPropertyChanged(nameof(CockpitPort));
			OnPropertyChanged(nameof(Color));
			OnPropertyChanged(nameof(Comma));
			OnPropertyChanged(nameof(CommonAlarm));
			OnPropertyChanged(nameof(Communication));
			OnPropertyChanged(nameof(Communications));
			OnPropertyChanged(nameof(CommunicationStatus));
			OnPropertyChanged(nameof(CommunicationTest));
			OnPropertyChanged(nameof(Compare));
			OnPropertyChanged(nameof(CompareLook));
			OnPropertyChanged(nameof(CompeteCondition));
			OnPropertyChanged(nameof(Composing));
			OnPropertyChanged(nameof(ConditionTimer));
			OnPropertyChanged(nameof(Cone));
			OnPropertyChanged(nameof(ConfigComputerNet));
			OnPropertyChanged(nameof(ConfigSoftwareCom));
			OnPropertyChanged(nameof(ConfigSoftwareNet));
			OnPropertyChanged(nameof(Configuration));
			OnPropertyChanged(nameof(Configure));
			OnPropertyChanged(nameof(Confirm));
			OnPropertyChanged(nameof(ConfirmButton));
			OnPropertyChanged(nameof(ConfirmDelete));
			OnPropertyChanged(nameof(ConfirmDeleteModule));
			OnPropertyChanged(nameof(ConfirmDeleteUser));
			OnPropertyChanged(nameof(ConfirmDeleteVar));
			OnPropertyChanged(nameof(ConfirmThatTheModule));
			OnPropertyChanged(nameof(Content));
			OnPropertyChanged(nameof(Continue));
			OnPropertyChanged(nameof(Coord));
			OnPropertyChanged(nameof(CoordRef));
			OnPropertyChanged(nameof(Copy));
			OnPropertyChanged(nameof(CopyCreate));
			OnPropertyChanged(nameof(CopySelectedModule));
			OnPropertyChanged(nameof(CPKTest));
			OnPropertyChanged(nameof(Create));
			OnPropertyChanged(nameof(CreateProject));
			OnPropertyChanged(nameof(CreateTime));
			OnPropertyChanged(nameof(CropRate));
			OnPropertyChanged(nameof(CropRateSet));
			OnPropertyChanged(nameof(CT));
			OnPropertyChanged(nameof(CTStatistics));
			OnPropertyChanged(nameof(Cuboid));
			OnPropertyChanged(nameof(CurrentProject));
			OnPropertyChanged(nameof(CurrentValue));
			OnPropertyChanged(nameof(CurrentValue2));
			OnPropertyChanged(nameof(Custom));
			OnPropertyChanged(nameof(CustomModule));
			OnPropertyChanged(nameof(Cylinder));
			OnPropertyChanged(nameof(DataBase));
			OnPropertyChanged(nameof(DataDirectory));
			OnPropertyChanged(nameof(DataMark));
			OnPropertyChanged(nameof(DataProc));
			OnPropertyChanged(nameof(DataProcess));
			OnPropertyChanged(nameof(DataTransfer));
			OnPropertyChanged(nameof(DataType));
			OnPropertyChanged(nameof(DataValidation));
			OnPropertyChanged(nameof(Day));
			OnPropertyChanged(nameof(Debug));
			OnPropertyChanged(nameof(DebugFunction));
			OnPropertyChanged(nameof(Dec));
			OnPropertyChanged(nameof(Default));
			OnPropertyChanged(nameof(DefaultPath));
			OnPropertyChanged(nameof(DefaultValue));
			OnPropertyChanged(nameof(Delay));
			OnPropertyChanged(nameof(Delete));
			OnPropertyChanged(nameof(DeleteCustomModule));
			OnPropertyChanged(nameof(DeleteProject));
			OnPropertyChanged(nameof(Denoising));
			OnPropertyChanged(nameof(DepositRate));
			OnPropertyChanged(nameof(DeskName));
			OnPropertyChanged(nameof(Device));
			OnPropertyChanged(nameof(DeviceFirm));
			OnPropertyChanged(nameof(DeviceInfo));
			OnPropertyChanged(nameof(DeviceMonitor));
			OnPropertyChanged(nameof(DeviceName));
			OnPropertyChanged(nameof(DeviceSN));
			OnPropertyChanged(nameof(DeviceState));
			OnPropertyChanged(nameof(DeviceType));
			OnPropertyChanged(nameof(DialogBox));
			OnPropertyChanged(nameof(Digital_In));
			OnPropertyChanged(nameof(Digital_In_Single));
			OnPropertyChanged(nameof(Digital_Out));
			OnPropertyChanged(nameof(Digital_Out_Single));
			OnPropertyChanged(nameof(DigitalAss));
			OnPropertyChanged(nameof(DigitalVision));
			OnPropertyChanged(nameof(DisLine2Line));
			OnPropertyChanged(nameof(DisLine2Plane));
			OnPropertyChanged(nameof(DisMeasure));
			OnPropertyChanged(nameof(Display));
			OnPropertyChanged(nameof(DisplayDirection));
			OnPropertyChanged(nameof(DisplayElementParaSetting));
			OnPropertyChanged(nameof(DisplayTypeSetting));
			OnPropertyChanged(nameof(DisPoint2Line));
			OnPropertyChanged(nameof(DisPoint2Plane));
			OnPropertyChanged(nameof(DisPoint2Point));
			OnPropertyChanged(nameof(Done));
			OnPropertyChanged(nameof(DoorLock));
			OnPropertyChanged(nameof(DoublePage));
			OnPropertyChanged(nameof(DownSampling));
			OnPropertyChanged(nameof(DownTimeReason));
			OnPropertyChanged(nameof(DrawCuboidROI));
			OnPropertyChanged(nameof(DrawCylinderROI));
			OnPropertyChanged(nameof(DrawSphereROI));
			OnPropertyChanged(nameof(DumpPath));
			OnPropertyChanged(nameof(DuplicatePwdCannotBeEmpty));
			OnPropertyChanged(nameof(Edit));
			OnPropertyChanged(nameof(EditClass));
			OnPropertyChanged(nameof(EditFeature));
			OnPropertyChanged(nameof(EditPlcAlarm));
			OnPropertyChanged(nameof(EditUser));
			OnPropertyChanged(nameof(EleCylinder));
			OnPropertyChanged(nameof(Embossing));
			OnPropertyChanged(nameof(EmergencyStop));
			OnPropertyChanged(nameof(EmptyRun));
			OnPropertyChanged(nameof(EmptyRunMode));
			OnPropertyChanged(nameof(Enable));
			OnPropertyChanged(nameof(EnableBuzzer));
			OnPropertyChanged(nameof(EnableLightCurtain));
			OnPropertyChanged(nameof(EnableListening));
			OnPropertyChanged(nameof(EnableSafetyDoor));
			OnPropertyChanged(nameof(End));
			OnPropertyChanged(nameof(EndModule));
			OnPropertyChanged(nameof(EndProduct));
			OnPropertyChanged(nameof(EndTime));
			OnPropertyChanged(nameof(EnterStationName));
			OnPropertyChanged(nameof(EpsonRobot));
			OnPropertyChanged(nameof(Error));
			OnPropertyChanged(nameof(ErrorForeignMessage));
			OnPropertyChanged(nameof(ErrorImgPath));
			OnPropertyChanged(nameof(ErrorImgSize));
			OnPropertyChanged(nameof(Exit));
			OnPropertyChanged(nameof(Export));
			OnPropertyChanged(nameof(ExportData));
			OnPropertyChanged(nameof(ExportFlowTree));
			OnPropertyChanged(nameof(ExportImage));
			OnPropertyChanged(nameof(ExportProject));
			OnPropertyChanged(nameof(ExportRecipe));
			OnPropertyChanged(nameof(Extract));
			OnPropertyChanged(nameof(ExtractAsyncGroup));
			OnPropertyChanged(nameof(ExtractBranchGroup));
			OnPropertyChanged(nameof(ExtractModule));
			OnPropertyChanged(nameof(ExtractNGGroup));
			OnPropertyChanged(nameof(ExtractStepGroup));
			OnPropertyChanged(nameof(ExtractSwitchGroup));
			OnPropertyChanged(nameof(Feeder));
			OnPropertyChanged(nameof(FeedStation));
			OnPropertyChanged(nameof(FFU));
			OnPropertyChanged(nameof(FFUSpeedLevel));
			OnPropertyChanged(nameof(File));
			OnPropertyChanged(nameof(FileAddress));
			OnPropertyChanged(nameof(FileConfig));
			OnPropertyChanged(nameof(FileIO));
			OnPropertyChanged(nameof(FileType));
			OnPropertyChanged(nameof(Filtering));
			OnPropertyChanged(nameof(FinalResult));
			OnPropertyChanged(nameof(Find));
			OnPropertyChanged(nameof(FindCircle));
			OnPropertyChanged(nameof(Finish));
			OnPropertyChanged(nameof(FirstClass));
			OnPropertyChanged(nameof(FirstPieceModeCommand));
			OnPropertyChanged(nameof(FirstPieceModeStatus));
			OnPropertyChanged(nameof(FirstStation));
			OnPropertyChanged(nameof(Fixture));
			OnPropertyChanged(nameof(Flatness));
			OnPropertyChanged(nameof(FlingMaterialStatistics));
			OnPropertyChanged(nameof(Floor));
			OnPropertyChanged(nameof(Flow));
			OnPropertyChanged(nameof(FlowWait));
			OnPropertyChanged(nameof(FlyingPhoto));
			OnPropertyChanged(nameof(ForceAxis));
			OnPropertyChanged(nameof(ForceCollect));
			OnPropertyChanged(nameof(FormatError));
			OnPropertyChanged(nameof(ForMemoryLeakDetection));
			OnPropertyChanged(nameof(Free));
			OnPropertyChanged(nameof(FreeStation));
			OnPropertyChanged(nameof(FTPUpload));
			OnPropertyChanged(nameof(FunctionalModule));
			OnPropertyChanged(nameof(FunctionEnable));
			OnPropertyChanged(nameof(FunctionId));
			OnPropertyChanged(nameof(FunctionManagement));
			OnPropertyChanged(nameof(FX_BindCarrier));
			OnPropertyChanged(nameof(FX_OrderQuery));
			OnPropertyChanged(nameof(FX_RouteQuery));
			OnPropertyChanged(nameof(FX_UnBindCarrier));
			OnPropertyChanged(nameof(FX_UploadResult));
			OnPropertyChanged(nameof(FXContent));
			OnPropertyChanged(nameof(FXTCP));
			OnPropertyChanged(nameof(Gap));
			OnPropertyChanged(nameof(GearRatioNumerator));
			OnPropertyChanged(nameof(GenAxisPos));
			OnPropertyChanged(nameof(GenCircleByData));
			OnPropertyChanged(nameof(GenCircleByFit));
			OnPropertyChanged(nameof(GenCloudByPoints));
			OnPropertyChanged(nameof(GenConeByFit));
			OnPropertyChanged(nameof(GenCoordByData));
			OnPropertyChanged(nameof(GenCoordByPlane3));
			OnPropertyChanged(nameof(GenCuboidByData));
			OnPropertyChanged(nameof(GenCylinderByData));
			OnPropertyChanged(nameof(GenCylinderByFit));
			OnPropertyChanged(nameof(Generate));
			OnPropertyChanged(nameof(GenerationMode));
			OnPropertyChanged(nameof(GenLineBy2Point));
			OnPropertyChanged(nameof(GenLineByCross));
			OnPropertyChanged(nameof(GenLineByData));
			OnPropertyChanged(nameof(GenLineByFit));
			OnPropertyChanged(nameof(GenLineByMove));
			OnPropertyChanged(nameof(GenLineByProj));
			OnPropertyChanged(nameof(GenLineByTransform));
			OnPropertyChanged(nameof(GenPlaneBy3Point));
			OnPropertyChanged(nameof(GenPlaneByFit));
			OnPropertyChanged(nameof(GenPlaneByMove));
			OnPropertyChanged(nameof(GenPlaneByObj));
			OnPropertyChanged(nameof(GenPoint));
			OnPropertyChanged(nameof(GenPointByCaliper));
			OnPropertyChanged(nameof(GenPointByCloud));
			OnPropertyChanged(nameof(GenPointByCMM));
			OnPropertyChanged(nameof(GenPointByCross));
			OnPropertyChanged(nameof(GenPointByData));
			OnPropertyChanged(nameof(GenPointByMove));
			OnPropertyChanged(nameof(GenPointByProj));
			OnPropertyChanged(nameof(GenRandomNumber));
			OnPropertyChanged(nameof(GenReport));
			OnPropertyChanged(nameof(GenSphereByData));
			OnPropertyChanged(nameof(GenSphereByFit));
			OnPropertyChanged(nameof(GenString));
			OnPropertyChanged(nameof(GeometricFeatures));
			OnPropertyChanged(nameof(Geometry));
			OnPropertyChanged(nameof(GetAverage));
			OnPropertyChanged(nameof(GetByDataBase));
			OnPropertyChanged(nameof(GetDirectionByObj));
			OnPropertyChanged(nameof(GetIO));
			OnPropertyChanged(nameof(GetLineByObj));
			OnPropertyChanged(nameof(GetMachineStatus));
			OnPropertyChanged(nameof(GetModbus));
			OnPropertyChanged(nameof(GetPlaneByObj));
			OnPropertyChanged(nameof(GetPointByObj));
			OnPropertyChanged(nameof(GetSlopeIntercept));
			OnPropertyChanged(nameof(Global));
			OnPropertyChanged(nameof(GlobalVar));
			OnPropertyChanged(nameof(GlobalVariable));
			OnPropertyChanged(nameof(GoToModule));
			OnPropertyChanged(nameof(GreenLamp));
			OnPropertyChanged(nameof(Group));
			OnPropertyChanged(nameof(HandledUser));
			OnPropertyChanged(nameof(HandlingMethod));
			OnPropertyChanged(nameof(HardWare));
			OnPropertyChanged(nameof(Height));
			OnPropertyChanged(nameof(Heightfinder));
			OnPropertyChanged(nameof(Help));
			OnPropertyChanged(nameof(HideLabel));
			OnPropertyChanged(nameof(HighCurrentLowLimit));
			OnPropertyChanged(nameof(HighCurrentUpperLimit));
			OnPropertyChanged(nameof(HighLevelTime));
			OnPropertyChanged(nameof(HiveAppId));
			OnPropertyChanged(nameof(HiveConfig));
			OnPropertyChanged(nameof(HiveCT));
			OnPropertyChanged(nameof(HiveIgnoreFeedback));
			OnPropertyChanged(nameof(HiveValve));
			OnPropertyChanged(nameof(Holo3D));
			OnPropertyChanged(nameof(Home));
			OnPropertyChanged(nameof(HomeDone));
			OnPropertyChanged(nameof(HomeStation));
			OnPropertyChanged(nameof(HomeZero));
			OnPropertyChanged(nameof(Horizontal));
			OnPropertyChanged(nameof(ICW));
			OnPropertyChanged(nameof(Idle));
			OnPropertyChanged(nameof(Ignore));
			OnPropertyChanged(nameof(Image));
			OnPropertyChanged(nameof(Import));
			OnPropertyChanged(nameof(ImportParameterName));
			OnPropertyChanged(nameof(ImportRecipe));
			OnPropertyChanged(nameof(InboundTime));
			OnPropertyChanged(nameof(Index));
			OnPropertyChanged(nameof(Info));
			OnPropertyChanged(nameof(Ingore));
			OnPropertyChanged(nameof(InitComplete));
			OnPropertyChanged(nameof(InOutParameterConfigure));
			OnPropertyChanged(nameof(Input));
			OnPropertyChanged(nameof(InputParameter));
			OnPropertyChanged(nameof(InputQty));
			OnPropertyChanged(nameof(Insert));
			OnPropertyChanged(nameof(InsertPoint));
			OnPropertyChanged(nameof(InsightType));
			OnPropertyChanged(nameof(IntegratedHardware));
			OnPropertyChanged(nameof(Interval10m));
			OnPropertyChanged(nameof(Interval1h));
			OnPropertyChanged(nameof(Interval1m));
			OnPropertyChanged(nameof(Interval2h));
			OnPropertyChanged(nameof(Interval30m));
			OnPropertyChanged(nameof(Interval30s));
			OnPropertyChanged(nameof(Interval5m));
			OnPropertyChanged(nameof(IOConform));
			OnPropertyChanged(nameof(IOSimulation));
			OnPropertyChanged(nameof(IPAddress));
			OnPropertyChanged(nameof(IsDeleteCurrentProject));
			OnPropertyChanged(nameof(IsHomeDefault));
			OnPropertyChanged(nameof(IsMemoric));
			OnPropertyChanged(nameof(IsNecessary));
			OnPropertyChanged(nameof(isShieldDoor));
			OnPropertyChanged(nameof(IsVisible));
			OnPropertyChanged(nameof(JOG));
			OnPropertyChanged(nameof(JoinBitToInt));
			OnPropertyChanged(nameof(JSONParse));
			OnPropertyChanged(nameof(Judge));
			OnPropertyChanged(nameof(JudgeString));
			OnPropertyChanged(nameof(Jump));
			OnPropertyChanged(nameof(KeyMaterialQuery));
			OnPropertyChanged(nameof(KeyParameters));
			OnPropertyChanged(nameof(KeywordMatching));
			OnPropertyChanged(nameof(KeyWordWithSymbol));
			OnPropertyChanged(nameof(LADUpload));
			OnPropertyChanged(nameof(LangComment));
			OnPropertyChanged(nameof(LaserScan));
			OnPropertyChanged(nameof(LaserSensor));
			OnPropertyChanged(nameof(LaserVersion));
			OnPropertyChanged(nameof(LastMonthMaintenance));
			OnPropertyChanged(nameof(LastStation));
			OnPropertyChanged(nameof(LastWeekMaintenance));
			OnPropertyChanged(nameof(Lead));
			OnPropertyChanged(nameof(LightController));
			OnPropertyChanged(nameof(LightCurtain));
			OnPropertyChanged(nameof(LightFlashing));
			OnPropertyChanged(nameof(LightingSettings));
			OnPropertyChanged(nameof(LimitsVersion));
			OnPropertyChanged(nameof(Line));
			OnPropertyChanged(nameof(LineLaser));
			OnPropertyChanged(nameof(Liner));
			OnPropertyChanged(nameof(LineScale));
			OnPropertyChanged(nameof(LineScan));
			OnPropertyChanged(nameof(LineWidth));
			OnPropertyChanged(nameof(Load));
			OnPropertyChanged(nameof(LoadCell));
			OnPropertyChanged(nameof(Loading));
			OnPropertyChanged(nameof(LoadingSilo));
			OnPropertyChanged(nameof(Log));
			OnPropertyChanged(nameof(LogBackUpDays));
			OnPropertyChanged(nameof(Logic));
			OnPropertyChanged(nameof(LogicCalculator));
			OnPropertyChanged(nameof(Login));
			OnPropertyChanged(nameof(LoginLevel));
			OnPropertyChanged(nameof(LoginMode));
			OnPropertyChanged(nameof(LoginName));
			OnPropertyChanged(nameof(Logout));
			OnPropertyChanged(nameof(Loop));
			OnPropertyChanged(nameof(LowCurrentLowLimit));
			OnPropertyChanged(nameof(LowCurrentUpperLimit));
			OnPropertyChanged(nameof(LowerLimit));
			OnPropertyChanged(nameof(LSMesUnLoad));
			OnPropertyChanged(nameof(CableSNManager));
			OnPropertyChanged(nameof(LusterSmartCockpit));
			OnPropertyChanged(nameof(MacAddress));
			OnPropertyChanged(nameof(Machine));
			OnPropertyChanged(nameof(MachineConfigure));
			OnPropertyChanged(nameof(MainCarrierNum));
			OnPropertyChanged(nameof(MainParameters));
			OnPropertyChanged(nameof(Maintenance));
			OnPropertyChanged(nameof(ManageDept_Vision));
			OnPropertyChanged(nameof(Manual));
			OnPropertyChanged(nameof(ManualGetBarcode));
			OnPropertyChanged(nameof(ManualSwitch));
			OnPropertyChanged(nameof(Material));
			OnPropertyChanged(nameof(MaterialNotObtained));
			OnPropertyChanged(nameof(MaxPerPage));
			OnPropertyChanged(nameof(MergePoints));
			OnPropertyChanged(nameof(Mesh));
			OnPropertyChanged(nameof(MiddleCurrentLowLimit));
			OnPropertyChanged(nameof(MiddleCurrentUpperLimit));
			OnPropertyChanged(nameof(Miscellaneous));
			OnPropertyChanged(nameof(ModbusRTU));
			OnPropertyChanged(nameof(Model));
			OnPropertyChanged(nameof(ModelDisplayMode));
			OnPropertyChanged(nameof(ModelGroup));
			OnPropertyChanged(nameof(Module));
			OnPropertyChanged(nameof(Module_Name));
			OnPropertyChanged(nameof(ModuleError));
			OnPropertyChanged(nameof(ModuleName));
			OnPropertyChanged(nameof(ModuleSet));
			OnPropertyChanged(nameof(Month));
			OnPropertyChanged(nameof(Morphological));
			OnPropertyChanged(nameof(Motion));
			OnPropertyChanged(nameof(MotionCard));
			OnPropertyChanged(nameof(MotionPriorityOfEachAxisInMultipleScenes));
			OnPropertyChanged(nameof(MotionSpeed));
			OnPropertyChanged(nameof(MotionSpeed_mm_s_));
			OnPropertyChanged(nameof(MotionSpeedWithUnit));
			OnPropertyChanged(nameof(MoveDirection));
			OnPropertyChanged(nameof(MovePosition));
			OnPropertyChanged(nameof(MovePostion_mm_));
			OnPropertyChanged(nameof(MoveTo));
			OnPropertyChanged(nameof(MultiAxis));
			OnPropertyChanged(nameof(Name));
			OnPropertyChanged(nameof(NewPressurize));
			OnPropertyChanged(nameof(NewProject));
			OnPropertyChanged(nameof(NewRecipe));
			OnPropertyChanged(nameof(NextGet));
			OnPropertyChanged(nameof(NextPage));
			OnPropertyChanged(nameof(NG));
			OnPropertyChanged(nameof(NGAmount));
			OnPropertyChanged(nameof(NGGroup));
			OnPropertyChanged(nameof(NGRate));
			OnPropertyChanged(nameof(NGReason));
			OnPropertyChanged(nameof(NGStation));
			OnPropertyChanged(nameof(No));
			OnPropertyChanged(nameof(NoData));
			OnPropertyChanged(nameof(NoMatchDeviceFound));
			OnPropertyChanged(nameof(NoRecipeInProject));
			OnPropertyChanged(nameof(NotFoundActiveRecipePath));
			OnPropertyChanged(nameof(Null));
			OnPropertyChanged(nameof(NumberOfCycles));
			OnPropertyChanged(nameof(ObtainSwVersion));
			OnPropertyChanged(nameof(OffLineMode));
			OnPropertyChanged(nameof(OKAmount));
			OnPropertyChanged(nameof(OKRate));
			OnPropertyChanged(nameof(OnLineMode));
			OnPropertyChanged(nameof(Opacity));
			OnPropertyChanged(nameof(Open));
			OnPropertyChanged(nameof(OpenCloseDoor));
			OnPropertyChanged(nameof(OpenProject));
			OnPropertyChanged(nameof(OPenPrompt));
			OnPropertyChanged(nameof(Operate));
			OnPropertyChanged(nameof(OperateTime));
			OnPropertyChanged(nameof(OperateType));
			OnPropertyChanged(nameof(OperatingTips));
			OnPropertyChanged(nameof(OperationType));
			OnPropertyChanged(nameof(Order));
			OnPropertyChanged(nameof(OriginalPassword));
			OnPropertyChanged(nameof(OriginalPassWordWrong));
			OnPropertyChanged(nameof(OriginLimit));
			OnPropertyChanged(nameof(Others));
			OnPropertyChanged(nameof(OutBoundTime));
			OnPropertyChanged(nameof(OutIO));
			OnPropertyChanged(nameof(OutlierFit));
			OnPropertyChanged(nameof(OutOfRange));
			OnPropertyChanged(nameof(OutParam));
			OnPropertyChanged(nameof(OutportParameterName));
			OnPropertyChanged(nameof(Output));
			OnPropertyChanged(nameof(OutPutItem));
			OnPropertyChanged(nameof(OutPutItemUnSave));
			OnPropertyChanged(nameof(OutputParameter));
			OnPropertyChanged(nameof(PageControl));
			OnPropertyChanged(nameof(PageMode));
			OnPropertyChanged(nameof(Parallel));
			OnPropertyChanged(nameof(Parallelism));
			OnPropertyChanged(nameof(Parameter));
			OnPropertyChanged(nameof(ParameterConfig));
			OnPropertyChanged(nameof(ParameterConfigure));
			OnPropertyChanged(nameof(ParseString));
			OnPropertyChanged(nameof(PartName));
			OnPropertyChanged(nameof(PassWord));
			OnPropertyChanged(nameof(PassWordError));
			OnPropertyChanged(nameof(Paste));
			OnPropertyChanged(nameof(Pause));
			OnPropertyChanged(nameof(Paused));
			OnPropertyChanged(nameof(PauseLamp));
			OnPropertyChanged(nameof(PCHeartbeat));
			OnPropertyChanged(nameof(PCRelevant));
			OnPropertyChanged(nameof(PCStatus));
			OnPropertyChanged(nameof(PDCA));
			OnPropertyChanged(nameof(PDCAELimit));
			OnPropertyChanged(nameof(PDCAELimt));
			OnPropertyChanged(nameof(PDCAFailRetry));
			OnPropertyChanged(nameof(PDCAFlow));
			OnPropertyChanged(nameof(PDCAWIP));
			OnPropertyChanged(nameof(PDOAction));
			OnPropertyChanged(nameof(PickLine));
			OnPropertyChanged(nameof(PickPlane));
			OnPropertyChanged(nameof(PickPoint));
			OnPropertyChanged(nameof(PictureStoragePath));
			OnPropertyChanged(nameof(Plane));
			OnPropertyChanged(nameof(PlantArea));
			OnPropertyChanged(nameof(PLC));
			OnPropertyChanged(nameof(PLCAddress));
			OnPropertyChanged(nameof(PLCClearMistake));
			OnPropertyChanged(nameof(PLCConfigure));
			OnPropertyChanged(nameof(PLCServer));
			OnPropertyChanged(nameof(PlcStation));
			OnPropertyChanged(nameof(PLCStatus));
			OnPropertyChanged(nameof(PlcVersion));
			OnPropertyChanged(nameof(Please_Enter_AlarmCode));
			OnPropertyChanged(nameof(PleaseCheckTheDataFormatorType));
			OnPropertyChanged(nameof(PleaseEnterAnIntegerGreaterThan0));
			OnPropertyChanged(nameof(PleaseEnterASingleByteDelimiter));
			OnPropertyChanged(nameof(PleaseEnterConditions));
			OnPropertyChanged(nameof(PleaseEnterSNCode));
			OnPropertyChanged(nameof(Pm));
			OnPropertyChanged(nameof(PngImg));
			OnPropertyChanged(nameof(Point));
			OnPropertyChanged(nameof(Point2Point));
			OnPropertyChanged(nameof(PointCloud));
			OnPropertyChanged(nameof(PointCloudDisplayMode));
			OnPropertyChanged(nameof(PointCloudGroup));
			OnPropertyChanged(nameof(PointCloudSize));
			OnPropertyChanged(nameof(PointerCoord));
			OnPropertyChanged(nameof(PointSize));
			OnPropertyChanged(nameof(PointTeaching));
			OnPropertyChanged(nameof(PositionOutput));
			OnPropertyChanged(nameof(PosLocation));
			OnPropertyChanged(nameof(PressDriver));
			OnPropertyChanged(nameof(PressForm1));
			OnPropertyChanged(nameof(PressForm2));
			OnPropertyChanged(nameof(PressForm3));
			OnPropertyChanged(nameof(PressForm4));
			OnPropertyChanged(nameof(PressForm5));
			OnPropertyChanged(nameof(PressureRepetition));
			OnPropertyChanged(nameof(PressureSensor));
			OnPropertyChanged(nameof(Pressurize));
			OnPropertyChanged(nameof(PrevHave));
			OnPropertyChanged(nameof(Preview));
			OnPropertyChanged(nameof(PreviousPage));
			OnPropertyChanged(nameof(Print));
			OnPropertyChanged(nameof(Printer));
			OnPropertyChanged(nameof(PrintPreview));
			OnPropertyChanged(nameof(PrintSet));
			OnPropertyChanged(nameof(Priority));
			OnPropertyChanged(nameof(Product_Vision));
			OnPropertyChanged(nameof(ProductAmount));
			OnPropertyChanged(nameof(ProductInfo));
			OnPropertyChanged(nameof(ProductNG));
			OnPropertyChanged(nameof(ProductStatistics));
			OnPropertyChanged(nameof(ProEvent));
			OnPropertyChanged(nameof(Profileanysurface));
			OnPropertyChanged(nameof(ProgramMustStop));
			OnPropertyChanged(nameof(ProgramStop));
			OnPropertyChanged(nameof(Project));
			OnPropertyChanged(nameof(ProjectAddress));
			OnPropertyChanged(nameof(ProjectAlreadyExists));
			OnPropertyChanged(nameof(ProjectCannotBeEmpty));
			OnPropertyChanged(nameof(ProjectFileError));
			OnPropertyChanged(nameof(ProjectName));
			OnPropertyChanged(nameof(ProjectProperty));
			OnPropertyChanged(nameof(ProLoaded));
			OnPropertyChanged(nameof(PromptContent));
			OnPropertyChanged(nameof(Property));
			OnPropertyChanged(nameof(Protocol));
			OnPropertyChanged(nameof(PulseCal));
			OnPropertyChanged(nameof(PwdCannotBeEmpty));
			OnPropertyChanged(nameof(PwdInconsistent));
			OnPropertyChanged(nameof(Query));
			OnPropertyChanged(nameof(Quit));
			OnPropertyChanged(nameof(QuitSoftWare));
			OnPropertyChanged(nameof(R_Acceleration_Target));
			OnPropertyChanged(nameof(R_AccelerationTime_Actual));
			OnPropertyChanged(nameof(R_Speed_Actual));
			OnPropertyChanged(nameof(R_Speed_Target));
			OnPropertyChanged(nameof(ReadCAD));
			OnPropertyChanged(nameof(ReadCloud));
			OnPropertyChanged(nameof(ReadDataFile));
			OnPropertyChanged(nameof(ReadDatas));
			OnPropertyChanged(nameof(ReadFins));
			OnPropertyChanged(nameof(ReadMatrix));
			OnPropertyChanged(nameof(ReadMC));
			OnPropertyChanged(nameof(ReadModbus));
			OnPropertyChanged(nameof(ReadPlc));
			OnPropertyChanged(nameof(ReadRobotSpeed));
			OnPropertyChanged(nameof(ReadSTL));
			OnPropertyChanged(nameof(RealTime));
			OnPropertyChanged(nameof(RealTimeLocation));
			OnPropertyChanged(nameof(Reason));
			OnPropertyChanged(nameof(RecentFile));
			OnPropertyChanged(nameof(RecentProject));
			OnPropertyChanged(nameof(ReCheck));
			OnPropertyChanged(nameof(ReciepeVersion));
			OnPropertyChanged(nameof(Recipe));
			OnPropertyChanged(nameof(RecipeBackUpDays));
			OnPropertyChanged(nameof(RecipeFormatWrong));
			OnPropertyChanged(nameof(Recoverey));
			OnPropertyChanged(nameof(Recovery));
			OnPropertyChanged(nameof(RecoveryLamp));
			OnPropertyChanged(nameof(RedLamp));
			OnPropertyChanged(nameof(Redo));
			OnPropertyChanged(nameof(Reference_DoubleClick_));
			OnPropertyChanged(nameof(RefGroup));
			OnPropertyChanged(nameof(Refresh));
			OnPropertyChanged(nameof(RefreshFrequency));
			OnPropertyChanged(nameof(Region));
			OnPropertyChanged(nameof(Register));
			OnPropertyChanged(nameof(RegisterType));
			OnPropertyChanged(nameof(Registration));
			OnPropertyChanged(nameof(Relative));
			OnPropertyChanged(nameof(RelativeMotion));
			OnPropertyChanged(nameof(ReleaseNotes));
			OnPropertyChanged(nameof(Remove));
			OnPropertyChanged(nameof(Rename));
			OnPropertyChanged(nameof(RepeatPassword));
			OnPropertyChanged(nameof(ReplaceMaterials));
			OnPropertyChanged(nameof(Report));
			OnPropertyChanged(nameof(ReportContents));
			OnPropertyChanged(nameof(ReportForm));
			OnPropertyChanged(nameof(ReportNavigation));
			OnPropertyChanged(nameof(ReportSource));
			OnPropertyChanged(nameof(ReportType));
			OnPropertyChanged(nameof(Reset));
			OnPropertyChanged(nameof(ResetCapacity));
			OnPropertyChanged(nameof(ResetStation));
			OnPropertyChanged(nameof(ResetVariable));
			OnPropertyChanged(nameof(RestartTakesEffect));
			OnPropertyChanged(nameof(Result));
			OnPropertyChanged(nameof(Retry));
			OnPropertyChanged(nameof(Return));
			OnPropertyChanged(nameof(Revoke));
			OnPropertyChanged(nameof(Robot));
			OnPropertyChanged(nameof(RobotAction));
			OnPropertyChanged(nameof(RobotAction2));
			OnPropertyChanged(nameof(RobotInfo));
			OnPropertyChanged(nameof(RobotMove));
			OnPropertyChanged(nameof(RobotStatus));
			OnPropertyChanged(nameof(RobotStatus2));
			OnPropertyChanged(nameof(RobotVersion));
			OnPropertyChanged(nameof(ROIConfig));
			OnPropertyChanged(nameof(RollSet));
			OnPropertyChanged(nameof(RoolMaterialCal));
			OnPropertyChanged(nameof(RotateViewClockwise));
			OnPropertyChanged(nameof(RotateViewCounterclockwise));
			OnPropertyChanged(nameof(Roundness));
			OnPropertyChanged(nameof(RoutineMonitoring));
			OnPropertyChanged(nameof(Run));
			OnPropertyChanged(nameof(RunAllF5));
			OnPropertyChanged(nameof(RunExe));
			OnPropertyChanged(nameof(RunMode));
			OnPropertyChanged(nameof(RunModeIsAlreadyExist));
			OnPropertyChanged(nameof(RunModeTips));
			OnPropertyChanged(nameof(Runners));
			OnPropertyChanged(nameof(RunNext));
			OnPropertyChanged(nameof(Running));
			OnPropertyChanged(nameof(RunningTime));
			OnPropertyChanged(nameof(RunOne));
			OnPropertyChanged(nameof(s));
			OnPropertyChanged(nameof(Save));
			OnPropertyChanged(nameof(SaveAs));
			OnPropertyChanged(nameof(SaveAsProject));
			OnPropertyChanged(nameof(SaveDays));
			OnPropertyChanged(nameof(SaveFile));
			OnPropertyChanged(nameof(SaveProject));
			OnPropertyChanged(nameof(SaveProjectTask));
			OnPropertyChanged(nameof(SaveSuccess));
			OnPropertyChanged(nameof(SaveTask));
			OnPropertyChanged(nameof(ScanBarcode));
			OnPropertyChanged(nameof(ScanCodeCount));
			OnPropertyChanged(nameof(ScanCodeDataSource));
			OnPropertyChanged(nameof(ScanCodeStatistics));
			OnPropertyChanged(nameof(ScanCodeSuccessRate));
			OnPropertyChanged(nameof(Scram));
			OnPropertyChanged(nameof(Script));
			OnPropertyChanged(nameof(ScrollMode));
			OnPropertyChanged(nameof(SDOAction));
			OnPropertyChanged(nameof(SearchFileKeywords));
			OnPropertyChanged(nameof(SearchModule));
			OnPropertyChanged(nameof(Second));
			OnPropertyChanged(nameof(Segment));
			OnPropertyChanged(nameof(Select));
			OnPropertyChanged(nameof(SelectedAxis));
			OnPropertyChanged(nameof(SelectFile));
			OnPropertyChanged(nameof(Semicolon));
			OnPropertyChanged(nameof(Senior));
			OnPropertyChanged(nameof(Separator));
			OnPropertyChanged(nameof(SerialNumber));
			OnPropertyChanged(nameof(SerialPortDrive));
			OnPropertyChanged(nameof(Server));
			OnPropertyChanged(nameof(Set));
			OnPropertyChanged(nameof(SetAxisPos));
			OnPropertyChanged(nameof(SetCoordTemplate));
			OnPropertyChanged(nameof(SetDisPlayOption));
			OnPropertyChanged(nameof(SetGlobalVar));
			OnPropertyChanged(nameof(SetIO));
			OnPropertyChanged(nameof(SetLightCurtain));
			OnPropertyChanged(nameof(SetMachineMode));
			OnPropertyChanged(nameof(SetMeasure));
			OnPropertyChanged(nameof(SetModbus));
			OnPropertyChanged(nameof(SetModbusEx));
			OnPropertyChanged(nameof(SetRobotStatus));
			OnPropertyChanged(nameof(SetStation));
			OnPropertyChanged(nameof(SetVariable));
			OnPropertyChanged(nameof(SetWorkFlow));
			OnPropertyChanged(nameof(SFC));
			OnPropertyChanged(nameof(SFCFlow));
			OnPropertyChanged(nameof(SFCFlowTiaoJi));
			OnPropertyChanged(nameof(SFTPUpload));
			OnPropertyChanged(nameof(SignalLamp));
			OnPropertyChanged(nameof(SingelAxisFlyShot));
			OnPropertyChanged(nameof(SingleAxis));
			OnPropertyChanged(nameof(SingleCT));
			OnPropertyChanged(nameof(SinglePage));
			OnPropertyChanged(nameof(SinleAxis));
			OnPropertyChanged(nameof(Skip));
			OnPropertyChanged(nameof(SmokeAlarmDevice));
			OnPropertyChanged(nameof(Smooth));
			OnPropertyChanged(nameof(SNCode));
			OnPropertyChanged(nameof(Soft));
			OnPropertyChanged(nameof(SoftConfigure));
			OnPropertyChanged(nameof(SoftInformation));
			OnPropertyChanged(nameof(SoftVersion));
			OnPropertyChanged(nameof(SoftWareStopByClick));
			OnPropertyChanged(nameof(SoftWareUpdateInfo));
			OnPropertyChanged(nameof(SoftWareVersion));
			OnPropertyChanged(nameof(Solution));
			OnPropertyChanged(nameof(SolutionMode));
			OnPropertyChanged(nameof(Source));
			OnPropertyChanged(nameof(Space));
			OnPropertyChanged(nameof(SpaceFactor));
			OnPropertyChanged(nameof(SparePartsUsed));
			OnPropertyChanged(nameof(SpecSet));
			OnPropertyChanged(nameof(SpeedFactor));
			OnPropertyChanged(nameof(Sphere));
			OnPropertyChanged(nameof(SplitIntToBit));
			OnPropertyChanged(nameof(SplitString));
			OnPropertyChanged(nameof(StandardValue));
			OnPropertyChanged(nameof(Start));
			OnPropertyChanged(nameof(StartColumn));
			OnPropertyChanged(nameof(StartLamp));
			OnPropertyChanged(nameof(StartModule));
			OnPropertyChanged(nameof(StartRepair));
			OnPropertyChanged(nameof(StartRow));
			OnPropertyChanged(nameof(StartStation));
			OnPropertyChanged(nameof(StartTime));
			OnPropertyChanged(nameof(State));
			OnPropertyChanged(nameof(Station));
			OnPropertyChanged(nameof(StationID));
			OnPropertyChanged(nameof(StationName));
			OnPropertyChanged(nameof(StationOverview));
			OnPropertyChanged(nameof(Stations));
			OnPropertyChanged(nameof(StationSet));
			OnPropertyChanged(nameof(StationType));
			OnPropertyChanged(nameof(Statistics));
			OnPropertyChanged(nameof(Status));
			OnPropertyChanged(nameof(Step));
			OnPropertyChanged(nameof(StepGroup));
			OnPropertyChanged(nameof(STL));
			OnPropertyChanged(nameof(Stop));
			OnPropertyChanged(nameof(StopF8));
			OnPropertyChanged(nameof(StopLamp));
			OnPropertyChanged(nameof(Straightness));
			OnPropertyChanged(nameof(StringMerge));
			OnPropertyChanged(nameof(StringParse));
			OnPropertyChanged(nameof(SubCarrierNum));
			OnPropertyChanged(nameof(SuctionNozzle));
			OnPropertyChanged(nameof(Switch));
			OnPropertyChanged(nameof(SwitchGroup));
			OnPropertyChanged(nameof(SysOperateInfo));
			OnPropertyChanged(nameof(SysOperationIO));
			OnPropertyChanged(nameof(System_Operation_Information));
			OnPropertyChanged(nameof(SystemOperationPromptInformation));
			OnPropertyChanged(nameof(SystemOperationTips));
			OnPropertyChanged(nameof(TableCreate));
			OnPropertyChanged(nameof(TableInsert));
			OnPropertyChanged(nameof(TaikeAnnotatedCurve));
			OnPropertyChanged(nameof(DHVCMMonitor));
			OnPropertyChanged(nameof(TaikeContent));
			OnPropertyChanged(nameof(TaikeCurve));
			OnPropertyChanged(nameof(TaiKeScrewDriver));
			OnPropertyChanged(nameof(Target_CT));
			OnPropertyChanged(nameof(TargetUnitOfMotion));
			OnPropertyChanged(nameof(TargetWithUnit));
			OnPropertyChanged(nameof(TaskFlow));
			OnPropertyChanged(nameof(TaskSimulator));
			OnPropertyChanged(nameof(Teach));
			OnPropertyChanged(nameof(TeachLocation));
			OnPropertyChanged(nameof(Tearing));
			OnPropertyChanged(nameof(TechnologicalProcess));
			OnPropertyChanged(nameof(TestBotton));
			OnPropertyChanged(nameof(TestStation));
			OnPropertyChanged(nameof(ThisGet));
			OnPropertyChanged(nameof(ThisHave));
			OnPropertyChanged(nameof(ThreeDimision));
			OnPropertyChanged(nameof(ThrowingSetting));
			OnPropertyChanged(nameof(ThrowingTime));
			OnPropertyChanged(nameof(Time));
			OnPropertyChanged(nameof(TimeLogEvent));
			OnPropertyChanged(nameof(TimerJudge));
			OnPropertyChanged(nameof(Tip));
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(To_Be_Initialized));
			OnPropertyChanged(nameof(Tolerance));
			OnPropertyChanged(nameof(TolParameter));
			OnPropertyChanged(nameof(Tool));
			OnPropertyChanged(nameof(TooLarge));
			OnPropertyChanged(nameof(TorqueForm));
			OnPropertyChanged(nameof(TorqueForm2));
			OnPropertyChanged(nameof(TotalCarrierNum));
			OnPropertyChanged(nameof(TotalCodeSweep));
			OnPropertyChanged(nameof(TotalTime));
			OnPropertyChanged(nameof(Train));
			OnPropertyChanged(nameof(Transform));
			OnPropertyChanged(nameof(TransformCoord));
			OnPropertyChanged(nameof(TriColorStatus));
			OnPropertyChanged(nameof(Turntable));
			OnPropertyChanged(nameof(TwoD));
			OnPropertyChanged(nameof(TwoDimision));
			OnPropertyChanged(nameof(TwoPageMode));
			OnPropertyChanged(nameof(TxtReader));
			OnPropertyChanged(nameof(TxtWriter));
			OnPropertyChanged(nameof(Type));
			OnPropertyChanged(nameof(TypeNotSupported));
			OnPropertyChanged(nameof(U_Acceleration_Target));
			OnPropertyChanged(nameof(U_AccelerationTime_Actual));
			OnPropertyChanged(nameof(U_Speed_Actual));
			OnPropertyChanged(nameof(U_Speed_Target));
			OnPropertyChanged(nameof(Unit));
			OnPropertyChanged(nameof(Unknown));
			OnPropertyChanged(nameof(UnknownPLCStatus));
			OnPropertyChanged(nameof(UnknownSize));
			OnPropertyChanged(nameof(UnKnownStation));
			OnPropertyChanged(nameof(UnLoadingSilo));
			OnPropertyChanged(nameof(Update));
			OnPropertyChanged(nameof(UpdateContent));
			OnPropertyChanged(nameof(UpdateVar));
			OnPropertyChanged(nameof(UploadData));
			OnPropertyChanged(nameof(UpperLimit));
			OnPropertyChanged(nameof(Use));
			OnPropertyChanged(nameof(UseID));
			OnPropertyChanged(nameof(UserConfigure));
			OnPropertyChanged(nameof(UserList));
			OnPropertyChanged(nameof(UserName));
			OnPropertyChanged(nameof(UsingTutorials));
			OnPropertyChanged(nameof(VA));
			OnPropertyChanged(nameof(Vacuum));
			OnPropertyChanged(nameof(Value));
			OnPropertyChanged(nameof(VAxis));
			OnPropertyChanged(nameof(VAxis3));
			OnPropertyChanged(nameof(VAxisM));
			OnPropertyChanged(nameof(VBelt));
			OnPropertyChanged(nameof(VButton));
			OnPropertyChanged(nameof(VCamera));
			OnPropertyChanged(nameof(VCommuncation));
			OnPropertyChanged(nameof(VCylinder));
			OnPropertyChanged(nameof(VDevice));
			OnPropertyChanged(nameof(VersionInfoDialog));
			OnPropertyChanged(nameof(VESD));
			OnPropertyChanged(nameof(VFeeder));
			OnPropertyChanged(nameof(View));
			OnPropertyChanged(nameof(ViewCentered));
			OnPropertyChanged(nameof(ViewDirectionSetting));
			OnPropertyChanged(nameof(ViewFlip));
			OnPropertyChanged(nameof(VInputIO));
			OnPropertyChanged(nameof(VIO));
			OnPropertyChanged(nameof(VIOSimulation));
			OnPropertyChanged(nameof(VisibleLabel));
			OnPropertyChanged(nameof(Vision));
			OnPropertyChanged(nameof(VisionCalibration));
			OnPropertyChanged(nameof(VisionExtra));
			OnPropertyChanged(nameof(VisionInformation));
			OnPropertyChanged(nameof(VisionIP));
			OnPropertyChanged(nameof(VisionPort));
			OnPropertyChanged(nameof(VisionProcessData));
			OnPropertyChanged(nameof(VisionStationId));
			OnPropertyChanged(nameof(VisionVersion));
			OnPropertyChanged(nameof(VLineLaser));
			OnPropertyChanged(nameof(VOutputIO));
			OnPropertyChanged(nameof(VPCylinder));
			OnPropertyChanged(nameof(VPlc));
			OnPropertyChanged(nameof(VPrinter));
			OnPropertyChanged(nameof(VTricolorlamp));
			OnPropertyChanged(nameof(VVacuum));
			OnPropertyChanged(nameof(Wait));
			OnPropertyChanged(nameof(WaitCondition));
			OnPropertyChanged(nameof(WaitFins));
			OnPropertyChanged(nameof(WaitIO));
			OnPropertyChanged(nameof(WaitMC));
			OnPropertyChanged(nameof(WaitModbus));
			OnPropertyChanged(nameof(WaitPlc));
			OnPropertyChanged(nameof(WaitStatus));
			OnPropertyChanged(nameof(Warning));
			OnPropertyChanged(nameof(WebConfigRead));
			OnPropertyChanged(nameof(WebHttp));
			OnPropertyChanged(nameof(Week));
			OnPropertyChanged(nameof(Width));
			OnPropertyChanged(nameof(WipPrint));
			OnPropertyChanged(nameof(WorkFlow));
			OnPropertyChanged(nameof(WorkOrder));
			OnPropertyChanged(nameof(WriteFins));
			OnPropertyChanged(nameof(WriteMC));
			OnPropertyChanged(nameof(WritePlc));
			OnPropertyChanged(nameof(Wrong));
			OnPropertyChanged(nameof(X_Acceleration_Target));
			OnPropertyChanged(nameof(X_AccelerationTime_Actual));
			OnPropertyChanged(nameof(X_Speed_Actual));
			OnPropertyChanged(nameof(X_Speed_Target));
			OnPropertyChanged(nameof(XJCPressureSensor));
			OnPropertyChanged(nameof(XJCPressureSensorF600));
			OnPropertyChanged(nameof(Y_Acceleration_Target));
			OnPropertyChanged(nameof(Y_AccelerationTime_Actual));
			OnPropertyChanged(nameof(Y_Speed_Actual));
			OnPropertyChanged(nameof(Y_Speed_Target));
			OnPropertyChanged(nameof(YellowLamp));
			OnPropertyChanged(nameof(Yes));
			OnPropertyChanged(nameof(Yield));
			OnPropertyChanged(nameof(Z_Acceleration_Target));
			OnPropertyChanged(nameof(Z_AccelerationTime_Actual));
			OnPropertyChanged(nameof(Z_Speed_Actual));
			OnPropertyChanged(nameof(Z_Speed_Target));
			OnPropertyChanged(nameof(ZAxisSafeRegion));
			OnPropertyChanged(nameof(Zoom));
			OnPropertyChanged(nameof(ZoomIn));
			OnPropertyChanged(nameof(ZoomOut));
        }

        /// <summary>
        ///   查找类似 绝对 的本地化字符串。
        /// </summary>
		public string Absolutely => Lang.Absolutely;

        /// <summary>
        ///   查找类似 绝对运动 的本地化字符串。
        /// </summary>
		public string AbsoluteMotion => Lang.AbsoluteMotion;

        /// <summary>
        ///   查找类似 加速度 的本地化字符串。
        /// </summary>
		public string Acc => Lang.Acc;

        /// <summary>
        ///   查找类似 辅料数量 的本地化字符串。
        /// </summary>
		public string AccessoriesAmount => Lang.AccessoriesAmount;

        /// <summary>
        ///   查找类似 辅料批号 的本地化字符串。
        /// </summary>
		public string AccessoriesBatch => Lang.AccessoriesBatch;

        /// <summary>
        ///   查找类似 辅料名称 的本地化字符串。
        /// </summary>
		public string AccessoriesName => Lang.AccessoriesName;

        /// <summary>
        ///   查找类似 动作 的本地化字符串。
        /// </summary>
		public string Action => Lang.Action;

        /// <summary>
        ///   查找类似 激活 的本地化字符串。
        /// </summary>
		public string Active => Lang.Active;

        /// <summary>
        ///   查找类似 未找到激活的配方 的本地化字符串。
        /// </summary>
		public string ActiveRecipeNotFound => Lang.ActiveRecipeNotFound;

        /// <summary>
        ///   查找类似 Actual_CT 的本地化字符串。
        /// </summary>
		public string Actual_CT => Lang.Actual_CT;

        /// <summary>
        ///   查找类似 真实速度=系数*速度 的本地化字符串。
        /// </summary>
		public string ActualSpeed => Lang.ActualSpeed;

        /// <summary>
        ///   查找类似 新增 的本地化字符串。
        /// </summary>
		public string Add => Lang.Add;

        /// <summary>
        ///   查找类似 添加班别 的本地化字符串。
        /// </summary>
		public string AddClass => Lang.AddClass;

        /// <summary>
        ///   查找类似 添加Plc报警 的本地化字符串。
        /// </summary>
		public string AddPlcAlarm => Lang.AddPlcAlarm;

        /// <summary>
        ///   查找类似 添加产品 的本地化字符串。
        /// </summary>
		public string AddProduct => Lang.AddProduct;

        /// <summary>
        ///   查找类似 添加用户 的本地化字符串。
        /// </summary>
		public string AddUser => Lang.AddUser;

        /// <summary>
        ///   查找类似 报警 的本地化字符串。
        /// </summary>
		public string Alarm => Lang.Alarm;

        /// <summary>
        ///   查找类似 报警地址 的本地化字符串。
        /// </summary>
		public string AlarmAddress => Lang.AlarmAddress;

        /// <summary>
        ///   查找类似 报警代码 的本地化字符串。
        /// </summary>
		public string AlarmCode => Lang.AlarmCode;

        /// <summary>
        ///   查找类似 报警配置 的本地化字符串。
        /// </summary>
		public string AlarmConfigure => Lang.AlarmConfigure;

        /// <summary>
        ///   查找类似 报警内容 的本地化字符串。
        /// </summary>
		public string AlarmContent => Lang.AlarmContent;

        /// <summary>
        ///   查找类似 报警详细信息 的本地化字符串。
        /// </summary>
		public string AlarmDetailInfo => Lang.AlarmDetailInfo;

        /// <summary>
        ///   查找类似 报警英文 的本地化字符串。
        /// </summary>
		public string AlarmEnglish => Lang.AlarmEnglish;

        /// <summary>
        ///   查找类似 报警ID 的本地化字符串。
        /// </summary>
		public string AlarmID => Lang.AlarmID;

        /// <summary>
        ///   查找类似 报警信息 的本地化字符串。
        /// </summary>
		public string AlarmInfo => Lang.AlarmInfo;

        /// <summary>
        ///   查找类似 报警中 的本地化字符串。
        /// </summary>
		public string Alarming => Lang.Alarming;

        /// <summary>
        ///   查找类似 报警监控 的本地化字符串。
        /// </summary>
		public string AlarmMonitoring => Lang.AlarmMonitoring;

        /// <summary>
        ///   查找类似 报警处理方式 的本地化字符串。
        /// </summary>
		public string AlarmSolution => Lang.AlarmSolution;

        /// <summary>
        ///   查找类似 报警时长 的本地化字符串。
        /// </summary>
		public string AlarmTime => Lang.AlarmTime;

        /// <summary>
        ///   查找类似 报警类型 的本地化字符串。
        /// </summary>
		public string AlarmType => Lang.AlarmType;

        /// <summary>
        ///   查找类似 算法 的本地化字符串。
        /// </summary>
		public string Algorithm => Lang.Algorithm;

        /// <summary>
        ///   查找类似 最佳拟合对齐 的本地化字符串。
        /// </summary>
		public string AlignByBestFit => Lang.AlignByBestFit;

        /// <summary>
        ///   查找类似 坐标系对齐 的本地化字符串。
        /// </summary>
		public string AlignByCoord => Lang.AlignByCoord;

        /// <summary>
        ///   查找类似 初始对齐 的本地化字符串。
        /// </summary>
		public string AlignByInit => Lang.AlignByInit;

        /// <summary>
        ///   查找类似 RPS对齐 的本地化字符串。
        /// </summary>
		public string AlignByRPS => Lang.AlignByRPS;

        /// <summary>
        ///   查找类似 对齐 的本地化字符串。
        /// </summary>
		public string Alignment => Lang.Alignment;

        /// <summary>
        ///   查找类似 全部 的本地化字符串。
        /// </summary>
		public string All => Lang.All;

        /// <summary>
        ///   查找类似 已存在 的本地化字符串。
        /// </summary>
		public string AlreadyExists => Lang.AlreadyExists;

        /// <summary>
        ///   查找类似 上午 的本地化字符串。
        /// </summary>
		public string Am => Lang.Am;

        /// <summary>
        ///   查找类似 模拟量转换 的本地化字符串。
        /// </summary>
		public string AnalogConvert => Lang.AnalogConvert;

        /// <summary>
        ///   查找类似 角度测量 的本地化字符串。
        /// </summary>
		public string AngleMeasure => Lang.AngleMeasure;

        /// <summary>
        ///   查找类似 注释生成 的本地化字符串。
        /// </summary>
		public string AnnotationGeneration => Lang.AnnotationGeneration;

        /// <summary>
        ///   查找类似 AOI 的本地化字符串。
        /// </summary>
		public string AOI => Lang.AOI;

        /// <summary>
        ///   查找类似 Api版本 的本地化字符串。
        /// </summary>
		public string ApiVersion => Lang.ApiVersion;

        /// <summary>
        ///   查找类似 应用 的本地化字符串。
        /// </summary>
		public string Apply => Lang.Apply;

        /// <summary>
        ///   查找类似 线线角度 的本地化字符串。
        /// </summary>
		public string ArcLine2Line => Lang.ArcLine2Line;

        /// <summary>
        ///   查找类似 线面角度 的本地化字符串。
        /// </summary>
		public string ArcLine2Plane => Lang.ArcLine2Plane;

        /// <summary>
        ///   查找类似 面面角度 的本地化字符串。
        /// </summary>
		public string ArcPlane2Plane => Lang.ArcPlane2Plane;

        /// <summary>
        ///   查找类似 面阵 的本地化字符串。
        /// </summary>
		public string AreaCamera => Lang.AreaCamera;

        /// <summary>
        ///   查找类似 文件整理 的本地化字符串。
        /// </summary>
		public string ArrangeDocument => Lang.ArrangeDocument;

        /// <summary>
        ///   查找类似 未找到程序集 的本地化字符串。
        /// </summary>
		public string AssemblyNotFound => Lang.AssemblyNotFound;

        /// <summary>
        ///   查找类似 未找到程序集 的本地化字符串。
        /// </summary>
		public string AssemblyNotFound1 => Lang.AssemblyNotFound1;

        /// <summary>
        ///   查找类似 异步组 的本地化字符串。
        /// </summary>
		public string AsyncGroup => Lang.AsyncGroup;

        /// <summary>
        ///   查找类似 通讯端口自动配置 的本地化字符串。
        /// </summary>
		public string AutoCommunicationConfig => Lang.AutoCommunicationConfig;

        /// <summary>
        ///   查找类似 自动视野 的本地化字符串。
        /// </summary>
		public string AutoFieldOfView => Lang.AutoFieldOfView;

        /// <summary>
        ///   查找类似 自动对焦 的本地化字符串。
        /// </summary>
		public string AutoFocusing => Lang.AutoFocusing;

        /// <summary>
        ///   查找类似 自动灰度 的本地化字符串。
        /// </summary>
		public string AutoGrayScale => Lang.AutoGrayScale;

        /// <summary>
        ///   查找类似 自动压印 的本地化字符串。
        /// </summary>
		public string AutomaticEmbossing => Lang.AutomaticEmbossing;

        /// <summary>
        ///   查找类似 自动LoadCell 的本地化字符串。
        /// </summary>
		public string AutomaticLoadCell => Lang.AutomaticLoadCell;

        /// <summary>
        ///   查找类似 自动定位与水平 的本地化字符串。
        /// </summary>
		public string AutomaticPosAndLeveling => Lang.AutomaticPosAndLeveling;

        /// <summary>
        ///   查找类似 自动运行 的本地化字符串。
        /// </summary>
		public string AutoRun => Lang.AutoRun;

        /// <summary>
        ///   查找类似 Auto Verification 的本地化字符串。
        /// </summary>
		public string AutoVerication => Lang.AutoVerication;

        /// <summary>
        ///   查找类似 手眼标定 的本地化字符串。
        /// </summary>
		public string AutoVisualCalibration => Lang.AutoVisualCalibration;

        /// <summary>
        ///   查找类似 平均耗时 的本地化字符串。
        /// </summary>
		public string AverageTime => Lang.AverageTime;

        /// <summary>
        ///   查找类似 AvgNoOfCodeSewwp 的本地化字符串。
        /// </summary>
		public string AvgNoOfCodeSewwp => Lang.AvgNoOfCodeSewwp;

        /// <summary>
        ///   查找类似 轴 的本地化字符串。
        /// </summary>
		public string Axis => Lang.Axis;

        /// <summary>
        ///   查找类似 多轴龙门 的本地化字符串。
        /// </summary>
		public string AxisArm => Lang.AxisArm;

        /// <summary>
        ///   查找类似 轴调试 的本地化字符串。
        /// </summary>
		public string AxisDebug => Lang.AxisDebug;

        /// <summary>
        ///   查找类似 轴位置 的本地化字符串。
        /// </summary>
		public string AxisPos => Lang.AxisPos;

        /// <summary>
        ///   查找类似 点位运动 的本地化字符串。
        /// </summary>
		public string AxisPosMove => Lang.AxisPosMove;

        /// <summary>
        ///   查找类似 点位运动 的本地化字符串。
        /// </summary>
		public string AxisPosArray => Lang.AxisPosArray;

        /// <summary>
        ///   查找类似 大寰音圈电机 的本地化字符串。
        /// </summary>
		public string DHRoboticsVCM => Lang.DHRoboticsVCM;

        /// <summary>
        ///   查找类似 钧舵音圈电机 的本地化字符串。
        /// </summary>
		public string JunRudderVCM => Lang.JunRudderVCM;

        /// <summary>
        ///   查找类似 轴优先级 的本地化字符串。
        /// </summary>
		public string AxisPriority => Lang.AxisPriority;

        /// <summary>
        ///   查找类似 回流线治具数量 的本地化字符串。
        /// </summary>
		public string BackCarrierNum => Lang.BackCarrierNum;

        /// <summary>
        ///   查找类似 后台工站 的本地化字符串。
        /// </summary>
		public string BackgroundStation => Lang.BackgroundStation;

        /// <summary>
        ///   查找类似 备份 的本地化字符串。
        /// </summary>
		public string BackUp => Lang.BackUp;

        /// <summary>
        ///   查找类似 备份设置 的本地化字符串。
        /// </summary>
		public string BackUpSet => Lang.BackUpSet;

        /// <summary>
        ///   查找类似 Bali版本 的本地化字符串。
        /// </summary>
		public string BaliVersion => Lang.BaliVersion;

        /// <summary>
        ///   查找类似 批量导入点 的本地化字符串。
        /// </summary>
		public string BatchImportPoints => Lang.BatchImportPoints;

        /// <summary>
        ///   查找类似 皮带搬运 的本地化字符串。
        /// </summary>
		public string BeltCarry => Lang.BeltCarry;

        /// <summary>
        ///   查找类似 最佳拟合 的本地化字符串。
        /// </summary>
		public string BestFit => Lang.BestFit;

        /// <summary>
        ///   查找类似 二值化 的本地化字符串。
        /// </summary>
		public string Binarization => Lang.Binarization;

        /// <summary>
        ///   查找类似 阻塞 的本地化字符串。
        /// </summary>
		public string Block => Lang.Block;

        /// <summary>
        ///   查找类似 包围盒 的本地化字符串。
        /// </summary>
		public string BoundBox => Lang.BoundBox;

        /// <summary>
        ///   查找类似 分支 的本地化字符串。
        /// </summary>
		public string Branch => Lang.Branch;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string Branch_Does_Not_Exist => Lang.Branch_Does_Not_Exist;

        /// <summary>
        ///   查找类似 判断分支 的本地化字符串。
        /// </summary>
		public string BranchGroup => Lang.BranchGroup;

        /// <summary>
        ///   查找类似 浏览 的本地化字符串。
        /// </summary>
		public string Broswer => Lang.Broswer;

        /// <summary>
        ///   查找类似 业务 的本地化字符串。
        /// </summary>
		public string Business => Lang.Business;

        /// <summary>
        ///   查找类似 BUSOP 的本地化字符串。
        /// </summary>
		public string BUSOP => Lang.BUSOP;

        /// <summary>
        ///   查找类似 按钮 的本地化字符串。
        /// </summary>
		public string Button => Lang.Button;

        /// <summary>
        ///   查找类似 按钮控制 的本地化字符串。
        /// </summary>
		public string ButtonControl => Lang.ButtonControl;

        /// <summary>
        ///   查找类似 蜂鸣器 的本地化字符串。
        /// </summary>
		public string Buzzer => Lang.Buzzer;

        /// <summary>
        ///   查找类似 缓存数据 的本地化字符串。
        /// </summary>
		public string CacheData => Lang.CacheData;

        /// <summary>
        ///   查找类似 CAD模型 的本地化字符串。
        /// </summary>
		public string CADModel => Lang.CADModel;

        /// <summary>
        ///   查找类似 计时器 的本地化字符串。
        /// </summary>
		public string CalcTime => Lang.CalcTime;

        /// <summary>
        ///   查找类似 计算器 的本地化字符串。
        /// </summary>
		public string Calculator => Lang.Calculator;

        /// <summary>
        ///   查找类似 标定 的本地化字符串。
        /// </summary>
		public string Calib => Lang.Calib;

        /// <summary>
        ///   查找类似 轴系相机标定 的本地化字符串。
        /// </summary>
		public string CalibByPosMove => Lang.CalibByPosMove;

        /// <summary>
        ///   查找类似 压力线性 的本地化字符串。
        /// </summary>
		public string CalibrationTable => Lang.CalibrationTable;

        /// <summary>
        ///   查找类似 相机 的本地化字符串。
        /// </summary>
		public string Camera => Lang.Camera;

        /// <summary>
        ///   查找类似 相机IO 的本地化字符串。
        /// </summary>
		public string CameraIO => Lang.CameraIO;

        /// <summary>
        ///   查找类似 取消 的本地化字符串。
        /// </summary>
		public string Cancel => Lang.Cancel;

        /// <summary>
        ///   查找类似 取消模板 的本地化字符串。
        /// </summary>
		public string CancelCoordTemplate => Lang.CancelCoordTemplate;

        /// <summary>
        ///   查找类似 取消忽略 的本地化字符串。
        /// </summary>
		public string CancelSkip => Lang.CancelSkip;

        /// <summary>
        ///   查找类似 该工程下配方已激活，无法删除该工程 的本地化字符串。
        /// </summary>
		public string CannotDeleteProjWhithRecipeActive => Lang.CannotDeleteProjWhithRecipeActive;

        /// <summary>
        ///   查找类似 系统自带用户，无法修改功能权限配置 的本地化字符串。
        /// </summary>
		public string CannotPermissionToModifyFunction => Lang.CannotPermissionToModifyFunction;

        /// <summary>
        ///   查找类似 产能清零 的本地化字符串。
        /// </summary>
		public string CapacityReset => Lang.CapacityReset;

        /// <summary>
        ///   查找类似 产能明细统计 的本地化字符串。
        /// </summary>
		public string CapacityStatistics => Lang.CapacityStatistics;

        /// <summary>
        ///   查找类似 治具黑名单 的本地化字符串。
        /// </summary>
		public string CarrierBlackList => Lang.CarrierBlackList;

        /// <summary>
        ///   查找类似 载具数量 的本地化字符串。
        /// </summary>
		public string CarrierCount => Lang.CarrierCount;

        /// <summary>
        ///   查找类似 机种 的本地化字符串。
        /// </summary>
		public string category_key => Lang.category_key;

        /// <summary>
        ///   查找类似 2D图像采集 的本地化字符串。
        /// </summary>
		public string CCDImage => Lang.CCDImage;

        /// <summary>
        ///   查找类似 CgAoi 的本地化字符串。
        /// </summary>
		public string CgAoi => Lang.CgAoi;

        /// <summary>
        ///   查找类似 变更记录 的本地化字符串。
        /// </summary>
		public string ChangeRecord => Lang.ChangeRecord;

        /// <summary>
        ///   查找类似 变更类型 的本地化字符串。
        /// </summary>
		public string ChangeType => Lang.ChangeType;

        /// <summary>
        ///   查找类似 曲线列表 的本地化字符串。
        /// </summary>
		public string ChartList => Lang.ChartList;

        /// <summary>
        ///   查找类似 检查更新 的本地化字符串。
        /// </summary>
		public string CheckForUpdates => Lang.CheckForUpdates;

        /// <summary>
        ///   查找类似 检查信号 的本地化字符串。
        /// </summary>
		public string CheckIO => Lang.CheckIO;

        /// <summary>
        ///   查找类似 等待变量 的本地化字符串。
        /// </summary>
		public string CheckVariable => Lang.CheckVariable;

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public string Choose => Lang.Choose;

        /// <summary>
        ///   查找类似 选择提示 的本地化字符串。
        /// </summary>
		public string ChooseTips => Lang.ChooseTips;

        /// <summary>
        ///   查找类似 圆 的本地化字符串。
        /// </summary>
		public string Circle => Lang.Circle;

        /// <summary>
        ///   查找类似 班别 的本地化字符串。
        /// </summary>
		public string Class => Lang.Class;

        /// <summary>
        ///   查找类似 清空 的本地化字符串。
        /// </summary>
		public string Clear => Lang.Clear;

        /// <summary>
        ///   查找类似 清错 的本地化字符串。
        /// </summary>
		public string ClearMistake => Lang.ClearMistake;

        /// <summary>
        ///   查找类似 关闭 的本地化字符串。
        /// </summary>
		public string Close => Lang.Close;

        /// <summary>
        ///   查找类似 关闭所有 的本地化字符串。
        /// </summary>
		public string CloseAll => Lang.CloseAll;

        /// <summary>
        ///   查找类似 关闭其他 的本地化字符串。
        /// </summary>
		public string CloseOther => Lang.CloseOther;

        /// <summary>
        ///   查找类似 点云拼接 的本地化字符串。
        /// </summary>
		public string CloudCalib => Lang.CloudCalib;

        /// <summary>
        ///   查找类似 点云去噪 的本地化字符串。
        /// </summary>
		public string CloudDenoising => Lang.CloudDenoising;

        /// <summary>
        ///   查找类似 点云采样 的本地化字符串。
        /// </summary>
		public string CloudDownSampling => Lang.CloudDownSampling;

        /// <summary>
        ///   查找类似 网格比较 的本地化字符串。
        /// </summary>
		public string CloudMesh => Lang.CloudMesh;

        /// <summary>
        ///   查找类似 点云处理 的本地化字符串。
        /// </summary>
		public string CloudProcess => Lang.CloudProcess;

        /// <summary>
        ///   查找类似 点云配准 的本地化字符串。
        /// </summary>
		public string CloudRegistration => Lang.CloudRegistration;

        /// <summary>
        ///   查找类似 点云去重 的本地化字符串。
        /// </summary>
		public string CloudReRepeat => Lang.CloudReRepeat;

        /// <summary>
        ///   查找类似 点云裁剪 的本地化字符串。
        /// </summary>
		public string CloudSegment => Lang.CloudSegment;

        /// <summary>
        ///   查找类似 点云平滑 的本地化字符串。
        /// </summary>
		public string CloudSmooth => Lang.CloudSmooth;

        /// <summary>
        ///   查找类似 点云变换 的本地化字符串。
        /// </summary>
		public string CloudTransform => Lang.CloudTransform;

        /// <summary>
        ///   查找类似 驾驶舱 的本地化字符串。
        /// </summary>
		public string Cockpit => Lang.Cockpit;

        /// <summary>
        ///   查找类似 驾驶舱IP 的本地化字符串。
        /// </summary>
		public string CockpitIP => Lang.CockpitIP;

        /// <summary>
        ///   查找类似 驾驶舱端口 的本地化字符串。
        /// </summary>
		public string CockpitPort => Lang.CockpitPort;

        /// <summary>
        ///   查找类似 颜色 的本地化字符串。
        /// </summary>
		public string Color => Lang.Color;

        /// <summary>
        ///   查找类似 逗号 的本地化字符串。
        /// </summary>
		public string Comma => Lang.Comma;

        /// <summary>
        ///   查找类似 通用报警 的本地化字符串。
        /// </summary>
		public string CommonAlarm => Lang.CommonAlarm;

        /// <summary>
        ///   查找类似 通信 的本地化字符串。
        /// </summary>
		public string Communication => Lang.Communication;

        /// <summary>
        ///   查找类似 通讯端口配置 的本地化字符串。
        /// </summary>
		public string Communications => Lang.Communications;

        /// <summary>
        ///   查找类似 通信状态 的本地化字符串。
        /// </summary>
		public string CommunicationStatus => Lang.CommunicationStatus;

        /// <summary>
        ///   查找类似 通讯连接测试 的本地化字符串。
        /// </summary>
		public string CommunicationTest => Lang.CommunicationTest;

        /// <summary>
        ///   查找类似 比较 的本地化字符串。
        /// </summary>
		public string Compare => Lang.Compare;

        /// <summary>
        ///   查找类似 对比查看 的本地化字符串。
        /// </summary>
		public string CompareLook => Lang.CompareLook;

        /// <summary>
        ///   查找类似 竞争条件 的本地化字符串。
        /// </summary>
		public string CompeteCondition => Lang.CompeteCondition;

        /// <summary>
        ///   查找类似 排版 的本地化字符串。
        /// </summary>
		public string Composing => Lang.Composing;

        /// <summary>
        ///   查找类似 条件定时器 的本地化字符串。
        /// </summary>
		public string ConditionTimer => Lang.ConditionTimer;

        /// <summary>
        ///   查找类似 圆锥 的本地化字符串。
        /// </summary>
		public string Cone => Lang.Cone;

        /// <summary>
        ///   查找类似 配置电脑网络 的本地化字符串。
        /// </summary>
		public string ConfigComputerNet => Lang.ConfigComputerNet;

        /// <summary>
        ///   查找类似 配置软件串口 的本地化字符串。
        /// </summary>
		public string ConfigSoftwareCom => Lang.ConfigSoftwareCom;

        /// <summary>
        ///   查找类似 配置软件网络 的本地化字符串。
        /// </summary>
		public string ConfigSoftwareNet => Lang.ConfigSoftwareNet;

        /// <summary>
        ///   查找类似 配置 的本地化字符串。
        /// </summary>
		public string Configuration => Lang.Configuration;

        /// <summary>
        ///   查找类似 配置 的本地化字符串。
        /// </summary>
		public string Configure => Lang.Configure;

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public string Confirm => Lang.Confirm;

        /// <summary>
        ///   查找类似 按钮确认 的本地化字符串。
        /// </summary>
		public string ConfirmButton => Lang.ConfirmButton;

        /// <summary>
        ///   查找类似 确认删除 的本地化字符串。
        /// </summary>
		public string ConfirmDelete => Lang.ConfirmDelete;

        /// <summary>
        ///   查找类似 确认删除模块 的本地化字符串。
        /// </summary>
		public string ConfirmDeleteModule => Lang.ConfirmDeleteModule;

        /// <summary>
        ///   查找类似 确认删除用户 的本地化字符串。
        /// </summary>
		public string ConfirmDeleteUser => Lang.ConfirmDeleteUser;

        /// <summary>
        ///   查找类似 确认删除变量 的本地化字符串。
        /// </summary>
		public string ConfirmDeleteVar => Lang.ConfirmDeleteVar;

        /// <summary>
        ///   查找类似 确认将模块 的本地化字符串。
        /// </summary>
		public string ConfirmThatTheModule => Lang.ConfirmThatTheModule;

        /// <summary>
        ///   查找类似 内容 的本地化字符串。
        /// </summary>
		public string Content => Lang.Content;

        /// <summary>
        ///   查找类似 继续 的本地化字符串。
        /// </summary>
		public string Continue => Lang.Continue;

        /// <summary>
        ///   查找类似 坐标系 的本地化字符串。
        /// </summary>
		public string Coord => Lang.Coord;

        /// <summary>
        ///   查找类似 参考坐标系 的本地化字符串。
        /// </summary>
		public string CoordRef => Lang.CoordRef;

        /// <summary>
        ///   查找类似 拷贝 的本地化字符串。
        /// </summary>
		public string Copy => Lang.Copy;

        /// <summary>
        ///   查找类似 复制创建 的本地化字符串。
        /// </summary>
		public string CopyCreate => Lang.CopyCreate;

        /// <summary>
        ///   查找类似 复制需要选中模块[根节点不支持复制] 的本地化字符串。
        /// </summary>
		public string CopySelectedModule => Lang.CopySelectedModule;

        /// <summary>
        ///   查找类似 CPK检测 的本地化字符串。
        /// </summary>
		public string CPKTest => Lang.CPKTest;

        /// <summary>
        ///   查找类似 新建 的本地化字符串。
        /// </summary>
		public string Create => Lang.Create;

        /// <summary>
        ///   查找类似 创建项目 的本地化字符串。
        /// </summary>
		public string CreateProject => Lang.CreateProject;

        /// <summary>
        ///   查找类似 创建时间 的本地化字符串。
        /// </summary>
		public string CreateTime => Lang.CreateTime;

        /// <summary>
        ///   查找类似 稼动率 的本地化字符串。
        /// </summary>
		public string CropRate => Lang.CropRate;

        /// <summary>
        ///   查找类似 稼动率设置 的本地化字符串。
        /// </summary>
		public string CropRateSet => Lang.CropRateSet;

        /// <summary>
        ///   查找类似 单片耗时 的本地化字符串。
        /// </summary>
		public string CT => Lang.CT;

        /// <summary>
        ///   查找类似 CT统计 的本地化字符串。
        /// </summary>
		public string CTStatistics => Lang.CTStatistics;

        /// <summary>
        ///   查找类似 长方体 的本地化字符串。
        /// </summary>
		public string Cuboid => Lang.Cuboid;

        /// <summary>
        ///   查找类似 最 近 项 目 的本地化字符串。
        /// </summary>
		public string CurrentProject => Lang.CurrentProject;

        /// <summary>
        ///   查找类似 当前值 的本地化字符串。
        /// </summary>
		public string CurrentValue => Lang.CurrentValue;

        /// <summary>
        ///   查找类似 当前值2 的本地化字符串。
        /// </summary>
		public string CurrentValue2 => Lang.CurrentValue2;

        /// <summary>
        ///   查找类似 自定义 的本地化字符串。
        /// </summary>
		public string Custom => Lang.Custom;

        /// <summary>
        ///   查找类似 自定义模块 的本地化字符串。
        /// </summary>
		public string CustomModule => Lang.CustomModule;

        /// <summary>
        ///   查找类似 气缸 的本地化字符串。
        /// </summary>
		public string Cylinder => Lang.Cylinder;

        /// <summary>
        ///   查找类似 数据库 的本地化字符串。
        /// </summary>
		public string DataBase => Lang.DataBase;

        /// <summary>
        ///   查找类似 数据目录 的本地化字符串。
        /// </summary>
		public string DataDirectory => Lang.DataDirectory;

        /// <summary>
        ///   查找类似 数据标识 的本地化字符串。
        /// </summary>
		public string DataMark => Lang.DataMark;

        /// <summary>
        ///   查找类似 数据处理 的本地化字符串。
        /// </summary>
		public string DataProc => Lang.DataProc;

        /// <summary>
        ///   查找类似 数据处理 的本地化字符串。
        /// </summary>
		public string DataProcess => Lang.DataProcess;

        /// <summary>
        ///   查找类似 数据转移 的本地化字符串。
        /// </summary>
		public string DataTransfer => Lang.DataTransfer;

        /// <summary>
        ///   查找类似 数据类型 的本地化字符串。
        /// </summary>
		public string DataType => Lang.DataType;

        /// <summary>
        ///   查找类似 数据验证 的本地化字符串。
        /// </summary>
		public string DataValidation => Lang.DataValidation;

        /// <summary>
        ///   查找类似 天 的本地化字符串。
        /// </summary>
		public string Day => Lang.Day;

        /// <summary>
        ///   查找类似 调试 的本地化字符串。
        /// </summary>
		public string Debug => Lang.Debug;

        /// <summary>
        ///   查找类似 调试功能 的本地化字符串。
        /// </summary>
		public string DebugFunction => Lang.DebugFunction;

        /// <summary>
        ///   查找类似 减速度 的本地化字符串。
        /// </summary>
		public string Dec => Lang.Dec;

        /// <summary>
        ///   查找类似 默认 的本地化字符串。
        /// </summary>
		public string Default => Lang.Default;

        /// <summary>
        ///   查找类似 默认路径 的本地化字符串。
        /// </summary>
		public string DefaultPath => Lang.DefaultPath;

        /// <summary>
        ///   查找类似 默认值 的本地化字符串。
        /// </summary>
		public string DefaultValue => Lang.DefaultValue;

        /// <summary>
        ///   查找类似 延时 的本地化字符串。
        /// </summary>
		public string Delay => Lang.Delay;

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public string Delete => Lang.Delete;

        /// <summary>
        ///   查找类似 确认删除自定义模块 的本地化字符串。
        /// </summary>
		public string DeleteCustomModule => Lang.DeleteCustomModule;

        /// <summary>
        ///   查找类似 移除工程 的本地化字符串。
        /// </summary>
		public string DeleteProject => Lang.DeleteProject;

        /// <summary>
        ///   查找类似 去噪 的本地化字符串。
        /// </summary>
		public string Denoising => Lang.Denoising;

        /// <summary>
        ///   查找类似 抛料率 的本地化字符串。
        /// </summary>
		public string DepositRate => Lang.DepositRate;

        /// <summary>
        ///   查找类似 工位 的本地化字符串。
        /// </summary>
		public string DeskName => Lang.DeskName;

        /// <summary>
        ///   查找类似 设备 的本地化字符串。
        /// </summary>
		public string Device => Lang.Device;

        /// <summary>
        ///   查找类似 设备厂商 的本地化字符串。
        /// </summary>
		public string DeviceFirm => Lang.DeviceFirm;

        /// <summary>
        ///   查找类似 设备信息 的本地化字符串。
        /// </summary>
		public string DeviceInfo => Lang.DeviceInfo;

        /// <summary>
        ///   查找类似 设备监控 的本地化字符串。
        /// </summary>
		public string DeviceMonitor => Lang.DeviceMonitor;

        /// <summary>
        ///   查找类似 设备名称 的本地化字符串。
        /// </summary>
		public string DeviceName => Lang.DeviceName;

        /// <summary>
        ///   查找类似 设备SN 的本地化字符串。
        /// </summary>
		public string DeviceSN => Lang.DeviceSN;

        /// <summary>
        ///   查找类似 设备状态 的本地化字符串。
        /// </summary>
		public string DeviceState => Lang.DeviceState;

        /// <summary>
        ///   查找类似 设备类型 的本地化字符串。
        /// </summary>
		public string DeviceType => Lang.DeviceType;

        /// <summary>
        ///   查找类似 对话框 的本地化字符串。
        /// </summary>
		public string DialogBox => Lang.DialogBox;

        /// <summary>
        ///   查找类似 数字输入 的本地化字符串。
        /// </summary>
		public string Digital_In => Lang.Digital_In;

        /// <summary>
        ///   查找类似 单个数字输入 的本地化字符串。
        /// </summary>
		public string Digital_In_Single => Lang.Digital_In_Single;

        /// <summary>
        ///   查找类似 数字输出 的本地化字符串。
        /// </summary>
		public string Digital_Out => Lang.Digital_Out;

        /// <summary>
        ///   查找类似 单个数字输出 的本地化字符串。
        /// </summary>
		public string Digital_Out_Single => Lang.Digital_Out_Single;

        /// <summary>
        ///   查找类似 数字架线 的本地化字符串。
        /// </summary>
		public string DigitalAss => Lang.DigitalAss;

        /// <summary>
        ///   查找类似 视觉调试 的本地化字符串。
        /// </summary>
		public string DigitalVision => Lang.DigitalVision;

        /// <summary>
        ///   查找类似 线线距离 的本地化字符串。
        /// </summary>
		public string DisLine2Line => Lang.DisLine2Line;

        /// <summary>
        ///   查找类似 线面距离 的本地化字符串。
        /// </summary>
		public string DisLine2Plane => Lang.DisLine2Plane;

        /// <summary>
        ///   查找类似 距离测量 的本地化字符串。
        /// </summary>
		public string DisMeasure => Lang.DisMeasure;

        /// <summary>
        ///   查找类似 显示 的本地化字符串。
        /// </summary>
		public string Display => Lang.Display;

        /// <summary>
        ///   查找类似 显示方向 的本地化字符串。
        /// </summary>
		public string DisplayDirection => Lang.DisplayDirection;

        /// <summary>
        ///   查找类似 显示元素参数设置 的本地化字符串。
        /// </summary>
		public string DisplayElementParaSetting => Lang.DisplayElementParaSetting;

        /// <summary>
        ///   查找类似 显示类型设置 的本地化字符串。
        /// </summary>
		public string DisplayTypeSetting => Lang.DisplayTypeSetting;

        /// <summary>
        ///   查找类似 点线距离 的本地化字符串。
        /// </summary>
		public string DisPoint2Line => Lang.DisPoint2Line;

        /// <summary>
        ///   查找类似 点面距离 的本地化字符串。
        /// </summary>
		public string DisPoint2Plane => Lang.DisPoint2Plane;

        /// <summary>
        ///   查找类似 两点距离 的本地化字符串。
        /// </summary>
		public string DisPoint2Point => Lang.DisPoint2Point;

        /// <summary>
        ///   查找类似 完成 的本地化字符串。
        /// </summary>
		public string Done => Lang.Done;

        /// <summary>
        ///   查找类似 门锁 的本地化字符串。
        /// </summary>
		public string DoorLock => Lang.DoorLock;

        /// <summary>
        ///   查找类似 双页 的本地化字符串。
        /// </summary>
		public string DoublePage => Lang.DoublePage;

        /// <summary>
        ///   查找类似 下采样 的本地化字符串。
        /// </summary>
		public string DownSampling => Lang.DownSampling;

        /// <summary>
        ///   查找类似 宕机原因 的本地化字符串。
        /// </summary>
		public string DownTimeReason => Lang.DownTimeReason;

        /// <summary>
        ///   查找类似 绘制长方体ROI 的本地化字符串。
        /// </summary>
		public string DrawCuboidROI => Lang.DrawCuboidROI;

        /// <summary>
        ///   查找类似 绘制圆柱ROI 的本地化字符串。
        /// </summary>
		public string DrawCylinderROI => Lang.DrawCylinderROI;

        /// <summary>
        ///   查找类似 绘制球体ROI 的本地化字符串。
        /// </summary>
		public string DrawSphereROI => Lang.DrawSphereROI;

        /// <summary>
        ///   查找类似 转储路径 的本地化字符串。
        /// </summary>
		public string DumpPath => Lang.DumpPath;

        /// <summary>
        ///   查找类似 重复密码不能为空 的本地化字符串。
        /// </summary>
		public string DuplicatePwdCannotBeEmpty => Lang.DuplicatePwdCannotBeEmpty;

        /// <summary>
        ///   查找类似 编辑 的本地化字符串。
        /// </summary>
		public string Edit => Lang.Edit;

        /// <summary>
        ///   查找类似 编辑班别 的本地化字符串。
        /// </summary>
		public string EditClass => Lang.EditClass;

        /// <summary>
        ///   查找类似 编辑特征 的本地化字符串。
        /// </summary>
		public string EditFeature => Lang.EditFeature;

        /// <summary>
        ///   查找类似 编辑Plc报警 的本地化字符串。
        /// </summary>
		public string EditPlcAlarm => Lang.EditPlcAlarm;

        /// <summary>
        ///   查找类似 编辑用户 的本地化字符串。
        /// </summary>
		public string EditUser => Lang.EditUser;

        /// <summary>
        ///   查找类似 电缸 的本地化字符串。
        /// </summary>
		public string EleCylinder => Lang.EleCylinder;

        /// <summary>
        ///   查找类似 自动压印 的本地化字符串。
        /// </summary>
		public string Embossing => Lang.Embossing;

        /// <summary>
        ///   查找类似 急停 的本地化字符串。
        /// </summary>
		public string EmergencyStop => Lang.EmergencyStop;

        /// <summary>
        ///   查找类似 空跑 的本地化字符串。
        /// </summary>
		public string EmptyRun => Lang.EmptyRun;

        /// <summary>
        ///   查找类似 空跑模式 的本地化字符串。
        /// </summary>
		public string EmptyRunMode => Lang.EmptyRunMode;

        /// <summary>
        ///   查找类似 是否启用 的本地化字符串。
        /// </summary>
		public string Enable => Lang.Enable;

        /// <summary>
        ///   查找类似 启动蜂鸣器 的本地化字符串。
        /// </summary>
		public string EnableBuzzer => Lang.EnableBuzzer;

        /// <summary>
        ///   查找类似 光栅启用 的本地化字符串。
        /// </summary>
		public string EnableLightCurtain => Lang.EnableLightCurtain;

        /// <summary>
        ///   查找类似 启动监听 的本地化字符串。
        /// </summary>
		public string EnableListening => Lang.EnableListening;

        /// <summary>
        ///   查找类似 安全门启用 的本地化字符串。
        /// </summary>
		public string EnableSafetyDoor => Lang.EnableSafetyDoor;

        /// <summary>
        ///   查找类似 结束 的本地化字符串。
        /// </summary>
		public string End => Lang.End;

        /// <summary>
        ///   查找类似 结束模块 的本地化字符串。
        /// </summary>
		public string EndModule => Lang.EndModule;

        /// <summary>
        ///   查找类似 成品 的本地化字符串。
        /// </summary>
		public string EndProduct => Lang.EndProduct;

        /// <summary>
        ///   查找类似 结束时间 的本地化字符串。
        /// </summary>
		public string EndTime => Lang.EndTime;

        /// <summary>
        ///   查找类似 请输入站点名称 的本地化字符串。
        /// </summary>
		public string EnterStationName => Lang.EnterStationName;

        /// <summary>
        ///   查找类似 爱普生机器人 的本地化字符串。
        /// </summary>
		public string EpsonRobot => Lang.EpsonRobot;

        /// <summary>
        ///   查找类似 错误 的本地化字符串。
        /// </summary>
		public string Error => Lang.Error;

        /// <summary>
        ///   查找类似 报错英文描述 的本地化字符串。
        /// </summary>
		public string ErrorForeignMessage => Lang.ErrorForeignMessage;

        /// <summary>
        ///   查找类似 错误的图片路径 的本地化字符串。
        /// </summary>
		public string ErrorImgPath => Lang.ErrorImgPath;

        /// <summary>
        ///   查找类似 非法的图片尺寸 的本地化字符串。
        /// </summary>
		public string ErrorImgSize => Lang.ErrorImgSize;

        /// <summary>
        ///   查找类似 退出 的本地化字符串。
        /// </summary>
		public string Exit => Lang.Exit;

        /// <summary>
        ///   查找类似 导出 的本地化字符串。
        /// </summary>
		public string Export => Lang.Export;

        /// <summary>
        ///   查找类似 导出数据 的本地化字符串。
        /// </summary>
		public string ExportData => Lang.ExportData;

        /// <summary>
        ///   查找类似 导出流程图 的本地化字符串。
        /// </summary>
		public string ExportFlowTree => Lang.ExportFlowTree;

        /// <summary>
        ///   查找类似 导出图片 的本地化字符串。
        /// </summary>
		public string ExportImage => Lang.ExportImage;

        /// <summary>
        ///   查找类似 导出工程 的本地化字符串。
        /// </summary>
		public string ExportProject => Lang.ExportProject;

        /// <summary>
        ///   查找类似 导出配方 的本地化字符串。
        /// </summary>
		public string ExportRecipe => Lang.ExportRecipe;

        /// <summary>
        ///   查找类似 Extract 的本地化字符串。
        /// </summary>
		public string Extract => Lang.Extract;

        /// <summary>
        ///   查找类似 提取异步组 的本地化字符串。
        /// </summary>
		public string ExtractAsyncGroup => Lang.ExtractAsyncGroup;

        /// <summary>
        ///   查找类似 提取分支组 的本地化字符串。
        /// </summary>
		public string ExtractBranchGroup => Lang.ExtractBranchGroup;

        /// <summary>
        ///   查找类似 提取模块 的本地化字符串。
        /// </summary>
		public string ExtractModule => Lang.ExtractModule;

        /// <summary>
        ///   查找类似 提取NG组 的本地化字符串。
        /// </summary>
		public string ExtractNGGroup => Lang.ExtractNGGroup;

        /// <summary>
        ///   查找类似 提取分组 的本地化字符串。
        /// </summary>
		public string ExtractStepGroup => Lang.ExtractStepGroup;

        /// <summary>
        ///   查找类似 提取分支组 的本地化字符串。
        /// </summary>
		public string ExtractSwitchGroup => Lang.ExtractSwitchGroup;

        /// <summary>
        ///   查找类似 飞达 的本地化字符串。
        /// </summary>
		public string Feeder => Lang.Feeder;

        /// <summary>
        ///   查找类似 供料站 的本地化字符串。
        /// </summary>
		public string FeedStation => Lang.FeedStation;

        /// <summary>
        ///   查找类似 FFU 的本地化字符串。
        /// </summary>
		public string FFU => Lang.FFU;

        /// <summary>
        ///   查找类似 FFU速度等级 的本地化字符串。
        /// </summary>
		public string FFUSpeedLevel => Lang.FFUSpeedLevel;

        /// <summary>
        ///   查找类似 文件 的本地化字符串。
        /// </summary>
		public string File => Lang.File;

        /// <summary>
        ///   查找类似 文件地址 的本地化字符串。
        /// </summary>
		public string FileAddress => Lang.FileAddress;

        /// <summary>
        ///   查找类似 文件配置 的本地化字符串。
        /// </summary>
		public string FileConfig => Lang.FileConfig;

        /// <summary>
        ///   查找类似 文件输入 的本地化字符串。
        /// </summary>
		public string FileIO => Lang.FileIO;

        /// <summary>
        ///   查找类似 文件类型 的本地化字符串。
        /// </summary>
		public string FileType => Lang.FileType;

        /// <summary>
        ///   查找类似 滤波 的本地化字符串。
        /// </summary>
		public string Filtering => Lang.Filtering;

        /// <summary>
        ///   查找类似 最终结果 的本地化字符串。
        /// </summary>
		public string FinalResult => Lang.FinalResult;

        /// <summary>
        ///   查找类似 查找 的本地化字符串。
        /// </summary>
		public string Find => Lang.Find;

        /// <summary>
        ///   查找类似 找圆 的本地化字符串。
        /// </summary>
		public string FindCircle => Lang.FindCircle;

        /// <summary>
        ///   查找类似 完 成 的本地化字符串。
        /// </summary>
		public string Finish => Lang.Finish;

        /// <summary>
        ///   查找类似 首班 的本地化字符串。
        /// </summary>
		public string FirstClass => Lang.FirstClass;

        /// <summary>
        ///   查找类似 首件指令 的本地化字符串。
        /// </summary>
		public string FirstPieceModeCommand => Lang.FirstPieceModeCommand;

        /// <summary>
        ///   查找类似 首件状态 的本地化字符串。
        /// </summary>
		public string FirstPieceModeStatus => Lang.FirstPieceModeStatus;

        /// <summary>
        ///   查找类似 首站 的本地化字符串。
        /// </summary>
		public string FirstStation => Lang.FirstStation;

        /// <summary>
        ///   查找类似 治具号 的本地化字符串。
        /// </summary>
		public string Fixture => Lang.Fixture;

        /// <summary>
        ///   查找类似 平面度 的本地化字符串。
        /// </summary>
		public string Flatness => Lang.Flatness;

        /// <summary>
        ///   查找类似 抛料统计 的本地化字符串。
        /// </summary>
		public string FlingMaterialStatistics => Lang.FlingMaterialStatistics;

        /// <summary>
        ///   查找类似 楼层 的本地化字符串。
        /// </summary>
		public string Floor => Lang.Floor;

        /// <summary>
        ///   查找类似 流程 的本地化字符串。
        /// </summary>
		public string Flow => Lang.Flow;

        /// <summary>
        ///   查找类似 流程等待 的本地化字符串。
        /// </summary>
		public string FlowWait => Lang.FlowWait;

        /// <summary>
        ///   查找类似 飞拍模块 的本地化字符串。
        /// </summary>
		public string FlyingPhoto => Lang.FlyingPhoto;

        /// <summary>
        ///   查找类似 力传感轴 的本地化字符串。
        /// </summary>
		public string ForceAxis => Lang.ForceAxis;

        /// <summary>
        ///   查找类似 压力采集 的本地化字符串。
        /// </summary>
		public string ForceCollect => Lang.ForceCollect;

        /// <summary>
        ///   查找类似 格式错误 的本地化字符串。
        /// </summary>
		public string FormatError => Lang.FormatError;

        /// <summary>
        ///   查找类似 用于内存泄露检测 的本地化字符串。
        /// </summary>
		public string ForMemoryLeakDetection => Lang.ForMemoryLeakDetection;

        /// <summary>
        ///   查找类似 空闲 的本地化字符串。
        /// </summary>
		public string Free => Lang.Free;

        /// <summary>
        ///   查找类似 自由工站 的本地化字符串。
        /// </summary>
		public string FreeStation => Lang.FreeStation;

        /// <summary>
        ///   查找类似 FTP上传 的本地化字符串。
        /// </summary>
		public string FTPUpload => Lang.FTPUpload;

        /// <summary>
        ///   查找类似 功能模块 的本地化字符串。
        /// </summary>
		public string FunctionalModule => Lang.FunctionalModule;

        /// <summary>
        ///   查找类似 功能启用 的本地化字符串。
        /// </summary>
		public string FunctionEnable => Lang.FunctionEnable;

        /// <summary>
        ///   查找类似 功能部门 的本地化字符串。
        /// </summary>
		public string FunctionId => Lang.FunctionId;

        /// <summary>
        ///   查找类似 功能管理 的本地化字符串。
        /// </summary>
		public string FunctionManagement => Lang.FunctionManagement;

        /// <summary>
        ///   查找类似 治具绑定 的本地化字符串。
        /// </summary>
		public string FX_BindCarrier => Lang.FX_BindCarrier;

        /// <summary>
        ///   查找类似 工单查询 的本地化字符串。
        /// </summary>
		public string FX_OrderQuery => Lang.FX_OrderQuery;

        /// <summary>
        ///   查找类似 路由查询 的本地化字符串。
        /// </summary>
		public string FX_RouteQuery => Lang.FX_RouteQuery;

        /// <summary>
        ///   查找类似 治具解绑 的本地化字符串。
        /// </summary>
		public string FX_UnBindCarrier => Lang.FX_UnBindCarrier;

        /// <summary>
        ///   查找类似 NG结果上传 的本地化字符串。
        /// </summary>
		public string FX_UploadResult => Lang.FX_UploadResult;

        /// <summary>
        ///   查找类似 FX首页 的本地化字符串。
        /// </summary>
		public string FXContent => Lang.FXContent;

        /// <summary>
        ///   查找类似 数字孪生 的本地化字符串。
        /// </summary>
		public string FXTCP => Lang.FXTCP;

        /// <summary>
        ///   查找类似 Gap 的本地化字符串。
        /// </summary>
		public string Gap => Lang.Gap;

        /// <summary>
        ///   查找类似 齿轮比分子 的本地化字符串。
        /// </summary>
		public string GearRatioNumerator => Lang.GearRatioNumerator;

        /// <summary>
        ///   查找类似 获取轴位置 的本地化字符串。
        /// </summary>
		public string GenAxisPos => Lang.GenAxisPos;

        /// <summary>
        ///   查找类似 数据圆 的本地化字符串。
        /// </summary>
		public string GenCircleByData => Lang.GenCircleByData;

        /// <summary>
        ///   查找类似 拟合圆 的本地化字符串。
        /// </summary>
		public string GenCircleByFit => Lang.GenCircleByFit;

        /// <summary>
        ///   查找类似 多点构建点云 的本地化字符串。
        /// </summary>
		public string GenCloudByPoints => Lang.GenCloudByPoints;

        /// <summary>
        ///   查找类似 圆锥拟合 的本地化字符串。
        /// </summary>
		public string GenConeByFit => Lang.GenConeByFit;

        /// <summary>
        ///   查找类似 坐标系生成 的本地化字符串。
        /// </summary>
		public string GenCoordByData => Lang.GenCoordByData;

        /// <summary>
        ///   查找类似 三面构建 的本地化字符串。
        /// </summary>
		public string GenCoordByPlane3 => Lang.GenCoordByPlane3;

        /// <summary>
        ///   查找类似 数据长方体 的本地化字符串。
        /// </summary>
		public string GenCuboidByData => Lang.GenCuboidByData;

        /// <summary>
        ///   查找类似 数据圆柱 的本地化字符串。
        /// </summary>
		public string GenCylinderByData => Lang.GenCylinderByData;

        /// <summary>
        ///   查找类似 圆柱拟合 的本地化字符串。
        /// </summary>
		public string GenCylinderByFit => Lang.GenCylinderByFit;

        /// <summary>
        ///   查找类似 生成 的本地化字符串。
        /// </summary>
		public string Generate => Lang.Generate;

        /// <summary>
        ///   查找类似 生成方式 的本地化字符串。
        /// </summary>
		public string GenerationMode => Lang.GenerationMode;

        /// <summary>
        ///   查找类似 两点构线 的本地化字符串。
        /// </summary>
		public string GenLineBy2Point => Lang.GenLineBy2Point;

        /// <summary>
        ///   查找类似 求相交线 的本地化字符串。
        /// </summary>
		public string GenLineByCross => Lang.GenLineByCross;

        /// <summary>
        ///   查找类似 数据线 的本地化字符串。
        /// </summary>
		public string GenLineByData => Lang.GenLineByData;

        /// <summary>
        ///   查找类似 拟合直线 的本地化字符串。
        /// </summary>
		public string GenLineByFit => Lang.GenLineByFit;

        /// <summary>
        ///   查找类似 线平移 的本地化字符串。
        /// </summary>
		public string GenLineByMove => Lang.GenLineByMove;

        /// <summary>
        ///   查找类似 求投影线 的本地化字符串。
        /// </summary>
		public string GenLineByProj => Lang.GenLineByProj;

        /// <summary>
        ///   查找类似 线变换 的本地化字符串。
        /// </summary>
		public string GenLineByTransform => Lang.GenLineByTransform;

        /// <summary>
        ///   查找类似 三点构面 的本地化字符串。
        /// </summary>
		public string GenPlaneBy3Point => Lang.GenPlaneBy3Point;

        /// <summary>
        ///   查找类似 拟合面 的本地化字符串。
        /// </summary>
		public string GenPlaneByFit => Lang.GenPlaneByFit;

        /// <summary>
        ///   查找类似 面平移 的本地化字符串。
        /// </summary>
		public string GenPlaneByMove => Lang.GenPlaneByMove;

        /// <summary>
        ///   查找类似 创建面 的本地化字符串。
        /// </summary>
		public string GenPlaneByObj => Lang.GenPlaneByObj;

        /// <summary>
        ///   查找类似 生成点 的本地化字符串。
        /// </summary>
		public string GenPoint => Lang.GenPoint;

        /// <summary>
        ///   查找类似 卡尺取点 的本地化字符串。
        /// </summary>
		public string GenPointByCaliper => Lang.GenPointByCaliper;

        /// <summary>
        ///   查找类似 点云极值点 的本地化字符串。
        /// </summary>
		public string GenPointByCloud => Lang.GenPointByCloud;

        /// <summary>
        ///   查找类似 CMM取点 的本地化字符串。
        /// </summary>
		public string GenPointByCMM => Lang.GenPointByCMM;

        /// <summary>
        ///   查找类似 求交点 的本地化字符串。
        /// </summary>
		public string GenPointByCross => Lang.GenPointByCross;

        /// <summary>
        ///   查找类似 数字点 的本地化字符串。
        /// </summary>
		public string GenPointByData => Lang.GenPointByData;

        /// <summary>
        ///   查找类似 点偏移 的本地化字符串。
        /// </summary>
		public string GenPointByMove => Lang.GenPointByMove;

        /// <summary>
        ///   查找类似 求投影点 的本地化字符串。
        /// </summary>
		public string GenPointByProj => Lang.GenPointByProj;

        /// <summary>
        ///   查找类似 随机数 的本地化字符串。
        /// </summary>
		public string GenRandomNumber => Lang.GenRandomNumber;

        /// <summary>
        ///   查找类似 生成报告 的本地化字符串。
        /// </summary>
		public string GenReport => Lang.GenReport;

        /// <summary>
        ///   查找类似 数据球 的本地化字符串。
        /// </summary>
		public string GenSphereByData => Lang.GenSphereByData;

        /// <summary>
        ///   查找类似 球体拟合 的本地化字符串。
        /// </summary>
		public string GenSphereByFit => Lang.GenSphereByFit;

        /// <summary>
        ///   查找类似 字符创建 的本地化字符串。
        /// </summary>
		public string GenString => Lang.GenString;

        /// <summary>
        ///   查找类似 几何特征 的本地化字符串。
        /// </summary>
		public string GeometricFeatures => Lang.GeometricFeatures;

        /// <summary>
        ///   查找类似 几何 的本地化字符串。
        /// </summary>
		public string Geometry => Lang.Geometry;

        /// <summary>
        ///   查找类似 平均值 的本地化字符串。
        /// </summary>
		public string GetAverage => Lang.GetAverage;

        /// <summary>
        ///   查找类似 数据库获取 的本地化字符串。
        /// </summary>
		public string GetByDataBase => Lang.GetByDataBase;

        /// <summary>
        ///   查找类似 获取方向 的本地化字符串。
        /// </summary>
		public string GetDirectionByObj => Lang.GetDirectionByObj;

        /// <summary>
        ///   查找类似 获取信号 的本地化字符串。
        /// </summary>
		public string GetIO => Lang.GetIO;

        /// <summary>
        ///   查找类似 获取线 的本地化字符串。
        /// </summary>
		public string GetLineByObj => Lang.GetLineByObj;

        /// <summary>
        ///   查找类似 获取机台状态 的本地化字符串。
        /// </summary>
		public string GetMachineStatus => Lang.GetMachineStatus;

        /// <summary>
        ///   查找类似 获取MBus 的本地化字符串。
        /// </summary>
		public string GetModbus => Lang.GetModbus;

        /// <summary>
        ///   查找类似 获取面 的本地化字符串。
        /// </summary>
		public string GetPlaneByObj => Lang.GetPlaneByObj;

        /// <summary>
        ///   查找类似 获取点 的本地化字符串。
        /// </summary>
		public string GetPointByObj => Lang.GetPointByObj;

        /// <summary>
        ///   查找类似 获取线性KBR值 的本地化字符串。
        /// </summary>
		public string GetSlopeIntercept => Lang.GetSlopeIntercept;

        /// <summary>
        ///   查找类似 全局 的本地化字符串。
        /// </summary>
		public string Global => Lang.Global;

        /// <summary>
        ///   查找类似 全局变量 的本地化字符串。
        /// </summary>
		public string GlobalVar => Lang.GlobalVar;

        /// <summary>
        ///   查找类似 全局变量 的本地化字符串。
        /// </summary>
		public string GlobalVariable => Lang.GlobalVariable;

        /// <summary>
        ///   查找类似 跳转 的本地化字符串。
        /// </summary>
		public string GoToModule => Lang.GoToModule;

        /// <summary>
        ///   查找类似 绿灯 的本地化字符串。
        /// </summary>
		public string GreenLamp => Lang.GreenLamp;

        /// <summary>
        ///   查找类似 分组 的本地化字符串。
        /// </summary>
		public string Group => Lang.Group;

        /// <summary>
        ///   查找类似 处理人 的本地化字符串。
        /// </summary>
		public string HandledUser => Lang.HandledUser;

        /// <summary>
        ///   查找类似 处理方式 的本地化字符串。
        /// </summary>
		public string HandlingMethod => Lang.HandlingMethod;

        /// <summary>
        ///   查找类似 硬件 的本地化字符串。
        /// </summary>
		public string HardWare => Lang.HardWare;

        /// <summary>
        ///   查找类似 高度 的本地化字符串。
        /// </summary>
		public string Height => Lang.Height;

        /// <summary>
        ///   查找类似 测高 的本地化字符串。
        /// </summary>
		public string Heightfinder => Lang.Heightfinder;

        /// <summary>
        ///   查找类似 帮助 的本地化字符串。
        /// </summary>
		public string Help => Lang.Help;

        /// <summary>
        ///   查找类似 隐藏标签 的本地化字符串。
        /// </summary>
		public string HideLabel => Lang.HideLabel;

        /// <summary>
        ///   查找类似 高风速模式电流下限 的本地化字符串。
        /// </summary>
		public string HighCurrentLowLimit => Lang.HighCurrentLowLimit;

        /// <summary>
        ///   查找类似 高风速模式电流上限 的本地化字符串。
        /// </summary>
		public string HighCurrentUpperLimit => Lang.HighCurrentUpperLimit;

        /// <summary>
        ///   查找类似 高级权限时间 的本地化字符串。
        /// </summary>
		public string HighLevelTime => Lang.HighLevelTime;

        /// <summary>
        ///   查找类似 HiveAppId 的本地化字符串。
        /// </summary>
		public string HiveAppId => Lang.HiveAppId;

        /// <summary>
        ///   查找类似 Hive配置 的本地化字符串。
        /// </summary>
		public string HiveConfig => Lang.HiveConfig;

        /// <summary>
        ///   查找类似 HiveCT 的本地化字符串。
        /// </summary>
		public string HiveCT => Lang.HiveCT;

        /// <summary>
        ///   查找类似 忽略Hive反馈 的本地化字符串。
        /// </summary>
		public string HiveIgnoreFeedback => Lang.HiveIgnoreFeedback;

        /// <summary>
        ///   查找类似 Hive阀门 的本地化字符串。
        /// </summary>
		public string HiveValve => Lang.HiveValve;

        /// <summary>
        ///   查找类似 Holo3D 的本地化字符串。
        /// </summary>
		public string Holo3D => Lang.Holo3D;

        /// <summary>
        ///   查找类似 主页 的本地化字符串。
        /// </summary>
		public string Home => Lang.Home;

        /// <summary>
        ///   查找类似 回零完成 的本地化字符串。
        /// </summary>
		public string HomeDone => Lang.HomeDone;

        /// <summary>
        ///   查找类似 回零站 的本地化字符串。
        /// </summary>
		public string HomeStation => Lang.HomeStation;

        /// <summary>
        ///   查找类似 回零 的本地化字符串。
        /// </summary>
		public string HomeZero => Lang.HomeZero;

        /// <summary>
        ///   查找类似 平台水平确认 的本地化字符串。
        /// </summary>
		public string Horizontal => Lang.Horizontal;

        /// <summary>
        ///   查找类似 ICW 的本地化字符串。
        /// </summary>
		public string ICW => Lang.ICW;

        /// <summary>
        ///   查找类似 空闲 的本地化字符串。
        /// </summary>
		public string Idle => Lang.Idle;

        /// <summary>
        ///   查找类似 忽略 的本地化字符串。
        /// </summary>
		public string Ignore => Lang.Ignore;

        /// <summary>
        ///   查找类似 图像 的本地化字符串。
        /// </summary>
		public string Image => Lang.Image;

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public string Import => Lang.Import;

        /// <summary>
        ///   查找类似 输入参数别名 的本地化字符串。
        /// </summary>
		public string ImportParameterName => Lang.ImportParameterName;

        /// <summary>
        ///   查找类似 导入配方 的本地化字符串。
        /// </summary>
		public string ImportRecipe => Lang.ImportRecipe;

        /// <summary>
        ///   查找类似 入站时间 的本地化字符串。
        /// </summary>
		public string InboundTime => Lang.InboundTime;

        /// <summary>
        ///   查找类似 序号 的本地化字符串。
        /// </summary>
		public string Index => Lang.Index;

        /// <summary>
        ///   查找类似 信息 的本地化字符串。
        /// </summary>
		public string Info => Lang.Info;

        /// <summary>
        ///   查找类似 忽略 的本地化字符串。
        /// </summary>
		public string Ingore => Lang.Ingore;

        /// <summary>
        ///   查找类似 初始化完成 的本地化字符串。
        /// </summary>
		public string InitComplete => Lang.InitComplete;

        /// <summary>
        ///   查找类似 输入输出参数配置 的本地化字符串。
        /// </summary>
		public string InOutParameterConfigure => Lang.InOutParameterConfigure;

        /// <summary>
        ///   查找类似 输入 的本地化字符串。
        /// </summary>
		public string Input => Lang.Input;

        /// <summary>
        ///   查找类似 输入参数 的本地化字符串。
        /// </summary>
		public string InputParameter => Lang.InputParameter;

        /// <summary>
        ///   查找类似 投入数 的本地化字符串。
        /// </summary>
		public string InputQty => Lang.InputQty;

        /// <summary>
        ///   查找类似 插入 的本地化字符串。
        /// </summary>
		public string Insert => Lang.Insert;

        /// <summary>
        ///   查找类似 插入点 的本地化字符串。
        /// </summary>
		public string InsertPoint => Lang.InsertPoint;

        /// <summary>
        ///   查找类似 Insight名称 的本地化字符串。
        /// </summary>
		public string InsightType => Lang.InsightType;

        /// <summary>
        ///   查找类似 软硬件调试 的本地化字符串。
        /// </summary>
		public string IntegratedHardware => Lang.IntegratedHardware;

        /// <summary>
        ///   查找类似 间隔10分钟 的本地化字符串。
        /// </summary>
		public string Interval10m => Lang.Interval10m;

        /// <summary>
        ///   查找类似 间隔1小时 的本地化字符串。
        /// </summary>
		public string Interval1h => Lang.Interval1h;

        /// <summary>
        ///   查找类似 间隔1分钟 的本地化字符串。
        /// </summary>
		public string Interval1m => Lang.Interval1m;

        /// <summary>
        ///   查找类似 间隔2小时 的本地化字符串。
        /// </summary>
		public string Interval2h => Lang.Interval2h;

        /// <summary>
        ///   查找类似 间隔30分钟 的本地化字符串。
        /// </summary>
		public string Interval30m => Lang.Interval30m;

        /// <summary>
        ///   查找类似 间隔30秒 的本地化字符串。
        /// </summary>
		public string Interval30s => Lang.Interval30s;

        /// <summary>
        ///   查找类似 间隔5分钟 的本地化字符串。
        /// </summary>
		public string Interval5m => Lang.Interval5m;

        /// <summary>
        ///   查找类似 I/O点检 的本地化字符串。
        /// </summary>
		public string IOConform => Lang.IOConform;

        /// <summary>
        ///   查找类似 IO仿真 的本地化字符串。
        /// </summary>
		public string IOSimulation => Lang.IOSimulation;

        /// <summary>
        ///   查找类似 IP地址 的本地化字符串。
        /// </summary>
		public string IPAddress => Lang.IPAddress;

        /// <summary>
        ///   查找类似 是否确认移除当前工程 的本地化字符串。
        /// </summary>
		public string IsDeleteCurrentProject => Lang.IsDeleteCurrentProject;

        /// <summary>
        ///   查找类似 回零参数重置 的本地化字符串。
        /// </summary>
		public string IsHomeDefault => Lang.IsHomeDefault;

        /// <summary>
        ///   查找类似 参数记忆 的本地化字符串。
        /// </summary>
		public string IsMemoric => Lang.IsMemoric;

        /// <summary>
        ///   查找类似 不能为空 的本地化字符串。
        /// </summary>
		public string IsNecessary => Lang.IsNecessary;

        /// <summary>
        ///   查找类似 屏蔽安全门 的本地化字符串。
        /// </summary>
		public string isShieldDoor => Lang.isShieldDoor;

        /// <summary>
        ///   查找类似 是否显示 的本地化字符串。
        /// </summary>
		public string IsVisible => Lang.IsVisible;

        /// <summary>
        ///   查找类似 JOG 的本地化字符串。
        /// </summary>
		public string JOG => Lang.JOG;

        /// <summary>
        ///   查找类似 按位拼接 的本地化字符串。
        /// </summary>
		public string JoinBitToInt => Lang.JoinBitToInt;

        /// <summary>
        ///   查找类似 JSON解析 的本地化字符串。
        /// </summary>
		public string JSONParse => Lang.JSONParse;

        /// <summary>
        ///   查找类似 判断 的本地化字符串。
        /// </summary>
		public string Judge => Lang.Judge;

        /// <summary>
        ///   查找类似 字符判断 的本地化字符串。
        /// </summary>
		public string JudgeString => Lang.JudgeString;

        /// <summary>
        ///   查找类似 跳转 的本地化字符串。
        /// </summary>
		public string Jump => Lang.Jump;

        /// <summary>
        ///   查找类似 关键物料查询 的本地化字符串。
        /// </summary>
		public string KeyMaterialQuery => Lang.KeyMaterialQuery;

        /// <summary>
        ///   查找类似 关键参数 的本地化字符串。
        /// </summary>
		public string KeyParameters => Lang.KeyParameters;

        /// <summary>
        ///   查找类似 关键字匹配 的本地化字符串。
        /// </summary>
		public string KeywordMatching => Lang.KeywordMatching;

        /// <summary>
        ///   查找类似 关键字 ： 的本地化字符串。
        /// </summary>
		public string KeyWordWithSymbol => Lang.KeyWordWithSymbol;

        /// <summary>
        ///   查找类似 LAD Upload 的本地化字符串。
        /// </summary>
		public string LADUpload => Lang.LADUpload;

        /// <summary>
        ///   查找类似 查找类似 {0} 的本地化字符串。 的本地化字符串。
        /// </summary>
		public string LangComment => Lang.LangComment;

        /// <summary>
        ///   查找类似 线激光 的本地化字符串。
        /// </summary>
		public string LaserScan => Lang.LaserScan;

        /// <summary>
        ///   查找类似 激光测距 的本地化字符串。
        /// </summary>
		public string LaserSensor => Lang.LaserSensor;

        /// <summary>
        ///   查找类似 激光版本 的本地化字符串。
        /// </summary>
		public string LaserVersion => Lang.LaserVersion;

        /// <summary>
        ///   查找类似 上次月保养时间 的本地化字符串。
        /// </summary>
		public string LastMonthMaintenance => Lang.LastMonthMaintenance;

        /// <summary>
        ///   查找类似 尾站 的本地化字符串。
        /// </summary>
		public string LastStation => Lang.LastStation;

        /// <summary>
        ///   查找类似 上次周保养时间 的本地化字符串。
        /// </summary>
		public string LastWeekMaintenance => Lang.LastWeekMaintenance;

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public string Lead => Lang.Lead;

        /// <summary>
        ///   查找类似 光源控制器 的本地化字符串。
        /// </summary>
		public string LightController => Lang.LightController;

        /// <summary>
        ///   查找类似 光幕 的本地化字符串。
        /// </summary>
		public string LightCurtain => Lang.LightCurtain;

        /// <summary>
        ///   查找类似 灯报警 的本地化字符串。
        /// </summary>
		public string LightFlashing => Lang.LightFlashing;

        /// <summary>
        ///   查找类似 光源设置 的本地化字符串。
        /// </summary>
		public string LightingSettings => Lang.LightingSettings;

        /// <summary>
        ///   查找类似 AELimits版本 的本地化字符串。
        /// </summary>
		public string LimitsVersion => Lang.LimitsVersion;

        /// <summary>
        ///   查找类似 线 的本地化字符串。
        /// </summary>
		public string Line => Lang.Line;

        /// <summary>
        ///   查找类似 线激光 的本地化字符串。
        /// </summary>
		public string LineLaser => Lang.LineLaser;

        /// <summary>
        ///   查找类似 线体 的本地化字符串。
        /// </summary>
		public string Liner => Lang.Liner;

        /// <summary>
        ///   查找类似 线延长比 的本地化字符串。
        /// </summary>
		public string LineScale => Lang.LineScale;

        /// <summary>
        ///   查找类似 线扫 的本地化字符串。
        /// </summary>
		public string LineScan => Lang.LineScan;

        /// <summary>
        ///   查找类似 线宽 的本地化字符串。
        /// </summary>
		public string LineWidth => Lang.LineWidth;

        /// <summary>
        ///   查找类似 加载 的本地化字符串。
        /// </summary>
		public string Load => Lang.Load;

        /// <summary>
        ///   查找类似 自动LoadCell 的本地化字符串。
        /// </summary>
		public string LoadCell => Lang.LoadCell;

        /// <summary>
        ///   查找类似 运行中 的本地化字符串。
        /// </summary>
		public string Loading => Lang.Loading;

        /// <summary>
        ///   查找类似 上料仓 的本地化字符串。
        /// </summary>
		public string LoadingSilo => Lang.LoadingSilo;

        /// <summary>
        ///   查找类似 日志 的本地化字符串。
        /// </summary>
		public string Log => Lang.Log;

        /// <summary>
        ///   查找类似 日志备份天数 的本地化字符串。
        /// </summary>
		public string LogBackUpDays => Lang.LogBackUpDays;

        /// <summary>
        ///   查找类似 逻辑 的本地化字符串。
        /// </summary>
		public string Logic => Lang.Logic;

        /// <summary>
        ///   查找类似 逻辑判断 的本地化字符串。
        /// </summary>
		public string LogicCalculator => Lang.LogicCalculator;

        /// <summary>
        ///   查找类似 登录 的本地化字符串。
        /// </summary>
		public string Login => Lang.Login;

        /// <summary>
        ///   查找类似 登录等级 的本地化字符串。
        /// </summary>
		public string LoginLevel => Lang.LoginLevel;

        /// <summary>
        ///   查找类似 登录模式 的本地化字符串。
        /// </summary>
		public string LoginMode => Lang.LoginMode;

        /// <summary>
        ///   查找类似 登录名 的本地化字符串。
        /// </summary>
		public string LoginName => Lang.LoginName;

        /// <summary>
        ///   查找类似 登出 的本地化字符串。
        /// </summary>
		public string Logout => Lang.Logout;

        /// <summary>
        ///   查找类似 循环 的本地化字符串。
        /// </summary>
		public string Loop => Lang.Loop;

        /// <summary>
        ///   查找类似 低风速模式电流下限 的本地化字符串。
        /// </summary>
		public string LowCurrentLowLimit => Lang.LowCurrentLowLimit;

        /// <summary>
        ///   查找类似 低风速模式电流上限 的本地化字符串。
        /// </summary>
		public string LowCurrentUpperLimit => Lang.LowCurrentUpperLimit;

        /// <summary>
        ///   查找类似 公差下限 的本地化字符串。
        /// </summary>
		public string LowerLimit => Lang.LowerLimit;

        /// <summary>
        ///   查找类似 MCH弹片测量数据上传 的本地化字符串。
        /// </summary>
		public string LSMesUnLoad => Lang.LSMesUnLoad;

        /// <summary>
        ///   查找类似 排线SN管理 的本地化字符串。
        /// </summary>
		public string CableSNManager => Lang.CableSNManager;

        /// <summary>
        ///   查找类似 智能驾驶舱 的本地化字符串。
        /// </summary>
		public string LusterSmartCockpit => Lang.LusterSmartCockpit;

        /// <summary>
        ///   查找类似 Mac地址 的本地化字符串。
        /// </summary>
		public string MacAddress => Lang.MacAddress;

        /// <summary>
        ///   查找类似 机种 的本地化字符串。
        /// </summary>
		public string Machine => Lang.Machine;

        /// <summary>
        ///   查找类似 机台配置 的本地化字符串。
        /// </summary>
		public string MachineConfigure => Lang.MachineConfigure;

        /// <summary>
        ///   查找类似 主流线治具数量 的本地化字符串。
        /// </summary>
		public string MainCarrierNum => Lang.MainCarrierNum;

        /// <summary>
        ///   查找类似 参数导入确认 的本地化字符串。
        /// </summary>
		public string MainParameters => Lang.MainParameters;

        /// <summary>
        ///   查找类似 保养 的本地化字符串。
        /// </summary>
		public string Maintenance => Lang.Maintenance;

        /// <summary>
        ///   查找类似 Vision管理部门 的本地化字符串。
        /// </summary>
		public string ManageDept_Vision => Lang.ManageDept_Vision;

        /// <summary>
        ///   查找类似 手动 的本地化字符串。
        /// </summary>
		public string Manual => Lang.Manual;

        /// <summary>
        ///   查找类似 手动获取条码 的本地化字符串。
        /// </summary>
		public string ManualGetBarcode => Lang.ManualGetBarcode;

        /// <summary>
        ///   查找类似 手动切换 的本地化字符串。
        /// </summary>
		public string ManualSwitch => Lang.ManualSwitch;

        /// <summary>
        ///   查找类似 物料 的本地化字符串。
        /// </summary>
		public string Material => Lang.Material;

        /// <summary>
        ///   查找类似 未获取到辅料名称 的本地化字符串。
        /// </summary>
		public string MaterialNotObtained => Lang.MaterialNotObtained;

        /// <summary>
        ///   查找类似 每页数量 的本地化字符串。
        /// </summary>
		public string MaxPerPage => Lang.MaxPerPage;

        /// <summary>
        ///   查找类似 点合并 的本地化字符串。
        /// </summary>
		public string MergePoints => Lang.MergePoints;

        /// <summary>
        ///   查找类似 网格数据 的本地化字符串。
        /// </summary>
		public string Mesh => Lang.Mesh;

        /// <summary>
        ///   查找类似 中风速模式电流下限 的本地化字符串。
        /// </summary>
		public string MiddleCurrentLowLimit => Lang.MiddleCurrentLowLimit;

        /// <summary>
        ///   查找类似 中风速模式电流上限 的本地化字符串。
        /// </summary>
		public string MiddleCurrentUpperLimit => Lang.MiddleCurrentUpperLimit;

        /// <summary>
        ///   查找类似 杂项 的本地化字符串。
        /// </summary>
		public string Miscellaneous => Lang.Miscellaneous;

        /// <summary>
        ///   查找类似 ModbusRTU 的本地化字符串。
        /// </summary>
		public string ModbusRTU => Lang.ModbusRTU;

        /// <summary>
        ///   查找类似 模式 的本地化字符串。
        /// </summary>
		public string Model => Lang.Model;

        /// <summary>
        ///   查找类似 模型显示模式 的本地化字符串。
        /// </summary>
		public string ModelDisplayMode => Lang.ModelDisplayMode;

        /// <summary>
        ///   查找类似 模型点集 的本地化字符串。
        /// </summary>
		public string ModelGroup => Lang.ModelGroup;

        /// <summary>
        ///   查找类似 模块 的本地化字符串。
        /// </summary>
		public string Module => Lang.Module;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string Module_Name => Lang.Module_Name;

        /// <summary>
        ///   查找类似 报警信息 的本地化字符串。
        /// </summary>
		public string ModuleError => Lang.ModuleError;

        /// <summary>
        ///   查找类似 模块名称 的本地化字符串。
        /// </summary>
		public string ModuleName => Lang.ModuleName;

        /// <summary>
        ///   查找类似 模块设置 的本地化字符串。
        /// </summary>
		public string ModuleSet => Lang.ModuleSet;

        /// <summary>
        ///   查找类似 月 的本地化字符串。
        /// </summary>
		public string Month => Lang.Month;

        /// <summary>
        ///   查找类似 形态学 的本地化字符串。
        /// </summary>
		public string Morphological => Lang.Morphological;

        /// <summary>
        ///   查找类似 运动 的本地化字符串。
        /// </summary>
		public string Motion => Lang.Motion;

        /// <summary>
        ///   查找类似 控制卡 的本地化字符串。
        /// </summary>
		public string MotionCard => Lang.MotionCard;

        /// <summary>
        ///   查找类似 多场景下，每个轴的运动优先级 的本地化字符串。
        /// </summary>
		public string MotionPriorityOfEachAxisInMultipleScenes => Lang.MotionPriorityOfEachAxisInMultipleScenes;

        /// <summary>
        ///   查找类似 运动速度 的本地化字符串。
        /// </summary>
		public string MotionSpeed => Lang.MotionSpeed;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string MotionSpeed_mm_s_ => Lang.MotionSpeed_mm_s_;

        /// <summary>
        ///   查找类似 运动速度，单位mm 的本地化字符串。
        /// </summary>
		public string MotionSpeedWithUnit => Lang.MotionSpeedWithUnit;

        /// <summary>
        ///   查找类似 运动方向 的本地化字符串。
        /// </summary>
		public string MoveDirection => Lang.MoveDirection;

        /// <summary>
        ///   查找类似 移动位置 的本地化字符串。
        /// </summary>
		public string MovePosition => Lang.MovePosition;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string MovePostion_mm_ => Lang.MovePostion_mm_;

        /// <summary>
        ///   查找类似 移动到 的本地化字符串。
        /// </summary>
		public string MoveTo => Lang.MoveTo;

        /// <summary>
        ///   查找类似 多轴 的本地化字符串。
        /// </summary>
		public string MultiAxis => Lang.MultiAxis;

        /// <summary>
        ///   查找类似 名称 的本地化字符串。
        /// </summary>
		public string Name => Lang.Name;

        /// <summary>
        ///   查找类似 新保压 的本地化字符串。
        /// </summary>
		public string NewPressurize => Lang.NewPressurize;

        /// <summary>
        ///   查找类似 新增工程 的本地化字符串。
        /// </summary>
		public string NewProject => Lang.NewProject;

        /// <summary>
        ///   查找类似 新增配方 的本地化字符串。
        /// </summary>
		public string NewRecipe => Lang.NewRecipe;

        /// <summary>
        ///   查找类似 下游要料 的本地化字符串。
        /// </summary>
		public string NextGet => Lang.NextGet;

        /// <summary>
        ///   查找类似 下一页 的本地化字符串。
        /// </summary>
		public string NextPage => Lang.NextPage;

        /// <summary>
        ///   查找类似 NG模块 的本地化字符串。
        /// </summary>
		public string NG => Lang.NG;

        /// <summary>
        ///   查找类似 NG数 的本地化字符串。
        /// </summary>
		public string NGAmount => Lang.NGAmount;

        /// <summary>
        ///   查找类似 NG模组 的本地化字符串。
        /// </summary>
		public string NGGroup => Lang.NGGroup;

        /// <summary>
        ///   查找类似 NG率 的本地化字符串。
        /// </summary>
		public string NGRate => Lang.NGRate;

        /// <summary>
        ///   查找类似 NG原因 的本地化字符串。
        /// </summary>
		public string NGReason => Lang.NGReason;

        /// <summary>
        ///   查找类似 NG工站 的本地化字符串。
        /// </summary>
		public string NGStation => Lang.NGStation;

        /// <summary>
        ///   查找类似 否 的本地化字符串。
        /// </summary>
		public string No => Lang.No;

        /// <summary>
        ///   查找类似 暂无数据 的本地化字符串。
        /// </summary>
		public string NoData => Lang.NoData;

        /// <summary>
        ///   查找类似 未找到对应匹配的设备 的本地化字符串。
        /// </summary>
		public string NoMatchDeviceFound => Lang.NoMatchDeviceFound;

        /// <summary>
        ///   查找类似 工程下没有配方 的本地化字符串。
        /// </summary>
		public string NoRecipeInProject => Lang.NoRecipeInProject;

        /// <summary>
        ///   查找类似 未找到激活的配方的路径 的本地化字符串。
        /// </summary>
		public string NotFoundActiveRecipePath => Lang.NotFoundActiveRecipePath;

        /// <summary>
        ///   查找类似 空字符 的本地化字符串。
        /// </summary>
		public string Null => Lang.Null;

        /// <summary>
        ///   查找类似 循环次数 的本地化字符串。
        /// </summary>
		public string NumberOfCycles => Lang.NumberOfCycles;

        /// <summary>
        ///   查找类似 获取软件版本 的本地化字符串。
        /// </summary>
		public string ObtainSwVersion => Lang.ObtainSwVersion;

        /// <summary>
        ///   查找类似 离线模式 的本地化字符串。
        /// </summary>
		public string OffLineMode => Lang.OffLineMode;

        /// <summary>
        ///   查找类似 OK数 的本地化字符串。
        /// </summary>
		public string OKAmount => Lang.OKAmount;

        /// <summary>
        ///   查找类似 OK率 的本地化字符串。
        /// </summary>
		public string OKRate => Lang.OKRate;

        /// <summary>
        ///   查找类似 在线模式 的本地化字符串。
        /// </summary>
		public string OnLineMode => Lang.OnLineMode;

        /// <summary>
        ///   查找类似 透明度 的本地化字符串。
        /// </summary>
		public string Opacity => Lang.Opacity;

        /// <summary>
        ///   查找类似 打开 的本地化字符串。
        /// </summary>
		public string Open => Lang.Open;

        /// <summary>
        ///   查找类似 开关门 的本地化字符串。
        /// </summary>
		public string OpenCloseDoor => Lang.OpenCloseDoor;

        /// <summary>
        ///   查找类似 打开项目 的本地化字符串。
        /// </summary>
		public string OpenProject => Lang.OpenProject;

        /// <summary>
        ///   查找类似 开启提示 的本地化字符串。
        /// </summary>
		public string OPenPrompt => Lang.OPenPrompt;

        /// <summary>
        ///   查找类似 操作 的本地化字符串。
        /// </summary>
		public string Operate => Lang.Operate;

        /// <summary>
        ///   查找类似 操作时间 的本地化字符串。
        /// </summary>
		public string OperateTime => Lang.OperateTime;

        /// <summary>
        ///   查找类似 操纵类型 的本地化字符串。
        /// </summary>
		public string OperateType => Lang.OperateType;

        /// <summary>
        ///   查找类似 操作提示 的本地化字符串。
        /// </summary>
		public string OperatingTips => Lang.OperatingTips;

        /// <summary>
        ///   查找类似 操作类型 的本地化字符串。
        /// </summary>
		public string OperationType => Lang.OperationType;

        /// <summary>
        ///   查找类似 工单 的本地化字符串。
        /// </summary>
		public string Order => Lang.Order;

        /// <summary>
        ///   查找类似 原始密码 的本地化字符串。
        /// </summary>
		public string OriginalPassword => Lang.OriginalPassword;

        /// <summary>
        ///   查找类似 原密码输入错误 的本地化字符串。
        /// </summary>
		public string OriginalPassWordWrong => Lang.OriginalPassWordWrong;

        /// <summary>
        ///   查找类似 原点限位 的本地化字符串。
        /// </summary>
		public string OriginLimit => Lang.OriginLimit;

        /// <summary>
        ///   查找类似 其他 的本地化字符串。
        /// </summary>
		public string Others => Lang.Others;

        /// <summary>
        ///   查找类似 出站时间 的本地化字符串。
        /// </summary>
		public string OutBoundTime => Lang.OutBoundTime;

        /// <summary>
        ///   查找类似 输出IO 的本地化字符串。
        /// </summary>
		public string OutIO => Lang.OutIO;

        /// <summary>
        ///   查找类似 局外点拟合 的本地化字符串。
        /// </summary>
		public string OutlierFit => Lang.OutlierFit;

        /// <summary>
        ///   查找类似 不在范围内 的本地化字符串。
        /// </summary>
		public string OutOfRange => Lang.OutOfRange;

        /// <summary>
        ///   查找类似 输出参数 的本地化字符串。
        /// </summary>
		public string OutParam => Lang.OutParam;

        /// <summary>
        ///   查找类似 输出参数别名 的本地化字符串。
        /// </summary>
		public string OutportParameterName => Lang.OutportParameterName;

        /// <summary>
        ///   查找类似 输出 的本地化字符串。
        /// </summary>
		public string Output => Lang.Output;

        /// <summary>
        ///   查找类似 输出项 的本地化字符串。
        /// </summary>
		public string OutPutItem => Lang.OutPutItem;

        /// <summary>
        ///   查找类似 输出项设置未保存，请先点击保存 的本地化字符串。
        /// </summary>
		public string OutPutItemUnSave => Lang.OutPutItemUnSave;

        /// <summary>
        ///   查找类似 输出参数 的本地化字符串。
        /// </summary>
		public string OutputParameter => Lang.OutputParameter;

        /// <summary>
        ///   查找类似 页面控制 的本地化字符串。
        /// </summary>
		public string PageControl => Lang.PageControl;

        /// <summary>
        ///   查找类似 页面模式 的本地化字符串。
        /// </summary>
		public string PageMode => Lang.PageMode;

        /// <summary>
        ///   查找类似 并行 的本地化字符串。
        /// </summary>
		public string Parallel => Lang.Parallel;

        /// <summary>
        ///   查找类似 平行度 的本地化字符串。
        /// </summary>
		public string Parallelism => Lang.Parallelism;

        /// <summary>
        ///   查找类似 参数 的本地化字符串。
        /// </summary>
		public string Parameter => Lang.Parameter;

        /// <summary>
        ///   查找类似 参数配置 的本地化字符串。
        /// </summary>
		public string ParameterConfig => Lang.ParameterConfig;

        /// <summary>
        ///   查找类似 参数配置 的本地化字符串。
        /// </summary>
		public string ParameterConfigure => Lang.ParameterConfigure;

        /// <summary>
        ///   查找类似 字符解析 的本地化字符串。
        /// </summary>
		public string ParseString => Lang.ParseString;

        /// <summary>
        ///   查找类似 卷料批次名称 的本地化字符串。
        /// </summary>
		public string PartName => Lang.PartName;

        /// <summary>
        ///   查找类似 密码 的本地化字符串。
        /// </summary>
		public string PassWord => Lang.PassWord;

        /// <summary>
        ///   查找类似 密码错误 的本地化字符串。
        /// </summary>
		public string PassWordError => Lang.PassWordError;

        /// <summary>
        ///   查找类似 粘贴 的本地化字符串。
        /// </summary>
		public string Paste => Lang.Paste;

        /// <summary>
        ///   查找类似 暂停 的本地化字符串。
        /// </summary>
		public string Pause => Lang.Pause;

        /// <summary>
        ///   查找类似 暂停中 的本地化字符串。
        /// </summary>
		public string Paused => Lang.Paused;

        /// <summary>
        ///   查找类似 暂停灯 的本地化字符串。
        /// </summary>
		public string PauseLamp => Lang.PauseLamp;

        /// <summary>
        ///   查找类似 PC心跳 的本地化字符串。
        /// </summary>
		public string PCHeartbeat => Lang.PCHeartbeat;

        /// <summary>
        ///   查找类似 PC相关 的本地化字符串。
        /// </summary>
		public string PCRelevant => Lang.PCRelevant;

        /// <summary>
        ///   查找类似 PC状态 的本地化字符串。
        /// </summary>
		public string PCStatus => Lang.PCStatus;

        /// <summary>
        ///   查找类似 PDCA 的本地化字符串。
        /// </summary>
		public string PDCA => Lang.PDCA;

        /// <summary>
        ///   查找类似 AE上传 的本地化字符串。
        /// </summary>
		public string PDCAELimit => Lang.PDCAELimit;

        /// <summary>
        ///   查找类似 AELimt上传 的本地化字符串。
        /// </summary>
		public string PDCAELimt => Lang.PDCAELimt;

        /// <summary>
        ///   查找类似 PDCA数据失败补传 的本地化字符串。
        /// </summary>
		public string PDCAFailRetry => Lang.PDCAFailRetry;

        /// <summary>
        ///   查找类似 PDCA业务 的本地化字符串。
        /// </summary>
		public string PDCAFlow => Lang.PDCAFlow;

        /// <summary>
        ///   查找类似 PDCAWIP 的本地化字符串。
        /// </summary>
		public string PDCAWIP => Lang.PDCAWIP;

        /// <summary>
        ///   查找类似 PDO读写 的本地化字符串。
        /// </summary>
		public string PDOAction => Lang.PDOAction;

        /// <summary>
        ///   查找类似 拾取线 的本地化字符串。
        /// </summary>
		public string PickLine => Lang.PickLine;

        /// <summary>
        ///   查找类似 拾取面 的本地化字符串。
        /// </summary>
		public string PickPlane => Lang.PickPlane;

        /// <summary>
        ///   查找类似 拾取点 的本地化字符串。
        /// </summary>
		public string PickPoint => Lang.PickPoint;

        /// <summary>
        ///   查找类似 图片存储路径 的本地化字符串。
        /// </summary>
		public string PictureStoragePath => Lang.PictureStoragePath;

        /// <summary>
        ///   查找类似 面 的本地化字符串。
        /// </summary>
		public string Plane => Lang.Plane;

        /// <summary>
        ///   查找类似 厂区 的本地化字符串。
        /// </summary>
		public string PlantArea => Lang.PlantArea;

        /// <summary>
        ///   查找类似 PLC 的本地化字符串。
        /// </summary>
		public string PLC => Lang.PLC;

        /// <summary>
        ///   查找类似 PLC地址 的本地化字符串。
        /// </summary>
		public string PLCAddress => Lang.PLCAddress;

        /// <summary>
        ///   查找类似 PLC清错 的本地化字符串。
        /// </summary>
		public string PLCClearMistake => Lang.PLCClearMistake;

        /// <summary>
        ///   查找类似 PLC配置 的本地化字符串。
        /// </summary>
		public string PLCConfigure => Lang.PLCConfigure;

        /// <summary>
        ///   查找类似 PLC服务器 的本地化字符串。
        /// </summary>
		public string PLCServer => Lang.PLCServer;

        /// <summary>
        ///   查找类似 Plc工站 的本地化字符串。
        /// </summary>
		public string PlcStation => Lang.PlcStation;

        /// <summary>
        ///   查找类似 PLC状态 的本地化字符串。
        /// </summary>
		public string PLCStatus => Lang.PLCStatus;

        /// <summary>
        ///   查找类似 Plc版本 的本地化字符串。
        /// </summary>
		public string PlcVersion => Lang.PlcVersion;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string Please_Enter_AlarmCode => Lang.Please_Enter_AlarmCode;

        /// <summary>
        ///   查找类似 请检查数据格式或类型！ 的本地化字符串。
        /// </summary>
		public string PleaseCheckTheDataFormatorType => Lang.PleaseCheckTheDataFormatorType;

        /// <summary>
        ///   查找类似 请输入大于零的整数！ 的本地化字符串。
        /// </summary>
		public string PleaseEnterAnIntegerGreaterThan0 => Lang.PleaseEnterAnIntegerGreaterThan0;

        /// <summary>
        ///   查找类似 请输入单字节分割符！ 的本地化字符串。
        /// </summary>
		public string PleaseEnterASingleByteDelimiter => Lang.PleaseEnterASingleByteDelimiter;

        /// <summary>
        ///   查找类似 请输入条件 的本地化字符串。
        /// </summary>
		public string PleaseEnterConditions => Lang.PleaseEnterConditions;

        /// <summary>
        ///   查找类似 请输入SN编码 的本地化字符串。
        /// </summary>
		public string PleaseEnterSNCode => Lang.PleaseEnterSNCode;

        /// <summary>
        ///   查找类似 下午 的本地化字符串。
        /// </summary>
		public string Pm => Lang.Pm;

        /// <summary>
        ///   查找类似 PNG图片 的本地化字符串。
        /// </summary>
		public string PngImg => Lang.PngImg;

        /// <summary>
        ///   查找类似 点 的本地化字符串。
        /// </summary>
		public string Point => Lang.Point;

        /// <summary>
        ///   查找类似 两点距离 的本地化字符串。
        /// </summary>
		public string Point2Point => Lang.Point2Point;

        /// <summary>
        ///   查找类似 点云 的本地化字符串。
        /// </summary>
		public string PointCloud => Lang.PointCloud;

        /// <summary>
        ///   查找类似 点云显示模式 的本地化字符串。
        /// </summary>
		public string PointCloudDisplayMode => Lang.PointCloudDisplayMode;

        /// <summary>
        ///   查找类似 点云点集 的本地化字符串。
        /// </summary>
		public string PointCloudGroup => Lang.PointCloudGroup;

        /// <summary>
        ///   查找类似 点云尺寸 的本地化字符串。
        /// </summary>
		public string PointCloudSize => Lang.PointCloudSize;

        /// <summary>
        ///   查找类似 指针坐标 的本地化字符串。
        /// </summary>
		public string PointerCoord => Lang.PointerCoord;

        /// <summary>
        ///   查找类似 点尺寸 的本地化字符串。
        /// </summary>
		public string PointSize => Lang.PointSize;

        /// <summary>
        ///   查找类似 点位示教 的本地化字符串。
        /// </summary>
		public string PointTeaching => Lang.PointTeaching;

        /// <summary>
        ///   查找类似 位置输出 的本地化字符串。
        /// </summary>
		public string PositionOutput => Lang.PositionOutput;

        /// <summary>
        ///   查找类似 点位 的本地化字符串。
        /// </summary>
		public string PosLocation => Lang.PosLocation;

        /// <summary>
        ///   查找类似 压力曲线 的本地化字符串。
        /// </summary>
		public string PressDriver => Lang.PressDriver;

        /// <summary>
        ///   查找类似 压力1 的本地化字符串。
        /// </summary>
		public string PressForm1 => Lang.PressForm1;

        /// <summary>
        ///   查找类似 压力2 的本地化字符串。
        /// </summary>
		public string PressForm2 => Lang.PressForm2;

        /// <summary>
        ///   查找类似 压力3 的本地化字符串。
        /// </summary>
		public string PressForm3 => Lang.PressForm3;

        /// <summary>
        ///   查找类似 压力4 的本地化字符串。
        /// </summary>
		public string PressForm4 => Lang.PressForm4;

        /// <summary>
        ///   查找类似 压力5 的本地化字符串。
        /// </summary>
		public string PressForm5 => Lang.PressForm5;

        /// <summary>
        ///   查找类似 压力重复性 的本地化字符串。
        /// </summary>
		public string PressureRepetition => Lang.PressureRepetition;

        /// <summary>
        ///   查找类似 压力传感器 的本地化字符串。
        /// </summary>
		public string PressureSensor => Lang.PressureSensor;

        /// <summary>
        ///   查找类似 保压 的本地化字符串。
        /// </summary>
		public string Pressurize => Lang.Pressurize;

        /// <summary>
        ///   查找类似 上游有料 的本地化字符串。
        /// </summary>
		public string PrevHave => Lang.PrevHave;

        /// <summary>
        ///   查找类似 预览 的本地化字符串。
        /// </summary>
		public string Preview => Lang.Preview;

        /// <summary>
        ///   查找类似 上一页 的本地化字符串。
        /// </summary>
		public string PreviousPage => Lang.PreviousPage;

        /// <summary>
        ///   查找类似 打印 的本地化字符串。
        /// </summary>
		public string Print => Lang.Print;

        /// <summary>
        ///   查找类似 打印机 的本地化字符串。
        /// </summary>
		public string Printer => Lang.Printer;

        /// <summary>
        ///   查找类似 打印预览 的本地化字符串。
        /// </summary>
		public string PrintPreview => Lang.PrintPreview;

        /// <summary>
        ///   查找类似 打印设置 的本地化字符串。
        /// </summary>
		public string PrintSet => Lang.PrintSet;

        /// <summary>
        ///   查找类似 优先级 的本地化字符串。
        /// </summary>
		public string Priority => Lang.Priority;

        /// <summary>
        ///   查找类似 Vision机种 的本地化字符串。
        /// </summary>
		public string Product_Vision => Lang.Product_Vision;

        /// <summary>
        ///   查找类似 产品数 的本地化字符串。
        /// </summary>
		public string ProductAmount => Lang.ProductAmount;

        /// <summary>
        ///   查找类似 产品信息 的本地化字符串。
        /// </summary>
		public string ProductInfo => Lang.ProductInfo;

        /// <summary>
        ///   查找类似 产品NG 的本地化字符串。
        /// </summary>
		public string ProductNG => Lang.ProductNG;

        /// <summary>
        ///   查找类似 产品统计 的本地化字符串。
        /// </summary>
		public string ProductStatistics => Lang.ProductStatistics;

        /// <summary>
        ///   查找类似 产品事件 的本地化字符串。
        /// </summary>
		public string ProEvent => Lang.ProEvent;

        /// <summary>
        ///   查找类似 面轮廓度 的本地化字符串。
        /// </summary>
		public string Profileanysurface => Lang.Profileanysurface;

        /// <summary>
        ///   查找类似 The Program Must be Stopped to Close 的本地化字符串。
        /// </summary>
		public string ProgramMustStop => Lang.ProgramMustStop;

        /// <summary>
        ///   查找类似 程序停止 的本地化字符串。
        /// </summary>
		public string ProgramStop => Lang.ProgramStop;

        /// <summary>
        ///   查找类似 工程 的本地化字符串。
        /// </summary>
		public string Project => Lang.Project;

        /// <summary>
        ///   查找类似 项 目 地 址 的本地化字符串。
        /// </summary>
		public string ProjectAddress => Lang.ProjectAddress;

        /// <summary>
        ///   查找类似 项目已存在！ 的本地化字符串。
        /// </summary>
		public string ProjectAlreadyExists => Lang.ProjectAlreadyExists;

        /// <summary>
        ///   查找类似 项目名或地址不可为空！ 的本地化字符串。
        /// </summary>
		public string ProjectCannotBeEmpty => Lang.ProjectCannotBeEmpty;

        /// <summary>
        ///   查找类似 工程文件错误，无法添加 的本地化字符串。
        /// </summary>
		public string ProjectFileError => Lang.ProjectFileError;

        /// <summary>
        ///   查找类似 项 目 名 称 的本地化字符串。
        /// </summary>
		public string ProjectName => Lang.ProjectName;

        /// <summary>
        ///   查找类似 项目属性 的本地化字符串。
        /// </summary>
		public string ProjectProperty => Lang.ProjectProperty;

        /// <summary>
        ///   查找类似 入料通知 的本地化字符串。
        /// </summary>
		public string ProLoaded => Lang.ProLoaded;

        /// <summary>
        ///   查找类似 提示内容 的本地化字符串。
        /// </summary>
		public string PromptContent => Lang.PromptContent;

        /// <summary>
        ///   查找类似 属性 的本地化字符串。
        /// </summary>
		public string Property => Lang.Property;

        /// <summary>
        ///   查找类似 协议 的本地化字符串。
        /// </summary>
		public string Protocol => Lang.Protocol;

        /// <summary>
        ///   查找类似 脉冲比计算 的本地化字符串。
        /// </summary>
		public string PulseCal => Lang.PulseCal;

        /// <summary>
        ///   查找类似 密码不能为空 的本地化字符串。
        /// </summary>
		public string PwdCannotBeEmpty => Lang.PwdCannotBeEmpty;

        /// <summary>
        ///   查找类似 密码输入不一致，请重新输入 的本地化字符串。
        /// </summary>
		public string PwdInconsistent => Lang.PwdInconsistent;

        /// <summary>
        ///   查找类似 查询 的本地化字符串。
        /// </summary>
		public string Query => Lang.Query;

        /// <summary>
        ///   查找类似 退出 的本地化字符串。
        /// </summary>
		public string Quit => Lang.Quit;

        /// <summary>
        ///   查找类似 是否退出软件? 的本地化字符串。
        /// </summary>
		public string QuitSoftWare => Lang.QuitSoftWare;

        /// <summary>
        ///   查找类似 R_加速时间_Target 的本地化字符串。
        /// </summary>
		public string R_Acceleration_Target => Lang.R_Acceleration_Target;

        /// <summary>
        ///   查找类似 R_加速时间_Actual 的本地化字符串。
        /// </summary>
		public string R_AccelerationTime_Actual => Lang.R_AccelerationTime_Actual;

        /// <summary>
        ///   查找类似 R_速度_Actual 的本地化字符串。
        /// </summary>
		public string R_Speed_Actual => Lang.R_Speed_Actual;

        /// <summary>
        ///   查找类似 R_速度_Target 的本地化字符串。
        /// </summary>
		public string R_Speed_Target => Lang.R_Speed_Target;

        /// <summary>
        ///   查找类似 加载CAD 的本地化字符串。
        /// </summary>
		public string ReadCAD => Lang.ReadCAD;

        /// <summary>
        ///   查找类似 加载点云 的本地化字符串。
        /// </summary>
		public string ReadCloud => Lang.ReadCloud;

        /// <summary>
        ///   查找类似 数据文件 的本地化字符串。
        /// </summary>
		public string ReadDataFile => Lang.ReadDataFile;

        /// <summary>
        ///   查找类似 读入数据 的本地化字符串。
        /// </summary>
		public string ReadDatas => Lang.ReadDatas;

        /// <summary>
        ///   查找类似 读取Fins 的本地化字符串。
        /// </summary>
		public string ReadFins => Lang.ReadFins;

        /// <summary>
        ///   查找类似 加载矩阵 的本地化字符串。
        /// </summary>
		public string ReadMatrix => Lang.ReadMatrix;

        /// <summary>
        ///   查找类似 读取MC 的本地化字符串。
        /// </summary>
		public string ReadMC => Lang.ReadMC;

        /// <summary>
        ///   查找类似 读取Modbus 的本地化字符串。
        /// </summary>
		public string ReadModbus => Lang.ReadModbus;

        /// <summary>
        ///   查找类似 读取PLC 的本地化字符串。
        /// </summary>
		public string ReadPlc => Lang.ReadPlc;

        /// <summary>
        ///   查找类似 读取机械手速度 的本地化字符串。
        /// </summary>
		public string ReadRobotSpeed => Lang.ReadRobotSpeed;

        /// <summary>
        ///   查找类似 加载STL 的本地化字符串。
        /// </summary>
		public string ReadSTL => Lang.ReadSTL;

        /// <summary>
        ///   查找类似 实时 的本地化字符串。
        /// </summary>
		public string RealTime => Lang.RealTime;

        /// <summary>
        ///   查找类似 实时位置 的本地化字符串。
        /// </summary>
		public string RealTimeLocation => Lang.RealTimeLocation;

        /// <summary>
        ///   查找类似 原因 的本地化字符串。
        /// </summary>
		public string Reason => Lang.Reason;

        /// <summary>
        ///   查找类似 最近文件 的本地化字符串。
        /// </summary>
		public string RecentFile => Lang.RecentFile;

        /// <summary>
        ///   查找类似 最近项目 的本地化字符串。
        /// </summary>
		public string RecentProject => Lang.RecentProject;

        /// <summary>
        ///   查找类似 复检 的本地化字符串。
        /// </summary>
		public string ReCheck => Lang.ReCheck;

        /// <summary>
        ///   查找类似 配方版本 的本地化字符串。
        /// </summary>
		public string ReciepeVersion => Lang.ReciepeVersion;

        /// <summary>
        ///   查找类似 配方 的本地化字符串。
        /// </summary>
		public string Recipe => Lang.Recipe;

        /// <summary>
        ///   查找类似 配方备份天数 的本地化字符串。
        /// </summary>
		public string RecipeBackUpDays => Lang.RecipeBackUpDays;

        /// <summary>
        ///   查找类似 配方格式不正确，请检查 的本地化字符串。
        /// </summary>
		public string RecipeFormatWrong => Lang.RecipeFormatWrong;

        /// <summary>
        ///   查找类似 恢复 的本地化字符串。
        /// </summary>
		public string Recoverey => Lang.Recoverey;

        /// <summary>
        ///   查找类似 恢复 的本地化字符串。
        /// </summary>
		public string Recovery => Lang.Recovery;

        /// <summary>
        ///   查找类似 恢复灯 的本地化字符串。
        /// </summary>
		public string RecoveryLamp => Lang.RecoveryLamp;

        /// <summary>
        ///   查找类似 红灯 的本地化字符串。
        /// </summary>
		public string RedLamp => Lang.RedLamp;

        /// <summary>
        ///   查找类似 重做 的本地化字符串。
        /// </summary>
		public string Redo => Lang.Redo;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string Reference_DoubleClick_ => Lang.Reference_DoubleClick_;

        /// <summary>
        ///   查找类似 引用模块 的本地化字符串。
        /// </summary>
		public string RefGroup => Lang.RefGroup;

        /// <summary>
        ///   查找类似 刷新 的本地化字符串。
        /// </summary>
		public string Refresh => Lang.Refresh;

        /// <summary>
        ///   查找类似 刷新频率 的本地化字符串。
        /// </summary>
		public string RefreshFrequency => Lang.RefreshFrequency;

        /// <summary>
        ///   查找类似 区域 的本地化字符串。
        /// </summary>
		public string Region => Lang.Region;

        /// <summary>
        ///   查找类似 注册 的本地化字符串。
        /// </summary>
		public string Register => Lang.Register;

        /// <summary>
        ///   查找类似 注册类型 的本地化字符串。
        /// </summary>
		public string RegisterType => Lang.RegisterType;

        /// <summary>
        ///   查找类似 配准 的本地化字符串。
        /// </summary>
		public string Registration => Lang.Registration;

        /// <summary>
        ///   查找类似 相对 的本地化字符串。
        /// </summary>
		public string Relative => Lang.Relative;

        /// <summary>
        ///   查找类似 相对运动 的本地化字符串。
        /// </summary>
		public string RelativeMotion => Lang.RelativeMotion;

        /// <summary>
        ///   查找类似 发布说明 的本地化字符串。
        /// </summary>
		public string ReleaseNotes => Lang.ReleaseNotes;

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public string Remove => Lang.Remove;

        /// <summary>
        ///   查找类似 重命名 的本地化字符串。
        /// </summary>
		public string Rename => Lang.Rename;

        /// <summary>
        ///   查找类似 重复密码 的本地化字符串。
        /// </summary>
		public string RepeatPassword => Lang.RepeatPassword;

        /// <summary>
        ///   查找类似 更换物料 的本地化字符串。
        /// </summary>
		public string ReplaceMaterials => Lang.ReplaceMaterials;

        /// <summary>
        ///   查找类似 报告 的本地化字符串。
        /// </summary>
		public string Report => Lang.Report;

        /// <summary>
        ///   查找类似 报告目录 的本地化字符串。
        /// </summary>
		public string ReportContents => Lang.ReportContents;

        /// <summary>
        ///   查找类似 报表 的本地化字符串。
        /// </summary>
		public string ReportForm => Lang.ReportForm;

        /// <summary>
        ///   查找类似 报告导航窗 的本地化字符串。
        /// </summary>
		public string ReportNavigation => Lang.ReportNavigation;

        /// <summary>
        ///   查找类似 报告源 的本地化字符串。
        /// </summary>
		public string ReportSource => Lang.ReportSource;

        /// <summary>
        ///   查找类似 报表类型 的本地化字符串。
        /// </summary>
		public string ReportType => Lang.ReportType;

        /// <summary>
        ///   查找类似 复位 的本地化字符串。
        /// </summary>
		public string Reset => Lang.Reset;

        /// <summary>
        ///   查找类似 产能清零 的本地化字符串。
        /// </summary>
		public string ResetCapacity => Lang.ResetCapacity;

        /// <summary>
        ///   查找类似 复位工站 的本地化字符串。
        /// </summary>
		public string ResetStation => Lang.ResetStation;

        /// <summary>
        ///   查找类似 复位变量 的本地化字符串。
        /// </summary>
		public string ResetVariable => Lang.ResetVariable;

        /// <summary>
        ///   查找类似 一旦删除需要重启软件生效 的本地化字符串。
        /// </summary>
		public string RestartTakesEffect => Lang.RestartTakesEffect;

        /// <summary>
        ///   查找类似 结果 的本地化字符串。
        /// </summary>
		public string Result => Lang.Result;

        /// <summary>
        ///   查找类似 重试 的本地化字符串。
        /// </summary>
		public string Retry => Lang.Retry;

        /// <summary>
        ///   查找类似 终止 的本地化字符串。
        /// </summary>
		public string Return => Lang.Return;

        /// <summary>
        ///   查找类似 撤销 的本地化字符串。
        /// </summary>
		public string Revoke => Lang.Revoke;

        /// <summary>
        ///   查找类似 机器人 的本地化字符串。
        /// </summary>
		public string Robot => Lang.Robot;

        /// <summary>
        ///   查找类似 机器人动作 的本地化字符串。
        /// </summary>
		public string RobotAction => Lang.RobotAction;

        /// <summary>
        ///   查找类似 2#机器人动作 的本地化字符串。
        /// </summary>
		public string RobotAction2 => Lang.RobotAction2;

        /// <summary>
        ///   查找类似 机器人信息 的本地化字符串。
        /// </summary>
		public string RobotInfo => Lang.RobotInfo;

        /// <summary>
        ///   查找类似 机器人运动 的本地化字符串。
        /// </summary>
		public string RobotMove => Lang.RobotMove;

        /// <summary>
        ///   查找类似 机器人状态 的本地化字符串。
        /// </summary>
		public string RobotStatus => Lang.RobotStatus;

        /// <summary>
        ///   查找类似 2#机器人状态 的本地化字符串。
        /// </summary>
		public string RobotStatus2 => Lang.RobotStatus2;

        /// <summary>
        ///   查找类似 机器人版本 的本地化字符串。
        /// </summary>
		public string RobotVersion => Lang.RobotVersion;

        /// <summary>
        ///   查找类似 ROI配置 的本地化字符串。
        /// </summary>
		public string ROIConfig => Lang.ROIConfig;

        /// <summary>
        ///   查找类似 卷料 的本地化字符串。
        /// </summary>
		public string RollSet => Lang.RollSet;

        /// <summary>
        ///   查找类似 卷料计算 的本地化字符串。
        /// </summary>
		public string RoolMaterialCal => Lang.RoolMaterialCal;

        /// <summary>
        ///   查找类似 顺时针旋转视图 的本地化字符串。
        /// </summary>
		public string RotateViewClockwise => Lang.RotateViewClockwise;

        /// <summary>
        ///   查找类似 逆时针旋转视图 的本地化字符串。
        /// </summary>
		public string RotateViewCounterclockwise => Lang.RotateViewCounterclockwise;

        /// <summary>
        ///   查找类似 圆度 的本地化字符串。
        /// </summary>
		public string Roundness => Lang.Roundness;

        /// <summary>
        ///   查找类似 常规监控 的本地化字符串。
        /// </summary>
		public string RoutineMonitoring => Lang.RoutineMonitoring;

        /// <summary>
        ///   查找类似 运行 的本地化字符串。
        /// </summary>
		public string Run => Lang.Run;

        /// <summary>
        ///   查找类似 运行全部 F5 的本地化字符串。
        /// </summary>
		public string RunAllF5 => Lang.RunAllF5;

        /// <summary>
        ///   查找类似 执行程序 的本地化字符串。
        /// </summary>
		public string RunExe => Lang.RunExe;

        /// <summary>
        ///   查找类似 运行模式 的本地化字符串。
        /// </summary>
		public string RunMode => Lang.RunMode;

        /// <summary>
        ///   查找类似 运动模式已存在 的本地化字符串。
        /// </summary>
		public string RunModeIsAlreadyExist => Lang.RunModeIsAlreadyExist;

        /// <summary>
        ///   查找类似 (启用编辑后双击修改) 的本地化字符串。
        /// </summary>
		public string RunModeTips => Lang.RunModeTips;

        /// <summary>
        ///   查找类似 流道 的本地化字符串。
        /// </summary>
		public string Runners => Lang.Runners;

        /// <summary>
        ///   查找类似 下一步 的本地化字符串。
        /// </summary>
		public string RunNext => Lang.RunNext;

        /// <summary>
        ///   查找类似 运行中 的本地化字符串。
        /// </summary>
		public string Running => Lang.Running;

        /// <summary>
        ///   查找类似 运行时间 的本地化字符串。
        /// </summary>
		public string RunningTime => Lang.RunningTime;

        /// <summary>
        ///   查找类似 单步运行 的本地化字符串。
        /// </summary>
		public string RunOne => Lang.RunOne;

        /// <summary>
        ///   查找类似 s 的本地化字符串。
        /// </summary>
		public string s => Lang.s;

        /// <summary>
        ///   查找类似 保存 的本地化字符串。
        /// </summary>
		public string Save => Lang.Save;

        /// <summary>
        ///   查找类似 另存为 的本地化字符串。
        /// </summary>
		public string SaveAs => Lang.SaveAs;

        /// <summary>
        ///   查找类似 另存项目 的本地化字符串。
        /// </summary>
		public string SaveAsProject => Lang.SaveAsProject;

        /// <summary>
        ///   查找类似 保存天数 的本地化字符串。
        /// </summary>
		public string SaveDays => Lang.SaveDays;

        /// <summary>
        ///   查找类似 保存文件 的本地化字符串。
        /// </summary>
		public string SaveFile => Lang.SaveFile;

        /// <summary>
        ///   查找类似 保存项目 的本地化字符串。
        /// </summary>
		public string SaveProject => Lang.SaveProject;

        /// <summary>
        ///   查找类似 是否保存工程? 的本地化字符串。
        /// </summary>
		public string SaveProjectTask => Lang.SaveProjectTask;

        /// <summary>
        ///   查找类似 保存成功 的本地化字符串。
        /// </summary>
		public string SaveSuccess => Lang.SaveSuccess;

        /// <summary>
        ///   查找类似 任务要保存吗 的本地化字符串。
        /// </summary>
		public string SaveTask => Lang.SaveTask;

        /// <summary>
        ///   查找类似 扫二维码 的本地化字符串。
        /// </summary>
		public string ScanBarcode => Lang.ScanBarcode;

        /// <summary>
        ///   查找类似 条码长度 的本地化字符串。
        /// </summary>
		public string ScanCodeCount => Lang.ScanCodeCount;

        /// <summary>
        ///   查找类似 条码来源 的本地化字符串。
        /// </summary>
		public string ScanCodeDataSource => Lang.ScanCodeDataSource;

        /// <summary>
        ///   查找类似 扫码统计 的本地化字符串。
        /// </summary>
		public string ScanCodeStatistics => Lang.ScanCodeStatistics;

        /// <summary>
        ///   查找类似 条码码率 的本地化字符串。
        /// </summary>
		public string ScanCodeSuccessRate => Lang.ScanCodeSuccessRate;

        /// <summary>
        ///   查找类似 急停 的本地化字符串。
        /// </summary>
		public string Scram => Lang.Scram;

        /// <summary>
        ///   查找类似 C#脚本 的本地化字符串。
        /// </summary>
		public string Script => Lang.Script;

        /// <summary>
        ///   查找类似 滚动模式 的本地化字符串。
        /// </summary>
		public string ScrollMode => Lang.ScrollMode;

        /// <summary>
        ///   查找类似 SDO读写 的本地化字符串。
        /// </summary>
		public string SDOAction => Lang.SDOAction;

        /// <summary>
        ///   查找类似 搜索文件关键字 的本地化字符串。
        /// </summary>
		public string SearchFileKeywords => Lang.SearchFileKeywords;

        /// <summary>
        ///   查找类似 搜索模块 的本地化字符串。
        /// </summary>
		public string SearchModule => Lang.SearchModule;

        /// <summary>
        ///   查找类似 秒 的本地化字符串。
        /// </summary>
		public string Second => Lang.Second;

        /// <summary>
        ///   查找类似 分割 的本地化字符串。
        /// </summary>
		public string Segment => Lang.Segment;

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public string Select => Lang.Select;

        /// <summary>
        ///   查找类似 已选轴 的本地化字符串。
        /// </summary>
		public string SelectedAxis => Lang.SelectedAxis;

        /// <summary>
        ///   查找类似 选择文件 的本地化字符串。
        /// </summary>
		public string SelectFile => Lang.SelectFile;

        /// <summary>
        ///   查找类似 分号 的本地化字符串。
        /// </summary>
		public string Semicolon => Lang.Semicolon;

        /// <summary>
        ///   查找类似 高级 的本地化字符串。
        /// </summary>
		public string Senior => Lang.Senior;

        /// <summary>
        ///   查找类似 分隔符 的本地化字符串。
        /// </summary>
		public string Separator => Lang.Separator;

        /// <summary>
        ///   查找类似 序号 的本地化字符串。
        /// </summary>
		public string SerialNumber => Lang.SerialNumber;

        /// <summary>
        ///   查找类似 RS232 的本地化字符串。
        /// </summary>
		public string SerialPortDrive => Lang.SerialPortDrive;

        /// <summary>
        ///   查找类似 服务器 的本地化字符串。
        /// </summary>
		public string Server => Lang.Server;

        /// <summary>
        ///   查找类似 设置 的本地化字符串。
        /// </summary>
		public string Set => Lang.Set;

        /// <summary>
        ///   查找类似 设置轴点位 的本地化字符串。
        /// </summary>
		public string SetAxisPos => Lang.SetAxisPos;

        /// <summary>
        ///   查找类似 设置模板 的本地化字符串。
        /// </summary>
		public string SetCoordTemplate => Lang.SetCoordTemplate;

        /// <summary>
        ///   查找类似 显示选项设置 的本地化字符串。
        /// </summary>
		public string SetDisPlayOption => Lang.SetDisPlayOption;

        /// <summary>
        ///   查找类似 设置全局变量 的本地化字符串。
        /// </summary>
		public string SetGlobalVar => Lang.SetGlobalVar;

        /// <summary>
        ///   查找类似 设置信号 的本地化字符串。
        /// </summary>
		public string SetIO => Lang.SetIO;

        /// <summary>
        ///   查找类似 设置光幕 的本地化字符串。
        /// </summary>
		public string SetLightCurtain => Lang.SetLightCurtain;

        /// <summary>
        ///   查找类似 机台模式 的本地化字符串。
        /// </summary>
		public string SetMachineMode => Lang.SetMachineMode;

        /// <summary>
        ///   查找类似 尺寸设置 的本地化字符串。
        /// </summary>
		public string SetMeasure => Lang.SetMeasure;

        /// <summary>
        ///   查找类似 设置MBus 的本地化字符串。
        /// </summary>
		public string SetModbus => Lang.SetModbus;

        /// <summary>
        ///   查找类似 设置Modbus 的本地化字符串。
        /// </summary>
		public string SetModbusEx => Lang.SetModbusEx;

        /// <summary>
        ///   查找类似 设置机器人状态 的本地化字符串。
        /// </summary>
		public string SetRobotStatus => Lang.SetRobotStatus;

        /// <summary>
        ///   查找类似 设置工站 的本地化字符串。
        /// </summary>
		public string SetStation => Lang.SetStation;

        /// <summary>
        ///   查找类似 设置变量 的本地化字符串。
        /// </summary>
		public string SetVariable => Lang.SetVariable;

        /// <summary>
        ///   查找类似 设置工作流 的本地化字符串。
        /// </summary>
		public string SetWorkFlow => Lang.SetWorkFlow;

        /// <summary>
        ///   查找类似 SFC相关配置 的本地化字符串。
        /// </summary>
		public string SFC => Lang.SFC;

        /// <summary>
        ///   查找类似 SFC流程 的本地化字符串。
        /// </summary>
		public string SFCFlow => Lang.SFCFlow;

        /// <summary>
        ///   查找类似 调机料SFC流程 的本地化字符串。
        /// </summary>
		public string SFCFlowTiaoJi => Lang.SFCFlowTiaoJi;

        /// <summary>
        ///   查找类似 SFTP上传 的本地化字符串。
        /// </summary>
		public string SFTPUpload => Lang.SFTPUpload;

        /// <summary>
        ///   查找类似 信号灯 的本地化字符串。
        /// </summary>
		public string SignalLamp => Lang.SignalLamp;

        /// <summary>
        ///   查找类似 单轴喷码 的本地化字符串。
        /// </summary>
		public string SingelAxisFlyShot => Lang.SingelAxisFlyShot;

        /// <summary>
        ///   查找类似 单轴 的本地化字符串。
        /// </summary>
		public string SingleAxis => Lang.SingleAxis;

        /// <summary>
        ///   查找类似 CT 的本地化字符串。
        /// </summary>
		public string SingleCT => Lang.SingleCT;

        /// <summary>
        ///   查找类似 单页 的本地化字符串。
        /// </summary>
		public string SinglePage => Lang.SinglePage;

        /// <summary>
        ///   查找类似 单轴 的本地化字符串。
        /// </summary>
		public string SinleAxis => Lang.SinleAxis;

        /// <summary>
        ///   查找类似 忽略 的本地化字符串。
        /// </summary>
		public string Skip => Lang.Skip;

        /// <summary>
        ///   查找类似 烟雾报警器 的本地化字符串。
        /// </summary>
		public string SmokeAlarmDevice => Lang.SmokeAlarmDevice;

        /// <summary>
        ///   查找类似 平滑 的本地化字符串。
        /// </summary>
		public string Smooth => Lang.Smooth;

        /// <summary>
        ///   查找类似 SN编码 的本地化字符串。
        /// </summary>
		public string SNCode => Lang.SNCode;

        /// <summary>
        ///   查找类似 软件 的本地化字符串。
        /// </summary>
		public string Soft => Lang.Soft;

        /// <summary>
        ///   查找类似 软件配置 的本地化字符串。
        /// </summary>
		public string SoftConfigure => Lang.SoftConfigure;

        /// <summary>
        ///   查找类似 软件信息 的本地化字符串。
        /// </summary>
		public string SoftInformation => Lang.SoftInformation;

        /// <summary>
        ///   查找类似 软件版本 的本地化字符串。
        /// </summary>
		public string SoftVersion => Lang.SoftVersion;

        /// <summary>
        ///   查找类似 按钮点击触发软件停止 的本地化字符串。
        /// </summary>
		public string SoftWareStopByClick => Lang.SoftWareStopByClick;

        /// <summary>
        ///   查找类似 版本更新信息 的本地化字符串。
        /// </summary>
		public string SoftWareUpdateInfo => Lang.SoftWareUpdateInfo;

        /// <summary>
        ///   查找类似 软件版本 的本地化字符串。
        /// </summary>
		public string SoftWareVersion => Lang.SoftWareVersion;

        /// <summary>
        ///   查找类似 方案 的本地化字符串。
        /// </summary>
		public string Solution => Lang.Solution;

        /// <summary>
        ///   查找类似 工程模式 的本地化字符串。
        /// </summary>
		public string SolutionMode => Lang.SolutionMode;

        /// <summary>
        ///   查找类似 来源 的本地化字符串。
        /// </summary>
		public string Source => Lang.Source;

        /// <summary>
        ///   查找类似 空格 的本地化字符串。
        /// </summary>
		public string Space => Lang.Space;

        /// <summary>
        ///   查找类似 间距 的本地化字符串。
        /// </summary>
		public string SpaceFactor => Lang.SpaceFactor;

        /// <summary>
        ///   查找类似 消耗物品 的本地化字符串。
        /// </summary>
		public string SparePartsUsed => Lang.SparePartsUsed;

        /// <summary>
        ///   查找类似 规格值配置 的本地化字符串。
        /// </summary>
		public string SpecSet => Lang.SpecSet;

        /// <summary>
        ///   查找类似 速度系数 的本地化字符串。
        /// </summary>
		public string SpeedFactor => Lang.SpeedFactor;

        /// <summary>
        ///   查找类似 球体 的本地化字符串。
        /// </summary>
		public string Sphere => Lang.Sphere;

        /// <summary>
        ///   查找类似 按位读取 的本地化字符串。
        /// </summary>
		public string SplitIntToBit => Lang.SplitIntToBit;

        /// <summary>
        ///   查找类似 字符串分割 的本地化字符串。
        /// </summary>
		public string SplitString => Lang.SplitString;

        /// <summary>
        ///   查找类似 标准值 的本地化字符串。
        /// </summary>
		public string StandardValue => Lang.StandardValue;

        /// <summary>
        ///   查找类似 启动 的本地化字符串。
        /// </summary>
		public string Start => Lang.Start;

        /// <summary>
        ///   查找类似 起始列 的本地化字符串。
        /// </summary>
		public string StartColumn => Lang.StartColumn;

        /// <summary>
        ///   查找类似 启动灯 的本地化字符串。
        /// </summary>
		public string StartLamp => Lang.StartLamp;

        /// <summary>
        ///   查找类似 开始模块 的本地化字符串。
        /// </summary>
		public string StartModule => Lang.StartModule;

        /// <summary>
        ///   查找类似 开始维修 的本地化字符串。
        /// </summary>
		public string StartRepair => Lang.StartRepair;

        /// <summary>
        ///   查找类似 起始行 的本地化字符串。
        /// </summary>
		public string StartRow => Lang.StartRow;

        /// <summary>
        ///   查找类似 开始工站 的本地化字符串。
        /// </summary>
		public string StartStation => Lang.StartStation;

        /// <summary>
        ///   查找类似 开始时间 的本地化字符串。
        /// </summary>
		public string StartTime => Lang.StartTime;

        /// <summary>
        ///   查找类似 状态 的本地化字符串。
        /// </summary>
		public string State => Lang.State;

        /// <summary>
        ///   查找类似 工站 的本地化字符串。
        /// </summary>
		public string Station => Lang.Station;

        /// <summary>
        ///   查找类似 工站ID 的本地化字符串。
        /// </summary>
		public string StationID => Lang.StationID;

        /// <summary>
        ///   查找类似 工站名称 的本地化字符串。
        /// </summary>
		public string StationName => Lang.StationName;

        /// <summary>
        ///   查找类似 工站总览 的本地化字符串。
        /// </summary>
		public string StationOverview => Lang.StationOverview;

        /// <summary>
        ///   查找类似 工站 的本地化字符串。
        /// </summary>
		public string Stations => Lang.Stations;

        /// <summary>
        ///   查找类似 工站有料 的本地化字符串。
        /// </summary>
		public string StationSet => Lang.StationSet;

        /// <summary>
        ///   查找类似 工站类型 的本地化字符串。
        /// </summary>
		public string StationType => Lang.StationType;

        /// <summary>
        ///   查找类似 统计 的本地化字符串。
        /// </summary>
		public string Statistics => Lang.Statistics;

        /// <summary>
        ///   查找类似 状态 的本地化字符串。
        /// </summary>
		public string Status => Lang.Status;

        /// <summary>
        ///   查找类似 步骤 的本地化字符串。
        /// </summary>
		public string Step => Lang.Step;

        /// <summary>
        ///   查找类似 串行任务 的本地化字符串。
        /// </summary>
		public string StepGroup => Lang.StepGroup;

        /// <summary>
        ///   查找类似 STL格式 的本地化字符串。
        /// </summary>
		public string STL => Lang.STL;

        /// <summary>
        ///   查找类似 停止 的本地化字符串。
        /// </summary>
		public string Stop => Lang.Stop;

        /// <summary>
        ///   查找类似 暂停 F8 的本地化字符串。
        /// </summary>
		public string StopF8 => Lang.StopF8;

        /// <summary>
        ///   查找类似 停止灯 的本地化字符串。
        /// </summary>
		public string StopLamp => Lang.StopLamp;

        /// <summary>
        ///   查找类似 直线度 的本地化字符串。
        /// </summary>
		public string Straightness => Lang.Straightness;

        /// <summary>
        ///   查找类似 字符拼接 的本地化字符串。
        /// </summary>
		public string StringMerge => Lang.StringMerge;

        /// <summary>
        ///   查找类似 字符解析 的本地化字符串。
        /// </summary>
		public string StringParse => Lang.StringParse;

        /// <summary>
        ///   查找类似 物流线治具数量 的本地化字符串。
        /// </summary>
		public string SubCarrierNum => Lang.SubCarrierNum;

        /// <summary>
        ///   查找类似 吸嘴压力标定 的本地化字符串。
        /// </summary>
		public string SuctionNozzle => Lang.SuctionNozzle;

        /// <summary>
        ///   查找类似 多分支 的本地化字符串。
        /// </summary>
		public string Switch => Lang.Switch;

        /// <summary>
        ///   查找类似 条件任务 的本地化字符串。
        /// </summary>
		public string SwitchGroup => Lang.SwitchGroup;

        /// <summary>
        ///   查找类似 系统操作信息 的本地化字符串。
        /// </summary>
		public string SysOperateInfo => Lang.SysOperateInfo;

        /// <summary>
        ///   查找类似 稼动IO 的本地化字符串。
        /// </summary>
		public string SysOperationIO => Lang.SysOperationIO;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string System_Operation_Information => Lang.System_Operation_Information;

        /// <summary>
        ///   查找类似 系统操作提示信息 的本地化字符串。
        /// </summary>
		public string SystemOperationPromptInformation => Lang.SystemOperationPromptInformation;

        /// <summary>
        ///   查找类似 系统操作提示 的本地化字符串。
        /// </summary>
		public string SystemOperationTips => Lang.SystemOperationTips;

        /// <summary>
        ///   查找类似 表格生成 的本地化字符串。
        /// </summary>
		public string TableCreate => Lang.TableCreate;

        /// <summary>
        ///   查找类似 表格写入 的本地化字符串。
        /// </summary>
		public string TableInsert => Lang.TableInsert;

        /// <summary>
        ///   查找类似 压力曲线堆叠图 的本地化字符串。
        /// </summary>
		public string TaikeAnnotatedCurve => Lang.TaikeAnnotatedCurve;

        /// <summary>
        ///   查找类似 大寰音圈电机曲线监控 的本地化字符串。
        /// </summary>
		public string DHVCMMonitor => Lang.DHVCMMonitor;

        /// <summary>
        ///   查找类似 泰科统计 的本地化字符串。
        /// </summary>
		public string TaikeContent => Lang.TaikeContent;

        /// <summary>
        ///   查找类似 泰科曲线 的本地化字符串。
        /// </summary>
		public string TaikeCurve => Lang.TaikeCurve;

        /// <summary>
        ///   查找类似 太科电批 的本地化字符串。
        /// </summary>
		public string TaiKeScrewDriver => Lang.TaiKeScrewDriver;

        /// <summary>
        ///   查找类似 Target_CT 的本地化字符串。
        /// </summary>
		public string Target_CT => Lang.Target_CT;

        /// <summary>
        ///   查找类似 运动的目标单位，单位mm 的本地化字符串。
        /// </summary>
		public string TargetUnitOfMotion => Lang.TargetUnitOfMotion;

        /// <summary>
        ///   查找类似 目标：mm 的本地化字符串。
        /// </summary>
		public string TargetWithUnit => Lang.TargetWithUnit;

        /// <summary>
        ///   查找类似 任务流 的本地化字符串。
        /// </summary>
		public string TaskFlow => Lang.TaskFlow;

        /// <summary>
        ///   查找类似 任务模拟器 的本地化字符串。
        /// </summary>
		public string TaskSimulator => Lang.TaskSimulator;

        /// <summary>
        ///   查找类似 示教 的本地化字符串。
        /// </summary>
		public string Teach => Lang.Teach;

        /// <summary>
        ///   查找类似 示教位置 的本地化字符串。
        /// </summary>
		public string TeachLocation => Lang.TeachLocation;

        /// <summary>
        ///   查找类似 撕膜 的本地化字符串。
        /// </summary>
		public string Tearing => Lang.Tearing;

        /// <summary>
        ///   查找类似 工艺流程 的本地化字符串。
        /// </summary>
		public string TechnologicalProcess => Lang.TechnologicalProcess;

        /// <summary>
        ///   查找类似 测试按钮 的本地化字符串。
        /// </summary>
		public string TestBotton => Lang.TestBotton;

        /// <summary>
        ///   查找类似 测试工站 的本地化字符串。
        /// </summary>
		public string TestStation => Lang.TestStation;

        /// <summary>
        ///   查找类似 本站要料 的本地化字符串。
        /// </summary>
		public string ThisGet => Lang.ThisGet;

        /// <summary>
        ///   查找类似 本站有料 的本地化字符串。
        /// </summary>
		public string ThisHave => Lang.ThisHave;

        /// <summary>
        ///   查找类似 3D 的本地化字符串。
        /// </summary>
		public string ThreeDimision => Lang.ThreeDimision;

        /// <summary>
        ///   查找类似 抛料设置 的本地化字符串。
        /// </summary>
		public string ThrowingSetting => Lang.ThrowingSetting;

        /// <summary>
        ///   查找类似 抛料耗时 的本地化字符串。
        /// </summary>
		public string ThrowingTime => Lang.ThrowingTime;

        /// <summary>
        ///   查找类似 耗时 的本地化字符串。
        /// </summary>
		public string Time => Lang.Time;

        /// <summary>
        ///   查找类似 时间记录事件 的本地化字符串。
        /// </summary>
		public string TimeLogEvent => Lang.TimeLogEvent;

        /// <summary>
        ///   查找类似 定时判断 的本地化字符串。
        /// </summary>
		public string TimerJudge => Lang.TimerJudge;

        /// <summary>
        ///   查找类似 提示 的本地化字符串。
        /// </summary>
		public string Tip => Lang.Tip;

        /// <summary>
        ///   查找类似 标题 的本地化字符串。
        /// </summary>
		public string Title => Lang.Title;

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public string To_Be_Initialized => Lang.To_Be_Initialized;

        /// <summary>
        ///   查找类似 公差 的本地化字符串。
        /// </summary>
		public string Tolerance => Lang.Tolerance;

        /// <summary>
        ///   查找类似 公差参数 的本地化字符串。
        /// </summary>
		public string TolParameter => Lang.TolParameter;

        /// <summary>
        ///   查找类似 工具 的本地化字符串。
        /// </summary>
		public string Tool => Lang.Tool;

        /// <summary>
        ///   查找类似 过大 的本地化字符串。
        /// </summary>
		public string TooLarge => Lang.TooLarge;

        /// <summary>
        ///   查找类似 Cowling报表 的本地化字符串。
        /// </summary>
		public string TorqueForm => Lang.TorqueForm;

        /// <summary>
        ///   查找类似 锁螺丝报表 的本地化字符串。
        /// </summary>
		public string TorqueForm2 => Lang.TorqueForm2;

        /// <summary>
        ///   查找类似 总治具数量 的本地化字符串。
        /// </summary>
		public string TotalCarrierNum => Lang.TotalCarrierNum;

        /// <summary>
        ///   查找类似 TotalCodeSweep 的本地化字符串。
        /// </summary>
		public string TotalCodeSweep => Lang.TotalCodeSweep;

        /// <summary>
        ///   查找类似 总计耗时 的本地化字符串。
        /// </summary>
		public string TotalTime => Lang.TotalTime;

        /// <summary>
        ///   查找类似 训练 的本地化字符串。
        /// </summary>
		public string Train => Lang.Train;

        /// <summary>
        ///   查找类似 变换 的本地化字符串。
        /// </summary>
		public string Transform => Lang.Transform;

        /// <summary>
        ///   查找类似 坐标系变换 的本地化字符串。
        /// </summary>
		public string TransformCoord => Lang.TransformCoord;

        /// <summary>
        ///   查找类似 三色灯状态 的本地化字符串。
        /// </summary>
		public string TriColorStatus => Lang.TriColorStatus;

        /// <summary>
        ///   查找类似 转盘 的本地化字符串。
        /// </summary>
		public string Turntable => Lang.Turntable;

        /// <summary>
        ///   查找类似 2D算子 的本地化字符串。
        /// </summary>
		public string TwoD => Lang.TwoD;

        /// <summary>
        ///   查找类似 2D 的本地化字符串。
        /// </summary>
		public string TwoDimision => Lang.TwoDimision;

        /// <summary>
        ///   查找类似 双页模式 的本地化字符串。
        /// </summary>
		public string TwoPageMode => Lang.TwoPageMode;

        /// <summary>
        ///   查找类似 读取Txt 的本地化字符串。
        /// </summary>
		public string TxtReader => Lang.TxtReader;

        /// <summary>
        ///   查找类似 文本写入 的本地化字符串。
        /// </summary>
		public string TxtWriter => Lang.TxtWriter;

        /// <summary>
        ///   查找类似 类型 的本地化字符串。
        /// </summary>
		public string Type => Lang.Type;

        /// <summary>
        ///   查找类似 类型不支持 的本地化字符串。
        /// </summary>
		public string TypeNotSupported => Lang.TypeNotSupported;

        /// <summary>
        ///   查找类似 U_加速时间_Target 的本地化字符串。
        /// </summary>
		public string U_Acceleration_Target => Lang.U_Acceleration_Target;

        /// <summary>
        ///   查找类似 U_加速时间_Actual 的本地化字符串。
        /// </summary>
		public string U_AccelerationTime_Actual => Lang.U_AccelerationTime_Actual;

        /// <summary>
        ///   查找类似 U_速度_Actual 的本地化字符串。
        /// </summary>
		public string U_Speed_Actual => Lang.U_Speed_Actual;

        /// <summary>
        ///   查找类似 U_速度_Target 的本地化字符串。
        /// </summary>
		public string U_Speed_Target => Lang.U_Speed_Target;

        /// <summary>
        ///   查找类似 单元 的本地化字符串。
        /// </summary>
		public string Unit => Lang.Unit;

        /// <summary>
        ///   查找类似 未知 的本地化字符串。
        /// </summary>
		public string Unknown => Lang.Unknown;

        /// <summary>
        ///   查找类似 未知PLC状态 的本地化字符串。
        /// </summary>
		public string UnknownPLCStatus => Lang.UnknownPLCStatus;

        /// <summary>
        ///   查找类似 未知大小 的本地化字符串。
        /// </summary>
		public string UnknownSize => Lang.UnknownSize;

        /// <summary>
        ///   查找类似 未知工站 的本地化字符串。
        /// </summary>
		public string UnKnownStation => Lang.UnKnownStation;

        /// <summary>
        ///   查找类似 下料仓 的本地化字符串。
        /// </summary>
		public string UnLoadingSilo => Lang.UnLoadingSilo;

        /// <summary>
        ///   查找类似 更新 的本地化字符串。
        /// </summary>
		public string Update => Lang.Update;

        /// <summary>
        ///   查找类似 更新内容 的本地化字符串。
        /// </summary>
		public string UpdateContent => Lang.UpdateContent;

        /// <summary>
        ///   查找类似 更新变量 的本地化字符串。
        /// </summary>
		public string UpdateVar => Lang.UpdateVar;

        /// <summary>
        ///   查找类似 数据上传 的本地化字符串。
        /// </summary>
		public string UploadData => Lang.UploadData;

        /// <summary>
        ///   查找类似 公差上限 的本地化字符串。
        /// </summary>
		public string UpperLimit => Lang.UpperLimit;

        /// <summary>
        ///   查找类似 使用 的本地化字符串。
        /// </summary>
		public string Use => Lang.Use;

        /// <summary>
        ///   查找类似 用户ID 的本地化字符串。
        /// </summary>
		public string UseID => Lang.UseID;

        /// <summary>
        ///   查找类似 用户配置 的本地化字符串。
        /// </summary>
		public string UserConfigure => Lang.UserConfigure;

        /// <summary>
        ///   查找类似 用户列表 的本地化字符串。
        /// </summary>
		public string UserList => Lang.UserList;

        /// <summary>
        ///   查找类似 用户名 的本地化字符串。
        /// </summary>
		public string UserName => Lang.UserName;

        /// <summary>
        ///   查找类似 使用教程 的本地化字符串。
        /// </summary>
		public string UsingTutorials => Lang.UsingTutorials;

        /// <summary>
        ///   查找类似 VA 的本地化字符串。
        /// </summary>
		public string VA => Lang.VA;

        /// <summary>
        ///   查找类似 真空 的本地化字符串。
        /// </summary>
		public string Vacuum => Lang.Vacuum;

        /// <summary>
        ///   查找类似 值 的本地化字符串。
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
        ///   查找类似 仿真多轴 的本地化字符串。
        /// </summary>
		public string VAxisM => Lang.VAxisM;

        /// <summary>
        ///   查找类似 仿真皮带 的本地化字符串。
        /// </summary>
		public string VBelt => Lang.VBelt;

        /// <summary>
        ///   查找类似 仿真按钮 的本地化字符串。
        /// </summary>
		public string VButton => Lang.VButton;

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
        ///   查找类似 版本信息窗 的本地化字符串。
        /// </summary>
		public string VersionInfoDialog => Lang.VersionInfoDialog;

        /// <summary>
        ///   查找类似 仿真电批 的本地化字符串。
        /// </summary>
		public string VESD => Lang.VESD;

        /// <summary>
        ///   查找类似 仿真飞达 的本地化字符串。
        /// </summary>
		public string VFeeder => Lang.VFeeder;

        /// <summary>
        ///   查找类似 视图 的本地化字符串。
        /// </summary>
		public string View => Lang.View;

        /// <summary>
        ///   查找类似 视图居中 的本地化字符串。
        /// </summary>
		public string ViewCentered => Lang.ViewCentered;

        /// <summary>
        ///   查找类似 视图方向设置 的本地化字符串。
        /// </summary>
		public string ViewDirectionSetting => Lang.ViewDirectionSetting;

        /// <summary>
        ///   查找类似 视图翻转 的本地化字符串。
        /// </summary>
		public string ViewFlip => Lang.ViewFlip;

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
        ///   查找类似 显示标签 的本地化字符串。
        /// </summary>
		public string VisibleLabel => Lang.VisibleLabel;

        /// <summary>
        ///   查找类似 Vision相关配置 的本地化字符串。
        /// </summary>
		public string Vision => Lang.Vision;

        /// <summary>
        ///   查找类似 视觉标定 的本地化字符串。
        /// </summary>
		public string VisionCalibration => Lang.VisionCalibration;

        /// <summary>
        ///   查找类似 Vision补充 的本地化字符串。
        /// </summary>
		public string VisionExtra => Lang.VisionExtra;

        /// <summary>
        ///   查找类似 Vision控制参数 的本地化字符串。
        /// </summary>
		public string VisionInformation => Lang.VisionInformation;

        /// <summary>
        ///   查找类似 Vision IP 的本地化字符串。
        /// </summary>
		public string VisionIP => Lang.VisionIP;

        /// <summary>
        ///   查找类似 Vision端口 的本地化字符串。
        /// </summary>
		public string VisionPort => Lang.VisionPort;

        /// <summary>
        ///   查找类似 Vision过程参数 的本地化字符串。
        /// </summary>
		public string VisionProcessData => Lang.VisionProcessData;

        /// <summary>
        ///   查找类似 Vision工站ID 的本地化字符串。
        /// </summary>
		public string VisionStationId => Lang.VisionStationId;

        /// <summary>
        ///   查找类似 视觉版本 的本地化字符串。
        /// </summary>
		public string VisionVersion => Lang.VisionVersion;

        /// <summary>
        ///   查找类似 仿真线激光 的本地化字符串。
        /// </summary>
		public string VLineLaser => Lang.VLineLaser;

        /// <summary>
        ///   查找类似 仿真输出 的本地化字符串。
        /// </summary>
		public string VOutputIO => Lang.VOutputIO;

        /// <summary>
        ///   查找类似 力矩电缸 的本地化字符串。
        /// </summary>
		public string VPCylinder => Lang.VPCylinder;

        /// <summary>
        ///   查找类似 PLC 的本地化字符串。
        /// </summary>
		public string VPlc => Lang.VPlc;

        /// <summary>
        ///   查找类似 仿真打印机 的本地化字符串。
        /// </summary>
		public string VPrinter => Lang.VPrinter;

        /// <summary>
        ///   查找类似 仿真三色灯 的本地化字符串。
        /// </summary>
		public string VTricolorlamp => Lang.VTricolorlamp;

        /// <summary>
        ///   查找类似 仿真真空 的本地化字符串。
        /// </summary>
		public string VVacuum => Lang.VVacuum;

        /// <summary>
        ///   查找类似 等待 的本地化字符串。
        /// </summary>
		public string Wait => Lang.Wait;

        /// <summary>
        ///   查找类似 等待条件 的本地化字符串。
        /// </summary>
		public string WaitCondition => Lang.WaitCondition;

        /// <summary>
        ///   查找类似 等待Fins 的本地化字符串。
        /// </summary>
		public string WaitFins => Lang.WaitFins;

        /// <summary>
        ///   查找类似 等待信号 的本地化字符串。
        /// </summary>
		public string WaitIO => Lang.WaitIO;

        /// <summary>
        ///   查找类似 等待MC 的本地化字符串。
        /// </summary>
		public string WaitMC => Lang.WaitMC;

        /// <summary>
        ///   查找类似 等待MBus 的本地化字符串。
        /// </summary>
		public string WaitModbus => Lang.WaitModbus;

        /// <summary>
        ///   查找类似 等待PLC 的本地化字符串。
        /// </summary>
		public string WaitPlc => Lang.WaitPlc;

        /// <summary>
        ///   查找类似 等待模块 的本地化字符串。
        /// </summary>
		public string WaitStatus => Lang.WaitStatus;

        /// <summary>
        ///   查找类似 警告 的本地化字符串。
        /// </summary>
		public string Warning => Lang.Warning;

        /// <summary>
        ///   查找类似 设备参数读取 的本地化字符串。
        /// </summary>
		public string WebConfigRead => Lang.WebConfigRead;

        /// <summary>
        ///   查找类似 Web请求 的本地化字符串。
        /// </summary>
		public string WebHttp => Lang.WebHttp;

        /// <summary>
        ///   查找类似 周 的本地化字符串。
        /// </summary>
		public string Week => Lang.Week;

        /// <summary>
        ///   查找类似 宽度 的本地化字符串。
        /// </summary>
		public string Width => Lang.Width;

        /// <summary>
        ///   查找类似 条码打印 的本地化字符串。
        /// </summary>
		public string WipPrint => Lang.WipPrint;

        /// <summary>
        ///   查找类似 工艺流程 的本地化字符串。
        /// </summary>
		public string WorkFlow => Lang.WorkFlow;

        /// <summary>
        ///   查找类似 工单 的本地化字符串。
        /// </summary>
		public string WorkOrder => Lang.WorkOrder;

        /// <summary>
        ///   查找类似 写入Fins 的本地化字符串。
        /// </summary>
		public string WriteFins => Lang.WriteFins;

        /// <summary>
        ///   查找类似 写入MC 的本地化字符串。
        /// </summary>
		public string WriteMC => Lang.WriteMC;

        /// <summary>
        ///   查找类似 写入PLC 的本地化字符串。
        /// </summary>
		public string WritePlc => Lang.WritePlc;

        /// <summary>
        ///   查找类似 错误 的本地化字符串。
        /// </summary>
		public string Wrong => Lang.Wrong;

        /// <summary>
        ///   查找类似 X_加速时间_Target 的本地化字符串。
        /// </summary>
		public string X_Acceleration_Target => Lang.X_Acceleration_Target;

        /// <summary>
        ///   查找类似 X_加速时间_Actual 的本地化字符串。
        /// </summary>
		public string X_AccelerationTime_Actual => Lang.X_AccelerationTime_Actual;

        /// <summary>
        ///   查找类似 X_速度_Actual 的本地化字符串。
        /// </summary>
		public string X_Speed_Actual => Lang.X_Speed_Actual;

        /// <summary>
        ///   查找类似 X_速度_Target 的本地化字符串。
        /// </summary>
		public string X_Speed_Target => Lang.X_Speed_Target;

        /// <summary>
        ///   查找类似 鑫精诚压力传感器 的本地化字符串。
        /// </summary>
		public string XJCPressureSensor => Lang.XJCPressureSensor;

        /// <summary>
        ///   查找类似 鑫精诚多通道F600 的本地化字符串。
        /// </summary>
		public string XJCPressureSensorF600 => Lang.XJCPressureSensorF600;

        /// <summary>
        ///   查找类似 Y_加速时间_Target 的本地化字符串。
        /// </summary>
		public string Y_Acceleration_Target => Lang.Y_Acceleration_Target;

        /// <summary>
        ///   查找类似 Y_加速时间_Actual 的本地化字符串。
        /// </summary>
		public string Y_AccelerationTime_Actual => Lang.Y_AccelerationTime_Actual;

        /// <summary>
        ///   查找类似 Y_速度_Actual 的本地化字符串。
        /// </summary>
		public string Y_Speed_Actual => Lang.Y_Speed_Actual;

        /// <summary>
        ///   查找类似 Y_速度_Target 的本地化字符串。
        /// </summary>
		public string Y_Speed_Target => Lang.Y_Speed_Target;

        /// <summary>
        ///   查找类似 黄灯 的本地化字符串。
        /// </summary>
		public string YellowLamp => Lang.YellowLamp;

        /// <summary>
        ///   查找类似 是 的本地化字符串。
        /// </summary>
		public string Yes => Lang.Yes;

        /// <summary>
        ///   查找类似 良率 的本地化字符串。
        /// </summary>
		public string Yield => Lang.Yield;

        /// <summary>
        ///   查找类似 Z_加速时间_Target 的本地化字符串。
        /// </summary>
		public string Z_Acceleration_Target => Lang.Z_Acceleration_Target;

        /// <summary>
        ///   查找类似 Z_加速时间_Actual 的本地化字符串。
        /// </summary>
		public string Z_AccelerationTime_Actual => Lang.Z_AccelerationTime_Actual;

        /// <summary>
        ///   查找类似 Z_速度_Actual 的本地化字符串。
        /// </summary>
		public string Z_Speed_Actual => Lang.Z_Speed_Actual;

        /// <summary>
        ///   查找类似 Z_速度_Target 的本地化字符串。
        /// </summary>
		public string Z_Speed_Target => Lang.Z_Speed_Target;

        /// <summary>
        ///   查找类似 Z轴安全区 的本地化字符串。
        /// </summary>
		public string ZAxisSafeRegion => Lang.ZAxisSafeRegion;

        /// <summary>
        ///   查找类似 缩放 的本地化字符串。
        /// </summary>
		public string Zoom => Lang.Zoom;

        /// <summary>
        ///   查找类似 放大 的本地化字符串。
        /// </summary>
		public string ZoomIn => Lang.ZoomIn;

        /// <summary>
        ///   查找类似 缩小 的本地化字符串。
        /// </summary>
		public string ZoomOut => Lang.ZoomOut;


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class LangKeys
    {
        /// <summary>
        ///   查找类似 绝对 的本地化字符串。
        /// </summary>
		public static string Absolutely = nameof(Absolutely);

        /// <summary>
        ///   查找类似 绝对运动 的本地化字符串。
        /// </summary>
		public static string AbsoluteMotion = nameof(AbsoluteMotion);

        /// <summary>
        ///   查找类似 加速度 的本地化字符串。
        /// </summary>
		public static string Acc = nameof(Acc);

        /// <summary>
        ///   查找类似 辅料数量 的本地化字符串。
        /// </summary>
		public static string AccessoriesAmount = nameof(AccessoriesAmount);

        /// <summary>
        ///   查找类似 辅料批号 的本地化字符串。
        /// </summary>
		public static string AccessoriesBatch = nameof(AccessoriesBatch);

        /// <summary>
        ///   查找类似 辅料名称 的本地化字符串。
        /// </summary>
		public static string AccessoriesName = nameof(AccessoriesName);

        /// <summary>
        ///   查找类似 动作 的本地化字符串。
        /// </summary>
		public static string Action = nameof(Action);

        /// <summary>
        ///   查找类似 激活 的本地化字符串。
        /// </summary>
		public static string Active = nameof(Active);

        /// <summary>
        ///   查找类似 未找到激活的配方 的本地化字符串。
        /// </summary>
		public static string ActiveRecipeNotFound = nameof(ActiveRecipeNotFound);

        /// <summary>
        ///   查找类似 Actual_CT 的本地化字符串。
        /// </summary>
		public static string Actual_CT = nameof(Actual_CT);

        /// <summary>
        ///   查找类似 真实速度=系数*速度 的本地化字符串。
        /// </summary>
		public static string ActualSpeed = nameof(ActualSpeed);

        /// <summary>
        ///   查找类似 新增 的本地化字符串。
        /// </summary>
		public static string Add = nameof(Add);

        /// <summary>
        ///   查找类似 添加班别 的本地化字符串。
        /// </summary>
		public static string AddClass = nameof(AddClass);

        /// <summary>
        ///   查找类似 添加Plc报警 的本地化字符串。
        /// </summary>
		public static string AddPlcAlarm = nameof(AddPlcAlarm);

        /// <summary>
        ///   查找类似 添加产品 的本地化字符串。
        /// </summary>
		public static string AddProduct = nameof(AddProduct);

        /// <summary>
        ///   查找类似 添加用户 的本地化字符串。
        /// </summary>
		public static string AddUser = nameof(AddUser);

        /// <summary>
        ///   查找类似 报警 的本地化字符串。
        /// </summary>
		public static string Alarm = nameof(Alarm);

        /// <summary>
        ///   查找类似 报警地址 的本地化字符串。
        /// </summary>
		public static string AlarmAddress = nameof(AlarmAddress);

        /// <summary>
        ///   查找类似 报警代码 的本地化字符串。
        /// </summary>
		public static string AlarmCode = nameof(AlarmCode);

        /// <summary>
        ///   查找类似 报警配置 的本地化字符串。
        /// </summary>
		public static string AlarmConfigure = nameof(AlarmConfigure);

        /// <summary>
        ///   查找类似 报警内容 的本地化字符串。
        /// </summary>
		public static string AlarmContent = nameof(AlarmContent);

        /// <summary>
        ///   查找类似 报警详细信息 的本地化字符串。
        /// </summary>
		public static string AlarmDetailInfo = nameof(AlarmDetailInfo);

        /// <summary>
        ///   查找类似 报警英文 的本地化字符串。
        /// </summary>
		public static string AlarmEnglish = nameof(AlarmEnglish);

        /// <summary>
        ///   查找类似 报警ID 的本地化字符串。
        /// </summary>
		public static string AlarmID = nameof(AlarmID);

        /// <summary>
        ///   查找类似 报警信息 的本地化字符串。
        /// </summary>
		public static string AlarmInfo = nameof(AlarmInfo);

        /// <summary>
        ///   查找类似 报警中 的本地化字符串。
        /// </summary>
		public static string Alarming = nameof(Alarming);

        /// <summary>
        ///   查找类似 报警监控 的本地化字符串。
        /// </summary>
		public static string AlarmMonitoring = nameof(AlarmMonitoring);

        /// <summary>
        ///   查找类似 报警处理方式 的本地化字符串。
        /// </summary>
		public static string AlarmSolution = nameof(AlarmSolution);

        /// <summary>
        ///   查找类似 报警时长 的本地化字符串。
        /// </summary>
		public static string AlarmTime = nameof(AlarmTime);

        /// <summary>
        ///   查找类似 报警类型 的本地化字符串。
        /// </summary>
		public static string AlarmType = nameof(AlarmType);

        /// <summary>
        ///   查找类似 算法 的本地化字符串。
        /// </summary>
		public static string Algorithm = nameof(Algorithm);

        /// <summary>
        ///   查找类似 最佳拟合对齐 的本地化字符串。
        /// </summary>
		public static string AlignByBestFit = nameof(AlignByBestFit);

        /// <summary>
        ///   查找类似 坐标系对齐 的本地化字符串。
        /// </summary>
		public static string AlignByCoord = nameof(AlignByCoord);

        /// <summary>
        ///   查找类似 初始对齐 的本地化字符串。
        /// </summary>
		public static string AlignByInit = nameof(AlignByInit);

        /// <summary>
        ///   查找类似 RPS对齐 的本地化字符串。
        /// </summary>
		public static string AlignByRPS = nameof(AlignByRPS);

        /// <summary>
        ///   查找类似 对齐 的本地化字符串。
        /// </summary>
		public static string Alignment = nameof(Alignment);

        /// <summary>
        ///   查找类似 全部 的本地化字符串。
        /// </summary>
		public static string All = nameof(All);

        /// <summary>
        ///   查找类似 已存在 的本地化字符串。
        /// </summary>
		public static string AlreadyExists = nameof(AlreadyExists);

        /// <summary>
        ///   查找类似 上午 的本地化字符串。
        /// </summary>
		public static string Am = nameof(Am);

        /// <summary>
        ///   查找类似 模拟量转换 的本地化字符串。
        /// </summary>
		public static string AnalogConvert = nameof(AnalogConvert);

        /// <summary>
        ///   查找类似 角度测量 的本地化字符串。
        /// </summary>
		public static string AngleMeasure = nameof(AngleMeasure);

        /// <summary>
        ///   查找类似 注释生成 的本地化字符串。
        /// </summary>
		public static string AnnotationGeneration = nameof(AnnotationGeneration);

        /// <summary>
        ///   查找类似 AOI 的本地化字符串。
        /// </summary>
		public static string AOI = nameof(AOI);

        /// <summary>
        ///   查找类似 Api版本 的本地化字符串。
        /// </summary>
		public static string ApiVersion = nameof(ApiVersion);

        /// <summary>
        ///   查找类似 应用 的本地化字符串。
        /// </summary>
		public static string Apply = nameof(Apply);

        /// <summary>
        ///   查找类似 线线角度 的本地化字符串。
        /// </summary>
		public static string ArcLine2Line = nameof(ArcLine2Line);

        /// <summary>
        ///   查找类似 线面角度 的本地化字符串。
        /// </summary>
		public static string ArcLine2Plane = nameof(ArcLine2Plane);

        /// <summary>
        ///   查找类似 面面角度 的本地化字符串。
        /// </summary>
		public static string ArcPlane2Plane = nameof(ArcPlane2Plane);

        /// <summary>
        ///   查找类似 面阵 的本地化字符串。
        /// </summary>
		public static string AreaCamera = nameof(AreaCamera);

        /// <summary>
        ///   查找类似 文件整理 的本地化字符串。
        /// </summary>
		public static string ArrangeDocument = nameof(ArrangeDocument);

        /// <summary>
        ///   查找类似 未找到程序集 的本地化字符串。
        /// </summary>
		public static string AssemblyNotFound = nameof(AssemblyNotFound);

        /// <summary>
        ///   查找类似 未找到程序集 的本地化字符串。
        /// </summary>
		public static string AssemblyNotFound1 = nameof(AssemblyNotFound1);

        /// <summary>
        ///   查找类似 异步组 的本地化字符串。
        /// </summary>
		public static string AsyncGroup = nameof(AsyncGroup);

        /// <summary>
        ///   查找类似 通讯端口自动配置 的本地化字符串。
        /// </summary>
		public static string AutoCommunicationConfig = nameof(AutoCommunicationConfig);

        /// <summary>
        ///   查找类似 自动视野 的本地化字符串。
        /// </summary>
		public static string AutoFieldOfView = nameof(AutoFieldOfView);

        /// <summary>
        ///   查找类似 自动对焦 的本地化字符串。
        /// </summary>
		public static string AutoFocusing = nameof(AutoFocusing);

        /// <summary>
        ///   查找类似 自动灰度 的本地化字符串。
        /// </summary>
		public static string AutoGrayScale = nameof(AutoGrayScale);

        /// <summary>
        ///   查找类似 自动压印 的本地化字符串。
        /// </summary>
		public static string AutomaticEmbossing = nameof(AutomaticEmbossing);

        /// <summary>
        ///   查找类似 自动LoadCell 的本地化字符串。
        /// </summary>
		public static string AutomaticLoadCell = nameof(AutomaticLoadCell);

        /// <summary>
        ///   查找类似 自动定位与水平 的本地化字符串。
        /// </summary>
		public static string AutomaticPosAndLeveling = nameof(AutomaticPosAndLeveling);

        /// <summary>
        ///   查找类似 自动运行 的本地化字符串。
        /// </summary>
		public static string AutoRun = nameof(AutoRun);

        /// <summary>
        ///   查找类似 Auto Verification 的本地化字符串。
        /// </summary>
		public static string AutoVerication = nameof(AutoVerication);

        /// <summary>
        ///   查找类似 手眼标定 的本地化字符串。
        /// </summary>
		public static string AutoVisualCalibration = nameof(AutoVisualCalibration);

        /// <summary>
        ///   查找类似 平均耗时 的本地化字符串。
        /// </summary>
		public static string AverageTime = nameof(AverageTime);

        /// <summary>
        ///   查找类似 AvgNoOfCodeSewwp 的本地化字符串。
        /// </summary>
		public static string AvgNoOfCodeSewwp = nameof(AvgNoOfCodeSewwp);

        /// <summary>
        ///   查找类似 轴 的本地化字符串。
        /// </summary>
		public static string Axis = nameof(Axis);

        /// <summary>
        ///   查找类似 多轴龙门 的本地化字符串。
        /// </summary>
		public static string AxisArm = nameof(AxisArm);

        /// <summary>
        ///   查找类似 轴调试 的本地化字符串。
        /// </summary>
		public static string AxisDebug = nameof(AxisDebug);

        /// <summary>
        ///   查找类似 轴位置 的本地化字符串。
        /// </summary>
		public static string AxisPos = nameof(AxisPos);

        /// <summary>
        ///   查找类似 点位运动 的本地化字符串。
        /// </summary>
		public static string AxisPosMove = nameof(AxisPosMove);

        /// <summary>
        ///   查找类似 轴优先级 的本地化字符串。
        /// </summary>
		public static string AxisPriority = nameof(AxisPriority);

        /// <summary>
        ///   查找类似 回流线治具数量 的本地化字符串。
        /// </summary>
		public static string BackCarrierNum = nameof(BackCarrierNum);

        /// <summary>
        ///   查找类似 后台工站 的本地化字符串。
        /// </summary>
		public static string BackgroundStation = nameof(BackgroundStation);

        /// <summary>
        ///   查找类似 备份 的本地化字符串。
        /// </summary>
		public static string BackUp = nameof(BackUp);

        /// <summary>
        ///   查找类似 备份设置 的本地化字符串。
        /// </summary>
		public static string BackUpSet = nameof(BackUpSet);

        /// <summary>
        ///   查找类似 Bali版本 的本地化字符串。
        /// </summary>
		public static string BaliVersion = nameof(BaliVersion);

        /// <summary>
        ///   查找类似 批量导入点 的本地化字符串。
        /// </summary>
		public static string BatchImportPoints = nameof(BatchImportPoints);

        /// <summary>
        ///   查找类似 皮带搬运 的本地化字符串。
        /// </summary>
		public static string BeltCarry = nameof(BeltCarry);

        /// <summary>
        ///   查找类似 最佳拟合 的本地化字符串。
        /// </summary>
		public static string BestFit = nameof(BestFit);

        /// <summary>
        ///   查找类似 二值化 的本地化字符串。
        /// </summary>
		public static string Binarization = nameof(Binarization);

        /// <summary>
        ///   查找类似 阻塞 的本地化字符串。
        /// </summary>
		public static string Block = nameof(Block);

        /// <summary>
        ///   查找类似 包围盒 的本地化字符串。
        /// </summary>
		public static string BoundBox = nameof(BoundBox);

        /// <summary>
        ///   查找类似 分支 的本地化字符串。
        /// </summary>
		public static string Branch = nameof(Branch);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string Branch_Does_Not_Exist = nameof(Branch_Does_Not_Exist);

        /// <summary>
        ///   查找类似 判断分支 的本地化字符串。
        /// </summary>
		public static string BranchGroup = nameof(BranchGroup);

        /// <summary>
        ///   查找类似 浏览 的本地化字符串。
        /// </summary>
		public static string Broswer = nameof(Broswer);

        /// <summary>
        ///   查找类似 业务 的本地化字符串。
        /// </summary>
		public static string Business = nameof(Business);

        /// <summary>
        ///   查找类似 BUSOP 的本地化字符串。
        /// </summary>
		public static string BUSOP = nameof(BUSOP);

        /// <summary>
        ///   查找类似 按钮 的本地化字符串。
        /// </summary>
		public static string Button = nameof(Button);

        /// <summary>
        ///   查找类似 按钮控制 的本地化字符串。
        /// </summary>
		public static string ButtonControl = nameof(ButtonControl);

        /// <summary>
        ///   查找类似 蜂鸣器 的本地化字符串。
        /// </summary>
		public static string Buzzer = nameof(Buzzer);

        /// <summary>
        ///   查找类似 缓存数据 的本地化字符串。
        /// </summary>
		public static string CacheData = nameof(CacheData);

        /// <summary>
        ///   查找类似 CAD模型 的本地化字符串。
        /// </summary>
		public static string CADModel = nameof(CADModel);

        /// <summary>
        ///   查找类似 计时器 的本地化字符串。
        /// </summary>
		public static string CalcTime = nameof(CalcTime);

        /// <summary>
        ///   查找类似 计算器 的本地化字符串。
        /// </summary>
		public static string Calculator = nameof(Calculator);

        /// <summary>
        ///   查找类似 标定 的本地化字符串。
        /// </summary>
		public static string Calib = nameof(Calib);

        /// <summary>
        ///   查找类似 轴系相机标定 的本地化字符串。
        /// </summary>
		public static string CalibByPosMove = nameof(CalibByPosMove);

        /// <summary>
        ///   查找类似 压力线性 的本地化字符串。
        /// </summary>
		public static string CalibrationTable = nameof(CalibrationTable);

        /// <summary>
        ///   查找类似 相机 的本地化字符串。
        /// </summary>
		public static string Camera = nameof(Camera);

        /// <summary>
        ///   查找类似 相机IO 的本地化字符串。
        /// </summary>
		public static string CameraIO = nameof(CameraIO);

        /// <summary>
        ///   查找类似 取消 的本地化字符串。
        /// </summary>
		public static string Cancel = nameof(Cancel);

        /// <summary>
        ///   查找类似 取消模板 的本地化字符串。
        /// </summary>
		public static string CancelCoordTemplate = nameof(CancelCoordTemplate);

        /// <summary>
        ///   查找类似 取消忽略 的本地化字符串。
        /// </summary>
		public static string CancelSkip = nameof(CancelSkip);

        /// <summary>
        ///   查找类似 该工程下配方已激活，无法删除该工程 的本地化字符串。
        /// </summary>
		public static string CannotDeleteProjWhithRecipeActive = nameof(CannotDeleteProjWhithRecipeActive);

        /// <summary>
        ///   查找类似 系统自带用户，无法修改功能权限配置 的本地化字符串。
        /// </summary>
		public static string CannotPermissionToModifyFunction = nameof(CannotPermissionToModifyFunction);

        /// <summary>
        ///   查找类似 产能清零 的本地化字符串。
        /// </summary>
		public static string CapacityReset = nameof(CapacityReset);

        /// <summary>
        ///   查找类似 产能明细统计 的本地化字符串。
        /// </summary>
		public static string CapacityStatistics = nameof(CapacityStatistics);

        /// <summary>
        ///   查找类似 治具黑名单 的本地化字符串。
        /// </summary>
		public static string CarrierBlackList = nameof(CarrierBlackList);

        /// <summary>
        ///   查找类似 载具数量 的本地化字符串。
        /// </summary>
		public static string CarrierCount = nameof(CarrierCount);

        /// <summary>
        ///   查找类似 机种 的本地化字符串。
        /// </summary>
		public static string category_key = nameof(category_key);

        /// <summary>
        ///   查找类似 2D图像采集 的本地化字符串。
        /// </summary>
		public static string CCDImage = nameof(CCDImage);

        /// <summary>
        ///   查找类似 CgAoi 的本地化字符串。
        /// </summary>
		public static string CgAoi = nameof(CgAoi);

        /// <summary>
        ///   查找类似 变更记录 的本地化字符串。
        /// </summary>
		public static string ChangeRecord = nameof(ChangeRecord);

        /// <summary>
        ///   查找类似 变更类型 的本地化字符串。
        /// </summary>
		public static string ChangeType = nameof(ChangeType);

        /// <summary>
        ///   查找类似 曲线列表 的本地化字符串。
        /// </summary>
		public static string ChartList = nameof(ChartList);

        /// <summary>
        ///   查找类似 检查更新 的本地化字符串。
        /// </summary>
		public static string CheckForUpdates = nameof(CheckForUpdates);

        /// <summary>
        ///   查找类似 检查信号 的本地化字符串。
        /// </summary>
		public static string CheckIO = nameof(CheckIO);

        /// <summary>
        ///   查找类似 等待变量 的本地化字符串。
        /// </summary>
		public static string CheckVariable = nameof(CheckVariable);

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public static string Choose = nameof(Choose);

        /// <summary>
        ///   查找类似 选择提示 的本地化字符串。
        /// </summary>
		public static string ChooseTips = nameof(ChooseTips);

        /// <summary>
        ///   查找类似 圆 的本地化字符串。
        /// </summary>
		public static string Circle = nameof(Circle);

        /// <summary>
        ///   查找类似 班别 的本地化字符串。
        /// </summary>
		public static string Class = nameof(Class);

        /// <summary>
        ///   查找类似 清空 的本地化字符串。
        /// </summary>
		public static string Clear = nameof(Clear);

        /// <summary>
        ///   查找类似 清错 的本地化字符串。
        /// </summary>
		public static string ClearMistake = nameof(ClearMistake);

        /// <summary>
        ///   查找类似 关闭 的本地化字符串。
        /// </summary>
		public static string Close = nameof(Close);

        /// <summary>
        ///   查找类似 关闭所有 的本地化字符串。
        /// </summary>
		public static string CloseAll = nameof(CloseAll);

        /// <summary>
        ///   查找类似 关闭其他 的本地化字符串。
        /// </summary>
		public static string CloseOther = nameof(CloseOther);

        /// <summary>
        ///   查找类似 点云拼接 的本地化字符串。
        /// </summary>
		public static string CloudCalib = nameof(CloudCalib);

        /// <summary>
        ///   查找类似 点云去噪 的本地化字符串。
        /// </summary>
		public static string CloudDenoising = nameof(CloudDenoising);

        /// <summary>
        ///   查找类似 点云采样 的本地化字符串。
        /// </summary>
		public static string CloudDownSampling = nameof(CloudDownSampling);

        /// <summary>
        ///   查找类似 网格比较 的本地化字符串。
        /// </summary>
		public static string CloudMesh = nameof(CloudMesh);

        /// <summary>
        ///   查找类似 点云处理 的本地化字符串。
        /// </summary>
		public static string CloudProcess = nameof(CloudProcess);

        /// <summary>
        ///   查找类似 点云配准 的本地化字符串。
        /// </summary>
		public static string CloudRegistration = nameof(CloudRegistration);

        /// <summary>
        ///   查找类似 点云去重 的本地化字符串。
        /// </summary>
		public static string CloudReRepeat = nameof(CloudReRepeat);

        /// <summary>
        ///   查找类似 点云裁剪 的本地化字符串。
        /// </summary>
		public static string CloudSegment = nameof(CloudSegment);

        /// <summary>
        ///   查找类似 点云平滑 的本地化字符串。
        /// </summary>
		public static string CloudSmooth = nameof(CloudSmooth);

        /// <summary>
        ///   查找类似 点云变换 的本地化字符串。
        /// </summary>
		public static string CloudTransform = nameof(CloudTransform);

        /// <summary>
        ///   查找类似 驾驶舱 的本地化字符串。
        /// </summary>
		public static string Cockpit = nameof(Cockpit);

        /// <summary>
        ///   查找类似 驾驶舱IP 的本地化字符串。
        /// </summary>
		public static string CockpitIP = nameof(CockpitIP);

        /// <summary>
        ///   查找类似 驾驶舱端口 的本地化字符串。
        /// </summary>
		public static string CockpitPort = nameof(CockpitPort);

        /// <summary>
        ///   查找类似 颜色 的本地化字符串。
        /// </summary>
		public static string Color = nameof(Color);

        /// <summary>
        ///   查找类似 逗号 的本地化字符串。
        /// </summary>
		public static string Comma = nameof(Comma);

        /// <summary>
        ///   查找类似 通用报警 的本地化字符串。
        /// </summary>
		public static string CommonAlarm = nameof(CommonAlarm);

        /// <summary>
        ///   查找类似 通信 的本地化字符串。
        /// </summary>
		public static string Communication = nameof(Communication);

        /// <summary>
        ///   查找类似 通讯端口配置 的本地化字符串。
        /// </summary>
		public static string Communications = nameof(Communications);

        /// <summary>
        ///   查找类似 通信状态 的本地化字符串。
        /// </summary>
		public static string CommunicationStatus = nameof(CommunicationStatus);

        /// <summary>
        ///   查找类似 通讯连接测试 的本地化字符串。
        /// </summary>
		public static string CommunicationTest = nameof(CommunicationTest);

        /// <summary>
        ///   查找类似 比较 的本地化字符串。
        /// </summary>
		public static string Compare = nameof(Compare);

        /// <summary>
        ///   查找类似 对比查看 的本地化字符串。
        /// </summary>
		public static string CompareLook = nameof(CompareLook);

        /// <summary>
        ///   查找类似 竞争条件 的本地化字符串。
        /// </summary>
		public static string CompeteCondition = nameof(CompeteCondition);

        /// <summary>
        ///   查找类似 排版 的本地化字符串。
        /// </summary>
		public static string Composing = nameof(Composing);

        /// <summary>
        ///   查找类似 条件定时器 的本地化字符串。
        /// </summary>
		public static string ConditionTimer = nameof(ConditionTimer);

        /// <summary>
        ///   查找类似 圆锥 的本地化字符串。
        /// </summary>
		public static string Cone = nameof(Cone);

        /// <summary>
        ///   查找类似 配置电脑网络 的本地化字符串。
        /// </summary>
		public static string ConfigComputerNet = nameof(ConfigComputerNet);

        /// <summary>
        ///   查找类似 配置软件串口 的本地化字符串。
        /// </summary>
		public static string ConfigSoftwareCom = nameof(ConfigSoftwareCom);

        /// <summary>
        ///   查找类似 配置软件网络 的本地化字符串。
        /// </summary>
		public static string ConfigSoftwareNet = nameof(ConfigSoftwareNet);

        /// <summary>
        ///   查找类似 配置 的本地化字符串。
        /// </summary>
		public static string Configuration = nameof(Configuration);

        /// <summary>
        ///   查找类似 配置 的本地化字符串。
        /// </summary>
		public static string Configure = nameof(Configure);

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public static string Confirm = nameof(Confirm);

        /// <summary>
        ///   查找类似 按钮确认 的本地化字符串。
        /// </summary>
		public static string ConfirmButton = nameof(ConfirmButton);

        /// <summary>
        ///   查找类似 确认删除 的本地化字符串。
        /// </summary>
		public static string ConfirmDelete = nameof(ConfirmDelete);

        /// <summary>
        ///   查找类似 确认删除模块 的本地化字符串。
        /// </summary>
		public static string ConfirmDeleteModule = nameof(ConfirmDeleteModule);

        /// <summary>
        ///   查找类似 确认删除用户 的本地化字符串。
        /// </summary>
		public static string ConfirmDeleteUser = nameof(ConfirmDeleteUser);

        /// <summary>
        ///   查找类似 确认删除变量 的本地化字符串。
        /// </summary>
		public static string ConfirmDeleteVar = nameof(ConfirmDeleteVar);

        /// <summary>
        ///   查找类似 确认将模块 的本地化字符串。
        /// </summary>
		public static string ConfirmThatTheModule = nameof(ConfirmThatTheModule);

        /// <summary>
        ///   查找类似 内容 的本地化字符串。
        /// </summary>
		public static string Content = nameof(Content);

        /// <summary>
        ///   查找类似 继续 的本地化字符串。
        /// </summary>
		public static string Continue = nameof(Continue);

        /// <summary>
        ///   查找类似 坐标系 的本地化字符串。
        /// </summary>
		public static string Coord = nameof(Coord);

        /// <summary>
        ///   查找类似 参考坐标系 的本地化字符串。
        /// </summary>
		public static string CoordRef = nameof(CoordRef);

        /// <summary>
        ///   查找类似 拷贝 的本地化字符串。
        /// </summary>
		public static string Copy = nameof(Copy);

        /// <summary>
        ///   查找类似 复制创建 的本地化字符串。
        /// </summary>
		public static string CopyCreate = nameof(CopyCreate);

        /// <summary>
        ///   查找类似 复制需要选中模块[根节点不支持复制] 的本地化字符串。
        /// </summary>
		public static string CopySelectedModule = nameof(CopySelectedModule);

        /// <summary>
        ///   查找类似 CPK检测 的本地化字符串。
        /// </summary>
		public static string CPKTest = nameof(CPKTest);

        /// <summary>
        ///   查找类似 新建 的本地化字符串。
        /// </summary>
		public static string Create = nameof(Create);

        /// <summary>
        ///   查找类似 创建项目 的本地化字符串。
        /// </summary>
		public static string CreateProject = nameof(CreateProject);

        /// <summary>
        ///   查找类似 创建时间 的本地化字符串。
        /// </summary>
		public static string CreateTime = nameof(CreateTime);

        /// <summary>
        ///   查找类似 稼动率 的本地化字符串。
        /// </summary>
		public static string CropRate = nameof(CropRate);

        /// <summary>
        ///   查找类似 稼动率设置 的本地化字符串。
        /// </summary>
		public static string CropRateSet = nameof(CropRateSet);

        /// <summary>
        ///   查找类似 单片耗时 的本地化字符串。
        /// </summary>
		public static string CT = nameof(CT);

        /// <summary>
        ///   查找类似 CT统计 的本地化字符串。
        /// </summary>
		public static string CTStatistics = nameof(CTStatistics);

        /// <summary>
        ///   查找类似 长方体 的本地化字符串。
        /// </summary>
		public static string Cuboid = nameof(Cuboid);

        /// <summary>
        ///   查找类似 最 近 项 目 的本地化字符串。
        /// </summary>
		public static string CurrentProject = nameof(CurrentProject);

        /// <summary>
        ///   查找类似 当前值 的本地化字符串。
        /// </summary>
		public static string CurrentValue = nameof(CurrentValue);

        /// <summary>
        ///   查找类似 当前值2 的本地化字符串。
        /// </summary>
		public static string CurrentValue2 = nameof(CurrentValue2);

        /// <summary>
        ///   查找类似 自定义 的本地化字符串。
        /// </summary>
		public static string Custom = nameof(Custom);

        /// <summary>
        ///   查找类似 自定义模块 的本地化字符串。
        /// </summary>
		public static string CustomModule = nameof(CustomModule);

        /// <summary>
        ///   查找类似 气缸 的本地化字符串。
        /// </summary>
		public static string Cylinder = nameof(Cylinder);

        /// <summary>
        ///   查找类似 数据库 的本地化字符串。
        /// </summary>
		public static string DataBase = nameof(DataBase);

        /// <summary>
        ///   查找类似 数据目录 的本地化字符串。
        /// </summary>
		public static string DataDirectory = nameof(DataDirectory);

        /// <summary>
        ///   查找类似 数据标识 的本地化字符串。
        /// </summary>
		public static string DataMark = nameof(DataMark);

        /// <summary>
        ///   查找类似 数据处理 的本地化字符串。
        /// </summary>
		public static string DataProc = nameof(DataProc);

        /// <summary>
        ///   查找类似 数据处理 的本地化字符串。
        /// </summary>
		public static string DataProcess = nameof(DataProcess);

        /// <summary>
        ///   查找类似 数据转移 的本地化字符串。
        /// </summary>
		public static string DataTransfer = nameof(DataTransfer);

        /// <summary>
        ///   查找类似 数据类型 的本地化字符串。
        /// </summary>
		public static string DataType = nameof(DataType);

        /// <summary>
        ///   查找类似 数据验证 的本地化字符串。
        /// </summary>
		public static string DataValidation = nameof(DataValidation);

        /// <summary>
        ///   查找类似 天 的本地化字符串。
        /// </summary>
		public static string Day = nameof(Day);

        /// <summary>
        ///   查找类似 调试 的本地化字符串。
        /// </summary>
		public static string Debug = nameof(Debug);

        /// <summary>
        ///   查找类似 调试功能 的本地化字符串。
        /// </summary>
		public static string DebugFunction = nameof(DebugFunction);

        /// <summary>
        ///   查找类似 减速度 的本地化字符串。
        /// </summary>
		public static string Dec = nameof(Dec);

        /// <summary>
        ///   查找类似 默认 的本地化字符串。
        /// </summary>
		public static string Default = nameof(Default);

        /// <summary>
        ///   查找类似 默认路径 的本地化字符串。
        /// </summary>
		public static string DefaultPath = nameof(DefaultPath);

        /// <summary>
        ///   查找类似 默认值 的本地化字符串。
        /// </summary>
		public static string DefaultValue = nameof(DefaultValue);

        /// <summary>
        ///   查找类似 延时 的本地化字符串。
        /// </summary>
		public static string Delay = nameof(Delay);

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public static string Delete = nameof(Delete);

        /// <summary>
        ///   查找类似 确认删除自定义模块 的本地化字符串。
        /// </summary>
		public static string DeleteCustomModule = nameof(DeleteCustomModule);

        /// <summary>
        ///   查找类似 移除工程 的本地化字符串。
        /// </summary>
		public static string DeleteProject = nameof(DeleteProject);

        /// <summary>
        ///   查找类似 去噪 的本地化字符串。
        /// </summary>
		public static string Denoising = nameof(Denoising);

        /// <summary>
        ///   查找类似 抛料率 的本地化字符串。
        /// </summary>
		public static string DepositRate = nameof(DepositRate);

        /// <summary>
        ///   查找类似 工位 的本地化字符串。
        /// </summary>
		public static string DeskName = nameof(DeskName);

        /// <summary>
        ///   查找类似 设备 的本地化字符串。
        /// </summary>
		public static string Device = nameof(Device);

        /// <summary>
        ///   查找类似 设备厂商 的本地化字符串。
        /// </summary>
		public static string DeviceFirm = nameof(DeviceFirm);

        /// <summary>
        ///   查找类似 设备信息 的本地化字符串。
        /// </summary>
		public static string DeviceInfo = nameof(DeviceInfo);

        /// <summary>
        ///   查找类似 设备监控 的本地化字符串。
        /// </summary>
		public static string DeviceMonitor = nameof(DeviceMonitor);

        /// <summary>
        ///   查找类似 设备名称 的本地化字符串。
        /// </summary>
		public static string DeviceName = nameof(DeviceName);

        /// <summary>
        ///   查找类似 设备SN 的本地化字符串。
        /// </summary>
		public static string DeviceSN = nameof(DeviceSN);

        /// <summary>
        ///   查找类似 设备状态 的本地化字符串。
        /// </summary>
		public static string DeviceState = nameof(DeviceState);

        /// <summary>
        ///   查找类似 设备类型 的本地化字符串。
        /// </summary>
		public static string DeviceType = nameof(DeviceType);

        /// <summary>
        ///   查找类似 对话框 的本地化字符串。
        /// </summary>
		public static string DialogBox = nameof(DialogBox);

        /// <summary>
        ///   查找类似 数字输入 的本地化字符串。
        /// </summary>
		public static string Digital_In = nameof(Digital_In);

        /// <summary>
        ///   查找类似 单个数字输入 的本地化字符串。
        /// </summary>
		public static string Digital_In_Single = nameof(Digital_In_Single);

        /// <summary>
        ///   查找类似 数字输出 的本地化字符串。
        /// </summary>
		public static string Digital_Out = nameof(Digital_Out);

        /// <summary>
        ///   查找类似 单个数字输出 的本地化字符串。
        /// </summary>
		public static string Digital_Out_Single = nameof(Digital_Out_Single);

        /// <summary>
        ///   查找类似 数字架线 的本地化字符串。
        /// </summary>
		public static string DigitalAss = nameof(DigitalAss);

        /// <summary>
        ///   查找类似 视觉调试 的本地化字符串。
        /// </summary>
		public static string DigitalVision = nameof(DigitalVision);

        /// <summary>
        ///   查找类似 线线距离 的本地化字符串。
        /// </summary>
		public static string DisLine2Line = nameof(DisLine2Line);

        /// <summary>
        ///   查找类似 线面距离 的本地化字符串。
        /// </summary>
		public static string DisLine2Plane = nameof(DisLine2Plane);

        /// <summary>
        ///   查找类似 距离测量 的本地化字符串。
        /// </summary>
		public static string DisMeasure = nameof(DisMeasure);

        /// <summary>
        ///   查找类似 显示 的本地化字符串。
        /// </summary>
		public static string Display = nameof(Display);

        /// <summary>
        ///   查找类似 显示方向 的本地化字符串。
        /// </summary>
		public static string DisplayDirection = nameof(DisplayDirection);

        /// <summary>
        ///   查找类似 显示元素参数设置 的本地化字符串。
        /// </summary>
		public static string DisplayElementParaSetting = nameof(DisplayElementParaSetting);

        /// <summary>
        ///   查找类似 显示类型设置 的本地化字符串。
        /// </summary>
		public static string DisplayTypeSetting = nameof(DisplayTypeSetting);

        /// <summary>
        ///   查找类似 点线距离 的本地化字符串。
        /// </summary>
		public static string DisPoint2Line = nameof(DisPoint2Line);

        /// <summary>
        ///   查找类似 点面距离 的本地化字符串。
        /// </summary>
		public static string DisPoint2Plane = nameof(DisPoint2Plane);

        /// <summary>
        ///   查找类似 两点距离 的本地化字符串。
        /// </summary>
		public static string DisPoint2Point = nameof(DisPoint2Point);

        /// <summary>
        ///   查找类似 完成 的本地化字符串。
        /// </summary>
		public static string Done = nameof(Done);

        /// <summary>
        ///   查找类似 门锁 的本地化字符串。
        /// </summary>
		public static string DoorLock = nameof(DoorLock);

        /// <summary>
        ///   查找类似 双页 的本地化字符串。
        /// </summary>
		public static string DoublePage = nameof(DoublePage);

        /// <summary>
        ///   查找类似 下采样 的本地化字符串。
        /// </summary>
		public static string DownSampling = nameof(DownSampling);

        /// <summary>
        ///   查找类似 宕机原因 的本地化字符串。
        /// </summary>
		public static string DownTimeReason = nameof(DownTimeReason);

        /// <summary>
        ///   查找类似 绘制长方体ROI 的本地化字符串。
        /// </summary>
		public static string DrawCuboidROI = nameof(DrawCuboidROI);

        /// <summary>
        ///   查找类似 绘制圆柱ROI 的本地化字符串。
        /// </summary>
		public static string DrawCylinderROI = nameof(DrawCylinderROI);

        /// <summary>
        ///   查找类似 绘制球体ROI 的本地化字符串。
        /// </summary>
		public static string DrawSphereROI = nameof(DrawSphereROI);

        /// <summary>
        ///   查找类似 转储路径 的本地化字符串。
        /// </summary>
		public static string DumpPath = nameof(DumpPath);

        /// <summary>
        ///   查找类似 重复密码不能为空 的本地化字符串。
        /// </summary>
		public static string DuplicatePwdCannotBeEmpty = nameof(DuplicatePwdCannotBeEmpty);

        /// <summary>
        ///   查找类似 编辑 的本地化字符串。
        /// </summary>
		public static string Edit = nameof(Edit);

        /// <summary>
        ///   查找类似 编辑班别 的本地化字符串。
        /// </summary>
		public static string EditClass = nameof(EditClass);

        /// <summary>
        ///   查找类似 编辑特征 的本地化字符串。
        /// </summary>
		public static string EditFeature = nameof(EditFeature);

        /// <summary>
        ///   查找类似 编辑Plc报警 的本地化字符串。
        /// </summary>
		public static string EditPlcAlarm = nameof(EditPlcAlarm);

        /// <summary>
        ///   查找类似 编辑用户 的本地化字符串。
        /// </summary>
		public static string EditUser = nameof(EditUser);

        /// <summary>
        ///   查找类似 电缸 的本地化字符串。
        /// </summary>
		public static string EleCylinder = nameof(EleCylinder);

        /// <summary>
        ///   查找类似 自动压印 的本地化字符串。
        /// </summary>
		public static string Embossing = nameof(Embossing);

        /// <summary>
        ///   查找类似 急停 的本地化字符串。
        /// </summary>
		public static string EmergencyStop = nameof(EmergencyStop);

        /// <summary>
        ///   查找类似 空跑 的本地化字符串。
        /// </summary>
		public static string EmptyRun = nameof(EmptyRun);

        /// <summary>
        ///   查找类似 空跑模式 的本地化字符串。
        /// </summary>
		public static string EmptyRunMode = nameof(EmptyRunMode);

        /// <summary>
        ///   查找类似 是否启用 的本地化字符串。
        /// </summary>
		public static string Enable = nameof(Enable);

        /// <summary>
        ///   查找类似 启动蜂鸣器 的本地化字符串。
        /// </summary>
		public static string EnableBuzzer = nameof(EnableBuzzer);

        /// <summary>
        ///   查找类似 光栅启用 的本地化字符串。
        /// </summary>
		public static string EnableLightCurtain = nameof(EnableLightCurtain);

        /// <summary>
        ///   查找类似 启动监听 的本地化字符串。
        /// </summary>
		public static string EnableListening = nameof(EnableListening);

        /// <summary>
        ///   查找类似 安全门启用 的本地化字符串。
        /// </summary>
		public static string EnableSafetyDoor = nameof(EnableSafetyDoor);

        /// <summary>
        ///   查找类似 结束 的本地化字符串。
        /// </summary>
		public static string End = nameof(End);

        /// <summary>
        ///   查找类似 结束模块 的本地化字符串。
        /// </summary>
		public static string EndModule = nameof(EndModule);

        /// <summary>
        ///   查找类似 成品 的本地化字符串。
        /// </summary>
		public static string EndProduct = nameof(EndProduct);

        /// <summary>
        ///   查找类似 结束时间 的本地化字符串。
        /// </summary>
		public static string EndTime = nameof(EndTime);

        /// <summary>
        ///   查找类似 请输入站点名称 的本地化字符串。
        /// </summary>
		public static string EnterStationName = nameof(EnterStationName);

        /// <summary>
        ///   查找类似 爱普生机器人 的本地化字符串。
        /// </summary>
		public static string EpsonRobot = nameof(EpsonRobot);

        /// <summary>
        ///   查找类似 错误 的本地化字符串。
        /// </summary>
		public static string Error = nameof(Error);

        /// <summary>
        ///   查找类似 报错英文描述 的本地化字符串。
        /// </summary>
		public static string ErrorForeignMessage = nameof(ErrorForeignMessage);

        /// <summary>
        ///   查找类似 错误的图片路径 的本地化字符串。
        /// </summary>
		public static string ErrorImgPath = nameof(ErrorImgPath);

        /// <summary>
        ///   查找类似 非法的图片尺寸 的本地化字符串。
        /// </summary>
		public static string ErrorImgSize = nameof(ErrorImgSize);

        /// <summary>
        ///   查找类似 退出 的本地化字符串。
        /// </summary>
		public static string Exit = nameof(Exit);

        /// <summary>
        ///   查找类似 导出 的本地化字符串。
        /// </summary>
		public static string Export = nameof(Export);

        /// <summary>
        ///   查找类似 导出数据 的本地化字符串。
        /// </summary>
		public static string ExportData = nameof(ExportData);

        /// <summary>
        ///   查找类似 导出流程图 的本地化字符串。
        /// </summary>
		public static string ExportFlowTree = nameof(ExportFlowTree);

        /// <summary>
        ///   查找类似 导出图片 的本地化字符串。
        /// </summary>
		public static string ExportImage = nameof(ExportImage);

        /// <summary>
        ///   查找类似 导出工程 的本地化字符串。
        /// </summary>
		public static string ExportProject = nameof(ExportProject);

        /// <summary>
        ///   查找类似 导出配方 的本地化字符串。
        /// </summary>
		public static string ExportRecipe = nameof(ExportRecipe);

        /// <summary>
        ///   查找类似 Extract 的本地化字符串。
        /// </summary>
		public static string Extract = nameof(Extract);

        /// <summary>
        ///   查找类似 提取异步组 的本地化字符串。
        /// </summary>
		public static string ExtractAsyncGroup = nameof(ExtractAsyncGroup);

        /// <summary>
        ///   查找类似 提取分支组 的本地化字符串。
        /// </summary>
		public static string ExtractBranchGroup = nameof(ExtractBranchGroup);

        /// <summary>
        ///   查找类似 提取模块 的本地化字符串。
        /// </summary>
		public static string ExtractModule = nameof(ExtractModule);

        /// <summary>
        ///   查找类似 提取NG组 的本地化字符串。
        /// </summary>
		public static string ExtractNGGroup = nameof(ExtractNGGroup);

        /// <summary>
        ///   查找类似 提取分组 的本地化字符串。
        /// </summary>
		public static string ExtractStepGroup = nameof(ExtractStepGroup);

        /// <summary>
        ///   查找类似 提取分支组 的本地化字符串。
        /// </summary>
		public static string ExtractSwitchGroup = nameof(ExtractSwitchGroup);

        /// <summary>
        ///   查找类似 飞达 的本地化字符串。
        /// </summary>
		public static string Feeder = nameof(Feeder);

        /// <summary>
        ///   查找类似 供料站 的本地化字符串。
        /// </summary>
		public static string FeedStation = nameof(FeedStation);

        /// <summary>
        ///   查找类似 FFU 的本地化字符串。
        /// </summary>
		public static string FFU = nameof(FFU);

        /// <summary>
        ///   查找类似 FFU速度等级 的本地化字符串。
        /// </summary>
		public static string FFUSpeedLevel = nameof(FFUSpeedLevel);

        /// <summary>
        ///   查找类似 文件 的本地化字符串。
        /// </summary>
		public static string File = nameof(File);

        /// <summary>
        ///   查找类似 文件地址 的本地化字符串。
        /// </summary>
		public static string FileAddress = nameof(FileAddress);

        /// <summary>
        ///   查找类似 文件配置 的本地化字符串。
        /// </summary>
		public static string FileConfig = nameof(FileConfig);

        /// <summary>
        ///   查找类似 文件输入 的本地化字符串。
        /// </summary>
		public static string FileIO = nameof(FileIO);

        /// <summary>
        ///   查找类似 文件类型 的本地化字符串。
        /// </summary>
		public static string FileType = nameof(FileType);

        /// <summary>
        ///   查找类似 滤波 的本地化字符串。
        /// </summary>
		public static string Filtering = nameof(Filtering);

        /// <summary>
        ///   查找类似 最终结果 的本地化字符串。
        /// </summary>
		public static string FinalResult = nameof(FinalResult);

        /// <summary>
        ///   查找类似 查找 的本地化字符串。
        /// </summary>
		public static string Find = nameof(Find);

        /// <summary>
        ///   查找类似 找圆 的本地化字符串。
        /// </summary>
		public static string FindCircle = nameof(FindCircle);

        /// <summary>
        ///   查找类似 完 成 的本地化字符串。
        /// </summary>
		public static string Finish = nameof(Finish);

        /// <summary>
        ///   查找类似 首班 的本地化字符串。
        /// </summary>
		public static string FirstClass = nameof(FirstClass);

        /// <summary>
        ///   查找类似 首件指令 的本地化字符串。
        /// </summary>
		public static string FirstPieceModeCommand = nameof(FirstPieceModeCommand);

        /// <summary>
        ///   查找类似 首件状态 的本地化字符串。
        /// </summary>
		public static string FirstPieceModeStatus = nameof(FirstPieceModeStatus);

        /// <summary>
        ///   查找类似 首站 的本地化字符串。
        /// </summary>
		public static string FirstStation = nameof(FirstStation);

        /// <summary>
        ///   查找类似 治具号 的本地化字符串。
        /// </summary>
		public static string Fixture = nameof(Fixture);

        /// <summary>
        ///   查找类似 平面度 的本地化字符串。
        /// </summary>
		public static string Flatness = nameof(Flatness);

        /// <summary>
        ///   查找类似 抛料统计 的本地化字符串。
        /// </summary>
		public static string FlingMaterialStatistics = nameof(FlingMaterialStatistics);

        /// <summary>
        ///   查找类似 楼层 的本地化字符串。
        /// </summary>
		public static string Floor = nameof(Floor);

        /// <summary>
        ///   查找类似 流程 的本地化字符串。
        /// </summary>
		public static string Flow = nameof(Flow);

        /// <summary>
        ///   查找类似 流程等待 的本地化字符串。
        /// </summary>
		public static string FlowWait = nameof(FlowWait);

        /// <summary>
        ///   查找类似 飞拍模块 的本地化字符串。
        /// </summary>
		public static string FlyingPhoto = nameof(FlyingPhoto);

        /// <summary>
        ///   查找类似 力传感轴 的本地化字符串。
        /// </summary>
		public static string ForceAxis = nameof(ForceAxis);

        /// <summary>
        ///   查找类似 压力采集 的本地化字符串。
        /// </summary>
		public static string ForceCollect = nameof(ForceCollect);

        /// <summary>
        ///   查找类似 格式错误 的本地化字符串。
        /// </summary>
		public static string FormatError = nameof(FormatError);

        /// <summary>
        ///   查找类似 用于内存泄露检测 的本地化字符串。
        /// </summary>
		public static string ForMemoryLeakDetection = nameof(ForMemoryLeakDetection);

        /// <summary>
        ///   查找类似 空闲 的本地化字符串。
        /// </summary>
		public static string Free = nameof(Free);

        /// <summary>
        ///   查找类似 自由工站 的本地化字符串。
        /// </summary>
		public static string FreeStation = nameof(FreeStation);

        /// <summary>
        ///   查找类似 FTP上传 的本地化字符串。
        /// </summary>
		public static string FTPUpload = nameof(FTPUpload);

        /// <summary>
        ///   查找类似 功能模块 的本地化字符串。
        /// </summary>
		public static string FunctionalModule = nameof(FunctionalModule);

        /// <summary>
        ///   查找类似 功能启用 的本地化字符串。
        /// </summary>
		public static string FunctionEnable = nameof(FunctionEnable);

        /// <summary>
        ///   查找类似 功能部门 的本地化字符串。
        /// </summary>
		public static string FunctionId = nameof(FunctionId);

        /// <summary>
        ///   查找类似 功能管理 的本地化字符串。
        /// </summary>
		public static string FunctionManagement = nameof(FunctionManagement);

        /// <summary>
        ///   查找类似 治具绑定 的本地化字符串。
        /// </summary>
		public static string FX_BindCarrier = nameof(FX_BindCarrier);

        /// <summary>
        ///   查找类似 工单查询 的本地化字符串。
        /// </summary>
		public static string FX_OrderQuery = nameof(FX_OrderQuery);

        /// <summary>
        ///   查找类似 路由查询 的本地化字符串。
        /// </summary>
		public static string FX_RouteQuery = nameof(FX_RouteQuery);

        /// <summary>
        ///   查找类似 治具解绑 的本地化字符串。
        /// </summary>
		public static string FX_UnBindCarrier = nameof(FX_UnBindCarrier);

        /// <summary>
        ///   查找类似 NG结果上传 的本地化字符串。
        /// </summary>
		public static string FX_UploadResult = nameof(FX_UploadResult);

        /// <summary>
        ///   查找类似 FX首页 的本地化字符串。
        /// </summary>
		public static string FXContent = nameof(FXContent);

        /// <summary>
        ///   查找类似 数字孪生 的本地化字符串。
        /// </summary>
		public static string FXTCP = nameof(FXTCP);

        /// <summary>
        ///   查找类似 Gap 的本地化字符串。
        /// </summary>
		public static string Gap = nameof(Gap);

        /// <summary>
        ///   查找类似 齿轮比分子 的本地化字符串。
        /// </summary>
		public static string GearRatioNumerator = nameof(GearRatioNumerator);

        /// <summary>
        ///   查找类似 获取轴位置 的本地化字符串。
        /// </summary>
		public static string GenAxisPos = nameof(GenAxisPos);

        /// <summary>
        ///   查找类似 数据圆 的本地化字符串。
        /// </summary>
		public static string GenCircleByData = nameof(GenCircleByData);

        /// <summary>
        ///   查找类似 拟合圆 的本地化字符串。
        /// </summary>
		public static string GenCircleByFit = nameof(GenCircleByFit);

        /// <summary>
        ///   查找类似 多点构建点云 的本地化字符串。
        /// </summary>
		public static string GenCloudByPoints = nameof(GenCloudByPoints);

        /// <summary>
        ///   查找类似 圆锥拟合 的本地化字符串。
        /// </summary>
		public static string GenConeByFit = nameof(GenConeByFit);

        /// <summary>
        ///   查找类似 坐标系生成 的本地化字符串。
        /// </summary>
		public static string GenCoordByData = nameof(GenCoordByData);

        /// <summary>
        ///   查找类似 三面构建 的本地化字符串。
        /// </summary>
		public static string GenCoordByPlane3 = nameof(GenCoordByPlane3);

        /// <summary>
        ///   查找类似 数据长方体 的本地化字符串。
        /// </summary>
		public static string GenCuboidByData = nameof(GenCuboidByData);

        /// <summary>
        ///   查找类似 数据圆柱 的本地化字符串。
        /// </summary>
		public static string GenCylinderByData = nameof(GenCylinderByData);

        /// <summary>
        ///   查找类似 圆柱拟合 的本地化字符串。
        /// </summary>
		public static string GenCylinderByFit = nameof(GenCylinderByFit);

        /// <summary>
        ///   查找类似 生成 的本地化字符串。
        /// </summary>
		public static string Generate = nameof(Generate);

        /// <summary>
        ///   查找类似 生成方式 的本地化字符串。
        /// </summary>
		public static string GenerationMode = nameof(GenerationMode);

        /// <summary>
        ///   查找类似 两点构线 的本地化字符串。
        /// </summary>
		public static string GenLineBy2Point = nameof(GenLineBy2Point);

        /// <summary>
        ///   查找类似 求相交线 的本地化字符串。
        /// </summary>
		public static string GenLineByCross = nameof(GenLineByCross);

        /// <summary>
        ///   查找类似 数据线 的本地化字符串。
        /// </summary>
		public static string GenLineByData = nameof(GenLineByData);

        /// <summary>
        ///   查找类似 拟合直线 的本地化字符串。
        /// </summary>
		public static string GenLineByFit = nameof(GenLineByFit);

        /// <summary>
        ///   查找类似 线平移 的本地化字符串。
        /// </summary>
		public static string GenLineByMove = nameof(GenLineByMove);

        /// <summary>
        ///   查找类似 求投影线 的本地化字符串。
        /// </summary>
		public static string GenLineByProj = nameof(GenLineByProj);

        /// <summary>
        ///   查找类似 线变换 的本地化字符串。
        /// </summary>
		public static string GenLineByTransform = nameof(GenLineByTransform);

        /// <summary>
        ///   查找类似 三点构面 的本地化字符串。
        /// </summary>
		public static string GenPlaneBy3Point = nameof(GenPlaneBy3Point);

        /// <summary>
        ///   查找类似 拟合面 的本地化字符串。
        /// </summary>
		public static string GenPlaneByFit = nameof(GenPlaneByFit);

        /// <summary>
        ///   查找类似 面平移 的本地化字符串。
        /// </summary>
		public static string GenPlaneByMove = nameof(GenPlaneByMove);

        /// <summary>
        ///   查找类似 创建面 的本地化字符串。
        /// </summary>
		public static string GenPlaneByObj = nameof(GenPlaneByObj);

        /// <summary>
        ///   查找类似 生成点 的本地化字符串。
        /// </summary>
		public static string GenPoint = nameof(GenPoint);

        /// <summary>
        ///   查找类似 卡尺取点 的本地化字符串。
        /// </summary>
		public static string GenPointByCaliper = nameof(GenPointByCaliper);

        /// <summary>
        ///   查找类似 点云极值点 的本地化字符串。
        /// </summary>
		public static string GenPointByCloud = nameof(GenPointByCloud);

        /// <summary>
        ///   查找类似 CMM取点 的本地化字符串。
        /// </summary>
		public static string GenPointByCMM = nameof(GenPointByCMM);

        /// <summary>
        ///   查找类似 求交点 的本地化字符串。
        /// </summary>
		public static string GenPointByCross = nameof(GenPointByCross);

        /// <summary>
        ///   查找类似 数字点 的本地化字符串。
        /// </summary>
		public static string GenPointByData = nameof(GenPointByData);

        /// <summary>
        ///   查找类似 点偏移 的本地化字符串。
        /// </summary>
		public static string GenPointByMove = nameof(GenPointByMove);

        /// <summary>
        ///   查找类似 求投影点 的本地化字符串。
        /// </summary>
		public static string GenPointByProj = nameof(GenPointByProj);

        /// <summary>
        ///   查找类似 随机数 的本地化字符串。
        /// </summary>
		public static string GenRandomNumber = nameof(GenRandomNumber);

        /// <summary>
        ///   查找类似 生成报告 的本地化字符串。
        /// </summary>
		public static string GenReport = nameof(GenReport);

        /// <summary>
        ///   查找类似 数据球 的本地化字符串。
        /// </summary>
		public static string GenSphereByData = nameof(GenSphereByData);

        /// <summary>
        ///   查找类似 球体拟合 的本地化字符串。
        /// </summary>
		public static string GenSphereByFit = nameof(GenSphereByFit);

        /// <summary>
        ///   查找类似 字符创建 的本地化字符串。
        /// </summary>
		public static string GenString = nameof(GenString);

        /// <summary>
        ///   查找类似 几何特征 的本地化字符串。
        /// </summary>
		public static string GeometricFeatures = nameof(GeometricFeatures);

        /// <summary>
        ///   查找类似 几何 的本地化字符串。
        /// </summary>
		public static string Geometry = nameof(Geometry);

        /// <summary>
        ///   查找类似 平均值 的本地化字符串。
        /// </summary>
		public static string GetAverage = nameof(GetAverage);

        /// <summary>
        ///   查找类似 数据库获取 的本地化字符串。
        /// </summary>
		public static string GetByDataBase = nameof(GetByDataBase);

        /// <summary>
        ///   查找类似 获取方向 的本地化字符串。
        /// </summary>
		public static string GetDirectionByObj = nameof(GetDirectionByObj);

        /// <summary>
        ///   查找类似 获取信号 的本地化字符串。
        /// </summary>
		public static string GetIO = nameof(GetIO);

        /// <summary>
        ///   查找类似 获取线 的本地化字符串。
        /// </summary>
		public static string GetLineByObj = nameof(GetLineByObj);

        /// <summary>
        ///   查找类似 获取机台状态 的本地化字符串。
        /// </summary>
		public static string GetMachineStatus = nameof(GetMachineStatus);

        /// <summary>
        ///   查找类似 获取MBus 的本地化字符串。
        /// </summary>
		public static string GetModbus = nameof(GetModbus);

        /// <summary>
        ///   查找类似 获取面 的本地化字符串。
        /// </summary>
		public static string GetPlaneByObj = nameof(GetPlaneByObj);

        /// <summary>
        ///   查找类似 获取点 的本地化字符串。
        /// </summary>
		public static string GetPointByObj = nameof(GetPointByObj);

        /// <summary>
        ///   查找类似 获取线性KBR值 的本地化字符串。
        /// </summary>
		public static string GetSlopeIntercept = nameof(GetSlopeIntercept);

        /// <summary>
        ///   查找类似 全局 的本地化字符串。
        /// </summary>
		public static string Global = nameof(Global);

        /// <summary>
        ///   查找类似 全局变量 的本地化字符串。
        /// </summary>
		public static string GlobalVar = nameof(GlobalVar);

        /// <summary>
        ///   查找类似 全局变量 的本地化字符串。
        /// </summary>
		public static string GlobalVariable = nameof(GlobalVariable);

        /// <summary>
        ///   查找类似 跳转 的本地化字符串。
        /// </summary>
		public static string GoToModule = nameof(GoToModule);

        /// <summary>
        ///   查找类似 绿灯 的本地化字符串。
        /// </summary>
		public static string GreenLamp = nameof(GreenLamp);

        /// <summary>
        ///   查找类似 分组 的本地化字符串。
        /// </summary>
		public static string Group = nameof(Group);

        /// <summary>
        ///   查找类似 处理人 的本地化字符串。
        /// </summary>
		public static string HandledUser = nameof(HandledUser);

        /// <summary>
        ///   查找类似 处理方式 的本地化字符串。
        /// </summary>
		public static string HandlingMethod = nameof(HandlingMethod);

        /// <summary>
        ///   查找类似 硬件 的本地化字符串。
        /// </summary>
		public static string HardWare = nameof(HardWare);

        /// <summary>
        ///   查找类似 高度 的本地化字符串。
        /// </summary>
		public static string Height = nameof(Height);

        /// <summary>
        ///   查找类似 测高 的本地化字符串。
        /// </summary>
		public static string Heightfinder = nameof(Heightfinder);

        /// <summary>
        ///   查找类似 帮助 的本地化字符串。
        /// </summary>
		public static string Help = nameof(Help);

        /// <summary>
        ///   查找类似 隐藏标签 的本地化字符串。
        /// </summary>
		public static string HideLabel = nameof(HideLabel);

        /// <summary>
        ///   查找类似 高风速模式电流下限 的本地化字符串。
        /// </summary>
		public static string HighCurrentLowLimit = nameof(HighCurrentLowLimit);

        /// <summary>
        ///   查找类似 高风速模式电流上限 的本地化字符串。
        /// </summary>
		public static string HighCurrentUpperLimit = nameof(HighCurrentUpperLimit);

        /// <summary>
        ///   查找类似 高级权限时间 的本地化字符串。
        /// </summary>
		public static string HighLevelTime = nameof(HighLevelTime);

        /// <summary>
        ///   查找类似 HiveAppId 的本地化字符串。
        /// </summary>
		public static string HiveAppId = nameof(HiveAppId);

        /// <summary>
        ///   查找类似 Hive配置 的本地化字符串。
        /// </summary>
		public static string HiveConfig = nameof(HiveConfig);

        /// <summary>
        ///   查找类似 HiveCT 的本地化字符串。
        /// </summary>
		public static string HiveCT = nameof(HiveCT);

        /// <summary>
        ///   查找类似 忽略Hive反馈 的本地化字符串。
        /// </summary>
		public static string HiveIgnoreFeedback = nameof(HiveIgnoreFeedback);

        /// <summary>
        ///   查找类似 Hive阀门 的本地化字符串。
        /// </summary>
		public static string HiveValve = nameof(HiveValve);

        /// <summary>
        ///   查找类似 Holo3D 的本地化字符串。
        /// </summary>
		public static string Holo3D = nameof(Holo3D);

        /// <summary>
        ///   查找类似 主页 的本地化字符串。
        /// </summary>
		public static string Home = nameof(Home);

        /// <summary>
        ///   查找类似 回零完成 的本地化字符串。
        /// </summary>
		public static string HomeDone = nameof(HomeDone);

        /// <summary>
        ///   查找类似 回零站 的本地化字符串。
        /// </summary>
		public static string HomeStation = nameof(HomeStation);

        /// <summary>
        ///   查找类似 回零 的本地化字符串。
        /// </summary>
		public static string HomeZero = nameof(HomeZero);

        /// <summary>
        ///   查找类似 平台水平确认 的本地化字符串。
        /// </summary>
		public static string Horizontal = nameof(Horizontal);

        /// <summary>
        ///   查找类似 ICW 的本地化字符串。
        /// </summary>
		public static string ICW = nameof(ICW);

        /// <summary>
        ///   查找类似 空闲 的本地化字符串。
        /// </summary>
		public static string Idle = nameof(Idle);

        /// <summary>
        ///   查找类似 忽略 的本地化字符串。
        /// </summary>
		public static string Ignore = nameof(Ignore);

        /// <summary>
        ///   查找类似 图像 的本地化字符串。
        /// </summary>
		public static string Image = nameof(Image);

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public static string Import = nameof(Import);

        /// <summary>
        ///   查找类似 输入参数别名 的本地化字符串。
        /// </summary>
		public static string ImportParameterName = nameof(ImportParameterName);

        /// <summary>
        ///   查找类似 导入配方 的本地化字符串。
        /// </summary>
		public static string ImportRecipe = nameof(ImportRecipe);

        /// <summary>
        ///   查找类似 入站时间 的本地化字符串。
        /// </summary>
		public static string InboundTime = nameof(InboundTime);

        /// <summary>
        ///   查找类似 序号 的本地化字符串。
        /// </summary>
		public static string Index = nameof(Index);

        /// <summary>
        ///   查找类似 信息 的本地化字符串。
        /// </summary>
		public static string Info = nameof(Info);

        /// <summary>
        ///   查找类似 忽略 的本地化字符串。
        /// </summary>
		public static string Ingore = nameof(Ingore);

        /// <summary>
        ///   查找类似 初始化完成 的本地化字符串。
        /// </summary>
		public static string InitComplete = nameof(InitComplete);

        /// <summary>
        ///   查找类似 输入输出参数配置 的本地化字符串。
        /// </summary>
		public static string InOutParameterConfigure = nameof(InOutParameterConfigure);

        /// <summary>
        ///   查找类似 输入 的本地化字符串。
        /// </summary>
		public static string Input = nameof(Input);

        /// <summary>
        ///   查找类似 输入参数 的本地化字符串。
        /// </summary>
		public static string InputParameter = nameof(InputParameter);

        /// <summary>
        ///   查找类似 投入数 的本地化字符串。
        /// </summary>
		public static string InputQty = nameof(InputQty);

        /// <summary>
        ///   查找类似 插入 的本地化字符串。
        /// </summary>
		public static string Insert = nameof(Insert);

        /// <summary>
        ///   查找类似 插入点 的本地化字符串。
        /// </summary>
		public static string InsertPoint = nameof(InsertPoint);

        /// <summary>
        ///   查找类似 Insight名称 的本地化字符串。
        /// </summary>
		public static string InsightType = nameof(InsightType);

        /// <summary>
        ///   查找类似 软硬件调试 的本地化字符串。
        /// </summary>
		public static string IntegratedHardware = nameof(IntegratedHardware);

        /// <summary>
        ///   查找类似 间隔10分钟 的本地化字符串。
        /// </summary>
		public static string Interval10m = nameof(Interval10m);

        /// <summary>
        ///   查找类似 间隔1小时 的本地化字符串。
        /// </summary>
		public static string Interval1h = nameof(Interval1h);

        /// <summary>
        ///   查找类似 间隔1分钟 的本地化字符串。
        /// </summary>
		public static string Interval1m = nameof(Interval1m);

        /// <summary>
        ///   查找类似 间隔2小时 的本地化字符串。
        /// </summary>
		public static string Interval2h = nameof(Interval2h);

        /// <summary>
        ///   查找类似 间隔30分钟 的本地化字符串。
        /// </summary>
		public static string Interval30m = nameof(Interval30m);

        /// <summary>
        ///   查找类似 间隔30秒 的本地化字符串。
        /// </summary>
		public static string Interval30s = nameof(Interval30s);

        /// <summary>
        ///   查找类似 间隔5分钟 的本地化字符串。
        /// </summary>
		public static string Interval5m = nameof(Interval5m);

        /// <summary>
        ///   查找类似 I/O点检 的本地化字符串。
        /// </summary>
		public static string IOConform = nameof(IOConform);

        /// <summary>
        ///   查找类似 IO仿真 的本地化字符串。
        /// </summary>
		public static string IOSimulation = nameof(IOSimulation);

        /// <summary>
        ///   查找类似 IP地址 的本地化字符串。
        /// </summary>
		public static string IPAddress = nameof(IPAddress);

        /// <summary>
        ///   查找类似 是否确认移除当前工程 的本地化字符串。
        /// </summary>
		public static string IsDeleteCurrentProject = nameof(IsDeleteCurrentProject);

        /// <summary>
        ///   查找类似 回零参数重置 的本地化字符串。
        /// </summary>
		public static string IsHomeDefault = nameof(IsHomeDefault);

        /// <summary>
        ///   查找类似 参数记忆 的本地化字符串。
        /// </summary>
		public static string IsMemoric = nameof(IsMemoric);

        /// <summary>
        ///   查找类似 不能为空 的本地化字符串。
        /// </summary>
		public static string IsNecessary = nameof(IsNecessary);

        /// <summary>
        ///   查找类似 屏蔽安全门 的本地化字符串。
        /// </summary>
		public static string isShieldDoor = nameof(isShieldDoor);

        /// <summary>
        ///   查找类似 是否显示 的本地化字符串。
        /// </summary>
		public static string IsVisible = nameof(IsVisible);

        /// <summary>
        ///   查找类似 JOG 的本地化字符串。
        /// </summary>
		public static string JOG = nameof(JOG);

        /// <summary>
        ///   查找类似 按位拼接 的本地化字符串。
        /// </summary>
		public static string JoinBitToInt = nameof(JoinBitToInt);

        /// <summary>
        ///   查找类似 JSON解析 的本地化字符串。
        /// </summary>
		public static string JSONParse = nameof(JSONParse);

        /// <summary>
        ///   查找类似 判断 的本地化字符串。
        /// </summary>
		public static string Judge = nameof(Judge);

        /// <summary>
        ///   查找类似 字符判断 的本地化字符串。
        /// </summary>
		public static string JudgeString = nameof(JudgeString);

        /// <summary>
        ///   查找类似 跳转 的本地化字符串。
        /// </summary>
		public static string Jump = nameof(Jump);

        /// <summary>
        ///   查找类似 关键物料查询 的本地化字符串。
        /// </summary>
		public static string KeyMaterialQuery = nameof(KeyMaterialQuery);

        /// <summary>
        ///   查找类似 关键参数 的本地化字符串。
        /// </summary>
		public static string KeyParameters = nameof(KeyParameters);

        /// <summary>
        ///   查找类似 关键字匹配 的本地化字符串。
        /// </summary>
		public static string KeywordMatching = nameof(KeywordMatching);

        /// <summary>
        ///   查找类似 关键字 ： 的本地化字符串。
        /// </summary>
		public static string KeyWordWithSymbol = nameof(KeyWordWithSymbol);

        /// <summary>
        ///   查找类似 LAD Upload 的本地化字符串。
        /// </summary>
		public static string LADUpload = nameof(LADUpload);

        /// <summary>
        ///   查找类似 查找类似 {0} 的本地化字符串。 的本地化字符串。
        /// </summary>
		public static string LangComment = nameof(LangComment);

        /// <summary>
        ///   查找类似 线激光 的本地化字符串。
        /// </summary>
		public static string LaserScan = nameof(LaserScan);

        /// <summary>
        ///   查找类似 激光测距 的本地化字符串。
        /// </summary>
		public static string LaserSensor = nameof(LaserSensor);

        /// <summary>
        ///   查找类似 激光版本 的本地化字符串。
        /// </summary>
		public static string LaserVersion = nameof(LaserVersion);

        /// <summary>
        ///   查找类似 上次月保养时间 的本地化字符串。
        /// </summary>
		public static string LastMonthMaintenance = nameof(LastMonthMaintenance);

        /// <summary>
        ///   查找类似 尾站 的本地化字符串。
        /// </summary>
		public static string LastStation = nameof(LastStation);

        /// <summary>
        ///   查找类似 上次周保养时间 的本地化字符串。
        /// </summary>
		public static string LastWeekMaintenance = nameof(LastWeekMaintenance);

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public static string Lead = nameof(Lead);

        /// <summary>
        ///   查找类似 光源控制器 的本地化字符串。
        /// </summary>
		public static string LightController = nameof(LightController);

        /// <summary>
        ///   查找类似 光幕 的本地化字符串。
        /// </summary>
		public static string LightCurtain = nameof(LightCurtain);

        /// <summary>
        ///   查找类似 灯报警 的本地化字符串。
        /// </summary>
		public static string LightFlashing = nameof(LightFlashing);

        /// <summary>
        ///   查找类似 光源设置 的本地化字符串。
        /// </summary>
		public static string LightingSettings = nameof(LightingSettings);

        /// <summary>
        ///   查找类似 AELimits版本 的本地化字符串。
        /// </summary>
		public static string LimitsVersion = nameof(LimitsVersion);

        /// <summary>
        ///   查找类似 线 的本地化字符串。
        /// </summary>
		public static string Line = nameof(Line);

        /// <summary>
        ///   查找类似 线激光 的本地化字符串。
        /// </summary>
		public static string LineLaser = nameof(LineLaser);

        /// <summary>
        ///   查找类似 线体 的本地化字符串。
        /// </summary>
		public static string Liner = nameof(Liner);

        /// <summary>
        ///   查找类似 线延长比 的本地化字符串。
        /// </summary>
		public static string LineScale = nameof(LineScale);

        /// <summary>
        ///   查找类似 线扫 的本地化字符串。
        /// </summary>
		public static string LineScan = nameof(LineScan);

        /// <summary>
        ///   查找类似 线宽 的本地化字符串。
        /// </summary>
		public static string LineWidth = nameof(LineWidth);

        /// <summary>
        ///   查找类似 加载 的本地化字符串。
        /// </summary>
		public static string Load = nameof(Load);

        /// <summary>
        ///   查找类似 自动LoadCell 的本地化字符串。
        /// </summary>
		public static string LoadCell = nameof(LoadCell);

        /// <summary>
        ///   查找类似 运行中 的本地化字符串。
        /// </summary>
		public static string Loading = nameof(Loading);

        /// <summary>
        ///   查找类似 上料仓 的本地化字符串。
        /// </summary>
		public static string LoadingSilo = nameof(LoadingSilo);

        /// <summary>
        ///   查找类似 日志 的本地化字符串。
        /// </summary>
		public static string Log = nameof(Log);

        /// <summary>
        ///   查找类似 日志备份天数 的本地化字符串。
        /// </summary>
		public static string LogBackUpDays = nameof(LogBackUpDays);

        /// <summary>
        ///   查找类似 逻辑 的本地化字符串。
        /// </summary>
		public static string Logic = nameof(Logic);

        /// <summary>
        ///   查找类似 逻辑判断 的本地化字符串。
        /// </summary>
		public static string LogicCalculator = nameof(LogicCalculator);

        /// <summary>
        ///   查找类似 登录 的本地化字符串。
        /// </summary>
		public static string Login = nameof(Login);

        /// <summary>
        ///   查找类似 登录等级 的本地化字符串。
        /// </summary>
		public static string LoginLevel = nameof(LoginLevel);

        /// <summary>
        ///   查找类似 登录模式 的本地化字符串。
        /// </summary>
		public static string LoginMode = nameof(LoginMode);

        /// <summary>
        ///   查找类似 登录名 的本地化字符串。
        /// </summary>
		public static string LoginName = nameof(LoginName);

        /// <summary>
        ///   查找类似 登出 的本地化字符串。
        /// </summary>
		public static string Logout = nameof(Logout);

        /// <summary>
        ///   查找类似 循环 的本地化字符串。
        /// </summary>
		public static string Loop = nameof(Loop);

        /// <summary>
        ///   查找类似 低风速模式电流下限 的本地化字符串。
        /// </summary>
		public static string LowCurrentLowLimit = nameof(LowCurrentLowLimit);

        /// <summary>
        ///   查找类似 低风速模式电流上限 的本地化字符串。
        /// </summary>
		public static string LowCurrentUpperLimit = nameof(LowCurrentUpperLimit);

        /// <summary>
        ///   查找类似 公差下限 的本地化字符串。
        /// </summary>
		public static string LowerLimit = nameof(LowerLimit);

        /// <summary>
        ///   查找类似 MCH弹片测量数据上传 的本地化字符串。
        /// </summary>
		public static string LSMesUnLoad = nameof(LSMesUnLoad);

        /// <summary>
        ///   查找类似 排线SN管理 的本地化字符串。
        /// </summary>
		public static string CableSNManager = nameof(CableSNManager);

        /// <summary>
        ///   查找类似 智能驾驶舱 的本地化字符串。
        /// </summary>
		public static string LusterSmartCockpit = nameof(LusterSmartCockpit);

        /// <summary>
        ///   查找类似 Mac地址 的本地化字符串。
        /// </summary>
		public static string MacAddress = nameof(MacAddress);

        /// <summary>
        ///   查找类似 机种 的本地化字符串。
        /// </summary>
		public static string Machine = nameof(Machine);

        /// <summary>
        ///   查找类似 机台配置 的本地化字符串。
        /// </summary>
		public static string MachineConfigure = nameof(MachineConfigure);

        /// <summary>
        ///   查找类似 主流线治具数量 的本地化字符串。
        /// </summary>
		public static string MainCarrierNum = nameof(MainCarrierNum);

        /// <summary>
        ///   查找类似 参数导入确认 的本地化字符串。
        /// </summary>
		public static string MainParameters = nameof(MainParameters);

        /// <summary>
        ///   查找类似 保养 的本地化字符串。
        /// </summary>
		public static string Maintenance = nameof(Maintenance);

        /// <summary>
        ///   查找类似 Vision管理部门 的本地化字符串。
        /// </summary>
		public static string ManageDept_Vision = nameof(ManageDept_Vision);

        /// <summary>
        ///   查找类似 手动 的本地化字符串。
        /// </summary>
		public static string Manual = nameof(Manual);

        /// <summary>
        ///   查找类似 手动获取条码 的本地化字符串。
        /// </summary>
		public static string ManualGetBarcode = nameof(ManualGetBarcode);

        /// <summary>
        ///   查找类似 手动切换 的本地化字符串。
        /// </summary>
		public static string ManualSwitch = nameof(ManualSwitch);

        /// <summary>
        ///   查找类似 物料 的本地化字符串。
        /// </summary>
		public static string Material = nameof(Material);

        /// <summary>
        ///   查找类似 未获取到辅料名称 的本地化字符串。
        /// </summary>
		public static string MaterialNotObtained = nameof(MaterialNotObtained);

        /// <summary>
        ///   查找类似 每页数量 的本地化字符串。
        /// </summary>
		public static string MaxPerPage = nameof(MaxPerPage);

        /// <summary>
        ///   查找类似 点合并 的本地化字符串。
        /// </summary>
		public static string MergePoints = nameof(MergePoints);

        /// <summary>
        ///   查找类似 网格数据 的本地化字符串。
        /// </summary>
		public static string Mesh = nameof(Mesh);

        /// <summary>
        ///   查找类似 中风速模式电流下限 的本地化字符串。
        /// </summary>
		public static string MiddleCurrentLowLimit = nameof(MiddleCurrentLowLimit);

        /// <summary>
        ///   查找类似 中风速模式电流上限 的本地化字符串。
        /// </summary>
		public static string MiddleCurrentUpperLimit = nameof(MiddleCurrentUpperLimit);

        /// <summary>
        ///   查找类似 杂项 的本地化字符串。
        /// </summary>
		public static string Miscellaneous = nameof(Miscellaneous);

        /// <summary>
        ///   查找类似 ModbusRTU 的本地化字符串。
        /// </summary>
		public static string ModbusRTU = nameof(ModbusRTU);

        /// <summary>
        ///   查找类似 模式 的本地化字符串。
        /// </summary>
		public static string Model = nameof(Model);

        /// <summary>
        ///   查找类似 模型显示模式 的本地化字符串。
        /// </summary>
		public static string ModelDisplayMode = nameof(ModelDisplayMode);

        /// <summary>
        ///   查找类似 模型点集 的本地化字符串。
        /// </summary>
		public static string ModelGroup = nameof(ModelGroup);

        /// <summary>
        ///   查找类似 模块 的本地化字符串。
        /// </summary>
		public static string Module = nameof(Module);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string Module_Name = nameof(Module_Name);

        /// <summary>
        ///   查找类似 报警信息 的本地化字符串。
        /// </summary>
		public static string ModuleError = nameof(ModuleError);

        /// <summary>
        ///   查找类似 模块名称 的本地化字符串。
        /// </summary>
		public static string ModuleName = nameof(ModuleName);

        /// <summary>
        ///   查找类似 模块设置 的本地化字符串。
        /// </summary>
		public static string ModuleSet = nameof(ModuleSet);

        /// <summary>
        ///   查找类似 月 的本地化字符串。
        /// </summary>
		public static string Month = nameof(Month);

        /// <summary>
        ///   查找类似 形态学 的本地化字符串。
        /// </summary>
		public static string Morphological = nameof(Morphological);

        /// <summary>
        ///   查找类似 运动 的本地化字符串。
        /// </summary>
		public static string Motion = nameof(Motion);

        /// <summary>
        ///   查找类似 控制卡 的本地化字符串。
        /// </summary>
		public static string MotionCard = nameof(MotionCard);

        /// <summary>
        ///   查找类似 多场景下，每个轴的运动优先级 的本地化字符串。
        /// </summary>
		public static string MotionPriorityOfEachAxisInMultipleScenes = nameof(MotionPriorityOfEachAxisInMultipleScenes);

        /// <summary>
        ///   查找类似 运动速度 的本地化字符串。
        /// </summary>
		public static string MotionSpeed = nameof(MotionSpeed);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string MotionSpeed_mm_s_ = nameof(MotionSpeed_mm_s_);

        /// <summary>
        ///   查找类似 运动速度，单位mm 的本地化字符串。
        /// </summary>
		public static string MotionSpeedWithUnit = nameof(MotionSpeedWithUnit);

        /// <summary>
        ///   查找类似 运动方向 的本地化字符串。
        /// </summary>
		public static string MoveDirection = nameof(MoveDirection);

        /// <summary>
        ///   查找类似 移动位置 的本地化字符串。
        /// </summary>
		public static string MovePosition = nameof(MovePosition);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string MovePostion_mm_ = nameof(MovePostion_mm_);

        /// <summary>
        ///   查找类似 移动到 的本地化字符串。
        /// </summary>
		public static string MoveTo = nameof(MoveTo);

        /// <summary>
        ///   查找类似 多轴 的本地化字符串。
        /// </summary>
		public static string MultiAxis = nameof(MultiAxis);

        /// <summary>
        ///   查找类似 名称 的本地化字符串。
        /// </summary>
		public static string Name = nameof(Name);

        /// <summary>
        ///   查找类似 新保压 的本地化字符串。
        /// </summary>
		public static string NewPressurize = nameof(NewPressurize);

        /// <summary>
        ///   查找类似 新增工程 的本地化字符串。
        /// </summary>
		public static string NewProject = nameof(NewProject);

        /// <summary>
        ///   查找类似 新增配方 的本地化字符串。
        /// </summary>
		public static string NewRecipe = nameof(NewRecipe);

        /// <summary>
        ///   查找类似 下游要料 的本地化字符串。
        /// </summary>
		public static string NextGet = nameof(NextGet);

        /// <summary>
        ///   查找类似 下一页 的本地化字符串。
        /// </summary>
		public static string NextPage = nameof(NextPage);

        /// <summary>
        ///   查找类似 NG模块 的本地化字符串。
        /// </summary>
		public static string NG = nameof(NG);

        /// <summary>
        ///   查找类似 NG数 的本地化字符串。
        /// </summary>
		public static string NGAmount = nameof(NGAmount);

        /// <summary>
        ///   查找类似 NG模组 的本地化字符串。
        /// </summary>
		public static string NGGroup = nameof(NGGroup);

        /// <summary>
        ///   查找类似 NG率 的本地化字符串。
        /// </summary>
		public static string NGRate = nameof(NGRate);

        /// <summary>
        ///   查找类似 NG原因 的本地化字符串。
        /// </summary>
		public static string NGReason = nameof(NGReason);

        /// <summary>
        ///   查找类似 NG工站 的本地化字符串。
        /// </summary>
		public static string NGStation = nameof(NGStation);

        /// <summary>
        ///   查找类似 否 的本地化字符串。
        /// </summary>
		public static string No = nameof(No);

        /// <summary>
        ///   查找类似 暂无数据 的本地化字符串。
        /// </summary>
		public static string NoData = nameof(NoData);

        /// <summary>
        ///   查找类似 未找到对应匹配的设备 的本地化字符串。
        /// </summary>
		public static string NoMatchDeviceFound = nameof(NoMatchDeviceFound);

        /// <summary>
        ///   查找类似 工程下没有配方 的本地化字符串。
        /// </summary>
		public static string NoRecipeInProject = nameof(NoRecipeInProject);

        /// <summary>
        ///   查找类似 未找到激活的配方的路径 的本地化字符串。
        /// </summary>
		public static string NotFoundActiveRecipePath = nameof(NotFoundActiveRecipePath);

        /// <summary>
        ///   查找类似 空字符 的本地化字符串。
        /// </summary>
		public static string Null = nameof(Null);

        /// <summary>
        ///   查找类似 循环次数 的本地化字符串。
        /// </summary>
		public static string NumberOfCycles = nameof(NumberOfCycles);

        /// <summary>
        ///   查找类似 获取软件版本 的本地化字符串。
        /// </summary>
		public static string ObtainSwVersion = nameof(ObtainSwVersion);

        /// <summary>
        ///   查找类似 离线模式 的本地化字符串。
        /// </summary>
		public static string OffLineMode = nameof(OffLineMode);

        /// <summary>
        ///   查找类似 OK数 的本地化字符串。
        /// </summary>
		public static string OKAmount = nameof(OKAmount);

        /// <summary>
        ///   查找类似 OK率 的本地化字符串。
        /// </summary>
		public static string OKRate = nameof(OKRate);

        /// <summary>
        ///   查找类似 在线模式 的本地化字符串。
        /// </summary>
		public static string OnLineMode = nameof(OnLineMode);

        /// <summary>
        ///   查找类似 透明度 的本地化字符串。
        /// </summary>
		public static string Opacity = nameof(Opacity);

        /// <summary>
        ///   查找类似 打开 的本地化字符串。
        /// </summary>
		public static string Open = nameof(Open);

        /// <summary>
        ///   查找类似 开关门 的本地化字符串。
        /// </summary>
		public static string OpenCloseDoor = nameof(OpenCloseDoor);

        /// <summary>
        ///   查找类似 打开项目 的本地化字符串。
        /// </summary>
		public static string OpenProject = nameof(OpenProject);

        /// <summary>
        ///   查找类似 开启提示 的本地化字符串。
        /// </summary>
		public static string OPenPrompt = nameof(OPenPrompt);

        /// <summary>
        ///   查找类似 操作 的本地化字符串。
        /// </summary>
		public static string Operate = nameof(Operate);

        /// <summary>
        ///   查找类似 操作时间 的本地化字符串。
        /// </summary>
		public static string OperateTime = nameof(OperateTime);

        /// <summary>
        ///   查找类似 操纵类型 的本地化字符串。
        /// </summary>
		public static string OperateType = nameof(OperateType);

        /// <summary>
        ///   查找类似 操作提示 的本地化字符串。
        /// </summary>
		public static string OperatingTips = nameof(OperatingTips);

        /// <summary>
        ///   查找类似 操作类型 的本地化字符串。
        /// </summary>
		public static string OperationType = nameof(OperationType);

        /// <summary>
        ///   查找类似 工单 的本地化字符串。
        /// </summary>
		public static string Order = nameof(Order);

        /// <summary>
        ///   查找类似 原始密码 的本地化字符串。
        /// </summary>
		public static string OriginalPassword = nameof(OriginalPassword);

        /// <summary>
        ///   查找类似 原密码输入错误 的本地化字符串。
        /// </summary>
		public static string OriginalPassWordWrong = nameof(OriginalPassWordWrong);

        /// <summary>
        ///   查找类似 原点限位 的本地化字符串。
        /// </summary>
		public static string OriginLimit = nameof(OriginLimit);

        /// <summary>
        ///   查找类似 其他 的本地化字符串。
        /// </summary>
		public static string Others = nameof(Others);

        /// <summary>
        ///   查找类似 出站时间 的本地化字符串。
        /// </summary>
		public static string OutBoundTime = nameof(OutBoundTime);

        /// <summary>
        ///   查找类似 输出IO 的本地化字符串。
        /// </summary>
		public static string OutIO = nameof(OutIO);

        /// <summary>
        ///   查找类似 局外点拟合 的本地化字符串。
        /// </summary>
		public static string OutlierFit = nameof(OutlierFit);

        /// <summary>
        ///   查找类似 不在范围内 的本地化字符串。
        /// </summary>
		public static string OutOfRange = nameof(OutOfRange);

        /// <summary>
        ///   查找类似 输出参数 的本地化字符串。
        /// </summary>
		public static string OutParam = nameof(OutParam);

        /// <summary>
        ///   查找类似 输出参数别名 的本地化字符串。
        /// </summary>
		public static string OutportParameterName = nameof(OutportParameterName);

        /// <summary>
        ///   查找类似 输出 的本地化字符串。
        /// </summary>
		public static string Output = nameof(Output);

        /// <summary>
        ///   查找类似 输出项 的本地化字符串。
        /// </summary>
		public static string OutPutItem = nameof(OutPutItem);

        /// <summary>
        ///   查找类似 输出项设置未保存，请先点击保存 的本地化字符串。
        /// </summary>
		public static string OutPutItemUnSave = nameof(OutPutItemUnSave);

        /// <summary>
        ///   查找类似 输出参数 的本地化字符串。
        /// </summary>
		public static string OutputParameter = nameof(OutputParameter);

        /// <summary>
        ///   查找类似 页面控制 的本地化字符串。
        /// </summary>
		public static string PageControl = nameof(PageControl);

        /// <summary>
        ///   查找类似 页面模式 的本地化字符串。
        /// </summary>
		public static string PageMode = nameof(PageMode);

        /// <summary>
        ///   查找类似 并行 的本地化字符串。
        /// </summary>
		public static string Parallel = nameof(Parallel);

        /// <summary>
        ///   查找类似 平行度 的本地化字符串。
        /// </summary>
		public static string Parallelism = nameof(Parallelism);

        /// <summary>
        ///   查找类似 参数 的本地化字符串。
        /// </summary>
		public static string Parameter = nameof(Parameter);

        /// <summary>
        ///   查找类似 参数配置 的本地化字符串。
        /// </summary>
		public static string ParameterConfig = nameof(ParameterConfig);

        /// <summary>
        ///   查找类似 参数配置 的本地化字符串。
        /// </summary>
		public static string ParameterConfigure = nameof(ParameterConfigure);

        /// <summary>
        ///   查找类似 字符解析 的本地化字符串。
        /// </summary>
		public static string ParseString = nameof(ParseString);

        /// <summary>
        ///   查找类似 卷料批次名称 的本地化字符串。
        /// </summary>
		public static string PartName = nameof(PartName);

        /// <summary>
        ///   查找类似 密码 的本地化字符串。
        /// </summary>
		public static string PassWord = nameof(PassWord);

        /// <summary>
        ///   查找类似 密码错误 的本地化字符串。
        /// </summary>
		public static string PassWordError = nameof(PassWordError);

        /// <summary>
        ///   查找类似 粘贴 的本地化字符串。
        /// </summary>
		public static string Paste = nameof(Paste);

        /// <summary>
        ///   查找类似 暂停 的本地化字符串。
        /// </summary>
		public static string Pause = nameof(Pause);

        /// <summary>
        ///   查找类似 暂停中 的本地化字符串。
        /// </summary>
		public static string Paused = nameof(Paused);

        /// <summary>
        ///   查找类似 暂停灯 的本地化字符串。
        /// </summary>
		public static string PauseLamp = nameof(PauseLamp);

        /// <summary>
        ///   查找类似 PC心跳 的本地化字符串。
        /// </summary>
		public static string PCHeartbeat = nameof(PCHeartbeat);

        /// <summary>
        ///   查找类似 PC相关 的本地化字符串。
        /// </summary>
		public static string PCRelevant = nameof(PCRelevant);

        /// <summary>
        ///   查找类似 PC状态 的本地化字符串。
        /// </summary>
		public static string PCStatus = nameof(PCStatus);

        /// <summary>
        ///   查找类似 PDCA 的本地化字符串。
        /// </summary>
		public static string PDCA = nameof(PDCA);

        /// <summary>
        ///   查找类似 AE上传 的本地化字符串。
        /// </summary>
		public static string PDCAELimit = nameof(PDCAELimit);

        /// <summary>
        ///   查找类似 AELimt上传 的本地化字符串。
        /// </summary>
		public static string PDCAELimt = nameof(PDCAELimt);

        /// <summary>
        ///   查找类似 PDCA数据失败补传 的本地化字符串。
        /// </summary>
		public static string PDCAFailRetry = nameof(PDCAFailRetry);

        /// <summary>
        ///   查找类似 PDCA业务 的本地化字符串。
        /// </summary>
		public static string PDCAFlow = nameof(PDCAFlow);

        /// <summary>
        ///   查找类似 PDCAWIP 的本地化字符串。
        /// </summary>
		public static string PDCAWIP = nameof(PDCAWIP);

        /// <summary>
        ///   查找类似 PDO读写 的本地化字符串。
        /// </summary>
		public static string PDOAction = nameof(PDOAction);

        /// <summary>
        ///   查找类似 拾取线 的本地化字符串。
        /// </summary>
		public static string PickLine = nameof(PickLine);

        /// <summary>
        ///   查找类似 拾取面 的本地化字符串。
        /// </summary>
		public static string PickPlane = nameof(PickPlane);

        /// <summary>
        ///   查找类似 拾取点 的本地化字符串。
        /// </summary>
		public static string PickPoint = nameof(PickPoint);

        /// <summary>
        ///   查找类似 图片存储路径 的本地化字符串。
        /// </summary>
		public static string PictureStoragePath = nameof(PictureStoragePath);

        /// <summary>
        ///   查找类似 面 的本地化字符串。
        /// </summary>
		public static string Plane = nameof(Plane);

        /// <summary>
        ///   查找类似 厂区 的本地化字符串。
        /// </summary>
		public static string PlantArea = nameof(PlantArea);

        /// <summary>
        ///   查找类似 PLC 的本地化字符串。
        /// </summary>
		public static string PLC = nameof(PLC);

        /// <summary>
        ///   查找类似 PLC地址 的本地化字符串。
        /// </summary>
		public static string PLCAddress = nameof(PLCAddress);

        /// <summary>
        ///   查找类似 PLC清错 的本地化字符串。
        /// </summary>
		public static string PLCClearMistake = nameof(PLCClearMistake);

        /// <summary>
        ///   查找类似 PLC配置 的本地化字符串。
        /// </summary>
		public static string PLCConfigure = nameof(PLCConfigure);

        /// <summary>
        ///   查找类似 PLC服务器 的本地化字符串。
        /// </summary>
		public static string PLCServer = nameof(PLCServer);

        /// <summary>
        ///   查找类似 Plc工站 的本地化字符串。
        /// </summary>
		public static string PlcStation = nameof(PlcStation);

        /// <summary>
        ///   查找类似 PLC状态 的本地化字符串。
        /// </summary>
		public static string PLCStatus = nameof(PLCStatus);

        /// <summary>
        ///   查找类似 Plc版本 的本地化字符串。
        /// </summary>
		public static string PlcVersion = nameof(PlcVersion);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string Please_Enter_AlarmCode = nameof(Please_Enter_AlarmCode);

        /// <summary>
        ///   查找类似 请检查数据格式或类型！ 的本地化字符串。
        /// </summary>
		public static string PleaseCheckTheDataFormatorType = nameof(PleaseCheckTheDataFormatorType);

        /// <summary>
        ///   查找类似 请输入大于零的整数！ 的本地化字符串。
        /// </summary>
		public static string PleaseEnterAnIntegerGreaterThan0 = nameof(PleaseEnterAnIntegerGreaterThan0);

        /// <summary>
        ///   查找类似 请输入单字节分割符！ 的本地化字符串。
        /// </summary>
		public static string PleaseEnterASingleByteDelimiter = nameof(PleaseEnterASingleByteDelimiter);

        /// <summary>
        ///   查找类似 请输入条件 的本地化字符串。
        /// </summary>
		public static string PleaseEnterConditions = nameof(PleaseEnterConditions);

        /// <summary>
        ///   查找类似 请输入SN编码 的本地化字符串。
        /// </summary>
		public static string PleaseEnterSNCode = nameof(PleaseEnterSNCode);

        /// <summary>
        ///   查找类似 下午 的本地化字符串。
        /// </summary>
		public static string Pm = nameof(Pm);

        /// <summary>
        ///   查找类似 PNG图片 的本地化字符串。
        /// </summary>
		public static string PngImg = nameof(PngImg);

        /// <summary>
        ///   查找类似 点 的本地化字符串。
        /// </summary>
		public static string Point = nameof(Point);

        /// <summary>
        ///   查找类似 两点距离 的本地化字符串。
        /// </summary>
		public static string Point2Point = nameof(Point2Point);

        /// <summary>
        ///   查找类似 点云 的本地化字符串。
        /// </summary>
		public static string PointCloud = nameof(PointCloud);

        /// <summary>
        ///   查找类似 点云显示模式 的本地化字符串。
        /// </summary>
		public static string PointCloudDisplayMode = nameof(PointCloudDisplayMode);

        /// <summary>
        ///   查找类似 点云点集 的本地化字符串。
        /// </summary>
		public static string PointCloudGroup = nameof(PointCloudGroup);

        /// <summary>
        ///   查找类似 点云尺寸 的本地化字符串。
        /// </summary>
		public static string PointCloudSize = nameof(PointCloudSize);

        /// <summary>
        ///   查找类似 指针坐标 的本地化字符串。
        /// </summary>
		public static string PointerCoord = nameof(PointerCoord);

        /// <summary>
        ///   查找类似 点尺寸 的本地化字符串。
        /// </summary>
		public static string PointSize = nameof(PointSize);

        /// <summary>
        ///   查找类似 点位示教 的本地化字符串。
        /// </summary>
		public static string PointTeaching = nameof(PointTeaching);

        /// <summary>
        ///   查找类似 位置输出 的本地化字符串。
        /// </summary>
		public static string PositionOutput = nameof(PositionOutput);

        /// <summary>
        ///   查找类似 点位 的本地化字符串。
        /// </summary>
		public static string PosLocation = nameof(PosLocation);

        /// <summary>
        ///   查找类似 压力曲线 的本地化字符串。
        /// </summary>
		public static string PressDriver = nameof(PressDriver);

        /// <summary>
        ///   查找类似 压力1 的本地化字符串。
        /// </summary>
		public static string PressForm1 = nameof(PressForm1);

        /// <summary>
        ///   查找类似 压力2 的本地化字符串。
        /// </summary>
		public static string PressForm2 = nameof(PressForm2);

        /// <summary>
        ///   查找类似 压力3 的本地化字符串。
        /// </summary>
		public static string PressForm3 = nameof(PressForm3);

        /// <summary>
        ///   查找类似 压力4 的本地化字符串。
        /// </summary>
		public static string PressForm4 = nameof(PressForm4);

        /// <summary>
        ///   查找类似 压力5 的本地化字符串。
        /// </summary>
		public static string PressForm5 = nameof(PressForm5);

        /// <summary>
        ///   查找类似 压力重复性 的本地化字符串。
        /// </summary>
		public static string PressureRepetition = nameof(PressureRepetition);

        /// <summary>
        ///   查找类似 压力传感器 的本地化字符串。
        /// </summary>
		public static string PressureSensor = nameof(PressureSensor);

        /// <summary>
        ///   查找类似 保压 的本地化字符串。
        /// </summary>
		public static string Pressurize = nameof(Pressurize);

        /// <summary>
        ///   查找类似 上游有料 的本地化字符串。
        /// </summary>
		public static string PrevHave = nameof(PrevHave);

        /// <summary>
        ///   查找类似 预览 的本地化字符串。
        /// </summary>
		public static string Preview = nameof(Preview);

        /// <summary>
        ///   查找类似 上一页 的本地化字符串。
        /// </summary>
		public static string PreviousPage = nameof(PreviousPage);

        /// <summary>
        ///   查找类似 打印 的本地化字符串。
        /// </summary>
		public static string Print = nameof(Print);

        /// <summary>
        ///   查找类似 打印机 的本地化字符串。
        /// </summary>
		public static string Printer = nameof(Printer);

        /// <summary>
        ///   查找类似 打印预览 的本地化字符串。
        /// </summary>
		public static string PrintPreview = nameof(PrintPreview);

        /// <summary>
        ///   查找类似 打印设置 的本地化字符串。
        /// </summary>
		public static string PrintSet = nameof(PrintSet);

        /// <summary>
        ///   查找类似 优先级 的本地化字符串。
        /// </summary>
		public static string Priority = nameof(Priority);

        /// <summary>
        ///   查找类似 Vision机种 的本地化字符串。
        /// </summary>
		public static string Product_Vision = nameof(Product_Vision);

        /// <summary>
        ///   查找类似 产品数 的本地化字符串。
        /// </summary>
		public static string ProductAmount = nameof(ProductAmount);

        /// <summary>
        ///   查找类似 产品信息 的本地化字符串。
        /// </summary>
		public static string ProductInfo = nameof(ProductInfo);

        /// <summary>
        ///   查找类似 产品NG 的本地化字符串。
        /// </summary>
		public static string ProductNG = nameof(ProductNG);

        /// <summary>
        ///   查找类似 产品统计 的本地化字符串。
        /// </summary>
		public static string ProductStatistics = nameof(ProductStatistics);

        /// <summary>
        ///   查找类似 产品事件 的本地化字符串。
        /// </summary>
		public static string ProEvent = nameof(ProEvent);

        /// <summary>
        ///   查找类似 面轮廓度 的本地化字符串。
        /// </summary>
		public static string Profileanysurface = nameof(Profileanysurface);

        /// <summary>
        ///   查找类似 The Program Must be Stopped to Close 的本地化字符串。
        /// </summary>
		public static string ProgramMustStop = nameof(ProgramMustStop);

        /// <summary>
        ///   查找类似 程序停止 的本地化字符串。
        /// </summary>
		public static string ProgramStop = nameof(ProgramStop);

        /// <summary>
        ///   查找类似 工程 的本地化字符串。
        /// </summary>
		public static string Project = nameof(Project);

        /// <summary>
        ///   查找类似 项 目 地 址 的本地化字符串。
        /// </summary>
		public static string ProjectAddress = nameof(ProjectAddress);

        /// <summary>
        ///   查找类似 项目已存在！ 的本地化字符串。
        /// </summary>
		public static string ProjectAlreadyExists = nameof(ProjectAlreadyExists);

        /// <summary>
        ///   查找类似 项目名或地址不可为空！ 的本地化字符串。
        /// </summary>
		public static string ProjectCannotBeEmpty = nameof(ProjectCannotBeEmpty);

        /// <summary>
        ///   查找类似 工程文件错误，无法添加 的本地化字符串。
        /// </summary>
		public static string ProjectFileError = nameof(ProjectFileError);

        /// <summary>
        ///   查找类似 项 目 名 称 的本地化字符串。
        /// </summary>
		public static string ProjectName = nameof(ProjectName);

        /// <summary>
        ///   查找类似 项目属性 的本地化字符串。
        /// </summary>
		public static string ProjectProperty = nameof(ProjectProperty);

        /// <summary>
        ///   查找类似 入料通知 的本地化字符串。
        /// </summary>
		public static string ProLoaded = nameof(ProLoaded);

        /// <summary>
        ///   查找类似 提示内容 的本地化字符串。
        /// </summary>
		public static string PromptContent = nameof(PromptContent);

        /// <summary>
        ///   查找类似 属性 的本地化字符串。
        /// </summary>
		public static string Property = nameof(Property);

        /// <summary>
        ///   查找类似 协议 的本地化字符串。
        /// </summary>
		public static string Protocol = nameof(Protocol);

        /// <summary>
        ///   查找类似 脉冲比计算 的本地化字符串。
        /// </summary>
		public static string PulseCal = nameof(PulseCal);

        /// <summary>
        ///   查找类似 密码不能为空 的本地化字符串。
        /// </summary>
		public static string PwdCannotBeEmpty = nameof(PwdCannotBeEmpty);

        /// <summary>
        ///   查找类似 密码输入不一致，请重新输入 的本地化字符串。
        /// </summary>
		public static string PwdInconsistent = nameof(PwdInconsistent);

        /// <summary>
        ///   查找类似 查询 的本地化字符串。
        /// </summary>
		public static string Query = nameof(Query);

        /// <summary>
        ///   查找类似 退出 的本地化字符串。
        /// </summary>
		public static string Quit = nameof(Quit);

        /// <summary>
        ///   查找类似 是否退出软件? 的本地化字符串。
        /// </summary>
		public static string QuitSoftWare = nameof(QuitSoftWare);

        /// <summary>
        ///   查找类似 R_加速时间_Target 的本地化字符串。
        /// </summary>
		public static string R_Acceleration_Target = nameof(R_Acceleration_Target);

        /// <summary>
        ///   查找类似 R_加速时间_Actual 的本地化字符串。
        /// </summary>
		public static string R_AccelerationTime_Actual = nameof(R_AccelerationTime_Actual);

        /// <summary>
        ///   查找类似 R_速度_Actual 的本地化字符串。
        /// </summary>
		public static string R_Speed_Actual = nameof(R_Speed_Actual);

        /// <summary>
        ///   查找类似 R_速度_Target 的本地化字符串。
        /// </summary>
		public static string R_Speed_Target = nameof(R_Speed_Target);

        /// <summary>
        ///   查找类似 加载CAD 的本地化字符串。
        /// </summary>
		public static string ReadCAD = nameof(ReadCAD);

        /// <summary>
        ///   查找类似 加载点云 的本地化字符串。
        /// </summary>
		public static string ReadCloud = nameof(ReadCloud);

        /// <summary>
        ///   查找类似 数据文件 的本地化字符串。
        /// </summary>
		public static string ReadDataFile = nameof(ReadDataFile);

        /// <summary>
        ///   查找类似 读入数据 的本地化字符串。
        /// </summary>
		public static string ReadDatas = nameof(ReadDatas);

        /// <summary>
        ///   查找类似 读取Fins 的本地化字符串。
        /// </summary>
		public static string ReadFins = nameof(ReadFins);

        /// <summary>
        ///   查找类似 加载矩阵 的本地化字符串。
        /// </summary>
		public static string ReadMatrix = nameof(ReadMatrix);

        /// <summary>
        ///   查找类似 读取MC 的本地化字符串。
        /// </summary>
		public static string ReadMC = nameof(ReadMC);

        /// <summary>
        ///   查找类似 读取Modbus 的本地化字符串。
        /// </summary>
		public static string ReadModbus = nameof(ReadModbus);

        /// <summary>
        ///   查找类似 读取PLC 的本地化字符串。
        /// </summary>
		public static string ReadPlc = nameof(ReadPlc);

        /// <summary>
        ///   查找类似 读取机械手速度 的本地化字符串。
        /// </summary>
		public static string ReadRobotSpeed = nameof(ReadRobotSpeed);

        /// <summary>
        ///   查找类似 加载STL 的本地化字符串。
        /// </summary>
		public static string ReadSTL = nameof(ReadSTL);

        /// <summary>
        ///   查找类似 实时 的本地化字符串。
        /// </summary>
		public static string RealTime = nameof(RealTime);

        /// <summary>
        ///   查找类似 实时位置 的本地化字符串。
        /// </summary>
		public static string RealTimeLocation = nameof(RealTimeLocation);

        /// <summary>
        ///   查找类似 原因 的本地化字符串。
        /// </summary>
		public static string Reason = nameof(Reason);

        /// <summary>
        ///   查找类似 最近文件 的本地化字符串。
        /// </summary>
		public static string RecentFile = nameof(RecentFile);

        /// <summary>
        ///   查找类似 最近项目 的本地化字符串。
        /// </summary>
		public static string RecentProject = nameof(RecentProject);

        /// <summary>
        ///   查找类似 复检 的本地化字符串。
        /// </summary>
		public static string ReCheck = nameof(ReCheck);

        /// <summary>
        ///   查找类似 配方版本 的本地化字符串。
        /// </summary>
		public static string ReciepeVersion = nameof(ReciepeVersion);

        /// <summary>
        ///   查找类似 配方 的本地化字符串。
        /// </summary>
		public static string Recipe = nameof(Recipe);

        /// <summary>
        ///   查找类似 配方备份天数 的本地化字符串。
        /// </summary>
		public static string RecipeBackUpDays = nameof(RecipeBackUpDays);

        /// <summary>
        ///   查找类似 配方格式不正确，请检查 的本地化字符串。
        /// </summary>
		public static string RecipeFormatWrong = nameof(RecipeFormatWrong);

        /// <summary>
        ///   查找类似 恢复 的本地化字符串。
        /// </summary>
		public static string Recoverey = nameof(Recoverey);

        /// <summary>
        ///   查找类似 恢复 的本地化字符串。
        /// </summary>
		public static string Recovery = nameof(Recovery);

        /// <summary>
        ///   查找类似 恢复灯 的本地化字符串。
        /// </summary>
		public static string RecoveryLamp = nameof(RecoveryLamp);

        /// <summary>
        ///   查找类似 红灯 的本地化字符串。
        /// </summary>
		public static string RedLamp = nameof(RedLamp);

        /// <summary>
        ///   查找类似 重做 的本地化字符串。
        /// </summary>
		public static string Redo = nameof(Redo);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string Reference_DoubleClick_ = nameof(Reference_DoubleClick_);

        /// <summary>
        ///   查找类似 引用模块 的本地化字符串。
        /// </summary>
		public static string RefGroup = nameof(RefGroup);

        /// <summary>
        ///   查找类似 刷新 的本地化字符串。
        /// </summary>
		public static string Refresh = nameof(Refresh);

        /// <summary>
        ///   查找类似 刷新频率 的本地化字符串。
        /// </summary>
		public static string RefreshFrequency = nameof(RefreshFrequency);

        /// <summary>
        ///   查找类似 区域 的本地化字符串。
        /// </summary>
		public static string Region = nameof(Region);

        /// <summary>
        ///   查找类似 注册 的本地化字符串。
        /// </summary>
		public static string Register = nameof(Register);

        /// <summary>
        ///   查找类似 注册类型 的本地化字符串。
        /// </summary>
		public static string RegisterType = nameof(RegisterType);

        /// <summary>
        ///   查找类似 配准 的本地化字符串。
        /// </summary>
		public static string Registration = nameof(Registration);

        /// <summary>
        ///   查找类似 相对 的本地化字符串。
        /// </summary>
		public static string Relative = nameof(Relative);

        /// <summary>
        ///   查找类似 相对运动 的本地化字符串。
        /// </summary>
		public static string RelativeMotion = nameof(RelativeMotion);

        /// <summary>
        ///   查找类似 发布说明 的本地化字符串。
        /// </summary>
		public static string ReleaseNotes = nameof(ReleaseNotes);

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public static string Remove = nameof(Remove);

        /// <summary>
        ///   查找类似 重命名 的本地化字符串。
        /// </summary>
		public static string Rename = nameof(Rename);

        /// <summary>
        ///   查找类似 重复密码 的本地化字符串。
        /// </summary>
		public static string RepeatPassword = nameof(RepeatPassword);

        /// <summary>
        ///   查找类似 更换物料 的本地化字符串。
        /// </summary>
		public static string ReplaceMaterials = nameof(ReplaceMaterials);

        /// <summary>
        ///   查找类似 报告 的本地化字符串。
        /// </summary>
		public static string Report = nameof(Report);

        /// <summary>
        ///   查找类似 报告目录 的本地化字符串。
        /// </summary>
		public static string ReportContents = nameof(ReportContents);

        /// <summary>
        ///   查找类似 报表 的本地化字符串。
        /// </summary>
		public static string ReportForm = nameof(ReportForm);

        /// <summary>
        ///   查找类似 报告导航窗 的本地化字符串。
        /// </summary>
		public static string ReportNavigation = nameof(ReportNavigation);

        /// <summary>
        ///   查找类似 报告源 的本地化字符串。
        /// </summary>
		public static string ReportSource = nameof(ReportSource);

        /// <summary>
        ///   查找类似 报表类型 的本地化字符串。
        /// </summary>
		public static string ReportType = nameof(ReportType);

        /// <summary>
        ///   查找类似 复位 的本地化字符串。
        /// </summary>
		public static string Reset = nameof(Reset);

        /// <summary>
        ///   查找类似 产能清零 的本地化字符串。
        /// </summary>
		public static string ResetCapacity = nameof(ResetCapacity);

        /// <summary>
        ///   查找类似 复位工站 的本地化字符串。
        /// </summary>
		public static string ResetStation = nameof(ResetStation);

        /// <summary>
        ///   查找类似 复位变量 的本地化字符串。
        /// </summary>
		public static string ResetVariable = nameof(ResetVariable);

        /// <summary>
        ///   查找类似 一旦删除需要重启软件生效 的本地化字符串。
        /// </summary>
		public static string RestartTakesEffect = nameof(RestartTakesEffect);

        /// <summary>
        ///   查找类似 结果 的本地化字符串。
        /// </summary>
		public static string Result = nameof(Result);

        /// <summary>
        ///   查找类似 重试 的本地化字符串。
        /// </summary>
		public static string Retry = nameof(Retry);

        /// <summary>
        ///   查找类似 终止 的本地化字符串。
        /// </summary>
		public static string Return = nameof(Return);

        /// <summary>
        ///   查找类似 撤销 的本地化字符串。
        /// </summary>
		public static string Revoke = nameof(Revoke);

        /// <summary>
        ///   查找类似 机器人 的本地化字符串。
        /// </summary>
		public static string Robot = nameof(Robot);

        /// <summary>
        ///   查找类似 机器人动作 的本地化字符串。
        /// </summary>
		public static string RobotAction = nameof(RobotAction);

        /// <summary>
        ///   查找类似 2#机器人动作 的本地化字符串。
        /// </summary>
		public static string RobotAction2 = nameof(RobotAction2);

        /// <summary>
        ///   查找类似 机器人信息 的本地化字符串。
        /// </summary>
		public static string RobotInfo = nameof(RobotInfo);

        /// <summary>
        ///   查找类似 机器人运动 的本地化字符串。
        /// </summary>
		public static string RobotMove = nameof(RobotMove);

        /// <summary>
        ///   查找类似 机器人状态 的本地化字符串。
        /// </summary>
		public static string RobotStatus = nameof(RobotStatus);

        /// <summary>
        ///   查找类似 2#机器人状态 的本地化字符串。
        /// </summary>
		public static string RobotStatus2 = nameof(RobotStatus2);

        /// <summary>
        ///   查找类似 机器人版本 的本地化字符串。
        /// </summary>
		public static string RobotVersion = nameof(RobotVersion);

        /// <summary>
        ///   查找类似 ROI配置 的本地化字符串。
        /// </summary>
		public static string ROIConfig = nameof(ROIConfig);

        /// <summary>
        ///   查找类似 卷料 的本地化字符串。
        /// </summary>
		public static string RollSet = nameof(RollSet);

        /// <summary>
        ///   查找类似 卷料计算 的本地化字符串。
        /// </summary>
		public static string RoolMaterialCal = nameof(RoolMaterialCal);

        /// <summary>
        ///   查找类似 顺时针旋转视图 的本地化字符串。
        /// </summary>
		public static string RotateViewClockwise = nameof(RotateViewClockwise);

        /// <summary>
        ///   查找类似 逆时针旋转视图 的本地化字符串。
        /// </summary>
		public static string RotateViewCounterclockwise = nameof(RotateViewCounterclockwise);

        /// <summary>
        ///   查找类似 圆度 的本地化字符串。
        /// </summary>
		public static string Roundness = nameof(Roundness);

        /// <summary>
        ///   查找类似 常规监控 的本地化字符串。
        /// </summary>
		public static string RoutineMonitoring = nameof(RoutineMonitoring);

        /// <summary>
        ///   查找类似 运行 的本地化字符串。
        /// </summary>
		public static string Run = nameof(Run);

        /// <summary>
        ///   查找类似 运行全部 F5 的本地化字符串。
        /// </summary>
		public static string RunAllF5 = nameof(RunAllF5);

        /// <summary>
        ///   查找类似 执行程序 的本地化字符串。
        /// </summary>
		public static string RunExe = nameof(RunExe);

        /// <summary>
        ///   查找类似 运行模式 的本地化字符串。
        /// </summary>
		public static string RunMode = nameof(RunMode);

        /// <summary>
        ///   查找类似 运动模式已存在 的本地化字符串。
        /// </summary>
		public static string RunModeIsAlreadyExist = nameof(RunModeIsAlreadyExist);

        /// <summary>
        ///   查找类似 (启用编辑后双击修改) 的本地化字符串。
        /// </summary>
		public static string RunModeTips = nameof(RunModeTips);

        /// <summary>
        ///   查找类似 流道 的本地化字符串。
        /// </summary>
		public static string Runners = nameof(Runners);

        /// <summary>
        ///   查找类似 下一步 的本地化字符串。
        /// </summary>
		public static string RunNext = nameof(RunNext);

        /// <summary>
        ///   查找类似 运行中 的本地化字符串。
        /// </summary>
		public static string Running = nameof(Running);

        /// <summary>
        ///   查找类似 运行时间 的本地化字符串。
        /// </summary>
		public static string RunningTime = nameof(RunningTime);

        /// <summary>
        ///   查找类似 单步运行 的本地化字符串。
        /// </summary>
		public static string RunOne = nameof(RunOne);

        /// <summary>
        ///   查找类似 s 的本地化字符串。
        /// </summary>
		public static string s = nameof(s);

        /// <summary>
        ///   查找类似 保存 的本地化字符串。
        /// </summary>
		public static string Save = nameof(Save);

        /// <summary>
        ///   查找类似 另存为 的本地化字符串。
        /// </summary>
		public static string SaveAs = nameof(SaveAs);

        /// <summary>
        ///   查找类似 另存项目 的本地化字符串。
        /// </summary>
		public static string SaveAsProject = nameof(SaveAsProject);

        /// <summary>
        ///   查找类似 保存天数 的本地化字符串。
        /// </summary>
		public static string SaveDays = nameof(SaveDays);

        /// <summary>
        ///   查找类似 保存文件 的本地化字符串。
        /// </summary>
		public static string SaveFile = nameof(SaveFile);

        /// <summary>
        ///   查找类似 保存项目 的本地化字符串。
        /// </summary>
		public static string SaveProject = nameof(SaveProject);

        /// <summary>
        ///   查找类似 是否保存工程? 的本地化字符串。
        /// </summary>
		public static string SaveProjectTask = nameof(SaveProjectTask);

        /// <summary>
        ///   查找类似 保存成功 的本地化字符串。
        /// </summary>
		public static string SaveSuccess = nameof(SaveSuccess);

        /// <summary>
        ///   查找类似 任务要保存吗 的本地化字符串。
        /// </summary>
		public static string SaveTask = nameof(SaveTask);

        /// <summary>
        ///   查找类似 扫二维码 的本地化字符串。
        /// </summary>
		public static string ScanBarcode = nameof(ScanBarcode);

        /// <summary>
        ///   查找类似 条码长度 的本地化字符串。
        /// </summary>
		public static string ScanCodeCount = nameof(ScanCodeCount);

        /// <summary>
        ///   查找类似 条码来源 的本地化字符串。
        /// </summary>
		public static string ScanCodeDataSource = nameof(ScanCodeDataSource);

        /// <summary>
        ///   查找类似 扫码统计 的本地化字符串。
        /// </summary>
		public static string ScanCodeStatistics = nameof(ScanCodeStatistics);

        /// <summary>
        ///   查找类似 条码码率 的本地化字符串。
        /// </summary>
		public static string ScanCodeSuccessRate = nameof(ScanCodeSuccessRate);

        /// <summary>
        ///   查找类似 急停 的本地化字符串。
        /// </summary>
		public static string Scram = nameof(Scram);

        /// <summary>
        ///   查找类似 C#脚本 的本地化字符串。
        /// </summary>
		public static string Script = nameof(Script);

        /// <summary>
        ///   查找类似 滚动模式 的本地化字符串。
        /// </summary>
		public static string ScrollMode = nameof(ScrollMode);

        /// <summary>
        ///   查找类似 SDO读写 的本地化字符串。
        /// </summary>
		public static string SDOAction = nameof(SDOAction);

        /// <summary>
        ///   查找类似 搜索文件关键字 的本地化字符串。
        /// </summary>
		public static string SearchFileKeywords = nameof(SearchFileKeywords);

        /// <summary>
        ///   查找类似 搜索模块 的本地化字符串。
        /// </summary>
		public static string SearchModule = nameof(SearchModule);

        /// <summary>
        ///   查找类似 秒 的本地化字符串。
        /// </summary>
		public static string Second = nameof(Second);

        /// <summary>
        ///   查找类似 分割 的本地化字符串。
        /// </summary>
		public static string Segment = nameof(Segment);

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public static string Select = nameof(Select);

        /// <summary>
        ///   查找类似 已选轴 的本地化字符串。
        /// </summary>
		public static string SelectedAxis = nameof(SelectedAxis);

        /// <summary>
        ///   查找类似 选择文件 的本地化字符串。
        /// </summary>
		public static string SelectFile = nameof(SelectFile);

        /// <summary>
        ///   查找类似 分号 的本地化字符串。
        /// </summary>
		public static string Semicolon = nameof(Semicolon);

        /// <summary>
        ///   查找类似 高级 的本地化字符串。
        /// </summary>
		public static string Senior = nameof(Senior);

        /// <summary>
        ///   查找类似 分隔符 的本地化字符串。
        /// </summary>
		public static string Separator = nameof(Separator);

        /// <summary>
        ///   查找类似 序号 的本地化字符串。
        /// </summary>
		public static string SerialNumber = nameof(SerialNumber);

        /// <summary>
        ///   查找类似 RS232 的本地化字符串。
        /// </summary>
		public static string SerialPortDrive = nameof(SerialPortDrive);

        /// <summary>
        ///   查找类似 服务器 的本地化字符串。
        /// </summary>
		public static string Server = nameof(Server);

        /// <summary>
        ///   查找类似 设置 的本地化字符串。
        /// </summary>
		public static string Set = nameof(Set);

        /// <summary>
        ///   查找类似 设置轴点位 的本地化字符串。
        /// </summary>
		public static string SetAxisPos = nameof(SetAxisPos);

        /// <summary>
        ///   查找类似 设置模板 的本地化字符串。
        /// </summary>
		public static string SetCoordTemplate = nameof(SetCoordTemplate);

        /// <summary>
        ///   查找类似 显示选项设置 的本地化字符串。
        /// </summary>
		public static string SetDisPlayOption = nameof(SetDisPlayOption);

        /// <summary>
        ///   查找类似 设置全局变量 的本地化字符串。
        /// </summary>
		public static string SetGlobalVar = nameof(SetGlobalVar);

        /// <summary>
        ///   查找类似 设置信号 的本地化字符串。
        /// </summary>
		public static string SetIO = nameof(SetIO);

        /// <summary>
        ///   查找类似 设置光幕 的本地化字符串。
        /// </summary>
		public static string SetLightCurtain = nameof(SetLightCurtain);

        /// <summary>
        ///   查找类似 机台模式 的本地化字符串。
        /// </summary>
		public static string SetMachineMode = nameof(SetMachineMode);

        /// <summary>
        ///   查找类似 尺寸设置 的本地化字符串。
        /// </summary>
		public static string SetMeasure = nameof(SetMeasure);

        /// <summary>
        ///   查找类似 设置MBus 的本地化字符串。
        /// </summary>
		public static string SetModbus = nameof(SetModbus);

        /// <summary>
        ///   查找类似 设置Modbus 的本地化字符串。
        /// </summary>
		public static string SetModbusEx = nameof(SetModbusEx);

        /// <summary>
        ///   查找类似 设置机器人状态 的本地化字符串。
        /// </summary>
		public static string SetRobotStatus = nameof(SetRobotStatus);

        /// <summary>
        ///   查找类似 设置工站 的本地化字符串。
        /// </summary>
		public static string SetStation = nameof(SetStation);

        /// <summary>
        ///   查找类似 设置变量 的本地化字符串。
        /// </summary>
		public static string SetVariable = nameof(SetVariable);

        /// <summary>
        ///   查找类似 设置工作流 的本地化字符串。
        /// </summary>
		public static string SetWorkFlow = nameof(SetWorkFlow);

        /// <summary>
        ///   查找类似 SFC相关配置 的本地化字符串。
        /// </summary>
		public static string SFC = nameof(SFC);

        /// <summary>
        ///   查找类似 SFC流程 的本地化字符串。
        /// </summary>
		public static string SFCFlow = nameof(SFCFlow);

        /// <summary>
        ///   查找类似 调机料SFC流程 的本地化字符串。
        /// </summary>
		public static string SFCFlowTiaoJi = nameof(SFCFlowTiaoJi);

        /// <summary>
        ///   查找类似 SFTP上传 的本地化字符串。
        /// </summary>
		public static string SFTPUpload = nameof(SFTPUpload);

        /// <summary>
        ///   查找类似 信号灯 的本地化字符串。
        /// </summary>
		public static string SignalLamp = nameof(SignalLamp);

        /// <summary>
        ///   查找类似 单轴喷码 的本地化字符串。
        /// </summary>
		public static string SingelAxisFlyShot = nameof(SingelAxisFlyShot);

        /// <summary>
        ///   查找类似 单轴 的本地化字符串。
        /// </summary>
		public static string SingleAxis = nameof(SingleAxis);

        /// <summary>
        ///   查找类似 CT 的本地化字符串。
        /// </summary>
		public static string SingleCT = nameof(SingleCT);

        /// <summary>
        ///   查找类似 单页 的本地化字符串。
        /// </summary>
		public static string SinglePage = nameof(SinglePage);

        /// <summary>
        ///   查找类似 单轴 的本地化字符串。
        /// </summary>
		public static string SinleAxis = nameof(SinleAxis);

        /// <summary>
        ///   查找类似 忽略 的本地化字符串。
        /// </summary>
		public static string Skip = nameof(Skip);

        /// <summary>
        ///   查找类似 烟雾报警器 的本地化字符串。
        /// </summary>
		public static string SmokeAlarmDevice = nameof(SmokeAlarmDevice);

        /// <summary>
        ///   查找类似 平滑 的本地化字符串。
        /// </summary>
		public static string Smooth = nameof(Smooth);

        /// <summary>
        ///   查找类似 SN编码 的本地化字符串。
        /// </summary>
		public static string SNCode = nameof(SNCode);

        /// <summary>
        ///   查找类似 软件 的本地化字符串。
        /// </summary>
		public static string Soft = nameof(Soft);

        /// <summary>
        ///   查找类似 软件配置 的本地化字符串。
        /// </summary>
		public static string SoftConfigure = nameof(SoftConfigure);

        /// <summary>
        ///   查找类似 软件信息 的本地化字符串。
        /// </summary>
		public static string SoftInformation = nameof(SoftInformation);

        /// <summary>
        ///   查找类似 软件版本 的本地化字符串。
        /// </summary>
		public static string SoftVersion = nameof(SoftVersion);

        /// <summary>
        ///   查找类似 按钮点击触发软件停止 的本地化字符串。
        /// </summary>
		public static string SoftWareStopByClick = nameof(SoftWareStopByClick);

        /// <summary>
        ///   查找类似 版本更新信息 的本地化字符串。
        /// </summary>
		public static string SoftWareUpdateInfo = nameof(SoftWareUpdateInfo);

        /// <summary>
        ///   查找类似 软件版本 的本地化字符串。
        /// </summary>
		public static string SoftWareVersion = nameof(SoftWareVersion);

        /// <summary>
        ///   查找类似 方案 的本地化字符串。
        /// </summary>
		public static string Solution = nameof(Solution);

        /// <summary>
        ///   查找类似 工程模式 的本地化字符串。
        /// </summary>
		public static string SolutionMode = nameof(SolutionMode);

        /// <summary>
        ///   查找类似 来源 的本地化字符串。
        /// </summary>
		public static string Source = nameof(Source);

        /// <summary>
        ///   查找类似 空格 的本地化字符串。
        /// </summary>
		public static string Space = nameof(Space);

        /// <summary>
        ///   查找类似 间距 的本地化字符串。
        /// </summary>
		public static string SpaceFactor = nameof(SpaceFactor);

        /// <summary>
        ///   查找类似 消耗物品 的本地化字符串。
        /// </summary>
		public static string SparePartsUsed = nameof(SparePartsUsed);

        /// <summary>
        ///   查找类似 规格值配置 的本地化字符串。
        /// </summary>
		public static string SpecSet = nameof(SpecSet);

        /// <summary>
        ///   查找类似 速度系数 的本地化字符串。
        /// </summary>
		public static string SpeedFactor = nameof(SpeedFactor);

        /// <summary>
        ///   查找类似 球体 的本地化字符串。
        /// </summary>
		public static string Sphere = nameof(Sphere);

        /// <summary>
        ///   查找类似 按位读取 的本地化字符串。
        /// </summary>
		public static string SplitIntToBit = nameof(SplitIntToBit);

        /// <summary>
        ///   查找类似 字符串分割 的本地化字符串。
        /// </summary>
		public static string SplitString = nameof(SplitString);

        /// <summary>
        ///   查找类似 标准值 的本地化字符串。
        /// </summary>
		public static string StandardValue = nameof(StandardValue);

        /// <summary>
        ///   查找类似 启动 的本地化字符串。
        /// </summary>
		public static string Start = nameof(Start);

        /// <summary>
        ///   查找类似 起始列 的本地化字符串。
        /// </summary>
		public static string StartColumn = nameof(StartColumn);

        /// <summary>
        ///   查找类似 启动灯 的本地化字符串。
        /// </summary>
		public static string StartLamp = nameof(StartLamp);

        /// <summary>
        ///   查找类似 开始模块 的本地化字符串。
        /// </summary>
		public static string StartModule = nameof(StartModule);

        /// <summary>
        ///   查找类似 开始维修 的本地化字符串。
        /// </summary>
		public static string StartRepair = nameof(StartRepair);

        /// <summary>
        ///   查找类似 起始行 的本地化字符串。
        /// </summary>
		public static string StartRow = nameof(StartRow);

        /// <summary>
        ///   查找类似 开始工站 的本地化字符串。
        /// </summary>
		public static string StartStation = nameof(StartStation);

        /// <summary>
        ///   查找类似 开始时间 的本地化字符串。
        /// </summary>
		public static string StartTime = nameof(StartTime);

        /// <summary>
        ///   查找类似 状态 的本地化字符串。
        /// </summary>
		public static string State = nameof(State);

        /// <summary>
        ///   查找类似 工站 的本地化字符串。
        /// </summary>
		public static string Station = nameof(Station);

        /// <summary>
        ///   查找类似 工站ID 的本地化字符串。
        /// </summary>
		public static string StationID = nameof(StationID);

        /// <summary>
        ///   查找类似 工站名称 的本地化字符串。
        /// </summary>
		public static string StationName = nameof(StationName);

        /// <summary>
        ///   查找类似 工站总览 的本地化字符串。
        /// </summary>
		public static string StationOverview = nameof(StationOverview);

        /// <summary>
        ///   查找类似 工站 的本地化字符串。
        /// </summary>
		public static string Stations = nameof(Stations);

        /// <summary>
        ///   查找类似 工站有料 的本地化字符串。
        /// </summary>
		public static string StationSet = nameof(StationSet);

        /// <summary>
        ///   查找类似 工站类型 的本地化字符串。
        /// </summary>
		public static string StationType = nameof(StationType);

        /// <summary>
        ///   查找类似 统计 的本地化字符串。
        /// </summary>
		public static string Statistics = nameof(Statistics);

        /// <summary>
        ///   查找类似 状态 的本地化字符串。
        /// </summary>
		public static string Status = nameof(Status);

        /// <summary>
        ///   查找类似 步骤 的本地化字符串。
        /// </summary>
		public static string Step = nameof(Step);

        /// <summary>
        ///   查找类似 串行任务 的本地化字符串。
        /// </summary>
		public static string StepGroup = nameof(StepGroup);

        /// <summary>
        ///   查找类似 STL格式 的本地化字符串。
        /// </summary>
		public static string STL = nameof(STL);

        /// <summary>
        ///   查找类似 停止 的本地化字符串。
        /// </summary>
		public static string Stop = nameof(Stop);

        /// <summary>
        ///   查找类似 暂停 F8 的本地化字符串。
        /// </summary>
		public static string StopF8 = nameof(StopF8);

        /// <summary>
        ///   查找类似 停止灯 的本地化字符串。
        /// </summary>
		public static string StopLamp = nameof(StopLamp);

        /// <summary>
        ///   查找类似 直线度 的本地化字符串。
        /// </summary>
		public static string Straightness = nameof(Straightness);

        /// <summary>
        ///   查找类似 字符拼接 的本地化字符串。
        /// </summary>
		public static string StringMerge = nameof(StringMerge);

        /// <summary>
        ///   查找类似 字符解析 的本地化字符串。
        /// </summary>
		public static string StringParse = nameof(StringParse);

        /// <summary>
        ///   查找类似 物流线治具数量 的本地化字符串。
        /// </summary>
		public static string SubCarrierNum = nameof(SubCarrierNum);

        /// <summary>
        ///   查找类似 吸嘴压力标定 的本地化字符串。
        /// </summary>
		public static string SuctionNozzle = nameof(SuctionNozzle);

        /// <summary>
        ///   查找类似 多分支 的本地化字符串。
        /// </summary>
		public static string Switch = nameof(Switch);

        /// <summary>
        ///   查找类似 条件任务 的本地化字符串。
        /// </summary>
		public static string SwitchGroup = nameof(SwitchGroup);

        /// <summary>
        ///   查找类似 系统操作信息 的本地化字符串。
        /// </summary>
		public static string SysOperateInfo = nameof(SysOperateInfo);

        /// <summary>
        ///   查找类似 稼动IO 的本地化字符串。
        /// </summary>
		public static string SysOperationIO = nameof(SysOperationIO);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string System_Operation_Information = nameof(System_Operation_Information);

        /// <summary>
        ///   查找类似 系统操作提示信息 的本地化字符串。
        /// </summary>
		public static string SystemOperationPromptInformation = nameof(SystemOperationPromptInformation);

        /// <summary>
        ///   查找类似 系统操作提示 的本地化字符串。
        /// </summary>
		public static string SystemOperationTips = nameof(SystemOperationTips);

        /// <summary>
        ///   查找类似 表格生成 的本地化字符串。
        /// </summary>
		public static string TableCreate = nameof(TableCreate);

        /// <summary>
        ///   查找类似 表格写入 的本地化字符串。
        /// </summary>
		public static string TableInsert = nameof(TableInsert);

        /// <summary>
        ///   查找类似 压力曲线堆叠图 的本地化字符串。
        /// </summary>
		public static string TaikeAnnotatedCurve = nameof(TaikeAnnotatedCurve);

        /// <summary>
        ///   查找类似 大寰音圈电机曲线监控 的本地化字符串。
        /// </summary>
		public static string DHVCMMonitor = nameof(DHVCMMonitor);

        /// <summary>
        ///   查找类似 泰科统计 的本地化字符串。
        /// </summary>
		public static string TaikeContent = nameof(TaikeContent);

        /// <summary>
        ///   查找类似 泰科曲线 的本地化字符串。
        /// </summary>
		public static string TaikeCurve = nameof(TaikeCurve);

        /// <summary>
        ///   查找类似 太科电批 的本地化字符串。
        /// </summary>
		public static string TaiKeScrewDriver = nameof(TaiKeScrewDriver);

        /// <summary>
        ///   查找类似 Target_CT 的本地化字符串。
        /// </summary>
		public static string Target_CT = nameof(Target_CT);

        /// <summary>
        ///   查找类似 运动的目标单位，单位mm 的本地化字符串。
        /// </summary>
		public static string TargetUnitOfMotion = nameof(TargetUnitOfMotion);

        /// <summary>
        ///   查找类似 目标：mm 的本地化字符串。
        /// </summary>
		public static string TargetWithUnit = nameof(TargetWithUnit);

        /// <summary>
        ///   查找类似 任务流 的本地化字符串。
        /// </summary>
		public static string TaskFlow = nameof(TaskFlow);

        /// <summary>
        ///   查找类似 任务模拟器 的本地化字符串。
        /// </summary>
		public static string TaskSimulator = nameof(TaskSimulator);

        /// <summary>
        ///   查找类似 示教 的本地化字符串。
        /// </summary>
		public static string Teach = nameof(Teach);

        /// <summary>
        ///   查找类似 示教位置 的本地化字符串。
        /// </summary>
		public static string TeachLocation = nameof(TeachLocation);

        /// <summary>
        ///   查找类似 撕膜 的本地化字符串。
        /// </summary>
		public static string Tearing = nameof(Tearing);

        /// <summary>
        ///   查找类似 工艺流程 的本地化字符串。
        /// </summary>
		public static string TechnologicalProcess = nameof(TechnologicalProcess);

        /// <summary>
        ///   查找类似 测试按钮 的本地化字符串。
        /// </summary>
		public static string TestBotton = nameof(TestBotton);

        /// <summary>
        ///   查找类似 测试工站 的本地化字符串。
        /// </summary>
		public static string TestStation = nameof(TestStation);

        /// <summary>
        ///   查找类似 本站要料 的本地化字符串。
        /// </summary>
		public static string ThisGet = nameof(ThisGet);

        /// <summary>
        ///   查找类似 本站有料 的本地化字符串。
        /// </summary>
		public static string ThisHave = nameof(ThisHave);

        /// <summary>
        ///   查找类似 3D 的本地化字符串。
        /// </summary>
		public static string ThreeDimision = nameof(ThreeDimision);

        /// <summary>
        ///   查找类似 抛料设置 的本地化字符串。
        /// </summary>
		public static string ThrowingSetting = nameof(ThrowingSetting);

        /// <summary>
        ///   查找类似 抛料耗时 的本地化字符串。
        /// </summary>
		public static string ThrowingTime = nameof(ThrowingTime);

        /// <summary>
        ///   查找类似 耗时 的本地化字符串。
        /// </summary>
		public static string Time = nameof(Time);

        /// <summary>
        ///   查找类似 时间记录事件 的本地化字符串。
        /// </summary>
		public static string TimeLogEvent = nameof(TimeLogEvent);

        /// <summary>
        ///   查找类似 定时判断 的本地化字符串。
        /// </summary>
		public static string TimerJudge = nameof(TimerJudge);

        /// <summary>
        ///   查找类似 提示 的本地化字符串。
        /// </summary>
		public static string Tip = nameof(Tip);

        /// <summary>
        ///   查找类似 标题 的本地化字符串。
        /// </summary>
		public static string Title = nameof(Title);

        /// <summary>
        ///   查找类似  的本地化字符串。
        /// </summary>
		public static string To_Be_Initialized = nameof(To_Be_Initialized);

        /// <summary>
        ///   查找类似 公差 的本地化字符串。
        /// </summary>
		public static string Tolerance = nameof(Tolerance);

        /// <summary>
        ///   查找类似 公差参数 的本地化字符串。
        /// </summary>
		public static string TolParameter = nameof(TolParameter);

        /// <summary>
        ///   查找类似 工具 的本地化字符串。
        /// </summary>
		public static string Tool = nameof(Tool);

        /// <summary>
        ///   查找类似 过大 的本地化字符串。
        /// </summary>
		public static string TooLarge = nameof(TooLarge);

        /// <summary>
        ///   查找类似 Cowling报表 的本地化字符串。
        /// </summary>
		public static string TorqueForm = nameof(TorqueForm);

        /// <summary>
        ///   查找类似 锁螺丝报表 的本地化字符串。
        /// </summary>
		public static string TorqueForm2 = nameof(TorqueForm2);

        /// <summary>
        ///   查找类似 总治具数量 的本地化字符串。
        /// </summary>
		public static string TotalCarrierNum = nameof(TotalCarrierNum);

        /// <summary>
        ///   查找类似 TotalCodeSweep 的本地化字符串。
        /// </summary>
		public static string TotalCodeSweep = nameof(TotalCodeSweep);

        /// <summary>
        ///   查找类似 总计耗时 的本地化字符串。
        /// </summary>
		public static string TotalTime = nameof(TotalTime);

        /// <summary>
        ///   查找类似 训练 的本地化字符串。
        /// </summary>
		public static string Train = nameof(Train);

        /// <summary>
        ///   查找类似 变换 的本地化字符串。
        /// </summary>
		public static string Transform = nameof(Transform);

        /// <summary>
        ///   查找类似 坐标系变换 的本地化字符串。
        /// </summary>
		public static string TransformCoord = nameof(TransformCoord);

        /// <summary>
        ///   查找类似 三色灯状态 的本地化字符串。
        /// </summary>
		public static string TriColorStatus = nameof(TriColorStatus);

        /// <summary>
        ///   查找类似 转盘 的本地化字符串。
        /// </summary>
		public static string Turntable = nameof(Turntable);

        /// <summary>
        ///   查找类似 2D算子 的本地化字符串。
        /// </summary>
		public static string TwoD = nameof(TwoD);

        /// <summary>
        ///   查找类似 2D 的本地化字符串。
        /// </summary>
		public static string TwoDimision = nameof(TwoDimision);

        /// <summary>
        ///   查找类似 双页模式 的本地化字符串。
        /// </summary>
		public static string TwoPageMode = nameof(TwoPageMode);

        /// <summary>
        ///   查找类似 读取Txt 的本地化字符串。
        /// </summary>
		public static string TxtReader = nameof(TxtReader);

        /// <summary>
        ///   查找类似 文本写入 的本地化字符串。
        /// </summary>
		public static string TxtWriter = nameof(TxtWriter);

        /// <summary>
        ///   查找类似 类型 的本地化字符串。
        /// </summary>
		public static string Type = nameof(Type);

        /// <summary>
        ///   查找类似 类型不支持 的本地化字符串。
        /// </summary>
		public static string TypeNotSupported = nameof(TypeNotSupported);

        /// <summary>
        ///   查找类似 U_加速时间_Target 的本地化字符串。
        /// </summary>
		public static string U_Acceleration_Target = nameof(U_Acceleration_Target);

        /// <summary>
        ///   查找类似 U_加速时间_Actual 的本地化字符串。
        /// </summary>
		public static string U_AccelerationTime_Actual = nameof(U_AccelerationTime_Actual);

        /// <summary>
        ///   查找类似 U_速度_Actual 的本地化字符串。
        /// </summary>
		public static string U_Speed_Actual = nameof(U_Speed_Actual);

        /// <summary>
        ///   查找类似 U_速度_Target 的本地化字符串。
        /// </summary>
		public static string U_Speed_Target = nameof(U_Speed_Target);

        /// <summary>
        ///   查找类似 单元 的本地化字符串。
        /// </summary>
		public static string Unit = nameof(Unit);

        /// <summary>
        ///   查找类似 未知 的本地化字符串。
        /// </summary>
		public static string Unknown = nameof(Unknown);

        /// <summary>
        ///   查找类似 未知PLC状态 的本地化字符串。
        /// </summary>
		public static string UnknownPLCStatus = nameof(UnknownPLCStatus);

        /// <summary>
        ///   查找类似 未知大小 的本地化字符串。
        /// </summary>
		public static string UnknownSize = nameof(UnknownSize);

        /// <summary>
        ///   查找类似 未知工站 的本地化字符串。
        /// </summary>
		public static string UnKnownStation = nameof(UnKnownStation);

        /// <summary>
        ///   查找类似 下料仓 的本地化字符串。
        /// </summary>
		public static string UnLoadingSilo = nameof(UnLoadingSilo);

        /// <summary>
        ///   查找类似 更新 的本地化字符串。
        /// </summary>
		public static string Update = nameof(Update);

        /// <summary>
        ///   查找类似 更新内容 的本地化字符串。
        /// </summary>
		public static string UpdateContent = nameof(UpdateContent);

        /// <summary>
        ///   查找类似 更新变量 的本地化字符串。
        /// </summary>
		public static string UpdateVar = nameof(UpdateVar);

        /// <summary>
        ///   查找类似 数据上传 的本地化字符串。
        /// </summary>
		public static string UploadData = nameof(UploadData);

        /// <summary>
        ///   查找类似 公差上限 的本地化字符串。
        /// </summary>
		public static string UpperLimit = nameof(UpperLimit);

        /// <summary>
        ///   查找类似 使用 的本地化字符串。
        /// </summary>
		public static string Use = nameof(Use);

        /// <summary>
        ///   查找类似 用户ID 的本地化字符串。
        /// </summary>
		public static string UseID = nameof(UseID);

        /// <summary>
        ///   查找类似 用户配置 的本地化字符串。
        /// </summary>
		public static string UserConfigure = nameof(UserConfigure);

        /// <summary>
        ///   查找类似 用户列表 的本地化字符串。
        /// </summary>
		public static string UserList = nameof(UserList);

        /// <summary>
        ///   查找类似 用户名 的本地化字符串。
        /// </summary>
		public static string UserName = nameof(UserName);

        /// <summary>
        ///   查找类似 使用教程 的本地化字符串。
        /// </summary>
		public static string UsingTutorials = nameof(UsingTutorials);

        /// <summary>
        ///   查找类似 VA 的本地化字符串。
        /// </summary>
		public static string VA = nameof(VA);

        /// <summary>
        ///   查找类似 真空 的本地化字符串。
        /// </summary>
		public static string Vacuum = nameof(Vacuum);

        /// <summary>
        ///   查找类似 值 的本地化字符串。
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
        ///   查找类似 仿真多轴 的本地化字符串。
        /// </summary>
		public static string VAxisM = nameof(VAxisM);

        /// <summary>
        ///   查找类似 仿真皮带 的本地化字符串。
        /// </summary>
		public static string VBelt = nameof(VBelt);

        /// <summary>
        ///   查找类似 仿真按钮 的本地化字符串。
        /// </summary>
		public static string VButton = nameof(VButton);

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
        ///   查找类似 版本信息窗 的本地化字符串。
        /// </summary>
		public static string VersionInfoDialog = nameof(VersionInfoDialog);

        /// <summary>
        ///   查找类似 仿真电批 的本地化字符串。
        /// </summary>
		public static string VESD = nameof(VESD);

        /// <summary>
        ///   查找类似 仿真飞达 的本地化字符串。
        /// </summary>
		public static string VFeeder = nameof(VFeeder);

        /// <summary>
        ///   查找类似 视图 的本地化字符串。
        /// </summary>
		public static string View = nameof(View);

        /// <summary>
        ///   查找类似 视图居中 的本地化字符串。
        /// </summary>
		public static string ViewCentered = nameof(ViewCentered);

        /// <summary>
        ///   查找类似 视图方向设置 的本地化字符串。
        /// </summary>
		public static string ViewDirectionSetting = nameof(ViewDirectionSetting);

        /// <summary>
        ///   查找类似 视图翻转 的本地化字符串。
        /// </summary>
		public static string ViewFlip = nameof(ViewFlip);

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
        ///   查找类似 显示标签 的本地化字符串。
        /// </summary>
		public static string VisibleLabel = nameof(VisibleLabel);

        /// <summary>
        ///   查找类似 Vision相关配置 的本地化字符串。
        /// </summary>
		public static string Vision = nameof(Vision);

        /// <summary>
        ///   查找类似 视觉标定 的本地化字符串。
        /// </summary>
		public static string VisionCalibration = nameof(VisionCalibration);

        /// <summary>
        ///   查找类似 Vision补充 的本地化字符串。
        /// </summary>
		public static string VisionExtra = nameof(VisionExtra);

        /// <summary>
        ///   查找类似 Vision控制参数 的本地化字符串。
        /// </summary>
		public static string VisionInformation = nameof(VisionInformation);

        /// <summary>
        ///   查找类似 Vision IP 的本地化字符串。
        /// </summary>
		public static string VisionIP = nameof(VisionIP);

        /// <summary>
        ///   查找类似 Vision端口 的本地化字符串。
        /// </summary>
		public static string VisionPort = nameof(VisionPort);

        /// <summary>
        ///   查找类似 Vision过程参数 的本地化字符串。
        /// </summary>
		public static string VisionProcessData = nameof(VisionProcessData);

        /// <summary>
        ///   查找类似 Vision工站ID 的本地化字符串。
        /// </summary>
		public static string VisionStationId = nameof(VisionStationId);

        /// <summary>
        ///   查找类似 视觉版本 的本地化字符串。
        /// </summary>
		public static string VisionVersion = nameof(VisionVersion);

        /// <summary>
        ///   查找类似 仿真线激光 的本地化字符串。
        /// </summary>
		public static string VLineLaser = nameof(VLineLaser);

        /// <summary>
        ///   查找类似 仿真输出 的本地化字符串。
        /// </summary>
		public static string VOutputIO = nameof(VOutputIO);

        /// <summary>
        ///   查找类似 力矩电缸 的本地化字符串。
        /// </summary>
		public static string VPCylinder = nameof(VPCylinder);

        /// <summary>
        ///   查找类似 PLC 的本地化字符串。
        /// </summary>
		public static string VPlc = nameof(VPlc);

        /// <summary>
        ///   查找类似 仿真打印机 的本地化字符串。
        /// </summary>
		public static string VPrinter = nameof(VPrinter);

        /// <summary>
        ///   查找类似 仿真三色灯 的本地化字符串。
        /// </summary>
		public static string VTricolorlamp = nameof(VTricolorlamp);

        /// <summary>
        ///   查找类似 仿真真空 的本地化字符串。
        /// </summary>
		public static string VVacuum = nameof(VVacuum);

        /// <summary>
        ///   查找类似 等待 的本地化字符串。
        /// </summary>
		public static string Wait = nameof(Wait);

        /// <summary>
        ///   查找类似 等待条件 的本地化字符串。
        /// </summary>
		public static string WaitCondition = nameof(WaitCondition);

        /// <summary>
        ///   查找类似 等待Fins 的本地化字符串。
        /// </summary>
		public static string WaitFins = nameof(WaitFins);

        /// <summary>
        ///   查找类似 等待信号 的本地化字符串。
        /// </summary>
		public static string WaitIO = nameof(WaitIO);

        /// <summary>
        ///   查找类似 等待MC 的本地化字符串。
        /// </summary>
		public static string WaitMC = nameof(WaitMC);

        /// <summary>
        ///   查找类似 等待MBus 的本地化字符串。
        /// </summary>
		public static string WaitModbus = nameof(WaitModbus);

        /// <summary>
        ///   查找类似 等待PLC 的本地化字符串。
        /// </summary>
		public static string WaitPlc = nameof(WaitPlc);

        /// <summary>
        ///   查找类似 等待模块 的本地化字符串。
        /// </summary>
		public static string WaitStatus = nameof(WaitStatus);

        /// <summary>
        ///   查找类似 警告 的本地化字符串。
        /// </summary>
		public static string Warning = nameof(Warning);

        /// <summary>
        ///   查找类似 设备参数读取 的本地化字符串。
        /// </summary>
		public static string WebConfigRead = nameof(WebConfigRead);

        /// <summary>
        ///   查找类似 Web请求 的本地化字符串。
        /// </summary>
		public static string WebHttp = nameof(WebHttp);

        /// <summary>
        ///   查找类似 周 的本地化字符串。
        /// </summary>
		public static string Week = nameof(Week);

        /// <summary>
        ///   查找类似 宽度 的本地化字符串。
        /// </summary>
		public static string Width = nameof(Width);

        /// <summary>
        ///   查找类似 条码打印 的本地化字符串。
        /// </summary>
		public static string WipPrint = nameof(WipPrint);

        /// <summary>
        ///   查找类似 工艺流程 的本地化字符串。
        /// </summary>
		public static string WorkFlow = nameof(WorkFlow);

        /// <summary>
        ///   查找类似 工单 的本地化字符串。
        /// </summary>
		public static string WorkOrder = nameof(WorkOrder);

        /// <summary>
        ///   查找类似 写入Fins 的本地化字符串。
        /// </summary>
		public static string WriteFins = nameof(WriteFins);

        /// <summary>
        ///   查找类似 写入MC 的本地化字符串。
        /// </summary>
		public static string WriteMC = nameof(WriteMC);

        /// <summary>
        ///   查找类似 写入PLC 的本地化字符串。
        /// </summary>
		public static string WritePlc = nameof(WritePlc);

        /// <summary>
        ///   查找类似 错误 的本地化字符串。
        /// </summary>
		public static string Wrong = nameof(Wrong);

        /// <summary>
        ///   查找类似 X_加速时间_Target 的本地化字符串。
        /// </summary>
		public static string X_Acceleration_Target = nameof(X_Acceleration_Target);

        /// <summary>
        ///   查找类似 X_加速时间_Actual 的本地化字符串。
        /// </summary>
		public static string X_AccelerationTime_Actual = nameof(X_AccelerationTime_Actual);

        /// <summary>
        ///   查找类似 X_速度_Actual 的本地化字符串。
        /// </summary>
		public static string X_Speed_Actual = nameof(X_Speed_Actual);

        /// <summary>
        ///   查找类似 X_速度_Target 的本地化字符串。
        /// </summary>
		public static string X_Speed_Target = nameof(X_Speed_Target);

        /// <summary>
        ///   查找类似 鑫精诚压力传感器 的本地化字符串。
        /// </summary>
		public static string XJCPressureSensor = nameof(XJCPressureSensor);

        /// <summary>
        ///   查找类似 鑫精诚多通道F600 的本地化字符串。
        /// </summary>
		public static string XJCPressureSensorF600 = nameof(XJCPressureSensorF600);

        /// <summary>
        ///   查找类似 Y_加速时间_Target 的本地化字符串。
        /// </summary>
		public static string Y_Acceleration_Target = nameof(Y_Acceleration_Target);

        /// <summary>
        ///   查找类似 Y_加速时间_Actual 的本地化字符串。
        /// </summary>
		public static string Y_AccelerationTime_Actual = nameof(Y_AccelerationTime_Actual);

        /// <summary>
        ///   查找类似 Y_速度_Actual 的本地化字符串。
        /// </summary>
		public static string Y_Speed_Actual = nameof(Y_Speed_Actual);

        /// <summary>
        ///   查找类似 Y_速度_Target 的本地化字符串。
        /// </summary>
		public static string Y_Speed_Target = nameof(Y_Speed_Target);

        /// <summary>
        ///   查找类似 黄灯 的本地化字符串。
        /// </summary>
		public static string YellowLamp = nameof(YellowLamp);

        /// <summary>
        ///   查找类似 是 的本地化字符串。
        /// </summary>
		public static string Yes = nameof(Yes);

        /// <summary>
        ///   查找类似 良率 的本地化字符串。
        /// </summary>
		public static string Yield = nameof(Yield);

        /// <summary>
        ///   查找类似 Z_加速时间_Target 的本地化字符串。
        /// </summary>
		public static string Z_Acceleration_Target = nameof(Z_Acceleration_Target);

        /// <summary>
        ///   查找类似 Z_加速时间_Actual 的本地化字符串。
        /// </summary>
		public static string Z_AccelerationTime_Actual = nameof(Z_AccelerationTime_Actual);

        /// <summary>
        ///   查找类似 Z_速度_Actual 的本地化字符串。
        /// </summary>
		public static string Z_Speed_Actual = nameof(Z_Speed_Actual);

        /// <summary>
        ///   查找类似 Z_速度_Target 的本地化字符串。
        /// </summary>
		public static string Z_Speed_Target = nameof(Z_Speed_Target);

        /// <summary>
        ///   查找类似 Z轴安全区 的本地化字符串。
        /// </summary>
		public static string ZAxisSafeRegion = nameof(ZAxisSafeRegion);

        /// <summary>
        ///   查找类似 缩放 的本地化字符串。
        /// </summary>
		public static string Zoom = nameof(Zoom);

        /// <summary>
        ///   查找类似 放大 的本地化字符串。
        /// </summary>
		public static string ZoomIn = nameof(ZoomIn);

        /// <summary>
        ///   查找类似 缩小 的本地化字符串。
        /// </summary>
		public static string ZoomOut = nameof(ZoomOut);

    }
}
