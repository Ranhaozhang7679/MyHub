using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
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
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class PDCAFlow : MotionFunction, IPauseFunction
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
        /// CPK模式
        /// </summary>
        [Parameter("CPK模式", 2, CN = "CPK", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsCPKMode { get; set; }

        /// <summary>
        /// CPK模式
        /// </summary>
        [Parameter("GRR模式", 3, CN = "GRR", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsGRRMode { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        [Parameter("SN", 4, CN = "SN", CanRef = ParamRef.Ref)]
        public string SN { get; set; }


        [Parameter("WIP", 5, CN = "WIP", CanRef = ParamRef.Ref)]
        public string WIP_Ex { get; set; }


        [Parameter("WIP长度", 5, CN = "WIP长度", DefaultV = 18)]
        public int WIP_Length { get; set; }

        /// <summary>
        /// MachineSN
        /// </summary>
        [NotEmpty]
        [Parameter("MachineSN", 6, CN = "MachineSN", CanRef = ParamRef.Ref)]
        public string MachineSN { get; set; }

        /// <summary>
        /// CarrierSN
        /// </summary>
        [Parameter("CarrierSN", 6, CN = "CarrierSN", CanRef = ParamRef.Ref)]
        public string CarrierSN { get; set; }


        /// <summary>
        /// CycleTime
        /// </summary>
        [Parameter("CycleTime", 7, CN = "CycleTime", CanRef = ParamRef.Ref, DefaultV = 4.5)]
        public double CycleTime { get; set; }

        /// <summary>
        /// IsQPL
        /// </summary>
        [NotEmpty]
        [Parameter("是否CG2", 8, CN = "是否CG2", DefaultV = false)]
        public bool IsCG2 { get; set; }

        /// <summary>
        /// IsQPL
        /// </summary>
        [NotEmpty]
        [Parameter("是否是QPL", 8, CN = "QPL", DefaultV = false)]
        public bool IsQPL { get; set; }

        /// <summary>
        /// QPL站号
        /// </summary>
        [DependOn("IsQPL", true)]
        [Parameter("QPL站号", 9, CN = "QPL站号", DefaultV = 1)]
        public int QPLNum { get; set; }

        // <summary>
        // 工位号
        // </summary>
        [Parameter("工位号", 9, CN = "工位号", CanRef = ParamRef.Ref, DefaultV = 1)]
        public int WorkId { get; set; }

        /// <summary>
        /// 过程数据数量
        /// </summary>
        [Parameter("需要上传PDCA数据的数量", 9, CN = "上传数量", DefaultV = 1)]
        public int AECount { get; set; }

        /// <summary>
        /// 源图片路径
        /// </summary>
        [NotEmpty]
        [Parameter("图片文件夹名称，以反斜杠进行分隔(/)", 10, CN = "源图片路径")]
        public LStringEx SourceImagePath { get; set; }

        /// <summary>
        /// 目标图片路径
        /// </summary>
        [NotEmpty]
        [Parameter("图片文件夹名称，以反斜杠进行分隔(/)", 11, CN = "目标图片路径")]
        public LStringEx DesImagePath { get; set; }

        /// <summary>
        /// 自动运行数据
        /// </summary>
        [NotEmpty]
        [Parameter("只需要对过程数据进行拼接，如X@1.1@0.4@1.5@mm", 13, CN = "过程数据")]
        public LStringEx ProdData { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        [NotEmpty]
        [Parameter("动作类型", 14, CN = "动作类型", DefaultV = PDCAType.SendData)]
        public PDCAType PDCAMode { get; set; }


        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("PDCA上传结果,1成功；2图片拷贝失败；3数据上传失败;4图片和数据上传都失败", 20, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int iResult { get; set; }

        /// <summary>
        /// 输出WIP
        /// </summary>
        [DependOn("PDCAMode", PDCAType.GetWIP)]
        [Parameter("WIP", 20, CN = "WIP", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public String OutWIP { get; set; }


        static string testSeriesID;

        /// <summary>
        /// PDCA上传重新使用新的通信服务
        /// </summary>
        private VCommuncation vCommPdca = null;

        public PDCAFlow()
        {
            this.Tips = "PDCA流程处理";
            this.Icon = "\xe6c9";
        }
        public override bool DoExcute(out string errMsg)
        {
            // 空跑模式或者没启用直接返回
            // 1成功；2图片拷贝失败；3数据上传失败; 4图片和数据上传都失败
            if (IsEmptyMode || !IsEnable)
            {
                iResult = 1;
                errMsg = "";
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
            //源图片文件夹
            string sourceimgPath = "";
            //目标图片路径
            string desimgPath = Path.Combine(DesImagePath.GetString(Owner));
            // 复制图片返回值
            int iCopyImgRes;
            //发送数据返回值
            int iSendPDCARes;
            //输出结果清零
            iResult = 0;
            // PDCA登录账户
            string pdcaAccount = "gdlocal";
            // PDCA 登录密码
            string pdcaPassword = "gdlocal";
            #endregion

            // 创建一个新的通信
            if (vCommPdca == null)
            {
                //1.获取通讯设备
                GetVDevice<VCommuncation>(CommDevice, out var vCommu);
                vCommPdca = vCommu.Clone() as VCommuncation;
            }

            ////1.获取通讯设备
            //GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);
            var lseDatas = GetParametersByType<LStringEx>();
            string WIP = "";

            //2.连接通讯设备
            vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            vCommPdca.Open();
            List<LStringEx> aelimits = new List<LStringEx>();
            foreach (var item in lseDatas)
            {
                if (item.StringEx.Contains("@"))
                {
                    aelimits.Add(item);
                }
            }

            //2.1获取WIP
            if (!string.IsNullOrEmpty(WIP_Ex))
            {
                WIP = WIP_Ex;
            }
            else
            {
                sendStr = $"sfc_post@c=QUERY_RECORD&sn={SN}&p=fgsn\n";
                MyOwner.OnLog(LogType.Debug, $"PDCA发送数据,获取WIP:{sendStr}");
                vCommPdca.Write(sendStr);
                // 等待结果
                string socketResult = vCommPdca.ReadSingle<string>("", 5000);
                MyOwner.OnLog(LogType.Debug, $"PDCA接收数据,获取WIP:{socketResult}");
                var wipArray = socketResult.Split('\n');
                if (wipArray.Length > 1 && wipArray[1].Length >= WIP_Length)
                {
                    WIP = wipArray[1].Substring(5, WIP_Length);
                }
            }
            //确保已经有WIP
            sourceimgPath = Path.Combine(SourceImagePath.GetString(Owner), WIP);

            //3.动作
            switch (PDCAMode)
            {
                //1.发送Start命令
                case PDCAType.Start:
                    sendStr = $"{WIP}@start\n";
                    iSendPDCARes = SendDataToPDCA(sendStr);
                    iResult = iSendPDCARes == 1 ? 1 : 3;
                    break;

                //2.获取WIP
                case PDCAType.GetWIP:
                    sendStr = $"sfc_post@c=QUERY_RECORD&sn={SN}&p=fgsn\n";
                    MyOwner.OnLog(LogType.Debug, $"PDCA发送数据,只获取WIP:{sendStr}");
                    vCommPdca.Write(sendStr);
                    // 等待结果
                    string socketResult = vCommPdca.ReadSingle<string>("", 5000);
                    MyOwner.OnLog(LogType.Debug, $"PDCA接收数据,只获取WIP:{socketResult}");
                    var wipArray = socketResult.Split('\n');
                    if (wipArray.Length > 1 && wipArray[1].Length >= WIP_Length)
                    {
                        OutWIP = wipArray[1].Substring(5, WIP_Length);
                    }
                    else
                        OutWIP = SN;
                    break;



                //3.发送所有参数
                case PDCAType.SendData:
                    sendStr = GetSendData(IsCPKMode, IsGRRMode, IsQPL, QPLNum, true);
                    iSendPDCARes = SendDataToPDCA(sendStr);
                    if (iSendPDCARes == 1)
                        iResult = 1;
                    else
                        iResult = 3;
                    break;


                //4.拷贝图片
                case PDCAType.CopyImage:

                    //判断是否能连接上
                    if (ConnectStatus(desimgPath, pdcaAccount, pdcaPassword))
                    {
                        iCopyImgRes = CopyFolder(sourceimgPath, desimgPath);
                        iResult = iCopyImgRes == 1 ? 1 : 2;
                    }
                    else
                    {
                        iResult = 2;
                        MyOwner.OnLog(LogType.Debug, $"PDCAFlow连接网盘失败," +
                            $"sourceimgPath={sourceimgPath}\r\n" +
                            $"desimgPath={desimgPath}\r\n" +
                            $"pdcaAccount={pdcaAccount}\r\n" +
                            $"pdcaPassword={pdcaPassword}\r\n");
                    }
                    break;

                //5.所有动作
                case PDCAType.Whole:

                    //1.第一步 发送Start
                    if (IsCPKMode || IsGRRMode)
                    {
                        sendStr = $"{WIP}@start@audit\n";
                    }
                    else
                    {
                        sendStr = $"{WIP}@start\n";
                    }
                    SendDataToPDCA(sendStr);

                    //2.拷贝图片
                    //CPK模式下/GRR模式下，不上传图片
                    if (!IsCPKMode && !IsGRRMode)
                    {
                        //判断是否能连接上
                        //能够连接上的情况下，再去复制
                        if (ConnectStatus(desimgPath, pdcaAccount, pdcaPassword))
                        {
                            iCopyImgRes = CopyFolder(sourceimgPath, desimgPath);
                        }
                        else
                        {
                            iCopyImgRes = 0;
                            MyOwner.OnLog(LogType.Debug, $"PDCAFlow连接网盘失败," +
                            $"sourceimgPath={sourceimgPath}\r\n" +
                            $"desimgPath={desimgPath}\r\n" +
                            $"pdcaAccount={pdcaAccount}\r\n" +
                            $"pdcaPassword={pdcaPassword}\r\n");
                        }

                    }
                    else
                    {
                        iCopyImgRes = 1;
                        MyOwner.OnLog(LogType.Debug, $"PDCAFlow在CPK模式下/GRR模式下，不上传图片，默认返回成功");
                    }

                    //3.发送Submit
                    sendStr = GetSendData(IsCPKMode, IsGRRMode, IsQPL, QPLNum, false);
                    iSendPDCARes = SendDataToPDCA(sendStr);
                    if (iCopyImgRes == 1 && iSendPDCARes == 1)
                        iResult = 1;
                    else if (iCopyImgRes != 1)
                        iResult = 2;
                    else if (iSendPDCARes != 1)
                        iResult = 3;
                    else if (iCopyImgRes != 1 && iSendPDCARes != 1)
                        iResult = 4;
                    break;

                //6.发送结束指令
                case PDCAType.End:
                    sendStr = $"{WIP}@submit@Luster.11.12.23.45_v1.02\n";
                    iSendPDCARes = SendDataToPDCA(sendStr);
                    iResult = iSendPDCARes == 1 ? 1 : 3;
                    break;

                //默认
                default:
                    break;
            }


            #region 方法
            /// <summary>
            /// 复制文件夹及文件
            /// </summary>
            /// <param name="sourceFolder">原文件路径</param>
            /// <param name="destFolder">目标文件路径</param>
            /// <returns></returns>
            int CopyFolder(string sourceFolder, string destFolder)
            {
                try
                {
                    if (!Directory.Exists(sourceFolder))
                    {
                        MyOwner.OnLog(LogType.Error, $"PDCAFlow模块:{MyOwner.Alias} 原文件路径:{sourceFolder}不存在");
                        return 1;
                    }

                    string folderName = System.IO.Path.GetFileName(sourceFolder);
                    string destfolderdir = System.IO.Path.Combine(destFolder, folderName);
                    Common.Tools.FolderTool.CopyFiles(sourceFolder, destfolderdir);

                    return 1;
                }
                catch (Exception e)
                {
                    MyOwner.OnLog(LogType.Error, $"PDCAFlow拷贝文件失败，\r\n{e.ToString()}");
                    return 0;
                }

            }

            //拼接发送数据
            string GetSendData(bool IsCPK, bool IsGRR, bool IsQPL, int QPLNum, bool IsAll = false)
            {

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
                    aelmts = $"{WIP}@pdata@{aelmts}\n";
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
                       $"{WIP}@pdata@Cycle Time@{CycleTime}@@@sec\n" +
                       $"{WIP}@pdata@Peak Voltage@999@@@V\n" +
                       $"{WIP}@pdata@Peak Current@999@@@A\n" +
                       $"{WIP}@pdata@Active Energy Current Unit@999@@@w_h\n" +
                       $"{WIP}@pdata@Active Energy Cummulative@999@@@w_h\n";
                    mode = "1";
                    priority = "-2";
                    straudit = "@audit";
                }
                else if (IsGRRMode)
                {
                    //data = cpkDatas;
                    data =
                       $"{prodDatas}" +
                       $"{WIP}@pdata@Cycle Time@{CycleTime}@@@sec\n" +
                       $"{WIP}@pdata@Peak Voltage@999@@@V\n" +
                       $"{WIP}@pdata@Peak Current@999@@@A\n" +
                       $"{WIP}@pdata@Active Energy Current Unit@999@@@w_h\n" +
                       $"{WIP}@pdata@Active Energy Cummulative@999@@@w_h\n";
                    mode = "2";
                    priority = "-2";
                    straudit = "@audit";

                }
                else
                {
                    data =
                    $"{prodDatas}" +
                    $"{WIP}@pdata@Cycle Time@{CycleTime}@@@sec\n" +
                    $"{WIP}@pdata@Peak Voltage@999@@@V\n" +
                    $"{WIP}@pdata@Peak Current@999@@@A\n" +
                    $"{WIP}@pdata@{(IsCG2 ? "Active energy current unit" : "Active Energy Current Unit")}@999@@@w_h\n" +
                    $"{WIP}@pdata@{(IsCG2 ? "Active energy cumulative" : "Active Energy Cummulative")}@999@@@w_h\n";
                    mode = "0";
                    priority = "0";
                    straudit = "";
                    testSeriesID = "0";
                }
                strData =
                $"{WIP}@attr@Machine SN@{MachineSN}\n" +
                $"{WIP}@attr@CG SN@{SN}\n" +
                $"{WIP}@attr@{(IsCG2 ? "Fixture SN" : "Carrier SN")}@{CarrierSN}\n";

                if (IsCPKMode || IsGRRMode)
                {
                    if (IsAll)
                    {
                        sendContent =
                        $"_{{\n" +
                        $"{WIP}@start{straudit}\n" +
                        $"{strData}" +
                        $"{data}" +
                        $"{WIP}@pdata@Mode@{mode}\n" +
                        $"{WIP}@pdata@Operator_ID@1\n" +
                        $"{WIP}@pdata@Priority@{priority}\n" +
                        $"{WIP}@pdata@TestSeriesID@{testSeriesID}\n" +
                        $"{WIP}@submit@MyOwner.ConfigManager.GetWebConfig(\"SoftVersion\")\n" +
                        $"}}\n";
                    }
                    else
                    {
                        sendContent =
                          $"_{{\n" +
                          //$"{WIP}@start{straudit}\n" +
                          $"{WIP}@dut_pos@{QPLNum}@{WorkId}\n" +
                          $"{strData}" +
                          $"{data}" +
                          $"{WIP}@pdata@Mode@{mode}\n" +
                          $"{WIP}@pdata@Operator_ID@1\n" +
                          $"{WIP}@pdata@Priority@{priority}\n" +
                          $"{WIP}@pdata@TestSeriesID@{testSeriesID}\n" +
                          $"{WIP}@submit@AMyOwner.ConfigManager.GetWebConfig(\"SoftVersion\")\n" +
                          $"}}\n";
                    }
                }
                else
                {
                    if (IsAll)
                    {
                        sendContent =
                           $"_{{\n" +
                           $"{WIP}@start{straudit}\n" +
                           $"{strData}" +
                           $"{data}" +
                           $"{WIP}@pdata@Mode@{mode}\n" +
                           $"{WIP}@pdata@Operator_ID@1\n" +
                           $"{WIP}@pdata@Priority@{priority}\n" +
                           $"{WIP}@pdata@TestSeriesID@{0}\n" +
                           $"{WIP}@submit@MyOwner.ConfigManager.GetWebConfig(\"SoftVersion\")\n" +
                           $"}}\n";
                    }
                    else
                    {
                        sendContent =
                           $"_{{\n" +
                           //$"{WIP}@start{straudit}\n" +
                           $"{WIP}@dut_pos@{QPLNum}@{WorkId}\n" +
                           $"{strData}" +
                           $"{data}" +
                           $"{WIP}@pdata@Mode@{mode}\n" +
                           $"{WIP}@pdata@Operator_ID@1\n" +
                           $"{WIP}@pdata@Priority@{priority}\n" +
                           $"{WIP}@pdata@TestSeriesID@{0}\n" +
                           $"{WIP}@submit@MyOwner.ConfigManager.GetWebConfig(\"SoftVersion\")\n" +
                           $"}}\n";
                    }

                }

                return sendContent;
            }

            //发送数据
            //1代表 OK
            //2代表 NG
            int SendDataToPDCA(string sendData)
            {
                MyOwner.OnLog(LogType.Debug, $"PDCA发送数据:{sendData}");
                vCommPdca.Write(sendData);
                // 等待结果
                string socketResult = vCommPdca.ReadSingle<string>("", 5000);
                MyOwner.OnLog(LogType.Debug, $"PDCA接收数据:{socketResult}");
                if (socketResult.Contains("ok"))
                    return 1;
                else
                    return 2;
            }


            /// <summary>
            /// PDCA映射网盘连接状态
            /// </summary>
            /// <param name="path">目标路径</param>
            /// <param name="account">账号</param>
            /// <param name="password">密码</param>
            /// <returns></returns>
            bool ConnectStatus(string path, string account, string password)
            {
                bool flag = true;
                path = path.Substring(0, 2);
                string dosline = @"/c net use " + path + @" \\169.254.1.10\Public\blobs " + "\"" + password + "\"" + " /user:" + "\"" + account + "\"" + " /persistent:no ";
                Process.Start("cmd.exe", @"/c @echo off");
                Process.Start("cmd.exe", dosline);
                return flag;

                //bool Flag = false;
                //Process proc = new Process();
                //try
                //{
                //    proc.StartInfo.FileName = "cmd.exe";
                //    proc.StartInfo.UseShellExecute = false;
                //    proc.StartInfo.RedirectStandardInput = true;
                //    proc.StartInfo.RedirectStandardOutput = true;
                //    proc.StartInfo.RedirectStandardError = true;
                //    proc.StartInfo.CreateNoWindow = true;
                //    proc.Start();
                //    //登录验证
                //    string dosLine = @"net use " + path + " " + password + " /User:domain\\" + account;
                //    proc.StandardInput.WriteLine("net use * /del /y");
                //    proc.StandardInput.WriteLine(dosLine);
                //    proc.StandardInput.WriteLine("exit");


                //    while (!proc.HasExited)
                //    {
                //        proc.WaitForExit(1000);
                //    }
                //    string errormsg = proc.StandardError.ReadToEnd();
                //    proc.StandardError.Close();
                //    if (string.IsNullOrEmpty(errormsg))
                //    {
                //        Flag = true;
                //    }
                //    else
                //    {
                //        throw new Exception(errormsg);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Flag=false;
                //    throw ex;
                //}
                //finally
                //{
                //    proc.Close();
                //    proc.Dispose();
                //}
                //return Flag;
            }


            #endregion
            return base.DoExcute(out errMsg);
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
