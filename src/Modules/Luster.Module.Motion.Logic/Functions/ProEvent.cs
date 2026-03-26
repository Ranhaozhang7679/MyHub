#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProLoaded
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       ProLoaded.cs
* 创建时间:       2022/8/4 14:36:24
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      01454307-87e3-489b-a051-a06c05729b91
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/4 14:36:24
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public enum ProEventType
    {
        [Description("入料事件")]
        Loaded,

        [Description("出料事件")]
        UnLoaded,

        [Description("抛料事件")]
        Throw
    }

    /// <summary>
    /// 产品入料
    /// </summary>
    public class ProEvent : MotionFunction
    {
        [NotEmpty]
        [Parameter("治具二维码", 0, CN = "事件类型", DefaultV = ProEventType.Loaded)]
        public ProEventType EventType { get; set; }

        //[DependOn("EventType", ProEventType.Loaded, ProEventType.UnLoaded)]
        //[Parameter("治具二维码", 1, CN = "治具码", CanRef = ParamRef.Ref)]
        public string JigCode { get; set; }

        [DependOn("EventType", ProEventType.Loaded, ProEventType.UnLoaded)]
        [DependOn("EventType", ProEventType.Loaded, ProEventType.Throw)]
        [Parameter("产品条码", 2, CN = "产品码", DefaultV = "NULL", CanRef = ParamRef.Ref)]
        public string BarCode { get; set; }

        [DependOn("EventType", ProEventType.Loaded)]
        [Parameter("产品的最终结果", 3, CN = "上游结果", CanRef = ParamRef.Ref)]
        public bool PrevResult { get; set; }

        /// <summary>
        /// 入料时间
        /// </summary>
        [DependOn("EventType", ProEventType.Loaded)]
        [Parameter("产品的入料时间固定格式是 yyyy-MM-dd HH:mm:ss:fff", 4, CN = "入料时间", CanRef = ParamRef.Ref)]
        public string EnterTime { get; set; }

        [DependOn("EventType", ProEventType.UnLoaded)]
        [Parameter("产品的最终结果", 5, CN = "本站结果", CanRef = ParamRef.Ref)]
        public bool ThisResult { get; set; }

        [DependOn("EventType", ProEventType.UnLoaded)]
        [Parameter("是否是抛料", 5, CN = "抛料", DefaultV = "false", CanRef = ParamRef.Ref)]
        public bool IsToss { get; set; }

        [DependOn("EventType", ProEventType.UnLoaded)]
        [DependOn("EventType", ProEventType.Throw)]
        [Parameter("报错代码", 5, CN = "报错代码", CanRef = ParamRef.Ref)]
        public string NGCode { get; set; }

        [DependOn("EventType", ProEventType.UnLoaded)]
        [DependOn("EventType", ProEventType.Throw)]
        [Parameter("报错描述", 5, CN = "报错描述", CanRef = ParamRef.Ref)]
        public string NGMsg { get; set; }

        [DependOn("EventType", ProEventType.UnLoaded)]
        [Parameter("图片路径,路径格式 D:/AA/BB/AA", 9, CN = "图像路径")]
        public LStringEx ImgPath { get; set; }

        [DependOn("EventType", ProEventType.Throw)]
        [Parameter("要进行抛料的物料", 6, CN = "物料名称")]
        public string Material { get; set; }

        [DependOn("EventType", ProEventType.UnLoaded)]
        [DependOn("EventType", ProEventType.Throw)]
        [Parameter("如果WIP不为空，优先用WIP,不再去获取SN", 7, CN = "WIP", CanRef = ParamRef.Ref)]
        public string WIP { get; set; }

        [DependOn("EventType", ProEventType.UnLoaded)]
        [DependOn("EventType", ProEventType.Throw)]
        [Parameter("前站未做", 7, CN = "前站未做", CanRef = ParamRef.Ref)]
        public bool IsPreviousStationUndo { get; set; }

        [Parameter("如果是入料就是入料时间，如果是出料就是出料时间", 7, CN = "事件时间", ParamType = ParamType.OUT)]
        public DateTime Time { get; set; }

        public override string[] NoteParams => new string[] { nameof(EventType) };

        /// <summary>
        /// 字符串格式
        /// </summary>
        private readonly string[] DateFormats = new string[]
        {
            "yyyy-MM-dd HH:mm:ss:fff",
            "yyyy_MM_dd HH:mm:ss:fff",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd/HH:mm:ss",
        };

        Dictionary<string, object> StationData = new Dictionary<string, object>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProEvent()
        {
            this.Tips = "产品入料或者出料事件，记录入站和出站信息";
            this.Icon = "\xe66b";
            JigCode = "None";
        }

        /// <summary>
        /// 产品运行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>返回结果</returns>
        public override bool DoExcute(out string errMsg)
        {
            //清空
            StationData.Clear();

            if (EventType == ProEventType.Loaded)
            {
                if (string.IsNullOrEmpty(JigCode))
                {
                    JigCode = "NG";
                }

                if (string.IsNullOrEmpty(BarCode))
                {
                    BarCode = "NG";
                }

                DateTime now = DateTime.Now;

                if (string.IsNullOrEmpty(EnterTime))
                {
                    now = DateTime.Now;
                }
                else
                {
                    //now = Convert.ToDateTime(EnterTime);
                    if (DateTime.TryParseExact(EnterTime, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out now))
                    {
                    }
                }

                MyOwner.OnProLoaded(JigCode, BarCode, PrevResult, now);
                Time = DateTime.Now;
            }
            else if (EventType == ProEventType.UnLoaded)
            {
                if (string.IsNullOrEmpty(JigCode))
                {
                    JigCode = "NG";
                }

                if (string.IsNullOrEmpty(BarCode))
                {
                    BarCode = "NG";
                }

                string imgPath = "";
                if (ImgPath != null)
                {
                    imgPath = ImgPath.GetString(Owner);
                }

                // 标准WIP长度18， 时间戳yyyyMMddHHmmss长度14，空跑时传时间戳
                string wip = (string.IsNullOrEmpty(WIP) || (WIP.Length != 18 && WIP.Length != 14 && WIP.Length != 10)) ? "N999999999" : WIP;

                //外部传入WIP
                //StationData.Add("WIP", wip);
                //if (!String.IsNullOrEmpty(WIP))
                //{
                //    StationData.Add("WIP", WIP);
                //}

                //判断前站未做
                if (IsPreviousStationUndo)
                {
                    StationData.Add("IsPreviousStationUndo", IsPreviousStationUndo);
                }
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info, $"传入二维码值为{BarCode}");
                MyOwner.OnProUnloaded(new StationResult() { JigCode = JigCode, NgCode = NGCode, ErrMsg = NGMsg, IsToss = IsToss, ProCode = BarCode, Result = ThisResult, ImagePath = imgPath, Datas = StationData, IsPreviousStationUndo = IsPreviousStationUndo, WIP = wip });

                Time = DateTime.Now;
            }
            else if (EventType == ProEventType.Throw)
            {
                if (string.IsNullOrEmpty(Material))
                {
                    errMsg = "物料名称不存在!";
                    return false;
                }

                // 标准WIP长度18， 时间戳yyyyMMddHHmmss长度14，空跑时传时间戳, N999999999长度10
                string wip = (string.IsNullOrEmpty(WIP) || (WIP.Length != 18 && WIP.Length != 14 && WIP.Length != 10)) ? "N999999999" : WIP;
                string sn = (string.IsNullOrEmpty(BarCode) || BarCode.ToUpper().Contains("NULL") || BarCode.ToUpper().Contains("Empty")) ? "" : BarCode;
                //抛小料添加抛小料的名称
                StationData.Add("ThrowMaterialName", Material);
                StationData.Add("IsSmallPart", true);
                
                IsToss = true;
                MyOwner.OnProThrow(new StationResult() { JigCode = JigCode, NgCode = NGCode, ErrMsg = NGMsg, IsToss = IsToss, ProCode = sn, WIP = wip, Datas = StationData }, Material);
                Time = DateTime.Now;
            }
            MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info, $"产品事件完成");
            return base.DoExcute(out errMsg);
        }
    }
}