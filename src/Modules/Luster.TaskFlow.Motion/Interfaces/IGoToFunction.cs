using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    public interface IGoToFunction
    {
        /// <summary>
        /// 跳转成功
        /// </summary>
        bool GetJump(out IMotionModule jumpModule);
    }
}
