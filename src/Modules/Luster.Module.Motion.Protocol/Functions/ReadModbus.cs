using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.TaskFlow.Common.Enums;

namespace Luster.Module.Motion.Protocol.Functions
{
    /// <summary>
    /// Modbus功能码
    /// </summary>
    public enum ModbusFuncCode
    {
        [Description("01 读取线圈")]
        ReadCoils = 1,
        [Description("02 读取离散输入")]
        ReadDiscreteInputs = 2,
        [Description("03 读取保持寄存器")]
        ReadHoldingRegisters = 3,
        [Description("04 读取输入寄存器")]
        ReadInputRegisters = 4
    }

    /// <summary>
    /// 通用读值(Modbus)
    /// </summary>
    public class ReadModbus : MotionFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("设备地址", 1, CN = "协议类型", DefaultV = ProtocolType.ModbusTCP)]
        public ProtocolType ProtocolType { get; set; }

        [Parameter("设备地址", 2, CN = "设备地址", DefaultV = 1)]
        public int Address { get; set; }

        [Parameter("功能码", 3, CN = "功能码", DefaultV = ModbusFuncCode.ReadHoldingRegisters)]
        public ModbusFuncCode FuncCode { get; set; }

        [Parameter("数据类型", 4, CN = "数据类型")]
        public DataType DataType { get; set; }

        [Parameter("起始地址,十进制值", 6, CN = "内存地址")]
        public int StartAddress { get; set; }

        [DependOn("DataType", DataType.Bool)]
        [Parameter("读取的线圈值", 7, CN = "线圈值", ParamType = ParamType.OUT, CanRef = ParamRef.Ref)]
        public bool CoilVal { get; set; }

        [DependOn("DataType", DataType.Int, DataType.Short, DataType.Double)]
        [Parameter("读取的寄存器值", 8, CN = "寄存器值", ParamType = ParamType.OUT, CanRef = ParamRef.Ref)]
        public double RegisterVal { get; set; }

        [Limit(-1, 1000000)]
        [Parameter("超时时间，单位为s", 9, CN = "超时时间", DefaultV = 2)]
        public int OverTime { get; set; }

        public ReadModbus()
        {
            this.Tips = "读取Modbus地址上的值并输出";
            this.Icon = "\xe692";
        }

        public override bool DoExcute(out string errMsg)
        {
            int readTimeout = OverTime > 0 ? OverTime * 1000 : 2000;
            GetVDevice<VCommuncation>(CommDevice, out var cDevice);

            cDevice.Open();
            cDevice.SetProtocol(ProtocolType);

            // 格式化功能码为两位的10进制字符串如 "01", "03" 等
            string funcCodeStr = ((int)FuncCode).ToString("D2");

            // 根据数据类型计算需要读取的寄存器长度
            int length = 1;
            if (DataType == DataType.Int) length = 2;
            else if (DataType == DataType.Double) length = 4;

            // 构造指令，格式为：地址 功能码 内存起始地址 读取长度
            string readCmd = $"{Address} {funcCodeStr} {StartAddress} {length}";

            if (DataType == DataType.Bool)
            {
                CoilVal = cDevice.ReadSingle<bool>(readCmd, readTimeout);
            }
            else if (DataType == DataType.Short)
            {
                RegisterVal = cDevice.ReadSingle<short>(readCmd, readTimeout);
            }
            else if (DataType == DataType.Int)
            {
                RegisterVal = cDevice.ReadSingle<int>(readCmd, readTimeout);
            }
            else if (DataType == DataType.Double)
            {
                RegisterVal = cDevice.ReadSingle<double>(readCmd, readTimeout);
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 模块关闭
        /// </summary>
        public override void Stop()
        {
            // 不自动关闭共用的原实例
        }
    }
}