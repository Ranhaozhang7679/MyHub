using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class AlarmCodeDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 设备引擎
        /// </summary>
        private IDeviceEngine _deviceEngine;
        public AlarmCodeDialogVM(IDeviceEngine dEngine)
        {
            _deviceEngine = dEngine;

            AlarmCodeList = _deviceEngine?.GetDevices(typeof(VAlarm))?.OfType<VAlarm>().ToList() ?? new List<VAlarm>();
        }

        /// <summary>
        /// 设备列表 IVirtualDevice
        /// </summary>
        private List<VAlarm> _alarmCodeList;
        public List<VAlarm> AlarmCodeList
        {
            get { return _alarmCodeList; }
            set { SetProperty(ref _alarmCodeList, value); }
        }

        /// <summary>
        /// 选择对象
        /// </summary>
        private DelegateCommand<VAlarm> _selectCommand;
        public DelegateCommand<VAlarm> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<VAlarm>(device =>
        {
            IDialogResult r = new DialogResult(ButtonResult.OK);
            r.Parameters.Add("VAlarm", device);
            r.Parameters.Add("SearchKey", SearchKey);
            RaiseRequestClose(r);
        }));

        /// <summary>
        /// 当前类型
        /// </summary>
        private Type _currentType;
        public Type CurrentType
        {
            get { return _currentType; }
            set { SetProperty(ref _currentType, value); }
        }

        /// <summary>
        /// 查找对象
        /// </summary>
        private DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(() =>
        {
            if (CurrentType != null)
            {
                FilterItems(CurrentType);
            }
        }));

        /// <summary>
        /// 查询关键字
        /// </summary>
        private string _searchKey;
        public string SearchKey
        {
            get { return _searchKey; }
            set { SetProperty(ref _searchKey, value); }
        }

        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters">参数列表</param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<ParameterAttribute>("Parameter", out var pAttr))
            {
                Type editorType = null;
                if (pAttr.EditorType != null)
                {
                    editorType = pAttr.EditorType;
                    CurrentType = editorType;
                }

                if (parameters.TryGetValue<string>("SearchKey", out var sKey))
                {
                    SearchKey = sKey;
                }

                // 通过参数来加载设备列表
                FilterItems(editorType);
            }
        }

        /// <summary>
        /// 查询筛选
        /// </summary>
        private void FilterItems(Type editorType)
        {
            // 直接将 GetDevices 的结果转换为 VAlarm 序列，处理 null 情况
            var vAlarms = _deviceEngine.GetDevices(editorType)?.OfType<VAlarm>() ?? Enumerable.Empty<VAlarm>();

            if (string.IsNullOrEmpty(SearchKey))
            {
                // 无查询关键字，直接赋值所有 VAlarm
                AlarmCodeList = vAlarms.ToList();
            }
            else
            {
                var key = SearchKey;
                AlarmCodeList = vAlarms
                    .Where(u =>
                        (!string.IsNullOrEmpty(u.AlarmKey) && u.AlarmKey.Contains(key)) ||
                        (!string.IsNullOrEmpty(u.AlarmCN) && u.AlarmCN.Contains(key)) ||
                        (!string.IsNullOrEmpty(u.AlarmEn) && u.AlarmEn.Contains(key))
                    )
                    .ToList();
            }
        }
    } 
}
