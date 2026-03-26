#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       KeyenceLineLaser
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.Laser.KEYENCE
* 文 件 名:       KeyenceLineLaser.cs
* 创建时间:       2022/11/11 15:21:08
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      b18f375c-ed2d-4195-a9a6-c7facfb51eca
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/11 15:21:08
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Laser.KEYENCE
{
    public class KeyenceLineLaser : LineLaserBase, ILineLaser
    {

        [DllImport("kernel32.dll")]
        static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        //构造函数
        public KeyenceLineLaser()
        {

        }

        ~KeyenceLineLaser()
        {
            if (_HeightDataPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_HeightDataPtr);
            }
        }

        #region 字段 
        /// <summary>
        /// 厂商
        /// </summary>
        public override string Brand => "Keyence";

        /// <summary>
        /// 扫描完成事件
        /// </summary>
        public event Action<LineLaser> ScanFinishEvent;

        //激光回调句柄
        private HighSpeedDataCallBack _callBackHandle;

        //高度值
        private IntPtr _HeightDataPtr = IntPtr.Zero;
        //线扫数据
        private LineLaser _laserData;
        //是否连接
        public bool _isConnect = false;

        private bool _isLogoCallBack = false;
        //当前设备ID端口号
        private int _devID = 0;
        //当前配置文件编号
        private int _curJobID = 1;
        //软件版本
        private string _keyenceDllVersion = string.Empty;
        //DLL版本信息
        private LJX8IF_VERSION_INFO versionInfo;
        //激光设置参数
        private LJX8IF_TARGET_SETTING _laserParam = new LJX8IF_TARGET_SETTING();
        //开始触发类别
        private LJX8IF_StartTriggerMode _startTriggerMode;
        //控制器序列号
        private string _controlSN = string.Empty;
        //相机序号号
        private string _camSN = string.Empty;
        //不同系列产品的Z向分辨率
        private double _zResolution = 0;
        //
        private double _zOffsset = 0;

        LJX8IF_ETHERNET_CONFIG _ethernetConfig = new LJX8IF_ETHERNET_CONFIG();
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
        /// 初始化API
        /// </summary>
        public override void InitApi()
        {
            SafeNativeMethod(() =>
            {
                if (HasInit())
                {
                    OnLog(LogType.Info, $"Keyence:端口号{Adapter.GetMethod()} has initialize!");
                    return true;
                }
                if (Adapter != null && ((Adapter as Network) != null))
                {
                    var network = Adapter as Network;
                    if (NativeMethod.LJX8IF_Initialize() != 0 || NativeMethod.LJX8IF_Finalize() != 0)
                    {
                        return false;
                    }
                    versionInfo = NativeMethod.LJX8IF_GetVersion();
                    OnLog(LogType.Debug, $"Keyence:端口号{Adapter.GetMethod()}初始化API");
                    return true;
                }
                else
                {
                    return false;
                }
            }, $"Keyence激光:{Adapter.GetMethod()},初始化API错误");
        }

        /// <summary>
        /// 线扫初步开启（连接）
        /// </summary>
        public override void Open()
        {
            base.Open();
            //连接
            SafeNativeMethod(() =>
            {
                if (KeyenceLaserConnect(Adapter.GetMethod()) != 0)
                {
                    return false;
                }
                Status = DeviceStatus.Online;
                LaserStart();
                OnLog(LogType.Debug, $"Keyence:端口号{Adapter.GetMethod()}打开线扫");
                return true;
            }, $"Keyence激光:{Adapter.GetMethod()},打开线扫失败！");
        }

        /// <summary>
        /// 激光关闭
        /// </summary>
        public override void Close()
        {
            SafeNativeMethod(() =>
            {
                int status = -1;
                base.Close();
                if (_isConnect)
                {
                    //断开连接
                    status = NativeMethod.LJX8IF_CommunicationClose(_devID);
                }
                if (status != 0)
                {
                    return false;
                }
                _isLogoCallBack = false;
                _isConnect = false;
                OnStatusChanged(DeviceStatus.Offline);
                OnLog(LogType.Debug, $"Keyence激光:{Adapter.GetMethod()}关闭");
                return true;
            }, $"Keyence激光:{Adapter.GetMethod()},关闭失败！");
        }

        /// <summary>
        /// 基恩士线扫对象释放（设置离线时需要释放）
        /// </summary>
        /// <param name="isDispose"></param>
        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            if (!HasInit()) return;
            if (isDispose)
            {
                this.Close();
                OnLog(LogType.Debug, $"Keyence激光:{Adapter.GetMethod()}对象被释放");
            }
        }

        /// <summary>
        /// 相机开启取流等操作
        /// </summary>
        public void LaserStart()
        {
            int status = 0;
            string mess = string.Empty;
            SafeNativeMethod(() =>
            {
                if (KeyenceLaserConnect(Adapter.GetMethod()) != 0)
                {
                    mess = ":线扫未连接";
                    return false;
                }
                OnLog(LogType.Debug, $"Keyence激光:{Adapter.GetMethod()}取流");
                return true;
            }, $"Keyence激光:{Adapter.GetMethod()}取流失败,{mess}");
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
        /// 设置激光参数
        /// </summary>
        /// <param name="pamaName"></param>
        /// <param name="val"></param>
        public void LaserSetPamas(string pamaName, object val)
        {
            //此处需要和设备商沟通
        }

        /// <summary>
        /// 获取激光参数
        /// </summary>
        /// <param name="pamaName"></param>
        /// <returns></returns>
        public object LaserGetPamas(string pamaName = "")
        {
            object retv = 0;
            if (pamaName != null)
            {
                var para = (LaserPamas)Enum.Parse(typeof(LaserPamas), pamaName);
                switch (para)
                {
                    case LaserPamas.Exposure:
                        {
                            var exp = 0;
                            //GetParam(_devID, _curJobID, (int)SR7IF_SETTING_ITEM.EXP_TIME, out exp);
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
        /// 获取相机配置列表（默认6个配置文件）
        /// </summary>
        /// <returns></returns>
        public List<string> GetJobs()
        {
            var keyence = new List<string>() { "1", "2", "3", "4", "5", "6" };
            return keyence;
        }

        /// <summary>
        /// 切换配置Job文件
        /// </summary>
        /// <param name="jobName"></param>
        public void SetJob(string jobName)
        {
            SafeNativeMethod(() =>
            {
                if (int.TryParse(jobName, out int v))
                {
                    _curJobID = v;
                    byte id = byte.Parse((_curJobID - 1).ToString());
                    byte progID = 0;
                    NativeMethod.LJX8IF_ChangeActiveProgram(_devID, id);

                }
                OnLog(LogType.Debug, $"Keyence激光:{Adapter.GetMethod()}切换Job");
                return true;
            }, $"Keyence激光:{Adapter.GetMethod()}切换Job失败");
        }

        /// <summary>
        /// 暂时相机取流等操作
        /// </summary>
        public void LaserStop()
        {
            //关闭测量
            SafeNativeMethod(() =>
            {
                //关闭批量测量，关闭通讯
                if (NativeMethod.LJX8IF_StopMeasure(_devID) != 0 && NativeMethod.LJX8IF_CommunicationClose(_devID) != 0)
                {
                    return false;
                }
                OnLog(LogType.Debug, $"Keyence激光:{Adapter.GetMethod()}关闭线扫取流");
                return true;
            }, $"Keyence激光:{Adapter.GetMethod()}关闭线扫取流失败");
        }

        /// <summary>
        /// 软触发
        /// </summary>
        public void SoftTrigger()
        {
            if (_startTriggerMode == LJX8IF_StartTriggerMode.Soft)
            {
                SafeNativeMethod(() =>
                {
                    //开始批量处理
                    if (NativeMethod.LJX8IF_StartMeasure(_devID) != 0)
                    {
                        return false;
                    }
                    OnLog(LogType.Debug, $"Keyence激光:{Adapter.GetMethod()}软触发");
                    return true;

                }, $"Keyence激光:{Adapter.GetMethod()}软触发失败");
            }
        }

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
                                SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T10US);
                            }
                            else if (ex > 10 && ex <= 15)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T15US);
                            }
                            else if (ex > 15 && ex <= 30)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T30US);
                            }
                            else if (ex > 30 && ex <= 60)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T60US);
                            }
                            else if (ex > 60 && ex <= 120)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T120US);
                            }
                            else if (ex > 120 && ex <= 240)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T240US);
                            }
                            else if (ex > 240 && ex <= 480)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T480US);
                            }
                            else if (ex > 480 && ex <= 960)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T960US);
                            }
                            else if (ex > 960 && ex <= 1920)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T1920US);
                            }
                            else if (ex > 1920 && ex <= 2400)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T2400US);
                            }
                            else if (ex > 2400 && ex <= 4900)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T4900US);
                            }
                            else if (ex > 4900 && ex <= 9800)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T9800US);
                            }
                            else
                            {
                                SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.EXP_TIME, (int)LJX8IF_EXP_TIME.T120US);
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
                    case LaserPamas.Frame:
                        {
                            Frame = Convert.ToInt32(val);

                            if (Frame <= 10)
                            {
                                SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F10Hz);
                            }
                            else if (Frame > 10 && Frame <= 20)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F20Hz);
                            }
                            else if (Frame > 20 && Frame <= 50)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F50Hz);
                            }
                            else if (Frame > 50 && Frame <= 100)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F100Hz);
                            }
                            else if (Frame > 100 && Frame <= 200)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F200Hz);
                            }
                            else if (Frame > 200 && Frame <= 500)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F500Hz);
                            }
                            else if (Frame > 1000 && Frame <= 2000)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F2000Hz);
                            }
                            else if (Frame > 2000 && Frame <= 4000)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F4000Hz);
                            }
                            else if (Frame > 4000 && Frame <= 8000)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F8000Hz);
                            }
                            else if (Frame > 8000 && Frame <= 16000)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F16000Hz);
                            }

                            else
                            {
                                SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE, (int)LJX8IF_Frame.F1000Hz);
                            }


                            status = 0;
                            mess = $"采样周期{ColNum}Hz";
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
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.TRIG_MODE, (int)LJX8IF_TRIG_MODE.EXT_TRIGGER);
                            }
                            else if ((LaserDataTrigger)val == LaserDataTrigger.Time)
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.TRIG_MODE, (int)LJX8IF_TRIG_MODE.CONTINUE);
                            }
                            else
                            {
                                status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.TRIG_MODE, (int)LJX8IF_TRIG_MODE.ENCODER);
                            }
                            mess = $"数据触发方式{val.GetDescription()}";
                        }
                        break;
                    case LaserPamas.StartTriggerMode:
                        {
                            status = 0;
                            if ((LaserStartTrigger)val == LaserStartTrigger.Hardware)
                            {
                                _startTriggerMode = LJX8IF_StartTriggerMode.IO;
                                //此处需要测试
                                NativeMethod.LJX8IF_Trigger(_devID);
                            }
                            else
                            {
                                _startTriggerMode = LJX8IF_StartTriggerMode.Soft;
                            }
                            mess = $"开始触发方式{val.GetDescription()}";
                        }
                        break;
                    case LaserPamas.RowNum:
                        {
                            //一次点云数据量是50~60000行
                            int count = (int)double.Parse(val?.ToString());
                            if (count <= 50)
                            {
                                count = 50;
                            }
                            else if (count >= 60000)
                            {
                                count = 60000;
                            }
                            status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.BATCH_ON_OFF, (int)LJX8IF_BATCH_ON_OFF.ON);
                            status = SetParam(_devID, _curJobID, (int)LJX8IF_SETTING_ITEM.BATCH_POINT, count);
                            RowNum = count;
                            InitHighSpeedCommunication();
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

                OnLog(LogType.Debug, $"Keyence激光:{Adapter.GetMethod()}参数设置:{mess}");
                return true;

            }, $"Keyence激光:{Adapter.GetMethod()}参数设置失败");

        }

        // 初始化线扫高度值内存
        private void InitHeightIntPtr(int size)
        {
            if (_HeightDataPtr != IntPtr.Zero)
            {
                //释放内存 
                Marshal.FreeHGlobal(_HeightDataPtr);
            }
            _HeightDataPtr = Marshal.AllocHGlobal(size);
        }

        #endregion

        #region 原始方法封装

        //基恩士线激光连接
        private int KeyenceLaserConnect(string ip)
        {
            int status = 0;
            if (!_isConnect)
            {
                //1、IP创建
                string[] ipTmp = ip.Split('.');
                _ethernetConfig.abyIpAddress = new Byte[]
                {
                    Convert.ToByte(ipTmp[0]),
                    Convert.ToByte(ipTmp[1]),
                    Convert.ToByte(ipTmp[2]),
                    Convert.ToByte(ipTmp[3].Split(':')[0])
                };
                //端口号设置
                _ethernetConfig.wPortNo = ushort.Parse(ipTmp[3].Split(':')[1]);
                //2 建立连接
                if (NativeMethod.LJX8IF_EthernetOpen(_devID, ref _ethernetConfig) == 0)
                {
                    if (status != 0)
                    {
                        _isConnect = false;
                        return -1;
                    }
                    //3、设置回调句柄
                    _callBackHandle = new HighSpeedDataCallBack(CallBack);
                    //4、获取硬件序列号,并计算不同系列产品的Z向分辨率
                    if (GetCamAndHeaderSN(_devID, ref _camSN, ref _controlSN) != 0 || GetZResolution(_camSN) != 0)
                    {
                        _isConnect = false;
                        return -1;
                    }
                    _isConnect = true;
                    OnStatusChanged(DeviceStatus.Online);
                }
                else
                {
                    status = -1;
                }
            }
            return status;
        }

        private void CallBack(IntPtr tempHeightPtr, uint size, uint count, uint notify, uint user)
        {
            //行数
            int rowCount = (int)count;
            if (notify != 0 && notify == 16 || RowNum != count)
            {
                return;
            }
            OnLog(LogType.Debug, "激光扫描完成,准备回调!");
            //每行点数（固定值，不同信号可能有所区别）
            int colCount = 3200;
            //x方向分辨率
            uint pdwXpitch = 0;
            NativeMethod.LJX8IF_GetXpitch(_devID, ref pdwXpitch);
            //一片点云数据量(点个数)，SSZN返回数据为int  
            int sizeCloudPoint = rowCount * colCount;
            if (sizeCloudPoint <= 0)
            {
                OnLog(LogType.Debug, "点云数量异常,终止回调");
                return;
            }
            //获取控制器对应端口_devID数据
            if (tempHeightPtr != IntPtr.Zero)
            {
                //开辟内存
                InitHeightIntPtr(sizeCloudPoint * sizeof(int));
                CopyMemory(_HeightDataPtr, tempHeightPtr, sizeCloudPoint * sizeof(int));
                int[] data = new int[sizeCloudPoint];
                Marshal.Copy(_HeightDataPtr, data, 0, sizeCloudPoint);

                return;
                //构建对象
                _laserData = new LineLaser()
                {
                    ZPointer = _HeightDataPtr,
                    Row = rowCount,
                    RowDis = RowDis,
                    Column = colCount,
                    //X方向分辨率需确认
                    ColDis = pdwXpitch,
                    Resolution = _zResolution,
                    OffsetZ = _zOffsset,
                    //这里需要补充新的
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

        //参数设置方法封装
        /// <summary>
        /// 设置参数 
        /// </summary>
        /// <param name="iAB">控制器ID（控制器端口号 0 和 1）</param>
        /// <param name="configNum">配置文件ID（job）</param>
        /// <param name="SupportItem">参数项（曝光、激光亮度等参数）</param>
        /// <param name="num">值</param>
        private int SetParam(int devID, int configNum, int SupportItem, int val)
        {
            int status = -1;
            //耗时严重
            LJX8IF_SETTING_DEPTH lJX8IF_SETTING_DEPTH = LJX8IF_SETTING_DEPTH.Running;
            int DataSize = 0;
            //解析对应的数据所属类别
            status = TransCategory(configNum + 15, SupportItem, out DataSize);
            if (status != 0)
            {
                return -1;
            }
            byte[] pData = null;
            //将输入值转换
            TransVal(val, DataSize, ref pData);

            using (PinnedObject pin = new PinnedObject(pData))
            {
                uint pdwErr = 0;

                int errS = NativeMethod.LJX8IF_SetSetting(devID, (byte)lJX8IF_SETTING_DEPTH, _laserParam, pin.Pointer, (uint)DataSize, ref pdwErr);
                if (errS != 0)
                {
                    return -1;
                }
            }
            return 0;
        }

        private int TransCategory(int jobID, int SupportItem, out int DataSize)
        {
            int Category = SupportItem / 256;
            int Item = SupportItem % 256;
            DataSize = 1;
            int baseNumber = 10;
            // 数据设置 
            _laserParam.byType = Convert.ToByte(jobID.ToString(), baseNumber);
            _laserParam.byCategory = Convert.ToByte(Category.ToString(), baseNumber);
            _laserParam.byItem = Convert.ToByte(Item.ToString(), baseNumber);
            switch (SupportItem)
            {
                //触发模式
                case (int)LJX8IF_SETTING_ITEM.TRIG_MODE:
                    DataSize = 1;
                    break;

                //采样周期
                case (int)LJX8IF_SETTING_ITEM.SAMPLED_CYCLE:
                    DataSize = 4;
                    break;

                //批处理开关
                case (int)LJX8IF_SETTING_ITEM.BATCH_ON_OFF:
                    DataSize = 1;
                    break;

                //编码器类型
                case (int)LJX8IF_SETTING_ITEM.ENCODER_TYPE:
                    DataSize = 1;
                    break;

                //细化点数
                case (int)LJX8IF_SETTING_ITEM.REFINING_POINTS:
                    DataSize = 2;
                    break;

                //批处理点数
                case (int)LJX8IF_SETTING_ITEM.BATCH_POINT:

                    DataSize = 2;
                    break;

                case (int)LJX8IF_SETTING_ITEM.CYCLICAL_PATTERN:
                    DataSize = 1;
                    break;

                case (int)LJX8IF_SETTING_ITEM.Z_MEASURING_RANGE:
                    DataSize = 1;
                    break;

                //感光灵敏度
                case (int)LJX8IF_SETTING_ITEM.SENSITIVITY:
                    DataSize = 1;
                    break;

                //曝光时间
                case (int)LJX8IF_SETTING_ITEM.EXP_TIME:
                    DataSize = 1;
                    break;

                //光亮控制
                case (int)LJX8IF_SETTING_ITEM.LIGHT_CONTROL:
                    DataSize = 1;
                    break;

                //激光亮度上限
                case (int)LJX8IF_SETTING_ITEM.LIGHT_MAX:
                    DataSize = 1;
                    break;

                //激光亮度下限
                case (int)LJX8IF_SETTING_ITEM.LIGHT_MIN:
                    DataSize = 1;
                    break;

                //峰值灵敏度
                case (int)LJX8IF_SETTING_ITEM.PEAK_SENSITIVITY:
                    DataSize = 1;
                    break;

                //峰值选择
                case (int)LJX8IF_SETTING_ITEM.PEAK_SELECT:
                    DataSize = 1;
                    break;

                //X轴压缩设定
                case (int)LJX8IF_SETTING_ITEM.X_SAMPLING:
                    DataSize = 1;
                    break;

                //X轴中位数滤波
                case (int)LJX8IF_SETTING_ITEM.FILTER_X_MEDIAN:
                    DataSize = 1;
                    break;

                //Y轴中位数滤波
                case (int)LJX8IF_SETTING_ITEM.FILTER_Y_MEDIAN:
                    DataSize = 1;
                    break;

                //X轴平滑滤波
                case (int)LJX8IF_SETTING_ITEM.FILTER_X_SMOOTH:
                    DataSize = 1;
                    break;

                //Y轴平滑滤波
                case (int)LJX8IF_SETTING_ITEM.FILTER_Y_SMOOTH:
                    DataSize = 2;
                    break;

                //3D/2.5D模式切换
                case (int)LJX8IF_SETTING_ITEM.CHANGE_3D_25D:
                    DataSize = 1;
                    break;

                default:
                    Item = 0;
                    DataSize = 1;
                    return -1;
            }
            return 0;
        }

        //转换
        private int TransVal(int num, int DataSize, ref byte[] byteNum)
        {
            byteNum = new byte[4];
            for (int i = 0; i < DataSize; i++)
            {
                byteNum[i] = (byte)((num >> (i * 8)) & 0xFF);
            }
            _laserParam.byTarget1 = byteNum[0];
            _laserParam.byTarget2 = byteNum[1];
            _laserParam.byTarget3 = byteNum[2];
            _laserParam.byTarget4 = byteNum[3];
            return 0;
        }

        //获取相机和控制器序列号
        private int GetCamAndHeaderSN(int _currentDeviceId, ref string camSN, ref string controlSN)
        {
            int status = -1;
            byte[] controllerSerialNumber = new byte[16];
            byte[] headSerialNumber = new byte[16];

            using (PinnedObject pinForControllerSerialNumber = new PinnedObject(controllerSerialNumber))
            using (PinnedObject pinForHeadSerialNumber = new PinnedObject(headSerialNumber))
            {
                status = NativeMethod.LJX8IF_GetSerialNumber(_currentDeviceId, pinForControllerSerialNumber.Pointer, pinForHeadSerialNumber.Pointer);
                camSN = Encoding.ASCII.GetString(headSerialNumber).TrimEnd('\0');
                controlSN = Encoding.ASCII.GetString(controllerSerialNumber).TrimEnd('\0');
            }
            return status;
        }

        //获取设备Z向分辨率(Z向分辨率)
        private int GetZResolution(string camSN)
        {
            //需要确认序列号和型号的关系？？？
            if (camSN == "L-J-X8020")
            {
                _zResolution = 0.0004;
            }
            else if (camSN == "L-J-X8060")
            {
                _zResolution = 0.0008;
            }
            else if (camSN == "L-J-X8080")
            {
                _zResolution = 0.0016;
            }
            else if (camSN == "L-J-X8200")
            {
                _zResolution = 0.004;
            }
            else if (camSN == "L-J-X8400")
            {
                _zResolution = 0.008;
            }
            else if (camSN == "L-J-X8900")
            {
                _zResolution = 0.016;
            }
            else
            {
                //return -1;
            }
            _zOffsset = _zResolution * (-32768);
            return 0;
        }


        private int GetErrorCode(int devID, byte receivedMax = 10)
        {
            byte errCount = 0;
            ushort[] errCode = new ushort[receivedMax];
            using (PinnedObject pin = new PinnedObject(errCode))
            {
                NativeMethod.LJX8IF_GetError(devID, receivedMax, ref errCount, pin.Pointer);

            }
            return 0;
        }

        private int InitHighSpeedCommunication()
        {
            int status = 0;
            //为什么要设置行数
            status = NativeMethod.LJX8IF_FinalizeHighSpeedDataCommunication(_devID);
            status = NativeMethod.LJX8IF_CommunicationClose(_devID);
            NativeMethod.LJX8IF_EthernetOpen(_devID, ref _ethernetConfig);
            ushort hPortNo = (ushort)(_ethernetConfig.wPortNo + (ushort)1);
            status = NativeMethod.LJX8IF_InitializeHighSpeedDataCommunication(_devID, ref _ethernetConfig, hPortNo, _callBackHandle, (uint)RowNum, (uint)_devID);
            LJX8IF_HIGH_SPEED_PRE_START_REQUEST request = new LJX8IF_HIGH_SPEED_PRE_START_REQUEST();
            request.bySendPosition = 0;
            LJX8IF_PROFILE_INFO profileInfo = new LJX8IF_PROFILE_INFO();
            status = NativeMethod.LJX8IF_PreStartHighSpeedDataCommunication(_devID, ref request, ref profileInfo);
            status = NativeMethod.LJX8IF_StartHighSpeedDataCommunication(_devID);
            return status;
        }

        public void LaserSetSpPama(string key, object value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}