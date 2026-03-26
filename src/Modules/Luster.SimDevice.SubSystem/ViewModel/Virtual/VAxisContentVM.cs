#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VAxisContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VAxisContentVM.cs
* 创建时间:       2022/4/21 18:18:46
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      30b50060-9a83-4783-9885-1de86f51de6a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/21 18:18:46
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Luster.SimDevice.SubSystem.Extension;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    /// <summary>
    /// 虚拟轴
    /// </summary>
    public class VAxisContentVM : PageVM
    {
        #region 
        /// <summary>
        /// 运动控制器实例
        /// </summary>
        private readonly IMotionController _motionController;

        /// <summary>
        /// 当前运行模式
        /// </summary>
        private string _currentMode;
        public string CurrentMode
        {
            get { return _currentMode; }
            set { SetProperty(ref _currentMode, value); }
        }

        private DispatcherTimer _modeTextTimer;
        private string _modeSpeedText;
        public string ModeSpeedText
        {
            get { return _modeSpeedText; }
            set { SetProperty(ref _modeSpeedText, value); }
        }

        /// <summary>
        /// 所有可用模式列表
        /// </summary>
        private ObservableCollection<RunModeModel> _modeList;
        public ObservableCollection<RunModeModel> ModeList
        {
            get { return _modeList; }
            set { SetProperty(ref _modeList, value); }
        }
        #endregion

        public override bool IsShowAdd => true;

        public override bool IsShowRemove => true;

        public DelegateCommand<PositionItem> RemovePositionCommand { get; set; }

        #region 
        protected VAxisContentVM(ISimDeviceEngineUI _engine, IMotionController motionController) : base(_engine)
        {
            // 注入运动控制器
            _motionController = motionController;

            // 设置方向
            HomeDirList = typeof(HomeMode).EnumToDataSource();
            AxisPMLList = typeof(AxisPML).EnumToDataSource();
            AxisForwardList = typeof(AxisForward).EnumToDataSource();
            PriorityList = typeof(Priority).EnumToDataSource();
            AxisTypes = typeof(AxisType).EnumToDataSource();
            SelectedList = new ObservableCollection<EngineUI.Models.AxisModel>();
            RemovePositionCommand = new DelegateCommand<PositionItem>(RemovePosition);

            // 初始化模式相关数据
            InitModeData();
            // 初始化模式文本定时器
            InitModeTextTimer();
            // 加载设备
            LoadDevices();

            IsEnabled = false;
            IsEnabledPosion = false;
            SysRole = SystemRole.Admin;

            // 订阅模式变更事件
            SubscribeModeChangedEvent();
        }
        #endregion

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            base.Subscribe(engineUI);
        }

        #region 新增：模式相关初始化和事件订阅
        /// <summary>
        /// 初始化模式数据
        /// </summary>
        private void InitModeData()
        {
            try
            {
                // 获取所有模式列表
                var modes = _motionController?.GetModeList() ?? new List<RunModeModel>();
                ModeList = new ObservableCollection<RunModeModel>(modes);

                // 获取当前激活的模式
                CurrentMode = _motionController?.GetCurrentMode() ?? "生产模式";

                // 根据当前模式设置所有轴的速度百分比
                UpdateSpeedPercentByCurrentMode();
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(LogType.Error, $"初始化模式数据失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 根据当前模式更新所有轴的速度百分比
        /// </summary>
        private void UpdateSpeedPercentByCurrentMode()
        {
            if (AxisList == null || !AxisList.Any()) return;

            foreach (var axisModel in AxisList)
            {
                if (axisModel?.Tag is VAxis vAxis)
                {
                    // 根据当前模式获取对应的速度百分比
                    double speedPercent = GetCurrentModeSpeedPercent(vAxis);
                    
                    vAxis.CurrentMode = CurrentMode;
                    if (CurrentMode != "生产模式" && CurrentMode != "空跑模式" && CurrentMode != "调试模式" && CurrentMode != "调机模式")
                    {
                        vAxis.SpeedPercent = speedPercent;
                    }
                        //if (CurrentMode != "生产模式" && CurrentMode != "空跑模式" && CurrentMode != "调试模式" && CurrentMode != "调机模式")
                        //{
                        //    vAxis.SpeedPercent = speedPercent;
                        //    vAxis.CurrentMode = CurrentMode;
                        //}

                        SpeedPercent = speedPercent * 100;
                    if (SpeedPercent > 150)
                    {
                        SpeedPercent = 150;
                    }
                }
            }
        }
        /// <summary>
        /// 订阅模式变更事件
        /// </summary>
        private void SubscribeModeChangedEvent()
        {
            if (_motionController != null)
            {
                _motionController.ModeChangedEvent += OnModeChanged;
            }
        }

        /// <summary>
        /// 模式变更事件处理
        /// </summary>
        /// <param name="oldMode">旧模式</param>
        /// <param name="newMode">新模式</param>
        private void OnModeChanged(string oldMode, string newMode)
        {
            // 更新当前模式显示
            CurrentMode = newMode;

            foreach (var mode in ModeList)
            {
                mode.IsRunMode = mode.Mode == newMode;
            }
            // 根据新模式更新所有轴的速度百分比
            UpdateSpeedPercentByCurrentMode();
            UpdateModeSpeedText();
        }

        // 初始化定时器方法
        private void InitModeTextTimer()
        {
            _modeTextTimer = new DispatcherTimer();
            _modeTextTimer.Interval = TimeSpan.FromSeconds(2); // 每2秒刷新一次
            _modeTextTimer.Tick += (s, e) => UpdateModeSpeedText();
            _modeTextTimer.Start();
        }

        #endregion

        /// <summary>
        /// 回零方向
        /// </summary>
        public List<KeyValue> HomeDirList { get; set; }

        /// <summary>
        /// 伺服品牌
        /// </summary>
        public List<KeyValue> AxisPMLList { get; set; }

        /// <summary>
        /// 正方向
        /// </summary>
        public List<KeyValue> AxisForwardList { get; set; }

        /// <summary>
        /// 回零方向
        /// </summary>
        public List<KeyValue> PriorityList { get; set; }

        /// <summary>
        /// 轴的类型
        /// </summary>
        public List<KeyValue> AxisTypes { get; set; }


        public List<VAxis> ListAxis { get; set; }
        /// <summary>
        /// 显示Home参数
        /// </summary>
        private bool _isShowHome;
        public bool IsShowHome
        {
            get { return _isShowHome; }
            set { SetProperty(ref _isShowHome, value); }
        }

        /// <summary>
        /// 是否启用轴的GroupBox
        /// </summary>
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// 是否启用点位的GroupBox
        /// </summary>
        private bool _isEnabledPosion;
        public bool IsEnabledPosion
        {
            get { return _isEnabledPosion; }
            set { SetProperty(ref _isEnabledPosion, value); }
        }


        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            var devices = deviceEngine.GetDevices(typeof(VAxis));
            AxisList = new ObservableCollection<AxisModel>();
            foreach (var device in devices)
            {
                var vAxis = device as VAxis;
                var axisModel = new AxisModel(vAxis);

                // 根据当前模式设置对应的速度百分比
                double currentSpeedPercent = GetCurrentModeSpeedPercent(vAxis);
                SpeedPercent = currentSpeedPercent * 100;

                if (SpeedPercent > 150)
                {
                    SpeedPercent = 150;
                }

                axisModel.SelectedChanged += AxisModel_SelectedChanged;
                if (string.IsNullOrEmpty(key))
                {
                    AxisList.Add(axisModel);
                }
                else
                {
                    if (vAxis.Module.Contains(key) || vAxis.Name.Contains(key))
                    {
                        AxisList.Add(axisModel);
                    }
                }
            }
        }


        /// <summary>
        /// 切换当前对
        /// </summary>
        /// <param name="mode"></param>
        protected override void ModeChanged(DeviceMode mode)
        {
            base.ModeChanged(mode);
        }

        /// <summary>
        /// 刷新状态
        /// </summary>
        private void StartMonitor()
        {
            // 尝试刷新轴的状态
            if (Current == null || Current.Tag.AxisType == AxisType.None) return;

            if (whileToken != null)
            {
                whileToken?.Cancel();
                Thread.Sleep(150);
            }

            whileToken = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (true)
                {
                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // 更新状态信息
                    var dictStatus = Current.Tag.GetAxisStatus(false);
                    foreach (var sItem in Current.StatusList)
                    {
                        if (dictStatus.ContainsKey(sItem.Name))
                        {
                            sItem.Status = dictStatus[sItem.Name];
                        }
                        else
                        {
                            sItem.Status = false;
                        }
                    }

                    Current.CurrentPos = Current.Tag.GetCurrentPos();

                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                }
            }, whileToken.Token);
        }

        /// <summary>
        /// 如果来源与Header点击事件变化
        /// </summary>
        private bool isHeaderClick = false;

        /// <summary>
        /// Axis 选择
        /// </summary>
        /// <param name="isSelected"></param>
        private void AxisModel_SelectedChanged(bool isSelected)
        {
            // 防止重复触发，如果是来源于行
            if (!isHeaderClick)
            {
                IsSelectAll = AxisList.Count(u => u.IsSelected) == AxisList.Count();

                // 将Item添加到Selected
                UpdateSelected();
            }
        }

        /// <summary>
        /// 更新选中项目
        /// </summary>
        private void UpdateSelected()
        {
            // 点击的情况
            SelectedList = new ObservableCollection<EngineUI.Models.AxisModel>();
            foreach (var item in AxisList)
            {
                if (item.IsSelected)
                {
                    SelectedList.Add(item);
                }
            }
        }

        /// <summary>
        /// 轴列表
        /// </summary>
        private ObservableCollection<EngineUI.Models.AxisModel> selectedList;
        public ObservableCollection<EngineUI.Models.AxisModel> SelectedList
        {
            get { return selectedList; }
            set { SetProperty(ref selectedList, value); }
        }


        /// <summary>
        /// 轴列表
        /// </summary>
        private ObservableCollection<EngineUI.Models.AxisModel> axisList;
        public ObservableCollection<EngineUI.Models.AxisModel> AxisList
        {
            get { return axisList; }
            set { SetProperty(ref axisList, value); }
        }

        /// <summary>
        /// 显示Home参数
        /// </summary>
        private bool _isSelectAll;
        public bool IsSelectAll
        {
            get { return _isSelectAll; }
            set { SetProperty(ref _isSelectAll, value); }
        }

        /// <summary>
        /// 显示Home参数
        /// </summary>
        private bool _isRead = false;
        public bool IsRead
        {
            get { return _isRead; }
            set { SetProperty(ref _isRead, value); }
        }

        private bool _isAllowUpdate = true;
        public bool IsAllowUpdate
        {
            get { return _isAllowUpdate; }
            set { SetProperty(ref _isAllowUpdate, value); }
        }

        /// <summary>
        /// 回零参数是否只读
        /// </summary>
        private bool _isAllowHome = true;
        public bool IsAllowHome
        {
            get { return _isAllowHome; }
            set
            {
                SetProperty(ref _isAllowHome, value);
                if (SysRole == SystemRole.Admin)
                {
                    if (_isAllowHome)
                    {
                        IsAllowUpdate = true;
                    }
                    else
                    {
                        IsAllowUpdate = false;
                    }
                }

            }
        }


        /// <summary>
        /// 所有轴选择-暂时未使用
        /// </summary>
        private DelegateCommand<object> _selectAllCommand;
        public DelegateCommand<object> SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new DelegateCommand<object>((dgControl) =>
        {
            isHeaderClick = true;
            var selectCount = AxisList.Count(u => u.IsSelected);

            foreach (var item in AxisList)
            {
                item.IsSelected = selectCount != AxisList.Count;
            }

            UpdateSelected();
            isHeaderClick = false;
        }));

        /// <summary>
        /// 常规参数，轴参数，导入以及导出命令选择
        /// </summary>
        private DelegateCommand<string> _changeParamCommand;
        public DelegateCommand<string> ChangeParamCommand => _changeParamCommand ?? (_changeParamCommand = new DelegateCommand<string>((type) =>
        {
            IsShowHome = type == "Home";
            IsAllowUpdate = true;
        }));

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<string> _searchCommand;
        public DelegateCommand<string> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<string>((key) =>
        {
            string trimKey = key.Trim();

            LoadDevices(trimKey);
        }));

        /// <summary>
        /// 轴参数编辑指令
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {
            var editArgs = obj as DataGridCellEditEndingEventArgs;
            if (editArgs != null)
            {
                var row = editArgs.Row;
                var axis = row.DataContext as EngineUI.Models.AxisModel;

                var vAxis = deviceEngine.GetVirtualByID(axis.ID);
                if (vAxis != null)
                {
                    object curVal = null;
                    if (editArgs.EditingElement is TextBox txt)
                    {
                        curVal = txt.Text;
                    }
                    else if (editArgs.EditingElement is ComboBox comb)
                    {
                        curVal = comb.SelectedValue;
                    }

                    // 更新
                    if (curVal != null)
                    {
                        string propName = editArgs.Column.SortMemberPath;

                        var prop = vAxis.GetType().GetProperty(propName);
                        if (prop != null)
                        {
                            prop.SetValue(vAxis, curVal.ConvertTo(prop.PropertyType));
                        }
                    }


                }


                // 代表有更新
                deviceEngine.IsNeedSave = true;
            }

            var axisModel = obj as AxisModel;
            if (axisModel != null)
            {
                axisModel.CalPulseRatio();
                deviceEngine.IsNeedSave = true;


            }

        }));

        #region 轴参数批量导入导出

        /// <summary>
        /// 批量导入
        /// </summary>
        private DelegateCommand _batchImportCommand;
        public DelegateCommand BatchImportCommand => _batchImportCommand ?? (_batchImportCommand = new DelegateCommand(() =>
        {
            var ioCount = deviceEngine.GetDevices(typeof(VAxis)).Count();
            if (ioCount > 0)
            {
                dialogService.ShowConfirm("导入数据的同时清除历史数据?", r =>
                {
                    if (r.Result == ButtonResult.Cancel)
                    {
                        return;
                    }

                    if (r.Result == ButtonResult.OK)
                    {
                        // 清理已有数据
                        deviceEngine.ReomoveVirtual(typeof(VAxis));
                    }

                    dialogService.ShowBatchImport(false, (res) =>
                    {
                        LoadDevices();
                    });
                });
            }
            else
            {
                dialogService.ShowBatchImport(false, (res) =>
                {
                    LoadDevices();
                });
            }
        }));

        /// <summary>
        /// 批量导出
        /// </summary>
        private DelegateCommand _batchExportCommand;
        public DelegateCommand BatchExportCommand => _batchExportCommand ?? (_batchExportCommand = new DelegateCommand(() =>
        {
            BatchExport();
        }));

        // 导出
        private void BatchExport()
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "XLS|*.xls";
            var result = saveFile.ShowDialog().Value;
            if (result)
            {
                var filename = saveFile.FileName;
                var axises = deviceEngine.GetDevices(typeof(VAxis)).Select(u => u as VAxis).ToList();
                var exportModels = new List<ExportAxisModel>();
                int i = 1;
                foreach (var axis in axises)
                {
                    var model = new ExportAxisModel();
                    model.Index = i;
                    model.AxisNo = $"Axis{i}";
                    model.Name = axis.Name;
                    model.ModuleName = axis.Module;
                    model.AccSpeed = axis.Acc;
                    model.DecSpeed = axis.Dec;
                    model.MoveSpeed = axis.MoveSpeed;
                    model.Pluse = axis.PerPluse;
                    model.SoftLimitML = axis.SoftLimitML;
                    model.SoftLimitML = axis.SoftLimitML;
                    model.Stime = axis.Stime;
                    model.AxisBand = axis.AxisBand;

                    model.HomeSpeedHigh = (int)axis.HomeSpeedHigh;
                    model.HomeSpeedLow = (int)axis.HomeSpeedLow;
                    model.HomeMode = (int)axis.HomeMode;
                    model.Priority = (int)axis.HomePriority;
                    model.HomeAcc = axis.HomeAcc;
                    model.HomeDec = axis.HomeDec;
                    model.HomeOffset = axis.HomeOffset;
                    model.AxisType = axis.AxisType;

                    model.HaveTransmission = axis.HaveTransmission;
                    model.CommandPulse = axis.CommandPulse;
                    model.CircleDisplacement = axis.CircleDisplacement;
                    model.GearRatioNumerator = axis.GearRatioNumerator;
                    model.GearRatioDenominator = axis.GearRatioDenominator;


                    exportModels.Add(model);
                    i++;
                }
                var excel = new ExcelTool();
                excel.AddWorksheet("AxisPara");
                var prop = typeof(ExportAxisModel).GetProperties().Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null).ToList();
                var header = prop.Select(x => x.GetCustomAttribute<DisplayNameAttribute>().DisplayName).ToArray();
                var data = new object[header.Length];
                excel.SetHeaders(0, 0, header);
                for (int k = 0; k < exportModels.Count(); k++)
                {
                    for (int j = 0; j < header.Length; j++)
                    {
                        data[j] = prop.Where(x => x.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName == header[j]).FirstOrDefault()?.GetValue(exportModels[k]);
                    }
                    excel.WriteRowDatas(k + 1, 0, data);
                }
                excel.Save(filename);
            }
        }

        private DelegateCommand _exportToTxtCommand;

        public DelegateCommand ExportToTxtCommand => _exportToTxtCommand ?? (_exportToTxtCommand = new DelegateCommand(() =>
        {
            var saveFile = new SaveFileDialog();
            var result = saveFile.ShowDialog().Value;
            saveFile.Filter = "Txt|*.txt";
            if (result)
            {
                System.IO.TextWriter writer = new System.IO.StreamWriter(saveFile.FileName);
                StringBuilder sb = new StringBuilder();
                var axises = deviceEngine.GetDevices(typeof(VAxis)).Select(u => u as VAxis).ToList();
                foreach (var axis in axises)
                {
                    var posList = axis.Positions;
                    foreach (var pos in posList)
                    {
                        sb.AppendLine(axis.AxisNo + "_" + pos.Name + ",double," + pos.Position);
                    }
                }
                writer.WriteLine(sb.ToString());
                Thread.Sleep(1);
                writer.Flush();
                Thread.Sleep(1);
                writer.Close();
            }
        }));

        #endregion

        #region 轴对象新增和删除

        /// <summary>
        /// 新增对象
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowAxisDialog(r =>
            {
                LoadDevices();
            });
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        public override void RemoveItem()
        {
            if (SelectedList.Count == 0)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, "请先选择要进行删除的项");
            }

            // 删除确认
            dialogService.ShowConfirm($"确认删除{SelectedList.Count}项?", (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var lstIds = new List<Guid>();
                    foreach (var item in SelectedList)
                    {
                        AxisList.Remove(item);
                        lstIds.Add(item.ID);
                    }

                    // 删除DeviceEngine中
                    deviceEngine.ReomoveVirtual(lstIds.ToArray());
                    SelectedList.Clear();

                    // 全部清除的时候，将
                    IsSelectAll = false;
                }
            });
        }

        #endregion

        #region 轴的绝对运动、相对运动和回零
        /// <summary>
        /// 绝对运动
        /// </summary>
        private DelegateCommand<EngineUI.Models.AxisModel> _moveAbsCommand;
        public DelegateCommand<EngineUI.Models.AxisModel> MoveAbsCommand => _moveAbsCommand ?? (_moveAbsCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                Task.Run(() =>
                {
                    axis.Tag.MoveAbs(MovePos);
                    axis.Tag.CheckMotionDone(targetPulse : MovePos * axis.PerPluse);
                });
            }
        }));

        /// <summary>
        /// 相对运动
        /// </summary>
        private DelegateCommand<EngineUI.Models.AxisModel> _moveRelCommand;
        public DelegateCommand<EngineUI.Models.AxisModel> MoveRelCommand => _moveRelCommand ?? (_moveRelCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                Task.Run(() =>
                {
                    axis.Tag.MoveRel(MovePos);
                    axis.Tag.CheckMotionDone(targetPulse: (axis.CurrentPos + MovePos) * axis.PerPluse);
                });
            }
        }));

        /// <summary>
        /// 回零
        /// </summary>
        private DelegateCommand<EngineUI.Models.AxisModel> _homeCommand;
        public DelegateCommand<EngineUI.Models.AxisModel> HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                Task.Run(() =>
                {
                    LogTool.Debug("1:开始执行回零");
                    axis.Tag.Home();
                    axis.Tag.CheckHomeDone();
                });

                //if (JudgeMotionCondiction(axis, "HomeZero", out string info))
                //{
                //    // 添加到异步方法中，防止主线程卡顿
                //    Task.Run(() =>
                //    {
                //        axis.Tag.Home();
                //        axis.Tag.CheckHomeDone();
                //    });
                //}
                //else
                //{
                //    throw new FriendlyException($"无法运动，请检查运动限制条件:{info}");
                //}

            }
        }));

        #endregion


        #region 轴的使能

        private DelegateCommand<AxisModel> _enableCommand;
        public DelegateCommand<AxisModel> EnableCommand => _enableCommand ?? (_enableCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.ServOn(true);
                // 使能后，重新开启监控，实时显示当前位置
                StartMonitor();
            }
        }));

        private DelegateCommand<EngineUI.Models.AxisModel> _disEnableCommand;
        public DelegateCommand<EngineUI.Models.AxisModel> DisEnableCommand => _disEnableCommand ?? (_disEnableCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.ServOn(false);
            }
        }));

        #endregion



        /// <summary>
        /// 确认是否修改
        /// </summary>
        /// 
        public void Confirm()
        {
            dialogService.ShowConfirm($"确认更改当前数据?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    IsRead = true;
                }
                else
                {
                    IsRead = false;
                }
            });
        }

        public void ConfirmUpdate()
        {
            dialogService.ShowConfirm($"确认更改当前数据?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (IsShowHome)
                    {
                        IsAllowHome = false;
                    }
                    else
                    {
                        IsAllowUpdate = false;
                    }
                }
                else
                {
                    if (IsShowHome)
                    {
                        IsAllowHome = true;
                    }
                    else
                    {
                        IsAllowUpdate = true;
                    }
                }
            });
        }

        #region JOG运动模式

        /// <summary>
        /// Jog运动
        /// </summary>
        private DelegateCommand<string> _JogCommand;
        public DelegateCommand<string> JogCommand => _JogCommand ?? (_JogCommand = new DelegateCommand<string>((name) =>
        {
            if (name == null) return;
            MotionCondictions(Current, name);
        }));

        /// <summary>
        /// JOG+点动
        /// </summary>
        private DelegateCommand<EngineUI.Models.AxisModel> _jogCommandAdd;
        public DelegateCommand<EngineUI.Models.AxisModel> JogCommandAdd => _jogCommandAdd ?? (_jogCommandAdd = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.Jog(true, axis.JogSpeed);

                //if (JudgeMotionCondiction(axis, "Jog+", out string info))
                //{
                //    axis.Tag.Jog(true, axis.JogSpeed);
                //}
                //else
                //{
                //    throw new FriendlyException("无法运动，请检查运动限制条件！"+ info);
                //}
            }
        }));

        /// <summary>
        /// JOG-点动
        /// </summary>
        private DelegateCommand<EngineUI.Models.AxisModel> _jogCommandSub;
        public DelegateCommand<EngineUI.Models.AxisModel> JogCommandSub => _jogCommandSub ?? (_jogCommandSub = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.Jog(false, axis.JogSpeed);

                //if (JudgeMotionCondiction(axis, "Jog-",out string info))
                //{
                //    axis.Tag.Jog(false, axis.JogSpeed);
                //}
                //else
                //{
                //    throw new FriendlyException("无法运动，请检查运动限制条件！");
                //}

            }
        }));

        /// <summary>
        /// 轴停止
        /// </summary>
        private DelegateCommand<EngineUI.Models.AxisModel> _stopCommand;
        public DelegateCommand<EngineUI.Models.AxisModel> StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                // 显示对象
                axis.Tag.Stop();
            }
        }));

        #endregion

        /// <summary>
        /// 轴清错
        /// </summary>
        private DelegateCommand<EngineUI.Models.AxisModel> _clearErrorCommand;
        public DelegateCommand<EngineUI.Models.AxisModel> ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            if (axis != null)
            {
                // 显示对象
                axis.Tag.ResetStatus();
            }
        }));

        /// <summary>
        /// 运动距离
        /// </summary>
        private double _movePos = 100;
        public double MovePos
        {
            get { return _movePos; }
            set { SetProperty(ref _movePos, value); }
        }

        /// <summary>
        /// 当前轴
        /// </summary>
        private AxisModel _current;
        public AxisModel Current
        {
            get { return _current; }
            set { SetProperty(ref _current, value); }
        }

        /// <summary>
        /// 速度
        /// </summary>
        private double _speedPercent;
        public double SpeedPercent
        {
            get { return _speedPercent; }
            set { SetProperty(ref _speedPercent, value); }
        }

        /// <summary>
        /// 回零参数选中行
        /// </summary>
        private int _selectHomeIndex = 0;
        public int SelectHomeIndex
        {
            get { return _selectHomeIndex; }
            set { SetProperty(ref _selectHomeIndex, value); }
        }

        /// <summary>
        /// 轴选择指令
        /// </summary>
        private DelegateCommand<object> _selectCommand;
        public DelegateCommand<object> SelectedCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<object>((args) =>
        {
            SelectionChangedEventArgs sArgs = args as SelectionChangedEventArgs;
            if (sArgs != null && sArgs.AddedItems.Count > 0)
            {

                if (sArgs.AddedItems[0] is KeyValue kVal)
                {
                    return;
                }
                else if (sArgs.AddedItems[0] is AxisModel m)
                {
                    Current = m;
                    IsEnabled = true;
                    SelectedList.Clear();
                    SelectedList.Add(Current);

                    // 加载安全区域
                    LoadSafeRegions();

                    // 点击进入实时显示当前位置
                    StartMonitor();

                    LoadTeachPoint();
                }
            }
            else
            {
                Current = null;
                IsEnabled = false;
                Leave();
            }
        }));

        /// <summary>
        /// 轴的速度修改指令
        /// </summary>
        private DelegateCommand<object> _changeSpeedCommand;
        public DelegateCommand<object> ChangeSpeedCommand => _changeSpeedCommand ?? (_changeSpeedCommand = new DelegateCommand<object>((obj) =>
        {
            RoutedPropertyChangedEventArgs<double> args = obj as RoutedPropertyChangedEventArgs<double>;

            foreach (var item in AxisList)
            {
                if (item?.Tag is VAxis vAxis)
                {
                    // 根据当前模式保存到对应的属性
                    double speedPercentValue = Math.Round(args.NewValue / 100, 1);

                    // 设置当前模式的百分比值
                    SetCurrentModeSpeedPercent(vAxis, speedPercentValue);
                }
            }

            // 更新UI显示
            SpeedPercent = args.NewValue;
            UpdateModeSpeedText();
        }));

        /// <summary>
        /// 获取当前模式的百分比值
        /// </summary>
        private double GetCurrentModeSpeedPercent(VAxis vAxis)
        {
            if (vAxis == null) return 0.1;

            return CurrentMode switch
            {
                "生产模式" => vAxis.ProductionModelSpeedPercent,
                "空跑模式" => vAxis.EmptyRunSpeedPercent,
                "调试模式" => vAxis.DebugSpeedPercent,
                "调机模式" => vAxis.DebugSpeedPercent,
                _ => vAxis.SpeedPercent 
            };
        }

        /// <summary>
        /// 设置当前模式的百分比值
        /// </summary>
        private void SetCurrentModeSpeedPercent(VAxis vAxis, double value)
        {
            if (vAxis == null) return;

            switch (CurrentMode)
            {
                case "生产模式":
                    vAxis.ProductionModelSpeedPercent = value;
                    break;
                case "空跑模式":
                    vAxis.EmptyRunSpeedPercent = value;
                    break;
                case "调试模式":
                case "调机模式": 
                    vAxis.DebugSpeedPercent = value;
                    break;
                default:
                    vAxis.SpeedPercent = value;
                    break;
            }
        }
        private void UpdateModeSpeedText()
        {
            try
            {
                if (AxisList == null || !AxisList.Any())
                {
                    ModeSpeedText = "";
                    return;
                }

                // 获取第一个轴的示例数据
                var firstAxis = AxisList.FirstOrDefault();
                if (firstAxis?.Tag is not VAxis vAxis)
                {
                    ModeSpeedText = "";
                    return;
                }

                // 计算各模式百分比
                var productionPercent = Math.Round(vAxis.ProductionModelSpeedPercent * 100, 0);
                var emptyRunPercent = Math.Round(vAxis.EmptyRunSpeedPercent * 100, 0);
                var debugPercent = Math.Round(vAxis.DebugSpeedPercent * 100, 0);
                var otherPercent = Math.Round(vAxis.SpeedPercent * 100, 0); 

                // 生成文本
                ModeSpeedText = $"生产模式: {productionPercent:F0}% " +
                               $"空跑模式: {emptyRunPercent:F0}% " +
                               $"调试/机模式: {debugPercent:F0}% " +
                               $"其他模式: {otherPercent:F0}%";
            }
            catch (Exception ex)
            {
                ModeSpeedText = "加载模式数据中...";
            }
        }
        #region 轴前提条件管理
        private void LoadSafeRegions()
        {
            if (Current != null)
            {
                SafeRegions = new ObservableCollection<SafeRegionModel>();

                if (Current.Tag == null || Current.Tag is not IPosition pos) return;

                foreach (var item in pos.GetSafeRegions())
                {
                    SafeRegions.Add(new SafeRegionModel(item, item.Min, item.Max));
                }
            }
        }

        /// <summary>
        /// 安全列表
        /// </summary>
        private ObservableCollection<SafeRegionModel> _safeRegions;
        public ObservableCollection<SafeRegionModel> SafeRegions
        {
            get { return _safeRegions; }
            set { SetProperty(ref _safeRegions, value); }
        }

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _addSafeCommand;
        public DelegateCommand<object> AddSafeCommand => _addSafeCommand ?? (_addSafeCommand = new DelegateCommand<object>((obj) =>
        {
            var axisModel = obj as AxisModel;
            if (axisModel != null)
            {
                dialogService.ShowSafeRegionDialog(axisModel, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        LoadSafeRegions();
                    }
                });
            }
        }));

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _removeSafeCommand;
        public DelegateCommand<object> RemoveSafeCommand => _removeSafeCommand ?? (_removeSafeCommand = new DelegateCommand<object>((obj) =>
        {
            if (Current == null) return;

            var safeRegion = obj as SafeRegionModel;
            if (safeRegion == null)
            {
                return;
            }

            dialogService.ShowConfirm($"确认删除:{safeRegion.Tag.Pos.Name}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (Current != null && Current.Tag is IPosition pos)
                    {
                        // 删除对应的名称
                        pos.RemovePosition(safeRegion.Tag);
                        SafeRegions.Remove(safeRegion);
                        LoadSafeRegions();
                    }
                }
            });
        }));
        #endregion

        #region  示教点位管理
        private ObservableCollection<PositionItem> _axisPositions;
        /// <summary>
        /// 轴示教位列表
        /// </summary>
        public ObservableCollection<PositionItem> AxisPositions
        {
            get { return _axisPositions; }
            set { SetProperty(ref _axisPositions, value); }
        }

        private void LoadTeachPoint()
        {
            if (Current != null)
            {
                AxisPositions = new ObservableCollection<PositionItem>();
                var lNodes = Current.Tag.GetAxisPosNodes();
                foreach (var node in lNodes.Children)
                {
                    var axisPosition = (AxisPosition)node.Tag;
                    var position = new PositionItem()
                    {
                        Name = axisPosition.Name,
                        Position = axisPosition.Position,
                        Tag = axisPosition
                    };
                    AxisPositions.Add(position);
                }
            }
        }

        private void RemovePosition(PositionItem item)
        {
            if (Current != null)
            {
                var axisPos = Current.Tag.Positions.FirstOrDefault(u => u.Name == item.Name);
                deviceEngine.RemoveAxisPos(axisPos);
                AxisPositions.Remove(item);
            }
        }

        private DelegateCommand<AxisModel> _teachCommand;
        public DelegateCommand<AxisModel> TeachCommand => _teachCommand ?? (_teachCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis == null) return;
            dialogService.ShowTeachPositionDialog(axis.CurrentPos, axis.Tag.AxisType.ToString(), false, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<string>("Name", out var name) &&
                     r.Parameters.TryGetValue<double>("Position", out var position))
                    {
                        axis.Tag.AddPostion(name, position);
                        var src = axis.Tag.Positions.FirstOrDefault(u => u.Name == name);
                        AxisPositions.Add(new PositionItem()
                        {
                            Name = name,
                            Position = position,
                            Tag = src
                        });
                    }
                }
            });
        }));

        private DelegateCommand<AxisModel> _motionCondictionsCommand;
        public DelegateCommand<AxisModel> MotionCondictionsCommand => _motionCondictionsCommand ?? (_motionCondictionsCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis == null) return;
            MotionCondictions(axis, "Jog+");
        }));

        private void MotionCondictions(AxisModel axis, string ActionName)
        {
            dialogService.ShowMotionConditionsDialog(axis, AxisList.ToList(), ActionName, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<VIOMoveCheckModel>("MoveCheckmodel", out var iomodel) &&
                     r.Parameters.TryGetValue<VAxisMoveDisModel>("MoveDismodel", out var dismodel))
                    {
                        var model = AxisList.FirstOrDefault(n => n.Name == axis.Name);
                        if (model != null && dismodel != null)
                        {
                            if (model.Tag == null) return;
                            if (model.Tag.MoveDisLimit != null)
                            {
                                var exist = model.Tag.MoveDisLimit.FirstOrDefault(m => m.ActionName == dismodel.ActionName);
                                if (exist != null)
                                {
                                    model.Tag.MoveDisLimit.Remove(exist);
                                    model.Tag.MoveDisLimit.Add(dismodel);
                                }
                                else
                                {
                                    model.Tag.MoveDisLimit.Add(dismodel);
                                }
                            }
                            else
                            {
                                List<VAxisMoveDisModel> list = new List<VAxisMoveDisModel>();
                                list.Add(dismodel);
                                model.Tag.MoveDisLimit = list;
                            }
                        }

                        if (model != null && iomodel != null)
                        {
                            if (model.Tag == null) return;
                            if (model.Tag.MoveIOLimit != null)
                            {
                                var exist = model.Tag.MoveIOLimit.FirstOrDefault(m => m.ActionName == dismodel.ActionName);
                                if (exist != null)
                                {
                                    model.Tag.MoveIOLimit.Remove(exist);
                                    model.Tag.MoveIOLimit.Add(iomodel);
                                }
                                else
                                {
                                    model.Tag.MoveIOLimit.Add(iomodel);
                                }
                            }
                            else
                            {
                                List<VIOMoveCheckModel> list = new List<VIOMoveCheckModel>();
                                list.Add(iomodel);
                                model.Tag.MoveIOLimit = list;
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 排序
        /// </summary>
        public void SortPositions()
        {
            for (int i = 0; i < AxisPositions.Count; i++)
            {
                var position = Current.Tag.Positions.FirstOrDefault(x => x.Name == AxisPositions[i].Name);

                if (position != null)
                {
                    Current.Tag.Positions.Remove(position);
                    Current.Tag.Positions.Insert(i, position);
                }
            }
        }

        private DelegateCommand<PositionItem> _updateTeachCommand;
        public DelegateCommand<PositionItem> UpdateTeachCommand => _updateTeachCommand ?? (_updateTeachCommand = new DelegateCommand<PositionItem>((item) =>
        {
            if (item == null) return;
            if (Current != null)
            {
                dialogService.ShowConfirm($"确认将点位:{item.Name}的值进行变更:{item.Position}->{Current.CurrentPos}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var axisPos = Current.Tag.Positions.FirstOrDefault(u => u.Name == item.Name);
                        item.Position = Current.CurrentPos;
                        deviceEngine.UpdatePostion(axisPos, Current.CurrentPos);

                        // 代表有更新
                        deviceEngine.IsNeedSave = true;
                    }
                });
            }
        }));

        /// <summary>
        /// 点位运动指令
        /// </summary>
        private DelegateCommand<PositionItem> _moveTeachCommand;
        public DelegateCommand<PositionItem> MoveTeachCommand => _moveTeachCommand ?? (_moveTeachCommand = new DelegateCommand<PositionItem>((pos) =>
        {
            if (pos == null) return;
            //AxisModel model = new AxisModel(pos.Tag.Axis);
            //if (!JudgeMotionCondiction(model, pos.Name,out string info))
            //{
            //    throw new FriendlyException("无法运动，请检查运动限制条件！"+info);
            //}
            if (!PosionJudgeMotionCondiction(pos, out string info))
            {
                throw new FriendlyException("无法运动，请检查运动限制条件！" + info);
            }
            pos.Tag.Axis.MoveAbs(pos.Position);
        }));

        /// <summary>
        /// 点位配置指令
        /// </summary>
        private DelegateCommand<PositionItem> _moveTeachLimitCommand;
        public DelegateCommand<PositionItem> MoveTeachLimitCommand => _moveTeachLimitCommand ?? (_moveTeachLimitCommand = new DelegateCommand<PositionItem>((pos) =>
        {
            if (pos == null) return;
            AxisModel model = new AxisModel(pos.Tag.Axis);
            MotionCondictions(model, pos.Name);
        }));

        #endregion

        #region 点位前提条件管理

        private void LoadPosionSafeRegions()
        {
            if (Current != null)
            {
                PosionSafeRegions = new ObservableCollection<PosionSafeRegionModel>();

                //if (Current.Tag == null || Current.Tag is not IPosition pos) return;

                if (CurrentPosion.Tag == null || CurrentPosion.Tag.Axis is not IPosition pos) return;


                foreach (var item in CurrentPosion.Tag.Axis.posionSafes)
                {
                    if (item.Position == CurrentPosion.Tag)
                    {
                        var aaa = item.PosionSafePostions;
                        foreach (var item1 in aaa)
                        {
                            PosionSafeRegions.Add(new PosionSafeRegionModel(item1, item1.Min, item1.Max));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 当前点位
        /// </summary>
        private PositionItem _currentPosion;
        public PositionItem CurrentPosion
        {
            get { return _currentPosion; }
            set { SetProperty(ref _currentPosion, value); }
        }

        /// <summary>
        /// 点位选择
        /// </summary>
        private DelegateCommand<object> _selectPosionCommand;
        public DelegateCommand<object> SelectedPosionCommand => _selectPosionCommand ?? (_selectPosionCommand = new DelegateCommand<object>((args) =>
        {
            SelectionChangedEventArgs sArgs = args as SelectionChangedEventArgs;
            if (sArgs != null && sArgs.AddedItems.Count > 0)
            {
                if (sArgs.AddedItems[0] is KeyValue kVal)
                {
                    return;
                }
                else if (sArgs.AddedItems[0] is PositionItem p)
                {
                    IsEnabledPosion = true;
                    CurrentPosion = p;
                    //加载安全区域
                    LoadPosionSafeRegions();
                }
            }
            else
            {
                IsEnabledPosion = false;
                CurrentPosion = null;
                PosionSafeRegions.Clear();
                Leave();
            }
        }));

        private ObservableCollection<PosionSafeRegionModel> _posionSafeRegions;
        public ObservableCollection<PosionSafeRegionModel> PosionSafeRegions
        {
            get { return _posionSafeRegions; }
            set { SetProperty(ref _posionSafeRegions, value); }
        }

        private DelegateCommand<object> _addPosionSafeCommand;
        public DelegateCommand<object> AddPosionSafeCommand => _addPosionSafeCommand ?? (_addPosionSafeCommand = new DelegateCommand<object>((obj) =>
        {
            var positionItem = obj as PositionItem;
            if (positionItem != null)
            {
                //AxisModel model = new AxisModel(positionItem.Tag.Axis);
                //MotionCondictions(model, positionItem.Name);
                dialogService.ShowPosionSafeRegionDialog(positionItem, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        LoadPosionSafeRegions();
                    }
                });
            }
        }));


        /// <summary>
        /// 删除点位安全前提
        /// </summary>
        private DelegateCommand<object> _removePosionSafeCommand;
        public DelegateCommand<object> RemovePosionSafeCommand => _removePosionSafeCommand ?? (_removePosionSafeCommand = new DelegateCommand<object>((obj) =>
        {
            if (Current == null) return;

            var posionSafeRegion = obj as PosionSafeRegionModel;
            if (posionSafeRegion == null)
            {
                return;
            }

            dialogService.ShowConfirm($"确认删除:{posionSafeRegion.Tag.Pos.Name}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (Current != null && Current.Tag is IPosition pos)
                    {
                        // 删除对应的名称
                        pos.RemovePosionPosition(posionSafeRegion.Tag);
                        PosionSafeRegions.Remove(posionSafeRegion);
                        LoadPosionSafeRegions();
                    }
                }
            });
        }));


        #endregion

        /// <summary>
        /// 轴运动前提
        /// </summary>
        /// <param name="model"></param>
        /// <param name="Action"></param>
        /// <returns></returns>
        private bool JudgeMotionCondiction(AxisModel model, string Action, out string info)
        {
            info = "";
            if (model.Tag == null) return false;
            if (model.Tag.MoveDisLimit != null)
            {
                var exist = model.Tag.MoveDisLimit.FirstOrDefault(m => m.ActionName == Action);
                if (exist != null)
                {
                    foreach (var item in exist.ListDisLimit)
                    {
                        var selectaxis = AxisList.FirstOrDefault(m => m.Name == item.Axis);
                        if (selectaxis != null)
                        {
                            if (selectaxis.CurrentPos < item.MinPos || selectaxis.CurrentPos > item.MaxPos || !selectaxis.Tag.IsHome)
                            {
                                info = $"{selectaxis.Tag.Name}位置为:{selectaxis.CurrentPos}，不在{item.MinPos}~{item.MaxPos}范围内";
                                return false;
                            }
                        }
                    }
                }
            }

            if (model.Tag.MoveIOLimit != null)
            {
                var exist = model.Tag.MoveIOLimit.FirstOrDefault(m => m.ActionName == Action);
                if (exist != null)
                {
                    var ioList = deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO).ToList();
                    foreach (var item in exist.ListIOCheck)
                    {
                        List<VIO> selectIOList = new List<VIO>();
                        if (item.vIO.Behavior == IOBehavior.Input)
                        {
                            selectIOList = ioList.Where(u => u.Behavior == IOBehavior.Input).ToList();

                        }
                        else
                        {
                            selectIOList = ioList.Where(u => u.Behavior == IOBehavior.Output).ToList();
                        }
                        var iomodel = selectIOList.FirstOrDefault(m => m.Name == item.vIO.Name);
                        if (iomodel != null)
                        {
                            //bool value = iomodel.Value==1.0?true:false;
                            bool value = iomodel.GetDigital();
                            if (value != item.CheckValue)
                            {
                                info = (iomodel.Name + "不满足值为:" + item.CheckValue);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 点位运动前提
        /// </summary>
        /// <param name="model"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool PosionJudgeMotionCondiction(PositionItem model, out string info)
        {
            info = "";
            if (model.Tag == null) return false;

            if (!model.Tag.Axis.IsHome)
            {
                info = $"设备{model.Tag.Axis.Name}没有回零!";
                return false;
            }

            if (PosionSafeRegions != null)
            {
                PosionSafeModel posionSafeModel = null;
                double pos = 0;

                foreach (var region in PosionSafeRegions)
                {
                    pos = region.Tag.Pos.GetCurrentPos();
                    if (pos < region.Min || pos > region.Max)
                    {
                        posionSafeModel = region.Tag;
                        break;
                    }
                }
                // 不满足安全的前提信息 需要将所有的不满足条件都显示出来
                if (posionSafeModel != null)
                {
                    info = $"设备:{model.Name} 运动前提:{posionSafeModel.Pos.Name}的位置:{pos}不在{posionSafeModel.Min}~{posionSafeModel.Max}范围内!";
                    return false;
                }
            }
            return true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            Leave();

            // 取消订阅模式变更事件，防止内存泄漏
            if (_motionController != null)
            {
                _motionController.ModeChangedEvent -= OnModeChanged;
            }
            if (_modeTextTimer != null)
            {
                _modeTextTimer.Stop();
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            ModeChanged(deviceEngine.DeviceMode);

            // 重新初始化模式数据
            InitModeData();
            LoadDevices();

            if (_modeTextTimer != null && !_modeTextTimer.IsEnabled)
            {
                _modeTextTimer.Start();
            }
            // 立即更新一次文本
            UpdateModeSpeedText();
        }

        public override void Enter()
        {
            base.Enter();
            StartMonitor();
        }

        public override void Leave()
        {
            whileToken?.Cancel();
            Thread.Sleep(200);
            whileToken = null;
        }
    }
}