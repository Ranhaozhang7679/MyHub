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
    public class RollSetContentVM : MotionVM, IDockContent
    {
        /// <summary>
        /// 订单集合
        /// </summary>
        private ObservableCollection<RollModel> _rollModels;
        public ObservableCollection<RollModel> RollModels
        {
            get => _rollModels;
            set => SetProperty(ref _rollModels, value);
        }

        private ObservableCollection<string> _stations;
        public ObservableCollection<string> Stations
        {
            get => _stations;
            set => SetProperty(ref _stations, value);
        }

        public static DelegateCommand EditBarcodeCommand { get; set; }
        private Dispatcher _dispatcher;

        public static BarcodeRuleModel _barcodeRuleModel { get; set; }

        /// <summary>
        /// 工单管理
        /// </summary>
        private IRollManager _rollManager;
        private IDeviceEngine _deviceEngine;
        private SFCHelper _sfcHelper = null;
        /// <summary>
        /// 对话框
        /// </summary>
        private IDialogService _dialogService;
        public RollSetContentVM()
        { }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_commonBus"></param>
        /// <param name="orderManger"></param>
        /// <param name="motionController"></param>
        /// <param name="dispatcher"></param>
        public RollSetContentVM(IDialogService dialogServce,ICommonBus _commonBus,IRollManager orderManger,Dispatcher dispatcher, IDeviceEngine deviceEngine) : base(_commonBus)
        {
            _dialogService = dialogServce;
            _rollManager = orderManger;
            _dispatcher = dispatcher;
            _deviceEngine = deviceEngine;
            //_rollManager.OrderUpdateEvent -= OrderManager_FinishEvent;
            //_rollManager.OrderUpdateEvent += OrderManager_FinishEvent;
            EditBarcodeCommand = new DelegateCommand(EditBarcode);
            Stations = new ObservableCollection<string>();
            Stations.Add("左");
            Stations.Add("右");

            
            _sfcHelper = new SFCHelper("");
            // 第一次加载要进行初始化
            InitOrders();
            //第一次加载获取条码设置
            LoadBarcodeSetting();
            //更新列表
            _rollManager.OrderUpdateEvent -= _rollManager_OrderUpdateEvent;
            _rollManager.OrderUpdateEvent += _rollManager_OrderUpdateEvent;



        }
        
        private void _rollManager_OrderUpdateEvent(TbRollInfo obj)
        {
            InitOrders();
        }

        private void OrderManager_FinishEvent(TbRollInfo order)
        {
            //InitOrders();
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            // 配方更新
            bus.GetEvent<RecipeOpenEvent>().Subscribe((r) =>
            {
                InitOrders();
            });
        }


        /// <summary>
        /// 初始化产品数据
        /// </summary>
        private void InitOrders()
        {
            _dispatcher.Invoke(() =>
            {
                RollModels = new ObservableCollection<RollModel>();
                var list = _rollManager.GetOrders();
                int i = 0;
                foreach (var item in list)
                {
                    var surplus = GetUse(item.RollNo);

                    var surplusOrder = GetSurplus(item.RollNo);

                    // 工单余量为0，并且创建时间超过1天，就删除该工单
                    if (surplusOrder == 0 )
                    {
                        _rollManager.RemoveOrder(item.RollNo);
                    }

                    RollModels.Add(new RollModel()
                    {
                        RollNo = item.RollNo,
                       
                        IsFirst = i == 0,
                        RollNum = $"余量 {surplusOrder.ToString()}",
                        Sort = i + 1,
                        Station=item.Sation,
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
                commonBus.OnLog(LogType.Info, $"扫描卷料条码:{BarText}");
                if (RollModels.Count() >= 2)
                {
                    throw new FriendlyException($"最多输入二笔卷料单!");
                }
              
                _sfcHelper = new SFCHelper("");
                _rollManager.ScanOrder(orderNo);
                bool isFirst = RollModels.Count() == 0;
                RollModels.Insert(0, new RollModel()
                {
                    RollNo = orderNo,
                    IsFirst = isFirst,
                    Sort = RollModels.Count() + 1,

                    RollNum = $"余量 {GetSurplus(orderNo)}",
                    Station = "左",
                    CreateDate = DateTime.Now.ToString("yyyy-MM-dd")
                }); ;

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
            if (obj is RollModel model)
            {
                _rollManager.ClearUseNum(model.RollNo);

            }
        }));


        /// <summary>
        /// 删除当前
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (new DelegateCommand<object>((obj) =>
        {
            if (obj is RollModel model)
            {
                _dialogService.ShowConfirm($"确认删除订单:{model.RollNo}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        _rollManager.RemoveOrder(model.RollNo);

                        commonBus.OnLog(LogType.Info, $"卷料单:{model.RollNo} 被删除!");
                        InitOrders();
                    }
                });
            }
        }));

        /// <summary>
        /// 刷新已用
        /// </summary>
        private DelegateCommand<object> _refreshCommand;
        public DelegateCommand<object> RefreshCommand => _refreshCommand ?? (new DelegateCommand<object>((obj) =>
        {
            commonBus.OnLog(LogType.Info, $"卷料余量刷新");
            if (obj is RollModel model)
            {
                model.RollNum = $"余量 {GetSurplus(model.RollNo)}";
            }
        }));

       

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
                surplus = _sfcHelper.QueryHandingLot(orderNo,out string errMsg);
            }
            catch (Exception)
            {
                // 忽略掉访问通信异常
                surplus = -99;
            }

            return surplus;
        }

        /// <summary>
        /// SelectChange
        /// </summary>
        private DelegateCommand<object> _selectchangeCommand;
        public DelegateCommand<object> SelectChangeCommand => _selectchangeCommand ?? (new DelegateCommand<object>((obj) =>
        {
            if (obj is RollModel model)
            {
                _rollManager.UpdateStation(model.RollNo, model.Station);
                InitOrders();
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
        private int GetUse(string orderNo)
        {
            int usenum = 0;
            try
            {
                usenum = _rollManager.GetUse(orderNo);
            }
            catch (Exception)
            {
                usenum = 0;
            }

            return usenum;
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
        /// 卷料数量
        /// </summary>
        private int _totalNum;
        public int TotalNum
        {
            get => _totalNum;
            set { SetProperty(ref _totalNum, value); }
        }

        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "RollSet";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "RollSetContent";
    }

    /// <summary>
    /// 订单
    /// </summary>
    public class RollModel : BindableBase
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
        /// 单号
        /// </summary>
        private string _rollNo = "";
        public string RollNo
        {
            get => _rollNo;
            set { SetProperty(ref _rollNo, value); }
        }

    
        /// <summary>
        /// 订单结果
        /// </summary>
        private string _useNum = "已用 0";
        public string UseNum
        {
            get => _useNum;
            set { SetProperty(ref _useNum, value); }
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
        ///卷料工单余量
        /// </summary>
        private string _orderNum = string.Empty;
        public string OrderNum
        {
            get => _orderNum;
            set { SetProperty(ref _orderNum, value); }
        }
        /// <summary>
        ///数量
        /// </summary>
        private string _rollNum=string.Empty;
        public string RollNum
        {
            get => _rollNum;
            set { SetProperty(ref _rollNum, value); }
        }

        /// <summary>
        /// 工站
        /// </summary>
        private string _station = "";
        public string Station
        {
            get => _station;
            set { SetProperty(ref _station, value); }
        }
    }
}


