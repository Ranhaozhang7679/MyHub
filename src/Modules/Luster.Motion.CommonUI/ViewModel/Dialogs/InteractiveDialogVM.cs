using Luster.Motion.DataStruct;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Enums;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class InteractiveDialogVM:MotionDialogVM
    {
        private IDeviceEngine _deviceEngine;
        private IMotionEngine _mEngine;


        /// <summary>
        /// 交互提示
        /// </summary>
        private string _interactiveTip;
        public string InteractiveTip
        {
            get { return _interactiveTip; }
            set { SetProperty(ref _interactiveTip, value); }
        }

        public InteractiveDialogVM(IDeviceEngine deviceEngine, IMotionEngine mEngine) : base()
        {
            _deviceEngine = deviceEngine;
            _mEngine = mEngine;
        }

        /// <summary>
        /// 弹窗打开
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            string tip;
            base.OnDialogOpened(parameters);
            parameters.TryGetValue<string>("Tip", out tip );
            InteractiveTip=tip;

        }



        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("InteractiveType", InteractiveType.Retry);
        }

        protected override void Cancel(IDialogResult result)
        {
            base.Cancel(result);
            result.Parameters.Add("InteractiveType", InteractiveType.Stop);
        }

        protected override void No(IDialogResult result)
        {
            base.No(result);
            result.Parameters.Add("InteractiveType", InteractiveType.Continue);
        }


    }
}
