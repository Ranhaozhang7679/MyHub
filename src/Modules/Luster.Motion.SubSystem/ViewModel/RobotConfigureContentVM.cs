using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class RobotConfigureContentVM : MotionVM
    {
        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;
        private IMotionController _mController;


        // 设备引擎
        private IDeviceEngine _deviceEngine;

        private Dispatcher _dispatcher;

        private IMotionEngine _engine;

        /// <summary>
        /// 机器人状态集合
        /// </summary>
        private ObservableCollection<RobotConfigModel> _listRobotStatus;
        public ObservableCollection<RobotConfigModel> ListRobotStatus
        {
            get { return _listRobotStatus; }
            set { SetProperty(ref _listRobotStatus, value); }
        }

        /// <summary>
        /// 机器人命令集合
        /// </summary>
        private ObservableCollection<RobotConfigModel> _listRobotCommands;
        public ObservableCollection<RobotConfigModel> ListRobotCommands
        {
            get { 
                 if(_listRobotCommands==null)
                {
                    //SystemConfig sysConfig=new SystemConfig();
                    //string sysConfigPath = Path.Combine(Path.GetDirectoryName(commonBus.ProjInfo.FullName), "Config", "SystemConfig.xml");
                    //sysConfig.LoadSysConfig(sysConfigPath);
                    //_listRobotCommands=new ObservableCollection<RobotModel>()
                    //{
                    //    //new RobotModel(){Name="RobotStart",SelectDevice=(sysConfig.RobotStart==null ?"":sysConfig.RobotStart.Name)},
                    //};
                }
                
                
                
                return _listRobotCommands; }
            set { SetProperty(ref _listRobotCommands, value); }
        }


        /// <summary>
        /// 机器人状态集合
        /// </summary>
        private ObservableCollection<RobotConfigModel2> _listRobotStatus2;
        public ObservableCollection<RobotConfigModel2> ListRobotStatus2
        {
            get { return _listRobotStatus2; }
            set { SetProperty(ref _listRobotStatus2, value); }
        }

        /// <summary>
        /// 机器人命令集合
        /// </summary>
        private ObservableCollection<RobotConfigModel2> _listRobotCommands2;
        public ObservableCollection<RobotConfigModel2> ListRobotCommands2
        {
            get
            {
                if (_listRobotCommands2 == null)
                {
                    //SystemConfig sysConfig=new SystemConfig();
                    //string sysConfigPath = Path.Combine(Path.GetDirectoryName(commonBus.ProjInfo.FullName), "Config", "SystemConfig.xml");
                    //sysConfig.LoadSysConfig(sysConfigPath);
                    //_listRobotCommands=new ObservableCollection<RobotModel>()
                    //{
                    //    //new RobotModel(){Name="RobotStart",SelectDevice=(sysConfig.RobotStart==null ?"":sysConfig.RobotStart.Name)},
                    //};
                }
                return _listRobotCommands2;
            }
            set { SetProperty(ref _listRobotCommands2, value); }
        }


        public RobotConfigureContentVM(ICommonBus _commonBus,IDialogService dialogService,  IMotionController mControler ,IDeviceEngine deviceEngine, 
               Dispatcher dispatcher,IMotionEngine engine) : base(_commonBus)
        {
            _dialogService = dialogService;
            _mController = mControler;
            _deviceEngine = deviceEngine;
            _dispatcher = dispatcher;
            _engine = engine;
            BuildRobotDIDO();

        }


        /// <summary>
        /// 添加机器人状态
        /// </summary>
        private DelegateCommand _addRobotStatusCommand;
        public DelegateCommand AddRobotStatusCommand => _addRobotStatusCommand ?? (_addRobotStatusCommand = new DelegateCommand(() =>
        {
            //_dialogService.

        }
        ));



        private void BuildRobotDIDO()
        {
            if (commonBus.ProjInfo != null && !string.IsNullOrEmpty(commonBus.ProjInfo.ProjPath))
            {
                string sysConfig = Path.Combine(Path.GetDirectoryName(commonBus.ProjInfo.FullName), "Config", "SystemConfig.xml");
                RobotConfigModel.SysConfigPath = sysConfig;
                RobotConfigModel2.SysConfigPath = sysConfig;
                ListRobotStatus = new ObservableCollection<RobotConfigModel>();
                ListRobotStatus.AddRange(RobotConfigModel.RobotDICollections);

                ListRobotCommands = new ObservableCollection<RobotConfigModel>();
                ListRobotCommands.AddRange(RobotConfigModel.RobotDOCollections);

                ListRobotStatus2 = new ObservableCollection<RobotConfigModel2>();
                ListRobotStatus2.AddRange(RobotConfigModel2.RobotDICollections);

                ListRobotCommands2= new ObservableCollection<RobotConfigModel2>();
                ListRobotCommands2.AddRange(RobotConfigModel2.RobotDOCollections);
            }
        }

        /// <summary>
        /// 编辑操作按钮
        /// </summary>
        private DelegateCommand<object> _editOperateCommand;
        public DelegateCommand<object> EditOPerateCommand=> _editOperateCommand ?? (_editOperateCommand= new DelegateCommand<object>((o) =>
        {

            RobotConfigModel model = (RobotConfigModel)o;
            ParameterAttribute args = new ParameterAttribute(Luster.Motion.Assests.Langs.LangProvider.GetLang("Device"), 0);
            args.EditorType = typeof(VIO);
            _dialogService.ShowDeviceConfig(args, "", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("Device", out VIO vDevice))
                    {
                        model.SelectDevice = vDevice.Name;
                        SetSelectDevice(model, vDevice);
                    }
                }
                if (r.Result == ButtonResult.Cancel)
                {
                    CancelSelectDevice(model);
                }
                commonBus.IsNeedSave = true;
            });
        }));

        /// <summary>
        /// 编辑操作按钮
        /// </summary>
        private DelegateCommand<object> _editOperateCommand2;
        public DelegateCommand<object> EditOPerateCommand2 => _editOperateCommand2 ?? (_editOperateCommand2 = new DelegateCommand<object>((o) =>
        {

            RobotConfigModel2 model = (RobotConfigModel2)o;
            ParameterAttribute args = new ParameterAttribute(Luster.Motion.Assests.Langs.LangProvider.GetLang("Device"), 0);
            args.EditorType = typeof(VIO);
            _dialogService.ShowDeviceConfig(args, "", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("Device", out VIO vDevice))
                    {
                        model.SelectDevice = vDevice.Name;
                        SetSelectDevice2(model, vDevice);
                    }
                }
                if (r.Result == ButtonResult.Cancel)
                {
                    CancelSelectDevice2(model);
                }
                commonBus.IsNeedSave = true;
            });
        }));

        /// <summary>
        /// 选中设备
        /// </summary>
        /// <param name="model"></param>
        /// <param name="vDevice"></param>
        private void SetSelectDevice(RobotConfigModel model, VIO vDevice)
        {
            model.SetSelectBtnDevice(model);
            switch (model.Name)
            {
                case "Start":
                    _mController.SysConfig.RobotStart = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Pause":
                    _mController.SysConfig.RobotPause = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Stop":
                    _mController.SysConfig.RobotStop = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Reset":
                    _mController.SysConfig.RobotReset = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "ClearFault":
                    _mController.SysConfig.RobotClearFault = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Running":
                    _mController.SysConfig.RobotRuning = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Paused":
                    _mController.SysConfig.RobotPaused = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Idle":
                    _mController.SysConfig.RobotIdle = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Error":
                    _mController.SysConfig.RobotError = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
            }
        }

        /// <summary>
        /// 取消选中设备
        /// </summary>
        /// <param name="model"></param>
        private void CancelSelectDevice(RobotConfigModel model)
        {
            model.CancelSelectBtnDevice(model);
            switch (model.Name)
            {
                case "Start":
                    _mController.SysConfig.RobotStart = null;
                    break;
                case "Pause":
                    _mController.SysConfig.RobotPause =null;
                    break;
                case "Stop":
                    _mController.SysConfig.RobotStop = null;
                    break;
                case "Reset":
                    _mController.SysConfig.RobotReset = null ;
                    break;
                case "ClearFault":
                    _mController.SysConfig.RobotClearFault = null;
                    break;
                case "Running":
                    _mController.SysConfig.RobotRuning = null;
                    break;
                case "Paused":
                    _mController.SysConfig.RobotPaused = null;
                    break;
                case "Idle":
                    _mController.SysConfig.RobotIdle = null;
                    break;
                case "Error":
                    _mController.SysConfig.RobotError = null;
                    break;
            }
        }

        private void SetSelectDevice2(RobotConfigModel2 model, VIO vDevice)
        {
            model.SetSelectBtnDevice(model);
            switch (model.Name)
            {
                case "Start":
                    _mController.SysConfig.RobotStart2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Pause":
                    _mController.SysConfig.RobotPause2= new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Stop":
                    _mController.SysConfig.RobotStop2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Reset":
                    _mController.SysConfig.RobotReset2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "ClearFault":
                    _mController.SysConfig.RobotClearFault2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Running":
                    _mController.SysConfig.RobotRuning2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Paused":
                    _mController.SysConfig.RobotPaused2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Idle":
                    _mController.SysConfig.RobotIdle2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Error":
                    _mController.SysConfig.RobotError2 = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
            }
        }

        /// <summary>
        /// 取消选中设备
        /// </summary>
        /// <param name="model"></param>
        private void CancelSelectDevice2(RobotConfigModel2 model)
        {
            model.CancelSelectBtnDevice(model);
            switch (model.Name)
            {
                case "Start":
                    _mController.SysConfig.RobotStart2 = null;
                    break;
                case "Pause":
                    _mController.SysConfig.RobotPause2 = null;
                    break;
                case "Stop":
                    _mController.SysConfig.RobotStop2 = null;
                    break;
                case "Reset":
                    _mController.SysConfig.RobotReset2 = null;
                    break;
                case "ClearFault":
                    _mController.SysConfig.RobotClearFault2 = null;
                    break;
                case "Running":
                    _mController.SysConfig.RobotRuning2 = null;
                    break;
                case "Paused":
                    _mController.SysConfig.RobotPaused2 = null;
                    break;
                case "Idle":
                    _mController.SysConfig.RobotIdle2 = null;
                    break;
                case "Error":
                    _mController.SysConfig.RobotError2 = null;
                    break;
            }
        }



    }
}
