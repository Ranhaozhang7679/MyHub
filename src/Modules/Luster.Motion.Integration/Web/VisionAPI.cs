#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VisionAPI
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.TaskFlow.Engine.HyperTrain
* 文 件 名:       VisionAPI.cs
* 创建时间:       2023/1/6 21:54:48
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      32a2d63c-c0dd-43eb-883a-6fbc56b14c7a
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/1/6 21:54:48
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Foxconn.IMES.PictureCollection;
using Foxconn.IMES.PictureCollection.Model;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using SimpleTCP;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using static FreeSql.Internal.GlobalFilter;
using Path = System.IO.Path;

namespace Luster.Motion.Integration.Web
{
    public class VisionState
    {
        public TrainRunMode VisionMachineState { get; set; } = 0;
        public string VisionErrorCode { get; set; } = "";
        public string VisionErrorMessage { get; set; } = "";

    }


    /// <summary>
    /// Vision 系统API
    /// </summary>
    public class VisionAPI : BaseMESAPI, ISubWebSystem
    {
        public string WIP = "";
        Instance picUploadInstance;
        JsonResultModel resultModel;

        static bool IsShieldDoorMem = false;
        static bool IsProductModeMem = false;
        static bool IsDryRunModeMem = false;
        static bool IsFirstPieceModeMem = false;
        static bool IsPressureMonitorMem = false;
        static bool IsRobotEnabledMem = false;
        /// <summary>
        /// 软件关机xx->Down,软件开机Idle->xx,中间缺少Down->Idle的切换
        /// 因此在首次开机传Idle->xx前不上Down->Idle的流程
        /// </summary>
        private bool bIsFirstOpen = true;

        private readonly string PDCANAME = "Extend_PDCA启用";
        private readonly string SFCNAME = "Extend_SFC启用";
        private readonly string VISIONNAME = "开启VISION上传";
        private readonly string HIVENAME = "启用Hive";
        /// <summary>
        /// 设备引擎
        /// </summary>
        protected IDeviceEngine deviceEngine;

        List<MapData> MapDatas { get; set; }

        public static Dictionary<string, string> VisionProcessData { get; set; } = new Dictionary<string, string>();


        private ICacheManager _cacheManager;

        private System.Timers.Timer _timer;
        /// <summary>
        /// SFCHelper
        /// </summary>
        private SFCHelper _sfcHelper = null;
        /// <summary>
        /// 错误内容
        /// </summary>
        /// </summary>
        private readonly IErrorManager _errorMangaer;
        /// <summary>
        /// 是否上传过控制参数，客户要求设备开启第一次，需要上传所有控制参数
        /// </summary>
        private static bool IsUploadPara;

        public Dictionary<string, string> ctConfigs = new Dictionary<string, string>();
        List<string> listA = new List<string>();

        //2025/12/24 添加
        public VisionState _visionState;
        public VisionAPI(ICacheManager cacheManager, IErrorManager errorManager)
        {
            _cacheManager = cacheManager;
            _errorMangaer = errorManager;
            _timer = new System.Timers.Timer(30000);
            _timer.Enabled = false;
            _timer.Elapsed -= OnHeartBeatEvent;
            _timer.Enabled = true;
            _timer.Elapsed += OnHeartBeatEvent;

            //2025/12/24 添加
            _visionState = new VisionState();

            //try
            //{
            //    string csvPath = "CtConfig.csv";
            //    if (File.Exists(csvPath))
            //    {
            //        var lines = File.ReadAllLines(csvPath,Encoding.UTF8);
            //        foreach (var line in lines)
            //        {
            //            var strs = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //            if (strs.Length > 1)
            //            {
            //                if (ctConfigs.ContainsKey(strs[0]) == false)
            //                {
            //                    ctConfigs.Add(strs[0], strs[1]);
            //                }
            //            }
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{

            //}
            //  picUploadInstance = new Instance();
            //  resultModel = new JsonResultModel();
            //  picUploadInstance.Init(sysConfig.MachineName,sysConfig.MachineSn,sysConfig.VendorName,sysConfig.Product,sysConfig.StationId,sysConfig.StationName,sysConfig.LineCode,sysConfig.Floor,sysConfig.Area,sysConfig.SiteCode);
        }

        public void LoadCtConfig(string Path)
        {
            try
            {
                string csvPath = System.IO.Path.Combine(Path, "CtConfig.csv");
                if (File.Exists(csvPath))
                {
                    var lines = File.ReadAllLines(csvPath, Encoding.Default);
                    foreach (var line in lines)
                    {
                        var strs = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (strs.Length > 1)
                        {
                            if (ctConfigs.ContainsKey(strs[0]) == false)
                            {
                                ctConfigs.Add(strs[0], strs[1]);
                            }
                        }
                    }
                    foreach (var keyPair in ctConfigs)
                    {
                        string[] parts = keyPair.Value.Split('_');
                        string stationName = parts[1]; // 取第二个字符，工站名
                        if (!listA.Contains(stationName))
                        {
                            listA.Add(stationName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnLog($"CtConfig.csv,读取异常。ex={ex.StackTrace}", "machineParameter");
            }
        }
        // 定义Elapsed事件的处理方法
        private void OnHeartBeatEvent(Object source, ElapsedEventArgs e)
        {
            MachineHeartbeatUpload2();
        }
        //暂借用Hive的errorcode
        public override string GetSystem()
        {
            return "Vision";
            //return "Hive";

        }
        private IMotionController _motionController;
        protected override void Register(IMotionController motionController)
        {
            _motionController = motionController;
            base.Register(motionController);
            var globalModule = motionController.MotionEngine.Get(GlobalModule.GlobalID);

            globalModule.PropertyChangedEvent -= global_PropertyChanged;
            globalModule.PropertyChangedEvent += global_PropertyChanged;

            motionController.PropertyChanged -= MotionController_PropertyChanged;
            motionController.PropertyChanged += MotionController_PropertyChanged;

            motionController.MotionEngine.VisionEvent -= MotionEngine_TimeStatisEvent;
            motionController.MotionEngine.VisionEvent += MotionEngine_TimeStatisEvent;

            // 产能数据
            motionController.ProStatEvent -= MotionController_ProStatEvent;
            motionController.ProStatEvent += MotionController_ProStatEvent;

            motionController.ProductEvent -= MotionController_ProductEvent;
            motionController.ProductEvent += MotionController_ProductEvent;

            motionController.ProductTrowEvent -= MotionController_ThrowEvent;
            motionController.ProductTrowEvent += MotionController_ThrowEvent;


            MapDatas = motionController.MotionEngine.MapDatas;

            //motionController.MachineStatusEvent -= MotionEngine_ProBlockEvent;
            //motionController.MachineStatusEvent += MotionEngine_ProBlockEvent;
        }

        //抛小料事件
        private void MotionController_ThrowEvent(StationResult stationResult)
        {
            string wip = "";
            LogTool.Debug("抛料事件" + stationResult.IsPreviousStationUndo.ToString(), "visionlog记录");
            if (!stationResult.IsPreviousStationUndo)
            {
                ProductInfoColletct(stationResult, out wip);
            }
        }

        /// <summary>
        /// 第一次出料
        /// </summary>
        bool isFirstUnload = true;
        //出料事件
        private void MotionController_ProductEvent(ProductInfo arg1, bool arg2, double ct)
        {
            string wip = "N999999999";

            //ProductParaUpload(arg1.Result.ProCode, GetAElimits(arg1.Result));
            GetAElimits(arg1.Result);
            if (!arg1.Result.IsPreviousStationUndo)
            {
                ProductInfoColletct(arg1.Result, out wip);
            }
            //第一次出料，获取下质量定义里面的上下限
            if (isFirstUnload)
            {
                MachineParaUpload("", "", "", arg1.Result.ProCode);
                isFirstUnload = false;
            }
        }




        protected override void MController_StatusEvent(IMotionController mController, TrainRunMode trainRunMode, string reason)
        {
            base.MController_StatusEvent(mController, trainRunMode, reason);
            if (trainRunMode == TrainRunMode.Running)
            {
                StatusChanged(mController, EngineStatus.Running, EngineStatus.Idle);
            }
            else
            {
                StatusChanged(mController, EngineStatus.Idle, EngineStatus.Running);
            }
        }

        protected override void ManualDown(object arg1, SystemOperation op, string reason)
        {
            base.ManualDown(arg1, op, reason);

            //手动停止后，缓存需要清除
            cacheTimes.Clear();
        }

        private ProductStat productStat;

        /// <summary>
        /// 获取产能数据
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MotionController_ProStatEvent(ProductStat arg1, ImpactData arg2)
        {
            productStat = arg1;
        }

        /// <summary>
        /// CT统计上传数据
        /// </summary>
        /// <param name="obj"></param>
        private void MotionEngine_TimeStatisEvent(List<DataStruct.DataModels.StationTime> obj, bool IsUseLog)
        {
            try
            {
                //GodMan();
                if (!IsUseLog || obj.Count <= 0) return;
                var validSN = obj[0].SN;
                foreach (var ctInfo in obj)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ctInfo.Module + ",");
                    sb.Append(ctInfo.Station + ",");
                    sb.Append(ctInfo.SN + ",");
                    sb.Append(ctInfo.StartTime + ",");
                    sb.Append(ctInfo.EndTime + ",");
                    sb.Append(ctInfo.Time + ",");
                    sb.Append(ctInfo.CT + ",");
                    foreach (var item in ctInfo.ExtParams)
                    {
                        sb.Append(item.Key + ",");
                    }
                    //string debugVisionParameterPath = @"D:\Vision\Parameter\";
                    //if (!Directory.Exists(debugVisionParameterPath))
                    //    Directory.CreateDirectory(debugVisionParameterPath);
                    //using (FileStream fs = new FileStream(Path.Combine(debugVisionParameterPath, $"{validSN}.csv"), FileMode.Append, FileAccess.Write))
                    //using (StreamWriter streamWriter = new StreamWriter(fs, Encoding.Default))
                    //{
                    //    streamWriter.WriteLine(sb.ToString());
                    //}
                }
                ProductParaUpload(validSN, GetCTData(obj));
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }
        /// <summary>
        /// 从外部文件中读取后，直接上传
        /// </summary>
        private void GodMan()
        {
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineParameter");
            try
            {
                string path = "D://VisionParameter.txt";
                if (!File.Exists(path)) return;
                var jsonStr = File.ReadAllText(path, Encoding.Default);
                if (jsonStr.Length < 0) return;
                File.Delete(path);

                //string jsonData = JsonTool.ToJson(data);
                CommException(url, jsonStr, r =>
                {
                    if (r.IsSuccess)
                    {
                        OnLog($"产品过程参数，上传OK!", url);
                    }
                    else
                    {
                        OnLog($"产品过程参数，上传Fail!，data={jsonStr}", url);
                    }
                });
            }
            catch (Exception ex)
            {
                OnLog($"产品过程参数，上传Fail!{ex.ToString()}", url);
            }

        }

        /// <summary>
        /// 参数变更
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        private void MotionController_PropertyChanged(string paramName, object srcV, object newV)
        {
            try
            {
                MachineParaUpload(paramName, srcV, newV);
                //LogTool.Debug(paramName + ":" + newV.ToString(), "Parameter");
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        /// <summary>
        /// 全局参数变更
        /// </summary>
        /// <param name="module"></param>
        /// <param name="paramName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        private void global_PropertyChanged(Luster.TaskFlow.Common.Module.IModule module, string paramName, object srcV, object newV)
        {
            try
            {
                OnLog("全局变量发生变化*******");
                //MachineParaUpload(paramName, srcV, newV);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        public override void SysConfig_IsEnabledEvent(string system, bool isEnabled)
        {
            base.SysConfig_IsEnabledEvent(system, isEnabled);
            if (system == "Vision")
            {
                MachineParaUpload("开启VISION上传", !isEnabled, isEnabled);
            }

            if (system == "Hive")
            {
                MachineParaUpload("启用Hive", !isEnabled, isEnabled);
            }
        }

        private WebTool webTool;


        /// <summary>
        /// 存放当前SN对应的所有步骤
        /// </summary>
        private ConcurrentDictionary<string, List<StationTime>> cacheTimes = new ConcurrentDictionary<string, List<StationTime>>();
        private ConcurrentDictionary<string, ArrayList> AelimtsCacheTimes = new ConcurrentDictionary<string, ArrayList>();
        private ConcurrentDictionary<string, proResultData> ProCacheTimes = new ConcurrentDictionary<string, proResultData>();

        /// <summary>
        /// 通过外部读取的CTConfig，找到缓存中对应的步序信息
        /// </summary>
        /// <param name="stationTimes">当前SN的所有步序</param>
        /// <param name="moduleName">模块别名，即ctconfig第一列</param>
        /// <param name="moduleValue">ctconfig第二列对应的真实要上传的键名</param>
        /// <param name="parameterName">返回的键名</param>
        /// <param name="parameterValue">返回对应的时间</param>
        private void FindStationTIme(List<StationTime> stationTimes, string moduleName, string moduleValue, out string parameterName, out string parameterValue)
        {
            var module = stationTimes.Find((x) => x.Module == moduleName.Trim('"', '\t'));

            //为了区分是否有左右工站，如GG8
            if (module.StartTime == DateTime.MinValue)
            {
                //parameterName = "None";
                //parameterValue = "None";
                parameterName = moduleValue;
                parameterValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            }
            else
            {
                //parameterName = moduleName;
                parameterName = moduleValue;

                if (moduleValue.EndsWith("Start_Time"))
                    parameterValue = module.StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
                else
                    parameterValue = module.EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff");
            }




        }

        /// <summary>
        /// 获取SN及对应的CT信息
        /// </summary>
        /// <param name="tbCTInfos"></param>
        /// <returns></returns>
        public string IncrementCtNumber(string ctNumber)
        {
            // 检查输入是否符合预期格式
            if (string.IsNullOrEmpty(ctNumber) || !ctNumber.StartsWith("CT"))
            {
                _motionController.MotionEngine.OnLog(LogType.Error, $"非法的CT编号");
                //throw new ArgumentException("Invalid CT number format. Expected format: 'CT<number>'");
            }

            // 提取数字部分
            string prefix = ctNumber.Substring(0, 2); // "CT"
            string numberPart = ctNumber.Substring(2); // "10"

            // 将数字部分转换为整数并递增
            if (!int.TryParse(numberPart, out int number))
            {
                _motionController.MotionEngine.OnLog(LogType.Error, $"非法的CT编号，不是可用的整数");
                //throw new Exception("Invalid CT number format. Number part is not a valid integer.");
            }

            number++; // 递增数字部分

            // 生成新的 CT 序号
            return $"{prefix}{number}";
        }
        public object GetCTData(List<DataStruct.DataModels.StationTime> tbCTInfos)
        {
            //从这一步开始，默认外部已经经过了筛选，默认CT>0,uselog,sn有效

            //将外部单个自由工站传入的过程tbCTInfos插入到当前SN对于的中间缓存
            //直到某个自由工站中包含出料事件，再拼接输出
            //这种写法就要求包含SN的出料事件也必须包含在工站中
            var validSN = tbCTInfos.Last().SN;
            if (!cacheTimes.ContainsKey(validSN))
            {
                cacheTimes.TryAdd(validSN, new List<StationTime>(tbCTInfos));
            }
            else
            {
                cacheTimes[validSN].AddRange(new List<StationTime>(tbCTInfos));
            }
            if (!tbCTInfos.Any((x) => x.Module.Contains("出料事件")))
            {
                return string.Empty;
            }
            //只有这三种模式会上传
            //var strUploadMode = new List<string>() { "生产模式", "CPK模式", "空跑模式" };
            //if (!strUploadMode.Contains(mController.GetCurrentMode()))
            //{
            //    return string.Empty;
            //}
            if (!(mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑") || mController.GetCurrentMode().Contains("CPK") || mController.GetCurrentMode().Contains("GRR")))
            {
                return string.Empty;
            }
            //开始从缓存中取数据拼接
            ArrayList ctList = new ArrayList();
            //一些自由的过程参数
            if (VisionProcessData.Count > 0)
            {
                foreach (var data in VisionProcessData)
                {
                    var moduleTime = new
                    {
                        parameter = data.Key,
                        value = data.Value
                    };
                    ctList.Add(moduleTime);
                }
            }

            //从缓存中取出对应出料SN的所有步序            
            cacheTimes.TryRemove(validSN, out List<StationTime> currentCTInfos);
            // 2026-1-19，CtConfig联动CtLogConfig
            var kvList = ctConfigs.ToList();          // 保持插入顺序
            for (int i = 0; i + 1 < kvList.Count; i += 2)
            {
                var firstKey = kvList[i].Key;     // 第一行 key
                var secondKey = kvList[i + 1].Key;// 第二行 key
                // 到 ctInfos 里找对应元素
                var first = currentCTInfos.FirstOrDefault(c => listA.Contains(c.Station) && c.Module == firstKey);
                var second = currentCTInfos.FirstOrDefault(c => listA.Contains(c.Station) && c.Module == secondKey);
                // first或second不会为空，只是属性取值会为空
                //if (first != null && second != null)
                if (first.StartTime == DateTime.MinValue)
                {
                    first.StartTime = DateTime.Now;
                    first.CT = 1000;
                }
                // SN不会拿不到，因为只有拿到后，才会存入currentCTInfos
                //if (!station.Contains("+") || station.Length < 14)
                //{
                //    station = "NULL";
                //}
                // 找到第一行，取第一行key在ctConfigs里面的value
                var firstValue = kvList[i].Value;
                //string moduleNameTrim = Regex.Replace(first.模块, @"-(?:Start|End)$", "");
                string moduleNameTrim = first.Station;
                string[] parts = firstValue.Split('_');
                if (string.IsNullOrEmpty(moduleNameTrim))
                {
                    moduleNameTrim = parts[1];
                }
                if (parts.Length > 0 && parts[0] == "CT2")
                {
                    //添加工站开始的3组数据
                    var moduleTime1 = new
                    {
                        parameter = $"CT1_{moduleNameTrim}_工站开始_Start_Time",
                        value = first.StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                    };
                    ctList.Add(moduleTime1);
                    var moduleTime2 = new
                    {
                        parameter = $"CT1_{moduleNameTrim}_工站开始_End_Time",
                        value = first.StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                    };
                    ctList.Add(moduleTime2);
                    var moduleTime3 = new
                    {
                        parameter = $"CT1_{moduleNameTrim}_工站开始_Target_CT",
                        value = 0
                    };
                    ctList.Add(moduleTime3);
                }
                if (second.EndTime == DateTime.MinValue)
                {
                    second.EndTime = DateTime.Now.AddSeconds(1);
                }
                var moduleTime4 = new
                {
                    parameter = $"{firstValue}_Start_Time",
                    value = first.StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                };
                ctList.Add(moduleTime4);
                var moduleTime5 = new
                {
                    parameter = $"{firstValue}_End_Time",
                    value = second.EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                };
                ctList.Add(moduleTime5);
                var moduleTime6 = new
                {
                    parameter = $"{firstValue}_Target_CT",
                    value = first.CT / 1000.0
                };
                ctList.Add(moduleTime6);
                // 匹配到两行后
                if (i + 2 < ctConfigs.Count)  // i+1改为i+2
                {
                    var nextKeyPair = ctConfigs.ElementAt(i + 2);
                    string[] nextParts = nextKeyPair.Value.Split('_');

                    if ((parts.Length > 1 && nextParts.Length > 1 && parts[1] != nextParts[1]) || nextParts[0] == "CT2")
                    {
                        //添加工站结束的3组数据
                        string nextCtNumber = IncrementCtNumber(parts[0]);
                        var moduleTime1 = new
                        {
                            parameter = $"{nextCtNumber}_{moduleNameTrim}_工站结束_Start_Time",
                            value = second.EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                        };
                        ctList.Add(moduleTime1);
                        var moduleTime2 = new
                        {
                            parameter = $"{nextCtNumber}_{moduleNameTrim}_工站结束_End_Time",
                            value = second.EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                        };
                        ctList.Add(moduleTime2);
                        var moduleTime3 = new
                        {
                            parameter = $"{nextCtNumber}_{moduleNameTrim}_工站结束_Target_CT",
                            value = 0
                        };
                        ctList.Add(moduleTime3);
                        //break;
                    }
                }
                else
                {
                    //处理完csv最后一行，也需要添加工站结束
                    string nextCtNumber = IncrementCtNumber(parts[0]);
                    var moduleTime1 = new
                    {
                        parameter = $"{nextCtNumber}_{moduleNameTrim}_工站结束_Start_Time",
                        value = second.EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                    };
                    ctList.Add(moduleTime1);
                    var moduleTime2 = new
                    {
                        parameter = $"{nextCtNumber}_{moduleNameTrim}_工站结束_End_Time",
                        value = second.EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                    };
                    ctList.Add(moduleTime2);
                    var moduleTime3 = new
                    {
                        parameter = $"{nextCtNumber}_{moduleNameTrim}_工站结束_Target_CT",
                        value = 0
                    };
                    ctList.Add(moduleTime3);
                }
            }

            //foreach (var keyPair in ctConfigs)
            //{
            //    FindStationTIme(currentCTInfos, keyPair.Key, keyPair.Value, out string paraName, out string paraValue);
            //    Debug.WriteLine($"当前SN= {validSN}, Name= {paraName}, value= {paraValue}");
            //    //如果没有找到对应的步序或者步序时间不对，就不上传
            //    if (paraName == "None") continue;
            //    var moduleTime = new
            //    {
            //        parameter = paraName,
            //        value = paraValue
            //    };
            //    ctList.Add(moduleTime);
            //}

            //Thread.Sleep(500);//2500
            AelimtsCacheTimes.TryRemove(validSN, out var ctlog);
            if (ctlog != null)
            {
                ctList.AddRange(ctlog);
            }
            ProCacheTimes.TryRemove(validSN, out var proData);

            if (cacheTimes.Count > 20)
            {
                for (int i = 0; i < cacheTimes.Count - 20; i++)
                {
                    var keyToRemove = cacheTimes.ElementAt(0).Key;
                    cacheTimes.TryRemove(keyToRemove, out _);
                }
            }
            // 增加作业模式的参数
            ctList.AddRange(new[]
            {
                new { parameter = "Card_Mesg", value = editUser },
                new { parameter = "HomeWork_Mode", value = Homework_Mode }
            });
            object jsonData = new
            {
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                appVersion = sysConfig.SoftVersion,
                materialSn = proData.materialSn,
                carrierSn = proData.carrierSn,
                startTime = proData.startTime,
                endTime = proData.endTime,
                cycleTime = proData.cycleTime,
                result = proData.result,
                resultDescription = proData.resultDescription,
                from = "MACHINE-PARAMETER-API",
                type = "R_TEST_PARAM",
                //cgtossCount=proData.cgTossCount,
                //tossReason= proData.tossReason,

                apiVersion = sysConfig.ApiVersion,
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                data = ctList
            };
            return new ArrayList { jsonData };
        }


        /// <summary>
        /// 生成AElimits
        /// </summary>
        /// <param name="stationResult"></param>
        /// <returns></returns>
        public object GetAElimits(StationResult stationResult)
        {
            string wip = "";
            string carrierId = "";
            //创建AElimits实体
            ArrayList ctList = new ArrayList();
            //获取wip
            wip = LcgToWip(stationResult.ProCode);
            if (wip.Equals("None"))
            {
                wip = stationResult.WIP;
            }
            carrierId = LcgToCarrierId(stationResult.ProCode);
            //获取质量定义里面所有的值
            var cacheItem = _cacheManager.GetItem(stationResult.ProCode);


            double upLimit = 0;
            double downLimit = 0;

            if (cacheItem != null && cacheItem is CacheItem cItem)
            {
                foreach (LColumn aelimt in (CacheItem)cacheItem)
                {
                    var selectMapData = MapDatas.Find(x => x.Alias == aelimt.Alias);
                    if (selectMapData == null)
                    {
                        continue;
                    }
                    upLimit = selectMapData.PositivestandardDeviation;
                    downLimit = selectMapData.NegativestandardDeviation;
                    if (upLimit != 0 || downLimit != 0)
                    {
                        //参数值
                        var ael = new
                        {
                            parameter = aelimt.Alias,
                            value = aelimt.Value
                        };
                        ctList.Add(ael);
                    }
                }
            }

            int iCgTossCount = productStat.Toss;
            var cgTossCount = new
            {
                parameter = $"CGTossCount",
                value = iCgTossCount
            };
            ctList.Add(cgTossCount);
            var tossReason = new
            {
                parameter = $"TossReason",
                value = stationResult.IsToss ? stationResult.ErrMsg : "0"
            };
            ctList.Add(tossReason);
            int tossCount = 0;
            foreach (var val in productStat.ThrowMaterialDic)
            {
                if (val.Key.Contains("小料"))
                {
                    tossCount += val.Value;
                }
            }
            var materialTossCount = new
            {
                parameter = $"MaterialTossCount",
                value = tossCount
            };
            ctList.Add(materialTossCount);

            proResultData proResultData = new proResultData()
            {
                materialSn = wip,
                carrierSn = carrierId,
                startTime = stationResult.EnterTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                cycleTime = Math.Round((DateTime.Now - stationResult.EnterTime).TotalMilliseconds / 1000.0, 3).ToString(),
                result = stationResult.IsToss ? "TOSSING" : stationResult.Result ? "PASS" : "FAIL",
                resultDescription = stationResult.Result ? "" : stationResult.ErrMsg,
            };

            AelimtsCacheTimes.TryAdd(stationResult.ProCode, ctList);
            ProCacheTimes.TryAdd(stationResult.ProCode, proResultData);

            return "";
        }

        /// <summary>
        /// 1.设备心跳监测
        /// </summary>
        protected override void MachineHeartbeatUpload()
        {
            // 如果没有连接上，不触发通讯
            //if (!isConnected) return;
            //string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineHeartbeat");

            //ArrayList paramList = new ArrayList();

            //// 构建数据
            //ArrayList jsonArray = new ArrayList();

            //var machineParaData = new
            //{
            //    machineSn = sysConfig.MachineSn,
            //    stationId = sysConfig.VisionStationId,
            //    cellId = "1",
            //    uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    deviceIp = sysConfig.DeviceIP,
            //    deviceMac = sysConfig.DeviceMac,
            //    from = "MACHINE-HEARTBEAT-API",
            //    apiVersion = sysConfig.ApiVersion
            //};

            //jsonArray.Add(machineParaData);
            //string jsonData = JsonTool.ToJson(jsonArray);
            //CommException(url, jsonData, r =>
            //{
            //    if (r.IsSuccess)
            //    {
            //    }
            //});
        }

        private void MachineHeartbeatUpload2()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineHeartbeat");

            ArrayList paramList = new ArrayList();

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var machineParaData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                from = "MACHINE-HEARTBEAT-API",
                apiVersion = sysConfig.ApiVersion
            };

            jsonArray.Add(machineParaData);
            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, r =>
            {
                if (r?.IsSuccess == true)
                {
                }
            });
        }

        protected override void OnLog(string message, string url = "")
        {
            // 针对心跳，CTLog，机台状态，单独记录 
            if (url.Contains("machineHeartbeat"))
            {
                LogTool.Debug(message, "VisionHeart");
            }
            //针对注册
            else if (url.Contains("machineRegister"))
            {
                LogTool.Debug(message, "VisionRegister");
            }
            //针对设备状态
            else if (url.Contains("machineStatus"))
            {
                LogTool.Debug(message, "VisionMachineStatus");
            }
            //针对产能
            else if (url.Contains("machineIndicator"))
            {
                LogTool.Debug(message, "VisionMachineIndicator");
            }
            //针对控制参数
            else if (url.Contains("machineControl"))
            {
                LogTool.Debug(message, "VisionMachineControl");
            }
            //过程参数
            else if (url.Contains("machineParameter"))
            {
                LogTool.Debug(message, "VisionMachineParameter");
            }
            //产品纬度
            else if (url.Contains("productInformation"))
            {
                LogTool.Debug(message, "VisionProductInformation");
            }
            else
            {
                base.OnLog(message, url);
            }
        }

        /// <summary>
        /// 2.设备注册
        /// </summary>

        public override void Register()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected)
            {
                LogTool.Debug("vision连接失败", "visionlog记录");
                return;
            }
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineRegister");
            ArrayList paramList = new ArrayList();

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var machineParaData = new
            {
                machineSn = sysConfig.MachineSn,
                machineName = sysConfig.MachineName,
                machineType = sysConfig.MachineType,
                functionId = sysConfig.FunctionId,
                manageDept = sysConfig.ManageDept_Vision,
                //registerType = sysConfig.RegisterType,
                product = sysConfig.Product_Vision,
                siteCode = sysConfig.SiteCode,
                area = sysConfig.Area,
                floor = sysConfig.Floor,
                lineCode = sysConfig.LineCode,
                uniteCode = $"{sysConfig.LineCode}_{sysConfig.UniteCode}",
                stationName = sysConfig.StationName_Vision,
                stationType = sysConfig.StationType,
                stationId = sysConfig.VisionStationId,
                vendorName = sysConfig.VendorName,
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                osVersion = "Windows10",
                appVersion = sysConfig.SoftVersion,
                configVersion = "V1.2",
                //1.0.2
                addBy = "H000001",
                //editBy = "-1",
                //addDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //editDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                from = "MACHINE-REGISTER-API",
                apiVersion = sysConfig.ApiVersion
            };
            jsonArray.Add(machineParaData);
            string jsonData = JsonTool.ToJson(jsonArray);
            LogTool.Debug($"vision注册发送数据{jsonData}", "visionlog记录");
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });
        }


        TrainRunMode dstStatusMemory = TrainRunMode.Idle;

        /// <summary>
        /// 设备状态变更
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public override void StatusChanged(IMotionController mController, EngineStatus src, EngineStatus dst)
        {

            base.StatusChanged(mController, src, dst);
            try
            {
                if (mController != null)
                {
                    var srcStatus = ConvertToMode(src);
                    var dstStatus = ConvertToMode(dst);
                    string errCode = "";
                    string errInfo = "";
                    // 2025-5-7 wyy
                    if (srcStatus != dstStatus && !sysConfig.HiveEnabled && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑")))  // 生产和空跑下均会上传状态变化
                    {
                        //errcode 配置问题
                        if (mController.AlarmInfo != null)
                        {
                            var err = _errorMangaer.GetErrorDetail(mController.AlarmInfo, "Hive");
                            errCode = err.Code;
                            errInfo = err.Message;
                        }
                        if (bIsFirstOpen)
                        {
                            //首次开机，补传缺失的Down->Idle状态
                            bIsFirstOpen = false;
                            MachineStatusUpload(TrainRunMode.Down, srcStatus, "", "");
                            _visionState.VisionMachineState = srcStatus;
                        }
                        //检查上传状态并过滤
                        CheckUpStatus(srcStatus, dstStatus, errCode, errInfo);
                    }

                    //暂停，vision不再发状态切换
                    if (dst == EngineStatus.Pause) return;

                    //下一次的起始状态不等于上一次的结束状态，直接返回
                    if (srcStatus != dstStatusMemory)
                    {
                        return;
                    }
                    //保存目标状态
                    dstStatusMemory = dstStatus;

                    // 2025-5-7 注釋掉
                    ////Vision共用Hive的errorcode
                    ////need to do 
                    ////errcode 配置问题
                    //if (mController.AlarmInfo != null)
                    //{
                    //    var err = _errorMangaer.GetErrorDetail(mController.AlarmInfo, "Hive");
                    //    errCode = err.Code;
                    //    errInfo = err.Message;
                    //}

                    //// 停机后不在上传
                    //if (/*dst == DataStruct.EngineStatus.Stop ||*/ srcStatus == dstStatus)
                    //{
                    //    return;
                    //}

                    ////if (errCode == "" || !errCode.Contains("-"))
                    ////{
                    ////    errCode = "C02PNCY-05-01";
                    ////    errInfo = "Pin Cylinder Extend Fail";
                    ////}
                    //MachineStatusUpload(srcStatus, dstStatus, errCode, errInfo);
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }


            //软件开启时第一次上传所有参数
            if (!IsUploadPara)
            {
                MachineParaUpload("FirstLoad", "1", "1");
                IsUploadPara = true;
            }
        }


        public void CheckUpStatus(TrainRunMode src, TrainRunMode dst, string errCode, string errInfo)
        {
            // 1) 归一化非法枚举值，>5 统一映射为 Down
            if ((int)src > 5) src = TrainRunMode.Down;
            if ((int)dst > 5) dst = TrainRunMode.Down;

            // 2) 过滤无效切换：源与目标一致且不是 Down，则无需上传
            if (src == dst && dst != TrainRunMode.Down) return;

            switch (_visionState.VisionMachineState)
            {
                case 0:
                    _visionState.VisionMachineState = dst;
                    break;

                case TrainRunMode.Running: // Running
                    switch (dst)
                    {
                        case TrainRunMode.Idle: // Idle
                            _visionState.VisionMachineState = TrainRunMode.Idle;
                            MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Idle, "", "");
                            break;

                        case TrainRunMode.Down: // Down
                            _visionState.VisionMachineState = TrainRunMode.Down;
                            MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Down, errCode, errInfo);
                            break;

                        case TrainRunMode.Running: // Running
                            _visionState.VisionMachineState = TrainRunMode.Running;
                            break;
                    }
                    break;

                case TrainRunMode.Idle: // Idle
                    switch (dst)
                    {
                        case TrainRunMode.Running: // Idle->Running
                            _visionState.VisionMachineState = TrainRunMode.Running;
                            MachineStatusUpload(TrainRunMode.Idle, TrainRunMode.Running, "", "");
                            return;

                        case TrainRunMode.Down: // Idle->Down
                            _visionState.VisionMachineState = TrainRunMode.Down;
                            MachineStatusUpload(TrainRunMode.Idle, TrainRunMode.Down, errCode, errInfo);
                            break;
                    }
                    break;

                case TrainRunMode.Down: // Down
                    switch (dst)
                    {
                        case TrainRunMode.Idle: // Down不会->Idle
                            break; // 规避穿透原则
                        case TrainRunMode.Running: // Down->Running
                            _visionState.VisionMachineState = TrainRunMode.Running;
                            _visionState.VisionErrorCode = "";
                            _visionState.VisionErrorMessage = "";
                            MachineStatusUpload(TrainRunMode.Down, TrainRunMode.Running, "", "");
                            //if (mController.MachineStatus != EngineStatus.Ready && mController.MachineStatus != EngineStatus.Pause && mController.MachineStatus != EngineStatus.Running)
                            //{
                            //    return;
                            //}
                            //MachineStatusUpload(TrainRunMode.Down, TrainRunMode.Running, "", "");
                            break;
                        case TrainRunMode.Down:
                            _visionState.VisionMachineState = TrainRunMode.Down;
                            break;
                    }
                    break;

            }

        }
        /// <summary>
        /// 3.设备状态
        /// </summary>
        public void MachineStatusUpload(TrainRunMode src, TrainRunMode dst, string errCode, string errInfo)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineStatus");
            ArrayList paramList = new ArrayList();

            // 构建数据
            ArrayList jsonArray = new ArrayList();
            if (!errCode.Contains("-"))
            {
                errCode = "";
                errInfo = "";
            }
            if (errInfo.Contains("&"))
            {
                var errMsg1 = errInfo.Split('&');
                errInfo = errMsg1[0];
            }
            if (errCode.Contains("@"))
            {
                var errMsg1 = errCode.Split('@');
                errCode = errMsg1[0];
                errInfo = errMsg1[1];

            }
            // Vision Status 切换不能用数字
            var machineParaData = getParaData(src, dst, errCode, errInfo);
            jsonArray.Add(machineParaData);
            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        private object getParaData(TrainRunMode src, TrainRunMode dst, string errCode, string errInfo)
        {
            return new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                //V1.0.2
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                fromStatus = ((int)src > 5 ? TrainRunMode.Down : src).ToString(),
                toStatus = ((int)dst > 5 ? TrainRunMode.Down : dst).ToString(),
                statusDescription = errInfo,
                status = ((int)dst > 5 ? 5 : (int)dst).ToString(),
                errorCode = dst != TrainRunMode.Down ? "" : (string.IsNullOrEmpty((src == TrainRunMode.Running || src == TrainRunMode.Idle) ? errCode : string.Empty) ? "F99OOOO-08" : (src == TrainRunMode.Running || src == TrainRunMode.Idle) ? (errCode) : string.Empty),
                errorMessage = dst != TrainRunMode.Down ? "" : (string.IsNullOrEmpty((src == TrainRunMode.Running || src == TrainRunMode.Idle) ? errInfo : string.Empty) ? "Operator Stopped for repair" : (src == TrainRunMode.Running || src == TrainRunMode.Idle) ? errInfo : string.Empty),
                maintainerName = "ZS",
                maintenanceId = "1",
                maintenanceType = "",
                from = "MACHINE-STATUS-API",
                apiVersion = sysConfig.ApiVersion
            };
        }

        /// <summary>
        /// 4.设备生产指标
        /// </summary>
        public override void MachineIndicatorUpload()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineIndicator");
            ArrayList paramList = new ArrayList();
            int tossCount = 0;
            if (productStat == null) return;
            foreach (int val in productStat.ThrowMaterialDic.Values)
            {
                tossCount += val;
            }
            // 构建数据
            ArrayList jsonArray = new ArrayList();
            var machineParaData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                startTime = DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                inputQty = productStat.AllCount.ToString(),
                outputQty = productStat.AllCount.ToString(),
                passQty = productStat.OKCount.ToString(),
                failQty = productStat.NGCount.ToString(),
                tossingQty = tossCount.ToString(),
                retestQty = "2",
                targetUph = "600",
                uph = productStat.UPH.ToString(),
                yield = productStat.GetYield().ToString(),
                tossingRate = productStat.GetThrowRate(tossCount).ToString(),
                retestRate = "0.0222",
                from = "MACHINE-INDICATOR-API",
                apiVersion = sysConfig.ApiVersion,
            };
            jsonArray.Add(machineParaData);
            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        /// <summary>
        /// 设备参数变更
        /// 5、设备控制参数 API 接口
        /// </summary>
        public void MachineParaUpload(string name, object srcV, object newV, string barcode = "")
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineControl");
            string jsonData = JsonTool.ToJson(GetDeviceControlPara(name, srcV, newV, barcode));

            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        /// <summary>
        /// 获取设备控制参数
        /// </summary>
        /// <returns></returns>
        private object GetDeviceControlPara(string changeName, object srcV, object newV, string barcode = "")
        {
            ArrayList ctList = new ArrayList();

            var vAxises = mController.MotionEngine.DeviceEngine.GetVDevices<VAxis>();

            //上传轴参数相关
            foreach (var item in vAxises)
            {

                foreach (var pItem in item.Positions)
                {
                    if (changeName != $"{item.Name}_{pItem.Name}")
                    {
                        var addContent = new
                        {
                            parameter = $"{item.Name}_{pItem.Name}",
                            preValue = pItem.Position,
                            aftValue = pItem.Position,
                            changeReason = "NoRepair"
                        };
                        ctList.Add(addContent);
                    }
                    else
                    {
                        var addChangedContent = new
                        {
                            parameter = $"{item.Name}_{pItem.Name}",
                            preValue = srcV,
                            aftValue = newV,
                            changeReason = "Repair"
                        };
                        ctList.Add(addChangedContent);
                    }
                }

                //位置

                //速度

                var addSpdContent = new
                {
                    parameter = $"{item.Name}_Speed",
                    preValue = item.MoveSpeed,
                    aftValue = item.MoveSpeed,
                    changeReason = "NoRepair"
                };
                ctList.Add(addSpdContent);

                //加速度
                var addAccContent = new
                {
                    parameter = $"{item.Name}_Acc",
                    preValue = item.Acc,
                    aftValue = item.Acc,
                    changeReason = "NoRepair"
                };
                ctList.Add(addAccContent);

                //减速度
                var addDecContent = new
                {
                    parameter = $"{item.Name}_Dec",
                    preValue = item.Dec,
                    aftValue = item.Dec,
                    changeReason = "NoRepair"
                };
                ctList.Add(addDecContent);

            }

            //上传PDCA启用,SFC启用
            if (changeName == PDCANAME)
            {
                bool.TryParse(newV?.ToString(), out var isEnabled);
                int enableVal = isEnabled ? 1 : 0;
                int revenableVal = isEnabled ? 0 : 1;
                var addContent = new
                {
                    parameter = $"启用PDCA",
                    //preValue = revenableVal,
                    //aftValue = enableVal,
                    preValue = 0,
                    aftValue = 1,
                    changeReason = "Repair"
                };
                ctList.Add(addContent);
            }


            if (changeName == SFCNAME)
            {
                bool.TryParse(newV?.ToString(), out var isEnabled);
                int enableVal = isEnabled ? 1 : 0;
                int revenableVal = isEnabled ? 0 : 1;
                var addContent = new
                {
                    parameter = $"启用SFC",
                    //preValue = revenableVal,
                    //aftValue = enableVal,
                    preValue = 0,
                    aftValue = 1,
                    changeReason = "Repair"
                };
                ctList.Add(addContent);
            }

            //上传Vision启用
            if (changeName == VISIONNAME)
            {
                bool.TryParse(newV?.ToString(), out var isEnabled);
                int enableVal = isEnabled ? 1 : 0;
                int revenableVal = isEnabled ? 0 : 1;
                var addContent = new
                {
                    parameter = $"开启VISION上传",
                    preValue = revenableVal,
                    aftValue = enableVal,
                    changeReason = "Repair"
                };
                ctList.Add(addContent);
            }
            else
            {
                var addContent = new
                {
                    parameter = $"开启VISION上传",
                    preValue = sysConfig.VisionEnabled ? 1 : 0,
                    aftValue = sysConfig.VisionEnabled ? 1 : 0,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }


            //启用Hive
            if (changeName == HIVENAME)
            {
                bool.TryParse(newV?.ToString(), out var isEnabled);
                int enableVal = isEnabled ? 1 : 0;
                int revenableVal = isEnabled ? 0 : 1;
                var addContent = new
                {
                    parameter = HIVENAME,
                    preValue = revenableVal,
                    aftValue = enableVal,
                    changeReason = "Repair"
                };
                ctList.Add(addContent);
            }
            else
            {
                var addContent = new
                {
                    parameter = HIVENAME,
                    preValue = sysConfig.HiveEnabled ? 1 : 0,
                    aftValue = sysConfig.HiveEnabled ? 1 : 0,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }

            //上传图片
            if (true)
            {
                var addContent = new
                {
                    parameter = $"上传图片",
                    preValue = 1,
                    aftValue = 1,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }

            //安全门功能开启
            if (true)
            {
                var addContent = new
                {

                    parameter = $"安全门启用",
                    preValue = IsShieldDoorMem ? 0 : 1,
                    aftValue = sysConfig.IsShieldDoor ? 0 : 1,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }
            //安全门屏蔽记忆
            IsShieldDoorMem = sysConfig.IsShieldDoor;
            if (true)
            {
                var addContent = new
                {
                    parameter = $"生产模式启用",
                    preValue = IsProductModeMem ? 1 : 0,
                    aftValue = mController.GetCurrentMode().Contains("生产") ? 1 : 0,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }
            //0517Error
            IsProductModeMem = mController.GetCurrentMode().Contains("生产");

            ///路由检查开启
            if (true)
            {
                var addContent = new
                {
                    parameter = $"路由检查开启",
                    preValue = 1,
                    aftValue = 1,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }

            ///防呆检查开启
            if (true)
            {
                var addContent = new
                {
                    parameter = $"防呆检查开启",
                    preValue = 1,
                    aftValue = 1,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }

            ///防呆检查开启
            if (true)
            {
                var addContent = new
                {
                    parameter = $"扫码功能开启",
                    preValue = 1,
                    aftValue = 1,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }

            if (true)
            {
                var addContent = new
                {
                    parameter = $"取料补偿",
                    preValue = "0,0,0",
                    aftValue = "0,0,0",
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }

            if (true)
            {
                var addContent = new
                {
                    parameter = $"CCD曝光值",
                    preValue = "30",
                    aftValue = "30",
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }

            //空跑模式启用
            if (true)
            {
                var addContent = new
                {
                    parameter = $"空跑模式启用",
                    preValue = IsDryRunModeMem ? 1 : 0,
                    aftValue = mController.GetCurrentMode().Contains("空跑") ? 1 : 0,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
            }
            IsDryRunModeMem = mController.GetCurrentMode().Contains("空跑");

            //首件模式启用
            if (true)
            {
                bool firstPieceEnabled = false;
                var globalModule = mController.MotionEngine.Get(GlobalModule.GlobalID);
                if (globalModule != null)
                {
                    foreach (var p in globalModule.Parameters)
                    {
                        if (p.Key == "Extend_首件启用" && p.Value != null)
                        {
                            firstPieceEnabled = (bool)p.Value.Value;
                            break;
                        }
                    }
                }
                var addContent = new
                {
                    parameter = $"首件模式启用",
                    preValue = IsFirstPieceModeMem ? 1 : 0,
                    aftValue = firstPieceEnabled ? 1 : 0,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
                IsFirstPieceModeMem = firstPieceEnabled;
            }

            //压力监控启用
            if (true)
            {
                bool pressureEnabled = MapDatas?.Any(m => m.PositivestandardDeviation != 0 || m.NegativestandardDeviation != 0) ?? false;
                var addContent = new
                {
                    parameter = $"压力监控启用",
                    preValue = IsPressureMonitorMem ? 1 : 0,
                    aftValue = pressureEnabled ? 1 : 0,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
                IsPressureMonitorMem = pressureEnabled;
            }

            //机器人启用
            if (true)
            {
                bool robotEnabled = !(mController.SysConfig.RobotStart?.IsEmpty() ?? true);
                var addContent = new
                {
                    parameter = $"机器人启用",
                    preValue = IsRobotEnabledMem ? 1 : 0,
                    aftValue = robotEnabled ? 1 : 0,
                    changeReason = "NoRepair"
                };
                ctList.Add(addContent);
                IsRobotEnabledMem = robotEnabled;
            }

            //根据FX的需求,CGL CGSFA CGSF等工站临时添加机器人速度,定值
            var vComms = mController.MotionEngine.DeviceEngine.GetVDevices<VCommuncation>();
            foreach (var item in vComms)
            {
                if (item.Name.Contains("机器人") || item.Name.Contains("机械手"))
                {
                    var addSpdContent = new
                    {
                        parameter = $"Robot Speed",
                        preValue = 100,
                        aftValue = 100,
                        changeReason = "NoRepair"
                    };
                    ctList.Add(addSpdContent);
                    break;
                }
            }

            //获取aelimits上下限不再跟二维码相关
            double upLimit = 0;
            double downLimit = 0;

            foreach (MapData mapData in MapDatas)
            {
                upLimit = mapData.PositivestandardDeviation;
                downLimit = mapData.NegativestandardDeviation;
                if (upLimit != 0 || downLimit != 0)
                {

                    //参数上限值
                    var aelUplimit = new
                    {
                        parameter = $"{mapData.Alias}UpLimit",
                        preValue = upLimit,
                        aftValue = upLimit,
                        changeReason = "NoRepair"
                    };
                    ctList.Add(aelUplimit);
                    //参数下限值
                    var aelDownlimit = new
                    {
                        parameter = $"{mapData.Alias}DownLimit",
                        preValue = downLimit,
                        aftValue = downLimit,
                        changeReason = "NoRepair"
                    };
                    ctList.Add(aelDownlimit);
                }
                if (mapData.StandardValue != 0)
                {

                    var Dwell_Time = new
                    {
                        parameter = $"{mapData.Alias}Dwell_Time",
                        preValue = mapData.StandardValue,
                        aftValue = mapData.StandardValue,
                        changeReason = "NoRepair"
                    };
                    ctList.Add(Dwell_Time);
                }
            }
            // 增加作业模式的参数
            ctList.Add(new
            {
                parameter = "HomeWork_Mode",
                preValue = last_Homework_Mode,
                aftValue = Homework_Mode,
                changeReason = "NoRepair"
            });
            object jsonData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                vendor = sysConfig.VendorName,

                cellId = "1",
                //V1.0.2
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                appVersion = sysConfig.SoftVersion,
                editUser = editUser,
                startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss"),
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                from = "MACHINE-CONTROL-API",
                type = "R_TEST_PARAM",
                apiVersion = sysConfig.ApiVersion,
                data = ctList
            };
            ArrayList array = new ArrayList();
            array.Add(jsonData);
            return array;
        }


        /// <summary>
        /// 获取设备控制参数
        /// </summary>
        /// <returns></returns>
        private object GetSingleDeviceControlPara(string name, object srcV, object newV)
        {
            ArrayList ctList = new ArrayList();
            //创建工站开始
            var addContent = new
            {
                parameter = name,
                preValue = srcV,
                aftValue = newV,
                changeReason = "Repair"
            };
            ctList.Add(addContent);


            object jsonData = new
            {
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                appVersion = sysConfig.SoftVersion,
                editUser = "ZS",
                startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss"),
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                from = "MACHINE-CONTROL-API",
                type = "R_TEST_PARAM",
                apiVersion = sysConfig.ApiVersion,
                data = ctList
            };
            ArrayList array = new ArrayList();
            array.Add(jsonData);
            return array;
        }



        /// <summary>
        /// 产品过程参数
        /// 6、设备过程参数 API 接口
        /// </summary>
        public void ProductParaUpload(string SN, object data)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected || string.IsNullOrEmpty(data.ToString()))
            {
                OnLog($"产品过程参数,异常,ProductParaUpload,isConnected={isConnected},data={string.IsNullOrEmpty(data.ToString())}");
                return;
            }
            //if (!mController.GetCurrentMode().Contains("生产") && !mController.GetCurrentMode().Contains("CPK"))
            //{
            //    OnLog($"产品过程参数,异常,返回。当前模式={mController.GetCurrentMode()}");
            //    return;
            //}
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineParameter");
            string jsonData = JsonTool.ToJson(data);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                    OnLog($"产品过程参数，上传OK!", url);
                }
                else
                {
                    OnLog($"产品过程参数，上传Fail!，data={jsonData}", url);
                }
            });
        }

        /// <summary>
        /// 设备叫修
        /// 7、设备叫修 API 接口
        /// </summary>
        public void MachineCall(string description)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected || true) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineCall");
            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var machineCallData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                callType = "Yield",
                callDescription = description,
                callerName = "ZS",
                callerId = "1",
                callerType = "OP",
                from = "MACHINE-CONTROL-API",
                apiVersion = sysConfig.ApiVersion,
            };
            jsonArray.Add(machineCallData);
            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        /// <summary>
        /// 设备维护
        /// 8、设备维护回复 API 接口
        /// </summary>
        public void MachineMaintain(string description)
        {
            // 如果没有连接上，不触发通讯
            //不启用
            if (!isConnected || true) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineMaintain");
            // 构建数据
            ArrayList jsonArray = new ArrayList();
            var machineCallData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                maintainType = "Other",
                maintainDescription = "NA",
                startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                maintainerName = "ZS",
                maintainerId = "1",
                maintainerType = "AE",
                from = "MACHINE-CONTROL-API",
                apiVersion = sysConfig.ApiVersion,
            };
            jsonArray.Add(machineCallData);
            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }


        /// <summary>
        /// 设备点检
        /// 9、设备点检 API 接口
        /// </summary>7
        public void MachineInspect()
        {            // 如果没有连接上，不触发通讯
            if (!isConnected || true) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineInspect");

            // 构建数据
            ArrayList jsonArray = new ArrayList();
            var machineInspectData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.VisionStationId,
                cellId = "1",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                inspectName = "ZS",
                inspectId = "1",
                inspectType = "AE",
                from = "MACHINE-CONTROL-API",
                apiVersion = sysConfig.ApiVersion,
                data = "[{},{},{}]",
            };
            jsonArray.Add(machineInspectData);
            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }


        /// <summary>
        /// 10.产品维度接口
        /// </summary>
        public void ProductInfoColletct(StationResult stationResult, out string WIP)
        {
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/productInformation");
            WIP = string.IsNullOrEmpty(stationResult.WIP) ? "N999999999" : stationResult.WIP;

            //if (!mController.GetCurrentMode().Contains("生产")) return;
            //OnLog($"生产模式下可以上传", url);
            //如果前站NG,不上传Vision
            if (!string.IsNullOrEmpty(stationResult.NgCode))
            {
                if (stationResult.NgCode.ToUpper().Contains("UNIT"))
                {
                    OnLog($"NG代码包含Unit,不可以上传", stationResult.NgCode);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(stationResult.ErrMsg))
            {
                if (stationResult.ErrMsg.ToUpper().Contains("UNIT"))
                {
                    OnLog($"NG内容包含Unit,不可以上传", stationResult.ErrMsg);
                    return;
                }
            }

            // OnLog($"NG原因不包含Unit,可以上传", url);

            // 如果没有连接上，不触发通讯
            //if (!isConnected) return;
            if (!isConnected)
            {
                OnLog("$Vision连接失败,尝试上传", url);
            }
            {
                //OnLog($"Vision已经连上,可以上传", url);
            }

            string wip = WIP;
#if false
            //如果未扫到码，直接赋值 N999999999
            if (stationResult.ProCode.StartsWith("NG_") && stationResult.ProCode.Length == 20)
            {
                wip = "N999999999";
            }
            else
            {
                //WIP从出料事件传入，无需再查数据
                if (!stationResult.Datas.ContainsKey("WIP"))
                {
                    var cacheItem = _cacheManager.GetItem(stationResult.ProCode);
                    if (cacheItem != null && cacheItem is CacheItem cItem)
                    {
                        bool isExist = cItem.Any(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                        if (isExist)
                        {
                            var wipItem = cItem.FirstOrDefault(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                            if (wipItem.Value != null)
                            {
                                wip = wipItem.Value.ToString();
                                WIP = wip;
                            }
                        }
                    }
                }
                else
                {
                    wip = stationResult.Datas["WIP"].ToString();
                    WIP = wip;
                }

                if (String.IsNullOrEmpty(wip))
                {
                    wip = "N999999999";
                }
                WIP = wip;
            }
#endif

            // 构建数据
            ArrayList jsonArray = new ArrayList();
            // 出料事件的BarCode 产品码，界面为空时，会赋值 NG，不会为空
            string keypartsSn = "";//stationResult.ProCode;
            int keypartsSnCount = 0;
            //如果出料时，本站结果为OK，则把NGCode和Errmsg全部清空，防止任务里面忘记清空
            if (stationResult.Result && !stationResult.IsToss)
            {
                stationResult.NgCode = "";
                stationResult.ErrMsg = "";
                ///keypartsSn = "";
            }

            // OnLog($"Vision开始组织内容", url);

            //抛小料时无WIP,按客户写成IP是N888888888
            if (stationResult.Datas.ContainsKey("IsSmallPart"))
            {
                return;
                wip = "N888888888";
                WIP = wip;
                //OnLog($"Vision上传，ZZ54要求抛小料，取消上传", url);
            }

            List<object> tossingList = new List<object>();

            string tossSn = "";
            if (stationResult.IsToss)
            {
                tossSn += wip;
                keypartsSn = !stationResult.ProCode.ToUpper().Contains("NG") ? stationResult.ProCode : "";
                keypartsSnCount++;
                tossingList.Add(new
                {
                    excepDesc = stationResult.ErrMsg,
                    tossingSnList = keypartsSn,
                    tossingSnCnt = "1",
                    snType = "Keypart"
                });
            }

            var keyPartList = stationResult.TossDatas?.Where(a => !string.IsNullOrEmpty(a.Item2) && a.Item2 != "NULL").ToList();
            if (keyPartList?.Count() > 0)
            {
                var listGroup = keyPartList.Where(a => a.Item1 != null).GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => a.Select(a => a.Item2).ToList());
                foreach (var item in listGroup)
                {
                    if (item.Value.Count() > 0)
                    {
                        string snList = String.Join(",", item.Value); //Value is Item2, Item2 is not null or NULL
                        if (!String.IsNullOrEmpty(snList))
                            keypartsSn = String.IsNullOrEmpty(keypartsSn) ? snList : keypartsSn + "," + snList;
                        keypartsSnCount += item.Value.Count();
                        tossingList.Add(new
                        {
                            excepDesc = item.Key,
                            tossingSnList = snList,
                            tossingSnCnt = item.Value.Count().ToString(),
                            snType = "Keypart"
                        });
                    }
                }
            }

            string lotsSn = "";
            int lotsSnCount = 0;
            var lotPartList = stationResult.TossDatas?.Where(a => string.IsNullOrEmpty(a.Item2) || a.Item2 == "NULL").ToList();
            if (lotPartList?.Count() > 0)
            {
                var listGroup = lotPartList.Where(a => a.Item1 != null).GroupBy(a => a.Item1).ToDictionary(a => a.Key, a => a.Select(a => a.Item2).ToList());
                foreach (var item in listGroup)
                {
                    if (item.Value.Count() > 0)
                    {
                        string snList = String.Join(",", item.Value.Where(a => !string.IsNullOrEmpty(a) && a != "NULL"));
                        if (!String.IsNullOrEmpty(snList))
                            lotsSn = String.IsNullOrEmpty(lotsSn) ? snList : lotsSn + "," + snList;
                        lotsSnCount += item.Value.Count();
                        tossingList.Add(new
                        {
                            excepDesc = item.Key,
                            tossingSnList = snList,
                            tossingSnCnt = item.Value.Count().ToString(),
                            snType = "LotNo"
                        });
                    }
                }
            }
#if false
            // 根据变量塞 0~2 个对象
            if (!string.IsNullOrEmpty(keypartsSn) && stationResult.Datas.ContainsKey("IsSmallPart"))
            {
                tossingList.Add(new
                {
                    excepDesc = "Desc1",
                    tossingSnList = keypartsSn,
                    tossingSnCnt = "1",
                    snType = "Keypart"
                });
                tossingList.Add(new
                {
                    excepDesc = "Desc2",
                    tossingSnList = "",
                    tossingSnCnt = "1",
                    snType = "LotNo"
                });
            }
            else if (!string.IsNullOrEmpty(keypartsSn))
            {
                tossingList.Add(new
                {
                    excepDesc = "Desc1",
                    tossingSnList = keypartsSn,
                    tossingSnCnt = "1",
                    snType = "Keypart"
                });
            }
            else if (stationResult.Datas.ContainsKey("IsSmallPart"))
            {
                tossingList.Add(new
                {
                    excepDesc = "Desc1",
                    tossingSnList = "",
                    tossingSnCnt = "1",
                    snType = "LotNo"
                });
            }
#endif

            var machineInspectData = new
            {
                productSn = WIP,//二维码，
                locationId = "1",//穴号
                fxStationType = sysConfig.StationName_Vision,//
                inStationType = sysConfig.InsightType,
                machineSn = sysConfig.MachineSn,
                stationID = sysConfig.VisionStationId,
                cellId = "1",//工位序号
                result = stationResult.Result ? "PASS" : "FAIL", // 去掉Tossing结果，stationResult.IsToss ? "TOSSING" : 
                errorCode = stationResult.NgCode,//报错代码
                errorDesc = stationResult.ErrMsg,//报错描述
                startTime = stationResult.EnterTime == DateTime.MinValue ? DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-dd HH:mm:ss.fff") :
                                                                        stationResult.EnterTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                overlayVersion = sysConfig.BaliVersion,
                //V1.0.2
                appVersion = sysConfig.SoftVersion,
                isAudit = _motionController.GetCurrentMode().Contains("生产") ? 0 : _motionController.GetCurrentMode().Contains("空跑") ? 2 : 1,
                //资料来源
                form = "MACHINE-PRODUCT-API",
                apiVersion = sysConfig.ApiVersion,
                //物料名称
                //snList = stationResult.Datas.ContainsKey("ThrowMaterialName") ? stationResult.Datas["ThrowMaterialName"].ToString() : "",
                snList = string.IsNullOrEmpty(tossSn) ? "" : "NA;" + tossSn + ";NA",
                // 2025.11新增抛料相关内容
                keypartsSn = keypartsSn,
                keypartsSnCount = keypartsSnCount.ToString(), //string.IsNullOrEmpty(keypartsSn) ? "0" : "1",
                lotNoSn = lotsSn,
                lotNoSnCount = lotsSnCount.ToString(), //stationResult.Datas.ContainsKey("IsSmallPart") ? "1" : "0",
                tossingDesc = tossingList.ToArray()
            };
            jsonArray.Add(machineInspectData);
            string jsonData = JsonTool.ToJson(jsonArray);
            bool isUploadSuccess = false;
            //OnLog($"Vision内容组织完成，开始上传", url);

            for (int i = 0; i < 3; i++)
            {
                CommException(url, jsonData, r =>
                {
                    if (r.IsSuccess)
                    {
                        isUploadSuccess = true;
                    }
                });
                if (isUploadSuccess) return;
                Thread.Sleep(50);
            }

        }


        /// <summary>
        /// 11.图片上传接口
        /// </summary>
        /// <param name="picFolderPath">文件路径</param>
        /// <param name="barcode">二维码</param>
        /// <returns></returns>
        public string UploadPic(string picFolderPath, string barcode)
        {
            string path = "";
            path = Path.Combine(picFolderPath, barcode);
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();
            foreach (FileInfo pic in files)
            {
                resultModel = picUploadInstance.Upload(pic.DirectoryName);
            }
            return resultModel.errorMessage;
        }


        /// <summary>
        ///通过LCG码获取WIP码 
        /// </summary>
        /// <param name="Lcg"></param>
        /// <returns></returns>
        public string LcgToWip(string Lcg)
        {
            string wip = "None";
            var cacheItem = _cacheManager.GetItem(Lcg);
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
            return wip;
        }


        /// <summary>
        /// 通过LCG码获取治具码
        /// </summary>
        /// <param name="Lcg"></param>
        /// <returns></returns>
        public string LcgToCarrierId(string Lcg)
        {
            string carrierId = "N999999999";
            var cacheItem = _cacheManager.GetItem(Lcg);
            if (cacheItem != null && cacheItem is CacheItem cItem)
            {
                bool isExist = cItem.Any(u => u.Alias != null && u.Alias.ToUpper().Contains("CARRIER"));
                if (isExist)
                {
                    var wipItem = cItem.FirstOrDefault(u => u.Alias != null && u.Alias.ToUpper().Contains("CARRIER"));
                    if (wipItem.Value != null)
                    {
                        carrierId = wipItem.Value.ToString();
                    }
                }
            }
            return carrierId;
        }

        /// <summary>
        /// CT 信息
        /// </summary>
        public void MachineCT()
        {

        }


        #region 作业模式的参数定义和方法
        /// <summary>
        /// 厂商-姓名(上一次登录)
        /// </summary>
        private string last_editUser = "Vendor-Name";
        /// <summary>
        /// 作业模式OP/Sustaining/Admin（上一次登录）
        /// </summary>
        private string last_Homework_Mode = "OP";
        /// <summary>
        /// 厂商-姓名
        /// </summary>
        private string editUser = "Vendor-Name";
        /// <summary>
        /// 作业模式OP/Sustaining/Admin
        /// </summary>
        private string Homework_Mode = "OP";

        private string ConvertUserRoleToHomeworkMode(SystemRole? role)
        {
            return role switch
            {
                SystemRole.Operator => "OP",
                SystemRole.Maintenance => "Maintenance",
                SystemRole.Integrator => "Integrator",
                SystemRole.Admin => "Admin",
                _ => "OP"
            };
        }

        public void AuthService_RoleChanged(UserInfo role)
        {
            //参数变化时，上传信息必须包含刷卡人信息
            //产品加工完成，上传信息必须包含刷卡详细信息
            last_editUser = editUser.ToString();
            last_Homework_Mode = Homework_Mode.ToString();
            editUser = string.Concat(role?.Company, "-", role?.Name);
            Homework_Mode = ConvertUserRoleToHomeworkMode(role?.Role);
            //MachineParaUpload("控制参数上传", true, true);

        }
        #endregion
    }

    public class proResultData
    {

        public string materialSn;

        public string carrierSn;

        public string startTime;

        public string endTime;

        public string cycleTime;

        public string result;

        public string resultDescription;

        public string cgTossCount;

        public string tossReason;
    }
}
