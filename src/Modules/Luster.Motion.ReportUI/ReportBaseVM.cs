#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ReportVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.ReportUI
* 文 件 名:       ReportVM.cs
* 创建时间:       2022/10/17 15:48:54
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      87307d38-b1e0-4eb0-acd3-daf3ab41b81e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/17 15:48:54
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Data;
using Luster.Common.DataAccess.Repositories;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.ReportUI
{
    /// <summary>
    /// 报表的VM对象
    /// </summary>
    public abstract class ReportBaseVM : BindableBase, INavigationAware
    {
        /// <summary>
        /// 报表的名称
        /// </summary>
        public abstract string ReportName { get; }

        protected DateTime FirstClassTime = DateTime.Now;

        /// <summary>
        /// 数据库访问
        /// </summary>
        protected IRepository reporitory;

        /// <summary>
        /// 控制器
        /// </summary>
        protected IMotionController motionController;

        #region 数据选择条件
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value); }
        }

        /// <summary>
        /// 筛选条件
        /// </summary>
        private string _searchParas;
        public string SearchParas
        {
            get { return _searchParas; }
            set { SetProperty(ref _searchParas, value); }
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _iPhoneStartTime;
        public DateTime IPhoneStartTime
        {
            get { return _iPhoneStartTime; }
            set { SetProperty(ref _iPhoneStartTime, value); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _iPhoneEndTime;
        public DateTime IPhoneEndTime
        {
            get { return _iPhoneEndTime; }
            set { SetProperty(ref _iPhoneEndTime, value); }
        }

        #endregion

        #region 分页参数
        /// <summary>
        /// 每页数量
        /// </summary>
        private int _perPageCount;

        public int PerPageCount
        {
            get { return _perPageCount; }
            set { SetProperty(ref _perPageCount, value); }
        }

        /// <summary>
        /// 页数
        /// </summary>
        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }

        //跳转页码
        private int _pageIndex;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }
        #endregion

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand { get; set; }

        public DelegateCommand QueryCommand { get; set; }

        public DelegateCommand ExportCommand { get; set; }

        public DelegateCommand ImportCommand { get; set; }

        public DelegateCommand ImportTotalCommand { get; set; }

        public ReportBaseVM()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reporitory"></param>
        public ReportBaseVM(IRepository reporitory, IMotionController motionController)
        {
            this.reporitory = reporitory;
            this.motionController = motionController;
            PageUpdatedCommand = new DelegateCommand<FunctionEventArgs<int>>(PageUpdated);
            QueryCommand = new DelegateCommand(Query);
            ExportCommand = new DelegateCommand(Export);
            ImportCommand = new DelegateCommand(Import);
            ImportTotalCommand = new DelegateCommand(ImportTotal);
        }


        /// <summary>
        /// 事件注册
        /// </summary>
        /// <param name="bus">事件</param>
        protected virtual void RegisterEvent(IEventAggregator bus)
        {

        }

        /// <summary>
        ///  初始化ViewModel
        /// </summary>
        protected virtual void Init()
        {
            PerPageCount = 20;
            PageCount = 1;
            var curClass = motionController.SysConfig.GetCurrentClass(DateTime.Now);
            FirstClassTime = curClass.StartTime;
            StartTime = curClass.StartTime;
            EndTime = curClass.EndTime;
        }

        /// <summary>
        /// 导出
        /// </summary>
        protected virtual void Export()
        {

        }

        /// <summary>
        ///  查询
        /// </summary>
        protected virtual void Query()
        {

        }

        /// <summary>
        /// 导出
        /// </summary>
        protected virtual void Import()
        {

        }
        /// <summary>
        /// 导出
        /// </summary>
        protected virtual void ImportTotal()
        {

        }

        /// <summary>
        /// 页面更新
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void PageUpdated(FunctionEventArgs<int> obj)
        {

        }

        #region 导航接口
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 离开当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// 进入当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            var classInfo = motionController.SysConfig.GetCurrentClass(DateTime.Now);
            if (classInfo != null)
            {
                StartTime = classInfo.StartTime;
                EndTime = classInfo.EndTime;
            }
        }
        #endregion
    }

    /// <summary>
    /// 对应的视图
    /// </summary>
    public interface IReportBaseView
    {
    }
}