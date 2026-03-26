#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VAxisMContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VAxisMContentVM.cs
* 创建时间:       2022/6/13 20:55:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ca34e131-19a4-40c3-a4ff-f000a04478a3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/13 20:55:33
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Interactivity;
using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Luster.SimDevice.SubSystem.Langs;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class AxisIODebugContentVM : PageVM
    {
        /// <summary>
        /// 是否添加
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 点位列表
        /// </summary>
        private ObservableCollection<MotionPosGroupModel> _Currents;
        public ObservableCollection<MotionPosGroupModel> PosGroups
        {
            get { return _Currents; }
            set { SetProperty(ref _Currents, value); }
        }

        /// <summary>
        /// 轴列表
        /// </summary>
        private ObservableCollection<AxisModel> _SubAxisList;
        public ObservableCollection<AxisModel> SubAxisList
        {
            get { return _SubAxisList; }
            set { SetProperty(ref _SubAxisList, value); }
        }

        /// <summary>
        /// 轴列表
        /// </summary>
        private List<VAxis> axisDatas;
        public List<VAxis> AxisDatas
        {
            get => axisDatas; set => SetProperty(ref axisDatas, value);
        }

        private List<object> stationDatas;
        public List<object> StationDatas
        {
            get => stationDatas; set => SetProperty(ref stationDatas, value);
        }
        /// <summary>
        /// 轴列表
        /// </summary>
        private MotionPosGroupModel _current;
        public MotionPosGroupModel Current
        {
            get => _current; set => SetProperty(ref _current, value);
        }

        /// <summary>
        /// 优先级
        /// </summary>
        public List<KeyValue> Priorities { get; set; }

        #region IOCardPropertiesAndVariables
        private const string ButtonAnglogName = "模拟赋值";
        private const string ButtonRealName = "输出赋值";

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

        private string _searchIOText;
        public string SearchIOText
        {
            get => _searchIOText;
            set => SetProperty(ref _searchIOText, value);
        }

        /// <summary>
        /// 间隔事件
        /// </summary>
        private int _spanTime;
        public int SpanTime
        {
            get { return _spanTime; }
            set { SetProperty(ref _spanTime, value > 30 ? value : 30); }
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
        /// 输入数组
        /// </summary>
        private List<VIO> _ioInList;
        public List<VIO> IoInList
        {
            get => _ioInList;
            set => SetProperty(ref _ioInList, value);
        }

        /// <summary>
        /// 输出数组
        /// </summary>
        private List<VIO> _ioOutList;
        public List<VIO> IoOutList
        {
            get => _ioOutList;
            set => SetProperty(ref _ioOutList, value);
        }


        /// <summary>
        /// 输入数组
        /// </summary>
        private ObservableCollection<IOModel> _ioInDatas;
        public ObservableCollection<IOModel> IoInDatas
        {
            get => _ioInDatas;
            set => SetProperty(ref _ioInDatas, value);
        }

        /// <summary>
        /// 输出数组
        /// </summary>
        private ObservableCollection<IOModel> _ioOutDatas;
        public ObservableCollection<IOModel> IoOutDatas
        {
            get => _ioOutDatas;
            set => SetProperty(ref _ioOutDatas, value);
        }

        private Dispatcher _dispatcher;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine">引擎</param>
        protected AxisIODebugContentVM(ISimDeviceEngineUI _engine, IDialogService dialogService, Dispatcher dispatcher) : base(_engine)
        {
            _dialogService = dialogService;
            PosGroups = new ObservableCollection<MotionPosGroupModel>();
            SubAxisList = new ObservableCollection<AxisModel>();

            AxisDatas = new List<VAxis>();
            Priorities = typeof(Priority).EnumToDataSource();
            LoadDevices();

            _dispatcher = dispatcher;
            IoInList = new List<VIO>();
            IoOutList = new List<VIO>();
            IoInDatas = new ObservableCollection<IOModel>();
            IoOutDatas = new ObservableCollection<IOModel>();
            SearchIOText = string.Empty;
            SpanTime = 500;
            // 控制卡
            Cards = new List<string>();
            Cards = deviceEngine.GetRealDevices(typeof(IMotionCard)).Select(u => u.Name).ToList();
            if (Cards.Count > 0)
            {
                CurrentCard = Cards[0];
            }
        }

        /// <summary>
        /// 加载可用的轴
        /// </summary>
        private void LoadCanUseAxis()
        {
            //if (SubAxisList != null && SubAxisList.Count > 0)
            //{
            //    var hasAxises = SubAxisList.Select(u => u.ID).ToList();
            //    AxisDatas = deviceEngine.GetVDevices<VAxis>()
            //           .Where(u => !hasAxises.Contains(u.ID) && u.AxisType != AxisType.None)
            //           .ToList();
            //}
            //else
            //{
            //AxisDatas = deviceEngine.GetVDevices<VAxis>()
            //        .Where(u => u.AxisType != AxisType.None)
            //        .ToList();
            //}

            stationDatas = deviceEngine.GetModulesFromMotionEngine(null, null);
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            LoadCanUseAxis();
        }

        /// <summary>
        /// 添加
        /// </summary>
        //public override void AddNewItem()
        //{
        //    _dialogService.ShowAxisMDialog(r =>
        //    {
        //        if (r.Result == ButtonResult.OK)
        //        {
        //            if (r.Parameters.TryGetValue<VAxisM>("VAxisM", out var vM))
        //            {
        //                deviceEngine.AddVirtual(vM);
        //                LoadDevices();
        //            }
        //        }
        //    });
        //}

        /// <summary>
        /// 选中设备
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((obj) =>
        {
            Current = obj as MotionPosGroupModel;
            SubAxisList = new ObservableCollection<AxisModel>();
            if (Current != null)
            {
                foreach (var item in Current.Axises)
                {
                    var pos = Current.GetTeachPos(item.AxisNo);
                    AddSubItem(item, pos.Position, pos.MovePriority);
                }
            }

            //LoadCanUseAxis();
        }));

        ///// <summary>
        ///// 搜索命名
        ///// </summary>
        //private DelegateCommand<string> _searchCommand;
        //public DelegateCommand<string> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<string>((key) =>
        //{
        //    string trimKey = key.Trim();

        //    LoadDevices(trimKey);
        //}));

        /// <summary>
        /// 显示点位
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        private void AddSubItem(VAxis axis, double pos, Priority priority = Priority.Low)
        {
            var axisItem = new AxisModel(axis, false, whileToken);
            axisItem.TeachPos = pos;
            axisItem.MovePriority = priority;
            axisItem.PropertyChanged -= AxisItem_PropertyChanged;
            axisItem.PropertyChanged += AxisItem_PropertyChanged;
            SubAxisList.Add(axisItem);
        }

        private void AxisItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MovePriority" && Current != null && sender is AxisModel m)
            {
                Current.UpdatePriority(m.AxisNo, m.MovePriority);
            }
        }

        T ObjectCastToAnonymousType<T>(object obj, T type)
        {
            return (T)obj;
        }

        /// <summary>
        /// 双击添加
        /// </summary>
        private DelegateCommand<object> _doubleAddCommand;
        public DelegateCommand<object> DoubleAddCommand => _doubleAddCommand ?? (_doubleAddCommand = new DelegateCommand<object>((obj) =>
        {
            var stationID = obj.GetType().GetProperty("ID").GetValue(obj).ToString();
            if (string.IsNullOrEmpty(stationID))
            {
                throw new FriendlyException($"获取工站{obj.ToString()}ID失败!");
            }
            else
            {
                PosGroups.Clear();
                IoInList.Clear();
                IoOutList.Clear();

                var elements = deviceEngine.GetModulesFromMotionEngine(stationID, "AxisPosMove");
                var ioInElements = deviceEngine.GetModulesFromMotionEngine(stationID, "GetIO");
                var ioOutElements = deviceEngine.GetModulesFromMotionEngine(stationID, "SetIO");

                if (elements.Count() <= 0 && ioInElements.Count() <= 0 && ioOutElements.Count() <= 0)
                {
                    LoadDatas();
                    throw new FriendlyException($"当前工站不存在点位运动和IO操作模块!");
                }
                else
                {
                    if (elements.Count() > 0)
                    {
                        foreach (var item in elements)
                        {
                            var list = item.GetType().GetProperty("Values").GetValue(item) as List<AxisPosItem>;
                            var axisPosList = list == null ? new List<AxisPosition>() : list.Select(s => s.AxisPostion).ToList();
                            var model = new MotionPosGroupModel(Guid.Parse(item.GetType().GetProperty("ID").GetValue(item) as string),
                                                                item.GetType().GetProperty("Name").GetValue(item) as string,
                                                                axisPosList);
                            if (!PosGroups.Contains(model))
                            {
                                PosGroups.Add(model);
                            }
                        }
                    }

                    if (ioInElements.Count() > 0)
                    {
                        foreach (var item in ioInElements)
                        {
                            var vdevice = item.GetType().GetProperty("Values").GetValue(item) as VDevice;
                            VIO vio = deviceEngine.GetVirtualByID(vdevice.DeviceID) as VIO;

                            if (vio != null)
                            {
                                IoInList.Add(vio);
                            }
                        }
                    }

                    if (ioOutElements.Count() > 0)
                    {
                        foreach (var item in ioOutElements)
                        {
                            var vdevice = item.GetType().GetProperty("Values").GetValue(item) as VDevice;
                            var vio = deviceEngine.GetVirtualByID(vdevice.DeviceID) as VIO;

                            if (vio != null)
                            {
                                IoOutList.Add(vio);
                            }
                        }
                    }

                    LoadDatas(SearchIOText.Trim());
                }
            }
        }));

        /// <summary>
        /// 双击添加
        /// </summary>
        //private DelegateCommand<object> _deleteAxisCommand;
        //public DelegateCommand<object> DeleteAxisCommand => _deleteAxisCommand ?? (_deleteAxisCommand = new DelegateCommand<object>((obj) =>
        //{
        //    var axis = obj as AxisModel;

        //    // 1.添加到记录中
        //    SubAxisList.Remove(axis);

        //    if (Current != null)
        //    {
        //        // 2.将轴列表从集合中删除
        //        Current.RemovePos(axis.Tag);
        //    }

        //    LoadCanUseAxis();
        //}));


        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _moveCommand;
        public DelegateCommand<object> MoveCommand => _moveCommand ?? (_moveCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as MotionPosGroupModel;
            if (model != null)
            {
                VAxisPos vPos = new VAxisPos();
                foreach (var item in model.Tag)
                {
                    vPos.Add(new AxisPosItem()
                    {
                        PosKey = Guid.NewGuid(),
                        AxisPostion = item,
                        MovePriority = item.MovePriority,
                        Acc = item.Axis.Acc,
                        Dec = item.Axis.Dec,
                        Speed = item.Axis.MoveSpeed,
                        MoveMode = MoveMode.Abs,
                    });
                }

                Task.Run(() =>
                {
                    // 开始运动
                    vPos.MovePostion(deviceEngine);
                });
            }
        }));

        /// <summary>
        /// 更新
        /// </summary>
        private DelegateCommand<object> _updateCommand;
        public DelegateCommand<object> UpdateCommand => _updateCommand ?? (_updateCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as MotionPosGroupModel;
            if (model != null)
            {
                dialogService.ShowConfirm($"点位:{model.Name}，确认将示教点更新为实时点位?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        // 将当前的实时点位更新为示教点

                        if (SubAxisList.Count() > 0)
                        {
                            foreach (var item in SubAxisList)
                            {
                                var axisPos = Current.Tag.FirstOrDefault(u => u.AxisNo == item.AxisNo);
                                if (axisPos != null)
                                {
                                    axisPos.Position = item.CurrentPos;
                                    deviceEngine.UpdatePostion(axisPos, item.CurrentPos);

                                    // 代表有更新
                                    deviceEngine.IsNeedSave = true;
                                }
                            }
                        }
                    }
                });
            }
        }));


        /// <summary>
        /// 搜索命名
        /// </summary>
        //private DelegateCommand<object> _removeCommand;
        //public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        //{
        //    var model = obj as PosGroupModel;
        //    if (model != null)
        //    {
        //        dialogService.ShowConfirm($"确认删除点位:{model.Name}", r =>
        //        {
        //            if (r.Result == ButtonResult.OK)
        //            {
        //                deviceEngine.RemovePosGroup(model.Name);
        //                LoadDevices();
        //            }
        //        });
        //    }
        //}));

        /// <summary>
        /// 移除
        /// </summary>
        public override void RemoveItem()
        {
            base.RemoveItem();
        }

        /// <summary>
        /// 清理错误信息
        /// </summary>
        private DelegateCommand<AxisModel> _clearErrorCommand;
        public DelegateCommand<AxisModel> ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            // 显示对象
            axis.Tag.ResetStatus();
        }));

        /// <summary>
        /// 多轴移动
        /// </summary>
        private DelegateCommand<string> _axisCommand;
        public DelegateCommand<string> AxisCommand => _axisCommand ?? (_axisCommand = new DelegateCommand<string>((str) =>
        {
            // 统一功能
            foreach (var item in SubAxisList)
            {
                if (str == "ClearError")
                {
                    item.Tag.ResetStatus();
                }
                else if (str == "ServOn")
                {
                    item.Tag.ServOn(true);
                }
                else if (str == "ServOff")
                {
                    item.Tag.ServOn(false);
                }
                else if (str == "Stop")
                {
                    item.Tag.Stop();
                }
            }

            //// 如果示教，就将数据更新到点位中
            //if (str == "Teach")
            //{
            //    if (Current == null)
            //    {
            //        _dialogService.ShowTeachPositionDialog(0, "", true, r =>
            //        {
            //            if (r.Result == ButtonResult.OK)
            //            {
            //                if (r.Parameters.TryGetValue<string>("Name", out var name))
            //                {
            //                    deviceEngine.TeachPosGroup(name, SubAxisList.Select(u => u.Tag).ToArray());
            //                    //LoadDevices();
            //                }
            //            }
            //        });
            //    }
            //}
        }));

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            Enter();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            Leave();
        }

        public override void Enter()
        {
            if (whileToken != null)
            {
                whileToken?.Cancel();
                Thread.Sleep(300);
            }

            whileToken = new System.Threading.CancellationTokenSource();

            #region IOCardEventAndFunction
            StartMonitor();
            #endregion

        }

        public override void Leave()
        {
            whileToken?.Cancel();
            Thread.Sleep(200);
            whileToken = null;

            #region IOCardEventAndFunction
            StopMonitor();
            #endregion
        }


        #region IOCardEventAndFunction
        /// <summary>
        /// 暂停
        /// </summary>
        private ManualResetEventSlim pauseReset = new ManualResetEventSlim(true);

        /// <summary>
        /// 按钮切换
        /// </summary>
        //private DelegateCommand<string> _changeCardCommand;
        //public DelegateCommand<string> ChangeCardCommand => _changeCardCommand ?? (_changeCardCommand = new DelegateCommand<string>((card) =>
        //{
        //    CurrentCard = card;

        //    // 页面刷新
        //    LoadDatas();
        //}));

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand _searchIOCommand;
        public DelegateCommand SearchIOCommand => _searchIOCommand ?? (_searchIOCommand = new DelegateCommand(() =>
        {
            LoadDatas(SearchIOText.Trim());
        }));

        /// <summary>
        /// 按钮切换
        /// </summary>
        //private DelegateCommand<object> _loadedCommand;
        //public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        //{
        //    var args = obj as RoutedEventArgs;
        //    if (args != null && args.OriginalSource is ItemsControl ctrl)
        //    {
        //        CalcItemsSize(new Size(ctrl.ActualWidth, ctrl.ActualHeight));
        //    }

        //    LoadDatas();
        //}));

        /// <summary>
        /// 按钮切换
        /// </summary>
        //private DelegateCommand<object> _sizeChangedCommand;
        //public DelegateCommand<object> SizeChangedCommand => _sizeChangedCommand ?? (_sizeChangedCommand = new DelegateCommand<object>((obj) =>
        //{
        //    var sizeChanged = obj as SizeChangedEventArgs;
        //    if (sizeChanged != null)
        //    {
        //        var size = sizeChanged.NewSize;

        //        CalcItemsSize(size);
        //        LoadDatas();
        //    }
        //}));


        //private void CalcItemsSize(Size size)
        //{
        //    // 计算页面能够容纳的IO数量
        //    int col = (int)(size.Width / 300);
        //    int row = (int)(size.Height / 40);
        //}

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
                _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
                {
                    pauseReset.Reset();

                    IoInDatas?.Clear();

                    IoOutDatas?.Clear();

                    // IO界面刷新
                    pauseReset.Set();
                });
            });
        }

        protected override void ModeChanged(DeviceMode mode)
        {
            base.ModeChanged(mode);

            _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                SetValueBtnName = IsReal ? LangProvider.GetLang("PutValue") : LangProvider.GetLang("SimulationValue");

                foreach (var item in IoInDatas)
                {
                    item.DigitalValue = item.Tag.GetDigitalIn();
                    // 如果开始暂停了，那么直接中断循环
                    if (pauseReset != null && !pauseReset.IsSet)
                    {
                        break;
                    }
                }

                foreach (var item in IoOutDatas)
                {
                    item.DigitalValue = item.Tag.GetDigitalOut();

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
            _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                pauseReset.Reset();

                IoInDatas?.Clear();
                IoOutDatas?.Clear();

                var inList = IoInList;
                var outList = IoOutList;
                if (!string.IsNullOrEmpty(key))
                {
                    inList = inList.Where(u => u.Name.Contains(key)).ToList();
                    outList = outList.Where(u => u.Name.Contains(key)).ToList();
                }

                IoInDatas.AddRange(inList.Select(u => new IOModel(u)).ToList());
                IoOutDatas.AddRange(outList.Select(u => new IOModel(u)).ToList());

                // IO界面刷新
                pauseReset.Set();
            });
        }

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

        #endregion


    }
}