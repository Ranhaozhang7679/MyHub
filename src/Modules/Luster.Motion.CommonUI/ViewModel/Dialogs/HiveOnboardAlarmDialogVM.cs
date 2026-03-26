using Luster.Common.Assets;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.Integration.Web;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveOnboardAlarmDialogVM : MotionDialogVM
    {
        private IMotionController mController;
        private IDialogService _dialogService;
        private HiveAPI _hiveAPI;
        private HiveOnboardAlarmDialogVM(IMotionController _motionController,IDialogService dialogService, HiveAPI hiveAPI)
        {
            mController = _motionController;
            _dialogService = dialogService;
            _hiveAPI = hiveAPI;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        //private DelegateCommand<object> _clickCommand;
        //public DelegateCommand<object> ClickCommand => _clickCommand ?? (_clickCommand = new DelegateCommand<object>((o) =>
        //{
        //    //base.CloseDialog("ok");
        //    // 用户点了按钮，给Hive发送消息
        //    _hiveAPI.StartFinish();
        //}));

        private DelegateCommand _clickCommand;
        public DelegateCommand ClickCommand => _clickCommand ??= new DelegateCommand(async () =>
        {
            await Task.Run(() =>
            {
                try
                {
                    _hiveAPI.StartFinish();
                }
                catch (Exception ex)
                {
                    // 记录日志或提示用户
                }
            });
        });
    }
}
