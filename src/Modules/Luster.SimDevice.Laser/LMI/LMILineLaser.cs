#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LMILineLaser
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.LMILaser
* 文 件 名:       LMILineLaser.cs
* 创建时间:       2022/4/11 14:34:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      cdc2140b-4779-4266-bd3e-ede4b987a795
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 14:34:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Lmi3d.GoSdk;
using Lmi3d.GoSdk.Messages;
using Lmi3d.Zen;
using Lmi3d.Zen.Io;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.SimDevice.Laser.LMI
{
    /// <summary>
    /// LMI 激光设备
    /// </summary>
    public class LMILineLaser : LineLaserBase, ILineLaser
    {
        [DllImport("kernel32.dll")]
        static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        public override string Brand => "LMI";

        public VIO StartIO { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Z方向指针
        /// </summary>
        private IntPtr linePtr = IntPtr.Zero;
        private LineLaser _laserData;
        private GoSystem goSystem;
        private GoSensor goSensor;
        public event Action<LineLaser> ScanFinishEvent;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LMILineLaser()
        {

        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~LMILineLaser()
        {
            if (linePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(linePtr);
            }
        }

        /// <summary>
        /// 扫码所有Sensor口
        /// </summary>
        /// <param name="ips"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void LaserListRead(out List<IDevice> ips)
        {
            ips = new List<IDevice>();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public override void InitApi()
        {
            SafeNativeMethod(() =>
            {
                // 1.激光初始化
                KApiLib.Construct();
                GoSdkLib.Construct();
                goSystem = new GoSystem();

                // 2.从离线到在线
                OnStatusChanged(DeviceStatus.Online);
                return true;
            });
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        private void UpdateStatus()
        {
            if (Enum.TryParse<DeviceStatus>(goSensor.State.ToString(), out var status))
            {
                OnStatusChanged(status);
            }
        }

        /// <summary>
        /// 设备打开
        /// </summary>
        public override void Open()
        {
            LMIConnect();
            base.Open();
        }

        private void LMIConnect()
        {
            if (Adapter == null || !(Adapter is Network))
            {
                ErrMsg = "连接方式没有配置";
            }
            SafeNativeMethod(() =>
            {
                // 1.ip连接
                Network network = Adapter as Network;
                if (network != null)
                {
                    var ipAddr = KIpAddress.Parse(network.Ip);
                    goSensor = goSystem.FindSensorByIpAddress(ipAddr);
                }

                // 1.判断
                if (goSensor == null) return false;

                // 2、相机连接与载入对应文件,连接超时认为相机不存在
                goSensor.Connect();
                goSystem.Connect();
                if (goSensor.State.ToString() != "Offline")
                {
                    // 更新状态
                    UpdateStatus();
                    // 3.使能和设置函数回调
                    goSystem.EnableData(true);

                    // 注册回调
                    goSystem.SetDataHandler(OnFinish);
                    return true;
                }
                else
                {
                    return false;
                }

            }, $"LMI激光{SN}打开失败");
        }

        /// <summary>
        /// 设备关闭
        /// </summary>
        public override void Close()
        {
            Status = DeviceStatus.Offline;

            SafeNativeMethod(() =>
            {
                if (goSystem != null)
                {
                    goSystem?.Dispose();
                    goSystem = null;
                }

                if (goSensor != null)
                {
                    goSensor?.Dispose();
                    goSensor = null;
                }

                return true;
            });
        }

        /// <summary>
        /// 启用禁用设备
        /// </summary>
        /// <param name="enabled">启用或禁用</param>
        public override void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                // 4.启动
                goSystem?.Start();
            }
            else
            {
                goSystem?.Stop();
            }
        }


        /// <summary>
        /// 回调事件
        /// </summary>
        /// <param name="data">线激光数据</param>
        private void OnFinish(KObject data)
        {
            var dataSet = (GoDataSet)data;
            if (dataSet.Count < 2) return;
            GoDataMsg dataObj = (GoDataMsg)dataSet.Get(1);

            // 更新激光状态
            UpdateStatus();

            if (dataObj.MessageType == GoDataMessageType.UniformSurface)
            {
                GoUniformSurfaceMsg goSurfaceMsg = (GoUniformSurfaceMsg)dataObj;
                // width 是宽的点数
                // height 是激光条数
                // bufferSize 是一次点云数据量
                int width = (int)goSurfaceMsg.Width;
                int height = (int)goSurfaceMsg.Length;
                int bufferSize = width * height;

                // 分辨率
                double resoluton = 12.5 / 32767.0000;

                // 获取Z向的值
                short[] ranges = new short[bufferSize];

                IntPtr bufferPointer = goSurfaceMsg.Data;

                // 释放上次开辟的对象
                if (linePtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(linePtr);
                }

                linePtr = Marshal.AllocHGlobal(bufferSize);
                CopyMemory(linePtr, goSurfaceMsg.Data, bufferSize);

                // 行距离，单位是纳米
                double rowDis = goSurfaceMsg.YResolution / 1000000.0;
                double colDis = goSurfaceMsg.XResolution / 1000000.0;

                // 构建对象
                _laserData = new LineLaser()
                {
                    ZPointer = linePtr,
                    Row = height,
                    RowDis = rowDis,
                    Column = width,
                    ColDis = colDis,
                    Resolution = resoluton,
                    ZDataType = Common.DataStruct.Enums.DataType.Short,
                };
            }
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        /// <param name="isDispose"></param>
        protected override void Dispose(bool isDispose)
        {
            base.Dispose(isDispose);
            Status = DeviceStatus.Offline;
            if (isDispose)
            {
                Close();
            }
        }


        public void LaserOpen(string id = "")
        {
            //激光开启
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取工作
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<string> GetJobs()
        {
            List<string> sensorJobName = new List<string>();
            if (goSensor != null)
            {
                for (int i = 0; i < goSensor.FileCount; i++)
                {
                    if (goSensor.GetFileName(i).Contains(".job"))
                    {
                        sensorJobName.Add(goSensor.GetFileName(i));
                    }
                    sensorJobName.Remove("_live.job");
                }
            }
            return sensorJobName;
        }

        /// <summary>
        /// 配置JOB
        /// </summary>
        /// <param name="jobName"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetJob(string jobName)
        {
            if (goSensor != null)
            {
                goSensor.Stop();
                Thread.Sleep(100);
                goSensor.CopyFile(jobName, "_live.job");
                goSensor.Start();
            }
        }

        public void LoadConfig(string configPath)
        {

        }

        public void LaserStart()
        {
            throw new NotImplementedException();
        }

        public void LaserStop()
        {
            throw new NotImplementedException();
        }

        public void SoftTrigger()
        {
            throw new NotImplementedException();
        }

        public void LaserSetPamas(List<KeyValue> keyValues)
        {
            throw new NotImplementedException();
        }

        public void LaserSetPamas(LaserPamas pamaName, object val)
        {
            throw new NotImplementedException();
        }

        public object LaserGetPamas(string pamaName)
        {
            throw new NotImplementedException();
        }

        public void LaserSetSpPama(string key, object value)
        {
            throw new NotImplementedException();
        }
    }
}