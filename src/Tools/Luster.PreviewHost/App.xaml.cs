namespace Luster.PreviewHost
{
    /// <summary>预览宿主入口,主题在 App.xaml 合并,真实 Main 在 Program.cs</summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            // 实际调度在 Program.Main,这里仅保证主题加载
        }
    }
}
