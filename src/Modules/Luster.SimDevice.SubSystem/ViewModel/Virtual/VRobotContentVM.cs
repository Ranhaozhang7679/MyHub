using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VRobotContentVM : PageVM
    {
        protected VRobotContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            CurrentRobotPointModels = new ObservableCollection<VRobotPointModel>();
            CurrentSixRobotPointModels = new ObservableCollection<VSixRobotPointModel>();
            CoordTypes = typeof(CoordType).EnumToDataSource();
            RobotRunModes = new List<string>() { "单步运动0.1", "单步运动1", "单步运动5", "连续运动" };
            CurRobotRunMode = RobotRunModes[0];
            UpdateRobotCoordinateName();
            LoadDevices();
        }

        #region 字段
        private VRobot _vRobot;
        public int CurrentPointIndex = -1;
        #endregion

        #region 属性  

        /// <summary>
        /// 坐标系的类型
        /// </summary>
        private List<KeyValue> _coordTypes;
        public List<KeyValue> CoordTypes
        {
            get => _coordTypes;
            set
            {
                SetProperty(ref _coordTypes, value);
            }
        }

        /// <summary>
        /// 当前坐标系
        /// </summary>
        private CoordType _curCoordType = CoordType.Cartesian;
        public CoordType CurCoordType
        {
            get { return _curCoordType; }
            set
            {
                SetProperty(ref _curCoordType, value);
                UpdateRobotCoordinateName();
            }
        }

        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 当前选择的机器人
        /// </summary>
        private VRobotModel _current;
        public VRobotModel Current
        {
            get => _current;
            set
            {
                SetProperty(ref _current, value);
            }
        }

        /// <summary>
        /// 机器人列表
        /// </summary>
        private ObservableCollection<VRobotModel> _vRobotModels;
        public ObservableCollection<VRobotModel> VRobotModels
        {
            get { return _vRobotModels; }
            set { SetProperty(ref _vRobotModels, value); }
        }


        /// <summary>
        /// 机器人点位列表
        /// </summary>

        private List<KeyValue> _robotPoints;
        public List<KeyValue> RobotPoints
        {
            get => _robotPoints;
            set
            {
                SetProperty(ref _robotPoints, value);
            }
        }

        #region 机器人轴名称
        /// <summary>
        /// 机器人轴1+名称
        /// </summary>
        private string _robotAxis1Name;
        public string RobotAxis1Name
        {
            get => _robotAxis1Name;
            set => SetProperty(ref _robotAxis1Name, value);
        }

        /// <summary>
        /// 机器人轴1-名称
        /// </summary>
        private string _robotAxis2Name;
        public string RobotAxis2Name
        {
            get => _robotAxis2Name;
            set => SetProperty(ref _robotAxis2Name, value);
        }

        /// <summary>
        /// 机器人轴2+名称
        /// </summary>
        private string _robotAxis3Name;
        public string RobotAxis3Name
        {
            get => _robotAxis3Name;
            set => SetProperty(ref _robotAxis3Name, value);
        }

        /// <summary>
        /// 机器人轴2-名称
        /// </summary>
        private string _robotAxis4Name;
        public string RobotAxis4Name
        {
            get => _robotAxis4Name;
            set => SetProperty(ref _robotAxis4Name, value);
        }

        /// <summary>
        /// 机器人轴3+名称
        /// </summary>
        private string _robotAxis5Name;
        public string RobotAxis5Name
        {
            get => _robotAxis5Name;
            set => SetProperty(ref _robotAxis5Name, value);
        }

        /// <summary>
        /// 机器人轴3-名称
        /// </summary>
        private string _robotAxis6Name;
        public string RobotAxis6Name
        {
            get => _robotAxis6Name;
            set => SetProperty(ref _robotAxis6Name, value);
        }

        /// <summary>
        /// 机器人轴4+名称
        /// </summary>
        private string _robotAxis7Name;
        public string RobotAxis7Name
        {
            get => _robotAxis7Name;
            set => SetProperty(ref _robotAxis7Name, value);
        }

        /// <summary>
        /// 机器人轴4-名称
        /// </summary>
        private string _robotAxis8Name;
        public string RobotAxis8Name
        {
            get => _robotAxis8Name;
            set => SetProperty(ref _robotAxis8Name, value);
        }

        /// <summary>
        /// 机器人轴5+名称
        /// </summary>
        private string _robotAxis9Name;
        public string RobotAxis9Name
        {
            get => _robotAxis9Name;
            set => SetProperty(ref _robotAxis9Name, value);
        }

        /// <summary>
        /// 机器人轴5-名称
        /// </summary>
        private string _robotAxis10Name;
        public string RobotAxis10Name
        {
            get => _robotAxis10Name;
            set => SetProperty(ref _robotAxis10Name, value);
        }

        /// <summary>
        /// 机器人轴6+名称
        /// </summary>
        private string _robotAxis11Name;
        public string RobotAxis11Name
        {
            get => _robotAxis11Name;
            set => SetProperty(ref _robotAxis11Name, value);
        }

        /// <summary>
        /// 机器人轴6-名称
        /// </summary>
        private string _robotAxis12Name;
        public string RobotAxis12Name
        {
            get => _robotAxis12Name;
            set => SetProperty(ref _robotAxis12Name, value);
        }
        #endregion

        /// <summary>
        /// 机器人运动模式
        /// </summary>
        private List<string> _robotRunModes;
        public List<string> RobotRunModes
        {
            get { return _robotRunModes; }
            set { SetProperty(ref _robotRunModes, value); }
        }

        /// <summary>
        /// 当前机器人运动模式
        /// </summary>
        private string _curRobotRunMode;
        public string CurRobotRunMode
        {
            get { return _curRobotRunMode; }
            set
            {
                SetProperty(ref _curRobotRunMode, value);
                if (_vRobot != null)
                {
                    _vRobot.SetManualMode(GetCurRobotRunMode(_curRobotRunMode));
                }
            }
        }

        private short GetCurRobotRunMode(string msg)
        {
            short mode = 0;
            switch (msg)
            {
                case "单步运动0.1":
                    mode = 0;
                    break;

                case "单步运动1":
                    mode = 1;
                    break;
                case "单步运动5":
                    mode = 2;
                    break;
                case "连续运动":
                    mode = 3;
                    break;
            }
            return mode;
        }

        /// <summary>
        /// 机器人是否为六轴机器人
        /// </summary>
        private bool _isSixRobot;
        public bool IsSixRobot
        {
            get { return _isSixRobot; }
            set
            {
                SetProperty(ref _isSixRobot, value);
            }
        }

        /// <summary>
        /// 机器人运动距离
        /// </summary>
        private double _movePos;
        public double MovePos
        {
            get { return _movePos; }
            set { SetProperty(ref _movePos, value); }
        }

        /// <summary>
        /// 机器人速度系数
        /// </summary>
        private int _speedCoefficient = 1;
        public int SpeedCoefficient
        {
            get { return _speedCoefficient; }
            set
            {
                SetProperty(ref _speedCoefficient, value);
                _vRobot?.SetSpeedCoefficient((short)_speedCoefficient);
            }
        }

        private ObservableCollection<RobotPointItem> _robotPointItems;
        /// <summary>
        /// 机器人坐标系点位列表
        /// </summary>
        public ObservableCollection<RobotPointItem> RobotPointItems
        {
            get { return _robotPointItems; }
            set { SetProperty(ref _robotPointItems, value); }
        }

        private ObservableCollection<SixRobotPointItem> _sixRobotPointItems;
        /// <summary>
        /// 6轴机器人坐标系点位列表
        /// </summary>
        public ObservableCollection<SixRobotPointItem> SixRobotPointItems
        {
            get { return _sixRobotPointItems; }
            set { SetProperty(ref _sixRobotPointItems, value); }
        }


        /// <summary>
        /// 当前点位显示
        /// </summary>
        private ObservableCollection<VRobotPointModel> _currentRobotPointModels;
        public ObservableCollection<VRobotPointModel> CurrentRobotPointModels
        {
            get { return _currentRobotPointModels; }
            set { SetProperty(ref _currentRobotPointModels, value); }
        }

        private ObservableCollection<VSixRobotPointModel> _currentSixRobotPointModels;
        public ObservableCollection<VSixRobotPointModel> CurrentSixRobotPointModels
        {
            get { return _currentSixRobotPointModels; }
            set { SetProperty(ref _currentSixRobotPointModels, value); }
        }

        /// <summary>
        /// 当前点位
        /// </summary>
        private RobotPointItem _currentPoint;
        public RobotPointItem CurrentPoint
        {
            get { return _currentPoint; }
            set { SetProperty(ref _currentPoint, value); }
        }

        private SixRobotPointItem _currentSixPoint;
        public SixRobotPointItem CurrentSixPoint
        {
            get { return _currentSixPoint; }
            set { SetProperty(ref _currentSixPoint, value); }
        }

        #endregion

        #region 命令

        /// <summary>
        /// 机器人选择
        /// </summary>
        private DelegateCommand<VRobotModel> selectedCommand;
        public DelegateCommand<VRobotModel> SelectedCommand => selectedCommand ?? (selectedCommand = new DelegateCommand<VRobotModel>((item) =>
        {
            Current = item;

            if (item != null)
            {
                var device = deviceEngine.GetVirtualByID(item.ID);
                _vRobot = device as VRobot;


                if (_vRobot != null)
                {
                    IsSixRobot = (_vRobot.GetDevice() as IRobot).AxisCount > 4 ? true : false;
                    UpdateRobotCoordinateName();
                    LoadTeachPoint();
                    RobotPoints = Current.Tag.GetCurrentPosion(CurCoordType, false);
                }
            }
            else
            {
                PropertyObj = new object();
            }
        }));


        private DelegateCommand<VRobotModel> _removeCommand;
        /// <summary>
        /// 飞拍模块删除
        /// </summary>
        public DelegateCommand<VRobotModel> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<VRobotModel>((item) =>
        {
            dialogService.ShowConfirm($"确认删除机器人:{item.Name}", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    deviceEngine.ReomoveVirtual(item.ID);
                    var model = VRobotModels.FirstOrDefault(u => u.ID == item.ID);
                    if (model != null)
                    {
                        if (model.ID == Current.ID)
                        {
                            Current = null;
                        }
                        VRobotModels.Remove(model);
                    }
                }
            });
        }));


        /// <summary>
        /// 连接机器人
        /// </summary>
        private DelegateCommand connectRobotCommand;
        public DelegateCommand ConnectRobotCommand => connectRobotCommand ?? (connectRobotCommand = new DelegateCommand(() =>
        {
            //连接机器人
            if (_vRobot != null)
            {
                _vRobot.Connect();

                //更新当前速率
                SpeedCoefficient = _vRobot.GetSpeedCoefficient();

                //开启监控
                StartMonitor();
            }
        }));

        /// <summary>
        /// 断开连接机器人
        /// </summary>
        private DelegateCommand disConnectRobotCommand;
        public DelegateCommand DisConnectRobotCommand => disConnectRobotCommand ?? (disConnectRobotCommand = new DelegateCommand(() =>
        {
            //连接机器人
            if (_vRobot != null)
            {
                _vRobot.DisConnect();
            }
        }));

        /// <summary>
        /// 上使能
        /// </summary>
        private DelegateCommand servoOnCommand;
        public DelegateCommand ServoOnCommand => servoOnCommand ?? (servoOnCommand = new DelegateCommand(() =>
        {
            //上使能
            if (_vRobot != null)
            {
                _vRobot.ServOn(1);
            }
        }));

        /// <summary>
        /// 断开使能
        /// </summary>
        private DelegateCommand servoOffCommand;
        public DelegateCommand ServoOffCommand => servoOffCommand ?? (servoOffCommand = new DelegateCommand(() =>
        {
            //断开使能
            if (_vRobot != null)
            {
                _vRobot.ServOn(0);
            }
        }));

        /// <summary>
        /// 拖拽模式
        /// </summary>
        private DelegateCommand servoDragCommand;
        public DelegateCommand ServoDragCommand => servoDragCommand ?? (servoDragCommand = new DelegateCommand(() =>
        {
            //拖拽模式
            if (_vRobot != null)
            {
                _vRobot.ServOn(2);
            }
        }));

        /// <summary>
        /// 获取运动权限
        /// </summary>
        private DelegateCommand mAcceptCommand;
        public DelegateCommand MAcceptCommand => mAcceptCommand ?? (mAcceptCommand = new DelegateCommand(() =>
        {
            if (_vRobot != null)
            {
                _vRobot.SetMoveAccept(true);
            }
        }));

        /// <summary>
        /// 释放运动权限
        /// </summary>
        private DelegateCommand mReleaseCommand;
        public DelegateCommand MReleaseCommand => mReleaseCommand ?? (mReleaseCommand = new DelegateCommand(() =>
        {
            if (_vRobot != null)
            {
                _vRobot.SetMoveAccept(false);
            }
        }));

        /// <summary>
        /// 清除报警
        /// </summary>
        private DelegateCommand clearErrorCommand;
        public DelegateCommand ClearErrorCommand => clearErrorCommand ?? (clearErrorCommand = new DelegateCommand(() =>
        {
            if (_vRobot != null)
            {
                _vRobot.ClearAlarm();
            }
        }));


        /// <summary>
        /// 手动运动运行
        /// </summary>
        private DelegateCommand<object> preMouseDownCommand;
        public DelegateCommand<object> PreMouseDownCommand => preMouseDownCommand ?? (preMouseDownCommand = new DelegateCommand<object>((obj) =>
        {
            if (_vRobot != null)
            {
                switch (obj?.ToString())
                {
                    case "J1+":
                        _vRobot.ManualMove(1, true);
                        break;
                    case "J1-":
                        _vRobot.ManualMove(2, true);
                        break;
                    case "J2+":
                        _vRobot.ManualMove(3, true);
                        break;
                    case "J2-":
                        _vRobot.ManualMove(4, true);
                        break;
                    case "J3+":
                        _vRobot.ManualMove(5, true);
                        break;
                    case "J3-":
                        _vRobot.ManualMove(6, true);
                        break;
                    case "J4+":
                        _vRobot.ManualMove(7, true);
                        break;
                    case "J4-":
                        _vRobot.ManualMove(8, true);
                        break;

                    case "J5+":
                        _vRobot.ManualMove(9, true);
                        break;
                    case "J5-":
                        _vRobot.ManualMove(10, true);
                        break;

                    case "J6+":
                        _vRobot.ManualMove(11, true);
                        break;
                    case "J6-":
                        _vRobot.ManualMove(12, true);
                        break;


                    case "X+":
                        _vRobot.ManualMove(1, false);
                        break;
                    case "X-":
                        _vRobot.ManualMove(2, false);
                        break;
                    case "Y+":
                        _vRobot.ManualMove(3, false);
                        break;
                    case "Y-":
                        _vRobot.ManualMove(4, false);
                        break;
                    case "Z+":
                        _vRobot.ManualMove(5, false);
                        break;
                    case "Z-":
                        _vRobot.ManualMove(6, false);
                        break;
                    case "U+":
                        _vRobot.ManualMove(7, false);
                        break;
                    case "U-":
                        _vRobot.ManualMove(8, false);
                        break;
                    case "V+":
                        _vRobot.ManualMove(9, false);
                        break;
                    case "V-":
                        _vRobot.ManualMove(10, false);
                        break;
                    case "W+":
                        _vRobot.ManualMove(11, false);
                        break;
                    case "W-":
                        _vRobot.ManualMove(12, false);
                        break;
                }
            }
        }));

        /// <summary>
        /// 手动运动停止
        /// </summary>
        private DelegateCommand<object> preMouseUpCommand;
        public DelegateCommand<object> PreMouseUpCommand => preMouseUpCommand ?? (preMouseUpCommand = new DelegateCommand<object>((obj) =>
        {
            if (_vRobot != null)
            {

                if (CurCoordType == CoordType.Cartesian)
                {

                    _vRobot.ManualMove(0, false);
                }
                else
                {
                    _vRobot.ManualMove(0, true);
                }
            }
        }));

        /// <summary>
        /// 点到点相对运动
        /// </summary>
        private DelegateCommand<object> movPRCommand;
        public DelegateCommand<object> MovPRCommand => movPRCommand ?? (movPRCommand = new DelegateCommand<object>((obj) =>
        {
            //MovPR运动
            if (_vRobot != null)
            {
                switch (obj?.ToString())
                {
                    case "X":
                        _vRobot.MovP(1, false, MovePos);
                        break;
                    case "Y":
                        _vRobot.MovP(2, false, MovePos);
                        break;
                    case "Z":
                        _vRobot.MovP(3, false, MovePos);
                        break;
                    case "C":
                        _vRobot.MovP(4, false, MovePos);
                        break;
                }
            }
        }));

        /// <summary>
        /// 回零运动
        /// </summary>
        private DelegateCommand homeCommand;
        public DelegateCommand HomeCommand => homeCommand ?? (homeCommand = new DelegateCommand(() =>
        {
            //MovP回零运动
            if (_vRobot != null)
            {
                Task.Run(() =>
                {
                    _vRobot.MovP(0);
                });
            }
        }));

        /// <summary>
        /// 停止运动
        /// </summary>
        private DelegateCommand stopCommand;
        public DelegateCommand StopCommand => stopCommand ?? (stopCommand = new DelegateCommand(() =>
        {
            //MovP回零运动
            if (_vRobot != null)
            {
                _vRobot.Stop();
            }
        }));

        /// <summary>
        /// 点到点运动
        /// </summary>
        private DelegateCommand movPCommand;
        public DelegateCommand MovPCommand => movPCommand ?? (movPCommand = new DelegateCommand(() =>
        {
            //MovP运动
            if (_vRobot != null && CurrentPointIndex != -1)
            {
                Task.Run(() =>
                {
                    _vRobot.MovP(CurrentPointIndex);
                });
            }
        }));

        /// <summary>
        /// Jump运动
        /// </summary>
        private DelegateCommand jumpCommand;
        public DelegateCommand JumpCommand => jumpCommand ?? (jumpCommand = new DelegateCommand(() =>
        {
            //MovP运动
            if (_vRobot != null && CurrentPointIndex != -1)
            {
                Task.Run(() =>
                {
                    _vRobot.Jump(CurrentPointIndex);
                });
            }
        }));


        /// <summary>
        /// 点位运动指令
        /// </summary>
        private DelegateCommand<RobotPointItem> _moveTeachCommand;
        public DelegateCommand<RobotPointItem> MoveTeachCommand => _moveTeachCommand ?? (_moveTeachCommand = new DelegateCommand<RobotPointItem>((pos) =>
        {
            if (pos == null) return;
            Task.Run(() =>
            {
                _vRobot.MovP(CurrentPointIndex);
                //pos.Tag.Robot.MovP(CurrentPointIndex);
            });

        }));

        /// <summary>
        /// 点位运动指令
        /// </summary>
        private DelegateCommand<SixRobotPointItem> _moveSixTeachCommand;
        public DelegateCommand<SixRobotPointItem> MoveSixTeachCommand => _moveSixTeachCommand ?? (_moveSixTeachCommand = new DelegateCommand<SixRobotPointItem>((pos) =>
        {
            if (pos == null) return;
            Task.Run(() =>
            {
                double[] pose = new double[6];
                pose[0] = pos.PositionX / 1000;
                pose[1] = pos.PositionY / 1000;
                pose[2] = pos.PositionZ / 1000;
                pose[3] = pos.PositionU / 1000;
                pose[4] = pos.PositionV / 1000;
                pose[5] = pos.PositionW / 1000;
                _vRobot.SixMoveL(pos.Name, pose);

            });

        }));

        /// <summary>
        /// 更新写入点位
        /// </summary>
        private DelegateCommand<RobotPointItem> _updateTeachCommand;
        public DelegateCommand<RobotPointItem> UpdateTeachCommand => _updateTeachCommand ?? (_updateTeachCommand = new DelegateCommand<RobotPointItem>((pos) =>
        {
            if (pos == null || pos.Name == "Home") return;

            List<double> point = new List<double>();
            point.Add(pos.PositionX);
            point.Add(pos.PositionY);
            point.Add(pos.PositionZ);
            point.Add(pos.PositionC);
            point.Add(pos.PositionH);
            pos.Tag.Robot.WritePoint(CurrentPointIndex, point);
        }));

        private DelegateCommand<SixRobotPointItem> _updateSixTeachCommand;
        public DelegateCommand<SixRobotPointItem> UpdateSixTeachCommand => _updateSixTeachCommand ?? (_updateSixTeachCommand = new DelegateCommand<SixRobotPointItem>((pos) =>
        {
            if (pos == null || pos.Name == "Home") return;

            pos.Tag.Robot.WriteCurSixPoint(pos.Name);
        }));



        private DelegateCommand<RobotPointItem> _removePositionCommand;
        public DelegateCommand<RobotPointItem> RemovePositionCommand => _removePositionCommand ?? (_removePositionCommand = new DelegateCommand<RobotPointItem>((item) =>
        {
            if (Current != null && _vRobot != null)
            {
                var robotPos = Current.Tag.Positions.FirstOrDefault(u => u.Name == item.Name);
                _vRobot.RemovePostion(robotPos.Name);
                //deviceEngine.RemoveAxisPos(axisPos);
                RobotPointItems.Remove(item);
            }
        }));

        private DelegateCommand<SixRobotPointItem> _removeSixPositionCommand;
        public DelegateCommand<SixRobotPointItem> RemoveSixPositionCommand => _removeSixPositionCommand ?? (_removeSixPositionCommand = new DelegateCommand<SixRobotPointItem>((item) =>
        {
            if (Current != null && _vRobot != null)
            {
                var robotPos = Current.Tag.SixPositions.FirstOrDefault(u => u.Name == item.Name);
                _vRobot.RemovePostion(robotPos.Name);
                //deviceEngine.RemoveAxisPos(axisPos);
                SixRobotPointItems.Remove(item);
            }
        }));


        private DelegateCommand<VRobotModel> _teachCommand;
        public DelegateCommand<VRobotModel> TeachCommand => _teachCommand ?? (_teachCommand = new DelegateCommand<VRobotModel>((robot) =>
        {
            if (robot == null) return;
            //robot.CurrentPoint = robot.Tag.GetCurrentPosion(CoordType.Cartesian);
            dialogService.ShowTeachRobotPositionDialog(RobotPoints, IsSixRobot, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<string>("Name", out var name) &&
                     r.Parameters.TryGetValue<List<KeyValue>>("Point", out var point))
                    {
                        robot.Tag.AddPostion(name, point);

                        if (!IsSixRobot)
                        {
                            var src = robot.Tag.Positions.FirstOrDefault(u => u.Name == name);
                            RobotPointItems.Add(new RobotPointItem()
                            {
                                Name = name,
                                PositionX = Convert.ToDouble(point[0].Value),
                                PositionY = Convert.ToDouble(point[1].Value),
                                PositionZ = Convert.ToDouble(point[2].Value),
                                PositionC = Convert.ToDouble(point[3].Value),
                                PositionH = Convert.ToDouble(point[4].Value),
                                Tag = src
                            });
                        }
                        else
                        {
                            var src = robot.Tag.SixPositions.FirstOrDefault(u => u.Name == name);
                            SixRobotPointItems.Add(new SixRobotPointItem()
                            {
                                Name = name,
                                PositionX = Convert.ToDouble(point[0].Value),
                                PositionY = Convert.ToDouble(point[1].Value),
                                PositionZ = Convert.ToDouble(point[2].Value),
                                PositionU = Convert.ToDouble(point[3].Value),
                                PositionV = Convert.ToDouble(point[4].Value),
                                PositionW = Convert.ToDouble(point[5].Value),
                                Tag = src
                            });
                        }
                    }
                }
            });
        }));


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
                else if (sArgs.AddedItems[0] is RobotPointItem p)
                {
                    CurrentPoint = p;
                    CurrentRobotPointModels.Clear();
                    CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "X", Pos = CurrentPoint.PositionX });
                    CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "Y", Pos = CurrentPoint.PositionY });
                    CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "Z", Pos = CurrentPoint.PositionZ });
                    CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "C", Pos = CurrentPoint.PositionC });
                    CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "Hand", Pos = CurrentPoint.PositionH });
                }
                else if (sArgs.AddedItems[0] is SixRobotPointItem p6)
                {
                    CurrentSixPoint = p6;
                    CurrentSixRobotPointModels.Clear();
                    CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "X", Pos = CurrentSixPoint.PositionX });
                    CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "Y", Pos = CurrentSixPoint.PositionY });
                    CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "Z", Pos = CurrentSixPoint.PositionZ });
                    CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "U", Pos = CurrentSixPoint.PositionU });
                    CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "V", Pos = CurrentSixPoint.PositionV });
                    CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "W", Pos = CurrentSixPoint.PositionW });
                }
            }
            else
            {
                CurrentPoint = null;
                Leave();
            }
        }));

        #endregion

        #region 方法

        private void StartMonitor()
        {
            if (_vRobot == null) return;

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
                    var dictStatus = Current.Tag.GetRobotStatus();
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

                    //更新点位
                    RobotPoints = Current.Tag.GetCurrentPosion(CurCoordType, true);


                    if (whileToken == null || whileToken.IsCancellationRequested)
                    {
                        break;
                    }

                    Thread.Sleep(100);
                    Debug.WriteLine("123");
                }
            }, whileToken.Token);

        }

        /// <summary>
        /// 机器人坐标系名称更新
        /// </summary>
        private void UpdateRobotCoordinateName()
        {
            if (CurCoordType == CoordType.Cartesian)
            {
                RobotAxis1Name = "X+";
                RobotAxis2Name = "X-";
                RobotAxis3Name = "Y+";
                RobotAxis4Name = "Y-";
                RobotAxis5Name = "Z+";
                RobotAxis6Name = "Z-";
                RobotAxis7Name = "U+";
                RobotAxis8Name = "U-";
                RobotAxis9Name = "V+";
                RobotAxis10Name = "V-";
                RobotAxis11Name = "W+";
                RobotAxis12Name = "W-";
            }
            else
            {
                RobotAxis1Name = "J1+";
                RobotAxis2Name = "J1-";
                RobotAxis3Name = "J2+";
                RobotAxis4Name = "J2-";
                RobotAxis5Name = "J3+";
                RobotAxis6Name = "J3-";
                RobotAxis7Name = "J4+";
                RobotAxis8Name = "J4-";
                RobotAxis9Name = "J5+";
                RobotAxis10Name = "J5-";
                RobotAxis11Name = "J6+";
                RobotAxis12Name = "J6-";
            }
        }

        /// <summary>
        /// 添加设备
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowVRobotDialog(r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<VRobot>("VRobot", out var vR))
                    {
                        deviceEngine.AddVirtual(vR);
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
            //线扫设备
            var devices = deviceEngine.GetDevices(typeof(VRobot));
            VRobotModels = new ObservableCollection<VRobotModel>();
            foreach (var device in devices)
            {
                var vRobot = device as VRobot;
                var vRobotModel = new VRobotModel(vRobot);
                vRobotModel.RobotName = vRobot.GetDevice().Name;
                VRobotModels.Add(vRobotModel);
            }

        }

        private void LoadTeachPoint()
        {
            if (Current != null)
            {
                if (!IsSixRobot)
                {
                    RobotPointItems = new ObservableCollection<RobotPointItem>();
                    var positions = Current.Tag.Positions;
                    foreach (var node in positions)
                    {
                        var axisPosition = node;
                        var position = new RobotPointItem()
                        {
                            Name = axisPosition.Name,
                            PositionX = axisPosition.PositionX,
                            PositionY = axisPosition.PositionY,
                            PositionZ = axisPosition.PositionZ,
                            PositionC = axisPosition.PositionC,
                            PositionH = axisPosition.PositionH,
                            Tag = axisPosition
                        };
                        RobotPointItems.Add(position);
                    }
                }
                else
                {
                    SixRobotPointItems = new ObservableCollection<SixRobotPointItem>();
                    var positions = Current.Tag.SixPositions;
                    foreach (var node in positions)
                    {
                        var robotPosition = node;
                        var position = new SixRobotPointItem()
                        {
                            Name = robotPosition.Name,
                            PositionX = robotPosition.PositionX,
                            PositionY = robotPosition.PositionY,
                            PositionZ = robotPosition.PositionZ,
                            PositionU = robotPosition.PositionU,
                            PositionV = robotPosition.PositionV,
                            PositionW = robotPosition.PositionW,
                            Tag = robotPosition
                        };
                        SixRobotPointItems.Add(position);
                    }
                }

            }
        }

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
}
