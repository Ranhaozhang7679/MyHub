using Luster.Motion.DigitalSetup.Converters;
using Luster.Motion.DigitalSetup.ViewModel.Validations;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// DataValidationContent.xaml 的交互逻辑
    /// </summary>
    public partial class DataValidationContent : UserControl
    {
        public DataValidationContent()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // 为所有自动生成的列设置双向绑定，使用户编辑能回写到数据模型
            if (e.Column is DataGridBoundColumn boundColumn)
            {
                var binding = boundColumn.Binding as Binding;
                if (binding != null)
                {
                    var newBinding = new Binding(binding.Path.Path)
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    boundColumn.Binding = newBinding;
                }
            }
        }
    }

    /// <summary>
    /// 验证状态到颜色的转换器
    /// </summary>
    public class ValidationStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ValidationStatus status)
            {
                return status switch
                {
                    ValidationStatus.Pass => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // 绿色
                    ValidationStatus.Fail => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // 红色
                    ValidationStatus.Pending => new SolidColorBrush(Color.FromRgb(158, 158, 158)), // 灰色
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
                };
            }
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
