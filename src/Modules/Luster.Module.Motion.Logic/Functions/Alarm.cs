#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Alarm
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       Alarm.cs
* 创建时间:       2022/8/11 13:47:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5f25c61b-a560-408c-9f25-d8b6dd82df8c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/11 13:47:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace Luster.Module.Motion.Logic.Functions
{
    /// <summary>
    /// 报警支持的报警方式
    /// </summary>
    public enum AlarmMethod
    {
        [Description("结果判断")]
        Normal,

        [Description("表达式判断")]
        Condition,

        [Description("清除报警")]
        Clear
    }

    public class AlarmInfo
    {
        [Description("故障代码")]
        public string ErrorCode { get; set; }

        [Description("故障信息")]
        public string ErrorMessage { get; set; }

        [Description("故障描述")]
        public string ErrorDetail { get; set; }
    }

    /// <summary>
    /// 报警
    /// </summary>
    public class Alarm : Group, IPauseFunction, IHomeFunction, IAlarm, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 报警方式
        /// </summary>
        [Parameter("报警条件/报警次数", 0, CN = "报警方式")]
        public AlarmMethod Method { get; set; }

        /// <summary>
        /// 是否发生报警
        /// </summary>
        [DependOn("Method", AlarmMethod.Normal)]
        [Parameter("是否报警，如果为true,就报警，如果为false，继续进行", 1, CN = "是否报警", CanRef = ParamRef.Ref)]
        public bool IsAlarm { get; set; }

        /// <summary>
        /// 条件报警
        /// </summary>
        [DependOn("Method", AlarmMethod.Condition)]
        [Parameter("是否报警，如果为true,就报警，如果为false，继续进行", 2, CN = "报警条件")]
        public override LExpression Express { get; set; }

        /// <summary>
        /// 连续报警次数
        /// </summary>
        [Range(-1, 100)]
        [DependOn("Method", AlarmMethod.Condition)]
        [Parameter("连续出现N次异常进行报警,如果值为-1,则立即报警", 3, CN = "连续报警次数", DefaultV = -1)]
        public int AlarmNum { get; set; }


        /// <summary>
        /// 固定时间报警次数
        /// </summary>
        [Range(-1, 100)]
        [DependOn("Method", AlarmMethod.Condition)]
        [Parameter("设定时间内出现N次异常进行报警", 3, CN = "固定时间报警次数", DefaultV = -1)]
        public int FixTimeAlarmNum { get; set; }

        /// <summary>
        /// 固定时间
        /// </summary>
        [Range(-1, 100)]
        [DependOn("Method", AlarmMethod.Condition)]
        [Parameter("固定时间，单位为分钟", 3, CN = "固定时间", DefaultV = -1)]
        public int FixTime { get; set; }


        /// <summary>
        /// 报警类型
        /// </summary>
        [DependOn("Method", AlarmMethod.Condition, AlarmMethod.Normal)]
        [Parameter("报警类型", 4, CN = "报警类型")]
        public AlarmType AlarmType { get; set; }

        /// <summary>
        /// 报警内容
        /// </summary>
        private string _alarmCode;
        [Parameter("报警代码", 5, CN = "报警代码", DefaultV = "TBD", IsReadOnly = true)]
        public string AlarmCode 
        { 
            get => _alarmCode; 
            set 
            {
                if (_alarmCode != value)
                {
                    _alarmCode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlarmCode)));
                }
            }
        }

        /// <summary>
        /// 报警内容
        /// </summary>
        private string _message;
        [Parameter("报警内容", 6, CN = "报警内容", IsReadOnly = true)]
        public string Message 
        { 
            get => _message; 
            set 
            {
                if (_message != value)
                {
                    _message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }

        private string _detail;
        [Parameter("报警英文", 7, CN = "报警英文", IsReadOnly = true)]
        public string Detail 
        { 
            get => _detail; 
            set 
            {
                if (_detail != value)
                {
                    _detail = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Detail)));
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            UpdateFromVAlarm();
        }

        public override void Added()
        {
            base.Added();
            UpdateFromVAlarm();
            UpdateReadOnlyState();
            if (MyOwner?.DeviceEngine != null)
            {
                MyOwner.DeviceEngine.VDeviceChangedEvent -= DeviceEngine_VDeviceChangedEvent;
                MyOwner.DeviceEngine.VDeviceChangedEvent += DeviceEngine_VDeviceChangedEvent;
                MyOwner.DeviceEngine.AlarmCodeChangedEvent -= DeviceEngine_AlarmCodeChangedEvent;
                MyOwner.DeviceEngine.AlarmCodeChangedEvent += DeviceEngine_AlarmCodeChangedEvent;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (MyOwner?.DeviceEngine != null)
            {
                MyOwner.DeviceEngine.VDeviceChangedEvent -= DeviceEngine_VDeviceChangedEvent;
                MyOwner.DeviceEngine.AlarmCodeChangedEvent -= DeviceEngine_AlarmCodeChangedEvent;
            }
            base.Dispose(disposing);
        }

        private void DeviceEngine_AlarmCodeChangedEvent(string oldCode, string newCode)
        {
            if (AlarmCode == oldCode)
            {
                // 检查是否存在按模块 ID 匹配的 VAlarm（同名报警工具独立配置场景）
                var allAlarms = MyOwner?.DeviceEngine?.GetVDevices<VAlarm>();
                if (allAlarms != null && allAlarms.Any(a => a.DeviceID != Guid.Empty))
                {
                    // 有按模块绑定的 VAlarm：仅当本模块的 VAlarm 被修改时才响应
                    var myAlarm = allAlarms.FirstOrDefault(a => a.AlarmKey == newCode && a.DeviceID == MyOwner.ID);
                    if (myAlarm == null)
                        return; // 不是本模块的 VAlarm 变更，跳过
                }

                AlarmCode = newCode;
                if (MyOwner != null && MyOwner.Parameters.ContainsKey(nameof(AlarmCode)))
                {
                    var pCode = MyOwner.Parameters[nameof(AlarmCode)];
                    if (pCode.Value?.ToString() != AlarmCode) pCode.Value = AlarmCode;
                }
                UpdateFromVAlarm();
            }
        }

        private void DeviceEngine_VDeviceChangedEvent()
        {
            UpdateFromVAlarm();
        }

        private void UpdateFromVAlarm()
        {
            if (MyOwner != null && MyOwner.DeviceEngine != null && !string.IsNullOrEmpty(AlarmCode))
            {
                var allAlarms = MyOwner.DeviceEngine.GetVDevices<VAlarm>();
                // 优先按 AlarmKey + DeviceID 匹配（支持同名报警工具独立配置）
                var alarm = allAlarms?.FirstOrDefault(a => a.AlarmKey == AlarmCode && a.DeviceID == MyOwner.ID);
                // 回退到仅按 DeviceID 匹配（处理 AlarmKey 不一致的情况）
                if (alarm == null && MyOwner.ID != Guid.Empty)
                    alarm = allAlarms?.FirstOrDefault(a => a.DeviceID == MyOwner.ID && a.Name != "ProEvent");
                // 回退到仅按 AlarmKey 匹配（兼容旧数据）
                if (alarm == null)
                    alarm = allAlarms?.FirstOrDefault(a => a.AlarmKey == AlarmCode && a.DeviceID == Guid.Empty);
                if (alarm == null)
                    alarm = allAlarms?.FirstOrDefault(a => a.AlarmKey == AlarmCode);

                if (alarm != null)
                {
                    // 当通过 DeviceID 回退找到 VAlarm 时，同步修正 AlarmCode
                    if (AlarmCode != alarm.AlarmKey)
                    {
                        AlarmCode = alarm.AlarmKey;
                        if (MyOwner.Parameters.ContainsKey(nameof(AlarmCode)))
                        {
                            var pCode = MyOwner.Parameters[nameof(AlarmCode)];
                            if (pCode.Value?.ToString() != AlarmCode) pCode.Value = AlarmCode;
                        }
                    }

                    Message = alarm.AlarmCN;
                    Detail = alarm.AlarmEn;

                    if (MyOwner.Parameters.ContainsKey(nameof(Message)))
                    {
                        var pMessage = MyOwner.Parameters[nameof(Message)];
                        if (pMessage.Value?.ToString() != Message) pMessage.Value = Message;
                    }
                    if (MyOwner.Parameters.ContainsKey(nameof(Detail)))
                    {
                        var pDetail = MyOwner.Parameters[nameof(Detail)];
                        if (pDetail.Value?.ToString() != Detail) pDetail.Value = Detail;
                    }

                    var newAlarmC = new VAlarm();
                    newAlarmC.ID = alarm.ID;
                    newAlarmC.AlarmKey = alarm.AlarmKey;
                    newAlarmC.AlarmCN = alarm.AlarmCN;
                    newAlarmC.AlarmEn = alarm.AlarmEn;

                    AlarmC = newAlarmC;

                    if (MyOwner.Parameters.ContainsKey(nameof(AlarmC)))
                    {
                        var pAlarmC = MyOwner.Parameters[nameof(AlarmC)];
                        if (pAlarmC.Value != AlarmC)
                        {
                            pAlarmC.Value = AlarmC;
                        }
                    }
                }
            }
        }

        [Parameter("对哪个灯进行开启", 8, CN = "开灯类型", DefaultV = LightType.None)]
        public LightType LightType { get; set; }

        [Parameter("是否开启开启蜂鸣器,如果为否，则关闭蜂鸣器", 9, CN = "开启蜂鸣器", DefaultV = false)]
        public bool IsBuzzer { get; set; }

        [DependOn("IsBuzzer", true)]
        [Parameter("中断灯闪烁的Io数量", 10, CN = "蜂鸣器间隔", DefaultV = 200)]
        public int Interval { get; set; }

        //private VAlarm alarmC;
        [Parameter("VAlarm类型，通过关键字搜索", 4, CN = "报警代码", EditorType = typeof(VAlarm))]
        public VAlarm AlarmC { get; set; }

        /// <summary>
        /// 记录连续报警次数
        /// </summary>
        private static Dictionary<string, int> alarmDict = new Dictionary<string, int>();

        /// <summary>
        /// 记录固定时间内多次报警次数
        /// </summary>
        private static Dictionary<string, List<DateTime>> alarmDict2 = new Dictionary<string, List<DateTime>>();


        private static List<AlarmInfo> errorInfos = new List<AlarmInfo>();

        /// <summary>
        /// 报警方式
        /// </summary>
        public Alarm()
        {
            this.Tips = "通过报警，来暂停流程";
            this.Icon = "\xe871";
        }

        public override bool DoExcute(out string errMsg)
        {


            errMsg = "";

            bool isSuccess = true;

            // 先执行内部模块，接着再判断变量
            if (MyOwner.Children.Count > 0)
            {
                // 1.获取Start
                var startModule = MyOwner.Children[0];

                // 2.程序运行
                motionRunEngine.Run(startModule, ref isSuccess);

                // 3.运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                }
            }

            if (isSuccess)
            {
                bool isAlarm = IsAlarm;
                switch (Method)
                {
                    case AlarmMethod.Normal:
                        break;
                    case AlarmMethod.Condition:
                        if (Express == null)
                        {
                            errMsg = "条件表达式不能为空!";
                            return false;
                        }
                        isAlarm = Express.GetResult(MyOwner);

                        if (AlarmNum > -1)
                        {
                            if (isAlarm)
                            {
                                // 此时更新为false
                                isAlarm = false;
                                if (!alarmDict.ContainsKey(AlarmCode))
                                {
                                    alarmDict.Add(AlarmCode, 0);
                                }

                                //固定时间内多次
                                if (!alarmDict2.ContainsKey(AlarmCode)&& FixTimeAlarmNum>0)
                                {
                                    alarmDict2.Add(AlarmCode, new List<DateTime>());
                                    alarmDict2[AlarmCode].Add(DateTime.Now);
                                }
                                else if (alarmDict2.ContainsKey(AlarmCode))
                                {
                                    alarmDict2[AlarmCode].Add(DateTime.Now);
                                    if (FixTime > 0 && FixTimeAlarmNum > 0)
                                    {
                                        if (alarmDict2[AlarmCode].Count >= FixTimeAlarmNum)
                                        {
                                            if ((alarmDict2[AlarmCode][alarmDict2[AlarmCode].Count-1] - alarmDict2[AlarmCode][0]).TotalMinutes >= FixTime)
                                            {
                                                //ExchangeAlarmInfo(AlarmCode, out string AlarmCodeNew, out string MessageNew);
                                                var text = Detail == null ? "Not config" : Detail;
                                                var message = Message == null ? "未配置" : Message;
                                                OnAlarm(AlarmType, message, $"{AlarmCode}@{text}");
                                                alarmDict2.Remove(AlarmCode);
                                                MyOwner.LightManager.OpenLight(LightType, IsBuzzer, Interval);
                                            }
                                            else
                                            {
                                                alarmDict2[AlarmCode].RemoveAt(0);
                                            }

                                        }

                                    }

                                }

                                //连续多次
                                alarmDict[AlarmCode]++;
                                if (alarmDict[AlarmCode] >= AlarmNum)
                                {
                                    alarmDict.Remove(AlarmCode);
                                    //ExchangeAlarmInfo(AlarmCode, out string AlarmCodeNew, out string MessageNew);

                                    //OnAlarm(AlarmType, MessageNew, AlarmCodeNew);
                                    var text = Detail == null ? "Not config" : Detail;
                                    var message = Message == null ? "未配置" : Message;
                                    OnAlarm(AlarmType, message, $"{AlarmCode}@{text}");
                                    MyOwner.LightManager.OpenLight(LightType, IsBuzzer, Interval);

                                }
                            }
                            else
                            {
                                // 第二次没有报警，就给记录清除掉
                                if (alarmDict.ContainsKey(AlarmCode))
                                {
                                    alarmDict.Remove(AlarmCode);
                                }
                            }
                            isAlarm = false;
                        }
                        break;
                    case AlarmMethod.Clear:
                        if (alarmDict.ContainsKey(AlarmCode))
                        {
                            alarmDict.Remove(AlarmCode);
                        }
                        isAlarm = false;
                        break;
                }

                if (isAlarm)
                {
                    if (string.IsNullOrEmpty(Message))
                    {
                        Message = $"{MyOwner.Alias}";
                    }
                    //ExchangeAlarmInfo(AlarmCode, out string AlarmCodeNew, out string MessageNew);
                    //OnAlarm(AlarmType, MessageNew, AlarmCodeNew);
                    var text = Detail == null ? "Not config" : Detail;
                    var message = Message == null ? "未配置" : Message;
                    OnAlarm(AlarmType, message, $"{AlarmCode}@{text}");
                    MyOwner.LightManager.OpenLight(LightType, IsBuzzer, Interval);
                }
                else
                {
                    SetSkip(false);
                }

            }

            return isSuccess;
        }

        /// <summary>
        /// 回零的时候清除记录
        /// </summary>
        public override void Home()
        {
            alarmDict.Clear();
            //LoadAlarmInfoFromXml();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public override void Pause()
        {
            SetSkip(true);
        }

        private void SetSkip(bool isSkip)
        {
            if (MyOwner.Children.Count > 0)
            {
                SetSkipWait(isSkip);

                foreach (var item in MyOwner.Children)
                {
                    if (item.TaskFunction is LogicFunction logicFunc)
                    {
                        logicFunc.SetSkipWait(isSkip);
                    }
                }
            }
        }



        private void LoadAlarmInfoFromXml()
        {
            errorInfos.Clear();

            //读取csv文件
            string src = Path.Combine(MyOwner.DeviceEngine.RecipeConfigPath, "ProcessErrors.csv");
            try
            {
                using (var fileStream = new FileStream(src, FileMode.Open, FileAccess.Read))
                {
                    StreamReader read = new StreamReader(fileStream, Encoding.Default);
                    string line;
                    while ((line = read.ReadLine()) != null)
                    {
                        var data = line.Split(',');
                        if (data.Length >=3)
                        {
                            var xml_Alarm = new AlarmInfo();
                            xml_Alarm.ErrorCode = data[0];
                            xml_Alarm.ErrorMessage = data[1];
                            xml_Alarm.ErrorDetail = data[2];
                            errorInfos.Add(xml_Alarm);
                        }
                    }
                    read.Close();
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
            }

            //加载到内存中


            /*XmlDocument xmlDocument = new XmlDocument();
            string src = Path.Combine(MyOwner.DeviceEngine.RecipeConfigPath, "ProcessErrors.xml");
            if (!File.Exists(src))
            {
                File.Create(src).Close();
                AlarmInfo alarmModel = new AlarmInfo
                {
                    ErrorCode = "O99OOOO-01",
                    ErrorMessage = "Track temperature alarm",
                    ErrorDetail = "轨道温度报警",
                };


                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                XmlElement rootElement = xmlDoc.CreateElement("Errors");
                XmlElement childElement = xmlDoc.CreateElement("Error");
                childElement.SetAttribute("ErrorCode", alarmModel.ErrorCode);
                childElement.SetAttribute("ErrorMessage", alarmModel.ErrorMessage);
                childElement.SetAttribute("ErrorDetail", alarmModel.ErrorDetail);
                rootElement.AppendChild(childElement);
                xmlDoc.AppendChild(rootElement);
                xmlDoc.Save(src);

            }
            xmlDocument.Load(src);
            XmlNode rootNode = xmlDocument.DocumentElement;

            foreach (XmlNode alramNode in rootNode.ChildNodes)
            {
                var xml_Alarm = new AlarmInfo();
                xml_Alarm.ErrorCode = alramNode.Attributes["ErrorCode"].Value;
                xml_Alarm.ErrorMessage = alramNode.Attributes["ErrorMessage"].Value;
                xml_Alarm.ErrorDetail = alramNode.Attributes["ErrorDetail"].Value;
                errorInfos.Add(xml_Alarm);
            }*/
        }


        private void ExchangeAlarmInfo(string errcode, out string combineErrcode, out string message)
        {
            combineErrcode = string.Empty;
            message = string.Empty;
            foreach (var alr in errorInfos)
            {

                if (alr.ErrorCode==errcode)
                {
                    combineErrcode = $"{errcode}@{alr.ErrorMessage}";
                    message= alr.ErrorDetail;
                    return;
                }
            }

            //提供没有配置的默认值
            combineErrcode = $"{errcode}@Not config";
            message = "未配置";
        }


        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(AlarmC))
            {
                if (newV is VAlarm alarm)
                {
                    AlarmCode = alarm.AlarmKey;
                    Message = alarm.AlarmCN;
                    Detail = alarm.AlarmEn;
                }
                else if (newV == null || newV.ToString() == string.Empty)
                {
                    AlarmCode = null;
                    Message = null;
                    Detail = null;
                }

                if (MyOwner != null)
                {
                    try
                    {
                        if (MyOwner.Parameters.TryGetValue(nameof(AlarmCode), out var pCode))
                            if (pCode.Value?.ToString() != AlarmCode) pCode.Value = AlarmCode;

                        if (MyOwner.Parameters.TryGetValue(nameof(Message), out var pMessage))
                            if (pMessage.Value?.ToString() != Message) pMessage.Value = Message;

                        if (MyOwner.Parameters.TryGetValue(nameof(Detail), out var pDetail))
                            if (pDetail.Value?.ToString() != Detail) pDetail.Value = Detail;
                    }
                    catch { }
                }
            }

            if (parameter.Name == nameof(AlarmType))
            {
                UpdateReadOnlyState();
            }
        }

        /// <summary>
        /// 根据报警类型动态更新 报警代码/报警内容/报警英文 的只读状态。
        /// 信息提示、报警断点、人工介入提示 允许手动编辑，其他类型为只读。
        /// </summary>
        private void UpdateReadOnlyState()
        {
            // 优先从 ParameterAttribute.Value 获取 AlarmType，
            // 因为 XML 加载时自动属性尚未同步，仍为默认值 InfoTip
            AlarmType currentType = this.AlarmType;
            if (MyOwner?.Parameters.TryGetValue(nameof(AlarmType), out var pType) == true
                && pType.Value is AlarmType at)
            {
                currentType = at;
            }

            bool isEditable = currentType == AlarmType.InfoTip ||
                              currentType == AlarmType.RetryAlarm ||
                              currentType == AlarmType.ManuOperationAlarm;

            if (MyOwner != null)
            {
                if (MyOwner.Parameters.TryGetValue(nameof(AlarmCode), out var pCode))
                    pCode.IsReadOnly = !isEditable;
                if (MyOwner.Parameters.TryGetValue(nameof(Message), out var pMsg))
                    pMsg.IsReadOnly = !isEditable;
                if (MyOwner.Parameters.TryGetValue(nameof(Detail), out var pDetail))
                    pDetail.IsReadOnly = !isEditable;
            }
        }


    }
}