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
using Prism.Mvvm;
using Luster.TaskFlow.Motion.Interfaces;
using System.Windows.Input;
using Luster.Motion.CommonUI.Dock;
using System.Diagnostics;
using Luster.Common.Assets;
using Luster.Common.DataStruct.Interfaces;
using System.Xml.Linq;
using Luster.Motion.CommonUI.ViewModel.Dialogs;
using Luster.Common.DataStruct;
using System.Text.RegularExpressions;
using Luster.Motion.Integration.SFC;
using Luster.SimDevice.Engine;
using Luster.Motion.DataStruct.VDevice;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 首页缓存报表
    /// </summary>
    public class ProOrderContentVM : MotionVM, IDockContent
    {
        /// <summary>
        /// 订单集合
        /// </summary>
        private ObservableCollection<OrderModel> _orderModels;
        public ObservableCollection<OrderModel> OrderModels
        {
            get => _orderModels;
            set => SetProperty(ref _orderModels, value);
        }

        public static DelegateCommand EditBarcodeCommand { get; set; }
        private Dispatcher _dispatcher;

        public static BarcodeRuleModel _barcodeRuleModel { get; set; }

        /// <summary>
        /// 工单管理
        /// </summary>
        private IOrderManager _orderManager;
        private IDeviceEngine _deviceEngine;
        private SFCHelper _sfcHelper = null;
        /// <summary>
        /// 对话框
        /// </summary>
        private IDialogService _dialogService;
        public ProOrderContentVM()
        { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_commonBus"></param>
        /// <param name="orderManger"></param>
        /// <param name="motionController"></param>
        /// <param name="dispatcher"></param>
        public ProOrderContentVM(
            IDialogService dialogServce,
            ICommonBus _commonBus,
            IOrderManager orderManger,
            IDeviceEngine deviceEngine,
            Dispatcher dispatcher) : base(_commonBus)
        {
            _dialogService = dialogServce;
            _orderManager = orderManger;
            _dispatcher = dispatcher;
            _deviceEngine = deviceEngine;
            _orderManager.OrderUpdateEvent -= OrderManager_FinishEvent;
            _orderManager.OrderUpdateEvent += OrderManager_FinishEvent;
            EditBarcodeCommand = new DelegateCommand(EditBarcode);

           

            // 第一次加载要进行初始化
            InitOrders();
            //第一次加载获取条码设置
            LoadBarcodeSetting();

        }

        

        private void OrderManager_FinishEvent(TbWorkOrder order)
        {
            InitOrders();
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            // 配方更新
            bus.GetEvent<RecipeOpenEvent>().Subscribe((r) =>
            {
                InitOrders();
                _sfcHelper = new SFCHelper("");
            });
        }


        /// <summary>
        /// 初始化产品数据
        /// </summary>
        private void InitOrders()
        {
            _dispatcher.Invoke(() =>
            {
                OrderModels = new ObservableCollection<OrderModel>();
                var list = _orderManager.GetOrders();
                int i = 0;
                foreach (var item in list)
                {
                    var surplus = GetSurplus(item.OrderNo);

                    // 工单余量为0，并且创建时间超过1天，就删除该工单
                    if (surplus == 0 && Math.Abs((item.CreateTime - DateTime.Now).Days) > 1)
                    {
                        _orderManager.RemoveOrder(item.OrderNo);
                    }

                    OrderModels.Add(new OrderModel()
                    {
                        OrderNo = item.OrderNo,
                        OrderData = $"余量 {surplus}",
                        IsFirst = i == 0,
                        Sort = i + 1,
                        CreateDate = item.CreateTime.ToString("yyyy-MM-dd")
                    });
                    i++;
                }
            });
        }

        /// <summary>
        /// 扫码
        /// </summary>
        private DelegateCommand<object> _scanCommand;
        public DelegateCommand<object> ScanCommand => _scanCommand ?? (new DelegateCommand<object>((obj) =>
        {
            if (obj is KeyEventArgs rArgs && rArgs.Source is TextBox txt && (/*rArgs.Key == Key.D6 ||*/ rArgs.Key == Key.Enter))
            {
                string orderNo = BarText.TrimEnd('\r', '\n');
                if (string.IsNullOrEmpty(orderNo) || orderNo.Length < 2) { return; }
                if (_barcodeRuleModel.CheckLength && orderNo.Length != _barcodeRuleModel.Length)
                {
                    throw new FriendlyException($"{orderNo}条码长度不匹配,长度要求为{_barcodeRuleModel.Length}");
                    //return;
                }
                if (_barcodeRuleModel.CheckExpression && Regex.Match(orderNo, _barcodeRuleModel.Expression).Success)
                {
                    throw new FriendlyException($"{orderNo}条码规则不匹配,正则表达式为{_barcodeRuleModel.Expression}");
                }
                if (_barcodeRuleModel.CheckLength) { }
                commonBus.OnLog(LogType.Info, $"扫描工单条码:{BarText}");
                if (OrderModels.Count() == 1)
                {
                    throw new FriendlyException($"最多输入一笔工单!");
                }
                _orderManager.ScanOrder(orderNo);
                //var vCom = _deviceEngine.GetVDevices<VCommuncation>();
                //foreach (var cffu in vCom)
                //{
                //    if (cffu.Name.ToUpper() == "PDCA")
                //    {
                //        cffu.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
                //        cffu.Open();
                //        _sfcHelper = new SFCHelper(cffu);
                //    }
                //}
                //if (_sfcHelper == null)
                //{
                //    _sfcHelper = new SFCHelper();
                //}
                _sfcHelper = new SFCHelper("");
                bool isFirst = OrderModels.Count() == 0;
                OrderModels.Insert(0, new OrderModel()
                {
                    OrderNo = orderNo,
                    IsFirst = isFirst,
                    Sort = OrderModels.Count() + 1,
                    OrderData = $"余量 {GetSurplus(orderNo)}",
                    CreateDate = DateTime.Now.ToString("yyyy-MM-dd")
                });

                // 清空
                BarText = "";
            }
        }));

        /// <summary>
        /// 激活当前
        /// </summary>
        private DelegateCommand<object> _activeCommand;
        public DelegateCommand<object> ActiveCommand => _activeCommand ?? (new DelegateCommand<object>((obj) =>
        {
            if (obj is OrderModel model)
            {
                _orderManager.ActiveOrder(model.OrderNo);
                commonBus.OnLog(LogType.Info, $"工单:{model.OrderNo} 被激活!");
                InitOrders();
            }
        }));


        /// <summary>
        /// 删除当前
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (new DelegateCommand<object>((obj) =>
        {
            if (obj is OrderModel model)
            {
                _dialogService.ShowConfirm($"确认删除订单:{model.OrderNo}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        _orderManager.RemoveOrder(model.OrderNo);
                        commonBus.OnLog(LogType.Info, $"工单:{model.OrderNo} 被删除!");
                        InitOrders();
                    }
                });
            }
        }));

        /// <summary>
        /// 删除当前
        /// </summary>
        private DelegateCommand<object> _refreshCommand;
        public DelegateCommand<object> RefreshCommand => _refreshCommand ?? (new DelegateCommand<object>((obj) =>
        {
            if (obj is OrderModel model)
            {
                model.OrderData = $"余量 {GetSurplus(model.OrderNo)}";
            }
        }));

        private void LoadBarcodeSetting()
        {
            if (commonBus.BarConfig != null)
            {
                commonBus.BarConfig.LoadBarcodeConfig();
                _barcodeRuleModel = commonBus.BarConfig.Barcodes;
            }
        }


        private void EditBarcode()
        {
            DialogParameters param = new DialogParameters();
            EditBarcodeVM._rulemodel = _barcodeRuleModel;
            param.Add("Title", Luster.Motion.Assests.Langs.LangProvider.GetLang("EditBarcode"));
            _dialogService.ShowDialog("EditBarcodeDialog", param, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var barCode = r.Parameters.GetValue<BarcodeRuleModel>("Barcode");
                    _barcodeRuleModel = barCode;
                    commonBus.BarConfig.Barcodes = barCode;
                    commonBus.BarConfig.SaveBarcodeConfig();
                }
            });
        }

        /// <summary>
        /// 工单余量查询
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private int GetSurplus(string orderNo)
        {
            int surplus = 0;
            try
            {
                surplus = _sfcHelper.GetOrderSurplus(orderNo, out string errMsg);
            }
            catch (Exception)
            {
                // 忽略掉访问通信异常
                surplus = -99;
            }

            return surplus;
        }



        /// <summary>
        /// 工单
        /// </summary>
        private string _text = "";
        public string BarText
        {
            get => _text;
            set { SetProperty(ref _text, value); }
        }

        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "WorkOrder";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "ProOrderContent";
    }

    /// <summary>
    /// 订单
    /// </summary>
    public class OrderModel : BindableBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        private int _sort = 1;
        public int Sort
        {
            get => _sort;
            set { SetProperty(ref _sort, value); }
        }


        /// <summary>
        /// 订单
        /// </summary>
        private bool _isFirst = false;
        public bool IsFirst
        {
            get => _isFirst;
            set { SetProperty(ref _isFirst, value); }
        }

        /// <summary>
        /// 订单
        /// </summary>
        private string _orderNo = "";
        public string OrderNo
        {
            get => _orderNo;
            set { SetProperty(ref _orderNo, value); }
        }


        /// <summary>
        /// 订单结果
        /// </summary>
        private string _orderData = "余量 0";
        public string OrderData
        {
            get => _orderData;
            set { SetProperty(ref _orderData, value); }
        }

        /// <summary>
        /// 创建日期
        /// </summary>
        private string _createDate = "2023-01-01";
        public string CreateDate
        {
            get => _createDate;
            set { SetProperty(ref _createDate, value); }
        }

        /// <summary>
        /// 订单数量
        /// </summary>
        private int _orderNum;
        public int OrderNum
        {
            get => _orderNum;
            set { SetProperty(ref _orderNum, value); }
        }
    }
}


