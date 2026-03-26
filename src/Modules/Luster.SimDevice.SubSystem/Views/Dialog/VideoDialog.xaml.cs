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

namespace Luster.SimDevice.SubSystem.Views.Dialog
{
    /// <summary>
    /// VideoDialog.xaml 的交互逻辑
    /// </summary>
    public partial class VideoDialog : UserControl
    {
        private Point _pressedPosition;

        public VideoDialog()
        {
            InitializeComponent();
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            Window win = ((UserControl)sender).Parent as Window;
            if (win != null)
            {
                win.WindowStyle = WindowStyle.None;
                win.ResizeMode = ResizeMode.NoResize;

            }
        }

        private void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _pressedPosition = e.GetPosition(this);
        }

        private void ViewPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Window win = ((UserControl)sender).Parent as Window;
            if (win != null)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed && _pressedPosition != e.GetPosition(this))
                {
                    win.DragMove();
                }
            }
        }
    }

}
