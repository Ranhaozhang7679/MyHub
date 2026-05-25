using Luster.Motion.EditorUI.ViewModel;
using Luster.Motion.EditorUI.ViewModel.Dialogs;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System.Windows;

namespace Luster.Motion.EditorUI.Views.Dialogs
{
    /// <summary>
    /// EditorWindowDialog.xaml 的交互逻辑
    /// 在独立弹窗中编辑指定模块的子模块，支持 EditorContent 的所有功能
    /// </summary>
    public partial class EditorWindowDialog : System.Windows.Controls.UserControl
    {
        public EditorWindowDialog()
        {
            InitializeComponent();
            Loaded += EditorWindowDialog_Loaded;
        }

        private void EditorWindowDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // 配置父窗口：允许调整大小，显示系统按钮
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.SizeToContent = SizeToContent.Manual;
                window.ResizeMode = ResizeMode.CanResize;
                window.Width = 1200;
                window.Height = 700;
                window.MinWidth = 600;
                window.MinHeight = 400;

                // 启用右上角关闭/最大化/最小化按钮
                if (window is HandyControl.Controls.Window hcWindow)
                {
                    hcWindow.ShowSystemButton = true;
                }
            }

            // 设置目标模块并刷新
            if (DataContext is EditorWindowDialogVM vm && vm.TargetModule != null
                && editorContent.DataContext is EditorContentVM editorVM)
            {
                editorVM.RootModule = vm.TargetModule;
                editorVM.RefreshUI();
            }
        }
    }
}
