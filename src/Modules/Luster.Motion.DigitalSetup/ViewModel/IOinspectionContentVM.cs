using HandyControl.Controls;
using HandyControl.Data;
using Luster.Common.Assets;
using Luster.Common.Assets.FloatingInfo.Services;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Views.Dialogs;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.DigitalSetup.Views;
using Luster.Motion.EditorUI;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using static FreeSql.Internal.GlobalFilter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// IO点检
    /// </summary>
    public class IOinspectionContentVM : BaseAss
    {
        // 新增3个按钮和1个进度条的定义
        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }
        public ICommand DeleteRowCommand { get; private set; }

        /// <summary>
        /// 批量导入IO点检图片命令
        /// </summary>
        public ICommand BatchImportImagesCommand { get; private set; }

        private Dispatcher _dispatcher;
        private const string PageName = "IOConform";

        /// <summary>
        /// 进度条
        /// </summary>
        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        /// <summary>
        /// 是否正在点检中
        /// </summary>
        private bool _isChecking;
        public bool IsChecking
        {
            get { return _isChecking; }
            set
            {
                if (SetProperty(ref _isChecking, value))
                {
                    // 当IsChecking状态改变时，通知DeleteRowCommand重新评估可执行状态
                    (DeleteRowCommand as DelegateCommand<object>)?.RaiseCanExecuteChanged();
                }
            }
        }

        readonly IDeviceEngine _deviceEngine;
        protected IDialogService _dialogService;
        private bool _checkResult = false;
        private string _checkErrorMessage = string.Empty;

        /// <summary>
        /// 用于控制同时只能弹出一个IO交替检测对话框
        /// </summary>
        private static readonly object _dialogLock = new object();
        private static IOAlternatingCheckDialog _currentDialog = null;

        /// <summary>
        /// 用于控制同时只能弹出一个IO输出检测对话框
        /// </summary>
        private static IOCheckDialog _currentOutputDialog = null;

        private readonly IFloatingInfoConfigService _configService;

        public IOinspectionContentVM(ISimDeviceEngineUI engineUI, IDeviceEngine deviceEngine, ICommonBus commonBus,
               IRepository repository, IRegionManager regionManager,
               IDialogService dialogService, ISimDeviceEngineUI simDeviceEngineUI, CSVHelper cSVHelper, Dispatcher dispatcher, FlowBus flowBus,
               IFloatingInfoConfigService configService, IFloatingInfoService floatingInfoService, CheckStatusService checkStatusService) :
               base(repository, regionManager, commonBus, cSVHelper, flowBus, dialogService, checkStatusService)
        {
            _configService = configService;
            _floatingInfoService = floatingInfoService;

            // 设置父页面Region名称，用于构建子页面浮动信息窗口的PageId和点检状态保存
            _parentRegionName = "IOinspectionContent";

            Pages = new ObservableCollection<CommonPageModel>();
            //Pages.Add(new CommonPageModel() { Name = "Vacuum", IsSelected = true, Region = "", ViewType = typeof(AssTbVacuum) });
            //Pages.Add(new CommonPageModel() { Name = "OriginLimit", IsSelected = false, Region = "", ViewType = typeof(AssTbOriginLimit) });
            //Pages.Add(new CommonPageModel() { Name = "Runners", IsSelected = false, Region = "", ViewType = typeof(AssTbRunners) });
            Pages.Add(new CommonPageModel() { Name = "Digital_In_Single", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalInSingle) });
            Pages.Add(new CommonPageModel() { Name = "Digital_Out_Single", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalOutSingle) });
            //Pages.Add(new CommonPageModel() { Name = "Digital_In", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalIn) });
            //Pages.Add(new CommonPageModel() { Name = "Digital_Out", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalOut) });
            Pages.Add(new CommonPageModel() { Name = "Cylinder", IsSelected = false, Region = "", ViewType = typeof(AssTbCylinder) });

            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("IOinspectionContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            _deviceEngine = deviceEngine;
            _dialogService = engineUI.Dialog;
            _dispatcher = dispatcher;
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);
            DeleteRowCommand = new DelegateCommand<object>(OnDeleteRow, CanDeleteRow);
            BatchImportImagesCommand = new DelegateCommand(OnBatchImportImages);
            LoadCheckConfirmMessages();

            // 延迟加载点检状态，确保 UI 绑定已建立
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private bool CanDeleteRow(object arg)
        {
            return !IsChecking;
        }


        public override async void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }

        /// <summary>
        /// 获取页面整体状态
        /// </summary>
        private string GetOverallStatus()
        {
            if (ItemModels == null || ItemModels.Count == 0)
                return "未点检";

            bool hasNG = false;
            bool hasOK = false;
            bool hasNotChecked = false;

            foreach (var item in ItemModels)
            {
                string status = "";

                // 根据类型获取状态
                if (item is AssTbCylinder cylinder)
                    status = cylinder.状态;
                else if (item is AssTbDigitalIn digitalIn)
                    status = digitalIn.状态;
                else if (item is AssTbDigitalOut digitalOut)
                    status = digitalOut.状态;
                else if (item is AssTbDigitalInSingle digitalInSingle)
                    status = digitalInSingle.状态;
                else if (item is AssTbDigitalOutSingle digitalOutSingle)
                    status = digitalOutSingle.状态;
                else if (item is AssTbOriginLimit originLimit)
                    status = originLimit.状态;

                // 跳过状态忽略不计
                if (status == "跳过")
                    continue;

                // NG 优先级最高
                if (status == "NG" || status == "超时")
                {
                    hasNG = true;
                    break;  // 只要有一个 NG，整体就是 NG，直接退出
                }

                if (status == "OK")
                {
                    hasOK = true;
                }
                else if (status == "" || status == "未点检" || status == "未完成")
                {
                    hasNotChecked = true;
                }
            }

            // 有 NG 直接返回 NG
            if (hasNG)
                return "NG";

            // 有未点检，返回未点检
            if (hasNotChecked)
                return "未点检";

            // 有 OK 返回 OK
            if (hasOK)
                return "OK";

            // 默认返回未点检
            return "未点检";
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
                        // 先设置 ParentRegion，这样 PageKey 才会返回正确的值
                        page.ParentRegion = "IOinspectionContent";

                        // 现在 page.PageKey 会返回正确的值: "IOinspectionContent_XXX"
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

        public override async void OnOneKeyCheck(object obj)
        {
            await base.OnOneKeyCheckAsync(obj);
            // 子界面的一键点检逻辑
            try
            {
                // 先验证项次与仿真设备对应关系
                var mismatched = ValidateItemsWithSimDevices();
                if (mismatched.Count > 0)
                {
                    var message = $"以下项次在仿真设备中未找到对应：\n{string.Join("\n", mismatched.Take(10))}" +
                                  (mismatched.Count > 10 ? $"\n...共{mismatched.Count}项" : "") +
                                  "\n\n是否仍要继续点检？（未找到的项将标记为'设备未找到'）";
                    var result = await ShowConfirmAsync(message);
                    if (result != ButtonResult.OK)
                    {
                        return;
                    }
                }
                StartAsync();
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"{SelectedReportPage.Name}:失败" });
            }
            finally { }
        }

        /// <summary>
        /// 验证界面数据项与仿真设备的对应关系
        /// </summary>
        /// <returns>返回不匹配的项次列表</returns>
        private List<string> ValidateItemsWithSimDevices()
        {
            var mismatched = new List<string>();

            if (SelectedReportPage.Name == "Cylinder")
            {
                var cylinders = _deviceEngine.GetDevices(typeof(VCylinder));
                foreach (var item in ItemModels.OfType<AssTbCylinder>())
                {
                    if (string.IsNullOrEmpty(item.项次)) continue;
                    var parts = item.项次.Split('/');
                    var deviceName = parts[0];
                    var exists = cylinders.Any(c => c.Name == deviceName);
                    //var isValid = !item.项次.Contains("备用") && !item.项次.Contains("弃用");
                    if (!exists /*&& isValid*/)
                    {
                        mismatched.Add(item.项次);
                    }
                }
            }
            else if (SelectedReportPage.Name == "Digital_In")
            {
                var vios = _deviceEngine.GetDevices(typeof(VIO));
                foreach (var item in ItemModels.OfType<AssTbDigitalIn>())
                {
                    if (string.IsNullOrEmpty(item.项次)) continue;
                    var exists = vios.Any(v => v.Name == item.项次);
                    //var isValid = !item.项次.Contains("备用") && !item.项次.Contains("弃用");
                    if (!exists /*&& isValid*/)
                    {
                        mismatched.Add(item.项次);
                    }
                }
            }
            else if (SelectedReportPage.Name == "Digital_Out")
            {
                var vios = _deviceEngine.GetDevices(typeof(VIO));
                foreach (var item in ItemModels.OfType<AssTbDigitalOut>())
                {
                    if (string.IsNullOrEmpty(item.项次)) continue;
                    var exists = vios.Any(v => v.Name == item.项次);
                    //var isValid = !item.项次.Contains("备用") && !item.项次.Contains("弃用");
                    if (!exists /*&& isValid*/)
                    {
                        mismatched.Add(item.项次);
                    }
                }
            }
            else if (SelectedReportPage.Name == "Digital_In_Single")
            {
                var vios = _deviceEngine.GetDevices(typeof(VIO));
                foreach (var item in ItemModels.OfType<AssTbDigitalInSingle>())
                {
                    if (string.IsNullOrEmpty(item.项次)) continue;
                    var exists = vios.Any(v => v.Name == item.项次);
                    //var isValid = !item.项次.Contains("备用") && !item.项次.Contains("弃用");
                    if (!exists /*&& isValid*/)
                    {
                        mismatched.Add(item.项次);
                    }
                }
            }

            return mismatched;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            IsChecking = true;
            bool wasCancelled = false;

            try
            {
                switch (SelectedReportPage.Name)
                {
                    case "Vacuum":
                        Check_Vacuum();
                        break;
                    case "Cylinder":
                        await Check_Cylinder(token);
                        break;
                    case "OriginLimit":
                        await Check_OriginLimit(token);
                        break;
                    //case "Runners":
                    //    Check_Runners();
                    //    break;
                    case "Digital_In":
                        await Check_IO(token);
                        break;
                    case "Digital_Out":
                        await Check_IO(token);
                        break;
                    case "Digital_In_Single":
                        await Check_IO_Single(token);
                        break;
                    case "Digital_Out_Single":
                        await Check_IO_Single(token);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
                throw; // 重新抛出，让外层处理
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                IsChecking = false;

                // 保存当前子页面的点检状态
                var currentOverallStatus = GetOverallStatus();
                var checkStatus = CheckStatus.NotChecked;
                string remark = "";

                if (wasCancelled && token.IsCancellationRequested)
                {
                    // 用户中止
                    bool canContinue = CanContinueFromLastCheck();

                    if (canContinue)
                    {
                        checkStatus = CheckStatus.NotChecked;
                        remark = "执行中止，可从上次继续";
                    }
                    else
                    {
                        checkStatus = CheckStatus.CheckedFail;
                        remark = "执行中止，需从头开始";
                    }
                }
                else if (currentOverallStatus == "OK")
                {
                    checkStatus = CheckStatus.CheckedOK;
                    remark = "全部点检项合格";
                }
                else if (currentOverallStatus == "NG")
                {
                    checkStatus = CheckStatus.CheckedFail;
                    remark = "发现点检不合格项";
                }
                else
                {
                    checkStatus = CheckStatus.NotChecked;
                    remark = "未完成点检";
                }

                SaveCheckStatus(checkStatus, remark);

                // 同步一级界面整体状态到 PageStatusService
                SyncOverallStatusToPageStatusService();
            }
        }
        private async Task Check_Cylinder(CancellationToken token)
        {
            try
            {
                // 循环前只弹一次确认
                var confirmResult = await ShowConfirmAsync("确定要依次执行所有气缸操作吗？");
                if (confirmResult != ButtonResult.OK)
                {
                    // 用户取消
                    foreach (var item in ItemModels)
                    {
                        if (item is AssTbCylinder CylinderTb)
                        {
                            CylinderTb.实测 = "";
                            CylinderTb.状态 = "未完成";
                        }
                    }
                    return;
                }
                foreach (var item in ItemModels.OfType<AssTbCylinder>())
                {
                    item.实测 = "";
                    item.状态 = "";
                }

                ProgressValue = 0; // 进度
                // 遍历所有气缸模型并执行操作
                for (int i = 0; i < ItemModels.Count; i++)
                {
                    if (ItemModels[i] is AssTbCylinder CylinderTb)
                    {
                        if (string.IsNullOrWhiteSpace(CylinderTb.标准))
                        {
                            CylinderTb.状态 = "格式错误";
                            continue;
                        }
                        var devices = _deviceEngine.GetDevices(typeof(VCylinder));
                        if (!string.IsNullOrEmpty(CylinderTb.项次))
                        {
                            List<string> result = CylinderTb.项次.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            var vCylinder = devices.FirstOrDefault(x => x.Name == result[0]) as VCylinder;
                            if (vCylinder == null)
                            {
                                CylinderTb.状态 = "设备未找到";
                                continue;
                            }

                            if (result.Count > 1)
                            {
                                // 直接调用交替检测方法
                                await CheckCylinderAlternatingAsync(CylinderTb, vCylinder, CylinderTb.标准, token);
                            }
                            else
                            {
                                CylinderTb.实测 = "0";
                                CylinderTb.状态 = "格式错误";
                            }
                        }
                        if (!token.IsCancellationRequested)//取消任务，不刷新进度，避免切换到其他Page页进度条会自动变
                        {
                            ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                        }
                    }
                    await Task.Delay(500);
                }
                //弹出提示框， 所有气缸操作已完成，请检查结果

                //throw new FriendlyException("所有气缸操作已完成，请检查结果。");
            }
            catch (Exception ex)
            {
                //throw new FriendlyException($"气缸操作失败: {ex.Message}");
            }
        }

        // 不再弹窗的气缸动作方法
        private async Task CheckCylinderActionAsync(AssTbCylinder CylinderTb, VCylinder vCylinder, string action, string 标准, CancellationToken token)
        {
            await Task.Run(async () =>
            {
                // 获取当前状态和目标位置
                int currentPos = (int)vCylinder.GetCurrentPos();  // 10=伸出, 0=缩回
                int targetPos = action == "伸出" ? 10 : 0;
                var timeout = TimeSpan.FromSeconds(5);

                try
                {
                    // 如果气缸已经在目标状态，先执行反向动作到位（不计时）
                    if (currentPos == targetPos)
                    {
                        LogStatus($"气缸 [{vCylinder.Name}] 已在{action}状态，先执行反向动作做准备");

                        int reversePos = targetPos == 10 ? 0 : 10;

                        // 执行反向动作
                        if (reversePos == 10)
                            vCylinder.Extend();
                        else
                            vCylinder.Retract();

                        // 等待反向动作完成（不计时）
                        await WaitForPosition(vCylinder, reversePos, timeout, token);
                    }

                    // 现在开始执行目标动作并计时
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    if (action == "伸出")
                        vCylinder.Extend();
                    else
                        vCylinder.Retract();

                    // 等待目标动作完成
                    await WaitForPosition(vCylinder, targetPos, timeout, token);

                    stopwatch.Stop();

                    // 更新UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var TIME = stopwatch.ElapsedMilliseconds.ToString();
                        CylinderTb.实测 = TIME;
                        if (string.IsNullOrEmpty(CylinderTb.实测))
                        {
                            CylinderTb.状态 = "未完成";
                        }
                        else if (CylinderTb.标准 == CylinderTb.实测)
                        {
                            CylinderTb.状态 = "OK";
                        }
                        else if (CylinderTb.标准.Contains('~'))
                        {
                            var range = ParseColumnRange(CylinderTb.标准);
                            double.TryParse(CylinderTb.实测, out double 实测浮点值);
                            if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                            {
                                CylinderTb.状态 = "OK";
                            }
                            else
                            {
                                CylinderTb.状态 = "NG";
                            }
                        }
                        else if (double.TryParse(CylinderTb.标准, out double std) && double.TryParse(CylinderTb.实测, out double act))
                        {
                            CylinderTb.状态 = std > act ? "OK" : "NG";
                        }
                        else
                        {
                            CylinderTb.状态 = "NG";
                        }
                    });
                }
                catch (TimeoutException ex)
                {
                    // 超时异常：更新UI为超时状态
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CylinderTb.实测 = "";
                        CylinderTb.状态 = "超时";
                        LogStatus($"气缸 [{vCylinder.Name}] {action}动作超时: {ex.Message}");
                    });
                }
                catch (OperationCanceledException)
                {
                    // 取消操作：更新UI为未完成状态
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CylinderTb.实测 = "";
                        CylinderTb.状态 = "已取消";
                    });
                }
                catch (Exception ex)
                {
                    // 其他异常：更新UI为错误状态
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CylinderTb.实测 = "";
                        CylinderTb.状态 = "错误";
                        LogStatus($"气缸 [{vCylinder.Name}] {action}动作异常: {ex.Message}");
                    });
                }
            }, token);
        }

        /// <summary>
        /// 气缸交替检测方法
        /// 执行"取反（计时）→还原（计时）"，实测值 = 两个动作的总耗时
        /// </summary>
        /// <param name="CylinderTb">气缸数据表项</param>
        /// <param name="vCylinder">气缸设备</param>
        /// <param name="标准">标准值（范围或单值）</param>
        /// <param name="token">取消令牌</param>
        private async Task CheckCylinderAlternatingAsync(AssTbCylinder CylinderTb, VCylinder vCylinder, string 标准, CancellationToken token)
        {
            await Task.Run(async () =>
            {
                var timeout = TimeSpan.FromSeconds(5);

                try
                {
                    // 1. 获取当前状态
                    int currentPos = (int)vCylinder.GetCurrentPos();  // 10=伸出, 0=缩回

                    // 2. 确定取反位置
                    int reversePos = currentPos == 10 ? 0 : 10;

                    // 3. 确定还原位置
                    int restorePos = currentPos;

                    LogStatus($"气缸 [{vCylinder.Name}] 开始交替检测，当前状态: {currentPos}，取反目标: {reversePos}，还原目标: {restorePos}");

                    // 4. 执行取反动作并计时
                    var reverseStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    if (reversePos == 10)
                        vCylinder.Extend();
                    else
                        vCylinder.Retract();

                    await WaitForPosition(vCylinder, reversePos, timeout, token);

                    reverseStopwatch.Stop();
                    long reverseTime = reverseStopwatch.ElapsedMilliseconds;

                    LogStatus($"气缸 [{vCylinder.Name}] 取反动作完成，耗时: {reverseTime}ms");

                    // 5. 执行还原动作并计时
                    var restoreStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    if (restorePos == 10)
                        vCylinder.Extend();
                    else
                        vCylinder.Retract();

                    await WaitForPosition(vCylinder, restorePos, timeout, token);

                    restoreStopwatch.Stop();
                    long restoreTime = restoreStopwatch.ElapsedMilliseconds;

                    LogStatus($"气缸 [{vCylinder.Name}] 还原动作完成，耗时: {restoreTime}ms");

                    // 6. 计算总耗时
                    long totalTime = reverseTime + restoreTime;

                    // 7. 更新UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CylinderTb.实测 = totalTime.ToString();

                        // 8. 根据 标准 判定状态
                        if (string.IsNullOrEmpty(CylinderTb.实测))
                        {
                            CylinderTb.状态 = "未完成";
                        }
                        else if (CylinderTb.标准 == CylinderTb.实测)
                        {
                            CylinderTb.状态 = "OK";
                        }
                        else if (CylinderTb.标准.Contains('~'))
                        {
                            var range = ParseColumnRange(CylinderTb.标准);
                            double.TryParse(CylinderTb.实测, out double 实测浮点值);
                            if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                            {
                                CylinderTb.状态 = "OK";
                            }
                            else
                            {
                                CylinderTb.状态 = "NG";
                            }
                        }
                        else if (double.TryParse(CylinderTb.标准, out double std) && double.TryParse(CylinderTb.实测, out double act))
                        {
                            CylinderTb.状态 = std > act ? "OK" : "NG";
                        }
                        else
                        {
                            CylinderTb.状态 = "NG";
                        }
                    });
                }
                catch (TimeoutException ex)
                {
                    // 超时异常：更新UI为超时状态
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CylinderTb.实测 = "";
                        CylinderTb.状态 = "超时";
                        LogStatus($"气缸 [{vCylinder.Name}] 交替检测超时: {ex.Message}");
                    });
                }
                catch (OperationCanceledException)
                {
                    // 取消操作：更新UI为已取消状态
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CylinderTb.实测 = "";
                        CylinderTb.状态 = "已取消";
                    });
                }
                catch (Exception ex)
                {
                    // 其他异常：更新UI为错误状态
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CylinderTb.实测 = "";
                        CylinderTb.状态 = "错误";
                        LogStatus($"气缸 [{vCylinder.Name}] 交替检测异常: {ex.Message}");
                    });
                }
            }, token);
        }

        /// <summary>
        /// 等待气缸到达目标位置
        /// </summary>
        /// <param name="cylinder">气缸设备</param>
        /// <param name="targetPos">目标位置（10=伸出, 0=缩回）</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令牌</param>
        private async Task WaitForPosition(VCylinder cylinder, int targetPos, TimeSpan timeout, CancellationToken token)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (cylinder.GetCurrentPos() != targetPos)
            {
                if (sw.Elapsed > timeout)
                {
                    throw new TimeoutException($"气缸 {cylinder.Name} 动作超时，未能在 {timeout.TotalSeconds} 秒内到达目标位置 {targetPos}");
                }
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await Task.Delay(10, token);
            }
        }

        private Task<ButtonResult> ShowConfirmAsync(string message)
        {
            var tcs = new TaskCompletionSource<ButtonResult>();
            this._dispatcher.BeginInvoke(new Action(() =>
            {
                _dialogService.ShowConfirm(message, r => tcs.SetResult(r.Result), false);
            }));
            return tcs.Task;
        }

        private void Check_Vacuum()
        {
            foreach (var item in ItemModels.OfType<AssTbVacuum>())
            {
                item.实测 = "";
                item.状态 = "";
            }
            // 这里可以添加真空检查的逻辑
            ProgressValue = 0; // 进度
            for (int i = 0; i < ItemModels.Count; i++)
            {
                if (ItemModels[i] is AssTbVacuum vacuum)
                {
                    // 检查软件版本是否为空
                    if (string.IsNullOrWhiteSpace(vacuum.标准))
                    {
                        vacuum.状态 = "格式错误";
                        continue;
                    }
                    if ("" == vacuum.实测)
                    {
                        vacuum.状态 = "未完成";
                    }
                    else if (vacuum.标准 == vacuum.实测)
                    {
                        vacuum.状态 = "OK";
                    }
                    else
                    {
                        vacuum.状态 = "NG";
                    }
                    ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                }
            }
        }

        private void Check_Runners()
        {
            // 这里可以添加真空检查的逻辑
            ProgressValue = 0; // 进度
            for (int i = 0; i < ItemModels.Count; i++)
            {
                if (ItemModels[i] is AssTbRunners runners)
                {
                    // 检查软件版本是否为空
                    if (string.IsNullOrWhiteSpace(runners.标准))
                    {
                        runners.状态 = "格式错误";
                        continue;
                    }
                    if ("" == runners.实测)
                    {
                        runners.状态 = "未完成";
                    }
                    else if (runners.标准 == runners.实测)
                    {
                        runners.状态 = "OK";
                    }
                    else
                    {
                        runners.状态 = "NG";
                    }
                    ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                }
            }
        }

        /// <summary>
        /// 更新I/O列表 - 从CSV文件读取数据
        /// </summary>
        private void OnUpdateItems()
        {
            // 使用 CSVHelper 直接根据类型读取对应的CSV文件
            try
            {
                ItemModels.Clear();
                long totalCount = 0;
                IEnumerable<object> items = null;

                switch (SelectedReportPage.Name)
                {
                    case "Cylinder":
                        items = _csvHelper.GetAllDataNew1<AssTbCylinder>(0, 0, out totalCount).Cast<object>();
                        break;
                    case "Digital_In":
                        items = _csvHelper.GetAllDataNew1<AssTbDigitalIn>(0, 0, out totalCount).Cast<object>();
                        break;
                    case "Digital_Out":
                        items = _csvHelper.GetAllDataNew1<AssTbDigitalOut>(0, 0, out totalCount).Cast<object>();
                        break;
                    case "Digital_In_Single":
                        items = _csvHelper.GetAllDataNew1<AssTbDigitalInSingle>(0, 0, out totalCount).Cast<object>();
                        break;
                    case "Digital_Out_Single":
                        items = _csvHelper.GetAllDataNew1<AssTbDigitalOutSingle>(0, 0, out totalCount).Cast<object>();
                        break;
                }

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        // 清空 实测 和 状态
                        var type = item.GetType();
                        var propMeasured = type.GetProperty("实测");
                        if (propMeasured != null && propMeasured.CanWrite)
                        {
                            propMeasured.SetValue(item, "");
                        }

                        var propStatus = type.GetProperty("状态");
                        if (propStatus != null && propStatus.CanWrite)
                        {
                            propStatus.SetValue(item, "");
                        }

                        var IOName = type.GetProperty("项次").GetValue(item);
                        if (IOName != null)
                        {
                            if (IOName.ToString().Contains("备用") || IOName.ToString().Contains("弃用"))
                            {
                                continue;
                            }
                        }
                        ItemModels.Add(item);
                    }
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"从CSV文件成功读取 {ItemModels.Count} 条数据 ({SelectedReportPage.Name})" });
                    PageStatusService.Instance.UpdateStatus(PageName, "未点检");
                    if (ItemModels.Count > 1)
                    {
                        return; // CSV读取成功，直接返回
                    }


                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Warning, LogMessage = $"从CSV读取失败: {ex.Message}，将从设备引擎获取数据" });
            }

            // CSV读取失败或无数据时，从设备引擎获取数据（备用逻辑）
            ObservableCollection<object> tempCollection = new ObservableCollection<object>();
            if (SelectedReportPage.Name == "Vacuum")
            {
                var Vacuums = _deviceEngine.GetDevices(typeof(VVacuum));
                for (int i = 0; i < Vacuums.Count; i++)
                {
                    var vacuum = Vacuums[i] as VVacuum;
                    if (vacuum != null)
                    {
                        AssTbVacuum item = new AssTbVacuum()
                        {
                            项序 = i,
                            项次 = vacuum.Name,
                            标准 = "未具备模拟量读数", // 默认标准值为0
                            实测 = "未具备模拟量读数", // 默认实测值为0
                            完成时间 = DateTime.Now
                        };
                        tempCollection.Add(item);
                    }
                }

            }
            if (SelectedReportPage.Name == "Cylinder")
            {
                var Cylinders = _deviceEngine.GetDevices(typeof(VCylinder));
                for (int i = 0; i < Cylinders.Count; i++)
                {
                    var cylinder = Cylinders[i] as VCylinder;
                    if (cylinder != null)
                    {
                        AssTbCylinder item = new AssTbCylinder()
                        {
                            项序 = i,
                            项次 = cylinder.Name + "/" + "交替检",
                            标准 = "200~400", // 两个动作的总和范围
                            实测 = "", // 默认实测值为0
                            完成时间 = DateTime.Now
                        };
                        tempCollection.Add(item);
                    }
                }

            }
            if (SelectedReportPage.Name == "OriginLimit")
            {
                var OriginLimitS = _deviceEngine.GetDevices(typeof(VAxis));
                var reordered = OriginLimitS.OrderByDescending(x => x.Name.Contains("U") || x.Name.Contains("Z")).ToList();
                for (int i = 0; i < reordered.Count; i++)
                {
                    var axis = reordered[i] as VAxis;
                    if (axis != null)
                    {
                        AssTbOriginLimit item = new AssTbOriginLimit()
                        {
                            项序 = i,
                            项次 = axis.Name,
                            标准 = "0", // 默认标准值为0
                            实测 = "", // 默认实测值为0
                            完成时间 = DateTime.Now
                        };
                        ItemModels.Add(item);
                    }
                }
            }
            if (SelectedReportPage.Name == "Digital_In")
            {
                var ioList = GetIOList("", IOBehavior.Input, IOType.Digital).ToList();
                for (int i = 0; i < ioList.Count; i++)
                {
                    AssTbDigitalIn item;
                    //名称中存在: 下限、上限、伸位、到位、破真空  则标准置为1;否则为0
                    if (ioList[i].Name.Contains("下限") ||
                        ioList[i].Name.Contains("上限") ||
                        ioList[i].Name.Contains("伸位") ||
                        ioList[i].Name.Contains("到位") ||
                        ioList[i].Name.Contains("破真空"))
                    {
                        item = new AssTbDigitalIn()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "True",
                            完成时间 = DateTime.Now
                        };
                    }
                    else
                    {
                        item = new AssTbDigitalIn()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "False",
                            完成时间 = DateTime.Now
                        };
                    }
                    if (item != null)
                    {
                        tempCollection.Add(item);
                    }
                }
            }
            if (SelectedReportPage.Name == "Digital_Out")
            {
                var ioList = GetIOList("", IOBehavior.Output, IOType.Digital).ToList();
                for (int i = 0; i < ioList.Count; i++)
                {
                    AssTbDigitalOut item;
                    //名称中存在: 下限、上限、伸位、到位、破真空  则标准置为1;否则为0
                    if (ioList[i].Name.Contains("下限") ||
                        ioList[i].Name.Contains("上限") ||
                        ioList[i].Name.Contains("伸位") ||
                        ioList[i].Name.Contains("到位") ||
                        ioList[i].Name.Contains("破真空"))
                    {
                        item = new AssTbDigitalOut()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "True",
                            完成时间 = DateTime.Now
                        };
                    }
                    else
                    {
                        item = new AssTbDigitalOut()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "False",
                            完成时间 = DateTime.Now
                        };
                    }
                    if (item != null)
                    {
                        tempCollection.Add(item);
                    }
                }
            }
            if (SelectedReportPage.Name == "Digital_In_Single")
            {
                var ioList = GetIOList("", IOBehavior.Input, IOType.Digital).ToList();
                for (int i = 0; i < ioList.Count; i++)
                {
                    AssTbDigitalInSingle item;
                    //名称中存在: 下限、上限、伸位、到位、破真空  则标准置为1;否则为0
                    if (ioList[i].Name.Contains("下限") ||
                        ioList[i].Name.Contains("上限") ||
                        ioList[i].Name.Contains("伸位") ||
                        ioList[i].Name.Contains("到位") ||
                        ioList[i].Name.Contains("破真空"))
                    {
                        item = new AssTbDigitalInSingle()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "True",
                            完成时间 = DateTime.Now
                        };
                    }
                    else
                    {
                        item = new AssTbDigitalInSingle()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "False",
                            完成时间 = DateTime.Now
                        };
                    }
                    if (item != null)
                    {
                        tempCollection.Add(item);
                    }
                }
            }
            if (SelectedReportPage.Name == "Digital_Out_Single")
            {
                var ioList = GetIOList("", IOBehavior.Output, IOType.Digital).ToList();
                for (int i = 0; i < ioList.Count; i++)
                {
                    AssTbDigitalOutSingle item;
                    //名称中存在: 下限、上限、伸位、到位、破真空  则标准置为1;否则为0
                    if (ioList[i].Name.Contains("下限") ||
                        ioList[i].Name.Contains("上限") ||
                        ioList[i].Name.Contains("伸位") ||
                        ioList[i].Name.Contains("到位") ||
                        ioList[i].Name.Contains("破真空"))
                    {
                        item = new AssTbDigitalOutSingle()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "True",
                            完成时间 = DateTime.Now
                        };
                    }
                    else
                    {
                        item = new AssTbDigitalOutSingle()
                        {
                            项序 = i,
                            项次 = ioList[i].Name,
                            标准 = "False",
                            完成时间 = DateTime.Now
                        };
                    }
                    if (item != null)
                    {
                        tempCollection.Add(item);
                    }
                }
            }


            foreach (var tempItem in tempCollection)
            {
                // 通过反射获取项次属性
                var tempItemType = tempItem.GetType();
                var tempItemProp_XC = tempItemType.GetProperty("项次");
                var tempItemProp_BZ = tempItemType.GetProperty("标准");
                if (tempItemProp_XC == null || tempItemProp_BZ == null) continue;
                var tempItemValue = tempItemProp_XC.GetValue(tempItem) as string;

                // 在ItemModels中查找项次相同的item
                var existItem = ItemModels.FirstOrDefault(x =>
                {
                    var existType = x.GetType();
                    var existProp = existType.GetProperty("项次");
                    if (existProp == null) return false;
                    var existValue = existProp.GetValue(x) as string;
                    return existValue == tempItemValue;
                });

                // 如果找到，则将tempItem.项次赋值为ItemModels中的值
                if (existItem != null)
                {
                    var existType = existItem.GetType();
                    var existProp = existType.GetProperty("标准");
                    if (existProp != null)
                    {
                        var existValue = existProp.GetValue(existItem);
                        tempItemProp_BZ.SetValue(tempItem, existValue);
                    }
                }
            }

            ItemModels.Clear();
            foreach (var item in tempCollection)
            {
                var IOName = item.GetType().GetProperty("项次").GetValue(item);
                if (IOName.ToString().Contains("备用") || IOName.ToString().Contains("弃用"))
                {
                    continue;
                }
                ItemModels.Add(item);
            }
        }

        /// 删除指定行
        /// </summary>
        private async void OnDeleteRow(object obj)
        {
            if (obj == null) return;

            var itemToDelete = obj;

            // 获取项次信息用于确认提示
            var itemType = itemToDelete.GetType();
            var propXc = itemType.GetProperty("项次");
            var itemName = propXc != null ? propXc.GetValue(itemToDelete)?.ToString() : "未知项";

            // 弹出确认对话框
            var confirmResult = await ShowConfirmAsync($"确定要删除项次 [{itemName}] 吗？\n此操作不可撤销。");
            if (confirmResult != ButtonResult.OK)
            {
                return;
            }

            // 从集合中移除
            if (ItemModels.Contains(itemToDelete))
            {
                ItemModels.Remove(itemToDelete);
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"已删除项次: {itemName}" });
            }
        }

        /// <summary>
        /// 一键点检I/O
        /// </summary>
        private async Task Check_IO(CancellationToken token = default)
        {
            string errMessage = string.Empty;

            try
            {
                ProgressValue = 0; // 进度

                //复位所有气缸
                //弹窗确认
                var confirmResult = await ShowConfirmAsync("确定要执行IO检测操作吗？");
                if (confirmResult != ButtonResult.OK)
                {
                    // 用户取消
                    foreach (var item in ItemModels)
                    {
                        if (item is AssTbDigitalIn inIo)
                        {
                            inIo.实测 = "";
                            inIo.状态 = "未完成";
                        }

                        else if (item is AssTbDigitalOut outIo)
                        {
                            outIo.实测 = "";
                            outIo.状态 = "未完成";
                        }

                        //if (item is AssTbCylinder CylinderTb)
                        //{
                        //    CylinderTb.实测 = "0ms";
                        //    CylinderTb.状态 = "未完成";
                        //}
                    }
                    return;
                }
                //else
                //{
                //    var Cylinders = _deviceEngine.GetDevices(typeof(VCylinder));
                //    try
                //    {
                //        foreach (var device in Cylinders)
                //        {
                //            (device as VCylinder).Retract();
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        errMessage += "所有气缸回原位失败";
                //        _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = errMessage });
                //        throw new FriendlyException($"{errMessage}I/O检查失败，气缸回原位失败。");
                //    }
                //}
                foreach (var item in ItemModels.OfType<AssTbDigitalIn>())
                {
                    item.实测 = "";
                    item.状态 = "";
                }
                // 获取所有VIO设备
                var VIOs = _deviceEngine.GetDevices(typeof(VIO));

                for (int i = 0; i < ItemModels.Count; i++)
                {
                    //输入
                    if (ItemModels[i] is AssTbDigitalIn io)
                    {
                        // 检查软件版本是否为空
                        if (string.IsNullOrWhiteSpace(io.标准))
                        {
                            io.状态 = "格式错误";
                            continue;
                        }

                        var vio = VIOs.FirstOrDefault(x => x.Name == io.项次) as VIO;

                        if (vio == null)
                        {
                            io.状态 = "设备未找到";
                            continue;
                        }
                        if (vio.Behavior == IOBehavior.Input)
                        {
                            // 如果交替检测未通过，则使用原有的单次检测逻辑
                            io.实测 = vio.GetDigitalIn().ToString();
                            io.状态 = vio.GetDigitalIn() == Convert.ToBoolean(io.标准) ? "OK" : "NG";
                        }
                        //if (vio.Behavior == IOBehavior.Output)
                        //{
                        //    io.实测 = vio.GetDigitalOut().ToString();
                        //    io.状态 = vio.GetDigitalOut() == Convert.ToBoolean(io.标准) ? "OK" : "NG";
                        //}
                        io.完成时间 = DateTime.Now;
                        ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                    }
                    //输出
                    if (ItemModels[i] is AssTbDigitalOut outIO)
                    {
                        // 检查软件版本是否为空
                        if (string.IsNullOrWhiteSpace(outIO.标准))
                        {
                            outIO.状态 = "格式错误";
                            continue;
                        }

                        var vio = VIOs.FirstOrDefault(x => x.Name == outIO.项次) as VIO;

                        if (vio == null)
                        {
                            outIO.状态 = "设备未找到";
                            continue;
                        }
                        if (vio.Behavior == IOBehavior.Input)
                        {
                            vio = VIOs.OfType<VIO>().FirstOrDefault(x => x.Name == outIO.项次 && x.Behavior == IOBehavior.Output);
                            //outIO.实测 = vio.GetDigitalIn().ToString();
                            //outIO.状态 = outIO.实测 == outIO.标准 ? "OK" : "NG";

                        }
                        if (vio != null && vio.Behavior == IOBehavior.Output)
                        {
                            outIO.实测 = vio.GetDigitalOut().ToString();
                            outIO.状态 = outIO.实测 == outIO.标准 ? "OK" : "NG";
                        }
                        outIO.完成时间 = DateTime.Now;
                        ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                    }
                }
            }
            catch (Exception e)
            {
                throw new FriendlyException(errMessage + e);
            }

        }

        private async Task Check_IO_Single(CancellationToken token = default)
        {
            string errMessage = string.Empty;

            try
            {
                ProgressValue = 0; // 进度
                //弹窗确认
                var confirmResult = await ShowConfirmAsync("确定要执行IO检测（单个）操作吗？");
                if (confirmResult != ButtonResult.OK)
                {
                    // 用户取消
                    foreach (var item in ItemModels)
                    {
                        if (item is AssTbDigitalInSingle inIo)
                        {
                            inIo.实测 = "";
                            inIo.状态 = "未完成";
                        }

                        else if (item is AssTbDigitalOutSingle outIo)
                        {
                            outIo.实测 = "";
                            outIo.状态 = "未完成";
                        }
                    }
                    return;
                }
                foreach (var item in ItemModels.OfType<AssTbDigitalInSingle>())
                {
                    if (string.IsNullOrEmpty(item.状态))
                    {
                        item.实测 = "";
                    }
                }
                // 获取所有VIO设备
                var VIOs = _deviceEngine.GetDevices(typeof(VIO));

                for (int i = 0; i < ItemModels.Count; i++)
                {
                    if (token.IsCancellationRequested) return;
                    //输入
                    if (ItemModels[i] is AssTbDigitalInSingle io)
                    {
                        if (io.状态 == "OK" || io.状态 == "跳过")
                        {
                            continue; // 跳过已OK的项
                        }
                        // 检查IO标准值是否为空
                        if (string.IsNullOrWhiteSpace(io.标准))
                        {
                            io.状态 = "格式错误";
                            continue;
                        }

                        var vio = VIOs.FirstOrDefault(x => x.Name == io.项次) as VIO;

                        if (vio == null)
                        {
                            io.状态 = "设备未找到";
                            continue;
                        }
                        if (vio.Behavior == IOBehavior.Input)
                        {
                            // 首先尝试True/False交替触发检测
                            var alternatingResult = await CheckIOAlternatingAsync(vio, io, token);
                            io.状态 = alternatingResult switch
                            {
                                IOAlternatingResult.OK => "OK",
                                IOAlternatingResult.Skip => "跳过",
                                IOAlternatingResult.Cancel => "Cancel",
                                IOAlternatingResult.Error => "Error",
                            };

                        }
                        io.完成时间 = DateTime.Now;
                        ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                    }
                    //输出
                    else if (ItemModels[i] is AssTbDigitalOutSingle outIO)
                    {
                        if (outIO.状态 == "OK" || outIO.状态 == "跳过") continue;
                        if (string.IsNullOrWhiteSpace(outIO.标准))
                        {
                            outIO.状态 = "格式错误";
                            continue;
                        }
                        var vio = VIOs.FirstOrDefault(x => x.Name == outIO.项次) as VIO;
                        if (vio == null)
                        {
                            outIO.状态 = "设备未找到";
                            continue;
                        }
                        if (vio.Behavior == IOBehavior.Input)
                        {
                            // 查找和输入同名的 Output
                            vio = VIOs.OfType<VIO>().FirstOrDefault(x => x.Name == outIO.项次 && x.Behavior == IOBehavior.Output);
                        }
                        if (vio != null && vio.Behavior == IOBehavior.Output)
                        {
                            bool targetLevel = !Convert.ToBoolean(outIO.标准);

                            // 等待当前输出对话框关闭（确保同时只有一个对话框）
                            while (_currentOutputDialog != null)
                            {
                                await Task.Delay(50, token);
                            }

                            // 使用TaskCompletionSource等待对话框关闭
                            var tcs = new TaskCompletionSource<(IOCheckResult Result, bool IsButtonClicked)>();

                            // 在UI线程上弹出对话框（非阻塞）
                            await _dispatcher.InvokeAsync(() =>
                            {
                                lock (_dialogLock)
                                {
                                    // 再次检查，防止竞态条件
                                    if (_currentOutputDialog != null)
                                    {
                                        tcs.SetResult((IOCheckResult.Skip, false));
                                        return;
                                    }

                                    // 弹出窗体，用于人工确认输出状态
                                    var dialog = new IOCheckDialog(vio, targetLevel, i, _configService, _floatingInfoService);
                                    _currentOutputDialog = dialog;

                                    dialog.Closed += (s, e) =>
                                    {
                                        lock (_dialogLock)
                                        {
                                            _currentOutputDialog = null;
                                        }
                                        tcs.TrySetResult((dialog.Result, dialog.IsButtonClicked));
                                    };

                                    // 使用Show()而非ShowDialog()，非阻塞显示
                                    dialog.Show();
                                }
                            });

                            // 等待对话框关闭
                            var dialogResult = await tcs.Task;

                            if (dialogResult.IsButtonClicked == false)
                            {
                                if (vio.GetDigitalOut().ToString() != outIO.标准)
                                {
                                    vio.SetDigital(Convert.ToBoolean(outIO.标准));
                                }
                                return;
                            }
                            // 获取用户点击的结果，填充"状态"，实测无需填写
                            switch (dialogResult.Result)
                            {
                                case IOCheckResult.OK:
                                    outIO.状态 = "OK";
                                    break;
                                case IOCheckResult.NG:
                                    outIO.状态 = "NG";
                                    break;
                                case IOCheckResult.Skip:
                                    outIO.状态 = "跳过";
                                    break;
                            }
                        }
                        // 当前IO点检结束后，将IO的值设置为标准值
                        if (vio.GetDigitalOut().ToString() != outIO.标准)
                        {
                            vio.SetDigital(Convert.ToBoolean(outIO.标准));
                        }
                        //outIO.实测 = vio.GetDigitalOut().ToString();
                        outIO.完成时间 = DateTime.Now;
                        ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                    }
                }
            }
            catch (Exception e)
            {
                throw new FriendlyException(errMessage + e);
            }

        }

        /// <summary>
        /// 检测IO信号True/False交替触发逻辑
        /// 当IO信号完成True→False→True或False→True→False的交替变化时，判定为OK
        /// 不需要超时，只需要界面提示人工操作IO
        /// </summary>
        /// <param name="vio">VIO设备</param>
        /// <param name="ioItem">IO数据项（用于更新界面显示）</param>
        /// <param name="token">取消令牌</param>
        /// <param name="timeoutMs">此参数已废弃，不再使用</param>
        /// <returns>如果检测到交替变化返回true，否则返回false</returns>
        private async Task<IOAlternatingResult> CheckIOAlternatingAsync(VIO vio, dynamic ioItem, CancellationToken token, int timeoutMs = 10000)
        {
            try
            {
                // 根据IO类型获取初始状态
                bool initialState;
                if (vio.Behavior == IOBehavior.Input)
                {
                    initialState = vio.GetDigitalIn();
                }
                else
                {
                    initialState = vio.GetDigitalOut();
                }

                ioItem.实测 = initialState.ToString();
                LogStatus($"IO [{vio.Name}] 开始检测交替触发，初始状态: {initialState}");

                // 等待当前对话框关闭（确保同时只有一个对话框）
                while (_currentDialog != null)
                {
                    await Task.Delay(50, token);
                }

                // 使用TaskCompletionSource等待对话框关闭
                var tcs = new TaskCompletionSource<IOAlternatingResult>();

                // 在UI线程上弹出对话框
                await _dispatcher.InvokeAsync(() =>
                {
                    lock (_dialogLock)
                    {
                        // 再次检查，防止竞态条件
                        if (_currentDialog != null)
                        {
                            tcs.SetResult(IOAlternatingResult.Cancel);
                            return;
                        }

                        var dialog = new IOAlternatingCheckDialog(vio, initialState, _configService, _floatingInfoService);
                        _currentDialog = dialog;

                        dialog.Closed += (s, e) =>
                        {
                            lock (_dialogLock)
                            {
                                _currentDialog = null;
                            }
                            tcs.TrySetResult(dialog.Result);
                        };

                        dialog.Show();
                    }
                });

                // 等待对话框关闭
                var result = await tcs.Task;

                // 根据对话框结果判断
                if (result == IOAlternatingResult.OK)
                {
                    LogStatus($"IO [{vio.Name}] 检测到有效的True/False交替触发，判定为OK");
                    ioItem.实测 = "交替触发OK";
                }
                else if (result == IOAlternatingResult.Skip)
                {
                    LogStatus($"IO [{vio.Name}] 检测被用户跳过");
                    ioItem.实测 = "跳过";
                }
                else
                {
                    LogStatus($"IO [{vio.Name}] 检测被用户取消");
                    ioItem.实测 = "取消";
                }
                return result;
            }
            catch (OperationCanceledException)
            {
                LogStatus($"IO [{vio.Name}] 检测被取消");
                return IOAlternatingResult.Cancel;
            }
            catch (Exception ex)
            {
                LogStatus($"IO [{vio.Name}] 检测过程中发生异常: {ex.Message}");
                return IOAlternatingResult.Error;
            }
        }

        private IEnumerable<VIO> GetIOList(string key = "", IOBehavior behavior = IOBehavior.Input, IOType ioType = IOType.Digital)
        {
            // 控制卡
            string CurrentCard = null;
            var Cards = _deviceEngine.GetRealDevices(typeof(IMotionCard)).Select(u => u.Name).ToList();
            if (Cards.Count > 0)
            {
                CurrentCard = Cards[0];
            }
            var list = _deviceEngine.GetDevices(typeof(VIO))
                .Select(u => u as VIO)
                .Where(u => u.CardName == CurrentCard)
                .Where(u => u.IOType == ioType)
                .Where(u => u.Behavior == behavior)
                .Where(u => !u.Name.Contains("备用") && !u.Name.Contains("弃用"));

            if (!string.IsNullOrEmpty(key))
            {
                list = list.Where(u => u.CardName.Contains(key) || u.Name.Contains(key)).Where(u => !u.Name.Contains("备用") && !u.Name.Contains("弃用"));
            }

            return list;
        }

        private void FillTableContent(AssTbVacuum swVersion)
        {
            if ("" == swVersion.实测)
            {
                swVersion.状态 = "未完成";
            }
            else if (swVersion.标准 == swVersion.实测)
            {
                swVersion.状态 = "OK";
            }
            else
            {
                swVersion.状态 = "NG";
            }
        }

        // 原点限位：单轴回零点检
        private async Task Check_OriginLimit(CancellationToken token)
        {
            try
            {
                // 循环前只弹一次确认
                var confirmResult = await ShowConfirmAsync("确定要依次执行所有单轴的原点限位（回零）操作吗？");
                if (confirmResult != ButtonResult.OK)
                {
                    // 用户取消
                    foreach (var item in ItemModels)
                    {
                        if (item is AssTbOriginLimit oriLimitTb)
                        {
                            //oriLimitTb.实测 = "";
                            oriLimitTb.状态 = "未完成";
                        }
                    }
                    return;
                }
                foreach (var item in ItemModels.OfType<AssTbOriginLimit>())
                {
                    item.实测 = "";
                    item.状态 = "";
                }
                ProgressValue = 0; // 进度
                for (int i = 0; i < ItemModels.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    if (ItemModels[i] is AssTbOriginLimit oriLimitTb)
                    {
                        if (string.IsNullOrWhiteSpace(oriLimitTb.标准))
                        {
                            oriLimitTb.状态 = "格式错误";
                            ProgressValue = (i + 1) * 100 / ItemModels.Count;
                            continue;
                        }
                        var devices = _deviceEngine.GetDevices(typeof(VAxis));
                        if (!string.IsNullOrEmpty(oriLimitTb.项次))
                        {
                            //List<string> result = oriLimitTb.项次.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            var vAxis = devices.FirstOrDefault(x => x.Name == oriLimitTb.项次) as VAxis;
                            if (vAxis == null)
                            {
                                oriLimitTb.状态 = "设备未找到";
                                ProgressValue = (i + 1) * 100 / ItemModels.Count;
                                continue;
                            }

                            await CheckVAxisActionAsync(oriLimitTb, vAxis, "回零", oriLimitTb.标准, token);
                            if (!token.IsCancellationRequested)
                            {
                                ProgressValue = (i + 1) * 100 / ItemModels.Count;
                            }
                        }
                    }
                }
                throw new FriendlyException("所有单轴操作已完成，请检查结果。");
            }
            catch (Exception ex)
            {

            }
        }
        private async Task CheckVAxisActionAsync(AssTbOriginLimit oriLimitTb, VAxis vAxis, string action, string 标准, CancellationToken token)
        {
            await Task.Run(async () =>
            {
                if (action == "回零")
                {
                    vAxis.Home(); //执行回零操作
                    Thread.Sleep(100); //稍作延时，确保命令发送出去
                    var timeoutTask = Task.Delay(10000); // 10秒超时
                    var checkHomeTask = Task.Run(() =>
                    {
                        vAxis.CheckHomeDone();
                    }, token);
                    var completedTask = await Task.WhenAny(timeoutTask, checkHomeTask);
                    if (completedTask == timeoutTask)
                    {
                        oriLimitTb.状态 = "轴到位超时";
                        vAxis.Stop(); //发送停止命令
                    }
                    else if (completedTask == checkHomeTask)
                    {
                        double pos = vAxis.GetCurrentPos();
                        oriLimitTb.实测 = pos.ToString();
                    }
                }
                else
                    return;
            }, token);

            Application.Current.Dispatcher.Invoke(() =>
            {
                FillTableContent(oriLimitTb);
            });
        }
        // 原点限位支持标准值输入一个范围。
        public void FillTableContent(AssTbOriginLimit oriLimitTb)
        {
            if (string.IsNullOrEmpty(oriLimitTb.实测))
            {
                oriLimitTb.状态 = "未完成";
            }
            else if (string.IsNullOrEmpty(oriLimitTb.标准))
            {
                bool bIsOk = double.TryParse(oriLimitTb.实测, out double 实测浮点值);
                if (!bIsOk)
                {
                    oriLimitTb.状态 = "格式错误";
                }
                else
                {
                    // 与0比较，插值在0.1以内即OK
                    if (Math.Abs(实测浮点值 - 0) <= 0.1)
                    {
                        oriLimitTb.状态 = "OK";
                    }
                    else
                    {
                        oriLimitTb.状态 = "NG";
                    }
                }
            }
            else if (oriLimitTb.标准.Contains('~'))
            {
                var range = ParseColumnRange(oriLimitTb.标准);
                bool bIsOk = double.TryParse(oriLimitTb.实测, out double 实测浮点值);
                if (!bIsOk)
                {
                    oriLimitTb.实测 = 实测浮点值.ToString();//给默认值0
                }
                if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                {
                    oriLimitTb.状态 = "OK";
                }
                else
                {
                    oriLimitTb.状态 = "NG";
                }
            }
            else
            {
                oriLimitTb.状态 = "NG";
            }
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

        //private DelegateCommand _checkDeviceCommand;
        //public DelegateCommand CheckDeviceCommand1 => _checkDeviceCommand ?? (_checkDeviceCommand = new DelegateCommand(async () =>
        //{
        //    try
        //    {
        //        foreach (var item in ItemModels)
        //        {
        //            if (item is AssTbCylinder CylinderTb)
        //            {
        //                var devices = _deviceEngine.GetDevices(typeof(VCylinder));
        //                if (!string.IsNullOrEmpty(CylinderTb.项次))
        //                {
        //                    List<string> result = CylinderTb.项次.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //                    var vCylinder = devices.FirstOrDefault(x => x.Name == result[0]) as VCylinder;
        //                    if (vCylinder == null)
        //                    {
        //                        CylinderTb.状态 = "设备未找到";
        //                        continue;
        //                    }

        //                    if (result.Count > 1)
        //                    {
        //                        if (result[1].Contains("伸出"))
        //                        {
        //                            // 弹窗确认
        //                            _dialogService.ShowConfirm($"确定要执行 [{vCylinder.Name}] 伸出操作吗？", async r =>
        //                            {
        //                                if (r.Result == ButtonResult.OK)
        //                                {
        //                                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        //                                    await Task.Run(async () =>
        //                                    {
        //                                        vCylinder.Extend();
        //                                        // 等待IO到位，增加超时机制
        //                                        var timeout = TimeSpan.FromSeconds(5);
        //                                        var sw = System.Diagnostics.Stopwatch.StartNew();
        //                                        while (vCylinder.GetCurrentPos() != 10)
        //                                        {
        //                                            if (sw.Elapsed > timeout)
        //                                            {
        //                                                break;
        //                                            }
        //                                            await Task.Delay(10);
        //                                        }
        //                                    });
        //                                    stopwatch.Stop();
        //                                    Application.Current.Dispatcher.Invoke(() =>
        //                                    {
        //                                        CylinderTb.实测 = stopwatch.ElapsedMilliseconds.ToString();
        //                                        // 将 CylinderTb.实测 和 CylinderTb.标准 转换为 long 进行比较
        //                                        if (long.TryParse(CylinderTb.实测, out long 实测值) && long.TryParse(CylinderTb.标准, out long 标准值))
        //                                        {
        //                                            CylinderTb.状态 = 实测值 > 标准值 ? "NG" : "OK";
        //                                        }
        //                                        else
        //                                        {
        //                                            CylinderTb.状态 = "格式错误";
        //                                        }
        //                                    });
        //                                }
        //                                else
        //                                {
        //                                    CylinderTb.实测 = "0";
        //                                    CylinderTb.状态 = "用户取消";
        //                                }
        //                            });
        //                        }
        //                        else if (result[1].Contains("缩回"))
        //                        {
        //                            // 弹窗确认
        //                            _dialogService.ShowConfirm($"确定要执行 [{vCylinder.Name}] 缩回操作吗？", async r =>
        //                            {
        //                                if (r.Result == ButtonResult.OK)
        //                                {
        //                                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        //                                    await Task.Run(async () =>
        //                                    {
        //                                        vCylinder.Retract();
        //                                        // 等待IO到位，增加超时机制
        //                                        var timeout = TimeSpan.FromSeconds(5);
        //                                        var sw = System.Diagnostics.Stopwatch.StartNew();
        //                                        while (vCylinder.GetCurrentPos() != 0)
        //                                        {
        //                                            if (sw.Elapsed > timeout)
        //                                            {
        //                                                break;
        //                                            }
        //                                            await Task.Delay(10);
        //                                        }
        //                                    });
        //                                    stopwatch.Stop();
        //                                    Application.Current.Dispatcher.Invoke(() =>
        //                                    {
        //                                        CylinderTb.实测 = stopwatch.ElapsedMilliseconds.ToString();
        //                                        // 将 CylinderTb.实测 和 CylinderTb.标准 转换为 long 进行比较
        //                                        if (long.TryParse(CylinderTb.实测, out long 实测值) && long.TryParse(CylinderTb.标准, out long 标准值))
        //                                        {
        //                                            CylinderTb.状态 = 实测值 > 标准值 ? "NG" : "OK";
        //                                        }
        //                                        else
        //                                        {
        //                                            CylinderTb.状态 = "格式错误";
        //                                        }
        //                                    });
        //                                }
        //                                else
        //                                {
        //                                    CylinderTb.实测 = "0";
        //                                    CylinderTb.状态 = "用户取消";
        //                                }
        //                            });
        //                        }
        //                        else
        //                        {
        //                            CylinderTb.实测 = "0";
        //                            CylinderTb.状态 = "标准格式错误";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        CylinderTb.实测 = "0";
        //                        CylinderTb.状态 = "标准格式错误";
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}));

        #region 批量导入IO点检图片功能

        /// <summary>
        /// 批量导入IO点检图片命令处理方法
        /// </summary>
        private async void OnBatchImportImages()
        {
            await ImportIOImagesAsync();
        }

        /// <summary>
        /// 批量导入IO点检图片的核心逻辑
        /// </summary>
        private async Task ImportIOImagesAsync()
        {
            try
            {
                // 获取默认图片目录
                var configDir = Path.GetDirectoryName(_configService.GetConfigPath());
                var defaultImageDir = Path.Combine(configDir, "Images");

                // 使用文件夹浏览器对话框选择图片目录
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "选择存放IO点检图片的文件夹";
                    dialog.ShowNewFolderButton = true;

                    if (Directory.Exists(defaultImageDir))
                    {
                        dialog.SelectedPath = defaultImageDir;
                    }

                    var result = System.Windows.Forms.DialogResult.OK;

                    // 在UI线程上显示对话框
                    await _dispatcher.InvokeAsync(() =>
                    {
                        result = dialog.ShowDialog();
                    });

                    if (result != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }

                    string selectedPath = dialog.SelectedPath;

                    // 检查目录是否存在
                    if (!Directory.Exists(selectedPath))
                    {
                        await ShowMessageAsync("错误", $"选择的目录不存在: {selectedPath}");
                        return;
                    }

                    // 获取目录下所有支持的图片文件
                    var imageFiles = GetImageFiles(selectedPath);

                    if (imageFiles.Count == 0)
                    {
                        await ShowMessageAsync("提示", $"在目录 {selectedPath} 中未找到支持的图片文件。\n支持的格式: .png, .jpg, .jpeg, .bmp, .gif");
                        return;
                    }

                    // 获取所有IO名称
                    var ioNames = GetAllIONames();

                    // 执行匹配
                    var matchResult = MatchImagesWithIOs(imageFiles, ioNames);

                    // 如果没有匹配成功，显示结果并返回
                    if (matchResult.MatchedCount == 0)
                    {
                        ShowImportResultDialog(matchResult, selectedPath, false);
                        return;
                    }

                    // 确认是否更新配置
                    var confirmResult = await ShowConfirmAsync(
                        $"找到 {matchResult.MatchedCount} 个匹配的IO图片\n" +
                        $"未匹配的图片: {matchResult.UnmatchedImages.Count}\n" +
                        $"未匹配的IO: {matchResult.UnmatchedIONames.Count}\n\n" +
                        $"是否继续更新配置？");

                    if (confirmResult != ButtonResult.OK)
                    {
                        return;
                    }

                    // 备份原配置文件
                    BackupConfigFile();

                    // 更新配置
                    await UpdateConfigsAsync(matchResult, configDir);

                    // 显示成功结果
                    ShowImportResultDialog(matchResult, selectedPath, true);
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("错误", $"批量导入失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定目录下所有支持的图片文件
        /// </summary>
        /// <param name="directory">图片目录</param>
        /// <returns>图片文件字典（文件名不含扩展名 -> 完整路径）</returns>
        private Dictionary<string, string> GetImageFiles(string directory)
        {
            var supportedExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
            var imageFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.GetFiles(directory))
            {
                var extension = Path.GetExtension(file).ToLower();
                if (supportedExtensions.Contains(extension))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    imageFiles[fileNameWithoutExt] = file;
                }
            }

            return imageFiles;
        }

        /// <summary>
        /// 获取所有IO名称
        /// </summary>
        /// <returns>IO名称列表</returns>
        private List<string> GetAllIONames()
        {
            var ioNames = new List<string>();

            // 从设备引擎获取所有VIO设备
            var vios = _deviceEngine.GetDevices(typeof(VIO));
            foreach (var vio in vios)
            {
                if (!string.IsNullOrEmpty(vio.Name) &&
                    !vio.Name.Contains("备用") &&
                    !vio.Name.Contains("弃用"))
                {
                    ioNames.Add(vio.Name);
                }
            }

            return ioNames;
        }

        /// <summary>
        /// 匹配图片文件与IO名称
        /// </summary>
        /// <param name="imageFiles">图片文件字典</param>
        /// <param name="ioNames">IO名称列表</param>
        /// <returns>匹配结果</returns>
        private ImageMatchResult MatchImagesWithIOs(Dictionary<string, string> imageFiles, List<string> ioNames)
        {
            var result = new ImageMatchResult();

            foreach (var ioName in ioNames)
            {
                // 不区分大小写匹配
                var matchedImage = imageFiles.FirstOrDefault(kvp =>
                    string.Equals(kvp.Key, ioName, StringComparison.OrdinalIgnoreCase)).Value;

                if (!string.IsNullOrEmpty(matchedImage))
                {
                    result.MatchedPairs.Add(ioName, matchedImage);
                }
                else
                {
                    result.UnmatchedIONames.Add(ioName);
                }
            }

            // 找出未匹配的图片
            foreach (var imageFile in imageFiles)
            {
                var isMatched = result.MatchedPairs.Values.Any(path =>
                    string.Equals(path, imageFile.Value, StringComparison.OrdinalIgnoreCase));

                if (!isMatched)
                {
                    result.UnmatchedImages.Add(imageFile.Key);
                }
            }

            result.MatchedCount = result.MatchedPairs.Count;

            return result;
        }

        /// <summary>
        /// 批量更新IO图片配置
        /// </summary>
        /// <param name="matchResult">匹配结果</param>
        /// <param name="configDir">配置文件目录</param>
        private async Task UpdateConfigsAsync(ImageMatchResult matchResult, string configDir)
        {
            await Task.Run(() =>
            {
                foreach (var pair in matchResult.MatchedPairs)
                {
                    var ioName = pair.Key;
                    var imagePath = pair.Value;

                    // 计算相对路径
                    var relativePath = GetRelativePath(configDir, imagePath);

                    // 获取现有配置
                    var config = _configService.GetConfig(ioName);

                    // 如果配置中没有图片项，创建新配置
                    if (config.ContentItems == null || config.ContentItems.Count == 0)
                    {
                        config.ContentItems = new ObservableCollection<Luster.Common.Assets.FloatingInfo.Models.ContentItem>();
                    }

                    // 查找是否已有图片项
                    var existingImageItem = config.ContentItems.OfType<Luster.Common.Assets.FloatingInfo.Models.ImageContentItem>().FirstOrDefault();

                    if (existingImageItem != null)
                    {
                        // 更新现有图片路径
                        existingImageItem.ImagePath = relativePath;
                    }
                    else
                    {
                        // 添加新的图片项
                        var imageItem = new Luster.Common.Assets.FloatingInfo.Models.ImageContentItem
                        {
                            Order = config.ContentItems.Count,
                            ImagePath = relativePath,
                            MaxWidth = 400,
                            MaxHeight = 300
                        };
                        config.ContentItems.Add(imageItem);
                    }

                    // 保存配置
                    _configService.SaveConfig(config);
                }
            });
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="basePath">基础路径</param>
        /// <param name="fullPath">完整路径</param>
        /// <returns>相对路径</returns>
        private string GetRelativePath(string basePath, string fullPath)
        {
            try
            {
                var baseUri = new Uri(basePath + Path.DirectorySeparatorChar);
                var fullUri = new Uri(fullPath);
                var relativeUri = baseUri.MakeRelativeUri(fullUri);
                return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
            }
            catch
            {
                // 如果计算相对路径失败，返回绝对路径
                return fullPath;
            }
        }

        /// <summary>
        /// 备份配置文件
        /// </summary>
        private void BackupConfigFile()
        {
            try
            {
                var configPath = _configService.GetConfigPath();
                if (File.Exists(configPath))
                {
                    var backupPath = configPath + ".backup";
                    File.Copy(configPath, backupPath, true);
                }
            }
            catch
            {
                // 备份失败，忽略
            }
        }

        /// <summary>
        /// 显示导入结果对话框
        /// </summary>
        /// <param name="result">匹配结果</param>
        /// <param name="selectedPath">选择的路径</param>
        /// <param name="isSuccess">是否成功</param>
        private void ShowImportResultDialog(ImageMatchResult result, string selectedPath, bool isSuccess)
        {
            var message = isSuccess ? "批量导入成功！\n\n" : "批量导入结果：\n\n";
            message += $"成功匹配: {result.MatchedCount} 个\n";

            if (result.UnmatchedImages.Count > 0)
            {
                message += $"\n未匹配的图片 ({result.UnmatchedImages.Count} 个):\n";
                foreach (var img in result.UnmatchedImages.Take(10))
                {
                    message += $"  - {img}\n";
                }
                if (result.UnmatchedImages.Count > 10)
                {
                    message += $"  ... 还有 {result.UnmatchedImages.Count - 10} 个\n";
                }
            }

            if (result.UnmatchedIONames.Count > 0)
            {
                message += $"\n未匹配的IO ({result.UnmatchedIONames.Count} 个):\n";
                foreach (var io in result.UnmatchedIONames.Take(10))
                {
                    message += $"  - {io}\n";
                }
                if (result.UnmatchedIONames.Count > 10)
                {
                    message += $"  ... 还有 {result.UnmatchedIONames.Count - 10} 个\n";
                }
            }

            _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = message });

            // 显示消息框
            System.Windows.MessageBox.Show(message, isSuccess ? "导入成功" : "导入结果",
                System.Windows.MessageBoxButton.OK,
                isSuccess ? System.Windows.MessageBoxImage.Information : System.Windows.MessageBoxImage.Warning);
        }

        /// <summary>
        /// 显示消息对话框
        /// </summary>
        private async Task ShowMessageAsync(string title, string message)
        {
            await _dispatcher.InvokeAsync(() =>
            {
                System.Windows.MessageBox.Show(message, title,
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            });
        }

        #endregion

        #region 内部类

        /// <summary>
        /// 图片匹配结果
        /// </summary>
        private class ImageMatchResult
        {
            /// <summary>
            /// 匹配成功的对 (IO名称 -> 图片路径)
            /// </summary>
            public Dictionary<string, string> MatchedPairs { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// 未匹配的IO名称列表
            /// </summary>
            public List<string> UnmatchedIONames { get; set; } = new List<string>();

            /// <summary>
            /// 未匹配的图片名称列表（不含扩展名）
            /// </summary>
            public List<string> UnmatchedImages { get; set; } = new List<string>();

            /// <summary>
            /// 匹配成功的数量
            /// </summary>
            public int MatchedCount { get; set; }
        }

        #endregion
    }
}
