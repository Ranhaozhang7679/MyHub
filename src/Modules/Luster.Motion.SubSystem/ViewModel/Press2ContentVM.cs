using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Luster.TaskFlow.Motion.Logic;
using System.Timers;
using System.Windows;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.Motion.DataStruct;
using TaiKeCommon;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using Luster.SimDevice.SubSystem.Events;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class Press2ContentVM : MotionVM, IDockContent
    {

        private ICommonBus _commonBus;

        private IDialogService _dialogService;

        private IMotionController _motionController;

        private IMotionEngine _motionEngine;

        // UI线程
        private Dispatcher _dispatcher;

        /// <summary>
        /// 是否显示
        /// </summary>
        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set => SetProperty(ref _isVisible, value);
        }

        /// <summary>
        /// 启动状态按钮不可点击
        /// </summary>
        private bool _btnEnable = true;
        public bool BtnEnable
        {
            get { return _btnEnable; }
            set { SetProperty(ref _btnEnable, value); }
        }

        // 界面的控件集合，需要传递给扭力类使用
        object[] screwControls = null;

        public Press2ContentVM()  
        {


        }

        public Press2ContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController motionController, Dispatcher dispatcher, IMotionController mController, IMotionEngine motionEngine) : base(commonBus)
        {
            _dialogService = dialogService;
            _commonBus = commonBus;
            _motionController = motionController;
            _motionEngine = motionEngine;
            _dispatcher = dispatcher;
             
        }
        TimePress driver;
        string name = "";

        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<SysOperationBtnEvent>().Subscribe((btnobj) =>
            {
                //if (btnobj == DataStruct.Enums.SystemOperation.Start)
                //{
                //    BtnEnable = false;
                //}
                //else
                //{
                //    BtnEnable = true;
                //}
            });

            bus.GetEvent<ProjectChangeEvent>().Subscribe((mapdatas) =>
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (driver != null)
                    {
                        driver.Dispose();
                    }
                }));
            });
           bus.GetEvent<CloseEvent>().Subscribe(() =>
            {
                if (driver != null)
                {
                    driver.Dispose();
                }
            });

            bus.GetEvent<PressRegisterEvent>().Subscribe(pressDriver =>  
            {
                FrameworkElement parent_press_time = (FrameworkElement)screwControls[0]; 
                CartesianChart chartPressTime = (CartesianChart)screwControls[1];
                 
                if (!pressDriver.Name.Contains("2"))
                {
                    return;
                }
                name = pressDriver.Name;
                driver = (TimePress)pressDriver.Driver;

                driver.SetChartControl(_dispatcher, chartPressTime, parent_press_time); 
            });

 
        }




        /// <summary>
        /// 设置Chart
        /// </summary>
        private DelegateCommand _setCommand;
        public DelegateCommand SetCommand => _setCommand ?? (_setCommand = new DelegateCommand(() =>
        {

        }));

        private DelegateCommand<object> _getChartControls;
        public DelegateCommand<object> GetChartControls => _getChartControls ?? (_getChartControls = new DelegateCommand<object>((items) =>
        {
            //List<object>

            screwControls = (object[])items;
        }));

        #region DockContent
        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "PressForm2";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "Press2Content";
        #endregion

    }
}
