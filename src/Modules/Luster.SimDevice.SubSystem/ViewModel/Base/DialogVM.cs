#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       DialogVM.cs
* 创建时间:       2022/4/15 11:06:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      09cb0f14-f44b-49cf-9984-cd349236c25a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 11:06:33
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem
{
    /// <summary>
    /// 对话框父类
    /// </summary>
    public class DialogVM : BaseDialogVM, IDialogAware, INotifyDataErrorInfo
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
        /// 构造函数
        /// </summary>
        /// <param name="_engine"></param>
        protected DialogVM(ISimDeviceEngineUI _engine)
        {
            deviceEngine = _engine.Engine;
            SimEngineUI = _engine;
            Modules = deviceEngine.GetModules();
        }

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override string L(string key)
        {
            return base.L(key);
        }
        
        /// <summary>
        /// 模块集合
        /// </summary>
        private List<string> _modules;
        public List<string> Modules
        {
            get { return _modules; }
            set
            {
                SetProperty(ref _modules, value);
            }
        }
    }
}