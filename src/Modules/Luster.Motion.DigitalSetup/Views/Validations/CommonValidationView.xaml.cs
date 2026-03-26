using Luster.Motion.DigitalSetup.ViewModel.Validations;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Motion.DigitalSetup.Views.Validations
{
    /// <summary>
    /// CommonValidationView.xaml 的交互逻辑
    /// </summary>
    public partial class CommonValidationView : UserControl
    {
        public CommonValidationView()
        {
            InitializeComponent();
            this.Loaded += CommonValidationView_Loaded;
        }

        private void CommonValidationView_Loaded(object sender, RoutedEventArgs e)
        {
            // View加载完成后，通知ViewModel更新配置内容
            if (this.DataContext is CommonValidationVM viewModel)
            {
                viewModel.OnViewLoaded();
            }
        }
    }
}
