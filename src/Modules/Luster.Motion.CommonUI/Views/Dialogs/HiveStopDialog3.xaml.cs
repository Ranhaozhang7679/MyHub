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
    /// HiveStopDialog2.xaml 的交互逻辑
    /// </summary>
    public partial class HiveStopDialog3 : UserControl
    {
        public HiveStopDialog3()
        {
            InitializeComponent();
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SetAllCheckBoxes(true);
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAllCheckBoxes(false);
        }

        private void SetAllCheckBoxes(bool isChecked)
        {
            foreach (var child in CheckListPanel.Children)
            {
                if (child is CheckBox cb)
                {
                    cb.IsChecked = isChecked;
                }
            }
        }
    }
}
