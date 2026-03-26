using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.SFC;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Luster.Module.Motion.Business.Functions
{
    public class SFCFlowTiaoJi : MotionFunction, IPauseFunction
    {
        /// <summary>
        /// SN，CG或者小料等的SN
        /// </summary>
        [NotEmpty]
        [Parameter("SN编码", 0, CN = "SN编码", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        /// <summary>
        /// 机种，例如：V53
        /// </summary>
        [NotEmpty]
        [Parameter("机种（示例V53）", 1, CN = "机种")]
        public string categoryKey { get; set; }

        /// <summary>
        /// 阶段，例如：CRB
        /// </summary>
        [NotEmpty]
        [Parameter("调机物料实际使用阶段", 2, CN = "阶段")]
        public string wbs { get; set; }

        /// <summary>
        /// 阶段，例如：Top Moudle
        /// </summary>
        [NotEmpty]
        [Parameter("调机物料所属模块名称", 3, CN = "模块名称", DefaultV = "Top module")]
        public string partName { get; set; }

        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("SFC通信结果", 20, CN = "SFC通信结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [Parameter("SFC返回结果", 20, CN = "SFC返回结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool ThisPass { get; set; }

        /// <summary>
        /// web 通信组件
        /// </summary>
        private WebTool webTool;
        private const string OKChar = "0 SFC_OK";


        public SFCFlowTiaoJi()
        {
            this.Tips = "调机料SFC流程处理";
            this.Icon = "\xe6a8";
        }

        /// <summary>
        /// SFCHelper
        /// </summary>
        private SFCHelper _sfcHelper = null;

        public override bool DoExcute(out string errMsg)
        {
            
            errMsg = "";
            // 默认是失败
            Result = false;
            ThisPass = false;

            _sfcHelper = new SFCHelper("");
            _sfcHelper.GetTiaoJi(SN, categoryKey, wbs, partName, out errMsg);
            Result = true;
            if (!string.IsNullOrEmpty(errMsg))
            {
                MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, "F99OOOO-01");
                return true;
            }
            ThisPass = true;
            // 不用报警
            //if (!string.IsNullOrEmpty(errMsg))
            //{
            //    Result = false;
            //    MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
            //    return true;
            //}

            return base.DoExcute(out errMsg);
        }
    }
}