using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice
{
    public abstract class RobotBase : DeviceBase
    {
        public override DeviceType DeviceType => DeviceType.Robot;

        public override string Brand => "";

        public override string Icon => "\xe6d8";
    }
}
