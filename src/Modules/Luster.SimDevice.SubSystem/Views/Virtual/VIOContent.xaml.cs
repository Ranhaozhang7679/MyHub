using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.ViewModel.Virtual;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Luster.SimDevice.SubSystem.Views.Virtual
{
    /// <summary>
    /// VIOContent.xaml 的交互逻辑
    /// </summary>
    public partial class VIOContent
    {
        public VIOContent()
        {
            InitializeComponent();

        }


        private void ListBoxItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void BorderItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(DataContext is VIOContentVM viewModel)
            {
                viewModel.ShowIODialog();
            }
        }

        // 数字量输入右键事件
        private void BorderItem_MouseRightButtonDown1(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is VIOContentVM viewModel)
            {
                viewModel.ShowInPutDialog();
            }
        }

        // 左键选中事件（用于模拟量）
        private void BorderItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is IOModel ioModel)
            {
                var listBoxItem = FindParent<ListBoxItem>(border);
                if (listBoxItem != null)
                {
                    listBoxItem.IsSelected = true;
                }

                ioModel.IsSelected = true;

                if (e.ClickCount == 2)
                {
                    if (DataContext is VIOContentVM viewModel)
                    {
                        if (ioModel.IsIn)
                        {
                            viewModel.ShowInPutDialog();
                        }
                        else
                        {
                            viewModel.ShowIODialog();
                        }
                    }
                }
            }
        }

        // 模拟量输入右键事件
        private void BorderItem_MouseRightButtonDown_AnalogIn(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is IOModel ioModel)
            {
                var listBoxItem = FindParent<ListBoxItem>(border);
                if (listBoxItem != null)
                {
                    listBoxItem.IsSelected = true;
                }
                ioModel.IsSelected = true;

                if (DataContext is VIOContentVM viewModel)
                {
                    viewModel.ShowInPutDialog();
                }

                e.Handled = true;
            }
        }

        // 模拟量输出右键事件
        private void BorderItem_MouseRightButtonDown_AnalogOut(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.DataContext is IOModel ioModel)
            {
                var listBoxItem = FindParent<ListBoxItem>(border);
                if (listBoxItem != null)
                {
                    listBoxItem.IsSelected = true;
                }
                ioModel.IsSelected = true;

                if (DataContext is VIOContentVM viewModel)
                {
                    viewModel.ShowIODialog();
                }

                e.Handled = true;
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }

    /// <summary>
    /// IO 模板选择器
    /// </summary>
    public class ValueTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// 数字输入模板
        /// </summary>
        public DataTemplate DigInTemplate { get; set; }

        /// <summary>
        /// 数字输出模板
        /// </summary>
        public DataTemplate DigOutTemplate { get; set; }

        /// <summary>
        /// 模拟输入模板
        /// </summary>
        public DataTemplate AnaInTemplate { get; set; }

        /// <summary>
        /// 模拟输出模板
        /// </summary>
        public DataTemplate AnaOutTemplate { get; set; }

        /// <summary>
        /// 方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is IOModel model)
            {
                if (model.IsDigital)
                {
                    if (model.IsIn)
                    {
                        return DigInTemplate;
                    }
                    else
                    {
                        return DigOutTemplate;
                    }
                }
                else
                {
                    if (model.IsIn)
                    {
                        return AnaInTemplate;
                    }
                    else
                    {
                        return AnaOutTemplate;
                    }
                }

            }

            return DigInTemplate;
        }
    }

    public class IndexToOneBasedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value + 1;  // 从1开始
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

}
