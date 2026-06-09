using Foxconn.IMES.PictureCollection;
using Foxconn.IMES.PictureCollection.Model;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Regions;
using SimpleTCP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using static FreeSql.Internal.GlobalFilter;
using static System.Collections.Specialized.BitVector32;
using Path = System.IO.Path;

namespace Luster.Motion.Integration.Web
{
    public enum RepairState
    {
        Normal = 0,
        HelpRequest = 1,
        RepairStart = 2,
        RepairEnd = 3,
        ReportEnd = 4
    }

    public class HiveState
    {
        public bool IsRepairing { get; set; }
        public RepairState HiveRepairState { get; set; } = RepairState.Normal;
        public int HiveMachineState { get; set; } = 0;
        public string HiveCurrentCode { get; set; } = "";

        // 2025-12-10 新增字段，记录当前维修事件ID
        public string CurrentEventId { get; set; } = "";
        // 2026-2-24 新增字段，记录上一个维修事件ID
        public string LastEventId { get; set; } = "";
        // 记录repair start时的报警码，用于report end判断周/月保养
        public string RepairStartAlarmCode { get; set; } = "";
    }

    // ==================== 新增：Hive事件链追踪 ====================
    internal class HiveEventChain
    {
        public string EventId { get; set; } = "";
        public DateTime? HelpRequestTime { get; set; }
        public bool HelpRequestSent { get; set; }
        public DateTime? RepairStartTime { get; set; }
        public bool RepairStartSent { get; set; }
        public DateTime? RepairEndTime { get; set; }
        public bool RepairEndSent { get; set; }
        public DateTime? ReportEndTime { get; set; }
        public bool ReportEndSent { get; set; }

        public bool IsOpen => HelpRequestSent && !RepairEndSent; // 简化定义：未RepairEnd视为未完成链路
        public bool IsCompleted => HelpRequestSent && RepairStartSent && RepairEndSent; // ReportEnd可选，由业务决定是否必填
    }
    // ============================================================

    public class IniParser
    {
        private Dictionary<string, Dictionary<string, string>> _sections = new();

        public void Load(string filePath)
        {
            var currentSection = new Dictionary<string, string>();
            _sections[""] = currentSection; // 默认无节区

            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    continue; // 忽略注释和空行

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    var sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                    currentSection = new Dictionary<string, string>();
                    _sections[sectionName] = currentSection;
                }
                else
                {
                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, equalsIndex).Trim();
                        string value = trimmedLine.Substring(equalsIndex + 1).Trim();
                        currentSection[key] = value;
                    }
                }
            }
        }

        public string GetValue(string section, string key, string defaultValue = "")
        {
            if (_sections.TryGetValue(section, out var sectionData))
                if (sectionData.TryGetValue(key, out var value))
                    return value;
            return defaultValue;
        }
    }

    public class StationInfo
    {
        public string machine_sn { get; set; }
        public string machine_model_number { get; set; }
        public string machine_rifd { get; set; }
        public string vendor { get; set; }
        public string site { get; set; }
        public string building { get; set; }
        public string product { get; set; }
        public string station_type { get; set; }
        public string line { get; set; }
        // 2025-5-10 A客户要求 string-->int
        public int instance { get; set; }
        public string line_type { get; set; }
        public string station_name { get; set; }
        // 2025-5-6 wyy新增字段build_type
        public string build_type { get; set; }
    }

    public class HiveAPI : BaseMESAPI, ISubWebSystem
    {
        // 2025-6-16
        public string repairVersion = "1.0.0.0";
        public string spareVersion = "1.0.0.0";
        public string ApiVision = "";
        public static string token = "";
        public string machineSN = "";
        private ICacheManager _cacheManager;
        private string PDCANAME = "Extend_PDCA启用";
        private string CPKNAME = "Extend_CPK启用";
        private string DRYRUNNAME = "Extend_空跑模式";
        private string GRRNAME = "Extend_GRR启用";
        private static int curStatus = 0;
        private static int latestStatus = 0;
        public static int seed = 0;
        public static Random rand = new Random();
        public static bool isShieldDoor = false;
        public static bool IsUploadPara = false;
        public string statusNow = string.Empty;
        // 2025-5-10 wyy
        public static int seque = 0;
        private const string Str_RepairEnd = "repair end";
        private string currentEventID = "";
        private string lastEventID = "";
        /// <summary>
        /// 错误内容
        /// </summary>
        private readonly IErrorManager _errorMangaer;
        private static bool IsFirstLoad = false;
        public HiveState _hiveState = new HiveState();

        //hive report上传弹窗是否打开
        public bool Dialog2Part2_Opend = false;

        // ============== 新增：链路追踪容器与锁 ==============
        private readonly object _chainLock = new object();
        private readonly Dictionary<string, HiveEventChain> _hiveEventChains = new Dictionary<string, HiveEventChain>();

        private HiveEventChain GetOrCreateChain(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            lock (_chainLock)
            {
                if (!_hiveEventChains.TryGetValue(eventId, out var chain))
                {
                    chain = new HiveEventChain { EventId = eventId };
                    _hiveEventChains[eventId] = chain;
                }
                return chain;
            }
        }

        private bool HasOpenChain()
        {
            lock (_chainLock)
            {
                return _hiveEventChains.Values.Any(c => c.IsOpen);
            }
        }

        private void MarkChain(string eventId, RepairState state, DateTime ts)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return;
            var chain = GetOrCreateChain(eventId);
            if (chain == null) return;
            lock (_chainLock)
            {
                switch (state)
                {
                    case RepairState.HelpRequest:
                        chain.HelpRequestSent = true;
                        chain.HelpRequestTime = ts;
                        break;
                    case RepairState.RepairStart:
                        chain.RepairStartSent = true;
                        chain.RepairStartTime = ts;
                        break;
                    case RepairState.RepairEnd:
                        chain.RepairEndSent = true;
                        chain.RepairEndTime = ts;
                        break;
                    case RepairState.ReportEnd:
                        chain.ReportEndSent = true;
                        chain.ReportEndTime = ts;
                        break;
                }
            }
        }
        // ===================================================

        System.Threading.Timer timer;

        //与客户系统交互确认OnBoard状态
        private bool GetOnboardState()
        {
            // 2025-5-12 POST方法，发送时带body数据
            var path = "D:\\Hive\\VendorStationInfo.ini";
            //LogTool.Debug("GetOnboardState---start");
            //读取INI文件，提取需要发送的内容
            if (File.Exists(path))
            {
                //string content = File.ReadAllText("filename.txt");
                var parser = new IniParser();
                parser.Load(path);
                stationInfo.machine_sn = parser.GetValue("machine_info", "machine_sn", "");
                stationInfo.machine_sn = stationInfo.machine_sn.Trim('"');
                machineSN = stationInfo.machine_sn;
                stationInfo.machine_model_number = parser.GetValue("machine_info", "machine_model_number", "");
                stationInfo.machine_model_number = stationInfo.machine_model_number.Trim('"');
                stationInfo.machine_rifd = parser.GetValue("machine_info", "machine_rfid", "");
                stationInfo.machine_rifd = stationInfo.machine_rifd.Trim('"');
                stationInfo.vendor = parser.GetValue("station_info", "vendor", "");
                stationInfo.vendor = stationInfo.vendor.Trim('"');
                stationInfo.site = parser.GetValue("station_info", "site", "");
                stationInfo.site = stationInfo.site.Trim('"');
                stationInfo.building = parser.GetValue("station_info", "building", "");
                stationInfo.building = stationInfo.building.Trim('"');
                stationInfo.product = parser.GetValue("station_info", "product", "");
                stationInfo.product = stationInfo.product.Trim('"');
                stationInfo.station_type = parser.GetValue("station_info", "station_type", "");
                stationInfo.station_type = stationInfo.station_type.Trim('"');
                stationInfo.line = parser.GetValue("station_info", "line", "");
                stationInfo.line = stationInfo.line.Trim('"');
                string instss = "";
                int inst2 = 0;
                instss = parser.GetValue("station_info", "instance", "");
                int.TryParse(instss, out inst2);
                stationInfo.instance = inst2;
                stationInfo.line_type = parser.GetValue("station_info", "line_type", "");
                stationInfo.line_type = stationInfo.line_type.Trim('"');
                stationInfo.station_name = parser.GetValue("station_info", "station_name", "");
                stationInfo.station_name = stationInfo.station_name.Trim('"');
                stationInfo.build_type = parser.GetValue("station_info", "build_type", "");
                stationInfo.build_type = stationInfo.build_type.Trim('"');
                //LogTool.Debug("GetOnboardState----iniFile.Exists(path) == true end");
            }
            else
            {
                //读取文件异常，转为OffBoard并记录日志
                this.Status = TrainRunMode.OffBoard;
                LogTool.Error("ini文件已被删除，需要人工重新下载！");
                LogTool.Debug("GetOnboardState----iniFile.Exists(path) == false end，异常处理");
                return false;//若未存在文件，则下面Get2_post会抛出异常，不会返回false
            }
            var statusData = new
            {
                machine_sn = stationInfo.machine_sn,
                machine_model_number = stationInfo.machine_model_number,
                machine_rfid = stationInfo.machine_rifd,
                vendor = stationInfo.vendor,
                site = stationInfo.site,
                building = stationInfo.building,
                product = stationInfo.product,
                station_type = stationInfo.station_type,
                line = stationInfo.line,
                instance = stationInfo.instance,
                line_type = stationInfo.line_type,
                station_name = stationInfo.station_name,
                build_type = stationInfo.build_type,
            };
            string jsonData = JsonTool.ToJson(statusData);
            string host = sysConfig.HiveIP.Replace("http://", "");
            string url_onboard = $"http://{host}:{sysConfig.HivePort}/idms/v2/station/status";
            LogTool.Debug($"HiveSimulator地址（http 开头，/ 结尾）: {sysConfig.HiveSimulatorURL}");
            if (Get2_post(url_onboard, jsonData, 5))
                return true;
            else
                return false;
            // 方法1：GET  2025-05-12 GET方法需要发送body，而不是parameter引用，
            // 改为使用POST方法，带body和不带body
            //var str = Get("http://10.0.0.2:5008/idms/v2/station/status", 5);
            //LogTool.Debug("GetOnboardState():str " + str);
            //if (str.Contains("onboarding_status") && str.Contains("Onboarded"))
            //    return true;
            ////else if (Get2_post("http://10.0.0.2:5008/idms/v2/station/status", "", 5))  jsonData
            //else if (Get2_post("http://10.0.0.2:5008/idms/v2/station/status", jsonData, 5))
            //    return true;
            //else
            //    return false;
        }

        private StationInfo stationInfo = new StationInfo();

        private void BeatHeart(object obj)
        {
            string url = Path.Combine(URL, "capture/v6/heartbeat");
            string urlSim = Path.Combine(URLSimulator, "capture/v6/heartbeat");
            var statusData = new
            {
                machine_sn = stationInfo.machine_sn,
                machine_model_number = stationInfo.machine_model_number,
                machine_rfid = stationInfo.machine_rifd,
                vendor = stationInfo.vendor,
                site = stationInfo.site,
                building = stationInfo.building,
                product = stationInfo.product,
                station_type = stationInfo.station_type,
                line = stationInfo.line,
                instance = stationInfo.instance,
                line_type = stationInfo.line_type,
                station_name = stationInfo.station_name,
                build_type = stationInfo.build_type,
            };
            string jsonData = JsonTool.ToJson(statusData);
            _ = SendAsync(url, jsonData, 5, enableLog: true);
            _ = SendAsync(urlSim, jsonData, 5);
        }

        public override void StartFinish()
        {
            try
            {
                // 2025-5-10
                string sequess = sysConfig.UniteCode;
                if (!int.TryParse(sequess, out seque))
                {
                    LogTool.Error("sequess转为int类型失败");
                }
                //与客户系统交互确认OnBoard状态
                IsOnboard = GetOnboardState();
                //LogTool.Debug("IsOnboard：" + IsOnboard);
                //如果是OffBoard状态，改变界面显示
                var path = "D:\\Hive\\VendorStationInfo.ini"; // VendorStationInfo.ini"  stationInfo
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                //if (!File.Exists(path))
                //    File.Create(path);
                //// 手动将状态改为：已登录
                //IsOnboard = true;
                //LogTool.Debug("IsOnboard：" + IsOnboard);
                if (IsOnboard == false)
                {
                    LogTool.Debug("IsOnboard=false分支");
                    this.Status = TrainRunMode.OffBoard;

                    //PDCA获取设备信息
                    stationInfo.site = "FXGL";
                    stationInfo.product = "V53";
                    stationInfo.station_type = "AE-72";
                    stationInfo.line = "C04-4FC-03";
                    stationInfo.instance = 3;
                    stationInfo.building = "C04";
                    stationInfo.line_type = "default";
                    stationInfo.station_name = "default";
                    stationInfo.build_type = "CRB";

                    try
                    {
                        using (TcpClient client = new TcpClient())
                        {
                            // 连接服务器
                            client.Connect("169.254.1.10", 1111);

                            using (NetworkStream stream = client.GetStream())
                            {
                                // 获取Site
                                // 发送数据
                                string message = $"_{{@ghi_site@}}\n";
                                byte[] sendData = Encoding.ASCII.GetBytes(message);
                                LogTool.Debug("发送数据1： " + Encoding.ASCII.GetString(sendData));
                                stream.Write(sendData, 0, sendData.Length);

                                // 接收响应
                                byte[] buffer = new byte[1024];
                                int bytesRead;
                                StringBuilder response = new StringBuilder();

                                var startTime = DateTime.Now;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    response.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                                    LogTool.Debug("接收数据1： " + response.ToString());
                                    if (response.Length > 3)
                                    {
                                        break;
                                    }

                                    if ((DateTime.Now - startTime).TotalSeconds > 2)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(50);
                                }

                                var str = response.ToString();
                                if (str.Contains("ok@"))
                                {
                                    stationInfo.site = str.Substring(str.IndexOf("ok@{") + 4);
                                    stationInfo.site = stationInfo.site.Substring(0, stationInfo.site.Count() - 3);
                                }

                                // 获取product
                                // 发送数据
                                message = $"_{{@ghi_product@}}\n"; // sfc_post@c=ghi_product
                                sendData = Encoding.ASCII.GetBytes(message);
                                stream.Write(sendData, 0, sendData.Length);

                                // 接收响应
                                buffer = new byte[1024];
                                response = new StringBuilder();

                                startTime = DateTime.Now;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    response.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                                    if (response.Length > 3)
                                    {
                                        break;
                                    }

                                    if ((DateTime.Now - startTime).TotalSeconds > 2)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(50);
                                }

                                str = response.ToString();
                                if (str.Contains("ok@"))
                                {
                                    stationInfo.product = str.Substring(str.IndexOf("ok@{") + 4);
                                    stationInfo.product = stationInfo.product.Substring(0, stationInfo.product.Count() - 3);
                                }

                                // 获取station_type
                                // 发送数据
                                message = $"_{{@ghi_station_type@}}\n";
                                sendData = Encoding.ASCII.GetBytes(message);
                                stream.Write(sendData, 0, sendData.Length);

                                // 接收响应
                                buffer = new byte[1024];
                                response = new StringBuilder();

                                startTime = DateTime.Now;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    response.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                                    if (response.Length > 3)
                                    {
                                        break;
                                    }

                                    if ((DateTime.Now - startTime).TotalSeconds > 2)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(50);
                                }

                                str = response.ToString();
                                if (str.Contains("ok@"))
                                {
                                    stationInfo.station_type = str.Substring(str.IndexOf("ok@{") + 4);
                                    stationInfo.station_type = stationInfo.station_type.Substring(0, stationInfo.station_type.Count() - 3);
                                }

                                // 获取station_id
                                // 发送数据
                                message = $"_{{@ghi_station@}}\n";
                                sendData = Encoding.ASCII.GetBytes(message);
                                stream.Write(sendData, 0, sendData.Length);

                                // 接收响应
                                buffer = new byte[1024];
                                response = new StringBuilder();

                                startTime = DateTime.Now;
                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    response.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                                    if (response.Length > 3)
                                    {
                                        break;
                                    }

                                    if ((DateTime.Now - startTime).TotalSeconds > 2)
                                    {
                                        break;
                                    }
                                    Thread.Sleep(50);
                                }

                                str = response.ToString();
                                if (str.Contains("ok@"))
                                {
                                    var stationId = str.Substring(str.IndexOf("ok@{") + 4);
                                    stationId = stationId.Substring(0, stationId.Count() - 1);

                                    var strs = stationId.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (strs.Length == 4)
                                    {
                                        stationInfo.line = strs[1];
                                        int inst = 0;
                                        int.TryParse(strs[2], out inst);
                                        stationInfo.instance = inst;
                                        if (stationInfo.line.Contains("-"))
                                        {
                                            stationInfo.building = stationInfo.line.Substring(0, stationInfo.line.IndexOf("-"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"网络错误: {ex.Message}");
                    }

                    //确认ini文件是否生成，未生成的话需要进行生成
                    if (File.Exists(path) == false)
                    {
                        LogTool.Debug("开始生成ini文件");
                        var lines = new List<string>();
                        lines.Add("[machine_info]");
                        lines.Add($"machine_model_number=\"{sysConfig.MachineType}\"");
                        lines.Add($"machine_sn=\"{sysConfig.MachineSn}\"");
                        lines.Add("machine_rfid=\"000000000000A123456789\"");

                        lines.Add("[station_info]");
                        lines.Add("vendor=\"LUSTER\"");

                        //写入PDCA反馈的值
                        lines.Add($"site=\"{stationInfo.site}\"");
                        lines.Add($"product=\"{stationInfo.product}\"");
                        lines.Add($"station_type=\"{stationInfo.station_type}\"");
                        lines.Add($"line=\"{stationInfo.line}\"");
                        lines.Add($"instance={stationInfo.instance}");
                        lines.Add($"building=\"{stationInfo.building}\"");
                        lines.Add($"line_type=\"{stationInfo.line_type}\"");
                        lines.Add($"station_name=\"{stationInfo.station_name}\"");
                        lines.Add($"build_type=\"{stationInfo.build_type}\"");
                        File.WriteAllLines(path, lines);
                    }
                    else
                    {
                        //读取INI文件，并更新PDCA相关数据
                        // 删除已有ini文件的内容，将从PDCA处获取的数据写入ini
                        LogTool.Warn("ini文件已存在，请人工检查正确性！");
                        LogTool.Debug("IsOnboard=false分支----iniFile.Exists(path) == true end");

                    }
                }
                else
                {
                    //读取INI文件，提取需要发送的内容
                    if (File.Exists(path))
                    {
                        //string content = File.ReadAllText("filename.txt");
                        var parser = new IniParser();
                        parser.Load(path);
                        stationInfo.machine_sn = parser.GetValue("machine_info", "machine_sn", "");
                        stationInfo.machine_sn = stationInfo.machine_sn.Trim('"');
                        stationInfo.machine_model_number = parser.GetValue("machine_info", "machine_model_number", "");
                        stationInfo.machine_model_number = stationInfo.machine_model_number.Trim('"');
                        stationInfo.machine_rifd = parser.GetValue("machine_info", "machine_rfid", "");
                        stationInfo.machine_rifd = stationInfo.machine_rifd.Trim('"');
                        stationInfo.vendor = parser.GetValue("station_info", "vendor", "");
                        stationInfo.vendor = stationInfo.vendor.Trim('"');
                        stationInfo.site = parser.GetValue("station_info", "site", "");
                        stationInfo.site = stationInfo.site.Trim('"');
                        stationInfo.building = parser.GetValue("station_info", "building", "");
                        stationInfo.building = stationInfo.building.Trim('"');
                        stationInfo.product = parser.GetValue("station_info", "product", "");
                        stationInfo.product = stationInfo.product.Trim('"');
                        stationInfo.station_type = parser.GetValue("station_info", "station_type", "");
                        stationInfo.station_type = stationInfo.station_type.Trim('"');
                        stationInfo.line = parser.GetValue("station_info", "line", "");
                        stationInfo.line = stationInfo.line.Trim('"');
                        string instss = "";
                        int inst2 = 0;
                        instss = parser.GetValue("station_info", "instance", "");
                        int.TryParse(instss, out inst2);
                        stationInfo.instance = inst2;
                        stationInfo.line_type = parser.GetValue("station_info", "line_type", "");
                        stationInfo.line_type = stationInfo.line_type.Trim('"');
                        stationInfo.station_name = parser.GetValue("station_info", "station_name", "");
                        stationInfo.station_name = stationInfo.station_name.Trim('"');
                        stationInfo.build_type = parser.GetValue("station_info", "build_type", "");
                        stationInfo.build_type = stationInfo.build_type.Trim('"');

                        // 2025-5-11 为便于测试，暂时屏蔽计时器（5min发一次心跳）
                        timer = new System.Threading.Timer(new TimerCallback(BeatHeart), this, 0, 5 * 60 * 1000);//创建定时器
                        // 发送软件版本信息
                        Register();
                        // Hive状态转为idle                
                        this.Status = TrainRunMode.Idle;
                        LogTool.Debug("IsOnboard=true分支----iniFile.Exists(path) == true end");
                    }
                    else
                    {
                        //读取文件异常，转为OffBoard并记录日志
                        this.Status = TrainRunMode.OffBoard;
                        LogTool.Error("ini文件已被删除，需要人工重新下载！");
                        LogTool.Debug("IsOnboard=true分支----iniFile.Exists(path) == false end，异常处理");
                    }
                }

                if (JsonSerializerHelper<HiveState>.Load(out _hiveState, out string reason) == false || _hiveState == null)
                {
                    _hiveState = new HiveState();
                    _hiveState.HiveRepairState = RepairState.Normal;
                    _hiveState.HiveMachineState = 1;
                    JsonSerializerHelper<HiveState>.Save(_hiveState, out reason);
                }
                currentEventID = _hiveState.CurrentEventId ?? string.Empty;
                lastEventID = _hiveState.LastEventId ?? string.Empty;

                if (_hiveState.HiveRepairState == RepairState.HelpRequest)
                {
                    // 2025-05-02 客户要求
                    //RepairStart("Stopped For repair", "F990000-1", "L1");
                    RepairStart("Stopped For repair", "F99OOOO-01", "L1");
                }
                else if (_hiveState.HiveRepairState == RepairState.RepairStart)
                {

                }
                else if (_hiveState.HiveRepairState == RepairState.Normal && (_hiveState.HiveMachineState == 1 || _hiveState.HiveMachineState == 2))
                {
                    HelpRequest("Machine stopped through UI", "F99OOOO-20");
                    RepairStart("Stopped For repair", "F99OOOO-01", "L1");
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogTool.Debug("StartFinish函数:" + ex.ToString());
            }

        }

        private void SetIsRepairing(bool isRepairing)
        {
            _hiveState.IsRepairing = isRepairing;
            SaveHiveState();
        }

        private void SaveHiveState()
        {
            //JsonSerializerHelper<HiveState>.Save(_hiveState, out string reason);
            // 异步保存，不阻塞当前流程
            Task.Run(() =>
            {
                try
                {
                    JsonSerializerHelper<HiveState>.Save(_hiveState, out string reason);
                    if (!string.IsNullOrEmpty(reason))
                    {
                        LogTool.Warn($"保存HiveState失败: {reason}");
                    }
                }
                catch (Exception ex)
                {
                    LogTool.Error($"保存HiveState异常: {ex}");
                }
            });
        }

        private VisionAPI _visionAPI;
        public HiveAPI(ICacheManager cacheManager, IErrorManager errorManager, VisionAPI visionAPI)
        {
            ApiVision = "Luster" + SoftVersionTool.GetVersion("LusterMotion.exe");
            _cacheManager = cacheManager;
            _errorMangaer = errorManager;
            IsFirstLoad = true;
            _visionAPI = visionAPI;
        }

        private static bool IsHiveIgnoreAlarm = false;


        public override string GetSystem()
        {
            return "Hive";
        }

        //1.状态变化，发送设备状态
        //2.Error产生，发送error/tossing
        //3.出料时，发送步骤信息
        //4.手动点击维修时，发送信息
        //5.软件加载时，发送版本信息
        //6.测试流程


        /// <summary>
        /// 设备状态改变自动发送hive
        /// 1.状态变更,状态有1/2/5
        /// </summary>
        /// <param name="mController"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public override void StatusChanged(IMotionController mController, EngineStatus src, EngineStatus dst)
        {
            //报警的同时会触发状态变化
            string errCode = "";
            string errMsg = "";
            if (IsFirstLoad)
            {
                IsFirstLoad = false;
                Register();
            }
            if (dst == EngineStatus.Running)
            {
                statusNow = "running";
            }
            else
            {
                statusNow = "idle";
            }
            //if (!PDCAEnable()||CPKEnable()) return;
            if (dst == EngineStatus.Alarm && mController.AlarmInfo != null && mController.AlarmInfo.Message.Contains("&"))
            {
                var err = _errorMangaer.GetErrorDetail(mController.AlarmInfo, GetSystem());
                errCode = err.Code;
                var errMsg1 = err.Message.Split('&');
                errMsg = errMsg1[1];
            }
            //MachineStatusChange(errCode, errMsg, src, dst); // 取消设备状态改变自动发送hive状态
        }

        public bool SendStausChange(int status, string errorMessage, string alarmCode, RepairState dstRepairState, string cardID = "",
        string downCause = "", string actionTaken = "", string spareParts = "", string rating = "")
        {
            string url = Path.Combine(URL, "capture/v6/machinestate");
            string urlSim = Path.Combine(URLSimulator, "capture/v6/machinestate");
            var statusMessage = "";
            bool isChainStep = false;

            // 单独处理ReportEnd状态，不伴随HiveMachineState状态变更
            if (!string.IsNullOrEmpty(downCause))
            {
                // 不能SaveHiveState()，因为不更新HiveMachineState
                statusMessage = "report end";
                // 发送数据后，标记链路完成
                int rat = 0;
                int.TryParse(rating, out rat);
                string actualEventID = "";
                // hive reportEnd弹窗是否打开
                if (string.IsNullOrEmpty(lastEventID))
                {
                    actualEventID = currentEventID;
                }
                else
                {
                    actualEventID = lastEventID;
                    lastEventID = "";
                    _hiveState.LastEventId = lastEventID;
                    // 单独刷新下LastEventId，不改变其他字段
                    SaveHiveState();
                }
                // Hive3.2，周/月保养强制使用固定的 downtime_cause
                if (_hiveState.RepairStartAlarmCode == "F99OOOO-05" || _hiveState.RepairStartAlarmCode == "F99OOOO-06")
                {
                    downCause = "Preventive Maintenance;设备保养";
                }
                var statusData = new
                {
                    machine_state = status,
                    state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                    data = new
                    {
                        downtime_cause = downCause,
                        action_taken = actionTaken,
                        spare_parts = spareParts,
                        rating = rat,
                        status = statusMessage,
                        event_id = actualEventID
                    },
                    station_info = new
                    {
                        machine_sn = stationInfo.machine_sn,
                        machine_model_number = stationInfo.machine_model_number,
                        machine_rfid = stationInfo.machine_rifd,
                        vendor = stationInfo.vendor,
                        site = stationInfo.site,
                        building = stationInfo.building,
                        product = stationInfo.product,
                        station_type = stationInfo.station_type,
                        line = stationInfo.line,
                        instance = stationInfo.instance,
                        line_type = stationInfo.line_type,
                        station_name = stationInfo.station_name,
                        build_type = stationInfo.build_type,
                    }
                };
                string jsonData = JsonTool.ToJson(statusData);
                _ = SendAsync(urlSim, jsonData, 5);
                _ = SendAsync(url, jsonData, 5, ok =>
                {
                    if (ok && !string.IsNullOrEmpty(actualEventID) && statusMessage == "report end")
                    {
                        MarkChain(actualEventID, RepairState.ReportEnd, DateTime.Now);
                    }
                }, enableLog: true);
                return true;
            }

            // 用 switch 重写状态推进逻辑，计算 statusMessage 并更新 _hiveState
            switch (_hiveState.HiveMachineState)
            {
                case 0:
                    switch (dstRepairState)
                    {
                        case RepairState.HelpRequest:
                            statusMessage = "help request";
                            break;
                        case RepairState.RepairStart:
                            statusMessage = "repair start";
                            break;
                        case RepairState.RepairEnd:
                            statusMessage = "repair end";
                            break;
                        case RepairState.ReportEnd:
                            statusMessage = "report end";
                            break;
                        default:
                            if (!string.IsNullOrEmpty(downCause)) statusMessage = "report end";
                            break;
                    }
                    _hiveState.HiveMachineState = status;
                    _hiveState.HiveRepairState = dstRepairState;
                    _hiveState.HiveCurrentCode = alarmCode;
                    break;

                case 1: // Running
                    switch (status)
                    {
                        case 2: // Idle
                            _hiveState.HiveMachineState = 2;
                            // _hiveState.CurrentEventId = ""; // 可以清空，但不影响
                            _visionAPI.MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Idle, "", "");
                            break;

                        case 5: // Down
                            if (dstRepairState == RepairState.HelpRequest)
                            {
                                _hiveState.HiveMachineState = 5;
                                _hiveState.HiveRepairState = dstRepairState;
                                _hiveState.HiveCurrentCode = alarmCode;
                                statusMessage = "help request";
                                _visionAPI.MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Down, alarmCode, errorMessage);
                            }
                            else
                            {
                                return true;
                            }
                            break;

                        case 1: // Running
                            //if (!string.IsNullOrEmpty(downCause))
                            //{
                            //    if (currentEventID != "")
                            //    {
                            //        _hiveState.HiveMachineState = 1;
                            //        _hiveState.HiveRepairState = dstRepairState;
                            //        statusMessage = "report end";
                            //    }
                            //    else
                            //    {
                            //        _hiveState.HiveMachineState = 1;
                            //        _hiveState.HiveRepairState = dstRepairState;
                            //        statusMessage = "";
                            //    }
                            //}
                            if (dstRepairState == RepairState.Normal)
                            {
                                // 1->1且不是ReportEnd，软件在运行中被强制关闭后重启，启动刷卡。
                                // 但是HiveSimulator反馈，status字段取值必须为repair/report end或者这个字段不存在。
                                // 不可以存在字段但取值为空。先改回repair end。
                                _hiveState.HiveMachineState = 1;
                                _hiveState.HiveRepairState = dstRepairState;
                                statusMessage = "repair end"; // repair end
                                currentEventID = "";
                                _hiveState.CurrentEventId = currentEventID;
                            }
                            else
                            {
                                // 非预期状态步进
                                return true;
                            }
                            break;

                        default:
                            return true;
                    }
                    break;

                case 2: // Idle
                    switch (status)
                    {
                        case 1: // Idle->Running（单独立即发送不带data）
                            _hiveState.HiveMachineState = 1;
                            var statusData0 = new
                            {
                                machine_state = status,
                                state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                                data = new { },
                                station_info = new
                                {
                                    machine_sn = stationInfo.machine_sn,
                                    machine_model_number = stationInfo.machine_model_number,
                                    machine_rfid = stationInfo.machine_rifd,
                                    vendor = stationInfo.vendor,
                                    site = stationInfo.site,
                                    building = stationInfo.building,
                                    product = stationInfo.product,
                                    station_type = stationInfo.station_type,
                                    line = stationInfo.line,
                                    instance = stationInfo.instance,
                                    line_type = stationInfo.line_type,
                                    station_name = stationInfo.station_name,
                                    build_type = stationInfo.build_type,
                                }
                            };
                            string jsonData0 = JsonTool.ToJson(statusData0);
                            _ = SendAsync(url, jsonData0, 5, enableLog: true);
                            _ = SendAsync(urlSim, jsonData0, 5);
                            SaveHiveState();
                            _visionAPI.MachineStatusUpload(TrainRunMode.Idle, TrainRunMode.Running, "", "");
                            return true;

                        case 5: // Idle->Down（HelpRequest）
                            if (dstRepairState == RepairState.HelpRequest)
                            {
                                _hiveState.HiveMachineState = 5;
                                _hiveState.HiveRepairState = dstRepairState;
                                _hiveState.HiveCurrentCode = alarmCode;
                                statusMessage = "help request";
                                _visionAPI.MachineStatusUpload(TrainRunMode.Idle, TrainRunMode.Down, alarmCode, errorMessage);
                            }
                            else
                            {
                                return true;
                            }
                            break;

                        default:
                            return true;
                    }
                    break;

                case 5: // Down
                    switch (status)
                    {
                        case 2: // Down->Idle
                            //break;
                            return true;
                        case 1: // Down->Running
                            if (_hiveState.HiveRepairState == RepairState.HelpRequest)
                            {
                                // 自动DT无需维修：help request后直接恢复
                                _hiveState.HiveMachineState = status;
                                _hiveState.HiveRepairState = RepairState.Normal;
                                _hiveState.HiveCurrentCode = "";
                                statusMessage = "";
                                cardID = "";
                                _visionAPI.MachineStatusUpload(TrainRunMode.Down, TrainRunMode.Running, "", "");
                            }
                            else if (_hiveState.HiveRepairState == RepairState.RepairStart && dstRepairState == RepairState.Normal)
                            {
                                if (mController.MachineStatus != EngineStatus.Ready && mController.MachineStatus != EngineStatus.Pause && mController.MachineStatus != EngineStatus.Running)
                                {
                                    return true;
                                }
                                _hiveState.HiveMachineState = status;
                                _hiveState.HiveRepairState = RepairState.Normal;
                                _hiveState.HiveCurrentCode = "";
                                statusMessage = "repair end";
                                _visionAPI.MachineStatusUpload(TrainRunMode.Down, TrainRunMode.Running, "", "");
                            }
                            else
                            {
                                return true;
                            }
                            break;

                        case 5: // Down内部repair start
                            if (_hiveState.HiveRepairState == RepairState.HelpRequest && dstRepairState == RepairState.RepairStart)
                            {
                                _hiveState.HiveMachineState = 5;
                                _hiveState.HiveRepairState = dstRepairState;
                                _hiveState.HiveCurrentCode = alarmCode;
                                _hiveState.RepairStartAlarmCode = alarmCode; // 记录repair start的报警码
                                statusMessage = "repair start";
                            }
                            else
                            {
                                return true;
                            }
                            break;

                        default:
                            return true;
                    }
                    break;

                default:
                    return true;
            }
            //保证event id在链路结束前不会被更新（如果这边截住则不会更新）
            // 是否链路步骤
            //if (!string.IsNullOrEmpty(statusMessage))
            //{
            //    var sm = statusMessage.ToLowerInvariant();
            //    isChainStep = sm == "help request" || sm == "repair start" || sm == "repair end" || sm == "report end";
            //}

            // 自动DT（无需维修）豁免：help request → 蓝色按钮 → state 1/2 
            bool isDirectRecoverFromHelp =
                (_hiveState.HiveRepairState == RepairState.Normal) &&
                (status == 1 || status == 2); // && string.IsNullOrEmpty(statusMessage);

            // 非链路步骤且存在未完成维修链时阻断，特例除外
            //if (!isChainStep && HasOpenChain() && !isDirectRecoverFromHelp)
            //{
            //    LogTool.Warn("存在未完成的维修链（repair end未发送成功），阻止非链路步骤的machinestate发送。");
            //    return true;
            //}

            SaveHiveState();

            // 发报文 + 链路打点（保持原分支结构）
            if (!string.IsNullOrEmpty(cardID))
            {
                if (status == 1 || status == 2)
                {
                    int rat = 0;
                    int.TryParse(rating, out rat);
                    var statusData = new
                    {
                        machine_state = status,
                        state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                        data = new
                        {
                            status = statusMessage,
                            badge = cardID,
                            event_id = currentEventID
                        },
                        station_info = new
                        {
                            machine_sn = stationInfo.machine_sn,
                            machine_model_number = stationInfo.machine_model_number,
                            machine_rfid = stationInfo.machine_rifd,
                            vendor = stationInfo.vendor,
                            site = stationInfo.site,
                            building = stationInfo.building,
                            product = stationInfo.product,
                            station_type = stationInfo.station_type,
                            line = stationInfo.line,
                            instance = stationInfo.instance,
                            line_type = stationInfo.line_type,
                            station_name = stationInfo.station_name,
                            build_type = stationInfo.build_type,
                        }
                    };
                    string jsonData = JsonTool.ToJson(statusData);
                    _ = SendAsync(urlSim, jsonData, 5);
                    var eid = currentEventID;
                    var sm = statusMessage;
                    _ = SendAsync(url, jsonData, 5, ok =>
                    {
                        if (ok && !string.IsNullOrEmpty(eid))
                        {
                            var now = DateTime.Now;
                            switch (sm)
                            {
                                case "repair end":
                                    MarkChain(eid, RepairState.RepairEnd, now);
                                    break;
                                case "report end":
                                    MarkChain(eid, RepairState.ReportEnd, now);
                                    break;
                            }
                        }
                    }, enableLog: true);
                    return true;
                }
                else
                {
                    var statusData = new
                    {
                        machine_state = status,
                        state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                        data = new
                        {
                            error_message = string.IsNullOrEmpty(errorMessage) ? "" : errorMessage,
                            code = string.IsNullOrEmpty(alarmCode) ? "" : alarmCode,
                            status = statusMessage,
                            badge = cardID,
                            event_id = currentEventID
                        },
                        station_info = new
                        {
                            machine_sn = stationInfo.machine_sn,
                            machine_model_number = stationInfo.machine_model_number,
                            machine_rfid = stationInfo.machine_rifd,
                            vendor = stationInfo.vendor,
                            site = stationInfo.site,
                            building = stationInfo.building,
                            product = stationInfo.product,
                            station_type = stationInfo.station_type,
                            line = stationInfo.line,
                            instance = stationInfo.instance,
                            line_type = stationInfo.line_type,
                            station_name = stationInfo.station_name,
                            build_type = stationInfo.build_type,
                        }
                    };
                    string jsonData = JsonTool.ToJson(statusData);
                    _ = SendAsync(urlSim, jsonData, 5);
                    var eid = currentEventID;
                    var sm = statusMessage;
                    _ = SendAsync(url, jsonData, 5, ok =>
                    {
                        if (ok && !string.IsNullOrEmpty(eid))
                        {
                            var now = DateTime.Now;
                            switch (sm)
                            {
                                case "help request":
                                    MarkChain(eid, RepairState.HelpRequest, now);
                                    break;
                                case "repair start":
                                    MarkChain(eid, RepairState.RepairStart, now);
                                    break;
                            }
                        }
                    }, enableLog: true);
                    return true;
                }
            }
            else
            {
                if (status == 1) //  || status == 2
                {
                    var statusData = new
                    {
                        machine_state = status,
                        state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                        data = new
                        {
                            event_id = currentEventID
                        },
                        station_info = new
                        {
                            machine_sn = stationInfo.machine_sn,
                            machine_model_number = stationInfo.machine_model_number,
                            machine_rfid = stationInfo.machine_rifd,
                            vendor = stationInfo.vendor,
                            site = stationInfo.site,
                            building = stationInfo.building,
                            product = stationInfo.product,
                            station_type = stationInfo.station_type,
                            line = stationInfo.line,
                            instance = stationInfo.instance,
                            line_type = stationInfo.line_type,
                            station_name = stationInfo.station_name,
                            build_type = stationInfo.build_type,
                        }
                    };
                    string jsonData = JsonTool.ToJson(statusData);
                    _ = SendAsync(urlSim, jsonData, 5);
                    var eid = currentEventID;
                    var sm = statusMessage;
                    var isDirect = isDirectRecoverFromHelp;
                    _ = SendAsync(url, jsonData, 5, ok =>
                    {
                        if (ok && !string.IsNullOrEmpty(eid))
                        {
                            var now = DateTime.Now;
                            if (sm == "repair end")
                            {
                                MarkChain(eid, RepairState.RepairEnd, now);
                            }
                            else if (isDirect)
                            {
                                // 特例闭合：help request后直接恢复
                                MarkChain(eid, RepairState.RepairEnd, now);
                                _hiveState.HiveRepairState = RepairState.Normal;
                                _hiveState.HiveCurrentCode = "";
                                SaveHiveState();
                            }
                        }
                    }, enableLog: true);
                    return true;
                }
                // 90s无料Running切idle，不上传event_id
                else if (status == 2)
                {
                    var statusData = new
                    {
                        machine_state = status,
                        state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                        data = new
                        {
                            //event_id = currentEventID
                        },
                        station_info = new
                        {
                            machine_sn = stationInfo.machine_sn,
                            machine_model_number = stationInfo.machine_model_number,
                            machine_rfid = stationInfo.machine_rifd,
                            vendor = stationInfo.vendor,
                            site = stationInfo.site,
                            building = stationInfo.building,
                            product = stationInfo.product,
                            station_type = stationInfo.station_type,
                            line = stationInfo.line,
                            instance = stationInfo.instance,
                            line_type = stationInfo.line_type,
                            station_name = stationInfo.station_name,
                            build_type = stationInfo.build_type,
                        }
                    };
                    string jsonData = JsonTool.ToJson(statusData);
                    _ = SendAsync(urlSim, jsonData, 5);
                    _ = SendAsync(url, jsonData, 5, enableLog: true);
                    return true;
                }
                else
                {
                    if (Dialog2Part2_Opend)
                    {
                        lastEventID = currentEventID;
                        _hiveState.LastEventId = lastEventID;
                    }
                    // 新DT事件：生成 event_id（help request）
                    currentEventID = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800");
                    _hiveState.CurrentEventId = currentEventID;
                    SaveHiveState();

                    var statusData = new
                    {
                        machine_state = status,
                        state_change_time = currentEventID,
                        data = new
                        {
                            error_message = string.IsNullOrEmpty(errorMessage) ? "" : errorMessage,
                            code = string.IsNullOrEmpty(alarmCode) ? "" : alarmCode,
                            status = statusMessage,
                            event_id = currentEventID
                        },
                        station_info = new
                        {
                            machine_sn = stationInfo.machine_sn,
                            machine_model_number = stationInfo.machine_model_number,
                            machine_rfid = stationInfo.machine_rifd,
                            vendor = stationInfo.vendor,
                            site = stationInfo.site,
                            building = stationInfo.building,
                            product = stationInfo.product,
                            station_type = stationInfo.station_type,
                            line = stationInfo.line,
                            instance = stationInfo.instance,
                            line_type = stationInfo.line_type,
                            station_name = stationInfo.station_name,
                            build_type = stationInfo.build_type,
                        }
                    };
                    string jsonData = JsonTool.ToJson(statusData);
                    _ = SendAsync(urlSim, jsonData, 5);
                    var eid = currentEventID;
                    var sm = statusMessage;
                    _ = SendAsync(url, jsonData, 5, ok =>
                    {
                        if (ok && !string.IsNullOrEmpty(eid) && sm == "help request")
                        {
                            MarkChain(eid, RepairState.HelpRequest, DateTime.Now);
                        }
                    }, enableLog: true);
                    return true;
                }
            }
        }

        private object locker = new object();
        //1.1
        /// <summary>
        /// 状态切换，用于点击停止/暂停/启动按钮
        /// </summary>
        /// <param name="alarmCode"></param>
        /// <param name="alarmInfo"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public bool MachineStatusChange(string alarmCode, string alarmInfo, EngineStatus src, EngineStatus dst, bool IsQulification = false, string cardID = "")
        {
            lock (locker)
            {
                int sendState;
                string sendMessage = "";
                string sendCode = "";
                RepairState sendRepairState = RepairState.Normal;

                if (dst == EngineStatus.Running)
                {
                    statusNow = "running";
                }
                else
                {
                    statusNow = "idle";
                }
                base.StatusChanged(mController, src, dst);
                // 如果没有连接上，不触发通讯
                //if ((!isConnected)) return false;
                //string url = Path.Combine(URL, "v5/capture/machinestate");
                string url = Path.Combine(URL, "capture/v6/machinestate");
                //自动运行时，发送1
                if (dst == EngineStatus.Running)
                {
                    latestStatus = 1;

                    if (isShieldDoor && sysConfig.IsShieldDoor)
                    {
                        isShieldDoor = false;
                        LogTool.Debug("安全门屏蔽hive，alarm-run", "安全门屏蔽hive");
                        return true;
                    }
                }
                //空闲，发送2
                else if (dst == EngineStatus.Idle || dst == EngineStatus.Pause)
                {
                    latestStatus = 2;

                }
                //报警，发送5
                else if (dst == EngineStatus.Alarm /*|| dst == EngineStatus.Pause*/)
                {
                    latestStatus = 5;
                    if (alarmInfo.Contains("门") && sysConfig.VisionStationId.Contains("FXZZ") && sysConfig.IsShieldDoor)
                    {
                        LogTool.Debug("安全门屏蔽hive，run-alarm", "安全门屏蔽hive");
                        isShieldDoor = true;
                        return true;
                    }
                }
                //如过暂停，直接return
                if (dst == EngineStatus.Pause) return true;

                //状态不同时，上传
                if (curStatus != latestStatus)
                {
                    curStatus = latestStatus;
                    //构建发送数据
                    //格式应该有4种
                    //1.Running/Idle
                    //2.Alarm触发
                    //3.Alarm维修开始
                    //4.Alarm维修结束,放到EndAlarm函数中
                    //默认1个其他异常
                    var statusData = new Object();
                    if (curStatus != 5)
                    {
                        ///维修中，不再发状态
                        if (_hiveState.IsRepairing) return true;
                        //构建发送数据
                        statusData = new
                        {
                            machine_state = curStatus,
                            state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                            data =
                            new
                            {
                                //无需新增series和parallel字段，CG线按单工站逻辑上传
                                //series = seque,
                            },
                            station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                                   station_name = stationInfo.station_name,
                                   build_type = stationInfo.build_type,
                               }
                        };
                        sendState = curStatus;
                        sendMessage = "";
                        sendCode = "";
                        sendRepairState = RepairState.Normal;
                    }
                    else if (string.IsNullOrEmpty(cardID))
                    {
                        statusData = new
                        {
                            machine_state = curStatus,
                            state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                            data =
                           new
                           {
                               error_message = (string.IsNullOrEmpty(alarmInfo) && curStatus == 5) ? "Stopped for repair" : alarmInfo,
                               code = (string.IsNullOrEmpty(alarmCode) && curStatus == 5) ? "F99OOOO-01" : alarmCode,
                               status = "help request",
                           },
                            station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                                   station_name = stationInfo.station_name,
                                   build_type = stationInfo.build_type,
                               }
                        };
                        sendState = curStatus;
                        sendMessage = (string.IsNullOrEmpty(alarmInfo) && curStatus == 5) ? "Stopped for repair" : alarmInfo;
                        sendCode = (string.IsNullOrEmpty(alarmCode) && curStatus == 5) ? "F99OOOO-01" : alarmCode;
                        sendRepairState = RepairState.HelpRequest;
                    }
                    else
                    {
                        statusData = new
                        {
                            machine_state = curStatus,
                            state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                            data =
                           new
                           {
                               error_message = (string.IsNullOrEmpty(alarmInfo) && curStatus == 5) ? "Stopped for repair" : alarmInfo,
                               code = (string.IsNullOrEmpty(alarmCode) && curStatus == 5) ? "F99OOOO-01" : alarmCode,
                               status = "repair start",
                               badge = cardID
                           },
                            station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                                   station_name = stationInfo.station_name,
                                   build_type = stationInfo.build_type,
                               }
                        };
                        sendState = curStatus;
                        sendMessage = (string.IsNullOrEmpty(alarmInfo) && curStatus == 5) ? "Stopped for repair" : alarmInfo;
                        sendCode = (string.IsNullOrEmpty(alarmCode) && curStatus == 5) ? "F99OOOO-01" : alarmCode;
                        sendRepairState = RepairState.RepairStart;
                    }
                    /*string jsonData = JsonTool.ToJson(statusData);
                    return Send(url, jsonData, 5);*/
                    //自动设备状态检测流程中的Hive状态发送
                    //if (!Dialog2Part2_Opend)
                    return SendStausChange(sendState, sendMessage, sendCode, sendRepairState);
                }
                return true;
            }
        }

        protected override void MController_StatusEvent(IMotionController mController, TrainRunMode trainRunMode, string reason)
        {
            base.MController_StatusEvent(mController, trainRunMode, reason);
            //StatusChanged(mController, EngineStatus.Running, EngineStatus.Idle);
        }

        /// <summary>
        /// 2.Error产生
        /// </summary>
        /// <param name="alarmInfo"></param>
        protected override void AlarmProcEvent(AlarmInfo alarmInfo, bool isOK = true)
        {

        }

        /// <summary>
        /// 2.1 主要用于抛料（抛主料和抛小料）
        /// </summary>
        /// <param name="errType">error or tossing</param>
        /// <param name="errCode"></param>
        /// <param name="errInfo"></param>
        private void AlarmEvent(string errType, string errCode, string errInfo)
        {
            //主料:retry;小料:tossing;
            string currentSeverity = errType;
            // 如果没有连接上，不触发通讯
            if (!isConnected || !PDCAEnable() || CPKEnable()) return;
            //string url = Path.Combine(URL, "v5/capture/errordata");
            string url = Path.Combine(URL, "capture/v6/errordata");
            string urlSim = Path.Combine(URLSimulator, "capture/v6/errordata");
            //构建发送数据
            if (string.IsNullOrEmpty(errCode))
            {
                errCode = "N03OOOO-01";
                errInfo = "CG NG";
            }
            else if (!errCode.Contains("-"))
            {
                errCode = "N03OOOO-01";
                errInfo = "CG NG";
            }
            var errorData = new
            {
                message = string.IsNullOrEmpty(errInfo) ? "Stopped for repair" : errInfo,
                //sequence = sysConfig.UniteCode,
                code = string.IsNullOrEmpty(errCode) ? "F99OOOO-01" : errCode,
                severity = currentSeverity,
                occurrence_time = DateTime.Now.AddMilliseconds(-100).ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                resolved_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                    //sub_station = sysConfig.UniteCode,
                    // 20251021，更改sequence关键字为series
                    // CG线按单工站逻辑即可
                    // series = seque,   //sequence = seque,
                },
                station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                                   station_name = stationInfo.station_name,
                                   build_type = stationInfo.build_type,
                               }
            };
            string jsonData = JsonTool.ToJson(errorData);
            _ = SendAsync(url, jsonData, 5, enableLog: true);
            _ = SendAsync(urlSim, jsonData, 5);
        }

        /// <summary>
        /// 3.出料信息
        /// </summary>
        /// <param name="info"></param>
        protected override void MController_ProductEvent(ProductInfo productInfo)
        {
            base.MController_ProductEvent(productInfo);

            //出料事件不再触发cycletime上传-20250714
            if (true) return;
            // 如果没有连接上，不触发通讯
            if (!isConnected || !PDCAEnable() || !productInfo.Result.Result || CPKEnable()) return;
            //string url = Path.Combine(URL, "v5/capture/machinedata");
            string url = Path.Combine(URL, "capture/v6/machinedata");
            string wip = "N999999999";

            if (!productInfo.Result.Datas.ContainsKey("WIP"))
            {
                var cacheItem = _cacheManager.GetItem(productInfo.Result.ProCode);
                if (cacheItem != null && cacheItem is CacheItem cItem)
                {
                    bool isExist = cItem.Any(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                    if (isExist)
                    {
                        var wipItem = cItem.FirstOrDefault(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                        if (wipItem.Value != null)
                        {
                            wip = wipItem.Value.ToString();
                        }
                    }
                }
            }
            else
            {
                wip = productInfo.Result.Datas["WIP"].ToString();
            }
            //构建发送数据
            var errorData = new
            {
                unit_sn = wip,
                sequence = seque,
                serials = new { },
                pass = productInfo.Result.Result,
                input_time = productInfo.EnterTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                output_time = productInfo.OutTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                    //sub_station = sysConfig.UniteCode,
                },
                station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                                   station_name = stationInfo.station_name,
                                   build_type = stationInfo.build_type,
                               }
            };
            string jsonData = JsonTool.ToJson(errorData);
            LogTool.Debug("hive产品接口上传" + productInfo.Result.IsPreviousStationUndo.ToString(), "visionlog记录");
            if (!productInfo.Result.IsPreviousStationUndo && statusNow == "running" && statusNow != "idle")
            {
                _ = SendAsync(url, jsonData, 5, enableLog: true);
            }
        }

        protected override void MController_StationEvent(string WIP, string InputTime, string OutputTime, bool Result)
        {
            base.MController_StationEvent(WIP, InputTime, OutputTime, Result);
            // 如果没有连接上，不触发通讯
            if (!isConnected || !Result)
            {
                LogTool.Warn("通讯异常或PDCA未启用，无法上传生产数据到Hive！");
                return;
            }
            //string url = Path.Combine(URL, "v5/capture/machinedata");
            string url = Path.Combine(URL, "capture/v6/machinedata");
            string urlSim = Path.Combine(URLSimulator, "capture/v6/machinedata");
            // 20251021 生产数据接口新增mode字段
            int modeCur = -1;
            if (PDCAEnable() && !CPKEnable() && !GRREnable() && !DryRunEnable()) modeCur = 0;
            else if (PDCAEnable() && CPKEnable() && !GRREnable() && !DryRunEnable()) modeCur = 1;
            else if (PDCAEnable() && !CPKEnable() && GRREnable() && !DryRunEnable()) modeCur = 2;
            else if (!PDCAEnable() && !CPKEnable() && !GRREnable() && DryRunEnable()) modeCur = 5;
            else modeCur = 6;

            //构建发送数据
            var sendData = new
            {
                unit_sn = WIP,
                serials = new { },
                pass = Result,
                input_time = InputTime,
                output_time = OutputTime,
                data = new
                {
                    //sub_station = sysConfig.UniteCode,
                    // CG线按单工站逻辑即可
                    //series = seque, //sequence = seque,
                    mode = modeCur,
                },
                station_info =
                                   new
                                   {
                                       machine_sn = stationInfo.machine_sn,
                                       machine_model_number = stationInfo.machine_model_number,
                                       machine_rfid = stationInfo.machine_rifd,
                                       vendor = stationInfo.vendor,
                                       site = stationInfo.site,
                                       building = stationInfo.building,
                                       product = stationInfo.product,
                                       station_type = stationInfo.station_type,
                                       line = stationInfo.line,
                                       instance = stationInfo.instance,
                                       line_type = stationInfo.line_type,
                                       station_name = stationInfo.station_name,
                                       build_type = stationInfo.build_type,
                                   }
            };
            string jsonData = JsonTool.ToJson(sendData);
            //if ( statusNow == "running" && statusNow != "idle")
            //{
            _ = SendAsync(url, jsonData, 5, enableLog: true);
            _ = SendAsync(urlSim, jsonData, 5);
            //}

        }



        #region Hive 2025新增方法
        public void HiveManualStart(string cmdStr, string cardId)
        {
            string alarmCode = "";
            string alarmMsg = "";
            switch (cmdStr)
            {
                case "一级供应商":
                    alarmCode = "F99OOOO-01";
                    alarmMsg = "Stopped for repair by 1st tier vendor";
                    break;

                case "一+二级供应商":
                    alarmCode = "F99OOOO-02";
                    alarmMsg = "Stopped for repair by 2 vendors";
                    break;

                case "凌云光":
                    alarmCode = "F99OOOO-03";
                    alarmMsg = "Stopped for repair by Cognex";
                    break;

                case "汇川":
                    alarmCode = "F99OOOO-04";
                    alarmMsg = "Stopped for repair by Keyence";
                    break;

                case "凌云光维修":
                    alarmCode = "F99OOOO-12";
                    alarmMsg = "Stopped for repair by Luster";
                    break;

                case "工厂人员":
                    alarmCode = "F99OOOO-11";
                    alarmMsg = "Stopped for repair by contract manufacturer";
                    break;

                case "耗材/物料补充":

                    alarmCode = "F99OOOO-08";
                    alarmMsg = "Stopped for Consumable/material replenishment";
                    break;

                case "非生产时间调试":

                    alarmCode = "F99OOOO-09";
                    //alarmMsg = "Stopped for Consumable/material replenishment";
                    alarmMsg = "Fine-tuning during non-production time";
                    break;

                case "周保养":

                    alarmCode = "F99OOOO-05";
                    alarmMsg = "Stopped for Weekly Maintenance";
                    break;

                case "月保养":

                    alarmCode = "F99OOOO-06";
                    alarmMsg = "Stopped for Monthly Maintenance";
                    break;

                case "计划关机":

                    alarmCode = "F99OOOO-07";
                    alarmMsg = "Planned shutdown";
                    break;

                case "记录非停机异常":

                    alarmCode = "F99OOOO-10";
                    alarmMsg = "Degraded mode";
                    break;

                default:

                    alarmCode = "F99OOOO-20";
                    alarmMsg = "Machine stopped through UI";
                    break;
            }

            ////string url = Path.Combine(URL, "v5/capture/machinestate");
            //string url = Path.Combine(URL, "capture/v6/machinestate");

            ////构建发送数据
            //var statusData = new
            //{
            //    machine_state = 5,
            //    state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
            //    data =
            //    new
            //    {
            //        error_message = alarmMsg,
            //        code = alarmCode,
            //        status = "repair start",
            //        badge = cardId
            //    },
            //    station_info =
            //                   new
            //                   {
            //                       machine_sn = stationInfo.machine_sn,
            //                       machine_model_number = stationInfo.machine_model_number,
            //                       machine_rfid = stationInfo.machine_rifd,
            //                       vendor = stationInfo.vendor,
            //                       site = stationInfo.site,
            //                       building = stationInfo.building,
            //                       product = stationInfo.product,
            //                       station_type = stationInfo.station_type,
            //                       line = stationInfo.line,
            //                       instance = stationInfo.instance,
            //                       line_type = stationInfo.line_type,
            //                   }
            //};
            //string jsonData = JsonTool.ToJson(statusData);
            //Send(url, jsonData, 5);
            SendStausChange(5, alarmMsg, alarmCode, RepairState.RepairStart, cardId);
        }


        bool hasSendHelpRequest = false;

        public void HelpRequest(string errorMessage, string code)
        {
            lock (locker)
            {
                //YQS
                if (string.IsNullOrEmpty(URL))
                {
                    return;
                }

                /*if(hasSendHelpRequest)
                {
                    return;
                }*/

                SetIsRepairing(true);

                //string url = Path.Combine(URL, "v5/capture/machinestate");
                string url = Path.Combine(URL, "capture/v6/machinestate");

                SendStausChange(5, errorMessage, code, RepairState.HelpRequest);
            }
        }


        public void RepairStart(string errorMessage, string code, string cardId)
        {     //YQS
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }
            //string url = Path.Combine(URL, "v5/capture/machinestate");
            string url = Path.Combine(URL, "capture/v6/machinestate");

            //构建发送数据
            var statusData = new
            {
                machine_state = 5,
                state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data =
                new
                {
                    error_message = errorMessage,
                    code = code,
                    status = "repair start",
                    badge = cardId,
                    event_id = currentEventID
                },
                station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                               }
            };
            //string jsonData = JsonTool.ToJson(statusData);
            //Send(url, jsonData, 5);
            SendStausChange(5, errorMessage, code, RepairState.RepairStart, cardId);
        }


        /// <summary>
        /// 清除alarm,直接启动
        /// </summary>
        public void ClearAlarm()
        {
            //YQS
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }
            //string url = Path.Combine(URL, "v5/capture/machinestate");
            string url = Path.Combine(URL, "capture/v6/machinestate");
            SetIsRepairing(false);
            //正常运行过程中，触发异常弹窗，设备暂停，手动复位后，点击“清楚报警”按钮，
            //请求发送状态为1，交由最后的状态机判断是否应该发送
            SendStausChange(1, "", "", RepairState.RepairEnd);
            //构建发送数据
            /*var statusData = new
            {
                machine_state = 1,
                state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data =
                new
                {
                },
            };
            string jsonData = JsonTool.ToJson(statusData);
            Send(url, jsonData, 5);*/
        }


        /// <summary>
        /// 用于结束hive alarm
        /// </summary>
        /// <param name="downCause"></param>
        /// <param name="actionTaken"></param>
        /// <param name="spareParts"></param>
        /// <param name="badge"></param>
        /// <param name="rating"></param>
        public void EndAlarm(string downCause, string actionTaken, string spareParts, string badge, string rating)
        {     //YQS
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }

            lock (locker)
            {
                //string url = Path.Combine(URL, "v5/capture/machinestate");
                string url = Path.Combine(URL, "capture/v6/machinestate");


                //如果是非停机异常，缺乏显示内容，自动补充
                if (string.IsNullOrEmpty(spareParts))
                {
                    spareParts = "999;Others;其他";
                }
                if (string.IsNullOrEmpty(downCause))
                {
                    downCause = "Others;其他";
                }
                downCause = downCause.Trim();
                spareParts = spareParts.Trim();
                //构建发送数据
                var statusData = new
                {
                    machine_state = 1,
                    state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                    data =
                    new
                    {
                        status = "repair end",
                        downtime_cause = downCause,
                        action_taken = actionTaken,
                        spare_parts = spareParts,
                        badge = badge,
                        rating = rating
                    },
                    station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                               }
                };
                //string jsonData = JsonTool.ToJson(statusData);
                //Send(url, jsonData, 5);
                SendStausChange(1, "", "", RepairState.Normal, badge, downCause, actionTaken, spareParts, rating);
                SetIsRepairing(false);
                curStatus = 1;
            }
        }
        public void EndAlarm(string badge)
        {     //YQS
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }

            lock (locker)
            {
                string url = Path.Combine(URL, "capture/v6/machinestate");
                //构建发送数据
                var statusData = new
                {
                    machine_state = 1,
                    state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                    data =
                    new
                    {
                        status = "repair end",
                        badge = badge,
                        event_id = currentEventID
                    },
                    station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                               }
                };
                //string jsonData = JsonTool.ToJson(statusData);
                //Send(url, jsonData, 5);
                SendStausChange(1, "", "", RepairState.Normal, badge);
                SetIsRepairing(false);
                curStatus = 1;
            }
        }

        /// <summary>
        /// 用于结束hive alarm后的维修信息上报
        /// </summary>
        /// <param name="downCause"></param>
        /// <param name="actionTaken"></param>
        /// <param name="spareParts"></param>
        /// <param name="badge"></param>
        /// <param name="rating"></param>
        public void EndAlarmReport(string downCause, string actionTaken, string spareParts, string rating)
        {     //YQS
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }

            lock (locker)
            {
                string url = Path.Combine(URL, "capture/v6/machinestate");
                //如果是非停机异常，缺乏显示内容，自动补充
                if (string.IsNullOrEmpty(spareParts))
                {
                    spareParts = "999;Others;其他";
                }
                if (string.IsNullOrEmpty(downCause))
                {
                    downCause = "Others;其他";
                }
                downCause = downCause.Trim();
                spareParts = spareParts.Trim();
                //构建发送数据
                var statusData = new
                {
                    machine_state = 1,
                    state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                    data =
                    new
                    {
                        status = "report end",
                        downtime_cause = downCause,
                        action_taken = actionTaken,
                        spare_parts = spareParts,
                        rating = rating,
                        event_id = currentEventID
                    },
                    station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                               }
                };
                //string jsonData = JsonTool.ToJson(statusData);
                //Send(url, jsonData, 5);
                SendStausChange(1, "", "", RepairState.Normal, "", downCause, actionTaken, spareParts, rating);
                //SetIsRepairing(false);
                // ReportEnd不参与HiveState状态切换
                //curStatus = 1;
            }
        }

        #endregion


        /// <summary>
        /// 5.软件版本监控
        /// 1.运控软件版本
        /// 2.视觉软件版本
        /// 3.机器人软件版本
        /// </summary>
        public override void Register()
        {     //YQS
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }
            // 如果没有连接上，不触发通讯
            //string url = Path.Combine(URL, "capture/v5/softwareversion");
            string url = Path.Combine(URL, "capture/v6/softwareversions");
            string urlSim = Path.Combine(URLSimulator, "capture/v6/softwareversions");
            if (!isConnected /*|| !PDCAEnable()*/) return;

            //读取error code list信息
            var (errorCodeHash, errorCodeCount) = GetErrorCodeListInfo();

            //所有版本需要一起发送
            var motionSw = new
            {
                //app_id = sysConfig.HiveAppId,
                //sequence = sysConfig.UniteCode,
                //ts = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                //data = new
                //{
                //    ms_version = ApiVision,
                //    ms_sha1 = Sha1Signature(ApiVision),
                //    vs_version = sysConfig.VisionVersion,
                //    vs_sha1 = Sha1Signature(sysConfig.VisionVersion),
                //    rs_version = sysConfig.RobotVersion,
                //    rs_sha1 = Sha1Signature(sysConfig.RobotVersion),
                //},
                // 2025-5-10 实现；2025-6-16 优化
                main_software = new
                {
                    version = ApiVision,
                    hash_key = Sha1Signature(ApiVision),
                    //hashalgo = "sha1"
                },
                sub_module = new
                {
                    error_code_list = new
                    {
                        version = "1.0.0.0",
                        hash_key = errorCodeHash,
                        count = errorCodeCount,
                    },
                    //repair_sop = new
                    //{
                    //    version = repairVersion,
                    //    hash_key = Sha1Signature(repairVersion),
                    //},
                    spare_part_list = new
                    {
                        version = spareVersion,
                        hash_key = Sha1Signature(spareVersion),
                        count = 15,
                    }                   
                },
                attributes = new
                {
                },
                station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                                   station_name = stationInfo.station_name,
                                   build_type = stationInfo.build_type,
                               }
            };


            string jsonData = JsonTool.ToJson(motionSw);
            _ = SendAsync(url, jsonData, 5, specialTimeout: true, enableLog: true);
            _ = SendAsync(urlSim, jsonData, 5, specialTimeout: true);

        }

        /// <summary>
        /// 参数变更时触发软件版本信息上传
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        public void Register(string paramName, object srcV, object newV)
        {
            //Register();
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }
            // 如果没有连接上，不触发通讯
            string url = Path.Combine(URL, "capture/v6/softwareversions");
            string urlSim = Path.Combine(URLSimulator, "capture/v6/softwareversions");
            if (!isConnected /*|| !PDCAEnable()*/) return;

            //读取error code list信息
            var (errorCodeHash2, errorCodeCount2) = GetErrorCodeListInfo();

            //所有版本需要一起发送
            var motionSw = new
            {
                main_software = new
                {
                    version = ApiVision,
                    hash_key = Sha1Signature(ApiVision),
                },
                sub_module = new
                {
                    error_code_list = new
                    {
                        version = "1.0.0.0",
                        hash_key = errorCodeHash2,
                        count = errorCodeCount2,
                    },
                    spare_part_list = new
                    {
                        version = spareVersion,
                        hash_key = Sha1Signature(spareVersion),
                    }
                },
                attributes = new Dictionary<string, object>
                {
                    { paramName, new { old = srcV, @new = newV } }
                },
                station_info =
                               new
                               {
                                   machine_sn = stationInfo.machine_sn,
                                   machine_model_number = stationInfo.machine_model_number,
                                   machine_rfid = stationInfo.machine_rifd,
                                   vendor = stationInfo.vendor,
                                   site = stationInfo.site,
                                   building = stationInfo.building,
                                   product = stationInfo.product,
                                   station_type = stationInfo.station_type,
                                   line = stationInfo.line,
                                   instance = stationInfo.instance,
                                   line_type = stationInfo.line_type,
                                   station_name = stationInfo.station_name,
                                   build_type = stationInfo.build_type,
                               }
            };
            string jsonData = JsonTool.ToJson(motionSw);
            _ = SendAsync(url, jsonData, 5, specialTimeout: true, enableLog: true);
            _ = SendAsync(urlSim, jsonData, 5, specialTimeout: true);
        }
        protected override void Register(IMotionController motionController)
        {
            base.Register(motionController);

            // 产品出料
            motionController.ProductEvent -= MotionController_ProductEvent;
            motionController.ProductEvent += MotionController_ProductEvent;

            //小料触发事件
            motionController.ProductTrowEvent -= MotionControl_ThrowSmallPartEvent;
            motionController.ProductTrowEvent += MotionControl_ThrowSmallPartEvent;

            //// 参数变更，同步触发软件版本信息上传
            //motionController.PropertyChanged -= MotionController_PropertyChanged;
            //motionController.PropertyChanged += MotionController_PropertyChanged;

        }

        /// <summary>
        /// 参数变更，同步触发软件版本信息上传
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        private void MotionController_PropertyChanged(string paramName, object srcV, object newV)
        {
            try
            {
                if (paramName != null && paramName.Contains("Z轴"))
                {
                    Register(paramName, srcV, newV);
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        private void MotionController_ProductEvent(ProductInfo arg1, bool arg2, double ct)
        {
            //hive上传cycle time

            //如果主料抛料了，需要上传error
            if (arg2)
            {
                if (arg1.Result.IsToss && !arg1.Result.Result && !arg1.Result.IsPreviousStationUndo)
                {
                    AlarmEvent("retry", arg1.Result.NgCode, arg1.Result.ErrMsg);
                }
            }
        }

        private void MotionControl_ThrowSmallPartEvent(StationResult stationResult)
        {
            if (!stationResult.IsPreviousStationUndo)
            {
                AlarmEvent("tossing", stationResult.NgCode, stationResult.ErrMsg);
            }

        }


        /// <summary>
        /// 主动停止
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="op"></param>
        /// <param name="arg3"></param>
        protected override void ManualDown(object arg1, SystemOperation op, string arg3)
        {
            base.ManualDown(arg1, op, arg3);

            // 如果没有连接上，不触发通讯
            if (!isConnected || !PDCAEnable() || CPKEnable()) return;


        }

        /// <summary>
        /// Sha1 签名算法
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private string Sha1Signature(string s, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var buffer = encoding.GetBytes(s);
            var data = SHA1.Create().ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.Append(item.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成token
        /// </summary>
        /// <returns></returns>
        public string CreateToken()
        {
            var rndData = new byte[43];
            rand.NextBytes(rndData);
            var seedValue = System.Threading.Interlocked.Add(ref seed, 1);
            var seedData = BitConverter.GetBytes(seedValue);
            var tokenData = rndData.Concat(seedData).OrderBy(_ => rand.Next());
            return Convert.ToBase64String(tokenData.ToArray()).TrimEnd('=').Replace('/', '_');
        }


        /// <summary>
        /// 手动报修类型
        /// </summary>
        public enum ERepiarType
        {
            [Description("设备故障")]
            Unplaned,
            [Description("预防性维护")]
            Planed
        }

        /// <summary>
        /// 判断PDCA是否启用
        /// </summary>
        /// <returns></returns>
        private bool PDCAEnable()
        {
            // 对SFC和PDCA进行监控
            var globalModule = mController.MotionEngine.Get(GlobalModule.GlobalID);
            foreach (var item in globalModule.Parameters)
            {
                if (item.Key == PDCANAME)
                {
                    if (item.Value != null)
                    {
                        return (bool)item.Value.Value;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 判断CPK是否启用
        /// </summary>
        /// <returns></returns>
        private bool CPKEnable()
        {
            // 对SFC和PDCA进行监控
            var globalModule = mController.MotionEngine.Get(GlobalModule.GlobalID);
            foreach (var item in globalModule.Parameters)
            {
                if (item.Key == CPKNAME)
                {
                    if (item.Value != null)
                    {
                        return (bool)item.Value.Value;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 判断空跑是否启用
        /// </summary>
        /// <returns></returns>
        private bool DryRunEnable()
        {
            // 获取所有的全局变量，找到关键字DRYRUNNAME
            var globalModule = mController.MotionEngine.Get(GlobalModule.GlobalID);
            foreach (var item in globalModule.Parameters)
            {
                if (item.Key == DRYRUNNAME)
                {
                    if (item.Value != null)
                    {
                        return (bool)item.Value.Value;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 判断GRR是否启用
        /// </summary>
        /// <returns></returns>
        private bool GRREnable()
        {
            // 对SFC和PDCA进行监控
            var globalModule = mController.MotionEngine.Get(GlobalModule.GlobalID);
            foreach (var item in globalModule.Parameters)
            {
                if (item.Key == GRRNAME)
                {
                    if (item.Value != null)
                    {
                        return (bool)item.Value.Value;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 读取D:\Hive下LUSTER ERROR LIST的csv文件，取前4列内容计算hash，返回(hashKey, 数据行数)
        /// </summary>
        private (string hashKey, int count) GetErrorCodeListInfo()
        {
            try
            {
                var dir = "D:\\Hive";
                if (!Directory.Exists(dir)) return ("", 0);
                var file = Directory.GetFiles(dir, "*LUSTER ERROR LIST*").FirstOrDefault();
                if (file == null) return ("", 0);

                var lines = File.ReadAllLines(file, Encoding.UTF8);
                if (lines.Length <= 1) return ("", 0);

                var sb = new StringBuilder();
                for (int i = 1; i < lines.Length; i++) // 跳过表头
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var cols = lines[i].Split(',');
                    for (int c = 0; c < 4 && c < cols.Length; c++)
                    {
                        sb.Append(cols[c].Trim());
                    }
                }
                return (Sha1Signature(sb.ToString()), lines.Length - 1);
            }
            catch (Exception ex)
            {
                LogTool.Error($"读取ErrorCodeList失败: {ex.Message}");
                return ("", 0);
            }
        }


        /// <summary>
        /// 发送至hive，2025-11-11 负责记录除了登录状态和软件版本之外的日志
        /// </summary>
        /// <param name="times">次数</param>
        private bool Send(string url, string datas, int times, bool specialTimeout = false)
        {
            bool isSendSuccess = false;
            //string rawRes = "";
            ResultStatus resultRaw = null;
            if (sysConfig.IsHiveIgnoreFeedBack) times = 1;
            for (int i = 0; i < times; i++)
            {
                CommException(url, datas, (r) =>
                {
                    //观澜反馈，CG5返回的msg为空，导致contains报错
                    if (r?.msg == null) return;
                    //if (r.Status.Contains("Success") || r.Status.Contains("OK"))
                    if (r.msg.Contains("Success") || r.msg.Contains("OK"))
                    {
                        //heartSource?.Cancel();
                        //heartSource = new CancellationTokenSource();
                        isSendSuccess = true;
                    }
                    // 将服务器返回结果保存，用于D盘日志记录
                    resultRaw = r;
                    //if (r?.msg != null)
                    //{
                    //    // 2. 自己想看原始字符串，再发一次（几乎无性能压力）
                    //    PostAndGetRaw(url, datas, raw =>
                    //    {
                    //        //LogTool.Debug($"Hive 原始返回：{raw}");
                    //        rawRes = raw;
                    //    });
                    //}
                });
                if (isSendSuccess) break;
                Thread.Sleep(100);
            }

            // 2025-6-16 新增客户要求的Hive日志
            if (url.Contains("heartbeat"))//记录到心跳日志中
            {
                var time = DateTime.Now;
                FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/Heartbeat/{time.ToString("yyyyMMdd")}");
            }
            //增加客户要求的Hive日志
            if (url.Contains("softwareversio"))//记录到版本日志中
            {
                var time = DateTime.Now;
                FileLogger.Log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/SW Version/{time.ToString("yyyyMMdd")}");
            }
            if (url.Contains("machinedata"))//记录到版本日志中
            {
                var time = DateTime.Now;
                FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/Machine Data/{time.ToString("yyyyMMdd")}/hour_{time.Hour}");
            }
            if (url.Contains("machinestate"))
            {
                var time = DateTime.Now;
                FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/Machine State/{time.ToString("yyyyMMdd")}");
            }
            if (url.Contains("errordata"))
            {
                var time = DateTime.Now;
                FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/TR Data/{time.ToString("yyyyMMdd")}");
            }

            //跳过启动时的version报文产生报警的影响
            if (specialTimeout)
            {
                isSendSuccess = true;
            }

            if (!isSendSuccess && !sysConfig.IsHiveValveEnabled)
            {
                if (IsHiveIgnoreAlarm == false)
                {
                    IsHiveIgnoreAlarm = true;
                    mController.MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.FailError, "Hive连续发送失败，客户要求停机", "N03OOOO-02@Send Hive message failed"));
                }
            }
            return isSendSuccess;
        }

        /// <summary>
        /// 异步发送Hive报文（带回调重载），不阻塞调用线程，完成后执行回调
        /// </summary>
        private async Task<bool> SendAsync(string url, string datas, int times, Action<bool> onDone, bool specialTimeout = false, bool enableLog = false)
        {
            var result = await SendAsync(url, datas, times, specialTimeout: specialTimeout, enableLog: enableLog);
            onDone?.Invoke(result);
            return result;
        }

        private async Task<bool> SendAsync(string url, string datas, int times, bool specialTimeout = false, bool enableLog = false)
        {
            bool isSendSuccess = false;
            ResultStatus? resultRaw = null;
            if (sysConfig.IsHiveIgnoreFeedBack) times = 1;
            for (int i = 0; i < times; i++)
            {
                await CommExceptionAsync(url, datas, async (r) =>
                {
                    if (r?.msg == null) return;
                    if (r.msg.Contains("Success") || r.msg.Contains("OK"))
                    {
                        isSendSuccess = true;
                    }
                    resultRaw = r;
                });
                if (isSendSuccess) break;
                await Task.Delay(100);
            }

            // 2025-6-16 新增客户要求的Hive日志
            if (enableLog)
            {
                if (url.Contains("heartbeat"))//记录到心跳日志中
                {
                    var time = DateTime.Now;
                    FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/Heartbeat/{time.ToString("yyyyMMdd")}");
                }
                if (url.Contains("softwareversio"))//记录到版本日志中
                {
                    var time = DateTime.Now;
                    FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/SW Version/{time.ToString("yyyyMMdd")}");
                }
                if (url.Contains("machinedata"))//记录到版本日志中
                {
                    var time = DateTime.Now;
                    FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/Machine Data/{time.ToString("yyyyMMdd")}/hour_{time.Hour}");
                }
                if (url.Contains("machinestate"))
                {
                    var time = DateTime.Now;
                    FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/Machine State/{time.ToString("yyyyMMdd")}");
                }
                if (url.Contains("errordata"))
                {
                    var time = DateTime.Now;
                    FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.msg}", $"D:/Hive/Hive Log/TR Data/{time.ToString("yyyyMMdd")}");
                }
            }

            if (specialTimeout)
            {
                isSendSuccess = true;
            }

            if (!isSendSuccess && !sysConfig.IsHiveValveEnabled)
            {
                if (IsHiveIgnoreAlarm == false)
                {
                    IsHiveIgnoreAlarm = true;
                    mController.MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.FailError, "Hive连续发送失败，客户要求停机", "N03OOOO-02@Send Hive message failed"));
                }
            }
            return isSendSuccess;
        }

        //// 新增异步Post方法
        //private async Task<string> PostAndGetRawAsync(string url, string jsonData)
        //{
        //    var wt = new WebTool() { Encoding = Encoding.UTF8 };
        //    return await Task.Run(() => wt.Post(url, jsonData));
        //}

        // 新增异步CommException方法
        private async Task CommExceptionAsync(string url, string datas, Func<dynamic, Task> onResult)
        {
            await Task.Run(() =>
            {
                CommException(url, datas, r =>
                {
                    onResult(r).Wait();
                });
            });
        }

        // 2025-10-24 日志记录详细的Hive返回信息
        //private void PostAndGetRaw(string url, string jsonData, Action<string> onRaw)
        //{
        //    // 重新发一次一模一样的 POST，只拿字符串
        //    var wt = new WebTool() { Encoding = Encoding.UTF8 };
        //    string raw = wt.Post(url, jsonData);
        //    onRaw?.Invoke(raw);
        //}

        // 2025-11-11 负责记录软件版本日志
        private bool Send22(string url, string datas, int times, bool specialTimeout = false)
        {
            bool isSendSuccess = false;
            if (sysConfig.IsHiveIgnoreFeedBack) times = 1;
            //string rawRes = "";
            ResultStatus resultRaw = null;
            for (int i = 0; i < times; i++)
            {
                CommException(url, datas, (r) =>
                {
                    //观澜反馈，CG5返回的msg为空，导致contains报错
                    if (r?.Status == null) return;
                    if (r.Status.Contains("Success") || r.Status.Contains("OK"))
                    //if (r.msg.Contains("Success") || r.msg.Contains("OK"))
                    {
                        //heartSource?.Cancel();
                        //heartSource = new CancellationTokenSource();
                        isSendSuccess = true;
                    }
                    resultRaw = r;
                    //if (r?.msg != null)
                    //{
                    //    PostAndGetRaw(url, datas, raw =>
                    //    {
                    //        rawRes = raw;
                    //    });
                    //}
                });
                if (isSendSuccess) break;
                Thread.Sleep(100);
            }

            //增加客户要求的Hive日志
            if (url.Contains("softwareversio"))//记录到版本日志中
            {
                var time = DateTime.Now;
                FileLogger.Log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas} Receive: {resultRaw?.Status}", $"D:/Hive/Hive Log/SW Version/{time.ToString("yyyyMMdd")}");
            }
            if (url.Contains("machinedata"))//记录到版本日志中
            {
                var time = DateTime.Now;
                FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas} Receive: {resultRaw?.Status}", $"D:/Hive/Hive Log/Machine Data/{time.ToString("yyyyMMdd")}/hour_{time.Hour}");
            }
            if (url.Contains("machinestate"))
            {
                var time = DateTime.Now;
                FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas} Receive: {resultRaw?.Status}", $"D:/Hive/Hive Log/Machine State/{time.ToString("yyyyMMdd")}");
            }
            if (url.Contains("errordata"))
            {
                var time = DateTime.Now;
                FileLogger.Log($"{time.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas} Receive: {resultRaw?.Status}", $"D:/Hive/Hive Log/TR Data/{time.ToString("yyyyMMdd")}");
            }

            //跳过启动时的version报文产生报警的影响
            if (specialTimeout)
            {
                isSendSuccess = true;
            }

            if (!isSendSuccess && !sysConfig.IsHiveValveEnabled)
            {
                if (IsHiveIgnoreAlarm == false)
                {
                    IsHiveIgnoreAlarm = true;
                    mController.MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.FailError, "Hive连续发送失败，客户要求停机", "N03OOOO-02@Send Hive message failed"));
                }
            }
            return isSendSuccess;
        }

        private string Get(string url, int times)
        {
            bool isSendSuccess = false;
            if (sysConfig.IsHiveIgnoreFeedBack) times = 1;
            var returnString = "";
            for (int i = 0; i < times; i++)
            {
                returnString = GetCommand(url);
                if (isSendSuccess) break;
                Thread.Sleep(100);
            }
            return returnString;
        }
        // 2025-5-10
        private bool Get2_post(string url, string datas, int times)
        {
            bool isOnboarded = false;
            // string rawRes = "";
            ResultStatus resultRaw = null;
            if (sysConfig.IsHiveIgnoreFeedBack) times = 1;
            for (int i = 0; i < times; i++)
            {
                CommException(url, datas, (r) =>
                {
                    // 2025-5-10 wyy
                    isOnboarded = r?.onboarding_status?.Contains("ONBOARDED") == true;
                    resultRaw = r;
                    //if (r?.msg != null)
                    //{
                    //    PostAndGetRaw(url, datas, raw =>
                    //    {
                    //        rawRes = raw;
                    //    });
                    //}
                });
                if (isOnboarded) break;
                Thread.Sleep(100);
            }
            // 2025-6-16 新增客户要求的Hive日志
            if (url.Contains("status"))//记录到登陆状态日志中
            {
                FileLogger.Log($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} URL:{url} send: {datas}  Receive: {resultRaw?.onboarding_status}", "D:/Hive/Hive Log/Onboarding Status/onboarding_status"); //  result:{isSendSuccess}
            }
            return isOnboarded;
        }
    }

    public static class JsonSerializerHelper<T> where T : class, new()
    {
        //将配置文件放在Config目录下,避免更换程序时拷贝两个文件
        private static string FolderName = "E:\\Config\\";//必须放在调用前
        private static string FilePath = Path.Combine(FolderName, Name + ".json");

        private static string Name { get { return typeof(T).ToString().Substring(typeof(T).ToString().LastIndexOf('.') + 1); } }

        public static bool Load(out T result, out string reason)
        {
            result = null;
            reason = string.Empty;
            if (!File.Exists(FilePath))
            {
                result = null;
                reason = "文件不存在";
                return false;
            }
            try
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                    string strRead = string.Empty;

                    while (sr.EndOfStream != true)
                    {
                        strRead += sr.ReadLine();
                    }
                    sr.Close();
                    fs.Close();
                    result = JsonConvert.DeserializeObject<T>(strRead);
                    return true;
                }
            }
            catch (Exception ex)
            {
                result = null;
                reason = ex.Message;
                return false;
            }
        }

        public static bool Save(T value, out string reason)
        {
            reason = string.Empty;

            try
            {
                Directory.CreateDirectory(FolderName);
                using (var fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
                {
                    var strWrite = JsonConvert.SerializeObject(value);
                    var writeBytes = Encoding.UTF8.GetBytes(strWrite);
                    fileStream.Write(writeBytes, 0, writeBytes.Length);
                    fileStream.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }
    }
}
