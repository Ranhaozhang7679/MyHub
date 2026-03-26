using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel.Dialogs;
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

namespace Luster.Motion.CommonUI.Views.Dialogs
{
    /// <summary>
    /// CreateProjectContent.xaml 的交互逻辑
    /// </summary>
    public partial class SetOutPutChartDialog : UserControl
    {
        public SetOutPutChartDialog()
        {
            InitializeComponent();
            paramSouce.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(paramSouce_MouseLeftButtonDown), true);
            this.gridOutPut.MouseLeftButtonUp += new MouseButtonEventHandler(gridOutPut_MouseLeftButtonUp);

            //TvwImportSource.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(TvwInOutportSource_MouseLeftButtonDown), true);
            // 注册鼠标左键事件
            //this.gridAnyOutPut.MouseLeftButtonUp += new MouseButtonEventHandler(dgOutput_MouseLeftButtonUp);


        }

        private void paramSouce_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGrid datagrid = e.Source as DataGrid;
            if (datagrid.SelectedItem != null)
            {
                OutputItemModel node = datagrid.SelectedItem as OutputItemModel;
                DataObject dataObj;
                dataObj = new DataObject(node);
                DragDrop.DoDragDrop(datagrid, dataObj, DragDropEffects.Move);
            }
        }

        private void gridOutPut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is SetOutPutChartDialogVM model)
            {
            }
        }

        private void TvwInOutportSource_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeView treeView = e.Source as TreeView;
            if (treeView.SelectedItem != null)
            {
                LNode node = treeView.SelectedItem as LNode;
                DataObject dataObj;
                dataObj = new DataObject(node);
                DragDrop.DoDragDrop(treeView, dataObj, DragDropEffects.Move);
            }
        }

        private void dgOutput_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is SetOutPutChartDialogVM model)
            {
            }
        }
    }
}
