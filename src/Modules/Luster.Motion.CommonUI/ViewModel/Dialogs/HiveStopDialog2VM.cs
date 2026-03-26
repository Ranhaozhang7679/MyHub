using Luster.Common.Assets;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveStopDialog2VM : MotionDialogVM
    {
        private IMotionController mController;
        private IDialogService _dialogService;
        private HiveStopDialog2VM(IMotionController _motionController,IDialogService dialogService)
        {
            mController = _motionController;
            _dialogService = dialogService;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private DelegateCommand<object> _clickCommand;
        public DelegateCommand<object> ClickCommand => _clickCommand ?? (_clickCommand = new DelegateCommand<object>((o) =>
        {

            base.CloseDialog("ok");
            
            //mController.Stop();
        }));

        private DelegateCommand<object> _clickCommand2;
        public DelegateCommand<object> ClickCommand2 => _clickCommand2 ?? (_clickCommand2 = new DelegateCommand<object>((o) =>
        {
            base.CloseDialog("ok");
            //mController.Pause();
        }));
    }
}
