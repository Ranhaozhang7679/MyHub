#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MultiAxisModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       MultiAxisModel.cs
* 创建时间:       2022/7/7 12:19:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      714eddb7-052a-47d2-a8e1-d169b79fce10
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/7 12:19:22
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Luster.Motion.EditorUI.Models
{
    /// <summary>
    /// 多轴
    /// </summary>
    public class MultiAxisModel : BindableBase
    {
        /// <summary>
        /// 属性变更
        /// </summary>
        public event Action<string, object, object> AxisPropertyChanged;

        public string Name { get; set; }

        /// <summary>
        /// <summary>
        /// 位置
        /// </summary>
        private double _position;
        public double Position
        {
            get { return _position; }
            set
            {
                SetProperty(ref _position, value);
            }
        }

        /// <summary>
        /// 实时位置
        /// </summary>
        private double _currentPos;
        public double CurrentPos
        {
            get { return _currentPos; }
            set { SetProperty(ref _currentPos, value); }
        }

        /// <summary>
        /// 目标位置
        /// </summary>
        private double _dstPostion = 0;
        public double DstPosition
        {
            get { return _dstPostion; }
            set { SetProperty(ref _dstPostion, value); }
        }

        /// <summary>
        /// 速度
        /// </summary>
        private double _speed;
        public double Speed
        {
            get { return _speed; }
            set
            {
                double srcVal = _speed;
                SetProperty(ref _speed, value);
                if (currentItem != null)
                    currentItem.Speed = value;

                // 单轴
                if (axisItem != null)
                {
                    axisItem.Speed = value;
                }

                // 点位
                if (axisPosition != null)
                {
                    axisPosition.Speed = value;
                }

                if (srcVal != value)
                    AxisPropertyChanged?.Invoke(nameof(Speed), srcVal, value);


            }
        }

        /// <summary>
        /// 优先级
        /// </summary>
        private Priority _priority;
        public Priority Priority
        {
            get { return _priority; }
            set
            {
                var srcVal = _priority;

                SetProperty(ref _priority, value);
                if (currentItem != null)
                    currentItem.MovePriority = value;

                // 点位
                if (axisPosition != null)
                {
                    axisPosition.MovePriority = value;
                }

                if (srcVal != value)
                    AxisPropertyChanged?.Invoke(nameof(Priority), srcVal, value);
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }


        /// <summary>
        /// 设置运行状态
        /// </summary>
        public List<AxisStatusModel> StatusList { get; set; }

        /// <summary>
        /// 轴ID
        /// </summary>
        public Guid AxisID { get; set; }

        /// <summary>
        /// 当前对象
        /// </summary>
        private AxisItem currentItem;

        /// <summary>
        /// 单轴
        /// </summary>
        private VAxisDevice axisItem;

        /// <summary>
        /// 点位运动
        /// </summary>
        private AxisPosItem axisPosition;

        public MultiAxisModel(AxisItem axis, CancellationTokenSource token)
        {
            currentItem = axis;
            Position = axis.Position;
            Speed = axis.Speed;
            Priority = axis.MovePriority;
            AxisID = axis.AxisID;
            Current = axis.Axis;
            Name = Current.Name;

            DstPosition = axis.Position;

            StatusList = new List<AxisStatusModel>()
            {
                new AxisStatusModel(AxisStatus.Alarm,false),
                new AxisStatusModel(AxisStatus.Pel,false),
                new AxisStatusModel(AxisStatus.Mel,false),
                new AxisStatusModel(AxisStatus.Org,false),
                new AxisStatusModel(AxisStatus.Emg,false),
                new AxisStatusModel(AxisStatus.Moving,false),
                new AxisStatusModel(AxisStatus.OnPos,false),
                new AxisStatusModel(AxisStatus.SvOn,false),
            };

            StartMonitor(token);
        }

        public VAxis Current { get; set; }

        public MultiAxisModel(VAxis vAxis, CancellationTokenSource token)
        {
            Position = 0;
            Speed = vAxis.MoveSpeed;
            Priority = Priority.Low;
            AxisID = vAxis.ID;
            Current = vAxis;
            Name = Current.Name;

            StatusList = new List<AxisStatusModel>()
            {
                new AxisStatusModel(AxisStatus.Alarm,false),
                new AxisStatusModel(AxisStatus.Pel,false),
                new AxisStatusModel(AxisStatus.Mel,false),
                new AxisStatusModel(AxisStatus.Org,false),
                new AxisStatusModel(AxisStatus.Emg,false),
                new AxisStatusModel(AxisStatus.Moving,false),
                new AxisStatusModel(AxisStatus.OnPos,false),
                new AxisStatusModel(AxisStatus.SvOn,false),
            };

            // 实时刷新位置信息以及状态
            StartMonitor(token);
        }

        public MultiAxisModel(VAxisDevice vDevice, CancellationTokenSource token)
        {
            Position = vDevice.Position;
            Speed = vDevice.Speed;
            Priority = Priority.Low;
            AxisID = vDevice.DeviceID;
            axisItem = vDevice;
            Current = vDevice.Axis;
            Name = Current.Name;

            StatusList = new List<AxisStatusModel>()
            {
                new AxisStatusModel(AxisStatus.Alarm,false),
                new AxisStatusModel(AxisStatus.Pel,false),
                new AxisStatusModel(AxisStatus.Mel,false),
                new AxisStatusModel(AxisStatus.Org,false),
                new AxisStatusModel(AxisStatus.Emg,false),
                new AxisStatusModel(AxisStatus.Moving,false),
                new AxisStatusModel(AxisStatus.OnPos,false),
                new AxisStatusModel(AxisStatus.SvOn,false),
            };

            // 实时刷新位置信息以及状态
            StartMonitor(token);
        }

        public MultiAxisModel(AxisPosItem aPos, CancellationTokenSource token)
        {
            Position = aPos.Position;
            Speed = aPos.Speed;
            Priority = aPos.MovePriority;
            AxisID = aPos.Axis.ID;
            Current = aPos.Axis;
            Name = Current.Name;
            IsEnabled = false;
            axisPosition = aPos;

            StatusList = new List<AxisStatusModel>()
            {
                new AxisStatusModel(AxisStatus.Alarm,false),
                new AxisStatusModel(AxisStatus.Pel,false),
                new AxisStatusModel(AxisStatus.Mel,false),
                new AxisStatusModel(AxisStatus.Org,false),
                new AxisStatusModel(AxisStatus.Emg,false),
                new AxisStatusModel(AxisStatus.Moving,false),
                new AxisStatusModel(AxisStatus.OnPos,false),
                new AxisStatusModel(AxisStatus.SvOn,false),
            };

            // 实时刷新位置信息以及状态
            StartMonitor(token);
        }

        /// <summary>
        /// 绝对运动
        /// </summary>
        private DelegateCommand _moveAbsCommand;
        public DelegateCommand MoveAbsCommand => _moveAbsCommand ?? (_moveAbsCommand = new DelegateCommand(() =>
        {
            Current.MoveAbs(DstPosition, Speed);
        }));

        /// <summary>
        /// 相对运动
        /// </summary>
        private DelegateCommand _moveRelCommand;
        public DelegateCommand MoveRelCommand => _moveRelCommand ?? (_moveRelCommand = new DelegateCommand(() =>
        {
            Current.MoveRel(DstPosition, Speed);
        }));

        /// <summary>
        /// 程序应用
        /// </summary>
        private DelegateCommand<object> _applyCommand;
        public DelegateCommand<object> ApplyCommand => _applyCommand ?? (_applyCommand = new DelegateCommand<object>((obj) =>
        {
            AxisPropertyChanged?.Invoke(nameof(Position), Position, CurrentPos);
            Position = CurrentPos;
            // 针对单轴每一个项进行赋值
            if (obj is MultiAxisModel m && m.axisItem != null)
            {
                m.axisItem.Position = CurrentPos;
            }
            // 针对多轴每一个项进行赋值
            if (currentItem != null)
            {
                currentItem.Position = CurrentPos;
            }

            if (axisPosition != null)
            {
                axisPosition.AxisPostion.Position = CurrentPos;
            }
        }));

        /// <summary>
        /// 点位示教
        /// </summary>
        private DelegateCommand<object> _teachCommand;
        public DelegateCommand<object> TeachCommand => _teachCommand ?? (_teachCommand = new DelegateCommand<object>((obj) =>
        {
            Position = DstPosition;
        }));

        /// <summary>
        /// 更新轴状态
        /// </summary>
        private void StartMonitor(CancellationTokenSource tokenS)
        {
            if (Current == null || Current.AxisType == AxisType.None) return;

            Task.Run(() =>
            {
                while (true)
                {
                    // 外部取消后
                    if (tokenS.IsCancellationRequested)
                    {
                        break;
                    }
                    // 更新状态信息
                    var dictStatus = Current.GetAxisStatus(false);
                    foreach (var sItem in StatusList)
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

                    CurrentPos = Current.GetCurrentPos();
                    Thread.Sleep(100);
                }
            }, tokenS.Token);
        }

        /// <summary>
        /// 回零
        /// </summary>
        private DelegateCommand _homeCommand;
        public DelegateCommand HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand(() =>
        {
            Task.Run(() => { Current.Home(); Current.CheckHomeDone(); });
        }));

        #region JOG运动模式
        /// <summary>
        /// JOG+点动
        /// </summary>
        private DelegateCommand _jogCommandAdd;
        public DelegateCommand JogCommandAdd => _jogCommandAdd ?? (_jogCommandAdd = new DelegateCommand(() =>
        {
            Current.Jog(true, Speed);
        }));

        /// <summary>
        /// JOG-点动
        /// </summary>
        private DelegateCommand _jogCommandSub;
        public DelegateCommand JogCommandSub => _jogCommandSub ?? (_jogCommandSub = new DelegateCommand(() =>
        {
            Current.Jog(false, Speed);
        }));

        /// <summary>
        /// 停止运动
        /// </summary>
        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand(() =>
        {
            // 显示对象
            Current.Stop();
        }));

        #endregion

        /// <summary>
        /// 清理错误信息
        /// </summary>
        private DelegateCommand _clearErrorCommand;
        public DelegateCommand ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand(() =>
        {
            // 显示对象
            Current.ResetStatus();
        }));
    }

    /// <summary>
    /// 轴状态
    /// </summary>
    public class AxisStatusModel : BindableBase
    {
        /// <summary>
        /// 模组名称
        /// </summary>
        private AxisStatus _name;
        public AxisStatus Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 模组名称
        /// </summary>
        private string _tips;
        public string Tips { get => _tips; set => SetProperty(ref _tips, value); }

        /// <summary>
        /// 模组名称
        /// </summary>
        private string _alias;
        public string Alias { get => _alias; set => SetProperty(ref _alias, value); }


        /// <summary>
        /// 模组名称
        /// </summary>
        private bool _status;
        public bool Status { get => _status; set => SetProperty(ref _status, value); }


        public AxisStatusModel(AxisStatus status, bool isPass)
        {
            Name = status;
            Status = isPass;
            switch (status)
            {
                case AxisStatus.Alarm:
                    Alias = "ALM";
                    break;
                case AxisStatus.Pel:
                    Alias = "PEL";
                    break;
                case AxisStatus.Mel:
                    Alias = "AEL";
                    break;
                case AxisStatus.Org:
                    Alias = "ORG";
                    break;
                case AxisStatus.Emg:
                    Alias = "EMG";
                    break;
                case AxisStatus.Moving:
                    Alias = "MOV";
                    break;
                case AxisStatus.OnPos:
                    Alias = "ONT";
                    break;
                case AxisStatus.SvOn:
                    Alias = "ON";
                    break;
            }
        }

    }
}