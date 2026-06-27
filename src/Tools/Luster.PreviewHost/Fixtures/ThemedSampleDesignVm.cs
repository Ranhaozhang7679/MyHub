namespace Luster.PreviewHost.Fixtures
{
    /// <summary>引用主题夹具 View 的设计时 VM</summary>
    public class ThemedSampleDesignVm
    {
        public string Title { get; set; } = "主题渲染验证";
        public string Subtitle { get; set; } = "同线程渲染 + PrimaryBrush + HandyControl";
        public string ActionLabel { get; set; } = "确认";
    }
}
