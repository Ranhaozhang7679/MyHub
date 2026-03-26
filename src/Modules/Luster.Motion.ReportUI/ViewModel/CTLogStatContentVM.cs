using Luster.Common.DataAccess.Repositories;
using Luster.Motion.ReportUI;
using Luster.Motion.ReportUI.ViewModel;
using Luster.Motion.ReportUI.Views;
using Luster.Motion.TaskFlow.Engine;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace YourNs.ViewModels
{
    public class CTLogStatContentVM : ReportBaseVM
    {
        public ObservableCollection<CTLogChildContentVM> Pages { get; } = new ObservableCollection<CTLogChildContentVM>();
        public DelegateCommand ImportCsvCmd { get; }

        public override string ReportName => "CTLog统计";
        private IMotionController _mController;
        public CTLogStatContentVM(IRepository reporitory, IMotionController motionController) : base(reporitory, motionController)
        {
            _mController = motionController;
            //_dispatcher = dispatcher;
            ImportCsvCmd = new DelegateCommand(OnImport);
        }

        private void OnImport()
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "CSV files (*.csv)|*.csv"
            };
            if (dlg.ShowDialog() == true)
            {
                foreach (var f in dlg.FileNames)
                    Pages.Add(new CTLogChildContentVM(f, RemovePage));
            }
            void RemovePage(CTLogChildContentVM p) => Pages.Remove(p);
        }

        #region 导航接口
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 离开当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// 进入当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            Query();
        }

        #endregion
    }
}