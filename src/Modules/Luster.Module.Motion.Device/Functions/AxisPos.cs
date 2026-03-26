using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class AxisPos:MotionFunction
    {
        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 1, CN = "点位参数", MultiValues = true)]
        public VAxisPos SelectAxisPos { get; set; }

        [Parameter("精度", 2, CN = "精度", DefaultV = 0.1)]
        public double Precision { get; set; }


        [Parameter("到位", 10, CN = "到位", ParamType = TaskFlow.Common.Enums.ParamType.OUT,DefaultV =false)]
        public bool MotionDone { get; set; }

        [Parameter("原点", 11, CN = "原点", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool Org { get; set; }

        [Parameter("正极限", 12, CN = "正极限", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool Pel { get; set; }

        [Parameter("负极限", 13, CN = "负极限", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool Mel { get; set; }
        public AxisPos()
        {
            this.Tips = "轴到位";
            this.Icon = "\xe6bd";
        }

        public override bool DoExcute(out string errMsg)
        {
            double cmdPos;
            double feedbackPos;
            MotionDone = true;
            foreach (var item in SelectAxisPos)
            {
                cmdPos = item.AxisPostion.Position + item.Compensate;
                feedbackPos = item.Axis.GetCurrentPos();
                if (Math.Abs(feedbackPos-cmdPos)>Precision)
                {
                    MotionDone = false;
                    var dictStatus = item.Axis.GetAxisStatus();
                    if (dictStatus != null) 
                    {
                        if(dictStatus.ContainsKey(AxisStatus.Org))
                        {
                            Org = dictStatus[AxisStatus.Org];
                        }

                        if (dictStatus.ContainsKey(AxisStatus.Pel))
                        {
                            Pel = dictStatus[AxisStatus.Pel];
                        }
                        if (dictStatus.ContainsKey(AxisStatus.Mel))
                        {
                            Mel = dictStatus[AxisStatus.Mel];
                        }
                    }
                    break;
                }
            }


            return base.DoExcute(out errMsg);
        }




        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);

            if (parameter.Name == nameof(SelectAxisPos) && newV is VAxisPos vPos)
            {
                vPos.UpdateAxis(MyOwner.DeviceEngine);
                BuildDynamicAxisPos(vPos, 5);
                BuildDynamicAxisPos(vPos, 20, TaskFlow.Common.Enums.ParamType.OUT, false);
            }
        }
    }
}
