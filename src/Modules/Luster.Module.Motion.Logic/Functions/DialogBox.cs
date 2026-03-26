using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public class DialogBox: MotionFunction
    { 

        [Parameter("提示内容", 1, CN = "提示内容")]
        public string Content { get; set; }

        public DialogBox() 
        {
            this.Tips = "对话框";
            this.Icon = "\xe69c";
        }

        InteractiveType interactiveType = InteractiveType.Continue;

        public override bool DoExcute(out string errMsg)
        {
            interactiveType = MyOwner.OnDialogBox(Content);
            if (interactiveType == InteractiveType.Continue)
            {
                return base.DoExcute(out errMsg);

            }
            else if (interactiveType == InteractiveType.Retry)

            {
               return  DoExcute(out errMsg);
            }
            else
            {
                errMsg = "交互停止";
                return false;
            }

        }
    }
}
