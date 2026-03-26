using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.TwoD.Functions
{
    public class CCDImage:MotionFunction
    {

        [Parameter("相机名称，通过关键字搜索", 0, CN = "相机名称", EditorType = typeof(VCommuncation))]
        public VDevice Camera { get; set; }

       
        [Parameter("输出图像，类型LImage", 10, CN = "图像缓存", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]

        public LImage Image { get; set; }



        public CCDImage() 
        {
            this.Tips = "CCD图像采集";
            this.Icon = "\xe6d1";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCamera>(Camera, out var camera);
            if (camera == null)
            {
                errMsg = $"相机:{Camera.Name}未找到";
                return false;
            }
            camera.OnCamera();



            return base.DoExcute(out errMsg);

        }
    }


}
