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
    public class FX_UnBindCarrier : MotionFunction, IPauseFunction
    {
        public FX_UnBindCarrier()
        {
            this.Tips = "SFC解绑载具";
            this.Icon = "\xe6a8";
        }

        private const string OKChar = "0 SFC_OK";

        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        /// <summary>
        /// 上传PDCA启用
        /// </summary>
        [NotEmpty]
        [Parameter("启用即上传PDCA", 1, CN = "启用", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsEnable { get; set; }

        //[Parameter("本站ID", 4, CN = "本站ID", CanRef = ParamRef.Ref)]
        //public string StationID { get; set; }

        [NotEmpty]
        [Parameter("通过扫描治具得到的二维码", 2, CN = "治具码", CanRef = ParamRef.Ref)]
        public string CarrierSN { get; set; }
        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("工单绑定结果", 10, CN = "解绑结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        //[Parameter("本站站名", 7, CN = "本工站名", CanRef = ParamRef.Ref)]
        //public string StationName { get; set; }

        private SFCHelper _sfcHelper = null;

        public override bool DoExcute(out string errMsg)
        {
            #region 参数
            string sendStr = string.Empty;
            string recStr = string.Empty;
            errMsg = string.Empty;
            Result = true;
            #endregion

            if (!IsEnable || IsEmptyMode)
            {
                Result = true;
                return true;
            }

            //1.获取通讯设备
            GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);

            //2.连接通讯设备
            vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            vCommPdca.Open();
            if (_sfcHelper == null)
            {
                _sfcHelper = new SFCHelper(vCommPdca);
            }

            // 查询载具有没有
            _sfcHelper.QueryCarrier(CarrierSN, out errMsg);
            if (string.IsNullOrEmpty(errMsg))
            {
                _sfcHelper.UnbindCarrier(CarrierSN, out errMsg);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    // 解绑失败 直接返回，由流程自己判定处理
                    MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);
                    return true;
                }
            }

            Result = true;

            ////3.获取WIP
            //if (CarrierSN != null)
            //{
            //    //2.1.9 查询治具是否有绑定的SN
            //    sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd=qry_test_result&carrier_sn={CarrierSN}&station_code={StationName}&station_id={StationID}\n";
            //    MyOwner.OnLog(LogType.Debug, $"PDCA发送，查询治具码下面是否有绑定:{sendStr}");
            //    vCommPdca.Write(sendStr);
            //    // 等待结果
            //    recStr = vCommPdca.ReadSingle<string>("", 5000);
            //    MyOwner.OnLog(LogType.Debug, $"PDCA接收数据,查询治具码下面是否有绑定:{recStr}");
            //    if (recStr.Contains("0 SFC_OK\nOK") || recStr.Contains("0 SFC_OK OK"))
            //    {
            //        //2.1.10 治具解绑
            //        sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd=carrier_dislink_moudle&carrier_sn={CarrierSN}&station_id={StationID}\n";
            //        MyOwner.OnLog(LogType.Debug, $"PDCA发送解绑治具:{sendStr}");
            //        vCommPdca.Write(sendStr);
            //        // 等待结果
            //        recStr = vCommPdca.ReadSingle<string>("", 5000);
            //        MyOwner.OnLog(LogType.Debug, $"PDCA接收解绑治具:{recStr}");
            //    }
            //    else
            //    {
            //        MyOwner.OnLog(LogType.Debug, $"查询治具码下面无绑定,无需解绑");
            //    }

            //}
            return base.DoExcute(out errMsg);
        }
    }
}


