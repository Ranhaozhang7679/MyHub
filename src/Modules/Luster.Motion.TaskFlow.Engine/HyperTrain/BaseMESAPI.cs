#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseAPI
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.HyperTrain
* 文 件 名:       BaseAPI.cs
* 创建时间:       2022/10/6 15:13:37
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ef9b1325-9bfb-4e55-a0d7-f6bf0d5375a4
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/6 15:13:37
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Luster.Motion.TaskFlow.Engine
{
    /// <summary>
    /// MES系统的API
    /// </summary>
    public abstract class BaseMESAPI 
    {
        /// <summary>
        /// 系统状态
        /// </summary>
        private TrainRunMode _status;
        public TrainRunMode Status
        {
            get => _status; set
            {
                var src = _status;
                _status = value;
                if (src != value)
                {
                    StatusChangedEvent?.Invoke(src, value);
                }
            }
        }

        /// <summary>
        /// 状态发送变更
        /// </summary>
        public event Action<TrainRunMode, TrainRunMode> StatusChangedEvent;

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                var src = _isEnabled;
                _isEnabled = value;
                if (src != value)
                {
                    IsEnabledEvent?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// 状态发送变更
        /// </summary>
        public event Action<bool> IsEnabledEvent;

        /// <summary>
        /// 控制器
        /// </summary>
        protected IMotionController mController;

        /// <summary>
        /// 系统配置
        /// </summary>
        public SystemConfig sysConfig;

        /// <summary>
        /// 通讯
        /// </summary>
        private WebTool webTool;

        /// <summary>
        /// ftp通讯
        /// </summary>
        public FtpTool ftpTool;

        /// <summary>
        /// 驾驶舱请求地址
        /// </summary>
        protected string URL;

        /// <summary>
        /// 10s上传产品间隔数据
        /// </summary>
        private const int ProInterval = 20000;

        /// <summary>
        /// 用于通过外部取消复位
        /// </summary>
        protected CancellationTokenSource tokenSource;

        /// <summary>
        /// 用于通过外部取消复位
        /// </summary>
        protected CancellationTokenSource heartSource;

        /// <summary>
        /// 是否连接
        /// </summary>
        protected bool isConnected = false;

        /// <summary>
        /// 是否正在连接中
        /// </summary>
        public event Action<object, bool> IsConnectEvent;

        // ip 地址
        protected string serverIp = "";

        // 端口
        protected int port = 0;


        /// <summary>
        /// 隶属系统
        /// </summary>
        /// <returns></returns>
        public abstract string GetSystem();

        /// <summary>
        /// 接口版本号
        /// </summary>
        public string ApiVision = "";

        /// <summary>
        /// 软件版本号
        /// </summary>
        public string AppVersion = "";
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="mController"></param>
        public BaseMESAPI()
        {
        }

        protected virtual void CheckParam(string serverIP, int port)
        {
            if (string.IsNullOrEmpty(serverIP) || port <= 0)
            {
                throw new FriendlyException($"服务器地址和端口不能为空!");
            }

            if (string.IsNullOrEmpty(sysConfig.MachineSn))
            {
                throw new FriendlyException($"设备SN不能为空!");
            }

            if (string.IsNullOrEmpty(sysConfig.StationId))
            {
                throw new FriendlyException($"工站 ID不能为空!");
            }

            if (string.IsNullOrEmpty(sysConfig.StationName))
            {
                throw new FriendlyException($"工站名称不能为空!");
            }

            if (string.IsNullOrEmpty(sysConfig.VendorName))
            {
                throw new FriendlyException($"设备厂商不能为空!");
            }
        }

        private bool isStopPing = false;

        /// <summary>
        /// 开启服务
        /// </summary>
        public void StartService(IMotionController motionController, string ip, int p)
        {
            // 服务已经启动
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
            {
                return;
            }

            serverIp = ip;
            port = p;

            mController = motionController;
            sysConfig = mController.SysConfig;
            AppVersion = sysConfig.SoftVersion;
            ApiVision = sysConfig.ApiVision;
            
            sysConfig.IsEnabledEvent -= SysConfig_IsEnabledEvent;
            sysConfig.IsEnabledEvent += SysConfig_IsEnabledEvent;

            CheckParam(serverIp, port);

            webTool = new WebTool() { Encoding = Encoding.UTF8, Timeout = 3000 };
            URL = $"http://{serverIp}:{port}/";

            ftpTool = FtpTool.GetClient(serverIp, sysConfig.FtpUser, sysConfig.FtpPwd);

            // 如果tokenSource不为空，才停止
            StopService();

            mController.EngineStatusEvent -= MController_EngineStatusEvent;
            mController.EngineStatusEvent += MController_EngineStatusEvent;

            mController.ManualDownEvent -= MController_ManualDownEvent;
            mController.ManualDownEvent += MController_ManualDownEvent;

            mController.AlarmProcEvent -= MController_AlarmProcEvent;
            mController.AlarmProcEvent += MController_AlarmProcEvent;

            mController.ProductEvent -= MController_ProductEvent;
            mController.ProductEvent += MController_ProductEvent;

            mController.MachineStatusEvent -= MController_StatusEvent;
            mController.MachineStatusEvent += MController_StatusEvent;




            tokenSource = new CancellationTokenSource();

            // 主动调用一次
            var pingResult = CheckComm(serverIp);
            if (pingResult)
            {
                // 如果服务器能够连接，但是原来通讯是中断的那么就需要重新注册
                isConnected = true;
                if (GetSystem() == "HyperTrain")
                {
                    Register();
                }
            }

            // 单独开一个线程检查通讯是否正常
            Task.Run(() =>
            {
                while (true)
                {
                    if (tokenSource == null || tokenSource.IsCancellationRequested)
                    {
                        isStopPing = true;
                        break;
                    }

                    var pingResult = CheckComm(serverIp);

                    if (pingResult)
                    {
                        // 如果服务器能够连接，但是原来通讯是中断的那么就需要重新注册
                        if (!isConnected)
                        {
                            isConnected = true;

                            // 由False->True
                            OnIsConntected(isConnected);
                            if (GetSystem() == "HyperTrain")
                            {
                                Register();
                            }
                        }
                    }
                    else
                    {
                        // 发生变化事件 由True->False
                        if (isConnected)
                        {
                            OnIsConntected(false);
                        }

                        isConnected = false;
                        heartSource?.Cancel();
                        Thread.Sleep(500);
                    }

                    if (tokenSource == null || tokenSource.IsCancellationRequested)
                    {
                        isStopPing = true;
                        break;
                    }

                    // 5000 ms 和服务器进行通讯验证下
                    Thread.Sleep(5000);

                    MachineHeartbeatUpload();

                }
            }, tokenSource.Token);

            // 注册事件
            Register(mController);
        }

        private void SysConfig_IsEnabledEvent(string system, bool isEnabled)
        {
            if (GetSystem() == system)
            {
                IsEnabled = isEnabled;
            }
        }

        private void MController_ManualDownEvent(object arg1, DataStruct.Enums.SystemOperation arg2, string arg3)
        {
            ManualDown(arg1, arg2, arg3);
        }

        /// <summary>
        ///报警消除
        /// </summary>
        /// <param name="alarmInfo"></param>
        private void MController_AlarmProcEvent(AlarmInfo alarmInfo, bool isOK = true)
        {
            AlarmProcEvent(alarmInfo, isOK);
        }
        /// <summary>
        /// 报警消除
        /// </summary>
        protected virtual void AlarmProcEvent(AlarmInfo alarmInfo, bool isOK = true)
        {

        }



        /// <summary>
        ///出料信息
        /// </summary>
        /// <param name="alarmInfo"></param>
        private void MController_ProductEvent(ProductInfo productInfo, bool isUnload)
        {
            if (isUnload)
            {
                MController_ProductEvent(productInfo);
            }
        }
        /// <summary>
        /// 出料信息
        /// </summary>
        protected virtual void MController_ProductEvent(ProductInfo productInfo)
        {

        }


        protected virtual void MachineHeartbeatUpload()
        { }



        /// <summary>
        /// 主动暂停
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3">停机原因</param>
        protected virtual void ManualDown(object arg1, DataStruct.Enums.SystemOperation op, string arg3)
        {
            if (op == DataStruct.Enums.SystemOperation.Pause || op == DataStruct.Enums.SystemOperation.Stop)
            {
                Status = TrainRunMode.ManualDown;
            }
        }

        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        private void MController_EngineStatusEvent(object sender, EngineStatus src, EngineStatus dst)
        {
            Task.Run(() =>
            {
                Status = ConvertToMode(dst);
                StatusChanged(sender as IMotionController, src, dst);
            });
        }

        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public virtual void StatusChanged(IMotionController mController, EngineStatus src, EngineStatus dst)
        {

        }

        /// <summary>
        /// 初始化的时候触发
        /// </summary>
        /// <param name="motionController"></param>
        protected virtual void Register(IMotionController motionController)
        {

        }

        /// <summary>
        /// 触发通信连接
        /// </summary>
        /// <param name="isConn"></param>
        protected void OnIsConntected(bool isConn)
        {
            IsConnectEvent?.Invoke(this, isConn);
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public void StopService()
        {
            isConnected = false;
            heartSource?.Cancel();
            if (tokenSource != null)
            {
                tokenSource.Cancel();

                int time = 0;
                // 等待线程终止
                while (true)
                {
                    Thread.Sleep(10);
                    if (isStopPing)
                    {
                        isStopPing = false;
                        break;
                    }

                    // 超时时间
                    time += 10;
                    if (time > 5000)
                    {
                        break;
                    }
                }
                OnIsConntected(false);

                tokenSource = null;

            }
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonData"></param>
        /// <param name="req"></param>
        /// <exception cref="FriendlyException"></exception>
        protected virtual void CommException(string url, string jsonData, Action<ResultStatus> req)
        {
            if (!isConnected) return;

            // 创建连接
            webTool = new WebTool() { Encoding = Encoding.UTF8 };
            var strResult = "";
            try
            {
                strResult = webTool.Post(url, jsonData);
                // 异常
                OnLog($"System:{this.GetType().Name} URL:{url} Send:{jsonData}\r\n Receive:{strResult}", url);
                //当前设备生产指标返回值格式不对，会导致误报，实际上是传成功
                var resStatus = JsonTool.ToObject<ResultStatus>(strResult);
                req.Invoke(resStatus);
            }
            catch (Exception wx)
            {
                var resultStatus = new ResultStatus()
                {
                    Status = "NG"
                };
                req.Invoke(resultStatus);

                //isConnected = false;
                //OnIsConntected(false);

                // 异常
                if (!strResult.Contains("OK"))
                {
                    OnLog($"System:{this.GetType().Name} URL:{url} Send:{jsonData} Communation Exception:{wx.Message}");
                }
            }
        }

        protected virtual void OnLog(string message, string url = "")
        {
            // 异常
            LogTool.Debug(message, GetSystem());
        }

        /// <summary>
        /// 转换到对应模式
        /// </summary>
        /// <param name="eStatus"></param>
        /// <returns></returns>
        protected TrainRunMode ConvertToMode(EngineStatus eStatus)
        {
            switch (eStatus)
            {
                case EngineStatus.Idle:
                case EngineStatus.Ready:
                case EngineStatus.Homing:

                    return TrainRunMode.Idle;

                case EngineStatus.Running:
                    return TrainRunMode.Running;


                case EngineStatus.Stop:
                case EngineStatus.Pause:
                case EngineStatus.Alarm:
                case EngineStatus.Resetting:
                    return TrainRunMode.Down;
                default:
                    throw new KeyNotFoundException(eStatus.GetDescription());
            }
        }


        /// <summary>
        /// 注册
        /// 2、设备注册信息 API 接口
        /// </summary>
        public virtual void Register()
        {
        }


        /// <summary>
        /// 统计报文
        /// 4、设备生产指标 API 接口
        /// </summary>
        public virtual void ProReport(DateTime startTime, DateTime endTime)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            // 心跳URL
            string url = Path.Combine(URL, "machineMonitorController/machineIndicator");

            var mData = mController.GetUploadProductionQuota(startTime, endTime);
            ArrayList arr = new ArrayList();

            var postData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                startTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = endTime.ToString("yyyy-MM-dd HH:mm:ss"),
                inputQty = mData.InputQty,
                outputQty = mData.OutputQty,
                passQty = mData.PassQty,
                failQty = mData.FailQty,
                tossingQty = mData.TossingQty,
                retestQty = mData.RetestQty,
                targetUph = mData.TargetUph,
                uph = mData.TargetUph,
                yield = mData.Yield,
                tossingRate = mData.TossingRate,
                retestRate = mData.RetestRate,
                from = "MACHINE - INDICATOR - API",
                apiVersion = "V1.0.1"
            };

            arr.Add(postData);
            var jsonData = JsonTool.ToJson(arr);

            CommException(url, jsonData, r =>
            {
                if (r.IsSuccess)
                {
                }
            });
        }

        /// <summary>
        /// 产品出站
        /// </summary>
        public void ProUnloaded(bool isOK, double ct)
        {
            // 如果没有连接上，不触发通讯
            if (!isConnected) return;

            // 心跳URL
            string url = Path.Combine(URL, "machineMonitorController/machineParameter");

            ArrayList arr = new ArrayList();

            var postData = new
            {
                machineSn = sysConfig.MachineSn,
                stationId = sysConfig.StationId,
                cycleTime = ct,
                result = isOK ? "OK" : "NG",
                from = "MACHINE - INDICATOR - API",
                apiVersion = "V1.0.1"
            };

            arr.Add(postData);

            var jsonData = JsonTool.ToJson(arr);
            CommException(url, jsonData, r =>
            {

            });
        }

        private System.Net.NetworkInformation.Ping ping = new Ping();

        /// <summary>
        /// 通信连接
        /// </summary>
        /// <returns></returns>
        protected bool CheckComm(string serverIp)
        {
            try
            {
                // 2s 的ping
                PingReply pingResult = ping.Send(serverIp, 2000);

                var result = pingResult.Status == IPStatus.Success;

                return result;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 根据PLC状态变化，触发Vision/Hive状态变化，用于主界面状态显示
        /// </summary>
        /// <param name="motionController"></param>
        /// <param name="dstMode"></param>
        /// <param name="reason"></param>
        private void MController_StatusEvent(IMotionController motionController, TrainRunMode dstMode, string reason)
        {
            Status = dstMode;
        }
    }
}