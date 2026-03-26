using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.Tools;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Policy;
using Luster.Motion.DataStruct;
using System.Diagnostics;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Common.Tools.Tools;
using System.Text;
using Luster.Common.DataStruct;
using System.Net;

namespace Luster.Motion.TaskFlow.Engine
{
    /// <summary>
    /// 驾驶舱API
    /// </summary>
    public class HyperTrainAPI : BaseMESAPI
    {
        /// <summary>
        /// 心跳定时xingye
        /// </summary>
        private const int HeartSleep = 500;

        // 输出项列表
        private List<MapData> _mapDatas;

        string ImagePath = "";

        System.Threading.Timer timer;

        //CPK计算所需的历史数据
        Dictionary<string, List<double>> DataDict = new Dictionary<string, List<double>>();

        public override string GetSystem()
        {
            return "HyperTrain";
        }

        /// <summary>
        /// 数据库访问
        /// </summary>
        protected IRepository reporitory;

        public HyperTrainAPI(IRepository _reporitory)
        {
            reporitory = _reporitory;
        }

        /// <summary>
        /// 维护设备参数
        /// </summary>
        private List<IMaintain> maintains = null;

        protected override void Register(IMotionController mController)
        {
            base.Register(mController);

            maintains = mController.MotionEngine.DeviceEngine.GetVDevices<IMaintain>();

            _mapDatas = mController.MotionEngine.MapDatas;
            JudgeIsNeedGetHistory(_mapDatas);//获取输出项

            mController.ProductEvent -= MController_ProductEvent;
            mController.ProductEvent += MController_ProductEvent;

            mController.ProStatEvent -= MController_ProStatEvent;
            mController.ProStatEvent += MController_ProStatEvent;

            mController.MotionEngine.WorkFlow.ProStatusChangedEvent -= WorkFlow_ProStatusChangedEvent;
            mController.MotionEngine.WorkFlow.ProStatusChangedEvent += WorkFlow_ProStatusChangedEvent;

            mController.MotionEngine.DeviceEngine.MaintainZeroEvent -= DeviceEngine_MaintainZeroEvent;
            mController.MotionEngine.DeviceEngine.MaintainZeroEvent += DeviceEngine_MaintainZeroEvent;

            mController.ProjBackUpEvent -= MController_ProjBackUpEvent;
            mController.ProjBackUpEvent += MController_ProjBackUpEvent;
            StartTimer();

        }


        private void StartTimer()
        {
            int minute=DateTime.Now.Minute;
            int dueSecond = 0;
            if (minute <= 30)
            {
                dueSecond = (30 - minute) * 60 * 1000;
            }
            else
            {
                dueSecond = (60 - minute) * 60 * 1000;
            }
            timer = new System.Threading.Timer(new TimerCallback(FTPUPLoadData), this, dueSecond, 1000 * 60 * 30);//创建定时器
        }


        /// <summary>
        /// 申请备份
        /// </summary>
        /// <param name="obj"></param>
        private void MController_ProjBackUpEvent(string obj)
        {
            ApplyBackUpProject(obj);
        }

        public override void StatusChanged(IMotionController mController, EngineStatus src, EngineStatus dst)
        {
            base.StatusChanged(mController, src, dst);

            AlarmInfo alarmInfo = mController.AlarmInfo;

            Activation(src, dst, alarmInfo);
        }

        /// <summary>
        /// 稼栋状态
        /// 3、设备状态数据 API 接口
        /// </summary>
        public void Activation(EngineStatus src, EngineStatus dst, AlarmInfo alarmInfo)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            string url = Path.Combine(URL, "machineMonitorController/machineStatus");

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            string alarmMsg = "";
            string alarmCode = "";

            if (dst == EngineStatus.Alarm && alarmInfo != null)
            {
                alarmCode = alarmInfo.AlarmCode;
                alarmMsg = alarmInfo.Message;
            }

            var activationData = new
            {
                machineSn = sysConfig.MachineSn,
                stationName = sysConfig.MachineSn,
                StationId = sysConfig.StationId,
                StationName = sysConfig.StationName,
                fromStatus = ConvertToMode(src).ToString(),
                toStatus = ConvertToMode(dst).ToString(),
                errorMessage = alarmMsg,
                errorCode = alarmCode
            };

            jsonArray.Add(activationData);
            string jsonData = JsonTool.ToJson(jsonArray);

            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        protected override void ManualDown(object arg1, SystemOperation arg2, string arg3)
        {
            base.ManualDown(arg1, arg2, arg3);

            if (arg3 == "设备换料")
            {
                // 如果没有连接上，不触发通讯
                if (!isConnected) return;

                string url = Path.Combine(URL, "machineMonitorController/materialChange");

                // 构建数据
                ArrayList jsonArray = new ArrayList();

                var materialData = new
                {
                    machineSn = sysConfig.MachineSn,
                    StationId = sysConfig.StationId,
                    cellId = "",
                    uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                    data = new ArrayList()
                {
                    new
                    {
                        materialType = "Axis",
                        materialId = "SN2023",
                        LotNo = "2023-03",
                    }
                },
                    opName = "Admin",
                    opId = "Admin",
                    from = "MACHINE-MaterialChange-API",
                    apiVersion = "V1.0.1",

                };

                jsonArray.Add(materialData);
                string jsonData = JsonTool.ToJson(jsonArray);

                CommException(url, jsonData, r =>
                {
                    if (r.IsSuccess)
                    {
                    }
                });
            }
            else if (arg3 == "换料预警")
            {
                Warning();
            }

        }

        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="nodeName">工站名称</param>
        /// <param name="machineSn">机台SN</param>
        /// <param name="ct">CT统计</param>
        /// <param name="start">开始</param>
        /// <exception cref="NotImplementedException"></exception>
        private void WorkFlow_ProStatusChangedEvent(string nodeName, string machineSn, double ct, int start = 1, bool isSelf = true)
        {
            try
            {
                if (isSelf)
                {
                    MachineProductBeat(nodeName, machineSn, ct, start);
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        /// <summary>
        /// 产品出站
        /// </summary>
        private DateTime sTime = DateTime.Now;
        private void MController_ProStatEvent(ProductStat arg1, ImpactData arg2)
        {
            try
            {
                ProReport(sTime, DateTime.Now, arg1, arg2);
                sTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        /// <summary>
        /// 构建保压参数
        /// </summary>
        /// <returns></returns>
        private ArrayList BuildMatainData()
        {
            ArrayList arr = new ArrayList();

            if (maintains == null || maintains.Count == 0)
            {
                return arr;
            }

            foreach (var item in maintains)
            {
                arr.Add(new
                {
                    name = item.Name,
                    maxHP = item.MaxHP,
                    currentHP = item.CurrentHP,
                    maintainHP = item.MaintainHP,
                    usedHP = item.UsedHP,
                    unit = item.Unit
                });
            }

            return arr;
        }
        /// <summary>
        /// 注册
        /// 2、设备注册信息 API 接口
        /// </summary>
        public override void Register()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            string url = Path.Combine(URL, "machineMonitorController/machineRegister");

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var registerData = new
            {
                machineSn = sysConfig.MachineSn,
                stationName = sysConfig.MachineSn,
                StationId = sysConfig.StationId,
                StationName = sysConfig.StationName,
                LineCode = sysConfig.LineCode
            };
            jsonArray.Add(registerData);

            string jsonData = JsonTool.ToJson(jsonArray);

            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    Thread.Sleep(HeartSleep);

                    heartSource = new CancellationTokenSource();

                    // 开启心跳
                    StartHeart();

                    // 注册第一次状态
                    Activation(mController.MachineStatus, mController.MachineStatus, null);
                }
                else
                {
                    isConnected = false;
                    OnIsConntected(isConnected);
                }
            });
        }


        /// <summary>
        /// 心跳监控
        /// 1.设备心跳监测 API 接口
        /// </summary>
        public virtual void StartHeart()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            // 心跳URL
            string url = Path.Combine(URL, "machineMonitorController/machineHeartbeat");

            // 开一个心跳监控
            Task.Run(() =>
            {
                while (true)
                {
                    if (heartSource.IsCancellationRequested)
                    {
                        break;
                    }

                    ProcessHeart(url);

                    // 快速停止心跳检查
                    if (heartSource.IsCancellationRequested)
                    {
                        break;
                    }

                    Thread.Sleep(HeartSleep);
                }
            }, heartSource.Token);
        }

        /// <summary>
        /// 心跳处理
        /// </summary>
        /// <param name="url"></param>
        private void ProcessHeart(string url)
        {
            var postData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                stationName = sysConfig.MachineSn,
                deviceMac = sysConfig.DeviceMac,
                deviceIp = sysConfig.DeviceIP,
                from = "MACHINE-HeartBeat-API",
            };

            var jsonData = BuildJsonData(postData);

            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                    ProcessCmd(r);
                }
                else
                {
                    // 心跳检查失败，关闭连接
                    isConnected = false;
                }
            });
        }

        /// <summary>
        /// 构建驾驶舱JSON格式数据
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        private string BuildJsonData(params object[] jsonData)
        {
            ArrayList arr = new ArrayList();
            foreach (var item in jsonData)
            {
                arr.Add(item);
            }

            return JsonTool.ToJson(arr);
        }

        #region 在籍节拍
        private void MachineProductBeat(string nodeName, string machineSn, double ct, int start = 1)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            // 心跳URL
            string url = Path.Combine(URL, "machineMonitorController/machineProductBeat");
            var postData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                cellId = machineSn == "NG" ? "" : machineSn,
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                deviceMac = sysConfig.DeviceMac,
                positionData = new ArrayList()
                {
                    new
                    {
                        nodeName = nodeName,
                        materialSn = machineSn=="NG"?"": machineSn,
                        time = ct.ToString(),
                        state = start.ToString(),
                    }
                },
                from = "MACHINE-ProductBeat-API",
                apiVersion = "V1.0.1",
            };

            var jsonData = BuildJsonData(postData);

            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }
        #endregion

        #region 切机相关

        /// <summary>
        /// 指令处理
        /// </summary>
        /// <param name="rStatus"></param>
        private void ProcessCmd(ResultStatus rStatus)
        {
            if (!string.IsNullOrEmpty(rStatus.ActionName))
            {
                switch (rStatus.ActionName)
                {
                    case "ToEngineer":
                        mController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Warning, $"{this.GetType().Name} 发送Stop!");
                        mController.Stop();
                        ReturnResult(rStatus.ActionName, true);
                        break;
                    case "ToRun":
                        mController.MotionEngine.OnLog(Common.DataStruct.Enums.LogType.Info, $"{this.GetType().Name} 发送Start!");
                        mController.Start();
                        ReturnResult(rStatus.ActionName, true);
                        break;
                    case "GetRecipes":
                        var recipeList = GetRecipes();
                        ReturnResult(rStatus.ActionName, true, "", recipeList);
                        break;
                    case "ChangeRecipe":
                        ChangeRecipe(rStatus);
                        break;
                    case "FA":
                        FTPUpLoad(rStatus);
                        break;
                }
            }
        }

        /// <summary>
        /// 返回切机结果
        /// </summary>
        /// <param name="action"></param>
        /// <param name="isSuccess"></param>
        private void ReturnResult(string action, bool isSuccess, string reason = "", ArrayList dataList = null)
        {
            string url = Path.Combine(URL, "machineMonitorController/machineOperationReturn");
            var postData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                stationName = sysConfig.MachineSn,
                deviceMac = sysConfig.DeviceMac,
                deviceIp = sysConfig.DeviceIP,
                actionName = action,
                actionResult = isSuccess ? "Success" : "Fail",
                recipeData = dataList,
                reason = "",
                from = "MACHINE-OPERATIONRESULT-API",
                apiVersion = "V1.0.0"

            };

            var jData = BuildJsonData(postData);
            CommException(url, jData, r =>
            {

            });
        }

        /// <summary>
        /// 一键切机启动
        /// </summary>
        private bool ToRun()
        {
            bool toRun=false;
            var eStatus = mController.MachineStatus;
            if (eStatus == EngineStatus.Running) return true;
            if (eStatus == EngineStatus.Ready)
            {
                mController.Start();
                toRun = true;
            }
            else
            {
                mController.Home();
                mController.Start();
            }
            return toRun;
        }

        /// <summary>
        /// 获取配方信息
        /// </summary>
        private ArrayList GetRecipes()
        {
            ArrayList listRecipe = new ArrayList();
            var listRecipes = mController.SysConfig.ListRecipe;
            if (mController.SysConfig.ListRecipe == null) return listRecipe;
            foreach (var recipe in listRecipes)
            {
                var data = new
                {
                    //projectName = recipe.ProjectName,//去掉
                    recipeName = recipe.RecipeName,
                    isActive = recipe.IsActive,
                };
                listRecipe.Add(data);
            }
            return listRecipe;
        }

        /// <summary>
        /// 切换配方
        /// </summary>
        private void ChangeRecipe(ResultStatus rStatus)
        {
            var eStatus = mController.MachineStatus;
            string errormsg = "";
            if (eStatus == EngineStatus.Running || eStatus == EngineStatus.Pause)//启动暂停状态不可切换配方
            {
                mController.Stop();
            }
            if (rStatus.ActionData1 != "")
            {
                mController.ChangeRecipe(rStatus.ActionData1);
                ReturnResult(rStatus.ActionName, true, "", null);
            }
            else
            {
                errormsg = $"配方不能为空！";
                ReturnResult(rStatus.ActionName, false, errormsg, null);
            }
        }
        #endregion

        #region FA上传
        private void FTPUpLoad(ResultStatus rStatus)
        {
            string localFilePath = "";
            try
            {
                if (rStatus.ActionData1 != "" && rStatus.ActionData2 != "")
                {
                    var listRecipes = mController.SysConfig.ListRecipe;
                    if (listRecipes != null)
                    {
                        var model = listRecipes.FirstOrDefault(m => m.IsActive == true);
                        if (model != null)
                        {
                            string uploadPicPath = Path.Combine("ftp://" + serverIp, rStatus.ActionData2, "Pic");
                            string uploadLogPath = Path.Combine("ftp://" + serverIp, rStatus.ActionData2, "Log");
                            if (!ftpTool.DirectoryExist(Path.Combine(rStatus.ActionData2, "Log")))//判断文件夹是否存在不存在就创建
                            {
                                ftpTool.MakeDir(rStatus.ActionData2, "Log");
                            }
                            if (!ftpTool.DirectoryExist(Path.Combine(rStatus.ActionData2, "Pic")))
                            {
                                ftpTool.MakeDir(rStatus.ActionData2, "Pic");
                            }

                            localFilePath = Path.Combine(model.RecipePath, "logs", $"{DateTime.Now.ToString("yyyy-MM-dd")}.log");
                            if (File.Exists(localFilePath))
                            {
                                ftpTool.UploadFile(Path.Combine(rStatus.ActionData2, "Log"), localFilePath);
                            }

                            if (ImagePath != "")
                            {
                                ftpTool.UploadFile(Path.Combine(rStatus.ActionData2, "Pic"), ImagePath);
                            }
                            ReturnResult(rStatus.ActionName, true);
                        }
                        else
                        {
                            ReturnResult(rStatus.ActionName, false, "未找到对应配方");
                        }
                    }
                    else
                    {
                        LogTool.Debug($"System:ftp targetURL:{Path.Combine(URL, rStatus.ActionData2)} localFilePath:{localFilePath}\r\n Errormsg:配方列表为空", GetSystem());
                    }

                }
            }
            catch (Exception ex)
            {
                ReturnResult(rStatus.ActionName, false, ex.Message);
                LogTool.Debug($"System:ftp targetURL:{Path.Combine(URL, rStatus.ActionData2)} localFilePath:{localFilePath}\r\n Errormsg:{ex.Message}", GetSystem());
            }
        }
        #endregion

        #region 知识图谱
        private void FTPUPLoadData(object obj)
        {
            var listRecipes = mController.SysConfig.ListRecipe;
            if (listRecipes != null)
            {
                var model = listRecipes.FirstOrDefault(m => m.IsActive == true);
                if (model != null)
                {
                    string path = Path.Combine(model.ProjectName,DateTime.Now.ToString("yyMMdd"));
                    string uploadCTPath = Path.Combine("CT", model.ProjectName);
                    string uploadProductPath = Path.Combine("Product", model.ProjectName);
                    if (!isConnected) return;
                    if (!ftpTool.DirectoryExist(uploadCTPath))//判断文件夹是否存在不存在就创建
                    {
                        //ftpTool.MakeDir("CT", model.ProjectName);
                    }
                    if (!ftpTool.DirectoryExist(uploadProductPath))
                    {
                        //ftpTool.MakeDir("Product", model.ProjectName);
                    }
                    FTPProductCSV(uploadProductPath, Path.GetDirectoryName(model.RecipePath));//上传产品
                    FTPCTCSV(uploadCTPath,Path.GetDirectoryName(model.RecipePath));//上传CT

                }
                else
                {
                }
            }
        }

        /// <summary>
        /// 自动上传产品
        /// </summary>
        /// <param name="uploadPath"></param>
        private void FTPProductCSV(string uploadPath, string projPath)
        {
            if (mController.SysConfig.ClassModels != null)
            {
                var model = mController.SysConfig.GetCurrentClass(DateTime.Now);
                if (model == null) return;
                string dataPath = Path.Combine(projPath, @"Datas\\AutoSave\\Product", $"{DateTime.Now.ToString("yyyy-MM-dd")}_{model.ClassName}.csv");
                if (File.Exists(dataPath))
                {
                    ftpTool.UploadFile(uploadPath, dataPath);
                    ftpTool.Rename(uploadPath, $"{DateTime.Now.ToString("yyyy-MM-dd")}_{model.ClassName}.csv", $"{DateTime.Now.ToString("yyMMddHHmmss")}_{model.ClassName}.csv");
                }

            }
        }

        /// <summary>
        /// 自动上传CT文件
        /// </summary>
        /// <param name="uploadPath"></param>
        private void FTPCTCSV(string uploadPath,string projPath)
        {
            string dataPath = Path.Combine(projPath, @"Datas\\AutoSave\\CT", DateTime.Now.ToString("yyyyMMdd"));
            if (Directory.Exists(dataPath))
            {
                string timeName = DateTime.Now.ToString("yyMMddHHmmss");
                var listmotion = mController.MotionEngine.GetStations();
                DirectoryInfo root = new DirectoryInfo(dataPath);
                FileInfo[] files = root.GetFiles();
                foreach (var file in files)
                {
                    string filename=file.Name.Replace(file.Extension,"");
                    var stationModule=listmotion.FirstOrDefault(x => x.Alias == filename);
                    if (stationModule != null)
                    {
                        ftpTool.UploadFile(uploadPath, file.FullName);
                        ftpTool.Rename(uploadPath, $"{filename}.csv", $"{filename + timeName}.csv");
                    }
                }
            }

        }
        #endregion

        #region CPK解析计算

        // CPK分析
        private void MController_ProductEvent(ProductInfo arg1, bool arg2)
        {
            try
            {
                ImagePath = arg1.ImagePath;
                //判断输出项有无新增 _mapDatas更改
                foreach (var item in arg1.Data)
                {
                    if (DataDict.ContainsKey(item.Key))
                    {
                        if (DataDict[item.Key].Count >= 32)
                        {
                            if (item.Value != null)
                            {
                                DataDict[item.Key].RemoveAt(0);
                                DataDict[item.Key].Add(Convert.ToDouble(item.Value));
                            }
                        }
                        else
                        {
                            if (item.Value != null)
                            {
                                DataDict[item.Key].Add(Convert.ToDouble(item.Value));
                            }
                        }
                    }
                    else
                    {
                        if (item.Value != null)
                        {
                            List<double> listData = new List<double>();
                            if (double.TryParse(item.Value.ToString(), out var d))
                            {
                                listData.Add(d);
                                DataDict.Add(item.Key, listData);
                            }
                        }
                    }

                }
                CPKParaUpLoad(arg1, arg2);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        /// <summary>
        /// CPK分析上传
        /// </summary>
        /// <param name="productInfo"></param>
        /// <param name="arg2"></param>
        public void CPKParaUpLoad(ProductInfo productInfo, bool arg2)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, "machineMonitorController/machineParameter");

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var machineParaData = new
            {
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                cellId = productInfo.Barcode,
                result = productInfo.Result.Result ? "OK" : "NG",
                startTime = productInfo.EnterTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = productInfo.OutTime.ToString("yyyy-MM-dd HH:mm:ss"),
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                cycleTime = productInfo.GetCT(),
                from = "MACHINE-Parameter-API",
                type = "R_TEST_PARAM",
                apiVersion = "V1.0.0",
                data = "",
                cpkData = CalcCPK(),
                maintainComponent = BuildMatainData()
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
        /// 判断有无历史数据，并添加历史数据
        /// </summary>
        private void JudgeIsNeedGetHistory(List<MapData> _mapDatas)
        {
            if (DataDict.Count == 0) return;

            long count = 0;
            var productInfos = new List<TbProductInfo>();
            productInfos = reporitory.GetPage<TbProductInfo>(x => x.CreateTime < DateTime.Now, x => x.ID, 1, 32, out count).ToList();//查询所有最新20条数据

            if (productInfos == null || productInfos.Count == 0) return;

            foreach (var product in productInfos)
            {
                //有测量数据进入
                if (string.IsNullOrEmpty(product.Data)) continue;
                var splitGroup = product.Data.Split('|');
                foreach (var item in splitGroup)
                {
                    var tempValue = item.Split(':');
                    if (tempValue.Length != 2) continue;

                    string mapKey = tempValue[0];
                    string mapValue = tempValue[1];

                    // 数据列包含此项，更新字典值
                    var outputmodel = _mapDatas.FirstOrDefault(x => x.Alias == tempValue[0]);
                    if (outputmodel == null) continue;

                    if (string.IsNullOrEmpty(mapValue)) continue;

                    if (DataDict.ContainsKey(mapKey))
                    {
                        if (DataDict[mapKey].Count >= 32)
                        {
                            DataDict[mapKey].RemoveAt(0);
                        }

                        if (double.TryParse(mapValue, out var d))
                        {
                            DataDict[mapKey].Add(d);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(mapValue))
                        {
                            List<double> listData = new List<double>();
                            if (double.TryParse(mapValue, out var d))
                            {
                                listData.Add(d);
                                DataDict.Add(mapKey, listData);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 计算CPK
        /// </summary>
        /// <returns></returns>
        private ArrayList CalcCPK()
        {
            var cpklist = new ArrayList();
            if (DataDict != null && DataDict.Count > 0)
            {
                foreach (var dicValue in DataDict)
                {
                    if (_mapDatas != null && _mapDatas.Count > 0)
                    {
                        var outputmodel = _mapDatas.FirstOrDefault(x => x.Alias == dicValue.Key);
                        if (outputmodel != null)
                        {
                            double target = outputmodel.StandardValue;
                            double usl = outputmodel.PositivestandardDeviation + outputmodel.StandardValue;
                            double lsl = outputmodel.StandardValue - outputmodel.NegativestandardDeviation;
                            double avg = 0;//平均数
                            double stdev = 0;//标准偏差
                            double sum = 0;//总和
                            double Tsum = 0;//值与平均数差的平方综合
                            var listData = dicValue.Value;
                            if (listData != null && listData.Count >= 2)
                            {
                                foreach (var item in listData)
                                {
                                    sum += item;
                                }
                                avg = sum / listData.Count;
                                foreach (var item in listData)
                                {
                                    Tsum += ((item - avg) * (item - avg));
                                }
                                stdev = Convert.ToSingle(Math.Sqrt(Tsum / (listData.Count - 1)).ToString());
                                double cpk = 0;
                                if (stdev != 0)
                                {
                                    cpk = Math.Round((1 - Math.Abs((avg - (usl + lsl) / 2) / (usl - lsl) * 2)) * ((usl - lsl) / (6 * stdev)), 4);
                                }
                                double cuurntvalue = dicValue.Value.Last();
                                var cpkData = new
                                {
                                    name = dicValue.Key,
                                    Target = target.ToString(),
                                    USL = usl.ToString(),
                                    LSL = lsl.ToString(),
                                    value = cuurntvalue.ToString(),
                                    CPK = cpk.ToString(),
                                };
                                cpklist.Add(cpkData);
                            }

                        }
                    }

                }
            }
            return cpklist;
        }
        #endregion

        #region 上产指标

        /// <summary>
        /// 统计报文
        /// 4、设备生产指标 API 接口
        /// </summary>
        public void ProReport(DateTime startTime, DateTime endTime, ProductStat productStat, ImpactData impactData)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            // 心跳URL
            string url = Path.Combine(URL, "machineMonitorController/machineIndicator");

            var mData = mController.GetUploadProductionQuota(startTime, endTime);

            double PF = Math.Round(sysConfig.CT / productStat.RealCT, 3);//性能表现率
            double AV = Math.Round(impactData.GetImpactPer()/100, 3);//时间使用率
            var pYeild = ProStatToYeild(productStat);
            double Yield =Math.Round(pYeild.GetYield()/100,3);

            var postData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                startTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = endTime.ToString("yyyy-MM-dd HH:mm:ss"),
                inputQty = "1",
                outputQty = "1",
                passQty = productStat.Result ? "1" : "0",
                failQty = productStat.Result ? "0" : "1",
                retestQty = "0",
                targetUph = mData.TargetUph.ToString(),
                uph = mData.TargetUph.ToString(),
                retestRate = mData.RetestRate.ToString(),

                oeeData = new ArrayList()
                {
                    new
                    {
                        pf = PF.ToString(),
                        av = AV.ToString(),
                        oee = Math.Round(AV * PF * Yield, 3).ToString(),
                        yield = Yield.ToString(),
                    }
                },
                ThrowCalc = ThrowCalc(mData),
                from = "MACHINE - INDICATOR - API",
                apiVersion = "V1.0.1"
            };

            var jsonData = BuildJsonData(postData);

            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }
        private TbProductYeild ProStatToYeild(ProductStat pStat)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in pStat.ThrowMaterialDic)
            {
                sb.Append($"{item.Key}_{item.Value}|");
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return new TbProductYeild()
            {
                ID = 1,
                OKCount = pStat.OKCount,
                AllCount = pStat.AllCount,
                NGCount = pStat.NGCount,
                ThrowMaterial = pStat.ThrowMaterialDic,
                Data = sb.ToString()
            };
        }


        private ArrayList ThrowCalc(UploadProductionQuota mData)
        {
            ArrayList list = new ArrayList();
            if (mData.throwDic != null)
            {
                foreach (var item in mData.throwDic)
                {
                    var tossingData = new
                    {
                        parameter = item.Key.ToString(),
                        value = item.Value.ToString(),
                    };
                    list.Add(tossingData);
                }
            }
            return list;
        }
        #endregion

        #region 设备保养
        /// <summary>
        /// 设备保养
        /// </summary>
        /// <param name="maintain"></param>
        private void DeviceEngine_MaintainZeroEvent(IMaintain maintain)
        {
            try
            {
                Maintain(maintain);
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        private void Maintain(IMaintain maintain)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            string url = Path.Combine(URL, "machineMonitorController/machineMaintain");

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var maintainData = new
            {
                machineSn = sysConfig.MachineSn,
                StationId = sysConfig.StationId,
                cellID = "",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                maintainType = "预防性维护",
                maintainDescription = $"{maintain.Name}清零养护完成！",
                maintainComponent = maintain.Name,
                startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                maintainerName = "Admin",
                maintainerId = "",
                maintainerType = "",
                from = "MACHINE-MAINTAIN-API",
                apiVersion = "V1.0.1"

            };
            jsonArray.Add(maintainData);

            string jsonData = JsonTool.ToJson(jsonArray);

            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        #endregion

        #region  备份
        /// <summary>
        /// 设备申请备份
        /// </summary>
        public void ApplyBackUpProject(string BackUpPath)
        {

            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            string url = Path.Combine(URL, "machineMonitorController/machineBackupApply");

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var registerData = new
            {
                machineSn = sysConfig.MachineSn,
                StationId = sysConfig.StationId,
                cellId = "",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                from = "MACHINE-BACKUPAPPLY-API",
                apiVersion = "V1.0.1"
            };
            jsonArray.Add(registerData);

            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    if (!string.IsNullOrEmpty(r.FtpPath))
                    {
                        string msg = "";
                        bool issuccess = ZIPFile(r.FtpPath, BackUpPath, out msg);
                        BackupFinishProject(issuccess, msg);
                    }
                }
                else
                {
                    throw new FriendlyException("申请备份失败！");
                }
            });
        }


        /// <summary>
        /// 备份完成
        /// </summary>
        public void BackupFinishProject(bool isSuccess, string msg)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            string url = Path.Combine(URL, "machineMonitorController/machineBackupFinish");

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var registerData = new
            {
                machineSn = sysConfig.MachineSn,
                StationId = sysConfig.StationId,
                cellId = "",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                result = isSuccess ? "Success" : "Fail",
                reason = msg,
                from = "MACHINE-BACKUPFINISH-API",
                apiVersion = "V1.0.1"
            };
            jsonArray.Add(registerData);

            string jsonData = JsonTool.ToJson(jsonArray);

            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {

                }
            });
        }

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="ftpLoadPath"></param>
        /// <param name="backUpPath"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ZIPFile(string ftpLoadPath, string backUpPath, out string msg)
        {
            try
            {
                //先创建文件夹并复制，数据库可能被使用导致无法压缩成功
                string dstPath = Path.Combine(Path.GetDirectoryName(backUpPath), "BackUpProj");
                Luster.Common.Tools.FolderTool.CopyFiles(backUpPath, dstPath);         
                string fileName = Path.GetFileName(backUpPath);
                string logpath = Path.Combine(dstPath,fileName,"logs");
                if (Directory.Exists(logpath))//防止文件过大删除日志
                {
                    Directory.Delete(logpath, true);
                }
                string desinationpath = Path.Combine(Path.GetDirectoryName(backUpPath), Path.GetFileName(ftpLoadPath));
                Luster.Common.Tools.ZipTool.CompressFile(desinationpath, new string[] { backUpPath }, "");
                ftpTool.UploadFile(Path.GetDirectoryName(ftpLoadPath), desinationpath);
                msg = "NA";
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                throw new FriendlyException("备份失败！");
                return false;
            }
        }
        #endregion

        #region 设备预警

        private void Warning()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            string url = Path.Combine(URL, "machineMonitorController/warning");

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var maintainData = new
            {
                machineSn = sysConfig.MachineSn,
                StationId = sysConfig.StationId,
                cellID = "",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                warningDescription = "工位1大膜需要换料",
                from = "MACHINE-WARNING-API",
                apiVersion = "V1.0.1"

            };
            jsonArray.Add(maintainData);

            string jsonData = JsonTool.ToJson(jsonArray);

            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        #endregion
    }
}
