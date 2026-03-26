using HandyControl.Controls;
using HandyControl.Controls;
using Luster.Common.Assets.Global;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Config;
using Luster.SimDevice.SubSystem.Events;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class DeviceContentVM : BaseVM, INavigationAware
    {
        /// <summary>
        /// 页面模型
        /// </summary>
        private PageVM pageVM = null;

        private Dispatcher _dispatcher;

        private IRegionManager _regionManager;
        protected DeviceContentVM(ISimDeviceEngineUI engineUI,
            IRegionManager regionManager,
            Dispatcher dispatcher) : base(engineUI)
        {
            _regionManager = regionManager;
            _dispatcher = dispatcher;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            pageVM?.Leave();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            pageVM?.Enter();
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

            engineUI.Subscribe<PageEnabledEvent, bool>(pageEnabled =>
            {
                GlobalProperty.IsEnabeld = pageEnabled;
            });
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
