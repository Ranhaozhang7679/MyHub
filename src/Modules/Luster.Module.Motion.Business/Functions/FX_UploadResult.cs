#region 作者和版本
/*----------------------------------------------------------------
 * 版权所有 (c) 2023 P R C  保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：D05149
 * 公司名称：P R C
 * 命名空间：Luster.Module.Motion.Business.Functions
 * 唯一标识：281e20a6-90ed-44e0-a46d-fbe138561b40
 * 文件名：FX_OrderQuery
 * 当前用户域：LUSTERINC
 * 
 * 创建者：D05149
 * 电子邮箱：huidong@lusterinc.com
 * 创建时间：2023/2/1 16:30:31
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion 作者和版本

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.Integration.SFC;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 工单查询
    /// </summary>
    public class FX_UploadResult : MotionFunction, IPauseFunction
    {
        public FX_UploadResult()
        {
            this.Tips = "SFC上传errCode";
            this.Icon = "\xe6a8";
        }

        private const string OKChar = "0 SFC_OK";

        /// <summary>
        /// 通信服务器
        /// </summary>
        [DependOn("IsUsePDCA", true)]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }
        [Parameter("是否使用PDCA上传", 1, CN = "MacMini工站")]
        public bool IsUsePDCA { get; set; } = true;
        /// <summary>
        /// 上传PDCA启用
        /// </summary>
        [NotEmpty]
        [Parameter("启用即上传sfc", 1, CN = "启用", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsEnable { get; set; }

        //[Parameter("站号", 10, CN = "站号", DefaultV = 1)]
        //public int StationNum { get; set; }

        [Parameter("wip编码", 3, CN = "WIP编码", CanRef = ParamRef.Ref)]
        public string WIP { get; set; }

        /// <summary>
        /// 入料时间
        /// </summary>
        [NotEmpty]
        [Parameter("产品的入料时间固定格式是 yyyy-MM-dd HH:mm:ss", 4, CN = "入料时间", CanRef = ParamRef.Ref)]
        public string EnterTime { get; set; }


        [Parameter("上传Pass/Fail", 5, CN = "结果", CanRef = ParamRef.Ref, DefaultV = true)]
        public bool IsOK { get; set; }

        
        

        //[Parameter("MacAddress", 8, CN = "MacAddress", CanRef = ParamRef.Ref)]
        //public string MacAddress { get; set; }

        [Parameter("NG码", 8, CN = "NGCode", CanRef = ParamRef.Ref)]
        public string ErrorCode { get; set; }

        [Parameter("NG信息", 9, CN = "NGMsg", CanRef = ParamRef.Ref)]
        public string ErrorMsg { get; set; }

        //[Parameter("线体号", 6, CN = "线号", DefaultV = "FXGL_C02-3FC-01", CanRef = ParamRef.Ref)]
        //public string LineName { get; set; }
        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("上传结果", 10, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        


        private SFCHelper _sfcHelper = null;

        public override bool DoExcute(out string errMsg)
        {
            #region 参数
           
            errMsg = string.Empty;
            Result = false;
            #endregion

            if (!IsEnable || IsEmptyMode)
            {
                Result = true;
                return true;
            }

            if (IsUsePDCA)
            {
                //1.获取通讯设备
                GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);

                //2.连接通讯设备
                vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
                vCommPdca.Open();
                _sfcHelper = new SFCHelper(vCommPdca);

            }
            else
            {
                _sfcHelper = new SFCHelper("");
            }
            if(!IsOK)
            {
                if (_sfcHelper.UpdataErrorCode(WIP, EnterTime, ErrorCode,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),ErrorMsg,out errMsg))
                {
                    Result=true;
                }
                else
                { Result=false; }
            }

            return base.DoExcute(out errMsg);
        }
    }
}


