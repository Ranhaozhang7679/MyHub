using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 工作流
    /// </summary>
    public class WorkFlow : List<WorkItem>, IXMLParser
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _mEngine;

        /// <summary>
        /// 产品状态变更
        /// <para>1.名称</para>
        /// <para>2.sn</para>
        /// <para>3.ct</para>
        /// <para>4.1是入 0是出</para>
        /// <para>5.1 是否自身触发</para>
        /// </summary>
        public event Action<string, string, double, int, bool> ProStatusChangedEvent;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mEngine"></param>
        public WorkFlow(IMotionEngine mEngine)
        {
            _mEngine = mEngine;
        }

        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("WorkFlow");

            foreach (var item in this)
            {
                xRoot.Add(item.ExportXml());
            }

            return xRoot;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            // 清除历史对象
            Clear();

            // 解析对象
            foreach (var xItem in xElement.Elements())
            {
                WorkItem workItem = new();
                workItem.MotionEngine = _mEngine;
                workItem.StatusChanged -= WorkItem_StatusChanged;
                workItem.StatusChanged += WorkItem_StatusChanged;
                workItem.ParserXml(xItem);
                if (workItem.StartItem != null)
                    this.Add(workItem);
            }

            // 解析依赖
            foreach (var item in this)
            {
                if (item.PrevIds.Count > 0)
                {
                    foreach (var prevId in item.PrevIds)
                    {
                        var xPrev = this.FirstOrDefault(u => u.ID == prevId);
                        if (xPrev != null)
                        {
                            item.PrevItems.Add(xPrev);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 工作项目状态时间变更
        /// </summary>
        /// <param name="module"></param>
        /// <param name="isStart"></param>
        /// <param name="dst"></param>
        private void WorkItem_StatusChanged(IMotionModule module, string name, bool isStart, RunStatus dst, bool isSelf = true)
        {
            ProStatusChangedEvent?.Invoke(name, module.DataID, module.TimeConsuming, isStart ? 1 : 0, isSelf);
        }
        #endregion

        /// <summary>
        /// 新增对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startModule"></param>
        /// <param name="endModule"></param>
        /// <exception cref="FriendlyException"></exception>
        public void AddNewItem(string name, IMotionModule startModule, IMotionModule endModule = null)
        {
            var newItem = new WorkItem()
            {
                StartItem = startModule,
                EndItem = endModule,
                Name = name,
            };

            AddItem(newItem);
        }

        public void AddItem(WorkItem wItem)
        {
            if (this.Any(u => u.Name == wItem.Name))
            {
                throw new FriendlyException($"工艺名称:{wItem.Name}已重复!");
            }

            wItem.RegisterEvent();

            if (this.Count > 0)
            {
                var maxRow = this.Max(u => u.Row);
                wItem.Row = maxRow + 1;
            }

            Add(wItem);
        }

        /// <summary>
        /// 回零
        /// </summary>
        public void Home()
        {
            foreach (var item in this)
            {
                item.Status = RunStatus.Default;
            }
        }
    }

    /// <summary>
    /// 工艺对象
    /// </summary>
    public class WorkItem : IXMLParser
    {
        /// <summary>
        /// 状态变更
        /// </summary>
        public event Action<IMotionModule, string, bool, RunStatus, bool> StatusChanged;

        /// <summary>
        /// 模块ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Ignore]
        public string Icon
        {
            get
            {
                return StartItem.TaskFunction?.Icon;
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        public string SN { get; set; } = "";

        /// <summary>
        /// 是否选择
        /// </summary>
        [Ignore]
        public bool IsSelected { get; set; }

        /// <summary>
        /// 行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Ignore]
        public RunStatus Status { get; set; }

        /// <summary>
        /// 工作开始
        /// </summary>
        [Ignore]
        public IMotionModule StartItem { get; set; }

        /// <summary>
        /// 工作结束
        /// </summary>
        [Ignore]
        public IMotionModule EndItem { get; set; }

        /// <summary>
        /// 前一站
        /// </summary>
        [Ignore]
        public List<WorkItem> PrevItems { get; }

        /// <summary>
        /// 对应的ID
        /// </summary>
        [Ignore]
        public List<Guid> PrevIds { get; set; }

        /// <summary>
        /// 引起对象
        /// </summary>
        [Ignore]
        public IMotionEngine MotionEngine { get; set; }

        /// <summary>
        /// 函数
        /// </summary>
        public WorkItem()
        {
            // 上一个Items
            ID = Guid.NewGuid();
            PrevItems = new List<WorkItem>();
            PrevIds = new List<Guid>();
        }

        /// <summary>
        /// 添加引用
        /// </summary>
        /// <param name="prev"></param>
        public void AddPrev(WorkItem prev)
        {
            if (!PrevItems.Any(u => u.ID == prev.ID))
            {
                PrevItems.Add(prev);
            }
        }

        /// <summary>
        /// 移除前置
        /// </summary>
        /// <param name="prev"></param>
        public void RemovePrev(WorkItem prev)
        {
            if (PrevItems.Any(u => u.ID == prev.ID))
            {
                PrevItems.Remove(prev);
            }
        }

        /// <summary>
        /// 获取耗时
        /// </summary>
        /// <returns></returns>
        public float GetTimeConsuming()
        {
            if (EndItem != null)
            {
                return (EndItem.EndTime - StartItem.StartTime).Milliseconds;
            }
            else
            {
                return StartItem.TimeConsuming;
            }
        }

        /// <summary>
        /// 事件注册
        /// </summary>
        public void RegisterEvent()
        {
            if (StartItem != null)
            {
                StartItem.PropertyChangedEvent -= StartItem_PropertyChangedEvent;
                StartItem.PropertyChangedEvent += StartItem_PropertyChangedEvent;

                RegisterStation(StartItem);
            }

            if (EndItem != null)
            {
                EndItem.PropertyChangedEvent -= EndItem_PropertyChangedEvent;
                EndItem.PropertyChangedEvent += EndItem_PropertyChangedEvent;
                RegisterStation(EndItem);
            }
        }

        /// <summary>
        /// 工站注册
        /// </summary>
        /// <param name="mItem"></param>
        private void RegisterStation(IMotionModule mItem)
        {
            if (mItem.Station == null)
            {
                mItem.UpdateStation();
            }

            mItem.Station.PropertyChangedEvent -= Station_PropertyChangedEvent;
            mItem.Station.PropertyChangedEvent += Station_PropertyChangedEvent;
        }

        /// <summary>
        /// 工站注册
        /// </summary>
        /// <param name="module"></param>
        /// <param name="propName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        private void Station_PropertyChangedEvent(IModule module, string propName, object srcV, object newV)
        {
            if (propName == "DataID")
            {
                SN = module.DataID;
            }
        }

        /// <summary>
        /// 终止的Item属性
        /// </summary>
        /// <param name="module"></param>
        /// <param name="propName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        private void EndItem_PropertyChangedEvent(Luster.TaskFlow.Common.Module.IModule module, string propName, object srcV, object newV)
        {
            if (propName == nameof(Status))
            {
                var newStatus = newV.To<RunStatus>();

                // 存在结束的场景下,只有成功和移除才更新状态
                if (newStatus == RunStatus.Success || newStatus == RunStatus.Error)
                {
                    SetStatus(module as IMotionModule, newStatus);
                }
            }
        }

        private void StartItem_PropertyChangedEvent(Luster.TaskFlow.Common.Module.IModule module, string propName, object srcV, object newV)
        {
            if (propName == nameof(Status))
            {
                var newStatus = newV.To<RunStatus>();

                if (EndItem == null)
                {
                    // 如果没有根节点的情况,则workItem状态和模块状态保持一致
                    SetStatus(module, newStatus);
                }
                else
                {
                    // 如果存在结束节点
                    if (newStatus == RunStatus.Running || newStatus == RunStatus.Error)
                    {
                        SetStatus(module, newStatus);

                        // 前已状态状态变为运行中
                        if (newStatus == RunStatus.Running)
                        {
                            foreach (var item in PrevItems)
                            {
                                item.SetStatus(module, RunStatus.Default, false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 配置状态
        /// </summary>
        /// <param name="rStatus"></param>
        public void SetStatus(IModule module, RunStatus rStatus = RunStatus.Default, bool isOnEvent = true)
        {
            var srv = Status;
            Status = rStatus;

            // 实时更新
            StatusChanged?.Invoke(module as IMotionModule, Name, module == StartItem, rStatus, isOnEvent);
        }

        #region 导入和导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public XElement ExportXml()
        {
            var xml = this.ToXml();
            if (StartItem != null)
            {
                xml.SetAttributeValue("Start", StartItem.ID);
            }

            if (EndItem != null)
            {
                xml.SetAttributeValue("End", EndItem.ID);
            }

            // 前一站
            if (PrevItems != null && PrevItems.Count > 0)
            {
                foreach (var item in PrevItems)
                {
                    XElement refElement = new XElement("PrevItem");
                    refElement.SetAttributeValue("RefID", item.ID);
                    xml.Add(refElement);
                }
            }

            return xml;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);

            // 设置起始点
            xElement.GetAttribute("Start", id =>
            {
                var mID = Guid.Parse(id);
                StartItem = MotionEngine.Get(mID);
            });

            // 设置结束点
            xElement.GetAttribute("End", id =>
            {
                var mID = Guid.Parse(id);
                EndItem = MotionEngine.Get(mID);
            });

            // 设置Prev
            PrevIds = new List<Guid>();
            foreach (var xPrev in xElement.Elements("PrevItem"))
            {
                var xRefId = xPrev.Attribute("RefID").Value;
                PrevIds.Add(Guid.Parse(xRefId));
            }

            RegisterEvent();
        }
        #endregion
    }
}
