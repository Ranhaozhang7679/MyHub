using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.Integration.AOI
{
    public class CommunData
    {
        public VCommuncation Communcation { get; set; }

        public CancellationTokenSource Token { get; set; }
    }

    public class AOIHelper
    {
        /// <summary>
        /// 用于数据发送
        /// </summary>
        private VCommuncation vSend = null;

        /// <summary>
        /// AOI结果信息
        /// </summary>
        public static ConcurrentDictionary<string, AOIResult> AOIResult = new ConcurrentDictionary<string, AOIResult>();

        /// <summary>
        /// 日志事件
        /// </summary>
        public event Action<LogType, string, Exception> LogEvent;

        /// <summary>
        /// 报警
        /// </summary>
        public event Action<string> AlarmEvent;

        /// <summary>
        /// 
        /// </summary>
        public event Action<AOIResult> DbSaveEvent;

        /// <summary>
        /// 静态实例
        /// </summary>
        public static AOIHelper Instance = new AOIHelper();

        /// <summary>
        /// 通信对象
        /// </summary>
        private static Dictionary<string, CommunData> vCommuncations;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vComm"></param>
        static AOIHelper()
        {
            vCommuncations = new Dictionary<string, CommunData>();
        }

        /// <summary>
        /// 发送拍照通知
        /// </summary>
        public void Start(VCommuncation vSend, string startJson, int timeout = 5000)
        {
            // 1.发送Start命令,并等待结果
            string startCommand = $"`^{startJson}^`";
            vSend.Write(startCommand, "", false);

            // 记录通讯请求
            LogEvent?.Invoke(LogType.Debug, $"Start Send:{startCommand}", null);
        }

        /// <summary>
        /// 停止接收器
        /// </summary>
        public void StopReceiver(VCommuncation vComm)
        {
            if (vComm == null) return;

            string name = vComm.Name;
            if (vCommuncations.ContainsKey(name))
            {
                var cData = vCommuncations[name];
                cData.Token?.Cancel();
                cData.Token = null;

                cData.Communcation?.Close();
                cData.Communcation = null;

                vCommuncations.Remove(name);
            }
        }

        /// <summary>
        /// 启动接收器
        /// </summary>
        /// <param name="vComm">通信</param>
        /// <param name="timeout">超时</param>
        public void StartReceiver(VCommuncation vComm, bool isMain = false, int timeout = 5000)
        {
            // 是否主服务器，如果不是直接退出
            if (!isMain)
            {
                return;
            }

            // 如果存在，并且线程无异常，直接返回
            if (vCommuncations.ContainsKey(vComm.Name))
            {
                var vData = vCommuncations[vComm.Name];

                // 检测通信和线程都没有中断
                if (vData.Communcation != null && vData.Token != null && !vData.Token.IsCancellationRequested)
                {
                    return;
                }
            }

            // 记录连接
            var cancelTokenSource = new CancellationTokenSource();
            var cData = new CommunData() { Communcation = vComm, Token = cancelTokenSource };
            vCommuncations.Add(vComm.Name, cData);

            // 异步等待结果
            Task.Factory.StartNew((obj) =>
            {
                var c = obj as CommunData;
                int threadID = Thread.CurrentThread.ManagedThreadId;
                LogEvent?.Invoke(LogType.Debug, $"AOI通信_{vComm?.Name} 线程:{threadID} 启动监听!", null);
                while (true)
                {
                    if (c.Token != null && c.Token.IsCancellationRequested)
                    {
                        LogEvent?.Invoke(LogType.Debug, $"AOI通信_{vComm?.Name} 线程:{threadID} 终止!", null);
                        break;
                    }

                    try
                    {
                        if (c.Communcation == null) return;

                        // 乘以10
                        timeout *= 10;

                        string replay = c.Communcation.ReadSingle<string>("", timeout, false, false);
                        ProcReponse(replay, (action, result) =>
                        {
                            switch (action)
                            {
                                case ReplayAction.Alarm:
                                    if (result is AOIAlarm alarm)
                                    {
                                        AlarmEvent?.Invoke($"AOI通信_{vComm?.Name} Error:{alarm.GetAlarmMessage()}");
                                    }
                                    break;
                                case ReplayAction.CheckResult:
                                    if (result is AOIResult aoi)
                                    {
                                        string productID = aoi.Product_Id.ToLower();
                                        if (AOIResult.ContainsKey(productID))
                                        {
                                            AOIResult.TryRemove(productID, out var rTemp);
                                        }

                                        if (!AOIResult.TryAdd(productID, aoi))
                                        {
                                            LogEvent?.Invoke(LogType.Warning, $"AOI通信_{vComm?.Name} 结果异常:{aoi}", null);
                                        }
                                        else
                                        {
                                            LogEvent?.Invoke(LogType.Debug, $"AOI通信_{vComm?.Name} 获取成功:ID={productID} Serial={aoi.Serial}", null);
                                        }
                                    }
                                    else
                                    {
                                        LogEvent?.Invoke(LogType.Warning, $"Result Convert Fail:{result}", null);
                                    }
                                    break;
                            }
                        });

                        // 记录通讯请求
                        LogEvent?.Invoke(LogType.Debug, $"AOI通信_{vComm?.Name} 线程:{threadID} Receive:{replay}", null);
                    }
                    catch (DeviceTimeoutException time)
                    {
                        LogEvent?.Invoke(LogType.Debug, $"AOI通信_{vComm?.Name} 线程:{threadID} 访问超时:{time.Message}", time);
                    }
                    catch (Exception ex)
                    {
                        cancelTokenSource?.Cancel();
                        LogEvent?.Invoke(LogType.Warning, $"AOI通信_{vComm?.Name} 线程:{threadID} 访问异常:{ex.Message}", ex);
                    }

                    // 循环等待
                    Thread.Sleep(50);
                }
            }, cData, cData.Token.Token);
        }

        /// <summary>
        /// `^ Result ^`
        /// </summary>
        /// <param name="result"></param>
        private void ProcReponse(string reponse, Action<ReplayAction, object> retAction, Action errAction = null)
        {
            if (string.IsNullOrEmpty(reponse)) return;

            var datas = reponse.Split(new char[] { '^', '`' }).Where(u => !string.IsNullOrEmpty(u)).ToList();

            foreach (var item in datas)
            {
                // 可能会解析到异常字符串
                if (item.Length < 5) continue;

                try
                {
                    IAOIAction r = JsonTool.ToObject<AOIAction>(item);
                    if (Enum.TryParse<ReplayAction>(r.Action, out var action))
                    {
                        switch (action)
                        {
                            case ReplayAction.Alarm:
                                r = JsonTool.ToObject<AOIAlarm>(item);
                                break;
                            case ReplayAction.CheckResult:
                                r = JsonTool.ToObject<AOIResult>(item);
                                break;
                            case ReplayAction.ReplyStart:
                                r = JsonTool.ToObject<AOIReplay>(item);
                                break;
                        }

                        retAction?.Invoke(action, r);
                    }
                }
                catch (Exception ex)
                {
                    errAction?.Invoke();
                    LogEvent?.Invoke(LogType.Warning, $"AOI 格式解析失败:{ex.Message}", ex);
                }
            }
        }


        /// <summary>
        /// 获取结果
        /// </summary>
        /// <param name="serial"></param>
        /// <returns></returns>
        public AOIResult GetResult(VCommuncation vComm, string uniqueID, string outJson = "", int timeout = 5000)
        {
            string id = uniqueID.ToLower();

            // aoi 
            AOIResult aoiResult = null;
            if (AOIResult.TryRemove(id, out aoiResult))
            {
                LogEvent?.Invoke(LogType.Debug, $"GetResult:{uniqueID} is ok!", null);

                // 数据访问
                DbSaveEvent?.Invoke(aoiResult);
                return aoiResult;
            }
            else
            {
                if (!string.IsNullOrEmpty(outJson))
                {
                    // 1.发送下料强制命令,并等待结果
                    string outCommand = $"`^{outJson}^`";
                    vComm.Write(outCommand, "", false);

                    // 2.通过UniqueId 获取
                    int time = 0;
                    while (true)
                    {
                        // 尝试获取结果
                        if (AOIResult.TryRemove(id, out aoiResult))
                        {
                            // 数据访问
                            DbSaveEvent?.Invoke(aoiResult);
                            break;
                        }

                        // 时间超时，也退出循环
                        if (time > timeout)
                        {
                            LogEvent?.Invoke(LogType.Warning, $"AOI Unique={uniqueID}下料请求超时，请求时间>{timeout}", null);
                            break;
                        }

                        Thread.Sleep(5);
                        time += 5;
                    }

                    // 获取结果
                    string logInfo = $"PannalOut:{outCommand} {(time > timeout ? "NG" : "OK")} Result:{aoiResult}";
                    LogEvent?.Invoke(LogType.Debug, logInfo, null);
                }

                // 如果pannelout后，输出NG结果
                if (aoiResult == null)
                {
                    aoiResult = new AOIResult();
                    aoiResult.Result = "NG";
                    aoiResult.Images = new List<TbAOIImage>();
                    aoiResult.Resulttype = $"AOI Unique={uniqueID} 未获取到结果";
                    LogEvent?.Invoke(LogType.Warning, aoiResult.Resulttype, null);
                }
            }

            return aoiResult;
        }
    }

    /// <summary>
    /// 响应动作
    /// </summary>
    public enum ReplayAction
    {
        ReplyStart,

        Alarm,

        CheckResult
    }

    public interface IAOIAction
    {
        string Action { get; set; }
    }

    /// <summary>
    /// 动作
    /// </summary>
    public class AOIAction : IAOIAction
    {
        public string Action { get; set; }
    }

    /// <summary>
    /// 报警内容
    /// </summary>
    public class AOIAlarm : AOIAction
    {
        /// <summary>
        /// 对应的报警
        /// </summary>
        public int PC { get; set; }

        /// <summary>
        /// 报警数据
        /// </summary>
        public List<Alarm> Data { get; set; }

        public string GetAlarmMessage()
        {
            return string.Join(",", Data.Select(u => u.AlarmText));
        }
    }

    /// <summary>
    /// 报警模型
    /// </summary>
    public class Alarm
    {
        public int AlarmID { get; set; }

        public string AlarmText { get; set; }
    }

    /// <summary>
    /// AOI 结果
    /// </summary>
    public class AOIResult : TbAOIResult, IAOIAction
    {
    }

    /// <summary>
    /// Start 响应
    /// </summary>
    public class AOIReplay : AOIAction
    {
        /// <summary>
        /// 返回信息
        /// </summary>
        public string Status { get; set; }


        /// <summary>
        /// 治具吗
        /// </summary>
        public int JigNumber { get; set; }
    }
}
