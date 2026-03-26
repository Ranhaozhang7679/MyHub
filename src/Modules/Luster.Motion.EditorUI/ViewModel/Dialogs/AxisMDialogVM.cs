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
    public class AxisMDialogVM : MotionDialogVM
    {
        private IDeviceEngine _deviceEngine;

        /// <summary>
        /// Token
        /// </summary>
        private CancellationTokenSource tokenS;

        public AxisMDialogVM(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
            Priorities = typeof(Priority).EnumToDataSource();
            Directions = typeof(MoveDirection).EnumToDataSource();
        }

        public List<KeyValue> Priorities { get; set; }

        public List<KeyValue> Directions { get; set; }

        /// <summary>
        /// 轴对象
        /// </summary>
        public ObservableCollection<VAxisM> _axisDatas;
        public ObservableCollection<VAxisM> AxisDatas
        {
            get { return _axisDatas; }
            set { SetProperty(ref _axisDatas, value); }
        }

        /// <summary>
        /// 已有轴对象
        /// </summary>
        public ObservableCollection<MultiAxisModel> _hasAxisData;
        public ObservableCollection<MultiAxisModel> HasAxisDatas
        {
            get { return _hasAxisData; }
            set { SetProperty(ref _hasAxisData, value); }
        }

        /// <summary>
        /// 皮带运动方向
        /// </summary>
        private MoveDirection moveDirection;
        public MoveDirection MoveDirection
        {
            get { return moveDirection; }
            set { SetProperty(ref moveDirection, value); }
        }


        /// <summary>
        /// 当前选中项
        /// </summary>
        private VAxisM _curItem;
        public VAxisM CurrentItem
        {
            get { return _curItem; }
            set { SetProperty(ref _curItem, value); }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            MoveDirection = MoveDirection.Positive;
            // 1.初始化所有轴
            AxisDatas = new ObservableCollection<VAxisM>();
            var axises = _deviceEngine.GetDevices(typeof(VAxisM));
            foreach (var item in axises)
            {
                AxisDatas.Add(item as VAxisM);
            }


            // 2.初始化已有的轴
            HasAxisDatas = new ObservableCollection<MultiAxisModel>();
            if (parameters.TryGetValue<VAxisMDevice>("VAxisM", out var device))
            {
                if (device == null) return;

                // 更新运动方向
                MoveDirection = device.MoveDirection;

                CurrentItem = AxisDatas.FirstOrDefault(u => u.ID == device.DeviceID);

                tokenS?.Cancel();
                tokenS = new CancellationTokenSource();
                if (CurrentItem != null)
                {
                    // 添加到已有轴集合中
                    foreach (var aItem in device.Items)
                    {
                        var vAxisItem = CurrentItem.Axises.FirstOrDefault(u => u.AxisID == aItem.AxisID);
                        aItem.Axis = vAxisItem.Axis;
                        HasAxisDatas.Add(new MultiAxisModel(aItem, tokenS));
                    }
                }
            }
        }

        /// <summary>
        /// 多轴运动
        /// </summary>
        private DelegateCommand<string> _axisCommand;
        public DelegateCommand<string> AxisCommand => _axisCommand ?? (_axisCommand = new DelegateCommand<string>((type) =>
        {
            if (CurrentItem != null)
            {
                var vAxisM = NewAxisMDevice();
                if (type == "Move")
                {
                    CurrentItem.Move(vAxisM);
                }
                else if (type == "Home")
                {
                    CurrentItem.Home();
                }
                else if (type == "JOG")
                {
                    CurrentItem.Jog(vAxisM);
                    CurrentItem.VStatus = VStatus.Idle;
                }
                else if (type == "Stop")
                {
                    CurrentItem.Stop();
                }
            }
        }));

        /// <summary>
        /// 轴添加到集合中
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((vm) =>
        {
            HasAxisDatas.Clear();
            tokenS?.Cancel();
            tokenS = new CancellationTokenSource();
            foreach (var item in CurrentItem.Axises)
            {
                HasAxisDatas.Add(new MultiAxisModel(item, tokenS));
            }
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

            var vAxisM = NewAxisMDevice();

            // 添加到对象
            result.Parameters.Add("VAxisMDevice", vAxisM);

            tokenS?.Cancel();
        }

        private VAxisMDevice NewAxisMDevice()
        {
            List<AxisItem> items = new List<AxisItem>();
            foreach (var item in HasAxisDatas)
            {
                AxisItem axisItem = new AxisItem()
                {
                    AxisID = item.AxisID,
                    Axis = item.Current,
                    MovePriority = item.Priority,
                    Position = item.Position,
                    Speed = item.Speed
                };

                items.Add(axisItem);
            }

            VAxisMDevice vAxisM = new VAxisMDevice() { DeviceID = CurrentItem.ID, Name = CurrentItem.Name, Items = items, MoveDirection = MoveDirection };

            return vAxisM;
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