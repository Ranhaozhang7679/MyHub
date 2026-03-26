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
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.Motion.Integration.SFC;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 工单查询
    /// </summary>
    public class KeyMaterialQuery : MotionFunction, IPauseFunction
    {
        public KeyMaterialQuery()
        {
            this.Tips = "SFC关键物料查询";
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

        [Parameter("SN编码", 1, CN = "SN编码", CanRef = ParamRef.Ref)]
        public string LCG { get; set; }

        /// <summary>
        /// WIP长度
        /// </summary>
        [Parameter("WIP长度", 3, CN = "WIP长度", DefaultV = 18)]
        public int WIP_Length { get; set; }

        [Parameter("自动化工站", 1, CN = "自动化工站", CanRef = ParamRef.Ref)]
        public string StationCode { get; set; }

        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("查询结果", 10, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 输出WIP
        /// </summary>
        [Parameter("WIP", 11, CN = "WIP", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public String OutWIP { get; set; }

        //[Parameter("ThisPass", 12, CN = "ThisPass", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        //public bool ThisPass { get; set; }

        private SFCHelper _sfcHelper = null;

        public override bool DoExcute(out string errMsg)
        {
            #region 参数
            string sendStr = string.Empty;
            string recStr = string.Empty;
            OutWIP = string.Empty;
            errMsg = string.Empty;
            Result = false;
            #endregion

            if (!IsEnable || IsEmptyMode)
            {
                OutWIP = LCG;
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

            // Wip查询
            OutWIP = _sfcHelper.GetWip(LCG, WIP_Length, out errMsg);
            if (!string.IsNullOrEmpty(errMsg))
            {
                MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);
                return true;
            }

            _sfcHelper.KeyMaterial(OutWIP, out errMsg);
            if (!string.IsNullOrEmpty(errMsg))
            {
                MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);

                return true;
            }

            Result = true;
            ////3.获取WIP
            //sendStr = $"sfc_post@c=QUERY_RECORD&sn={LCG}&p=fgsn\n";
            //MyOwner.OnLog(LogType.Debug, $"PDCA发送数据,只获取WIP:{sendStr}");
            //vCommPdca.Write(sendStr);
            //// 等待结果
            //recStr = vCommPdca.ReadSingle<string>("", 5000);
            //MyOwner.OnLog(LogType.Debug, $"PDCA接收数据,只获取WIP:{recStr}");
            //var wipArray = recStr.Split('\n');

            //if (wipArray.Length > 1 && wipArray[1].Length >= WIP_Length)
            //{
            //    OutWIP = wipArray[1].Substring(5, WIP_Length);
            //    Result = true;
            //}
            ////4.获取不到WIP就需要进行绑定
            //else
            //{
            //    MyOwner.OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"通过料件查询WIP码失败:{LCG}");
            //    Result = false;
            //    return true;
            //}

            //sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd=check_partlist&sn={OutWIP}&pdca_station_code={StationCode}\n";
            //MyOwner.OnLog(LogType.Debug, $"PDCA发送数据,关键物料查询:{sendStr}");
            //vCommPdca.Write(sendStr);
            //// 等待结果
            //recStr = vCommPdca.ReadSingle<string>("", 5000);
            //MyOwner.OnLog(LogType.Debug, $"PDCA接收数据,关键物料查询:{recStr}");
            //if (recStr.Contains("OK:") || recStr.ToUpper().Contains("CHECK OK"))
            //{
            //    ThisPass = true;
            //    Result = true;
            //}
            //else
            //{
            //    ThisPass = false;
            //    Result = false;
            //}

            Result = false;

            return base.DoExcute(out errMsg);
        }

    }
}


