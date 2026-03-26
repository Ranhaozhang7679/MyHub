using Luster.Motion.EditorUI.Models;
using Luster.Motion.SubSystem.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Motion.SubSystem.Views
{
    public partial class DeviceDialogWindow : Window
    {
        public GlobalVar SelectedGlobalVariable { get; private set; }

        public System.Collections.Generic.List<GlobalVar> SelectedGlobalVariables { get; private set; }

        public DeviceDialogWindow()
        {
            InitializeComponent();
            this.DataContext = new DeviceDialogViewModel();
            SelectedGlobalVariables = new System.Collections.Generic.List<GlobalVar>();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as DeviceDialogViewModel;
            if (viewModel?.HasSelectedVariables == true)
            {
                // 获取所有选中的变量
                SelectedGlobalVariables = viewModel.SelectedVariables.ToList();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("请至少选择一个变量", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as DeviceDialogViewModel;
            viewModel?.OnSelectionChanged();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as DeviceDialogViewModel;
            viewModel?.OnSelectionChanged();
        }
    }
}