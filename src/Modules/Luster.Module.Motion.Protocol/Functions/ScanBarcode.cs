#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ScanBarcode
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       ScanBarcode.cs
* 创建时间:       2022/6/29 17:30:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e5d1e94b-5257-40d0-9206-7cf7bcd91814
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/29 17:30:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    public enum StringEncoding
    {
        Default,
        UTF8,
    }

    /// <summary>
    /// 扫码抢
    /// </summary>
    public class ScanBarcode : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("字符串格式", 1, CN = "编码格式", DefaultV = StringEncoding.Default)]
        public StringEncoding StringEncoding { get; set; }

        //[Parameter("持续等待线圈值", 2, CN = "NG结果", DefaultV = "NG")]
        //public string NGResult { get; set; }

        [Parameter("二维码长度,不符合长度扫码NG", 2, CN = "最小长度", DefaultV = 23)]
        public int SNLength { get; set; }

        [Parameter("二维码偏差范围", 2, CN = "偏差范围", DefaultV = 5)]
        public int Range { get; set; }


        [Parameter("扫码NG重复扫码次数", 3, CN = "扫码次数", DefaultV = 3)]
        public int RescanNum { get; set; }

        [Parameter("分隔符", 4, CN = "分隔符", DefaultV = ';')]
        public string Separator { get; set; }

        [Parameter("第几个二维码", 5, CN = "二维码序号", DefaultV = 0)]
        public int BarcodeNum { get; set; }


        [Parameter("扫码屏蔽", 6, CN = "扫码屏蔽", DefaultV = false, CanRef = ParamRef.Ref)]
        public bool IsShield { get; set; }

        [Parameter("超时时间,单位ms", 7, CN = "超时时间", DefaultV = 2000)]
        public int Overtime { get; set; }


        [Parameter("启动手动输码", 10, CN = "手动输码")]
        public bool IsHandle { get; set; }

        [DependOn("IsHandle", true)]
        [Parameter("手动输入条码", 11, CN = "固定条码")]
        public string FixBarCode { get; set; }

        [Parameter("多次扫码间隔", 12, CN = "多次扫码间隔", DefaultV = 50)]
        public int InterVal { get; set; }

        [Parameter("是否需要匹配", 13, CN = "匹配", DefaultV = false)]
        public bool IsMatch { get; set; }

        [DependOn("IsMatch", true)]
        [Parameter("匹配字符串", 14, CN = "匹配字符串", DefaultV = "VAN")]
        public string MatchStr { get; set; }

        [Parameter("唯一码", 15, CN = "唯一码", DefaultV = "")]
        public string UniqueCode { get; set; }

        [Parameter("扫码枪二维码", 7, CN = "二维码", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string BarCode { get; set; }

        [Parameter("最终扫码是否成功", 8, CN = "扫码结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool ScanResult { get; set; }

        [Parameter("最终扫码扫码次数", 9, CN = "扫码次数", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int ScanNum { get; set; }

        [Parameter("空跑测试扫码", 16, CN = "空跑扫码", DefaultV = false)]
        public bool IsEmptyModeScan { get; set; }


        private int scanNum = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ScanBarcode()
        {
            this.Tips = "用于二维码扫码";
            this.Icon = "\xe683";
            ScanNum = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            BarCode = "None";
            ScanResult = true;
            scanNum = 0;

            if (IsEmptyMode || IsShield)
            {
                if (IsEmptyModeScan == false)
                {
                    BarCode = $"NG_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{UniqueCode}";
                    ScanNum = 1;
                    return true;
                }
            }

            // 手动输入条码
            if (IsHandle)
            {
                if (string.IsNullOrEmpty(FixBarCode))
                {
                    OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.WarningTip, "固定条码为空!");
                    return true;
                }

                BarCode = FixBarCode.Trim();
                ScanNum = 1;
                return true;
            }

            GetVDevice<VCommuncation>(CommDevice, out var comm);

            if (comm.Protocol == null)
            {
                IProtocol protocol = null;

                List<byte> commands = new List<byte>();
                switch (StringEncoding)
                {
                    case StringEncoding.UTF8:
                        protocol = new StringProtocol();
                        break;
                    case StringEncoding.Default:
                        protocol = new StringProtocol();
                        break;
                }

                comm.Protocol = protocol;
            }


            for (int i = 0; i < RescanNum; i++)
            {
                scanNum = i + 1;

                // 1.写入T
                comm.Open();
                //清除缓存
                comm.ClearCache();
                comm?.Write("T");

                // 2.接收结果
                List<string> strResults = new List<string>();
                try
                {
                    strResults = comm?.Read<string>("", Overtime);
                }
                catch (Exception dx)
                {
                    MyOwner.OnLog(LogType.Debug, $"通讯:{comm?.ToString()} 返回结果异常!");
                    ScanResult = false;
                    //continue;
                }

                if (IsEmptyMode || IsShield)
                {
                    if (IsEmptyModeScan)
                    {
                        BarCode = $"NG_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{UniqueCode}";
                        ScanNum = 1;
                        MyOwner.OnLog(LogType.Debug, $"通讯:{comm?.ToString()} 返回结果异常!，使用自定义码");
                        return true;
                    }
                }

                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"ScanResult:{string.Join(",", strResults)}");
                if (strResults != null && strResults.Count > 0)
                {
                    BarCode = strResults[0];
                }

                //3.判断是否有多个二维码
                if (BarcodeNum != 0)
                {
                    //判断是否包含分隔符
                    if (BarCode.Contains(Separator))
                    {
                        BarCode = BarCode.Split(Separator.ToCharArray()).ToArray()[BarcodeNum - 1];
                    }
                }

                // barCode为空,代表算法没有返回结果，直接跳出循环
                if (string.IsNullOrEmpty(BarCode) || BarCode == "None")
                {
                    // 接收超时，直接NG处理
                    BarCode = $"NG_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{UniqueCode}";
                    ScanResult = false;
                    OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"扫码枪3s内未返回结果!", "T01OOOO-01");
                    break;
                }

                // 判断是否开启匹配功能
                if (IsMatch)
                {
                    if (!BarCode.Contains(MatchStr))
                    {
                        ScanResult = false;
                        OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"此二维码不应该出现在这个线体中!");
                        break;
                    }
                }


                // 条码长度小于最小长度-范围 or 长度大于最小长度+范围
                if (BarCode.Trim().Length < (SNLength-Range)|| BarCode.Trim().Length>(SNLength + Range))
                {
                    ScanResult = false;

                    //达到设定次数再报警提示
                    if (i >= RescanNum - 1)
                    {
                        if (BarCode.Contains("\r") || BarCode.Contains("\n"))
                        {
                            OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"扫码异常:条码:{BarCode}包含换行和回车!", "T01OOOO-01");
                        }
                        else
                        {
                            OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"扫码异常:条码:{BarCode} 长度小于:{SNLength}!", "T01OOOO-01");
                        }
                    }

                    BarCode = $"NG_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{UniqueCode}";
                }
                //包含回车、换行
                else if (BarCode.Contains("\r") || BarCode.Contains("\n"))
                {
                    ScanResult = false;
                    //达到设定次数再报警提示
                    if (i >= RescanNum - 1)
                    {
                        OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"扫码异常:条码:{BarCode}包含换行和回车!", "T01OOOO-01");
                    }
                    BarCode = $"NG_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{UniqueCode}";
                }
                else
                {
                    ScanResult = true;
                    break;
                }
                //多次扫码间隔
                Thread.Sleep(InterVal);
            }

            ScanNum = scanNum;
            return base.DoExcute(out errMsg);
        }
    }
}