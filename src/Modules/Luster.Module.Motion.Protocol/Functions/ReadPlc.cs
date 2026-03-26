#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WaitPlc
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       ModbusCheck.cs
* 创建时间:       2022/7/25 13:40:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7f4d76d4-422d-4c5b-bea2-f43ce8fc99a5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/25 13:40:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class ReadPlc : PlcBase, IPauseFunction
    {

        [DependOn("DataType", DataType.Bool)]
        [Parameter("持续等待线圈值", 5, CN = "布尔值", DefaultV = false, ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool CoilVal { get; set; }

        [DependOn("DataType", DataType.Int, DataType.Short, DataType.Float, DataType.Double)]
        [Parameter("持续等寄存器值", 6, CN = "数值", DefaultV = 0, ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double RegisterVal { get; set; }

        public ReadPlc()
        {
            this.Tips = "读取一次结果";
            this.Icon = "\xe68c";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VPlc>(PlcDevice, out var plc);

            plc.Open();

            if (DataType == DataType.Bool)
            {
                CoilVal = plc.ReadBool(Address);

                MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias} Modbus 读取值:{CoilVal}");
            }
            else
            {
                RegisterVal = plc.ReadNumber(Address);

                MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias} Modbus 读取值:{RegisterVal}");
            }

            return base.DoExcute(out errMsg);
        }
    }
}
