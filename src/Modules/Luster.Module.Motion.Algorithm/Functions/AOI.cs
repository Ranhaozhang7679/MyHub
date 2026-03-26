#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Group
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       Group.cs
* 创建时间:       2022/5/23 21:49:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      645e1ea1-2539-4326-816a-83fa324348de
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/23 21:49:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.Integration.AOI;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public enum AOICommandType
    {
        [Description("开始拍照")]
        Start,

        [Description("查询结果")]
        Query,
    }

    public class AOI : OverTimeFunction, IPauseFunction, IGetExtResult
    {
        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("通信服务器地址", 1, CN = "字符协议", DefaultV = ProtocolType.StringUtf8)]
        public ProtocolType ProtocolType { get; set; }

        [Parameter("命令类型", 2, CN = "命令类型", DefaultV = AOICommandType.Start)]
        public AOICommandType Command { get; set; }

        [DependOn("Command", AOICommandType.Start)]
        [Parameter("开始命令", 3, CN = "开始命令")]
        public LStringEx StartCommand { get; set; }

        [DependOn("Command", AOICommandType.Start)]
        [Parameter("是否作为服务器启用", 4, CN = "主服务器", DefaultV = false)]
        public bool MainServer { get; set; }

        [DependOn("Command", AOICommandType.Query)]
        [Parameter("产品ID,对应命令的UniqueId", 6, CN = "产品ID", DefaultV = "", CanRef = ParamRef.Ref)]
        public string QuerySN { get; set; }

        [DependOn("Command", AOICommandType.Query)]
        [Parameter("强制下料", 7, CN = "强制下料")]
        public LStringEx OutCommand { get; set; }

        [Parameter("AOI结果", 21, CN = "AOI结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool AOIResult { get; set; }

        [Parameter("AOI耗时,单位s", 22, CN = "AOI耗时", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double AOITime { get; set; }

        [Parameter("缺陷原因", 23, CN = "缺陷原因", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string AOIReason { get; set; }

        [Parameter("错误Code", 24, CN = "AOICode", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string AOICode { get; set; }

        [Parameter("AOI结果", 25, CN = "原始图片", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string SrcImgPath { get; set; }

        [Parameter("AOI结果", 26, CN = "结果图片", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string ResultImgPath { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public AOI()
        {
            this.Icon = "\xe600";
            this.Tips = "定位或者输出位置信息,输出的格式必须是 NG,XPos,YPos,UPos,XPos1,YPos1,UPos1,XXX";
        }

        /// <summary>
        /// 函数
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            SrcImgPath = "";
            ResultImgPath = "";

            GetVDevice<VCommuncation>(CommDevice, out var vComm);
            vComm.SetProtocol(ProtocolType);
            vComm.Open();

            if (Command == AOICommandType.Start)
            {
                // 1.先开启接收服务
                AOIHelper.Instance.StartReceiver(vComm, MainServer, OverTime);

                // 2.发送启动命令
                AOIHelper.Instance.Start(vComm, $"{StartCommand.GetString(MyOwner)}", OverTime);
            }
            else if (Command == AOICommandType.Query)
            {
                string outCommond = OutCommand?.GetString(MyOwner);
                var aoiResult = AOIHelper.Instance.GetResult(vComm, QuerySN, outCommond, OverTime);
                if (aoiResult != null)
                {
                    AOIResult = aoiResult.Result == "OK";
                    AOITime = aoiResult.Checktime;
                    AOIReason = aoiResult.Resulttype;
                    AOICode = GetErrorCode(aoiResult);
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 获取ErrorCode
        /// </summary>
        /// <param name="aoiResult"></param>
        /// <returns></returns>
        private string GetErrorCode(AOIResult aoiResult)
        {
            string errCode = aoiResult.Code;
            if (string.IsNullOrEmpty(errCode) && aoiResult.Images != null && aoiResult.Images.Count > 0)
            {
                var img = aoiResult.Images[0];
                if (img.Defects != null && img.Defects.Count > 0)
                {
                    errCode = img.Defects[0].Code;
                }
            }

            return errCode;
        }

        /// <summary>
        /// 中断线程
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="newV"></param>
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(CommDevice))
            {
                GetVDevice<VCommuncation>(CommDevice, out var vComm);
                AOIHelper.Instance.StopReceiver(vComm);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            GetVDevice<VCommuncation>(CommDevice, out var vComm);
            AOIHelper.Instance.StopReceiver(vComm);
        }

        public override void Pause()
        {
            //GetVDevice<VCommuncation>(CommDevice, out var vComm);
            //AOIHelper.Instance.StopReceiver(vComm);
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

    public class AOIAction
    {
        public string Action { get; set; }
    }

    public class Alarm
    {
        public int AlarmID { get; set; }

        public string AlarmText { get; set; }
    }

    public class AOIAlarm
    {
        public string Action { get; set; }

        public int PC { get; set; }

        /// <summary>
        /// 报警数据
        /// </summary>
        public List<Alarm> Data { get; set; }
    }
}