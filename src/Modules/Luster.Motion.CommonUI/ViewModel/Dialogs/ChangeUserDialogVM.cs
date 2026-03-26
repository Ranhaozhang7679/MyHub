using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.Web;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class ChangeUserDialogVM : MotionDialogVM
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        #region 属性
        /// <summary>
        /// 倒计时
        /// </summary>
        private string _countDown;
        public string CountDown
        {
            get => _countDown;
            set => SetProperty(ref _countDown, value);
        }



        /// <summary>
        /// 倒计时
        /// </summary>
        private string _checkResault;
        public string CheckResault
        {
            get => _checkResault;
            set => SetProperty(ref _checkResault, value);
        }

        /// <summary>
        /// 确认按钮是否可点击
        /// </summary>
        private bool _isButtonEnable;
        public bool IsButtonEnable
        {
            get => _isButtonEnable;
            set => SetProperty(ref _isButtonEnable, value);
        }



        /// <summary>
        /// 离线模式
        /// </summary>
        private bool _offline;
        public bool Offline
        {
            get => _offline;
            set => SetProperty(ref _offline, value);
        }

        /// <summary>
        /// 刷卡背景色        /// </summary>
        private Brush _cardBkColor;
        public Brush CardBkColor
        {
            get { return _cardBkColor; }
            set { SetProperty(ref _cardBkColor, value); }
        }
        /// <summary>
        /// 刷卡提示
        /// </summary>
        private string _cardLog;
        public string CardLog
        {
            get { return _cardLog; }
            set { SetProperty(ref _cardLog, value); }
        }

        /// <summary>
        /// _cardID
        /// </summary>
        private string _cardID;
        public string cardID
        {
            get { return _cardID; }
            set { SetProperty(ref _cardID, value); }
        }

        /// <summary>
        /// _cardID
        /// </summary>
        private string _PassWord;
        public string PassWord
        {
            get { return _PassWord; }
            set { SetProperty(ref _PassWord, value); }
        }

        #endregion





        private IDialogService _dialogService;
        DateTime beginTime;

        public ChangeUserDialogVM(IMotionController motionController, IAuthService authService, IDialogService dialogService, ICommonBus commonBus)
        {
            _dialogService = dialogService;
            this.IsButtonEnable = false;
            cardID = "";
            CheckResault = "未查询";
            Offline = false;
            CountDown = "登录";


        }
        private string currentAuth = "";

        public override void OnDialogOpened(IDialogParameters parameters)
        {

        }


        /// <summary>
        /// 刷卡
        /// </summary>
        private DelegateCommand<object> _scanCommand;
        public DelegateCommand<object> ScanCommand => _scanCommand ?? (new DelegateCommand<object>((obj) =>
        {
            //接收刷卡响应消息
            //var e = obj as KeyEventArgs;
            //if (e != null)
            //{
            //    if (e.Key == Key.Enter)
            //    {
            //        {
            //            if (cardID.Length > 10 || cardID.Length < 6)
            //            {
            //                cardID = "";
            //                return;
            //            }
            //            //进行报文上报
            //            var currentID = cardID;
            //            if (cardID.Substring(0, 1) == "0")
            //                cardID = cardID.Substring(1, cardID.Length - 1);
            //            var ret = sfcHelper.CheckCard_login(cardID, out string auth, out string strReceived);
            //            if (ret)
            //            {
            //                CheckResault = "查询成功";
            //                Offline = false;
            //            }
            //            else
            //            {
            //                CheckResault = "查询失败";
            //                Offline = true;
            //                CardLog = $"ID:{currentID}" + Environment.NewLine +
            //                          $"查询失败" + Environment.NewLine +
            //                          $"SFC 网络错误" + Environment.NewLine +
            //                          $"可尝试离线登录";
            //                IsButtonEnable = true;
            //            }
            //            currentAuth = auth;

            //            if (strReceived.ToUpper().Contains("LUSTER"))
            //            {
            //                if (ret && (auth == "L1" || auth == "L2" || auth == "L3" || auth == "L6" || auth == "L7" || auth == "L8"))
            //                {


            //                    CardLog = $"ID:{currentID}" + Environment.NewLine +
            //                              $"查询权限成功" + Environment.NewLine +
            //                              $"SFC Recv{strReceived}" + Environment.NewLine +
            //                              $"登录成功-模式已切换";


            //                    //CardBkColor = new SolidColorBrush(Color.FromRgb(141, 212, 113));


            //                }
            //                else if (ret && auth == "L7")
            //                {
            //                    //不做动作，等待按钮
            //                }
            //                else
            //                {
            //                    CardLog = $"ID:{currentID}" + Environment.NewLine +
            //                              $"查询权限成功" + Environment.NewLine +
            //                              $"SFC Recv{strReceived}" + Environment.NewLine +
            //                              $"登录失败-无权限";
            //                    //CardBkColor = new SolidColorBrush(Color.FromRgb(225, 83, 88));
            //                }
            //            }

            //        }
            //        //开始倒计时
            //        beginTime = DateTime.Now.AddSeconds(3);
            //        dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            //        dispatcherTimer.Tick += Timer_Tick;
            //        dispatcherTimer.Start();
            //    }
            //    else
            //    {
            //        //记录数据
            //        if (e.Key.ToString().Contains('D'))
            //        {
            //            cardID += e.Key.ToString()[1];
            //        }
            //    }
            //}
        }));

        private void Timer_Tick(object sender, EventArgs e)
        {
            CountDown = "登录 " + ":" + (beginTime - DateTime.Now).Seconds.ToString();
            if ((beginTime - DateTime.Now).TotalSeconds <= 0)
            {
                dispatcherTimer.Stop();
                base.CloseDialog("ok");
            }
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        private DelegateCommand<object> _clickCommand;
        public DelegateCommand<object> ClickCommand => _clickCommand ?? (_clickCommand = new DelegateCommand<object>((o) =>
        {
            
        }));

        protected override void CloseDialog(string parameter)
        {
            base.CloseDialog(parameter);
            dispatcherTimer.Stop();
        }

    }
}
