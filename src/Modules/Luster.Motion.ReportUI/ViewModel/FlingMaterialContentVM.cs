#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlingMaterialContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.ViewModel
* 文 件 名:       FlingMaterialContentVM.cs
* 创建时间:       2022/12/14 13:08:56
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      d864658c-a34e-4038-957c-751c7a77aa11
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/14 13:08:56
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using HandyControl.Data;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.Tools;
using Luster.Motion.TaskFlow.Engine;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.ReportUI.ViewModel
{
    internal class FlingMaterialContentVM : ReportBaseVM
    {
        public override string ReportName => "FlingMaterialStatistics";

        /// <summary>
        /// CT信息
        /// </summary>
        private ObservableCollection<TbThrow> _itemModels;

        public ObservableCollection<TbThrow> ItemModels
        {
            get { return _itemModels; }
            set { SetProperty(ref _itemModels, value); }
        }


        public FlingMaterialContentVM(IRepository reporitory, IMotionController motionController) : base(reporitory, motionController)
        {
            Init();
            InitModels();
        }

        /// <summary>
        /// 页面内容更新
        /// </summary>
        /// <param name="obj"></param>
        protected override void PageUpdated(FunctionEventArgs<int> obj)
        {
            var index = obj.Info;
            long count = 0;
            var startTime = StartTime.AddHours(FirstClassTime.Hour).AddMinutes(FirstClassTime.Minute);
            var endTime = EndTime.AddHours(FirstClassTime.Hour).AddMinutes(FirstClassTime.Minute);
            var infos = new List<TbThrow>();
            if (string.IsNullOrEmpty(SearchParas))
            {
                infos = reporitory.GetPage<TbThrow>(x => x.CreateTime > startTime && x.CreateTime < endTime,
                          x => x.ID, index, PerPageCount, out count).ToList();
            }
            else
            {
                infos = reporitory.GetPage<TbThrow>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.Station.Contains(SearchParas),
                          x => x.ID, index, PerPageCount, out count).ToList();
            }
            if (infos != null && infos.Count > 0)
            {
                ItemModels = new ObservableCollection<TbThrow>(infos);            
                if (count > 0)
                {
                    PageCount = (int)(count % PerPageCount == 0 ? count / PerPageCount : count / PerPageCount + 1);
                }
                else
                {
                    PageCount = 1;
                }
            }
            else
            {
                ItemModels = new ObservableCollection<TbThrow>();
            }
        }

        private void InitModels()
        {
            PageUpdated(new FunctionEventArgs<int>(1));
            PageIndex = 1;
        }

        protected override void Query()
        {
            InitModels();
        }

        #region 导航接口
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
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
        }

        #endregion
    }
}
