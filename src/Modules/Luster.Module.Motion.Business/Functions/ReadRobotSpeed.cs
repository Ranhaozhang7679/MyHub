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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 通过通讯读取机械手速度并保存到本地文件
    /// 用途: VisionAPI 控制参数上传时从本地文件读取真实机械手速度
    /// 通讯部分参照 EpsonRobot, 额外增加速度解析与本地文件保存功能
    /// 流程: 打开通讯 → 发送指令 → 接收响应 → 正则提取速度 → 写入 {RecipeConfigPath}\{RobotId}.txt
    /// </summary>
    public class ReadRobotSpeed : OverTimeFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("字符串格式", 1, CN = "编码格式", DefaultV = StringEncoding.Default)]
        public StringEncoding StringEncoding { get; set; }

        [Parameter("通讯类型", 2, CN = "通讯类型", DefaultV = CommMethod.GetResult)]
        public CommMethod Method { get; set; }

        [NotEmpty]
        [Parameter("支持多个字符串进行拼接", 3, CN = "通信数据")]
        public LStringEx StringEx { get; set; }

        [DependOn("Method", CommMethod.GetResult)]
        [Parameter("正则表达式解析", 5, CN = "正则表达式", DefaultV = @"(?<=,[A-Za-z]+,)\d+(?:\.\d+)?")]
        public string Pattern { get; set; }

        [Parameter("机械手ID", 6, CN = "机械手ID", DefaultV = "R1")]
        public string RobotId { get; set; }

        [Parameter("是否启用回车换行", 7, CN = "回车换行启用", CanRef = ParamRef.Ref, DefaultV = true)]
        public bool IsRN { get; set; }

        [Parameter("是否启用通讯超时或读取失败断连", 7, CN = "通讯超时或读取失败断连", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsAutoDisConnect { get; set; }

        [Parameter("判断返回值", 7, CN = "返回值判断", CanRef = ParamRef.Ref)]
        public string ReturnValue { get; set; }

        [Parameter("通信结果", 6, CN = "通信结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutString { get; set; }

        [Parameter("动作时间是否在合理区间", 10, CN = "是否合理", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool IsInVaildRange { get; set; }

        [Parameter("标准时间偏差", 10, CN = "标准时间偏差", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Deviation { get; set; }

        [Parameter("速度值", 20, CN = "速度值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutSpeed { get; set; }

        [Parameter("执行结果", 21, CN = "执行结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool OutResult { get; set; }

        [Parameter("失败原因", 22, CN = "失败原因", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutFailReason { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReadRobotSpeed()
        {
            this.Tips = "用于读取机械手速度并保存";
            this.Icon = "\xe692";
            this.StringEx = new LStringEx("speed,true");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            OutResult = false;
            OutFailReason = "";
            OutSpeed = 0;
            OutString = "";

            GetVDevice<VCommuncation>(CommDevice, out var comm);
            DateTime beginTime = DateTime.Now;
            if (comm.Protocol == null)
            {
                IProtocol protocol = null;

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

            switch (Method)
            {
                case CommMethod.GetResult:
                    comm.Write(command);
                    ReceiveMessage(comm, IsAutoDisConnect);
                    break;
                case CommMethod.Send:
                    comm.Write(command);
                    break;
                case CommMethod.Receive:
                    ReceiveMessage(comm, IsAutoDisConnect);
                    break;
            }
            DateTime endTime = DateTime.Now;
            IsInVaildRange = (endTime - beginTime).TotalMilliseconds >= comm.MinValue && (endTime - beginTime).TotalMilliseconds <= comm.MaxValue;
            Deviation = (int)(endTime - beginTime).TotalMilliseconds - comm.TargetValue;

            // 通信失败提示
            if (OutString?.Trim() == "NG")
            {
                errMsg = "通信接收超时，收到内容为空";
                OnAlarm(AlarmType.WarningTip, $"通信接收超时，收到内容为{OutString}");
            }

            // 解析速度并保存到本地文件(ReadRobotSpeed 特有功能)
            ParseSpeedAndSave();

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

            // ReadSingle 异常或失败时可能返回 null, 直接标记为 NG, 避免 Regex 抛 ArgumentNullException
            if (string.IsNullOrWhiteSpace(OutString))
            {
                OutString = "NG";
                return;
            }

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

        /// <summary>
        /// 从通信结果中解析速度值并保存到本地文件
        /// </summary>
        private void ParseSpeedAndSave()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OutString) || OutString.Trim() == "NG")
                {
                    OutFailReason = $"未获取到有效速度值, OutString={OutString}";
                    return;
                }

                if (double.TryParse(OutString.Trim(), out double speed))
                {
                    OutSpeed = Math.Round(speed, 2);
                    OutResult = true;
                    SaveSpeedToFile();
                }
                else
                {
                    OutFailReason = $"速度值解析失败, OutString={OutString}";
                }
            }
            catch (Exception ex)
            {
                OutFailReason = $"解析或保存速度异常: {ex.Message}";
            }
        }

        /// <summary>
        /// 把速度写入 {RecipeConfigPath}\{RobotId}.txt (纯数值字符串, 覆盖)
        /// 路径与 VisionAPI.LoadRobotSpeedFromDisk 保持一致
        /// </summary>
        private void SaveSpeedToFile()
        {
            try
            {
                string dir = MyOwner?.DeviceEngine?.RecipeConfigPath;
                if (string.IsNullOrEmpty(dir))
                {
                    OutFailReason = "RecipeConfigPath 为空，无法保存机械手速度";
                    return;
                }
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string robotId = string.IsNullOrEmpty(RobotId) ? "R1" : RobotId;
                string filePath = Path.Combine(dir, $"{robotId}.txt");
                File.WriteAllText(filePath, OutSpeed.ToString("F2"));
            }
            catch (Exception ex)
            {
                // 保存失败不影响读取结果
                try
                {
                    MyOwner.OnLog(LogType.Debug, $"模块 {MyOwner.Alias} 保存机械手速度失败: {ex.Message}");
                }
                catch
                {
                    // 忽略日志异常
                }
            }
        }

        // 允许模块暂停：暂停时进入 Pause 接口关闭通讯，避免长时间挂着 TCP 连接
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
