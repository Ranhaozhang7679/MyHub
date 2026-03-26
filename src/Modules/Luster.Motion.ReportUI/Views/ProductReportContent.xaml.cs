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
    public partial class ProductReportContent : UserControl
    {
        public ProductReportContent()
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
                    string header = viewModel.DataHeaders[i];

                    if (header.Contains("图片") || header.Contains("图像") || header.Contains("Image") || header.Contains("路径"))
                    {
                        // 使用模板列，显示为超链接
                        var column = new DataGridTemplateColumn
                        {
                            Width = DataGridLength.Auto,
                            MinWidth = 80,
                            Header = header,
                            CellTemplate = CreateHyperlinkTemplate(header)
                        };
                        DgReport.Columns.Add(column);
                    }
                    else
                    {
                        // 普通文本列
                        var column = new DataGridTextColumn()
                        {
                            Width = DataGridLength.Auto,
                            MinWidth = 80,
                            CanUserSort = false,
                            Header = header,
                            Binding = new Binding($"Data[{header}]"),
                            ElementStyle = (Style)this.FindResource("DataColumnTextElementStyleCenter"),
                        };
                        DgReport.Columns.Add(column);
                    }
                }
            }
        }

        private DataTemplate CreateHyperlinkTemplate(string header)
        {
            var template = new DataTemplate();

            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            textBlockFactory.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.Blue));
            textBlockFactory.SetValue(TextBlock.CursorProperty, System.Windows.Input.Cursors.Hand);
            textBlockFactory.SetValue(TextBlock.TextDecorationsProperty, null);

            // 绑定文本
            var binding = new Binding($"Data[{header}]");
            textBlockFactory.SetBinding(TextBlock.TextProperty, binding);

            textBlockFactory.AddHandler(TextBlock.MouseLeftButtonDownEvent,
                new MouseButtonEventHandler((sender, e) =>
                {
                    var tb = sender as TextBlock;
                    if (tb != null)
                    {
                        string filePath = tb.Text;
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            try
                            {
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                                }
                                else
                                {
                                    string directory = System.IO.Path.GetDirectoryName(filePath);
                                    if (!string.IsNullOrEmpty(directory) && System.IO.Directory.Exists(directory))
                                    {
                                        System.Diagnostics.Process.Start("explorer.exe", directory);
                                    }
                                    else
                                    {
                                        MessageBox.Show(
                                            $"文件不存在：\n{filePath}",
                                            "提示",
                                            MessageBoxButton.OK,
                                            MessageBoxImage.Warning);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    $"打开失败：{ex.Message}",
                                    "错误",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                    }
                }));

            template.VisualTree = textBlockFactory;
            return template;
        }
    }
}