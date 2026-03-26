#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseMotionVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.ViewModel
* 文 件 名:       BaseMotionVM.cs
* 创建时间:       2022/6/27 20:56:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      8434699f-e027-4e87-acdc-0acaf0a39ecf
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/27 20:56:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using DC.Authorization;
using DC.Authorization.WPF;
using Luster.Common.DataStruct;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct.Enums;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class MotionVM : AuthViewModelBase
    {
        /// <summary>
        /// 通用事件总线
        /// </summary>
        protected ICommonBus commonBus { get; set; }

        /// <summary>
        /// 多语言属性
        /// </summary>
        private List<PropertyInfo> LangProperties { get; set; }

        /// <summary>
        /// 角色Token
        /// </summary>
        private SubscriptionToken roleToken = null;

        /// <summary>
        /// 信息描述
        /// </summary>
        private string _desc;
        public string Desc
        {
            get { return _desc; }
            set
            {
                SetProperty(ref _desc, value);
            }
        }

        /// <summary>
        /// 角色对象
        /// </summary>
        private SystemRole _systemRole = SystemRole.Operator;
        private readonly IAuthorizationFacade? facade;

        public SystemRole SysRole
        {
            get { return _systemRole; }
            set
            {
                SetProperty(ref _systemRole, value);
            }
        }

        /// <summary>
        /// 是否管理员权限
        /// </summary>
        protected bool IsAdmin => SysRole == SystemRole.Admin;

        protected bool IsEngineer => (int)SysRole <= (int)SystemRole.Integrator;

        public MotionVM(IAuthorizationFacade auth = null) : base(auth)
        {
            this.facade = auth;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_commonBus">参数注册</param>
        public MotionVM(ICommonBus _commonBus, IAuthorizationFacade auth = null) : base(auth)
        {
            commonBus = _commonBus;
            this.facade = auth;
            RegisterEvent(commonBus.EventBus);
            BindGlobalCommnads();
            LangProperties = new List<PropertyInfo>();
            Desc = "Desc";
            HandyControl.Tools.ConfigHelper.Instance.PropertyChanged -= Instance_PropertyChanged;
            HandyControl.Tools.ConfigHelper.Instance.PropertyChanged += Instance_PropertyChanged;
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HandyControl.Tools.ConfigHelper cHelper = (HandyControl.Tools.ConfigHelper)sender;
            string lang = cHelper.Lang.IetfLanguageTag;

            if (LangProperties.Count > 0)
            {
                if (lang == "en")
                {
                    Desc = "Key";
                }
                else
                {
                    Desc = nameof(Desc);
                }

                // 更新属性
                foreach (var item in LangProperties)
                {
                    var srcV = item.GetValue(this, null);
                    item.SetValue(this, srcV);
                }
            }
        }

        /// <summary>
        /// 事件注册
        /// </summary>
        /// <param name="bus">事件</param>
        protected virtual void RegisterEvent(IEventAggregator bus)
        {
            // 初始化
            if (commonBus.CurrentUser != null)
            {
                SysRole = commonBus.CurrentUser.UserRole;
            }

            Unsubscribe<RoleChangeEvent>(roleToken);

            roleToken = bus.GetEvent<RoleChangeEvent>().Subscribe(r =>
            {
                SysRole = r;
            });
        }

        /// <summary>
        /// 注册全局命令
        /// </summary>
        protected virtual void BindGlobalCommnads()
        {

        }


        protected void Unsubscribe<T>(SubscriptionToken token) where T : EventBase, new()
        {
            if (token != null)
            {
                commonBus.EventBus.GetEvent<T>().Unsubscribe(token);
                token = null;
            }
        }

        protected void RegisterMuliLang(params string[] propLangs)
        {
            foreach (var item in propLangs)
            {
                LangProperties.Add(GetType().GetProperty(item));
            }
        }

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string L(string key)
        {
            return commonBus.L(key);
        }

        public ICommonBus GetBus()
        {
            return commonBus;
        }
    }
}