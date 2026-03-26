#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel.Dialogs
* 文 件 名:       DeviceDialogVM.cs
* 创建时间:       2022/6/9 15:00:49
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ed3fac66-f055-47d9-97b1-de0af1e952ad
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/9 15:00:49
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets.ViewModel;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    /// <summary>
    /// Dialog
    /// </summary>
    public sealed class DeviceDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 设备引擎
        /// </summary>
        private IDeviceEngine _deviceEngine;
        public DeviceDialogVM(IDeviceEngine dEngine)
        {
            _deviceEngine = dEngine;
            VTypes = _deviceEngine.GetVirtualTypes().Select(u => new KeyValue()
            {
                Value = u,
                Desc = Path.GetFileName(u.Name)
            }).ToList();
        }

        /// <summary>
        /// 设备列表 IVirtualDevice
        /// </summary>
        private List<IVirtualDevice> _deviceList;
        public List<IVirtualDevice> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }

        /// <summary>
        /// 虚拟设备类型
        /// </summary>
        private List<KeyValue> _vTypes;
        public List<KeyValue> VTypes
        {
            get { return _vTypes; }
            set { SetProperty(ref _vTypes, value); }
        }

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
        /// 是否可以选
        /// </summary>
        private bool _canSelected;
        public bool CanSelected
        {
            get { return _canSelected; }
            set { SetProperty(ref _canSelected, value); }
        }

        /// <summary>
        /// 是否可以选
        /// </summary>
        private bool _isIO = false;
        public bool IsIO
        {
            get { return _isIO; }
            set { SetProperty(ref _isIO, value); }
        }

        /// <summary>
        /// 是否可以选
        /// </summary>
        private bool _isIn = true;
        public bool IsIn
        {
            get { return _isIn; }
            set
            {
                SetProperty(ref _isIn, value);
                FilterItems(CurrentType);
            }
        }

        /// <summary>
        /// 选择对象
        /// </summary>
        private DelegateCommand<IVirtualDevice> _selectCommand;
        public DelegateCommand<IVirtualDevice> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<IVirtualDevice>(device =>
             {
                 IDialogResult r = new DialogResult(ButtonResult.OK);
                 r.Parameters.Add("Device", device);
                 r.Parameters.Add("SearchKey", SearchKey);
                 RaiseRequestClose(r);
             }));

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
                    CanSelected = false;

                    IsIO = editorType == typeof(VIO);
                    if (IsIO)
                    {
                        IsIn = true;
                    }
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
        /// 设备类别筛选
        /// </summary>
        /// <param name="editorType">设备类型</param>
        /// <param name="key"></param>
        private void FilterItems(Type editorType)
        {
            List<IVirtualDevice> devices = null;
           
            if (string.IsNullOrEmpty(SearchKey))
            {
                // 通过参数来加载设备列表
                 devices = _deviceEngine.GetDevices(editorType);
            }
            else
            {
                devices = _deviceEngine.GetDevices(editorType).
                    Where(u => u.Name.Contains(SearchKey) || (!string.IsNullOrEmpty(u.Module) && u.Module.Contains(SearchKey))).
                    ToList();
            }

            if (IsIO)
            {
                IOBehavior behavior = IsIn ? IOBehavior.Input : IOBehavior.Output;
                devices = devices.Where(u => u is VIO io && io.Behavior == behavior).Cast<IVirtualDevice>().ToList();
            }

            DeviceList = devices;
        }
    }
}