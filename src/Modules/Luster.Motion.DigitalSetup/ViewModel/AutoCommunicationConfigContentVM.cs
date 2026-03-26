using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.EditorUI;
using Luster.SimDevice.Adapter;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 通讯端口配置
    /// </summary>
    public class AutoCommunicationConfigContentVM : BaseAss
    {
        // 新增3个按钮和1个进度条的定义
        private double _progressValue;
        readonly IDeviceEngine _deviceEngine;
        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }
        public AutoCommunicationConfigContentVM(IRepository repository,
                                                IRegionManager regionManager, ICommonBus commonBus, CSVHelper cSVHelper, IDeviceEngine deviceEngine,FlowBus flowBus) : base(repository, regionManager, commonBus, cSVHelper, flowBus)
        {
            Pages = new ObservableCollection<CommonPageModel>();
            //注释电脑网卡配置页面，用不到，0718
            //Pages.Add(new CommonPageModel() { Name = "ConfigComputerNet", IsSelected = true, Region = "", ViewType = typeof(AssTbConfigComputerNet) });
            Pages.Add(new CommonPageModel() { Name = "ConfigSoftwareNet", IsSelected = true, Region = "", ViewType = typeof(AssTbConfigSoftwareNet) });
            Pages.Add(new CommonPageModel() { Name = "ConfigSoftwareCom", IsSelected = true, Region = "", ViewType = typeof(AssTbConfigSoftwareCom) });
            //Pages.Add(new CommonPageModel() { Name = "CommunicationTest", IsSelected = true, Region = "", ViewType = typeof(AssTbCommunicationTest) });
            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            _deviceEngine = deviceEngine;
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);
        }

        private void OnUpdateItems()
        {
            ItemModels.Clear();
            var Commus = _deviceEngine.GetDevices(typeof(VCommuncation));
            try
            {
                switch (SelectedReportPage.Name)
                {
                    case "ConfigComputerNet":

                        break;
                    case "ConfigSoftwareNet":
                        long totalCount = 0;
                        var items = _csvHelper.GetAllDataNew1<AssTbConfigSoftwareNet>(0, 0, out totalCount);
                        foreach (var item in items)
                        {
                            item.实测 = "";
                            item.状态 = "";
                            ItemModels.Add(item);
                        }
                        break;
                    case "ConfigSoftwareCom":
                        long comTotalCount = 0;
                        var comItems = _csvHelper.GetAllDataNew1<AssTbConfigSoftwareCom>(0, 0, out comTotalCount);
                        foreach (var item in comItems)
                        {
                            item.实测 = "";
                            item.状态 = "";
                            ItemModels.Add(item);
                        }
                        break;
                }

            }
            catch (Exception)
            {

            }
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                ProgressValue = 0;
                if (SelectedReportPage == null || ItemModels == null || ItemModels.Count == 0)
                    return;

                int count = ItemModels.Count;
                int processedCount = 0;

                // 顺序处理每个项目（一个接一个）
                foreach (var item in ItemModels)
                {
                    // 检查取消请求
                    token.ThrowIfCancellationRequested();

                    // 执行单个任务
                    await Task.Run(() =>
                    {
                        Check_ComputerNet_Single(item);
                    }, token);

                    // 更新进度
                    processedCount++;
                    ProgressValue = processedCount * 100 / count;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {

            }
        }

        // 修改后的 Check_ComputerNet_Single 方法
        private void Check_ComputerNet_Single(object item)
        {
            if (item == null || SelectedReportPage == null)
                return;

            switch (SelectedReportPage.Name)
            {
                case "ConfigComputerNet":
                    if (item is AssTbConfigComputerNet computerNet)
                    {
                        if (string.IsNullOrWhiteSpace(computerNet.项次) || string.IsNullOrWhiteSpace(computerNet.标准))
                        {
                            computerNet.状态 = "格式错误";
                            return;
                        }
                        string[] stdParts = computerNet.标准.Split(new[] { '/', '\\', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (stdParts.Length < 2)
                        {
                            computerNet.状态 = "格式错误";
                            return;
                        }
                        string stdIp = stdParts[0].Trim();
                        string stdPort = stdParts[1].Trim();
                        var nics = NetworkInterface.GetAllNetworkInterfaces();
                        string foundIp = null;
                        foreach (var nic in nics)
                        {
                            if (nic.Name.Equals(computerNet.项次, StringComparison.OrdinalIgnoreCase))
                            {
                                var ipProps = nic.GetIPProperties();
                                var addr = ipProps.UnicastAddresses
                                    .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                                if (addr != null)
                                {
                                    foundIp = addr.Address.ToString();
                                    computerNet.实测 = foundIp + "/" + stdPort;
                                    break;
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(foundIp))
                        {
                            computerNet.状态 = "NG";
                            return;
                        }
                        if (foundIp == stdIp)
                        {
                            bool portOk = commonHelpers.Ping(stdIp, stdPort);
                            computerNet.状态 = portOk ? "OK" : "NG";
                        }
                        else
                        {
                            computerNet.状态 = "NG";
                        }
                    }
                    break;
                case "ConfigSoftwareNet":
                    try
                    {
                        if (item is AssTbConfigSoftwareNet softwareNet)
                        {
                            // 1. 获取和验证基础数据 (无需UI线程)
                            string itemName = softwareNet.项次;
                            string standard = softwareNet.标准;

                            if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(standard))
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { softwareNet.状态 = "格式错误"; });
                                return;
                            }

                            string[] stdParts = standard.Split(new[] { '/', '\\', ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (stdParts.Length < 2)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { softwareNet.状态 = "格式错误"; });
                                return;
                            }
                            string stdIp = stdParts[0].Trim();
                            string stdPort = stdParts[1].Trim();

                            // 2. 访问设备引擎和更新UI需要切回主线程
                            string actualConfig = null;
                            bool deviceFound = false;

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (_deviceEngine == null) return;
                                var Commus = _deviceEngine.GetDevices(typeof(VCommuncation));
                                if (Commus != null)
                                {
                                    var targetDevice = Commus.OfType<VCommuncation>().FirstOrDefault(d => d.Name == itemName);
                                    if (targetDevice != null && targetDevice.Communication != null)
                                    {
                                        deviceFound = true;
                                        actualConfig = targetDevice.Communication.ToString();
                                        softwareNet.实测 = actualConfig;
                                    }
                                    else
                                    {
                                         if (targetDevice == null) softwareNet.实测 = "设备未找到";
                                         else softwareNet.实测 = "配置为空";
                                    }
                                }
                            });

                            if (!deviceFound)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { softwareNet.状态 = "NG"; });
                                return;
                            }

                            // 3. 比较逻辑 (无需UI线程)
                            if (string.IsNullOrEmpty(actualConfig)) actualConfig = "";
                            
                            bool isMatch = false;
                            string[] actualParts = actualConfig.Split(new[] { '/', '\\', ':' }, StringSplitOptions.RemoveEmptyEntries);
                            
                            if (actualParts.Length >= 2)
                            {
                                if (actualParts[0].Trim() == stdIp && actualParts[1].Trim() == stdPort)
                                {
                                    isMatch = true;
                                }
                            }

                            if (!isMatch)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { softwareNet.状态 = "NG"; });
                            }
                            else
                            {
                                // 4. Ping测试 (耗时操作，保持在Task线程)
                                bool portOk = commonHelpers.Ping(stdIp, stdPort);
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { softwareNet.状态 = portOk ? "OK" : "NG"; });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                         System.Windows.Application.Current.Dispatcher.Invoke(() => 
                         {
                             if (item is AssTbConfigSoftwareNet sn)
                             {
                                 sn.状态 = "Error";
                                 sn.实测 = "异常"; // 简化错误信息防止字符异常
                             }
                         });
                    }
                    break;
                case "ConfigSoftwareCom":
                    try
                    {
                        if (item is AssTbConfigSoftwareCom comItem)
                        {
                            // 1. 获取和验证基础数据
                            string itemName = comItem.项次;
                            string standard = comItem.标准;

                            if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(standard))
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { comItem.状态 = "格式错误"; });
                                return;
                            }

                            // 2. 从仿真通信获取设备信息
                            string actualConfig = null;
                            bool deviceFound = false;

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (_deviceEngine == null) return;
                                var Commus = _deviceEngine.GetDevices(typeof(VCommuncation));
                                if (Commus != null)
                                {
                                    var targetDevice = Commus.OfType<VCommuncation>().FirstOrDefault(d => d.Name == itemName);
                                    if (targetDevice != null && targetDevice.Communication != null)
                                    {
                                        deviceFound = true;
                                        actualConfig = targetDevice.Communication.ToString();
                                        comItem.实测 = actualConfig;
                                    }
                                    else
                                    {
                                         if (targetDevice == null) comItem.实测 = "设备未找到";
                                         else comItem.实测 = "配置为空";
                                    }
                                }
                            });

                            if (!deviceFound)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { comItem.状态 = "NG"; });
                                return;
                            }

                            // 3. 比较标准和实测是否一致
                            if (string.IsNullOrEmpty(actualConfig)) actualConfig = "";
                            bool isMatch = standard.Trim() == actualConfig.Trim();

                            if (!isMatch)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => { comItem.状态 = "NG"; });
                                return;
                            }

                            // 4. 一致，进行串口诊断
                            System.Windows.Application.Current.Dispatcher.Invoke(() => { CheckSerialPortStatus(comItem); });
                        }
                    }
                    catch (Exception ex)
                    {
                         System.Windows.Application.Current.Dispatcher.Invoke(() => 
                         {
                             if (item is AssTbConfigSoftwareCom ci)
                             {
                                 ci.状态 = "Error";
                                 ci.实测 = "异常";
                             }
                         });
                    }
                    break;
            }
        }

        /// <summary>
        /// 检查串口状态核心逻辑
        /// </summary>
        /// <param name="comItem">串口配置项</param>
        private void CheckSerialPortStatus(AssTbConfigSoftwareCom comItem)
        {
            try
            {
                // 参数解析部分
                string std = comItem.标准;
                if (string.IsNullOrWhiteSpace(std))
                {
                    comItem.状态 = "格式错误";
                    return;
                }

                string[] parts = std.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                {
                    parts = std.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 5)
                    {
                        comItem.状态 = "格式错误";
                        comItem.实测 = "参数格式无法解析";
                        return;
                    }
                }

                string portName = parts[0].Trim();
                int.TryParse(parts[1].Trim(), out int baudRate);
                int.TryParse(parts[2].Trim(), out int dataBits);

                string stopBitsStr = parts[3].Trim();
                StopBits stopBits = StopBits.One;
                if (Enum.TryParse(stopBitsStr, true, out StopBits sb)) stopBits = sb;
                else if (stopBitsStr == "1") stopBits = StopBits.One;

                string parityStr = parts[4].Trim();
                Parity parity = Parity.None;
                if (Enum.TryParse(parityStr, true, out Parity pa)) parity = pa;

                if (string.IsNullOrWhiteSpace(portName))
                {
                    comItem.状态 = "格式错误";
                    return;
                }

                // 1. 首先检查是否被本程序的 VDevice 占用
                //    如果是，肯定是 OK
                if (IsSerialPortOpenedByApplication(portName))
                {
                    comItem.状态 = "OK";
                    comItem.实测 = $"{portName}通畅(本程序使用中)";
                    comItem.完成时间 = DateTime.Now;
                    return;
                }

                // 2. 如果没被本程序占用，尝试物理连接测试
                //    不仅看是否能打开，还要看"为什么"打不开
                var testResult = TestSerialPortDetailed(portName, baudRate, parity, dataBits, stopBits);

                if (testResult.Status == PortTestStatus.Available)
                {
                    // 能打开 -> OK
                    comItem.状态 = "OK";
                    comItem.实测 = $"{portName}通畅(空闲)";
                }
                else if (testResult.Status == PortTestStatus.Occupied)
                {
                    // 被占用 -> OK (用户要求：占用意味着串口正常)
                    comItem.状态 = "OK";
                    comItem.实测 = $"{portName}通畅(被占用)";
                }
                else
                {
                    // 不存在或其它错误 -> NG
                    comItem.状态 = "NG";
                    comItem.实测 = $"{portName}异常: {testResult.Message}";
                }

                comItem.完成时间 = DateTime.Now;
            }
            catch (Exception ex)
            {
                comItem.状态 = "NG";
                comItem.实测 = ex.Message;
            }
        }

        // 定义测试状态枚举
        private enum PortTestStatus
        {
            Available, // 可用（成功打开关闭）
            Occupied,  // 被占用（UnauthorizedAccessException）
            Error      // 错误（不存在或其他）
        }

        // 定义结果结构
        private struct PortTestResult
        {
            public PortTestStatus Status;
            public string Message;
        }

        /// <summary>
        /// 详细测试串口状态
        /// </summary>
        private PortTestResult TestSerialPortDetailed(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            SerialPort testPort = null;
            try
            {
                testPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
                testPort.ReadTimeout = 200;
                testPort.WriteTimeout = 200;

                testPort.Open(); // 尝试打开

                // 如果能走到这里，说明打开成功
                if (testPort.IsOpen)
                {
                    testPort.Close();
                }

                return new PortTestResult { Status = PortTestStatus.Available, Message = "Success" };
            }
            catch (UnauthorizedAccessException)
            {
                // 关键点：捕获“未授权访问异常”，这通常意味着串口存在但被其他进程占用
                // 返回 Occupied 状态
                return new PortTestResult { Status = PortTestStatus.Occupied, Message = "被占用" };
            }
            catch (Exception ex)
            {
                // 其它异常（如 IOException 找不到文件）视为错误
                return new PortTestResult { Status = PortTestStatus.Error, Message = "无法打开或不存在" };
            }
            finally
            {
                if (testPort != null) testPort.Dispose();
            }
        }

        /// <summary>
        /// 辅助方法：检查串口是否被VCommuncation设备打开
        /// </summary>
        private bool IsSerialPortOpenedByApplication(string portName)
        {
            try
            {
                var allCommDevices = _deviceEngine.GetDevices(typeof(VCommuncation));
                if (allCommDevices == null) return false;

                foreach (var device in allCommDevices)
                {
                    if (device is VCommuncation vComm && vComm.Communication != null && vComm.Communication.IsConnected)
                    {
                        if (vComm.Communication is CommSerial commSerial)
                        {
                            if (string.Equals(commSerial.PortName, portName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override async void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }

        // 修改后的 OnOneKeyCheck 方法
        public override void OnOneKeyCheck(object obj)
        {
            try
            {
                StartAsync();
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"{SelectedReportPage?.Name}:失败" });
            }
            finally
            {
                ProgressValue = 100; // 完成后设置进度条为100%
            }
        }

    }
}

