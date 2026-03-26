using Luster.Motion.ReportUI.ViewModel;
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

namespace Luster.Motion.ReportUI.Views
{
    /// <summary>
    /// ProductReportContent.xaml 的交互逻辑
    /// </summary>
    public partial class TestContent : UserControl
    {
        public TestContent()
        {
            InitializeComponent();
            if (DataContext is ProductReportContentVM viewModel)
            {
                AddDataColumns(viewModel);
            }
        }

        private void AddDataColumns(ProductReportContentVM viewModel)
        {
            if (viewModel.DataHeaders != null && viewModel.DataHeaders.Count > 0)
            {
                for (int i = 0; i < viewModel.DataHeaders.Count; i++)
                {
                    var column = new DataGridTextColumn()
                    {
                        Width = DataGridLength.Auto,
                        MinWidth = 80,
                        CanUserSort = false,
                        Header = viewModel.DataHeaders[i],
                        Binding = new Binding($"Data[{viewModel.DataHeaders[i]}]"),
                        ElementStyle = (Style)this.FindResource("DataColumnTextElementStyleCenter"),
                    };
                    DgReport.Columns.Add(column);
                }
            }
        }
    }
}
