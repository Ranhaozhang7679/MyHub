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
using System.Windows.Shapes;

namespace LusterMotion.ViewModel
{
    /// <summary>
    /// TaskWaitWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TaskWaitWindow : Window
    {
        Task task;
        public TaskWaitWindow()
        {
            InitializeComponent();
        }

        public void SetTask(Task task)
        {
            this.task = task;
        }

        public void UpdateProgress(int value,int max)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.progressBar.Maximum = max;
                this.progressBar.Value = value;
            });

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            task.ContinueWith(t =>
            {
                this.Dispatcher.Invoke(() =>
                {              
                    MessageBox.Show("保存完成");
                    this.Close();
                });
            });

            task.Start();
        }
    }
}
