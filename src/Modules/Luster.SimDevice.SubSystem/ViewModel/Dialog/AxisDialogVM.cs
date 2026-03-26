#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       AxisDialogVM.cs
* 创建时间:       2022/4/21 18:50:34
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d60dd329-a7b9-4676-bba2-74e9d7d54375
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/21 18:50:34
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.Extensions;
using Luster.SimDevice.Real;
using Luster.SimDevice.MotionCards;
using Prism.Commands;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.DataModels;
using System.ComponentModel.DataAnnotations;
using Luster.Motion.DataStruct;
using Luster.Common.DataStruct.Attributes;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    /// <summary>
    /// 添加轴对话框
    /// </summary>
    public class AxisDialogVM : DialogVM
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_engine"></param>
        protected AxisDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            HomeDirList = typeof(HomeMode).EnumToDataSource();
            PriorityList = typeof(Priority).EnumToDataSource();
            // 初始化控制块
            MotionCardList = deviceEngine.GetRealDevices(typeof(IMotionCard));
        }

        /// <summary>
        /// list集合
        /// </summary>
        public List<KeyValue> HomeDirList { get; set; }

        /// <summary>
        /// list集合
        /// </summary>
        public List<KeyValue> PriorityList { get; set; }


        /// <summary>
        /// 轴卡列表
        /// </summary>
        private List<IDevice> motionCardList;
        public List<IDevice> MotionCardList
        {
            get { return motionCardList; }
            set { SetProperty(ref motionCardList, value); }
        }


        #region 通用参数
        /// <summary>
        /// 模组名称
        /// </summary>
        private IDevice _mDevice;
        [Required]
        public IDevice MDevice { get => _mDevice; set => SetProperty(ref _mDevice, value); }

        /// <summary>
        /// 模块名称
        /// </summary>
        private string _module;
        [Required]
        public string Module
        {
            get { return _module; }
            set { SetProperty(ref _module, value); }
        }

        /// <summary>
        /// 起始编号
        /// </summary>
        private int _startNo = 1;
        [Range(0, 1000)]
        public int StartNo { get => _startNo; set => SetProperty(ref _startNo, value); }

        /// <summary>
        /// 一次创建轴的数量
        /// </summary>
        private int _axisNumber = 1;
        [Range(1, 1000)]
        public int AxisNumber { get => _axisNumber; set => SetProperty(ref _axisNumber, value); }

        /// <summary>
        /// 轴别名
        /// </summary>
        private string _name;
        [Required(ErrorMessage = "名称必须输入")]
        public string AxisName { get => _name; set => SetProperty(ref _name, value); }
        #endregion

        #region 常规参数
        /// <summary>
        /// 加速度，单位mm*mm/s
        /// </summary>
        private int _Acc = 100;
        [Range(100, 100000)]
        public int Acc { get => _Acc; set => SetProperty(ref _Acc, value); }

        /// <summary>
        /// 减速度，单位mm*mm/s
        /// </summary>
        private int _Dec = 100;
        [Range(100, 100000)]
        public int Dec { get => _Dec; set => SetProperty(ref _Dec, value); }

        /// <summary>
        /// 定位速度，单位mm/s
        /// </summary>
        private int _MoveSpeed = 1000;
        [Range(1, 3000)]
        public int MoveSpeed { get => _MoveSpeed; set => SetProperty(ref _MoveSpeed, value); }

        /// <summary>
        ///点动速度，单位 mm/s
        /// </summary>
        private int _JogSpeed = 100;
        [Range(1, 2000)]
        public int JogSpeed { get => _JogSpeed; set => SetProperty(ref _JogSpeed, value); }

        /// <summary>
        /// 脉冲毫米比，即多少个脉冲走1mm
        /// </summary>
        private int _PerPluse = 1000;
        [Range(1, 2000)]
        public int PerPluse { get => _PerPluse; set => SetProperty(ref _PerPluse, value); }

        /// <summary>
        /// 正向软限位,单位为mm
        /// </summary>
        private float _SoftLimitPL;
        [Ignore]
        [Range(1, 100000)]
        public float SoftLimitPL { get => _SoftLimitPL; set => SetProperty(ref _SoftLimitPL, value); }
        /// <summary>
        /// 负向软限位,单位为mm
        /// </summary>
        private float _SoftLimitML;
        [Range(-10000, 0)]
        public float SoftLimitML { get => _SoftLimitML; set => SetProperty(ref _SoftLimitML, value); }

        /// <summary>
        /// 轴定位稳定时间，单位为ms
        /// </summary>
        private int _Stime = 10;
        [Range(1, 1000)]
        public int Stime { get => _Stime; set => SetProperty(ref _Stime, value); }
        /// <summary>
        /// 轴定位允许偏差，单位为脉冲
        /// </summary>
        private int _AxisBand = 50;
        [Range(1, 500)]
        public int AxisBand { get => _AxisBand; set => SetProperty(ref _AxisBand, value); }
        #endregion

        #region 回零参数
        /// <summary>
        /// 回零模式
        /// </summary>
        private HomeMode _homeMode = HomeMode.Negative;
        [Required]
        public HomeMode HomeMode { get => _homeMode; set => SetProperty(ref _homeMode, value); }

        /// <summary>
        /// 回零高速
        /// </summary>
        private uint _HomeSpeedHigh = 100;
        [Range(1, 500)]
        public uint HomeSpeedHigh { get => _HomeSpeedHigh; set => SetProperty(ref _HomeSpeedHigh, value); }

        /// <summary>
        /// 回零低速
        /// </summary>
        private uint _HomeSpeedLow = 10;
        [Range(1, 500)]
        public uint HomeSpeedLow { get => _HomeSpeedLow; set => SetProperty(ref _HomeSpeedLow, value); }

        /// <summary>
        /// 加速度
        /// </summary>
        private uint _HomeAcc = 100;
        [Range(1, 1000000)]
        public uint HomeAcc { get => _HomeAcc; set => SetProperty(ref _HomeAcc, value); }

        /// <summary>
        /// 减速度
        /// </summary>
        private uint _HomeDec = 100;
        [Range(1, 1000000)]
        public uint HomeDec { get => _HomeDec; set => SetProperty(ref _HomeDec, value); }

        /// <summary>
        /// 回零偏移
        /// </summary>
        private float _HomeOffset = 0;
        [Range(0, 1000)]
        public float HomeOffset { get => _HomeOffset; set => SetProperty(ref _HomeOffset, value); }

        /// <summary>
        /// 回零优先级
        /// </summary>
        private Priority _HomePriority = Priority.Low;
        [Required]
        public Priority HomePriority { get => _HomePriority; set => SetProperty(ref _HomePriority, value); }

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

        /// <summary>
        /// 自动扫描
        /// </summary>
        private DelegateCommand _scanCommand;
        public DelegateCommand ScanCommand => _scanCommand ?? (_scanCommand = new DelegateCommand(() =>
        {
            // 是否在线
            if (SimEngineUI.DeviceMode != DeviceMode.Real)
            {
                SimEngineUI.OnAlert(LogType.Warning, "请先调整在线模式");
                return;
            }

            // 是否选择设备
            if (MDevice == null)
            {
                SimEngineUI.OnAlert(LogType.Info, "请先选择控制卡");
                return;
            }

            if (MDevice is IMotionCard card)
            {
                // 扫描轴数量
                card.ScanAxis(out var axisNum);
                StartNo = 0;
                AxisNumber = (int)axisNum;
                SimEngineUI.OnLog(LogType.Info, $"共扫描出轴数量为:{AxisNumber}");
            }
        }));



        private DelegateCommand _calCommand;
        public DelegateCommand CalCommand => _calCommand ?? (_calCommand = new DelegateCommand(() =>
        {
            CalPulseRatio();
        }));



        /// <summary>
        /// ok 按钮
        /// </summary>
        /// <param name="result"></param>
        /// <exception cref="DeviceException"></exception>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);

            // 总数量
            int len = StartNo + AxisNumber;
            List<int> axisNos = new List<int>();

            // 确认轴的编号是否重复
            for (int i = StartNo; i < len; i++)
            {
                axisNos.Add(i);
            }

            // 相同的的控制卡才检查是否轴号匹配
            var existNos = deviceEngine.GetDevices(typeof(VAxis))
                .Where(u => u.DeviceID == MDevice.ID)
                .Select(u => (u as VAxis).AxisNo)
                .ToList();
            if (existNos.Exists(u => axisNos.Contains(u)))
            {
                throw new DeviceException("新生成的轴编号和已有轴编号重复!");
            }

            // 构建虚拟设备对象
            for (int i = StartNo; i < len; i++)
            {
                string axisName = $"{AxisName}_{i}";
                VAxis axis = new VAxis()
                {
                    ID = Guid.NewGuid(),
                    DeviceID = MDevice.ID,
                    Name = axisName,
                    Module = this.Module,
                    AxisNo = i,
                    Acc = Acc,
                    Dec = Dec,
                    MoveSpeed = MoveSpeed,
                    JogSpeed = JogSpeed,
                    PerPluse = PerPluse,
                    SoftLimitML = SoftLimitML,
                    SoftLimitPL = SoftLimitPL,
                    Stime = Stime,
                    AxisBand = AxisBand,
                    HomeMode = HomeMode,
                    HomeAcc = HomeAcc,
                    HomeDec = HomeDec,
                    HomePriority = HomePriority,
                    HomeOffset = HomeOffset,
                    HomeSpeedHigh = HomeSpeedHigh,
                    HomeSpeedLow = HomeSpeedLow,
                    HaveTransmission = HaveTransmission,
                    CommandPulse = CommandPulse,
                    CircleDisplacement = CircleDisplacement,
                    GearRatioNumerator = GearRatioNumerator,
                    GearRatioDenominator = GearRatioDenominator
                };

                // 添加对象
                deviceEngine.AddVirtual(axis);
            }
        }

        private void CalPulseRatio()
        {
            if(!HaveTransmission)
            {
                PerPluse = CommandPulse / CircleDisplacement;
            }
            else
            {
                PerPluse = CommandPulse*GearRatioNumerator / CircleDisplacement*GearRatioDenominator;
            }
        }
    }
}