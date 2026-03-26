using Luster.Module.Motion.Algorithm.Functions;
using Luster.Module.Motion.Logic.Functions;
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
    public class Algorithm : MotionModule
    {
        public override void InitFunctions()
        {
            AddFunction<VA>();
            //AddFunction<Holo3D>();
            AddFunction<AOI>();
            AddFunction<ICW>();
            AddFunction<CgAoi>();
        }
    }

    /// <summary>
    /// 模块创建
    /// </summary>
    public class AlgorithmCreator : MotionModuleCreator<Algorithm>
    {
        public override int Sort => 6;

        public override string Icon => "\xea1a";
    }
}