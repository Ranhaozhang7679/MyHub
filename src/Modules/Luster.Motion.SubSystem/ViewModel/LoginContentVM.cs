using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class LoginContentVM : MotionPageVM, INavigationAware
    {
        public ICommonBus _commonBus;

        private IDeviceEngine _deviceEngine;

        private IDialogService _dialogService;

        private IMotionController _mController;

        protected LoginContentVM(ICommonBus commonBus, IDeviceEngine deviceEngine, IDialogService dialogService, IMotionController mController) : base(commonBus)
        {
            _dialogService = dialogService;
            _mController = mController;
            _deviceEngine = deviceEngine;
            ModeList = typeof(DeviceMode).EnumToDataSource();
            BindUser();
            BindProject();
            _commonBus = commonBus;
            //AutoLogin();//暂时自动登录，后面禁用
            RegisterMuliLang(nameof(ModeList));
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            // 更新用户
            bus.GetEvent<UserChangeEvent>().Subscribe(u =>
            {
                BindUser();
            });
        }

        /// <summary>
        /// 工程列表Change方法
        /// </summary>
        private void ProjectListChange()
        {
            if (commonBus.UserConfig != null)
            {
                ListProject = new List<ProjectInfo>(commonBus.ProjectList.ToArray());
            }
        }


        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }


        private bool _projectEnable = true;
        /// <summary>
        /// 选择工程是否可用
        /// </summary>
        public bool ProjectEnable
        {
            get { return _projectEnable; }
            set { SetProperty(ref _projectEnable, value); }
        }

        /// <summary>
        /// 用户集
        /// </summary>
        private List<UserModel> _listUser;
        public List<UserModel> ListUser
        {
            get { return _listUser; }
            set { SetProperty(ref _listUser, value); }
        }

        /// <summary>
        /// 工程信息
        /// </summary>
        private List<ProjectInfo> _listProject;
        public List<ProjectInfo> ListProject
        {
            get { return _listProject; }
            set { SetProperty(ref _listProject, value); }
        }

        /// <summary>
        /// 用户
        /// </summary>
        private UserModel _selectUser;
        public UserModel SelectUser
        {
            get { return _selectUser; }
            set
            {
                SetProperty(ref _selectUser, value);
                PWText = string.Empty;
            }
        }

        /// <summary>
        /// 选择工程
        /// </summary>
        private ProjectInfo _selectProject;
        public ProjectInfo SelectProject
        {
            get { return _selectProject; }
            set { SetProperty(ref _selectProject, value); }
        }

        /// <summary>
        /// 密码
        /// </summary>
        private string _pWText;
        public string PWText
        {
            get { return _pWText; }
            set { SetProperty(ref _pWText, value); }
        }

        /// <summary>
        /// 当前模式
        /// </summary>
        private DeviceMode _selectMode = DeviceMode.Virtual;
        public DeviceMode SelectMode
        {
            get { return _selectMode; }
            set
            {
                SetProperty(ref _selectMode, value);
            }
        }

        /// <summary>
        /// 模式列表
        /// </summary>
        private List<KeyValue> _modeList;
        public List<KeyValue> ModeList
        {
            get => _modeList; set
            {
                SetProperty(ref _modeList, value);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        private DelegateCommand _loginCommand;
        public DelegateCommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(() =>
        {
            Login();
        }));

        protected virtual void Login()
        {
            if (SelectUser != null)
            {
                if (SelectUser.Password != PWText &&!Debugger.IsAttached)
                {
                    //throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("PassWordError")}！");
                    _dialogService.ShowErrorPassword();
                }
                else
                {
                    commonBus.CurrentUser = SelectUser;
                    if (SelectProject != null && SelectProject.RecipeList != null)
                    {
                        _commonBus.ProjInfo = SelectProject;
                        var recipe = SelectProject.RecipeList.FirstOrDefault(u => u.IsActive);
                        if (recipe != null)
                        {
                            commonBus.OnActiveRecipe(recipe);
                        }
                        else
                        {
                            if (SelectProject.RecipeList.Count > 0)
                            {
                                commonBus.OnActiveRecipe(SelectProject.RecipeList[0]);
                            }
                        }
                        PWText = string.Empty;
                        _pWText = string.Empty;
                        // 记录当前的模式
                        SelectUser.CurrentMode = SelectMode;
                        UpdateMode();

                        // 开始登录
                        commonBus.OnUserLogin(SelectUser);


                    }
                    else
                    {
                        var proj = PageModel.Pages.FirstOrDefault(u => u.Name == "Project");
                        if (proj == null)
                        {
                            proj= PageModel.Pages.FirstOrDefault(u => u.Name == "Configure");
                        }
                        commonBus.OnNavigate(proj);
                        commonBus.OnUserLogin(SelectUser);
                    }
                }
            }
        }


        protected virtual void UpdateMode()
        {
            // 保存配置信息
            if (_deviceEngine.DeviceMode == DeviceMode.Empty)
            {
                _mController.SysConfig.RunMode = DeviceMode.Real;
            }
            else
            {
                _mController.SysConfig.RunMode = _deviceEngine.DeviceMode;
            }
        }

        /// <summary>
        /// 绑定用户
        /// </summary>
        private void BindUser()
        {
            if (commonBus.UserConfig != null)
            {
                commonBus.UserConfig.LoadUserConfig();
                ListUser = commonBus.UserConfig.Users;
                if (ListUser != null & ListUser.Count > 0)
                {
                    SelectUser = ListUser[0];
                }
            }
        }

        /// <summary>
        /// 获取绑定工程
        /// </summary>
        private void BindProject()
        {
            if (commonBus.ProjInfo != null)
            {
                ListProject = new List<ProjectInfo>(commonBus.ProjectList.ToArray());
                if (ListProject != null && ListProject.Count > 0)
                {
                    var projectInfo = ListProject.FirstOrDefault(u => u.IsActive == true);
                    if (projectInfo != null)
                    {
                        SelectProject = projectInfo;

                    }
                    else
                    {
                        SelectProject = ListProject[0];
                    }
                    ModeChangeByProj();
                }
            }
        }

        private void ModeChangeByProj()
        {
            if (SelectProject != null)
            {
                string sysConfig = Path.Combine(Path.GetDirectoryName(SelectProject.FullName), "Config", "SystemConfig.xml");
                _mController.SysConfig.LoadSysConfig(sysConfig);//加载配置
                if (_mController.SysConfig != null)
                {
                    SelectMode = _mController.SysConfig.RunMode;
                    commonBus.EventBus.GetEvent<DeviceModeChangeEvent>().Publish(SelectMode);
                }
            }
        }


        /// <summary>
        /// 切换模式
        /// </summary>
        private DelegateCommand<object> _changeModeCommand;
        public DelegateCommand<object> ChangeModeCommand => _changeModeCommand ?? (_changeModeCommand = new DelegateCommand<object>((arg) =>
        {
            var cArgs = arg as SelectionChangedEventArgs;

            if (cArgs.AddedItems.Count > 0)
            {
                var keyVal = cArgs.AddedItems[0] as KeyValue;
                DeviceMode mode = (DeviceMode)keyVal.Value;
                commonBus.EventBus.GetEvent<DeviceModeChangeEvent>().Publish(mode);
            }
        }));

        /// <summary>
        /// 切换工程
        /// </summary>
        private DelegateCommand<object> _changeProjCommand;
        public DelegateCommand<object> ChangeProjCommand => _changeProjCommand ?? (_changeProjCommand = new DelegateCommand<object>((arg) =>
        {
            var cArgs = arg as SelectionChangedEventArgs;
            if (cArgs.AddedItems.Count > 0)
            {
                SelectProject = cArgs.AddedItems[0] as ProjectInfo;
                commonBus.ProjInfo = SelectProject;
                commonBus.EventBus.GetEvent<ProjectNameEvent>().Publish(SelectProject.ProjName);
                ModeChangeByProj();
            }
        }));



        private void AutoLogin()
        {
#if DEBUG
            SelectUser = ListUser.LastOrDefault();
            if (SelectUser != null)
            {
                _pWText = SelectUser.Password;
            }
            //_selectMode = DeviceMode.Virtual;
#endif
        }

        #region 导航接口
        bool isFirst = true;

        public event Action<IDialogResult> RequestClose;

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("ProjectEnable"))
            {
                ProjectEnable = navigationContext.Parameters.GetValue<bool>("ProjectEnable");
            }

            if (isFirst)
            {
                SelectMode = _mController.SysConfig.RunMode;
                isFirst = false;
            }
            else
            {
                SelectMode = _mController.MotionEngine.DeviceEngine.DeviceMode;
            }
        }


        #endregion
    }
}
