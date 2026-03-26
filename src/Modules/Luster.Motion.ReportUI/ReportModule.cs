#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ReportModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.ReportUI
* 文 件 名:       ReportModule.cs
* 创建时间:       2022/6/28 9:48:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3279c5c4-3dae-480a-9308-01dc7ff5530f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/28 9:48:00
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Tools;
using Luster.Control.Wpf.Motion;
using Luster.Motion.ReportUI.ViewModel;
using Luster.Motion.ReportUI.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YourNs.ViewModels;

namespace Luster.Motion.ReportUI
{
    public class ReportModule : IModule
    {
        /// <summary>
        /// View和VM对应的字典信息
        /// </summary>
        private Dictionary<Type, Type> ViewAndVM = new Dictionary<Type, Type>();

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ReportRegion", typeof(ProductReportContent));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ReportContent, ReportContentVM>();
            //containerRegistry.RegisterForNavigation<CTReportContent, CTReportContentVM>();
            containerRegistry.RegisterForNavigation<ProductReportContent, ProductReportContentVM>();
            containerRegistry.RegisterForNavigation<FlingMaterialContent, FlingMaterialContentVM>();
            containerRegistry.RegisterForNavigation<ChangeContent, ChangeContentVM>();
            containerRegistry.RegisterForNavigation<TaikeContent, TaikeContentVM>();
            containerRegistry.RegisterForNavigation<CTLogStatContent, CTLogStatContentVM>();
            LoadReportDlls();

            foreach (var item in ViewAndVM)
            {
                // 视图注册
                ViewModelLocationProvider.Register(item.Key.ToString(), item.Value);
                containerRegistry.RegisterForNavigation(item.Key, item.Key.Name);
            }
        }

        /// <summary>
        /// 动态加载报表的dlls
        /// </summary>
        private void LoadReportDlls()
        {
            ViewAndVM = new Dictionary<Type, Type>();
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
                        ViewAndVM.Add(viewType, vmType);
                    }
                }
            }
        }
    }
}