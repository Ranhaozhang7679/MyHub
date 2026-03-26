using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Luster.Common.DataStruct;

namespace Luster.Motion.SubSystem.ViewModel.Dialogs
{
    public class ModuleConfigureDialogVM : MotionDialogVM
    {
        private ICommonBus _commonBus;
        private IMotionController _mController;

        public ModuleConfigureDialogVM(ICommonBus commonBus, IMotionController mController)
        {
            _commonBus = commonBus;
            _mController = mController;
        }

        private ObservableCollection<ModuleConfigureModel> _moduleConfigures;
        public ObservableCollection<ModuleConfigureModel> ModuleConfigures
        {
            get => _moduleConfigures;
            set => SetProperty(ref _moduleConfigures, value);
        }




        /// <summary>
        /// 标题
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }


        private DelegateCommand<ModuleConfigureModel> _deleteCommand;
        public DelegateCommand<ModuleConfigureModel> DeleteCommand => _deleteCommand ?? (_deleteCommand = new DelegateCommand<ModuleConfigureModel>((o) =>
        {
            if (o == null) { return; }
            ModuleConfigures.Remove(o);
        }));

        // WPF项目，假设o绑定到CheckBox的IsChecked
        private DelegateCommand<ModuleConfigureModel> _checkCommand;
        public DelegateCommand<ModuleConfigureModel> CheckCommand => _checkCommand ?? (_checkCommand = new DelegateCommand<ModuleConfigureModel>((o) =>
        {
            if (o == null) return;

            // 记录当前状态
            bool oldValue = o.IsSelected;

            // 检查权限
            if (_commonBus.CurrentUser.UserRole != Common.DataStruct.SystemRole.Admin)
            {
                System.Windows.MessageBox.Show("当前用户权限不足！", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

                // 使用Dispatcher异步还原状态，确保UI已更新
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    o.IsSelected = oldValue;
                }));
            }
        }));

        /// <summary>
        /// 动态初始化可配置Dock页面
        /// </summary>
        private void InitAllDockContent()
        {
            ModuleConfigures = new ObservableCollection<ModuleConfigureModel>();
            Assembly assembly = GetType().Assembly;
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (typeof(IDockContent).IsAssignableFrom(type) && !type.IsInterface)
                {
                    var dContent = Activator.CreateInstance(type) as IDockContent;
                    ModuleConfigures.Add(new ModuleConfigureModel()
                    {
                        Name = Luster.Motion.Assests.Langs.LangProvider.GetLang(dContent.Name),
                        DockName= dContent.Name,
                        IsSelected = false,
                        ControlName = dContent.RegionName
                    });
                }
            }

            // 选中已经选择的Dock
            if (_mController.SysConfig != null && _mController.SysConfig.ListModule != null)
            {
                foreach (var item in ModuleConfigures)
                {
                    var modulemodel = _mController.SysConfig.ListModule.Find(u => u.DockName == item.DockName);
                    if (modulemodel != null)
                    {
                        item.IsSelected = modulemodel.IsSelected;
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("ModuleList", ModuleConfigures.ToList());
        }


        public override void OnDialogOpened(IDialogParameters parameters)
        {
            InitAllDockContent();
        }
    }

}
