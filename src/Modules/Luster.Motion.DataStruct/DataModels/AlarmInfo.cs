#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmInfo
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       AlarmInfo.cs
* 创建时间:       2022/9/17 22:24:01
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dc73deaa-515b-4c8c-b4d9-93aba9d6b63c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/17 22:24:01
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 警告
    /// </summary>
    public class AlarmInfo
    {
        /// <summary>
        /// 报警唯一标识
        /// </summary>
        public long AlarmID { get; set; } = -1;

        /// <summary>
        /// 图表
        /// </summary>
        public string AlarmIcon { get; set; }

        /// <summary>
        /// 触发
        /// </summary>
        public object Sender { get; set; }

        /// <summary>
        /// 报警时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 处理方式
        /// </summary>
        public string ProcMethod { get; set; }

        /// <summary>
        /// 报警时长(s)
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// 警告类型
        /// </summary>
        public AlarmType AlarmType { get; set; }

        /// <summary>
        /// 报警包含对应的ErrorCode
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 报警代码
        /// </summary>
        public string AlarmCode { get; set; }

        /// <summary>
        /// 机台的原始状态
        /// </summary>
        public EngineStatus EStatus { get; set; }

        /// <summary>
        /// 报警元件所属模组
        /// </summary>
        public string Module;

        /// <summary>
        /// 报警元件所属模组
        /// </summary>
        public string Name;

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="alarmType"></param>
        /// <param name="message"></param>
        /// <param name="alarmCode"></param>
        /// <param name="alarmProcAction"></param>
        public AlarmInfo(object sender, AlarmType alarmType, string message, string alarmCode,string module="",string name="")
        {
            Sender = sender;
            AlarmType = alarmType;
            Message = message;
            AlarmCode = alarmCode;
            StartTime = DateTime.Now;
            Module = module;
            Name = name;

            switch (alarmType)
            {
                case AlarmType.InfoTip:
                    AlarmIcon = "\xe6c5";
                    break;
                case AlarmType.WarningTip:
                case AlarmType.Timeout:
                    AlarmIcon = "\xe6c2";
                    break;
                case AlarmType.FailError:
                case AlarmType.DeviceError:
                    AlarmIcon = "\xea96";
                    break;
                case AlarmType.PlcAlarm:
                    AlarmIcon = "\xe68c";
                    break;
            }
        }

        public bool OnAlarmProc(AlarmProc alarmProc)
        {
            if (AlarmProcEvent == null)
            {
                return true;
            }
            else
            {
                return AlarmProcEvent.Invoke(Sender, alarmProc);
            }
        }

        public event Func<object, AlarmProc, bool> AlarmProcEvent;

    }
}