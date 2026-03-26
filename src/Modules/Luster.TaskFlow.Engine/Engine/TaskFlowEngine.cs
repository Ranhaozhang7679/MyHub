#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TaskFlowEngine
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Engine.Engine
* 文 件 名:       TaskFlowEngine
* 创建时间:       2021/11/1 16:58:56
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      ea00b636-444f-4824-abf0-e54e80e9733a
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/1 16:58:56
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.Common.DataStruct;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Engine.Events;
using Luster.ThreeD.Algorithm;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.Common.DataStruct.Interfaces;
using Luster.ThreeD.Algorithm.Interfaces;
using Luster.TaskFlow.Common;
using Luster.Common.DataStruct.Enums;
using Luster.TaskFlow.Common.Models;

namespace Luster.TaskFlow.Engine
{
    public class TaskFlowEngine : List<IModule>, ITaskFlowEngine, IModuleCollection
    {
        #region 字段

        /// <summary>
        /// 根节点
        /// </summary>
        private RootModule _rootModule;

        /// <summary>
        /// 参数信息
        /// </summary>
        private List<ParameterInfo> _paramInfos;

        /// <summary>
        /// 外部信息
        /// </summary>
        private Dictionary<string, object> _externalInfo;

        #endregion

        #region 属性

        /// <summary>
        /// 解析算子的工厂
        /// </summary>
        public IModuleFactory Factory { get; set; }

        /// <summary>
        /// 运行的编号
        /// </summary>
        public string ExcutorNo { get; set; }

        #endregion

        public TaskFlowEngine(IModuleFactory factory)
        {
            Factory = factory;
            Initialize();
            RegisterTypes();
        }

        /// <summary>
        /// 注册系统用到数据类型
        /// </summary>
        public virtual void RegisterTypes()
        {
            ParameterAttribute.ResgisterTypeMaps("Int", typeof(int));
            ParameterAttribute.ResgisterTypeMaps("String", typeof(string));
            ParameterAttribute.ResgisterTypeMaps("Double", typeof(double));
            ParameterAttribute.ResgisterTypeMaps("Bool", typeof(bool));
            ParameterAttribute.ResgisterTypeMaps("LPath", typeof(LPath));
            ParameterAttribute.ResgisterTypeMaps("Size", typeof(VSize));
            ParameterAttribute.ResgisterTypeMaps("Vector", typeof(VVector));
            ParameterAttribute.ResgisterTypeMaps("Direction", typeof(VDirection));
            ParameterAttribute.ResgisterTypeMaps("Cloud", typeof(VCloud));
            ParameterAttribute.ResgisterTypeMaps("Mesh", typeof(VMesh));
            ParameterAttribute.ResgisterTypeMaps("Line", typeof(VLine));
            ParameterAttribute.ResgisterTypeMaps("Plane", typeof(VPlane));
            ParameterAttribute.ResgisterTypeMaps("Circle", typeof(VCircle));
            ParameterAttribute.ResgisterTypeMaps("Cuboid", typeof(VCuboid));
            ParameterAttribute.ResgisterTypeMaps("Sphere", typeof(VSphere));
            ParameterAttribute.ResgisterTypeMaps("Cylinder", typeof(VCylinder));
            ParameterAttribute.ResgisterTypeMaps("Cone", typeof(VCone));
            ParameterAttribute.ResgisterTypeMaps("RelyType", typeof(LRelyType));
            ParameterAttribute.ResgisterTypeMaps("StringEx", typeof(LStringEx));

            // 接口
            ParameterAttribute.ResgisterTypeMaps("IGetProperty", typeof(IGetProperty));
            ParameterAttribute.ResgisterTypeMaps("IROI", typeof(IGeometry));
            ParameterAttribute.ResgisterTypeMaps("IDirection", typeof(IDirection));

            // 泛型对象
            ParameterAttribute.ResgisterTypeMaps("DArray", typeof(LArray<double>));
            ParameterAttribute.ResgisterTypeMaps("IArray", typeof(LArray<int>));
            ParameterAttribute.ResgisterTypeMaps("SArray", typeof(LArray<string>));
        }

        #region 事件

        /// <summary>
        /// 运行前事件
        /// </summary>
        public event Action<PrevRunEventArgs> PrevRunEvent;

        /// <summary>
        /// 运行后事件
        /// </summary>
        public event Action<PostRunEventArgs> PostRunEvent;

        /// <summary>
        /// Log日志
        /// </summary>
        public event Action<LogType, string, string> LogEvent;

        /// <summary>
        /// 多语言
        /// </summary>
        public event Func<string, string> LanguageEvent;

        /// <summary>
        /// Log日志
        /// </summary>
        public event Action<IModule, ModuleUpdate> MouleUpdateEvent;

        /// <summary>
        /// 输入或输出参数发生了变更
        /// </summary>
        public event Action<ITaskFlowEngine> ParameterUpdateEvent;
        #endregion

        #region 任务运行

        /// <summary>
        /// 单步执行
        /// </summary>
        /// <param name="mId">运行模块ID</param>
        /// <returns>是否运行成功</returns>
        public bool Run(Guid mId)
        {
            // 1.触发启动事件
            PrevRunEvent?.Invoke(new PrevRunEventArgs()
            {
                RunType = Enums.RunType.Run,
            });

            // 2.开始计时
            float timeConsuming = 0;

            // 3.是否执行成功
            bool isSuccess = false;

            // 4.执行异常记录错误提示
            string errMsg = string.Empty;

            IModule runModule = Get(Guid.Empty);

            // 5.循环运行
            foreach (IModule item in runModule.Children)
            {
                isSuccess = item.DoFunction();

                // 累加耗时信息
                timeConsuming += item.TimeConsuming;

                // 执行到此处中断
                if (item.ID == mId)
                {
                    break;
                }

                // 出现异常终端
                if (!isSuccess)
                {
                    errMsg = item.StatusMsg;
                    //运行模式下不阻断后面节点运行
                    if (item.Mode == DesignMode.Runtime)
                    {
                        continue;
                    }
                    else if (item is not ILogic)
                    {
                        break;
                    }
                }
            }

            // 6.设置总耗时
            runModule.TimeConsuming = timeConsuming;

            // 7.运行完成事件
            if (PostRunEvent != null)
            {
                PostRunEvent.Invoke(new PostRunEventArgs()
                {
                    IsSucess = isSuccess,
                    RunType = Enums.RunType.Run,
                    ErrorMessage = errMsg,
                    Outputs = GetOutputs(),
                    Time = timeConsuming,
                    ExternalInfo = new Dictionary<string, object>(_externalInfo)
                });
            }
            return isSuccess;
        }

        /// <summary>
        /// 执行所有
        /// </summary>
        /// <returns></returns>
        public bool RunAll()
        {

            return Run(Guid.Empty);
        }

        /// <summary>
        /// 单独运行
        /// </summary>
        /// <param name="mId"></param>
        /// <returns></returns>
        public bool RunOne(Guid mId)
        {
            IModule module = Get(mId);
            return module != null && module.DoFunction();
        }

        #endregion

        #region 算子管理

        /// <summary>
        /// 添加新的算子
        /// </summary>
        /// <param name="module">模块</param>
        public void Insert(IModule module, IModule pModule = null, int index = -1)
        {
            // 1.设置父节点
            if (pModule == null)
            {
                pModule = GetParentModule();

                // 2.判断是否logic,如果不是Loigc
                if (pModule is not ILogic)
                {
                    var parent = pModule.Parent;
                    int curIndex = parent.Children.IndexOf(pModule);
                    index = curIndex + 1;

                    // 更新模块父节点
                    pModule = parent;
                }
            }

            module.Parent = pModule;
            module.Level = pModule.Level + 1;

            // 2.设置对应的集合
            module.TaskModules = this;

            // 添加到Squence列表中
            if (index == -1)
            {
                // 添加到序列中
                Add(module);
                pModule.Children.Add(module);
            }
            else
            {
                Insert(index, module);
                pModule.Children.Insert(index, module);
            }
        }

        /// <summary>
        /// 通过模块名称和对应的函数名称来创建
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="funcName">函数名称</param>
        /// <returns>模块</returns>
        public IModule CreateByName(string moduleName, string funcName)
        {
            // 1.获取对应的Creator
            var mCreator = Factory.GetCreator(moduleName, "Holo3D");
            if (mCreator == null)
            {
                throw new FriendlyException($"Module:{moduleName} is not exist");
            }

            IModule newModule = mCreator.Create() as IModule;

            RegisterModuleEvent(newModule);

            newModule.SetFunction(funcName);

            // 默认排除最后
            newModule.Sort = Count + 1;
            newModule.TaskModules = this;

            return newModule;
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="module"></param>
        private void RegisterModuleEvent(IModule module)
        {
            // 3.给模块注册事件
            module.LogEvent -= Module_LogEvent;
            module.LogEvent += Module_LogEvent;

            // 4.模块变更事件
            module.UpdateEvent -= Module_UpdateEvent;
            module.UpdateEvent += Module_UpdateEvent;

            module.LanguageEvent -= Module_LanguageEvent;
            module.LanguageEvent += Module_LanguageEvent;
        }

        /// <summary>
        /// 多语言查询
        /// </summary>
        /// <param name="key">多语言关键字</param>
        /// <returns>Key对应的多语言</returns>
        private string Module_LanguageEvent(string key)
        {
            return LanguageEvent?.Invoke(key);
        }

        /// <summary>
        /// 模块更新方法
        /// </summary>
        /// <param name="obj"></param>
        private void Module_UpdateEvent(IModule obj, ModuleUpdate mUpdate) => MouleUpdateEvent?.Invoke(obj, mUpdate);

        /// <summary>
        /// 算子触发事件后，触发LogEvent方法
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="message">消息</param>
        private void Module_LogEvent(LogType type, string message, string logThread = "")
        {
            LogEvent?.Invoke(type, message, logThread);
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <returns></returns>
        private IModule GetParentModule()
        {
            var pModule = this.FirstOrDefault(u => u.IsCurrent);
            if (pModule == null)
            {
                // 设置为默认Current
                _rootModule.IsCurrent = true;
                return _rootModule;
            }
            else
            {
                return pModule;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 释放已有对象
            if (Count > 0)
            {
                foreach (var item in this)
                {
                    item.Dispose();
                }
            }

            // 清理所有对象
            Clear();

            // 初始化参数信息
            _paramInfos = new List<ParameterInfo>();
            _externalInfo = new Dictionary<string, object>();

            // 创建根节点基元
            _rootModule = new RootModule();
            _rootModule.SetFunction("Root");
            _rootModule.TaskModules = this;
            _rootModule.IsCurrent = true;

            // 添加到节点中
            Add(_rootModule);

            // 构建Excutor的唯一标识
            ExcutorNo = string.Empty;
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>模块</returns>
        public IModule Get(Guid id)
        {
            return this.FirstOrDefault(u => u.ID == id);
        }

        #endregion

        #region 导入和导出
        /// <summary>
        /// 加载工程文件
        /// </summary>
        /// <param name="projfile">工程文件路径，后缀是*.3dproj</param>
        public void LoadProjfile(string projfile)
        {
            try
            {
                var xProj = XElement.Load(projfile);
                ParserXml(xProj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            // 1.创建根节点
            XElement xRoot = new XElement("Luster3D");

            // 2.导出参数
            XElement xParameter = new XElement("Parameters");
            foreach (var item in _paramInfos)
            {
                xParameter.Add(item.ExportXml());
            }
            xRoot.Add(xParameter);

            // 2.创建基元集合
            var xPrims = new XElement("Modules");
            foreach (IModule item in _rootModule.Children.OrderBy(u => u.Sort))
            {
                var xPrim = item.ExportXml();
                if (xPrim != null)
                {
                    xPrims.Add(xPrim);
                }
            }

            xRoot.Add(xPrims);

            return xRoot;
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            // 对象初始化
            Initialize();
            // 解析参数信息
            XElement xParameter = xElement.Element("Parameters");
            if (xParameter != null)
            {
                foreach (var xItem in xParameter.Elements())
                {
                    ParameterInfo xParamInfo = new ParameterInfo();
                    xParamInfo.ParserXml(xItem);
                    _paramInfos.Add(xParamInfo);
                }
            }
            // 解析配置
            var xModules = xElement.Element("Modules");
            foreach (XElement xModule in xModules.Elements("Module"))
            {
                Parse(xModule, _rootModule);
            }
        }

        /// <summary>
        /// 解析基元
        /// </summary>
        /// <param name="xModule">xModule</param>
        /// <param name="parent">parent</param>
        public IModule Parse(XElement xModule, IModule parent, int index = -1)
        {
            // 1.获取基元类别
            var mName = xModule.Attribute("Name").Value;

            // 2.获取基元创建器
            IModuleCreator taskCreator = Factory.GetCreator(mName, "Holo3D");

            if (taskCreator == null)
            {
                throw new KeyNotFoundException($"Module:{mName} is not exist!");
            }

            // 3.创建基元
            var taskModule = taskCreator.Create() as IModule;
            taskModule.TaskModules = this;
            RegisterModuleEvent(taskModule);

            // 4.解析XML
            taskModule.ParserXml(xModule);

            var exist = Get(taskModule.ID);
            if (exist != null)
            {
                throw new FriendlyException($"task engine:{taskModule.Alias} is exist!");
            }

            // 1.设置父节点
            //taskModule.Parent = parent;
            //taskModule.Level = parent.Level + 1;
            //parent.Children.Add(taskModule);

            Insert(taskModule, parent, index);

            // 设置顺序
            //Add(taskModule);

            // 7.解析Children节点信息
            if (typeof(ILogic).IsAssignableFrom(taskModule.GetType()))
            {
                //parent.IsCurrent = false;

                //// 用于指定父节点
                //taskModule.IsCurrent = true;

                var xChildren = xModule.Element("Children");
                if (xChildren != null)
                {
                    var xChilds = xChildren.Elements("Module");
                    foreach (var item in xChilds)
                    {
                        Parse(item, taskModule);
                    }
                }
            }

            return taskModule;
        }

        #endregion

        #region IModuleCollection

        /// <summary>
        /// 是否存在当前ID
        /// </summary>
        /// <param name="mId">模块ID</param>
        /// <returns>是否包含</returns>
        public bool Contains(Guid mId)
        {
            return this.Any(u => u.ID == mId);
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="mId">模块ID</param>
        /// <returns>模块</returns>
        public IModule this[Guid mId] => Get(mId);

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="mId">模块ID</param>
        /// <returns>模块</returns>
        public IModule this[string mId] => Get(Guid.Parse(mId));

        #endregion

        #region 输入和输出参数配置

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="paramName">参数名称</param>
        public void AddParameterInfo(Guid moduleId, string paramName)
        {
            var module = Get(moduleId);
            if (_paramInfos.Any(u => u.ID == moduleId.ToString() && u.ParamName == paramName))
            {
                throw new FriendlyException($"{module.Alias}'s {paramName} is exists!");
            }

            if (!module.Parameters.ContainsKey(paramName))
            {
                throw new FriendlyException($"{module.Alias}'s is not exists {paramName}!");
            }

            var paramInfo = module.Parameters[paramName];
            _paramInfos.Add(new ParameterInfo()
            {
                ID = moduleId.ToString(),
                ParamName = paramName,
                Key = paramName,
                IsInput = paramInfo.ParamType == ParamType.IN,
                ParamType = module.Parameters[paramName].Type
            });
        }

        private string GetLabelName(string labelName, string paraName, string iD)
        {
            if (!_paramInfos.Any(u => u.Label == paraName))
            {
                return paraName;
            }
            if (_paramInfos.Any(u => u.Label == paraName) && !_paramInfos.Any(u => u.Key == paraName) && _paramInfos.Any(u => u.ID == iD))
            {
                return paraName;
            }

            if (paraName.LastIndexOf('_') >= 0)
            {
                paraName = paraName.Substring(0, paraName.LastIndexOf('_'));
            }

            var count = _paramInfos.Count(u => (u.Label.IndexOf(paraName) == 0 && u.Label.Substring(0, paraName.Length) == paraName) && (u.Key.IndexOf(paraName) == 0 && u.Key.Substring(0, paraName.Length) == paraName));
            return $"{paraName}_{count}";
        }

        public void AddParameterInfo(LNode node)
        {
            Guid moduleId = Guid.Parse(node.Parent.Key);
            string paramName = node.Key.Split('_').Last();
            var module = Get(moduleId);
            if (_paramInfos.Any(u => u.ID == moduleId.ToString() && u.Key == paramName))
            {
                throw new FriendlyException($"{module.Alias}'s {paramName} is exists!");
            }

            if (!module.Parameters.ContainsKey(paramName))
            {
                throw new FriendlyException($"{module.Alias}'s is not exists {paramName}!");
            }
            //同名修改
            string nodelabel = GetLabelName(node.Parent.Text, paramName, node.Parent.Key);
            var paramInfo = module.Parameters[paramName];
            _paramInfos.Add(new ParameterInfo()
            {
                ID = moduleId.ToString(),
                ParamName = node.Parent.Text,
                Key = paramName,
                IsInput = paramInfo.ParamType == ParamType.IN,
                ParamType = module.Parameters[paramName].Type,
                Label = nodelabel,
            });

            ParameterUpdateEvent?.Invoke(this);
        }

        public void UpDateParameterInfo(List<ParameterInfo> parameterInfos)
        {
            if (parameterInfos != null)
            {
                _paramInfos = parameterInfos;
            }
        }

        /// <summary>
        /// 移除参数
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="paramName">参数名称</param>
        public void RemoveParameterInfo(Guid moduleId, string paramName)
        {
            var paramInfo = _paramInfos.FirstOrDefault(u => u.ID == moduleId.ToString() && u.Key == paramName);
            if (paramInfo == null)
            {
                throw new FriendlyException($"Parameters is not exists {paramName}!");
            }
            _paramInfos.Remove(paramInfo);

            ParameterUpdateEvent?.Invoke(this);
        }

        /// <summary>
        /// 获取参数信息
        /// </summary>
        /// <returns>获取当前任务所有参数信息</returns>
        public List<ParameterInfo> GetParameterInfo()
        {
            foreach (var item in _paramInfos)
            {
                var module = Get(Guid.Parse(item.ID));
                if (module == null) continue;
                //别名可能重复，在输入输出模块支持自动修改
                //item.Label = $"{module.Alias}";

                if (!module.Parameters.ContainsKey(item.Key)) continue;

                var outVal = module.Parameters[item.Key];
                if (outVal.Value is null) continue;
                if (outVal.Value is ICloneable cloneable)
                {
                    item.Value = cloneable.Clone();
                }
                else
                {
                    item.Value = outVal.Value;
                }
            }

            return _paramInfos;
        }

        /// <summary>
        /// 设置输入参数
        /// </summary>
        /// <param name="label">label名称</param>
        /// <param name="value">参数值</param>
        public void SetInput(string label, object value)
        {
            if (value == null)
            {
                LogEvent?.Invoke(LogType.Error, "value is null!", "");
                return;
            }


            foreach (var item in _paramInfos.Where(u => u.IsInput))
            {
                if (item.Label == label)
                {
                    if (item.ParamType != value.GetType())
                    {
                        LogEvent?.Invoke(LogType.Error, $"value's type is {value.GetType()},but Paramameter's type is {item.ParamType}!", "");
                        break;
                    }

                    var module = Get(Guid.Parse(item.ID));
                    if (module != null)
                    {
                        module.SetInParameter(item.Key, value);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 添加额外信息
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="val">值</param>
        public void AddExternInfo(string name, object val)
        {
            if (_externalInfo == null)
            {
                _externalInfo = new Dictionary<string, object>();
            }

            if (_externalInfo.ContainsKey(name))
            {
                _externalInfo[name] = val;
            }
            else
            {
                _externalInfo.Add(name, val);
            }
        }

        /// <summary>
        /// 获取输出参数
        /// </summary>
        /// <param name="pNames">参数名称，如果不为空，则默认获取所有参数</param>
        /// <returns>返回所有输出参数</returns>
        public List<ParameterInfo> GetOutputs(params string[] pNames)
        {
            List<ParameterInfo> paramInfos = new List<ParameterInfo>();

            if (pNames == null || pNames.Length == 0)
            {
                foreach (var item in _paramInfos)
                {
                    if (item.IsInput) continue;

                    var module = Get(Guid.Parse(item.ID));

                    // 给对象赋值
                    item.Value = module.GetOutParameter(item.Key);

                    paramInfos.Add(item);
                }
            }
            else
            {
                foreach (var item in _paramInfos)
                {
                    if (item.IsInput) continue;

                    if (!pNames.Contains(item.ParamName)) continue;

                    var module = Get(Guid.Parse(item.ID));

                    // 给对象赋值
                    item.Value = module.GetOutParameter(item.Key);

                    paramInfos.Add(item);
                }
            }

            return paramInfos;
        }

        /// <summary>
        /// 对象Clone
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            // 导出XML
            var exportXml = this.ExportXml();

            var newObj = new TaskFlowEngine(Factory);
            newObj.ParserXml(exportXml);

            return newObj;
        }

        #endregion
    }
}