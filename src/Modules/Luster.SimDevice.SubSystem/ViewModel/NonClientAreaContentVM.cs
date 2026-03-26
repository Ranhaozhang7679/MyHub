#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NonClientAreaContent
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       NonClientAreaContent.cs
* 创建时间:       2022/4/22 10:08:10
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      70571470-c7a5-4720-979e-c19bf6e910bd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/22 10:08:10
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets.Global;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.SubSystem.Config;
using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalCommands = Luster.SimDevice.SubSystem.Config.GlobalCommands;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class NonClientAreaContentVM : BaseVM
    {

        protected NonClientAreaContentVM(ISimDeviceEngineUI engineUI) : base(engineUI)
        {
        }

        protected override void ResisterGlobal()
        {
            GlobalCommands.LoadProjCommand.RegisterCommand(LoadProjCommand);
            GlobalCommands.SaveProjCommand.RegisterCommand(SaveProjCommand);
        }

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            // 模式切换
            engineUI.Subscribe<DeviceModeEvent, DeviceMode>((m) =>
            {
                ModeTitle = SimEngineUI.DeviceMode.GetDescription();
            });
        }

        /// <summary>
        /// 打开工程
        /// </summary>
        private DelegateCommand _loadProjCommand;
        public DelegateCommand LoadProjCommand => _loadProjCommand ?? (_loadProjCommand = new DelegateCommand(LoadProj));

        /// <summary>
        /// 保存项目
        /// </summary>
        private DelegateCommand _saveProjCommand;
        public DelegateCommand SaveProjCommand => _saveProjCommand ?? (_saveProjCommand = new DelegateCommand(SaveProj));

        /// <summary>
        /// 保存项目
        /// </summary>
        private DelegateCommand<string> _actionCommand;
        public DelegateCommand<string> ActionCommand => _actionCommand ?? (_actionCommand = new DelegateCommand<string>((strType) =>
        {
            if (strType == "Home")
            {
                GlobalProperty.IsEnabeld = false;
                Task.Run(() =>
                {
                    bool isOk = SimEngineUI.Engine.Home(out string errMsg);
                    if (!isOk)
                    {

                    }
                });
            }
            else if (strType == "Recovery")
            {
                GlobalProperty.IsEnabeld = true;
            }
        }));

        /// <summary>
        /// 保存项目
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void SaveProj()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "工程文件|*.dproj";
            if (saveFileDialog.ShowDialog() == true)
            {
                string saveFile = saveFileDialog.FileName;
                SimEngineUI.OnSaveProj(saveFile);
            }
        }

        /// <summary>
        /// 加载工程
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void LoadProj()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "工程文件|*.dproj";
            if (openFileDialog.ShowDialog() == true)
            {
                SimEngineUI.OnLoadProj(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// 模式标题
        /// </summary>
        private string _modeTitle = DeviceMode.Virtual.GetDescription();
        public string ModeTitle
        {
            get { return _modeTitle; }
            set { SetProperty(ref _modeTitle, value); }
        }

        /// <summary>
        /// 菜单切换功能
        /// </summary>
        private DelegateCommand<string> _changeModeCommand;
        public DelegateCommand<string> ChangeModeCommand => _changeModeCommand ?? (_changeModeCommand = new DelegateCommand<string>((str) =>
        {
            SimEngineUI.SetDeviceModeAsync((DeviceMode)Enum.Parse(typeof(DeviceMode), str));
        }));
    }
}