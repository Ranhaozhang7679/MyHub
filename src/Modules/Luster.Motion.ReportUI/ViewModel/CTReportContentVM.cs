#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CTReportContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.ViewModel
* 文 件 名:       CTReportContentVM.cs
* 创建时间:       2022/10/17 16:17:59
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      b5aa5a55-e903-4ce7-8e0b-b472d26d5dc2
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/17 16:17:59
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using HandyControl.Data;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.Tools;
using Luster.Motion.ReportUI.Model;
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
    internal class CTReportContentVM : ReportBaseVM
    {
        public override string ReportName => "CTStatistics";

        /// <summary>
        /// CT信息
        /// </summary>
        private ObservableCollection<CTInfoModel> _ctItemModels;

        public ObservableCollection<CTInfoModel> CtItemModels
        {
            get { return _ctItemModels; }
            set { SetProperty(ref _ctItemModels, value); }
        }

        public CTReportContentVM() : base()
        {

        }

        public CTReportContentVM(IRepository reporitory, IMotionController motionController) : base(reporitory, motionController)
        {
            Init();
            InitCtModels();
        }


        private void InitCtModels()
        {
            PageUpdated(new FunctionEventArgs<int>(1));
            PageIndex = 1;
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
            var cTInfos = new List<TbCTInfo>();
            if (string.IsNullOrEmpty(SearchParas))
            {
                cTInfos = reporitory.GetPage<TbCTInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime,
                          x => x.ID, index, PerPageCount, out count).ToList();
            }
            else
            {
                cTInfos = reporitory.GetPage<TbCTInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.SNCode.Contains(SearchParas),
                          x => x.ID, index, PerPageCount, out count).ToList();
            }
            if (cTInfos != null && cTInfos.Count > 0)
            {
                CtItemModels = new ObservableCollection<CTInfoModel>();
                foreach (var ct in cTInfos)
                {
                    var model = new CTInfoModel()
                    {
                        Id = ct.ID,
                        StationName = ct.StationName,
                        ModuleName = ct.ModuleName,
                        CtTime = Math.Round(ct.CtTime, 3),
                        CreateTime = ct.CreateTime,
                        StandardCtTime = Math.Round(ct.StandardCTTime, 3),
                        SnCode = ct.SNCode,
                        Differ = Math.Round(ct.Differ, 3),
                        StartTime = ct.StartTime.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        EndTime = ct.EndTime.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        XSpd = ct.XSpd,
                        XAcc = ct.XAcc,
                        XSpdTarget = ct.XSpdTarget,
                        XAccTarget = ct.XAccTarget,


                        //XDec = ct.XDec,
                        //XDistance = ct.XDistance,
                        YSpd = ct.YSpd,
                        YAcc = ct.YAcc,
                        YSpdTarget = ct.YSpdTarget,
                        YAccTarget = ct.YAccTarget,
                        //YDec = ct.YDec,
                        //YDistance = ct.YDistance,
                        ZSpd = ct.ZSpd,
                        ZSpdTarget = ct.ZSpdTarget,
                        ZAcc = ct.ZAcc,
                        ZAccTarget = ct.ZAccTarget,
                        //ZDec = ct.ZDec,
                        //ZDistance = ct.ZDistance,
                        USpd = ct.USpd,
                        //UDec = ct.UDec,
                        UAcc = ct.UAcc,
                        USpdTarget = ct.USpdTarget,
                        UAccTarget = ct.UAccTarget,
                        //UDistance = ct.UDistance,

                        RSpd = ct.RSpd,
                        RAcc = ct.RAcc,
                        RSpdTarget = ct.RSpdTarget,
                        RAccTarget = ct.RAccTarget,

                        Delay = ct.Delay,
                    };
                    CtItemModels.Add(model);
                }
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
                CtItemModels = new ObservableCollection<CTInfoModel>();
            }
        }


        protected override void Query()
        {
            base.Query();
            InitCtModels();
        }


        protected override void Export()
        {
            Luster.WindowsAPICodePack.Dialogs.CommonOpenFileDialog fileDialog = new Luster.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();

            fileDialog.IsFolderPicker = true;
            if (fileDialog.ShowDialog() == WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                var path = fileDialog.FileName;
                var startTime = StartTime.AddHours(FirstClassTime.Hour).AddMinutes(FirstClassTime.Minute);
                var endTime = EndTime.AddHours(FirstClassTime.Hour).AddMinutes(FirstClassTime.Minute);
                SaveCTToCSVFile(startTime, endTime, SearchParas, path);
            }
        }

        private void SaveCTToCSVFile(DateTime startTime, DateTime endTime, string paras, string path)
        {
            // 异步导出文件，不卡顿UI线程
            Task.Run(() =>
            {
                //orm 拼接条件 查询                
                Dictionary<string, List<TbCTInfo>> ctInfos = new Dictionary<string, List<TbCTInfo>>();
                if (motionController.SysConfig.CtFileType == DataStruct.Enums.CtFileType.Product)
                {
                    if (!string.IsNullOrEmpty(paras))
                    {
                        ctInfos = reporitory.GetList<TbCTInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.SNCode.Contains(paras),
                            x => x.ID).GroupBy(x => x.SNCode).ToDictionary(x => x.Key, x => x.ToList());
                    }
                    else
                    {
                        ctInfos = reporitory.GetList<TbCTInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime,
                            x => x.ID).GroupBy(x => x.SNCode).ToDictionary(x => x.Key, x => x.ToList());
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(paras))
                    {
                        ctInfos = reporitory.GetList<TbCTInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.SNCode.Contains(paras),
                            x => x.ID).GroupBy(x => x.StationName).ToDictionary(x => x.Key, x => x.ToList());
                    }
                    else
                    {
                        ctInfos = reporitory.GetList<TbCTInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime,
                            x => x.ID).GroupBy(x => x.StationName).ToDictionary(x => x.Key, x => x.ToList());
                    }
                }

                foreach (var item in ctInfos)
                {
                    var fileName = Path.Combine(path, item.Key + ".csv");
                    CSVTool.SaveCSV<TbCTInfo>(item.Value, fileName, Encoding.UTF8);
                }
            });
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
