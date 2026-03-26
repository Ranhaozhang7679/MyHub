using Luster.Motion.DataStruct.DataModels;
using System.Windows.Threading;
using System.Windows;
using System;

namespace Luster.Motion.DigitalSetup.Views
{
    // 结果枚举
    public enum IOCheckResultIn { OK, NG, Skip }

    // 弹窗Window
    public partial class IOCheckDialogIn : Window
    {
        
        private VIO _vio1;
        private bool _currentLevel1;
        public IOCheckResult Result1 { get; private set; } = IOCheckResult.Skip;

        // 是否点击了按钮
        public bool IsButtonClicked1 { get; private set; } = false;
        //public bool IsCancelled => !IsButtonClicked;  

        

        public IOCheckDialogIn(VIO vio1, bool initialLevel, int ioIndex)
        {
            InitializeComponent();

            _vio1 = vio1;
            _currentLevel1 = initialLevel;

            // 设置显示文本
            txtMessage.Text = $"请选择第【{ioIndex}】个IO({vio1.Name})的人工检查结果{Environment.NewLine}请注意，关闭本窗口将直接退出本次点检！";

            // 窗口居中
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
        }

        public void CloseDialog()
        {
            this.Close();
        }
        


        // NG按钮
        private void BtnNG_ClickIn(object sender, RoutedEventArgs e)
        {
            Result1 = IOCheckResult.NG;
            IsButtonClicked1 = true;  // 关键：标记为已点击按钮
            Close();
        }

        // 跳过按钮
        private void BtnSkip_ClickIn(object sender, RoutedEventArgs e)
        {
            Result1 = IOCheckResult.Skip;
            IsButtonClicked1 = true;
            Close();
        }

        
    }
}
