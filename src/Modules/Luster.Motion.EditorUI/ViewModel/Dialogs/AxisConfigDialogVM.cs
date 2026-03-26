#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisMDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel.Dialogs
* 文 件 名:       AxisMDialogVM.cs
* 创建时间:       2022/6/21 10:57:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      91290ea7-0eaf-4124-8b91-5a0271cd788a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/21 10:57:59
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    /// <summary>
    /// 多轴调试界面
    /// </summary>
    public class AxisConfigDialogVM : MotionDialogVM
    {
        private IDeviceEngine _deviceEngine;

        private CancellationTokenSource tokenS;

        public AxisConfigDialogVM(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
            Priorities = typeof(Priority).EnumToDataSource();
        }

        public List<KeyValue> Priorities { get; set; }

        /// <summary>
        /// 轴对象
        /// </summary>
        public ObservableCollection<VAxis> _axisDatas;
        public ObservableCollection<VAxis> AxisDatas
        {
            get { return _axisDatas; }
            set { SetProperty(ref _axisDatas, value); }
        }

        /// <summary>
        /// 当前选中项
        /// </summary>
        private VAxis _selectItem;
        public VAxis SelectedItem
        {
            get { return _selectItem; }
            set { SetProperty(ref _selectItem, value); }
        }

        /// <summary>
        /// 当前选中项
        /// </summary>
        private MultiAxisModel _curItem;
        public MultiAxisModel CurrentItem
        {
            get { return _curItem; }
            set { SetProperty(ref _curItem, value); }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            // 1.初始化所有轴
            AxisDatas = new ObservableCollection<VAxis>();
            var axises = _deviceEngine.GetDevices(typeof(VAxis));
            foreach (var item in axises)
            {
                AxisDatas.Add(item as VAxis);
            }

            // 2.初始化已有的轴
            if (parameters.TryGetValue<VAxisDevice>("VAxis", out var device))
            {
                if (device == null) return;
               
                SelectedItem = AxisDatas.FirstOrDefault(u => u.ID == device.DeviceID);

                tokenS?.Cancel();
                tokenS = new CancellationTokenSource();
                CurrentItem = new MultiAxisModel(SelectedItem, tokenS);
                CurrentItem.Position = device.Position;
                CurrentItem.Speed = (int)device.Speed;
            }
        }

        /// <summary>
        /// 轴添加到集合中
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((vm) =>
        {
            SelectedItem = vm as VAxis;
            tokenS?.Cancel();
            tokenS = new CancellationTokenSource();
            CurrentItem = new MultiAxisModel(SelectedItem, tokenS);
        }));

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            // 1.构建VAxisM对象
            if (CurrentItem == null)
            {
                return;
            }

            var vAxis = new VAxisDevice()
            { DeviceID = CurrentItem.AxisID, Name = CurrentItem.Name, Position = CurrentItem.Position, Speed = CurrentItem.Speed };

            // 添加到对象
            result.Parameters.Add("VAxisDevice", vAxis);
            tokenS?.Cancel();
        }

        protected override void Cancel(IDialogResult result)
        {
            base.Cancel(result);
            tokenS?.Cancel();
        }

        protected override void No(IDialogResult result)
        {
            base.No(result);
            tokenS?.Cancel();
        }
    }
}