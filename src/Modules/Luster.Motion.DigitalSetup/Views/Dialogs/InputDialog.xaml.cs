using System.Windows;
using System.Windows.Controls;

namespace Luster.Motion.DigitalSetup.Views.Dialogs
{
    public partial class InputDialog : UserControl
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 显示输入对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">提示消息</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="owner">所有者窗口</param>
        /// <returns>用户输入的文本，如果取消则返回null</returns>
        public static string ShowDialog(string title, string message, string defaultValue = "", Window owner = null)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = owner ?? Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Content = new InputDialog()
            };

            var inputDialog = (InputDialog)dialog.Content;
            inputDialog.MessageTextBlock.Text = message;
            inputDialog.InputTextBox.Text = defaultValue;

            string result = null;

            inputDialog.ConfirmButton.Click += (s, e) =>
            {
                result = inputDialog.InputTextBox.Text;
                dialog.Close();
            };

            inputDialog.CancelButton.Click += (s, e) => dialog.Close();

            dialog.ShowDialog();

            return result;
        }
    }
}