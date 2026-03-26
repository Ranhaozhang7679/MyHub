using Luster.Motion.SubSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// UserDefineMainContent.xaml 的交互逻辑
    /// </summary>
    public partial class ProReportContent : Border
    {
        public ProReportContent()
        {
            InitializeComponent();

            if (DataContext is ProReportContentVM viewModel)
            {
                viewModel.DataHeaderChangedEvent -= ViewModel_DataHeaderChangedEvent;
                viewModel.DataHeaderChangedEvent += ViewModel_DataHeaderChangedEvent;
                AddDataColumns(viewModel);
            }
        }

        private void ViewModel_DataHeaderChangedEvent()
        {
            Dispatcher.Invoke(() =>
            {
                if (DataContext is ProReportContentVM viewModel)
                {
                    // 先移除添加的项
                    while (dgProduct.Columns.Count > 8)
                    {
                        dgProduct.Columns.RemoveAt(8);
                    }

                    AddDataColumns(viewModel);
                }
            });
        }

        private void AddDataColumns(ProReportContentVM viewModel)
        {
            if (viewModel.DataHeaders != null && viewModel.DataHeaders.Count > 0)
            {
                for (int i = 0; i < viewModel.DataHeaders.Count; i++)
                {
                    var column = new DataGridTextColumn()
                    {
                        Width = 100,
                        MinWidth = 80,
                        Header = viewModel.DataHeaders[i],
                        Binding = new Binding($"Data[{viewModel.DataHeaders[i]}]"),
                        ElementStyle = (Style)this.FindResource("DataColumnTextElementStyleCenter"),
                    };
                    dgProduct.Columns.Add(column);
                }
            }
        }

        
    }
}
