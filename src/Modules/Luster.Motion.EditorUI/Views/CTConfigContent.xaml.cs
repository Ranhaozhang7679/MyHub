using Luster.Motion.CommonUI.Models;
using Luster.Motion.EditorUI.Models;
using Luster.TaskFlow.Motion.Logic;
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
using langs = Luster.Motion.Assests.Langs;

namespace Luster.Motion.EditorUI.Views
{
    /// <summary>
    /// FormulaContent.xaml 的交互逻辑
    /// </summary>
    public partial class CTConfigContent : UserControl
    {
        /// <summary>
        /// 右键菜单
        /// </summary>
        private ContextMenu contextMenu;

        //右键菜单
        public CTConfigContent()
        {
            InitializeComponent();

            contextMenu = new ContextMenu();
            contextMenu.PlacementTarget = treeFlow;

            treeFlow.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(TreeViewItem_PreviewMouseRightButtonDown), true);
        }

        /// <summary>
        /// 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var sItems = treeFlow.SelectedItems;
            if (sItems.Count == 0) return;

            contextMenu.Items.Clear();
            var context = treeFlow.DataContext;
            var moduleCT = sItems[0] as ModuleCT;

            if (moduleCT.Tag != null)
            {
                var function = moduleCT.Tag.TaskFunction;
                if (function is not IStation && function.Name != "WaitStatus")
                {
                    AddToWaitModule(context, moduleCT);
                    contextMenu.IsOpen = true;
                }
            }
        }


        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddToWaitModule(object dataContext, ModuleCT mCt)
        {
            // 添加
            MenuItem removeItem = new MenuItem();
            removeItem.Header = langs.LangProvider.GetLang(langs.LangKeys.WaitStatus);
            removeItem.Icon = GetFontText("\xe6b4");

            // 命令绑定
            BindingOperations.SetBinding(removeItem, MenuItem.CommandProperty,
              new Binding($"ToWaitCommand")
              {
                  Source = dataContext,
              });

            removeItem.CommandParameter = mCt;

            contextMenu.Items.Add(removeItem);
        }

        /// <summary>
        /// 构造字体图标
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private TextBlock GetFontText(string icon)
        {
            TextBlock txtBlock = new TextBlock();
            txtBlock.Text = icon;
            txtBlock.Style = this.FindResource("IconSmall") as Style;
            return txtBlock;
        }

        private void txtCT_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox txt = sender as TextBox;
            txt.Background = Brushes.White;
        }

        private void txtCT_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBox txt = sender as TextBox;
            txt.Background = Brushes.Transparent;
        }
    }
}
