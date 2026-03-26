using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace Luster.TaskFlow.Motion
{
    public abstract class MotionModule : AbsModule, IMotionModule
    {
        /// <summary>
        /// 容器控制对象
        /// </summary>
        public IIocManager Ioc { get; set; }

        /// <summary>
        /// 订单管理
        /// </summary>
        public IOrderManager OrderManager { get; set; }


        /// <summary>
        /// 卷料管理
        /// </summary>
        public IRollManager RollManager { get; set; }

        /// <summary>
        /// 错误管理器
        /// </summary>
        public IErrorManager ErrorManager { get; set; }

        /// <summary>
        /// 光源控制器
        /// </summary>
        public ILightManager LightManager { get; set; }

        /// <summary>
        /// 订单管理
        /// </summary>
        public IConfigManager ConfigManager { get; set; }

        /// <summary>
        /// 数据库访问接口
        /// </summary>
        public IDbHelper DbHelper { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public new List<IMotionModule> Children { get; set; }

        /// <summary>
        /// 所属父节点
        /// </summary>
        public new IMotionModule Parent { get; set; }

        /// <summary>
        /// 运行模式
        /// </summary>
        public RunMode RunMode { get; set; }

        /// <summary>
        /// 报警类别
        /// </summary>
        public AlarmInfo AlarmInfo { get; set; }

        /// <summary>
        /// 标准CT
        /// </summary>
        public int CT { get; set; } = 0;

        /// <summary>
        /// 隶属工站
        /// </summary>
        public IMotionModule Station { get; set; }

        /// <summary>
        /// 开始事件
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 暂停时间
        /// </summary>
        public float PauseTime { get; set; } = 0;

        /// <summary>
        /// 前一模块
        /// </summary>
        public IMotionModule PrevModule { get; set; }

        /// <summary>
        /// 下一步模块
        /// </summary>
        public IMotionModule NextModule { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        public IDeviceEngine DeviceEngine { get; set; }


        /// <summary>
        /// 触发事件
        /// </summary>
        public event Action<string, string> MachineStatusComplete;

        /// <summary>
        /// 终止流程
        /// </summary>
        public bool IsBreak { get; set; }

        /// <summary>
        /// 是否包含输出数据
        /// </summary>
        public bool IsOutData { get; set; }
        /// <summary>
        /// 当前运行次数
        /// </summary>
        private int _runNum = 0;
        public int RunNum
        {
            get => _runNum; set
            {
                int srcV = _runNum;
                _runNum = value;
                if (srcV != _runNum)
                {
                    OnPropertyChanged(nameof(RunNum), srcV, value);
                }
            }
        }

        /// <summary>
        /// 线程ID
        /// </summary>
        public int ThreadID { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 报警代码 默认报警代码为 -1：未配置，不需要上传报警信息
        /// </summary>
        public virtual string ErrorCode { get; set; }
        /// <summary>
        /// 报警内容
        /// </summary>
        public virtual string ErrorContent { get; set; }

        /// <summary>
        /// 是否使用Log
        /// </summary>
        /// <returns></returns>
        public bool IsUseLog()
        {
            if (mStation != null)
            {
                return mStation.UseLog;
            }

            return false;
        }

        /// <summary>
        /// 模块节点类型
        /// </summary>
        public ModuleType ModuleType
        {
            get
            {
                if (PrevModule == null) return ModuleType.Start;
                else if (NextModule == null) return ModuleType.End;
                else return ModuleType.Middle;
            }
        }

        /// <summary>
        /// 函数
        /// </summary>
        //public new IMotionFunction TaskFunction { get; set; }

        /// <summary>
        /// 用于线程中断
        /// </summary>
        public ManualResetEventSlim BrokenOff { get; set; }


        /// <summary>
        /// 报警事件
        /// </summary>
        public event Action<AlarmInfo> AlarmEvent_module;

        /// <summary>
        /// 被引用模块
        /// </summary>
        private Dictionary<Guid, string> OtherRefModules = new Dictionary<Guid, string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public MotionModule() : base()
        {
            Status = Common.Enums.RunStatus.Default;
            Children = new List<IMotionModule>();
        }

        public override void SetFunction(string funcName)
        {
            base.SetFunction(funcName);
            //var func = Activator.CreateInstance(FuncTypes[funcName]) as IMotionFunction;

            //// 设置模块的Function
            //TaskFunction = func;
            //Alias = L(func.Name) ?? func.Name;
            TaskFunction.Owner = this;

            // 默认运行时
            //Mode = DesignMode.Runtime;
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        private string moduleAlias = "";

        /// <summary>
        /// 模块隶属工站
        /// </summary>
        private IStation mStation = null;

        /// <summary>
        /// 函数运行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>运行是否成功</returns>
        public override bool DoFunction()
        {
            ThreadID = Thread.CurrentThread.ManagedThreadId;

            if (IsBreak)
            {
                OnLog(LogType.Debug, $"模块:{Alias} 被停止,需要回零才能运行!");
                return true;
            }

            if (string.IsNullOrEmpty(moduleAlias))
            {
                UpdateStation();
            }
            if (moduleAlias.Contains("出料事件"))
            {
                OnLog(LogType.Debug, $"模块:{moduleAlias}开始");
            }
            // 运行状态初始化
            // 模块运行开始时间
            StartTime = DateTime.Now;

            // 1.初始标识状态
            statusMsg = string.Empty;
            AlarmInfo = null;
            bool success = false;

            TaskFunction.Status.SetDefault();
            Status = Common.Enums.RunStatus.Running;

            // 显示当前运行的模块
            if (Station != null && Station.TaskFunction is IFreeStation f)
            {
                f.AddRunningModule(this);
            }

            // 2.开始计算时间
            StartTimer();

            // 3.清空Output结果
            ResetParameters();

            try
            {
                // 4.参数验证及赋值
                var isValid = ValidateHelper.ValidateAllIn(TaskFunction, out statusMsg);
                timeconsumingValid = (float)Math.Round(_sw.Elapsed.TotalMilliseconds, 3);
                // 5.函数运行
                if (isValid)
                {
                    // 超时报警
                    success = TaskFunction.DoExcute(out statusMsg);
                    timeconsumingDoexcute = (float)Math.Round(_sw.Elapsed.TotalMilliseconds, 3);

                    if (moduleAlias == "复检工站")
                    {
                        OnLog(LogType.Debug, $"模块:{moduleAlias}成功");
                    }
                }

                // 对输出参数 Value 结果进行赋值，供引用该参数的类型使用
                if (success)
                {
                    SetOutput();
                    timeconsumingSetOutput = (float)Math.Round(_sw.Elapsed.TotalMilliseconds, 3);
                    if (moduleAlias == "复检工站")
                    {
                        OnLog(LogType.Debug, $"模块:{moduleAlias}SetOutput()");
                    }
                }
            }
            catch (DeviceTimeoutException tx)
            {
                //bool isEmpty = string.IsNullOrEmpty(ErrorCode) || ErrorCode == "报警代码";

                string eCode = tx.AlarmCode;
                string eMessage = tx.Message;
                // 状态更报警
                Status = Common.Enums.RunStatus.Alarmed;
                AlarmInfo = new AlarmInfo(this, AlarmType.Timeout, $"{eMessage}", $"{eCode}") { DeviceID = tx.DeviceID, Module = tx.Module, Name = tx.DeviceName };
                statusMsg = tx.Message;
            }
            catch (DeviceException dx)
            {
                //bool isEmpty = string.IsNullOrEmpty(ErrorCode) || ErrorCode == "报警代码";
                string eCode = dx.AlarmCode;
                string eMessage = dx.Message;
                AlarmInfo = new AlarmInfo(this, AlarmType.DeviceError, $" {eMessage}", eCode) { DeviceID = dx.DeviceID };
                statusMsg = dx.Message;
            }
            catch (Exception ex)
            {
                statusMsg = ex.Message;

                // 给结果异常
                OnLog(LogType.Error, $"模块:{Alias},{statusMsg},{ex.StackTrace}");
            }

            // 终止计时器
            StopTimer();
            if (moduleAlias == "复检工站")
            {
                OnLog(LogType.Debug, $"模块:{moduleAlias}StopTimer()");
            }
            // 6.后处理
            if (success)
            {
                Status = Common.Enums.RunStatus.Success;
            }
            else
            {
                Status = Common.Enums.RunStatus.Error;

                // 变更状态信息
                string errMsg = $"模块:{moduleAlias} 运行失败:{statusMsg}";
                OnLog(LogType.Error, errMsg);
            }
            if (mStation != null && mStation.UseLog &&
                DeviceEngine != null && DeviceEngine.AutoSetMotionModuleCT)
            {
                this.CT = Convert.ToInt32(timeconsuming);
            }

            // 6.渲染
            if (Mode == DesignMode.Design && success)
            {
                Render();
                if (moduleAlias == "复检工站")
                {
                    OnLog(LogType.Debug, $"模块:{moduleAlias}Render()");
                }
            }
            try
            {
                // 打印Log信息
                if (success)
                {
                    // 记录隶属工站及耗时情况 ，如果CT是大于0，并且模块支持暂停
                    if (mStation != null && mStation.UseLog && (CT > 0 || (TaskFunction is IPauseFunction)))
                    {
                        string sn = Station?.DataID;

                        Dictionary<string, object> extParams = null;
                        if (TaskFunction is IGetExtResult getExResult)
                        {
                            extParams = getExResult.GetExtResult();
                        }

                        foreach (var para in Parameters)
                        {
                            if (para.Key == "IsPreviousStationUndo")
                            {
                                if (bool.Parse(para.Value.Value.ToString()))
                                {
                                    extParams = new Dictionary<string, object>();
                                    extParams.Add("IsPreviousStationUndo", true);
                                }
                            }
                        }
                        mStation.StationTimes.Add(new StationTime(mStation.Station, Alias, TimeConsuming, CT, StartTime, sn, extParams));
                    }

                    // 对数据进行上传  
                    OnDataUpload();
                    if (moduleAlias == "复检工站")
                    {
                        OnLog(LogType.Debug, $"模块:{moduleAlias}OnDataUpload()");
                    }
                    // 记录运行的次数
                    RunNum++;

                    // 只有启用LOG才进行记录
                    if (mStation != null && mStation.UseLog)
                        OnLog(LogType.Info, string.Format("模块:{0} 耗时:{1} ms 暂停耗时:{2} 输入参数耗时:{3} 执行耗时:{4} 输出参数耗时:{5}",
                                                          moduleAlias,
                                                          TimeConsuming,
                                                          PauseTime,
                                                          timeconsumingValid,
                                                          timeconsumingDoexcute,
                                                          timeconsumingSetOutput));

                    // 运行状态完成
                    TaskFunction.Status.SetEnd();
                }
            }
            catch (Exception ex)
            {
                // 清空数组，避免再次出现：索引超出数组界限问题
                // 能在别的线程或模块正在遍历List，直接 Clear()，此时集合正在使用中，导致越界。
                // 加锁，确保 mStation.StationTimes 的访问是线程安全的。
                lock (mStation.StationTimes)
                {
                    mStation.StationTimes.Clear();
                }
                OnLog(LogType.Info, string.Format("模块:{0} 报警:{1} ", moduleAlias, ex.ToString()));
            }


            // 结束时间
            EndTime = DateTime.Now;

            return Status == Common.Enums.RunStatus.Success;
        }

        /// <summary>
        /// 总耗时减去暂停时间
        /// </summary>
        public override void StopTimer()
        {
            _sw.Stop();
            timeconsuming = (float)Math.Round(_sw.Elapsed.TotalMilliseconds - PauseTime, 3);
            if (timeconsuming < 0)
            {
                timeconsuming = 0;
            }
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public void ModuleValid()
        {
            var isValid = ValidateHelper.ValidateAllIn(TaskFunction, out var msg);
            if (!isValid)
            {
                Status = RunStatus.Error;
                StatusMsg = msg;
            }
            else if (Status == RunStatus.Error)
            {
                Status = RunStatus.Default;
                StatusMsg = msg;
            }
        }

        /// <summary>
        /// 更新工站信息
        /// </summary>
        public void UpdateStation()
        {
            SetCurStation(this);
            if (string.IsNullOrEmpty(moduleAlias))
            {
                GetModuleAlias(this, ref moduleAlias);
            }
        }

        /// <summary>
        /// 递归查找
        /// </summary>
        /// <param name="motion"></param>
        /// <param name="alias"></param>
        public void GetModuleAlias(IMotionModule motion, ref string alias)
        {
            if (motion == null) return;
            if (motion.TaskFunction is IStation station)
            {
                mStation = station;
                if (string.IsNullOrEmpty(alias))
                {
                    alias = $"{motion.Alias}";
                }
                else
                {
                    alias = $"{motion.Alias}->{alias}";
                }
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(alias))
                {
                    alias = $"{motion.Alias}";
                }
                else
                {
                    alias = $"{motion.Alias}->{alias}";
                }

                GetModuleAlias(motion.Parent, ref alias);
            }
        }

        #region 报警处理
        /// <summary>
        /// 报警中断
        /// </summary>
        /// <param name="alarmMsg">报警信息</param>
        public void OnAlarm(AlarmType alarmType, string alarmMsg, string code = "", string ModuleName = "")
        {
            switch (alarmType)
            {
                case AlarmType.InfoTip:
                    AlarmInfo = new AlarmInfo(this, alarmType, alarmMsg, code);
                    break;
                case AlarmType.WarningTip:
                    AlarmInfo = new AlarmInfo(this, alarmType, alarmMsg, code);
                    break;
                case AlarmType.PopInfoTip:
                    AlarmInfo = new AlarmInfo(this, alarmType, alarmMsg, code);
                    break;
                case AlarmType.Timeout:
                    throw new DeviceTimeoutException(code, alarmMsg, ModuleName);
                default:
                    AlarmInfo = new AlarmInfo(this, alarmType, alarmMsg, code);
                    break;
                    // throw new DeviceException(alarmMsg);
            }
        }

        public void OnAlarm_Module()
        {
            Status = Common.Enums.RunStatus.Alarmed;
            if (AlarmInfo != null)
            {
                if (Station == null)
                {
                    UpdateStation();
                }

                // 子模块报警，更新父模块报警
                Station.Status = Status;
                Station.AlarmInfo = AlarmInfo;

                // 因为子模块报警，会将报警信息传递到父模块，所以报警事件必须是本模块自己触发
                if (AlarmInfo.Sender == this)
                {
                    AlarmEvent_module?.Invoke(AlarmInfo); //传递至MotionEngion
                }
            }
        }

        /// <summary>
        /// 清除报警
        /// </summary>
        public void ClearAlarm()
        {
            Status = Common.Enums.RunStatus.Default;
        }
        #endregion

        public override object Clone()
        {
            XElement xClone = this.ExportXml();

            var cloneModule = Activator.CreateInstance(GetType()) as IMotionModule;

            // 必须要配置名称
            cloneModule.Name = this.Name;
            cloneModule.Icon = this.Icon;
            cloneModule.Tips = this.Tips;
            cloneModule.CT = this.CT;
            cloneModule.TaskModules = this.TaskModules;
            cloneModule.Interactor = this.Interactor;
            cloneModule.DeviceEngine = this.DeviceEngine;
            cloneModule.UpdateEvent += (s, a) => OnUpdate(a);
            cloneModule.LogEvent += OnLog;
            cloneModule.LanguageEvent += (s) => L(s);
            cloneModule.ParserXml(xClone);

            // 需要记住当前的模式，否则无法渲染
            cloneModule.Mode = this.Mode;

            if (Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    var childClone = child.Clone() as IMotionModule;
                    cloneModule.Children.Add(childClone);
                }
            }

            return cloneModule;
        }

        //public string GetInParameters()
        //{
        //    string p = string.Empty;
        //    foreach (var item in Parameters)
        //    {
        //        object v = item.Value?.Value;
        //        if (v == null)
        //        {
        //            continue;
        //        }

        //        if (item.Value.ParamType == ParamType.IN)
        //        {
        //            p = p + $"{item.Value.Name}#{JsonTool.ToJson(item.Value.Value)};";
        //        }
        //    }

        //    p = p.Trim();

        //    return p;
        //}

        public string GetInParameters()
        {
            // 使用 StringBuilder 避免字符串拼接的内存分配
            var sb = new StringBuilder();

            foreach (var item in Parameters)
            {
                // 提前过滤，减少嵌套
                if (item.Value?.ParamType != ParamType.IN)
                    continue;

                object v = item.Value.Value;
                if (v == null)
                    continue;

                // 使用 StringBuilder.Append，避免临时字符串
                sb.Append(item.Value.Name)
                  .Append('#')
                  .Append(JsonTool.ToJson(v))
                  .Append(';');
            }

            // 移除末尾分号（比 Trim 更高效，只处理特定字符）
            if (sb.Length > 0)
                sb.Length--; // 直接修改长度，避免创建新字符串

            return sb.ToString();
        }

        public override LNode GetTreeNode(bool isOutParam = false, string icon = "", Func<Type, bool> typeFunc = null)
        {
            LNode rNode = new LNode()
            {
                Text = this.Alias,
                Icon = this.TaskFunction?.Icon,
                //FunctionName = this.TaskFunction.Name,
                //FunctionInParameters = GetInParameters(),
                //RunStatus = (int)((IModule)this).Status,
                Tag = this,
                Key = ID.ToString(),
                Level = this.Level,
            };

            if (Children.Count > 0)
            {
                if (isOutParam && TaskFunction is ILoop loop)
                {
                    var pItem = Parameters[nameof(loop.LoopNum)];
                    var pNode = new LNode()
                    {
                        Key = pItem.Name,
                        Text = pItem.CN,
                        Tag = pItem,
                        Icon = icon,
                        Parent = rNode,
                        Level = rNode.Level + 1
                    };
                    rNode.Children.Add(pNode);

                    var pItem1 = Parameters[nameof(loop.OutLoop)];
                    var pNode1 = new LNode()
                    {
                        Key = pItem1.Name,
                        Text = pItem1.CN,
                        Tag = pItem1,
                        Icon = icon,
                        Parent = rNode,
                        Level = rNode.Level + 1
                    };
                    rNode.Children.Add(pNode1);

                }
                else if (isOutParam && TaskFunction is IFreeStation free)
                {
                    foreach (var item in Parameters)
                    {
                        if (item.Value.ParamType != ParamType.OUT || item.Value.Type == typeof(LStatus)) continue;

                        var pNode = new LNode()
                        {
                            Key = item.Key,
                            Text = item.Value.CN,
                            Tag = item.Value,
                            Icon = icon,
                            Level = rNode.Level + 1,
                            Parent = rNode
                        };
                        rNode.Children.Add(pNode);
                    }
                }

                foreach (var item in Children)
                {
                    if (item.Status == RunStatus.Skip) continue;
                    rNode.Children.Add(item.GetTreeNode(isOutParam, icon, typeFunc));
                }
            }
            else
            {
                if (isOutParam)
                {
                    foreach (var item in Parameters)
                    {
                        if (item.Value.ParamType != ParamType.OUT) continue;

                        if (typeFunc != null)
                        {
                            if (typeFunc(item.Value.Type))
                            {
                                var pNode = new LNode()
                                {
                                    Key = item.Key,
                                    Text = item.Value.CN,
                                    Tag = item.Value,
                                    Icon = icon,
                                    Level = rNode.Level + 1,
                                    Parent = rNode
                                };
                                rNode.Children.Add(pNode);
                            }
                        }
                        else
                        {
                            var pNode = new LNode()
                            {
                                Key = item.Key,
                                Text = item.Value.CN,
                                Tag = item.Value,
                                Icon = icon,
                                Level = rNode.Level + 1,
                                Parent = rNode
                            };
                            rNode.Children.Add(pNode);
                        }
                    }

                    // 循环支持此时引用
                    if (TaskFunction is ILoop loop)
                    {
                        var pItem = Parameters[nameof(loop.LoopNum)];
                        var pNode = new LNode()
                        {
                            Key = pItem.Name,
                            Text = pItem.CN,
                            Tag = pItem,
                            Icon = icon
                        };
                        rNode.Children.Insert(0, pNode);

                        var pOutLoop = Parameters[nameof(loop.OutLoop)];
                        var outNode = new LNode()
                        {
                            Key = pOutLoop.Name,
                            Text = pOutLoop.CN,
                            Tag = pOutLoop,
                            Icon = icon
                        };
                        rNode.Children.Append(outNode);
                    }
                }
            }

            return rNode;
        }

        public override LNode GetTreeNodeForFlow(bool isOutParam = false, string icon = "", Func<Type, bool> typeFunc = null)
        {
            LNode rNode = new LNode()
            {
                Text = this.Alias,
                Icon = this.TaskFunction?.Icon,
                FunctionName = this.TaskFunction.Name,
                FunctionInParameters = GetInParameters(),
                RunStatus = (int)((IModule)this).Status,
                Tag = this,
                Key = ID.ToString(),
                Level = this.Level,
            };

            if (Children.Count > 0)
            {
                if (isOutParam && TaskFunction is ILoop loop)
                {
                    var pItem = Parameters[nameof(loop.LoopNum)];
                    var pNode = new LNode()
                    {
                        Key = pItem.Name,
                        Text = pItem.CN,
                        Tag = pItem,
                        Icon = icon,
                        Parent = rNode,
                        Level = rNode.Level + 1
                    };
                    rNode.Children.Add(pNode);

                    var pItem1 = Parameters[nameof(loop.OutLoop)];
                    var pNode1 = new LNode()
                    {
                        Key = pItem1.Name,
                        Text = pItem1.CN,
                        Tag = pItem1,
                        Icon = icon,
                        Parent = rNode,
                        Level = rNode.Level + 1
                    };
                    rNode.Children.Add(pNode1);

                }
                else if (isOutParam && TaskFunction is IFreeStation free)
                {
                    foreach (var item in Parameters)
                    {
                        if (item.Value.ParamType != ParamType.OUT || item.Value.Type == typeof(LStatus)) continue;

                        var pNode = new LNode()
                        {
                            Key = item.Key,
                            Text = item.Value.CN,
                            Tag = item.Value,
                            Icon = icon,
                            Level = rNode.Level + 1,
                            Parent = rNode
                        };
                        rNode.Children.Add(pNode);
                    }
                }

                foreach (var item in Children)
                {
                    if (item.Status == RunStatus.Skip) continue;
                    rNode.Children.Add(item.GetTreeNodeForFlow(isOutParam, icon, typeFunc));
                }
            }
            else
            {
                if (isOutParam)
                {
                    foreach (var item in Parameters)
                    {
                        if (item.Value.ParamType != ParamType.OUT) continue;

                        if (typeFunc != null)
                        {
                            if (typeFunc(item.Value.Type))
                            {
                                var pNode = new LNode()
                                {
                                    Key = item.Key,
                                    Text = item.Value.CN,
                                    Tag = item.Value,
                                    Icon = icon,
                                    Level = rNode.Level + 1,
                                    Parent = rNode
                                };
                                rNode.Children.Add(pNode);
                            }
                        }
                        else
                        {
                            var pNode = new LNode()
                            {
                                Key = item.Key,
                                Text = item.Value.CN,
                                Tag = item.Value,
                                Icon = icon,
                                Level = rNode.Level + 1,
                                Parent = rNode
                            };
                            rNode.Children.Add(pNode);
                        }
                    }

                    // 循环支持此时引用
                    if (TaskFunction is ILoop loop)
                    {
                        var pItem = Parameters[nameof(loop.LoopNum)];
                        var pNode = new LNode()
                        {
                            Key = pItem.Name,
                            Text = pItem.CN,
                            Tag = pItem,
                            Icon = icon
                        };
                        rNode.Children.Insert(0, pNode);

                        var pOutLoop = Parameters[nameof(loop.OutLoop)];
                        var outNode = new LNode()
                        {
                            Key = pOutLoop.Name,
                            Text = pOutLoop.CN,
                            Tag = pOutLoop,
                            Icon = icon
                        };
                        rNode.Children.Append(outNode);
                    }
                }
            }

            return rNode;
        }
        

        #region 导入和导出
        public override XElement ExportXml()
        {
            var xml = base.ExportXml();
            if (CT > 0)
            {
                xml.SetAttributeValue("CT", CT);
            }
            xml.SetAttributeValue("ErrorCode", ErrorCode);
            xml.SetAttributeValue("ErrorContent", ErrorContent);

            return xml;
        }

        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);

            xElement.GetAttribute("CT", ct => CT = int.Parse(ct));
            xElement.GetAttribute("ErrorCode", item => ErrorCode = item);
            xElement.GetAttribute("ErrorContent", item => ErrorContent = item);

            // 如果当前模块是工站
            if (TaskFunction is IStation)
            {
                Station = this;
            }
        }
        #endregion

        #region 产品相关事件
        /// <summary>
        /// 产品入料
        /// </summary>
        public event Action<IMotionModule, StationResult, List<LColumn>> ProLoadedEvent;

        /// <summary>
        /// 产品入料事件
        /// </summary>
        /// <param name="jigCode"></param>
        public void OnProLoaded(string jigCode, string barCode, bool prevResult, DateTime dateTime)
        {
            if (Station == null)
            {
                SetCurStation(this);
            }

            if (Station != null && Station.TaskFunction is IStation curStation)
            {
                // 更新数据ID
                Station.DataID = barCode;

                var sResult = new StationResult()
                {
                    ProCode = barCode,
                    JigCode = jigCode,
                    EnterTime = dateTime,
                    Result = prevResult
                };

                if (curStation.Result == null)
                {
                    curStation.Result = sResult;
                }
                else
                {
                    curStation.Result.ProCode = barCode;
                    curStation.Result.JigCode = jigCode;
                    curStation.Result.Result = prevResult;
                }

                curStation.Enqueue(barCode);
                OnLog(LogType.Info, $"入队列二维码{sResult.ProCode}");
                // 通知产品入料事件
                ProLoadedEvent?.Invoke(this, sResult, curStation.Datas);

            }
        }

        /// <summary>
        /// 设置当前工站模块
        /// </summary>
        /// <param name="module"></param>
        private void SetCurStation(IMotionModule module)
        {
            if (Station != null)
            {
                return;
            }
            ;

            if (module.TaskFunction is IStation)
            {
                Station = this;
                return;
            }

            var p = module.Parent;
            if (p == null)
            {
                return;
            }


            if (p.TaskFunction is IStation curStation)
            {
                Station = p;
            }
            else
            {
                SetCurStation(p);
            }
        }

        /// <summary>
        /// 产品出料
        /// </summary>
        public event Action<IMotionModule, StationResult> ProUnloadedEvent;

        /// <summary>
        /// 产品出料
        /// </summary>
        /// <param name="barCode">产品编号</param>
        /// <param name="sResult">产品结果</param>
        /// <param name="datas">所有数据</param>
        public void OnProUnloaded(StationResult sResult)
        {
            if (Station == null)
            {
                SetCurStation(this);
            }

            if (Station != null && Station.TaskFunction is IStation station)
            {

                //1.首先需要判断配方中是否传入二维码
                //2.如果传入二维码，则使用配方中的传入二维码
                //3.如果未传入，则使用出队列的二维码

                //未传入SN
                if (sResult.ProCode == "NG")
                {
                    if (station.TryDequeue(out string dataID))
                    {
                        OnLog(LogType.Info, $"出队列二维码二维码{sResult.ProCode}");
                        sResult.ProCode = dataID;
                        Station.DataID = dataID;
                    }
                    //如果SN为空
                    else if (string.IsNullOrEmpty(Station.DataID))
                    {
                        Station.DataID = sResult.ProCode;
                    }
                }
                //传入了SN
                else
                {

                    Station.DataID = sResult.ProCode;
                }

                //// 将数据ID放到队列中
                //if (station.TryDequeue(out string dataID))
                //{
                //    OnLog(LogType.Info, $"出队列二维码二维码{sResult.ProCode}");
                //    sResult.ProCode = dataID;
                //    Station.DataID = dataID;
                //}
                //else if (!string.IsNullOrEmpty(Station.DataID))
                //{
                //    sResult.ProCode = Station.DataID;
                //}
                //else if (string.IsNullOrEmpty(Station.DataID))
                //{
                //    Station.DataID = sResult.ProCode;
                //}
                OnLog(LogType.Info, $"出料事件二维码{sResult.ProCode}");
                // 通知产品出料事件
                ProUnloadedEvent?.Invoke(Station, sResult);
            }
        }

        /// <summary>
        /// 产品抛料
        /// </summary>
        public event Action<StationResult, string, string> ProThrowEvent;



        /// <summary>
        /// 工站耗时统计事件
        /// WIP 
        /// 入站时间
        /// 出站时间
        /// 做料结果
        /// </summary>
        public event Action<string, string, string, bool> StationTimeEvent;

        /// <summary>
        /// 产品抛料
        /// </summary>
        public void OnProThrow(StationResult sResult, string material = "")
        {
            if (Station == null)
            {
                SetCurStation(this);
            }

            if (Station != null && Station.TaskFunction is IStation station)
            {
                ProThrowEvent?.Invoke(sResult, this.Alias, material);
            }
        }



        /// <summary>
        /// 工站结束
        /// </summary>
        public void OnStationTime(string WIP, string InputTime, string OutputTime, bool Result)
        {
            StationTimeEvent?.Invoke(WIP, InputTime, OutputTime, Result);
        }


        /// <summary>
        /// 获取对应的DataID
        /// </summary>
        /// <returns></returns>
        protected override string GetDataID()
        {
            if (Station == null)
            {
                UpdateStation();
            }

            if (TaskFunction is IStation)
            {
                return _dataID;
            }
            else
            {
                return Station.DataID;
            }
        }

        /// <summary>
        /// 检测如果是相同的节点，就不从缓存中获取数据
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public override bool IsSameRoot(IModule module)
        {
            // 如果当前模式是全局变量，默认获取当前值
            if (this is IGlobal) return true;

            if (module is IMotionModule m)
            {
                if (m.Station == null)
                {
                    m.UpdateStation();
                }

                if (Station == null)
                {
                    UpdateStation();
                }

                return m.Station == Station;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 数据上传
        /// </summary>
        /// <param name="code"></param>
        /// <param name="datas"></param>
        public void OnDataUpload(string code, List<LColumn> keyDatas)
        {
            DataUploadEvent?.Invoke(ID, code, keyDatas);
        }

        /// <summary>
        /// 运行完成后，默认执行上传事件
        /// </summary>
        private void OnDataUpload()
        {
            if (IsOutData)
            {
                List<LColumn> tempDatas = new List<LColumn>();

                foreach (var p in Parameters)
                {
                    var parameter = p.Value;
                    if (parameter.ParamType == Common.Enums.ParamType.IN) continue;

                    if (parameter.IsOutData)
                    {
                        tempDatas.Add(new LColumn(parameter));
                    }
                }

                if (Station == null)
                {
                    SetCurStation(this);
                }

                if (!string.IsNullOrEmpty(Station.DataID))
                {
                    OnDataUpload(Station.DataID, tempDatas);
                }
                else
                {
                    // 如果此时还没有触发入料事件，那么就将数据同步的本工站中
                    var station = Station.TaskFunction as IStation;
                    foreach (var item in tempDatas)
                    {
                        // 删除历史存在记录
                        station.Datas.RemoveAll(u => u == item);

                        // 新增当前记录
                        station.Datas.Add(item);
                    }
                }
            }
            else if (Station != null && !string.IsNullOrEmpty(Station.DataID) &&
                        Station.TaskFunction is IStation s)
            {
                // 自由工站如果存在数据就进行上传
                if (s.Datas.Count > 0)
                {
                    // 自由工站如果存在还没有上传的数据,那么就上传到缓存中，上传完成后，清理集合
                    OnDataUpload(Station.DataID, s.Datas);
                    s.Datas.Clear();
                }
            }
        }

        /// <summary>
        /// 产品抛料
        /// </summary>
        public event Action<Guid, string, List<LColumn>> DataUploadEvent;

        /// <summary>
        /// 产品阻塞事件
        /// </summary>
        public event Action<IMotionModule, double, string, string> ProBlockEvent;

        /// <summary>
        /// 产品阻塞
        /// </summary>
        /// <param name="blockCt">阻塞时间</param>
        /// <param name="errCode">错误编码 block/</param>
        /// <param name="reason">阻塞原因</param>
        public void OnProBlock(double blockCt, string errCode, string reason) => ProBlockEvent?.Invoke(this, blockCt, errCode, reason);
        #endregion

        public override void OnPropertyChanged(string propertyName, object srcVal, object newV)
        {
            base.OnPropertyChanged(propertyName, srcVal, newV);

            // 属性变更，更新模块别名
            if (propertyName == "Alias")
            {
                moduleAlias = "";
            }
        }

        #region 和其他模块引用关系
        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherRef"></param>
        /// <returns></returns>
        public bool IsOtherRef(out Dictionary<Guid, string> otherRef)
        {
            var keys = OtherRefModules.Keys.ToList();
            foreach (var item in keys)
            {
                if (!TaskModules.Contains(item))
                {
                    OtherRefModules.Remove(item);
                }
            }

            otherRef = OtherRefModules;

            return otherRef.Count > 0;
        }


        public event Func<string, InteractiveType> DialogBoxEvent;


        public InteractiveType OnDialogBox(string content)
        {
            return DialogBoxEvent.Invoke(content);
        }

        #endregion

        // 太科电批注册事件
        public event Action<object, string> TaiKeScrewRegisterEvent;
        public void TaiKeScrewRegister(object obj, string name)
        {
            TaiKeScrewRegisterEvent?.Invoke(obj, name);
        }


        // Toein注册事件
        public event Action<object, string> ToeinForceRegisterEvent;
        public void ToeinForceRegister(object obj, string name)
        {
            ToeinForceRegisterEvent?.Invoke(obj, name);
        }





        // 压力注册事件
        public event Action<object, string> PressRegisterEvent;
        public void PressRegister(object obj, string name)
        {
            PressRegisterEvent?.Invoke(obj, name);
        }

        // 压力注册事件
        public event Action<object, string> ForceRegisterEvent;


        public void ForceRegister(object obj, string name)
        {
            ForceRegisterEvent?.Invoke(obj, name);
        }

    }
}
