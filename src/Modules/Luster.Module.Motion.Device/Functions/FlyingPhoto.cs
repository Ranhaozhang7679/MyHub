using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{

    public enum TriggerNoType
    {
        [Description("相机")]
        Camera,

        [Description("踢料")]
        KickDut
    }

    public enum FlyingPhotoAction
    {
        [Description("设置参数")]
        SetParm,

        [Description("压入数据")]
        SetData
    }

    public class FlyingPhoto : MotionFunction
    {

        [NotEmpty]
        [Parameter("飞拍模块", 1, CN = "飞拍模块", EditorType = typeof(VFlyingPhoto), CanRef = ParamRef.None)]
        public VDevice FlyingPhotoDevice { get; set; }

        [Parameter("飞拍要做的动作", 2, CN = "飞拍动作", DefaultV = FlyingPhotoAction.SetParm)]
        public FlyingPhotoAction FlyingPhotoAction { get; set; }

        [Parameter("触发输出通道号", 3, CN = "输出号", DefaultV = 0)]
        public int TriggerNo { get; set; }

        [DependOn("FlyingPhotoAction", FlyingPhotoAction.SetParm)]
        [Parameter("锁存通道号", 4, CN = "锁存号", DefaultV = 0)]
        public int LCTNo { get; set; }

        [DependOn("FlyingPhotoAction", FlyingPhotoAction.SetParm)]
        [Parameter("触发通道的类型", 5, CN = "通道类型", DefaultV = TriggerNoType.KickDut)]
        public TriggerNoType TriggerNoType { get; set; }

        [DependOn("TriggerNoType", TriggerNoType.Camera)]
        [Parameter("相机触发相对于光电的相对距离", 6, CN = "相对距离", DefaultV = 0)]
        public long CameraOffset { get; set; }


        [DependOn("FlyingPhotoAction", FlyingPhotoAction.SetParm)]
        [Parameter("触发输出的脉冲宽度,单位:10ns", 7, CN = "脉冲宽度", DefaultV = 100000)]
        public int PulseWidth { get; set; }


        public FlyingPhoto()
        {
            this.Tips = "飞拍模块";
            this.Icon = "\xe675";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            GetVDevice<VFlyingPhoto>(FlyingPhotoDevice, out var flyingPhoto);

            if (FlyingPhotoAction == FlyingPhotoAction.SetData)
            {
                if(TriggerNoType== TriggerNoType.KickDut)
                {
                    foreach (var item in flyingPhoto.Positions)
                    {
                        flyingPhoto.SetTriggerPos(item.TriggerNo, item.Position);
                    }
                }
            }
            else
            {
                if (TriggerNoType == TriggerNoType.Camera)
                {
                    flyingPhoto.SetTriggerParm(TriggerNo, LCTNo, 0);
                }
                else
                {
                    flyingPhoto.SetTriggerParm(TriggerNo, -1, 1);
                }
                flyingPhoto.SetPulseWidth(TriggerNo, PulseWidth);

            }

            return base.DoExcute(out errMsg);
        }
    }
}
