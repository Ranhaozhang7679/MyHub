#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PageModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       PageModel.cs
* 创建时间:       2022/7/12 9:09:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6ebf29df-d05d-4ad7-9bdf-6791332abd82
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 9:09:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup.Datas
{
    public class DigitalAssPageModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// control名称
        /// </summary>
        private string _region = "";
        public string Region
        {
            get { return _region; }
            set { SetProperty(ref _region, value); }
        }

        /// <summary>
        /// 图片信息
        /// </summary>
        private string _iconfont;
        public string Iconfont
        {
            get { return _iconfont; }
            set { SetProperty(ref _iconfont, value); }
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// 点检状态（聚合所有二级子界面的状态）
        /// </summary>
        private CheckStatus _checkStatus = CheckStatus.NotChecked;
        public CheckStatus CheckStatus
        {
            get => _checkStatus;
            set => SetProperty(ref _checkStatus, value);
        }

        /// <summary>
        /// 引用对应的二级子页面列表（用于状态计算）
        /// </summary>
        private List<CommonPageModel> _subPages;
        public List<CommonPageModel> SubPages
        {
            get => _subPages;
            set
            {
                if (_subPages != value)
                {
                    _subPages = value;
                    // 当子页面列表变化时，重新计算整体状态
                    //RefreshCheckStatus();
                }
            }
        }

        /// <summary>
        /// 刷新点检状态（根据所有二级子页面的状态聚合计算）
        /// </summary>
        public void RefreshCheckStatus()
        {
            CheckStatus newStatus;

            if (_subPages == null || _subPages.Count == 0)
            {
                newStatus = CheckStatus.NotChecked;
            }
            else
            {
                bool hasNG = false;
                bool hasOK = false;
                bool hasNotChecked = false;

                foreach (var page in _subPages)
                {
                    if (page == null) continue;

                    switch (page.CheckStatus)
                    {
                        case CheckStatus.CheckedFail:
                            hasNG = true;
                            break; // 任一 NG，整体就是 NG
                        case CheckStatus.CheckedOK:
                            hasOK = true;
                            break;
                        case CheckStatus.NotChecked:
                            hasNotChecked = true;
                            break;
                    }
                }

                // 有 NG 直接返回 NG
                if (hasNG)
                {
                    newStatus = CheckStatus.CheckedFail;
                }
                // 有未点检的，返回 NotChecked
                else if (hasNotChecked)
                {
                    newStatus = CheckStatus.NotChecked;
                }
                // 全部 OK
                else if (hasOK)
                {
                    newStatus = CheckStatus.CheckedOK;
                }
                else
                {
                    newStatus = CheckStatus.NotChecked;
                }
            }

            // 即使值相同，也强制触发 PropertyChanged 事件以更新 UI
            _checkStatus = newStatus;
            RaisePropertyChanged(nameof(CheckStatus));
        }


        /// <summary>
        /// 子页面注册表 - 存储每个一级页面对应的二级子页面
        /// </summary>
        private static Dictionary<string, List<CommonPageModel>> _subPagesRegistry = new Dictionary<string, List<CommonPageModel>>();

        /// <summary>
        /// 注册子页面
        /// </summary>
        /// <param name="parentRegion">父页面的Region名称</param>
        /// <param name="subPages">子页面列表</param>
        public static void RegisterSubPages(string parentRegion, ObservableCollection<CommonPageModel> subPages)
        {
            if (string.IsNullOrEmpty(parentRegion) || subPages == null)
                return;

            _subPagesRegistry[parentRegion] = subPages.ToList();
        }

        /// <summary>
        /// 获取子页面列表
        /// </summary>
        /// <param name="parentRegion">父页面的Region名称</param>
        /// <returns>子页面列表</returns>
        public static List<CommonPageModel> GetSubPages(string parentRegion)
        {
            if (_subPagesRegistry.TryGetValue(parentRegion, out var subPages))
            {
                return subPages;
            }
            return new List<CommonPageModel>();
        }

        /// <summary>
        /// 根据父页面Region和子页面Name查找子页面
        /// </summary>
        public static CommonPageModel FindSubPage(string parentRegion, string subPageName)
        {
            var subPages = GetSubPages(parentRegion);
            return subPages.FirstOrDefault(p => p.Name == subPageName);
        }

        /// <summary>
        /// 根据Region名称查找一级页面
        /// </summary>
        /// <param name="region">Region名称</param>
        /// <returns>找到的DigitalAssPageModel，未找到返回null</returns>
        public static DigitalAssPageModel FindPageByRegion(string region)
        {
            if (_pages == null || string.IsNullOrEmpty(region))
                return null;

            return _pages.FirstOrDefault(p => p.Region == region);
        }

        /// <summary>
        /// 根据Region名称查找对应的页面Name（用于PageStatusService）
        /// </summary>
        /// <param name="region">Region名称</param>
        /// <returns>页面Name，未找到返回null</returns>
        public static string GetNameByRegion(string region)
        {
            if (_pages == null || string.IsNullOrEmpty(region))
                return null;

            var page = _pages.FirstOrDefault(p => p.Region == region);
            return page?.Name;
        }

        private static ObservableCollection<DigitalAssPageModel> _pages;
        public static ObservableCollection<DigitalAssPageModel> Pages
        {
            get
            {
                if (_pages == null)
                {
                    _pages = new ObservableCollection<DigitalAssPageModel>()
                    {
                        new DigitalAssPageModel() 
                        { Name = "MainParameters",IsSelected=false,Region="ParamConfirmContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609"  },
                        new DigitalAssPageModel() 
                        { Name = "Communications",IsSelected=false,Region="AutoCommunicationConfigContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609"  },
                        new DigitalAssPageModel() 
                        { Name = "IOConform",IsSelected=false,Region="IOinspectionContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609"   },
                        new DigitalAssPageModel() 
                        { Name = "Horizontal",IsSelected=false,Region="PlatformLevelAutoConfirmContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609"   },
                        new DigitalAssPageModel()
                        { Name = "LoadCell",IsSelected=false,Region="AutomaticLoadCellContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },
                        new DigitalAssPageModel() 
                        { Name = "Embossing" ,IsSelected=false, Region = "AutomaticEmbossingContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },
                        new DigitalAssPageModel() 
                        { Name = "DigitalVision",IsSelected=false, Region = "DigitalVisionContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },
                        //AutoVisualCalibration
                        new DigitalAssPageModel()
                        { Name = "PointTeaching",IsSelected=false, Region = "PointTeachingContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },
                         new DigitalAssPageModel()
                        { Name = "AutoVisualCalibration",IsSelected=false, Region = "AutoVisualCalibrationContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },
                           new DigitalAssPageModel()
                        { Name = "DataValidation",IsSelected=false, Region = "DataValidationContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },
                         new DigitalAssPageModel()
                        { Name = "LADUpload",IsSelected=false, Region = "LADUploadContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },
                         new DigitalAssPageModel()
                        { Name = "AutoVerication",IsSelected=false, Region = "AutoVericationContent",
                            IsVisible=true,IsEnabled=true ,Iconfont="\xe609" },

                    };

                    List<DigitalAssPageModel> list = new List<DigitalAssPageModel>()
                    {

                    };

                }
                return _pages;
            }
            set
            {
                _pages = value;
            }
        }

        /// <summary>
        /// 选中项为互斥
        /// </summary>
        /// <param name="name"></param>
        public void SetSelected(string Name)
        {
            foreach (var item in Pages)
            {
                if (item.Name != Name)
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.IsSelected = true;
                }
            }
        }

        public void SetUnSelected(string Name)
        {
            foreach (var item in Pages)
            {
                if (item.Name == Name)
                {
                    item.IsSelected = false;
                }
            }
        }
    }
}