using Luster.Common.DataStruct.DataModels;
using Luster.Motion.SubSystem.ViewModel;
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
    /// QualitySetContent.xaml 的交互逻辑
    /// </summary>
    public partial class QualitySetContent : UserControl
    {
        public QualitySetContent()
        {
            InitializeComponent();
            TvwImportSource.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(TvwInOutportSource_MouseLeftButtonDown), true);
            // 注册鼠标左键事件
            this.dgOutput.MouseLeftButtonUp += new MouseButtonEventHandler(dgOutput_MouseLeftButtonUp);
        }

        private void dgOutput_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is QualitySetContentVM model)
            {
                model.RefreshData();
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
    }
}
