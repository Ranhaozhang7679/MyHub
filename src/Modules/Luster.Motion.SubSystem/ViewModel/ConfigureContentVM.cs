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
        /// <summary>
        /// 导航事件
        /// </summary>
        public DelegateCommand<ConfigurePageModel> NavigateCommand { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="commonBus">通用</param>
        /// <param name="regionManager"></param>
        protected ConfigureContentVM(ICommonBus commonBus, IRegionManager regionManager, IDialogService dialogService, Dispatcher dispatcher) : base(commonBus)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<ConfigurePageModel>(Navigate);
            _dialogService = dialogService;
            _dispatcher = dispatcher;
            BuildPages();
        }

        /// <summary>
        /// 获取菜单信息
        /// </summary>
        private void BuildPages()
        {
            Pages = new ObservableCollection<ConfigurePageModel>();
            Pages.AddRange(ConfigurePageModel.Pages);
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
