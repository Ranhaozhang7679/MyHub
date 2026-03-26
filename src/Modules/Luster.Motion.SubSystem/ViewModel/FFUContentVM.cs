using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Luster.Motion.DataStruct.VDevice;
using Luster.Common.Tools;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.Integration.Web;
using System.Net;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using System.Threading;
using Prism.Commands;
using System.IO.Ports;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class FFUContentVM : MotionVM, IDockContent
    {
        string IDockContent.Name => "FFU";

        string IDockContent.RegionName { get; set; } = "FFUContent";

        /// <summary>
        /// 循环
        /// </summary>
        private WhileTool whileTool = null;

        /// <summary>
        /// UI线程
        /// </summary>
        private Dispatcher _dispatcher;

        private readonly CancellationTokenSource _cts = null;

        /// <summary>
        /// FFU
        /// </summary>
        VCommuncation? vFFU;


        /// <summary>
        /// FFU2
        /// </summary>
        VCommuncation? vFFU2;

        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;



        /// <summary>
        /// 低速度模式电流值下限
        /// </summary>
        private double lowCurrentLowLimit;
        public double LowCurrentLowLimit
        {
            get => lowCurrentLowLimit;
            set => SetProperty(ref lowCurrentLowLimit, value);
        }

        /// <summary>
        /// 低速度模式电流值上限
        /// </summary>
        private double lowCurrentUpperLimit;
        public double LowCurrentUpperLimit
        {
            get => lowCurrentUpperLimit;
            set => SetProperty(ref lowCurrentUpperLimit, value);
        }

        /// <summary>
        /// 中速度模式电流值下限
        /// </summary>
        private double middleCurrentLowLimit;
        public double MiddleCurrentLowLimit
        {
            get => middleCurrentLowLimit;
            set => SetProperty(ref middleCurrentLowLimit, value);
        }
        /// <summary>
        /// 中速度模式电流值上限
        /// </summary>
        private double middleCurrentUpperLimit;
        public double MiddleCurrentUpperLimit
        {
            get => middleCurrentUpperLimit;
            set => SetProperty(ref middleCurrentUpperLimit, value);
        }

        /// <summary>
        /// 高速度模式电流值下限
        /// </summary>
        private double highCurrentLowLimit;
        public double HighCurrentLowLimit
        {
            get => highCurrentLowLimit;
            set => SetProperty(ref highCurrentLowLimit, value);
        }
        /// <summary>
        /// 高速度模式电流值上限
        /// </summary>
        private double highCurrentUpperLimit;

        public double HighCurrentUpperLimit
        {
            get => highCurrentUpperLimit;
            set => SetProperty(ref highCurrentUpperLimit, value);
        }



        ///// <summary>
        ///// 电流值2
        ///// </summary>
        //private double current2;

        //public double Current2
        //{
        //    get => current2;
        //    set => SetProperty(ref current2, value);
        //}
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

        #region UI相关
        /// <summary>
        /// 电流值
        /// </summary>
        private double current;

        public double Current
        {
            get => current;
            set => SetProperty(ref current, value);
        }

        /// <summary>
        /// FFU速度等级
        /// </summary>
        private string ffuSpeedLevel = "Low";
        public string FFUSpeedLevel
        {
            get => ffuSpeedLevel;
            set => SetProperty(ref ffuSpeedLevel, value);
        }

        /// <summary>
        ///  FFU开关
        /// </summary>
        private string ffuSwitch = "关机中";

        public string FFUSwitch
        {
            get => ffuSwitch;
            set => SetProperty(ref ffuSwitch, value);
        }

        private DelegateCommand _openFFU = null;

        public DelegateCommand FFUOpen => _openFFU ?? (_openFFU = new DelegateCommand(
        () => {
            if (vFFU != null && CheckSerialPortIsExist(vFFU.ConnectStr))
            {
                vFFU.Write<short>(1, "211 06 260 1");
            }
            if(vFFU2 != null && CheckSerialPortIsExist(vFFU2.ConnectStr))
            {
                vFFU2.Write<short>(1, "211 06 260 1");
            }
        }));

        private DelegateCommand _closeFFU = null;

        public DelegateCommand FFUClose => _closeFFU ?? (_closeFFU = new DelegateCommand(
        () => {
            if (vFFU != null && CheckSerialPortIsExist(vFFU.ConnectStr))
            {
                vFFU.Write<short>(0, "211 06 260 1");
            }
            if (vFFU2 != null && CheckSerialPortIsExist(vFFU2.ConnectStr))
            {
                vFFU2.Write<short>(0, "211 06 260 1");
            }
        }));

        private DelegateCommand _FFULow = null;

        public DelegateCommand FFULow => _FFULow ?? (_FFULow = new DelegateCommand(
        () => {
            if (vFFU != null && CheckSerialPortIsExist(vFFU.ConnectStr))
            {
                vFFU.Write<short>(1, "211 06 261 1");
            }
            if (vFFU2 != null && CheckSerialPortIsExist(vFFU2.ConnectStr))
            {
                vFFU2.Write<short>(1, "211 06 261 1");
            }
        }));


        private DelegateCommand _FFUMiddle = null;

        public DelegateCommand FFUMiddle => _FFUMiddle ?? (_FFUMiddle = new DelegateCommand(
        () => {
            if (vFFU != null && CheckSerialPortIsExist(vFFU.ConnectStr))
            {
                vFFU.Write<short>(2, "211 06 261 1");
            }
            if (vFFU2 != null && CheckSerialPortIsExist(vFFU2.ConnectStr))
            {
                vFFU2.Write<short>(2, "211 06 261 1");
            }
        }));


        private DelegateCommand _FFUHigh = null;

        public DelegateCommand FFUHigh => _FFUHigh ?? (_FFUHigh = new DelegateCommand(
        () => {
            if (vFFU != null && CheckSerialPortIsExist(vFFU.ConnectStr))
            {
                vFFU.Write<short>(3, "211 06 261 1");
            }
            if (vFFU2 != null && CheckSerialPortIsExist(vFFU2.ConnectStr))
            {
                vFFU2.Write<short>(2, "211 06 261 1");
            }
        }));

        private DelegateCommand _ClosePage;
        public DelegateCommand ClosePage => _ClosePage ??= new DelegateCommand(() =>
        {
            _cts?.Cancel();
        });


        #endregion


        /// <summary>
        /// 设备
        /// </summary>
        private IDeviceEngine _deviceEngine;


        IConfigManager _configManager;

        public FFUContentVM()
        {

        }


        public FFUContentVM(ICommonBus _commonBus,
            IDialogService dialogService,
            IDbManager dbManager,
            IMotionController motionController,
            IWebService webService,
            IConfigManager configManager,
            Dispatcher dispatcher,
            IDeviceEngine deviceEngine) : base(_commonBus)
        {
            _cts = new CancellationTokenSource();
            _deviceEngine = deviceEngine;
            _dispatcher = dispatcher;
            _mController = motionController;
            _configManager = configManager;
            var vCom = _deviceEngine.GetVDevices<VCommuncation>();
            foreach (var cffu in vCom)
            {
                if (cffu.Name.ToUpper() == "FFU")
                {
                    vFFU = cffu;
                }
                if (cffu.Name.ToUpper() == "FFU2")
                {
                    vFFU2 = cffu;
                }
            }                   

            whileTool = new WhileTool(true);

            // 开启一个循环读取
            //Task.Run(() =>
            //{
            //    whileTool.While(Refresh, 5000);
            //});
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    Refresh();
                    await Task.Delay(5000, _cts.Token);  // 使用异步的方式，以便响应取消请求
                }
            }, _cts.Token);

        }

        private bool isSend = false;
        public string ActualSpeedLevel;
        AlarmInfo alarmInfo;

        public void Refresh()
        {
            //_dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            //{
                if (_deviceEngine.DeviceMode == DeviceMode.Virtual)
                {
                    return;
                }
                try
                {
                    if (vFFU != null || vFFU2 != null)
                    {
                        isSend = true;
                        //FFUSpeedLevel = _configManager.GetWebConfig("FFUSpeedLevel");
                        //double.TryParse(_configManager.GetWebConfig("LowCurrentLowLimit"), out double lowCurrentLowLimit);
                        //LowCurrentLowLimit = lowCurrentLowLimit;
                        //double.TryParse(_configManager.GetWebConfig("LowCurrentUpperLimit"), out double lowCurrentUpperLimit);
                        //LowCurrentUpperLimit = lowCurrentUpperLimit;
                        //double.TryParse(_configManager.GetWebConfig("MiddleCurrentLowLimit"), out double middleCurrentLowLimit);
                        //MiddleCurrentLowLimit = middleCurrentLowLimit;
                        //double.TryParse(_configManager.GetWebConfig("MiddleCurrentUpperLimit"), out double middleCurrentUpperLimit);
                        //MiddleCurrentUpperLimit = middleCurrentUpperLimit;
                        //double.TryParse(_configManager.GetWebConfig("HighCurrentLowLimit"), out double highCurrentLowLimit);
                        //HighCurrentLowLimit = highCurrentLowLimit;
                        //double.TryParse(_configManager.GetWebConfig("HighCurrentUpperLimit"), out double highCurrentUpperLimit);
                        //HighCurrentUpperLimit = highCurrentUpperLimit;

                        //Task.Run(() =>
                        //{
                        var sPortAll = SerialPort.GetPortNames();
                        if (vFFU != null && CheckSerialPortIsExist(vFFU.ConnectStr))
                        {
                            ReadFFUInfo(vFFU);
                        }
                        if(vFFU2 != null && CheckSerialPortIsExist(vFFU2.ConnectStr))
                        {

                        }
                        //if (vFFU2 != null)
                        //{
                        //    ReadFFUCurrent(vFFU2, Current2);
                        //}
                        //});
                    }
                }
                catch (Exception ex)
                {
                    alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"FFU连接失败", "F98OOOO-02");
                    _mController.MotionEngine.OnAlarm(alarmInfo);

                }
            //}));

        }

        public void ReadFFUInfo(VCommuncation ffu)
        {
            try
            {
                vFFU.Open();
                var coilVs = vFFU.Read<short>("211 03 256 9");
                Thread.Sleep(10);
                if (coilVs == null)
                {
                    alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}未读取到值!!!&{ffu.Name}No value was read", "F98OOOO-03");
                    _mController.MotionEngine.OnAlarm(alarmInfo);
                    return;
                }
                if (coilVs.Count < 6)
                {
                    alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}未读取到值!!!&{ffu.Name}No value was read", "F98OOOO-03");
                    _mController.MotionEngine.OnAlarm(alarmInfo);
                    return;
                }

                if (int.Parse(coilVs[4].ToString()) == 0)
                {
                    FFUSwitch = "关机中";
                    alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, "FFU风扇未开启&The FFU fan is not started", "F98OOOO-04");
                    _mController.MotionEngine.OnAlarm(alarmInfo);
                    return;
                }
                FFUSwitch = "已开机";

                switch (coilVs[5])
                {
                    case 1:
                        FFUSpeedLevel = "Low";
                        break;


                    case 2:
                        FFUSpeedLevel = "Middle";
                        break;

                    case 3:
                        FFUSpeedLevel = "High";
                        break;
                }

                Current = coilVs[7] / 100.0;
                
            }
            catch (Exception ex)
            {
                alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"FFU连接失败&FFU Connected Fail", "F98OOOO-02");
                _mController.MotionEngine.OnAlarm(alarmInfo);
            }
        }

        public void ReadFFUInfo2(VCommuncation ffu)
        {
            try
            {
                vFFU2.Open();
                var coilVs = vFFU2.Read<short>("211 03 256 9");
                Thread.Sleep(10);
                if (coilVs == null)
                {
                    alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}未读取到值!!!", "");
                    _mController.MotionEngine.OnAlarm(alarmInfo);
                    return;
                }
                if (coilVs.Count < 6)
                {
                    alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}未读取到值!!!", "");
                    _mController.MotionEngine.OnAlarm(alarmInfo);
                    return;
                }

                if (int.Parse(coilVs[4].ToString()) == 0)
                {
                    FFUSwitch = "关机中";
                    alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, "风扇未开启", "");
                    _mController.MotionEngine.OnAlarm(alarmInfo);
                    return;
                }
                FFUSwitch = "已开机";

                switch (coilVs[5])
                {
                    case 1:
                        FFUSpeedLevel = "Low";
                        break;


                    case 2:
                        FFUSpeedLevel = "Middle";
                        break;

                    case 3:
                        FFUSpeedLevel = "High";
                        break;
                }

                Current = coilVs[7] / 100.0;
            }
            catch (Exception ex)
            {
                alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"FFU连接失败", "");
                _mController.MotionEngine.OnAlarm(alarmInfo);
            }
        }


        public void ReadFFUCurrent(VCommuncation ffu, double current)
        {
            vFFU.Open();
            isSend = false;
            short val = 0;
            if (FFUSpeedLevel == "Low") val = 1;
            if (FFUSpeedLevel == "Middle") val = 2;
            if (FFUSpeedLevel == "High") val = 3;

            //批量读取
            //0.驱动器型号
            //1.电源板IP
            //2.FFU台数
            //3.最高档位
            //4.起/停风机
            //5.调风速
            //6.系统状态
            //7.电流值（单位0.01A）
            //8.过载电流NA(单位0.01A)
            var coilVs = vFFU.Read<short>("211 03 256 9");
            Thread.Sleep(10);
            if (coilVs == null)
            {
                alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}未读取到值!!!", "");
                _mController.MotionEngine.OnAlarm(alarmInfo);
                return;
            }
            if (coilVs.Count < 6)
            {
                alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}未读取到值!!!", "");
                _mController.MotionEngine.OnAlarm(alarmInfo);
                return;
            }

            if (int.Parse(coilVs[4].ToString()) == 0)
            {
                alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, "风扇未开启", "");
                _mController.MotionEngine.OnAlarm(alarmInfo);
                return;
            }
            if (int.Parse(coilVs[5].ToString()) != val)
            {
                //写3次，确保能够写成功
                for (int i = 0; i < 3; i++)
                {
                    vFFU.Write<short>(val, "211 06 261 1");
                    Thread.Sleep(100);
                }
                //如果切换成功后，需要延时30S，再判断电流值
                Thread.Sleep(30000);
                //30后再读取一次
                coilVs = vFFU.Read<short>("211 03 256 9");
            }
            if (coilVs != null)
                current = coilVs[7] / 100.0;

            Current = current;

            //因为低中高的电流区间有重合，所以上下限设定10%的浮动
            // 如果在低模式
            if (FFUSpeedLevel == "Low")
            {
                if (current >= LowCurrentLowLimit & current <= LowCurrentUpperLimit * 1.1)
                {
                    if (current >= LowCurrentUpperLimit)
                    {
                        current = LowCurrentUpperLimit;
                    }

                    return;
                }
            }
            //如果在中模式
            if (FFUSpeedLevel == "Middle")
            {
                if (current >= MiddleCurrentLowLimit * 0.9 & current <= MiddleCurrentUpperLimit * 1.1)
                {
                    if (current <= MiddleCurrentLowLimit)
                    {
                        current = MiddleCurrentLowLimit;
                    }
                    if (current >= MiddleCurrentUpperLimit)
                    {
                        current = MiddleCurrentUpperLimit;
                    }
                    return;
                }
            }
            //如果在高模式
            if (FFUSpeedLevel == "High")
            {
                if (current >= HighCurrentLowLimit * 0.9 & current <= HighCurrentUpperLimit)
                {
                    if (current <= HighCurrentLowLimit)
                    {
                        current = HighCurrentLowLimit;
                    }
                    return;
                }
            }

            if (current < LowCurrentLowLimit)
                ActualSpeedLevel = "BlowLow";
            else if (current >= LowCurrentLowLimit & current <= LowCurrentUpperLimit)
                ActualSpeedLevel = "Low";
            else if (current > MiddleCurrentLowLimit && current <= MiddleCurrentUpperLimit)
                ActualSpeedLevel = "Middle";
            else if (current > HighCurrentLowLimit && current <= HighCurrentUpperLimit)
                ActualSpeedLevel = "High";
            else if (current > HighCurrentUpperLimit)
                ActualSpeedLevel = "AboveHigh";
            if (FFUSpeedLevel != ActualSpeedLevel)
            {
                alarmInfo = new AlarmInfo(null, AlarmType.InfoTip, $"{ffu.Name}设定模式为{FFUSpeedLevel};实际模式为{ActualSpeedLevel}", "");
                _mController.MotionEngine.OnAlarm(alarmInfo);
            }
        }

    }
}
