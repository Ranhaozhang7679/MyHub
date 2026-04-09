using System.Windows.Controls;

namespace Luster.Motion.DigitalSetup.Views.Dialogs
{
    /// <summary>
    /// 文本输入对话框
    /// </summary>
    public partial class TextInputDialog : UserControl
    {
        public TextInputDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => InputTextBox.Focus();
        }
    }
}