using Luster.Motion.DataStruct.DataModels;
using System.Windows.Threading;
using System.Windows;
using System;

namespace Luster.Motion.DigitalSetup.Views
{
    // 结果枚举
    public enum IOCheckResult { OK, NG, Skip }

    // 弹窗Window
    public partial class IOCheckDialog : Window
    {
        private DispatcherTimer _timer;
        private VIO _vio;
        private bool _currentLevel;
        public IOCheckResult Result { get; private set; } = IOCheckResult.Skip;

        // 是否点击了按钮
        public bool IsButtonClicked { get; private set; } = false;
        //public bool IsCancelled => !IsButtonClicked;  

        // 添加计数器字段
        private int _toggleCount = 0;  
        private const int MaxToggleCount = 20;

        public IOCheckDialog(VIO vio, bool initialLevel, int ioIndex)
        {
            InitializeComponent();

            _vio = vio;
            _currentLevel = initialLevel;

            // 设置显示文本
            txtMessage.Text = $"请选择第【{ioIndex}】个IO({vio.Name})的人工检查结果{Environment.NewLine}请注意，关闭本窗口将直接退出本次点检！";

            // 窗口居中
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _toggleCount = 0;

            // 启动定时器，每秒切换一次
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, args) =>
            {
                if (_toggleCount >= MaxToggleCount)
                {
                    _timer.Stop();
                    txtMessage2.Text = $"已达到单个输出IO最大设置次数：{MaxToggleCount}次 ";
                    return;
                }
                _currentLevel = !_currentLevel;
                _vio.SetDigital(_currentLevel);
                _toggleCount++;
            };
            _timer.Start();
        }

        // OK按钮
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Result = IOCheckResult.OK;
            IsButtonClicked = true;
            Close();
        }

        // NG按钮
        private void BtnNG_Click(object sender, RoutedEventArgs e)
        {
            Result = IOCheckResult.NG;
            IsButtonClicked = true;  // 关键：标记为已点击按钮
            Close();
        }

        // 跳过按钮
        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            Result = IOCheckResult.Skip;
            IsButtonClicked = true;
            Close();
        }

        // 窗口关闭时停止定时器
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
        }
    }
}
