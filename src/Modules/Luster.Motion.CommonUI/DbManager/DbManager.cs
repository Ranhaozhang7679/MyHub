#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DbManager
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.DbManager
* 文 件 名:       DbManager.cs
* 创建时间:       2022/8/9 8:40:33
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      692e20f5-1c79-4132-973c-547d10fa67ed
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/9 8:40:33
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Luster.Motion.CommonUI.Events;
using MS.WindowsAPICodePack.Internal;
using System.Windows.Media.Media3D;
using System.Collections;
using System.Globalization;
using Luster.TaskFlow.Common.Interfaces;
using Luster.Motion.Integration.AOI;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.Integration.Web;
using System.Diagnostics;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using HandyControl.Controls;

namespace Luster.Motion.CommonUI
{
    public class DbManager : IDbManager
    {
        /// <summary>
        /// 报警缓存时间跨度
        /// </summary>
        private class AlarmTimeCache
        {
            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public AlarmTimeCache(TbAlarm alarm)
            {
                StartTime = alarm.StartTime;
                EndTime = alarm.EndTime;
            }
        }

        // 2025-4-13 修改CT csv log保存路径
        //private readonly IWebService _webService;
        //private readonly IConfigManager _configManager;
        // 2025-4-24
        public WebConfig sysConfig = null;
      
        private AlarmTimeCache _alarmTimeCache;
        // 数据写入频率
        private const int DbFrequency = 50;
        // 缓存记录产品入站信息
        private List<TbProductInfo> productCashe = new List<TbProductInfo>();

        // 缓存产品出站信息
        private ConcurrentQueue<TbProductInfo> tbProductQueue = new ConcurrentQueue<TbProductInfo>();
        private ConcurrentQueue<TbThrow> tbThrowQueue = new ConcurrentQueue<TbThrow>();
        private ConcurrentQueue<TbAOIResult> tbAOIQueue = new ConcurrentQueue<TbAOIResult>();
        private ConcurrentQueue<TbChangeRecord> tbRecordQueue = new ConcurrentQueue<TbChangeRecord>();

        private IList<SaveProductModel> saveProductList = new List<SaveProductModel>();
        private IList<SaveTossingModel> saveTossingList = new List<SaveTossingModel>();
        private DateTime _lastClearTime;
        private IRepository _repository;
        private IMotionController _motionController;
        private IMotionEngine _motionEngine;
        private WhileTool whileTool = null;
        private ICacheManager _cacheManager;

        public DbManager(IRepository repository, IMotionController motionController, IMotionEngine motionEngine, ICacheManager cacheManager)
        {
            _repository = repository;
            _motionController = motionController;
            _cacheManager = cacheManager;
            _motionEngine = motionEngine;
            // 监听出料事件写入数据库
            motionEngine.ProUnloadedDBEvent -= MotionEngine_ProUnloadedEvent;
            motionEngine.ProUnloadedDBEvent += MotionEngine_ProUnloadedEvent;

            // 抛料写入数据库
            motionEngine.ProThrowEvent -= MotionEngine_ProThrowEvent;
            motionEngine.ProThrowEvent += MotionEngine_ProThrowEvent;

            //写入CT 统计
            motionEngine.TimeStatisEvent -= MotionEngine_TimeStatisEvent;
            motionEngine.TimeStatisEvent += MotionEngine_TimeStatisEvent;

            motionEngine.MapDataChangeEvent -= MotionEngine_MapDataChangeEvent;
            motionEngine.MapDataChangeEvent += MotionEngine_MapDataChangeEvent;

            MotionEngine_MapDataChangeEvent();

            // AOI 数据缓存
            AOIHelper.Instance.DbSaveEvent -= Instance_DbSaveEvent;
            AOIHelper.Instance.DbSaveEvent += Instance_DbSaveEvent;

            _lastClearTime = DateTime.Now;
            whileTool = new WhileTool(true);

            System.Threading.Tasks.Task.Run(() =>
            {
                WriteProductInfoToDB();
            });   
        }
        string oldHeader = "";
        // 选择一个可用的磁盘作为日志存储路径的根目录
        public string PickAvailableDrive()
        {
            var allFixed = DriveInfo.GetDrives()
                                    .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                                    .ToList();

            // 1. 必须用 E:
            var eDrive = allFixed.FirstOrDefault(d => d.Name.StartsWith(@"E:\", StringComparison.OrdinalIgnoreCase));
            if (eDrive != null) return eDrive.Name;

            // 2. 其次用 D:
            var dDrive = allFixed.FirstOrDefault(d => d.Name.StartsWith(@"D:\", StringComparison.OrdinalIgnoreCase));
            if (dDrive != null) return dDrive.Name;

            // 3. 兜底：剩余空间最大的盘【改为exe程序所在的盘】
            var fallback = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            // var fallback = allFixed.OrderByDescending(d => d.TotalFreeSpace).FirstOrDefault();
            if (fallback != null) return fallback;

            return "";
        }
        public void WriteParameterToCSV() // List<VAxis> vAxises
        {
            try
            {
                List<VAxis> vAxises = _motionEngine.DeviceEngine.GetVDevices<VAxis>();
                //if (vAxises == null || !vAxises.Any()) return;
                //    var headers = vAxises.SelectMany(a => new[]
                //    {
                //    $"{a.Name}_速度",
                //    $"{a.Name}_加速度",
                //    $"{a.Name}_减速度"
                //    }).ToArray();
                // 固定列
                var fixedCols = new[] { "start_time", "end_time", "Hive启用", "Vision启用" };
                // 动态列
                var dynamicCols = vAxises.SelectMany(a => new[]
                {
                    $"{a.Name}_速度",
                    $"{a.Name}_加速度",
                    $"{a.Name}_减速度",
                    $"{a.Name}_速度百分比",
                    $"{a.Name}_脉冲比"
                });
                // 功能参数动态列
                //List<string> funcPara = _motionController.SysConfig?.EnableDisableVarNames ?? new List<string>(); // .Any()
                Dictionary<string, bool> funcPara = _motionController.SysConfig?.EnableDisableVarValues ?? new Dictionary<string, bool>();
                // 1. 筛完的新字典
                var funcParaSelect = funcPara
                    .Where(kv =>
                           kv.Key.IndexOf("CPK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           kv.Key.IndexOf("PDCA", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           kv.Key.IndexOf("SFC", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                // 2. 只拿键做表头序列
                var dynamicColsFunc = funcParaSelect.Keys;   // IEnumerable<string>
                // 最终表头
                var newHeaderLine = string.Join(",", fixedCols.Concat(dynamicCols).Concat(dynamicColsFunc));

                string folder = "";
                if (string.IsNullOrEmpty(_motionController.FileConfig.LogsSavePath))
                {
                    string StatName = sysConfig?.StationName_Vision; // StationName
                    if (string.IsNullOrEmpty(StatName))
                    {
                        sysConfig = _motionController.WebService.GetConfig() as WebConfig;
                        StatName = sysConfig?.StationName_Vision;
                    }
                    var root = PickAvailableDrive();
                    folder = Path.Combine(root, StatName ?? "DefaultStation", "LUSTER", "Parameter");
                    //folder = Path.Combine($"E:\\{StatName}", "LUSTER", "Parameter");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
                else
                {
                    folder = Path.Combine(_motionController.FileConfig.LogsSavePath, "Parameter");
                }
                var filename = Path.Combine(folder, "Parameter" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv");
                // 3. 文件不存在 → 写表头+数据
                if (!File.Exists(filename))
                {
                    File.AppendAllText(filename, newHeaderLine + Environment.NewLine, Encoding.UTF8);
                    AppendData(filename, vAxises, sysConfig, funcParaSelect);
                    oldHeader = newHeaderLine;
                    return;
                }
                // 4. 文件已存在 → 读旧表头（第一行）- 存全局变量
                //string oldHeader;
                //using (var sr = new StreamReader(fileName, Encoding.UTF8))
                //    oldHeader = sr.ReadLine() ?? "";

                // 5. 表头变化 → 先写一条新表头；不变 → 直接追加数据
                //if (oldHeader != newHeaderLine)
                //{
                //    File.AppendAllText(filename, newHeaderLine + Environment.NewLine, Encoding.UTF8);
                //}

                AppendData(filename, vAxises, sysConfig, funcParaSelect);
                //using (var sw = new StreamWriter(filename, false, Encoding.UTF8))
                //{
                //    sw.WriteLine(string.Join(",", headers));

                //    var values = vAxises.SelectMany(a => new[]
                //    {
                //            a.MoveSpeed.ToString("F3"),
                //            a.Acc.ToString("F3"),
                //            a.Dec.ToString("F3")
                //        }).ToArray();

                //    sw.WriteLine(string.Join(",", values));
                //}
            }
            catch (Exception ex)
            {
                // 记录日志即可，别让软件起不来
                //Log.Error(ex, "写轴快照 CSV 失败");
            }
        }
        // 纯追加一行数据
        private void AppendData(string fileName, List<VAxis> vAxises, WebConfig sysConfig, Dictionary<string, bool> funcPara)
        {
            // 假设 start/end 你已经在别处算好
            string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            string endTime = DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss:fff");
            int hiveEn = sysConfig.HiveEnabled ? 1 : 0;
            int visionEn = sysConfig.VisionEnabled ? 1 : 0;

            // 固定值
            var fixedValues = new[] { startTime, endTime, hiveEn.ToString(), visionEn.ToString() };

            // 轴值
            var axisValues = vAxises.SelectMany(a => new[]
            {
                a.MoveSpeed.ToString("F3"),
                a.Acc.ToString("F3"),
                a.Dec.ToString("F3"),
                a.SpeedPercent.ToString("F3"),
                a.PerPluse.ToString("F3")
            });

            // “功能设定”参数值
            var funcValues = funcPara.SelectMany(a => new[]{ (a.Value ? 1 : 0).ToString() });

            var line = string.Join(",", fixedValues.Concat(axisValues).Concat(funcValues));
            File.AppendAllText(fileName, line + Environment.NewLine, Encoding.UTF8);

            //var line = string.Join(",", vAxises.SelectMany(a => new[]
            //{
            //    a.MoveSpeed.ToString("F3"),
            //    a.Acc.ToString("F3"),
            //    a.Dec.ToString("F3")
            //}));
            //File.AppendAllText(fileName, line + Environment.NewLine, Encoding.UTF8);
        }

        /// <summary>
        /// CSV 配置文件读取
        /// </summary>
        public Dictionary<string, string> ctConfigs = new Dictionary<string, string>();
        List<string> listA = new List<string>();
        // by模块（自由工站）存储CTLog的TimeSlot
        public Dictionary<string, DateTime> timeSlotByModule = new Dictionary<string, DateTime>();
        public void LoadCtConfig(string Path)
        {
            try
            {
                string csvPath = System.IO.Path.Combine(Path, "CtLogConfig.csv");
                if (File.Exists(csvPath))
                {
                    // 本地测试UTF8，现场测试需要Default
                    var lines = File.ReadAllLines(csvPath, Encoding.Default);
                    foreach (var line in lines)
                    {
                        var strs = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (strs.Length > 1)
                        {
                            // 支持CtLogConfig新增一倍行，两行为一组，取第一行的开始时间和第二行的结束时间，作为第二列模块的开始和结束时间
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
                        //string startt = $"{stationName}-Start";
                        if (!listA.Contains(stationName))
                        {
                            listA.Add(stationName); // 如果工站名不在 listA 中，再添加进去
                            //listA.Add(startt);
                            timeSlotByModule[stationName] = DateTime.MinValue;
                        }
                    }
                }               
            }
            catch (Exception ex)
            {
                //OnLog($"CtConfig.csv,读取异常。ex={ex.StackTrace}", "machineParameter");
            }
        }

        /// <summary>
        /// AOI 数据存储
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_DbSaveEvent(AOIResult obj)
        {
            tbAOIQueue.Enqueue(obj);
            whileTool.Continue();
        }

        /// <summary>
        /// 表头
        /// </summary>
        List<string> headers = new List<string>();
        private void MotionEngine_MapDataChangeEvent()
        {
            headers = _motionController.MotionEngine.MapDatas.GroupBy(u => u.Alias).Select(u => u.Key).ToList();
        }

        /// <summary>
        /// 新增变更记录
        /// </summary>
        /// <param name="tbRecord"></param>
        public void AddChangeRecord(TbChangeRecord tbRecord)
        {
            tbRecordQueue.Enqueue(tbRecord);
            whileTool.Continue();
        }


        #region 监听事件异步写入数据库
        public void AddSysOperation(SystemOperation systemOperation, string memo = "", string user = "Operator")
        {
            var operation = new TbSysOperation() { CreateTime = DateTime.Now };
            operation.Operation = systemOperation.GetDescription();
            operation.Memo = memo;
            operation.User = user;
            _repository.Insert(operation);
        }


        /// <summary>
        /// 写入抛料信息
        /// </summary>
        /// <param name="module"></param>
        /// <param name="material"></param>
        private void MotionEngine_ProThrowEvent(StationResult stationResult, string module, string material)
        {

            var tbThrow = new TbThrow() { CreateTime = DateTime.Now, SNCode = stationResult.ProCode, Wip = stationResult.WIP };
            tbThrow.Station = module;
            tbThrow.Material = material;
            //tbThrow.Reason = "产品NG";
            tbThrow.Reason = stationResult.ErrMsg;
            tbThrow.Mode = _motionController.GetCurrentMode().Contains("生产") ? 0 : _motionController.GetCurrentMode().Contains("空跑") ? 2 : 1;

            tbThrowQueue.Enqueue(tbThrow);
            whileTool.Continue();
        }

        /// <summary>
        /// 出料事件写入数据 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void MotionEngine_ProUnloadedEvent(IMotionModule module, StationResult result, Dictionary<string, object> data, double ct)
        {
            // 构建一个产品对象
            var product = new TbProductInfo()
            {
                CreateTime = DateTime.Now,
                EnterTime = result.EnterTime,
                SNCode = result.ProCode,
                Jig = result.JigCode,
                Wip = result.WIP,
            };

            product.ImagePath = result.ImagePath;

            if (result.IsToss)
            {
                product.IsToss = result.IsToss;
            }

            if (result.Result)
            {
                product.Result = "OK";
            }
            else
            {
                product.Result = "NG";
            }

            if (data != null && data.Count > 0)
            {
                var dataStr = new List<string>();
                foreach (var item in data)
                {
                    dataStr.Add($"{item.Key}:{item.Value}");
                }

                product.Data = string.Join("|", dataStr);
            }

            product.OutTime = DateTime.Now;
            product.Mode = _motionController.GetCurrentMode().Contains("生产") ? 0 : _motionController.GetCurrentMode().Contains("空跑") ? 2 : 1;
            product.CT = Math.Round((product.OutTime - product.EnterTime).TotalSeconds, 2);
            product.NewCT = ct;//20250424 
            product.CT = product.CT < 0 ? 0 : product.CT;
            product.NgReason = result.ErrMsg;
            product.NgCode = result.NgCode;

            tbProductQueue.Enqueue(product);
            whileTool.Continue();
        }

        private void WriteProductInfoToDB()
        {
            whileTool.While(() =>
            {
                TbProductInfo product = null;
                bool isRun = !tbProductQueue.IsEmpty && tbProductQueue.TryDequeue(out product);
                if (isRun)
                {
                    if (product.ID > 0)
                    {
                        _repository.Update(product);
                    }
                    else
                    {
                        _repository.Insert(product);
                    }
                    // 实时产品数据写入Product的csv文件中
                    if(false == product.IsToss)
                    {
                        AutoSaveProductInfo(product);
                    }
                    else
                    {
                        AutoSaveTossingInfo(product);
                    }

                    if (tbProYeildCache != null)
                    {
                        int rowNum = _repository.Update<TbProductYeild>(tbProYeildCache);
                        if (rowNum == 0)
                        {
                            _repository.Insert(tbProYeildCache);
                        }
                    }
                }

                // 添加进缓存
                TbThrow throwCache = null;
                if (tbThrowQueue.TryDequeue(out throwCache))
                {
                    // 告诉循环运行过了
                    isRun = true;
                    _repository.Insert(throwCache);
                    AutoSaveTossingInfo(throwCache);
                }

                // AOI 结果信息
                if (tbAOIQueue.TryDequeue(out var tbAOI))
                {
                    isRun = true;
                    _repository.SaveAOIResult(tbAOI);
                }

                // 新增队列记录
                if (tbRecordQueue.TryDequeue(out var tbRecord))
                {
                    isRun = true;
                    _repository.Insert(tbRecord);
                }

                return isRun;
            });
        }

        private void AutoSaveProductInfo(TbProductInfo info)
        {
            if (headers.Count == 0)
            {
                MotionEngine_MapDataChangeEvent();
            }

            var saveModel = new SaveProductModel(info, headers);
            // 获取当前班次信息
            var className = GetClassName(DateTime.Now);

            // 获取文件产品信息存储路径
            //var folder = GetFolderPath(true);
            // Vision要求，产品信息存到E盘固定路径
            string folder = "";
            if (string.IsNullOrEmpty(_motionController.FileConfig.LogsSavePath))
            {
                string StatName = sysConfig?.StationName_Vision;
                if (string.IsNullOrEmpty(StatName))
                {
                    sysConfig = _motionController.WebService.GetConfig() as WebConfig;
                    StatName = sysConfig?.StationName_Vision;
                }
                var root = PickAvailableDrive();
                folder = Path.Combine(root, StatName ?? "DefaultStation", "LUSTER", "PRODUCT");
                //folder = Path.Combine($"E:\\{StatName}", "LUSTER", "PRODUCT"); // , DateTime.Now.ToString("yyyyMMdd")
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            else
            {
                folder = Path.Combine(_motionController.FileConfig.LogsSavePath, "PRODUCT");
            }
            var filename = Path.Combine(folder, "PRODUCT" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + className + ".csv");
            if (File.Exists(filename))
            {
                try
                {
                    using (var streamWriter = new StreamWriter(filename, true, Encoding.UTF8))
                    {
                        var properties = CSVTool.GetProperties<SaveProductModel>();
                        // 如果有未存入数据先存入记录数据
                        if (saveProductList.Count > 0)
                        {
                            foreach (var item in saveProductList)
                            {
                                CSVTool.AppendCSV(streamWriter, item, properties, "yyyy/MM/dd HH:mm:ss:fff");
                            }
                            saveProductList.Clear();
                        }
                        CSVTool.AppendCSV(streamWriter, saveModel, properties, "yyyy/MM/dd HH:mm:ss:fff");
                    }
                }
                catch (Exception ex)
                {
                    // 如果因为文件异常，记录未存入的数据保存起来
                    if (ex is IOException)
                    {
                        saveProductList.Add(saveModel);
                    }
                }
            }
            else
            {
                // 配置输出英文
                CSVTool.IsCN = false;
                CSVTool.SaveCSV(new List<SaveProductModel>() { saveModel }, filename, Encoding.UTF8, "yyyy/MM/dd HH:mm:ss:fff");
            }
        }
        // Toss抛料记录本地存储：小料LotNo，大料KeyPart
        private void AutoSaveTossingInfo(TbProductInfo info)
        {
            var saveModel = new SaveTossingModel(info);
            // 获取当前班次信息
            var className = GetClassName(DateTime.Now);

            // 获取文件产品信息存储路径
            //var folder = GetFolderPath(true);
            // Vision要求，产品信息存到E盘固定路径
            string folder = "";
            if (string.IsNullOrEmpty(_motionController.FileConfig.LogsSavePath))
            {
                string StatName = sysConfig?.StationName_Vision;
                if (string.IsNullOrEmpty(StatName))
                {
                    sysConfig = _motionController.WebService.GetConfig() as WebConfig;
                    StatName = sysConfig?.StationName_Vision;
                }
                var root = PickAvailableDrive();
                folder = Path.Combine(root, StatName ?? "DefaultStation", "LUSTER", "TOSSING");
                //folder = Path.Combine($"E:\\{StatName}", "LUSTER", "TOSSING");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            else
            {
                folder = Path.Combine(_motionController.FileConfig.LogsSavePath, "TOSSING");
            }
            var filename = Path.Combine(folder, "TOSSING" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + className + ".csv");
            if (File.Exists(filename))
            {
                try
                {
                    using (var streamWriter = new StreamWriter(filename, true, Encoding.UTF8))
                    {
                        var properties = CSVTool.GetProperties<SaveTossingModel>();
                        // 如果有未存入数据先存入记录数据
                        if (saveTossingList.Count > 0)
                        {
                            foreach (var item in saveTossingList)
                            {
                                CSVTool.AppendCSV(streamWriter, item, properties, "yyyy/MM/dd HH:mm:ss:fff");
                            }
                            saveTossingList.Clear();
                        }
                        CSVTool.AppendCSV(streamWriter, saveModel, properties, "yyyy/MM/dd HH:mm:ss:fff");
                    }
                }
                catch (Exception ex)
                {
                    // 如果因为文件异常，记录未存入的数据保存起来
                    if (ex is IOException)
                    {
                        saveTossingList.Add(saveModel);
                    }
                }
            }
            else
            {
                // 配置输出英文
                CSVTool.IsCN = false;
                CSVTool.SaveCSV(new List<SaveTossingModel>() { saveModel }, filename, Encoding.UTF8, "yyyy/MM/dd HH:mm:ss:fff");
            }
        }

        private void AutoSaveTossingInfo(TbThrow info)
        {
            var saveModel = new SaveTossingModel(info);
            // 获取当前班次信息
            var className = GetClassName(DateTime.Now);

            // 获取文件产品信息存储路径
            //var folder = GetFolderPath(true);
            // Vision要求，产品信息存到E盘固定路径
            string folder = "";
            if (string.IsNullOrEmpty(_motionController.FileConfig.LogsSavePath))
            {
                string StatName = sysConfig?.StationName_Vision;
                if (string.IsNullOrEmpty(StatName))
                {
                    sysConfig = _motionController.WebService.GetConfig() as WebConfig;
                    StatName = sysConfig?.StationName_Vision;
                }
                var root = PickAvailableDrive();
                folder = Path.Combine(root, StatName ?? "DefaultStation", "LUSTER", "TOSSING");
                //folder = Path.Combine($"E:\\{StatName}", "LUSTER", "TOSSING");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            else
            {
                folder = Path.Combine(_motionController.FileConfig.LogsSavePath, "TOSSING");
            }
            var filename = Path.Combine(folder, "TOSSING" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + className + ".csv");
            if (File.Exists(filename))
            {
                try
                {
                    using (var streamWriter = new StreamWriter(filename, true, Encoding.UTF8))
                    {
                        var properties = CSVTool.GetProperties<SaveTossingModel>();
                        // 如果有未存入数据先存入记录数据
                        if (saveTossingList.Count > 0)
                        {
                            foreach (var item in saveTossingList)
                            {
                                CSVTool.AppendCSV(streamWriter, item, properties, "yyyy/MM/dd HH:mm:ss:fff");
                            }
                            saveTossingList.Clear();
                        }
                        CSVTool.AppendCSV(streamWriter, saveModel, properties, "yyyy/MM/dd HH:mm:ss:fff");
                    }
                }
                catch (Exception ex)
                {
                    // 如果因为文件异常，记录未存入的数据保存起来
                    if (ex is IOException)
                    {
                        saveTossingList.Add(saveModel);
                    }
                }
            }
            else
            {
                // 配置输出英文
                CSVTool.IsCN = false;
                CSVTool.SaveCSV(new List<SaveTossingModel>() { saveModel }, filename, Encoding.UTF8, "yyyy/MM/dd HH:mm:ss:fff");
            }
        }

        // CT 信息
        /// <summary>
        /// CT 信息写入数据库
        /// </summary>
        /// <param name="stationTimes"></param>
        private void MotionEngine_TimeStatisEvent(List<StationTime> stationTimes, bool IsUseLog)
        {
            if (null == sysConfig)
            {
                // 2025-4-24
                sysConfig = _motionController.WebService.GetConfig() as WebConfig;
            }

            // 开启CT统计，则写入数据库
            if (_motionController.SysConfig.IsEnableCTStatistics)
            {
                if (stationTimes != null && stationTimes.Count() > 0)
                {
                    var stationInfos = new List<StationTime>(stationTimes);

                    List<TbCTInfo> tbCTInfos = new List<TbCTInfo>();
                    // 2025-5-8 wyy
                    List<TbCTInfo2> tbCTInfos2 = new List<TbCTInfo2>();
                    List<TbCTInfo2> ctInfoSelected = new List<TbCTInfo2>();
                    var time = DateTime.Now;

                    foreach (var stat in stationInfos)
                    {
                        var ct = new TbCTInfo()
                        {
                            //看CG11-2的CT记录，存的StationName是Sn码，存的ModuleName是模块
                            StationName = stat.Station,
                            ModuleName = stat.Module,
                            CtTime = Math.Round(stat.Time / 1000, 3),
                            CreateTime = time,
                            StandardCTTime = Math.Round(stat.CT / 1000, 3),
                            SNCode = stat.SN,
                            Differ = Math.Round(stat.Time / 1000 - stat.CT / 1000, 3),
                            StartTime = stat.StartTime,
                            EndTime = stat.EndTime,
                            XSpd = (float)Convert.ToSingle(stat.ExtParams["X_Spd"]),
                            XAcc = (float)Convert.ToSingle(stat.ExtParams["X_RotateSpd"]),
                            XSpdTarget = (float)Convert.ToSingle(stat.ExtParams["X_Spd_Target"]),
                            XAccTarget = (float)Convert.ToSingle(stat.ExtParams["X_Acc_Target"]),

                            YSpd = (float)Convert.ToSingle(stat.ExtParams["Y_Spd"]),
                            YAcc = (float)Convert.ToSingle(stat.ExtParams["Y_RotateSpd"]),
                            YSpdTarget = (float)Convert.ToSingle(stat.ExtParams["Y_Spd_Target"]),
                            YAccTarget = (float)Convert.ToSingle(stat.ExtParams["Y_Acc_Target"]),

                            ZSpd = (float)Convert.ToSingle(stat.ExtParams["Z_Spd"]),
                            ZAcc = (float)Convert.ToSingle(stat.ExtParams["Z_RotateSpd"]),
                            ZSpdTarget = (float)Convert.ToSingle(stat.ExtParams["Z_Spd_Target"]),
                            ZAccTarget = (float)Convert.ToSingle(stat.ExtParams["Z_Acc_Target"]),

                            USpd = (float)Convert.ToSingle(stat.ExtParams["U_Spd"]),
                            UAcc = (float)Convert.ToSingle(stat.ExtParams["U_RotateSpd"]),
                            USpdTarget = (float)Convert.ToSingle(stat.ExtParams["U_Spd_Target"]),
                            UAccTarget = (float)Convert.ToSingle(stat.ExtParams["U_Acc_Target"]),
                            //XDistance = (float)Convert.ToSingle(stat.ExtParams["X_Distance"]),
                            //YDistance = (float)Convert.ToSingle(stat.ExtParams["Y_Distance"]),
                            //ZDistance = (float)Convert.ToSingle(stat.ExtParams["Z_Distance"]),
                            //UDistance = (float)Convert.ToSingle(stat.ExtParams["U_Distance"]),

                            RSpd = (float)Convert.ToSingle(stat.ExtParams["R_Spd"]),
                            RAcc = (float)Convert.ToSingle(stat.ExtParams["R_RotateSpd"]),
                            RSpdTarget = (float)Convert.ToSingle(stat.ExtParams["R_Spd_Target"]),
                            RAccTarget = (float)Convert.ToSingle(stat.ExtParams["R_Acc_Target"]),
                            //XDec = (float)Convert.ToSingle(stat.ExtParams["X_Dec"]),
                            //YDec = (float)Convert.ToSingle(stat.ExtParams["Y_Dec"]),
                            //ZDec = (float)Convert.ToSingle(stat.ExtParams["Z_Dec"]),
                            //UDec = (float)Convert.ToSingle(stat.ExtParams["U_Dec"]),
                            Delay = Convert.ToInt32(stat.ExtParams["Delayed"]),

                        };
                        tbCTInfos.Add(ct);
                        // 2025-5-8 wyy
                        var ct2 = new TbCTInfo2()
                        {
                            // 2025-5-8 wyy
                            模块 = stat.Station,
                            动作 = stat.Module,
                            Actual_CT = Math.Round(stat.Time / 1000, 3),
                            CreateTime = time,
                            Target_CT = Math.Round(stat.CT / 1000, 3),
                            SN = stat.SN,
                            Gap = Math.Round(stat.Time / 1000 - stat.CT / 1000, 3),
                            开始时间 = stat.StartTime,
                            结束时间 = stat.EndTime,

                        };
                        tbCTInfos2.Add(ct2);
                    }

                    // CT 数量过大，不能插入到数据库中，否则一两天就就被巨大了
                    //_repository.BatchInsert(tbCTInfos);
                    try
                    {
                        //AutoSaveCTInfos(tbCTInfos);
                    }
                    catch (Exception e)
                    {
                        _motionController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Info, $"AutoSaveCT is Error:{e.Message}");
                    }
                    // 2025-5-8
                    try
                    {
                        AutoSaveCTInfos2(tbCTInfos2, ctInfoSelected);
                        if (ctInfoSelected.Any(info => info.动作.Contains("工站结束")))
                        {
                            // 如果有，清空 ctInfoSelected
                            ctInfoSelected.Clear();
                            //Console.WriteLine("ctInfoSelected 已被清空，因为存在 '工站结束'。");
                        }
                    }
                    catch (Exception e)
                    {
                        _motionController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Info, $"AutoSaveCT is Error:{e.Message}");
                    }
                }
            }
        }

        private void AutoSaveCTInfos(List<TbCTInfo> tbCTInfos)
        {
            //List<TbCTInfo> ctInfos = new List<TbCTInfo>();

            // 获取当前班次信息
            var className = GetClassName(DateTime.Now);
            if (tbCTInfos.Count == 0) return;
            //string sn = tbCTInfos[0].SNCode;
            string sn = "None";
            sn = tbCTInfos.Where(x => !string.IsNullOrEmpty(x.SNCode) && !string.Equals(x.SNCode, "None")).LastOrDefault()?.SNCode;
            if (string.IsNullOrEmpty(sn) || string.Equals(sn, "None")) sn = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            //  存储到哪个csv文件名里
            string fileName = sn;
            // 获取文件产品信息存储路径
            var folder = GetCTFolderPath(true);
            if (_motionController.SysConfig.CtFileType == CtFileType.Module)
            {
                fileName = tbCTInfos[0].StationName;

                // 移除Start和End
                fileName = fileName.Replace("-Start", "").Replace("-Middle", "").Replace("-End", "");
            }

            //替换"_"为"-"
            for (int i = 0; i < tbCTInfos.Count; i++)
            {
                tbCTInfos[i].ModuleName = tbCTInfos[i].ModuleName.Replace("_", "-");
            }

            //// 2025-4-13
            //string webFile = "WebConfig.xml";
            //string slnDir = ProjectPath;
            //string configDir = Path.Combine(ProjectPath, "Config");
            //string webConfig = Path.Combine(configDir, webFile);
            //_webService.LoadConfig(webConfig);
            //_configManager.SetWebConfig(_webService.GetConfig());
            //string StatName = _configManager.GetWebConfig("StationName");
            //var folderName = Path.Combine("E:", StatName, "LUSTER", "STEPCT", DateTime.Now.ToString("yyyyMMdd"));
            var folderName = Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd"));
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            // 最终的文件
            var fullName = Path.Combine(folderName, fileName + ".csv");
            try
            {
                if (File.Exists(fullName))
                {
                    using (var streamWriter = new StreamWriter(fullName, true, Encoding.UTF8))
                    {
                        var properties = CSVTool.GetProperties<TbCTInfo>();

                        BuildStartEnd(sn, tbCTInfos);
                        foreach (var info in tbCTInfos)
                        {
                            CSVTool.AppendCSV(streamWriter, info, properties, "yyyy/MM/dd HH:mm:ss:fff");
                        }
                    }

                }
                else
                {
                    BuildStartEnd(sn, tbCTInfos);

                    CSVTool.SaveCSV(tbCTInfos, fullName, Encoding.UTF8, "yyyy/MM/dd HH:mm:ss:fff");
                }
            }
            catch (Exception ex)
            {
                // 如果因为文件异常，记录未存入的数据保存起来
                if (ex is IOException)
                {
                    _motionController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Warning, $"文件:{fullName} 被打开程序无法写入!");
                    _motionController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Debug, $"文件:{fullName} 被打开程序无法写入!,{ex.StackTrace}");
                }
            }
            finally
            {
                // 使用完后清除缓存
                //cacheTimes.TryRemove(key, out var v);
            }

        }

        private void AutoSaveCTInfos2(List<TbCTInfo2> tbCTInfos2, List<TbCTInfo2> ctInfoSelected)
        {
            // 2025-5-8 单独为 CTLog标准化 存储一个csv文件，不修改平台之前的逻辑
            // 获取当前班次信息
            var className = GetClassName(DateTime.Now);
            if (tbCTInfos2.Count == 0) return;
            //string sn = tbCTInfos[0].SNCode;
            string sn = "None";
            sn = tbCTInfos2.Where(x => !string.IsNullOrEmpty(x.SN) && !string.Equals(x.SN, "None")).LastOrDefault()?.SN;
            if (string.IsNullOrEmpty(sn) || string.Equals(sn, "None")) sn = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            //  存储到哪个csv文件名里
            string fileName = sn;
            string StatName = sysConfig?.StationName_Vision;
            //0603，修复绝对路径标识不正确，异常存到相对路径的问题，部分机台存在
            var root = PickAvailableDrive();
            var folderName2 = Path.Combine(root, StatName ?? "DefaultStation", "LUSTER", "STEPCT", DateTime.Now.ToString("yyyyMMdd"));
            //var folderName2 = Path.Combine($"E:\\{StatName}", "LUSTER", "STEPCT", DateTime.Now.ToString("yyyyMMdd"));
            //var folderName2 = Path.Combine("E", StatName, "LUSTER", "STEPCT", DateTime.Now.ToString("yyyyMMdd"));
            if (!Directory.Exists(folderName2))
            {
                Directory.CreateDirectory(folderName2);
            }
            // 2025-5-8  直接存到 工站.csv 里面
            fileName = tbCTInfos2[0].模块;
            // 移除Start和End
            // fileName = fileName.Replace("-Start", "").Replace("-Middle", "").Replace("-End", "");
            string timeTag = DateTime.Now.ToString("yyyyMMddHH"); 
            var fullName2 = Path.Combine(folderName2, $"STEPCT_{fileName}_{timeTag}.csv");
            try
            {
                if (File.Exists(fullName2))
                {
                    using (var streamWriter = new StreamWriter(fullName2, true, Encoding.UTF8))
                    {
                        var properties = CSVTool.GetProperties<TbCTInfo2>();

                        //BuildStartEnd2(fileName, tbCTInfos);
                        BuildStartEnd2(sn, tbCTInfos2, ctInfoSelected);
                        foreach (var info in ctInfoSelected) // tbCTInfos2
                        {
                            CSVTool.AppendCSV(streamWriter, info, properties, "yyyy-MM-dd HH:mm:ss.fff");
                        }
                    }

                }
                else
                {
                    //BuildStartEnd2(fileName, tbCTInfos2); 
                    BuildStartEnd2(sn, tbCTInfos2, ctInfoSelected);
                    CSVTool.SaveCSV(ctInfoSelected, fullName2, Encoding.UTF8, "yyyy-MM-dd HH:mm:ss.fff"); // tbCTInfos2
                }
            }
            catch (Exception ex)
            {
                // 如果因为文件异常，记录未存入的数据保存起来
                if (ex is IOException)
                {
                    _motionController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Warning, $"文件:{fullName2} 被打开程序无法写入!");
                    _motionController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Debug, $"文件:{fullName2} 被打开程序无法写入!,{ex.StackTrace}");
                }
            }
            finally
            {
                // 使用完后清除缓存
                //cacheTimes.TryRemove(key, out var v);
            }

        }

        /// <summary>
        /// 构建开始和结束
        /// </summary>
        /// <param name="station"></param>
        /// <param name="ctInfos"></param>
        private void BuildStartEnd(string station, List<TbCTInfo> ctInfos)
        {
            if (ctInfos.Count <= 0)
                return;
            TbCTInfo tbStart = new TbCTInfo();
            TbCTInfo tbEnd = new TbCTInfo();
            string wip = "None";
            string lcg = "None";


            //找到第一个不为空的sn(LCG)
            foreach (TbCTInfo tbCTInfo in ctInfos)
            {
                if (tbCTInfo.SNCode != "None")
                {
                    // 2025-5-1 wyy 有效果，是否添加待定，因为存储的csv文件（None.csv）有误
                    //if ("None" == station)
                    //{
                    //    station = tbCTInfo.SNCode;
                    //}
                    lcg = tbCTInfo.StationName; // lcg = tbCTInfo.SNCode
                    break;
                }
            }

            //根据CG SN查询到WIP
            var cacheItem = _cacheManager.GetItem(lcg);
            if (cacheItem != null && cacheItem is CacheItem cItem)
            {
                try
                {
                    var wipItem = cItem.FirstOrDefault(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                    if (wipItem != null && wipItem.Value != null)
                    {
                        wip = wipItem.Value.ToString();
                        if (wip == "IsEmptyMode")
                        { wip = "None"; }
                    }
                }
                catch (Exception ex)
                {
                    _motionController.MotionEngine.OnLog(LogType.Error, $"BuildStartEnd 方法查询Wip异常:{ex.StackTrace}");
                }
            }

            //创建开始
            tbStart.SNCode = station;   // wip
            tbStart.StationName = lcg;  // station
            tbStart.ModuleName = $"00_{tbStart.StationName}_工站开始";
            tbStart.StartTime = ctInfos[0].StartTime;
            tbStart.EndTime = tbStart.StartTime;


            int sort = 1;
            foreach (var info in ctInfos)
            {
                info.StationName = info.StationName; // station
                info.ModuleName = $"{sort.ToString().PadLeft(2, '0')}_{station}_{info.ModuleName}";
                info.SNCode = station;               // wip
                info.CtTime = Math.Round((info.EndTime - info.StartTime).TotalMilliseconds / 1000.0, 3);
                sort++;
            }

            //创建结束
            tbEnd.SNCode = station;  // wip 
            tbEnd.StationName = lcg; // station
            tbEnd.ModuleName = $"{sort.ToString().PadLeft(2, '0')}_{tbEnd.StationName}_工站结束";
            tbEnd.StartTime = ctInfos[ctInfos.Count - 1].EndTime;
            tbEnd.EndTime = tbEnd.StartTime;
            ctInfos.Insert(0, tbStart);
            ctInfos.Add(tbEnd);
        }
        // 2025-5-8
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
        //TbCTInfo2 tbStartPerUnit = new TbCTInfo2();
        private void BuildStartEnd2(string station, List<TbCTInfo2> ctInfos, List<TbCTInfo2> ctInfoSelected)
        {
            if (ctInfos.Count <= 0)
                return;
            TbCTInfo2 tbStart = new TbCTInfo2();
            TbCTInfo2 tbEnd = new TbCTInfo2();
            string wip = "None";
            string lcg = "None";


            //找到第一个不为空的sn(LCG)
            foreach (TbCTInfo2 tbCTInfo in ctInfos)
            {
                if (tbCTInfo.SN != "None")
                {
                    // 2025-5-1 wyy 
                    if ("None" == station)
                    {
                        station = tbCTInfo.SN;
                    }
                    lcg = tbCTInfo.模块;
                    break;
                }
            }

            //根据CG SN查询到WIP
            var cacheItem = _cacheManager.GetItem(lcg);
            if (cacheItem != null && cacheItem is CacheItem cItem)
            {
                try
                {
                    var wipItem = cItem.FirstOrDefault(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                    if (wipItem != null && wipItem.Value != null)
                    {
                        wip = wipItem.Value.ToString();
                        if (wip == "IsEmptyMode")
                        { wip = "None"; }
                    }
                }
                catch (Exception ex)
                {
                    _motionController.MotionEngine.OnLog(LogType.Error, $"BuildStartEnd2 方法查询Wip异常:{ex.StackTrace}");
                }
            }

            // 2025-11-7，CtLogConfig行数扩充一倍
            var kvList = ctConfigs.ToList();          // 保持插入顺序
            for (int i = 0; i + 1 < kvList.Count; i += 2)
            {
                var firstKey = kvList[i].Key;     // 第一行 key
                var secondKey = kvList[i + 1].Key;// 第二行 key
                // 到 ctInfos 里找对应元素
                var first = ctInfos.FirstOrDefault(c => listA.Contains(c.模块) && c.动作 == firstKey);
                var second = ctInfos.FirstOrDefault(c => listA.Contains(c.模块) && c.动作 == secondKey);
                if (first != null && second != null)
                {
                    // 当拿不到SN时，平台会赋值一个时间戳，FX要求统一改为NULL
                    if (!station.Contains("+")|| station.Length<14)
                    {
                        station = "NULL";
                    }
                    // 找到第一行，取第一行key在ctConfigs里面的value
                    var firstValue = kvList[i].Value;
                    //string moduleNameTrim = Regex.Replace(first.模块, @"-(?:Start|End)$", "");
                    string moduleNameTrim = first.模块;
                    string[] parts = firstValue.Split('_');
                    if (parts.Length > 0 && parts[0] == "CT2")
                    {
                        //创建开始
                        tbStart.SN = station;
                        tbStart.模块 = moduleNameTrim;
                        tbStart.动作 = $"CT1_{tbStart.模块}_工站开始";
                        tbStart.开始时间 = first.开始时间;
                        tbStart.结束时间 = tbStart.开始时间;
                        // 新增Time_Slot
                        tbStart.Time_Slot = tbStart.开始时间;
                        ctInfoSelected.Add(tbStart);
                        timeSlotByModule[parts[1]] = tbStart.开始时间;
                        //tbStartPerUnit = tbStart;
                    }
                    if (second != null)
                    {
                        // 第一个key的界面CT暂时写为：动作CT
                        TbCTInfo2 infoSelect = new TbCTInfo2
                        {
                            模块 = moduleNameTrim,
                            动作 = firstValue,
                            SN = station,
                            开始时间 = first.开始时间,
                            结束时间 = second.结束时间,
                            Actual_CT = Math.Round((second.结束时间 - first.开始时间).TotalMilliseconds / 1000.0, 3),
                            Target_CT = first.Target_CT,
                            Gap = Math.Round((second.结束时间 - first.开始时间).TotalMilliseconds / 1000.0 - first.Target_CT, 3), 
                            Time_Slot = timeSlotByModule[parts[1]] //tbStartPerUnit.开始时间
                        };
                        ctInfoSelected.Add(infoSelect);
                    }
                    // 匹配到两行后
                    if (i + 2 < ctConfigs.Count)  // i+1改为i+2
                    {
                        var nextKeyPair = ctConfigs.ElementAt(i + 2);
                        string[] nextParts = nextKeyPair.Value.Split('_');

                        if ((parts.Length > 1 && nextParts.Length > 1 && parts[1] != nextParts[1]) || nextParts[0]=="CT2")
                        {
                            //创建结束
                            tbEnd.SN = station;
                            //tbEnd.模块 = ctInfos[0].模块;
                            tbEnd.模块 = moduleNameTrim;
                            string nextCtNumber = IncrementCtNumber(parts[0]);
                            tbEnd.动作 = $"{nextCtNumber}_{tbEnd.模块}_工站结束";
                            //tbEnd.开始时间 = ctInfos[ctInfos.Count - 1].结束时间;
                            tbEnd.开始时间 = second.结束时间;
                            tbEnd.结束时间 = tbEnd.开始时间;
                            tbEnd.Time_Slot = timeSlotByModule[parts[1]];
                            ctInfoSelected.Add(tbEnd);
                            break;
                        }
                    }
                    else
                    {
                        //处理完csv最后一行，也需要创建结束
                        tbEnd.SN = station;
                        //tbEnd.模块 = ctInfos[0].模块;
                        tbEnd.模块 = moduleNameTrim;
                        string nextCtNumber = IncrementCtNumber(parts[0]);
                        tbEnd.动作 = $"{nextCtNumber}_{tbEnd.模块}_工站结束";
                        //tbEnd.开始时间 = ctInfos[ctInfos.Count - 1].结束时间;
                        tbEnd.开始时间 = second.结束时间;
                        tbEnd.结束时间 = tbEnd.开始时间;
                        tbEnd.Time_Slot = timeSlotByModule[parts[1]];
                        ctInfoSelected.Add(tbEnd);
                    }
                }
            } 

            //// 2025-7-1 根据配置文件筛选并生成最终存入CTLog的数据
            ////  Process ctInfos
            //foreach (var info in ctInfos)
            //{
            //    if (listA.Contains(info.模块))
            //    {
            //        // 使用 for 循环遍历 ctConfigs，同时访问当前项和下一项
            //        for (int i = 0; i < ctConfigs.Count; i++) //  - 1
            //            {
            //                var keyPair = ctConfigs.ElementAt(i);
            //                if (info.动作 == keyPair.Key)
            //                {
            //                    string moduleNameTrim = Regex.Replace(info.模块, @"-(?:Start|End)$", "");
            //                    string[] parts = keyPair.Value.Split('_');
            //                    if (parts.Length > 0 && parts[0] == "CT2")
            //                    {
            //                        //创建开始
            //                        tbStart.SN = station;
            //                        // 解决 工站开始 没有 stationName工站名称
            //                        //lcg = ctInfos[0].模块;
            //                        tbStart.模块 = moduleNameTrim;
            //                        tbStart.动作 = $"CT1_{tbStart.模块}_工站开始";
            //                        //tbStart.开始时间 = ctInfos[0].开始时间;
            //                        tbStart.开始时间 = info.开始时间;
            //                        tbStart.结束时间 = tbStart.开始时间;
            //                        ctInfoSelected.Add(tbStart);
            //                    }

            //                    info.模块 = moduleNameTrim;
            //                    //info.动作 = $"{parts0[0]}_{ctInfos[0].模块}_{info.动作}";
            //                    info.动作 = keyPair.Value;
            //                    info.SN = station;
            //                    info.Actual_CT = Math.Round((info.结束时间 - info.开始时间).TotalMilliseconds / 1000.0, 3);
            //                    ctInfoSelected.Add(info);

            //                    if (i + 1 < ctConfigs.Count)
            //                    {
            //                        var nextKeyPair = ctConfigs.ElementAt(i + 1);
            //                        string[] nextParts = nextKeyPair.Value.Split('_');

            //                        if (parts.Length > 1 && nextParts.Length > 1 && parts[1] != nextParts[1])
            //                        {
            //                            //创建结束
            //                            tbEnd.SN = station;
            //                            //tbEnd.模块 = ctInfos[0].模块;
            //                            tbEnd.模块 = moduleNameTrim;
            //                            string nextCtNumber = IncrementCtNumber(parts[0]);
            //                            tbEnd.动作 = $"{nextCtNumber}_{tbEnd.模块}_工站结束";
            //                            //tbEnd.开始时间 = ctInfos[ctInfos.Count - 1].结束时间;
            //                            tbEnd.开始时间 = info.结束时间;
            //                            tbEnd.结束时间 = tbEnd.开始时间;
            //                            ctInfoSelected.Add(tbEnd);
            //                            break;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        //处理完csv最后一行，也需要创建结束
            //                        tbEnd.SN = station;
            //                        //tbEnd.模块 = ctInfos[0].模块;
            //                        tbEnd.模块 = moduleNameTrim;
            //                        string nextCtNumber = IncrementCtNumber(parts[0]);
            //                        tbEnd.动作 = $"{nextCtNumber}_{tbEnd.模块}_工站结束";
            //                        //tbEnd.开始时间 = ctInfos[ctInfos.Count - 1].结束时间;
            //                        tbEnd.开始时间 = info.结束时间;
            //                        tbEnd.结束时间 = tbEnd.开始时间;
            //                        ctInfoSelected.Add(tbEnd);
            //                    }
            //                }
            //            }
            //    }

            //    ////创建开始
            //    //tbStart.SN = station;
            //    //// 解决 工站开始 没有 stationName工站名称
            //    //lcg = ctInfos[0].模块;
            //    //tbStart.模块 = lcg;
            //    //tbStart.动作 = $"CT1_{tbStart.模块}_工站开始";
            //    //tbStart.开始时间 = ctInfos[0].开始时间;
            //    //tbStart.结束时间 = tbStart.开始时间;


            //    //int sort = 2;
            //    //foreach (var info in ctInfos)
            //    //{
            //    //    info.模块 = info.模块;
            //    //    info.动作 = $"CT{sort.ToString()}_{ctInfos[0].模块}_{info.动作}";
            //    //    info.SN = station;
            //    //    info.Actual_CT = Math.Round((info.结束时间 - info.开始时间).TotalMilliseconds / 1000.0, 3);
            //    //    sort++;
            //    //}

            //    ////创建结束
            //    //tbEnd.SN = station;
            //    //tbEnd.模块 = lcg;
            //    //tbEnd.动作 = $"CT{sort.ToString()}_{tbEnd.模块}_工站结束";
            //    //tbEnd.开始时间 = ctInfos[ctInfos.Count - 1].结束时间;
            //    //tbEnd.结束时间 = tbEnd.开始时间;
            //    //ctInfos.Insert(0, tbStart);
            //    //ctInfos.Add(tbEnd);
            //}
        }

        public void AppendCSV<T>(StreamWriter sw, T item, List<PropertyInfo> properties)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(item);
                if (value == null)
                {
                    stringBuilder.Append(",");
                    continue;
                }

                Dictionary<string, object> dictionary = value as Dictionary<string, object>;
                if (dictionary != null)
                {
                    foreach (KeyValuePair<string, object> item2 in dictionary)
                    {
                        stringBuilder.Append($"{item2.Value},");
                    }
                }
                else
                {
                    stringBuilder.Append($"{value},");
                }
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            sw.WriteLine(stringBuilder.ToString());
        }

        #endregion

        /// <summary>
        /// 获取开班时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetStartTime()
        {
            if (_motionController.SysConfig == null || _motionController.SysConfig.ClassModels == null)
            {
                return DateTime.Now.Date.AddHours(8);
            }

            var firstClass = _motionController.SysConfig.ClassModels.Where(x => x.FirstClass == true).FirstOrDefault();
            if (firstClass != null)
            {
                var hour = firstClass.StartTime.Hour;
                var minute = firstClass.StartTime.Minute;
                return DateTime.Now.Date.AddHours(hour).AddMinutes(minute);
            }
            else
            {
                if (_motionController.SysConfig.ClassModels.Count > 0)
                {
                    var defaultClass = _motionController.SysConfig.ClassModels.First();
                    var hour = defaultClass.StartTime.Hour;
                    var minute = defaultClass.StartTime.Minute;
                    return DateTime.Now.Date.AddHours(hour).AddMinutes(minute);
                }
                else
                {
                    return DateTime.Now.Date.AddHours(8);
                }
            }
        }

        /// <summary>
        /// 项目路径
        /// </summary>
        public string ProjectPath { get; set; }


        #region 首页信息相关

        /// <summary>
        /// 获取该配方下的所有质量配置
        /// </summary>
        /// <param name="recipeName"></param>
        /// <returns></returns>
        public IEnumerable<TbLTolerance> GetQualityItems(string recipeName)
        {
            var lisLTolerances = _repository.GetList<TbLTolerance>(x => x.RecipeName == recipeName, x => x.ID);
            return lisLTolerances;
        }

        /// <summary>
        /// 获取最后50条数据
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public IEnumerable<TbProductInfo> GetLast50Products()
        {
            return _repository.GetPage<TbProductInfo>(u => true, u => u.ID, 1, 50, out var total, true);
        }

        /// <summary>
        /// 删除不匹配的质量配置项
        /// </summary>
        /// <param name="models"></param>
        /// <param name="recipeName"></param>
        public void DeleteQualityItems(IEnumerable<TbLTolerance> tbLTolerances, string recipeName)
        {
            if (tbLTolerances.Count() > 0)
            {
                _repository.BatchDelete(tbLTolerances);
            }
        }

        /// <summary>
        /// 删除指定的质量配置项
        /// </summary>
        /// <param name="model"></param>
        /// <param name="recipeName"></param>
        public void DeleteQualityItem(TbLTolerance tbLTolerance, string recipeName)
        {
            _repository.Delete(tbLTolerance);
        }

        /// <summary>
        /// 添加规则
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public void AddTbLTolerance(IEnumerable<TbLTolerance> models)
        {
            List<TbLTolerance> newItems = new List<TbLTolerance>();
            List<TbLTolerance> existItems = new List<TbLTolerance>();
            foreach (var model in models)
            {
                if (model.ID > 0)
                {
                    existItems.Add(model);
                }
                else
                {
                    newItems.Add(model);
                }
            }
            if (newItems.Count > 0)
            {
                _repository.BatchInsert(newItems);
            }

            if (existItems.Count > 0)
            {
                _repository.BatchUpdate(existItems);
            }
        }


        public TbLTolerance GetLTolerance(Guid id, string name)
        {
            var item = _repository.GetList<TbLTolerance>(x => x.MapId == id && x.Name == name, x => x.ID).FirstOrDefault();
            return item;
        }

        #endregion

        #region 报警相关统计

        /// <summary>
        /// 获取数据数量
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="searchParas">查询参数</param>
        /// <returns></returns>
        public int GetAlarmCount(DateTime startTime, DateTime endTime, string searchParas = "")
        {
            if (!string.IsNullOrEmpty(searchParas))
            {
                return (int)_repository.Count<TbAlarm>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.Module.Contains(searchParas));
            }
            else
            {
                return (int)_repository.Count<TbAlarm>(x => x.CreateTime > startTime && x.CreateTime < endTime);
            }
        }


        /// <summary>
        /// 根据条件查询一页数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="searchContent"></param>
        /// <param name="predicate"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="perPageCount"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<TbAlarm> GetAlarmPageData(DateTime startTime, DateTime endTime, string searchContent,
    string sort, int pageIndex, int perPageCount, out long count, List<string> filterTypes = null, bool excludeTypes = false)
        {
            //orm 拼接条件 查询  
            Expression<Func<TbAlarm, bool>> where = null;

            if (!string.IsNullOrEmpty(searchContent))
            {
                if (filterTypes != null && filterTypes.Count > 0)
                {
                    if (excludeTypes)
                    {
                        where = x => x.CreateTime > startTime && x.CreateTime < endTime
                            && (x.Module.Contains(searchContent) || (x.AlarmType != null && x.AlarmType.Contains(searchContent)))
                            && !filterTypes.Contains(x.AlarmType);
                    }
                    else
                    {
                        where = x => x.CreateTime > startTime && x.CreateTime < endTime
                            && (x.Module.Contains(searchContent) || (x.AlarmType != null && x.AlarmType.Contains(searchContent)))
                            && filterTypes.Contains(x.AlarmType);
                    }
                }
                else
                {
                    where = x => x.CreateTime > startTime && x.CreateTime < endTime
                        && (x.Module.Contains(searchContent) || (x.AlarmType != null && x.AlarmType.Contains(searchContent)));
                }
            }
            else
            {
                if (filterTypes != null && filterTypes.Count > 0)
                {
                    if (excludeTypes)
                    {
                        where = x => x.CreateTime > startTime && x.CreateTime < endTime && !filterTypes.Contains(x.AlarmType);
                    }
                    else
                    {
                        where = x => x.CreateTime > startTime && x.CreateTime < endTime && filterTypes.Contains(x.AlarmType);
                    }
                }
                else
                {
                    where = x => x.CreateTime > startTime && x.CreateTime < endTime;
                }
            }

            return _repository.GetPage<TbAlarm>(where, x => x.ID, pageIndex, perPageCount, out count);
        }

        /// <summary>
        /// 获取分析数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public IEnumerable<TbAlarm> GetAnalyzeModels(DateTime startTime, DateTime endTime)
        {
            var data = _repository.GetList<TbAlarm>(x => x.CreateTime > startTime && x.CreateTime < endTime, x => x.ID);
            return data;
        }

        #endregion

        #region 用户操作相关
        /// <summary>
        /// 获取数据数量
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="searchContent">查询内容</param>
        /// <returns></returns>
        public int GetLogCount(DateTime startTime, DateTime endTime, string searchParas = "")
        {
            if (!string.IsNullOrEmpty(searchParas))
            {
                return (int)_repository.Count<TbSysOperation>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.Operation.Contains(searchParas));
            }
            else
            {
                return (int)_repository.Count<TbSysOperation>(x => x.CreateTime > startTime && x.CreateTime < endTime);
            }
        }


        /// <summary>
        /// 根据条件查询一页数据
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="searchContent"></param>
        /// <param name="predicate"></param>
        /// <param name="sort"></param>
        /// <param name="pageIndex"></param>
        /// <param name="perPageCount"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<TbSysOperation> GetOperationPageData(DateTime startTime, DateTime endTime, string searchContent,
            string sort, int pageIndex, int perPageCount, out long count)
        {
            if (!string.IsNullOrEmpty(searchContent))
            {
                return _repository.GetPage<TbSysOperation>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.Operation.Contains(searchContent),
                    x => x.ID, pageIndex, perPageCount, out count);
            }
            else
            {
                return _repository.GetPage<TbSysOperation>(x => x.CreateTime > startTime && x.CreateTime < endTime,
                     x => x.ID, pageIndex, perPageCount, out count);
            }
        }

        /// <summary>
        /// 获取机台状态数据List
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TbSysOperation> GetOperationList(DateTime startTime, DateTime endTime, string searchParas = "")
        {
            if (!string.IsNullOrEmpty(searchParas))
            {
                return _repository.GetList<TbSysOperation>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.Operation.Contains(searchParas), x => x.ID, false);
            }
            else
            {
                return _repository.GetList<TbSysOperation>(x => x.CreateTime > startTime && x.CreateTime < endTime, x => x.ID, false);
            }
        }

        /// <summary>
        /// 获取时间段内最新一次机台记录
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IEnumerable<TbSysOperation> GetLastOperation(DateTime startTime, DateTime endTime)
        {
            return _repository.GetPage<TbSysOperation>(x => x.CreateTime > startTime && x.CreateTime < endTime, x => x.ID, 1, 1, out var total, true);
        }
        #endregion

        #region 首页清空产能
        public void ClearCurrentCapacityInfo()
        {
            //异步执行删除，导出数据任务，不卡顿UI主线程
            System.Threading.Tasks.Task.Run(() =>
            {
                var time = DateTime.Now;
                // 1 清空当前时间以前的CT记录数据
                var tbInfos = _repository.GetList<TbCTInfo>(x => x.CreateTime < time, x => x.ID);
                _repository.BatchDelete(tbInfos);

                // 2 导出产品记录，产品时间范围大于lastTime 小于当前时间,生成CSV文件
                // 筛选出当前时间段数据  

                var products = _repository.GetList<TbProductInfo>(x => x.CreateTime > _lastClearTime && x.CreateTime < time, x => x.ID);
                var listSaveProducts = new List<SaveProductModel>();

                var headers = _motionController.MotionEngine.MapDatas.GroupBy(x => x.Alias).Select(x => x.Key).ToList();
                foreach (var product in products)
                {
                    listSaveProducts.Add(new SaveProductModel(product, headers));
                }
                // 获取当前班次信息
                var className = GetClassName(time);

                // 获取文件产品信息存储路径
                var folder = GetFolderPath();
                var filename = Path.Combine(folder, DateTime.Now.ToString("yyyy-MM-dd") + "_" + className + ".csv");
                CSVTool.SaveCSV(listSaveProducts, filename, Encoding.UTF8);
                _lastClearTime = time;
                tbProYeildCache = new TbProductYeild() { ID = 1, AllCount = 0, NGCount = 0, OKCount = 0, Data = "" };

                // 更新数据
                UpdateProYield(tbProYeildCache);

                // 更新数据库
                _repository.Update<TbProductYeild>(tbProYeildCache);
            });
        }

        /// <summary>
        /// 获取文件产品信息存储路径
        /// </summary>
        ///<param name="isAuto">是否自动存储</param>
        /// <returns></returns>
        private string GetFolderPath(bool isAuto = false)
        {
            var slnDir = ProjectPath;
            var productDir = string.Empty;
            if (isAuto)
            {
                productDir = Path.Combine(slnDir, "Datas", "AutoSave", "Product");
            }
            else
            {
                productDir = Path.Combine(slnDir, "Datas", "Product");
            }

            if (!Directory.Exists(productDir))
            {
                Directory.CreateDirectory(productDir);
            }
            return productDir;
        }

        private string GetClassName(DateTime time)
        {
            var curClass = _motionController.SysConfig.GetCurrentClass(DateTime.Now);
            if (curClass == null)
            {
                return "未分班次";
            }


            return curClass.ClassName;
        }

        private string GetCTFolderPath(bool isAuto = false)
        {
            var slnDir = ProjectPath;
            var productDir = string.Empty;
            if (isAuto)
            {
                productDir = Path.Combine(slnDir, "Datas", "AutoSave", "CT");
            }
            else
            {
                productDir = Path.Combine(slnDir, "Datas", "CT");
            }

            if (!Directory.Exists(productDir))
            {
                Directory.CreateDirectory(productDir);
            }
            return productDir;
        }

        private TbProductYeild tbProYeildCache = null;
        private object motionEngine;

        /// <summary>
        /// 更新产能信息
        /// </summary>
        /// <param name="all"></param>
        /// <param name="ok"></param>
        /// <param name="ng"></param>
        public void UpdateProYield(TbProductYeild tbProYield)
        {
            tbProYeildCache = tbProYield;
        }

        /// <summary>
        /// 获取产能信息
        /// </summary>
        /// <returns></returns>
        public TbProductYeild GetProYield()
        {
            var list = _repository.GetList<TbProductYeild>(u => true, u => u.ID, true);
            return _repository.Get<TbProductYeild>(1);
        }

        /// <summary>
        /// 获取当前UPH
        /// </summary>
        /// <returns></returns>
        public long GetCurrentUPH()
        {
            var now = DateTime.Now;
            // 开始时间
            var startNow = DateTime.Parse($"{now.Year}-{now.Month}-{now.Day} {now.Hour}:00:00");
            var endNow = DateTime.Parse($"{now.Year}-{now.Month}-{now.Day} {now.Hour}:59:59");

            return _repository.Count<TbProductInfo>(u => u.OutTime >= startNow && u.OutTime <= endNow);
        }
        #endregion

        #region 辅料相关操作

        /// <summary>
        /// 添加辅料信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="persent"></param>
        /// <param name="pieceCount"></param>
        public void AddAccessory(string name, int persent, int pieceCount)
        {
            var tbAccessory = new TbAccessory()
            {
                Name = name,
                AlarmThreshold = persent,
                PieceUsedCount = pieceCount,
                CreateTime = DateTime.Now
            };
            _repository.Insert(tbAccessory);
        }

        /// <summary>
        /// 根据Id删除辅料信息
        /// </summary>
        /// <param name="id"></param>
        public void DeleteAccessory(long id)
        {
            _repository.DeleteById<TbAccessory>(id);
        }

        /// <summary>
        /// 添加辅料更换记录
        /// </summary>
        /// <param name="name"></param>
        /// <param name="batcgNo"></param>
        /// <param name="count"></param>
        /// <param name="stationName"></param>
        public void AddChangeAccessoryLog(string name, string batcgNo, int count, string stationName)
        {
            var item = new TbAccessoryChangeTable()
            {
                Name = name,
                BatchNo = batcgNo,
                Count = count,
                StationName = stationName,
                CreateTime = DateTime.Now
            };
            _repository.Insert(item);
        }

        /// <summary>
        /// 获取全部辅料信息
        /// Todo 首页抛料字典以后要根据这里的信息
        /// Todo 流程中抛料事件的物料名称要从这里获取
        /// </summary>
        /// <returns></returns>
        public List<TbAccessory> GetAllAccessory()
        {
            var list = _repository.GetList<TbAccessory>(x => x.ID > 0, x => x.ID).ToList();
            return list;
        }
        #endregion
    }
}