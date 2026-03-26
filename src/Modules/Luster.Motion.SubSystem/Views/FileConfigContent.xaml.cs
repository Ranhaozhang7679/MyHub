using Luster.Motion.SubSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// FileConfigContent.xaml 的交互逻辑
    /// </summary>
    public partial class FileConfigContent : UserControl
    {
        public FileConfigContent()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is FileConfigContentVM vModel)
            {
                TextBox txt = e.Source as TextBox;
                if (txt != null)
                {
                    if (txt.IsKeyboardFocused)
                    {
                        vModel.IsSave = true;
                    }
                }
            }
        }
    }
}
