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
               IFloatingInfoConfigService configService, IFloatingInfoService floatingInfoService) :
               base(repository, regionManager, commonBus, cSVHelper, flowBus, dialogService)
        {
            _configService = configService;
            _floatingInfoService = floatingInfoService;

            // 设置父页面Region名称，用于构建子页面浮动信息窗口的PageId
            //_parentRegionName = "IOinspectionContent";

            Pages = new ObservableCollection<CommonPageModel>();
            //Pages.Add(new CommonPageModel() { Name = "Vacuum", IsSelected = true, Region = "", ViewType = typeof(AssTbVacuum) });
            //Pages.Add(new CommonPageModel() { Name = "OriginLimit", IsSelected = false, Region = "", ViewType = typeof(AssTbOriginLimit) });
            //Pages.Add(new CommonPageModel() { Name = "Runners", IsSelected = false, Region = "", ViewType = typeof(AssTbRunners) });
            Pages.Add(new CommonPageModel() { Name = "Digital_In_Single", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalInSingle) });
            Pages.Add(new CommonPageModel() { Name = "Digital_Out_Single", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalOutSingle) });
            Pages.Add(new CommonPageModel() { Name = "Digital_In", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalIn) });
            Pages.Add(new CommonPageModel() { Name = "Digital_Out", IsSelected = false, Region = "", ViewType = typeof(AssTbDigitalOut) });
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
            LoadCheckConfirmMessages();
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
                if (status == "NG")
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
                PageStatusService.Instance.UpdateStatus(PageName, GetOverallStatus());
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                IsChecking = false;
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
                                if (result[1].Contains("伸出"))
                                {
                                    await CheckCylinderActionAsync(CylinderTb, vCylinder, "伸出", CylinderTb.标准, token);
                                }
                                else if (result[1].Contains("缩回"))
                                {
                                    await CheckCylinderActionAsync(CylinderTb, vCylinder, "缩回", CylinderTb.标准, token);
                                }
                                else
                                {
                                    CylinderTb.实测 = "0";
                                    CylinderTb.状态 = "格式错误";
                                }
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
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await Task.Run(async () =>
            {
                if (action == "伸出")
                    vCylinder.Extend();
                else
                    vCylinder.Retract();

                var timeout = TimeSpan.FromSeconds(5);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                int targetPos = action == "伸出" ? 10 : 0;
                while (vCylinder.GetCurrentPos() != targetPos)
                {
                    if (sw.Elapsed > timeout)
                    {
                        //CylinderTb.实测 = "动作超时";
                        break;
                    }
                    await Task.Delay(10);
                }
            }, token);
            stopwatch.Stop();
            Application.Current.Dispatcher.Invoke(() =>
            {
                var TIME = stopwatch.ElapsedMilliseconds.ToString();
                CylinderTb.实测 = TIME; //  + "ms"
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

                //if (标准.Length>=3)
                //{
                //    if (long.TryParse(TIME, out long 实测值) && long.TryParse(标准.Replace("ms",""), out long 标准值))
                //        CylinderTb.状态 = ((实测值 < 标准值)) ? "OK" : "NG";
                //    else
                //        CylinderTb.状态 = "格式错误";
                //}
                //else
                //{
                //    CylinderTb.状态 = "标准值格式错误";
                //}
                //    CylinderTb.实测 = TIME + "ms";
            });
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
                    if (ItemModels.Count>1)
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
                            项次 = cylinder.Name + "/" + "伸出",
                            标准 = "100~200", // 默认标准值为0
                            实测 = "", // 默认实测值为0
                            完成时间 = DateTime.Now
                        };
                        AssTbCylinder item1 = new AssTbCylinder()
                        {
                            项序 = i,
                            项次 = cylinder.Name + "/" + "缩回",
                            标准 = "100~200", // 默认标准值为0
                            实测 = "", // 默认实测值为0
                            完成时间 = DateTime.Now
                        };
                        tempCollection.Add(item);
                        tempCollection.Add(item1);
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
    }
}
