#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SwitchModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       SwitchModel.cs
* 创建时间:       2022/8/2 13:21:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      be8220ef-eb9b-4a40-9d88-1bfff86cd084
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/2 13:21:29
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Motion;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Models
{
    public class SwitchModel : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                SetProperty(ref _name, value);
            }
        }

        /// <summary>
        /// 是否当前
        /// </summary>
        private bool _isCurrent;
        public bool IsCurrent
        {
            get { return _isCurrent; }
            set { SetProperty(ref _isCurrent, value); }
        }

        private IMotionModule _value;
        public IMotionModule Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public SwitchModel(IMotionModule motion)
        {
            Name = motion.Alias;
            IsCurrent = motion.IsCurrent;
            Value = motion;
        }
    }
}