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

namespace Luster.Motion.EditorUI.Views
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class FlowContent : UserControl
    {
        public FlowContent()
        {
            InitializeComponent();
            this.Loaded += FlowContent_Loaded;
        }

        private void FlowContent_Loaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetIsFocusScope(this, true);
            Keyboard.Focus(this);
        }
    }
}
