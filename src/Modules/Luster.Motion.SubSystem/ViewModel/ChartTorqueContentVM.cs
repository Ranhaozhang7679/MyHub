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
    public class ChartTorqueContentVM : MotionVM, IDockContent
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
        private bool _isVisible = false;
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

        object[] toeInControls = null;


        public ChartTorqueContentVM()
        {


        }

        public ChartTorqueContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController motionController, Dispatcher dispatcher, IMotionController mController, IMotionEngine motionEngine) : base(commonBus)
        {
            _dialogService = dialogService;
            _commonBus = commonBus;
            _motionController = motionController;
            _motionEngine = motionEngine;
            _dispatcher = dispatcher;

        }
        ElectricScrewDrivers driver;
        string name = "";

        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<SysOperationBtnEvent>().Subscribe((btnobj) =>
            {
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

            bus.GetEvent<TorqueRegisterEvent>().Subscribe(screwDriver =>
            {
                FrameworkElement parent_chart_time = (FrameworkElement)screwControls[0];
                CartesianChart chartTorqueTime = (CartesianChart)screwControls[1];
                TextBlock tbxTime_EC = (TextBlock)screwControls[2];
                TextBlock tbxTime_T1 = (TextBlock)screwControls[3];
                TextBlock tbxTime_T2 = (TextBlock)screwControls[4];
                TextBlock tbxTime_T3 = (TextBlock)screwControls[5];
                TextBlock tbxTime_Tt = (TextBlock)screwControls[6];
                TextBlock tbxTime_A1 = (TextBlock)screwControls[7];
                TextBlock tbxTime_A2 = (TextBlock)screwControls[8];
                TextBlock tbxTime_A3 = (TextBlock)screwControls[9];
                TextBlock tbxTime_SSL = (TextBlock)screwControls[10];
                TextBlock tbxTime_MaxAngle = (TextBlock)screwControls[11];
                TextBlock tbxTime_MaxTorque = (TextBlock)screwControls[12];
                TextBlock tbxTime_SeatingTorque = (TextBlock)screwControls[13];
                TextBlock tbxTime_ClampTorque = (TextBlock)screwControls[14];
                TextBlock tbxTime_PrevailingTorque = (TextBlock)screwControls[15];
                TextBlock tbxTime_MC = (TextBlock)screwControls[16];
                TextBlock tbxMax_Press = (TextBlock)screwControls[17]; 

                FrameworkElement parent_chart_angle = (FrameworkElement)screwControls[18];
                CartesianChart chartTorqueAngle = (CartesianChart)screwControls[19];
                TextBlock tbxAngle_EC = (TextBlock)screwControls[20];
                TextBlock tbxAngle_T1 = (TextBlock)screwControls[21];
                TextBlock tbxAngle_T2 = (TextBlock)screwControls[22];
                TextBlock tbxAngle_T3 = (TextBlock)screwControls[23];
                TextBlock tbxAngle_Tt = (TextBlock)screwControls[24];
                TextBlock tbxAngle_A1 = (TextBlock)screwControls[25];
                TextBlock tbxAngle_A2 = (TextBlock)screwControls[26];
                TextBlock tbxAngle_A3 = (TextBlock)screwControls[27];
                TextBlock tbxAngle_SSL = (TextBlock)screwControls[28];
                TextBlock tbxAngle_MaxAngle = (TextBlock)screwControls[29];
                TextBlock tbxAngle_MaxTorque = (TextBlock)screwControls[30];
                TextBlock tbxAngle_SeatingTorque = (TextBlock)screwControls[31];
                TextBlock tbxAngle_ClampTorque = (TextBlock)screwControls[32];
                TextBlock tbxAngle_PrevailingTorque = (TextBlock)screwControls[33];
                TextBlock tbxAngle_MC = (TextBlock)screwControls[34];

                TextBlock[] tbxs_t = new TextBlock[]
                {
                    tbxTime_EC ,
                    tbxTime_T1 ,
                    tbxTime_T2 ,
                    tbxTime_T3 ,
                    tbxTime_Tt ,
                    tbxTime_A1 ,
                    tbxTime_A2 ,
                    tbxTime_A3 ,
                    tbxTime_SSL,
                    tbxTime_MaxAngle,
                    tbxTime_MaxTorque,
                    tbxTime_SeatingTorque,
                    tbxTime_ClampTorque,
                    tbxTime_PrevailingTorque,
                    tbxTime_MC,
                    tbxMax_Press
                };
                TextBlock[] tbxs_a = new TextBlock[]
                {
                    tbxAngle_EC ,
                    tbxAngle_T1 ,
                    tbxAngle_T2 ,
                    tbxAngle_T3 ,
                    tbxAngle_Tt ,
                    tbxAngle_A1 ,
                    tbxAngle_A2 ,
                    tbxAngle_A3 ,
                    tbxAngle_SSL,
                    tbxAngle_MaxAngle,
                    tbxAngle_MaxTorque,
                    tbxAngle_SeatingTorque,
                    tbxAngle_ClampTorque,
                    tbxAngle_PrevailingTorque,
                    tbxAngle_MC
                };

                if (!screwDriver.Name.Contains("1"))
                {
                    return;
                }
                name = screwDriver.Name;
                driver = (ElectricScrewDrivers)screwDriver.Driver;

                driver.SetChartControl(_dispatcher, chartTorqueTime, tbxs_t, chartTorqueAngle, tbxs_a, parent_chart_time, parent_chart_angle);
            });

            bus.GetEvent<ToeinRegisterEvent>().Subscribe(screwDriver =>
            {
                FrameworkElement parent_chart_ToeinForce = (FrameworkElement)toeInControls[0];
                CartesianChart chartToeinForce = (CartesianChart)toeInControls[1];
                name = screwDriver.Name;
                driver = (ElectricScrewDrivers)screwDriver.Driver;
                driver.SetToeinChartControl(_dispatcher, chartToeinForce, parent_chart_ToeinForce);
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
            screwControls = (object[])items;
        }));



        private DelegateCommand<object> _getToeinChartControls;
        public DelegateCommand<object> GetToeinChartControls => _getToeinChartControls ?? (_getToeinChartControls = new DelegateCommand<object>((items) =>
        {
            toeInControls = (object[])items;
        }));

        #region DockContent
        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "TorqueForm";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "ChartTorqueContent";
        #endregion

    }
}
