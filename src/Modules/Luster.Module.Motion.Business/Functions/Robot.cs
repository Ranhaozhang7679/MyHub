using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class Robot : OverTimeFunction
    {
        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }


        //[Limit(10, 100)]
        //[Parameter("速度百分比", 1, CN = "速度百分比", DefaultV = 30)]
        //public int SpeedPercent { get; set; }

        [Limit(1, 1000)]
        [Parameter("点位号", 2, CN = "点位号", CanRef = ParamRef.Ref, DefaultV = 1)]
        public int PosNum { get; set; }

        [Limit(0, 100)]
        [Parameter("点位号增量", 2, CN = "点位号增量", CanRef = ParamRef.Ref, DefaultV = 0)]
        public int Increment { get; set; }

        [Parameter("点位名", 3, CN = "点位名", DefaultV = "上料位")]
        public string PosName { get; set; }



        //[Parameter("数据校验", 4, CN = "结尾标识", DefaultV = "&")]
        //public string LastCharter { get; set; }


        [Parameter("是否有补偿值", 4, CN = "补偿值", DefaultV = false)]
        public bool IsHaveOffset { get; set; }

        [DependOn("IsHaveOffset", true)]
        [Parameter("X轴补偿值", 5, CN = "X轴补偿值", CanRef = ParamRef.Ref, DefaultV = 0)]
        public double XOffset { get; set; }

        [DependOn("IsHaveOffset", true)]
        [Parameter("Y轴补偿值", 6, CN = "Y轴补偿值", CanRef = ParamRef.Ref, DefaultV = 0)]
        public double YOffset { get; set; }

        [DependOn("IsHaveOffset", true)]
        [Parameter("U轴补偿值", 7, CN = "U轴补偿值", CanRef = ParamRef.Ref, DefaultV = 0)]
        public double UOffset { get; set; }

        [Parameter("定位超时时间,单位为ms", 7, CN = "定位超时", CanRef = ParamRef.Ref, DefaultV = 30000)]
        public int MotionOverTime { get; set; }

        [Parameter("X轴位置", 10, CN = "X轴位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double XOutPos { get; set; }

        [Parameter("Y轴位置", 11, CN = "Y轴位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double YOutPos { get; set; }

        [Parameter("U轴位置", 12, CN = "U轴位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double UOutPos { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Robot()
        {
            this.Icon = "\xe6b8";
            this.Tips = "机器人通讯指令，格式为 点位,速度百分比,X轴补偿,Y轴补偿,RZ轴补偿#";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            string posNum;
            string spdPercent;
            string xOffset;
            string yOffset;
            string uOffset;
            //发送指令
            string sendContent;
            string socketResult;

            //
            GetVDevice<VCommuncation>(CommDevice, out var vComm);
            vComm.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            //1.通讯打开
            vComm.Open();

            //2.通讯发送
            //实例化一个轴，目的是获取轴速度比例参数。（这里拿到的速度是默认速度，不会更新）
            // 获取真实轴的速度比例系数。
            VAxis a = MyOwner.DeviceEngine.GetVDevices<VAxis>().FirstOrDefault();

            posNum = (PosNum + Increment).ToString("000");
            spdPercent = (100 * (a?.GetCurrentModeSpeedPercent() ?? 0.1)).ToString("000");
            xOffset = XOffset >= 0 ? XOffset.ToString("0000.000") : XOffset.ToString("000.000");
            yOffset = YOffset >= 0 ? YOffset.ToString("0000.000") : YOffset.ToString("000.000");
            uOffset = UOffset >= 0 ? UOffset.ToString("0000.000") : UOffset.ToString("000.000");


            sendContent = $"{posNum},{spdPercent},{xOffset},{yOffset},{uOffset}\r";
            if (IsHaveOffset && XOffset == 0 && YOffset == 0 && UOffset == 0)
            {
                // 空跑模式下不在提示
                if (!IsEmptyMode)
                {
                    MyOwner.OnAlarm(AlarmType.InfoTip, $"Robot 勾选补偿，但是补偿值为0 XOffset:{xOffset},YOffset:{yOffset},UOffset:{uOffset}");
                }
            }

            //加入重试机制
            try
            {
                vComm.Write(sendContent);

                //3.通讯接收
                //第一次收数据代表通讯成功
                 socketResult = vComm.ReadSingle<string>("", OverTime);
            }
            catch
            {
                System.Threading.Thread.Sleep(100);
                vComm.Write(sendContent);

                //3.通讯接收
                //第一次收数据代表通讯成功
                 socketResult = vComm.ReadSingle<string>("", OverTime);
            }


            if (socketResult.Contains("R01"))
            {
                //第二次收数据代表运动完成
                socketResult = vComm.ReadSingle<string>("", MotionOverTime);

                //if (vComm.ConnectOK())
                //    vComm.Close();
                if (socketResult.Contains("R02"))
                {
                    socketResult = socketResult.Replace("\\r", "").Replace("\n", "").Replace("\r", "");
                    List<string> vaResult = socketResult.Split(',').ToList();
                    XOutPos = double.Parse(vaResult[3]);
                    YOutPos = double.Parse(vaResult[4]);
                    UOutPos = double.Parse(vaResult[5]);
                    return true;
                }
                else
                {
                    errMsg = "机器人结束运动失败";
                    return false;
                }
            }
            else
            {
                errMsg = "机器人开始运动失败";
            }

            return base.DoExcute(out errMsg);
        }

    }
}
