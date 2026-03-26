#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IMotionManager
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Manager
* 文 件 名:       IMotionManager.cs
* 创建时间:       2022/5/18 11:26:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a37203cf-e0ee-4b4f-ad83-30de2ff7eb82
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 11:26:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Events;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine
{
    /// <summary>
    /// 运动控制器
    /// </summary>
    public interface IMotionEngine : IList<IMotionModule>, IXMLParser, ICloneable
    {
        #region 属性  

        /// <summary>
        /// 解析算子的工厂
        /// </summary>
        IModuleFactory Factory { get; set; }

        /// <summary>
        /// 运行的编号
        /// </summary>
        string ExcutorNo { get; set; }

        /// <summary>
        /// 引擎状态信息
        /// </summary>
        EngineStatus EngineStatus { get; set; }

        /// <summary>
        /// 设备引擎
        /// </summary>
        IDeviceEngine DeviceEngine { get; set; }

        /// <summary>
        /// 数据关联对象
        /// </summary>
        List<MapData> MapDatas { get; set; }

        /// <summary>
        /// 工作流
        /// </summary>
        WorkFlow WorkFlow { get; set; }

        /// <summary>
        /// 缓存
        /// </summary>
        ICacheManager CacheManager { get; set; }
        #endregion

        #region 事件
        /// <summary>
        /// 属性变更
        /// </summary>
        event Action<IMotionModule, string, object, object> PropertyChanged;

        /// <summary>
        /// 模块更新
        /// </summary>
        event Action<IMotionModule, ModuleUpdate> ModuleUpdateEvent;

        /// <summary>
        /// 报警事件
        /// <para>IMotionModule：报警模块</para>
        /// <para>AlarmType：报警类型</para>
        /// <para>string：报警消息</para>
        /// <para>string：报警代码</para>
        /// </summary>
        event Action<AlarmInfo> AlarmEvent_MEngine;

        /// <summary>
        /// 运行后事件
        /// </summary>
        event Action<PostRunEventArgs> PostRunEvent;

        /// <summary>
        /// Log时间
        /// </summary>
        event Action<LogType, string, string> LogEvent;

        /// <summary>
        /// 多语言
        /// </summary>
        event Func<string, string> LanguageEvent;

        /// <summary>
        /// 产品入料
        /// <para>参数是 模块名称</para>
        /// </summary>
        event Action<IMotionModule, StationResult> ProLoadedEvent;

        /// <summary>
        /// 产品出料事件
        /// </summary>
        event Action<IMotionModule, StationResult, Dictionary<string, object>,Action<IMotionModule, StationResult, Dictionary<string, object>,double>> ProUnloadedEvent;

        /// <summary>
        /// 产品出料事件DB
        /// </summary>
        event Action<IMotionModule, StationResult, Dictionary<string, object>,double> ProUnloadedDBEvent;

        /// <summary>
        /// 产品抛料事件
        /// </summary>
        event Action<StationResult, string, string> ProThrowEvent;

        /// <summary>
        /// 回零失败
        /// </summary>
        event Action<string> HomeFailEvent;

        /// <summary>
        /// 运行加载
        /// </summary>
        event Action<int, int, string> LoadingEvent;

        /// <summary>
        /// 时间统计事件
        /// </summary>
        event Action<List<StationTime>, bool> TimeStatisEvent;
        /// <summary>
        /// Vision步序事件
        /// </summary>
        event Action<List<StationTime>, bool> VisionEvent;

        /// <summary>
        /// 引擎状态变化 用于定制首页显示运行状态
        /// </summary>
        event Func<EngineStatus, EngineStatus, bool> EngineStatusEvent;

        /// <summary>
        /// 模块变化引起输出项配置变化
        /// </summary>
        event Action MapDataChangeEvent;


        event Func<string, InteractiveType> DialogBoxEvent;

        //复位失败
         event Action ResetFailEvent;


        //工站结束时间事件
        public event Action<string, string, string, bool> StationTimeEvent;


        #endregion

        #region 运行模块

        /// <summary>
        /// 执行单个模块
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool RunOne(Guid id);

        /// <summary>
        /// 执行某个模块
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Run(Guid id);

        /// <summary>
        /// 调试流程
        /// </summary>
        /// <returns>是否执行成功</returns>
        void RunAll();


        /// <summary>
        /// 单步运行停止
        /// </summary>
        void RunStop();
        #endregion

        #region 控制
        /// <summary>
        /// 工站运行
        /// </summary>
        /// <param name="isOne"></param>
        void RunStations();

        /// <summary>
        /// 启动工站
        /// </summary>
        void RunStartStation();

        /// <summary>
        /// 后台工站
        /// </summary>
        void RunBackGroundStation();
        /// <summary>
        /// 暂停
        /// </summary>
        void Pause(bool isAlarm = false, Action<bool, Exception> callback = null);

        /// <summary>
        /// 恢复运行
        /// </summary>
        void Recovery(Action<bool> action = null);

        /// <summary>
        /// 流程复位
        /// </summary>
        void Reset(AlarmInfo alarmInfo, Action<bool> callback = null);

        /// <summary>
        /// 停止
        /// </summary>
        void Stop(Action callback = null);

        /// <summary>
        /// 回零
        /// </summary>
        void Home(Action callback = null, HomeType homeType = HomeType.Home);
        #endregion

        #region 算子管理

        /// <summary>
        /// 添加新的算子
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="index">模块位置</param>
        void Insert(IMotionModule module, int index = -1, IMotionModule pModule = null);

        /// <summary>
        /// 通过模块名称和对应的函数名称来创建
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="funcName">函数名称</param>
        /// <returns>模块</returns>
        IMotionModule CreateByName(string moduleName, string funcName);

        /// <summary>
        /// 通过ID删除模块
        /// </summary>
        /// <param name="mID"></param>
        void RemoveByID(Guid mID);
        #endregion

        #region 通用方法

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id">guid</param>
        /// <returns></returns>
        IMotionModule Get(Guid id);

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();

        /// <summary>
        /// 获取状态在Running的模块
        /// </summary>
        /// <returns>当前运行的模块</returns>
        IMotionModule GetBusyModule();

        /// <summary>
        /// 解析子节点
        /// </summary>
        /// <param name="xModule"></param>
        /// <param name="parent"></param>
        /// <param name="index"></param>
        IMotionModule Parse(XElement xModule, IMotionModule parent, int index = -1);

        /// <summary>
        /// 加载工程文件
        /// </summary>
        /// <param name="projfile">工程文件路径，后缀是*.3dproj</param>
        void LoadProjfile(string projfile);

        /// <summary>
        /// 导出配置
        /// </summary>
        /// <returns></returns>
        XElement ExportConfig();

        /// <summary>
        /// 解析XML配置
        /// </summary>
        /// <param name="xElement"></param>
        void ParseConfig(XElement xElement);

        /// <summary>
        /// 加载项目文件
        /// </summary>
        /// <param name="xProj"></param>
        void LoadProjfile(XElement xProj);

        /// <summary>
        /// 获取工站模块
        /// </summary>
        List<IMotionModule> GetStations();

        /// <summary>
        /// 设置模块运行状态
        /// </summary>
        /// <param name="motion"></param>
        void SetDefaultStatus(RunStatus status);

        /// <summary>
        /// 报警
        /// </summary>
        /// <param name="type">报警类型</param>
        /// <param name="msg">报警消息</param>
        /// <param name="alarmCode">报警代码</param>
        void OnAlarm(AlarmInfo alarmInfo);

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="logThreadID"></param>
        void OnLog(LogType type, string message, string logThreadID = "");
        #endregion

        #region 数据关联
        void GetOwnerStation(IMotionModule module, ref IProStation station);
        #endregion

        #region 模块提取到组合步骤中
        /// <summary>
        /// 添加或更新
        /// </summary>
        void AddComposition(IMotionModule groupModule);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid"></param>
        void RemoveComposition(Guid guid);

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        List<IMotionModule> GetCompositions();
        #endregion

        /// <summary>
        /// 等待运行
        /// </summary>
        /// <param name="waitMessage">等待的消息</param>
        void WaitRunning(string waitMessage);


        void BuildPrimModule();


        // 太科电批注册事件
        public event Action<object, string> TaiKeScrewRegisterEvent;

        // 压力注册事件
        public event Action<object, string> PressRegisterEvent;

        // Cowling压力注册事件
        public event Action<object, string> ToeinForceRegisterEvent;

    }
}