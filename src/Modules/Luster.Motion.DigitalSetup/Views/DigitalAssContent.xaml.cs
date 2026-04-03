using Luster.Motion.DigitalSetup.Converters;
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

namespace Luster.Motion.DigitalSetup.Views
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class DigitalAssContent : UserControl
    {
        public DigitalAssContent()
        {
            InitializeComponent();
            this.Loaded += FlowContent_Loaded;
        }

        private void FlowContent_Loaded(object sender, RoutedEventArgs e)
        {
            FocusManager.SetIsFocusScope(this, true);
            Keyboard.Focus(this);
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "状态") // 只针对"状态"列
            {
                DataGridTextColumn column = e.Column as DataGridTextColumn;
                if (column != null)
                {
                    // 创建样式并绑定转换器
                    var cellStyle = new Style(typeof(DataGridCell));
                    cellStyle.Setters.Add(
                        new Setter(
                            DataGridCell.BackgroundProperty,
                            new Binding("状态") { Converter = new StatusToBackgroundConverter() }
                        )
                    );
                    column.CellStyle = cellStyle;
                }
            }
            if (string.Equals(e.Column.Header.ToString(), "标准", StringComparison.OrdinalIgnoreCase))
            {
                e.Column.IsReadOnly = true;
            }
            // 其他列不做处理，保持默认

        }
    }
}
