using Luster.Motion.EditorUI.Models;
using Luster.Motion.EditorUI.ViewModel.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Motion.EditorUI.Views.Dialogs
{
    /// <summary>
    /// ClassDialog.xaml 的交互逻辑
    /// </summary>
    public partial class SetModeGlobalVarDialog : UserControl
    {
        public SetModeGlobalVarDialog()
        {
            InitializeComponent();
            //if (DataContext is SetModeGlobalVarDialogVM vModel)
            //{
            //    VarTransfer.SelectedItems = vModel.SelectVarDatas;
            //}

            ListOrign.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ListOrign_MouseLeftButtonDown), true);

            ListTo.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ListTo_MouseLeftButtonDown), true);
        }

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

        private void ListTo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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


    }
}
