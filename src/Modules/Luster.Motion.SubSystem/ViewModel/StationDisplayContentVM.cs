using Luster.Control.Wpf.Motion.CoverFlow;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class StationDisplayContentVM : MotionVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _engine;

        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        private readonly Uri ImgURI = new Uri(@"Pack://application:,,,/Luster.Motion.Assests;component/Images/station.png");

        private Dispatcher _dispatcher;

        private IMotionController _motionController;

        public StationDisplayContentVM(ICommonBus commonBus, Dispatcher dispatcher, IMotionEngine motionEngine, IDialogService dialogService, IMotionController motionController) : base(commonBus)
        {
            InitListColor();
            Stations = new ObservableCollection<StationModel>();
            workingStationsList = new List<IMotionModule>();
            _engine = motionEngine;
            _dialogService = dialogService;
            _dispatcher = dispatcher;
            _motionController = motionController;

            //motionEngine.ProLoadedEvent += MotionEngine_ProLoadedEvent;
            //motionEngine.ProUnloadedEvent += MotionEngine_ProUnloadedEvent;
            //_motionController.HomeEvent += _motionController_StartHomeEvent;
        }

        /// <summary>
        /// 图像集合
        /// </summary>
        private ObservableCollection<ImageModel> _imageDatas;
        public ObservableCollection<ImageModel> ImageDatas
        {
            get { return _imageDatas; }
            set { SetProperty(ref _imageDatas, value); }
        }

        /// <summary>
        /// 工站集
        /// </summary>
        private ObservableCollection<StationModel> _stations;
        public ObservableCollection<StationModel> Stations
        {
            get { return _stations; }
            set { SetProperty(ref _stations, value); }
        }

        /// <summary>
        /// 工站颜色
        /// </summary>
        private SolidColorBrush _borderColor;
        public SolidColorBrush BorderColor
        {
            get { return _borderColor; }
            set { SetProperty(ref _borderColor, value); }
        }

        private List<IMotionModule> workingStationsList;

        private List<SolidColorBrush> listColor;

        /// <summary>
        /// 颜色顺序
        /// </summary>
        private int ColorIndex = 0;

        /// <summary>
        /// 获取工站
        /// </summary>
        private void BuildStation()
        {
            List<IMotionModule> listStation = _engine.GetStations();
            List<IMotionModule> list = listStation.Where(t => (t.TaskFunction as IStation).Visible == true).ToList();
            ProcessStation(list);
        }

        private void InitListColor()
        {
            listColor = new List<SolidColorBrush>();
            listColor.Add(Brushes.Red);
            listColor.Add(Brushes.Green);
            listColor.Add(Brushes.Yellow);
            listColor.Add(Brushes.MediumPurple);
            listColor.Add(Brushes.Blue);

        }

        private void ProcessStation(List<IMotionModule> listStation)
        {
            if (listStation != null)
            {
                workingStationsList = listStation.OrderBy(t => t.Sort).ToList();
                _dispatcher.Invoke(() =>
                {
                    Stations.Clear();
                    for (int i = 0; i < workingStationsList.Count; i++)
                    {
                        var model = new StationModel()
                        {
                            Name = workingStationsList[i].Alias,
                            IsSelected = workingStationsList[i].IsSelected,
                            Icon = workingStationsList[i].Icon,
                            IsEnabled = true,
                            BackColor = Brushes.Black
                        };
                        if (i != workingStationsList.Count - 1)
                        {
                            model.IsLastStation = true;
                        }
                        else
                        {
                            model.IsLastStation = false;
                        }
                        Stations.Add(model);
                    }
                });
            }
        }

        /// <summary>
        /// 工站出料
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MotionEngine_ProUnloadedEvent(IMotionModule arg1, DataStruct.DataModels.StationResult arg2, Dictionary<string, object> arg3)
        {
            _dispatcher.Invoke(() =>
            {
                if (coverFlowCtrl != null)
                {
                    Brush OutStationColor = coverFlowCtrl.Remove(arg2.ProCode);
                    if (Stations.Any(u => u.Name == arg1.Alias))
                    {
                        var addStation = Stations.FirstOrDefault(u => u.Name== arg1.Alias);
                        addStation.IsSelected = true;
                        addStation.BackColor = (SolidColorBrush)OutStationColor;

                    }
                }
            });
        }

        /// <summary>
        /// 工站入料
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="obj"></param>
        private void MotionEngine_ProLoadedEvent(IMotionModule arg1, DataStruct.DataModels.StationResult obj)
        {
            _dispatcher.Invoke(() =>
            {
                if (coverFlowCtrl != null)
                {
                    coverFlowCtrl.AddItem(obj.ProCode, listColor[ColorIndex % 5], ImgURI);
                    BorderColor = listColor[ColorIndex % 5];
                }
            });
            if (Stations.Any(u => u.Name == arg1.Station.Alias))
            {
                var addStation = Stations.FirstOrDefault(u => u.Name== arg1.Station.Alias);
                addStation.IsSelected = true;
                addStation.BackColor = listColor[ColorIndex % 5];
            }
            ColorIndex++;
        }

        private void _motionController_StartHomeEvent()
        {

            _dispatcher.Invoke(() =>
            {
                if (coverFlowCtrl != null)
                {
                    coverFlowCtrl.Clear();
                }
            });

        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                BuildStation();
            });
        }

        /// <summary>
        /// 设置工站是否显示
        /// </summary>
        private DelegateCommand _setStationCommand;
        public DelegateCommand SetStationCommand => _setStationCommand ?? (_setStationCommand = new DelegateCommand(() =>
        {
            _dialogService.ShowStationSet((r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("Stations", out ObservableCollection<IMotionModule> liststation))
                    {
                        List<IMotionModule> list = liststation.Where(t => (t.TaskFunction as IStation).Visible == true).ToList();
                        ProcessStation(list);
                        commonBus.OnSaveRecipe();

                    }
                }
            });
        }));

        #region 图片显示
        private CoverFlowCtrl coverFlowCtrl;
        /// <summary>
        /// 设置工站是否显示
        /// </summary>
        private DelegateCommand<object> _flowLoadedCommand;
        public DelegateCommand<object> FlowLoadedCommand => _flowLoadedCommand ?? (_flowLoadedCommand = new DelegateCommand<object>((obj) =>
        {
            System.Windows.RoutedEventArgs args = obj as System.Windows.RoutedEventArgs;
            if (args != null)
            {
                coverFlowCtrl = args.Source as CoverFlowCtrl;
            }
        }));
        #endregion
    }
}
