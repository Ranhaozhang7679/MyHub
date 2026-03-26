using HandyControl.Expression.Shapes;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Prism.Events;
using Prism.Mvvm;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.SubSystem.Models
{
    public class SysStatusModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 边框颜色
        /// </summary>
        private Brush _brush = Brushes.Gray;
        public Brush Brush
        {
            get { return _brush; }
            set { SetProperty(ref _brush, value); }
        }

        private string sysName = "";


        private TrainRunMode trainRunMode = TrainRunMode.Idle;

        private bool isEnable;
        // Hive状态条变化时，发布事件
        private readonly IEventAggregator _ea;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseApi"></param>
        public SysStatusModel(BaseMESAPI baseApi)
        {
            sysName = baseApi.GetSystem();
            Name = sysName;

            baseApi.StatusChangedEvent -= BaseApi_StatusChangedEvent;
            baseApi.StatusChangedEvent += BaseApi_StatusChangedEvent;

            baseApi.IsConnectEvent -= BaseApi_IsConnectEvent;
            baseApi.IsConnectEvent += BaseApi_IsConnectEvent;

            baseApi.IsEnabledEvent -= BaseApi_IsEnabledEvent;
            baseApi.IsEnabledEvent += BaseApi_IsEnabledEvent;

            // 运行时才去抓，不依赖构造函数
            _ea = ContainerLocator.Container.Resolve<IEventAggregator>();
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        /// <param name="isEnabled"></param>
        private void BaseApi_IsEnabledEvent(bool isEnabled)
        {
            if (!isEnabled)
            {
                Brush = Brushes.Gray;
                Name = $"{sysName} Disabled";
                isEnable = false;
            }
            else
            {
                isEnable = true;
                BaseApi_StatusChangedEvent(trainRunMode, trainRunMode);
            }
        }

        /// <summary>
        /// 是否连接失败
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="isConn"></param>
        private void BaseApi_IsConnectEvent(object arg1, bool isConn)
        {
            // 连接失败，则紫色显示

            if (sysName.ToUpper()=="HIVE")
            {
                if (!isConn)
                {
                    Brush = Brushes.Purple;
                    Name = $"{sysName} Comm error";
                    // 发布事件
                    _ea.GetEvent<AlarmEventOnboard>().Publish();
                }
            }
            else if (!isConn & isEnable)
            {
                Brush = Brushes.Purple;
                Name = $"{sysName} Comm error";
                // 发布事件
                _ea.GetEvent<AlarmEventOnboard>().Publish();
            }
            
        }

        /// <summary>
        /// 设备状态发送变更
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        private void BaseApi_StatusChangedEvent(TrainRunMode src, TrainRunMode dst)
        {
            switch (dst)
            {
                case TrainRunMode.OffBoard:
                    Brush = Brushes.Gray;
                    Name = $"{sysName} OffBoard";
                    trainRunMode = TrainRunMode.OffBoard;
                    if (sysName.ToUpper() == "HIVE")
                    {
                        _ea.GetEvent<AlarmEventOnboard2>().Publish();
                    }
                    break;
                case TrainRunMode.Idle:
                    Brush = Brushes.Yellow;
                    Name = $"{sysName} Idle";
                    trainRunMode = TrainRunMode.Idle;
                    if (sysName.ToUpper() == "HIVE")
                    {
                        _ea.GetEvent<AlarmEventOnboardClosed>().Publish();
                    }
                    break;
                case TrainRunMode.Running:
                    Brush = Brushes.Green;
                    Name = $"{sysName} Running";
                    trainRunMode = TrainRunMode.Running;
                    if (sysName.ToUpper() == "HIVE")
                    {
                        _ea.GetEvent<AlarmEventOnboardClosed>().Publish();
                    }

                    break;
                case TrainRunMode.Engineering:
                    break;
                case TrainRunMode.Down:
                    Brush = Brushes.Red;
                    Name = $"{sysName} AutoDown";
                    trainRunMode = TrainRunMode.Down;
                    if (sysName.ToUpper() == "HIVE")
                    {
                        _ea.GetEvent<AlarmEventOnboardClosed>().Publish();
                    }
                    break;
                case TrainRunMode.ManualDown:
                    Brush = Brushes.Blue;
                    Name = $"{sysName} ManualDown";
                    trainRunMode = TrainRunMode.ManualDown;
                    break;
                case TrainRunMode.CommErr:
                    Brush = Brushes.Purple;
                    Name = $"{sysName} Comm error";
                    trainRunMode = TrainRunMode.CommErr;
                    if (sysName.ToUpper() == "HIVE")
                    {
                        _ea.GetEvent<AlarmEventOnboard>().Publish();
                    }
                    break;
            }
        }
    }
}
