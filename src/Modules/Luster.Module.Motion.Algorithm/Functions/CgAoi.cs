using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Luster.Module.Motion.Algorithm.Functions
{
    public enum TriggerType
    {
        [Description("发送信号")]
        SendSignal,

        [Description("获取结果")]
        GetResult,
    }
    public class CgAoi : OverTimeFunction, IPauseFunction, IGetExtResult
    {
        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("触发命令", 2, CN = "触发命令")]
        public LStringEx Command { get; set; }

        [Parameter("防止VA返回多次数据，平台只取最后一次数据", 5, CN = "结尾标识", DefaultV = "&")]
        public string LastCharter { get; set; }

        [Parameter("触发类型", 5, CN = "触发类型", DefaultV = TriggerType.SendSignal)]
        public TriggerType triggerType { get; set; } = TriggerType.SendSignal;

        [DependOn("triggerType",TriggerType.GetResult)]
        [Parameter("数据个数", 6, CN = "每个产品的数据个数", DefaultV = 14)]
        public int DataNumberOfSingleProduct { get; set; }
        //[Parameter("会和Command组合起来发送到VA中", 2, CN = "产品码", CanRef = ParamRef.Ref)]
        //public string SN { get; set; }

        [Parameter("输出参数", 9, CN = "工位1结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [Parameter("输出参数", 9, CN = "工位2结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result2 { get; set; }

        [Parameter("输出参数", 10, CN = "工位1结果字符串", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string Result1Str { get; set; }

        [Parameter("输出参数", 11, CN = "工位2结果字符串", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string Result2Str { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        public CgAoi()
        {
            this.Icon = "\xe65b";
            this.Tips = "CgAoi的通用格式";
        }

        /// <summary>
        /// 函数
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            GetVDevice<VCommuncation>(CommDevice, out var vComm);
            vComm.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);

            vComm.Open();

            ///清空缓存
            vComm.ClearCache();

            // 发送命令
            string sendContent = Command.GetString(MyOwner);

            vComm.Write($"{sendContent}");

            // 空跑模式直接返回
            if (IsEmptyMode)
            {
                Result = true;
                Result2 = true;
                Result1Str = string.Empty;
                Result2Str = string.Empty;
                return true;
            }
            ///等待结果
            string socketResult = vComm.ReadSingle<string>("", OverTime);

            var datas = socketResult.Split(LastCharter.ToCharArray());

            // 取最后一个
            string lastResult = datas.LastOrDefault(u => !string.IsNullOrEmpty(u));
            if (lastResult != null)
            {
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"{MyOwner.Alias} 原始结果:{string.Join(",", datas)},使用结果:{lastResult}");
                if (lastResult.Split(',').Length >= 6)
                {

                    Result = (lastResult.Split(',')[4] == "OK");
                    Result2 = (lastResult.Split(',')[5] == "OK");
                    if (triggerType == TriggerType.GetResult)
                    {
                        if (lastResult.Split(',').Length >= DataNumberOfSingleProduct * 2 + 6)
                        {
                            List<string> result = lastResult.Split(',').ToList();
                            var lstResult1 = result.GetRange(6, DataNumberOfSingleProduct);
                            var lstResult2 = result.GetRange(6 + DataNumberOfSingleProduct, DataNumberOfSingleProduct);

                            Result1Str = string.Join(",", lstResult1);
                            Result2Str = string.Join(",", lstResult2);
                        }
                        else
                        {
                            MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"数据长度不足,数据结果为{lastResult},请确认单个工位数据长度");
                        }
                    }
                }
                else
                {
                    MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"数据长度不足,数据结果为{lastResult}");
                    Result = false;
                    Result = false;
                }
            }
            else
            {
                return false;
            }
            return base.DoExcute(out errMsg);
        }

        public override void Pause()
        {
            GetVDevice<VCommuncation>(CommDevice, out var vComm);
            if (vComm != null)
            {
                vComm.Close();
            }
        }

        public Dictionary<string, object> GetExtResult()
        {
            return new Dictionary<string, object>
        {
            { "FrameRate", 23 },
            { "Resolution", 12000000 },
            { "Width", 4000 },
            { "Height", 3096 },
            { "Bits", 8 },
            { "Bandwidth", "1GB" }
        };
        }
    }
}

