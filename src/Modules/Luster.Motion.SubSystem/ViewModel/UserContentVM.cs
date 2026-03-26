using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class UserContentVM : MotionVM
    {

        /// <summary>
        /// 用户列表
        /// </summary>
        private ObservableCollection<UserModel> _userList;
        public ObservableCollection<UserModel> UserList
        {
            get { return _userList; }
            set { SetProperty(ref _userList, value); }
        }

        /// <summary>
        /// 页面列表
        /// </summary>
        private ObservableCollection<FeatureModel> _pageItems;
        public ObservableCollection<FeatureModel> PageItems
        {
            get { return _pageItems; }
            set { SetProperty(ref _pageItems, value); }
        }

        /// <summary>
        /// 按钮列表
        /// </summary>
        private ObservableCollection<FeatureModel> _buttonItems;
        public ObservableCollection<FeatureModel> ButtonItems
        {
            get { return _buttonItems; }
            set { SetProperty(ref _buttonItems, value); }
        }

        // 用户操作信息列表
        private ObservableCollection<OperationTipItem> _oprationVms;
        public ObservableCollection<OperationTipItem> OprationVms
        {
            get => _oprationVms;
            set => SetProperty(ref _oprationVms, value);
        }

        private DelegateCommand _addOperationCommand;
        public DelegateCommand AddOperationCommand
        {
            get => _addOperationCommand;
            set => SetProperty(ref _addOperationCommand, value);
        }


        private DelegateCommand<OperationTipItem> _editOperationCommand;
        public DelegateCommand<OperationTipItem> EditOperationCommand
        {
            get => _editOperationCommand;
            set => SetProperty(ref _editOperationCommand, value);
        }


        private DelegateCommand<OperationTipItem> _removeOperationCommand;
        public DelegateCommand<OperationTipItem> RemoveOperationCommand
        {
            get => _removeOperationCommand;
            set => SetProperty(ref _removeOperationCommand, value);
        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            // 更新用户
            bus.GetEvent<UserLoginEvent>().Subscribe(u =>
            {
                AccessManagement();
            });
        }


        public UserModel SelectedUser { get; set; }

        public DelegateCommand<UserModel> UserSelectedCommand { get; set; }

        public DelegateCommand SaveFeatureCommand { get; set; }

        public DelegateCommand<UserModel> EditUserCommand { get; set; }

        public DelegateCommand<UserModel> RemoveUserCommand { get; set; }

        public DelegateCommand AddUserCommand { get; set; }

        private IDialogService _dialogService;
        private ICommonBus _commonBus;
        private IMotionController _mController;

        public UserContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController mController) : base(commonBus)
        {
            _commonBus = commonBus;
            _dialogService = dialogService;
            _mController = mController;
            UserList = new ObservableCollection<UserModel>();
            PageItems = new ObservableCollection<FeatureModel>();
            ButtonItems = new ObservableCollection<FeatureModel>();
            RemoveUserCommand = new DelegateCommand<UserModel>(RemoveUser);
            UserSelectedCommand = new DelegateCommand<UserModel>(UserSelected);
            AddUserCommand = new DelegateCommand(AddUser);
            EditUserCommand = new DelegateCommand<UserModel>(EditUser);
            EditOperationCommand = new DelegateCommand<OperationTipItem>(EditOperation);
            RemoveOperationCommand = new DelegateCommand<OperationTipItem>(RemoveOperation);
            AddOperationCommand = new DelegateCommand(AddOperation);
            GetUserList();
            AccessManagement();
            OprationVms = new ObservableCollection<OperationTipItem>();
            LoadOperationTips();
         
        }

        private void EditUser(UserModel user)
        {
            if (_commonBus.CurrentUser.UserRole == SystemRole.Admin)
            {
                DialogParameters param = new DialogParameters();
                param.Add("Title", Luster.Motion.Assests.Langs.LangProvider.GetLang("EditUser"));
                param.Add("User", SelectedUser);
                _dialogService.ShowDialog("AddUserDialog", param, (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var user = r.Parameters.GetValue<UserModel>("User");
                        var removeUser = _commonBus.UserConfig.Users.FirstOrDefault(x => x.Id == user.Id);
                        UserList.Remove(removeUser);
                        _commonBus.UserConfig.Users.Remove(removeUser);
                        _commonBus.UserConfig.Users.Insert(user.Id - 1, user);
                        UserList.Insert(user.Id - 1, user);
                        _commonBus.UserConfig.SaveUserConfig();
                        commonBus.EventBus.GetEvent<UserChangeEvent>().Publish(user);
                    }
                });
            }
            else
            {
                throw new FriendlyException("非管理员权限无法编辑");
            }
        }

        private void AddUser()
        {
            if (_commonBus.CurrentUser.UserRole == SystemRole.Admin)
            {
                DialogParameters param = new DialogParameters();
                param.Add("Title", Luster.Motion.Assests.Langs.LangProvider.GetLang("AddUser"));
                _dialogService.ShowDialog("AddUserDialog", param, (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var user = r.Parameters.GetValue<UserModel>("User");
                        user.Id = UserList.Count + 1;
                        user.IsSystem = false;
                        UserList.Add(user);
                        _commonBus.UserConfig.Users.Add(user);
                        _commonBus.UserConfig.SaveUserConfig();
                        commonBus.EventBus.GetEvent<UserChangeEvent>().Publish(user);
                    }
                });
            }
            else
            {
                throw new FriendlyException("非管理员权限无法增加");
            }
        }


        private void UserSelected(UserModel user)
        {
            if (user != null)
            {
                SelectedUser = user;
            }
        }

        private void RemoveUser(UserModel user)
        {
            if (_commonBus.CurrentUser.UserRole == SystemRole.Admin)
            {
                if (user == null) { return; }
                _dialogService.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ConfirmDeleteUser")}:{user.UserName}", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        _commonBus.UserConfig.Users.Remove(user);
                        UpdateUserIndex();
                        _commonBus.UserConfig.SaveUserConfig();
                        commonBus.EventBus.GetEvent<UserChangeEvent>().Publish(null);
                    }
                });
            }
            else
            {
                throw new FriendlyException("非管理员权限无法删除");
            }
        }

        private void UpdateUserIndex()
        {
            for (int i = 0; i < _commonBus.UserConfig.Users.Count; i++)
            {
                _commonBus.UserConfig.Users[i].Id = i + 1;
            }
            UserList = new ObservableCollection<UserModel>(_commonBus.UserConfig.Users);
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        private void GetUserList()
        {
            _commonBus.UserConfig.LoadUserConfig();
            UserList.AddRange(_commonBus.UserConfig.Users);
        }

        private void AccessManagement()
        {
            if (_commonBus.CurrentUser.UserRole == SystemRole.Admin)
            {
                for (int i = 0; i < _commonBus.UserConfig.Users.Count; i++)
                {
                    if (_commonBus.UserConfig.Users[i].UserRole == SystemRole.Admin)
                    {
                        _commonBus.UserConfig.Users[i].IsSystem = true;
                    }
                    else
                    {
                        _commonBus.UserConfig.Users[i].IsSystem = false;
                    }
                  
                }
                UserList = new ObservableCollection<UserModel>(_commonBus.UserConfig.Users);
            }
            else
            {
                for (int i = 0; i < _commonBus.UserConfig.Users.Count; i++)
                {
                    _commonBus.UserConfig.Users[i].IsSystem = true;
                }
                UserList = new ObservableCollection<UserModel>(_commonBus.UserConfig.Users);
            }
        }


        private void RemoveOperation(OperationTipItem item)
        {
            if (_commonBus.CurrentUser.UserRole == SystemRole.Admin)
            {
                if (item != null)
                {
                    _mController.SysConfig.OperationTips.Remove(item.Tag);
                    OprationVms.Remove(item);
                    commonBus.IsNeedSave = true;
                }
            }
            else
            {
                throw new FriendlyException("非管理员权限无法删除");
            }
        }

        private void EditOperation(OperationTipItem obj)
        {
            _dialogService.ShowOperationTip(obj.Tag, CallBack);
        }

        private void LoadOperationTips()
        {
            var tips = _mController.SysConfig.OperationTips;

            // 自动添加设备换料
            if (!tips.Any(u => u.Tip == SystemConsts.PlanDT))
            {
                tips.Add(new OperationTip()
                {
                    Operation = SystemOperation.Pause,
                    Tip = SystemConsts.PlanDT,
                });
            }

            // 自动添加午休
            if (!tips.Any(u => u.Tip == SystemConsts.NoonBreak))
            {
                tips.Add(new OperationTip()
                {
                    Operation = SystemOperation.Pause,
                    Tip = SystemConsts.NoonBreak,
                });
            }

            OprationVms = new ObservableCollection<OperationTipItem>();
            foreach (var tip in tips)
            {
                OprationVms.Add(new OperationTipItem(tip));
            }
        }

        /// <summary>
        /// 添加操作信息提示 
        /// </summary>
        private void AddOperation()
        {
            _dialogService.ShowOperationTip(null, CallBack);
        }

        private void CallBack(IDialogResult result)
        {
            if (result.Result == ButtonResult.OK)
            {
                if (result.Parameters.TryGetValue<OperationTip>("Model", out var tipModel))
                {
                    if (tipModel != null)
                    {
                        var item = OprationVms.FirstOrDefault(x => x.GUID == tipModel.GUID);
                        if (item != null)
                        {
                            item.Tip = tipModel.Tip;
                            item.Operation = tipModel.Operation.ToDescriptionOrString();
                        }
                        else
                        {
                            item = new OperationTipItem(tipModel);
                            OprationVms.Add(item);
                            _mController.SysConfig.OperationTips.Add(tipModel);
                        }
                        commonBus.IsNeedSave = true;
                    }
                }
            }

        }
    }
}
