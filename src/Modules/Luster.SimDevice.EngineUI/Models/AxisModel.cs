#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       AxisModel.cs
* 创建时间:       2022/4/21 19:00:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9469fdd6-b32f-4728-9c9f-a086befd18d3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/21 19:00:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Luster.SimDevice.EngineUI.Models
{

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
                    Alias = "OPS";
                    break;
                case AxisStatus.SvOn:
                    Alias = "SvOn";
                    break;
            }
        }

    }

    /// <summary>
    /// 参数
    /// </summary>
    public class AxisModel : BindableBase
    {
        /// <summary>
        /// 对应的ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected; set
            {
                bool src = _isSelected;
                SetProperty(ref _isSelected, value);
                if (src != value)
                    OnSelected(value);
            }
        }

        /// <summary>
        /// 模组名称
        /// </summary>
        private string _module;
        public string Module
        {
            get => _module; set
            {
                SetProperty(ref _module, value);
                if (Tag != null)
                {
                    Tag.Module = value;
                }
            }
        }

        #region 常规参数
        private string _name;
        public string Name
        {
            get => _name; set
            {
                SetProperty(ref _name, value);
                if (Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        private int _axisNo;
        public int AxisNo { get => _axisNo; set => SetProperty(ref _axisNo, value); }

        /// <summary>
        /// 轴类型
        /// </summary>
        private AxisType _axisType;
        public AxisType AxisType
        {
            get => _axisType; set
            {
                SetProperty(ref _axisType, value);
                if (Tag != null)
                {
                    Tag.AxisType = value;
                }

                ShowHome = value != AxisType.None;
            }
        }

        private AxisPML axisPML;
        public AxisPML AxisPML
        {
            get => axisPML;
            set
            {
                SetProperty(ref axisPML, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.AxisPML = value;
                }
            }
        }

        
        private AxisForward _axisForward;
        public AxisForward AxisForward
        {
            get => _axisForward;
            set
            {
                SetProperty(ref _axisForward, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.AxisForward = value;
                }
            }
        }

        private int _Acc;
        public int Acc
        {
            get => _Acc; set
            {
                if (value < 10)
                {
                    return;
                }
                SetProperty(ref _Acc, value);
                if (Tag != null)
                {
                    Tag.Acc = value;
                }
            }
        }

        private int _Dec;
        public int Dec
        {
            get => _Dec; set
            {
                if (value < 10)
                {
                    return;
                }
                SetProperty(ref _Dec, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.Dec = value;
                }
            }
        }

        private double _MoveSpeed;
        public double MoveSpeed
        {
            get => _MoveSpeed; set
            {
                if (value < 10)
                {
                    return;
                }

                SetProperty(ref _MoveSpeed, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.MoveSpeed = value;
                }
            }
        }

        private int _JogSpeed;
        public int JogSpeed
        {
            get => _JogSpeed; set
            {
                if (value < 1)
                {
                    value = 1;
                }
                else if(value>10)
                { 
                    value = 10; 
                }

                SetProperty(ref _JogSpeed, value);

                if (Tag != null && _isTwoWay)
                {
                    Tag.JogSpeed = value;
                }
            }
        }

        private int _PerPluse;
        public int PerPluse
        {
            get => _PerPluse; set
            {
                SetProperty(ref _PerPluse, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.PerPluse = value;
                }
            }
        }

        private float _SoftLimitPL;
        public float SoftLimitPL { get => _SoftLimitPL; set => SetProperty(ref _SoftLimitPL, value); }

        private float _SoftLimitML;
        public float SoftLimitML { get => _SoftLimitML; set => SetProperty(ref _SoftLimitML, value); }

        private int _Stime;
        public int Stime { get => _Stime; set => SetProperty(ref _Stime, value); }

        private int _AxisBand;
        public int AxisBand { get => _AxisBand; set => SetProperty(ref _AxisBand, value); }

        /// <summary>
        /// 当前位置
        /// </summary>
        private double _currentPos;
        public double CurrentPos { get => _currentPos; set => SetProperty(ref _currentPos, value); }

        /// <summary>
        /// 示教位置
        /// </summary>
        private double _dstPostion;
        public double DstPostion { get => _dstPostion; set => SetProperty(ref _dstPostion, value); }

        /// <summary>
        /// 示教位置
        /// </summary>
        private double _teachPos;
        public double TeachPos { get => _teachPos; set => SetProperty(ref _teachPos, value); }

        /// <summary>
        /// 移动优先级
        /// </summary>
        private Priority _movePriority;
        public Priority MovePriority
        {
            get => _movePriority; set
            {
                var src = _movePriority;

                SetProperty(ref _movePriority, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.MovePriority = value;
                }
            }
        }

        
        #endregion

        #region 回零参数
        /// <summary>
        /// 是否需要回零
        /// </summary>
        private bool _showHome;
        public bool ShowHome
        {
            get { return _showHome; }
            set
            {
                SetProperty(ref _showHome, value);
            }
        }

        /// <summary>
        /// 回零高速
        /// </summary>
        private int _homeSort;
        public int HomeSort
        {
            get => _homeSort; set
            {
                SetProperty(ref _homeSort, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.HomeSort = value;
                }
            }
        }

        /// <summary>
        /// 回零方式
        /// </summary>
        private HomeMode _homeMode;
        public HomeMode HomeMode
        {
            get => _homeMode; set
            {
                SetProperty(ref _homeMode, value);
                if (Tag != null && _isTwoWay)
                {
                    Tag.HomeMode = value;
                }
            }
        }

        /// <summary>
        /// 回零高速
        /// </summary>
        private uint _HomeSpeedHigh;
        public uint HomeSpeedHigh { get => _HomeSpeedHigh; set { SetProperty(ref _HomeSpeedHigh, value); } }

        /// <summary>
        /// 回0速度2
        /// </summary>
        /// 
        private uint _HomeSpeedLow;
        public uint HomeSpeedLow { get => _HomeSpeedLow; set => SetProperty(ref _HomeSpeedLow, value); }

        /// <summary>
        /// 加速度
        /// </summary>
        private uint _HomeAcc;
        public uint HomeAcc { get => _HomeAcc; set => SetProperty(ref _HomeAcc, value); }

        /// <summary>
        /// 减速度
        /// </summary>
        private uint _HomeDec;
        public uint HomeDec { get => _HomeDec; set => SetProperty(ref _HomeDec, value); }

        /// <summary>
        /// 回零偏移
        /// </summary>
        private float _HomeOffset;
        public float HomeOffset { get => _HomeOffset; set => SetProperty(ref _HomeOffset, value); }

        /// <summary>
        /// 回0优先级
        /// </summary>
        private Priority _HomePriority;
        public Priority HomePriority { get => _HomePriority; set => SetProperty(ref _HomePriority, value); }

        /// <summary>
        /// 回的位置
        /// </summary>
        private double _homePos;
        public double HomePos { get => _homePos; set => SetProperty(ref _homePos, value); }



        /// <summary>
        /// 是否使用齿轮箱
        /// </summary>
        private bool _haveTransmission;
        public bool HaveTransmission { get => _haveTransmission; set => SetProperty(ref _haveTransmission, value); }


        /// <summary>
        /// 指令脉冲
        /// </summary>
        private int _commandPulse = 1000;
        [Range(1, 1000000)]
        public int CommandPulse { get => _commandPulse; set => SetProperty(ref _commandPulse, value); }

        /// <summary>
        /// 一圈移动量
        /// </summary>
        private int _circleDisplacement = 10;
        [Range(1, 1000000)]
        public int CircleDisplacement { get => _circleDisplacement; set => SetProperty(ref _circleDisplacement, value); }


        /// <summary>
        /// 齿轮比分子
        /// </summary>
        private int _gearRatioNumerator = 10;
        [Range(1, 1000000)]
        public int GearRatioNumerator { get => _gearRatioNumerator; set => SetProperty(ref _gearRatioNumerator, value); }

        /// <summary>
        /// 齿轮比分母
        /// </summary>
        private int _gearRatioDenominator = 1;
        [Range(1, 1000000)]
        public int GearRatioDenominator { get => _gearRatioDenominator; set => SetProperty(ref _gearRatioDenominator, value); }

        #endregion

        #region 运动限制条件

        /// <summary>
        /// 运动最小范围
        /// </summary>
        private double _motionLimitMin;
        public double MotionLimitMin
        {
            get { return _motionLimitMin; }
            set
            {
                SetProperty(ref _motionLimitMin, value);
            }
        }

        /// <summary>
        /// 运动最大范围
        /// </summary>
        private double _motionLimitMax;
        public double MotionLimitMax
        {
            get { return _motionLimitMax; }
            set
            {
                SetProperty(ref _motionLimitMax, value);
            }
        }

        /// <summary>
        /// 运动判断IO
        /// </summary>
        private VIO _motionLimitIO;
        public VIO MotionLimitIO
        {
            get { return _motionLimitIO; }
            set
            {
                SetProperty(ref _motionLimitIO, value);
            }
        }

        private bool _isIn;
        public bool IsIn
        {
            get { return _isIn; }
            set
            {
                SetProperty(ref _isIn, value);
            }
        }

        private int _selectIOValue;
        public int SelectIOValue
        {
            get { return _selectIOValue; }
            set
            {
                SetProperty(ref _selectIOValue, value);
            }
        }
        //private List<VAxisMoveDisModel> _listMoveDisLimit;
        //[Ignore]
        //public List<VAxisMoveDisModel> ListMoveDisLimit
        //{
        //    get => _listMoveDisLimit; set
        //    {
        //        SetProperty(ref _listMoveDisLimit, value);
        //        //if (Tag != null)
        //        //{
        //        //    Tag.MoveDisLimit = value;
        //        //}
        //    }
        //}

        //private List<VIOMoveCheckModel> _listMoveIOLimit;
        //[Ignore]
        //public List<VIOMoveCheckModel> ListMoveIOLimit
        //{
        //    get => _listMoveIOLimit; set
        //    {
        //        SetProperty(ref _listMoveIOLimit, value);
        //        if (Tag != null)
        //        {
        //            Tag.MoveIOLimit = value;
        //        }
        //    }
        //}

        #endregion

        /// <summary>
        /// 设置运行状态
        /// </summary>
        public List<AxisStatusModel> StatusList { get; set; }

        /// <summary>
        /// 对象
        /// </summary>
        public VAxis Tag { get; set; }

        /// <summary>
        /// 选择发生了变更
        /// </summary>
        public event Action<bool> SelectedChanged;

        private bool _isTwoWay = false;
        public void OnSelected(bool isSelected)
        {
            SelectedChanged?.Invoke(isSelected);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vAxis"></param>
        public AxisModel(VAxis vAxis, bool isTwoWay = true, CancellationTokenSource cancellationToken = null)
        {
            _isTwoWay = isTwoWay;
            ID = vAxis.ID;
            IsSelected = false;
            Module = vAxis.Module;
            Name = vAxis.Name;
            AxisNo = vAxis.AxisNo;
            AxisType = vAxis.AxisType;
            Acc = vAxis.Acc;
            Dec = vAxis.Dec;
            MoveSpeed = vAxis.MoveSpeed;
            JogSpeed = vAxis.JogSpeed;
            PerPluse = vAxis.PerPluse;
            SoftLimitPL = vAxis.SoftLimitPL;
            SoftLimitML = vAxis.SoftLimitML;
            Stime = vAxis.Stime;
            AxisBand = vAxis.AxisBand;
            HaveTransmission = vAxis.HaveTransmission;
            CommandPulse = vAxis.CommandPulse;
            CircleDisplacement = vAxis.CircleDisplacement;
            GearRatioNumerator = vAxis.GearRatioNumerator;
            GearRatioDenominator = vAxis.GearRatioDenominator;

            // 运动优先级
            MovePriority = vAxis.MovePriority;

            HomeMode = vAxis.HomeMode;
            HomeSpeedHigh = vAxis.HomeSpeedHigh;
            HomeSpeedLow = vAxis.HomeSpeedLow;
            HomeAcc = vAxis.HomeAcc;

            HomeDec = vAxis.HomeDec;
            HomeOffset = vAxis.HomeOffset;
            HomePriority = vAxis.HomePriority;
            ShowHome = vAxis.AxisType != AxisType.None;
            HomeSort = vAxis.HomeSort;

            MotionLimitMin = vAxis.MotionLimitMin;
            MotionLimitMax = vAxis.MotionLimitMax;
            MotionLimitIO = vAxis.MotionLimitIO;
            SelectIOValue = vAxis.SelectIOValue;
            IsIn = vAxis.IsIn;
            Tag = vAxis;
            AxisPML = vAxis.AxisPML;
            AxisForward = vAxis.AxisForward;


            StatusList = new List<AxisStatusModel>()
            {
                new AxisStatusModel(AxisStatus.Alarm,false),
                new AxisStatusModel(AxisStatus.Pel,false),
                new AxisStatusModel(AxisStatus.Mel,false),
                new AxisStatusModel(AxisStatus.Org,false),
                new AxisStatusModel(AxisStatus.Emg,false),
                new AxisStatusModel(AxisStatus.Moving,false),
                new AxisStatusModel(AxisStatus.OnPos,false),
                new AxisStatusModel(AxisStatus.SvOn,false)
            };

            // 启动监控
            if (cancellationToken != null)
            {
                StartMonitor(cancellationToken);
            }
        }

        /// <summary>
        /// 更新轴状态
        /// </summary>
        private void StartMonitor(CancellationTokenSource tokenS)
        {
            if (Tag == null || Tag.AxisType == AxisType.None) return;

            Task.Run(() =>
            {
                while (true)
                {
                    // 外部取消后
                    if (tokenS.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        // 更新状态信息
                        var dictStatus = Tag.GetAxisStatus(false);
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

                        CurrentPos = Tag.GetCurrentPos();
                    }
                    catch
                    {
                        // 通讯异常时忽略，下一轮继续重试
                    }

                    Thread.Sleep(100);
                }
            }, tokenS.Token);
        }

        public void CalPulseRatio()
        {
            if (!HaveTransmission)
            {
                if (CircleDisplacement == 0) return;
                PerPluse = CommandPulse / CircleDisplacement;
            }
            else
            {
                if (CircleDisplacement == 0) return;
                if (GearRatioDenominator == 0) return;
                PerPluse = CommandPulse * GearRatioNumerator / CircleDisplacement * GearRatioDenominator;
            }
        }

        #region 命令
        /// <summary>
        /// 绝对运动
        /// </summary>
        private DelegateCommand _moveAbsCommand;
        public DelegateCommand MoveAbsCommand => _moveAbsCommand ?? (_moveAbsCommand = new DelegateCommand(() =>
        {
            Tag.MoveAbs(DstPostion, MoveSpeed);
        }));

        /// <summary>
        /// 相对运动
        /// </summary>
        private DelegateCommand _moveRelCommand;
        public DelegateCommand MoveRelCommand => _moveRelCommand ?? (_moveRelCommand = new DelegateCommand(() =>
        {
            Tag.MoveRel(DstPostion, MoveSpeed);
        }));


        private bool _isBusy = false;

        private DelegateCommand<string> _moveRelCommandAdd;
        public DelegateCommand<string> MoveRelCommandAdd => _moveRelCommandAdd ?? (_moveRelCommandAdd = new DelegateCommand<string>((string direction) =>
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            try
            {
                if (double.TryParse(direction, out double dir))
                {
                    Tag.MoveRel(DstPostion, MoveSpeed);
                    //System.Diagnostics.Debug.WriteLine($"Moving axis by {dir} * {DstPostion} units.");
                }
            }
            finally
            {
                _isBusy = false;
            }
        }));

        /// <summary>
        /// 相对运动
        /// </summary>
        private DelegateCommand<string> _moveRelCommandSub;
        public DelegateCommand<string> MoveRelCommandSub => _moveRelCommandSub ?? (_moveRelCommandSub = new DelegateCommand<string>((string direction) =>
        {
            if (_isBusy)
            {
                return;
            }

            _isBusy = true;
            try
            {
                if (double.TryParse(direction, out double dir))
                {
                    Tag.MoveRel(dir * DstPostion, MoveSpeed);
                    //System.Diagnostics.Debug.WriteLine($"Moving axis by {dir} * {DstPostion} units.");
                }
            }
            finally
            {
                _isBusy = false;
            }
        }));

        /// <summary>
        /// 回零
        /// </summary>
        private DelegateCommand _homeCommand;
        public DelegateCommand HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand(() =>
        {
            Task.Run(() => { Tag.Home(); Tag.CheckHomeDone(); });
        }));

        #region JOG运动模式
        /// <summary>
        /// JOG+点动
        /// </summary>
        private DelegateCommand _jogCommandAdd;
        public DelegateCommand JogCommandAdd => _jogCommandAdd ?? (_jogCommandAdd = new DelegateCommand(() =>
        {
            Tag.Jog(true, JogSpeed);
        }));

        /// <summary>
        /// JOG-点动
        /// </summary>
        private DelegateCommand _jogCommandSub;
        public DelegateCommand JogCommandSub => _jogCommandSub ?? (_jogCommandSub = new DelegateCommand(() =>
        {
            Tag.Jog(false, JogSpeed);
        }));

        /// <summary>
        /// 停止运动
        /// </summary>
        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand(() =>
        {
            // 显示对象
            Tag.Stop();
        }));

        /// <summary>
        /// 清理错误信息
        /// </summary>
        private DelegateCommand _clearErrorCommand;
        public DelegateCommand ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand(() =>
        {
            // 显示对象
            Tag.ResetStatus();
        }));

        /// <summary>
        /// 是否进行使能操作
        /// </summary>
        private DelegateCommand<string> _enableCommand;
        public DelegateCommand<string> EnableCommand => _enableCommand ?? (_enableCommand = new DelegateCommand<string>((isEnable) =>
        {
            if (bool.TryParse(isEnable, out var result))
            {
                // 显示对象
                Tag.ServOn(result);
            }
        }));
        #endregion

        #endregion
    }

}