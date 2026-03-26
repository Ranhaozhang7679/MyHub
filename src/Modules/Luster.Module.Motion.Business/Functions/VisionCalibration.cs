using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 相机标定
    /// </summary>
    public class VisionCalibration : OverTimeFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通讯设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("是否是机器人", 0, CN = "机器人", DefaultV = false)]
        public bool IsRobot { get; set; }

        [DependOn("IsRobot", false)]
        [Parameter("是否有Z轴", 0, CN = "有Z轴", DefaultV = false)]
        public bool IsHaveZAxis { get; set; }

        [DependOn("IsRobot", true)]
        [Parameter("机器人通讯设备", 0, CN = "机器人", EditorType = typeof(VCommuncation))]
        public VDevice RobotCommDevice { get; set; }

        [DependOn("IsRobot", true)]
        [Parameter("机器人拍照点位", 0, CN = "拍照点", DefaultV = 0)]
        public int RobotPos { get; set; }

        [NotEmpty]
        [Parameter("标定步数", 1, CN = "标定步数", DefaultV = 7)]
        public int CaliStep { get; set; }


        [DependOn("IsRobot", false)]
        [Parameter("轴类型，通过关键字搜索", 2, CN = "X轴名称")]
        public VAxisDevice XAxis { get; set; }


        [DependOn("IsRobot", false)]
        [Parameter("轴类型，通过关键字搜索", 3, CN = "Y轴名称")]
        public VAxisDevice YAxis { get; set; }

        [DependOn("IsRobot", false)]
        [Parameter("轴类型，通过关键字搜索", 4, CN = "U轴名称")]
        public VAxisDevice UAxis { get; set; }

        [DependOn("IsHaveZAxis", true)]
        [DependOn("IsRobot", false)]
        [Parameter("轴类型，通过关键字搜索", 4, CN = "Z轴名称")]
        public VAxisDevice ZAxis { get; set; }

        [Parameter("IO类型，通过关键字搜索", 4, CN = "光源触发IO", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice LightDo { get; set; }

        [Parameter("防止VA返回多次数据，平台只取最后一次数据", 5, CN = "结尾标识", DefaultV = "&")]
        public string LastCharter { get; set; }

        [Parameter("第二位发送字符串", 6, CN = "第二位发送内容", DefaultV = "Guide")]
        public string SecondSendContent { get; set; }

        public VisionCalibration()
        {
            this.Tips = "相机标定";
            this.Icon = "\xe6b7";
        }

        public override bool DoExcute(out string errMsg)
        {

            //发送字符串
            string sendStr = "";
            //接受字符串
            string socketResult;
            //接收值数组
            List<string> vaResult = new List<string>();
            //X/Y/U轴偏移值
            double XOffset = 0;
            double YOffset = 0;
            double UOffset = 0;

            //X/Y/U轴当前位置
            double XCurrentPos = 0;
            double YCurrentPos = 0;
            double UCurretnPos = 0;


            //0.获取硬件
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            GetVDevice<VCommuncation>(RobotCommDevice, out var robot);
            GetVDevice<VAxis>(XAxis, out var xAxis);
            GetVDevice<VAxis>(YAxis, out var yAxis);
            GetVDevice<VAxis>(UAxis, out var uAxis);
            GetVDevice<VAxis>(ZAxis, out var zAxis);
            GetVDevice<VIO>(LightDo, out var lightDo);

            //1.打开通讯
            communcation.SetProtocol(ProtocolType.StringDefault);
            communcation.Open();

            //2.弹窗提示是否有干涉
            // OnAlarm(AlarmType.InfoTip, "即将开始标定，请保证没有干涉!!!");

            //3.XYU运动至拍照位
            lightDo.SetDigital(true);
            if(!IsRobot)
            {
                XAxis.Compensate = 0;
                YAxis.Compensate = 0;
                UAxis.Compensate = 0;
            }
           
            if(IsRobot)
            {
                //拼接字符串
                sendStr = $"{RobotPos.ToString("000")},030,0000.000,0000.000,0000.000\r" ;
                robot.Open();
                robot.Write(sendStr);
                //通讯接收
                //第一次收数据代表通讯成功
                string socketRes = robot.ReadSingle<string>("", OverTime);
                if (socketRes.Contains("R01"))
                {
                    //第二次收数据代表运动完成
                    socketResult = robot.ReadSingle<string>("", 300000);
                    if (socketResult.Contains("R02"))
                    {
                        socketResult = socketResult.Replace("\\r", "").Replace("\n", "").Replace("\r", "");
                        var res = socketResult.Split(',').ToList();
                        XCurrentPos = double.Parse(res[3]) ;
                        YCurrentPos = double.Parse(res[4]);
                        UCurretnPos = double.Parse(res[5]);
                    }
                    else
                    {
                        //Need To Do
                    }
                }
            }
            else
            {
                xAxis.MoveAbs(XAxis);
                yAxis.MoveAbs(YAxis);
                uAxis.MoveAbs(UAxis);
                xAxis.CheckMotionDone();
                yAxis.CheckMotionDone();
                uAxis.CheckMotionDone();
                if (IsHaveZAxis)
                {
                    ZAxis.Compensate = 0;
                    zAxis.MoveAbs(ZAxis);
                    zAxis.CheckMotionDone();
                }
                XCurrentPos = xAxis.GetCurrentPos();
                YCurrentPos = yAxis.GetCurrentPos();
                UCurretnPos = uAxis.GetCurrentPos();
            }

            //4.通讯触发拍照，并获取下一点位偏移值
            for (int i = 0; i <= CaliStep; i++)
            {

                //拼接字符串
                if (!IsRobot)
                {
                    XCurrentPos = xAxis.GetCurrentPos();
                    YCurrentPos = yAxis.GetCurrentPos();
                    UCurretnPos = uAxis.GetCurrentPos();
                }

                sendStr = $"Calib,{SecondSendContent},{i},{XCurrentPos},{YCurrentPos},{UCurretnPos}";
                communcation.Write(sendStr);

                // 等待结果
                socketResult = communcation.ReadSingle<string>("", OverTime);
                if (string.IsNullOrEmpty(LastCharter))
                {
                    LastCharter = "";
                }

                //解析结果
                var datas = socketResult.Split(LastCharter.ToCharArray());
                string lastResult = datas.LastOrDefault(u => !string.IsNullOrEmpty(u));
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"{MyOwner.Alias} 原始结果:{string.Join(",", datas)},使用结果:{lastResult}");
                vaResult = lastResult.Split(',').ToList();

                //偏移值获取
                XOffset = double.Parse(vaResult[1]);
                YOffset = double.Parse(vaResult[2]);
                UOffset = double.Parse(vaResult[3]);

                if(IsRobot)
                {
                    //拼接字符串
                    string xoffset;
                    string yoffset;
                    string uoffset;
                    xoffset = XOffset >= 0 ? XOffset.ToString("0000.000") : XOffset.ToString("000.000");
                    yoffset = YOffset >= 0 ? YOffset.ToString("0000.000") : YOffset.ToString("000.000");
                    uoffset = UOffset >= 0 ? UOffset.ToString("0000.000") : UOffset.ToString("000.000");
                    sendStr = $"888,030,{xoffset},{yoffset},{uoffset}\r";
                    robot.Write(sendStr);

                    //通讯接收
                    //第一次收数据代表通讯成功
                    string socketRes = robot.ReadSingle<string>("", OverTime);
                    if (socketRes.Contains("R01"))
                    {
                        //第二次收数据代表运动完成
                        socketResult = robot.ReadSingle<string>("", 300000);
                        if (socketResult.Contains("R02"))
                        {
                            socketResult = socketResult.Replace("\\r", "").Replace("\n", "").Replace("\r", "");
                            var res = socketResult.Split(',').ToList();
                            XCurrentPos = double.Parse(res[2]);
                            YCurrentPos = double.Parse(res[3]);
                            UCurretnPos = double.Parse(res[4]);
                        }
                        else
                        {
                            //Need To Do
                        }
                    }
                }
                else
                {
                    //轴运动
                    if (XOffset != 0)
                    {
                        XAxis.Compensate = XOffset;
                        xAxis.MoveRel(XAxis);
                        //xAxis.MoveRel(XOffset);
                    }
                    if (YOffset != 0)
                    {
                        YAxis.Compensate = YOffset;
                        yAxis.MoveRel(YAxis);
                        //yAxis.MoveRel(YOffset);
                    }
                    if (UOffset != 0)
                    {
                        UAxis.Compensate = UOffset;
                        uAxis.MoveRel(UAxis);
                        //uAxis.MoveRel(UOffset);
                    }
                    //等待轴运动完成
                    if (XOffset != 0)
                    {
                        xAxis.CheckMotionDone();
                    }
                    if (YOffset != 0)
                    {
                        yAxis.CheckMotionDone();
                    }
                    if (UOffset != 0)
                    {
                        uAxis.CheckMotionDone();
                    }
                }

                //运动完成，发送指令
                sendStr = $"Calib,Guide,{i},Done";
                communcation.Write(sendStr);

                //等待拍照完成结果
                socketResult = communcation.ReadSingle<string>("", OverTime);
                if (string.IsNullOrEmpty(LastCharter))
                {
                    LastCharter = "";
                }
            }
            lightDo.SetDigital(false);
            MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, "标定完成！");
            return base.DoExcute(out errMsg);
        }

    }
}
