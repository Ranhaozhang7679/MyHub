#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AbsModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Module
* 文 件 名:       AbsModule
* 创建时间:       2021/10/30 9:57:24
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      2e3349c1-f7f8-498e-a0ba-e49dead11bde
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 9:57:24
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.Common.DataStruct;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Interfaces;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Common.DataStruct.Enums;
using Luster.TaskFlow.Common.Logics;
using Luster.Common.DataStruct.DataModels;
using System.Collections.ObjectModel;
using System.Runtime.Remoting;
using System.Collections.Concurrent;

namespace Luster.TaskFlow.Common.Module
{
    public abstract class AbsModule : IModule
    {
        #region NameInfo

        /// <summary>
        /// 别名
        /// </summary>
        private string _alias;

        public string Alias
        {
            get => _alias; set
            {
                var srcV = _alias;
                _alias = value;

                if (srcV != value)
                {
                    OnPropertyChanged(nameof(Alias), srcV, value);
                }
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模块提示
        /// </summary>
        public string Tips { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        #endregion

        /// <summary>
        /// 唯一ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        protected string statusMsg;
        public string StatusMsg { get => statusMsg; set => statusMsg = value; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 算子运行状态
        /// </summary>
        private RunStatus _runStatus;

        public RunStatus Status
        {
            get => _runStatus; set
            {
                var srcV = _runStatus;
                _runStatus = value;

                if (srcV != value)
                {
                    OnPropertyChanged(nameof(Status), srcV, value);
                }
            }
        }

        private ICoord3 refCoord;
        public ICoord3 RefCoord
        {
            get
            {
                var relCoord = this.GetRefCoord();
                if (relCoord != null)
                {
                    refCoord = relCoord.Value as ICoord3;
                }
                return refCoord;
            }
        }

        /// <summary>
        /// 模块模式
        /// </summary>
        public DesignMode Mode { get; set; }

        /// <summary>
        /// 交互者
        /// </summary>
        private IInteractor _interactor;

        public IInteractor Interactor
        {
            get => _interactor; set
            {
                _interactor = value;

                // 自己点配置
                foreach (var item in Children)
                {
                    item.Interactor = _interactor;
                }

                if (value != null)
                {
                    // 设计器模式
                    Mode = DesignMode.Design;
                }
            }
        }

        /// <summary>
        /// 是否激活状态
        /// </summary>
        private bool _isActive;

        public bool IsActive
        {
            get => _isActive; set
            {
                var srcV = _isActive;
                _isActive = value;

                if (srcV != value)
                {
                    OnPropertyChanged(nameof(IsActive), srcV, value);
                }
            }
        }

        /// <summary>
        /// 包含的函数列表
        /// </summary>
        public Dictionary<string, Type> FuncTypes { get; set; }

        /// <summary>
        /// 当前函数
        /// </summary>
        public IFunction TaskFunction { get; set; }

        /// <summary>
        /// 设置模块函数
        /// </summary>
        /// <param name="func">函数</param>
        public virtual void SetFunction(string funcName)
        {
            if (!FuncTypes.ContainsKey(funcName))
            {
                throw new FriendlyException($"Function:{funcName} does not exist in module:{Name}");
            }

            var func = Activator.CreateInstance(FuncTypes[funcName]) as IFunction;

            // 设置模块的Function
            TaskFunction = func;
            Alias = L(func.Name);
            func.Owner = this;

            // 默认运行时
            Mode = DesignMode.Runtime;
        }

        /// <summary>
        /// 参数字典
        /// </summary>
        public Dictionary<string, ParameterAttribute> Parameters { get; set; }

        /// <summary>
        /// 模块集合
        /// </summary>
        public IModuleCollection TaskModules { get; set; }

        /// <summary>
        /// 获取参考坐标系
        /// </summary>
        /// <returns></returns>
        public ParameterAttribute GetRefCoord()
        {
            ParameterAttribute coordAttr = null;
            if (Parent == null) return null;

            // 1.优先找同级的坐标系,从后往前找
            foreach (var item in Parent.Children.OrderByDescending(u => u.Sort))
            {
                if (item.Sort >= Sort) continue;

                if (item.Status == RunStatus.Skip) continue;

                if (item.TaskFunction is ICoord c)
                {
                    coordAttr = item.Parameters[c.ParamName];
                    break;
                }
            }

            // 2.找上级坐标系
            if (coordAttr == null)
            {
                if (Parent.Status != RunStatus.Skip && Parent.TaskFunction is ICoord c)
                {
                    coordAttr = Parent.Parameters[c.ParamName];
                }
                else
                {
                    coordAttr = Parent.GetRefCoord();
                }
            }

            return coordAttr;
        }

        /// <summary>
        /// 隶属父级节点
        /// </summary>
        public IModule Parent { get; set; }

        /// <summary>
        /// 是否当前节点
        /// </summary>
        private bool _isCurrent = false;

        public bool IsCurrent
        {
            get => _isCurrent; set
            {
                var srcV = _isCurrent;
                _isCurrent = value;

                if (srcV != value)
                {
                    OnPropertyChanged(nameof(IsCurrent), srcV, value);
                }
            }
        }

        /// <summary>
        /// 节点是否被选中
        /// </summary>
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected; set
            {
                var srcV = _isSelected;
                _isSelected = value;

                if (srcV != value)
                {
                    OnPropertyChanged(nameof(IsSelected), srcV, value);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// 子节点信息
        /// </summary>
        public List<IModule> Children { get; set; }

        /// <summary>
        /// 是否显示渲染结果
        /// </summary>
        private bool _visible = true;

        public bool Visible
        {
            get => _visible; set
            {
                var srcV = _visible;
                _visible = value;

                if (srcV != value)
                {
                    OnPropertyChanged(nameof(Visible), srcV, value);
                }
            }
        }

        /// <summary>
        /// 演员的颜色属性
        /// </summary>
        private string _actorColor = "";

        public string ActorColor
        {
            get => _actorColor;
            set
            {
                if (_actorColor != value)
                {
                    if (Status == RunStatus.Success)
                    {
                        Interactor?.SetActorColor(ID, value);
                    }
                    else
                    {
                        OnLog(LogType.Error, $"模块：{Alias}未运行");
                    }
                }

                _actorColor = value;
            }
        }


        /// <summary>
        /// 标签注释
        /// </summary>
        public bool LabelVisible { get; set; }

        /// <summary>
        /// 函数构造
        /// </summary>
        public AbsModule()
        {
            ID = Guid.NewGuid();
            Alias = Name;
            FuncTypes = new Dictionary<string, Type>();
            TaskFunction = null;
            Status = RunStatus.Default;
            Parameters = new Dictionary<string, ParameterAttribute>();
            Children = new List<IModule>();
            _sw = new Stopwatch();
            InitFunctions();
        }

        /// <summary>
        /// 初始化函数列表
        /// </summary>
        public abstract void InitFunctions();

        /// <summary>
        /// 添加功能函数
        /// </summary>
        /// <typeparam name="T">泛型函数</typeparam>
        protected virtual void AddFunction<T>() where T : IFunction
        {
            if (FuncTypes == null)
            {
                FuncTypes = new Dictionary<string, Type>();
            }

            // 类型
            Type funcType = typeof(T);

            // 检测是否存在该元素
            var pName = funcType.Name;
            if (!FuncTypes.ContainsKey(pName))
            {
                FuncTypes.Add(pName, funcType);
            }
        }

        /// <summary>
        /// 函数执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>是否执行成功</returns>
        public virtual bool DoFunction()
        {
            // 1.初始标识状态
            statusMsg = string.Empty;

            // 0.如果忽略状态不执行
            if (Status == RunStatus.Skip)
            {
                return true;
            }

            Status = RunStatus.Running;

            bool success = false;

            // 2.开始计算时间
            StartTimer();

            // 3.清空Output结果
            ResetParameters();

            try
            {
                // 4.参数验证及赋值
                var isValid = ValidateHelper.ValidateAllIn(TaskFunction, out statusMsg);

                // 5.函数运行
                if (isValid)
                {
                    success = TaskFunction.DoExcute(out statusMsg);
                }

                // 对输出参数 Value 结果进行赋值，供引用该参数的类型使用
                if (success)
                {
                    SetOutput();
                }
            }
            catch (Exception ex)
            {
                statusMsg = ex.Message;
            }

            // 终止计时器
            StopTimer();

            // 6.后处理
            if (success)
            {
                statusMsg = string.Empty;
                Status = RunStatus.Success;
            }
            else
            {
                OnLog(LogType.Error, $"{Alias}:{statusMsg}");
                Status = RunStatus.Error;
            }

            // 6.渲染
            if (Mode == DesignMode.Design)
            {
                if (success)
                {
                    Render();
                }
                else
                {
                    Interactor.RemoveActor(ID);
                }
            }

            // 打印Log信息
            if (success)
                OnLog(LogType.Debug, string.Format("模块:{0} 耗时:{1} ms", this.Alias, timeconsuming));

            return Status == RunStatus.Success;
        }

        #region 计时器相关

        /// <summary>
        /// 耗时统计
        /// </summary>
        protected Stopwatch _sw;

        /// <summary>
        /// 计算总耗时
        /// </summary>
        protected float timeconsuming = 0;

        protected float timeconsumingValid = 0;

        protected float timeconsumingDoexcute = 0;

        protected float timeconsumingSetOutput = 0;



        /// <summary>
        /// 耗时
        /// </summary>
        public float TimeConsuming
        {
            get { return timeconsuming; }
            set { timeconsuming = value; }
        }

        /// <summary>
        /// 当前产品ID
        /// </summary>
        protected string _dataID;
        public string DataID
        {
            get => GetDataID();
            set
            {
                var srcV = _dataID;
                _dataID = value;

                if (srcV != value)
                {
                    OnPropertyChanged(nameof(DataID), srcV, value);
                }
            }
        }

        /// <summary>
        /// 是否是相同的根节点
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public virtual bool IsSameRoot(IModule module)
        {
            return false;
        }
        /// <summary>
        /// 获取缓存ID
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDataID()
        {
            return _dataID;
        }

        /// <summary>
        /// 缓存管理器
        /// </summary>
        public ICacheManager CacheManager { get; set; }

        /// <summary>
        /// 初始化计时器
        /// </summary>
        public void StartTimer()
        {
            _sw.Reset();
            _sw.Start();
            timeconsuming = 0;
            timeconsumingValid = 0;
            timeconsumingDoexcute = 0;
            timeconsumingSetOutput = 0;
        }


        private readonly float TIME = 1000;

        /// <summary>
        /// 停止计时器
        /// </summary>
        public virtual void StopTimer()
        {
            _sw.Stop();
            timeconsuming = (float)Math.Round(_sw.Elapsed.TotalMilliseconds, 3);
        }

        #endregion

        /// <summary>
        /// 对象渲染
        /// </summary>
        public virtual void Render()
        {
            // 如果重新运行，则移除已有的Actor
            bool isDraw = TaskFunction.Draw(Interactor, ActorColor);

            // 如果没有自定义渲染方法，则自动查找输出函数
            if (!isDraw)
            {
                foreach (var item in Parameters)
                {
                    ParameterAttribute pVal = item.Value;
                    if (pVal == null || pVal.ParamType == ParamType.IN) continue;
                    if (pVal.Value is IActor actor && actor != null)
                    {
                        // 给对象赋值
                        actor.ID = ID;
                        actor.Label = Alias;

                        actor.AddActor(Interactor);
                        break;
                    }
                }
            }

            LabelRender();

            if (!string.IsNullOrEmpty(ActorColor))
            {
                Interactor?.SetActorColor(ID, ActorColor);
            }

            // 默认可见，如果不可见，则隐藏
            if (!Visible)
            {
                TaskFunction.SetDrawVisible(Interactor, Visible);
            }
        }

        /// <summary>
        /// 标签渲染
        /// </summary>
        public void LabelRender()
        {
            if (TaskFunction is ISetMeasure setMeasure)
            {
                Interactor.SetTextVisible(ID, Alias, LabelVisible, setMeasure.Measure);
            }
            else
            {
                Interactor.SetTextVisible(ID, Alias, LabelVisible);
            }
        }

        /// <summary>
        /// 对属性结果进行赋值
        /// </summary>
        protected virtual void SetOutput()
        {
            foreach (var item in Parameters)
            {
                var pItem = item.Value;
                if (pItem.ParamType == ParamType.IN) continue;

                // 获取属性对应的值

                if (pItem.Property != null)
                {
                    pItem.Value = pItem.Property.GetValue(TaskFunction, null);
                }

                // Struct基元，自定义out参数通过获取子节点的out参数
                //if (pItem.RefOut != null)
                //{
                //    // 1.如果依赖基元
                //    pItem.Value = pItem.RefOut.Owner.GetOutParameter(pItem.RefOut.Name);
                //}
                //else
                //{
                //    pItem.Value = pItem.Property.GetValue(TaskFunction, null);
                //}
            }
        }

        #region 参数设置

        /// <summary>
        /// 清理输出参数
        /// </summary>
        protected virtual void ResetParameters()
        {
            foreach (var item in Parameters)
            {
                object v = item.Value?.Value;
                if (v == null)
                {
                    continue;
                }

                if (item.Value.ParamType == ParamType.IN) continue;

                // 如果应用引用对象，不进行视频
                if (v is IReferenceObj r && r.IsReference)
                {
                    continue;
                }

                if (v is IDisposable disposable)
                {
                    disposable.Dispose();

                    // 属性置为空
                    item.Value.Property.SetValue(TaskFunction, null);
                }
                else if (item.Value.DefaultV != null)
                {
                    // 如果有默认值，值还原默认值
                    item.Value.Value = item.Value.DefaultV;
                }
                else if (v.GetType() == typeof(string))
                {
                    item.Value.Value = string.Empty;
                }
            }
        }

        /// <summary>
        /// 获取所有输出参数名称
        /// </summary>
        /// <returns>参数字典</returns>
        public Dictionary<string, Type> GetOutParameterNames()
        {
            Dictionary<string, Type> type = new Dictionary<string, Type>();
            foreach (var item in Parameters)
            {
                type.Add(item.Value.Alias, item.Value.Type);
            }

            return type;
        }

        /// <summary>
        /// 通过引用类型来查找当前模块引用了那些Module
        /// </summary>
        /// <param name="refType"></param>
        /// <returns></returns>
        public IModule[] GetReferModules(Type refType)
        {
            List<IModule> refModules = new List<IModule>();
            foreach (var item in Parameters)
            {
                var p = item.Value;
                if (p.ParamType == ParamType.OUT) continue;

                if (p.RefOut == null) continue;

                if (p.Type == refType || refType.IsAssignableFrom(p.Type))
                {
                    refModules.Add(p.RefOut.Owner);
                }
            }

            return refModules.ToArray();
        }

        /// <summary>
        /// 通过参数名称,获取参数对应的值
        /// </summary>
        /// <param name="pName">参数名称</param>
        /// <returns>参数对象</returns>
        public object GetOutParameter(string pName)
        {
            // 默认返回第一个输出参数
            if (string.IsNullOrWhiteSpace(pName))
            {
                foreach (var item in Parameters)
                {
                    return item.Value.Value;
                }
            }

            // 通过名称来获取
            if (Parameters.ContainsKey(pName))
            {
                var val = Parameters[pName].Value;

                // 如果对象支持Clone，那么就使用Clone模式
                if (val is ICloneable clone)
                {
                    return clone.Clone();
                }
                else
                {
                    return val;
                }
            }

            throw new KeyNotFoundException(pName);
        }

        public Dictionary<string, object> GetOutParameters(params string[] pNames)
        {
            Dictionary<string, object> vals = new Dictionary<string, object>();

            foreach (var item in pNames)
            {
                if (Parameters.ContainsKey(item))
                {
                    vals.Add(item, Parameters[item].Value);
                }
            }

            return vals;
        }

        public Dictionary<string, string> GetModuleParameters()
        {
            Dictionary<string, string> returnValues = new Dictionary<string, string>();
            foreach (KeyValuePair<string, ParameterAttribute> pair in Parameters)
            {
                //暂时选取中文名
                string key = pair.Value.CN;
                string value = pair.Value.Value?.ToString();
                returnValues.Add(key, value);
            }
            return returnValues;
        }

        /// <summary>
        /// 对参数进行赋值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public void SetInParameter(string name, object val)
        {
            if (Parameters.ContainsKey(name))
            {
                var parameter = Parameters[name];

                // 如果是函数内部赋值，那么不能触发UI验证
                bool isBind = parameter.IsBindUI;
                parameter.IsBindUI = false;

                // 如果参数的值已经存在并且实现了IDispose接口，那么就自动释放
                if (parameter.Value != null && parameter.Value is IDisposable dis)
                {
                    dis.Dispose();
                }

                if (val.GetType() == parameter.EditorType)
                {
                    parameter.EditorValue = val;
                }
                else
                {
                    parameter.EditorValue = null;
                    parameter.Value = val;
                }

                parameter.IsBindUI = isBind;
            }
            else
            {
                OnLog(LogType.Error, $"Parameter:{name} is not found!");
            }
        }

        public void SetInParameters(Dictionary<string, object> parameters)
        {
            foreach (var item in parameters)
            {
                SetInParameter(item.Key, item.Value);
            }
        }

        /// <summary>
        /// 给参数设置引用
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="refModuleID">引用的ID</param>
        /// <param name="refParamName">引用的参数名</param>
        public void SetInParameter(string name, Guid refModuleID, string refParamName = "")
        {
            if (Parameters.ContainsKey(name))
            {
                var parameter = Parameters[name];
                if (TaskModules.Contains(refModuleID))
                {
                    var refModule = TaskModules[refModuleID];
                    var paramters = refModule.Parameters;
                    if (string.IsNullOrEmpty(refParamName))
                    {
                        if (refModule.TaskFunction is IActive active)
                        {
                            parameter.RefOut = paramters[active.ParamName];
                        }
                    }
                    else
                    {

                        if (paramters.ContainsKey(refParamName))
                        {
                            parameter.RefOut = paramters[refParamName];
                        }
                    }
                }
            }
            else
            {
                OnLog(LogType.Error, $"Parameter:{name} is not found!");
            }
        }
        #endregion


        #region 导入导出

        /// <summary>
        /// XML解析
        /// </summary>
        /// <param name="xElement">XML</param>
        public virtual void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("ID", (id) => ID = Guid.Parse(id));
            xElement.GetAttribute("Tips", (tips) => Tips = tips);
            xElement.GetAttribute("Visible", (visible) => Visible = bool.Parse(visible));
            xElement.GetAttribute("ActorColor", (aColor) => ActorColor = aColor);
            xElement.GetAttribute("IsActive", (isActive) => IsActive = bool.Parse(isActive));
            xElement.GetAttribute("LabelVisible", (labelVisible) => IsActive = bool.Parse(labelVisible));
            xElement.GetAttribute("IsSkip", (isSkip) =>
            {
                if (bool.TryParse(isSkip, out bool skip) && skip)
                {
                    Status = RunStatus.Skip;
                }
            });

            xElement.GetAttribute("Sort", (sort) =>
            {
                Sort = int.Parse(sort);
            });

            // 创建函数
            XElement xFunc = xElement.Element("Function");
            if (xFunc != null && xFunc.Attribute("Name") != null)
            {
                string type = xFunc.Attribute("Name").Value;
                SetFunction(type);
                TaskFunction.ParserXml(xFunc);
            }

            // 因为 SetFunction 会重新赋值，所以该对象需要写在后面
            xElement.GetAttribute("Alias", (alias) => Alias = alias);
        }

        /// <summary>
        /// 模块导出到Xml中
        /// </summary>
        /// <returns>XElement</returns>
        public virtual XElement ExportXml()
        {
            XElement xModule = new XElement("Module");
            xModule.SetAttributeValue("ID", ID);
            xModule.SetAttributeValue("Name", Name);
            xModule.SetAttributeValue("Alias", Alias);
            xModule.SetAttributeValue("Tips", Tips);
            xModule.SetAttributeValue("Visible", Visible);
            xModule.SetAttributeValue("Sort", Sort);

            if (IsActive)
            {
                xModule.SetAttributeValue("IsActive", IsActive);
            }

            if (LabelVisible)
            {
                xModule.SetAttributeValue("LabelVisible", LabelVisible);
            }

            if (Status == RunStatus.Skip)
            {
                xModule.SetAttributeValue("IsSkip", true);
            }

            if (!string.IsNullOrEmpty(ActorColor))
            {
                xModule.SetAttributeValue("ActorColor", ActorColor);
            }

            var xFunc = TaskFunction.ExportXml();

            xModule.Add(xFunc);

            return xModule;
        }

        #endregion

        #region 克隆

        /// <summary>
        /// 模块克隆
        /// </summary>
        /// <returns>克隆对象</returns>
        public virtual object Clone()
        {
            XElement xClone = this.ExportXml();

            var cloneModule = Activator.CreateInstance(GetType()) as IModule;

            // 必须要配置名称
            cloneModule.Name = this.Name;
            cloneModule.Icon = this.Icon;
            cloneModule.Tips = this.Tips;
            cloneModule.TaskModules = this.TaskModules;
            cloneModule.Interactor = this.Interactor;
            cloneModule.UpdateEvent += this.UpdateEvent;
            cloneModule.LogEvent += this.LogEvent;
            cloneModule.LanguageEvent += this.LanguageEvent;
            cloneModule.ParserXml(xClone);
            cloneModule.IsActive = false;

            // 需要记住当前的模式，否则无法渲染
            cloneModule.Mode = this.Mode;

            if (Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    var childClone = child.Clone() as IModule;
                    cloneModule.Children.Add(childClone);
                }
            }

            return cloneModule;
        }

        #endregion

        #region 对象释放

        /// <summary>
        /// 临时变量
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// 对象释放
        /// </summary>
        public void Dispose()
        {
            // 释放自定义对象
            TaskFunction.Dispose();

            // 释放输出对象
            ResetParameters();

            if (Interactor != null)
                Interactor.RemoveActor(ID);

            Dispose(true);

            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 主动释放
        /// </summary>
        /// <param name="disposing">是否需要释放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            Parameters.Clear();
            FuncTypes.Clear();

            isDisposed = true;
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 事件消息
        /// </summary>
        public event Action<LogType, string, string> LogEvent;

        /// <summary>
        /// 算子变更
        /// </summary>
        public event Action<IModule, ModuleUpdate> UpdateEvent;

        /// <summary>
        /// 多语言
        /// </summary>
        public event Func<string, string> LanguageEvent;

        /// <summary>
        /// 日志触发
        /// </summary>
        /// <param name="type">日志记录</param>
        /// <param name="message">消息</param>
        public void OnLog(LogType type, string message, string logThread = "")
        {
            LogEvent?.Invoke(type, message, logThread);
        }

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public string L(string key)
        {
            if (LanguageEvent != null)
            {
                return LanguageEvent.Invoke(key);
            }
            else
            {
                return key;
            }
        }

        /// <summary>
        /// 模块变更通知
        /// </summary>
        public void OnUpdate(ModuleUpdate mUpdate) => UpdateEvent?.Invoke(this, mUpdate);

        /// <summary>
        /// 属性变更事件
        /// </summary>
        /// <param name="propertyName"></param>
        public virtual void OnPropertyChanged(string propertyName, object srcVal, object newV)
        {
            PropertyChangedEvent?.Invoke(this, propertyName, srcVal, newV);
        }

        /// <summary>
        /// 事件变更
        /// </summary>
        public event Action<IModule, string, object, object> PropertyChangedEvent;

        /// <summary>
        /// 获取模块树形结构
        /// </summary>
        /// <param name="isOutParam">是否输出参数</param>
        /// <param name="pIcon">对应参数图标</param>
        /// <param name="typeFunc">满足的类型</param>
        /// <returns>节点信息</returns>
        public virtual LNode GetTreeNode(bool isOutParam = false, string icon = "", Func<Type, bool> typeFunc = null)
        {
            LNode rNode = new LNode()
            {
                Text = this.Alias,
                Icon = this.TaskFunction?.Icon,
                Tag = this,
                Key = ID.ToString(),
            };

            if (Children.Count > 0)
            {
                foreach (var item in Children)
                {
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
                                    Icon = icon
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
                                Icon = icon
                            };
                            rNode.Children.Add(pNode);
                        }
                    }
                }
            }

            return rNode;
        }

        public virtual LNode GetTreeNodeForFlow(bool isOutParam = false, string icon = "", Func<Type, bool> typeFunc = null)
        {
            LNode rNode = new LNode()
            {
                Text = this.Alias,
                Icon = this.TaskFunction?.Icon,
                Tag = this,
                Key = ID.ToString(),
            };

            if (Children.Count > 0)
            {
                foreach (var item in Children)
                {
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
                                    Icon = icon
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
                                Icon = icon
                            };
                            rNode.Children.Add(pNode);
                        }
                    }
                }
            }

            return rNode;
        }

        #endregion

        #region 模块引用和被引用关系

        /// <summary>
        /// 引用模块
        /// </summary>
        private List<LReference> refModules = new List<LReference>();

        /// <summary>
        /// 被引用模块
        /// </summary>
        private List<LReference> byRefModules = new List<LReference>();

        /// <summary>
        /// 被引用模块的唯一键缓存，用于O(1)快速查重
        /// </summary>
        private HashSet<(Guid RefID, string RefName, string ByRefName)> _byRefModulesCache = new HashSet<(Guid, string, string)>();

        /// <summary>
        /// 获取引用的模块
        /// </summary>
        /// <returns></returns>
        public virtual void UpdateReferences()
        {
            // 1.将历史的引用都清除掉
            foreach (var item in GetReferences())
            {
                if (!TaskModules.Contains(item.RefID))
                {
                    continue;
                }

                var refModule = TaskModules[item.RefID];

                refModule.UnRegisterByRef(this, item.ByRefName);
            }

            refModules = new List<LReference>();
            byRefModules = new List<LReference>();
            _byRefModulesCache.Clear();

            // 1.正常引用
            foreach (var item in Parameters)
            {
                var pAttr = item.Value;
                if (pAttr.RefOut == null) continue;

                var refModule = pAttr.RefOut.Owner;

                // 注册引用
                refModule.RegisterByRef(this, item.Key, pAttr.RefOut.Name);

                // 添加引用模块和对应模块的参数
                refModules.Add(new LReference() { RefID = refModule.ID, RefName = pAttr.RefOut.Name, ByRefName = item.Key });
            }

            // 2.定制引用，针对Switch、if、WaitStatus
            if (TaskFunction is IRefFunction refFunc)
            {
                var references = refFunc.GetReferences();

                // Group by RefID to handle multiple references to the same module efficiently
                var refGroups = references.GroupBy(r => r.RefID).ToList();

                foreach (var group in refGroups)
                {
                    if (!TaskModules.Contains(group.Key))
                    {
                        continue;
                    }

                    // 引用模块
                    var refModule = TaskModules[group.Key];

                    // 使用 HashSet 加速 RefName 查找, O(1) instead of O(N)
                    var groupRefNames = new HashSet<string>(group.Select(g => g.RefName));

                    List<LReference> removeRefs = new List<LReference>();

                    // 检查模块是否还有无被引用
                    var byReferences = refModule.GetByReferences();

                    foreach (var item in byReferences)
                    {
                        if (!TaskModules.Contains(item.RefID))
                        {
                            continue;
                        }

                        var mItem = TaskModules[item.RefID];
                        var byRefs = mItem.GetReferences();

                        // Check if this back-reference is valid from our current perspective.
                        bool isProtected = (item.RefID == ID && groupRefNames.Contains(item.ByRefName));

                        if (byRefs.Count == 0 && !isProtected)
                        {
                            removeRefs.Add(item);
                        }
                    }

                    // 删除被引用的
                    if (removeRefs.Count > 0)
                    {
                        var removeSet = new HashSet<LReference>(removeRefs);
                        byReferences.RemoveAll(u => removeSet.Contains(u));
                    }

                    foreach (var refer in group)
                    {
                        refModule.RegisterByRef(this, refer.ByRefName, refer.RefName);
                    }
                }

                refModules.AddRange(references);
            }
        }

        /// <summary>
        /// 注册引用
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="byProp">被哪个参数引用</param>
        public void RegisterByRef(IModule module, string refName, string byRefName)
        {
            if (byRefModules == null)
            {
                byRefModules = new List<LReference>();
                _byRefModulesCache = new HashSet<(Guid, string, string)>();
            }

            // 使用 HashSet 进行 O(1) 查重，而不是 O(N) 的 Any() 查找
            var key = (module.ID, refName, byRefName);
            if (_byRefModulesCache.Add(key))
            {
                byRefModules.Add(new LReference() { RefID = module.ID, RefName = refName, ByRefName = byRefName });
            }
        }

        /// <summary>
        /// 取消注册引用
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="byProp">被哪个参数引用</param>
        public void UnRegisterByRef(IModule module, string byProp)
        {
            if (byRefModules == null || byRefModules.Count == 0) return;

            // 移除引用并同步更新缓存
            var toRemove = byRefModules.Where(u => u.RefName == byProp && u.RefID == module.ID).ToList();
            foreach (var item in toRemove)
            {
                byRefModules.Remove(item);
                _byRefModulesCache.Remove((item.RefID, item.RefName, item.ByRefName));
            }
        }

        /// <summary>
        /// 获取被哪些模块引用
        /// </summary>
        /// <returns></returns>
        public virtual List<LReference> GetReferences()
        {
            return refModules;
        }

        /// <summary>
        /// 获取被哪些模块引用
        /// </summary>
        /// <returns></returns>
        public virtual List<LReference> GetByReferences()
        {
            return byRefModules;
        }

        /// <summary>
        /// 删除所有引用
        /// </summary>
        public virtual void RemoveAllRefs()
        {
            // 1.将历史的引用都清除掉
            foreach (var item in GetReferences())
            {
                if (!TaskModules.Contains(item.RefID))
                {
                    continue;
                }

                var refModule = TaskModules[item.RefID];

                refModule.UnRegisterByRef(this, item.ByRefName);
            }
        }

        #endregion
    }
}