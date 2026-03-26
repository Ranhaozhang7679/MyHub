using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Luster.SimDevice.Laser.SSZN
{

    /// <summary>
    /// 深视智能线扫相机
    /// </summary>
    public class SsznLineLaser : LineLaserBase, ILineLaser
    {
        [DllImport("kernel32.dll")]
        static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public SsznLineLaser()
        {

        }

        ~SsznLineLaser()
        {
            if (_heightDataPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_heightDataPtr);
            }
        }

        #region 字段 

        /// <summary>
        /// 厂商
        /// </summary>
        public override string Brand => "SSZN";

        /// <summary>
        /// 扫描完成事件
        /// </summary>
        public event Action<LineLaser> ScanFinishEvent;

        /// <summary>
        /// 默认开关量用软触发
        /// </summary>
        private SR7IF_StartTriggerMode _startTriggerMode = SR7IF_StartTriggerMode.Soft;

        /// <summary>
        /// 激光回调句柄
        /// </summary>
        private SR7IF_BatchOneTimeCallBack _batchOneTimeCallBack;

        /// <summary>
        /// 高度值(线扫回调高度值数据指针)
        /// </summary>
        private IntPtr _heightDataPtr = IntPtr.Zero;

        /// <summary>
        /// 当前存储高度值开辟的内存大小
        /// </summary>
        private int _curHeightDataSize = 0;

        /// <summary>
        /// 线扫数据
        /// </summary>
        private LineLaser _laserData;

        /// <summary>
        /// 是否连接
        /// </summary>
        private bool _isConnect = false;

        /// <summary>
        /// 是否注册回调
        /// </summary>
        private bool _isLogoCallBack = false;

        /// <summary>
        /// 当前设备ID端口号
        /// </summary>
        private int _devID = 0;

        /// <summary>
        /// 当前配置文件编号
        /// </summary>
        private int _iCurJobID = 1;

        /// <summary>
        /// 软件版本
        /// </summary>
        private string _ssznDllVersion = string.Empty;

        #endregion

        #region 属性

        private VIO startIO;
        /// <summary>
        /// 开始IO
        /// </summary>
        public VIO StartIO { get => startIO; set => startIO = value; }


        #endregion

        #region 方法

        /// <summary>
        /// 初始化线扫高度值内存
        /// </summary>
        /// <param name="size">内存尺寸</param>
        private void InitHeightIntPtr(int size)
        {
            if (_curHeightDataSize != size || _heightDataPtr == IntPtr.Zero)
            {
                _curHeightDataSize = size;
                if (_heightDataPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_heightDataPtr);
                }
                _heightDataPtr = Marshal.AllocHGlobal(size);
            }
        }

        /// <summary>
        /// 初始化API
        /// </summary>
        public override void InitApi()
        {
            SafeNativeMethod(() =>
            {
                if (HasInit())
                {
                    OnLog(LogType.Info, $"SSZN:端口号{Adapter.GetMethod()} has initialize!");
                    return true;
                }
                if (Adapter != null && ((Adapter as Network) != null))
                {
                    var network = Adapter as Network;
                    _devID = network.Port;
                    _ssznDllVersion = GetVersion();
                    OnLog(LogType.Debug, $"SSZN:端口号{Adapter.GetMethod()}初始化API");
                    return true;
                }
                else
                {
                    return false;
                }
            }, $"SSZN激光:{Adapter.GetMethod()},初始化API错误");
        }

        /// <summary>
        /// 线扫初步打开
        /// </summary>
        public override void Open()
        {
            base.Open();
            //连接
            SafeNativeMethod(() =>
            {
                if (SSZNLaserConnect(Adapter.GetMethod()) != 0)
                {
                    return false;
                }
                LaserStart();
                OnLog(LogType.Debug, $"SSZN:端口号{Adapter.GetMethod()}打开线扫");
                return true;
            }, $"SSZN激光:{Adapter.GetMethod()},打开线扫失败！");
        }

        /// <summary>
        /// 线扫关闭
        /// </summary>
        public override void Close()
        {
            SafeNativeMethod(() =>
            {
                int status = -1;
                base.Close();
                if (_isConnect)
                {
                    status = NativeMethod.SR7IF_StopMeasure(_devID);
                    status = NativeMethod.SR7IF_CommClose(_devID);
                }
                if (status != 0)
                {
                    return false;
                }
                _isLogoCallBack = false;
                _isConnect = false;
                OnStatusChanged(DeviceStatus.Offline);
                OnLog(LogType.Debug, $"SSZN激光:{Adapter.GetMethod()}关闭");
                return true;
            }, $"SSZN激光:{Adapter.GetMethod()},关闭失败！");
        }

        /// <summary>
        /// SSZN线扫对象释放
        /// </summary>
        /// <param name="isDispose">是否释放</param>
        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            if (isDispose)
            {
                this.Close();
                OnLog(LogType.Debug, $"SSZN激光:{Adapter.GetMethod()}对象被释放");
            }
        }

        /// <summary>
        /// 线扫开启等操作
        /// </summary>
        public void LaserStart()
        {
            string strMess = string.Empty;
            SafeNativeMethod(() =>
            {
                if (SSZNLaserConnect(Adapter.GetMethod()) != 0)
                {
                    strMess = "线扫未连接";
                    return false;
                }
                // 设置触发模式 0 软触发 硬件触发 1 , 注意 相机本身模式问题，执行下面函数，线扫会进入回调
                if (_isLogoCallBack == false)
                {
                    if (_startTriggerMode == SR7IF_StartTriggerMode.IO)
                    {
                        //调用一次既可
                        if (NativeMethod.SR7IF_StartMeasureWithCallback(_devID, (int)_startTriggerMode) != 0)
                        {
                            strMess = "触发失败";
                            return false;
                        }
                    }
                    _isLogoCallBack = true;
                }

                OnLog(LogType.Debug, $"SSZN激光:{Adapter.GetMethod()}取流开始");
                return true;
            }, $"SSZN激光:{Adapter.GetMethod()}取流开始设置失败,{strMess}");
        }

        /// <summary>
        /// 获取当前线激光结果
        /// </summary>
        /// <returns></returns>
        public LineLaser GetLaserData()
        {
            return _laserData;
        }

        /// <summary>
        /// 提前读取工控机所连接的线扫数量（SSZN无此接口）
        /// </summary>
        /// <param name="ips"></param>
        public void LaserListRead(out List<IDevice> ips)
        {
            ips = new List<IDevice>();
        }

        /// <summary>
        /// 从本地文件读取配置Job
        /// </summary>
        /// <param name="configPath"></param>
        public void LoadConfig(string configPath)
        {
            //SSZN无需上载步骤
        }

        /// <summary>
        /// 获取激光参数
        /// </summary>
        /// <param name="pamaName">参数名称</param>
        /// <returns></returns>
        public object LaserGetPamas(string pamaName = "")
        {
            object retv = 0;
            if (pamaName == "StartTriggerIO")
            {
                return StartIO;
            }
            if (pamaName != null)
            {
                var para = (LaserPamas)Enum.Parse(typeof(LaserPamas), pamaName);
                switch (para)
                {
                    case LaserPamas.Exposure:
                        {
                            var exp = 0;
                            GetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, out exp);
                            retv = exp;
                        }
                        break;
                    case LaserPamas.Gain:
                        {

                        }
                        break;
                    case LaserPamas.DataMode:
                        {

                        }
                        break;
                    case LaserPamas.DataTriggerMode:
                        {


                        }
                        break;
                    case LaserPamas.StartTriggerMode:
                        {

                        }
                        break;
                    case LaserPamas.RowNum:
                        {

                        }
                        break;
                    case LaserPamas.RowDis:

                        break;
                    case LaserPamas.ColDis:

                        break;
                    case LaserPamas.ColNum:

                        break;
                }
            }
            return retv;
        }

        /// <summary>
        /// 获取相机配置列表（默认15个配置文件）
        /// </summary>
        /// <returns></returns>
        public List<string> GetJobs()
        {
            var sszn = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
            return sszn;
        }

        /// <summary>
        /// 切换配置Job文件
        /// </summary>
        /// <param name="jobName"></param>
        public void SetJob(string strJobName)
        {
            SafeNativeMethod(() =>
            {
                if (int.TryParse(strJobName, out int v))
                {
                    if (_iCurJobID != v)
                    {
                        _iCurJobID = v;
                        //切换设备ID,  程序ID 0~63
                        int status = NativeMethod.SR7IF_SwitchProgram(_devID, _iCurJobID - 1);
                        if (status != 0)
                        {
                            return false;
                        }
                        OnLog(LogType.Debug, $"SSZN激光:{Adapter.GetMethod()}切换Job为{_iCurJobID}");
                    }
                }
                return true;
            }, $"SSZN激光:{Adapter.GetMethod()}切换Job{_iCurJobID}失败");
        }

        /// <summary>
        /// 暂停相机测量等操作
        /// </summary>
        public void LaserStop()
        {
            //关闭测量
            SafeNativeMethod(() =>
            {
                if (NativeMethod.SR7IF_StopMeasure(_devID) != 0)
                {
                    return false;
                }
                OnLog(LogType.Debug, $"SSZN激光:{Adapter.GetMethod()}关闭线扫测量");
                return true;
            }, $"SSZN激光:{Adapter.GetMethod()}关闭线扫测量失败");
        }

        /// <summary>
        /// 软触发
        /// </summary>
        public void SoftTrigger()
        {
            if (_startTriggerMode == SR7IF_StartTriggerMode.Soft)
            {
                //开启软触发
                SafeNativeMethod(() =>
                {
                    if (NativeMethod.SR7IF_StartMeasureWithCallback(_devID, (int)_startTriggerMode) != 0)
                    {
                        return false;
                    }
                    OnLog(LogType.Debug, $"SSZN激光:{Adapter.GetMethod()}软触发");
                    return true;
                }, $"SSZN激光:{Adapter.GetMethod()}软触发失败");
            }
        }

        /// <summary>
        /// 批量设置线扫参数
        /// </summary>
        /// <param name="keyValues">参数表</param>
        public void LaserSetPamas(List<KeyValue> keyValues)
        {
            if (keyValues != null && keyValues.Count > 0 && _isConnect)
            {
                foreach (KeyValue kv in keyValues)
                {
                    var paraName = (LaserPamas)Enum.Parse(typeof(LaserPamas), kv.Key);
                    LaserSetPamas(paraName, kv.Value);
                }
            }
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="pamas">参数类型</param>
        /// <param name="val">值</param>
        public void LaserSetPamas(LaserPamas pamas, object val)
        {
            string mess = string.Empty;
            if (!_isConnect)
            {
                OnLog(LogType.Debug, $"线激光:{Adapter.GetMethod()} 连接失败");
                return;
            }
            SafeNativeMethod(() =>
            {
                int status = -1;
                switch (pamas)
                {
                    case LaserPamas.Exposure:
                        {
                            int ex = (int)double.Parse(val?.ToString());
                            if (ex <= 10)
                            {
                                SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T10US);
                            }
                            else if (ex > 10 && ex <= 15)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T15US);
                            }
                            else if (ex > 15 && ex <= 30)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T30US);
                            }
                            else if (ex > 30 && ex <= 60)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T60US);
                            }
                            else if (ex > 60 && ex <= 120)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T120US);
                            }
                            else if (ex > 120 && ex <= 240)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T240US);
                            }
                            else if (ex > 240 && ex <= 480)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T480US);
                            }
                            else if (ex > 480 && ex <= 960)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T960US);
                            }
                            else if (ex > 960 && ex <= 1920)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T1920US);
                            }
                            else if (ex > 1920 && ex <= 2400)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T2400US);
                            }
                            else if (ex > 2400 && ex <= 4900)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T4900US);
                            }
                            else if (ex > 4900 && ex <= 9800)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T9800US);
                            }
                            else
                            {
                                SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, (int)SR7IF_EXP_TIME.T120US);
                            }
                            mess = $"曝光{ex}";
                        }
                        break;
                    case LaserPamas.Gain:
                        {
                            int gain = int.Parse(val?.ToString());
                            status = 0;
                        }
                        break;
                    case LaserPamas.DataMode:
                        {
                            // 设置所需的图像格式，点云和轮廓(SSZN目前仅使用点云)
                            status = 0;
                        }
                        break;
                    case LaserPamas.DataTriggerMode:
                        {
                            // 设置数据触发模式
                            if ((LaserDataTrigger)val == LaserDataTrigger.IO)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.TRIG_MODE, (int)SR7IF_TRIG_MODE.EXT_TRIGGER);
                            }
                            else if ((LaserDataTrigger)val == LaserDataTrigger.Time)
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.TRIG_MODE, (int)SR7IF_TRIG_MODE.CONTINUE);
                            }
                            else
                            {
                                status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.TRIG_MODE, (int)SR7IF_TRIG_MODE.ENCODER);
                            }
                            mess = $"数据触发方式{val.GetDescription()}";
                        }
                        break;
                    case LaserPamas.StartTriggerMode:
                        {
                            status = 0;
                            if ((LaserStartTrigger)val == LaserStartTrigger.Hardware)
                            {
                                _startTriggerMode = SR7IF_StartTriggerMode.IO;
                                status = NativeMethod.SR7IF_StartMeasureWithCallback(_devID, (int)_startTriggerMode);
                            }
                            else
                            {
                                _startTriggerMode = SR7IF_StartTriggerMode.Soft;
                            }
                            mess = $"开始触发方式{val.GetDescription()}";
                        }
                        break;
                    case LaserPamas.RowNum:
                        {
                            //SSZN 一次点云数据量是50~15000行
                            int count = (int)double.Parse(val?.ToString());
                            if (count <= 50)
                            {
                                count = 50;
                            }
                            else if (count >= 15000)
                            {
                                count = 15000;
                            }
                            status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.BATCH_ON_OFF, (int)SR7IF_BATCH_ON_OFF.ON);
                            status = SetParam(_devID, _iCurJobID, (int)SR7IF_SETTING_ITEM.BATCH_POINT, count);
                            RowNum = count;
                            mess = $"一次扫描行数{count}";
                        }
                        break;
                    case LaserPamas.RowDis:
                        {
                            RowDis = Convert.ToDouble(val);
                            status = 0;
                            mess = $"一次扫描行间距{RowDis}";
                        }
                        break;
                    case LaserPamas.ColDis:
                        {
                            ColDis = Convert.ToDouble(val);
                            status = 0;
                            mess = $"两点间距{ColDis}";
                        }
                        break;
                    case LaserPamas.ColNum:
                        {
                            ColNum = Convert.ToInt32(val);
                            status = 0;
                            mess = $"一行点数量{ColNum}";
                        }
                        break;
                }
                if (status != 0)
                {
                    return false;
                }

                OnLog(LogType.Debug, $"SSZN激光:{Adapter.GetMethod()}参数设置:{mess}");
                return true;

            }, $"SSZN激光:{Adapter.GetMethod()}参数{mess}设置失败，请检查是否连接传感器");

        }

        /// <summary>
        /// 设置非常用参数
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="value">参数值</param>
        public void LaserSetSpPama(string key, object value)
        {
            if (key == "StartTriggerIO")
            {
                StartIO = value as VIO;
            }
        }

        #endregion

        #region 原始方法封装

        /// <summary>
        /// 线扫连接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns></returns>
        private int SSZNLaserConnect(string ip)
        {
            int status = -1;
            if (!_isConnect)
            {
                SR7IF_ETHERNET_CONFIG _ethernetConfig;
                //1、IP创建
                string[] ipTmp = ip.Split('.');
                _ethernetConfig.abyIpAddress = new Byte[]
                {
                    Convert.ToByte(ipTmp[0]),
                    Convert.ToByte(ipTmp[1]),
                    Convert.ToByte(ipTmp[2]),
                    Convert.ToByte(ipTmp[3].Split(':')[0])
                };
                //2、建立连接
                int retcon = NativeMethod.SR7IF_EthernetOpen((int)_devID, ref _ethernetConfig);
                if (retcon != 0)
                {
                    ErrorSaveAndLog("网口连接", retcon, "请检查网线是否连接或者网段是否设置正确");
                    _isConnect = false;
                    return -1;
                }
                _isConnect = true;
                //3、设置默认触发为硬件IO
                _startTriggerMode = SR7IF_StartTriggerMode.IO;
                //默认设置成硬件触发
                _batchOneTimeCallBack = new SR7IF_BatchOneTimeCallBack(CallBack);
                status = NativeMethod.SR7IF_SetBatchOneTimeDataHandler((int)_devID, _batchOneTimeCallBack);
                if (status != 0)
                {
                    ErrorSaveAndLog("绑定批量处理回调", retcon, "请尝试重新连接");
                    _isConnect = false;
                    return -1;
                }
                // 从离线到在线切换
                OnStatusChanged(DeviceStatus.Online);
                bool isBOnline = false;
                if (GetCamBOnline(out isBOnline) == 0)
                {
                    if (isBOnline)
                    {
                        OnLog(LogType.Debug, $"SSZN激光控制器{_devID}上HeaderB在线");
                    }
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 错误解决与记录
        /// </summary>
        /// <param name="status">出现时机</param>
        /// <param name="errID">错误码</param>
        /// <param name="saveWay">解决办法</param>
        private void ErrorSaveAndLog(string status, int errID, string saveWay = "")
        {
            switch (errID)
            {
                case (int)SR7IF_ERROR.SR7IF_ERROR:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_ABORT:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_ABORT.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_ALREADY_EXISTS:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_ALREADY_EXISTS.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_CAMERA_NOT_ONLINE:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_CAMERA_NOT_ONLINE.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_CLOSED:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_CLOSED.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_COMMAND:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_COMMAND.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_DATABUFFER:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_DATABUFFER.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_FRAME_LOSS:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_FRAME_LOSS.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_HANDLE:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_HANDLE.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_MEMORY:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_MEMORY.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_MODE:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_MODE.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_NOT_FOUND:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_NOT_FOUND.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_PARAMETER:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_PARAMETER.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_ROLL_BUSY:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_ROLL_BUSY.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_ROLL_DATA_OVERFLOW:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_ROLL_DATA_OVERFLOW.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_STREAM:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_STREAM.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_TIMEOUT:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_TIMEOUT.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_UNIMPLEMENTED:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_UNIMPLEMENTED.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_ERROR_VERSION:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_ERROR_VERSION.GetDescription()}!{saveWay}!");
                    }
                    break;
                case (int)SR7IF_ERROR.SR7IF_NORMAL_STOP:
                    {
                        OnLog(LogType.Error, $"SSZN激光:{Adapter.GetMethod() + status}时错误,错误码{errID.ToString() + SR7IF_ERROR.SR7IF_NORMAL_STOP.GetDescription()}!{saveWay}!");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// SSZN回调
        /// </summary>
        /// <param name="info">回调头信息</param>
        /// <param name="data">数据信息</param>
        private void CallBack(IntPtr info, IntPtr data)
        {
            SR7IF_STR_CALLBACK_INFO coninfo = new SR7IF_STR_CALLBACK_INFO();
            coninfo = (SR7IF_STR_CALLBACK_INFO)Marshal.PtrToStructure(info, typeof(SR7IF_STR_CALLBACK_INFO));
            if (coninfo.returnStatus != 0)
            {
                if (coninfo.returnStatus != -100)
                {
                    ErrorSaveAndLog("回调函数", coninfo.returnStatus);
                }
                return;
            }
            OnLog(LogType.Debug, "激光扫描完成,准备回调!");
            //行数
            int rowCount = coninfo.BatchPoints;
            //每行点数
            int colCount = coninfo.xPoints;
            //x方向分辨率
            double colDis = coninfo.xPixth;
            //一片点云数据量(点个数)，SSZN返回数据为int  
            int sizeCloudPoint = rowCount * colCount;
            if (sizeCloudPoint <= 0)
            {
                OnLog(LogType.Debug, "点云数量异常,终止回调,请检查设置激光条数等参数是否正确");
                return;
            }
            //获取控制器对应端口_devID数据
            IntPtr tempHeightPtr = NativeMethod.SR7IF_GetBatchProfilePoint(data, _devID);
            //内存拷贝
            if (tempHeightPtr != IntPtr.Zero)
            {
                //开辟内存
                InitHeightIntPtr(sizeCloudPoint * sizeof(int));
                CopyMemory(_heightDataPtr, tempHeightPtr, sizeCloudPoint * sizeof(int));
                //构建对象
                _laserData = new LineLaser()
                {
                    ZPointer = _heightDataPtr,
                    Row = rowCount,
                    RowDis = RowDis,
                    Column = colCount,
                    ColDis = colDis,
                    //SSZN所有系列传感器Z向分辨率均值下值
                    Resolution = 0.00001,
                    ZDataType = DataType.Int,
                };
                ScanFinishEvent?.Invoke(_laserData);
                OnLog(LogType.Debug, "激光扫描完成,完成回调!");
            }
            else
            {
                OnLog(LogType.Debug, "未获取到点云");
            }
        }

        /// <summary>
        /// 获取软件版本号
        /// </summary>
        /// <returns></returns>
        private string GetVersion()
        {
            IntPtr str_Version = NativeMethod.SR7IF_GetVersion();
            string s = Marshal.PtrToStringAnsi(str_Version);
            return s;
        }

        /// <summary>
        /// 设置参数 
        /// </summary>
        /// <param name="iAB">控制器ID（控制器端口号 0 和 1）</param>
        /// <param name="configNum">配置文件ID（job）</param>
        /// <param name="SupportItem">参数项（曝光、激光亮度等参数）</param>
        /// <param name="num">值</param>
        private int SetParam(int iAB, int configNum, int SupportItem, int num)
        {
            int status = -1;
            int depth = 0x02;//1 掉电不保存;2 掉电保存
            int Category = 0x00;//不同页面 0 1 2 
            int Item = 0x01;
            int[] tar = new int[4] { iAB, 0, 0, 0 };

            int DataSize = 0;
            status = TransCategory(SupportItem, out Category, out Item, out DataSize);
            if (status != 0)
            {
                ErrorSaveAndLog("设置参数解析所属类别", status, "请重新设置");
                return -1;
            }

            byte[] pData = null;
            TransNum(num, DataSize, ref pData);
            using (PinnedObject pin = new PinnedObject(pData))
            {
                int errS = NativeMethod.SR7IF_SetSetting((uint)_devID, depth, 15 + configNum,
                    Category, Item, tar, pin.Pointer, DataSize);
                if (errS != 0)
                {
                    ErrorSaveAndLog("设置参数", errS, "请重新设置");
                    return -1;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取线激光参数
        /// </summary>
        /// <param name="iAB">控制器ID（控制器端口号 0 和 1）</param>
        /// <param name="configNum">配置文件ID（job）</param>
        /// <param name="SupportItem">参数项（曝光、激光亮度等参数）</param>
        /// <param name="num">值</param>
        /// <returns></returns>
        private int GetParam(int iAB, int configNum, int SupportItem, out int num)
        {
            num = 0;
            try
            {
                if (SupportItem <= 0x3000)
                {
                    int Category = 0x00;//不同页面 0 1 2 
                    int Item = 0x01;
                    int[] tar = new int[4] { iAB, 0, 0, 0 };
                    int DataSize = 1;
                    int errT = TransCategory(SupportItem, out Category, out Item, out DataSize);
                    if (0 != errT)
                    {
                        ErrorSaveAndLog("获取参数解析所属类别", errT, "请重新尝试");
                        return errT;
                    }
                    byte[] pData = new byte[DataSize];
                    using (PinnedObject pin = new PinnedObject(pData))
                    {
                        int errG = NativeMethod.SR7IF_GetSetting((uint)_devID, 15 + configNum,
                             Category, Item, tar, pin.Pointer, DataSize);

                        if (0 != errG)
                        {
                            ErrorSaveAndLog("执行获取参数函数", errG, "请重新尝试");
                            return errG;
                        }

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < pData.Length; i++)
                        {
                            num += pData[i] * (int)Math.Pow(256, i);
                        }
                    }
                }
                else if (SupportItem <= 0x4000 && SupportItem > 0x3000)
                {
                    switch (SupportItem)
                    {
                        //X数据宽度(单位像素)
                        case (int)SR7IF_SETTING_ITEM.X_PIXEL:
                            int camNum = NativeMethod.SR7IF_GetOnlineCameraB(_devID) == 0 ? 2 : 1;
                            num = NativeMethod.SR7IF_ProfileDataWidth(_devID, new IntPtr()) / camNum;
                            break;
                        default:
                            return -3;
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorSaveAndLog("获取参数", -1, "请重新尝试");
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 获取线激光参数
        /// </summary>
        /// <param name="iAB">控制器ID（控制器端口号 0 和 1）</param>
        /// <param name="configNum">配置文件ID（job）</param>
        /// <param name="SupportItem">参数项（曝光、激光亮度等参数）</param>
        /// <param name="num">值</param>
        /// <returns></returns>
        private int GetParam(int iAB, int configNum, int SupportItem, out double num)
        {
            num = 0.0;
            try
            {
                switch (SupportItem)
                {
                    case (int)SR7IF_SETTING_ITEM.X_PITCH:
                        num = NativeMethod.SR7IF_ProfileData_XPitch(_devID, new IntPtr());
                        break;
                    default:
                        return -2;
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 返回指定端口0 - 7 输入电平，0/False 低 1/True 高
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="level">结果</param>
        /// <returns></returns>
        private int GetIo(uint port, out bool level)
        {
            level = false;
            using (PinnedObject pin = new PinnedObject(level))
            {
                int status = NativeMethod.SR7IF_GetInputPortLevel((uint)_devID, port, pin.Pointer);
                if (status != 0)
                {
                    return -1;
                }
            }
            return 0;
        }

        /// <summary>
        /// 设置端口IO
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="level">结果</param>
        /// <returns></returns>
        private int SetIO(uint port, bool level)
        {
            int status = NativeMethod.SR7IF_SetOutputPortLevel((uint)_devID, port, level);
            if (status != 0)
            {
                return -1;
            }
            return 0;
        }
        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="num"></param>
        /// <param name="DataSize">大小</param>
        /// <param name="byteNum"></param>
        /// <returns></returns>
        private int TransNum(int num, int DataSize, ref byte[] byteNum)
        {
            byteNum = new byte[DataSize];
            for (int i = 0; i < DataSize; i++)
            {
                byteNum[i] = (byte)((num >> (i * 8)) & 0xFF);
            }
            return 0;
        }

        /// <summary>
        /// 追踪所属序列
        /// </summary>
        /// <param name="SupportItem">设置类别</param>
        /// <param name="Category">所属目录</param>
        /// <param name="Item">所属位置</param>
        /// <param name="DataSize">数据尺寸</param>
        /// <returns></returns>
        private int TransCategory(int SupportItem, out int Category, out int Item, out int DataSize)
        {
            Category = SupportItem / 256;
            Item = SupportItem % 256;
            DataSize = 1;

            switch (SupportItem)
            {
                //触发模式
                case (int)SR7IF_SETTING_ITEM.TRIG_MODE:
                    DataSize = 1;
                    break;

                //采样周期
                case (int)SR7IF_SETTING_ITEM.SAMPLED_CYCLE:
                    DataSize = 4;
                    break;

                //批处理开关
                case (int)SR7IF_SETTING_ITEM.BATCH_ON_OFF:
                    DataSize = 1;
                    break;

                //编码器类型
                case (int)SR7IF_SETTING_ITEM.ENCODER_TYPE:
                    DataSize = 1;
                    break;

                //细化点数
                case (int)SR7IF_SETTING_ITEM.REFINING_POINTS:
                    DataSize = 2;
                    break;

                //批处理点数
                case (int)SR7IF_SETTING_ITEM.BATCH_POINT:
                    DataSize = 2;
                    break;

                case (int)SR7IF_SETTING_ITEM.CYCLICAL_PATTERN:
                    DataSize = 1;
                    break;

                case (int)SR7IF_SETTING_ITEM.Z_MEASURING_RANGE:
                    DataSize = 1;
                    break;

                //感光灵敏度
                case (int)SR7IF_SETTING_ITEM.SENSITIVITY:
                    DataSize = 1;
                    break;

                //曝光时间
                case (int)SR7IF_SETTING_ITEM.EXP_TIME:
                    DataSize = 1;
                    break;

                //光亮控制
                case (int)SR7IF_SETTING_ITEM.LIGHT_CONTROL:
                    DataSize = 1;
                    break;

                //激光亮度上限
                case (int)SR7IF_SETTING_ITEM.LIGHT_MAX:
                    DataSize = 1;
                    break;

                //激光亮度下限
                case (int)SR7IF_SETTING_ITEM.LIGHT_MIN:
                    DataSize = 1;
                    break;

                //峰值灵敏度
                case (int)SR7IF_SETTING_ITEM.PEAK_SENSITIVITY:
                    DataSize = 1;
                    break;

                //峰值选择
                case (int)SR7IF_SETTING_ITEM.PEAK_SELECT:
                    DataSize = 1;
                    break;

                //X轴压缩设定
                case (int)SR7IF_SETTING_ITEM.X_SAMPLING:
                    DataSize = 1;
                    break;

                //X轴中位数滤波
                case (int)SR7IF_SETTING_ITEM.FILTER_X_MEDIAN:
                    DataSize = 1;
                    break;

                //Y轴中位数滤波
                case (int)SR7IF_SETTING_ITEM.FILTER_Y_MEDIAN:
                    DataSize = 1;
                    break;

                //X轴平滑滤波
                case (int)SR7IF_SETTING_ITEM.FILTER_X_SMOOTH:
                    DataSize = 1;
                    break;

                //Y轴平滑滤波
                case (int)SR7IF_SETTING_ITEM.FILTER_Y_SMOOTH:
                    DataSize = 2;
                    break;

                //3D/2.5D模式切换
                case (int)SR7IF_SETTING_ITEM.CHANGE_3D_25D:
                    DataSize = 1;
                    break;

                default:
                    Item = 0;
                    DataSize = 1;
                    return -1;
            }
            return 0;
        }
        /// <summary>
        /// 确定同一个控制器的激光头B是否在线
        /// </summary>
        /// <param name="num">激光器数量</param>
        /// <returns></returns>
        private int GetCamBOnline(out bool isOnline)
        {
            int ret = NativeMethod.SR7IF_GetOnlineCameraB(_devID);
            if (ret == -982)
            {
                isOnline = false;
                return 0;
            }
            else if (ret == 0)
            {
                isOnline = true;
                return 0;
            }
            else
            {
                isOnline = false;
                return -1;
            }
        }

        /// <summary>
        /// 读取当前在线相机个数
        /// </summary>
        /// <param name="readaNum"></param>
        /// <returns></returns>
        private SR7IF_ETHERNET_CONFIG GetOnlineCamNum(out int readaNum)
        {
            var ether = NativeMethod.SR7IF_SearchOnline(out readaNum, 1000);
            var ret = (SR7IF_ETHERNET_CONFIG)Marshal.PtrToStructure(ether, typeof(SR7IF_ETHERNET_CONFIG));
            return ret;
        }
        /// <summary>
        /// 获取不同系列线扫有效测量高度（FS的一半）
        /// </summary>
        /// <param name="height">高度结果</param>
        /// <returns>是否成功（0成功/其余失败）</returns>
        private int GetCamHeight(out double height)
        {
            height = 0.0;
            //获取型号判断高度范围
            IntPtr str_Model = NativeMethod.SR7IF_GetModels(_devID);
            String s_model = Marshal.PtrToStringAnsi(str_Model);
            switch (s_model)
            {
                case "SR7050": height = 2.5; break;
                case "SR7080": height = 6.0; break;
                case "SR7140": height = 12.0; break;
                case "SR7240": height = 20.0; break;
                case "SR7400": height = 100.0; break;
                case "SR7300": height = 144.0; break;
                case "SR7900": height = 250.0; break;
                case "SR71600": height = 750.0; break;
                case "SR7060D": height = 3.0; break;
                case "SR8020": height = 2.6; break;
                case "SR8060": height = 9.0; break;
                case "SR6030": height = 5.0; break;
                case "SR6070": height = 13.5; break;
                case "SR6071": height = 35.5; break;
                case "SR6130": height = 70.0; break;
                case "SR6260": height = 145.0; break;
                default:
                    return -2;
            }
            return 0;
        }
        #endregion
    }
}


