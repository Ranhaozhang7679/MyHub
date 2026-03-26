using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class DIYAlarmContentVM:MotionPageVM
    {
        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        private IMotionController _mController;

        private IMotionEngine _mEngine;

        private Dispatcher _dispatcher;

        private ObservableCollection<DIY_Alarm> _diylarmList;
        public ObservableCollection<DIY_Alarm> DIYAlarmList
        {
            get { return _diylarmList; }
            set { SetProperty(ref _diylarmList, value); }
        }
        // <summary>
        /// 是否保存
        /// </summary>
        private bool _isSave = false;
        public bool IsSave
        {
            get { return _isSave; }
            set
            {
                SetProperty(ref _isSave, value);
                commonBus.IsNeedSave = value;
            }
        }

        public DIYAlarmContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController mController, IMotionEngine mEngine, Dispatcher Dispatcher) : base(commonBus)
        {
            _mController = mController;
            _dispatcher = Dispatcher;
            _dialogService = dialogService;
            _mEngine = mEngine;
            //PlcAddrList = new ObservableCollection<PlcAddr>();
            BuildAlarmList();
            InitData();

        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                InitData();
            });
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            if (commonBus.ProjInfo != null && !string.IsNullOrEmpty(commonBus.ProjInfo.ProjPath))
            {
                string sysConfig = Path.Combine(Path.GetDirectoryName(commonBus.ProjInfo.FullName), "Config", "SystemConfig.xml");
                _mController.SysConfig.LoadSysConfig(sysConfig);//保存配置
                //PlcName = _mController.SysConfig.PlcServer == null ? "" : _mController.SysConfig.PlcServer.Name;

                commonBus.IsNeedSave = false;
            }
        }

        private void BuildAlarmList()
        {
            DIYAlarmList = new ObservableCollection<DIY_Alarm>();
            var alarmList = _mController.SysConfig.DIYAlarmList;
            if (alarmList != null && alarmList.Count > 0)
            {
                foreach (var model in alarmList)
                {
                    DIYAlarmList.Add(new DIY_Alarm()
                    {
                        AlarmID = model.AlarmID,
                        AlarmContent = model.AlarmContent,
                        AlarmSolution = model.AlarmSolution
                    }); ;
                }
            }
        }


        /// <summary>
        /// 添加报警
        /// </summary>
        private DelegateCommand _addDIYAlarmCommand;
        public DelegateCommand AddDIYAlarmCommand => _addDIYAlarmCommand ?? (_addDIYAlarmCommand = new DelegateCommand(() =>
        {
            DIY_Alarm model = null;
            _dialogService.ShowAddDIYAlarm(model, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    DIY_Alarm alarmmodel = new DIY_Alarm();
                    if (r.Parameters.TryGetValue("AlarmID", out int AlarmID))
                    {
                        alarmmodel.AlarmID = AlarmID;
                    }
                    if (r.Parameters.TryGetValue("AlarmContent", out string AlarmContent))
                    {
                        alarmmodel.AlarmContent = AlarmContent;
                    }
                    if (r.Parameters.TryGetValue("AlarmSolution", out string AlarmSolution))
                    {
                        alarmmodel.AlarmSolution = AlarmSolution;
                    }

                    if (DIYAlarmList== null)
                    {
                        DIYAlarmList = new ObservableCollection<DIY_Alarm>();
                    }

                    DIYAlarmList.Add(alarmmodel);

                    CalcDIYAlarmList();
                }
            });
        }));

        /// <summary>
        /// 更新AlarmList
        /// </summary>
        private void CalcDIYAlarmList()
        {
            if (DIYAlarmList != null)
            {
                _mController.SysConfig.DIYAlarmList.Clear();
                _mController.SysConfig.DIYAlarmList.AddRange(DIYAlarmList);
                commonBus.IsNeedSave = true;
            }
        }

    }
}
