using Luster.Motion.CommonUI.Models;
using Luster.Motion.EditorUI.Models;
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
    /// ProjectContent.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectContent : UserControl
    {
        private ContextMenu contextMenu;
        public ProjectContent()
        {
            InitializeComponent();
            contextMenu = new ContextMenu();
            contextMenu.PlacementTarget = TreeProject;
            TreeProject.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(TreeProject_PreviewMouseRightButtonDown), true);
        }

        /// <summary>
        /// TreeProject右击方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeProject_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            contextMenu.Visibility = Visibility.Visible;

            var item = TreeProject.Items;
            if (item.Count == 0) return;
            if (item.Count > 0)
            {
                contextMenu.Items.Clear();
                Recipe recipe = TreeProject.SelectedItem as Recipe;

                if (recipe != null)
                {
                    if (recipe != null)
                    {
                        DeleteRecipe(recipe);
                        ActiveRecipe(recipe);
                        OPenRecipePath(recipe);
                        OPenSlnPath(recipe);
                        ReceipeSaveAs(recipe);
                    }
                }
                contextMenu.IsOpen = true;
            }
        }

        /// <summary>
        /// 删除配方
        /// </summary>
        private void DeleteRecipe(Recipe recipe)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "删除配方";
            menuItem.CommandParameter = recipe;
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"DeleteRecipeCommand")
               {
                   Source = TreeProject.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }

        /// <summary>
        /// 激活配方
        /// </summary>
        private void ActiveRecipe(Recipe recipe)
        {
            //TreeProject.SelectedItem = recipe;
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "激活配方";
            menuItem.CommandParameter = recipe;
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"ActiveRecipeCommand")
               {
                   Source = TreeProject.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }

        /// <summary>
        /// 打开配方路径
        /// </summary>
        private void OPenRecipePath(Recipe recipe)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "打开配方路径";
            menuItem.CommandParameter = recipe;
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"OpenRecipePathCommand")
               {
                   Source = TreeProject.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }
        /// <summary>
        /// 打开工程路径
        /// </summary>
        private void OPenSlnPath(Recipe recipe)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "打开工程路径";
            menuItem.CommandParameter = recipe;
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"OpenSlnPathCommand")
               {
                   Source = TreeProject.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }

        /// <summary>
        /// 配方另存为
        /// </summary>
        private void ReceipeSaveAs(Recipe recipe)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "配方另存为";
            menuItem.CommandParameter = recipe;
            BindingOperations.SetBinding(menuItem, MenuItem.CommandProperty,
               new Binding($"ReceipeSaveAsCommand")
               {
                   Source = TreeProject.DataContext,
               });
            contextMenu.Items.Add(menuItem);
        }


    }
}
