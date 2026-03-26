using Luster.Common.Assets;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VFlyingPhotoContentVM : PageVM
    {
        protected VFlyingPhotoContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            TriggerNoTypes = typeof(TriggerNoType).EnumToDataSource();
            LoadDevices();
        }

        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        private VFlyingPhoto flyingPhoto;

        /// <summary>
        /// 显示E4O4模块
        /// </summary>
        private bool _isShowE4;
        public bool IsShowE4
        {
            get { return _isShowE4; }
            set { SetProperty(ref _isShowE4, value); }
        }

        private List<KeyValue> _triggerNoTypes;
        /// <summary>
        /// 触发通道类型
        /// </summary>
        public List<KeyValue> TriggerNoTypes
        {
            get => _triggerNoTypes;
            set
            {
                SetProperty(ref _triggerNoTypes, value);
            }
        }

        /// <summary>
        /// 当前触发通道类型
        /// </summary>
        private TriggerNoType _curTriggerNoType = TriggerNoType.KickDut;
        public TriggerNoType CurTriggerNoType
        {
            get { return _curTriggerNoType; }
            set
            {
                SetProperty(ref _curTriggerNoType, value);
            }
        }


        /// <summary>
        /// 添加设备
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowVFlyingPhotoDialog(r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<VFlyingPhoto>("VFlyingPhoto", out var vFP))
                    {
                        deviceEngine.AddVirtual(vFP);
                        LoadDevices();
                    }
                }
            });
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices()
        {
            //加载设备
            var devices = deviceEngine.GetDevices(typeof(VFlyingPhoto));
            VFlyingPhotoModels = new ObservableCollection<VFlyingPhotoModel>();
            foreach (var device in devices)
            {
                var item = device as VFlyingPhoto;
                var vFlyingPhotoModel = new VFlyingPhotoModel(item);
                vFlyingPhotoModel.FlyingPhotoName = item.GetDevice().Name;
                VFlyingPhotoModels.Add(vFlyingPhotoModel);
            }

            //加载轴
            var axisDevices = deviceEngine.GetDevices(typeof(VAxis));
            AxisList = new ObservableCollection<AxisModel>();
            foreach (var device in axisDevices)
            {
                var vAxis = device as VAxis;
                var axisModel = new AxisModel(vAxis);
                AxisList.Add(axisModel);
            }
        }

        /// <summary>
        /// 加载示教点位
        /// </summary>
        private void LoadTeachPoint()
        {
            if (Current != null)
            {
                FlyingPhotoPointItems = new ObservableCollection<FlyingPhotoPointItem>();
                var positions = Current.Tag.Positions;
                foreach (var node in positions)
                {
                    var position = new FlyingPhotoPointItem()
                    {
                        Name = node.Name,
                        Position = node.Position,
                        EncoderNo = node.EncoderNo,
                        TriggerNo = node.TriggerNo,
                        Tag = node
                    };
                    FlyingPhotoPointItems.Add(position);
                }
            }
        }

        /// <summary>
        /// 飞拍模块列表
        /// </summary>
        private ObservableCollection<VFlyingPhotoModel> _vFlyingPhotoModels;
        public ObservableCollection<VFlyingPhotoModel> VFlyingPhotoModels
        {
            get { return _vFlyingPhotoModels; }
            set { SetProperty(ref _vFlyingPhotoModels, value); }
        }

        /// <summary>
        /// 当前选择的飞拍模块
        /// </summary>
        private VFlyingPhotoModel _current;
        public VFlyingPhotoModel Current
        {
            get => _current;
            set
            {
                SetProperty(ref _current, value);
            }
        }


        private int _triggerNoNum = 1;
        /// <summary>
        /// 触发输出通道数量
        /// </summary>
        public int TriggerNoNum
        {
            get => _triggerNoNum;
            set
            {
                SetProperty(ref _triggerNoNum, value);
                UpdateTriggerNoShow();
            }
        }

        private int _encoderNo;
        /// <summary>
        /// 编码器通道号
        /// </summary>
        public int EncoderNo
        {
            get => _encoderNo;
            set
            {
                SetProperty(ref _encoderNo, value);
            }
        }

        private int _triggerNo;
        /// <summary>
        /// 触发输出通道
        /// </summary>
        public int TriggerNo
        {
            get => _triggerNo;
            set
            {
                SetProperty(ref _triggerNo, value);
            }
        }

        private int _lCTNo;
        /// <summary>
        /// 锁存通道
        /// </summary>
        public int LCTNo
        {
            get => _lCTNo;
            set => SetProperty(ref _lCTNo, value);
        }

        private int _pulseWidth = 100000;
        /// <summary>
        /// 输出脉宽
        /// </summary>
        public int PulseWidth
        {
            get => _pulseWidth;
            set => SetProperty(ref _pulseWidth, value);
        }

        private int _cameraOffsetPulse;
        /// <summary>
        /// 偏移距离
        /// </summary>
        public int CameraOffsetPulse
        {
            get => _cameraOffsetPulse;
            set => SetProperty(ref _cameraOffsetPulse, value);
        }


        /// <summary>
        /// 点位列表
        /// </summary>
        private List<KeyValue> _points;
        public List<KeyValue> Points
        {
            get => _points;
            set
            {
                SetProperty(ref _points, value);
            }
        }

        /// <summary>
        /// 飞拍点位
        /// </summary>
        private long _encoderValue;
        public long EncoderValue
        {
            get => _encoderValue;
            set => SetProperty(ref _encoderValue, value);
        }

        #region 命令

        private DelegateCommand<VFlyingPhotoModel> selectedCommand;
        /// <summary>
        /// 飞拍模块选择
        /// </summary>
        public DelegateCommand<VFlyingPhotoModel> SelectedCommand => selectedCommand ?? (selectedCommand = new DelegateCommand<VFlyingPhotoModel>((item) =>
        {
            Current = item;

            if (item != null)
            {
                var device = deviceEngine.GetVirtualByID(item.ID);
                flyingPhoto = device as VFlyingPhoto;

                IsShowE4 = (flyingPhoto.GetDevice() as IDevice).Brand == "凌臣飞拍E4O4";

                if (flyingPhoto != null)
                {
                    LoadTeachPoint();
                }
            }
            else
            {
                PropertyObj = new object();
            }
        }));


        private DelegateCommand<VFlyingPhotoModel> _removeCommand;
        /// <summary>
        /// 飞拍模块删除
        /// </summary>
        public DelegateCommand<VFlyingPhotoModel> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<VFlyingPhotoModel>((item) =>
        {
            dialogService.ShowConfirm($"确认删除飞拍模块:{item.Name}", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    deviceEngine.ReomoveVirtual(item.ID);
                    var model = VFlyingPhotoModels.FirstOrDefault(u => u.ID == item.ID);
                    if (model != null)
                    {
                        if (model.ID == Current.ID)
                        {
                            Current = null;
                        }
                        VFlyingPhotoModels.Remove(model);
                    }
                }
            });
        }));




        private DelegateCommand connectCommand;
        /// <summary>
        /// 连接总线
        /// </summary>
        public DelegateCommand ConnectCommand => connectCommand ?? (connectCommand = new DelegateCommand(() =>
        {
            //连接机器人
            if (flyingPhoto != null)
            {
                //flyingPhoto.Connect();

                //更新当前速率
                //SpeedCoefficient = _vRobot.GetSpeedCoefficient();

                //开启监控
                StartMonitor();
            }
        }));

        private DelegateCommand _clearEncoderDataCommand;
        /// <summary>
        /// 当前编码器值清零
        /// </summary>
        public DelegateCommand ClearEncoderDataCommand => _clearEncoderDataCommand ?? (_clearEncoderDataCommand = new DelegateCommand(() =>
        {
            //连接机器人
            if (flyingPhoto != null)
            {
                flyingPhoto.ClearEncoderData();
            }
        }));


        private DelegateCommand _setParmCommand;
        /// <summary>
        /// 设置参数
        /// </summary>
        public DelegateCommand SetParmCommand => _setParmCommand ?? (_setParmCommand = new DelegateCommand(() =>
        {
            if (flyingPhoto != null)
            {
                if(!IsShowE4)
                {
                    if (CurTriggerNoType == TriggerNoType.KickDut)
                    {
                        flyingPhoto.SetTriggerParm(TriggerNo, -1, 1);
                    }
                    else
                    {
                        flyingPhoto.SetTriggerParm(TriggerNo, LCTNo, 0);
                        flyingPhoto.SetTriggerCameraOffset(TriggerNo, CameraOffsetPulse);
                    }

                    flyingPhoto.SetPulseWidth(TriggerNo, PulseWidth);
                }
                else
                {

                }

            }
        }));


        private ObservableCollection<FlyingPhotoPointItem> _flyingPhotoPointItem;
        /// <summary>
        /// 飞拍点位列表
        /// </summary>
        public ObservableCollection<FlyingPhotoPointItem> FlyingPhotoPointItems
        {
            get { return _flyingPhotoPointItem; }
            set { SetProperty(ref _flyingPhotoPointItem, value); }
        }

        #region 示教点位相关操作

        /// <summary>
        /// 示教
        /// </summary>
        private DelegateCommand<VFlyingPhotoModel> _teachCommand;
        public DelegateCommand<VFlyingPhotoModel> TeachCommand => _teachCommand ?? (_teachCommand = new DelegateCommand<VFlyingPhotoModel>((flyingPhoto) =>
        {
            if (flyingPhoto == null) return;

            dialogService.ShowTeachFlyingPhotoPointDialog(EncoderValue, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<string>("Name", out var name) &&
                     r.Parameters.TryGetValue<long>("Position", out var position) &&
                     r.Parameters.TryGetValue<int>("EncoderNo", out var encoderNo) &&
                     r.Parameters.TryGetValue<int>("TriggerNo", out var triggerNo))
                    {
                        flyingPhoto.Tag.AddPosition(name, position, triggerNo, encoderNo);
                        var src = flyingPhoto.Tag.Positions.FirstOrDefault(u => u.Name == name);

                        FlyingPhotoPointItems.Add(new FlyingPhotoPointItem()
                        {
                            Name = name,
                            Position = position,
                            EncoderNo = encoderNo,
                            TriggerNo = triggerNo,
                            Tag = src
                        });
                    }
                }
            });
        }));



        private DelegateCommand<FlyingPhotoPointItem> _moveTeachCommand;
        /// <summary>
        /// 点位运动指令
        /// </summary>
        public DelegateCommand<FlyingPhotoPointItem> MoveTeachCommand => _moveTeachCommand ?? (_moveTeachCommand = new DelegateCommand<FlyingPhotoPointItem>((pos) =>
        {
            if (pos == null) return;
            Task.Run(() =>
            {
                //pos.Tag.FlyingPhoto.MovP(CurrentPointIndex);
                double cmdPos = pos.Position / CurAxisModel.Tag.PerPluse;
                CurAxisModel.Tag.MoveAbs(pos.Position);
            });

        }));


        private DelegateCommand<FlyingPhotoPointItem> _updateTeachCommand;
        /// <summary>
        /// 更新写入点位
        /// </summary>
        public DelegateCommand<FlyingPhotoPointItem> UpdateTeachCommand => _updateTeachCommand ?? (_updateTeachCommand = new DelegateCommand<FlyingPhotoPointItem>((pos) =>
        {
            if (pos == null) return;

            dialogService.ShowConfirm($"确认将点位:{pos.Name}的值进行变更:{pos.Position}->{EncoderValue}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    pos.Position = EncoderValue;
                    pos.Tag.Position = EncoderValue;

                    // 代表有更新
                    deviceEngine.IsNeedSave = true;
                }
            });

        }));

        private DelegateCommand<FlyingPhotoPointItem> _removePositionCommand;
        /// <summary>
        /// 删除示教点位
        /// </summary>
        public DelegateCommand<FlyingPhotoPointItem> RemovePositionCommand => _removePositionCommand ?? (_removePositionCommand = new DelegateCommand<FlyingPhotoPointItem>((item) =>
        {
            if (Current != null)
            {

                dialogService.ShowConfirm($"确认将点位:{item.Name}删除?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var flyingPhotoPos = Current.Tag.Positions.FirstOrDefault(u => u.Name == item.Name);
                        flyingPhoto.RemovePostion(flyingPhotoPos.Name);

                        FlyingPhotoPointItems.Remove(item);

                        // 代表有更新
                        deviceEngine.IsNeedSave = true;
                    }
                });


            }
        }));



        #endregion

        #endregion

        /// <summary>
        /// 开启监控
        /// </summary>
        private void StartMonitor()
        {
            if (flyingPhoto == null) return;

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

                    // 更新触发次数信息
                    UpdateTriggerNoShow();

                    //更新点位
                    long value = 0;
                    Current.Tag.GetEncoderCurData(out value);
                    EncoderValue = value;
                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        break;
                    }



                    Thread.Sleep(100);
                    //Debug.WriteLine("123");
                }
            }, whileToken.Token);

        }

        private FlyingPhotoPointItem _currentFlyingPhotoPoint;
        /// <summary>
        /// 当前点位
        /// </summary>
        public FlyingPhotoPointItem CurrentFlyingPhotoPoint
        {
            get { return _currentFlyingPhotoPoint; }
            set { SetProperty(ref _currentFlyingPhotoPoint, value); }
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
                else if (sArgs.AddedItems[0] is FlyingPhotoPointItem p)
                {
                    CurrentFlyingPhotoPoint = p;
                }
            }
            else
            {
                CurrentFlyingPhotoPoint = null;
                Leave();
            }
        }));

        private ObservableCollection<TriggerNoCountModel> _triggerNoCountModels;
        /// <summary>
        /// 触发通道计数
        /// </summary>
        public ObservableCollection<TriggerNoCountModel> TriggerNoCountModels
        {
            get { return _triggerNoCountModels; }
            set { SetProperty(ref _triggerNoCountModels, value); }
        }

        /// <summary>
        /// 更新输出通道触发计数
        /// </summary>
        public void UpdateTriggerNoShow()
        {
            int count = 0;
            TriggerNoCountModels = new ObservableCollection<TriggerNoCountModel>();
            for (int i = 0; i < _triggerNoNum; i++)
            {
                TriggerNoCountModel triggerNoCountModel = new TriggerNoCountModel();
                triggerNoCountModel.TriggerNo = i;

                count = 0;
                flyingPhoto?.GetTriggerCount(i, ref count);
                triggerNoCountModel.TriggerNoCount = count;

                count = 0;
                flyingPhoto?.GetTriggerFifoCount(i, CurTriggerNoType == TriggerNoType.KickDut ? 1 : 0, ref count);
                triggerNoCountModel.TriggerNoFifoCount = count;

                TriggerNoCountModels.Add(triggerNoCountModel);
            }

        }


        #region 轴的相关操作

        /// <summary>
        /// 轴列表
        /// </summary>
        private ObservableCollection<AxisModel> axisList;
        public ObservableCollection<AxisModel> AxisList
        {
            get { return axisList; }
            set { SetProperty(ref axisList, value); }
        }

        private AxisModel _curAxisModel;
        public AxisModel CurAxisModel
        {
            get => _curAxisModel;
            set
            {
                SetProperty(ref _curAxisModel, value);
            }
        }

        /// <summary>
        /// 运动距离
        /// </summary>
        private double _movePos = 100;
        public double MovePos
        {
            get { return _movePos; }
            set { SetProperty(ref _movePos, value); }
        }


        #region 轴的绝对运动、相对运动和回零
        /// <summary>
        /// 绝对运动
        /// </summary>
        private DelegateCommand<AxisModel> _moveAbsCommand;
        public DelegateCommand<AxisModel> MoveAbsCommand => _moveAbsCommand ?? (_moveAbsCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                Task.Run(() =>
                {
                    axis.Tag.MoveAbs(MovePos);
                    axis.Tag.CheckMotionDone();
                });
            }
        }));

        /// <summary>
        /// 相对运动
        /// </summary>
        private DelegateCommand<AxisModel> _moveRelCommand;
        public DelegateCommand<AxisModel> MoveRelCommand => _moveRelCommand ?? (_moveRelCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                Task.Run(() =>
                {
                    axis.Tag.MoveRel(MovePos);
                    axis.Tag.CheckMotionDone();
                });
            }
        }));

        /// <summary>
        /// 回零
        /// </summary>
        private DelegateCommand<AxisModel> _homeCommand;
        public DelegateCommand<AxisModel> HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                Task.Run(() =>
                {
                    axis.Tag.Home();
                    axis.Tag.CheckHomeDone();

                    //飞拍模块设置当前编码器值为0
                    Current.Tag.ClearEncoderData();

                });
            }
        }));

        #region JOG运动模式


        /// <summary>
        /// JOG+点动
        /// </summary>
        private DelegateCommand<AxisModel> _jogCommandAdd;
        public DelegateCommand<AxisModel> JogCommandAdd => _jogCommandAdd ?? (_jogCommandAdd = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.Jog(true, axis.JogSpeed);
            }
        }));

        /// <summary>
        /// JOG-点动
        /// </summary>
        private DelegateCommand<AxisModel> _jogCommandSub;
        public DelegateCommand<AxisModel> JogCommandSub => _jogCommandSub ?? (_jogCommandSub = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.Jog(false, axis.JogSpeed);
            }
        }));

        /// <summary>
        /// 轴停止
        /// </summary>
        private DelegateCommand<AxisModel> _stopCommand;
        public DelegateCommand<AxisModel> StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                // 显示对象
                axis.Tag.Stop();
            }
        }));

        #endregion


        #endregion


        #region 轴的使能

        private DelegateCommand<AxisModel> _enableCommand;
        public DelegateCommand<AxisModel> EnableCommand => _enableCommand ?? (_enableCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.ServOn(true);
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
        /// 轴清错
        /// </summary>
        private DelegateCommand<AxisModel> _clearErrorCommand;
        public DelegateCommand<AxisModel> ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                // 显示对象
                axis.Tag.ResetStatus();
            }
        }));

        #endregion

        #region 页面离开

        public override void Leave()
        {
            whileToken?.Cancel();
            Thread.Sleep(200);
            whileToken = null;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            Leave();
        }

        #endregion

    }

    public enum TriggerNoType
    {
        [Description("相机")]
        Camera,

        [Description("踢料")]
        KickDut
    }
}
