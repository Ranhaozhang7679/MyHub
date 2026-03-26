using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.Enums;
using System.Threading;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Luster.TaskFlow.Motion.Interfaces;

namespace Luster.Module.Motion.Device.Functions
{
    public class ForceAxis : MotionFunction, IPauseFunction, IStopFunction
    {
        [NotEmpty]
        [Parameter("轴", 0, CN = "轴")]
        public VAxisDevice Axis { get; set; }

        [NotEmpty]
        [Parameter("通信设备", 1, CN = "力传感器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [NotEmpty]
        [Parameter("站号", 2, CN = "站号", DefaultV = 1)]
        public int Slave { get; set; }

        [NotEmpty]
        [Parameter("通道号", 3, CN = "通道号", DefaultV = 1)]
        public int Channel { get; set; }

        [NotEmpty]
        [Parameter("高速度", 4, CN = "高速度", DefaultV = 200)]
        public int HighSpeed { get; set; }

        [NotEmpty]
        [Parameter("低速度", 5, CN = "低速度", DefaultV = 20)]
        public int LowSpeed { get; set; }

        [NotEmpty]
        [Parameter("过渡位置", 6, CN = "过渡位置", DefaultV = 5)]
        public double TransitionPos { get; set; }

        [NotEmpty]
        [Parameter("极限位置", 7, CN = "极限位置", DefaultV = 10)]
        public double LimitPos { get; set; }

        [NotEmpty]
        [Parameter("力值设置", 8, CN = "力值(N)", DefaultV = 6)]
        public double SetForce { get; set; }

        [NotEmpty]
        [Parameter("低速下压超时", 8, CN = "下压超时", DefaultV = 5000)]
        public int TimeOut { get; set; }

        [NotEmpty]
        [Parameter("力值", 12, CN = "力值(N)", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double FeedBackForce { get; set; }

        private CancellationTokenSource stopToken = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        public ForceAxis()
        {
            this.Icon = "\xe6b3";
            this.Tips = "带力传感器的轴";
        }

        public override bool DoExcute(out string errMsg)
        {
            //数据定义
            errMsg = "";

            //比较规则
            OpRule OpRule = OpRule.GatherEqual;

            //起始地址
            int StartAddress = 80;

            //设置力值，单位为克
            int SetForceVal;

            //输出力值，单位为克
            int ForceValue = 0;

            //0.获取设备
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            GetVDevice<VAxis>(Axis, out var axis);
            SetForceVal = (Int32)((SetForce * 1000.000 / 9.8));

            //1.轴高速运动
            axis.MoveAbs(TransitionPos, HighSpeed);
            axis.CheckMotionDone();
            pauseRest.Set();

            //2.轴低速运动
            axis.Jog(true, LowSpeed);

            //3.实时获取轴位置
            stopToken = new CancellationTokenSource();

            //4.等待力值到达
            communcation.Open();
            communcation.Wait<int>(MyOwner.ID, SetForceVal, OpRule, $"{Slave} 03 {StartAddress} 1", TimeOut, stopToken, 10);

            //5.轴停止
            axis.Stop();

            //6.输出力值
            var coilVs = communcation.Read<int>($"{Slave} 03 {StartAddress} 1");
            if (coilVs.Count > 0)
            {
                ForceValue = coilVs[0];
            }
            //7.单位转换
            //单位转换成N
            FeedBackForce = ((double)ForceValue / 1000.000) * 9.8;
            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 轴Token
        /// </summary>
        private CancellationTokenSource axisToken = new CancellationTokenSource();

        /// <summary>
        /// 控制暂停
        /// </summary>
        private ManualResetEventSlim pauseRest = new ManualResetEventSlim(false);

        /// <summary>
        /// 属性变更
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <param name="newV">新值</param>
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(Axis))
            {
                GetVDevice<VAxis>(newV as VDevice, out var vAxis);
                MontiorPosition(vAxis);
            }
        }

        /// <summary>
        /// 监视安全距离
        /// </summary>
        /// <param name="vAxis"></param>
        private void MontiorPosition(VAxis vAxis)
        {
            axisToken?.Cancel();
            axisToken = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (axisToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // 将流程暂停
                    if (!pauseRest.IsSet)
                    {
                        pauseRest.Wait();
                    }

                    // 实时获取轴当前位置
                    double currentPos;
                    currentPos = vAxis.GetCurrentPos();
                    if (currentPos >= LimitPos)
                    {
                        stopToken?.Cancel();
                    }

                    Thread.Sleep(10);
                }

            }, axisToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
