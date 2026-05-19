#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MainContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       MainContentVM.cs
* 创建时间:       2022/5/24 10:54:07
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4fd534e1-34fc-4474-bae1-fa2f2c671817
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 10:54:07
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using DC.Authorization;
using DC.Authorization.Models;
using Luster.Common.Tools;
using Luster.Control.Wpf.Motion;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 流程首页
    /// </summary>
    public class FlowContentVM : MotionPageVM
    {
        private IRegionManager _regionManager;

        /// <summary>
        /// 流程
        /// </summary>
        private FlowBus eventBus;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        private readonly Dispatcher _dispatcher;
        private readonly IAuthorizationFacade _authFacade;

        /// <summary>
        /// 构造函数
        /// </summary>
        public FlowContentVM(ICommonBus cBus, IRegionManager regionManager, FlowBus flowBus,
            IDialogService dialogService, Dispatcher dispatcher, IAuthorizationFacade authFacade) : base(cBus, authFacade)
        {
            _regionManager = regionManager;
            eventBus = flowBus;
            _dialogService = dialogService;
            _dispatcher = dispatcher;
            _authFacade = authFacade;

            RefreshVisibility();

            // 登录/注销/等级切换时刷新面板可见性
            _authFacade.AuthChanged += (_, _) => _dispatcher.Invoke(() => RefreshVisibility());
        }

        private void RefreshVisibility()
        {
            FlowFuncModuleVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowFuncModule, RightType.Visibility));
            FlowParamConfigVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowParamConfig, RightType.Visibility));
            FlowSearchModuleVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowSearchModule, RightType.Visibility));
            FlowEditorVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowEditor, RightType.Visibility));
            FlowLogVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowLog, RightType.Visibility));
            FlowGlobalVarVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowGlobalVar, RightType.Visibility));
            FlowAxisDebugVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowAxisDebug, RightType.Visibility));
            FlowCTVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowCT, RightType.Visibility));
            FlowCacheDataVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowCacheData, RightType.Visibility));
            FlowStationOverviewVisible = ToVisibility(_authFacade.HasAuth(AuthDictionary.FlowStationOverview, RightType.Visibility));
        }

        private static Visibility ToVisibility(bool hasAuth) => hasAuth ? Visibility.Visible : Visibility.Collapsed;

        // ─── 面板可见性（绑定到 XAML，登录/注销时自动刷新）───
        private Visibility _flowFuncModuleVisible;
        public Visibility FlowFuncModuleVisible { get => _flowFuncModuleVisible; private set => SetProperty(ref _flowFuncModuleVisible, value); }
        private Visibility _flowParamConfigVisible;
        public Visibility FlowParamConfigVisible { get => _flowParamConfigVisible; private set => SetProperty(ref _flowParamConfigVisible, value); }
        private Visibility _flowSearchModuleVisible;
        public Visibility FlowSearchModuleVisible { get => _flowSearchModuleVisible; private set => SetProperty(ref _flowSearchModuleVisible, value); }
        private Visibility _flowEditorVisible;
        public Visibility FlowEditorVisible { get => _flowEditorVisible; private set => SetProperty(ref _flowEditorVisible, value); }
        private Visibility _flowLogVisible;
        public Visibility FlowLogVisible { get => _flowLogVisible; private set => SetProperty(ref _flowLogVisible, value); }
        private Visibility _flowGlobalVarVisible;
        public Visibility FlowGlobalVarVisible { get => _flowGlobalVarVisible; private set => SetProperty(ref _flowGlobalVarVisible, value); }
        private Visibility _flowAxisDebugVisible;
        public Visibility FlowAxisDebugVisible { get => _flowAxisDebugVisible; private set => SetProperty(ref _flowAxisDebugVisible, value); }
        private Visibility _flowCTVisible;
        public Visibility FlowCTVisible { get => _flowCTVisible; private set => SetProperty(ref _flowCTVisible, value); }
        private Visibility _flowCacheDataVisible;
        public Visibility FlowCacheDataVisible { get => _flowCacheDataVisible; private set => SetProperty(ref _flowCacheDataVisible, value); }
        private Visibility _flowStationOverviewVisible;
        public Visibility FlowStationOverviewVisible { get => _flowStationOverviewVisible; private set => SetProperty(ref _flowStationOverviewVisible, value); }

        // ─── Region名称 → AuthItem 映射（用于导航前权限拦截）───
        private static readonly Dictionary<string, AuthItem> ConfigAuthMap = new Dictionary<string, AuthItem>
        {
            { "LogContent", AuthDictionary.FlowLog },
            { "GlobalContent", AuthDictionary.FlowGlobalVar },
            { "AxisSetContent", AuthDictionary.FlowAxisDebug },
            { "CTConfigContent", AuthDictionary.FlowCT },
            { "CacheDataContent", AuthDictionary.FlowCacheData },
            { "FlowViewContent", AuthDictionary.FlowStationOverview },
        };

        private static readonly Dictionary<string, AuthItem> LeftPanelAuthMap = new Dictionary<string, AuthItem>
        {
            { "ModuleContent", AuthDictionary.FlowFuncModule },
            { "InParamContent", AuthDictionary.FlowParamConfig },
            { "SearchContent", AuthDictionary.FlowSearchModule },
        };

        /// <summary>
        /// 根据 ConfigAuthMap 顺序，找到第一个有权限的面板名称
        /// </summary>
        private string GetFirstAuthorizedConfigPanel()
        {
            foreach (var kv in ConfigAuthMap)
            {
                if (_authFacade.HasAuth(kv.Value, RightType.Visibility))
                    return kv.Key;
            }
            return null;
        }

        /// <summary>
        /// 注册全局命令
        /// </summary>
        protected override void BindGlobalCommnads()
        {
            GlobalCommands.SearchCommand.RegisterCommand(SearchCommand);
            GlobalCommands.BackCommand.RegisterCommand(BackCommand);
            GlobalCommands.ForwardCommand.RegisterCommand(ForwardCommand);
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            bus.GetEvent<ModuleSelectedEvent>().Subscribe(ms =>
            {
                SelectPara = true;
            });

            bus.GetEvent<ModuleLoadedEvent>().Subscribe(m =>
            {
                SelectModule = false;
            });

            bus.GetEvent<CompareLookEvent>().Subscribe(arrayModules =>
            {
                if (!IsModuleDiffer && _authFacade.HasAuth(AuthDictionary.FlowStationOverview, RightType.Visibility))
                {
                    _regionManager.RequestNavigate("EditorConfigRegion", "FlowViewContent");
                    IsModuleDiffer = true;
                    if (IsFirst)
                    {
                        IsFirst = false;
                        eventBus.OnCompareLook(arrayModules.ToList());
                    }
                }
            });
        }

        /// <summary>
        /// 模块移动
        /// </summary>
        private DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(() =>
        {
            SelectSearch = true;
        }));

        /// <summary>
        /// 模块移动
        /// </summary>
        private DelegateCommand<string> _changeCommand;
        public DelegateCommand<string> ChangeCommand => _changeCommand ?? (_changeCommand = new DelegateCommand<string>((region) =>
        {
            if (LeftPanelAuthMap.TryGetValue(region, out var authItem)
                && !_authFacade.HasAuth(authItem, RightType.Visibility))
                return;
            _regionManager.RequestNavigate("TopRegion", region);
        }));

        /// <summary>
        /// 模块移动
        /// </summary>
        private DelegateCommand<string> _configCommand;
        public DelegateCommand<string> ConfigCommand => _configCommand ?? (_configCommand = new DelegateCommand<string>((region) =>
        {
            if (ConfigAuthMap.TryGetValue(region, out var authItem)
                && !_authFacade.HasAuth(authItem, RightType.Visibility))
                return;
            _regionManager.RequestNavigate("EditorConfigRegion", region);
        }));

        /// <summary>
        /// 是否选择参数
        /// </summary>
        private bool _selectPara;
        public bool SelectPara
        {
            get { return _selectPara; }
            set
            {
                SetProperty(ref _selectPara, value);
                if (value)
                {
                    SelectModule = false;
                    SelectSearch = false;
                }
            }
        }

        /// <summary>
        /// 是否选择模块
        /// </summary>
        private bool _selectModule;
        public bool SelectModule
        {
            get { return _selectModule; }
            set
            {
                SetProperty(ref _selectModule, value); if (value)
                {
                    SelectPara = false;
                    SelectSearch = false;
                }
            }
        }

        /// <summary>
        /// 是否选择搜索
        /// </summary>
        private bool _selectSearch;
        public bool SelectSearch
        {
            get { return _selectSearch; }
            set
            {
                SetProperty(ref _selectSearch, value); if (value)
                {
                    SelectModule = false;
                    SelectPara = false;
                }
            }
        }

        /// <summary>
        /// 是否选择Log
        /// </summary>
        private bool _isCheckLog;
        public bool IsCheckLog
        {
            get { return _isCheckLog; }
            set { SetProperty(ref _isCheckLog, value); }
        }

        /// <summary>
        /// 是否选择轴
        /// </summary>
        private bool _isCheckAxis;
        public bool IsCheckAxis
        {
            get { return _isCheckAxis; }
            set { SetProperty(ref _isCheckAxis, value); }
        }

        /// <summary>
        /// 是否选择CT
        /// </summary>
        private bool _isCheckCT;
        public bool IsCheckCT
        {
            get { return _isCheckCT; }
            set { SetProperty(ref _isCheckCT, value); }
        }

        /// <summary>
        /// 是否选择CT
        /// </summary>
        private bool _isCheckGlobal;
        public bool IsCheckGlobal
        {
            get { return _isCacheData; }
            set { SetProperty(ref _isCacheData, value); }
        }


        /// <summary>
        /// 是否选择CT
        /// </summary>
        private bool _isCacheData;
        public bool IsCacheData
        {
            get { return _isCacheData; }
            set { SetProperty(ref _isCacheData, value); }
        }

        /// <summary>
        /// 是否选择模块对比
        /// </summary>
        private bool _isModuleDiffer;
        public bool IsModuleDiffer
        {
            get { return _isModuleDiffer; }
            set { SetProperty(ref _isModuleDiffer, value); }
        }

        /// <summary>
        /// 是否选择报警代码对比
        /// </summary>
        private bool _isModuleError;
        public bool IsModuleError
        {
            get { return _isModuleError; }
            set { SetProperty(ref _isModuleError, value); }
        }

        /// <summary>
        /// 是否第一次
        /// </summary>
        private bool _isFirst = true;
        public bool IsFirst
        {
            get { return _isFirst; }
            set { SetProperty(ref _isFirst, value); }
        }

        /// <summary>
        /// 模块加载
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            // 导航到第一个有权限的右侧面板
            var firstPanel = GetFirstAuthorizedConfigPanel();
            if (firstPanel != null)
            {
                _regionManager.RequestNavigate("EditorConfigRegion", firstPanel);
                IsCheckLog = firstPanel == "LogContent";
            }
        }));

        /// <summary>
        /// 回退
        /// </summary>
        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand => _backCommand ?? (_backCommand = new DelegateCommand(() =>
        {
            eventBus.OnBackForward(true);
        }));

        /// <summary>
        /// 前进
        /// </summary>
        private DelegateCommand _forwardCommand;
        public DelegateCommand ForwardCommand => _forwardCommand ?? (_forwardCommand = new DelegateCommand(() =>
        {
            eventBus.OnBackForward(false);
        }));

        //public override void OnNavigatedTo(NavigationContext navigationContext)
        //{
        //    base.OnNavigatedTo(navigationContext);

        //    // 检查是否需要进行指纹认证

        //}
    }
}