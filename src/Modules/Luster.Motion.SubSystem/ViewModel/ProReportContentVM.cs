#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UserDefineMainContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       UserDefineMainContentVM.cs
* 创建时间:       2022/9/6 15:46:33
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      d8a7bc81-f833-423e-8960-603956805ecb
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/6 15:46:33
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Control.Wpf.Motion.Controls;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Luster.Motion.DataStruct;
using Luster.SimDevice.SubSystem.Events;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 首页缓存报表
    /// </summary>
    public class ProReportContentVM : MotionVM
    {
        /// <summary>
        /// 记录产品序号
        /// </summary>
        private int productIndex = 0;

        /// <summary>
        /// 列表头动态变化事件
        /// </summary>
        public event Action DataHeaderChangedEvent;

        /// <summary>
        /// 输出数据名称
        /// </summary>
        private List<string> _dataheaders;
        public List<string> DataHeaders
        {
            get { return _dataheaders; }
            set
            {
                if (_dataheaders != value)
                {
                    _dataheaders = value;
                    DataHeaderChangedEvent?.Invoke();
                }
            }
        }

        /// <summary>
        /// 数据库访问
        /// </summary>
        private IDbManager _dbManager;

        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// UI线程
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 产品信息
        /// </summary>
        private ObservableCollection<ProductItemModel> _productInfos;
        public ObservableCollection<ProductItemModel> ProductInfos
        {
            get => _productInfos;
            set => SetProperty(ref _productInfos, value);
        }

        public ProReportContentVM(ICommonBus _commonBus, IDbManager dbManager,
            IMotionController motionController, Dispatcher dispatcher) : base(_commonBus)
        {
            _dbManager = dbManager;
            _mController = motionController;
            _dispatcher = dispatcher;

            // 产品事件
            _mController.ProductEvent -= MotionController_ProductEvent;
            _mController.ProductEvent += MotionController_ProductEvent;

            _mController.ProStatEvent -= _mController_ProStatEvent;
            _mController.ProStatEvent += _mController_ProStatEvent;
            ProductInfos = new ObservableCollection<ProductItemModel>();

            // 初始化
            SetDataHeader();
            InitProducts();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void _mController_ProStatEvent(ProductStat pStatus, ImpactData arg2)
        {
            // 产能清零
            if (pStatus.AllCount == 0)
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ProductInfos.Clear();
                    productIndex = 0;
                }));
            }
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            // 更新表头
            bus.GetEvent<RecipeOpenEvent>().Subscribe((r) =>
            {
                SetDataHeader();
                InitProducts();
            });

            // 事件注册
            bus.GetEvent<MapDataEvent>().Subscribe((mData) =>
            {
                SetDataHeader();
            });
        }

        /// <summary>
        /// 初始化产品数据
        /// </summary>
        private void InitProducts()
        {
            var products = _dbManager.GetLast50Products();
            int i = products.Count();
            productIndex = i;

            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                foreach (var item in products)
                {
                    var model = new ProductItemModel();
                    model.Index = i;
                    model.Result = item.Result == "OK";
                    model.EnterTime = item.EnterTime;
                    model.OutTime = item.OutTime;
                    model.JigCode = item.Jig;
                    model.ProCode = item.SNCode;
                    model.IsToss = item.IsToss;
                    model.CT = item.CT;
                    model.NewCT =item.NewCT;
                    model.NgReason = item.NgReason;

                    var dicData = item.GetDataStr();
                    foreach (var header in DataHeaders)
                    {
                        if (!dicData.ContainsKey(header))
                        {
                            dicData.Add(header, "");
                        }
                    }

                    model.Data = dicData;

                    ProductInfos.Add(model);
                    i--;
                }
            }));

            // 获取产品良率
            var proYield = _dbManager.GetProYield();

            if (proYield != null)
            {
                _mController.SetProductInfo(new ProductStat()
                {
                    AllCount = proYield.AllCount,
                    OKCount = proYield.OKCount,
                    NGCount = proYield.NGCount,
                    ThrowMaterialDic = proYield.GetThrowMaterial()
                });
            }
        }

        /// <summary>
        /// 跳转页面
        /// </summary>
        private DelegateCommand<string> _NavigateCommand;
        public DelegateCommand<string> NavigateCommand => _NavigateCommand ?? (new DelegateCommand<string>((regionName) =>
        {
            commonBus.OnNavigate(new PageModel() { Region = regionName });
        }));

        private DataGrid datagrid;
        /// <summary>
        /// 
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (new DelegateCommand<object>((obj) =>
        {
            if (obj is System.Windows.RoutedEventArgs args && args.Source is DataGrid dg)
            {
                datagrid = dg;
            }
        }));

        /// <summary>
        /// 动态更新表头
        /// </summary>
        private void SetDataHeader()
        {
            _dispatcher.Invoke(() =>
            {
                DataHeaders = commonBus.GetMapDatas().GroupBy(x => x.Alias).Select(x => x.Key).ToList();
                ProductInfos.Clear();
            });
        }

        /// <summary>
        /// 插入产品列表
        /// </summary>
        /// <param name="pInfo">产品信息</param>
        /// <param name="enter">来料标识 true出料 false 入料</param>
        private void MotionController_ProductEvent(ProductInfo pInfo, bool enter, double ct)
        {
            if (pInfo == null) return;
            if (!enter)
            {
                return;
            }

            var dicData = new Dictionary<string, object>();
            foreach (var header in DataHeaders)
            {
                if (!dicData.ContainsKey(header))
                {
                    dicData.Add(header, "");
                }
            }

            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (ProductInfos.Count > 99)
                {
                    ProductInfos.RemoveAt(99);
                }

                productIndex++;

                // 根据输出结果转换类型
                var model = new ProductItemModel();
                model.Result = pInfo.Result.Result;
                model.IsToss = pInfo.Result.IsToss;
                model.EnterTime = pInfo.EnterTime;
                model.CT = pInfo.GetCT();
                model.NewCT = ct;
                model.OutTime = pInfo.OutTime;
                model.JigCode = pInfo.Result.JigCode;
                model.ProCode = pInfo.Result.ProCode;
                model.Index = productIndex;
                model.NgReason = pInfo.Result.ErrMsg;

                if (pInfo.Data != null && pInfo.Data.Count > 0)
                {
                    foreach (var data in pInfo.Data)
                    {
                        if (dicData.ContainsKey(data.Key))
                        {
                            dicData[data.Key] = data.Value;
                        }
                    }
                }
                // Need to do
                // 如果为空怎么办
                model.Data = dicData;
                ProductInfos.Insert(0, model);

                // 滚动到最新的一条
                datagrid.ScrollIntoView(model);
            }));
        }
    }
}
