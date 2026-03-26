#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       EventBus
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI
* 文 件 名:       EventBus.cs
* 创建时间:       2022/5/24 11:04:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      08038353-c255-4ef6-8d14-679c264cfae0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 11:04:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;

namespace Luster.Motion.EditorUI
{
    /// <summary>
    /// 事件总线
    /// </summary>
    public class FlowBus
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _engine;

        /// <summary>
        /// 事件总线
        /// </summary>
        public IEventAggregator Bus { get; set; }

        /// <summary>
        /// 用来记录前进还原功能
        /// </summary>
        private Stack<XElement> backStack = new Stack<XElement>();

        /// <summary>
        /// 用来记录前进
        /// </summary>
        private Stack<XElement> forwordStack = new Stack<XElement>();

        /// <summary>
        /// 通用Bus总线
        /// </summary>
        private ICommonBus commonBus;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionEngine">引擎构造</param>
        public FlowBus(IMotionEngine motionEngine, IEventAggregator eBus, ICommonBus commBus)
        {
            _engine = motionEngine;
            Bus = eBus;
            commonBus = commBus;


            _engine.ModuleUpdateEvent += (m, u) =>
            {
                // 更新状态
                commBus.IsNeedSave = true;
                eBus.GetEvent<ModuleUpdateEvent>().Publish(new ModuleUpdateModule() { Module = m, UpdateType = u });
            };

            // 加载xml
            commBus.LoadRecipeEvent += (xFlow) =>
            {
                backStack.Clear();
                forwordStack.Clear();
            };
        }

        /// <summary>
        /// 获取当前模块
        /// </summary>
        /// <returns></returns>
        public IMotionModule GetCurrent()
        {
            return _engine.FirstOrDefault(u => u.IsCurrent);
        }

        /// <summary>
        /// 对模块排序
        /// </summary>
        private void SortModule(IMotionModule pModule)
        {
            if (pModule.Name == "Root")
            {
                pModule.Children.Sort(new StationCompare());
                for (int i = 0; i < pModule.Children.Count; i++)
                {
                    var child = pModule.Children[i];
                    child.Sort = i;
                }
            }
            else
            {
                IMotionModule prevModule = null;

                int count = pModule.Children.Count;
                for (int i = 0; i < count; i++)
                {
                    var item = pModule.Children[i];
                    item.Sort = i;

                    // 只有任务流才会需要设置上一模块和下一模块
                    if (item.TaskFunction is not IStation)
                    {
                        if (i == 0)
                        {
                            item.PrevModule = null;
                        }
                        else if (i == count - 1)
                        {
                            prevModule.NextModule = item;
                            item.PrevModule = prevModule;
                            item.NextModule = null;
                        }
                        else
                        {
                            prevModule.NextModule = item;
                            item.PrevModule = prevModule;
                        }

                        prevModule = item;
                    }
                }

                if (pModule.TaskFunction is ISwitch || pModule.TaskFunction is IBranch || pModule.TaskFunction is IParallel)
                {
                    pModule.Children.ForEach(u =>
                    {
                        u.NextModule = null;
                        u.PrevModule = null;
                    });
                }
            }
        }

        #region 界面交互

        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="module">模块名称</param>
        /// <param name="func">函数</param>
        public IMotionModule OnAddModule(string module, string func, int index = -1, int row = 0, int col = 0)
        {
            // 1.基于名称和方法名称构建模块
            var newModule = _engine.CreateByName(module, func);
            newModule.Alias = GetDefaultAlias(newModule);

            var curModule = GetCurrent();
            if (curModule == null)
            {
                throw new FriendlyException($"模块名:{module},函数名:{func} 不存在!");
            }

            // 如果在首页，值只能方
            if (curModule.Parent == null)
            {
                var station = newModule.TaskFunction as IStation;
                if (station == null)
                {
                    throw new FriendlyException($"主页面只能添加工站模块!");
                }
                else
                {
                    if (row > 0 || col > 0)
                    {
                        station.Row = row;
                        station.Column = col;
                    }
                    else
                    {
                        var stations = _engine.GetStations();
                        if (stations.Count > 0)
                        {
                            // 新增时，默认行在最大行下面
                            var maxRow = stations.Max(u => (u.TaskFunction as IStation).Row);
                            station.Row = maxRow + 1;
                        }
                        else
                        {
                            station.Row = 0;
                        }
                    }

                    // 配置模块自身
                    newModule.Station = newModule;
                }
            }
            else
            {
                if (newModule.TaskFunction is IStation station)
                {
                    throw new FriendlyException($"工站只能添加在主页中!");
                }
            }

            // 模块添加之前
            Bus.GetEvent<ModulePrevAddEvent>().Publish(newModule);
            if ((newModule.TaskFunction is ISwitch || newModule.TaskFunction is IBranch) && newModule.Children.Count == 0)
            {
                return newModule;
            }

            // 0.工程变更
            OnRecipeChanged();

            // 2.添加到engine中
            _engine.Insert(newModule, index);

            _engine.BuildPrimModule();


            // 3.排序
            SortModule(curModule);

            // 4.更新工站
            newModule.UpdateStation();

            // 5.事件通知
            Bus.GetEvent<ModuleAddEvent>().Publish(newModule);

            // 6.log 记录
            commonBus.OnChangeRecord(OperationType.Add, newModule.Alias, "", $"模块新增:{newModule.Alias}");
            commonBus.OnLog(LogType.Info, $"模块新增:{newModule.Alias}");

            return newModule;
        }

        private bool MatchParameter(ParameterAttribute outParam, ParameterAttribute dstParam)
        {
            ParamType pType = outParam.Owner.Name == "Global" ? ParamType.IN : ParamType.OUT;

            var type = dstParam.Type;

            return outParam.ParamType == pType && (type.IsAssignableFrom(outParam.Type) || outParam.Type == type);
        }

        private void BuildRefValue(IMotionModule module, ParameterAttribute dstParamAttr, List<LNode> list)
        {
            foreach (var gParam in module.Parameters)
            {
                if (module is not IGlobal global && gParam.Value.ParamType == ParamType.IN)
                {
                    continue;
                }

                if (MatchParameter(gParam.Value, dstParamAttr))
                {
                    var refNode = new LNode()
                    {
                        Key = gParam.Key,
                        Text = $"{module.Alias}.{gParam.Value.CN}",
                        Tag = gParam.Value,
                        Tag1 = dstParamAttr,
                        IsSelected = false,
                        Icon = (dstParamAttr.RefOut == gParam.Value) ? "Link" : ""
                    };

                    list.Add(refNode);
                }
            }

            // 递归遍历
            //foreach (var item in module.Children)
            //{
            //    BuildRefValue(item, dstParamAttr, list);
            //}
        }

        private void GetStationModule(IMotionModule module, ref IMotionModule stationModule)
        {
            if (module.TaskFunction is IStation station)
            {
                stationModule = module;
                return;
            }

            if (module.Parent != null)
            {
                GetStationModule(module.Parent, ref stationModule);
            }
        }

        public List<LNode> GetRefNodes(ParameterAttribute dstParamAttr)
        {
            // 1.找全局变量
            // 2.找站上的变量
            // 3.找同一目录的变量
            var type = dstParamAttr.Type;
            var list = new List<LNode>();
            var gModule = _engine.Get(GlobalModule.GlobalID);

            // 1.找全局变量
            var gNode = new LNode()
            {
                Key = "Global",
                Text = $"{commonBus.L("Global")}",
                Tag = "Group",
                Tag1 = gModule,
                IsSelected = false,
                Icon = ""
            };
            List<LNode> gList = new List<LNode>();
            BuildRefValue(gModule, dstParamAttr, gList);
            if (gList.Count > 0)
            {
                gNode.Children.AddRange(gList);
                list.Add(gNode);
            }

            var owner = dstParamAttr.Owner as IMotionModule;
            if (owner.Station == null)
            {
                owner.UpdateStation();
            }

            // 2.找隶属站上的变量
            var stations = _engine.GetStations().OrderBy(u => u.Sort);
            foreach (var item in stations)
            {
                if (owner.Station.TaskFunction is IHomeStation)
                {
                    if (item.TaskFunction is IHomeStation)
                    {
                        BuildModuleRef(item, dstParamAttr, list);
                    }
                }
                else if (owner.Station.TaskFunction is ITestStation)
                {
                    if (item.TaskFunction is ITestStation)
                    {
                        BuildModuleRef(item, dstParamAttr, list);
                    }
                }
                else
                {
                    if (item.TaskFunction is IHomeStation || item.TaskFunction is ITestStation)
                    {
                        continue;
                    }

                    BuildModuleRef(item, dstParamAttr, list);
                }
            }

            return list;
        }

        /// <summary>
        /// 获取全局变量和本站引用
        /// </summary>
        /// <returns></returns>
        public List<ParameterAttribute> GetVariables(ParameterAttribute pAttr, IMotionModule pModule = null)
        {
            List<ParameterAttribute> lstVars = new List<ParameterAttribute>();

            if (pModule == null)
            {
                pModule = GetCurrent();
            }

            foreach (var item in _engine)
            {
                if (item.TaskFunction is IStation) continue;

                // 当前模块的变量
                if (item == pAttr.Owner)
                {
                    foreach (var p in item.Parameters)
                    {
                        if (p.Value.ParamType == ParamType.OUT) continue;

                        if (p.Value.Type.IsNumeric() || p.Value.Type == typeof(string) || p.Value.Type == typeof(DateTime))
                        {
                            lstVars.Add(p.Value);
                        }
                    }
                }

                // 支持全局变量引用或者相同节点引用
                var isGlobal = item is IGlobal;
                if (isGlobal || item.Parent == pModule)
                {
                    // 并且在后面
                    if (!isGlobal && item.Sort >= pAttr.Owner.Sort) continue;

                    foreach (var p in item.Parameters)
                    {
                        // 忽略输出
                        if (!isGlobal && p.Value.ParamType == ParamType.IN) continue;

                        if (p.Value.Type.IsNumeric() || p.Value.Type == typeof(string) || p.Value.Type == typeof(DateTime))
                        {
                            lstVars.Add(p.Value);
                        }
                    }
                }
            }

            return lstVars;
        }

        private void BuildModuleRef(IMotionModule module, ParameterAttribute dstParamAttr, List<LNode> nodes)
        {
            if (module.TaskFunction is IRefComposition refComp)
            {
                var refModule = refComp.GetRefModule();

                if (refModule != null)
                {
                    BuildModuleRef(refModule, dstParamAttr, nodes);
                }
                else
                {
                    commonBus.OnLog(LogType.Warning, $"引用模块:{module.Alias}对应的组合模块不存在!");
                }
            }
            else if (module.TaskFunction is IGroup || module.TaskFunction is ISwitch || module.TaskFunction is IStation)
            {
                var sNode = new LNode()
                {
                    Key = $"{module.ID}",
                    Text = $"{module.Alias}",
                    Tag = "Group",
                    Tag1 = module,
                    IsSelected = false,
                    Icon = ""
                };

                List<LNode> sList = new List<LNode>();
                BuildRefValue(module, dstParamAttr, sList);

                foreach (var item in module.Children)
                {
                    var myOwner = dstParamAttr.Owner as IMotionModule;
                    if (myOwner.Parent == module && dstParamAttr.Owner.Sort <= item.Sort) continue;
                    BuildModuleRef(item, dstParamAttr, sList);
                }

                sNode.Children.AddRange(sList);

                nodes.Add(sNode);
            }
            else
            {
                BuildRefValue(module, dstParamAttr, nodes);
            }

            //foreach (var item in stations)
            //{
            //    if (item.Sort > mStation.Sort) continue;

            //    var sNode = new LNode()
            //    {
            //        Key = "Station",
            //        Text = $"{item.Alias}",
            //        Tag = "Group",
            //        Tag1 = item,
            //        IsSelected = false,
            //        Icon = ""
            //    };

            //    List<LNode> sList = new List<LNode>();
            //    BuildRefValue(item, dstParamAttr, sList);

            //    foreach (var subItem in item.Children)
            //    {
            //        //if (subItem.Sort > module.Sort) continue;

            //        // 如果再相同的层级,模块位置还在当前模块后面，就忽略
            //        if (subItem.Parent == module.Parent && subItem.Sort > module.Sort)
            //        {
            //            continue;
            //        }

            //        List<LNode> sList = new List<LNode>();
            //        BuildRefValue(subItem, dstParamAttr, sList);

            //        sNode.Children.AddRange(sList);
            //    }

            //    if (sNode.Children.Count > 0)
            //    {
            //        nodes.Add(sNode);
            //    }
            //}
        }

        /// <summary>
        /// 提取获取Alias
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private string GetDefaultAlias(IMotionModule module)
        {
            var name = module.Alias;

            // 1.如果没有重名
            if (!_engine.Any(u => u.Alias == name))
            {
                return name;
            }

            if (name.LastIndexOf('-') >= 0)
            {
                name = name.Substring(0, name.LastIndexOf('-'));
            }

            // 1.检测是否有重名的元素
            // 检测是否有重名，如果重名则名称 +1
            
            var count = _engine.Count(u => u.Alias.IndexOf(name) == 0 && u.Alias.Substring(0, name.Length) == name);

            return $"{name}{count}";
        }

        /// <summary>
        /// 模块移动
        /// </summary>
        /// <param name="modules">模块列表</param>
        /// <param name="destIndex">移动的目标</param>
        public void OnMoveModule(List<IMotionModule> modules, int destIndex)
        {
            if (modules.Count == 0)
            {
                throw new FriendlyException("要移动的模块不存在!");
            }

            OnRecipeChanged();

            var curModule = GetCurrent();

            // 原始索引
            var srcIndex = modules[0].Sort;

            if (srcIndex > destIndex)
            {
                // 从后往前删除模块
                var descModules = modules.OrderByDescending(u => u.Sort);
                foreach (var item in descModules)
                {
                    curModule.Children.RemoveAt(item.Sort);
                }

                // 将移动的模块添加到模组中
                foreach (var item in modules)
                {
                    curModule.Children.Insert(destIndex++, item);
                }
            }
            else
            {
                // 将移动的模块添加到模组中
                foreach (var item in modules)
                {
                    destIndex++;
                    curModule.Children.Insert(destIndex, item);
                }

                foreach (var item in modules)
                {
                    curModule.Children.RemoveAt(item.Sort);
                }
            }

            // 重新排序
            SortModule(curModule);

            string moduleNames = string.Join(",", modules.Select(u => u.Alias).ToArray());
            // 5.log 记录
            commonBus.OnLog(LogType.Info, $"模块移动:{moduleNames} {srcIndex}->{destIndex}");


            Bus.GetEvent<ModuleMovedEvent>().Publish(modules.ToArray());
        }

        /// <summary>
        /// Station站模块发生了变化
        /// </summary>
        public void OnStationMove(IMotionModule stationModule)
        {
            if (stationModule != null && stationModule.TaskFunction is IStation station)
            {
                var parent = stationModule.Parent;

                // 2.更新模块Sort
                SortModule(parent);

                // 3.对连接线进行判断自动断掉
                foreach (var item in parent.Children)
                {
                    if (item.TaskFunction is IStation sta)
                    {
                        sta.PrevStations.RemoveAll(u => u.Sort > sta.Sort);
                    }
                }

                // 5.log 记录
                commonBus.OnLog(LogType.Info, $"模块移动:{stationModule.Alias} 新的行:{station.Row},新的列:{station.Column}");

                // 4.通知刷新
                Bus.GetEvent<ModuleMovedEvent>().Publish(new IMotionModule[] { stationModule });
            }
        }

        /// <summary>
        /// 触发选择事件
        /// </summary>
        /// <param name="selectModule">要选择的模块</param>
        public void OnSelected(params IMotionModule[] selectModules)
        {
            foreach (var item in _engine)
            {
                item.IsSelected = false;
            }

            foreach (var item in selectModules)
            {
                item.IsSelected = true;
            }

            Bus.GetEvent<ModuleSelectedEvent>().Publish(selectModules);
        }

        /// <summary>
        /// 触发模块删除事件
        /// </summary>
        /// <param name="mId"></param>
        public void OnRemoveModule(params IMotionModule[] modules)
        {
            if (modules.Length == 0)
            {
                throw new FriendlyException("未找到要删除的模块!");
            }

            // 1.对象是否被应用
            foreach (var item in modules)
            {
                var byRefs = item.GetByReferences();
                if (byRefs.Count == 0) continue;

                foreach (var byref in byRefs)
                {
                    var motion = _engine.Get(byref.RefID);
                    if (motion == null) continue;

                    throw new FriendlyException($"模块被:{motion.Alias} 引用,无法删除!");
                }
            }

            // 在动作之前进行备份
            OnRecipeChanged();


            List<Guid> ids = new List<Guid>();

            foreach (var removeModule in modules)
            {
                var parent = removeModule.Parent;
                if (parent == null)
                {
                    throw new FriendlyException("根节点无法删除!");
                }

                RemoveRescure(removeModule);

                // 更新删除的模块
                removeModule.RemoveAllRefs();

                // 删除隶属父节点
                parent.Children.Remove(removeModule);

                ids.Add(removeModule.ID);
            }

            // 重新排序和更新前后节点
            SortModule(modules[0].Parent as IMotionModule);


            // 触发删除事件
            Bus.GetEvent<ModuleRemoveEvent>().Publish(modules);
        }

        /// <summary>
        /// 模块递归删除
        /// </summary>
        /// <param name="removeModule"></param>
        private void RemoveRescure(IMotionModule removeModule)
        {
            foreach (var item in removeModule.Children)
            {
                RemoveRescure(item);
            }

            // 对象首先要先释放
            removeModule.Dispose();

            // 数据集合中删除
            _engine.RemoveByID(removeModule.ID);

            // 删除所有子节点
            if (removeModule.Children.Count > 0)
            {
                removeModule.Children.RemoveAll(u => true);
            }

            commonBus.OnLog(LogType.Info, $"模块删除:{removeModule.Alias}");
            commonBus.OnChangeRecord(OperationType.Delete, removeModule.Alias, "", $"模块删除:{removeModule.Alias}");
        }

        /// <summary>
        /// 切换模块
        /// </summary>
        /// <param name="module"></param>
        public void OnLoaded(IMotionModule module, IMotionModule selectModule = null)
        {
            if (module != null && module.TaskFunction is IGroup)
            {
                var curModule = GetCurrent();
                curModule.IsCurrent = false;
                module.IsCurrent = true;

                // 将已有被选择的取消选中
                foreach (var item in _engine)
                {
                    if (item.IsSelected)
                        item.IsSelected = false;
                }

                if (selectModule != null)
                {
                    foreach (var item in module.Children)
                    {
                        item.IsSelected = item == selectModule;
                    }
                }

                // 通知切换功能
                Bus.GetEvent<ModuleLoadedEvent>().Publish(module);
            }
        }

        /// <summary>
        /// 删除上一个站
        /// </summary>
        /// <param name="iD1"></param>
        /// <param name="iD2"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void OnRemovePrevStation(Guid prevID, Guid currentID)
        {
            var curStation = GetModule(currentID);
            if (curStation.TaskFunction is IStation station)
            {
                station.PrevStations.RemoveAll(u => u.OwnerID == prevID);

                Bus.GetEvent<ModuleUpdateEvent>().Publish(new ModuleUpdateModule() { Module = curStation, UpdateType = ModuleUpdate.Move });
            }
        }

        /// <summary>
        /// 触发提取功能
        /// </summary>
        /// <param name="extractType"></param>
        /// <param name="label">提取到的步骤名称</param>
        /// <param name="modules"></param>
        public void OnExtract(string extractType, string label, params IMotionModule[] modules)
        {
            // 1.创建类别
            IMotionModule newModule = _engine.CreateByName("Logic", extractType);
            newModule.Alias = label;

            foreach (var item in modules)
            {
                // 从原来节点中删除
                item.Parent.Children.Remove(item);

                // 加入新节点
                item.Parent = newModule;
                newModule.Children.Add(item);
            }

            SortModule(newModule);

            var current = GetCurrent();
            _engine.Insert(newModule);
            SortModule(current);

            // 6.自动切换到Swith模块中
            Bus.GetEvent<ModuleLoadedEvent>().Publish(current);
        }


        /// <summary>
        /// 提取switch
        /// </summary>
        /// <param name="conditon"></param>
        /// <param name="text"></param>
        /// <param name="modules"></param>
        public void OnExtractSwitch(LCondition conditon, string switchText, string groupName, params IMotionModule[] modules)
        {
            // 1.创建模块
            var switchModule = _engine.CreateByName("Logic", "Switch");
            switchModule.Alias = switchText;
            switchModule.SetInParameter("Condition", conditon);

            // 2.创建分支
            foreach (var item in conditon)
            {
                var group = _engine.CreateByName("Logic", "Group");
                group.Alias = item.Name;
                _engine.Insert(group, 0, switchModule);
            }

            // 3.提取当前模块到group中
            var curGroup = switchModule.Children.FirstOrDefault(u => u.Alias == groupName);
            if (curGroup != null)
            {
                foreach (var item in modules)
                {
                    // 从原来节点中删除
                    item.Parent.Children.Remove(item);

                    // 加入新节点
                    item.Parent = curGroup;
                    curGroup.Children.Add(item);
                }

                SortModule(curGroup);
            }

            var current = GetCurrent();

            // 4.添加到引擎中
            _engine.Insert(switchModule);

            // 5.重新排序
            SortModule(current);

            // 6.自动切换到Swith模块中
            Bus.GetEvent<ModuleLoadedEvent>().Publish(current);
        }

        /// <summary>
        /// 触发忽略事件
        /// </summary>
        /// <param name="skipModules">忽略模块</param>
        public void OnSkip(params IMotionModule[] skipModules)
        {
            OnRecipeChanged();

            foreach (var item in skipModules)
            {
                if (item.Status == RunStatus.Skip)
                {
                    item.Status = RunStatus.Default;
                    commonBus.OnChangeRecord(OperationType.Skip, item.Alias, "", $"取消忽略:{item.Alias}");
                }
                else
                {
                    item.Status = RunStatus.Skip;
                    commonBus.OnChangeRecord(OperationType.Skip, item.Alias, "", $"模块忽略:{item.Alias}");
                }
            }

            Bus.GetEvent<ModuleSkipEvent>().Publish(skipModules);
        }
        #endregion

        #region 运行模块
        /// <summary>
        /// 防止快速的点击F5造成并发执行任务报错的bug
        /// </summary>
        private int lockNum = 0;

        /// <summary>
        /// 递归初始化状态
        /// </summary>
        /// <param name="module"></param>
        private void SetStatusDefault(IMotionModule module)
        {
            if (module.Status == RunStatus.Skip) return;

            // 还原状态
            module.Status = RunStatus.Default;
            if (module.Children.Count > 0)
            {
                foreach (var child in module.Children)
                {
                    SetStatusDefault(child);
                }
            }
        }

        /// <summary>
        /// 触发运行
        /// </summary>
        /// <param name="runID">要执行模块的ID</param>
        public void OnRun(Guid runID)
        {
            if (lockNum > 0) return;

            // 运行前事件
            Bus.GetEvent<TaskPrevEvent>().Publish();

            // 状态统一更新默认
            foreach (var item in _engine)
            {
                SetStatusDefault(item);
            }

            // 防止界面卡顿
            Task.Run(() =>
            {
                try
                {
                    Interlocked.Increment(ref lockNum);
                    if (runID == Guid.Empty)
                    {
                        _engine.RunAll();
                    }
                    else
                    {
                        _engine.Run(runID);
                    }
                }
                catch (Exception ex)
                {
                    commonBus.OnLog(LogType.Error, ex.Message);
                }
                finally
                {
                    _engine.EngineStatus = EngineStatus.Idle;
                    Bus.GetEvent<TaskPostEvent>().Publish();
                    Interlocked.Decrement(ref lockNum);
                }
            });

            // 防止程序运行一直运行，造成无法点击运行按钮的bug
            Task.Run(() =>
            {
                Thread.Sleep(2000);
                if (lockNum > 0)
                {
                    Interlocked.Decrement(ref lockNum);
                }
            });
        }

        /// <summary>
        /// 单步执行
        /// </summary>
        /// <param name="runID">要执行模块的ID</param>
        public void OnRunOne(params Guid[] runIDs)
        {
            if (runIDs.Length == 0) return;

            var list = new List<IMotionModule>();
            foreach (var item in runIDs)
            {
                var module = _engine.Get(runIDs[0]);
                SetStatusDefault(module);

                list.Add(module);
            }

            // 如果模块本身需要耗时加载或者模块属于ILogic模块，那么需要通过异步进行执行，否则会卡顿主线程
            // 防止界面卡顿
            Task.Run(() =>
            {
                // 单步运行
                Bus.GetEvent<ModulePrevRunEvent>().Publish();

                Interlocked.Increment(ref lockNum);
                foreach (var rID in runIDs)
                {
                    _engine.RunOne(rID);
                }

                Interlocked.Decrement(ref lockNum);

                // 单步运行结束
                Bus.GetEvent<ModulePostRunEvent>().Publish(list.ToArray());
                commonBus.OnLog(LogType.Info, $"模块单步运行完成");
            });

            // 防止程序运行一直运行，造成无法点击运行按钮的bug
            Task.Run(() =>
            {
                Thread.Sleep(2000);
                if (lockNum > 0)
                {
                    Interlocked.Decrement(ref lockNum);
                }
            });
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void OnStop()
        {
            if (lockNum > 0)
            {
                Interlocked.Decrement(ref lockNum);
            }

            _engine.RunStop();
        }
        #endregion

        /// <summary>
        /// 获取节点信息
        /// </summary>
        /// <returns>将任务模块转换为树节点</returns>
        public IMotionModule GetRootModule()
        {
            return _engine.Get(Guid.Empty);
        }

        /// <summary>
        /// 通过ID获取模块
        /// </summary>
        /// <param name="modID">模块ID</param>
        /// <returns>模块方法</returns>
        public IMotionModule GetModule(Guid modID)
        {
            return _engine.Get(modID);
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        /// <param name="xCopy">xCopyElement</param>
        /// <param name="parent">parent</param>
        public void OnPaste(XElement xCopy, IMotionModule parent, int index = -1)
        {
            // 0.粘贴后需要记录上次的执行器内容
            OnRecipeChanged();

            if (parent == null)
            {
                parent = GetCurrent();
            }

            // 1.转换为字符串
            string strXML = xCopy.ToString();

            try
            {
                // 2.替换GUID，防止和现有基元的冲突
                ReplaceXML(xCopy, ref strXML);

                List<Guid> newIDs = new List<Guid>();

                List<IMotionModule> newModules = new List<IMotionModule>();
                // 3.添加到当前基元中
                var newXElement = XElement.Parse(strXML);
                var maxRow = _engine.GetStations().Select(u => u.TaskFunction as IStation).Max(u => u.Row) + 1;
                foreach (var xModule in newXElement.Elements("Module"))
                {
                    newIDs.Add(Guid.Parse(xModule.Attribute("ID").Value.ToString()));
                    var newModule = _engine.Parse(xModule, parent, index);

                    if (newModule.TaskFunction is IStation station)
                    {
                        station.Row += maxRow;
                    }

                    newModules.Add(newModule);

                    if (index != -1) index++;
                }

                SortModule(parent);

                Bus.GetEvent<ModulePastedEvent>().Publish(newModules.ToArray());
            }
            catch (XmlException xe)
            {
                // 5.log 记录
                commonBus.OnLog(LogType.Error, $"模块粘贴异常:{xe.Message}");
            }
        }

        /// <summary>
        /// 替换XML的ID
        /// </summary>
        /// <param name="xModule">模块Xml</param>
        /// <param name="strXML">strXML</param>
        private void ReplaceXML(XElement xModule, ref string strXML)
        {
            // 解析当前节点
            var xId = xModule.Attribute("ID");
            if (xId != null)
            {
                // 替换相同的ID
                var newID = Guid.NewGuid().ToString();
                strXML = strXML.Replace(xId.Value, newID);
            }

            // 解析子节点
            if (xModule != null)
            {
                foreach (var subPrim in xModule.Elements("Module"))
                {
                    ReplaceXML(subPrim, ref strXML);

                    var xChild = subPrim.Element("Children");
                    if (xChild != null)
                    {
                        ReplaceXML(xChild, ref strXML);
                    }
                }
            }
        }

        /// <summary>
        /// 节点的
        /// </summary>
        /// <param name="modules"></param>
        private void RescureAddChildrenClone(List<IMotionModule> modules, IMotionModule parent, List<Guid> ids)
        {
            if (modules == null || modules.Count == 0) return;

            foreach (var item in modules)
            {
                ids.Add(item.ID);

                // 更新ID
                item.ID = Guid.NewGuid();

                _engine.Add(item);

                item.Parent = parent;

                RescureAddChildrenClone(item.Children, item, ids);
            }

            SortModule(parent);
        }

        /// <summary>
        /// 通过关键字搜索功能列表
        /// </summary>
        /// <param name="type">0 通过模块名称搜索 1通过模块类型搜索</param>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public List<KeyValue> SearchFuncByKey(int type, string strKey)
        {
            List<KeyValue> list = new List<KeyValue>();

            string keyUppper = strKey.ToUpper();

            List<IMotionModule> modules = new List<IMotionModule>();
            if (type == 0)
            {
                modules = _engine.Where(u => u.Alias.ToUpper().Contains(keyUppper)).ToList();
            }
            else
            {

                modules = _engine.Where(u => commonBus.L(u.TaskFunction.Name).Contains(strKey)).ToList();
            }

            int i = 1;
            foreach (var func in modules)
            {
                if (func.TaskFunction is IRefComposition) continue;
                if (func.Station == null)
                {
                    func.UpdateStation();
                }

                string name = func.Alias;
                if (func.Station != func)
                {
                    name = $"{func.Station?.Alias}_{func.Alias}";
                }

                list.Add(new KeyValue() { Key = func.ID.ToString(), Value = func, Desc = name });
                i++;
                //if (i > 30)
                //{
                //    break;
                //}
            }

            return list;
        }

        #region 前进和后退命令

        /// <summary>
        /// 后退事件
        /// </summary>
        public void OnBackForward(bool isBack)
        {
            XElement xTask = null;
            if (isBack)
            {
                if (backStack.Count == 0) return;
                forwordStack.Push(_engine.ExportXml());
                Remain10Stack(forwordStack);
                xTask = backStack.Pop();
            }
            else
            {
                if (forwordStack.Count == 0) return;

                backStack.Push(_engine.ExportXml());
                xTask = forwordStack.Pop();
            }

            commonBus.IsNeedSave = true;

            // 1.获取最新模块的ID
            var curID = GetCurrent().ID;

            // 解析新的引擎|复原状态
            _engine.LoadProjfile(xTask);

            // 记录原来的Current
            foreach (var item in _engine)
            {
                item.IsCurrent = item.ID == curID;
            }

            // 触发引擎事件
            Bus.GetEvent<TaskChangedEvent>().Publish();
        }
        #endregion 前进和后退命令

        private void Remain10Stack(Stack<XElement> xStack)
        {
            if (xStack.Count > 10)
            {
                List<XElement> xElements = new List<XElement>();
                while (xStack.Count > 0)
                {
                    xElements.Insert(0, xStack.Pop());
                }

                // 删除最后一个
                xElements.RemoveAt(0);

                foreach (var item in xElements)
                {
                    xStack.Push(item);
                }
            }
        }

        /// <summary>
        /// 在 新增、修改、删除、移动、模块提取、复制粘贴模块后触发该事件
        /// </summary>
        private void OnRecipeChanged(bool isNeedSave = true)
        {
            // 只保留10次记录,超过10次，则给最远的一次删除掉
            Remain10Stack(backStack);

            if (isNeedSave)
            {
                backStack.Push(_engine.ExportXml());

                // 通知备份
                Bus.GetEvent<RecipeChangedEvent>().Publish(commonBus.CurrentRecipe);
            }

            // 程序是否需要保存
            commonBus.IsNeedSave = isNeedSave;
        }

        /// <summary>
        /// 获取
        /// </summary>
        public List<LNode> GetModuleAxis(IStation station)
        {
            var root = GetModule(station.OwnerID);

            List<LNode> nodes = new List<LNode>();

            BuildAxisNodes(root, nodes);

            return nodes;
        }

        /// <summary>
        /// 构建轴节点
        /// </summary>
        /// <param name="module"></param>
        /// <param name="node"></param>
        private void BuildAxisNodes(IMotionModule module, List<LNode> nodes)
        {
            foreach (var item in module.Children)
            {
                var anyAxis = item.Parameters.
                    FirstOrDefault(u => u.Value.Type == typeof(VAxisDevice) || u.Value.Type == typeof(VAxisMDevice) || u.Value.Type == typeof(VAxisPos));
                if (anyAxis.Value != null)
                {
                    LNode newNode = new LNode()
                    {
                        Tag = anyAxis.Value,
                        Text = item.Alias,
                        Key = anyAxis.Key,
                        Tag1 = anyAxis.Value.Value,
                    };

                    nodes.Add(newNode);
                }

                BuildAxisNodes(item, nodes);
            }
        }

        public List<IMotionModule> GetRefGlobalModules(ParameterAttribute gParameter)
        {
            List<IMotionModule> refList = new List<IMotionModule>();

            var refs = gParameter.Owner.GetByReferences();
            foreach (var item in refs)
            {
                if (item.ByRefName == gParameter.Name)
                {
                    var modules = gParameter.Owner.TaskModules;
                    if (modules.Contains(item.RefID))
                    {
                        refList.Add(modules[item.RefID] as IMotionModule);
                    }
                }
            }

            return refList;
        }

        #region 模块引用提取
        public List<IMotionModule> GetCompositions()
        {
            return _engine.GetCompositions();
        }

        public void OnRemoveCompositions(IMotionModule m)
        {
            // 遍历该对象是否有被项目引用
            foreach (var item in _engine)
            {
                if (item.TaskFunction is IRefComposition refC)
                {
                    if (refC.GetRefModule() == m)
                    {
                        throw new FriendlyException($"模组被:{item.Alias}引用,不能删除!");
                    }
                }
            }

            _engine.RemoveComposition(m.ID);
            commonBus.IsNeedSave = true;

            // 4.自动切换到Swith模块中
            Bus.GetEvent<ModuleCompositionEvent>().Publish(m);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extractType"></param>
        /// <param name="label">提取到的步骤名称</param>
        /// <param name="modules"></param>
        public void OnAddCompostion(string label, params IMotionModule[] modules)
        {
            if (modules.Length == 0)
            {
                throw new FriendlyException("要进行提取的模块为空");
            }

            // 创建组合模块
            IMotionModule newModule = _engine.CreateByName("Logic", "Group");
            newModule.Alias = label;

            // 使用复制粘贴逻辑，但确保只复制一次
            XElement xCopy = new XElement("XCopy");
            foreach (var item in modules)
            {
                xCopy.Add(item.ExportXml());
            }

            // 转换为字符串并替换GUID
            string strXML = xCopy.ToString();
            ReplaceXML(xCopy, ref strXML);

            // 解析为新的模块
            var newXElement = XElement.Parse(strXML);

            newModule.Children.Clear();

            // 逐个添加模块，确保不重复
            foreach (var xModule in newXElement.Elements("Module"))
            {
                var copiedModule = _engine.Parse(xModule, newModule, -1);

                // 检查是否已经存在相同ID的模块
                if (newModule.Children.All(child => child.ID != copiedModule.ID))
                {
                    newModule.Children.Add(copiedModule);
                }
            }
            // 排序新组合中的模块
            SortModule(newModule);

            // 添加到引用模块集合中
            _engine.AddComposition(newModule);

            // 触发组合事件
            Bus.GetEvent<ModuleCompositionEvent>().Publish(newModule);

            // 设置需要保存标志
            commonBus.IsNeedSave = true;
        }

        /// <summary>
        /// 多视图查看
        /// </summary>
        /// <param name="selectModules"></param>
        internal void OnCompareLook(List<IMotionModule> selectModules)
        {
            Bus.GetEvent<CompareLookEvent>().Publish(selectModules.ToArray());
        }

        #endregion
    }

    /// <summary>
    /// 动作排序
    /// </summary>
    class StationCompare : IComparer<IMotionModule>
    {
        public int Compare(IMotionModule x, IMotionModule y)
        {
            if (x.TaskFunction is IStation xStation && y.TaskFunction is IStation yStation)
            {
                // 列相同
                if (xStation.Column == yStation.Column)
                {
                    if (xStation.Row < yStation.Row)
                    {
                        return -1;

                    }
                    else if (xStation.Row > yStation.Row)
                    {
                        return 1;
                    }
                }
                else if (xStation.Row == yStation.Row)
                {
                    if (xStation.Column < yStation.Column)
                    {
                        return -1;

                    }
                    else if (xStation.Column > yStation.Column)
                    {
                        return 1;
                    }
                }
                else if (xStation.Column < yStation.Column)
                {
                    return -1;
                }
                else if (xStation.Column > yStation.Column)
                {
                    return 1;
                }
            }

            // 两个对象相等
            return 0;
        }
    }
}