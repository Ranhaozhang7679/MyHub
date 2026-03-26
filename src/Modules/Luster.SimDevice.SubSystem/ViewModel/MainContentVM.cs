#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MainContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       MainContentVM.cs
* 创建时间:       2022/4/13 19:06:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      113b6b86-10ca-4d10-b9b9-b573eb8ae920
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/13 19:06:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Controls;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Config;
using Luster.SimDevice.SubSystem.Events;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    /// <summary>
    /// 内容主页
    /// </summary>
    public class MainContentVM : BaseVM, INavigationAware
    {
        /// <summary>
        /// 页面模型
        /// </summary>
        private PageVM pageVM = null;

        private Dispatcher _dispatcher;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine">引擎</param>
        protected MainContentVM(ISimDeviceEngineUI _engine, Dispatcher dispatcher) : base(_engine)
        {
            _dispatcher = dispatcher;
        }


        #region 常用属性
        /// <summary>
        /// 是否显示添加按钮
        /// </summary>
        private bool _isShowAdd;

        public bool IsShowAdd
        {
            get { return _isShowAdd; }
            set { SetProperty(ref _isShowAdd, value); }
        }

        /// <summary>
        /// 显示自动硬件搜索
        /// </summary>
        private bool _isShowAuto;

        public bool IsShowAuto
        {
            get { return _isShowAuto; }
            set { SetProperty(ref _isShowAuto, value); }
        }

        /// <summary>
        /// 显示自动硬件搜索
        /// </summary>
        private bool _isShowRemove;

        public bool IsShowRemove
        {
            get { return _isShowRemove; }
            set { SetProperty(ref _isShowRemove, value); }
        }



        /// <summary>
        /// 图标
        /// </summary>
        private string _icon;
        public string Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        /// <summary>
        /// 标题
        /// </summary>
        private string _title = Langs.LangProvider.GetLang(nameof(Title));

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        #endregion

        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <param name="engineUI"></param>
        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            engineUI.Subscribe<NavigationEvent, PageModel>(model =>
             {

                 if (model != null && model.CurrentVM != null && model.CurrentVM is PageVM pVM)
                 {
                     pageVM = pVM;
                     IsShowAdd = pVM.IsShowAdd;
                     IsShowAuto = pVM.IsShowAuto;
                     IsShowRemove = pVM.IsShowRemove;
                 }

                 Icon = model.Icon;
                 Title = Langs.LangProvider.GetLang(model.Name);
             });

            engineUI.Subscribe<LogEvent, LogInfo>(message =>
            {
                // 日志记录
                //if (message.Type == "Error")
                //    Growl.Error(message.Message);
            });

            engineUI.Subscribe<AlertEvent, LogInfo>((msg) =>
            {
                switch (msg.LogType)
                {
                    case LogType.Info:
                        Growl.Info(msg.LogMessage);
                        break;
                    case LogType.Warning:
                        Growl.Warning(msg.LogMessage);
                        break;
                    case LogType.Error:
                        Growl.Error(msg.LogMessage);
                        break;
                }
            });
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            pageVM?.Enter();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 整个页面离开
        /// </summary>
        /// <param name="navigationContext"></param>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            pageVM?.Leave();
        }

        /// <summary>
        /// 新增命令
        /// </summary>
        private DelegateCommand _addNewCommand;
        public DelegateCommand AddNewCommand => _addNewCommand ?? (_addNewCommand = new DelegateCommand(() =>
          {
              if (pageVM == null)
              {
                  return;
              }

              pageVM.AddNewItem();
          }));


        private DelegateCommand _autoSearchCommand;
        public DelegateCommand AutoSearchCommand => _autoSearchCommand ?? (_autoSearchCommand = new DelegateCommand(() =>
        {
            if (pageVM == null)
            {
                return;
            }

            pageVM.AutoSearchDevice();
        }));

        private DelegateCommand _removeCommand;
        public DelegateCommand RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand(() =>
        {
            if (pageVM == null)
            {
                return;
            }

            pageVM.RemoveItem();
        }));
    }
}