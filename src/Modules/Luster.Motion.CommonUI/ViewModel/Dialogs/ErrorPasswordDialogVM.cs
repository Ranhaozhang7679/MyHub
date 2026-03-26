using Luster.Motion.DataStruct;
using Luster.Motion.TaskFlow.Engine;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class ErrorPasswordDialogVM : MotionDialogVM
    {
        private IDialogService _dialogService;
        private IDeviceEngine _deviceEngine;
        private IMotionEngine _engine;
        private IMotionController _motionController;
        public ErrorPasswordDialogVM(IDialogService dialogService, IDeviceEngine deviceEngine, IMotionEngine engine)
        {
            _dialogService = dialogService;
            _deviceEngine = deviceEngine;
            _engine = engine;
          
        }


        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
                //FilterItems();
            }
            else
            {
                Title = string.Empty;
            }

        }
    }
}
