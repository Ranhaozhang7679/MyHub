#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VirtualAxis
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Virtual
* 文 件 名:       VirtualAxis.cs
* 创建时间:       2022/4/6 14:44:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e843c19c-d9a8-448b-b40a-0ee47c53d8dc
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/6 14:44:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.FXVirtual;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 轴
    /// </summary>
    public class VAxis : VirtualDeviceBase, IMaintain, IAutoCheck, IStop, IPosition, IPause, IDeviceError
    {
        public override int Sort => 2;

        public new event Action<IVirtualDevice, string, object, object> PropertyChanged;
        /// <summary>
        /// 设备具备的轴错误对象
        /// </summary>
        public override DeviceError[] ErrorCodes
        => new DeviceError[]
        {
            DeviceError.ZeroFail,
            DeviceError.SerOnFail,
            DeviceError.PelFail,
            DeviceError.MelFail,
            DeviceError.MoveTimeFail
        };

        #region 常规参数
        /// <summary>
        /// 轴号
        /// </summary>
        public int AxisNo { get; set; }

        /// <summary>
        /// 加速度
        /// </summary>
        public int Acc { get; set; }

        /// <summary>
        /// 减速度
        /// </summary>
        public int Dec { get; set; }

        /// <summary>
        /// 运动速度
        /// </summary>
        private double _moveSpeed;
        public double MoveSpeed
        {
            get => _moveSpeed; set
            {
                double srcV = _moveSpeed;
                _moveSpeed = value;
                if (srcV != value)
                    OnPropertyChanged(nameof(MoveSpeed), srcV, value);
            }
        }

        /// <summary>
        /// Jog速度
        /// </summary>
        private int _jogSpeed;
        public int JogSpeed
        {
            get => _jogSpeed; set
            {
                int srcV = _jogSpeed;
                _jogSpeed = value;
                if (srcV != value)
                    OnPropertyChanged(nameof(JogSpeed), srcV, value);
            }
        }

        /// <summary>
        /// 脉冲毫米比
        /// </summary>
        public int PerPluse { get; set; }

        /// <summary>
        /// 正向最大行程
        /// </summary>
        public float SoftLimitPL { get; set; }

        /// <summary>
        /// 负向最大行程
        /// </summary>
        public float SoftLimitML { get; set; }

        /// <summary>
        /// 到位稳定时间
        /// </summary>
        public int Stime { get; set; }

        /// <summary>
        /// 到位判断脉冲数
        /// </summary>
        public int AxisBand { get; set; }

        /// <summary>
        /// 运动优先级
        /// </summary>
        public Priority MovePriority { get; set; }

        /// <summary>
        /// 速度百分比
        /// </summary>
        private double _speedPercent = 0.1;
        public double SpeedPercent
        {
            get => _speedPercent; set
            {
                double srcV = _speedPercent;
                _speedPercent = value;
                if (srcV != value)
                {
                    OnPropertyChanged(nameof(SpeedPercent), srcV, value);
                }
            }
        }

        /// <summary>
        /// 生产模式速度百分比
        /// </summary>
        private double _productionModelSpeedPercent = 0.1;
        [Ignore]
        public double ProductionModelSpeedPercent
        {
            get => _productionModelSpeedPercent;
            set
            {
                double srcV = _productionModelSpeedPercent;
                _productionModelSpeedPercent = value;
                if (Math.Abs(srcV - value) > 0.001)
                {
                    OnPropertyChanged(nameof(ProductionModelSpeedPercent), srcV, value);
                }
            }
        }

        /// <summary>
        /// 空跑模式速度百分比
        /// </summary>
        private double _emptyRunSpeedPercent = 0.1;
        [Ignore]
        public double EmptyRunSpeedPercent
        {
            get => _emptyRunSpeedPercent;
            set
            {
                double srcV = _emptyRunSpeedPercent;
                _emptyRunSpeedPercent = value;
                if (Math.Abs(srcV - value) > 0.001)
                {
                    OnPropertyChanged(nameof(EmptyRunSpeedPercent), srcV, value);
                }
            }
        }

        /// <summary>
        /// 调试模式速度百分比
        /// </summary>
        private double _debugSpeedPercent = 0.1;
        [Ignore]
        public double DebugSpeedPercent
        {
            get => _debugSpeedPercent;
            set
            {
                double srcV = _debugSpeedPercent;
                _debugSpeedPercent = value;
                if (Math.Abs(srcV - value) > 0.001)
                {
                    OnPropertyChanged(nameof(DebugSpeedPercent), srcV, value);
                }
            }
        }
        /// <summary>
        /// 其他模式速度百分比（用于除生产、空跑、调试之外的模式）
        /// </summary>
        private double _otherModeSpeedPercent = 0.1;
        [Ignore]
        public double OtherModeSpeedPercent
        {
            get => _otherModeSpeedPercent;
            set
            {
                double srcV = _otherModeSpeedPercent;
                _otherModeSpeedPercent = value;
                if (Math.Abs(srcV - value) > 0.001)
                {
                    OnPropertyChanged(nameof(OtherModeSpeedPercent), srcV, value);
                }
            }
        }

        /// <summary>
        /// 是否使用模式特定的速度设置
        /// </summary>
        private bool _useModeSpecificSpeed = false;
        [Ignore]
        public bool UseModeSpecificSpeed
        {
            get => _useModeSpecificSpeed;
            set
            {
                if (_useModeSpecificSpeed != value)
                {
                    _useModeSpecificSpeed = value;
                    OnPropertyChanged(nameof(UseModeSpecificSpeed), !value, value);
                }
            }
        }

        /// <summary>
        /// 设备类型不同
        /// </summary>
        private AxisType _axisType;
        public AxisType AxisType
        {
            get { return _axisType; }
            set
            {
                var srcV = _axisType;
                _axisType = value;
                if (srcV != value)
                {
                    OnPropertyChanged(nameof(AxisType), srcV, value);
                }
            }
        }

        /// <summary>
        /// 轴型号
        /// </summary>
        private AxisPML _axisPML;
        public AxisPML AxisPML
        {
            get { return _axisPML; }
            set
            {
                var srcV = _axisPML;
                _axisPML = value;
                if (srcV != value)
                {
                    OnPropertyChanged(nameof(AxisPML), srcV, value);
                }
            }
        }

        private AxisForward _axisForward;
        public AxisForward AxisForward
        {
            get { return _axisForward; }
            set
            {
                var srcV = _axisForward;
                _axisForward = value;
                if (srcV != value)
                {
                    OnPropertyChanged(nameof(AxisForward), srcV, value);
                }
            }
        }
        #endregion

        #region 回零参数
        /// <summary>
        /// 回零高速
        /// </summary>
        public uint HomeSpeedHigh { get; set; }

        /// <summary>
        /// 回零低速
        /// </summary>
        public uint HomeSpeedLow { get; set; }

        /// <summary>
        /// 回零加速度
        /// </summary>
        public uint HomeAcc { get; set; }

        /// <summary>
        /// 回零减速度
        /// </summary>
        public uint HomeDec { get; set; }

        /// <summary>
        /// 回零偏移
        /// </summary>
        public float HomeOffset { get; set; }

        /// <summary>
        /// 回零方式
        /// </summary>
        private HomeMode _homeMode;
        public HomeMode HomeMode
        {
            get => _homeMode; set
            {
                HomeMode srcV = _homeMode;
                _homeMode = value;
                OnPropertyChanged(nameof(HomeMode), srcV, value);
            }
        }

        /// <summary>
        /// 回零优先级
        /// </summary>
        private Priority _homePriority;
        public Priority HomePriority
        {
            get => _homePriority; set
            {
                Priority srcV = _homePriority;
                _homePriority = value;
                OnPropertyChanged(nameof(HomePriority), srcV, value);
            }
        }

        /// <summary>
        /// 是否回零
        /// </summary>
        [Ignore]
        public bool IsHome { get; set; } = false;
        #endregion


        #region 脉冲比计算
        /// <summary>
        /// 是否使用齿轮箱
        /// </summary>
        private bool _haveTransmission = false;
        public bool HaveTransmission { get; set; }


        /// <summary>
        /// 指令脉冲
        /// </summary>
        private int _commandPulse = 10;
        public int CommandPulse { get; set; }

        /// <summary>
        /// 一圈移动量
        /// </summary>
        private int _circleDisplacement = 10;
        public int CircleDisplacement { get; set; }


        /// <summary>
        /// 齿轮比分子
        /// </summary>
        private int _gearRatioNumerator = 10;
        public int GearRatioNumerator { get; set; }

        /// <summary>
        /// 齿轮比分母
        /// </summary>
        private int _gearRatioDenominator = 1;
        public int GearRatioDenominator { get; set; }
        #endregion

        /// <summary>
        /// 总脉冲
        /// </summary>
        private double _targetPulse;
        public double TargetPulse { get; set; }


        #region 全局变量字段
        /// <summary>
        /// 控制卡
        /// </summary>
        private IMotionCard motionCard;

        #endregion

        #region 运动限制条件

        /// <summary>
        /// 运动最小范围
        /// </summary>

        public double MotionLimitMin { get; set; }

        /// <summary>
        /// 运动最大范围
        /// </summary>
        public double MotionLimitMax { get; set; }

        /// <summary>
        /// 运动判断IO
        /// </summary>
        public VIO MotionLimitIO { get; set; }

        public bool IsIn { get; set; }

        public int SelectIOValue { get; set; }

        [Ignore]
        public List<VAxisMoveDisModel> MoveDisLimit { get; set; } = new List<VAxisMoveDisModel>();

        [Ignore]
        public List<VIOMoveCheckModel> MoveIOLimit { get; set; } = new List<VIOMoveCheckModel>();

        #endregion

        /// <summary>
        /// 设备关联
        /// </summary>
        /// <param name="hardDevice"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            base.SetDevice(hardDevice, out errMsg);

            motionCard = hardDevice as IMotionCard;
           // motionCard = new FXVirtualMotionCardAspect(motionCard, Engine);
            if (motionCard == null)
            {
                errMsg = "设备为空!";
                return false;
            }

            ProcessAction(() =>
            {
                motionCard.ResetState(AxisNo);
            });

            return true;
        }

        /// <summary>
        /// 当前位置
        /// </summary>
        protected double virtualPosition = 0;

        /// <summary>
        /// 当前位置
        /// </summary>
        /// <returns></returns>
        public override double GetCurrentPos()
        {
            ProcessAction(() =>
            {
                if (AxisType != AxisType.None)
                {
                    virtualPosition = motionCard.GetCurrentPos(AxisNo, PerPluse);
                }
            });

            return virtualPosition;
        }

        /// <summary>
        /// 位置更新
        /// </summary>
        /// <param name="position"></param>
        public void SetCurrentPos(double position)
        {
            ProcessAction(() =>
            {
                if (AxisType != AxisType.None)
                {
                    motionCard.SetCurrentPos(AxisNo, PerPluse, position);
                }
            });
        }
        /// <summary>
        /// 当前运行模式（从外部传入）
        /// </summary>
        private string _currentMode;
        [Ignore]
        public string CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode != value)
                {
                    _currentMode = value;
                }
            }
        }

        /// <summary>
        /// 根据当前模式获取速度百分比
        /// </summary>
        public double GetCurrentModeSpeedPercent()
        {
            if (string.IsNullOrEmpty(CurrentMode))
                return SpeedPercent;
            if (!UseModeSpecificSpeed)
                return SpeedPercent;

            return CurrentMode switch
            {
                "生产模式" => ProductionModelSpeedPercent,
                "空跑模式" => EmptyRunSpeedPercent,
                "调试模式" => DebugSpeedPercent,
                "调机模式" => DebugSpeedPercent,
                _ => OtherModeSpeedPercent
            };
        }

        /// <summary>
        /// 设置当前模式的百分比值
        /// </summary>
        public void SetCurrentModeSpeedPercent(string currentMode, double value)
        {
            if (!UseModeSpecificSpeed)
            {
                SpeedPercent = value;
                return;
            }

            switch (currentMode)
            {
                case "生产模式":
                    ProductionModelSpeedPercent = value;
                    break;
                case "空跑模式":
                    EmptyRunSpeedPercent = value;
                    break;
                case "调试模式":
                case "调机模式":
                    DebugSpeedPercent = value;
                    break;
                default:
                    OtherModeSpeedPercent = value;
                    break;
            }
        }

        /// <summary>
        /// 获取当前模式下的速度
        /// </summary>
        public double GetSpeedByCurrentMode(double speed, bool IsFixedSpd = false)
        {
            if (IsFixedSpd)
                return speed;

            double currentSpeedPercent = GetCurrentModeSpeedPercent();

            // 防止输入错误
            if (currentSpeedPercent > 1)
            {
                currentSpeedPercent = 1;
            }

            return speed * currentSpeedPercent;
        }

        /// <summary>
        /// 获取速度
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public double GetSpeed(double speed, bool IsFixedSpd = false, bool useCurrentMode = true)
        {
            if (IsFixedSpd)
                return speed;

            double speedPercent = 0;
            //if (CurrentMode != "生产模式" && CurrentMode != "空跑模式" && CurrentMode != "调试模式" && CurrentMode != "调机模式")
            //{
            //    speedPercent = SpeedPercent;
            //}
            //else
            //{
            //    if (useCurrentMode && !string.IsNullOrEmpty(CurrentMode))
            //    {
            //        speedPercent = GetCurrentModeSpeedPercent();
            //    }
            //}
            if (useCurrentMode && !string.IsNullOrEmpty(CurrentMode))
            {
                speedPercent = GetCurrentModeSpeedPercent();
            }
            else
            {
                speedPercent = SpeedPercent;
            }

            if (speedPercent > 1)
            {
                speedPercent = 1;
            }

            return speed * speedPercent;
        }

        /// <summary>
        /// 回零
        /// </summary>
        public override void Home()
        {
            ProcessAction(() =>
            {
                // 安全检查
                LogTool.Debug("2:开始安全检查");
                CheckIsSafe(() =>
                {
                    VStatus = VStatus.Home;
                    LogTool.Debug("3:开始复位轴状态");
                    motionCard.ResetState(AxisNo);
                    LogTool.Debug("4:开始执行回零函数");
                    motionCard.Home(AxisNo, HomeMode, HomeSpeedHigh, HomeSpeedLow, PerPluse, HomeAcc, HomeOffset, AxisPML);
                    IsHome = false;
                });
            }, () => virtualPosition = 0);
        }


        /// <summary>
        /// 检查回零超时时间,单位秒
        /// </summary>
        public override void CheckHomeDone(int timeout = 60)
        {
            ProcessAction(() =>
            {
                OnLog(LogType.Debug, $"Axis:{AxisNo} Start CheckHomeDone ");

                CalcTime(() =>
                {
                    return motionCard.CheckHomeDone(AxisNo);
                }, timeout * 1000, 5, CheckException);

                // 3.将状态更新为初始状态
                if (VStatus == VStatus.Home)
                {
                    lastAxisConfig = null;
                    VStatus = VStatus.Idle;

                    OnLog(LogType.Debug, $"Axis:{AxisNo} End CheckHomeDone");

                    // 防止回零到了极限位置，下次再运动出现轴卡报错的BUG
                    motionCard.ResetState(AxisNo);

                    // 记录回零状态
                    IsHome = true;
                }

            }, () => Thread.Sleep(500));
        }

        /// <summary>
        /// 是否处于回零
        /// </summary>
        /// <returns></returns>
        public void CheckIsHome()
        {
            ProcessAction(() =>
            {
                if (!IsHome)
                {
                    throw new DeviceException($"Axis:{Name} is not Home!");
                }
            });
        }

        /// <summary>
        /// 回零取消
        /// </summary>
        public override void HomeCancel()
        {
            ProcessAction(() =>
            {
                motionCard.HomeCancel(AxisNo);
            });
        }

        /// <summary>
        /// 绝对运动
        /// </summary>
        /// <param name="cmdPos">运动位置和速度</param>
        /// <param name="moveSpeed">运动速度</param>
        public void MoveAbs(double cmdPos, double moveSpeed = -1, double acc = -1, double dec = -1, bool IsFixedSpd = false)
        {
            if (moveSpeed < 0)
            {
                moveSpeed = MoveSpeed;
            }

            MoveAbs(new VAxisDevice()
            {
                Compensate = 0,
                DeviceID = ID,
                Axis = this,
                Position = cmdPos,
                Speed = moveSpeed,
                Acc = acc == -1 ? Acc : acc,
                Dec = dec == -1 ? Dec : dec,
            });
        }

        /// <summary>
        /// 相对运动
        /// </summary>
        /// <param name="cmdPos"></param>
        /// <param name="moveSpeed"></param>
        public void MoveRel(double relPos, double moveSpeed = -1)
        {
            if (moveSpeed < 0)
            {
                moveSpeed = MoveSpeed;
            }

            // 因为被多轴调用 用于CheckMotionDone完成后，进行结束
            var axisParam = new VAxisDevice()
            {
                Compensate = relPos,
                DeviceID = ID,
                Axis = this,
                Position = 0,
                Speed = moveSpeed
            };

            MoveRel(axisParam);
        }


        /// <summary>
        /// 绝对运动
        /// </summary>
        /// <param name="cmdPos">运动位置和速度</param>
        /// <param name="moveSpeed">运动速度</param>
        public void MoveAbs(VAxisDevice vDevice, bool IsFixedSpd = false)
        {
            ProcessAction(() =>
            {
                RealMoveAbs(vDevice, IsFixedSpd);

            }, () =>
            {
                // 安全位置检查
                CheckIsSafe(() =>
                {
                    virtualPosition = (vDevice.Position + vDevice.Compensate);
                });

            }, true);
        }

        private void RealMoveAbs(VAxisDevice vDevice, bool IsFixedSpd)
        {
            if (vDevice == null)
            {
                throw new FriendlyException($"{nameof(vDevice)}对象为空");
            }
            if (motionCard == null)
            {
                throw new FriendlyException($"{nameof(motionCard)}对象为空");
            }

            // 回零检查
            CheckIsHome();

            vDevice.MoveMode = MoveMode.Abs;
            VStatus = VStatus.Running;
            axisMode = AxisMode.Move;
            lastAxisConfig = vDevice;

            // 安全位置检查
            CheckIsSafe(() =>
            {
                // 记录运行前的位置
                vDevice.PrevMovePos = GetCurrentPos();

                double pos = vDevice.Position + vDevice.Compensate;

                double speed = GetSpeed(vDevice.Speed, IsFixedSpd);

                // 如果是空跑模式，那么Z轴只走0.8
                //if (AxisType == AxisType.Z && Mode == DeviceMode.Empty)
                //{
                //    pos = pos * 0.5;
                //}

                motionCard.Move(pos, speed, Acc, Dec, PerPluse, Stime, true, AxisNo, AxisPML);
            });
        }

        /// <summary>
        /// JOG运动
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="jogSpeed"></param>
        public void Jog(bool isAdd, double jogSpeed, bool isFixVal = false)
        {
            VAxisDevice jogParam = new VAxisDevice()
            {
                Direction = isAdd ? MoveDirection.Positive : MoveDirection.Negative,
                Speed = jogSpeed,
            };

            Jog(jogParam, isFixVal);
        }

        /// <summary>
        /// JOG运动
        /// </summary>
        /// <param name="isAdd"></param>
        /// <param name="jogSpeed"></param>
        public void Jog(VAxisDevice vDevice, bool IsFixVal = false)
        {
            ProcessAction(() =>
            {
                // 安全位置检查
                CheckIsSafe(() =>
                {
                    VStatus = VStatus.Running;
                    axisMode = AxisMode.JOG;

                    bool isAdd = vDevice.Direction == MoveDirection.Positive;
                    double speed;
                    if (IsFixVal)
                        speed = isAdd ? vDevice.Speed : -vDevice.Speed;
                    else
                        speed = GetSpeed(isAdd ? vDevice.Speed : -vDevice.Speed);
                    motionCard.Jog(speed, Acc, Dec, PerPluse, Stime, AxisNo, AxisPML);

                    if (alarmInfo != null)
                    {
                        Thread.Sleep(300);
                        Stop();
                    }
                });

            }, () => Thread.Sleep(10), true);
        }

        /// <summary>
        /// 相对运动
        /// </summary>
        /// <param name="cmdPos"></param>
        /// <param name="moveSpeed"></param>
        public void MoveRel(IAxisParam vDevice)
        {
            ProcessAction(() =>
            {
                CheckIsSafe(() =>
                {
                    vDevice.MoveMode = MoveMode.Rel;
                    VStatus = VStatus.Running;
                    axisMode = AxisMode.Move;

                    double dstPos = 0;

                    // 如果为空
                    if (lastAxisConfig == null)
                    {
                        // 记录历史
                        lastAxisConfig = vDevice.Clone() as IAxisParam;
                    }

                    dstPos = GetCurrentPos() + vDevice.Compensate;
                    lastAxisConfig.Position = dstPos;
                    lastAxisConfig.Compensate = vDevice.Compensate;
                    double speed = GetSpeed(vDevice.Speed);

                    // 转换为绝对运动
                    motionCard.Move(dstPos, speed, Acc, Dec, PerPluse, Stime, true, AxisNo, AxisPML);
                });
            }, () => Thread.Sleep(200), true);
        }

        /// <summary>
        /// 清错
        /// </summary>
        public void ResetStatus()
        {
            ProcessAction(() =>
            {
                motionCard.ResetState(AxisNo);
            });
        }

        /// <summary>
        /// 停止
        /// </summary>
        public override void Stop()
        {
            // 发送停止指令
            ProcessAction(() =>
            {
                if (VStatus == VStatus.Home)
                {
                    motionCard.HomeCancel(AxisNo);
                }
                else
                {
                    motionCard.Stop(AxisNo);
                }
            });

            // 中断循环
            IsBreak = true;

            VStatus = VStatus.Idle;
            lastAxisConfig = null;
        }

        /// <summary>
        /// 检查轴运动完成
        /// </summary>
        /// <param name="percision">脉冲偏差范围</param>
        /// <param name="posAction">运动到位置回调</param>
        /// <param name="axisParam">轴参数,默认使用lastConfig,多轴的时候因为使用的是AxisItem，所以会从外部传递过来</param>
        public void CheckMotionDone(IAxisParam axisParam = null, double percision = 45, Action<double> posAction = null, double targetPulse = 0)
        {
            if (lastAxisConfig != null)
            {
                axisParam = lastAxisConfig;
            }

            if (axisParam == null)
            {
                OnLog(LogType.Error, $"Axis:{AxisNo} 在ChekcMotionDone时，轴运动参数为空!");
            }

            ProcessAction(() =>
            {
                // 1.运动的总行程
                var startPos = GetCurrentPos();
                int timeout = -1;
                double allDis = 0;
                double speed = GetSpeed(axisParam.Speed);

                if (axisParam != null)
                {
                    if (axisParam.MoveMode == MoveMode.Abs)
                    {
                        // 增加补偿值到总行程
                        allDis = Math.Abs(axisParam.Position + axisParam.Compensate - startPos);
                    }
                    else if (axisParam.MoveMode == MoveMode.Rel)
                    {
                        allDis = Math.Abs(axisParam.Compensate);
                    }

                    // 1.计算超时时间 ms
                    timeout = (int)GetMSecond(allDis, speed) * 2; // 
                }

                CurrentHP += (float)allDis;
                OnLog(LogType.Debug, $"Axis:{AxisNo} Start CheckMotionDone MoveMode:{axisParam?.MoveMode} Speed:{speed} Distance:{allDis} Start Pos:{startPos} Dest Pos:{axisParam?.Position} Compensate:{axisParam?.Compensate} Timeout:{timeout}");

                // 3.循环等待位置是否停止
                // 增加自适应到位误差检测最小值限定
                // 对于运动轴，到位最小误差为 0.008mm
                // 对于旋转轴，到位最小误差为 0.008°
                double minPercision = 0.008;
                int axisPercision = percision > (minPercision * PerPluse) ? (int)percision : (int)(minPercision * PerPluse);
                //临时补救，对于CG1-1的U轴，放大其定位脉冲
                //后期需要补上
                if (PerPluse == 5000)
                {
                    axisPercision = 200;
                }
                CalcTime(() =>
                {
                    if (posAction != null && allDis > 0)
                    {
                        var pos = GetCurrentPos();
                        posAction?.Invoke(pos);
                    }

                    return motionCard.CheckMotionDone(axisPercision, AxisNo, targetPulse);
                }, timeout, 5, CheckException);


                // 3.将状态更新为初始状态
                if (VStatus == VStatus.Running)
                {
                    lastAxisConfig = null;
                    VStatus = VStatus.Idle;

                    OnLog(LogType.Debug, $"Axis:{AxisNo} End CheckMotionDone");
                }

            }, () =>
            {
                if (axisParam != null)
                {
                    Thread.Sleep(100);
                    double allDis = Math.Abs(axisParam.Position + axisParam.Compensate - virtualPosition + 2);
                    CurrentHP += (float)allDis;
                }

            });
        }

        /// <summary>
        /// 检测异常
        /// </summary>
        /// <exception cref="DeviceException"></exception>
        /// <exception cref="DeviceTimeoutException"></exception>
        private void CheckException()
        {
            // 轴卡超时
            // 获取当前状态
            var status = GetAxisStatus();
            if (status[AxisStatus.SvOn] == false)
            {

                // 轴卡掉使能，需要重新回零
                IsHome = false;
                CurrentErrorCode = DeviceError.SerOnFail;
                throw new DeviceException(Errors[CurrentErrorCode], this.ID);
            }
            else if (status[AxisStatus.Pel])
            {
                // 轴卡碰到正限位，不需要重新回零
                // 可以重试，因为可能是误触，重试可以继续
                // 如果真的是限位，重试也不会运行，仍然在原地
                //IsHome = false;
                CurrentErrorCode = DeviceError.PelFail;
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, "超正限位", this.Module, this.Name);
            }
            else if (status[AxisStatus.Mel])
            {
                // 轴卡碰到负限位，不需要重新回零
                // 可以重试，因为可能是误触，重试可以继续
                // 如果真的是限位，重试也不会运行，仍然在原地
                //IsHome = false;
                CurrentErrorCode = DeviceError.MelFail;
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, "超负限位", this.Module, this.Name);

            }
            else
            {
                CurrentErrorCode = DeviceError.MoveTimeFail;
                throw new DeviceTimeoutException(Errors[CurrentErrorCode], this.ID, "运行超时", this.Module, this.Name);
            }
        }

        /// <summary>
        /// 计算运动的理论耗时。输入参数：总位移、加速度、减速度、匀速度
        /// </summary>
        /// <param name="allDis"></param>
        /// <returns></returns>
        private double GetMSecond(double allDis, double speed)
        {
            double t = 0;
            double v2 = Math.Pow(speed, 2);
            if (v2 / 2 / Acc + v2 / 2 / Dec <= allDis)
            {
                t = speed / Acc + speed / Dec + (allDis - v2 / 2 / Acc - v2 / 2 / Dec) / speed;
            }
            else
            {
                t = Math.Sqrt(2 * Dec * allDis / (Acc * Dec + Math.Pow(Acc, 2))) + Math.Sqrt(2 * Acc * allDis / (Acc * Dec + Math.Pow(Dec, 2)));
            }

            // 这边计算的时间单位是 秒，函数返回值是 毫秒。
            // 如果超时时间小于1s,则将超时时间设置1s，否则原点位置运动会提示超时
            if (t < 1)
            {
                return 1000;
            }

            return t * 1000;
        }

        /// <summary>
        /// 获取轴状态
        /// </summary>
        /// <returns></returns>
        public Dictionary<AxisStatus, bool> GetAxisStatus(bool IsThrowException = true)
        {
            Dictionary<AxisStatus, bool> tuples = new Dictionary<AxisStatus, bool>();

            ProcessAction(() =>
            {
                tuples = motionCard.GetAxisStatus(AxisNo, IsThrowException);
            });

            return tuples;
        }

        /// <summary>
        /// 使能
        /// </summary>
        /// <param name="isOn">启用|禁用</param>
        public void ServOn(bool isOn)
        {
            ProcessAction(() =>
            {
                motionCard.ServOn(AxisNo, isOn);
            });
        }

        #region 暂停和恢复
        private AxisMode axisMode = AxisMode.Move;
        /// <summary>
        /// 最终的位置
        /// </summary>
        private IAxisParam lastAxisConfig = null;

        public override void Pause()
        {
            // 发送停止指令
            ProcessAction(() =>
            {
                if (VStatus == VStatus.Home)
                {
                    motionCard.HomeCancel(AxisNo);
                }
                else
                {
                    motionCard.Stop(AxisNo);
                }
            });
        }

        public float MaxDistance = 0;

        public float Distance { get; set; }

        public double GetPercent()
        {
            if (MaxDistance == 0) return 0;

            return Math.Round(Distance / MaxDistance, 3) * 100;
        }

        public string GetActual()
        {
            return $"{Distance} / {MaxDistance}";
        }

        /// <summary>
        /// 保养提示
        /// </summary>
        /// <returns></returns>
        public override string GetMaintainTips()
        {
            return $"轴:{Name}行驶里程>{MaintainHP},请进行保养";
        }
        #endregion

        #region 点检接口
        /// <summary>
        /// 点检
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            bool isOk = OnCheckEvent($"即将点检轴{AxisNo}:确认是否在安全位置?", true);
            if (!isOk) return false;
            // 正向移动
            isOk = OnCheckEvent("即将朝正向移动,请确认?", true);
            if (!isOk) return false;

            MoveRel(10, 10);
            isOk = OnCheckEvent("是否实际往正向运动?", true);
            if (!isOk) return false;

            // 负向运动
            isOk = OnCheckEvent("即将朝负向移动,请确认?", true);
            if (!isOk) return false;

            MoveRel(-10, 10);
            isOk = OnCheckEvent("是否实际往负向运动?", true);
            if (!isOk) return false;
            return isOk;
        }


        /// <summary>
        /// 点检事件
        /// </summary>
        public event Func<string, bool, bool> OnCheckEvent;
        #endregion

        #region 安全位置
        /// <summary>
        /// 前提条件
        /// </summary>
        [Ignore]
        private List<SafeModel> SafePostions { get; set; } = new List<SafeModel>();

        [Ignore]
        private List<PosionSafeModel> PosionSafePostions { get; set; } = new List<PosionSafeModel>();

        [Ignore]
        public List<PosionSafe> posionSafes { get; set; } = new List<PosionSafe>();

        /// <summary>
        /// 是否处理成功
        /// </summary>
        private bool isSafe = true;

        /// <summary>
        /// 安全报警
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="alarmProc"></param>
        /// <returns></returns>
        private bool AlarmInfo_AlarmProcEvent(object arg1, AlarmProc alarmProc)
        {
            // 回调确认处理
            bool isTrue = true;

            isSafe = true;
            if (alarmProc == AlarmProc.Stop)
            {
                isSafe = false;
            }

            return isTrue;
        }

        AlarmInfo alarmInfo = null;
        /// <summary>
        /// 检查位置是否安全
        /// </summary>
        /// <returns></returns>
        protected virtual void CheckIsSafe(Action action = null)
        {
            isSafe = true;

            // 初始化默认为空
            alarmInfo = null;

            //判断设备是否自动运行
            LogTool.Debug($"2.1 引擎当前状态{Engine.GetMachineStatus()}");

            if (Engine.GetMachineStatus() == EngineStatus.Running || Engine.GetMachineStatus() == EngineStatus.Resetting)
            {
                action?.Invoke();
                return;
            }
            LogTool.Debug($"2.2 安全点当前数量{SafePostions.Count}");

            if (SafePostions != null && SafePostions.Count > 0)
            {
                SafeModel safeModel = null;
                StringBuilder sb = new StringBuilder();
                double pos = 0;
                foreach (var item in SafePostions)
                {
                    pos = item.Pos.GetCurrentPos();
                    if (pos < item.Min || pos > item.Max)
                    {
                        safeModel = item;
                        break;
                    }
                }

                // 不满足安全的前提信息 需要将所有的不满足条件都显示出来
                if (safeModel != null)
                {
                    isSafe = false;
                    string errMsg = $"设备:{Name} 运动前提:{safeModel.Pos.Name}的位置:{pos}不在{safeModel.Min}~{safeModel.Max}范围内!";
                    AlarmCode = "F99OOOO-1" + safeModel.Axis.AxisNo.ToString();
                    alarmInfo = new AlarmInfo(this, AlarmType.WarningTip, errMsg, AlarmCode, this.Module);
                    alarmInfo.AlarmProcEvent += AlarmInfo_AlarmProcEvent;
                    Engine.OnAlarm(alarmInfo);
                }
            }

            // 如果位置安全继续走
            LogTool.Debug($"2.3 是否安全{isSafe}");

            if (isSafe)
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// 获取当前轴依赖的安全区域
        /// </summary>
        /// <returns></returns>
        public override List<SafeModel> GetSafeRegions()
        {
            return SafePostions;
        }

        /// <summary>
        /// 获取当前点位依赖的安全区域
        /// </summary>
        /// <returns></returns>
        public override List<PosionSafeModel> GetPosionSafeRegions()
        {
            return PosionSafePostions;
        }

        /// <summary>
        /// 添加安全Postion
        /// </summary>
        /// <param name="position"></param>
        public override void AddPosition(SafeModel position)
        {
            if (SafePostions.Any(u => u.Pos == position.Pos))
            {
                throw new FriendlyException($"安全点位:{position.Pos.Name}已经存在!");
            }

            // 新增点位
            SafePostions.Add(position);
            Engine.IsNeedSave = true;
        }

        //Jack20231019
        public override void AddPosionPosition(PosionSafeModel position)
        {
            if (posionSafes.Any(u => u.Position.Key == position.Position.Key))
            {
                var posionSafeExisted = posionSafes.Where(u => u.Position.Key == position.Position.Key).ToList();
                //if(posionSafeExisted.Count >1)
                //{
                //    throw new FriendlyException($"安全轴的点位:{position.Position.Name}已经存在!");
                //}
                if (posionSafeExisted[0].PosionSafePostions.Any(u => u.Pos == position.Pos))
                {
                    throw new FriendlyException($"点位的安全前提:{position.Pos.Name}已经存在!");
                }
                posionSafeExisted[0].PosionSafePostions.Add(position);
            }
            else
            {
                PosionSafe posionSafe = new PosionSafe();
                posionSafe.Position = position.Position;
                posionSafe.PosionSafePostions.Add(position);
                posionSafes.Add(posionSafe);
            }
            Engine.IsNeedSave = true;
        }

        /// <summary>
        /// 通过ID获取Position
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override IPosition GetPosition(Guid id)
        {
            var vDevice = Engine.GetVirtualByID(id) as IPosition;

            return vDevice;
        }

        public override void RemovePosition(SafeModel sModel)
        {
            if (!SafePostions.Any(u => u.Pos == sModel.Pos))
            {
                throw new FriendlyException($"安全点位:{sModel.Pos.Name}不存在!");
            }

            SafePostions.RemoveAll(u => u.Pos == sModel.Pos);
            Engine.IsNeedSave = true;
        }

        public override void RemovePosionPosition(PosionSafeModel pModel)
        {
            var posionSafeExisted = posionSafes.Where(u => u.Position.Key == pModel.Position.Key).ToList();
            if (!posionSafeExisted[0].PosionSafePostions.Any(u => u.Pos == pModel.Pos))
            {
                throw new FriendlyException($"点位的安全前提:{pModel.Pos.Name}不存在!");
            }
            //if (!PosionSafePostions.Any(u => u.Pos == pModel.Pos))
            //{
            //    throw new FriendlyException($"安全点位:{pModel.Pos.Name}不存在!");
            //}

            posionSafeExisted[0].PosionSafePostions.RemoveAll(u => u.Pos == pModel.Pos);
            Engine.IsNeedSave = true;
        }

        #endregion

        #region 点位配置
        [Ignore]
        public List<AxisPosition> Positions { get; set; } = new List<AxisPosition>();


        /// <summary>
        /// 新增点位
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pos"></param>
        /// <exception cref="FriendlyException"></exception>
        public void AddPostion(string name, double pos)
        {
            if (Positions.Any(u => u.Name == name))
            {
                throw new FriendlyException($"点位名称:{name}已存在!");
            }

            Positions.Add(new AxisPosition { Name = name, Position = pos, Key = Guid.NewGuid(), Axis = this });
            Engine.IsNeedSave = true;
        }

        /// <summary>
        /// 移除点位
        /// </summary>
        /// <param name="name"></param>
        public void RemovePostion(string name)
        {
            int num = Positions.RemoveAll(u => u.Name == name);
            if (num > 0)
            {
                Engine.IsNeedSave = true;
            }
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <returns></returns>
        public LNode GetAxisPosNodes()
        {
            LNode node = new LNode()
            {
                Text = Name,
                Tag = this,
                Key = ID.ToString(),
            };

            foreach (var item in Positions)
            {
                LNode pNode = new LNode()
                {
                    Text = item.Name,
                    Tag = item,
                    Key = item.Name,
                    Parent = node
                };

                node.Children.Add(pNode);
            }

            return node;
        }
        #endregion

        #region SDO读写

        public void SDORead(short index, short subindex, short data_size, out int value, short count)
        {
            int pBuf = 0;
            ProcessAction(() =>
            {
                short axisNo = (short)AxisNo;
                motionCard.SDORead(axisNo, index, subindex, data_size, out pBuf, count);
            });
            value = pBuf;
        }

        public void SDOWrite(short index, short subindex, int data, short data_size)
        {
            ProcessAction(() =>
            {
                short axisNo = (short)AxisNo;
                motionCard.SDOWrite(axisNo, index, subindex, data, data_size);
            });
        }

        #endregion

        #region PDO读写

        public void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count)
        {
            int valueCur = 0;
            ProcessAction(() =>
            {
                motionCard.PDORead(axis, index, subindex, data_size, ref valueCur, count);
            });
            value = valueCur;
        }

        public void PDOWrite(short axis, short index, short subindex, int data, short data_size)
        {
            ProcessAction(() =>
            {
                motionCard.PDOWrite(axis, index, subindex, data, data_size);
            });
        }

        #endregion 

        #region 导入和导出
        public override XElement ExportXml()
        {
            var xml = base.ExportXml();

            if (Positions.Count > 0)
            {
                XElement xPos = new XElement(nameof(Positions));
                foreach (var item in Positions)
                {
                    xPos.Add(item.ExportXml());
                }

                xml.Add(xPos);
            }

            if (MoveDisLimit.Count > 0)
            {
                XElement xDisLimit = new XElement(nameof(MoveDisLimit));
                foreach (var item in MoveDisLimit)
                {
                    xDisLimit.Add(item.ExportXml());
                }

                xml.Add(xDisLimit);
            }

            if (MoveIOLimit.Count > 0)
            {
                XElement xIOLimit = new XElement(nameof(MoveIOLimit));
                foreach (var item in MoveIOLimit)
                {
                    xIOLimit.Add(item.ExportXml());
                }

                xml.Add(xIOLimit);
            }

            return xml;
        }

        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);

            Positions.Clear();
            MoveDisLimit.Clear();
            MoveIOLimit.Clear();

            // 解析点位
            var xPos = xElement.Element(nameof(Positions));
            if (xPos != null)
            {
                foreach (var xItem in xPos.Elements())
                {
                    AxisPosition pos = new AxisPosition();
                    pos.ParserXml(xItem);
                    pos.Axis = this;
                    Positions.Add(pos);
                }
            }

            var xDis = xElement.Element(nameof(MoveDisLimit));
            if (xDis != null)
            {
                foreach (var xItem in xDis.Elements())
                {
                    VAxisMoveDisModel dislimit = new VAxisMoveDisModel();
                    dislimit.ParserXml(xItem);
                    MoveDisLimit.Add(dislimit);
                }
            }

            var xIO = xElement.Element(nameof(MoveIOLimit));
            if (xIO != null)
            {
                foreach (var xItem in xIO.Elements())
                {
                    VIOMoveCheckModel iolimit = new VIOMoveCheckModel();
                    iolimit.ParserXml(xItem);
                    MoveIOLimit.Add(iolimit);
                }
            }
        }

        #endregion


        private void CalPulseRatio()
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
    }

    public class PosionSafe
    {
        public AxisPosition Position { get; set; }

        public List<PosionSafeModel> PosionSafePostions { get; set; } = new List<PosionSafeModel>();
    }
}