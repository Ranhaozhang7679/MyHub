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

using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DigitalSetup.Datas;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 流程首页
    /// </summary>
    public class DigitalAssContentVM : MotionPageVM
    {
        private IRegionManager _regionManager;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 菜单信息
        /// </summary>
        private ObservableCollection<DigitalAssPageModel> _pages;
        public ObservableCollection<DigitalAssPageModel> Pages
        {
            get { return _pages; }
            set { SetProperty(ref _pages, value); }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cBus"></param>
        /// <param name="bus"></param>
        public DigitalAssContentVM(ICommonBus cBus, IRegionManager regionManager, IDialogService dialogService, Dispatcher dispatcher) : base(cBus)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;
            BuildPages();
        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

        }

        /// <summary>
        /// 获取菜单信息
        /// </summary>
        private void BuildPages()
        {
            Pages = new ObservableCollection<DigitalAssPageModel>();
            Pages.AddRange(DigitalAssPageModel.Pages);
        }

        /// <summary>
        /// 菜单切换功能
        /// </summary>
        private DelegateCommand<DigitalAssPageModel> _selectedCommand;
        public DelegateCommand<DigitalAssPageModel> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<DigitalAssPageModel>((item) =>
        {
            if (item == null) return;
            SetSelected(item.Name);
            _regionManager.RequestNavigate("DigitalAssEditorRegion", item.Region);
        }));

        private void SetSelected(string name)
        {
            foreach (var item in Pages)
            {
                if (item.Name != name)
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.IsSelected = true;
                }
            }
        }


        /// <summary>
        /// 模块加载
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            // 默认日志页面
            _regionManager.RequestNavigate("LogContentRegon_Ass", "LogContent");
        }));




    }
}