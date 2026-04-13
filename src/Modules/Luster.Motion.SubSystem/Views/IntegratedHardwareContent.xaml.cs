using System.Windows.Controls;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// IntegratedHardwareContent.xaml 的交互逻辑
    /// </summary>
    public partial class IntegratedHardwareContent : UserControl
    {
        public IntegratedHardwareContent()
        {
            InitializeComponent();
            this.Loaded += IntegratedHardwareContent_Loaded;
        }

        private void IntegratedHardwareContent_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 触发默认导航：导航到第一个配置项（驾驶舱配置）
            if (DataContext is ViewModel.IntegratedHardwareContentVM vm)
            {
                vm.NavigateToDefault();
            }
        }
    }
}
