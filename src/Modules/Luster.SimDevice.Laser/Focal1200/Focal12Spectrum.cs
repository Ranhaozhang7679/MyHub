#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Focal12Spectrum
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.Laser.Focal1200
* 文 件 名:       Focal12Spectrum.cs
* 创建时间:       2022/10/18 14:07:48
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      b97728da-440e-4f42-a94c-0319127a5171
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/18 14:07:48
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using FocalSpec.FsApiNet.Model;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.SimDevice.Laser.Focal1200
{
    public class Focal12Spectrum : LineLaserBase, ILineLaser
    {
        [DllImport("kernel32.dll")]
        static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public Focal12Spectrum()
        {

        }

        ~Focal12Spectrum()
        {
            if (linePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(linePtr);
            }
            if (_cloudDatas != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_cloudDatas);
            }
        }

        #region 字段
        /// <summary>
        /// 厂商
        /// </summary>
        public override string Brand => "Focal1200";

        public VIO StartIO { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// 扫描完成事件
        /// </summary>
        public event Action<LineLaser> ScanFinishEvent;
        //线光谱对象
        private FsApi _sensorF12;
        //一条线数据高度值指针
        private IntPtr linePtr = IntPtr.Zero;
        //多条线数据点云指针
        private IntPtr _cloudDatas = IntPtr.Zero;
        //光谱数据对象
        private LineLaser _laserData;
        //光谱对象个数
        private int spectrumCount = 0;
        //光谱SN序列
        private List<string> spectrumSNs;
        //连接标志
        private bool _isConnect = false;
        //单条光谱线数据（字节）长度
        private int _lengthData = 0;
        //当前配置文件()
        private string _curJobFile = string.Empty;
        //配置文件名称集合
        private List<string> _jobNames = new List<string>();

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
                    OnLog(LogType.Info, $"Focal1200:{SN} has initialize!");
                    return true;
                }
                //检查配置job文件
                LoadConfig(string.Empty);
                OnLog(LogType.Debug, $"Focal1200:{SN}初始化API");
                return true;
            });
        }

        /// <summary>
        /// 打开光谱(连接等)
        /// </summary>
        public override void Open()
        {
            SafeNativeMethod(() => {
                base.Open();
                if (Focal12Connect() != 0)
                {
                    return false;
                }
                OnLog(LogType.Debug, $"Focal1200:{SN}打开光谱");
                return true;
            },$"Focal1200:{SN},连接失败！");
           
        }
        /// <summary>
        /// 关闭（关闭取流并关闭光谱）
        /// </summary>
        public override void Close()
        {
            SafeNativeMethod(() => {
                base.Close();
                if (!string.IsNullOrEmpty(SN) && _sensorF12 != null)
                {
                    StopGrabbing();
                    if (_sensorF12.Close()!=0)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                OnLog(LogType.Debug, $"Focal1200:{SN}关闭成功");
                return true;
            }, $"Focal1200:{SN},关闭失败！");
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
        /// 光谱参数设置
        /// </summary>
        /// <param name="pamas">参数名称</param>
        /// <param name="val">值</param>
        public void LaserSetPamas(LaserPamas pamas, object val)
        {
            string mess= string.Empty;
            SafeNativeMethod(() => {
                if (_isConnect == true)
                {
                    int status = -1;
                    switch (pamas)
                    {
                        case LaserPamas.Exposure:
                            {
                                float pw = (float)double.Parse(val?.ToString());
                                if (pw <= 1)
                                {
                                    pw = 1;
                                }
                                //LED脉冲宽度,可以控制曝光值，通常设置在1us`100us 在低光泽度表面也可能更高  （注意该值设置为0是就是硬件触发）
                                status = (int)_sensorF12.SetParameter(SN, SensorParameter.PulseWidthFloat, pw);
                                mess = "曝光";
                            }
                            break;
                        case LaserPamas.Gain:
                            {
                                int gain = int.Parse(val?.ToString());
                                status = (int)_sensorF12.SetParameter(SN, SensorParameter.Gain, gain);
                                mess = "增益";
                            }
                            break;
                        case LaserPamas.DataMode:
                            {
                                // 设置所需的图像格式，点云和轮廓
                            }
                            break;
                        case LaserPamas.DataTriggerMode:
                            {
                                // 设置数据触发模式
                                if ((LaserDataTrigger)val == LaserDataTrigger.Time)
                                {
                                    status = (int)_sensorF12.SetParameter(SN, SensorParameter.PulseWidthFloat, (float)100);
                                    mess = "数据触发：自由时间";
                                }
                                else
                                {
                                    //PulseWidthFloat设置为0，就是硬件触发模式
                                    status =(int) _sensorF12.SetParameter(SN, SensorParameter.PulseWidthFloat, (float)0);
                                    mess = "数据触发：编码器";
                                }
                            }
                            break;
                        case LaserPamas.StartTriggerMode:
                            {
                                if ((LaserStartTrigger)val == LaserStartTrigger.Hardware)
                                {
                                    //PulseWidthFloat设置为0，就是硬件触发模式
                                    status =(int) _sensorF12.SetParameter(SN, SensorParameter.PulseWidthFloat, (float)0);
                                    mess = "开启触发：外部硬件";
                                }
                                else
                                {
                                    //PulseWidthFloat设置大于0，就是内部触发模式
                                    status = (int)_sensorF12.SetParameter(SN, SensorParameter.PulseWidthFloat, (float)100);
                                    mess = "开启触发：内部软件";
                                }
                            }
                            break;
                        case LaserPamas.RowNum:
                            {
                                //需要的点云行数
                                int count = (int)double.Parse(val?.ToString());
                                if (count>=0)
                                {
                                    RowNum = count;
                                    status = 0;
                                }
                                else
                                {
                                    status = -1;
                                    mess = "一次扫描行数";
                                }
                            }
                            break;
                        case LaserPamas.RowDis:
                            {
                                double rdis = Convert.ToDouble(val);
                                if (rdis >= 0)
                                {
                                    RowDis = rdis;
                                    status = 0;
                                }
                                else
                                {
                                    status = -1;
                                    mess = "一次扫描行间距";
                                }
                            }
                            break;
                        case LaserPamas.ColDis:
                            {
                                double cdis = Convert.ToDouble(val);
                                if (cdis >= 0)
                                {
                                    ColDis = cdis;
                                    status = 0;
                                }
                                else
                                {
                                    status = -1;
                                    mess = "两点间距";
                                }
                            }
                            break;
                        case LaserPamas.ColNum:
                            {
                                int col = Convert.ToInt32(val);
                                if (col >= 0)
                                {
                                    ColNum = col;
                                    status = 0;
                                }
                                else
                                {
                                    status = -1;
                                    mess = "一行点数量";
                                }
                            }
                            break;
                    }
                    if (status!=0)
                    {
                        return false;
                    }
                    OnLog(LogType.Debug, $"Focal1200:{SN}的{mess}参数设置成功");
                    return true;
                }
                else
                {
                    return false;
                }
            }, $"Focal1200:{SN},{mess}参数设置失败！");
            
        }

        /// <summary>
        /// 光谱参数设置
        /// </summary>
        /// <param name="keyValues"></param>
        public void LaserSetPamas(List<KeyValue> keyValues)
        {
            SafeNativeMethod(() => {

                if (keyValues != null && keyValues.Count > 0 && _isConnect)
                {
                    foreach (KeyValue kv in keyValues)
                    {
                        var paraName = (LaserPamas)Enum.Parse(typeof(LaserPamas), kv.Key);
                        LaserSetPamas(paraName, kv.Value);
                    }
                }
                else
                {
                    return false;
                }
                OnLog(LogType.Debug, $"Focal1200:{SN}参数设置成功");
                return true;
            }, $"Focal1200:{SN},参数设置失败！");
            
        }

        /// <summary>
        /// 相机取流开启
        /// </summary>
        public void LaserStart()
        {
            //开流操作（软触发时，开流就是开始触发）
            SafeNativeMethod(() => {
                if (StartGrabbing()!=0)
                {
                    return false;
                }
                OnLog(LogType.Debug, $"Focal1200:{SN}启动取流");
                return true;
            }, $"Focal1200:{SN},启动取流失败！");
            
        }

        /// <summary>
        /// 线扫关闭
        /// </summary>
        public void LaserStop()
        {
            SafeNativeMethod(() => {
                if (StopGrabbing() != 0)
                {
                    return false;
                }
                OnLog(LogType.Debug, $"Focal1200:{SN}关闭取流");
                return true;
            }, $"Focal1200:{SN},关闭取流失败！");
        }

        /// <summary>
        /// 从本地文件读取配置Job
        /// </summary>
        /// <param name="configPath"></param>
        public void LoadConfig(string configPath)
        {
            SafeNativeMethod(() => {
                if (string.IsNullOrEmpty(configPath))
                {
                    //默认文件地址,相机驱动安装后生成默认地址
                    configPath = @"C:\FocalSpec\SDK Recipes";
                }
                List<string> files = Directory.GetFiles(configPath).ToList();
                foreach (var file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    _jobNames.Add(name);
                }
                OnLog(LogType.Debug, $"Focal1200:{SN}获取Job列表");
                return true;
            }, $"Focal1200:{SN},获取Job列表失败！");
        }

        /// <summary>
        /// 切换Job配置
        /// </summary>
        /// <param name="jobName"></param>
        public void SetJob(string jobName)
        {
            SafeNativeMethod(() =>
            {
                if (_jobNames.Contains(jobName))
                {
                    if (ChangeJob(jobName) != 0)
                    {
                        return false;
                    }
                    OnLog(LogType.Debug, $"Focal1200:{SN}Job切换成功");
                }
                else
                {
                    return false;
                }
                return true;
            }, $"Focal1200:{SN},配方切换失败！");
        }

        /// <summary>
        /// 软触发
        /// </summary>
        public void SoftTrigger()
        {
            //当选择软触发时，Focal1200系列开流就是触发
            SafeNativeMethod(() => {

                if (_sensorF12 != null && !string.IsNullOrEmpty(SN))
                {
                    int status = (int)_sensorF12.GetParameter(SN, SensorParameter.PulseWidthFloat, out double pw);
                    if (status == 0 && pw != 0)
                    {
                        if (StartGrabbing() != 0)
                        {
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }, $"Focal光谱{SN}软触发失败！");
        }

        /// <summary>
        /// 获取配置Job列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetJobs()
        {
            return _jobNames;
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="pamaName"></param>
        /// <returns></returns>
        public object LaserGetPamas(string pamaName = "")
        {
            return null;
        }

        /// <summary>
        /// 提前读取工控机所连接的光谱数量
        /// </summary>
        /// <param name="ips"></param>
        public void LaserListRead(out List<IDevice> ips)
        {
            ips = new List<IDevice>();
            if (_sensorF12 == null)
            {
                _sensorF12 = new FsApi();
            }
            _sensorF12.Open(ref spectrumCount, out spectrumSNs, 1000);
            if (spectrumCount > 0)
            {
                foreach (var cameraId in spectrumSNs)
                {
                    var spectrum = new Focal12Spectrum() { SN = cameraId };
                    ips.Add(spectrum);
                }
            }
        }

        //初始化内存
        private void InitIntPtr(int size, ref IntPtr intPtr)
        {
            if (intPtr != IntPtr.Zero)
            {
                //释放内存 
                Marshal.FreeHGlobal(intPtr);
            }
            intPtr = Marshal.AllocHGlobal(size);
        }


        #endregion

        #region 原始方法封装
        //轮廓回调（1200系列是每一条轮廓都进入回调的）
        private void F12LineCallback(int layerId, float[] zValues, float[] intensityValues, int lineLength, double xStep, FsApi.Header header)
        {
            //单行点距离
            ColDis = xStep;
            //一行点数据量
            ColNum = lineLength;
            //初始化一个指针数据
            InitIntPtr(zValues.Length * sizeof(float), ref linePtr);
            Marshal.Copy(zValues, 0, linePtr, lineLength);
            CopyMemory(_cloudDatas + _lengthData, linePtr, sizeof(float) * lineLength);
            if (header.Index >= RowNum - 1)
            {
                //点云数据量满足，就停止取流
                _sensorF12.StopGrabbing(SN);
                //单挑数据线的字节长度
                _lengthData = 0;
                _laserData = new LineLaser()
                {
                    ZPointer = _cloudDatas,
                    Row = RowNum,
                    RowDis = RowDis,
                    Column = lineLength,
                    ColDis = xStep,
                    //Focal1200所有系列均是该值
                    Resolution = 0.001,
                    ZDataType = Common.DataStruct.Enums.DataType.Float,
                };
                ScanFinishEvent?.Invoke(_laserData);
            }
            _lengthData += sizeof(float) * lineLength;
        }
        //光谱连接
        private int Focal12Connect()
        {
            int status = -1;
            if (!_isConnect)
            {
                if (_sensorF12 == null)
                {
                    _sensorF12 = new FsApi();
                }
                //连接超时1000ms
                var cameraStatus = _sensorF12.Open(ref spectrumCount, out spectrumSNs, 1000);
                //连接
                if (spectrumSNs.Contains(SN))
                {
                    //这里应该是根据配置信息进而进行连接的
                    cameraStatus = _sensorF12.Connect(SN, string.Empty);
                    if (_sensorF12.IsConnected(SN) == CameraStatusCode.NotConnected)
                    {
                        _isConnect = false;
                        return -1;
                    }
                    //设置回调函数
                    _sensorF12.SetLineCallback(SN, 0, F12LineCallback);
                    //设置模式
                    _sensorF12.SetParameter(SN, SensorParameter.SurfaceMode, 2);
                    //设置标定系数
                    _sensorF12.SetParameter(SN, SensorParameter.PeakXUnit, 1);
                    _sensorF12.SetParameter(SN, SensorParameter.PeakYUnit, 1);
                    //LED脉冲宽度,可以控制曝光值，通常设置在1us`100us 在低光泽度表面也可能更高  （注意该值设置为0是就是硬件触发,此处默认设置成软软触发）
                    _sensorF12.SetParameter(SN, SensorParameter.PulseWidthFloat, (float)500);
                    //图像采集帧率
                    _sensorF12.SetParameter(SN, SensorParameter.PulseFrequency, 500);
                    //FIR参数 FIR越高，峰值位置越准确。FIR越低，可以检测透明层状材料中的较薄层
                    _sensorF12.SetParameter(SN, SensorParameter.FirLength, 16);
                    //检测滤波器用于平均信号，以降低阈值前的噪声 . 当检测到的信号相当微弱时，低检测滤波器长度很有用。
                    _sensorF12.SetParameter(SN, SensorParameter.SignalDetectionFilterLength, 8);
                    //滤波器用于平均检测到的峰值的强度值。使用高平均强度过滤器长度，可以生成最佳的强度图像
                    _sensorF12.SetParameter(SN, SensorParameter.PeakAverageIntensityFilterLength, 8);
                    //峰值检测中使用的强度阈值。如果峰值强度低于此限制，则丢弃峰值
                    _sensorF12.SetParameter(SN, SensorParameter.PeakThreshold, 16);
                }
                else
                {
                    _isConnect = false;
                    return -1;
                }
                OnLog(LogType.Debug, $"Focal1200:{SN}连接成功");
                status = 0;
                _isConnect = true;
                OnStatusChanged(DeviceStatus.Online);
            }
            else
            {
                status = 0;
            }
            return status;
        }

        //开始取流操作
        private int StartGrabbing()
        {
            int status = -1;
            //给拼接数据长度设置为0
            _lengthData = 0;
            //和光谱型号相关，因为不同型号，其一行数据点数不同   一般有 两种 2048 和 1728
            int sizePtr = 2048 * RowNum * sizeof(float);
            //初始化点云指针
            InitIntPtr(sizePtr, ref _cloudDatas);
            //取流前先关闭一下
            _sensorF12.StopGrabbing(SN);
            Thread.Sleep(30);
            status = (int)_sensorF12.StartGrabbing(SN);
            return status;
        }

        //停止取流操作，  返回值  0 表示成功
        private int StopGrabbing()
        {
            var status = (int)_sensorF12.GetParameter(SN, SensorParameter.Grabbing, out int isGrabbing);
            if (status != 0)
            {
                return status;
            }
            //isGrabbing 0 已经停止状态  1 表示正在取流
            if (isGrabbing != 0)
            {
                status = (int)_sensorF12.StopGrabbing(SN);
                Thread.Sleep(30);
            }
            return status;
        }

        //上载配方
        private int ChangeJob(string recipe)
        {
            int status = -1;
            if (File.Exists(recipe + ".json"))
            {
                _curJobFile = recipe;
                status = (int)_sensorF12.SetParameter(SN, SensorParameter.LoadRecipe, recipe);
            }
            else
            {
                status = -1;
            }
            return status;
        }

        //存储配方到本地文件
        private int SaveJob(string jobPath)
        {
            int status = -1;
            _curJobFile = jobPath;
            status = (int)_sensorF12.SetParameter(SN, SensorParameter.SaveRecipe, jobPath);
            return status;
        }

        public void LaserSetSpPama(string key, object value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}