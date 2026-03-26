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
    public class ICW : OverTimeFunction, IPauseFunction, IGetExtResult
    {
        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("通信服务器地址", 1, CN = "字符协议", DefaultV = ProtocolType.StringUtf8)]
        public ProtocolType ProtocolType { get; set; }

        [NotEmpty]
        [Parameter("强制下料", 2, CN = "ICW命令")]
        public LStringEx ICWCommand { get; set; }

        /// <summary>
        /// 第一个结果
        /// </summary>
        [Parameter("ICW结果", 3, CN = "ICW结果1", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool ICWResult { get; set; }

        /// <summary>
        /// 第二个结果
        /// </summary>
        [Parameter("ICW结果", 4, CN = "ICW结果2", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool ICWResult1 { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ICW()
        {
            this.Icon = "\xe600";
            this.Tips = "通过ICW一次获取多个结果";
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
            vComm.SetProtocol(ProtocolType);
            vComm.Open();

            string icwCommand = "NA";
            try
            {
                icwCommand = ICWCommand?.GetString(MyOwner) + "\r\n";
                vComm.Write(icwCommand);
            }
            catch (Exception)
            {
                MyOwner.OnLog(LogType.Error, $"IcwCommand:{ICWCommand}");
            }

            var results = vComm.Read<string>("", OverTime);
            MyOwner.OnLog(LogType.Debug, $"Source={icwCommand},Result={string.Join(",", results)}");
            if (results == null)
            {
                OnAlarm(AlarmType.WarningTip, $"ICW 返回结果为空");
                return true;
            }

            ICWResult = false;
            var rs = results[0].TrimEnd(new char[] { '\r', '\n' }).Split(',');
            for (int i = 2; i < rs.Length; i++)
            {
                if (i == 3)
                {
                    ICWResult = rs[3] == "OK";
                }

                if (i == 5)
                {
                    ICWResult1 = rs[5] == "OK";
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
}