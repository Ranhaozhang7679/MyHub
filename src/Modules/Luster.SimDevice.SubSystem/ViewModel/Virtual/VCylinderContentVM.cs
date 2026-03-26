#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VCylinderContentVM
* 机器名称:       F04846
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VCylinderContentVM.cs
* 创建时间:       2022/6/16 11:17:27
* 作    者:       房晶鹏
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       jingpengfang@lusterinc.com 
* 唯一标识：      ac299086-e4e4-460a-b201-902a279af15b
* 登录用户:       fangjingpeng
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/16 11:17:27
* 修 改 人:		  F04846
************************************************************************************/
#endregion
using Luster.Common.Assets;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
//using Luster.SimDevice.VDevice;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VCylinderContentVM : PageVM
    {
        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 显示移除按钮
        /// </summary>
        public override bool IsShowRemove => true;

        /// <summary>
        /// 实时刷新标志
        /// </summary>
        private bool isRefresh = false;

        private Dispatcher _dispatcher;
        protected VCylinderContentVM(ISimDeviceEngineUI _engine, Dispatcher dispatcher) : base(_engine)
        {
            SelectedList = new ObservableCollection<CylinderModel>();
            _dispatcher = dispatcher;
            // 加载设备
            LoadDevices();
        }

        /// <summary>
        /// 离开
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            whileToken?.Cancel();
        }

        /// <summary>
        /// 进入
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            RefreshCylinderStatus();
        }

        /// <summary>
        /// 显示参数
        /// </summary>
        private bool _isShowParam;
        public bool IsShowParam
        {
            get { return _isShowParam; }
            set { SetProperty(ref _isShowParam, value); }
        }

        /// <summary>
        /// 选择所有
        /// </summary>
        private bool _isSelectAll;
        public bool IsSelectAll
        {
            get { return _isSelectAll; }
            set { SetProperty(ref _isSelectAll, value); }
        }

        /// <summary>
        /// 增加新的气缸
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowCylinderDialog(null, r =>
             {
                 LoadDevices();
             });
        }

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
                    
                    foreach (var item in SelectedList)
                    {
                        var lstIds = new List<Guid>();
                        lstIds.Add(item.ID);
                        // 删除DeviceEngine中
                        deviceEngine.ReomoveVirtual(lstIds.ToArray());
                        CylinderList.Remove(item);                        
                    }
                    
                    
                    SelectedList.Clear();

                   
                    // 全部清除的时候，将
                    IsSelectAll = false;
                }
            });
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            var devices = deviceEngine.GetDevices(typeof(VCylinder));
            CylinderList = new ObservableCollection<CylinderModel>();
            foreach (var device in devices)
            {
                var vCylinder = device as VCylinder;
                var cylinderModel = new CylinderModel(vCylinder);
                cylinderModel.SelectedChanged += CylinderModel_SelectedChanged;
                if (string.IsNullOrEmpty(key))
                {
                    CylinderList.Add(cylinderModel);
                }
                else
                {
                    if (vCylinder.Module.Contains(key) || vCylinder.Name.Contains(key))
                    {
                        CylinderList.Add(cylinderModel);
                    }
                }
            }
        }

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            base.Subscribe(engineUI);
        }
        /// <summary>
        /// 气缸列表
        /// </summary>
        private ObservableCollection<CylinderModel> cylinderList;
        public ObservableCollection<CylinderModel> CylinderList
        {
            get { return cylinderList; }
            set { SetProperty(ref cylinderList, value); }
        }

        /// <summary>
        /// 气缸选中列表
        /// </summary>
        private ObservableCollection<CylinderModel> selectedList;
        public ObservableCollection<CylinderModel> SelectedList
        {
            get { return selectedList; }
            set { SetProperty(ref selectedList, value); }
        }

        /// <summary>
        /// 如果来源与Header点击事件变化
        /// </summary>
        private bool isHeaderClick = false;

        private void CylinderModel_SelectedChanged(bool isSelected)
        {
            // 防止重复触发，如果是来源于行
            if (!isHeaderClick)
            {
                IsSelectAll = CylinderList.Count(u => u.IsSelected) == CylinderList.Count();

                // 将Item添加到Selected
                UpdateSelected();
                // 刷新气缸的状态
                //RefreshCylinderStatus();
            }
        }

        /// <summary>
        /// 更新选中项目
        /// </summary>
        private void UpdateSelected()
        {
            // 点击的情况
            SelectedList = new ObservableCollection<CylinderModel>();
            foreach (var item in CylinderList)
            {
                if (item.IsSelected)
                {
                    SelectedList.Add(item);
                }
            }
        }

        /// <summary>
        /// 刷新气缸DI状态
        /// </summary>
        private void RefreshCylinderStatus()
        {
            if (whileToken != null)
            {
                whileToken?.Cancel();
                Thread.Sleep(300);
            }

            whileToken = new System.Threading.CancellationTokenSource();

            Task.Run(() =>
            {
                while (true)
                {
                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (SelectedList.Count > 0)
                    {
                        // 状态更新，涉及到UI显示，需要在主线程调用
                        _dispatcher.Invoke(() =>
                        {
                            foreach (var item in SelectedList)
                            {
                                item.StatusExtend = item.Tag.InExtendSensor.GetDigitalIn();
                                item.StatusRetract = item.Tag.InRetractSensor.GetDigitalIn();
                            }
                        });
                    }

                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // 等待时长
                    Thread.Sleep(200);
                }
            }, whileToken.Token);
        }

        /// <summary>
        /// 进入
        /// </summary>
        public override void Enter()
        {
            RefreshCylinderStatus();
        }

        /// <summary>
        /// 离开
        /// </summary>
        public override void Leave()
        {
            whileToken?.Cancel();
            Thread.Sleep(200);
            whileToken = null;
        }

        #region Command指令
        /// <summary>
        /// 气缸伸出
        /// </summary>
        private DelegateCommand<CylinderModel> _cylinderExtendCommand;
        public DelegateCommand<CylinderModel> CylinderExtendCommand => _cylinderExtendCommand ?? (_cylinderExtendCommand = new DelegateCommand<CylinderModel>((cylinder) =>
        {
            cylinder.Tag.Extend();
            //cylinder.Tag.InExtendSensorCheck(1000);
        }));

        /// <summary>
        /// 气缸缩回
        /// </summary>
        private DelegateCommand<CylinderModel> _cylinderRetractCommand;
        public DelegateCommand<CylinderModel> CylinderRetractCommand => _cylinderRetractCommand ?? (_cylinderRetractCommand = new DelegateCommand<CylinderModel>((cylinder) =>
        {
            cylinder.Tag.Retract();
            //cylinder.Tag.InRetractSensorCheck(1000);
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
        /// 按钮切换
        /// </summary>
        private DelegateCommand<object> _selectAllCommand;
        public DelegateCommand<object> SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new DelegateCommand<object>((dgControl) =>
        {
            isHeaderClick = true;
            var selectCount = CylinderList.Count(u => u.IsSelected);

            foreach (var item in CylinderList)
            {
                item.IsSelected = selectCount != CylinderList.Count;
            }

            UpdateSelected();
            isHeaderClick = false;
        }));

        /// <summary>
        /// 编辑指令
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((item) =>
        {
            dialogService.ShowCylinderDialog(item as CylinderModel, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<CylinderModel>("CylinderModel", out var deviceModel))
                    {
                        LoadDevices();
                        // update device
                        //for (int i = 0; i < CylinderList.Count; i++)
                        //{
                        //    if (CylinderList[i].ID == deviceModel.ID)
                        //    {
                        //        CylinderList[i] = deviceModel;
                        //        break;
                        //    }
                        //}
                    }
                }
            });
        }));
        #endregion

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }
    }
}
