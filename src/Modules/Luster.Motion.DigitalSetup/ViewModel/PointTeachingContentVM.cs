using DocumentFormat.OpenXml.Bibliography;
using HandyControl.Data;
using Luster.Common.Assets;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.EditorUI;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    public class PointTeachingContentVM : BaseAss
    {
        String OffsetJsonPath;
        String AxisPositionsPath;
        private const string PageName = "PointTeaching";
        private readonly IMotionController _mController;
        private readonly IDeviceEngine _deviceEngine;
        private readonly FlowBus flowBus;
        /// <summary>
        /// 弹窗服务
        /// </summary>
        private IDialogService _dialogService;

        //防止多次更新点位
        private Dictionary<string, bool> _axisUpdatedDict = new Dictionary<string, bool>();

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }

        /// <summary>
        /// 进度条
        /// </summary>
        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }


        public override string SelectedStationName
        {
            get => _selectedStationName;
            set
            {
                if (_selectedStationName != value)
                {
                    base.SelectedStationName = value;
                    LoadOffsetsFromJson();
                }
            }
        }

        private CommonPageModel _seletedReportPage;
        public new CommonPageModel SelectedReportPage
        {
            get => _seletedReportPage;
            set
            {
                if (_seletedReportPage != null)
                {
                    if (value != null && _seletedReportPage?.Name != value.Name)
                    {
                        SaveGridItems(ItemModels);
                    }

                }
                SetProperty(ref _seletedReportPage, value);

                //同步赋值给基类属性
                base.SelectedReportPage = value;
                if (_seletedReportPage == null) return;
                ConfigKey = _seletedReportPage.Name;
                // 加载界面属性
                LoadStationConfigFromJson();
                //更新界面属性
                UpdateStationConfigs();
                // 加载工站点检状态
                LoadStationCheckStatus();

            }
        }


        public PointTeachingContentVM(IRepository repository,
                                      IDialogService dialogService,
                                      IRegionManager regionManager,
                                      IMotionController motionController,
                                      IDeviceEngine deviceEngine,
                                      FlowBus _flowBus,
                                      ICommonBus commonBus,
                                      CSVHelper cSVHelper,
                                      CheckStatusService checkStatusService)
                                                    : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _dialogService = dialogService;
            _mController = motionController;
            _parentRegionName = "PointTeachingContent";

            //获取当前的轴列表
            LoadDevices();

            Pages = new ObservableCollection<CommonPageModel>();
            for (int i = 0; i < AxisList.Count; i++)
            {
                Pages.Add(new CommonPageModel() { Name = AxisList[i].Name, IsSelected = false, Region = "", ViewType = null });
            }

            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("PointTeachingContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            EndCommand = new DelegateCommand(OnEnd);
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            //OneKeyCheckCommand = new DelegateCommand(OnOneKeyCheck);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);

            var recipeDir = _commonbus.CurrentRecipe.GetRecipePath();
            AxisPositionsPath = Path.Combine(recipeDir, "db", "Ass_Data", "Ass_GodLine", " AxisPositions.json");
            OffsetJsonPath = Path.Combine(recipeDir, "db", "Ass_Data", "AxisOffSet.json");

            // 监听StationConfigs集合变化
            AxisPositions.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (PositionItem sc in e.NewItems)
                    {
                        sc.PropertyChanged += AxisPositions_PropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (PositionItem sc in e.OldItems)
                    {
                        sc.PropertyChanged -= AxisPositions_PropertyChanged;
                    }
                }
            };
            // 初始化时为已有元素添加监听
            foreach (var sc in AxisPositions)
            {
                sc.PropertyChanged += AxisPositions_PropertyChanged;
            }
            LoadCheckConfirmMessages();

            // 延迟加载点检状态，确保 UI 绑定已建立
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 加载所有子页面的历史点检状态
        /// </summary>
        private void LoadCheckStatusForAllPages()
        {
            if (_checkStatusService == null || Pages == null)
                return;

            try
            {
                foreach (var page in Pages)
                {
                    if (page != null)
                    {
                        page.ParentRegion = "PointTeachingContent";
                        var record = _checkStatusService.GetRecord(page.PageKey);
                        if (record != null)
                        {
                            page.CheckStatus = record.Status;
                        }
                        else
                        {
                            page.CheckStatus = CheckStatus.NotChecked;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载点检状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新点检状态 - 每次页面激活时调用
        /// </summary>
        protected override void RefreshCheckStatus()
        {
            LoadCheckStatusForAllPages();
        }

        #region  示教点位管理
        /// <summary>
        /// 轴列表
        /// </summary>
        private ObservableCollection<AxisModel> axisList;
        public ObservableCollection<AxisModel> AxisList
        {
            get { return axisList; }
            set { SetProperty(ref axisList, value); }
        }

        /// <summary>
        /// 初始化加载
        /// </summary>
        /// <param name="key"></param>
        private void LoadDevices(string key = "")
        {
            var devices = _deviceEngine.GetDevices(typeof(VAxis));
            AxisList = new ObservableCollection<AxisModel>();
            foreach (var device in devices)
            {
                var vAxis = device as VAxis;
                if (vAxis.Name.Contains("Z")) //去掉Z轴
                {
                    continue;
                }
                var axisModel = new AxisModel(vAxis);
                AxisList.Add(axisModel);
            }
        }

        private ObservableCollection<PositionItem> _axisPositions = new ObservableCollection<PositionItem>();
        /// <summary>
        /// 轴示教位列表
        /// </summary>
        public ObservableCollection<PositionItem> AxisPositions
        {
            get { return _axisPositions; }
            set { SetProperty(ref _axisPositions, value); }
        }

        private PositionItem _axisPositionsSelectedItem;
        public PositionItem AxisPositionsSelectedItem
        {
            get { return _axisPositionsSelectedItem; }
            set { SetProperty(ref _axisPositionsSelectedItem, value); }
        }

        private ObservableCollection<PositionItem> _axisPositions_GodLine;
        /// <summary>
        /// GodLine轴示教位列表
        /// </summary>
        public ObservableCollection<PositionItem> AxisPositions_GodLine
        {
            get { return _axisPositions_GodLine; }
            set { SetProperty(ref _axisPositions_GodLine, value); }
        }

        private double _offsetX;
        public double OffsetX
        {
            get { return _offsetX; }
            set
            {
                var ss = Math.Round(value, 3);
                _offsetX = ss;
                RaisePropertyChanged(nameof(OffsetX));
            }
        }
        private double _offsetY;
        public double OffsetY
        {
            get { return _offsetY; }
            set { SetProperty(ref _offsetY, value); }
        }
        private double _offsetU;
        public double OffsetU
        {
            get { return _offsetU; }
            set { SetProperty(ref _offsetU, value); }
        }

        /// <summary>
        /// 子界面切换时加载示教点位
        /// </summary>
        private void LoadCurrentPoints(AxisModel Current)
        {
            AxisPositions.Clear();
            var lNodes = Current.Tag.GetAxisPosNodes();
            foreach (var node in lNodes.Children)
            {
                var axisPosition = (AxisPosition)node.Tag;
                var position = new PositionItem()
                {
                    Name = axisPosition.Name,
                    Position = axisPosition.Position,
                    Tag = axisPosition
                };
                AxisPositions.Add(position);
            }

        }

        /// <summary>
        /// 将示教点位保存到JSON文件
        /// </summary>
        /// <summary>
        /// 将示教点位保存到JSON文件
        /// </summary>
        private void SaveGodLineTeachPoint()
        {
            try
            {
                // 判断文件是否存在，不存在则新建空的JSON对象
                JObject jObj;
                if (!File.Exists(AxisPositionsPath))
                {
                    jObj = new JObject();
                    Directory.CreateDirectory(Path.GetDirectoryName(AxisPositionsPath));
                    File.WriteAllText(AxisPositionsPath, jObj.ToString(Newtonsoft.Json.Formatting.Indented));
                }
                else
                {
                    // 读取已存在的JSON，避免覆盖其他轴数据
                    jObj = JObject.Parse(File.ReadAllText(AxisPositionsPath));
                }

                foreach (var axis in AxisList)
                {
                    // 加载每个轴的点位
                    var lNodes = axis.Tag.GetAxisPosNodes();
                    var axisPositions = new JObject();
                    foreach (var node in lNodes.Children)
                    {
                        var axisPosition = (AxisPosition)node.Tag;
                        axisPositions[axisPosition.Name] = axisPosition.Position;
                    }
                    jObj[axis.Name] = axisPositions;
                }

                File.WriteAllText(AxisPositionsPath, jObj.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"保存GodLine示教点位失败,{ex.Message}" });
                throw;
            }
        }
        /// <summary>
        /// 加载GodLine示教点位
        /// </summary>
        private void LoadGodLineTeachPoint()
        {
            try
            {
                var axisName = SelectedReportPage.Name;
                if (!File.Exists(AxisPositionsPath) || string.IsNullOrEmpty(axisName))
                    return;

                var json = File.ReadAllText(AxisPositionsPath);
                var jObj = JObject.Parse(json);
                var axisToken = jObj[axisName];
                if (axisToken == null || axisToken.Type != JTokenType.Object)
                    return;

                AxisPositions_GodLine = new ObservableCollection<PositionItem>();
                foreach (var prop in ((JObject)axisToken).Properties())
                {
                    double posValue = 0;
                    if (double.TryParse(prop.Value.ToString(), out posValue))
                    {
                        AxisPositions_GodLine.Add(new PositionItem
                        {
                            Name = prop.Name,
                            Position = posValue,
                            Tag = null // 可根据需要设置Tag
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"加载GodLine示教点位失败,{ex.Message}" });
                throw;
            }
        }
        #endregion

        #region commands

        //UpdatePositionsCommand
        private DelegateCommand<PositionItem> _updatePositionsCommand;
        public DelegateCommand<PositionItem> UpdatePositionsCommand => _updatePositionsCommand ?? (_updatePositionsCommand = new DelegateCommand<PositionItem>((o) =>
        {
            var axisName = SelectedReportPage?.Name;
            if (string.IsNullOrEmpty(axisName))
                throw new FriendlyException("未选择轴！");
            if (_axisUpdatedDict.TryGetValue(axisName, out bool updated) && updated)
                throw new FriendlyException("该轴点位已经更新！");
            if (AxisPositions == null || AxisPositions.Count == 0) return;

            _dialogService.ShowConfirm("是否进行点位更新？（每个轴只能更新一次）", r =>
            {
                if (r.Result != ButtonResult.OK)
                    return;

                foreach (var item in AxisPositions)
                {
                    if (item.Name != null && item.Name.Contains("示教位"))
                        continue;
                    if (item.Name != null && AxisPositionsSelectedItem != null && string.Equals(item.Name, AxisPositionsSelectedItem.Name))
                    {
                        item.Position += OffsetX;
                    }
                }
                SaveGodLineTeachPoint();
                _axisUpdatedDict[axisName] = true; // 标记当前轴已更新
            });
        }));

        //UpdateGodLinePositionsCommand
        private DelegateCommand<PositionItem> _updateGodLinePositionsCommand;
        public DelegateCommand<PositionItem> UpdateGodLinePositionsCommand => _updateGodLinePositionsCommand ?? (_updateGodLinePositionsCommand = new DelegateCommand<PositionItem>((o) =>
        {
            SaveGodLineTeachPoint();
        }));

        /// <summary>
        /// 点位运动指令
        /// </summary>
        private DelegateCommand<PositionItem> _moveTeachCommand;
        public DelegateCommand<PositionItem> MoveTeachCommand => _moveTeachCommand ?? (_moveTeachCommand = new DelegateCommand<PositionItem>((pos) =>
        {
            if (pos == null) return;
            pos.Tag.Axis.MoveAbs(pos.Position);

        }));

        #endregion

        //切换子界面时加载对应的示教点位
        public override void PageUpdated(FunctionEventArgs<int> obj)
        {
            if (SelectedReportPage == null) return;
            var select = SelectedReportPage.Name;
            var Current = AxisList.Where(x => x.Name == select).FirstOrDefault();
            LoadCurrentPoints(Current);
            LoadGodLineTeachPoint();

            // 每次切换轴时，自动根据示教位更新OffsetX
            UpdateOffsetXByTeachPoint();

            // 切换轴时不重置所有轴的已更新状态，只影响当前轴
            // 若需要切换后允许再次更新，可在此处重置当前轴的状态
            // _axisUpdatedDict[select] = false;
        }
        private void UpdateOffsetXByTeachPoint()
        {
            if (AxisPositions == null || AxisPositions_GodLine == null) return;
            // 查找同名“示教位”点
            var teachItem = AxisPositions.FirstOrDefault(x => x.Name != null && x.Name.Contains("示教位"));
            var godLineItem = AxisPositions_GodLine?.FirstOrDefault(x => x.Name == teachItem?.Name);
            if (teachItem != null && godLineItem != null)
            {
                OffsetX = teachItem.Position - godLineItem.Position;
            }
        }
        public override async void OnOneKeyCheck(object obj)
        {
            await base.OnOneKeyCheckAsync(obj);
            bool wasCancelled = false;

            try
            {
                ProgressValue = 0; // 进度

                if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                {
                    var stations = _mController.MotionEngine.GetStations();
                    var stat = stations?.FirstOrDefault(s => string.Equals(s?.Alias, "点位示教"));
                    if (stat != null)
                    {
                        flowBus.OnRunOne(stat.ID);
                        await Task.Run(async () =>
                        {

                            while (stat.Status != RunStatus.Success)
                            {
                                await Task.Delay(200); // 200ms轮询
                            }
                        }, _cts.Token);
                    }
                    else
                    {
                        throw new FriendlyException("未找到点位示教站点");
                    }
                }
                else
                {
                    throw new FriendlyException("回零完成后方可运行测试流程");
                }
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "点位示教被用户中止" });
                throw;
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"手眼标定失败,{ex.Message}" });
                throw;
            }
            finally
            {
                ProgressValue = 100;

                // 保存当前子页面的点检状态
                var checkStatus = CheckStatus.NotChecked;
                string remark = "";

                if (wasCancelled && _cts.IsCancellationRequested)
                {
                    // 用户中止 - 点位示教不支持继续，需从头开始
                    checkStatus = CheckStatus.CheckedFail;
                    remark = "执行中止，需从头开始";
                }
                else
                {
                    checkStatus = CheckStatus.CheckedOK;
                    remark = "点位示教完成";
                }

                SaveCheckStatus(checkStatus, remark);

                // 同步一级界面整体状态到 PageStatusService
                SyncOverallStatusToPageStatusService();
            }
        }

        /// <summary>
        /// 点位示教不使用 AssTb 表格模式，ItemModels 中是 PositionItem，
        /// 基类的 GetCurrentPageCheckStatus() 无法识别，因此重写为直接读取已保存的点检状态。
        /// </summary>
        protected override CheckStatus GetCurrentPageCheckStatus()
        {
            if (SelectedReportPage == null)
                return CheckStatus.NotChecked;

            return SelectedReportPage.CheckStatus;
        }

        /// <summary>
        /// 重写状态同步：点位示教是轴维度的页面，不适用基类中工站维度的聚合逻辑。
        /// 基类的 UpdateAllSubPagesCheckStatus 会遍历所有工站，因只有当前工站有状态
        /// 而把已保存的 OK 覆盖为 NotChecked。这里直接使用已保存的子页面状态。
        /// </summary>
        protected override void SyncOverallStatusToPageStatusService()
        {
            var currentPageStatus = GetCurrentPageCheckStatus();
            string pageStatusText = currentPageStatus switch
            {
                CheckStatus.CheckedOK => "OK",
                CheckStatus.CheckedFail => "NG",
                _ => "未点检"
            };

            string pageStatusName = DigitalAssPageModel.GetNameByRegion(_parentRegionName);
            if (string.IsNullOrEmpty(pageStatusName))
                return;

            // 更新 PageStatusService
            PageStatusService.Instance.UpdateStatus(pageStatusName, pageStatusText);

            // 更新 DigitalAssPageModel 的聚合状态（基于所有轴子页面的状态）
            var overallStatus = GetOverallCheckStatus();
            string overallStatusText = overallStatus switch
            {
                CheckStatus.CheckedOK => "OK",
                CheckStatus.CheckedFail => "NG",
                _ => "未点检"
            };
            PageStatusService.Instance.UpdateStatus(pageStatusName, overallStatusText);

            // 更新 DigitalAssPageModel 的 CheckStatus（用于一级界面状态圆圈显示）
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                var parentPage = DigitalAssPageModel.FindPageByRegion(_parentRegionName);
                if (parentPage != null)
                {
                    parentPage.CheckStatus = GetOverallCheckStatus();
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SaveOffsetsToJson()
        {
            Dictionary<string, OffsetData> allOffsets = new Dictionary<string, OffsetData>();
            if (File.Exists(OffsetJsonPath))
            {
                var json = File.ReadAllText(OffsetJsonPath);
                allOffsets = JsonConvert.DeserializeObject<Dictionary<string, OffsetData>>(json) ?? new Dictionary<string, OffsetData>();
            }
            allOffsets[SelectedStationName] = new OffsetData
            {
                offsetX = this.OffsetX,
                offsetY = this.OffsetY,
                offsetU = this.OffsetU
            };
            File.WriteAllText(OffsetJsonPath, JsonConvert.SerializeObject(allOffsets, Formatting.Indented));
        }

        private void LoadOffsetsFromJson()
        {
            if (File.Exists(OffsetJsonPath))
            {
                var json = File.ReadAllText(OffsetJsonPath);
                var allOffsets = JsonConvert.DeserializeObject<Dictionary<string, OffsetData>>(json);
                if (allOffsets != null && allOffsets.TryGetValue(SelectedStationName, out var data))
                {
                    OffsetX = data.offsetX;
                    OffsetY = data.offsetY;
                    OffsetU = data.offsetU;
                }
            }
        }

        // 监听方法
        public void AxisPositions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 只处理 Position 属性变化
            if (e.PropertyName != nameof(PositionItem.Position))
                return;

            var changedItem = sender as PositionItem;
            if (changedItem == null)
                return;

            // 判断是否为“示教位”点
            if (changedItem.Name == null || !changedItem.Name.Contains("示教位"))
                return;

            // 查找 AxisPositions_GodLine 中同名点位
            if (AxisPositions_GodLine == null)
                return;

            var godLineItem = AxisPositions_GodLine.FirstOrDefault(x => x.Name == changedItem.Name);
            if (godLineItem == null)
                return;

            // 计算偏差并更新 OffsetX
            OffsetX = changedItem.Position - godLineItem.Position;
        }

        public void FillTableContent(AssTb visualCali)
        {
            if (string.IsNullOrEmpty(visualCali.实测))
            {
                visualCali.状态 = "未完成";
            }
            else if (visualCali.标准 == visualCali.实测)
            {
                visualCali.状态 = "OK";
            }
            else if (visualCali.标准.Contains('~'))
            {
                var range = ParseColumnRange(visualCali.标准);
                double.TryParse(visualCali.实测, out double 实测浮点值);
                if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                {
                    visualCali.状态 = "OK";
                }
                else
                {
                    visualCali.状态 = "NG";
                }
            }
            else
            {
                visualCali.状态 = "NG";
            }
        }
        private void OnUpdateItems()
        {
        }
        public override void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }
        private static (double lower, double upper) ParseColumnRange(string standardValue)
        {
            // 1) 不含 ~ ：按单个数字处理
            if (!standardValue.Contains('~'))
            {
                if (!double.TryParse(standardValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    throw new FormatException("第一列单值格式非法");
                return (v, v);
            }

            // 2) 含 ~ ：必须是“下限~上限”且仅出现一次 ~
            string[] tokens = standardValue.Split('~');
            if (tokens.Length != 2)
                throw new FormatException("第一列区间只能包含一个 '~'");

            string lowerStr = tokens[0].Trim();
            string upperStr = tokens[1].Trim();

            if (!double.TryParse(lowerStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double lower) ||
                !double.TryParse(upperStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double upper))
                throw new FormatException("第一列区间上下限格式非法");

            if (lower > upper)
                throw new FormatException("第一列区间下限不能大于上限");

            return (lower, upper);
        }
    }
    public class OffsetData
    {
        public double offsetX { get; set; }
        public double offsetY { get; set; }
        public double offsetU { get; set; }
    }
}
