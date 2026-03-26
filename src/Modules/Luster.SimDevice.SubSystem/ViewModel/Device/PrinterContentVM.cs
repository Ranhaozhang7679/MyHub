using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Device
{
    public class PrinterContentVM : BaseDeviceVM
    {
        protected PrinterContentVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(IPrinter))
        {
        }

        /// <summary>
        /// 根据名字检查设备是否存在
        /// </summary>
        /// <param name="device">设备信息</param>
        /// <returns>true已存在 false不存在</returns>
        public override bool CheckDeviceIsExist(IDevice device)
        {
            if (device != null)
            {
                var list = deviceEngine.GetRealDevices(typeof(IPrinter)).
                    Where(u => u.Name == device.Name);

                if (list.Count() > 0)
                {
                    deviceEngine.OnLog(Common.DataStruct.Enums.LogType.Error, $"打印机{device.Name}已存在");
                    return true;
                }
            }
            return false;
        }
    }
}
