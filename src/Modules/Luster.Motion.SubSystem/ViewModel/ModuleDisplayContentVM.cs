#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleDisplayContentVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       ModuleDisplayContentVM.cs
* 创建时间:       2022/11/3 16:53:03
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      105797c9-8a51-4ddb-955d-c4247be646e5
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/3 16:53:03
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ModuleDisplayContentVM : MotionVM
    {
        private ICommonBus _commonBus;

        private IDialogService _dialogService;

        private IMotionController _mController;


        public ModuleDisplayContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController mController) : base(commonBus)
        {
            _commonBus = commonBus;
            _mController = mController;
            _dialogService = dialogService;
            BuildModule();

            if (_commonBus.ProjInfo != null && !string.IsNullOrEmpty(_commonBus.ProjInfo.ProjPath))
            {
                LayOutPath = Path.Combine(_commonBus.ProjInfo.ProjPath, "Config", "Layout.xml");
            }
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<LayOutSaveEvent>().Subscribe(() =>
            {
                SaveCommand.Execute();
            });

            bus.GetEvent<LangChangedDoneEvent>().Subscribe(() =>
            {
                // ModuleConfigureModel.ModuleConfigures.Clear();
                BuildModule();
                LanguageChangedEvent?.Invoke();
            });

        }


        /// <summary>
        /// 保存布局
        /// </summary>
        protected override void BindGlobalCommnads()
        {
            GlobalCommands.SaveCommand.RegisterCommand(SaveCommand);
        }


        /// <summary>
        /// 配置变化事件
        /// </summary>
        public event Action ModuleChangedEvent;

        /// <summary>
        /// 布局保存
        /// </summary>
        public event Action ModuleSaveEvent;


        /// <summary>
        /// 语言变化
        /// </summary>
        public event Action LanguageChangedEvent;

        private ObservableCollection<ModuleConfigureModel> _moduleConfigures;
        public ObservableCollection<ModuleConfigureModel> ModuleConfigures
        {
            get => _moduleConfigures;
            set => SetProperty(ref _moduleConfigures, value);
        }

        /// <summary>
        /// 模块显示
        /// </summary>
        private bool _moduleVisible;
        public bool ModuleVisible
        {
            get { return _moduleVisible; }
            set { SetProperty(ref _moduleVisible, value); }
        }


        /// <summary>
        /// 配置显示
        /// </summary>
        private bool _setVisible;
        public bool SetVisible
        {
            get { return _setVisible; }
            set { SetProperty(ref _setVisible, value); }
        }

        /// <summary>
        /// 布局路径
        /// </summary>
        private string _layoutPath;
        public string LayOutPath
        {
            get { return _layoutPath; }
            set { SetProperty(ref _layoutPath, value); }
        }

        /// <summary>
        /// 删除模块
        /// </summary>
        private string _deleteModule;
        public string DeleteModule
        {
            get { return _deleteModule; }
            set
            {
                SetProperty(ref _deleteModule, value);
                if (_mController.SysConfig != null && _mController.SysConfig.ListModule != null)
                {
                    var model = _mController.SysConfig.ListModule.FirstOrDefault(u => u.Name == value);
                    if (model != null)
                    {
                        var modelmodule = new ModuleConfigureModel()
                        {
                            Name = model.Name,
                            IsSelected = model.IsSelected,
                            ControlName = model.ControlName
                        };
                        var configuremodel = ModuleConfigures.FirstOrDefault(u => u.Name == value);
                        if (configuremodel != null)
                        {
                            ModuleConfigures.Remove(configuremodel);
                        }
                        _mController.SysConfig.ListModule.Remove(model);
                        JudgeVisible();
                    }
                }
            }
        }

        private void BuildModule()
        {
            ModuleConfigures = new ObservableCollection<ModuleConfigureModel>();

            if (_mController.SysConfig != null && _mController.SysConfig.ListModule != null)
            {
                foreach (var item in _mController.SysConfig.ListModule)
                {
                    if (item.IsSelected)
                    {
                        var model = new ModuleConfigureModel()
                        {

                            Name = Luster.Motion.Assests.Langs.LangProvider.GetLang(item.Name),
                            DockName = item.DockName,
                            IsSelected = item.IsSelected,
                            ControlName = item.ControlName
                        };
                        ModuleConfigures.Add(model);
                    }
                }
                AddLogContent();
            }
            JudgeVisible();
        }

        /// <summary>
        /// 模块设置
        /// </summary>
        private DelegateCommand _moduleSetCommand;
        public DelegateCommand ModuleSetCommand => _moduleSetCommand ?? (_moduleSetCommand = new DelegateCommand(() =>
        {
            _dialogService.ShowModuleSet((r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("ModuleList", out List<ModuleConfigureModel> lsit))
                    {
                        if (lsit != null)
                        {
                            ModuleConfigures.Clear();
                            var _moduleSelectlist = lsit.Where(m => m.IsSelected == true).ToList();
                            _moduleSelectlist.ForEach(x => ModuleConfigures.Add(x));
                            AddLogContent();
                            var listModule = new List<ModuleSetModel>();
                            foreach (var item in lsit)
                            {
                                var module = new ModuleSetModel()
                                {
                                    Name = item.Name,
                                    DockName = item.DockName,
                                    IsSelected = item.IsSelected,
                                    ControlName = item.ControlName
                                };
                                listModule.Add(module);
                            }
                            _mController.SysConfig.ListModule = listModule;
                            commonBus.IsNeedSave = true;
                            ModuleChangedEvent?.Invoke();
                            JudgeVisible();
                        }
                    }
                }
            });
        }));


        private void AddLogContent()
        {
            if (ModuleConfigures != null)
            {
                var selectmodel = ModuleConfigures.FirstOrDefault(u => u.Name == "Log");
                if (selectmodel == null)
                {
                    var model = new ModuleConfigureModel()
                    {
                        Name = Luster.Motion.Assests.Langs.LangProvider.GetLang("Log"),
                        IsSelected = true,
                        DockName = "Log",
                        ControlName = "LogContent"
                    };
                    ModuleConfigures.Add(model);
                }
            }
        }

        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(() =>
        {
            ModuleSaveEvent?.Invoke();
        }));


        private void JudgeVisible()
        {
            if (ModuleConfigures.Count > 0)
            {
                ModuleVisible = true;
                SetVisible = false;
            }
            else
            {
                ModuleVisible = false;
                SetVisible = true;
            }
        }

    }
}