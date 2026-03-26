using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    public interface IRefComposition: IGroup
    {
        /// <summary>
        /// 涉资
        /// </summary>
        /// <param name="motionModule"></param>
        void SetRefModule(IMotionModule motionModule);

        /// <summary>
        /// 获取引用的模块
        /// </summary>
        /// <returns></returns>
        IMotionModule GetRefModule();
    }
}
