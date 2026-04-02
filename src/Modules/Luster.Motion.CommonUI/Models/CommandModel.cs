#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommandModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       CommandModel.cs
* 创建时间:       2022/8/4 15:38:35
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      b59bc934-37e1-4db9-863e-265e36627a82
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/4 15:38:35
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using HandyControl.Expression.Shapes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Luster.Motion.DataStruct;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Luster.Motion.CommonUI.Models
{
    public class CommandModel : BindableBase
    {
        public SystemOperation Key { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool cmd_IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        private bool _isEnabled;
        public bool cmd_IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        private string _tips;
        public string Tips
        {
            get { return _tips; }
            set { SetProperty(ref _tips, value); }
        }
        /// <summary>
        /// 图片信息
        /// </summary>
        private string _iconfont;
        public string Iconfont
        {
            get { return _iconfont; }
            set { SetProperty(ref _iconfont, value); }
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        private bool _isVisible;
        public bool cmd_IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        private ImageSource _imagenormal;
        public ImageSource Imagenormal
        {
            get { return _imagenormal; }
            set { SetProperty(ref _imagenormal, value); }
        }

        private ImageSource _imageSelected;
        public ImageSource ImageSelected
        {
            get { return _imageSelected; }
            set { SetProperty(ref _imageSelected, value); }
        }

        private ImageSource _imageDisable;
        public ImageSource ImageDisable
        {
            get { return _imageDisable; }
            set { SetProperty(ref _imageDisable, value); }
        }


        /// <summary>
        /// 命令集合
        /// </summary>
        private static ObservableCollection<CommandModel> _commands;
        public static ObservableCollection<CommandModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = new ObservableCollection<CommandModel>()
                    {
                        new CommandModel() { Key=SystemOperation.Start,Name = "Start", Tips = "启动/恢复",  cmd_IsSelected = false,cmd_IsEnabled= false,Iconfont="\xe65f",cmd_IsVisible=false },
                        new CommandModel() { Key=SystemOperation.Recovery,Name = "Reset", Tips = "复位",  cmd_IsSelected = false,cmd_IsEnabled= false,Iconfont="\xe64d",cmd_IsVisible=false },
                        new CommandModel() { Key=SystemOperation.Pause,Name = "Pause", Tips = "暂停当前的动作",  cmd_IsSelected = false,cmd_IsEnabled= false,Iconfont="\xe640" ,cmd_IsVisible=false },
                        new CommandModel() { Key=SystemOperation.Stop,Name = "Stop", Tips = "入料站不在上料，现有料做完",  cmd_IsSelected = false,cmd_IsEnabled= false,Iconfont="\xe641",cmd_IsVisible=false  },
                        new CommandModel() { Key=SystemOperation.Home,Name = "HomeZero", Tips = "回零按钮", cmd_IsSelected = false,cmd_IsEnabled=true , Iconfont = "\xe642",cmd_IsVisible = false},
                    };
                }
                return _commands;
            }
            set
            {
                _commands = value;
            }
        }

        /// <summary>
        /// 选中项为互斥
        /// </summary>
        /// <param name="name"></param>
        private void SetSelected(SystemOperation key)
        {
            foreach (var item in CommandModel.Commands)
            {
                item.cmd_IsSelected = item.Key == key;
                //if (item.Key != key)
                //{
                //    if (key == SystemOperation.Recovery & item.Key == SystemOperation.Start)
                //    {
                //        item.IsSelected = true;
                //    }
                //    else
                //    {
                //        item.IsSelected = false;
                //    }
                //}
                //else
                //{
                //    item.IsSelected = true;
                //}
            }
        }

        private UserModel curentUser = null;

        /// <summary>
        /// 设置显示
        /// </summary>
        /// <param name="listuser"></param>
        public void SetUserPermission(UserModel user)
        {
            curentUser = user;
            SetVisible();
        }

        /// <summary>
        /// 启用
        /// </summary>
        /// <param name="isEnable"></param>
        public void SetEnabled(bool isEnable)
        {
            foreach (var item in Commands)
            {
                item.cmd_IsEnabled = isEnable;
            }
        }

        private void SetVisible()
        {
            foreach (var item in Commands)
            {
                item.cmd_IsVisible = item.Key != SystemOperation.Recovery;
            }

            if (curentUser != null)
            {
                foreach (var item in Commands)
                {
                    if (!item.cmd_IsVisible) continue;
                }
            }
        }

        private CommandModel GetCommand(SystemOperation key)
        {
            return Commands.FirstOrDefault(u => u.Key == key);
        }

        // 复位按钮可用
        public void EnableRecoveryButton()
        {
            GetCommand(SystemOperation.Recovery).cmd_IsEnabled = true;
            GetCommand(SystemOperation.Recovery).cmd_IsVisible = true;
        }

        //启动按钮取消选中
        public void ClearCmdButton()
        {
            GetCommand(SystemOperation.Start).cmd_IsSelected = false;
            GetCommand(SystemOperation.Pause).cmd_IsSelected = false;
            GetCommand(SystemOperation.Stop).cmd_IsSelected = false;
        }

        public void ChangeButton(StatusChanged sChagned)
        {
            SetVisible();

            // 取消所有选中
            SetSelected(SystemOperation.Contine);

            // 所有按钮禁用
            SetEnabled(false);

            if (sChagned.Dst == EngineStatus.Idle)
            {
                // 1.软件启动
                GetCommand(SystemOperation.Home).cmd_IsEnabled = true;
            }
            else if (sChagned.Dst == EngineStatus.Homing)
            {
                // 2.回零进行中
                GetCommand(SystemOperation.Stop).cmd_IsEnabled = true;
                SetSelected(SystemOperation.Home);
            }
            else if (sChagned.Dst == EngineStatus.Ready)
            {
                // 3.回零完成
                GetCommand(SystemOperation.Home).cmd_IsEnabled = true;
                GetCommand(SystemOperation.Start).cmd_IsEnabled = true;
                //GetCommand(SystemOperation.Stop).cmd_IsEnabled = true;
            }
            else if (sChagned.Dst == EngineStatus.Running)
            {
                // 4.自动运行
                GetCommand(SystemOperation.Pause).cmd_IsEnabled = true;
                GetCommand(SystemOperation.Stop).cmd_IsEnabled = true;
                SetSelected(SystemOperation.Start);
            }
            else if (sChagned.Dst == EngineStatus.Pause)
            {
                // 4.自动运行
                GetCommand(SystemOperation.Start).cmd_IsEnabled = true;
                GetCommand(SystemOperation.Stop).cmd_IsEnabled = true;
                SetSelected(SystemOperation.Pause);
            }
            else if (sChagned.Dst == EngineStatus.Alarm)
            {
                // 4.自动运行
                var cmd = GetCommand(SystemOperation.Recovery);
                if (cmd != null)
                {
                    cmd.cmd_IsEnabled = true;
                }

                GetCommand(SystemOperation.Recovery).cmd_IsEnabled = true;
                GetCommand(SystemOperation.Stop).cmd_IsEnabled = true;

                // 隐藏启动按钮
                GetCommand(SystemOperation.Start).cmd_IsVisible = false;
                GetCommand(SystemOperation.Recovery).cmd_IsVisible = true;
            }
            else if (sChagned.Dst == EngineStatus.Resetting)
            {
                // 4.自动运行
                GetCommand(SystemOperation.Stop).cmd_IsEnabled = true;
            }
            else if (sChagned.Dst == EngineStatus.Stop)
            {
                // 4.自动运行
                GetCommand(SystemOperation.Home).cmd_IsEnabled = true;
                SetSelected(SystemOperation.Stop);
            }
        }

        public void ChangeBtnForMotionCardInit(bool bIsEnable)
        {
            GetCommand(SystemOperation.Home).cmd_IsEnabled = bIsEnable;
        }
    }
}
