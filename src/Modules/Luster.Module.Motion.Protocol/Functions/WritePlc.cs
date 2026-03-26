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
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class WritePlc : PlcBase, IPauseFunction
    {
        [DependOn("DataType", DataType.Bool)]
        [Parameter("持续等待线圈值", 5, CN = "布尔值", DefaultV = false, ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public bool BoolVal { get; set; }

        [DependOn("DataType", DataType.Int, DataType.Short, DataType.Float, DataType.Double)]
        [Parameter("持续等寄存器值", 6, CN = "数值", DefaultV = 0, ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public double NumberVal { get; set; }

        [DependOn("DataType", DataType.String)]
        [Parameter("持续等寄存器值", 7, CN = "数值", DefaultV = "", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public string StringVal { get; set; }

        public WritePlc()
        {
            this.Tips = "写入地址";
            this.Icon = "\xe68c";
        }

        private static object lockPlc = new object();

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VPlc>(PlcDevice, out var plc);
            plc.Open();

            // 对PLC进行锁
            lock (lockPlc)
            {
                if (DataType == DataType.Bool)
                {
                    plc.Write<bool>(Address, BoolVal, $"1 05 {Address} 1");
                }
                else if (DataType == DataType.Short)
                {
                    plc.Write<short>(Address, (short)NumberVal, $"1 06 {Address} 1");
                }
                else if (DataType == DataType.Int)
                {
                    plc.Write<int>(Address, (int)NumberVal, $"1 06 {Address} 1");
                }
                else if (DataType == DataType.Float)
                {
                    plc.Write<float>(Address, (float)NumberVal, $"1 06 {Address} 1");
                }
                else if (DataType == DataType.Double)
                {
                    plc.Write<double>(Address, (float)NumberVal, $"1 06 {Address} 1");
                }
                else if (DataType == DataType.String)
                {
                    List<char> charArray = StringVal.ToCharArray().ToList();

                    ///奇数长度补齐一位
                    bool IsOdd = charArray.Count % 2 != 0;
                    List<int> Datas = new List<int>();
                    charArray.ForEach(x => { Datas.Add((int)x); });

                    ///确保长度都是偶数
                    if (IsOdd)
                    {
                        Datas.Add(0);
                    }
                    for (int i = 0; i < Datas.Count; i += 2)
                    {
                        short data = (short)(Datas[i] * 256 + Datas[i + 1]);
                        int realAddress = Convert.ToInt32(Address) + i / 2;
                        plc.Write<short>(Address, data, $"01 06 {realAddress} 1");
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 写入地址支持写入所有的地址（读和写）
        /// </summary>
        /// <param name="plc"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        protected override List<string> GetAddress(VPlc plc, DataType dataType)
        {
            return plc.GetAddress(dataType, PlcAddrType.None);
        }
    }
}
