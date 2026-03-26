using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.SimDevice.Engine;
using Luster.TaskFlow.Motion.Interfaces;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Runtime;
using System.Collections;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class DustContentVM : MotionVM, IDockContent
    {
        string IDockContent.Name => "Dust";

        string IDockContent.RegionName { get; set; } = "DustContent";

        /// <summary>
        /// UI线程
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// dust
        /// </summary>
        VCommuncation? vDust;

        private IMotionController _controller;

        private IDeviceEngine _engine;

        private WhileTool whileTool;

        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _isEnable=true;

        public bool IsEnable
        {
            get => _isEnable;
            set=>SetProperty(ref _isEnable, value);
        }

        /// <summary>
        /// 0.3um的微尘数
        /// </summary>
        private string _zeroPointThreeDust="0";

        public string ZeroPointThreeDust
        {
            get => _zeroPointThreeDust;
            set=>SetProperty(ref _zeroPointThreeDust, value);
        }

        /// <summary>
        /// 0.5um的微尘数
        /// </summary>
        private string _zeroPointFiveDust="0";

        public string ZeroPointFiveDust
        {
            get => _zeroPointFiveDust;
            set => SetProperty(ref _zeroPointFiveDust, value);
        }


        /// <summary>
        /// 1.0um的微尘数
        /// </summary>
        private string _oneDust = "0";

        public string OneDust
        {
            get => _oneDust;
            set => SetProperty(ref _oneDust, value);
        }

        /// <summary>
        /// 5.0um的微尘数
        /// </summary>
        private string _fiveDust = "0";

        public string FiveDust
        {
            get => _fiveDust;
            set => SetProperty(ref _fiveDust, value);
        }

        private string _testTime = "0";

        public string TestTime
        {
            get => _testTime;
            set=>SetProperty(ref _testTime, value);
        }


        public DustContentVM()
        {

        }

        public DustContentVM(ICommonBus _commonBus,
            IDialogService dialogService,
            IDbManager dbManager,
            IMotionController motionController,
            IWebService webService,
            IConfigManager configManager,
        Dispatcher dispatcher,
            IDeviceEngine deviceEngine) : base(_commonBus)
        {
            _engine = deviceEngine;
            _dispatcher = dispatcher;
            _controller = motionController;
            var vCom = _engine.GetVDevices<VCommuncation>();
            foreach (var cffu in vCom)
            {
                if (cffu.Name.ToUpper() == "DUST")
                {
                    vDust = cffu;
                }
            }
            if (vDust == null)
            {
                var newAlarm = new DataStruct.DataModels.AlarmInfo(null,
                    DataStruct.Enums.AlarmType.InfoTip, "没有找到名为Dust的串口通讯", "");
                _controller.MotionEngine.OnAlarm(newAlarm);
            }
            whileTool = new WhileTool(true);

            // 开启一个循环读取
            Task.Run(() =>
            {
                whileTool.While(Refresh, 120000);
            });

        }

        public void Refresh()
        {
            var Sendmessage = "01 03 00 00 00 26";
            _dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(() =>
            {
                if (vDust != null&&IsEnable)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            vDust.Open();
                            var recv = vDust.Read<byte>(Sendmessage, 2000);

                            if (recv != null&&recv.Count>=1)
                            {
                                var str = ByteToHexString(recv.ToArray());
                                //var result=Convert.ToHexString(recv.ToArray(),0,recv.Count);
                                ////string um03Num = result.Substring(85, 4);
                                //dustinfo.Um03Num = Convert.ToInt32(result.Substring(86, 8), 16);
                                //dustinfo.Um05Num = Convert.ToInt32(result.Substring(94, 8), 16);
                                //dustinfo.Um10Num = Convert.ToInt32(result.Substring(102, 8), 16);
                                //dustinfo.Um50Num = Convert.ToInt32(result.Substring(110, 8), 16);
                                //dustinfo.TestTime = Convert.ToInt32(result.Substring(10, 4), 16);
                                ZeroPointThreeDust = Convert.ToInt32(str.Substring(86, 8), 16).ToString();
                                ZeroPointFiveDust = Convert.ToInt32(str.Substring(94, 8), 16).ToString();
                                OneDust = Convert.ToInt32(str.Substring(102, 8), 16).ToString();
                                FiveDust = Convert.ToInt32(str.Substring(110, 8), 16).ToString();
                                TestTime = Convert.ToInt32(str.Substring(10, 4), 16).ToString();
                            }
                        }
                        catch (DeviceTimeoutException ex)
                        {
                            //    var newAlarm = new DataStruct.DataModels.AlarmInfo(null,
                            //DataStruct.Enums.AlarmType.InfoTip, "尘埃计数器连接超时", "");
                            //    _controller.MotionEngine.OnAlarm(newAlarm);
                        }
                    });
                 }
                //else
                //{
                //    var newAlarm = new DataStruct.DataModels.AlarmInfo(null,
                //        DataStruct.Enums.AlarmType.InfoTip, "没有找到名为Dust的串口通讯", "");
                //    _controller.MotionEngine.OnAlarm(newAlarm);
                //}

            }));
        }

        public string ByteToHexString(byte[] bytes)
        {
            // string str = "";
            var str = new System.Text.StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                str.Append(String.Format("{0:X} ", bytes[i]));//var拼接
            }
            string s = str.ToString();
            return s;
        }
    }
}
