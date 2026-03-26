using Luster.Motion.SubSystem.ViewModel;
using Luster.Motion.TaskFlow.Engine.Models;
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

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// SoftConfigureContent.xaml 的交互逻辑
    /// </summary>
    public partial class SoftConfigureContent : UserControl
    {
        private ContextMenu contextMenu;
        public SoftConfigureContent()
        {
            InitializeComponent();
            contextMenu = new ContextMenu();
            contextMenu.PlacementTarget = listmode;
            listmode.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(listmode_PreviewMouseRightButtonDown), true);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is SoftConfigureContentVM vModel)
            {
                TextBox txt=e.Source as TextBox;
                if (txt != null)
                {
                    if (txt.IsKeyboardFocused)
                    {
                        vModel.IsSave = true;
                    }
                }     
            }
        }

        /// <summary>
        /// TreeProject右击方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listmode_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contextMenu.Visibility = Visibility.Visible;

            var item = listmode.Items;
            if (item.Count == 0) return;
            if (item.Count > 0)
            {
                contextMenu.Items.Clear();
                RunModeModel mode = listmode.SelectedItem as RunModeModel;

                if (mode != null)
                {
                    if (mode != null)
                    {
                        DeleteMode(mode);
                    }
                }
                contextMenu.IsOpen = true;
            }
        }


        /// <summary>
        /// 删除配方
        /// </summary>
        private void DeleteMode(RunModeModel mode)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "删除模式";
            menuItem.CommandParameter = mode;
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"DeleteModeCommand")
               {
                   Source = listmode.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }

    }
}
