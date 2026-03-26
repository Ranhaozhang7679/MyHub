#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MaintainContentVm
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       MaintainContentVm.cs
* 创建时间:       2022/12/9 9:10:18
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      9a8ae735-374c-4b1d-b7a9-f7a26c9e4280
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/9 9:10:18
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class ModuleNameContentVM : PageVM
    {
        public override bool IsShowAdd => true;

        private IDialogService _dialogService;
        public ISimDeviceEngineUI simDeviceEngineUI;


        private ObservableCollection<ModuleNameVM> selectedList;
        public ObservableCollection<ModuleNameVM> SelectedList
        {
            get { return selectedList; }
            set { SetProperty(ref selectedList, value); }
        }

        private ObservableCollection<ModuleNameVM> _moduleDatas;
        public ObservableCollection<ModuleNameVM> ModuleDatas
        {
            get { return _moduleDatas; }
            set { SetProperty(ref _moduleDatas, value); }
        }

        protected ModuleNameContentVM(ISimDeviceEngineUI _engine,　IDialogService dialogService ) : base(_engine)
        {
            simDeviceEngineUI = _engine;
            _dialogService = dialogService;

            ModuleDatas = new ObservableCollection<ModuleNameVM>();

            InitModule();
        }

        /// <summary>
        /// 根据类型初始化Model
        /// </summary>
        /// <param name="type"></param>
        private void InitModule()
        {
            ModuleDatas.Clear();
            var devices = deviceEngine.ModuleNameGroup;
            foreach (var device in devices)
            {
                var model = new ModuleNameVM(device);
                ModuleDatas.Add(model);
            }
        }

        private DelegateCommand<MaintainDeviceTypeItem> _selectedCommand;
        public DelegateCommand<MaintainDeviceTypeItem> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<MaintainDeviceTypeItem>(item =>
        {
        }));

        public override void AddNewItem()
        {
            string name = "";

            dialogService.ShowModuleNameDialog( r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var moduleUsed = deviceEngine.GetModulesUsed();
                    r.Parameters.TryGetValue<string>("Name", out var name);

                    if (moduleUsed.Contains(name))
                    {
                        throw new FriendlyException($"模组名称:{name}已存在,无法添加!");
                    }
                    else
                    {
                        deviceEngine.AddModuleNameGroup(name);

                        ModuleDatas.Add(new ModuleNameVM(new ModuleNameModel() { Name = name }));
                    }
                }

            });
        }

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as ModuleNameVM;
            if (model != null)
            {
                dialogService.ShowConfirm($"确认删除:{model.Name}项?", r =>
                {
                    if (r.Result == ButtonResult.OK && deviceEngine != null)
                    {
                        var moduleUsed = deviceEngine.GetModulesUsed();
                        if (moduleUsed.Contains(model.Name))
                        {
                            throw new FriendlyException($"模组名称:{model.Name}被引用,无法删除!");
                        }
                        else
                        {
                            deviceEngine.RemoveModuleNameGroup(model.Name);
                            ModuleDatas.Remove(model);
                        }
                    }
                });
            }
        }));

        public override void RemoveItem()
        {
            if (SelectedList.Count == 0)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, "请先选择要进行删除的项");
            }

            // 删除确认
            dialogService.ShowConfirm($"确认删除{SelectedList.Count}项?", (r) =>
            {
                if (r.Result == ButtonResult.OK && deviceEngine != null)
                {
                    var moduleUsed = deviceEngine.GetModulesUsed();
                    foreach (var item in SelectedList)
                    {
                        // 查找并删除对应的 VAlarm 设备
                        if (moduleUsed.Contains(item.Name))
                        {
                            SelectedList.Clear();
                            throw new FriendlyException($"模组名称:{item.Name}被引用,无法删除!");
                        }
                        else
                        { 
                            deviceEngine.RemoveModuleNameGroup(item.Name);
                        }

                        ModuleDatas.Remove(item);
                    }
                    SelectedList.Clear();
                }
            });
        }

    }
}
