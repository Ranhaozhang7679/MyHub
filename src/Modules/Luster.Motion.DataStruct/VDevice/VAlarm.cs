using Luster.Motion.DataStruct.FXVirtual;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.VDevice
{
    public class VAlarm : VirtualDeviceBase
    {
        public string  AlarmKey { get; set; }
        public string AlarmCN{ get; set; }
        public string AlarmEn { get; set; }

        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            base.SetDevice(hardDevice, out errMsg);
            return true;
        }
    }
}
