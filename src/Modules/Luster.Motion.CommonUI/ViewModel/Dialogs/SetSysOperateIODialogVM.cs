using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class SetSysOperateIODialogVM : MotionDialogVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IDeviceEngine _deviceEngine;

        public SetSysOperateIODialogVM(IDeviceEngine dEngine)
        {
            _deviceEngine = dEngine;

        }

        /// <summary>
        /// 门锁名称
        /// </summary>
        private SystemOperation _sysOperateype= SystemOperation.Start;
        public SystemOperation SysOperateype
        {
            get { return _sysOperateype; }
            set
            {
                SetProperty(ref _sysOperateype, value);
                IsIn = false;               
                SearchIOs();
                SelectVarDatas = new ObservableCollection<IVirtualDevice>();
            }
        }


        private ObservableCollection<KeyValue> sysOperationlist;
        public ObservableCollection<KeyValue> SysOperationlist
        {
            get { return sysOperationlist; }
            set { SetProperty(ref sysOperationlist, value); }
        }


        /// <summary>
        /// 是否可以选
        /// </summary>
        private bool _isIn = false;
        public bool IsIn
        {
            get { return _isIn; }
            set { SetProperty(ref _isIn, value); }
        }


        /// <summary>
        /// 原始数据变量集
        /// </summary>
        private ObservableCollection<IVirtualDevice> _orignvarDatas;
        public ObservableCollection<IVirtualDevice> OrignVarDatas
        {
            get { return _orignvarDatas; }
            set { SetProperty(ref _orignvarDatas, value); }
        }

        /// <summary>
        /// 左侧变量集
        /// </summary>
        private ObservableCollection<IVirtualDevice> _leftDatas;
        public ObservableCollection<IVirtualDevice> LeftDatas
        {
            get { return _leftDatas; }
            set { SetProperty(ref _leftDatas, value); }
        }

        /// <summary>
        /// 右侧变量集
        /// </summary>
        private ObservableCollection<IVirtualDevice> _selectVarDatas;
        public ObservableCollection<IVirtualDevice> SelectVarDatas
        {
            get { return _selectVarDatas; }
            set { SetProperty(ref _selectVarDatas, value); }
        }

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
        /// 查找对象
        /// </summary>
        private DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(() =>
        {
            if (SysOperateype != null)
            {
                SearchIOs();
            }
        }));

        /// <summary>
        /// 选择IO
        /// </summary>
        private DelegateCommand<object> _reciveDropCommand;
        public DelegateCommand<object> ReciveDropCommand => _reciveDropCommand ?? (_reciveDropCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj == null) { return; }
            var source = obj as DragEventArgs;
            if (source != null)
            {
                var node = source.Data.GetData(typeof(VIO)) as IVirtualDevice;
                if (node != null)
                {
                    var mode = OrignVarDatas.FirstOrDefault(u => u.ID == node.ID);
                    if (mode != null)
                    {
                        SelectVarDatas.Add(mode);
                        //OrignVarDatas.Remove(mode);
                        SearchIOs();
                    }
                }
            }
        }));

        /// <summary>
        /// 撤回选择
        /// </summary>
        private DelegateCommand<object> _recallDropCommand;
        public DelegateCommand<object> ReCallDropCommand => _recallDropCommand ?? (_recallDropCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj == null) { return; }
            var source = obj as DragEventArgs;
            if (source != null)
            {
                var node = source.Data.GetData(typeof(VIO)) as IVirtualDevice;
                if (node != null)
                {
                    var mode = SelectVarDatas.FirstOrDefault(u => u.ID == node.ID);
                    if (mode != null)
                    {
                        //OrignVarDatas.Add(mode);
                        SelectVarDatas.Remove(mode);
                        SearchIOs();
                    }
                }
            }
        }));

        /// <summary>
        /// 删除选择
        /// </summary>
        private DelegateCommand<object> _deleteCommand;
        public DelegateCommand<object> DeleteCommand => _deleteCommand ?? (_deleteCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj == null) { return; }

            var node = obj as IVirtualDevice;
            if (node != null)
            {
                var mode = SelectVarDatas.FirstOrDefault(u => u.ID == node.ID);
                if (mode != null)
                {
                    //OrignVarDatas.Add(mode);
                    SelectVarDatas.Remove(mode);
                    SearchIOs();
                }
            }
        }));

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
       
            if (SelectVarDatas != null)
            {
                result.Parameters.Add("SelectIOList", SelectVarDatas.ToList());
            }

            result.Parameters.Add("SysOperateype", SysOperateype);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;

            }
            else
            {
                Title = string.Empty;
            }
            SelectVarDatas = new ObservableCollection<IVirtualDevice>();
            var operateList = typeof(SystemOperation).EnumToDataSource();
            SysOperationlist = new ObservableCollection<KeyValue>();
            if (operateList != null)
            {
                foreach (var item in operateList)
                {
                    if (item.Desc == SystemOperation.Alarm.GetDescription() || item.Desc == SystemOperation.Pause.GetDescription() || item.Desc == SystemOperation.Stop.GetDescription())
                    {
                        SysOperationlist.Add(item);
                    }
                }
            }
        }


        private void SearchIOs()
        {
            OrignVarDatas = new ObservableCollection<IVirtualDevice>();
            List<IVirtualDevice> devices = null;
            if (string.IsNullOrEmpty(SearchKey))
            {
                // 通过参数来加载设备列表
                devices = _deviceEngine.GetDevices(typeof(VIO));
            }
            else
            {
                devices = _deviceEngine.GetDevices(typeof(VIO)).
                    Where(u => u.Name.Contains(SearchKey) || (!string.IsNullOrEmpty(u.Module) && u.Module.Contains(SearchKey))).
                    ToList();
            }

     
                IOBehavior behavior = IsIn ? IOBehavior.Input : IOBehavior.Output;
                devices = devices.Where(u => u is VIO io && io.Behavior == behavior).Cast<IVirtualDevice>().ToList();
            devices.ForEach(u => OrignVarDatas.Add(u));
            foreach (var item in SelectVarDatas)
            {
                var selectitem=OrignVarDatas.FirstOrDefault(u=>u.Name==item.Name);
                if (selectitem != null)
                {
                    OrignVarDatas.Remove(selectitem);
                }
               
            }
        }

        private void BuildLeftTreeVars()
        {
            OrignVarDatas = new ObservableCollection<IVirtualDevice>();
            SelectVarDatas = new ObservableCollection<IVirtualDevice>();
            List<IVirtualDevice> devices = null;
            devices = _deviceEngine.GetDevices(typeof(VIO));
            IOBehavior behavior = IOBehavior.Output;
            devices = devices.Where(u => u is VIO io && io.Behavior == behavior).Cast<IVirtualDevice>().ToList();
            devices.ForEach(u => OrignVarDatas.Add(u));


        }


    }
}
