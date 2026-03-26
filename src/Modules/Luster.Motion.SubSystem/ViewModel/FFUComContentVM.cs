using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 应用串口通讯的后端FFU驱动
    /// </summary>
    public class FFUComContentVM : MotionVM, IDockContent
    {
        private readonly CancellationTokenSource _cts = null;

        string IDockContent.Name => "FFUCom";

        string IDockContent.RegionName { get; set; } = "FFUComContent";

        /// <summary>
        /// 循环
        /// </summary>
        private WhileTool whileTool = null;
        /// <summary>
        /// UI线程
        /// </summary>
        private Dispatcher _dispatcher;
        /// <summary>
        /// FFU
        /// </summary>
        VCommuncation? vFFU;
        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;
        /// <summary>
        /// 设备
        /// </summary>
        private IDeviceEngine _deviceEngine;

        #region UI相关
        /// <summary>
        /// 电流值
        /// </summary>
        private double current = -1;

        public double Current
        {
            get => current;
            set => SetProperty(ref current, value);
        }

        /// <summary>
        /// FFU速度等级
        /// </summary>
        private string ffuSpeedLevel = "-1";
        public string FFUSpeedLevel
        {
            get => ffuSpeedLevel;
            set => SetProperty(ref ffuSpeedLevel, value);
        }

        /// <summary>
        ///  FFU开关
        /// </summary>
        private string ffuSwitch = "-1";

        public string FFUSwitch
        {
            get => ffuSwitch;
            set => SetProperty(ref ffuSwitch, value);
        }
        private readonly Dictionary<string, List<byte>> FFUComCode = new()
        {
            //只针对ID=1
            { "FFUOpen", new List<byte>(){0x01,0x06,0x00,0x00,0x00,0x01,0x48,0x0A} },//启动
            { "FFUClose", new List<byte>(){0x01,0x06,0x00,0x00,0x00,0x00,0x89,0xCA} },//关闭
            { "FFULow", new List<byte>(){0x01,0x06,0x00,0x01,0x00,0x01,0x19,0xCA} },//低档
            { "FFUMiddle", new List<byte>(){0x01,0x06,0x00,0x01,0x00,0x02,0x59,0xCB} },//中档 
            { "FFUHigh", new List<byte>(){0x01,0x06,0x00,0x01,0x00,0x03,0x98,0x0B} }, //高档
            { "FFURead1", new List<byte>(){0x01,0x03,0x00,0x00,0x00,0x02,0xC4,0x0B} },//读取启停、档位
            { "FFURead2", new List<byte>(){0x01,0x03,0x00,0x0A,0x00,0x02,0xE4,0x09} },//读取电流、故障
        };
        private DelegateCommand _openFFU;
        private DelegateCommand _closeFFU;
        private DelegateCommand _FFULow;
        private DelegateCommand _FFUMiddle;
        private DelegateCommand _FFUHigh;
        // 1. 在类级别定义字段，用于统计失败次数
        private int _ffuFailCount = 0;
        private int _ffuCurrentFailCount = 0;
        public DelegateCommand FFUOpen => _openFFU ??= CreateFFUCommand("FFUOpen");
        public DelegateCommand FFUClose => _closeFFU ??= CreateFFUCommand("FFUClose");
        public DelegateCommand FFULow => _FFULow ??= CreateFFUCommand("FFULow");
        public DelegateCommand FFUMiddle => _FFUMiddle ??= CreateFFUCommand("FFUMiddle");
        public DelegateCommand FFUHigh => _FFUHigh ??= CreateFFUCommand("FFUHigh");

        private DelegateCommand CreateFFUCommand(string commandKey)
        {
            return new DelegateCommand(() =>
            {
                if (FFUComCode.TryGetValue(commandKey, out var code))
                {
                    //var msg = Encoding.Default.GetBytes(code).ToList();
                    vFFU?.Communication?.Send(code);
                }
            });
        }


        private DelegateCommand _ClosePage;
        public DelegateCommand ClosePage => _ClosePage ??= new DelegateCommand(() =>
        {
            _cts?.Cancel();
        });

        #endregion

        public FFUComContentVM()
        {

        }

        public FFUComContentVM(ICommonBus _commonBus,
            IMotionController motionController,
            Dispatcher dispatcher,
            IDeviceEngine deviceEngine) : base(_commonBus)
        {
            _cts = new CancellationTokenSource();
            _deviceEngine = deviceEngine;
            _dispatcher = dispatcher;
            _mController = motionController;
            var vCom = _deviceEngine.GetVDevices<VCommuncation>();
            foreach (var cffu in vCom)
            {
                if (cffu.Name.ToUpper() == "FFUCOM")
                {
                    vFFU = cffu;
                }
            }

            whileTool = new WhileTool(true);

            // 开启一个循环读取
            //Task.Run(() =>
            //{
            //    whileTool.While(Refresh, 5000);
            //}, _cts.Token);
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    Refresh();
                    await Task.Delay(5000, _cts.Token);  // 使用异步的方式，以便响应取消请求
                }
            }, _cts.Token);


        }

        private bool CheckSerialPortIsExist(string cmd)
        {
            return SerialPort.GetPortNames().Any(p =>
            {
                int idx = cmd.IndexOf(p, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return false;

                int end = idx + p.Length;
                return end < cmd.Length && cmd[end] == '/';
            });
        }

        private AlarmInfo alarmInfo;
        private const int CurrentHighIndex = 3;
        private const int CurrentLowIndex = 4;
        private const double CurrentDivisor = 10.0; // 若单位为0.01A则为100.0
        public void Refresh()
        {
            //_dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            //{
            if (_deviceEngine.DeviceMode == DeviceMode.Virtual)
            {
                //alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"离线模式不支持FFU", "SO9999");
                //_mController.MotionEngine.OnAlarm(alarmInfo);
                return;
            }
            try
            {
                if (vFFU != null && CheckSerialPortIsExist(vFFU.ConnectStr))
                {
                    ReadFFUInfo(vFFU);
                }
            }
            catch (Exception ex)
            {
                alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"FFU连接失败", "F98OOOO-02");
                _mController.MotionEngine.OnAlarm(alarmInfo);
            }
            //}));

        }
        // 协议相关常量

        private double ParseCurrent(List<byte> data)
        {
            if (data == null || data.Count < 9)
                throw new ArgumentException("数据长度不足，无法解析电流值。");
            ushort raw = (ushort)(data[CurrentHighIndex] << 8 | data[CurrentLowIndex]);
            return raw / CurrentDivisor;
        }

        /// <summary>
        /// 读取FFU信息，至于电流等级判断就不做了，参考之前的也注释了
        /// </summary>
        /// <param name="ffu"></param>
        public void ReadFFUInfo(VCommuncation ffu)
        {
            const string netName = "FFUCOM";
            bool bIsOpen = false;
            string state = string.Empty;

            try
            {
                vFFU.Open();
                if (vFFU == null || vFFU.Communication == null || !vFFU.Communication.IsConnected)
                {
                    state = "FFU通讯异常";
                    _mController?.Update_FFUCOM_ConnectStatus(netName, bIsOpen, state);
                    return;
                }
                // 读取启停、档位
                //var readOpenLevel = Encoding.Default.GetBytes(FFUComCode["FFURead1"]).ToList();
                var readBytes = vFFU?.Communication?.Send(FFUComCode["FFURead1"], 9);
                if (!IsValidFFUResponse(readBytes))
                {
                    _ffuFailCount++;
                    if (_ffuFailCount >= 3)
                    {
                        _ffuFailCount = 0;
                        state = "FFU读取启停状态异常";
                        RaiseFFUAlarm(ffu, "FFU风扇未读取到值!!!&No value was read by the FFU fan", "F98OOOO-03");
                        _mController?.Update_FFUCOM_ConnectStatus(netName, bIsOpen, state);
                    }
                    return;
                }
                else
                {
                    _ffuFailCount = 0;
                }              
                if (readBytes[4] == 0)
                {
                    state = "FFU关机";
                    FFUSwitch = "关机中";
                    RaiseFFUAlarm(ffu, "FFU风扇未开启&The FFU fan is not started", "F98OOOO-04");
                    _mController?.Update_FFUCOM_ConnectStatus(netName, bIsOpen, state);
                    return;
                }
                FFUSwitch = "已开机";
                bIsOpen = true;
                FFUSpeedLevel = readBytes[6] switch
                {
                    1 => "Low",
                    2 => "Middle",
                    3 => "High",
                    _ => "High"
                };
                state = readBytes[6] switch
                {
                    1 => "FFU1档",
                    2 => "FFU2档",
                    3 => "FFU3档",
                    _ => $"FFU{(int)readBytes[6]}档"
                };

                Thread.Sleep(10);

                var currentBytes = vFFU?.Communication?.Send(FFUComCode["FFURead2"], 9);
                if (!IsValidFFUResponse(currentBytes))
                {
                    _ffuCurrentFailCount++;
                    if (_ffuCurrentFailCount >= 3)
                    {
                        _ffuCurrentFailCount = 0;
                        state = "FFU读取电流异常";
                        RaiseFFUAlarm(ffu, "FFU风扇读取电流异常!!!&The FFU fan reads abnormal current", "F98OOOO-05");
                        _mController?.Update_FFUCOM_ConnectStatus(netName, bIsOpen, state);
                    }
                    return;
                }
                else
                {
                    _ffuCurrentFailCount = 0;
                }
                Current = ParseCurrent(currentBytes);
            }
            catch (Exception ex)
            {
                state = "FFU连接异常";
                //alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"FFU连接失败&FFU Connected Fail: {ex.Message}", "F98OOOO-02");
                //_mController.MotionEngine.OnAlarm(alarmInfo);
            }
            //更新界面下方的StatusContent的FFU状态
            _mController?.Update_FFUCOM_ConnectStatus(netName, bIsOpen, state);
        }

        private void NormalProcedure()
        {

        }

        private void ErrorProcedure(int errorTimes)
        {
            if (errorTimes < 3) return;
        }

        private bool IsValidFFUResponse(List<byte> data) => data != null && data.Count == 9;

        private void RaiseFFUAlarm(VCommuncation ffu, string message, string code)
        {
            alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}{message}", code);
            _mController.MotionEngine.OnAlarm(alarmInfo);
        }
    }
}

