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

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public class VA : OverTimeFunction, IPauseFunction, IGetExtResult
    {
        /// <summary>
        /// 通信服务器
        /// </summary>
        [NotEmpty]
        [Parameter("通信服务器地址", 0, CN = "服务器", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }



        [Parameter("触发命令", 2, CN = "触发命令")]
        public LStringEx Command { get; set; }

        [Limit(1, 1000)]
        [Parameter("连续NG后，重复拍照次数", 3, CN = "重复次数", DefaultV = 1)]
        public int LoopNum { get; set; }

        [Limit(-1000, 1000)]
        [Parameter("对VA结果进行防呆，防止过大", 4, CN = "偏移范围")]
        public LRange Range { get; set; }

        [Parameter("防止VA返回多次数据，平台只取最后一次数据", 5, CN = "结尾标识", DefaultV = "&")]
        public string LastCharter { get; set; }
        //[Parameter("会和Command组合起来发送到VA中", 2, CN = "产品码", CanRef = ParamRef.Ref)]
        //public string SN { get; set; }


        [Parameter("IO类型，通过关键字搜索", 6, CN = "相机触发IO", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice CameraDo { get; set; }

        [Parameter("关闭硬触发", 7, CN = "硬触发", DefaultV = false)]
        public bool IsHardTrigger { get; set; }

        [Parameter("空跑启用", 7, CN = "空跑启用", DefaultV = false)]
        public bool IsEmptyState { get; set; }

        [DependOn("IsHardTrigger", true)]
        [Parameter("IO类型，通过关键字搜索", 8, CN = "光源触发IO", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice LightDo { get; set; }

        [Parameter("输出参数", 9, CN = "复检结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [Parameter("输出参数", 10, CN = "XOffset", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double XPos { get; set; }

        [Parameter("输出参数", 11, CN = "YOffset", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double YPos { get; set; }

        [Parameter("输出参数", 12, CN = "UOffset", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double UPos { get; set; }

        [Parameter("输出参数", 13, CN = "XOffset1", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double XPos1 { get; set; }

        [Parameter("输出参数", 14, CN = "YOffset1", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double YPos1 { get; set; }

        [Parameter("输出参数", 15, CN = "UOffset1", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double UPos1 { get; set; }


        [Parameter("相机扫描的二维码", 16, CN = "二维码", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string BarCode { get; set; }

        [Parameter("AOI结果", 17, CN = "AOI结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool AOIResult { get; set; }

        [Parameter("Data数据", 18, CN = "Data数据", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string DataStrs { get; set; }


        [Parameter("图像地址", 19, CN = "图像地址", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string ImagePath { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public VA()
        {
            this.Icon = "\xe65b";
            this.Tips = "定位或者输出位置信息,输出的格式必须是 NG,XPos,YPos,UPos,XPos1,YPos1,UPos1,Barcode,AOIResult,DataStrs,ImagePath";
            this.Range = new LRange(-200, 200);
        }

        /// <summary>
        /// 函数
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            GetVDevice<VIO>(LightDo, out var lightDo);
            GetVDevice<VCommuncation>(CommDevice, out var vComm);
            vComm.SetProtocol(Luster.Motion.DataStruct.Enums.ProtocolType.StringDefault);

            vComm.Open();

            List<string> results = new List<string>();
            for (int i = 0; i < LoopNum; i++)
            {
                // 如果NG就循环运行
                if (!IsNG(vComm, i + 1, out results))
                {
                    break;
                }
                else
                {
                    // 如果结果NG，尝试开启光源，再次拍摄
                    if (lightDo != null)
                    {
                        lightDo.SetDigital(true);
                        System.Threading.Thread.Sleep(100);
                    }

                    MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias} 第 {i + 1} 次结果NG");
                    if (i >= LoopNum - 1)
                    {
                        if (results.Count > 0)
                        {
                            if (results.ToString().ToUpper().Contains("NG")) break;
                        }
                        OnAlarm(AlarmType.Timeout, $"VA执行超时", "NOOOO-01-10@va time out");
                    }

                }
            }

            // 如果是OK，代表True
            if (results.Count > 0)
            {
                Result = results[0].ToUpper() == "OK";
            }

            if (results.Count > 3)
            {
                if (double.TryParse(results[1], out var x))
                {
                    XPos = x;

                    if (!Range.IsSatisfy(x))
                    {
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"VA结果XPos={x} 超出防呆范围:{Range.Min}-{Range.Max}");
                        XPos = 0;
                    }
                }

                if (double.TryParse(results[2], out var y))
                {
                    YPos = y;
                    if (!Range.IsSatisfy(y))
                    {
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"VA结果YPos={y} 超出防呆范围:{Range.Min}-{Range.Max}");
                        YPos = 0;
                    }
                }

                if (double.TryParse(results[3], out var z))
                {
                    UPos = z;
                    if (!Range.IsSatisfy(z))
                    {
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"VA结果UPos={z} 超出防呆范围:{Range.Min}-{Range.Max}");
                        UPos = 0;
                    }
                }
            }

            if (results.Count > 6)
            {
                if (double.TryParse(results[4], out var x))
                {
                    XPos1 = x;
                    if (!Range.IsSatisfy(x))
                    {
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"VA结果XPos1={x} 超出防呆范围:{Range.Min}-{Range.Max}");
                        XPos1 = 0;
                    }
                }

                if (double.TryParse(results[5], out var y))
                {
                    YPos1 = y;
                    if (!Range.IsSatisfy(y))
                    {
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"VA结果YPos1={y} 超出防呆范围:{Range.Min}-{Range.Max}");
                        YPos1 = 0;
                    }
                }

                if (double.TryParse(results[6], out var z))
                {
                    UPos1 = z;
                    if (!Range.IsSatisfy(z))
                    {
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Error, $"VA结果UPos1={z} 超出防呆范围:{Range.Min}-{Range.Max}");
                        UPos1 = 0;
                    }
                }
            }

            if (results.Count > 7)
            {
                BarCode = results[7];
            }

            if (results.Count > 8)
            {
                AOIResult = results[8].ToUpper() == "OK";
            }

            DataStrs = "";
            if (results.Count > 9)
            {
                for (int i = 9; i < results.Count; i++)
                {
                    DataStrs += results[i].ToString() + ",";
                }
            }
            DataStrs = DataStrs.TrimEnd(',');

            //第10位为图片位置
            if (results.Count > 10)
            {
                ImagePath = results[10];
            }


            // 拍照结束关闭光源
            if (lightDo != null)
            {
                lightDo.SetDigital(false);
            }

            //判断是否是硬触发
            if (IsHardTrigger)
            {
                Task.Run(() =>
                {
                    GetVDevice<VIO>(CameraDo, out var cameraDo);
                    //关闭相机硬触发
                    if (cameraDo != null)
                    {
                        cameraDo.SetDigital(false);
                    }
                });
            }

            return base.DoExcute(out errMsg);
        }

        private bool IsNG(VCommuncation vComm, int Count, out List<string> vaResult)
        {
            vaResult = new List<string>();

            try
            {
                // 清理缓存
                vComm.ClearCache();

                // 发送命令
                // 自动补齐10位发送内容，共9个逗号,其中第10位为3A次数
                string sendContent = Command.GetString(MyOwner);
                int addCharCount = 8 - sendContent.Where(x => x == ',').Count();
                if (addCharCount > 0)
                {
                    for (int i = 0; i < addCharCount; i++)
                    {
                        sendContent = sendContent + ",";
                    }
                }

                // 第10位是发送的次数 第11位是机台StationID
                vComm.Write($"{sendContent},{Count},{MyOwner.ConfigManager.GetWebConfig("StationID")}");

                // 空跑模式直接返回
                if (IsEmptyMode && !IsEmptyState)
                {
                    vaResult = new List<string> { "OK", "0", "0", "0", "0", "0", "0", "EmptyMode", "OK" };
                    return false;
                }

                // 等待结果
                string socketResult = vComm.ReadSingle<string>("", OverTime);

                if (string.IsNullOrEmpty(LastCharter))
                {
                    LastCharter = "";
                }

                if (string.IsNullOrEmpty(socketResult))
                {
                    socketResult = "";
                    OnAlarm(AlarmType.Timeout, $"VA执行超时", "NOOOO-01-10@va time out");
                }

                var datas = socketResult.Split(LastCharter.ToCharArray());

                // 取最后一个
                string lastResult = datas.LastOrDefault(u => !string.IsNullOrEmpty(u));
                if (lastResult != null)
                {
                    MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"{MyOwner.Alias} 原始结果:{string.Join(",", datas)},使用结果:{lastResult}");

                    vaResult = lastResult.Split(',').ToList();
                    return vaResult[0] == "NG";
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                vaResult = new List<string>() { "NG" };
                MyOwner.OnLog(LogType.Debug, $"VA.IsNG()执行时发生异常");
                OnAlarm(AlarmType.DeviceError, $"VA执行超时", "NOOOO-01-10@va time out");
                return true;
            }
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