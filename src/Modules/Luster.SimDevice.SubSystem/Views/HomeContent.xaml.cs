using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.SimDevice.SubSystem.Views
{
    // Declare a Delegate which will return the position of the 
    // DragDropEventArgs and the MouseButtonEventArgs event object
    public delegate Point GetDragDropPosition(IInputElement theElement);

    /// <summary>
    /// HomeContent.xaml 的交互逻辑
    /// </summary>
    public partial class HomeContent : UserControl
    {
        public HomeContent()
        {
            InitializeComponent();

            // 注册鼠标左键事件
            this.dgHome.MouseLeftButtonUp += new MouseButtonEventHandler(dgHome_MouseLeftButtonUp);
        }

        private void dgHome_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is HomeContentVM homeModel)
            {   
                // 拖拽后,归零设备排序
                homeModel.UpdateHomeSort();
                homeModel.UpdateCheckSort();
            }           
        }
    }
}
