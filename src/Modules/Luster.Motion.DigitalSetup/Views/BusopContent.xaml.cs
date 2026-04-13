using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.Views
{
    /// <summary>
    /// BusopContent.xaml 的交互逻辑
    /// </summary>
    public partial class BusopContent : UserControl
    {
        private bool _isDragging;
        private System.Windows.Point _dragStart;

        public BusopContent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 鼠标滚轮缩放图片
        /// </summary>
        private void ImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is ViewModel.BusopContentVM vm && vm.HasSheetImage)
            {
                if (e.Delta > 0)
                    vm.ZoomInCommand.Execute(null);
                else
                    vm.ZoomOutCommand.Execute(null);

                e.Handled = true;
            }
        }

        private void ImageContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModel.BusopContentVM vm && vm.HasSheetImage)
            {
                _isDragging = true;
                _dragStart = e.GetPosition(sender as System.Windows.IInputElement);
                ((System.Windows.IInputElement)sender).CaptureMouse();
                e.Handled = true;
            }
        }

        private void ImageContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ((System.Windows.IInputElement)sender).ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void ImageContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && DataContext is ViewModel.BusopContentVM vm)
            {
                var pos = e.GetPosition(sender as System.Windows.IInputElement);
                vm.OffsetX += pos.X - _dragStart.X;
                vm.OffsetY += pos.Y - _dragStart.Y;
                _dragStart = pos;
                e.Handled = true;
            }
        }
    }
}