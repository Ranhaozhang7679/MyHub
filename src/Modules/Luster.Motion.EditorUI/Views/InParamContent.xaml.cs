using Luster.Control.Wpf.Motion;
using Luster.TaskFlow.Common.Attributes;
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

namespace Luster.Motion.EditorUI.Views
{
    /// <summary>
    /// InParamContent.xaml 的交互逻辑
    /// </summary>
    public partial class InParamContent : UserControl
    {
        public InParamContent()
        {
            InitializeComponent();
        }


        private void Preview_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock block && block.Tag != null && block.Tag is ParameterAttribute p)
            {
                var pGrid = e.Source as ParamGrid;

                if (!pGrid.HasPermission)
                {
                    return;
                }

                // 1.如果
                if (pGrid.Tag == null) return;

                var cm = this.FindResource("menuLink") as ContextMenu;
                if (cm != null)
                {
                    cm.PlacementTarget = pGrid;
                    cm.IsOpen = true;
                }
            }
        }
    }
}
