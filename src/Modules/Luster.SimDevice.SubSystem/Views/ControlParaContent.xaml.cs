using Luster.SimDevice.SubSystem.ViewModel;
using Luster.SimDevice.SubSystem.ViewModel.Virtual;
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

namespace Luster.SimDevice.SubSystem.Views
{
    /// <summary>
    /// ControlParaContent.xaml 的交互逻辑
    /// </summary>
    public partial class ControlParaContent : UserControl
    {
        public ControlParaContent()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            Point aP = e.GetPosition(datagrid);
            IInputElement obj = datagrid.InputHitTest(aP);
            DependencyObject target = obj as DependencyObject;

            if (DataContext is ControlParaVM viewModel)
            {
                viewModel.ConfirmUpdate();
                datagrid.IsReadOnly = !viewModel.IsAllowUpdate;
            }
           target = VisualTreeHelper.GetParent(target);
        }




    }
}
