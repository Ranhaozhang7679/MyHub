#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PageVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem
* 文 件 名:       PageVM.cs
* 创建时间:       2022/4/14 17:17:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ca3da9e7-77f7-4057-8c18-fb33191711f9
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/14 17:17:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Config;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem
{
    public class PageVM : BaseVM, INavigationAware
    {
        /// <summary>
        /// 循环
        /// </summary>
        protected CancellationTokenSource whileToken = null;

        /// <summary>
        /// 是否显示添加按钮
        /// </summary>
        public virtual bool IsShowAdd { get; }

        /// <summary>
        /// 是否显示自动查找按钮
        /// </summary>
        public virtual bool IsShowAuto { get; }

        /// <summary>
        /// 是否显示删除按钮
        /// </summary>
        public virtual bool IsShowRemove { get; }

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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine"></param>
        protected PageVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            Modules = deviceEngine.GetModules();
        }

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            base.Subscribe(engineUI);
            engineUI.Subscribe<DeviceModeEvent, DeviceMode>((mode) =>
            {
                ModeChanged(mode);
            });

            engineUI.Subscribe<RoleChangedEvent, SystemRole>(role =>
            {
                SysRole = role;
            });

            engineUI.Subscribe<PageEnabledEvent, bool>(pageEnabled =>
            {
                PageEnabled = pageEnabled;
            });
        }

        /// <summary>
        /// 模式状态切换
        /// </summary>
        /// <param name="mode">模式状态</param>
        protected virtual void ModeChanged(DeviceMode mode)
        {
            IsReal = mode == DeviceMode.Real;
            PageEnabled = deviceEngine.GetMachineStatus() != Motion.DataStruct.EngineStatus.Running;
        }

        /// <summary>
        /// 是否创建新示例。为true的时候表示不创建新示例，页面还是之前的；如果为false，则创建新的页面
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        /// <summary>
        /// 导航离开当前页面前
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// 导航到当前页面前, 此处可以传递过来的参数以及是否允许导航等动作的控制
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            PageModel pModel = navigationContext.Parameters["Page"] as PageModel;
            if (pModel != null)
            {
                pModel.CurrentVM = this;
            }

            ModeChanged(SimEngineUI.Engine.DeviceMode);
        }

        /// <summary>
        /// 属性对象
        /// </summary>
        private object _propertyObj;
        public object PropertyObj
        {
            get { return _propertyObj; }
            set { SetProperty(ref _propertyObj, value); }
        }

        /// <summary>
        /// 权限
        /// </summary>
        private SystemRole _sysRole = SystemRole.Operator;
        public SystemRole SysRole
        {
            get { return _sysRole; }
            set { SetProperty(ref _sysRole, value); }
        }

        /// <summary>
        /// 页面是否启用
        /// </summary>
        private bool _pageEnabled = true;
        public bool PageEnabled
        {
            get { return _pageEnabled; }
            set { SetProperty(ref _pageEnabled, value); }
        }

        /// <summary>
        /// 是否硬件模式
        /// </summary>
        private bool _isReal;
        public bool IsReal
        {
            get { return _isReal; }
            set { SetProperty(ref _isReal, value); }
        }

        /// <summary>
        /// 响应新增
        /// </summary>
        /// <param name="dialog"></param>
        public virtual void AddNewItem()
        {

        }

        /// <summary>
        /// 自动搜索硬件
        /// </summary>
        public virtual void AutoSearchDevice()
        {
        }

        /// <summary>
        /// 删除按钮
        /// </summary>
        public virtual void RemoveItem()
        {
        }

        /// <summary>
        /// 页面离开
        /// </summary>
        public virtual void Leave()
        {
            whileToken?.Cancel();
        }

        public virtual void Enter()
        {

        }
    }
}