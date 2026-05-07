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
        /// �������ƶ���
        /// </summary>
        public IIocManager Ioc { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public IOrderManager OrderManager { get; set; }


        /// <summary>
        /// ���Ϲ���
        /// </summary>
        public IRollManager RollManager { get; set; }

        /// <summary>
        /// ���������
        /// </summary>
        public IErrorManager ErrorManager { get; set; }

        /// <summary>
        /// ��Դ������
        /// </summary>
        public ILightManager LightManager { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public IConfigManager ConfigManager { get; set; }

        /// <summary>
        /// ���ݿ���ʽӿ�
        /// </summary>
        public IDbHelper DbHelper { get; set; }

        /// <summary>
        /// �ӽڵ�
        /// </summary>
        public new List<IMotionModule> Children { get; set; }

        /// <summary>
        /// �������ڵ�
        /// </summary>
        public new IMotionModule Parent { get; set; }

        /// <summary>
        /// ����ģʽ
        /// </summary>
        public RunMode RunMode { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public AlarmInfo AlarmInfo { get; set; }

        /// <summary>
        /// ��׼CT
        /// </summary>
        public int CT { get; set; } = 0;

        /// <summary>
        /// ������վ
        /// </summary>
        public IMotionModule Station { get; set; }

        /// <summary>
        /// ��ʼ�¼�
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// ��ͣʱ��
        /// </summary>
        public float PauseTime { get; set; } = 0;

        /// <summary>
        /// ǰһģ��
        /// </summary>
        public IMotionModule PrevModule { get; set; }

        /// <summary>
        /// ��һ��ģ��
        /// </summary>
        public IMotionModule NextModule { get; set; }

        /// <summary>
        /// �豸
        /// </summary>
        public IDeviceEngine DeviceEngine { get; set; }


        /// <summary>
        /// �����¼�
        /// </summary>
        public event Action<string, string> MachineStatusComplete;

        /// <summary>
        /// ��ֹ����
        /// </summary>
        public bool IsBreak { get; set; }

        /// <summary>
        /// �Ƿ�����������
        /// </summary>
        public bool IsOutData { get; set; }
        /// <summary>
        /// ��ǰ���д���
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
        /// �߳�ID
        /// </summary>
        public int ThreadID { get; set; }

        /// <summary>
        /// �汾
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// �������� Ĭ�ϱ�������Ϊ -1��δ���ã�����Ҫ�ϴ�������Ϣ
        /// </summary>
        public virtual string ErrorCode { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        public virtual string ErrorContent { get; set; }

        /// <summary>
        /// �Ƿ�ʹ��Log
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
        /// ģ��ڵ�����
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
        /// ����
        /// </summary>
        //public new IMotionFunction TaskFunction { get; set; }

        /// <summary>
        /// �����߳��ж�
        /// </summary>
        public ManualResetEventSlim BrokenOff { get; set; }


        /// <summary>
        /// �����¼�
        /// </summary>
        public event Action<AlarmInfo> AlarmEvent_module;

        /// <summary>
        /// ������ģ��
        /// </summary>
        private Dictionary<Guid, string> OtherRefModules = new Dictionary<Guid, string>();

        /// <summary>
        /// ���캯��
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

            //// ����ģ���Function
            //TaskFunction = func;
            //Alias = L(func.Name) ?? func.Name;
            TaskFunction.Owner = this;

            // Ĭ������ʱ
            //Mode = DesignMode.Runtime;
        }

        /// <summary>
        /// ģ������
        /// </summary>
        private string moduleAlias = "";

        /// <summary>
        /// ģ��������վ
        /// </summary>
        private IStation mStation = null;

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="errMsg">������Ϣ</param>
        /// <returns>�����Ƿ�ɹ�</returns>
        public override bool DoFunction()
        {
            // 忽略状态不执行
            if (Status == Common.Enums.RunStatus.Skip)
            {
                return true;
            }

            ThreadID = Thread.CurrentThread.ManagedThreadId;

            if (IsBreak)
            {
                OnLog(LogType.Debug, $"ģ��:{Alias} ��ֹͣ,��Ҫ�����������!");
                return true;
            }

            if (string.IsNullOrEmpty(moduleAlias))
            {
                UpdateStation();
            }
            if (moduleAlias.Contains("�����¼�"))
            {
                OnLog(LogType.Debug, $"ģ��:{moduleAlias}��ʼ");
            }
            // ����״̬��ʼ��
            // ģ�����п�ʼʱ��
            StartTime = DateTime.Now;

            // 1.��ʼ��ʶ״̬
            statusMsg = string.Empty;
            AlarmInfo = null;
            bool success = false;

            TaskFunction.Status.SetDefault();
            Status = Common.Enums.RunStatus.Running;

            // ��ʾ��ǰ���е�ģ��
            if (Station != null && Station.TaskFunction is IFreeStation f)
            {
                f.AddRunningModule(this);
            }

            // 2.��ʼ����ʱ��
            StartTimer();

            // 3.���Output���
            ResetParameters();

            try
            {
                // 4.������֤����ֵ
                var isValid = ValidateHelper.ValidateAllIn(TaskFunction, out statusMsg);
                timeconsumingValid = (float)Math.Round(_sw.Elapsed.TotalMilliseconds, 3);
                // 5.��������
                if (isValid)
                {
                    // ��ʱ����
                    success = TaskFunction.DoExcute(out statusMsg);
                    timeconsumingDoexcute = (float)Math.Round(_sw.Elapsed.TotalMilliseconds, 3);

                    if (moduleAlias == "���칤վ")
                    {
                        OnLog(LogType.Debug, $"ģ��:{moduleAlias}�ɹ�");
                    }
                }

                // ��������� Value ������и�ֵ�������øò���������ʹ��
                if (success)
                {
                    SetOutput();
                    timeconsumingSetOutput = (float)Math.Round(_sw.Elapsed.TotalMilliseconds, 3);
                    if (moduleAlias == "���칤վ")
                    {
                        OnLog(LogType.Debug, $"ģ��:{moduleAlias}SetOutput()");
                    }
                }
            }
            catch (DeviceTimeoutException tx)
            {
                //bool isEmpty = string.IsNullOrEmpty(ErrorCode) || ErrorCode == "��������";

                string eCode = tx.AlarmCode;
                string eMessage = tx.Message;
                // ״̬������
                Status = Common.Enums.RunStatus.Alarmed;
                AlarmInfo = new AlarmInfo(this, AlarmType.Timeout, $"{eMessage}", $"{eCode}") { DeviceID = tx.DeviceID, Module = tx.Module, Name = tx.DeviceName };
                statusMsg = tx.Message;
            }
            catch (DeviceException dx)
            {
                //bool isEmpty = string.IsNullOrEmpty(ErrorCode) || ErrorCode == "��������";
                string eCode = dx.AlarmCode;
                string eMessage = dx.Message;
                AlarmInfo = new AlarmInfo(this, AlarmType.DeviceError, $" {eMessage}", eCode) { DeviceID = dx.DeviceID };
                statusMsg = dx.Message;
            }
            catch (Exception ex)
            {
                statusMsg = ex.Message;

                // ������쳣
                OnLog(LogType.Error, $"ģ��:{Alias},{statusMsg},{ex.StackTrace}");
            }

            // ��ֹ��ʱ��
            StopTimer();
            if (moduleAlias == "���칤վ")
            {
                OnLog(LogType.Debug, $"ģ��:{moduleAlias}StopTimer()");
            }
            // 6.����
            if (success)
            {
                Status = Common.Enums.RunStatus.Success;
            }
            else
            {
                Status = Common.Enums.RunStatus.Error;

                // ���״̬��Ϣ
                string errMsg = $"ģ��:{moduleAlias} ����ʧ��:{statusMsg}";
                OnLog(LogType.Error, errMsg);
            }
            if (mStation != null && mStation.UseLog &&
                DeviceEngine != null && DeviceEngine.AutoSetMotionModuleCT)
            {
                this.CT = Convert.ToInt32(timeconsuming);
            }

            // 6.��Ⱦ
            if (Mode == DesignMode.Design && success)
            {
                Render();
                if (moduleAlias == "���칤վ")
                {
                    OnLog(LogType.Debug, $"ģ��:{moduleAlias}Render()");
                }
            }
            try
            {
                // ��ӡLog��Ϣ
                if (success)
                {
                    // ��¼������վ����ʱ��� �����CT�Ǵ���0������ģ��֧����ͣ
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

                    // �����ݽ����ϴ�  
                    OnDataUpload();
                    if (moduleAlias == "���칤վ")
                    {
                        OnLog(LogType.Debug, $"ģ��:{moduleAlias}OnDataUpload()");
                    }
                    // ��¼���еĴ���
                    RunNum++;

                    // ֻ������LOG�Ž��м�¼
                    if (mStation != null && mStation.UseLog)
                        OnLog(LogType.Info, string.Format("ģ��:{0} ��ʱ:{1} ms ��ͣ��ʱ:{2} ���������ʱ:{3} ִ�к�ʱ:{4} ���������ʱ:{5}",
                                                          moduleAlias,
                                                          TimeConsuming,
                                                          PauseTime,
                                                          timeconsumingValid,
                                                          timeconsumingDoexcute,
                                                          timeconsumingSetOutput));

                    // ����״̬���
                    TaskFunction.Status.SetEnd();
                }
            }
            catch (Exception ex)
            {
                // ������飬�����ٴγ��֣��������������������
                // ���ڱ���̻߳�ģ�����ڱ���List��ֱ�� Clear()����ʱ��������ʹ���У�����Խ�硣
                // ������ȷ�� mStation.StationTimes �ķ������̰߳�ȫ�ġ�
                lock (mStation.StationTimes)
                {
                    mStation.StationTimes.Clear();
                }
                OnLog(LogType.Info, string.Format("ģ��:{0} ����:{1} ", moduleAlias, ex.ToString()));
            }


            // ����ʱ��
            EndTime = DateTime.Now;

            return Status == Common.Enums.RunStatus.Success;
        }

        /// <summary>
        /// �ܺ�ʱ��ȥ��ͣʱ��
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
        /// ����У��
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
        /// ���¹�վ��Ϣ
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
        /// �ݹ����
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

        #region ��������
        /// <summary>
        /// �����ж�
        /// </summary>
        /// <param name="alarmMsg">������Ϣ</param>
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

                // ��ģ�鱨�������¸�ģ�鱨��
                Station.Status = Status;
                Station.AlarmInfo = AlarmInfo;

                // ��Ϊ��ģ�鱨�����Ὣ������Ϣ���ݵ���ģ�飬���Ա����¼������Ǳ�ģ���Լ�����
                if (AlarmInfo.Sender == this)
                {
                    AlarmEvent_module?.Invoke(AlarmInfo); //������MotionEngion
                }
            }
        }

        /// <summary>
        /// �������
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

            // ����Ҫ��������
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

            // ��Ҫ��ס��ǰ��ģʽ�������޷���Ⱦ
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
            // ʹ�� StringBuilder �����ַ���ƴ�ӵ��ڴ����
            var sb = new StringBuilder();

            foreach (var item in Parameters)
            {
                // ��ǰ���ˣ�����Ƕ��
                if (item.Value?.ParamType != ParamType.IN)
                    continue;

                object v = item.Value.Value;
                if (v == null)
                    continue;

                // ʹ�� StringBuilder.Append��������ʱ�ַ���
                sb.Append(item.Value.Name)
                  .Append('#')
                  .Append(JsonTool.ToJson(v))
                  .Append(';');
            }

            // �Ƴ�ĩβ�ֺţ��� Trim ����Ч��ֻ�����ض��ַ���
            if (sb.Length > 0)
                sb.Length--; // ֱ���޸ĳ��ȣ����ⴴ�����ַ���

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

                    // ѭ��֧�ִ�ʱ����
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
                IsSelected = this.IsSelected
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

                    // ѭ��֧�ִ�ʱ����
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
        

        #region ����͵���
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

            // �����ǰģ���ǹ�վ
            if (TaskFunction is IStation)
            {
                Station = this;
            }
        }
        #endregion

        #region ��Ʒ����¼�
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public event Action<IMotionModule, StationResult, List<LColumn>> ProLoadedEvent;

        /// <summary>
        /// ��Ʒ�����¼�
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
                // ��������ID
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
                OnLog(LogType.Info, $"����ж�ά��{sResult.ProCode}");
                // ֪ͨ��Ʒ�����¼�
                ProLoadedEvent?.Invoke(this, sResult, curStation.Datas);

            }
        }

        /// <summary>
        /// ���õ�ǰ��վģ��
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
        /// ��Ʒ����
        /// </summary>
        public event Action<IMotionModule, StationResult> ProUnloadedEvent;

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        /// <param name="barCode">��Ʒ���</param>
        /// <param name="sResult">��Ʒ���</param>
        /// <param name="datas">��������</param>
        public void OnProUnloaded(StationResult sResult)
        {
            if (Station == null)
            {
                SetCurStation(this);
            }

            if (Station != null && Station.TaskFunction is IStation station)
            {

                //1.������Ҫ�ж��䷽���Ƿ����ά��
                //2.��������ά�룬��ʹ���䷽�еĴ����ά��
                //3.���δ���룬��ʹ�ó����еĶ�ά��

                //δ����SN
                if (sResult.ProCode == "NG")
                {
                    if (station.TryDequeue(out string dataID))
                    {
                        OnLog(LogType.Info, $"�����ж�ά���ά��{sResult.ProCode}");
                        sResult.ProCode = dataID;
                        Station.DataID = dataID;
                    }
                    //���SNΪ��
                    else if (string.IsNullOrEmpty(Station.DataID))
                    {
                        Station.DataID = sResult.ProCode;
                    }
                }
                //������SN
                else
                {

                    Station.DataID = sResult.ProCode;
                }

                //// ������ID�ŵ�������
                //if (station.TryDequeue(out string dataID))
                //{
                //    OnLog(LogType.Info, $"�����ж�ά���ά��{sResult.ProCode}");
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
                OnLog(LogType.Info, $"�����¼���ά��{sResult.ProCode}");
                // ֪ͨ��Ʒ�����¼�
                ProUnloadedEvent?.Invoke(Station, sResult);
            }
        }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public event Action<StationResult, string, string> ProThrowEvent;



        /// <summary>
        /// ��վ��ʱͳ���¼�
        /// WIP 
        /// ��վʱ��
        /// ��վʱ��
        /// ���Ͻ��
        /// </summary>
        public event Action<string, string, string, bool> StationTimeEvent;

        /// <summary>
        /// ��Ʒ����
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
        /// ��վ����
        /// </summary>
        public void OnStationTime(string WIP, string InputTime, string OutputTime, bool Result)
        {
            StationTimeEvent?.Invoke(WIP, InputTime, OutputTime, Result);
        }


        /// <summary>
        /// ��ȡ��Ӧ��DataID
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
        /// ����������ͬ�Ľڵ㣬�Ͳ��ӻ����л�ȡ����
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public override bool IsSameRoot(IModule module)
        {
            // �����ǰģʽ��ȫ�ֱ�����Ĭ�ϻ�ȡ��ǰֵ
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
        /// �����ϴ�
        /// </summary>
        /// <param name="code"></param>
        /// <param name="datas"></param>
        public void OnDataUpload(string code, List<LColumn> keyDatas)
        {
            DataUploadEvent?.Invoke(ID, code, keyDatas);
        }

        /// <summary>
        /// ������ɺ�Ĭ��ִ���ϴ��¼�
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
                    // �����ʱ��û�д��������¼�����ô�ͽ�����ͬ���ı���վ��
                    var station = Station.TaskFunction as IStation;
                    foreach (var item in tempDatas)
                    {
                        // ɾ����ʷ���ڼ�¼
                        station.Datas.RemoveAll(u => u == item);

                        // ������ǰ��¼
                        station.Datas.Add(item);
                    }
                }
            }
            else if (Station != null && !string.IsNullOrEmpty(Station.DataID) &&
                        Station.TaskFunction is IStation s)
            {
                // ���ɹ�վ����������ݾͽ����ϴ�
                if (s.Datas.Count > 0)
                {
                    // ���ɹ�վ������ڻ�û���ϴ�������,��ô���ϴ��������У��ϴ���ɺ���������
                    OnDataUpload(Station.DataID, s.Datas);
                    s.Datas.Clear();
                }
            }
        }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public event Action<Guid, string, List<LColumn>> DataUploadEvent;

        /// <summary>
        /// ��Ʒ�����¼�
        /// </summary>
        public event Action<IMotionModule, double, string, string> ProBlockEvent;

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        /// <param name="blockCt">����ʱ��</param>
        /// <param name="errCode">������� block/</param>
        /// <param name="reason">����ԭ��</param>
        public void OnProBlock(double blockCt, string errCode, string reason) => ProBlockEvent?.Invoke(this, blockCt, errCode, reason);
        #endregion

        public override void OnPropertyChanged(string propertyName, object srcVal, object newV)
        {
            base.OnPropertyChanged(propertyName, srcVal, newV);

            // ���Ա��������ģ�����
            if (propertyName == "Alias")
            {
                moduleAlias = "";
            }
        }

        #region ������ģ�����ù�ϵ
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

        // ̫�Ƶ���ע���¼�
        public event Action<object, string> TaiKeScrewRegisterEvent;
        public void TaiKeScrewRegister(object obj, string name)
        {
            TaiKeScrewRegisterEvent?.Invoke(obj, name);
        }


        // Toeinע���¼�
        public event Action<object, string> ToeinForceRegisterEvent;
        public void ToeinForceRegister(object obj, string name)
        {
            ToeinForceRegisterEvent?.Invoke(obj, name);
        }





        // ѹ��ע���¼�
        public event Action<object, string> PressRegisterEvent;
        public void PressRegister(object obj, string name)
        {
            PressRegisterEvent?.Invoke(obj, name);
        }

        // ѹ��ע���¼�
        public event Action<object, string> ForceRegisterEvent;


        public void ForceRegister(object obj, string name)
        {
            ForceRegisterEvent?.Invoke(obj, name);
        }

    }
}
