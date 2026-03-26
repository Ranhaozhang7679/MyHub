#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AddUserDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.ViewModel.Dialogs
* 文 件 名:       AddUserDialogVM.cs
* 创建时间:       2022/8/5 10:11:51
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      43a54989-7d80-4fbc-9ca3-42b63b21afb9
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/5 10:11:51
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI.Models;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    internal class AddUserDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 角色列表
        /// </summary>
        private List<KeyValue> _userRoles;
        public List<KeyValue> UserRoles
        {
            get { return _userRoles; }
            set { SetProperty(ref _userRoles, value); }
        }


        private UserModel _user;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AddUserDialogVM()
        {
            IsEdit = false;
            UserRoles = typeof(SystemRole).EnumToDataSource();
            CurrentRole = UserRoles.LastOrDefault();
        }

        /// <summary>
        /// 班别
        /// </summary>
        private string _userName;
        [Required]
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        /// <summary>
        /// 原始密码
        /// </summary>
        private string _originalPassword;

        public string OriginalPassword
        {
            get { return _originalPassword; }
            set { SetProperty(ref _originalPassword, value); }
        }

        /// <summary>
        /// 原始密码
        /// </summary>
        private KeyValue _curRole ;
        public KeyValue CurrentRole
        {
            get { return _curRole; }
            set { SetProperty(ref _curRole, value); }
        }

        /// <summary>
        /// 编辑标识
        /// </summary>
        private bool _isEdit;
        public bool IsEdit
        {
            get { return _isEdit; }
            set { SetProperty(ref _isEdit, value); }
        }

        /// <summary>
        /// 密码
        /// </summary>
        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        /// <summary>
        /// BadgeID
        /// </summary>
        private string _badgeID;
        public string BadgeID
        {
            get { return _badgeID; }
            set { SetProperty(ref _badgeID, value); }
        }

        /// <summary>
        /// 确认密码
        /// </summary>
        private string _repeatePassword;
        public string RepeatePassword
        {
            get { return _repeatePassword; }
            set { SetProperty(ref _repeatePassword, value); }
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);

            if (_user != null)
            {
                if (_user.Password != OriginalPassword)
                {
                    throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("OriginalPassWordWrong")}");
                }
            }
            else
            {
                _user = new UserModel();
            }
            if (string.IsNullOrEmpty(Password))
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("PwdCannotBeEmpty")}");
            }

            if (string.IsNullOrEmpty(RepeatePassword))
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("DuplicatePwdCannotBeEmpty")}");
            }

            if (Password != RepeatePassword)
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("PwdInconsistent")}");
            }
            _user.UserName = UserName;
            _user.Password = Password;
            _user.UserRole = (SystemRole)CurrentRole.Value;
            //_user.BadgeID = BadgeID;

            result.Parameters.Add("User", _user);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;
            }
            if (parameters.TryGetValue<UserModel>("User", out var userModel))
            {
                _user = userModel;
                UserName = _user.UserName;
                //BadgeID = _user.BadgeID;
                IsEdit = true;
            }
            base.OnDialogOpened(parameters);
        }
    }
}
