using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.SFC;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class PDCAELimit : MotionFunction, IPauseFunction, IHomeFunction
    {
        /// <summary>
        /// PDCA动作方式
        /// </summary>
        public enum PDCAType
        {
            [Description("开始")]
            Start,
            [Description("获取WIP")]
            GetWIP,
            [Description("图片拷贝")]
            CopyImage,
            [Description("数据发送")]
            SendData,
            [Description("所有动作")]
            Whole,
            [Description("结束")]
            End,
            [Description("CCTV文件拷贝")]
            CCTV,
            [Description("CSV拷贝")]
            CopyCSV,
            [Description("打包上传")]
            WholeContinous
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

        [NotEmpty]
        [Parameter("PDCA读取超时时间MS", 1, CN = "PDCA读取超时时间MS", CanRef = ParamRef.Ref, DefaultV = 5000)]
        public int TimeOutMS { get; set; }

        [NotEmpty]
        [Parameter("PDCA读取失败后是否主动断连", 1, CN = "PDCA读取失败后是否主动断连", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsAutoDisConnect { get; set; }

        /// <summary>
        /// 上传PDCA启用
        /// </summary>
        [NotEmpty]
        [Parameter("全手动输入AElimits", 17, CN = "手输AElimits", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsManual { get; set; }
        /// <summary>
        /// 动作类型
        /// </summary>
        [NotEmpty]
        [Parameter("动作类型", 2, CN = "动作类型", DefaultV = PDCAType.SendData)]
        public PDCAType PDCAMode { get; set; }

        [DependOn("PDCAMode", PDCAType.Whole, PDCAType.SendData, PDCAType.WholeContinous)]
        [Parameter("启用Display_SN上传项", 3, CN = "启用Display_SN", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsCGDisplaySN { get; set; }

        [DependOn("IsCGDisplaySN", true)]
        [Parameter("排线二维码", 4, CN = "Display_SN", CanRef = ParamRef.Ref)]
        public string CGDisplaySN { get; set; }

        /// <summary>
        /// CPK模式
        /// </summary>
        [Parameter("CPK模式", 5, CN = "CPK", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsCPKMode { get; set; }

        /// <summary>
        /// CPK模式
        /// </summary>
        [Parameter("GRR模式", 6, CN = "GRR", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsGRRMode { get; set; }



        /// <summary>
        /// SN
        /// </summary>
        [Parameter("SN", 7, CN = "SN", CanRef = ParamRef.Ref)]
        public string SN { get; set; }

        [Parameter("WIP长度", 8, CN = "WIP长度", DefaultV = 18)]
        public int WIP_Length { get; set; }

        [Parameter("用于存放虚拟码的文件，只有在CPK模式下才有效，使用txt存储,一行一个码", 9, CN = "虚拟码文件")]
        public LPath WIPFile { get; set; }

        /// <summary>
        /// CarrierSN
        /// </summary>
        [Parameter("CarrierSN", 10, CN = "CarrierSN", CanRef = ParamRef.Ref)]
        public string CarrierSN { get; set; }

        /// <summary>
        /// CycleTime
        /// </summary>
        [Parameter("CycleTime", 11, CN = "CycleTime", CanRef = ParamRef.Ref, DefaultV = 4.5)]
        public double CycleTime { get; set; }

        // <summary>
        // 工位号
        // </summary>
        [Parameter("工位号", 12, CN = "工位号", CanRef = ParamRef.Ref, DefaultV = 1)]
        public int WorkId { get; set; }

        /// <summary>
        /// 源图片路径
        /// </summary>
        [NotEmpty]
        [Parameter("图片文件夹名称，以反斜杠进行分隔(/)", 13, CN = "源图片路径", CanRef = ParamRef.Ref)]
        public string SourceImagePath { get; set; }
        /// <summary>
        /// 目标图片路径
        /// </summary>
        [NotEmpty]
        [Parameter("图片文件夹名称，以反斜杠进行分隔(/)", 14, CN = "目标图片路径")]
        public LStringEx DesImagePath { get; set; }
        /// <summary>
        /// 图片名称规则
        /// </summary>
        [NotEmpty]
        [Parameter("图片名称规则", 15, CN = "路径规则")]
        public string IsImageRol { get; set; }

        /// <summary>
        /// 是否3ANG
        /// </summary>
        [Parameter("是否3ANG", 16, CN = "3ANG", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool Is3ANG { get; set; }

        ///// <summary>
        ///// 是否图片上传，产线上10.08版本没有这个参数，因此暂时注释掉。
        ///// </summary>
        //[Parameter("是否软体上传图片", 16, CN = "图片上传", CanRef = ParamRef.Ref, DefaultV = true)]
        //public bool IsImageUpdata { get; set; }

        /// <summary>
        /// 自动运行数据
        /// </summary>
        [NotEmpty]
        [Parameter("每条数据通过分隔符;间隔", 17, CN = "过程数据")]
        public LStringEx ProdData { get; set; }

        /// <summary>
        /// 是否包含下划线
        /// </summary>
        [Parameter("是否包含下划线", 18, CN = "是否包含下划线", DefaultV = false)]
        public bool IsContainLine { get; set; }

        /// <summary>
        /// 是否启用载具查询WIP
        /// </summary>
        [Parameter("启用载具查询WIP", 19, CN = "启用载具查询WIP", DefaultV = false)]
        public bool IsEnableCarrier { get; set; }

        /// <summary>
        /// 源CSV路径
        /// </summary>
        [DependOn("PDCAMode", PDCAType.CopyCSV)]
        [Parameter("源CSV文件夹路径", 24, CN = "源CSV路径", CanRef = ParamRef.Ref)]
        public string SourceCSVPath { get; set; }

        /// <summary>
        /// 目标CSV路径
        /// </summary>
        [DependOn("PDCAMode", PDCAType.CopyCSV)]
        [Parameter("目标CSV文件夹路径", 25, CN = "目标CSV路径")]
        public LStringEx DesCSVPath { get; set; }

        /// <summary>
        /// CSV文件名称规则
        /// </summary>
        [DependOn("PDCAMode", PDCAType.CopyCSV)]
        [Parameter("CSV名称规则", 26, CN = "CSV名称规则", CanRef = ParamRef.Ref)]
        public string CSVRol { get; set; }

        /// <summary>
        /// 是否上传csv文件
        /// </summary>
        [DependOn("PDCAMode", PDCAType.CopyCSV)]
        [Parameter("是否上传csv文件", 18, CN = "是否上传csv文件", DefaultV = false)]
        public bool IsUploadCsv { get; set; }

        /// <summary>
        /// 是否上传力控图片
        /// </summary>
        [DependOn("PDCAMode", PDCAType.CopyCSV)]
        [Parameter("是否上传力控图片", 18, CN = "是否上传力控图片", DefaultV = false)]
        public bool IsUploadDHPic { get; set; }

        [DependOn("PDCAMode", PDCAType.Whole)]
        [Parameter("全部拷贝", 19, CN = "全部拷贝", DefaultV = false)]
        public bool FullCopy { get; set; }


        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("PDCA上传结果,1成功；2图片拷贝失败；3数据上传失败;4图片和数据上传都失败;5WIP获取失败", 23, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int iResult { get; set; }

        /// <summary>
        /// 输出WIP
        /// </summary>
        [Parameter("WIP", 25, CN = "WIP", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutWIP { get; set; }

        static string testSeriesID;
        public Type Type { get; set; }
        public object _val;
        // PDCA登录账户
        private readonly string pdcaAccount = "gdlocal";
        // PDCA 登录密码
        private readonly string pdcaPassword = "gdlocal";

        /// <summary>
        /// PDCA上传重新使用新的通信服务
        /// </summary>
        private VCommuncation vCommPdca = null;
        private SFCHelper _sfcHelper = null;

        /// <summary>
        /// 存放Wip码的对象
        /// </summary>
        private Queue<string> wipCodes = new Queue<string>();

        public PDCAELimit()
        {
            this.Tips = "PDCA流程处理";
            this.Icon = "\xe6c9";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            switch (PDCAMode)
            {
                case PDCAType.CCTV:
                    CopyFolder();
                    break;

            }
            if (PDCAMode == PDCAType.CCTV)
            {
                return true;
            }
            // 空跑模式或者没启用直接返回
            // 0 Wip获取失败 1成功；2图片拷贝失败；3数据上传失败; 4图片和数据上传都失败
            if (IsEmptyMode || !IsEnable)
            {
                iResult = 1;
                OutWIP = DateTime.Now.ToString("yyyyMMddHHmmss");
                return true;
            }
            if (string.IsNullOrEmpty(testSeriesID) || testSeriesID == "0")
                testSeriesID = DateTime.Now.ToString("yyyyMMddHHmmss");

            #region 0.定义参数及初始化
            //建议：走批量发送模式，提升CT
            //0.定义参数
            //发送指令
            string sendStr;

            // 复制图片返回值
            int iCopyImgRes = 0;
            //发送数据返回值
            int iSendPDCARes = 0;
            //输出结果清零
            iResult = 0;

            #endregion

            OutWIP = "";

            if (WIPFile == null && IsCPKMode)
            {
                OnAlarm(AlarmType.WarningTip, $"AELimit模块没有配置CPK_WIP文件路径，请配置！");
                return false;
            }

            if (!File.Exists(SFCHelper.CPKWIPFilePath) && IsCPKMode)
            {
                OnAlarm(AlarmType.WarningTip, $"CPK_WIP文件不存在，请拷贝放置当前任务Config目录下！");
                return false;
            }

            if (WIPFile.Path != SFCHelper.CPKWIPFilePath)
            {
                WIPFile.Path = SFCHelper.CPKWIPFilePath;
                LoadWipFile(WIPFile.Path);
            }

            //2.连接通讯设备
            GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);
            vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            try
            {
                vCommPdca.Open();
            }
            catch (Exception)
            {
                // 连接失败的通讯支持重新赋值
                vCommPdca = null;
                _sfcHelper = null;
                throw;
            }



            // 没有在缓存中获取到WIP，则通过网络查询
            if (_sfcHelper == null)
            {
                _sfcHelper = new SFCHelper(vCommPdca);
            }

            // CPK模式
            if (IsCPKMode || IsGRRMode)
            {
                if (wipCodes.Count == 0)
                {
                    OnAlarm(AlarmType.WarningTip, $"目前处于CPK模式,请先加载虚拟码文件!");

                    return true;
                }
                else
                {
                    OutWIP = wipCodes.Dequeue();
                }

                MyOwner.OnLog(LogType.Warning, $"CPK模式,自动生成虚拟Wip:{OutWIP}");
            }
            else
            {
                if (IsManual)
                {
                    sendStr = GetSendData(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId, true);
                }
                else
                {
                    sendStr = GetSendDataNew(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId, true);
                }


                if (sendStr.ToLower().Contains("force@-1@"))
                {
                    MyOwner.OnAlarm(AlarmType.InfoTip, $"压力值读到-1，不再上传PDCA!!!");
                    iResult = 5;
                    return true;
                }
                else
                {


                    if (IsEnableCarrier)
                    {
                        OutWIP = _sfcHelper.QueryWipByCarrierSn(CarrierSN, out errMsg);
                    }
                    else
                    {
                        OutWIP = _sfcHelper.GetWip(SN, WIP_Length, out errMsg);
                    }

                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, $"WIP已经分配，但是Wip 码获取失败！！！");
                        iResult = 5;
                        return true;
                    }
                }
            }
            if (string.IsNullOrEmpty(OutWIP))
            {
                MyOwner.OnAlarm(AlarmType.InfoTip, $"【PDCAElimits】查询WIP为空！！！");
                return false;
            }

            switch (PDCAMode)
            {
                //1.发送Start命令
                case PDCAType.Start:
                    sendStr = $"{OutWIP}@start\n";
                    _sfcHelper.SendPDCAData(sendStr, "PDCA Start", out errMsg, TimeOutMS, IsAutoDisConnect);
                    iResult = string.IsNullOrEmpty(errMsg) ? 1 : 3;
                    break;

                //3.发送所有参数
                case PDCAType.SendData:
                    if (IsManual)
                    {
                        sendStr = GetSendData(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId, true);
                    }
                    else
                    {
                        sendStr = GetSendDataNew(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId, true);
                    }
                    _sfcHelper.SendPDCAData(sendStr, "PDCA SendData", out errMsg, TimeOutMS, IsAutoDisConnect);
                    if (string.IsNullOrEmpty(errMsg))
                    {
                        iResult = 1;
                    }
                    else
                    {
                        iResult = 3;
                        //将sendStr和SN保存到本地,输入路径，按照SN命名，保存为json格式
                        SaveErrorData(SN, sendStr);

                    }
                    break;

                //4.拷贝图片
                case PDCAType.CopyImage:

                    //判断是否能连接上
                    // 判断是否能连接上，能够连接上的情况下，再去复制
                    if (IsConnectMacMini)
                    {
                        //CopyFolder(OutWIP, ref iCopyImgRes);
                        var bRes = CopyFolderAsync(OutWIP).GetAwaiter().GetResult();//只拷贝图片的情况下同步调用
                        iResult = bRes ? 1 : 2;
                        //iResult = iCopyImgRes == 1 ? 1 : 2;
                    }
                    else
                        iResult = 2;
                    break;

                case PDCAType.CopyCSV:
                    if (IsConnectMacMini)
                    {
                        // 异步拷贝CSV文件
                        var bRes = CopyCSVAsync(OutWIP).GetAwaiter().GetResult();
                        iResult = bRes ? 1 : 2;
                    }
                    else
                        iResult = 2;
                    break;

                case PDCAType.Whole:

                    //1.第一步 发送Start
                    if (IsCPKMode || IsGRRMode)
                    {
                        sendStr = $"{OutWIP}@start@audit\n";
                    }
                    else
                    {
                        sendStr = $"{OutWIP}@start\n";
                    }

                    // 发生Start 命令
                    _sfcHelper.SendPDCAData(sendStr, "PDCA Start", out errMsg, TimeOutMS, IsAutoDisConnect);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, $"PDCA Start 失败,{errMsg}");
                        iResult = 4;
                        return true;
                    }

                    // 2.拷贝图片
                    // CPK模式下/GRR模式下，不上传图片
                    if (!IsCPKMode && !IsGRRMode)
                    {
                        // 判断是否能连接上，能够连接上的情况下，再去复制
                        if (IsConnectMacMini)
                        {
                            //CopyFolder(OutWIP, ref iCopyImgRes);
                            CopyFolderAsync(OutWIP.ToString());//异步的话，传递一个拷贝字符串过去
                        }
                    }
                    else
                    {
                        iCopyImgRes = 1;
                    }

                    // 3.发送Submit
                    if (IsManual)
                    {
                        sendStr = GetSendData(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId, false);
                    }
                    else
                    {
                        sendStr = GetSendDataNew(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId, false);
                    }
                    _sfcHelper.SendPDCAData(sendStr, "PDCA上传数据", out errMsg, TimeOutMS, IsAutoDisConnect);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        OnAlarm(AlarmType.InfoTip, $"PDCA 数据上传失败:{errMsg}");
                        iSendPDCARes = 0;
                        SaveErrorData(SN, sendStr);
                        iResult = 5;
                        return true;
                    }
                    else
                    {
                        iSendPDCARes = 1;
                    }

                    break;

                case PDCAType.WholeContinous:

                    // 1.异步拷贝图片（非CPK/GRR模式下）
                    if (!IsCPKMode && !IsGRRMode)
                    {
                        if (IsConnectMacMini)
                        {
                            CopyFolderAsync(OutWIP.ToString());
                        }
                        // 图片拷贝为异步操作，不阻塞数据上传
                        iCopyImgRes = 1;
                    }
                    else
                    {
                        iCopyImgRes = 1;
                    }

                    // 2.拼接完整批量消息（start + attr + data + submit 一次性发送）
                    if (IsManual)
                        sendStr = GetSendDataContinuous(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId);
                    else
                        sendStr = GetSendDataContinuousNew(OutWIP, ProdData, IsCPKMode, IsGRRMode, WorkId);

                    // 3.检测压力值异常
                    if (sendStr.ToLower().Contains("force@-1@"))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, $"压力值读到-1，不再上传PDCA!!!");
                        iResult = 5;
                        return true;
                    }

                    // 4.一次性发送
                    _sfcHelper.SendPDCAData(sendStr, "PDCA打包上传", out errMsg, TimeOutMS, IsAutoDisConnect);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        OnAlarm(AlarmType.InfoTip, $"PDCA打包上传失败:{errMsg}");
                        iSendPDCARes = 0;
                        SaveErrorData(SN, sendStr);
                        iResult = 3;
                        return true;
                    }
                    else
                    {
                        iSendPDCARes = 1;
                    }

                    break;
            }

            if (PDCAMode == PDCAType.Whole || PDCAMode == PDCAType.WholeContinous)
            {
                if (iCopyImgRes == 1 && iSendPDCARes == 1)
                    iResult = 1;
                else if (iCopyImgRes != 1 && iSendPDCARes == 1)
                    iResult = 2;
                else if (iCopyImgRes == 1 && iSendPDCARes != 1)
                    iResult = 3;
                else if (iCopyImgRes != 1 && iSendPDCARes != 1)
                    iResult = 4;
            }
            else
            {
                if (iResult != 1)
                {
                    iResult = 4;
                }
            }


            return true;
        }

        private void SaveErrorData(string sn, string sendStr)
        {
            try
            {
                string baseDir = Path.Combine(MyOwner.DeviceEngine.RecipeSlnPath, "PDACError");
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }
                string fileName = $"{sn}.json";
                string filePath = Path.Combine(baseDir, fileName);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(sendStr, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);
                MyOwner.OnLog(LogType.Warning, $"PDCAELimit失败已备份SendData到本地: {filePath}");
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Warning, $"PDCAELimit本地保存SendData异常: SN={sn}, SendStr={sendStr}, 异常: {ex}");
            }
        }

        //拼接发送数据
        private string GetSendData(string wip, LStringEx pDataEx, bool IsCPK, bool IsGRR, int WorkId, bool IsAll = false)
        {
            string QPLNum = MyOwner.ConfigManager.GetWebConfig("UniteCode");

            //批量传递
            string sendContent;
            string data;
            string mode = "0";
            string priority = "0";
            string straudit = "@audit";
            string strData = "";
            string prodDatas = pDataEx.GetString(MyOwner).Replace('\r', ';').Replace('\n', ';');
            string cpkDatas = "";
            string aelmts = "";
            prodDatas = prodDatas.Replace("QPL@1@@", $"QPL@{QPLNum}@@");

            StringBuilder sb = new StringBuilder();
            var datas = prodDatas.Split(';');
            foreach (var item in datas)
            {
                if (string.IsNullOrEmpty(item.Trim())) continue;
                sb.Append($"{wip}@{item}\n");
            }

            data = sb.ToString();

            if (IsCPKMode)
            {
                straudit = "@audit";
            }
            else if (IsGRRMode)
            {
                straudit = "@audit";
            }
            else
            {
                straudit = "";
            }
            if (IsCPKMode || IsGRRMode)
            {
                if (IsAll)
                {
                    sendContent =
                    $"_{{\n" +
                    $"{wip}@start{straudit}\n" +
                    $"{data}" +
                    $"{wip}@pdata@TestSeriesID@{testSeriesID}\n" +
                    $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                    $"}}\n";
                }
                else
                {
                    sendContent =
                      $"_{{\n" +
                      $"{wip}@dut_pos@{QPLNum}@{WorkId}\n" +
                      $"{data}" +
                      $"{wip}@pdata@TestSeriesID@{testSeriesID}\n" +
                      $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                      $"}}\n";
                }
            }
            else
            {
                if (IsAll)
                {
                    sendContent =
                       $"_{{\n" +
                       $"{wip}@start{straudit}\n" +
                       $"{data}" +
                       $"{wip}@pdata@TestSeriesID@{0}\n" +
                       $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                       $"}}\n";
                }
                else
                {
                    sendContent =
                       $"_{{\n" +
                       $"{wip}@dut_pos@{QPLNum}@{WorkId}\n" +
                       $"{data}" +
                       $"{wip}@pdata@TestSeriesID@{0}\n" +
                       $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                       $"}}\n";
                }
            }
            return sendContent;
        }
        private string GetSendDataNew(string wip, LStringEx pDataEx, bool IsCPK, bool IsGRR, int WorkId, bool IsAll = false)
        {
            string QPLNum = MyOwner.ConfigManager.GetWebConfig("UniteCode");

            //批量传递
            string sendContent;
            string data;
            string mode = "0";
            string priority = "0";
            string straudit = "@audit";
            string strData = "";
            string prodDatas = pDataEx.GetString(MyOwner).Replace('\r', ';').Replace('\n', ';');
            string cpkDatas = "";
            string aelmts = "";
            prodDatas = prodDatas.Replace("QPL@1@@", $"QPL@{QPLNum}@@");

            StringBuilder sb = new StringBuilder();
            var datas = prodDatas.Split(';');
            foreach (var item in datas)
            {
                if (string.IsNullOrEmpty(item.Trim())) continue;
                sb.Append($"{wip}@pdata@{item}\n");
            }

            data = sb.ToString();

            if (IsCPKMode)
            {

                mode = "1";
                priority = "-2";
                straudit = "@audit";
            }
            else if (IsGRRMode)
            {
                mode = "2";
                priority = "-2";
                straudit = "@audit";
            }
            else
            {
                mode = "0";
                priority = "0";
                straudit = "";
                testSeriesID = "0";
            }

            if (!IsContainLine)
            {
                if (IsCGDisplaySN)
                {
                    strData =
                    $"{wip}@attr@Machine SN@{MyOwner.ConfigManager.GetWebConfig("MachineSN")}\n" +
                    $"{wip}@attr@CG SN@{SN}\n" +
                    $"{wip}@attr@Carrier SN@{CarrierSN}\n" +
                    $"{wip}@attr@CG Display_SN@{CGDisplaySN}\n";
                }
                else
                {
                    strData =
                    $"{wip}@attr@Machine SN@{MyOwner.ConfigManager.GetWebConfig("MachineSN")}\n" +
                    $"{wip}@attr@CG SN@{SN}\n" +
                    $"{wip}@attr@Carrier SN@{CarrierSN}\n";
                }
            }
            else
            {
                if (IsCGDisplaySN)
                {
                    strData =
                    $"{wip}@attr@Machine_SN@{MyOwner.ConfigManager.GetWebConfig("MachineSN")}\n" +
                    $"{wip}@attr@CG_SN@{SN}\n" +
                    $"{wip}@attr@Carrier_SN@{CarrierSN}\n" +
                    $"{wip}@attr@CG Display_SN@{CGDisplaySN}\n";
                }
                else
                {
                    strData =
                    $"{wip}@attr@Machine_SN@{MyOwner.ConfigManager.GetWebConfig("MachineSN")}\n" +
                    $"{wip}@attr@CG_SN@{SN}\n" +
                    $"{wip}@attr@Carrier_SN@{CarrierSN}\n";
                }
            }
            if (IsCPKMode || IsGRRMode)
            {
                if (IsAll)
                {
                    sendContent =
                    $"_{{\n" +
                    $"{wip}@start{straudit}\n" +
                    $"{strData}" +
                    $"{data}" +
                    $"{wip}@pdata@Mode@{mode}\n" +
                    $"{wip}@pdata@Operator_ID@1\n" +
                    $"{wip}@pdata@Priority@{priority}\n" +
                    $"{wip}@pdata@TestSeriesID@{testSeriesID}\n" +
                    $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                    $"}}\n";
                }
                else
                {
                    sendContent =
                      $"_{{\n" +
                      //$"{WIP}@start{straudit}\n" +
                      $"{wip}@dut_pos@{QPLNum}@{WorkId}\n" +
                      $"{strData}" +
                      $"{data}" +
                      $"{wip}@pdata@Mode@{mode}\n" +
                      $"{wip}@pdata@Operator_ID@1\n" +
                      $"{wip}@pdata@Priority@{priority}\n" +
                      $"{wip}@pdata@TestSeriesID@{testSeriesID}\n" +
                      $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                      $"}}\n";
                }
            }
            else
            {
                if (IsAll)
                {
                    sendContent =
                       $"_{{\n" +
                       $"{wip}@start{straudit}\n" +
                       $"{strData}" +
                       $"{data}" +
                       $"{wip}@pdata@Mode@{mode}\n" +
                       $"{wip}@pdata@Operator_ID@1\n" +
                       $"{wip}@pdata@Priority@{priority}\n" +
                       $"{wip}@pdata@TestSeriesID@{0}\n" +
                       $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                       $"}}\n";
                }
                else
                {
                    sendContent =
                       $"_{{\n" +
                       //$"{WIP}@start{straudit}\n" +
                       $"{wip}@dut_pos@{QPLNum}@{WorkId}\n" +
                       $"{strData}" +
                       $"{data}" +
                       $"{wip}@pdata@Mode@{mode}\n" +
                       $"{wip}@pdata@Operator_ID@1\n" +
                       $"{wip}@pdata@Priority@{priority}\n" +
                       $"{wip}@pdata@TestSeriesID@{0}\n" +
                       $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                       $"}}\n";
                }
            }

            return sendContent;
        }

        /// <summary>
        /// 连续一次性发送：将 start + attr + data + submit 拼成一条 _{} 批量消息
        /// </summary>
        private string GetSendDataContinuous(string wip, LStringEx pDataEx, bool IsCPK, bool IsGRR, int WorkId)
        {
            string QPLNum = MyOwner.ConfigManager.GetWebConfig("UniteCode");

            string mode = "0";
            string priority = "0";
            string straudit = "";

            // 拼接过程数据
            string prodDatas = pDataEx.GetString(MyOwner).Replace('\r', ';').Replace('\n', ';');
            prodDatas = prodDatas.Replace("QPL@1@@", $"QPL@{QPLNum}@@");

            StringBuilder sbData = new StringBuilder();
            var datas = prodDatas.Split(';');
            foreach (var item in datas)
            {
                if (string.IsNullOrEmpty(item.Trim())) continue;
                sbData.Append($"{wip}@{item}\n");
            }

            if (IsCPKMode)
            {
                mode = "1";
                priority = "-2";
                straudit = "@audit";
            }
            else if (IsGRRMode)
            {
                mode = "2";
                priority = "-2";
                straudit = "@audit";
            }
            else
            {
                mode = "0";
                priority = "0";
                straudit = "";
                testSeriesID = "0";
            }

            // 拼接 attr
            StringBuilder sbAttr = new StringBuilder();
            string machineSN = MyOwner.ConfigManager.GetWebConfig("MachineSN");
            if (!IsContainLine)
            {
                //sbAttr.Append($"{wip}@attr@Machine SN@{machineSN}\n");
                //sbAttr.Append($"{wip}@attr@CG SN@{SN}\n");
                //sbAttr.Append($"{wip}@attr@Carrier SN@{CarrierSN}\n");
                if (IsCGDisplaySN)
                    sbAttr.Append($"{wip}@attr@CG Display_SN@{CGDisplaySN}\n");
            }
            else
            {
                //sbAttr.Append($"{wip}@attr@Machine_SN@{machineSN}\n");
                //sbAttr.Append($"{wip}@attr@CG_SN@{SN}\n");
                //sbAttr.Append($"{wip}@attr@Carrier_SN@{CarrierSN}\n");
                if (IsCGDisplaySN)
                    sbAttr.Append($"{wip}@attr@CG Display_SN@{CGDisplaySN}\n");
            }

            string sendContent =
                $"_{{\n" +
                $"{wip}@start{straudit}\n" +
                sbAttr.ToString() +
                sbData.ToString() +
                //$"{wip}@pdata@Mode@{mode}\n" +
                //$"{wip}@pdata@Operator_ID@1\n" +
                //$"{wip}@pdata@Priority@{priority}\n" +
                $"{wip}@pdata@TestSeriesID@{(IsCPKMode || IsGRRMode ? testSeriesID : "0")}\n" +
                $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                $"}}\n";

            return sendContent;
        }

        private string GetSendDataContinuousNew(string wip, LStringEx pDataEx, bool IsCPK, bool IsGRR, int WorkId)
        {
            string QPLNum = MyOwner.ConfigManager.GetWebConfig("UniteCode");

            string mode = "0";
            string priority = "0";
            string straudit = "";

            // 拼接过程数据
            string prodDatas = pDataEx.GetString(MyOwner).Replace('\r', ';').Replace('\n', ';');
            prodDatas = prodDatas.Replace("QPL@1@@", $"QPL@{QPLNum}@@");

            StringBuilder sbData = new StringBuilder();
            var datas = prodDatas.Split(';');
            foreach (var item in datas)
            {
                if (string.IsNullOrEmpty(item.Trim())) continue;
                sbData.Append($"{wip}@pdata@{item}\n");
            }

            if (IsCPKMode)
            {
                mode = "1";
                priority = "-2";
                straudit = "@audit";
            }
            else if (IsGRRMode)
            {
                mode = "2";
                priority = "-2";
                straudit = "@audit";
            }
            else
            {
                mode = "0";
                priority = "0";
                straudit = "";
                testSeriesID = "0";
            }

            // 拼接 attr
            StringBuilder sbAttr = new StringBuilder();
            string machineSN = MyOwner.ConfigManager.GetWebConfig("MachineSN");
            if (!IsContainLine)
            {
                sbAttr.Append($"{wip}@attr@Machine SN@{machineSN}\n");
                sbAttr.Append($"{wip}@attr@CG SN@{SN}\n");
                sbAttr.Append($"{wip}@attr@Carrier SN@{CarrierSN}\n");
                if (IsCGDisplaySN)
                    sbAttr.Append($"{wip}@attr@CG Display_SN@{CGDisplaySN}\n");
            }
            else
            {
                sbAttr.Append($"{wip}@attr@Machine_SN@{machineSN}\n");
                sbAttr.Append($"{wip}@attr@CG_SN@{SN}\n");
                sbAttr.Append($"{wip}@attr@Carrier_SN@{CarrierSN}\n");
                if (IsCGDisplaySN)
                    sbAttr.Append($"{wip}@attr@CG Display_SN@{CGDisplaySN}\n");
            }

            string sendContent =
                $"_{{\n" +
                $"{wip}@start{straudit}\n" +
                sbAttr.ToString() +
                sbData.ToString() +
                $"{wip}@pdata@Mode@{mode}\n" +
                $"{wip}@pdata@Operator_ID@1\n" +
                $"{wip}@pdata@Priority@{priority}\n" +
                $"{wip}@pdata@TestSeriesID@{(IsCPKMode || IsGRRMode ? testSeriesID : "0")}\n" +
                $"{wip}@submit@{MyOwner.ConfigManager.GetWebConfig("SoftVersion")}\n" +
                $"}}\n";

            return sendContent;
        }

        // 定义静态信号量（全局串行队列）
        private static readonly SemaphoreSlim _copySemaphore = new SemaphoreSlim(1, 1);

        private async Task<bool> CopyFolderAsync(string wip)
        {
            await _copySemaphore.WaitAsync();
            try
            {
                return await CopyFolderAsyncInner(wip);
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Warning, $"PDCAELimit模块:{MyOwner.Alias} 异步拷贝图片异常:{ex.ToString()}");
                return false;
            }
            finally
            {
                _copySemaphore.Release();
            }
        }
        private async Task<bool> CopyCSVAsync(string wip)
        {
            await _copySemaphore.WaitAsync();
            try
            {
                return await CopyCSVAsyncInner(wip);
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Warning, $"PDCAELimit模块:{MyOwner.Alias} 异步拷贝CSV异常:{ex.ToString()}");
                return false;
            }
            finally
            {
                _copySemaphore.Release();
            }
        }

        private async Task<bool> CopyCSVAsyncInner(string wip)
        {
            int maxRetry = 3;
            int retry = 0;
            Exception lastEx = null;
            string srcCSVFolder = "";
            string dstCSVFolder = "";

            while (retry < maxRetry)
            {
                try
                {
                    srcCSVFolder = SourceCSVPath.Trim();
                    dstCSVFolder = Path.Combine(DesImagePath.GetString(Owner), wip).Trim();

                    MyOwner.OnLog(LogType.Info, $"PDCAELimit模块:{MyOwner.Alias} 源CSV路径:{srcCSVFolder}  目标路径{dstCSVFolder}");

                    if (!Directory.Exists(srcCSVFolder))
                    {
                        MyOwner.OnLog(LogType.Warning, $"PDCAELimit模块:{MyOwner.Alias} 源CSV路径:{srcCSVFolder}不存在");
                        return false;
                    }
                    if (!Directory.Exists(dstCSVFolder))
                    {
                        Directory.CreateDirectory(dstCSVFolder);
                    }

                    if (IsUploadCsv)
                    {
                        string[] csvList = Directory.GetFiles(srcCSVFolder, "*.csv");
                        foreach (var fPath in csvList)
                        {
                            var fName = Path.GetFileName(fPath);
                            // 文件名规则过滤：未配置规则则拷贝所有CSV，配置了则只拷贝匹配的
                            if (string.IsNullOrEmpty(CSVRol) || fName.IndexOf(CSVRol, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                File.Copy(fPath, Path.Combine(dstCSVFolder, fName), true);
                                MyOwner.OnLog(LogType.Warning, $"PDCAELimit CSV复制OK;{Path.Combine(srcCSVFolder, fName)}->{Path.Combine(dstCSVFolder, fName)}\r\n");
                            }
                        }
                    }

                    if (IsUploadDHPic)
                    {
                        string[] DHpicList = Directory.GetFiles(srcCSVFolder, "*.jpg")
                                                   .Concat(Directory.GetFiles(srcCSVFolder, "*.png"))
                                                   .Concat(Directory.GetFiles(srcCSVFolder, "*.jpeg"))
                                                   .ToArray();
                        foreach (var fPath in DHpicList)
                        {
                            var fName = Path.GetFileName(fPath);
                            // 文件名规则过滤：未配置规则则拷贝所有CSV，配置了则只拷贝匹配的
                            if (string.IsNullOrEmpty(CSVRol) || fName.IndexOf(CSVRol, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                File.Copy(fPath, Path.Combine(dstCSVFolder, fName), true);
                                MyOwner.OnLog(LogType.Warning, $"PDCAELimit CSV复制OK;{Path.Combine(srcCSVFolder, fName)}->{Path.Combine(dstCSVFolder, fName)}\r\n");
                            }
                        }
                    }


                    return true;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    MyOwner.OnLog(LogType.Warning, $"PDCAELimit CSV复制失败;第{retry + 1}次尝试;{srcCSVFolder}->{dstCSVFolder}\r\n{ex}");
                    await Task.Delay(500);
                }
                retry++;
            }

            // 三次失败，保存到本地日志
            try
            {
                string baseLogPath = Path.Combine($"E:\\DefaultStation", "LUSTER", "PDCAELimit_CSVCopyFailLog");
                string failLog = Path.Combine(baseLogPath, DateTime.Now.ToString("yyyy-MM-dd"), ".log");
                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WIP:{wip} 源:{srcCSVFolder} 目标:{dstCSVFolder} 错误:{lastEx}\r\n";
                File.AppendAllText(failLog, logContent, Encoding.UTF8);
            }
            catch { /* 忽略本地日志写入异常 */ }

            return false;
        }

        private async Task<bool> CopyFolderAsyncInner(string wip)
        {
            int maxRetry = 3;
            int retry = 0;
            Exception lastEx = null;
            string srcImgFolder = "";
            string srcImgFolderEx = "";
            string dstImgFolder = "";

            while (retry < maxRetry)
            {
                try
                {
                    srcImgFolder = SourceImagePath.Trim();
                    dstImgFolder = Path.Combine(DesImagePath.GetString(Owner), wip).Trim();

                    if (!Directory.Exists(srcImgFolder))
                    {
                        MyOwner.OnLog(LogType.Warning, $"PDCAELimit模块:{MyOwner.Alias} 原文件路径:{srcImgFolder}不存在");
                        return false;
                    }
                    if (!Directory.Exists(dstImgFolder))
                    {
                        Directory.CreateDirectory(dstImgFolder);
                    }
                    if (FullCopy)
                    {
                        string[] picList = Directory.GetFiles(srcImgFolder);
                        foreach (var fPath in picList)
                        {
                            var fName = Path.GetFileName(fPath);
                            File.Copy(fPath, Path.Combine(dstImgFolder, fName), true);
                        }
                    }
                    else
                    {
                        string[] picList = Directory.GetFiles(srcImgFolder, "*.jpg")
                                .Concat(Directory.GetFiles(srcImgFolder, "*.png"))
                                .ToArray();
                        foreach (var fPath in picList)
                        {
                            //string fName = fPath.Substring(srcImgFolder.Length + 1);
                            //if (fName.Contains(IsImageRol))
                            //{
                            //    File.Copy(Path.Combine(srcImgFolder, fName), Path.Combine(dstImgFolder, fName), true);
                            var fName = Path.GetFileName(fPath);
                            if (!string.IsNullOrEmpty(IsImageRol) && fName.IndexOf(IsImageRol, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                File.Copy(fPath, Path.Combine(dstImgFolder, fName), true);
                                MyOwner.OnLog(LogType.Warning, $"PDCAELimit文件复制OK;{Path.Combine(srcImgFolder, fName)}->{Path.Combine(dstImgFolder, fName)}\r\n");
                            }
                        }

                        //遍历OK和NG文件夹
                        if (Is3ANG)
                        {
                            if (srcImgFolder.Contains("\\OK\\"))
                            {
                                srcImgFolderEx = srcImgFolder.Replace("\\OK\\", "\\NG\\");
                            }
                            if (srcImgFolder.Contains("\\NG\\"))
                            {
                                srcImgFolderEx = srcImgFolder.Replace("\\NG\\", "\\OK\\");
                            }
                            if (!string.IsNullOrEmpty(srcImgFolderEx) && Directory.Exists(srcImgFolderEx))
                            {
                                picList = Directory.GetFiles(srcImgFolderEx, "*.jpg");
                                foreach (var fPath in picList)
                                {
                                    string fName = fPath.Substring(srcImgFolderEx.Length + 1);
                                    if (fName.Contains(IsImageRol))
                                    {
                                        File.Copy(Path.Combine(srcImgFolderEx, fName), Path.Combine(dstImgFolder, fName), true);
                                        MyOwner.OnLog(LogType.Warning, $"PDCAELimit文件3ANG复制OK;{Path.Combine(srcImgFolderEx, fName)}->{Path.Combine(dstImgFolder, fName)}\r\n");
                                    }
                                }
                            }
                        }
                    }
                    // 成功
                    return true;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    MyOwner.OnLog(LogType.Warning, $"PDCAELimit文件夹复制失败;第{retry + 1}次尝试;{srcImgFolder}->{dstImgFolder}\r\n{ex}");
                    await Task.Delay(500); // 可选：重试间隔
                }
                retry++;
            }

            // 三次失败，保存到本地日志
            try
            {
                string baseLogPath = Path.Combine($"E:\\DefaultStation", "LUSTER", "PDCAELimit_CopyFailLog");
                string failLog = Path.Combine(baseLogPath, DateTime.Now.ToString("yyyy-MM-dd"), ".log");
                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] WIP:{wip} 源:{srcImgFolder} 目标:{dstImgFolder} 错误:{lastEx}\r\n";
                File.AppendAllText(failLog, logContent, Encoding.UTF8);
            }
            catch { /* 忽略本地日志写入异常 */ }

            return false;
        }

        private void CopyFolder()
        {

            try
            {


                // 获取文件名（包含扩展名）
                string fileNameWithExtension = Path.GetFileName(SourceImagePath);

                // 提取编号部分：假设文件名格式为“通道X_XXXXXX.mp4”
                string[] parts = fileNameWithExtension.Split('_');
                if (parts.Length < 2)
                {
                    throw new ArgumentException("文件名格式不正确，无法提取编号。");
                }
                string folderName = Path.GetFileNameWithoutExtension(parts[1]);

                // 目标文件夹路径
                string targetFolderPath = Path.Combine(DesImagePath.GetString(Owner), folderName).Trim();

                // 确保目标文件夹存在
                if (!Directory.Exists(targetFolderPath))
                {
                    Directory.CreateDirectory(targetFolderPath);
                }

                // 目标文件完整路径
                string targetFilePath = Path.Combine(targetFolderPath, fileNameWithExtension);

                // 复制文件
                File.Copy(SourceImagePath, targetFilePath, true); // 覆盖已存在的文件
                MyOwner.OnLog(LogType.Info, $"文件复制成功{SourceImagePath}");
                File.Delete(SourceImagePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误：{ex.Message}");
            }
        }
        private void CopyFolder(string wip, ref int copySuccess)
        {
            //if (!IsImageUpdata)
            //    return;
            copySuccess = 0;
            try
            {

                string srcImgFolder = "";
                string srcImgFolderEx = "";
                string dstImgFolder = "";
                //Task.Run(() =>
                //{
                try
                {
                    srcImgFolder = SourceImagePath.Trim();
                    dstImgFolder = Path.Combine(DesImagePath.GetString(Owner), wip).Trim();
                    if (!Directory.Exists(srcImgFolder))
                    {
                        MyOwner.OnLog(LogType.Warning, $"PDCAELimit模块:{MyOwner.Alias} 原文件路径:{srcImgFolder}不存在");
                        return;
                    }
                    //if (!Directory.Exists(dstImgFolder.Split('/')[0]))
                    //{
                    //    MyOwner.OnLog(LogType.Warning, $"模块:{MyOwner.Alias} 目标文件路径:{dstImgFolder}不存在");
                    //    return;
                    //}
                    if (!Directory.Exists(dstImgFolder))
                    {
                        Directory.CreateDirectory(dstImgFolder);
                    }

                    string[] picList = Directory.GetFiles(srcImgFolder, "*.jpg");
                    foreach (var fPath in picList)
                    {
                        string fName = fPath.Substring(srcImgFolder.Length + 1);
                        if (fName.Contains(IsImageRol))
                        {
                            File.Copy(Path.Combine(srcImgFolder, fName), Path.Combine(dstImgFolder, fName), true);
                            MyOwner.OnLog(LogType.Warning, $"PDCAELimit文件复制OK;{Path.Combine(srcImgFolder, fName)}->{Path.Combine(dstImgFolder, fName)}\r\n");
                        }
                    }

                    //遍历OK和NG文件夹
                    if (Is3ANG)
                    {
                        if (srcImgFolder.Contains("\\OK\\"))
                        {
                            srcImgFolderEx = srcImgFolder.Replace("\\OK\\", "\\NG\\");
                        }
                        if (srcImgFolder.Contains("\\NG\\"))
                        {
                            srcImgFolderEx = srcImgFolder.Replace("\\NG\\", "\\OK\\");
                        }
                        if (Directory.Exists(srcImgFolderEx))
                        {
                            picList = Directory.GetFiles(srcImgFolderEx, "*.jpg");
                            foreach (var fPath in picList)
                            {
                                string fName = fPath.Substring(srcImgFolderEx.Length + 1);
                                if (fName.Contains(IsImageRol))
                                {
                                    File.Copy(Path.Combine(srcImgFolderEx, fName), Path.Combine(dstImgFolder, fName), true);
                                    MyOwner.OnLog(LogType.Warning, $"PDCAELimit文件3ANG复制OK;{Path.Combine(srcImgFolder, fName)}->{Path.Combine(dstImgFolder, fName)}\r\n");
                                }
                            }
                        }
                    }

                    copySuccess = 1;
                }
                catch (Exception ex)
                {
                    MyOwner.OnLog(LogType.Warning, $"PDCAELimit文件夹复制失败1;{srcImgFolder}->{dstImgFolder}\r\n{ex.ToString()}");
                }
                //});
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Warning, $"PDCAELimit文件夹复制失败2;\r\n{ex.ToString()}");
            }
        }

        /// <summary>
        /// 路径缓存成功
        /// </summary>
        private bool isConnectMacMini = false;
        private bool IsConnectMacMini
        {
            get
            {
                if (isConnectMacMini) return true;

                string path = Path.Combine(DesImagePath.GetString(Owner));
                path = path.Substring(0, 2);
                string dosline = @"/c net use " + path + @" \\169.254.1.10\Public\blobs " + "\"" + pdcaPassword + "\"" + " /user:" + "\"" + pdcaAccount + "\"" + " /persistent:no ";
                Process.Start("cmd.exe", @"/c @echo off");
                Process.Start("cmd.exe", dosline);
                isConnectMacMini = true;
                if (!isConnectMacMini) MyOwner.OnLog(LogType.Warning, $"PDCAELimit连接网盘失败;pdcaAccount={pdcaAccount}\r\npdcaPassword={pdcaPassword}\r\n");
                return isConnectMacMini;
            }
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "CommDevice")
            {
                _sfcHelper = null;
            }
            else if (parameter.Name == nameof(WIPFile))
            {
                if (newV != null && newV is LPath path)
                {
                    LoadWipFile(path.Path);
                }
            }
        }

        /// <summary>
        /// 加载WipFile
        /// </summary>
        /// <param name="path"></param>
        private void LoadWipFile(string path)
        {
            wipCodes.Clear();
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        wipCodes.Enqueue(line);
                    }
                }
            }
        }

        /// <summary>
        /// 回零完成
        /// </summary>
        public override void Home()
        {
            base.Home();
            if (IsCPKMode && WIPFile != null)
            {
                // 重新加载文件
                LoadWipFile(WIPFile.Path);
            }
        }

        public object GetCacheValue(IModule module)
        {
            // 如果对象作为数据列输出，并且存在缓存对象，那么尝试从缓存对象中获取
            if (!Owner.IsSameRoot(module) && module.CacheManager != null && !string.IsNullOrEmpty(module.DataID))
            {
                return module.CacheManager.GetCache(module.DataID, Owner.ID, Name);
            }
            else if (Owner.Status == RunStatus.Success || Owner is IGlobal)
            {
                // 正常模块需要检查石佛运行成功 全局模块不需要运行成功
                return _val;
            }
            else
            {
                // 如果是值类型就返回默认值
                if (Type.IsValueType || Type == typeof(LStatus))
                {
                    return _val;
                }

                return null;
            }
        }
    }
}
