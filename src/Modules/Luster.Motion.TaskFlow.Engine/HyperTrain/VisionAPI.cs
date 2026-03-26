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
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Path = System.IO.Path;

namespace Luster.Motion.TaskFlow.Engine
{

    /// <summary>
    /// Vision 系统API
    /// </summary>
    public class VisionAPI : BaseMESAPI
    {
        public string WIP = "";
        Instance picUploadInstance;
        JsonResultModel resultModel;

        private ICacheManager _cacheManager;

        /// <summary>
        /// 错误内容
        /// </summary>
        private readonly IErrorManager _errorMangaer;

        public VisionAPI(ICacheManager cacheManager, IErrorManager errorManager)
        {
            _cacheManager = cacheManager;
            _errorMangaer = errorManager;
            //  picUploadInstance = new Instance();
            //  resultModel = new JsonResultModel();
            //  picUploadInstance.Init(sysConfig.MachineName,sysConfig.MachineSn,sysConfig.VendorName,sysConfig.Product,sysConfig.StationId,sysConfig.StationName,sysConfig.LineCode,sysConfig.Floor,sysConfig.Area,sysConfig.SiteCode);
        }

        public override string GetSystem()
        {
            return "Vision";
        }

        protected override void Register(IMotionController motionController)
        {
            base.Register(motionController);
            motionController.PropertyChanged -= MotionController_PropertyChanged;
            motionController.PropertyChanged += MotionController_PropertyChanged;

            motionController.MotionEngine.TimeStatisEvent -= MotionEngine_TimeStatisEvent;
            motionController.MotionEngine.TimeStatisEvent += MotionEngine_TimeStatisEvent;

            // 产能数据
            motionController.ProStatEvent -= MotionController_ProStatEvent;
            motionController.ProStatEvent += MotionController_ProStatEvent;

            motionController.ProductEvent -= MotionController_ProductEvent;
            motionController.ProductEvent += MotionController_ProductEvent;

            motionController.MachineStatusEvent -= MotionEngine_ProBlockEvent;
            motionController.MachineStatusEvent += MotionEngine_ProBlockEvent;
        }

        private void MotionController_ProductEvent(ProductInfo arg1, bool arg2)
        {
            string wip = "N999999999";
            ProductInfoColletct(arg1.Result, out wip);
            MachineIndicatorUpload();

            //if(wip!= "N999999999")
            //{
            //    try
            //    {
            //        UploadPic(sysConfig.ImagePath, wip);
            //    }
            //    catch (Exception ex) { }
            //}
        }

        /// <summary>
        /// 产品阻塞状态切换
        /// </summary>
        /// <param name="blockCt">阻塞时间</param>
        /// <param name="errCode">错误编码 block/</param>
        /// <param name="reason">阻塞原因</param>
        private void MotionEngine_ProBlockEvent(IMotionController module, TrainRunMode runStatus, string reason)
        {
            try
            {
                // 是否阻塞
                if (reason == "block")
                {
                    MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Idle, "Block", "");
                }
                //是否前站没有物料
                else if (reason == "idle")
                {
                    MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Idle, "Idle", "");
                }
                else
                {
                    MachineStatusUpload(TrainRunMode.Idle, TrainRunMode.Running, "", "");
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }


        protected override void ManualDown(object arg1, SystemOperation op, string reason)
        {
            base.ManualDown(arg1, op, reason);

            // 如果暂停，则记录暂停原因
            if (op == DataStruct.Enums.SystemOperation.Pause)
            {
                if (reason == SystemConsts.PlanDT)
                {
                    // 换料暂停
                    MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Idle, nameof(SystemConsts.PlanDT), reason);
                }
                else if (reason == SystemConsts.NoonBreak)
                {
                    // 午休
                    MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Idle, nameof(SystemConsts.NoonBreak), reason);
                }
            }
        }

        private ProductStat productStat;

        /// <summary>
        /// 获取产能数据
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MotionController_ProStatEvent(Models.ProductStat arg1, Models.ImpactData arg2)
        {
            productStat = arg1;
        }

        /// <summary>
        /// CT统计上传数据
        /// </summary>
        /// <param name="obj"></param>
        private void MotionEngine_TimeStatisEvent(List<DataStruct.DataModels.StationTime> obj)
        {
            try
            {
                if (obj.Count > 0)
                {
                    string sn = obj[0].SN;
                    string station = obj[0].Station;

                    ProductParaUpload(obj[0].SN, GetCTData(obj));
                }

            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
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
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
            }
        }

        private WebTool webTool;

        /// <summary>
        /// 获取SN及对应的CT信息
        /// </summary>
        /// <param name="tbCTInfos"></param>
        /// <returns></returns>
        public object GetCTData(List<DataStruct.DataModels.StationTime> tbCTInfos)
        {
            string WIP = "None";
            ArrayList ctList = new ArrayList();

            if (tbCTInfos.Count == 0) return ctList;
            int sort = 1;
            string resDes = "";
            string startStation = tbCTInfos[0].Station.Replace("-Start", "").Replace("-Middle", "").Replace("-End", "").Replace("-Key", "");

            //LCG码转换成WIP码
            foreach (StationTime stationTime in tbCTInfos)
            {
                if (stationTime.SN != "None")
                {
                    WIP = LcgToWip(stationTime.SN);
                    if (WIP == "IsEmptyMode")
                    { WIP = "None"; }
                    break;
                }
            }

            //创建工站开始

            var startTarget_CT = new
            {
                parameter = $"00_{startStation}_工站开始_Target_CT",
                value = "0"
            };
            ctList.Add(startTarget_CT);

            var startGap = new
            {
                parameter = $"00_{startStation}_工站开始_Gap",
                value = "0"
            };
            ctList.Add(startGap);

            var startActualCT = new
            {
                parameter = $"00_{startStation}_工站开始_Actual_CT",
                value = "0"
            };
            ctList.Add(startActualCT);

            var startSN = new
            {
                parameter = $"00_{startStation}_工站开始_SN",
                value = WIP
            };
            ctList.Add(startSN);


            var startStartTime = new
            {
                parameter = $"00_{startStation}_工站开始_开始时间",
                value = tbCTInfos[0].StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
            };
            ctList.Add(startStartTime);

            var startEndTime = new
            {
                parameter = $"00_{startStation}_工站开始_结束时间",
                value = tbCTInfos[0].StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
            };
            ctList.Add(startEndTime);


            //创建中间内容
            foreach (StationTime u in tbCTInfos)
            {
                if (u.Station != u.Module)
                {
                    if (u.Station.Contains("Key"))
                    {
                        resDes = "Key";
                    }
                    else
                    {
                        resDes = "";
                    }
                    string station = u.Station.Replace("-Start", "").Replace("-Middle", "").Replace("-End", "").Replace("-Key", "");
                    string strSort = sort.ToString().PadLeft(2, '0');

                    var targetCt = new
                    {
                        parameter = $"{strSort}_{station}_{u.Module}_Target_CT",
                        value = Math.Round(u.CT / 1000.0, 3).ToString()
                    };

                    ctList.Add(targetCt);

                    var gap = new
                    {
                        parameter = $"{strSort}_{station}_{u.Module}_Gap",
                        value = Math.Round(((u.Time - u.CT) / (1000.0)), 3).ToString()
                    };

                    ctList.Add(gap);

                    var actualCt = new
                    {
                        parameter = $"{strSort}_{station}_{u.Module}_Actual_CT",
                        value = Math.Round(u.Time / 1000.0, 3).ToString()
                    };

                    ctList.Add(actualCt);

                    var sn = new
                    {
                        parameter = $"{strSort}_{station}_{u.Module}_SN",
                        value = WIP
                    };

                    ctList.Add(sn);

                    var moduleStartTime = new
                    {
                        parameter = $"{strSort}_{station}_{u.Module}_开始时间",
                        value = u.StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                    };
                    ctList.Add(moduleStartTime);

                    var moduleEndTime = new
                    {
                        parameter = $"{strSort}_{station}_{u.Module}_结束时间",
                        value = u.EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
                    };
                    ctList.Add(moduleEndTime);

                    foreach (var ext in u.ExtParams)
                    {
                        // 排除掉
                        string val = ext.Value?.ToString();
                        if (double.TryParse(val, out var v) && v > 0)
                        {
                            var extItem = new
                            {
                                parameter = $"{strSort}_{station}_{u.Module}_{ext.Key}",
                                value = val
                            };

                            ctList.Add(extItem);
                        }
                    }
                    sort++;
                }
            }

            string endStation = tbCTInfos[0].Station.Replace("-Start", "").Replace("-Middle", "").Replace("-End", "").Replace("-Key", "");


            //创建结束内容
            var endTarget_CT = new
            {
                parameter = $"{sort.ToString().PadLeft(2, '0')}_{endStation}_工站结束_Target_CT",
                value = "0"
            };
            ctList.Add(endTarget_CT);

            var endGap = new
            {
                parameter = $"{sort.ToString().PadLeft(2, '0')}_{endStation}_工站结束_Gap",
                value = "0"
            };
            ctList.Add(endGap);

            var endActualCT = new
            {
                parameter = $"{sort.ToString().PadLeft(2, '0')}_{endStation}_工站结束_Actual_CT",
                value = "0"
            };
            ctList.Add(endActualCT);

            var endSN = new
            {
                parameter = $"{sort.ToString().PadLeft(2, '0')}_{endStation}_工站结束_SN",
                value = WIP
            };
            ctList.Add(endSN);

            var endStartTime = new
            {
                parameter = $"{sort.ToString().PadLeft(2, '0')}_{endStation}_工站结束_开始时间",
                value = tbCTInfos[tbCTInfos.Count - 1].EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
            };
            ctList.Add(endStartTime);
            var endEndTime = new
            {
                parameter = $"{sort.ToString().PadLeft(2, '0')}_{endStation}_工站结束_结束时间",
                value = tbCTInfos[tbCTInfos.Count - 1].EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff")
            };
            ctList.Add(endEndTime);

            // 计算工站的总CT
            double totalCT = 0;
            if (tbCTInfos.Count > 1)
            {
                foreach (var ctInfo in tbCTInfos)
                {
                    totalCT = ctInfo.Time + totalCT;
                }
            }

            object jsonData = new
            {
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                cellId = "1",
                appVersion = ApiVision,
                materialSn = WIP,
                carrierSn = "NULL",
                startTime = tbCTInfos[0].StartTime.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                endTime = tbCTInfos[tbCTInfos.Count - 1].EndTime.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                cycleTime = Convert.ToInt32(totalCT).ToString(),
                result = "PASS",
                resultDescription = resDes,
                from = "MACHINE-PARAMETER-API",
                type = "R_TEST_PARAM",
                apiVersion = sysConfig.ApiVersion,
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                data = ctList
            };

            ArrayList array = new ArrayList();

            array.Add(jsonData);
            return array;
        }


        /// <summary>
        /// 1.设备心跳监测
        /// </summary>
        protected override void MachineHeartbeatUpload()
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
                stationId = sysConfig.StationId,
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
                if (r.IsSuccess)
                {
                }
            });
        }

        protected override void OnLog(string message, string url = "")
        {
            // 针对心跳，CTLog，机台状态，单独记录 
            if (url.Contains("machineHeartbeat") || url.Contains("machineParameter") || url.Contains("machineStatus"))
            {
                // 异常
                LogTool.Debug(message, "VisionHeart");
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
            if (!isConnected) return;

            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineRegister");
            ArrayList paramList = new ArrayList();

            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var machineParaData = new
            {
                siteCode = sysConfig.SiteCode,
                area = sysConfig.Area,
                machineType = sysConfig.MachineType,
                manageDept = sysConfig.ManageDept_Vision,
                registerType = "INSERT",
                stationType = sysConfig.stationType,
                osVersion = "V1.2",
                appVersion = sysConfig.SoftVersion,
                configVersion = "V1.2",
                from = "MACHINE-REGISTER-API",
                apiVersion = sysConfig.ApiVersion,
                product = sysConfig.Product_Vision,
                floor = sysConfig.Floor,
                lineCode = sysConfig.LineCode,
                machineSn = sysConfig.MachineSn,
                stationName = sysConfig.StationType,
                vendorName = sysConfig.VendorName,
                machineName = sysConfig.MachineName,
                stationId = sysConfig.StationId,
                deviceIp = sysConfig.DeviceIP,
                deviceMac = sysConfig.DeviceMac,
                addBy = "-1",
                editBy = "-1",
                addDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                editDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            jsonArray.Add(machineParaData);
            string jsonData = JsonTool.ToJson(jsonArray);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });
        }

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

                    if (mController.AlarmInfo != null)
                    {
                        var err = _errorMangaer.GetErrorDetail(mController.AlarmInfo, GetSystem());
                        errCode = err.Code;
                        errInfo = err.Message;
                    }

                    // 停机后不在上传
                    if (dst == DataStruct.EngineStatus.Stop || srcStatus == dstStatus)
                    {
                        return;
                    }

                    MachineStatusUpload(srcStatus, dstStatus, errCode, errInfo);
                }
            }
            catch (Exception ex)
            {
                OnLog(ex.Message);
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
            //var machineParaData = new
            //{
            //    machineSn = sysConfig.MachineSn,
            //    stationId = sysConfig.StationId,
            //    cellId = "1",
            //    uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //    fromStatus = (int)src > 5 ? 5 : (int)src,
            //    toStatus = (int)dst > 5 ? 5 : (int)dst,
            //    statusDescription = "",
            //    status = (int)dst > 5 ? 5 : (int)dst,
            //    erroCode = errCode,
            //    errorMessage = errInfo,
            //    maintainerName = "ZS",
            //    maintenanceId = "1",
            //    maintenanceType = "",
            //    from = "MACHINE-STATUS-API",
            //    apiVersion = ApiVision
            //};

            // Vision Status 切换不能用数字
            var machineParaData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                cellId = "1",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                fromStatus = ((int)src > 5 ? TrainRunMode.Down : src).ToString(),
                toStatus = ((int)dst > 5 ? TrainRunMode.Down : dst).ToString(),
                statusDescription = "",
                status = ((int)dst > 5 ? 5 : (int)dst).ToString(),
                errorCode = errCode,
                errorMessage = errInfo,
                maintainerName = "ZS",
                maintenanceId = "1",
                maintenanceType = "",
                from = "MACHINE-STATUS-API",
                apiVersion = sysConfig.ApiVersion
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
        /// 4.设备生产指标
        /// </summary>
        public void MachineIndicatorUpload()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineIndicator");
            ArrayList paramList = new ArrayList();
            int tossCount = 0;
            foreach (int val in productStat.ThrowMaterialDic.Values)
            {
                tossCount += val;
            }
            // 构建数据
            ArrayList jsonArray = new ArrayList();
            var machineParaData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                cellId = "",
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                startTime = DateTime.Now.AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                inputQty = productStat.AllCount.ToString(),
                outputQty = productStat.AllCount.ToString(),
                passQty = productStat.OKCount.ToString(),
                failQty = productStat.NGCount.ToString(),
                tossingQty = tossCount.ToString(),
                retestQty = "2",
                targetUph = "650",
                uph = productStat.RealCT.ToString(),
                yield = "0.9932",
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
        public void MachineParaUpload(string name, object srcV, object newV)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineControl");
            string jsonData = JsonTool.ToJson(GetSingleDeviceControlPara(name, srcV, newV));
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
        private object GetDeviceControlPara()
        {
            ArrayList ctList = new ArrayList();

            var vAxises = mController.MotionEngine.DeviceEngine.GetVDevices<VAxis>();
            foreach (var item in vAxises)
            {
                foreach (var pItem in item.Positions)
                {

                    //创建工站开始
                    var addContent = new
                    {
                        parameter = $"{item.Name}_{pItem.Name}",
                        preValue = 0,
                        aftValue = pItem.Position,
                        changeReason = "Repair"
                    };
                    ctList.Add(addContent);
                }
            }

            object jsonData = new
            {
                uploadDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                cellId = "1",
                appVersion = ApiVision,
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
                stationId = sysConfig.StationId,
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
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineParameter");
            string jsonData = JsonTool.ToJson(data);
            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
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
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineCall");
            // 构建数据
            ArrayList jsonArray = new ArrayList();

            var machineCallData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
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
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineMaintain");
            // 构建数据
            ArrayList jsonArray = new ArrayList();
            var machineCallData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
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
        /// </summary>
        public void MachineInspect()
        {            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/machineInspect");

            // 构建数据
            ArrayList jsonArray = new ArrayList();
            var machineInspectData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
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
            WIP = "N999999999";
            if (!mController.GetCurrentMode().Contains(SystemConsts.ProductMode)) return;

            // 如果没有连接上，不触发通讯
            if (!isConnected) return;
            string url = Path.Combine(URL, sysConfig.VisionExtra, "machineMonitorController/productInformation");

            string wip = "N999999999";
            //如果未扫到码，直接赋值 N999999999
            if (stationResult.ProCode.StartsWith("NG_") && stationResult.ProCode.Length == 20)
            {
                wip = "N999999999";
            }
            else
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


            // 构建数据
            ArrayList jsonArray = new ArrayList();

            //如果出料时，本站结果为OK，则把NGCode和Errmsg全部清空，防止任务里面忘记清空
            if (stationResult.Result)
            {
                stationResult.NgCode = "";
                stationResult.ErrMsg = "";
            }

            var machineInspectData = new
            {
                productSn = wip,//二维码，
                locationId = "1",//穴号
                fxStationType = sysConfig.StationType,//
                inStationType = sysConfig.InsightType,
                machineSn = sysConfig.MachineSn,
                stationID = sysConfig.StationId,
                cellId = "1",//工位序号
                result = stationResult.IsToss ? "TOSSING" : stationResult.Result ? "PASS" : "FAIL",
                errorCode = stationResult.NgCode,//报错代码
                errorDesc = stationResult.ErrMsg,//报错描述
                startTime = stationResult.EnterTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                overlayVersion = sysConfig.BaliVersion,
                apiVersion = sysConfig.ApiVersion,
                isAudit = 0
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
        /// 
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
        /// CT 信息
        /// </summary>
        public void MachineCT()
        {

        }
    }
}
