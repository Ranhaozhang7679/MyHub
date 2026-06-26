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
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.ViewModel.Dialogs;

namespace Luster.Motion.CommonUI.Views.Dialogs
{
    /// <summary>
    /// HiveStartDialog2.xaml 的交互逻辑
    /// </summary>
    public partial class HiveStartDialog2Part2 : UserControl
    {
        public HiveStartDialog2Part2()
        {
            InitializeComponent();

            //var edit = GetVisualChild<TextBox>(rateComboBox, "PART_EditableTextBox");
            //if (edit != null)
            //    BindingOperations.SetBinding(edit, TextBox.TextProperty,
            //        new Binding("Rate")
            //        {
            //            Mode = BindingMode.TwoWay,
            //            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //        });
        }
        // 通用递归
        //private static T GetVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(parent, i);
        //        if (child is T t && (child as FrameworkElement)?.Name == name)
        //            return t;
        //        var result = GetVisualChild<T>(child, name);
        //        if (result != null) return result;
        //    }
        //    return null;
        //}

        private void tb_MouseLeave(object sender, MouseEventArgs e)
        {
            Keyboard.ClearFocus();//Keyboard.ClearFocus 方法    清除焦点
        }


        private void tb_MouseLeave(object sender, SelectionChangedEventArgs e)
        {
            Keyboard.ClearFocus();//Keyboard.ClearFocus 方法    清除焦点
        }

        // 使用 DataContext 拿到 VM，安全
        private HiveStartDialog2Part2VM VM => (HiveStartDialog2Part2VM)DataContext;

        // 过滤 + 显示下拉
        private void TbAction_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VM?.ActionLists == null) return;

            var txt = TbAction.Text;
            var filtered = VM.ActionLists
                 .Where(x => x.Desc != null &&
                            x.Desc.IndexOf(txt, StringComparison.OrdinalIgnoreCase) >= 0)
                 .ToList();
            LbAction.ItemsSource = filtered;
            PopAction.IsOpen = filtered.Any();
        }

        // 选中回填
        private void LbAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbAction.SelectedItem is { } item)   // 只要非 null 就继续
            {
                var desc = item.GetType().GetProperty("Desc")?.GetValue(item)?.ToString();
                if (desc != null)
                    VM.ActionTaken = desc;
                PopAction.IsOpen = false;
            }
        }

        // 失焦后关闭
        private async void TbAction_LostFocus(object sender, RoutedEventArgs e)
        {
            await Task.Delay(150);
            PopAction.IsOpen = false;
        }
        private void Arrow_Click(object sender, RoutedEventArgs e)
        {
            // 切换下拉开关
            PopAction.IsOpen = !PopAction.IsOpen;
        }

        private void Arrow_Tool_Click(object sender, RoutedEventArgs e)
        {
            PopTool.IsOpen = !PopTool.IsOpen;
        }

        private void LbTool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbTool.SelectedItem is { } item)
            {
                var desc = item.GetType().GetProperty("Desc")?.GetValue(item)?.ToString();
                if (desc != null)
                    VM.Tool = desc;
                PopTool.IsOpen = false;
            }
        }

        private void Arrow_Setting_Click(object sender, RoutedEventArgs e)
        {
            PopSetting.IsOpen = !PopSetting.IsOpen;
        }

        private void LbSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbSetting.SelectedItem is { } item)
            {
                var desc = item.GetType().GetProperty("Desc")?.GetValue(item)?.ToString();
                if (desc != null)
                    VM.Setting = desc;
                PopSetting.IsOpen = false;
            }
        }

    }
}
