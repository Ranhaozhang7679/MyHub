using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class PlcBase : MotionFunction
    {
        [NotEmpty]
        [Parameter("PLC设备", 0, CN = "PLC设备", EditorType = typeof(VPlc))]
        public VDevice PlcDevice { get; set; }

        [Parameter("设备地址", 1, CN = "数据类型")]
        public DataType DataType { get; set; }

        [NotEmpty]
        [Parameter("设备地址", 2, CN = "Plc地址")]
        public virtual string Address { get; set; }


        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(DataType))
            {
                var pDevice = MyOwner.Parameters[nameof(PlcDevice)].Value as VDevice;
                if (pDevice != null)
                {
                    GetVDevice<VPlc>(pDevice, out var plcDevice);
                    if (plcDevice != null)
                    {
                        var dataType = newV.Parse<DataType>();
                        var addr = GetAddress(plcDevice, dataType);
                        OnDataSource(nameof(Address), addr.ToArray());
                    }
                }
            }
            else if (parameter.Name == nameof(PlcDevice))
            {
                GetVDevice<VPlc>(newV as VDevice, out var plcDevice);
                if (plcDevice != null)
                {
                    var pDataVal = MyOwner.Parameters[nameof(DataType)].Value;
                    if (pDataVal != null)
                    {
                        var dataType = pDataVal.Parse<DataType>();
                        var addr = GetAddress(plcDevice, dataType);
                        OnDataSource(nameof(Address), addr.ToArray());
                    }
                }
            }
        }

        protected virtual List<string> GetAddress(VPlc plc, DataType dataType)
        {
            return plc.GetAddress(dataType, PlcAddrType.Read);
        }
    }
}
