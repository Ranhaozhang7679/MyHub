using HandyControl.Data;
using LiveCharts.Wpf;
using LiveCharts;
using Luster.Motion.ReportUI;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Linq;
using LiveCharts.Defaults;
using System.Collections.Generic;
using Luster.Motion.ReportUI.Model;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using Prism.Regions;
using System.Windows.Forms;
using Prism.Services.Dialogs;
using Prism.Mvvm;
using System.IO;
using System.Reflection;
using Luster.Common.Tools;
using Luster.Motion.ReportUI.ViewModel;
using Luster.Common.DataAccess.Repositories;
using System.Configuration;

namespace Luster.Control.Wpf.Motion
{
    public class ReportContentVM : BindableBase
    {
        private IRegionManager _regionManager;

        private ReportPageModel _seletedReportPage;
        public ReportPageModel SelectedReportPage
        {
            get => _seletedReportPage;
            set => SetProperty(ref _seletedReportPage, value);
        }

        public DelegateCommand<ReportPageModel> SelectedCommand { get; set; }

        public ObservableCollection<ReportPageModel> Pages { get; }

        public ReportContentVM(IRepository reporitory, Dispatcher dispatcher, IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SelectedCommand = new DelegateCommand<ReportPageModel>(Selected);
            Pages = new ObservableCollection<ReportPageModel>();
            Pages.Add(new ReportPageModel() { Name = "ProductStatistics", IsSelected = true, Region = "ProductReportContent" });
            Pages.Add(new ReportPageModel() { Name = "CTStatistics", IsSelected = false, Region = "CTLogStatRTContent" });
            Pages.Add(new ReportPageModel() { Name = "FlingMaterialStatistics", IsSelected = false, Region = "FlingMaterialContent" });
            Pages.Add(new ReportPageModel() { Name = "ChangeRecord", IsSelected = false, Region = "ChangeContent" });
            Pages.Add(new ReportPageModel() { Name = "TaikeCurve", IsSelected = false, Region = "TaikeContent" });
            Pages.Add(new ReportPageModel() { Name = "TaikeAnnotatedCurve", IsSelected = false, Region = "TaikeAnnotatedContent" });
            //Pages.Add(new ReportPageModel() { Name = "CTLog统计", IsSelected = false, Region = "CTLogStatContent" });

            LoadFromDll();
            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();

            if (ConfigurationManager.AppSettings.AllKeys.Contains("UITemplate"))
            {
                var keyvalue = ConfigurationManager.AppSettings["UITemplate"];
                if (keyvalue != "")
                {
                    ReportSelectVisible = false;
                }
                else
                {
                    ReportSelectVisible = true;
                }
            }
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        private bool _reportSelectVisible;
        public bool ReportSelectVisible
        {
            get { return _reportSelectVisible; }
            set { SetProperty(ref _reportSelectVisible, value); }
        }

        private void Selected(ReportPageModel obj)
        {
            if (obj != null)
            {
                SetSelected(obj.Name);
                _regionManager.RequestNavigate("ReportRegion", obj.Region);
            }
        }
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
        /// 加载Dll
        /// </summary>
        private void LoadFromDll()
        {
            List<ReportBaseVM> reportTypes = new List<ReportBaseVM>();
            var reportDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report");
            if (!Directory.Exists(reportDllPath))
            {
                return;
            }

            foreach (var item in Directory.GetFiles(reportDllPath, "*.dll"))
            {
                Assembly reportAss = Assembly.LoadFile(item);
                var vmTypes = ReflectionTool.GetTypesByAssembly<ReportBaseVM>(reportAss);
                var viewTypes = ReflectionTool.GetTypesByAssembly<IReportBaseView>(reportAss);

                foreach (var viewType in viewTypes)
                {
                    // 通过View查找对应的ViewModel
                    var vmType = vmTypes.FirstOrDefault(u => u.Name.StartsWith(viewType.Name));
                    if (vmType != null)
                    {
                        var model = Activator.CreateInstance(vmType) as ReportBaseVM;
                        //Pages.Add(new ReportPageModel() { Name = model.ReportName, IsSelected = false, Region = viewType.Name });
                        Pages.Add(new ReportPageModel() { Name = "ScanCodeStatistics", IsSelected = false, Region = viewType.Name });//强制改名显示，后续调整
                    }
                }
            }
        }

        /// <summary>
        /// 页面
        /// </summary>
        private int _page;
        public int Page
        {
            get { return _page; }
            set { SetProperty(ref _page, value); }
        }

        /// <summary>
        /// 没页数据多少个
        /// </summary>
        private int _total;
        public int Total
        {
            get { return _total; }
            set { SetProperty(ref _total, value); }
        }


    }
}
