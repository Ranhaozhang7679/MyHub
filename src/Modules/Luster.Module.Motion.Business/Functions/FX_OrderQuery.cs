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
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.Integration.SFC;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 工单查询
    /// </summary>
    public class FX_OrderQuery : MotionFunction, IPauseFunction
    {
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

        [Parameter("LCFM", 2, CN = "LCFM", CanRef = ParamRef.Ref)]
        public string LCFM { get; set; }

        [Parameter("查询不到WIP是否自动绑定工单", 3, CN = "自动绑定WIP", CanRef = ParamRef.Ref)]
        public bool IsAutoBuildWip { get; set; }

        //[Parameter("本站ID", 4, CN = "本站ID", CanRef = ParamRef.Ref)]
        //public string StationID { get; set; }

        //[Parameter("本站站名", 5, CN = "本工站名", CanRef = ParamRef.Ref)]
        //public string StationName { get; set; }

        //[Parameter("站号", 10, CN = "站号", DefaultV = 1)]
        //public int StationNum { get; set; }

        //[Parameter("线体号", 6, CN = "线号", DefaultV = "FXGL_C02-3FC-01", CanRef = ParamRef.Ref)]
        //public string LineName { get; set; }


        ///// <summary>
        ///// 工单数据
        ///// </summary>
        //[Parameter("工单数据，如果有多个工单请以$分隔开", 13, CN = "工单")]
        //public LStringEx WorkOrder { get; set; }

        //[Parameter("MacAddress", 8, CN = "Mac地址", CanRef = ParamRef.Ref)]
        //public string MacAddress { get; set; }

        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("工单绑定结果", 10, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 输出WIP
        /// </summary>
        [Parameter("WIP", 11, CN = "WIP", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutWIP { get; set; }

        private SFCHelper _sfcHelper = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public FX_OrderQuery()
        {
            this.Tips = "SFC工单查询";
            this.Icon = "\xe6c9";
        }

        public override bool DoExcute(out string errMsg)
        {
            #region 参数
            string sendStr = string.Empty;
            string recStr = string.Empty;

            OutWIP = string.Empty;
            errMsg = string.Empty;
            #endregion

            // 默认结果输出为False，只有流程走完，才为True
            Result = false;

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

            // Wip 码获取
            OutWIP = _sfcHelper.GetWip(LCG, WIP_Length, out errMsg);

            // CG1.1和CG1.2
            if (string.IsNullOrEmpty(OutWIP))
            {
                // 如果启动自动分配
                if (IsAutoBuildWip)
                {
                    // 工单余量查询
                    if (!MyOwner.OrderManager.HasOrder(out var orderNo))
                    {
                        MyOwner.OnAlarm(AlarmType.WarningTip, $"工单不存在,请扫工单");
                        return true;
                    }

                    int surplus = _sfcHelper.GetOrderSurplus(orderNo, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg) || surplus == 0)
                    {
                        MyOwner.OnAlarm(AlarmType.WarningTip, errMsg);
                        return true;
                    }

                    // 判断LCFM和LCG是否匹配
                    _sfcHelper.MatchLCGAndLCFM(LCG, LCFM, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);
                        if(LCFM.StartsWith("NG_"))
                        {
                            MyOwner.OnAlarm(AlarmType.InfoTip, $"LCFM扫码失败！！！");
                        }
                        return true;
                    }

                    // 自动分配Wip
                    OutWIP = _sfcHelper.AutoWip(orderNo, LCG, LCFM, WIP_Length, out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);
                        return true;
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
                        // 如果Wip码获取失败
                        MyOwner.OnAlarm(AlarmType.InfoTip, $"Wip获取错误:{errMsg}");
                        return true;
                    }
                }
            }
            else
            {
                // 如果Wip码获取失败
                if (!string.IsNullOrEmpty(errMsg))
                {
                    MyOwner.OnAlarm(AlarmType.InfoTip, errMsg);
                    return true;
                }
            }

            // 首战查询关键物料
            _sfcHelper.KeyMaterial(OutWIP, out errMsg);
            if (!string.IsNullOrEmpty(errMsg))
            {
                _sfcHelper.BindingLCFM(OutWIP, LCFM, out errMsg);
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
            //    //是否需要自动绑定
            //    if (IsAutoBuildWip)
            //    {
            //        //1.需要判断是否有工单
            //        if (!MyOwner.OrderManager.HasOrder(out var orderNum))
            //        {
            //            MyOwner.OnAlarm(AlarmType.InfoTip, $"请扫入工单！！！");
            //            Result = false;
            //        }
            //        else
            //        {
            //            MyOwner.OnLog(LogType.Debug, $"当前工单为{orderNum}！！！");
            //            //设备发送工单号查询工单余量
            //            sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd=get_wip_color_info&wo_no={orderNum}&type=WOQTY\n";
            //            MyOwner.OnLog(LogType.Debug, $"2.1.4发送设备发送工单号查询工单余量:{sendStr}");
            //            vCommPdca.Write(sendStr);
            //            recStr = vCommPdca.ReadSingle<string>("", 5000);
            //            MyOwner.OnLog(LogType.Debug, $"2.1.4接收设备发送工单号查询工单余量:{recStr}");
            //            //判断工单余量是否为INVAlID Data
            //            if (recStr.Contains("INVAlID"))
            //            {
            //                MyOwner.OnAlarm(AlarmType.InfoTip, $"工单余量为非法数据，请联系相关人员查看！！！");
            //                Result = false;
            //                return true;
            //            }
            //            else if (int.TryParse(recStr, out var surplus))
            //            {
            //                MyOwner.OrderManager.UpdateOrder(orderNum, surplus);
            //            }

            //            //判断工单余量是否为0
            //            if (recStr == "0")
            //            {
            //                MyOwner.OnLog(LogType.Debug, $"工单{orderNum}余量为零，切换下一个工单！！！");
            //                MyOwner.OrderManager.HasOrder(out orderNum);
            //                MyOwner.OnLog(LogType.Debug, $"当前工单为{orderNum}！！！");
            //            }

            //            //var workOrderArray = WorkOrder.GetString(Owner).Split('$');
            //            //2.1.4 判断LCG与LCFM是否匹配
            //            sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd=compare_cg_to_lcfm&sn={LCG}&serial_no={LCFM}\n";
            //            MyOwner.OnLog(LogType.Debug, $"2.1.4发送判断LCG与LCFM是否匹配:{sendStr}");
            //            vCommPdca.Write(sendStr);
            //            recStr = vCommPdca.ReadSingle<string>("", 5000);
            //            MyOwner.OnLog(LogType.Debug, $"2.1.4接收判断LCG与LCFM是否匹配:{recStr}");
            //            if (recStr.Contains(OKChar))
            //            {
            //                //2.1.6 自动获取WIP
            //                //首先，虚拟一个治具码
            //                string tempCarrierId;
            //                tempCarrierId = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            //                //sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd=carrier_link_csa_part&carrier_no={tempCarrierId}&wo_no={workOrderArray[0]}&link_inf=1:{LCG}&ccd_flag=0&line={LineName}_{StationNum}_{StationID}&mac_address={MacAddress}\n";
            //                sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd=carrier_link_csa_part&carrier_no={tempCarrierId}&wo_no={orderNum}&link_inf=1:{LCG}&ccd_flag=0&line={LineName}_{StationNum}_{StationID}&mac_address={MacAddress}\n";
            //                MyOwner.OnLog(LogType.Debug, $"发送自动获取WIP:{sendStr}");
            //                vCommPdca.Write(sendStr);
            //                recStr = vCommPdca.ReadSingle<string>("", 5000);
            //                MyOwner.OnLog(LogType.Debug, $"接收自动获取WIP:{recStr}");
            //                if (recStr.Contains("0 SFC_OK\nOK"))
            //                {
            //                    wipArray = recStr.Split(':');
            //                    OutWIP = wipArray[1].Substring(0, WIP_Length);
            //                    Result = !string.IsNullOrEmpty(OutWIP);
            //                }
            //                else
            //                {
            //                    MyOwner.OnLog(LogType.Debug, $"无法自动分配WIP:{recStr}");
            //                    Result = false;
            //                    return true;

            //                }
            //            }
            //            else
            //            {
            //                MyOwner.OnLog(LogType.Debug, $"LCG与LCFM不匹配:{recStr}");
            //                Result = false;
            //                return true;
            //            }
            //        }

            //        ///5.WIP获取到了，绑定LCFM
            //        //2.1.7 绑定LCFM
            //        //扫描到的码，可能末尾带空格
            //        LCFM = LCFM.TrimEnd();
            //        sendStr = $"sfc_post@c=QUERY_4_SFC&subcmd= asy_4_sfc&sn={OutWIP}&serialno={LCFM}&partname=LCFM&ccdflag=0&line={LineName}_{StationNum}_{StationName}&mac_address={MacAddress}\n";
            //        MyOwner.OnLog(LogType.Debug, $"发送自动绑定LCFM:{sendStr}");
            //        vCommPdca.Write(sendStr);
            //        recStr = vCommPdca.ReadSingle<string>("", 5000);
            //        MyOwner.OnLog(LogType.Debug, $"接收自动绑定LCFM:{recStr}");
            //        if (!recStr.Contains("LCFM is ok to link") || recStr.ToUpper().Contains("NG"))
            //        {
            //            MyOwner.OnLog(LogType.Debug, $"LCFM:{LCFM}绑定WIP:{OutWIP}失败:{recStr}");
            //            Result = false;
            //            return true;
            //        }
            //    }
            //    else
            //    {
            //        MyOwner.OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"通过料件查询WIP码失败:{LCG}");
            //        Result = false;
            //        return true;
            //    }

            //}
            return base.DoExcute(out errMsg);
        }
    }
}


