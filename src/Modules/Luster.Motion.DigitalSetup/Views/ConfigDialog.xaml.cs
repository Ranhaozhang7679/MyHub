using Luster.Motion.DigitalSetup.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Motion.DigitalSetup.Views
{
    public partial class ConfigDialog : Window
    {
        public ConfigDialog()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox?.DataContext is LADUploadContentVM vm && vm.TempSelectedParameters != null)
            {
                vm.TempSelectedParameters.Clear();
                foreach (var item in listBox.SelectedItems)
                {
                    vm.TempSelectedParameters.Add(item.ToString());
                }
            }
        }
    }
}