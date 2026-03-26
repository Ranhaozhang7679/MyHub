#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       BaseVM.cs
* 创建时间:       2022/4/13 16:28:23
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      07033e06-222b-4ea1-b03f-4c006f42403a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/13 16:28:23
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.SimDevice.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Events;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using Luster.Motion.DataStruct;

namespace Luster.SimDevice.SubSystem
{
    public class BaseVM : BindableBase
    {
        /// <summary>
        /// 设备引擎
        /// </summary>
        protected IDeviceEngine deviceEngine;

        /// <summary>
        /// 仿真ui引擎
        /// </summary>
        protected ISimDeviceEngineUI SimEngineUI;

        /// <summary>
        /// 对话框
        /// </summary>
        protected IDialogService dialogService;

        protected BaseVM(ISimDeviceEngineUI engineUI)
        {
            SimEngineUI = engineUI;
            deviceEngine = engineUI.Engine;
            dialogService = engineUI.Dialog;

            // 注册全局命令
            ResisterGlobal();

            // 事件订阅
            Subscribe(engineUI);
        }

        protected virtual void ResisterGlobal()
        {

        }

        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <param name="engineUI">引擎</param>
        protected virtual void Subscribe(ISimDeviceEngineUI engineUI)
        {
        }
    }
}