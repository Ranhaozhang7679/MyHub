#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MaintainItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       MaintainItemModel.cs
* 创建时间:       2022/12/9 10:55:36
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      19f317a9-8d7b-4435-80a5-4bbfaa5dc9cb
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/9 10:55:36
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.Real;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.SimDevice.EngineUI.Models
{
    public class ErrorItemModel : BindableBase
    {
        /// <summary>
        /// 错误类型
        /// </summary>
        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value); }
        }

        /// <summary>
        /// 错误类型
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                string src = _name;
                SetProperty(ref _name, value);
                if (Tag != null && src != value)
                {
                    var vDevice = Tag as Luster.Motion.DataStruct.Virtual.VirtualDeviceBase;
                    if (vDevice?.ErrorNames != null)
                        vDevice.ErrorNames[ErrorType] = value;
                }
            }
        }

        public DeviceError ErrorType { get; set; }

        /// <summary>
        /// 错误类型
        /// </summary>
        private string _errorCode = "10000";
        public string ErrorCode
        {
            get { return _errorCode; }
            set
            {
                string src = _errorCode;
                SetProperty(ref _errorCode, value);
                if (Tag != null && src != value)
                    Tag.Errors[ErrorType] = value;
            }
        }

        /// <summary>
        /// 错误代码英文描述
        /// </summary>
        private string errorForeignMessage;
        public string ErrorForeignMessage
        {
            get { return errorForeignMessage; }
            set {
                string src = errorForeignMessage;
                SetProperty(ref errorForeignMessage, value);
                if (Tag != null && src != value)
                {
                    var vDevice = Tag as Luster.Motion.DataStruct.Virtual.VirtualDeviceBase;
                    if (vDevice?.ErrorMessages != null)
                        vDevice.ErrorMessages[ErrorType] = value;
                }
            }

        }


        public IDeviceError Tag { get; set; }

        /// <summary>
        /// 报警种类下拉选项列表
        /// </summary>
        public List<string> AlarmCategoryOptions => ErrorItemCustomModel.AlarmCategoryOptions;

        /// <summary>
        /// 报警种类
        /// </summary>
        private string _alarmCategory;
        public string AlarmCategory
        {
            get { return _alarmCategory; }
            set
            {
                string src = _alarmCategory;
                SetProperty(ref _alarmCategory, value);
                if (Tag != null && src != value)
                {
                    var vDevice = Tag as Luster.Motion.DataStruct.Virtual.VirtualDeviceBase;
                    if (vDevice?.ErrorAlarmCategories != null)
                        vDevice.ErrorAlarmCategories[ErrorType] = value;
                }
            }
        }

        /// <summary>
        /// 维修动作
        /// </summary>
        private string _repairAction;
        public string RepairAction
        {
            get { return _repairAction; }
            set
            {
                string src = _repairAction;
                SetProperty(ref _repairAction, value);
                if (Tag != null && src != value)
                {
                    var vDevice = Tag as Luster.Motion.DataStruct.Virtual.VirtualDeviceBase;
                    if (vDevice?.ErrorRepairActions != null)
                        vDevice.ErrorRepairActions[ErrorType] = value;
                }
            }
        }


        /// <summary>
        /// 错误代码
        /// </summary>
        /// <param name="errType"></param>
        /// <param name="errCode"></param>
        public ErrorItemModel(IDeviceError tag, KeyValuePair<DeviceError, string> error)
        {
            DeviceName = (tag as IVirtualDevice).Name;
            ErrorType = error.Key;
            ErrorCode = error.Value;
            Tag = tag;
            // 优先读取自定义名称，否则使用枚举描述
            var vDevice = tag as Luster.Motion.DataStruct.Virtual.VirtualDeviceBase;
            if (vDevice?.ErrorNames != null && vDevice.ErrorNames.TryGetValue(error.Key, out var customName) && !string.IsNullOrEmpty(customName))
                _name = customName;
            else
                _name = error.Key.GetDescription();
            if (vDevice?.ErrorMessages != null && vDevice.ErrorMessages.TryGetValue(error.Key, out var msg))
                errorForeignMessage = msg;
            else
                errorForeignMessage = tag.ErrorMessage;
            if (vDevice?.ErrorAlarmCategories != null && vDevice.ErrorAlarmCategories.TryGetValue(error.Key, out var cat))
                _alarmCategory = cat;
            else
                _alarmCategory = vDevice?.AlarmCategory;
            if (vDevice?.ErrorRepairActions != null && vDevice.ErrorRepairActions.TryGetValue(error.Key, out var act))
                _repairAction = act;
            else
                _repairAction = vDevice?.RepairAction;
        }
    }
}
