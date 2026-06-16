using DC.Authorization;
using DC.Authorization.Models;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.SubSystem.Models;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ConfigureContentVM : MotionPageVM
    {
        private IRegionManager _regionManager;
        private IDialogService _dialogService;
        private readonly Dispatcher _dispatcher;
        private readonly IAuthorizationFacade _authFacade;

        /// <summary>
        /// 页面名称 → AuthItem 映射（用于权限过滤和导航拦截）
        /// </summary>
        private static readonly Dictionary<string, AuthItem> PageAuthMap = new Dictionary<string, AuthItem>
        {
            { "MachineConfigure", AuthDictionary.CfgMachineConfigure },
            { "PLCConfigure", AuthDictionary.CfgPLCConfigure },
            { "SoftConfigure", AuthDictionary.CfgSoftConfigure },
            { "Cockpit", AuthDictionary.CfgCockpit },
            { "RobotInfo", AuthDictionary.CfgRobotInfo },
            { "FileConfig", AuthDictionary.CfgFileConfig },
            { "VisionInformation", AuthDictionary.CfgVisionInfo },
            { "FXTCP", AuthDictionary.CfgFXTCP },
            { "FunctionEnable", AuthDictionary.CfgFunctionEnable },
        };

        /// <summary>
        /// 导航事件
        /// </summary>
        public DelegateCommand<ConfigurePageModel> NavigateCommand { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        protected ConfigureContentVM(ICommonBus commonBus, IRegionManager regionManager,
            IDialogService dialogService, Dispatcher dispatcher, IAuthorizationFacade authFacade) : base(commonBus, authFacade)
        {
            _regionManager = regionManager;
            _authFacade = authFacade;
            NavigateCommand = new DelegateCommand<ConfigurePageModel>(Navigate);
            _dialogService = dialogService;
            _dispatcher = dispatcher;
            BuildPages();

            // 登录/注销/等级切换时刷新页面列表
            _authFacade.AuthChanged += (_, _) => _dispatcher.Invoke(() => BuildPages());
        }

        /// <summary>
        /// 获取菜单信息（根据权限过滤）
        /// </summary>
        private void BuildPages()
        {
            Pages = new ObservableCollection<ConfigurePageModel>();
            foreach (var page in ConfigurePageModel.Pages)
            {
                if (PageAuthMap.TryGetValue(page.Name, out var authItem)
                    && !_authFacade.HasAuth(authItem, RightType.Visibility))
                    continue;
                Pages.Add(page);
            }
        }

        /// <summary>
        /// 菜单信息
        /// </summary>
        private ObservableCollection<ConfigurePageModel> _pages;
        public ObservableCollection<ConfigurePageModel> Pages
        {
            get { return _pages; }
            set { SetProperty(ref _pages, value); }
        }

        private void Navigate(ConfigurePageModel pagemodel)
        {
            if (pagemodel.Region != null)
            {
                // 权限拦截
                if (PageAuthMap.TryGetValue(pagemodel.Name, out var authItem)
                    && !_authFacade.HasAuth(authItem, RightType.Operation))
                    return;

                if (commonBus.CurrentRecipe == null)
                {
                    pagemodel.SetUnSelected(pagemodel.Name);
                    return;
                }
                pagemodel.SetSelected(pagemodel.Name);
                _regionManager.RequestNavigate("ConfigurationRegion", pagemodel.Region);
            }
        }
    }
}
