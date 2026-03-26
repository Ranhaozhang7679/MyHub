using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{

    public class CameraIO : MotionFunction
    {

        [Parameter("是否需要光源", 0, CN = "需要光源", DefaultV = true)]
        public bool IsNeedLight { get; set; }

        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 1, CN = "相机触发IO", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice CameraDo { get; set; }

        [DependOn("IsNeedLight", true)]
        [Parameter("IO类型，通过关键字搜索", 2, CN = "光源触发IO", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice LightDo { get; set; }

        public CameraIO()
        {
            this.Tips = "相机硬触发";
            this.Icon = "\xe6b6";
           
        }

        public override bool DoExcute(out string errMsg)
        {
            //判断是否需要光源
            if (IsNeedLight)
            {
                GetVDevice<VIO>(LightDo, out var lightDo);
                //如果光源未打开
                if (!lightDo.GetDigitalOut())
                {
                    lightDo.SetDigital(true);
                    Thread.Sleep(100);
                }
            }
            GetVDevice<VIO>(CameraDo, out var cameraDo);
            //判断相机硬触发是否为True
            //如果为True，需要先关掉
            if (cameraDo.GetDigitalOut())
            {
                cameraDo.SetDigital(false);
                Thread.Sleep(10);
                cameraDo.SetDigital(true);
            }
            else
            {
                cameraDo.SetDigital(true);
            }
            return base.DoExcute(out errMsg);
        }
    }
}
