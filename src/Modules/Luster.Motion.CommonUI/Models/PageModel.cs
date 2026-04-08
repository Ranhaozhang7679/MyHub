#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PageModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       PageModel.cs
* 创建时间:       2022/8/4 15:37:12
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      ef7f2ed6-20c5-4279-8448-7cf9dc304794
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/4 15:37:12
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Luster.Motion.CommonUI.Models
{
    public class PageModel : BindableBase
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
        public bool page_IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// 名称
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
        public bool page_IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        private bool _isEnabled;
        public bool page_IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        private ImageSource _imagenormal;
        public ImageSource Imagenormal
        {
            get { return _imagenormal; }
            set { SetProperty(ref _imagenormal, value); }
        }

        private ImageSource _imageSelected;
        public ImageSource ImageSelected
        {
            get { return _imageSelected; }
            set { SetProperty(ref _imageSelected, value); }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="region">通过region名进行构建</param>
        public PageModel(string region)
        {
            if (Pages != null)
            {
                var pageselect = Pages.Where(t => t.Region == region);
                if (pageselect != null && pageselect.Count() > 0)
                {
                    Name = pageselect.First().Name;
                    region = pageselect.First().Region;
                    page_IsSelected = pageselect.First().page_IsSelected;
                    Iconfont = pageselect.First().Iconfont;
                }
            }
        }

        /// <summary>
        /// 空构造函数
        /// </summary>
        public PageModel()
        {
        }


        private static ObservableCollection<PageModel> _pages;
        public static ObservableCollection<PageModel> Pages
        {
            get
            {
                if (_pages == null)
                {

                    _pages = new ObservableCollection<PageModel>()
                    {
                          //new PageModel() { Name = "主页",IsSelected=false,Region="MainContent",Iconfont="\xe646",IsVisible=false,IsEnabled=true  },
                          new PageModel() {
                              Name = "Home",page_IsSelected=false,Region="MainContent",
                              Iconfont="\xe646",page_IsVisible=true,page_IsEnabled=true  },
                          new PageModel() { 
                              Name = "HardWare",page_IsSelected=false,Region="SimDeviceControl"
                          ,Iconfont="\xe647",page_IsVisible=true,page_IsEnabled=true    },
                          new PageModel() { 
                              Name = "Flow",page_IsSelected=false,Region="FlowContent",
                              Iconfont="\xe64a",page_IsVisible=true ,page_IsEnabled=true   },
                          new PageModel() { 
                              Name = "Alarm" ,page_IsSelected=false, Region = "AlarmContent",
                              Iconfont="\xe643",page_IsVisible=true,page_IsEnabled=true  },
                          new PageModel() { 
                              Name = "Statistics",page_IsSelected=false,Region="ReportContent" ,
                              Iconfont="\xe648",page_IsVisible=true ,page_IsEnabled=true  },
                          new PageModel() { 
                              Name = "Configure",page_IsSelected=false,Region="ConfigureContent",
                              Iconfont="\xe62c",page_IsVisible=true,page_IsEnabled=true    },
                          new PageModel() {
                              Name = "DigitalAss" ,page_IsSelected=false,Region="DigitalAssContent",
                              Iconfont="\xe64a",page_IsVisible=true,page_IsEnabled=true   },
                          new PageModel() {
                              Name = "IntegratedHardware" ,page_IsSelected=false,Region="IntegratedHardwareContent",
                              Iconfont="\xe64a",page_IsVisible=true,page_IsEnabled=true   },
                          new PageModel() { 
                              Name = "Project" ,page_IsSelected=false,Region="ProjectContent",
                              Iconfont="\xe61c",page_IsVisible=true,page_IsEnabled=true   },
                          
                    };

                    //_pages.Add(new PageModel()
                    //{
                    //    Name = "HardWare",
                    //    IsSelected = false,
                    //    Region = "DeviceDebugContent",
                    //    Iconfont = "\xe61c",
                    //    IsVisible = true,
                    //    IsEnabled = true
                    //});
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
            var page = Pages.Where(x => x.Name == Name).FirstOrDefault();
            if (page == null)
            {
                return;
            }
            foreach (var item in PageModel.Pages)
            {
                if (item.Name != Name)
                {
                    item.page_IsSelected = false;
                }
                else
                {
                    item.page_IsSelected = true;
                }
            }
        }

        /// <summary>
        /// 设置按钮是否可操作
        /// </summary>
        /// <param name="name"></param>
        public void SetEnable(string Name, bool isenable)
        {
            var page = Pages.Where(x => x.Name == Name).FirstOrDefault();
            if (page == null)
            {
                return;
            }
            foreach (var item in PageModel.Pages)
            {
                if (item.Name == Name)
                {
                    item.page_IsEnabled = isenable;
                }
            }
        }
    }
}
