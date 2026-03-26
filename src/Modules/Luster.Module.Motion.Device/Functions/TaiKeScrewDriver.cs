#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WriteOutput
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       WriteOutput.cs
* 创建时间:       2022/5/30 9:36:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b0171b65-cb6e-4fd0-97fd-62a80d2a0632
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 9:36:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TaiKeCommon;

//using TaiKeTDP;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.Module.Motion.Device.Functions
{
    public enum POutputType
    {
        [Description("通信设备")]
        CommDevice,

        [Description("Analog IO")]
        AnalogIO
    }

    public enum PRESSFactory
    {
        [Description("鑫精诚")]
        鑫精诚,
        [Description("鹏力达")]
        鹏力达,
        [Description("力准")]
        力准
    }

    public enum COMType
    {
        [Description("网口和串口")]
        NetAndCOM = 1,
        [Description("仅网口")]
        Net,
        [Description("仅串口")]
        Com
    }
    public class TaiKeScrewDriver : MotionFunction
    {

        [NotEmpty]
        [Parameter("通信类型", 0, CN = "通信类型", DefaultV = COMType.NetAndCOM)]
        public COMType ComType { get; set; }

        //[NotEmpty]
        //[DependOn("ComType", COMType.Net)]
        //[DependOn("ComType", COMType.NetAndCOM)]
        //[Parameter("IP地址", 1, CN = "IP地址", DefaultV = "192.168.1.111")]
        private string Ip { get; set; }

        //[NotEmpty]
        //[Range(1, 65535)]
        //[DependOn("ComType", COMType.Net)]
        //[DependOn("ComType", COMType.NetAndCOM)]
        //[Parameter("端口号", 2, CN = "端口号", DefaultV = "5000")]
        private int Port { get; set; }

        [NotEmpty]
        [DependOn("ComType", COMType.Net)]
        [DependOn("ComType", COMType.NetAndCOM)]
        [Parameter("IP地址", 1, CN = "IP地址", EditorType = typeof(VCommuncation))]
        public VDevice NetDevice { get; set; }

        //[NotEmpty]
        //[DependOn("ComType", COMType.Com)]
        //[DependOn("ComType", COMType.NetAndCOM)]
        //[Parameter("串口号", 2, CN = "串口号", DefaultV = "com7")]
        private string ComPort { get; set; }

        //[NotEmpty]
        //[DependOn("ComType", COMType.Com)]
        //[DependOn("ComType", COMType.NetAndCOM)]
        //[Parameter("波特率", 3, CN = "波特率", DefaultV = "115200")]
        private int BaudRate { get; set; }
        
        [NotEmpty]
        [DependOn("ComType", COMType.Com)]
        [DependOn("ComType", COMType.NetAndCOM)]
        [Parameter("串口地址", 2, CN = "串口地址", EditorType = typeof(VCommuncation))]
        public VDevice ComDevice { get; set; }

        [NotEmpty]
        [Parameter("电批名称", 3, CN = "电批名称", DefaultV = "1号电批")]
        public string Name { get; set; }

        [Parameter("压力放大器输出模式", 4, CN = "压力输出模式", DefaultV = POutputType.CommDevice)]
        public POutputType OutputType { get; set; }

        [DependOn("OutputType", POutputType.CommDevice)]
        [NotEmpty]
        [Parameter("通信设备", 5, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Range(1, 8)]
        [DependOn("OutputType", POutputType.CommDevice)]
        [Parameter("站号", 6, CN = "压力传感器站号", DefaultV = 1)]
        public int StationNum { get; set; }

        //[NotEmpty]
        [DependOn("OutputType", POutputType.AnalogIO)]
        [Parameter("IO类型，通过关键字搜索", 7, CN = "IO名称", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [DependOn("OutputType", POutputType.AnalogIO)]
        [NotEmpty]
        [Parameter("重量范围", 8, CN = "范围", DefaultV = 5)]
        public int Weight { get; set; }

        [Parameter("产品条码", 9, CN = "产品码", CanRef = ParamRef.Ref)]
        public string SNCode { get; set; }

        [Parameter("规定时间内检查信号是否满足", 10, CN = "超时时间")]
        public int TimeOut { get; set; } = 3000;


        [Parameter("压力传感器名称", 11, CN = "压力传感器名称", DefaultV = PRESSFactory.鑫精诚)]
        public PRESSFactory FactoryName { get; set; }

        [Range(1, 1000)]
        [Parameter("压力值放大的倍率", 12, CN = "压力值放大的倍率", DefaultV = 1)]
        public int beilv { get; set; }

        //JIAN
        [Range(0, 5000)]
        [Parameter("压力曲线读取延时", 13, CN = "压力曲线读取延时", DefaultV = 0)]
        public int delay { get; set; }

        /// <summary>
        /// 压力值数组
        /// </summary>
        private List<double> listPressVal = new List<double>();

        /// <summary>
        /// 时间
        /// </summary>
        private List<double> listTimeVal = new List<double>();

        #region 输出参数
        [Parameter("错误代码", 101, CN = "EC", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string EC { get; set; } = "";  //错误代码

        [Parameter("第一阶段(螺丝旋入)的时间", 102, CN = "T1", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string T1 { get; set; } = "";  //第一阶段(螺丝旋入)的时间

        [Parameter("第二阶段(扭矩爬坡)的时间", 103, CN = "T2", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string T2 { get; set; } = "";  //第二阶段(扭矩爬坡)的时间

        [Parameter("第三阶段(保持扭矩)的时间", 104, CN = "T3", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string T3 { get; set; } = "";  //第三阶段(保持扭矩)的时间

        [Parameter("总时间", 105, CN = "Tt", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string Tt { get; set; } = "";  //总时间

        [Parameter("第一阶段(螺丝旋入)的角度", 106, CN = "A1", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string A1 { get; set; } = "";  //第一阶段(螺丝旋入)的角度

        [Parameter("第二阶段(扭矩爬坡)自的角度", 107, CN = "A2", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string A2 { get; set; } = "";  //第二阶段(扭矩爬坡)自的角度

        [Parameter("第三阶段(保持扭矩)的角度", 108, CN = "A3", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string A3 { get; set; } = "";  //第三阶段(保持扭矩)的角度

        [Parameter("第一阶段和第二阶段分界点", 109, CN = "SSL", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string SSL { get; set; } = "";  //第一阶段和第二阶段分界点

        [Parameter("总角度", 110, CN = "MaxAngle", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string MaxAngle { get; set; } = "";  //总角度

        [Parameter("最大扭矩值", 111, CN = "MaxTorque", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string MaxTorque { get; set; } = "";  //最大扭矩值

        [Parameter("着座扭矩", 112, CN = "SeatingTorque", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string SeatingTorque { get; set; } = "";  //着座扭矩

        [Parameter("夹紧扭矩", 113, CN = "ClampTorque", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string ClampTorque { get; set; } = "";  //夹紧扭矩

        [Parameter("摩擦力扭矩", 114, CN = "PrevailingTorque", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string PrevailingTorque { get; set; } = "";  //摩擦力扭矩

        [Parameter("所选的程序", 115, CN = "MC", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string MC { get; set; } = "";  //所选的程序

        [Parameter("最大压力", 116, CN = "MaxPressure", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string MaxPressure { get; set; } = "";  //最大压力


        #endregion

        bool connectedNet = false;
        bool connectedSerial = false;

        private ElectricScrewDrivers electricScrewDrivers;

        bool updated = false;

        private void ConnectNet()
        {
            if (!connectedNet)
            {
                GetVDevice<VCommuncation>(NetDevice, out var curentNet);
                var cN = curentNet.Communication as CommTCP;
                Ip= cN.Network.Ip;
                Port= cN.Network.Port;
                connectedNet = electricScrewDrivers.OpenNetController(Ip, Port);
            }
        }
        private void ConnectSerial()
        {
            if (!connectedSerial)
            {
                GetVDevice<VCommuncation>(ComDevice, out var curentCom);
                var cC = curentCom.Communication as CommSerial;
                ComPort= cC.PortName;
                BaudRate= cC.BaudRate;
                connectedSerial = electricScrewDrivers.OpenSerialPort(ComPort, BaudRate, 8, Parity.None, StopBits.One);
            }
        }

        /// <summary>
        /// 自动注释
        /// </summary>
        public override string[] NoteParams => new string[] { "设置", nameof(Device) };

        public TaiKeScrewDriver()
        {
            this.Tips = "扭力曲线";
            this.Icon = "\xe652";
            electricScrewDrivers = new ElectricScrewDrivers();
            electricScrewDrivers.DataUpdated -= DataUpdated;
            electricScrewDrivers.DataUpdated += DataUpdated;
            electricScrewDrivers.LogEvent -= LogEvent;
            electricScrewDrivers.LogEvent += LogEvent;
            //// 2025-5-5
            //electricScrewDrivers.StopPressSet -= StopPressSet;
            //electricScrewDrivers.StopPressSet += StopPressSet;

        }

        private void LogEvent(string msg)
        {
            if (MyOwner != null)
            {
                MyOwner.OnLog(LogType.Info, msg);
            }
        }

        private void DataUpdated(TaiKeData data)
        {
            // 返回的扭力数据

            EC = data.EC;
            T1 = data.T1;
            T2 = data.T2;
            T3 = data.T3;
            Tt = data.Tt;
            A1 = data.A1;
            A2 = data.A2;
            A3 = data.A3;
            SSL = data.SSL;
            MaxAngle = data.MaxAngle;
            MaxTorque = data.MaxTorque;
            SeatingTorque = data.SeatingTorque;
            ClampTorque = data.ClampTorque;
            PrevailingTorque = data.PrevailingTorque;
            MC = data.MC;
            //JIAN
            Thread.Sleep(delay);
            updated = true;
            //Thread.Sleep(800);
        }
        //private void StopPressSet(bool res)
        //{
        //    updated = true;
        //    Thread.Sleep(800);
        //}

        bool uiRegistered = false;

        public override bool DoExcute(out string errMsg)
        {
            listTimeVal.Clear();
            listPressVal.Clear();
            //DateTime timestart = DateTime.Now;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            errMsg = "";
            errMsg += $"压力传感器的类型:{FactoryName}\t\n";
            if (!uiRegistered && MyOwner != null)
            {
                uiRegistered = true;
                MyOwner.TaiKeScrewRegister(electricScrewDrivers, Name);
                LogEvent("注册电批");
            }

            bool res = false;
            switch (ComType)
            {
                case COMType.NetAndCOM:
                    electricScrewDrivers.SetUseComType((int)COMType.NetAndCOM);
                    res = UseNetAndComConnect(ref errMsg);
                    break;
                case COMType.Net:
                    electricScrewDrivers.SetUseComType((int)COMType.Net);
                    res = UseNetConnect(ref errMsg);
                    break;
                case COMType.Com:
                    electricScrewDrivers.SetUseComType((int)COMType.Com);
                    res = UseComConnect(ref errMsg);
                    break;
                default:
                    electricScrewDrivers.SetUseComType((int)COMType.NetAndCOM);
                    res = UseNetAndComConnect(ref errMsg);
                    break;
            }
            //如果连接失败，直接返回
            if (!res) return res;
            //复位输出数据更新标签
            updated = false;
            electricScrewDrivers.SetRootDir($"D:\\TaiKeScrewDatas\\{DateTime.Now.ToString("yyyyMMdd")}\\电批_{Name}\\");
            electricScrewDrivers.ProductSn = SNCode;
            electricScrewDrivers.SetScrewName(Name);
            string StationID = MyOwner.ConfigManager.GetWebConfig("StationID");
            string StationType = MyOwner.ConfigManager.GetWebConfig("Product_Vision");
            electricScrewDrivers.SetDirStationID(StationID, StationType);
            //结果输出设置默认值
            initResultOut();

            VCommuncation communcation = null;
            VIO io = null;
            if (OutputType == POutputType.CommDevice)
            {
                //errMsg += $"\t开始获取压力传感器";
                // 0.连接压力传感器通讯
                GetVDevice<VCommuncation>(CommDevice, out communcation);
                communcation.SetProtocol(ProtocolType.ModbusRTU);
                communcation.Open();
            }
            else
            {
                // 获取模拟量信号
                GetVDevice<VIO>(Device, out io);
            }

            int timeConsuse = 0;

            while (true)
            {
                //Thread.Sleep(10);
                if (OutputType == POutputType.CommDevice)
                {
                    string statname;
                    if (FactoryName == PRESSFactory.鹏力达/*name.Contains("鹏力达")*/)
                    {
                        statname = $"{StationNum} 03 80 1";
                        var coilVs = communcation?.Read<int>(statname);
                        listTimeVal.Add(stopwatch.Elapsed.TotalMilliseconds);
                        ////读取成功后计时清零
                        //if (coilVs.Count() >= 0) timeConsuse = 0;

                        if (coilVs.Count() > 0)
                        {
                            listPressVal.Add((Math.Round(Math.Abs(coilVs[0]) * 1.0 / 1000 * Convert.ToDouble(beilv), 3)));
                        }
                        else
                        {
                            listPressVal.Add(0);
                        }
                    }
                    else
                    {
                        statname = $"{StationNum} 04 02 1";
                        var coilVs = communcation.Read<float>(statname);
                        ////读取成功后计时清零
                        //if (coilVs.Count() > 0) timeConsuse = 0;

                        // 判断stopwatch.Elapsed.TotalMilliseconds是否为负值，如果为负值，则lstTimeVal添加值为lstTimeVal最后一个元素+120的值
                        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
                        if (elapsedMs < 0)
                        {
                            double lastVal = listTimeVal.Count > 0 ? listTimeVal[listTimeVal.Count - 1] : 0;
                            listTimeVal.Add(lastVal + 120);
                        }
                        else
                        {
                            listTimeVal.Add(elapsedMs);
                        }
                        if (coilVs.Count() > 0)
                        {
                            listPressVal.Add(Math.Round(coilVs[0] * Convert.ToDouble(beilv), 3));
                        }
                        else
                        {
                            listPressVal.Add(0);
                        }
                    }
                }
                else
                {
                    // 2025-5-18
                    double AnalogVal = io.GetAnglogIn() / 65535 * Weight;
                    listTimeVal.Add(stopwatch.Elapsed.TotalMilliseconds);
                    listPressVal.Add(AnalogVal);
                    //timeConsuse = 0;
                    Thread.Sleep(19);
                }

                //等待太科电批数据处理完成标记
                if (updated)
                {
                    updated = false;
                    electricScrewDrivers.UpdateTotalCurve(listTimeVal.ToArray(), listPressVal.ToArray());
                    MaxPressure = listPressVal.Max().ToString("f3");
                    break;
                }

                timeConsuse += 10;
                if (timeConsuse > TimeOut)
                {
                    errMsg = $"电批数据读取超时:{Name}_{timeConsuse}";
                    LogEvent($"电批数据读取超时:{Name}_{timeConsuse}");
                    connectedNet = electricScrewDrivers.CloseNetController();
                    connectedSerial = electricScrewDrivers.CloseSerialPort();
                    return true;
                }
            }

            if (connectedNet)
            {
                connectedNet = electricScrewDrivers.CloseNetController();
                GetVDevice<VCommuncation>(NetDevice, out var curentNet);
                var cN = curentNet.Communication as CommTCP;
                curentNet.CurrentHP++;
            }
            if (connectedSerial)
                connectedSerial = electricScrewDrivers.CloseSerialPort();
            errMsg = string.Empty;
            return true;
        }

        private bool UseNetAndComConnect(ref string errMsg)
        {
            bool rtn = false;
            int reconnectTimes = 0;
            // 连接串口和网口，给三次机会，每次间隔10ms
            while (!connectedSerial || !connectedNet)
            {
                reconnectTimes++;
                ConnectNet();
                ConnectSerial();
                if (connectedSerial && connectedNet)
                {
                    rtn = true;
                    break;
                }
                if (reconnectTimes >= 3)
                {
                    break;
                }
                Thread.Sleep(10);
            }

            if (!(connectedSerial && connectedNet))
            {
                if (!connectedNet)
                {
                    LogEvent($"电批网口连接3次失败:{Name}_{Ip}");
                    errMsg += $"电批网口连接3次失败:{Name}_{Ip}";
                }
                if (!connectedSerial)
                {
                    LogEvent($"\t电批串口口连接3次失败:{Name}_{ComPort}");
                    errMsg += $"\t电批串口口连接3次失败:{Name}_{ComPort}";
                }
                connectedNet = electricScrewDrivers.CloseNetController();
                connectedSerial = electricScrewDrivers.CloseSerialPort();

                rtn = false;
            }

            return rtn;
        }

        private bool UseNetConnect(ref string errMsg)
        {
            bool rtn = false;
            int reconnectTimes = 0;
            electricScrewDrivers.SetUseComType(2);
            // 连接串口和网口，给三次机会，每次间隔10ms
            while (!connectedNet)
            {
                reconnectTimes++;
                ConnectNet();
                if (connectedNet)
                {
                    rtn = true;
                    break;
                }
                if (reconnectTimes >= 3)
                {
                    break;
                }
                Thread.Sleep(10);
            }


            if (!connectedNet)
            {
                LogEvent($"电批网口连接3次失败:{Name}_{Ip}");
                errMsg += $"电批网口连接3次失败:{Name}_{Ip}";
                connectedNet = electricScrewDrivers.CloseNetController();

                rtn = false;
            }

            return rtn;
        }

        private bool UseComConnect(ref string errMsg)
        {
            bool rtn = false;
            int reconnectTimes = 0;

            // 连接串口，给三次机会，每次间隔10ms
            while (!connectedSerial)
            {
                reconnectTimes++;
                ConnectSerial();
                if (connectedSerial)
                {
                    rtn = true;
                    break;
                }
                if (reconnectTimes >= 3)
                {
                    break;
                }
                Thread.Sleep(10);
            }


            if (!connectedSerial)
            {
                LogEvent($"\t电批串口口连接3次失败:{Name}_{ComPort}");
                errMsg += $"\t电批串口口连接3次失败:{Name}_{ComPort}";
                connectedSerial = electricScrewDrivers.CloseSerialPort();
                rtn = false;
            }

            return rtn;
        }

        /// <summary>
        /// 结果输出设置默认值
        /// </summary>
        private void initResultOut()
        {
            EC = "OK";
            T1 = "273";
            T2 = "158";
            T3 = "100";
            Tt = "531";
            A1 = "1265";
            A2 = "62";
            A3 = "2.0";
            SSL = "0.07";
            MaxAngle = "1330";
            MaxTorque = "0";
            SeatingTorque = "0.03";
            ClampTorque = "0";
            PrevailingTorque = "0.1";
            MC = "PRESET2_PRQ:0.375_Fst";
            MaxPressure = "0.99";
        }
    }



}