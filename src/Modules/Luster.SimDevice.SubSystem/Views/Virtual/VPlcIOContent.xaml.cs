using Luster.SimDevice.SubSystem.ViewModel.Virtual;
using System.Windows.Controls;

namespace Luster.SimDevice.SubSystem.Views.Virtual
{
    /// <summary>
    /// VPlcIOContent.xaml 的交互逻辑
    /// </summary>
    public partial class VPlcIOContent : Border
    {
        public VPlcIOContent()
        {
            InitializeComponent();
        }

        private void InputDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (DataContext is VPlcIOContentVM vm)
            {
                vm.CellEditFinishedCommand.Execute();
            }
        }

        private void OutputDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (DataContext is VPlcIOContentVM vm)
            {
                vm.CellEditFinishedCommand.Execute();
            }
        }
    }
}
