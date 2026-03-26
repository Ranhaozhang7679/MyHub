using HandyControl.Data;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveChartsCore.Kernel;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.EditorUI;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static FreeSql.Internal.GlobalFilter;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 参数导入确认
    /// </summary>
    public class ParamConfirmContentVM : BaseAss    //BindableBase
    {
        // 新增3个按钮和1个进度条的定义
        private double _progressValue;
        private readonly IWebService _webService;

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        public ParamConfirmContentVM(IRepository repository, IRegionManager regionManager, ICommonBus commonBus, CSVHelper cSVHelper, IWebService webService, FlowBus flowBus) : base(repository, regionManager, commonBus, cSVHelper, flowBus)
        {

            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "ObtainSwVersion", IsSelected = true, Region = "", ViewType = typeof(AssTbSwVersion) });
            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);
            this._webService = webService;
        }

        public override void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }

        public override void OnOneKeyCheck(object obj)
        {
            // 子界面的一键点检逻辑
            try
            {
                ProgressValue = 0; // 进度
                
                // 获取 WebConfig 信息
                var webConfig = _webService.GetConfig() as WebConfig;
                
                for (int i = 0; i < ItemModels.Count; i++)
                {
                    if (ItemModels[i] is AssTbSwVersion swVersion)
                    {
                        // 检查标准是否为空
                        if (string.IsNullOrWhiteSpace(swVersion.标准))
                        {
                            swVersion.状态 = "格式错误";
                            continue;
                        }
                        
                        // 根据项次从 WebConfig 获取实测值
                        string actualVersion = GetVersionFromWebConfig(webConfig, swVersion.项次);
                        swVersion.实测 = actualVersion;
                        
                        // 比较标准和实测
                        if (swVersion.标准.Trim() == swVersion.实测.Trim())
                        {
                            swVersion.状态 = "OK";
                        }
                        else
                        {
                            swVersion.状态 = "NG";
                        }
                        
                        swVersion.完成时间 = DateTime.Now;
                        ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                    }
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"获取软件版本失败" });
            }
            finally
            {
                ProgressValue = 100;
            }
        }
        
        /// <summary>
        /// 根据项次名称从 WebConfig 获取对应版本
        /// </summary>
        private string GetVersionFromWebConfig(WebConfig webConfig, string itemName)
        {
            if (webConfig == null) return "NN.NN";
            
            return itemName switch
            {
                "平台版本" => webConfig.SoftVersion ?? "NN.NN",
                "配方版本" => webConfig.ReciepeVersion ?? "NN.NN",
                "PLC版本" => webConfig.PlcVersion ?? "NN.NN",
                "视觉版本" => webConfig.VisionVersion ?? "NN.NN",
                "机器人版本" => webConfig.RobotVersion ?? "NN.NN",
                "激光版本" => webConfig.LaserVersion ?? "NN.NN",
                _ => "NN.NN"
            };
        }

        private void OnUpdateItems()
        {
            ItemModels.Clear();
            try
            {
                switch (SelectedReportPage.Name)
                {
                    case "ObtainSwVersion":
                        // 从 CSV 文件读取标准数据
                        long totalCount = 0;
                        var items = _csvHelper.GetAllDataNew1<AssTbSwVersion>(0, 0, out totalCount);
                        foreach (var item in items)
                        {
                            item.实测 = "";
                            item.状态 = "";
                            ItemModels.Add(item);
                        }
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
