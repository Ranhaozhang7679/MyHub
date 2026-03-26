using Luster.Common.Assets.Converter;
using Luster.Common.DataStruct;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
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

namespace Luster.Motion.SubSystem.Views
{
    /// <summary> 
    /// LogContent.xaml 的交互逻辑
    /// </summary>
    public partial class WorkFlowContent : UserControl
    {
        /// <summary>
        /// 右键菜单
        /// </summary>
        private ContextMenu contextMenu;

        public WorkFlowContent()
        {
            InitializeComponent();

            contextMenu = new ContextMenu();
            contextMenu.PlacementTarget = flowEditor;
            // 右键菜单
            flowEditor.AddHandler(FlowEditor.ItemSelectedEvent, new RoutedEventHandler(MouseDownRight), true);
            flowEditor.AddHandler(FlowEditor.LineSelectedEvent, new RoutedEventHandler(LineDownRight), true);
            flowEditor.AddHandler(FlowEditor.MouseRightButtonDownEvent, new RoutedEventHandler(MouseEmptyRightDown), true);
        }

        /// <summary>
        /// 右键空白处
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MouseEmptyRightDown(object sender, RoutedEventArgs args)
        {
            contextMenu.Items.Clear();
            AddAddItem();
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// 连线右键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void LineDownRight(object sender, RoutedEventArgs args)
        {
            FlowLineDownArgs downArgs = args as FlowLineDownArgs;
            if (downArgs.Fline == null || downArgs.MouseDownArgs.ChangedButton != MouseButton.Right) return;

            contextMenu.Items.Clear();

            MenuItem removeItem = new MenuItem();
            removeItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Remove);
            removeItem.Icon = GetFontText("\xe69e");
            removeItem.InputGestureText = "Delete";
            BindingOperations.SetBinding(removeItem, MenuItem.CommandProperty,
             new Binding("RemoveLineCommand")
             {
                 Source = flowEditor.DataContext,
             });
            removeItem.CommandParameter = downArgs.Fline;
            contextMenu.Items.Add(removeItem);
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// 动态右键菜单功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MouseDownRight(object sender, RoutedEventArgs args)
        {
            FlowItemDownArgs downArgs = args as FlowItemDownArgs;
            var sItems = downArgs.SelectItems;
            if (sItems.Count == 0) return;

            if (downArgs.MouseDownArgs.ChangedButton != MouseButton.Right) return;

            // 右键菜单
            var context = flowEditor.DataContext;
            contextMenu.Items.Clear();
            AddRemoveItem();
            AddEditItem();
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// 构造字体图标
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private TextBlock GetFontText(string icon, Brush foreBrush = null)
        {
            TextBlock txtBlock = new TextBlock();
            txtBlock.Text = icon;
            txtBlock.Style = this.FindResource("MotionIconSmall") as Style;

            if (foreBrush != null)
            {
                txtBlock.Foreground = foreBrush;
            }

            //var prop = DependencyProperty.Register("Property", typeof(string), typeof(Trigger), new PropertyMetadata("IsMouseOver"));
            //txtBlock.Style.Triggers.Add(new Trigger()
            //{
            //    Property = prop,
            //    Value = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            //});

            return txtBlock;
        }

        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddAddItem()
        {
            // 异步
            MenuItem addItem = new MenuItem();
            addItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Add);
            addItem.Icon = GetFontText("\xe742");
            addItem.InputGestureText = "Add";
            BindingOperations.SetBinding(addItem, MenuItem.CommandProperty,
             new Binding("AddCommand")
             {
                 Source = flowEditor.DataContext,
             });
            SetPermission(addItem);
            contextMenu.Items.Add(addItem);
        }

        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddRemoveItem()
        {
            // 异步
            MenuItem removeItem = new MenuItem();
            removeItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Remove);
            removeItem.Icon = GetFontText("\xe620");
            removeItem.InputGestureText = "Delete";
            BindingOperations.SetBinding(removeItem, MenuItem.CommandProperty,
             new Binding("RemoveCommand")
             {
                 Source = flowEditor.DataContext,
             });

            SetPermission(removeItem);
            removeItem.CommandParameter = GetSelectItems();
            contextMenu.Items.Add(removeItem);
        }

        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddEditItem()
        {
            // 异步
            MenuItem editItem = new MenuItem();
            editItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Edit);
            editItem.Icon = GetFontText("\xe654");
            editItem.InputGestureText = "Edit";
            BindingOperations.SetBinding(editItem, MenuItem.CommandProperty,
             new Binding("EditCommand")
             {
                 Source = flowEditor.DataContext,
             });

            SetPermission(editItem);

            editItem.CommandParameter = GetSelectItems();
            contextMenu.Items.Add(editItem);
        }

        /// <summary>
        /// 权限配置
        /// </summary>
        /// <param name="item"></param>
        /// <param name="minRole"></param>
        private void SetPermission(MenuItem item, SystemRole minRole = SystemRole.Sustaining)
        {
            // 是否启用
            BindingOperations.SetBinding(item, MenuItem.IsEnabledProperty, new Binding("SysRole")
            {
                Source = flowEditor.DataContext,
                ConverterParameter = minRole.ToString(),
                Converter = new RoleEnabledCoverter()
            });
        }

        /// <summary>
        /// 获取旋转的模块
        /// </summary>
        /// <returns></returns>
        private List<WorkItem> GetSelectItems()
        {
            return flowEditor.GetSelectedItems().Select(u => u.Tag as WorkItem).ToList();
        }
    }
}
