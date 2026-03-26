using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.EditorUI;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Logic;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ProTestBottonContentVM : MotionVM, IDockContent
    {
        private IDeviceEngine _deviceEngine = null;
        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;
        /// <summary>
        /// 流程Bus
        /// </summary>
        private FlowBus flowBus;

        public class ButtonData
        {
            public string Content { get; set; }
        }

        private ObservableCollection<ButtonData> _buttons;

        public ObservableCollection<ButtonData> Buttons
        {
            get => _buttons;
            set
            {
                if (_buttons != value)
                {
                    _buttons = value;
                    OnPropertyChanged(nameof(Buttons));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public IDeviceEngine Engine { get; set; }

        public ProTestBottonContentVM()
        {
            _buttons = new ObservableCollection<ButtonData>();
        }

        public ProTestBottonContentVM(ICommonBus _commonBus, IDbManager dbManager,
            IMotionController motionController, IErrorManager errorManager, IDeviceEngine deviceEngine,
            Dispatcher dispatcher, IDialogService dialogService, HiveAPI _hiveApi, FlowBus _flowBus) : base(_commonBus)
        {
            _mController = motionController;
            _deviceEngine = deviceEngine;
            flowBus = _flowBus;
            InitGlobal();
        }


        private List<ButtonData> data = new List<ButtonData>();
        private void InitGlobal()
        {
            
            var stations = _mController.MotionEngine.GetStations();
            Buttons = new ObservableCollection<ButtonData>();
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i].TaskFunction.Alias == "TestStation")
                {
                    AddNewButton(stations[i].Alias);
                }
            }



        }

        public string Name => "TestBotton";

        public string RegionName { get; set; } = "ProTestBottonContent";

        public void AddNewButton(string data)
        {
            Buttons.Add(new ButtonData
            {
                Content = data
            });
        }

        public void OnButtonClicked(string buttonName)
        {
            if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
            {
                var stations = _mController.MotionEngine.GetStations();
                foreach (var stat in stations)
                {
                    if (stat.Alias == buttonName)
                    {
                        flowBus.OnRunOne(stat.ID);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "回零完成后方可运行测试流程",
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
            }
        }
    }
}
