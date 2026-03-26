#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Modbus
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       Modbus.cs
* 创建时间:       2022/7/25 10:31:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      67fa49f1-17cc-442a-a5b2-df43d44e6f2f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/25 10:31:43
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Motion.Interfaces;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class GetModbus : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("设备地址", 1, CN = "协议类型", DefaultV = ProtocolType.ModbusTCP)]
        public ProtocolType ProtocolType { get; set; }

        [Parameter("设备地址", 2, CN = "设备地址", DefaultV = 1)]
        public int Address { get; set; }

        [Parameter("设备地址", 3, CN = "数据类型")]
        public DataType DataType { get; set; }

        [DependOn("DataType", DataType.Int, DataType.Float, DataType.Double)]
        [Parameter("设备地址", 4, CN = "字节顺序")]
        public EndianType EndianType { get; set; }

        [Parameter("内存地址,十进制值", 5, CN = "内存地址")]
        public int StartAddress { get; set; }

        [DependOn("DataType", DataType.String)]
        [Parameter("字符颠倒", 6, CN = "字符颠倒", DefaultV = false)]
        public bool ReverseString { get; set; }

        [DependOn("DataType", DataType.String)]
        [Parameter("字符串长度", 7, CN = "字符串长度")]
        public int StrLen { get; set; }


        [DependOn("DataType", DataType.Bool)]
        [Parameter("持续等待线圈值", 8, CN = "线圈值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool CoilVal { get; set; }

        [DependOn("DataType", DataType.Int, DataType.Short, DataType.Float, DataType.Double)]
        [Parameter("持续等寄存器值", 9, CN = "寄存器值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int RegisterVal { get; set; }

        [DependOn("DataType", DataType.String)]
        [Parameter("字符串值", 10, CN = "字符串值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string StrVal { get; set; }

        public GetModbus()
        {
            this.Tips = "获取线圈或者寄存器上的值";
            this.Icon = "\xe692";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCommuncation>(CommDevice, out var communcation);

            communcation.Open();

            // 配置协议
            communcation.SetProtocol(ProtocolType);

            // 配置协议
            if (communcation.Protocol is IModbus m)
            {
                m.EndianType = EndianType;
            }

            //lock (SetModbus.lockPlc)
            {
                if (DataType == DataType.Bool)
                {
                    var coilVs = communcation.Read<bool>($"{Address} 01 {StartAddress} 1");
                    if (coilVs.Count > 0)
                    {
                        CoilVal = coilVs[0];
                    }
                    else
                    {
                        CoilVal = false;
                    }
                    MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias} Modbus 读取值:{CoilVal}");
                }
                else if (DataType == DataType.Int)
                {
                    var coilVs = communcation.Read<int>($"{Address} 03 {StartAddress} 1");
                    if (coilVs.Count > 0)
                    {
                        RegisterVal = coilVs[0];
                    }
                    else
                    {
                        RegisterVal = 0;
                    }
                    MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias} Modbus 读取值:{RegisterVal}");
                }
                else if (DataType == DataType.Short)
                {
                    var coilVs = communcation.Read<short>($"{Address} 03 {StartAddress} 1");
                    if (coilVs.Count > 0)
                    {
                        RegisterVal = coilVs[0];
                    }
                    else
                    {
                        RegisterVal = 0;
                    }
                    MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias} Modbus 读取值:{RegisterVal}");
                }
                else if (DataType == DataType.String)
                {
                    if (communcation.Protocol is IModbus modBus)
                    {
                        modBus.ReverseString = ReverseString;
                    }

                    var result = communcation.Read<string>($"{Address} 03 {StartAddress} {StrLen}");
                    if (result.Count > 0)
                    {
                        StrVal = result[0].Replace("\0", "");
                    }
                    else
                    {
                        OnAlarm(AlarmType.FailError, $"值获取失败");
                    }

                    MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias} Modbus 读取值:{StrVal}");
                }
            }
            return base.DoExcute(out errMsg);
        }
    }
}