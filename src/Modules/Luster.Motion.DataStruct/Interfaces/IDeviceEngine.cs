#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IVDManager
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.VDeviceManger
* 文 件 名:       IVDManager.cs
* 创建时间:       2022/4/6 10:13:35
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      65c47c39-3a5b-40b0-b1f4-4a4205456bc3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/6 10:13:35
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 设备引擎
    /// </summary>
    public interface IDeviceEngine : IDisposable
    {

        /// <summary>
        /// 是否设置CT耗时中各个模块的标准CT时间
        /// </summary>
        bool AutoSetMotionModuleCT { get; set; }
        /// <summary>
        /// 是否需要保存
        /// </summary>
        bool IsNeedSave { get; set; }

        /// <summary>
        /// 当前设备运行模式
        /// </summary>
        DeviceMode DeviceMode { get; set; }

        /// <summary>
        /// 属性变更事件
        /// </summary>
        event Action<string, object, object> PropertyChangedEvent;

        /// <summary>
        /// 控制参数发生变化
        /// </summary>
        event Action<Guid, string, string> ControlValueChangedEvent;

        /// <summary>
        /// 日志事件
        /// </summary>
        event Action<LogType, string> LogEvent;

        /// <summary>
        /// Loading事件
        /// </summary>
        event Action<int, int, string> LoadingEvent;

        /// <summary>
        /// 设备异常报警
        /// </summary>
        event Action<AlarmInfo> AlarmEvent_device;

        /// <summary>
        /// 删除前进行参数校验，防止被引用
        /// </summary>
        event Func<IVirtualDevice, string> PrevDeleteEvent;

        /// <summary>
        /// 报警号发生修改事件
        /// </summary>
        event Action<string, string> AlarmCodeChangedEvent;

        /// <summary>
        /// 触发报警号修改事件
        /// </summary>
        void RaiseAlarmCodeChangedEvent(string oldCode, string newCode);

        /// <summary>
        /// 轴点位删除，进行校验防止被引用
        /// </summary>
        event Func<AxisPosition, string> AxisPosDeleteEvent;

        /// <summary>
        /// 设备名称发生变更
        /// </summary>
        event Action<Guid, string, string> DeviceNameChangedEvent;

        /// <summary>
        /// 设备发生变更
        /// </summary>
        event Action VDeviceChangedEvent;

        /// <summary>
        /// 全局保存事件，在DeviceEngine.Save()时触发
        /// </summary>
        event Action SaveEvent;

        /// <summary>
        /// 触发设备发生变更事件
        /// </summary>
        void RaiseVDeviceChangedEvent();

        /// <summary>
        /// 模式变更
        /// </summary>
        event Action<DeviceMode> ModeChangedEvent;

        /// <summary>
        /// 模式变更之前事件
        /// </summary>
        event Action<bool> PrevModeChangeEvent;

        /// <summary>
        /// 初始化完成
        /// </summary>
        event Action<IDeviceEngine, string> InitializedEvent;

        /// <summary>
        /// 获取机台状态事件
        /// </summary>
        event Action<EngineStatus, EngineStatus> StatusChangedEvent;

        /// <summary>
        /// 获取机台状态
        /// </summary>
        event Func<EngineStatus> GetMachineStatusEvent;

        /// <summary>
        /// 触发消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        void OnLog(LogType type, string message);

        /// <summary>
        /// 加载进度条件
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cur"></param>
        /// <param name="message"></param>
        void OnLoading(int count, int cur, string message);

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="mode"></param>
        bool SetEngineMode(DeviceMode mode, out string errMsg);

        /// <summary>
        /// 1、动态解析配置文件
        /// 2、动态加载硬件设备
        /// <Devices>
        ///     <Device ID="" Name=""></Device>
        ///     <VDevice ID="" Name=""></VDevice>
        /// </Devices>
        /// </summary>
        void Initialize(string deviceTask);

        /// <summary>
        /// 对现有硬件进行检查，确认连接是否正常
        /// </summary>
        /// <param name="msg">错误消息</param>
        /// <returns>是否都正常</returns>
        bool CheckHardware(out string msg);
        /// <summary>
        /// 获取所有的连接器
        /// </summary>
        /// <returns></returns>
        List<Type> GetAdapters();

        /// <summary>
        /// 通过类型获取设备厂商集合信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<KeyValue> GetBrands(Type type);

        /// <summary>
        /// 加载驱动
        /// </summary>
        void LoadDrivers(string loadPath = "");

        /// <summary>
        /// 获取所有虚拟设备类型
        /// </summary>
        /// <returns></returns>
        List<Type> GetVirtualTypes();

        /// <summary>
        /// 获取真实设备类型
        /// </summary>
        /// <returns></returns>
        List<Type> GetRealTypes();

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="saveTask"></param>
        void Save(string saveTask = "");

        /// <summary>
        /// 获取所有模块（模块来源通过IO表导入，如果没有只返回System）
        /// </summary>
        /// <returns></returns>
        List<string> GetModules();

        List<string> GetModulesUsed();

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="alarm"></param>
        void OnAlarm(AlarmInfo alarm);

        #region 真实设备增删改
        /// <summary>
        /// 新增设备
        /// </summary>
        /// <param name="device"></param>
        void AddDevice(IDevice device);

        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="deviceID"></param>
        void ReomoveDevice(Guid deviceID);

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDevice GetDeviceByID(Guid deviceID);

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDevice GetDeviceByName(string name);

        /// <summary>
        /// 查询真实设备列表
        /// </summary>
        /// <param name="virtualType"></param>
        /// <returns></returns>
        List<IDevice> GetRealDevices(Type virtualType = null);
        #endregion

        #region 虚拟设备增删改
        /// <summary>
        /// 新增设备
        /// </summary>
        /// <param name="device"></param>
        void AddVirtual(IVirtualDevice virDevice, bool setReal = true);

        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="deviceID"></param>
        void ReomoveVirtual(params Guid[] deviceID);

        /// <summary>
        /// 通过类型移除设备
        /// </summary>
        /// <param name="deviceID"></param>
        void ReomoveVirtual(Type vType);

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IVirtualDevice GetVirtualByID(Guid id);

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IVirtualDevice GetVirtualByName(string name);

        /// <summary>
        /// 获取虚拟设备列表
        /// </summary>
        /// <param name="virtualType"></param>
        /// <returns></returns>
        List<IVirtualDevice> GetDevices(Type virtualType = null, string module = "");

        /// <summary>
        /// 获取虚拟设备列表
        /// </summary>
        /// <param name="virtualType"></param>
        /// <returns></returns>
        List<T> GetVDevices<T>(string module = null);


        /// <summary>
        /// 获取控制参数
        /// </summary>
        /// <returns></returns>
        List<XElement> LoadControlPara();


        #endregion

        #region 设备控制
        /// <summary>
        /// 对所有设备进行回零,设备耗时严重，需要异步线程调用
        /// </summary>
        /// <param name="errMsg">异常信息</param>
        /// <returns>回零成功与否</returns>
        bool Home(out string errMsg);

        /// <summary>
        /// 停止
        /// </summary>
        void Stop();

        /// <summary>
        /// 是否在回零状态
        /// </summary>
        /// <param name="errMsg">没有回零的原因</param>
        /// <returns></returns>
        bool IsHome(out string errMsg);

        /// <summary>
        /// 警告后，确认是否还继续做动作，用在安全位检查上面
        /// </summary>
        /// <returns></returns>
        bool IsBreakAction { get; set; }

        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();

        /// <summary>
        /// 复位
        /// </summary>
        bool Recovery();
        #endregion

        #region 设备监听
        /// <summary>
        /// 开始设备监听
        /// </summary>
        void StartDeviceMonitor();

        /// <summary>
        /// 停止监听
        /// </summary>
        void StopDeviceMonitor();
        #endregion

        #region 点位删除
        void RemoveAxisPos(AxisPosition axisPos);

        /// <summary>
        /// 获取轴的点位
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        AxisPosition GetAxisPosition(Guid key);

        /// <summary>
        /// 更新点位
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="newV"></param>
        void UpdatePostion(AxisPosition pos, double newV);
        #endregion

        #region 多点位示教组
        /// <summary>
        /// 点位集合
        /// </summary>
        List<VAxisPosGroup> PosGroup { get; set; }

        /// <summary>
        /// 示教点位
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vAxis"></param>
        void TeachPosGroup(string name, string module, params VAxis[] vAxis);

        /// <summary>
        /// 删除点位
        /// </summary>
        /// <param name="name">删除指定示教位</param>
        void RemovePosGroup(string name);

        /// <summary>
        /// 更新轴点位
        /// </summary>
        /// <param name="pGroup"></param>
        /// <param name="aType"></param>
        /// <param name="position"></param>
        void UpdatePosGroup(VAxisPosGroup pGroup, AxisType aType, double position);
        void UpdatePosGroup(AxisPosition pPos, double position);

        /// <summary>
        /// 保存点位配置
        /// </summary>
        void SavePosGroup(XElement xParent);

        /// <summary>
        /// 加载点位配置
        /// </summary>
        void LoadPosGroup(XElement xPosGroup);
        #endregion

        #region 维保
        /// <summary>
        /// 触发清零的事件
        /// </summary>
        event Action<IMaintain> MaintainZeroEvent;

        /// <summary>
        /// 触发方法
        /// </summary>
        void MaintainZero(IMaintain maintain);
        #endregion

        #region 页面权限
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="role"></param>
        void OnUserLogin(SystemRole role);

        /// <summary>
        /// 权限变更
        /// </summary>
        event Action<SystemRole> RoleChangedEvent;

        /// <summary>
        /// 获取当前机台状态
        /// </summary>
        /// <returns></returns>
        void OnStatusChanged(EngineStatus src, EngineStatus dst);

        /// <summary>
        /// 获取机台当前状态
        /// </summary>
        /// <returns></returns>
        EngineStatus GetMachineStatus();
        #endregion

        /// <summary>
        /// 配方控制参数路径
        /// </summary>
        string RecipeDataPath { get; set; }

        /// <summary>
        /// 配方解决方案目录
        /// </summary>
        string RecipeSlnPath { get; set; }
        /// <summary>
        /// 配方Config路径
        /// </summary>
        string RecipeConfigPath { get; set; }


        /// <summary>
        /// 更新控制参数
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        void UpdateControlPara(Guid guid, string Name, string value);



        /// <summary>
        /// 模块配置路径
        /// </summary>
        string ModuleConfigPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<Type, object> GetDevice_Type();

        public event Func<string, string, List<object>> GetModuleListEvent;
        public List<object> GetModulesFromMotionEngine(string guid = null, string type = null);

        public event Func<IEnumerable<object>> GetPDCAModulesEvent;
        public IEnumerable<object> GetPDCAModulesFromMotionEngine();
        public event Func<IEnumerable<object>> GetSFCModulesEvent;
        public IEnumerable<object> GetSFCModulesFromMotionEngine();


        #region 模块信息组
        /// <summary>
        /// 点位集合
        /// </summary>
        List<ModuleNameModel> ModuleNameGroup { get; set; }

        /// <summary>
        /// 添加模组
        /// </summary>
        /// <param name="name"></param>
        void AddModuleNameGroup(string name);

        /// <summary>
        /// 删除模组
        /// </summary>
        /// <param name="name"></param>
        void RemoveModuleNameGroup(string name);

        /// <summary>
        /// 保存模组配置
        /// </summary>
        void SaveModuleNameGroup(XElement xParent);

        /// <summary>
        /// 加载模组配置
        /// </summary>
        void LoadModuleNameGroup(XElement xModuleGroup);
        #endregion
    }
}
