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
using langs = Luster.Motion.Assests.Langs;

namespace Luster.Motion.EditorUI.Views
{
    /// <summary>
    /// ModuleContent.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleContent : UserControl
    {
        /// <summary>
        /// 右键菜单
        /// </summary>
        private ContextMenu contextMenu;

        public ModuleContent()
        {
            InitializeComponent();
            contextMenu = new ContextMenu();
            
        }

        /// <summary>
        /// 双击添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (args.RightButton == MouseButtonState.Released)
            {
                var border = Luster.Motion.CommonUI.CommonHelper.VisualUpwardSearch<Border>(args.Source as DependencyObject);
                if (border != null && border is Border b)
                {
                    contextMenu.Items.Clear();
                    contextMenu.PlacementTarget = b;
                    var mNode = b.DataContext as ModuleNode;
                    if (mNode.Tag.ToString() == "Composition")
                    {
                        AddRemoveItem(mNode);
                        contextMenu.IsOpen = true;
                    }
                }
            }

            // 1.支持拖拽
            if (args.ClickCount == 1)
            {

            }

            // 2.双击新增
            if (args.ClickCount > 1)
            {

            }
        }

        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddRemoveItem(ModuleNode mNode)
        {
            // 异步
            MenuItem removeItem = new MenuItem();
            removeItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Remove);
            removeItem.Icon = GetFontText("\xe620");
            removeItem.InputGestureText = "Delete";
            BuildCommand(removeItem, "RemoveCommand");
            removeItem.CommandParameter = mNode.Tag1;
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
            txtBlock.Style = this.FindResource("MotionIconSmall") as Style;

            return txtBlock;
        }

        /// <summary>
        /// 通用构建命令方法
        /// </summary>
        /// <param name="item">菜单</param>
        /// <param name="cmdName">命令名称</param>
        private void BuildCommand(MenuItem item, string cmdName)
        {
            BindingOperations.SetBinding(item, MenuItem.CommandProperty,
             new Binding(cmdName)
             {
                 Source = this.DataContext,
             });
        }
    }

    public class ModuleDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = container as FrameworkElement;
            var obj = item as ModuleNode;
            DataTemplate dt = null;
            if (obj != null && fe != null)
            {
                if (obj.Tag.ToString() == "Composition")
                    dt = fe.FindResource("Composition") as DataTemplate;
                else
                    dt = fe.FindResource("Normal") as DataTemplate;

            }
            return dt;
        }
    }
}
