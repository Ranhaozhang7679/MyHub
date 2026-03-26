using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Module.Motion.Protocol.Functions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class Heightfinder : GoToFunction
    {
        

        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 0, CN = "测高起点", MultiValues = false)]
        public VAxisPos StartAxisPos { get; set; }

        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 0, CN = "测高终点", MultiValues = false)]
        public VAxisPos EndAxisPos { get; set; }


        [NotEmpty]
        [Parameter("通信设备", 2, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [NotEmpty]
        [Parameter("支持多个字符串进行拼接", 3, CN = "通信数据")]
        public LStringEx StringEx { get; set; }

        [Parameter("是否启用回车换行", 7, CN = "回车换行启用", CanRef = ParamRef.Ref)]
        public bool IsRN { get; set; }

        [Parameter("测高值", 20, CN = "测高值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string  HeightVal { get; set; }

        [Parameter("通信结果", 6, CN = "通信结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutString { get; set; }

        public override bool IsNeedPause => true;

        public Heightfinder()
        {
            this.Tips = "测高";
            this.Icon = "\ue652";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCommuncation>(CommDevice, out var comm);
            DateTime beginTime = DateTime.Now;
            if (comm.Protocol == null)
            {
                IProtocol protocol = null;

                List<byte> commands = new List<byte>();
                protocol = new StringProtocol();
                comm.Protocol = protocol;
            }
            string command = StringEx.GetString(Owner);
            if (IsRN)
            {
                command += "\r\n";
            }

            for(int i = 0; i < 3;i++)
            {
                bool isReceiveOK=false;
                comm.Open();
                comm.ClearCache();
                StartAxisPos.MovePostion(MyOwner.DeviceEngine, false);
                comm.Write(command);
                ReceiveMessage(comm, true);
                if(OutString=="NG")
                {
                    comm.Stop();
                    System.Threading.Thread.Sleep(300);
                }
                else
                {
                    OutString = "NG";
                    Task.Run(() =>
                    {
                        ReceiveMessage(comm, true);
                        isReceiveOK = true;
                    });
                    EndAxisPos.MovePostion(MyOwner.DeviceEngine, false);
                    while(!isReceiveOK && OutString == "NG")
                    {
                        System.Threading.Thread.Sleep(300);
                    }
                    if(OutString=="NG")
                    {
                        comm.Stop();
                        System.Threading.Thread.Sleep(300);
                        OnAlarm(AlarmType.Timeout, $"测高返回值NG");
                    }
                    else
                    {
                        i = 3;
                    }
                }
            }
          

            return base.DoExcute(out errMsg);
        }



        private void ReceiveMessage(VCommuncation comm, bool IsAutoDisConnect)
        {
            // 通讯超时、读取为空时，不主动断开连接
            OutString = comm.ReadSingle<string>("", 3000, isCloseConnect: IsAutoDisConnect);

            if (string.IsNullOrWhiteSpace(OutString))
            {
                OutString = "NG";
            }
        }
        //public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        //{
        //    base.OnNotifyPropertyUIChanged(parameter, newV);
        //    VAxisPos val = default(VAxisPos);
        //    int num;
        //    if (parameter.Name == "AxisPos")
        //    {
        //        val = (VAxisPos)((newV is VAxisPos) ? newV : null);
        //        num = ((val != null) ? 1 : 0);
        //    }
        //    else
        //    {
        //        num = 0;
        //    }
        //    if (num != 0)
        //    {
        //        val.UpdateAxis(MyOwner.DeviceEngine);
        //    }
        //}

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);

            if (parameter.Name == nameof(StartAxisPos) && newV is VAxisPos vPos)
            {
                vPos.UpdateAxis(MyOwner.DeviceEngine);
                BuildDynamicAxisPos(vPos, 5);
                BuildDynamicAxisPos(vPos, 20, TaskFlow.Common.Enums.ParamType.OUT, false);
            }
            if (parameter.Name == nameof(EndAxisPos) && newV is VAxisPos vPos1)
            {
                vPos1.UpdateAxis(MyOwner.DeviceEngine);
                BuildDynamicAxisPos(vPos1, 5);
                BuildDynamicAxisPos(vPos1, 20, TaskFlow.Common.Enums.ParamType.OUT, false);
            }
        }

        public override void Stop()
        {
            if (StartAxisPos != null)
            {
                bool flag = false;
                StartAxisPos.Stop(MyOwner.DeviceEngine, flag);
            }
          
        }

        public override void Pause()
        {
            ((MotionFunction)this).Stop();
        }

        public Dictionary<string, object> GetExtResult()
        {
            
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (AxisPosItem item in (List<AxisPosItem>)(object)StartAxisPos)
            {
                dictionary.Add($"{item.Axis.AxisType}_Spd", item.Speed);
                dictionary.Add($"{item.Axis.AxisType}_Acc", item.Acc);
                dictionary.Add($"{item.Axis.AxisType}_Dec", item.Dec);
                dictionary.Add($"{item.Axis.AxisType}_SpdTarget", item.Axis.MoveSpeed * 0.8);
                dictionary.Add($"{item.Axis.AxisType}_AccTarget", 0.1);
                dictionary.Add($"{item.Axis.AxisType}_Distance", item.Distance);
            }
            return dictionary;
        }
    }
}
