using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class LightCurtainDialogVM : MotionDialogVM
    {
        private IDialogService _dialogService;
        private IDeviceEngine _deviceEngine;
        private IMotionEngine _engine;
        private IMotionController _motionController;
        public LightCurtainDialogVM(IDialogService dialogService,IDeviceEngine deviceEngine, IMotionEngine engine)
        {
            _dialogService = dialogService;
            _deviceEngine = deviceEngine;
            _engine= engine;
            BuildGlobals();
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value); }
        }

        /// <summary>
        /// 设备列表
        /// </summary>
        private List<IVirtualDevice> _deviceList;
        public List<IVirtualDevice> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }


        private List<GlobalModel> _varDatas;
        public List<GlobalModel> VarDatas
        {
            get { return _varDatas; }
            set { SetProperty(ref _varDatas, value); }
        }

        private VDevice _selectDevice;
        public VDevice SelectDevice
        {
            get { return _selectDevice; }
            set { SetProperty(ref _selectDevice, value); }
        }

        private string _selectGlobal;
        public string SelectGlobal
        {
            get { return _selectGlobal; }
            set { SetProperty(ref _selectGlobal, value); }
        }

        private string _globalKey;
        public string GlobalKey
        {
            get { return _globalKey; }
            set { SetProperty(ref _globalKey, value); }
        }

        private DelegateCommand<object> _deviceChangeCommand;
        public DelegateCommand<object> DeviceChangeCommand => _deviceChangeCommand ?? (_deviceChangeCommand = new DelegateCommand<object>((o) =>
        {
            if (o != null)
            {
                var device = o as SelectionChangedEventArgs;
                var vdevice= device.AddedItems[0] as IVirtualDevice;
                DeviceName = vdevice.Name;
                SelectDevice = new VDevice() { DeviceID = vdevice.ID, Name = vdevice.Name };
            }
        
        }));

        private DelegateCommand<object> _globalChangeCommand;
        public DelegateCommand<object> GlobalChangeCommand => _globalChangeCommand ?? (_globalChangeCommand = new DelegateCommand<object>((o) =>
        {


            if (o != null)
            {
                var device = o as SelectionChangedEventArgs;
                var globalModel = device.AddedItems[0] as GlobalModel;
                SelectGlobal = globalModel.GlobalName;
                GlobalKey = globalModel.GlobalKey;
            }


        

        }));

        /// <summary>
        /// 构建变量
        /// </summary>
        private void BuildGlobals(string key = "")
        {
            VarDatas = new List<GlobalModel>();
            var gModule = GetGlobal();
            foreach (var item in gModule.Parameters)
            {
                if (item.Value.Type.Name == "Boolean")
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        var model = new GlobalModel()
                        {
                            GlobalName = item.Value.Alias,
                            GlobalKey=item.Key
                        };

                        VarDatas.Add(model);
                    }
                    else
                    {
                        if (item.Value.Alias.ToLower().Contains(key.ToLower()))
                        {
                            var model = new GlobalModel()
                            {
                                GlobalName = item.Value.Alias,
                                GlobalKey = item.Key
                            };
                            VarDatas.Add(model);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IMotionModule GetGlobal()
        {
            var id = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
            return _engine.Get(id);
        }


        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("VDevice", SelectDevice);
            result.Parameters.Add("VarGlobal", SelectGlobal);
            result.Parameters.Add("GlobalKey", GlobalKey);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
                FilterItems();
            }
            else
            {
                Title = string.Empty;
            }

        }

        private void FilterItems()
        {
            var EditorType = typeof(VIO);
            DeviceList = _deviceEngine.GetDevices(EditorType);
            
          
        }
    }
}
