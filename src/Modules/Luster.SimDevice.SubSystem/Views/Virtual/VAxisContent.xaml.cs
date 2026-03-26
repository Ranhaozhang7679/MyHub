using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.ViewModel.Virtual;
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

namespace Luster.SimDevice.SubSystem.Views.Virtual
{
    /// <summary>
    /// VAxisContent.xaml 的交互逻辑
    /// </summary>
    public partial class VAxisContent
    {
        private ContextMenu contextMenu;
        int homeIndex = 0;
        int commonIndex = 0;
        int techpostion = 0;
        public VAxisContent()
        {
            InitializeComponent();
            contextMenu = new ContextMenu();
            contextMenu.PlacementTarget = JogUp;
            contextMenu.PlacementTarget = JogDown;
            contextMenu.PlacementTarget = Home;
            // 注册鼠标左键事件
            this.dgPosition.MouseLeftButtonUp += new MouseButtonEventHandler(dgHome_MouseLeftButtonUp);
            JogUp.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(JogUp_PreviewMouseRightButtonDown), true);
            JogDown.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(JogDown_PreviewMouseRightButtonDown), true);
            Home.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(HomeZero_PreviewMouseRightButtonDown), true);
        }

        private void dgHome_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is VAxisContentVM viewModel)
            {
                // 拖拽后,归零设备排序
                viewModel.SortPositions();
            }
        }

        private void JogUp_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contextMenu.Visibility = Visibility.Visible;
            contextMenu.Items.Clear();
            JogUP();
            contextMenu.IsOpen = true;

        }

        private void JogUP()
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "配置";
            menuItem.CommandParameter = "Jog+";
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"JogCommand")
               {
                   Source = JogUp.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }

        private void JogDown_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contextMenu.Visibility = Visibility.Visible;
            contextMenu.Items.Clear();
            JogDOWN();
            contextMenu.IsOpen = true;

        }

        private void JogDOWN()
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "配置";
            menuItem.CommandParameter = "Jog-";
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"JogCommand")
               {
                   Source = JogDown.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }

        private void HomeZero_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contextMenu.Visibility = Visibility.Visible;
            contextMenu.Items.Clear();
            HomeZero();
            contextMenu.IsOpen = true;

        }

        private void HomeZero()
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "配置";
            menuItem.CommandParameter = "HomeZero";
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"JogCommand")
               {
                   Source = JogDown.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is VAxisContentVM viewModel)
            {
                viewModel.Confirm();
            }
        }

        /// <summary>
        /// 常规参数双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            Point aP = e.GetPosition(datagrid);
            IInputElement obj = datagrid.InputHitTest(aP);
            DependencyObject target = obj as DependencyObject;
            if (target is DataGridRow)
            {
                if (DataContext is VAxisContentVM viewModel)
                {
                    viewModel.ConfirmUpdate();
                }
            }
            else
            {
                if (DataContext is VAxisContentVM viewModel)
                {
                    if (viewModel.IsAllowUpdate)
                    {
                        viewModel.ConfirmUpdate();
                    }
                }
            }
            target = VisualTreeHelper.GetParent(target);


        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var grid=sender as DataGrid;
            if (DataContext is VAxisContentVM viewModel)
            {
                if (grid.SelectedIndex == homeIndex) return;
                homeIndex =grid.SelectedIndex;
                if (!viewModel.IsAllowHome)
                {
                    viewModel.IsAllowHome = true;
                }

            }
        }

        private void DataGrid_SelectedCellsChanged_1(object sender, SelectedCellsChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (DataContext is VAxisContentVM viewModel)
            {
                if (grid.SelectedIndex == commonIndex) return;
                commonIndex = grid.SelectedIndex;
                if (!viewModel.IsAllowUpdate)
                {
                    viewModel.IsAllowUpdate = true;
                }

            }
        }

        private void dgPosition_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (DataContext is VAxisContentVM viewModel)
            {
                if (grid.SelectedIndex == techpostion) return;
                techpostion = grid.SelectedIndex;
                viewModel.IsRead = false;

            }
        }
    }
}
