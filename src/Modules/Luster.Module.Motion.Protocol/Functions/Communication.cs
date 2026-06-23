#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Communication
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       Communication.cs
* 创建时间:       2022/9/8 15:26:07
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      33bf0090-9c14-4a68-8a09-805374d1eaf6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 15:26:07
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
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

    public class Communication : OverTimeFunction, IPauseFunction,IStopFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

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

        [Parameter("判断返回值添加补偿", 8, CN = "结果补偿", CanRef = ParamRef.NoRef)]
        public SocketAction ReturnAddOffset { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Communication()
        {
            this.Tips = "用于普通命令通信";
            this.Icon = "\xe682";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
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

            comm.Open();
            string command = StringEx.GetString(Owner);
            if (IsRN)
            {
                command += "\r\n";
            }
            switch (Method)
            {
                case CommMethod.GetResult:
                    // 1.写入T
                    comm.ClearCache();
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
                //errMsg = "通信接收超时，收到内容为空";
                //OnAlarm(AlarmType.WarningTip, $"通信接收超时，收到内容为{OutString}", "N03VSOO-01@Communication timeout");
                // 报警内容与代码取自仿真通信(CommDevice)界面"报警配置"列配置，未配置时由 GetConfigMessage 回退
                errMsg = comm.GetConfigMessage(DeviceError.ConnectTimeFail);
                OnAlarm(AlarmType.WarningTip, comm.GetConfigMessage(DeviceError.ConnectTimeFail), comm.Errors[DeviceError.ConnectTimeFail]);
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
            OutString = comm.ReadSingle<string>("", OverTime, isCloseConnect: IsAutoDisConnect)?.Trim(); // ??string.Empty

            if (!string.IsNullOrEmpty(Pattern))
            {
                if (RegexTool.IsMatch(OutString, Pattern))
                {
                    OutString = RegexTool.GetValue(OutString, Pattern);
                    if(double.TryParse(OutString, out double outs ))
                    {
                        OutString = (outs + getReturnAddOffset(comm)).ToString();
                    }
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

        private double getReturnAddOffset(VCommuncation comm)
        {
            double returnAddOffset = 0.0;
            for (int i = 0; i < comm.Actions.Count; i++)
            {
                if (comm.Actions[i].Name == ReturnAddOffset.Name)
                {
                    double.TryParse(comm.Actions[i].Value, out returnAddOffset);
                    break;
                }
            }
            return returnAddOffset;
        }

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