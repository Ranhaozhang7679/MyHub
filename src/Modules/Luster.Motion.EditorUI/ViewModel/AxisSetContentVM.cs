#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       LogContentVM.cs
* 创建时间:       2022/8/7 15:33:38
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9b5d0f31-a0d4-4ea9-bbfd-4cf5313146b0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/7 15:33:38
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Control.Wpf.Motion.Controls;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 轴配置
    /// </summary>
    public class AxisSetContentVM : MotionPageVM
    {
        private Dispatcher _dispatcher;

        private IDeviceEngine _deviceEngine;

        /// <summary>
        /// 事件总线
        /// </summary>
        private FlowBus eventBus;

        private SubscriptionToken loadedToken;

        /// <summary>
        /// 控制器
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// 线程
        /// </summary>
        private CancellationTokenSource tokenS;

        public AxisSetContentVM(FlowBus _eventBus,
            ICommonBus commonBus,
            IDeviceEngine deviceEngine,
            Dispatcher dispatcher,
            IMotionController mController) : base(commonBus)
        {
            _dispatcher = dispatcher;
            eventBus = _eventBus;
            _mController = mController;
            // 轴节点
            AxisNodes = new List<LNode>();
            Directions = typeof(MoveDirection).EnumToDataSource();
            _deviceEngine = deviceEngine;
            Priorities = typeof(Priority).EnumToDataSource();
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            loadedToken = bus.GetEvent<ModuleLoadedEvent>().Subscribe(m =>
            {
                if (m.TaskFunction is IStation s)
                {
                    AxisNodes = eventBus.GetModuleAxis(s);
                    AxisGroup = s.Station;
                }
            });

            bus.GetEvent<OperationEvent>().Subscribe(p =>
            {
                // 系统运行中，如果有线程在刷新轴状态，就取消监控
                if (p.Dst == EngineStatus.Running)
                {
                    tokenS?.Cancel();
                }
            });
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var m = eventBus.GetCurrent();
            if (m.TaskFunction is IStation s)
            {
                AxisNodes = eventBus.GetModuleAxis(s);
                AxisGroup = s.Station;
            }
        }

        /// <summary>
        /// 运动方向
        /// </summary>
        private List<KeyValue> _directions;
        public List<KeyValue> Directions
        {
            get { return _directions; }
            set { SetProperty(ref _directions, value); }
        }

        public List<KeyValue> Priorities { get; set; }

        /// <summary>
        /// 当前运动方向
        /// </summary>
        private MoveDirection _moveDirection;
        public MoveDirection MoveDirection
        {
            get { return _moveDirection; }
            set { SetProperty(ref _moveDirection, value); }
        }

        /// <summary>
        /// 轴节点
        /// </summary>
        private List<LNode> _axisNodes;
        public List<LNode> AxisNodes
        {
            get { return _axisNodes; }
            set { SetProperty(ref _axisNodes, value); }
        }

        /// <summary>
        /// 速度百分比
        /// </summary>
        private string _axisGroup = Luster.Motion.Assests.Langs.LangProvider.GetLang("UnKnownStation");
        public string AxisGroup
        {
            get { return _axisGroup; }
            set { SetProperty(ref _axisGroup, value); }
        }

        /// <summary>
        /// 速度百分比
        /// </summary>
        private double _speedPercent;
        public double SpeedPercent
        {
            get { return _speedPercent; }
            set { SetProperty(ref _speedPercent, value); }
        }

        /// <summary>
        /// 当前轴
        /// </summary>
        private ObservableCollection<MultiAxisModel> _hasAxisDatas;
        public ObservableCollection<MultiAxisModel> HasAxisDatas
        {
            get { return _hasAxisDatas; }
            set { SetProperty(ref _hasAxisDatas, value); }
        }

        /// <summary>
        /// 多轴
        /// </summary>
        private VAxisM curVAxisM;

        /// <summary>
        /// 当前单轴
        /// </summary>
        private VAxis curVAxis;

        /// <summary>
        /// 是否多轴
        /// </summary>
        private bool isMAxis;
        public bool IsMAxis
        {
            get { return isMAxis; }
            set { SetProperty(ref isMAxis, value); }
        }

        /// <summary>
        /// 设备轴名称
        /// </summary>
        private string _axisName;
        public string AxisName
        {
            get { return _axisName; }
            set { SetProperty(ref _axisName, value); }
        }

        /// <summary>
        /// 选择对象
        /// </summary>
        public DelegateCommand<object> _selectCommand;
        public DelegateCommand<object> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<object>((obj) =>
        {
            tokenS?.Cancel();
            tokenS = new CancellationTokenSource();
            IsMAxis = false;
            SelectionChangedEventArgs args = obj as SelectionChangedEventArgs;
            if (args != null && args.AddedItems.Count > 0)
            {
                HasAxisDatas = new ObservableCollection<MultiAxisModel>();
                var node = args.AddedItems[0] as LNode;
                AxisName = node.Text;
                if (node.Tag1 is VAxisMDevice mAxis)
                {

                    IsMAxis = true;
                    curVAxisM = mAxis.GetVDevice<VAxisM>(_deviceEngine);
                    foreach (var item in mAxis.Items)
                    {
                        item.Axis = _deviceEngine.GetVirtualByID(item.AxisID) as VAxis;
                        var mAxisModel = new MultiAxisModel(item, tokenS);

                        SetSpeedPercent(item.Axis);

                        mAxisModel.AxisPropertyChanged -= MAxisModel_PropertyChanged;
                        mAxisModel.AxisPropertyChanged += MAxisModel_PropertyChanged;
                        HasAxisDatas.Add(mAxisModel);
                    }
                }
                else if (node.Tag1 is VAxisDevice axis)
                {
                    curVAxis = _deviceEngine.GetVirtualByID(axis.DeviceID) as VAxis;
                    SetSpeedPercent(curVAxis);

                    axis.Axis = curVAxis;
                    var mAxisModel = new MultiAxisModel(axis, tokenS);
                    mAxisModel.AxisPropertyChanged -= MAxisModel_PropertyChanged;
                    mAxisModel.AxisPropertyChanged += MAxisModel_PropertyChanged;
                    HasAxisDatas.Add(mAxisModel);
                }
                else if (node.Tag1 is VAxisPos vPos)
                {
                    IsMAxis = true;
                    curVAxisPos = vPos;
                    foreach (var item in vPos)
                    {
                        item.UpdatePosition(_deviceEngine);
                        var mAxisModel = new MultiAxisModel(item, tokenS);
                        //mAxisModel.AxisPropertyChanged -= MAxisModel_PropertyChanged;
                        //mAxisModel.AxisPropertyChanged += MAxisModel_PropertyChanged;
                        HasAxisDatas.Add(mAxisModel);
                    }
                }
            }
        }));

        /// <summary>
        /// 当前动作
        /// </summary>
        private VAxisPos curVAxisPos = null;

        /// <summary>
        /// 设置速度百分比
        /// </summary>
        /// <param name="vAxis">轴</param>
        private void SetSpeedPercent(VAxis vAxis)
        {
            SpeedPercent = vAxis.SpeedPercent * 100;
            if (SpeedPercent > 150)
            {
                SpeedPercent = 150;
            }
        }

        /// <summary>
        /// 参数变更，通知更新
        /// </summary>
        /// <param name="obj"></param>
        private void MAxisModel_PropertyChanged(string obj, object srcV, object value)
        {
            _mController.OnPropertyChanged(obj, srcV, value);
            _deviceEngine.IsNeedSave = true;
        }

        /// <summary>
        /// 轴运动
        /// </summary>
        public DelegateCommand<string> _axisCommand;
        public DelegateCommand<string> AxisCommand => _axisCommand ?? (_axisCommand = new DelegateCommand<string>((type) =>
        {
            if (type == "Move")
            {
                if (curVAxisM != null)
                {
                    var mData = NewAxisMDevice();

                    curVAxisM.Move(mData);
                }
                else if (curVAxisPos != null)
                {
                    curVAxisPos.MovePostion(_deviceEngine);
                }
            }
            else if (type == "JOG")
            {
                if (curVAxisM != null)
                {
                    var mData = NewAxisMDevice();
                    curVAxisM.Jog(mData);
                    curVAxisM.VStatus = VStatus.Idle;
                }
            }
            else if (type == "Stop")
            {
                if (curVAxisM != null)
                {
                    curVAxisM.Stop();
                }
                else if (curVAxis != null)
                {
                    curVAxis.Stop();
                }
                else if (curVAxisPos != null)
                {
                    curVAxisPos.Stop(_deviceEngine);
                }
            }
            else if (type == "Clear")
            {
                if (curVAxisM != null)
                {
                    foreach (var item in HasAxisDatas)
                    {
                        item.Current.ResetStatus();
                    }
                }
                else if (curVAxis != null)
                {
                    curVAxis.ResetStatus();
                }
            }
        }));

        /// <summary>
        /// 构造轴的参数配置
        /// </summary>
        /// <returns></returns>
        private VAxisMDevice NewAxisMDevice()
        {
            List<AxisItem> items = new List<AxisItem>();
            foreach (var item in HasAxisDatas)
            {
                AxisItem axisItem = new AxisItem()
                {
                    AxisID = item.AxisID,
                    Axis = item.Current,
                    MovePriority = item.Priority,
                    Position = item.Position,
                    Speed = item.Speed
                };

                items.Add(axisItem);
            }

            VAxisMDevice vAxisM = new VAxisMDevice() { DeviceID = curVAxis.ID, Name = curVAxis.Name, Items = items, MoveDirection = MoveDirection };

            return vAxisM;
        }

        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _changeSpeedCommand;
        public DelegateCommand<object> ChangeSpeedCommand => _changeSpeedCommand ?? (_changeSpeedCommand = new DelegateCommand<object>((obj) =>
        {
            RoutedPropertyChangedEventArgs<double> args = obj as RoutedPropertyChangedEventArgs<double>;

            var axis = _deviceEngine.GetVDevices<VAxis>();
            foreach (var item in axis)
            {
                item.SpeedPercent = Math.Round(args.NewValue / 100, 1);
            }
        }));

        /// <summary>
        /// 页面离开
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            tokenS?.Cancel();
        }
    }
}