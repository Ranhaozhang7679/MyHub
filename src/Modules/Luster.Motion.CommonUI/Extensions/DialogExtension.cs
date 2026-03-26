#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DialogExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Extensions
* 文 件 名:       DialogExtension.cs
* 创建时间:       2022/6/9 15:02:35
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      fe4c5fe3-23d1-43da-a578-dc7ba056ffdb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/9 15:02:35
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using langs = Luster.Motion.Assests.Langs;

namespace Luster.Motion.EditorUI.Extensions
{
    public static class DialogExtension
    {
        private static readonly System.Threading.Mutex showAlarmMutex = new System.Threading.Mutex();
        private static readonly object objShowLock = new object();

        public const string SearchKey = nameof(SearchKey);
        private static bool IsStartAg = false;
        private static bool IsStartAgAlarm = false;
        private static bool _isOnboardAlarm = false;

        /// <summary>
        /// 弹窗
        /// </summary>
        private static IDialogWindow _showWindow;
        private static IDialogWindow _alarmWindow;
        private static IDialogWindow _hiveOnWindow;
        private static IDialogWindow _hiveReportWindow;

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowDeviceConfig(this IDialogService service, ParameterAttribute pAttr, string searchKey, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", pAttr.CN);
            param.Add("Parameter", pAttr);
            param.Add("SearchKey", searchKey);

            service.ShowDialog("DeviceDialog", param, callback);
        }

        /// <summary>
        /// 加载显示报警代码弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAlarmConfig(this IDialogService service, ParameterAttribute pAttr, string searchKey, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", pAttr.CN);
            param.Add("Parameter", pAttr);
            param.Add("SearchKey", searchKey);

            service.ShowDialog("AlarmCodeDialog", param, callback);
        }


        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSlaveConfig(this IDialogService service, ParameterAttribute pAttr, string searchKey, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", pAttr.CN);
            param.Add("Parameter", pAttr);
            param.Add("SearchKey", searchKey);

            _showWindow = service.Show("SlaveDialog", param, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowWinform(this IDialogService service, string title, string formAssembly, string eventName, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);

            DialogParameters param = new DialogParameters();
            param.Add("Title", title);
            param.Add("FormAssembly", formAssembly);
            param.Add("EventName", eventName);

            service.ShowDialog("WinFormDialog", param, callback);
        }


        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        [Obsolete]
        public static void ShowAlarm(this IDialogService service, AlarmInfo alarmInfo, Action<IDialogResult> callback = null)
        {
            lock (objShowLock)
            {
                //关闭已有的弹窗，重新赋值对象为null
                service.CloseExistShowWindow(_showWindow);
                if (_showWindow != null)
                {
                    _showWindow.Closing -= _showWindow_Closing;
                    _showWindow = null;//取消之前事件的挂载
                }
                service.CloseExistShowWindow(_alarmWindow);
                if (_alarmWindow != null)
                {
                    _alarmWindow.Closing -= _alarmWindow_Closing;
                    _alarmWindow = null;
                }

                //开启新的弹窗，注册事件
                DialogParameters param = new DialogParameters();
                param.Add("Title", langs.LangProvider.GetLang("Alarm"));
                param.Add("AlarmInfo", alarmInfo);
                
                

                if (alarmInfo.AlarmType == AlarmType.RetryAlarm || alarmInfo.AlarmType == AlarmType.WarningTip)
                {
                    string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},AlarmDialog called";
                    File.AppendAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dialog.csv"), new List<string> { logLine });
                    service.ShowDialog("AlarmDialog", param, callback);
                }
                else
                {
                    _alarmWindow = service.Show("HiveAlarmDialog", param, callback);
                    if (_alarmWindow != null)
                    {
                        _alarmWindow.Closing -= _alarmWindow_Closing;
                        _alarmWindow.Closing += _alarmWindow_Closing;
                    }
                    else
                    {
                        string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},HiveAlarmDialog null";
                        File.AppendAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dialog.csv"), new List<string> { logLine });
                    }
                }
            }
        }

        /// <summary>
        /// HIVE 启动 弹窗提示
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAlarmNew(this IDialogService service, AlarmInfo alarmInfo, bool _HiveStartDialog2Part2_Opend, Action<IDialogResult> callback = null)
        {
            DialogExtensionJ.ShowAlarmNew(service, alarmInfo, _HiveStartDialog2Part2_Opend, callback);
            return;

            //由于ShowAlarmNew是在多线程中直接调用的，可能会存在资源竞争问题，因此使用Mutex来确保线程安全
            //showAlarmMutex.WaitOne();
            // 追加写入CSV文件，记录时间和详细描述
            string csvPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dialog.csv");
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{IsStartAg},HiveAlarmDialog called";
            try
            {
                bool fileExists = System.IO.File.Exists(csvPath);
                using (var sw = new System.IO.StreamWriter(csvPath, true, Encoding.UTF8))
                {
                    if (!fileExists)
                    {
                        sw.WriteLine("Time,IsStartAg,Description");
                    }
                    sw.WriteLine(logLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入CSV日志失败: {ex.Message}");
            }
            //try
            //{
            lock (objShowLock)
            {
                if (IsStartAgAlarm) return;
            }
            service.CloseExistShowWindow(_showWindow);
            if (_showWindow != null)
            {
                _showWindow.Closing -= _showWindow_Closing;
                _showWindow = null;//取消之前事件的挂载
            }
            //关闭已有的弹窗，重新赋值对象为null
            service.CloseExistShowWindow(_alarmWindow);
            if (_alarmWindow != null)
            {
                _alarmWindow.Closing -= __alarmWindow_Closing;
                _alarmWindow = null;//取消之前事件的挂载
            }
            //开启新的弹窗，注册事件
            DialogParameters param = new DialogParameters();
            param.Add("Title", langs.LangProvider.GetLang("Alarm"));
            param.Add("AlarmInfo", alarmInfo);

            if (alarmInfo.AlarmType == AlarmType.RetryAlarm || alarmInfo.AlarmType == AlarmType.WarningTip)
            {
                service.ShowDialog("AlarmDialog", param, callback);
            }
            else
            {
                _alarmWindow = service.Show("HiveAlarmDialog", param, callback);
            }

            if (_alarmWindow != null)
            {
                _alarmWindow.Closing -= __alarmWindow_Closing;
                _alarmWindow.Closing += __alarmWindow_Closing;
                lock (objShowLock)
                    IsStartAgAlarm = true; // 设置标志位，表示正在启动
            }
            else
            {
                string logLineN = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},AlarmDialog null";
                File.AppendAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dialog.csv"), new List<string> { logLineN });
            }
            //}
            //finally
            //{
            //    showAlarmMutex.ReleaseMutex();
            //}
        }
        private static void __alarmWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (sender is Luster.Common.Assets.Views.DialogWindow)
                {
                    var window = sender as Luster.Common.Assets.Views.DialogWindow;
                    if (window != null && window.Result != null && window.Result.Result == ButtonResult.OK)
                    {
                        lock (objShowLock)
                            IsStartAgAlarm = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            finally
            {
                // 防止异常导致IsStartAg未重置
                //if (!e.Cancel)
                lock (objShowLock)
                    IsStartAgAlarm = false;
            }
        }
        [Obsolete]
        private static void _alarmWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is Luster.Common.Assets.Views.DialogWindow)
            {
                var window = sender as Luster.Common.Assets.Views.DialogWindow;
                if (window != null && window.Result != null && window.Result.Result == ButtonResult.OK)
                {

                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        ///// <summary>
        ///// 加载显示属性弹窗
        ///// </summary>
        ///// <param name="service"></param>
        ///// <param name="module"></param>
        //public static void ShowModule(this IDialogService service, string Module, Action<IDialogResult> callback = null)
        //{
        //    service.CloseExistShowWindow(_showWindow);

        //    DialogParameters param = new DialogParameters();
        //    param.Add("Title", langs.LangProvider.GetLang("Alarm"));
        //    param.Add("Module", Module);

        //    service.ShowDialog("ShowModuleDialog", param, callback);
        //}

        /// <summary>
        /// 加载显示班别弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAddClass(this IDialogService service, ClassModel model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();

            if (model != null)
            {
                param.Add("Title", "编辑班别");
                param.Add("ClassName", model.ClassType);
                param.Add("StartTime", model.StartTime);
                param.Add("EndTime", model.EndTime);
            }
            else
            {
                param.Add("Title", "添加班别");
            }
            _showWindow = service.Show("ClassDialog", param, callback);
        }


        public static void ShowErrorPassword(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "密码错误");
            _showWindow = service.Show("ErrorPasswordDialog", param, callback);
        }


        /// <summary>
        /// 加载显示光幕弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowLightCurtain(this IDialogService service, Action<IDialogResult> callback = null)
        {
            //service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();

            param.Add("Title", "设置光幕");

            _showWindow = service.Show("LightCurtainDialog", param, callback);
        }

        /// <summary>
        /// 加载显示机器人状态弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowRobotStatus(this IDialogService service, Action<IDialogResult> callback = null)
        {
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设置机器人状态");
            _showWindow = service.Show("RobotStatusDialog", param, callback);
        }

        /// <summary>
        /// 加载显示产品
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowProduct(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "添加产品");
            _showWindow = service.Show("ProductDialog", param, callback);
        }

        /// <summary>
        /// 设置工站是否显示
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowStationSet(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设置工站");
            _showWindow = service.Show("SetStationDialog", param, callback);
        }

        /// <summary>
        /// 加载设置Plc报警
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAddPlcAlarm(this IDialogService service, PlcAlarm model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();

            if (model != null)
            {
                param.Add("Title", "编辑plc报警");
                param.Add("Address", model.Address);
                param.Add("Desc", model.Desc);
                param.Add("AlarmCode", model.AlarmCode);
            }
            else
            {
                param.Add("Title", "添加Plc报警");
            }
            _showWindow = service.Show("PlcAlarmDialog", param, callback);
        }

        /// <summary>
        /// 加载设置DIY报警
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAddDIYAlarm(this IDialogService service, DIY_Alarm model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();

            if (model != null)
            {
                param.Add("Title", "编辑自定义报警");
                param.Add("AlarmID", model.AlarmID);
                param.Add("AlarmContent", model.AlarmContent);
                param.Add("AlarmSolution", model.AlarmSolution);
            }
            else
            {
                param.Add("Title", "添加自定义报警");
            }
            _showWindow = service.Show("DIYAlarmDialog", param, callback);
        }


        /// <summary>
        /// 设置工站是否显示
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowOperationTip(this IDialogService service, OperationTip tip = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "操作提示");
            param.Add("Model", tip);
            _showWindow = service.Show("AddOperationTipDialog", param, callback);
        }

        /// <summary>
        ///  HIVE 暂停/停止 弹窗提示
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSelectTip(this IDialogService service, bool enable, SystemOperation operation = SystemOperation.Home, Action<IDialogResult> callback = null)
        {
            if (_alarmWindow != null)
            {
                _alarmWindow.Closing -= __alarmWindow_Closing;
            }
            service.CloseExistShowWindow(_alarmWindow);

            if (_showWindow != null)
            {
                _showWindow.Closing -= _showWindow_Closing;
            }
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "Hive");
            param.Add("Operation", operation);

            if (enable)
            {
                _showWindow = service.Show("HiveOperationDialog", param, callback);
                _showWindow.Closing += _showWindow_Closing;
            }
            else
            {
                _showWindow = service.Show("ShowOperationTipDialog", param, callback);
            }
        }

        private static void _showWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (sender is Luster.Common.Assets.Views.DialogWindow)
                {
                    var window = sender as Luster.Common.Assets.Views.DialogWindow;
                    if (window != null && window.Result != null && window.Result.Result == ButtonResult.OK)
                    {
                        IsStartAg = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            finally
            {
                // 防止异常导致IsStartAg未重置
                //if (!e.Cancel)
                IsStartAg = false;
            }
        }




        /// <summary>
        /// HIVE 启动 弹窗提示
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void HiveRunPopUp(this IDialogService service, SystemOperation operation = SystemOperation.Home, Action<IDialogResult> callback = null)
        {
            DialogExtensionJ.HiveRunPopUp(service, operation, callback);
            return;

            // 追加写入CSV文件，记录时间和详细描述
            string csvPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dialog.csv");
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{IsStartAg},HiveRunPopUp called";
            try
            {
                bool fileExists = System.IO.File.Exists(csvPath);
                using (var sw = new System.IO.StreamWriter(csvPath, true, Encoding.UTF8))
                {
                    if (!fileExists)
                    {
                        sw.WriteLine("Time,IsStartAg,Description");
                    }
                    sw.WriteLine(logLine);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"写入CSV日志失败: {ex.Message}");
            }
            if (IsStartAg || IsStartAgAlarm)
            {
                return;
            }
            //关闭已有的弹窗，重新赋值对象为null
            service.CloseExistShowWindow(_showWindow);
            if (_showWindow != null)
            {
                _showWindow.Closing -= _showWindow_Closing;
                _showWindow = null;//取消之前事件的挂载
            }
            //开启新的弹窗，注册事件
            DialogParameters param = new DialogParameters();
            param.Add("Title", "Hive");
            param.Add("Operation", operation);
            _showWindow = service.Show("HiveStartDialog2", param, callback);
            if (_showWindow != null)
            {
                _showWindow.Closing -= _showWindow_Closing;
                _showWindow.Closing += _showWindow_Closing;
                IsStartAg = true; // 设置标志位，表示正在启动
            }
            else
            {
                string logLineN = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},HiveStartDialog2 null";
                File.AppendAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dialog.csv"), new List<string> { logLineN });
            }
        }
        /// <summary>
        /// HIVE 上传报告 弹窗提示
        /// </summary>
        public static void ShowHiveReportDialog(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_hiveReportWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "Hive上传维修信息");
            _hiveReportWindow = service.Show("HiveStartDialog2Part2", param, callback);
            //_hiveReportWindow.Closing += _showWindow_Closing;
        }

        /// <summary>
        /// HIVE 状态条报警 弹窗弹出
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void HiveOnboardAlarmPopUp(this IDialogService service, Action<IDialogResult> callback = null)
        {
            if (_isOnboardAlarm) return;
            _isOnboardAlarm = true;
            //开启新的弹窗，注册事件
            DialogParameters param = new DialogParameters();
            param.Add("Title", "HiveOnboardAlarm");
            _hiveOnWindow = service.Show("HiveOnboardAlarmDialog", param, r =>
            {
                _isOnboardAlarm = false;
                callback?.Invoke(r);
            });
        }
        /// <summary>
        /// HIVE 状态条报警 弹窗关闭
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void HiveOnboardAlarmClosed(this IDialogService service, Action<IDialogResult> callback = null)
        {
            if (!_isOnboardAlarm) return;
            _isOnboardAlarm = false;
            //关闭已有的弹窗，重新赋值对象为null
            service.CloseExistShowWindow(_hiveOnWindow);
            if (_hiveOnWindow != null)
            {
                _hiveOnWindow = null;//取消之前事件的挂载
            }

        }


        /// <summary>
        /// 设置工站是否显示
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowChangeAccessory(this IDialogService service, string name = "", Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "更换物料");
            param.Add("AccessoryName", name);
            _showWindow = service.Show("ChangeAccessoryDialog", param, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowWorkFlowSet(this IDialogService service, WorkItem wItem, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设置工作流");
            param.Add("WorkItem", wItem);
            _showWindow = service.Show("SetWorkFlowDialog", param, callback);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowHive1Dialog(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            if (_showWindow != null)
            {
                _showWindow.Closing -= _showWindow_Closing;
            }
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设备保养");
            param.Add("Reason", reason);
            _showWindow = service.Show("HiveDialog1", param, callback);
            _showWindow.Closing += _showWindow_Closing;
        }


        public static void ShowHiveDialog(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设备保养");
            param.Add("Reason", reason);
            _showWindow = service.Show("HiveDialog", param, callback);
        }



        public static void ShowHiveStopDialog(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "停机");
            _showWindow = service.Show("HiveStopDialog", param, callback);
            _showWindow.Closing += _showWindow_Closing;
        }


        public static void ShowHiveStopDialog2(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            // service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "停机");
            _showWindow = service.Show("HiveStopDialog2", param, callback);
        }

        public static void ShowHiveStopDialog3(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            // service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "周保养/Weekly Maintenance");
            _showWindow = service.Show("HiveStopDialog3", param, callback);
        }

        public static void ShowHiveStopDialog4(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            // service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "月保养/Monthly Maintenance");
            _showWindow = service.Show("HiveStopDialog4", param, callback);
        }

        public static void ShowHivePauseDialog(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            //service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "暂停");
            _showWindow = service.Show("HivePauseDialog", param, callback);
            _showWindow.Closing += _showWindow_Closing;
        }

        public static void ShowVersionDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "版本信息");
            _showWindow = service.Show("VersionDialog", param, callback);
        }

        public static void ShowHomeDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "回零确认");
            service.ShowDialog("HomeConfirmDialog", param, callback);
        }

        /// <summary>
        /// 设置开关门IO
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSetOpenCloseDoor(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "开关门设置");
            _showWindow = service.Show("SetOpenCloseDoorDialog", param, callback);
        }


        /// <summary>
        /// 启动，暂停，停止设置IO
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSysOperateIOSet(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "设置");
            _showWindow = service.Show("SetSysOperateIODialog", param, callback);
        }



        /// <summary>
        /// HIVE 启动 弹窗提示
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowInteractiveDialog(this IDialogService service, string reason, Action<IDialogResult> callback = null)
        {
            //service.CloseExistShowWindow(_showWindow);
            //DialogParameters param = new DialogParameters();
            //param.Add("Title", "Hive");
            //param.Add("Operation", reason);
            //_showWindow = service.Show("InteractiveDialog", param, callback);


            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "交互对话框");
            param.Add("Tip", reason);
            service.ShowDialog("InteractiveDialog", param, callback);
        }

        public static void ShowCSharpEditorDialog(this IDialogService service, IMotionModule module, string ID, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow(_showWindow);
            DialogParameters param = new DialogParameters();
            param.Add("Title", "C#脚本编辑");
            param.Add("ID", ID);
            param.Add("Module", module);
            service.ShowDialog("CSharpEditor", param, callback);
        }
    }
}