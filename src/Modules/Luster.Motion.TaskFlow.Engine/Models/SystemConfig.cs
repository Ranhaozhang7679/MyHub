#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SystemConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       SystemConfig.cs
* 创建时间:       2022/7/12 10:00:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f7a39ee0-76dd-4970-973f-357d4c803b15
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 10:00:57
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class SystemConfig : IXMLParser
    {
        /// <summary>
        /// IO 通用处理
        /// </summary>
        /// <param name="io"></param>
        private void ProcessIO(IDeviceEngine deviceEngine, VDevice vDevice, Action<VIO> ioAction)
        {
            // 启动按钮
            if (vDevice != null && !vDevice.IsEmpty())
            {
                var io = vDevice.GetVDevice<VIO>(deviceEngine);
                ioAction(io);
            }
        }

        #region 按钮
        /// <summary>
        /// 急停
        /// </summary>
        public VDevice EmergencyButton { get; set; }

        /// <summary>
        /// 启动按钮
        /// </summary>
        public VDevice StartButton { get; set; }

        /// <summary>
        /// 暂停按钮
        /// </summary>
        public VDevice PauseButton { get; set; }

        /// <summary>
        /// 恢复按钮
        /// </summary>
        public VDevice RecoveryButton { get; set; }

        /// <summary>
        /// 停止按钮
        /// </summary>
        public VDevice StopButton { get; set; }

        /// <summary>
        /// 忽略按钮
        /// </summary>
        public VDevice IgnoreButton { get; set; }

        /// <summary>
        /// 重试按钮
        /// </summary>
        public VDevice RetryButton { get; set; }

        /// <summary>
        /// 手动按钮
        /// </summary>
        public VDevice ManualSwitchButton { get; set; }

        /// <summary>
        /// 烟雾报警器
        /// </summary>
        public VDevice SmokeAlarmDevice { get; set; }
        #endregion


        #region
        /// <summary>
        /// 门锁
        /// </summary>
        [Ignore]
        public List<DoorLockModel> ListOpenCloseDoor { get; set; }

        /// <summary>
        /// 稼动IO设置
        /// </summary>
        [Ignore]
        public List<SysOperateIOModel> ListSysOperateIO { get; set; }
        #endregion
        #region 按钮灯

        /// <summary>
        /// 启动按钮灯
        /// </summary>
        public VDevice LightStart { get; set; }

        /// <summary>
        /// 暂停按钮灯
        /// </summary>
        public VDevice LightPause { get; set; }

        /// <summary>
        /// 恢复按钮灯
        /// </summary>
        public VDevice LightRecovery { get; set; }

        /// <summary>
        /// 停止按钮灯
        /// </summary>
        public VDevice LightStop { get; set; }

        #endregion

        #region Plc服务器
        /// <summary>
        /// Plc服务器
        /// </summary>
        public VDevice PlcServer { get; set; }

        /// <summary>
        /// 启动地址
        /// </summary>
        public string StartAddr { get; set; }

        /// <summary>
        /// 暂停地址
        /// </summary>
        public string PauseAddr { get; set; }

        /// <summary>
        /// 回零地址
        /// </summary>
        public string HomeAddr { get; set; }

        /// <summary>
        /// 回零完成地址
        /// </summary>
        public string HomeFinishAddr { get; set; }

        /// <summary>
        /// 是否自动运行中
        /// </summary>
        public string AutoRunAddr { get; set; }

        /// <summary>
        /// 复位地址
        /// </summary>
        public string RecoveryAddr { get; set; }

        /// <summary>
        /// 停止地址
        /// </summary>
        public string StopAddr { get; set; }

        /// <summary>
        /// 空跑指令
        /// </summary>
        public string EmptyRunAddr { get; set; }

        /// <summary>
        /// 2000
        /// 状态地址 1.手动模式 2.自动模式  3.初始化中 4.初始化完成 5.运行中 6.设备暂停 7.报警中 8.急停中
        /// M7000 报警
        /// </summary>
        public string StatusAddr { get; set; }
        /// <summary>
        /// 4404 2/3为IDLE状态
        /// </summary>
        public string IdleStatusAddr { get; set; }

        /// <summary>
        /// plc回零中标志位
        /// </summary>
        public string HomingAddr { get; set; }


        /// <summary>
        /// 上游有料
        /// </summary>
        public string MainPrevHaveAddr { get; set; }
        /// <summary>
        /// 本站要料
        /// </summary>
        public string MainThisGetAddr { get; set; }
        /// <summary>
        /// 本站有料
        /// </summary>
        public string MainThisHaveAddr { get; set; }
        /// <summary>
        /// 下游要料
        /// </summary>
        public string MainNextGetAddr { get; set; }

        /// <summary>
        /// 上游有料
        /// </summary>
        public string SubPrevHaveAddr { get; set; }
        /// <summary>
        /// 本站要料
        /// </summary>
        public string SubThisGetAddr { get; set; }
        /// <summary>
        /// 本站有料
        /// </summary>
        public string SubThisHaveAddr { get; set; }
        /// <summary>
        /// 下游要料
        /// </summary>
        public string SubNextGetAddr { get; set; }


        /// <summary>
        /// 载具数量
        /// </summary>
        public string CarrierCountAddr { get; set; }

        /// <summary>
        /// 总载具数量
        /// </summary>
        public string TotalCarrierCountAddr { get; set; }

        /// <summary>
        /// 主流线载具数量
        /// </summary>
        public string MainCarrierCountAddr { get; set; }


        /// <summary>
        /// 物流线载具数量
        /// </summary>
        public string SubCarrierCountAddr { get; set; }

        /// <summary>
        /// 回流线载具数量
        /// </summary>
        public string BackCarrierCountAddr { get; set; }

        /// <summary>
        /// PC心跳
        /// </summary>
        public string PCHeartBeatAddr { get; set; }

        /// <summary>
        /// PC状态
        /// </summary>
        public string PCStatusAddr { get; set; }


        /// <summary>
        /// 三色灯状态地址
        /// </summary>
        public string TricolorStatusAddr { get; set; }

        /// <summary>
        /// 首件模式控制地址
        /// </summary>
        public string FirstPieceModeCommandAddr { get; set; }

        /// <summary>
        /// 首件模式状态地址
        /// </summary>
        public string FirstPieceModeStatusAddr { get; set; }



        /// <summary>
        /// 报警列表
        /// </summary>
        [Ignore]
        public List<PlcAlarm> PLCAlarmList { get; set; }


        /// <summary>
        /// 报警列表
        /// </summary>
        [Ignore]
        public List<DIY_Alarm> DIYAlarmList { get; set; }

        private VCommuncation plcCommuncation = null;

        /// <summary>
        /// 通用方法
        /// </summary>
        /// <param name="deviceEngine"></param>
        /// <param name="action"></param>
        private void PlcAction(IDeviceEngine deviceEngine, Action<VPlc> action)
        {
            if (PlcServer != null && !PlcServer.IsEmpty())
            {
                if (plcCommuncation == null || !plcCommuncation.Communication.IsConnected)
                {
                    var plcServer = PlcServer.GetVDevice<VPlc>(deviceEngine);
                    plcServer.Open();
                    action(plcServer);
                }
            }
        }

        /// <summary>
        /// Plc 启动
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcStart(IDeviceEngine deviceEngine)
        {
            // 启动之前要进行复位
            PlcRecovery(deviceEngine);

            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(StartAddr)) return;
                plc.Write<bool>(StartAddr, true, $"01 05 {StartAddr} 1");
                Thread.Sleep(200);

                // 指令复位
                plc.Write<bool>(StartAddr, false, $"01 05 {StartAddr} 1");
            });
        }

        /// <summary>
        /// Plc暂停
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcPause(IDeviceEngine deviceEngine)
        {
            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(PauseAddr)) return;
                plc.Write<bool>(PauseAddr, true, $"01 05 {PauseAddr} 1");
                Thread.Sleep(200);
                plc.Write<bool>(PauseAddr, false, $"01 05 {PauseAddr} 1");
            });
        }

        /// <summary>
        /// Plc回零
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcHome(IDeviceEngine deviceEngine)
        {
            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(HomeAddr)) return;

                if (string.IsNullOrEmpty(HomeFinishAddr)) return;

                // 1.检查是否已经完成
                bool isFinish = plc.ReadBool(HomeFinishAddr);

                // 2.如果初始化完成，不在进行初始化
                if (isFinish) return;

                // 3.如果初始化没有完成，但是运行中
                if (string.IsNullOrEmpty(AutoRunAddr))
                {
                    // 判断是否自动运行中,如果自动运行就不进行初始化了
                    bool isAutoRun = plc.ReadBool(AutoRunAddr);
                    if (isAutoRun) return;
                }

                // 2、启动回零
                plc.Write<bool>(HomeAddr, true, $"01 05 {HomeAddr} 1");
            });
        }

        /// <summary>
        /// 等待回零完成
        /// </summary>
        /// <param name="deviceEngine"></param>
        /// <exception cref="FriendlyException"></exception>
        public void PlcWaitHome(IDeviceEngine deviceEngine)
        {
            PlcAction(deviceEngine, plc =>
            {
                // 3、等待回零成功（因为皮带需要转动几秒中）
                if (string.IsNullOrEmpty(HomeAddr) && string.IsNullOrEmpty(HomeFinishAddr)) return;

                if (string.IsNullOrEmpty(HomeAddr) && string.IsNullOrEmpty(HomeFinishAddr)) return;

                plc.Wait(HomeFinishAddr, DataStruct.Enums.OpRule.Equal, 1, 6000);
            });
        }

        /// <summary>
        /// Plc 复位，清除报警
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcRecovery(IDeviceEngine deviceEngine)
        {
            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(RecoveryAddr)) return;
                plc.Write<bool>(RecoveryAddr, true, $"01 05 {RecoveryAddr} 1");

                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    plc.Write<bool>(RecoveryAddr, false, $"01 05 {RecoveryAddr} 1");
                });
            });
        }

        /// <summary>
        /// Plc 复位，清除报警
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcStop(IDeviceEngine deviceEngine)
        {
            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(StopAddr)) return;
                plc.Write<bool>(StopAddr, true, $"01 05 {StopAddr} 1");

                Task.Run(() =>
                {
                    Thread.Sleep(200);
                    plc.Write<bool>(StopAddr, false, $"01 05 {StopAddr} 1");
                });
            });
        }

        /// <summary>
        /// 空跑
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcEmpty(IDeviceEngine deviceEngine, bool start = true)
        {
            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(EmptyRunAddr)) return;
                plc.Write<bool>(EmptyRunAddr, start, $"01 05 {EmptyRunAddr} 1");
            });
        }

        /// <summary>
        /// 报警
        /// </summary>
        private Dictionary<PlcAlarm, List<PlcAlarm>> alarms = new Dictionary<PlcAlarm, List<PlcAlarm>>();

        /// <summary>
        /// Plc报警对象
        /// </summary>
        /// <param name="deviceEngine"></param>
        /// <param name="alarmAction"></param>
        public void PlcAlarm(IDeviceEngine deviceEngine, Action<List<PlcAlarm>, AlarmType> alarmAction)
        {
            if (PLCAlarmList == null) return;

            PlcAction(deviceEngine, plc =>
            {
                if (PLCAlarmList == null || PLCAlarmList.Count == 0) return;

                //注释这一行，无用，第一个地址位也不一定是最小的
                //var result = plc.ReadBatchAlarm(PLCAlarmList[0].Address, PLCAlarmList.Count);
                int startAddr = PLCAlarmList.Min(u => u.Address);
                int endAddr = PLCAlarmList.Max(u => u.Address);
                int len = endAddr - startAddr + 1;
                List<bool> results = plc.ReadBatchAlarm(startAddr, len);

                foreach (var item in PLCAlarmList)
                {
                    int index = item.Address - startAddr;
                    item.IsAlarm = results[index];
                }

                // 报警
                var alarms = PLCAlarmList.Where(u => u.IsAlarm).ToList();
                if (alarms != null && alarms.Count > 0)
                {
                    alarmAction?.Invoke(alarms, AlarmType.PlcAlarm);
                }
            });
        }

        /// <summary>
        /// Plc的状态
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcStatus(IDeviceEngine deviceEngine, Action<PlcStatus> plcAction)
        {
            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(StatusAddr)) return;
                
                var pStatus = plc.ReadNumber(StatusAddr);

                if (Enum.TryParse<PlcStatus>(pStatus.ToString(), out var p))
                {
                    plcAction?.Invoke(p);
                }
            });
        }

        /// <summary>
        /// 检测PLC 是否报警
        /// </summary>
        /// <param name="deviceEngine"></param>
        /// <returns></returns>
        public bool PlcIsAlarm(IDeviceEngine deviceEngine, out string plcErr)
        {
            plcErr = "";
            string errMsg = "";
            bool isAlarm = false;
            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(StatusAddr)) return;

                var pStatus = (int)plc.ReadNumber(StatusAddr);
                if (Enum.TryParse<PlcStatus>(pStatus.ToString(), out var p))
                {
                    isAlarm = (p == Luster.Motion.TaskFlow.Engine.Models.PlcStatus.Alarm);

                    if (isAlarm)
                    {
                        PlcAlarm(deviceEngine, (alarms, alarmType) =>
                        {
                            errMsg = $"PLC:{string.Join(",", alarms.Select(u => u.Desc))}";
                        });
                    }
                }
            });

            plcErr = errMsg;
            return isAlarm;
        }


        /// <summary>
        /// Plc回零中
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void PlcHoming(IDeviceEngine deviceEngine, Action<AlarmType> alarmAction)
        {

            PlcAction(deviceEngine, plc =>
            {
                if (string.IsNullOrEmpty(HomingAddr)) return;

                // 1.检查是否突然回零
                bool isHoming = plc.ReadBool(HomingAddr);
                if (isHoming) alarmAction?.Invoke(AlarmType.InfoTip);


            });
        }


        #endregion

        #region 三色灯
        /// <summary>
        /// 红
        /// </summary>
        public VDevice LightRed { get; set; }

        /// <summary>
        /// 绿
        /// </summary>
        public VDevice LightGreen { get; set; }

        /// <summary>
        /// 黄灯
        /// </summary>
        public VDevice LightYellow { get; set; }

        /// <summary>
        /// 蜂鸣器
        /// </summary>
        public VDevice Buzzer { get; set; }
        #endregion

        #region 光幕
        [Ignore]
        public List<LightCurtainModel> LightCurtainList { get; set; }
        #endregion

        #region 系统语言
        /// <summary>
        /// 系统语言
        /// </summary>
        public string Language { get; set; }
        #endregion

        #region 模式
        public DeviceMode RunMode { get; set; } = DeviceMode.Virtual;
        #endregion

        #region 机器人相关

        #region DO
        /// <summary>
        /// 机器人启动
        /// </summary>
        public VDevice RobotStart { get; set; }

        /// <summary>
        /// 机器人暂停
        /// </summary>
        public VDevice RobotPause { get; set; }

        /// <summary>
        /// 机器人停止
        /// </summary>
        public VDevice RobotStop { get; set; }

        /// <summary>
        /// 机器人复位
        /// </summary>
        public VDevice RobotReset { get; set; }

        /// <summary>
        /// 机器人清错
        /// </summary>
        public VDevice RobotClearFault { get; set; }
        #endregion

        #region DI
        /// <summary>
        /// 机器人运行中
        /// </summary>
        public VDevice RobotRuning { get; set; }

        /// <summary>
        /// 机器人暂停中
        /// </summary>
        public VDevice RobotPaused { get; set; }

        /// <summary>
        /// 机器人空闲中
        /// </summary>
        public VDevice RobotIdle { get; set; }

        /// <summary>
        /// 机器人报错中
        /// </summary>
        public VDevice RobotError { get; set; }

        #endregion

        #endregion

        #region 机器人相关2

        #region DO
        /// <summary>
        /// 机器人启动
        /// </summary>
        public VDevice RobotStart2 { get; set; }

        /// <summary>
        /// 机器人暂停
        /// </summary>
        public VDevice RobotPause2 { get; set; }

        /// <summary>
        /// 机器人停止
        /// </summary>
        public VDevice RobotStop2 { get; set; }

        /// <summary>
        /// 机器人复位
        /// </summary>
        public VDevice RobotReset2 { get; set; }

        /// <summary>
        /// 机器人清错
        /// </summary>
        public VDevice RobotClearFault2 { get; set; }
        #endregion

        #region DI
        /// <summary>
        /// 机器人运行中
        /// </summary>
        public VDevice RobotRuning2 { get; set; }

        /// <summary>
        /// 机器人暂停中
        /// </summary>
        public VDevice RobotPaused2 { get; set; }

        /// <summary>
        /// 机器人空闲中
        /// </summary>
        public VDevice RobotIdle2 { get; set; }

        /// <summary>
        /// 机器人报错中
        /// </summary>
        public VDevice RobotError2 { get; set; }

        #endregion

        #endregion

        #region 全局变量
        [Ignore]
        public List<GlobalModel> ListGlobalVar { get; set; }
        #endregion

        #region 运行模式列表
        [Ignore]
        public List<RunModeModel> ListRunMode { get; set; }
        #endregion

        #region 功能设定

        /// <summary>
        /// 一键屏蔽变量名称列表
        /// </summary>
        [Ignore]
        public List<string> EnableDisableVarNames { get; set; } = new List<string>();

        /// <summary>
        /// 存储变量值的字典
        /// </summary>
        [Ignore]
        public Dictionary<string, bool> EnableDisableVarValues { get; set; } = new Dictionary<string, bool>();

        #endregion

        #region 关键参数全局变量
        [Ignore]
        public List<string> KeyParameterGlobalNames { get; set; } = new List<string>();
        #endregion

        #region 模块配置
        [Ignore]
        public List<ModuleSetModel> ListModule { get; set; }
        #endregion

        #region 软件配置

        /// <summary>
        /// CT
        /// </summary>
        public int CT { get; set; }

        /// <summary>
        /// 开启CT统计
        /// </summary>
        public bool IsEnableCTStatistics { get; set; }


        /// <summary>
        /// 门锁取反
        /// </summary>
        public bool IsDoorLock { get; set; }
        public CtFileType CtFileType { get; set; }

        /// <summary>
        /// 启用蜂鸣器
        /// </summary>
        public bool IsEnableBuzzer { get; set; } = true;

        /// <summary>
        /// 当前机台名称
        /// </summary>
        //public string StationName { get; set; }

        /// <summary>
        /// 抛料固定耗时
        /// </summary>
        public double TossingTime { get; set; }

        /// <summary>
        /// 稼动率刷新频率
        /// </summary>
        public double PvRefreshInterval { get; set; }

        /// <summary>
        /// 是否弹窗操作提示
        /// </summary>
        public bool IsShowOperationTip { get; set; }

        /// <summary>
        /// 高级权限停留时间
        /// </summary>
        public double HighLevelTime { get; set; } = 3;

        /// <summary>
        /// 启用安全门
        /// </summary>
        public bool EnableSaftyDoor { get; set; } = false;

        /// <summary>
        /// 启用光栅
        /// </summary>
        public bool EnableLightCurtain { get; set; } = false;

        /// <summary>
        /// 班次列表
        /// </summary>
        [Ignore]
        public List<ClassInfo> ClassModels { get; set; }
        [Ignore]
        public List<OperationTip> OperationTips { get; set; }

        #endregion

        #region 当前项目下的配方列表
        [Ignore]
        public List<RecipeModel> ListRecipe { get; set; }
        #endregion

        #region 获取当前时间对应的班别
        public ClassInfo GetCurrentClass(DateTime dateTime)
        {
            var defaultTime = DateTime.Parse($"{DateTime.Now.ToString("yyyy-MM-dd")} 08:00:00");
            var defaultClass = new ClassInfo()
            {
                FirstClass = true,
                ClassName = "None",
                StartTime = defaultTime,
                EndTime = defaultTime.AddHours(8)
            };

            if (ClassModels == null || ClassModels.Count == 0)
            {
                return defaultClass;
            }

            // 找到和当前时间最近的班别
            int minIndex = 0;
            double minOffset = 0;
            for (int i = 0; i < ClassModels.Count; i++)
            {
                var item = ClassModels[i];

                double start = Math.Abs((item.GetStartTime() - dateTime).TotalMinutes);
                double end = Math.Abs((item.GetEndTime() - dateTime).TotalMinutes);
                var offset = Math.Min(start, end);
                if (minOffset == 0)
                {
                    minOffset = offset;
                }

                if (minOffset > offset)
                {
                    minIndex = i;
                    minOffset = offset;
                }
            }

            var c = ClassModels[minIndex];


            return new ClassInfo()
            {
                FirstClass = c.FirstClass,
                ClassName = c.ClassName,
                StartTime = c.GetStartTime(),
                EndTime = c.GetEndTime()
            };
        }
        #endregion

        #region 导入导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            var root = this.ToXml("SysConfig");

            if (ClassModels != null)
            {
                var clsassElement = new XElement("Classes");
                // 添加用户Feature
                foreach (var model in ClassModels)
                {
                    var element = model.ExportXml();
                    clsassElement.Add(element);
                }
                root.Add(clsassElement);
            }

            if (PLCAlarmList != null)
            {
                var alarmElement = new XElement("AlarmList");
                foreach (var model in PLCAlarmList)
                {
                    var element = model.ExportXml();
                    alarmElement.Add(element);
                }
                root.Add(alarmElement);
            }
            if (LightCurtainList != null)
            {
                var lightCurtainElement = new XElement("LightCurtainList");
                foreach (var model in LightCurtainList)
                {
                    var element = model.ExportXml();
                    lightCurtainElement.Add(element);
                }
                root.Add(lightCurtainElement);
            }
            if (ListGlobalVar != null)
            {
                var listGlobalVarElement = new XElement("ListGlobalVar");
                foreach (var model in ListGlobalVar)
                {
                    var element = model.ExportXml();
                    listGlobalVarElement.Add(element);
                }
                root.Add(listGlobalVarElement);
            }

            if (ListModule != null)
            {
                var listModuleElement = new XElement("ListModule");
                foreach (var model in ListModule)
                {
                    var element = model.ExportXml();
                    listModuleElement.Add(element);
                }
                root.Add(listModuleElement);
            }
            if (OperationTips != null)
            {
                var operatiobElement = new XElement("Operation");
                // 添加用户Feature
                foreach (var model in OperationTips)
                {
                    var element = model.ExportXml();
                    operatiobElement.Add(element);
                }
                root.Add(operatiobElement);
            }
            if (ListRunMode != null)
            {
                var runmodeElement = new XElement("ListRunMode");
                foreach (var model in ListRunMode)
                {
                    var element = model.ExportXml();
                    runmodeElement.Add(element);
                }
                root.Add(runmodeElement);
            }

            if (ListOpenCloseDoor != null)
            {
                var doorElement = new XElement("ListOpenCloseDoor");
                foreach (var model in ListOpenCloseDoor)
                {
                    var element = model.ExportXml();
                    doorElement.Add(element);
                }
                root.Add(doorElement);
            }

            if (ListSysOperateIO != null)
            {
                var operateIOElement = new XElement("ListSysOperateIO");
                foreach (var model in ListSysOperateIO)
                {
                    var element = model.ExportXml();
                    operateIOElement.Add(element);
                }
                root.Add(operateIOElement);
            }

            // 添加一键屏蔽变量名称列表导出
            if (EnableDisableVarNames != null && EnableDisableVarNames.Any())
            {
                var enableDisableElement = new XElement("EnableDisableVars");
                foreach (var varName in EnableDisableVarNames)
                {
                    bool value = EnableDisableVarValues.ContainsKey(varName) ? EnableDisableVarValues[varName] : false;
                    // 只存储变量名称
                    enableDisableElement.Add(new XElement("Var",
                        new XAttribute("Name", varName ?? ""),
                        new XAttribute("Value", value.ToString().ToLower())));
                }
                root.Add(enableDisableElement);
            }

            if (KeyParameterGlobalNames != null && KeyParameterGlobalNames.Any())
            {
                var keyParameterGlobalNames = new XElement("KeyParameterGlobalNames");
                foreach (var varName in KeyParameterGlobalNames)
                {
                    // 只存储变量名称
                    keyParameterGlobalNames.Add(new XElement("Var",
                        new XAttribute("Name", varName ?? "")));
                }
                root.Add(keyParameterGlobalNames);
            }

            return root;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            ClassModels = new List<ClassInfo>();
            var element = xElement.Elements("Classes");
            if (element != null)
            {
                foreach (var xItems in element.Elements("ClassInfo"))
                {
                    var user = new ClassInfo();
                    user.ParserXml(xItems);
                    ClassModels.Add(user);
                }

                // 添加默认白班
                if (ClassModels.Count == 0)
                {
                    DateTime.TryParse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} 08:30:00", out var sDate);
                    DateTime.TryParse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} 18:30:00", out var eDate);
                    ClassModels.Add(new ClassInfo()
                    {
                        ClassName = SystemConsts.Morning,
                        FirstClass = true,
                        StartTime = sDate,
                        EndTime = eDate
                    });
                }

                // 添加默认白班
                if (ClassModels.Count == 1)
                {
                    DateTime.TryParse($"{DateTime.Now.ToString("yyyy-MM-dd")} 19:30:00", out var sDate);
                    DateTime.TryParse($"{DateTime.Now.AddDays(1).ToString("yyyy-MM-dd")} 6:30:00", out var eDate);
                    ClassModels.Add(new ClassInfo()
                    {
                        ClassName = SystemConsts.Night,
                        FirstClass = false,
                        StartTime = sDate,
                        EndTime = eDate
                    });
                }
            }

            PLCAlarmList = new List<PlcAlarm>();
            var alarmelement = xElement.Elements("AlarmList");
            if (alarmelement != null)
            {
                foreach (var xItems in alarmelement.Elements("PlcAlarm"))
                {
                    var alarm = new PlcAlarm();
                    alarm.ParserXml(xItems);
                    PLCAlarmList.Add(alarm);
                }
            }
            alarmelement.Remove();

            LightCurtainList = new List<LightCurtainModel>();
            var lightCurtainElement = xElement.Elements("LightCurtainList");
            if (lightCurtainElement != null)
            {
                foreach (var xItems in lightCurtainElement.Elements("LightCurtainModel"))
                {
                    var lightCurtain = new LightCurtainModel();
                    lightCurtain.ParserXml(xItems);
                    LightCurtainList.Add(lightCurtain);
                }
            }
            lightCurtainElement.Remove();

            ListGlobalVar = new List<GlobalModel>();
            var listGlobalVarElement = xElement.Elements("ListGlobalVar");
            if (listGlobalVarElement != null)
            {
                foreach (var xItems in listGlobalVarElement.Elements("GlobalModel"))
                {
                    var globalModel = new GlobalModel();
                    globalModel.ParserXml(xItems);
                    ListGlobalVar.Add(globalModel);
                }
            }
            listGlobalVarElement.Remove();

            ListModule = new List<ModuleSetModel>();
            var listModuleElement = xElement.Elements("ListModule");
            if (listModuleElement != null)
            {
                foreach (var xItems in listModuleElement.Elements("ModuleSetModel"))
                {
                    var modulesetModel = new ModuleSetModel();
                    modulesetModel.ParserXml(xItems);
                    ListModule.Add(modulesetModel);
                }
            }
            listModuleElement.Remove();
            OperationTips = new List<OperationTip>();
            var OperationElement = xElement.Elements("Operation");
            if (OperationElement != null)
            {
                foreach (var xItems in OperationElement.Elements("OperationTip"))
                {
                    var opTip = new OperationTip();
                    opTip.ParserXml(xItems);
                    OperationTips.Add(opTip);
                }
            }
            OperationElement.Remove();

            // 默认包含生产模式
            ListRunMode = new List<RunModeModel>();

            var xRunMode = xElement.Element("ListRunMode");
            if (xRunMode != null && xRunMode.Elements().Count() > 0)
            {
                foreach (var xItems in xRunMode.Elements("RunModeModel"))
                {
                    var mode = new RunModeModel();
                    mode.ParserXml(xItems);
                    ListRunMode.Add(mode);
                }
            }

            // 自动添加生成模式
            if (!ListRunMode.Any(u => u.Mode == SystemConsts.ProductMode))
            {
                ListRunMode.Add(new RunModeModel() { Mode = SystemConsts.ProductMode, IsRunMode = true });
            }

            // 自动添加空跑模式
            if (!ListRunMode.Any(u => u.Mode == SystemConsts.EmptyMode))
            {
                ListRunMode.Add(new RunModeModel() { Mode = SystemConsts.EmptyMode, IsRunMode = false });
            }

            // 自动添加首件模式
            if (!ListRunMode.Any(u => u.Mode == SystemConsts.FirstMode))
            {
                ListRunMode.Add(new RunModeModel() { Mode = SystemConsts.FirstMode, IsRunMode = false });
            }

            ListOpenCloseDoor = new List<DoorLockModel>();
            var xdoorMode = xElement.Element("ListOpenCloseDoor");
            if (xdoorMode != null && xdoorMode.Elements().Count() > 0)
            {
                foreach (var xItems in xdoorMode.Elements("DoorLockModel"))
                {
                    var mode = new DoorLockModel();
                    mode.ParserXml(xItems);
                    ListOpenCloseDoor.Add(mode);
                }
            }

            ListSysOperateIO = new List<SysOperateIOModel>();
            var xoperateIOMode = xElement.Element("ListSysOperateIO");
            if (xoperateIOMode != null && xoperateIOMode.Elements().Count() > 0)
            {
                foreach (var xItems in xoperateIOMode.Elements("SysOperateIOModel"))
                {
                    var mode = new SysOperateIOModel();
                    mode.ParserXml(xItems);
                    ListSysOperateIO.Add(mode);
                }
            }

            DIYAlarmList = new List<DIY_Alarm>();
            var diyalarm = xElement.Element("DIYAlarmList");
            if (diyalarm != null)
            {
                foreach (var xItems in diyalarm.Elements("DIYAlarm"))
                {
                    var mode = new DIY_Alarm();
                    mode.ParserXml(xItems);
                    DIYAlarmList.Add(mode);
                }
            }

            // 解析一键屏蔽变量名称列表
            EnableDisableVarNames = new List<string>();
            EnableDisableVarValues = new Dictionary<string, bool>();

            var enableDisableElement = xElement.Element("EnableDisableVars");
            if (enableDisableElement != null && enableDisableElement.Elements().Any())
            {
                foreach (var xItem in enableDisableElement.Elements("Var"))
                {
                    var varName = xItem.Attribute("Name")?.Value ?? "";
                    var varValueStr = xItem.Attribute("Value")?.Value ?? "false";

                    if (!string.IsNullOrEmpty(varName))
                    {
                        EnableDisableVarNames.Add(varName);
                        EnableDisableVarValues[varName] = bool.TryParse(varValueStr, out bool value) ? value : false;
                    }
                }
            }

            KeyParameterGlobalNames = new List<string>();

            var keyParameterGlobalNames = xElement.Element("KeyParameterGlobalNames");
            if (keyParameterGlobalNames != null && keyParameterGlobalNames.Elements().Any())
            {
                foreach (var xItem in keyParameterGlobalNames.Elements("Var"))
                {
                    var varName = xItem.Attribute("Name")?.Value ?? "";

                    if (!string.IsNullOrEmpty(varName))
                    {
                        KeyParameterGlobalNames.Add(varName);
                    }
                }
            }

            this.FromXml(xElement);
        }

        #endregion

        /// <summary>
        /// 配置设备引擎，进行监控
        /// </summary>
        /// <param name="deviceEngine"></param>
        public void SetDeviceEngine(IDeviceEngine deviceEngine)
        {
            // 按钮
            InitDevice(StartButton, deviceEngine);
            InitDevice(PauseButton, deviceEngine);
            InitDevice(RecoveryButton, deviceEngine);
            InitDevice(StopButton, deviceEngine);
            InitDevice(EmergencyButton, deviceEngine);
            InitDevice(IgnoreButton, deviceEngine);
            InitDevice(RetryButton, deviceEngine);

            // 信号灯
            InitDevice(LightRed, deviceEngine);
            InitDevice(LightGreen, deviceEngine);
            InitDevice(LightYellow, deviceEngine);

            // 按钮灯
            InitDevice(LightStart, deviceEngine);
            InitDevice(LightPause, deviceEngine);
            InitDevice(LightRecovery, deviceEngine);
            InitDevice(LightStop, deviceEngine);

            //机器人
            InitDevice(RobotStart, deviceEngine);
            InitDevice(RobotPause, deviceEngine);
            InitDevice(RobotStop, deviceEngine);
            InitDevice(RobotReset, deviceEngine);
            InitDevice(RobotClearFault, deviceEngine);

            InitDevice(RobotRuning, deviceEngine);
            InitDevice(RobotPaused, deviceEngine);
            InitDevice(RobotIdle, deviceEngine);
            InitDevice(RobotError, deviceEngine);

            ///机器人2 
            InitDevice(RobotStart2, deviceEngine);
            InitDevice(RobotPause2, deviceEngine);
            InitDevice(RobotStop2, deviceEngine);
            InitDevice(RobotReset2, deviceEngine);
            InitDevice(RobotClearFault2, deviceEngine);

            InitDevice(RobotRuning2, deviceEngine);
            InitDevice(RobotPaused2, deviceEngine);
            InitDevice(RobotIdle2, deviceEngine);
            InitDevice(RobotError2, deviceEngine);

        }

        /// <summary>
        /// 设备初始化
        /// </summary>
        /// <param name="vDevice"></param>
        /// <param name="engine"></param>
        public void InitDevice(VDevice vDevice, IDeviceEngine engine)
        {
            if (vDevice != null)
            {
                var io = engine.GetVirtualByID(vDevice.DeviceID) as VIO;
                if (io != null)
                {
                    vDevice.Virtual = io;
                }
            }
        }

        /// <summary>
        /// IO监控
        /// </summary>
        private Dictionary<string, bool> ioDict = new Dictionary<string, bool>();

        /// <summary>
        /// 获取IO结果
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, bool> GetIOResult()
        {
            ioDict.Clear();

            if (StartButton.Virtual is VIO sIO)
            {
                ioDict.Add("StartButton", sIO.GetDigitalIn());
            }

            if (PauseButton.Virtual is VIO pIO)
            {
                ioDict.Add("PauseButton", pIO.GetDigitalIn());
            }

            if (RecoveryButton.Virtual is VIO rIO)
            {
                ioDict.Add("RecoveryButton", rIO.GetDigitalIn());
            }

            if (StopButton.Virtual is VIO stopIO)
            {
                ioDict.Add("StopButton", stopIO.GetDigitalIn());
            }

            if (StartButton.Virtual is VIO emercyIO)
            {
                ioDict.Add("EmergencyButton", emercyIO.GetDigitalIn());
            }
            if (IgnoreButton.Virtual is VIO ignoreIO)
            {
                ioDict.Add("IgnoreButton", ignoreIO.GetDigitalIn());
            }
            if (RetryButton.Virtual is VIO retryIO)
            {
                ioDict.Add("RetryButton", retryIO.GetDigitalIn());
            }

            if (LightRed.Virtual is VIO redIO)
            {
                ioDict.Add("LightRed", redIO.GetDigitalOut());
            }

            if (LightGreen.Virtual is VIO greenIO)
            {
                ioDict.Add("LightGreen", greenIO.GetDigitalOut());
            }

            if (LightYellow.Virtual is VIO yellowIO)
            {
                ioDict.Add("LightYellow", yellowIO.GetDigitalOut());
            }
            if (LightStart.Virtual is VIO lsIO)
            {
                ioDict.Add("LightStart", lsIO.GetDigitalOut());
            }
            if (LightStop.Virtual is VIO lstopIO)
            {
                ioDict.Add("LightStop", lstopIO.GetDigitalOut());
            }
            if (LightPause.Virtual is VIO lpIO)
            {
                ioDict.Add("LightPause", lpIO.GetDigitalOut());
            }
            if (LightRecovery.Virtual is VIO lrIO)
            {
                ioDict.Add("LightRecovery", lrIO.GetDigitalOut());
            }


            return ioDict;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void SaveSysConfig(string sysConfig = "")
        {
            if (!Directory.Exists(Path.GetDirectoryName(sysConfig)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sysConfig));//创建路径
            }
            XElement xe = ExportXml();
            xe.Save(sysConfig);
        }

        /// <summary>
        /// 加载
        /// </summary>
        public void LoadSysConfig(string sysConfig = "")
        {
            if (sysConfig != "")
            {
                if (!File.Exists(sysConfig))
                {
                    SaveSysConfig(sysConfig);
                }

                try
                {
                    XElement xSysConfig = XElement.Load(sysConfig);
                    ParserXml(xSysConfig);
                }
                catch (Exception)
                {

                }
            }

        }
    }
}