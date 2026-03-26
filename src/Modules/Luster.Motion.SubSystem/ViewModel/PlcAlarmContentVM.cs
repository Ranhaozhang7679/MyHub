#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PlcAlarmContentVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       PlcAlarmContentVM.cs
* 创建时间:       2022/9/5 19:01:43
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      5962cbc8-b97c-4244-a494-ca32d6064d74
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/5 19:01:43
* 修 改 人:		  L05590
************************************************************************************/
#endregion

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
    public class PlcAlarmContentVM : MotionPageVM
    {
        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        private IMotionController _mController;

        private IMotionEngine _mEngine;

        private Dispatcher _dispatcher;


        /// <summary>
        /// EditEnable
        /// </summary>
        private bool _editEnable = true;
        public bool EditEnable
        {
            get { return _editEnable; }
            set
            {
                SetProperty(ref _editEnable, value);
            }
        }

        /// <summary>
        /// ListenEnable
        /// </summary>
        private bool _listenEnable = false;
        public bool ListenEnable
        {
            get { return _listenEnable; }
            set
            {
                SetProperty(ref _listenEnable, value);
            }
        }
        /// <summary>
        /// StartAddr
        /// </summary>
        private string _statusAddr;
        public string StatusAddr
        {
            get { return _statusAddr; }
            set
            {
                SetProperty(ref _statusAddr, value);
                _mController.SysConfig.StatusAddr = value;
            }
        }

        private string _idleStatusAddr;
        public string IdleStatusAddr
        {
            get { return _idleStatusAddr; }
            set
            {
                SetProperty(ref _idleStatusAddr, value);
                _mController.SysConfig.IdleStatusAddr = value;
            }
        }

        /// <summary>
        /// StartAddr
        /// </summary>
        private string _startAddr;
        public string StartAddr
        {
            get { return _startAddr; }
            set
            {
                SetProperty(ref _startAddr, value);
                _mController.SysConfig.StartAddr = value;
            }
        }

        /// <summary>
        /// PauseAddr
        /// </summary>
        private string _pauseAddr;
        public string PauseAddr
        {
            get { return _pauseAddr; }
            set
            {
                SetProperty(ref _pauseAddr, value);
                _mController.SysConfig.PauseAddr = value;
            }
        }

        /// <summary>
        /// HomeAddr
        /// </summary>
        private string _homeAddr;
        public string HomeAddr
        {
            get { return _homeAddr; }
            set
            {
                SetProperty(ref _homeAddr, value);
                _mController.SysConfig.HomeAddr = value;
            }
        }

        /// <summary>
        /// HomeAddr
        /// </summary>
        private string _autoRunAddr;
        public string AutoRunAddr
        {
            get { return _autoRunAddr; }
            set
            {
                SetProperty(ref _autoRunAddr, value);
                _mController.SysConfig.AutoRunAddr = value;
            }
        }

        /// <summary>
        /// HomeAddr
        /// </summary>
        private string _homeFinishAddr;
        public string HomeFinishAddr
        {
            get { return _homeFinishAddr; }
            set
            {
                SetProperty(ref _homeFinishAddr, value);
                _mController.SysConfig.HomeFinishAddr = value;
            }
        }

        /// <summary>
        /// RecoveryAddr
        /// </summary>
        private string _recoveryAddr;
        public string RecoveryAddr
        {
            get { return _recoveryAddr; }
            set
            {
                SetProperty(ref _recoveryAddr, value);
                _mController.SysConfig.RecoveryAddr = value;
            }
        }

        /// <summary>
        /// StopAddr
        /// </summary>
        private string _stopAddr;
        public string StopAddr
        {
            get { return _stopAddr; }
            set
            {
                SetProperty(ref _stopAddr, value);
                _mController.SysConfig.StopAddr = value;
            }
        }

        /// <summary>
        /// EmptyRunAddr
        /// </summary>
        private string _emptyRunAddr;
        public string EmptyRunAddr
        {
            get { return _emptyRunAddr; }
            set
            {
                SetProperty(ref _emptyRunAddr, value);
                _mController.SysConfig.EmptyRunAddr = value;
            }
        }

        /// <summary>
        /// plc服务器名称
        /// </summary>
        private string _plcName;
        public string PlcName
        {
            get { return _plcName; }
            set { SetProperty(ref _plcName, value); }
        }

        #region 上下游信号

        /// <summary>
        /// 回流线上游有料
        /// </summary>
        private string _PrevHaveAddr;
        public string PrevHaveAddr
        {
            get { return _PrevHaveAddr; }
            set
            {
                SetProperty(ref _PrevHaveAddr, value);
                _mController.SysConfig.SubPrevHaveAddr = value;
            }
        }

        /// <summary>
        /// 回流线本站要料
        /// </summary>
        private string _ThisGetAddr;
        public string ThisGetAddr
        {
            get { return _ThisGetAddr; }
            set
            {
                SetProperty(ref _ThisGetAddr, value);
                _mController.SysConfig.SubThisGetAddr = value;
            }
        }

        /// <summary>
        /// 回流线本站有料
        /// </summary>
        private string _ThisHaveAddr;
        public string ThisHaveAddr
        {
            get { return _ThisHaveAddr; }
            set
            {
                SetProperty(ref _ThisHaveAddr, value);
                _mController.SysConfig.SubThisHaveAddr = value;
            }
        }

        /// <summary>
        /// 回流线下游要料
        /// </summary>
        private string _NextGetAddr;
        public string NextGetAddr
        {
            get { return _NextGetAddr; }
            set
            {
                SetProperty(ref _NextGetAddr, value);
                _mController.SysConfig.SubNextGetAddr = value;
            }
        }

        /// <summary>
        /// 主流线前站有料
        /// </summary>
        private string _MainPrevHaveAddr;
        public string MainPrevHaveAddr
        {
            get { return _MainPrevHaveAddr; }
            set
            {
                SetProperty(ref _MainPrevHaveAddr, value);
                _mController.SysConfig.MainPrevHaveAddr = value;
            }
        }

        /// <summary>
        /// 主流线本站要料
        /// </summary>
        private string _MainThisGetAddr;
        public string MainThisGetAddr
        {
            get { return _MainThisGetAddr; }
            set
            {
                SetProperty(ref _MainThisGetAddr, value);
                _mController.SysConfig.MainThisGetAddr = value;
            }
        }

        /// <summary>
        /// 主流线本站有料
        /// </summary>
        private string _MainThisHaveAddr;
        public string MainThisHaveAddr
        {
            get { return _MainThisHaveAddr; }
            set
            {
                SetProperty(ref _MainThisHaveAddr, value);
                _mController.SysConfig.MainThisHaveAddr = value;
            }
        }

        /// <summary>
        /// 主流线下游要料
        /// </summary>
        private string _MainNextGetAddr;
        public string MainNextGetAddr
        {
            get { return _MainNextGetAddr; }
            set
            {
                SetProperty(ref _MainNextGetAddr, value);
                _mController.SysConfig.MainNextGetAddr = value;
            }
        }

        /// <summary>
        /// 载具数量
        /// </summary>
        private string _carrierCountAddr;
        public string CarrierCountAddr
        {
            get { return _carrierCountAddr; }
            set
            {
                SetProperty(ref _carrierCountAddr, value);
                _mController.SysConfig.CarrierCountAddr = value;
            }
        }


        /// <summary>
        /// 总载具数量
        /// </summary>
        private string _totalCarrierCountAddr;
        public string TotalCarrierCountAddr
        {
            get { return _totalCarrierCountAddr; }
            set
            {
                SetProperty(ref _totalCarrierCountAddr, value);
                _mController.SysConfig.TotalCarrierCountAddr = value;
            }
        }

        /// <summary>
        /// 主流线载具数量
        /// </summary>
        private string _mainCarrierCountAddr;
        public string MainCarrierCountAddr
        {
            get { return _mainCarrierCountAddr; }
            set
            {
                SetProperty(ref _mainCarrierCountAddr, value);
                _mController.SysConfig.MainCarrierCountAddr = value;
            }
        }


        /// <summary>
        /// 物流线载具数量
        /// </summary>
        private string _subCarrierCountAddr;
        public string SubCarrierCountAddr
        {
            get { return _subCarrierCountAddr; }
            set
            {
                SetProperty(ref _subCarrierCountAddr, value);
                _mController.SysConfig.SubCarrierCountAddr = value;
            }
        }


        /// <summary>
        /// 回流线载具数量
        /// </summary>
        private string _backCarrierCountAddr;
        public string BackCarrierCountAddr
        {
            get { return _backCarrierCountAddr; }
            set
            {
                SetProperty(ref _backCarrierCountAddr, value);
                _mController.SysConfig.BackCarrierCountAddr = value;
            }
        }

        /// <summary>
        /// PC心跳
        /// </summary>
        private string _pcHeartBeatAddr;
        public string PCHeartBeattAddr
        {
            get { return _pcHeartBeatAddr; }
            set
            {
                SetProperty(ref _pcHeartBeatAddr, value);
                _mController.SysConfig.PCHeartBeatAddr = value;
            }
        }


        /// <summary>
        /// PC状态
        /// </summary>
        private string _tricolorStatusAddr;
        public string TricolorStatusAddr
        {
            get { return _tricolorStatusAddr; }
            set
            {
                SetProperty(ref _tricolorStatusAddr, value);
                _mController.SysConfig.TricolorStatusAddr = value;
            }
        }

        /// <summary>
        /// PC状态
        /// </summary>
        private string _firstPieceModeCommandAddr;
        public string FirstPieceModeCommandAddr
        {
            get { return _firstPieceModeCommandAddr; }
            set
            {
                SetProperty(ref _firstPieceModeCommandAddr, value);
                _mController.SysConfig.FirstPieceModeCommandAddr = value;
            }
        }

        /// <summary>
        /// PC状态
        /// </summary>
        private string _firstPieceModeStatusAddr;
        public string FirstPieceModeStatusAddr
        {
            get { return _firstPieceModeStatusAddr; }
            set
            {
                SetProperty(ref _firstPieceModeStatusAddr, value);
                _mController.SysConfig.FirstPieceModeStatusAddr = value;
            }
        }

        /// <summary>
        /// PC状态
        /// </summary>
        private string _pcStatusAddr;
        public string PCStatusAddr
        {
            get { return _pcStatusAddr; }
            set
            {
                SetProperty(ref _pcStatusAddr, value);
                _mController.SysConfig.PCStatusAddr = value;
            }
        }



        #endregion
        /// <summary>
        /// 是否可编辑
        /// </summary>
        private bool _isEdit;
        public bool IsEdit
        {
            get { return _isEdit; }
            set { SetProperty(ref _isEdit, value); }
        }

        /// <summary>
        /// plc报警列表
        /// </summary>
        private ObservableCollection<PlcAlarm> _plcAlarmList;
        public ObservableCollection<PlcAlarm> PlcAlarmList
        {
            get { return _plcAlarmList; }
            set { SetProperty(ref _plcAlarmList, value); }
        }

        /// <summary>
        /// plc地址
        /// </summary>
        private ObservableCollection<PlcAddr> _plcAddrList;
        public ObservableCollection<PlcAddr> PlcAddrList
        {
            get { return _plcAddrList; }
            set { SetProperty(ref _plcAddrList, value); }
        }

        /// <summary>
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


        public PlcAlarmContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController mController, IMotionEngine mEngine, Dispatcher Dispatcher) : base(commonBus)
        {
            _mController = mController;
            _dispatcher = Dispatcher;
            _dialogService = dialogService;
            _mEngine = mEngine;
            PlcAddrList = new ObservableCollection<PlcAddr>();
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
            IsEdit = false;
            if (commonBus.ProjInfo != null && !string.IsNullOrEmpty(commonBus.ProjInfo.ProjPath))
            {
                string sysConfig = Path.Combine(Path.GetDirectoryName(commonBus.ProjInfo.FullName), "Config", "SystemConfig.xml");
                _mController.SysConfig.LoadSysConfig(sysConfig);//保存配置
                PlcName = _mController.SysConfig.PlcServer == null ? "" : _mController.SysConfig.PlcServer.Name;
                if (PlcName != "")
                {
                    var device = _mEngine.DeviceEngine.GetVirtualByID(_mController.SysConfig.PlcServer.DeviceID) as VPlc;
                    _dispatcher.Invoke(() =>
                    {
                        PlcAddrList.Clear();
                        if (device != null)
                        {
                            device.Address.ForEach(x => PlcAddrList.Add(x));
                        }
                    }, DispatcherPriority.Normal);
                }
                StatusAddr = _mController.SysConfig.StatusAddr ?? "";
                IdleStatusAddr = _mController.SysConfig.IdleStatusAddr ?? "";
                StartAddr = _mController.SysConfig.StartAddr ?? "";
                PauseAddr = _mController.SysConfig.PauseAddr ?? "";
                HomeAddr = _mController.SysConfig.HomeAddr ?? "";
                HomeFinishAddr = _mController.SysConfig.HomeFinishAddr ?? "";
                AutoRunAddr = _mController.SysConfig.AutoRunAddr ?? "";
                RecoveryAddr = _mController.SysConfig.RecoveryAddr ?? "";
                EmptyRunAddr = _mController.SysConfig.EmptyRunAddr ?? "";
                StopAddr = _mController.SysConfig.StopAddr ?? "";

                // 地址初始化
                MainPrevHaveAddr = _mController.SysConfig.MainPrevHaveAddr ?? "";
                MainThisGetAddr = _mController.SysConfig.MainThisGetAddr ?? "";
                MainThisHaveAddr = _mController.SysConfig.MainThisHaveAddr ?? "";
                MainNextGetAddr = _mController.SysConfig.MainNextGetAddr ?? "";
                PrevHaveAddr = _mController.SysConfig.SubPrevHaveAddr ?? "";
                ThisGetAddr = _mController.SysConfig.SubThisGetAddr ?? "";
                ThisHaveAddr = _mController.SysConfig.SubThisHaveAddr ?? "";
                NextGetAddr = _mController.SysConfig.SubNextGetAddr ?? "";
                CarrierCountAddr = _mController.SysConfig.CarrierCountAddr ?? "";
                TotalCarrierCountAddr = _mController.SysConfig.TotalCarrierCountAddr ?? "";
                MainCarrierCountAddr = _mController.SysConfig.MainCarrierCountAddr ?? "";
                SubCarrierCountAddr = _mController.SysConfig.SubCarrierCountAddr ?? "";
                BackCarrierCountAddr = _mController.SysConfig.BackCarrierCountAddr ?? "";

                PCHeartBeattAddr = _mController.SysConfig.PCHeartBeatAddr ?? "";

                TricolorStatusAddr = _mController.SysConfig.TricolorStatusAddr ?? "";
                PCStatusAddr = _mController.SysConfig.PCStatusAddr ?? "";
                FirstPieceModeCommandAddr = _mController.SysConfig.FirstPieceModeCommandAddr ?? "";
                FirstPieceModeStatusAddr = _mController.SysConfig.FirstPieceModeStatusAddr ?? "";

                commonBus.IsNeedSave = false;
            }
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        private void BuildAlarmList()
        {
            PlcAlarmList = new ObservableCollection<PlcAlarm>();
            var alarmList = _mController.SysConfig.PLCAlarmList;
            if (alarmList != null && alarmList.Count > 0)
            {
                foreach (var model in alarmList)
                {
                    PlcAlarmList.Add(new PlcAlarm()
                    {
                        Address = model.Address,
                        Desc = model.Desc,
                        AlarmCode = model.AlarmCode
                    });
                }
            }
        }

        /// <summary>
        /// 监听
        /// </summary>
        private DelegateCommand _listenCommand;
        public DelegateCommand ListenCommand => _listenCommand ?? (_listenCommand = new DelegateCommand(() =>
        {
            IsEdit = false;
            _mController.StartButtonMontor();
            EditEnable = true;
            ListenEnable = false;
        }));

        /// <summary>
        /// 按钮是否可编辑
        /// </summary>
        private DelegateCommand _editEnableCommand;
        public DelegateCommand EditEnableCommand => _editEnableCommand ?? (_editEnableCommand = new DelegateCommand(() =>
        {
            _mController.StopButtonMontor();
            IsEdit = true;
            EditEnable = false;
            ListenEnable = true;
        }));

        /// <summary>
        /// PLC选择设备
        /// </summary>
        private DelegateCommand _selectPlcCommand;
        public DelegateCommand SelectPlcCommand => _selectPlcCommand ?? (_selectPlcCommand = new DelegateCommand(() =>
        {
            if (IsEdit)
            {
                ParameterAttribute args = new ParameterAttribute(Luster.Motion.Assests.Langs.LangProvider.GetLang("Device"), 0);
                args.EditorType = typeof(VPlc);
                _dialogService.ShowDeviceConfig(args, "", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("Device", out VPlc vDevice))
                        {
                            if (PlcName != vDevice.Name)
                            {
                                ClearData();
                            }
                            PlcName = vDevice.Name;
                            _mController.SysConfig.PlcServer = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                            PlcAddrList.Clear();
                            vDevice.Address.ForEach(x => PlcAddrList.Add(x));
                        }
                    }
                    if (r.Result == ButtonResult.Cancel)
                    {
                        ClearData();
                        _mController.SysConfig.PlcServer = null;
                    }
                });
            }
        }));


        /// <summary>
        /// 添加报警
        /// </summary>
        private DelegateCommand _addPlcAlarmCommand;
        public DelegateCommand AddPlcAlarmCommand => _addPlcAlarmCommand ?? (_addPlcAlarmCommand = new DelegateCommand(() =>
        {
            PlcAlarm model = null;
            _dialogService.ShowAddPlcAlarm(model, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    PlcAlarm alarmmodel = new PlcAlarm();
                    if (r.Parameters.TryGetValue("Address", out int value))
                    {
                        alarmmodel.Address = value;
                    }
                    if (r.Parameters.TryGetValue("Desc", out string desc))
                    {
                        alarmmodel.Desc = desc;
                    }
                    if (r.Parameters.TryGetValue("AlarmCode", out string alarmCode))
                    {
                        alarmmodel.AlarmCode = alarmCode;
                    }

                    if (PlcAlarmList == null)
                    {
                        PlcAlarmList = new ObservableCollection<PlcAlarm>();
                    }

                    PlcAlarmList.Add(alarmmodel);

                    CalcPlcAlarmList();
                }
            });
        }));

        /// <summary>
        /// 编辑报警
        /// </summary>
        private DelegateCommand<object> _plcAlarmItemCellEditCommand;
        public DelegateCommand<object> PlcAlarmItemCellEditCommand => _plcAlarmItemCellEditCommand ?? (_plcAlarmItemCellEditCommand = new DelegateCommand<object>((obj) =>
        {
            var editArgs = obj as DataGridCellEditEndingEventArgs;
            if (editArgs != null)
            {
                var column = editArgs.Column;
                if (column.DisplayIndex == 0)
                {
                    return;
                }
                var row = editArgs.Row;
                var model = row.DataContext as PlcAlarm;
                object curVal = null;
                if (editArgs.EditingElement is System.Windows.Controls.TextBox txt)
                {
                    curVal = txt.Text;
                }
                // 更新
                if (curVal != null)
                {
                    string propName = editArgs.Column.SortMemberPath;

                    var prop = model.GetType().GetProperty(propName);
                    if (prop != null)
                    {
                        prop.SetValue(model, curVal.ConvertTo(prop.PropertyType));
                    }
                    CalcPlcAlarmList();
                }
            }
        }));

        /// <summary>
        /// 禁止编辑plc地址
        /// </summary>
        private DelegateCommand<object> _plcAlarmItemBeginEditCommand;
        public DelegateCommand<object> PlcAlarmItemBeginEditCommand => _plcAlarmItemBeginEditCommand ?? (_plcAlarmItemBeginEditCommand = new DelegateCommand<object>((obj) =>
        {
            var editArgs = obj as DataGridBeginningEditEventArgs;
            if (editArgs != null)
            {
                if (editArgs.Column.DisplayIndex == 0)
                {
                    editArgs.Cancel = true;
                }
            }
        }));

        /// <summary>
        /// 删除报警
        /// </summary>
        private DelegateCommand<object> _deletePlcAlarmCommand;
        public DelegateCommand<object> DeletePlcAlarmCommand => _deletePlcAlarmCommand ?? (_deletePlcAlarmCommand = new DelegateCommand<object>((o) =>
        {
            PlcAlarm model = (PlcAlarm)o;
            if (model != null)
            {
                PlcAlarmList.Remove(model);
                CalcPlcAlarmList();
            }
        }));

        /// <summary>
        /// 更新AlarmList
        /// </summary>
        private void CalcPlcAlarmList()
        {
            if (PlcAlarmList != null)
            {
                _mController.SysConfig.PLCAlarmList.Clear();
                _mController.SysConfig.PLCAlarmList.AddRange(PlcAlarmList);
                commonBus.IsNeedSave = true;
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        private DelegateCommand _exportCommand;
        public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(() =>
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "CSV|*.csv";
            if (saveFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var filename = saveFile.FileName;
                CSVTool.SaveCSV(PlcAlarmList, filename, Encoding.UTF8);
            }
        }));

        private DelegateCommand _importCommand;
        public DelegateCommand ImportCommand => _importCommand ?? (_importCommand = new DelegateCommand(() =>
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = "CSV|*.csv";
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var filename = openFile.FileName;
                var alarmplcList = OpenCSV<PlcAlarm>(filename);
                if (alarmplcList.Count > 0)
                {
                    PlcAlarmList.Clear();
                    alarmplcList.ForEach(x => PlcAlarmList.Add(x));
                    if (PlcAlarmList.Count > 0)
                    {
                        CalcPlcAlarmList();
                    }
                }
            }
        }));

        public List<T> OpenCSV<T>(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("file:" + fileName + " is not exist!");
            }

            List<PropertyInfo> properties = GetProperties<T>();
            List<T> list = new List<T>();
            using (StreamReader streamReader = new StreamReader(fileName, Encoding.Default))
            {
                string text;
                int Lineno = 0;
                while ((text = streamReader.ReadLine()) != null)
                {
                    Lineno++;
                    if (Lineno == 1) continue;
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    string[] array = text.Split(',');
                    T val = Activator.CreateInstance<T>();
                    for (int i = 0; i < properties.Count && i < array.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(array[i]))
                        {
                            object value = Convert.ChangeType(array[i], properties[i].PropertyType);
                            properties[i].SetValue(val, value);
                        }
                    }
                    list.Add(val);
                }
            }
            return list;
        }

        public List<PropertyInfo> GetProperties<T>()
        {
            List<PropertyInfo> list = (from u in typeof(T).GetProperties()
                                       where u.GetCustomAttribute<IgnoreAttribute>() == null
                                       select u).ToList();
            list.Sort(new PropSortCompare<PropertyInfo>());
            return list;
        }

        private void ClearData()
        {
            PlcName = "";
            StatusAddr = "";
            StartAddr = "";
            PauseAddr = "";
            HomeAddr = "";
            HomeFinishAddr = "";
            AutoRunAddr = "";
            RecoveryAddr = "";
            EmptyRunAddr = "";
            StopAddr = "";
            IdleStatusAddr = "4404";
        }


        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //base.OnNavigatedTo(navigationContext);
            //InitData();

        }
    }
}