using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.Web;
using Luster.Motion.Integration.WorkCardVerify;
using System;
using System.Collections;
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading;
using static System.Collections.Specialized.BitVector32;
using static System.Net.WebRequestMethods;
namespace Luster.Motion.Integration.SFC
{
    public class SFCHelper
    {
        //    IT在Y26年將對自動化組裝接口(subcmd= asy_4_sfc)進行升級優化， 
        //    所有的自動化組裝工站必須將原有的舊接口(subcmd= asy_4_sfc)替換為(subcmd= asy_4_sfc_new)，
        //先在Alpine BG的CIM&CG的CGL和CGSFA 3個工站驗證，驗證OK後將同步導入Y26所有自動化組裝相關設備。需要測試URL：172.25.3.168/fatpap?


        /// <summary>
        /// OK字符
        /// 
        /// </summary>
        private readonly string OKChar = "0 SFC OK";
        private readonly string OKChar_ = "0 SFC_OK";
        private readonly string NGChar_ = "0 SFC_OK NG";
        private readonly string RepairChar_ = "repair_in_time";
        private readonly string subcmd_old = "asy_4_sfc";
        private readonly string subcmd_new = "asy_4_sfc_new";
        /// <summary>
        /// 日志事件
        /// </summary>
        public event Action<LogType, string> LogEvent;

        // 常规地址
        private static string Url = "http://172.25.3.167/fatpvk";
        private static string subcmd = "";
        private static string Url_new = "http://172.25.3.168/fatpap";
        private static string Url1 = "http://172.25.3.167/fatpvk";
        // 2025-5-5 wyy 新增调机料上传IP
        private static string Url2 = "http://172.25.3.168/fatpvk";
        /// <summary>
        /// 2025-5-5 wyy 新增作业模式上传IP，从界面获取具体值
        /// </summary>
        public string UrlHomeWork { get; set; }
        private static string Product = "";
        private static string MacAddress = "";
        private static string StationID = "";
        private static string StationName = "";
        private static string Line = "";
        private static string StationNum = "";
        private static string StationType = "";
        private static string PartName = "";
        private static string Category_key = "";
        public static string CPKWIPFilePath = "";

        public static void SetCPKWIPFilePath(string filePath)
        {
            CPKWIPFilePath = filePath;
        }

        /// <summary>
        /// 参数配置发生变更
        /// </summary>
        /// <param name="config"></param>
        public static void SetConfig(WebConfig config)
        {
            MacAddress = config.DeviceMac;
            StationID = config.StationId;
            StationName = config.StationName;
            Line = config.LineCode;
            StationNum = config.UniteCode;
            Product = config.Product;
            Url = config.SFCUrl;
            subcmd = config.subcmd;
            StationType = config.StationType;
            PartName = config.PartName;
            Category_key = config.category_key;
            if (string.IsNullOrEmpty(MacAddress))
            {
                MacAddress = config.DeviceMac != "" ? config.DeviceMac : "18:47:3D:E9:BE:B5";
            }
        }

        private string SfcUrl = string.Empty;
        /// <summary>
        /// SFC 查询相关
        /// </summary>
        /// <param name="mController"></param>
        /// <param name="orderManager"></param>
        public SFCHelper(string url)
        {
            //if (string.IsNullOrEmpty(url))
            //{
            //    Url1 = Url;
            //}
            //else
            //{
            //    Url1=url;
            //}    
            SfcUrl = url;
            vComm = null;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="_vComm"></param>
        public SFCHelper(VCommuncation _vComm)
        {
            vComm = _vComm;
        }
        public virtual void HttpSend1(string cmd, string logTitle, Func<string, string> action, out string errMsg)
        {
            errMsg = "";

            if (vComm == null)
            {
                byte[] bytes = new byte[0];
                string result = "";
                //string url1 = $"{Url1}{cmd}";
                var u = string.IsNullOrEmpty(SfcUrl) ? Url : SfcUrl;
                string url1 = $"{u}?{cmd}";
                bool isBreakConnect = false;
                for (int i = 0; i < 3; i++)
                {
                    isBreakConnect = false;

                    // 构建Web HttpRequest
                    var webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 1000 };
                    try
                    {
                        result = webTool.Post(Url1, cmd);
                        action?.Invoke(result);
                        break;
                    }
                    catch (System.UriFormatException)
                    {
                        isBreakConnect = true;
                    }
                    catch (System.Net.WebException)
                    {
                        isBreakConnect = true;
                    }
                    catch (Exception ex)
                    {
                        OnLog(LogType.Error, $"SFC {logTitle} Request:{url1} Error", ex);
                    }
                    finally
                    {
                        OnLog(LogType.Debug, $"SFC 访问次数:{i + 1} {logTitle} {(string.IsNullOrEmpty(errMsg) ? "OK" : "Error")} Request:{url1} Result:{result}");
                    }
                }

                // 连续重试三次后，依然通讯异常，则报警
                if (isBreakConnect)
                {
                    throw new DeviceTimeoutException("N03OOOO-01", $"SFC Connected Fail");
                }
            }
            else
            {
                PDCASend(cmd, logTitle, action, out errMsg);
            }
        }
        /// <summary>
        /// 统一回调方法
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="logTitle"></param>
        /// <param name="action"></param>
        public virtual void HttpSend(string cmd, string logTitle, Func<string, string> action, out string errMsg)
        {
            errMsg = "";

            if (vComm == null)
            {
                byte[] bytes = new byte[0];
                string result = "";
                //string url1 = $"{Url1}?{cmd}";
                var u = string.IsNullOrEmpty(SfcUrl) ? Url : SfcUrl;
                if (subcmd == subcmd_new && (logTitle =="LCFM自动绑定WIP" 
                    || logTitle == "Flex自动绑定WIP"
                    || logTitle == "查询Flex绑定结果"
                    || logTitle == "Flex自动绑定WIP"))
                {
                    u = Url_new;
                }
                string url1 = $"{u}?{cmd}";
                bool isBreakConnect = false;
                for (int i = 0; i < 3; i++)
                {
                    isBreakConnect = false;

                    // 构建Web HttpRequest
                    var webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 1000 };
                    try
                    {
                        result = webTool.Post(url1, bytes);
                        errMsg = action?.Invoke(result);
                        break;
                    }
                    catch (System.UriFormatException)
                    {
                        isBreakConnect = true;
                    }
                    catch (System.Net.WebException)
                    {
                        isBreakConnect = true;
                    }
                    catch (Exception ex)
                    {
                        OnLog(LogType.Error, $"SFC {logTitle} Request:{url1} Error", ex);
                    }
                    finally
                    {
                        OnLog(LogType.Debug, $"SFC 访问次数:{i + 1} {logTitle} {(string.IsNullOrEmpty(errMsg) ? "OK" : "Error")} Request:{url1} Result:{result}");
                    }
                }

                // 连续重试三次后，依然通讯异常，则报警
                if (isBreakConnect)
                {
                    throw new DeviceTimeoutException("N03OOOO-01", $"SFC Connected Fail");
                }
            }
            else
            {
                PDCASend(cmd, logTitle, action, out errMsg);
            }
        }



        /// <summary>
        /// 统一回调方法
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="logTitle"></param>
        /// <param name="action"></param>
        public virtual void HttpSend2(string cmd, string logTitle, Func<string, string> action, out string errMsg)
        {
            errMsg = "";

            byte[] bytes = new byte[0];
            string result = "";
            var u = string.IsNullOrEmpty(SfcUrl) ? Url : SfcUrl;
            string url1 = $"{u}?{cmd}";
            bool isBreakConnect = false;
            for (int i = 0; i < 3; i++)
            {
                isBreakConnect = false;

                // 构建Web HttpRequest
                var webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 1000 };
                try
                {
                    result = webTool.Post(url1, bytes);
                    errMsg = action?.Invoke(result);
                    break;
                }
                catch (System.UriFormatException)
                {
                    isBreakConnect = true;
                }
                catch (System.Net.WebException)
                {
                    isBreakConnect = true;
                }
                catch (Exception ex)
                {
                    OnLog(LogType.Error, $"SFC {logTitle} Request:{url1} Error", ex);
                }
                finally
                {
                    OnLog(LogType.Debug, $"SFC 访问次数:{i + 1} {logTitle} {(string.IsNullOrEmpty(errMsg) ? "OK" : "Error")} Request:{url1} Result:{result}");
                }
            }

            // 连续重试三次后，依然通讯异常，则报警
            if (isBreakConnect)
            {
                throw new DeviceTimeoutException("N03OOOO-01", $"SFC Connected Fail");
            }

        }

        #region PDCA 通讯

        /// <summary>
        /// 通讯服务
        /// </summary>
        private VCommuncation vComm = null;

        /// <summary>
        /// 配置通讯服务
        /// </summary>
        /// <param name="vComm"></param>
        public void SetCommunation(VCommuncation vCommuncation)
        {
            vComm = vCommuncation;
        }



        /// <summary>
        /// PDCA 通讯发送与接收
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="sfcCommand"></param>
        private void PDCASend(string sfcCommand, string logTitle, Func<string, string> action, out string errMsg)
        {
            errMsg = "";
            string result = "";
            string pdcaCommand = $"sfc_post@{sfcCommand}\n";
            int startIndex = 0;
            int endIndex = 0;
            try
            {
                //3A尝试
                //防止有时返回值为空的情况，导致程序停止
                for (int i = 0; i < 3; i++)
                {
                    //0.判断是否连接，如果没有会连接一次
                    vComm.ConnectOK();

                    // 1.发送请求
                    vComm.Write(pdcaCommand);

                    // 2.接收数据
                    //result = vComm.ReadSingle<string>("", 3000);
                    result = vComm.ReadSingle<string>("", 15000, true, false);
                    if (string.IsNullOrEmpty(result))
                    {
                        errMsg = "SFC通讯异常，结果返回为空！";
                        throw new DeviceTimeoutException("N03OOOO-01", errMsg);
                    }
                    // 2.1 针对PDCA 返回结果进行处理
                    if (!string.IsNullOrEmpty(result))
                    {
                        startIndex = result.IndexOf('{');
                        if (result.Contains("err"))
                        {
                            endIndex = result.LastIndexOf('-') - 3;
                        }
                        else
                        {
                            endIndex = result.LastIndexOf('}') - 1;
                        }

                        //如果不为空，结束3A
                        break;
                    }
                }

                // 如果结束index<=开始Index，则报警，可恢复
                if ((endIndex - startIndex) <= 0)
                {
                    throw new DeviceTimeoutException("N03OOOO-01", $"Receive empty message");
                }
                string sfcResult = result.Substring(startIndex + 1, endIndex - startIndex);

                // 如果包含err信息，并通讯失败支持重连机制
                if (sfcResult.Contains("SFC Link may be disconnected"))
                {
                    throw new DeviceTimeoutException("N03OOOO-01", $"Link may be disconnected");
                }

                // 3.通讯后处理
                errMsg = action?.Invoke(sfcResult);
            }
            catch (DeviceTimeoutException ex)
            {
                OnLog(LogType.Error, logTitle, ex);
                throw new DeviceTimeoutException(ex.AlarmCode, $"PDCA Connect ERROR:{ex.Message}");
            }
            finally
            {
                // 所有LOG
                OnLog(LogType.Debug, $"PDCA {logTitle} Send:{pdcaCommand}Result:{result}");
            }
        }
        #endregion

        /// <summary>
        /// Log事件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        private void OnLog(LogType type, string message, Exception ex = null)
        {
            switch (type)
            {
                case LogType.Debug:
                    LogTool.Debug(message, "SFC");
                    break;
                case LogType.Info:
                    LogTool.Info(message, "SFC");
                    break;
                case LogType.Warning:
                    LogTool.Warn(message, "SFC");
                    break;
                case LogType.Error:
                    LogTool.Error(message, ex, "SFC");
                    break;
            }

            // 触发事件
            LogEvent?.Invoke(type, message);
        }

        #region 业务逻辑
        /// <summary>
        /// 获取Wip 码
        /// </summary>
        /// <returns></returns>
        public string GetWip(string sn, int wipLength, out string errMsg, bool isFirstStation = false)
        {
            string wip = "";
            string wipCommand = $"c=QUERY_RECORD&sn={sn}&p=fgsn";
            errMsg = "";
            bool isGetWipOK;
            for (int i = 0; i < 1; i++)
            {
                isGetWipOK = false;
                HttpSend(wipCommand, "获取Wip码", result =>
                {
                    var wipArray = result.Split('\n');
                    //预防解析字符串时，字符数量不够引起的报错
                    if (wipArray.Length > 1 && wipArray[1].Length >= 5 + wipLength)
                    {
                        wip = wipArray[1].Substring(5, wipLength);
                        isGetWipOK = true;
                        return string.Empty;
                    }
                    else
                    {
                        //如果是首站查询，则不需要3A
                        if (i <= 2 && !isFirstStation)
                        {
                            Thread.Sleep(10);
                            return string.Empty;
                        }
                        else
                        {
                            //强制为true，用于退出循环,因为正常情况下，首战是查不到WIP
                            isGetWipOK = true;
                            return "Wip码获取失败!";
                        }
                    }
                }, out errMsg);
                if (isGetWipOK)
                {
                    break;
                }
            }
            return wip;

        }

        // 2025-5-5 wyy 新增调机料上传接口
        public virtual void HttpSendTiaoJi(string cmd, string logTitle, Func<string, string> action, out string errMsg)
        {
            errMsg = "";

            byte[] bytes = new byte[0];
            string result = "";
            string url2 = $"{Url2}?{cmd}";
            bool isBreakConnect = false;
            for (int i = 0; i < 3; i++)
            {
                isBreakConnect = false;

                // 构建Web HttpRequest
                var webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 1000 };
                try
                {
                    result = webTool.Post(url2, bytes);
                    errMsg = action?.Invoke(result);
                    break;
                }
                catch (System.UriFormatException)
                {
                    isBreakConnect = true;
                }
                catch (System.Net.WebException)
                {
                    isBreakConnect = true;
                }
                catch (Exception ex)
                {
                    OnLog(LogType.Error, $"SFC {logTitle} Request:{url2} Error", ex);
                }
                finally
                {
                    OnLog(LogType.Debug, $"SFC 访问次数:{i + 1} {logTitle} {(string.IsNullOrEmpty(errMsg) ? "OK" : "Error")} Request:{url2} Result:{result}");
                }
            }

            // 连续重试三次后，依然通讯异常，则报警
            if (isBreakConnect)
            {
                throw new DeviceTimeoutException("N03OOOO-01", $"SFC Connected Fail");
            }
        }


        public virtual void GetTiaoJi(string sn, string ckey, string wbs, string partName, out string errMsg)
        {
            string wipCommand = $"c=PROCESS_HANDING&subcmd=add_mdp_record&serialNo={sn}&categoryKey={ckey}&wbs={wbs}&mac_address={MacAddress}&station_id={StationID}&partName={partName}&isAutomation=1";
            errMsg = "";
            HttpSendTiaoJi(wipCommand, "获取调机SFC上传数据", result =>
            {
                string[] strsplit = result.Split('\n');
                //预防解析字符串时，字符数量不够引起的报错
                if (strsplit.Length > 1 && strsplit[1].Length >= 2)
                {
                    string[] strparts = strsplit[0].Trim().Split(' ');
                    if ("SFC_OK" == strparts[1])
                    {
                        if ("OK" == strsplit[1])
                            return string.Empty;
                        else
                        {
                            if (strsplit[1].Contains("NG"))
                            {
                                string[] strsplit2 = strsplit[1].Split(' ');
                                return strsplit2[1];
                            }
                            else
                                return "SFC系统连接正常时，返回NG的调机料内容错误!";
                        }
                    }
                    else if ("SFC_FATAL_ERROR" == strparts[1])
                    {
                        string logTitle = "调机料上传通信连接异常";
                        OnLog(LogType.Error, $"SFC {logTitle} Receive:{strsplit[1]}");
                        string err = "SFC_FATAL_ERROR" + strsplit[1];
                        return err;
                    }
                    else
                        return "SFC系统返回的作业模式通信状态错误!";
                }
                else
                {
                    return "SFC系统返回调机料内容的长度错误!";
                }

            }, out errMsg);

            //for (int i = 0; i < 3; i++)
            //{
            //    isBreakConnect = false;
            //    // 构建Web HttpRequest
            //    var webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 1000 };
            //    try
            //    {
            //        result = webTool.Post(url2, bytes);
            //        //errMsg = action?.Invoke(result);
            //        string[] strsplit = result.Split('\n');
            //        if ("OK" == strsplit[1])
            //            errMsg = "OK";
            //        else
            //        {
            //            if (strsplit[1].Contains("NG"))
            //            {
            //                string[] strsplit2 = strsplit[1].Split(' ');
            //                errMsg = strsplit2[1];
            //            }
            //        }
            //        break;
            //    }
        }

        [Obsolete]
        // 2025-5-5 wyy 新增设备作业模式SFC查询
        public virtual void HttpSendHomework(string urlHomeWork, string cmd, string logTitle, Func<string, string> action, out string errMsg)
        {
            errMsg = "";

            byte[] bytes = new byte[0];
            string result = "";
            string url2 = $"{urlHomeWork}?{cmd}";
            bool isBreakConnect = false;
            for (int i = 0; i < 3; i++)
            {
                isBreakConnect = false;

                // 构建Web HttpRequest
                var webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 1000 };
                try
                {
                    result = webTool.Post(url2, bytes);
                    errMsg = action?.Invoke(result);
                    break;
                }
                catch (System.UriFormatException)
                {
                    isBreakConnect = true;
                }
                catch (System.Net.WebException)
                {
                    isBreakConnect = true;
                }
                catch (Exception ex)
                {
                    OnLog(LogType.Error, $"SFC {logTitle} Request:{url2} Error", ex);
                }
                finally
                {
                    OnLog(LogType.Debug, $"SFC 访问次数:{i + 1} {logTitle} {(string.IsNullOrEmpty(errMsg) ? "OK" : "Error")} Request:{url2} Result:{result}");
                }
            }

            // 连续重试三次后，依然通讯异常，则报警
            if (isBreakConnect)
            {
                throw new DeviceTimeoutException("N03OOOO-01", $"SFC Connected Fail");
            }
        }

        [Obsolete]
        public virtual void GetHomework(string cardtagid, out string errMsg)
        {
            string wipCommand = $"c=QUERY_4_SFC&subcmd=query_skills_info&isAutomation=1&cardtagid={cardtagid}&station_id={StationID}&mac_address={MacAddress}";
            errMsg = "";
            HttpSendHomework(UrlHomeWork, wipCommand, "获取作业模式卡号SFC上传数据", result =>
            {
                string[] strsplit = result.Split('\n');
                //预防解析字符串时，字符数量不够引起的报错
                if (strsplit.Length > 1 && strsplit[1].Length >= 2)
                {
                    string[] strparts = strsplit[0].Trim().Split(' ');
                    if ("SFC_OK" == strparts[1])
                    {
                        return strsplit[1];
                    }
                    else if ("SFC_FATAL_ERR" == strparts[1])
                    {
                        string logTitle = "作业模式通信连接异常";
                        OnLog(LogType.Error, $"SFC {logTitle} Receive:{strsplit[1]}");
                        string err = "SFC_FATAL_ERR" + strsplit[1];
                        return err;
                    }
                    else
                        return "SFC系统返回的作业模式通信状态错误!";
                }
                else
                    return "SFC系统返回作业模式内容的长度错误!";

            }, out errMsg);
        }


        /// <summary>
        /// 获取RecheckAOI配方
        /// </summary>
        /// <returns></returns>
        public string GetRecheckAOI(string sn, string stationID, string sfcStationCode, string mac_address, bool isFirstStation, out string errMsg)
        {
            string wip = string.Empty;
            errMsg = "";
            bool isGetWipOK;
            ArrayList jsonArray = new ArrayList();
            var machineParaData = new
            {
                c = "PROCESS_HANDING",
                subcmd = "get_last_replacement",
                station_id = stationID,
                sfcStationCode = sfcStationCode,
                sn = sn,
                mac_address = mac_address,
                isAutomation = "1",
                isTryrun = isFirstStation,

            };
            jsonArray.Add(machineParaData);
            string jsonData = JsonTool.ToJson(jsonArray);
            jsonData = jsonData.TrimStart('[').TrimEnd(']');
            for (int i = 0; i < 3; i++)
            {
                HttpSend1(jsonData, "获取RecheckAOI配方", result =>
                {
                    wip = result.ToString();
                    return wip;
                }, out errMsg);

                //如果反馈异常为空，作为OK，不再重发
                if (string.IsNullOrEmpty(errMsg))
                {
                    break;
                }

            }
            return wip;

        }
        #endregion

        #region 工单分配
        /// <summary>
        /// 查询工单余量
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public int GetOrderSurplus(string orderNo, out string errMsg)
        {
            int surplus = 0;
            errMsg = "";

            // 设备发送工单号查询工单余量
            string cmd = $"c=QUERY_4_SFC&subcmd=get_wip_color_info&wo_no={orderNo}&type=WOQTY&station_id={StationID}";
            HttpSend(cmd, "查询工单余量", r =>
            {
                r = r.Replace("\\r", "").Replace("\n", "").Trim();
                if (!int.TryParse(r, out var num))
                {
                    return $"查询工单余量无效无效，请联系相关人员查看！";
                }
                else
                {
                    surplus = num;
                    if (num == 0)
                    {
                        return $"查询工单余量:{orderNo}余量为零，请切换下一个工单！";
                    }

                    // 更新工单余量
                    return string.Empty;
                }
            }, out errMsg);

            return surplus;
        }

        /// <summary>
        /// LCGAndLCFM
        /// </summary>
        /// <param name="lcg"></param>
        /// <param name="lcfm"></param>
        /// <param name="errMsg"></param>
        public void MatchLCGAndLCFM(string lcg, string lcfm, out string errMsg)
        {
            // 判断LCG与LCFM是否匹配
            string cmd = $"c=QUERY_4_SFC&subcmd=compare_cg_to_lcfm&sn={lcg}&serial_no={lcfm}";
            HttpSend(cmd, "判断LCG与LCFM是否匹配", r =>
            {
                if (r.Contains(OKChar) || r.Contains("0 SFC_OK") && !r.ToLower().Contains("fail") && lcfm.Contains(lcg))
                {
                    // 返回分配错误数据
                    return string.Empty;
                }
                else
                {
                    if (!lcfm.Contains(lcg))
                    {
                        OnLog(LogType.Warning, $"lcfm{lcfm}未包含{lcg},不符合规则！！！");
                    }
                    return $"LCG与LCFM不匹配";
                }
            }, out errMsg);
        }

        /// <summary>
        /// 自动分配工单
        /// </summary>
        /// <param name="orderNo"></param>
        /// <param name="lcg"></param>
        /// <param name="lcfm"></param>
        /// <param name="wipLen"></param>
        /// <param name="errMsg"></param>
        public string AutoWip(string orderNo, string lcg, string lcfm, int wipLen, out string errMsg)
        {
            string wip = "";



            // 判断LCG与LCFM是否匹配
            //2.1.6 自动获取WIP
            //生成临时治具码时，通过时间戳+lineid的方式，保证临时治具码在时间和空间上的唯一性
            string tempCarrierId = DateTime.Now.ToString("yyyyMMddHHmmssfff") + StationID.Substring(5, 12).Replace("-", "");
            string cmd = $"c=QUERY_4_SFC&subcmd=carrier_link_csa_part&carrier_no={tempCarrierId}&wo_no={orderNo}&link_inf=1:{lcg}&ccd_flag=0&line={StationID}&mac_address={MacAddress}";

            //获取WIP，开启3A模式
            //for (int i = 0; i < 3; i++)
            //{
            HttpSend(cmd, "自动分配WIP", res =>
            {
                if (res.Contains("ERROR"))
                {
                    //3次都获取不到，才会触发失败
                    //if (i >= 2)
                    //{
                    return $"自动分配WIP失败";
                    //}
                    //else
                    //{
                    //    Thread.Sleep(10);
                    //    return string.Empty;
                    //}
                }
                else
                {
                    var wipArray = res.Split(':');
                    wip = wipArray[1].Substring(0, wipLen);
                    return string.Empty;
                }
            }, out errMsg);

            //////如果第一次就获取到，直接break
            //if (!string.IsNullOrEmpty(wip))
            //{
            //    break;
            //}

            //如果3次获取为空，返回错误
            if (string.IsNullOrEmpty(wip)/* && i >= 2*/)
            {
                errMsg = $"Wip 自动分配失败:{errMsg}";
                return wip;
            }
            //}


            // 绑定LCFM
            BindingLCFM(wip, lcfm, out errMsg);

            return wip;
        }

        /// <summary>
        /// LCG
        /// </summary>
        /// <param name="errMsg"></param>
        public void BindingLCFM(string wip, string lcfm, out string errMsg)
        {
            // 2.1.7 绑定LCFM
            // 扫描到的码，可能末尾带空格
            lcfm = lcfm.TrimEnd();
            var subcmd = Get_asy_4_sfc();
            string cmd = $"c=QUERY_4_SFC&subcmd={subcmd}&sn={wip}&serialno={lcfm}&partname=LCFM&ccdflag=0&line={StationID}&mac_address={MacAddress}";
            OnLog(LogType.Debug, $"绑定LCFM : {cmd}");
            HttpSend(cmd, $"LCFM自动绑定WIP", res =>
            {
                if (res.Contains("LCFM is ok to link") )
                {
                    return String.Empty;
                }
                else
                {
                    return $"LCFM:{lcfm}绑定WIP:{wip}失败:{res}";
                }
            }, out errMsg);
        }

        public bool BindingATLF(string wip, string lcfm, string partname, string ccdflag, string returnMessage, out string errMsg)
        {
            bool isOK = false;
            // 2.1.7 绑定LCFM
            // 扫描到的码，可能末尾带空格
            lcfm = lcfm.TrimEnd();
            var a = returnMessage.Split(',');
            var subcmd = Get_asy_4_sfc();
            string cmd = $"c=QUERY_4_SFC&subcmd={subcmd}&sn={wip}&serialno={lcfm}&partname={partname}&ccdflag={ccdflag}&line={StationID}&mac_address={MacAddress}";
            HttpSend(cmd, $"Flex自动绑定WIP", res =>
            {
                if (a.Length == 1)
                {
                    if (res.Contains(a[0]))
                    {
                        isOK = true;
                        return String.Empty;
                    }
                    else
                    {
                        isOK = false;
                        return $"Flex:{lcfm}绑定WIP:{wip}失败:{res}";
                    }
                }
                else if (a.Length == 2)
                {
                    if (res.Contains(a[0]) || res.Contains(a[1]))
                    {
                        isOK = true;
                        return String.Empty;
                    }
                    else
                    {
                        isOK = false;
                        return $"Flex:{lcfm}绑定WIP:{wip}失败:{res}";
                    }
                }

                return string.Empty;
            }, out errMsg);
            return isOK;
        }
        #endregion

        #region 上站查询
        /// <summary>
        /// 上站查询
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public int QueryUpStation(string wip, out string err)
        {
            int queryCount = 0;
            err = "";
            string stationCmd = $"c=QUERY_RECORD&p=unit_process_check&sn={wip}&tsid={StationID}";
            for (int i = 0; i < 2; i++)
            {
                HttpSend(stationCmd, $"查询上站,当前次数：{i + 1}", r =>
                {
                    if (!r.Contains("UNIT OUT OF PROCESS"))
                    {
                        queryCount = i + 1;
                        return string.Empty;
                    }
                    else
                    {
                        return "查询上站失败";
                    }
                }, out err);

                // 成功就退出
                if (string.IsNullOrEmpty(err))
                {
                    break;
                }

                if (i != 1)
                {
                    queryCount = i + 1;
                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    err = "查询上站失败";
                }
            }

            return queryCount;
        }
        #endregion
        #region CG3小料查询
        /// <summary>
        /// 上站查询
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool QueryHanding(string wip, string ccdflag, string orderLot, out string isDone, out string err)
        {
            int queryCount = 0;
            err = "";
            bool isOK = false;
            isDone = string.Empty;
            string status = string.Empty;
            string stationCmd = $"c=PROCESS_HANDING&subcmd=asy_4_lot&station_id={StationID}&mac_address={MacAddress}&wipNo={wip}&lotNo={orderLot}&partName={PartName}&ccdflag={ccdflag}&isAutomation=1";
            for (int i = 0; i < 3; i++)
            {
                HttpSend(stationCmd, $"卷料绑定确认：{i + 1}", r =>
                {
                    OnLog(LogType.Info, $"卷料绑定确认返回值{r}");
                    if (r.Contains("OK WIP asy lot ok") || r.Contains("OK WIP check lot ok") || r.Contains("has assembled lotNo"))
                    {
                        queryCount = i + 1;
                        isOK = true;
                        OnLog(LogType.Info, $"卷料绑定确认返回OK");
                        if (r.Contains("has assembled lotNo"))
                        {
                            status = "不上传";
                        }
                        else if (r.Contains("OK WIP check lot ok"))
                        {
                            status = "上传";
                        }
                        else if (r.Contains("OK WIP asy lot ok"))
                        {
                            status = "上传成功";
                        }
                        return string.Empty;
                    }
                    else
                    {
                        isOK = false;
                        OnLog(LogType.Info, $"卷料绑定确认返回NG");
                        return "绑定卷料失败";
                    }
                }, out err);

                // 成功就退出
                if (string.IsNullOrEmpty(err))
                {
                    break;
                }

                if (i != 2)
                {
                    queryCount = i + 1;
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    err = "绑定卷料失败";
                }
            }
            isDone = status;
            return isOK;
        }
        #endregion
        #region CG3小料余量查询
        /// <summary>
        /// 上站查询
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public int QueryHandingLot(string orderLot, out string err)
        {

            int surplus = 0;
            err = "";
            // 设备发送工单号查询工单余量
            string stationCmd = $"c=PROCESS_HANDING&subcmd=query_remain_lot&lotNo={orderLot}&partName={PartName}&station_id={StationID}&mac_address={MacAddress}&category_key={Category_key}&isAutomation=1";
            HttpSend(stationCmd, "查询卷料工单余量", r =>
            {

                var wipArray = r.Split('\n');
                string sfcResult = wipArray[1].ToString();
                if (!int.TryParse(sfcResult, out var num))
                {
                    return $"查询卷料工单余量无效无效，请联系相关人员查看！";
                }
                else
                {
                    surplus = num;
                    if (num == 0)
                    {
                        return $"查询工单余量:{orderLot}余量为零，请切换下一个工单！";
                    }

                    // 更新工单余量
                    return string.Empty;
                }
            }, out err);
            return surplus;
        }
        #endregion


        #region CG4组装确认查询
        /// <summary>
        /// 查询是否组装
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool QueryLotCheck(string orderLot, out string err)
        {
            int queryCount = 0;
            err = "";
            bool isOK = false;
            string stationCmd = $"c=PROCESS_HANDING&subcmd=lot_asy_check&station_id={StationID}&mac_address={MacAddress}&wipNo={orderLot}&partName={PartName}&isAutomation=1";
            for (int i = 0; i < 3; i++)
            {
                HttpSend(stationCmd, $"是否组装卷料,当前次数：{i + 1}", r =>
                {
                    OnLog(LogType.Info, $"是否组装卷料返回值{r}");
                    if (r.Contains("OK All Lots have bean assembled"))
                    {
                        queryCount = i + 1;
                        isOK = true;
                        OnLog(LogType.Info, $"是否组装卷料返回OK");
                        return string.Empty;
                    }
                    else
                    {
                        OnLog(LogType.Info, $"是否组装卷料返回NG");
                        return "查询是否组装失败";
                    }
                }, out err);
                // 成功就退出
                if (string.IsNullOrEmpty(err))
                {
                    break;
                }
                if (i != 2)
                {
                    queryCount = i + 1;
                    System.Threading.Thread.Sleep(10);
                }
                else
                {
                    err = "查询是否组装失败";
                }
            }

            return isOK;
        }


        /// <summary>
        /// 获取小料WIP
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="wipLength"></param>
        /// <param name="errMsg"></param>
        /// <param name="isFirstStation"></param>
        /// <returns></returns>
        public string GetSmallPartWip(string sn, int wipLength, out string errMsg)
        {
            string wip = "";
            string wipCommand = $"c=QUERY_RECORD&sn={sn}&p=fgsn&station_id={StationID}";

            errMsg = "";
            bool isGetWipOK;
            for (int i = 0; i < 1; i++)
            {
                isGetWipOK = false;
                HttpSend(wipCommand, "获取小料Wip码", result =>
                {
                    var wipArray = result.Split('\n');
                    //预防解析字符串时，字符数量不够引起的报错
                    if (wipArray.Length > 1 && wipArray[1].Length >= 5 + wipLength)
                    {
                        wip = wipArray[1].Substring(5, wipLength);
                        isGetWipOK = true;
                        return string.Empty;
                    }
                    else
                    {
                        if (i <= 2 )
                        {
                            Thread.Sleep(10);
                            return string.Empty;
                        }
                        else
                        {
                            //强制为true，用于退出循环,因为正常情况下，首战是查不到WIP
                            isGetWipOK = true;
                            return "小料Wip码获取失败!";
                        }
                    }
                }, out errMsg);
                if (isGetWipOK)
                {
                    break;
                }
            }
            return wip;

        }

        /// <summary>
        /// 获取Flex绑定结果
        /// </summary>
        /// <param name="wip"></param>
        /// <param name="smallPartWip"></param>
        /// <param name="partname"></param>
        /// <param name="ccdflag"></param>
        /// <param name="returnMessage"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool FlexBindingResult(string wip, string smallPartWip, string partname, out string errMsg)
        {
            bool isOK = false;
            var subcmd = Get_asy_4_sfc();
            string cmd = $"c=QUERY_4_SFC&subcmd={subcmd}&sn={wip}&serialno={smallPartWip}&partname={partname}&ccdflag=2&station_id={StationID}";   
            HttpSend(cmd, $"查询Flex绑定结果", res =>
            {
                if (res.Contains(OKChar_)&&res.Contains("is ok"))
                {
                    isOK = true;
                    return String.Empty;
                }
                else
                {
                    isOK = false;
                    return String.Empty;
                }
            }, out errMsg);
            return isOK;
        }

        /// <summary>
        /// 绑定Flex
        /// </summary>
        /// <param name="wip"></param>
        /// <param name="smallPartWip"></param>
        /// <param name="partname"></param>
        /// <param name="ccdflag"></param>
        /// <param name="returnMessage"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool BindingFlex(string wip, string smallPartWip, string partname, out string errMsg)
        {
            bool isOK = false;
            var subcmd = Get_asy_4_sfc();
            string cmd = $"c=QUERY_4_SFC&subcmd={subcmd}&sn={wip}&serialno={smallPartWip}&partname={partname}&ccdflag=0&line={StationID}&mac_address={MacAddress}";
            HttpSend(cmd, $"Flex自动绑定WIP", res =>
            {
                if (res.Contains(OKChar_)&&res.Contains("is ok"))
                {
                    isOK = true;
                    return String.Empty;
                }
                else
                {
                    isOK = false;
                    return $"Flex:{smallPartWip}绑定WIP:{wip}失败:{res}";
                }
            }, out errMsg);
            return isOK;
        }
        #endregion

        #region 获取治具码
        public string GetCarrierSN(string wip, out string errMsg)
        {
            string carrierSN = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss:fff");
            // 3.通过物料条码，获取载具码
            string cmd = $"c=QUERY_4_SFC&subcmd=query_carrierno_by_wip&sn={wip}";
            HttpSend(cmd, $"获取绑定载具", r =>
            {
                if (r.Contains("have not link effective"))
                {
                    return string.Empty;
                }

                // 通过 \n 获取载具信息
                var result = r.Split('\n');
                if (result.Length > 1 && !string.IsNullOrEmpty(result[1]))
                {
                    carrierSN = result[1];

                    if (!carrierSN.Contains("have not link"))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return $"通过Wip码未获取到治具码:{carrierSN}";
                    }
                }
                else
                {
                    return $"通过Wip码获取绑定载具失败:{r}";
                }
            }, out errMsg);

            return carrierSN;
        }

        public bool UpdataErrorCode(string wip, string starttime, string errorcode, string endtime, string errmsg, string stationid, string test_station_name, string product, string MacAddress, out string errMsg)
        {
            bool isOK = false;
            // 3.通过物料条码，获取载具码
            string cmd = $"c=ADD_RECORD&sn={wip}&result=FAIL&start_time={starttime}&stop_time={endtime}&station_id={stationid}&test_station_name={test_station_name}&failure_message={errmsg}&product={product}&list_of_failing_tests={errorcode}&mac_address={MacAddress}&audit_mode=0";
            HttpSend(cmd, $"SFC上传NG结果 ", r =>
            {
                if (r.Contains(OKChar) || r.Contains(OKChar_))
                {
                    isOK = true;
                    return string.Empty;
                }
                else
                {
                    isOK = false;
                    return $"上传errorcode失败";
                }
            }, out errMsg);
            return isOK;
        }
        public bool UpdataErrorCode(string wip, string starttime, string errorcode, string endtime, string errmsg, out string errMsg)
        {
            bool isOK = false;
            // 3.通过物料条码，获取载具码
            string cmd = $"c=ADD_RECORD&sn={wip}&result=FAIL&start_time={starttime}&stop_time={endtime}&station_id={StationID}&test_station_name={StationType}&failure_message={errmsg}&product={Product}&list_of_failing_tests={errorcode}&mac_address={MacAddress}&audit_mode=0";
            HttpSend(cmd, $"SFC上传NG结果 ", r =>
            {
                if (r.Contains(OKChar) || r.Contains(OKChar_))
                {
                    isOK = true;
                    return string.Empty;
                }
                else
                {
                    isOK = false;
                    return $"上传errorcode失败";
                }
            }, out errMsg);
            return isOK;
        }
        /// <summary>
        /// 治具解绑
        /// </summary>
        /// <param name="carrierSN"></param>
        /// <param name="errMsg"></param>
        public void UnbindCarrier(string carrierSN, out string errMsg)
        {
            string unbindCommand = $"c=QUERY_4_SFC&subcmd=carrier_dislink_moudle&carrier_sn={carrierSN}&station_id={StationID}";
            HttpSend(unbindCommand, "解绑载具", r =>
            {
                return string.Empty;
            }, out errMsg);
        }


        /// <summary>
        /// 治具解绑
        /// </summary>
        /// <param name="carrierSN"></param>
        /// <param name="errMsg"></param>
        public void QueryCarrier(string carrierSn, out string errMsg)
        {
            string cmd = $"c=QUERY_4_SFC&subcmd=qry_test_result&carrier_sn={carrierSn}&station_code={StationName}&station_id={StationID}";
            //string cmd = $"c=QUERY_4_SFC&subcmd=qry_test_result&carrier_sn={carrierSn}&station_code={StationType}&station_id={StationID}";
            HttpSend(cmd, "查询治具是否有绑定的SN", res =>
            {
                return string.Empty;
            }, out errMsg);
        }

        /// <summary>
        /// 绑定载具
        /// </summary>
        /// <param name="errMsg"></param>
        public void BindCarrier(string carrierSN, string wip, out string errMsg)
        {
            //2.载具绑定
            string cmd = $"c=QUERY_4_SFC&subcmd=carrier_link_csa&carrier_no={carrierSN}&link_inf=1:{wip}&station_id={StationID}";
            HttpSend(cmd, "绑定载具", r =>
            {
                if ((r.Contains(OKChar) || r.Contains(OKChar_)) && r.ToUpper().Contains("LINK SN SUCCESS"))
                {
                    return string.Empty;
                }
                else
                {
                    return $"载具绑定失败:{r}";
                }
            }, out errMsg);
        }

        /// <summary>
        /// 查询本站是否做过
        /// </summary>
        public bool QueryThisPass(string wip, out string errMsg, string defaultStationName = null, string defaultStationID = null)
        {
            bool thisPass = false;
            // 2.1 判断是否可以重新测试
            string testCmd = $"c=QUERY_4_SFC&subcmd=qry_reflow_test_result&sn={wip}&station={StationType}&station_id={StationID}";
            if (defaultStationName != null && defaultStationID != null)
            {
                testCmd = $"c=QUERY_4_SFC&subcmd=qry_reflow_test_result&sn={wip}&station={defaultStationName}&station_id={defaultStationID}";
            }
            //string testCmd = $"c=QUERY_4_SFC&subcmd=qry_reflow_test_result&sn={wip}&station={StationName}&station_id={StationID}";
            HttpSend(testCmd, "判断本站是否做过", r =>
            {
                if (r.Length > 1 && r.Contains("N:"))
                {
                    thisPass = false;
                }
                else
                {
                    thisPass = true;
                }

                return string.Empty;
            }, out errMsg);

            return thisPass;
        }
        #endregion

        /// <summary>
        /// 结果上传
        /// </summary>
        public void UploadResult(string wip, bool isOk, string errMsg, string errCode, out string err)
        {
            err = "";

            // 上传测试结果
            string cmd = $"c=ADD_RECORD&sn={wip}&result={(isOk ? "PASS" : "FAIL")}&start_time={DateTime.Now.AddSeconds(-5).ToString("yyyy-MM-dd HH:mm:ss")}&stop_time={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}&station_id={StationID}&test_station_name={StationName}&product={Product}&mac_address={MacAddress}&audit_mode=0&&failure_message={errMsg}&list_of_failing_tests={errCode}";

            int queryCount = 1;
            for (int i = 0; i < 3; i++)
            {
                HttpSend(cmd, $"SFC上传结果 {queryCount}", r =>
                {
                    if (r.Contains(OKChar) || r.Contains(OKChar_))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return $"结果上传失败:{r}";
                    }
                }, out err);

                // 成功就退出
                if (string.IsNullOrEmpty(err))
                {
                    break;
                }

                if (i != 2)
                {
                    queryCount = i + 1;
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    err = "Add_RECORD 结果上传失败";
                }
            }
        }


        /// <summary>
        /// 查询CardID
        /// </summary>
        public bool CheckCard(string ID, string machineSN, out string auth)
        {
            string err = "";
            auth = "";
            var sfcAuth = "";
            // 
            string cmd = $"c=QUERY_4_SFC&subcmd=query_skills_info&isAutomation=1&cardtagid={ID}&machinesn={machineSN}";// &station_id={StationID}&mac_address={MacAddress}";

            int queryCount = 1;
            for (int i = 0; i < 3; i++)
            {
                sfcAuth = "L1";
                HttpSend2(cmd, $"开始查询卡片信息 {queryCount}", r =>
                {
                    if (r.Contains("L1"))
                    {
                        sfcAuth = "L1";
                    }
                    if (r.Contains("L2"))
                    {
                        sfcAuth = "L2";
                    }
                    if (r.Contains("L3"))
                    {
                        sfcAuth = "L3";
                    }
                    if (r.Contains("L4"))
                    {
                        sfcAuth = "L4";
                    }
                    if (r.Contains("L5"))
                    {
                        sfcAuth = "L5";
                    }
                    if (r.Contains("L6"))
                    {
                        sfcAuth = "L6";
                    }
                    if (r.Contains("L7"))
                    {
                        sfcAuth = "L7";
                    }
                    if (r.Contains("L8"))
                    {
                        sfcAuth = "L8";
                    }
                    if (r.Contains("L9"))
                    {
                        sfcAuth = "L9";
                    }
                    if (r.Contains("VENDOR") || r.Contains("Foxconn") || r.ToUpper().Contains("LUSTER"))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return $"查询Card失败:{r}";
                    }
                }, out err);

                auth = sfcAuth;
                // 成功就退出
                if (string.IsNullOrEmpty(err))
                {
                    return true;
                }

                if (i != 2)
                {
                    queryCount = i + 1;
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    err = "查询Card失败";
                }
            }
            return false;
        }

        public bool CheckCard_login(string ID, out string auth, out string strReceived)
        {
            string err = "";
            auth = "";
            var sfcAuth = "";
            var strRe = "";
            strReceived = "";
            // 
            string cmd = $"c=QUERY_4_SFC&subcmd=query_skills_info&isAutomation=1&cardtagid={ID}";// &station_id={StationID}&mac_address={MacAddress}";

            int queryCount = 1;
            for (int i = 0; i < 3; i++)
            {
                sfcAuth = "L1";
                HttpSend2(cmd, $"开始查询卡片信息 {queryCount}", r =>
                {
                    strRe = r;
                    if (r.Contains("L1"))
                    {
                        sfcAuth = "L1";
                    }
                    if (r.Contains("L2"))
                    {
                        sfcAuth = "L2";
                    }
                    if (r.Contains("L3"))
                    {
                        sfcAuth = "L3";
                    }
                    if (r.Contains("L4"))
                    {
                        sfcAuth = "L4";
                    }
                    if (r.Contains("L5"))
                    {
                        sfcAuth = "L5";
                    }
                    if (r.Contains("L6"))
                    {
                        sfcAuth = "L6";
                    }
                    if (r.Contains("L7"))
                    {
                        sfcAuth = "L7";
                    }
                    if (r.Contains("L8"))
                    {
                        sfcAuth = "L8";
                    }
                    if (r.Contains("L9"))
                    {
                        sfcAuth = "L9";
                    }
                    if (r.Contains("VENDOR") || r.Contains("Foxconn") || r.ToUpper().Contains("LUSTER"))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return $"查询Card失败:{r}";
                    }


                }, out err);

                auth = sfcAuth;
                strReceived = strRe;

                // 成功就退出
                if (string.IsNullOrEmpty(err))
                {
                    return true;
                }

                if (i != 2)
                {
                    queryCount = i + 1;
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    err = "查询Card失败";
                }
            }
            return false;
        }


        /// <summary>
        /// 结果上传
        /// </summary>
        public void UploadResultOK(string wip, string StationID, string StationName, string Product, string MacAddress, out string err)
        {
            err = "";

            // 上传测试结果
            string cmd = $"c=ADD_RECORD&sn={wip}&result=PASS&start_time={DateTime.Now.AddSeconds(-5).ToString("yyyy-MM-dd HH:mm:ss")}&stop_time={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}&station_id={StationID}&test_station_name={StationName}&product={Product}&mac_address={MacAddress}&audit_mode=0";

            int queryCount = 1;
            for (int i = 0; i < 3; i++)
            {
                HttpSend(cmd, $"SFC上传OK结果 {queryCount}", r =>
                {
                    if (r.Contains(OKChar) || r.Contains(OKChar_))
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return $"结果上传失败:{r}";
                    }
                }, out err);

                // 成功就退出
                if (string.IsNullOrEmpty(err))
                {
                    break;
                }

                if (i != 2)
                {
                    queryCount = i + 1;
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    err = "Add_RECORD 结果上传失败";
                }
            }
        }



        public void KeyMaterial(string wip, out string errMsg)
        {
            // 查询关键物料
            HttpSend($"c=QUERY_4_SFC&subcmd=check_partlist&sn={wip}&pdca_station_code={StationType}", "SFC关键物料查询", r =>
            //HttpSend($"c=QUERY_4_SFC&subcmd=check_partlist&sn={wip}&pdca_station_code={StationType}", "SFC关键物料查询", r =>

            {
                if (r.ToUpper().Contains("OK:") || r.ToUpper().Contains("CHECK OK"))
                {
                    return string.Empty;
                }
                else
                {
                    return r;
                }
            }, out errMsg);
        }

        // 锁住PDCA
        private static object lockPDCA = new object();

        /// <summary>
        /// 数据三次上传
        /// </summary>
        /// <param name="startCmd"></param>
        /// <param name="title"></param>
        /// <param name="err"></param>
        public int SendPDCAData(string startCmd, string title, out string err, int timeOutMs = 10000, bool bIsAutoCloseConnect = false)
        {
            err = string.Empty;
            int queryCount = 1;
            string result = string.Empty;

            //取消PDCA 3次上传功能
            //for (int i = 0; i < 3; i++)
            //{
            try
            {
                //PDCA 3A
                for (int i = 0; i < 3; i++)
                {
                    lock (lockPDCA)
                    {
                        // 1.发生请求
                        vComm.Write(startCmd);

                        // 2.接收数据
                        result = vComm.ReadSingle<string>("", 3000);
                    }

                    //PDCA返回超时后，result会为空导致设备停机
                    if (string.IsNullOrEmpty(result)) continue;

                    if (result.StartsWith("ok"))
                    {
                        err = "";
                        break;
                    }
                    else if (result.StartsWith("err") && result.Contains("SFC Link may be disconnected"))
                    {
                        err = result;
                    }
                    else
                    {
                        err = result == string.Empty ? "PDCA返回空值" : result;
                        OnLog(LogType.Warning, $"PDCA读取Error,内容为:{result}");
                    }
                }

            }
            catch (Exception e)
            {
                OnLog(LogType.Error, $"PDCA Error:{e.Message}");
                err = "PDCA返回空值";
            }
            finally
            {
                // 所有LOG
                OnLog(LogType.Debug, $"PDCA {title} Recv:{startCmd} Result:{result}");
            }
            //}

            return queryCount;
        }

        /// <summary>
        /// 设备载具自动加黑管控接口
        /// 返回值True代表加入成功
        /// 返回值False代表加入失败
        /// </summary>
        public bool AddCarrierBlackList(string CarrierSN, string returnOKValue, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_4_SFC&subcmd=add_reuse_carrier&carrier_no={CarrierSN}&product_type=CG-LINK&line_code={Line}&station_id={StationID}&type=UOP";
            HttpSend(cmd, "加入黑名单", r =>
            {
                if (r.Length > 1 && r.Contains(returnOKValue))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                return string.Empty;
            }, out errMsg);
            if (!result)
            {
                OnLog(LogType.Error, $"PDCA Error 载具加入黑名单失败:{errMsg}");
            }
            return result;
        }

        /// <summary>
        /// 查询载具黑名单接口
        /// 返回值True代表在黑名单中
        /// 返回值False代表不在黑名单中
        /// </summary>
        public bool QueryCarrierBlackList(string CarrierSN, string returnOKValue, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_4_SFC&subcmd=query_blacklist_carrier&carrier_sn={CarrierSN}&station_id={StationID}";
            HttpSend(cmd, "查询黑名单", r =>
            {
                if (r.Length > 1 && r.Contains(returnOKValue))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
                return string.Empty;
            }, out errMsg);
            return result;
        }

        /// <summary>
        /// 查询物料黑名单
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="checkflag"></param>
        /// <param name="stationid"></param>
        /// <param name="errrMsg"></param>
        /// <returns></returns>
        public bool QueryMaterialBlackList(string sn, int checkflag, string stationid, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_4_SFC&subcmd=query_blacklist_status_csa&sn={sn}&check_flag={checkflag}&station_id={stationid}";
            const string sfc_ok = "SFC_OK";
            HttpSend(cmd, "查询物料黑名单", r =>
            {
                if (r.Contains(sfc_ok))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                return string.Empty;
            }, out errMsg);
            return result;
        }

        /// <summary>
        /// 查询是否是维修机
        /// </summary>
        /// <param name="Wip"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool QueryRepairRecord(string Wip, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_RECORD&sn={Wip}&p={RepairChar_}";
            HttpSend(cmd, "查询维修记录", r =>
            {
                if (r.Length > 1 && r.Contains(RepairChar_ + "=") && r.Contains("-"))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                return string.Empty;
            }, out errMsg);
            return result;
        }

        /// <summary>
        /// CG5实名制查询
        /// </summary>
        /// <param name="SN"></param>
        /// <param name="TestTime"></param>
        /// <param name="StationId"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool QueryCG5RealName(string SN, string rns_station, string TestTime, string StationId, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_4_SFC&subcmd=get_rns_station_times&sn={SN}&rns_station={rns_station}&test_time={TestTime}&station_id={StationId}";
            errMsg = "";
            HttpSend(cmd, "CG5实名制查询", r =>
            {
                var wipArray = r.Split('\n');
                //预防解析字符串时，字符数量不够引起的报错
                if (wipArray.Length > 1)
                {
                    if (Convert.ToInt32(wipArray[1]) > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }

                return string.Empty;
            }, out errMsg);
            return result;
        }

        public (bool, int) QueryCG5RealNameNew(string SN, string rns_station, string TestTime, string StationId, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_4_SFC&subcmd=get_rns_station_times&sn={SN}&rns_station={rns_station}&test_time={TestTime}&station_id={StationId}";
            errMsg = "";
            const string keyName = "SFC_OK";
            int realSelectTimes = 0;
            HttpSend(cmd, "CG5实名制查询", r =>
            {
                if (r.Contains(keyName))
                {
                    int index = r.IndexOf(keyName);
                    var strCount = r.Substring(index + keyName.Length).Trim();

                    if (int.TryParse(strCount, out var counts))
                    {
                        realSelectTimes = counts;
                    }
                    result = realSelectTimes > 0;
                }
                else
                {
                    result = false;
                }
                return string.Empty;
            }, out errMsg);
            return (result, realSelectTimes);
        }

        /// <summary>
        /// CG6查询排线绑定
        /// </summary>
        /// <param name="Wip"></param>
        /// <param name="StationId"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool QueryCG6CableBinding(string SN, string StationId, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_4_SFC&subcmd=check_partlist&sn={SN}&pdca_station_code={StationType}&station_id={StationId}";
            errMsg = "";
            HttpSend(cmd, "CG6查询排线绑定", r =>
            {
                if (string.IsNullOrEmpty(r) || !r.Contains("OK") || r.Contains("NG"))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
                return string.Empty;
            }, out errMsg);
            return result;
        }


        /// <summary>
        /// CG5查询Flex黑名单
        /// </summary>
        /// <param name="Wip"></param>
        /// <param name="StationId"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool QueryFlexBlacklist_Status(string SN, out string errMsg)
        {
            bool result = false;
            string cmd = $"c=QUERY_4_SFC&subcmd=query_blacklist_status_csa&sn={SN}&check_flag=3";
            errMsg = "";
            string res = "";
            for (int i = 0; i < 3; i++)
            {
                HttpSend(cmd, "CG5查询Flex黑名单", r =>
                {
                    res = r;
                    if (r.Contains(OKChar_))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    return string.Empty;
                }, out errMsg);
                //此种情况代表网络问题
                if (!res.Contains("SFC_FATAL_ERROR"))
                {
                    break;
                }
            }
            return result;
        }


        public string QueryWipByCarrierSn(string CarrierSn,out string errMsg)
        {
            string Wip = "None";
            errMsg = "";
            string cmd = $"c=QUERY_4_SFC&subcmd=query_vsn_by_carrier&carrier_sn={CarrierSn}&station_code={StationName}&station_id={StationID}";
            string res = "";
            for (int i = 0; i < 3; i++)
            {
                HttpSend(cmd, "通过载具码查询WIP", r =>
                {
                    res = r;
                    // 通过 \n 获取载具信息
                    var result = r.Split('\n');
                    if (result.Length > 1 && !string.IsNullOrEmpty(result[1]))
                    {
                        Wip = result[1];

                        if (!Wip.Contains("have not link"))
                        {
                            return "";
                        }
                        else
                        {
                            return $"通过治具码未获取Wip码:{Wip}";
                        }
                    }
                    else
                    {
                        return $"通过治具码获取WIP失败:{r}";
                    }
                }, out errMsg);
                //此种情况代表网络问题
                if (!res.Contains("SFC_FATAL_ERROR"))
                {
                    break;
                }

            }
            return Wip;
        }

        internal SFCRecvEventArgs AuthService_SFCCheckEvent(SFCEventArgs args)
        {
            UrlHomeWork = args.HomeWorkUrl;
            var cardtagid = args.CardId;
            string wipCommand = $"c=QUERY_4_SFC" +
                $"&subcmd=query_skills_info" +
                $"&isAutomation=1" +
                $"&cardtagid={cardtagid}";

            string url = $"{UrlHomeWork}?{wipCommand}";
            string strRecv = null;
            Exception exception = null;

            for (int i = 0; i < 3; i++)
            {
                var webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 1000 };
                try
                {
                    strRecv = webTool.Post(url, new byte[0]);
                    if (!string.IsNullOrEmpty(strRecv)) break;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    Thread.Sleep(300); // 增加重试间隔
                }
            }
            if (strRecv == null)
            {
                exception = exception ?? new DeviceTimeoutException("N03OOOO-01", $"SFC Connected Fail");
            }

            return new SFCRecvEventArgs
            {
                Recv = strRecv,
                SfcException = exception
            };
        }

        private string Get_asy_4_sfc()
        {
            return subcmd == subcmd_new ? subcmd_new : subcmd_old;
            if (Url.Contains("fatpap") && (StationID.Contains("1327") || StationID.Contains("1778")))  //1327  1778
            {
                return subcmd_new;
            }
            else
            {
                return subcmd_old;
            }
        }

         
    }
}
