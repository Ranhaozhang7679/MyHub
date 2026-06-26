using System.Windows;

namespace Luster.Tools.UIHost
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var win = new HostWindow();
            win.Show();
        }
    }
}
