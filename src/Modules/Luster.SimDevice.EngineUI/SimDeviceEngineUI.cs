#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SimDeviceEngineUI
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI
* 文 件 名:       SimDeviceEngineUI.cs
* 创建时间:       2022/4/18 9:15:47
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b061a8e0-30ab-4498-b968-436fecec19fc
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 9:15:47
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Events;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Ioc;
using Luster.SimDevice.EngineUI.Events;
using System.IO;
using Prism.Regions;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.DataStruct.Interfaces;
using Luster.SimDevice.Real;

namespace Luster.SimDevice.EngineUI
{
    /// <summary>
    /// 1.用于管理事件订阅
    /// 2.用于管理任务引擎
    /// 3.用于管理弹窗
    /// </summary>
    public class SimDeviceEngineUI : ISimDeviceEngineUI
    {
        /// <summary>
        /// 事件总线
        /// </summary>
        private IEventAggregator eventBus;

        /// <summary>
        /// 引擎
        /// </summary>
        public IDeviceEngine Engine { get; set; }

        /// <summary>
        /// Dialog 服务
        /// </summary>
        public IDialogService Dialog { get; set; }

        /// <summary>
        /// 记录当前的状态
        /// </summary>
        public DeviceMode DeviceMode => Engine.DeviceMode;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ea"></param>
        public SimDeviceEngineUI(IEventAggregator ea, IDeviceEngine engine, IDialogService dialogService)
        {
            eventBus = ea;
            Engine = engine;

            Engine.LogEvent -= Engine_LogEvent;
            Engine.LogEvent += Engine_LogEvent;

            Engine.LoadingEvent -= Engine_LoadingEvent;
            Engine.LoadingEvent += Engine_LoadingEvent;

            Engine.VDeviceChangedEvent -= Engine_VDeviceChangedEvent;
            Engine.VDeviceChangedEvent += Engine_VDeviceChangedEvent;

            Engine.RoleChangedEvent -= Engine_RoleChangedEvent;
            Engine.RoleChangedEvent += Engine_RoleChangedEvent;

            Engine.InitializedEvent -= Engine_InitializedEvent;
            Engine.InitializedEvent += Engine_InitializedEvent;

            Engine.StatusChangedEvent -= Engine_StatusChangedEvent;
            Engine.StatusChangedEvent += Engine_StatusChangedEvent;
            Dialog = dialogService;

            // 动态加载驱动
            Engine.LoadDrivers();
        }

        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="src">原始状态</param>
        /// <param name="dst">目标状态</param>
        private void Engine_StatusChangedEvent(EngineStatus src, EngineStatus dst)
        {
            // 页面启用
            eventBus.GetEvent<PageEnabledEvent>().Publish(dst != EngineStatus.Running);
        }

        private void Engine_InitializedEvent(IDeviceEngine arg1, string deviceTask)
        {
            eventBus.GetEvent<ProjOpenEvent>().Publish(new ProjectInfo()
            {
                SolutionPath = Path.GetDirectoryName(deviceTask),
                ProjectName = Path.GetFileName(deviceTask),
            });
        }

        /// <summary>
        /// 角色变更
        /// </summary>
        /// <param name="sysRole"></param>
        private void Engine_RoleChangedEvent(Common.DataStruct.SystemRole sysRole)
        {
            eventBus.GetEvent<RoleChangedEvent>().Publish(sysRole);
        }

        /// <summary>
        /// 设备发生了变更
        /// </summary>
        private void Engine_VDeviceChangedEvent()
        {
            eventBus.GetEvent<DeviceChangedEvent>().Publish();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Engine_LoadingEvent(int count, int val, string message)
        {
            eventBus.GetEvent<LoadingEvent>().Publish(new LoadingModel()
            {
                Value = val,
                MaxValue = count,
                LoadingMsg = message
            });
        }

        /// <summary>
        /// 注册Log事件
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Engine_LogEvent(LogType arg1, string arg2)
        {
            eventBus.GetEvent<LogEvent>().Publish(new LogInfo()
            {
                LogType = arg1,
                LogMessage = arg2
            });
        }


        #region 1.发布事件
        public void OnPublish<TEvent, TModel>(TModel model) where TEvent : PubSubEvent<TModel>, new()
        {
            eventBus.GetEvent<TEvent>().Publish(model);
        }

        public void OnPublish<TEvent>() where TEvent : PubSubEvent, new()
        {
            eventBus.GetEvent<TEvent>().Publish();
        }
        #endregion

        #region 2.订阅事件
        public SubscriptionToken Subscribe<TEvent, TModel>(Action<TModel> action) where TEvent : PubSubEvent<TModel>, new()
        {
            return eventBus.GetEvent<TEvent>().Subscribe(action);
        }

        public SubscriptionToken Subscribe<TEvent>(Action action) where TEvent : PubSubEvent, new()
        {
            return eventBus.GetEvent<TEvent>().Subscribe(action);
        }
        #endregion

        #region 3.0 取消订阅
        public void UnSubscribe<TEvent>(SubscriptionToken token) where TEvent : PubSubEvent, new()
        {
            eventBus.GetEvent<TEvent>().Unsubscribe(token);
        }


        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="token">订阅Token</param>
        public void UnSubscribe<TEvent, TModel>(SubscriptionToken token) where TEvent : PubSubEvent<TModel>, new()
        {
            eventBus.GetEvent<TEvent>().Unsubscribe(token);
        }
        #endregion

        #region 保存和加载工程
        public void OnSaveProj(string saveProj)
        {
            Engine.Save(saveProj);
        }

        /// <summary>
        /// 加载项目
        /// </summary>
        /// <param name="projName"></param>
        public void OnLoadProj(string projName)
        {
            Engine.Initialize(projName);

        }
        #endregion

        /// <summary>
        /// 设置模式
        /// </summary>
        /// <param name="mode"></param>
        public void SetDeviceModeAsync(DeviceMode mode)
        {
            Task.Run(() =>
            {
                if (Engine.SetEngineMode(mode, out string errMsg))
                {
                    Engine.IsNeedSave = true;
                }
                else
                {
                    OnAlert(LogType.Error, errMsg);
                }
                // 通知
                eventBus.GetEvent<DeviceModeEvent>().Publish(DeviceMode);
            });
        }


        public void StartHomeAsync()
        {
            Task.Run(() =>
            {
                if (!Engine.Home(out string errMsg))
                {
                    OnAlert(LogType.Error, errMsg);
                }
            });
        }

        /// <summary>
        /// 发生Log
        /// </summary>
        /// <param name="type">Log类型</param>
        /// <param name="message">消息</param>
        public void OnLog(LogType type, string message)
        {
            Engine_LogEvent(type, message);
        }


        /// <summary>
        /// 消息提醒
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">消息</param>
        public void OnAlert(LogType type, string message)
        {
            eventBus.GetEvent<AlertEvent>().Publish(new LogInfo() { LogType = type, LogMessage = message });
            OnLog(type, message);
        }

        /// <summary>
        /// 检查IO是否在同类型设备上已经使用
        /// </summary>
        /// <param name="vDeiviceType">设备类型</param>
        /// <param name="checkIO">IO信号</param>
        /// <returns>是否已经使用</returns>
        public void CheckIsUsed(IVirtualDevice[] vDevice, params Guid[] excludeDevices)
        {
            foreach (var item in vDevice)
            {
                if (item.ByUse != null)
                {
                    if (excludeDevices.Contains(item.ByUse.ID)) continue;
                    string message = $"{item.Name} 已经被类型为:{item.ByUse.GetType().Name} 的设备:{item.ByUse.Name} 使用!";
                    throw new DeviceException(message);
                }
            }
        }

        public void MainTainClear(MaintainItemModel model)
        {
            var maintainmodel = new MaintainModel()
            {
                Name = model.Name,
                MaxHP = (float)model.MaxHP,
                MaintainHP = (float)model.MaintainHP,
                UsedHP = (float)model.UsedHP,
                CurrentHP = (float)model.CurrentHP,
            };
            Engine.MaintainZero(maintainmodel);
        }
    }
}