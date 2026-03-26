using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;

namespace DC.Authorization.WPF.Helper
{
    public interface ITradDialogService
    {
        string? OpenFileDialog(string title);
        string? SaveFileDialog(string title);
        void Notification(string message);
    }

    public class TradDialogService : ITradDialogService
    {
        public void Notification(string message)
        {
            MessageBox.Show(message, "提示", MessageBoxButton.OK);
        }

        public string? OpenFileDialog(string title)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.RestoreDirectory = false;
            dialog.Filter = "CSV Files|*.csv;";
            if (dialog.ShowDialog() == true) { return dialog.FileName; }
            return null;
        }

        public string? SaveFileDialog(string title)
        {
            var dialog = new SaveFileDialog();
            dialog.RestoreDirectory = false;
            dialog.Filter = "CSV Files|*.csv;";
            if (dialog.ShowDialog() == true) { return dialog.FileName; }
            return null;
        }
    }
}
