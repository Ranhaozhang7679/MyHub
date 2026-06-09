using HandyControl.Data;
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
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Luster.Module.Motion.Business.Functions
{
    public class SFCFlow : MotionFunction, IPauseFunction
    {
        /// <summary>
        /// SFC动作方式
        /// </summary>
        public enum SFCType
        {
            [Description("开始")]
            Start,

            [Description("治具解绑")]
            UnbindCarrier,

            [Description("治具绑定")]
            BindCarrier,

            [Description("结束")]
            End,

            [Description("关键物料")]
            KeyMaterial,

            [Description("查询载具黑名单")]
            QueryBlackCarrier,

            [Description("加入载具黑名单")]
            AddBlackCarrier,
            [Description("卷料工单查询")]
            A,
            [Description("卷料是否可以组装CCDFlag=2")]
            B,
            [Description("卷料组装绑定CCDFlag=0")]
            C,
            [Description("卷料绑定确认")]
            D,
            [Description("CG1查询黑名单")]
            E,
            [Description("RecheckAOI查询检测配方")]
            F,
            [Description("RecheckAOI上传NG")]
            G,
            [Description("RecheckAOI上传OK")]
            H,
            [Description("CG5Flex与CG绑定")]
            I,
            [Description("CG5实名制查询")]
            J,
            [Description("CG6查询排线绑定")]
            K,
            [Description("CG5查询物料黑名单")]
            QueryBlackMaterial,
            [Description("CG1绑定LCFM")]
            L,
            [Description("CG5查询Flex黑名单")]
            M,
            [Description("通过载具码查询WIP")]
            N,
            [Description("CG5Flex绑定CG_68")]
            CG5Flex绑定CG_68,
            [Description("CG5通过WIP查询排线SN")]
            O
        }

        public enum CheckFlag
        {
            [Description("黑白名单")]
            BlackList = 1,
            [Description("是否扣货")]
            IsSet = 2,
            [Description("是否扣货与黑白名单")]
            IsSetAndBlackList = 3,
        }

        /// <summary>
        /// 通信服务器
        /// </summary>
        [DependOn("IsUsePDCA", true)]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }
        /// <summary>
        /// 动作类型
        /// </summary>
        [NotEmpty]
        [Parameter("动作类型", 0, CN = "动作类型", DefaultV = SFCType.Start)]
        public SFCType SfcMode { get; set; }

        [Parameter("SFC启用", 1, CN = "SFC启用", CanRef = ParamRef.Ref)]
        public bool IsSFCEnable { get; set; }

        [Parameter("是否是首站", 2, CN = "首站")]
        public bool IsFirstStation { get; set; }

        [DependOn("SfcMode", SFCType.Start)]
        [Parameter("是否强制本站名称", 0, CN = "强制本站名称", DefaultV = false)]
        public bool IsForceStationName { get; set; } = false;

        [DependOn("SfcMode", SFCType.Start)]
        [DependOn("IsForceStationName", true)]
        [Parameter("本站名称", 2, CN = "本站名称")]
        public string ForceStationName { get; set; }

        [DependOn("SfcMode", SFCType.J)]
        [Parameter("RnsStation", 2, CN = "RnsStation")]
        public string RnsStation { get; set; }

        [DependOn("SfcMode", SFCType.Start)]
        [DependOn("IsForceStationName", true)]
        [Parameter("本站Type", 2, CN = "本站Type")]
        public string ForceStationType { get; set; }

        [Parameter("是否查询上一站结果", 3, CN = "上站查询", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool QueryPrevStation { get; set; }


        [DependOn("IsFirstStation", true)]
        [Parameter("是否解绑治具码", 4, CN = "是否解绑")]
        public bool IsUnBind { get; set; }

        [DependOn("IsFirstStation", true)]
        [Parameter("根据工单自动生成WIP", 5, CN = "自动绑定", CanRef = ParamRef.Ref, DefaultV = true)]
        public bool IsAutoBuildWip { get; set; }

        [Parameter("SN编码", 6, CN = "SN编码", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        [DependOn("SfcMode", SFCType.F)]
        [Parameter("查两次", 5, CN = "查两次", CanRef = ParamRef.Ref)]
        public bool IsTwo { get; set; }

        [DependOn("SfcMode", SFCType.L)]
        [DependOn("IsFirstStation", true)]
        [Parameter("LCFM", 7, CN = "LCFM", CanRef = ParamRef.Ref)]
        public string LCFM { get; set; }

        [DependOn("SfcMode", SFCType.UnbindCarrier, SFCType.BindCarrier, SFCType.E, SFCType.N)]
        [Parameter("治具码", 8, CN = "治具码", CanRef = ParamRef.Ref)]
        public string CarrierSN { get; set; }

        [Parameter("Wip码长度", 9, CN = "Wip码长度", DefaultV = 23)]
        public int WipLength { get; set; }
        [DependOn("SfcMode", SFCType.F)]
        [Parameter("Wip码长度2", 9, CN = "Wip码长度2", DefaultV = 23)]
        public int WipLength2 { get; set; }

        [DependOn("SfcMode", SFCType.End)]
        [Parameter("上传Pass/Fail", 10, CN = "结果", CanRef = ParamRef.Ref, DefaultV = true)]
        public bool IsOK { get; set; }

        [DependOn("SfcMode", SFCType.F)]
        [DependOn("SfcMode", SFCType.G)]
        [DependOn("SfcMode", SFCType.H)]
        [Parameter("SFCUrl2", 11, CN = "SFCUrl2", CanRef = ParamRef.Ref)]
        public string SFCUrl2 { get; set; }

        [DependOn("SfcMode", SFCType.F)]
        [DependOn("SfcMode", SFCType.G)]
        [DependOn("SfcMode", SFCType.H)]
        [Parameter("UseSFCUrl2", 11, CN = "UseSFCUrl2", CanRef = ParamRef.Ref)]
        public bool UseSFCUrl2 { get; set; }

        [DependOn("SfcMode", SFCType.End)]
        [DependOn("SfcMode", SFCType.G)]
        [Parameter("NG码", 11, CN = "NGCode", CanRef = ParamRef.Ref)]
        public string ErrorCode { get; set; }

        [DependOn("SfcMode", SFCType.End)]
        [DependOn("SfcMode", SFCType.G)]
        [Parameter("NG信息", 12, CN = "NGMsg", CanRef = ParamRef.Ref)]
        public string ErrorMsg { get; set; }

        [DependOn("SfcMode", SFCType.F)]
        [DependOn("SfcMode", SFCType.G)]
        [DependOn("SfcMode", SFCType.H)]
        [DependOn("SfcMode", SFCType.J)]
        [DependOn("SfcMode", SFCType.K)]
        [DependOn("SfcMode", SFCType.QueryBlackMaterial)]
        [Parameter("stationID", 12, CN = "stationID", CanRef = ParamRef.Ref)]
        public string stationID { get; set; }

        [DependOn("SfcMode", SFCType.QueryBlackMaterial)]
        [Parameter("check_flag", 12, CN = "check_flag", CanRef = ParamRef.Ref)]
        public CheckFlag checkFlag { get; set; }

        [DependOn("SfcMode", SFCType.F)]
        [Parameter("sfcStationCode", 12, CN = "sfcStationCode", CanRef = ParamRef.Ref)]
        public string sfcStationCode { get; set; }


        [DependOn("SfcMode", SFCType.G)]
        [Parameter("test_station_name", 12, CN = "test_station_name", CanRef = ParamRef.Ref)]
        public string test_station_name { get; set; }


        [DependOn("SfcMode", SFCType.G)]
        [DependOn("SfcMode", SFCType.H)]
        [Parameter("product", 12, CN = "product", CanRef = ParamRef.Ref)]
        public string product { get; set; }

        [DependOn("SfcMode", SFCType.H)]
        [Parameter("StationName", 12, CN = "StationName", CanRef = ParamRef.Ref)]
        public string StationName { get; set; }


        [DependOn("SfcMode", SFCType.F)]
        [DependOn("SfcMode", SFCType.G)]
        [DependOn("SfcMode", SFCType.H)]
        [Parameter("mac_address", 12, CN = "mac_address", CanRef = ParamRef.Ref)]
        public string mac_address { get; set; }

        [DependOn("SfcMode", SFCType.Start)]
        [Parameter("本站强制未做", 13, CN = "本站强制未做", DefaultV = false, CanRef = ParamRef.Ref)]
        public bool DefaultNotPass { get; set; }

        [DependOn("SfcMode", SFCType.I)]
        [Parameter("ccdflag", 13, CN = "ccdflag", DefaultV = false, CanRef = ParamRef.Ref)]
        public string ccdflag { get; set; }

        [DependOn("SfcMode", SFCType.I)]
        [DependOn("SfcMode", SFCType.CG5Flex绑定CG_68)]

        [Parameter("partname", 13, CN = "partname", DefaultV = false, CanRef = ParamRef.Ref)]
        public string partname { get; set; }

        [DependOn("SfcMode", SFCType.I)]
        [DependOn("SfcMode", SFCType.J)]
        [DependOn("SfcMode", SFCType.CG5Flex绑定CG_68)]
        [Parameter("FlexSN", 13, CN = "FlexSN", DefaultV = false, CanRef = ParamRef.Ref)]
        public string FlexSN { get; set; }

        [DependOn("SfcMode", SFCType.J)]
        [Parameter("QueryRealTime", 13, CN = "实名制查询时间", DefaultV = "2020-12-12 12:12:12", CanRef = ParamRef.Ref)]
        public string QueryRealTime { get; set; }

        [DependOn("SfcMode", SFCType.F)]
        [Parameter("isTryrun", 13, CN = "isTryrun", DefaultV = false, CanRef = ParamRef.Ref)]
        public bool isTryrun { get; set; }



        //[DependOn("SfcMode", SFCType.AddBlackCarrier)]
        //[Parameter("打黑设定次数", 14, CN = "打黑设定次数", DefaultV = 3)]
        //public int SetCount { get; set; }

        //[NotEmpty]
        //[DependOn("SfcMode", SFCType.AddBlackCarrier)]
        //[Parameter("NG", 15, CN = "NG", CanRef = ParamRef.Ref, DefaultV = false)]
        //public bool NG { get; set; }


        [NotEmpty]
        [Parameter("查询维修", 16, CN = "查询维修", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsQueryRepair { get; set; }


        [Parameter("是否使用PDCA上传", 1, CN = "MacMini工站")]
        public bool IsUsePDCA { get; set; } = true;
        [NotEmpty]
        [Parameter("报警代码", 16, CN = "报警代码", CanRef = ParamRef.Ref, DefaultV = "F99OOOO-01")]
        public string AlarmCode { get; set; }

        [DependOn("IsFirstStation", true)]
        [Parameter("无LCFM", 17, CN = "无LCFM", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsNoLCFM { get; set; }


        [DependOn("SfcMode", SFCType.F)]
        [Parameter("RechecKAOI配方", 20, CN = "配方", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string recipe { get; set; }

        [Parameter("通信结果", 20, CN = "最终结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [DependOn("SfcMode", SFCType.J)]
        [Parameter("实名制记录次数", 20, CN = "实名制记录次数", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int RealTimes { get; set; }

        //[DependOn("SfcMode", SFCType.J)]
        //[Parameter("通信结果", 20, CN = "CG5查询实名制结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        //public bool ResultRealName { get; set; }

        //[DependOn("SfcMode", SFCType.K)]
        //[Parameter("通信结果", 20, CN = "CG6查询排线绑定结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        //public bool ResultCableBinding { get; set; }

        [DependOn("SfcMode")]
        [Parameter("治具码", 20, CN = "治具码", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string JigCode { get; set; } = string.Empty;

        [DependOn("SfcMode")]
        [Parameter("通信结果", 20, CN = "本站Pass", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool ThisPass { get; set; }

        [DependOn("SfcMode")]
        [Parameter("Wip编码", 20, CN = "Wip码", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string Wip { get; set; }

        [DependOn("SfcMode")]
        [Parameter("卷料绑定OK", 20, CN = "卷料绑定OK", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool RollOK { get; set; }

        [DependOn("SfcMode", SFCType.O)]
        [Parameter("CableSN", 20, CN = "排线SN", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string CableSN { get; set; }

        [DependOn("SfcMode", SFCType.B)]
        [Parameter("上传/不上传", 20, CN = "查询卷料是否上传", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string RollResult { get; set; }
        /// <summary>
        /// 工单数据
        /// </summary>
        [DependOn("IsFirstStation", false)]
        [Parameter("3A查询上站是否做过次数", 20, CN = "3A查询次数", DefaultV = 1, ParamType = ParamType.OUT)]
        public int QueryPreCount { get; set; }

        /// <summary>
        /// 是否是黑名单
        /// </summary>
        [DependOn("SfcMode", SFCType.QueryBlackCarrier)]
        [DependOn("SfcMode", SFCType.AddBlackCarrier)]
        [DependOn("SfcMode", SFCType.E)]
        [DependOn("SfcMode", SFCType.K)]
        [Parameter("是否是黑名单", 20, CN = "黑名单", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool IsBlackCarrier { get; set; }





        //[DependOn("SfcMode", SFCType.AddBlackCarrier)]
        //[Parameter("黑名单计次", 20, CN = "黑名单计次", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        //public int CurrentCount { get; set; }

        [Parameter("左工位true，右工位fals", 1, CN = "左工位")]
        public bool IsStationLeft { get; set; } = true;
        /// <summary>
        /// 是否维修过
        /// </summary>
        [DependOn("IsQueryRepair", true)]
        [Parameter("维修机", 20, CN = "维修机", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool IsRepaired { get; set; }


        [Parameter("SFCOK返回值", 7, CN = "sfc返回值", CanRef = ParamRef.Ref)]
        public string ReturnOKValue { get; set; }

        [DependOn("SfcMode", SFCType.J)]
        [Parameter("Flex工站ID", 21, CN = "Flex工站", DefaultV = "X12", CanRef = ParamRef.Ref)]
        public string FlexStationID { get; set; }

        //[Parameter("工单查询失败", 21, CN = "工单查询失败", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        //public bool OrderZero { get; set; }

        /// <summary>
        /// web 通信组件
        /// </summary>
        private WebTool webTool;
        private const string OKChar = "0 SFC_OK";




        public SFCFlow()
        {
            this.Tips = "SFC流程处理";
            this.Icon = "\xe6a8";
        }

        /// <summary>
        /// SFCHelper
        /// </summary>
        private SFCHelper _sfcHelper = null;
        private SFCHelper _sfcHelper1 = null;

        public override bool DoExcute(out string errMsg)
        {

            errMsg = "";
            QueryPreCount = 1;
            // 默认是失败
            Result = false;
            ThisPass = false;
            //// 空跑模式
            if (IsEmptyMode || !IsSFCEnable)
            {
                Wip = nameof(IsEmptyMode);
                JigCode = nameof(IsEmptyMode);
                Result = true;
                ThisPass = false;
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
            switch (SfcMode)
            {
                //入料
                case (SFCType.Start):

                    // Wip 码获取,首站不需要3A
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg, IsFirstStation);

                    // CG1-1和CG1-2
                    if (IsFirstStation /*&& string.IsNullOrEmpty(Wip)*/)
                    {
                        if (string.IsNullOrEmpty(Wip))
                        {
                            // 如果启动自动分配
                            if (IsAutoBuildWip)
                            {
                                // 工单余量查询
                                if (!MyOwner.OrderManager.HasOrder(out var orderNo))
                                {
                                    MyOwner.OnAlarm(AlarmType.WarningTip, $"工单不存在,请扫工单", AlarmCode);
                                    //OrderZero = true;
                                    return true;
                                }

                                int surplus = _sfcHelper.GetOrderSurplus(orderNo, out errMsg);
                                if (!string.IsNullOrEmpty(errMsg) || surplus == 0)
                                {
                                    MyOwner.OnAlarm(AlarmType.WarningTip, errMsg, AlarmCode);
                                    return true;
                                }

                            // 判断LCFM和LCG是否匹配
                            // 且要有LCFM码
                            if (IsFirstStation&&!IsNoLCFM)
                            {
                                if (string.IsNullOrEmpty(LCFM))
                                {
                                    MyOwner.OnAlarm(AlarmType.InfoTip, $"LCFM扫码失败！！！", AlarmCode);
                                    return true;
                                }

                                _sfcHelper.MatchLCGAndLCFM(SN, LCFM, out errMsg);
                                if (!string.IsNullOrEmpty(errMsg))
                                {
                                    MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                                    if (LCFM.StartsWith("NG_"))
                                    {
                                        MyOwner.OnAlarm(AlarmType.InfoTip, $"LCFM扫码失败！！！", AlarmCode);
                                    }
                                    return true;
                                }
                            }

                                // 自动分配Wip
                                Wip = _sfcHelper.AutoWip(orderNo, SN, LCFM, WipLength, out errMsg);
                                if (!string.IsNullOrEmpty(errMsg))
                                {
                                    //返回失败
                                    // 如果Wip码获取，可能是网络返回慢，不一定是真的分配失败
                                    // 这个时候可以再获取下WIP
                                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg, IsFirstStation);

                                // 需要再绑定LCFM，因为自动分配WIP时，那边退出了，并没有绑定LCFM
                                if (!string.IsNullOrEmpty(Wip))
                                {
                                    if(!IsNoLCFM)
                                    {
                                        _sfcHelper.BindingLCFM(Wip, LCFM, out errMsg);
                                        if (!string.IsNullOrEmpty(errMsg))
                                        {
                                            MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                                            return true;
                                        }
                                    }
                                }
                                // 如果wip为空，则再报警
                                // 否则是分配成功的
                                else
                                {
                                    MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                                    return true;
                                }
                            }

                                // 自动分配完成后，再次查询余量，保证余量是最新的数据
                                surplus = _sfcHelper.GetOrderSurplus(orderNo, out errMsg);
                                MyOwner.OrderManager.UpdateOrder(orderNo, surplus);
                            }
                            else
                            {
                                // 如果Wip码获取失败并且没有开启自动分配
                                if (!string.IsNullOrEmpty(errMsg))
                                {
                                    MyOwner.OnAlarm(AlarmType.InfoTip, $"Wip获取错误:{errMsg}", AlarmCode);
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            // 判断LCFM和LCG是否匹配
                            _sfcHelper.MatchLCGAndLCFM(SN, LCFM, out errMsg);
                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                                if (LCFM.StartsWith("NG_"))
                                {
                                    MyOwner.OnAlarm(AlarmType.InfoTip, $"LCFM扫码失败！！！", AlarmCode);
                                }
                                return true;
                            }

                            LogTool.Debug($"首站已绑定过Wip，Wip:{Wip}, LCFM:{LCFM}");
                            if (!string.IsNullOrEmpty(Wip))
                            {
                                _sfcHelper.BindingLCFM(Wip, LCFM, out errMsg);
                                if (!string.IsNullOrEmpty(errMsg))
                                {
                                    LogTool.Debug($"LCFM绑定失败，Wip:{Wip}, LCFM:{LCFM}");
                                    MyOwner.OnAlarm(AlarmType.InfoTip, $"LCFM绑定失败", AlarmCode);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 如果Wip码获取失败
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                            return true;
                        }
                        //由于前站查询可能失败,导致下面的JigCode为Null,导致停机,移到后面查询
                        //// 前站查询
                        //if (QueryPrevStation)
                        //{
                        //    QueryPreCount = _sfcHelper.QueryUpStation(Wip, out errMsg);
                        //    if (!string.IsNullOrEmpty(errMsg))
                        //    {
                        //        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        //        this.Result = false;
                        //        return true;
                        //    }
                        //}
                    }

                    // 首站查询关键物料
                    if (IsFirstStation)
                    {
                        _sfcHelper.KeyMaterial(Wip, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            if(!IsNoLCFM)
                            {
                              _sfcHelper.MatchLCGAndLCFM(SN, LCFM, out errMsg);
                            }
                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                                return true;
                            }
                            if (!IsNoLCFM)
                            {
                                _sfcHelper.BindingLCFM(Wip, LCFM, out errMsg);
                            }
                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                                return true;
                            }

                            _sfcHelper.KeyMaterial(Wip, out errMsg);
                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                                return true;
                            }
                        }
                    }

                    // 获取绑定载具,如果通过Wip没有获取到载具，直接下一片产品
                    JigCode = _sfcHelper.GetCarrierSN(Wip, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        this.Result = false;
                        return true;
                    }

                    //由于前站查询可能失败,导致下面的JigCode为Null,导致停机,移到后面查询
                    if (!IsFirstStation && QueryPrevStation)
                    {
                        // 前站查询
                        QueryPreCount = _sfcHelper.QueryUpStation(Wip, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                            this.Result = false;
                            return true;
                        }
                    }

                    // 是否解绑载具
                    // 载具号不为空，再解绑
                    // 默认解绑成功，不对解绑结果进行判断
                    if (IsUnBind)
                    {
                        if (!string.IsNullOrEmpty(JigCode))
                        {
                            _sfcHelper.UnbindCarrier(JigCode, out errMsg);
                        }
                    }

                    // 判定本站是否做过
                    if (!IsFirstStation)
                    {
                        if (IsForceStationName)
                        {
                            this.ThisPass = _sfcHelper.QueryThisPass(Wip, out errMsg, ForceStationName, ForceStationType);
                        }
                        else
                        {
                            this.ThisPass = _sfcHelper.QueryThisPass(Wip, out errMsg);
                        }
                    }
                    //判断是否强制
                    if (DefaultNotPass)
                    {
                        this.ThisPass = false;
                    }

                    //查询维修记录,需要查询才会判断
                    if (IsQueryRepair)
                    {
                        IsRepaired = _sfcHelper.QueryRepairRecord(Wip, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            // 解绑失败 直接返回，由流程自己判定处理
                            //MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                            LogTool.Debug($"治具解绑失败，{errMsg}");
                            Result = false;
                            return true;

                        }
                    }
                    // 最终结果代码查询没有出现异常
                    Result = true;
                    break;

                // 解绑治具
                case SFCType.UnbindCarrier:

                    // 查询载具有没有
                    _sfcHelper.QueryCarrier(CarrierSN, out errMsg);
                    if (string.IsNullOrEmpty(errMsg))
                    {
                        _sfcHelper.UnbindCarrier(CarrierSN, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            LogTool.Debug($"绑定载具失败，{errMsg}");
                            //MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                            Result = false;
                            return true;
                        }

                        Result = true;
                    }

                    break;

                //绑定载具
                case SFCType.BindCarrier:

                    // Wip 输出
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }

                    _sfcHelper.BindCarrier(CarrierSN, Wip, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        // 解绑失败弹出提示信息
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }

                    Result = true;
                    break;

                //出料
                case SFCType.End:

                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }

                    if (IsFirstStation)
                    {
                        _sfcHelper.BindCarrier(CarrierSN, Wip, out errMsg);
                        if (!string.IsNullOrEmpty(errMsg))
                        {
                            MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                            return true;
                        }
                    }

                    _sfcHelper.UploadResult(Wip, IsOK, ErrorMsg, ErrorCode, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }

                    Result = true;

                    break;
                case SFCType.KeyMaterial:

                    // Wip查询
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }

                    _sfcHelper.KeyMaterial(Wip, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);

                        return true;
                    }

                    Result = true;
                    break;

                case SFCType.QueryBlackCarrier:
                    //0.通过WIP拿到载具码
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg, IsFirstStation);
                    CarrierSN = _sfcHelper.GetCarrierSN(Wip, out errMsg);
                    //1.查询载具是否在黑名单内,如果在直接解绑
                    if (_sfcHelper.QueryCarrierBlackList(CarrierSN, ReturnOKValue, out string msg))
                    {

                        IsBlackCarrier = true;
                    }
                    else
                    {
                        IsBlackCarrier = false;
                    }
                    Result = true;
                    break;

                case SFCType.AddBlackCarrier:
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg, IsFirstStation);
                    //0.通过WIP拿到载具码
                    CarrierSN = _sfcHelper.GetCarrierSN(Wip, out errMsg);
                    //2.计黑&3.打黑
                    if (_sfcHelper.AddCarrierBlackList(CarrierSN, ReturnOKValue, out msg))
                    {
                        IsBlackCarrier = true;
                    }
                    else
                    {
                        IsBlackCarrier = false;
                    }
                    Result = true;
                    break;

                case SFCType.A:
                    if (!MyOwner.RollManager.HasOrder(IsStationLeft, out var orderNo2))
                    {
                        MyOwner.OnAlarm(AlarmType.WarningTip, $"工单不存在,请扫工单");
                        errMsg = "工单不存在,请扫工单";
                        return false;
                    }
                    var lot = _sfcHelper.QueryHandingLot(orderNo2, out errMsg);
                    break;
                case SFCType.B:
                    if (!MyOwner.RollManager.HasOrder(IsStationLeft, out var orderNo3))
                    {
                        MyOwner.OnAlarm(AlarmType.WarningTip, $"工单不存在,请扫工单");
                        errMsg = "工单不存在,请扫工单";
                        return false;
                    }
                    string wip3 = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    Wip = wip3;
                    Result = _sfcHelper.QueryHanding(wip3, "2", orderNo3, out string status, out errMsg);
                    RollResult = status;
                    break;
                case SFCType.C:
                    if (!MyOwner.RollManager.HasOrder(IsStationLeft, out var orderNo4))
                    {
                        MyOwner.OnAlarm(AlarmType.WarningTip, $"工单不存在,请扫工单");
                        errMsg = "工单不存在,请扫工单";
                        return false;
                    }
                    string wip2 = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    Wip = wip2;
                    Result = _sfcHelper.QueryHanding(wip2, "0", orderNo4, out string status1, out errMsg);
                    var surplus1 = _sfcHelper.QueryHandingLot(orderNo4, out errMsg);
                    MyOwner.RollManager.UpdateOrder(orderNo4, surplus1);
                    break;
                case SFCType.D:
                    string wip1 = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    Wip = wip1;
                    Result = _sfcHelper.QueryLotCheck(wip1, out errMsg);
                    break;
                case SFCType.E:
                    if (_sfcHelper.QueryCarrierBlackList(CarrierSN, ReturnOKValue, out string msg1))
                    {
                        IsBlackCarrier = true;
                    }
                    else
                    {
                        IsBlackCarrier = false;
                    }
                    Result = true;
                    break;
                case SFCType.F:
                    string wip123 = string.Empty;

                    if (IsTwo)
                    {
                        string a = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                        wip123 = _sfcHelper.GetWip(a, WipLength2, out errMsg);
                        Wip = wip123;
                    }
                    else
                    {
                        wip123 = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                        Wip = wip123;
                    }
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }
                    if (UseSFCUrl2)
                    {
                        _sfcHelper1 = new SFCHelper(SFCUrl2);
                    }
                    recipe = _sfcHelper1.GetRecheckAOI(wip123, stationID, sfcStationCode, mac_address, isTryrun, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }
                    else
                    {
                        Result = true;
                        return true;
                    }
                    break;
                case SFCType.G:
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }
                    Result = _sfcHelper.UpdataErrorCode(Wip, DateTime.Now.AddSeconds(-5).ToString("yyyy-MM-dd HH:mm:ss"), ErrorCode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ErrorMsg, stationID, test_station_name, product, mac_address, out errMsg);
                    break;
                case SFCType.H:
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        Result = false;
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }
                    _sfcHelper.UploadResultOK(Wip, stationID, StationName, product, mac_address, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        Result = false;
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }
                    else
                    {
                        Result = true;
                    }
                    break;
                case SFCType.I:
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    Result = _sfcHelper.BindingATLF(Wip, FlexSN, partname, ccdflag, ReturnOKValue, out errMsg);
                    break;
                case SFCType.J:
                    //Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    //ResultRealName = _sfcHelper.QueryCG5RealName(stationID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), stationID, out errMsg);
                    RealTimes = 0;
                    (Result, RealTimes) = _sfcHelper.QueryCG5RealNameNew(FlexSN, RnsStation, QueryRealTime, stationID, out errMsg);
                    break;
                case SFCType.K:
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    Result = _sfcHelper.QueryCG6CableBinding(Wip, stationID, out errMsg);
                    break;
                //CG1-LCG与LCFM绑定
                case SFCType.L:
                    _sfcHelper.MatchLCGAndLCFM(SN, LCFM, out errMsg);
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    _sfcHelper.BindingLCFM(Wip, LCFM, out errMsg);
                    _sfcHelper.KeyMaterial(Wip, out errMsg);
                    break;
                //CG5查询Flex黑名单
                case SFCType.QueryBlackMaterial:
                    Result = _sfcHelper.QueryMaterialBlackList(SN, (int)checkFlag, stationID, out errMsg);
                    break;
                //通过载具查询wip码
                case SFCType.N:
                    Wip = _sfcHelper.QueryWipByCarrierSn(CarrierSN, out errMsg);
                    //查询维修记录,需要查询才会判断
                    if (IsQueryRepair)
                    {
                        IsRepaired = _sfcHelper.QueryRepairRecord(Wip, out errMsg);
                    }
                    break;
                //68 CG5 排线绑定
                case SFCType.CG5Flex绑定CG_68:
                    Wip = _sfcHelper.GetWip(SN, WipLength, out errMsg);
                    string smallPartWip = _sfcHelper.GetSmallPartWip(FlexSN, WipLength, out errMsg);
                    if(_sfcHelper.FlexBindingResult(Wip, smallPartWip, partname, out errMsg)||true)
                    {
                        Result = _sfcHelper.BindingFlex(Wip, smallPartWip,partname, out errMsg);
                    }
                    break;
                // CG5通过WIP查询排线SN
                case SFCType.O:
                    CableSN = _sfcHelper.QueryCableSN(SN, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg, AlarmCode);
                        return true;
                    }
                    Result = !string.IsNullOrEmpty(CableSN);
                    break;
            }

            return base.DoExcute(out errMsg);
        }
    }
}