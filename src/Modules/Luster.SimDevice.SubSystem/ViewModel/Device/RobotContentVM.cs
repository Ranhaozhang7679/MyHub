using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Device
{
    public class RobotContentVM : BaseDeviceVM
    {
        protected RobotContentVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(IRobot))
        {
        }
    }
}
