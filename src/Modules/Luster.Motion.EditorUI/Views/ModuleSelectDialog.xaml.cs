using Luster.TaskFlow.Motion;
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
using System.Windows.Shapes;
using Luster.Motion.EditorUI.ViewModel;


namespace Luster.Motion.EditorUI.Views
{
    public partial class ModuleSelectDialog : Window
    {
        public List<ModuleInfo> SelectedModules { get; private set; } = new List<ModuleInfo>();

        public ModuleSelectDialog(List<ModuleInfo> modulesInfo)
        {
            InitializeComponent();
            ModulesDataGrid.ItemsSource = modulesInfo;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedModules = ModulesDataGrid.SelectedItems.Cast<ModuleInfo>().ToList();
            if (SelectedModules.Count == 0)
            {
                System.Windows.MessageBox.Show("请至少选择一个模块", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            ModulesDataGrid.SelectAll();
        }
    }
}

