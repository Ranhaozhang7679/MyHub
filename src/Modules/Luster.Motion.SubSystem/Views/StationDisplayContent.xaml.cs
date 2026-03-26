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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// StationDisplayControl.xaml 的交互逻辑
    /// </summary>
    public partial class StationDisplayContent : UserControl
    {

        public StationDisplayContent()
        {
            InitializeComponent();

            //CoverFlowMain.AddRange(new[]
            //{
            //    new Uri(@"Pack://application:,,,/Luster.Motion.Assests;component/Images/station.png"),
            //    new Uri(@"Pack://application:,,,/Luster.Motion.Assests;component/Images/station.png"),
            //    new Uri(@"Pack://application:,,,/Luster.Motion.Assests;component/Images/station.png"),
            //});
            //CoverFlowMain.PageIndex = 1;
        }
    }
}
