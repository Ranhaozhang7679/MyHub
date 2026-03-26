using Luster.Module.Motion.TwoD.Functions;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Modules;
using System;

namespace Luster.Module.Motion.IO
{
    /// <summary>
    /// 算法模块
    /// </summary>
    public class TwoD : MotionModule
    {
        public override void InitFunctions()
        {
            //AddFunction<CCDImage>();
            AddFunction<Binarization>();
            AddFunction<Morphological>();
            AddFunction<FindCircle>();

        }
    }

    /// <summary>
    /// 模块创建
    /// </summary>
    public class TwoDCreator : MotionModuleCreator<TwoD>
    {
        public override int Sort => 7;

        public override string Icon => "\xea1a";
    }
}