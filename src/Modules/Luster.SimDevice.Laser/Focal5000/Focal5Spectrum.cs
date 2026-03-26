#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Focal5000
* 机器名称:       L05014-NB
* 命名空间:       Luster.SimDevice.LMILaser.Focal5000
* 文 件 名:       FocalLineSpectrum5000.cs
* 创建时间:       2022/9/20 19:34:44
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      3dae138c-c794-4243-94ac-c4058ad00374
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/20 19:34:44
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using GoPxlSDKNet;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Laser.Focal5000
{
    public class Focal5Spectrum : LineLaserBase, ILineLaser
    {
        public Focal5Spectrum()
        {

        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~Focal5Spectrum()
        {
            if (linePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(linePtr);
            }
        }
        #region 字段

        private IntPtr linePtr = IntPtr.Zero;

        private LineLaser _laserData;

        public override string Brand => "Focal5000";

        private GoDiscoveryClient _discoverClient = null;

        private Dictionary<string, GoSystemEx> _systems = new Dictionary<string, GoSystemEx>();

        private GoSystemEx _senser;

        public event Action<LineLaser> ScanFinishEvent;
        #endregion

        #region 属性

        /// <summary>
        /// 是否已经打开线扫激光
        /// </summary>
        public bool IsOpen { get; set; }
        public VIO StartIO { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化API
        /// </summary>
        /// <param name="configFile"></param>
        /// <returns></returns>
        public override void InitApi()
        {
            SafeNativeMethod(() =>
            {
                // 1.构造Api，需在调用GoPxlSdkNet 前使用
                GoApi.Construct();
                return true;
            });
        }

        public override void Open()
        {
            base.Open();
            LaserOpen();
        }

        public override void Close()
        {
            base.Close();
            if (!string.IsNullOrEmpty(SN))
            {
                _senser = GetSystem(SN);
                _senser?.Stop();
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
                _senser?.Start();
            }
            else
            {
                _senser?.Stop();
            }
        }

        /// <summary>
        /// 获取线激光数据
        /// </summary>
        /// <returns></returns>
        public LineLaser GetLaserData()
        {
            return _laserData;
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

        public void LaserListRead(out List<IDevice> laserList)
        {
            laserList = new List<IDevice>();

            //1.扫描设备
            if (Discover() == 0)
            {
                //2.获得扫描设备的ID并确确定与当前ID匹配
                List<string> senserIDs = GetAllSenserID();

                for (int i = 0; i < senserIDs.Count; i++)
                {
                    laserList.Add(new Focal5Spectrum());
                }
            }
        }

        public void LaserOpen(string id = "")
        {
            SafeNativeMethod(() =>
            {
                //1.扫描设备
                if (Discover() == 0)
                {
                    //2.获得扫描设备的ID并确确定与当前ID匹配
                    IEnumerable<string> senserIDs = GetAllSenserID().Where(u => u == SN);
                    if (senserIDs.Count() == 1)
                    {
                        //3.开启当前相机
                        _senser = GetSystem(senserIDs.ToArray()[0]);
                        _senser.Connect();
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
            }
           , $"FocalSpec激光{SN}打开失败");
        }

        #endregion

        #region Focal基本方法
        /// <summary>
        /// 发现传感器
        /// </summary>
        /// <summary>
        /// 发现当前网段内所有的GoPxl对象
        /// </summary>
        /// <returns>1 成功，其他： 失败</returns>
        private int Discover()
        {
            if (_discoverClient == null)
                _discoverClient = new GoDiscoveryClient();
            _discoverClient.BlockingDiscover(10000);
            if (_discoverClient.Count() > 0)
                return 1;
            return 0;
        }

        /// <summary>
        /// 获得所有传感器 的App id
        /// </summary>
        /// <returns></returns>
        private List<string> GetAllSenserID()
        {
            List<string> list = new List<string>();
            ulong nCount = _discoverClient.Count();
            for (ulong i = 0; i < nCount; i++)
            {
                list.Add(_discoverClient.GetInstanceAt(i).GetAppId());
            }
            return list;
        }

        /// <summary>
        /// 构建或者获取GoSystemEx 对象，该对象也可以通过Ip地址，rest 端口号，gdp端口进行创建
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private GoSystemEx GetSystem(string id)
        {
            if (_systems.ContainsKey(id))
                return _systems[id];
            else
            {
                GoSystemEx s = new GoSystemEx(_discoverClient.GetInstance(id));
                _systems.Add(id, s);
                return s;
            }
        }

        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private int OnData(IntPtr data)
        {
            GoDataSet ds = new GoDataSet(data);
            ulong count = ds.Count();
            for (ulong i = 0; i < count; i++)
            {
                GoGdpMsg msg = ds.GdpMsgAt(i);
                switch (msg.Type())
                {
                    case MessageType.STAMP:
                        {
                            GoGdpStamp gdpStamp = new GoGdpStamp(msg.Ptr);
                            Stamp stamp = gdpStamp.GetStamp();
                            break;
                        }
                    case MessageType.UNIFORM_SURFACE:
                        {
                            GoGdpSurfaceUniform surfaceMsg = new GoGdpSurfaceUniform(msg.Ptr);
                            int dataLength = (int)(surfaceMsg.Length() * surfaceMsg.Width());
                            short[] surfaceBuffer = new short[dataLength];
                            Marshal.Copy(surfaceMsg.Ranges(), surfaceBuffer, 0, dataLength);
                            break;
                        }
                    case MessageType.UNIFORM_PROFILE:
                        {
                            GoGdpProfileUniform profileMsg = new GoGdpProfileUniform(msg.Ptr);
                            short[] surfaceBuffer = new short[profileMsg.Width()];
                            Marshal.Copy(profileMsg.Ranges(), surfaceBuffer, 0, (int)profileMsg.Width());

                            break;
                        }
                    case MessageType.LINE_FEATURE:
                        {
                            GoGdpFeatureLine lineMsg = new GoGdpFeatureLine(msg.Ptr);
                            break;
                        }
                    case MessageType.POINT_FEATURE:
                        {
                            GoGdpFeaturePoint pointMsg = new GoGdpFeaturePoint(msg.Ptr);
                            break;
                        }
                    case MessageType.CIRCLE_FEATURE:
                        {
                            GoGdpFeatureCircle circleMsg = new GoGdpFeatureCircle(msg.Ptr);
                            break;
                        }
                    case MessageType.PLANE_FEATURE:
                        {
                            GoGdpFeaturePlane planeMsg = new GoGdpFeaturePlane(msg.Ptr);
                            break;
                        }
                }
            }
            return 1;
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        private void UpdateStatus()
        {
            if (Enum.TryParse<DeviceStatus>("", out var status))
            {
                OnStatusChanged(status);
            }
        }

        public List<string> GetJobs()
        {
            throw new NotImplementedException();
        }

        public void SetJob(string jobName)
        {
            throw new NotImplementedException();
        }

        public void LaserSetPamas(LaserPamas pamaName, object val)
        {
            throw new NotImplementedException();
        }

        object ILineLaser.LaserGetPamas(string pamaName)
        {
            throw new NotImplementedException();
        }

        public void LoadConfig(string configPath)
        {
            throw new NotImplementedException();
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

        public void LaserSetSpPama(string key, object value)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}