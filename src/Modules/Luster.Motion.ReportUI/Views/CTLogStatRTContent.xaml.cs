using Luster.Motion.ReportUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Luster.Motion.ReportUI.Views
{
    /// <summary>
    /// CTLogStatRTContent.xaml 的交互逻辑
    /// </summary>
    public partial class CTLogStatRTContent : UserControl
    {
        public CTLogStatRTContent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// DataGrid加载完成事件，同步生成动态列
        /// </summary>
        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                // 监听DataContext变化
                dataGrid.DataContextChanged += DataGrid_DataContextChanged;
                // 监听Tab页的DynamicColumnNames属性变化
                dataGrid.Unloaded += DataGrid_Unloaded;

                // 如果DataContext是CTStatTabPageModel，监听其PropertyChanged事件
                if (dataGrid.DataContext is CTStatTabPageModel tabPage)
                {
                    tabPage.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(CTStatTabPageModel.DynamicColumnNames))
                        {
                            // 当DynamicColumnNames变化时，重新同步列
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                SyncDataGridColumns(dataGrid, tabPage);
                            }), System.Windows.Threading.DispatcherPriority.Normal);
                        }
                    };
                }

                // 延迟同步，确保DataContext已完全设置
                // 使用重试机制处理DataContext绑定延迟问题
                int retryCount = 0;
                System.Windows.Threading.DispatcherTimer timer = null;
                timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(50)
                };
                timer.Tick += (s, args) =>
                {
                    retryCount++;
                    if (TrySyncColumns(dataGrid) || retryCount >= 10)
                    {
                        timer.Stop();
                        timer = null;
                    }
                };
                timer.Start();
            }
        }

        /// <summary>
        /// DataGrid卸载事件
        /// </summary>
        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                dataGrid.DataContextChanged -= DataGrid_DataContextChanged;
                dataGrid.Unloaded -= DataGrid_Unloaded;

                // 取消订阅TabPage的PropertyChanged事件
                if (dataGrid.DataContext is CTStatTabPageModel tabPage)
                {
                    // 注意：这里简化处理，实际使用中应该保存token来取消订阅
                    // 由于每次DataGrid加载都会重新订阅，这里不做特殊处理
                }
            }
        }

        /// <summary>
        /// DataContext变化事件
        /// </summary>
        private void DataGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                // 延迟同步，确保数据已完全加载
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TrySyncColumns(dataGrid);
                }), System.Windows.Threading.DispatcherPriority.Normal);
            }
        }

        /// <summary>
        /// 尝试同步列
        /// </summary>
        /// <returns>是否成功同步</returns>
        private bool TrySyncColumns(DataGrid dataGrid)
        {
            var tabPage = GetDataContextTabPage(dataGrid);
            if (tabPage != null)
            {
                SyncDataGridColumns(dataGrid, tabPage);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取DataGrid对应的Tab页
        /// </summary>
        private CTStatTabPageModel GetDataContextTabPage(DataGrid dataGrid)
        {
            // DataGrid在ContentTemplate中，其DataContext应该是CTStatTabPageModel
            if (dataGrid.DataContext is CTStatTabPageModel tabPage)
            {
                return tabPage;
            }
            return null;
        }

        /// <summary>
        /// DataGrid行加载事件，设置行样式
        /// </summary>
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // 行样式已通过DataGrid.RowStyle中的DataTrigger设置
        }

        /// <summary>
        /// 同步DataGrid的列结构
        /// </summary>
        private void SyncDataGridColumns(DataGrid dataGrid, CTStatTabPageModel tabPage)
        {
            if (dataGrid == null || tabPage == null) return;

            // 获取当前动态列名集合
            var columnNames = tabPage.DynamicColumnNames;
            if (columnNames == null) columnNames = new List<string>();

            // 检查是否需要重新生成列（至少要有5个固定列）
            int currentDynamicColumnCount = dataGrid.Columns.Count - 5; // 减去5个固定列
            if (currentDynamicColumnCount == columnNames.Count && dataGrid.Columns.Count >= 5)
            {
                return; // 列数匹配，无需重新生成
            }

            dataGrid.BeginInit();

            try
            {
                dataGrid.Columns.Clear();

                // ========== 固定列（前5列，通过FrozenColumnCount冻结）==========
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "时间",
                    Binding = new Binding("Time") { Mode = BindingMode.OneWay },
                    Width = new DataGridLength(160, DataGridLengthUnitType.Pixel)
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "SN",
                    Binding = new Binding("SN") { Mode = BindingMode.OneWay },
                    Width = new DataGridLength(70, DataGridLengthUnitType.Pixel)
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "总CT",
                    Binding = new Binding("TotalCT") { Mode = BindingMode.OneWay },
                    Width = new DataGridLength(60, DataGridLengthUnitType.Pixel)
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "净CT",
                    Binding = new Binding("NetCT") { Mode = BindingMode.OneWay },
                    Width = new DataGridLength(60, DataGridLengthUnitType.Pixel)
                });

                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "等待时间",
                    Binding = new Binding("WaitTime") { Mode = BindingMode.OneWay },
                    Width = new DataGridLength(70, DataGridLengthUnitType.Pixel)
                });

                // ========== 动态列（第6列起）==========
                foreach (var columnName in columnNames)
                {
                    // 使用 DataGridTemplateColumn 来正确支持刷新和换行
                    var templateColumn = new DataGridTemplateColumn
                    {
                        Header = columnName,
                        Width = new DataGridLength(85, DataGridLengthUnitType.Pixel)
                    };

                    // 创建 CellTemplate - 使用带条件格式的 TextBlock
                    var factory = new FrameworkElementFactory(typeof(TextBlock));
                    factory.SetBinding(TextBlock.TextProperty, new Binding("DynamicColumns")
                    {
                        Mode = BindingMode.OneWay,
                        Converter = new DynamicColumnValueConverter(),
                        ConverterParameter = columnName
                    });
                    factory.SetBinding(TextBlock.BackgroundProperty, new Binding("DynamicColumns")
                    {
                        Mode = BindingMode.OneWay,
                        Converter = new DynamicColumnBackgroundConverter(),
                        ConverterParameter = columnName
                    });
                    factory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                    factory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    factory.SetValue(TextBlock.MaxWidthProperty, 85.0);
                    factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);

                    templateColumn.CellTemplate = new DataTemplate { VisualTree = factory };
                    dataGrid.Columns.Add(templateColumn);
                }
            }
            finally
            {
                dataGrid.EndInit();
            }
        }
    }

    /// <summary>
    /// 动态列值转换器
    /// 从DynamicColumns集合中查找指定列名的值
    /// </summary>
    public class DynamicColumnValueConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // value是ObservableCollection<DynamicColumnInfo>集合
            if (value is System.Collections.ObjectModel.ObservableCollection<DynamicColumnInfo> columns && parameter is string columnName)
            {
                var col = columns.FirstOrDefault(c => c.ColumnName == columnName);
                return col?.Value ?? "";
            }
            return "";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 动态列背景色转换器
    /// 根据行类型和列名/IsOverTarget返回背景色
    /// - 表头行（RowType=Header）：列名包含"等待"时返回黄色，否则透明
    /// - 数据行（RowType=Data）：IsOverTarget为true且列名不包含"等待"时返回红色，否则透明
    /// </summary>
    public class DynamicColumnBackgroundConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // value是ObservableCollection<DynamicColumnInfo>集合
            if (value is System.Collections.ObjectModel.ObservableCollection<DynamicColumnInfo> columns && parameter is string columnName)
            {
                var col = columns.FirstOrDefault(c => c.ColumnName == columnName);
                if (col != null)
                {
                    // 根据行类型判断逻辑
                    if (col.RowType == CTRowType.Header)
                    {
                        // 表头行：列名包含"等待"时返回黄色
                        if (!string.IsNullOrEmpty(col.ColumnName) && col.ColumnName.Contains("等待"))
                        {
                            return System.Windows.Media.Brushes.Yellow;
                        }
                    }
                    else if (col.RowType == CTRowType.Data)
                    {
                        // 数据行：只有当IsOverTarget为true且列名不包含"等待"时才返回浅红色
                        bool containsWait = !string.IsNullOrEmpty(col.ColumnName) && col.ColumnName.Contains("等待");
                        if (col.IsOverTarget && !containsWait)
                        {
                            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 180, 180));
                        }
                    }
                }
            }
            return System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
