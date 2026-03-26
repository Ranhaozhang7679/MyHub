using Luster.Common.Assets.FloatingInfo.Models;
using Luster.Common.Assets.FloatingInfo.Services;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.DigitalSetup.Views
{
    /// <summary>
    /// IO交替检测对话框的结果
    /// </summary>
    public enum IOAlternatingResult
    {
        /// <summary>
        /// 检测成功（自动检测到交替变化）
        /// </summary>
        OK,
        /// <summary>
        /// 用户跳过
        /// </summary>
        Skip,
        /// <summary>
        /// 用户取消
        /// </summary>
        Cancel,
        Error
    }

    /// <summary>
    /// IO交替检测对话框 - 用于提示用户操作IO并自动检测交替变化
    /// </summary>
    public partial class IOAlternatingCheckDialog : Window
    {
        private VIO _vio;
        private DispatcherTimer _updateTimer;
        private int _changeCount = 0;
        private bool _lastState;
        
        /// <summary>
        /// 对话框结果
        /// </summary>
        public IOAlternatingResult Result { get; private set; } = IOAlternatingResult.Cancel;
        
        /// <summary>
        /// 状态变化次数
        /// </summary>
        public int ChangeCount => _changeCount;

        /// <summary>
        /// 是否检测成功（自动关闭）
        /// </summary>
        public bool IsDetected { get; private set; } = false;

        private readonly IFloatingInfoConfigService _configService;
        private readonly IFloatingInfoService _floatingInfoService;

        public static readonly DependencyProperty ContentItemsProperty =
            DependencyProperty.Register("ContentItems", typeof(ObservableCollection<ContentItem>), typeof(IOAlternatingCheckDialog), new PropertyMetadata(null));

        public ObservableCollection<ContentItem> ContentItems
        {
            get { return (ObservableCollection<ContentItem>)GetValue(ContentItemsProperty); }
            set { SetValue(ContentItemsProperty, value); }
        }

        public IOAlternatingCheckDialog(VIO vio, bool initialState, IFloatingInfoConfigService configService, IFloatingInfoService floatingInfoService)
        {
            InitializeComponent();
            
            _vio = vio;
            _lastState = initialState;
            _configService = configService;
            _floatingInfoService = floatingInfoService;

            LoadFloatingInfo();
            
            // 设置显示文本
            txtMessage.Text = $"请人工操作IO [{vio.Name}]\n\n" +
                             $"请完成以下操作之一：\n" +
                             $"• 如果当前状态为True，请操作IO完成 True→False→True\n" +
                             $"• 如果当前状态为False，请操作IO完成 False→True→False\n\n" +
                             $"检测到交替变化后将自动判定为OK。\n\n" +
                             $"OK后此弹窗自动关闭";
            
            UpdateStateDisplay(initialState);
            
            // 启动定时器更新状态显示
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(50);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
            
            // 窗口居中
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_vio == null) return;
            
            bool currentState = _vio.Behavior == IOBehavior.Input 
                ? _vio.GetDigitalIn() 
                : _vio.GetDigitalOut();
            
            // 检测状态变化
            if (currentState != _lastState)
            {
                _changeCount++;
                _lastState = currentState;
                UpdateStateDisplay(currentState);
                
                txtStatus.Text = $"状态变化次数: {_changeCount}";
                
                // 检查是否完成交替（至少2次变化）
                if (_changeCount >= 2)
                {
                    IsDetected = true;
                    Result = IOAlternatingResult.OK;
                    _updateTimer.Stop();
                    this.Close();
                }
            }
        }

        private void UpdateStateDisplay(bool state)
        {
            txtCurrentState.Text = $"当前状态: {(state ? "True" : "False")}";
            txtCurrentState.Foreground = state 
                ? System.Windows.Media.Brushes.Green 
                : System.Windows.Media.Brushes.Red;
        }

        /// <summary>
        /// 关闭对话框（供外部调用）
        /// </summary>
        public void CloseDialog()
        {
            _updateTimer?.Stop();
            this.Close();
        }

        private void LoadFloatingInfo()
        {
            if (_configService != null && _vio != null)
            {
                var config = _configService.GetConfig(_vio.Name);
                if (config != null && config.ContentItems != null)
                {
                    ContentItems = new ObservableCollection<ContentItem>(config.ContentItems);
                }
                else
                {
                    ContentItems = new ObservableCollection<ContentItem>();
                }
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_floatingInfoService != null && _vio != null)
            {
                _floatingInfoService.OpenSettings(_vio.Name);
                LoadFloatingInfo();
            }
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            Result = IOAlternatingResult.Skip;
            _updateTimer?.Stop();
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = IOAlternatingResult.Cancel;
            _updateTimer?.Stop();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
