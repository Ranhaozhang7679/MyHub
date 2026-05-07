using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.ReportUI.Model;
using Luster.Motion.ReportUI.Views;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.ReportUI.ViewModel
{
    /// <summary>
    /// CT统计实时页面ViewModel
    ///
    /// 功能说明：
    /// 1. 管理多个子Tab页（1-10个动态tab）
    /// 2. 每个tab包含一个CT统计表格
    /// 3. 表格结构：
    ///    - 前五列固定：时间、SN、总CT、净CT、等待时间
    ///    - 第6列起：动态CT列（5-20列）
    ///    - 第1行：TargetCT行（浅绿色背景）
    ///    - 第2行：表头行（浅蓝色背景）
    ///    - 第3行起：数据行（最多8行，FIFO队列）
    /// </summary>
    public class CTLogStatRTContentVM : ReportBaseVM
    {
        /// <summary>
        /// 所有子Tab页的集合
        /// </summary>
        public ObservableCollection<CTStatTabPageModel> TabPages { get; } = new ObservableCollection<CTStatTabPageModel>();

        /// <summary>
        /// 当前选中的Tab页
        /// </summary>
        private CTStatTabPageModel _selectedTabPage;
        public CTStatTabPageModel SelectedTabPage
        {
            get => _selectedTabPage;
            set => SetProperty(ref _selectedTabPage, value);
        }

        public override string ReportName => "CTLogStatRealTime";

        private IMotionController _mController;
        private IDbManager _dbManager;
        private IEventAggregator _eventAggregator;
        private SubscriptionToken _eventToken;

        /// <summary>
        /// 页面是否处于活跃状态（当前显示中）
        /// </summary>
        private bool _isActive = true;

        public CTLogStatRTContentVM() : base()
        {
        }

        public CTLogStatRTContentVM(IRepository reporitory, IMotionController motionController, IDbManager dbManager, IEventAggregator eventAggregator) : base(reporitory, motionController)
        {
            _mController = motionController;
            _dbManager = dbManager;
            _eventAggregator = eventAggregator;

            // 订阅CT统计实时数据事件
            _eventToken = _eventAggregator.GetEvent<CTStatRealTimeEvent>().Subscribe(OnCTStatDataReceived);

            // 从 DbManager 获取站名称列表并创建 Tab 页
            InitializeTabPages();
        }

        /// <summary>
        /// 初始化Tab页：从 DbManager 获取站名称列表并创建对应的Tab页
        /// </summary>
        private void InitializeTabPages()
        {
            try
            {
                var stationNames = _dbManager?.GetCTConfigStationNames();
                var fullActionNames = _dbManager?.GetCTConfigFullActionNames();

                if (stationNames != null && stationNames.Count > 0)
                {
                    foreach (var stationName in stationNames)
                    {
                        if (!TabPages.Any(t => t.TabName == stationName))
                        {
                            var tabPage = new CTStatTabPageModel(stationName);
                            TabPages.Add(tabPage);

                            // 用配置的完整动作列表预初始化列（所有步序都显示，目标值默认0.000）
                            if (fullActionNames != null && fullActionNames.TryGetValue(stationName, out var actions))
                            {
                                var initTargetData = new Dictionary<string, string>();
                                foreach (var action in actions)
                                {
                                    initTargetData[action] = "0.000";
                                }
                                tabPage.InitializeTargetRow(null, initTargetData);
                            }
                        }
                    }
                }
                else
                {
                    // 没有配置数据时，创建测试 Tab 页
                    //System.Diagnostics.Debug.WriteLine($"[CTStatRT] 没有配置数据，创建测试 Tab 页");
                    var testTabPage = new CTStatTabPageModel("测高工站");
                    TabPages.Add(testTabPage);

                    // 初始化测试数据
                    var testDynamicData = new Dictionary<string, string>
                    {
                        { "动作1", "1.500" },
                        { "动作2", "2.300" },
                        { "动作3", "0.800" },
                        { "动作4", "1.200" },
                        { "动作5", "3.100" },
                        { "动作6", "0.500" }
                    };
                    testTabPage.InitializeTargetRow(null, testDynamicData);

                    // 添加几条测试数据
                    for (int i = 1; i <= 5; i++)
                    {
                        var testData = new Dictionary<string, string>
                        {
                            { "动作1", (1.5 + i * 0.1).ToString("F3") },
                            { "动作2", (2.3 + i * 0.1).ToString("F3") },
                            { "动作3", (0.8 + i * 0.1).ToString("F3") },
                            { "动作4", (1.2 + i * 0.1).ToString("F3") },
                            { "动作5", (3.1 + i * 0.1).ToString("F3") },
                            { "动作6", (0.5 + i * 0.1).ToString("F3") }
                        };
                        testTabPage.AddDataRow(new CTFixedColumnData
                        {
                            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            SN = $"TEST{i:000}"
                        }, testData);
                    }
                }

                // 默认选中第一个 Tab
                if (SelectedTabPage == null && TabPages.Count > 0)
                {
                    SelectedTabPage = TabPages[0];
                }

                //System.Diagnostics.Debug.WriteLine($"[CTStatRT] 初始化完成，Tab 页总数: {TabPages.Count}");
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"InitializeTabPages Error: {ex.Message}");
                // 发生异常时也创建一个测试 Tab 页
                var testTabPage = new CTStatTabPageModel("测试工站");
                TabPages.Add(testTabPage);
                SelectedTabPage = testTabPage;
            }
        }

        /// <summary>
        /// 处理 CT 统计实时数据事件
        /// </summary>
        /// <param name="ctInfoList">CT信息列表</param>
        private void OnCTStatDataReceived(List<TbCTInfo2> ctInfoList)
        {
            if (!_isActive) return;
            if (ctInfoList == null || ctInfoList.Count == 0) return;

            //System.Diagnostics.Debug.WriteLine($"[CTStatRT] 收到数据，共 {ctInfoList.Count} 条");

            // 复制数据到局部变量，避免异步访问时的线程问题
            var capturedList = ctInfoList.ToList();

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // 按 Time_Slot 分组（每个周期的数据）
                var groupedBySlot = capturedList.Where(x => x.Time_Slot != DateTime.MinValue)
                                                .GroupBy(x => x.Time_Slot);

                foreach (var slotGroup in groupedBySlot)
                {
                    // 获取该组数据中第一个有效元素的 SN 和 开始时间
                    var firstItem = slotGroup.FirstOrDefault(x => !string.IsNullOrEmpty(x.模块) && !x.动作.Contains("开始") && !x.动作.Contains("结束"))
                                    ?? slotGroup.FirstOrDefault();
                    if (firstItem == null) continue;

                    string sn = firstItem.SN ?? "";
                    string startTime = firstItem.开始时间.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    // 按站名（模块）分组处理
                    var groupedByModule = slotGroup.Where(x => !string.IsNullOrEmpty(x.模块))
                                                   .GroupBy(x => x.模块);

                    foreach (var moduleGroup in groupedByModule)
                    {
                        string stationName = moduleGroup.Key;
                        if (string.IsNullOrEmpty(stationName)) continue;

                        //System.Diagnostics.Debug.WriteLine($"[CTStatRT] 处理站: {stationName}, 该站数据数: {moduleGroup.Count()}");

                        // 查找或创建对应的 Tab 页
                        var tabPage = TabPages.FirstOrDefault(t => t.TabName == stationName);
                        if (tabPage == null)
                        {
                            tabPage = new CTStatTabPageModel(stationName);
                            TabPages.Add(tabPage);
                            //System.Diagnostics.Debug.WriteLine($"[CTStatRT] 创建新Tab页: {stationName}");
                        }

                        // 提取该站的动作列数据（包含所有动作，不省略第一个和最后一个）
                        var actionItems = moduleGroup.OrderBy(x => x.动作).ToList();

                        //System.Diagnostics.Debug.WriteLine($"[CTStatRT] 动作项数量: {actionItems.Count}");
                        foreach (var item in actionItems)
                        {
                            //System.Diagnostics.Debug.WriteLine($"[CTStatRT]   动作: {item.动作}, Target_CT: {item.Target_CT}, Actual_CT: {item.Actual_CT}");
                        }

                        if (actionItems.Count == 0) continue;

                        // 如果 Tab 页未初始化（未预初始化的情况），用实时数据初始化
                        // 如果已预初始化（列已从配置生成），则更新目标值
                        if (!tabPage.IsInitialized)
                        {
                            var targetData = new Dictionary<string, string>();
                            foreach (var item in actionItems)
                            {
                                targetData[item.动作] = item.Target_CT.ToString("F3");
                            }
                            tabPage.InitializeTargetRow(null, targetData);
                        }
                        else
                        {
                            // 已预初始化，更新目标值（首次收到数据时）
                            var targetData = new Dictionary<string, string>();
                            foreach (var item in actionItems)
                            {
                                targetData[item.动作] = item.Target_CT.ToString("F3");
                            }
                            tabPage.UpdateTargetValues(targetData);
                        }

                        // 准备动态列数据（Actual_CT）
                        var dynamicData = new Dictionary<string, string>();
                        foreach (var item in actionItems)
                        {
                            dynamicData[item.动作] = item.Actual_CT.ToString("F3");
                        }

                        //System.Diagnostics.Debug.WriteLine($"[CTStatRT] 添加数据行: Time={startTime}, SN={sn}, 动态列数={dynamicData.Count}");

                        // 判断是否包含"工站开始"：有则新建一行，无则追加到当前行
                        bool hasStationStart = actionItems.Any(x => x.动作.Contains("工站开始"));

                        if (hasStationStart)
                        {
                            // 新周期开始 — 新建一行
                            var fixedData = new CTFixedColumnData
                            {
                                Time = startTime,
                                SN = sn,
                                TotalCT = "",
                                NetCT = "",
                                WaitTime = ""
                            };
                            tabPage.AddDataRow(fixedData, dynamicData);
                        }
                        else
                        {
                            // 当前周期数据续传 — 追加到当前行
                            tabPage.AppendToCurrentRow(dynamicData);
                        }
                    }
                }

                // 如果当前没有选中 Tab，选中第一个
                if (SelectedTabPage == null && TabPages.Count > 0)
                {
                    SelectedTabPage = TabPages[0];
                }
            }), DispatcherPriority.Background);
        }

        /// <summary>
        /// 【核心方法1】外部调用：添加或更新一个Tab页的数据
        ///
        /// 使用场景：当获取到新的CT数据时调用此方法
        ///
        /// 参数说明：
        /// @param tabName: Tab页名称（如："A1R1飞达供料"）
        /// @param fixedData: 固定列数据（时间、SN、总CT、净CT、等待时间）
        /// @param dynamicData: 动态列数据（列名->值的字典，如："CT1"->"2.5"）
        /// @param isTargetRow: 是否是TargetCT行（用于初始化）
        /// </summary>
        public void AddOrUpdateTabPage(string tabName, CTFixedColumnData fixedData, Dictionary<string, string> dynamicData, bool isTargetRow = false)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var tabPage = TabPages.FirstOrDefault(t => t.TabName == tabName);
                if (tabPage == null)
                {
                    // 创建新Tab页
                    tabPage = new CTStatTabPageModel(tabName);
                    TabPages.Add(tabPage);

                    // 如果是第一次创建，先初始化TargetCT行
                    if (isTargetRow)
                    {
                        tabPage.InitializeTargetRow(fixedData, dynamicData);
                    }
                }

                // 更新数据行
                if (!isTargetRow)
                {
                    tabPage.AddDataRow(fixedData, dynamicData);
                }

                // 如果当前没有选中Tab，自动选中第一个
                if (SelectedTabPage == null && TabPages.Count > 0)
                {
                    SelectedTabPage = TabPages[0];
                }
            }), DispatcherPriority.Background);
        }

        /// <summary>
        /// 【核心方法2】初始化Tab页的TargetCT行和表头行
        ///
        /// 使用场景：Tab页首次创建时调用，设置目标值和列名
        ///
        /// 参数说明：
        /// @param tabName: Tab页名称
        /// @param fixedData: 固定列的目标值（只有总CT、净CT、等待时间有意义）
        /// @param dynamicData: 动态列的列名和目标值
        /// </summary>
        public void InitializeTabPage(string tabName, CTFixedColumnData fixedData, Dictionary<string, string> dynamicData)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var tabPage = TabPages.FirstOrDefault(t => t.TabName == tabName);
                if (tabPage == null)
                {
                    tabPage = new CTStatTabPageModel(tabName);
                    TabPages.Add(tabPage);
                }

                tabPage.InitializeTargetRow(fixedData, dynamicData);
            }), DispatcherPriority.Background);
        }

        /// <summary>
        /// 【核心方法3】向指定Tab页添加一条数据
        ///
        /// 使用场景：实时数据更新时调用
        /// </summary>
        public void AddDataToTabPage(string tabName, CTFixedColumnData fixedData, Dictionary<string, string> dynamicData)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var tabPage = TabPages.FirstOrDefault(t => t.TabName == tabName);
                if (tabPage != null)
                {
                    tabPage.AddDataRow(fixedData, dynamicData);
                }
            }), DispatcherPriority.Background);
        }

        #region 导航接口
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _isActive = false;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _isActive = true;
            base.OnNavigatedTo(navigationContext);
        }
        #endregion
    }

    /// <summary>
    /// 动态列信息（用于XAML绑定）
    /// </summary>
    public class DynamicColumnInfo : BindableBase
    {
        private string _columnName;
        public string ColumnName
        {
            get => _columnName;
            set => SetProperty(ref _columnName, value);
        }

        private string _value;
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private bool _isOverTarget;
        /// <summary>
        /// 是否超过目标值（用于数据行红色高亮）
        /// </summary>
        public bool IsOverTarget
        {
            get => _isOverTarget;
            set => SetProperty(ref _isOverTarget, value);
        }

        private CTRowType _rowType;
        /// <summary>
        /// 行类型（用于判断是表头行还是数据行）
        /// </summary>
        public CTRowType RowType
        {
            get => _rowType;
            set => SetProperty(ref _rowType, value);
        }

        public DynamicColumnInfo() { }

        public DynamicColumnInfo(string columnName, string value)
        {
            ColumnName = columnName;
            Value = value;
        }

        public DynamicColumnInfo(string columnName, string value, bool isOverTarget)
        {
            ColumnName = columnName;
            Value = value;
            IsOverTarget = isOverTarget;
        }

        public DynamicColumnInfo(string columnName, string value, CTRowType rowType)
        {
            ColumnName = columnName;
            Value = value;
            RowType = rowType;
        }
    }

    /// <summary>
    /// 固定列数据结构
    /// 对应前5列：时间、SN、总CT、净CT、等待时间
    /// </summary>
    public class CTFixedColumnData
    {
        /// <summary>列1: 时间</summary>
        public string Time { get; set; }

        /// <summary>列2: SN</summary>
        public string SN { get; set; }

        /// <summary>列3: 总CT</summary>
        public string TotalCT { get; set; }

        /// <summary>列4: 净CT</summary>
        public string NetCT { get; set; }

        /// <summary>列5: 等待时间</summary>
        public string WaitTime { get; set; }
    }

    /// <summary>
    /// CT统计Tab页模型
    ///
    /// 表格结构（固定10行）：
    /// ┌─────────────────────────────────────────────────────┐
    /// │ 行1: TargetCT行  | 浅绿色 | 第1列显示"TargetCT(s)" │
    /// │ 行2: 表头行    | 浅蓝色 | 固定列名+动态列名    │
    /// │ 行3-10: 数据行 | 白色   | 实际数据（FIFO队列） │
    /// └─────────────────────────────────────────────────────┘
    /// </summary>
    public class CTStatTabPageModel : BindableBase
    {
        /// <summary>
        /// Tab页名称
        /// </summary>
        public string TabName { get; }

        /// <summary>
        /// 表格所有行的集合（固定10行）
        /// </summary>
        public ObservableCollection<CTGridRowModel> GridRows { get; } = new ObservableCollection<CTGridRowModel>();

        /// <summary>
        /// 动态列名集合（用于表头绑定）
        /// </summary>
        private List<string> _dynamicColumnNames = new List<string>();
        public List<string> DynamicColumnNames
        {
            get => _dynamicColumnNames;
            private set => SetProperty(ref _dynamicColumnNames, value);
        }

        /// <summary>
        /// 当前数据行写入位置（3-12）
        /// </summary>
        private int _currentDataIndex = 0;

        /// <summary>
        /// 数据行是否已初始化
        /// </summary>
        private bool _isInitialized = false;
        /// <summary>
        /// 公开属性：数据行是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        public CTStatTabPageModel(string tabName)
        {
            TabName = tabName;
            InitializeRows();
            // 默认初始化一些动态列名，避免首次加载时列名集合为空
            _dynamicColumnNames = new List<string>();
        }

        /// <summary>
        /// 初始化表格行结构（创建12个空行）
        /// </summary>
        private void InitializeRows()
        {
            GridRows.Clear();

            // 第1行：TargetCT行（浅绿色背景）
            var targetRow = new CTGridRowModel
            {
                RowType = CTRowType.TargetCT,
                RowIndex = 1,
                BackgroundColor = "#C8E6C9"  // 浅绿色
            };
            // 固定列设置
            targetRow.Time = "TargetCT(s)";  // 第1列显示"TargetCT(s)"
            targetRow.SN = "";
            targetRow.TotalCT = "";
            targetRow.NetCT = "";
            targetRow.WaitTime = "";
            GridRows.Add(targetRow);

            // 第2-9行：数据行（白色背景，共8行）
            for (int i = 1; i <= 8; i++)
            {
                var dataRow = new CTGridRowModel
                {
                    RowType = CTRowType.Data,
                    RowIndex = i,
                    BackgroundColor = "#FFFFFF"  // 白色
                };
                GridRows.Add(dataRow);
            }

            _currentDataIndex = 0;
            _isInitialized = false;
        }

        /// <summary>
        /// 【方法1】初始化TargetCT行（第1行）和表头行（第2行）
        ///
        /// 此方法设置：
        /// 1. 第1行的目标值
        /// 2. 第2行的动态列名
        /// </summary>
        public void InitializeTargetRow(CTFixedColumnData fixedData, Dictionary<string, string> dynamicData)
        {
            if (dynamicData == null) return;

            // 更新动态列名集合（用于表头绑定）
            _dynamicColumnNames = dynamicData.Keys.ToList();
            RaisePropertyChanged(nameof(DynamicColumnNames));

            // 更新第1行：TargetCT行
            var targetRow = GridRows[0];
            // 设置动态列的目标值
            targetRow.DynamicColumns.Clear();
            foreach (var kvp in dynamicData)
            {
                targetRow.DynamicColumns.Add(new DynamicColumnInfo(kvp.Key, kvp.Value) { RowType = CTRowType.TargetCT });
            }
            // 计算第1行的总CT、净CT、等待时间
            targetRow.CalculateCTValues();

            _isInitialized = true;
        }

        /// <summary>
        /// 更新TargetCT行的目标值（用于预初始化后接收实时数据时更新）
        /// </summary>
        public void UpdateTargetValues(Dictionary<string, string> targetData)
        {
            if (targetData == null) return;

            var targetRow = GridRows[0];
            foreach (var kvp in targetData)
            {
                var existingCol = targetRow.DynamicColumns.FirstOrDefault(c => c.ColumnName == kvp.Key);
                if (existingCol != null)
                {
                    existingCol.Value = kvp.Value;
                }
            }
            targetRow.CalculateCTValues();
            targetRow.RefreshDynamicColumns();
        }

        /// <summary>
        /// 【方法2】添加一条数据行
        ///
        /// 核心逻辑：
        /// 1. 如果数据行未满（<8条），填充到下一个空位
        /// 2. 如果数据行已满（=8条），移除第3行（最早数据），后续数据前移
        ///
        /// FIFO队列实现：
        /// ┌─────────────────────────────────────┐
        /// │ 空位模式: [D1][D2][D3][ ][ ]...   │
        /// │ 满位模式: [D2][D3][D4][D5][D6]... │ ← D1被移除
        /// └─────────────────────────────────────┘
        /// </summary>
        public void AddDataRow(CTFixedColumnData fixedData, Dictionary<string, string> dynamicData)
        {
            if (!_isInitialized)
            {
                // 如果未初始化，先初始化
                InitializeTargetRow(null, dynamicData ?? new Dictionary<string, string>());
            }

            CTGridRowModel targetRow;

            if (_currentDataIndex < 8)
            {
                // ===== 情况1：有空位，直接填充 =====
                targetRow = GridRows[1 + _currentDataIndex];
                _currentDataIndex++;
            }
            else
            {
                // ===== 情况2：已满，FIFO队列 =====
                for (int i = 1; i < 8; i++)
                {
                    GridRows[i].CopyFrom(GridRows[i + 1]);
                }
                // 最后一行（GridRows[8]）作为目标行
                targetRow = GridRows[8];
                // FIFO前移后也需要触发刷新
                ForceRefreshAllRows();
            }

            // 填充数据到目标行
            if (fixedData != null)
            {
                targetRow.Time = fixedData.Time ?? "";
                targetRow.SN = fixedData.SN ?? "";
            }

            if (dynamicData != null)
            {
                targetRow.DynamicColumns.Clear();

                // 获取第一行TargetCT的动态列，用于比较
                var targetCTRRow = GridRows[0]; // 第1行是TargetCT行

                foreach (var kvp in dynamicData)
                {
                    // 查找第一行对应列的目标值
                    var targetColumn = targetCTRRow.DynamicColumns.FirstOrDefault(c => c.ColumnName == kvp.Key);
                    double targetValue = 0;
                    if (targetColumn != null && double.TryParse(targetColumn.Value, out double tv))
                    {
                        targetValue = tv;
                    }

                    // 解析当前值
                    double currentValue = 0;
                    double.TryParse(kvp.Value, out currentValue);

                    // 判断是否超过目标值
                    bool isOverTarget = currentValue > targetValue;

                    targetRow.DynamicColumns.Add(new DynamicColumnInfo(kvp.Key, kvp.Value, isOverTarget) { RowType = CTRowType.Data });
                }
            }

            // 计算第3-5列的值（总CT、净CT、等待时间）
            targetRow.CalculateCTValues();

            // 强制刷新：触发所有行的DynamicColumns属性通知
            ForceRefreshAllRows();
        }

        /// <summary>
        /// 追加数据到当前行（不创建新行）
        /// 用于同一周期内多次收到数据时，合并到同一行
        /// </summary>
        public void AppendToCurrentRow(Dictionary<string, string> dynamicData)
        {
            if (_currentDataIndex <= 0) return;
            if (dynamicData == null || dynamicData.Count == 0) return;

            var currentRow = GridRows[_currentDataIndex];
            var targetCTRRow = GridRows[0];

            foreach (var kvp in dynamicData)
            {
                var existingCol = currentRow.DynamicColumns.FirstOrDefault(c => c.ColumnName == kvp.Key);

                // 获取目标值用于比较
                var targetColumn = targetCTRRow.DynamicColumns.FirstOrDefault(c => c.ColumnName == kvp.Key);
                double targetValue = 0;
                if (targetColumn != null && double.TryParse(targetColumn.Value, out double tv))
                    targetValue = tv;
                double currentValue = 0;
                double.TryParse(kvp.Value, out currentValue);
                bool isOverTarget = currentValue > targetValue;

                if (existingCol != null)
                {
                    // 更新已有列的值
                    existingCol.Value = kvp.Value;
                    existingCol.IsOverTarget = isOverTarget;
                }
                else
                {
                    // 新列，追加
                    currentRow.DynamicColumns.Add(new DynamicColumnInfo(kvp.Key, kvp.Value, isOverTarget) { RowType = CTRowType.Data });
                }
            }

            // 重新计算总CT、净CT、等待时间
            currentRow.CalculateCTValues();
            ForceRefreshAllRows();
        }

        /// <summary>
        /// 强制刷新所有行的显示
        /// </summary>
        private void ForceRefreshAllRows()
        {
            foreach (var row in GridRows)
            {
                row.RefreshDynamicColumns();
            }
        }

        /// <summary>
        /// 清空该Tab页的所有数据
        /// </summary>
        public void ClearData()
        {
            InitializeRows();
            _isInitialized = false;
        }
    }

    /// <summary>
    /// 表格行类型枚举
    /// </summary>
    public enum CTRowType
    {
        TargetCT,  // 目标CT行（第1行，浅绿色）
        Header,    // 表头行（第2行，浅蓝色）
        Data       // 数据行（第3行起，白色）
    }

    /// <summary>
    /// CT表格行模型
    ///
    /// 结构：
    /// ├── 固定列（前5列）：Time, SN, TotalCT, NetCT, WaitTime
    /// ├── 动态列（第6列起）：DynamicColumns (ObservableCollection)
    /// └── 行属性：RowType, RowIndex, BackgroundColor
    ///
    /// 计算规则：
    /// - 第3列（总CT）= 第6列及之后所有列数值的总和
    /// - 第5列（等待时间）= 列名包含"等待"的列数值的总和
    /// - 第4列（净CT）= 第3列 - 第5列
    /// </summary>
    public class CTGridRowModel : BindableBase
    {
        /// <summary>
        /// 计算并更新总CT、净CT、等待时间
        /// </summary>
        public void CalculateCTValues()
        {
            double totalCT = 0;
            double waitTime = 0;

            if (DynamicColumns != null)
            {
                foreach (var col in DynamicColumns)
                {
                    if (double.TryParse(col.Value, out double value))
                    {
                        // 累加总CT
                        totalCT += value;

                        // 如果列名包含"等待"，累加等待时间
                        if (!string.IsNullOrEmpty(col.ColumnName) && col.ColumnName.Contains("等待"))
                        {
                            waitTime += value;
                        }
                    }
                }
            }

            // 更新第3列：总CT
            TotalCT = totalCT.ToString("F3");

            // 更新第5列：等待时间
            WaitTime = waitTime.ToString("F3");

            // 更新第4列：净CT = 总CT - 等待时间
            double netCT = totalCT - waitTime;
            NetCT = netCT.ToString("F3");
        }
        // ===== 固定列（前5列）=====

        /// <summary>列1: 时间 / "TargetCT(s)" / "时间"</summary>
        private string _time;
        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        /// <summary>列2: SN</summary>
        private string _sn;
        public string SN
        {
            get => _sn;
            set => SetProperty(ref _sn, value);
        }

        /// <summary>列3: 总CT</summary>
        private string _totalCT;
        public string TotalCT
        {
            get => _totalCT;
            set => SetProperty(ref _totalCT, value);
        }

        /// <summary>列4: 净CT</summary>
        private string _netCT;
        public string NetCT
        {
            get => _netCT;
            set => SetProperty(ref _netCT, value);
        }

        /// <summary>列5: 等待时间</summary>
        private string _waitTime;
        public string WaitTime
        {
            get => _waitTime;
            set => SetProperty(ref _waitTime, value);
        }

        // ===== 动态列（第6列起）=====

        private ObservableCollection<DynamicColumnInfo> _dynamicColumns = new ObservableCollection<DynamicColumnInfo>();

        /// <summary>
        /// 动态列数据集合
        /// </summary>
        public ObservableCollection<DynamicColumnInfo> DynamicColumns
        {
            get => _dynamicColumns;
            set
            {
                _dynamicColumns = value;
                RaisePropertyChanged(nameof(DynamicColumns));
                RaisePropertyChanged(nameof(HasDynamicColumns));
            }
        }

        /// <summary>
        /// 索引器，用于DataGrid绑定动态列
        /// 使用方式：绑定路径如 "DynamicValue[动作1]"
        /// </summary>
        public string this[string columnName]
        {
            get
            {
                var col = _dynamicColumns.FirstOrDefault(c => c.ColumnName == columnName);
                return col?.Value ?? "";
            }
        }

        /// <summary>
        /// 是否有动态列数据
        /// </summary>
        public bool HasDynamicColumns => _dynamicColumns != null && _dynamicColumns.Count > 0;

        // ===== 行属性 =====

        /// <summary>行类型</summary>
        public CTRowType RowType { get; set; }

        /// <summary>行高度（用于表头行动态计算高度）</summary>
        private double _rowHeight = 35;
        public double RowHeight
        {
            get => _rowHeight;
            set => SetProperty(ref _rowHeight, value);
        }

        /// <summary>行索引（从1开始）</summary>
        public int RowIndex { get; set; }

        /// <summary>背景颜色</summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// 从另一行复制数据
        /// </summary>
        public void CopyFrom(CTGridRowModel sourceRow)
        {
            Time = sourceRow.Time;
            SN = sourceRow.SN;
            TotalCT = sourceRow.TotalCT;
            NetCT = sourceRow.NetCT;
            WaitTime = sourceRow.WaitTime;

            if (sourceRow.DynamicColumns != null)
            {
                // 清空并复制，而不是创建新集合
                DynamicColumns.Clear();
                foreach (var col in sourceRow.DynamicColumns)
                {
                    // 复制所有属性，包括IsOverTarget和RowType
                    DynamicColumns.Add(new DynamicColumnInfo(col.ColumnName, col.Value, col.IsOverTarget)
                    {
                        RowType = col.RowType
                    });
                }
            }
        }

        /// <summary>
        /// 获取动态列的值
        /// </summary>
        public string GetDynamicValue(string columnName)
        {
            return _dynamicColumns?.FirstOrDefault(d => d.ColumnName == columnName)?.Value ?? string.Empty;
        }

        /// <summary>
        /// 强制刷新动态列的显示（用于外部调用触发UI更新）
        /// </summary>
        public void RefreshDynamicColumns()
        {
            RaisePropertyChanged(nameof(DynamicColumns));
        }
    }
}
