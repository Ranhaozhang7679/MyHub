#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VIOContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VIOContentVM.cs
* 创建时间:       2022/4/21 18:39:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      acdd297a-3483-4986-b20a-2af8cef1993c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/21 18:39:33
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Interactivity;
using Luster.Common.Assets;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Luster.SimDevice.SubSystem.Extension;
using Luster.SimDevice.SubSystem.Langs;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VIOContentVM : PageVM
    {
        /// <summary>
        /// 输入每页数量
        /// </summary>
        private int _PerPageCount = 10;
        public int PerPageCount
        {
            get => _PerPageCount;
            set => SetProperty(ref _PerPageCount, value);
        }

        /// <summary>
        /// 输入项页数
        /// </summary>
        private int _PageCount;
        public int PageCount
        {
            get => _PageCount;
            set => SetProperty(ref _PageCount, value);
        }

        /// <summary>
        /// 输出项当前页
        /// </summary>
        private int _PageIndex = 1;
        public int PageIndex
        {
            get => _PageIndex;
            set => SetProperty(ref _PageIndex, value);
        }
        /// <summary>
        /// 输入每页数量
        /// </summary>
        private int _PerPageCount2 = 10;
        public int PerPageCount2
        {
            get => _PerPageCount2;
            set => SetProperty(ref _PerPageCount2, value);
        }
        /// <summary>
        /// 分组
        /// </summary>
        private string _module = "Null";
        public string Module
        {
            get => _module; set
            {
                SetProperty(ref _module, value);
            }
        }
        /// <summary>
        /// 输入项页数
        /// </summary>
        private int _PageCount2;
        public int PageCount2
        {
            get => _PageCount2;
            set => SetProperty(ref _PageCount2, value);
        }

        /// <summary>
        /// 输出项当前页
        /// </summary>
        private int _PageIndex2 = 1;
        public int PageIndex2
        {
            get => _PageIndex2;
            set => SetProperty(ref _PageIndex2, value);
        }
        private string _searchParas;
        public string SearchParas
        {
            get => _searchParas;
            set => SetProperty(ref _searchParas, value);
        }

        /// <summary>
        /// 控制卡
        /// </summary>
        private string _currentCard;
        public string CurrentCard
        {
            get => _currentCard;
            set => SetProperty(ref _currentCard, value);
        }

        /// <summary>
        /// 控制卡集合
        /// </summary>
        private List<string> _cards;
        public List<string> Cards
        {
            get => _cards;
            set => SetProperty(ref _cards, value);
        }

        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 显示移除按钮
        /// </summary>
        public override bool IsShowRemove => true;

        private const string ButtonAnglogName = "模拟赋值";
        private const string ButtonRealName = "输出赋值";
        public DelegateCommand QueryOutPutCommand { get; set; }
        public DelegateCommand<FunctionEventArgs<int>> OutPutPageUpdatedCommand { get; private set; }

        public DelegateCommand QueryInputCommand { get; set; }
        public DelegateCommand BatchExportCommand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 缓存最后一次 ListBox 的尺寸，用于切换 IO 类型时重新计算分页
        /// </summary>
        private Size _lastSize;

        protected VIOContentVM(ISimDeviceEngineUI _engine, Dispatcher dispatcher) : base(_engine)
        {
            _dispatcher = dispatcher;
            IoDatas = new ObservableCollection<IOModel>();
            InputDatas = new ObservableCollection<IOModel>();
            SearchParas = string.Empty;
            Cards = new List<string>();

            BatchExportCommand = new DelegateCommand(BatchExport);
            SpanTime = 500;
            Modules = new();
            Modules.Add("Null");
            Modules.AddRange(deviceEngine.GetModules());
            // 控制卡
            Cards = deviceEngine.GetRealDevices(typeof(IMotionCard)).Select(u => u.Name).ToList();
            if (Cards.Count > 0)
            {
                CurrentCard = Cards[0];
            }
        }

        private DelegateCommand<string> _pageUpdatedCommand;
        public DelegateCommand<string> PageUpdatedCommand => _pageUpdatedCommand ?? (_pageUpdatedCommand = new DelegateCommand<string>((str) =>
        {

            var behavior = str == "Input" ? IOBehavior.Input : IOBehavior.Output;
            PageUpdated(behavior);

        }));
        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<string> _changeModuleCommand;
        public DelegateCommand<string> ChangeModuleCommand => _changeModuleCommand ?? (_changeModuleCommand = new DelegateCommand<string>((module) =>
        {

            // 页面刷新
            LoadDatas();
        }));
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="info"></param>
        private void PageUpdated(IOBehavior behavior)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                pauseReset.Reset();
                //var pageIndex = info.Info;


                IOType ioType = (IsDigitalIn || IsDigitalOut) ? IOType.Digital : IOType.Analog;

                var list = GetIOList(SearchParas, behavior, ioType);
                var count = list.Count();
                if (behavior == IOBehavior.Input)
                {

                    InputDatas?.Clear();
                    PageCount2 = count / PerPageCount2 + (count % PerPageCount2 == 0 ? 0 : 1);
                    foreach (var item in list.Skip((PageIndex2 - 1) * PerPageCount2).Take(PerPageCount2))
                    {
                        var ioModel = new IOModel(item);
                        //ioModel.SelectedChanged -= IoModel_SelectedChanged;
                        //ioModel.SelectedChanged += IoModel_SelectedChanged;
                        InputDatas.Add(ioModel);
                    }
                    // IO界面刷新
                    pauseReset.Set();
                    return;
                }

                IoDatas?.Clear();
                PageCount = count / PerPageCount + (count % PerPageCount == 0 ? 0 : 1);
                foreach (var item in list.Skip((PageIndex - 1) * PerPageCount).Take(PerPageCount))
                {
                    var ioModel = new IOModel(item);
                    //ioModel.SelectedChanged -= IoModel_SelectedChanged;
                    //ioModel.SelectedChanged += IoModel_SelectedChanged;
                    IoDatas.Add(ioModel);
                }


                pauseReset.Set();
            });
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="engineUI"></param>
        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            // 继承父类的注册，可以实时更新IsReal的值
            base.Subscribe(engineUI);

            // 打开已有工程清理数据
            engineUI.Subscribe<ProjOpenEvent, ProjectInfo>((proj) =>
            {
                LoadDatas();
            });
        }

        protected override void ModeChanged(DeviceMode mode)
        {
            base.ModeChanged(mode);

            _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                SetValueBtnName = IsReal ? LangProvider.GetLang("PutValue") : LangProvider.GetLang("SimulationValue");


                foreach (var item in IoDatas)
                {
                    if (IsDigitalOut)
                    {
                        item.DigitalValue = item.Tag.GetDigitalOut();
                        //var oldValue = item.DigitalValue;
                        //var newValue = item.Tag.GetDigitalOut();
                        //if (newValue != oldValue)
                        //{
                        //    item.Tag.SetDigital(oldValue);
                        //}
                    }

                    else if (IsAnalogOut)
                    {
                        item.AnalogValue = item.Tag.GetAnglogOut();
                    }
                    // 如果开始暂停了，那么直接中断循环
                    if (pauseReset != null && !pauseReset.IsSet)
                    {
                        break;
                    }

                }
                foreach (var item in InputDatas)
                {
                    if (IsDigitalIn)
                    {
                        item.DigitalValue = item.Tag.GetDigitalIn();
                    }
                    else if (IsAnalogIn)
                    {
                        item.AnalogValue = item.Tag.GetAnglogIn();
                    }
                    // 如果开始暂停了，那么直接中断循环
                    if (pauseReset != null && !pauseReset.IsSet)
                    {
                        break;
                    }
                }

            });
        }

        /// <summary>
        /// 加载IO和DO数据
        /// </summary>
        private void LoadDatas(string key = "")
        {
            PageUpdated(IOBehavior.Input);

            PageUpdated(IOBehavior.Output);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="behavior"></param>
        /// <param name="ioType"></param>
        /// <returns></returns>
        private IEnumerable<VIO> GetIOList(string key = "", IOBehavior behavior = IOBehavior.Input, IOType ioType = IOType.Digital)
        {
            var str = Module == "Null" ? "" : Module;
            var list = deviceEngine.GetDevices(typeof(VIO), str)
                .Select(u => u as VIO)
                .Where(u => u.CardName == CurrentCard)
                .Where(u => u.IOType == ioType)
                .Where(u => u.Behavior == behavior);

            if (!string.IsNullOrEmpty(key))
            {
                list = list.Where(u => u.CardName.Contains(key) || u.Name.Contains(key));
            }

            return list;
        }




        public override void AddNewItem()
        {
            dialogService.ShowIODialog((r) =>
            {
                if (r.Parameters.TryGetValue<IOType>("IOType", out var ioType))
                {
                    IsDigitalIn = ioType == IOType.Digital;
                    IsDigitalOut = ioType == IOType.Digital;
                    IsAnalogIn = ioType == IOType.Analog;
                    IsAnalogOut = ioType == IOType.Analog;

                    // 构架IO
                    LoadDatas();
                }
            });
        }

        /// <summary>
        /// 间隔事件
        /// </summary>
        private int _spanTime;
        public int SpanTime
        {
            get { return _spanTime; }
            set { SetProperty(ref _spanTime, value); }
        }

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            var args = obj as RoutedEventArgs;
            if (args != null && args.OriginalSource is ItemsControl ctrl)
            {
                CalcItemsSize(new Size(ctrl.ActualWidth, ctrl.ActualHeight));
            }


            LoadDatas();
        }));

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<object> _changeIOTypeCommand;
        public DelegateCommand<object> ChangeIOTypeCommand => _changeIOTypeCommand ?? (_changeIOTypeCommand = new DelegateCommand<object>((type) =>
        {
            var comItem = type as ComboBoxItem;
            var str = comItem.Tag.ToString();


            IsDigitalOut=IsDigitalIn = str == "Digital";
            IsAnalogIn= IsAnalogOut = str == "Analog";

            // 切换 IO 类型后重新计算分页参数
            CalcItemsSize(_lastSize);
            LoadDatas();
        }));

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<string> _changeCardCommand;
        public DelegateCommand<string> ChangeCardCommand => _changeCardCommand ?? (_changeCardCommand = new DelegateCommand<string>((card) =>
        {
            CurrentCard = card;

            // 页面刷新
            LoadDatas();
        }));

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(() =>
        {
            string trimKey = SearchParas.Trim();

            LoadDatas(trimKey);
        }));

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<IOModel> _configCommand;
        public DelegateCommand<IOModel> ConfigCommand => _configCommand ?? (_configCommand = new DelegateCommand<IOModel>((ioModel) =>
        {
            var func = () =>
            {
                return ioModel.Tag.GetAnglogIn();
            };
            dialogService.ShowCalculator(ioModel.Express, func, callback =>
            {
                if (callback.Result == ButtonResult.OK)
                {
                    if (callback.Parameters.TryGetValue<string>("Express", out var exp))
                    {
                        ioModel.Express = exp;
                    }
                }
            });
        }));



        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<object> _sizeChangedCommand;
        public DelegateCommand<object> SizeChangedCommand => _sizeChangedCommand ?? (_sizeChangedCommand = new DelegateCommand<object>((obj) =>
        {
            var sizeChanged = obj as SizeChangedEventArgs;
            if (sizeChanged != null)
            {
                var size = sizeChanged.NewSize;

                CalcItemsSize(size);
                LoadDatas();
            }
        }));


        private void CalcItemsSize(Size size)
        {
            _lastSize = size;

            // 计算页面能够容纳的IO数量
            int col = (int)(size.Width / 250);
            int row = (int)(size.Height / 35);

            if (IsAnalogIn || IsAnalogOut)
            {
                col = 1;
                row = (int)(size.Height / 42);
            }

            // 更新配置
            PerPageCount = row * col;
            PerPageCount2 = row * col;
        }

        /// <summary>
        /// 如果是数字量
        /// </summary>
        private bool _isDigtalIn = true;
        public bool IsDigitalIn
        {
            get { return _isDigtalIn; }
            set { SetProperty(ref _isDigtalIn, value); }
        }

        /// <summary>
        /// 如果是数字量
        /// </summary>
        private bool _isDigtalOut = true;
        public bool IsDigitalOut
        {
            get { return _isDigtalOut; }
            set { SetProperty(ref _isDigtalOut, value); }
        }

        /// <summary>
        /// 如果是模拟量
        /// </summary>
        private bool _isAnalogIn = false;
        public bool IsAnalogIn
        {
            get { return _isAnalogIn; }
            set { SetProperty(ref _isAnalogIn, value); }
        }

        /// <summary>
        /// 如果是模拟量
        /// </summary>
        private bool _isAnalogOut = false;
        public bool IsAnalogOut
        {
            get { return _isAnalogOut; }
            set { SetProperty(ref _isAnalogOut, value); }
        }

        /// <summary>
        /// 设置值名称
        /// </summary>
        private string _setValueBtnName = ButtonAnglogName;
        public string SetValueBtnName
        {
            get { return _setValueBtnName; }
            set { SetProperty(ref _setValueBtnName, value); }
        }

        /// <summary>
        /// 输出数组
        /// </summary>
        private ObservableCollection<IOModel> _ioDatas;
        public ObservableCollection<IOModel> IoDatas
        {
            get => _ioDatas;
            set=> SetProperty(ref _ioDatas, value);
        }
        /// <summary>
        /// 输入数组
        /// </summary>
        private ObservableCollection<IOModel> _inputDatas;
        public ObservableCollection<IOModel> InputDatas
        {
            get => _inputDatas;
            set => SetProperty(ref _inputDatas, value);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        public override void RemoveItem()
        {
            var outSelectIos = IoDatas.Where(u => u.IsSelected).ToList();
            var inSelectIos = InputDatas.Where(u => u.IsSelected).ToList();
            var selectIos = outSelectIos.Concat(inSelectIos).ToList();
            if (selectIos.Count == 0) return;

            string message = $"确认要删除信号:{string.Join(",", selectIos.Select(u => u.Name).ToArray())}?";
            dialogService.ShowConfirm(message, (r) =>
            {
                // 如果确认删除，则删除对象
                if (r.Result == ButtonResult.OK)
                {
                    pauseReset?.Reset();
                    var ids = selectIos.Select(u => u.Tag.ID).ToArray();
                    deviceEngine.ReomoveVirtual(ids);

                    // 清空并重新加载数据
                    IoDatas.Clear();
                    InputDatas.Clear();
                    LoadDatas();

                    pauseReset.Set();
                }
            });
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            StopMonitor();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            StartMonitor();
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        private ManualResetEventSlim pauseReset = new ManualResetEventSlim(true);

        /// <summary>
        /// 启动监控
        /// </summary>
        private void StartMonitor()
        {
            //return;
            if (whileToken != null)
            {
                pauseReset?.Set();
                Thread.Sleep(SpanTime + 100);
            }

            whileToken = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (true)
                {
                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        whileToken = null;
                        break;
                    }

                    if (pauseReset != null && !pauseReset.IsSet)
                    {
                        pauseReset.Wait();
                    }

                    // 更新
                    ModeChanged(SimEngineUI.DeviceMode);

                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        whileToken = null;
                        break;
                    }

                    Thread.Sleep(SpanTime + 20);
                }
                ;

            }, whileToken.Token);
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        private void StopMonitor()
        {
            //return;
            pauseReset?.Set();
            whileToken?.Cancel();
            Thread.Sleep(SpanTime + 20);
        }

        public override void Leave()
        {
            StopMonitor();
        }
        /// <summary>
        /// 进入
        /// </summary>
        public override void Enter()
        {
            StartMonitor();
        }

        #region Excel 导入和导出
        /// <summary>
        /// 批量导入
        /// </summary>
        private DelegateCommand _batchImportCommand;
        public DelegateCommand BatchImportCommand => _batchImportCommand ?? (_batchImportCommand = new DelegateCommand(() =>
        {
            var ioCount = deviceEngine.GetDevices(typeof(VIO)).Count();
            if (ioCount > 0)
            {
                dialogService.ShowConfirm("导入数据的同时清除历史数据?", r =>
                {
                    if (r.Result == ButtonResult.Cancel)
                    {
                        return;
                    }

                    if (r.Result == ButtonResult.OK)
                    {
                        // 清理已有数据
                        deviceEngine.ReomoveVirtual(typeof(VIO));
                    }

                    dialogService.ShowBatchImport(true, (res) =>
                    {
                        LoadDatas();
                        Modules = deviceEngine.GetModules();
                    });
                });
            }
            else
            {
                dialogService.ShowBatchImport(true, (res) =>
                {
                    LoadDatas();
                    Modules = deviceEngine.GetModules();
                });
            }
        }));
        private void BatchExport()
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "XLS|*.xls";
            var result = saveFile.ShowDialog().Value;
            if (result)
            {
                var filename = saveFile.FileName;
                var excel = new ExcelTool();
                SaveDigitalInIO(excel);
                SaveDigitalOutIO(excel);
                SaveAnalogInIO(excel);
                SaveAnalogOutIO(excel);
                excel.Save(filename);
            }
        }

        private void SaveDigitalInIO(ExcelTool excel)
        {
            var vios = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO)
                .Where(x => x.Behavior == IOBehavior.Input && x.IOType == IOType.Digital).ToList();
            var lists = new List<ExportVIOModel>();
            int i = 1;
            foreach (var io in vios)
            {
                var model = new ExportVIOModel();
                model.Index = i;
                model.PortNo = $"DI{i}";
                model.ModuleName = io.Module;
                model.Name = io.Name;
                model.SubDefinite = io.SubDefinite;
                lists.Add(model);

                i++;
            }
            WriteToSheet(excel, lists, "DI");
        }

        private void SaveDigitalOutIO(ExcelTool excel)
        {
            var vios = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO)
                .Where(x => x.Behavior == IOBehavior.Output && x.IOType == IOType.Digital).ToList();
            var lists = new List<ExportVIOModel>();
            int i = 1;
            foreach (var io in vios)
            {
                var model = new ExportVIOModel();
                model.Index = i;
                model.PortNo = $"DO{i}";
                model.ModuleName = io.Module;
                model.Name = io.Name;
                model.SubDefinite = io.SubDefinite;
                lists.Add(model);

                i++;
            }
            WriteToSheet(excel, lists, "DO");
        }

        private void SaveAnalogInIO(ExcelTool excel)
        {
            var vios = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO)
                 .Where(x => x.Behavior == IOBehavior.Input && x.IOType == IOType.Analog).ToList();
            var lists = new List<ExportVIOModel>();
            int i = 1;
            foreach (var io in vios)
            {
                var model = new ExportVIOModel();
                model.Index = i;
                model.PortNo = $"AI{i}";
                model.ModuleName = io.Module;
                model.Name = io.Name;
                lists.Add(model);

                i++;
            }
            WriteToSheet(excel, lists, "AI");
        }

        private void SaveAnalogOutIO(ExcelTool excel)
        {
            var vios = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO)
                .Where(x => x.Behavior == IOBehavior.Output && x.IOType == IOType.Analog).ToList();
            var lists = new List<ExportVIOModel>();
            int i = 1;
            foreach (var io in vios)
            {
                var model = new ExportVIOModel();
                model.Index = i;
                model.PortNo = $"AO{i}";
                model.ModuleName = io.Module;
                model.Name = io.Name;
                lists.Add(model);

                i++;
            }
            WriteToSheet(excel, lists, "AO");

        }

        private void WriteToSheet(ExcelTool excel, List<ExportVIOModel> exportVIOModels, string sheetName)
        {
            excel.AddWorksheet(sheetName);
            var prop = typeof(ExportVIOModel).GetProperties().Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null).ToList();
            var header = prop.Select(x => x.GetCustomAttribute<DisplayNameAttribute>().DisplayName).ToArray();
            var data = new object[header.Length];
            excel.SetHeaders(0, 0, header);
            for (int i = 0; i < exportVIOModels.Count(); i++)
            {
                for (int j = 0; j < header.Length; j++)
                {
                    data[j] = prop.Where(x => x.GetCustomAttribute<DisplayNameAttribute>().DisplayName == header[j]).FirstOrDefault()?.GetValue(exportVIOModels[i]);
                }
                excel.WriteRowDatas(i + 1, 0, data);
            }
        }
        #endregion


        public void ShowIODialog()
        {
            var selectIos = IoDatas.Where(u => u.IsSelected).ToList();
            if (selectIos.Count == 0) return;

            dialogService.ShowIOUpdateDialog(selectIos[0], (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<IOModel>("IOModel", out var iOModel))
                    {
                        selectIos[0] = iOModel;
                    }
                }
            });

        }
        public void ShowInPutDialog()
        {
            var selectIos = InputDatas.Where(u => u.IsSelected).ToList();
            if (selectIos.Count == 0) return;

            dialogService.ShowIOUpdateDialog(selectIos[0], (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<IOModel>("IOModel", out var iOModel))
                    {
                        selectIos[0] = iOModel;
                    }
                }
            });

        }

    }
}