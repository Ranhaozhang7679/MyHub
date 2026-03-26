using Luster.Common.DataStruct.DataModels;
using Luster.SimDevice.Adapter;
using Smartray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Laser.SmartRay
{
    public class NativeMethod : IDisposable
    {

        private volatile static NativeMethod instance = null;

        private static readonly object padlock = new object();

        private NativeMethod()
        {
            InitApi();
        }

        public static NativeMethod Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new NativeMethod();
                        }
                    }
                }
                return instance;
            }
        }

        #region 字段

        // 传感器集合
        static List<SRLineLaser> _sensors = new List<SRLineLaser>();

        #endregion

        #region 静态方法

        public static void RemoveSensor(SRLineLaser sRLineLaser)
        {
            var sen = _sensors.Where(u => u._sensorObject.IPAdr.Equals((sRLineLaser.Adapter as Network).Ip)).ToList()[0];
            if (sen != null)
            {
                _sensors.Remove(sen);
            }
        }

        public static int GetSensorCount()
        {
            return _sensors.Count;
        }

        //API状态回调
        static int ApiStatusCallback(Api.Sensor sensorObject, Api.MessageType msgType, Api.SubMessageType subMsgType, int msgData, string msg)
        {
            // 检查信息是否为空
            if (msg == null)
                return -1;
            //匹配相机对象
            SRLineLaser sensor;
            if (!TryGetSensor(sensorObject, out sensor))
                return -1;
            // 网口连接问题
            if (msgType == Api.MessageType.Connection)
            {
                sensor._sensorState.LastConnectionMessage = msg;
                //连接状态更新
                if (subMsgType == Api.SubMessageType.Connection_SensorConnected)
                {
                    sensor._sensorState.SensorConnection = SensorState.ConnectionState.Connected;
                }
                else
                {
                    sensor._sensorState.SensorConnection = SensorState.ConnectionState.Disconnected;
                }
            }
            //消息记录
            else if (msgType == Api.MessageType.Info)
            {
                sensor._sensorState.LastInfoMessage = msg;
            }

            //错误消息
            else if (msgType == Api.MessageType.Error)
            {
                sensor._sensorState.LastErrorMessage = msg;
            }
            return 0;
        }

        //PIL图回调
        static int PilImageCallback(Api.Sensor sensorObject, Api.ImageDataType imageType, int originX, int height, int width, ushort[] profileImage, ushort[] intensityImage, ushort[] lltImage, int numExtData, Byte[] extData)
        {
            SRLineLaser sensor;
            if (!TryGetSensor(sensorObject, out sensor))
                return -1;
            return 0;
        }

        //点云回调
        static int PointCloudCallback(Api.Sensor sensorObject, Api.ImageDataType imageType, int numPoints, int numProfiles, Api.Point3d[] pointCloud, ushort[] intensity, ushort[] laserlinethickness, uint[] porfileIndex, uint[] columnIndex, int numExtData, ushort[] extData)
        {
            SRLineLaser sensor;
            if (!TryGetSensor(sensorObject, out sensor))
                return -1;
            return 0;
        }

        //ZIL图回调
        static int ZilImageCallback(Api.Sensor sensorObject, Api.ImageDataType imageType, int height, int width, float verticalRes, float horizontalRes, ushort[] zMap, ushort[] intensityZmap, ushort[] lltZmap, float originYMillimeters, int numExtData, ushort[] extData)
        {
            //匹配对象
            SRLineLaser sensor;
            if (!TryGetSensor(sensorObject, out sensor))
                return -1;
            sensor.DataInvoke(sensorObject, imageType, height, width, verticalRes, horizontalRes, zMap);
            return 0;
        }

        //多传感器点云回调
        static int MSRPointCloudCallback(Api.Sensor sensorObject, Api.ImageDataType imageType, int numPoints,
            Api.Point3d[] pointCloud, ushort[] intensity, ushort[] laserlinethickness,
            uint[] sensorIndex, uint[] porfileIndex, uint[] pointIndex,
            int numExtData, ushort[] extData)
        {
            SRLineLaser sensor;
            if (!TryGetSensor(sensorObject, out sensor))
                return -1;
            return 0;
        }

        //根据顺序匹配加载线扫
        static bool TryGetSensor(Api.Sensor sensorObject, out SRLineLaser sensor)
        {
            foreach (var sensorCandidate in _sensors)
            {
                if (sensorCandidate._sensorObject.cam_index == sensorObject.cam_index && sensorCandidate._sensorObject.IPAdr == sensorObject.IPAdr)
                {
                    sensor = sensorCandidate;
                    return true;
                }
            }
            sensor = null;
            return false;
        }

        //初始化
        public void Init()
        {

        }

        //初始化API
        void InitApi()
        {
            int ret;
            string apiVersion;
            ret = Api.GetApiVersion(out apiVersion);
            ret = Api.Initialize(ApiStatusCallback);
            // 注册PIL回调
            Api.RegisterPilImageCB(PilImageCallback);
            //注册ZIL 回调
            Api.RegisterZilImageCB(ZilImageCallback);
        }

        //退出Api
        int DeinitApi()
        {
            return Api.Exit();
        }

        #endregion

        //注销
        public void Dispose()
        {
            DeinitApi();
            foreach (var sensor in _sensors)
                sensor.Dispose();
            _sensors.Clear();
        }

        //复制传感器
        public Api.Sensor CreateSensor(SRLineLaser sRLineLaser, string name, int multiSensorIndex = 0, string ipAddress = "192.168.178.200", ushort port = 40)
        {
            if (!_sensors.Any(u => u._sensorObject.IPAdr.Equals((sRLineLaser.Adapter as Network).Ip)))
            {
                sRLineLaser._sensorObject = new Api.Sensor();
                sRLineLaser._sensorState = new SensorState();
                // 如果要实施>1个传感器，则每个新传感器增加一个
                sRLineLaser._sensorObject.cam_index = multiSensorIndex;
                // 传感器名称
                sRLineLaser._sensorObject.name = name;
                // 传感器对象IP
                sRLineLaser._sensorObject.IPAdr = ipAddress;
                // 传感器对象IP
                sRLineLaser._sensorObject.portnum = port;
                sRLineLaser.SensorIndex = multiSensorIndex;
                //检查是否已经存在
                _sensors.Add(sRLineLaser);
            }
            return sRLineLaser._sensorObject;
        }
    }

    public class SensorState : ICloneable
    {

        public SensorState()
        {
            ImagePackageCounter = 0;
            AliveSignalCounter = 0;
            SensorConnection = ConnectionState.Unknown;
        }
        #region 字段
        //状态记录
        public ConnectionState SensorConnection;
        //上次连接状态记录
        public string LastConnectionMessage = string.Empty;
        //上次消息记录
        public string LastInfoMessage = string.Empty;
        //上次错误消息记录
        public string LastErrorMessage = string.Empty;
        // 数据包大小
        public long ImagePackageCounter;
        // 活动信号增加
        public long AliveSignalCounter;

        #endregion

        #region 方法
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        #endregion

        #region 类型设置

        /// <summary>
        /// 相机连接状态
        /// </summary>
        public enum ConnectionState
        {
            Unknown = 0,
            Connecting = 1,
            Connected = 2,
            Disconnected = 3
        }

        #endregion
    }

    public enum ParameterSet
    {
        Undefined = 0,
        LiveImage = 1,
        Snapshot3d = 2,
        Snapshot3dRepeat = 3,
    };
}
