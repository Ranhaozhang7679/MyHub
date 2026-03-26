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
using Luster.Motion.DataStruct.Network;
using System.Threading;
using System.Data;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class SetModbus : MotionFunction, IPauseFunction
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
        [Parameter("字节顺序", 4, CN = "字节顺序")]
        public EndianType EndianType { get; set; }


        [Parameter("内存地址,十进制值", 5, CN = "内存地址")]
        public int StartAddress { get; set; }

        [DependOn("DataType", DataType.Bool)]
        [Parameter("持续等待线圈值", 6, CN = "布尔值")]
        public bool BoolVal { get; set; }

        [DependOn("DataType", DataType.Short, DataType.Int)]
        [Parameter("持续等寄存器值", 7, CN = "整型值", DefaultV = 0,CanRef = ParamRef.Ref)]
        public int IntVal { get; set; }

        [DependOn("DataType", DataType.Double)]
        [Parameter("持续等寄存器值", 8, CN = "双精度值", DefaultV = 0, CanRef = ParamRef.Ref)]
        public double DoubleVal { get; set; }

        [DependOn("DataType", DataType.String)]
        [Parameter("持续等寄存器值", 9, CN = "字符串", DefaultV = 0, CanRef = ParamRef.Ref)]
        public string StringVal { get; set; }

        public SetModbus()
        {
            this.Tips = "设置线圈或者寄存器上的值";
            this.Icon = "\xe692";
        }

        internal static object lockPlc = new object();

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

            lock (lockPlc)
            {
                if (DataType == DataType.Bool)
                {
                    // 1、写入完成后
                    communcation.Write<bool>(BoolVal, $"{Address} 05 {StartAddress} 1");
                    //// 2025-5-9 执行一次，写2遍，为了提高写入成功率【DJ】
                    //communcation.Write<bool>(BoolVal, $"{Address} 05 {StartAddress} 1");
                    Retry<bool>(communcation, BoolVal);
                }
                else if (DataType == DataType.Int)
                {
                    communcation.Write<int>(IntVal, $"{Address} 06 {StartAddress} 1");
                    //communcation.Write<int>(IntVal, $"{Address} 06 {StartAddress} 1");
                    Retry<int>(communcation, IntVal);
                }
                else if (DataType == DataType.Short)
                {
                    communcation.Write<short>((short)IntVal, $"{Address} 06 {StartAddress} 1");
                    //communcation.Write<short>((short)IntVal, $"{Address} 06 {StartAddress} 1");
                    Retry<short>(communcation, (short)IntVal);
                }
                else if (DataType == DataType.Double)
                {
                    communcation.Write<double>(DoubleVal, $"{Address} 06 {StartAddress} 1");
                    //communcation.Write<double>(DoubleVal, $"{Address} 06 {StartAddress} 1");
                    Retry<double>(communcation, DoubleVal);
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
                        int realAddress = StartAddress + i / 2;
                        communcation.Write<short>(data, $"{Address} 06 {realAddress} 1");
                        //communcation.Write<short>(data, $"{Address} 06 {realAddress} 1");
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 获取当前的Plc
        /// </summary>
        private VPlc _vPlc = null;
        private void SetPlc<T>(int addr, T val)
        {
            if (_vPlc == null)
            {
                _vPlc = MyOwner.DeviceEngine.GetVDevices<VPlc>().FirstOrDefault();
            }

            if (_vPlc != null)
            {
                var plcAddr = _vPlc.Address.FirstOrDefault(u => u.Address == addr.ToString());
                if (plcAddr != null)
                {
                    if (DataType == DataType.Bool && bool.TryParse(val?.ToString(), out var v))
                    {
                        plcAddr.Value = v ? 1 : 0;
                    }
                    else
                    {
                        if (double.TryParse(val?.ToString(), out var d))
                        {
                            plcAddr.Value = d;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 重试，确认写入Modbus成功呢
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="communcation"></param>
        /// <param name="val"></param>
        private void Retry<T>(VCommuncation communcation, T val) where T : IComparable
        {
            // 同步更新缓存
            SetPlc(StartAddress, val);

            string readMsg = $"{Address} {(typeof(T) == typeof(bool) ? "01" : "03")} {StartAddress} 1";
            // 2025-5-7 wyy dong去掉防呆，設置後不再進行讀取判斷
            //bool writeSuccess = true;
            bool writeSuccess = false;
            for (int i = 0; i < 1; i++)
            {
                // 2、延时20ms后,再次读取确认
                SpinWait.SpinUntil(() => false, 20);
                var resultVals = communcation.Read<T>(readMsg);
                if (resultVals != null && resultVals.Count > 0)
                {
                    if (CompareVal<T>.Compare(OpRule.Equal, val, resultVals[0]))
                    {
                        writeSuccess = true;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }

            // 如果查询一次后，结果没有就记录下
            if (!writeSuccess)
            {
                MyOwner.OnLog(LogType.Warning, $"模块:{MyOwner.Alias} 写入Modbus 失败!");
            }
        }
    }
}