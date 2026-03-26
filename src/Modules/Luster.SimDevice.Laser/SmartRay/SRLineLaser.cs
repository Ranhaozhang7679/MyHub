#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SRLineLaser
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.LMILaser.SmartRay
* 文 件 名:       SRLineLaser.cs
* 创建时间:       2022/9/29 15:00:04
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      9ea09a4c-2ae2-4492-8547-77adc96a0970
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/29 15:00:04
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Smartray;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.SimDevice.Laser.SmartRay
{
    public class SRLineLaser : LineLaserBase, ILineLaser
    {
        [DllImport("kernel32.dll")]
        static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public SRLineLaser()
        {

        }

        public SRLineLaser(SRLineLaser sRLineLaser, string name, int multiSensorIndex, string ipAddress, ushort port)
        {
            this.RowNum = sRLineLaser.RowNum;
        }

        ~SRLineLaser()
        {
            if (_HeightDataPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_HeightDataPtr);
            }
        }

        #region 字段

        /// <summary>
        /// 传感器序号
        /// </summary>
        public int SensorIndex = 0;

        /// <summary>
        /// 厂商
        /// </summary>
        public override string Brand => "SmartRay";

        /// <summary>
        /// SmartRay内部传感器对象
        /// </summary>
        public Api.Sensor _sensorObject;

        /// <summary>
        /// 图像数据采集完成事件
        /// </summary>
        public event Action<LineLaser> ScanFinishEvent;

        /// <summary>
        /// 线扫状态 (API callback时更新 )
        /// </summary>
        internal SensorState _sensorState;

        /// <summary>
        /// 开始触发信号(软出发或者硬件触发)
        /// </summary>
        private LaserStartTrigger _laserStartTrigger = LaserStartTrigger.Hardware;

        /// <summary>
        /// 高度值(线扫回调高度值数据指针)
        /// </summary>
        private IntPtr _HeightDataPtr = IntPtr.Zero;

        /// <summary>
        /// 当前存储高度值开辟的内存大小
        /// </summary>
        private int _curHeightDataSize = 0;

        /// <summary>
        /// 传感器系列 如 ECCO55, ECCO75,ECCO95...
        /// </summary>
        private string _sensorSeries = string.Empty;

        /// <summary>
        /// 获得的线扫数据
        /// </summary>
        private LineLaser _laserData;

        /// <summary>
        /// 连接状态
        /// </summary>
        private bool _isConnect = false;

        /// <summary>
        /// 配置JOB文件名称与地址
        /// </summary>
        private Dictionary<string, string> _jobs = new Dictionary<string, string>();

        /// <summary>
        /// 用于存储临时轮廓高度值
        /// </summary>
        private List<ushort> _curZHeightData = new List<ushort>();

        /// <summary>
        /// 当前配置JOB名称
        /// </summary>
        private string _curJobName = string.Empty;

        /// <summary>
        /// 采集总线数
        /// </summary>
        private int _numberOfCapturedProfiles;

        #endregion

        #region 属性

        private VIO startIO;

        /// <summary>
        /// 起始触发ROI
        /// </summary>
        public VIO StartIO { get => startIO; set => startIO = value; }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化高度指针参数
        /// </summary>
        /// <param name="size">高度指针大小</param>
        private void InitHeightIntPtr(int size)
        {
            if (_curHeightDataSize != size || _HeightDataPtr == IntPtr.Zero)
            {
                _curHeightDataSize = size;
                if (_HeightDataPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_HeightDataPtr);
                }
                _HeightDataPtr = Marshal.AllocHGlobal(size);
            }
        }

        /// <summary>
        /// 获取线扫数据
        /// </summary>
        /// <returns></returns>
        public LineLaser GetLaserData()
        {
            return _laserData;
        }

        /// <summary>
        /// 读取电脑硬件接口的线扫（SR不支持）
        /// </summary>
        /// <param name="laserList"></param>
        public void LaserListRead(out List<IDevice> laserList)
        {
            laserList = new List<IDevice>();
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
                    OnLog(LogType.Info, $"SmartRay:端口号{Adapter.GetMethod()} has initialize!");
                    return true;
                }
                // 1.构造Api，需在调用前使用
                NativeMethod.Instance.Init();
                return true;
            });
        }

        /// <summary>
        /// 开启SR线扫
        /// </summary>
        public override void Open()
        {
            //连接相机
            base.Open();
            //连接
            SafeNativeMethod(() =>
            {
                if (SRLaserConnect(Adapter.GetMethod()) != 0)
                {
                    return false;
                }
                LaserStart();
                OnLog(LogType.Debug, $"SmartRay:端口号{Adapter.GetMethod()}打开线扫");
                return true;
            }, $"SmartRay激光:{Adapter.GetMethod()},打开线扫失败！");

            base.Open();
        }

        /// <summary>
        /// 关闭SR线扫
        /// </summary>
        public override void Close()
        {
            base.Close();
            if (!string.IsNullOrEmpty(SN) && _sensorObject != null)
            {
                //关闭相机的数据获取
                Api.StopAcquisition(_sensorObject);
                DeinitApi();
                _isConnect = false;
                OnStatusChanged(DeviceStatus.Offline);
                if (_sensorObject != null)
                {
                    _sensorObject.Dispose();
                }
            }
        }

        /// <summary>
        /// SR线扫对象释放（设置离线时需要释放）
        /// </summary>
        /// <param name="isDispose"></param>
        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            if (!HasInit()) return;
            if (isDispose)
            {
                this.Close();
                OnLog(LogType.Debug, $"SmartRay激光:{Adapter.GetMethod()}对象被释放");
            }
        }

        /// <summary>
        /// 启用禁用设备
        /// </summary>
        /// <param name="enabled">启用或禁用</param>
        public override void SetEnabled(bool enabled)
        {
            if (enabled)
            {

            }
            else
            {

            }
        }

        /// <summary>
        /// 获取配置文件列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetJobs()
        {
            List<string> srjob = new List<string>();
            LoadConfig(CurJobID);
            if (_jobs != null && _jobs.Count > 0)
            {
                srjob = _jobs.Keys.ToList();
            }
            return srjob;
        }

        /// <summary>
        /// 切换相机配置JOB
        /// </summary>
        /// <param name="jobName"></param>
        public void SetJob(string jobName)
        {
            SafeNativeMethod(() =>
            {
                if (!string.IsNullOrEmpty(jobName) && _sensorObject != null)
                {
                    if (_curJobName != jobName)
                    {
                        string jobPath = "";
                        if (_jobs != null && _jobs.Count > 0)
                        {
                            if (_jobs.ContainsKey(jobName))
                            {
                                _jobs.TryGetValue(jobName, out jobPath);
                            }
                            if (Api.LoadParameterSetFromFile(_sensorObject, jobPath) != 0)
                            {
                                return false;
                            }
                            else
                            {
                                _curJobName = jobName;
                            }
                        }
                    }
                }
                return true;
            }, $"SmartRay线激光:{Adapter.GetMethod()} 切换配置Job{jobName}失败");
        }

        /// <summary>
        /// 设置线扫相机参数
        /// </summary>
        /// <param name="laserPamas">线扫相机参数</param>
        /// <param name="val">参数值</param>
        public void LaserSetPamas(LaserPamas laserPamas, object val)
        {
            string mess = string.Empty;
            if (!_isConnect)
            {
                OnLog(LogType.Debug, $"SmartRay线激光:{Adapter.GetMethod()} 设置参数时发现连接失败");
                return;
            }
            SafeNativeMethod(() =>
            {
                int status = -1;
                if (_sensorObject != null)
                {
                    switch (laserPamas)
                    {
                        case LaserPamas.Exposure:
                            {
                                int ex = (int)double.Parse(val?.ToString());
                                status = Api.SetNumberOfExposureTimes(_sensorObject, ex);
                                mess = "曝光";
                            }
                            break;
                        case LaserPamas.Gain:
                            {
                                int gain = int.Parse(val?.ToString());
                                status = Api.SetGain(_sensorObject, false, gain);
                                mess = "增益";
                            }
                            break;
                        case LaserPamas.DataTriggerMode:
                            {
                                // 设置数据触发模式
                                if ((LaserDataTrigger)val == LaserDataTrigger.IO)
                                {
                                    status = Api.SetDataTriggerMode(_sensorObject, Api.DataTriggerMode.External);
                                }
                                else if ((LaserDataTrigger)val == LaserDataTrigger.Time)
                                {
                                    status = Api.SetDataTriggerMode(_sensorObject, Api.DataTriggerMode.FreeRunning);
                                }
                                else
                                {
                                    status = Api.SetDataTriggerMode(_sensorObject, Api.DataTriggerMode.Internal);
                                }
                                mess = $"数据触发方式{val.GetDescription()}";
                            }
                            break;
                        case LaserPamas.StartTriggerMode:
                            {
                                if ((LaserStartTrigger)val == LaserStartTrigger.Hardware)
                                {
                                    //设置触发模式，默认上升延和下降延都有效（统一连接Input0）
                                    Api.StartTriggerSource startTriggerSource = Api.StartTriggerSource.None;
                                    bool startEnable = false;
                                    Api.TriggerEdgeMode edgeMode = Api.TriggerEdgeMode.Both;
                                    Api.GetStartTrigger(_sensorObject, out startTriggerSource, out startEnable, out edgeMode);
                                    if (startTriggerSource == Api.StartTriggerSource.None)
                                    {
                                        //默认设置为Input0
                                        startTriggerSource = Api.StartTriggerSource.Input0;
                                    }
                                    status = Api.SetStartTrigger(_sensorObject, Api.StartTriggerSource.Input0, true, Api.TriggerEdgeMode.Both);
                                    _laserStartTrigger = LaserStartTrigger.Hardware;
                                }
                                else
                                {
                                    status = Api.SetStartTrigger(_sensorObject, Api.StartTriggerSource.None, true, Api.TriggerEdgeMode.Both);
                                    _laserStartTrigger = LaserStartTrigger.SoftWare;
                                }
                                mess = $"开始触发方式{val.GetDescription()}";
                            }
                            break;
                        case LaserPamas.RowNum:
                            {
                                RowNum = int.Parse(val?.ToString());
                                status = Api.SetNumberOfProfilesToCapture(_sensorObject, (uint)RowNum);
                                mess = $"一次扫描行数{RowNum}";
                                Api.ImageAcquisitionType imageAcquisitionType = Api.ImageAcquisitionType.ZMapIntensityLaserLineThickness;
                                status = Api.SetImageAcquisitionType(_sensorObject, imageAcquisitionType);
                                status = Api.SetTransportResolution(_sensorObject, (float)0.1);
                            }
                            break;
                        case LaserPamas.RowDis:
                            {
                                RowDis = Convert.ToDouble(val);
                                status = 0;
                                mess = $"一次扫描行间距{RowDis}";
                            }
                            break;
                    }
                    //将设置信息传给线扫
                    status = Api.SendParameterSetToSensor(_sensorObject);
                }
                if (status != 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }, $"SmartRay激光:{Adapter.GetMethod()}参数{mess}设置失败");


        }

        /// <summary>
        /// 获取线扫相机参数
        /// </summary>
        /// <param name="pamaName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object LaserGetPamas(string pamaName)
        {
            object retv = 0;
            if (pamaName != null && _sensorObject != null)
            {
                var para = (LaserPamas)Enum.Parse(typeof(LaserPamas), pamaName);
                switch (para)
                {
                    case LaserPamas.Exposure:
                        {
                            var exp = 0;
                            Api.GetExposureTime(_sensorObject, 0, out exp);
                            retv = exp;
                        }
                        break;
                    case LaserPamas.Frame:
                        {
                            var fra = 0;
                            //获取触发模式
                            Api.DataTriggerMode dataMode = Api.DataTriggerMode.FreeRunning;
                            Api.GetDataTriggerMode(_sensorObject, out dataMode);
                            if (dataMode == Api.DataTriggerMode.External)
                            {
                                Api.GetExternalTriggerRate(_sensorObject, out fra);
                            }
                            else
                            {
                                Api.GetDataTriggerInternalFrequency(_sensorObject, out fra);
                            }
                            retv = fra;
                        }
                        break;
                    case LaserPamas.Gain:
                        {
                            var gain = 0;
                            bool enable = false;
                            Api.GetGain(_sensorObject, out enable, out gain);
                            retv = gain;
                        }
                        break;
                    case LaserPamas.DataMode:
                        {
                            //点云模式或者是轮廓模式
                        }
                        break;
                    case LaserPamas.DataTriggerMode:
                        {
                            Api.DataTriggerMode dataMode = Api.DataTriggerMode.FreeRunning;
                            Api.GetDataTriggerMode(_sensorObject, out dataMode);
                            if (dataMode == Api.DataTriggerMode.FreeRunning)
                            {
                                retv = LaserDataTrigger.Time;
                            }
                            else if (dataMode == Api.DataTriggerMode.External)
                            {
                                Api.DataTriggerSource dataTriggerSource = Api.DataTriggerSource.Input1;
                                Api.GetDataTriggerExternalTriggerSource(_sensorObject, out dataTriggerSource);
                                if (dataTriggerSource == Api.DataTriggerSource.Input1 || dataTriggerSource == Api.DataTriggerSource.Input2)
                                {
                                    retv = LaserDataTrigger.IO;
                                }
                                else
                                {
                                    retv = LaserDataTrigger.Encode;
                                }
                            }
                        }
                        break;
                    case LaserPamas.StartTriggerMode:
                        {
                            bool startEnable = false;
                            Api.StartTriggerSource startTriggerSource = Api.StartTriggerSource.None;
                            Api.TriggerEdgeMode edgeMode = Api.TriggerEdgeMode.Both;
                            Api.GetStartTrigger(_sensorObject, out startTriggerSource, out startEnable, out edgeMode);
                            if (startTriggerSource == Api.StartTriggerSource.None)
                            {
                                retv = LaserStartTrigger.SoftWare;
                            }
                            else
                            {
                                retv = LaserStartTrigger.Hardware;
                            }
                        }
                        break;
                    case LaserPamas.RowNum:
                        {

                        }
                        break;
                    case LaserPamas.RowDis:
                        {

                        }
                        break;
                    case LaserPamas.ColDis:
                        {

                        }
                        break;
                    case LaserPamas.ColNum:
                        {

                        }
                        break;
                }
            }
            return retv;
        }

        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <param name="configPath">配置文件地址</param>
        public void LoadConfig(string configPath)
        {
            //遍历一个文件夹内所有后缀文件名称
            if (Directory.Exists(configPath))
            {
                _jobs.Clear();
                CurJobID = configPath;
                foreach (var item in Directory.GetFiles(configPath))
                {
                    var ext = Path.GetExtension(item);
                    if (ext == ".par")
                    {
                        //设置配置文件地址
                        var jobName = Path.GetFileName(item);
                        _jobs.Add(jobName, item);
                    }
                }
            }
        }

        /// <summary>
        /// 相机起始动作
        /// </summary>
        public void LaserStart()
        {
            //SmartRay没有取流操作 
        }

        public void LaserStop()
        {
            SafeNativeMethod(() =>
            {
                if (Api.StopAcquisition(_sensorObject) != 0)
                {
                    return false;
                }
                OnLog(LogType.Debug, $"SmartRay激光:{Adapter.GetMethod()}关闭线扫取流");
                return true;
            }, $"SmartRay激光:{Adapter.GetMethod()}关闭线扫取流失败");
        }

        /// <summary>
        /// 软触发
        /// </summary>
        public void SoftTrigger()
        {
            if (_laserStartTrigger == LaserStartTrigger.SoftWare && _isConnect)
            {
                SafeNativeMethod(() =>
                {
                    Api.StopAcquisition(_sensorObject);
                    int ret = Api.StartAcquisition(_sensorObject);
                    if (ret == 0)
                    {
                        OnLog(LogType.Debug, $"SmartRay激光:{Adapter.GetMethod()}软触发");
                        return true;
                    }
                    else
                    {
                        HandleErrorCode("软触发", ret);
                        return false;
                    }
                }, $"SmartRay激光:{Adapter.GetMethod()}软触发失败");
            }
            else
            {
                OnLog(LogType.Debug, $"SmartRay激光:{Adapter.GetMethod()}相机未连接或者触发模式设置为硬件触发，导致软触发未执行");
            }
        }

        /// <summary>
        /// 线扫设置参数
        /// </summary>
        /// <param name="keyValues"></param>
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
        /// 设置非常用参数
        /// </summary>
        /// <param name="key">名称</param>
        /// <param name="value">参数值</param>
        public void LaserSetSpPama(string key, object value)
        {

        }

        /// <summary>
        /// 错误码解析 
        /// </summary>
        /// <param name="status">当前状态</param>
        /// <param name="errID">错误码</param>
        /// <param name="saveWay">解决办法</param>
        /// <returns></returns>
        internal void HandleErrorCode(string status, int errID, string saveWay = "")
        {
            //API错误获取与初步处理
            string apiErrorString;
            //需添加解决办法

            Api.GetErrorMsg(errID, out apiErrorString);
            string errorText = errID.ToString() + apiErrorString;
            ErrorSaveAndLog(status, errorText, saveWay);
        }

        //错误记录
        internal void ErrorSaveAndLog(string status, string errInfo, string saveWay)
        {
            OnLog(LogType.Error, $"SmartRay激光:{Adapter.GetMethod() + status}时错误,错误码{errInfo }!{saveWay}!");
        }
        #endregion

        #region 原始方法封装

        /// <summary>
        /// 创建并连接线扫激光
        /// </summary>
        /// <param name="network">IP</param>
        /// <returns></returns>
        public int SRLaserConnect(string network = "")
        {
            int status = -1;
            if (!_isConnect)
            {
                string ip = String.Empty;
                ushort port = 40;
                if (!string.IsNullOrEmpty(network))
                {
                    string[] parts = network.Split(':');
                    ip = parts[0];
                    port = ushort.Parse(parts[1]);
                }
                else
                {
                    return status;
                }
                if (_sensorObject == null)
                {
                    //1、相机对象创建
                    int num = NativeMethod.GetSensorCount();
                    _sensorObject = NativeMethod.Instance.CreateSensor(this, "_sensor", num, ip, port);
                }
                if (_sensorObject != null)
                {
                    //2、相机连接与载入对应配置文件,连接超时认为相机不存在               
                    if (Api.ConnectSensor(_sensorObject, 30) == 0)
                    {
                        GetSensorSeries();
                        //开启激光
                        Api.SetLaserPower(_sensorObject, true);
                        //激光模式默认设置成持续模式
                        Api.SetLaserMode(_sensorObject, Api.LaserMode.ContinousMode);
                        //上载标定文件
                        Api.LoadCalibrationDataFromSensor(_sensorObject);
                        //上载默认配置Job文件
                        LoadParameterSet(ParameterSet.Snapshot3d);
                        //设置默认图像格式（目前仅使用Zmap高度图）
                        Api.SetImageAcquisitionType(_sensorObject, Api.ImageAcquisitionType.ZMap);
                        //设置数据包大小（0表示自动匹配数据包大小）
                        Api.SetPacketSize(_sensorObject, 0);
                        //设置数据包超时时间(ms)
                        if (_sensorSeries == "ECCO95")
                        {
                            Api.SetPacketTimeOut(_sensorObject, 500);
                        }
                        OnStatusChanged(DeviceStatus.Online);
                        _isConnect = true;
                        return 0;
                    }
                    else
                    {
                        OnLog(LogType.Error, $"SmartRay线扫{Adapter.GetMethod()}连接失败");
                        Disconnect();
                        //是否连接
                        _isConnect = false;
                        return status;
                    }
                }
                else
                {
                    OnLog(LogType.Error, $"SmartRay线扫{Adapter.GetMethod()}连接时创建线扫对象失败");
                    return status;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// API退出
        /// </summary>
        void DeinitApi()
        {
            int ret = Api.Exit();
        }

        /// <summary>
        /// 数据处理
        /// </summary>
        /// <param name="sensor">相机句柄</param>
        /// <param name="imageType">图像数据类型</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="verticalRes"></param>
        /// <param name="horizontalRes"></param>
        /// <param name="zMap"></param>
        /// <returns></returns>
        public int DataInvoke(Api.Sensor sensor, Api.ImageDataType imageType, int height, int width, float verticalRes, float horizontalRes, ushort[] zMap)
        {
            _numberOfCapturedProfiles = _numberOfCapturedProfiles + height;
            if (_numberOfCapturedProfiles < RowNum)
            {
                //数据做拼接
                _curZHeightData.AddRange(zMap);
            }
            else
            {
                OnLog(LogType.Debug, "激光扫描完成,准备开始回调!");
                _curZHeightData.AddRange(zMap);
                int size = RowNum * width;
                //准备内存空间
                InitHeightIntPtr(size * sizeof(short));
                int ret = Api.GetTransportResolution(sensor, out float xTrans);
                if (RowDis == 0)
                {
                    RowDis = xTrans;
                }
                //数据类型转换
                short[] temp = Array.ConvertAll<ushort, short>(_curZHeightData.ToArray(), value => (short)value);
                Marshal.Copy(temp, 0, _HeightDataPtr, size);
                //构建对象                                     
                _laserData = new LineLaser()
                {
                    ZPointer = _HeightDataPtr,
                    Row = RowNum,
                    RowDis = RowDis,
                    Column = width,
                    ColDis = horizontalRes,
                    Resolution = verticalRes,
                    ZDataType = DataType.Short,
                };
                ScanFinishEvent?.Invoke(_laserData);
                //清空数据
                _curZHeightData.Clear();
                _numberOfCapturedProfiles = 0;
                OnLog(LogType.Debug, "激光扫描完成,完成回调!");
            }
            return 0;
        }

        //获取线扫相机序列
        public void GetSensorSeries()
        {
            string modelName;
            string partNumber;

            int ret = Api.GetSensorModelName(_sensorObject, out modelName, out partNumber);
            if (ret != 0)
            {
                HandleErrorCode("获取线扫序列号", ret);
            }
            _sensorSeries = string.Empty;
            for (int i = 0; i < modelName.Length; i++)
            {
                if (modelName[i] == '.')
                    break;

                if (modelName[i] == ' ')
                    continue;

                if (_sensorSeries.Length == 4 && _sensorSeries.Substring(0, 2) == "SR")
                {
                    _sensorSeries += "00";
                    break;
                }
                _sensorSeries += modelName[i];
            }
        }

        /// <summary>
        /// 断开线扫连接
        /// </summary>
        public int Disconnect()
        {
            return Api.DisconnectSensor(_sensorObject);
        }

        //配置图像数据类型和轮廓数据
        public void ConfigureImageAquisition(Api.Sensor sensor, Api.ImageAcquisitionType imageType, uint numberOfProfiles = 100)
        {
            int ret = Api.SetImageAcquisitionType(sensor, imageType);
            ret = Api.SetNumberOfProfilesToCapture(sensor, numberOfProfiles);
            ret = Api.SetPacketSize(sensor, 0);
        }

        public void LoadParameterSet(ParameterSet parameterSet)
        {
            string smartrayInstallationDirectory = Environment.GetEnvironmentVariable("Smartray");
            string parameterSetBaseDirectory = smartrayInstallationDirectory;
            parameterSetBaseDirectory += "\\SR_API\\sr_parameter_sets\\";
            string parameterSetPath = parameterSetBaseDirectory;
            switch (parameterSet)
            {
                case ParameterSet.LiveImage:
                    parameterSetPath += "Pars_" + _sensorSeries + "\\" + _sensorSeries + "_Liveimage.par";
                    break;
                case ParameterSet.Snapshot3d:
                    parameterSetPath += "Pars_" + _sensorSeries + "\\" + _sensorSeries + "_3D_Snapshot.par";
                    break;
                case ParameterSet.Snapshot3dRepeat:
                    parameterSetPath += "Pars_" + _sensorSeries + "\\" + _sensorSeries + "_3D_Repeat_Snapshot.par";
                    break;
                default:
                    break;
            }
            int ret = Api.LoadParameterSetFromFile(_sensorObject, parameterSetPath);

        }

        #endregion

    }
}