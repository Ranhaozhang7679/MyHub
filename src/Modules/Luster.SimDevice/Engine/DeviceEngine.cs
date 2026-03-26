#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceEngine
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Engine
* 文 件 名:       DeviceEngine.cs
* 创建时间:       2022/4/6 10:14:16
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3fe993a0-117a-44b1-9776-f2beb781e3ef
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/6 10:14:16
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Luster.SimDevice.Engine
{
    public class DeviceEngine : IDeviceEngine
    {
        /// <summary>
        /// 是否设置CT耗时中各个模块的标准CT时间
        /// </summary>
        public bool AutoSetMotionModuleCT { get; set; }
        /// <summary>
        /// 是否需要保存
        /// </summary>
        public bool IsNeedSave { get; set; }

        /// <summary>
        /// 运行模式
        /// </summary>
        public DeviceMode DeviceMode { get; set; }

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event Action<string, object, object> PropertyChangedEvent;

        /// <summary>
        /// 硬件设备列表
        /// </summary>
        private List<IDevice> deviceList;

        /// <summary>
        /// 虚拟设备列表
        /// </summary>
        private List<IVirtualDevice> virtualDevices;

        /// <summary>
        /// 设备类型
        /// </summary>
        private List<Type> RdeviceTypes;

        /// <summary>
        /// Virtual设备类型
        /// </summary>
        private List<Type> VdeviceTypes;

        /// <summary>
        /// 设备列表-类型
        /// </summary>
        private Dictionary<Type, object> dic_Devices;

        /// <summary>
        /// 要进行加载的dll
        /// </summary>
        private readonly string StartDll = "Luster.SimDevice";

        /// <summary>
        /// dll附加目录关键字
        /// </summary>
        private readonly string DeviceStr = "Devices";

        /// <summary>
        /// 任务文件后缀名
        /// </summary>
        private readonly string TaskExtension = ".dproj";

        /// <summary>
        /// 设备配置文件
        /// </summary>
        private string deviceTask = "";

        public string RecipeDataPath { get; set; }

        /// <summary>
        /// 日志事件
        /// </summary>
        public event Action<LogType, string> LogEvent;

        /// <summary>
        /// Loading事件
        /// <para>第一个参数是 总数量</para>
        /// <para>第二个参数是 当前数量</para>
        /// <para>第三个参数是 提示信息</para>
        /// </summary>
        public event Action<int, int, string> LoadingEvent;

        /// <summary>
        /// 报警事件
        /// </summary>
        public event Action<AlarmInfo> AlarmEvent_device;

        /// <summary>
        /// 删除前进行参数校验，防止被引用
        /// </summary>
        public event Func<IVirtualDevice, string> PrevDeleteEvent;

        /// <summary>
        /// 轴点位删除，进行校验防止被引用
        /// </summary>
        public event Func<AxisPosition, string> AxisPosDeleteEvent;

        /// <summary>
        /// 报警号发生修改事件
        /// </summary>
        public event Action<string, string> AlarmCodeChangedEvent;

        public void RaiseAlarmCodeChangedEvent(string oldCode, string newCode)
        {
            AlarmCodeChangedEvent?.Invoke(oldCode, newCode);
        }

        /// <summary>
        /// 设备发生变更
        /// </summary>
        public event Action VDeviceChangedEvent;

        /// <summary>
        /// 全局保存事件
        /// </summary>
        public event Action SaveEvent;

        public void RaiseVDeviceChangedEvent()
        {
            VDeviceChangedEvent?.Invoke();
        }

        /// <summary>
        /// 初始化完成
        /// </summary>
        public event Action<IDeviceEngine, string> InitializedEvent;

        /// <summary>
        /// 获取机台状态事件
        /// </summary>
        public event Action<EngineStatus, EngineStatus> StatusChangedEvent;

        /// <summary>
        /// 获取机台状态
        /// </summary>
        public event Func<EngineStatus> GetMachineStatusEvent;

        /// <summary>
        /// 模式变更
        /// </summary>
        public event Action<DeviceMode> ModeChangedEvent;

        /// <summary>
        /// 模式变更之前事件
        /// </summary>
        public event Action<bool> PrevModeChangeEvent;

        /// <summary>
        /// 获取所有module的名称或者某个module
        /// </summary>
        public event Func<string, string, List<object>> GetModuleListEvent;
        public event Func<IEnumerable<object>> GetPDCAModulesEvent;
        public event Func<IEnumerable<object>> GetSFCModulesEvent;

        /// <summary>
        /// 
        /// </summary>
        private ManualResetEventSlim pauseReset = new ManualResetEventSlim(true);

        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="alarm"></param>
        public void OnAlarm(AlarmInfo alarm)
        {
            AlarmEvent_device?.Invoke(alarm);
        }

        /// <summary>
        /// 触发消息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void OnLog(LogType type, string message)
        {
            LogEvent?.Invoke(type, message);
        }

        /// <summary>
        /// 加载进度条件
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cur"></param>
        /// <param name="message"></param>
        public void OnLoading(int count, int cur, string message)
        {
            LoadingEvent?.Invoke(count, cur, message);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DeviceEngine()
        {
            deviceList = new List<IDevice>();
            virtualDevices = new List<IVirtualDevice>();
            RdeviceTypes = new List<Type>();
            VdeviceTypes = new List<Type>();
            dic_Devices = new Dictionary<Type, object>();
        }

        /// <summary>
        /// 对象初始化
        /// </summary>
        /// <param name="deviceTask"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Initialize(string deviceTask)
        {
            if (string.IsNullOrEmpty(deviceTask))
            {
                throw new ArgumentNullException(nameof(deviceTask));
            }

            if (this.deviceTask == deviceTask)
            {
                return;
            }

            // 如果当前模式真实模式，要对原来的对象进行释放
            if (DeviceMode == DeviceMode.Real)
            {
                SetEngineMode(DeviceMode.Virtual, out var msg);
            }

            if (deviceList.Count > 0 || virtualDevices.Count > 0)
            {
                // 清理列表
                deviceList.Clear();
                virtualDevices.Clear();
                //通知页面设备已清空
                VDeviceChangedEvent?.Invoke();
            }

            if (Path.GetExtension(deviceTask).ToLower() != TaskExtension)
            {
                throw new FormatException($"仅支持后缀{TaskExtension}格式任务");
            }

            try
            {
                RdeviceTypes = GetRealTypes();
                VdeviceTypes = GetVirtualTypes();

                XElement xRoot = XElement.Load(deviceTask);
                foreach (var xItem in xRoot.Elements())
                {
                    var xType = xItem.Attribute("Type");
                    if (xType == null) continue;

                    // 字符串类型
                    var strType = xType.Value;
                    if (xItem.Name == "Device")
                    {
                        var realType = GetRealTypes().FirstOrDefault(u => u.Name == strType);
                        var device = Activator.CreateInstance(realType) as IDevice;
                        device.ParserXml(xItem);

                        AddDevice(device);
                    }
                    else if (xItem.Name == "VDevice")
                    {
                        var realType = GetVirtualTypes().FirstOrDefault(u => u.Name == strType);

                        var vDevice = Activator.CreateInstance(realType) as IVirtualDevice;
                        vDevice.Engine = this;

                        vDevice.ParserXml(xItem);
                        AddVirtual(vDevice);
                    }
                }

                // 安全区域
                XElement xSafe = xRoot.Element("SafeRegion");
                if (xSafe != null)
                {
                    foreach (var xItem in xSafe.Elements())
                    {
                        xItem.GetAttribute("AxisID", axisID =>
                        {
                            var vID = Guid.Parse(axisID);
                            var vAxis = virtualDevices.FirstOrDefault(u => u.ID == vID) as VAxis;
                            if (vAxis != null && xItem.HasElements)
                            {
                                foreach (var xSubItem in xItem.Elements())
                                {
                                    var newSafe = new SafeModel(vAxis);
                                    newSafe.ParserXml(xSubItem);
                                }
                            }
                        });
                    }
                }

                // 点位安全区域-Jack
                XElement xPosionSafe = xRoot.Element("PosionSafeRegion");
                if (xPosionSafe != null)
                {
                    foreach (var xItem in xPosionSafe.Elements())
                    {
                        xItem.GetAttribute("PosionID", posionID =>
                        {
                            var vID = Guid.Parse(posionID);
                            var axisPosion = GetAxisPosition(vID);
                            if (axisPosion != null && xItem.HasElements)
                            {
                                foreach (var xSubItem in xItem.Elements())
                                {
                                    var newSafe = new PosionSafeModel(axisPosion.Axis, axisPosion);
                                    newSafe.ParserXml(xSubItem);
                                }
                            }
                        });
                    }
                }

                XElement xPosGroup = xRoot.Element(nameof(PosGroup));
                LoadPosGroup(xPosGroup);

                XElement xModule = xRoot.Element(nameof(ModuleNameGroup));
                if (xModule != null)
                {
                    LoadModuleNameGroup(xModule);
                }

                //XmlElement controlPara = xRoot.GetElement();



                //加载参数
                //   LoadControlPara();



                // 由于调用了AddDevice，默认将IsNeedSave更新为True，所以需要置为false
                IsNeedSave = false;
                this.deviceTask = deviceTask;
                InitializedEvent?.Invoke(this, deviceTask);

                //获取设备类型字典
                GetDevice_Dic();

            }
            catch (XmlException xE)
            {
                throw xE;
            }
        }

        private Dictionary<Type, object> GetDevice_Dic()
        {
            try
            {
                foreach (var item in RdeviceTypes)
                {
                    var device = Activator.CreateInstance(item) as IDevice;
                    if (!dic_Devices.Any(u => ((IDevice)u.Value).DeviceType == device.DeviceType))
                    {
                        dic_Devices.Add(item, device);
                    }
                }
                foreach (var item in VdeviceTypes)
                {
                    var device = Activator.CreateInstance(item) as IVirtualDevice;
                    if (item.Name == "VPrinter" || item.Name == "VAxisM") continue;

                    dic_Devices.Add(item, device);

                }
                return dic_Devices;
            }
            catch (Exception)
            {
                return dic_Devices;
            }

        }

        /// <summary>
        /// 获取设备类型字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<Type, object> GetDevice_Type()
        {
            return dic_Devices;
        }

        /// <summary>
        /// 对现有硬件进行检查，确认连接是否正常
        /// </summary>
        /// <param name="msg">错误消息</param>
        /// <returns>是否都正常</returns>
        public bool CheckHardware(out string errMsg)
        {
            errMsg = "";
            StringBuilder sb = new StringBuilder();
            foreach (var item in deviceList)
            {
                try
                {
                    // 设备API 初始化
                    item.InitApi();

                    // 连接尝试
                    item.Open();
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"设备:{item.Name} 连接失败!");
                    break;
                }
            }

            errMsg = sb.ToString();

            return true;
        }

        /// <summary>
        /// 通过程序集获取对应的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private List<Type> GetTypesByAssembly<T>(Assembly assembly)
        {
            List<Type> types = new List<Type>();
            try
            {
                var allTypes = assembly.GetTypes();
                foreach (var type in allTypes)
                {
                    if (!type.IsInterface && !type.IsAbstract && typeof(T).IsAssignableFrom(type))
                    {
                        types.Add(type);
                    }
                }

                return types;
            }
            catch (ReflectionTypeLoadException rE)
            {
                OnLog(LogType.Error, $"程序集:{Path.GetFileName(assembly.Location)} 解析失败,请确认驱动是否安装成功!");
            }
            catch (Exception ex)
            {
                throw;
            }
            return types;
        }

        /// <summary>
        /// 获取所有的连接器
        /// </summary>
        /// <returns></returns>
        public List<Type> GetAdapters()
        {
            return GetTypesByAssembly<IAdapter>(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// 通过类型获取设备厂商集合信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<KeyValue> GetBrands(Type deviceType)
        {
            List<KeyValue> brands = new List<KeyValue>();

            var types = GetRealTypes().Where(u => deviceType.IsAssignableFrom(u)).ToList();
            foreach (var type in types)
            {
                if (typeof(IDevice).IsAssignableFrom(type))
                {
                    var device = Activator.CreateInstance(type) as IDevice;
                    brands.Add(new KeyValue()
                    {
                        Key = type.Name,
                        Desc = device.Brand,
                        Value = type
                    });
                }
            }

            return brands;
        }

        /// <summary>
        /// 加载驱动
        /// </summary>
        public void LoadDrivers(string loadPath = "")
        {
            string path = $"{Luster.Common.Tools.ReflectionTool.GetAssemblyPath()}\\{DeviceStr}\\";
            if (!string.IsNullOrEmpty(loadPath))
            {
                if (!loadPath.Contains(DeviceStr))
                {
                    loadPath = $"{loadPath}\\{DeviceStr}\\";
                }

                path = loadPath;
            }

            // 动态加载驱动程序集
            foreach (var item in Directory.GetFiles(path, "*.dll"))
            {
                var fileName = Path.GetFileName(item);
                if (fileName.StartsWith(StartDll))
                {
                    var assemblyName = AssemblyName.GetAssemblyName(item);
                    AppDomain.CurrentDomain.Load(assemblyName);
                }
            }
        }

        /// <summary>
        /// 所有真实设备类型
        /// </summary>
        /// <returns></returns>
        public List<Type> GetRealTypes()
        {
            if (RdeviceTypes.Count == 0)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.GetName().Name.StartsWith(StartDll))
                    {
                        RdeviceTypes.AddRange(GetTypesByAssembly<IDevice>(assembly));
                    }
                }
            }

            return RdeviceTypes;
        }

        /// <summary>
        /// 获取所有虚拟设备类型
        /// </summary>
        /// <returns></returns>
        public List<Type> GetVirtualTypes()
        {
            List<Type> vTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetName().Name.StartsWith("Luster.Motion.DataStruct"))
                {
                    vTypes.AddRange(GetTypesByAssembly<IVirtualDevice>(assembly));
                }
            }

            return vTypes;
        }

        /// <summary>
        /// 保存设备配置
        /// </summary>
        /// <param name="saveTask">任务保存路径</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Save(string saveTask = "")
        {
            XElement xRoot = new XElement("Devices");
            foreach (var item in deviceList)
            {
                xRoot.Add(item.ExportXml());
            }

            foreach (var item in virtualDevices.OrderBy(u => u.Sort))
            {
                xRoot.Add(item.ExportXml());
            }

            // 轴安全区域
            XElement xSafe = new XElement("SafeRegion");
            var vAxises = GetDevices(typeof(VAxis));
            foreach (var item in vAxises)
            {
                if (item is VAxis vAxis && vAxis is IPosition pos && pos.GetSafeRegions().Count > 0)
                {
                    var safeModel = new SafeModel(vAxis);
                    var xSafeItem = safeModel.ExportXml();
                    xSafe.Add(xSafeItem);
                }
            }
            xRoot.Add(xSafe);

            // 点位安全区域
            XElement xPosionSafe = new XElement("PosionSafeRegion");
            var vAxisesAll = GetDevices(typeof(VAxis));
            foreach (var item in vAxisesAll)
            {

                if (item is VAxis vAxis && vAxis is IPosition pos && vAxis.Positions.Count > 0)
                {
                    foreach (var posion in vAxis.Positions)
                    {
                        var posionSafeModel = new PosionSafeModel(vAxis, posion);
                        var xPosionSafeItem = posionSafeModel.ExportXml();
                        xPosionSafe.Add(xPosionSafeItem);
                    }
                }
            }

            xRoot.Add(xPosionSafe);

            // 保存点位配置
            SavePosGroup(xRoot);

            //保存模组名
            SaveModuleNameGroup(xRoot);

            // 如果没有给出提示，那么使用默认打开的配置文件
            if (string.IsNullOrEmpty(saveTask))
            {
                saveTask = this.deviceTask;
            }

            if (!string.IsNullOrEmpty(saveTask))
            {
                // 保存对象
                xRoot.Save(saveTask);
            }

            // 通知订阅者保存各自的数据
            SaveEvent?.Invoke();

            // 保存成功
            IsNeedSave = false;
        }

        /// <summary>
        /// 获取所有模块（模块来源通过IO表导入，如果没有只返回System）
        /// </summary>
        /// <returns></returns>
        public List<string> GetModules()
        {
            //var devices = GetDevices();
            //if (devices == null || devices.Count == 0)
            //{
            //    return new List<string>() { "System" };
            //}
            //else
            //{
            //    return devices.GroupBy(u => u.Module).Select(u => u.Key).ToList();
            //}

            if (ModuleNameGroup.Count == 0)
            {
                return new List<string>() { "System" };
            }
            else
            {
                return ModuleNameGroup.Select(u => u.Name).ToList();
            }
        }

        public List<string> GetModulesUsed()
        {
            var devices = GetDevices();
            if (devices == null || devices.Count == 0)
            {
                return new List<string>() { "System" };
            }
            else
            {
                return devices.GroupBy(u => u.Module).Select(u => u.Key).ToList();
            }
        }

        #region 真实设备增删改
        /// <summary>
        /// 新增设备
        /// </summary>
        /// <param name="device"></param>
        public void AddDevice(IDevice device)
        {
            device.LogEvent -= OnLog;
            device.LogEvent += OnLog;

            deviceList.Add(device);
            IsNeedSave = true;
        }

        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="deviceID"></param>
        public void ReomoveDevice(Guid deviceID)
        {
            if (!CheckDeviceIsUsed(deviceID))
            {
                var device = GetDeviceByID(deviceID);
                if (device != null)
                {
                    device.Dispose();
                }

                device.LogEvent -= OnLog;
                deviceList.Remove(device);
                IsNeedSave = true;
            }
        }

        private bool CheckDeviceIsUsed(Guid deviceID)
        {
            var vDevice = virtualDevices.Where(x => x.DeviceID == deviceID).ToList();
            if (vDevice.Count > 0)
            {
                throw new FriendlyException("此设备正在被使用，无法删除");
            }

            var vCameras = virtualDevices.Where(x => x.GetType() == typeof(VCamera)).ToList();
            if (vCameras.Count > 0)
            {
                var light = vCameras.Where(x => (x as VCamera).LightController.ID == deviceID).ToList();
                if (light.Count > 0)
                {
                    throw new FriendlyException("此设备正在被使用，无法删除");
                }
            }
            return false;
        }

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDevice GetDeviceByID(Guid deviceID)
        {
            return deviceList.FirstOrDefault(x => x.ID == deviceID);
        }

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDevice GetDeviceByName(string name)
        {
            return deviceList.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// 查询真实设备列表
        /// </summary>
        /// <param name="virtualType"></param>
        /// <returns></returns>
        public List<IDevice> GetRealDevices(Type virtualType = null)
        {
            var list = deviceList;
            if (virtualType != null)
            {
                list = list.Where(u => virtualType.IsAssignableFrom(u.GetType())).ToList();
            }

            return list;
        }
        #endregion

        #region 虚拟设备增删改
        /// <summary>
        /// 新增设备
        /// </summary>
        /// <param name="virDevice">虚拟设备</param>
        /// <param name="setReal">是否设置对应的真实设备，如果在初始化的时候，就不进行设置真实设备，因为此时在离线状态，所以不需要配置真实设备</param>
        public void AddVirtual(IVirtualDevice virDevice, bool setReal = true)
        {
            // 设置引擎
            virDevice.Engine = this;

            // 更新设备的状态
            virDevice.Mode = DeviceMode;

            virDevice.LogEvent -= OnLog;
            virDevice.LogEvent += OnLog;
            virDevice.PropertyChanged -= VirDevice_PropertyChanged;
            virDevice.PropertyChanged += VirDevice_PropertyChanged;

            // 针对所有虚拟设备和真实设备进行关联
            if (virDevice.DeviceID != Guid.Empty && setReal)
            {
                var device = GetDeviceByID(virDevice.DeviceID);

                if (!virDevice.SetDevice(device, out string msg))
                {
                    OnLog(LogType.Error, $"{virDevice.GetType().Name} {virDevice.Name} 未找到对应的真实设备!");
                }
            }

            // 配置对象引用
            if (virDevice is IReference refObj)
            {
                var type = virDevice.GetType();
                foreach (var item in refObj.GetRefObjs())
                {
                    item.ByUse = virDevice;
                }
            }

            virtualDevices.Add(virDevice);

            IsNeedSave = true;

            VDeviceChangedEvent?.Invoke();
        }

        public event Action<Guid, string, string> DeviceNameChangedEvent;

        private void VirDevice_PropertyChanged(IVirtualDevice vDevice, string propName, object srcV, object newV)
        {
            // 设备状态变更不在记录
            if (propName != nameof(VStatus))
            {
                OnLog(LogType.Info, $"设备{vDevice.Name}的属性:{propName}值发生变更,原始值:{srcV} 新值:{newV}");
            }

            if (propName == "Name")
            {
                DeviceNameChangedEvent?.Invoke(vDevice.ID, srcV?.ToString(), newV?.ToString());
            }

            if (propName != nameof(VStatus))
                IsNeedSave = true;
        }

        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="deviceID"></param>
        public void ReomoveVirtual(params Guid[] deviceIDs)
        {
            foreach (var item in deviceIDs)
            {
                var vDevice = GetVirtualByID(item);

                if (vDevice == null) continue;

                // 检查对象是否被其他对象引用
                if (vDevice.ByUse != null && !deviceIDs.Contains(vDevice.ByUse.ID))
                {
                    throw new FriendlyException($"设备:{vDevice.Name}被设备:{vDevice.ByUse.Name} 引用,无法删除!");
                }

                // 确认对象是否被模块引用
                string outMsg = PrevDeleteEvent?.Invoke(vDevice);
                if (!string.IsNullOrEmpty(outMsg))
                {
                    throw new FriendlyException($"设备:{vDevice.Name}被模块:{outMsg} 引用,无法删除!");
                }

                virtualDevices.Remove(vDevice);
                if (vDevice is IReference refObj)
                {
                    foreach (var refProp in refObj.GetRefObjs())
                    {
                        refProp.ByUse = null;
                    }
                }
            }

            IsNeedSave = true;
            VDeviceChangedEvent?.Invoke();
        }

        /// <summary>
        /// 通过类型移除设备
        /// </summary>
        /// <param name="deviceID"></param>
        public void ReomoveVirtual(Type vType)
        {
            var vDevices = GetDevices(vType);

            foreach (var vDevice in vDevices)
            {
                if (vDevice == null) continue;

                // 检查对象是否被其他对象引用
                if (vDevice.ByUse != null && !vDevices.Contains(vDevice.ByUse))
                {
                    throw new FriendlyException($"设备:{vDevice.Name}被设备:{vDevice.ByUse.Name} 引用,无法删除!");
                }

                // 确认对象是否被模块引用
                string outMsg = PrevDeleteEvent?.Invoke(vDevice);
                if (!string.IsNullOrEmpty(outMsg))
                {
                    throw new FriendlyException($"设备:{vDevice.Name}被模块:{outMsg} 引用,无法删除!");
                }

                virtualDevices.Remove(vDevice);

                if (vDevice is IReference refObj)
                {
                    foreach (var refProp in refObj.GetRefObjs())
                    {
                        refProp.ByUse = null;
                    }
                }
            }

            IsNeedSave = true;
            VDeviceChangedEvent?.Invoke();
        }

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IVirtualDevice GetVirtualByID(Guid id)
        {
            return virtualDevices.FirstOrDefault(x => x.ID == id);
        }

        /// <summary>
        /// 通过名称获取设备
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IVirtualDevice GetVirtualByName(string name)
        {
            return virtualDevices.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// 获取虚拟设备列表
        /// </summary>
        /// <param name="virtualType"></param>
        /// <returns></returns>
        public List<IVirtualDevice> GetDevices(Type virtualType = null, string module = "")
        {
            var list = virtualDevices;
            if (virtualType != null && module == "")
            {
                list = list.Where(u => virtualType.IsAssignableFrom(u.GetType())).ToList();
            }
            if (virtualType != null && module != "")
            {
                list = list.Where(u => u.Module == module).ToList();
                list = list.Where(u => virtualType.IsAssignableFrom(u.GetType())).ToList();
            }
            return list;
        }

        public List<T> GetVDevices<T>(string module = null)
        {
            var list = virtualDevices;
            if (!string.IsNullOrEmpty(module))
            {
                list = list.Where(u => u.Module == module).ToList();
            }


            return list.Where(u => typeof(T).IsAssignableFrom(u.GetType())).Cast<T>().ToList();
        }

        /// <summary>
        /// 加载LOG
        /// </summary>
        /// <param name="total"></param>
        /// <param name="cur"></param>
        /// <param name="message"></param>
        /// <param name="func"></param>
        private void OnLoadingLog(int total, int cur, string message, Func<bool> func = null)
        {
            if (func != null && func() == false)
            {
                return;
            }

            LoadingEvent?.Invoke(total, cur, message);
        }

        /// <summary>
        /// 设置设备的运行状态
        /// </summary>
        /// <param name="mode">新的模式</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>是否切换成功</returns>
        public bool SetEngineMode(DeviceMode mode, out string errMsg)
        {
            //PrevModeChangeEvent?.Invoke(mode);

            bool flag = false;
            errMsg = "";

            // 如果当前模式和要切换的模式不同，则显示LOG信息
            bool isShowLog = mode != DeviceMode;

            // 初始化真实设备
            if (mode == DeviceMode.Real || mode == DeviceMode.Empty)
            {
                var devices = GetRealDevices();

                // +1 的目的是将虚拟设备初始化统一放到一个状态中
                int count = devices.Count + 1;
                int curVal = 0;

                try
                {
                    PrevModeChangeEvent?.Invoke(false);
                    foreach (var item in devices)
                    {
                        // 触发进度条加载LOG信息
                        OnLoadingLog(count, curVal, $"厂商:{item.Brand} 设备:{item.Name} 初始化..", () => isShowLog);

                        //光源暂不初始化，目前光源IO居多
                        if (item.DeviceType == DeviceType.LightController) continue;

                        // API 初始化
                        item.InitApi();

                        if (item is IMotionCard m)
                        {
                            m.ClearEmg();
                        }

                        // 打开设备
                        item.Open();

                        // 使能
                        item.SetEnabled(true);

                        curVal++;

                        OnLoadingLog(count, curVal, $"厂商:{item.Brand} 设备:{item.Name} 初始化完成", () => isShowLog);
                    }

                    // 仿真设备初始化
                    OnLoadingLog(count, curVal, $"设备初始化,设备模式:{DeviceMode}->{mode}", () => isShowLog);
                    Guid deviceID = Guid.Empty;
                    IDevice device = null;

                    // 更新虚拟设备状态
                    foreach (var item in GetDevices())
                    {
                        if (deviceID != item.DeviceID)
                        {
                            deviceID = item.DeviceID;
                            device = GetDeviceByID(deviceID);
                        }

                        item.Mode = mode;

                        // 关联设备状态
                        if (device != null)
                        {
                            if (!item.SetDevice(device, out errMsg))
                            {
                                OnLog(LogType.Error, errMsg);
                                break;
                            }

                            if (item.ByUse != null)
                            {
                                item.ByUse.Mode = mode;
                            }
                        }
                    }

                    StartDeviceMonitor();

                    // 发现模式变更后，才触发事件
                    flag = true;
                }
                catch (Exception ex)
                {
                    // 如果失败，重新断开轴连接
                    foreach (var item in devices)
                    {
                        item.Dispose();
                    }

                    errMsg = ex.Message;
                }
                finally
                {
                    PrevModeChangeEvent?.Invoke(true);
                    // 保障进度条进度结束正确
                    OnLoadingLog(count, count, $"仿真设备完成", () => isShowLog);
                }
            }
            else
            {
                // 关闭设备监控
                StopDeviceMonitor();

                // 关闭设备
                Dispose();

                foreach (var item in GetDevices())
                {
                    item.Mode = mode;
                }

                flag = true;
            }

            // 切换成功，才更新当前的设备模式
            if (flag)
            {
                DeviceMode = mode;
                if (isShowLog)
                {
                    ModeChangedEvent?.Invoke(mode);
                }
            }

            return flag;
        }

        /// <summary>
        /// 设备监控
        /// </summary>
        private CancellationTokenSource montorSource;

        /// <summary>
        /// 关闭
        /// </summary>
        public void StopDeviceMonitor()
        {
            // 如果停止监控，先将循环中等待结束掉
            pauseReset?.Set();

            // 停止线程信息
            if (montorSource != null)
            {
                montorSource.Cancel();
            }

            // 等待延时
            Thread.Sleep(200);
        }

        public void StartDeviceMonitor()
        {
            StopDeviceMonitor();

            montorSource = new CancellationTokenSource();
            //var mIos = GetVDevices<VIO>().Where(u => u.Montor != IOMonitor.None).ToList();

            //var hpDevices = GetVDevices<IMaintain>();
            //OnLog(LogType.Info, $"参与寿命监控的设备:{hpDevices.Count}");

            //if (mIos.Count > 0)
            //{
            //    // 开启设备监控
            //    Task.Factory.StartNew(() =>
            //    {
            //        OnLog(LogType.Info, $"安全门 线程:{Thread.CurrentThread.Name} 启动监听");
            //        while (true)
            //        {
            //            if (montorSource.IsCancellationRequested)
            //            {
            //                OnLog(LogType.Info, $"安全门 线程:{Thread.CurrentThread.Name} 被终止");
            //                break;
            //            }

            //            if (pauseReset != null && !pauseReset.IsSet)
            //            {
            //                OnLog(LogType.Debug, $"安全门监控暂停中..");
            //                pauseReset.Wait();
            //                OnLog(LogType.Debug, $"安全门监控恢复..");
            //            }

            //            // 安全IO检查
            //            CheckSafeIOs(mIos);

            //            // 针对设备里程进行监控,这边持续检测，会导致刷屏
            //            //CheckDeviceHP(hpDevices);

            //            // 等待300 ms 监控设备状态
            //            for (int i = 0; i < 300; i += 50)
            //            {
            //                if (montorSource.IsCancellationRequested)
            //                {
            //                    break;
            //                }

            //                Thread.Sleep(50);
            //            }
            //        }
            //    }, montorSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            //}

            StartMonitor();
        }

        /// <summary>
        /// 监控设备的HP
        /// </summary>
        private void CheckDeviceHP(List<IMaintain> maintains)
        {
            foreach (var item in maintains)
            {
                if (item.MaintainHP == 0) continue;

                if (item.CurrentHP > item.MaintainHP)
                {
                    var alarm = new AlarmInfo(this, AlarmType.InfoTip, item.GetMaintainTips(), DeviceError.HPMatain.ToString(), "")
                    {
                        DeviceID = item.ID.ToString(),
                    };

                    OnAlarm(alarm);
                }
            }
        }

        /// <summary>
        /// 安全
        /// </summary>
        /// <returns></returns>
        private bool CheckSafeIOs(List<VIO> mIos)
        {
            List<IVirtualDevice> alarmDevices = new List<IVirtualDevice>();

            foreach (var item in mIos)
            {
                bool checkVal = bool.Parse(item.Montor.ToString());
                if (item.GetDigitalIn() != checkVal)
                {
                    var alarmInfo = new AlarmInfo(this, AlarmType.WarningTip, $"安全门:{item.Name}打开报警", DeviceError.SafeDoorFail.ToString());
                    AlarmEvent_device?.Invoke(alarmInfo);
                }
            }

            return alarmDevices.Count == 0;
        }

        /// <summary>
        /// 通信监视
        /// </summary>
        public void StartMonitor()
        {
            var montors = GetVDevices<IMonitor>();

            Task.Factory.StartNew(() =>
            {
                OnLog(LogType.Info, $"方法StartMonitor 线程:{Thread.CurrentThread.Name} 启动监听");
                while (true)
                {
                    if (montorSource.IsCancellationRequested)
                    {
                        OnLog(LogType.Info, $"方法StartMonitor 线程:{Thread.CurrentThread.Name} 被终止");
                        break;
                    }

                    foreach (var monitor in montors)
                    {
                        try
                        {
                            if (monitor.IsMonitor)
                            {
                                monitor.StartMonitor();
                            }
                        }
                        catch (Exception ex)
                        {
                            var alarmInfo = new AlarmInfo(monitor, AlarmType.WarningTip, ex.Message, monitor.AlarmCode);
                            AlarmEvent_device?.Invoke(alarmInfo);
                        }
                        finally
                        {
                            // 监控等待
                            for (int i = 0; i < monitor.Frequency; i += 20)
                            {
                                if (montorSource.IsCancellationRequested)
                                {
                                    break;
                                }

                                SpinWait.SpinUntil(() => false, 20);

                                //Thread.Sleep(20);
                            }
                        }
                    }
                }
            }, montorSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// 对象释放
        /// </summary>
        public void Dispose()
        {
            if (DeviceMode == DeviceMode.Real)
            {
                foreach (var item in GetRealDevices())
                {
                    item.Dispose();
                    item.Status = DeviceStatus.Offline;
                }
            }

            foreach (var item in GetDevices())
            {
                if (item is IDisposable dis)
                {
                    dis.Dispose();
                }
            }

            // 对象释放
            montorSource?.Cancel();
        }
        #endregion

        #region 设备控制
        /// <summary>
        /// 对所有设备进行回零,设备耗时严重，需要异步线程调用
        /// </summary>
        /// <param name="errMsg">异常信息</param>
        /// <returns>回零成功与否</returns>
        public bool Home(out string errMsg)
        {
            pauseReset?.Set();
            errMsg = "";

            // 1.先中断所有IO循环等待
            GetDevices().ForEach(u => u.IsBreak = true);

            // 2.默认回零成功
            bool isOk = true;

            // 3.获取所有回零对象
            var others = GetDevices()
                        .Where(u => u is IHome)
                        .Select(u => u as IHome)
                        .OrderBy(u => u.HomeSort)
                        .ToList();

            // 4 是将虚拟设备IsStop状态调整为False
            int total = others.Count;

            // 5.回零时将设备调整为在线状态
            foreach (var item in GetRealDevices())
            {
                item.Status = DeviceStatus.Ready;
            }

            // 6.对所有实现IHome接口的设备进行回零
            foreach (var item in others)
            {
                try
                {
                    if (item.NeedHome)
                    {
                        OnLog(LogType.Debug, $"回零项：{item.Name},回零开始");
                        item.Home();
                        OnLog(LogType.Debug, $"回零项：{item.Name},回零结束");
                    }

                    // 通讯不能连接的太快
                    if (item is IHeartbeat)
                    {
                        Thread.Sleep(30);
                    }
                }
                catch (Exception ex)
                {
                    isOk = false;
                    errMsg = $"{item.Name} 回零失败! 异常:{ex.Message}";
                    break;
                }
            }

            // 等待50ms，防止循环没有中断掉的bug
            Thread.Sleep(50);

            // 还原中断状态,如果放在func后的场景下造成等待类的程序无法运动
            GetDevices().ForEach(u => u.IsBreak = false);

            return isOk;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            // 1.先中断所有IO循环等待
            GetDevices().ForEach(u =>
            {
                u.IsBreak = true;

                // 连接中断
                if (u is VCommuncation vComm)
                {
                    vComm.Stop();
                }
            });

            // 2.停止真实设备，放在后面的目的，防止还为暂停，Move方法就结束了
            foreach (var item in GetRealDevices())
            {
                item.Status = DeviceStatus.Stop;
            }

            // 3.还原中断状态
            Thread.Sleep(100); // 防止内部循环等待的时间过长，最终给IsBreak又还原了
            GetDevices().ForEach(u => u.IsBreak = false);

            // 4.结束流程,如果取消了阻塞，那么安全门会一直报警
            //pauseReset?.Set();
        }

        /// <summary>
        /// 是否在回零状态
        /// </summary>
        /// <returns></returns>
        public bool IsHome(out string errMsg)
        {
            bool isHome = true;
            errMsg = "";
            var axises = GetDevices(typeof(VAxis)).Where(u => u is not VAxisM && u.ByUse == null).ToList();
            foreach (var item in axises)
            {
                if (item is VAxis v)
                {
                    v.CheckIsHome();
                }
            }

            return isHome;
        }

        public bool IsBreakAction { get; set; }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void Pause()
        {
            pauseReset?.Reset();
            //所有设备停止计时
            //GetDevices().ForEach(u =>
            //{
            //    u.IsBreak = true;
            //});
            GetDevices().ForEach(u => u.IsPause = true);

        }

        /// <summary>
        /// 进行监控复位
        /// </summary>
        public bool Recovery()
        {
            // 1.获取所有IOs
            var mIos = GetVDevices<VIO>().Where(u => u.Montor != IOMonitor.None).ToList();
            //var mIos = GetVDevices<VIO>().Where(u => u.Montor == IOMonitor.True).ToList();
            OnLog(LogType.Info, "获取所有IOs完成");
            // 2.尝试安全检查
            if (!CheckSafeIOs(mIos))
            {
                return false;
            }
            OnLog(LogType.Info, "安全检查完成");
            // 检查安全是否还存在安全
            pauseReset?.Set();
            OnLog(LogType.Info, "pauseReset完成");
            GetDevices().ForEach(u => u.IsPause = false);
            OnLog(LogType.Info, "GetDevices完成");
            return true;
        }
        #endregion

        #region 点位删除
        public void RemoveAxisPos(AxisPosition axisPos)
        {
            if (axisPos.Axis == null)
            {
                throw new FriendlyException($"点位:{axisPos.Name}的轴不存在!");
            }

            string errMsg = AxisPosDeleteEvent?.Invoke(axisPos);
            if (!string.IsNullOrEmpty(errMsg))
            {
                throw new FriendlyException(errMsg);
            }

            // 删除
            axisPos.Axis.RemovePostion(axisPos.Name);

            IsNeedSave = true;
        }

        /// <summary>
        /// 通过点位KEY获取点位
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AxisPosition GetAxisPosition(Guid key)
        {
            AxisPosition axisPos = null;
            var vAxises = GetVDevices<VAxis>();
            foreach (var item in vAxises)
            {
                foreach (var pItem in item.Positions)
                {
                    if (pItem.Key == key)
                    {
                        axisPos = pItem;
                        break;
                    }
                }
            }

            return axisPos;
        }

        public void UpdatePostion(AxisPosition pos, double newV)
        {
            if (pos.Position != newV)
            {
                PropertyChangedEvent?.Invoke($"{pos.Axis.Name}_{pos.Name}", pos.Position, newV);
                OnLog(LogType.Debug, $"点位:{pos.Name} 值发生变更,旧值:{pos.Position}->{newV}");
                pos.Position = newV;
            }
        }

        #endregion

        #region 多点位示教组
        /// <summary>
        /// 点位组
        /// </summary>
        public List<VAxisPosGroup> PosGroup { get; set; } = new List<VAxisPosGroup>();
        /// <summary>
        /// 模块配置路径
        /// </summary>
        public string ModuleConfigPath { get; set; }
        public string RecipeConfigPath { get; set; }
        public string RecipeSlnPath { get; set; }

        /// <summary>
        /// 示教点位
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vAxis"></param>
        public void TeachPosGroup(string name, string module, params VAxis[] vAxis)
        {
            if (PosGroup.Any(u => u.Name == name && u.Module == module))
            {
                throw new FriendlyException($"点位:{name} 已存在");
            }

            // 构建点位组
            VAxisPosGroup pGroup = new VAxisPosGroup()
            {
                Key = Guid.NewGuid(),
                Name = name,
                Module = module,
                DeviceEngine = this,
            };

            foreach (var item in vAxis)
            {
                pGroup.Add(new AxisPosition()
                {
                    Axis = item,
                    AxisNo = item.AxisNo,
                    Key = Guid.NewGuid(),
                    Name = $"{item.AxisType}_{name}",
                    Position = item.GetCurrentPos(),
                    MovePriority = item.MovePriority,
                });
            }

            PosGroup.Add(pGroup);
            IsNeedSave = true;
        }

        /// <summary>
        /// 删除点位
        /// </summary>
        /// <param name="name">删除指定示教位</param>
        public void RemovePosGroup(string name)
        {
            PosGroup.RemoveAll(u => u.Name == name);
            IsNeedSave = true;
        }

        /// <summary>
        /// 更新轴点位
        /// </summary>
        /// <param name="pGroup"></param>
        /// <param name="aType"></param>
        /// <param name="position"></param>
        public void UpdatePosGroup(VAxisPosGroup pGroup, AxisType aType, double position)
        {
            var pos = pGroup.FirstOrDefault(u => u.Axis.AxisType == aType);
            if (pos == null)
            {
                throw new FriendlyException($"轴类型:{aType} 不存在");
            }

            if (pos.Position != position)
            {
                PropertyChangedEvent?.Invoke($"{pos.Name}", pos.Position, position);
                OnLog(LogType.Debug, $"点位:{pGroup.Name}的{aType}值发生变更,旧值:{pos.Position}->{position}");
                pos.Position = position;
            }

            IsNeedSave = true;
        }

        public void UpdatePosGroup(AxisPosition pPos, double position)
        {
            // 只在首次使用时进行分割
            var posNameItems = pPos.Name.Split('_');
            if (posNameItems.Length < 2 || posNameItems[1] == null)
            {
                throw new FriendlyException($"点位名称格式不正确: {pPos.Name}");
            }

            var group = PosGroup.FirstOrDefault(u => u.Module == pPos.Axis.Module && u.Name == posNameItems[1]);
            if (group == null)
            {
                throw new FriendlyException($"点位组:{posNameItems[1]} 不存在");
            }

            var pos = group.FirstOrDefault(u => u.Name == pPos.Name);
            if (pos == null)
            {
                throw new FriendlyException($"点位:{pPos.Name} 不存在");
            }

            if (pos.Position != position)
            {
                PropertyChangedEvent?.Invoke($"{pos.Name}", pos.Position, position);
                OnLog(LogType.Debug, $"点位:{pos.Name}的值发生变更,旧值:{pos.Position}->{position}");
                pos.Position = position;
                IsNeedSave = true;
            }
        }

        /// <summary>
        /// 保存点位配置
        /// </summary>
        public void SavePosGroup(XElement xParent)
        {
            if (PosGroup.Count > 0)
            {
                XElement xGroup = new XElement(nameof(PosGroup));
                foreach (var item in PosGroup)
                {
                    xGroup.Add(item.ExportXml());
                }

                xParent.Add(xGroup);
            }
        }

        /// <summary>
        /// 加载点位配置
        /// </summary>
        public void LoadPosGroup(XElement xPosGroup)
        {
            if (xPosGroup == null) return;

            PosGroup.Clear();

            foreach (var xItem in xPosGroup.Elements())
            {
                VAxisPosGroup pGroup = new VAxisPosGroup();
                pGroup.DeviceEngine = this;
                pGroup.ParserXml(xItem);

                PosGroup.Add(pGroup);
            }
        }
        #endregion

        #region 模块信息组
        /// <summary>
        /// 点位集合
        /// </summary>
        public List<ModuleNameModel> ModuleNameGroup { get; set; } = new List<ModuleNameModel>();

        /// <summary>
        /// 添加模组
        /// </summary>
        /// <param name="name"></param>
        public void AddModuleNameGroup(string name)
        {
            if (ModuleNameGroup.Any(u => u.Name == name))
            {
                throw new FriendlyException($"模组名称:{name} 已存在");
            }

            // 构建点位组
            ModuleNameModel pModule = new ModuleNameModel()
            {
                Name = name,
            };

            ModuleNameGroup.Add(pModule);
            IsNeedSave = true;
        }

        /// <summary>
        /// 删除模组
        /// </summary>
        /// <param name="name"></param>
        public void RemoveModuleNameGroup(string name)
        {
            ModuleNameGroup.RemoveAll(u => u.Name == name);
            IsNeedSave = true;
        }

        /// <summary>
        /// 保存模组配置
        /// </summary>
        public void SaveModuleNameGroup(XElement xParent)
        {
            if (ModuleNameGroup.Count > 0)
            {
                XElement xModuel = new XElement(nameof(ModuleNameGroup));
                foreach (var item in ModuleNameGroup)
                {
                    xModuel.Add(item.ExportXml());
                }

                xParent.Add(xModuel);
            }
        }

        /// <summary>
        /// 加载模组配置
        /// </summary>
        public void LoadModuleNameGroup(XElement xModuleGroup)
        {
            if (xModuleGroup == null) return;

            ModuleNameGroup.Clear();

            foreach (var xItem in xModuleGroup.Elements())
            {
                ModuleNameModel moduleModel = new ModuleNameModel();
                moduleModel.ParserXml(xItem);

                ModuleNameGroup.Add(moduleModel);
            }
        }
        #endregion

        #region 维保
        /// <summary>
        /// 触发清零的事件
        /// </summary>
        public event Action<IMaintain> MaintainZeroEvent;

        /// <summary>
        /// 触发方法
        /// </summary>
        public void MaintainZero(IMaintain maintain) => MaintainZeroEvent?.Invoke(maintain);
        #endregion

        #region 页面权限
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="role"></param>
        public void OnUserLogin(SystemRole role) => RoleChangedEvent?.Invoke(role);

        /// <summary>
        /// 权限变更
        /// </summary>
        public event Action<SystemRole> RoleChangedEvent;
        public event Action<Guid, string, string> ControlValueChangedEvent;

        /// <summary>
        /// 获取当前机台状态
        /// </summary>
        /// <returns></returns>
        public void OnStatusChanged(EngineStatus src, EngineStatus dst)
        {
            StatusChangedEvent?.Invoke(src, dst);
        }

        /// <summary>
        /// 获取机台当前状态
        /// </summary>
        /// <returns></returns>
        public EngineStatus GetMachineStatus()
        {
            if (GetMachineStatusEvent == null)
            {
                return EngineStatus.Idle;
            }

            return GetMachineStatusEvent.Invoke();
        }
        #endregion


        #region
        //加载点位
        public List<XElement> LoadControlPara()
        {
            List<XElement> controlParas = new List<XElement>();
            XElement xRoot = XElement.Load(RecipeDataPath);
            foreach (var xItem in xRoot.Elements())
            {
                foreach (var p in xItem.Elements())
                {
                    var xType = p.Attribute("Type");
                    if (xType == null) continue;
                    if (xType.Value == "Bool" || xType.Value == "Int" || xType.Value == "Double")
                    {
                        controlParas.Add(p);
                    }
                }
            }
            return controlParas;
        }
        //更新控制参数,通过事件触发
        public void UpdateControlPara(Guid guid, string Name, string value)
        {
            ControlValueChangedEvent?.Invoke(guid, Name, value);
        }
        #endregion


        /// <summary>
        /// 获取所有模块（模块来源通过事件在MotionEngine中导入）
        /// </summary>
        /// <returns></returns>
        public List<object> GetModulesFromMotionEngine(string guid = null, string type = null)
        {
            var modules = new List<object>();
            try
            {
                modules = GetModuleListEvent?.Invoke(guid, type);
            }
            catch
            {
                modules = new List<object>();
            }

            return modules;
        }
        public IEnumerable<object> GetPDCAModulesFromMotionEngine()
        {
            try
            {
                return GetPDCAModulesEvent?.Invoke();
            }
            catch
            {
                return new List<object>();
            }
        }
        public IEnumerable<object> GetSFCModulesFromMotionEngine()
        {
            try
            {
                return GetSFCModulesEvent?.Invoke();
            }
            catch
            {
                return new List<object>();
            }
        }
    }

}