using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class PDCAWIP : MotionFunction, IPauseFunction
    {

        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        /// <summary>
        /// 上传PDCA启用
        /// </summary>
        [NotEmpty]
        [Parameter("启用即上传PDCA", 1, CN = "启用", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool IsEnable { get; set; }


        /// <summary>
        /// LCG
        /// </summary>
        [Parameter("LCG", 2, CN = "LCG", CanRef = ParamRef.Ref)]
        public string LCG { get; set; }

        /// <summary>
        /// WIP长度
        /// </summary>
        [Parameter("WIP长度", 3, CN = "WIP长度", DefaultV = 18)]
        public int WIP_Length { get; set; }


        /// <summary>
        /// 输出结果
        /// </summary>
        [Parameter("获取WIP结果", 10, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 输出WIP
        /// </summary>
        [Parameter("WIP", 11, CN = "WIP", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public String OutWIP { get; set; }


        static string testSeriesID;


        public PDCAWIP()
        {
            this.Tips = "PDCA获取WIP";
            this.Icon = "\xe6a8";
        }
        public override bool DoExcute(out string errMsg)
        {
            // 空跑模式或者没启用直接返回
            // 1成功；2图片拷贝失败；3数据上传失败; 4图片和数据上传都失败
            if (IsEmptyMode || !IsEnable)
            {
                errMsg = "";
                OutWIP = DateTime.Now.ToString("yyyyMMddHHmmss");
                Result = true;
                return true;
            }

            #region 0.定义参数及初始化
            string sendStr;
            #endregion


            //1.获取通讯设备
            GetVDevice<VCommuncation>(CommDevice, out var vCommPdca);


            //2.连接通讯设备
            vCommPdca.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);
            vCommPdca.Open();


            //3.获取WIP
            sendStr = $"sfc_post@c=QUERY_RECORD&sn={LCG}&p=fgsn\n";
            MyOwner.OnLog(LogType.Debug, $"PDCA发送数据,只获取WIP:{sendStr}");
            vCommPdca.Write(sendStr);
            // 等待结果
            string socketResult = vCommPdca.ReadSingle<string>("", 5000);
            MyOwner.OnLog(LogType.Debug, $"PDCA接收数据,只获取WIP:{socketResult}");
            var wipArray = socketResult.Split('\n');
            if (wipArray.Length > 1 && wipArray[1].Length >= WIP_Length)
            {
                OutWIP = wipArray[1].Substring(5, WIP_Length);
                Result = true;
            }
            else
            {
                OutWIP = LCG;
                Result = false;
            }
            return base.DoExcute(out errMsg);
        }

    }
}
