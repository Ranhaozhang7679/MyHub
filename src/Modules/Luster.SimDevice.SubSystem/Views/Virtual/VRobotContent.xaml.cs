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

namespace Luster.SimDevice.SubSystem.Views.Virtual
{
    /// <summary>
    /// VRobotContent.xaml 的交互逻辑
    /// </summary>
    public partial class VRobotContent
    {
        public VRobotContent()
        {
            InitializeComponent();
        }

        //private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        //{
        //    var grid = sender as DataGrid;
        //    if (DataContext is VRobotContentVM viewModel)
        //    {
        //        viewModel.CurrentPointIndex = grid.SelectedIndex + 1;
        //    }
        //}
    }
}
