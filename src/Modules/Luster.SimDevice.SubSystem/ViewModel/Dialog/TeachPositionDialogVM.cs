#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TeachPositionDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       TeachPositionDialogVM.cs
* 创建时间:       2022/12/15 16:57:30
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      8458c4e3-4ded-40c0-8cde-e5d011499b9c
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/15 16:57:30
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class TeachPositionDialogVM : DialogVM
    {
        private string _name;
        /// <summary>
        /// 点位名称
        /// </summary>
        [Required]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private double _position;
        /// <summary>
        /// 点位位置
        /// </summary>
        [Required]
        public double Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        /// <summary>
        /// 是否是组
        /// </summary>
        private bool _isGroup = false;
        public bool IsGroup
        {
            get { return _isGroup; }
            set { SetProperty(ref _isGroup, value); }
        }

        protected TeachPositionDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {

        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<bool>("IsGroup", out var group))
            {
                IsGroup = group;
            }

            if (!IsGroup)
            {
                if (parameters.TryGetValue<double>("Position", out var position))
                {
                    Position = position;
                }

                if (parameters.TryGetValue<string>("AxisName", out var name))
                {
                    Name = $"{name}_";
                }
            }
        }

        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add(nameof(Name), Name);
            result.Parameters.Add(nameof(Position), Position);
        }

    }
}
