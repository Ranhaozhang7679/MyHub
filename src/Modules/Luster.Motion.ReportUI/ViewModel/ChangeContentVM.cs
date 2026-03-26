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
using Luster.Common.DataAccess.Tables;
using Luster.Motion.TaskFlow.Engine;

namespace Luster.Control.Wpf.Motion
{
    public class ChangeContentVM : ReportBaseVM
    {
        /// <summary>
        /// 数据查询
        /// </summary>
        private IRepository _repository;

        /// <summary>
        /// 界面刷新
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reporitory">仓储</param>
        /// <param name="dispatcher"></param>
        public ChangeContentVM(IRepository reporitory,
            IMotionController mController,
            Dispatcher dispatcher) : base(reporitory, mController)
        {
            _repository = reporitory;
            _dispatcher = dispatcher;
            StartTime = DateTime.Now.AddDays(-1);
            EndTime = DateTime.Now.AddDays(1);
            PerPageCount = 30;
            PageIndex = 1;
        }

        /// <summary>
        /// 报表数据
        /// </summary>
        private List<TbChangeRecord> _changeReports;
        public List<TbChangeRecord> ChangeReports
        {
            get { return _changeReports; }
            set { SetProperty(ref _changeReports, value); }
        }

        /// <summary>
        /// 页面切换
        /// </summary>
        /// <param name="obj"></param>
        protected override void PageUpdated(FunctionEventArgs<int> obj)
        {
            Expression<Func<TbChangeRecord, bool>> expression = u => u.CreateTime >= StartTime && u.CreateTime <= EndTime;
            if (!string.IsNullOrEmpty(SearchParas))
            {
                expression = expression.And(u => u.Module.Contains(SearchParas) || u.Content.Contains(SearchParas) || u.Property.Contains(SearchParas)||u.Role.Contains(SearchParas));
            }


            var list = _repository.GetPage<TbChangeRecord>(expression, u => u.ID, obj.Info, PerPageCount, out var total);
            PageCount = (int)(total / PerPageCount) + (total % PerPageCount == 0 ? 0 : 1);
            ChangeReports = list.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Query()
        {
            PageUpdated(new FunctionEventArgs<int>(PageIndex));
        }

        /// <summary>
        /// 变更记录
        /// </summary>
        public override string ReportName => "ChangeRecord";

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            Query();
        }
    }
}
