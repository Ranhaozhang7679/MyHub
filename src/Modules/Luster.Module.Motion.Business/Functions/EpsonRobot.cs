using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Module.Motion.Protocol.Functions;
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public enum CommMethod
    {
        [Description("仅发送")]
        Send,

        [Description("发送并获取")]
        GetResult,

        [Description("接收数据")]
        Receive
    }


    /// <summary>
    /// 增加Epson机器人的基元，目的时为了在运行过程中报警导致机器人停机时，能够直接复位机器人模块，
    /// </summary>
    public class EpsonRobot : OverTimeFunction, IPauseFunction
    {

        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("运行状态IO", 0, CN = "RunningStatus", EditorType = typeof(VIO))]
        public VDevice IO_RunningStatus { get; set; }

        [Parameter("暂停状态IO", 0, CN = "PauseStatus", EditorType = typeof(VIO))]
        public VDevice IO_PauseStatus { get; set; }

        [Parameter("继续IO", 0, CN = "Continue", EditorType = typeof(VIO))]
        public VDevice IO_Continue { get; set; }

        [Parameter("暂停IO", 0, CN = "Pause", EditorType = typeof(VIO))]
        public VDevice IO_Pause { get; set; }

        [Parameter("字符串格式", 1, CN = "编码格式", DefaultV = StringEncoding.Default)]
        public StringEncoding StringEncoding { get; set; }

        [Parameter("通讯类型", 2, CN = "通讯类型", DefaultV = CommMethod.Send)]
        public CommMethod Method { get; set; }

        [NotEmpty]
        [Parameter("支持多个字符串进行拼接", 3, CN = "通信数据")]
        public LStringEx StringEx { get; set; }

        [DependOn("Method", CommMethod.GetResult)]
        [Parameter("正则表达式解析", 5, CN = "正则表达式")]
        public string Pattern { get; set; }

        [Parameter("通信结果", 6, CN = "通信结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutString { get; set; }

        [Parameter("动作时间是否在合理区间", 10, CN = "是否合理", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool IsInVaildRange { get; set; }

        [Parameter("标准时间偏差", 10, CN = "标准时间偏差", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Deviation { get; set; }

        [Parameter("是否启用回车换行", 7, CN = "回车换行启用", CanRef = ParamRef.Ref)]
        public bool IsRN { get; set; }

        [Parameter("是否启用通讯超时或读取失败断连", 7, CN = "通讯超时或读取失败断连", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsAutoDisConnect { get; set; }

        [Parameter("判断返回值", 7, CN = "返回值判断", CanRef = ParamRef.Ref)]
        public string ReturnValue { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public EpsonRobot()
        {
            this.Tips = "用于Epson机器人通信";
            this.Icon = "\xe6b8";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            int retryTimes = 0;

            if (IO_Continue == null || IO_Pause == null || IO_PauseStatus == null || IO_RunningStatus == null)
            {
                errMsg = "爱普生机器人IO接口未引用，请确认";
                OnAlarm(AlarmType.WarningTip, errMsg);
                return true;
            }

            GetVDevice<VCommuncation>(CommDevice, out var comm);
            DateTime beginTime = DateTime.Now;
            if (comm.Protocol == null)
            {
                IProtocol protocol = null;

                List<byte> commands = new List<byte>();
                switch (StringEncoding)
                {
                    case StringEncoding.UTF8:
                        protocol = new StringProtocol();
                        break;
                    case StringEncoding.Default:
                        protocol = new StringProtocol();
                        break;
                }

                comm.Protocol = protocol;
            }

            //打开通信接口
            comm.Open();
            string command = StringEx.GetString(Owner);
            if (IsRN)
            {
                command += "\r\n";
            }
            comm.ClearCache();

            GetVDevice<VIO>(IO_RunningStatus, out var io_RunningStatus);
            //在暂停状态直接复位,不再执行通信,暂时默认在running状态下正常的，解决手动运行没有清楚AlarmInfo的问题
            if (MyOwner.Station.AlarmInfo != null
                && MyOwner.Station.AlarmInfo.Sender is Luster.Module.Motion.Business.Business p
                && p.ID == MyOwner.ID && !io_RunningStatus.GetDigitalIn())
            {
                GetVDevice<VIO>(IO_Continue, out var io_Continue);
                GetVDevice<VIO>(IO_PauseStatus, out var io_PauseStatus);

                //其他报警干涉机器人导致停止、暂停，再次复位时会触发这个操作
                while (io_PauseStatus.GetDigitalIn() && retryTimes < 3)
                {
                    //模拟手动点击IO复位情况
                    Thread.Sleep(1500);
                    io_Continue?.SetDigital(true);
                    Thread.Sleep(50);
                    io_Continue?.SetDigital(false);
                    Thread.Sleep(50);
                    retryTimes++;
                }

                if (retryTimes >= 3)
                {
                    OnAlarm(AlarmType.InfoTip, $"机器人重启失败, 请检查与机器人的IO信号");
                }

                ReceiveMessage(comm, IsAutoDisConnect);
                DateTime endTime = DateTime.Now;
                IsInVaildRange = (endTime - beginTime).TotalMilliseconds >= comm.MinValue && (endTime - beginTime).TotalMilliseconds <= comm.MaxValue;
                Deviation = (int)(endTime - beginTime).TotalMilliseconds - comm.TargetValue;

                // 2.接收结果
                if (OutString?.Trim() == "NG")
                {
                    // 报警内容取自仿真通信(CommDevice)界面"报警配置"列配置，未配置时由 GetConfigMessage 回退
                    errMsg = comm.GetConfigMessage(DeviceError.ConnectTimeFail);
                    OnAlarm(AlarmType.WarningTip, comm.GetConfigMessage(DeviceError.ConnectTimeFail), comm.Errors[DeviceError.ConnectTimeFail]);
                }

            }
            else
            {
                switch (Method)
                {
                    case CommMethod.GetResult:
                        // 1.写入T
                        comm.Write(command);
                        ReceiveMessage(comm, IsAutoDisConnect);
                        break;
                    case CommMethod.Send:
                        // 1.写入T
                        comm.Write(command);
                        break;
                    case CommMethod.Receive:
                        ReceiveMessage(comm, IsAutoDisConnect);
                        break;
                }
                DateTime endTime = DateTime.Now;
                IsInVaildRange = (endTime - beginTime).TotalMilliseconds >= comm.MinValue && (endTime - beginTime).TotalMilliseconds <= comm.MaxValue;
                Deviation = (int)(endTime - beginTime).TotalMilliseconds - comm.TargetValue;

                // 2.接收结果
                if (OutString?.Trim() == "NG")
                {
                    // 报警内容取自仿真通信(CommDevice)界面"报警配置"列配置，未配置时由 GetConfigMessage 回退
                    errMsg = comm.GetConfigMessage(DeviceError.ConnectTimeFail);
                    OnAlarm(AlarmType.WarningTip, comm.GetConfigMessage(DeviceError.ConnectTimeFail), comm.Errors[DeviceError.ConnectTimeFail]);

                    GetVDevice<VIO>(IO_Pause, out var io_Pause);
                    GetVDevice<VIO>(IO_PauseStatus, out var io_PauseStatus);

                    //告警时暂停机器人
                    io_Pause?.SetDigital(true);
                    Thread.Sleep(50);
                    io_Pause?.SetDigital(false);
                    Thread.Sleep(50);
                    while (!io_PauseStatus.GetDigitalIn() && retryTimes < 2)
                    {
                        //模拟手动点击IO复位情况
                        io_Pause?.SetDigital(true);
                        Thread.Sleep(50);
                        io_Pause?.SetDigital(false);
                        Thread.Sleep(50);
                        retryTimes++;
                    }

                    if (retryTimes >= 2)
                    {
                        MyOwner.OnLog(LogType.Debug, $"EpsonRobot pause failed！！！");
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 等待服务器返回数据
        /// </summary>
        /// <param name="comm"></param>
        private void ReceiveMessage(VCommuncation comm, bool IsAutoDisConnect)
        {
            // 通讯超时、读取为空时，不主动断开连接
            OutString = comm.ReadSingle<string>("", OverTime, isCloseConnect: IsAutoDisConnect);

            if (!string.IsNullOrEmpty(Pattern))
            {
                if (RegexTool.IsMatch(OutString, Pattern))
                {
                    OutString = RegexTool.GetValue(OutString, Pattern);
                }
                else
                {
                    OutString = "NG";
                }
            }
            if (string.IsNullOrWhiteSpace(OutString))
            {
                OutString = "NG";
            }
        }

        //允许模块暂停，暂停后会进入Pause接口，断开连接，从而告警，
        //告警后进入复位队列，再启动时查找告警模块列表，存在在告警模块列表时直接continue,不再给机器人发送通讯指令
        public override bool IsNeedPause => true;

        public override void Pause()
        {
            GetVDevice<VCommuncation>(CommDevice, out var comm);
            if (comm != null)
            {
                comm.Close();
                MyOwner.OnLog(LogType.Debug, $"TCP---close！！！");//监听TCP实例释放掉
            }
        }

        public override void Stop()
        {
            GetVDevice<VCommuncation>(CommDevice, out var comm);
            if (comm != null)
            {
                comm.Stop();
                comm.ClearCache();
                comm.Close();
                this.Dispose();
                MyOwner.OnLog(LogType.Debug, $"TCP---close！！！");//监听TCP实例释放掉
            }
        }
    }

}
