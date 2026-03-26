using Luster.Common.DataStruct;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Control.Wpf.Motion.TreeEx;
using Luster.Motion.CommonUI;
using Luster.Motion.EditorUI.Models;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// ModuleDifferContent.xaml 的交互逻辑
    /// </summary>
    public partial class FlowViewContent : UserControl
    {
        /// <summary>
        /// 右键菜单
        /// </summary>
        private ContextMenu contextMenu;

        public FlowViewContent()
        {
            InitializeComponent();
            lsTree.Items.CurrentChanged += Items_CurrentChanged;
            contextMenu = new ContextMenu();

        }

        private void Items_CurrentChanged(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                // 等待200 ms在绑定右键对象
                Thread.Sleep(200);
                Dispatcher.Invoke(() =>
                {
                    var treeList = GetChildren<TreeListBox>(lsTree);

                    foreach (var item in treeList)
                    {
                        item.AddHandler(TreeListBox.MouseRightButtonDownEvent, new RoutedEventHandler(MouseDownRight), true);
                    }
                });
            });
        }


        private void MouseDownRight(object sender, RoutedEventArgs args)
        {
            var tree = (sender as TreeListBox);
            if (tree == null) return;
            contextMenu.PlacementTarget = tree;

            var selectItem = tree.SelectedItem as ModuleCT;
            if (selectItem == null) return;

            contextMenu.Items.Clear();

            // 右键菜单
            BuildSingleMenu(selectItem);
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// 构建菜单
        /// </summary>
        /// <param name="mCT"></param>
        private void BuildSingleMenu(ModuleCT mCT)
        {
            // 运行菜单
            if (mCT.Tag.Status != RunStatus.Skip)
            {
                AddRunItem(mCT);
                //AddSetFlowItem(mCT);
            }

            AddSkipItem(mCT);
            //contextMenu.Items.Add(new Separator());
            AddRemoveItem(mCT);
            AddCopyItems(mCT);

            var function = mCT.Tag.TaskFunction;
            if (function is not IStation && function.Name != "WaitStatus")
            {
                AddToWaitItem(mCT);
            }
        }


        /// <summary>
        /// 复制粘贴
        /// </summary>
        private void AddCopyItems(ModuleCT mCT)
        {
            // 复制
            MenuItem copyItem = new MenuItem();
            copyItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Copy);
            copyItem.InputGestureText = "Ctrl+C";
            copyItem.Icon = GetFontText("\xe63a");
            BuildCommand(copyItem, "CopyCommand");
            copyItem.CommandParameter = mCT.Tag;
            contextMenu.Items.Add(copyItem);
        }

        /// <summary>
        /// 模块忽略
        /// </summary>
        private void AddRunItem(ModuleCT mCT)
        {
            // 运行
            MenuItem runItem = new MenuItem();
            runItem.Header = langs.LangProvider.GetLang(langs.LangKeys.RunOne);
            runItem.Icon = GetFontText("\xe644");
            BuildCommand(runItem, "RunOneCommand");
            runItem.CommandParameter = mCT.Tag;
            runItem.InputGestureText = "F6";
            contextMenu.Items.Add(runItem);
        }

        /// <summary>
        /// 模块忽略
        /// </summary>
        private void AddSkipItem(ModuleCT mCT)
        {
            // 忽略
            MenuItem skipItem = new MenuItem();
            string skipText = langs.LangKeys.Skip;
            if (mCT != null && mCT.Tag is IMotionModule module && module.Status == RunStatus.Skip)
            {
                skipText = langs.LangKeys.CancelSkip;
            }

            skipItem.Header = langs.LangProvider.GetLang(skipText);
            BuildCommand(skipItem, "SkipCommand");
            skipItem.CommandParameter = mCT.Tag;
            skipItem.Icon = GetFontText("\xe6a1");
            contextMenu.Items.Add(skipItem);
        }

        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddRemoveItem(ModuleCT mCT)
        {
            // 异步
            MenuItem removeItem = new MenuItem();
            removeItem.Header = langs.LangProvider.GetLang(langs.LangKeys.Remove);
            removeItem.Icon = GetFontText("\xe620");
            removeItem.InputGestureText = "Delete";
            BuildCommand(removeItem, "RemoveCommand");
            removeItem.CommandParameter = mCT.Tag;
            contextMenu.Items.Add(removeItem);
        }

        /// <summary>
        /// 添加删除命令
        /// </summary>
        /// <param name="dataContext"></param>
        private void AddToWaitItem(ModuleCT mCt)
        {
            contextMenu.Items.Add(new Separator());
            MenuItem waitItem = new MenuItem();
            waitItem.Header = langs.LangProvider.GetLang(langs.LangKeys.WaitStatus);
            waitItem.Icon = GetFontText("\xe6b4");

            // 命令绑定
            BuildCommand(waitItem, "ToWaitCommand");

            waitItem.CommandParameter = mCt;

            contextMenu.Items.Add(waitItem);
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
                 Source = lsTree.DataContext,
             });
        }

        /// <summary>
        /// 查找所有满足条件的子节点，
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depen"></param>
        /// <param name="childname"></param>
        /// <returns></returns>
        public static List<T> GetChildren<T>(DependencyObject depen, string childname = null) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> lists = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depen); i++)
            {
                child = VisualTreeHelper.GetChild(depen, i);
                if ((child is T) && (((T)child).Name == childname || string.IsNullOrEmpty(childname)))
                {
                    lists.Add((T)child);
                }
                lists.AddRange(GetChildren<T>(child, childname));
            }
            return lists;
        }
    }
}
