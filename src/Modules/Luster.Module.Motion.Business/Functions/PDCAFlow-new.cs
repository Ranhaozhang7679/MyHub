using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.SFC;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
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
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class PDCAFlow_new : MotionFunction, IPauseFunction
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
            End
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
        /// 动作类型
        /// </summary>
        [NotEmpty]
        [Parameter("动作类型", 2, CN = "动作类型", DefaultV = PDCAType.SendData)]
        public PDCAType PDCAMode { get; set; }

        /// <summary>
        /// CPK模式
        /// </summary>
        [Parameter("CPK模式", 3, CN = "CPK", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsCPKMode { get; set; }

        /// <summary>
        /// CPK模式
        /// </summary>
        [Parameter("GRR模式", 4, CN = "GRR", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsGRRMode { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        [Parameter("SN", 5, CN = "SN", CanRef = ParamRef.Ref)]
        public string SN { get; set; }


        [Parameter("WIP", 6, CN = "WIP", CanRef = ParamRef.Ref)]
        public string WIP_Ex { get; set; }


        [Parameter("WIP长度", 7, CN = "WIP长度", DefaultV = 18)]
        public int WIP_Length { get; set; }

        /// <summary>
        /// MachineSN
        /// </summary>
        //[Parameter("MachineSN", 6, CN = "MachineSN", CanRef = ParamRef.Ref)]
        //public string MachineSN { get; set; }

        /// <summary>
        /// CarrierSN
        /// </summary>
        [Parameter("CarrierSN", 8, CN = "CarrierSN", CanRef = ParamRef.Ref)]
        public string CarrierSN { get; set; }


        /// <summary>
        /// CycleTime
        /// </summary>
        [Parameter("CycleTime", 9, CN = "CycleTime", CanRef = ParamRef.Ref, DefaultV = 4.5)]
        public double CycleTime { get; set; }

        /// <summary>
        /// IsQPL
        /// </summary>
        //[NotEmpty]
        //[Parameter("是否是QPL", 8, CN = "QPL", DefaultV = false)]
        //public bool IsQPL { get; set; }

        /// <summary>
        /// QPL站号
        /// </summary>
        //[DependOn("IsQPL", true)]
        //[Parameter("QPL站号", 9, CN = "QPL站号", DefaultV = 1)]
        //public int QPLNum { get; set; }

        // <summary>
        // 工位号
        // </summary>
        [Parameter("工位号", 11, CN = "工位号", CanRef = ParamRef.Ref, DefaultV = 1)]
        public int WorkId { get; set; }

        /// <summary>
        /// 源图片路径
        /// </summary>
        [NotEmpty]
        [Parameter("图片文件夹名称，以反斜杠进行分隔(/)", 12, CN = "源图片路径")]
        public LStringEx SourceImagePath { get; set; }

        /// <summary>
        /// 目标图片路径
        /// </summary>
        [NotEmpty]
        [Parameter("图片文件夹名称，以反斜杠进行分隔(/)", 13, CN = "目标图片路径")]
        public LStringEx DesImagePath { get; set; }


        /// <summary>
        /// 过程数据数量
        /// </summary>
        [Parameter("需要上传PDCA数据的数量", 14, CN = "上传数量", DefaultV = 1)]
        public int AECount { get; set; }

        /// <summary>
        /// 自动运行数据
        /// </summary>
        [NotEmpty]
        [Parameter("只需要对过程数据进行拼接，如X@1.1@0.4@1.5@mm", 15, CN = "过程数据")]
        public LStringEx ProdData { get; set; }

        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("PDCA上传结果,1成功；2图片拷贝失败；3数据上传失败;4图片和数据上传都失败", 23, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int iResult { get; set; }

        /// <summary>
        /// 输出WIP
        /// </summary>
        [Parameter("WIP", 25, CN = "WIP", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutWIP { get; set; }


        static string testSeriesID;

        // PDCA登录账户
        private readonly string pdcaAccount = "gdlocal";
        // PDCA 登录密码
        private readonly string pdcaPassword = "gdlocal";

        /// <summary>
        /// PDCA上传重新使用新的通信服务
        /// </summary>
        private VCommuncation vCommPdca = null;

        /// <summary>
        /// 
        /// </summary>
        private SFCHelper _sfcHelper = null;

        public PDCAFlow_new()
        {
            this.Tips = "PDCA流程处理";
            this.Icon = "\xe6a8";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
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

            ////1.获取通讯设备
            GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);
            var lseDatas = GetParametersByType<LStringEx>();

            //2.连接通讯设备
            vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            try
            {
                vCommPdca.Open();
            }
            catch (Exception)
            {
                // 连接失败的通讯支持重新赋值
                vCommPdca = null;
                throw;
            }

            List<LStringEx> aelimits = new List<LStringEx>();
            foreach (var item in lseDatas)
            {
                if (item.StringEx.Contains("@"))
                {
                    aelimits.Add(item);
                }
            }

            if (_sfcHelper == null)
            {
                _sfcHelper = new SFCHelper(vCommPdca);
            }

            // 2.1 获取缓存WIP
            //if (!string.IsNullOrEmpty(MyOwner.DataID))
            //{
            //    var cache = MyOwner.CacheManager.GetItem(MyOwner.DataID) as CacheItem;
            //    if (cache != null)
            //    {
            //        var cItem = cache.FirstOrDefault(u => u.ParamKey == "WIP");
            //        if (cItem != null && cItem.Value != null)
            //        {
            //            OutWIP = cItem.Value?.ToString();
            //            MyOwner.OnLog(LogType.Debug, $"PDCA获取缓存Wip:{OutWIP}");
            //        }
            //    }
            //}

            // 没有在缓存中获取到WIP，则通过网络查询
            //_sfcHelper.SetCommunation(vCommPdca);
            OutWIP = _sfcHelper.GetWip(SN, WIP_Length, out errMsg);

            // WIP 获取失败，信息提示
            if (!string.IsNullOrEmpty(errMsg))
            {
                //MyOwner.OnAlarm(AlarmType.InfoTip, $"Wip 码获取失败");
                iResult = 3;
                LogTool.Debug($"Wip 码获取失败，{errMsg}");
                return true;
            }

            switch (PDCAMode)
            {
                //1.发送Start命令
                case PDCAType.Start:
                    sendStr = $"{OutWIP}@start\n";
                    _sfcHelper.SendPDCAData(sendStr, "PDCA Start", out errMsg);
                    iResult = string.IsNullOrEmpty(errMsg) ? 1 : 3;
                    break;

                //3.发送所有参数
                case PDCAType.SendData:
                    sendStr = GetSendData(OutWIP, aelimits, IsCPKMode, IsGRRMode, WorkId, true);
                    _sfcHelper.SendPDCAData(sendStr, "PDCA SendData", out errMsg);
                    if (string.IsNullOrEmpty(errMsg))
                        iResult = 1;
                    else
                        iResult = 3;
                    break;

                //4.拷贝图片
                case PDCAType.CopyImage:

                    //判断是否能连接上
                    // 判断是否能连接上，能够连接上的情况下，再去复制
                    if (IsConnectMacMini)
                    {
                        CopyFolder(OutWIP, ref iCopyImgRes);

                        iResult = iCopyImgRes == 1 ? 1 : 2;
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
                    _sfcHelper.SendPDCAData(sendStr, "PDCA Wip查询", out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        MyOwner.OnAlarm(AlarmType.InfoTip, $"Wip 码获取失败");
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
                            CopyFolder(OutWIP, ref iCopyImgRes);
                        }
                    }
                    else
                    {
                        iCopyImgRes = 1;
                    }

                    // 3.发送Submit
                    sendStr = GetSendData(OutWIP, aelimits, IsCPKMode, IsGRRMode, WorkId, false);
                    _sfcHelper.SendPDCAData(sendStr, "PDCA上传数据", out errMsg);
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        OnAlarm(AlarmType.InfoTip, $"PDCA 数据上传失败:{errMsg}");
                        iSendPDCARes = 0;
                    }
                    else
                    {
                        iSendPDCARes = 1;
                    }

                    break;
            }

            if (iCopyImgRes == 1 && iSendPDCARes == 1)
                iResult = 1;
            else if (iCopyImgRes != 1)
                iResult = 2;
            else if (iSendPDCARes != 1)
                iResult = 3;
            else if (iCopyImgRes != 1 && iSendPDCARes != 1)
                iResult = 4;

            return true;
        }

        //拼接发送数据
        private string GetSendData(string wip, List<LStringEx> aelimits, bool IsCPK, bool IsGRR, int WorkId, bool IsAll = false)
        {
            string QPLNum = MyOwner.ConfigManager.GetWebConfig("UniteCode");

            //批量传递
            string sendContent;
            string data;
            string mode = "0";
            string priority = "0";
            string straudit = "@audit";
            string strData = "";
            string prodDatas = "";
            string cpkDatas = "";
            string aelmts = "";
            for (int i = 0; i < aelimits.Count; i++)
            {
                //把"dot"转换成小数点
                aelmts = aelimits[i].GetString(MyOwner).Replace("dot", ".");

                //添加符合规定格式的数据
                aelmts = $"{wip}@pdata@{aelmts}\n";
                //去除@CPK
                if (aelmts.Contains("@CPK"))
                {
                    aelmts = aelmts.Replace("@CPK", "");
                    cpkDatas += aelmts;
                }
                prodDatas += aelmts;
            }

            if (IsCPKMode)
            {
                //data = cpkDatas;
                data =
                   $"{prodDatas}" +
                   $"{wip}@pdata@Cycle Time@{CycleTime}@@@sec\n" +
                   $"{wip}@pdata@Peak Voltage@999@@@V\n" +
                   $"{wip}@pdata@Peak Current@999@@@A\n" +
                   $"{wip}@pdata@Active Energy Current Unit@999@@@w_h\n" +
                   $"{wip}@pdata@Active Energy Cummulative@999@@@w_h\n";
                mode = "1";
                priority = "-2";
                straudit = "@audit";
            }
            else if (IsGRRMode)
            {
                //data = cpkDatas;
                data =
                   $"{prodDatas}" +
                   $"{wip}@pdata@Cycle Time@{CycleTime}@@@sec\n" +
                   $"{wip}@pdata@Peak Voltage@999@@@V\n" +
                   $"{wip}@pdata@Peak Current@999@@@A\n" +
                   $"{wip}@pdata@Active Energy Current Unit@999@@@w_h\n" +
                   $"{wip}@pdata@Active Energy Cummulative@999@@@w_h\n";
                mode = "2";
                priority = "-2";
                straudit = "@audit";

            }
            else
            {
                data =
                $"{prodDatas}" +
                $"{wip}@pdata@Cycle Time@{CycleTime}@@@sec\n" +
                $"{wip}@pdata@Peak Voltage@999@@@V\n" +
                $"{wip}@pdata@Peak Current@999@@@A\n" +
                $"{wip}@pdata@Active Energy Current Unit@999@@@w_h\n" +
                $"{wip}@pdata@Active Energy Cummulative@999@@@w_h\n";
                mode = "0";
                priority = "0";
                straudit = "";
                testSeriesID = "0";
            }
            strData =
            $"{wip}@attr@Machine SN@{MyOwner.ConfigManager.GetWebConfig("MachineSN")}\n" +
            $"{wip}@attr@CG SN@{SN}\n" +
            $"{wip}@attr@Carrier SN@{CarrierSN}\n";

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
                    $"{wip}@submit@ABC-Luster.{MyOwner.Version}\n" +
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
                      $"{wip}@submit@ABC-Luster.{MyOwner.Version}\n" +
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
                       $"{wip}@submit@ABC-Luster.{MyOwner.Version}\n" +
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
                       $"{wip}@submit@ABC-Luster.{MyOwner.Version}\n" +
                       $"}}\n";
                }
            }

            return sendContent;
        }


        private void CopyFolder(string wip, ref int copySuccess)
        {
            copySuccess = 0;
            try
            {
                //源图片文件夹
                string srcImgFolder = Path.Combine(SourceImagePath.GetString(Owner), wip);
                if (!Directory.Exists(srcImgFolder))
                {
                    MyOwner.OnLog(LogType.Error, $"模块:{MyOwner.Alias} 原文件路径:{srcImgFolder}不存在");
                    return;
                }

                //目标图片路径
                string dstImgFolder = Path.Combine(DesImagePath.GetString(Owner));
                if (Directory.Exists(dstImgFolder))
                {
                    MyOwner.OnLog(LogType.Error, $"模块:{MyOwner.Alias} 原文件路径:{srcImgFolder}不存在");
                    return;
                }

                copySuccess = 1;

                string folderName = System.IO.Path.GetFileName(srcImgFolder);
                string destfolderdir = System.IO.Path.Combine(dstImgFolder, folderName);

                Task.Run(() =>
                {
                    try
                    {
                        Common.Tools.FolderTool.CopyFiles(srcImgFolder, destfolderdir);
                    }
                    catch (Exception)
                    {
                        MyOwner.OnLog(LogType.Warning, $"文件夹复制失败;{srcImgFolder}->{destfolderdir}");
                    }
                });
            }
            catch (Exception)
            {
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

                return isConnectMacMini;
            }
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "AECount" && int.TryParse(newV?.ToString(), out var num) && num >= 1)
            {
                BuildExternParameters(1, num, "ProdData", "过程数据", typeof(LStringEx), (p) =>
                {
                    p.EditorType = typeof(LStringEx);
                });
            }
        }
    }
}
