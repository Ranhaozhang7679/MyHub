using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Luster.Motion.TaskFlow.Engine
{
    public class HiveAPI : BaseMESAPI
    {
        public string ApiVision = "";
        public static string token = "";
        private ICacheManager _cacheManager;
        private string PDCANAME = "Extend_PDCA启用";


        private static int curStatus = 0;
        private static int latestStatus = 0;
        public static int seed = 0;
        public static Random rand = new Random();


        /// <summary>
        /// 错误内容
        /// </summary>
        private readonly IErrorManager _errorMangaer;

        public HiveAPI(ICacheManager cacheManager, IErrorManager errorManager)
        {
            ApiVision = "Luster_" + SoftVersionTool.GetVersion("LusterMotion.exe");
            _cacheManager = cacheManager;
            _errorMangaer = errorManager;
        }


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
            if (!PDCAEnable()) return;
            if (dst == EngineStatus.Alarm && mController.AlarmInfo != null)
            {
                var err = _errorMangaer.GetErrorDetail(mController.AlarmInfo, GetSystem());
                errCode = err.Code;
                errMsg = err.Message;
            }

            MachineStatusChange(errCode, errMsg, src, dst);
        }


        //1.1
        /// <summary>
        /// 状态切换，用于点击停止/暂停/启动按钮
        /// </summary>
        /// <param name="alarmCode"></param>
        /// <param name="alarmInfo"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void MachineStatusChange(string alarmCode, string alarmInfo, EngineStatus src, EngineStatus dst, bool IsQulification = false)
        {
            base.StatusChanged(mController, src, dst);
            // 如果没有连接上，不触发通讯
            if ((!isConnected || !PDCAEnable()) && !IsQulification) return;
            string url = Path.Combine(URL, "v5/capture/machinestate");

            //自动运行时，发送1
            if (dst == EngineStatus.Running)
            {
                latestStatus = 1;
            }
            //空闲，发送2
            else if (dst == EngineStatus.Idle)
            {
                latestStatus = 2;
            }
            //报警，发送5
            else if (dst == EngineStatus.Alarm || dst == EngineStatus.Pause)
            {
                latestStatus = 5;
            }
            //状态不同时，上传
            if (curStatus != latestStatus)
            {
                curStatus = latestStatus;
                //构建发送数据
                var statusData = new
                {
                    machine_state = curStatus,
                    state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                    data =
                    new
                    {
                        error_message = alarmInfo,
                        code = alarmCode,
                        gh_station_id = sysConfig.StationId,
                    },
                };
                string jsonData = JsonTool.ToJson(statusData);
                CommException(url, jsonData, (r) =>
                {
                    if (r.IsSuccess)
                    {
                        heartSource?.Cancel();
                        heartSource = new CancellationTokenSource();
                    }
                });
            }
        }

        //1.2
        /// <summary>
        /// 状态切换，根据PLC的下游要料，本站有料，本站要料，上游有料进行自动判断
        /// </summary>
        /// <param name="module"></param>
        /// <param name="runStatus"></param>
        /// <param name="reason"></param>
        public void MotionEngine_ProBlockEvent(IMotionController module, TrainRunMode runStatus, string reason)
        {

            //自动运行时，发送1
            if (runStatus == TrainRunMode.Running)
            {
                latestStatus = 1;
            }
            //空闲，发送2
            else if (runStatus == TrainRunMode.Idle)
            {
                latestStatus = 2;
            }


            // 如果没有连接上，不触发通讯
            if (!isConnected || !PDCAEnable()) return;
            string url = Path.Combine(URL, "v5/capture/machinestate");
            //状态不同时，上传
            if (curStatus != latestStatus)
            {
                curStatus = latestStatus;
                //构建发送数据
                var statusData = new
                {
                    machine_state = curStatus,
                    state_change_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                    data =
                    new
                    {
                        error_message = "",
                        code = "",
                        gh_station_id = sysConfig.StationId,
                    },
                };
                string jsonData = JsonTool.ToJson(statusData);
                CommException(url, jsonData, (r) =>
                {
                    if (r.IsSuccess)
                    {
                        heartSource?.Cancel();
                        heartSource = new CancellationTokenSource();
                    }
                });
            }
        }
        /// <summary>
        /// 2.Error产生
        /// </summary>
        /// <param name="alarmInfo"></param>
        protected override void AlarmProcEvent(AlarmInfo alarmInfo, bool isOK = true)
        {
            //error or toss main part only
            string currentSeverity = "error";
            base.AlarmProcEvent(alarmInfo, isOK);
            // 如果没有连接上，不触发通讯
            if (!isConnected || !PDCAEnable()) return;
            string url = Path.Combine(URL, "v5/capture/errordata");
            //构建发送数据
            var errorData = new
            {
                message = alarmInfo.Message,
                code = alarmInfo.AlarmCode,
                severity = currentSeverity,
                occurrence_time = alarmInfo.StartTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                resolved_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                },
            };
            string jsonData = JsonTool.ToJson(errorData);
            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });
        }

        /// <summary>
        /// 2.1 主要用于抛料（抛主料和抛小料）
        /// </summary>
        /// <param name="errType">error or tossing</param>
        /// <param name="errCode"></param>
        /// <param name="errInfo"></param>
        private void AlarmEvent(string errType, string errCode, string errInfo)
        {
            //主料:error;小料:tossing;
            string currentSeverity = errType;
            // 如果没有连接上，不触发通讯
            if (!isConnected || !PDCAEnable()) return;
            string url = Path.Combine(URL, "v5/capture/errordata");
            //构建发送数据
            var errorData = new
            {
                message = errInfo,
                code = errCode,
                severity = currentSeverity,
                occurrence_time = DateTime.Now.AddMilliseconds(-100).ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                resolved_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                },
            };
            string jsonData = JsonTool.ToJson(errorData);
            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });

        }



        /// <summary>
        /// 3.出料信息
        /// </summary>
        /// <param name="info"></param>
        protected override void MController_ProductEvent(ProductInfo productInfo)
        {
            base.MController_ProductEvent(productInfo);

            // 如果没有连接上，不触发通讯
            if (!isConnected || !PDCAEnable() || !productInfo.Result.Result) return;
            string url = Path.Combine(URL, "v5/capture/machinedata");

            string wip = "N999999999";
            var cacheItem = _cacheManager.GetItem(productInfo.Barcode);
            if (cacheItem != null && cacheItem is CacheItem cItem)
            {
                bool isExist = cItem.Any(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                if (isExist)
                {
                    var wipItem = cItem.FirstOrDefault(u => u.Alias != null && u.Alias.ToUpper().StartsWith("WIP"));
                    if (wipItem.Value != null)
                    {
                        wip = wipItem.Value.ToString();
                        if (wip == "IsEmptyMode")
                        {
                            wip = "N999999999";
                        }
                    }
                }
            }
            //构建发送数据
            var errorData = new
            {
                unit_sn = wip,
                serials = new { },
                pass = productInfo.Result.Result,
                input_time = productInfo.EnterTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                output_time = productInfo.OutTime.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                },
            };
            string jsonData = JsonTool.ToJson(errorData);
            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });
        }


        /// <summary>
        /// 4.1 主动叫修-呼叫
        /// </summary>
        public void ManualRepairCall()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected /*|| !PDCAEnable()*/) return;
            string url = Path.Combine(URL, "v5/capture/maintmode/1");
            //生成token 字符串
            token = CreateToken();
            //构建发送数据
            var errorData = new
            {
                state_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data =
                    new
                    {
                        session_token = token,
                        status = "Waiting for repair",
                        machinesn = sysConfig.MachineSn,
                    },
            };
            string jsonData = JsonTool.ToJson(errorData);
            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });
        }
        /// <summary>
        /// 4.2 主动叫修-开始
        /// </summary>
        public void ManualRepairStart()
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected /*|| !PDCAEnable()*/) return;
            string url = Path.Combine(URL, "v5/capture/maintmode/1");
            //构建发送数据
            var errorData = new
            {
                state_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                    session_token = token,
                    status = "Repair started",
                },
            };
            string jsonData = JsonTool.ToJson(errorData);
            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });
        }
        /// <summary>
        /// 4.3 主动叫修-结束
        /// </summary>
        public void ManualRepairEnd(ERepiarType repairType, string repairDetail)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected /*|| !PDCAEnable()*/) return;
            string url = Path.Combine(URL, "v5/capture/maintmode/0");
            //构建发送数据
            var errorData = new
            {
                state_time = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                    downtime_cause = repairType == ERepiarType.Planed ? "Planed downtiontime" : "Unplaned downtime",
                    action_performed = "None",
                    addtional_info = repairDetail,
                    session_token = token,
                    ini_file = new ArrayList() { },
                },
            };
            string jsonData = JsonTool.ToJson(errorData);
            CommException(url, jsonData, (r) =>
            {
                if (r.IsSuccess)
                {
                    heartSource?.Cancel();
                    heartSource = new CancellationTokenSource();
                }
            });
        }

        /// <summary>
        /// 5.软件版本监控
        /// 1.运控软件版本
        /// 2.视觉软件版本
        /// 3.机器人软件版本
        /// </summary>
        public override void Register()
        {
            // 如果没有连接上，不触发通讯
            string url = Path.Combine(URL, "iot/timeseries");
            if (!isConnected /*|| !PDCAEnable()*/) return;
            var motionSw = new
            {
                app_id = sysConfig.HiveAppId,
                ts = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                    ms_version = ApiVision,
                    ms_sha1 = Sha1Signature(ApiVision),
                },
            };

            var visionSw = new
            {
                app_id = sysConfig.HiveAppId,
                ts = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                    vs_version = sysConfig.VisionVersion,
                    vs_sha1 = Sha1Signature(sysConfig.VisionVersion),
                }
                ,
            };

            var robotSw = new
            {
                app_id = sysConfig.HiveAppId,
                ts = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800"),
                data = new
                {
                    rs_version = sysConfig.RobotVersion,
                    rs_sha1 = Sha1Signature(sysConfig.RobotVersion),
                },
            };

            string jsonMotionData = JsonTool.ToJson(motionSw);
            string jsonVisionData = JsonTool.ToJson(visionSw);
            string jsonRobotData = JsonTool.ToJson(robotSw);

            List<string> swVer = new List<string>();
            swVer.Add(jsonMotionData);
            swVer.Add(jsonVisionData);
            swVer.Add(jsonRobotData);


            foreach (var item in swVer)
            {
                CommException(url, item, (r) =>
                {
                    if (r.IsSuccess)
                    {
                        heartSource?.Cancel();
                        heartSource = new CancellationTokenSource();
                    }
                });
            }


        }
        protected override void Register(IMotionController motionController)
        {
            base.Register(motionController);

            // 产品出料
            motionController.ProductEvent -= MotionController_ProductEvent;
            motionController.ProductEvent += MotionController_ProductEvent;

            //设备状态变化
            motionController.MachineStatusEvent -= MotionEngine_ProBlockEvent;
            motionController.MachineStatusEvent += MotionEngine_ProBlockEvent;

            //小料触发事件
            motionController.ProductTrowEvent -= MotionControl_ThrowSmallPartEvent;
            motionController.ProductTrowEvent += MotionControl_ThrowSmallPartEvent;

        }

        private void MotionController_ProductEvent(ProductInfo arg1, bool arg2)
        {
            //hive上传cycle time

            //如果主料抛料了，需要上传error
            if (arg2)
            {
                if (arg1.Result.IsToss)
                {
                    AlarmEvent("error", arg1.Result.NgCode, arg1.Result.ErrMsg);
                }
            }
        }

        private void MotionControl_ThrowSmallPartEvent(StationResult stationResult)
        {
            AlarmEvent("tossing", stationResult.NgCode, stationResult.ErrMsg);
        }


        /// <summary>
        /// 6.验证流程
        /// </summary>
        public string SetupQualification()
        {
            string errmsg = "error";
            //1.验证手动维修模式
            //1.1 设备故障
            //Step1:叫修呼叫
            ManualRepairCall();
            Thread.Sleep(500);
            //Step2:叫修开始
            ManualRepairStart();
            Thread.Sleep(500);
            //Step3:叫修完成
            ManualRepairEnd(ERepiarType.Unplaned, "视觉修改抓圆区域");
            Thread.Sleep(500);

            //1.2 预防性维护
            //Step1:叫修呼叫
            ManualRepairCall();
            Thread.Sleep(500);
            //Step2:叫修开始
            ManualRepairStart();
            Thread.Sleep(500);
            //Step3:叫修完成
            ManualRepairEnd(ERepiarType.Planed, "托盘上料");
            Thread.Sleep(500);


            //2.验证自动停机模式
            //2.1 state 1, 5, 2, 1, SN1, SN2, SN3, 2, x
            //Step1:状态进入1：自动运行
            MachineStatusChange("", "", EngineStatus.Idle, EngineStatus.Running);
            Thread.Sleep(1000);
            //Step2:状态进入5：暂停，原因一级供应商
            MachineStatusChange("F99OOOO-01", "Stopped for repair", EngineStatus.Running, EngineStatus.Pause);
            Thread.Sleep(1000);
            //Step3:状态进入2：已经维修好，待料中
            MachineStatusChange("", "", EngineStatus.Pause, EngineStatus.Idle);
            Thread.Sleep(1000);
            //Step4:状态进入1：设备来料
            MachineStatusChange("", "", EngineStatus.Idle, EngineStatus.Running);

            Thread.Sleep(1000);


            //Step5~7:做3次料

            Dictionary<int, string> barcodes = new Dictionary<int, string>();
            barcodes.Add(0, "G6TGUE0002M00001WD");
            barcodes.Add(1, "G6TGUE0002P00001WD");
            barcodes.Add(2, "G6TGUE0002R00001WD");
            for (int i = 0; i < 3; i++)
            {
                ProductInfo product = new ProductInfo();
                StationResult stationResult = new StationResult();
                product.Barcode = barcodes[i];
                product.EnterTime = DateTime.Now;
                Thread.Sleep(1000);
                product.OutTime = DateTime.Now;
                stationResult.Result = true;
                product.Result = stationResult;
                MController_ProductEvent(product);
                Thread.Sleep(500);
            }

            Thread.Sleep(1000);
            //Step8:状态进入2：待料
            MachineStatusChange("", "", EngineStatus.Running, EngineStatus.Idle);

            Thread.Sleep(1000);
            //Step9:状态进入5：暂停，原因一级供应商
            MachineStatusChange("F99OOOO-01", "Stopped for repair", EngineStatus.Running, EngineStatus.Pause);
            Thread.Sleep(1000);

            //2.2 state 1, 5, 1, SN1, SN2, SN3, 2, x
            //Step1:状态进入1：自动运行
            MachineStatusChange("", "", EngineStatus.Idle, EngineStatus.Running);
            Thread.Sleep(1000);
            //Step2:状态进入5：暂停，原因计划维修
            MachineStatusChange("F99OOOO-05", "Planned downtime", EngineStatus.Running, EngineStatus.Pause);
            Thread.Sleep(1000);
            //Step3:状态进入1：设备来料
            MachineStatusChange("", "", EngineStatus.Idle, EngineStatus.Running);
            Thread.Sleep(1000);
            //Step4~6:做3次料
            for (int i = 0; i < 3; i++)
            {
                ProductInfo product = new ProductInfo();
                StationResult stationResult = new StationResult();
                product.Barcode = barcodes[i];
                product.EnterTime = DateTime.Now;
                Thread.Sleep(1000);
                product.OutTime = DateTime.Now;
                product.Result = stationResult;
                MController_ProductEvent(product);
                Thread.Sleep(500);
            }
            Thread.Sleep(1000);
            //Step7:状态进入2：待料
            MachineStatusChange("", "", EngineStatus.Running, EngineStatus.Idle);
            Thread.Sleep(1000);
            //Step8:状态进入5：暂停，原因计划维修
            MachineStatusChange("F99OOOO-05", "Planned downtime", EngineStatus.Running, EngineStatus.Pause);
            Thread.Sleep(1000);

            //3.验证抛主料，抛小料
            //3.1 抛主料
            AlarmEvent("error", "L04SSSP-17-03", "Recheck height1NG");
            Thread.Sleep(1000);
            //3.2 抛小料
            AlarmEvent("tossing", "V03VSDV-04-03", "CCD2 foolproof capture PF NG");
            errmsg = "";
            return errmsg;
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
            if (!isConnected || !PDCAEnable()) return;

            string url = Path.Combine(URL, "v5/capture/maintmode/1");

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
        /// 手动报修流程
        /// </summary>
        public enum ERepairSequence
        {
            [Description("叫修")]
            Call,
            [Description("开始")]
            Start,
            [Description("结束")]
            End
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

    }



}
