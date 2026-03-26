using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HandyControl.Tools;

namespace Luster.Common.Assets.Langs
{
    public class LangProvider : INotifyPropertyChanged
    {
        public static LangProvider Instance { get; } = ResourceHelper.GetResource<LangProvider>("Langs");

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
			OnPropertyChanged(nameof(Active));
			OnPropertyChanged(nameof(Add));
			OnPropertyChanged(nameof(AddInParam));
			OnPropertyChanged(nameof(AdjustModelAndPointCloud));
			OnPropertyChanged(nameof(AliasCannotDuplicate));
			OnPropertyChanged(nameof(AliasCannotEmpty));
			OnPropertyChanged(nameof(AlignByBestFit));
			OnPropertyChanged(nameof(AlignByCoord));
			OnPropertyChanged(nameof(AlignByInit));
			OnPropertyChanged(nameof(AlignByRPS));
			OnPropertyChanged(nameof(Alignment));
			OnPropertyChanged(nameof(All));
			OnPropertyChanged(nameof(Am));
			OnPropertyChanged(nameof(AngleMeasure));
			OnPropertyChanged(nameof(Apply));
			OnPropertyChanged(nameof(ArcLine2Line));
			OnPropertyChanged(nameof(ArcLine2Plane));
			OnPropertyChanged(nameof(ArcPlane2Plane));
			OnPropertyChanged(nameof(ArrangeDocument));
			OnPropertyChanged(nameof(AsyncGroup));
			OnPropertyChanged(nameof(AverageTime));
			OnPropertyChanged(nameof(AxisDirection));
			OnPropertyChanged(nameof(BatchChangeGeometry));
			OnPropertyChanged(nameof(BatchCreateMeasurements));
			OnPropertyChanged(nameof(BatchGenMeaPoints));
			OnPropertyChanged(nameof(BatchGenPoints));
			OnPropertyChanged(nameof(BatchGeometry));
			OnPropertyChanged(nameof(BatchImportPoints));
			OnPropertyChanged(nameof(BatchLoadingPointClouds));
			OnPropertyChanged(nameof(BestFit));
			OnPropertyChanged(nameof(BoundBox));
			OnPropertyChanged(nameof(Branch));
			OnPropertyChanged(nameof(BranchGroup));
			OnPropertyChanged(nameof(CADModel));
			OnPropertyChanged(nameof(Calculator));
			OnPropertyChanged(nameof(Calib));
			OnPropertyChanged(nameof(CalibrationOperationInstructions));
			OnPropertyChanged(nameof(Cancel));
			OnPropertyChanged(nameof(CancelCalibration));
			OnPropertyChanged(nameof(CancelCoordTemplate));
			OnPropertyChanged(nameof(CancelSkip));
			OnPropertyChanged(nameof(CenterPoint));
			OnPropertyChanged(nameof(CheckBenchmarkEmpty));
			OnPropertyChanged(nameof(CheckForUpdates));
			OnPropertyChanged(nameof(Choose));
			OnPropertyChanged(nameof(Circle));
			OnPropertyChanged(nameof(Clear));
			OnPropertyChanged(nameof(ClearBenchmarkParameters));
			OnPropertyChanged(nameof(ClearInOutParameters));
			OnPropertyChanged(nameof(ClickModelAndPointCloud));
			OnPropertyChanged(nameof(Close));
			OnPropertyChanged(nameof(CloseAll));
			OnPropertyChanged(nameof(CloseCalibration));
			OnPropertyChanged(nameof(CloseOther));
			OnPropertyChanged(nameof(CloudAlignMatch));
			OnPropertyChanged(nameof(CloudCalib));
			OnPropertyChanged(nameof(CloudDenoising));
			OnPropertyChanged(nameof(CloudDownSampling));
			OnPropertyChanged(nameof(CloudExtractEdge));
			OnPropertyChanged(nameof(CloudMesh));
			OnPropertyChanged(nameof(CloudNormal));
			OnPropertyChanged(nameof(CloudProcess));
			OnPropertyChanged(nameof(CloudProjPlane));
			OnPropertyChanged(nameof(CloudRegistration));
			OnPropertyChanged(nameof(CloudReRepeat));
			OnPropertyChanged(nameof(CloudSectionSegment));
			OnPropertyChanged(nameof(CloudSegment));
			OnPropertyChanged(nameof(CloudSmooth));
			OnPropertyChanged(nameof(CloudTransform));
			OnPropertyChanged(nameof(CollisionDirection));
			OnPropertyChanged(nameof(Color));
			OnPropertyChanged(nameof(Comma));
			OnPropertyChanged(nameof(Compare));
			OnPropertyChanged(nameof(Composing));
			OnPropertyChanged(nameof(Cone));
			OnPropertyChanged(nameof(ConfigureInputParametersFirst));
			OnPropertyChanged(nameof(Confirm));
			OnPropertyChanged(nameof(ConfirmActivePointCloud));
			OnPropertyChanged(nameof(ConfirmCalibration));
			OnPropertyChanged(nameof(Coord));
			OnPropertyChanged(nameof(CoordRef));
			OnPropertyChanged(nameof(Copy));
			OnPropertyChanged(nameof(CopyCreate));
			OnPropertyChanged(nameof(Create));
			OnPropertyChanged(nameof(CreateProject));
			OnPropertyChanged(nameof(CreateProjectReplaceCurrent));
			OnPropertyChanged(nameof(Cuboid));
			OnPropertyChanged(nameof(CuboidTransform));
			OnPropertyChanged(nameof(CurrentProject));
			OnPropertyChanged(nameof(Custom));
			OnPropertyChanged(nameof(Cylinder));
			OnPropertyChanged(nameof(Cylindricity));
			OnPropertyChanged(nameof(DataDirectory));
			OnPropertyChanged(nameof(DataProcess));
			OnPropertyChanged(nameof(Delete));
			OnPropertyChanged(nameof(Denoising));
			OnPropertyChanged(nameof(DetectionDepth));
			OnPropertyChanged(nameof(DigitalPoint));
			OnPropertyChanged(nameof(DisCircleAttribute));
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
			OnPropertyChanged(nameof(DoublePage));
			OnPropertyChanged(nameof(DownSampling));
			OnPropertyChanged(nameof(DrawCuboidROI));
			OnPropertyChanged(nameof(DrawCylinderROI));
			OnPropertyChanged(nameof(DrawSphereROI));
			OnPropertyChanged(nameof(Edit));
			OnPropertyChanged(nameof(EditFeature));
			OnPropertyChanged(nameof(Error));
			OnPropertyChanged(nameof(ErrorImgPath));
			OnPropertyChanged(nameof(ErrorImgSize));
			OnPropertyChanged(nameof(Export));
			OnPropertyChanged(nameof(ExpressDialog));
			OnPropertyChanged(nameof(ExtractAsyncGroup));
			OnPropertyChanged(nameof(ExtractBranchGroup));
			OnPropertyChanged(nameof(ExtractStepGroup));
			OnPropertyChanged(nameof(ExtractSwitchGroup));
			OnPropertyChanged(nameof(FallbackDistance));
			OnPropertyChanged(nameof(File));
			OnPropertyChanged(nameof(FileAddress));
			OnPropertyChanged(nameof(FileIO));
			OnPropertyChanged(nameof(Filtering));
			OnPropertyChanged(nameof(Find));
			OnPropertyChanged(nameof(Finish));
			OnPropertyChanged(nameof(Flatness));
			OnPropertyChanged(nameof(ForegroundAndBackground));
			OnPropertyChanged(nameof(FormatError));
			OnPropertyChanged(nameof(ForMemoryLeakDetection));
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
			OnPropertyChanged(nameof(GenerationMethodPoint));
			OnPropertyChanged(nameof(GenerationMode));
			OnPropertyChanged(nameof(GenGeomFromPoint));
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
			OnPropertyChanged(nameof(GenPointByPlaneZ));
			OnPropertyChanged(nameof(GenPointByProj));
			OnPropertyChanged(nameof(GenReport));
			OnPropertyChanged(nameof(GenSphereByData));
			OnPropertyChanged(nameof(GenSphereByFit));
			OnPropertyChanged(nameof(GeometricFeatures));
			OnPropertyChanged(nameof(GeometricFeaturesInclude));
			OnPropertyChanged(nameof(Geometry));
			OnPropertyChanged(nameof(GetDirectionByObj));
			OnPropertyChanged(nameof(GetLineByObj));
			OnPropertyChanged(nameof(GetPlaneByObj));
			OnPropertyChanged(nameof(GetPointByObj));
			OnPropertyChanged(nameof(HDirection));
			OnPropertyChanged(nameof(Height));
			OnPropertyChanged(nameof(HeightCorSpect));
			OnPropertyChanged(nameof(Help));
			OnPropertyChanged(nameof(HideBoundBox));
			OnPropertyChanged(nameof(HideLabel));
			OnPropertyChanged(nameof(HomePage));
			OnPropertyChanged(nameof(ImportParameterName));
			OnPropertyChanged(nameof(Inclination));
			OnPropertyChanged(nameof(InCloud));
			OnPropertyChanged(nameof(Info));
			OnPropertyChanged(nameof(InOutParameterConfigure));
			OnPropertyChanged(nameof(Input));
			OnPropertyChanged(nameof(InputParameter));
			OnPropertyChanged(nameof(Insert));
			OnPropertyChanged(nameof(InsertPoint));
			OnPropertyChanged(nameof(Interval10m));
			OnPropertyChanged(nameof(Interval1h));
			OnPropertyChanged(nameof(Interval1m));
			OnPropertyChanged(nameof(Interval2h));
			OnPropertyChanged(nameof(Interval30m));
			OnPropertyChanged(nameof(Interval30s));
			OnPropertyChanged(nameof(Interval5m));
			OnPropertyChanged(nameof(IsGenerateMeasurement));
			OnPropertyChanged(nameof(IsHomeDefault));
			OnPropertyChanged(nameof(IsMemoric));
			OnPropertyChanged(nameof(IsNecessary));
			OnPropertyChanged(nameof(Jump));
			OnPropertyChanged(nameof(KeywordMatching));
			OnPropertyChanged(nameof(LangComment));
			OnPropertyChanged(nameof(LDirection));
			OnPropertyChanged(nameof(Lead));
			OnPropertyChanged(nameof(LightingSettings));
			OnPropertyChanged(nameof(Line));
			OnPropertyChanged(nameof(LineProfile));
			OnPropertyChanged(nameof(LineScale));
			OnPropertyChanged(nameof(LineWidth));
			OnPropertyChanged(nameof(Load));
			OnPropertyChanged(nameof(Loading));
			OnPropertyChanged(nameof(Logic));
			OnPropertyChanged(nameof(Loop));
			OnPropertyChanged(nameof(LowerLimit));
			OnPropertyChanged(nameof(MeasureBenchmarkⅠ));
			OnPropertyChanged(nameof(MeasureBenchmarkⅡ));
			OnPropertyChanged(nameof(MeasureType));
			OnPropertyChanged(nameof(MergePoints));
			OnPropertyChanged(nameof(Mesh));
			OnPropertyChanged(nameof(Miscellaneous));
			OnPropertyChanged(nameof(ModbusRTU));
			OnPropertyChanged(nameof(ModelDisplayMode));
			OnPropertyChanged(nameof(ModelGroup));
			OnPropertyChanged(nameof(Module));
			OnPropertyChanged(nameof(ModuleName));
			OnPropertyChanged(nameof(Name));
			OnPropertyChanged(nameof(NextPage));
			OnPropertyChanged(nameof(No));
			OnPropertyChanged(nameof(NoData));
			OnPropertyChanged(nameof(NoDigitalPointsUpdated));
			OnPropertyChanged(nameof(NoMeasurementUnderNode));
			OnPropertyChanged(nameof(NumberOfCycles));
			OnPropertyChanged(nameof(NumberOfDataPoints));
			OnPropertyChanged(nameof(Ok));
			OnPropertyChanged(nameof(Opacity));
			OnPropertyChanged(nameof(Open));
			OnPropertyChanged(nameof(OpenProject));
			OnPropertyChanged(nameof(Operate));
			OnPropertyChanged(nameof(OutlierFit));
			OnPropertyChanged(nameof(OutOfRange));
			OnPropertyChanged(nameof(OutParam));
			OnPropertyChanged(nameof(OutportParameterName));
			OnPropertyChanged(nameof(Output));
			OnPropertyChanged(nameof(OutputParameter));
			OnPropertyChanged(nameof(PageMode));
			OnPropertyChanged(nameof(Parallelism));
			OnPropertyChanged(nameof(ParameterConfig));
			OnPropertyChanged(nameof(Paste));
			OnPropertyChanged(nameof(Patch));
			OnPropertyChanged(nameof(Pause));
			OnPropertyChanged(nameof(PickLine));
			OnPropertyChanged(nameof(PickPlane));
			OnPropertyChanged(nameof(PickPoint));
			OnPropertyChanged(nameof(Plane));
			OnPropertyChanged(nameof(PleaseCheckTheDataFormatorType));
			OnPropertyChanged(nameof(PleaseEnterAnIntegerGreaterThan0));
			OnPropertyChanged(nameof(PleaseEnterASingleByteDelimiter));
			OnPropertyChanged(nameof(Pm));
			OnPropertyChanged(nameof(PngImg));
			OnPropertyChanged(nameof(Point));
			OnPropertyChanged(nameof(Point2Point));
			OnPropertyChanged(nameof(PointCloud));
			OnPropertyChanged(nameof(PointCloudDisplayMode));
			OnPropertyChanged(nameof(PointCloudGroup));
			OnPropertyChanged(nameof(PointCloudSize));
			OnPropertyChanged(nameof(PointDirection));
			OnPropertyChanged(nameof(PointerCoord));
			OnPropertyChanged(nameof(PointSize));
			OnPropertyChanged(nameof(PointToPoint));
			OnPropertyChanged(nameof(Preview));
			OnPropertyChanged(nameof(PreviewReport));
			OnPropertyChanged(nameof(PreviousPage));
			OnPropertyChanged(nameof(Print));
			OnPropertyChanged(nameof(PrintPreview));
			OnPropertyChanged(nameof(PrintSet));
			OnPropertyChanged(nameof(Profileanysurface));
			OnPropertyChanged(nameof(Project));
			OnPropertyChanged(nameof(ProjectAddress));
			OnPropertyChanged(nameof(ProjectAlreadyExists));
			OnPropertyChanged(nameof(ProjectCannotBeEmpty));
			OnPropertyChanged(nameof(ProjectName));
			OnPropertyChanged(nameof(ProjectProperty));
			OnPropertyChanged(nameof(Property));
			OnPropertyChanged(nameof(Quit));
			OnPropertyChanged(nameof(QuitSoftWare));
			OnPropertyChanged(nameof(Radius));
			OnPropertyChanged(nameof(ReadCAD));
			OnPropertyChanged(nameof(ReadCloud));
			OnPropertyChanged(nameof(ReadDatas));
			OnPropertyChanged(nameof(ReadDirection));
			OnPropertyChanged(nameof(ReadMatrix));
			OnPropertyChanged(nameof(ReadPoint));
			OnPropertyChanged(nameof(ReadSTL));
			OnPropertyChanged(nameof(RecentFile));
			OnPropertyChanged(nameof(RecentProject));
			OnPropertyChanged(nameof(Redo));
			OnPropertyChanged(nameof(RefTemplate));
			OnPropertyChanged(nameof(Registration));
			OnPropertyChanged(nameof(ReleaseNotes));
			OnPropertyChanged(nameof(Remove));
			OnPropertyChanged(nameof(Rename));
			OnPropertyChanged(nameof(Report));
			OnPropertyChanged(nameof(ReportContents));
			OnPropertyChanged(nameof(ReportName));
			OnPropertyChanged(nameof(ReportNavigation));
			OnPropertyChanged(nameof(ReportSource));
			OnPropertyChanged(nameof(RepTemplate));
			OnPropertyChanged(nameof(Revoke));
			OnPropertyChanged(nameof(RobotMove));
			OnPropertyChanged(nameof(ROIConfig));
			OnPropertyChanged(nameof(ROIType));
			OnPropertyChanged(nameof(RotateAroundX));
			OnPropertyChanged(nameof(RotateAroundY));
			OnPropertyChanged(nameof(RotateAroundZ));
			OnPropertyChanged(nameof(RotateViewClockwise));
			OnPropertyChanged(nameof(RotateViewCounterclockwise));
			OnPropertyChanged(nameof(Roundness));
			OnPropertyChanged(nameof(Run));
			OnPropertyChanged(nameof(RunNext));
			OnPropertyChanged(nameof(RunOne));
			OnPropertyChanged(nameof(Save));
			OnPropertyChanged(nameof(SaveAs));
			OnPropertyChanged(nameof(SaveAsProject));
			OnPropertyChanged(nameof(SaveProject));
			OnPropertyChanged(nameof(SaveProjectTask));
			OnPropertyChanged(nameof(SaveReportAsCompleted));
			OnPropertyChanged(nameof(SaveSuccess));
			OnPropertyChanged(nameof(ScreenShot));
			OnPropertyChanged(nameof(ScrollMode));
			OnPropertyChanged(nameof(SearchFileKeywords));
			OnPropertyChanged(nameof(Segment));
			OnPropertyChanged(nameof(Select));
			OnPropertyChanged(nameof(SelectCalibratePointCloud));
			OnPropertyChanged(nameof(SelectCorrectMeasurementBenchmark));
			OnPropertyChanged(nameof(Semicolon));
			OnPropertyChanged(nameof(Senior));
			OnPropertyChanged(nameof(Separator));
			OnPropertyChanged(nameof(SerialNumber));
			OnPropertyChanged(nameof(SetCoordTemplate));
			OnPropertyChanged(nameof(SetDisPlayOption));
			OnPropertyChanged(nameof(SetMeasure));
			OnPropertyChanged(nameof(SetSinglePage));
			OnPropertyChanged(nameof(ShowBoundBox));
			OnPropertyChanged(nameof(SinglePage));
			OnPropertyChanged(nameof(Size));
			OnPropertyChanged(nameof(Skip));
			OnPropertyChanged(nameof(SmokeAlarmDevice));
			OnPropertyChanged(nameof(Smooth));
			OnPropertyChanged(nameof(SoftwareFunctions));
			OnPropertyChanged(nameof(SoftWareUpdateInfo));
			OnPropertyChanged(nameof(SoftWareVersion));
			OnPropertyChanged(nameof(Solution));
			OnPropertyChanged(nameof(Space));
			OnPropertyChanged(nameof(Sphere));
			OnPropertyChanged(nameof(SplitIntToBit));
			OnPropertyChanged(nameof(StandardValue));
			OnPropertyChanged(nameof(StartColumn));
			OnPropertyChanged(nameof(StartRow));
			OnPropertyChanged(nameof(Status));
			OnPropertyChanged(nameof(Step));
			OnPropertyChanged(nameof(StepGroup));
			OnPropertyChanged(nameof(STL));
			OnPropertyChanged(nameof(StorageAddress));
			OnPropertyChanged(nameof(Straightness));
			OnPropertyChanged(nameof(StringExDialog));
			OnPropertyChanged(nameof(StringMatchDialog));
			OnPropertyChanged(nameof(StringMerge));
			OnPropertyChanged(nameof(StringParse));
			OnPropertyChanged(nameof(SurfaceDistance));
			OnPropertyChanged(nameof(SurfaceProfile));
			OnPropertyChanged(nameof(Switch));
			OnPropertyChanged(nameof(SwitchDialog));
			OnPropertyChanged(nameof(SwitchGroup));
			OnPropertyChanged(nameof(TaskFlow));
			OnPropertyChanged(nameof(TaskSimulator));
			OnPropertyChanged(nameof(ThreeGroups));
			OnPropertyChanged(nameof(Time));
			OnPropertyChanged(nameof(Tip));
			OnPropertyChanged(nameof(Title));
			OnPropertyChanged(nameof(Tolerance));
			OnPropertyChanged(nameof(ToleranceMeasurementsInclude));
			OnPropertyChanged(nameof(TolParameter));
			OnPropertyChanged(nameof(Tool));
			OnPropertyChanged(nameof(TooLarge));
			OnPropertyChanged(nameof(TotalTime));
			OnPropertyChanged(nameof(Train));
			OnPropertyChanged(nameof(Transform));
			OnPropertyChanged(nameof(TransformCoord));
			OnPropertyChanged(nameof(TutorialDontExist));
			OnPropertyChanged(nameof(TwoPageMode));
			OnPropertyChanged(nameof(TxtReader));
			OnPropertyChanged(nameof(TxtWriter));
			OnPropertyChanged(nameof(Type));
			OnPropertyChanged(nameof(Unknown));
			OnPropertyChanged(nameof(UnknownSize));
			OnPropertyChanged(nameof(Update));
			OnPropertyChanged(nameof(UpperLimit));
			OnPropertyChanged(nameof(UsingTutorials));
			OnPropertyChanged(nameof(Value));
			OnPropertyChanged(nameof(VersionInfoDialog));
			OnPropertyChanged(nameof(Vertex));
			OnPropertyChanged(nameof(Verticality));
			OnPropertyChanged(nameof(View));
			OnPropertyChanged(nameof(ViewCentered));
			OnPropertyChanged(nameof(ViewDirectionSetting));
			OnPropertyChanged(nameof(ViewFlip));
			OnPropertyChanged(nameof(VisibleLabel));
			OnPropertyChanged(nameof(Warning));
			OnPropertyChanged(nameof(WDirection));
			OnPropertyChanged(nameof(Wireframe));
			OnPropertyChanged(nameof(Yes));
			OnPropertyChanged(nameof(Zoom));
			OnPropertyChanged(nameof(ZoomIn));
			OnPropertyChanged(nameof(ZoomOut));
        }

        /// <summary>
        ///   查找类似 激活 的本地化字符串。
        /// </summary>
		public string Active => Lang.Active;

        /// <summary>
        ///   查找类似 新增 的本地化字符串。
        /// </summary>
		public string Add => Lang.Add;

        /// <summary>
        ///   查找类似 新增参数 的本地化字符串。
        /// </summary>
		public string AddInParam => Lang.AddInParam;

        /// <summary>
        ///   查找类似 2.将模型和点云调整到相同视角 的本地化字符串。
        /// </summary>
		public string AdjustModelAndPointCloud => Lang.AdjustModelAndPointCloud;

        /// <summary>
        ///   查找类似 别名不可重复 的本地化字符串。
        /// </summary>
		public string AliasCannotDuplicate => Lang.AliasCannotDuplicate;

        /// <summary>
        ///   查找类似 别名不可为空 的本地化字符串。
        /// </summary>
		public string AliasCannotEmpty => Lang.AliasCannotEmpty;

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
        ///   查找类似 上午 的本地化字符串。
        /// </summary>
		public string Am => Lang.Am;

        /// <summary>
        ///   查找类似 角度测量 的本地化字符串。
        /// </summary>
		public string AngleMeasure => Lang.AngleMeasure;

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
        ///   查找类似 文件整理 的本地化字符串。
        /// </summary>
		public string ArrangeDocument => Lang.ArrangeDocument;

        /// <summary>
        ///   查找类似 并行任务 的本地化字符串。
        /// </summary>
		public string AsyncGroup => Lang.AsyncGroup;

        /// <summary>
        ///   查找类似 平均耗时 的本地化字符串。
        /// </summary>
		public string AverageTime => Lang.AverageTime;

        /// <summary>
        ///   查找类似 轴线方向 的本地化字符串。
        /// </summary>
		public string AxisDirection => Lang.AxisDirection;

        /// <summary>
        ///   查找类似 批量修改几何体属性 的本地化字符串。
        /// </summary>
		public string BatchChangeGeometry => Lang.BatchChangeGeometry;

        /// <summary>
        ///   查找类似 批量创建测量项 的本地化字符串。
        /// </summary>
		public string BatchCreateMeasurements => Lang.BatchCreateMeasurements;

        /// <summary>
        ///   查找类似 批量生成测量点 的本地化字符串。
        /// </summary>
		public string BatchGenMeaPoints => Lang.BatchGenMeaPoints;

        /// <summary>
        ///   查找类似 批量生成点 的本地化字符串。
        /// </summary>
		public string BatchGenPoints => Lang.BatchGenPoints;

        /// <summary>
        ///   查找类似 批量生成几何体 的本地化字符串。
        /// </summary>
		public string BatchGeometry => Lang.BatchGeometry;

        /// <summary>
        ///   查找类似 批量导入点 的本地化字符串。
        /// </summary>
		public string BatchImportPoints => Lang.BatchImportPoints;

        /// <summary>
        ///   查找类似 批量加载点云，在设置输入参数时有效！ 的本地化字符串。
        /// </summary>
		public string BatchLoadingPointClouds => Lang.BatchLoadingPointClouds;

        /// <summary>
        ///   查找类似 最佳拟合 的本地化字符串。
        /// </summary>
		public string BestFit => Lang.BestFit;

        /// <summary>
        ///   查找类似 包围盒 的本地化字符串。
        /// </summary>
		public string BoundBox => Lang.BoundBox;

        /// <summary>
        ///   查找类似 分支 的本地化字符串。
        /// </summary>
		public string Branch => Lang.Branch;

        /// <summary>
        ///   查找类似 判断分支 的本地化字符串。
        /// </summary>
		public string BranchGroup => Lang.BranchGroup;

        /// <summary>
        ///   查找类似 CAD模型 的本地化字符串。
        /// </summary>
		public string CADModel => Lang.CADModel;

        /// <summary>
        ///   查找类似 计算器 的本地化字符串。
        /// </summary>
		public string Calculator => Lang.Calculator;

        /// <summary>
        ///   查找类似 标定 的本地化字符串。
        /// </summary>
		public string Calib => Lang.Calib;

        /// <summary>
        ///   查找类似 标定操作说明 的本地化字符串。
        /// </summary>
		public string CalibrationOperationInstructions => Lang.CalibrationOperationInstructions;

        /// <summary>
        ///   查找类似 取消 的本地化字符串。
        /// </summary>
		public string Cancel => Lang.Cancel;

        /// <summary>
        ///   查找类似 5.“取消”按钮，取消标定 的本地化字符串。
        /// </summary>
		public string CancelCalibration => Lang.CancelCalibration;

        /// <summary>
        ///   查找类似 取消模板 的本地化字符串。
        /// </summary>
		public string CancelCoordTemplate => Lang.CancelCoordTemplate;

        /// <summary>
        ///   查找类似 取消忽略 的本地化字符串。
        /// </summary>
		public string CancelSkip => Lang.CancelSkip;

        /// <summary>
        ///   查找类似 中心点 的本地化字符串。
        /// </summary>
		public string CenterPoint => Lang.CenterPoint;

        /// <summary>
        ///   查找类似 存在基准集合为空，请检查 的本地化字符串。
        /// </summary>
		public string CheckBenchmarkEmpty => Lang.CheckBenchmarkEmpty;

        /// <summary>
        ///   查找类似 检查更新 的本地化字符串。
        /// </summary>
		public string CheckForUpdates => Lang.CheckForUpdates;

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public string Choose => Lang.Choose;

        /// <summary>
        ///   查找类似 圆 的本地化字符串。
        /// </summary>
		public string Circle => Lang.Circle;

        /// <summary>
        ///   查找类似 清空 的本地化字符串。
        /// </summary>
		public string Clear => Lang.Clear;

        /// <summary>
        ///   查找类似 是否确定清空基准参数 的本地化字符串。
        /// </summary>
		public string ClearBenchmarkParameters => Lang.ClearBenchmarkParameters;

        /// <summary>
        ///   查找类似 确定清空输入输出参数 的本地化字符串。
        /// </summary>
		public string ClearInOutParameters => Lang.ClearInOutParameters;

        /// <summary>
        ///   查找类似 3.“点选”按钮，依次在模型和点云上点击 的本地化字符串。
        /// </summary>
		public string ClickModelAndPointCloud => Lang.ClickModelAndPointCloud;

        /// <summary>
        ///   查找类似 关闭 的本地化字符串。
        /// </summary>
		public string Close => Lang.Close;

        /// <summary>
        ///   查找类似 关闭所有 的本地化字符串。
        /// </summary>
		public string CloseAll => Lang.CloseAll;

        /// <summary>
        ///   查找类似 7.“关闭”按钮，退出当前页面 的本地化字符串。
        /// </summary>
		public string CloseCalibration => Lang.CloseCalibration;

        /// <summary>
        ///   查找类似 关闭其他 的本地化字符串。
        /// </summary>
		public string CloseOther => Lang.CloseOther;

        /// <summary>
        ///   查找类似 点云对其匹配 的本地化字符串。
        /// </summary>
		public string CloudAlignMatch => Lang.CloudAlignMatch;

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
        ///   查找类似 点云边缘提取 的本地化字符串。
        /// </summary>
		public string CloudExtractEdge => Lang.CloudExtractEdge;

        /// <summary>
        ///   查找类似 网格比较 的本地化字符串。
        /// </summary>
		public string CloudMesh => Lang.CloudMesh;

        /// <summary>
        ///   查找类似 点云法向信息 的本地化字符串。
        /// </summary>
		public string CloudNormal => Lang.CloudNormal;

        /// <summary>
        ///   查找类似 点云处理 的本地化字符串。
        /// </summary>
		public string CloudProcess => Lang.CloudProcess;

        /// <summary>
        ///   查找类似 点云投影面 的本地化字符串。
        /// </summary>
		public string CloudProjPlane => Lang.CloudProjPlane;

        /// <summary>
        ///   查找类似 点云配准 的本地化字符串。
        /// </summary>
		public string CloudRegistration => Lang.CloudRegistration;

        /// <summary>
        ///   查找类似 点云去重 的本地化字符串。
        /// </summary>
		public string CloudReRepeat => Lang.CloudReRepeat;

        /// <summary>
        ///   查找类似 点云多截面分割 的本地化字符串。
        /// </summary>
		public string CloudSectionSegment => Lang.CloudSectionSegment;

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
        ///   查找类似 碰撞方向 的本地化字符串。
        /// </summary>
		public string CollisionDirection => Lang.CollisionDirection;

        /// <summary>
        ///   查找类似 颜色 的本地化字符串。
        /// </summary>
		public string Color => Lang.Color;

        /// <summary>
        ///   查找类似 逗号 的本地化字符串。
        /// </summary>
		public string Comma => Lang.Comma;

        /// <summary>
        ///   查找类似 比较 的本地化字符串。
        /// </summary>
		public string Compare => Lang.Compare;

        /// <summary>
        ///   查找类似 排版 的本地化字符串。
        /// </summary>
		public string Composing => Lang.Composing;

        /// <summary>
        ///   查找类似 圆锥 的本地化字符串。
        /// </summary>
		public string Cone => Lang.Cone;

        /// <summary>
        ///   查找类似 请先配置输入参数(点云) 的本地化字符串。
        /// </summary>
		public string ConfigureInputParametersFirst => Lang.ConfigureInputParametersFirst;

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public string Confirm => Lang.Confirm;

        /// <summary>
        ///   查找类似 确认任务中存在从文件加载的激活点云 的本地化字符串。
        /// </summary>
		public string ConfirmActivePointCloud => Lang.ConfirmActivePointCloud;

        /// <summary>
        ///   查找类似 6.“确认”按钮，完成当前点云标定 的本地化字符串。
        /// </summary>
		public string ConfirmCalibration => Lang.ConfirmCalibration;

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
        ///   查找类似 新建 的本地化字符串。
        /// </summary>
		public string Create => Lang.Create;

        /// <summary>
        ///   查找类似 创建项目 的本地化字符串。
        /// </summary>
		public string CreateProject => Lang.CreateProject;

        /// <summary>
        ///   查找类似 当前项目源文件已删除，请先新建项目代替当前项目 的本地化字符串。
        /// </summary>
		public string CreateProjectReplaceCurrent => Lang.CreateProjectReplaceCurrent;

        /// <summary>
        ///   查找类似 长方体 的本地化字符串。
        /// </summary>
		public string Cuboid => Lang.Cuboid;

        /// <summary>
        ///   查找类似 长方体变换 的本地化字符串。
        /// </summary>
		public string CuboidTransform => Lang.CuboidTransform;

        /// <summary>
        ///   查找类似 最 近 项 目 的本地化字符串。
        /// </summary>
		public string CurrentProject => Lang.CurrentProject;

        /// <summary>
        ///   查找类似 自定义 的本地化字符串。
        /// </summary>
		public string Custom => Lang.Custom;

        /// <summary>
        ///   查找类似 圆柱 的本地化字符串。
        /// </summary>
		public string Cylinder => Lang.Cylinder;

        /// <summary>
        ///   查找类似 圆柱度 的本地化字符串。
        /// </summary>
		public string Cylindricity => Lang.Cylindricity;

        /// <summary>
        ///   查找类似 数据目录 的本地化字符串。
        /// </summary>
		public string DataDirectory => Lang.DataDirectory;

        /// <summary>
        ///   查找类似 数据处理 的本地化字符串。
        /// </summary>
		public string DataProcess => Lang.DataProcess;

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public string Delete => Lang.Delete;

        /// <summary>
        ///   查找类似 去噪 的本地化字符串。
        /// </summary>
		public string Denoising => Lang.Denoising;

        /// <summary>
        ///   查找类似 探测深度 的本地化字符串。
        /// </summary>
		public string DetectionDepth => Lang.DetectionDepth;

        /// <summary>
        ///   查找类似 数字点 的本地化字符串。
        /// </summary>
		public string DigitalPoint => Lang.DigitalPoint;

        /// <summary>
        ///   查找类似 圆尺寸测量 的本地化字符串。
        /// </summary>
		public string DisCircleAttribute => Lang.DisCircleAttribute;

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
        ///   查找类似 双页 的本地化字符串。
        /// </summary>
		public string DoublePage => Lang.DoublePage;

        /// <summary>
        ///   查找类似 下采样 的本地化字符串。
        /// </summary>
		public string DownSampling => Lang.DownSampling;

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
        ///   查找类似 编辑 的本地化字符串。
        /// </summary>
		public string Edit => Lang.Edit;

        /// <summary>
        ///   查找类似 编辑特征 的本地化字符串。
        /// </summary>
		public string EditFeature => Lang.EditFeature;

        /// <summary>
        ///   查找类似 错误 的本地化字符串。
        /// </summary>
		public string Error => Lang.Error;

        /// <summary>
        ///   查找类似 错误的图片路径 的本地化字符串。
        /// </summary>
		public string ErrorImgPath => Lang.ErrorImgPath;

        /// <summary>
        ///   查找类似 非法的图片尺寸 的本地化字符串。
        /// </summary>
		public string ErrorImgSize => Lang.ErrorImgSize;

        /// <summary>
        ///   查找类似 导出 的本地化字符串。
        /// </summary>
		public string Export => Lang.Export;

        /// <summary>
        ///   查找类似 表达式对话框 的本地化字符串。
        /// </summary>
		public string ExpressDialog => Lang.ExpressDialog;

        /// <summary>
        ///   查找类似 提取异步组 的本地化字符串。
        /// </summary>
		public string ExtractAsyncGroup => Lang.ExtractAsyncGroup;

        /// <summary>
        ///   查找类似 提取分支组 的本地化字符串。
        /// </summary>
		public string ExtractBranchGroup => Lang.ExtractBranchGroup;

        /// <summary>
        ///   查找类似 提取串行组 的本地化字符串。
        /// </summary>
		public string ExtractStepGroup => Lang.ExtractStepGroup;

        /// <summary>
        ///   查找类似 提取条件组 的本地化字符串。
        /// </summary>
		public string ExtractSwitchGroup => Lang.ExtractSwitchGroup;

        /// <summary>
        ///   查找类似 回退距离 的本地化字符串。
        /// </summary>
		public string FallbackDistance => Lang.FallbackDistance;

        /// <summary>
        ///   查找类似 文件 的本地化字符串。
        /// </summary>
		public string File => Lang.File;

        /// <summary>
        ///   查找类似 文件地址 的本地化字符串。
        /// </summary>
		public string FileAddress => Lang.FileAddress;

        /// <summary>
        ///   查找类似 文件输入 的本地化字符串。
        /// </summary>
		public string FileIO => Lang.FileIO;

        /// <summary>
        ///   查找类似 滤波 的本地化字符串。
        /// </summary>
		public string Filtering => Lang.Filtering;

        /// <summary>
        ///   查找类似 查找 的本地化字符串。
        /// </summary>
		public string Find => Lang.Find;

        /// <summary>
        ///   查找类似 完 成 的本地化字符串。
        /// </summary>
		public string Finish => Lang.Finish;

        /// <summary>
        ///   查找类似 平面度 的本地化字符串。
        /// </summary>
		public string Flatness => Lang.Flatness;

        /// <summary>
        ///   查找类似 4.亮色为前景色，黑色为背景色 的本地化字符串。
        /// </summary>
		public string ForegroundAndBackground => Lang.ForegroundAndBackground;

        /// <summary>
        ///   查找类似 格式错误 的本地化字符串。
        /// </summary>
		public string FormatError => Lang.FormatError;

        /// <summary>
        ///   查找类似 用于内存泄露检测 的本地化字符串。
        /// </summary>
		public string ForMemoryLeakDetection => Lang.ForMemoryLeakDetection;

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
        ///   查找类似 生成方式需设置为：点 的本地化字符串。
        /// </summary>
		public string GenerationMethodPoint => Lang.GenerationMethodPoint;

        /// <summary>
        ///   查找类似 生成方式 的本地化字符串。
        /// </summary>
		public string GenerationMode => Lang.GenerationMode;

        /// <summary>
        ///   查找类似 由点生成几何体 的本地化字符串。
        /// </summary>
		public string GenGeomFromPoint => Lang.GenGeomFromPoint;

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
        ///   查找类似 面Z获取 的本地化字符串。
        /// </summary>
		public string GenPointByPlaneZ => Lang.GenPointByPlaneZ;

        /// <summary>
        ///   查找类似 求投影点 的本地化字符串。
        /// </summary>
		public string GenPointByProj => Lang.GenPointByProj;

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
        ///   查找类似 几何特征 的本地化字符串。
        /// </summary>
		public string GeometricFeatures => Lang.GeometricFeatures;

        /// <summary>
        ///   查找类似 几何特征新建包含：点、线、面、长方体、圆柱、圆锥、球等 的本地化字符串。
        /// </summary>
		public string GeometricFeaturesInclude => Lang.GeometricFeaturesInclude;

        /// <summary>
        ///   查找类似 几何 的本地化字符串。
        /// </summary>
		public string Geometry => Lang.Geometry;

        /// <summary>
        ///   查找类似 获取方向 的本地化字符串。
        /// </summary>
		public string GetDirectionByObj => Lang.GetDirectionByObj;

        /// <summary>
        ///   查找类似 获取线 的本地化字符串。
        /// </summary>
		public string GetLineByObj => Lang.GetLineByObj;

        /// <summary>
        ///   查找类似 获取面 的本地化字符串。
        /// </summary>
		public string GetPlaneByObj => Lang.GetPlaneByObj;

        /// <summary>
        ///   查找类似 获取点 的本地化字符串。
        /// </summary>
		public string GetPointByObj => Lang.GetPointByObj;

        /// <summary>
        ///   查找类似 高方向 的本地化字符串。
        /// </summary>
		public string HDirection => Lang.HDirection;

        /// <summary>
        ///   查找类似 高 的本地化字符串。
        /// </summary>
		public string Height => Lang.Height;

        /// <summary>
        ///   查找类似 高度色谱 的本地化字符串。
        /// </summary>
		public string HeightCorSpect => Lang.HeightCorSpect;

        /// <summary>
        ///   查找类似 帮助 的本地化字符串。
        /// </summary>
		public string Help => Lang.Help;

        /// <summary>
        ///   查找类似 隐藏包围盒 的本地化字符串。
        /// </summary>
		public string HideBoundBox => Lang.HideBoundBox;

        /// <summary>
        ///   查找类似 隐藏标签 的本地化字符串。
        /// </summary>
		public string HideLabel => Lang.HideLabel;

        /// <summary>
        ///   查找类似 起始页 的本地化字符串。
        /// </summary>
		public string HomePage => Lang.HomePage;

        /// <summary>
        ///   查找类似 输入参数别名 的本地化字符串。
        /// </summary>
		public string ImportParameterName => Lang.ImportParameterName;

        /// <summary>
        ///   查找类似 倾斜度 的本地化字符串。
        /// </summary>
		public string Inclination => Lang.Inclination;

        /// <summary>
        ///   查找类似 内部点云 的本地化字符串。
        /// </summary>
		public string InCloud => Lang.InCloud;

        /// <summary>
        ///   查找类似 信息 的本地化字符串。
        /// </summary>
		public string Info => Lang.Info;

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
        ///   查找类似 插入 的本地化字符串。
        /// </summary>
		public string Insert => Lang.Insert;

        /// <summary>
        ///   查找类似 插入点 的本地化字符串。
        /// </summary>
		public string InsertPoint => Lang.InsertPoint;

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
        ///   查找类似 基准Ⅰ集合与基准Ⅱ集合个数不同，确认生成测量项 的本地化字符串。
        /// </summary>
		public string IsGenerateMeasurement => Lang.IsGenerateMeasurement;

        /// <summary>
        ///   查找类似 回零恢复默认值 的本地化字符串。
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
        ///   查找类似 跳转 的本地化字符串。
        /// </summary>
		public string Jump => Lang.Jump;

        /// <summary>
        ///   查找类似 关键字匹配 的本地化字符串。
        /// </summary>
		public string KeywordMatching => Lang.KeywordMatching;

        /// <summary>
        ///   查找类似 查找类似 {0} 的本地化字符串。 的本地化字符串。
        /// </summary>
		public string LangComment => Lang.LangComment;

        /// <summary>
        ///   查找类似 长方向 的本地化字符串。
        /// </summary>
		public string LDirection => Lang.LDirection;

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public string Lead => Lang.Lead;

        /// <summary>
        ///   查找类似 光源设置 的本地化字符串。
        /// </summary>
		public string LightingSettings => Lang.LightingSettings;

        /// <summary>
        ///   查找类似 线 的本地化字符串。
        /// </summary>
		public string Line => Lang.Line;

        /// <summary>
        ///   查找类似 线轮廓度 的本地化字符串。
        /// </summary>
		public string LineProfile => Lang.LineProfile;

        /// <summary>
        ///   查找类似 线延长比 的本地化字符串。
        /// </summary>
		public string LineScale => Lang.LineScale;

        /// <summary>
        ///   查找类似 线宽 的本地化字符串。
        /// </summary>
		public string LineWidth => Lang.LineWidth;

        /// <summary>
        ///   查找类似 加载 的本地化字符串。
        /// </summary>
		public string Load => Lang.Load;

        /// <summary>
        ///   查找类似 运行中 的本地化字符串。
        /// </summary>
		public string Loading => Lang.Loading;

        /// <summary>
        ///   查找类似 逻辑 的本地化字符串。
        /// </summary>
		public string Logic => Lang.Logic;

        /// <summary>
        ///   查找类似 循环 的本地化字符串。
        /// </summary>
		public string Loop => Lang.Loop;

        /// <summary>
        ///   查找类似 公差下限 的本地化字符串。
        /// </summary>
		public string LowerLimit => Lang.LowerLimit;

        /// <summary>
        ///   查找类似 测量基准Ⅰ 的本地化字符串。
        /// </summary>
		public string MeasureBenchmarkⅠ => Lang.MeasureBenchmarkⅠ;

        /// <summary>
        ///   查找类似 测量基准Ⅱ 的本地化字符串。
        /// </summary>
		public string MeasureBenchmarkⅡ => Lang.MeasureBenchmarkⅡ;

        /// <summary>
        ///   查找类似 测量类型 的本地化字符串。
        /// </summary>
		public string MeasureType => Lang.MeasureType;

        /// <summary>
        ///   查找类似 点合并 的本地化字符串。
        /// </summary>
		public string MergePoints => Lang.MergePoints;

        /// <summary>
        ///   查找类似 网格数据 的本地化字符串。
        /// </summary>
		public string Mesh => Lang.Mesh;

        /// <summary>
        ///   查找类似 杂项 的本地化字符串。
        /// </summary>
		public string Miscellaneous => Lang.Miscellaneous;

        /// <summary>
        ///   查找类似 ModbusRTU 的本地化字符串。
        /// </summary>
		public string ModbusRTU => Lang.ModbusRTU;

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
        ///   查找类似 模块名称 的本地化字符串。
        /// </summary>
		public string ModuleName => Lang.ModuleName;

        /// <summary>
        ///   查找类似 名称 的本地化字符串。
        /// </summary>
		public string Name => Lang.Name;

        /// <summary>
        ///   查找类似 下一页 的本地化字符串。
        /// </summary>
		public string NextPage => Lang.NextPage;

        /// <summary>
        ///   查找类似 否 的本地化字符串。
        /// </summary>
		public string No => Lang.No;

        /// <summary>
        ///   查找类似 暂无数据 的本地化字符串。
        /// </summary>
		public string NoData => Lang.NoData;

        /// <summary>
        ///   查找类似 任务中无数字点待更新 的本地化字符串。
        /// </summary>
		public string NoDigitalPointsUpdated => Lang.NoDigitalPointsUpdated;

        /// <summary>
        ///   查找类似 节点下存在非测量项基准，拖拽时将自动移除 的本地化字符串。
        /// </summary>
		public string NoMeasurementUnderNode => Lang.NoMeasurementUnderNode;

        /// <summary>
        ///   查找类似 循环次数 的本地化字符串。
        /// </summary>
		public string NumberOfCycles => Lang.NumberOfCycles;

        /// <summary>
        ///   查找类似 每个文件数据点个数范围是[4,1000] 的本地化字符串。
        /// </summary>
		public string NumberOfDataPoints => Lang.NumberOfDataPoints;

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public string Ok => Lang.Ok;

        /// <summary>
        ///   查找类似 透明度 的本地化字符串。
        /// </summary>
		public string Opacity => Lang.Opacity;

        /// <summary>
        ///   查找类似 打开 的本地化字符串。
        /// </summary>
		public string Open => Lang.Open;

        /// <summary>
        ///   查找类似 打开项目 的本地化字符串。
        /// </summary>
		public string OpenProject => Lang.OpenProject;

        /// <summary>
        ///   查找类似 操作 的本地化字符串。
        /// </summary>
		public string Operate => Lang.Operate;

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
        ///   查找类似 输出参数 的本地化字符串。
        /// </summary>
		public string OutputParameter => Lang.OutputParameter;

        /// <summary>
        ///   查找类似 页面模式 的本地化字符串。
        /// </summary>
		public string PageMode => Lang.PageMode;

        /// <summary>
        ///   查找类似 平行度 的本地化字符串。
        /// </summary>
		public string Parallelism => Lang.Parallelism;

        /// <summary>
        ///   查找类似 参数配置 的本地化字符串。
        /// </summary>
		public string ParameterConfig => Lang.ParameterConfig;

        /// <summary>
        ///   查找类似 粘贴 的本地化字符串。
        /// </summary>
		public string Paste => Lang.Paste;

        /// <summary>
        ///   查找类似 面片 的本地化字符串。
        /// </summary>
		public string Patch => Lang.Patch;

        /// <summary>
        ///   查找类似 暂停 的本地化字符串。
        /// </summary>
		public string Pause => Lang.Pause;

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
        ///   查找类似 面 的本地化字符串。
        /// </summary>
		public string Plane => Lang.Plane;

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
        ///   查找类似 取点方向 的本地化字符串。
        /// </summary>
		public string PointDirection => Lang.PointDirection;

        /// <summary>
        ///   查找类似 指针坐标 的本地化字符串。
        /// </summary>
		public string PointerCoord => Lang.PointerCoord;

        /// <summary>
        ///   查找类似 点尺寸 的本地化字符串。
        /// </summary>
		public string PointSize => Lang.PointSize;

        /// <summary>
        ///   查找类似 点对 的本地化字符串。
        /// </summary>
		public string PointToPoint => Lang.PointToPoint;

        /// <summary>
        ///   查找类似 预览 的本地化字符串。
        /// </summary>
		public string Preview => Lang.Preview;

        /// <summary>
        ///   查找类似 报告预览 的本地化字符串。
        /// </summary>
		public string PreviewReport => Lang.PreviewReport;

        /// <summary>
        ///   查找类似 上一页 的本地化字符串。
        /// </summary>
		public string PreviousPage => Lang.PreviousPage;

        /// <summary>
        ///   查找类似 打印 的本地化字符串。
        /// </summary>
		public string Print => Lang.Print;

        /// <summary>
        ///   查找类似 打印预览 的本地化字符串。
        /// </summary>
		public string PrintPreview => Lang.PrintPreview;

        /// <summary>
        ///   查找类似 打印设置 的本地化字符串。
        /// </summary>
		public string PrintSet => Lang.PrintSet;

        /// <summary>
        ///   查找类似 面轮廓度 的本地化字符串。
        /// </summary>
		public string Profileanysurface => Lang.Profileanysurface;

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
        ///   查找类似 项 目 名 称 的本地化字符串。
        /// </summary>
		public string ProjectName => Lang.ProjectName;

        /// <summary>
        ///   查找类似 项目属性 的本地化字符串。
        /// </summary>
		public string ProjectProperty => Lang.ProjectProperty;

        /// <summary>
        ///   查找类似 属性 的本地化字符串。
        /// </summary>
		public string Property => Lang.Property;

        /// <summary>
        ///   查找类似 退出 的本地化字符串。
        /// </summary>
		public string Quit => Lang.Quit;

        /// <summary>
        ///   查找类似 是否退出软件? 的本地化字符串。
        /// </summary>
		public string QuitSoftWare => Lang.QuitSoftWare;

        /// <summary>
        ///   查找类似 半径 的本地化字符串。
        /// </summary>
		public string Radius => Lang.Radius;

        /// <summary>
        ///   查找类似 加载CAD 的本地化字符串。
        /// </summary>
		public string ReadCAD => Lang.ReadCAD;

        /// <summary>
        ///   查找类似 加载点云 的本地化字符串。
        /// </summary>
		public string ReadCloud => Lang.ReadCloud;

        /// <summary>
        ///   查找类似 读入数据 的本地化字符串。
        /// </summary>
		public string ReadDatas => Lang.ReadDatas;

        /// <summary>
        ///   查找类似 读取方向向量 的本地化字符串。
        /// </summary>
		public string ReadDirection => Lang.ReadDirection;

        /// <summary>
        ///   查找类似 加载矩阵 的本地化字符串。
        /// </summary>
		public string ReadMatrix => Lang.ReadMatrix;

        /// <summary>
        ///   查找类似 读取数字点 的本地化字符串。
        /// </summary>
		public string ReadPoint => Lang.ReadPoint;

        /// <summary>
        ///   查找类似 加载STL 的本地化字符串。
        /// </summary>
		public string ReadSTL => Lang.ReadSTL;

        /// <summary>
        ///   查找类似 最近文件 的本地化字符串。
        /// </summary>
		public string RecentFile => Lang.RecentFile;

        /// <summary>
        ///   查找类似 最近项目 的本地化字符串。
        /// </summary>
		public string RecentProject => Lang.RecentProject;

        /// <summary>
        ///   查找类似 重做 的本地化字符串。
        /// </summary>
		public string Redo => Lang.Redo;

        /// <summary>
        ///   查找类似 参考模板 的本地化字符串。
        /// </summary>
		public string RefTemplate => Lang.RefTemplate;

        /// <summary>
        ///   查找类似 配准 的本地化字符串。
        /// </summary>
		public string Registration => Lang.Registration;

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
        ///   查找类似 报告 的本地化字符串。
        /// </summary>
		public string Report => Lang.Report;

        /// <summary>
        ///   查找类似 报告目录 的本地化字符串。
        /// </summary>
		public string ReportContents => Lang.ReportContents;

        /// <summary>
        ///   查找类似 报告名称 的本地化字符串。
        /// </summary>
		public string ReportName => Lang.ReportName;

        /// <summary>
        ///   查找类似 报告导航窗 的本地化字符串。
        /// </summary>
		public string ReportNavigation => Lang.ReportNavigation;

        /// <summary>
        ///   查找类似 报告源 的本地化字符串。
        /// </summary>
		public string ReportSource => Lang.ReportSource;

        /// <summary>
        ///   查找类似 更换模板 的本地化字符串。
        /// </summary>
		public string RepTemplate => Lang.RepTemplate;

        /// <summary>
        ///   查找类似 撤销 的本地化字符串。
        /// </summary>
		public string Revoke => Lang.Revoke;

        /// <summary>
        ///   查找类似 机器人运动 的本地化字符串。
        /// </summary>
		public string RobotMove => Lang.RobotMove;

        /// <summary>
        ///   查找类似 ROI配置 的本地化字符串。
        /// </summary>
		public string ROIConfig => Lang.ROIConfig;

        /// <summary>
        ///   查找类似 ROI点类型 的本地化字符串。
        /// </summary>
		public string ROIType => Lang.ROIType;

        /// <summary>
        ///   查找类似 绕X轴旋转 的本地化字符串。
        /// </summary>
		public string RotateAroundX => Lang.RotateAroundX;

        /// <summary>
        ///   查找类似 绕Y轴旋转 的本地化字符串。
        /// </summary>
		public string RotateAroundY => Lang.RotateAroundY;

        /// <summary>
        ///   查找类似 绕Z轴旋转 的本地化字符串。
        /// </summary>
		public string RotateAroundZ => Lang.RotateAroundZ;

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
        ///   查找类似 运行 的本地化字符串。
        /// </summary>
		public string Run => Lang.Run;

        /// <summary>
        ///   查找类似 下一步 的本地化字符串。
        /// </summary>
		public string RunNext => Lang.RunNext;

        /// <summary>
        ///   查找类似 单步运行 的本地化字符串。
        /// </summary>
		public string RunOne => Lang.RunOne;

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
        ///   查找类似 保存项目 的本地化字符串。
        /// </summary>
		public string SaveProject => Lang.SaveProject;

        /// <summary>
        ///   查找类似 是否保存工程? 的本地化字符串。
        /// </summary>
		public string SaveProjectTask => Lang.SaveProjectTask;

        /// <summary>
        ///   查找类似 报告另存完成 的本地化字符串。
        /// </summary>
		public string SaveReportAsCompleted => Lang.SaveReportAsCompleted;

        /// <summary>
        ///   查找类似 保存成功 的本地化字符串。
        /// </summary>
		public string SaveSuccess => Lang.SaveSuccess;

        /// <summary>
        ///   查找类似 屏幕快照 的本地化字符串。
        /// </summary>
		public string ScreenShot => Lang.ScreenShot;

        /// <summary>
        ///   查找类似 滚动模式 的本地化字符串。
        /// </summary>
		public string ScrollMode => Lang.ScrollMode;

        /// <summary>
        ///   查找类似 搜索文件关键字 的本地化字符串。
        /// </summary>
		public string SearchFileKeywords => Lang.SearchFileKeywords;

        /// <summary>
        ///   查找类似 分割 的本地化字符串。
        /// </summary>
		public string Segment => Lang.Segment;

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public string Select => Lang.Select;

        /// <summary>
        ///   查找类似 1.在任务树选择要标定的点云 的本地化字符串。
        /// </summary>
		public string SelectCalibratePointCloud => Lang.SelectCalibratePointCloud;

        /// <summary>
        ///   查找类似 请选择可以做测量基准的点、线、面等类型 的本地化字符串。
        /// </summary>
		public string SelectCorrectMeasurementBenchmark => Lang.SelectCorrectMeasurementBenchmark;

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
        ///   查找类似 设置模板 的本地化字符串。
        /// </summary>
		public string SetCoordTemplate => Lang.SetCoordTemplate;

        /// <summary>
        ///   查找类似 显示选项设置 的本地化字符串。
        /// </summary>
		public string SetDisPlayOption => Lang.SetDisPlayOption;

        /// <summary>
        ///   查找类似 尺寸设置 的本地化字符串。
        /// </summary>
		public string SetMeasure => Lang.SetMeasure;

        /// <summary>
        ///   查找类似 单页设置 的本地化字符串。
        /// </summary>
		public string SetSinglePage => Lang.SetSinglePage;

        /// <summary>
        ///   查找类似 显示包围盒 的本地化字符串。
        /// </summary>
		public string ShowBoundBox => Lang.ShowBoundBox;

        /// <summary>
        ///   查找类似 单页 的本地化字符串。
        /// </summary>
		public string SinglePage => Lang.SinglePage;

        /// <summary>
        ///   查找类似 尺寸 的本地化字符串。
        /// </summary>
		public string Size => Lang.Size;

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
        ///   查找类似 凌云光3D测量软件初始版本集成：几何特征新建、坐标系生成、标定、点云处理、对齐、比较、距离测量、角度测量与公差测量等 的本地化字符串。
        /// </summary>
		public string SoftwareFunctions => Lang.SoftwareFunctions;

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
        ///   查找类似 空格 的本地化字符串。
        /// </summary>
		public string Space => Lang.Space;

        /// <summary>
        ///   查找类似 球体 的本地化字符串。
        /// </summary>
		public string Sphere => Lang.Sphere;

        /// <summary>
        ///   查找类似 按位读取 的本地化字符串。
        /// </summary>
		public string SplitIntToBit => Lang.SplitIntToBit;

        /// <summary>
        ///   查找类似 标准值 的本地化字符串。
        /// </summary>
		public string StandardValue => Lang.StandardValue;

        /// <summary>
        ///   查找类似 起始列 的本地化字符串。
        /// </summary>
		public string StartColumn => Lang.StartColumn;

        /// <summary>
        ///   查找类似 起始行 的本地化字符串。
        /// </summary>
		public string StartRow => Lang.StartRow;

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
        ///   查找类似 存储地址 的本地化字符串。
        /// </summary>
		public string StorageAddress => Lang.StorageAddress;

        /// <summary>
        ///   查找类似 直线度 的本地化字符串。
        /// </summary>
		public string Straightness => Lang.Straightness;

        /// <summary>
        ///   查找类似 字符串拼接窗口 的本地化字符串。
        /// </summary>
		public string StringExDialog => Lang.StringExDialog;

        /// <summary>
        ///   查找类似 字符匹配对话框 的本地化字符串。
        /// </summary>
		public string StringMatchDialog => Lang.StringMatchDialog;

        /// <summary>
        ///   查找类似 字符拼接 的本地化字符串。
        /// </summary>
		public string StringMerge => Lang.StringMerge;

        /// <summary>
        ///   查找类似 字符解析 的本地化字符串。
        /// </summary>
		public string StringParse => Lang.StringParse;

        /// <summary>
        ///   查找类似 表面距离 的本地化字符串。
        /// </summary>
		public string SurfaceDistance => Lang.SurfaceDistance;

        /// <summary>
        ///   查找类似 面轮廓度 的本地化字符串。
        /// </summary>
		public string SurfaceProfile => Lang.SurfaceProfile;

        /// <summary>
        ///   查找类似 多分支 的本地化字符串。
        /// </summary>
		public string Switch => Lang.Switch;

        /// <summary>
        ///   查找类似 条件弹窗 的本地化字符串。
        /// </summary>
		public string SwitchDialog => Lang.SwitchDialog;

        /// <summary>
        ///   查找类似 条件任务 的本地化字符串。
        /// </summary>
		public string SwitchGroup => Lang.SwitchGroup;

        /// <summary>
        ///   查找类似 任务流 的本地化字符串。
        /// </summary>
		public string TaskFlow => Lang.TaskFlow;

        /// <summary>
        ///   查找类似 任务模拟器 的本地化字符串。
        /// </summary>
		public string TaskSimulator => Lang.TaskSimulator;

        /// <summary>
        ///   查找类似 最少三组 的本地化字符串。
        /// </summary>
		public string ThreeGroups => Lang.ThreeGroups;

        /// <summary>
        ///   查找类似 耗时 的本地化字符串。
        /// </summary>
		public string Time => Lang.Time;

        /// <summary>
        ///   查找类似 提示 的本地化字符串。
        /// </summary>
		public string Tip => Lang.Tip;

        /// <summary>
        ///   查找类似 标题 的本地化字符串。
        /// </summary>
		public string Title => Lang.Title;

        /// <summary>
        ///   查找类似 公差 的本地化字符串。
        /// </summary>
		public string Tolerance => Lang.Tolerance;

        /// <summary>
        ///   查找类似 公差测量主要包含：平行度、直线度、面轮廓度、线轮廓度、平面度、圆度等 的本地化字符串。
        /// </summary>
		public string ToleranceMeasurementsInclude => Lang.ToleranceMeasurementsInclude;

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
        ///   查找类似 教程文档不存在 的本地化字符串。
        /// </summary>
		public string TutorialDontExist => Lang.TutorialDontExist;

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
        ///   查找类似 类别 的本地化字符串。
        /// </summary>
		public string Type => Lang.Type;

        /// <summary>
        ///   查找类似 未知 的本地化字符串。
        /// </summary>
		public string Unknown => Lang.Unknown;

        /// <summary>
        ///   查找类似 未知大小 的本地化字符串。
        /// </summary>
		public string UnknownSize => Lang.UnknownSize;

        /// <summary>
        ///   查找类似 更新 的本地化字符串。
        /// </summary>
		public string Update => Lang.Update;

        /// <summary>
        ///   查找类似 公差上限 的本地化字符串。
        /// </summary>
		public string UpperLimit => Lang.UpperLimit;

        /// <summary>
        ///   查找类似 使用教程 的本地化字符串。
        /// </summary>
		public string UsingTutorials => Lang.UsingTutorials;

        /// <summary>
        ///   查找类似 值 的本地化字符串。
        /// </summary>
		public string Value => Lang.Value;

        /// <summary>
        ///   查找类似 版本信息窗 的本地化字符串。
        /// </summary>
		public string VersionInfoDialog => Lang.VersionInfoDialog;

        /// <summary>
        ///   查找类似 顶点 的本地化字符串。
        /// </summary>
		public string Vertex => Lang.Vertex;

        /// <summary>
        ///   查找类似 垂直度 的本地化字符串。
        /// </summary>
		public string Verticality => Lang.Verticality;

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
        ///   查找类似 显示标签 的本地化字符串。
        /// </summary>
		public string VisibleLabel => Lang.VisibleLabel;

        /// <summary>
        ///   查找类似 警告 的本地化字符串。
        /// </summary>
		public string Warning => Lang.Warning;

        /// <summary>
        ///   查找类似 宽方向 的本地化字符串。
        /// </summary>
		public string WDirection => Lang.WDirection;

        /// <summary>
        ///   查找类似 线框 的本地化字符串。
        /// </summary>
		public string Wireframe => Lang.Wireframe;

        /// <summary>
        ///   查找类似 是 的本地化字符串。
        /// </summary>
		public string Yes => Lang.Yes;

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
        ///   查找类似 激活 的本地化字符串。
        /// </summary>
		public static string Active = nameof(Active);

        /// <summary>
        ///   查找类似 新增 的本地化字符串。
        /// </summary>
		public static string Add = nameof(Add);

        /// <summary>
        ///   查找类似 新增参数 的本地化字符串。
        /// </summary>
		public static string AddInParam = nameof(AddInParam);

        /// <summary>
        ///   查找类似 2.将模型和点云调整到相同视角 的本地化字符串。
        /// </summary>
		public static string AdjustModelAndPointCloud = nameof(AdjustModelAndPointCloud);

        /// <summary>
        ///   查找类似 别名不可重复 的本地化字符串。
        /// </summary>
		public static string AliasCannotDuplicate = nameof(AliasCannotDuplicate);

        /// <summary>
        ///   查找类似 别名不可为空 的本地化字符串。
        /// </summary>
		public static string AliasCannotEmpty = nameof(AliasCannotEmpty);

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
        ///   查找类似 上午 的本地化字符串。
        /// </summary>
		public static string Am = nameof(Am);

        /// <summary>
        ///   查找类似 角度测量 的本地化字符串。
        /// </summary>
		public static string AngleMeasure = nameof(AngleMeasure);

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
        ///   查找类似 文件整理 的本地化字符串。
        /// </summary>
		public static string ArrangeDocument = nameof(ArrangeDocument);

        /// <summary>
        ///   查找类似 并行任务 的本地化字符串。
        /// </summary>
		public static string AsyncGroup = nameof(AsyncGroup);

        /// <summary>
        ///   查找类似 平均耗时 的本地化字符串。
        /// </summary>
		public static string AverageTime = nameof(AverageTime);

        /// <summary>
        ///   查找类似 轴线方向 的本地化字符串。
        /// </summary>
		public static string AxisDirection = nameof(AxisDirection);

        /// <summary>
        ///   查找类似 批量修改几何体属性 的本地化字符串。
        /// </summary>
		public static string BatchChangeGeometry = nameof(BatchChangeGeometry);

        /// <summary>
        ///   查找类似 批量创建测量项 的本地化字符串。
        /// </summary>
		public static string BatchCreateMeasurements = nameof(BatchCreateMeasurements);

        /// <summary>
        ///   查找类似 批量生成测量点 的本地化字符串。
        /// </summary>
		public static string BatchGenMeaPoints = nameof(BatchGenMeaPoints);

        /// <summary>
        ///   查找类似 批量生成点 的本地化字符串。
        /// </summary>
		public static string BatchGenPoints = nameof(BatchGenPoints);

        /// <summary>
        ///   查找类似 批量生成几何体 的本地化字符串。
        /// </summary>
		public static string BatchGeometry = nameof(BatchGeometry);

        /// <summary>
        ///   查找类似 批量导入点 的本地化字符串。
        /// </summary>
		public static string BatchImportPoints = nameof(BatchImportPoints);

        /// <summary>
        ///   查找类似 批量加载点云，在设置输入参数时有效！ 的本地化字符串。
        /// </summary>
		public static string BatchLoadingPointClouds = nameof(BatchLoadingPointClouds);

        /// <summary>
        ///   查找类似 最佳拟合 的本地化字符串。
        /// </summary>
		public static string BestFit = nameof(BestFit);

        /// <summary>
        ///   查找类似 包围盒 的本地化字符串。
        /// </summary>
		public static string BoundBox = nameof(BoundBox);

        /// <summary>
        ///   查找类似 分支 的本地化字符串。
        /// </summary>
		public static string Branch = nameof(Branch);

        /// <summary>
        ///   查找类似 判断分支 的本地化字符串。
        /// </summary>
		public static string BranchGroup = nameof(BranchGroup);

        /// <summary>
        ///   查找类似 CAD模型 的本地化字符串。
        /// </summary>
		public static string CADModel = nameof(CADModel);

        /// <summary>
        ///   查找类似 计算器 的本地化字符串。
        /// </summary>
		public static string Calculator = nameof(Calculator);

        /// <summary>
        ///   查找类似 标定 的本地化字符串。
        /// </summary>
		public static string Calib = nameof(Calib);

        /// <summary>
        ///   查找类似 标定操作说明 的本地化字符串。
        /// </summary>
		public static string CalibrationOperationInstructions = nameof(CalibrationOperationInstructions);

        /// <summary>
        ///   查找类似 取消 的本地化字符串。
        /// </summary>
		public static string Cancel = nameof(Cancel);

        /// <summary>
        ///   查找类似 5.“取消”按钮，取消标定 的本地化字符串。
        /// </summary>
		public static string CancelCalibration = nameof(CancelCalibration);

        /// <summary>
        ///   查找类似 取消模板 的本地化字符串。
        /// </summary>
		public static string CancelCoordTemplate = nameof(CancelCoordTemplate);

        /// <summary>
        ///   查找类似 取消忽略 的本地化字符串。
        /// </summary>
		public static string CancelSkip = nameof(CancelSkip);

        /// <summary>
        ///   查找类似 中心点 的本地化字符串。
        /// </summary>
		public static string CenterPoint = nameof(CenterPoint);

        /// <summary>
        ///   查找类似 存在基准集合为空，请检查 的本地化字符串。
        /// </summary>
		public static string CheckBenchmarkEmpty = nameof(CheckBenchmarkEmpty);

        /// <summary>
        ///   查找类似 检查更新 的本地化字符串。
        /// </summary>
		public static string CheckForUpdates = nameof(CheckForUpdates);

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public static string Choose = nameof(Choose);

        /// <summary>
        ///   查找类似 圆 的本地化字符串。
        /// </summary>
		public static string Circle = nameof(Circle);

        /// <summary>
        ///   查找类似 清空 的本地化字符串。
        /// </summary>
		public static string Clear = nameof(Clear);

        /// <summary>
        ///   查找类似 是否确定清空基准参数 的本地化字符串。
        /// </summary>
		public static string ClearBenchmarkParameters = nameof(ClearBenchmarkParameters);

        /// <summary>
        ///   查找类似 确定清空输入输出参数 的本地化字符串。
        /// </summary>
		public static string ClearInOutParameters = nameof(ClearInOutParameters);

        /// <summary>
        ///   查找类似 3.“点选”按钮，依次在模型和点云上点击 的本地化字符串。
        /// </summary>
		public static string ClickModelAndPointCloud = nameof(ClickModelAndPointCloud);

        /// <summary>
        ///   查找类似 关闭 的本地化字符串。
        /// </summary>
		public static string Close = nameof(Close);

        /// <summary>
        ///   查找类似 关闭所有 的本地化字符串。
        /// </summary>
		public static string CloseAll = nameof(CloseAll);

        /// <summary>
        ///   查找类似 7.“关闭”按钮，退出当前页面 的本地化字符串。
        /// </summary>
		public static string CloseCalibration = nameof(CloseCalibration);

        /// <summary>
        ///   查找类似 关闭其他 的本地化字符串。
        /// </summary>
		public static string CloseOther = nameof(CloseOther);

        /// <summary>
        ///   查找类似 点云对其匹配 的本地化字符串。
        /// </summary>
		public static string CloudAlignMatch = nameof(CloudAlignMatch);

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
        ///   查找类似 点云边缘提取 的本地化字符串。
        /// </summary>
		public static string CloudExtractEdge = nameof(CloudExtractEdge);

        /// <summary>
        ///   查找类似 网格比较 的本地化字符串。
        /// </summary>
		public static string CloudMesh = nameof(CloudMesh);

        /// <summary>
        ///   查找类似 点云法向信息 的本地化字符串。
        /// </summary>
		public static string CloudNormal = nameof(CloudNormal);

        /// <summary>
        ///   查找类似 点云处理 的本地化字符串。
        /// </summary>
		public static string CloudProcess = nameof(CloudProcess);

        /// <summary>
        ///   查找类似 点云投影面 的本地化字符串。
        /// </summary>
		public static string CloudProjPlane = nameof(CloudProjPlane);

        /// <summary>
        ///   查找类似 点云配准 的本地化字符串。
        /// </summary>
		public static string CloudRegistration = nameof(CloudRegistration);

        /// <summary>
        ///   查找类似 点云去重 的本地化字符串。
        /// </summary>
		public static string CloudReRepeat = nameof(CloudReRepeat);

        /// <summary>
        ///   查找类似 点云多截面分割 的本地化字符串。
        /// </summary>
		public static string CloudSectionSegment = nameof(CloudSectionSegment);

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
        ///   查找类似 碰撞方向 的本地化字符串。
        /// </summary>
		public static string CollisionDirection = nameof(CollisionDirection);

        /// <summary>
        ///   查找类似 颜色 的本地化字符串。
        /// </summary>
		public static string Color = nameof(Color);

        /// <summary>
        ///   查找类似 逗号 的本地化字符串。
        /// </summary>
		public static string Comma = nameof(Comma);

        /// <summary>
        ///   查找类似 比较 的本地化字符串。
        /// </summary>
		public static string Compare = nameof(Compare);

        /// <summary>
        ///   查找类似 排版 的本地化字符串。
        /// </summary>
		public static string Composing = nameof(Composing);

        /// <summary>
        ///   查找类似 圆锥 的本地化字符串。
        /// </summary>
		public static string Cone = nameof(Cone);

        /// <summary>
        ///   查找类似 请先配置输入参数(点云) 的本地化字符串。
        /// </summary>
		public static string ConfigureInputParametersFirst = nameof(ConfigureInputParametersFirst);

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public static string Confirm = nameof(Confirm);

        /// <summary>
        ///   查找类似 确认任务中存在从文件加载的激活点云 的本地化字符串。
        /// </summary>
		public static string ConfirmActivePointCloud = nameof(ConfirmActivePointCloud);

        /// <summary>
        ///   查找类似 6.“确认”按钮，完成当前点云标定 的本地化字符串。
        /// </summary>
		public static string ConfirmCalibration = nameof(ConfirmCalibration);

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
        ///   查找类似 新建 的本地化字符串。
        /// </summary>
		public static string Create = nameof(Create);

        /// <summary>
        ///   查找类似 创建项目 的本地化字符串。
        /// </summary>
		public static string CreateProject = nameof(CreateProject);

        /// <summary>
        ///   查找类似 当前项目源文件已删除，请先新建项目代替当前项目 的本地化字符串。
        /// </summary>
		public static string CreateProjectReplaceCurrent = nameof(CreateProjectReplaceCurrent);

        /// <summary>
        ///   查找类似 长方体 的本地化字符串。
        /// </summary>
		public static string Cuboid = nameof(Cuboid);

        /// <summary>
        ///   查找类似 长方体变换 的本地化字符串。
        /// </summary>
		public static string CuboidTransform = nameof(CuboidTransform);

        /// <summary>
        ///   查找类似 最 近 项 目 的本地化字符串。
        /// </summary>
		public static string CurrentProject = nameof(CurrentProject);

        /// <summary>
        ///   查找类似 自定义 的本地化字符串。
        /// </summary>
		public static string Custom = nameof(Custom);

        /// <summary>
        ///   查找类似 圆柱 的本地化字符串。
        /// </summary>
		public static string Cylinder = nameof(Cylinder);

        /// <summary>
        ///   查找类似 圆柱度 的本地化字符串。
        /// </summary>
		public static string Cylindricity = nameof(Cylindricity);

        /// <summary>
        ///   查找类似 数据目录 的本地化字符串。
        /// </summary>
		public static string DataDirectory = nameof(DataDirectory);

        /// <summary>
        ///   查找类似 数据处理 的本地化字符串。
        /// </summary>
		public static string DataProcess = nameof(DataProcess);

        /// <summary>
        ///   查找类似 删除 的本地化字符串。
        /// </summary>
		public static string Delete = nameof(Delete);

        /// <summary>
        ///   查找类似 去噪 的本地化字符串。
        /// </summary>
		public static string Denoising = nameof(Denoising);

        /// <summary>
        ///   查找类似 探测深度 的本地化字符串。
        /// </summary>
		public static string DetectionDepth = nameof(DetectionDepth);

        /// <summary>
        ///   查找类似 数字点 的本地化字符串。
        /// </summary>
		public static string DigitalPoint = nameof(DigitalPoint);

        /// <summary>
        ///   查找类似 圆尺寸测量 的本地化字符串。
        /// </summary>
		public static string DisCircleAttribute = nameof(DisCircleAttribute);

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
        ///   查找类似 双页 的本地化字符串。
        /// </summary>
		public static string DoublePage = nameof(DoublePage);

        /// <summary>
        ///   查找类似 下采样 的本地化字符串。
        /// </summary>
		public static string DownSampling = nameof(DownSampling);

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
        ///   查找类似 编辑 的本地化字符串。
        /// </summary>
		public static string Edit = nameof(Edit);

        /// <summary>
        ///   查找类似 编辑特征 的本地化字符串。
        /// </summary>
		public static string EditFeature = nameof(EditFeature);

        /// <summary>
        ///   查找类似 错误 的本地化字符串。
        /// </summary>
		public static string Error = nameof(Error);

        /// <summary>
        ///   查找类似 错误的图片路径 的本地化字符串。
        /// </summary>
		public static string ErrorImgPath = nameof(ErrorImgPath);

        /// <summary>
        ///   查找类似 非法的图片尺寸 的本地化字符串。
        /// </summary>
		public static string ErrorImgSize = nameof(ErrorImgSize);

        /// <summary>
        ///   查找类似 导出 的本地化字符串。
        /// </summary>
		public static string Export = nameof(Export);

        /// <summary>
        ///   查找类似 表达式对话框 的本地化字符串。
        /// </summary>
		public static string ExpressDialog = nameof(ExpressDialog);

        /// <summary>
        ///   查找类似 提取异步组 的本地化字符串。
        /// </summary>
		public static string ExtractAsyncGroup = nameof(ExtractAsyncGroup);

        /// <summary>
        ///   查找类似 提取分支组 的本地化字符串。
        /// </summary>
		public static string ExtractBranchGroup = nameof(ExtractBranchGroup);

        /// <summary>
        ///   查找类似 提取串行组 的本地化字符串。
        /// </summary>
		public static string ExtractStepGroup = nameof(ExtractStepGroup);

        /// <summary>
        ///   查找类似 提取条件组 的本地化字符串。
        /// </summary>
		public static string ExtractSwitchGroup = nameof(ExtractSwitchGroup);

        /// <summary>
        ///   查找类似 回退距离 的本地化字符串。
        /// </summary>
		public static string FallbackDistance = nameof(FallbackDistance);

        /// <summary>
        ///   查找类似 文件 的本地化字符串。
        /// </summary>
		public static string File = nameof(File);

        /// <summary>
        ///   查找类似 文件地址 的本地化字符串。
        /// </summary>
		public static string FileAddress = nameof(FileAddress);

        /// <summary>
        ///   查找类似 文件输入 的本地化字符串。
        /// </summary>
		public static string FileIO = nameof(FileIO);

        /// <summary>
        ///   查找类似 滤波 的本地化字符串。
        /// </summary>
		public static string Filtering = nameof(Filtering);

        /// <summary>
        ///   查找类似 查找 的本地化字符串。
        /// </summary>
		public static string Find = nameof(Find);

        /// <summary>
        ///   查找类似 完 成 的本地化字符串。
        /// </summary>
		public static string Finish = nameof(Finish);

        /// <summary>
        ///   查找类似 平面度 的本地化字符串。
        /// </summary>
		public static string Flatness = nameof(Flatness);

        /// <summary>
        ///   查找类似 4.亮色为前景色，黑色为背景色 的本地化字符串。
        /// </summary>
		public static string ForegroundAndBackground = nameof(ForegroundAndBackground);

        /// <summary>
        ///   查找类似 格式错误 的本地化字符串。
        /// </summary>
		public static string FormatError = nameof(FormatError);

        /// <summary>
        ///   查找类似 用于内存泄露检测 的本地化字符串。
        /// </summary>
		public static string ForMemoryLeakDetection = nameof(ForMemoryLeakDetection);

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
        ///   查找类似 生成方式需设置为：点 的本地化字符串。
        /// </summary>
		public static string GenerationMethodPoint = nameof(GenerationMethodPoint);

        /// <summary>
        ///   查找类似 生成方式 的本地化字符串。
        /// </summary>
		public static string GenerationMode = nameof(GenerationMode);

        /// <summary>
        ///   查找类似 由点生成几何体 的本地化字符串。
        /// </summary>
		public static string GenGeomFromPoint = nameof(GenGeomFromPoint);

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
        ///   查找类似 面Z获取 的本地化字符串。
        /// </summary>
		public static string GenPointByPlaneZ = nameof(GenPointByPlaneZ);

        /// <summary>
        ///   查找类似 求投影点 的本地化字符串。
        /// </summary>
		public static string GenPointByProj = nameof(GenPointByProj);

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
        ///   查找类似 几何特征 的本地化字符串。
        /// </summary>
		public static string GeometricFeatures = nameof(GeometricFeatures);

        /// <summary>
        ///   查找类似 几何特征新建包含：点、线、面、长方体、圆柱、圆锥、球等 的本地化字符串。
        /// </summary>
		public static string GeometricFeaturesInclude = nameof(GeometricFeaturesInclude);

        /// <summary>
        ///   查找类似 几何 的本地化字符串。
        /// </summary>
		public static string Geometry = nameof(Geometry);

        /// <summary>
        ///   查找类似 获取方向 的本地化字符串。
        /// </summary>
		public static string GetDirectionByObj = nameof(GetDirectionByObj);

        /// <summary>
        ///   查找类似 获取线 的本地化字符串。
        /// </summary>
		public static string GetLineByObj = nameof(GetLineByObj);

        /// <summary>
        ///   查找类似 获取面 的本地化字符串。
        /// </summary>
		public static string GetPlaneByObj = nameof(GetPlaneByObj);

        /// <summary>
        ///   查找类似 获取点 的本地化字符串。
        /// </summary>
		public static string GetPointByObj = nameof(GetPointByObj);

        /// <summary>
        ///   查找类似 高方向 的本地化字符串。
        /// </summary>
		public static string HDirection = nameof(HDirection);

        /// <summary>
        ///   查找类似 高 的本地化字符串。
        /// </summary>
		public static string Height = nameof(Height);

        /// <summary>
        ///   查找类似 高度色谱 的本地化字符串。
        /// </summary>
		public static string HeightCorSpect = nameof(HeightCorSpect);

        /// <summary>
        ///   查找类似 帮助 的本地化字符串。
        /// </summary>
		public static string Help = nameof(Help);

        /// <summary>
        ///   查找类似 隐藏包围盒 的本地化字符串。
        /// </summary>
		public static string HideBoundBox = nameof(HideBoundBox);

        /// <summary>
        ///   查找类似 隐藏标签 的本地化字符串。
        /// </summary>
		public static string HideLabel = nameof(HideLabel);

        /// <summary>
        ///   查找类似 起始页 的本地化字符串。
        /// </summary>
		public static string HomePage = nameof(HomePage);

        /// <summary>
        ///   查找类似 输入参数别名 的本地化字符串。
        /// </summary>
		public static string ImportParameterName = nameof(ImportParameterName);

        /// <summary>
        ///   查找类似 倾斜度 的本地化字符串。
        /// </summary>
		public static string Inclination = nameof(Inclination);

        /// <summary>
        ///   查找类似 内部点云 的本地化字符串。
        /// </summary>
		public static string InCloud = nameof(InCloud);

        /// <summary>
        ///   查找类似 信息 的本地化字符串。
        /// </summary>
		public static string Info = nameof(Info);

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
        ///   查找类似 插入 的本地化字符串。
        /// </summary>
		public static string Insert = nameof(Insert);

        /// <summary>
        ///   查找类似 插入点 的本地化字符串。
        /// </summary>
		public static string InsertPoint = nameof(InsertPoint);

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
        ///   查找类似 基准Ⅰ集合与基准Ⅱ集合个数不同，确认生成测量项 的本地化字符串。
        /// </summary>
		public static string IsGenerateMeasurement = nameof(IsGenerateMeasurement);

        /// <summary>
        ///   查找类似 回零恢复默认值 的本地化字符串。
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
        ///   查找类似 跳转 的本地化字符串。
        /// </summary>
		public static string Jump = nameof(Jump);

        /// <summary>
        ///   查找类似 关键字匹配 的本地化字符串。
        /// </summary>
		public static string KeywordMatching = nameof(KeywordMatching);

        /// <summary>
        ///   查找类似 查找类似 {0} 的本地化字符串。 的本地化字符串。
        /// </summary>
		public static string LangComment = nameof(LangComment);

        /// <summary>
        ///   查找类似 长方向 的本地化字符串。
        /// </summary>
		public static string LDirection = nameof(LDirection);

        /// <summary>
        ///   查找类似 导入 的本地化字符串。
        /// </summary>
		public static string Lead = nameof(Lead);

        /// <summary>
        ///   查找类似 光源设置 的本地化字符串。
        /// </summary>
		public static string LightingSettings = nameof(LightingSettings);

        /// <summary>
        ///   查找类似 线 的本地化字符串。
        /// </summary>
		public static string Line = nameof(Line);

        /// <summary>
        ///   查找类似 线轮廓度 的本地化字符串。
        /// </summary>
		public static string LineProfile = nameof(LineProfile);

        /// <summary>
        ///   查找类似 线延长比 的本地化字符串。
        /// </summary>
		public static string LineScale = nameof(LineScale);

        /// <summary>
        ///   查找类似 线宽 的本地化字符串。
        /// </summary>
		public static string LineWidth = nameof(LineWidth);

        /// <summary>
        ///   查找类似 加载 的本地化字符串。
        /// </summary>
		public static string Load = nameof(Load);

        /// <summary>
        ///   查找类似 运行中 的本地化字符串。
        /// </summary>
		public static string Loading = nameof(Loading);

        /// <summary>
        ///   查找类似 逻辑 的本地化字符串。
        /// </summary>
		public static string Logic = nameof(Logic);

        /// <summary>
        ///   查找类似 循环 的本地化字符串。
        /// </summary>
		public static string Loop = nameof(Loop);

        /// <summary>
        ///   查找类似 公差下限 的本地化字符串。
        /// </summary>
		public static string LowerLimit = nameof(LowerLimit);

        /// <summary>
        ///   查找类似 测量基准Ⅰ 的本地化字符串。
        /// </summary>
		public static string MeasureBenchmarkⅠ = nameof(MeasureBenchmarkⅠ);

        /// <summary>
        ///   查找类似 测量基准Ⅱ 的本地化字符串。
        /// </summary>
		public static string MeasureBenchmarkⅡ = nameof(MeasureBenchmarkⅡ);

        /// <summary>
        ///   查找类似 测量类型 的本地化字符串。
        /// </summary>
		public static string MeasureType = nameof(MeasureType);

        /// <summary>
        ///   查找类似 点合并 的本地化字符串。
        /// </summary>
		public static string MergePoints = nameof(MergePoints);

        /// <summary>
        ///   查找类似 网格数据 的本地化字符串。
        /// </summary>
		public static string Mesh = nameof(Mesh);

        /// <summary>
        ///   查找类似 杂项 的本地化字符串。
        /// </summary>
		public static string Miscellaneous = nameof(Miscellaneous);

        /// <summary>
        ///   查找类似 ModbusRTU 的本地化字符串。
        /// </summary>
		public static string ModbusRTU = nameof(ModbusRTU);

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
        ///   查找类似 模块名称 的本地化字符串。
        /// </summary>
		public static string ModuleName = nameof(ModuleName);

        /// <summary>
        ///   查找类似 名称 的本地化字符串。
        /// </summary>
		public static string Name = nameof(Name);

        /// <summary>
        ///   查找类似 下一页 的本地化字符串。
        /// </summary>
		public static string NextPage = nameof(NextPage);

        /// <summary>
        ///   查找类似 否 的本地化字符串。
        /// </summary>
		public static string No = nameof(No);

        /// <summary>
        ///   查找类似 暂无数据 的本地化字符串。
        /// </summary>
		public static string NoData = nameof(NoData);

        /// <summary>
        ///   查找类似 任务中无数字点待更新 的本地化字符串。
        /// </summary>
		public static string NoDigitalPointsUpdated = nameof(NoDigitalPointsUpdated);

        /// <summary>
        ///   查找类似 节点下存在非测量项基准，拖拽时将自动移除 的本地化字符串。
        /// </summary>
		public static string NoMeasurementUnderNode = nameof(NoMeasurementUnderNode);

        /// <summary>
        ///   查找类似 循环次数 的本地化字符串。
        /// </summary>
		public static string NumberOfCycles = nameof(NumberOfCycles);

        /// <summary>
        ///   查找类似 每个文件数据点个数范围是[4,1000] 的本地化字符串。
        /// </summary>
		public static string NumberOfDataPoints = nameof(NumberOfDataPoints);

        /// <summary>
        ///   查找类似 确定 的本地化字符串。
        /// </summary>
		public static string Ok = nameof(Ok);

        /// <summary>
        ///   查找类似 透明度 的本地化字符串。
        /// </summary>
		public static string Opacity = nameof(Opacity);

        /// <summary>
        ///   查找类似 打开 的本地化字符串。
        /// </summary>
		public static string Open = nameof(Open);

        /// <summary>
        ///   查找类似 打开项目 的本地化字符串。
        /// </summary>
		public static string OpenProject = nameof(OpenProject);

        /// <summary>
        ///   查找类似 操作 的本地化字符串。
        /// </summary>
		public static string Operate = nameof(Operate);

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
        ///   查找类似 输出参数 的本地化字符串。
        /// </summary>
		public static string OutputParameter = nameof(OutputParameter);

        /// <summary>
        ///   查找类似 页面模式 的本地化字符串。
        /// </summary>
		public static string PageMode = nameof(PageMode);

        /// <summary>
        ///   查找类似 平行度 的本地化字符串。
        /// </summary>
		public static string Parallelism = nameof(Parallelism);

        /// <summary>
        ///   查找类似 参数配置 的本地化字符串。
        /// </summary>
		public static string ParameterConfig = nameof(ParameterConfig);

        /// <summary>
        ///   查找类似 粘贴 的本地化字符串。
        /// </summary>
		public static string Paste = nameof(Paste);

        /// <summary>
        ///   查找类似 面片 的本地化字符串。
        /// </summary>
		public static string Patch = nameof(Patch);

        /// <summary>
        ///   查找类似 暂停 的本地化字符串。
        /// </summary>
		public static string Pause = nameof(Pause);

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
        ///   查找类似 面 的本地化字符串。
        /// </summary>
		public static string Plane = nameof(Plane);

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
        ///   查找类似 取点方向 的本地化字符串。
        /// </summary>
		public static string PointDirection = nameof(PointDirection);

        /// <summary>
        ///   查找类似 指针坐标 的本地化字符串。
        /// </summary>
		public static string PointerCoord = nameof(PointerCoord);

        /// <summary>
        ///   查找类似 点尺寸 的本地化字符串。
        /// </summary>
		public static string PointSize = nameof(PointSize);

        /// <summary>
        ///   查找类似 点对 的本地化字符串。
        /// </summary>
		public static string PointToPoint = nameof(PointToPoint);

        /// <summary>
        ///   查找类似 预览 的本地化字符串。
        /// </summary>
		public static string Preview = nameof(Preview);

        /// <summary>
        ///   查找类似 报告预览 的本地化字符串。
        /// </summary>
		public static string PreviewReport = nameof(PreviewReport);

        /// <summary>
        ///   查找类似 上一页 的本地化字符串。
        /// </summary>
		public static string PreviousPage = nameof(PreviousPage);

        /// <summary>
        ///   查找类似 打印 的本地化字符串。
        /// </summary>
		public static string Print = nameof(Print);

        /// <summary>
        ///   查找类似 打印预览 的本地化字符串。
        /// </summary>
		public static string PrintPreview = nameof(PrintPreview);

        /// <summary>
        ///   查找类似 打印设置 的本地化字符串。
        /// </summary>
		public static string PrintSet = nameof(PrintSet);

        /// <summary>
        ///   查找类似 面轮廓度 的本地化字符串。
        /// </summary>
		public static string Profileanysurface = nameof(Profileanysurface);

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
        ///   查找类似 项 目 名 称 的本地化字符串。
        /// </summary>
		public static string ProjectName = nameof(ProjectName);

        /// <summary>
        ///   查找类似 项目属性 的本地化字符串。
        /// </summary>
		public static string ProjectProperty = nameof(ProjectProperty);

        /// <summary>
        ///   查找类似 属性 的本地化字符串。
        /// </summary>
		public static string Property = nameof(Property);

        /// <summary>
        ///   查找类似 退出 的本地化字符串。
        /// </summary>
		public static string Quit = nameof(Quit);

        /// <summary>
        ///   查找类似 是否退出软件? 的本地化字符串。
        /// </summary>
		public static string QuitSoftWare = nameof(QuitSoftWare);

        /// <summary>
        ///   查找类似 半径 的本地化字符串。
        /// </summary>
		public static string Radius = nameof(Radius);

        /// <summary>
        ///   查找类似 加载CAD 的本地化字符串。
        /// </summary>
		public static string ReadCAD = nameof(ReadCAD);

        /// <summary>
        ///   查找类似 加载点云 的本地化字符串。
        /// </summary>
		public static string ReadCloud = nameof(ReadCloud);

        /// <summary>
        ///   查找类似 读入数据 的本地化字符串。
        /// </summary>
		public static string ReadDatas = nameof(ReadDatas);

        /// <summary>
        ///   查找类似 读取方向向量 的本地化字符串。
        /// </summary>
		public static string ReadDirection = nameof(ReadDirection);

        /// <summary>
        ///   查找类似 加载矩阵 的本地化字符串。
        /// </summary>
		public static string ReadMatrix = nameof(ReadMatrix);

        /// <summary>
        ///   查找类似 读取数字点 的本地化字符串。
        /// </summary>
		public static string ReadPoint = nameof(ReadPoint);

        /// <summary>
        ///   查找类似 加载STL 的本地化字符串。
        /// </summary>
		public static string ReadSTL = nameof(ReadSTL);

        /// <summary>
        ///   查找类似 最近文件 的本地化字符串。
        /// </summary>
		public static string RecentFile = nameof(RecentFile);

        /// <summary>
        ///   查找类似 最近项目 的本地化字符串。
        /// </summary>
		public static string RecentProject = nameof(RecentProject);

        /// <summary>
        ///   查找类似 重做 的本地化字符串。
        /// </summary>
		public static string Redo = nameof(Redo);

        /// <summary>
        ///   查找类似 参考模板 的本地化字符串。
        /// </summary>
		public static string RefTemplate = nameof(RefTemplate);

        /// <summary>
        ///   查找类似 配准 的本地化字符串。
        /// </summary>
		public static string Registration = nameof(Registration);

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
        ///   查找类似 报告 的本地化字符串。
        /// </summary>
		public static string Report = nameof(Report);

        /// <summary>
        ///   查找类似 报告目录 的本地化字符串。
        /// </summary>
		public static string ReportContents = nameof(ReportContents);

        /// <summary>
        ///   查找类似 报告名称 的本地化字符串。
        /// </summary>
		public static string ReportName = nameof(ReportName);

        /// <summary>
        ///   查找类似 报告导航窗 的本地化字符串。
        /// </summary>
		public static string ReportNavigation = nameof(ReportNavigation);

        /// <summary>
        ///   查找类似 报告源 的本地化字符串。
        /// </summary>
		public static string ReportSource = nameof(ReportSource);

        /// <summary>
        ///   查找类似 更换模板 的本地化字符串。
        /// </summary>
		public static string RepTemplate = nameof(RepTemplate);

        /// <summary>
        ///   查找类似 撤销 的本地化字符串。
        /// </summary>
		public static string Revoke = nameof(Revoke);

        /// <summary>
        ///   查找类似 机器人运动 的本地化字符串。
        /// </summary>
		public static string RobotMove = nameof(RobotMove);

        /// <summary>
        ///   查找类似 ROI配置 的本地化字符串。
        /// </summary>
		public static string ROIConfig = nameof(ROIConfig);

        /// <summary>
        ///   查找类似 ROI点类型 的本地化字符串。
        /// </summary>
		public static string ROIType = nameof(ROIType);

        /// <summary>
        ///   查找类似 绕X轴旋转 的本地化字符串。
        /// </summary>
		public static string RotateAroundX = nameof(RotateAroundX);

        /// <summary>
        ///   查找类似 绕Y轴旋转 的本地化字符串。
        /// </summary>
		public static string RotateAroundY = nameof(RotateAroundY);

        /// <summary>
        ///   查找类似 绕Z轴旋转 的本地化字符串。
        /// </summary>
		public static string RotateAroundZ = nameof(RotateAroundZ);

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
        ///   查找类似 运行 的本地化字符串。
        /// </summary>
		public static string Run = nameof(Run);

        /// <summary>
        ///   查找类似 下一步 的本地化字符串。
        /// </summary>
		public static string RunNext = nameof(RunNext);

        /// <summary>
        ///   查找类似 单步运行 的本地化字符串。
        /// </summary>
		public static string RunOne = nameof(RunOne);

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
        ///   查找类似 保存项目 的本地化字符串。
        /// </summary>
		public static string SaveProject = nameof(SaveProject);

        /// <summary>
        ///   查找类似 是否保存工程? 的本地化字符串。
        /// </summary>
		public static string SaveProjectTask = nameof(SaveProjectTask);

        /// <summary>
        ///   查找类似 报告另存完成 的本地化字符串。
        /// </summary>
		public static string SaveReportAsCompleted = nameof(SaveReportAsCompleted);

        /// <summary>
        ///   查找类似 保存成功 的本地化字符串。
        /// </summary>
		public static string SaveSuccess = nameof(SaveSuccess);

        /// <summary>
        ///   查找类似 屏幕快照 的本地化字符串。
        /// </summary>
		public static string ScreenShot = nameof(ScreenShot);

        /// <summary>
        ///   查找类似 滚动模式 的本地化字符串。
        /// </summary>
		public static string ScrollMode = nameof(ScrollMode);

        /// <summary>
        ///   查找类似 搜索文件关键字 的本地化字符串。
        /// </summary>
		public static string SearchFileKeywords = nameof(SearchFileKeywords);

        /// <summary>
        ///   查找类似 分割 的本地化字符串。
        /// </summary>
		public static string Segment = nameof(Segment);

        /// <summary>
        ///   查找类似 选择 的本地化字符串。
        /// </summary>
		public static string Select = nameof(Select);

        /// <summary>
        ///   查找类似 1.在任务树选择要标定的点云 的本地化字符串。
        /// </summary>
		public static string SelectCalibratePointCloud = nameof(SelectCalibratePointCloud);

        /// <summary>
        ///   查找类似 请选择可以做测量基准的点、线、面等类型 的本地化字符串。
        /// </summary>
		public static string SelectCorrectMeasurementBenchmark = nameof(SelectCorrectMeasurementBenchmark);

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
        ///   查找类似 设置模板 的本地化字符串。
        /// </summary>
		public static string SetCoordTemplate = nameof(SetCoordTemplate);

        /// <summary>
        ///   查找类似 显示选项设置 的本地化字符串。
        /// </summary>
		public static string SetDisPlayOption = nameof(SetDisPlayOption);

        /// <summary>
        ///   查找类似 尺寸设置 的本地化字符串。
        /// </summary>
		public static string SetMeasure = nameof(SetMeasure);

        /// <summary>
        ///   查找类似 单页设置 的本地化字符串。
        /// </summary>
		public static string SetSinglePage = nameof(SetSinglePage);

        /// <summary>
        ///   查找类似 显示包围盒 的本地化字符串。
        /// </summary>
		public static string ShowBoundBox = nameof(ShowBoundBox);

        /// <summary>
        ///   查找类似 单页 的本地化字符串。
        /// </summary>
		public static string SinglePage = nameof(SinglePage);

        /// <summary>
        ///   查找类似 尺寸 的本地化字符串。
        /// </summary>
		public static string Size = nameof(Size);

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
        ///   查找类似 凌云光3D测量软件初始版本集成：几何特征新建、坐标系生成、标定、点云处理、对齐、比较、距离测量、角度测量与公差测量等 的本地化字符串。
        /// </summary>
		public static string SoftwareFunctions = nameof(SoftwareFunctions);

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
        ///   查找类似 空格 的本地化字符串。
        /// </summary>
		public static string Space = nameof(Space);

        /// <summary>
        ///   查找类似 球体 的本地化字符串。
        /// </summary>
		public static string Sphere = nameof(Sphere);

        /// <summary>
        ///   查找类似 按位读取 的本地化字符串。
        /// </summary>
		public static string SplitIntToBit = nameof(SplitIntToBit);

        /// <summary>
        ///   查找类似 标准值 的本地化字符串。
        /// </summary>
		public static string StandardValue = nameof(StandardValue);

        /// <summary>
        ///   查找类似 起始列 的本地化字符串。
        /// </summary>
		public static string StartColumn = nameof(StartColumn);

        /// <summary>
        ///   查找类似 起始行 的本地化字符串。
        /// </summary>
		public static string StartRow = nameof(StartRow);

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
        ///   查找类似 存储地址 的本地化字符串。
        /// </summary>
		public static string StorageAddress = nameof(StorageAddress);

        /// <summary>
        ///   查找类似 直线度 的本地化字符串。
        /// </summary>
		public static string Straightness = nameof(Straightness);

        /// <summary>
        ///   查找类似 字符串拼接窗口 的本地化字符串。
        /// </summary>
		public static string StringExDialog = nameof(StringExDialog);

        /// <summary>
        ///   查找类似 字符匹配对话框 的本地化字符串。
        /// </summary>
		public static string StringMatchDialog = nameof(StringMatchDialog);

        /// <summary>
        ///   查找类似 字符拼接 的本地化字符串。
        /// </summary>
		public static string StringMerge = nameof(StringMerge);

        /// <summary>
        ///   查找类似 字符解析 的本地化字符串。
        /// </summary>
		public static string StringParse = nameof(StringParse);

        /// <summary>
        ///   查找类似 表面距离 的本地化字符串。
        /// </summary>
		public static string SurfaceDistance = nameof(SurfaceDistance);

        /// <summary>
        ///   查找类似 面轮廓度 的本地化字符串。
        /// </summary>
		public static string SurfaceProfile = nameof(SurfaceProfile);

        /// <summary>
        ///   查找类似 多分支 的本地化字符串。
        /// </summary>
		public static string Switch = nameof(Switch);

        /// <summary>
        ///   查找类似 条件弹窗 的本地化字符串。
        /// </summary>
		public static string SwitchDialog = nameof(SwitchDialog);

        /// <summary>
        ///   查找类似 条件任务 的本地化字符串。
        /// </summary>
		public static string SwitchGroup = nameof(SwitchGroup);

        /// <summary>
        ///   查找类似 任务流 的本地化字符串。
        /// </summary>
		public static string TaskFlow = nameof(TaskFlow);

        /// <summary>
        ///   查找类似 任务模拟器 的本地化字符串。
        /// </summary>
		public static string TaskSimulator = nameof(TaskSimulator);

        /// <summary>
        ///   查找类似 最少三组 的本地化字符串。
        /// </summary>
		public static string ThreeGroups = nameof(ThreeGroups);

        /// <summary>
        ///   查找类似 耗时 的本地化字符串。
        /// </summary>
		public static string Time = nameof(Time);

        /// <summary>
        ///   查找类似 提示 的本地化字符串。
        /// </summary>
		public static string Tip = nameof(Tip);

        /// <summary>
        ///   查找类似 标题 的本地化字符串。
        /// </summary>
		public static string Title = nameof(Title);

        /// <summary>
        ///   查找类似 公差 的本地化字符串。
        /// </summary>
		public static string Tolerance = nameof(Tolerance);

        /// <summary>
        ///   查找类似 公差测量主要包含：平行度、直线度、面轮廓度、线轮廓度、平面度、圆度等 的本地化字符串。
        /// </summary>
		public static string ToleranceMeasurementsInclude = nameof(ToleranceMeasurementsInclude);

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
        ///   查找类似 教程文档不存在 的本地化字符串。
        /// </summary>
		public static string TutorialDontExist = nameof(TutorialDontExist);

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
        ///   查找类似 类别 的本地化字符串。
        /// </summary>
		public static string Type = nameof(Type);

        /// <summary>
        ///   查找类似 未知 的本地化字符串。
        /// </summary>
		public static string Unknown = nameof(Unknown);

        /// <summary>
        ///   查找类似 未知大小 的本地化字符串。
        /// </summary>
		public static string UnknownSize = nameof(UnknownSize);

        /// <summary>
        ///   查找类似 更新 的本地化字符串。
        /// </summary>
		public static string Update = nameof(Update);

        /// <summary>
        ///   查找类似 公差上限 的本地化字符串。
        /// </summary>
		public static string UpperLimit = nameof(UpperLimit);

        /// <summary>
        ///   查找类似 使用教程 的本地化字符串。
        /// </summary>
		public static string UsingTutorials = nameof(UsingTutorials);

        /// <summary>
        ///   查找类似 值 的本地化字符串。
        /// </summary>
		public static string Value = nameof(Value);

        /// <summary>
        ///   查找类似 版本信息窗 的本地化字符串。
        /// </summary>
		public static string VersionInfoDialog = nameof(VersionInfoDialog);

        /// <summary>
        ///   查找类似 顶点 的本地化字符串。
        /// </summary>
		public static string Vertex = nameof(Vertex);

        /// <summary>
        ///   查找类似 垂直度 的本地化字符串。
        /// </summary>
		public static string Verticality = nameof(Verticality);

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
        ///   查找类似 显示标签 的本地化字符串。
        /// </summary>
		public static string VisibleLabel = nameof(VisibleLabel);

        /// <summary>
        ///   查找类似 警告 的本地化字符串。
        /// </summary>
		public static string Warning = nameof(Warning);

        /// <summary>
        ///   查找类似 宽方向 的本地化字符串。
        /// </summary>
		public static string WDirection = nameof(WDirection);

        /// <summary>
        ///   查找类似 线框 的本地化字符串。
        /// </summary>
		public static string Wireframe = nameof(Wireframe);

        /// <summary>
        ///   查找类似 是 的本地化字符串。
        /// </summary>
		public static string Yes = nameof(Yes);

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
