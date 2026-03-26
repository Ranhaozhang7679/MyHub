using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.TwoD.Functions
{
    public class Binarization: MotionFunction
    {


        /// <summary>
        /// 输出图像
        /// </summary>
        //[NotEmpty]
        [Parameter("输入图像", 1, CN = "图像")]
        public LImage Image { get; set; }

        public Binarization()
        {
            this.Tips = "二值化";
            this.Icon = "\xe6d1";
        }

        public override bool DoExcute(out string errMsg)
        {
            string a=Image.ToString();
            return base.DoExcute(out errMsg);
        }
    }
}
