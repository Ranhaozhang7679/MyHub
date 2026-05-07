#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IStation
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.interfaces
* 文 件 名:       IStation.cs
* 创建时间:       2022/5/30 8:50:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4207e3ac-9ec6-416f-ba40-d4a96b104c95
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 8:50:30
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Virtual;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Motion.Logic
{
    public interface IStation
    {
        #region UI相关
        /// <summary>
        /// 所处行
        /// </summary>
        int Row { get; set; }

        /// <summary>
        /// 所处列
        /// </summary>
        int Column { get; set; }
        #endregion

        /// <summary>
        /// 工站名称
        /// </summary>
        string Station { get; }

        /// <summary>
        /// 首页是否可见
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        int Sort { get; }

        /// <summary>
        /// 对应的ID
        /// </summary>
        Guid OwnerID { get; }

        /// <summary>
        /// 上游Station
        /// </summary>
        List<IStation> PrevStations { get; set; }

        /// <summary>
        /// 当前工作的最终结果
        /// </summary>
        StationResult Result { get; set; }

        /// <summary>
        /// 工站耗时统计
        /// </summary>
        List<StationTime> StationTimes { get; set; }

        /// <summary>
        /// 工站时间事件
        /// </summary>
        event Action<List<StationTime>, bool> StationTimeEvent;

        /// <summary>
        /// 使用Log
        /// </summary>
        bool UseLog { get; set; }

        /// <summary>
        /// 工站对象
        /// </summary>
        /// <returns></returns>
        List<LColumn> Datas { get; set; }

        /// <summary>
        /// 出队
        /// </summary>
        /// <returns></returns>
        bool TryDequeue(out string dataID);

        /// <summary>
        /// 工站支持缓存队列
        /// </summary>
        /// <param name="dataID"></param>
        void Enqueue(string dataID);

        /// <summary>
        /// 显示最外的一个队列
        /// </summary>
        /// <returns></returns>
        bool TryPeek(out string dataID);

        /// <summary>
        /// 清理数据
        /// </summary>
        void ClearDatas();

        /// <summary>
        /// 新增运行中的模块
        /// </summary>
        /// <param name="module"></param>
        void AddRunningModule(IMotionModule module);

        /// <summary>
        /// 获取运行中的模块
        /// </summary>
        /// <returns></returns>
        IMotionModule GetRunningModule();

        /// <summary>
        /// 运行模块变更事件
        /// </summary>
        event Action<IMotionModule> RunningModuleChanged;
    }



    /// <summary>
    /// 自由工站
    /// </summary>
    public interface IFreeStation : IStation
    {
        bool IsEnabled { get; set; }

        /// <summary>
        /// 获取模块是否可以使用
        /// </summary>
        /// <returns></returns>
        bool GetIsEnabled();

        public String IsReturn { get; set; }

        public bool IsReturnEnabled { get; set; }
    }

    /// <summary>
    /// 工站模块,做数据传递
    /// </summary>
    public interface IProStation : IStation
    {
        /// <summary>
        /// 检查上一站是否有料
        /// </summary>
        bool CheckPrevHaved(out IProStation havedStation);
    }

    /// <summary>
    /// 回零站
    /// </summary>
    public interface IHomeStation : IStation
    {
        /// <summary>
        /// 回零进度
        /// </summary>
        event ProgressDelegateHandler HomeProgressEvent;

        /// <summary>
        /// 超时时间
        /// </summary>
        int Overtime { get; set; }

        /// <summary>
        /// 属于类型
        /// </summary>
        HomeType HomeType { get; }
    }

    /// <summary>
    /// 复位工站
    /// </summary>
    public interface IResetStation : IHomeStation
    {

    }

    /// <summary>
    /// 空跑工站
    /// </summary>
    public interface IEmptyStation : IStation
    {

    }

    /// <summary>
    /// 调试工站
    /// </summary>
    public interface ITestStation : IStation
    {

    }

    /// <summary>
    /// 停止工站
    /// </summary>
    public interface INGStation : IStation
    {
        EngineStatus MachineStatus { get; set; }

        /// <summary>
        /// 更新当前报警
        /// </summary>
        /// <param name="alarmInfo"></param>
        void SetAlarm(params AlarmInfo[] alarmInfos);
    }
    /// <summary>
    /// 开始工站
    /// 每次点击启动时需要执行这个工站
    /// </summary>
    public interface IStartStation:IStation
    {

    }

    /// <summary>
    /// 后台工站
    /// 一直运行的工站
    /// </summary>
    public interface IBackGroundStation : IStation
    {

    }

    /// <summary>
    /// 站
    /// </summary>
    public class StationFunction : MotionFunction, IStation, IGroup
    {
        /// <summary>
        /// 所处行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 所处行
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 隶属ID
        /// </summary>
        public Guid OwnerID => MyOwner.ID;

        /// <summary>
        /// 隶属工站
        /// </summary>
        public string Station => MyOwner.Alias;

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort => MyOwner.Sort;

        /// <summary>
        /// 首页是否可见
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 使用Log
        /// </summary>
        public bool UseLog { get; set; } = true;

        /// <summary>
        /// 工站中断
        /// </summary>
        public bool StationBreak { get; set; } = false;

        /// <summary>
        /// 当前工作的最终结果
        /// </summary>
        public StationResult Result { get; set; }

        /// <summary>
        /// 工站耗时统计
        /// </summary>
        public List<StationTime> StationTimes { get; set; }

        /// <summary>
        /// 工站时间事件
        /// </summary>
        public event Action<List<StationTime>,bool> StationTimeEvent;



        /// <summary>
        /// 有料
        /// </summary>
        //[Parameter("有料状态", 100, CN = "有料状态", ParamType = ParamType.OUT)]
        public virtual bool Haved { get; set; }

        /// <summary>
        /// 上一站
        /// </summary>
        public virtual List<IStation> PrevStations { get; set; }

        /// <summary>
        /// 当前工站最终数据
        /// </summary>
        public List<LColumn> Datas { get; set; }

        /// <summary>
        /// 运动引擎
        /// </summary>
        protected MotionRunEngine motionRunEngine;

        /// <summary>
        /// 数据队列
        /// </summary>
        private ConcurrentQueue<string> dataQueue;

        /// <summary>
        /// 构造函数
        /// </summary>
        public StationFunction()
        {
            this.Icon = "\xe694";
            motionRunEngine = new MotionRunEngine();
            PrevStations = new List<IStation>();
            StationTimes = new List<StationTime>();
            dataQueue = new();
            Datas = new();
        }

        #region 对象导入导出
        public override XElement ExportXml()
        {
            var xml = base.ExportXml();

            if (PrevStations != null && PrevStations.Count > 0)
            {
                XElement xStation = new XElement("Station");
                foreach (var item in PrevStations)
                {
                    xStation.Add(new XElement("Item", item.OwnerID));
                }

                xml.Add(xStation);
            }

            xml.SetAttributeValue("Row", Row);
            xml.SetAttributeValue("Column", Column);
            xml.SetAttributeValue("Visible", Visible);
            xml.SetAttributeValue("Module", Station);
            if (!UseLog)
            {
                xml.SetAttributeValue("UseLog", UseLog);
            }

            return xml;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xFunc"></param>
        public override void ParserXml(XElement xFunc)
        {
            base.ParserXml(xFunc);

            var xStation = xFunc.Element("Station");

            // 工站对象解析
            if (xStation != null)
            {
                foreach (var item in xStation.Elements("Item"))
                {
                    var id = Guid.Parse(item.Value);
                    if (MyOwner.TaskModules.Contains(id))
                    {
                        PrevStations.Add(MyOwner.TaskModules[id].TaskFunction as IStation);
                    }
                }
            }

            // 记录位置信息
            xFunc.GetAttribute("Row", rowStr =>
            {
                if (int.TryParse(rowStr, out var row))
                {
                    Row = row;
                }
            });


            xFunc.GetAttribute("Column", column =>
            {
                if (int.TryParse(column, out var col))
                {
                    Column = col;
                }
            });
            xFunc.GetAttribute("Visible", visible =>
            {
                if (bool.TryParse(visible, out var vis))
                {
                    Visible = vis;
                }
            });

            xFunc.GetAttribute("UseLog", useLog =>
            {
                if (bool.TryParse(useLog, out var uLog))
                {
                    UseLog = uLog;
                }
            });
        }
        #endregion

        /// <summary>
        /// 触发工站耗时统计
        /// </summary>
        protected void OnStationTime()
        {
            try
            {
                //在一个自由工站运行结束后再进行获取事件
                var cloneS = new List<StationTime>(StationTimes);
                // 事件
                StationTimeEvent?.Invoke(cloneS, UseLog);
                // 事件触发后，清理记录
                StationTimes.Clear();
            }
            catch(Exception ex)
            {
               MyOwner.OnLog(LogType.Info, string.Format("模块:{0} 报警:{1} ", this.Alias, ex.ToString()));
            }

        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <returns></returns>
        public bool TryDequeue(out string dataID)
        {
            return dataQueue.TryDequeue(out dataID);
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="dataID"></param>
        public void Enqueue(string dataID)
        {
            dataQueue.Enqueue(dataID);
        }

        /// <summary>
        /// 显示获取最外的一个数据ID
        /// </summary>
        /// <returns></returns>
        public bool TryPeek(out string dataID)
        {
            // 数据为空
            return dataQueue.TryPeek(out dataID);
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public virtual void ClearDatas()
        {
            while (dataQueue.TryDequeue(out var dataID))
            {
            }

            // 移除运行中的Module
            runningModule.TryDequeue(out var rModule);
        }

        #region 运行中的模块变更
        /// <summary>
        /// 运行模块变更事件
        /// </summary>
        public event Action<IMotionModule> RunningModuleChanged;

        /// <summary>
        /// 运行中的module
        /// </summary>
        private ConcurrentQueue<IMotionModule> runningModule = new ConcurrentQueue<IMotionModule>();

        /// <summary>
        /// 新增运行中的模块
        /// </summary>
        /// <param name="module"></param>
        public void AddRunningModule(IMotionModule module)
        {
            if (runningModule.TryDequeue(out var m))
            {
            }


            runningModule.Enqueue(module);

            // 模块发送变更
            RunningModuleChanged?.Invoke(module);
        }

        /// <summary>
        /// 获取运行中的模块
        /// </summary>
        /// <returns>运行中的Module</returns>
        public IMotionModule GetRunningModule()
        {
            IMotionModule rModule = null;
            runningModule.TryPeek(out rModule);
            return rModule;
        }
        #endregion
    }
}