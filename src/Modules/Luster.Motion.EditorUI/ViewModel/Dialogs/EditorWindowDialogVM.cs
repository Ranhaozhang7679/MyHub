using Luster.Motion.CommonUI.ViewModel;
using Luster.TaskFlow.Motion;
using Prism.Services.Dialogs;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    /// <summary>
    /// 新窗口打开弹窗 ViewModel
    /// </summary>
    public class EditorWindowDialogVM : MotionDialogVM
    {
        private IMotionModule _targetModule;

        /// <summary>
        /// 目标模块
        /// </summary>
        public IMotionModule TargetModule => _targetModule;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            _targetModule = parameters.GetValue<IMotionModule>("Module");
        }
    }
}
