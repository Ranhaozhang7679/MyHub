#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VirtualBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Virtual
* 文 件 名:       VirtualBase.cs
* 创建时间:       2022/4/2 15:21:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      358bfbcf-f15f-4aa4-b18e-a09caa6d3ebe
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/2 15:21:39
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.Virtual
{
    /// <summary>
    /// 虚拟设备
    /// </summary>
    public abstract class VirtualDeviceBase : IVirtualDevice, IReference, IDisposable, System.ComponentModel.INotifyPropertyChanged
    {
        event System.ComponentModel.PropertyChangedEventHandler System.ComponentModel.INotifyPropertyChanged.PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }
        private event System.ComponentModel.PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// 模块名称
        /// </summary>
        private string module;
        public string Module
        {
            get => module; set
            {
                string srcV = module;
                module = value;
                if (srcV != module)
                {
                    PropertyChanged?.Invoke(this, nameof(Module), srcV, value);
                    propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Module)));
                }
            }
        }

        /// <summary>
        /// 用于页面排序
        /// </summary>
        [Ignore]
        public virtual int Sort => 99;

        /// <summary>
        /// 唯一标识符
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        private string name;
        public string Name
        {
            get => name; set
            {
                string srcV = name;
                name = value;
                if (srcV != name)
                {
                    PropertyChanged?.Invoke(this, nameof(Name), srcV, value);
                    propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon => "\xe6d7";

        /// <summary>
        /// 真实设备
        /// </summary>
        [Ignore]
        protected IDevice RealDevice { get; private set; }

        /// <summary>
        /// 被那个设备引用
        /// </summary>
        [Ignore]
        public IVirtualDevice ByUse { get; set; }

        /// <summary>
        /// 阻断线程
        /// </summary>
        [Ignore]
        public ManualResetEventSlim PauseResetEvent { get; set; }

        /// <summary>
        /// 设备是否屏蔽
        /// </summary>
        private bool _isShield;
        public bool IsShield
        {
            get { return _isShield; }
            set
            {
                bool srcV = _isShield;
                _isShield = value;
                if (srcV != value)
                {
                    PropertyChanged?.Invoke(this, nameof(IsShield), srcV, value);
                }
            }
        }

        /// <summary>
        /// 只有在空跑模式下，屏蔽功能才有效
        /// </summary>
        /// <returns></returns>
        protected bool GetIsShield()
        {
            if (Engine == null) return false;

            return Engine.DeviceMode == DeviceMode.Empty && IsShield;
        }

        /// <summary>
        /// 屏蔽更换为屏蔽时间
        /// </summary>
        protected int shieldSleep = 500;

        [Ignore]
        public virtual bool IsBreak { get; set; }

        [Ignore]
        public virtual bool IsPause { get; set; }

        #region 错误类别
        /// <summary>
        /// 当前ErrorCode
        /// </summary>
        protected DeviceError CurrentErrorCode { get; set; } = DeviceError.None;

        /// <summary>
        /// 设备的异常分类
        /// </summary>
        [Ignore]
        public virtual DeviceError[] ErrorCodes => new DeviceError[] { };

        /// <summary>
        /// 错误代码
        /// </summary>
        [Ignore]
        public Dictionary<DeviceError, string> Errors { get; set; }

        /// <summary>
        /// 错误名称（自定义报警名称）
        /// </summary>
        [Ignore]
        public Dictionary<DeviceError, string> ErrorNames { get; set; }

        /// <summary>
        /// 对应的错误代码
        /// </summary>
        /// <param name="eCode"></param>
        /// <returns></returns>
        protected string GetAlarmCode()
        {
            if (ErrorCodes.Length == 0) return "None";

            if (CurrentErrorCode == DeviceError.None) return "None";
            if (Errors.ContainsKey(CurrentErrorCode)) return Errors[CurrentErrorCode];

            return "None";
        }
        #endregion

        /// <summary>
        /// 虚拟设备状态
        /// </summary>
        private VStatus _vStatus = VStatus.Idle;
        [Ignore]
        public VStatus VStatus
        {
            get
            {
                return _vStatus;
            }
            set
            {
                var srcV = _vStatus;
                _vStatus = value;
                if (srcV != value)
                {
                    PropertyChanged?.Invoke(this, nameof(VStatus), srcV, value);
                }
            }
        }



        /// <summary>
        /// 设备异常后，添加报警代码
        /// </summary>
        private string _alarmCode = "01";
        public string AlarmCode
        {
            get => _alarmCode; set
            {
                string srcV = _alarmCode;
                _alarmCode = value;
                if (srcV != name)
                {
                    PropertyChanged?.Invoke(this, nameof(AlarmCode), srcV, value);
                }
            }
        }

        /// <summary>
        /// 错误提示
        /// </summary>
        public string ErrorMessage 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// 报警种类
        /// </summary>
        public string AlarmCategory { get; set; }

        /// <summary>
        /// 维修动作
        /// </summary>
        public string RepairAction { get; set; }

        /// <summary>
        /// 无参构造函数的目的是能够通过反射构造对象
        /// </summary>
        public VirtualDeviceBase()
        {
            IsBreak = false;
            VStatus = VStatus.Idle;
            Errors = new Dictionary<DeviceError, string>();
            ErrorNames = new Dictionary<DeviceError, string>();
            foreach (var item in ErrorCodes)
            {
                Errors.Add(item, "10000");
            }
        }

        /// <summary>
        /// 当前设备模式
        /// </summary>
        [Ignore]
        public DeviceMode Mode { get; set; } = DeviceMode.Virtual;

        /// <summary>
        /// 设备日志
        /// </summary>
        public event Action<LogType, string> LogEvent;

        /// <summary>
        /// 属性是否变更
        /// </summary>
        public event Action<IVirtualDevice, string, object, object> PropertyChanged;

        /// <summary>
        /// 设备引擎
        /// </summary>
        [Ignore]
        public IDeviceEngine Engine { get; set; }

        /// <summary>
        /// 隶属设备ID
        /// </summary>
        public Guid DeviceID { get; set; }

        public virtual string[] GetRefProps()
        {
            return new string[] { };
        }

        /// <summary>
        /// 获取所有引用对象
        /// </summary>
        /// <returns></returns>
        public virtual List<IVirtualDevice> GetRefObjs()
        {
            var list = new List<IVirtualDevice>();

            foreach (var obj in GetRefProps())
            {
                var prop = GetType().GetProperty(obj);
                if (prop != null)
                {
                    var refDevice = prop.GetValue(this, null);
                    if (refDevice != null && refDevice is IVirtualDevice vd)
                    {
                        list.Add(vd);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 设置设备信息
        /// </summary>
        /// <param name="hardDevice"></param>
        public virtual bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            errMsg = string.Empty;

            // 设置真实设备
            RealDevice = hardDevice;

            return true;
        }

        /// <summary>
        /// 获取真实设备
        /// </summary>
        /// <returns></returns>
        public virtual IDevice GetDevice()
        {
            return RealDevice;
        }


        #region 回零
        public virtual void Home()
        {
            VStatus = VStatus.Home;
        }

        /// <summary>
        /// 需要回零
        /// </summary>
        private bool needHome;
        public bool NeedHome
        {
            get => needHome; set
            {
                bool srcV = needHome;
                needHome = value;
                if (srcV != value)
                {
                    PropertyChanged?.Invoke(this, nameof(NeedHome), srcV, value);
                }
            }
        }

        /// <summary>
        /// 回零顺序
        /// </summary>
        private int _homeSort = 999;
        public int HomeSort
        {
            get => _homeSort; set
            {
                int srcV = _homeSort;
                _homeSort = value;
                PropertyChanged?.Invoke(this, nameof(HomeSort), srcV, value);
            }
        }

        /// <summary>
        /// 点检顺序
        /// </summary>
        private int _checkSort = 999;
        public int CheckSort
        {
            get => _checkSort; set
            {
                int srcV = _checkSort;
                _checkSort = value;
                PropertyChanged?.Invoke(this, nameof(CheckSort), srcV, value);
            }
        }

        /// <summary>
        /// 取消回零
        /// </summary>
        public virtual void HomeCancel()
        {

        }

        /// <summary>
        /// 检查回零超时时间,单位秒
        /// </summary>
        /// <param name="timeout"></param>
        public virtual void CheckHomeDone(int timeout = 60)
        {
        }
        #endregion

        #region 导入导出
        public virtual XElement ExportXml()
        {
            var xRoot = this.ToXml("VDevice");
            xRoot.SetAttributeValue("Type", GetType().Name);
            if (ByUse != null)
            {
                xRoot.SetAttributeValue("ByUse", ByUse.ID);
            }

            // 获取引用属性
            foreach (var item in GetRefProps())
            {
                var refVal = GetType().GetProperty(item).GetValue(this, null);
                if (refVal != null)
                {
                    var refObj = refVal as IVirtualDevice;
                    xRoot.Add(new XElement(item, refObj.ID));
                }
            }

            // 保养
            if (this is IMaintain m)
            {
                XElement xMain = new XElement("Maintain");
                xMain.SetAttributeValue("MaxHP", MaxHP);
                xMain.SetAttributeValue("UsedHP", UsedHP);
                xMain.SetAttributeValue("MaintainHP", MaintainHP);
                xMain.SetAttributeValue("CurrentHP", CurrentHP);

                xRoot.Add(xMain);
            }

            // 错误代码配置
            if (Errors != null && Errors.Count > 0)
            {
                XElement xError = new XElement("Errors");
                foreach (var item in Errors)
                {
                    XElement xItem = new XElement(item.Key.ToString());
                    xItem.SetValue(item.Value);
                    // 保存自定义名称
                    if (ErrorNames != null && ErrorNames.TryGetValue(item.Key, out var errorName))
                    {
                        xItem.SetAttributeValue("Name", errorName);
                    }
                    xError.Add(xItem);
                }
                xRoot.Add(xError);

            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                // 移除已存在的 ErrorMessage 元素，避免重复
                var exist = xRoot.Element("ErrorMessage");
                if (exist != null)
                    exist.Remove();

                xRoot.Add(new XElement("ErrorMessage", ErrorMessage));
            }

            if (!string.IsNullOrEmpty(AlarmCategory))
            {
                var exist = xRoot.Element("AlarmCategory");
                if (exist != null)
                    exist.Remove();
                xRoot.Add(new XElement("AlarmCategory", AlarmCategory));
            }

            if (!string.IsNullOrEmpty(RepairAction))
            {
                var exist = xRoot.Element("RepairAction");
                if (exist != null)
                    exist.Remove();
                xRoot.Add(new XElement("RepairAction", RepairAction));
            }
            return xRoot;
        }

        /// <summary>
        /// Xml解析
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);

            // 解析被用对象
            xElement.GetAttribute("ByUse", (s) =>
            {
                if (Engine != null)
                    ByUse = Engine.GetDeviceByID(Guid.Parse(s)) as VIO;
            });

            // 获取引用属性
            foreach (var item in GetRefProps())
            {
                xElement.GetElement(item, (id) =>
                {
                    var vDevice = Engine.GetVirtualByID(Guid.Parse(id)) as IVirtualDevice;
                    if (vDevice != null)
                    {
                        var refProp = GetType().GetProperty(item);
                        refProp.SetValue(this, vDevice);
                    }
                });
            }

            if (this is IMaintain m)
            {
                var mElement = xElement.Element("Maintain");
                if (mElement != null)
                {
                    mElement.GetAttribute(nameof(MaxHP), (hp) =>
                    {
                        if (float.TryParse(hp, out var maxHp))
                        {
                            MaxHP = maxHp;
                        }
                    });

                    mElement.GetAttribute(nameof(UsedHP), (hp) =>
                    {
                        if (float.TryParse(hp, out var usedHp))
                        {
                            UsedHP = usedHp;
                        }
                    });

                    mElement.GetAttribute(nameof(MaintainHP), (hp) =>
                    {
                        if (float.TryParse(hp, out var mainHp))
                        {
                            MaintainHP = mainHp;
                        }
                    });

                    mElement.GetAttribute(nameof(CurrentHP), (hp) =>
                    {
                        if (float.TryParse(hp, out var curHp))
                        {
                            CurrentHP = curHp;
                        }
                    });
                }
            }

            // 记录错误信息
            if (this is IDeviceError error)
            {
                var mElement = xElement.Element("Errors");
                if (mElement != null)
                {
                    foreach (var item in mElement.Elements())
                    {
                        if (Enum.TryParse<DeviceError>(item.Name.ToString(), out var eCode))
                        {
                            if (Errors.ContainsKey(eCode))
                            {
                                Errors[eCode] = item.Value;
                            }
                            // 读取自定义名称
                            var nameAttr = item.Attribute("Name");
                            if (nameAttr != null && ErrorNames != null)
                            {
                                ErrorNames[eCode] = nameAttr.Value;
                            }
                        }
                    }
                }
            }
            if (this is IDeviceError ErrorMes)
            {
                var mElement = xElement.Element("ErrorMessage");
                if (mElement != null)
                {
                    ErrorMessage = mElement.Value;
                }

                var catElement = xElement.Element("AlarmCategory");
                if (catElement != null)
                {
                    AlarmCategory = catElement.Value;
                }

                var repairElement = xElement.Element("RepairAction");
                if (repairElement != null)
                {
                    RepairAction = repairElement.Value;
                }
            }

        }
        #endregion

        /// <summary>
        /// 真实场景
        /// </summary>
        /// <param name="realAction">真实操作</param>
        /// <param name="virtualAction">虚拟操作</param>
        protected void ProcessAction(Action realAction, Action virtualAction = null, bool isStop = false)
        {
            if (Engine.DeviceMode == DeviceMode.Real || Engine.DeviceMode == DeviceMode.Empty || isStop)
            {
                // 此时无需检测设备，因为气缸和真空都只依赖设备
                //if (RealDevice == null)
                //{
                //    LogEvent?.Invoke(LogType.Error, $"警告 方法:{methodBase.Name} 设备和虚拟设备未绑定！需要SetDevice");
                //}
                try
                {
                    realAction?.Invoke();
                }
                catch (DeviceTimeoutException tx)
                {
                    throw new DeviceTimeoutException(tx.AlarmCode, this.ID, this.name + tx.Message);
                }
                catch (DeviceException ex)
                {
                    //throw new DeviceTimeoutException(DeviceError.ConnectTimeFail, this.ID, tx.Message);
                    throw new DeviceException(ex.AlarmCode, this.name + ex.Message);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                //throw new DeviceTimeoutException(DeviceError.ConnectTimeFail, this.ID, "1323");
                // 获取调用该方法的父方法名称
                //StackTrace stackTrace = new StackTrace();
                //StackFrame stackFrame = stackTrace.GetFrame(1);
                //MethodBase methodBase = stackFrame.GetMethod();
                //OnLog(LogType.Warning, $"警告 方法:{methodBase.Name} 的运行模式是仿真模式!");

                virtualAction?.Invoke();
            }
        }

        /// <summary>
        /// 动作超时判断
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeout">等待事件</param>
        protected bool CalcTime(Func<bool> action, int timeout = -1, int sleep = 5, Action timeoutAction = null)
        {
            double time = 0;

            // 再次进来要更新IsBreak;
            IsBreak = false;
            IsPause = false;

            // 等待运行
            while (true)
            {
                WaitRecovery();

                // 主动中断循环（Stop 触发）
                if (IsBreak)
                {
                    return false;
                }

                // 超时检查
                if (timeout > 0)
                {
                    if (time > timeout)
                    {
                        if (timeoutAction != null)
                        {
                            timeoutAction?.Invoke();
                            return false;
                        }
                        else
                        {
                            throw new DeviceTimeoutException("N03OOOO-01", $"CalcTime超时");
                        }
                    }
                }

                // 条件满足，正常退出
                if (action.Invoke())
                {
                    return true;
                }

                // 防止刷新太快IO检查错误
                Thread.Sleep(sleep);
                if (!IsPause)
                {
                    time += sleep;
                }
            }
        }

        /// <summary>
        /// 等待数字信号
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="ioVals"></param>
        /// <param name="Ios"></param>
        protected void WaitDiagital(int timeout, VIO[] Ios, bool[] ioVals, Action timeoutAction)
        {
            if (Ios.Length != ioVals.Length)
            {
                throw new FriendlyException("IO数量和IO的值数量不匹配!");
            }

            int ioLen = Ios.Length;

            CalcTime(() =>
            {
                bool isAll = true;

                for (int i = 0; i < ioLen; i++)
                {
                    bool ioVal = false;

                    // 同时支持输入
                    if (Ios[i].Behavior == IOBehavior.Input)
                    {
                        ioVal = Ios[i].GetDigitalIn();
                    }
                    else
                    {
                        ioVal = Ios[i].GetDigitalOut();
                    }

                    bool isEqual = ioVal == ioVals[i];

                    isAll = isAll && isEqual;
                }

                return isAll;
            }, timeout, 5, timeoutAction);
        }

        /// <summary>
        /// 属性发生变更
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        public void OnPropertyChanged(string pName, object srcV, object newV)
        {
            PropertyChanged?.Invoke(this, pName, srcV, newV);
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        public virtual void Dispose()
        {

        }

        /// <summary>
        /// 触发log事件
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="logMsg"></param>
        protected void OnLog(LogType logType, string logMsg)
        {
            LogEvent?.Invoke(logType, logMsg);
        }

        #region 暂停和恢复
        /// <summary>
        /// 放在动作的方法后吗
        /// </summary>
        protected void WaitRecovery(string msg = "")
        {
            if (PauseResetEvent != null)
            {
                // 如果状态变为运行状态，则更新
                if ((VStatus == VStatus.Idle || VStatus == VStatus.Running || VStatus == VStatus.Home)
                    && !PauseResetEvent.IsSet)
                {
                    PauseResetEvent.Set();
                }

                // 如果无信号，进入等待
                if (!PauseResetEvent.IsSet)
                {
                    OnLog(LogType.Debug, $"虚拟设备:{Name} 被暂停,{msg}!");
                    PauseResetEvent.Wait();
                    OnLog(LogType.Debug, $"虚拟设备:{Name} 被恢复,{msg}!");
                }
            }
        }

        /// 暂停
        /// </summary>
        public virtual void Pause()
        {
            if (PauseResetEvent == null)
            {
                PauseResetEvent = new ManualResetEventSlim(true);
            }

            // 无信号状态
            PauseResetEvent.Reset();
            VStatus = VStatus.Pause;
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public virtual void Recovery()
        {
            IsBreak = false;
        }

        /// <summary>
        /// 设备停止，目前主要时将等待终止
        /// </summary>
        public virtual void Stop()
        {
            IsBreak = true;
        }
        #endregion

        #region 安全检查
        /// <summary>
        /// 是否需要做安全检查
        /// </summary>
        [Ignore]
        public virtual bool IsSafeCheck { get; set; }

        [Ignore]
        public double Min { get; set; }

        [Ignore]
        public double Max { get; set; }

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <returns></returns>
        public virtual double GetCurrentPos()
        {
            return -999;
        }

        /// <summary>
        /// 获取当前点位依赖的安全区域
        /// </summary>
        /// <returns></returns>
        public virtual List<SafeModel> GetSafeRegions()
        {
            return new List<SafeModel>();
        }

        public virtual List<PosionSafeModel> GetPosionSafeRegions()
        {
            return new List<PosionSafeModel>();
        }

        /// <summary>
        /// 添加Postion
        /// </summary>
        /// <param name="position"></param>
        public virtual void AddPosition(SafeModel position)
        {

        }

        /// <summary>
        /// 添加Posion
        /// </summary>
        /// <param name="position"></param>
        public virtual void AddPosionPosition(PosionSafeModel position)
        {

        }

        /// <summary>
        /// 通过ID获取Position
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IPosition GetPosition(Guid id)
        {
            return null;
        }

        /// <summary>
        /// 移除安全区域
        /// </summary>
        /// <param name="id"></param>
        public virtual void RemovePosition(SafeModel sModel)
        {

        }

        /// <summary>
        /// 移除点位安全区域
        /// </summary>
        /// <param name="id"></param>
        public virtual void RemovePosionPosition(PosionSafeModel sModel)
        {

        }

        #endregion


        #region 轴的使用寿命相关参数
        /// <summary>
        /// 最大行程
        /// </summary>
        [Ignore]
        public double MaxHP { get; set; }

        /// <summary>
        /// 已经使用的次数
        /// </summary>
        [Ignore]
        public double UsedHP { get; set; }

        /// <summary>
        /// 当前行程
        /// </summary>
        [Ignore]
        public double MaintainHP { get; set; } = 0;

        /// <summary>
        /// 当前行程
        /// </summary>
        [Ignore]
        public double CurrentHP { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [Ignore]
        public virtual string Unit => "mm";

        public virtual double GetPercent()
        {
            if (MaxHP == 0) return 0;

            return Math.Round(CurrentHP / MaintainHP * 100, 2);
        }

        public virtual string GetActual()
        {
            return $"{CurrentHP} / {MaintainHP} {Unit}";
        }

        public virtual string GetMaintainTips()
        {
            return $"设备:{Name} 寿命>{MaintainHP},请进行保养!";
        }

        protected virtual void OnHPAlarmTips()
        {
            if (MaintainHP > 0 && CurrentHP > MaintainHP)
            {
                var alarm = new AlarmInfo(this, AlarmType.InfoTip, GetMaintainTips(), DeviceError.HPMatain.ToString(), this.module)
                {
                    DeviceID = this.ID.ToString()
                };

                Engine.OnAlarm(alarm);
            }
        }
        #endregion
    }
}