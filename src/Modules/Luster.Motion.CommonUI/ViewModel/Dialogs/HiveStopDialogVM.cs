using Luster.Motion.DataStruct;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveStopDialogVM : MotionDialogVM
    {

        private IMotionController mController;
        private IDeviceEngine deviceEngine;
        private HiveStopDialogVM(IMotionController _motionController,IDeviceEngine _deviceEngine)
        {
            mController = _motionController;
            deviceEngine = _deviceEngine;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private DelegateCommand<object> _clickCommand;
        public DelegateCommand<object> ClickCommand => _clickCommand ?? (_clickCommand = new DelegateCommand<object>((o) =>
        {
           base.CloseDialog("ok");
            bool isHome = false;
            try
            {
                isHome =deviceEngine.IsHome(out var errmsg);
            }
            catch
            (Exception ex)
            {
            }
            if (isHome)
            {
                // 此处调用Pause，会将机台状态切到Alarm，如果是复位后再刷卡，状态会从Pause又切到Alarm，切换错误
                // 如果没有复位直接刷卡，此时机台已处在报警中，无需再调用Pause
                //mController.Pause();
            }
        }));
    }
}
