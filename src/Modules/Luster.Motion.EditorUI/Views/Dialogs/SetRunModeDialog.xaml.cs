using Luster.Motion.EditorUI.Models;
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

namespace Luster.Motion.EditorUI.Views.Dialogs
{
    /// <summary>
    /// ClassDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SetRunModeDialog : UserControl
    {
        public SetRunModeDialog()
        {
            InitializeComponent();
            ListOrign.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ListOrign_MouseLeftButtonDown), true);
            //ListTo.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ListTo_MouseLeftButtonDown), true);
        }

        /// <summary>
        /// 选择全局变量拖拽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListOrign_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListView listview = e.Source as ListView;
            if (listview.SelectedItem != null)
            {
                GlobalVar node = listview.SelectedItem as GlobalVar;
                DataObject dataObj;
                dataObj = new DataObject(node);
                DragDrop.DoDragDrop(listview, dataObj, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// 撤销选择拖拽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListTo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid listview = e.Source as DataGrid;
            if (listview.SelectedItem != null)
            {
                GlobalVar node = listview.SelectedItem as GlobalVar;
                DataObject dataObj;
                dataObj = new DataObject(node);
                DragDrop.DoDragDrop(listview, dataObj, DragDropEffects.Move);
            }
        }
    }
}
