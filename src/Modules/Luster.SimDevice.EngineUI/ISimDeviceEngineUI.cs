#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SimDeviceEngineUI
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI
* 文 件 名:       ISimDeviceEngineUI.cs
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

using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Virtual;
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

namespace Luster.SimDevice.EngineUI
{
    public interface ISimDeviceEngineUI
    {
        /// <summary>
        /// 记录当前的状态
        /// </summary>
        DeviceMode DeviceMode { get; }

        /// <summary>
        /// 设备引擎
        /// </summary>
        IDeviceEngine Engine { get; set; }

        /// <summary>
        /// 对话框
        /// </summary>
        IDialogService Dialog { get; set; }

        /// <summary>
        /// 消息发布
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <typeparam name="TModel">发生的模型</typeparam>
        /// <param name="model">模型对象</param>
        void OnPublish<TEvent, TModel>(TModel model) where TEvent : PubSubEvent<TModel>, new();

        /// <summary>
        /// 消息发布
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        void OnPublish<TEvent>() where TEvent : PubSubEvent, new();

        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <typeparam name="callback">注册回调</typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        SubscriptionToken Subscribe<TEvent, TModel>(Action<TModel> callback) where TEvent : PubSubEvent<TModel>, new();

        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="callback">注册回调</param>
        /// <returns></returns>
        SubscriptionToken Subscribe<TEvent>(Action callback) where TEvent : PubSubEvent, new();

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="token">订阅Token</param>
        void UnSubscribe<TEvent>(SubscriptionToken token) where TEvent : PubSubEvent, new();

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="token">订阅Token</param>
        void UnSubscribe<TEvent, TModel>(SubscriptionToken token) where TEvent : PubSubEvent<TModel>, new();

        /// <summary>
        /// 保存工程
        /// </summary>
        void OnSaveProj(string saveProj);

        /// <summary>
        /// 工程名称
        /// </summary>
        /// <param name="projName"></param>
        void OnLoadProj(string projName);

        /// <summary>
        /// 设置模式
        /// </summary>
        /// <param name="mode"></param>
        void SetDeviceModeAsync(DeviceMode mode);

        void StartHomeAsync();

        /// <summary>
        /// 发生Log
        /// </summary>
        /// <param name="type">Log类型</param>
        /// <param name="message">消息</param>
        void OnLog(LogType type, string message);

        /// <summary>
        /// 消息提醒
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">消息</param>
        void OnAlert(LogType type, string message);

        /// <summary>
        /// 检查IO是否在同类型设备上已经使用
        /// </summary>
        /// <param name="excludeDevice">要排除</param>
        /// <param name="refernceDevices"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        void CheckIsUsed(IVirtualDevice[] vDevice, params Guid[] excludeDevices);

        void MainTainClear(MaintainItemModel model);
    }
}