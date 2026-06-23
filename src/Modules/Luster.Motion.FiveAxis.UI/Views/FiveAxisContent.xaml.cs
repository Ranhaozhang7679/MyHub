using System.Windows.Controls;

namespace Luster.Motion.FiveAxis.UI.Views
{
    /// <summary>
    /// 五轴标定参数面板 View（P6-A 基建）。
    /// VM 由 prism:ViewModelLocator.AutoWireViewModel 自动绑定到 FiveAxisContentVM。
    /// </summary>
    public partial class FiveAxisContent : UserControl
    {
        public FiveAxisContent()
        {
            InitializeComponent();
        }
    }
}
