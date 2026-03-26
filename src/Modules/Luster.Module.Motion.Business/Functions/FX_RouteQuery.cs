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
using Luster.Motion.Integration.SFC;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 工单查询
    /// </summary>
    public class FX_RouteQuery : MotionFunction, IPauseFunction
    {
        /// <summary>
        /// 同时只允许一个PDCA进行访问
        /// </summary>
        public static object lockPDCA = new object();

        public FX_RouteQuery()
        {
            this.Tips = "SFC路由查询";
            this.Icon = "\xe6c9";
        }

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

        /// <summary>
        /// 是否启用CPK,如果启用CPK，不在进行数据查询
        /// </summary>
        [Parameter("是否启用CPK", 2, CN = "CPK启用", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool CPKEnable { get; set; }

        [Parameter("SN编码", 3, CN = "SN编码", CanRef = ParamRef.Ref)]
        public string LCG { get; set; }

        [Parameter("前站查询启用", 4, CN = "前站查询启用", CanRef = ParamRef.Ref)]
        public bool PreQuery { get; set; }


        [Parameter("本站查询启用", 5, CN = "本站查询启用", CanRef = ParamRef.Ref)]
        public bool RedoQuery { get; set; }

        /// <summary>
        /// WIP长度
        /// </summary>
        [Parameter("WIP长度", 6, CN = "WIP长度", DefaultV = 18)]
        public int WIP_Length { get; set; }

        [Parameter("关键物料查询", 7, CN = "关键物料查询", DefaultV = false)]
        public bool CheckPartList { get; set; }

        [Parameter("本站强制未做", 8, CN = "本站强制未做", DefaultV = false,CanRef =ParamRef.Ref)]
        public bool DefaultNotPass { get; set; }

        [Parameter("是否通过载具查询WIP", 9, CN = "是否通过载具查询WIP", DefaultV = false)]
        public bool IsQueryByCarrierSN { get; set; }


        [Parameter("3A查询上站是否做过次数", 20, CN = "3A查询次数", DefaultV = 1, ParamType = ParamType.OUT)]
        public int QueryPreCount { get; set; }



        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("查询结果", 10, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }
        [Parameter("是否使用PDCA上传", 1, CN = "MacMini工站")]
        public bool IsUsePDCA { get; set; } = true;
        /// <summary>
        /// 输出WIP
        /// </summary>
        [Parameter("WIP", 11, CN = "WIP", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public String OutWIP { get; set; }

        [Parameter("ThisPass", 12, CN = "ThisPass", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool ThisPass { get; set; }

        [Parameter("治具码", 13, CN = "治具码", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string JigCode { get; set; }

        [Parameter("卷料启用", 14, CN = "卷料启用", CanRef = ParamRef.Ref)]
        public bool SmallPart { get; set; }

        [Parameter("关键物料查询结果", 20, CN = "关键物料查询结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool CheckPartListResult { get; set; }

        [Parameter("CG3使用", 15, CN = "允许组装卷料", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool LotPass { get; set; }

        [Parameter("左工位true，右工位fals", 1, CN = "左工位")]
        public bool IsStationLeft { get; set; } = true;

        [Parameter("CG3 True，CG4 False", 16, CN = "CG3/CG4", CanRef = ParamRef.Ref)]
        public bool SmallCheck { get; set; }

        [Parameter("CG4使用", 17, CN = "卷料组装OK", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool LotCheckPass { get; set; }

        private SFCHelper _sfcHelper = null;

        public override bool DoExcute(out string errMsg)
        {
            #region 参数
            string sendStr = string.Empty;
            string recStr = string.Empty;
            OutWIP = string.Empty;
            errMsg = string.Empty;
            Result = false;
            ThisPass = false;
            CheckPartListResult = true;
            #endregion
            
            if (!IsEnable || IsEmptyMode || CPKEnable)
            {
                QueryPreCount = 1;
                ThisPass = false;
                Result = true;
                JigCode = nameof(IsEmptyMode);

                if (CPKEnable)
                {
                    MyOwner.OnLog(LogType.Warning, $"模式是CPK模式，上站不进行查询");
                }
                return true;
            }
            if (IsUsePDCA)
            {
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
           

            lock (lockPDCA)
            {
                // Wip 码获取
                if(IsQueryByCarrierSN)
                {
                    OutWIP=_sfcHelper.QueryWipByCarrierSn(LCG,out errMsg);
                }
                else
                {
                    OutWIP = _sfcHelper.GetWip(LCG, WIP_Length, out errMsg);

                }

                // 如果Wip码获取失败
                if (!string.IsNullOrEmpty(errMsg))
                {
                    MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);
                    return true;
                }

                if (PreQuery)
                {
                    QueryPreCount = _sfcHelper.QueryUpStation(OutWIP, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);
                        return true;
                    }
                }
                // 本站是否做过
                if (RedoQuery)
                {
                    this.ThisPass = _sfcHelper.QueryThisPass(OutWIP, out errMsg);
                }
                if (SmallPart)
                {
                    
                    if (SmallCheck)
                    {
                        if (!MyOwner.RollManager.HasOrder(IsStationLeft, out var orderNo1))
                        {
                            MyOwner.OnAlarm(AlarmType.WarningTip, $"工单不存在,请扫工单");
                            errMsg = "工单不存在,请扫工单";
                            return false;
                        }
                        var lot = _sfcHelper.QueryHandingLot(orderNo1, out errMsg);
                        if (lot > 0)
                        {
                            this.LotPass = _sfcHelper.QueryHanding(OutWIP, "2", orderNo1,out string status, out errMsg);
                        }
                        else
                        {
                            this.LotPass = false;
                        }
                    }
                    else
                    {
                        this.LotCheckPass = _sfcHelper.QueryLotCheck(OutWIP, out errMsg);
                    }
                }
                
                // 本站强制未做
                if (DefaultNotPass)
                {
                    this.ThisPass = false;
                }

                JigCode = _sfcHelper.GetCarrierSN(OutWIP, out errMsg);
                if (!string.IsNullOrEmpty(errMsg))
                {
                    OnAlarm(AlarmType.InfoTip, errMsg);
                    return true;
                }

                // 如果载具没有获取到，自动创建一个载具编码用于程序使用
                if (string.IsNullOrEmpty(JigCode))
                {
                    //JigCode = DateTime.Now.ToString("yyyyMMddHHmmss");
                    //用固定载具码，用于判断
                    JigCode = "NoCarrier";
                }

                // 关键物料查询
                if (CheckPartList)
                {
                    _sfcHelper.KeyMaterial(OutWIP, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        CheckPartListResult = false;
                        return true;
                    }
                    else
                    {
                        CheckPartListResult = true;
                    }
                }
            }
            // 最终运行成功
            Result = true;

            

            return base.DoExcute(out errMsg);
        }

    }
}


