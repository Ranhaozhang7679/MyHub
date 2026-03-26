using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Luster.SimDevice.SubSystem.ViewModel.RobotPositionType;
using Luster.TaskFlow.Motion.Interfaces;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    //public class VRobotContentVM : PageVM
    //{
    //    protected VRobotContentVM(ISimDeviceEngineUI _engine) : base(_engine)
    //    {
    //        CurrentRobotPointModels = new ObservableCollection<VRobotPointModel>();
    //        CurrentSixRobotPointModels = new ObservableCollection<VSixRobotPointModel>();
    //        CoordTypes = typeof(CoordType).EnumToDataSource();
    //        RobotRunModes = new List<string>() { "单步运动0.1", "单步运动1", "单步运动5", "连续运动" };
    //        CurRobotRunMode = RobotRunModes[0];
    //        UpdateRobotCoordinateName();
    //        LoadDevices();
    //    }

    //    #region 字段
    //    private VRobot _vRobot;
    //    public int CurrentPointIndex = -1;
    //    #endregion

    //    #region 属性  

    //    /// <summary>
    //    /// 坐标系的类型
    //    /// </summary>
    //    private List<KeyValue> _coordTypes;
    //    public List<KeyValue> CoordTypes
    //    {
    //        get => _coordTypes;
    //        set
    //        {
    //            SetProperty(ref _coordTypes, value);
    //        }
    //    }

    //    /// <summary>
    //    /// 当前坐标系
    //    /// </summary>
    //    private CoordType _curCoordType = CoordType.Cartesian;
    //    public CoordType CurCoordType
    //    {
    //        get { return _curCoordType; }
    //        set
    //        {
    //            SetProperty(ref _curCoordType, value);
    //            UpdateRobotCoordinateName();
    //        }
    //    }

    //    /// <summary>
    //    /// 显示添加按钮
    //    /// </summary>
    //    public override bool IsShowAdd => true;

    //    /// <summary>
    //    /// 当前选择的机器人
    //    /// </summary>
    //    private VRobotModel _current;
    //    public VRobotModel Current
    //    {
    //        get => _current;
    //        set
    //        {
    //            SetProperty(ref _current, value);
    //        }
    //    }

    //    /// <summary>
    //    /// 机器人列表
    //    /// </summary>
    //    private ObservableCollection<VRobotModel> _vRobotModels;
    //    public ObservableCollection<VRobotModel> VRobotModels
    //    {
    //        get { return _vRobotModels; }
    //        set { SetProperty(ref _vRobotModels, value); }
    //    }


    //    /// <summary>
    //    /// 机器人点位列表
    //    /// </summary>

    //    private List<KeyValue> _robotPoints;
    //    public List<KeyValue> RobotPoints
    //    {
    //        get => _robotPoints;
    //        set
    //        {
    //            SetProperty(ref _robotPoints, value);
    //        }
    //    }

    //    #region 机器人轴名称
    //    /// <summary>
    //    /// 机器人轴1+名称
    //    /// </summary>
    //    private string _robotAxis1Name;
    //    public string RobotAxis1Name
    //    {
    //        get => _robotAxis1Name;
    //        set => SetProperty(ref _robotAxis1Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴1-名称
    //    /// </summary>
    //    private string _robotAxis2Name;
    //    public string RobotAxis2Name
    //    {
    //        get => _robotAxis2Name;
    //        set => SetProperty(ref _robotAxis2Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴2+名称
    //    /// </summary>
    //    private string _robotAxis3Name;
    //    public string RobotAxis3Name
    //    {
    //        get => _robotAxis3Name;
    //        set => SetProperty(ref _robotAxis3Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴2-名称
    //    /// </summary>
    //    private string _robotAxis4Name;
    //    public string RobotAxis4Name
    //    {
    //        get => _robotAxis4Name;
    //        set => SetProperty(ref _robotAxis4Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴3+名称
    //    /// </summary>
    //    private string _robotAxis5Name;
    //    public string RobotAxis5Name
    //    {
    //        get => _robotAxis5Name;
    //        set => SetProperty(ref _robotAxis5Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴3-名称
    //    /// </summary>
    //    private string _robotAxis6Name;
    //    public string RobotAxis6Name
    //    {
    //        get => _robotAxis6Name;
    //        set => SetProperty(ref _robotAxis6Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴4+名称
    //    /// </summary>
    //    private string _robotAxis7Name;
    //    public string RobotAxis7Name
    //    {
    //        get => _robotAxis7Name;
    //        set => SetProperty(ref _robotAxis7Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴4-名称
    //    /// </summary>
    //    private string _robotAxis8Name;
    //    public string RobotAxis8Name
    //    {
    //        get => _robotAxis8Name;
    //        set => SetProperty(ref _robotAxis8Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴5+名称
    //    /// </summary>
    //    private string _robotAxis9Name;
    //    public string RobotAxis9Name
    //    {
    //        get => _robotAxis9Name;
    //        set => SetProperty(ref _robotAxis9Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴5-名称
    //    /// </summary>
    //    private string _robotAxis10Name;
    //    public string RobotAxis10Name
    //    {
    //        get => _robotAxis10Name;
    //        set => SetProperty(ref _robotAxis10Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴6+名称
    //    /// </summary>
    //    private string _robotAxis11Name;
    //    public string RobotAxis11Name
    //    {
    //        get => _robotAxis11Name;
    //        set => SetProperty(ref _robotAxis11Name, value);
    //    }

    //    /// <summary>
    //    /// 机器人轴6-名称
    //    /// </summary>
    //    private string _robotAxis12Name;
    //    public string RobotAxis12Name
    //    {
    //        get => _robotAxis12Name;
    //        set => SetProperty(ref _robotAxis12Name, value);
    //    }
    //    #endregion

    //    /// <summary>
    //    /// 机器人运动模式
    //    /// </summary>
    //    private List<string> _robotRunModes;
    //    public List<string> RobotRunModes
    //    {
    //        get { return _robotRunModes; }
    //        set { SetProperty(ref _robotRunModes, value); }
    //    }

    //    /// <summary>
    //    /// 当前机器人运动模式
    //    /// </summary>
    //    private string _curRobotRunMode;
    //    public string CurRobotRunMode
    //    {
    //        get { return _curRobotRunMode; }
    //        set
    //        {
    //            SetProperty(ref _curRobotRunMode, value);
    //            if (_vRobot != null)
    //            {
    //                _vRobot.SetManualMode(GetCurRobotRunMode(_curRobotRunMode));
    //            }
    //        }
    //    }

    //    private short GetCurRobotRunMode(string msg)
    //    {
    //        short mode = 0;
    //        switch (msg)
    //        {
    //            case "单步运动0.1":
    //                mode = 0;
    //                break;

    //            case "单步运动1":
    //                mode = 1;
    //                break;
    //            case "单步运动5":
    //                mode = 2;
    //                break;
    //            case "连续运动":
    //                mode = 3;
    //                break;
    //        }
    //        return mode;
    //    }

    //    /// <summary>
    //    /// 机器人是否为六轴机器人
    //    /// </summary>
    //    private bool _isSixRobot;
    //    public bool IsSixRobot
    //    {
    //        get { return _isSixRobot; }
    //        set
    //        {
    //            SetProperty(ref _isSixRobot, value);
    //        }
    //    }

    //    /// <summary>
    //    /// 机器人运动距离
    //    /// </summary>
    //    private double _movePos;
    //    public double MovePos
    //    {
    //        get { return _movePos; }
    //        set { SetProperty(ref _movePos, value); }
    //    }

    //    /// <summary>
    //    /// 机器人速度系数
    //    /// </summary>
    //    private int _speedCoefficient = 1;
    //    public int SpeedCoefficient
    //    {
    //        get { return _speedCoefficient; }
    //        set
    //        {
    //            SetProperty(ref _speedCoefficient, value);
    //            _vRobot?.SetSpeedCoefficient((short)_speedCoefficient);
    //        }
    //    }

    //    private ObservableCollection<RobotPointItem> _robotPointItems;
    //    /// <summary>
    //    /// 机器人坐标系点位列表
    //    /// </summary>
    //    public ObservableCollection<RobotPointItem> RobotPointItems
    //    {
    //        get { return _robotPointItems; }
    //        set { SetProperty(ref _robotPointItems, value); }
    //    }

    //    private ObservableCollection<SixRobotPointItem> _sixRobotPointItems;
    //    /// <summary>
    //    /// 6轴机器人坐标系点位列表
    //    /// </summary>
    //    public ObservableCollection<SixRobotPointItem> SixRobotPointItems
    //    {
    //        get { return _sixRobotPointItems; }
    //        set { SetProperty(ref _sixRobotPointItems, value); }
    //    }


    //    /// <summary>
    //    /// 当前点位显示
    //    /// </summary>
    //    private ObservableCollection<VRobotPointModel> _currentRobotPointModels;
    //    public ObservableCollection<VRobotPointModel> CurrentRobotPointModels
    //    {
    //        get { return _currentRobotPointModels; }
    //        set { SetProperty(ref _currentRobotPointModels, value); }
    //    }

    //    private ObservableCollection<VSixRobotPointModel> _currentSixRobotPointModels;
    //    public ObservableCollection<VSixRobotPointModel> CurrentSixRobotPointModels
    //    {
    //        get { return _currentSixRobotPointModels; }
    //        set { SetProperty(ref _currentSixRobotPointModels, value); }
    //    }

    //    /// <summary>
    //    /// 当前点位
    //    /// </summary>
    //    private RobotPointItem _currentPoint;
    //    public RobotPointItem CurrentPoint
    //    {
    //        get { return _currentPoint; }
    //        set { SetProperty(ref _currentPoint, value); }
    //    }

    //    private SixRobotPointItem _currentSixPoint;
    //    public SixRobotPointItem CurrentSixPoint
    //    {
    //        get { return _currentSixPoint; }
    //        set { SetProperty(ref _currentSixPoint, value); }
    //    }

    //    #endregion

    //    #region 命令

    //    /// <summary>
    //    /// 机器人选择
    //    /// </summary>
    //    private DelegateCommand<VRobotModel> selectedCommand;
    //    public DelegateCommand<VRobotModel> SelectedCommand => selectedCommand ?? (selectedCommand = new DelegateCommand<VRobotModel>((item) =>
    //    {
    //        Current = item;

    //        if (item != null)
    //        {
    //            var device = deviceEngine.GetVirtualByID(item.ID);
    //            _vRobot = device as VRobot;


    //            if (_vRobot != null)
    //            {
    //                IsSixRobot = (_vRobot.GetDevice() as IRobot).AxisCount > 4 ? true : false;
    //                UpdateRobotCoordinateName();
    //                LoadTeachPoint();
    //                RobotPoints = Current.Tag.GetCurrentPosion(CurCoordType, false);
    //            }
    //        }
    //        else
    //        {
    //            PropertyObj = new object();
    //        }
    //    }));


    //    private DelegateCommand<VRobotModel> _removeCommand;
    //    /// <summary>
    //    /// 飞拍模块删除
    //    /// </summary>
    //    public DelegateCommand<VRobotModel> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<VRobotModel>((item) =>
    //    {
    //        dialogService.ShowConfirm($"确认删除机器人:{item.Name}", r =>
    //        {
    //            if (r.Result == ButtonResult.OK)
    //            {
    //                deviceEngine.ReomoveVirtual(item.ID);
    //                var model = VRobotModels.FirstOrDefault(u => u.ID == item.ID);
    //                if (model != null)
    //                {
    //                    if (model.ID == Current.ID)
    //                    {
    //                        Current = null;
    //                    }
    //                    VRobotModels.Remove(model);
    //                }
    //            }
    //        });
    //    }));


    //    /// <summary>
    //    /// 连接机器人
    //    /// </summary>
    //    private DelegateCommand connectRobotCommand;
    //    public DelegateCommand ConnectRobotCommand => connectRobotCommand ?? (connectRobotCommand = new DelegateCommand(() =>
    //    {
    //        //连接机器人
    //        if (_vRobot != null)
    //        {
    //            _vRobot.Connect();

    //            //更新当前速率
    //            SpeedCoefficient = _vRobot.GetSpeedCoefficient();

    //            //开启监控
    //            StartMonitor();
    //        }
    //    }));

    //    /// <summary>
    //    /// 断开连接机器人
    //    /// </summary>
    //    private DelegateCommand disConnectRobotCommand;
    //    public DelegateCommand DisConnectRobotCommand => disConnectRobotCommand ?? (disConnectRobotCommand = new DelegateCommand(() =>
    //    {
    //        //连接机器人
    //        if (_vRobot != null)
    //        {
    //            _vRobot.DisConnect();
    //        }
    //    }));

    //    /// <summary>
    //    /// 上使能
    //    /// </summary>
    //    private DelegateCommand servoOnCommand;
    //    public DelegateCommand ServoOnCommand => servoOnCommand ?? (servoOnCommand = new DelegateCommand(() =>
    //    {
    //        //上使能
    //        if (_vRobot != null)
    //        {
    //            _vRobot.ServOn(1);
    //        }
    //    }));

    //    /// <summary>
    //    /// 断开使能
    //    /// </summary>
    //    private DelegateCommand servoOffCommand;
    //    public DelegateCommand ServoOffCommand => servoOffCommand ?? (servoOffCommand = new DelegateCommand(() =>
    //    {
    //        //断开使能
    //        if (_vRobot != null)
    //        {
    //            _vRobot.ServOn(0);
    //        }
    //    }));

    //    /// <summary>
    //    /// 拖拽模式
    //    /// </summary>
    //    private DelegateCommand servoDragCommand;
    //    public DelegateCommand ServoDragCommand => servoDragCommand ?? (servoDragCommand = new DelegateCommand(() =>
    //    {
    //        //拖拽模式
    //        if (_vRobot != null)
    //        {
    //            _vRobot.ServOn(2);
    //        }
    //    }));

    //    /// <summary>
    //    /// 获取运动权限
    //    /// </summary>
    //    private DelegateCommand mAcceptCommand;
    //    public DelegateCommand MAcceptCommand => mAcceptCommand ?? (mAcceptCommand = new DelegateCommand(() =>
    //    {
    //        if (_vRobot != null)
    //        {
    //            _vRobot.SetMoveAccept(true);
    //        }
    //    }));

    //    /// <summary>
    //    /// 释放运动权限
    //    /// </summary>
    //    private DelegateCommand mReleaseCommand;
    //    public DelegateCommand MReleaseCommand => mReleaseCommand ?? (mReleaseCommand = new DelegateCommand(() =>
    //    {
    //        if (_vRobot != null)
    //        {
    //            _vRobot.SetMoveAccept(false);
    //        }
    //    }));

    //    /// <summary>
    //    /// 清除报警
    //    /// </summary>
    //    private DelegateCommand clearErrorCommand;
    //    public DelegateCommand ClearErrorCommand => clearErrorCommand ?? (clearErrorCommand = new DelegateCommand(() =>
    //    {
    //        if (_vRobot != null)
    //        {
    //            _vRobot.ClearAlarm();
    //        }
    //    }));


    //    /// <summary>
    //    /// 手动运动运行
    //    /// </summary>
    //    private DelegateCommand<object> preMouseDownCommand;
    //    public DelegateCommand<object> PreMouseDownCommand => preMouseDownCommand ?? (preMouseDownCommand = new DelegateCommand<object>((obj) =>
    //    {
    //        if (_vRobot != null)
    //        {
    //            switch (obj?.ToString())
    //            {
    //                case "J1+":
    //                    _vRobot.ManualMove(1, true);
    //                    break;
    //                case "J1-":
    //                    _vRobot.ManualMove(2, true);
    //                    break;
    //                case "J2+":
    //                    _vRobot.ManualMove(3, true);
    //                    break;
    //                case "J2-":
    //                    _vRobot.ManualMove(4, true);
    //                    break;
    //                case "J3+":
    //                    _vRobot.ManualMove(5, true);
    //                    break;
    //                case "J3-":
    //                    _vRobot.ManualMove(6, true);
    //                    break;
    //                case "J4+":
    //                    _vRobot.ManualMove(7, true);
    //                    break;
    //                case "J4-":
    //                    _vRobot.ManualMove(8, true);
    //                    break;

    //                case "J5+":
    //                    _vRobot.ManualMove(9, true);
    //                    break;
    //                case "J5-":
    //                    _vRobot.ManualMove(10, true);
    //                    break;

    //                case "J6+":
    //                    _vRobot.ManualMove(11, true);
    //                    break;
    //                case "J6-":
    //                    _vRobot.ManualMove(12, true);
    //                    break;


    //                case "X+":
    //                    _vRobot.ManualMove(1, false);
    //                    break;
    //                case "X-":
    //                    _vRobot.ManualMove(2, false);
    //                    break;
    //                case "Y+":
    //                    _vRobot.ManualMove(3, false);
    //                    break;
    //                case "Y-":
    //                    _vRobot.ManualMove(4, false);
    //                    break;
    //                case "Z+":
    //                    _vRobot.ManualMove(5, false);
    //                    break;
    //                case "Z-":
    //                    _vRobot.ManualMove(6, false);
    //                    break;
    //                case "U+":
    //                    _vRobot.ManualMove(7, false);
    //                    break;
    //                case "U-":
    //                    _vRobot.ManualMove(8, false);
    //                    break;
    //                case "V+":
    //                    _vRobot.ManualMove(9, false);
    //                    break;
    //                case "V-":
    //                    _vRobot.ManualMove(10, false);
    //                    break;
    //                case "W+":
    //                    _vRobot.ManualMove(11, false);
    //                    break;
    //                case "W-":
    //                    _vRobot.ManualMove(12, false);
    //                    break;
    //            }
    //        }
    //    }));

    //    /// <summary>
    //    /// 手动运动停止
    //    /// </summary>
    //    private DelegateCommand<object> preMouseUpCommand;
    //    public DelegateCommand<object> PreMouseUpCommand => preMouseUpCommand ?? (preMouseUpCommand = new DelegateCommand<object>((obj) =>
    //    {
    //        if (_vRobot != null)
    //        {

    //            if (CurCoordType == CoordType.Cartesian)
    //            {

    //                _vRobot.ManualMove(0, false);
    //            }
    //            else
    //            {
    //                _vRobot.ManualMove(0, true);
    //            }
    //        }
    //    }));

    //    /// <summary>
    //    /// 点到点相对运动
    //    /// </summary>
    //    private DelegateCommand<object> movPRCommand;
    //    public DelegateCommand<object> MovPRCommand => movPRCommand ?? (movPRCommand = new DelegateCommand<object>((obj) =>
    //    {
    //        //MovPR运动
    //        if (_vRobot != null)
    //        {
    //            switch (obj?.ToString())
    //            {
    //                case "X":
    //                    _vRobot.MovP(1, false, MovePos);
    //                    break;
    //                case "Y":
    //                    _vRobot.MovP(2, false, MovePos);
    //                    break;
    //                case "Z":
    //                    _vRobot.MovP(3, false, MovePos);
    //                    break;
    //                case "C":
    //                    _vRobot.MovP(4, false, MovePos);
    //                    break;
    //            }
    //        }
    //    }));

    //    /// <summary>
    //    /// 回零运动
    //    /// </summary>
    //    private DelegateCommand homeCommand;
    //    public DelegateCommand HomeCommand => homeCommand ?? (homeCommand = new DelegateCommand(() =>
    //    {
    //        //MovP回零运动
    //        if (_vRobot != null)
    //        {
    //            Task.Run(() =>
    //            {
    //                _vRobot.MovP(0);
    //            });
    //        }
    //    }));

    //    /// <summary>
    //    /// 停止运动
    //    /// </summary>
    //    private DelegateCommand stopCommand;
    //    public DelegateCommand StopCommand => stopCommand ?? (stopCommand = new DelegateCommand(() =>
    //    {
    //        //MovP回零运动
    //        if (_vRobot != null)
    //        {
    //            _vRobot.Stop();
    //        }
    //    }));

    //    /// <summary>
    //    /// 点到点运动
    //    /// </summary>
    //    private DelegateCommand movPCommand;
    //    public DelegateCommand MovPCommand => movPCommand ?? (movPCommand = new DelegateCommand(() =>
    //    {
    //        //MovP运动
    //        if (_vRobot != null && CurrentPointIndex != -1)
    //        {
    //            Task.Run(() =>
    //            {
    //                _vRobot.MovP(CurrentPointIndex);
    //            });
    //        }
    //    }));

    //    /// <summary>
    //    /// Jump运动
    //    /// </summary>
    //    private DelegateCommand jumpCommand;
    //    public DelegateCommand JumpCommand => jumpCommand ?? (jumpCommand = new DelegateCommand(() =>
    //    {
    //        //MovP运动
    //        if (_vRobot != null && CurrentPointIndex != -1)
    //        {
    //            Task.Run(() =>
    //            {
    //                _vRobot.Jump(CurrentPointIndex);
    //            });
    //        }
    //    }));


    //    /// <summary>
    //    /// 点位运动指令
    //    /// </summary>
    //    private DelegateCommand<RobotPointItem> _moveTeachCommand;
    //    public DelegateCommand<RobotPointItem> MoveTeachCommand => _moveTeachCommand ?? (_moveTeachCommand = new DelegateCommand<RobotPointItem>((pos) =>
    //    {
    //        if (pos == null) return;
    //        Task.Run(() =>
    //        {
    //            _vRobot.MovP(CurrentPointIndex);
    //            //pos.Tag.Robot.MovP(CurrentPointIndex);
    //        });

    //    }));

    //    /// <summary>
    //    /// 点位运动指令
    //    /// </summary>
    //    private DelegateCommand<SixRobotPointItem> _moveSixTeachCommand;
    //    public DelegateCommand<SixRobotPointItem> MoveSixTeachCommand => _moveSixTeachCommand ?? (_moveSixTeachCommand = new DelegateCommand<SixRobotPointItem>((pos) =>
    //    {
    //        if (pos == null) return;
    //        Task.Run(() =>
    //        {
    //            double[] pose = new double[6];
    //            pose[0] = pos.PositionX / 1000;
    //            pose[1] = pos.PositionY / 1000;
    //            pose[2] = pos.PositionZ / 1000;
    //            pose[3] = pos.PositionU / 1000;
    //            pose[4] = pos.PositionV / 1000;
    //            pose[5] = pos.PositionW / 1000;
    //            _vRobot.SixMoveL(pos.Name, pose);

    //        });

    //    }));

    //    /// <summary>
    //    /// 更新写入点位
    //    /// </summary>
    //    private DelegateCommand<RobotPointItem> _updateTeachCommand;
    //    public DelegateCommand<RobotPointItem> UpdateTeachCommand => _updateTeachCommand ?? (_updateTeachCommand = new DelegateCommand<RobotPointItem>((pos) =>
    //    {
    //        if (pos == null || pos.Name == "Home") return;

    //        List<double> point = new List<double>();
    //        point.Add(pos.PositionX);
    //        point.Add(pos.PositionY);
    //        point.Add(pos.PositionZ);
    //        point.Add(pos.PositionC);
    //        point.Add(pos.PositionH);
    //        pos.Tag.Robot.WritePoint(CurrentPointIndex, point);
    //    }));

    //    private DelegateCommand<SixRobotPointItem> _updateSixTeachCommand;
    //    public DelegateCommand<SixRobotPointItem> UpdateSixTeachCommand => _updateSixTeachCommand ?? (_updateSixTeachCommand = new DelegateCommand<SixRobotPointItem>((pos) =>
    //    {
    //        if (pos == null || pos.Name == "Home") return;

    //        pos.Tag.Robot.WriteCurSixPoint(pos.Name);
    //    }));



    //    private DelegateCommand<RobotPointItem> _removePositionCommand;
    //    public DelegateCommand<RobotPointItem> RemovePositionCommand => _removePositionCommand ?? (_removePositionCommand = new DelegateCommand<RobotPointItem>((item) =>
    //    {
    //        if (Current != null && _vRobot != null)
    //        {
    //            var robotPos = Current.Tag.Positions.FirstOrDefault(u => u.Name == item.Name);
    //            _vRobot.RemovePostion(robotPos.Name);
    //            //deviceEngine.RemoveAxisPos(axisPos);
    //            RobotPointItems.Remove(item);
    //        }
    //    }));

    //    private DelegateCommand<SixRobotPointItem> _removeSixPositionCommand;
    //    public DelegateCommand<SixRobotPointItem> RemoveSixPositionCommand => _removeSixPositionCommand ?? (_removeSixPositionCommand = new DelegateCommand<SixRobotPointItem>((item) =>
    //    {
    //        if (Current != null && _vRobot != null)
    //        {
    //            var robotPos = Current.Tag.SixPositions.FirstOrDefault(u => u.Name == item.Name);
    //            _vRobot.RemovePostion(robotPos.Name);
    //            //deviceEngine.RemoveAxisPos(axisPos);
    //            SixRobotPointItems.Remove(item);
    //        }
    //    }));


    //    private DelegateCommand<VRobotModel> _teachCommand;
    //    public DelegateCommand<VRobotModel> TeachCommand => _teachCommand ?? (_teachCommand = new DelegateCommand<VRobotModel>((robot) =>
    //    {
    //        if (robot == null) return;
    //        //robot.CurrentPoint = robot.Tag.GetCurrentPosion(CoordType.Cartesian);
    //        dialogService.ShowTeachRobotPositionDialog(RobotPoints, IsSixRobot, (r) =>
    //        {
    //            if (r.Result == ButtonResult.OK)
    //            {
    //                if (r.Parameters.TryGetValue<string>("Name", out var name) &&
    //                 r.Parameters.TryGetValue<List<KeyValue>>("Point", out var point))
    //                {
    //                    robot.Tag.AddPostion(name, point);

    //                    if (!IsSixRobot)
    //                    {
    //                        var src = robot.Tag.Positions.FirstOrDefault(u => u.Name == name);
    //                        RobotPointItems.Add(new RobotPointItem()
    //                        {
    //                            Name = name,
    //                            PositionX = Convert.ToDouble(point[0].Value),
    //                            PositionY = Convert.ToDouble(point[1].Value),
    //                            PositionZ = Convert.ToDouble(point[2].Value),
    //                            PositionC = Convert.ToDouble(point[3].Value),
    //                            PositionH = Convert.ToDouble(point[4].Value),
    //                            Tag = src
    //                        });
    //                    }
    //                    else
    //                    {
    //                        var src = robot.Tag.SixPositions.FirstOrDefault(u => u.Name == name);
    //                        SixRobotPointItems.Add(new SixRobotPointItem()
    //                        {
    //                            Name = name,
    //                            PositionX = Convert.ToDouble(point[0].Value),
    //                            PositionY = Convert.ToDouble(point[1].Value),
    //                            PositionZ = Convert.ToDouble(point[2].Value),
    //                            PositionU = Convert.ToDouble(point[3].Value),
    //                            PositionV = Convert.ToDouble(point[4].Value),
    //                            PositionW = Convert.ToDouble(point[5].Value),
    //                            Tag = src
    //                        });
    //                    }
    //                }
    //            }
    //        });
    //    }));


    //    /// <summary>
    //    /// 点位选择
    //    /// </summary>
    //    private DelegateCommand<object> _selectPosionCommand;
    //    public DelegateCommand<object> SelectedPosionCommand => _selectPosionCommand ?? (_selectPosionCommand = new DelegateCommand<object>((args) =>
    //    {
    //        SelectionChangedEventArgs sArgs = args as SelectionChangedEventArgs;
    //        if (sArgs != null && sArgs.AddedItems.Count > 0)
    //        {
    //            if (sArgs.AddedItems[0] is KeyValue kVal)
    //            {
    //                return;
    //            }
    //            else if (sArgs.AddedItems[0] is RobotPointItem p)
    //            {
    //                CurrentPoint = p;
    //                CurrentRobotPointModels.Clear();
    //                CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "X", Pos = CurrentPoint.PositionX });
    //                CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "Y", Pos = CurrentPoint.PositionY });
    //                CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "Z", Pos = CurrentPoint.PositionZ });
    //                CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "C", Pos = CurrentPoint.PositionC });
    //                CurrentRobotPointModels.Add(new VRobotPointModel() { Tag = CurrentPoint, Name = "Hand", Pos = CurrentPoint.PositionH });
    //            }
    //            else if (sArgs.AddedItems[0] is SixRobotPointItem p6)
    //            {
    //                CurrentSixPoint = p6;
    //                CurrentSixRobotPointModels.Clear();
    //                CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "X", Pos = CurrentSixPoint.PositionX });
    //                CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "Y", Pos = CurrentSixPoint.PositionY });
    //                CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "Z", Pos = CurrentSixPoint.PositionZ });
    //                CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "U", Pos = CurrentSixPoint.PositionU });
    //                CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "V", Pos = CurrentSixPoint.PositionV });
    //                CurrentSixRobotPointModels.Add(new VSixRobotPointModel() { Tag = CurrentSixPoint, Name = "W", Pos = CurrentSixPoint.PositionW });
    //            }
    //        }
    //        else
    //        {
    //            CurrentPoint = null;
    //            Leave();
    //        }
    //    }));

    //    #endregion

    //    #region 方法

    //    private void StartMonitor()
    //    {
    //        if (_vRobot == null) return;

    //        if (whileToken != null)
    //        {
    //            whileToken?.Cancel();
    //            Thread.Sleep(150);
    //        }

    //        whileToken = new CancellationTokenSource();

    //        Task.Run(() =>
    //        {
    //            while (true)
    //            {
    //                if (whileToken == null || whileToken.IsCancellationRequested)
    //                {
    //                    break;
    //                }

    //                // 更新状态信息
    //                var dictStatus = Current.Tag.GetRobotStatus();
    //                foreach (var sItem in Current.StatusList)
    //                {
    //                    if (dictStatus.ContainsKey(sItem.Name))
    //                    {
    //                        sItem.Status = dictStatus[sItem.Name];
    //                    }
    //                    else
    //                    {
    //                        sItem.Status = false;
    //                    }
    //                }

    //                //更新点位
    //                RobotPoints = Current.Tag.GetCurrentPosion(CurCoordType, true);


    //                if (whileToken == null || whileToken.IsCancellationRequested)
    //                {
    //                    break;
    //                }

    //                Thread.Sleep(100);
    //                Debug.WriteLine("123");
    //            }
    //        }, whileToken.Token);

    //    }

    //    /// <summary>
    //    /// 机器人坐标系名称更新
    //    /// </summary>
    //    private void UpdateRobotCoordinateName()
    //    {
    //        if (CurCoordType == CoordType.Cartesian)
    //        {
    //            RobotAxis1Name = "X+";
    //            RobotAxis2Name = "X-";
    //            RobotAxis3Name = "Y+";
    //            RobotAxis4Name = "Y-";
    //            RobotAxis5Name = "Z+";
    //            RobotAxis6Name = "Z-";
    //            RobotAxis7Name = "U+";
    //            RobotAxis8Name = "U-";
    //            RobotAxis9Name = "V+";
    //            RobotAxis10Name = "V-";
    //            RobotAxis11Name = "W+";
    //            RobotAxis12Name = "W-";
    //        }
    //        else
    //        {
    //            RobotAxis1Name = "J1+";
    //            RobotAxis2Name = "J1-";
    //            RobotAxis3Name = "J2+";
    //            RobotAxis4Name = "J2-";
    //            RobotAxis5Name = "J3+";
    //            RobotAxis6Name = "J3-";
    //            RobotAxis7Name = "J4+";
    //            RobotAxis8Name = "J4-";
    //            RobotAxis9Name = "J5+";
    //            RobotAxis10Name = "J5-";
    //            RobotAxis11Name = "J6+";
    //            RobotAxis12Name = "J6-";
    //        }
    //    }

    //    /// <summary>
    //    /// 添加设备
    //    /// </summary>
    //    public override void AddNewItem()
    //    {
    //        dialogService.ShowVRobotDialog(r =>
    //        {
    //            if (r.Result == ButtonResult.OK)
    //            {
    //                if (r.Parameters.TryGetValue<VRobot>("VRobot", out var vR))
    //                {
    //                    deviceEngine.AddVirtual(vR);
    //                    LoadDevices();
    //                }
    //            }
    //        });
    //    }

    //    /// <summary>
    //    /// 加载所有的设备信息
    //    /// </summary>
    //    private void LoadDevices()
    //    {
    //        //线扫设备
    //        var devices = deviceEngine.GetDevices(typeof(VRobot));
    //        VRobotModels = new ObservableCollection<VRobotModel>();
    //        foreach (var device in devices)
    //        {
    //            var vRobot = device as VRobot;
    //            var vRobotModel = new VRobotModel(vRobot);
    //            vRobotModel.RobotName = vRobot.GetDevice().Name;
    //            VRobotModels.Add(vRobotModel);
    //        }

    //    }

    //    private void LoadTeachPoint()
    //    {
    //        if (Current != null)
    //        {
    //            if (!IsSixRobot)
    //            {
    //                RobotPointItems = new ObservableCollection<RobotPointItem>();
    //                var positions = Current.Tag.Positions;
    //                foreach (var node in positions)
    //                {
    //                    var axisPosition = node;
    //                    var position = new RobotPointItem()
    //                    {
    //                        Name = axisPosition.Name,
    //                        PositionX = axisPosition.PositionX,
    //                        PositionY = axisPosition.PositionY,
    //                        PositionZ = axisPosition.PositionZ,
    //                        PositionC = axisPosition.PositionC,
    //                        PositionH = axisPosition.PositionH,
    //                        Tag = axisPosition
    //                    };
    //                    RobotPointItems.Add(position);
    //                }
    //            }
    //            else
    //            {
    //                SixRobotPointItems = new ObservableCollection<SixRobotPointItem>();
    //                var positions = Current.Tag.SixPositions;
    //                foreach (var node in positions)
    //                {
    //                    var robotPosition = node;
    //                    var position = new SixRobotPointItem()
    //                    {
    //                        Name = robotPosition.Name,
    //                        PositionX = robotPosition.PositionX,
    //                        PositionY = robotPosition.PositionY,
    //                        PositionZ = robotPosition.PositionZ,
    //                        PositionU = robotPosition.PositionU,
    //                        PositionV = robotPosition.PositionV,
    //                        PositionW = robotPosition.PositionW,
    //                        Tag = robotPosition
    //                    };
    //                    SixRobotPointItems.Add(position);
    //                }
    //            }

    //        }
    //    }

    //    public override void Leave()
    //    {
    //        whileToken?.Cancel();
    //        Thread.Sleep(200);
    //        whileToken = null;
    //    }

    //    public override void OnNavigatedFrom(NavigationContext navigationContext)
    //    {
    //        base.OnNavigatedFrom(navigationContext);
    //        Leave();
    //    }

    //    #endregion

    //}
    internal class VRobotContentVM : PageVM
    {
        //定时刷新计时器
        private DispatcherTimer _timer;

        string Filename = @"C:\Users\g07392\source\repos\Robot\Position.csv";

        private CsvOperation csv;

        private int fun = 1; //这边暂时设置0是本地文件方式，1为从机械手读取点位

        private bool robotConnected = false;//机械手连接完成
        #region 属性

        /// <summary>
        /// 页面选择索引
        /// </summary>
        private int _TabPagChoose;

        public int TabPagChoose
        {
            get => _TabPagChoose;
            set
            {
                SetProperty(ref _TabPagChoose, value);
            }
        }
        /// <summary>
        /// 当前机械手状态
        /// </summary>
        private string _Statues;
        public string Statues
        {
            get => _Statues;
            set
            {
                SetProperty(ref _Statues, value);
            }
        }
        /// <summary>
        /// 回馈的消息
        /// </summary>
        private string _ReciveCommand;
        public string ReciveCommand
        {
            get => _ReciveCommand;
            set
            {
                SetProperty(ref _ReciveCommand, value);
            }
        }
        /// <summary>
        /// 坐标系的类型
        /// </summary>
        private List<string> _coordTypes;
        public List<string> CoordTypes
        {
            get => _coordTypes;
            set
            {
                SetProperty(ref _coordTypes, value);
            }
        }
        /// <summary>
        /// 当前坐标系类型
        /// </summary>
        private string _CurCoordType;
        public string CurCoordType
        {
            get => _CurCoordType;
            set
            {
                SetProperty(ref _CurCoordType, value);
                CurCoordTypeChange(value);
            }
        }

        private void CurCoordTypeChange(string va)
        {
            if (va == CoordTypes[0])
            {
                CoordNumVis = Visibility.Hidden;
                string sendMess = "BaseTool";
                string temp = "";
                RobotTX(sendMess, out temp);
                
            }
            else
            {
                CoordNumVis = Visibility.Visible;
                CurCoordNum = CoordNums[0];
            }
        }
        /// <summary>
        /// 坐标系的序列
        /// </summary>
        private List<string> _coordNums;
        public List<string> CoordNums
        {
            get => _coordNums;
            set
            {
                SetProperty(ref _coordNums, value);
            }
        }
        /// <summary>
        /// 当前坐标系序列
        /// </summary>
        private string _CurCoordNum;
        public string CurCoordNum
        {
            get => _CurCoordNum;
            set
            {
                SetProperty(ref _CurCoordNum, value);
                CurCoordNumChange(value);
            }
        }
        /// <summary>
        /// 工具坐标系数字变化
        /// </summary>
        /// <param name="va"></param>
        private void CurCoordNumChange(string va)
        {
            if (CurCoordType != CoordTypes[0])
            {
                string sendMess = "ToolChange," + va;
                string temp = "";
                RobotTX(sendMess, out temp);
            }
        }
        /// <summary>
        /// 坐标系序列是否可见
        /// </summary>
        private Visibility _CoordNumVis;
        public Visibility CoordNumVis
        {
            get => _CoordNumVis;
            set
            {
                SetProperty(ref _CoordNumVis, value);
            }
        }

        /// <summary>
        /// 机械手速度比率
        /// </summary>
        private string _SpeedRateText;
        public string SpeedRateText
        {
            get => _SpeedRateText;
            set
            {
                if (SetProperty(ref _SpeedRateText, value))
                {
                    // 文本改变时自动执行
                    OnTextChanged(value);
                }
            }
        }

        private void OnTextChanged(string v1)
        {
            SpeedSet();
        }
        /// <summary>
        /// X方向存动距离所有
        /// </summary>
        private List<string> _X_distances;
        public List<string> X_distances
        {
            get => _X_distances;
            set
            {
                SetProperty(ref _X_distances, value);
            }
        }

        private string _X_distance;
        public string X_distance
        {
            get => _X_distance;
            set
            {
                SetProperty(ref _X_distance, value);
            }
        }

        private List<string> _Y_distances;
        public List<string> Y_distances
        {
            get => _Y_distances;
            set
            {
                SetProperty(ref _Y_distances, value);
            }
        }

        private string _Y_distance;
        public string Y_distance
        {
            get => _Y_distance;
            set
            {
                SetProperty(ref _Y_distance, value);
            }
        }

        private List<string> _Z_distances;
        public List<string> Z_distances
        {
            get => _Z_distances;
            set
            {
                SetProperty(ref _Z_distances, value);
            }
        }

        private string _Z_distance;
        public string Z_distance
        {
            get => _Z_distance;
            set
            {
                SetProperty(ref _Z_distance, value);
            }
        }

        private List<string> _U_distances;
        public List<string> U_distances
        {
            get => _U_distances;
            set
            {
                SetProperty(ref _U_distances, value);
            }
        }

        private string _U_distance;
        public string U_distance
        {
            get => _U_distance;
            set
            {
                SetProperty(ref _U_distance, value);
            }
        }

        private List<string> _V_distances;
        public List<string> V_distances
        {
            get => _V_distances;
            set
            {
                SetProperty(ref _V_distances, value);
            }
        }

        private string _V_distance;
        public string V_distance
        {
            get => _V_distance;
            set
            {
                SetProperty(ref _V_distance, value);
            }
        }

        private List<string> _W_distances;
        public List<string> W_distances
        {
            get => _W_distances;
            set
            {
                SetProperty(ref _W_distances, value);
            }
        }

        private string _W_distance;
        public string W_distance
        {
            get => _W_distance;
            set
            {
                SetProperty(ref _W_distance, value);
            }
        }
        /// <summary>
        /// X点位
        /// </summary>
        private string _X_Position;
        public string X_Position
        {
            get => _X_Position;
            set
            {
                SetProperty(ref _X_Position, value);
            }
        }
        /// <summary>
        /// Y点位
        /// </summary>
        private string _Y_Position;
        public string Y_Position
        {
            get => _Y_Position;
            set
            {
                SetProperty(ref _Y_Position, value);
            }
        }
        /// <summary>
        /// Z点位
        /// </summary>
        private string _Z_Position;
        public string Z_Position
        {
            get => _Z_Position;
            set
            {
                SetProperty(ref _Z_Position, value);
            }
        }
        /// <summary>
        /// U点位
        /// </summary>
        private string _U_Position;
        public string U_Position
        {
            get => _U_Position;
            set
            {
                SetProperty(ref _U_Position, value);
            }
        }
        /// <summary>
        /// V点位
        /// </summary>
        private string _V_Position;
        public string V_Position
        {
            get => _V_Position;
            set
            {
                SetProperty(ref _V_Position, value);
            }
        }
        /// <summary>
        /// W点位
        /// </summary>
        private string _W_Position;
        public string W_Position
        {
            get => _W_Position;
            set
            {
                SetProperty(ref _W_Position, value);
            }
        }
        /// <summary>
        /// 所有的点位名称
        /// </summary>
        private List<string> _PositionNames;
        public List<string> PositionNames
        {
            get => _PositionNames;
            set
            {
                SetProperty(ref _PositionNames, value);
            }
        }
        /// <summary>
        /// 示教的点位名称
        /// </summary>
        private string _PositionName;
        public string PositionName
        {
            get => _PositionName;
            set
            {
                SetProperty(ref _PositionName, value);
            }
        }
        /// <summary>
        /// 示教的点位索引
        /// </summary>
        private int _PositionNum;
        public int PositionNum
        {
            get => _PositionNum;
            set
            {
                SetProperty(ref _PositionNum, value);
            }
        }
        /// <summary>
        /// 选择的移动点位
        /// </summary>
        private string _MovePositionName;
        public string MovePositionName
        {
            get => _MovePositionName;
            set
            {
                SetProperty(ref _MovePositionName, value);
            }
        }

        /// <summary>
        /// 运动的点位索引
        /// </summary>
        private int _MovePositionNum;
        public int MovePositionNum
        {
            get => _MovePositionNum;
            set
            {
                SetProperty(ref _MovePositionNum, value);
            }
        }
        /// <summary>
        /// 所有的运动方式
        /// </summary>
        private List<string> _MoveStypes;
        public List<string> MoveStypes
        {
            get => _MoveStypes;
            set
            {
                SetProperty(ref _MoveStypes, value);
            }
        }
        /// <summary>
        /// 运动方式
        /// </summary>
        private string _MoveStype;
        public string MoveStype
        {
            get => _MoveStype;
            set
            {
                SetProperty(ref _MoveStype, value);
            }
        }
        /// <summary>
        /// 点位数据
        /// </summary>
        private ObservableCollection<EpsonPosition> _Group;

        public ObservableCollection<EpsonPosition> Group
        {
            get => _Group;
            set
            {
                SetProperty(ref _Group, value);
            }
        }
        /// <summary>
        /// 16个IO输入
        /// </summary>
        private List<string> _IOinstatues;
        public List<string> IOinstatues
        {
            get => _IOinstatues;
            set
            {
                SetProperty(ref _IOinstatues, value);
            }
        }

        /// <summary>
        /// 16个IO输入
        /// </summary>
        private List<string> _IOoutstatues;
        public List<string> IOoutstatues
        {
            get => _IOoutstatues;
            set
            {
                SetProperty(ref _IOoutstatues, value);
            }
        }
        #endregion
        public IMotionController mController;

        ObservableCollection<VCommuncation> vCommuncations = new ObservableCollection<VCommuncation>();

        VCommuncation vCommuncation;
        /// <summary>
        /// 机器人列表
        /// </summary>
        private ObservableCollection<string> _vRobotModels;
        public ObservableCollection<string> VRobotModels
        {
            get { return _vRobotModels; }
            set { SetProperty(ref _vRobotModels, value); }
        }

        private string _RobotCommuncation;
        public string RobotCommuncation
        {
            get { return _RobotCommuncation; }
            set { SetProperty(ref _RobotCommuncation, value);

                Communcation(value);
            }
        }

        private void Communcation(string va)
        {
            vCommuncation = vCommuncations[VRobotchoose];
            robotConnected = false; //需要重新连接
        }

        private int _VRobotchoose;

        public int VRobotchoose
        {
            get => _VRobotchoose;
            set
            {
                SetProperty(ref _VRobotchoose, value);
            }
        }
        #region 事件
        /// <summary>
        /// 机器人连接
        /// </summary>
        public DelegateCommand RconRobot { get; private set; }
        /// <summary>
        /// 机器人运动
        /// </summary>
        public DelegateCommand<string> RobotMove { get; private set; }
        /// <summary>
        /// 机器人示教点位
        /// </summary>
        public DelegateCommand TechPos { get; private set; }
        /// <summary>
        /// 点位运动
        /// </summary>
        public DelegateCommand MovePos { get; private set; }
        /// <summary>
        /// 输出IO点动
        /// </summary>
        public DelegateCommand<string> Io_DO_Click { get; private set; }
        #endregion
        protected VRobotContentVM(ISimDeviceEngineUI _engine, IMotionController _motionController) : base(_engine)
        {
            mController = _motionController; 
            // 初始化定时器
            _timer = new DispatcherTimer();
            // 设置间隔（2秒 = 2000毫秒）
            _timer.Interval = TimeSpan.FromSeconds(2);
            // 绑定事件处理
            _timer.Tick += Timer_Tick;

            RconRobot = new DelegateCommand(ConnectRobot);

            RobotMove = new DelegateCommand<string>(Robot_Move);
            //          
            if (fun == 0)
            {
                TechPos = new DelegateCommand(Tech_PositionFile);
            }
            else
            {
                TechPos = new DelegateCommand(Tech_PositionRobot);
            }
            if (fun == 0)
            {
                MovePos = new DelegateCommand(AbsMoveFile);
            }
            else
            {
                MovePos = new DelegateCommand(AbsMoveRobot);
            }
            Io_DO_Click = new DelegateCommand<string>(IO_DO_Ctl);
            LoadForm();
        }


        #region 方法
        /// <summary>
        /// 页面参数的初始化
        /// </summary>
        private void LoadForm()
        {
            X_distances = new List<string> { "0.01", "0.1", "0.5", "1", "2", "5", "10" };
            Y_distances = new List<string> { "0.01", "0.1", "0.5", "1", "2", "5", "10" };
            Z_distances = new List<string> { "0.01", "0.1", "0.5", "1", "2", "5", "10" };
            U_distances = new List<string> { "0.01", "0.1", "0.5", "1", "2", "5", "10" };
            V_distances = new List<string> { "0.01", "0.1", "0.5", "1", "2", "5", "10" };
            W_distances = new List<string> { "0.01", "0.1", "0.5", "1", "2", "5", "10" };
            TabPagChoose = 0;
            CoordTypes = new List<string> { "默认", "工具"};
            CurCoordType = "默认";
            CoordNums = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            CurCoordNum = "0";
            CoordNumVis = Visibility.Hidden;
            X_distance = X_distances[2];
            Y_distance = Y_distances[2];
            Z_distance = Z_distances[2];
            U_distance = U_distances[2];
            V_distance = V_distances[2];
            W_distance = W_distances[2];
            MoveStypes = new List<string> { "go", "Move", "Jump" };
            MoveStype = MoveStypes[0];
            IOinstatues = new List<string> { "False","False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False" };
            IOoutstatues = new List<string> { "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False" };
            SpeedRateText = "10";

            PositionNames = new List<string> { "P0;(未定义)", "P1;(未定义)", "P2;(未定义)", "P3;(未定义)", "P4;(未定义)", "P5;(未定义)", "P6;(未定义)", "P7;(未定义)", "P8;(未定义)", "P9;(未定义)" };
            PositionName = PositionNames[0];
            MovePositionName = PositionNames[0];

            var devices = deviceEngine.GetDevices(typeof(VCommuncation));
            VRobotModels = new ObservableCollection<string>();
            foreach (var device in devices)
            {
                var VCommuncation = device as VCommuncation;
                if (VCommuncation.Name!="")
                {
                    VRobotModels.Add(VCommuncation.Name);
                    vCommuncations.Add(VCommuncation);
                }
            }
        }
        /// <summary>
        /// 切换机械手的界面初始化
        /// </summary>
        private void changerobotForm()
        {
            CurCoordType = "默认";
            CurCoordNum = "0";
            CoordNumVis = Visibility.Hidden;
        }
        
        /// <summary>
        /// 从本地读取点位数据
        /// </summary>
        private void ReadPositionFile()
        {

            csv = new CsvOperation(Filename);

            csv.LoadCsvFile();

            int colcount = csv.ColCount;
            List<string> head = new List<string>();
            for (int i = 0; i < colcount; i++)
            {
                head.Add(csv[0, i]);
            }
            int rowcount = csv.RowCount;
            Group = new ObservableCollection<EpsonPosition>();
            PositionNames.Clear();
            for (int i = 1; i < rowcount; i++)
            {
                EpsonPosition my = new EpsonPosition();
                Type type = my.GetType();
                PropertyInfo[] array2 = type.GetProperties();
                foreach (PropertyInfo info in array2)
                {
                    if (head.Contains(info.Name))
                    {
                        int index = head.IndexOf(info.Name);
                        info.SetValue(my, csv[i, index], null);
                    }
                }
                Group.Add(my);
                PositionNames.Add(my.Mark);
            }
            PositionName = PositionNames[0];
            MovePositionName = PositionNames[0];
        }
        /// <summary>
        /// 从机械手读取点位
        /// </summary>
        private void ReadPositionRobot()
        {
            bool yx = true;
            int index = 0;
            Group = new ObservableCollection<EpsonPosition>();
            PositionNames.Clear();
            while (yx)
            {
                string sendMess = "ReadRealPos,"+ index.ToString();
                string temp = "";
                RobotTX(sendMess, out temp);
                string[] re;
                bool jx = CheckResult(temp, out re);
                yx = jx;
                index++;
                if (yx)
                {
                    EpsonPosition temppos = new EpsonPosition();
                    temppos.Num = re[2];
                    temppos.Mark = re[3];
                    temppos.X = re[4];
                    temppos.Y = re[5];
                    temppos.Z = re[6];
                    temppos.U = re[7];
                    temppos.V = re[8];
                    temppos.W = re[9];
                    temppos.Description= re[10]; 
                    Group.Add(temppos);
                    PositionNames.Add(temppos.Mark);
                }             
            }
        }
        /// <summary>
        /// csv保存本地点位，暂时没有使用，后续通用模板使用
        /// </summary>
        private void SavePosition()
        {
            csv.Save(Filename);
        }
        //机械手通讯连接
        private void ConnectRobot()
        {
            if (TXSY)
            {
                vCommuncation.Open();
                if (vCommuncation.ConnectOK())
                {
                    robotConnected = true;
                    Statues = "机械手连接完成";
                    IProtocol protocol = new StringProtocol();
                    vCommuncation.Protocol = protocol;
                    bool cs=InitRobot();
                    if (cs)
                    {
                        SpeedSet();
                        if (fun == 0)
                        {
                            ReadPositionFile();
                        }
                        else
                        {
                            ReadPositionRobot();
                        }
                        _timer.Start();
                    }
                    else
                    {
                        Statues = "机械手连接失败";
                        robotConnected = false;
                    }
                }
                else
                {
                    Statues = "机械手连接失败";
                    robotConnected = false;
                }
            }
            else
            {
                Statues = "机械手连接完成";
                InitRobot();
                SpeedSet();
                if (fun == 0)
                {
                    ReadPositionFile();
                }
                else
                {
                    ReadPositionRobot();
                }
                _timer.Start();
            }

        }
        /// <summary>
        /// 初始化机械手
        /// </summary>
        private bool InitRobot()
        {
            string re = "";
            RobotTX("Init", out re);
            string[] strings;
            return (CheckResult(re,out strings));
        }
        /// <summary>
        /// 设置机械手速度
        /// </summary>
        private void SpeedSet()
        {
            byte c = 0;
            bool d = byte.TryParse(SpeedRateText, out c);
            if (d && c > 0 && c <= 100)
            {

            }
            else
            {
                SpeedRateText = "10";
                c = 10;
            }
            string sendMess = "SpeedSet," + SpeedRateText;
            string temp = "";
            RobotTX(sendMess, out temp);
        }
        /// <summary>
        /// 测试程序，为true则通讯，为false则虚拟通讯
        /// </summary>
        private bool TXSY = true;
        /// <summary>
        /// 通讯使用
        /// </summary>
        /// <param name="sendmessage"></param>
        /// <param name="outresult"></param>
        /// <returns></returns>
        private bool RobotTX(string sendmessage, out string outresult)
        {
            string temp = sendmessage + "\r\n";
            if (TXSY)
            {
                //连接成功，并且处于停止和默认状态时才能手动控制机械手，为了安全考虑
                if (vCommuncation != null&& robotConnected&& (mController.MachineStatus== EngineStatus.Stop|| mController.MachineStatus == EngineStatus.Idle))
                {
                    vCommuncation.ClearCache();
                    vCommuncation.Write(temp);
                    outresult = vCommuncation.ReadSingle<string>("", 10000,true,false);                    
                    ReciveCommand = outresult;
                    string[] chst;
                    if (CheckResult(outresult,out chst))
                    {
                        return true;
                    }
                    else
                    {
                        //一但检测到机器人掉线，直接停止刷新和通讯
                        if (ReciveCommand=="")
                        {
                            robotConnected = false;
                            _timer.Stop();
                        }                      
                        return false;
                    }
                }
                else
                {
                    outresult = "";//
                    return false;
                }
            }
            else
            {
                char[] fgf = { ',' };
                string[] c = sendmessage.Split(fgf, StringSplitOptions.RemoveEmptyEntries);
                string repay = "";
                switch (c[0])
                {
                    case "Init":
                        repay = "Init,ok,EPSON";
                        break;
                    case "SpeedSet":
                        repay = "SpeedSet,ok";
                        break;
                    case "ReadPositon":
                        // ReadPositon,结果,X,Y,Z,U,V,W,R,S,T,Hand,Elbow,Wrist,J1Flag,J2Flag,J4Flag,G6Flag,J1Angle,J4Angle
                        repay = "ReadPositon,ok,1,2,3,4,5,6,0,0,0,2,1,1,0,0,0,0,0,0";
                        break;
                    case "RealMove":
                        repay = "RealMove,ok";
                        break;
                    case "AbsMove":
                        repay = "AbsMove,ok";
                        break;
                    case "DOCtl":
                        repay = "DOCtl,ok";
                        break;
                    case "DIRead":
                        repay = "DIRead,ok,16";
                        break;
                    case "ToolChange":
                        repay = "ToolChange,ok";
                        break;
                    case "BaseTool":
                        repay = "BaseTool,ok";
                        break;
                    case "ReadRealPos":
                        if (c[1]=="10")
                        {
                            repay = "ReadRealPos,ng," + c[1] + "," + "P" + c[1] + "," + "1,2,3,4,5,6";
                        }
                        else
                        {
                            repay = "ReadRealPos,ok," + c[1] + "," + "P" + c[1] + "," + "2,2,2,2,2,2" + "," + "点位注解" + c[1];
                        }                     
                        break;
                    case "Teachposition":
                        repay = "Teachposition,ok";
                        break;
                    case "AbsMoveRobot":
                        repay = "AbsMoveRobot,ok";
                        break;
                    default:
                        break;
                }
                //string[] repaysz = sendmessage.Split(fgf, StringSplitOptions.RemoveEmptyEntries);
                outresult = repay;
                //outresult = repaysz.ToList<string>();
                ReciveCommand = repay;
                return true;
            }
        }

        //定时触发
        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (TabPagChoose)
            {
                case 0:
                    break;
                case 1: //刷新点位
                    string sendMess = "ReadPositon";
                    string temp = "";
                    RobotTX(sendMess, out temp);
                    string x_Pos = "";
                    string y_Pos = "";
                    string z_Pos = "";
                    string u_Pos = "";
                    string v_Pos = "";
                    string w_Pos = "";
                    HandPosition(temp, out x_Pos, out y_Pos, out z_Pos, out u_Pos, out v_Pos, out w_Pos);
                    X_Position = x_Pos;
                    Y_Position = y_Pos;
                    Z_Position = z_Pos;
                    U_Position = u_Pos;
                    V_Position = v_Pos;
                    W_Position = w_Pos;
                    break;
                case 3:
                    string sendDIMess = "DIRead,0";
                    string temp1 = "";
                    RobotTX(sendDIMess, out temp1);
                    string[] fj;
                    CheckResult(temp1, out fj);
                    ushort a = Convert.ToUInt16(fj[2]);
                    for (int i = 0; i <= 15; i++)
                    {
                        // 右移 i 位，然后与 1 进行与运算，得到第 i 位的值（0 或 1）
                        int bit = (a >> i) & 1;
                        IOinstatues[i] = bit == 1 ? "True" : "False";
                    }
                    break;
                default:
                    break;
            }
        }

        //检查机械手反馈结果
        public static bool CheckResult(string sb, out string[] result)
        {
            if (sb == "")
            {
                result = new string[] {"","" };
                return false;               
            }
            else
            {
                char[] a = { ',' };/*指定多个分隔符*/
                string[] b;
                b = sb.Split(a);
                result = b;
                return b[1] == "ok";
            }            
        }
        //ReadPositon,结果,X,Y,Z,U,V,W,R,S,T,Hand,Elbow,Wrist,J1Flag,J2Flag,J4Flag,G6Flag,J1Angle,G4Angle
        /// <summary>
        /// 解析点位结果
        /// </summary>
        /// <param name="result"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="w"></param>
        public static void HandPosition(string result, out string x, out string y, out string z, out string u, out string v, out string w)
        {
            if (result == "")
            {
                x = "0";
                y = "0";
                z = "0";
                u = "0";
                v = "0";
                w = "0";
            }
            else 
            {
                char[] a = { ',' };/*指定多个分隔符*/
                string[] re = new string[20];
                re = result.Split(a);
                x = re[2];
                y = re[3];
                z = re[4];
                u = re[5];
                v = re[6];
                w = re[7];
            }
        }
        private void Robot_Move(string parameter)
        {
            string sendMess = "";
            switch (parameter)
            {
                case "X+":
                    sendMess = "RealMove," + X_distance + ",0,0,0,0,0";
                    break;
                case "X-":
                    sendMess = "RealMove," + "-" + X_distance + ",0,0,0,0,0";
                    break;
                case "Y+":
                    sendMess = "RealMove,0," + Y_distance + ",0,0,0,0";
                    break;
                case "Y-":
                    sendMess = "RealMove,0," + "-" + Y_distance + ",0,0,0,0";
                    break;
                case "Z+":
                    sendMess = "RealMove,0,0," + Z_distance + ",0,0,0";
                    break;
                case "Z-":
                    sendMess = "RealMove,0,0," + "-" + Z_distance + ",0,0,0";
                    break;
                case "U+":
                    sendMess = "RealMove,0,0,0," + U_distance + ",0,0";
                    break;
                case "U-":
                    sendMess = "RealMove,0,0,0," + "-" + U_distance + ",0,0";
                    break;
                case "V+":
                    sendMess = "RealMove,0,0,0,0," + V_distance + ",0";
                    break;
                case "V-":
                    sendMess = "RealMove,0,0,0,0," + "-" + V_distance + ",0";
                    break;
                case "W+":
                    sendMess = "RealMove,0,0,0,0,0," + W_distance;
                    break;
                case "W-":
                    sendMess = "RealMove,0,0,0,0,0," + "-" + W_distance;
                    break;
                default:
                    break;
            }
            string temp = "";
            RobotTX(sendMess, out temp);
        }
        /// <summary>
        /// 文件内的点位执行运动
        /// </summary>
        private void AbsMoveFile()
        {
            //AbsMove,0,X,Y,Z,U,V,W,R,S,T,Hnad,Elbow,Wrist,J1Flag,J2Flag,J4Flag,G6Flag,J1Angle,G4Angle
            int index = 0;
            for (int i = 0; i < MoveStypes.Count; i++)
            {
                if (MoveStype == MoveStypes[i])
                {
                    index = i;
                    break;
                }
            }
            EpsonPosition fp = Group[index];
            string sendMess = "AbsMove," + index.ToString()+","+ fp.X + "," + fp.Y + "," + fp.Z + "," + fp.U + "," + fp.V + "," + fp.W + "," + fp.R
                + "," + fp.S + "," + fp.T + "," + fp.Hand + "," + fp.Elbow + "," + fp.Wrist + "," + fp.J1Flag + "," + fp.J2Flag + "," + fp.J4Flag + "," + fp.J6Flag + "," + fp.J1Angle + "," + fp.J4Angle;
            string temp = "";
            RobotTX(sendMess, out temp);
        }
        /// <summary>
        /// 机械手内部点位运动
        /// </summary>
        private void AbsMoveRobot()
        {
            int index = 0;
            for (int i=0;i< MoveStypes.Count;i++)
            {
                if (MoveStype == MoveStypes[i])
                {
                    index = i;
                    break;
                }           
            }
            string sendMess = "AbsMoveRobot," + MovePositionNum.ToString() + "," + index.ToString();
            string temp = "";
            RobotTX(sendMess, out temp);
        }
        /// <summary>
        /// 示教文件点位，没有更新csv表格数据
        /// </summary>
        private void Tech_PositionFile()
        {
            string sendMess = "ReadPositon";
            string temp = "";
            RobotTX(sendMess, out temp);
            string[] re;
            CheckResult(temp,out re);
            Group[PositionNum].X = re[2];
            Group[PositionNum].Y = re[3];
            Group[PositionNum].Z = re[4];
            Group[PositionNum].U = re[5];
            Group[PositionNum].V = re[6];
            Group[PositionNum].W = re[7];
            Group[PositionNum].R = re[8];
            Group[PositionNum].S = re[9];
            Group[PositionNum].T = re[10];
            Group[PositionNum].Hand = re[11];
            Group[PositionNum].Elbow = re[12];
            Group[PositionNum].Wrist = re[13];
            Group[PositionNum].J1Flag = re[14];
            Group[PositionNum].J2Flag = re[15];
            Group[PositionNum].J4Flag = re[16];
            Group[PositionNum].J6Flag = re[17];
            Group[PositionNum].J1Angle = re[18];
            Group[PositionNum].J4Angle = re[19];
        }
        /// <summary>
        /// 示教机械手点位
        /// </summary>
        private void Tech_PositionRobot()
        {
            string sendMess = "Teachposition,"+ PositionNum.ToString();
            string temp = "";
            RobotTX(sendMess, out temp);
            ReadPositionRobot();
        }

        private void IO_DO_Ctl(string index)
        {
            int c = Int32.Parse(index);
            bool currentState = (IOoutstatues[c] == "True");
            IOoutstatues[c] = currentState ? "False" : "True";
            string sendMess = "DOCtl," + index + "," + IOoutstatues[c];
            string temp = "";
            RobotTX(sendMess, out temp);
        }
        #endregion
    }
}
