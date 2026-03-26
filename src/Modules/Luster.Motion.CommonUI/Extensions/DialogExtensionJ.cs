///*************************************************************************************
//* 文件名  ：DialogExtension.cs
//* 命名空间：Luster.Motion.EditorUI.Extensions
//* 描述    ：Prism 弹窗单例扩展（线程安全，杜绝重复弹窗）
//* 重写时间：2025-09-12
//************************************************************************************/
//using System;
//using System.Collections.Generic;
//using System.IO;
//using Prism.Services.Dialogs;
//using Luster.Motion.CommonUI.Models;
//using Luster.Motion.DataStruct.DataModels;
//using Luster.Motion.DataStruct.Enums;
//using Luster.Motion.TaskFlow.Engine.Models;
//using langs = Luster.Motion.Assests.Langs;
//using Luster.TaskFlow.Common.Attributes;

//namespace Luster.Motion.EditorUI.Extensions
//{
//    public static class DialogExtensionJ
//    {
//        #region ── 单例基础设施 ──
//        private static readonly object _lock = new object();
//        private static bool _isOpening = false;

using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Prism.Services.Dialogs;
using System;
using System.Text;
using langs = Luster.Motion.Assests.Langs;

namespace Luster.Motion.EditorUI.Extensions
{
    public static class DialogExtensionJ
    {
        //多线程调用同一个窗体，需要加锁
        private static readonly object _hiveLock = new object();
        private static readonly string _csvPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dialog.txt");

        private static bool _hiveAlarmDispose = true;
        private static bool _hiveStartDispose = true;
        private static bool _hiveOperationDispose = true;

//        /// <summary>
//        /// 关闭当前窗口（回调里必须调）
//        /// </summary>
//        public static void CloseCurrent()
//        {
//            lock (_lock) { _isOpening = false; }
//        }
//        #endregion

        public static void HiveRunPopUp(/*this*/ IDialogService service, SystemOperation operation = SystemOperation.Home, Action<IDialogResult> callback = null)
        {
            LogCsv("Start HiveRunPopUp");
            if (!_hiveStartDispose)
            {
                LogCsv("Return HiveRunPopUp");
                return;
            }

            DialogParameters param = new DialogParameters();
            param.Add("Title", "Hive");
            param.Add("Operation", operation);
            //var win = service.Show("HiveStartDialog2", param, callback);
            var win = service.Show("HiveStartDialog2Part1", param, callback);
            _hiveStartDispose = false;
            win.Closed += (sender, e) =>
            {
                _hiveStartDispose = true;
                LogCsv("Close HiveRunPopUp");
            };
            LogCsv("End HiveRunPopUp");
        }

        public static void ShowAlarmNew(/*this*/ IDialogService service, AlarmInfo alarmInfo, bool _HiveStartDialog2Part2_Opend, Action<IDialogResult> callback)
        {
            LogCsv("Start HiveAlarm");
            if (!_hiveAlarmDispose)
            {
                LogCsv("Return HiveAlarm");
                return;
            }

            DialogParameters param = new DialogParameters();
            param.Add("Title", langs.LangProvider.GetLang("Alarm"));
            param.Add("AlarmInfo", alarmInfo);
            param.Add("HiveStart2_Opend", _HiveStartDialog2Part2_Opend);
            IDialogWindow win = null;
            if (alarmInfo.AlarmType == AlarmType.RetryAlarm) //  || alarmInfo.AlarmType == AlarmType.WarningTip，警告提示需要弹HiveAlarm窗
            {
                service.ShowDialog("AlarmDialog", param, callback);
            }
            else if(alarmInfo.AlarmType == AlarmType.ManuOperationAlarm)
            {
                service.ShowDialog("AlarmDialog", param, callback);
            }
            else
            {
                win = service.Show("HiveAlarmDialog", param, callback);
                _hiveAlarmDispose = false;
                win.Closed += (sender, e) =>
                {
                    _hiveAlarmDispose = true;
                    LogCsv("Close HiveAlarm");
                };
            }
           
            LogCsv("End HiveAlarm");
        }

        [Obsolete("暂时未应用")]
        public static void ShowSelectTip(IDialogService service, bool enable, SystemOperation operation = SystemOperation.Home, Action<IDialogResult> callback = null)
        {
            LogCsv("Start HiveOperation");
            if (!_hiveOperationDispose)
            {
                LogCsv("Return HiveOperation");
                return;
            }

            DialogParameters param = new DialogParameters();
            param.Add("Title", "Hive");
            param.Add("Operation", operation);
            IDialogWindow win = null;
            if (enable)
            {
                win = service.Show("HiveOperationDialog", param, callback);
            }
            else
            {
                win = service.Show("ShowOperationTipDialog", param, callback);
            }
            _hiveOperationDispose = false;
            win.Closed += (sender, e) =>
            {
                _hiveOperationDispose = true;
                LogCsv("Close HiveOperation");
            };
            LogCsv("End HiveOperation");
        }

        private static void LogCsv(string msg)
        {
            try
            {
                bool fileExists = System.IO.File.Exists(_csvPath);
                using (var sw = new System.IO.StreamWriter(_csvPath, true, Encoding.UTF8))
                {
                    if (!fileExists)
                    {
                        sw.WriteLine("本文件用于记录Hive弹窗的状态");
                    }
                    sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}, " + msg);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入CSV日志失败: {ex.Message}");
            }
        }

    }

}