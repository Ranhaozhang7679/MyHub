using LiveChartsCore.Defaults;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class FlowWaitContentVM : MotionVM, IDockContent
    {
        /// <summary>
        /// 运控引擎
        /// </summary>
        private IMotionEngine _mEngine;

        /// <summary>
        /// 机台确认按
        /// </summary>
        private ObservableCollection<FLowStation> _stations;
        public ObservableCollection<FLowStation> Stations
        {
            get { return _stations; }
            set
            {
                SetProperty(ref _stations, value);
            }
        }

        /// <summary>
        /// 空构造函数，用于DockContent实例构建
        /// </summary>
        public FlowWaitContentVM() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bus"></param>
        public FlowWaitContentVM(ICommonBus bus, IMotionEngine motionEngine) : base(bus)
        {
            _mEngine = motionEngine;

            InitStations();
        }

        /// <summary>
        /// 初始化按钮
        /// </summary>
        private void InitStations()
        {
            Stations = new ObservableCollection<FLowStation>();

            // 工站列表
            var stations = _mEngine.GetStations();
            foreach (var station in stations)
            {
                if (station.TaskFunction is IFreeStation f)
                {
                    if (f.UseLog)
                    {
                        Stations.Add(new FLowStation(f));
                    }
                }
            }
        }

        /// <summary>
        /// 事件注册
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                InitStations();
            });
        }

        /// <summary>
        /// 流程等
        /// </summary>
        public string Name => "FlowWait";

        public string RegionName { get; set; } = "FlowWaitContent";
    }

    /// <summary>
    /// 流程工站
    /// </summary>
    public class FLowStation : BindableBase
    {
        /// <summary>
        /// 工站名称
        /// </summary>
        private string _stationName = "";
        public string StationName
        {
            get { return _stationName; }
            set { SetProperty(ref _stationName, value); }
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        private string _moduleName = "";
        public string Module
        {
            get { return _moduleName; }
            set { SetProperty(ref _moduleName, value); }
        }

        /// <summary>
        /// 等待CT
        /// </summary>
        private int _ct;
        public int CT
        {
            get { return _ct; }
            set { SetProperty(ref _ct, value); }
        }

        /// <summary>
        /// 工站Tag
        /// </summary>
        public IStation Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="station">工站</param>
        public FLowStation(IStation station)
        {
            Tag = station;
            StationName = station.Station;
            station.RunningModuleChanged -= Station_RunningModuleChanged;
            station.RunningModuleChanged += Station_RunningModuleChanged;
        }

        /// <summary>
        /// 运行模块
        /// </summary>
        /// <param name="obj">运行中的模块</param>
        private void Station_RunningModuleChanged(Luster.TaskFlow.Motion.IMotionModule obj)
        {
            Module = obj.Alias;
        }
    }
}