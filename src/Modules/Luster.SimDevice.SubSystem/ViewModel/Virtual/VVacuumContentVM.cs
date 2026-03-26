#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VVacuumContentVM
* 机器名称:       F04846
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VVacuumContentVM.cs
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
using Luster.Motion.DataStruct.DataModels;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    internal class VVacuumContentVM : PageVM
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

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            base.Subscribe(engineUI);
        }

        protected VVacuumContentVM(ISimDeviceEngineUI _engine, Dispatcher dispatcher) : base(_engine)
        {
            SelectedList = new ObservableCollection<VacuumModel>();
            _dispatcher = dispatcher;
            LoadDevices();
            isRefresh = true;
            RefreshVacuumStatus();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            isRefresh = false;
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
        /// 增加新的真空
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowVacuumDialog(null, r =>
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
                        VacuumList.Remove(item);
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

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            var devices = deviceEngine.GetDevices(typeof(VVacuum));
            VacuumList = new ObservableCollection<VacuumModel>();
            foreach (var device in devices)
            {
                var vVacuum = device as VVacuum;
                var vacuumModel = new VacuumModel(vVacuum);
                vacuumModel.SelectedChanged += VacuumModel_SelectedChanged;
                if (string.IsNullOrEmpty(key))
                {
                    VacuumList.Add(vacuumModel);
                }
                else
                {
                    if (vVacuum.Module.Contains(key) || vVacuum.Name.Contains(key))
                    {
                        VacuumList.Add(vacuumModel);
                    }
                }
            }
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
        /// 按钮切换
        /// </summary>
        private DelegateCommand<object> _selectAllCommand;
        public DelegateCommand<object> SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new DelegateCommand<object>((dgControl) =>
        {
            isHeaderClick = true;
            var selectCount = VacuumList.Count(u => u.IsSelected);

            foreach (var item in VacuumList)
            {
                item.IsSelected = selectCount != VacuumList.Count;
            }

            UpdateSelected();
            isHeaderClick = false;
        }));

        /// <summary>
        /// 更新选中项目
        /// </summary>
        private void UpdateSelected()
        {
            // 点击的情况
            SelectedList = new ObservableCollection<VacuumModel>();
            foreach (var item in VacuumList)
            {
                if (item.IsSelected)
                {
                    SelectedList.Add(item);
                }
            }
        }

        /// <summary>
        /// 如果来源与Header点击事件变化
        /// </summary>
        private bool isHeaderClick = false;

        private void VacuumModel_SelectedChanged(bool isSelected)
        {
            // 防止重复触发，如果是来源于行
            if (!isHeaderClick)
            {
                IsSelectAll = VacuumList.Count(u => u.IsSelected) == VacuumList.Count();

                // 将Item添加到Selected
                UpdateSelected();

                // 刷真空检知的状态
                //RefreshVacuumStatus();
            }
        }

        /// <summary>
        /// 刷新真空检知状态
        /// </summary>
        private void RefreshVacuumStatus()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!isRefresh)
                        break;

                    if (SelectedList.Count > 0)
                    {
                        // 状态更新，涉及到UI显示，需要在主线程调用
                        _dispatcher.Invoke(() =>
                        {
                            foreach (var item in SelectedList)
                            {
                                item.Status = item.Tag.InVacuumSensor.GetDigitalIn();
                            }
                        });
                    }
                    // 等待时长
                    await Task.Delay(500);
                }
            });
        }

        /// <summary>
        /// 真空列表
        /// </summary>
        private ObservableCollection<VacuumModel> vacuumList;
        public ObservableCollection<VacuumModel> VacuumList
        {
            get { return vacuumList; }
            set { SetProperty(ref vacuumList, value); }
        }

        /// <summary>
        /// 真空选中列表
        /// </summary>
        private ObservableCollection<VacuumModel> selectedList;
        public ObservableCollection<VacuumModel> SelectedList
        {
            get { return selectedList; }
            set { SetProperty(ref selectedList, value); }
        }

        #region 指令
        /// <summary>
        /// 真空吸
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumSuckCommand;
        public DelegateCommand<VacuumModel> VacuumSuckCommand => _vacuumSuckCommand ?? (_vacuumSuckCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.Suck();
        }));

        /// <summary>
        /// 真空破
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumBlowCommand;
        public DelegateCommand<VacuumModel> VacuumBlowCommand => _vacuumBlowCommand ?? (_vacuumBlowCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.Blow();
        }));

        /// <summary>
        /// 真空关
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumCloseCommand;
        public DelegateCommand<VacuumModel> VacuumCloseCommand => _vacuumCloseCommand ?? (_vacuumCloseCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.Close();
        }));


        /// <summary>
        /// 真空破+关
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumBlowCloseCommand;
        public DelegateCommand<VacuumModel> VacuumBlowCloseCommand => _vacuumBlowCloseCommand ?? (_vacuumBlowCloseCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.BlowClose();
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
        /// 编辑指令
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((item) =>
        {
            dialogService.ShowVacuumDialog(item as VacuumModel, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<VacuumModel>("VacuumModel", out var deviceModel))
                    {
                        // update device
                        var dModel = VacuumList.FirstOrDefault(u => u.ID == deviceModel.ID);
                        if (dModel != null)
                        {
                            dModel = deviceModel;
                        }
                    }
                }
            });
        }));

        #endregion
    }
}
