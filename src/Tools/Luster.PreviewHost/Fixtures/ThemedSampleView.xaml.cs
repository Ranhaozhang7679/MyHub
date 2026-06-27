namespace Luster.PreviewHost.Fixtures
{
    using System.Windows.Controls;

    /// <summary>
    /// 引用主题资源(PrimaryBrush / HandyControl 控件)的夹具 View,
    /// 用于验证同线程渲染修复后跨线程访问不再抛异常、主题资源正常加载。
    /// </summary>
    public partial class ThemedSampleView : UserControl
    {
        public ThemedSampleView() => InitializeComponent();
    }
}
