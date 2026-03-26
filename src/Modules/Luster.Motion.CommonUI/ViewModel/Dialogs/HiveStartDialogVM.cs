using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveStartDialogVM : MotionDialogVM
    {
        private string PDCANAME = "Extend_PDCA启用";

        private IMotionController mController;

        private HiveStartDialogVM(IMotionController _motionController)
        {
            mController = _motionController;
        }


        /// <summary>
        /// 调试/生产
        /// </summary>
        private DelegateCommand<object> _clickCommand;
        public DelegateCommand<object> ClickCommand => _clickCommand ?? (_clickCommand = new DelegateCommand<object>((o) =>
        {
            // 对SFC和PDCA进行监控
            var globalModule = mController.MotionEngine.Get(GlobalModule.GlobalID);

            //如果调试，PDCA关闭
            if ((string)o == "调试")
            {
                foreach (var item in globalModule.Parameters)
                {
                    if (item.Key == PDCANAME)
                    {
                        if (item.Value != null)
                        {
                            item.Value.Value = false;
                        }
                    }
                }
                base.CloseDialog("no");
            }
            //如果生产，PDCA启动
            else
            {
                //只有生产模式下，才启动PDCA
                foreach (var item in globalModule.Parameters)
                {
                    if (item.Key == PDCANAME)
                    {
                        if (item.Value != null)
                        {
                            item.Value.Value = true;
                        }
                    }
                }

                base.CloseDialog("ok");
            }
        }));
    }
}
