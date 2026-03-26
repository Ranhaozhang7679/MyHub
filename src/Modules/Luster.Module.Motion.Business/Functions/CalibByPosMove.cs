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
    public class CalibByPosMove : OverTimeFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通讯设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }


        [NotEmpty]
        [Parameter("标定步数", 1, CN = "标定步数", DefaultV = 7)]
        public int CaliStep { get; set; }

        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 2, CN = "点位参数",MultiValues =true)]
        public VAxisPos AxisPos { get; set; }


        [Parameter("IO类型，通过关键字搜索", 4, CN = "光源触发IO", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice LightDo { get; set; }

        [Parameter("防止VA返回多次数据，平台只取最后一次数据", 5, CN = "结尾标识", DefaultV = "&")]
        public string LastCharter { get; set; }

        [Parameter("第二位发送字符串", 6, CN = "第二位发送内容", DefaultV = "Guide")]
        public string SecondSendContent { get; set; }



        public CalibByPosMove()
        {
            this.Tips = "轴系相机标定";
            this.Icon = "\xe6b7";
        }

        public override bool DoExcute(out string errMsg)
        {
            if (AxisPos.Count != 3)
            {
                throw new Exception("轴数量必须为3!");
            }

            if (AxisPos[0].Axis.AxisType != AxisType.X
                || AxisPos[1].Axis.AxisType != AxisType.Y
                || AxisPos[2].Axis.AxisType != AxisType.U)
            {
                throw new Exception("轴号必须按照X、Y、U的顺序!");
            }



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

            var newAxisPos= AxisPos.Clone() as VAxisPos;

            //0.获取硬件
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            GetVDevice<VIO>(LightDo, out var lightDo);



            //1.打开通讯
            communcation.SetProtocol(ProtocolType.StringDefault);
            communcation.Open();

            //2.弹窗提示是否有干涉
            // OnAlarm(AlarmType.InfoTip, "即将开始标定，请保证没有干涉!!!");

            //3.XYU运动至拍照位
            lightDo.SetDigital(true);
            AxisPos.MovePostion(MyOwner.DeviceEngine);


            //4.通讯触发拍照，并获取下一点位偏移值
            for (int i = 0; i <= CaliStep; i++)
            {

                //拼接字符串
                XCurrentPos = newAxisPos[0].Axis.GetCurrentPos();
                YCurrentPos = newAxisPos[1].Axis.GetCurrentPos();
                UCurretnPos = newAxisPos[2].Axis.GetCurrentPos();

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


                ///获取补偿值作为下次的目标位置
                for (int index = 0; index < newAxisPos.Count; index++)
                {
                    double Offset = double.Parse(vaResult[index + 1]);
                    double CurPos = newAxisPos[index].Axis.GetCurrentPos();
                    newAxisPos[index].AxisPostion.Position = Offset + CurPos;

                }
                newAxisPos.MovePostion(MyOwner.DeviceEngine);


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
