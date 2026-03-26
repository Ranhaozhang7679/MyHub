using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.TwoD.Functions
{
    public class FindCircle: MotionFunction
    {
        public FindCircle()         
        {
            this.Tips = "找圆";
            this.Icon = "\xe6d1";
        }

        public override bool DoExcute(out string errMsg)
        {
            return base.DoExcute(out errMsg);
        }
    }
}
