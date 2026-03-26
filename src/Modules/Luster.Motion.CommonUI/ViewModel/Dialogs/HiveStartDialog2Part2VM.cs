using HandyControl.Controls;
using Luster.Common.Assets;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveStartDialog2Part2VM : MotionDialogVM
    {
        /// <summary>
        /// 宕机原因list///
        /// </summary>
        private List<KeyValue> _downTimeLists;
        public List<KeyValue> DownTimeLists
        {
            get { return _downTimeLists; }
            set { SetProperty(ref _downTimeLists, value); }
        }

        /// <summary>
        /// 宕机原因
        /// </summary>
        private string _downTimeCause;
        public string DownTimeCause
        {
            get { return _downTimeCause; }
            set { SetProperty(ref _downTimeCause, value); }
        }


        /// <summary>
        /// 消耗物品list///
        /// </summary>
        private List<KeyValue> _sparePartLists;
        public List<KeyValue> SparePartLists
        {
            get { return _sparePartLists; }
            set { SetProperty(ref _sparePartLists, value); }
        }

        /// <summary>
        /// 消耗物品
        /// </summary>
        private string _sparePart;
        public string SparePart
        {
            get { return _sparePart; }
            set { SetProperty(ref _sparePart, value); }
        }

        /// <summary>
        /// 维修动作
        /// </summary>
        private string _actionTaken;
        public string ActionTaken
        {
            get { return _actionTaken; }
            set { SetProperty(ref _actionTaken, value); }
        }

        /// <summary>
        /// 等级list///
        /// </summary>
        private List<KeyValue> _rateLists;
        public List<KeyValue> RateLists
        {
            get { return _rateLists; }
            set { SetProperty(ref _rateLists, value); }
        }

        private string _action;
        public string Action
        {
            get { return _action; }
            set { SetProperty(ref _action, value); }
        }
        /// <summary>
        /// 检查步骤list///
        /// </summary>
        private List<KeyValue> _actionLists;
        public List<KeyValue> ActionLists
        {
            get { return _actionLists; }
            set { SetProperty(ref _actionLists, value); }
        }

        private string _tool;
        public string Tool
        {
            get { return _tool; }
            set { SetProperty(ref _tool, value); }
        }
        /// <summary>
        /// 用到工具list///
        /// </summary>
        private List<KeyValue> _toolLists;
        public List<KeyValue> ToolLists
        {
            get { return _toolLists; }
            set { SetProperty(ref _toolLists, value); }
        }

        private string _setting;
        public string Setting
        {
            get { return _setting; }
            set { SetProperty(ref _setting, value); }
        }
        /// <summary>
        /// 调整参数list///
        /// </summary>
        private List<KeyValue> _settingLists;
        public List<KeyValue> SettingLists
        {
            get { return _settingLists; }
            set { SetProperty(ref _settingLists, value); }
        }

        /// <summary>
        /// 等级///
        /// </summary>
        private string _rate;
        public string Rate
        {
            get { return _rate; }
            set { SetProperty(ref _rate, value); }
        }

        /// <summary>
        /// 刷卡提示
        /// </summary>
        private string _cardLog;
        public string CardLog
        {
            get { return _cardLog; }
            set { SetProperty(ref _cardLog, value); }
        }

        private IMotionController mController;
        private IDialogService _dialogService;
        private HiveAPI _hiveAPI;
        private readonly ICommonBus _bus;

        private HiveStartDialog2Part2VM(IMotionController _motionController, IDialogService dialogService, HiveAPI hiveAPI, ICommonBus commonBus)
        {
            mController = _motionController;
            _dialogService = dialogService;
            _hiveAPI = hiveAPI;
            _bus = commonBus;

            DownTimeLists = new List<KeyValue>();
            SparePartLists = new List<KeyValue>();
            List<string> dwTimes = mController.FileConfig.DownTimeCauses.Split('@').ToList();
            foreach (var rate in dwTimes)
            {
                if (rate != null) DownTimeLists.Add(new KeyValue() { Desc = rate });
            }

            List<string> spParts = mController.FileConfig.SparePartsUsed.Split('@').ToList();
            foreach (var part in spParts)
            {
                if (part != null) SparePartLists.Add(new KeyValue() { Desc = part });
            }
            RateLists = new List<KeyValue>();
            RateLists.Add(new KeyValue() { Desc = "1" });
            RateLists.Add(new KeyValue() { Desc = "2" });
            RateLists.Add(new KeyValue() { Desc = "3" });
            RateLists.Add(new KeyValue() { Desc = "4" });
            RateLists.Add(new KeyValue() { Desc = "5" });

            // 手动新增，后续需改成配置文件
            ActionLists = new List<KeyValue>();
            ActionLists.Add(new KeyValue() { Desc = "机台点检" });
            ActionLists.Add(new KeyValue() { Desc = "检测气缸" }); // Detect cylinder;
            ActionLists.Add(new KeyValue() { Desc = "螺丝" });     // Screw;
            ActionLists.Add(new KeyValue() { Desc = "气管" });     // Trachea;
            ActionLists.Add(new KeyValue() { Desc = "联轴器" });   // Coupling;
            ActionLists.Add(new KeyValue() { Desc = "驱动器" });   // Drive;
            ActionLists.Add(new KeyValue() { Desc = "电机" });     // Motor;
            ActionLists.Add(new KeyValue() { Desc = "传感器" });   // Sensor;
            ActionLists.Add(new KeyValue() { Desc = "继电器" });   // Relay;
            ActionLists.Add(new KeyValue() { Desc = "IO" });       // IO;
            ActionLists.Add(new KeyValue() { Desc = "机械手" });   // Manipulator;
            ActionLists.Add(new KeyValue() { Desc = "电磁阀" });   // Solenoid valve;
            ActionLists.Add(new KeyValue() { Desc = "其他" });

            // 1. 把配置字符串拆成唯一集合
            var newActions = mController.FileConfig.ActionLists ?.Split('@').Select(x => x.Trim())   // 去掉首尾空格
                                .Where(x => !string.IsNullOrEmpty(x)).Distinct()                     // 去重
                            ?? Enumerable.Empty<string>();
            // 2. 与现有列表合并（仍然保持唯一）
            ActionLists = ActionLists.Select(kv => kv.Desc)     // 只拿出 Desc 当 key
                            .Concat(newActions)                 // 拼接新项
                            .Distinct().Select(desc => new KeyValue { Desc = desc }).ToList();

            ToolLists = new List<KeyValue>();
            ToolLists.Add(new KeyValue() { Desc = "六角扳手" });   // Hex wrench;
            ToolLists.Add(new KeyValue() { Desc = "螺丝刀" });     // Screwdriver;
            ToolLists.Add(new KeyValue() { Desc = "万用表" });     // Multimeter;
            ToolLists.Add(new KeyValue() { Desc = "斜口钳" });     // Diagonal pliers;
            ToolLists.Add(new KeyValue() { Desc = "开口扳手" });   // Open end wrench;
            ToolLists.Add(new KeyValue() { Desc = "无" });

            var newTools = mController.FileConfig.ToolLists?.Split('@').Select(x => x.Trim())
                                .Where(x => !string.IsNullOrEmpty(x)).Distinct() ?? Enumerable.Empty<string>();
            ToolLists = ToolLists.Select(kv => kv.Desc).Concat(newTools).Distinct().Select(desc => new KeyValue { Desc = desc }).ToList();

            SettingLists = new List<KeyValue>();
            SettingLists.Add(new KeyValue() { Desc = "气压" });    // Air pressure;
            SettingLists.Add(new KeyValue() { Desc = "点位" });    // Point;
            SettingLists.Add(new KeyValue() { Desc = "速度" });    // Speed;
            SettingLists.Add(new KeyValue() { Desc = "时间" });    // Time;
            SettingLists.Add(new KeyValue() { Desc = "阈值" });    // Threshold;
            SettingLists.Add(new KeyValue() { Desc = "压力" });    // Pressure;
            SettingLists.Add(new KeyValue() { Desc = "曝光" });    // Exposure;
            SettingLists.Add(new KeyValue() { Desc = "无" });

            var newSettings = mController.FileConfig.SettingLists?.Split('@').Select(x => x.Trim())
                                .Where(x => !string.IsNullOrEmpty(x)).Distinct()?? Enumerable.Empty<string>();
            SettingLists = SettingLists.Select(kv => kv.Desc).Concat(newSettings).Distinct().Select(desc => new KeyValue { Desc = desc }).ToList();

        }

        private bool _canClose = false;
        public override bool CanCloseDialog()
        {
            return _canClose;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private DelegateCommand<object> _clickCommand;
        public DelegateCommand<object> ClickCommand => _clickCommand ?? (_clickCommand = new DelegateCommand<object>((o) =>
        {
            if (mController.FileConfig.IsHiveContinueMaintenanceVisible)
            {
                //如果必填项目为空，则不进行后续流程
                if (string.IsNullOrEmpty(DownTimeCause) || string.IsNullOrEmpty(SparePart) || string.IsNullOrEmpty(ActionTaken))
                {
                    CardLog = $"请填写宕机原因、消耗物品和检查步骤";
                    //CardBkColor = new SolidColorBrush(Color.FromRgb(255, 83, 88));
                    return;
                }
            }
            else
            {
                //如果必填项目为空，则不进行后续流程
                if (string.IsNullOrEmpty(DownTimeCause))
                {
                    CardLog = $"请填写宕机原因";
                    //CardBkColor = new SolidColorBrush(Color.FromRgb(255, 83, 88));
                    return;
                }
                mController.FileConfig.IsHiveContinueMaintenanceVisible = true;
            }

            _canClose = true; // 允许关闭
            base.CloseDialog("ok");
            _canClose = false; // 关闭后立即恢复禁止关闭
            // Report end上传
            _hiveAPI.EndAlarmReport(DownTimeCause, ActionTaken + ";" + Tool + ";" + Setting, SparePart, Rate);
            _bus.EventBus.GetEvent<HiveReportStateChangedEvent>().Publish(false);

        }));

    }
}
