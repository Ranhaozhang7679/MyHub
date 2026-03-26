using Luster.Module.Motion.Camera.Functions;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.IO
{
    public class Camera : MotionModule
    {
        public override void InitFunctions()
        {
            AddFunction<AreaCamera>();
            AddFunction<LineScan>();
            AddFunction<LaserScan>();
        }

    }

    public class CameraCreator : MotionModuleCreator<Camera>
    {
        public override int Sort => 5;

        public override string Icon => "\xe67d";
    }
}
